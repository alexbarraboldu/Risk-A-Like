using UnityEngine.UIElements;

public class ExportTab : Tab
{
	public ExportTab(string label)
	{
		base.label = label;

		closeable = false;
	}
}
