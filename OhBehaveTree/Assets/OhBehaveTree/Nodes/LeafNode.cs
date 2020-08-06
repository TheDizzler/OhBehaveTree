using System;
using System.Reflection;
using UnityEngine.Events;

namespace AtomosZ.OhBehave
{
	public class LeafNode : INode
	{
		public MethodInfo actionInfo;


		public LeafNode()
		{
			nodeType = NodeType.Leaf;
		}



		public override INode Init()
		{
			nodeState = NodeState.Running;
			return this;
		}

		/// <summary>
		/// If nodeState is anything except Running, then return control to the parent.
		/// </summary>
		/// <returns></returns>
		public override NodeState Evaluate()
		{
			/* This is what the invokation would look like....*/
			var result = actionInfo.Invoke(this, new object[] { this });

			return nodeState;
		}


		public override INode Exit()
		{
			return parent.ChildFinished(nodeState);
		}
	}
}