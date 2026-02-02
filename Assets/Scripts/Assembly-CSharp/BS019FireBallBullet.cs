using System;
using UnityEngine;

public class BS019FireBallBullet : BasicBullet
{
	protected override void GenerateEndFx(bool bPlaySE = true)
	{
		if (bPlaySE)
		{
			PlaySE(_HitGuardSE[0], _HitGuardSE[1], isForceSE);
		}
		if (!(FxEnd == "") && FxEnd != null)
		{
			Quaternion quaternion = Quaternion.FromToRotation(Vector3.back, Direction);
			RaycastHit2D raycastHit2D = Physics2D.Raycast(oldPos, Direction, offset);
			if ((bool)raycastHit2D)
			{
				_transform.position = raycastHit2D.point;
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FxEnd, raycastHit2D.point, quaternion * BulletQuaternion, Array.Empty<object>());
			}
			else
			{
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FxEnd, _transform.position, quaternion * BulletQuaternion, Array.Empty<object>());
			}
		}
	}
}
