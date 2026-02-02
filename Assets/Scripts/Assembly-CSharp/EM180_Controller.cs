using System;
using UnityEngine;

public class EM180_Controller : Em001Controller
{
	[SerializeField]
	protected float _fMoveSpeedMultiple = 1.5f;

	[SerializeField]
	protected int _nWalkTime = 3000;

	[SerializeField]
	protected float _fAttackRange = 1f;

	protected CollideBullet _skill1Bullet;

	protected FxBase _fxSkill1;

	protected override void Awake()
	{
		base.Awake();
		_collideBullet = OrangeBattleUtility.FindChildRecursive(_transform, "CollideBullet", true).gameObject.AddOrGetComponent<CollideBullet>();
		_skill1Bullet = OrangeBattleUtility.FindChildRecursive(_transform, "Skill1Bullet", true).gameObject.AddOrGetComponent<CollideBullet>();
	}

	protected override void Start()
	{
		base.Start();
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_Blanka_Metall_000", 5);
	}

	public override void SetActive(bool isActive)
	{
		moveSpdX = new VInt(0.6f * _fMoveSpeedMultiple);
		base.SetActive(isActive);
		if (isActive)
		{
			_collideBullet.UpdateBulletData(EnemyWeapons[0].BulletData);
			_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			_collideBullet.Active(targetMask);
		}
		else
		{
			base.SoundSource.StopAll();
			_collideBullet.BackToPool();
		}
	}

	public override void UpdateEnemyID(int id)
	{
		base.UpdateEnemyID(id);
	}

	protected override void OnStartShootSk001()
	{
		_velocity.x = 0;
		emState = EmState.HIDE_START;
		UpdateLogicToNext(logicHideStart);
		_animator.Play(HASH_HIDE_START, 0, 0f);
	}

	protected override void ShootSk001()
	{
		if (EnemyWeapons.Length > SkillIdx)
		{
			_skill1Bullet.UpdateBulletData(EnemyWeapons[SkillIdx].BulletData);
			_skill1Bullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			_skill1Bullet.Active(targetMask);
			base.SoundSource.PlaySE("EnemySE", "em005_metall00_lp");
			_fxSkill1 = MonoBehaviourSingleton<FxManager>.Instance.PlayReturn<FxBase>("fxuse_Blanka_Metall_000", AimPosition, Quaternion.identity, Array.Empty<object>());
		}
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
			if (SectorSensor.Look(Controller.LogicPosition.vec3, base.direction, sensorAngle, _fAttackRange, targetMask, out hit))
			{
				if (logicFrameNow <= NextReloadFrame[1])
				{
					_velocity.x = 0;
				}
				else
				{
					OnStartShootSk001();
				}
			}
			else if (SectorSensor.Look(Controller.LogicPosition.vec3, -base.direction, sensorAngle, _fAttackRange, targetMask, out hit))
			{
				ReverseDirection();
				if (logicFrameNow <= NextReloadFrame[1])
				{
					_velocity.x = 0;
				}
				else
				{
					OnStartShootSk001();
				}
			}
			else
			{
				emState = EmState.RUN_LOOP;
			}
			break;
		case EmState.HIDE_START:
			if (logicFrameNow > logicToNext)
			{
				ShootSk001();
				emState = EmState.HIDE_LOOP;
				_animator.Play(HASH_HIDE_LOOP, 0, 0f);
				UpdateLogicToNext(logicHideLoop);
			}
			break;
		case EmState.HIDE_LOOP:
			if (logicFrameNow > logicToNext)
			{
				base.SoundSource.PlaySE("EnemySE", "em005_metall00_stop");
				emState = EmState.HIDE_END;
				_animator.Play(HASH_HIDE_END, 0, 0f);
				UpdateLogicToNext(logicHideEnd);
				_skill1Bullet.BackToPool();
				_fxSkill1.BackToPool();
			}
			break;
		case EmState.HIDE_END:
			if (logicFrameNow > logicToNext)
			{
				emState = EmState.IDLE;
				UpdateLogicToNext(logicHideEnd);
				NextReloadFrame[SkillIdx] = logicFrameNow + _nWalkTime / GameLogicUpdateManager.g_fixFrameLenFP.i;
			}
			break;
		case EmState.RUN_LOOP:
			if (_velocity.y != 0)
			{
				SkillIdx = 1;
				if (logicFrameNow > NextReloadFrame[SkillIdx])
				{
					OnStartShootSk001();
				}
				else if (SectorSensor.Look(Controller.LogicPosition.vec3, base.direction, sensorAngle, _fAttackRange, targetMask, out hit))
				{
					_velocity.x = 0;
					_animator.Play(HASH_IDLE_LOOP, 0);
				}
				else if (SectorSensor.Look(Controller.LogicPosition.vec3, -base.direction, sensorAngle, _fAttackRange, targetMask, out hit))
				{
					ReverseDirection();
					_velocity.x = 0;
					_animator.Play(HASH_IDLE_LOOP, 0);
				}
				else if (SectorSensor.Look(Controller.LogicPosition.vec3, base.direction, sensorAngle, sensorDistance, targetMask, out hit))
				{
					CheckDirection();
				}
				else if (SectorSensor.Look(Controller.LogicPosition.vec3, -base.direction, sensorAngle, sensorDistance, targetMask, out hit))
				{
					ReverseDirection();
					CheckDirection();
				}
				else
				{
					CheckDirection();
				}
			}
			break;
		case EmState.HURT_LOOP:
		case EmState.TRY_SKL001:
		case EmState.SKL_001:
		case EmState.DIE:
			break;
		}
	}

	protected override void DeadBehavior(ref HurtPassParam tHurtPassParam)
	{
		base.DeadBehavior(ref tHurtPassParam);
		if (_fxSkill1 != null)
		{
			_fxSkill1.BackToPool();
		}
		if (_skill1Bullet != null)
		{
			_skill1Bullet.BackToPool();
		}
	}
}
