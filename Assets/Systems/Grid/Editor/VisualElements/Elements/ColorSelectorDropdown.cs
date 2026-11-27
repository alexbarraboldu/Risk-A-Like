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
		private static readonly GUID StyleSheetGuid = new("d6cd07adf3dd5f547ad889cc0be11f1d");
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
			if (uss == null) uss = AssetDatabase.LoadAssetByGUID<StyleSheet>(StyleSheetGuid);
			if (uss != null) styleSheets.Add(uss);

			VisualElement popupInput;
			BuildUI(out popupInput, choices, defaultIndex);
			AddLogic(popupInput);
		}


		private void BuildUI(out VisualElement popupInput, List<TerrainColor> choices, int defaultIndex)
		{
			name = "color-selector-dropdown";
			AddToClassList("abb-color-selector-dropdown");

			colorField = new()
			{
				name = "color-field",
				showEyeDropper = false
			};
			colorField.AddToClassList("abb-color-selector-dropdown__color-field");
			colorField.SetValueWithoutNotify(Color.white);

			colorPopup = new ColorPopup(choices, defaultIndex);
			colorPopup.AddToClassList("abb-color-selector-dropdown__color-popup");

			popupInput = colorPopup.Q<VisualElement>(null, "unity-base-popup-field__input");

			addChoiceButton = new Button(EditorGUIUtility.FindTexture("Toolbar Plus More"))
			{
				name = "add-color-button",
				tooltip = "Add new color"
			};
			addChoiceButton.AddToClassList("abb-color-selector-dropdown__add-color-button");

			removeChoiceButton = new Button(EditorGUIUtility.FindTexture("Toolbar Minus"))
			{
				name = "remove-color-button",
				tooltip = "Remove selected color"
			};
			removeChoiceButton.AddToClassList("abb-color-selector-dropdown__remove-color-button");

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

				if (isDelayed && pickerWatcher?.isActive == true ||
					colorPopup.choices.Count == 0)
					return;

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

			addChoiceButton.clicked += AddColorClicked;

			removeChoiceButton.clicked += () =>
			{
				int oldIndex = index;
				RemoveChoice(oldIndex);
				if (oldIndex < 0) return;
				OnChoiceRemoved?.Invoke(oldIndex);
			};
		}

		private void AddColorClicked()
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
					int newTerrainValue = colorPopup.choices.Count;
					for (int i = 0; i < colorPopup.choices.Count; i++)
					{
						if (colorPopup.choices[i].value != i)
						{
							newTerrainValue = i;
							break;
						}
					}
					TerrainColor terrainColor = new(value.ToUpper(), newTerrainValue, Color.white);
					AddChoice(terrainColor);
					OnChoiceAdded?.Invoke(terrainColor);
					SetIndex(terrainColor.value);
				},
				false
			);
		}

		public void AddChoice(TerrainColor newChoice)
		{
			colorPopup.choices.Add(newChoice);
			IEnumerable<TerrainColor> toOrder = colorPopup.choices;
			colorPopup.choices = toOrder.OrderBy(terrain => terrain.value).ToList();
		}

		public void RemoveChoice(int index)
		{
			if (colorPopup.choices.Count == 0 || index < 0) return;
			colorPopup.choices.RemoveAt(index);
			int count = colorPopup.choices.Count;
			SetIndex(index >= count ? count - 1 : index);
		}

		private void DelayedBehaviour()
		{
			if (!isDelayed) return;

			if (pickerWatcher?.isActive == true)
			{
				if (colorPopup.choices.Count != 0 && colorPopup.choices[index].color != auxColor)
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
					if (colorPopup.choices.Count != 0 && colorPopup.choices[index].color != auxColor)
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
				colorField.value = Color.white;
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
				name = "color-popup";
				focusable = true;

				formatSelectedValueCallback = FormatItem;
				formatListItemCallback = FormatItem;

				RegisterCallback<PointerDownEvent>(evt =>
				{
					if (evt.button != 0) return;

					evt.StopImmediatePropagation();
					schedule.Execute(ShowColorMenu);
				}, TrickleDown.TrickleDown);
				RegisterCallback<NavigationSubmitEvent>(evt =>
				{
					evt.StopImmediatePropagation();
					ShowColorMenu();
				}, TrickleDown.TrickleDown);
			}

			static string FormatItem(TerrainColor terrainColor)
			{
				return terrainColor.name;
			}

			public void SetText(string text)
			{
				textElement.text = text;
			}

			private void ShowColorMenu()
			{
				if (choices.Count == 0) return;

				var menu = new GenericDropdownMenu();

				for (int i = 0; i < choices.Count; i++)
				{
					TerrainColor terrain = choices[i];
					menu.AddItem(i.ToString(), i == index, () => value = terrain);

					var label = menu.contentContainer[i].Q<Label>();
					label.text = FormatItem(terrain);

					var checkmark = menu.contentContainer[i].Q(null, "unity-base-dropdown__checkmark");
					checkmark.style.position = Position.Absolute;

					VisualElement color = new() { pickingMode = PickingMode.Ignore };
					color.style.backgroundColor = terrain.color;
					color.style.width = 18;
					color.style.marginRight = 3;

					label.parent.Add(color);
					color.SendToBack();
				}

				menu.DropDown(worldBound, this, DropdownMenuSizeMode.Content);
			}
		}
	}
}
