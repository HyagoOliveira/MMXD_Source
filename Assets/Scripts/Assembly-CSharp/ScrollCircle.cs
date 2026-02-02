using UnityEngine;
using UnityEngine.EventSystems;

public class ScrollCircle : MonoBehaviour, IDragHandler, IEventSystemHandler, IPointerUpHandler, IPointerDownHandler
{
	public RectTransform Radius;

	public RectTransform Stick;

	private float mRadius;

	private Vector3 _stickOriginalPos;

	private Vector3 _contentPostion;

	private Camera _currentCamera;

	private void Start()
	{
		mRadius = ((RectTransform)Radius.transform).sizeDelta.x * 0.5f;
		_stickOriginalPos = Radius.position;
		_contentPostion = Vector2.zero;
		_currentCamera = MonoBehaviourSingleton<OrangeSceneManager>.Instance.MainCamera;
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		UpdateStick(ref eventData);
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		Stick.anchoredPosition = Vector2.zero;
	}

	public void OnDrag(PointerEventData eventData)
	{
		if (eventData.button == PointerEventData.InputButton.Left)
		{
			UpdateStick(ref eventData);
		}
	}

	private void UpdateStick(ref PointerEventData eventData)
	{
		Vector2 vector = _stickOriginalPos;
		_contentPostion = eventData.position - vector;
		if (_contentPostion.sqrMagnitude > mRadius * mRadius)
		{
			_contentPostion = _contentPostion.normalized * mRadius;
		}
		Stick.anchoredPosition = _contentPostion;
	}

	public Vector2 GetStickValue()
	{
		return Stick.anchoredPosition.normalized;
	}
}
