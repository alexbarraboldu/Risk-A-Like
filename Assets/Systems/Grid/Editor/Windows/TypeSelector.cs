using System;
using System.Collections.Generic;
using System.Linq;

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

		public event Action<MonoScript> OnSelectedItem;

		private ToolbarSearchField _searchField = null;
		private Tab _tab = null;
		private ListView _listView = null;

		private MonoScript[] scripts;

		public string SelectedTypeName = string.Empty;


		public static TypeSelector OpenWindow()
		{
			TypeSelector window = ScriptableObject.CreateInstance<TypeSelector>();
			window.titleContent = new GUIContent("Select Script");
			window.minSize = new Vector2(150, 300);

			//window.ShowAuxWindow();
			window.ShowUtility();

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

			_searchField.RegisterValueChangedCallback(evt =>
			{
				if (evt.newValue == string.Empty)
				{
					_listView.itemsSource = scripts;
					return;
				}

				List<MonoScript> newScripts = new();
				for (int i = 0; i < scripts.Length; i++)
				{
					MonoScript script = scripts[i];
					if (script == null) continue;
					if (script.name.Contains(evt.newValue))
					{
						newScripts.Add(script);
						Debug.Log(script.name);
					}
				}

				_listView.itemsSource = newScripts.ToArray();
			});

			_tab = new("Assets");
			_tab.style.flexGrow = 1;
			_tab.contentContainer.style.flexGrow = 1;

			_listView = new();
			_listView.fixedItemHeight = 18;
			_listView.selectionType = SelectionType.Single;

			_listView.itemsChosen += (x) =>
			{
				object obj = x.First();
				MonoScript script = (MonoScript)obj;

				SelectedTypeName = script.name;
				OnSelectedItem?.Invoke(script);

				Close();
			};

			_listView.makeItem = () =>
			{
				VisualElement parent = new();
				parent.style.flexDirection = FlexDirection.Row;
				parent.style.alignItems = Align.Center;
				parent.style.paddingLeft = 18;

				Image icon = new() { name = "icon" };
				icon.style.width = 16;
				icon.style.height = 16;
				icon.style.flexShrink = 0;

				Label label = new() { name = "label" };

				parent.Add(icon);
				parent.Add(label);

				return parent;
			};

			_listView.bindItem = (element, index) =>
			{
				MonoScript script = (MonoScript)_listView.itemsSource[index];
				string name = script?.name ?? "None";

				Image icon = element.Q<Image>("icon");
				if (script == null)
					icon.style.display = DisplayStyle.None;
				else
				{
					icon.image = AssetPreview.GetMiniThumbnail(script);
					icon.style.display = DisplayStyle.Flex;
				}

				element.name = $"Parent_{name}";
				element.Q<Label>("label").text = name;
			};

			_tab.contentContainer.Add(_listView);

			TabView tabView = new();
			tabView.style.flexGrow = 1;
			tabView.contentContainer.style.flexGrow = 1;

			tabView.contentContainer.Add(_tab);

			rootVisualElement.Add(_searchField);
			rootVisualElement.Add(tabView);
		}


		private void LoadScripts()
		{
			string[] searchInFolders = { "Assets" };
			string[] assetGUIDs = AssetDatabase.FindAssets($"t:{typeof(MonoScript).Name}", searchInFolders);

			List<MonoScript> monoScripts = new();
			monoScripts.Add(null);

			foreach (string guid in assetGUIDs)
			{
				if (GUID.TryParse(guid, out GUID result))
				{
					MonoScript script = AssetDatabase.LoadAssetByGUID<MonoScript>(result);

					Type @class = script.GetClass();
					if (@class == null || !typeof(IExportGrid).IsAssignableFrom(@class) || @class.IsInterface || @class.Name == Type.Name)
						continue;

					if (script != null)
						monoScripts.Add(script);
				}
			}

			scripts = monoScripts.ToArray();
			_listView.itemsSource = scripts;
		}
	}
}
