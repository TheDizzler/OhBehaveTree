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
		/// <summary>
		/// Goddamn scriptable object LOVE losing data.
		/// </summary>
		public string controllerFilePath;
		public bool isDrawingNewConnection = false;

		[SerializeField]
		private NodeEditorObject selectedNode;
		private Dictionary<int, NodeEditorObject> nodeObjects;
		private SerializedObject serializedObject;
		/// <summary>
		/// For keeping track of indices when assigning an index to a new node.
		/// </summary>
		private int lastNodeIndex = ROOT_INDEX;
		private List<NodeEditorObject> deleteTasks = new List<NodeEditorObject>();
		private ConnectionPoint connectionDrawing;
		private bool cancelMakeNewConnection;
		private Vector2 savedMousePos;
		private bool save;


		public void ConstructNodes()
		{
			if (nodeObjects == null)
			{
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

					JsonData data = JsonUtility.FromJson<JsonData>(fileString);
					var ohBehave = EditorWindow.GetWindow<OhBehaveEditorWindow>();
					ohBehave.zoomer.SetScale(data.zoomScale);
					ohBehave.zoomer.SetOrigin(data.origin);

					JsonNodeWrapper nodes = data.nodeWrapper;

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
							-OhBehaveEditorWindow.SequenceNodeStyle.size.x / 2,
							-OhBehaveEditorWindow.SequenceNodeStyle.size.y,
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

		public void OnGui(Event current, Vector2 contentOffset)
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
				node.Offset(contentOffset);
				if (node.ProcessEvents(current))
					save = true;
				node.OnGUI();
			}

			PendingDeletes();

			if (cancelMakeNewConnection)
			{
				isDrawingNewConnection = false;
				connectionDrawing = null;
				cancelMakeNewConnection = false;
			}

			if (save)
			{
				Save();
				save = false;
			}
		}

		public void SelectNode(NodeEditorObject nodeObject)
		{
			if (IsNodeSelected())
			{
				selectedNode.window.Deselect();
			}

			selectedNode = nodeObject;
		}

		public void DrawingNewConnection(ConnectionPoint connectionPoint)
		{
			isDrawingNewConnection = true;
			connectionDrawing = connectionPoint;
		}

		public bool CheckValidConnection(ConnectionPoint potentialConnection)
		{
			if (connectionDrawing == null)
			{       // this is fucked. This should never happen, yet, here we are.
				isDrawingNewConnection = false;
				return false;
			}

			return connectionDrawing.type != potentialConnection.type && connectionDrawing.nodeWindow != potentialConnection.nodeWindow;
		}

		public void CancelNewConnection(ConnectionPoint connectionPoint)
		{
			cancelMakeNewConnection = true;
			savedMousePos = Event.current.mousePosition;
			// Prompt to create new node
			ProcessContextMenu(connectionPoint.nodeWindow.nodeObject, true);
		}

		/// <summary>
		/// The connection needs to be verified prior to this or we could have massive problems.
		/// </summary>
		/// <param name="connectionPoint"></param>
		public void CreateNewConnection(ConnectionPoint connectionPoint)
		{
			NodeEditorObject nodeParent, nodeChild;
			if (connectionPoint.type == ConnectionPointType.Out)
			{
				nodeParent = connectionPoint.nodeWindow.nodeObject;
				nodeChild = connectionDrawing.nodeWindow.nodeObject;
			}
			else
			{
				nodeChild = connectionPoint.nodeWindow.nodeObject;
				nodeParent = connectionDrawing.nodeWindow.nodeObject;
			}

			if (nodeParent.nodeType == NodeType.Inverter && nodeParent.children.Count != 0)
			{
				// orphan the olde child
				var oldChild = GetNodeObject(nodeParent.children[0]);
				if (!oldChild.ParentRemoved(nodeParent.index))
					throw new Exception("WTF problems removing parent from child in CreateNewConnection()");
			}

			var oldParent = GetNodeObject(nodeChild.parentIndex);
			if (oldParent != null)
			{
				// remove from old parent
				oldParent.ChildRemoved(nodeChild.index);
			}

			// ok, let's do this
			nodeParent.AddChild(nodeChild);
			nodeChild.ParentChanged(nodeParent.index);

			//nodeParent.window.CreateConnectionToParent(;
			nodeChild.window.CreateConnectionToParent((IParentNodeWindow)nodeParent.window);

			Save();
		}


		public void DeselectNode()
		{

			if (!IsNodeSelected())
				return;
			selectedNode.window.Deselect();
			selectedNode = null;
		}

		public void Save()
		{
			var ohBehave = EditorWindow.GetWindow<OhBehaveEditorWindow>();
			JsonData data = new JsonData();
			data.origin = ohBehave.zoomer.GetOrigin();
			data.zoomScale = ohBehave.zoomer.GetScale();


			JsonNodeWrapper wrappedNodes = new JsonNodeWrapper();
			List<NodeEditorObject> nodes = new List<NodeEditorObject>();
			foreach (var node in nodeObjects.Values)
				nodes.Add(node);
			wrappedNodes.nodes = nodes.ToArray();
			data.nodeWrapper = wrappedNodes;

			string jsonString = JsonUtility.ToJson(data, true);

			string jsonFilepath = Application.dataPath + "/../"
				+ Path.GetDirectoryName(AssetDatabase.GetAssetPath(this)) + "/"
				+ Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(this))
				+ ".json";
			StreamWriter writer = new StreamWriter(jsonFilepath);
			writer.WriteLine(jsonString);
			writer.Close();
		}

		public void ProcessContextMenu(NodeEditorObject parentNode, bool createAtMousePosition = false)
		{
			GenericMenu genericMenu = new GenericMenu();
			genericMenu.AddItem(new GUIContent("Add Leaf"), false, 
				() => CreateChildNode(parentNode, NodeType.Leaf, createAtMousePosition));
			genericMenu.AddItem(new GUIContent("Add Inverter"), false, 
				() => CreateChildNode(parentNode, NodeType.Inverter, createAtMousePosition));
			genericMenu.AddItem(new GUIContent("Add Sequence"), false, 
				() => CreateChildNode(parentNode, NodeType.Sequence, createAtMousePosition));
			genericMenu.AddItem(new GUIContent("Add Selector"), false, 
				() => CreateChildNode(parentNode, NodeType.Selector, createAtMousePosition));
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
			if (deleteTasks.Count == 0)
				return;
			foreach (var node in deleteTasks)
			{
				Debug.Log("delete me! " + node.displayName);
				if (node.parentIndex == ROOT_NODE_PARENT_INDEX)
				{
					Debug.Log("Delete denied: I am Root");
					return;
				}

				if (node.parentIndex != NO_PARENT_INDEX && node.Parent != null)
					node.Parent.ChildRemoved(node.index);

				node.NotifyChildrenOfDelete();


				if (!nodeObjects.Remove(node.index))
				{
					Debug.LogWarning(node.displayName + " index " + node.index + " was NOT found.");
				}
			}

			GUI.changed = true;
			deleteTasks.Clear();
			Save();
		}

		private void CreateChildNode(NodeEditorObject parentNode, NodeType nodeType, bool createAtMousePosition)
		{
			Rect parentWindowRect = parentNode.window.GetRectNoOffset();

			switch (nodeType)
			{
				case NodeType.Leaf:
				case NodeType.Sequence:
				case NodeType.Selector:
				case NodeType.Inverter:
					NodeEditorObject newNode
						= new NodeEditorObject(nodeType, ++lastNodeIndex, parentNode.index)
						{
							description = nodeType + " type node. Add description of desired behaviour",
							displayName = nodeType.ToString(),
							windowRect = new Rect(createAtMousePosition?
								savedMousePos + EditorWindow.GetWindow<OhBehaveEditorWindow>().zoomer.GetContentOffset():
								new Vector2(parentWindowRect.x,
									parentWindowRect.y + parentWindowRect.height + DefaultTreeRowHeight),
							OhBehaveEditorWindow.SequenceNodeStyle.size)
						};
					nodeObjects.Add(lastNodeIndex, newNode);
					parentNode.AddChild(newNode);
					break;

				default:
					Debug.LogWarning("TODO: CreateChildNode of type " + nodeType);
					break;
			}

			Save();
		}


		public bool IsNodeSelected()
		{
			return selectedNode != null && !string.IsNullOrEmpty(selectedNode.displayName);
		}

		public NodeEditorObject GetSelectedNode()
		{
			return selectedNode;
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
			controllerFilePath = path;

			JsonData data = new JsonData();
			data.origin = Vector2.zero;
			data.zoomScale = 1;
			JsonNodeWrapper wrappedNodes = new JsonNodeWrapper();
			wrappedNodes.nodes = new NodeEditorObject[0];
			data.nodeWrapper = wrappedNodes;
			string jsonString = JsonUtility.ToJson(data, true);

			string jsonFilepath = Application.dataPath + "/../"
				+ Path.GetDirectoryName(AssetDatabase.GetAssetPath(this)) + "/"
				+ Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(this))
				+ ".json";
			StreamWriter writer = new StreamWriter(jsonFilepath);
			writer.WriteLine(jsonString);
			writer.Close();
		}

		public void FindYourControllerDumbass()
		{
			ohBehaveTree = (OhBehaveTreeController)
				AssetDatabase.LoadAssetAtPath(controllerFilePath, typeof(OhBehaveTreeController));
		}



		[Serializable]
		private class JsonNodeWrapper
		{
			public NodeEditorObject[] nodes;
		}

		[Serializable]
		private class JsonData
		{
			public Vector2 origin;
			public float zoomScale;
			public JsonNodeWrapper nodeWrapper;
		}
	}
}