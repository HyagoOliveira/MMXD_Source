using System;
using UnityEngine;

public class DistortionFx : FxBase
{
	[SerializeField]
	private float scale = 30f;

	[SerializeField]
	private Transform target;

	private Vector3 zero = Vector3.zero;

	private bool isShaderSupport;

	private new void Awake()
	{
		Renderer component = target.GetComponent<Renderer>();
		if (component != null)
		{
			isShaderSupport = component.material.shader.isSupported;
		}
		else
		{
			isShaderSupport = false;
		}
		OriScale = base.transform.localScale;
	}

	public override void Active(params object[] p_params)
	{
		if (isShaderSupport)
		{
			base.gameObject.SetActive(true);
			tweenUid = LeanTween.value(target.gameObject, 0f, scale, timeBackToPool).setOnUpdate(delegate(float val)
			{
				target.localScale = new Vector3(val, val, val);
			}).setOnComplete((Action)delegate
			{
				tweenUid = -1;
				target.localScale = zero;
				BackToPool();
			})
				.uniqueId;
		}
	}
}
