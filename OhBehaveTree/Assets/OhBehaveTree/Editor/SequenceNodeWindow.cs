using UnityEditor;
using UnityEngine;

namespace AtomosZ.OhBehave.CustomEditors
{
	public class SequenceNodeWindow : CompositeNodeWindow
	{
		public SequenceNodeWindow(NodeWindow parent, Vector2 pos, INode nodeObj)
			: base(parent, new Rect(pos.x, pos.y, 100, 200), nodeObj)
		{
			bgColor = Color.yellow;
		}


		internal override void OnGUI()
		{
			GUI.backgroundColor = bgColor;

			rect = GUI.Window(windowID, rect, DrawWindow, "Sequence");
			EditorGUILayout.BeginHorizontal();
			foreach (NodeWindow node in children)
			{
				node.OnGUI();
				DrawNodeCurve(this, node);
			}
			EditorGUILayout.EndHorizontal();
		}

		protected override void DrawWindow(int id)
		{
			if (GUILayout.Button("Add Node"))
			{
				PopupWindow.Show(NodeTypeSelectPopup.PopupRect, new NodeTypeSelectPopup(this));
			}
		}
	}
}