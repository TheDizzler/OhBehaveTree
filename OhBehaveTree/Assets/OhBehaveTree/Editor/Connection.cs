using System;
using UnityEditor;
using UnityEngine;

namespace AtomosZ.OhBehave.CustomEditors
{
	public class Connection
	{
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
			//Handles.DrawBezier(
			//	parentPoint.rect.center, childPoint.rect.center,
			//	parentPoint.rect.center + Vector2.left * 50f,
			//	childPoint.rect.center - Vector2.left * 50f,
			//	Color.white, null, 2f
			//);
			Handles.DrawLine(parentPoint.rect.center, childPoint.rect.center);

			if (Handles.Button(
					(parentPoint.rect.center + childPoint.rect.center) * 0.5f, 
					Quaternion.identity, 4, 8, Handles.RectangleHandleCap))
			{
				OnClickRemoveConnection?.Invoke(this);
			}
		}
	}
}