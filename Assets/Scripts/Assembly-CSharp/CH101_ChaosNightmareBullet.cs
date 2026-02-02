using System;
using UnityEngine;

public class CH101_ChaosNightmareBullet : BasicBullet
{
	protected override void GenerateImpactFx(bool bPlaySE = true)
	{
		Vector3 vector = Direction.normalized * 1.5f + Vector3.down * 1.5f;
		if (ActivateTimer.GetMillisecond() < 100)
		{
			vector = Direction.normalized * heapDistance + Vector3.down * 1.5f;
		}
		MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FxImpact, _transform.position + vector, BulletQuaternion, Array.Empty<object>());
		if (isHitBlock || needWeaponImpactSE)
		{
			PlaySE(_HitGuardSE[0], _HitGuardSE[1], isForceSE);
		}
	}
}
