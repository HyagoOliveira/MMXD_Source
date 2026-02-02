using System;
using UnityEngine;

public sealed class CH127_BatBullet : BasicBullet
{
	[SerializeField]
	private bool CheckPhase = true;

	protected override void GenerateImpactFx(bool bPlaySE = true)
	{
		Quaternion quaternion = Quaternion.FromToRotation(Vector3.back, Direction);
		if (!CheckPhase || Phase != BulletPhase.Result)
		{
			if ((bool)lastHit)
			{
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FxImpact, lastHit.position, quaternion * BulletQuaternion, Array.Empty<object>());
			}
			else
			{
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FxImpact, _transform.position, quaternion * BulletQuaternion, Array.Empty<object>());
			}
		}
		if (isHitBlock || needPlayEndSE || needWeaponImpactSE)
		{
			PlaySE(_HitGuardSE[0], _HitGuardSE[1], isForceSE);
		}
	}
}
