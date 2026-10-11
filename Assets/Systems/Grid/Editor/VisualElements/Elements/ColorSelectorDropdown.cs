using System;
using System.Collections.Generic;

using ABB.Gameplay.Data.Types;

using UnityEditor.UIElements;

using UnityEngine;
using UnityEngine.UIElements;

namespace ABB.Grid.Editor.VisualElements
{
	public class ColorSelectorDropdown<E> : VisualElement where E : Enum
	{
		public Color Color { get; private set; }
		public int Value { get; private set; }

		public event Action<int> OnValueChanged;

		public ColorSelectorDropdown()
		{
			style.flexDirection = FlexDirection.Row;

			ColorField colorField = new();
			colorField.style.width = 25;
			colorField.style.height = 20;
			colorField.style.marginRight = 0;
			colorField.Q<VisualElement>("unity-eyedropper").style.display = DisplayStyle.None;

			colorField.RegisterValueChangedCallback(evt => Color = evt.newValue);

			DropdownField layerField = new DropdownField()
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

			layerField.choices = choices;
			layerField.index = 0;
			E e = (E)Enum.Parse(terrainType, layerField.value);
			Value = (int)(object)e;
			layerField.tooltip = layerField.value;
			layerField.RegisterValueChangedCallback(evt =>
			{
				Value = (int)(TerrainType)Enum.Parse(terrainType, evt.newValue);

				OnValueChanged?.Invoke(Value);
			});

			VisualElement popupInput = layerField.Q<VisualElement>(null, "unity-base-popup-field__input");
			layerField.RegisterCallback<TooltipEvent>(evt =>
			{
				evt.tooltip = layerField.value;
				evt.rect = popupInput.worldBound;
				evt.StopPropagation();
			}, TrickleDown.TrickleDown);

			Add(colorField);
			Add(layerField);
		}

	}
}
//public static int EnumToInt<EValue>(this EValue value) where EValue : Enum => (int)(object)value;
