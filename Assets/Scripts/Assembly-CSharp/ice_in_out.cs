using System;
using UnityEngine;

public class ice_in_out : MonoBehaviour
{
	private Material mMaterial;

	private Color mMainColor;

	private bool isActive;

	private void Awake()
	{
		mMaterial = base.transform.GetComponent<MeshRenderer>().material;
		if ((bool)mMaterial)
		{
			mMainColor = mMaterial.color;
		}
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxice_out", 10);
	}

	private void OnDestroy()
	{
	}

	private void OnPreRender()
	{
	}

	private void OnDisable()
	{
		FxBase fxBase = MonoBehaviourSingleton<FxManager>.Instance.PlayReturn<FxBase>("fxice_out", base.transform.position, base.transform.rotation, Array.Empty<object>());
		if ((bool)fxBase && ManagedSingleton<OrangeLayerManager>.Instance.DefaultLayer != fxBase.gameObject.layer)
		{
			fxBase.gameObject.layer = ManagedSingleton<OrangeLayerManager>.Instance.DefaultLayer;
		}
	}

	private void OnEnable()
	{
		isActive = true;
		if (!mMaterial)
		{
			return;
		}
		mMaterial.color = new Color(mMainColor.r, mMainColor.g, mMainColor.b, 0f);
		LeanTween.value(0f, 0.7f, 1f).setOnUpdate(delegate(float f)
		{
			if ((bool)mMaterial)
			{
				mMaterial.color = new Color(mMainColor.r, mMainColor.g, mMainColor.b, f);
			}
		}).setIgnoreTimeScale(true);
	}
}
