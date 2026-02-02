using UnityEngine;

public class FSFlashBombBullet : FSkillBullet
{
	public override void OnStart()
	{
		_capsuleCollider.size = _colliderSize;
		base.OnStart();
	}

	public override void UpdateBulletData(SKILL_TABLE pData, string owner = "", int nInRecordID = 0, int nInNetID = 0, int nDirection = 1)
	{
		base.UpdateBulletData(pData, owner, nInRecordID, nDirection);
		if (BulletData.n_MOTION_DEF > 0)
		{
			SKILL_TABLE value = null;
			if (ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT.TryGetValue(BulletData.n_MOTION_DEF, out value))
			{
				BulletBase.PreloadBullet<CollideBullet>(value);
			}
		}
	}

	public override void BackToPool()
	{
		if (BulletData.n_MOTION_DEF > 0)
		{
			CreateSubBullet(BulletData.n_MOTION_DEF);
		}
		base.BackToPool();
	}

	protected void CreateSubBullet(int skillId)
	{
		SKILL_TABLE value = null;
		if (ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT.TryGetNewValue(skillId, out value))
		{
			refPSShoter.ReCalcuSkill(ref value);
			CollideBullet poolObj = MonoBehaviourSingleton<PoolManager>.Instance.GetPoolObj<CollideBullet>(value.s_MODEL);
			WeaponStatus weaponStatus = new WeaponStatus
			{
				nHP = nHp,
				nATK = nOriginalATK,
				nCRI = nOriginalCRI,
				nHIT = nHit - refPSShoter.GetAddStatus(8, nWeaponCheck),
				nCriDmgPercent = nCriDmgPercent,
				nReduceBlockPercent = nReduceBlockPercent,
				nWeaponCheck = nWeaponCheck,
				nWeaponType = nWeaponType
			};
			PerBuffManager.BuffStatus tBuffStatus = new PerBuffManager.BuffStatus
			{
				fAtkDmgPercent = fDmgFactor - 100f,
				fCriPercent = fCriFactor - 100f,
				fCriDmgPercent = fCriDmgFactor - 100f,
				fMissPercent = fMissFactor,
				refPBM = refPBMShoter,
				refPS = refPSShoter
			};
			poolObj.UpdateBulletData(value, Owner);
			poolObj.BulletLevel = BulletLevel;
			poolObj.isSubBullet = true;
			poolObj.SetBulletAtk(weaponStatus, tBuffStatus);
			poolObj.transform.SetPositionAndRotation(_transform.position, Quaternion.identity);
			poolObj.Active(_transform.position, Direction, TargetMask);
		}
	}
}
