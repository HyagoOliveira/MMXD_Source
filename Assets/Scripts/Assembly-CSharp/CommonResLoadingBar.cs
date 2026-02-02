using System;
using UnityEngine;
using UnityEngine.UI;

public class CommonResLoadingBar : OrangeUIBase
{
	[SerializeField]
	private GameObject infoObj;

	[SerializeField]
	private Image FillFg;

	[SerializeField]
	private NonDrawingGraphic block;

	public void Setup(bool p_showBlocker = false)
	{
		FillFg.fillAmount = 0f;
		block.raycastTarget = p_showBlocker;
	}

	public void UpdateFill(float val)
	{
		FillFg.fillAmount = val;
	}

	public override void OnClickCloseBtn()
	{
		if (FillFg != null)
		{
			LeanTween.value(FillFg.gameObject, FillFg.fillAmount, 1f, 0.1f).setOnUpdate(delegate(float val)
			{
				FillFg.fillAmount = val;
			}).setOnComplete((Action)delegate
			{
				base.OnClickCloseBtn();
			});
		}
		else
		{
			base.OnClickCloseBtn();
		}
	}
}
