using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AtomosZ.OhBehave.CustomEditors
{
	public abstract class CompositeNodeWindow : NodeWindow
	{
		[SerializeField]
		public List<NodeWindow> children = new List<NodeWindow>();


		public CompositeNodeWindow(NodeWindow parent, Rect rct, ICompositeNode nodeObj)
			: base(parent, rct, nodeObj)
		{
			foreach (INode node in nodeObj.children)
			{
				children.Add(ohBehave.CreateNewNodeWindow(this,
					rect.center + new Vector2(-rect.width - 50 + children.Count * 50, rect.height),
					node));
			}
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

			var dir = Path.GetDirectoryName(AssetDatabase.GetAssetPath(nodeObject));
			string nodename = "New" + type;
			int num = AssetDatabase.FindAssets(nodename, new string[] { dir }).Length;
			if (num != 0)
			{
				nodename += " (" + num + ")";
			}

			var path = EditorUtility.SaveFilePanelInProject(
				"Create New Node Root", nodename, "asset", "Where to save node?", dir);

			if (path.Length != 0)
			{
				AssetDatabase.CreateAsset(node, path);
				node.parent = nodeObject;
				((ICompositeNode)nodeObject).children.Add(node);
				EditorUtility.SetDirty(node);
				EditorUtility.SetDirty(nodeObject);
				return node;
			}

			return null;
		}

		/// <summary>
		/// Creates a user generated child node for this node.
		/// </summary>
		/// <param name="type"></param>
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