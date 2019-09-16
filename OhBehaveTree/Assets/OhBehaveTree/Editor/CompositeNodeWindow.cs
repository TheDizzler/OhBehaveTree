using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AtomosZ.OhBehave.Editor
{
	public abstract class CompositeNodeWindow : NodeWindow
	{
		[SerializeField]
		protected List<NodeWindow> children = new List<NodeWindow>();

		public CompositeNodeWindow(NodeWindow parent, Rect rct) : base(parent, rct)
		{
		}


		private void AddChildNode(NodeType type)
		{
			switch (type)
			{
				case NodeType.Leaf:
					children.Add(new LeafNodeWindow(this,
						rect.center + new Vector2(-rect.width - 50 + children.Count * 50, rect.height - 50)));
					break;
				case NodeType.Selector:
					children.Add(new SelectorNodeWindow(this,
						rect.center + new Vector2(-rect.width - 50 + children.Count * 50, rect.height - 50)));
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