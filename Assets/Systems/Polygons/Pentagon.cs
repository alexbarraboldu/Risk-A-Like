using UnityEngine;

namespace ABB.Polygons
{
	public static class Pentagon
	{
		public static Mesh GeneratePrimitive(float width = 1f)
		{
			///	Neighbours directions are extracted from the dot product of each face
			///	Or through diving 360 by _faces and finding the direction through the angle

			Mesh mesh = new Mesh();

			///	Calculate vertices
			//float width = 1f;
			float inradius = width / 2;

			float pCircumRadius = inradius;
			float pInradius = pCircumRadius * Mathf.Cos(36f * Mathf.Deg2Rad);

			float side = Mathf.Sqrt((pCircumRadius * pCircumRadius) - (pInradius * pInradius)) * 2f;


			Vector3[] vertices = new Vector3[5]
				{
			///	bottom left
			new Vector3(0, 0, 0),
			///	bottom right
			new Vector3(side, 0, 0),
			///	top left
			new Vector3(side / 2 - pCircumRadius * Mathf.Cos(18f * Mathf.Deg2Rad), pInradius + pCircumRadius * Mathf.Sin(18f * Mathf.Deg2Rad), 0),
			///	top right
			new Vector3(side / 2 + pCircumRadius * Mathf.Cos(18f * Mathf.Deg2Rad), pInradius + pCircumRadius * Mathf.Sin(18f * Mathf.Deg2Rad), 0),
			///	top middle
			new Vector3(side / 2, pInradius + pCircumRadius, 0)
				};
			mesh.vertices = vertices;

			///	Calculate triangles
			int[] tris = new int[9]
			{
			/// left
			0, 2, 4,
			/// middle
			0, 4, 1,
			/// right
			1, 4, 3
			};
			mesh.triangles = tris;

			///	Calculate normals
			Vector3[] normals = new Vector3[5]
			{
			-Vector3.forward,
			-Vector3.forward,
			-Vector3.forward,
			-Vector3.forward,
			-Vector3.forward
			};
			mesh.normals = normals;

			///	Calculate texture coordinates
			///	These are very aproximated
			Vector2[] uv = new Vector2[5]
			{
			new Vector2(1/5, 0),
			new Vector2(4/5, 0),
			new Vector2(0, 3/5),
			new Vector2(1, 3/5),
			new Vector2(1/2, 1)
			};
			mesh.uv = uv;

			return mesh;
		}
	}
}
