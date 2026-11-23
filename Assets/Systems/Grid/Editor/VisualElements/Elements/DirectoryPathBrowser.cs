using System;

using UnityEditor;

using UnityEngine.UIElements;

public class DirectoryPathBrowser : VisualElement
{
	private static readonly GUID StyleSheetGuid = new("4b399f63cff3ffe408f7055d55259b74");
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
		if (uss == null) uss = AssetDatabase.LoadAssetByGUID<StyleSheet>(StyleSheetGuid);
		if (uss != null) styleSheets.Add(uss);

		this.folderPanelTitle = folderPanelTitle;

		name = "directory-path-browser";
		AddToClassList("abb-directory-path-browser");

		textField = new TextField(label)
		{
			name = "text-field",
			isReadOnly = true
		};
		textField.AddToClassList("abb-directory-path-browser__text-field");
		textField.RegisterValueChangedCallback(FolderFieldValueChanged);

		browseButton = new Button(OnBrowseClick)
		{
			name = "browse-button",
			text = "Browse..."
		};
		browseButton.AddToClassList("abb-directory-path-browser__browse-button");

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
