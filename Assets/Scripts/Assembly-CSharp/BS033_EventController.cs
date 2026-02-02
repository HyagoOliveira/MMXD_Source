#define RELEASE
using System;
using CallbackDefs;
using CodeStage.AntiCheat.ObscuredTypes;
using RootMotion.FinalIK;
using StageLib;
using UnityEngine;

public class BS033_EventController : EnemyControllerBase, IManagedUpdateBehavior
{
	protected enum MainStatus
	{
		IDLE = 0,
		EXPLODE = 1,
		MGUN = 2,
		MISSILE = 3,
		EGG = 4,
		IdleWaitNet = 5,
		MAX_STATUS = 6
	}

	protected enum Weapon
	{
		GUN = 0,
		MISSILE = 1,
		EGG = 2
	}

	protected AimIK _aimIk;

	private Transform _shootTransform;

	protected Vector3 _shootDirection;

	protected float _originalXPos;

	protected float _targetXPos;

	protected float _originalYPos;

	protected int[] _animatorHash;

	protected Transform _propellerTransform;

	protected Transform _gunTransform;

	protected Transform _gunShootPointTransform;

	protected Transform _leftMissileShootPointTransform;

	protected Transform _rightMissileShootPointTransform;

	protected Transform _assTransform;

	private readonly Vector3 _propellerSpeed = new Vector3(0f, 2700f, 0f);

	private Vector3 _floatVelocity;

	protected int _triggerFloorFlag = 10;

	protected MainStatus _mainStatus;

	protected AI_STATE _aiStage;

	protected OrangeTimer _summonTimer;

	[SerializeField]
	protected int _summonTime = 5000;

	protected bool PlayLoopSEOnce = true;

	private void OnEnable()
	{
		MonoBehaviourSingleton<UpdateManager>.Instance.AddUpdate(this);
	}

	private new void OnDisable()
	{
		MonoBehaviourSingleton<UpdateManager>.Instance.RemoveUpdate(this);
	}

	protected override void Awake()
	{
		base.Awake();
		_animator = GetComponentInChildren<Animator>();
		_animatorHash = new int[6];
		for (int i = 0; i < 6; i++)
		{
			_animatorHash[i] = Animator.StringToHash("BS033@idle_loop");
		}
		_animatorHash[0] = Animator.StringToHash("BS033@idle_loop");
		_animatorHash[2] = Animator.StringToHash("BS033@skill_01");
		_animatorHash[4] = Animator.StringToHash("BS033@skill_02");
		_animatorHash[3] = Animator.StringToHash("BS033@skill_03");
		_animatorHash[1] = Animator.StringToHash("BS033@dead");
		Transform[] target = _transform.GetComponentsInChildren<Transform>(true);
		_collideBullet = OrangeBattleUtility.FindChildRecursive(ref target, "CollideBullet", true).gameObject.AddOrGetComponent<CollideBullet>();
		ModelTransform = OrangeBattleUtility.FindChildRecursive(ref target, "model");
		_propellerTransform = OrangeBattleUtility.FindChildRecursive(ref target, "propeller");
		_gunTransform = OrangeBattleUtility.FindChildRecursive(ref target, "gun_TR");
		_gunShootPointTransform = OrangeBattleUtility.FindChildRecursive(ref target, "ShootPoint_Mouth");
		_assTransform = OrangeBattleUtility.FindChildRecursive(ref target, "siri5");
		_leftMissileShootPointTransform = OrangeBattleUtility.FindChildRecursive(ref target, "ShootPoint_L");
		_rightMissileShootPointTransform = OrangeBattleUtility.FindChildRecursive(ref target, "ShootPoint_R");
		_globalWaypoints = new float[2];
		_aimIk = GetComponentInChildren<AimIK>();
		OrangeBattleUtility.AddEnemyAutoAimSystem(_transform, out _enemyAutoAimSystem);
		_enemyAutoAimSystem.UpdateAimRange(100f);
		_summonTimer = OrangeTimerManager.GetTimer();
	}

	protected override void Start()
	{
		base.Start();
		_aimIk.solver.IKPositionWeight = 1f;
	}

