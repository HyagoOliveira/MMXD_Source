using System;
using CallbackDefs;
using StageLib;
using UnityEngine;

public class EM056_Controller : EnemyControllerBase, IManagedUpdateBehavior
{
	private enum MainStatus
	{
		Init = 0,
		Action = 1
	}

	private MainStatus _mainStatus;

	[SerializeField]
	private int _flySpeed = 5950;

	[SerializeField]
	private int _flyAngle = 225;

	private Vector3 _flyDirection;

	private ParticleSystem _fxFire;

	private bool _bRotate;

	[SerializeField]
	private float _rotateSpeed = 10f;

	[SerializeField]
	private float _rotateZ;

	private bool _patrolIsLoop;

	private Vector3[] _patrolPaths = new Vector3[0];

	private int _patrolIndex;

	private bool _comeBack;

	private bool _endPatrol;

	private void OnEnable()
	{
		MonoBehaviourSingleton<UpdateManager>.Instance.AddUpdate(this);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		MonoBehaviourSingleton<UpdateManager>.Instance.RemoveUpdate(this);
	}

	protected override void Awake()
	{
		base.Awake();
		Transform[] target = _transform.GetComponentsInChildren<Transform>(true);
		ModelTransform = OrangeBattleUtility.FindChildRecursive(ref target, "model", true);
		_fxFire = OrangeBattleUtility.FindChildRecursive(ref target, "fireFxRoot", true).GetComponent<ParticleSystem>();
		_collideBullet = OrangeBattleUtility.FindChildRecursive(ref target, "StrikeColider").gameObject.AddOrGetComponent<CollideBullet>();
		base.AimPoint = new Vector3(0f, 0f, 0f);
		OrangeBattleUtility.AddEnemyAutoAimSystem(_transform, out _enemyAutoAimSystem);
		_enemyAutoAimSystem.UpdateAimRange(100f);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuseTarget");
		_mainStatus = MainStatus.Init;
		AiTimer.TimerStart();
		_patrolIndex = 0;
		_comeBack = false;
		_endPatrol = false;
		base.AllowAutoAim = false;
		UnityEngine.Object.Destroy(_collideBullet.gameObject.GetComponent<OrangeCriSource>());
		_collideBullet.SoundSource = base.SoundSource;
		base.SoundSource.MaxDistance = 16f;
		ExplodeSE = new string[2] { "EnemySE02", "em023_prominence_stop" };
	}

	public override void Unlock()
	{
		_unlockReady = true;
		base.AllowAutoAim = false;
		SetColliderEnable(true);
		if (InGame && (int)Hp > 0)
		{
			Activate = true;
		}
	}

	public override void UpdateStatus(int nSet, string smsg, Callback tCB = null)
	{
	}

	public override void LogicUpdate()
	{
		if (!Activate)
		{
			return;
		}
		VInt3 logicPosition = Controller.LogicPosition;
		base.LogicUpdate();
		switch (_mainStatus)
		{
		case MainStatus.Init:
			SetStatus(MainStatus.Action);
			break;
		case MainStatus.Action:
			switch (AiState)
			{
			case AI_STATE.mob_001:
				if ((_velocity.x > 0 && Controller.Collisions.right) || (_velocity.x < 0 && Controller.Collisions.left))
				{
					Hp = 0;
					Hurt(new HurtPassParam());
				}
				break;
			case AI_STATE.mob_002:
				if ((Mathf.Abs(logicPosition.x - Controller.LogicPosition.x) < 10 && (Controller.Collisions.right || Controller.Collisions.left)) || (Mathf.Abs(logicPosition.y - Controller.LogicPosition.y) < 10 && (Controller.Collisions.above || Controller.Collisions.below || Controller.Collisions.JSB_below)))
				{
					Hp = 0;
					Hurt(new HurtPassParam());
				}
				if (_patrolPaths.Length == 0)
				{
					_flyDirection = Quaternion.Euler(0f, 0f, _flyAngle) * Vector3.right;
					_velocity.x = (int)(_flyDirection.normalized.x * (float)_flySpeed);
					_velocity.y = (int)(_flyDirection.normalized.y * (float)_flySpeed);
				}
				else if (_patrolPaths.Length != 0 && !_endPatrol)
				{
					float num = Vector3.Distance(TargetPos.vec3, Controller.LogicPosition.vec3);
					if (num < 0.05f || num < distanceDelta * 2f)
					{
						GotoNextPatrolPoint();
						break;
					}
					Vector3 normalized = (TargetPos - Controller.LogicPosition).vec3.normalized;
					_velocity = new VInt3(normalized * _flySpeed * 0.001f);
				}
				break;
			}
			break;
		}
	}

	private void SetStatus(MainStatus mainStatus)
	{
		_mainStatus = mainStatus;
		switch (_mainStatus)
		{
		case MainStatus.Init:
			_velocity = VInt3.zero;
			_patrolIndex = 0;
			break;
		case MainStatus.Action:
			switch (AiState)
			{
			case AI_STATE.mob_001:
				_bRotate = true;
				_velocity.x = _flySpeed * base.direction;
				_rotateSpeed = (float)_flySpeed * 0.001f * 2f;
				break;
			case AI_STATE.mob_002:
				_bRotate = false;
				if (_patrolPaths.Length == 0)
				{
					_velocity.x = (int)(_flyDirection.normalized.x * (float)_flySpeed);
					_velocity.y = (int)(_flyDirection.normalized.y * (float)_flySpeed);
				}
				else if (_patrolPaths.Length == 1)
				{
					TargetPos = new VInt3(_patrolPaths[0]);
				}
				else
				{
					TargetPos = new VInt3(_patrolPaths[_patrolIndex]);
					Vector3 normalized = (TargetPos - Controller.LogicPosition).vec3.normalized;
					_velocity = new VInt3(normalized * _flySpeed * 0.001f);
				}
				break;
			}
			break;
		}
		AiTimer.TimerStart();
	}

