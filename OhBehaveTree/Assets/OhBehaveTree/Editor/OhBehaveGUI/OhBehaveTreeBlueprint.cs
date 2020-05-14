using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace AtomosZ.OhBehave.EditorTools
{
	/// <summary>
	/// The visual representation of our Behavior Tree. This is an editor only object.
	/// This allows for "loose" nodes. Only the non-"loose" nodes get commited to
	/// the game BehaviorStateMachine.
	/// </summary>
	public class OhBehaveTreeBlueprint : ScriptableObject
	{
		public static string blueprintsPath = "Assets/OhBehaveTree/Editor/_Blueprints";
		public const string blueprintsPrefix = "BTO_";

		public OhBehaveTreeController ohBehaveTree;

		[SerializeField]
		private List<NodeEditorObject> nodeObjects;


		public void ConstructNodes()
		{
			Debug.Log("Show me the nodes!");
			if (nodeObjects == null)
			{
				Debug.LogError("We NEED to serialize our NodeObjects...");
				return;
			}

			if (nodeObjects.Count == 0)
			{
				Debug.Log("First!");
				NodeEditorObject newNode = new NodeEditorObject();
				newNode.nodeType = NodeType.Sequence;
				newNode.description = ohBehaveTree.description;
				newNode.displayName = ohBehaveTree.behaviorName;
				

			}

			foreach (var node in nodeObjects)
			{
				switch (node.nodeType)
				{
					case NodeType.Sequence:
						SequenceNodeWindow newWindow = new SequenceNodeWindow(node);
						break;
				}
			}
		}


		internal void ProcessContextMenu(CompositeNodeWindow parentNodeWindow)
		{
			GenericMenu genericMenu = new GenericMenu();
			//genericMenu.AddItem(new GUIContent("Add Leaf"), false, () => CreateChildNode(parentNodeWindow, NodeType.Leaf));
			//genericMenu.AddItem(new GUIContent("Add Sequence"), false, () => CreateChildNode(parentNodeWindow, NodeType.Sequence));
			//genericMenu.AddItem(new GUIContent("Add Selector"), false, () => CreateChildNode(parentNodeWindow, NodeType.Selector));
			genericMenu.ShowAsContext();
		}











		/// <summary>
		/// Only called when first constructed.
		/// </summary>
		/// <param name="path">Path to new OhbehaveTreeController</param>
		public void Initialize(string path)
		{
			if (ohBehaveTree != null)
			{
				throw new System.Exception("Initialize() should never be called "
					+ "on a OhBehaveTreeBlueprint more than once!");
			}

			if (!AssetDatabase.IsValidFolder(blueprintsPath))
			{
				string guid = AssetDatabase.CreateFolder(
					Path.GetDirectoryName(blueprintsPath), 
					Path.GetFileName(blueprintsPath));
				blueprintsPath = AssetDatabase.GUIDToAssetPath(guid);
			}

			AssetDatabase.CreateAsset(this,
				blueprintsPath + "/" + blueprintsPrefix + Path.GetFileNameWithoutExtension(path)
					+ GetInstanceID() + ".asset");

			var statemachine = CreateInstance<OhBehaveTreeController>();
			statemachine.Initialize(path);
			ohBehaveTree = statemachine;

			nodeObjects = new List<NodeEditorObject>();
		}
	}
}