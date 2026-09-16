using UnityEngine;

namespace ABB.Polygons
{
	public static class Hexagon
	{
		///	Half Height = 0.4330127;
		public static Mesh GeneratePrimitive(float width = 1f)
		{
			///	Neighbours directions are extracted from the dot product of each face
			///	Or through diving 360 by _faces and finding the direction through the angle

			Mesh mesh = new Mesh();

			///	Calculate vertices
			float radius = width / 2;
			float inRadius = Mathf.Sqrt((radius * radius) - ((radius / 2) * (radius / 2)));

			float side = radius;


			Vector3[] vertices = new Vector3[7]
			{
				///	0 bottom left
				new Vector3(-(radius / 2), -inRadius, 0),
				///	1 mid left
				new Vector3(-radius, 0, 0),
				///	2 top left
				new Vector3(-(radius / 2), inRadius, 0),
				///	3 top right
				new Vector3(radius / 2, inRadius, 0),
				///	4 mid right
				new Vector3(radius, 0, 0),
				///	5 bottom right
				new Vector3(radius / 2, -inRadius, 0),
				///	6 middle
				new Vector3(0, 0, 0)
			};

			///	Calculate triangles
			int[] tris = new int[18]
			{
				/// bottom middle
				0, 6, 5,
				/// bottom left
				1, 6, 0,
				/// top left
				2, 6, 1,
				/// top middle
				3, 6, 2,
				/// top right
				4, 6, 3,
				/// bottom right
				5, 6, 4
			};

			///	Calculate normals
			Vector3[] normals = new Vector3[7]
			{
				-Vector3.forward,
				-Vector3.forward,
				-Vector3.forward,
				-Vector3.forward,
				-Vector3.forward,
				-Vector3.forward,
				-Vector3.forward
			};

			///	Calculate texture coordinates
			///	These are very aproximated
			Vector2[] uv = new Vector2[7]
			{
			new Vector2(0.25f, 0f), /// 0 bottom left
			new Vector2(0f,    0.5f), /// 1 middle left
			new Vector2(0.25f, 1f), /// 2 top left
			new Vector2(0.75f, 1f), /// 3 top right
			new Vector2(1f,    0.5f), /// 4 middle right
			new Vector2(0.75f, 0f), /// 5 bottom right
			new Vector2(0.5f, 0.5f) /// 6 center
			};

			mesh.vertices = vertices;
			mesh.triangles = tris;
			mesh.normals = normals;
			mesh.uv = uv;

			return mesh;
		}
	}
}
