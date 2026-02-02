#define RELEASE
using System;
using System.Collections.Generic;
using CallbackDefs;
using StageLib;
using UnityEngine;
using enums;

public class EnemyHumanController : EnemyControllerBase, IManagedUpdateBehavior, IManagedLateUpdateBehavior
{
	protected enum MainStatus
	{
		Idle = 0,
		Debut = 1,
		Walk = 2,
		Dash = 3,
		AirDash = 4,
		Crouch = 5,
		Jump = 6,
		Fall = 7,
		Dead = 8,
		Hurt = 9,
		Summon = 10,
		SpecialMotion = 11,
		Approaching = 12,
		Skill0 = 13,
		Skill1 = 14,
		Skill2 = 15,
		Skill3 = 16,
		Skill4 = 17,
		Skill5 = 18,
		Skill6 = 19,
		Skill7 = 20,
		EventIdle = 1000,
		EventWalk = 1001,
		EventFall = 1002,
		SwitchStatus = 9999
	}

	protected enum SubStatus
	{
		Phase0 = 0,
		Phase1 = 1,
		Phase2 = 2,
		Phase3 = 3,
		Phase4 = 4,
		Phase5 = 5,
		Phase6 = 6,
		Phase7 = 7,
		Phase8 = 8,
		Phase9 = 9,
		MAX_SUBSTATUS = 10
	}

	[HideInInspector]
	public string LandFx;

	public float WalkDistance = 5f;

	public readonly int hashDirection = Animator.StringToHash("fDirection");

	public readonly int hashEquip = Animator.StringToHash("fEquip");

	public readonly int hashVelocityX = Animator.StringToHash("fVelocityX");

	public readonly int hashVelocityY = Animator.StringToHash("fVelocityY");

	public readonly int hashSpeedMultiplier = Animator.StringToHash("fSpeedMultiplier");

	protected int WeaponCurrent;

	protected SkinnedMeshRenderer[] _handMesh;

	protected float _currentFrame;

	protected MainStatus _mainStatus;

	protected SubStatus _subStatus;

	protected HumanBase.AnimateId _currentAnimationId;

	protected static int[][] _animationHash;

	protected int _nSummonEventId = 999;

	protected bool _bDoSummon;

	private OrangeTimer summonTimer;

	public EnemyHumanPoolObject CurrentEnemyHumanModel;

	public EnemyHumanPoolObject[] CurrentEnemyHumanWeapon;

	public WeaponStruct[] PlayerWeapons;

	public bool BuildDone;

	public int AvatarID;

	protected bool _isDash;

	protected readonly Quaternion _quaternionNormal = Quaternion.Euler(0f, 0f, 0f);

	protected readonly Quaternion _quaternionReverse = Quaternion.Euler(0f, 180f, 0f);

	public static int WalkSpeed;

	public static int JumpSpeed;

	public static int DashSpeed;

	public static readonly int StepSpeed = Mathf.RoundToInt(1000f * OrangeBattleUtility.PPU * OrangeBattleUtility.FPS);

	protected bool FristAttackWait = true;

	private float PreShootAngle;

	protected OrangeTimer AIHarbingerTimer = new OrangeTimer();

	private float MoveDis;

	private Vector3 StartPos;

	private bool isPatrol;

	private bool _patrolIsLoop;

	private Vector3[] _patrolPaths = new Vector3[0];

	private int _patrolIndex;

	private float MaxPoint = float.NegativeInfinity;

	private float MinPoint = float.PositiveInfinity;

	private readonly Queue<MainStatus> _netCommandQueue = new Queue<MainStatus>();

	public static readonly int BusterDelay = 32;

	protected sbyte _isShoot;

	protected short _isShootPrev;

	protected bool _freshCreateBullet;

	protected OrangeTimer _shootTimer;

	protected List<BulletDetails> bulletList = new List<BulletDetails>();

	public Vector3 _shootDirection;

	public float AimDeadZone = 2.25f;

	protected int AutoReloadDelay;

	protected float AutoReloadPercent;

	private Vector3 NextPos
	{
		get
		{
			return _patrolPaths[_patrolIndex % _patrolPaths.Length];
		}
	}

	protected override void Awake()
	{
		EnemyID = 0;
		base.Awake();
		AwakeJob();
	}

	protected virtual void AwakeJob()
	{
		_shootTimer = OrangeTimerManager.GetTimer();
		WalkSpeed = Mathf.RoundToInt(OrangeBattleUtility.PlayerWalkSpeed * OrangeBattleUtility.PPU * OrangeBattleUtility.FPS * 1000f);
		JumpSpeed = Mathf.RoundToInt(OrangeBattleUtility.PlayerJumpSpeed * OrangeBattleUtility.PPU * OrangeBattleUtility.FPS * 1000f);
		DashSpeed = Mathf.RoundToInt(OrangeBattleUtility.PlayerDashSpeed * OrangeBattleUtility.PPU * OrangeBattleUtility.FPS * 1000f);
		OrangeBattleUtility.AddEnemyAutoAimSystem(_transform, out _enemyAutoAimSystem);
		_enemyAutoAimSystem.UpdateAimRange(20f);
		AutoReloadDelay = OrangeConst.AUTO_RELOAD;
		AutoReloadPercent = (float)OrangeConst.AUTO_RELOAD_PERCENT * 0.01f;
		summonTimer = OrangeTimerManager.GetTimer();
		UpdateAnimationHash();
	}

