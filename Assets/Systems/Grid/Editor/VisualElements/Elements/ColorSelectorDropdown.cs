using System;
using System.Collections.Generic;

using ABB.Gameplay.Data.Types;
using ABB.Grid.Editor.Data;

using UnityEditor.UIElements;

using UnityEngine;
using UnityEngine.UIElements;

namespace ABB.Grid.Editor.VisualElements
{
	public class ColorSelectorDropdown<E> : VisualElement where E : Enum
	{
		public Dictionary<TerrainType, Color> TerrainColors { get; private set; }

		private int _value;
		public int Value
		{
			get => _value;

			set
			{
				if (_value == value) return;

				_value = value;
				colorField.value = TerrainColors[(TerrainType)value];
				dropdownField.index = value;
				OnValueChanged?.Invoke(value);
			}
		}

		public int Length => dropdownField.choices.Count;

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
			colorField.Q<VisualElement>("unity-eyedropper").style.display = DisplayStyle.None;

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

			List<string> choices = new();
			Type terrainType = typeof(E);
			foreach (var terrain in Enum.GetValues(terrainType))
			{
				string terrainStr = terrain.ToString();
				choices.Add(terrainStr);
			}

			dropdownField.choices = choices;
			dropdownField.index = 0;
			E e = (E)Enum.Parse(terrainType, dropdownField.value);
			_value = (int)(object)e;
			dropdownField.tooltip = dropdownField.value;

			popupInput = dropdownField.Q<VisualElement>(null, "unity-base-popup-field__input");

			Add(colorField);
			Add(dropdownField);
		}

		private void AddLogic(VisualElement popupInput)
		{
			colorField.RegisterValueChangedCallback(evt =>
			{
				TerrainColors[(TerrainType)_value] = evt.newValue;
				OnColorChanged(_value, evt.newValue);
			});

			dropdownField.RegisterValueChangedCallback(evt =>
			{
				Value = (int)dropdownField.index;
			});

			dropdownField.RegisterCallback<TooltipEvent>(evt =>
			{
				evt.tooltip = dropdownField.value;
				evt.rect = popupInput.worldBound;
				evt.StopPropagation();
			}, TrickleDown.TrickleDown);
		}


		public void SetColorsPalette(TerrainColor[] terrainColors)
		{
			TerrainColors = new();

			for (int i = 0; i < terrainColors.Length; i++)
			{
				TerrainColor terrainColor = terrainColors[i];
				Color color = new Color(terrainColor.r, terrainColor.g, terrainColor.b, terrainColor.a);
				TerrainType terrain = (TerrainType)terrainColor.terrain;

				TerrainColors.TryAdd(terrain, color);
			}

			if (TerrainColors.Count != 0)
				colorField.value = TerrainColors[(TerrainType)_value];
		}

		public void SetPaletteColor(int value, Color color)
		{
			TerrainColors[(TerrainType)value] = color;
		}

		public void SetColor(int value, Color color)
		{
			colorField.value = TerrainColors[(TerrainType)value];
		}
	}
}
