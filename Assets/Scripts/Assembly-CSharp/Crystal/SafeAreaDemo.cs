#define RELEASE
using System;
using UnityEngine;

namespace Crystal
{
	public class SafeAreaDemo : MonoBehaviour
	{
		[SerializeField]
		private KeyCode KeySafeArea = KeyCode.F1;

		private UIManager.SimDevice[] Sims;

		public static int SimIdx;

		private void Awake()
		{
			if (!Application.isEditor)
			{
				UnityEngine.Object.Destroy(base.gameObject);
			}
			Sims = (UIManager.SimDevice[])Enum.GetValues(typeof(UIManager.SimDevice));
		}

		private void Update()
		{
			if (Input.GetKeyDown(KeySafeArea))
			{
				ToggleSafeArea();
			}
		}

		private void ToggleSafeArea()
		{
			SimIdx++;
			if (SimIdx >= Sims.Length)
			{
				SimIdx = 0;
			}
			UIManager.Sim = Sims[SimIdx];
			Debug.LogFormat("Switched to sim device {0} with debug key '{1}'", Sims[SimIdx], KeySafeArea);
		}
	}
}
