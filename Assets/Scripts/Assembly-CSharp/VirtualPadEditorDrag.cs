using CallbackDefs;
using UnityEngine;
using UnityEngine.EventSystems;

public class VirtualPadEditorDrag : MonoBehaviour, IDragHandler, IEventSystemHandler, IPointerDownHandler
{
    [System.Obsolete]
    private CallbackObj OnPointerDownCB;

	public bool IsFixed;

    [System.Obsolete]
    public void Setup(CallbackObj p_OnPointerDownCB)
	{
		OnPointerDownCB = p_OnPointerDownCB;
	}

	public void OnDrag(PointerEventData eventData)
	{
		if (eventData.button == PointerEventData.InputButton.Left && !IsFixed)
		{
			base.transform.localPosition = base.transform.localPosition.xy() + eventData.delta;
		}
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		OnPointerDownCB(base.transform);
	}
}
