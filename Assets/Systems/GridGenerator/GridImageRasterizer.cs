using System.Collections.Generic;

using ABB.Grid;

using UnityEditor;

using UnityEngine;

public class GridImageRasterizer : MonoBehaviour
{
	[SerializeField] private bool _enableVisualHelpers = true;

	[SerializeField] private Texture2D _image = null;
	[SerializeField] private Sprite _sprite = null;

	[SerializeField, Min(1)] private int _size = 20;

	private MeshRenderer _meshRenderer = null;
	private PolygonCollider2D _polygonCollider = null;
	private GridGenerator _gridGenerator = null;

	private float _aspectRatio = 1f;

	private float _cellHeight = -1f;


	private void Awake()
	{
		_meshRenderer = GetComponentInChildren<MeshRenderer>();
		_polygonCollider = GetComponentInChildren<PolygonCollider2D>();
		_gridGenerator = FindFirstObjectByType<GridGenerator>();
	}

	private void Start()
	{
		if (_gridGenerator != null)
		{
			_cellHeight = _gridGenerator.GetCellSize().y;
			if (float.IsNegativeInfinity(_cellHeight))
				Debug.LogException(new System.DivideByZeroException("cellHeight is negative infinite"), this);
		}

		ApplyTexture(_image);
		CreateMesh(_sprite);
		Rasterize();

		_gridGenerator.GenerateGridFromIntersectionsCoordinates(intersectionsCoordinates);


		_meshRenderer.gameObject.SetActive(_enableVisualHelpers);
		_polygonCollider.gameObject.SetActive(_enableVisualHelpers);
	}

	private void OnValidate()
	{
		if (_meshRenderer == null)
			_meshRenderer = GetComponentInChildren<MeshRenderer>(true);

		if (_polygonCollider == null)
			_polygonCollider = GetComponentInChildren<PolygonCollider2D>(true);

#if UNITY_EDITOR
		EditorApplication.delayCall -= UpdateVisualHelpers;
		EditorApplication.delayCall += UpdateVisualHelpers;
#endif
	}

#if UNITY_EDITOR
	private void UpdateVisualHelpers()
	{
		if (this == null)
			return;

		if (_meshRenderer != null)
			_meshRenderer.enabled = _enableVisualHelpers;

		if (_polygonCollider != null)
			_polygonCollider.enabled = _enableVisualHelpers;
	}
#endif


	private void ApplyTexture(Texture2D texture2D)
	{
		_meshRenderer.enabled = true;

		_meshRenderer.material.SetTexture("_BaseMap", _image);

		Debug.Log($"{_image.width} | {_image.height}");

		_aspectRatio = (float)_image.width / (float)_image.height;

		Vector3 scale = Vector3.one * _size;
		scale.x *= _aspectRatio;

		_meshRenderer.transform.localScale = scale;

		_meshRenderer.enabled = _enableVisualHelpers;
	}

	private void CreateMesh(Sprite sprite)
	{
		_polygonCollider.enabled = true;

		_polygonCollider.CreateFromSprite(sprite);
		_polygonCollider.transform.localScale = Vector3.one * (_size / 8f);

		_polygonCollider.enabled = _enableVisualHelpers;
	}


	[SerializeField, TextArea(1, 4)] private string DEBUG_PolygonDimensions = "";
	private void Rasterize()
	{
		float polygonMeshScale = _polygonCollider.transform.localScale.x;

		float bottom = 0f, left = 0f, top = 0f, right = 0f;

		Vector2[] path = _polygonCollider.GetPath(0);
		foreach (Vector2 point in path)
		{
			if (point.x <= left) left = point.x;
			if (point.x >= right) right = point.x;
			if (point.y <= bottom) bottom = point.y;
			if (point.y >= top) top = point.y;
		}

		bottom = _polygonCollider.transform.position.y + bottom * polygonMeshScale;
		left = _polygonCollider.transform.position.x + left * polygonMeshScale;
		top = _polygonCollider.transform.position.y + top * polygonMeshScale;
		right = _polygonCollider.transform.position.x + right * polygonMeshScale;

		DEBUG_PolygonDimensions = $"Bottom: {bottom}\nLeft: {left}\nTop: {top}\nRight: {right}";

		///----------------------------------------------------------------------------------------

		DEBUG_intersectionLines.Clear();
		intersectionsCoordinates.Clear();

		float height = top - bottom;
		float numberOfLines = (height - _cellHeight) / _cellHeight;
		for (float i = _cellHeight / 2; i < height - (_cellHeight / 2); i += _cellHeight)
		{
			float z = transform.position.z;
			float y = bottom + i;
			Vector3 start = new Vector3(left, y, z), end = new Vector3(right, y, z);
			DEBUG_intersectionLines.Add(start, end);

			///	GET INTERSECTIONS
			List<float> intersections = GetIntersectionsAtY(_polygonCollider, y);

			for (int j = 0; j + 1 < intersections.Count; j += 2)
			{
				float startX = intersections[j];
				float endX = intersections[j + 1];

				//Debug.Log($"Start: {startX}, End: {endX}");

				intersectionsCoordinates.Add(new(startX, y), new(endX, y));
			}
		}

		///	Get how many cells are in each line on goodLines.
	}

	private Dictionary<Vector3, Vector3> intersectionsCoordinates = new();
	private Dictionary<Vector3, Vector3> DEBUG_intersectionLines = new();

	private void OnDrawGizmos()
	{
		if (!_enableVisualHelpers) return;

		foreach (var line in DEBUG_intersectionLines)
		{
			Gizmos.DrawLine(line.Key, line.Value);
		}

		foreach (var line in intersectionsCoordinates)
		{
			Gizmos.DrawSphere(line.Key, 0.1f);
			Gizmos.DrawSphere(line.Value, 0.1f);
		}
	}


	List<float> GetIntersectionsAtY(PolygonCollider2D polygon, float scanY)
	{
		List<float> intersections = new();

		for (int p = 0; p < polygon.pathCount; p++)
		{
			Vector2[] path = polygon.GetPath(p);

			for (int i = 0; i < path.Length; i++)
			{
				Vector2 a = polygon.transform.TransformPoint(path[i]);
				Vector2 b = polygon.transform.TransformPoint(
					path[(i + 1) % path.Length]
				);

				bool crosses =
					(a.y <= scanY && b.y > scanY) ||
					(b.y <= scanY && a.y > scanY);

				if (!crosses)
					continue;

				float t = (scanY - a.y) / (b.y - a.y);
				float x = Mathf.Lerp(a.x, b.x, t);

				intersections.Add(x);
			}
		}

		intersections.Sort();

		return intersections;
	}


	[ContextMenu("Rerasterize Image")]
	public void MenuReRasterize()
	{
		if (_meshRenderer == null) _meshRenderer = GetComponent<MeshRenderer>();

		ApplyTexture(_image);
		CreateMesh(_sprite);
		Rasterize();

		_gridGenerator.GenerateGridFromIntersectionsCoordinates(intersectionsCoordinates);
	}
}
