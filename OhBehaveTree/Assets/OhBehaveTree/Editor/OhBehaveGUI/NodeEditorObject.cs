using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AtomosZ.OhBehave.EditorTools
{
	/// <summary>
	/// Editor representation of an INode.
	/// </summary>
	[Serializable]
	public class NodeEditorObject
	{
		public NodeType nodeType;
		public int index;
		public int parentIndex = OhBehaveTreeBlueprint.NO_PARENT_INDEX;

		/// <summary>
		/// A nice, user-friendly display name.
		/// </summary>
		public string displayName;
		/// <summary>
		/// Pop-up info for this node.
		/// </summary>
		public string description;

		/// <summary>
		/// LeafNode Only.
		/// </summary>
		public MonoScript actionEvent;
		public Rect windowRect;


		/// <summary>
		/// Non-LeafNode Only.
		/// Decorators should only have 1 child.
		/// </summary>
		[SerializeField]
		private List<int> children;

		[NonSerialized]
		public Vector2 offset;

		private OhBehaveTreeBlueprint treeBlueprint;
		private NodeWindow window;
		/// <summary>
		/// Editor objects have a hard time serializing themselves.
		/// </summary>
		private NodeEditorObject parent;
		private bool isConnectedToRoot;

		public NodeEditorObject Parent
		{
			get
			{
				if (parent == null) // this is ALWAYS null! WHY???
				{
					if (treeBlueprint == null)
					{
						var ohBehave = EditorWindow.GetWindow<OhBehaveEditorWindow>();
						treeBlueprint = ohBehave.treeBlueprint;
					}
					parent = treeBlueprint.GetNodeObject(parentIndex);
				}

				return parent;
			}
		}


		public NodeEditorObject(NodeType type, int nodeIndex)
		{
			nodeType = type;
			index = nodeIndex;
			if (nodeIndex == OhBehaveTreeBlueprint.ROOT_INDEX)
			{
				isConnectedToRoot = true;
				parentIndex = OhBehaveTreeBlueprint.ROOT_NODE_PARENT_INDEX;
			}

			parent = Parent;
			CreateWindow();
		}


		/// <summary>
		/// Returns true if save needed.
		/// </summary>
		/// <param name="current"></param>
		/// <returns></returns>
		public bool ProcessEvents(Event current)
		{
			if (window == null)
			{
				CreateWindow();
			}

			return window.ProcessEvents(current);
		}

		public bool CheckIsValid(out InvalidNodeMessage invalidNodeMessage)
		{
			bool isValid;
			invalidNodeMessage.node = this;
			invalidNodeMessage.errorCode = InvalidNodeMessage.ErrorCode.Success.ToString();

			switch (nodeType)
			{
				case NodeType.Leaf:
					isValid = HasAction();
					if (!isValid)
					{
						invalidNodeMessage.errorCode = InvalidNodeMessage.ErrorCode.LeafActionNotSet.ToString();
					}
					break;
				case NodeType.Inverter:
					isValid = HasChildren() && children.Count == 1;
					if (!isValid)
					{
						invalidNodeMessage.errorCode = InvalidNodeMessage.ErrorCode.NoChildren.ToString();
					}
					break;
				default:
					isValid = HasChildren();
					if (!isValid)
					{
						invalidNodeMessage.errorCode = InvalidNodeMessage.ErrorCode.NoChildren.ToString();
					}
					break;
			}

			isConnectedToRoot = IsConnectedToRoot();
			if (!isConnectedToRoot)
				invalidNodeMessage.errorCode += " | " + InvalidNodeMessage.ErrorCode.NoConnectionToRoot.ToString();

			window.BranchBroken(isValid, isConnectedToRoot, invalidNodeMessage.errorCode);
			return isValid || !isConnectedToRoot;
		}


		public void DrawConnectionWires()
		{
			window.DrawConnectionWires();
		}

		public List<int> GetChildren()
		{
			return children;
		}

		public NodeWindow GetWindow()
		{
			if (window == null)
			{
				CreateWindow();
			}

			return window;
		}



		private bool IsConnectedToRoot()
		{
			if (Parent != null && (Parent.isConnectedToRoot || Parent.index == OhBehaveTreeBlueprint.ROOT_INDEX))
				return true;
			return index == OhBehaveTreeBlueprint.ROOT_INDEX;
		}

		private bool HasAction()
		{
			return actionEvent != null;
		}

		public void OnGUI()
		{
			if (window == null)
			{
				Debug.LogError("No window!");
				return;
			}

			window.OnGUI();
		}

		public void ChangeNodeType(NodeType newType)
		{
			if (index == OhBehaveTreeBlueprint.ROOT_INDEX)
			{
				Debug.LogWarning("Change denied: I am root");
				return;
			}

			if (HasChildren() && (newType == NodeType.Leaf || (newType == NodeType.Inverter && children.Count > 1)))
			{
				for (int i = children.Count - 1; i >= 0; --i)
					DisconnectNodes(this, treeBlueprint.GetNodeObject(children[i]));
			}

			nodeType = newType;
			CreateWindow();
		}


		public static void ConnectNodes(NodeEditorObject parent, NodeEditorObject child)
		{
			parent.AddChild(child);
			child.AddParent(parent.index);
		}

		public static void DisconnectNodes(NodeEditorObject parent, NodeEditorObject child)
		{
			child.RemoveParent();
			if (parent != null)
				parent.RemoveChild(child.index);
		}

		/// <summary>
		/// If node already has a parent, removes it first.
		/// </summary>
		/// <param name="newParentIndex"></param>
		private void AddParent(int newParentIndex)
		{
			if (parentIndex != OhBehaveTreeBlueprint.NO_PARENT_INDEX)
				window.ParentRemoved();
			parentIndex = newParentIndex;
			window.SetParentWindow((IParentNodeWindow)Parent.window);
		}

		private void RemoveParent()
		{
			if (parent != null)
			{
				//parent.RemoveChild(index);
				window.ParentRemoved();
				parent = null;
				parentIndex = OhBehaveTreeBlueprint.NO_PARENT_INDEX;
			}
		}

		/// <summary>
		/// Name was changed so should notify parent.
		/// </summary>
		public void RefreshParent()
		{
			if (parentIndex != OhBehaveTreeBlueprint.NO_PARENT_INDEX)
			{
				Parent.window.UpdateChildrenList();
				GUI.changed = true;
			}
		}



		public void NewChildOrder(int[] newChildOrder)
		{
			children.Clear();
			children.AddRange(newChildOrder);
			window.UpdateChildrenList();
		}


		private void AddChild(NodeEditorObject newChildNode)
		{
			if (children == null)
				children = new List<int>();
			else if (children.Contains(newChildNode.index))
			{
				Debug.LogError("Duplicate node index " + newChildNode.index + " found in " + displayName);
				return;
			}

			children.Add(newChildNode.index);
			window.UpdateChildrenList();
		}


		private void RemoveChild(int childIndex)
		{
			if (children == null)
			{
				Debug.LogError(displayName + " has no children");
				return;
			}

			if (!children.Remove(childIndex))
			{
				Debug.LogError(childIndex + " does not exist in " + displayName);
			}

			window.UpdateChildrenList();
		}

		/// <summary>
		/// Called when a node gets deleted to keep now orphaned nodes and parent node in sink.
		/// </summary>
		public void NotifyFamilyOfDelete()
		{
			if (parentIndex != OhBehaveTreeBlueprint.NO_PARENT_INDEX)
				Parent.RemoveChild(index);

			if (HasChildren())
			{
				// this node has children. Warn before deleting?
				var ohBehave = EditorWindow.GetWindow<OhBehaveEditorWindow>();
				var treeBlueprint = ohBehave.treeBlueprint;

				for (int i = children.Count - 1; i >= 0; --i)
				{
					NodeEditorObject child = treeBlueprint.GetNodeObject(children[i]);
					child.RemoveParent();
				}
			}
		}

		public void Offset(Vector2 contentOffset)
		{
			offset = contentOffset;
		}

		public bool HasChildren()
		{
			return children != null && children.Count != 0;
		}

		private void CreateWindow()
		{
			switch (nodeType)
			{
				case NodeType.Sequence:
					window = new SequenceNodeWindow(this);
					break;
				case NodeType.Selector:
					window = new SelectorNodeWindow(this);
					break;
				case NodeType.Leaf:
					window = new LeafNodeWindow(this);
					break;
				case NodeType.Inverter:
					window = new InverterNodeWindow(this);
					break;
				default:
					Debug.LogWarning("TODO: CreateWindow of type " + nodeType);
					break;
			}
		}

		public struct InvalidNodeMessage
		{
			public enum ErrorCode
			{
				Success,
				NoChildren,
				NoConnectionToRoot,
				LeafActionNotSet,
			};

			public NodeEditorObject node;
			public string errorCode;
		}
	}
}