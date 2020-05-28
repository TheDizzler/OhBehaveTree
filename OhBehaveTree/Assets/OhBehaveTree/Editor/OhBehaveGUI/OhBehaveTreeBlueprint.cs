using System;
using System.Collections.Generic;
using System.IO;
//using Leguar.TotalJSON;
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
		public const int ROOT_INDEX = 0;
		public const int NO_PARENT_INDEX = -1;
		public const int ROOT_NODE_PARENT_INDEX = -69;
		public const int DefaultTreeRowHeight = 75;
		public const string blueprintsPrefix = "BTO_";

		public static string blueprintsPath = "Assets/OhBehaveTree/Editor/_Blueprints";


		public OhBehaveTreeController ohBehaveTree;
		public NodeEditorObject selectedNode;

		private Dictionary<int, NodeEditorObject> nodeObjects;
		private SerializedObject serializedObject;
		/// <summary>
		/// For keeping track of indices when assigning an index to a new node.
		/// </summary>
		private int lastNodeIndex = ROOT_INDEX;
		private List<NodeEditorObject> deleteTasks = new List<NodeEditorObject>();


		public void ConstructNodes()
		{
			if (nodeObjects == null)
			{
				//Debug.LogError("We NEED to serialize our NodeObjects...");
				//return;
				nodeObjects = new Dictionary<int, NodeEditorObject>();
				string blueprintPath = AssetDatabase.GetAssetPath(this);
				string jsonFilepath = Application.dataPath + "/../"
					+ Path.GetDirectoryName(blueprintPath) + "/"
					+ Path.GetFileNameWithoutExtension(blueprintPath)
					+ ".json";
				if (!File.Exists(jsonFilepath))
				{
					Debug.LogError("Could not find json file for " + blueprintPath);
				}
				else
				{
					StreamReader reader = new StreamReader(jsonFilepath);
					string fileString = reader.ReadToEnd();
					reader.Close();

					JsonNodeWrapper nodes = JsonUtility.FromJson<JsonNodeWrapper>(fileString);

					foreach (var node in nodes.nodes)
					{
						nodeObjects.Add(node.index, node);
						if (node.index > lastNodeIndex)
							lastNodeIndex = node.index;
					}
				}
			}

			serializedObject = new SerializedObject(this);

			if (nodeObjects.Count == 0)
			{
				if (OhBehaveEditorWindow.SequenceNodeStyle == null)
					return;

				var winData = new Rect(
							EditorWindow.GetWindow<OhBehaveEditorWindow>().position.width / 2, 0,
							OhBehaveEditorWindow.SequenceNodeStyle.size.x,
							OhBehaveEditorWindow.SequenceNodeStyle.size.y);

				NodeEditorObject newNode = new NodeEditorObject(NodeType.Sequence, ROOT_INDEX, ROOT_NODE_PARENT_INDEX)
				{
					description = ohBehaveTree.description,
					displayName = "Root",
					windowRect = winData,
				};
				nodeObjects.Add(0, newNode);

				Save();
			}
		}

		public void DeselectNode()
		{
			if (selectedNode == null || selectedNode.window == null)
				return;
			selectedNode.window.Deselect();
			selectedNode = null;
		}

		public void Save()
		{
			JsonNodeWrapper wrappedNodes = new JsonNodeWrapper();
			List<NodeEditorObject> nodes = new List<NodeEditorObject>();
			foreach (var node in nodeObjects.Values)
				nodes.Add(node);
			wrappedNodes.nodes = nodes.ToArray();
			string jsonString = JsonUtility.ToJson(wrappedNodes, true);

			string jsonFilepath = Application.dataPath + "/../"
				+ Path.GetDirectoryName(AssetDatabase.GetAssetPath(this)) + "/"
				+ Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(this))
				+ ".json";
			StreamWriter writer = new StreamWriter(jsonFilepath);
			writer.WriteLine(jsonString);
			writer.Close();
		}


		public void OnGui(Event current)
		{
			if (serializedObject == null)
				serializedObject = new SerializedObject(this);

			if (nodeObjects == null)
			{
				ConstructNodes();
				return;
			}

			foreach (var node in nodeObjects.Values)
			{
				node.ProcessEvents(current);
				node.OnGUI();
			}

			PendingDeletes();
		}

		public void ProcessContextMenu(NodeEditorObject parentNode)
		{
			GenericMenu genericMenu = new GenericMenu();
			genericMenu.AddItem(new GUIContent("Add Leaf"), false, () => CreateChildNode(parentNode, NodeType.Leaf));
			genericMenu.AddItem(new GUIContent("Add Inverter"), false, () => CreateChildNode(parentNode, NodeType.Inverter));
			genericMenu.AddItem(new GUIContent("Add Sequence"), false, () => CreateChildNode(parentNode, NodeType.Sequence));
			genericMenu.AddItem(new GUIContent("Add Selector"), false, () => CreateChildNode(parentNode, NodeType.Selector));
			genericMenu.ShowAsContext();
		}

		public NodeEditorObject GetNodeObject(int nodeIndex)
		{
			if (nodeIndex <= OhBehaveTreeBlueprint.NO_PARENT_INDEX)
			{ 
				return null;
			}
			return nodeObjects[nodeIndex];
		}


		public void DeleteNode(NodeEditorObject node)
		{
			deleteTasks.Add(node);
		}


		private void PendingDeletes()
		{
			foreach (var node in deleteTasks)
			{
				Debug.Log("delete me! " + node.displayName);
				if (node.parentIndex == ROOT_NODE_PARENT_INDEX)
				{
					Debug.Log("Delete denied: I am Root");
					return;
				}

				if (node.parentIndex != NO_PARENT_INDEX && node.Parent != null)
					node.Parent.ChildDeleted(node.index);

				node.NotifyChildrenOfDelete();


				if (!nodeObjects.Remove(node.index))
				{
					Debug.LogWarning(node.displayName + " index " + node.index + " was NOT found.");
				}
			}

			deleteTasks.Clear();
			Save();
		}

		private void CreateChildNode(NodeEditorObject parentNode, NodeType nodeType)
		{
			Rect parentWindowRect = parentNode.window.GetRect();

			switch (nodeType)
			{
				case NodeType.Leaf:
				case NodeType.Sequence:
					NodeEditorObject newNode 
						= new NodeEditorObject(nodeType, ++lastNodeIndex, parentNode.index)
					{
						description = nodeType + " type node. Add description of desired behaviour",
						displayName = nodeType.ToString(),
						windowRect = new Rect(
						new Vector2(parentWindowRect.x,
							parentWindowRect.y +parentWindowRect.height + DefaultTreeRowHeight),
						OhBehaveEditorWindow.SequenceNodeStyle.size)
					};
					nodeObjects.Add(lastNodeIndex, newNode);
					parentNode.AddChild(newNode);
					break;
				case NodeType.Inverter:
				case NodeType.Selector:

					Debug.LogWarning("TODO: CreateChildNode of type " + nodeType);
					break;
			}

			Save();
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
				blueprintsPath + "/" + blueprintsPrefix
					+ Path.GetFileNameWithoutExtension(path)
					+ GetInstanceID() + ".asset");

			var statemachine = CreateInstance<OhBehaveTreeController>();
			statemachine.Initialize(path);
			ohBehaveTree = statemachine;

			JsonNodeWrapper wrappedNodes = new JsonNodeWrapper();
			wrappedNodes.nodes = new NodeEditorObject[0];
			string jsonString = JsonUtility.ToJson(wrappedNodes, true);

			string jsonFilepath = Application.dataPath + "/../"
				+ Path.GetDirectoryName(AssetDatabase.GetAssetPath(this)) + "/"
				+ Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(this))
				+ ".json";
			StreamWriter writer = new StreamWriter(jsonFilepath);
			writer.WriteLine(jsonString);
			writer.Close();
		}


		[Serializable]
		private class JsonNodeWrapper
		{
			public NodeEditorObject[] nodes;
		}
	}
}