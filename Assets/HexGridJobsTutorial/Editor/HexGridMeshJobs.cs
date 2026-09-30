using Unity.Collections;
using Unity.Jobs;
using Unity.Profiling;

using UnityEngine;
using UnityEngine.UIElements;

// Unity 6. Call DrawGrid only from a generateVisualContent callback.
// The original uint[column][row] array remains the authoritative editable data.
public static class HexGridMeshJobs
{
	public const float ColumnStepFactor = 0.75f;
	public const float HeightFactor = 0.8660254f;
	public const float DrawingPadding = 2f;
	public const float StrokeWidth = 2f;

	// Outlined cells use 12 vertices: 4096 * 12 = 49152, below 65535.
	private const int CellsPerMesh = /*4096*/5461;

	private static readonly ProfilerMarker PrepareMarker = new ProfilerMarker("HexGrid.Prepare");
	private static readonly ProfilerMarker BuildMarker = new ProfilerMarker("HexGrid.BuildChunk");

	public static void DrawGrid(MeshGenerationContext context, uint[][] grid, Vector2Int gridSize, float cellSize, Vector2 gridPosition, Rect viewport, bool useJobs = true, bool cullOffscreen = true)
	{
		if (gridSize.x <= 0 || gridSize.y <= 0 || cellSize <= 0f || float.IsNaN(cellSize) || float.IsInfinity(cellSize) || !(viewport.width > 0f) || !(viewport.height > 0f))
			return;

		using (PrepareMarker.Auto())
		{
			// Origin includes the same fixed drawing offset used by your picker.
			Vector2 origin = gridPosition + Vector2.one * DrawingPadding;
			GetVisibleRange(gridSize, cellSize, origin, viewport, cullOffscreen, out int firstColumn, out int lastColumn, out int firstRow, out int lastRow);

			if (firstColumn > lastColumn || firstRow > lastRow) return;

			int visibleColumns = lastColumn - firstColumn + 1;
			int visibleRows = lastRow - firstRow + 1;
			int visibleCount = checked(visibleColumns * visibleRows);

			// One immutable native snapshot per grid repaint, not one allocation per cell.
			// A job cannot contain the managed uint[][] array or a VisualElement reference.
			var snapshot = new NativeArray<uint>(visibleCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);

			JobHandle allJobs = default;
			try
			{
				int destination = 0;
				for (int column = firstColumn; column <= lastColumn; column++)
				{
					uint[] source = grid != null && column < grid.Length ? grid[column] : null;
					for (int row = firstRow; row <= lastRow; row++)
					{
						// Uninitialized/missing data is displayed as an empty cell.
						snapshot[destination++] = source != null && row < source.Length ? source[row] : 0u;
					}
				}

				context.GetTempMeshAllocator(out TempMeshAllocator allocator);

				int i = 0;
				for (int start = 0; start < visibleCount; start += CellsPerMesh)
				{
					i++;
					var job = new BuildChunkJob
					{
						cells = snapshot,
						allocator = allocator,
						start = start,
						count = Mathf.Min(CellsPerMesh, visibleCount - start),
						firstColumn = firstColumn,
						firstRow = firstRow,
						visibleRows = visibleRows,
						cellSize = cellSize,
						origin = origin,
						fillColor = Color.white,
						outlineColor = Color.white
					};

					// A separate node per job: completion order cannot change draw order.
					context.InsertMeshGenerationNode(out job.node);

					if (useJobs)
					{
						// Jobs share only read-only input; they can run concurrently.
						JobHandle handle = job.Schedule();
						allJobs = JobHandle.CombineDependencies(allJobs, handle);
					}
					else
					{
						// Teaching/profiling mode: identical batched geometry on the main thread.
						job.Execute();
					}
				}
				Debug.Log(i);
			}
			finally
			{
				if (useJobs)
				{
					// Dispose the input only after every reader finishes. This handle includes
					// mesh generation AND cleanup. Unity waits before consuming the meshes.
					JobHandle cleanup = snapshot.Dispose(allJobs);
					context.AddMeshGenerationJob(cleanup);
				}
				else
				{
					snapshot.Dispose();
				}
			}
		}
	}

	private static void GetVisibleRange(Vector2Int size, float cellSize, Vector2 origin, Rect view, bool cull, out int firstColumn, out int lastColumn, out int firstRow, out int lastRow)
	{
		firstColumn = 0;
		firstRow = 0;
		lastColumn = size.x - 1;
		lastRow = size.y - 1;
		if (!cull) return;

		float stepX = ColumnStepFactor * cellSize;
		float stepY = HeightFactor * cellSize;
		// A centered outline extends beyond the nominal cell. Use a conservative margin.
		float margin = StrokeWidth * 0.5f / HeightFactor;

		firstColumn = Mathf.Max(0, Mathf.CeilToInt((view.xMin - origin.x - cellSize - margin) / stepX));
		lastColumn = Mathf.Min(size.x - 1, Mathf.FloorToInt((view.xMax - origin.x + margin) / stepX));

		// Odd columns are displaced down by half a row. Include that extra half-height.
		firstRow = Mathf.Max(0, Mathf.CeilToInt((view.yMin - origin.y - 1.5f * stepY - margin) / stepY));
		lastRow = Mathf.Min(size.y - 1, Mathf.FloorToInt((view.yMax - origin.y + margin) / stepY));
	}

