using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UIEventListener : MonoBehaviour, IPointerClickHandler, IEventSystemHandler, IPointerDownHandler, IPointerUpHandler
{
	public delegate void UIEventProxy(GameObject gb);

	public UIEventProxy OnClick;

	public UIEventProxy OnPressDown;

	public UIEventProxy OnPressUp;

	public void OnPointerClick(PointerEventData eventData)
	{
		if (eventData.button == PointerEventData.InputButton.Left && OnClick != null)
		{
			OnClick(base.gameObject);
		}
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		if (OnPressDown != null)
		{
			OnPressDown(base.gameObject);
		}
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		if (OnPressUp != null)
		{
			OnPressUp(base.gameObject);
		}
	}
}
