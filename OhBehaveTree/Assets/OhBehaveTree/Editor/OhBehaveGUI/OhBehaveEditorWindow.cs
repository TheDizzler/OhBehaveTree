using UnityEditor;
using UnityEngine;

namespace AtomosZ.OhBehave.EditorTools
{
	public class OhBehaveEditorWindow : EditorWindow
	{
		internal static NodeStyle SelectorNodeStyle;
		internal static NodeStyle SequenceNodeStyle;
		internal static NodeStyle LeafNodeStyle;
		internal static GUIStyle InPointStyle;
		internal static GUIStyle OutPointStyle;

		internal OhBehaveTreeBlueprint treeBlueprint;

		private OhBehaveEditorWindow window;
		private Vector2 scrollPos;
		private OhBehaveTreeController currentTreeController;


		private void OnEnable()
		{
			window = GetWindow<OhBehaveEditorWindow>();
			window.titleContent = new GUIContent("OhBehave!");

			try
			{
				if (EditorStyles.helpBox == null)
				{ //EditorStyle not yet initialized
					return;
				}
			}
			catch (System.Exception)
			{ //EditorStyle not yet initialized
				return;
			}

			CreateStyles();

			if (treeBlueprint != null)
			{
				treeBlueprint.ConstructNodes();
			}
		}

		private void CreateStyles()
		{
			SelectorNodeStyle = new NodeStyle();
			SelectorNodeStyle.Init(
				EditorGUIUtility.Load("builtin skins/darkskin/images/node2.png") as Texture2D,
				EditorGUIUtility.Load("builtin skins/darkskin/images/node2 on.png") as Texture2D,
				new Vector2(250, 100));
			SequenceNodeStyle = new NodeStyle();
			SequenceNodeStyle.Init(
				EditorGUIUtility.Load("builtin skins/darkskin/images/node0.png") as Texture2D,
				EditorGUIUtility.Load("builtin skins/darkskin/images/node0 on.png") as Texture2D,
				new Vector2(250, 100));
			LeafNodeStyle = new NodeStyle();
			LeafNodeStyle.Init(
				EditorGUIUtility.Load("builtin skins/darkskin/images/node1.png") as Texture2D,
				EditorGUIUtility.Load("builtin skins/darkskin/images/node1 on.png") as Texture2D);

			InPointStyle = new GUIStyle();
			InPointStyle.normal.background = (Texture2D)
				EditorGUIUtility.Load("builtin skins/darkskin/images/radio.png");
			InPointStyle.active.background = (Texture2D)
				EditorGUIUtility.Load("builtin skins/darkskin/images/radio on.png");

			OutPointStyle = new GUIStyle();
			OutPointStyle.normal.background = (Texture2D)
				EditorGUIUtility.Load("builtin skins/darkskin/images/radio.png");
			OutPointStyle.active.background = (Texture2D)
				EditorGUIUtility.Load("builtin skins/darkskin/images/radio on.png");
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


		private void OnLostFocus()
		{
#pragma warning disable CS0618 // Type or member is obsolete
			if (mouseOverWindow != null && mouseOverWindow.title == "Inspector")
#pragma warning restore CS0618 // Type or member is obsolete
				return;
			treeBlueprint.DeselectNode();
			Repaint();
		}


		void OnGUI()
		{
			if (Selection.activeGameObject != null)
			{
				OhBehaveAI ohBehaveSM = Selection.activeGameObject.GetComponent<OhBehaveAI>();
				if (ohBehaveSM != null)
				{
					var ohBehaveController = ohBehaveSM.ohBehaveAI;
					if (ohBehaveController != null && ohBehaveController != currentTreeController)
					{ // switch to the currently selected gameobjects behavior tree
						Open(ohBehaveController);
						return;
					}
				}
			}

			

			if (treeBlueprint == null)
			{
				if (Selection.activeObject != null)
				{
					OhBehaveTreeBlueprint testIfTree = Selection.activeObject as OhBehaveTreeBlueprint;
					if (testIfTree != null)
					{
						treeBlueprint = testIfTree;
						Repaint();
					}
				}
				return;
			}

			if (InPointStyle == null)
			{
				CreateStyles();
			}

			if (NodeEditPopup.instance != null)
			{
				if (Event.current.type == EventType.MouseDown
					&& EditorWindow.mouseOverWindow != NodeEditPopup.instance)
					NodeEditPopup.instance.Hide();
			}

			treeBlueprint.OnGui(Event.current);


			if (GUI.changed)
				Repaint();
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
	}
}