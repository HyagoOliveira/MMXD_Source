using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemoScene1 : MonoBehaviour
{
	private enum PlayMode
	{
		Home = 0,
		Rain = 1,
		Blood = 2,
		SplashIn = 3,
		SplashOut = 4,
		Frozen = 5
	}

	[SerializeField]
	private List<RainCameraController> rainControllers;

	private PlayMode playMode;

	private float frozenValue;

	private float rainAlpha = 1f;

	private void Awake()
	{
	}

	private void SetResolution(int resolution)
	{
		float num = Mathf.Min(1f, (float)resolution / (float)Screen.height);
		int width = (int)((float)Screen.width * num);
		int height = (int)((float)Screen.height * num);
		Screen.SetResolution(width, height, true, 15);
	}

	private void StopAll()
	{
		foreach (RainCameraController rainController in rainControllers)
		{
			rainController.StopImmidiate();
		}
	}

	private IEnumerator Start()
	{
		yield return null;
		StopAll();
	}

	private void OnGUI()
	{
		int num = 0;
		foreach (RainCameraController rainController in rainControllers)
		{
			if (GuiButton(string.Format("Rain[{0}]", num)))
			{
				StopAll();
				rainController.Play();
			}
			num++;
		}
	}

	private bool GuiButton(string buttonName)
	{
		return GUILayout.Button(buttonName, GUILayout.Height(40f), GUILayout.Width(150f));
	}
}
