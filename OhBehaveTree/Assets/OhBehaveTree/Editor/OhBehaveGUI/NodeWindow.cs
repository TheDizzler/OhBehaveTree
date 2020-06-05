using System;
using UnityEditor;
using UnityEngine;

namespace AtomosZ.OhBehave.EditorTools
{
	public abstract class NodeWindow
	{
		public static float DoubleClickTime = .3f;

		/// <summary>
		/// This should contain all the data needed for the game.
		/// Everything else is editor visual representation stuff only.
		/// </summary>
		public NodeEditorObject nodeObject;

		public ConnectionPoint inPoint;
		public ConnectionPoint outPoint;
		public NodeStyle nodeStyle;


		protected string nodeName;
		protected GUIStyle currentStyle;
		protected Color bgColor;

		public IParentNodeWindow parent;
		public Connection connectionToParent;
		protected OhBehaveTreeBlueprint treeBlueprint;
		protected bool isDragged;
		protected bool isSelected;
		protected bool refreshConnection;
		private double timeClicked = double.MinValue;
		

		public NodeWindow(NodeEditorObject nodeObj)
		{
			var ohBehave = EditorWindow.GetWindow<OhBehaveEditorWindow>();
			treeBlueprint = ohBehave.treeBlueprint;
			nodeObject = nodeObj;
			nodeName = nodeObj.displayName;

			switch (nodeObj.nodeType)
			{
				case NodeType.Leaf:
					nodeStyle = OhBehaveEditorWindow.LeafNodeStyle;
					bgColor = NodeStyle.leafColor;
					break;
				case NodeType.Selector:
					nodeStyle = OhBehaveEditorWindow.SelectorNodeStyle;
					bgColor = NodeStyle.selectorColor;
					outPoint = new ConnectionPoint(this, ConnectionPointType.Out, ConnectionControls.OnClickOutPoint);
					break;
				case NodeType.Sequence:
					nodeStyle = OhBehaveEditorWindow.SequenceNodeStyle;
					bgColor = NodeStyle.sequenceColor;
					outPoint = new ConnectionPoint(this, ConnectionPointType.Out, ConnectionControls.OnClickOutPoint);
					break;
			}

			currentStyle = nodeStyle.defaultStyle;

			if (nodeObject.index != OhBehaveTreeBlueprint.ROOT_INDEX)
				inPoint = new ConnectionPoint(this, ConnectionPointType.In, ConnectionControls.OnClickInPoint);

			NodeEditorObject prntObj = nodeObject.Parent;
			if (prntObj != null)
			{
				parent = (IParentNodeWindow)prntObj.window;
				if (parent != null)
					connectionToParent = new Connection(((NodeWindow)parent).outPoint, inPoint, OnClickRemoveConnection);
				else
					refreshConnection = true;
			}
			else
			{
				bgColor = NodeStyle.rootColor;
			}
		}

		public Rect GetRect()
		{
			Rect rect = nodeObject.windowRect;
			rect.position -= nodeObject.offset;
			return rect;
		}

		public Rect GetRectNoOffset()
		{
			return nodeObject.windowRect;
		}

		public void ParentRemoved()
		{
			OnClickRemoveConnection(connectionToParent);
		}

		protected void RefreshConnection()
		{
			NodeEditorObject prntObj = nodeObject.Parent;
			if (prntObj != null)
			{
				parent = (IParentNodeWindow)prntObj.window;
				if (parent != null)
				{
					connectionToParent = new Connection(((NodeWindow)parent).outPoint, inPoint, OnClickRemoveConnection);
					refreshConnection = false;
				}
			}
		}

		protected void LeftClick(Event e)
		{
			if (TitleLabelRect().Contains(e.mousePosition))
			{
				if (EditorApplication.timeSinceStartup - timeClicked <= DoubleClickTime)
				{
					timeClicked = double.MinValue;
					isDragged = false;

					NodeEditPopup.Init(nodeObject);
					return;
				}


				timeClicked = EditorApplication.timeSinceStartup;
				isDragged = true;
				GUI.changed = true;
				isSelected = true;
				treeBlueprint.selectedNode = nodeObject;
				currentStyle = nodeStyle.selectedStyle;
				Selection.SetActiveObjectWithContext(treeBlueprint, null);
				e.Use();
			}
			else if (GetRect().Contains(e.mousePosition))
			{
				GUI.changed = true;
				isSelected = true;
				labelStyle.normal.textColor = Color.white;
				treeBlueprint.SelectNode(nodeObject);
				currentStyle = nodeStyle.selectedStyle;
				Selection.SetActiveObjectWithContext(treeBlueprint, null);
				//e.Use();
			}
			else
			{
				GUI.changed = true;
				Deselect();
				if (treeBlueprint.GetSelectedNode() == nodeObject)
					treeBlueprint.SelectNode(null);
				currentStyle = nodeStyle.defaultStyle;
			}
		}

		public abstract void UpdateChildren();

		public void Deselect()
		{
			isSelected = false;
			labelStyle.normal.textColor = Color.black;
		}

		protected Rect TitleLabelRect()
		{
			Rect rect = GetRect();
			rect.height = EditorGUIUtility.singleLineHeight;
			return rect;
		}

		public abstract bool ProcessEvents(Event e);

		public abstract void OnGUI();


		protected void Drag(Vector2 delta)
		{
			nodeObject.windowRect.position += delta;
		}

		/// <summary>
		/// TODO: This will need to be re-thunk to accomodate windows being total slaves to the NodeEditorObjects.
		/// Called from parent (CreateChildConnection())
		/// </summary>
		/// <param name="newParent"></param>
		public void CreateConnectionToParent(IParentNodeWindow newParent)
		{
			if (parent != null)
			{ // TODO: cleanup old connection
				throw new Exception("Must handle situation where child already has parent!");
			}

			parent = newParent;
			connectionToParent = new Connection(((NodeWindow)parent).outPoint, inPoint, OnClickRemoveConnection);
		}


		/// <summary>
		/// TODO: This will need to be re-thunk to accomodate windows being total slaves to the NodeEditorObjects.
		/// </summary>
		/// <param name="connection"></param>
		protected void OnClickRemoveConnection(Connection connection)
		{
			parent.RemoveChildConnection(this); // this probably should not be called here
			if (connection != connectionToParent)
			{
				throw new Exception("Huh? Connection and connectionToParent are not equal?");
			}

			connectionToParent = null;
			parent = null;
		}


	}
}