using UnityEditor;

using UnityEngine;
using UnityEngine.UIElements;

internal class LoadErrorWindow : EditorWindow
{
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
		rootVisualElement.style.paddingLeft = 4;
		rootVisualElement.style.paddingRight = 4;
		rootVisualElement.style.paddingBottom = 4;
		rootVisualElement.style.paddingTop = 4;

		affectedFilesLabel = new(affectedFiles);
		affectedFilesLabel.style.marginTop = 4;

		closeEditorButton = new()
		{
			text = closeButtonContent,
			tooltip = closeButtonContent
		};
		closeEditorButton.style.marginTop = 4;
		closeEditorButton.style.minHeight = 30;

		continueButton = new()
		{
			text = continueButtonContent,
			tooltip = continueButtonContent
		};
		continueButton.style.marginTop = 4;
		continueButton.style.minHeight = 30;

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
