using System;
using UnityEngine;

public class bs009_fxevent : MonoBehaviour
{
	public ParticleSystem akt1;

	private void Start()
	{
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("FX_BOSS_EXPLODE3", 2);
	}

	public void play_atk1_fx()
	{
		akt1.Play();
		MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("FX_BOSS_EXPLODE3", akt1.transform.position, Quaternion.identity, Array.Empty<object>());
	}

	public void hide_AimIcon2()
	{
		Transform transform = OrangeBattleUtility.FindChildRecursive(base.transform, "AimIcon2");
		if (transform != null)
		{
			transform.gameObject.SetActive(false);
		}
	}
}
