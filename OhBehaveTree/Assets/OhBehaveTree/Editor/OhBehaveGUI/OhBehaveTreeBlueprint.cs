using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using static AtomosZ.OhBehave.EditorTools.NodeEditorObject;

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
		public string controllerGUID;

		public List<NodeEditorObject> savedNodes;

		[SerializeField]
		private NodeEditorObject selectedNode;

		private Dictionary<int, NodeEditorObject> nodeObjects;
		private SerializedObject serializedObject;
		/// <summary>
		/// For keeping track of indices when assigning an index to a new node.
		/// </summary>
		[SerializeField]
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
				//LoadFromJson();
				InitializeNodeDictionary();
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

				AddNewNode(newNode, 0);


				AssetDatabase.Refresh();
				EditorUtility.SetDirty(this);
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
			List<InvalidNodeMessage> errorMsgs = new List<InvalidNodeMessage>();
			foreach (var node in nodeObjects.Values)
			{
				node.Offset(zoomer.GetContentOffset());
				if (node.ProcessEvents(current))
					save = true;
				if (!node.CheckIsValid(out InvalidNodeMessage invalidMsg))
				{
					isValidTree = false;
					errorMsgs.Add(invalidMsg);
				}

				node.OnGUI();
			}


			if (startConnection != null)
			{// we want to draw the line on-top of everything else

				Handles.DrawAAPolyLine(ConnectionControls.lineThickness,
					startConnection.rect.center,
					current.mousePosition);


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
				savedMousePos = current.mousePosition;
				CreateStandAloneContextMenu();
			}

			zoomer.DisplayInvalid(isValidTree, errorMsgs);


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
				selectedNode.GetWindow().Deselect();
			}

			selectedNode = nodeObject;
		}

		public void DeselectNode()
		{
			if (!IsNodeSelected())
				return;
			selectedNode.GetWindow().Deselect();
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
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
			EditorUtility.SetDirty(this);
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

				savedNodes.Remove(node);
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

					AddNewNode(newNode, lastNodeIndex);

					break;

				default:
					Debug.LogWarning("TODO: CreateChildNode of type " + nodeType);
					break;
			}
		}

		private void CreateParentNode(NodeEditorObject childNode, NodeType nodeType, bool createAtMousePosition)
		{
			Rect childWindowRect = childNode.GetWindow().GetRectNoOffset();
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
					AddNewNode(newNode, lastNodeIndex);
					newNode.AddChild(childNode);
					childNode.AddParent(newNode.index);
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
			Rect parentWindowRect = parentNode.GetWindow().GetRectNoOffset();
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

					AddNewNode(newNode, lastNodeIndex);
					parentNode.AddChild(newNode);
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


		private void InitializeNodeDictionary()
		{
			nodeObjects = new Dictionary<int, NodeEditorObject>();
			foreach (var node in savedNodes)
			{
				nodeObjects[node.index] = node;
				if (node.index > lastNodeIndex)
					lastNodeIndex = node.index;
			}
		}

		private void AddNewNode(NodeEditorObject newNode, int nodeIndex)
		{
			nodeObjects.Add(nodeIndex, newNode);
			savedNodes.Add(newNode);
			save = true;
		}


		/// <summary>
		/// Only called when first constructed.
		/// </summary>
		/// <param name="controllerFilepath">Path to new OhBehaveTreeController</param>
		public void Initialize(string controllerFilepath)
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
					+ Path.GetFileNameWithoutExtension(controllerFilepath)
					+ GetInstanceID() + ".asset");

			ohBehaveTree = CreateInstance<OhBehaveTreeController>();
			AssetDatabase.Refresh();
			ohBehaveTree.Initialize(controllerFilepath);
			string blueprintGUID = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(this));
			controllerGUID = AssetDatabase.AssetPathToGUID(controllerFilepath);
			ohBehaveTree.blueprintGUID = blueprintGUID;

			savedNodes = new List<NodeEditorObject>();

			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
			EditorUtility.SetDirty(ohBehaveTree);
			EditorUtility.SetDirty(this);
		}

		public void FindYourControllerDumbass()
		{
			ohBehaveTree =
				AssetDatabase.LoadAssetAtPath<OhBehaveTreeController>(
					AssetDatabase.GUIDToAssetPath(controllerGUID));
		}
	}
}