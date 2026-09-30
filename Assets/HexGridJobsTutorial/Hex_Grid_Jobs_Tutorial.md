**Making your hexagonal grid responsive with UI Toolkit mesh jobs**

This tutorial builds on the CellGridCanvas you uploaded: flat-top hexagons, odd columns displaced downwards, `uint[column][row]` data, left-click painting, right-click erasing, middle-button panning, and zoom around the mouse. It targets Unity 6's public mesh-generation APIs.

The goal is to reduce the work done when editing and move geometry generation to worker jobs. Performance must be measured in your Unity Editor; this example has not been compiled or profiled in Unity in the environment where it was prepared. The geometry, culling equations, batch limits and zoom equations were checked independently.

The accompanying files are:

| File | Purpose |
| --- | --- |
| `HexGridMeshJobs.cs` | The renderer to integrate into your existing canvas. Read this first. |
| `CellGridCanvasJobs.cs` | A complete example of the canvas, including input and independent mouse-marker drawing. Its different class name lets it coexist with your original. |
| `Editor/HexGridJobsDemoWindow.cs` | A 100 x 100 Editor demo, with Jobs and culling switches for comparison. |

1. **Understand where the existing code spends time.**

   A 100 x 100 grid contains 10,000 cells. Your old `DrawGrid` visits every cell every time the grid's visual content is regenerated. An empty cell creates a six-point managed array and calls `Painter2D.Stroke`; a filled cell creates vertex and index arrays and calls `context.Allocate`. During painting, the original pointer handler calls `MarkDirtyRepaint` even when the cell's value is already the requested value.

   These are observable costs in the source, rather than a claim that we have measured your specific CPU bottleneck. Capture a baseline in the Unity Profiler before replacing the renderer.

   The reference makes five changes that you can measure separately:

   | Change | Work reduced |
   | --- | --- |
   | Skip unchanged paint values | Avoids requesting a grid rebuild when the data stayed the same. |
   | Separate the mouse marker | Hovering redraws a small overlay instead of the whole grid. |
   | Cull offscreen cells | Mesh generation visits the visible rectangle plus a small conservative margin. |
   | Batch cells into meshes | Eliminates per-cell managed arrays and per-cell mesh submissions or strokes. |
   | Build batches in jobs | Moves geometry construction away from the main thread; independent batches can overlap. |

   Inserting a mesh-generation node by itself does not reduce the amount of geometry, allocate it, or schedule a job. It provides the place where that job's mesh commands will appear. See Unity's [node API](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/UIElements.MeshGenerationContext.InsertMeshGenerationNode.html).

2. **Try the complete example, then change one thing at a time.**

   Copy the three C# files into an `Assets/HexGridJobsTutorial` folder, keeping the demo file in its `Editor` subfolder. Let Unity compile, then open **Window > Hex Grid Jobs Tutorial**. The scripts import `Unity.Collections`, `Unity.Jobs`, `Unity.Profiling`, and `UnityEngine.UIElements`. This example uses ordinary `IJob` jobs; it does not require you to add Burst attributes.

   Left-click paints, right-click erases, middle-drag pans, and the wheel zooms. The two toggles let you compare batched rendering on the main thread against jobs, and full-grid generation against visible-cell generation. Turning Jobs off uses the same geometry-building code synchronously, so the comparison isolates scheduling from batching. The window is an in-memory demo; closing/rebuilding it can reset its sample data.

   To integrate just the renderer into your existing class, add `HexGridMeshJobs.cs`, then replace the body of your existing `DrawGrid` with:

   ```csharp
   private void DrawGrid(MeshGenerationContext context)
   {
       HexGridMeshJobs.DrawGrid(
           context, grid, gridSize, cellSize, gridPosition, contentRect);
   }
   ```

   Add `style.overflow = Overflow.Hidden;` in your canvas constructor. Keep your existing `generateVisualContent += OnGenerateVisualContent` registration. Remove the old calls to `AllocateHexagon` and `DrawHexagonOutline`; the new renderer draws both cell states. Those old methods can then be removed once unused.

   The renderer keeps your fixed two-unit drawing padding. Keep that same padding in the picker and mouse-anchored zoom calculation. The uploaded file's zoom division still used the new `cellSize`; apply the earlier correction to divide by `previousCellSize`. The complete example already includes that correction.

