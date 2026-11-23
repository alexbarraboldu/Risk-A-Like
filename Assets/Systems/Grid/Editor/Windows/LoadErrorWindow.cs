using UnityEditor;

using UnityEngine;
using UnityEngine.UIElements;

internal class LoadErrorWindow : EditorWindow
{
	[SerializeField] public StyleSheet uss;

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
		if (uss != null) rootVisualElement.styleSheets.Add(uss);

		rootVisualElement.name = "load-error-window";
		rootVisualElement.AddToClassList("abb-load-error-window");

		affectedFilesLabel = new(affectedFiles)
		{
			name = "affected-files-label"
		};
		affectedFilesLabel.AddToClassList("abb-load-error-window__affected-files");

		closeEditorButton = new()
		{
			name = "close-editor-button",
			text = closeButtonContent,
			tooltip = closeButtonContent
		};
		closeEditorButton.AddToClassList("abb-load-error-window__close-editor-button");

		continueButton = new()
		{
			name = "continue-button",
			text = continueButtonContent,
			tooltip = continueButtonContent
		};
		continueButton.AddToClassList("abb-load-error-window__continue-button");

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
