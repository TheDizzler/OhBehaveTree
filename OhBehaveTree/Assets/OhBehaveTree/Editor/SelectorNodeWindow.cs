using System;
using UnityEditor;
using UnityEngine;

namespace AtomosZ.OhBehave.CustomEditors
{
	public class SelectorNodeWindow : CompositeNodeWindow
	{
		public SelectorNodeWindow(CompositeNodeWindow parent, Vector2 pos, SelectorNode nodeObj,
			GUIStyle defaultStyle, GUIStyle selectedStyle, GUIStyle inPointStyle, GUIStyle outPointStyle,
			Action<ConnectionPoint> OnClickInPoint, Action<ConnectionPoint> OnClickOutPoint)
				: base(parent, new Rect(pos.x, pos.y, 100, 50), nodeObj,
				  defaultStyle, selectedStyle, inPointStyle, outPointStyle, OnClickInPoint, OnClickOutPoint)
		{
			bgColor = new Color(1, .65f, 0);
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