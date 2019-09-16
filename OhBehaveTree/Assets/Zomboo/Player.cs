using UnityEngine;
using UnityEngine.UI;

namespace AtomosZ.OhBehave.Demo.Zomboo
{
	public class Player : MonoBehaviour
	{
		// I want to create and fill the view radius dynamically, using a shader probably.
		public float viewRadius;
		public Text posText;

		private Vector3 worldUp = new Vector3(0, 0, 1);

		public float roationSpeed = 100f;


		public void Update()
		{
			Vector3 mousePos = Input.mousePosition;
			Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);
			worldPos.z = transform.localPosition.z;
			Vector3 dir = worldPos - transform.localPosition;

			Quaternion targetRotation = Quaternion.LookRotation(worldUp, Vector3.Cross(worldUp, dir));


			Quaternion rotated = Quaternion.RotateTowards(transform.localRotation, targetRotation, Time.deltaTime * roationSpeed);
			transform.rotation = rotated;

			string text = "Screen Pos: " + mousePos.ToString();
			text += "\ntarget rot: " + targetRotation;
			text += "\ncurrent rot: " + transform.localRotation;
			posText.text = text;
		}
	}
}