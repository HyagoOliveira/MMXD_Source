using UnityEngine;
using UnityEngine.UI;

public class DeepRecordContinuousMovementLogUI : OrangeUIBase
{
	[SerializeField]
	private DeepRecordContinuousMovementLogUIUnit prefabUnit;

	[SerializeField]
	private LoopVerticalScrollRect scrollRect;

	[SerializeField]
	private Canvas CanvasNoResult;

	public void Setup()
	{
		base._EscapeEvent = EscapeEvent.CLOSE_UI;
		UpdateLog();
		base.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
	}

	private void UpdateLog()
	{
		scrollRect.OrangeInit(prefabUnit, 10, ManagedSingleton<DeepRecordHelper>.Instance.ListMultiMoveLog.Count);
	}
}
