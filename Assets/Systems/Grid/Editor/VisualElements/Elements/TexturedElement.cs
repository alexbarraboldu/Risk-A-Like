using System;

using UnityEngine;
using UnityEngine.UIElements;

namespace ABB.Grid.Editor.VisualElements
{
	/// <summary>
	///	This VisualElement is extracted from https://docs.unity.com/en-us/engine/6000.6/script-reference/unityengine/uielements/meshgenerationcontext
	///	as an example on how generateVisualContent works.
	/// </summary>
	[Obsolete("This class is not used yet", true)]
	public class TexturedElement : VisualElement
	{
		static readonly Vertex[] k_Vertices = new Vertex[4];
		static readonly ushort[] k_Indices = { 0, 1, 2, 2, 3, 0 };

		static TexturedElement()
		{
			k_Vertices[0].tint = Color.white;
			k_Vertices[1].tint = Color.white;
			k_Vertices[2].tint = Color.white;
			k_Vertices[3].tint = Color.white;

			k_Vertices[0].uv = new Vector2(0, 0);
			k_Vertices[1].uv = new Vector2(0, 1);
			k_Vertices[2].uv = new Vector2(1, 1);
			k_Vertices[3].uv = new Vector2(1, 0);
		}

		Texture2D m_Texture;

		public TexturedElement()
		{
			//This element grows to fill the available space.
			style.flexGrow = 1.0f;

			//Subscribes the OnGenerateVisualContent method to the generateVisualContent delegate.
			generateVisualContent += OnGenerateVisualContent;

			//Create a simple 2x2 checkerboard texture.
			m_Texture = new Texture2D(2, 2);
			m_Texture.SetPixels(new Color[] {
			Color.white, Color.black,
			Color.black, Color.white
		});
			m_Texture.Apply();

			//You can also load a texture from a file.
			//m_Texture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/tex.png");
		}

		//This method is called when the element needs to render its content.
		void OnGenerateVisualContent(MeshGenerationContext mgc)
		{
			Rect r = contentRect;
			if (r.width < 0.01f || r.height < 0.01f)
				return; // Skip rendering when too small.

			float left = 0;
			float right = r.width;
			float top = 0;
			float bottom = r.height;

			k_Vertices[0].position = new Vector3(left, bottom, Vertex.nearZ);
			k_Vertices[1].position = new Vector3(left, top, Vertex.nearZ);
			k_Vertices[2].position = new Vector3(right, top, Vertex.nearZ);
			k_Vertices[3].position = new Vector3(right, bottom, Vertex.nearZ);

			MeshWriteData mwd = mgc.Allocate(k_Vertices.Length, k_Indices.Length, m_Texture);

			mwd.SetAllVertices(k_Vertices);
			mwd.SetAllIndices(k_Indices);
		}
	}
}
