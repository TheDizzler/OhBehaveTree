using UnityEngine;

namespace AtomosZ.OhBehave
{
	public class BehaviorStateMachine
	{
		private ICompositeNode root;
		private INode currentNode;


		public BehaviorStateMachine(ICompositeNode rootNode)
		{
			root = rootNode;
			currentNode = root.Init();
		}


		public void Evaluate()
		{
			if (currentNode.Evaluate() != NodeState.Running)
			{
				INode nextNode = currentNode.Exit();

				int whileLoops = 0;
				while (nextNode.nodeState != NodeState.Running)
				{
					whileLoops++;
					if (whileLoops > 50)
						throw new System.Exception("Behavior tree stuck in loop - cannot find valid node to run");
					if (nextNode == root)
					{
						Debug.Log("No nodes can run?");
						nextNode = nextNode.Init();
						break;
					}

					nextNode = nextNode.Exit();
				}

				currentNode = nextNode;
			}
		}
	}
}