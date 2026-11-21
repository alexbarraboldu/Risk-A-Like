using System.Collections.Generic;

using ABB.Grid.Editor.Data;

using UnityEngine.UIElements;

namespace ABB.Grid.Editor.VisualElements
{
	public class ColorPopup : PopupField<TerrainColor>
	{
		public ColorPopup(List<TerrainColor> terrainColors, int defaultIndex = 0) : base(terrainColors, defaultIndex)
		{
			formatSelectedValueCallback = FormatItem;
			formatListItemCallback = FormatItem;
		}

		static string FormatItem(TerrainColor terrainColor)
		{
			return terrainColor.name;
		}
	}
}
