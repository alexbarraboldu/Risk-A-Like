using UnityEngine;
using UnityEngine.UIElements;

public class Vector2IntElement : VisualElement
{
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

	VisualElement _root;

	Label _label;

	IntegerField xField;
	IntegerField yField;


	public Vector2IntElement(string label = "Vector2Int", string xLabel = "X", string yLabel = "Y")
	{
		BuildUI(label, xLabel, yLabel);

		AddLogic();
	}


	private void BuildUI(string label, string xLabel, string yLabel)
	{
		_root = new();
		_root.style.flexDirection = FlexDirection.Row;
		_root.style.alignItems = Align.Center;
		_root.style.paddingTop = 4;

		_label = new(label);
		_label.style.width = 100;
		_label.style.top = -10;
		_label.style.left = 3;

		VisualElement fieldsContainer = new();
		fieldsContainer.style.flexDirection = FlexDirection.Column;

		xField = new(xLabel);
		yField = new(yLabel);

		xField.value = Value.x;
		yField.value = Value.y;

		ConfigureIntegerField(xField, 70, 140);
		ConfigureIntegerField(yField, 70, 140);


		fieldsContainer.Add(xField);
		fieldsContainer.Add(yField);

		_root.Add(_label);
		_root.Add(fieldsContainer);

		base.Add(_root);
	}

	private void AddLogic()
	{
		xField.RegisterValueChangedCallback(evt =>
		{
			Value = new Vector2Int(evt.newValue, Value.y);
			ValueChanged?.Invoke(Value);
		});

		yField.RegisterValueChangedCallback(evt =>
		{
			Value = new Vector2Int(Value.x, evt.newValue);
			ValueChanged?.Invoke(Value);
		});
	}

	private static void ConfigureIntegerField(IntegerField field, float labelWidth, float totalWidth)
	{
		field.style.width = totalWidth;

		field.labelElement.style.width = labelWidth;
		field.labelElement.style.minWidth = labelWidth;
		field.labelElement.style.maxWidth = labelWidth;
		field.labelElement.style.flexBasis = labelWidth;
		field.labelElement.style.flexGrow = 0;
		field.labelElement.style.flexShrink = 0;
	}

}
