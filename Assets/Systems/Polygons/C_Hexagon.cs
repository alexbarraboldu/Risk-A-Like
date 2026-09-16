using System.Collections.Generic;

using UnityEngine;

namespace ABB.Polygons
{
	public static class C_Hexagon
	{
		public static Mesh GeneratePrimitive(float width = 1f)
		{
			float radius = width / 2f;
			float inRadius = Mathf.Sqrt(
				(radius * radius) - ((radius / 2f) * (radius / 2f))
			);

			Vector3[] sourceVertices =
			{
				new(-(radius / 2f), -inRadius, 0f),
				new(-radius, 0f, 0f),
				new(-(radius / 2f), inRadius, 0f),
				new(radius / 2f, inRadius, 0f),
				new(radius, 0f, 0f),
				new(radius / 2f, -inRadius, 0f),
				Vector3.zero
			};

			Vector2[] sourceUv =
			{
				new(0.25f, 0f),
				new(0f, 0.5f),
				new(0.25f, 1f),
				new(0.75f, 1f),
				new(1f, 0.5f),
				new(0.75f, 0f),
				new(0.5f, 0.5f)
			};

			int[] sourceTriangles =
			{
				0, 6, 5,
				1, 6, 0,
				2, 6, 1,
				3, 6, 2,
				4, 6, 3,
				5, 6, 4
			};

			int vertexCount = sourceTriangles.Length;

			var vertices = new Vector3[vertexCount];
			var normals = new Vector3[vertexCount];
			var uv = new Vector2[vertexCount];
			var triangles = new int[vertexCount];

			var barycentrics = new List<Vector4>(vertexCount);
			var edgeAngles = new List<Vector4>(vertexCount);
			var edgeAltitudes = new List<Vector4>(vertexCount);

			Vector4[] triangleBarycentrics =
			{
				new(1f, 0f, 0f, 0f),
				new(0f, 1f, 0f, 0f),
				new(0f, 0f, 1f, 0f)
			};

			for (int triangleStart = 0;
				triangleStart < sourceTriangles.Length;
				triangleStart += 3)
			{
				Vector3 p0 = sourceVertices[sourceTriangles[triangleStart]];
				Vector3 p1 = sourceVertices[sourceTriangles[triangleStart + 1]];
				Vector3 p2 = sourceVertices[sourceTriangles[triangleStart + 2]];

				float doubleArea = Vector3.Cross(p1 - p0, p2 - p0).magnitude;

				var triangleAltitudes = new Vector4(
					doubleArea / Vector3.Distance(p1, p2),
					doubleArea / Vector3.Distance(p2, p0),
					doubleArea / Vector3.Distance(p0, p1),
					0f
				);

				// Internal radial edges: 0
				// Outer hexagon edge: 1
				var triangleEdgeAngles = new Vector4(0f, 1f, 0f, 0f);

				for (int corner = 0; corner < 3; corner++)
				{
					int outputIndex = triangleStart + corner;
					int sourceIndex = sourceTriangles[outputIndex];

					vertices[outputIndex] = sourceVertices[sourceIndex];
					normals[outputIndex] = -Vector3.forward;
					uv[outputIndex] = sourceUv[sourceIndex];
					triangles[outputIndex] = outputIndex;

					barycentrics.Add(triangleBarycentrics[corner]);
					edgeAngles.Add(triangleEdgeAngles);
					edgeAltitudes.Add(triangleAltitudes);
				}
			}

			var mesh = new Mesh
			{
				name = "Hexagon"
			};

			mesh.vertices = vertices;
			mesh.normals = normals;
			mesh.uv = uv;
			mesh.triangles = triangles;

			mesh.SetUVs(3, barycentrics);
			mesh.SetUVs(4, edgeAngles);
			mesh.SetUVs(5, edgeAltitudes);

			mesh.RecalculateBounds();

			return mesh;
		}
	}
}
