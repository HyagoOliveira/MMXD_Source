#define RELEASE
using System;
using System.Collections.Generic;
using CallbackDefs;
using NaughtyAttributes;
using Newtonsoft.Json;
using StageLib;
using UnityEngine;

public class BS094_Controller : EnemyControllerBase, IManagedUpdateBehavior
{
	private enum MainStatus
	{
		Idle = 0,
		Debut = 1,
		Skill0 = 2,
		Skill1 = 3,
		Skill2 = 4,
		Skill3 = 5,
		Skill4 = 6,
		Die = 7
	}

	private enum SubStatus
	{
		Phase0 = 0,
		Phase1 = 1,
		Phase2 = 2,
		Phase3 = 3,
		Phase4 = 4,
		Phase5 = 5,
		MAX_SUBSTATUS = 6
	}

	public enum AnimationID
	{
		ANI_IDLE = 0,
		ANI_DEBUT = 1,
		ANI_SKILL0_START = 2,
		ANI_SKILL0_LOOP = 3,
		ANI_SKILL0_END = 4,
		ANI_SKILL0_END2 = 5,
		ANI_SKILL1_START = 6,
		ANI_SKILL1_LOOP = 7,
		ANI_SKILL1_END = 8,
		ANI_SKILL2_START1 = 9,
		ANI_SKILL2_LOOP1 = 10,
		ANI_SKILL2_START2 = 11,
		ANI_SKILL2_LOOP2 = 12,
		ANI_SKILL2_END2 = 13,
		ANI_SKILL4_START = 14,
		ANI_SKILL4_LOOP = 15,
		ANI_SKILL4_END = 16,
		ANI_HURT = 17,
		ANI_DEAD = 18,
		MAX_ANIMATION_ID = 19
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

	private int[] DefaultSkillCard = new int[5] { 0, 1, 2, 3, 4 };

	private static int[] DefaultRangedSkillCard = new int[3] { 3, 4, 5 };

	private List<int> RangedSKC = new List<int>();

	private List<int> SkillCard = new List<int>();

	private Vector3 LastTargetPos = new Vector3(0f, 0f, 0f);

	[SerializeField]
	private GameObject HandGunL;

	[SerializeField]
	private GameObject LHand1;

	[SerializeField]
	private GameObject LHand2;

	private bool UsingLGun;

	[SerializeField]
	private ParticleSystem Charge1;

	[SerializeField]
	private ParticleSystem Debut1;

	[SerializeField]
	private ParticleSystem Debut2;

	[SerializeField]
	private ParticleSystem Leave1;

	[SerializeField]
	private ParticleSystem Leave2;

	[SerializeField]
	private float IdleWaitTime = 0.2f;

	private int IdleWaitFrame;

	private int ShootTimes;

	private float ShootFrame;

	private bool HasShot;

	private Vector3 ShotPos;

	private Vector3 EndPos;

	private Vector3 MaxPos;

	private Vector3 MinPos;

	private Vector3 CenterPos;

	[SerializeField]
	private Transform HandGun;

	[SerializeField]
	private float ChangeColorTime = 1f;

	private int ChangeColorFrame;

	[Header("登場")]
	[SerializeField]
	private float DebutTime1 = 0.3f;

	[SerializeField]
	private float DebutTime2 = 0.2f;

	private int DebutFrame;

	[Header("手炮攻擊")]
	[SerializeField]
	private float Skill0ShootTime = 1f;

	private int Skill0ShootFrame;

	[SerializeField]
	private int Skill0ShootInterval = 5;

	private float ShotAngle;

	private readonly int _HashAngle = Animator.StringToHash("Angle");

	[SerializeField]
	private float Skill0ChargeTime = 1.5f;

	private int Skill0ChargeFrame;

	[Header("天狗刀刃")]
	[SerializeField]
	private int RushSpeed;

	[SerializeField]
	private ParticleSystem SlashFX;

	[Header("閃電雷擊")]
	[SerializeField]
	private int Skill2ShootTimes = 3;

	[Header("遙控水雷")]
	private int Skill3Pattern;

	private int Skill3Order;

