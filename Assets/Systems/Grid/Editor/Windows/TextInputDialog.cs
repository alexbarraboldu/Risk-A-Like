using System;

using UnityEditor;

using UnityEngine;
using UnityEngine.UIElements;

internal class TextInputDialog : EditorWindow
{
	private string textLabel = string.Empty;
	private string buttonLabel = string.Empty;

	private TextField textField = null;
	private Button acceptButton = null;

	private event Action<string> OnAccept;


	public static TextInputDialog OpenWindow(string titleContent, string textLabel, string buttonLabel, Rect parentWindowPosition, Action<string> onAccept)
	{
		TextInputDialog window = CreateInstance<TextInputDialog>();
		window.titleContent = new GUIContent(titleContent);
		window.minSize = new Vector2(300, 70);
		window.maxSize = new Vector2(300, 70);

		window.position = new Rect(parentWindowPosition.center - window.minSize * 0.5f, window.minSize);

		window.textLabel = textLabel;
		window.buttonLabel = buttonLabel;

		window.OnAccept += onAccept;

		window.ShowModalUtility();

		return window;
	}

	private void CreateGUI()
	{
		///	TEXT FIELD
		textField = new();
		textField.label = textLabel;

		///	CREATE BUTTON
		acceptButton = new();

		Label createLabel = new(buttonLabel);
		acceptButton.Add(createLabel);

		///	ADD UI
		rootVisualElement.Add(textField);
		rootVisualElement.Add(acceptButton);

		SetStyle();
		AddLogic();
	}


	private void SetStyle()
	{
		rootVisualElement.style.justifyContent = Justify.SpaceBetween;

		///	TEXT FIELD
		textField.style.marginTop = 8;
		textField.style.marginLeft = 8;
		textField.style.marginRight = 8;
		textField.style.alignSelf = Align.Stretch;

		textField.labelElement.style.minWidth = 68;

		///	CREATE BUTTON
		acceptButton.style.marginLeft = 8;
		acceptButton.style.marginRight = 8;
		acceptButton.style.marginBottom = 8;
		acceptButton.style.minHeight = 30;
		acceptButton.style.justifyContent = Justify.Center;

		Label label = acceptButton.Q<Label>();
		label.style.fontSize = 18;
	}

	private void AddLogic()
	{
		acceptButton.clicked += OnAcceptButtonClicked;
	}

	private void OnAcceptButtonClicked()
	{
		OnAccept?.Invoke(textField.value);
		Close();
	}
}
