using UnityEditor;
using UnityEngine;

namespace AtomosZ.OhBehave.EditorTools
{
	/// <summary>
	/// Borrowed heavily from https://gist.github.com/MattRix/564fa9c36c511ce9ec2b8f5c84022a97
	/// </summary>
	public class EditorZoomer
	{
		private const float MIN_ZOOM = .1f;
		private const float MAX_ZOOM = 2;
		private const float sliderWidth = 75;
		private const float sliderHeight = 50;
		private readonly float panMinimum = 1f;

		public bool isScreenMoved;

		private Rect zoomAreaRect;
		private float zoomScale = 1;
		private Vector2 zoomOrigin = Vector2.zero;
		private Vector2 lastMouse = Vector2.zero;
		private Matrix4x4 prevGUIMatrix;
		private bool zoomToCenter;
		private Texture2D bgTexture = EditorGUIUtility.FindTexture("Assets/OhBehaveTree/Editor/zoomerBG.jpg");
		/// <summary>
		/// Prevents zoom area from jumping around when left mouse button was clicked in a different context.
		/// </summary>
		private bool lastWasDragging;
		private Vector2 prePanZoomOrigin;
		private GUIStyle warningTextStyle;
		private bool displayWarning;


		public EditorZoomer()
		{
			warningTextStyle = new GUIStyle();
			warningTextStyle.normal.textColor = Color.red;
		}

		public void Begin(Rect zoomRect)
		{
			// Ends group that Unity implicity begins for every editor window
			zoomAreaRect = zoomRect;
			GUI.EndGroup();


			Vector2 offset = GetContentOffset();
			float xFactor = offset.x / bgTexture.width;
			float yFactor = offset.y / bgTexture.height;

			//GUI.DrawTextureWithTexCoords(zoomAreaRect, bgTexture, // texcoords are between 0 and 1! 1 == fullwrap!
			//	new Rect(xFactor, -yFactor, zoomAreaRect.width / (bgTexture.width * zoomScale),
			//		zoomAreaRect.height / (bgTexture.height * zoomScale)));

			GraphBackground.DrawGraphBackground(zoomAreaRect, -offset, zoomScale);


			Rect clippedArea = ScaleSizeBy(zoomAreaRect, 1.0f / zoomScale, new Vector2(zoomAreaRect.xMin, zoomAreaRect.yMin));
			GUI.BeginGroup(clippedArea);

			prevGUIMatrix = GUI.matrix;

			Matrix4x4 translation = Matrix4x4.TRS(
				new Vector2(clippedArea.xMin, clippedArea.yMin), Quaternion.identity, Vector3.one);
			Matrix4x4 scale = Matrix4x4.Scale(new Vector3(zoomScale, zoomScale, 1f));
			GUI.matrix = translation * scale * translation.inverse * GUI.matrix;
		}


		public void DisplayInvalid(bool isValidTree)
		{
			displayWarning = !isValidTree;
		}

		public void End(Rect postZoomArea)
		{
			var matrix = GUI.matrix;
			GUI.matrix = prevGUIMatrix;
			GUI.EndGroup();

			GUI.BeginGroup(zoomAreaRect, EditorStyles.helpBox);
			{
				GUILayout.BeginArea(
					new Rect(zoomAreaRect.xMax - sliderWidth * 1.5f, zoomAreaRect.yMin - 25,
						sliderWidth, sliderHeight),
					EditorStyles.helpBox);
				var defaultColor = GUI.color;
				GUI.color = new Color(0, 0, 0, .25f);
				GUILayout.Label("Zoom Scale: " + zoomScale + "x");
				float newZoom = GUILayout.HorizontalSlider(zoomScale, MIN_ZOOM, MAX_ZOOM);
				if (zoomScale != newZoom)
				{
					zoomToCenter = true;
					zoomScale = newZoom;
					GUI.changed = true;
				}

				GUILayout.EndArea();
				GUI.color = defaultColor;
			}
			GUI.EndGroup();

			if (displayWarning)
			{
				GUI.BeginGroup(zoomAreaRect, EditorStyles.helpBox);
				{
					GUILayout.BeginArea(
						new Rect(zoomAreaRect.xMax - sliderWidth * 4f, zoomAreaRect.yMax - 100,
							sliderWidth * 3.3f, sliderHeight * .5f),
						EditorStyles.helpBox);
					{
						GUILayout.Label("Tree Invalid - Please fix broken branches", warningTextStyle);
					}

					GUILayout.EndArea();
				}
				GUI.EndGroup();
			}
			GUI.BeginGroup(postZoomArea);
		}

