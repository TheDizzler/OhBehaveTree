using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AtomosZ.OhBehave.CustomEditors
{
	public class OhBehaveEditorWindow : EditorWindow
	{
		internal static string editorNodePath = "Assets/OhBehaveTree/Editor/EditorNodes";

		[SerializeField]
		public OhBehaveStateMachineController stateMachine;
		public List<NodeWindow> parentlessNodes = new List<NodeWindow>();

		internal NodeStyle selectorNodeStyle;
		internal NodeStyle sequenceNodeStyle;
		internal NodeStyle LeafNodeStyle;
		internal GUIStyle inPointStyle;
		internal GUIStyle outPointStyle;
		internal NodeTreeEditor nodeTree;

		private OhBehaveEditorWindow window;
		private Vector2 scrollPos;


		private void OnEnable()
		{
			window = EditorWindow.GetWindow<OhBehaveEditorWindow>();
			window.titleContent = new GUIContent("OhBehave!");

			selectorNodeStyle = new NodeStyle();
			selectorNodeStyle.Init(
				EditorGUIUtility.Load("builtin skins/darkskin/images/node2.png") as Texture2D,
				EditorGUIUtility.Load("builtin skins/darkskin/images/node2 on.png") as Texture2D);
			sequenceNodeStyle = new NodeStyle();
			sequenceNodeStyle.Init(
				EditorGUIUtility.Load("builtin skins/darkskin/images/node0.png") as Texture2D,
				EditorGUIUtility.Load("builtin skins/darkskin/images/node0 on.png") as Texture2D);
			LeafNodeStyle = new NodeStyle();
			LeafNodeStyle.Init(
				EditorGUIUtility.Load("builtin skins/darkskin/images/node1.png") as Texture2D,
				EditorGUIUtility.Load("builtin skins/darkskin/images/node1 on.png") as Texture2D);

			inPointStyle = new GUIStyle();
			inPointStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/radio.png") as Texture2D;
			inPointStyle.active.background = EditorGUIUtility.Load("builtin skins/darkskin/images/radio on.png") as Texture2D;

			outPointStyle = new GUIStyle();
			outPointStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/radio.png") as Texture2D;
			outPointStyle.active.background = EditorGUIUtility.Load("builtin skins/darkskin/images/radio on.png") as Texture2D;
		}


		public void Open(OhBehaveStateMachineController stateMachine)
		{
			nodeTree = GetNodeTreeFor(stateMachine);

			if (nodeTree == null)
			{

				if (!AssetDatabase.IsValidFolder(editorNodePath))
				{
					string guid = AssetDatabase.CreateFolder("Assets/OhBehaveTree/Editor", "EditorNodes");
					editorNodePath = AssetDatabase.GUIDToAssetPath(guid);
				}

				nodeTree = ScriptableObject.CreateInstance<NodeTreeEditor>();
				AssetDatabase.CreateAsset(nodeTree, editorNodePath + "/NTE_" + nodeTree.GetInstanceID() + ".asset");
				nodeTree.aiStateMachine = stateMachine;
			}

			nodeTree.ConstructTree();

			window.Show();
		}

		//internal NodeWindow CreateNewNodeWindow(CompositeNodeWindow parent, INode node)
		//{
		//	return nodeTree.CreateNewNodeWindow(parent, node);
		//}


		private NodeTreeEditor GetNodeTreeFor(OhBehaveStateMachineController stateMachine)
		{
			string[] guids = AssetDatabase.FindAssets("NTE_", new string[] { "Assets/OhBehaveTree/Editor" });
			for (int i = 0; i < guids.Length; i++)
			{
				string path = AssetDatabase.GUIDToAssetPath(guids[i]);
				NodeTreeEditor temp = AssetDatabase.LoadAssetAtPath(path, typeof(NodeTreeEditor)) as NodeTreeEditor;
				if (temp != null && temp.aiStateMachine == stateMachine)
					return temp;
			}

			return null;
		}


		private void OnGUI()
		{
			if (nodeTree == null)
			{
				if (Selection.activeGameObject == null)
					return;
				var ai = Selection.activeGameObject.GetComponent<AIOhBehave>();
				if (ai == null || ai.ai == null)
				{
					return;
				}

				Open(ai.ai);
				return;
			}

			nodeTree.OnGui(Event.current);
			if (GUI.changed)
				Repaint();
		}

	}
}