3. **Keep editable data on the main thread and give jobs a snapshot.**

   Your `uint[][] grid` is a managed jagged array. Keep it as your editable/saveable model. The renderer copies the visible portion into a single `NativeArray<uint>` before scheduling jobs:

   ```csharp
   var snapshot = new NativeArray<uint>(
       visibleCount, Allocator.TempJob,
       NativeArrayOptions.UninitializedMemory);
   ```

   Every entry is filled before scheduling. Missing/null source columns are drawn as empty cells, so displaying an initially unallocated grid does not mutate your model inside the drawing callback.

   The snapshot is column-major, matching your data layout. Its index mapping is:

   ```csharp
   int index = (column - firstColumn) * visibleRows + (row - firstRow);

   int columnFromIndex = firstColumn + index / visibleRows;
   int rowFromIndex = firstRow + index % visibleRows;
   ```

   The jobs receive `[ReadOnly] NativeArray<uint> cells` and copies of the zoom, origin and colors. They never read the `VisualElement`, pointer events, or the original jagged array. If painting changes the managed model, an already scheduled job still has its own consistent snapshot.

   `Allocator.TempJob` is used because this input crosses a job boundary. Treat this as one native input allocation per grid rebuild, not as an allocation-free renderer. See Unity's [NativeArray API](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Unity.Collections.NativeArray_1.html) and [allocator API](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Unity.Collections.Allocator.html).

4. **Represent both cell states with triangles.**

   The new renderer uses these two shapes:

   | Cell state | Vertices | Indices | Construction |
   | --- | ---: | ---: | --- |
   | Filled (`1`) | 7 | 18 | One center, six corners, six triangles. |
   | Empty/outlined | 12 | 36 | Six outer corners, six inner corners, six quads around an empty center. |

   Read `WriteFilledHex` and `WriteOutlineHex` in the renderer. Both write directly into `NativeSlice` buffers. `new Vertex` and `new Vector2` there create value types; they do not create a managed array for each cell.

   An outline is a narrow hexagonal ring. The apothem is the distance from the center to a side. To make a centered stroke of width two, the outer apothem is the nominal apothem plus one; the inner apothem is the nominal apothem minus one. The corresponding radius is `apothem / HeightFactor`.

   The ring has transparent space in the middle, rather than covering the center with a background-colored polygon. Neighboring outlines share the same edge positions. These simple mesh edges may look sharper or more aliased than Painter2D strokes; the reference does not reproduce Painter2D's full antialiasing behavior.

5. **Split the visible cells into meshes with safe index ranges.**

   This API allows at most 65,535 vertices per temporary mesh. Indices are `ushort`. A fully filled 100 x 100 grid would already need 70,000 vertices, so one mesh would exceed the limit. The outlined version needs even more vertices. See the [temporary mesh allocation limits](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/UIElements.TempMeshAllocator.AllocateTempMesh.html).

   The example chooses 4,096 cells per batch:

   ```csharp
   private const int CellsPerMesh = 4096;
   // Worst case: 4096 outlined cells * 12 vertices = 49152 vertices.
   ```

   Every mesh starts its vertex offsets at zero. Ten thousand visible cells therefore produce three batch submissions. That is an application-level mesh count, not a promise of exactly three GPU draw calls; Unity's renderer handles the final batching.

   The index count can exceed 65,535. The restriction is on the vertices referenced by the 16-bit index values, not the length of the index buffer. The largest outlined batch has 147,456 indices, all referencing vertices below 49,152.

