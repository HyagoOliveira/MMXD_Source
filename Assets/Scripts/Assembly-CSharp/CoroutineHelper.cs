using System;
using System.Collections;
using UnityEngine;

public class CoroutineHelper : MonoBehaviourSingleton<CoroutineHelper>
{
	public void DelayAction(Action action, float delay = 1f)
	{
		if (delay <= 0f)
		{
			if (action != null)
			{
				action();
			}
		}
		else
		{
			StartCoroutine(DelayActionCoroutine(action, delay));
		}
	}

	private IEnumerator DelayActionCoroutine(Action onFinished, float delay)
	{
		yield return new WaitForSeconds(delay);
		if (onFinished != null)
		{
			onFinished();
		}
	}
}
