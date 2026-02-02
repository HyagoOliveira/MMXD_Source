using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class FixedJoystickHandler : MonoBehaviour, IBeginDragHandler, IEventSystemHandler, IDragHandler, IEndDragHandler
{
	[Serializable]
	public class VirtualJoystickEvent : UnityEvent<Vector3>
	{
	}

	public Transform content;

	public UnityEvent beginControl;

	public VirtualJoystickEvent controlling;

	public UnityEvent endControl;

	public void OnBeginDrag(PointerEventData eventData)
	{
		if (eventData.button == PointerEventData.InputButton.Left)
		{
			beginControl.Invoke();
		}
	}

	public void OnDrag(PointerEventData eventData)
	{
		if (eventData.button == PointerEventData.InputButton.Left && (bool)content)
		{
			controlling.Invoke(content.localPosition.normalized);
		}
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		if (eventData.button == PointerEventData.InputButton.Left)
		{
			endControl.Invoke();
		}
	}
}
