using UnityEngine;

public class EM070_Controller : Em001Controller
{
	private CollideBullet _bodyCollideBullet;

	protected override void Awake()
	{
		base.Awake();
		HASH_IDLE_LOOP = Animator.StringToHash("EM070@idle_loop");
		HASH_HIDE_START = Animator.StringToHash("EM070@hide_start");
		HASH_HIDE_LOOP = Animator.StringToHash("EM070@hide_loop");
		HASH_HIDE_END = Animator.StringToHash("EM070@hide_end");
		HASH_HURT_LOOP = Animator.StringToHash("EM070@hurt_loop");
		HASH_RUN_LOOP = Animator.StringToHash("EM070@run_loop");
		HASH_SK001 = Animator.StringToHash("EM070@skill_01");
		Transform[] target = _transform.GetComponentsInChildren<Transform>(true);
		_bodyCollideBullet = OrangeBattleUtility.FindChildRecursive(ref target, "ColldeBullet").gameObject.AddOrGetComponent<CollideBullet>();
	}

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
				if (logicFrameNow <= NextReloadFrame[SkillIdx])
				{
					CheckDirection();
				}
				else if (SectorSensor.Look(Controller.LogicPosition.vec3, base.direction, sensorAngle, sensorDistance, targetMask, out hit))
				{
					OnStartShootSk001();
				}
				else if (SectorSensor.Look(Controller.LogicPosition.vec3, -base.direction, sensorAngle, sensorDistance, targetMask, out hit))
				{
					ReverseDirection();
					OnStartShootSk001();
				}
				else
				{
					CheckDirection();
				}
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

	protected override void OnStartShootSk001()
	{
		_velocity.x = 0;
		emState = EmState.TRY_SKL001;
		UpdateLogicToNext(logicTrySkl001);
	}

	protected override void ShootSk001()
	{
		NextReloadFrame[SkillIdx] = logicFrameNow + logicReloadFrame[SkillIdx];
		UpdateLogicToNext(logicSkl001);
		emState = EmState.SKL_001;
	}

	public override void SetActive(bool isActive)
	{
		base.SetActive(isActive);
		if (isActive)
		{
			_bodyCollideBullet.UpdateBulletData(EnemyWeapons[0].BulletData);
			_bodyCollideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			_bodyCollideBullet.Active(targetMask);
		}
		else
		{
			_bodyCollideBullet.BackToPool();
		}
	}
}
