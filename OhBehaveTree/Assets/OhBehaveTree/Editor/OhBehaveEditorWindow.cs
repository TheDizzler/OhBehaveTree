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

			BeginWindows();
			rootNode.OnGUI();
			EndWindows();
			//EditorGUILayout.BeginHorizontal();
			//GUILayout.Box(rootNode.nodeObject.GetNodeType() + "");
		}


	}
}