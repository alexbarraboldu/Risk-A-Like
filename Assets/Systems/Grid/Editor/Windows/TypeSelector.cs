using System;
using System.Collections.Generic;
using System.Linq;

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

		public event Action<MonoScript> OnTypeSelected;

		private ToolbarSearchField _searchField = null;
		private TabView _tabView = null;
		private Tab _tab = null;
		private ListView _listView = null;

		private MonoScript[] scripts = Array.Empty<MonoScript>();


		public static TypeSelector OpenWindow()
		{
			TypeSelector window = ScriptableObject.CreateInstance<TypeSelector>();
			window.titleContent = new GUIContent("Select Script");
			window.minSize = new Vector2(150, 300);

			window.ShowAuxWindow();
			//window.ShowUtility();

			return window;
		}

		private void CreateGUI()
		{
			rootVisualElement.name = "unity-object-selector";

			VisualElement visualElement = new VisualElement
			{
				name = "unity-object-selector__header"
			};

			Toolbar toolbar = new Toolbar
			{
				name = "unity-object-selector__search-bar"
			};

			_searchField = new ToolbarSearchField
			{
				name = "unity-object-selector__search-field"
			};
			//_searchField.SetValueWithoutNotify(_searchFilter);
			//_searchField.RegisterCallback<KeyDownEvent>(OnSearchFieldKeyDown, TrickleDown.TrickleDown);
			//_searchField.RegisterValueChangedCallback(OnSearchFieldChanged);
			//_searchField.RegisterCallback(delegate (AttachToPanelEvent evt)
			//{
			//	((VisualElement)evt.target).Focus();
			//});
			_searchField.RegisterValueChangedCallback(OnSearchFieldChanged);

			Toolbar _toolbar = new Toolbar
			{
				name = "unity-object-selector__toolbar"
			};

			_tabView = new TabView
			{
				name = "unity-object-selector__tab-view"
			};

			_tab = new Tab
			{
				label = "Assets",
				name = "unity-object-selector__assets-tab"
			};
			_tabView.Add(_tab);
			_tabView.activeTab = _tab;

			_listView = new();
			_listView.itemsChosen += (x) =>
			{
				object obj = x.First();
				MonoScript script = (MonoScript)obj;

				OnTypeSelected?.Invoke(script);

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


			toolbar.Add(_searchField);
			visualElement.Add(toolbar);

			_toolbar.Add(_tabView);
			visualElement.Add(_toolbar);
			rootVisualElement.Add(visualElement);

			rootVisualElement.Add(_listView);

			ApplyStyle();
		}


		private void OnSearchFieldChanged(ChangeEvent<string> evt)
		{
			if (evt.newValue == string.Empty)
			{
				_listView.itemsSource = scripts;
				return;
			}

			MonoScript[] currentScripts = scripts;
			List<MonoScript> newScripts = new();
			for (int i = 0; i < scripts.Length; i++)
			{
				MonoScript script = currentScripts[i];
				if (script == null) continue;

				if (script.name.IndexOf(evt.newValue, StringComparison.OrdinalIgnoreCase) >= 0)
					newScripts.Add(script);
			}

			_listView.itemsSource = newScripts.ToArray();
		}

		internal void AddSelectorStyle(string file)
		{
			StyleSheet sheet = EditorGUIUtility.Load($"Stylesheets/ObjectSelector/{file}.uss") as StyleSheet;

			if (sheet != null)
				rootVisualElement.styleSheets.Add(sheet);
		}

		private void ApplyStyle()
		{
			rootVisualElement.AddToClassList("unity-object-selector");

			AddSelectorStyle("ObjectSelector");
			AddSelectorStyle(EditorGUIUtility.isProSkin ? "ObjectSelectorDark" : "ObjectSelectorLight");
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
					if (script == null) continue;

					Type @class = script.GetClass();
					if (@class == null ||
						!_type.IsAssignableFrom(@class) ||
						!@class.IsClass ||
						@class.IsAbstract ||
						@class.ContainsGenericParameters ||
						@class.GetConstructor(Type.EmptyTypes) == null ||
						@class.IsInterface)
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