	[Header("渥斯呼叫")]
	[SerializeField]
	private Vector3 SpawnPos;

	private bool hasSpawn;

	private BossCorpsTool DogCorp;

	private EM167_Controller EMDog;

	private int SpawnCount;

	[Header("死亡離場")]
	[SerializeField]
	private float LeaveTime1 = 0.3f;

	[SerializeField]
	private float LeaveTime2 = 0.2f;

	private int LeaveFrame;

	[Header("Debug用")]
	[SerializeField]
	private bool DebugMode;

	[SerializeField]
	private MainStatus NextSkill;

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
			case "Skill4":
				NextSkill = MainStatus.Skill4;
				break;
			case "Skill9":
				SetStatus(MainStatus.Idle);
				break;
			}
		}
	}

	protected virtual void HashAnimation()
	{
		_animationHash = new int[19];
		_animationHash[0] = Animator.StringToHash("BS094@idle_loop");
		_animationHash[1] = Animator.StringToHash("BS094@debut");
		_animationHash[2] = Animator.StringToHash("BS094@skill_01_start");
		_animationHash[3] = Animator.StringToHash("BS094@skill_01_loop");
		_animationHash[4] = Animator.StringToHash("BS094@skill_01_end");
		_animationHash[5] = Animator.StringToHash("BS094@skill_01_charge_shot");
		_animationHash[6] = Animator.StringToHash("BS094@skill_02_start");
		_animationHash[7] = Animator.StringToHash("BS094@skill_02_loop");
		_animationHash[8] = Animator.StringToHash("BS094@skill_02_end");
		_animationHash[9] = Animator.StringToHash("BS094@skill_03_step1_start");
		_animationHash[10] = Animator.StringToHash("BS094@skill_03_step1_loop");
		_animationHash[11] = Animator.StringToHash("BS094@skill_03_step2_start");
		_animationHash[12] = Animator.StringToHash("BS094@skill_03_step2_loop");
		_animationHash[13] = Animator.StringToHash("BS094@skill_03_step2_end");
		_animationHash[14] = Animator.StringToHash("BS094@skill_05_start");
		_animationHash[15] = Animator.StringToHash("BS094@skill_05_loop");
		_animationHash[16] = Animator.StringToHash("BS094@skill_05_end");
		_animationHash[17] = Animator.StringToHash("BS094@hurt_loop");
		_animationHash[18] = Animator.StringToHash("BS094@dead");
	}

	private void LoadParts(ref Transform[] childs)
	{
		ModelTransform = OrangeBattleUtility.FindChildRecursive(ref childs, "model", true);
		_animator = ModelTransform.GetComponent<Animator>();
		_collideBullet = OrangeBattleUtility.FindChildRecursive(ref childs, "Collider", true).gameObject.AddOrGetComponent<CollideBullet>();
		if (HandGunL == null)
		{
			HandGunL = OrangeBattleUtility.FindChildRecursive(ref childs, "BusterMesh_m", true).gameObject;
		}
		if (LHand1 == null)
		{
			LHand1 = OrangeBattleUtility.FindChildRecursive(ref childs, "HandMesh_L_m", true).gameObject;
		}
		if (LHand2 == null)
		{
			LHand2 = OrangeBattleUtility.FindChildRecursive(ref childs, "HandMesh_L_c", true).gameObject;
		}
		if (HandGun == null)
		{
			HandGun = OrangeBattleUtility.FindChildRecursive(ref childs, "L WeaponPoint", true);
		}
		if (Charge1 == null)
		{
			Charge1 = OrangeBattleUtility.FindChildRecursive(ref childs, "Charge1", true).GetComponent<ParticleSystem>();
		}
		if (Debut1 == null)
		{
			Debut1 = OrangeBattleUtility.FindChildRecursive(ref childs, "Debut1", true).GetComponent<ParticleSystem>();
		}
		if (Debut2 == null)
		{
			Debut2 = OrangeBattleUtility.FindChildRecursive(ref childs, "Debut2", true).GetComponent<ParticleSystem>();
		}
		if (Leave1 == null)
		{
			Leave1 = OrangeBattleUtility.FindChildRecursive(ref childs, "Leave1", true).GetComponent<ParticleSystem>();
		}
		if (Leave2 == null)
		{
			Leave2 = OrangeBattleUtility.FindChildRecursive(ref childs, "Leave2", true).GetComponent<ParticleSystem>();
		}
		if (SlashFX == null)
		{
			SlashFX = OrangeBattleUtility.FindChildRecursive(ref childs, "SlashFX", true).GetComponent<ParticleSystem>();
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
		FallDownSE = new string[2] { "BossSE04", "bs032_forte00" };
	}

	protected override void Start()
	{
		base.Start();
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuseTarget", Skill2ShootTimes);
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
			if (nSet == 5 && netSyncData.nParam0 != -1)
			{
				Skill3Pattern = netSyncData.nParam0;
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
				PlaySE("BossSE04", "bs032_forte06", true);
				SwitchFx(Debut1, true);
				SetColliderEnable(false);
				DebutFrame = GameLogicUpdateManager.GameFrame + (int)(DebutTime1 * 20f);
				break;
			case SubStatus.Phase1:
				SwitchFx(Debut2, true);
				DebutFrame = GameLogicUpdateManager.GameFrame + (int)(DebutTime2 * 20f);
				break;
			case SubStatus.Phase2:
				UseLGun(false);
				_animator.speed = 0f;
				CheckRoomSize();
				break;
			case SubStatus.Phase3:
				_animator.speed = 1f;
				break;
			}
			break;
		case MainStatus.Idle:
			UseLGun(false);
			_velocity.x = 0;
			IdleWaitFrame = GameLogicUpdateManager.GameFrame + (int)(IdleWaitTime * 20f);
			break;
		case MainStatus.Skill0:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_characterMaterial.UpdateTex(0);
				ChangeColorFrame = GameLogicUpdateManager.GameFrame + (int)(ChangeColorTime * 20f);
				break;
			case SubStatus.Phase1:
			{
				UseLGun(true);
				_velocity = VInt3.zero;
				EndPos = GetTargetPos();
				UpdateDirection();
				float value = Vector3.Angle((EndPos - HandGun.position).normalized, Vector3.up);
				_animator.SetFloat(_HashAngle, value);
				break;
			}
			case SubStatus.Phase2:
				Skill0ShootFrame = GameLogicUpdateManager.GameFrame + (int)(Skill0ShootTime * 20f);
				ShootFrame = 0.3f;
				HasShot = false;
				break;
			case SubStatus.Phase4:
				SwitchFx(Charge1, true);
				PlaySE("BossSE04", "bs032_forte03_lp");
				Skill0ChargeFrame = GameLogicUpdateManager.GameFrame + (int)(Skill0ChargeTime * 20f);
				break;
			case SubStatus.Phase5:
				ShootFrame = 0.05f;
				HasShot = false;
				break;
			}
			break;
		case MainStatus.Skill1:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_characterMaterial.UpdateTex(2);
				ChangeColorFrame = GameLogicUpdateManager.GameFrame + (int)(ChangeColorTime * 20f);
				break;
			case SubStatus.Phase1:
				PlaySE("BossSE04", "bs032_forte10");
				_velocity = VInt3.zero;
				break;
			case SubStatus.Phase2:
				SwitchFx(SlashFX, true);
				EndPos = GetTargetPos();
				UpdateDirection();
				EndPos += Vector3.right * 2f * base.direction;
				_velocity.x = RushSpeed * base.direction;
				_collideBullet.UpdateBulletData(EnemyWeapons[2].BulletData);
				_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
				_collideBullet.Active(targetMask);
				break;
			case SubStatus.Phase3:
				_velocity = VInt3.zero;
				_collideBullet.UpdateBulletData(EnemyWeapons[0].BulletData);
				_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
				_collideBullet.Active(targetMask);
				break;
			}
			break;
		case MainStatus.Skill2:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_characterMaterial.UpdateTex(3);
				ChangeColorFrame = GameLogicUpdateManager.GameFrame + (int)(ChangeColorTime * 20f);
				break;
			case SubStatus.Phase1:
				_velocity = VInt3.zero;
				ShootTimes = Skill2ShootTimes;
				break;
			case SubStatus.Phase4:
				EndPos = ShotPos + Vector3.up * 10f;
				switch (ShootTimes)
				{
				case 2:
					EndPos += Vector3.left * 2f;
					break;
				case 1:
					EndPos += Vector3.right * 2f;
					break;
				}
				MonoBehaviourSingleton<FxManager>.Instance.PlayReturn<psSwingTarget>("fxuseTarget", EndPos, Quaternion.Euler(0f, 0f, Vector2.SignedAngle(Vector2.right, Vector2.down)), Array.Empty<object>()).SetEffect(20f, new Color(0.6f, 0f, 0.5f, 0.7f), new Color(0.6f, 0f, 0.5f), 1f);
				break;
			}
			break;
		case MainStatus.Skill3:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_characterMaterial.UpdateTex(1);
				ChangeColorFrame = GameLogicUpdateManager.GameFrame + (int)(ChangeColorTime * 20f);
				break;
			case SubStatus.Phase1:
				PlaySE("BossSE04", "bs032_forte11");
				UseLGun(true);
				_velocity = VInt3.zero;
				switch (Skill3Pattern)
				{
				case 0:
				case 1:
					ShootTimes = 4;
					break;
				case 2:
					ShootTimes = 6;
					break;
				default:
					ShootTimes = 0;
					break;
				}
				Skill3Order = 0;
				ShotAngle = 75f;
				_animator.SetFloat(_HashAngle, ShotAngle);
				break;
			case SubStatus.Phase2:
				ShotAngle = 75 - 10 * Skill3Order;
				_animator.SetFloat(_HashAngle, ShotAngle);
				ShootFrame = 0.4f;
				HasShot = false;
				break;
			}
			break;
		case MainStatus.Skill4:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_characterMaterial.UpdateTex(0);
				ChangeColorFrame = GameLogicUpdateManager.GameFrame + (int)(ChangeColorTime * 20f);
				break;
			case SubStatus.Phase1:
				_velocity = VInt3.zero;
				break;
			case SubStatus.Phase2:
				if (!hasSpawn)
				{
					if ((bool)OrangeBattleUtility.RaycastIgnoreSelf(Controller.GetCenterPos(), Vector2.right * base.direction, 2f, Controller.collisionMask, _transform))
					{
						UpdateDirection(-base.direction);
					}
					SpawnDog();
				}
				break;
			}
			break;
		case MainStatus.Die:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				base.AllowAutoAim = false;
				_velocity = VInt3.zero;
				base.DeadPlayCompleted = true;
				nDeadCount = 0;
				if (!Target)
				{
					Target = _enemyAutoAimSystem.GetClosetPlayer();
				}
				if ((bool)Target)
				{
					TargetPos = new VInt3(Target._transform.position);
				}
				UpdateDirection();
				break;
			case SubStatus.Phase2:
				SetColliderEnable(false);
				_characterMaterial.Disappear();
				SwitchFx(Leave2, true);
				LeaveFrame = GameLogicUpdateManager.GameFrame + (int)(LeaveTime2 * 20f);
				break;
			case SubStatus.Phase3:
				SwitchFx(Leave1, true);
				LeaveFrame = GameLogicUpdateManager.GameFrame + (int)(LeaveTime1 * 20f);
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
			default:
				return;
			case SubStatus.Phase3:
				_currentAnimationId = AnimationID.ANI_DEBUT;
				break;
			case SubStatus.Phase4:
				_currentAnimationId = AnimationID.ANI_IDLE;
				break;
			}
			break;
		case MainStatus.Idle:
			_currentAnimationId = AnimationID.ANI_IDLE;
			break;
		case MainStatus.Skill0:
			switch (_subStatus)
			{
			default:
				return;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_SKILL0_START;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_SKILL0_LOOP;
				break;
			case SubStatus.Phase3:
				_currentAnimationId = AnimationID.ANI_SKILL0_END;
				break;
			case SubStatus.Phase5:
				_currentAnimationId = AnimationID.ANI_SKILL0_END2;
				break;
			case SubStatus.Phase4:
				return;
			}
			break;
		case MainStatus.Skill1:
			switch (_subStatus)
			{
			default:
				return;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_SKILL1_START;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_SKILL1_LOOP;
				break;
			case SubStatus.Phase3:
				_currentAnimationId = AnimationID.ANI_SKILL1_END;
				break;
			}
			break;
		case MainStatus.Skill2:
			switch (_subStatus)
			{
			default:
				return;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_SKILL2_START1;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_SKILL2_LOOP1;
				break;
			case SubStatus.Phase3:
				_currentAnimationId = AnimationID.ANI_SKILL2_START2;
				break;
			case SubStatus.Phase4:
				_currentAnimationId = AnimationID.ANI_SKILL2_LOOP2;
				break;
			case SubStatus.Phase5:
				_currentAnimationId = AnimationID.ANI_SKILL2_END2;
				break;
			}
			break;
		case MainStatus.Skill3:
			switch (_subStatus)
			{
			default:
				return;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_SKILL0_START;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_SKILL0_LOOP;
				break;
			case SubStatus.Phase3:
				_currentAnimationId = AnimationID.ANI_SKILL0_END;
				break;
			}
			break;
		case MainStatus.Skill4:
			switch (_subStatus)
			{
			default:
				return;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_SKILL4_START;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_SKILL4_LOOP;
				break;
			case SubStatus.Phase3:
				_currentAnimationId = AnimationID.ANI_SKILL4_END;
				break;
			}
			break;
		case MainStatus.Die:
			switch (_subStatus)
			{
			default:
				return;
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_IDLE;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_HURT;
				break;
			case SubStatus.Phase3:
				_currentAnimationId = AnimationID.ANI_DEAD;
				break;
			case SubStatus.Phase1:
				return;
			}
			break;
		}
		_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
	}

	private void UpdateRandomState(MainStatus status = MainStatus.Idle)
	{
		MainStatus mainStatus = status;
		if (status == MainStatus.Idle)
		{
			switch (_mainStatus)
			{
			case MainStatus.Debut:
				SetStatus(MainStatus.Idle);
				return;
			case MainStatus.Idle:
				mainStatus = (MainStatus)RandomCard(2);
				break;
			}
		}
		if (DebugMode)
		{
			mainStatus = NextSkill;
		}
		if (mainStatus != 0 && CheckHost())
		{
			if (mainStatus == MainStatus.Skill3)
			{
				int num = OrangeBattleUtility.Random(0, 100);
				int num2 = ((num >= 40) ? ((num < 80) ? 1 : 2) : 0);
				UploadEnemyStatus((int)mainStatus, false, new object[1] { num2 });
			}
			else
			{
				UploadEnemyStatus((int)mainStatus);
			}
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
					PlaySE("BossSE04", "bs032_forte00", true);
					SwitchFx(Debut2, false);
					SetStatus(MainStatus.Debut, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				if (_introReady)
				{
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
				if (IntroCallBack != null)
				{
					IntroCallBack();
					if (!bWaitNetStatus)
					{
						UpdateRandomState();
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
				Target = _enemyAutoAimSystem.GetClosetPlayer();
				if ((bool)Target)
				{
					UpdateDirection();
					UpdateRandomState();
				}
				else
				{
					UpdateRandomState(MainStatus.Skill2);
				}
			}
			break;
		case MainStatus.Skill0:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (GameLogicUpdateManager.GameFrame > ChangeColorFrame)
				{
					SetStatus(MainStatus.Skill0, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill0, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				if ((Skill0ShootFrame - GameLogicUpdateManager.GameFrame) % Skill0ShootInterval == 0)
				{
					EndPos = GetTargetPos();
					UpdateDirection();
					float value = Vector3.Angle((EndPos - HandGun.position).normalized, Vector3.up);
					_animator.SetFloat(_HashAngle, value);
					BulletBase.TryShotBullet(EnemyWeapons[1].BulletData, HandGun, EndPos - HandGun.position, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
				}
				if (GameLogicUpdateManager.GameFrame > Skill0ShootFrame)
				{
					SetStatus(MainStatus.Skill0, SubStatus.Phase4);
				}
				break;
			case SubStatus.Phase3:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Idle);
				}
				break;
			case SubStatus.Phase4:
				if (GameLogicUpdateManager.GameFrame > Skill0ChargeFrame)
				{
					SetStatus(MainStatus.Skill0, SubStatus.Phase5);
				}
				break;
			case SubStatus.Phase5:
				if (!HasShot && _currentFrame > ShootFrame)
				{
					SwitchFx(Charge1, false);
					PlaySE("BossSE04", "bs032_forte03_stop");
					HasShot = true;
					EndPos = GetTargetPos();
					BulletBase.TryShotBullet(EnemyWeapons[6].BulletData, HandGun.position, EndPos - HandGun.position, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
				}
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
				if (GameLogicUpdateManager.GameFrame > ChangeColorFrame)
				{
					SetStatus(MainStatus.Skill1, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill1, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				if ((_transform.position.x - EndPos.x) * (float)base.direction > 0f || Controller.Collisions.right || Controller.Collisions.left)
				{
					SwitchFx(SlashFX, false);
					SetStatus(MainStatus.Skill1, SubStatus.Phase3);
				}
				break;
			case SubStatus.Phase3:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Idle);
				}
				break;
			}
			break;
		case MainStatus.Skill2:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (GameLogicUpdateManager.GameFrame > ChangeColorFrame)
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
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill2, SubStatus.Phase3);
				}
				break;
			case SubStatus.Phase3:
				if (_currentFrame > 1f)
				{
					ShotPos = new Vector3(GetTargetPos().x, MinPos.y, 0f);
					SetStatus(MainStatus.Skill2, SubStatus.Phase4);
				}
				break;
			case SubStatus.Phase4:
				if (_currentFrame > 1f)
				{
					(BulletBase.TryShotBullet(EnemyWeapons[3].BulletData, EndPos, Vector3.zero, null, selfBuffManager.sBuffStatus, EnemyData, targetMask) as CollideBullet).bNeedBackPoolModelName = true;
					if (--ShootTimes > 0)
					{
						SetStatus(MainStatus.Skill2, SubStatus.Phase4);
					}
					else
					{
						SetStatus(MainStatus.Skill2, SubStatus.Phase5);
					}
				}
				break;
			case SubStatus.Phase5:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Idle);
				}
				break;
			}
			break;
		case MainStatus.Skill3:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (GameLogicUpdateManager.GameFrame > ChangeColorFrame)
				{
					SetStatus(MainStatus.Skill3, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill3, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				if (!HasShot && _currentFrame > ShootFrame)
				{
					HasShot = true;
					EndPos = GetSKill3Pos(Skill3Pattern, Skill3Order);
					Vector3 pDirection = EndPos - HandGun.position;
					LocateBullet obj = BulletBase.TryShotBullet(EnemyWeapons[4].BulletData, HandGun.position, pDirection, null, selfBuffManager.sBuffStatus, EnemyData, targetMask) as LocateBullet;
					obj.FreeDISTANCE = 0.1f;
					obj.SetEndPos(EndPos);
					Skill3Order++;
				}
				if (_currentFrame > 1f)
				{
					if (--ShootTimes > 0)
					{
						SetStatus(MainStatus.Skill3, SubStatus.Phase2);
					}
					else
					{
						SetStatus(MainStatus.Skill3, SubStatus.Phase3);
					}
				}
				break;
			case SubStatus.Phase3:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Idle);
				}
				break;
			}
			break;
		case MainStatus.Skill4:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (GameLogicUpdateManager.GameFrame > ChangeColorFrame)
				{
					SetStatus(MainStatus.Skill4, SubStatus.Phase1);
					PlaySE("BossSE04", "bs032_gospel01");
				}
				break;
			case SubStatus.Phase1:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill4, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill4, SubStatus.Phase3);
				}
				break;
			case SubStatus.Phase3:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Idle);
				}
				break;
			}
			break;
		case MainStatus.Die:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
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
			case SubStatus.Phase1:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Die, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				if (GameLogicUpdateManager.GameFrame > LeaveFrame)
				{
					PlaySE("BossSE04", "bs032_forte08");
					PlaySE("BossSE04", "bs032_forte09");
					SwitchFx(Leave2, false);
					SetStatus(MainStatus.Die, SubStatus.Phase3);
				}
				break;
			case SubStatus.Phase3:
				if (GameLogicUpdateManager.GameFrame > LeaveFrame)
				{
					BackToPool();
					SwitchFx(Leave1, false);
					SetStatus(MainStatus.Die, SubStatus.Phase4);
				}
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
			_characterMaterial.Disappear();
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
		ModelTransform.localScale = new Vector3(ModelTransform.localScale.x, ModelTransform.localScale.y, ModelTransform.localScale.z * (float)base.direction);
		base.transform.position = pos;
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

	private int RandomCard(int StartPos)
	{
		if (SkillCard.ToArray().Length < 1)
		{
			SkillCard = new List<int>(DefaultSkillCard);
		}
		int num = SkillCard[OrangeBattleUtility.Random(0, SkillCard.ToArray().Length)];
		SkillCard.Remove(num);
		if (num + StartPos != 6 || !hasSpawn)
		{
			return num + StartPos;
		}
		return RandomCard(StartPos);
	}

	protected override void DeadBehavior(ref HurtPassParam tHurtPassParam)
	{
		if (_mainStatus != MainStatus.Die)
		{
			_characterMaterial.UpdateTex(0);
			if ((bool)_collideBullet)
			{
				_collideBullet.BackToPool();
			}
			StageUpdate.SlowStage();
			SetColliderEnable(false);
			UseLGun(false);
			if ((bool)Charge1)
			{
				PlaySE("BossSE04", "bs032_forte03_stop");
				Charge1.Stop();
			}
			if ((bool)Debut1)
			{
				Debut1.Stop();
			}
			if ((bool)Debut2)
			{
				Debut2.Stop();
			}
			if ((bool)SlashFX)
			{
				SlashFX.Stop();
			}
			SetStatus(MainStatus.Die);
		}
	}

	private void UseLGun(bool use)
	{
		HandGunL.SetActive(use);
		LHand1.SetActive(!use);
		LHand2.SetActive(!use);
		UsingLGun = use;
	}

	private Vector3 GetTargetPos()
	{
		if (!Target)
		{
			Target = _enemyAutoAimSystem.GetClosetPlayer();
		}
		if ((bool)Target)
		{
			TargetPos = new VInt3(Target.Controller.GetRealCenterPos());
			return TargetPos.vec3;
		}
		return _transform.position + Vector3.right * 3f * base.direction;
	}

	private void SpawnDog()
	{
		if (hasSpawn)
		{
			return;
		}
		EnemyControllerBase enemyControllerBase = StageUpdate.StageSpawnEnemyByMob(ManagedSingleton<OrangeDataManager>.Instance.MOB_TABLE_DICT[(int)EnemyWeapons[5].BulletData.f_EFFECT_X], sNetSerialID + SpawnCount);
		if ((bool)enemyControllerBase)
		{
			EMDog = enemyControllerBase.gameObject.GetComponent<EM167_Controller>();
			if ((bool)EMDog)
			{
				Vector3 pos = _transform.position + SpawnPos * base.direction;
				BossCorpsTool bossCorpsTool = new BossCorpsTool(EMDog);
				bossCorpsTool.SetIDAndCB(SpawnCount, null, DogBack);
				EMDog.SetPositionAndRotation(pos, base.direction == -1);
				EMDog.JoinCorps(bossCorpsTool, this);
				EMDog.SetActive(true);
			}
		}
		hasSpawn = true;
		SpawnCount++;
	}

	private void DogBack(object obj)
	{
		hasSpawn = false;
	}

	public override void UpdateEnemyID(int id)
	{
		base.UpdateEnemyID(id);
		UpdateAIState();
		AI_STATE aiState = AiState;
	}

	private void CheckRoomSize()
	{
		int layerMask = (1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockLayer) | (1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockEnemyLayer) | (1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockPlayerLayer);
		Vector3 vector = new Vector3(_transform.position.x, _transform.position.y, 0f);
		RaycastHit2D raycastHit2D = OrangeBattleUtility.RaycastIgnoreSelf(vector, Vector3.left, 30f, layerMask, _transform);
		RaycastHit2D raycastHit2D2 = OrangeBattleUtility.RaycastIgnoreSelf(vector, Vector3.right, 30f, layerMask, _transform);
		RaycastHit2D raycastHit2D3 = OrangeBattleUtility.RaycastIgnoreSelf(vector, Vector3.up, 30f, layerMask, _transform);
		RaycastHit2D raycastHit2D4 = OrangeBattleUtility.RaycastIgnoreSelf(vector, Vector3.down, 30f, layerMask, _transform);
		if (!raycastHit2D4 || !raycastHit2D3)
		{
			Debug.LogError("沒有偵測到地板或天花板，之後一些技能無法準確判斷位置");
			return;
		}
		MaxPos = new Vector3(raycastHit2D2.point.x, raycastHit2D3.point.y, 0f);
		MinPos = new Vector3(raycastHit2D.point.x, raycastHit2D4.point.y, 0f);
		CenterPos = (MaxPos + MinPos) / 2f;
	}

	private Vector3 GetSKill3Pos(int pattern, int order)
	{
		switch (pattern)
		{
		case 0:
			switch (order)
			{
			case 0:
				return new Vector2(MinPos.x + (CenterPos.x - MinPos.x) * 0.5f, MaxPos.y + (CenterPos.y - MaxPos.y) * 0.5f);
			case 1:
				return new Vector2(MaxPos.x + (CenterPos.x - MaxPos.x) * 0.5f, MaxPos.y + (CenterPos.y - MaxPos.y) * 0.5f);
			case 2:
				return new Vector2(MinPos.x + (CenterPos.x - MinPos.x) * 0.5f, MinPos.y + (CenterPos.y - MinPos.y) * 0.5f);
			case 3:
				return new Vector2(MaxPos.x + (CenterPos.x - MaxPos.x) * 0.5f, MinPos.y + (CenterPos.y - MinPos.y) * 0.5f);
			}
			break;
		case 1:
			switch (order)
			{
			case 0:
				return new Vector2(MinPos.x + (MaxPos.x - MinPos.x) * 0.2f, CenterPos.y);
			case 1:
				return new Vector2(MinPos.x + (MaxPos.x - MinPos.x) * 0.4f, CenterPos.y);
			case 2:
				return new Vector2(MinPos.x + (MaxPos.x - MinPos.x) * 0.6f, CenterPos.y);
			case 3:
				return new Vector2(MinPos.x + (MaxPos.x - MinPos.x) * 0.8f, CenterPos.y);
			}
			break;
		case 2:
			switch (order)
			{
			case 0:
				return new Vector2(MinPos.x + (CenterPos.x - MinPos.x) * 0.5f, MaxPos.y + (CenterPos.y - MaxPos.y) * 0.5f + 1.5f);
			case 1:
				return new Vector2(MinPos.x + (CenterPos.x - MinPos.x) * 0.5f, MaxPos.y + (CenterPos.y - MaxPos.y) * 0.5f - 0f);
			case 2:
				return new Vector2(CenterPos.x, MinPos.y + (CenterPos.y - MinPos.y) * 0.5f + 0f);
			case 3:
				return new Vector2(CenterPos.x, MinPos.y + (CenterPos.y - MinPos.y) * 0.5f - 1.5f);
			case 4:
				return new Vector2(MaxPos.x + (CenterPos.x - MaxPos.x) * 0.5f, MaxPos.y + (CenterPos.y - MaxPos.y) * 0.5f + 1.5f);
			case 5:
				return new Vector2(MaxPos.x + (CenterPos.x - MaxPos.x) * 0.5f, MaxPos.y + (CenterPos.y - MaxPos.y) * 0.5f - 0f);
			}
			break;
		}
		return _transform.position + Vector3.up * 3f;
	}

	public void SetHasSpawn()
	{
		hasSpawn = true;
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