	// IJob runs once for a batch. Several batches can be scheduled concurrently.
	// This baseline uses the Job System without requiring Burst.
	private struct BuildChunkJob : IJob
	{
		[ReadOnly] public NativeArray<uint> cells;
		[ReadOnly] public TempMeshAllocator allocator;
		public MeshGenerationNode node;
		public int start;
		public int count;
		public int firstColumn;
		public int firstRow;
		public int visibleRows;
		public float cellSize;
		public Vector2 origin;
		public Color32 fillColor;
		public Color32 outlineColor;

		public void Execute()
		{
			using (BuildMarker.Auto())
			{
				// Pass 1: each cell contributes one of two different mesh sizes.
				int vertexCount = 0;
				int indexCount = 0;
				int end = start + count;
				for (int i = start; i < end; i++)
				{
					bool filled = cells[i] == 1u;
					vertexCount += filled ? 7 : 12;
					indexCount += filled ? 18 : 36;
				}

				allocator.AllocateTempMesh(vertexCount, indexCount, out NativeSlice<Vertex> vertices, out NativeSlice<ushort> indices);

				float radius = cellSize * 0.5f;
				float apothem = radius * HeightFactor;
				float outerApothem = apothem + StrokeWidth * 0.5f;
				float innerApothem = Mathf.Max(0f, apothem - StrokeWidth * 0.5f);
				float outerRadius = outerApothem / HeightFactor;
				float innerRadius = innerApothem / HeightFactor;

				int vertexOffset = 0;
				int indexOffset = 0;

				// Pass 2: write every vertex/index directly into the native slices.
				for (int i = start; i < end; i++)
				{
					// Snapshot is column-major, matching grid[column][row].
					int column = firstColumn + i / visibleRows;
					int row = firstRow + i % visibleRows;
					float oddOffset = (column & 1) != 0 ? 0.5f : 0f;
					Vector2 center = origin + new Vector2(
						column * ColumnStepFactor * cellSize + radius,
						(row + oddOffset) * HeightFactor * cellSize + apothem);

					if (cells[i] == 1u)
					{
						WriteFilledHex(vertices, indices, center, radius, apothem, vertexOffset, indexOffset, fillColor);
						vertexOffset += 7;
						indexOffset += 18;
					}
					else
					{
						WriteOutlineHex(vertices, indices, center, outerRadius, outerApothem, innerRadius, innerApothem, vertexOffset, indexOffset, outlineColor);
						vertexOffset += 12;
						indexOffset += 36;
					}
				}

				// UI Toolkit owns the output slices. Do not dispose or cache them.
				node.DrawMesh(vertices, indices);
			}
		}

		private static void WriteFilledHex(NativeSlice<Vertex> vertices, NativeSlice<ushort> indices, Vector2 center, float radius, float apothem, int vertexOffset, int indexOffset, Color32 color)
		{
			vertices[vertexOffset] = MakeVertex(center, color);
			for (int corner = 0; corner < 6; corner++)
			{
				vertices[vertexOffset + 1 + corner] = MakeVertex(center + Corner(corner, radius, apothem), color);

				int next = (corner + 1) % 6;
				int triangle = indexOffset + corner * 3;
				indices[triangle] = (ushort)vertexOffset;
				indices[triangle + 1] = (ushort)(vertexOffset + 1 + corner);
				indices[triangle + 2] = (ushort)(vertexOffset + 1 + next);
			}
		}

		private static void WriteOutlineHex(NativeSlice<Vertex> vertices, NativeSlice<ushort> indices, Vector2 center, float outerRadius, float outerApothem, float innerRadius, float innerApothem, int vertexOffset, int indexOffset, Color32 color)
		{
			// Six outer + six inner corners form six quads around an empty center.
			for (int corner = 0; corner < 6; corner++)
			{
				vertices[vertexOffset + corner] = MakeVertex(center + Corner(corner, outerRadius, outerApothem), color);
				vertices[vertexOffset + 6 + corner] = MakeVertex(center + Corner(corner, innerRadius, innerApothem), color);

				int next = (corner + 1) % 6;
				int offset = indexOffset + corner * 6;
				int outer = vertexOffset + corner;
				int outerNext = vertexOffset + next;
				int inner = vertexOffset + 6 + corner;
				int innerNext = vertexOffset + 6 + next;

				indices[offset] = (ushort)outer;
				indices[offset + 1] = (ushort)outerNext;
				indices[offset + 2] = (ushort)innerNext;
				indices[offset + 3] = (ushort)outer;
				indices[offset + 4] = (ushort)innerNext;
				indices[offset + 5] = (ushort)inner;
			}
		}

		private static Vertex MakeVertex(Vector2 point, Color32 color)
		{
			return new Vertex
			{
				position = new Vector3(point.x, point.y, Vertex.nearZ),
				tint = color
			};
		}

		// Clockwise in UI coordinates (positive Y is down), matching UI Toolkit triangles.
		private static Vector2 Corner(int corner, float radius, float apothem)
		{
			switch (corner)
			{
				case 0: return new Vector2(-radius * 0.5f, -apothem);
				case 1: return new Vector2(radius * 0.5f, -apothem);
				case 2: return new Vector2(radius, 0f);
				case 3: return new Vector2(radius * 0.5f, apothem);
				case 4: return new Vector2(-radius * 0.5f, apothem);
				default: return new Vector2(-radius, 0f);
			}
		}
	}
}
