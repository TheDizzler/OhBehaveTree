using AtomosZ.OhBehave;
using UnityEngine;

namespace AtomosZ.Zomboo
{
	public class Zomboo : MonoBehaviour
	{

		public OhBehaveAI bsm;

		void Start()
		{
			SequenceNode rootNode = new SequenceNode();
			LeafNode countNode = new LeafNode();
			countNode.action.AddListener(MoveRight);
			countNode.initialize.AddListener(MoveRightStart);
			rootNode.AddNode(countNode);

			SelectorNode selectorNode = new SelectorNode();
			rootNode.AddNode(selectorNode);

			LeafNode countDownNode = new LeafNode();
			countDownNode.action.AddListener(MoveLeft);
			countDownNode.initialize.AddListener(MoveLeftStart);
			rootNode.AddNode(countDownNode);



			LeafNode timerNode = new LeafNode();
			timerNode.action.AddListener(TimerUp);
			timerNode.initialize.AddListener(TimerUpStart);
			selectorNode.AddNode(timerNode);

			LeafNode timerDownNode = new LeafNode();
			timerDownNode.action.AddListener(TimerDown);
			timerDownNode.initialize.AddListener(TimerDownStart);
			selectorNode.AddNode(timerDownNode);


			bsm.SetRoot(rootNode);
		}

		void Update()
		{
			bsm.Evaluate();
		}


		public void MoveRight(LeafNode node)
		{
			transform.position = transform.position + new Vector3(5 * Time.deltaTime, 0, 0);
			if (transform.position.x <= -3)
				node.nodeState = NodeState.Running;
			else
				node.nodeState = NodeState.Success;
		}

		public void MoveRightStart(LeafNode node)
		{
			Debug.Log("MoveRightStart");
			node.nodeState = NodeState.Running;
		}

		public void MoveLeft(LeafNode node)
		{
			transform.position = transform.position + new Vector3(-5 * Time.deltaTime, 0, 0);
			if (transform.position.x >= -6)
				node.nodeState = NodeState.Running;
			else
				node.nodeState = NodeState.Success;
		}

		public void MoveLeftStart(LeafNode node)
		{
			Debug.Log("MoveLeftStart");
			node.nodeState = NodeState.Running;
		}

		float time = 0;
		public void TimerUp(LeafNode node)
		{
			time += Time.deltaTime;
			if (time >= 2)
			{
				Debug.Log("2 second count down");
				node.nodeState = NodeState.Failure;
				return;
			}

			node.nodeState = NodeState.Running;
		}

		public void TimerUpStart(LeafNode node)
		{
			Debug.Log("TimerUpStart");
			node.nodeState = NodeState.Running;
		}

		public void TimerDown(LeafNode node)
		{
			time -= Time.deltaTime;
			if (time <= 0)
			{
				Debug.Log("counted off");
				node.nodeState = NodeState.Success;
				return;
			}

			node.nodeState = NodeState.Running;
		}

		public void TimerDownStart(LeafNode node)
		{
			Debug.Log("TimerDownStart");
			node.nodeState = NodeState.Running;
		}
	}
}