	protected static void UpdateAnimationHash()
	{
		if (_animationHash == null)
		{
			_animationHash = new int[145][];
			for (int i = 0; i < 145; i++)
			{
				_animationHash[i] = new int[2];
			}
			_animationHash[1][0] = Animator.StringToHash("ride_loop");
			_animationHash[0][0] = Animator.StringToHash("stand_loop");
			_animationHash[0][1] = Animator.StringToHash("stand_atk");
			_animationHash[8][0] = Animator.StringToHash("jump_start");
			_animationHash[8][1] = Animator.StringToHash("jump_atk_start");
			_animationHash[9][0] = Animator.StringToHash("fall_loop");
			_animationHash[9][1] = Animator.StringToHash("fall_atk");
			_animationHash[10][0] = Animator.StringToHash("landing");
			_animationHash[10][1] = Animator.StringToHash("landing_atk");
			_animationHash[22][0] = Animator.StringToHash("run_start");
			_animationHash[22][1] = Animator.StringToHash("run_atk_start");
			_animationHash[2][0] = Animator.StringToHash("run_loop");
			_animationHash[2][1] = Animator.StringToHash("run_atk_loop");
			_animationHash[3][0] = Animator.StringToHash("backward_atk");
			_animationHash[3][1] = Animator.StringToHash("backward_atk");
			_animationHash[4][0] = Animator.StringToHash("dash_start");
			_animationHash[4][1] = Animator.StringToHash("dash_atk_loop");
			_animationHash[5][0] = Animator.StringToHash("dash_end");
			_animationHash[5][1] = Animator.StringToHash("dash_atk_end");
			_animationHash[11][0] = Animator.StringToHash("wallgrab_step");
			_animationHash[12][0] = Animator.StringToHash("wallgrab_start");
			_animationHash[13][0] = Animator.StringToHash("wallgrab_loop");
			_animationHash[16][0] = Animator.StringToHash("walljump_start");
			_animationHash[17][0] = Animator.StringToHash("fall_loop");
			_animationHash[18][0] = Animator.StringToHash("crouch_start");
			_animationHash[18][1] = Animator.StringToHash("crouch_atk_start");
			_animationHash[19][0] = Animator.StringToHash("crouch_loop");
			_animationHash[19][1] = Animator.StringToHash("crouch_atk");
			_animationHash[20][0] = Animator.StringToHash("crouch_end");
			_animationHash[20][1] = Animator.StringToHash("crouch_atk_end");
			_animationHash[23][0] = Animator.StringToHash("damage_start");
			_animationHash[24][0] = Animator.StringToHash("damage_loop");
			_animationHash[21][0] = Animator.StringToHash("airdash_end");
			_animationHash[26][0] = Animator.StringToHash("melee_stand_classic_atk1_start");
			_animationHash[27][0] = Animator.StringToHash("melee_stand_classic_atk2_start");
			_animationHash[28][0] = Animator.StringToHash("melee_stand_classic_atk3_start");
			_animationHash[38][0] = Animator.StringToHash("melee_stand_classic_atk1_end");
			_animationHash[39][0] = Animator.StringToHash("melee_stand_classic_atk2_end");
			_animationHash[40][0] = Animator.StringToHash("melee_stand_classic_atk3_end");
			_animationHash[29][0] = Animator.StringToHash("melee_stand_atk1_start");
			_animationHash[30][0] = Animator.StringToHash("melee_stand_atk2_start");
			_animationHash[31][0] = Animator.StringToHash("melee_stand_atk3_start");
			_animationHash[32][0] = Animator.StringToHash("melee_stand_atk4_start");
			_animationHash[33][0] = Animator.StringToHash("melee_stand_atk5_start");
			_animationHash[41][0] = Animator.StringToHash("melee_stand_atk1_end");
			_animationHash[42][0] = Animator.StringToHash("melee_stand_atk2_end");
			_animationHash[43][0] = Animator.StringToHash("melee_stand_atk3_end");
			_animationHash[44][0] = Animator.StringToHash("melee_stand_atk4_end");
			_animationHash[45][0] = Animator.StringToHash("melee_stand_atk5_end");
			_animationHash[46][0] = Animator.StringToHash("melee_jump_atk_loop");
			_animationHash[47][0] = Animator.StringToHash("melee_dash_atk_loop");
			_animationHash[49][0] = Animator.StringToHash("melee_dash_atk_end");
			_animationHash[51][0] = Animator.StringToHash("melee_crouch_atk_start");
			_animationHash[52][0] = Animator.StringToHash("melee_crouch_atk_end");
			_animationHash[14][0] = Animator.StringToHash("wallgrab_slash");
			_animationHash[15][0] = Animator.StringToHash("wallgrab_slash_end");
			_animationHash[53][0] = Animator.StringToHash("login");
			_animationHash[54][0] = Animator.StringToHash("win");
			_animationHash[55][0] = Animator.StringToHash("logout");
			_animationHash[62][0] = Animator.StringToHash("enemy_summon0");
			_animationHash[63][0] = Animator.StringToHash("enemy_summon1");
			_animationHash[64][0] = Animator.StringToHash("enemy_summon2");
		}
	}

	private void OnEnable()
	{
		MonoBehaviourSingleton<UpdateManager>.Instance.AddUpdate(this);
		MonoBehaviourSingleton<UpdateManager>.Instance.AddLateUpdate(this);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		MonoBehaviourSingleton<UpdateManager>.Instance.RemoveUpdate(this);
		MonoBehaviourSingleton<UpdateManager>.Instance.RemoveLateUpdate(this);
	}

