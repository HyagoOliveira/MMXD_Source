using UnityEngine;

public class CH085_BallBullet : BasicBullet
{
	private readonly int HitInterval = (int)(0.5f / GameLogicUpdateManager.m_fFrameLen);

	private bool alreadyStop;

	protected Vector3 _vTargetPos = Vector3.zero;

	private int StopFrame;

	private int CurrentStopFrame;

	private int nextHitFrame;

	private SKILL_TABLE linkSkl;

	public override void UpdateBulletData(SKILL_TABLE pData, string owner = "", int nInRecordID = 0, int nInNetID = 0, int nDirection = 1)
	{
		base.UpdateBulletData(pData, owner, nInRecordID, nInNetID, nDirection);
		alreadyStop = false;
		CurrentStopFrame = 0;
		if (pData.n_ROLLBACK == 0)
		{
			StopFrame = (int)(3f / GameLogicUpdateManager.m_fFrameLen);
		}
		else
		{
			StopFrame = (int)((float)pData.n_ROLLBACK / 1000f / GameLogicUpdateManager.m_fFrameLen);
		}
		CheckUsePullBullet();
	}

	public override void OnStart()
	{
		base.OnStart();
		if (Target == null && refPBMShoter.SOB as OrangeCharacter != null)
		{
			OrangeCharacter orangeCharacter = refPBMShoter.SOB as OrangeCharacter;
			Target = orangeCharacter.PlayerAutoAimSystem.AutoAimTarget;
		}
		if (Target != null)
		{
			_vTargetPos = Target.AimTransform.position + Target.AimPoint;
			_vTargetPos.z = _transform.position.z;
		}
	}

	protected override void PhaseNormal()
	{
		base.PhaseNormal();
		if (Phase == BulletPhase.Normal)
		{
			if (CheckHitTarget())
			{
				if (CurrentStopFrame == 0)
				{
					base.SoundSource.PlaySE("SkillSE_GATE", "gt_hole02_lp");
				}
				CurrentStopFrame = GameLogicUpdateManager.GameFrame + StopFrame;
				nextHitFrame = GameLogicUpdateManager.GameFrame + HitInterval;
				Phase = BulletPhase.Boomerang;
				CreatePullBullet();
			}
		}
		else
		{
			if (CurrentStopFrame == 0)
			{
				base.SoundSource.PlaySE("SkillSE_GATE", "gt_hole02_lp");
			}
			CurrentStopFrame = GameLogicUpdateManager.GameFrame + StopFrame;
			CreatePullBullet();
		}
	}

	protected override void PhaseBoomerang()
	{
		int gameFrame = GameLogicUpdateManager.GameFrame;
		if (gameFrame > CurrentStopFrame)
		{
			Phase = BulletPhase.Result;
		}
		else if (gameFrame > nextHitFrame)
		{
			nextHitFrame = gameFrame + HitInterval;
			_hitList.Clear();
		}
	}

	private void CheckUsePullBullet()
	{
		if (BulletData.n_LINK_SKILL != 0 && ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT.TryGetNewValue(BulletData.n_LINK_SKILL, out linkSkl) && refPSShoter != null)
		{
			refPSShoter.ReCalcuSkill(ref linkSkl);
		}
	}

	private void CreatePullBullet()
	{
		if (linkSkl != null)
		{
			CreateBulletDetail(linkSkl);
		}
	}

	protected override void PhaseResult()
	{
		if (!alreadyStop)
		{
			alreadyStop = true;
			Phase = BulletPhase.Boomerang;
		}
		else
		{
			base.PhaseResult();
		}
	}

	protected bool CheckHitTarget()
	{
		if (Target != null && Mathf.Abs(Vector3.Distance(_vTargetPos, _transform.position)) < 1f)
		{
			return true;
		}
		return false;
	}

	public override void BackToPool()
	{
		base.SoundSource.PlaySE("SkillSE_GATE", "gt_hole02_stop");
		alreadyStop = false;
		StopFrame = 0;
		CurrentStopFrame = 0;
		base.BackToPool();
	}
}
