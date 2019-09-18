using UnityEngine;

namespace AtomosZ.OhBehave
{
	public class LeafNode : ScriptableObject, INode
	{
		protected NodeType nodeType = NodeType.Leaf;
		protected NodeState nodeState;


		public NodeType GetNodeType()
		{
			return nodeType;
		}

		public NodeState GetNodeState()
		{
			return nodeState;
		}

		public NodeState Evaluate()
		{
			throw new System.NotImplementedException();
		}
	}
}