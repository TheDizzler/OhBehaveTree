namespace AtomosZ.OhBehave
{
	public class LeafNode : INode
	{
		public LeafNode()
		{
			nodeType = NodeType.Leaf;
		}

		public override NodeState Evaluate()
		{
			throw new System.NotImplementedException();
		}
	}
}