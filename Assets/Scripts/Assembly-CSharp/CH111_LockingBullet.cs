using UnityEngine;

public class CH111_LockingBullet : LockingBullet
{
	protected SKILL_TABLE _tLinkSkill;

	protected bool _bTriggered;

	public override void Active(Transform pTransform, Vector3 pDirection, LayerMask pTargetMask, IAimTarget pTarget = null)
	{
		base.Active(pTransform, pDirection, pTargetMask, pTarget);
		_bTriggered = false;
	}

	public override void Active(Vector3 pPos, Vector3 pDirection, LayerMask pTargetMask, IAimTarget pTarget = null)
	{
		base.Active(pPos, pDirection, pTargetMask, pTarget);
		_bTriggered = false;
	}

	public override void OnStart()
	{
		base.OnStart();
		if (BulletData != null && BulletData.n_LINK_SKILL > 0)
		{
			_tLinkSkill = ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT[BulletData.n_LINK_SKILL];
			OrangeCharacter orangeCharacter = refPBMShoter.SOB as OrangeCharacter;
			if ((bool)orangeCharacter)
			{
				orangeCharacter.tRefPassiveskill.ReCalcuSkill(ref _tLinkSkill);
			}
		}
	}

	public override void LateUpdateFunc()
	{
		base.LateUpdateFunc();
		if (Target != null && Target.VanishStatus)
		{
			Target = null;
			FindTarget(TrackPriority.PlayerFirst);
			if (Target != null)
			{
				_transform.SetParent(Target.AimTransform);
			}
			else
			{
				_transform.SetParentNull();
			}
		}
	}

	public override void OnTriggerHit(Collider2D col)
	{
		if (CanTriggerHit)
		{
			base.OnTriggerHit(col);
			CheckTriggerLinkBullet(col);
		}
	}

	protected void CheckTriggerLinkBullet(Collider2D col)
	{
		if (refPBMShoter != null && !_bTriggered)
		{
			OrangeCharacter orangeCharacter = refPBMShoter.SOB as OrangeCharacter;
			if (!(orangeCharacter == null) && orangeCharacter.IsLocalPlayer && _tLinkSkill != null && (_tLinkSkill.n_MAGAZINE_TYPE != 2 || refPBMShoter.nMeasureNow >= _tLinkSkill.n_NUM_SHOOT))
			{
				_bTriggered = true;
				refPBMShoter.AddMeasure(-_tLinkSkill.n_NUM_SHOOT);
				CreateBulletDetail(_tLinkSkill);
			}
		}
	}
}
