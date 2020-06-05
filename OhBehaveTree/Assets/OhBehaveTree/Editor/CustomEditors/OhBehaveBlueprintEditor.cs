using System;
using UnityEditor;
using UnityEngine;

namespace AtomosZ.OhBehave.EditorTools.CustomEditors
{
	[CustomEditor(typeof(OhBehaveTreeBlueprint))]
	public class OhBehaveBlueprintEditor : Editor
	{
		private OhBehaveTreeBlueprint instance;


		void OnEnable()
		{
			instance = (OhBehaveTreeBlueprint)target;
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
			GUI.enabled = true;

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
					EditorGUILayout.IntField("Parent reference index", nodeObject.parentIndex);
					GUI.enabled = true;
				}
				GUILayout.EndHorizontal();

				GUILayout.BeginHorizontal();
				{
					EditorGUIUtility.labelWidth = 40f;
					string newName = EditorGUILayout.DelayedTextField("Name", nodeObject.displayName);
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
				EditorGUILayout.PropertyField(selectedNodeProperty.FindPropertyRelative("displayName"));
				switch (nodeObject.nodeType)
				{
					case NodeType.Leaf:
						var startEventProp = selectedNodeProperty.FindPropertyRelative("startEvent");
						EditorGUILayout.PropertyField(startEventProp);

						var actionEventProp = selectedNodeProperty.FindPropertyRelative("actionEvent");
						EditorGUILayout.PropertyField(actionEventProp);
						break;
					case NodeType.Selector:
					case NodeType.Sequence:
						GUI.enabled = false;
						var children = nodeObject.children;
						EditorGUILayout.PropertyField(selectedNodeProperty.FindPropertyRelative("children"));
						GUI.enabled = true;
						break;
				}
			}
			GUILayout.EndVertical();
		}
	}
}