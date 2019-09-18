using System;

namespace AtomosZ.OhBehave
{
	public enum NodeType { Selector, Sequence, Leaf }
	public enum NodeState { Failure, Success, Running }

	public interface INode
	{
		NodeType GetNodeType();
		NodeState GetNodeState();
		NodeState Evaluate();
	}
}