using UnityEngine;

public class BS059_IceSplit : BasicBullet
{
	public int splitlevel = 1;

	public override void Hit(Collider2D col)
	{
		if (CheckHitList(ref _hitList, col.transform))
		{
			return;
		}
		int num = 1 << col.gameObject.layer;
		switch (Phase)
		{
		case BulletPhase.Normal:
		case BulletPhase.Boomerang:
			if (splitlevel > 0)
			{
				SubBullet();
			}
			if (!_hitList.Contains(col.transform))
			{
				lastHit = col.transform;
				_hitList.Add(col.transform);
			}
			if ((num & (int)TargetMask) != 0)
			{
				isHitBlock = false;
			}
			else if (col.gameObject.GetComponent<StageHurtObj>() == null)
			{
				isHitBlock = true;
			}
			else
			{
				needWeaponImpactSE = (isHitBlock = false);
			}
			if (nThrough > 0 && (num & (int)TargetMask) != 0)
			{
				if (lastHit != null)
				{
					CaluDmg(BulletData, lastHit);
				}
				GenerateImpactFx();
				nThrough--;
				if (nThrough == 0)
				{
					Phase = BulletPhase.BackToPool;
				}
			}
			else if (nThrough > 0 && lastHit != null && lastHit.gameObject.GetComponent<StageHurtObj>() != null)
			{
				CaluDmg(BulletData, lastHit);
				GenerateImpactFx();
				nThrough--;
				if (nThrough == 0)
				{
					Phase = BulletPhase.BackToPool;
				}
			}
			else if (BulletData.f_RANGE == 0f)
			{
				Phase = BulletPhase.Result;
			}
			else
			{
				SetPhaseToSplash();
			}
			break;
		case BulletPhase.Splash:
		case BulletPhase.Result:
			break;
		}
	}

	public override void BackToPool()
	{
		_transform.localScale = new Vector3(1f, 1f, 1f);
		_transform.GetComponentsInChildren<Transform>(true);
		MonoBehaviourSingleton<UpdateManager>.Instance.RemoveLateUpdate(this);
		ActivateTimer.TimerStop();
		Phase = BulletPhase.Normal;
		_hitList.Clear();
		_rigidbody2D.Sleep();
		_capsuleCollider.enabled = false;
		isSubBullet = false;
		FreeDISTANCE = 0f;
		isBuffTrigger = false;
		base.BackToPool();
	}

	public override void SubBullet()
	{
		PoolManager instance = MonoBehaviourSingleton<PoolManager>.Instance;
		Vector3 vector = -base.GetDirection;
		for (int i = 0; i < 5; i++)
		{
			Vector3 pDirection = Quaternion.Euler(0f, 0f, (i - 2) * 20) * vector;
			BS059_IceSplit poolObj = instance.GetPoolObj<BS059_IceSplit>(BulletData.s_MODEL);
			poolObj.UpdateBulletData(BulletData, Owner);
			poolObj.BulletLevel = BulletLevel;
			poolObj.splitlevel = 0;
			poolObj.nHp = nHp;
			poolObj.nAtk = nAtk;
			poolObj.nCri = nCri;
			poolObj.nHit = nHit;
			poolObj.nWeaponCheck = nWeaponCheck;
			poolObj.nWeaponType = nWeaponType;
			poolObj.nCriDmgPercent = nCriDmgPercent;
			poolObj.nReduceBlockPercent = nReduceBlockPercent;
			poolObj.fDmgFactor = fDmgFactor;
			poolObj.fCriFactor = fCriFactor;
			poolObj.fCriDmgFactor = fCriDmgFactor;
			poolObj.fMissFactor = fMissFactor;
			poolObj.refPBMShoter = refPBMShoter;
			poolObj.refPSShoter = refPSShoter;
			poolObj.isSubBullet = true;
			poolObj.HitCount = HitCount;
			poolObj.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
			poolObj.transform.GetComponentsInChildren<Transform>(true);
			poolObj.Active(_transform.position + new Vector3(0f - base.GetDirection.x, 0f - base.GetDirection.y, 0f), pDirection, TargetMask);
		}
		base.SoundSource.PlaySE("BossSE02", "bs014_copyx20");
	}
}
