using UnityEditor;
using UnityEngine;

namespace AtomosZ.OhBehave.EditorTools
{
	public class OhBehaveEditorWindow : EditorWindow
	{

		private const float ZOOM_BORDER = 10;

		public static NodeStyle SelectorNodeStyle;
		public static NodeStyle SequenceNodeStyle;
		public static NodeStyle LeafNodeStyle;
		public static NodeStyle InverterNodeStyle;
		public static GUIStyle InPointStyle;
		public static GUIStyle OutPointStyle;

		public OhBehaveTreeBlueprint treeBlueprint;
		public EditorZoomer zoomer;


		private OhBehaveEditorWindow window;
		private OhBehaveTreeController currentTreeController;

		private Rect zoomRect;
		private float areaBelowZoomHeight = 50;


		private void OnEnable()
		{
			if (window != null)
			{
				Debug.LogWarning("Editor Window already existed");
				return;
			}
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

			zoomer = new EditorZoomer();
		}

		private void CreateStyles()
		{
			SelectorNodeStyle = new NodeStyle();
			SelectorNodeStyle.Init(new Vector2(250, 100));
			SequenceNodeStyle = new NodeStyle();
			SequenceNodeStyle.Init(new Vector2(250, 100));
			LeafNodeStyle = new NodeStyle();
			LeafNodeStyle.Init(new Vector2(250, 75));
			InverterNodeStyle = new NodeStyle();
			InverterNodeStyle.Init(new Vector2(250, 75));

			InPointStyle = new GUIStyle();
			InPointStyle.normal.background =
				EditorGUIUtility.FindTexture("Assets/OhBehaveTree/Editor/NodeInOut normal.png");
			InPointStyle.hover.background =
				EditorGUIUtility.FindTexture("Assets/OhBehaveTree/Editor/NodeInOut hover.png");

			OutPointStyle = new GUIStyle();
			OutPointStyle.normal.background = InPointStyle.normal.background;
			OutPointStyle.hover.background = InPointStyle.hover.background;

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
			if (treeBlueprint != null)
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

			}
			else
			{

				if (InPointStyle == null)
				{
					CreateStyles();
				}

				{   // Just keeping this around for future reference.
					if (NodeEditPopup.instance != null)
					{
						if (Event.current.type == EventType.MouseDown
							&& EditorWindow.mouseOverWindow != NodeEditPopup.instance)
							NodeEditPopup.instance.Hide();
					}
				}


				if (zoomer == null)
					zoomer = new EditorZoomer();

				zoomer.HandleEvents(Event.current);

				DrawHorizontalUILine(Color.gray);

				Rect lastRect = GUILayoutUtility.GetLastRect();
				if (Event.current.type == EventType.Repaint)
				{
					zoomRect.position = new Vector2(
						ZOOM_BORDER,
						lastRect.yMax + lastRect.height + ZOOM_BORDER);
					zoomRect.size = new Vector2(
						window.position.width - ZOOM_BORDER * 2,
						window.position.height - (lastRect.yMax + ZOOM_BORDER * 2 + areaBelowZoomHeight));
				}


				zoomer.Begin(zoomRect);
				{
					treeBlueprint.OnGui(Event.current, zoomer);
				}
				zoomer.End(new Rect(0, zoomRect.yMax + zoomRect.position.y - 50, window.position.width, window.position.height));


				//DrawHorizontalUILine(Color.gray);

				EditorGUILayout.Vector2Field("mouse", Event.current.mousePosition);
			}


			if (GUI.changed)
				Repaint();
		}





		public static void DrawHorizontalUILine(Color color, int thickness = 2, int padding = 10)
		{
			Rect r = EditorGUILayout.GetControlRect(GUILayout.Height(padding + thickness));
			r.height = thickness;
			r.width -= 9.5f;
			r.y += padding / 2;
			//r.x -= 2;
			r.width += 6;
			EditorGUI.DrawRect(r, color);
		}

		public static void DrawVerticalUILine(Color color, int thickness = 2, int padding = 10)
		{
			Rect r = EditorGUILayout.GetControlRect(GUILayout.Width(padding + thickness));
			r.width = thickness;
			//r.x += padding / 2;
			r.y -= 2;
			r.height += 6;
			EditorGUI.DrawRect(r, color);
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