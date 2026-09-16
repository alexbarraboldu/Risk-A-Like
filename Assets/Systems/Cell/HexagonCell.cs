using ABB.Cell.Data;
using ABB.Cell.Handlers;
using ABB.Grid.Biome;

using UnityEngine;

namespace ABB.Cell
{
	[ExecuteInEditMode]
	[RequireComponent(typeof(PointerHandler))]
	[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
	public class HexagonCell : MonoBehaviour
	{
		///	Fields
		private Mesh _mesh = null;

		private MeshFilter _meshFilter = null;
		private MeshRenderer _meshRenderer = null;

		[SerializeField] private Material _cellMaterial = null;
		[SerializeField] private Material _highlightMaterial = null;

		private Material[] _withoutShader = null;
		private Material[] _withShader = null;


		private MeshCollider _meshCollider = null;

		private PointerHandler _pointerHandler = null;

		///	Data
		private CellData _data = null;

		///	Properties
		public CellData Data => _data;


		private void Awake()
		{
			if (_meshFilter == null)
				_meshFilter = GetComponent<MeshFilter>();

			if (_meshRenderer == null)
				_meshRenderer = GetComponent<MeshRenderer>();

			if (_meshCollider == null)
				_meshCollider = GetComponent<MeshCollider>();

			if (_pointerHandler == null)
				_pointerHandler = GetComponent<PointerHandler>();


			if (_pointerHandler != null)
			{
				_pointerHandler.PointerClicked += OnCellClicked;
				_pointerHandler.PointerHovered += OnCellHovered;
			}

			///	Retrieve Data from DB or SO
			_data = new();

			_withoutShader = new[] { _cellMaterial };
			_withShader = new[] { _cellMaterial, _highlightMaterial };
		}

		private void GeneratePrimitive()
		{
			_mesh = ABB.Polygons.Hexagon.GeneratePrimitive();

			AssignMesh(_mesh);
		}

		public void AssignMesh(Mesh mesh)
		{
			if (mesh == null) return;

			_mesh = mesh;

			if (_meshFilter == null)
				_meshFilter = GetComponent<MeshFilter>();

			if (_meshCollider == null)
				_meshCollider = GetComponent<MeshCollider>();

			_meshFilter.mesh = _mesh;
			_meshCollider.sharedMesh = _mesh;
		}

		public void AssignName(int i, int j)
		{
			name = $"Cell_R{i}_C{j}";
		}

		public void AssignPosition(int i, int j)
		{
			_data.Position.x = i;
			_data.Position.y = j;
		}

		public void SetBiome(Biome biome)
		{
			_data.TerrainType = biome.terrain;

			_withoutShader[0] = biome.material;
			_withShader[0] = biome.material;

			_meshRenderer.materials = _withoutShader;
		}

		private void OnCellClicked()
		{
			Debug.Log($"ID: {_data.ID.Substring(0, 7)} | Position: {_data.Position} | Terrain type: {_data.TerrainType}");
		}

		private void OnCellHovered(bool isHovered)
		{
			_meshRenderer.materials = isHovered ? _withShader : _withoutShader;
		}

		public Vector2 GetSize()
		{
			if (_mesh == null)
				GeneratePrimitive();

			Vector2 size = new
				(_mesh.bounds.extents.x * 2,
				_mesh.bounds.extents.y * 2);

			return size;
		}
	}
}
