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
		public bool isDelayed = false;
		private IVisualElementScheduledItem pickerWatcher;
		private Color auxColor;

		public int index { get; private set; } = -1;

		public List<TerrainColor> choices => colorPopup.choices;

		public event Action<int> OnValueChanged;
		public event Action<int, Color> OnColorChanged;

		private ColorPopup colorPopup = null;
		private ColorField colorField = null;


		public ColorSelectorDropdown()
		{
			style.flexDirection = FlexDirection.Row;

			VisualElement popupInput;
			BuildUI(out popupInput, new(), 0);
			AddLogic(popupInput);
		}

		public ColorSelectorDropdown(List<TerrainColor> choices, int defaultIndex = 0)
		{
			style.flexDirection = FlexDirection.Row;

			VisualElement popupInput;
			BuildUI(out popupInput, choices, defaultIndex);
			AddLogic(popupInput);
		}


		private void BuildUI(out VisualElement popupInput, List<TerrainColor> choices, int defaultIndex)
		{
			colorField = new();
			colorField.style.width = 25;
			colorField.style.height = 20;
			colorField.style.marginRight = 0;
			colorField.showEyeDropper = false;

			colorPopup = new ColorPopup(choices, defaultIndex)
			{
				focusable = true,
				style =
				{
					maxWidth = 90,
					minWidth = 90,
					height = 20
				}
			};

			popupInput = colorPopup.Q<VisualElement>(null, "unity-base-popup-field__input");

			Add(colorField);
			Add(colorPopup);
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
			auxColor = colorPopup.choices[index].color;
			colorField.SetValueWithoutNotify(colorPopup.choices[index].color);
			colorPopup.SetValueWithoutNotify(colorPopup.choices[index]);
			OnValueChanged?.Invoke(index);
		}

		public void Configure(List<TerrainColor> choices) => colorPopup.choices = choices;


		public class ColorPopup : PopupField<TerrainColor>
		{
			public ColorPopup()
			{
				formatSelectedValueCallback = FormatItem;
				formatListItemCallback = FormatItem;
			}

			public ColorPopup(List<TerrainColor> terrainColors, int defaultIndex = 0) : base(terrainColors, defaultIndex)
			{
				formatSelectedValueCallback = FormatItem;
				formatListItemCallback = FormatItem;
			}

			static string FormatItem(TerrainColor terrainColor)
			{
				return terrainColor.name;
			}
		}
	}
}
