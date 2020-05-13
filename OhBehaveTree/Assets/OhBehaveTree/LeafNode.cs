using System;
using UnityEngine.Events;

namespace AtomosZ.OhBehave
{
	[Serializable]
	public class LeafNodeAction : UnityEvent<LeafNode> { }

	public class LeafNode : INode
	{
		public delegate NodeState InitializeNodeDelegate();
		/// <summary>
		/// This action MUST set the state for node.
		/// </summary>
		public LeafNodeAction action = new LeafNodeAction();
		public LeafNodeAction initialize = new LeafNodeAction();


		public LeafNode()
		{
			nodeType = NodeType.Leaf;
		}



		public override INode Init()
		{
			initialize.Invoke(this);
			if (nodeState == NodeState.Failure)
			{// will cause recurssion, especially if a long chain were to fail.
				return Exit();
			}

			return this;
		}

		/// <summary>
		/// If nodeState is anything except Running, then return control to the parent.
		/// </summary>
		/// <returns></returns>
		public override NodeState Evaluate()
		{
			action.Invoke(this);
			return nodeState;
		}


		public override INode Exit()
		{
			return parent.ChildFinished(nodeState);
		}
	}
}