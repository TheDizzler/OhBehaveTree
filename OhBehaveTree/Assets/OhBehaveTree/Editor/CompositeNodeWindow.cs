using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AtomosZ.OhBehave.CustomEditors
{
	public abstract class CompositeNodeWindow : NodeWindow
	{
		[SerializeField]
		protected List<NodeWindow> children = new List<NodeWindow>();


		public CompositeNodeWindow(NodeWindow parent, Rect rct, INode nodeObj)
			: base(parent, rct, nodeObj) { }


		private void AddChildNode(NodeType type)
		{
			switch (type)
			{
				case NodeType.Leaf:
					children.Add(ohBehave.CreateNode(
						this,
						rect.center + new Vector2(-rect.width - 50 + children.Count * 50, rect.height - 50),
						(LeafNode)ScriptableObject.CreateInstance(typeof(LeafNode))));
					break;
				case NodeType.Selector:
					children.Add(ohBehave.CreateNode(
						this,
						rect.center + new Vector2(-rect.width - 50 + children.Count * 50, rect.height - 50),
						(SelectorNode)ScriptableObject.CreateInstance(typeof(SelectorNode))));
					break;
				case NodeType.Sequence:
					children.Add(ohBehave.CreateNode(
						this,
						rect.center + new Vector2(-rect.width - 50 + children.Count * 50, rect.height - 50),
						(SequenceNode)ScriptableObject.CreateInstance(typeof(SequenceNode))));
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
					parentWindow.AddChildNode(NodeType.Leaf);
					EditorWindow.GetWindow<PopupWindow>().Close();
				}
				else if (GUILayout.Button("Selector"))
				{
					parentWindow.AddChildNode(NodeType.Selector);
					EditorWindow.GetWindow<PopupWindow>().Close();
				}
				else if (GUILayout.Button("Sequence"))
				{
					parentWindow.AddChildNode(NodeType.Sequence);
					EditorWindow.GetWindow<PopupWindow>().Close();
				}
			}
		}
	}
}