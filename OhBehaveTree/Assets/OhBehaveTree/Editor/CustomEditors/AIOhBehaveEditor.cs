using System.IO;
using UnityEditor;
using UnityEngine;

namespace AtomosZ.OhBehave.EditorTools.CustomEditors
{
	[CustomEditor(typeof(OhBehaveAI))]
	[CanEditMultipleObjects]
	public class AIOhBehaveEditor : Editor
	{
		public static string userNodeFolder;

		private const string DefaultNodeFolder = "OhBehaveNodes";
		private const string UserNodeFolderKey = "UserNodeFolder";

		private OhBehaveAI instance;



		public void OnEnable()
		{
			instance = (OhBehaveAI)target;

			userNodeFolder = EditorPrefs.GetString(UserNodeFolderKey, "");
			if (userNodeFolder == "")
			{
				userNodeFolder = DefaultNodeFolder;
			}
		}


		public override void OnInspectorGUI()
		{
			serializedObject.Update();
			GUI.enabled = false;
			EditorGUILayout.ObjectField("Script", target, typeof(OhBehaveAI), false);
			EditorGUILayout.DelayedTextField("Tree file", Path.GetFileName(instance.jsonFilepath));
			GUI.enabled = true;
			

			if (!string.IsNullOrEmpty(instance.jsonFilepath))
			{
				if (GUILayout.Button("Change json file"))
				{ // If FileChooser is opened here, will get EditorLayout Error.
					EditorWindow.GetWindow<OhBehaveEditorWindow>().openFileChooser = true;
				}

				// do something here to verify tree is well-formed. If not, display angry button.

				if (GUILayout.Button("Open AIOhBehaveEditor"))
				{
					if (!EditorWindow.GetWindow<OhBehaveEditorWindow>().Open(instance))
					{
						// couldn't find jsonfile. Should we save GUID of blueprint?
						instance.jsonFilepath = "";
						CreateNewJson();
					}
				}
			}

			serializedObject.ApplyModifiedProperties();
		}

		private void CreateNewJson()
		{
			Debug.Log(Application.streamingAssetsPath + "/" + userNodeFolder);
			if (!AssetDatabase.IsValidFolder("Assets/StreamingAssets/" + userNodeFolder))
			{
				if (!AssetDatabase.IsValidFolder("Assets/StreamingAssets/"))
					AssetDatabase.CreateFolder("Assets", "StreamingAssets");
				if (!AssetDatabase.IsValidFolder("Assets/StreamingAssets/" + DefaultNodeFolder))
					AssetDatabase.CreateFolder("Assets/StreamingAssets", DefaultNodeFolder);
				userNodeFolder = DefaultNodeFolder;
				EditorPrefs.SetString(UserNodeFolderKey, userNodeFolder);
			}

			string nodename = "NewOhBehaveTree";
			int num = AssetDatabase.FindAssets(nodename, new string[] { "Assets/StreamingAssets/" + userNodeFolder }).Length;
			if (num != 0)
			{
				nodename += " (" + num + ")";
			}

			var path = EditorUtility.SaveFilePanelInProject(
				"Create New Json Behavior State Machine", nodename, "OhJson",
				"Where to save json file?", "Assets/StreamingAssets/" + userNodeFolder);
			if (path.Length != 0)
			{
				// check if user is using a folder that isn't the default
				if (Path.GetFileName(Path.GetDirectoryName(path)) != userNodeFolder)
				{
					userNodeFolder = Path.GetFileName(Path.GetDirectoryName(path));
					EditorPrefs.SetString(UserNodeFolderKey, userNodeFolder);
				}

				var machineBlueprint = CreateInstance<OhBehaveTreeBlueprint>();
				machineBlueprint.Initialize(instance, path);

				EditorWindow.GetWindow<OhBehaveEditorWindow>().Open(instance);
			}
		}
	}
}