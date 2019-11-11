using System;
using UnityEditor;
using UnityEngine;

namespace AtomosZ.OhBehave.CustomEditors
{
	public abstract class NodeWindow
	{
		static protected Color RootColor = new Color(1, .65f, 1, .75f);
		static protected Color SelectorColor = new Color(1, .65f, 0, .75f);
		static protected Color LeafColor = new Color(0, 1, 0, .75f);
		static protected Color SequenceColor = new Color(1, .92f, .016f, .75f);

		public INode nodeObject;
		public Rect rect;
		public ConnectionPoint inPoint;
		public NodeStyle nodeStyle;

		protected string nodeName;
		protected int windowID;
		//protected GUIStyle defaultStyle;
		//protected GUIStyle selectedStyle;
		protected GUIStyle currentStyle;
		protected Color bgColor;
		/// <summary>
		/// Warning: if this is null it is the topmost node.
		/// </summary>
		protected CompositeNodeWindow parent;
		protected Connection connectionToParent;
		protected OhBehaveEditorWindow ohBehave;
		protected bool isDragged;
		protected bool isSelected;


		public NodeWindow(CompositeNodeWindow parent,
			INode nodeObj,
			GUIStyle inPointStyle,
			Action<ConnectionPoint> OnClickInPoint)
		{
			this.parent = parent;
			nodeObject = nodeObj;
			inPoint = new ConnectionPoint(this, ConnectionPointType.In, inPointStyle, OnClickInPoint);
			if (parent != null)
				connectionToParent = new Connection(parent.outPoint, inPoint, OnClickRemoveConnection);
			ohBehave = EditorWindow.GetWindow<OhBehaveEditorWindow>();
			nodeName = nodeObj.GetNodeType().ToString();
			//windowID = ++OhBehaveEditorWindow.NextWindowID;
			//currentStyle = nodeStyle.defaultStyle;
		}


		internal void OnGUI()
		{
			//if (currentStyle == null)
			//	currentStyle = nodeStyle.defaultStyle;
			inPoint.Draw();
			OnGUIExtra();
			if (connectionToParent != null)
				connectionToParent.Draw();
			//GUI.backgroundColor = bgColor;
			GUI.Box(rect, nodeName, currentStyle);
		}

		internal void Drag(Vector2 delta)
		{
			rect.position += delta;
		}

		/// <summary>
		/// Called from parent (CreateChildConnection())
		/// </summary>
		/// <param name="newParent"></param>
		internal void CreateConnectionToParent(CompositeNodeWindow newParent)
		{
			if (parent != null)
			{ // TODO: cleanup old connection
				throw new Exception("Must handle situation where child already has parent!");
			}

			parent = newParent;
			connectionToParent = new Connection(parent.outPoint, inPoint, OnClickRemoveConnection);
		}

		internal abstract bool ProcessEvents(Event e);

		protected void OnClickRemoveConnection(Connection connection)
		{
			parent.RemoveChildConnection(this);
			if (connection != connectionToParent)
			{
				throw new Exception("Huh? Connection and connectionToParent are not equal?");
			}

			connectionToParent = null;
			parent = null;
		}

		protected abstract void OnGUIExtra();
		/// <summary>
		/// Deprecate this in-favour of GUI.Box?
		/// </summary>
		/// <param name="id"></param>
		protected abstract void DrawWindow(int id);


		//protected void DrawNodeCurve(NodeWindow start, NodeWindow end)
		//{
		//	Vector3 startPos = new Vector3(start.rect.x + start.rect.width / 2, start.rect.y + start.rect.height, 0);
		//	Vector3 endPos = new Vector3(end.rect.x + end.rect.width / 2, end.rect.y, 0);
		//	Vector3 startTan = startPos + Vector3.right * 50;
		//	Vector3 endTan = endPos + Vector3.left * 50;
		//	Color shadowCol = new Color(0, 0, 0, 0.06f);
		//	for (int i = 0; i < 3; i++) // Draw a shadow
		//		Handles.DrawBezier(startPos, endPos, startTan, endTan, shadowCol, null, (i + 3) * 5);
		//	Handles.DrawBezier(startPos, endPos, startTan, endTan, Color.black, null, 3);
		//}
	}

	public class NodeStyle
	{
		public GUIStyle defaultStyle, selectedStyle;
		//public Rect rect;

		public NodeStyle(Texture2D normal, Texture2D selected)
		{
			defaultStyle = new GUIStyle();
			defaultStyle.normal.background = normal;
			defaultStyle.border = new RectOffset(12, 12, 12, 12);

			selectedStyle = new GUIStyle();
			selectedStyle.normal.background = selected;
			selectedStyle.border = new RectOffset(12, 12, 12, 12);

			//rect = new Rect(0, 0, defaultStyle.normal.background.width, defaultStyle.normal.background.height);
		}
	}
}