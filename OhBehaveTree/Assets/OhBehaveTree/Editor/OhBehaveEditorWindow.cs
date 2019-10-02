using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AtomosZ.OhBehave.CustomEditors
{
	public class OhBehaveEditorWindow : EditorWindow
	{
		public static int NextWindowID = 0;

		[SerializeField]
		public OhBehaveStateMachineController stateMachine;
		public List<NodeWindow> parentlessNodes = new List<NodeWindow>();
		[SerializeField]
		private CompositeNodeWindow rootNodeWindow;
		private OhBehaveEditorWindow window;
		private Vector2 scrollPos;
		//private GUIStyle defaultStyle;
		//private GUIStyle selectedStyle;
		private GUIStyle inPointStyle;
		private GUIStyle outPointStyle;
		private ConnectionPoint selectedInPoint;
		private ConnectionPoint selectedOutPoint;

		//[MenuItem("Window/OhBehave")]
		//static public void ShowWindow()
		//{
		//	var window = EditorWindow.GetWindow<OhBehaveEditorWindow>();
		//	window.titleContent = new GUIContent("OhBehave!");
		//}

		private void OnEnable()
		{
			window = EditorWindow.GetWindow<OhBehaveEditorWindow>();
			window.titleContent = new GUIContent("OhBehave!");

			inPointStyle = new GUIStyle();
			inPointStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/radio.png") as Texture2D;
			inPointStyle.active.background = EditorGUIUtility.Load("builtin skins/darkskin/images/radio on.png") as Texture2D;
			//inPointStyle.border = new RectOffset(4, 4, 12, 12);

			outPointStyle = new GUIStyle();
			outPointStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/radio.png") as Texture2D;
			outPointStyle.active.background = EditorGUIUtility.Load("builtin skins/darkskin/images/radio on.png") as Texture2D;
			//outPointStyle.border = new RectOffset(4, 4, 12, 12);

		}

		public void Open(OhBehaveStateMachineController stateMachine)
		{

			switch (stateMachine.parentNode.GetNodeType())
			{
				case NodeType.Leaf:
					Debug.LogError("Cannot build a Behaviour tree on top of a leaf!");
					return;
				case NodeType.Selector:
					rootNodeWindow = new SelectorNodeWindow(
						null, (SelectorNode)stateMachine.parentNode,
						inPointStyle, outPointStyle,
						OnClickInPoint, OnClickOutPoint);
					break;
				case NodeType.Sequence:
					rootNodeWindow = new SequenceNodeWindow(
						null, (SequenceNode)stateMachine.parentNode,
						inPointStyle, outPointStyle,
						OnClickInPoint, OnClickOutPoint);
					break;
			}
		}

		internal NodeWindow CreateNewNodeWindow(CompositeNodeWindow parent, INode node)
		{
			switch (node.GetNodeType())
			{
				case NodeType.Leaf:
					return new LeafNodeWindow(parent, (LeafNode)node,
						inPointStyle,
						OnClickInPoint);
				case NodeType.Selector:
					return new SelectorNodeWindow(parent, (SelectorNode)node,
						inPointStyle, outPointStyle,
						OnClickInPoint, OnClickOutPoint);
				case NodeType.Sequence:
					return new SequenceNodeWindow(parent, (SequenceNode)node,
						inPointStyle, outPointStyle,
						OnClickInPoint, OnClickOutPoint);
				default:
					Debug.LogError("Was a NodeType not implemented?");
					return null;
			}
		}

		private void OnClickInPoint(ConnectionPoint inPoint)
		{
			selectedInPoint = inPoint;
			if (selectedOutPoint != null)
			{
				if (selectedOutPoint.node != selectedInPoint.node)
				{
					CreateConnection();
					ClearConnectionSelection();
				}
				else
				{
					ClearConnectionSelection();
				}
			}
		}

		private void OnClickOutPoint(ConnectionPoint outPoint)
		{
			selectedOutPoint = outPoint;

			if (selectedInPoint != null)
			{
				if (selectedOutPoint.node != selectedInPoint.node)
				{
					CreateConnection();
					ClearConnectionSelection();
				}
				else
				{
					ClearConnectionSelection();
				}
			}
		}

		private void CreateConnection()
		{
			((CompositeNodeWindow)selectedOutPoint.node).CreateChildConnection(selectedInPoint.node);
		}

		private void ClearConnectionSelection()
		{
			selectedInPoint = null;
			selectedOutPoint = null;
		}

		private void OnGUI()
		{
			if (rootNodeWindow == null)
			{
				if (Selection.activeGameObject == null)
					return;
				var ai = Selection.activeGameObject.GetComponent<AIOhBehave>();
				if (ai == null || ai.ai == null)
				{
					return;
				}

				Open(ai.ai);
				return;
			}

			//BeginWindows();
			rootNodeWindow.OnGUI();
			//EndWindows();

			ProcessEvents(Event.current);
			if (GUI.changed)
				Repaint();
		}


		private void ProcessEvents(Event e)
		{
			if (rootNodeWindow != null)
			{
				rootNodeWindow.ProcessEvents(e);
			}

			switch (e.type)
			{
				case EventType.MouseDown:
					if (e.button == 1)
					{
						ProcessContextMenu(e.mousePosition);
					}
					break;
			}
		}

		private void ProcessContextMenu(Vector2 mousePosition)
		{
			GenericMenu genericMenu = new GenericMenu();
			genericMenu.AddItem(new GUIContent("Add Node"), false, () => OnClickAddNode(mousePosition));
			genericMenu.ShowAsContext();
		}

		private void OnClickAddNode(Vector2 mousePos)
		{
			if (rootNodeWindow == null)
			{
				Debug.Log("No root: cannot create nodes");
			}
			else
			{
				rootNodeWindow.CreateChildNode(NodeType.Selector);
			}
		}
	}
}