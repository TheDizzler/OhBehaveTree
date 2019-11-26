using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;


namespace AtomosZ.OhBehave.CustomEditors
{
	public class NodeTreeEditor : ScriptableObject
	{
		public OhBehaveStateMachineController aiStateMachine = null;
		public NodeWindow rootNode = null;

		private Dictionary<INode, NodeWindowData> nodeDict = new Dictionary<INode, NodeWindowData>();


		private static ConnectionPoint selectedInPoint;
		private static ConnectionPoint selectedOutPoint;


		internal class NodeWindowData
		{
			public INode iNode;
			public NodeWindow window;
			public CompositeNodeWindow parentWindow;
			/// <summary>
			/// Position to place editor object in editor window.
			/// </summary>
			public Rect rect;
			/// <summary>
			/// Name to display in editor window.
			/// </summary>
			public string name;
		}



		internal void ConstructTree()
		{
			var ohBehave = EditorWindow.GetWindow<OhBehaveEditorWindow>();
			ICompositeNode root = aiStateMachine.parentNode;
			NodeWindowData parentData;
			if (!nodeDict.TryGetValue(root, out parentData))
			{
				// This is the likely the first time this behaviour tree has been accessed.
				parentData = new NodeWindowData();
				parentData.iNode = root;
				parentData.name = "Root";
				parentData.rect = new Rect(ohBehave.position.width / 2, 0,
					ohBehave.selectorNodeStyle.defaultStyle.normal.background.width * 4,
					ohBehave.selectorNodeStyle.defaultStyle.normal.background.height);

				nodeDict.Add(root, parentData);

				CompositeNodeWindow window = (CompositeNodeWindow)CreateNewNodeWindow(root, parentData);
				FillDictWithChildren(root);

			}
		}


		internal void ProcessContextMenu(CompositeNodeWindow parentNodeWindow)
		{
			GenericMenu genericMenu = new GenericMenu();
			genericMenu.AddItem(new GUIContent("Add Leaf"), false, () => CreateChildNode(parentNodeWindow, NodeType.Leaf));
			genericMenu.AddItem(new GUIContent("Add Sequence"), false, () => CreateChildNode(parentNodeWindow, NodeType.Sequence));
			genericMenu.AddItem(new GUIContent("Add Selector"), false, () => CreateChildNode(parentNodeWindow, NodeType.Selector));
			genericMenu.ShowAsContext();
		}

		/// <summary>
		/// Creates a user generated child node for this node.
		/// </summary>
		/// <param name="type"></param>
		private void CreateChildNode(CompositeNodeWindow parentNodeWindow, NodeType type)
		{
			INode newnode = CreateNewNode(type, parentNodeWindow.nodeObject);
			if (newnode == null)
				return;
			var ohBehave = EditorWindow.GetWindow<OhBehaveEditorWindow>();
			NodeWindowData newdata = new NodeWindowData();
			newdata.iNode = newnode;
			newdata.name = type.ToString();
			newdata.parentWindow = parentNodeWindow;
			newdata.rect = new Rect(parentNodeWindow.rect.position.x, parentNodeWindow.rect.position.y + 50,
				ohBehave.selectorNodeStyle.defaultStyle.normal.background.width * 4,
				ohBehave.selectorNodeStyle.defaultStyle.normal.background.height);
			nodeDict.Add(newnode, newdata);
			CreateNewNodeWindow(newnode, newdata);

		}

		private INode CreateNewNode(NodeType type, INode parentNodeObject)
		{
			INode node;
			switch (type)
			{
				case NodeType.Leaf:
					node = (LeafNode)ScriptableObject.CreateInstance(typeof(LeafNode));
					break;
				case NodeType.Selector:
					node = (SelectorNode)ScriptableObject.CreateInstance(typeof(SelectorNode));
					break;
				case NodeType.Sequence:
					node = (SequenceNode)ScriptableObject.CreateInstance(typeof(SequenceNode));
					break;
				default:
					Debug.LogError("Could not create node of type " + type);
					return null;
			}

			var dir = Path.GetDirectoryName(AssetDatabase.GetAssetPath(parentNodeObject));
			string nodename = "New" + type;
			int num = AssetDatabase.FindAssets(nodename, new string[] { dir }).Length;
			if (num != 0)
			{
				nodename += " (" + num + ")";
			}

			var path = EditorUtility.SaveFilePanelInProject(
				"Create New Node Root", nodename, "asset", "Where to save node?", dir);

			if (path.Length != 0)
			{
				AssetDatabase.CreateAsset(node, path);
				node.parent = parentNodeObject;
				((ICompositeNode)parentNodeObject).children.Add(node);
				EditorUtility.SetDirty(node);
				EditorUtility.SetDirty(parentNodeObject);
				return node;
			}

			return null;
		}

