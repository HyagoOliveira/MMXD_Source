using System;
using CallbackDefs;
using Newtonsoft.Json;
using StageLib;
using UnityEngine;

public class EM158_Controller : EnemyControllerBase, IManagedUpdateBehavior
{
	private enum MainStatus
	{
		Idle = 0,
		Move = 1,
		Shoot = 2
	}

	private enum SubStatus
	{
		Phase0 = 0,
		Phase1 = 1,
		Phase2 = 2,
		MAX_SUBSTATUS = 3
	}

	public enum AnimationID
	{
		ANI_IDLE = 0,
		ANI_MOVE = 1,
		ANI_SHOOT_START = 2,
		ANI_SHOOT_LOOP = 3,
		ANI_SHOOT_END = 4,
		ANI_HURT = 5,
		MAX_ANIMATION_ID = 6
	}

	private MainStatus _mainStatus;

	private SubStatus _subStatus;

	private AnimationID _currentAnimationId;

	private float _currentFrame;

	private int[] _animationHash;

	private bool isPatrol;

	private bool _patrolIsLoop;

	private float MoveDis;

	private Vector3[] _patrolPaths = new Vector3[0];

	private bool _comeBack;

	private int _patrolIndex = -1;

	private Vector3 StartPos;

	private Vector3 NextPos;

	private Vector3 MoveDirection;

	private float distance;

	[SerializeField]
	private int MoveSpeed = 3000;

	[Header("射擊")]
	[SerializeField]
	private Transform ShootPoint;

	[SerializeField]
	private float IdleWaitTime = 1f;

	private int IdleWaitFrame;

	[SerializeField]
	private float ShootDistance = 5f;

	[SerializeField]
	[Tooltip("登場時間")]
	private float ShowTime = 1f;

	private int ShowFrame;

	private Vector3 ShowPos;

	private bool HasShow;

	private bool CanMove;

	[SerializeField]
	private SkinnedMeshRenderer BodyMesh;

	private Vector3 NowPos
	{
		get
		{
			return _transform.position;
		}
	}

	private Vector3 ShootPos
	{
		get
		{
			return ShootPoint.position;
		}
	}

	private void OnEnable()
	{
		MonoBehaviourSingleton<UpdateManager>.Instance.AddUpdate(this);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		MonoBehaviourSingleton<UpdateManager>.Instance.RemoveUpdate(this);
	}

	private void HashAnimator()
	{
		_animationHash = new int[6];
		_animationHash[0] = Animator.StringToHash("EM158@idle_loop");
		_animationHash[1] = Animator.StringToHash("EM158@move_loop");
		_animationHash[2] = Animator.StringToHash("EM158@skill_start");
		_animationHash[3] = Animator.StringToHash("EM158@skill_loop");
		_animationHash[4] = Animator.StringToHash("EM158@skill_end");
		_animationHash[5] = Animator.StringToHash("EM158@hurt_loop");
	}

	private void LoadParts(ref Transform[] childs)
	{
		ModelTransform = OrangeBattleUtility.FindChildRecursive(ref childs, "model", true);
		_animator = ModelTransform.GetComponent<Animator>();
		_collideBullet = OrangeBattleUtility.FindChildRecursive(ref childs, "Collider", true).gameObject.AddOrGetComponent<CollideBullet>();
	}

	protected override void Start()
	{
		base.Start();
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_ch984_Gate_000", 4);
	}

