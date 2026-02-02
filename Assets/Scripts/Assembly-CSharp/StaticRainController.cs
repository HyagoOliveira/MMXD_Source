using System;
using RainDropEffect;
using UnityEngine;

public class StaticRainController : MonoBehaviour
{
	public enum DrawState
	{
		Playing = 0,
		Disabled = 1
	}

	[Serializable]
	public class StaticRainDrawerContainer : RainDrawerContainer<RainDrawer>
	{
		public DrawState currentState = DrawState.Disabled;

		public float TimeElapsed;

		public StaticRainDrawerContainer(string name, Transform parent)
			: base(name, parent)
		{
		}
	}

	public StaticRainDrawerContainer staticDrawer;

	public StaticRainVariables Variables { get; set; }

	[HideInInspector]
	public int RenderQueue { get; set; }

	public Camera camera { get; set; }

	public float Alpha { get; set; }

	public bool NoMoreRain { get; set; }

	public RainDropTools.RainDropShaderType ShaderType { get; set; }

	public bool VRMode { get; set; }

	public bool IsPlaying
	{
		get
		{
			return staticDrawer.currentState == DrawState.Playing;
		}
	}

	public void Refresh()
	{
		if (staticDrawer != null)
		{
			UnityEngine.Object.DestroyImmediate(staticDrawer.Drawer.gameObject);
		}
		staticDrawer = new StaticRainDrawerContainer("Static RainDrawer", base.transform);
		staticDrawer.currentState = DrawState.Disabled;
		InitializeInstance(staticDrawer);
	}

	public void Play()
	{
		if (staticDrawer.currentState != 0)
		{
			InitializeInstance(staticDrawer);
		}
	}

	public void UpdateController()
	{
		if (Variables != null)
		{
			UpdateInstance(staticDrawer);
		}
	}

	private float GetProgress(StaticRainDrawerContainer dc)
	{
		return dc.TimeElapsed / Variables.fadeTime;
	}

	private void InitializeInstance(StaticRainDrawerContainer dc)
	{
		dc.TimeElapsed = 0f;
		dc.Drawer.NormalMap = Variables.NormalMap;
		dc.Drawer.ReliefTexture = Variables.OverlayTexture;
		dc.Drawer.Hide();
	}

	private void UpdateInstance(StaticRainDrawerContainer dc)
	{
		AnimationCurve fadeinCurve = Variables.FadeinCurve;
		if (!NoMoreRain)
		{
			dc.TimeElapsed = Mathf.Min(Variables.fadeTime, dc.TimeElapsed + Time.deltaTime);
		}
		else
		{
			dc.TimeElapsed = Mathf.Max(0f, dc.TimeElapsed - Time.deltaTime);
		}
		if (dc.TimeElapsed == 0f)
		{
			dc.Drawer.Hide();
			dc.currentState = DrawState.Disabled;
			return;
		}
		dc.currentState = DrawState.Playing;
		if (Variables.FullScreen)
		{
			Vector2 cameraOrthographicSize = RainDropTools.GetCameraOrthographicSize(camera);
			Vector3 localScale = new Vector3(cameraOrthographicSize.x / 2f, cameraOrthographicSize.y / 2f, 0f);
			if (VRMode)
			{
				localScale += Vector3.one * 0.02f;
			}
			dc.transform.localScale = localScale;
			dc.transform.localPosition = Vector3.zero;
		}
		else
		{
			dc.transform.localScale = new Vector3(Variables.SizeX, Variables.SizeY, 1f);
			Vector3 position = camera.ScreenToWorldPoint(new Vector3((float)(-Screen.width) * Variables.SpawnOffsetX + (float)(Screen.width / 2), (float)(-Screen.height) * Variables.SpawnOffsetY + (float)(Screen.height / 2), 0f));
			dc.transform.localPosition = base.transform.InverseTransformPoint(position);
			dc.transform.localPosition -= Vector3.forward * dc.transform.localPosition.z;
		}
		float progress = GetProgress(dc);
		dc.Drawer.RenderQueue = RenderQueue;
		dc.Drawer.NormalMap = Variables.NormalMap;
		dc.Drawer.ReliefTexture = Variables.OverlayTexture;
		dc.Drawer.OverlayColor = new Color(Variables.OverlayColor.r, Variables.OverlayColor.g, Variables.OverlayColor.b, Variables.OverlayColor.a * fadeinCurve.Evaluate(progress) * Alpha);
		dc.Drawer.DistortionStrength = Variables.DistortionValue * fadeinCurve.Evaluate(progress) * Alpha;
		dc.Drawer.ReliefValue = Variables.ReliefValue * fadeinCurve.Evaluate(progress) * Alpha;
		dc.Drawer.Blur = Variables.Blur * fadeinCurve.Evaluate(progress) * Alpha;
		dc.Drawer.Darkness = Variables.Darkness;
		dc.Drawer.ShaderType = ShaderType;
		dc.Drawer.Show();
	}
}
