using System;
using UnityEngine;

public class CH134_LockingBullet : LockingBullet
{
	private int backToPoolFrame;

	private int animationHash = Animator.StringToHash("ch134_Voss_skill_02_start");

	[SerializeField]
	private Animator _animator;

	private Vector3 atkOffsetRight = new Vector3(-0.8f, 0f, 0f);

	private Vector3 atkOffsetLeft = new Vector3(0.8f, 0f, 0f);

	private Vector3 atkScaleRight = new Vector3(1f, 1f, 1f);

	private Vector3 atkScaleLeft = new Vector3(1f, 1f, -1f);

	public override void Active(Transform pTransform, Vector3 pDirection, LayerMask pTargetMask, IAimTarget pTarget = null)
	{
		base.Active(pTransform, pDirection, pTargetMask, pTarget);
		_animator.enabled = true;
		_animator.Play(animationHash, 0, 0f);
	}

	public override void Active(Vector3 pPos, Vector3 pDirection, LayerMask pTargetMask, IAimTarget pTarget = null)
	{
		base.Active(pPos, pDirection, pTargetMask, pTarget);
		_animator.enabled = true;
		_animator.Play(animationHash, 0, 0f);
	}

	protected override void DoBoomerang()
	{
		backToPoolFrame = GameLogicUpdateManager.GameFrame + (int)((float)TrackingData.n_ENDTIME_3 * 0.001f / GameLogicUpdateManager.m_fFrameLen);
		base.DoBoomerang();
	}

	public override void BackToPool()
	{
		if (GameLogicUpdateManager.GameFrame >= backToPoolFrame)
		{
			if (FxEnd != string.Empty)
			{
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FxEnd, _animator.transform.position, Quaternion.identity, Array.Empty<object>());
			}
			base.BackToPool();
			_animator.enabled = false;
		}
	}

	protected override bool IsTargetWithinRange(IAimTarget aimTarget)
	{
		if (aimTarget != null)
		{
			if (aimTarget.AimPosition.x - base.transform.position.x > 0f)
			{
				_animator.transform.localPosition = atkOffsetRight;
				_animator.transform.localScale = atkScaleRight;
			}
			else
			{
				_animator.transform.localPosition = atkOffsetLeft;
				_animator.transform.localScale = atkScaleLeft;
			}
		}
		return base.IsTargetWithinRange(aimTarget);
	}
}
