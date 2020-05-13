using System;

namespace AtomosZ.OhBehave
{
	/// <summary>
	/// Basically just for reference.
	/// This is probably not needed and just increases amount of bookkeeping required.
	/// </summary>
	public enum NodeType { Selector, Sequence, Inverter, Leaf }
	public enum NodeState { Failure, Success, Running }

	[Serializable]
	public abstract class INode
	{
		public IParentNode parent;

		public NodeType nodeType { get; protected set; }
		public NodeState nodeState { get; protected set; }

		/// <summary>
		/// The way currently implemented, only one leaf node can be evaluated per Update,
		/// insteading immediately falling through to the next leaf if that one is invalid.
		/// </summary>
		/// <returns></returns>
		public abstract INode Init();
		public abstract NodeState Evaluate();
		public abstract INode Exit();
	}
}