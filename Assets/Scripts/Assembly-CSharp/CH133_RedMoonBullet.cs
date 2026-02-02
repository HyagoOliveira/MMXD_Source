using System;
using UnityEngine;

public class CH133_RedMoonBullet : ShingetsurinBullet
{
	protected override bool CheckHitTarget()
	{
		return _hitList.Count > 0;
	}

	protected override void GenerateImpactFx(bool bPlaySE = true)
	{
		if (_subStatus != SubStatus.Phase3)
		{
			Quaternion quaternion = Quaternion.FromToRotation(Vector3.back, Direction);
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
		lastHit = null;
	}
}
