using UnityEngine;

public class LockingBullet : BasicBullet
{
	protected int trackingEndFrame;

	private int trackingFreezeFrame;

	protected virtual bool CanTriggerHit
	{
		get
		{
			return Phase == BulletPhase.Splash;
		}
	}

	public override void UpdateBulletData(SKILL_TABLE pData, string owner = "", int nInRecordID = 0, int nInNetID = 0, int nDirection = 1)
	{
		base.UpdateBulletData(pData, owner, nInRecordID, nInNetID, nDirection);
		if (NeutralAIS == null)
		{
			OrangeBattleUtility.AddNeutralAutoAimSystem(base.transform, out NeutralAIS);
		}
		trackingEndFrame = GameLogicUpdateManager.GameFrame + (int)((float)TrackingData.n_ENDTIME_1 * 0.001f / GameLogicUpdateManager.m_fFrameLen);
		trackingFreezeFrame = GameLogicUpdateManager.GameFrame + (int)((float)TrackingData.n_ENDTIME_2 * 0.001f / GameLogicUpdateManager.m_fFrameLen);
	}

	public override void Active(Transform pTransform, Vector3 pDirection, LayerMask pTargetMask, IAimTarget pTarget = null)
	{
		base.Active(pTransform, pDirection, pTargetMask, pTarget);
		Target = pTarget;
		FindTarget(TrackPriority.PlayerFirst);
		if (IsTargetWithinRange(Target))
		{
			_transform.SetParent(Target.AimTransform);
			_transform.localPosition = Target.AimPoint;
		}
		else
		{
			Target = null;
		}
		oldPos = _transform.position;
	}

	public override void Active(Vector3 pPos, Vector3 pDirection, LayerMask pTargetMask, IAimTarget pTarget = null)
	{
		base.Active(pPos, pDirection, pTargetMask, pTarget);
		Target = pTarget;
		FindTarget(TrackPriority.PlayerFirst);
		if (IsTargetWithinRange(Target))
		{
			_transform.SetParent(Target.AimTransform);
			_transform.localPosition = Target.AimPoint;
		}
		else
		{
			Target = null;
		}
		oldPos = _transform.position;
	}

	protected virtual bool IsTargetWithinRange(IAimTarget aimTarget)
	{
		if (aimTarget != null)
		{
			return Vector2.Distance(MasterPosition, Target.AimTransform.position + Target.AimPoint) <= BulletData.f_DISTANCE;
		}
		return false;
	}

	public override void OnStart()
	{
		base.OnStart();
	}

	protected override void MoveBullet()
	{
	}

	protected override void PhaseNormal()
	{
		if (GameLogicUpdateManager.GameFrame >= trackingEndFrame)
		{
			_transform.SetParentNull();
			Phase = BulletPhase.Boomerang;
		}
	}

	protected override void PhaseBoomerang()
	{
		if (GameLogicUpdateManager.GameFrame >= trackingFreezeFrame)
		{
			DoBoomerang();
		}
	}

	protected virtual void DoBoomerang()
	{
		_capsuleCollider.enabled = true;
		CheckRollBack();
		oldPos = _transform.position;
		if (_rigidbody2D != null)
		{
			_rigidbody2D.WakeUp();
		}
	}

	protected override void TrackingTarget()
	{
		if (Target != null)
		{
			FindTarget(TrackPriority.PlayerFirst);
		}
	}

	public override void OnTriggerHit(Collider2D col)
	{
		if (CanTriggerHit)
		{
			base.OnTriggerHit(col);
		}
	}

	public override void LateUpdateFunc()
	{
		base.LateUpdateFunc();
		SetBulletRotation();
	}

	protected virtual void SetBulletRotation()
	{
		_transform.rotation = Quaternion.identity;
		if (Phase == BulletPhase.Normal && Target != null)
		{
			_transform.localPosition = Target.AimPoint;
		}
	}
}
