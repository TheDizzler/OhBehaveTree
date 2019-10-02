using System;
using UnityEditor;
using UnityEngine;

namespace AtomosZ.OhBehave.CustomEditors
{
	public class SelectorNodeWindow : CompositeNodeWindow
	{
		private static NodeStyle SelectorNodeStyle = new NodeStyle(
			EditorGUIUtility.Load("builtin skins/darkskin/images/node2.png") as Texture2D,
			EditorGUIUtility.Load("builtin skins/darkskin/images/node2 on.png") as Texture2D);

		public SelectorNodeWindow(CompositeNodeWindow parent,
			SelectorNode nodeObj,
			GUIStyle inPointStyle, GUIStyle outPointStyle,
			Action<ConnectionPoint> OnClickInPoint, Action<ConnectionPoint> OnClickOutPoint)
				: base(parent, nodeObj,
					inPointStyle, outPointStyle,
					OnClickInPoint, OnClickOutPoint)
		{
			bgColor = new Color(1, .65f, 0);
			nodeStyle = SelectorNodeStyle;
			currentStyle = nodeStyle.defaultStyle;
			if (parent == null)
				rect = new Rect(EditorWindow.GetWindow<OhBehaveEditorWindow>().position.width / 2, 0,
					currentStyle.normal.background.width * 4, currentStyle.normal.background.height);
			else
				rect = new Rect(parent.rect.x, parent.rect.y + 50,
					currentStyle.normal.background.width * 4, currentStyle.normal.background.height);
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