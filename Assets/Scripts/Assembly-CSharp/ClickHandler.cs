using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class ClickHandler : MonoBehaviour, IPointerClickHandler, IEventSystemHandler, IPointerDownHandler, IPointerUpHandler
{
	[Serializable]
	public class ClickEvent : UnityEvent
	{
	}

	[SerializeField]
	private ClickEvent pointerDownEvent;

	[SerializeField]
	private ClickEvent pointerUpEvent;

	[SerializeField]
	private ClickEvent pointerClickEvent;

	public void OnPointerDown(PointerEventData eventData)
	{
		if (pointerDownEvent != null)
		{
			pointerDownEvent.Invoke();
		}
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		if (pointerUpEvent != null)
		{
			pointerUpEvent.Invoke();
		}
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		if (eventData.button == PointerEventData.InputButton.Left && pointerClickEvent != null)
		{
			pointerClickEvent.Invoke();
		}
	}
}
