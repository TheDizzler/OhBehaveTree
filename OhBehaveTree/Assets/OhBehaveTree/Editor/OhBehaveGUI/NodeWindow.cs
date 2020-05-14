using System;
using UnityEditor;
using UnityEngine;

namespace AtomosZ.OhBehave.EditorTools
{
	public abstract class NodeWindow
	{
		static protected Color rootColor = new Color(1, .65f, 1, .75f);
		static protected Color selectorColor = new Color(1, .65f, 0, .75f);
		static protected Color leafColor = new Color(0, 1, 0, .75f);
		static protected Color sequenceColor = new Color(1, .92f, .016f, .75f);

		/// <summary>
		/// This should contain all the data needed for the game.
		/// Everything else is editor visual representation stuff only.
		/// </summary>
		public NodeEditorObject nodeObject;

		public Rect rect;
		public ConnectionPoint inPoint;
		public ConnectionPoint outPoint;
		public NodeStyle nodeStyle;


		protected string nodeName;
		protected GUIStyle currentStyle;
		protected Color bgColor;

		protected IParentNodeWindow parent;
		protected Connection connectionToParent;
		protected OhBehaveTreeBlueprint treeBlueprint;
		protected bool isDragged;
		protected bool isSelected;


		public NodeWindow(NodeEditorObject nodeObj)
		{
			var ohBehave = EditorWindow.GetWindow<OhBehaveEditorWindow>();
			treeBlueprint = ohBehave.treeBlueprint;
			//nodeObject = nodeObj;
			//windowData = data;
			//parent = nodeObj.parent;

			nodeName = nodeObj.name;
			//rect = data.rect;

			switch (nodeObj.nodeType)
			{
				case NodeType.Leaf:
					nodeStyle = OhBehaveEditorWindow.LeafNodeStyle;
					bgColor = leafColor;
					break;
				case NodeType.Selector:
					nodeStyle = OhBehaveEditorWindow.selectorNodeStyle;
					bgColor = selectorColor;
					outPoint = new ConnectionPoint(this, ConnectionPointType.Out, ConnectionControls.OnClickOutPoint);
					break;
				case NodeType.Sequence:
					nodeStyle = OhBehaveEditorWindow.sequenceNodeStyle;
					bgColor = sequenceColor;
					outPoint = new ConnectionPoint(this, ConnectionPointType.Out, ConnectionControls.OnClickOutPoint);
					break;
			}

			currentStyle = nodeStyle.defaultStyle;

			if (parent != null)
			{
				inPoint = new ConnectionPoint(this, ConnectionPointType.In, ConnectionControls.OnClickInPoint);
				connectionToParent = new Connection(((NodeWindow)parent).outPoint, inPoint, OnClickRemoveConnection);
			}
			else
			{
				bgColor = rootColor;
			}
		}


		internal void OnGUI()
		{
			if (inPoint != null)
				inPoint.Draw();
			if (outPoint != null)
				outPoint.Draw();

			if (connectionToParent != null)
				connectionToParent.Draw();
			GUI.Box(rect, nodeName, currentStyle);
		}

		internal void Drag(Vector2 delta)
		{
			rect.position += delta;
			//windowData.rect = rect;
		}

		/// <summary>
		/// Called from parent (CreateChildConnection())
		/// </summary>
		/// <param name="newParent"></param>
		internal void CreateConnectionToParent(IParentNodeWindow newParent)
		{
			if (parent != null)
			{ // TODO: cleanup old connection
				throw new Exception("Must handle situation where child already has parent!");
			}

			parent = newParent;
			connectionToParent = new Connection(((NodeWindow)parent).outPoint, inPoint, OnClickRemoveConnection);
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
	}


}