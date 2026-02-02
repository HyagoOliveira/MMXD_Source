using System;
using UnityEngine;

public class PvpReportUI : OrangeUIBase
{
	private int tweenUid = -1;

	[SerializeField]
	private OrangeText text;

	private string reportStr = string.Empty;

	public void SetMsg(string killer, string Killed, string sLocalKey = "PVP_KILL_REPORT_MSG")
	{
		LeanTween.cancel(ref tweenUid);
		text.supportRichText = true;
		reportStr = reportStr + "\n" + string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr(sLocalKey), killer, Killed);
		text.text = reportStr;
		tweenUid = LeanTween.value(base.gameObject, 1f, 0f, 2f).setOnComplete((Action)delegate
		{
			OnClickCloseBtn();
		}).uniqueId;
	}

	public void SetCanvasAlpha(float val)
	{
		canvasGroup.alpha = val;
	}
}
