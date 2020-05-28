using UnityEditor;
using UnityEngine;

namespace AtomosZ.OhBehave.EditorTools
{
	public class NodeStyle
	{
		public static Color rootColor = new Color(1, .65f, 1, .75f);
		public static Color selectorColor = new Color(1, .65f, 0, .75f);
		public static Color leafColor = new Color(0, 1, 0, .75f);
		public static Color sequenceColor = new Color(1, .92f, .016f, .75f);

		private static GUIStyle leafLabelStyle;
		private static GUIStyle sequencerLabelStyle;
		public static GUIStyle LeafLabelStyle
		{
			get
			{
				if (leafLabelStyle == null)
				{
					leafLabelStyle = new GUIStyle();
					Texture2D tex = new Texture2D(2, 2);
					var fillColorArray = tex.GetPixels32();

					for (var i = 0; i < fillColorArray.Length; ++i)
					{
						fillColorArray[i] = Color.cyan;
					}

					tex.SetPixels32(fillColorArray);
					tex.Apply();
					leafLabelStyle.normal.background = tex;
					leafLabelStyle.alignment = TextAnchor.UpperCenter;
				}

				return leafLabelStyle;
			}
		}

		public static GUIStyle SequencerLabelStyle
		{
			get
			{
				if (sequencerLabelStyle == null)
				{
					sequencerLabelStyle = new GUIStyle();
					Texture2D tex = new Texture2D(2, 2);
					var fillColorArray = tex.GetPixels32();

					for (var i = 0; i < fillColorArray.Length; ++i)
					{
						fillColorArray[i] = Color.blue;
					}

					tex.SetPixels32(fillColorArray);
					tex.Apply();
					sequencerLabelStyle.normal.background = tex;
					sequencerLabelStyle.alignment = TextAnchor.UpperCenter;
				}

				return sequencerLabelStyle;
			}
		}

		public GUIStyle defaultStyle, selectedStyle;
		public Vector2 size;

		private Texture2D texture2D;


		internal void Init(Texture2D normal, Texture2D selected)
		{
			CreateStyles(normal, selected);
			size = new Vector2(
				defaultStyle.normal.background.width * 4,
				defaultStyle.normal.background.height);
		}

		internal void Init(Texture2D normal, Texture2D selected, Vector2 rectSize)
		{
			CreateStyles(normal, selected);
			size = rectSize;
		}

		private void CreateStyles(Texture2D normal, Texture2D selected)
		{
			defaultStyle = new GUIStyle(EditorStyles.helpBox);
			defaultStyle.normal.background = normal;
			defaultStyle.normal.textColor = new Color(0, 0, 0, 0);
			defaultStyle.alignment = TextAnchor.UpperCenter;

			selectedStyle = new GUIStyle(EditorStyles.helpBox);
			selectedStyle.normal.background = selected;
			selectedStyle.normal.textColor = new Color(0,0,0,0);
			selectedStyle.alignment = TextAnchor.UpperCenter;
		}
	}
}