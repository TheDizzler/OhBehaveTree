using System;
using UnityEditor;
using UnityEngine;

namespace AtomosZ.OhBehave.CustomEditors
{
	public class SequenceNodeWindow : CompositeNodeWindow
	{
		public SequenceNodeWindow(CompositeNodeWindow parent, Vector2 pos, SequenceNode nodeObj,
			GUIStyle defaultStyle, GUIStyle selectedStyle, GUIStyle inPointStyle, GUIStyle outPointStyle,
			Action<ConnectionPoint> OnClickInPoint, Action<ConnectionPoint> OnClickOutPoint)
				: base(parent, new Rect(pos.x, pos.y, 100, 50), nodeObj,
				  defaultStyle, selectedStyle, inPointStyle, outPointStyle, OnClickInPoint, OnClickOutPoint)
		{
			bgColor = Color.yellow;
		}

		protected override void DrawWindow(int id)
		{
			if (GUILayout.Button("Add Node"))
			{
				PopupWindow.Show(NodeTypeSelectPopup.PopupRect, new NodeTypeSelectPopup(this));
			}

			GUI.DragWindow();
		}
	}
}