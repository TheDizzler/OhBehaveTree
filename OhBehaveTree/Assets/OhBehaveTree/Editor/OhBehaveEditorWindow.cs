using UnityEditor;
using UnityEngine;

namespace AtomosZ.OhBehave.CustomEditors
{
	public class OhBehaveEditorWindow : EditorWindow
	{
		public static int NextWindowID = 0;

		[SerializeField]
		CompositeNodeWindow rootNode;


		//[MenuItem("Window/OhBehave")]
		//static public void ShowWindow()
		//{
		//	var window = EditorWindow.GetWindow<OhBehaveEditorWindow>();
		//	window.titleContent = new GUIContent("OhBehave!");
		//}

		public void Open(SelectorNode node)
		{
			rootNode = (CompositeNodeWindow)CreateNode(null, new Vector2(position.width / 2, 0), node);
		}

		internal NodeWindow CreateNode(NodeWindow parent, Vector2 pos, INode node)
		{
			switch (node.GetNodeType())
			{
				case NodeType.Selector:
					return new SelectorNodeWindow(parent, pos, node);
				case NodeType.Sequence:
					return new SequenceNodeWindow(parent, pos, node);
				case NodeType.Leaf:
					return new LeafNodeWindow(parent, pos, node);
			}

			return null;
		}


		public void OnEnable()
		{
			Debug.Log("OnEnable");
		}


		void OnGUI()
		{
			BeginWindows();
			rootNode.OnGUI();
			EndWindows();
		}


	}
}