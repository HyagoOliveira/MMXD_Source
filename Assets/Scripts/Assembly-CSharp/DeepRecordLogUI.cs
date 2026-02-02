using UnityEngine;
using UnityEngine.UI;
using enums;

public class DeepRecordLogUI : OrangeUIBase
{
	[SerializeField]
	private ItemBoxTab tabBattleLog;

	[SerializeField]
	private ItemBoxTab tabAbilityLog;

	[SerializeField]
	private ItemBoxTab tabRandomLog;

	[SerializeField]
	private DeepRecordLogUIUnit prefabUnit;

	[SerializeField]
	private LoopVerticalScrollRect scrollRect;

	[SerializeField]
	private Canvas CanvasNoResult;

	public RecordGridLogType LogType { get; set; } = RecordGridLogType.Battle;


	public void Setup()
	{
		base._EscapeEvent = EscapeEvent.CLOSE_UI;
		UpdateLog(RecordGridLogType.Battle);
		tabBattleLog.AddBtnCB(OnClickBattleLogBtn);
		tabAbilityLog.AddBtnCB(OnClickAbilityLogBtn);
		tabRandomLog.AddBtnCB(OnClickRandomLogBtn);
		base.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
	}

	private void OnClickBattleLogBtn()
	{
		if (LogType != RecordGridLogType.Battle)
		{
			PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_CURSOR07);
		}
		UpdateLog(RecordGridLogType.Battle);
	}

	private void OnClickAbilityLogBtn()
	{
		if (LogType != RecordGridLogType.Ability)
		{
			PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_CURSOR07);
		}
		UpdateLog(RecordGridLogType.Ability);
	}

	private void OnClickRandomLogBtn()
	{
		if (LogType != RecordGridLogType.Random)
		{
			PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_CURSOR07);
		}
		UpdateLog(RecordGridLogType.Random);
	}

	private void UpdateLog(RecordGridLogType p_logType)
	{
		LogType = p_logType;
		tabBattleLog.UpdateState(LogType != RecordGridLogType.Battle);
		tabAbilityLog.UpdateState(LogType != RecordGridLogType.Ability);
		tabRandomLog.UpdateState(LogType != RecordGridLogType.Random);
		switch (LogType)
		{
		case RecordGridLogType.Battle:
			ManagedSingleton<DeepRecordHelper>.Instance.RetrieveRecordGridBattleLogReq(delegate
			{
				scrollRect.OrangeInit(prefabUnit, 10, ManagedSingleton<DeepRecordHelper>.Instance.ListBattleLog.Count);
				CanvasNoResult.enabled = ManagedSingleton<DeepRecordHelper>.Instance.ListBattleLog.Count == 0;
			});
			break;
		case RecordGridLogType.Ability:
			ManagedSingleton<DeepRecordHelper>.Instance.RetrieveRecordGridAbilityLogReq(delegate
			{
				scrollRect.OrangeInit(prefabUnit, 10, ManagedSingleton<DeepRecordHelper>.Instance.ListAbilityLog.Count);
				CanvasNoResult.enabled = ManagedSingleton<DeepRecordHelper>.Instance.ListAbilityLog.Count == 0;
			});
			break;
		case RecordGridLogType.Random:
			ManagedSingleton<DeepRecordHelper>.Instance.RetrieveRecordGridRandomLogReq(delegate
			{
				scrollRect.OrangeInit(prefabUnit, 10, ManagedSingleton<DeepRecordHelper>.Instance.ListRandomLog.Count);
				CanvasNoResult.enabled = ManagedSingleton<DeepRecordHelper>.Instance.ListRandomLog.Count == 0;
			});
			break;
		}
	}
}
