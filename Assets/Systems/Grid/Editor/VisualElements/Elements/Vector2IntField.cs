using UnityEngine;
using UnityEngine.UIElements;

namespace ABB.Grid.Editor.VisualElements
{
	public class Vector2IntField : VisualElement
	{
		private int maxIntegerSize = 0;

		private bool allowNegativeValues;

		private Vector2Int _value;
		public Vector2Int Value
		{
			get => _value;
			set
			{
				SetValueWithoutNotify(value);
				ValueChanged?.Invoke(_value);
			}
		}

		public event System.Action<Vector2Int> ValueChanged;

		///	Visual Elements
		private Label _label;

		private IntegerField xField;
		private IntegerField yField;


		public Vector2IntField(string label = "Vector2Int", string xLabel = "X", string yLabel = "Y", bool allowNegativeValues = true, int maxIntegerSize = 256)
		{
			this.allowNegativeValues = allowNegativeValues;

			if (maxIntegerSize < 0)
			{
				maxIntegerSize = 256;
				Debug.LogError("maxIntegerSize cannot be negative. The default value (256) has been assigned");
			}
			this.maxIntegerSize = maxIntegerSize;

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

			ConfigureIntegerField(xField, 70);
			ConfigureIntegerField(yField, 70);


			fieldsContainer.Add(xField);
			fieldsContainer.Add(yField);

			Add(_label);
			Add(fieldsContainer);
		}

		private void AddLogic()
		{
			xField.isDelayed = true;
			yField.isDelayed = true;

			xField.RegisterValueChangedCallback(evt =>
			{
				Vector2 previousValue = _value;
				SetValueWithoutNotify(new Vector2Int(evt.newValue, Value.y));

				if (previousValue != _value) ValueChanged?.Invoke(_value);
			});

			yField.RegisterValueChangedCallback(evt =>
			{
				Vector2 previousValue = _value;
				SetValueWithoutNotify(new Vector2Int(Value.x, evt.newValue));

				if (previousValue != _value) ValueChanged?.Invoke(_value);
			});
		}

		public void SetValueWithoutNotify(Vector2Int newValue)
		{
			int x = Mathf.Clamp(newValue.x, allowNegativeValues ? -maxIntegerSize : 0, maxIntegerSize);
			int y = Mathf.Clamp(newValue.y, allowNegativeValues ? -maxIntegerSize : 0, maxIntegerSize);

			xField.SetValueWithoutNotify(x);
			yField.SetValueWithoutNotify(y);

			_value.x = x;
			_value.y = y;
		}

		private static void ConfigureIntegerField(IntegerField field, float labelWidth)
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
