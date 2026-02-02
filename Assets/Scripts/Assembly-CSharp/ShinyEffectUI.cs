using System;
using CallbackDefs;
using UnityEngine;
using UnityEngine.UI;

public class ShinyEffectUI : OrangeUIBase
{
	[SerializeField]
	private Image rtShiny;

	public void Setup(float time = 0.5f, float delay = 0f, LeanTweenType p_leanTweenType = LeanTweenType.linear)
	{
		LeanTween.value(rtShiny.gameObject, 1f, 0f, time).setOnUpdate(delegate(float val)
		{
			rtShiny.color = new Color(1f, 1f, 1f, val);
		}).setDelay(delay)
			.setEase(p_leanTweenType)
			.setOnComplete(OnClickCloseBtn);
	}

	public void SetWhite(float time = 0.5f, float delay = 0f, LeanTweenType p_leanTweenType = LeanTweenType.linear)
	{
		LeanTween.value(rtShiny.gameObject, 0f, 1f, time).setOnUpdate(delegate(float val)
		{
			rtShiny.color = new Color(1f, 1f, 1f, val);
		}).setDelay(delay)
			.setEase(p_leanTweenType)
			.setOnComplete(OnClickCloseBtn);
	}

	public void SetWhite(Callback p_cb, float time = 0.5f, float delay = 0f, LeanTweenType p_leanTweenType = LeanTweenType.linear)
	{
		LeanTween.value(rtShiny.gameObject, 0f, 1f, time).setOnUpdate(delegate(float val)
		{
			rtShiny.color = new Color(1f, 1f, 1f, val);
		}).setDelay(delay)
			.setEase(p_leanTweenType)
			.setOnComplete((Action)delegate
			{
				p_cb.CheckTargetToInvoke();
			});
	}
}
