using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AtomosZ.OhBehave.CustomEditors
{
	public abstract class CompositeNodeWindow : NodeWindow
	{
		[SerializeField]
		protected List<NodeWindow> children = new List<NodeWindow>();


		public CompositeNodeWindow(NodeWindow parent, Rect rct, ICompositeNode nodeObj)
			: base(parent, rct, nodeObj)
		{
			foreach (INode node in nodeObj.children)
			{
				children.Add(ohBehave.CreateNewNodeWindow(this,
					rect.center + new Vector2(-rect.width - 50 + children.Count * 50, rect.height - 50),
					node));
			}
		}


		private void CreateChildNode(NodeType type)
		{
			INode newnode = CreateNewNode(type);
			if (newnode == null)
				return;
			var newWindow = ohBehave.CreateNewNodeWindow(this,
				rect.center + new Vector2(-rect.width - 50 + children.Count * 50, rect.height - 50),
				newnode);
			if (newWindow != null)
				children.Add(newWindow);
		}

		private INode CreateNewNode(NodeType type)
		{
			INode node;
			switch (type)
			{
				case NodeType.Leaf:
					node = (LeafNode)ScriptableObject.CreateInstance(typeof(LeafNode));
					break;
				case NodeType.Selector:
					node = (SelectorNode)ScriptableObject.CreateInstance(typeof(SelectorNode));
					break;
				case NodeType.Sequence:
					node = (SequenceNode)ScriptableObject.CreateInstance(typeof(SequenceNode));
					break;
				default:
					Debug.LogError("Code not creat node of type " + type);
					return null;
			}

			var path = EditorUtility.SaveFilePanelInProject(
				"Create New Node Root", "New" + type, "asset", "Where to save node?");
			if (path.Length != 0)
			{
				AssetDatabase.CreateAsset(node, path);
				node.parent = parent.nodeObject;
				((ICompositeNode)parent.nodeObject).children.Add(node);
				//return NewWindow(parent, pos, node);
				return node;
			}

			return null;
		}


		public class NodeTypeSelectPopup : PopupWindowContent
		{
			public static Rect PopupRect = new Rect(-100, -75, 200, 150);

			private CompositeNodeWindow parentWindow;


			public NodeTypeSelectPopup(CompositeNodeWindow selectorNodeWindow)
			{
				this.parentWindow = selectorNodeWindow;
			}


			public override void OnGUI(Rect rect)
			{
				GUILayout.Label("Choose child node type", EditorStyles.boldLabel);
				if (GUILayout.Button("Leaf"))
				{
					parentWindow.CreateChildNode(NodeType.Leaf);
					EditorWindow.GetWindow<PopupWindow>().Close();
				}
				else if (GUILayout.Button("Selector"))
				{
					parentWindow.CreateChildNode(NodeType.Selector);
					EditorWindow.GetWindow<PopupWindow>().Close();
				}
				else if (GUILayout.Button("Sequence"))
				{
					parentWindow.CreateChildNode(NodeType.Sequence);
					EditorWindow.GetWindow<PopupWindow>().Close();
				}
			}
		}
	}
}