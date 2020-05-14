using System;
using UnityEditor;
using UnityEngine;

namespace AtomosZ.OhBehave.EditorTools
{
	public abstract class CompositeNodeWindow : NodeWindow, IParentNodeWindow
	{
		public CompositeNodeWindow(NodeEditorObject node) : base(node) { }


		internal override bool ProcessEvents(Event e)
		{
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
							currentStyle = nodeStyle.selectedStyle;
							Selection.SetActiveObjectWithContext(nodeObject, null);
						}
						else
						{
							GUI.changed = true;
							isSelected = false;
							currentStyle = nodeStyle.defaultStyle;
						}
					}
					else if (e.button == 1)
					{
						if (rect.Contains(e.mousePosition))
						{
							treeBlueprint.ProcessContextMenu(this);
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

		public void CreateChildConnection(NodeWindow newChild)
		{
			newChild.CreateConnectionToParent(this);
		}

		public void RemoveChildConnection(NodeWindow childNode)
		{
			//EditorWindow.GetWindow<OhBehaveEditorWindow>().parentlessNodes.Add(childNode);

			throw new NotImplementedException();
		}
	}
}