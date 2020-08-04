using UnityEngine;

namespace AtomosZ.OhBehave
{
	public abstract class OhBehaveActions : MonoBehaviour
	{
		public OhBehaveAI bsm;
		public OhBehaveTreeController treeController;


		void Start()
		{
			bsm.SetRoot(treeController.rootNode);
		}

		void Update()
		{
			bsm.Evaluate();
		}
	}
}