﻿using System;
using System.Collections.Generic;
using UnityEngine;

namespace AtomosZ.OhBehave
{
	/// <summary>
	/// A Composite Node that evaluates all child nodes and returns Success
	/// if ALL child nodes return Success.
	/// </summary>
	[Serializable]
	public class SequenceNode : ICompositeNode
	{
		[SerializeField]
		protected List<INode> nodes;
		protected NodeState state;

		public SequenceNode()
		{
			nodeType = NodeType.Sequence;
		}

		public override NodeState Evaluate()
		{
			throw new NotImplementedException();
		}
	}
}