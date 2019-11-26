using System;
using UnityEditor;
using UnityEngine;
using static AtomosZ.OhBehave.CustomEditors.NodeTreeEditor;

namespace AtomosZ.OhBehave.CustomEditors
{
	public abstract class NodeWindow : ScriptableObject
	{
		static protected Color rootColor = new Color(1, .65f, 1, .75f);
		static protected Color selectorColor = new Color(1, .65f, 0, .75f);
		static protected Color leafColor = new Color(0, 1, 0, .75f);
		static protected Color sequenceColor = new Color(1, .92f, .016f, .75f);

		public INode nodeObject;
		private NodeWindowData windowData;
		public Rect rect;
		public ConnectionPoint inPoint;
		public ConnectionPoint outPoint;
		public NodeStyle nodeStyle;

		protected string nodeName;
		protected GUIStyle currentStyle;
		protected Color bgColor;
		/// <summary>
		/// Warning: if this is null it is the topmost node.
		/// </summary>
		protected CompositeNodeWindow parent;
		protected Connection connectionToParent;
		protected NodeTreeEditor nodeTreeEditor;
		protected bool isDragged;
		protected bool isSelected;


		internal NodeWindow Create(INode nodeObj, NodeWindowData data)
		{
			var ohBehave = EditorWindow.GetWindow<OhBehaveEditorWindow>();
			nodeTreeEditor = ohBehave.nodeTree;
			nodeObject = nodeObj;
			windowData = data;
			parent = data.parentWindow;

			nodeName = data.name;
			rect = data.rect;

			switch (nodeObj.GetNodeType())
			{
				case NodeType.Leaf:
					nodeStyle = ohBehave.LeafNodeStyle;
					bgColor = leafColor;
					break;
				case NodeType.Selector:
					nodeStyle = ohBehave.selectorNodeStyle;
					bgColor = selectorColor;
					outPoint = new ConnectionPoint(this, ConnectionPointType.Out, OnClickOutPoint);
					break;
				case NodeType.Sequence:
					nodeStyle = ohBehave.sequenceNodeStyle;
					bgColor = sequenceColor;
					outPoint = new ConnectionPoint(this, ConnectionPointType.Out, OnClickOutPoint);
					break;
			}

			currentStyle = nodeStyle.defaultStyle;

			if (parent != null)
			{
				inPoint = new ConnectionPoint(this, ConnectionPointType.In, OnClickInPoint);
				connectionToParent = new Connection(parent.outPoint, inPoint, OnClickRemoveConnection);
			}
			else
			{
				bgColor = rootColor;
			}

			return this;
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
			windowData.rect = rect;
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
	}


}