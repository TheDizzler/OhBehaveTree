using UnityEngine;

namespace AtomosZ.OhBehave
{
	public class LeafNode : INode
	{
		public delegate NodeState InitializeNodeDelegate();
		public delegate NodeState ActionNodeDelegate();
		public InitializeNodeDelegate initialize;
		public ActionNodeDelegate action;


		public LeafNode()
		{
			nodeType = NodeType.Leaf;
		}


		/// <summary>
		/// The way currently implemented, only one leaf node can be evaluated per Update,
		/// insteading immediately falling through to the next leaf if that one is invalid.
		/// </summary>
		/// <returns></returns>
		public override INode Init()
		{
			nodeState = initialize();
			// do something here to check if it's immediately obvious that this action will fail.
			// If will fail,
			//		nodeState = NodeState.Failure;
			//		return Exit();
			// above will cause recurssion, especially if a long chain were to fail.
			return this;
		}

		/// <summary>
		/// If nodeState is anything except Running, then return control to the parent.
		/// </summary>
		/// <returns></returns>
		public override NodeState Evaluate()
		{
			nodeState = action();
			return nodeState;
		}


		public override INode Exit()
		{
			return parent.ChildFinished(nodeState);
		}
	}
}