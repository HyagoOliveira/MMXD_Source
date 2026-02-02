#define RELEASE
using System;
using Coffee.UIExtensions;
using UnityEngine;
using UnityEngine.UI;

public class ConnectingUI : MonoBehaviour
{
	[SerializeField]
	private Text textConnecting;

	[SerializeField]
	private UIShiny shinyEft;

	private Canvas canvas;

	private int tweenId = -1;

	private int disableTweenId = -1;

	private float startValue = 0.5f;

	private float endValue = 1f;

	private float fadeTime = 0.5f;

	public bool IsConnecting
	{
		get
		{
			return canvas.enabled;
		}
	}

	private void Awake()
	{
		canvas = GetComponent<Canvas>();
	}

	public void ActiveUI()
	{
		CancelTween();
		canvas.enabled = true;
		shinyEft.Play(false);
		tweenId = LeanTween.value(base.gameObject, startValue, endValue, fadeTime).setOnUpdate(UpdateAlpha).setOnComplete((Action)delegate
		{
			tweenId = LeanTween.value(base.gameObject, endValue, startValue, fadeTime).setLoopClamp().uniqueId;
		})
			.uniqueId;
	}

	public void DisableUI()
	{
		CancelTween();
		disableTweenId = LeanTween.delayedCall(0.15f, (Action)delegate
		{
			shinyEft.Stop(false);
			canvas.enabled = false;
			disableTweenId = -1;
		}).uniqueId;
	}

	private void OnDisable()
	{
		CancelTween();
	}

	private void CancelTween()
	{
		LeanTween.cancel(ref tweenId, false);
		LeanTween.cancel(ref disableTweenId, false);
		tweenId = -1;
		disableTweenId = -1;
	}

	public void UpdateAlpha(float p_val)
	{
		textConnecting.color = new Color(textConnecting.color.r, textConnecting.color.g, textConnecting.color.b, p_val);
	}

	public void OnClick()
	{
		Debug.Log("Blocking Message!");
	}
}
