using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AtomosZ.OhBehave.EditorTools
{
	public class OhBehaveEditorWindow : EditorWindow
	{
		//[SerializeField]
		//public OhBehaveTreeBlueprint treeBlueprint;

		internal static NodeStyle selectorNodeStyle;
		internal static NodeStyle sequenceNodeStyle;
		internal static NodeStyle LeafNodeStyle;
		internal static GUIStyle inPointStyle;
		internal static GUIStyle outPointStyle;

		internal OhBehaveTreeBlueprint treeBlueprint;

		private OhBehaveEditorWindow window;
		private Vector2 scrollPos;
		private OhBehaveTreeController currentTreeController;


		private void OnEnable()
		{
			window = EditorWindow.GetWindow<OhBehaveEditorWindow>();
			window.titleContent = new GUIContent("OhBehave!");

			selectorNodeStyle = new NodeStyle();
			selectorNodeStyle.Init(
				EditorGUIUtility.Load("builtin skins/darkskin/images/node2.png") as Texture2D,
				EditorGUIUtility.Load("builtin skins/darkskin/images/node2 on.png") as Texture2D);
			sequenceNodeStyle = new NodeStyle();
			sequenceNodeStyle.Init(
				EditorGUIUtility.Load("builtin skins/darkskin/images/node0.png") as Texture2D,
				EditorGUIUtility.Load("builtin skins/darkskin/images/node0 on.png") as Texture2D);
			LeafNodeStyle = new NodeStyle();
			LeafNodeStyle.Init(
				EditorGUIUtility.Load("builtin skins/darkskin/images/node1.png") as Texture2D,
				EditorGUIUtility.Load("builtin skins/darkskin/images/node1 on.png") as Texture2D);

			inPointStyle = new GUIStyle();
			inPointStyle.normal.background = (Texture2D)
				EditorGUIUtility.Load("builtin skins/darkskin/images/radio.png");
			inPointStyle.active.background = (Texture2D)
				EditorGUIUtility.Load("builtin skins/darkskin/images/radio on.png");

			outPointStyle = new GUIStyle();
			outPointStyle.normal.background = (Texture2D)
				EditorGUIUtility.Load("builtin skins/darkskin/images/radio.png");
			outPointStyle.active.background = (Texture2D)
				EditorGUIUtility.Load("builtin skins/darkskin/images/radio on.png");

			if (treeBlueprint != null)
			{
				treeBlueprint.ConstructNodes();
			}
		}


		public void Open(OhBehaveTreeController ohBehaveController)
		{
			treeBlueprint = GetBlueprintFor(ohBehaveController);
			if (treeBlueprint == null)
			{
				Debug.LogError("Could not find blueprint");
				return;
			}

			currentTreeController = ohBehaveController;

			treeBlueprint.ConstructNodes();

			window.Show();
		}

		/// <summary>
		/// Because it's not possible to store editor objects in non-editor objects
		/// this rigamoral is needed to find the BehaviorTree blueprint.
		/// </summary>
		/// <param name="ohBehaveController"></param>
		/// <returns></returns>
		private OhBehaveTreeBlueprint GetBlueprintFor(OhBehaveTreeController ohBehaveController)
		{
			string[] guids = AssetDatabase.FindAssets("BTO_", new string[] { OhBehaveTreeBlueprint.blueprintsPath });
			for (int i = 0; i < guids.Length; i++)
			{
				string path = AssetDatabase.GUIDToAssetPath(guids[i]);
				OhBehaveTreeBlueprint temp = (OhBehaveTreeBlueprint)
					AssetDatabase.LoadAssetAtPath(path, typeof(OhBehaveTreeBlueprint));
				if (temp != null && temp.ohBehaveTree == ohBehaveController)
					return temp;
			}

			return null;
		}



		void OnGUI()
		{
			if (Selection.activeGameObject != null)
			{
				OhBehaveAI ohBehaveSM = Selection.activeGameObject.GetComponent<OhBehaveAI>();
				if (ohBehaveSM != null)
				{
					var ohBehaveController = ohBehaveSM.ohBehaveAI;
					if (ohBehaveController != currentTreeController)
					{ // switch to the currently selected gameobjects behavior tree
						Open(ohBehaveController);
						return;
					}
				}
			}


			//if (nodeTree == null)
			//{
			//	if (Selection.activeGameObject == null)
			//		return;


			//	var ai = Selection.activeGameObject.GetComponent<BehaviorStateMachine>();
			//	if (ai == null || ai.ohBehaveAI == null)
			//	{
			//		return;
			//	}

			//	Open(ai.ohBehaveAI);
			//	return;
			//}


			//nodeTree.OnGui(Event.current);
			if (GUI.changed)
				Repaint();
		}
	}

	public class NodeStyle
	{
		public GUIStyle defaultStyle, selectedStyle;
		private Texture2D texture2D;


		internal void Init(Texture2D normal, Texture2D selected)
		{
			defaultStyle = new GUIStyle();
			defaultStyle.normal.background = normal;
			defaultStyle.border = new RectOffset(12, 12, 12, 12);
			defaultStyle.alignment = TextAnchor.UpperCenter;

			selectedStyle = new GUIStyle();
			selectedStyle.normal.background = selected;
			selectedStyle.border = new RectOffset(12, 12, 12, 12);
			selectedStyle.alignment = TextAnchor.UpperCenter;
		}
	}
}