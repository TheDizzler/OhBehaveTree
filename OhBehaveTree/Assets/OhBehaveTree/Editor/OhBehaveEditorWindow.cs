using UnityEditor;
using UnityEngine;

namespace AtomosZ.OhBehave.CustomEditors
{
	public class OhBehaveEditorWindow : EditorWindow
	{
		public static int NextWindowID = 0;

		[SerializeField]
		public OhBehaveStateMachineController stateMachine;
		private CompositeNodeWindow rootNodeWindow;
		private OhBehaveEditorWindow window;
		private Vector2 scrollPos;

		//[MenuItem("Window/OhBehave")]
		//static public void ShowWindow()
		//{
		//	var window = EditorWindow.GetWindow<OhBehaveEditorWindow>();
		//	window.titleContent = new GUIContent("OhBehave!");
		//}

		public void Open(OhBehaveStateMachineController stateMachine)
		{
			window = EditorWindow.GetWindow<OhBehaveEditorWindow>();
			window.titleContent = new GUIContent("OhBehave!");
			switch (stateMachine.parentNode.GetNodeType())
			{
				case NodeType.Leaf:
					Debug.LogError("Cannot build a Behaviour tree on top of a leaf!");
					return;
				case NodeType.Selector:
					rootNodeWindow = new SelectorNodeWindow(null, new Vector2(position.width / 2, 0), (SelectorNode)stateMachine.parentNode);
					break;
				case NodeType.Sequence:
					rootNodeWindow = new SequenceNodeWindow(null, new Vector2(position.width / 2, 0), (SequenceNode)stateMachine.parentNode);
					break;
			}
		}

		internal NodeWindow CreateNewNodeWindow(CompositeNodeWindow parent, Vector2 pos, INode node)
		{
			switch (node.GetNodeType())
			{
				case NodeType.Selector:
					return new SelectorNodeWindow(parent, pos, (SelectorNode)node);
				case NodeType.Sequence:
					return new SequenceNodeWindow(parent, pos, (SequenceNode)node);
				case NodeType.Leaf:
					return new LeafNodeWindow(parent, pos, (LeafNode)node);
				default:
					Debug.LogError("Was a NodeType not implemented?");
					return null;
			}
		}

		void OnGUI()
		{
			if (rootNodeWindow == null)
			{
				if (Selection.activeGameObject == null)
					return;
				var ai = Selection.activeGameObject.GetComponent<AIOhBehave>();
				if (ai == null || ai.ai == null)
				{
					return;
				}

				Open(ai.ai);
			}

			scrollPos = EditorGUILayout.BeginScrollView(scrollPos, false, false);

			BeginWindows();
			rootNodeWindow.OnGUI();
			EndWindows();
			//float height = 50;
			//Rect rect = new Rect(0, 0, position.width, height);
			//GUILayout.BeginArea(rect);
			//GUIStyle centeredStyle = GUI.skin.GetStyle("Label");
			//centeredStyle.alignment = TextAnchor.MiddleCenter;
			//GUILayout.Box(rootNode.nodeObject.GetNodeType() + "", centeredStyle);
			//GUILayout.EndArea();
			//rect.y += height;
			//int childCount = rootNode.children.Count;
			//rect.width = position.width / childCount;
			//int i = 0;
			//EditorGUILayout.BeginHorizontal();
			//foreach (NodeWindow node in rootNode.children)
			//{
			//	rect.x += i * rect.width;
			//	GUILayout.BeginArea(rect);
			//	GUILayout.Box(node.nodeObject.GetNodeType() + "");
			//	GUILayout.EndArea();
			//	++i;
			//}
			//EditorGUILayout.EndHorizontal();

			EditorGUILayout.EndScrollView();
		}


	}
}