using System;

using UnityEditor;

using UnityEngine;
using UnityEngine.UIElements;

internal class TextInputDialog : EditorWindow
{
	[SerializeField] public StyleSheet uss;

	private string textLabel = string.Empty;
	private string buttonLabel = string.Empty;

	private TextField textField = null;
	private Button acceptButton = null;

	private event Action<string> OnAccept;

	public bool acceptBlankInput = true;


	public static TextInputDialog OpenWindow(string titleContent, string textLabel, string buttonLabel, Rect parentWindowPosition, Action<string> onAccept, bool acceptBlankInput = true)
	{
		TextInputDialog window = CreateInstance<TextInputDialog>();
		window.titleContent = new GUIContent(titleContent);
		window.minSize = new Vector2(300, 70);
		window.maxSize = new Vector2(300, 70);

		window.position = new Rect(parentWindowPosition.center - window.minSize * 0.5f, window.minSize);

		window.textLabel = textLabel;
		window.buttonLabel = buttonLabel;

		window.OnAccept += onAccept;

		window.acceptBlankInput = acceptBlankInput;

		window.ShowModalUtility();

		return window;
	}

	private void CreateGUI()
	{
		if (uss != null) rootVisualElement.styleSheets.Add(uss);

		rootVisualElement.AddToClassList("abb-text-input-dialog");

		///	TEXT FIELD
		textField = new()
		{
			name = "text-field"
		};
		textField.AddToClassList("abb-text-input-dialog__text-field");
		textField.label = textLabel;

		///	CREATE BUTTON
		acceptButton = new()
		{
			name = "accept-button"
		};
		acceptButton.AddToClassList("abb-text-input-dialog__accept-button");

		Label createLabel = new(buttonLabel);
		acceptButton.Add(createLabel);

		///	ADD UI
		rootVisualElement.Add(textField);
		rootVisualElement.Add(acceptButton);

		AddLogic();
	}

	private void AddLogic()
	{
		rootVisualElement.RegisterCallback<KeyDownEvent>(evt =>
		{
			if (evt.keyCode != KeyCode.Return && evt.keyCode != KeyCode.KeypadEnter) return;

			evt.StopPropagation();
			if (!acceptButton.enabledInHierarchy) return;

			using var submit = NavigationSubmitEvent.GetPooled(evt.modifiers);
			submit.target = acceptButton;
			acceptButton.SendEvent(submit);
		}, TrickleDown.TrickleDown);

		acceptButton.SetEnabled(acceptBlankInput);
		acceptButton.clicked += OnAcceptButtonClicked;

		textField.Focus();
		textField.RegisterValueChangedCallback(evt =>
		{
			if (!acceptBlankInput)
				acceptButton.SetEnabled(!string.IsNullOrWhiteSpace(evt.newValue));
		});
	}

	private void OnAcceptButtonClicked()
	{
		OnAccept?.Invoke(textField.value);
		Close();
	}
}
