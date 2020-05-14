using UnityEngine;

namespace AtomosZ.OhBehave.EditorTools
{
	/// <summary>
	/// Editor representation of an INode.
	/// </summary>
	[System.Serializable]
	public class NodeEditorObject : Object
	{
		public NodeEditorObject parent;
		public NodeType nodeType;

		public string displayName;
		/// <summary>
		/// Pop-up info for this node.
		/// </summary>
		public string description;
		//public LeafNodeAction startEvent;
		//public LeafNodeAction actionEvent;
	}
}