using UnityEditor;
using UnityEngine;

namespace AtomosZ.OhBehave
{
	public class OhBehaveStateMachineController : ScriptableObject
	{
		public ICompositeNode parentNode;


		public void Initialize(string path)
		{
			AssetDatabase.CreateAsset(this, path);
			// add root node to statemachine
			parentNode = CreateInstance<SelectorNode>();
			AssetDatabase.AddObjectToAsset(parentNode, path); 
		}
	}
}