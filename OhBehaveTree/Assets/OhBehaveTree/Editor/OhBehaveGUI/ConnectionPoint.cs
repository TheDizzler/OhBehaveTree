using System;
using UnityEditor;
using UnityEngine;

namespace AtomosZ.OhBehave.EditorTools
{
	public enum ConnectionPointType { In, Out }

	public class ConnectionPoint
	{
		public Rect rect;
		public ConnectionPointType type;
		public NodeWindow nodeWindow;
		public Action<ConnectionPoint> OnClickConnectionPoint;

		private GUIStyle style;
		private GUIStyle hoverStyle;
		private bool drawingNewConnection;
		private OhBehaveTreeBlueprint blueprint;

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

		public void Draw()
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


			if (rect.Contains(Event.current.mousePosition))
			{
				if (blueprint.isDrawingNewConnection && !drawingNewConnection)
				{
					if (blueprint.CheckValidConnection(this))
					{
						// we can connect these two nodes!
						DrawValid();
						if (Event.current.type == EventType.MouseUp
							&& Event.current.button == 0)
						{
							blueprint.CreateNewConnection(this);
						}
						return;
					}
					DrawInvalid();
					return;
				}

				if (Event.current.type == EventType.MouseDown
					&& Event.current.button == 0)
				{
					//OnClickConnectionPoint?.Invoke(this);
					drawingNewConnection = true;
					blueprint.DrawingNewConnection(this);
				}
				else if (drawingNewConnection)
					DrawValid();
				else
					GUI.Label(rect, "", hoverStyle);
			}
			else if (drawingNewConnection)
				DrawValid();
			else
				GUI.Label(rect, "", style);

			if (drawingNewConnection)
			{
				if (Event.current.type == EventType.MouseUp
					&& Event.current.button == 0)
				{
					drawingNewConnection = false;
					blueprint.CancelNewConnection(this);
					return;
				}

				Handles.DrawLine(rect.center, Event.current.mousePosition);
			}
		}

		private void DrawInvalid()
		{

			Color clr = GUI.backgroundColor;
			GUI.backgroundColor = Color.red;
			GUI.Label(rect, "", hoverStyle);
			GUI.backgroundColor = clr;
		}

		private void DrawValid()
		{
			Color clr = GUI.backgroundColor;
			GUI.backgroundColor = Color.green;
			GUI.Label(rect, "", hoverStyle);
			GUI.backgroundColor = clr;
		}

	}
}