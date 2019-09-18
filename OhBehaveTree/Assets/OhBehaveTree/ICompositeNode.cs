using UnityEngine;

namespace AtomosZ.OhBehave
{
	public abstract class ICompositeNode : ScriptableObject, INode
	{
		protected NodeType nodeType;
		protected NodeState nodeState;

		public NodeType GetNodeType() { return nodeType; }
		public NodeState GetNodeState() { return nodeState; }
		public abstract NodeState Evaluate();
	}
}
