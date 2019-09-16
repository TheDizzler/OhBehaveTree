using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AtomosZ.OhBehave.Editor
{
	public class OhBehaveEditorWindow : EditorWindow
	{
		public static int NextWindowID = 0;

		[SerializeField]
		CompositeNodeWindow startNode;


		[MenuItem("Window/OhBehave")]
		static public void ShowWindow()
		{
			var window = EditorWindow.GetWindow<OhBehaveEditorWindow>();
			window.titleContent = new GUIContent("OhBehave!");
			window.Init();
		}

		private void Init()
		{
			startNode = new SelectorNodeWindow(null, new Vector2(position.width/2, 0));
		}


		public void OnEnable()
		{
			Debug.Log("OnEnable");
		}


		void OnGUI()
		{
			BeginWindows();
			startNode.OnGUI();
			EndWindows();
		}





	}
}