	public override void OnToggleCharacterMaterial(bool appear)
	{
		CanMove = true;
		base.AllowAutoAim = true;
		SetColliderEnable(true);
		if ((bool)BodyMesh)
		{
			BodyMesh.enabled = appear;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		Transform[] childs = _transform.GetComponentsInChildren<Transform>(true);
		HashAnimator();
		LoadParts(ref childs);
		base.AimPoint = Vector3.up;
		OrangeBattleUtility.AddEnemyAutoAimSystem(_transform, out _enemyAutoAimSystem);
		_enemyAutoAimSystem.UpdateAimRange(8f);
		AiTimer.TimerStart();
	}

	public override void UpdateStatus(int nSet, string smsg, Callback tCB = null)
	{
		bWaitNetStatus = false;
		if ((int)Hp <= 0)
		{
			return;
		}
		if (smsg != null && smsg != "")
		{
			NetSyncData netSyncData = JsonConvert.DeserializeObject<NetSyncData>(smsg);
			Controller.LogicPosition.x = netSyncData.SelfPosX;
			Controller.LogicPosition.y = netSyncData.SelfPosY;
			Controller.LogicPosition.z = netSyncData.SelfPosZ;
			TargetPos.x = netSyncData.TargetPosX;
			TargetPos.y = netSyncData.TargetPosY;
			TargetPos.z = netSyncData.TargetPosZ;
			if (netSyncData.bSetHP)
			{
				Hp = netSyncData.nHP;
			}
		}
		SetStatus((MainStatus)nSet);
	}

	private void UpdateDirection(int forceDirection = 0, bool back = false)
	{
		if (forceDirection != 0)
		{
			base.direction = forceDirection;
		}
		else if (TargetPos.x > Controller.LogicPosition.x)
		{
			base.direction = 1;
		}
		else
		{
			base.direction = -1;
		}
		if (back)
		{
			base.direction = -base.direction;
		}
		ModelTransform.localScale = new Vector3(ModelTransform.localScale.x, ModelTransform.localScale.y, Mathf.Abs(ModelTransform.localScale.z) * (float)base.direction);
	}

	private void SetStatus(MainStatus mainStatus, SubStatus subStatus = SubStatus.Phase0)
	{
		_mainStatus = mainStatus;
		_subStatus = subStatus;
		switch (_mainStatus)
		{
		case MainStatus.Idle:
			_velocity = VInt3.zero;
			if (isPatrol && _patrolIndex < _patrolPaths.Length)
			{
				SetStatus(MainStatus.Move);
			}
			break;
		case MainStatus.Move:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
			{
				AI_STATE aiState = AiState;
				if (aiState == AI_STATE.mob_002)
				{
					break;
				}
				if (isPatrol)
				{
					if (_patrolIsLoop)
					{
						_patrolIndex = ++_patrolIndex % _patrolPaths.Length;
					}
					else
					{
						_patrolIndex++;
						if (_patrolIndex >= _patrolPaths.Length)
						{
							isPatrol = false;
							SetStatus(MainStatus.Idle);
							return;
						}
					}
					NextPos = _patrolPaths[_patrolIndex];
					StartPos = NowPos;
					MoveDis = Vector2.Distance(StartPos, NextPos);
					MoveDirection = (NextPos.xy() - StartPos.xy()).normalized;
					_velocity = new VInt3(MoveDirection * MoveSpeed * 0.001f);
					if (NextPos.x > StartPos.x)
					{
						UpdateDirection(1);
					}
					else
					{
						UpdateDirection(-1);
					}
				}
				else
				{
					SetStatus(MainStatus.Idle);
				}
				break;
			}
			case SubStatus.Phase2:
				TargetPos = new VInt3(NextPos);
				UpdateDirection();
				StartPos = NowPos;
				MoveDis = Vector2.Distance(StartPos, NextPos);
				MoveDirection = (NextPos.xy() - StartPos.xy()).normalized;
				_velocity = new VInt3(MoveDirection * MoveSpeed * 0.001f);
				break;
			}
			break;
		case MainStatus.Shoot:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_velocity = VInt3.zero;
				if (isPatrol && _patrolIndex >= 0)
				{
					_patrolIndex--;
				}
				break;
			case SubStatus.Phase1:
				if (!Target)
				{
					Target = _enemyAutoAimSystem.GetClosetPlayer();
				}
				if ((bool)Target)
				{
					TargetPos = Target.Controller.LogicPosition;
					UpdateDirection();
					BulletBase.TryShotBullet(EnemyWeapons[1].BulletData, ShootPos, (Target.Controller.GetRealCenterPos() - ShootPos).normalized, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
				}
				break;
			case SubStatus.Phase2:
				IdleWaitFrame = GameLogicUpdateManager.GameFrame + (int)(IdleWaitTime * 20f);
				break;
			}
			break;
		}
		AiTimer.TimerStart();
		UpdateAnimation();
	}

