using UnityEngine;

public class GUITrackingHelper : MonoBehaviour
{
	[SerializeField]
	private bool KeepTracking;

	[SerializeField]
	private Camera SourceCamera;

	[SerializeField]
	private Canvas TargetCanvas;

	[SerializeField]
	private GameObject TrackingObject;

	private RectTransform _rectTransform;

	private RectTransform _canvasRect;

	private void Awake()
	{
		if (!(TargetCanvas == null))
		{
			_rectTransform = GetComponent<RectTransform>();
			_canvasRect = TargetCanvas.GetComponent<RectTransform>();
		}
	}

	private void Start()
	{
		if (!KeepTracking)
		{
			DoTracking();
		}
	}

	private void LateUpdate()
	{
		if (KeepTracking)
		{
			DoTracking();
		}
	}

	private void DoTracking()
	{
		if (!(SourceCamera == null) && !(TrackingObject == null))
		{
			Vector3 vector = SourceCamera.WorldToScreenPoint(TrackingObject.transform.position);
			Vector2 vector2 = new Vector2(vector.x / (float)Screen.width, vector.y / (float)Screen.height);
			vector2 -= _rectTransform.pivot;
			Vector2 anchoredPosition = new Vector2(vector2.x * _canvasRect.rect.width, vector2.y * _canvasRect.rect.height);
			_rectTransform.anchoredPosition = anchoredPosition;
		}
	}
}
