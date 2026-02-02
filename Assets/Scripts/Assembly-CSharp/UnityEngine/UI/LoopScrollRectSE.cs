using UnityEngine.EventSystems;

namespace UnityEngine.UI
{
	public abstract class LoopScrollRectSE : LoopScrollRect
	{
		[SerializeField]
		[Tooltip("啟動滾動音效")]
		public bool UseScrollSE;

		[HideInInspector]
		public OrangeScrollSePlayer ScrollSePlayer;

		protected override void Awake()
		{
			base.Awake();
			if (base.vertical)
			{
				ScrollSePlayer = base.content.GetComponent<OrangeScrollSePlayer>();
			}
			else if (base.horizontal)
			{
				ScrollSePlayer = base.content.GetComponent<OrangeScrollSePlayerHorizontal>();
			}
		}

		public override void OnBeginDrag(PointerEventData eventData)
		{
			if (eventData.button == PointerEventData.InputButton.Left)
			{
				if (ScrollSePlayer != null && UseScrollSE)
				{
					ScrollSePlayer.enabled = true;
				}
				base.OnBeginDrag(eventData);
			}
		}

		public override void OnEndDrag(PointerEventData eventData)
		{
			if (eventData.button == PointerEventData.InputButton.Left)
			{
				if (ScrollSePlayer != null && UseScrollSE)
				{
					ScrollSePlayer.enabled = false;
				}
				base.OnEndDrag(eventData);
			}
		}
	}
}
