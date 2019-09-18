using UnityEditor;
using UnityEngine;

namespace AtomosZ.OhBehave.CustomEditors
{
	[CustomEditor(typeof(AIOhBehave))]
	[CanEditMultipleObjects]
	public class AIOhBehaveEditor : Editor
	{
		private AIOhBehave instance;
		private SerializedProperty aiBehaviourTree;
		private Editor cachedEditor;


		public void OnEnable()
		{
			instance = (AIOhBehave)target;
			aiBehaviourTree = serializedObject.FindProperty("ai");
			//cachedEditor = Editor.CreateEditor(target);
		}


		public override void OnInspectorGUI()
		{
			GUI.enabled = false;
			EditorGUILayout.ObjectField("Script", target, typeof(AIOhBehave), false);
			GUI.enabled = true;


			EditorGUILayout.PropertyField(aiBehaviourTree);
			

			if (aiBehaviourTree.objectReferenceValue == null)
			{
				if (GUILayout.Button("Create AI Tree"))
				{
					var path = EditorUtility.SaveFilePanelInProject(
						"Create New Node Root", "NewNode", "asset", "Where to save node?");
					if (path.Length != 0)
					{
						//var node = new SelectorNode();
						var node = CreateInstance<SelectorNode>();
						aiBehaviourTree.objectReferenceValue = node;
						AssetDatabase.CreateAsset(node, path);
						//OhBehaveEditorWindow.ShowWindow();
						EditorWindow.GetWindow<OhBehaveEditorWindow>().Open(node);
					}
				}
			}

			serializedObject.ApplyModifiedProperties();
		}
	}
}