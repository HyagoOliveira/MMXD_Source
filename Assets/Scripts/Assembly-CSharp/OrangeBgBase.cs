using System;
using CallbackDefs;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
[RequireComponent(typeof(RectTransform))]
public class OrangeBgBase : MonoBehaviour
{
	[HideInInspector]
	public RectTransform rt;

	[HideInInspector]
	public Canvas canvas;

	public bool AllowUnderEnable;

	private CanvasGroup canvasGroup;

	public Vector2 Pivot = new Vector2(0.5f, 0.5f);

	private int tweenUid = -1;

	protected virtual void Awake()
	{
		rt = GetComponent<RectTransform>();
		canvas = GetComponent<Canvas>();
		canvasGroup = GetComponent<CanvasGroup>();
		canvasGroup.ignoreParentGroups = true;
		rt.pivot = Pivot;
	}

	public void FadeOut(float fadeOutTime = 0.3f, bool destorySelf = true, Callback p_cb = null)
	{
		tweenUid = LeanTween.value(1f, 0f, fadeOutTime).setOnUpdate(delegate(float val)
		{
			canvasGroup.alpha = val;
		}).setOnComplete((Action)delegate
		{
			tweenUid = -1;
			p_cb.CheckTargetToInvoke();
			if (destorySelf)
			{
				UnityEngine.Object.Destroy(base.gameObject);
			}
		})
			.uniqueId;
	}

	protected virtual void OnDisable()
	{
		LeanTween.cancel(ref tweenUid);
	}
}
