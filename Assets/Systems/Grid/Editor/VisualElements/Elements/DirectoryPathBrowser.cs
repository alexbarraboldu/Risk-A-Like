using System;

using UnityEditor;

using UnityEngine.UIElements;

public class DirectoryPathBrowser : VisualElement
{
	private const string StylePath = "Assets/Systems/Grid/Editor/StyleSheets/DirectoryPathBrowser.uss";
	public StyleSheet uss;

	private string folderPanelTitle = string.Empty;

	private TextField textField = null;
	private Button browseButton = null;

	public string FolderPath
	{
		get => textField.value;
		set => textField.value = value;
	}

	public event Action<ChangeEvent<string>> OnFolderFieldValueChanged;


	public DirectoryPathBrowser(string label, string folderPanelTitle = "")
	{
		if (uss == null) uss = AssetDatabase.LoadAssetAtPath<StyleSheet>(StylePath);
		if (uss != null)
		{
			styleSheets.Add(uss);
		}

		this.folderPanelTitle = folderPanelTitle;

		textField = new TextField(label);
		textField.isReadOnly = true;
		textField.RegisterValueChangedCallback(FolderFieldValueChanged);

		browseButton = new Button(OnBrowseClick);
		browseButton.text = "Browse...";

		Add(textField);
		Add(browseButton);
	}

	private void FolderFieldValueChanged(ChangeEvent<string> folderPath)
	{
		OnFolderFieldValueChanged?.Invoke(folderPath);
	}

	private void OnBrowseClick()
	{
		string path = EditorUtility.OpenFolderPanel(
				folderPanelTitle,
				textField.value,
				""
			);

		///	Keep the previous selection if the dialog is cancelled.
		if (!string.IsNullOrEmpty(path))
			textField.value = path;
	}
}
