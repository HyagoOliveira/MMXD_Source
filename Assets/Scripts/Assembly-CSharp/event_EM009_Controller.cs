using StageLib;
using UnityEngine;

public class event_EM009_Controller : batController
{
	protected bool EventAI;

	protected Transform EventTarget;

	public override void LogicUpdate()
	{
		if (Activate)
		{
			BaseUpdate();
			if ((bool)EventTarget && AiTimer.GetMillisecond() > EnemyData.n_AI_TIMER)
			{
				AiTimer.TimerStart();
				_direction = (EventTarget.position - _transform.position).normalized;
				ModelTransform.eulerAngles = ((_direction.x < 0f) ? dirLeft : dirRight);
			}
			_velocityExtra = VInt3.zero;
			_velocityShift = VInt3.zero;
		}
	}

	public override void SetActive(bool isActive)
	{
		base.SetActive(isActive);
		if (isActive)
		{
			if (StageUpdate.gbIsNetGame)
			{
				ModelTransform.eulerAngles = ((_direction.x < 0f) ? new Vector3(0f, 180f, 0f) : new Vector3(0f, 135f, 0f));
			}
			_animator.enabled = true;
			_collideBullet.UpdateBulletData(EnemyWeapons[0].BulletData);
			_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			_collideBullet.Active(targetMask);
			EventTarget = FreeAutoAimSystem.GetTarget("EventSpot (3)");
			AiTimer.TimerStart();
			AiTimer.SetMillisecondsOffset(EnemyData.n_AI_TIMER + 1);
		}
		else
		{
			_collideBullet.BackToPool();
			_animator.enabled = false;
		}
	}
}
