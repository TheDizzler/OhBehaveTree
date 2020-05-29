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


		public void ChangeNodeType(NodeType newType)
		{
			Debug.Log("newType: " + newType);

			if (newType == NodeType.Leaf)
			{
				if (children != null && children.Count != 0)
				{
					Debug.LogError("Must handle children potentially losing their parent!");
				}
			}

			nodeType = newType;
			CreateWindow();
		}


		public bool ParentDeleted(int deletedParentIndex)
		{
			if (deletedParentIndex != parentIndex)
			{
				Debug.LogError(displayName + ": It was reported that "
					+ deletedParentIndex + " was my parent but my parent is " + parentIndex);
				return false;
			}

			window.ParentDeleted();
			parent = null;
			return true;
		}

		/// <summary>
		/// Name was changed so should notify parent.
		/// </summary>
		public void RefreshParent()
		{
			if (parentIndex != OhBehaveTreeBlueprint.NO_PARENT_INDEX)
			{
				Parent.window.UpdateChildren();
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
			window.UpdateChildren();
		}



		public void ChildDeleted(int childIndex)
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

			window.UpdateChildren();
		}


		public void ProcessEvents(Event current)
		{
			if (window == null)
			{
				CreateWindow();
			}

			window.ProcessEvents(current);
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


		private void CreateWindow()
		{
			switch (nodeType)
			{
				case NodeType.Sequence:
					window = new SequenceNodeWindow(this);
					break;
				case NodeType.Leaf:
					window = new LeafNodeWindow(this);
					break;
				case NodeType.Inverter:
				case NodeType.Selector:
					Debug.LogWarning("TODO: CreateWindow of type " + nodeType);
					break;
			}
		}

		public void NotifyChildrenOfDelete()
		{
			if (children == null)
				return;
			if (children.Count > 0)
			{ // this node has children. Warn before deleting?
				var ohBehave = EditorWindow.GetWindow<OhBehaveEditorWindow>();
				var treeBlueprint = ohBehave.treeBlueprint;
				foreach (int nodeIndex in children)
				{
					NodeEditorObject child = treeBlueprint.GetNodeObject(nodeIndex);
					if (child.ParentDeleted(index))
						child.parentIndex = OhBehaveTreeBlueprint.NO_PARENT_INDEX;
				}
			}
		}
	}
}