		public float GetScale()
		{
			return zoomScale;
		}

		public Vector2 GetOrigin()
		{
			return zoomOrigin;
		}

		public void SetScale(float savedScale)
		{
			zoomScale = savedScale;
		}

		public void SetOrigin(Vector2 savedOrigin)
		{
			zoomOrigin = savedOrigin;
		}

		public Vector2 GetContentOffset()
		{
			Vector2 offset = -zoomOrigin / zoomScale; //offset the midpoint
			offset -= (zoomAreaRect.size / 2f) / zoomScale; //offset the center
			return offset;
		}


		public void HandleEvents(Event current)
		{
			if (current.isMouse)
			{
				if (current.type == EventType.MouseDrag &&
					((current.button == 0 && current.modifiers == EventModifiers.Alt)
						|| current.button == 1))
				{
					if (lastWasDragging)
					{
						var mouseDelta = Event.current.mousePosition - lastMouse;
						zoomOrigin += mouseDelta;
						prePanZoomOrigin += mouseDelta;
						Event.current.Use();
					}

					lastWasDragging = true;
				}
				else if (current.type == EventType.MouseUp && current.button == 1)
				{
					isScreenMoved = Mathf.Abs(prePanZoomOrigin.x) > panMinimum
						|| Mathf.Abs(prePanZoomOrigin.y) > panMinimum;

					lastWasDragging = false;
					prePanZoomOrigin = Vector2.zero;
				}
				else
				{
					lastWasDragging = false;
					prePanZoomOrigin = Vector2.zero;
					isScreenMoved = false;
				}
				lastMouse = current.mousePosition;
			}

			if (current.type == EventType.ScrollWheel)
			{
				float oldZoom = zoomScale;
				float zoomChange = 1.10f;

				zoomScale *= Mathf.Pow(zoomChange, -Event.current.delta.y / 3f);
				zoomScale = Mathf.Clamp(zoomScale, MIN_ZOOM, MAX_ZOOM);

				if (oldZoom != zoomScale)
					zoomToCenter = false;
				//bool shouldZoomTowardsMouse = true; //if this is false, it will always zoom towards the center of the content (0,0)

				if (!zoomToCenter)
				{
					//we want the same content that was under the mouse pre-zoom to be there post-zoom as well
					//in other words, the content's position *relative to the mouse* should not change

					Vector2 areaMousePos = Event.current.mousePosition - zoomAreaRect.center;

					Vector2 contentOldMousePos = (areaMousePos / oldZoom) - (zoomOrigin / oldZoom);
					Vector2 contentMousePos = (areaMousePos / zoomScale) - (zoomOrigin / zoomScale);

					Vector2 mouseDelta = contentMousePos - contentOldMousePos;

					zoomOrigin += mouseDelta * zoomScale;
				}
				else
					zoomOrigin = zoomAreaRect.center;

				current.Use();
			}
		}

		private Rect ScaleSizeBy(Rect viewRect, float scale, Vector2 pivotPoint)
		{
			Rect result = viewRect;
			result.x -= pivotPoint.x;
			result.y -= pivotPoint.y;
			result.xMin *= scale;
			result.xMax *= scale;
			result.yMin *= scale;
			result.yMax *= scale;
			result.x += pivotPoint.x;
			result.y += pivotPoint.y;
			return result;
		}
	}
}