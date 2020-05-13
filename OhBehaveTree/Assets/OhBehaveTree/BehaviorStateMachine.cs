using UnityEngine;

namespace AtomosZ.OhBehave
{
	public class BehaviorStateMachine : MonoBehaviour
	{
		public ICompositeNode root;
		private INode currentNode;


		public void SetRoot(ICompositeNode rootNode)
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
						Debug.Log("Behaviour start from beginning");
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