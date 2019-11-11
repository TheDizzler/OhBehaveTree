using System;
using UnityEditor;
using UnityEngine;

namespace AtomosZ.OhBehave.CustomEditors
{
	public class LeafNodeWindow : NodeWindow
	{
		private static NodeStyle LeafNodeStyle = new NodeStyle(
			EditorGUIUtility.Load("builtin skins/darkskin/images/node1.png") as Texture2D,
			EditorGUIUtility.Load("builtin skins/darkskin/images/node1 on.png") as Texture2D);


		public LeafNodeWindow(CompositeNodeWindow parent,
			LeafNode nodeObj,
			GUIStyle inPointStyle,
			Action<ConnectionPoint> OnClickInPoint)
				: base(parent, nodeObj, inPointStyle, OnClickInPoint)
		{
			bgColor = LeafColor;
			nodeStyle = LeafNodeStyle;
			currentStyle = nodeStyle.defaultStyle;
			rect = new Rect(parent.rect.x, parent.rect.y + 50,
				currentStyle.normal.background.width * 4, currentStyle.normal.background.height);
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
							currentStyle = nodeStyle.selectedStyle;
							Selection.SetActiveObjectWithContext(nodeObject, null);
						}
						else
						{
							GUI.changed = true;
							isSelected = false;
							currentStyle = nodeStyle.defaultStyle;
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