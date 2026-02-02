#define RELEASE
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Canvas))]
public class CanvasWorldScaler : MonoBehaviour
{
	[Tooltip("A camera that uses the PixelPerfectCamera script")]
	public Camera uiCamera;

	private PixelPerfectCamera _pixelPerfectCamera;

	private Vector2 _cameraSize;

	private float _assetsPixelsPerUnit;

	private Canvas _canvas;

	private bool _isInitialized;

	private void Initialize(bool warn)
	{
		_canvas = GetComponent<Canvas>();
		if (_canvas.renderMode != RenderMode.WorldSpace)
		{
			Debug.Log(string.Concat("Render mode: ", _canvas.renderMode, " is not supported by CanvasWorldScaler"));
			return;
		}
		if (uiCamera == null)
		{
			if (warn)
			{
				Debug.Log("You have to assign a UI camera!");
			}
			return;
		}
		_pixelPerfectCamera = uiCamera.GetComponent<PixelPerfectCamera>();
		if (_pixelPerfectCamera == null)
		{
			if (warn)
			{
				Debug.Log("You have to use the PixelPerfectCamera script on the assigned UI camera!");
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

	private void Update()
	{
		if (!_isInitialized)
		{
			Initialize(false);
		}
		if (_isInitialized && _canvas.renderMode == RenderMode.WorldSpace && (_assetsPixelsPerUnit != _pixelPerfectCamera.assetsPixelsPerUnit || _cameraSize != _pixelPerfectCamera.cameraSize))
		{
			AdjustCanvas();
		}
	}

	private void AdjustCanvas()
	{
		if (_pixelPerfectCamera.isInitialized && _pixelPerfectCamera.cameraSize.x != 0f)
		{
			_cameraSize = _pixelPerfectCamera.cameraSize;
			_assetsPixelsPerUnit = _pixelPerfectCamera.assetsPixelsPerUnit;
			GetComponent<RectTransform>().sizeDelta = 2f * _assetsPixelsPerUnit * _cameraSize;
			Vector3 localScale = GetComponent<RectTransform>().localScale;
			localScale.x = 1f / _assetsPixelsPerUnit;
			localScale.y = 1f / _assetsPixelsPerUnit;
			GetComponent<RectTransform>().localScale = localScale;
		}
	}
}