6. **Use the node and allocator to move the batch construction into a job.**

   `BuildChunkJob` is a struct implementing `IJob`. Unity calls `Execute` once for one batch. Its input fields include:

   ```csharp
   [ReadOnly] public NativeArray<uint> cells;
   [ReadOnly] public TempMeshAllocator allocator;
   public MeshGenerationNode node;
   ```

   In `Execute`, the first pass counts the exact numbers of vertices and indices from the cells' states. The job then obtains the output buffers:

   ```csharp
   allocator.AllocateTempMesh(vertexCount, indexCount,
       out NativeSlice<Vertex> vertices,
       out NativeSlice<ushort> indices);
   ```

   A second pass fills the buffers. For each cell, `vertexOffset` and `indexOffset` identify where that cell's geometry starts inside the batch. Triangle indices include the vertex offset. After the entire batch is written:

   ```csharp
   node.DrawMesh(vertices, indices);
   ```

   This records the batch's mesh at the node's reserved position. `Execute` uses the allocator and node provided to it; calls on the original `MeshGenerationContext` remain on the main thread. The complete writer functions are in `HexGridMeshJobs.cs` so you can inspect every generated index.

   Unity describes the separation between main-thread and job allocation in its [parallel tessellation guide](https://docs.unity3d.com/6000.0/Documentation/Manual/UIE-parallel-tessellation.html). With fixed, cheaply known output counts, main-thread allocation is also a valid approach. Here we use the requested node path and count mixed filled/outlined cells inside the job.

7. **Schedule the work and give UI Toolkit the dependency.**

   During `generateVisualContent`, get the temporary allocator once. For each batch, create a job with the appropriate snapshot range and reserve a distinct node:

   ```csharp
   context.GetTempMeshAllocator(out TempMeshAllocator allocator);
   // Configure a BuildChunkJob for each batch using that allocator.
   context.InsertMeshGenerationNode(out job.node);

   JobHandle handle = job.Schedule();
   allJobs = JobHandle.CombineDependencies(allJobs, handle);
   ```

   The complete loop also sets `start`, `count`, visible-range coordinates, cell size, origin, and colors. Because each batch gets its own node and output buffers, there is no shared output being written by different jobs. All batches read the same immutable input snapshot. The order of node insertion preserves cell drawing order even if jobs finish in a different order.

   After scheduling all batches, register the dependency with the context. The renderer's cleanup code is:

   ```csharp
   JobHandle cleanup = snapshot.Dispose(allJobs);
   context.AddMeshGenerationJob(cleanup);
   ```

   `Dispose(allJobs)` schedules a cleanup job after all readers finish. That handle includes both generation and cleanup. UI Toolkit can safely consume the completed output once it finishes. The `finally` block ensures input cleanup is also arranged if preparation fails after the native array was allocated.

   Keep these ownership rules distinct:

   | Resource | Owner | Lifetime |
   | --- | --- | --- |
   | Managed `grid` | Your canvas/model | Until you replace or release your model. |
   | Native input `snapshot` | Your renderer | Disposed after all batch jobs finish. |
   | Temporary output vertices/indices | UI Toolkit | Current mesh-generation phase. |
   | Node and temporary allocator | UI Toolkit | Current mesh-generation phase. |

   Do not retain the temporary mesh slices, nodes, or allocator for another repaint. Your persistent data is the grid. The renderer can regenerate UI geometry from it when requested. See [job registration](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/UIElements.MeshGenerationContext.AddMeshGenerationJob.html), [dependent disposal](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Unity.Collections.NativeArray_1.Dispose.html), and [temporary allocator scope](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/UIElements.TempMeshAllocator.AllocateTempMesh.html).

8. **Limit generation to the visible cells.**

   Clipping keeps drawing inside the element's rectangle, but the CPU still pays for geometry you generated outside it. `GetVisibleRange` converts the visible rectangle into a conservative range of grid columns and rows before taking the snapshot.

   The horizontal step between columns is `0.75f * cellSize`; the vertical step between rows is `0.8660254f * cellSize`. Subtract the panned grid origin, divide by the step, and round outward. Include the cell's full size when testing the near edge. Odd columns need an extra half-row allowance, and centered outlines need a small margin.

   The result intentionally includes some partially visible or adjacent cells. That avoids popping at the edge. `Overflow.Hidden` performs the final exact clipping.

   For example, with a 1,000 x 700 viewport, a 100-unit cell size and the initial origin, the formulas select 14 columns by 9 rows: 126 candidate cells instead of 10,000. This is a calculated example, not a measured frame-time improvement. When zoomed far enough out to show the whole grid, all 10,000 cells must still be generated.

9. **Request a repaint only for changes that affect that element.**

   Change your paint function to return a bool. Once it has found a valid cell, finish with:

   ```csharp
   uint value = button == 0 ? 1u : 0u;
   if (grid[column][row] == value)
       return false;

   grid[column][row] = value;
   return true;
   ```

   The surrounding code must call this only for the paint/erase buttons, return `false` on invalid cells, and use `if (EditGridCells()) MarkDirtyRepaint();`. The complete example implements those guards. Panning, zooming, resizing, loading data and changes made by other code still require a grid repaint.

   For a mouse-following crosshair or brush rectangle, add an absolutely positioned child `VisualElement` with `PickingMode.Ignore`. Give that child its own `generateVisualContent` callback and call its `MarkDirtyRepaint` when only the mouse position changes. The grid callback then draws only the grid.

   UI Toolkit retains an element's generated visual content until it needs regeneration. This lets the mouse overlay move independently. See [generateVisualContent and caching](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/UIElements.VisualElement-generateVisualContent.html). Your complete reference includes pointer capture, release, clipping and the existing nearest-cell picking formula.

10. **Measure each improvement and decide what to keep.**

    Use the Profiler's Editor target for this EditorWindow. Start without Deep Profile. Compare the same viewport, zoom, fill pattern and interaction after the first few warm-up repaints.

    | Comparison | What it isolates |
    | --- | --- |
    | Original code vs demo with Jobs off and culling off | Primarily batching, array reduction and repaint changes. |
    | Jobs off; culling off vs on | The benefit of skipping offscreen geometry. |
    | Same culling setting; Jobs off vs on | The effect of job scheduling and moving batch generation off the main thread. |

    The code emits `HexGrid.Prepare` and `HexGrid.BuildChunk` profiler markers. Look at main-thread time, worker execution, time waiting for jobs, total frame time and managed GC allocation. In Jobs mode, expect the build marker on job workers; in synchronous mode it runs on the main thread.

    Try hovering, holding the paint button inside an already painted cell, crossing multiple cells, panning, and zooming. Hovering should not request new grid geometry in the reference canvas. Test both a close view and a view of the entire 100 x 100 grid.

    Jobs have overhead. With only a small visible batch, synchronous batched rendering may be faster. Changing the batch size trades scheduling overhead against concurrency, as long as every mesh remains within the vertex limit. Use the measurements to decide; there is no guaranteed FPS improvement from enabling jobs alone.

    If changing one cell still rebuilds too much, a subsequent step is to partition the grid into multiple chunk VisualElements, keep their existing visual content, and mark only the affected chunks dirty. The reference here has one grid element and rebuilds its visible batches on a data change. It does not implement persistent per-chunk caching or continuous brush-stroke interpolation.

**Validation performed for this reference**

Independent mathematical checks covered filled/outline triangle winding and areas at cell sizes 10, 20, 100 and 300; ring holes; vertex/index bounds around the 4,096-cell batch boundary and at 10,000 cells; 193,318 cell-bound intersections across 300 randomized rectangular grids/views; and 1,000 mouse-anchored zoom calculations. These checks validate the equations and topology, not compilation, Unity job safety at runtime, visual antialiasing, or Editor performance. Compile and profile the demo in your Unity project before adopting it.