		private void FillDictWithChildren(ICompositeNode parentNode)
		{
			var ohBehave = EditorWindow.GetWindow<OhBehaveEditorWindow>();
			int width = ohBehave.selectorNodeStyle.defaultStyle.normal.background.width * 4;
			int height = ohBehave.selectorNodeStyle.defaultStyle.normal.background.height;

			CompositeNodeWindow parent = (CompositeNodeWindow)nodeDict[parentNode].window;
			Vector2 childPos = parent.rect.position;
			childPos.x -= (parentNode.children.Count - 1) * .5f * (width + 25);
			childPos.y += height + 50;
			foreach (INode child in parentNode.children)
			{
				NodeWindowData nodeData = new NodeWindowData();
				nodeData.iNode = child;
				nodeData.name = child.GetNodeType().ToString();
				nodeData.parentWindow = parent;
				nodeData.rect = new Rect(childPos.x, childPos.y,
					width, height);
				CreateNewNodeWindow(child, nodeData);
				nodeDict.Add(child, nodeData);
				childPos.x += width + 25;

				if (child.GetNodeType() != NodeType.Leaf)
					FillDictWithChildren((ICompositeNode)child);
			}
		}

		internal NodeWindow CreateNewNodeWindow(INode node, NodeWindowData nodeData = null)
		{
			if (nodeData == null)
				nodeData = nodeDict[node];

			NodeWindow newWindow;
			switch (node.GetNodeType())
			{
				case NodeType.Leaf:
					newWindow = ScriptableObject.CreateInstance<LeafNodeWindow>().Create(
						node, nodeData);
					break;
				case NodeType.Selector:
					newWindow = ScriptableObject.CreateInstance<SelectorNodeWindow>().Create(
						node, nodeData);
					break;
				case NodeType.Sequence:
					newWindow = ScriptableObject.CreateInstance<SequenceNodeWindow>().Create(
						node, nodeData);
					break;
				default:
					throw new Exception("Was a NodeType not implemented?");
			}

			nodeData.window = newWindow;
			return newWindow;
		}


		internal void OnGui(Event current)
		{
			//rootNode.OnGUI();
			foreach (KeyValuePair<INode, NodeWindowData> kvp in nodeDict)
			{
				kvp.Value.window.OnGUI();
				kvp.Value.window.ProcessEvents(current);
			}
		}


		private void ProcessEvents(Event e)
		{
			if (rootNode != null)
			{
				rootNode.ProcessEvents(e);
			}
		}

		internal static void OnClickInPoint(ConnectionPoint inPoint)
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

		internal static void OnClickOutPoint(ConnectionPoint outPoint)
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

		private static void CreateConnection()
		{
			((CompositeNodeWindow)selectedOutPoint.node).CreateChildConnection(selectedInPoint.node);
		}

		private static void ClearConnectionSelection()
		{
			selectedInPoint = null;
			selectedOutPoint = null;
		}
	}

	public class NodeStyle
	{
		public GUIStyle defaultStyle, selectedStyle;

		internal void Init(Texture2D normal, Texture2D selected)
		{
			defaultStyle = new GUIStyle();
			defaultStyle.normal.background = normal;
			defaultStyle.border = new RectOffset(12, 12, 12, 12);
			defaultStyle.alignment = TextAnchor.UpperCenter;

			selectedStyle = new GUIStyle();
			selectedStyle.normal.background = selected;
			selectedStyle.border = new RectOffset(12, 12, 12, 12);
			selectedStyle.alignment = TextAnchor.UpperCenter;
		}
	}
}
