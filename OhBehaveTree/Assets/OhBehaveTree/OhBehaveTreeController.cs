using UnityEditor;
using UnityEngine;

namespace AtomosZ.OhBehave
{
	public class OhBehaveTreeController : ScriptableObject
	{
		public string behaviorName;
		public string description;
		public ICompositeNode rootNode;

		public GameObject functionSource;

		public string blueprintGUID;


		/// <summary>
		/// This should only be called when first created in the editor.
		/// </summary>
		/// <param name="path"></param>
		public void Initialize(string path)
		{
			AssetDatabase.CreateAsset(this, path);
			rootNode = new SequenceNode();

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