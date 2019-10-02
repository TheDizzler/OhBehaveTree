using System;
using UnityEditor;
using UnityEngine;

namespace AtomosZ.OhBehave.CustomEditors
{
	public class SequenceNodeWindow : CompositeNodeWindow
	{
		private static NodeStyle SequenceNodeStyle = new NodeStyle(
			EditorGUIUtility.Load("builtin skins/darkskin/images/node0.png") as Texture2D,
			EditorGUIUtility.Load("builtin skins/darkskin/images/node0 on.png") as Texture2D);

		public SequenceNodeWindow(CompositeNodeWindow parent,
			SequenceNode nodeObj,
			GUIStyle inPointStyle, GUIStyle outPointStyle,
			Action<ConnectionPoint> OnClickInPoint, Action<ConnectionPoint> OnClickOutPoint)
				: base(parent, nodeObj,
					inPointStyle, outPointStyle,
					OnClickInPoint, OnClickOutPoint)
		{
			bgColor = Color.yellow;
			nodeStyle = SequenceNodeStyle;
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