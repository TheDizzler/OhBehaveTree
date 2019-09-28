using UnityEditor;
using UnityEngine;

namespace AtomosZ.OhBehave.CustomEditors
{
	public class OhBehaveEditorWindow : EditorWindow
	{
		public static int NextWindowID = 0;

		[SerializeField]
		public CompositeNodeWindow rootNode;
		private OhBehaveEditorWindow window;
		private Vector2 scrollPos;

		//[MenuItem("Window/OhBehave")]
		//static public void ShowWindow()
		//{
		//	var window = EditorWindow.GetWindow<OhBehaveEditorWindow>();
		//	window.titleContent = new GUIContent("OhBehave!");
		//}

		public void Open(SelectorNode node)
		{
			window = EditorWindow.GetWindow<OhBehaveEditorWindow>();
			window.titleContent = new GUIContent("OhBehave!");
			rootNode = new SelectorNodeWindow(null, new Vector2(position.width / 2, 0), node);
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
			if (rootNode == null)
			{
				if (Selection.activeGameObject == null)
					return;
				var ai = Selection.activeGameObject.GetComponent<AIOhBehave>();
				if (ai == null || ai.ai == null)
				{
					return;
				}

				Open((SelectorNode)ai.ai);
			}

			scrollPos = EditorGUILayout.BeginScrollView(scrollPos, false, false);

			BeginWindows();
			rootNode.OnGUI();
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