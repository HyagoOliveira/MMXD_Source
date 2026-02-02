using System;
using System.Collections;
using UnityEngine;

public class CH139_BeamBullet : BeamBullet
{
	protected override IEnumerator OnStartMove()
	{
		CreateLinkBullet();
		string s_USE_FX = BulletData.s_USE_FX;
		if (!string.IsNullOrEmpty(s_USE_FX) && !isSubBullet)
		{
			if (BulletData.n_USE_FX_FOLLOW == 0)
			{
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(s_USE_FX, _transform.position, _transform.rotation * BulletQuaternion, Array.Empty<object>());
			}
			else
			{
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(s_USE_FX, _transform, BulletQuaternion, Array.Empty<object>());
			}
		}
		return base.OnStartMove();
	}

	protected void CreateLinkBullet()
	{
		if (BulletData.n_LINK_SKILL != 0)
		{
			int n_LINK_SKILL = BulletData.n_LINK_SKILL;
			SKILL_TABLE tSKILL_TABLE = ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT[n_LINK_SKILL];
			if (refPBMShoter.SOB as OrangeCharacter != null)
			{
				(refPBMShoter.SOB as OrangeCharacter).tRefPassiveskill.ReCalcuSkill(ref tSKILL_TABLE);
			}
			BeamBullet beamBullet = null;
			if (MonoBehaviourSingleton<PoolManager>.Instance.IsPreload(tSKILL_TABLE.s_MODEL))
			{
				beamBullet = MonoBehaviourSingleton<PoolManager>.Instance.GetPoolObj<BeamBullet>(tSKILL_TABLE.s_MODEL);
			}
			if (!(beamBullet == null))
			{
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
				beamBullet.UpdateBulletData(tSKILL_TABLE, Owner);
				beamBullet.SetBulletAtk(weaponStatus, buffStatus);
				beamBullet.BulletLevel = BulletLevel;
				beamBullet.isSubBullet = false;
				beamBullet.transform.SetPositionAndRotation(_transform.position, Quaternion.identity);
				beamBullet.Active(_transform.position, Direction, TargetMask);
			}
		}
	}
}
