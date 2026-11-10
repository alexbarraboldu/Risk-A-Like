using UnityEditor;

using UnityEngine;
using UnityEngine.UIElements;

internal class LoadErrorWindow : EditorWindow
{
	private Label affectedFilesLabel = null;
	private Button continueButton = null;
	private Button closeEditorButton = null;

	private string affectedFiles = string.Empty;

	private bool closeParent = true;
	private bool regenerate = false;

	public static LoadErrorWindow OpenWindow(string titleContent, out bool closeParent, out bool regenerate, string affectedFiles)
	{
		LoadErrorWindow window = CreateInstance<LoadErrorWindow>();
		window.titleContent = new GUIContent(titleContent);
		window.minSize = new Vector2(400, 100);
		window.maxSize = new Vector2(400, 100);

		Rect unityWindow = EditorGUIUtility.GetMainWindowPosition();
		window.position = new Rect(unityWindow.center - window.minSize * 0.5f, window.minSize);

		window.affectedFiles = affectedFiles;

		window.ShowModalUtility();

		closeParent = window.closeParent;
		regenerate = window.regenerate;

		return window;
	}

	private void CreateGUI()
	{
		rootVisualElement.style.paddingLeft = 4;
		rootVisualElement.style.paddingRight = 4;
		rootVisualElement.style.paddingBottom = 4;
		rootVisualElement.style.paddingTop = 4;

		affectedFilesLabel = new(affectedFiles);
		affectedFilesLabel.style.marginTop = 4;

		closeEditorButton = new()
		{
			text = "Close editor",
			tooltip = "Close editor"
		};
		closeEditorButton.style.marginTop = 4;
		closeEditorButton.style.minHeight = 30;

		continueButton = new()
		{
			text = "Continue (will regenerate files)",
			tooltip = "Continue (will regenerate files)"
		};
		continueButton.style.marginTop = 4;
		continueButton.style.minHeight = 30;

		rootVisualElement.Add(affectedFilesLabel);
		rootVisualElement.Add(closeEditorButton);
		rootVisualElement.Add(continueButton);

		closeEditorButton.clicked += () =>
		{
			closeParent = true;
			Close();
		};

		continueButton.clicked += () =>
		{
			closeParent = false;
			regenerate = true;
			Close();
		};


	}
}