	public override void LogicUpdate()
	{
		if (!Activate || !BuildDone || PlayerWeapons == null || PlayerWeapons[0].BulletData == null)
		{
			return;
		}
		for (int i = 0; i < PlayerWeapons.Length; i++)
		{
			PlayerWeapons[i].ChargeTimer += GameLogicUpdateManager.m_fFrameLenMS;
			PlayerWeapons[i].LastUseTimer += GameLogicUpdateManager.m_fFrameLenMS;
		}
		base.LogicUpdate();
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
		MainStatus mainStatus = _mainStatus;
		if ((uint)(mainStatus - 1000) > 1u)
		{
			UpdateAimDirection();
		}
		_currentFrame = _animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
		switch (_mainStatus)
		{
		case MainStatus.Idle:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				Target = _enemyAutoAimSystem.GetClosetPlayer();
				if (!Target)
				{
					break;
				}
				if (CanSummonModel() && summonTimer.GetMillisecond() > 10000 && !_bDoSummon)
				{
					_bDoSummon = true;
				}
				if ((AiState == AI_STATE.mob_001 || AiState == AI_STATE.mob_003 || AiState == AI_STATE.mob_004) && Mathf.Abs(Target.GetTargetPoint().x - _transform.position.x) > WalkDistance && !CheckMoveFall(_velocity + VInt3.signRight * base.direction * WalkSpeed))
				{
					SetStatus(MainStatus.Walk);
				}
				else if (_bDoSummon)
				{
					SetStatus(MainStatus.Summon);
				}
				else if (PlayerWeapons[WeaponCurrent].LastUseTimer.GetMillisecond() > PlayerWeapons[WeaponCurrent].BulletData.n_FIRE_SPEED && PlayerWeapons[WeaponCurrent].MagazineRemain > 0f)
				{
					if (FristAttackWait && PlayerWeapons[WeaponCurrent].WeaponData.n_TYPE != 8)
					{
						_isShoot = 1;
						if (PlayerWeapons[WeaponCurrent].GatlingSpinner != null)
						{
							PlayerWeapons[WeaponCurrent].GatlingSpinner.Activate = true;
						}
						if (!AIHarbingerTimer.IsStarted())
						{
							AIHarbingerTimer.TimerStart();
						}
						if (AIHarbingerTimer.GetMillisecond() > 500)
						{
							AIHarbingerTimer.TimerStop();
							FristAttackWait = false;
						}
					}
					else
					{
						PlayerShootBuster(ref PlayerWeapons[WeaponCurrent], 0);
					}
				}
				else if (PlayerWeapons[WeaponCurrent].MagazineRemain == 0f && _isShoot == 0)
				{
					SetStatus(MainStatus.Crouch);
				}
				UpdateDirection();
				break;
			case SubStatus.Phase1:
				if (_currentFrame > 1f)
				{
					AI_STATE aiState = AiState;
					if (aiState == AI_STATE.mob_004 && isPatrol)
					{
						SetStatus(MainStatus.Walk);
					}
					else
					{
						SetStatus(MainStatus.Idle);
					}
				}
				break;
			case SubStatus.Phase3:
				Target = null;
				UpdateAimDirection();
				PlayerShootBuster(ref PlayerWeapons[WeaponCurrent], 0);
				SetStatus(_mainStatus);
				break;
			case SubStatus.Phase2:
				break;
			}
			break;
		case MainStatus.Walk:
			if (CheckMoveFall(_velocity))
			{
				SetStatus(MainStatus.Idle);
				break;
			}
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (_currentFrame > 1f)
				{
					SetStatus(_mainStatus, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if (PlayerWeapons[WeaponCurrent].WeaponData.n_TYPE == 2 || PlayerWeapons[WeaponCurrent].WeaponData.n_TYPE == 4)
				{
					if (Target == null || Mathf.Abs(Target.GetTargetPoint().x - _transform.position.x) < WalkDistance - 3f)
					{
						SetStatus(MainStatus.Idle);
					}
				}
				else if (Target == null || Mathf.Abs(Target.GetTargetPoint().x - _transform.position.x) < WalkDistance - 1f)
				{
					SetStatus(MainStatus.Idle);
				}
				break;
			case SubStatus.Phase2:
				if (Mathf.Abs(StartPos.x - _transform.position.x) >= MoveDis || Controller.Collisions.left || Controller.Collisions.right)
				{
					_patrolIndex++;
					SetStatus(_mainStatus);
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
					FristAttackWait = true;
				}
				break;
			}
			break;
		case MainStatus.Summon:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (_currentFrame > 1f)
				{
					_bDoSummon = false;
					MonoBehaviourSingleton<OrangeBattleUtility>.Instance.CallSummonEnemyEvent(_transform, _nSummonEventId);
					SetStatus(MainStatus.Summon, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if (AiTimer.GetMillisecond() > 1000)
				{
					summonTimer.TimerStart();
					SetStatus(MainStatus.Idle);
				}
				break;
			}
			break;
		case MainStatus.EventWalk:
			if (_subStatus == SubStatus.Phase0 && _currentFrame > 1f)
			{
				SetStatus(_mainStatus, SubStatus.Phase1);
			}
			break;
		}
	}

	protected override void UpdateGravity()
	{
		bool flag = true;
		if (_velocity.y < 0)
		{
			if (!Controller.Collisions.below)
			{
				switch (_mainStatus)
				{
				case MainStatus.Jump:
					if (Controller.CollisionsOld.below)
					{
						_velocity.x = 0;
					}
					SetStatus(MainStatus.Fall);
					break;
				case MainStatus.Idle:
				case MainStatus.Walk:
				case MainStatus.Dash:
					if (!Controller.BelowInBypassRange)
					{
						SetStatus(MainStatus.Fall);
						if (_isDash)
						{
							_isDash = false;
						}
						_velocity.x = 0;
					}
					break;
				case MainStatus.EventIdle:
					if (!Controller.BelowInBypassRange)
					{
						SetStatus(MainStatus.EventFall);
						if (_isDash)
						{
							_isDash = false;
						}
						_velocity.x = 0;
					}
					break;
				}
			}
			else
			{
				switch (_mainStatus)
				{
				case MainStatus.Jump:
				case MainStatus.Fall:
					if (_velocity.y < 0 && !Controller.JumpThrough)
					{
						_isDash = false;
						_velocity.x = 0;
						MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(LandFx, _transform.position, _quaternionNormal, Array.Empty<object>());
						SetStatus(MainStatus.Idle, SubStatus.Phase1);
					}
					break;
				case MainStatus.EventFall:
					if (_velocity.y < 0 && !Controller.JumpThrough)
					{
						_isDash = false;
						_velocity.x = 0;
						MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(LandFx, _transform.position, _quaternionNormal, Array.Empty<object>());
						SetStatus(MainStatus.EventIdle, SubStatus.Phase1);
					}
					break;
				case MainStatus.Debut:
					if (_subStatus == SubStatus.Phase0)
					{
						SetStatus(MainStatus.Debut, SubStatus.Phase1);
					}
					break;
				default:
					if (!Controller.CollisionsOld.below)
					{
						Debug.Log("特例著地 - " + _mainStatus);
						_isDash = false;
						_velocity.x = 0;
						MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(LandFx, _transform.position, _quaternionNormal, Array.Empty<object>());
						SetStatus(MainStatus.Idle, SubStatus.Phase1);
					}
					break;
				}
				_velocity.y = 0;
			}
		}
		else if (Controller.Collisions.above)
		{
			_velocity.y = 0;
		}
		int i = OrangeBattleUtility.FP_MaxGravity.i;
		switch (_mainStatus)
		{
		case MainStatus.AirDash:
			flag = false;
			break;
		case MainStatus.Hurt:
			flag = Controller.BelowInBypassRange;
			break;
		}
		if (!flag || IgnoreGravity)
		{
			_velocity.y = 0;
			return;
		}
		_velocity.y += OrangeBattleUtility.FP_Gravity * GameLogicUpdateManager.g_fixFrameLenFP / 1000;
		_velocity.y = IntMath.Sign(_velocity.y) * IntMath.Min(IntMath.Abs(_velocity.y), IntMath.Abs(i));
	}

	public virtual void UpdateFunc()
	{
		if (Activate)
		{
			base.transform.localPosition = Vector3.MoveTowards(base.transform.localPosition, Controller.LogicPosition.vec3, distanceDelta);
		}
	}

	public override void UpdateStatus(int nSet, string smsg, Callback tCB = null)
	{
		if (_mainStatus != (MainStatus)nSet && nSet != 9999)
		{
			if (!BuildDone)
			{
				_netCommandQueue.Enqueue((MainStatus)nSet);
			}
			else
			{
				SetStatus((MainStatus)nSet);
			}
		}
	}

	public void LateUpdateFunc()
	{
		if (!Activate)
		{
			return;
		}
		if (_isShootPrev != _isShoot || _freshCreateBullet)
		{
			_animator.Play(_animationHash[(uint)_currentAnimationId][_isShoot], 0, 0f);
		}
		_freshCreateBullet = false;
		_isShootPrev = _isShoot;
		if (_shootTimer.IsStarted() && _shootTimer.GetTicks() > BusterDelay)
		{
			if (PlayerWeapons[WeaponCurrent].GatlingSpinner != null)
			{
				PlayerWeapons[WeaponCurrent].GatlingSpinner.Activate = false;
			}
			_isShoot = 0;
			_shootTimer.TimerStop();
		}
	}

	public override void SetActive(bool isActive)
	{
		InGame = isActive;
		if (BuildDone)
		{
			SetActiveReal(InGame);
		}
	}

	public virtual void SetActiveReal(bool isActive)
	{
		if (IsStun)
		{
			SetStun(false);
		}
		Controller.enabled = isActive;
		SetColliderEnable(isActive);
		if (isActive)
		{
			_transform.SetParent(null);
			Controller.LogicPosition = new VInt3(_transform.localPosition);
			_velocityExtra = VInt3.zero;
			MonoBehaviourSingleton<GameLogicUpdateManager>.Instance.AddUpdate(this);
			MonoBehaviourSingleton<FxManager>.Instance.UpdateFx(FxArray, true);
			EnableWeaponMesh(ref PlayerWeapons[WeaponCurrent]);
			_animator.SetFloat(hashSpeedMultiplier, 1f);
			SetStatus(MainStatus.Idle);
			if ((bool)_characterMaterial)
			{
				_characterMaterial.Appear();
			}
			if (CanSummonModel())
			{
				summonTimer.TimerStart();
				summonTimer.SetMillisecondsOffset(10000L);
			}
		}
		else
		{
			BuildDone = false;
			UpdateHurtAction();
			AiTimer.TimerStop();
			summonTimer.TimerStop();
			MonoBehaviourSingleton<GameLogicUpdateManager>.Instance.RemoveUpdate(this);
			MonoBehaviourSingleton<FxManager>.Instance.UpdateFx(FxArray, false);
			selfBuffManager.StopLoopSE();
			DisableWeaponMesh(ref PlayerWeapons[WeaponCurrent]);
			if ((bool)_characterMaterial)
			{
				_characterMaterial.Disappear(delegate
				{
					Transform[] target = base.transform.GetComponentsInChildren<Transform>(true);
					Transform transform = OrangeBattleUtility.FindChildRecursive(ref target, "AimIcon2");
					if ((bool)transform)
					{
						transform.SetParent(null);
					}
					if ((bool)CurrentEnemyHumanModel)
					{
						CurrentEnemyHumanModel.transform.SetParentNull();
						CurrentEnemyHumanModel.BackToPool();
						for (int i = 0; i < CurrentEnemyHumanWeapon.Length; i++)
						{
							if (CurrentEnemyHumanWeapon[i] != null)
							{
								CurrentEnemyHumanWeapon[i].transform.SetParentNull();
								CurrentEnemyHumanWeapon[i].BackToPool();
							}
						}
						ModelTransform = null;
					}
					MonoBehaviourSingleton<PoolManager>.Instance.BackToPool(this, EnemyData.s_MODEL);
				});
			}
		}
		Activate = ManagedSingleton<StageHelper>.Instance.bEnemyActive && isActive;
		if (!isActive)
		{
			bNeedDead = false;
			_patrolPaths = new Vector3[0];
			_patrolIndex = 0;
			isPatrol = false;
		}
	}

	protected virtual void InitializeWeaponStruct(ref WeaponStruct[] pWeaponStructs)
	{
		Transform[][] array = new Transform[pWeaponStructs.Length][];
		for (int i = 0; i < pWeaponStructs.Length; i++)
		{
			if (pWeaponStructs[i] == null)
			{
				continue;
			}
			pWeaponStructs[i].ShootTransform = new Transform[10];
			pWeaponStructs[i].WeaponMesh = new CharacterMaterial[10];
			if (pWeaponStructs[i].WeaponData.n_SKILL == 0)
			{
				pWeaponStructs[i].BulletData = new SKILL_TABLE();
			}
			else
			{
				pWeaponStructs[i].BulletData = ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT[pWeaponStructs[i].WeaponData.n_SKILL];
			}
			array[i] = OrangeBattleUtility.FindAllChildRecursive(base.transform, "NormalWeapon" + i);
			WeaponType weaponType;
			if (pWeaponStructs[i].WeaponData != null)
			{
				weaponType = (WeaponType)pWeaponStructs[i].WeaponData.n_TYPE;
				if (weaponType != WeaponType.Melee)
				{
					if (weaponType == WeaponType.Gatling && array[i].Length != 0)
					{
						Transform transform = OrangeBattleUtility.FindChildRecursive(array[i][0], "_sub");
						if ((bool)transform)
						{
							pWeaponStructs[i].GatlingSpinner = transform.gameObject.AddComponent<GatlingSpinner>();
						}
					}
					pWeaponStructs[i].ShootTransform = OrangeBattleUtility.FindMultiChildRecursive(array[i], "ShootPoint");
				}
				else
				{
					MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_flameblade_000_effect_f", 5);
					MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_flameblade_000_blade_f", 5);
					pWeaponStructs[i].WeaponTrial = array[i][0].GetComponentInChildren<MeleeWeaponTrail>();
					pWeaponStructs[i].SlashObject = OrangeBattleUtility.FindChildRecursive(base.transform, "SlashEfx");
					if ((bool)pWeaponStructs[i].SlashObject)
					{
						pWeaponStructs[i].SlashEfxCmp = pWeaponStructs[i].SlashObject.transform.GetComponent<SlashEfx>();
						pWeaponStructs[i].SlashEfxCmp.InitSlashData(pWeaponStructs[i].WeaponData.s_MODEL, ManagedSingleton<OrangeDataManager>.Instance.CHARACTER_TABLE_DICT[AvatarID].s_ANIMATOR.Substring(0, 4).Equals("male"), base.transform, Vector3.one);
					}
					GameObject gameObject = new GameObject();
					pWeaponStructs[i].MeleeBullet = gameObject.AddComponent<MeleeBullet>();
					pWeaponStructs[i].MeleeBullet.transform.SetParent(base.transform);
					gameObject.transform.localPosition = Vector3.zero;
				}
				pWeaponStructs[i].ShootTransform2 = OrangeBattleUtility.FindAllChildRecursive(_transform, "Bip");
			}
			else
			{
				string s_USE_MOTION = pWeaponStructs[i].BulletData.s_USE_MOTION;
				if (!(s_USE_MOTION == "SPECIAL_000"))
				{
					if (s_USE_MOTION == "MARINO_DARTS")
					{
						pWeaponStructs[i].ShootTransform[0] = OrangeBattleUtility.FindChildRecursive(_transform, "L WeaponPoint");
					}
				}
				else
				{
					pWeaponStructs[i].ShootTransform[0] = _transform;
					MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_fallen_000_f", 5);
				}
			}
			int n_CHARGE_MAX_LEVEL = pWeaponStructs[i].BulletData.n_CHARGE_MAX_LEVEL;
			pWeaponStructs[i].ChargeTime = new int[n_CHARGE_MAX_LEVEL + 1];
			int num = n_CHARGE_MAX_LEVEL + 1;
			weaponType = (WeaponType)pWeaponStructs[i].WeaponData.n_TYPE;
			if (weaponType == WeaponType.Melee && num < 20)
			{
				num = 20;
			}
			SKILL_TABLE[] array2 = new SKILL_TABLE[num];
			array2[0] = pWeaponStructs[i].BulletData;
			for (int j = 1; j < num; j++)
			{
				array2[j] = ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT[array2[0].n_ID + j];
			}
			pWeaponStructs[i].FastBulletDatas = array2;
			for (int k = 0; k <= n_CHARGE_MAX_LEVEL; k++)
			{
				pWeaponStructs[i].ChargeTime[k] = pWeaponStructs[i].FastBulletDatas[k].n_CHARGE;
			}
			if (array[i] != null)
			{
				for (int l = 0; l < array[i].Length; l++)
				{
					if (array[i][l] != null)
					{
						pWeaponStructs[i].WeaponMesh[l] = array[i][l].GetComponent<CharacterMaterial>();
					}
				}
			}
			pWeaponStructs[i].ForceLock = false;
			pWeaponStructs[i].ChargeTimer = new UpdateTimer();
			pWeaponStructs[i].LastUseTimer = new UpdateTimer();
			pWeaponStructs[i].LastUseTimer.TimerStart();
			pWeaponStructs[i].LastUseTimer += (float)(pWeaponStructs[i].BulletData.n_FIRE_SPEED / 2);
			pWeaponStructs[i].MagazineRemain = pWeaponStructs[i].BulletData.n_MAGAZINE;
		}
	}

	public override void UpdateEnemyID(int id)
	{
		base.UpdateEnemyID(id);
		EnemyHumanBuilder enemyHumanBuilder = base.gameObject.AddComponent<EnemyHumanBuilder>();
		enemyHumanBuilder.EnemyID = EnemyID;
		enemyHumanBuilder.CreateHumanEnemy(delegate
		{
			ModelTransform = OrangeBattleUtility.FindChildRecursive(_transform, "model", true);
			base.AimTransform = OrangeBattleUtility.FindChildRecursive(_transform, "Bip", true);
			Transform transform = OrangeBattleUtility.FindChildRecursive(_transform, "InfoBar", true);
			string s_ANIMATOR = ManagedSingleton<OrangeDataManager>.Instance.CHARACTER_TABLE_DICT[AvatarID].s_ANIMATOR;
			if (s_ANIMATOR.Contains("large"))
			{
				transform.localPosition = Vector3.up * 2f;
			}
			else if (s_ANIMATOR.Contains("medium"))
			{
				transform.localPosition = Vector3.up * 1.8f;
			}
			else if (s_ANIMATOR.Contains("small"))
			{
				transform.localPosition = Vector3.up * 1.6f;
			}
			_characterMaterial = GetComponentInChildren<CharacterMaterial>();
			_animator = ModelTransform.GetComponentInChildren<Animator>();
			Animators = base.gameObject.GetComponentsInChildren<Animator>();
			if ((bool)_characterMaterial)
			{
				_characterMaterial.DefaultDissolveValue = 1;
			}
			Transform[] array = OrangeBattleUtility.FindAllChildRecursive(ModelTransform, "HandMesh_L_c");
			Transform[] array2 = OrangeBattleUtility.FindAllChildRecursive(ModelTransform, "HandMesh_L_m");
			_handMesh = new SkinnedMeshRenderer[2];
			for (int i = 0; i < array.Length; i++)
			{
				_handMesh[i] = array[i].GetComponent<SkinnedMeshRenderer>();
			}
			int num = array.Length;
			for (int j = num; j < array2.Length + num; j++)
			{
				_handMesh[j] = array2[j - num].GetComponent<SkinnedMeshRenderer>();
			}
			InitializeWeaponStruct(ref PlayerWeapons);
			if (EnemyData.s_AI != "null")
			{
				AiState = (AI_STATE)Enum.Parse(typeof(AI_STATE), EnemyData.s_AI);
			}
			switch (AiState)
			{
			case AI_STATE.mob_001:
				if (PlayerWeapons[WeaponCurrent].WeaponData.n_TYPE == 2 || PlayerWeapons[WeaponCurrent].WeaponData.n_TYPE == 4)
				{
					WalkDistance = PlayerWeapons[0].BulletData.f_DISTANCE * 1.5f;
				}
				else if (PlayerWeapons[WeaponCurrent].WeaponData.n_TYPE == 8)
				{
					WalkDistance = 3f;
				}
				else
				{
					WalkDistance = 5f;
				}
				isPatrol = false;
				break;
			case AI_STATE.mob_002:
				isPatrol = false;
				break;
			case AI_STATE.mob_003:
				WalkDistance = PlayerWeapons[0].BulletData.f_DISTANCE * 0.8f;
				isPatrol = false;
				break;
			case AI_STATE.mob_004:
				if (PlayerWeapons[WeaponCurrent].WeaponData.n_TYPE == 2 || PlayerWeapons[WeaponCurrent].WeaponData.n_TYPE == 4)
				{
					WalkDistance = PlayerWeapons[0].BulletData.f_DISTANCE * 1.5f;
				}
				else if (PlayerWeapons[WeaponCurrent].WeaponData.n_TYPE == 8)
				{
					WalkDistance = 3f;
				}
				else
				{
					WalkDistance = 5f;
				}
				if (_patrolPaths.Length != 0)
				{
					isPatrol = true;
				}
				break;
			}
			FristAttackWait = true;
			BuildDone = true;
			SetActiveReal(InGame);
			while (_netCommandQueue.Count != 0)
			{
				SetStatus(_netCommandQueue.Dequeue());
			}
		});
	}

	protected virtual void UpdateDirection(int forceDirection = 0)
	{
		if (forceDirection != 0)
		{
			base.direction = forceDirection;
		}
		else if (StageUpdate.gbIsNetGame)
		{
			if (Target.Controller.LogicPosition.x > Controller.LogicPosition.x)
			{
				base.direction = 1;
			}
			else
			{
				base.direction = -1;
			}
		}
		else if (Target != null && Target.transform.position.x > _transform.position.x)
		{
			base.direction = 1;
		}
		else
		{
			base.direction = -1;
		}
		ModelTransform.localScale = new Vector3(ModelTransform.localScale.x, ModelTransform.localScale.y, Mathf.Abs(ModelTransform.localScale.z) * (float)base.direction);
	}

	protected override void SetStunStatus(bool enable)
	{
		IsStunStatus = true;
		if (enable)
		{
			SetStatus(MainStatus.Hurt);
		}
		else
		{
			SetStatus(MainStatus.Fall);
		}
	}

	protected void SetStatus(MainStatus mainStatus, SubStatus subStatus = SubStatus.Phase0)
	{
		SetStatus((int)mainStatus, (int)subStatus);
	}

	protected virtual void SetStatus(int mainStatus, int subStatus = 0)
	{
		_mainStatus = (MainStatus)mainStatus;
		_subStatus = (SubStatus)subStatus;
		switch (_mainStatus)
		{
		case MainStatus.Idle:
		case MainStatus.EventIdle:
		{
			_velocity.x = 0;
			if (_subStatus != 0)
			{
				break;
			}
			AI_STATE aiState = AiState;
			if (aiState == AI_STATE.mob_004)
			{
				if (!Controller.Collisions.below)
				{
					SetStatus(MainStatus.Fall);
					return;
				}
				if (isPatrol)
				{
					SetStatus(MainStatus.Walk);
					return;
				}
			}
			break;
		}
		case MainStatus.Hurt:
			_velocity = VInt3.zero;
			break;
		case MainStatus.Walk:
		case MainStatus.EventWalk:
		{
			AI_STATE aiState = AiState;
			if (aiState == AI_STATE.mob_004 && isPatrol)
			{
				_subStatus = SubStatus.Phase2;
			}
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_velocity.x = base.direction * StepSpeed;
				break;
			case SubStatus.Phase1:
				_velocity.x = base.direction * WalkSpeed;
				break;
			case SubStatus.Phase2:
				if (_patrolIndex >= _patrolPaths.Length)
				{
					_patrolIndex %= _patrolPaths.Length;
					isPatrol = false;
					SetStatus(MainStatus.Idle, SubStatus.Phase3);
					return;
				}
				StartPos = _transform.position;
				UpdateDirection((NextPos.x >= StartPos.x) ? 1 : (-1));
				_velocity.x = base.direction * WalkSpeed;
				MoveDis = Mathf.Abs(StartPos.x - NextPos.x);
				break;
			}
			break;
		}
		}
		AiTimer.TimerStart();
		UpdateAnimation();
		UpdateCollider();
	}

