using System;

namespace AtomosZ.OhBehave
{
	public enum NodeState { Failure, Success, Running }

	public interface INode
	{
		NodeState GetNodeState();
		NodeState Evaluate();
	}
}