using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AtomosZ.OhBehave.Editor
{
	[Serializable]
	public class NodeWindow
	{
		public Rect rect;

		private List<NodeWindow> nodes = new List<NodeWindow>();
		private int windowID;


		public NodeWindow(Rect rct)
		{
			this.rect = rct;
			windowID =  ++OhBehaveEditorWindow.NextWindowID;
		}


		internal void OnGUI()
		{
			GUI.backgroundColor = Color.cyan;

			rect = GUI.Window(windowID, rect, DrawWindow, "Node" + windowID);
			foreach (NodeWindow node in nodes)
			{
				node.OnGUI();
				DrawNodeCurve(rect, node.rect);
			}
		}


		private void DrawWindow(int id)
		{
			

			GUILayout.Label(new GUIContent("Node Type"));
			if (GUILayout.Button("Add Node"))
			{
				Debug.Log("Hi!");
				nodes.Add(new NodeWindow(new Rect(40, 40, 150, 250)));
			}

			GUI.DragWindow();
		}


		void DrawNodeCurve(Rect start, Rect end)
		{
			Vector3 startPos = new Vector3(start.x + start.width, start.y + start.height / 2, 0);
			Vector3 endPos = new Vector3(end.x, end.y + end.height / 2, 0);
			Vector3 startTan = startPos + Vector3.right * 50;
			Vector3 endTan = endPos + Vector3.left * 50;
			Color shadowCol = new Color(0, 0, 0, 0.06f);
			for (int i = 0; i < 3; i++) // Draw a shadow
				Handles.DrawBezier(startPos, endPos, startTan, endTan, shadowCol, null, (i + 3) * 5);
			Handles.DrawBezier(startPos, endPos, startTan, endTan, Color.black, null, 3);
		}
	}
}