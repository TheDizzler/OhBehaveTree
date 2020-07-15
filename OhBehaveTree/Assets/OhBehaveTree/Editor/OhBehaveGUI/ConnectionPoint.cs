﻿using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace AtomosZ.OhBehave.EditorTools
{
	public enum ConnectionPointType { In, Out }

	/// <summary>
	/// This class is for UI logic with the visual representation of the node only.
	/// A connection point should have no knowledge of the points connected to it,
	/// only the window it is attached to.
	/// </summary>
	public class ConnectionPoint
	{
		public Rect rect;
		public ConnectionPointType type;
		public NodeWindow nodeWindow;
		public Action<ConnectionPoint> OnClickConnectionPoint;

		private GUIStyle style;
		private GUIStyle hoverStyle;
		private OhBehaveTreeBlueprint blueprint;
		private bool isHovering;
		private Color hoverBGColor;


		public ConnectionPoint(NodeWindow node, ConnectionPointType type, Action<ConnectionPoint> OnClickConnectionPoint)
		{
			var ohBehave = EditorWindow.GetWindow<OhBehaveEditorWindow>();
			blueprint = ohBehave.treeBlueprint;
			this.nodeWindow = node;
			this.type = type;
			if (type == ConnectionPointType.In)
				style = OhBehaveEditorWindow.InPointStyle;
			else
				style = OhBehaveEditorWindow.OutPointStyle;
			this.OnClickConnectionPoint = OnClickConnectionPoint;

			hoverStyle = new GUIStyle();
			hoverStyle.normal.background = style.hover.background;

			rect = new Rect(0, 0, style.normal.background.width, style.normal.background.height);

		}

		public void ProcessEvents(Event e)
		{
			Rect windowRect = nodeWindow.GetRect();
			rect.x = windowRect.x + windowRect.width * .5f - rect.width * .5f;

			switch (type)
			{
				case ConnectionPointType.In:
					rect.y = windowRect.y - rect.height / 2;
					break;

				case ConnectionPointType.Out:
					rect.y = windowRect.y - rect.height / 2 + windowRect.height;
					break;
			}

			if (rect.Contains(e.mousePosition))
			{
				isHovering = true;

				if (blueprint.IsValidConnection(this))
				{
					hoverBGColor = Color.green;
				}
				else
				{
					hoverBGColor = Color.red;
				}

				if (e.button == 0)
				{
					if (e.type == EventType.MouseDown)
					{
						blueprint.StartPointSelected(this);
						e.Use();
					}
					else if (e.type == EventType.MouseUp)
					{
						blueprint.EndPointSelected(this);
						e.Use();
					}
				}
				else if (e.button == 1)
				{
					if (e.type == EventType.MouseUp)
					{
						CreateContextMenu();
						e.Use();
					}
				}
			}
			else
				isHovering = false;
		}



		public void DrawConnectionTo(List<int> childrenIndices)
		{
			if (childrenIndices == null || childrenIndices.Count == 0)
				return;

			Vector3 lineStart = rect.center + ConnectionControls.lineOffset;
			Handles.DrawAAPolyLine(ConnectionControls.lineThickness, rect.center, lineStart);

			int i = 0;
			foreach (int nodeIndex in childrenIndices)
			{
				ConnectionPoint otherPoint = blueprint.GetNodeObject(nodeIndex).GetWindow().inPoint;
				Vector3 lineEnd = otherPoint.rect.center - ConnectionControls.lineOffset;
				Handles.DrawAAPolyLine(ConnectionControls.lineThickness, lineStart,
					lineEnd, otherPoint.rect.center);
				Handles.Label((lineStart + lineEnd)/2, i.ToString());
				++i;
			}
		}


		public void OnGUI()
		{
			if (isHovering)
			{
				Color clr = GUI.backgroundColor;
				GUI.backgroundColor = hoverBGColor;
				GUI.Label(rect, "", hoverStyle);
				GUI.backgroundColor = clr;
			}
			else
				GUI.Label(rect, "", style);
		}

		private void CreateContextMenu()
		{
			switch (type)
			{
				case ConnectionPointType.In:
					if (!nodeWindow.TryGetParentName(out string parentName))
					{
						blueprint.CreateParentContextMenu(nodeWindow.nodeObject, false);
						return;
					}

					var genericMenu = new GenericMenu();
					genericMenu.AddItem(new GUIContent("Remove Connection to " + parentName), false,
						() => DisconnectParent());
					genericMenu.ShowAsContext();
					break;

				case ConnectionPointType.Out:
					var children = nodeWindow.GetChildren();
					if (children.Count > 0)
					{
						var disconnectMenu = new GenericMenu();
						disconnectMenu.allowDuplicateNames = true;
						foreach (int childIndex in children)
						{
							var node = blueprint.GetNodeObject(childIndex);
							disconnectMenu.AddItem(new GUIContent("Remove Connection to " + node.displayName), false, () => DisconnectChild(node));
						}

						disconnectMenu.ShowAsContext();
					}
					break;
			}
		}

		private void DisconnectChild(NodeEditorObject node)
		{
			Debug.Log("This does nothing but -> Disconnect " + node.displayName);
		}

		private void DisconnectParent()
		{
			Debug.Log("This does nothing but -> Disconnect parent!");
		}
	}
}