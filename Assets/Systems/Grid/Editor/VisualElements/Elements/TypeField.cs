using System;

using ABB.Grid.Editor.Windows;

using UnityEditor;

using UnityEngine.UIElements;

namespace ABB.Grid.Editor.VisualElements
{
	public class TypeField<T> : VisualElement
	{
		private Label label = null;
		private VisualElement displaySelectorParent = null;
		private TypeFieldDisplay typeFieldDisplay = null;
		private TypeFieldSelector typeFieldSelector = null;


		public TypeField(string text)
		{
			focusable = true;

			label = new Label();
			label.text = text;
			label.style.minWidth = 100;

			displaySelectorParent = new VisualElement()
			{
				pickingMode = PickingMode.Ignore
			};

			typeFieldDisplay = new TypeFieldDisplay();

			typeFieldSelector = new TypeFieldSelector(typeof(T));


			AddToClassList("unity-base-field");
			AddToClassList("unity-object-field");

			label.AddToClassList("unity-base-field__label");
			label.AddToClassList("unity-object-field__label");

			displaySelectorParent.AddToClassList("unity-base-field__input");
			displaySelectorParent.AddToClassList("unity-object-field__input");


			Add(label);
			Add(displaySelectorParent);

			displaySelectorParent.Add(typeFieldDisplay);
			displaySelectorParent.Add(typeFieldSelector);
		}


		public bool RegisterValueChanged(Action<MonoScript> action)
		{
			if (action == null) return false;

			typeFieldSelector.RegisterValueChanged(action);
			typeFieldSelector.RegisterValueChanged(typeFieldDisplay.OnMonoScriptChanged);

			return true;
		}


		internal class TypeFieldDisplay : VisualElement
		{
			private string text = "None (Mono Script)";

			private Image icon = null;
			private Label label = null;

			public string Text
			{
				get => label.text;
				set => label.text = value;
			}


			public TypeFieldDisplay()
			{
				Initialize();
			}

			public TypeFieldDisplay(string text)
			{
				Initialize();

				this.text = text;
			}


			private void Initialize()
			{
				focusable = true;

				icon = new() { pickingMode = PickingMode.Ignore };
				label = new(text) { pickingMode = PickingMode.Ignore };

				ApplyStyle();

				Add(icon);
				Add(label);
			}

			private void ApplyStyle()
			{
				AddToClassList("unity-object-field-display");
				AddToClassList("unity-object-field__object");

				icon.AddToClassList("unity-object-field-display__icon");

				label.AddToClassList("unity-object-field-display__label");
				label.AddToClassList("unity-object-field-display__label--value-null");

			}

			public void OnMonoScriptChanged(MonoScript script)
			{
				if (script == null)
				{
					label.AddToClassList("unity-object-field-display__label--value-null");

					icon.image = null;

					label.text = text;
				}
				else
				{
					label.RemoveFromClassList("unity-object-field-display__label--value-null");

					icon.image = EditorGUIUtility.IconContent("cs Script Icon").image;

					label.text = script.name;
				}
			}
		}

		internal class TypeFieldSelector : VisualElement
		{
			private Action<MonoScript> action;
			private TypeSelector typeSelector = null;


			public TypeFieldSelector(Type type)
			{
				RegisterCallback<ClickEvent>(evt =>
				{
					typeSelector = TypeSelector.OpenWindow();
					typeSelector.Type = type;
					typeSelector.OnSelectedItem += action;
				});

				AddToClassList("unity-object-field__selector");
			}


			public bool RegisterValueChanged(Action<MonoScript> action)
			{
				if (action == null) return false;

				this.action += action;

				return true;
			}
		}
	}
}
