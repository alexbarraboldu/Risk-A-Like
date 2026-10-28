using UnityEngine;
using UnityEngine.UIElements;

namespace ABB.Grid.Editor.VisualElements
{
	public class Vector2IntField : VisualElement
	{
		private bool allowNegativeValues;
		private int maxIntegerSize = 0;

		private Vector2Int _value;
		public Vector2Int Value
		{
			get => _value;
			set
			{
				bool changed = _value != value;
				SetValueWithoutNotify(value);
				if (changed) ValueChanged?.Invoke(value);
			}

		}

		public event System.Action<Vector2Int> ValueChanged;

		///	Visual Elements
		private Label _label;

		private IntegerField xField;
		private IntegerField yField;


		public Vector2IntField(string label = "Vector2Int", string xLabel = "X", string yLabel = "Y", int maxVectorSizeAllocation = 256, bool allowNegativeValues = true)
		{
			this.allowNegativeValues = allowNegativeValues;

			long maxVectorCount = (long)maxVectorSizeAllocation * 1_000_000 / sizeof(int);
			maxIntegerSize = (int)System.Math.Sqrt(maxVectorCount);

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
			xField.isDelayed = true;
			yField.isDelayed = true;

			xField.RegisterValueChangedCallback(evt =>
			{
				int x = Mathf.Clamp(evt.newValue, allowNegativeValues ? -maxIntegerSize : 0, maxIntegerSize);

				SetValueWithoutNotify(new Vector2Int(x, Value.y));
				ValueChanged?.Invoke(_value);
			});

			yField.RegisterValueChangedCallback(evt =>
			{
				int y = Mathf.Clamp(evt.newValue, allowNegativeValues ? -maxIntegerSize : 0, maxIntegerSize);

				SetValueWithoutNotify(new Vector2Int(Value.x, y));
				ValueChanged?.Invoke(_value);
			});
		}


		public void SetValueWithoutNotify(Vector2Int newValue)
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
