﻿using System;
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
			if (inPoint != null)
				inPoint.ProcessEvents(e);
			outPoint.ProcessEvents(e);
			bool saveNeeded = false;
			switch (e.type)
			{
				case EventType.MouseDown:
					if (e.button == 0)
					{
						LeftClick(e);
					}
					else if (e.button == 1)
					{
						RightClick(e);
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

			if (inPoint != null) // only a root node would not have an inpoint
				inPoint.OnGUI();
			outPoint.OnGUI();
		}


		public override void DrawConnectionWires()
		{
			outPoint.DrawConnectionTo(GetChildren());
		}


		public override void UpdateChildrenList()
		{
			CreateChildList();
		}

		
		public void CreateChildConnection(NodeWindow newChild)
		{
			newChild.SetParentWindow(this);
		}


		public void RemoveChildConnection(NodeWindow removedChild)
		{
			Debug.LogWarning("child removed from window. Action required?");
		}

		private void CreateChildList()
		{
			List<string> nodeNames = new List<string>();
			if (nodeObject.children != null)
			{
				foreach (var nodeIndex in nodeObject.children)
				{
					var node = treeBlueprint.GetNodeObject(nodeIndex);
					if (node == null)
					{
						Debug.LogError("Missing child error");
						nodeNames.Add("MISSING CHILD");
					}
					else
						nodeNames.Add(node.displayName);

				}
			}

			childNodesReorderable = new ReorderableList(nodeNames, typeof(string));
			childNodesReorderable.displayAdd = false;
			childNodesReorderable.displayRemove = false;
			childNodesReorderable.onReorderCallback += ChildrenReordered;
		}

		private void ChildrenReordered(ReorderableList list)
		{
			nodeObject.ChildrenReordered(list.list);
		}
	}
}