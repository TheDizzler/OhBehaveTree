using UnityEngine;

namespace AtomosZ.OhBehave.Editor
{
	public class LeafNodeWindow : NodeWindow
	{
		public LeafNodeWindow(NodeWindow parent, Vector2 pos)
			: base(parent, new Rect(pos.x, pos.y, 100, 250))
		{
			bgColor = Color.green;
		}


		internal override void OnGUI()
		{
			GUI.backgroundColor = bgColor;

			rect = GUI.Window(windowID, rect, DrawWindow, "Leaf");
		}

		protected override void DrawWindow(int id)
		{
			GUILayout.Label(new GUIContent("Leaf"));

			GUI.DragWindow();
		}
	}
}