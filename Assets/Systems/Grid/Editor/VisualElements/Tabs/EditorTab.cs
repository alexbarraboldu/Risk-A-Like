using UnityEngine.UIElements;

public class EditorTab : Tab
{
	public EditorTab(string label)
	{
		base.label = label;

		closeable = false;
	}
}
