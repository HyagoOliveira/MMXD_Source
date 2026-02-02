using System;
using UnityEngine;

public class EM060_Controller : EM059_Controller, IManagedUpdateBehavior
{
	protected override void GenRecoverBullet(float BulletPos)
	{
		RockBullet[0] = BulletBase.TryShotBullet(EnemyWeapons[1].BulletData, new Vector3(0f, (0f - BulletPos) * 1.4f, 0f) + ModelTransform.position, new Vector3(0f, 1f, 0f), null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
		RockBullet[1] = BulletBase.TryShotBullet(EnemyWeapons[1].BulletData, new Vector3((0f - BulletPos) * 1.4f, 0f, 0f) + ModelTransform.position, new Vector3(1f, 0f, 0f), null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
		RockBullet[2] = BulletBase.TryShotBullet(EnemyWeapons[1].BulletData, new Vector3(BulletPos * 1.4f, 0f, 0f) + ModelTransform.position, new Vector3(-1f, 0f, 0f), null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
		RockBullet[3] = BulletBase.TryShotBullet(EnemyWeapons[1].BulletData, new Vector3(0f, BulletPos * 1.4f, 0f) + ModelTransform.position, new Vector3(0f, -1f, 0f), null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
	}

	protected override void PlayFX()
	{
		MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxhit_em060_rockbroken_000", ModelTransform.transform, Quaternion.identity, new Vector3(0.3f, 0.3f, 0.3f), Array.Empty<object>());
	}
}
