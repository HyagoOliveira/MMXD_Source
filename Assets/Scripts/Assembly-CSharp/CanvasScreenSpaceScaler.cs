#define RELEASE
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Canvas))]
public class CanvasScreenSpaceScaler : MonoBehaviour
{
	private Canvas _canvas;

	private PixelPerfectCamera _pixelPerfectCamera;

	private bool _isInitialized;

	private void Initialize(bool warn)
	{
		_canvas = GetComponent<Canvas>();
		if (_canvas.renderMode != RenderMode.ScreenSpaceCamera)
		{
			if (warn)
			{
				Debug.Log(string.Concat("Render mode: ", _canvas.renderMode, " is not supported by CanvasScreenSpaceScaler"));
			}
			return;
		}
		Camera worldCamera = GetComponent<Canvas>().worldCamera;
		_pixelPerfectCamera = worldCamera.GetComponent<PixelPerfectCamera>();
		if (_pixelPerfectCamera == null)
		{
			if (warn)
			{
				Debug.Log("You have to use the PixelPerfectCamera script on the canvas' render camera!");
			}
		}
		else
		{
			_isInitialized = true;
			AdjustCanvas();
		}
	}

	private void OnEnable()
	{
		Initialize(true);
	}

	private void OnValidate()
	{
		Initialize(true);
	}

	private void Update()
	{
		if (!_isInitialized)
		{
			Initialize(false);
		}
		if (_isInitialized && _canvas.renderMode == RenderMode.ScreenSpaceCamera && _canvas.scaleFactor != _pixelPerfectCamera.ratio)
		{
			AdjustCanvas();
		}
	}

	private void AdjustCanvas()
	{
		if (_pixelPerfectCamera.isInitialized && _pixelPerfectCamera.ratio != 0f)
		{
			_canvas.scaleFactor = _pixelPerfectCamera.ratio;
		}
	}
}
