using System;

using ABB.Grid.Editor.Windows;

using UnityEditor;

using UnityEngine;
using UnityEngine.UIElements;

namespace ABB.Grid.Editor.VisualElements
{
	public class TypeField<T> : VisualElement
	{
		private Label label = null;
		private VisualElement displaySelectorParent = null;
		private TypeFieldDisplay typeFieldDisplay = null;
		private TypeFieldSelector typeFieldSelector = null;

		private Color hoverColor = Color.gray4;
		private Color normalColor = Color.gray1;
		private Color focusedBorder = new Color(58f / 255f, 121f / 255f, 187f / 255f, 1f);
		private Color focusedText = new Color(129f / 255f, 180f / 255f, 1f, 1f);

		private bool isPressed = false;


		public TypeField(string text)
		{
			focusable = true;

			style.flexDirection = FlexDirection.Row;

			style.marginLeft = 3;
			style.marginBottom = 1;
			style.marginRight = 3;
			style.marginTop = 1;

			style.paddingBottom = 8;
			style.paddingLeft = 4;
			style.paddingTop = 4;



			label = new Label();
			label.text = text;
			label.style.width = 100;

			displaySelectorParent = new VisualElement();

			typeFieldDisplay = new TypeFieldDisplay();

			typeFieldSelector = new TypeFieldSelector(typeof(T));


			RegisterCallback<PointerEnterEvent>(evt =>
			{
				if (isPressed) return;

				displaySelectorParent.style.borderLeftColor = hoverColor;
				displaySelectorParent.style.borderRightColor = hoverColor;
				displaySelectorParent.style.borderBottomColor = hoverColor;
				displaySelectorParent.style.borderTopColor = hoverColor;
			});
			RegisterCallback<PointerLeaveEvent>(evt =>
			{
				if (isPressed) return;
				displaySelectorParent.style.borderLeftColor = normalColor;
				displaySelectorParent.style.borderRightColor = normalColor;
				displaySelectorParent.style.borderBottomColor = normalColor;
				displaySelectorParent.style.borderTopColor = normalColor;
			});

			RegisterCallback<FocusInEvent>(evt =>
			{
				label.style.color = focusedText;

				displaySelectorParent.style.borderLeftColor = focusedBorder;
				displaySelectorParent.style.borderRightColor = focusedBorder;
				displaySelectorParent.style.borderBottomColor = focusedBorder;
				displaySelectorParent.style.borderTopColor = focusedBorder;

				isPressed = true;
			});
			RegisterCallback<FocusOutEvent>(evt =>
			{
				label.style.color = new Color(196f / 255f, 196f / 255f, 196f / 255f, 1f);

				displaySelectorParent.style.borderLeftColor = normalColor;
				displaySelectorParent.style.borderRightColor = normalColor;
				displaySelectorParent.style.borderBottomColor = normalColor;
				displaySelectorParent.style.borderTopColor = normalColor;

				isPressed = false;
			});


			displaySelectorParent.style.flexGrow = 2;
			displaySelectorParent.style.flexDirection = FlexDirection.Row;

			displaySelectorParent.style.marginLeft = 2;

			displaySelectorParent.style.borderLeftColor = normalColor;
			displaySelectorParent.style.borderRightColor = normalColor;
			displaySelectorParent.style.borderBottomColor = normalColor;
			displaySelectorParent.style.borderTopColor = normalColor;

			displaySelectorParent.style.borderRightWidth = 1;
			displaySelectorParent.style.borderLeftWidth = 1;
			displaySelectorParent.style.borderBottomWidth = 1;
			displaySelectorParent.style.borderTopWidth = 1;

			displaySelectorParent.style.borderBottomLeftRadius = 3;
			displaySelectorParent.style.borderBottomRightRadius = 3;
			displaySelectorParent.style.borderTopLeftRadius = 3;
			displaySelectorParent.style.borderTopRightRadius = 3;


			Add(label);
			Add(displaySelectorParent);
			displaySelectorParent.Add(typeFieldDisplay);
			displaySelectorParent.Add(typeFieldSelector);
		}


		internal class TypeFieldDisplay : VisualElement
		{
			private string text = "None (Mono Script)";

			private Label label = null;

			private string Text
			{
				get => label.text;
				set => label.text = value;
			}

			public TypeFieldDisplay()
			{
				label = new(text);

				ApplyStyle();

				Add(label);
			}

			public TypeFieldDisplay(string text)
			{
				this.text = text;

				label = new(text);

				ApplyStyle();

				Add(label);
			}

			private void ApplyStyle()
			{
				style.flexGrow = 1;
				style.flexDirection = FlexDirection.Row;

				style.backgroundColor = new Color(0.15f, 0.15f, 0.15f, 1);

				style.borderBottomLeftRadius = 3;
				style.borderTopLeftRadius = 3;

				label.style.color = Color.gray6;
				label.style.marginLeft = 3;
				label.style.marginTop = 1;

				focusable = true;
			}
		}

		internal class TypeFieldSelector : VisualElement
		{
			public Image icon = null;

			private TypeSelector typeSelector = null;

			private Color hoverColor = Color.gray3;
			private Color normalColor = new Color(0.216f, 0.216f, 0.216f, 1f); /*Color.gray2;*/

			public TypeFieldSelector(Type type)
			{
				focusable = true;

				RegisterCallback<ClickEvent>(evt =>
				{
					typeSelector = TypeSelector.OpenWindow();
					typeSelector.Type = type;
				});

				Texture2D texture = EditorGUIUtility.Load("StyleSheets/Northstar/Images/d_pick_uielements.png") as Texture2D;
				icon = new() { image = texture };

				icon.RegisterCallback<PointerEnterEvent>(evt =>
				{
					style.backgroundColor = hoverColor;
				});

				icon.RegisterCallback<PointerLeaveEvent>(evt =>
				{
					style.backgroundColor = normalColor;
				});

				ApplyStyle();

				Add(icon);
			}

			private void ApplyStyle()
			{
				style.backgroundColor = normalColor;

				icon.style.width = 18;
				icon.style.height = 16;

				//style.borderLeftWidth = 1;
				//style.borderRightWidth = 1;
				//style.borderBottomWidth = 1;
				//style.borderTopWidth = 1;

				//style.borderLeftColor = Color.gray1;
				//style.borderRightColor = Color.gray1;
				//style.borderBottomColor = Color.gray1;
				//style.borderTopColor = Color.gray1;
			}
		}
	}
}
