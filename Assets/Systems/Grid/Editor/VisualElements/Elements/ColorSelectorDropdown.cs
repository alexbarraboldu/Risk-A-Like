using System;
using System.Collections.Generic;
using System.Linq;

using ABB.Grid.Editor.Data;

using UnityEditor;
using UnityEditor.UIElements;

using UnityEngine;
using UnityEngine.UIElements;

namespace ABB.Grid.Editor.VisualElements
{
	public class ColorSelectorDropdown : VisualElement
	{
		private const string StylePath = "Assets/Systems/Grid/Editor/StyleSheets/ColorSelectorDropdown.uss";
		public StyleSheet uss;

		public bool isDelayed = false;
		private IVisualElementScheduledItem pickerWatcher;
		private Color auxColor;

		public int index { get; private set; } = -1;

		public List<TerrainColor> choices => colorPopup.choices;

		public event Action<int> OnIndexChanged;
		public event Action<int, Color> OnColorChanged;
		public event Action<TerrainColor> OnChoiceAdded;
		public event Action<int> OnChoiceRemoved;

		private ColorPopup colorPopup = null;
		private ColorField colorField = null;
		private Button addChoiceButton = null;
		private Button removeChoiceButton = null;


		public ColorSelectorDropdown()
		{
			Init(new());
		}

		public ColorSelectorDropdown(List<TerrainColor> choices, int defaultIndex = 0)
		{
			Init(choices, defaultIndex);
		}

		private void Init(List<TerrainColor> choices, int defaultIndex = 0)
		{
			if (uss == null) uss = AssetDatabase.LoadAssetAtPath<StyleSheet>(StylePath);
			if (uss != null) styleSheets.Add(uss);

			VisualElement popupInput;
			BuildUI(out popupInput, choices, defaultIndex);
			AddLogic(popupInput);
		}


		private void BuildUI(out VisualElement popupInput, List<TerrainColor> choices, int defaultIndex)
		{
			colorField = new();
			colorField.showEyeDropper = false;

			colorPopup = new ColorPopup(choices, defaultIndex);
			colorPopup.focusable = true;

			popupInput = colorPopup.Q<VisualElement>(null, "unity-base-popup-field__input");

			addChoiceButton = new Button(EditorGUIUtility.FindTexture("Toolbar Plus More"));
			addChoiceButton.name = "add-color-button";

			removeChoiceButton = new Button(EditorGUIUtility.FindTexture("Toolbar Minus"));
			removeChoiceButton.name = "remove-color-button";

			Add(colorField);
			Add(colorPopup);
			Add(addChoiceButton);
			Add(removeChoiceButton);
		}

		private void AddLogic(VisualElement popupInput)
		{
			var colorContainer = colorField.Q<VisualElement>("unity-color-container");
			colorContainer.RegisterCallback<PointerDownEvent>(evt => DelayedBehaviour());
			colorContainer.RegisterCallback<KeyDownEvent>(evt => DelayedBehaviour());

			colorField.RegisterValueChangedCallback(colorEvent =>
			{
				auxColor = colorEvent.newValue;

				if (isDelayed && pickerWatcher?.isActive == true) return;

				TerrainColor terrainColor = colorPopup.choices[index];
				terrainColor.color = auxColor;
				colorPopup.choices[index] = terrainColor;

				OnColorChanged?.Invoke(index, auxColor);
			});

			colorPopup.RegisterValueChangedCallback(evt =>
			{
				SetIndex(colorPopup.index);
			});

			colorPopup.RegisterCallback<TooltipEvent>(evt =>
			{
				evt.tooltip = colorPopup.value.name;
				evt.rect = popupInput.worldBound;
				evt.StopPropagation();
			}, TrickleDown.TrickleDown);

			addChoiceButton.clicked += () =>
			{
				EditorWindow owner = Resources.FindObjectsOfTypeAll<EditorWindow>()
				.FirstOrDefault(w => w.rootVisualElement == this || w.rootVisualElement.Contains(this));

				TextInputDialog window = TextInputDialog.OpenWindow(
					"Add new color",
					"Color name:",
					"Confirm",
					owner.position,
					(value) =>
					{
						TerrainColor terrainColor = new(value.ToUpper(), colorPopup.choices.Count, Color.white);
						AddChoice(terrainColor);
						OnChoiceAdded?.Invoke(terrainColor);
						SetIndex(terrainColor.terrain);
					},
					false
				);
			};

			removeChoiceButton.clicked += () =>
			{
				RemoveChoice(index);
				OnChoiceRemoved?.Invoke(index);
				SetIndex(index-- < 0 ? 0 : index--);
			};
		}

		public void AddChoice(TerrainColor newChoice)
		{
			colorPopup.choices.Add(newChoice);
		}

		public void RemoveChoice(int index)
		{
			colorPopup.choices.RemoveAt(index);
		}

		private void DelayedBehaviour()
		{
			if (!isDelayed) return;

			if (pickerWatcher?.isActive == true)
			{
				if (colorPopup.choices[index].color != auxColor)
				{
					TerrainColor terrainColor = colorPopup.choices[index];
					terrainColor.color = auxColor;

					colorPopup.choices[index] = terrainColor;
					OnColorChanged?.Invoke(index, auxColor);
				}
				pickerWatcher?.Pause();
			}

			var colorPicker = Resources.FindObjectsOfTypeAll<EditorWindow>().FirstOrDefault(window => window.GetType().FullName == "UnityEditor.ColorPicker");
			if (colorPicker == null) return;

			pickerWatcher = schedule.Execute(timerState =>
			{
				if (colorPicker == null)
				{
					if (colorPopup.choices[index].color != auxColor)
					{
						TerrainColor terrainColor = colorPopup.choices[index];
						terrainColor.color = auxColor;

						colorPopup.choices[index] = terrainColor;
						OnColorChanged?.Invoke(index, auxColor);
					}

					pickerWatcher?.Pause();
				}
			}).Every(100);
		}

		public void SetColor(int index, Color color)
		{
			TerrainColor terrainColor = colorPopup.choices[index];
			terrainColor.color = color;
			colorPopup.choices[index] = terrainColor;

			if (colorPopup.index == index)
			{
				colorField.SetValueWithoutNotify(color);
				auxColor = color;
			}
		}

		public void SetIndex(int index)
		{
			this.index = index;

			if (colorPopup.choices.Count == 0)
			{
				colorPopup.SetText("EMPTY");
				return;
			}

			auxColor = colorPopup.choices[index].color;
			colorField.SetValueWithoutNotify(colorPopup.choices[index].color);
			colorPopup.SetValueWithoutNotify(colorPopup.choices[index]);
			OnIndexChanged?.Invoke(index);
		}


		public class ColorPopup : PopupField<TerrainColor>
		{
			public ColorPopup()
			{
				Init();
			}

			public ColorPopup(List<TerrainColor> terrainColors, int defaultIndex = 0) : base(terrainColors, defaultIndex)
			{
				Init();
			}

			private void Init()
			{
				formatSelectedValueCallback = FormatItem;
				formatListItemCallback = FormatItem;
			}

			static string FormatItem(TerrainColor terrainColor)
			{
				return terrainColor.name;
			}

			public void SetText(string text)
			{
				textElement.text = text;
			}
		}
	}
}
