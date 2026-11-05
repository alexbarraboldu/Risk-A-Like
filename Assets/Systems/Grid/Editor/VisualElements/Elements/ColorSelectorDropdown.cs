using System;
using System.Collections.Generic;
using System.Linq;

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

		public Dictionary<int, Color> PaletteColors { get; set; } = new();

		private int _value = -1;
		public int Value
		{
			get => _value;

			set
			{
				if (_value == value) return;

				_value = value;
				auxColor = PaletteColors[value];
				colorField.SetValueWithoutNotify(PaletteColors[value]);
				dropdownField.SetValueWithoutNotify(dropdownField.choices[value]);
				OnValueChanged?.Invoke(value);
			}
		}

		public List<string> Choices => dropdownField.choices;

		public event Action<int> OnValueChanged;
		public event Action<int, Color> OnColorChanged;

		private DropdownField dropdownField = null;
		private ColorField colorField = null;


		public ColorSelectorDropdown()
		{
			style.flexDirection = FlexDirection.Row;

			VisualElement popupInput;
			BuildUI(out popupInput);
			AddLogic(popupInput);
		}


		private void BuildUI(out VisualElement popupInput)
		{
			colorField = new();
			colorField.style.width = 25;
			colorField.style.height = 20;
			colorField.style.marginRight = 0;
			colorField.showEyeDropper = false;

			dropdownField = new DropdownField()
			{
				focusable = true,
				style =
				{
					maxWidth = 90,
					minWidth = 90,
					height = 20
				}
			};

			popupInput = dropdownField.Q<VisualElement>(null, "unity-base-popup-field__input");

			Add(colorField);
			Add(dropdownField);
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

				PaletteColors[_value] = auxColor;
				OnColorChanged?.Invoke(_value, auxColor);
			});

			dropdownField.RegisterValueChangedCallback(evt =>
			{
				Value = dropdownField.index;
			});

			dropdownField.RegisterCallback<TooltipEvent>(evt =>
			{
				evt.tooltip = dropdownField.value;
				evt.rect = popupInput.worldBound;
				evt.StopPropagation();
			}, TrickleDown.TrickleDown);
		}

		private void DelayedBehaviour()
		{
			if (!isDelayed) return;

			if (pickerWatcher?.isActive == true)
			{
				if (PaletteColors[_value] != auxColor)
				{
					PaletteColors[_value] = auxColor;
					OnColorChanged?.Invoke(_value, auxColor);
				}
				pickerWatcher?.Pause();
			}

			var colorPicker = Resources.FindObjectsOfTypeAll<EditorWindow>().FirstOrDefault(window => window.GetType().FullName == "UnityEditor.ColorPicker");
			if (colorPicker == null) return;

			pickerWatcher = schedule.Execute(timerState =>
			{
				Debug.Log("LOL");
				if (colorPicker == null)
				{
					if (PaletteColors[_value] != auxColor)
					{
						PaletteColors[_value] = auxColor;
						OnColorChanged?.Invoke(_value, auxColor);
					}

					pickerWatcher?.Pause();
				}
			}).Every(100);
		}

		public void SetPaletteColor(int value, Color color)
		{
			PaletteColors[value] = color;
		}

		public void SetColorByValue(int value)
		{
			colorField.SetValueWithoutNotify(PaletteColors[value]);
			auxColor = PaletteColors[value];
			//colorField.value = PaletteColors[value];
		}
	}
}
