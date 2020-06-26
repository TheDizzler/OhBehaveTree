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
		public int parentIndex;

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
		public LeafNodeAction startEvent;
		/// <summary>
		/// LeafNode Only.
		/// </summary>
		public LeafNodeAction actionEvent;
		public Rect windowRect;

		[NonSerialized]
		public NodeWindow window;

		/// <summary>
		/// Non-LeafNode Only.
		/// Decorators should only have 1 child.
		/// </summary>
		[SerializeField]
		public List<int> children;

		[NonSerialized]
		public Vector2 offset;
		/// <summary>
		/// Editor objects have a hard time serializing themselves.
		/// </summary>
		private NodeEditorObject parent;



		public NodeEditorObject Parent
		{
			get
			{
				if (parent == null)
				{
					var ohBehave = EditorWindow.GetWindow<OhBehaveEditorWindow>();
					var treeBlueprint = ohBehave.treeBlueprint;
					parent = treeBlueprint.GetNodeObject(parentIndex);
				}

				return parent;
			}
		}


		public NodeEditorObject(NodeType type, int nodeIndex, int parentNodeObjectIndex)
		{
			nodeType = type;
			index = nodeIndex;
			parentIndex = parentNodeObjectIndex;
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

		public bool CheckIsValid()
		{
			bool isValid;

			switch (nodeType)
			{
				case NodeType.Leaf:
					isValid = HasAction();
					break;
				case NodeType.Inverter:
					isValid = HasChildren() && children.Count == 1;
					break;
				default:
					isValid = HasChildren();
					break;
			}

			window.BranchBroken(isValid);
			return isValid;
		}

		private bool HasAction()
		{
			return startEvent != null && startEvent.GetPersistentEventCount() != 0 
				&& actionEvent != null &&  actionEvent.GetPersistentEventCount() != 0;
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
			if (newType == NodeType.Leaf)
			{
				if (HasChildren())
				{
					Debug.LogError("Must handle children potentially losing their parent!");
				}
			}

			nodeType = newType;
			CreateWindow();
		}

		/// <summary>
		/// If node already has a parent, removes it first.
		/// </summary>
		/// <param name="newParentIndex"></param>
		public void AddParent(int newParentIndex)
		{
			if (parentIndex != OhBehaveTreeBlueprint.NO_PARENT_INDEX)
				window.ParentRemoved();
			parentIndex = newParentIndex;
			window.CreateConnectionToParent((IParentNodeWindow)Parent.window);
		}

		public void RemoveParent()
		{
			window.ParentRemoved();
			parent = null;
			parentIndex = OhBehaveTreeBlueprint.NO_PARENT_INDEX;
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


		public void AddChild(NodeEditorObject newChildNode)
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


		public void RemoveChild(int childIndex)
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
				foreach (int nodeIndex in children)
				{
					NodeEditorObject child = treeBlueprint.GetNodeObject(nodeIndex);
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
	}
}