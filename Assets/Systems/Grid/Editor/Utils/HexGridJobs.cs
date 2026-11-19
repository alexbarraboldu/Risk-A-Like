using ABB.Grid.Editor.Data;

using Unity.Collections;
using Unity.Jobs;
using Unity.Profiling;

using UnityEngine;
using UnityEngine.UIElements;

public class HexGridJobs
{
	public const float ColumnStepFactor = 0.75f;
	public const float HeightFactor = 0.8660254f;
	public const float DrawingPadding = 2f;
	public const float StrokeWidth = 2f;

	private const int CornerCount = 6;
	private const int FilledVertexCount = CornerCount + 1;
	private const int OutlinedVertexCount = CornerCount * 2;
	private const int CellsPerMesh = ushort.MaxValue / OutlinedVertexCount;

	private static readonly ProfilerMarker PrepareMarker = new ProfilerMarker("HexGrid.Prepare");
	private static readonly ProfilerMarker BuildMarker = new ProfilerMarker("HexGrid.BuildChunk");

	// X is in units of radius; Y is in units of apothem.
	// The outlined hex uses the same six corners for both rings.
	private static readonly Vertex[] HexVertexTemplate =
	{
		TemplateVertex(-0.5f, -1f),
		TemplateVertex(-1f,    0f),
		TemplateVertex(-0.5f,  1f),
		TemplateVertex( 0.5f,  1f),
		TemplateVertex( 1f,    0f),
		TemplateVertex( 0.5f, -1f),
		TemplateVertex( 0f,    0f) // Filled hex center, vertex 6.
	};

	private static readonly ushort[] FilledTriangleTemplate =
	{
		0, 5, 6,
		5, 4, 6,
		4, 3, 6,
		3, 2, 6,
		2, 1, 6,
		1, 0, 6
	};

	private static readonly ushort[] OutlinedTriangleTemplate =
	{
		0, 5, 6, 5, 11, 6,
		5, 4, 11, 11, 4, 10,
		10, 4, 9, 4, 3, 9,
		8, 9, 3, 8, 3, 2,
		1, 7, 2, 7, 8, 2,
		0, 7, 1, 0, 6, 7
	};

