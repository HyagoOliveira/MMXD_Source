using System;
using System.Collections;
using CallbackDefs;
using UnityEngine;

public class LoadingUI : MonoBehaviour
{
	[SerializeField]
	private Canvas canvas;

	[SerializeField]
	private CanvasGroup canvasGroup;

	private int tweenId;

	private float startValue;

	private float endValue = 1f;

	public bool IsOpen()
	{
		return canvas.enabled;
	}

	public void ActiveUI(Callback p_cb, GameObject go, float p_activeTime = 0.5f)
	{
		tweenId = LeanTween.value(startValue, endValue, p_activeTime).setOnUpdate(UpdateAlpha).setOnComplete((Action)delegate
		{
			tweenId = -1;
			ILoadingState component = go.GetComponent<ILoadingState>();
			if (component != null)
			{
				StartCoroutine(OnStartWaitLoadingComplete(component, p_cb));
			}
			else
			{
				p_cb.CheckTargetToInvoke();
			}
		})
			.uniqueId;
		canvas.enabled = true;
	}

	private IEnumerator OnStartWaitLoadingComplete(ILoadingState loadingState, Callback p_cb)
	{
		while (!loadingState.IsComplete)
		{
			yield return CoroutineDefine._waitForEndOfFrame;
		}
		p_cb.CheckTargetToInvoke();
	}

	public void DisableUI(Callback p_cb, float fadeTime = 0.5f)
	{
		LeanTween.cancel(ref tweenId);
		Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.UPDATE_LOADING_EFT, fadeTime);
		LeanTween.value(canvasGroup.gameObject, endValue, startValue, fadeTime).setEaseInQuint().setOnUpdate(UpdateAlpha)
			.setOnComplete((Action)delegate
			{
				for (int num = base.transform.childCount - 1; num >= 0; num--)
				{
					UnityEngine.Object.Destroy(base.transform.GetChild(num).gameObject);
				}
				canvas.enabled = false;
				p_cb.CheckTargetToInvoke();
			})
			.setDelay(fadeTime / 2f);
	}

	private void UpdateAlpha(float p_val)
	{
		canvasGroup.alpha = p_val;
	}
}
