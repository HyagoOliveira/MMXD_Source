using System;
using StageLib;
using UnityEngine;

public class FX_BOSS_EXPLODE2_ChooseCamera : FxBase
{
	public override void Active(params object[] p_params)
	{
		if (Array.Exists(OrangeSceneManager.FindObjectOfTypeCustom<CameraControl>().SpecialStages, (string stage) => stage == StageUpdate.gStageName))
		{
			Transform[] componentsInChildren = base.transform.GetComponentsInChildren<Transform>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].gameObject.SetLayer(ManagedSingleton<OrangeLayerManager>.Instance.EnemyLayer);
			}
		}
		base.Active(p_params);
	}
}
