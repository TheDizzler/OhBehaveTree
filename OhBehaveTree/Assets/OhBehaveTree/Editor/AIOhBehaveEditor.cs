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
						"Create New Node Root", "NewRootNode", "asset", "Where to save node?");
					if (path.Length != 0)
					{
						var node = CreateInstance<SelectorNode>();
						aiBehaviourTree.objectReferenceValue = node;
						AssetDatabase.CreateAsset(node, path);
						EditorWindow.GetWindow<OhBehaveEditorWindow>().Open(node);
					}
				}
			}
			else if (GUILayout.Button("Open AIOhBehaveEditor"))
				{
				EditorWindow.GetWindow<OhBehaveEditorWindow>().Open((SelectorNode)aiBehaviourTree.objectReferenceValue);
			}
			//else if (clickArea.Contains(Event.current.mousePosition) && Event.current.type == EventType.MouseDown)
			//{
			//	EditorWindow.GetWindow<OhBehaveEditorWindow>().Open((SelectorNode)aiBehaviourTree.objectReferenceValue);
			//}
			//else
			//{
			//	var picked = EditorGUIUtility.ObjectContent(aiBehaviourTree.objectReferenceValue, typeof(SelectorNode));
			//	if (Event.current.type == EventType.MouseDown)
			//		Debug.Log(Event.current.mousePosition);
			//}

			serializedObject.ApplyModifiedProperties();
		}
	}
}