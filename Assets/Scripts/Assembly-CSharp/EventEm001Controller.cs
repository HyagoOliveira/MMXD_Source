using System;
using UnityEngine;

public class EventEm001Controller : Em001Controller
{
	private Transform eventTarget;

	protected override void AI_mob_001()
	{
		switch (emState)
		{
		case EmState.INIT:
			_animator.Play(HASH_IDLE_LOOP, 0, 0f);
			_velocity.x = moveSpdX * base.direction;
			break;
		case EmState.IDLE:
			_animator.Play(HASH_IDLE_LOOP, 0);
			emState = EmState.RUN_LOOP;
			break;
		case EmState.HIDE_START:
			if (logicFrameNow > logicToNext)
			{
				emState = EmState.HIDE_LOOP;
				_animator.Play(HASH_HIDE_LOOP, 0, 0f);
				UpdateLogicToNext(logicHideLoop);
			}
			break;
		case EmState.HIDE_LOOP:
			if (logicFrameNow > logicToNext)
			{
				emState = EmState.HIDE_END;
				_animator.Play(HASH_HIDE_END, 0, 0f);
				UpdateLogicToNext(logicHideEnd);
			}
			break;
		case EmState.HIDE_END:
			if (logicFrameNow > logicToNext)
			{
				emState = EmState.IDLE;
			}
			break;
		case EmState.TRY_SKL001:
			if (logicFrameNow > logicToNext)
			{
				ShootSk001();
			}
			break;
		case EmState.SKL_001:
			if (logicFrameNow > logicToNext)
			{
				emState = EmState.HIDE_START;
				_animator.Play(HASH_HIDE_START, 0, 0f);
				UpdateLogicToNext(logicHideStart);
			}
			break;
		case EmState.RUN_LOOP:
			if (_velocity.y != 0)
			{
				SkillIdx = 0;
				CheckDirection();
			}
			break;
		case EmState.HURT_LOOP:
		case EmState.DIE:
			break;
		}
	}

	protected override void AI_mob_002()
	{
		switch (emState)
		{
		case EmState.INIT:
			_animator.Play(HASH_IDLE_LOOP, 0, 0f);
			_velocity.x = 0;
			break;
		case EmState.IDLE:
			emState = EmState.HIDE_START;
			_animator.Play(HASH_HIDE_START, 0, 0f);
			UpdateLogicToNext(logicHideStart);
			break;
		case EmState.HIDE_START:
			if (logicFrameNow > logicToNext)
			{
				emState = EmState.HIDE_LOOP;
				_animator.Play(HASH_HIDE_LOOP, 0, 0f);
				UpdateLogicToNext(logicHideLoop);
			}
			break;
		case EmState.HIDE_LOOP:
			SkillIdx = 0;
			if (logicFrameNow > NextReloadFrame[SkillIdx])
			{
				if (SectorSensor.Look(Controller.LogicPosition.vec3, base.direction, sensorAngle, sensorDistance, targetMask, out hit))
				{
					OnStartShootSk001();
				}
				else if (SectorSensor.Look(Controller.LogicPosition.vec3, -base.direction, sensorAngle, sensorDistance, targetMask, out hit))
				{
					ReverseDirection();
					OnStartShootSk001();
				}
			}
			break;
		case EmState.HIDE_END:
			if (logicFrameNow > logicToNext)
			{
				emState = EmState.IDLE;
			}
			break;
		case EmState.TRY_SKL001:
			if (logicFrameNow > logicToNext)
			{
				ShootSk001();
			}
			break;
		case EmState.SKL_001:
			if (logicFrameNow > logicToNext)
			{
				emState = EmState.HIDE_START;
				_animator.Play(HASH_HIDE_START, 0, 0f);
				UpdateLogicToNext(logicHideStart);
			}
			break;
		case EmState.HURT_LOOP:
		case EmState.RUN_LOOP:
		case EmState.DIE:
			break;
		}
	}

	protected override void CheckDirection()
	{
		_velocity.x = moveSpdX * base.direction;
		_animator.Play(HASH_RUN_LOOP, 0);
		eventTarget = FreeAutoAimSystem.GetClosestEventSpot(base.AimTransform);
		if ((bool)eventTarget && Math.Sign(eventTarget.position.x - base.AimTransform.position.x) != base.direction)
		{
			ReverseDirection();
		}
	}
}
