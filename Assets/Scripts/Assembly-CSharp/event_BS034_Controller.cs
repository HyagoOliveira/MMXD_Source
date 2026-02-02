using System;
using CodeStage.AntiCheat.ObscuredTypes;
using UnityEngine;

public class event_BS034_Controller : BS034_Controller
{
	private new int jumpDistance;

	private Transform eventTarget;

	private void UpdateDirection(int forceDirection = 0)
	{
		if (forceDirection != 0)
		{
			base.direction = forceDirection;
		}
		else if ((bool)eventTarget && Math.Sign(eventTarget.position.x - base.AimTransform.position.x) != base.direction)
		{
			base.direction = -base.direction;
		}
		ModelTransform.localScale = new Vector3(ModelTransform.localScale.x, ModelTransform.localScale.y, Mathf.Abs(ModelTransform.localScale.z) * (float)(-base.direction));
	}

	public override void LogicUpdate()
	{
		if (!Activate && _mainStatus != MainStatus.Debut)
		{
			return;
		}
		BaseUpdate();
		_currentFrame = _animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
		switch (_mainStatus)
		{
		case MainStatus.Idle:
			eventTarget = FreeAutoAimSystem.GetClosestEventSpot(base.AimTransform);
			if ((bool)eventTarget && Math.Sign(eventTarget.position.x - base.AimTransform.position.x) != base.direction)
			{
				UpdateDirection();
			}
			SetStatus(MainStatus.Walk);
			break;
		case MainStatus.Debut:
			SetStatus(MainStatus.Idle);
			break;
		case MainStatus.ShootClaw:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (_currentFrame > 1f)
				{
					SetStatus(_mainStatus, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if (_currentFrame > 1f)
				{
					SetStatus(_mainStatus, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Idle);
				}
				break;
			}
			break;
		case MainStatus.PullClaw:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (_currentFrame > 1f)
				{
					SetStatus(_mainStatus, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if (_currentFrame > 1f)
				{
					SetStatus(_mainStatus, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Idle);
				}
				break;
			}
			break;
		case MainStatus.PushAttack:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (_currentFrame > 1f)
				{
					SetStatus(_mainStatus, SubStatus.Phase1);
				}
				else if (_currentFrame > 0.35f)
				{
					_velocity.x = base.direction * WalkSpeed;
				}
				break;
			case SubStatus.Phase1:
				if (_currentFrame > 5f)
				{
					SetStatus(_mainStatus, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Idle);
				}
				break;
			}
			break;
		case MainStatus.Walk:
			eventTarget = FreeAutoAimSystem.GetClosestEventSpot(base.AimTransform);
			if ((bool)eventTarget && Math.Sign(eventTarget.position.x - base.AimTransform.position.x) != base.direction)
			{
				UpdateDirection();
				_velocity.x = base.direction * WalkSpeed;
			}
			break;
		}
		UpdateGravity();
		Controller.Move((_velocity + _velocityExtra) * GameLogicUpdateManager.m_fFrameLen);
		distanceDelta = Vector3.Distance(base.transform.localPosition, Controller.LogicPosition.vec3) * (Time.deltaTime / GameLogicUpdateManager.m_fFrameLen);
		_velocityExtra = VInt3.zero;
		_velocityShift = VInt3.zero;
	}

	public override ObscuredInt Hurt(HurtPassParam tHurtPassParam)
	{
		return base.Hurt(tHurtPassParam);
	}
}
