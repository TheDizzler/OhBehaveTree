using UnityEngine;

namespace AtomosZ.OhBehave
{
	public class AIOhBehave : MonoBehaviour
	{
		//[SerializeField] public OhBehaveStateMachineController ai = null;
		SequenceNode rootNode;
		BehaviorStateMachine bsm;

		void Start()
		{
			rootNode = new SequenceNode();
			LeafNode countNode = new LeafNode();
			countNode.action = new LeafNode.ActionNodeDelegate(CountUp);
			countNode.initialize = new LeafNode.InitializeNodeDelegate(CountUpStart);
			rootNode.AddNode(countNode);

			SelectorNode selectorNode = new SelectorNode();
			rootNode.AddNode(selectorNode);

			LeafNode countDownNode = new LeafNode();
			countDownNode.action = new LeafNode.ActionNodeDelegate(CountDown);
			countDownNode.initialize = new LeafNode.InitializeNodeDelegate(CountDownStart);
			rootNode.AddNode(countDownNode);



			LeafNode timerNode = new LeafNode();
			timerNode.action = new LeafNode.ActionNodeDelegate(TimerUp);
			timerNode.initialize = new LeafNode.InitializeNodeDelegate(TimerUpStart);
			selectorNode.AddNode(timerNode);

			LeafNode timerDownNode = new LeafNode();
			timerDownNode.action = new LeafNode.ActionNodeDelegate(TimerDown);
			timerDownNode.initialize = new LeafNode.InitializeNodeDelegate(TimerDownStart);
			selectorNode.AddNode(timerDownNode);


			bsm = new BehaviorStateMachine(rootNode);
		}


		void Update()
		{
			bsm.Evaluate();
		}



		int count = 0;
		private NodeState CountUp()
		{
			if (++count >= 1000)
			{
				Debug.Log("Counted to 1000");
				return NodeState.Success;
			}

			return NodeState.Running;
		}

		private NodeState CountUpStart()
		{
			Debug.Log("CountUpStart");
			return NodeState.Running;
		}

		private NodeState CountDown()
		{
			if (--count <= 0)
			{
				Debug.Log("Counted to 0");
				return NodeState.Success;
			}

			return NodeState.Running;
		}

		private NodeState CountDownStart()
		{
			Debug.Log("CountDownStart");
			return NodeState.Running;
		}

		float time = 0;
		private NodeState TimerUp()
		{
			time += Time.deltaTime;
			if (time >= 2)
			{
				Debug.Log("2 second count down");
				return NodeState.Failure;
			}

			return NodeState.Running;
		}

		private NodeState TimerUpStart()
		{
			Debug.Log("TimerUpStart");
			return NodeState.Running;
		}

		private NodeState TimerDown()
		{
			time -= Time.deltaTime;
			if (time <= 0)
			{
				Debug.Log("counted off");
				return NodeState.Success;
			}

			return NodeState.Running;
		}

		private NodeState TimerDownStart()
		{
			Debug.Log("TimerDownStart");
			return NodeState.Running;
		}
	}
}