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

			if (rect.Contains(Event.current.mousePosition))
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

				if (Event.current.button == 0)
				{
					if (e.type == EventType.MouseDown)
					{
						blueprint.StartPointSelected(this);
					}
					else if (e.type == EventType.MouseUp)
					{
						blueprint.EndPointSelected(this);
						e.Use();
					}
				}

			}
			else
				isHovering = false;
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
	}
}