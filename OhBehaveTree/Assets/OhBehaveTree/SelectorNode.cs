using System;
using System.Collections.Generic;
using UnityEngine;

namespace AtomosZ.OhBehave
{
	/// <summary>
	/// A Composite Node that returns Success if any of it's children return Success.
	/// Evaluates all Nodes in order (or random, if selected) and returns on first child Success.
	/// </summary>
	[CreateAssetMenu(fileName = "NewNode", menuName = "Nodes/Selector")]
	public class SelectorNode : ICompositeNode
	{
		[SerializeField]
		protected bool random = false;
		[SerializeField]
		protected List<INode> nodes;

		public SelectorNode()
		{
			nodeType = NodeType.Selector;
		}

		public override NodeState Evaluate()
		{
			throw new NotImplementedException();
		}
	}
}