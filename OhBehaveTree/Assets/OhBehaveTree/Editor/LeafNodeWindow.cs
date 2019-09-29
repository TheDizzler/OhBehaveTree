using System;
using UnityEditor;
using UnityEngine;

namespace AtomosZ.OhBehave.CustomEditors
{
	public class LeafNodeWindow : NodeWindow
	{
		public LeafNodeWindow(CompositeNodeWindow parent, Vector2 pos, LeafNode nodeObj,
			GUIStyle defaultStyle, GUIStyle selectedStyle, GUIStyle inPointStyle,
			Action<ConnectionPoint> OnClickInPoint)
				: base(parent, new Rect(pos.x, pos.y, 100, 50), nodeObj, defaultStyle, selectedStyle, inPointStyle, OnClickInPoint)
		{
			bgColor = Color.green;
		}

		internal override bool ProcessEvents(Event e)
		{
			switch (e.type)
			{
				case EventType.MouseDown:
					if (e.button == 0)
					{
						if (rect.Contains(e.mousePosition))
						{
							isDragged = true;
							GUI.changed = true;
							isSelected = true;
							currentStyle = selectedStyle;
							Selection.SetActiveObjectWithContext(nodeObject, null);
						}
						else
						{
							GUI.changed = true;
							isSelected = false;
							currentStyle = defaultStyle;
						}
					}
					break;
				case EventType.MouseUp:
					isDragged = false;
					break;
				case EventType.MouseDrag:
					if (e.button == 0 && isDragged)
					{
						Drag(e.delta);
						e.Use();
						return true;
					}
					break;
			}
			return false;
		}

		//internal override void OnGUI()
		//{
		//	inPoint.Draw();
		//	GUI.backgroundColor = bgColor;
		//	GUI.Box(rect, nodeName, defaultStyle);
		//	if (connectionToParent != null)
		//		connectionToParent.Draw();
		//}

		protected override void OnGUIExtra()
		{

		}

		protected override void DrawWindow(int id)
		{
			GUILayout.Label(new GUIContent(nodeName));

			GUI.DragWindow();
		}
	}
}