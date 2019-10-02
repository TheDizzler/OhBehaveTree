using System;
using UnityEngine;

namespace AtomosZ.OhBehave.CustomEditors
{
	public enum ConnectionPointType { In, Out }

	public class ConnectionPoint
	{
		public Rect rect;
		public ConnectionPointType type;
		public NodeWindow node;
		public GUIStyle style;
		public Action<ConnectionPoint> OnClickConnectionPoint;


		public ConnectionPoint(NodeWindow node, ConnectionPointType type,
			GUIStyle style, Action<ConnectionPoint> OnClickConnectionPoint)
		{
			this.node = node;
			this.type = type;
			this.style = style;
			this.OnClickConnectionPoint = OnClickConnectionPoint;

			rect = new Rect(0, 0, style.normal.background.width, style.normal.background.height);
		}

		public void Draw()
		{
			rect.x = node.rect.x + node.rect.width * .5f - rect.width * .5f;

			switch (type)
			{
				case ConnectionPointType.In:
					rect.y = node.rect.y;
					break;

				case ConnectionPointType.Out:
					rect.y = node.rect.y - rect.height + node.rect.height;
					break;
			}

			if (GUI.Button(rect, "", style))
			{
				OnClickConnectionPoint?.Invoke(this);
			}
		}
	}
}