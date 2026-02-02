using System;
using UnityEngine;

public class event_EM058_Controller : EM058_Controller
{
	private Transform eventTarget;

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
		case MainStatus.Walk:
			if (!Controller.BelowInBypassRange)
			{
				eventTarget = FreeAutoAimSystem.GetClosestEventSpot(base.AimTransform);
				if ((bool)eventTarget && Math.Sign(eventTarget.position.x - base.AimTransform.position.x) != base.direction)
				{
					base.direction = -base.direction;
					ModelTransform.localEulerAngles = new Vector3(0f, 90 * base.direction, 0f);
					_velocity.x = -_velocity.x;
				}
			}
			break;
		case MainStatus.Roll:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (_rollRecycleTimer.GetMillisecond() > 10000)
				{
					bNeedDead = true;
				}
				if ((bool)Controller.BelowInBypassRange)
				{
					SetStatus(_mainStatus, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if (_currentFrame >= 3f && (bool)Controller.BelowInBypassRange)
				{
					SetStatus(MainStatus.Open);
				}
				break;
			}
			break;
		case MainStatus.Open:
			if (_currentFrame >= 1f)
			{
				SetStatus(MainStatus.Morph);
			}
			break;
		case MainStatus.Morph:
			if (_currentFrame >= 1f)
			{
				SetStatus(MainStatus.Walk);
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
