using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AtomosZ.OhBehave.Editor
{
	public class OhBehaveEditorWindow : EditorWindow
	{
		public static int NextWindowID = 0;
		List<NodeWindow> nodes = new List<NodeWindow>();


		[MenuItem("Window/OhBehave")]
		static public void ShowWindow()
		{
			var window = EditorWindow.GetWindow<OhBehaveEditorWindow>();
			window.titleContent = new GUIContent("OhBehave!");
			window.Init();
		}

		public void Init()
		{
			nodes.Add(new NodeWindow(new Rect(10, 10, 100, 200)));
			nodes.Add(new NodeWindow(new Rect(30, 30, 150, 250)));
		}

		void OnGUI()
		{
			BeginWindows();
			foreach (NodeWindow node in nodes)
			{
				node.OnGUI();
			}

			EndWindows();
		}

		public void OnEnable()
		{

		}


		

	}
}