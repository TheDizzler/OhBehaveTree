using System.IO;
using UnityEditor;
using UnityEngine;

namespace AtomosZ.OhBehave.CustomEditors
{
	[CustomEditor(typeof(AIOhBehave))]
	[CanEditMultipleObjects]
	public class AIOhBehaveEditor : Editor
	{
		private const string DefaultNodeFolder = "OhBehaveNodes";
		private const string UserNodeFolderKey = "UserNodeFolder";

		private string userNodeFolder;
		private AIOhBehave instance;
		private SerializedProperty aiBehaviourTree;
		private Editor cachedEditor;



		public void OnEnable()
		{
			instance = (AIOhBehave)target;
			aiBehaviourTree = serializedObject.FindProperty("ai");

			userNodeFolder = EditorPrefs.GetString(UserNodeFolderKey, "");
			if (userNodeFolder == "")
			{
				userNodeFolder = DefaultNodeFolder;
			}
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
					if (!AssetDatabase.IsValidFolder("Assets/" + userNodeFolder))
					{
						if (!AssetDatabase.IsValidFolder("Assets/" + DefaultNodeFolder))
							AssetDatabase.CreateFolder("Assets", DefaultNodeFolder);
						userNodeFolder = DefaultNodeFolder;
						EditorPrefs.SetString(UserNodeFolderKey, userNodeFolder);
					}

					string nodename = "NewOhBehaveStateMachine";
					int num = AssetDatabase.FindAssets(nodename, new string[] { "Assets/" + userNodeFolder }).Length;
					if (num != 0)
					{
						nodename += " (" + num + ")";
					}

					var path = EditorUtility.SaveFilePanelInProject(
						"Create New Node Root", nodename, "asset", "Where to save node?", "Assets/" + userNodeFolder);
					if (path.Length != 0)
					{
						// check if user is using a folder that isn't the default
						if (Path.GetFileName(Path.GetDirectoryName(path)) != userNodeFolder)
						{
							userNodeFolder = Path.GetFileName(Path.GetDirectoryName(path));
							EditorPrefs.SetString(UserNodeFolderKey, userNodeFolder);
						}

						var statemachine = CreateInstance<OhBehaveStateMachineController>();
						statemachine.Initialize(path);
						aiBehaviourTree.objectReferenceValue = statemachine;
						EditorWindow.GetWindow<OhBehaveEditorWindow>().Open(statemachine);
					}
				}
			}
			else if (GUILayout.Button("Open AIOhBehaveEditor"))
			{
				EditorWindow.GetWindow<OhBehaveEditorWindow>().Open(
					(OhBehaveStateMachineController)aiBehaviourTree.objectReferenceValue);
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