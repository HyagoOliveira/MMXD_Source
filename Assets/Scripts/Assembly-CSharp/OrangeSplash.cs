#define RELEASE
using System;
using System.Collections;
using Steamworks;
using UnityEngine;

public class OrangeSplash : MonoBehaviour
{
	[SerializeField]
	private LeanTweenType tweenType = LeanTweenType.easeOutQuart;

	[SerializeField]
	private float activeTime = 1.5f;

	[SerializeField]
	private float delay = 1.5f;

	[SerializeField]
	private bool canIgnore;

	[SerializeField]
	private string clipName = string.Empty;

	[SerializeField]
	private CanvasGroup canvasGroup;

	private Vector2 outSidePos = new Vector2(10000f, 10000f);

	private bool jumpToNext;

	protected bool splashActive;

	public OrangeSplashSwitcher Switcher { get; set; }

	private void Awake()
	{
		try
		{
			if (SteamClient.RestartAppIfNecessary(MonoBehaviourSingleton<SteamManager>.Instance.AppID))
			{
				Application.Quit();
			}
		}
		catch (DllNotFoundException)
		{
			Debug.LogError("Not launched via Steam");
			Application.Quit();
		}
		UpdateAlpha(0f);
	}

	public void JumpToNext()
	{
		if (canIgnore)
		{
			jumpToNext = true;
			LeanTween.cancel(canvasGroup.gameObject);
		}
	}

	public IEnumerator Active()
	{
		splashActive = true;
		yield return CoroutineDefine._waitForEndOfFrame;
		if (clipName != string.Empty)
		{
			yield return Switcher.OnCriWareInitializer();
			LeanTween.delayedCall(base.gameObject, delay, (Action)delegate
			{
				Switcher.PlayAudio(clipName);
			});
		}
		float startValue = 0f;
		float endValue = 1f;
		LeanTween.value(canvasGroup.gameObject, startValue, endValue, activeTime).setOnUpdate(UpdateAlpha).setOnComplete((Action)delegate
		{
			LeanTween.value(canvasGroup.gameObject, endValue, startValue, 0.5f).setOnUpdate(UpdateAlpha).setOnComplete((Action)delegate
			{
				jumpToNext = true;
			})
				.setDelay(delay);
		})
			.setEase(tweenType)
			.setDelay(delay);
		while (!jumpToNext)
		{
			yield return CoroutineDefine._waitForEndOfFrame;
		}
		base.transform.localPosition = outSidePos;
	}

	private void UpdateAlpha(float p_alpha)
	{
		canvasGroup.alpha = p_alpha;
	}

	public virtual void SetSplashParam()
	{
	}
}
