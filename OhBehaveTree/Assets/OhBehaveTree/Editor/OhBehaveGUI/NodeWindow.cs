using System;
using UnityEditor;
using UnityEngine;

namespace AtomosZ.OhBehave.EditorTools
{
	public abstract class NodeWindow
	{
		protected const float TITLEBAR_OFFSET = 15;
		protected static Texture invalidTexture = EditorGUIUtility.FindTexture("Assets/OhBehaveTree/Editor/Node Broken Branch.png");
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
		protected GUIStyle labelStyle;

		public IParentNodeWindow parent;
		public Connection connectionToParent;
		protected OhBehaveTreeBlueprint treeBlueprint;
		protected bool isDragged;
		protected bool isSelected;
		/// <summary>
		/// If a child node is constructed before the parent the connection can
		/// not be constructed, so hold off on the construction until it's possible.
		/// </summary>
		protected bool refreshConnection;
		private double timeClicked = double.MinValue;
		private bool isValid;

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
					bgColor = NodeStyle.LeafColor;
					labelStyle = NodeStyle.LeafLabelStyle;
					break;
				case NodeType.Selector:
					nodeStyle = OhBehaveEditorWindow.SelectorNodeStyle;
					bgColor = NodeStyle.SelectorColor;
					labelStyle = NodeStyle.SelectorLabelStyle;
					outPoint = new ConnectionPoint(this, ConnectionPointType.Out, ConnectionControls.OnClickOutPoint);
					break;
				case NodeType.Sequence:
					nodeStyle = OhBehaveEditorWindow.SequenceNodeStyle;
					bgColor = NodeStyle.SequenceColor;
					labelStyle = NodeStyle.SequencerLabelStyle;
					outPoint = new ConnectionPoint(this, ConnectionPointType.Out, ConnectionControls.OnClickOutPoint);
					break;
				case NodeType.Inverter:
					nodeStyle = OhBehaveEditorWindow.InverterNodeStyle;
					bgColor = NodeStyle.InverterColor;
					labelStyle = NodeStyle.InverterLabelStyle;
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
				bgColor = NodeStyle.RootColor;
			}
		}


		public abstract bool ProcessEvents(Event e);
		public abstract void OnGUI();
		/// <summary>
		/// Keep list of children update-to-date. Used by Composite Nodes.
		/// </summary>
		public abstract void UpdateChildrenList();

		public void Deselect()
		{
			isSelected = false;
			labelStyle.normal.textColor = Color.black;
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

		public void BranchBroken(bool isFine)
		{
			isValid = isFine;
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
				labelStyle.normal.textColor = Color.white;
				treeBlueprint.SelectNode(nodeObject);
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



		protected void CreateTitleBar()
		{
			if (isValid)
				GUILayout.Space(TITLEBAR_OFFSET);
			else
				GUILayout.Label(invalidTexture);
			GUILayout.BeginHorizontal();
			{
				GUILayout.Label(
					new GUIContent(nodeObject.displayName + " - " + Enum.GetName(typeof(NodeType),
						nodeObject.nodeType)),
					labelStyle
				);

				//if (GUILayout.Button("Edit", NodeStyle.LeafLabelStyle))
				//{
				//	NodeEditPopup.Init(nodeObject);
				//}
			}
			GUILayout.EndHorizontal();
		}

		protected Rect TitleLabelRect()
		{
			Rect rect = GetRect();
			rect.height = EditorGUIUtility.singleLineHeight + TITLEBAR_OFFSET;
			return rect;
		}

		protected void Drag(Vector2 delta)
		{
			nodeObject.windowRect.position += delta;
		}


		/// <summary>
		/// TODO: This will need to be re-thunk to accomodate windows being total slaves to the NodeEditorObjects.
		/// </summary>
		/// <param name="connection"></param>
		protected void OnClickRemoveConnection(Connection connection)
		{
			//parent.RemoveChildConnection(this); // this probably should not be called here
			if (connection != connectionToParent)
			{
				throw new Exception("Huh? Connection and connectionToParent are not equal?");
			}

			connectionToParent = null;
			parent = null;
		}


	}
}