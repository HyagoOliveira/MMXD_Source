using System;
using UnityEngine;

public class bs011_in_out : MonoBehaviour
{
	private void Awake()
	{
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxBS011_Debut", 2);
	}

	private void OnDisable()
	{
		FxBase fxBase = MonoBehaviourSingleton<FxManager>.Instance.PlayReturn<FxBase>("fxBS011_Debut", base.transform.position, base.transform.rotation, Array.Empty<object>());
		if ((bool)fxBase && ManagedSingleton<OrangeLayerManager>.Instance.DefaultLayer != fxBase.gameObject.layer)
		{
			fxBase.gameObject.layer = ManagedSingleton<OrangeLayerManager>.Instance.DefaultLayer;
		}
	}
}
