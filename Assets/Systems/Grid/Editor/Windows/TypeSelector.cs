using System;

using ABB.Grid.Editor.Utils;

using UnityEditor;
using UnityEditor.UIElements;

using UnityEngine;
using UnityEngine.UIElements;

namespace ABB.Grid.Editor.Windows
{
	internal class TypeSelector : EditorWindow
	{
		private Type _type = typeof(MonoScript);
		public Type Type
		{
			get => _type;
			set
			{
				_type = value;
				LoadScripts();
			}
		}

		private ToolbarSearchField _searchField;

		private Tab _tab;

		public static TypeSelector OpenWindow()
		{
			TypeSelector window = ScriptableObject.CreateInstance<TypeSelector>();
			window.titleContent = new GUIContent("Select Script");
			window.minSize = new Vector2(150, 300);

			window.ShowAuxWindow();

			return window;
		}

		private void CreateGUI()
		{
			rootVisualElement.style.flexDirection = FlexDirection.Column;
			rootVisualElement.style.alignItems = Align.Stretch;

			_searchField = new();
			_searchField.style.width = StyleKeyword.Auto;
			_searchField.style.height = 16;
			_searchField.style.flexGrow = 0;
			_searchField.style.flexShrink = 0;

			_tab = new("Assets");
			_tab.style.flexGrow = 1;

			_tab.contentContainer.style.flexDirection = FlexDirection.Column;

			rootVisualElement.Add(_searchField);
			rootVisualElement.Add(_tab);
		}

		private void LoadScripts()
		{
			string[] searchInFolders = { "Assets" };

			string[] assetGUIDs = AssetDatabase.FindAssets($"t:{typeof(MonoScript).Name}", searchInFolders);

			foreach (string guid in assetGUIDs)
			{
				if (GUID.TryParse(guid, out GUID result))
				{
					MonoScript script = AssetDatabase.LoadAssetByGUID<MonoScript>(result);

					Type type = script.GetClass();
					if (type == null || !typeof(IExportGrid).IsAssignableFrom(type) || type.Name == Type.Name)
						continue;

					if (script != null)
					{
						Label label = new(script.name);
						_tab.contentContainer.Add(label);
					}
				}
			}
		}
	}
}
