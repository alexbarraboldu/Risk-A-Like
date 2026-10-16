using ABB.Grid.Editor.VisualElements;

using UnityEditor;

using UnityEngine;
using UnityEngine.UIElements;

namespace ABB.Grid.Editor.Windows
{
	public class GridEditorWindow : EditorWindow
	{
		private bool guiBuilt = false;

		private const string StylePath = "Assets/";
		public StyleSheet uss;

		private EditorTab _editorTab = null;
		private SaveFilesTab _saveFilesTab = null;


		[MenuItem("Window/Custom Tools/Grid Editor")]
		private static void OpenWindow()
		{
			GridEditorWindow window = GetWindow<GridEditorWindow>();
			window.titleContent = new GUIContent("Grid Editor");
			window.minSize = new Vector2(500, 500);
		}

		public void CreateGUI()
		{
			if (uss == null) uss = AssetDatabase.LoadAssetAtPath<StyleSheet>(StylePath);
			if (uss != null) rootVisualElement.styleSheets.Add(uss);

			///	BUILD WINDOW UI
			TabView tabView = new() { name = "grid-editor-window__tab-view" };
			tabView.style.flexGrow = 1;
			tabView.contentContainer.style.flexGrow = 1;

			_editorTab = new("Editor");
			_saveFilesTab = new("Save Files");

			///	ADD UI
			rootVisualElement.Add(tabView);

			tabView.contentContainer.Add(_editorTab);
			tabView.contentContainer.Add(_saveFilesTab);

			tabView.activeTab = _editorTab;

			guiBuilt = true;
		}

		private void OnDisable()
		{
			if (!guiBuilt) return;

			_editorTab?.SaveSessionFile();
			_editorTab?.SaveTerrainColorsFile();
		}

		private void OnDestroy()
		{
			if (!guiBuilt) return;

			_editorTab?.SaveSessionFile();
			_editorTab?.SaveTerrainColorsFile();
		}
	}
}