	public override void SetActive(bool isActive)
	{
		if (isActive)
		{
			InGame = true;
			_collideBullet.UpdateBulletData(EnemyWeapons[0].BulletData);
			_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			_collideBullet.Active(targetMask);
			IgnoreGravity = true;
			_summonTimer.TimerStart();
			_characterMaterial.Appear(delegate
			{
				_originalXPos = _transform.position.x;
				_originalYPos = _transform.position.y;
				_targetXPos = _originalXPos;
				_globalWaypoints[0] = _originalYPos + 0.25f;
				_globalWaypoints[1] = _originalYPos - 0.25f;
				Activate = ManagedSingleton<StageHelper>.Instance.bEnemyActive && isActive;
				Hp = EnemyData.n_HP;
				_mainStatus = MainStatus.IDLE;
				Controller.enabled = true;
				_aimIk.enabled = true;
				SetColliderEnable(true);
				AiTimer.TimerStart();
				_transform.SetParent(null);
				Controller.LogicPosition = new VInt3(_transform.localPosition);
				_velocityExtra = VInt3.zero;
				MonoBehaviourSingleton<GameLogicUpdateManager>.Instance.AddUpdate(this);
				MonoBehaviourSingleton<FxManager>.Instance.UpdateFx(FxArray, true);
			});
		}
		else
		{
			Activate = false;
			_collideBullet.BackToPool();
			Controller.enabled = false;
			_aimIk.enabled = false;
			SetColliderEnable(false);
			AiTimer.TimerStop();
			base.SetActive(Activate);
			_characterMaterial.Disappear(delegate
			{
				InGame = false;
				MonoBehaviourSingleton<PoolManager>.Instance.BackToPool(this, EnemyData.s_MODEL);
			});
			base.SoundSource.PlaySE("BossSE", AudioManager.FormatEnum2Name(BossSE.CRI_BOSSSE_BS104_BEE01_STOP.ToString()));
		}
	}

	public override void UpdateEnemyID(int id)
	{
		base.UpdateEnemyID(id);
		AI_STATE aiStage = AI_STATE.mob_001;
		if (EnemyData.s_AI != "null")
		{
			aiStage = (AI_STATE)Enum.Parse(typeof(AI_STATE), EnemyData.s_AI);
		}
		_aiStage = aiStage;
	}

	public void UpdateFunc()
	{
		if (Activate)
		{
			if (_mainStatus != MainStatus.EXPLODE)
			{
				_propellerTransform.localEulerAngles += _propellerSpeed * Time.deltaTime;
			}
			base.transform.localPosition = Vector3.MoveTowards(base.transform.localPosition, Controller.LogicPosition.vec3, distanceDelta);
		}
	}

