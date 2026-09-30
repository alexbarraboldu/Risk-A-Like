#if UNITY_EDITOR
using UnityEditor;

using UnityEngine;
using UnityEngine.UIElements;

public sealed class HexGridJobsDemoWindow : EditorWindow
{
	[MenuItem("Window/Hex Grid Jobs Tutorial")]
	public static void Open()
	{
		GetWindow<HexGridJobsDemoWindow>("Hex Grid Jobs");
	}

	public void CreateGUI()
	{
		rootVisualElement.Clear();
		var canvas = new CellGridCanvasJobs();
		canvas.SetGridSize(new Vector2Int(1000, 1000));

		var controls = new VisualElement();
		controls.style.flexDirection = FlexDirection.Row;
		controls.style.flexWrap = Wrap.Wrap;

		var jobs = new Toggle("Jobs") { value = true };
		jobs.RegisterValueChangedCallback(evt =>
		{
			canvas.useJobs = evt.newValue;
			canvas.MarkDirtyRepaint();
		});
		controls.Add(jobs);

		var culling = new Toggle("Cull offscreen") { value = true };
		culling.RegisterValueChangedCallback(evt =>
		{
			canvas.cullOffscreen = evt.newValue;
			canvas.MarkDirtyRepaint();
		});
		controls.Add(culling);
		controls.Add(new Button(canvas.ResetView) { text = "Reset view" });
		controls.Add(new Button(() =>
		{
			for (int column = 0; column < canvas.grid.Length; column++)
				System.Array.Clear(canvas.grid[column], 0, canvas.grid[column].Length);
			canvas.MarkDirtyRepaint();
		})
		{ text = "Clear" });

		rootVisualElement.Add(controls);
		rootVisualElement.Add(new Label("100 x 100 | Left: paint | Right: erase | Middle: pan | Wheel: zoom"));
		rootVisualElement.Add(canvas);
	}
}
#endif
