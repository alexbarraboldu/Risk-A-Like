using UnityEngine;
using UnityEngine.UIElements;

namespace ABB.Grid.Editor.VisualElements
{
	public class Vector2IntField : VisualElement
	{
		private bool allowNegativeValues;

		private Vector2Int _value;
		public Vector2Int Value
		{
			get => _value;
			set
			{
				_value = value;
				xField.value = value.x;
				yField.value = value.y;
				ValueChanged?.Invoke(value);
			}

		}

		public event System.Action<Vector2Int> ValueChanged;

		///	Visual Elements
		private Label _label;

		private IntegerField xField;
		private IntegerField yField;


		public Vector2IntField(string label = "Vector2Int", string xLabel = "X", string yLabel = "Y", bool allowNegativeValues = true)
		{
			this.allowNegativeValues = allowNegativeValues;

			BuildUI(label, xLabel, yLabel);

			AddLogic();
		}


		private void BuildUI(string label, string xLabel, string yLabel)
		{
			name = "vector2int-field";

			_label = new(label) { name = "vector2int-field__label" };
			_label.style.width = 100;
			_label.style.top = -10;
			_label.style.left = 3;

			VisualElement fieldsContainer = new() { name = "vector2int-field__fields-container" };
			fieldsContainer.style.flexDirection = FlexDirection.Column;

			xField = new(xLabel) { name = "vector2int-field__fields-xField" };
			yField = new(yLabel) { name = "vector2int-field__fields-yField" };

			xField.value = Value.x;
			yField.value = Value.y;

			ConfigureIntegerField(xField, 70, 140);
			ConfigureIntegerField(yField, 70, 140);


			fieldsContainer.Add(xField);
			fieldsContainer.Add(yField);

			Add(_label);
			Add(fieldsContainer);
		}

		private void AddLogic()
		{
			xField.RegisterValueChangedCallback(evt =>
			{
				int newValue = !allowNegativeValues && evt.newValue <= 0 ? 0 : evt.newValue;

				Value = new Vector2Int(newValue, Value.y);
			});

			yField.RegisterValueChangedCallback(evt =>
			{
				int newValue = !allowNegativeValues && evt.newValue <= 0 ? 0 : evt.newValue;

				Value = new Vector2Int(Value.x, newValue);
			});
		}

		public void SetValueWithoutModify(Vector2Int newValue)
		{
			xField.SetValueWithoutNotify(newValue.x);
			yField.SetValueWithoutNotify(newValue.y);
			_value = newValue;
		}

		private static void ConfigureIntegerField(IntegerField field, float labelWidth, float totalWidth)
		{
			field.labelElement.style.width = labelWidth;
			field.labelElement.style.minWidth = labelWidth;
			field.labelElement.style.maxWidth = labelWidth;
			field.labelElement.style.flexBasis = labelWidth;
			field.labelElement.style.flexGrow = 1;
			field.labelElement.style.flexShrink = 1;
		}

	}
}
