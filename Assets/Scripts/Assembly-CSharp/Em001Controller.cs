using System;
using UnityEngine;

public class Em001Controller : EnemyControllerBase, IManagedUpdateBehavior
{
	protected enum EmState
	{
		INIT = 0,
		IDLE = 1,
		HIDE_START = 2,
		HIDE_LOOP = 3,
		HIDE_END = 4,
		HURT_LOOP = 5,
		RUN_LOOP = 6,
		TRY_SKL001 = 7,
		SKL_001 = 8,
		DIE = 9
	}

	protected int HASH_IDLE_LOOP = Animator.StringToHash("EM001@idle_loop");

	protected int HASH_HIDE_START = Animator.StringToHash("EM001@hide_start");

	protected int HASH_HIDE_LOOP = Animator.StringToHash("EM001@hide_loop");

	protected int HASH_HIDE_END = Animator.StringToHash("EM001@hide_end");

	protected int HASH_HURT_LOOP = Animator.StringToHash("EM001@hurt_loop");

	protected int HASH_RUN_LOOP = Animator.StringToHash("EM001@run_loop");

	protected int HASH_SK001 = Animator.StringToHash("EM001@skl01");

	protected readonly int logicHideStart = (int)(0.333f / GameLogicUpdateManager.m_fFrameLen);

	protected readonly int logicHideEnd = (int)(0.367f / GameLogicUpdateManager.m_fFrameLen);

	protected readonly int logicSkl001 = (int)(0.8f / GameLogicUpdateManager.m_fFrameLen);

	protected EmState emState;

	protected VInt moveSpdX = new VInt(0.6f);

	protected int walkRayMask;

	protected Vector2 walkRayDirection = Vector2.down;

	protected RaycastHit2D hit;

	protected float walkRayDistance = 0.5f;

	[SerializeField]
	protected float sensorAngle = 90f;

	[SerializeField]
	protected int sensorDistance = 5;

	[SerializeField]
	protected Transform shootTransform;

	[SerializeField]
	protected int logicHideLoop = 30;

	[SerializeField]
	protected int logicTrySkl001 = 7;

	[SerializeField]
	protected int reduceDmgPercent = 50;

	protected int[] logicReloadFrame;

	protected int[] NextReloadFrame;

	protected int logicFrameNow;

	protected int logicToNext;

	protected Action actionAI;

	protected int SkillIdx;

	protected override void Awake()
	{
		base.Awake();
		base.AimPoint = new Vector3(0f, 0.5f, 0f);
		walkRayMask = (1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockLayer) | (1 << ManagedSingleton<OrangeLayerManager>.Instance.SemiBlockLayer);
	}

	protected override void SetStunStatus(bool enable)
	{
		IsStunStatus = true;
		if (enable)
		{
			emState = EmState.HURT_LOOP;
			_animator.Play(HASH_HURT_LOOP, 0, 0f);
		}
		else
		{
			emState = EmState.IDLE;
		}
	}

	public override void UpdateEnemyID(int id)
	{
		base.UpdateEnemyID(id);
		logicReloadFrame = new int[EquippedWeaponNum];
		NextReloadFrame = new int[EquippedWeaponNum];
		for (int i = 0; i < EquippedWeaponNum; i++)
		{
			logicReloadFrame[i] = EnemyWeapons[i].BulletData.n_RELOAD / GameLogicUpdateManager.g_fixFrameLenFP.i;
			NextReloadFrame[i] = 0;
		}
		AI_STATE aI_STATE = AI_STATE.mob_001;
		if (EnemyData.s_AI != "null")
		{
			aI_STATE = (AI_STATE)Enum.Parse(typeof(AI_STATE), EnemyData.s_AI);
		}
		switch (aI_STATE)
		{
		case AI_STATE.mob_001:
			actionAI = AI_mob_001;
			break;
		case AI_STATE.mob_002:
			actionAI = AI_mob_002;
			break;
		}
	}

	public override void SetActive(bool isActive)
	{
		base.SetActive(isActive);
		if (isActive)
		{
			emState = EmState.INIT;
			MonoBehaviourSingleton<UpdateManager>.Instance.AddUpdate(this);
			_animator.enabled = true;
			emState = EmState.IDLE;
		}
		else
		{
			MonoBehaviourSingleton<UpdateManager>.Instance.RemoveUpdate(this);
			_animator.Play(HASH_IDLE_LOOP, 0, 0f);
			_animator.enabled = false;
		}
	}

	public override void BackToPool()
	{
		base.BackToPool();
	}

	public override void LogicUpdate()
	{
		logicFrameNow = GameLogicUpdateManager.GameFrame;
		base.LogicUpdate();
		actionAI();
	}

	protected virtual void AI_mob_001()
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

	protected virtual void AI_mob_002()
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
					emState = EmState.HIDE_END;
					_animator.Play(HASH_HIDE_END, 0, 0f);
					UpdateLogicToNext(logicHideEnd);
				}
				else if (SectorSensor.Look(Controller.LogicPosition.vec3, -base.direction, sensorAngle, sensorDistance, targetMask, out hit))
				{
					ReverseDirection();
					emState = EmState.HIDE_END;
					_animator.Play(HASH_HIDE_END, 0, 0f);
					UpdateLogicToNext(logicHideEnd);
				}
			}
			break;
		case EmState.HIDE_END:
			if (logicFrameNow > logicToNext)
			{
				OnStartShootSk001();
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

	protected virtual void CheckDirection()
	{
		_velocity.x = moveSpdX * base.direction;
		_animator.Play(HASH_RUN_LOOP, 0);
		switch (base.direction)
		{
		case 1:
			if (Controller.Collisions.right)
			{
				ReverseDirection();
				return;
			}
			break;
		case -1:
			if (Controller.Collisions.left)
			{
				ReverseDirection();
				return;
			}
			break;
		}
		if (CheckMoveFall(_velocity))
		{
			ReverseDirection();
		}
	}

	protected virtual void ReverseDirection()
	{
		base.direction *= -1;
		_transform.Rotate(new Vector3Int(0, 180, 0));
		_velocity.x = moveSpdX * base.direction;
	}

	protected virtual void OnStartShootSk001()
	{
		_velocity.x = 0;
		emState = EmState.TRY_SKL001;
		UpdateLogicToNext(logicTrySkl001);
		_animator.Play(HASH_SK001, 0, 0f);
	}

	protected virtual void ShootSk001()
	{
		BulletBase.TryShotBullet(EnemyWeapons[SkillIdx].BulletData, shootTransform, Vector3.right * base.direction, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
		NextReloadFrame[SkillIdx] = logicFrameNow + logicReloadFrame[SkillIdx];
		UpdateLogicToNext(logicSkl001);
		emState = EmState.SKL_001;
	}

	public void UpdateFunc()
	{
		if (Activate)
		{
			_transform.localPosition = Vector3.MoveTowards(_transform.localPosition, Controller.LogicPosition.vec3, distanceDelta);
		}
	}

	protected void UpdateLogicToNext(int logicFrame)
	{
		logicToNext = logicFrameNow + logicFrame;
		if (emState == EmState.HIDE_LOOP)
		{
			selfBuffManager.sBuffStatus.fReduceDmgPercent = reduceDmgPercent;
		}
		else
		{
			selfBuffManager.sBuffStatus.fReduceDmgPercent = 0f;
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		MonoBehaviourSingleton<UpdateManager>.Instance.RemoveUpdate(this);
	}
}
