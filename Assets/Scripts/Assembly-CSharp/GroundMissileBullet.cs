using System;
using UnityEngine;

public class GroundMissileBullet : BasicBullet
{
	private bool AimDone;

	private float _maxSpeed = 10f;

	private float _accelerate = 0.02f;

	private Vector3 _velocityHold;

	protected override void Awake()
	{
		base.Awake();
		ForceAIS = true;
	}

	public override void LateUpdateFunc()
	{
		base.LateUpdateFunc();
		if (ActivateTimer.IsStarted() && ActivateTimer.GetMillisecond() >= 500 && !AimDone)
		{
			IAimTarget closetPlayer = NeutralAIS.GetClosetPlayer();
			if (closetPlayer != null)
			{
				DoAim(closetPlayer.AimTransform);
			}
			else
			{
				DoAim();
			}
		}
	}

	public override void Active(Transform pTransform, Vector3 pDirection, LayerMask pTargetMask, IAimTarget pTarget = null)
	{
		base.Active(pTransform, pDirection, pTargetMask, pTarget);
		NeutralAIS.UpdateAimRange(BulletData.f_DISTANCE * 1.5f);
	}

	public override void Active(Vector3 pPos, Vector3 pDirection, LayerMask pTargetMask, IAimTarget pTarget = null)
	{
		base.Active(pPos, pDirection, pTargetMask, pTarget);
		NeutralAIS.UpdateAimRange(BulletData.f_DISTANCE * 1.5f);
	}

	public override void BackToPool()
	{
		AimDone = false;
		base.BackToPool();
	}

	private void DoAim(Transform target = null)
	{
		AimDone = true;
		if ((bool)target)
		{
			_velocityHold = Velocity;
			Velocity = Vector3.zero;
			float from = Vector2.SignedAngle(Vector3.right, Direction);
			float num = Vector2.SignedAngle(Vector3.right, (target.position - _transform.position).normalized);
			if (num < -90f)
			{
				num = 360f - Mathf.Abs(num);
			}
			LeanTween.value(base.gameObject, from, num, 0.5f).setOnUpdate(delegate(float f)
			{
				Direction = new Vector3(0f, 0f, f);
				_transform.eulerAngles = Direction;
			}).setOnComplete((Action)delegate
			{
				Velocity = _velocityHold;
			});
		}
	}
}
