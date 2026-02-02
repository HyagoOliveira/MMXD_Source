using System;
using UnityEngine;

public class AxeBullet : ParabolaBullet
{
	[SerializeField]
	private float _rotateSpd = 600f;

	[SerializeField]
	private float _dropSpd = 6.5f;

	[SerializeField]
	private float _gravity = -30f;

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
		dropSpd = _dropSpd;
		g = _gravity;
		preEndFrame = base.EndLogicFrame;
	}

	public override void UpdateFunc()
	{
		base.UpdateFunc();
		_transform.Rotate(0f, 0f, _rotateSpd * Time.deltaTime);
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