	protected virtual void UpdateAnimation()
	{
		switch (_mainStatus)
		{
		case MainStatus.Idle:
		case MainStatus.EventIdle:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_currentAnimationId = HumanBase.AnimateId.ANI_STAND;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = HumanBase.AnimateId.ANI_LAND;
				break;
			}
			break;
		case MainStatus.Debut:
			_currentAnimationId = HumanBase.AnimateId.ANI_TELEPORT_IN_POSE;
			break;
		case MainStatus.Jump:
			_currentAnimationId = HumanBase.AnimateId.ANI_JUMP;
			break;
		case MainStatus.Fall:
		case MainStatus.EventFall:
			_currentAnimationId = HumanBase.AnimateId.ANI_FALL;
			break;
		case MainStatus.Dead:
			_currentAnimationId = HumanBase.AnimateId.ANI_HURT_BEGIN;
			break;
		case MainStatus.Hurt:
			if (IsStun)
			{
				_currentAnimationId = HumanBase.AnimateId.ANI_HURT_LOOP;
				_isShoot = 0;
			}
			else
			{
				_currentAnimationId = HumanBase.AnimateId.ANI_HURT_BEGIN;
			}
			break;
		case MainStatus.Walk:
		case MainStatus.EventWalk:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_currentAnimationId = HumanBase.AnimateId.ANI_STEP;
				break;
			case SubStatus.Phase1:
			case SubStatus.Phase2:
				_currentAnimationId = HumanBase.AnimateId.ANI_WALK;
				break;
			}
			break;
		case MainStatus.Crouch:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_currentAnimationId = HumanBase.AnimateId.ANI_CROUCH;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = HumanBase.AnimateId.ANI_CROUCH_END;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = HumanBase.AnimateId.ANI_CROUCH_UP;
				break;
			}
			break;
		case MainStatus.Dash:
		case MainStatus.AirDash:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_currentAnimationId = HumanBase.AnimateId.ANI_DASH;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = HumanBase.AnimateId.ANI_DASH_END;
				break;
			}
			break;
		case MainStatus.Summon:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_currentAnimationId = HumanBase.AnimateId.ANI_SUMMON0;
				_animator.Play(_animationHash[(uint)_currentAnimationId][0], 0, 0f);
				return;
			case SubStatus.Phase1:
				return;
			}
			break;
		}
		_animator.Play(_animationHash[(uint)_currentAnimationId][_isShoot], 0, 0f);
	}

	protected virtual void UpdateCollider()
	{
	}

	private void ToggleHandMesh(bool meshEnabled)
	{
		SkinnedMeshRenderer[] handMesh = _handMesh;
		foreach (SkinnedMeshRenderer skinnedMeshRenderer in handMesh)
		{
			if ((bool)skinnedMeshRenderer)
			{
				skinnedMeshRenderer.enabled = meshEnabled;
			}
		}
	}

	protected void ToggleWeaponMesh(ref WeaponStruct weapon, bool meshEnabled)
	{
		SetAnimatorEquip(weapon.WeaponData.n_TYPE);
		WeaponType weaponType = (WeaponType)weapon.WeaponData.n_TYPE;
		if ((uint)(weaponType - 1) <= 1u || weaponType == WeaponType.SprayHeavy)
		{
			ToggleHandMesh(!meshEnabled);
		}
		else
		{
			ToggleHandMesh(true);
		}
		if (meshEnabled)
		{
			weapon.WeaponMesh[0].Appear();
			if ((bool)weapon.WeaponMesh[1])
			{
				weapon.WeaponMesh[1].Appear();
			}
		}
		else
		{
			weapon.WeaponMesh[0].Disappear();
			if ((bool)weapon.WeaponMesh[1])
			{
				weapon.WeaponMesh[1].Disappear();
			}
		}
	}

	protected void EnableWeaponMesh(ref WeaponStruct weapon)
	{
		ToggleWeaponMesh(ref weapon, true);
	}

	protected void DisableWeaponMesh(ref WeaponStruct weapon)
	{
		ToggleWeaponMesh(ref weapon, false);
	}

	protected void UpdateWeaponMesh(ref WeaponStruct enableWeapon, ref WeaponStruct disableWeapon)
	{
		DisableWeaponMesh(ref disableWeapon);
		EnableWeaponMesh(ref enableWeapon);
	}

	public void SetAnimatorEquip(int equipType)
	{
		switch ((WeaponType)(short)equipType)
		{
		case WeaponType.Dummy:
			equipType = 0;
			break;
		case WeaponType.Buster:
			equipType = 1;
			break;
		case WeaponType.Spray:
			equipType = 2;
			break;
		case WeaponType.SprayHeavy:
			equipType = 3;
			break;
		case WeaponType.Melee:
			equipType = 4;
			break;
		case WeaponType.DualGun:
			equipType = 5;
			break;
		case WeaponType.MGun:
			equipType = 6;
			break;
		case WeaponType.Gatling:
			equipType = 7;
			break;
		case WeaponType.Launcher:
			equipType = 8;
			break;
		default:
			throw new ArgumentOutOfRangeException("equipType", equipType, null);
		}
		if (_animator != null)
		{
			_animator.SetFloat(hashEquip, equipType);
		}
	}

	protected void PlayerShootBuster(ref WeaponStruct weaponStruct, sbyte lv = 0)
	{
		MainStatus mainStatus = _mainStatus;
		if (mainStatus != MainStatus.Hurt)
		{
			EnableWeaponMesh(ref weaponStruct);
			SKILL_TABLE sKILL_TABLE = weaponStruct.FastBulletDatas[lv];
			weaponStruct.MagazineRemain -= sKILL_TABLE.n_USE_COST;
			weaponStruct.LastUseTimer.TimerStart();
			CreateBullet(weaponStruct, lv);
			if ((short)weaponStruct.WeaponData.n_TYPE == 16)
			{
				CreateBullet(weaponStruct, lv, 1);
			}
		}
	}

	protected void CreateBullet(WeaponStruct weaponStruct, sbyte lv, int shootPoint = 0)
	{
		_freshCreateBullet = true;
		_isShoot = (sbyte)(lv + 1);
		_shootTimer.TimerStart();
		PushBulletDetail(weaponStruct.FastBulletDatas[lv], weaponStruct.weaponStatus, (weaponStruct.ShootTransform.Length < shootPoint + 1) ? weaponStruct.ShootTransform[0] : weaponStruct.ShootTransform[shootPoint]);
	}

	protected void PushBulletDetail(SKILL_TABLE bulletData, WeaponStatus refWS, Transform ShootTransform)
	{
		BulletDetails item = new BulletDetails
		{
			bulletData = bulletData,
			refWS = refWS,
			ShootTransform = ShootTransform
		};
		bulletList.Add(item);
	}

	protected void CreateBulletDetail(SKILL_TABLE bulletData, WeaponStatus refWS, Vector3 ShootPosition, int nReordID, int nBulletRecordID)
	{
		BulletBase bulletBase = null;
		switch ((BulletType)(short)bulletData.n_TYPE)
		{
		case BulletType.Continuous:
			bulletBase = _poolManager.GetPoolObj<ContinuousBullet>(bulletData.s_MODEL);
			break;
		case BulletType.Spray:
			bulletBase = _poolManager.GetPoolObj<SprayBullet>(bulletData.s_MODEL);
			bulletBase.isForceSE = true;
			break;
		case BulletType.Collide:
			bulletBase = _poolManager.GetPoolObj<CollideBullet>(bulletData.s_MODEL);
			break;
		case BulletType.LrColliderBulle:
			bulletBase = _poolManager.GetPoolObj<LrColliderBullet>(bulletData.s_MODEL);
			break;
		default:
			bulletBase = _poolManager.GetPoolObj<BasicBullet>(bulletData.s_MODEL);
			break;
		}
		if ((bool)bulletBase)
		{
			bulletBase.UpdateBulletData(bulletData);
			bulletBase.SetBulletAtk(refWS, selfBuffManager.sBuffStatus);
			int n_SHOTLINE = bulletData.n_SHOTLINE;
			if (n_SHOTLINE == 10)
			{
				bulletBase.Active(ShootPosition, Target.Controller.GetCenterPos(), targetMask);
			}
			else
			{
				bulletBase.Active(ShootPosition, _shootDirection, targetMask);
			}
		}
	}

	protected void CreateBulletDetail(SKILL_TABLE bulletData, WeaponStatus refWS, Transform ShootTransform, int nReordID, int nBulletRecordID)
	{
		bool forcePlaySE = (short)bulletData.n_TYPE == 4;
		int n_SHOTLINE = bulletData.n_SHOTLINE;
		if (n_SHOTLINE != 0 && n_SHOTLINE == 10)
		{
			Vector3 pDirection = Vector3.right * base.direction * 5f;
			if ((bool)Target)
			{
				pDirection = Target.Controller.GetCenterPos();
			}
			BulletBase.TryShotBullet(bulletData, ShootTransform.position, pDirection, null, selfBuffManager.sBuffStatus, EnemyData, targetMask, forcePlaySE);
		}
		else
		{
			BulletBase.TryShotBullet(bulletData, ShootTransform, _shootDirection, null, selfBuffManager.sBuffStatus, EnemyData, targetMask, forcePlaySE);
		}
	}

	protected void UpdateAimDirection()
	{
		if (Target != null)
		{
			Transform transform = PlayerWeapons[WeaponCurrent].ShootTransform[0];
			Transform transform2 = PlayerWeapons[WeaponCurrent].ShootTransform2[0];
			if (transform != null)
			{
				if (Vector2.Distance(Target.GetTargetPoint(), transform.position) > AimDeadZone)
				{
					_shootDirection = (Target.GetTargetPoint().xy() - transform.position.xy()).normalized;
				}
				else
				{
					_shootDirection = (Target.GetTargetPoint().xy() - transform2.position.xy()).normalized;
				}
			}
		}
		else
		{
			_shootDirection = Vector2.right * base.direction;
		}
		float num = Mathf.Abs(Vector2.SignedAngle(Vector2.up, _shootDirection)) / 180f;
		float value = Mathf.Lerp(PreShootAngle, num, 0.05f);
		_animator.SetFloat(hashDirection, value);
		PreShootAngle = num;
	}

	protected void UpdateMagazine(ref WeaponStruct[] pWeaponStructs)
	{
		for (int i = 0; i < pWeaponStructs.Length; i++)
		{
			if (pWeaponStructs[i] == null)
			{
				continue;
			}
			switch (pWeaponStructs[i].BulletData.n_MAGAZINE_TYPE)
			{
			case 0:
				if (pWeaponStructs[i].MagazineRemain > 0f)
				{
					if ((float)pWeaponStructs[i].LastUseTimer.GetMillisecond() >= (float)AutoReloadDelay + (float)pWeaponStructs[i].BulletData.n_RELOAD * AutoReloadPercent)
					{
						pWeaponStructs[i].MagazineRemain = pWeaponStructs[i].BulletData.n_MAGAZINE;
					}
				}
				else if (pWeaponStructs[i].LastUseTimer.GetMillisecond() >= pWeaponStructs[i].BulletData.n_RELOAD)
				{
					pWeaponStructs[i].MagazineRemain = pWeaponStructs[i].BulletData.n_MAGAZINE;
				}
				break;
			case 1:
				if (pWeaponStructs[i].MagazineRemain < 0f)
				{
					pWeaponStructs[i].ForceLock = true;
				}
				if (pWeaponStructs[i].ForceLock)
				{
					if (pWeaponStructs[i].LastUseTimer.GetMillisecond() >= pWeaponStructs[i].BulletData.n_RELOAD)
					{
						pWeaponStructs[i].ForceLock = false;
						pWeaponStructs[i].MagazineRemain = pWeaponStructs[i].BulletData.n_MAGAZINE;
					}
					break;
				}
				if (pWeaponStructs[i].MagazineRemain < (float)pWeaponStructs[i].BulletData.n_MAGAZINE)
				{
					pWeaponStructs[i].MagazineRemain += GameLogicUpdateManager.m_fFrameLen * 10f;
				}
				if (pWeaponStructs[i].MagazineRemain > (float)pWeaponStructs[i].BulletData.n_MAGAZINE)
				{
					pWeaponStructs[i].MagazineRemain = pWeaponStructs[i].BulletData.n_MAGAZINE;
				}
				break;
			}
		}
	}

	public override void SetPositionAndRotation(Vector3 pos, bool bBack)
	{
		base.direction = ((!bBack) ? 1 : (-1));
		_transform.position = pos;
		Controller.LogicPosition = new VInt3(_transform.position);
	}

	public override void ResetStatus()
	{
		Transform[] target = base.transform.GetComponentsInChildren<Transform>(true);
		Transform transform = OrangeBattleUtility.FindChildRecursive(ref target, "model", true);
		Transform transform2 = OrangeBattleUtility.FindChildRecursive(ref target, "AimIcon2");
		if ((bool)transform2)
		{
			transform2.SetParent(null);
		}
		if ((bool)transform)
		{
			UnityEngine.Object.Destroy(transform.gameObject);
		}
	}

	public override void SetSummonEventID(int nSummonEventId)
	{
		if (nSummonEventId != 0)
		{
			_nSummonEventId = nSummonEventId;
		}
	}

	public bool CanSummonModel()
	{
		if (ManagedSingleton<OrangeDataManager>.Instance.CHARACTER_TABLE_DICT[AvatarID].s_MODEL == "ch999_001")
		{
			AI_STATE aiState = AiState;
			if (aiState == AI_STATE.mob_004)
			{
				return false;
			}
			return true;
		}
		return false;
	}

	public virtual string[] GetHumanDependAnimations()
	{
		return new string[0];
	}

	public virtual string[] GetHumanDependBlendAnimations()
	{
		return null;
	}

	public virtual string[][] GetHumanDependAnimationsBlendTree()
	{
		return null;
	}

	public virtual void GetUniqueMotion(out string[] source, out string[] target)
	{
		source = new string[1] { "null" };
		target = new string[1] { "null" };
	}

	public virtual int GetUniqueWeaponType()
	{
		return 0;
	}

	public override void SetPatrolPath(bool isLoop, int nMoveSpeed, Vector3[] paths)
	{
		base.SetPatrolPath(isLoop, nMoveSpeed, paths);
		_patrolIsLoop = isLoop;
		_patrolPaths = new Vector3[paths.Length];
		for (int i = 0; i < paths.Length; i++)
		{
			_patrolPaths[i] = paths[i];
		}
		if (_patrolPaths.Length > 1)
		{
			isPatrol = true;
		}
		if (isPatrol)
		{
			_transform.position = new Vector3(_patrolPaths[0].x, _transform.position.y, _transform.position.z);
			Controller.LogicPosition = new VInt3(_transform.position);
		}
	}
}
