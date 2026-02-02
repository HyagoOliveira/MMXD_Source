using UnityEngine;

namespace SSFS
{
	public class SimpleSSFSToggle : MonoBehaviour
	{
		public enum TargetMode
		{
			Material = 0,
			Renderer = 1
		}

		public enum ToggleMode
		{
			KeyPress = 0,
			KeyHold = 1,
			Boolean = 2,
			Timer = 3
		}

		public TargetMode targetMode;

		public ToggleMode toggleMode;

		public Material material;

		public Renderer targetRenderer;

		public bool phaseOn = true;

		public KeyCode key = KeyCode.E;

		public float timerDelay = 5f;

		public float transitionLength = 0.2f;

		private float targetPhase = 1f;

		private float timer;

		private bool transitioning;

		private Material _mat;

		private Material mat
		{
			get
			{
				if (_mat == null)
				{
					switch (targetMode)
					{
					case TargetMode.Material:
						_mat = material;
						break;
					case TargetMode.Renderer:
						_mat = targetRenderer.sharedMaterial;
						break;
					}
				}
				return _mat;
			}
		}

		private void Update()
		{
			float @float = mat.GetFloat("_Phase");
			switch (toggleMode)
			{
			case ToggleMode.KeyPress:
				if (Input.GetKeyDown(key) && !transitioning)
				{
					transitioning = true;
					targetPhase = ((@float > 0.5f) ? 0f : 1f);
				}
				break;
			case ToggleMode.KeyHold:
				targetPhase = (Input.GetKey(key) ? 1f : 0f);
				transitioning = @float != targetPhase;
				break;
			case ToggleMode.Boolean:
				targetPhase = (phaseOn ? 1f : 0f);
				transitioning = @float != targetPhase;
				break;
			case ToggleMode.Timer:
				timer += Time.deltaTime;
				if (timer > timerDelay)
				{
					transitioning = true;
					targetPhase = ((@float > 0.5f) ? 0f : 1f);
					timer = 0f;
				}
				break;
			}
			if (transitioning)
			{
				if (Mathf.Abs(@float - targetPhase) > 0.0001f)
				{
					mat.SetFloat("_Phase", Mathf.MoveTowards(@float, targetPhase, Time.deltaTime / transitionLength));
				}
				else
				{
					transitioning = false;
				}
			}
		}
	}
}
