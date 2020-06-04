using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace AtomosZ.OhBehave.EditorTools
{
	public abstract class CompositeNodeWindow : NodeWindow, IParentNodeWindow
	{
		public ReorderableList childNodesReorderable;

		public CompositeNodeWindow(NodeEditorObject node) : base(node) { }


		public override bool ProcessEvents(Event e)
		{
			switch (e.type)
			{
				case EventType.MouseDown:
					if (e.button == 0)
					{
						LeftClick(e);
					}
					else if (e.button == 1)
					{
						if (GetRect().Contains(e.mousePosition))
						{
							treeBlueprint.ProcessContextMenu(nodeObject);
							e.Use();
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
			if (inPoint != null)
				inPoint.Draw();

			if (isSelected)
				GUI.backgroundColor = bgColor;

			var content = new GUIContent("Name: " + nodeName, nodeObject.description);

			GUILayout.BeginArea(GetRect(), content, currentStyle);
			{
				GUILayout.Label(
					new GUIContent(nodeObject.displayName + " - " + Enum.GetName(typeof(NodeType),
						nodeObject.nodeType)),
					NodeStyle.SequencerLabelStyle
				);

				NodeType newType = (NodeType)EditorGUILayout.EnumPopup(nodeObject.nodeType);
				if (newType != nodeObject.nodeType)
				{
					nodeObject.ChangeNodeType(newType);
				}


				if (childNodesReorderable == null)
					CreateChildList();
				childNodesReorderable.DoLayoutList();

				if (Event.current.type == EventType.Repaint)
				{
					Rect lastrect = GUILayoutUtility.GetLastRect();
					nodeObject.windowRect.height = lastrect.yMax;
				}
			}
			GUILayout.EndArea();


			GUI.backgroundColor = clr;

			if (outPoint != null)
				outPoint.Draw();

			if (connectionToParent != null)
				connectionToParent.Draw();
		}

		public override void UpdateChildren()
		{
			CreateChildList();
		}

		/// <summary>
		/// TODO: This will need to be re-thunk to accomodate windows being total slaves to the NodeEditorObjects.
		/// </summary>
		/// <param name="newChild"></param>
		public void CreateChildConnection(NodeWindow newChild)
		{
			newChild.CreateConnectionToParent(this);
		}

		/// <summary>
		/// TODO: This will need to be re-thunk to accomodate windows being total slaves to the NodeEditorObjects.
		/// </summary>
		/// <param name="removedChild"></param>
		public void RemoveChildConnection(NodeWindow removedChild)
		{
		}

		private void CreateChildList()
		{
			List<string> nodeNames = new List<string>();
			if (nodeObject.children != null)
				foreach (var nodeIndex in nodeObject.children)
					nodeNames.Add(treeBlueprint.GetNodeObject(nodeIndex).displayName);
			childNodesReorderable = new ReorderableList(nodeNames, typeof(string));
			childNodesReorderable.displayAdd = false;
			childNodesReorderable.displayRemove = false;
		}
	}
}