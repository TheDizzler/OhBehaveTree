using System.Collections.Generic;
using UnityEngine;

namespace AtomosZ.OhBehave
{
	public abstract class ICompositeNode : INode
	{
		public List<INode> children = new List<INode>();
	}
}
