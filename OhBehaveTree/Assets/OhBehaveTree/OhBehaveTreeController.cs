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

		public string blueprintGUID;

		public OhBehaveActions behaviorSource;
		public List<MethodInfo> sharedMethods = null;
		public List<MethodInfo> privateMethods = null;
		public List<string> sharedMethodNames = null;
		public List<string> privateMethodNames = null;


		private BindingFlags flags = BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Instance;



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
			sharedMethods = null;
			sharedMethodNames = null;
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
				sharedMethods = null;
				privateMethods = null;
				sharedMethodNames = null;
				privateMethodNames = null;
				return;
			}


			sharedMethods = new List<MethodInfo>();
			privateMethods = new List<MethodInfo>();
			sharedMethodNames = new List<string>();
			privateMethodNames = new List<string>();

			foreach (MethodInfo element in behaviorSource.GetType().GetMethods(flags))
			{
				foreach (var param in element.GetParameters())
				{
					if (param.ParameterType == typeof(LeafNode))
					{ // at least one of the params must be a LeafNode
						privateMethods.Add(element);
						privateMethodNames.Add(element.Name);
						break;
					}
				}
			}

			sharedMethods.Add(null);
			sharedMethods.AddRange(privateMethods);
			sharedMethodNames.Add("No action selected");
			sharedMethodNames.AddRange(privateMethodNames);
		}

		private void OnEnable()
		{
			if (sharedMethods == null)
				GetFunctions();
		}
	}
}