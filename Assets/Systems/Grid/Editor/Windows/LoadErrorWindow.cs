using UnityEditor;

using UnityEngine;
using UnityEngine.UIElements;

internal class LoadErrorWindow : EditorWindow
{
	private const string StylePath = "Assets/Systems/Grid/Editor/StyleSheets/LoadErrorWindow.uss";
	public StyleSheet uss;

	private Label affectedFilesLabel = null;
	private Button continueButton = null;
	private Button closeEditorButton = null;

	private string closeButtonContent = string.Empty;
	private string continueButtonContent = string.Empty;

	private string affectedFiles = string.Empty;

	private bool onClose = true;
	private bool onContinue = false;


	public static LoadErrorWindow OpenWindow(string titleContent, out bool onClose, out bool onContinue, string closeButtonContent, string continueButtonContent, string affectedFiles)
	{
		LoadErrorWindow window = CreateInstance<LoadErrorWindow>();
		window.titleContent = new GUIContent(titleContent);
		window.minSize = new Vector2(400, 100);
		window.maxSize = new Vector2(400, 100);

		Rect unityWindow = EditorGUIUtility.GetMainWindowPosition();
		window.position = new Rect(unityWindow.center - window.minSize * 0.5f, window.minSize);

		window.closeButtonContent = closeButtonContent;
		window.continueButtonContent = continueButtonContent;

		window.affectedFiles = affectedFiles;

		window.ShowModalUtility();

		onClose = window.onClose;
		onContinue = window.onContinue;

		return window;
	}

	private void CreateGUI()
	{
		if (uss == null) uss = AssetDatabase.LoadAssetAtPath<StyleSheet>(StylePath);
		if (uss != null) rootVisualElement.styleSheets.Add(uss);

		rootVisualElement.name = "load-error-window-root";

		affectedFilesLabel = new(affectedFiles)
		{
			name = "load-error-window-affected-files-label"
		};

		closeEditorButton = new()
		{
			name = "load-error-window-close-editor-button",
			text = closeButtonContent,
			tooltip = closeButtonContent
		};

		continueButton = new()
		{
			name = "load-error-window-continue-button",
			text = continueButtonContent,
			tooltip = continueButtonContent
		};

		rootVisualElement.Add(affectedFilesLabel);
		rootVisualElement.Add(closeEditorButton);
		rootVisualElement.Add(continueButton);

		closeEditorButton.clicked += () =>
		{
			onClose = true;
			Close();
		};

		continueButton.clicked += () =>
		{
			onClose = false;
			onContinue = true;
			Close();
		};
	}
}
