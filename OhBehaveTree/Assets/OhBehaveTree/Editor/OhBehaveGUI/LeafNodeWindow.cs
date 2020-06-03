using System;
using UnityEditor;
using UnityEngine;

namespace AtomosZ.OhBehave.EditorTools
{
	public class LeafNodeWindow : NodeWindow
	{
		private bool isExpanded;


		public LeafNodeWindow(NodeEditorObject node) : base(node) { }


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
			Color clr = GUI.backgroundColor;
			inPoint.Draw();

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
						NodeStyle.LeafLabelStyle
					);

					//if (GUILayout.Button("Edit", NodeStyle.LeafLabelStyle))
					//{
					//	NodeEditPopup.Init(nodeObject);
					//}
				}
				GUILayout.EndHorizontal();

				NodeType newType = (NodeType)EditorGUILayout.EnumPopup(nodeObject.nodeType);
				if (newType != nodeObject.nodeType)
				{
					nodeObject.ChangeNodeType(newType);
				}

				isExpanded = EditorGUILayout.Foldout(isExpanded, new GUIContent("Actions"));
				if (isExpanded)
				{
					EditorGUILayout.LabelField("Action Start:");
					if (nodeObject.startEvent.GetPersistentEventCount() == 0)
					{
						EditorGUILayout.LabelField("\tNo Methods Set");
					}

					for (int i = 0; i < nodeObject.startEvent.GetPersistentEventCount(); ++i)
					{
						EditorGUILayout.LabelField("\t" + nodeObject.startEvent.GetPersistentMethodName(i));
					}

					EditorGUILayout.LabelField("Action:");
					if (nodeObject.actionEvent.GetPersistentEventCount() == 0)
					{
						EditorGUILayout.LabelField("\tNo Methods Set");
					}

					for (int i = 0; i < nodeObject.actionEvent.GetPersistentEventCount(); ++i)
					{
						EditorGUILayout.LabelField("\t" + nodeObject.actionEvent.GetPersistentMethodName(i));
					}
				}

				if (Event.current.type == EventType.Repaint)
				{
					Rect lastrect = GUILayoutUtility.GetLastRect();
					nodeObject.windowRect.height = lastrect.yMax + 10;
				}

				GUILayout.EndVertical();
			}
			GUILayout.EndArea();



			GUI.backgroundColor = clr;

			if (connectionToParent != null)
				connectionToParent.Draw();
		}

		public override void UpdateChildren()
		{
			Debug.LogError("Leaf nodes should NOT have children!");
		}
	}
}