﻿using System;
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

		[SerializeField]
		private NodeEditorObject selectedNode;
		private Dictionary<int, NodeEditorObject> nodeObjects;
		private SerializedObject serializedObject;
		/// <summary>
		/// For keeping track of indices when assigning an index to a new node.
		/// </summary>
		private int lastNodeIndex = ROOT_INDEX;
		private List<NodeEditorObject> deleteTasks = new List<NodeEditorObject>();
		private ConnectionPoint startConnection;
		private ConnectionPoint endConnection;
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

				save = true;
			}
		}


		public void OnGui(Event current, EditorZoomer zoomer)
		{
			if (serializedObject == null)
				serializedObject = new SerializedObject(this);

			if (nodeObjects == null)
			{
				ConstructNodes();
				return;
			}

			bool isValidTree = true;
			foreach (var node in nodeObjects.Values)
			{
				node.Offset(zoomer.GetContentOffset());
				if (node.ProcessEvents(current))
					save = true;
				if (!node.CheckIsValid())
					isValidTree = false;
				node.OnGUI();
			}


			if (startConnection != null)
			{// we want to draw the line on-top of everything else
				Handles.DrawLine(startConnection.rect.center, current.mousePosition);
				GUI.changed = true;
				if (current.button == 1
					&& current.type == EventType.MouseDown)
				{
					startConnection = null;
					endConnection = null;
				}
				else if (endConnection != null)
				{
					CompleteConnection();
				}
				else if (current.button == 0
					&& current.type == EventType.MouseUp)
				{
					// if this has not been consumed we can (?) assume that
					//	the mouse was not released over a connection point
					savedMousePos = current.mousePosition;
					if (startConnection.type == ConnectionPointType.Out)
						CreateChildContextMenu(startConnection.nodeWindow.nodeObject, true);
					else
						CreateParentContextMenu(startConnection.nodeWindow.nodeObject, true);
					startConnection = null;
				}
			}
			else if (current.button == 1
				&& current.type == EventType.MouseUp
				&& !zoomer.isScreenMoved)
			{
				CreateStandAloneContextMenu();
			}

			zoomer.DisplayInvalid(isValidTree);


			PendingDeletes();

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

		public void DeselectNode()
		{
			if (!IsNodeSelected())
				return;
			selectedNode.window.Deselect();
			selectedNode = null;
		}



		public void StartPointSelected(ConnectionPoint selectedConnection)
		{
			if (startConnection != null)
			{
				Debug.LogError("Trying to make new start point");
				return;
			}

			startConnection = selectedConnection;
		}

		public void EndPointSelected(ConnectionPoint endPoint)
		{
			if (endPoint.type != startConnection.type
				&& endPoint.nodeWindow != startConnection.nodeWindow)
			{
				endConnection = endPoint;
			}
			else
			{ // cancel this new connection
				startConnection = null;
			}
		}


		public bool IsValidConnection(ConnectionPoint hoveredPoint)
		{
			if (startConnection == null || hoveredPoint == startConnection)
				return true; // no points have been selected or this is the first selected point
			return (hoveredPoint.type != startConnection.type
				&& hoveredPoint.nodeWindow != startConnection.nodeWindow);
		}

		private void CompleteConnection()
		{
			NodeEditorObject nodeParent, nodeChild;
			if (startConnection.type == ConnectionPointType.Out)
			{
				nodeParent = startConnection.nodeWindow.nodeObject;
				nodeChild = endConnection.nodeWindow.nodeObject;
			}
			else
			{
				nodeParent = endConnection.nodeWindow.nodeObject;
				nodeChild = startConnection.nodeWindow.nodeObject;
			}

			if (nodeParent.nodeType == NodeType.Inverter && nodeParent.HasChildren())
			{
				// orphan the olde child
				var oldChild = GetNodeObject(nodeParent.children[0]);
				oldChild.RemoveParent();
				nodeParent.RemoveChild(oldChild.index);
			}

			var oldParent = GetNodeObject(nodeChild.parentIndex);
			if (oldParent != null)
			{
				// remove from old parent
				oldParent.RemoveChild(nodeChild.index);
				nodeChild.RemoveParent();
			}

			// ok, let's do this
			nodeParent.AddChild(nodeChild);
			nodeChild.AddParent(nodeParent.index);

			endConnection = null;
			startConnection = null;
			save = true;
		}


		private void CreateStandAloneContextMenu()
		{
			var genericMenu = new GenericMenu();
			genericMenu.AddItem(new GUIContent("Add Leaf"), false,
				() => CreateStandAloneNode(NodeType.Leaf));
			genericMenu.AddItem(new GUIContent("Add Inverter"), false,
				() => CreateStandAloneNode(NodeType.Inverter));
			genericMenu.AddItem(new GUIContent("Add Sequence"), false,
				() => CreateStandAloneNode(NodeType.Sequence));
			genericMenu.AddItem(new GUIContent("Add Selector"), false,
				() => CreateStandAloneNode(NodeType.Selector));
			genericMenu.ShowAsContext();
		}

		public void CreateParentContextMenu(NodeEditorObject childNode, bool createAtMousePosition = false)
		{
			var genericMenu = new GenericMenu();
			genericMenu.AddItem(new GUIContent("Add Inverter"), false,
				() => CreateParentNode(childNode, NodeType.Inverter, createAtMousePosition));
			genericMenu.AddItem(new GUIContent("Add Sequence"), false,
				() => CreateParentNode(childNode, NodeType.Sequence, createAtMousePosition));
			genericMenu.AddItem(new GUIContent("Add Selector"), false,
				() => CreateParentNode(childNode, NodeType.Selector, createAtMousePosition));
			genericMenu.ShowAsContext();
		}

		public void CreateChildContextMenu(NodeEditorObject parentNode, bool createAtMousePosition = false)
		{
			var genericMenu = new GenericMenu();
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

			if (!nodeObjects.TryGetValue(nodeIndex, out NodeEditorObject node))
			{
				Debug.LogError("GetNodeObject() Error! " + nodeIndex + " does not exist in!");
				return null;
			}

			return node;
		}


		public void DeleteNode(NodeEditorObject node)
		{
			deleteTasks.Add(node);
		}

		private void Save()
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

			AssetDatabase.Refresh();
		}

		private void PendingDeletes()
		{
			if (deleteTasks.Count == 0)
				return;
			foreach (var node in deleteTasks)
			{
				if (node.parentIndex == ROOT_NODE_PARENT_INDEX)
				{
					Debug.LogWarning("Delete denied: I am Root");
					continue;
				}

				node.NotifyFamilyOfDelete();


				if (!nodeObjects.Remove(node.index))
				{
					Debug.LogWarning(node.displayName + " index " + node.index + " was NOT found.");
				}
			}

			GUI.changed = true;
			deleteTasks.Clear();
			save = true;
		}

		private void CreateStandAloneNode(NodeType nodeType)
		{
			switch (nodeType)
			{
				case NodeType.Leaf:
				case NodeType.Sequence:
				case NodeType.Selector:
				case NodeType.Inverter:
					NodeEditorObject newNode
						= new NodeEditorObject(nodeType, ++lastNodeIndex, NO_PARENT_INDEX)
						{
							description = nodeType + " type node. Add description of desired behaviour",
							displayName = nodeType.ToString(),
							windowRect = new Rect(
								savedMousePos + EditorWindow.GetWindow<OhBehaveEditorWindow>().zoomer.GetContentOffset(),
								OhBehaveEditorWindow.SequenceNodeStyle.size)
						};
					nodeObjects.Add(lastNodeIndex, newNode);
					save = true;
					break;

				default:
					Debug.LogWarning("TODO: CreateChildNode of type " + nodeType);
					break;
			}
		}

		private void CreateParentNode(NodeEditorObject childNode, NodeType nodeType, bool createAtMousePosition)
		{
			Rect childWindowRect = childNode.window.GetRectNoOffset();
			childNode.RemoveParent();
			switch (nodeType)
			{
				case NodeType.Sequence:
				case NodeType.Selector:
				case NodeType.Inverter:
					NodeEditorObject newNode
						= new NodeEditorObject(nodeType, ++lastNodeIndex, NO_PARENT_INDEX)
						{
							description = nodeType + " type node. Add description of desired behaviour",
							displayName = nodeType.ToString(),
							windowRect = new Rect(createAtMousePosition ?
								savedMousePos + EditorWindow.GetWindow<OhBehaveEditorWindow>().zoomer.GetContentOffset() :
								new Vector2(childWindowRect.x,
									childWindowRect.y - childWindowRect.height - DefaultTreeRowHeight),
								OhBehaveEditorWindow.SequenceNodeStyle.size)
						};
					nodeObjects.Add(lastNodeIndex, newNode);
					newNode.AddChild(childNode);
					childNode.AddParent(newNode.index);
					save = true;
					break;

				case NodeType.Leaf:
					throw new Exception("A leaf may not be a parent");
				default:
					Debug.LogWarning("TODO: CreateParentNode of type " + nodeType);
					break;
			}
		}

		private void CreateChildNode(NodeEditorObject parentNode, NodeType nodeType, bool createAtMousePosition)
		{
			Rect parentWindowRect = parentNode.window.GetRectNoOffset();
			if (parentNode.nodeType == NodeType.Inverter)
			{
				if (parentNode.HasChildren())
				{
					GetNodeObject(parentNode.children[0]).RemoveParent();
					parentNode.RemoveChild(parentNode.children[0]);
				}
			}

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
							windowRect = new Rect(createAtMousePosition ?
								savedMousePos + EditorWindow.GetWindow<OhBehaveEditorWindow>().zoomer.GetContentOffset() :
								new Vector2(parentWindowRect.x,
									parentWindowRect.y + parentWindowRect.height + DefaultTreeRowHeight),
								OhBehaveEditorWindow.SequenceNodeStyle.size)
						};
					nodeObjects.Add(lastNodeIndex, newNode);
					parentNode.AddChild(newNode);
					save = true;
					break;

				default:
					Debug.LogWarning("TODO: CreateChildNode of type " + nodeType);
					break;
			}
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