	private void UpdateAnimation()
	{
		switch (_mainStatus)
		{
		case MainStatus.Idle:
			_currentAnimationId = AnimationID.ANI_IDLE;
			break;
		case MainStatus.Move:
			switch (_subStatus)
			{
			default:
				return;
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_MOVE;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_MOVE;
				break;
			case SubStatus.Phase1:
				break;
			}
			break;
		case MainStatus.Shoot:
			switch (_subStatus)
			{
			default:
				return;
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_SHOOT_START;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_SHOOT_LOOP;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_SHOOT_END;
				break;
			}
			break;
		}
		_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
	}

	public override void LogicUpdate()
	{
		if (!Activate)
		{
			return;
		}
		if (!HasShow && GameLogicUpdateManager.GameFrame > ShowFrame)
		{
			if ((bool)_characterMaterial)
			{
				_characterMaterial.Appear(delegate
				{
					OnToggleCharacterMaterial(true);
				});
			}
			HasShow = true;
			base.AllowAutoAim = true;
			SetColliderEnable(true);
		}
		if (!CanMove)
		{
			return;
		}
		BaseLogicUpdate();
		_currentFrame = _animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
		switch (_mainStatus)
		{
		case MainStatus.Idle:
		{
			if (bWaitNetStatus)
			{
				break;
			}
			AI_STATE aiState = AiState;
			if (aiState == AI_STATE.mob_002)
			{
				Target = _enemyAutoAimSystem.GetClosetPlayer();
				if (!Target)
				{
					break;
				}
				if (Vector2.Distance(Target._transform.position, NowPos) < ShootDistance)
				{
					if (CheckHost())
					{
						UploadEnemyStatus(2);
					}
				}
				else
				{
					UploadEnemyStatus(1);
				}
				break;
			}
			Target = _enemyAutoAimSystem.GetClosetPlayer();
			if ((bool)Target)
			{
				if (Vector2.Distance(Target._transform.position, NowPos) < ShootDistance && CheckHost())
				{
					UploadEnemyStatus(2);
				}
			}
			else if (isPatrol && _patrolIndex < _patrolPaths.Length)
			{
				UploadEnemyStatus(1);
			}
			break;
		}
		case MainStatus.Move:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
			{
				if (!Target)
				{
					Target = _enemyAutoAimSystem.GetClosetPlayer();
				}
				if ((bool)Target && Vector2.Distance(Target._transform.position, NowPos) < ShootDistance)
				{
					SetStatus(MainStatus.Shoot);
					break;
				}
				AI_STATE aiState = AiState;
				if (aiState == AI_STATE.mob_002)
				{
					if (!Target)
					{
						Target = _enemyAutoAimSystem.GetClosetPlayer();
					}
					if ((bool)Target)
					{
						TargetPos = Target.Controller.LogicPosition;
						UpdateDirection();
						NextPos = Target._transform.position;
						MoveDirection = (NextPos.xy() - NowPos.xy()).normalized;
						_velocity = new VInt3(MoveDirection * MoveSpeed * 0.001f);
					}
				}
				else if (Vector2.Distance(_transform.position, StartPos) > MoveDis)
				{
					SetStatus(MainStatus.Move);
				}
				break;
			}
			case SubStatus.Phase2:
				if (Vector2.Distance(_transform.position, StartPos) > MoveDis)
				{
					SetStatus(MainStatus.Idle);
				}
				break;
			case SubStatus.Phase1:
				break;
			}
			break;
		case MainStatus.Shoot:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (!Target)
				{
					Target = _enemyAutoAimSystem.GetClosetPlayer();
				}
				if ((bool)Target)
				{
					TargetPos = Target.Controller.LogicPosition;
					UpdateDirection();
				}
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Shoot, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Shoot, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				if (IdleWaitFrame < GameLogicUpdateManager.GameFrame && _currentFrame > 1f)
				{
					SetStatus(MainStatus.Idle);
				}
				break;
			}
			break;
		}
	}

	public void UpdateFunc()
	{
		if (Activate && CanMove)
		{
			Vector3 localPosition = _transform.localPosition;
			_transform.localPosition = Vector3.MoveTowards(_transform.localPosition, Controller.LogicPosition.vec3, distanceDelta);
		}
	}

	public override void SetActive(bool isActive)
	{
		AI_STATE aiState = AiState;
		if (aiState == AI_STATE.mob_003)
		{
			Mob_003SetActive(isActive);
			return;
		}
		base.SetActive(isActive);
		IgnoreGravity = true;
		if (isActive)
		{
			_collideBullet.UpdateBulletData(EnemyWeapons[0].BulletData);
			_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			_collideBullet.Active(targetMask);
			SetStatus(MainStatus.Idle);
		}
		else
		{
			_collideBullet.BackToPool();
		}
	}

	private void Mob_003SetActive(bool isActive)
	{
		if (IsStun)
		{
			SetStun(false);
		}
		try
		{
			if (StageUpdate.gStageName == "stage04_1401_e1")
			{
				MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_mob_explode_000", 5);
			}
		}
		catch (Exception)
		{
		}
		InGame = isActive;
		Controller.enabled = isActive;
		SetColliderEnable(isActive);
		MonoBehaviourSingleton<FxManager>.Instance.UpdateFx(FxArray, isActive);
		if (isActive)
		{
			AiTimer.TimerStart();
			_transform.SetParent(null);
			Controller.LogicPosition = new VInt3(_transform.localPosition);
			_velocityExtra = VInt3.zero;
			MonoBehaviourSingleton<GameLogicUpdateManager>.Instance.AddUpdate(this);
			MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_ch984_Gate_000", NowPos + Vector3.up * 0.5f, Quaternion.identity, new object[1] { Vector3.one });
			ShowFrame = GameLogicUpdateManager.GameFrame + (int)(ShowTime * 20f);
			CanMove = false;
			HasShow = false;
			base.AllowAutoAim = false;
			SetColliderEnable(false);
			if ((bool)BodyMesh)
			{
				BodyMesh.enabled = false;
			}
		}
		else
		{
			Hp = 0;
			UpdateHurtAction();
			AiTimer.TimerStop();
			MonoBehaviourSingleton<GameLogicUpdateManager>.Instance.RemoveUpdate(this);
			selfBuffManager.StopLoopSE();
			base.SoundSource.StopAll();
			if ((bool)_characterMaterial)
			{
				_characterMaterial.Disappear(delegate
				{
					OnToggleCharacterMaterial(true);
					if (!InGame)
					{
						MonoBehaviourSingleton<PoolManager>.Instance.BackToPool(this, EnemyData.s_MODEL);
					}
				});
			}
			else
			{
				OnToggleCharacterMaterial(false);
				MonoBehaviourSingleton<PoolManager>.Instance.BackToPool(this, EnemyData.s_MODEL);
			}
			if (KeepSpawnedMob)
			{
				SpawnedMobList.Clear();
			}
			else
			{
				DestroyAllSpawnedMob();
			}
		}
		Activate = ManagedSingleton<StageHelper>.Instance.bEnemyActive && isActive;
		if (!isActive)
		{
			bNeedDead = false;
		}
		_animator.enabled = isActive;
		if (isActive)
		{
			IgnoreGravity = true;
			_collideBullet.UpdateBulletData(EnemyWeapons[0].BulletData);
			_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			_collideBullet.Active(targetMask);
			SetStatus(MainStatus.Move, SubStatus.Phase2);
		}
		else
		{
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
		ModelTransform.localScale = new Vector3(_transform.localScale.x, _transform.localScale.y, _transform.localScale.z * (float)base.direction);
		_transform.position = pos;
	}

	public override void SetPatrolPath(bool isLoop, int nMoveSpeed, Vector3[] paths)
	{
		base.SetPatrolPath(isLoop, nMoveSpeed, paths);
		_patrolIsLoop = isLoop;
		if (nMoveSpeed > 0)
		{
			MoveSpeed = nMoveSpeed;
		}
		_patrolPaths = new Vector3[paths.Length];
		for (int i = 0; i < paths.Length; i++)
		{
			_patrolPaths[i] = paths[i];
		}
		if (_patrolPaths.Length != 0)
		{
			isPatrol = true;
		}
		if (isPatrol)
		{
			_patrolIndex = -1;
		}
	}

	public override void UpdateEnemyID(int id)
	{
		base.UpdateEnemyID(id);
		UpdateAIState();
		AiState = AI_STATE.mob_003;
		AI_STATE aiState = AiState;
		if (aiState == AI_STATE.mob_002)
		{
			isPatrol = false;
		}
	}

	public void SetNextPos(Vector3 nextpos)
	{
		NextPos = nextpos;
	}

	public void SetDie()
	{
		Hp = 0;
		Hurt(new HurtPassParam());
	}
}
