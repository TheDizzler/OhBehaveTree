using System.IO;
using UnityEditor;
using UnityEngine;

namespace AtomosZ.OhBehave.EditorTools.CustomEditors
{
	[CustomEditor(typeof(OhBehaveAI))]
	[CanEditMultipleObjects]
	public class AIOhBehaveEditor : Editor
	{
		private const string DefaultNodeFolder = "OhBehaveNodes";
		private const string UserNodeFolderKey = "UserNodeFolder";

		private string userNodeFolder;
		private OhBehaveAI instance;
		private SerializedProperty aiBehaviourTree;
		private Editor cachedEditor;



		public void OnEnable()
		{
			instance = (OhBehaveAI)target;
			aiBehaviourTree = serializedObject.FindProperty("ohBehaveAI");

			userNodeFolder = EditorPrefs.GetString(UserNodeFolderKey, "");
			if (userNodeFolder == "")
			{
				userNodeFolder = DefaultNodeFolder;
			}
		}


		public override void OnInspectorGUI()
		{
			GUI.enabled = false;
			EditorGUILayout.ObjectField("Script", target, typeof(OhBehaveAI), false);
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

					string nodename = "NewOhBehaveTree";
					int num = AssetDatabase.FindAssets(nodename, new string[] { "Assets/" + userNodeFolder }).Length;
					if (num != 0)
					{
						nodename += " (" + num + ")";
					}

					var path = EditorUtility.SaveFilePanelInProject(
						"Create New Behavior State Machine", nodename, "asset",
						"Where to save Behavior State Machine?", "Assets/" + userNodeFolder);
					if (path.Length != 0)
					{
						// check if user is using a folder that isn't the default
						if (Path.GetFileName(Path.GetDirectoryName(path)) != userNodeFolder)
						{
							userNodeFolder = Path.GetFileName(Path.GetDirectoryName(path));
							EditorPrefs.SetString(UserNodeFolderKey, userNodeFolder);
						}

						var machineBlueprint = CreateInstance<OhBehaveTreeBlueprint>();
						machineBlueprint.Initialize(path);

						aiBehaviourTree.objectReferenceValue = machineBlueprint.ohBehaveTree;

						EditorWindow.GetWindow<OhBehaveEditorWindow>().Open(machineBlueprint.ohBehaveTree);
					}
				}
			}
			else
			{
				// do something here to verify tree is well-formed. If not, display angry button.

				if (GUILayout.Button("Open AIOhBehaveEditor"))
				{
					EditorWindow.GetWindow<OhBehaveEditorWindow>().Open(
						(OhBehaveTreeController)aiBehaviourTree.objectReferenceValue);
				}
			}
			if (aiBehaviourTree.objectReferenceValue != null)
			{
				((OhBehaveTreeController)aiBehaviourTree.objectReferenceValue).functionSource =
					instance.gameObject;
			}

			serializedObject.ApplyModifiedProperties();
		}
	}
}