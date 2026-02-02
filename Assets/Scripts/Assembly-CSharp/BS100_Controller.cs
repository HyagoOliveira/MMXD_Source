#define RELEASE
using System;
using CallbackDefs;
using NaughtyAttributes;
using Newtonsoft.Json;
using StageLib;
using UnityEngine;

public class BS100_Controller : EnemyControllerBase, IManagedUpdateBehavior
{
	private enum MainStatus
	{
		Idle = 0,
		Debut = 1,
		Skill0 = 2,
		Skill1 = 3,
		Skill2 = 4,
		Skill3 = 5,
		Die = 6
	}

	private enum SubStatus
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
		Phase10 = 10,
		Phase11 = 11,
		Phase12 = 12,
		Phase13 = 13,
		Phase14 = 14,
		MAX_SUBSTATUS = 15
	}

	private enum WeaponID
	{
		Spray = 0,
		Gatling = 1,
		Skill = 2
	}

	public enum AnimationID
	{
		ANI_IDLE = 0,
		ANI_IDLE_WEAPON = 1,
		ANI_DEBUT = 3,
		ANI_SWITCH = 3,
		ANI_SKILL0_START = 4,
		ANI_SKILL1_START1 = 5,
		ANI_SKILL1_END1 = 6,
		ANI_SKILL1_START2 = 7,
		ANI_SKILL2_LOOP1 = 8,
		ANI_SKILL2_LOOP2 = 9,
		ANI_SKILL3_LOOP = 10,
		ANI_DASH_START = 11,
		ANI_DASH_LOOP = 13,
		ANI_DASH_END = 15,
		ANI_JUMP_START1 = 17,
		ANI_JUMP_LOOP1 = 19,
		ANI_JUMP_START2 = 21,
		ANI_JUMP_LOOP2 = 23,
		ANI_JUMP_END = 25,
		ANI_HURT = 27,
		ANI_DEAD = 28,
		ANI_DEAD_LOOP = 29,
		MAX_ANIMATION_ID = 30
	}

	[SerializeField]
	[NaughtyAttributes.ReadOnly]
	private MainStatus _mainStatus;

	[SerializeField]
	[NaughtyAttributes.ReadOnly]
	private SubStatus _subStatus;

	[SerializeField]
	[NaughtyAttributes.ReadOnly]
	private AnimationID _currentAnimationId;

	[SerializeField]
	[NaughtyAttributes.ReadOnly]
	private float _currentFrame;

	private int nDeadCount;

	private int[] _animationHash;

	[Header("待機")]
	[SerializeField]
	private float IdleWaitTime = 0.2f;

	private int IdleWaitFrame;

	[SerializeField]
	[Tooltip("判斷X距離")]
	private float JudgeXDis = 8f;

	[SerializeField]
	private GameObject HandMesh;

	[SerializeField]
	private CharacterMaterial GunMesh1;

	[SerializeField]
	private CharacterMaterial GunMesh2;

	[SerializeField]
	private CharacterMaterial GunMesh3;

	[Header("通用")]
	private Vector3 StartPos;

	private Vector3 EndPos;

	private float MaxXPos;

	private float MinXPos;

	private float CenterXPos;

	private int ShootTimes;

	private float ShootFrame;

	private bool HasShot;

	private float ShotAngle;

	private float NextAngle;

	private readonly int _HashAngle = Animator.StringToHash("Angle");

	[SerializeField]
	private int JumpSpeed = Mathf.RoundToInt(OrangeBattleUtility.PlayerJumpSpeed * OrangeBattleUtility.PPU * OrangeBattleUtility.FPS * 1000f);

	private int DashSpeed = Mathf.RoundToInt(OrangeBattleUtility.PlayerDashSpeed * OrangeBattleUtility.PPU * OrangeBattleUtility.FPS * 1000f);

	private int WalkSpeed = Mathf.RoundToInt(OrangeBattleUtility.PlayerWalkSpeed * OrangeBattleUtility.PPU * OrangeBattleUtility.FPS * 1000f);

	[SerializeField]
	private WeaponID UseWeapon;

	[Header("登場")]
	[SerializeField]
	private ParticleSystem Debut1;

	[SerializeField]
	private ParticleSystem Debut2;

	[SerializeField]
	private float DebutTime1 = 0.3f;

	[SerializeField]
	private float DebutTime2 = 0.2f;

	private int DebutFrame;

	[Header("黑箭")]
	[SerializeField]
	private int Skill0ShootTimes = 3;

	[SerializeField]
	private Transform Skill0ShotPos;

	[SerializeField]
	private int ShootTiming = -1000;

	[SerializeField]
	private float JumpTime = 0.1f;

	private int JumpFrame;

	private bool HasHit;

	[Header("電離子槍")]
	[SerializeField]
	private Transform Skill1ShotPos;

	[SerializeField]
	[Tooltip("判斷Y距離")]
	private float JudgeYDis = 5f;

	[SerializeField]
	[Tooltip("過高Y距離")]
	private float TooHighYDis = 8f;

	[SerializeField]
	[Tooltip("起跳X距離")]
	private float JumpXDis = 6f;

	[SerializeField]
	[Tooltip("發動攻擊距離")]
	private float AtkDis = 6f;

	[Header("噴槍武器")]
	[SerializeField]
	private float Skill2ShootTime = 5f;

	private float Skill2ShootFrame;

	[SerializeField]
	[Tooltip("射擊間隔N幀")]
	private int Skill2ShootInterval = 5;

	[SerializeField]
	private Transform Skill2ShotPos;

	[SerializeField]
	[Tooltip("衝刺追擊距離")]
	private float DashChaseDis = 2.5f;

	[SerializeField]
	[Tooltip("走路追擊距離")]
	private float WalkChaseDis = 2f;

	[SerializeField]
	[Tooltip("開始追擊距離")]
	private float StartChaseDis = 3f;

	[Header("機槍武器")]
	[SerializeField]
	[Tooltip("間歇射擊次數")]
	private int SKill3ShootTimes = 2;

	[SerializeField]
	[Tooltip("間歇射擊時間")]
	private float Skill3ShootTime = 3f;

	[SerializeField]
	private float SKill3RestTime = 1f;

	private float Skill3ActFrame;

	[SerializeField]
	private int Skill3ShootInterval = 2;

	[SerializeField]
	private float StartAngle = -15f;

	[SerializeField]
	private float EndAngle = 195f;

	[SerializeField]
	private Transform Skill3ShotPos;

	[Header("Debug用")]
	[SerializeField]
	private bool DebugMode;

	[SerializeField]
	private MainStatus NextSkill;

	private int AIStep;

	private int AISkill1Count;

	private void OnEnable()
	{
		MonoBehaviourSingleton<UpdateManager>.Instance.AddUpdate(this);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		MonoBehaviourSingleton<UpdateManager>.Instance.RemoveUpdate(this);
	}

	protected void ChangeDebugMode()
	{
		DebugMode = !DebugMode;
	}

	protected void ChangeSetSkill(object[] param)
	{
		string text = param[0] as string;
		if (!(text == string.Empty))
		{
			switch (text)
			{
			case "Idle":
				NextSkill = MainStatus.Idle;
				break;
			case "Skill0":
				NextSkill = MainStatus.Skill0;
				break;
			case "Skill1":
				NextSkill = MainStatus.Skill1;
				break;
			case "Skill2":
				NextSkill = MainStatus.Skill2;
				break;
			case "Skill3":
				NextSkill = MainStatus.Skill3;
				break;
			}
		}
	}

	protected virtual void HashAnimation()
	{
		_animationHash = new int[30];
		_animationHash[0] = Animator.StringToHash("idle_loop");
		_animationHash[1] = Animator.StringToHash("spray_stand_loop");
		_animationHash[2] = Animator.StringToHash("gatling_stand_loop");
		_animationHash[3] = Animator.StringToHash("debut");
		_animationHash[3] = Animator.StringToHash("skill_switch_weapon");
		_animationHash[4] = Animator.StringToHash("ch013_skill_01_jump_mid");
		_animationHash[5] = Animator.StringToHash("skill_02_stand_start");
		_animationHash[6] = Animator.StringToHash("ch013_skill_02_end");
		_animationHash[7] = Animator.StringToHash("skill_02_jump_start");
		_animationHash[8] = Animator.StringToHash("skill_03_stand");
		_animationHash[9] = Animator.StringToHash("skill_03_walk");
		_animationHash[10] = Animator.StringToHash("skill_04_stand");
		_animationHash[11] = Animator.StringToHash("spray_dash_start");
		_animationHash[12] = Animator.StringToHash("gatling_dash_start");
		_animationHash[13] = Animator.StringToHash("spray_dash_loop");
		_animationHash[14] = Animator.StringToHash("gatling_dash_loop");
		_animationHash[15] = Animator.StringToHash("spray_dash_end");
		_animationHash[16] = Animator.StringToHash("gatling_dash_end");
		_animationHash[17] = Animator.StringToHash("spray_jump_start");
		_animationHash[18] = Animator.StringToHash("gatling_jump_start");
		_animationHash[19] = Animator.StringToHash("spray_jump_loop");
		_animationHash[20] = Animator.StringToHash("gatling_jump_loop");
		_animationHash[21] = Animator.StringToHash("spray_jump_to_fall");
		_animationHash[22] = Animator.StringToHash("gatling_jump_to_fall");
		_animationHash[23] = Animator.StringToHash("spray_fall_loop");
		_animationHash[24] = Animator.StringToHash("gatling_fall_loop");
		_animationHash[25] = Animator.StringToHash("spray_landing");
		_animationHash[26] = Animator.StringToHash("gatling_landing");
		_animationHash[27] = Animator.StringToHash("hurt_loop");
		_animationHash[28] = Animator.StringToHash("dead");
		_animationHash[29] = Animator.StringToHash("dead_loop");
	}

	private void LoadParts(ref Transform[] childs)
	{
		ModelTransform = OrangeBattleUtility.FindChildRecursive(ref childs, "model", true);
		_animator = ModelTransform.GetComponent<Animator>();
		_collideBullet = OrangeBattleUtility.FindChildRecursive(ref childs, "Collider", true).gameObject.AddOrGetComponent<CollideBullet>();
		if (HandMesh == null)
		{
			HandMesh = OrangeBattleUtility.FindChildRecursive(ref childs, "HandMesh_L_m", true).gameObject;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		Transform[] childs = _transform.GetComponentsInChildren<Transform>(true);
		LoadParts(ref childs);
		HashAnimation();
		base.AimPoint = new Vector3(0f, 0.6f, 0f);
		OrangeBattleUtility.AddEnemyAutoAimSystem(_transform, out _enemyAutoAimSystem);
		_enemyAutoAimSystem.UpdateAimRange(20f);
		AiTimer.TimerStart();
	}

	protected override void Start()
	{
		base.Start();
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fx_bs086_teleport_out");
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
			UpdateDirection();
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
		case MainStatus.Debut:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				HideWeapon(WeaponID.Skill);
				HideWeapon(WeaponID.Spray);
				HideWeapon(WeaponID.Gatling);
				SwitchFx(Debut1, true);
				SetColliderEnable(false);
				DebutFrame = GameLogicUpdateManager.GameFrame + (int)(DebutTime1 * 20f);
				break;
			case SubStatus.Phase1:
				SwitchFx(Debut2, true);
				DebutFrame = GameLogicUpdateManager.GameFrame + (int)(DebutTime2 * 20f);
				break;
			case SubStatus.Phase2:
				CheckRoomSize();
				break;
			}
			break;
		case MainStatus.Idle:
			UpdateDirection();
			_velocity = VInt3.zero;
			IdleWaitFrame = GameLogicUpdateManager.GameFrame + (int)(IdleWaitTime * 20f);
			break;
		case MainStatus.Skill0:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				AIStep = 1;
				HasHit = false;
				_velocity = VInt3.zero;
				ShootTimes = Skill0ShootTimes;
				_velocity.y = JumpSpeed;
				ShotAngle = 90f;
				_animator.SetFloat(_HashAngle, ShotAngle);
				break;
			case SubStatus.Phase2:
				JumpFrame = GameLogicUpdateManager.GameFrame + (int)(JumpTime * 20f);
				break;
			case SubStatus.Phase3:
				JumpFrame = GameLogicUpdateManager.GameFrame + (int)(JumpTime * 20f);
				_velocity.y = JumpSpeed;
				break;
			case SubStatus.Phase5:
				HideWeapon(UseWeapon);
				ShootFrame = 0.1f;
				HasShot = false;
				IgnoreGravity = true;
				break;
			case SubStatus.Phase7:
				HideWeapon(UseWeapon);
				ShootFrame = 0.1f;
				HasShot = false;
				IgnoreGravity = true;
				break;
			case SubStatus.Phase8:
				ShowWeapon(UseWeapon);
				_velocity = VInt3.zero;
				break;
			}
			break;
		case MainStatus.Skill1:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_velocity = VInt3.zero;
				AIStep = 2;
				EndPos = GetTargetPos();
				UpdateDirection();
				if (EndPos.y - _transform.position.y > TooHighYDis)
				{
					SetStatus(MainStatus.Skill3);
				}
				else if (EndPos.y - _transform.position.y > TooHighYDis && Mathf.Abs(EndPos.x - _transform.position.x) < 3f)
				{
					SetStatus(MainStatus.Skill1, SubStatus.Phase3);
				}
				else if (Vector2.Distance(EndPos, _transform.position) < AtkDis)
				{
					SetStatus(MainStatus.Skill1, SubStatus.Phase6);
				}
				break;
			case SubStatus.Phase1:
			case SubStatus.Phase2:
				_velocity.x = DashSpeed * base.direction;
				break;
			case SubStatus.Phase3:
				_velocity.y = JumpSpeed;
				break;
			case SubStatus.Phase5:
				_velocity = VInt3.zero;
				break;
			case SubStatus.Phase6:
				SwitchWeapon(WeaponID.Skill);
				UpdateDirection();
				ShotAngle = Vector2.Angle(Vector2.up, EndPos - Skill1ShotPos.position);
				_animator.SetFloat(_HashAngle, ShotAngle);
				HasShot = false;
				ShootFrame = 1f;
				break;
			case SubStatus.Phase8:
				SwitchWeapon(WeaponID.Skill);
				IgnoreGravity = true;
				_velocity = VInt3.zero;
				UpdateDirection();
				ShotAngle = Vector2.Angle(Vector2.up, EndPos - Skill1ShotPos.position);
				_animator.SetFloat(_HashAngle, ShotAngle);
				HasShot = false;
				ShootFrame = 1f;
				break;
			case SubStatus.Phase9:
				IgnoreGravity = false;
				SwitchWeapon(UseWeapon);
				break;
			case SubStatus.Phase12:
				HideWeapon(WeaponID.Skill);
				ShowWeapon(UseWeapon);
				if (_transform.position.x > CenterXPos)
				{
					UpdateDirection(1);
				}
				else
				{
					UpdateDirection(-1);
				}
				break;
			case SubStatus.Phase13:
				_velocity.x = DashSpeed * base.direction;
				break;
			case SubStatus.Phase14:
				_velocity = VInt3.zero;
				break;
			}
			break;
		case MainStatus.Skill2:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_velocity = VInt3.zero;
				AIStep = 3;
				ShotAngle = 90f;
				_animator.SetFloat(_HashAngle, ShotAngle);
				if (UseWeapon != 0)
				{
					SwitchWeapon(WeaponID.Spray);
				}
				else
				{
					SetStatus(MainStatus.Skill2, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				EndPos = GetTargetPos();
				UpdateDirection();
				if (Vector2.Distance(EndPos, _transform.position) < DashChaseDis)
				{
					SetStatus(MainStatus.Skill2, SubStatus.Phase3);
				}
				break;
			case SubStatus.Phase2:
				_velocity.x = DashSpeed * base.direction;
				break;
			case SubStatus.Phase3:
				_velocity = VInt3.zero;
				break;
			case SubStatus.Phase4:
				_velocity = VInt3.zero;
				break;
			case SubStatus.Phase5:
				_velocity.x = WalkSpeed * base.direction;
				break;
			case SubStatus.Phase6:
				if (_transform.position.x > CenterXPos)
				{
					UpdateDirection(1);
				}
				else
				{
					UpdateDirection(-1);
				}
				break;
			case SubStatus.Phase7:
				_velocity.x = DashSpeed * base.direction;
				break;
			case SubStatus.Phase8:
				_velocity = VInt3.zero;
				break;
			}
			break;
		case MainStatus.Skill3:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				ShootTimes = SKill3ShootTimes;
				_velocity = VInt3.zero;
				AIStep = 4;
				ShotAngle = 90f;
				_animator.SetFloat(_HashAngle, ShotAngle);
				if (UseWeapon != WeaponID.Gatling)
				{
					SwitchWeapon(WeaponID.Gatling);
				}
				else
				{
					SetStatus(MainStatus.Skill3, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				Skill3ActFrame = GameLogicUpdateManager.GameFrame + (int)(Skill3ShootTime * 20f);
				break;
			case SubStatus.Phase2:
				Skill3ActFrame = GameLogicUpdateManager.GameFrame + (int)(SKill3RestTime * 20f);
				break;
			}
			break;
		case MainStatus.Die:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				PlaySE("BossSE04", "bs034_palette05", 1f);
				base.AllowAutoAim = false;
				_velocity = VInt3.zero;
				nDeadCount = 0;
				IgnoreGravity = false;
				if (!Controller.Collisions.below)
				{
					SetStatus(MainStatus.Die, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase1:
				base.DeadPlayCompleted = true;
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
		case MainStatus.Debut:
			switch (_subStatus)
			{
			case SubStatus.Phase3:
				_currentAnimationId = AnimationID.ANI_DEBUT;
				break;
			case SubStatus.Phase4:
				_currentAnimationId = (AnimationID)(1 + UseWeapon);
				break;
			}
			break;
		case MainStatus.Idle:
			_currentAnimationId = (AnimationID)(1 + UseWeapon);
			break;
		case MainStatus.Skill0:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_currentAnimationId = (AnimationID)(17 + UseWeapon);
				break;
			case SubStatus.Phase1:
				_currentAnimationId = (AnimationID)(19 + UseWeapon);
				break;
			case SubStatus.Phase2:
				_currentAnimationId = (AnimationID)(21 + UseWeapon);
				break;
			case SubStatus.Phase3:
				_currentAnimationId = (AnimationID)(17 + UseWeapon);
				break;
			case SubStatus.Phase4:
				_currentAnimationId = (AnimationID)(19 + UseWeapon);
				break;
			case SubStatus.Phase5:
			case SubStatus.Phase7:
				_currentAnimationId = AnimationID.ANI_SKILL0_START;
				break;
			case SubStatus.Phase6:
				_currentAnimationId = (AnimationID)(23 + UseWeapon);
				break;
			case SubStatus.Phase8:
				_currentAnimationId = (AnimationID)(25 + UseWeapon);
				break;
			}
			break;
		case MainStatus.Skill1:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
			case SubStatus.Phase12:
				_currentAnimationId = (AnimationID)(11 + UseWeapon);
				break;
			case SubStatus.Phase1:
			case SubStatus.Phase2:
			case SubStatus.Phase13:
				_currentAnimationId = (AnimationID)(13 + UseWeapon);
				break;
			case SubStatus.Phase3:
				_currentAnimationId = (AnimationID)(17 + UseWeapon);
				break;
			case SubStatus.Phase4:
				_currentAnimationId = (AnimationID)(19 + UseWeapon);
				break;
			case SubStatus.Phase5:
			case SubStatus.Phase14:
				_currentAnimationId = (AnimationID)(15 + UseWeapon);
				break;
			case SubStatus.Phase6:
				_currentAnimationId = AnimationID.ANI_SKILL1_START1;
				break;
			case SubStatus.Phase7:
				_currentAnimationId = AnimationID.ANI_SKILL1_END1;
				break;
			case SubStatus.Phase8:
				_currentAnimationId = AnimationID.ANI_SKILL1_START2;
				break;
			case SubStatus.Phase9:
				_currentAnimationId = (AnimationID)(21 + UseWeapon);
				break;
			case SubStatus.Phase10:
				_currentAnimationId = (AnimationID)(23 + UseWeapon);
				break;
			case SubStatus.Phase11:
				_currentAnimationId = (AnimationID)(25 + UseWeapon);
				break;
			}
			break;
		case MainStatus.Skill2:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_DEBUT;
				break;
			case SubStatus.Phase1:
			case SubStatus.Phase6:
				_currentAnimationId = (AnimationID)(11 + UseWeapon);
				break;
			case SubStatus.Phase2:
			case SubStatus.Phase7:
				_currentAnimationId = (AnimationID)(13 + UseWeapon);
				break;
			case SubStatus.Phase3:
			case SubStatus.Phase8:
				_currentAnimationId = (AnimationID)(15 + UseWeapon);
				break;
			case SubStatus.Phase4:
				_currentAnimationId = AnimationID.ANI_SKILL2_LOOP1;
				break;
			case SubStatus.Phase5:
				_currentAnimationId = AnimationID.ANI_SKILL2_LOOP2;
				break;
			}
			break;
		case MainStatus.Skill3:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_DEBUT;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_SKILL3_LOOP;
				break;
			}
			break;
		case MainStatus.Die:
			switch (_subStatus)
			{
			default:
				return;
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_DEAD;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_DEAD_LOOP;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_HURT;
				break;
			case SubStatus.Phase3:
				_currentAnimationId = AnimationID.ANI_DEAD;
				break;
			}
			break;
		}
		_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
	}

	private void UpdateNextState(MainStatus status = MainStatus.Idle)
	{
		MainStatus mainStatus = status;
		if (status == MainStatus.Idle)
		{
			switch (_mainStatus)
			{
			case MainStatus.Debut:
				SetStatus(MainStatus.Idle);
				break;
			case MainStatus.Idle:
				switch (AIStep)
				{
				case 0:
					Target = _enemyAutoAimSystem.GetClosetPlayer();
					if ((bool)Target)
					{
						TargetPos = Target.Controller.LogicPosition;
						UpdateDirection();
						mainStatus = ((!(Math.Abs(Target._transform.position.x - _transform.position.x) < JudgeXDis) || AISkill1Count >= 2) ? MainStatus.Skill0 : MainStatus.Skill1);
					}
					else
					{
						mainStatus = MainStatus.Skill0;
					}
					break;
				case 1:
					mainStatus = ((!HasHit) ? MainStatus.Skill3 : MainStatus.Skill2);
					break;
				case 2:
					mainStatus = MainStatus.Skill3;
					break;
				case 3:
					AIStep = 0;
					SetStatus(MainStatus.Idle);
					break;
				case 4:
					AIStep = 0;
					SetStatus(MainStatus.Idle);
					break;
				}
				break;
			}
		}
		if (mainStatus == MainStatus.Skill1)
		{
			AISkill1Count++;
		}
		else
		{
			AISkill1Count = 0;
		}
		if (DebugMode)
		{
			mainStatus = NextSkill;
		}
		if (mainStatus != 0 && CheckHost())
		{
			UploadEnemyStatus((int)mainStatus);
		}
	}

	public override void LogicUpdate()
	{
		if (_mainStatus == MainStatus.Debut)
		{
			BaseUpdate();
			UpdateGravity();
			Controller.Move((_velocity + _velocityExtra) * GameLogicUpdateManager.m_fFrameLen + _velocityShift);
			distanceDelta = Vector3.Distance(base.transform.localPosition, Controller.LogicPosition.vec3) * (Time.deltaTime / GameLogicUpdateManager.m_fFrameLen);
			_velocityExtra = VInt3.zero;
			_velocityShift = VInt3.zero;
		}
		if (!Activate && _mainStatus != MainStatus.Debut)
		{
			return;
		}
		base.LogicUpdate();
		_currentFrame = _animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
		switch (_mainStatus)
		{
		case MainStatus.Debut:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (GameLogicUpdateManager.GameFrame > DebutFrame)
				{
					SwitchFx(Debut1, false);
					SetStatus(MainStatus.Debut, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if (GameLogicUpdateManager.GameFrame > DebutFrame)
				{
					SetColliderEnable(true);
					_characterMaterial.Appear();
					SwitchFx(Debut2, false);
					SetStatus(MainStatus.Debut, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				if (_introReady)
				{
					PlaySE("BossSE04", "bs034_palette00");
					SetStatus(MainStatus.Debut, SubStatus.Phase3);
				}
				break;
			case SubStatus.Phase3:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Debut, SubStatus.Phase4);
				}
				break;
			case SubStatus.Phase4:
				ShowWeapon(WeaponID.Spray);
				if (IntroCallBack != null)
				{
					IntroCallBack();
					if (!bWaitNetStatus)
					{
						UpdateNextState();
					}
					else
					{
						SetStatus(MainStatus.Idle);
					}
				}
				break;
			}
			break;
		case MainStatus.Idle:
			if (!bWaitNetStatus && IdleWaitFrame < GameLogicUpdateManager.GameFrame)
			{
				UpdateNextState();
			}
			break;
		case MainStatus.Skill0:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill0, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if (_velocity.y < 1000)
				{
					SetStatus(MainStatus.Skill0, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				if (GameLogicUpdateManager.GameFrame > JumpFrame)
				{
					SetStatus(MainStatus.Skill0, SubStatus.Phase3);
				}
				break;
			case SubStatus.Phase3:
				if (GameLogicUpdateManager.GameFrame > JumpFrame)
				{
					SetStatus(MainStatus.Skill0, SubStatus.Phase4);
				}
				break;
			case SubStatus.Phase4:
				if (_velocity.y < 1000)
				{
					SetStatus(MainStatus.Skill0, SubStatus.Phase5);
				}
				break;
			case SubStatus.Phase5:
				if (!HasShot && _currentFrame > ShootFrame)
				{
					HasShot = true;
					EndPos = GetTargetPos();
					BulletBase.TryShotBullet(EnemyWeapons[1].BulletData, Skill0ShotPos.position, Vector3.right * base.direction, null, selfBuffManager.sBuffStatus, EnemyData, targetMask).HitCallback = SKill0Hit;
				}
				if (_currentFrame > 1f)
				{
					IgnoreGravity = false;
					SetStatus(MainStatus.Skill0, SubStatus.Phase6);
				}
				break;
			case SubStatus.Phase6:
				if (Controller.Collisions.below)
				{
					SetStatus(MainStatus.Skill0, SubStatus.Phase7);
				}
				else if (_velocity.y < ShootTiming && --ShootTimes > 0)
				{
					SetStatus(MainStatus.Skill0, SubStatus.Phase5);
				}
				break;
			case SubStatus.Phase7:
				if (!HasShot && _currentFrame > ShootFrame)
				{
					HasShot = true;
					EndPos = GetTargetPos();
					BulletBase.TryShotBullet(EnemyWeapons[1].BulletData, Skill0ShotPos.position, Vector3.right * base.direction, null, selfBuffManager.sBuffStatus, EnemyData, targetMask).HitCallback = SKill0Hit;
				}
				if (_currentFrame > 1f)
				{
					IgnoreGravity = false;
					SetStatus(MainStatus.Skill0, SubStatus.Phase8);
				}
				break;
			case SubStatus.Phase8:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Idle);
				}
				break;
			}
			break;
		case MainStatus.Skill1:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (_currentFrame > 1f)
				{
					if (EndPos.y - _transform.position.y > JudgeYDis)
					{
						SetStatus(MainStatus.Skill1, SubStatus.Phase2);
					}
					else
					{
						SetStatus(MainStatus.Skill1, SubStatus.Phase1);
					}
				}
				break;
			case SubStatus.Phase1:
				if (Vector2.Distance(EndPos, _transform.position) < AtkDis)
				{
					SetStatus(MainStatus.Skill1, SubStatus.Phase5);
				}
				else if (Controller.Collisions.left || Controller.Collisions.right)
				{
					SetStatus(MainStatus.Skill1, SubStatus.Phase14);
				}
				break;
			case SubStatus.Phase2:
				if (Mathf.Abs(EndPos.x - _transform.position.x) < JumpXDis)
				{
					SetStatus(MainStatus.Skill1, SubStatus.Phase3);
				}
				else if (Vector2.Distance(EndPos, _transform.position) < AtkDis)
				{
					SetStatus(MainStatus.Skill1, SubStatus.Phase5);
				}
				else if (Controller.Collisions.left || Controller.Collisions.right)
				{
					SetStatus(MainStatus.Skill1, SubStatus.Phase14);
				}
				break;
			case SubStatus.Phase3:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill1, SubStatus.Phase4);
				}
				break;
			case SubStatus.Phase4:
				if (Vector2.Distance(EndPos, _transform.position) < AtkDis)
				{
					SetStatus(MainStatus.Skill1, SubStatus.Phase8);
				}
				else if (_velocity.y < 1000)
				{
					SetStatus(MainStatus.Skill1, SubStatus.Phase9);
				}
				break;
			case SubStatus.Phase5:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill1, SubStatus.Phase6);
				}
				break;
			case SubStatus.Phase6:
				if (!HasShot && _currentFrame > ShootFrame)
				{
					UpdateDirection();
					ShotAngle = Vector3.Angle((EndPos - Skill1ShotPos.position).normalized, Vector3.up);
					_animator.SetFloat(_HashAngle, ShotAngle);
					HasShot = true;
					CollideBullet obj2 = BulletBase.TryShotBullet(EnemyWeapons[2].BulletData, Skill1ShotPos, Vector3.zero, null, selfBuffManager.sBuffStatus, EnemyData, targetMask) as CollideBullet;
					Quaternion quaternion2 = Quaternion.AngleAxis(ShotAngle * (float)(-base.direction) + 90f, Vector3.forward);
					obj2._transform.rotation = quaternion2;
					obj2.bNeedBackPoolModelName = true;
				}
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill1, SubStatus.Phase7);
				}
				break;
			case SubStatus.Phase7:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill1, SubStatus.Phase12);
				}
				break;
			case SubStatus.Phase8:
				if (!HasShot && _currentFrame > ShootFrame)
				{
					UpdateDirection();
					ShotAngle = Vector3.Angle((EndPos - Skill1ShotPos.position).normalized, Vector3.up);
					_animator.SetFloat(_HashAngle, ShotAngle);
					HasShot = true;
					CollideBullet obj = BulletBase.TryShotBullet(EnemyWeapons[2].BulletData, Skill1ShotPos, Vector3.zero, null, selfBuffManager.sBuffStatus, EnemyData, targetMask) as CollideBullet;
					Quaternion quaternion = Quaternion.AngleAxis(ShotAngle * (float)(-base.direction) + 90f, Vector3.forward);
					obj._transform.rotation = quaternion;
					obj.bNeedBackPoolModelName = true;
				}
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill1, SubStatus.Phase9);
				}
				break;
			case SubStatus.Phase9:
				if (!HasShot && Vector2.Distance(EndPos, _transform.position) < AtkDis)
				{
					SetStatus(MainStatus.Skill1, SubStatus.Phase8);
				}
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill1, SubStatus.Phase10);
				}
				break;
			case SubStatus.Phase10:
				if (Controller.Collisions.below)
				{
					SetStatus(MainStatus.Skill1, SubStatus.Phase11);
				}
				break;
			case SubStatus.Phase11:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill1, SubStatus.Phase12);
				}
				break;
			case SubStatus.Phase12:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill1, SubStatus.Phase13);
				}
				break;
			case SubStatus.Phase13:
				if (Controller.Collisions.left || Controller.Collisions.right)
				{
					SetStatus(MainStatus.Skill1, SubStatus.Phase14);
				}
				break;
			case SubStatus.Phase14:
				if (_currentFrame > 1f)
				{
					AIStep = 0;
					SetStatus(MainStatus.Idle);
				}
				break;
			}
			break;
		case MainStatus.Skill2:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill2, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill2, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				if (Mathf.Abs(EndPos.x - _transform.position.x) < DashChaseDis)
				{
					SetStatus(MainStatus.Skill2, SubStatus.Phase3);
				}
				else if (Controller.Collisions.left || Controller.Collisions.right)
				{
					SetStatus(MainStatus.Skill2, SubStatus.Phase5);
				}
				break;
			case SubStatus.Phase3:
				if (_currentFrame > 1f)
				{
					Skill2ShootFrame = GameLogicUpdateManager.GameFrame + (int)(Skill2ShootTime * 20f);
					SetStatus(MainStatus.Skill2, SubStatus.Phase4);
				}
				break;
			case SubStatus.Phase4:
				EndPos = GetTargetPos(false);
				UpdateDirection();
				NextAngle = Vector3.Angle((EndPos - Skill2ShotPos.position).normalized, Vector3.up);
				ShotAngle = Mathf.Lerp(ShotAngle, NextAngle, 0.3f);
				_animator.SetFloat(_HashAngle, ShotAngle);
				if ((float)GameLogicUpdateManager.GameFrame > Skill2ShootFrame)
				{
					SetStatus(MainStatus.Skill2, SubStatus.Phase6);
					break;
				}
				if (Mathf.Abs(EndPos.x - _transform.position.x) > StartChaseDis)
				{
					SetStatus(MainStatus.Skill2, SubStatus.Phase5);
				}
				if (((float)GameLogicUpdateManager.GameFrame - Skill2ShootFrame) % (float)Skill2ShootInterval == 0f)
				{
					Vector3 pDirection3 = Quaternion.AngleAxis((ShotAngle - 90f) * (float)(-base.direction), Vector3.forward) * (Vector3.right * base.direction);
					BulletBase.TryShotBullet(EnemyWeapons[3].BulletData, Skill2ShotPos, pDirection3, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
				}
				break;
			case SubStatus.Phase5:
				EndPos = GetTargetPos(false);
				UpdateDirection();
				_velocity.x = WalkSpeed * base.direction;
				NextAngle = Vector3.Angle((EndPos - Skill2ShotPos.position).normalized, Vector3.up);
				ShotAngle = Mathf.Lerp(ShotAngle, NextAngle, 0.3f);
				_animator.SetFloat(_HashAngle, ShotAngle);
				if ((float)GameLogicUpdateManager.GameFrame > Skill2ShootFrame)
				{
					SetStatus(MainStatus.Skill2, SubStatus.Phase6);
					break;
				}
				if (Mathf.Abs(EndPos.x - _transform.position.x) < WalkChaseDis)
				{
					SetStatus(MainStatus.Skill2, SubStatus.Phase4);
				}
				if (((float)GameLogicUpdateManager.GameFrame - Skill2ShootFrame) % (float)Skill2ShootInterval == 0f)
				{
					Vector3 pDirection2 = Quaternion.AngleAxis((ShotAngle - 90f) * (float)(-base.direction), Vector3.forward) * (Vector3.right * base.direction);
					BulletBase.TryShotBullet(EnemyWeapons[3].BulletData, Skill2ShotPos, pDirection2, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
				}
				break;
			case SubStatus.Phase6:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill2, SubStatus.Phase7);
				}
				break;
			case SubStatus.Phase7:
				if (Controller.Collisions.left || Controller.Collisions.right)
				{
					SetStatus(MainStatus.Skill2, SubStatus.Phase8);
				}
				break;
			case SubStatus.Phase8:
				if (_currentFrame > 1f)
				{
					AIStep = 0;
					SetStatus(MainStatus.Idle);
				}
				break;
			}
			break;
		case MainStatus.Skill3:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (_currentFrame > 1f)
				{
					Skill2ShootFrame = GameLogicUpdateManager.GameFrame + (int)(Skill2ShootTime * 20f);
					SetStatus(MainStatus.Skill3, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				EndPos = GetTargetPos(false);
				UpdateDirection();
				NextAngle = Vector3.Angle((EndPos - Skill3ShotPos.position).normalized, Vector3.up);
				ShotAngle = Mathf.Lerp(ShotAngle, NextAngle, 0.3f);
				_animator.SetFloat(_HashAngle, ShotAngle);
				if ((float)GameLogicUpdateManager.GameFrame > Skill3ActFrame)
				{
					AIStep = 0;
					if (--ShootTimes > 0)
					{
						SetStatus(MainStatus.Skill3, SubStatus.Phase2);
					}
					else
					{
						SetStatus(MainStatus.Idle);
					}
				}
				else if (((float)GameLogicUpdateManager.GameFrame - Skill3ActFrame) % (float)Skill3ShootInterval == 0f)
				{
					Vector3 pDirection = Quaternion.AngleAxis((ShotAngle - 90f) * (float)(-base.direction), Vector3.forward) * (Vector3.right * base.direction);
					BulletBase.TryShotBullet(EnemyWeapons[4].BulletData, Skill3ShotPos, pDirection, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
				}
				break;
			case SubStatus.Phase2:
				if ((float)GameLogicUpdateManager.GameFrame > Skill3ActFrame)
				{
					SetStatus(MainStatus.Skill3, SubStatus.Phase1);
				}
				break;
			}
			break;
		case MainStatus.Die:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
			case SubStatus.Phase3:
				if (_currentFrame > 0.4f)
				{
					if (nDeadCount > 10)
					{
						SetStatus(MainStatus.Die, SubStatus.Phase1);
					}
					else
					{
						nDeadCount++;
					}
				}
				break;
			case SubStatus.Phase2:
				if (Controller.Collisions.below)
				{
					SetStatus(MainStatus.Die, SubStatus.Phase3);
				}
				break;
			case SubStatus.Phase1:
				break;
			}
			break;
		}
	}

	public void UpdateFunc()
	{
		if (Activate || _mainStatus == MainStatus.Debut)
		{
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
			AIStep = 0;
			SetStatus(MainStatus.Debut);
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
		ModelTransform.localScale = new Vector3(ModelTransform.localScale.x, ModelTransform.localScale.y, Mathf.Abs(ModelTransform.localScale.z) * (float)base.direction);
		_transform.position = pos;
	}

	public override void BossIntro(Action cb)
	{
		IntroCallBack = cb;
		_introReady = true;
	}

	public override void Unlock()
	{
		_unlockReady = true;
		base.AllowAutoAim = true;
		if ((int)Hp > 0)
		{
			SetColliderEnable(true);
		}
		if (InGame)
		{
			Activate = true;
		}
	}

	protected override void DeadBehavior(ref HurtPassParam tHurtPassParam)
	{
		if (_mainStatus != MainStatus.Die)
		{
			HideWeapon(UseWeapon);
			HideWeapon(WeaponID.Skill);
			if ((bool)_collideBullet)
			{
				_collideBullet.BackToPool();
			}
			IgnoreGravity = false;
			StageUpdate.SlowStage();
			SetColliderEnable(false);
			SetStatus(MainStatus.Die);
		}
	}

	private Vector3 GetTargetPos(bool realcenter = true)
	{
		if (!Target)
		{
			Target = _enemyAutoAimSystem.GetClosetPlayer();
		}
		if ((bool)Target)
		{
			if (realcenter)
			{
				TargetPos = new VInt3(Target.Controller.GetRealCenterPos());
			}
			else
			{
				TargetPos = new VInt3(Target.GetTargetPoint());
			}
			return TargetPos.vec3;
		}
		return _transform.position + Vector3.right * 3f * base.direction;
	}

	private void SKill0Hit(object obj)
	{
		if (obj != null)
		{
			Collider2D collider2D = obj as Collider2D;
			if (obj != null && (bool)collider2D.transform.GetComponent<OrangeCharacter>())
			{
				HasHit = true;
			}
		}
	}

	private void SwitchWeapon(WeaponID weapon)
	{
		switch (UseWeapon)
		{
		case WeaponID.Spray:
			HandMesh.SetActive(true);
			GunMesh2.Disappear(delegate
			{
				ShowWeapon(weapon);
			});
			break;
		case WeaponID.Gatling:
			GunMesh3.Disappear(delegate
			{
				ShowWeapon(weapon);
			});
			break;
		case WeaponID.Skill:
			GunMesh1.Disappear(delegate
			{
				ShowWeapon(weapon);
			});
			break;
		}
	}

	private void ShowWeapon(WeaponID weapon)
	{
		switch (weapon)
		{
		case WeaponID.Spray:
			UseWeapon = weapon;
			HandMesh.SetActive(false);
			GunMesh2.Appear();
			break;
		case WeaponID.Gatling:
			UseWeapon = weapon;
			GunMesh3.Appear();
			break;
		case WeaponID.Skill:
			GunMesh1.Appear();
			break;
		}
	}

	private void HideWeapon(WeaponID weapon)
	{
		switch (weapon)
		{
		case WeaponID.Spray:
			HandMesh.SetActive(true);
			GunMesh2.Disappear();
			break;
		case WeaponID.Gatling:
			GunMesh3.Disappear();
			break;
		case WeaponID.Skill:
			GunMesh1.Disappear();
			break;
		}
	}

	private void CheckRoomSize()
	{
		int layerMask = (1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockLayer) | (1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockEnemyLayer) | (1 << ManagedSingleton<OrangeLayerManager>.Instance.WallKickMask);
		Vector3 vector = new Vector3(_transform.position.x, _transform.position.y + 5f, 0f);
		RaycastHit2D raycastHit2D = OrangeBattleUtility.RaycastIgnoreSelf(vector, Vector3.left, 30f, layerMask, _transform);
		RaycastHit2D raycastHit2D2 = OrangeBattleUtility.RaycastIgnoreSelf(vector, Vector3.right, 30f, layerMask, _transform);
		if (!raycastHit2D | !raycastHit2D2)
		{
			Debug.LogError("沒有偵測到某一邊的牆壁，之後技能無法準確判斷位置");
			return;
		}
		MaxXPos = raycastHit2D2.point.x;
		MinXPos = raycastHit2D.point.x;
		CenterXPos = (MaxXPos + MinXPos) / 2f;
	}

	private void SwitchFx(ParticleSystem Fx, bool onoff)
	{
		if ((bool)Fx)
		{
			if (onoff)
			{
				Fx.Play();
			}
			else
			{
				Fx.Stop();
			}
		}
		else
		{
			Debug.Log("特效載入有誤，目前狀態是 " + _mainStatus);
		}
	}
}
