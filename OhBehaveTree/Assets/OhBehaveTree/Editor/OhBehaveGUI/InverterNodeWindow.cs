using System;
using UnityEditor;
using UnityEngine;

namespace AtomosZ.OhBehave.EditorTools
{
	public class InverterNodeWindow : NodeWindow
	{
		public InverterNodeWindow(NodeEditorObject nodeObj) : base(nodeObj) { }

		public override bool ProcessEvents(Event e)
		{
			switch (e.type)
			{
				case EventType.MouseDown:
					if (e.button == 0)
					{
						LeftClick(e);
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
				case EventType.KeyDown:
					if (isSelected && e.keyCode == KeyCode.Delete)
					{
						treeBlueprint.DeleteNode(nodeObject);
					}
					break;
			}
			return false;
		}

		public override void OnGUI()
		{
			if (refreshConnection)
			{
				RefreshConnection();
			}

			Color clr = GUI.backgroundColor;

			if (isSelected)
				GUI.backgroundColor = bgColor;

			var content = new GUIContent("Name: " + nodeName, nodeObject.description);


			GUILayout.BeginArea(GetRect(), content, currentStyle);
			{
				GUILayout.BeginVertical();
				GUILayout.BeginHorizontal();
				{
					GUILayout.Label(
						new GUIContent(nodeObject.displayName + " - " + Enum.GetName(typeof(NodeType),
							nodeObject.nodeType)),
						labelStyle
					);

				}
				GUILayout.EndHorizontal();

				NodeType newType = (NodeType)EditorGUILayout.EnumPopup(nodeObject.nodeType);
				if (newType != nodeObject.nodeType)
				{
					nodeObject.ChangeNodeType(newType);
				}

				if (nodeObject.children != null && nodeObject.children.Count == 1)
					GUILayout.Label("NOT " + nodeObject.children[0]);
				else
					GUILayout.Label("Dangeling Inverter");
				if (Event.current.type == EventType.Repaint)
				{
					Rect lastrect = GUILayoutUtility.GetLastRect();
					nodeObject.windowRect.height = lastrect.yMax + 10;
				}

				GUILayout.EndVertical();
			}
			GUILayout.EndArea();



			GUI.backgroundColor = clr;

			inPoint.Draw();
			if (connectionToParent != null)
				connectionToParent.Draw();

			if (outPoint != null)
				outPoint.Draw();
		}



		public override void UpdateChildren()
		{
			throw new System.NotImplementedException();
		}
	}
}