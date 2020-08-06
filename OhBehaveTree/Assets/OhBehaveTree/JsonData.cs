namespace AtomosZ.OhBehave
{
	/// <summary>
	/// A complete, valid behavior tree for an OhBehaveAI actor.
	/// </summary>
	[System.Serializable]
	public class JsonBehaviourTree
	{
		public string name;
		public string blueprintGUID;

		/// <summary>
		/// Not sure how this will work with serialization.
		/// </summary>
		public OhBehaveActions actionSource;
		public JsonNodeData rootNode;
		public JsonNodeData[] tree;
	}

	[System.Serializable]
	public class JsonNodeData
	{
		public int index;
		public int parentIndex;
		public int[] childrenIndices;
		public NodeType nodeType;
		public string methodInfoName;
	}
}