	public override void LogicUpdate()
	{
		if (!Activate || !_enemyAutoAimSystem)
		{
			return;
		}
		base.LogicUpdate();
		switch (_mainStatus)
		{
		case MainStatus.IDLE:
		{
			if (PlayLoopSEOnce)
			{
				base.SoundSource.PlaySE("BossSE", AudioManager.FormatEnum2Name(BossSE.CRI_BOSSSE_BS104_BEE01_LP.ToString()));
				PlayLoopSEOnce = false;
			}
			if (AiTimer.GetMillisecond() < EnemyData.n_AI_TIMER)
			{
				break;
			}
			ShuffleArray(ref BulletOrder);
			int[] bulletOrder = BulletOrder;
			foreach (int num2 in bulletOrder)
			{
				if (num2 == 0 || !IsWeaponAvailable(num2))
				{
					continue;
				}
				EnemyWeapons[num2].MagazineRemain = EnemyWeapons[num2].BulletData.n_MAGAZINE;
				if (StageUpdate.gbIsNetGame)
				{
					if (StageUpdate.bIsHost)
					{
						StageUpdate.RegisterSendAndRun(sNetSerialID, num2);
						_mainStatus = MainStatus.IdleWaitNet;
					}
					break;
				}
				switch (num2)
				{
				default:
					_mainStatus = MainStatus.MGUN;
					break;
				case 2:
					_mainStatus = MainStatus.MISSILE;
					break;
				case 3:
					_mainStatus = MainStatus.EGG;
					break;
				}
				break;
			}
			break;
		}
		case MainStatus.MGUN:
		{
			int num = 1;
			if (EnemyWeapons[num].MagazineRemain > 0f)
			{
				if (EnemyWeapons[num].LastUseTimer.IsStarted() && EnemyWeapons[num].LastUseTimer.GetMillisecond() <= EnemyWeapons[num].BulletData.n_FIRE_SPEED)
				{
					break;
				}
				if (_aimIk.solver.target == null)
				{
					Target = _enemyAutoAimSystem.GetClosetPlayer();
					if ((bool)Target)
					{
						_aimIk.solver.target = Target.AimTransform;
					}
				}
				_shootDirection = (_gunShootPointTransform.position - _gunTransform.position).normalized;
				BulletBase.TryShotBullet(EnemyWeapons[num].BulletData, _gunShootPointTransform, _shootDirection, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
				EnemyWeapons[num].LastUseTimer.TimerStart();
				EnemyWeapons[num].MagazineRemain -= EnemyWeapons[num].BulletData.n_USE_COST;
			}
			else
			{
				_mainStatus = MainStatus.IDLE;
				AiTimer.TimerStart();
			}
			break;
		}
		case MainStatus.MISSILE:
		{
			int num = 2;
			if (EnemyWeapons[num].MagazineRemain > 0f)
			{
				if (EnemyWeapons[num].LastUseTimer.IsStarted() && EnemyWeapons[num].LastUseTimer.GetMillisecond() <= EnemyWeapons[num].BulletData.n_FIRE_SPEED)
				{
					break;
				}
				Vector3 vector = Vector3.zero;
				Target = _enemyAutoAimSystem.GetClosetPlayer();
				if ((bool)Target)
				{
					vector = Target.AimTransform.position;
				}
				switch ((int)EnemyWeapons[num].MagazineRemain % 2)
				{
				case 0:
				{
					_shootDirection = (vector - _leftMissileShootPointTransform.position).normalized;
					if (Vector2.Angle(Vector3.right * base.direction, _shootDirection) >= 45f || vector == Vector3.zero)
					{
						_shootDirection = Vector3.right * base.direction;
					}
					BasicBullet basicBullet = (BasicBullet)BulletBase.TryShotBullet(EnemyWeapons[num].BulletData, _leftMissileShootPointTransform, _shootDirection, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
					break;
				}
				case 1:
				{
					_shootDirection = (vector - _rightMissileShootPointTransform.position).normalized;
					if (Vector2.Angle(Vector3.right * base.direction, _shootDirection) >= 45f || vector == Vector3.zero)
					{
						_shootDirection = Vector3.right * base.direction;
					}
					BasicBullet basicBullet2 = (BasicBullet)BulletBase.TryShotBullet(EnemyWeapons[num].BulletData, _rightMissileShootPointTransform, _shootDirection, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
					break;
				}
				}
				EnemyWeapons[num].LastUseTimer.TimerStart();
				EnemyWeapons[num].MagazineRemain -= 1f;
			}
			else
			{
				_mainStatus = MainStatus.IDLE;
				AiTimer.TimerStart();
			}
			break;
		}
		case MainStatus.EGG:
		{
			int num = 3;
			if (EnemyWeapons.Length > 3 && EnemyWeapons[num].MagazineRemain > 0f)
			{
				if (!EnemyWeapons[num].LastUseTimer.IsStarted() || EnemyWeapons[num].LastUseTimer.GetMillisecond() > EnemyWeapons[num].BulletData.n_FIRE_SPEED)
				{
					SpawnSubMob((int)EnemyWeapons[num].BulletData.f_EFFECT_X, _assTransform.position + Vector3.right * base.direction * 1.385f, base.direction);
					EnemyWeapons[num].LastUseTimer.TimerStart();
					EnemyWeapons[num].MagazineRemain -= 1f;
				}
			}
			else
			{
				_mainStatus = MainStatus.IDLE;
				AiTimer.TimerStart();
			}
			break;
		}
		case MainStatus.EXPLODE:
		{
			ExcludePlayer(0, -1);
			ExcludeEnemy(0, -1);
			IgnoreGravity = false;
			if (!Controller.Collisions.below)
			{
				break;
			}
			RaycastHit2D raycastHit2D = Controller.SolidMeeting(0f, -1f);
			if (!raycastHit2D)
			{
				break;
			}
			DestroyAllSpawnedMob();
			Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.CAMERA_SHAKE, 5f, false);
			_characterMaterial.UpdateTex(0);
			FallingFloor component = raycastHit2D.collider.GetComponent<FallingFloor>();
			if ((bool)component)
			{
				if (_triggerFloorFlag > 0)
				{
					_triggerFloorFlag--;
					component.TriggerFall();
				}
				else
				{
					Debug.Log("Found FallingFloor");
					base.gameObject.layer = ManagedSingleton<OrangeLayerManager>.Instance.ObstacleLayer;
					Activate = false;
					base.enabled = false;
				}
				if (_triggerFloorFlag == 7)
				{
					MonoBehaviourSingleton<AudioManager>.Instance.Play("BattleSE", 44);
				}
			}
			else
			{
				Debug.Log("NotFound FallingFloor");
				base.gameObject.layer = ManagedSingleton<OrangeLayerManager>.Instance.ObstacleLayer;
				Activate = false;
				base.enabled = false;
				MonoBehaviourSingleton<AudioManager>.Instance.Play("BattleSE", 44);
			}
			break;
		}
		default:
			throw new ArgumentOutOfRangeException();
		case MainStatus.IdleWaitNet:
			break;
		}
		if (_mainStatus != MainStatus.EXPLODE)
		{
			_velocity.y = Mathf.RoundToInt(CalculateVerticalMovement() * 1000f);
			UpdateMagazine();
			if (_aiStage == AI_STATE.mob_002 && _summonTimer.GetMillisecond() > _summonTime)
			{
				MonoBehaviourSingleton<OrangeBattleUtility>.Instance.CallSummonEnemyEvent(_transform);
				_summonTimer.TimerStart();
			}
		}
		if (_transform.position.x > _targetXPos)
		{
			_velocity.x = -3000;
		}
		else
		{
			_velocity.x = 0;
		}
		_animator.Play(_animatorHash[(int)_mainStatus]);
	}

	public override void UpdateStatus(int nSet, string sMsg, Callback tCB = null)
	{
		if ((int)Hp > 0)
		{
			switch (nSet)
			{
			default:
				_mainStatus = MainStatus.MGUN;
				break;
			case 2:
				_mainStatus = MainStatus.MISSILE;
				break;
			case 3:
				_mainStatus = MainStatus.EGG;
				break;
			}
		}
	}

	public override void OnTargetEnter(OrangeCharacter target)
	{
		base.OnTargetEnter(target);
		_aimIk.solver.target = Target.AimTransform;
	}

	public override void OnTargetExit(OrangeCharacter target)
	{
		base.OnTargetExit(target);
		_aimIk.solver.target = null;
	}

	public override ObscuredInt Hurt(HurtPassParam tHurtPassParam)
	{
		OrangeBattleUtility.UpdateEnemyHp(ref Hp, ref tHurtPassParam.dmg, base.UpdateHurtAction);
		if ((int)Hp > 0)
		{
			_targetXPos = _originalXPos + (float)(base.direction * 5) * (1f - (float)(int)Hp / (float)EnemyData.n_HP);
			_characterMaterial.Hurt();
		}
		else if (_mainStatus != MainStatus.EXPLODE)
		{
			_velocity.y = 0;
			EnemyCollider[] enemyCollider = _enemyCollider;
			for (int i = 0; i < enemyCollider.Length; i++)
			{
				enemyCollider[i].gameObject.layer = ManagedSingleton<OrangeLayerManager>.Instance.DefaultLayer;
			}
			_mainStatus = MainStatus.EXPLODE;
			_collideBullet.BackToPool();
			if (base.AimTransform != null)
			{
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("FX_BOSS_EXPLODE1", base.AimTransform, Quaternion.Euler(0f, 0f, 0f), Array.Empty<object>());
			}
			else
			{
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("FX_BOSS_EXPLODE1", new Vector3(base.transform.position.x, base.transform.position.y + 1f, base.transform.position.z), Quaternion.Euler(0f, 0f, 0f), Array.Empty<object>());
			}
			base.SoundSource.PlaySE("BossSE", AudioManager.FormatEnum2Name(BossSE.CRI_BOSSSE_BS104_BEE01_STOP.ToString()));
			PlayBossSE("HitSE", 103);
		}
		return Hp;
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
		_aimIk.solver.axis = Vector3.forward * base.direction;
		base.transform.position = pos;
	}
}
