using System;
using UnityEngine;

public class CH043_skill01Bullet : CollideBullet
{
	public override void Active(Transform pTransform, Vector3 pDirection, LayerMask pTargetMask, IAimTarget pTarget = null)
	{
		base.Active(pTransform, pDirection, pTargetMask, pTarget);
		_transform.eulerAngles = new Vector3(0f, 0f, 0f);
		CreateLinkSkillBullet();
	}

	public override void Active(Vector3 pPos, Vector3 pDirection, LayerMask pTargetMask, IAimTarget pTarget = null)
	{
		base.Active(pPos, pDirection, pTargetMask, pTarget);
		_transform.eulerAngles = new Vector3(0f, 0f, 0f);
		CreateLinkSkillBullet();
	}

	public override void Hit(Collider2D col)
	{
		if (BulletData.n_TARGET == 3)
		{
			OrangeCharacter component = col.transform.GetComponent<OrangeCharacter>();
			if ((bool)component && component.sNetSerialID != refPBMShoter.SOB.sNetSerialID)
			{
				return;
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
			StageObjParam component2 = col.transform.GetComponent<StageObjParam>();
			if ((bool)component2)
			{
				IAimTarget aimTarget = component2.tLinkSOB as IAimTarget;
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
	}

	protected void CreateLinkSkillBullet()
	{
		if (BulletData.n_LINK_SKILL != 0)
		{
			SKILL_TABLE tSKILL_TABLE = ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT[BulletData.n_LINK_SKILL];
			OrangeCharacter orangeCharacter = null;
			if (refPBMShoter.SOB as OrangeCharacter != null)
			{
				orangeCharacter = refPBMShoter.SOB as OrangeCharacter;
				orangeCharacter.tRefPassiveskill.ReCalcuSkill(ref tSKILL_TABLE);
			}
			BulletBase poolObj = MonoBehaviourSingleton<PoolManager>.Instance.GetPoolObj<BulletBase>(tSKILL_TABLE.s_MODEL);
			if (orangeCharacter != null)
			{
				poolObj.UpdateBulletData(tSKILL_TABLE, Owner, orangeCharacter.GetNowRecordNO(), orangeCharacter.nBulletRecordID++);
			}
			else
			{
				poolObj.UpdateBulletData(tSKILL_TABLE, Owner);
			}
			poolObj.BulletLevel = BulletLevel;
			WeaponStatus weaponStatus = new WeaponStatus();
			weaponStatus.nHP = nHp;
			weaponStatus.nATK = nOriginalATK;
			weaponStatus.nCRI = nOriginalCRI;
			weaponStatus.nHIT = nHit - refPSShoter.GetAddStatus(8, nWeaponCheck);
			weaponStatus.nCriDmgPercent = nCriDmgPercent;
			weaponStatus.nReduceBlockPercent = nReduceBlockPercent;
			weaponStatus.nWeaponCheck = nWeaponCheck;
			weaponStatus.nWeaponType = nWeaponType;
			PerBuffManager.BuffStatus buffStatus = new PerBuffManager.BuffStatus();
			buffStatus.fAtkDmgPercent = fDmgFactor - 100f;
			buffStatus.fCriPercent = fCriFactor - 100f;
			buffStatus.fCriDmgPercent = fCriDmgFactor - 100f;
			buffStatus.fMissPercent = fMissFactor;
			buffStatus.refPBM = refPBMShoter;
			buffStatus.refPS = refPSShoter;
			poolObj.SetBulletAtk(weaponStatus, buffStatus);
			poolObj.Active(base.transform, Direction, TargetMask);
		}
	}

	private new void OnTriggerStay2D(Collider2D col)
	{
		if (!ManagedSingleton<StageHelper>.Instance.bEnemyActive || MonoBehaviourSingleton<OrangeGameManager>.Instance.IsGamePause || !IsActivate || ((1 << col.gameObject.layer) & (int)TargetMask) == 0 || col.isTrigger)
		{
			return;
		}
		StageObjParam component = col.GetComponent<StageObjParam>();
		if (component != null && component.tLinkSOB != null && ((1 << col.gameObject.layer) & (int)BlockMask) == 0)
		{
			if (component.tLinkSOB.GetSOBType() == 4)
			{
				PetControllerBase component2 = col.GetComponent<PetControllerBase>();
				if (component2 != null && component2.ignoreColliderBullet() && _transform.GetComponentInParent<EnemyControllerBase>() != null)
				{
					return;
				}
			}
			if ((int)component.tLinkSOB.Hp > 0)
			{
				Hit(col);
			}
		}
		else
		{
			PlayerCollider component3 = col.GetComponent<PlayerCollider>();
			if (component3 != null && component3.IsDmgReduceShield())
			{
				Hit(col);
			}
			else if (col.gameObject.GetComponentInParent<StageHurtObj>() != null)
			{
				Hit(col);
			}
		}
	}
}
