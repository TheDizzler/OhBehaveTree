using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace AtomosZ.OhBehave.CustomEditors
{
	public abstract class CompositeNodeWindow : NodeWindow
	{
		public List<NodeWindow> children = new List<NodeWindow>();
		public ConnectionPoint outPoint;


		public CompositeNodeWindow(CompositeNodeWindow parent, Rect rct, ICompositeNode nodeObj,
			GUIStyle defaultStyle, GUIStyle selectedStyle, GUIStyle inPointStyle, GUIStyle outPointStyle,
			Action<ConnectionPoint> OnClickInPoint, Action<ConnectionPoint> OnClickOutPoint)
				: base(parent, rct, nodeObj, defaultStyle, selectedStyle, inPointStyle, OnClickInPoint)
		{
			outPoint = new ConnectionPoint(this, ConnectionPointType.Out, outPointStyle, OnClickOutPoint);
			foreach (INode node in nodeObj.children)
			{
				children.Add(
					ohBehave.CreateNewNodeWindow(this,
						rect.center + new Vector2(-rect.width - 50 + children.Count * 50, rect.height),
						node));

			}
		}

		internal override bool ProcessEvents(Event e)
		{
			foreach (NodeWindow node in children)
			{
				if (node.ProcessEvents(e))
					GUI.changed = true;
			}

			switch (e.type)
			{
				case EventType.MouseDown:
					if (e.button == 0)
					{
						if (rect.Contains(e.mousePosition))
						{
							isDragged = true;
							GUI.changed = true;
							isSelected = true;
							currentStyle = selectedStyle;
							Selection.SetActiveObjectWithContext(nodeObject, null);
						}
						else
						{
							GUI.changed = true;
							isSelected = false;
							currentStyle = defaultStyle;
						}
					}
					break;
				case EventType.MouseUp:
					isDragged = false;
					break;
				case EventType.MouseDrag:
					if (e.button == 0 && isDragged)
					{
						Drag(e.delta);
						e.Use();
						return true;
					}
					break;
			}
			return false;
		}

		internal void CreateChildConnection(NodeWindow newChild)
		{
			children.Add(newChild);
			newChild.CreateConnectionToParent(this);
		}

		internal void RemoveChildConnection(NodeWindow childNode)
		{
			children.Remove(childNode);
			ohBehave.parentlessNodes.Add(childNode);
		}

		protected override void OnGUIExtra()
		{
			outPoint.Draw();
			foreach (NodeWindow node in children)
			{
				node.OnGUI();
				//DrawNodeCurve(this, node);
			}
		}

		//internal override void OnGUI()
		//{
		//	inPoint.Draw();
		//	outPoint.Draw();
		//	if (connectionToParent != null)
		//		connectionToParent.Draw();
		//	GUI.backgroundColor = bgColor;
		//	rect = GUI.Window(windowID, rect, DrawWindow, nodeName);
		//	foreach (NodeWindow node in children)
		//	{
		//		node.OnGUI();
		//		DrawNodeCurve(this, node);
		//	}
		//}

		/// <summary>
		/// Creates a user generated child node for this node.
		/// </summary>
		/// <param name="type"></param>
		internal void CreateChildNode(NodeType type)
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