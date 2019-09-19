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
				children.Add(ohBehave.CreateNewNodeWindow(
						this,
						rect.center + new Vector2(-rect.width - 50 + children.Count * 50, rect.height - 50),
						node));
			}
		}


		private void CreateChildNode(NodeType type)
		{
			switch (type)
			{
				case NodeType.Leaf:
					var newWindow = ohBehave.CreateNewNodeWindow(
						this,
						rect.center + new Vector2(-rect.width - 50 + children.Count * 50, rect.height - 50),
						(LeafNode)ScriptableObject.CreateInstance(typeof(LeafNode)));
					if (newWindow != null)
						children.Add(newWindow);
					break;
				case NodeType.Selector:
					newWindow = ohBehave.CreateNewNodeWindow(
						this,
						rect.center + new Vector2(-rect.width - 50 + children.Count * 50, rect.height - 50),
						(SelectorNode)ScriptableObject.CreateInstance(typeof(SelectorNode)));
					if (newWindow != null)
						children.Add(newWindow);
					break;
				case NodeType.Sequence:
					newWindow = ohBehave.CreateNewNodeWindow(
						this,
						rect.center + new Vector2(-rect.width - 50 + children.Count * 50, rect.height - 50),
						(SequenceNode)ScriptableObject.CreateInstance(typeof(SequenceNode)));
					if (newWindow != null)
						children.Add(newWindow);
					break;
			}
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