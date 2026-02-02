using UnityEngine;
using UnityEngine.EventSystems;

public class VirtualAnalogStick : MonoBehaviour, IDragHandler, IEventSystemHandler, IPointerUpHandler, IPointerDownHandler
{
	public RectTransform Radius;

	public RectTransform Stick;

	private float _radius;

	private Vector2 _stickCurrentPos;

	private void Awake()
	{
		_radius = Radius.sizeDelta.x * 0.5f;
		_stickCurrentPos = Vector2.zero;
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		UpdateStick(ref eventData);
		Stick.anchoredPosition = _stickCurrentPos;
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		_stickCurrentPos = Vector2.zero;
		Stick.anchoredPosition = _stickCurrentPos;
	}

	public void OnDrag(PointerEventData eventData)
	{
		if (eventData.button == PointerEventData.InputButton.Left)
		{
			UpdateStick(ref eventData);
			Stick.anchoredPosition = _stickCurrentPos;
		}
	}

	private void UpdateStick(ref PointerEventData eventData)
	{
		if (RectTransformUtility.ScreenPointToLocalPointInRectangle(Radius, eventData.position, eventData.pressEventCamera, out _stickCurrentPos) && _stickCurrentPos.sqrMagnitude > _radius * _radius)
		{
			_stickCurrentPos = _stickCurrentPos.normalized * _radius;
		}
	}

	public void ClearAnalogStick()
	{
		_stickCurrentPos = Vector2.zero;
		Stick.anchoredPosition = _stickCurrentPos;
	}

	public Vector2 GetStickValue()
	{
		return _stickCurrentPos / _radius;
	}
}
