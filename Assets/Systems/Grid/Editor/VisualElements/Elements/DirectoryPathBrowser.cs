using System;
using System.Linq;

using UnityEditor;

using UnityEngine.UIElements;

public class DirectoryPathBrowser : VisualElement
{
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
		this.folderPanelTitle = folderPanelTitle;

		style.flexDirection = FlexDirection.Row;
		style.marginLeft = 6;
		style.marginRight = 6;
		style.marginTop = 6;
		style.marginBottom = 1;

		textField = new TextField(label);
		textField.isReadOnly = true;
		textField.RegisterValueChangedCallback(FolderFieldValueChanged);
		textField.style.marginLeft = 0;
		textField.style.marginRight = 0;
		textField.style.flexGrow = 1;
		textField.style.flexShrink = 1;
		textField.Children().ToArray()[0].style.minWidth = 100;

		browseButton = new Button(OnBrowseClick);
		browseButton.text = "Browse...";
		browseButton.style.marginRight = 0;

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
