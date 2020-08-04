using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AtomosZ.OhBehave.EditorTools.CustomEditors
{
	[CustomEditor(typeof(OhBehaveTreeController))]
	public class OhBehaveTreeControllerEditor : Editor
	{
		private OhBehaveTreeController treeController;
		private SerializedProperty treeName;
		private SerializedProperty treeDesc;
		private SerializedProperty behaviorSource;


		private void OnEnable()
		{
			treeController = (OhBehaveTreeController)target;
			treeName = serializedObject.FindProperty("behaviorTreeName");
			treeDesc = serializedObject.FindProperty("description");
			behaviorSource = serializedObject.FindProperty("behaviorSource");
		}


		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			GUI.enabled = false;
			EditorGUILayout.ObjectField("Script", target, typeof(OhBehaveAI), false);
			EditorGUILayout.TextField("Blueprint GUID", treeController.blueprintGUID);
			GUI.enabled = true;


			EditorGUILayout.DelayedTextField(treeName);
			EditorGUILayout.DelayedTextField(treeDesc);

			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PropertyField(behaviorSource);


			if (treeController.sharedMethods != null && treeController.sharedMethods.Count > 0)
			{
				// Create the dropdown in the inspector for the found methods
				EditorGUILayout.Popup("Function List", 0, treeController.privateMethodNames.ToArray());
			}


			serializedObject.ApplyModifiedProperties();
			if (EditorGUI.EndChangeCheck())
			{
				treeController.EditorNeedsRefresh();
			}
		}
	}
}