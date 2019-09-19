using UnityEditor;
using UnityEngine;

namespace AtomosZ.OhBehave.CustomEditors
{
	public class SelectorNodeWindow : CompositeNodeWindow
	{
		public SelectorNodeWindow(NodeWindow parent, Vector2 pos, SelectorNode nodeObj)
			: base(parent, new Rect(pos.x, pos.y, 100, 200), nodeObj)
		{
			bgColor = new Color(1, .65f, 0);
		}

		internal override void OnGUI()
		{
			GUI.backgroundColor = bgColor;

			rect = GUI.Window(windowID, rect, DrawWindow, "Selector");
			foreach (NodeWindow node in children)
			{
				node.OnGUI();
				DrawNodeCurve(this, node);
			}
		}


		protected override void DrawWindow(int id)
		{
			if (GUILayout.Button("Add Node"))
			{
				PopupWindow.Show(NodeTypeSelectPopup.PopupRect, new NodeTypeSelectPopup(this));
			}

			//GUI.DragWindow();
		}
	}
}