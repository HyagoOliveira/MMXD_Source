using System;
using UnityEngine;

public class CrossBullet : ParabolaBullet
{
	[SerializeField]
	private float RotateSpd = 600f;

	private bool playFX;

	private OrangeTimer FXtimer;

	private int preEndFrame = -1;

	protected override void Awake()
	{
		base.Awake();
		FXtimer = OrangeTimerManager.GetTimer();
	}

	public override void Active(Vector3 pPos, Vector3 pDirection, LayerMask pTargetMask, IAimTarget pTarget)
	{
		base.Active(pPos, pDirection, pTargetMask, pTarget);
		playFX = false;
		FXtimer.TimerStart();
		preEndFrame = base.EndLogicFrame;
	}

	public override void UpdateFunc()
	{
		base.UpdateFunc();
		_transform.Rotate(0f, 0f, RotateSpd * Time.deltaTime);
		if (!isHit)
		{
			base.EndLogicFrame++;
		}
		else
		{
			if (!playFX)
			{
				playFX = true;
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(BulletData.s_HIT_FX, base.transform.position, Quaternion.identity, Array.Empty<object>());
				PlaySE(_HitGuardSE[0], _HitGuardSE[1], isForceSE);
			}
			if (base.NowLogicFrame >= preEndFrame)
			{
				BackToPool();
			}
			else
			{
				isHit = false;
			}
		}
		if (FXtimer.GetMillisecond() > 500)
		{
			playFX = false;
			FXtimer.TimerReset();
			FXtimer.TimerStart();
		}
	}

	public override void BackToPool()
	{
		FXtimer.TimerReset();
		playFX = false;
		base.transform.localRotation = Quaternion.identity;
		base.BackToPool();
	}
}
