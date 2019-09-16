using System;
using UnityEditor;
using UnityEngine;

namespace AtomosZ.OhBehave.Editor
{
	[Serializable]
	public abstract class NodeWindow
	{
		public enum NodeType { Selector, Sequence, Leaf }

		protected Rect rect;
		protected int windowID;
		protected Color bgColor;
		/// <summary>
		/// Warning: if this is null it is the topmost node.
		/// </summary>
		protected NodeWindow parent;


		public NodeWindow(NodeWindow parent, Rect rct)
		{
			this.parent = parent;
			this.rect = rct;
			windowID = ++OhBehaveEditorWindow.NextWindowID;
		}


		internal abstract void OnGUI();


		protected abstract void DrawWindow(int id);


		protected void DrawNodeCurve(NodeWindow start, NodeWindow end)
		{
			Vector3 startPos = new Vector3(start.rect.x + start.rect.width / 2, start.rect.y + start.rect.height, 0);
			Vector3 endPos = new Vector3(end.rect.x + end.rect.width / 2, end.rect.y, 0);
			Vector3 startTan = startPos + Vector3.right * 50;
			Vector3 endTan = endPos + Vector3.left * 50;
			Color shadowCol = new Color(0, 0, 0, 0.06f);
			for (int i = 0; i < 3; i++) // Draw a shadow
				Handles.DrawBezier(startPos, endPos, startTan, endTan, shadowCol, null, (i + 3) * 5);
			Handles.DrawBezier(startPos, endPos, startTan, endTan, Color.black, null, 3);
		}
	}
}