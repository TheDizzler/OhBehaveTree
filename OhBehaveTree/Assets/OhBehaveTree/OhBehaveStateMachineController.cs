using UnityEditor;
using UnityEngine;

namespace AtomosZ.OhBehave
{
	public class OhBehaveStateMachineController : ScriptableObject
	{
		public ICompositeNode rootNode;


		public void Initialize(string path)
		{
			AssetDatabase.CreateAsset(this, path);
			// add root node to statemachine
			//rootNode = CreateInstance<SelectorNode>();
			//AssetDatabase.AddObjectToAsset(rootNode, path);
		}

		public void Evaluate()
		{
			rootNode.Evaluate();
		}
	}
}