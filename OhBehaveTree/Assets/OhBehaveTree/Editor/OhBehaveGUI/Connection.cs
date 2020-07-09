using System;
using UnityEditor;
using UnityEngine;

namespace AtomosZ.OhBehave.EditorTools
{
	public class Connection
	{
		public static readonly Vector2 lineOffset = new Vector2(0, 40);
		public static readonly float lineThickness = 8;

		public ConnectionPoint parentPoint;
		public ConnectionPoint childPoint;
		public Action<Connection> OnClickRemoveConnection;

		public Connection(ConnectionPoint parentPoint, ConnectionPoint childPoint, Action<Connection> OnClickRemoveConnection)
		{
			this.parentPoint = parentPoint;
			this.childPoint = childPoint;
			this.OnClickRemoveConnection = OnClickRemoveConnection;
		}

		public void Draw()
		{
			Handles.DrawAAPolyLine(lineThickness, parentPoint.rect.center, parentPoint.rect.center + lineOffset, childPoint.rect.center - lineOffset, childPoint.rect.center);

			if (Handles.Button(
					(parentPoint.rect.center + childPoint.rect.center) * 0.5f, 
					Quaternion.identity, 4, 8, Handles.RectangleHandleCap))
			{
				OnClickRemoveConnection?.Invoke(this);
			}
		}
	}
}