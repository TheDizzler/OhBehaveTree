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
		public GUIStyle style;
		public Action<ConnectionPoint> OnClickConnectionPoint;


		public ConnectionPoint(NodeWindow node, ConnectionPointType type, Action<ConnectionPoint> OnClickConnectionPoint)
		{
			var ohBehave = EditorWindow.GetWindow<OhBehaveEditorWindow>();
			this.nodeWindow = node;
			this.type = type;
			if (type == ConnectionPointType.In)
				style = OhBehaveEditorWindow.InPointStyle;
			else
				style = OhBehaveEditorWindow.OutPointStyle;
			this.OnClickConnectionPoint = OnClickConnectionPoint;


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

			if (GUI.Button(rect, "", style))
			{
				OnClickConnectionPoint?.Invoke(this);
			}
		}
	}
}