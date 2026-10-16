using System;
using System.Linq;

using UnityEditor;

using UnityEngine.UIElements;

public class DirectoryPathBrowser : VisualElement
{
	private string folderPanelTitle = string.Empty;

	private TextField folderField = null;
	private Button browseButton = null;

	public string FolderPath
	{
		get => folderField.value;
		set => folderField.value = value;
	}

	public event Action<ChangeEvent<string>> OnFolderFieldValueChanged;


	public DirectoryPathBrowser(string label, string folderPanelTitle = "")
	{
		this.folderPanelTitle = folderPanelTitle;

		style.flexDirection = FlexDirection.Row;
		style.paddingLeft = 4;
		style.paddingRight = 4;

		folderField = new TextField(label)
		{
			isReadOnly = true
		};
		folderField.RegisterValueChangedCallback(FolderFieldValueChanged);
		folderField.style.flexGrow = 1;
		folderField.style.flexShrink = 1;
		folderField.Children().ToArray()[0].style.minWidth = 100;

		browseButton = new Button(OnBrowseClick)
		{
			text = "Browse..."
		};

		Add(folderField);
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
				folderField.value,
				""
			);

		///	Keep the previous selection if the dialog is cancelled.
		if (!string.IsNullOrEmpty(path))
			folderField.value = path;
	}
}
