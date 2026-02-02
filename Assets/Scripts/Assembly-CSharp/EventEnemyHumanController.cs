using System;
using StageLib;
using UnityEngine;

public class EventEnemyHumanController : EnemyHumanController
{
	private Transform EventTarget;

	private float EventWalkDistance = 100f;

	public override void LogicUpdate()
	{
		if (!Activate)
		{
			return;
		}
		BaseUpdate();
		foreach (BulletDetails bullet in bulletList)
		{
			if (bullet.ShootTransform != null)
			{
				CreateBulletDetail(bullet.bulletData, bullet.refWS, bullet.ShootTransform, bullet.nRecordID, bullet.nBulletRecordID);
			}
			else
			{
				CreateBulletDetail(bullet.bulletData, bullet.refWS, bullet.ShootPosition, bullet.nRecordID, bullet.nBulletRecordID);
			}
		}
		bulletList.Clear();
		UpdateMagazine(ref PlayerWeapons);
		UpdateAimDirection();
		_currentFrame = _animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
		switch (_mainStatus)
		{
		case MainStatus.Idle:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				Target = _enemyAutoAimSystem.GetClosetPlayer();
				EventTarget = FreeAutoAimSystem.GetClosestEventSpot(base.AimTransform);
				if ((bool)EventTarget)
				{
					SetStatus(MainStatus.Walk);
					UpdateDirection();
				}
				break;
			case SubStatus.Phase1:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Idle);
				}
				break;
			}
			break;
		case MainStatus.Walk:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (_currentFrame > 1f)
				{
					SetStatus(_mainStatus, SubStatus.Phase1);
				}
				break;
			}
			break;
		case MainStatus.Crouch:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (_currentFrame > 1f)
				{
					SetStatus(_mainStatus, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if (_currentFrame > 1f && PlayerWeapons[WeaponCurrent].MagazineRemain > 0f)
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
		default:
			throw new ArgumentOutOfRangeException();
		case MainStatus.Debut:
		case MainStatus.Dash:
		case MainStatus.AirDash:
		case MainStatus.Jump:
		case MainStatus.Fall:
		case MainStatus.Dead:
		case MainStatus.Hurt:
			break;
		}
		UpdateGravity();
		Controller.Move((_velocity + _velocityExtra) * GameLogicUpdateManager.m_fFrameLen);
		distanceDelta = Vector3.Distance(base.transform.localPosition, Controller.LogicPosition.vec3) * (Time.deltaTime / GameLogicUpdateManager.m_fFrameLen);
		_velocityExtra = VInt3.zero;
		_velocityShift = VInt3.zero;
	}

	protected override void UpdateDirection(int forceDirection = 0)
	{
		if (forceDirection != 0)
		{
			base.direction = forceDirection;
		}
		else if (StageUpdate.gbIsNetGame)
		{
			if (EventTarget.position.x > (float)Controller.LogicPosition.x)
			{
				base.direction = 1;
			}
			else
			{
				base.direction = -1;
			}
		}
		else if (EventTarget != null && EventTarget.position.x > _transform.position.x)
		{
			base.direction = 1;
		}
		else
		{
			base.direction = -1;
		}
		ModelTransform.localScale = new Vector3(ModelTransform.localScale.x, ModelTransform.localScale.y, Mathf.Abs(ModelTransform.localScale.z) * (float)base.direction);
	}
}
