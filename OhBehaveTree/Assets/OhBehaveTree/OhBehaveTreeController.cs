using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace AtomosZ.OhBehave
{
	public class OhBehaveTreeController : ScriptableObject
	{
		[Tooltip("Has no in-game influence.")]
		public string behaviorTreeName;
		[Tooltip("Has no in-game influence.")]
		public string description;
		public ICompositeNode rootNode;

		public OhBehaveActions behaviorSource;
		public List<MethodInfo> methods = null;
		private BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly;




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

		public void EditorNeedsRefresh()
		{
			methods = null;
			GetFunctions();
		}

		public void Evaluate()
		{
			rootNode.Evaluate();
		}



		public void GetFunctions()
		{
			if (behaviorSource == null)
			{
				methods = null;
				return;
			}

			List<MethodInfo> allMethods = new List<MethodInfo>();
			methods = new List<MethodInfo>();

			allMethods.AddRange(behaviorSource.GetType().GetMethods(flags));

			foreach (MethodInfo element in allMethods)
			{
				foreach (var param in element.GetParameters())
				{
					if (param.ParameterType == typeof(LeafNode))
					{ // at least one of the params must be a LeafNode
						methods.Add(element);
						break;
					}
				}
			}
		}

		private void OnEnable()
		{
			if (methods == null)
				GetFunctions();
		}
	}
}