	public void UpdateFunc()
	{
		if (Activate)
		{
			if (_bRotate)
			{
				_rotateZ = (_rotateZ + _rotateSpeed * (float)base.direction * (Time.deltaTime / GameLogicUpdateManager.m_fFrameLen)) % 360f;
				ModelTransform.localRotation = Quaternion.Euler(_rotateZ, 90f, 0f);
			}
			base.transform.localPosition = Vector3.MoveTowards(base.transform.localPosition, Controller.LogicPosition.vec3, distanceDelta);
		}
	}

	public override void SetActive(bool isActive)
	{
		base.SetActive(isActive);
		if (isActive)
		{
			_collideBullet.UpdateBulletData(EnemyWeapons[0].BulletData);
			_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			_collideBullet.Active(targetMask);
			_collideBullet.gameObject.layer = ManagedSingleton<OrangeLayerManager>.Instance.BulletLayer;
			_collideBullet.HitCallback = HitCB;
			_fxFire.Play();
			_velocity = VInt3.zero;
			_patrolIndex = 0;
			SetStatus(MainStatus.Init);
		}
		else
		{
			_fxFire.Stop();
			_flyAngle = 0;
			_flySpeed = 0;
			_velocity = VInt3.zero;
			_rotateZ = 0f;
			ModelTransform.localRotation = Quaternion.Euler(0f, 90f, 0f);
			_patrolIndex = 0;
			_patrolPaths = new Vector3[0];
			_collideBullet.BackToPool();
		}
	}

	public override void SetPositionAndRotation(Vector3 pos, bool bBack)
	{
		if (bBack)
		{
			base.direction = -1;
		}
		else
		{
			base.direction = 1;
		}
		ModelTransform.localScale = new Vector3(ModelTransform.localScale.x, ModelTransform.localScale.y, Mathf.Abs(ModelTransform.localScale.z) * (float)base.direction);
		base.transform.position = pos;
	}

	public override void UpdateEnemyID(int id)
	{
		base.UpdateEnemyID(id);
		if (null == _enemyAutoAimSystem)
		{
			OrangeBattleUtility.AddEnemyAutoAimSystem(_transform, out _enemyAutoAimSystem);
		}
		AI_STATE aiState = AI_STATE.mob_001;
		if (EnemyData.s_AI != "null")
		{
			aiState = (AI_STATE)Enum.Parse(typeof(AI_STATE), EnemyData.s_AI);
		}
		AiState = aiState;
		switch (AiState)
		{
		case AI_STATE.mob_001:
			IgnoreGravity = false;
			break;
		case AI_STATE.mob_002:
			IgnoreGravity = true;
			break;
		}
	}

	public override void SetStageCustomParams(int nStageCustomType, int[] nStageCustomParams)
	{
		base.SetStageCustomParams(nStageCustomType, nStageCustomParams);
		if (nStageCustomType == StageEnemy.GetStageCustomParamsType(EnemyData.s_MODEL))
		{
			_flyAngle = nStageCustomParams[0];
			_flySpeed = nStageCustomParams[1];
		}
	}

	private void HitCB(object obj)
	{
		if ((bool)_collideBullet.HitTarget.GetComponent<OrangeCharacter>())
		{
			Hp = 0;
			Hurt(new HurtPassParam());
		}
		else if (AiState == AI_STATE.mob_002)
		{
			Hp = 0;
			Hurt(new HurtPassParam());
		}
	}

	public override void SetPatrolPath(bool isLoop, int nMoveSpeed, Vector3[] paths)
	{
		base.SetPatrolPath(isLoop, nMoveSpeed, paths);
		_patrolIsLoop = isLoop;
		if (nMoveSpeed > 0)
		{
			_flySpeed = nMoveSpeed;
		}
		_patrolPaths = new Vector3[paths.Length];
		for (int i = 0; i < paths.Length; i++)
		{
			_patrolPaths[i] = paths[i];
		}
	}

	private void GotoNextPatrolPoint()
	{
		if (_patrolPaths.Length == 1)
		{
			_endPatrol = true;
		}
		else
		{
			if (_patrolPaths.Length <= 1)
			{
				return;
			}
			if (_patrolIsLoop)
			{
				_patrolIndex++;
				if (_patrolIndex >= _patrolPaths.Length)
				{
					_patrolIndex = 0;
				}
			}
			else
			{
				if ((!_comeBack && _patrolIndex + 1 >= _patrolPaths.Length) || (_comeBack && _patrolIndex == 0))
				{
					_comeBack = !_comeBack;
				}
				_patrolIndex += ((!_comeBack) ? 1 : (-1));
			}
			TargetPos = new VInt3(_patrolPaths[_patrolIndex]);
			Vector3 normalized = (TargetPos - Controller.LogicPosition).vec3.normalized;
			_velocity = new VInt3(normalized * _flySpeed * 0.001f);
		}
	}
}
