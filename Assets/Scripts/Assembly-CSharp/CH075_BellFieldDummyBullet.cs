using System;
using UnityEngine;

public class CH075_BellFieldDummyBullet : CollideBullet
{
	public override void Hit(Collider2D col)
	{
		if (BulletData.n_TARGET == 3)
		{
			OrangeCharacter component = col.transform.GetComponent<OrangeCharacter>();
			if ((bool)component)
			{
				if (component.sNetSerialID != refPBMShoter.SOB.sNetSerialID)
				{
					return;
				}
			}
			else
			{
				EnemyControllerBase component2 = col.transform.root.GetComponent<EnemyControllerBase>();
				col.transform.root.GetInstanceID();
				if ((bool)component2 && nRecordID != component2.gameObject.GetInstanceID())
				{
					return;
				}
			}
		}
		if (CheckHitList(ref _ignoreList, col.transform))
		{
			return;
		}
		if (HitCallback != null)
		{
			base.HitTarget = col;
			HitCallback(col);
		}
		else
		{
			base.HitTarget = null;
		}
		int value = -1;
		_hitCount.TryGetValue(col.transform, out value);
		if (value == -1)
		{
			_hitCount.Add(col.transform, 1);
		}
		else
		{
			_hitCount[col.transform] = value + 1;
		}
		_ignoreList.Add(col.transform);
		if (FxImpact != "null")
		{
			bool flag = false;
			StageObjParam component3 = col.transform.GetComponent<StageObjParam>();
			if ((bool)component3)
			{
				IAimTarget aimTarget = component3.tLinkSOB as IAimTarget;
				if (aimTarget != null && aimTarget.AimTransform != null)
				{
					MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FxImpact, aimTarget.AimTransform.position + aimTarget.AimPoint, BulletQuaternion, Array.Empty<object>());
					flag = true;
				}
			}
			if (!flag)
			{
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FxImpact, col.transform.position, BulletQuaternion, Array.Empty<object>());
			}
		}
		CaluDmg(BulletData, col.transform, 0f, 0.5f);
		if (MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.IsPvp)
		{
			OrangeCharacter component4 = col.transform.GetComponent<OrangeCharacter>();
			if ((bool)component4 && component4.IsLocalPlayer)
			{
				TriggerBuff(BulletData, component4.selfBuffManager, 0, true);
			}
		}
	}
}
