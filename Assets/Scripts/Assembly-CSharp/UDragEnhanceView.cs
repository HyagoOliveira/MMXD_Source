using UnityEngine.EventSystems;

public class UDragEnhanceView : EventTrigger
{
	private EnhanceScrollView enhanceScrollView;

	public void SetScrollView(EnhanceScrollView view)
	{
		enhanceScrollView = view;
	}

	public override void OnBeginDrag(PointerEventData eventData)
	{
		if (eventData.button == PointerEventData.InputButton.Left)
		{
			base.OnBeginDrag(eventData);
			enhanceScrollView.OnDragBegin();
		}
	}

	public override void OnDrag(PointerEventData eventData)
	{
		if (eventData.button == PointerEventData.InputButton.Left)
		{
			base.OnDrag(eventData);
			if (enhanceScrollView != null)
			{
				enhanceScrollView.OnDragEnhanceViewMove(eventData.delta);
			}
		}
	}

	public override void OnEndDrag(PointerEventData eventData)
	{
		if (eventData.button == PointerEventData.InputButton.Left)
		{
			base.OnEndDrag(eventData);
			if (enhanceScrollView != null)
			{
				enhanceScrollView.OnDragEnhanceViewEnd();
			}
		}
	}
}
