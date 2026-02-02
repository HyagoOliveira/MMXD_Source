using System;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class PixelPerfectCamera : MonoBehaviour
{
	public enum Dimension
	{
		Width = 0,
		Height = 1
	}

	public enum ConstraintType
	{
		None = 0,
		Horizontal = 1,
		Vertical = 2
	}

	public static int PIXELS_PER_UNIT = 100;

	public bool maxCameraHalfWidthEnabled;

	public bool maxCameraHalfHeightEnabled;

	public float maxCameraHalfWidth = 3f;

	public float maxCameraHalfHeight = 2f;

	public Dimension targetDimension = Dimension.Height;

	public float targetCameraHalfWidth = 2f;

	public float targetCameraHalfHeight = 1.5f;

	public bool pixelPerfect;

	public bool retroSnap;

	public float assetsPixelsPerUnit = PIXELS_PER_UNIT;

	public bool showHUD;

	[NonSerialized]
	public Vector2 cameraSize;

	[NonSerialized]
	public ConstraintType contraintUsed;

	[NonSerialized]
	public float cameraPixelsPerUnit;

	[NonSerialized]
	public float ratio;

	[NonSerialized]
	public Vector2 nativeAssetResolution;

	[NonSerialized]
	public float fovCoverage;

	[NonSerialized]
	public bool isInitialized;

	private Resolution res;

	private Camera cam;

	private float calculatePixelPerfectCameraSize(bool pixelPerfect, Resolution res, float assetsPixelsPerUnit, float maxCameraHalfWidth, float maxCameraHalfHeight, float targetHalfWidth, float targetHalfHeight, Dimension targetDimension)
	{
		float num = 2f * maxCameraHalfWidth;
		float num2 = 2f * maxCameraHalfHeight;
		float num3 = 2f * targetHalfWidth;
		float num4 = 2f * targetHalfHeight;
		float num5 = (float)res.width / (float)res.height;
		float num7;
		if (targetDimension == Dimension.Width)
		{
			float num6 = assetsPixelsPerUnit * num3;
			num7 = (float)res.width / num6;
		}
		else
		{
			float num8 = assetsPixelsPerUnit * num4;
			num7 = (float)res.height / num8;
		}
		float num9 = num7;
		if (pixelPerfect)
		{
			float num10 = Mathf.Ceil(num7);
			float num11 = num10 - 1f;
			num7 = ((1f / num7 - 1f / num10 < 1f / num11 - 1f / num7) ? num10 : num11);
			if (num10 <= 1f)
			{
				num7 = 1f;
			}
		}
		float num12 = 0f;
		float num13 = 0f;
		if (num > 0f)
		{
			float num14 = assetsPixelsPerUnit * num;
			num12 = (float)res.width / num14;
		}
		if (num2 > 0f)
		{
			float num15 = assetsPixelsPerUnit * num2;
			num13 = (float)res.height / num15;
		}
		float num16 = Mathf.Max(num12, num13);
		if (pixelPerfect)
		{
			num16 = Mathf.Ceil(num16);
		}
		float num17 = Mathf.Max(num16, num7);
		float num18 = (float)res.width / (assetsPixelsPerUnit * num17);
		float num19 = num18 / num5;
		cameraSize = new Vector2(num18 / 2f, num19 / 2f);
		bool flag = num7 >= Mathf.Max(num12, num13) && num9 >= Mathf.Max(num12, num13);
		contraintUsed = ((!flag) ? ((num12 > num13) ? ConstraintType.Horizontal : ConstraintType.Vertical) : ConstraintType.None);
		cameraPixelsPerUnit = (float)res.width / num18;
		ratio = num17;
		nativeAssetResolution = new Vector2(num18 * assetsPixelsPerUnit, num19 * assetsPixelsPerUnit);
		fovCoverage = num9 / num17;
		isInitialized = true;
		return num19 / 2f;
	}

	public void adjustCameraFOV()
	{
		if (cam == null)
		{
			cam = GetComponent<Camera>();
		}
		res = default(Resolution);
		res.width = cam.pixelWidth;
		res.height = cam.pixelHeight;
		res.refreshRate = Screen.currentResolution.refreshRate;
		if (res.width != 0 && res.height != 0)
		{
			float num = (maxCameraHalfWidthEnabled ? maxCameraHalfWidth : (-1f));
			float num2 = (maxCameraHalfHeightEnabled ? maxCameraHalfHeight : (-1f));
			float orthographicSize = calculatePixelPerfectCameraSize(pixelPerfect, res, assetsPixelsPerUnit, num, num2, targetCameraHalfWidth, targetCameraHalfHeight, targetDimension);
			cam.orthographicSize = orthographicSize;
		}
	}

	private void OnEnable()
	{
		adjustCameraFOV();
	}

	private void OnValidate()
	{
		maxCameraHalfWidth = Math.Max(maxCameraHalfWidth, 0.01f);
		maxCameraHalfHeight = Math.Max(maxCameraHalfHeight, 0.01f);
		targetCameraHalfWidth = Math.Max(targetCameraHalfWidth, 0.01f);
		targetCameraHalfHeight = Math.Max(targetCameraHalfHeight, 0.01f);
		adjustCameraFOV();
	}

	private void Update()
	{
		if (res.width != cam.pixelWidth || res.height != cam.pixelHeight)
		{
			adjustCameraFOV();
		}
	}

	private void OnGUI()
	{
		if (showHUD)
		{
			float num = Screen.dpi / 96f;
			GUIStyle gUIStyle = new GUIStyle(GUI.skin.box);
			gUIStyle.fontSize = (int)(13f * num);
			GUI.Box(new Rect(10f * num, 10f * num, 130f * num, 90f * num), "Camera", gUIStyle);
			GUIStyle gUIStyle2 = new GUIStyle(GUI.skin.toggle);
			gUIStyle2.fontSize = (int)(13f * num);
			gUIStyle2.border = new RectOffset(0, 0, 0, 0);
			gUIStyle2.overflow = new RectOffset(0, 0, 0, 0);
			gUIStyle2.padding = new RectOffset(0, 0, 0, 0);
			gUIStyle2.imagePosition = ImagePosition.ImageOnly;
			pixelPerfect = GUI.Toggle(new Rect(20f * num, 40f * num, 20f * num, 20f * num), pixelPerfect, new GUIContent("Pixel perfect"), gUIStyle2);
			GUIStyle gUIStyle3 = new GUIStyle(GUI.skin.label);
			gUIStyle3.fontSize = (int)(13f * num);
			GUI.Label(new Rect(40f * num, 40f * num, 80f * num, 20f * num), new GUIContent("Pixel perfect"), gUIStyle3);
			retroSnap = GUI.Toggle(new Rect(20f * num, 60f * num, 20f * num, 20f * num), retroSnap, new GUIContent("Retro Snap"), gUIStyle2);
			GUI.Label(new Rect(40f * num, 60f * num, 80f * num, 20f * num), new GUIContent("Retro Snap"), gUIStyle3);
			if (GUI.changed)
			{
				adjustCameraFOV();
			}
		}
	}
}
