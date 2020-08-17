using UnityEngine;

namespace AtomosZ.OhBehave.Demo
{
	public class Zomboo : OhBehaveActions
	{
		public void MoveRight(LeafNode node)
		{
			transform.position = transform.position + new Vector3(5 * Time.deltaTime, 0, 0);
			if (transform.position.x <= -3)
				node.nodeState = NodeState.Running;
			else
			{
				Debug.Log("More right done");
				node.nodeState = NodeState.Success;
			}
		}

		public void MoveLeft(LeafNode node)
		{
			transform.position = transform.position + new Vector3(-5 * Time.deltaTime, 0, 0);
			if (transform.position.x >= -6)
				node.nodeState = NodeState.Running;
			else
			{
				Debug.Log("More left done");
				node.nodeState = NodeState.Success;
			}
		}


		float time = 0;
		public void TimerUp(LeafNode node)
		{
			time += Time.deltaTime;
			if (time >= 2)
			{
				Debug.Log("counted up");
				node.nodeState = NodeState.Success;
				return;
			}

			node.nodeState = NodeState.Running;
		}


		public void TimerDown(LeafNode node)
		{
			time -= Time.deltaTime;
			if (time <= 0)
			{
				Debug.Log("counted down");
				node.nodeState = NodeState.Success;
				return;
			}

			node.nodeState = NodeState.Running;
		}

	}
}