	public static void DrawGrid(MeshGenerationContext context, int[] grid, Vector2Int gridSize, float cellSize, Vector2 gridPosition, Rect viewport, TerrainColor[] terrainColors, bool useJobs = true, bool cullOffscreen = true)
	{
		if (gridSize.x <= 0 || gridSize.y <= 0 || cellSize <= 0f || float.IsNaN(cellSize) || float.IsInfinity(cellSize) || !(viewport.width > 0f) || !(viewport.height > 0f))
			return;

		using (PrepareMarker.Auto())
		{
			Vector2 origin = gridPosition + Vector2.one * DrawingPadding;
			GetVisibleRange(gridSize, cellSize, origin, viewport, cullOffscreen, out int firstColumn, out int lastColumn, out int firstRow, out int lastRow);

			if (firstColumn > lastColumn || firstRow > lastRow)
				return;

			int visibleColumns = lastColumn - firstColumn + 1;
			int visibleRows = lastRow - firstRow + 1;
			int visibleCount = checked(visibleColumns * visibleRows);

			var snapshot = new NativeArray<int>(visibleCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
			NativeArray<Vertex> templateVertices = default;
			NativeArray<ushort> filledTriangles = default;
			NativeArray<ushort> outlinedTriangles = default;
			NativeArray<Color32> fillColors = default;

			JobHandle allJobs = default;

			try
			{
				int destination = 0;
				for (int column = firstColumn; column <= lastColumn; column++)
				{
					long columnOffset = (long)column * gridSize.y;
					for (int row = firstRow; row <= lastRow; row++)
					{
						long sourceIndex = columnOffset + row;
						snapshot[destination++] = grid != null && sourceIndex < grid.Length
							? grid[(int)sourceIndex]
							: -1;
					}
				}

				// Copy each template once for this draw. Every chunk shares these buffers.
				templateVertices = new NativeArray<Vertex>(HexVertexTemplate, Allocator.TempJob);
				filledTriangles = new NativeArray<ushort>(FilledTriangleTemplate, Allocator.TempJob);
				outlinedTriangles = new NativeArray<ushort>(OutlinedTriangleTemplate, Allocator.TempJob);

				NativeSlice<Vertex> vertexSlice = templateVertices;
				NativeSlice<ushort> filledTriangleSlice = filledTriangles;
				NativeSlice<ushort> outlinedTriangleSlice = outlinedTriangles;

				int colorsLength = terrainColors?.Length ?? 0;
				fillColors = new NativeArray<Color32>(colorsLength, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
				for (int i = 0; i < colorsLength; i++)
					fillColors[i] = (Color32)terrainColors[i].color;


				context.GetTempMeshAllocator(out TempMeshAllocator allocator);

				int jobCount = 0;
				for (int pass = 0; pass < 2; pass++)
				{
					for (int start = 0; start < visibleCount; start += CellsPerMesh)
					{
						var job = new GridChunkJob
						{
							drawFills = pass == 0,
							cells = snapshot,
							allocator = allocator,
							templateVertices = vertexSlice,
							filledTriangles = filledTriangleSlice,
							outlinedTriangles = outlinedTriangleSlice,
							start = start,
							count = Mathf.Min(CellsPerMesh, visibleCount - start),
							firstColumn = firstColumn,
							firstRow = firstRow,
							visibleRows = visibleRows,
							cellSize = cellSize,
							origin = origin,
							fillColors = fillColors,
							outlineColor = Color.white
						};

						context.InsertMeshGenerationNode(out job.node);

						if (useJobs)
						{
							jobCount++;
							JobHandle handle = job.Schedule();
							allJobs = JobHandle.CombineDependencies(allJobs, handle);
						}
						else
						{
							job.Execute();
						}
					}
					//Debug.Log($"Visible count: {visibleCount}");
					//Debug.Log($"Job count: {jobCount}");
				}
			}
			finally
			{
				// Free job inputs after the last chunk finishes reading them.
				JobHandle cleanup = DisposeBuffer(ref snapshot, allJobs, useJobs);
				cleanup = DisposeBuffer(ref templateVertices, cleanup, useJobs);
				cleanup = DisposeBuffer(ref filledTriangles, cleanup, useJobs);
				cleanup = DisposeBuffer(ref outlinedTriangles, cleanup, useJobs);
				cleanup = DisposeBuffer(ref fillColors, cleanup, useJobs);

				if (useJobs)
					context.AddMeshGenerationJob(cleanup);
			}
		}
	}

	internal static RectInt GetLayerCellRange(Vector2Int size, float cellSize, Rect viewport)
	{
		if (size.x <= 0 || size.y <= 0 || !(cellSize > 0f) || float.IsInfinity(cellSize) ||
			!(viewport.width > 0f && viewport.height > 0f))
			return default;

		GetVisibleRange(size, cellSize, Vector2.one * DrawingPadding, viewport, true,
			out int left, out int right, out int top, out int bottom);

		if (left > right || top > bottom) return default;
		return new RectInt(left, top, right - left + 1, bottom - top + 1);
	}

	private static Vertex TemplateVertex(float x, float y)
	{
		return new Vertex { position = new Vector3(x, y, Vertex.nearZ), tint = Color.white };
	}

	private static JobHandle DisposeBuffer<T>(ref NativeArray<T> buffer, JobHandle dependency, bool useJobs)
		where T : unmanaged
	{
		if (!buffer.IsCreated)
			return dependency;

		if (useJobs)
			return buffer.Dispose(dependency);

		buffer.Dispose();
		return dependency;
	}

	private static void GetVisibleRange(Vector2Int size, float cellSize, Vector2 origin, Rect view, bool cull, out int firstColumn, out int lastColumn, out int firstRow, out int lastRow)
	{
		firstColumn = 0;
		firstRow = 0;
		lastColumn = size.x - 1;
		lastRow = size.y - 1;

		if (!cull)
			return;

		float stepX = ColumnStepFactor * cellSize;
		float stepY = HeightFactor * cellSize;
		// A centered outline extends beyond the nominal cell.
		float margin = Mathf.Max(1f, StrokeWidth * (cellSize / 50f)) * 0.5f / HeightFactor;

		firstColumn = Mathf.Max(0, Mathf.CeilToInt((view.xMin - origin.x - cellSize - margin) / stepX));
		lastColumn = Mathf.Min(size.x - 1, Mathf.FloorToInt((view.xMax - origin.x + margin) / stepX));

		// Odd columns are displaced down by half a row.
		firstRow = Mathf.Max(0, Mathf.CeilToInt((view.yMin - origin.y - 1.5f * stepY - margin) / stepY));
		lastRow = Mathf.Min(size.y - 1, Mathf.FloorToInt((view.yMax - origin.y + margin) / stepY));
	}

	private struct GridChunkJob : IJob
	{
		public bool drawFills;
		[ReadOnly] public NativeArray<int> cells;
		[ReadOnly] public TempMeshAllocator allocator;
		[ReadOnly] public NativeSlice<Vertex> templateVertices;
		[ReadOnly] public NativeSlice<ushort> filledTriangles;
		[ReadOnly] public NativeSlice<ushort> outlinedTriangles;
		public MeshGenerationNode node;
		public int start;
		public int count;
		public int firstColumn;
		public int firstRow;
		public int visibleRows;
		public float cellSize;
		public Vector2 origin;
		[ReadOnly] public NativeSlice<Color32> fillColors;
		public Color32 outlineColor;

		public void Execute()
		{
			using (BuildMarker.Auto())
			{
				int vertexCount = 0;
				int indexCount = 0;
				int end = start + count;
				for (int i = start; i < end; i++)
				{
					bool filled = HasFillColor(cells[i]);
					if (filled != drawFills) continue;
					vertexCount += filled ? FilledVertexCount : OutlinedVertexCount;
					indexCount += filled ? filledTriangles.Length : outlinedTriangles.Length;
				}

				if (vertexCount == 0) return;
				allocator.AllocateTempMesh(vertexCount, indexCount, out NativeSlice<Vertex> vertices, out NativeSlice<ushort> indices);

				float proportionalStrokeWidth = StrokeWidth * (cellSize / 50f);
				float strokeWidth = proportionalStrokeWidth < 0.1f ? proportionalStrokeWidth * 10f : Mathf.Max(1f, proportionalStrokeWidth);
				Debug.Log($"Stroke width: {strokeWidth}; Proportional stroke width: {proportionalStrokeWidth}");
				float radius = cellSize * 0.5f;
				float apothem = radius * HeightFactor;
				float outerApothem = apothem + strokeWidth * 0.5f;
				float innerApothem = Mathf.Max(0f, apothem - strokeWidth * 0.5f);
				float outerRadius = outerApothem / HeightFactor;
				float innerRadius = innerApothem / HeightFactor;

				int vertexOffset = 0;
				int indexOffset = 0;
				for (int i = start; i < end; i++)
				{
					int cellValue = cells[i];
					bool filled = HasFillColor(cellValue);
					if (filled != drawFills) continue;

					int column = firstColumn + i / visibleRows;
					int row = firstRow + i % visibleRows;
					float oddOffset = (column & 1) != 0 ? 0.5f : 0f;
					Vector2 center = origin + new Vector2(
						column * ColumnStepFactor * cellSize + radius,
						(row + oddOffset) * HeightFactor * cellSize + apothem);

					float cellRadius = filled ? radius : outerRadius;
					float cellApothem = filled ? apothem : outerApothem;
					Color32 tint = filled ? fillColors[cellValue] : outlineColor;

					for (int corner = 0; corner < CornerCount; corner++)
					{
						Vertex template = templateVertices[corner];
						vertices[vertexOffset + corner] = PlaceVertex(template, center, cellRadius, cellApothem, tint);

						if (!filled)
							vertices[vertexOffset + CornerCount + corner] = PlaceVertex(template, center, innerRadius, innerApothem, tint);
					}

					if (filled)
						vertices[vertexOffset + CornerCount] = PlaceVertex(templateVertices[CornerCount], center, radius, apothem, tint);

					NativeSlice<ushort> triangles = filled ? filledTriangles : outlinedTriangles;
					for (int j = 0; j < triangles.Length; j++)
						indices[indexOffset + j] = (ushort)(vertexOffset + triangles[j]);

					vertexOffset += filled ? FilledVertexCount : OutlinedVertexCount;
					indexOffset += triangles.Length;
				}

				// UI Toolkit owns the mesh buffers. Submit the completed chunk once.
				node.DrawMesh(vertices, indices);
			}
		}

		private static Vertex PlaceVertex(Vertex vertex, Vector2 center, float radius, float apothem, Color32 tint)
		{
			vertex.position = new Vector3(
				center.x + vertex.position.x * radius,
				center.y + vertex.position.y * apothem,
				vertex.position.z);
			vertex.tint = tint;
			return vertex;
		}

		private bool HasFillColor(int value) => value >= 0 && value < fillColors.Length;
	}
}
