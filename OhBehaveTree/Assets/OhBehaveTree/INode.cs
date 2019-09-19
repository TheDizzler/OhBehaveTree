using System;
using UnityEngine;

namespace AtomosZ.OhBehave
{
	public enum NodeType { Selector, Sequence, Leaf }
	public enum NodeState { Failure, Success, Running }

	public abstract class INode : ScriptableObject
	{
		public INode parent;
		protected NodeType nodeType;
		protected NodeState nodeState;

		public NodeType GetNodeType() { return nodeType; }
		public NodeState GetNodeState() { return nodeState; }
		public abstract NodeState Evaluate();
	}
}