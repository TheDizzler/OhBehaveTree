using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AtomosZ.OhBehave.EditorTools.CustomEditors
{
	[CustomEditor(typeof(OhBehaveTreeBlueprint))]
	public class OhBehaveBlueprintEditor : Editor
	{
		private OhBehaveTreeBlueprint instance;
		private SerializedProperty nodeList;
		private OhBehaveActions source;
		private OhBehaveTreeController treeController;

		void OnEnable()
		{
			instance = (OhBehaveTreeBlueprint)target;
			nodeList = serializedObject.FindProperty("savedNodes");
			source = instance.ohBehaveTree.behaviorSource;
			treeController = instance.ohBehaveTree;
		}


		public override void OnInspectorGUI()
		{
			if (instance.IsNodeSelected())
			{
				DrawSelectedNode();
			}
			else
			{
				if (instance.ohBehaveTree == null)
				{
					Debug.LogWarning("POS blueprint lost track of it's controller");
					instance.FindYourControllerDumbass();
				}
				else
					DrawBlueprintEditor();
			}

			serializedObject.ApplyModifiedProperties();
		}


		private void DrawBlueprintEditor()
		{
			//base.OnInspectorGUI();
			GUI.enabled = false;
			EditorGUILayout.ObjectField("Script", target, typeof(OhBehaveAI), false);
			EditorGUILayout.ObjectField("OhBehaveTree", instance.ohBehaveTree, typeof(OhBehaveTreeController), false);
			EditorGUILayout.PropertyField(nodeList, true);
			GUI.enabled = true;

			EditorGUI.BeginChangeCheck();
			EditorGUILayout.ObjectField("Behaviour Source", source, typeof(OhBehaveActions), true);


			serializedObject.ApplyModifiedProperties();

			if (EditorGUI.EndChangeCheck())
			{
				Debug.LogWarning("Data source changed! Probably need some kind of popup warning here.");
			}

			if (GUILayout.Button("Open In Editor"))
			{
				EditorWindow.GetWindow<OhBehaveEditorWindow>().Open(
					instance.ohBehaveTree);
			}

		}


		private void DrawSelectedNode()
		{
			var nodeObject = instance.GetSelectedNode();

			GUILayout.BeginVertical();
			{
				GUILayout.BeginHorizontal();
				{
					GUILayout.Label(
						new GUIContent(nodeObject.displayName + " - " + Enum.GetName(typeof(NodeType),
							nodeObject.nodeType)),
						NodeStyle.LeafLabelStyle
					);
				}
				GUILayout.EndHorizontal();

				GUILayout.BeginHorizontal();
				{
					GUI.enabled = false;
					EditorGUILayout.IntField("Reference index", nodeObject.index);
					EditorGUILayout.IntField("Parent ref index", nodeObject.parentIndex);
					GUI.enabled = true;
				}
				GUILayout.EndHorizontal();

				GUILayout.BeginHorizontal();
				{
					EditorGUIUtility.labelWidth = 40f;
					string newName = EditorGUILayout.DelayedTextField("Display Name", nodeObject.displayName);
					if (newName != nodeObject.displayName)
					{
						nodeObject.displayName = newName;
						nodeObject.RefreshParent();
					}
				}
				GUILayout.EndHorizontal();

				EditorGUILayout.LabelField("Description:");
				string newDesc = EditorGUILayout.DelayedTextField(nodeObject.description);
				if (newDesc != nodeObject.description)
					nodeObject.description = newDesc;


				NodeType newType = (NodeType)EditorGUILayout.EnumPopup(nodeObject.nodeType);
				if (newType != nodeObject.nodeType)
				{
					nodeObject.ChangeNodeType(newType);
				}

				EditorGUIUtility.labelWidth = 0;

				var selectedNodeProperty = serializedObject.FindProperty("selectedNode");
				switch (nodeObject.nodeType)
				{
					case NodeType.Leaf:
						EditorGUILayout.LabelField("What do actions?");
						GUI.enabled = false;
						EditorGUILayout.ObjectField("Behaviour Source", source, typeof(OhBehaveActions), true);
						GUI.enabled = true;
						EditorGUILayout.ObjectField(nodeObject.actionMethod); // gotta figure out how to connect this to UnityEvent?

						if (treeController.methods != null && treeController.methods.Count > 0)
						{
							// Create the dropdown in the inspector for the found methods
							List<string> methodNames = new List<string>();
							foreach (var method in treeController.methods)
							{
								methodNames.Add(method.ToString());
							}

							EditorGUILayout.Popup("Function List", 0, methodNames.ToArray());
						}

						break;
					case NodeType.Selector:
					case NodeType.Sequence:
						GUI.enabled = false;
						EditorGUILayout.PropertyField(selectedNodeProperty.FindPropertyRelative("children"), true);
						GUI.enabled = true;
						break;
				}
			}
			GUILayout.EndVertical();
		}
	}
}