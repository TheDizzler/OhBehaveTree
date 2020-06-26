using UnityEditor;
using UnityEngine;

namespace AtomosZ.OhBehave.EditorTools
{
	public class LeafNodeWindow : NodeWindow
	{
		private bool isExpanded;
		private GUIStyle foldoutStyle;

		public LeafNodeWindow(NodeEditorObject node) : base(node) { }


		public override bool ProcessEvents(Event e)
		{
			inPoint.ProcessEvents(e);
			bool saveNeeded = false;
			switch (e.type)
			{
				case EventType.MouseDown:
					if (e.button == 0)
					{
						LeftClick(e);
					}

					break;
				case EventType.MouseUp:
					if (isDragged)
					{
						saveNeeded = true;
						e.Use();
					}
					isDragged = false;
					break;
				case EventType.MouseDrag:
					if (e.button == 0 && isDragged)
					{
						Drag(e.delta);
						e.Use();
					}
					break;
				case EventType.KeyDown:
					if (isSelected && e.keyCode == KeyCode.Delete)
					{
						treeBlueprint.DeleteNode(nodeObject);
					}
					break;
			}

			return saveNeeded;
		}

		
		public override void OnGUI()
		{
			inPoint.OnGUI();
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

					CreateTitleBar();

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
			}
			GUILayout.EndArea();



			GUI.backgroundColor = clr;


			if (connectionToParent != null)
				connectionToParent.Draw();
		}

		public override void UpdateChildrenList()
		{
			Debug.LogError("Leaf nodes should NOT have children!");
		}
	}
}