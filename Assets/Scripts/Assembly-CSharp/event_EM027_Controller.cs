using UnityEngine;

public class event_EM027_Controller : EM027_Controller
{
	private Transform eventTarget;

	protected override void UpdateDirection(int forceDirection = 0)
	{
		if (forceDirection != 0)
		{
			base.direction = forceDirection;
		}
		else if (eventTarget != null && eventTarget.position.x > _transform.position.x)
		{
			base.direction = 1;
		}
		else
		{
			base.direction = -1;
		}
		ModelTransform.localScale = new Vector3(ModelTransform.localScale.x, ModelTransform.localScale.y, Mathf.Abs(ModelTransform.localScale.z) * (float)base.direction);
	}

	public override void LogicUpdate()
	{
		if (!Activate)
		{
			return;
		}
		BaseUpdate();
		_currentFrame = _animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
		switch (_mainStatus)
		{
		case MainStatus.Idle:
			eventTarget = FreeAutoAimSystem.GetClosestEventSpot(base.AimTransform);
			if (!eventTarget)
			{
				break;
			}
			UpdateDirection();
			if (Mathf.Abs(eventTarget.position.x - _transform.position.x) < _shootRange)
			{
				if (EnemyWeapons[0].LastUseTimer.GetMillisecond() > EnemyWeapons[0].BulletData.n_RELOAD)
				{
					SetStatus(MainStatus.Shoot);
				}
			}
			else
			{
				SetStatus(MainStatus.Walk);
			}
			break;
		case MainStatus.Shoot:
			if (_currentFrame >= 1f)
			{
				SetStatus(MainStatus.Idle);
			}
			if (_currentFrame > 0.46f && !_shootDone)
			{
				_shootDone = true;
				EnemyWeapons[0].LastUseTimer.TimerStart();
				BulletBase.TryShotBullet(EnemyWeapons[0].BulletData, _shootTransform, base.direction * Vector3.right, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
			}
			break;
		case MainStatus.Walk:
			eventTarget = FreeAutoAimSystem.GetClosestEventSpot(base.AimTransform);
			if ((bool)eventTarget)
			{
				UpdateDirection();
				_velocity.x = base.direction * WalkSpeed;
			}
			else
			{
				SetStatus(MainStatus.Idle);
			}
			break;
		}
		UpdateGravity();
		Controller.Move((_velocity + _velocityExtra) * GameLogicUpdateManager.m_fFrameLen);
		distanceDelta = Vector3.Distance(base.transform.localPosition, Controller.LogicPosition.vec3) * (Time.deltaTime / GameLogicUpdateManager.m_fFrameLen);
		_velocityExtra = VInt3.zero;
		_velocityShift = VInt3.zero;
	}
}
