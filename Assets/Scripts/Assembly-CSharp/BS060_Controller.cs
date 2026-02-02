using System;
using System.Collections.Generic;
using CallbackDefs;
using Newtonsoft.Json;
using StageLib;
using UnityEngine;

public class BS060_Controller : EnemyControllerBase, IManagedUpdateBehavior
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
		Skill5 = 7,
		Skill6 = 8,
		Skill7 = 9,
		Skill8 = 10,
		Die = 11
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
		MAX_SUBSTATUS = 12
	}

	public enum AnimationID
	{
		ANI_IDLE = 0,
		ANI_DEBUT_LOOP = 1,
		ANI_DEBUT = 2,
		ANI_SKILL0_START = 3,
		ANI_SKILL0_LOOP = 4,
		ANI_SKILL0_END = 5,
		ANI_SKILL1_START = 6,
		ANI_SKILL1_LOOP = 7,
		ANI_SKILL1_END = 8,
		ANI_SKILL2_START1 = 9,
		ANI_SKILL2_LOOP1 = 10,
		ANI_SKILL2_START2 = 11,
		ANI_SKILL2_LOOP2 = 12,
		ANI_SKILL2_END2 = 13,
		ANI_SKILL3_START1 = 14,
		ANI_SKILL3_LOOP1 = 15,
		ANI_SKILL3_START2 = 16,
		ANI_SKILL3_LOOP2 = 17,
		ANI_SKILL3_START3 = 18,
		ANI_SKILL3_LOOP3 = 19,
		ANI_SKILL3_END3 = 20,
		ANI_SKILL4_START1 = 21,
		ANI_SKILL4_LOOP1 = 22,
		ANI_SKILL4_START2 = 23,
		ANI_SKILL4_LOOP2 = 24,
		ANI_SKILL4_END2 = 25,
		ANI_SKILL4_START3 = 26,
		ANI_SKILL4_LOOP3 = 27,
		ANI_SKILL4_START4 = 28,
		ANI_SKILL4_LOOP4 = 29,
		ANI_SKILL4_END4 = 30,
		ANI_SKILL5_START1 = 31,
		ANI_SKILL5_LOOP1 = 32,
		ANI_SKILL5_START2 = 33,
		ANI_SKILL5_LOOP2 = 34,
		ANI_SKILL5_START3 = 35,
		ANI_SKILL5_LOOP3 = 36,
		ANI_SKILL5_END3 = 37,
		ANI_SKILL6_START = 38,
		ANI_SKILL6_LOOP = 39,
		ANI_SKILL6_END = 40,
		ANI_SKILL7_START = 41,
		ANI_SKILL7_LOOP = 42,
		ANI_SKILL7_END = 43,
		ANI_HURT = 44,
		ANI_DEAD = 45,
		MAX_ANIMATION_ID = 46
	}

	private MainStatus _mainStatus;

	private SubStatus _subStatus;

	private AnimationID _currentAnimationId;

	private float _currentFrame;

	private int[] _animationHash;

	private Vector3 LastTargetPos = new Vector3(0f, 0f, 0f);

	private CollideBullet SwordRCollide;

	private CollideBullet SwordLCollide;

	private int DashSpeed = 15000;

	private static int[] DefaultRangedSkillCard = new int[3] { 0, 3, 2 };

	private static int[] DefaultShortSkillCard = new int[3] { 8, 4, 5 };

	private static int[] DefaultWallSkillCard = new int[3] { 3, 5, 2 };

	private List<int> ShortSKC = new List<int>(DefaultShortSkillCard);

	private List<int> RangedSKC = new List<int>(DefaultRangedSkillCard);

	private List<int> WallSKC = new List<int>(DefaultWallSkillCard);

	private bool RangedATK;

	private bool WallATK;

	private int AllowAction = 36;

	private bool UsingGun;

	private bool UsingSword;

	[SerializeField]
	private ParticleSystem[] ChargeFX = new ParticleSystem[2];

	[SerializeField]
	private ParticleSystem[] SkillFX = new ParticleSystem[15];

	private Transform ShootPoint;

	private int ShootTime;

	private float NextFrame;

	private int ShootNum = 5;

	private CollideBullet Skill8Collide;

	private VInt3 SK3Jump = new VInt3(7200, 14400, 0);

	private int RockShootTime;

	private float NextRockFrame;

	private float NextCloseFrame;

	private bool RockFall;

	private bool _hasShake;

	[SerializeField]
	private Transform RockTarget;

	[SerializeField]
	private SpriteRenderer PredictSpriteRenderer;

	private VInt3 SK5Jump = new VInt3(2400, 20000, 0);

	private bool IsHitPlayer;

	private int PlayerGravity;

	private OrangeCharacter _targetOC;

	public VInt3 SK7Jump = new VInt3(12000, 10500, 0);

	private int SK7Phase;

	[SerializeField]
	private int NoCatckSK7Phase = 2;

	private bool UseSK7;

	private bool _isCatching;

	public int SK7Phase7Jump = 9600;

	public int DuringJump = 1500;

	public int SK7Phase7JumpX = 3200;

	public int DuringJumpX = -10;

	public int HpPhase1 = 7;

	public int HpPhase2 = 4;

	public int HpPhase3 = 2;

	public bool HpPhase1Used;

	public bool HpPhase2Used;

	public bool HpPhase3Used;

	private bool FinalJump = true;

	private bool IsNewGravity;

	private int GravityNew;

	private int VDirection = 1;

	private float VDistance = 0.12f;

	private GameObject Gun;

	private GameObject SwordLMain;

	private GameObject SwordLSub;

	private GameObject SwordRMain;

	private GameObject SwordRSub;

	private int nDeadCount;

	private OrangeCharacter targetOC
	{
		get
		{
			return _targetOC;
		}
		set
		{
			_targetOC = value;
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

	protected virtual void HashAnimation()
	{
		_animationHash[2] = Animator.StringToHash("BS060@debut");
		_animationHash[1] = Animator.StringToHash("BS060@debut_fall_loop");
		_animationHash[0] = Animator.StringToHash("BS060@idle_loop");
		_animationHash[3] = Animator.StringToHash("BS060@skill_01_start");
		_animationHash[4] = Animator.StringToHash("BS060@skill_01_loop");
		_animationHash[5] = Animator.StringToHash("BS060@skill_01_end");
		_animationHash[6] = Animator.StringToHash("BS060@skill_02_start");
		_animationHash[7] = Animator.StringToHash("BS060@skill_02_loop");
		_animationHash[8] = Animator.StringToHash("BS060@skill_02_end");
		_animationHash[9] = Animator.StringToHash("BS060@skill_03_step1_start");
		_animationHash[10] = Animator.StringToHash("BS060@skill_03_step1_loop");
		_animationHash[11] = Animator.StringToHash("BS060@skill_03_step2_start");
		_animationHash[12] = Animator.StringToHash("BS060@skill_03_step2_loop");
		_animationHash[13] = Animator.StringToHash("BS060@skill_03_step2_end");
		_animationHash[14] = Animator.StringToHash("BS060@skill_04_step1_start");
		_animationHash[15] = Animator.StringToHash("BS060@skill_04_step1_loop");
		_animationHash[16] = Animator.StringToHash("BS060@skill_04_step2_start");
		_animationHash[17] = Animator.StringToHash("BS060@skill_04_step2_loop");
		_animationHash[18] = Animator.StringToHash("BS060@skill_04_step3_start");
		_animationHash[19] = Animator.StringToHash("BS060@skill_04_step3_loop");
		_animationHash[20] = Animator.StringToHash("BS060@skill_04_step3_end");
		_animationHash[21] = Animator.StringToHash("BS060@skill_05_step1_start");
		_animationHash[22] = Animator.StringToHash("BS060@skill_05_step1_loop");
		_animationHash[23] = Animator.StringToHash("BS060@skill_05_step2_start");
		_animationHash[24] = Animator.StringToHash("BS060@skill_05_step2_loop");
		_animationHash[25] = Animator.StringToHash("BS060@skill_05_step2_end");
		_animationHash[26] = Animator.StringToHash("BS060@skill_05_step3_start");
		_animationHash[27] = Animator.StringToHash("BS060@skill_05_step3_loop");
		_animationHash[28] = Animator.StringToHash("BS060@skill_05_step4_start");
		_animationHash[29] = Animator.StringToHash("BS060@skill_05_step4_loop");
		_animationHash[30] = Animator.StringToHash("BS060@skill_05_step4_end");
		_animationHash[31] = Animator.StringToHash("BS060@skill_06_step1_start");
		_animationHash[32] = Animator.StringToHash("BS060@skill_06_step1_loop");
		_animationHash[33] = Animator.StringToHash("BS060@skill_06_step2_start");
		_animationHash[34] = Animator.StringToHash("BS060@skill_06_step2_loop");
		_animationHash[35] = Animator.StringToHash("BS060@skill_06_step3_start");
		_animationHash[36] = Animator.StringToHash("BS060@skill_06_step3_loop");
		_animationHash[37] = Animator.StringToHash("BS060@skill_06_step3_end");
		_animationHash[38] = Animator.StringToHash("BS060@skill_07_start");
		_animationHash[39] = Animator.StringToHash("BS060@skill_07_loop");
		_animationHash[40] = Animator.StringToHash("BS060@skill_07_end");
		_animationHash[41] = Animator.StringToHash("BS060@skill_08_start");
		_animationHash[42] = Animator.StringToHash("BS060@skill_08_loop");
		_animationHash[43] = Animator.StringToHash("BS060@skill_08_end");
		_animationHash[44] = Animator.StringToHash("BS060@hurt_loop");
		_animationHash[45] = Animator.StringToHash("BS060@dead");
	}

	protected override void Awake()
	{
		base.Awake();
		Transform[] target = _transform.GetComponentsInChildren<Transform>(true);
		ModelTransform = OrangeBattleUtility.FindChildRecursive(ref target, "model", true);
		_animator = ModelTransform.GetComponent<Animator>();
		_collideBullet = OrangeBattleUtility.FindChildRecursive(ref target, "BodyCollider").gameObject.AddOrGetComponent<CollideBullet>();
		Gun = OrangeBattleUtility.FindChildRecursive(ref target, "GunMesh", true).gameObject;
		ShootPoint = OrangeBattleUtility.FindChildRecursive(ref target, "L WeaponPoint", true);
		SwordLMain = OrangeBattleUtility.FindChildRecursive(ref target, "SaberMesh_L_main", true).gameObject;
		SwordLSub = OrangeBattleUtility.FindChildRecursive(ref target, "SaberMesh_L_sub", true).gameObject;
		SwordRMain = OrangeBattleUtility.FindChildRecursive(ref target, "SaberMesh_R_main", true).gameObject;
		SwordRSub = OrangeBattleUtility.FindChildRecursive(ref target, "SaberMesh_R_sub", true).gameObject;
		SwordRCollide = OrangeBattleUtility.FindChildRecursive(ref target, "SaberCenterPoint_R").gameObject.AddOrGetComponent<CollideBullet>();
		SwordLCollide = OrangeBattleUtility.FindChildRecursive(ref target, "SaberCenterPoint_L").gameObject.AddOrGetComponent<CollideBullet>();
		Skill8Collide = OrangeBattleUtility.FindChildRecursive(ref target, "Skill8Collide").gameObject.AddOrGetComponent<CollideBullet>();
		PredictSpriteRenderer.enabled = false;
		_animationHash = new int[46];
		HashAnimation();
		base.AimPoint = new Vector3(0f, 1.2f, 0f);
		OrangeBattleUtility.AddEnemyAutoAimSystem(_transform, out _enemyAutoAimSystem);
		CloseAllFX();
		_enemyAutoAimSystem.UpdateAimRange(20f);
		FallDownSE = new string[2] { "BossSE02", "bs020_omgzero15" };
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
			UpdateDirection();
		}
		SetStatus((MainStatus)nSet);
	}

	private void UpdateDirection(int forceDirection = 0)
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
		ModelTransform.localScale = new Vector3(ModelTransform.localScale.x, ModelTransform.localScale.y, base.direction);
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
				SetGun();
				SetLSword();
				SetRSword();
				break;
			case SubStatus.Phase1:
				PlayBossSE("BossSE02", "bs020_omgzero01_lg");
				_velocity.y = -3000;
				break;
			}
			break;
		case MainStatus.Idle:
			UpdateDirection();
			_velocity.x = 0;
			AiTimer.TimerStart();
			break;
		case MainStatus.Skill0:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				ChargeFX[0].Play();
				SetGun(true);
				break;
			case SubStatus.Phase1:
				ShootTime = 2;
				NextFrame = 0f;
				break;
			case SubStatus.Phase2:
				SetGun();
				break;
			}
			break;
		case MainStatus.Skill1:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				ChargeFX[0].Stop();
				SetSwordCollide(3, ref SwordRCollide);
				SwordRCollide.Active(targetMask);
				_collideBullet.BackToPool();
				if (UseSK7)
				{
					if (SK7Phase == 2)
					{
						SK7Phase = 3;
					}
					else if (SK7Phase == 5)
					{
						SK7Phase = 6;
					}
				}
				else
				{
					ShootTime = 1;
					NextFrame = 0.4f;
					ChargeFX[1].Play();
				}
				break;
			}
			break;
		case MainStatus.Skill2:
		case MainStatus.Skill8:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (_mainStatus == MainStatus.Skill2)
				{
					ShowFX(2);
				}
				else if (_mainStatus == MainStatus.Skill8)
				{
					ShowFX(0);
				}
				break;
			case SubStatus.Phase1:
				if (_mainStatus == MainStatus.Skill2)
				{
					ShowFX(10);
				}
				else if (_mainStatus == MainStatus.Skill8)
				{
					ShowFX(9);
				}
				break;
			case SubStatus.Phase2:
				if (_mainStatus == MainStatus.Skill2)
				{
					StopFx(10);
					ShowFX(2);
				}
				else if (_mainStatus == MainStatus.Skill8)
				{
					StopFx(9);
					ShowFX(11);
				}
				break;
			case SubStatus.Phase3:
				ShootTime = 1;
				if (_mainStatus == MainStatus.Skill2)
				{
					NextFrame = 0.4f;
				}
				else if (_mainStatus == MainStatus.Skill8)
				{
					NextFrame = 0f;
				}
				break;
			}
			break;
		case MainStatus.Skill3:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				SetRSword(true);
				_velocity = SK3Jump;
				_velocity.x *= base.direction;
				break;
			case SubStatus.Phase2:
				IgnoreGravity = true;
				_velocity.x = 0;
				break;
			case SubStatus.Phase3:
				ShowFX(3);
				SetSwordCollide(6, ref SwordLCollide);
				SwordLCollide.Active(targetMask);
				_collideBullet.BackToPool();
				ShootTime = 1;
				NextFrame = 0f;
				break;
			case SubStatus.Phase4:
				SetRSword();
				IgnoreGravity = false;
				_collideBullet.Active(targetMask);
				SwordLCollide.BackToPool();
				break;
			case SubStatus.Phase6:
				_velocity = VInt3.zero;
				break;
			}
			break;
		case MainStatus.Skill4:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if ((bool)Target)
				{
					TargetPos = new VInt3(Target.transform.position);
					UpdateDirection();
					if (Math.Abs(TargetPos.x - Controller.LogicPosition.x) < 2000)
					{
						SetStatus(MainStatus.Skill4, SubStatus.Phase2);
						break;
					}
					ShowFX(6);
					PlaySE("BossSE02", "bs020_omgzero10_lp");
					_velocity.x = DashSpeed * base.direction;
				}
				else
				{
					SetStatus(MainStatus.Skill4, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase1:
				ShowFX(12);
				break;
			case SubStatus.Phase2:
				UpdateDirection();
				StopFx(12);
				ShowFX(4);
				PlaySE("BossSE02", "bs020_omgzero10_stop");
				_velocity.x = 0;
				SetRSword(true);
				SetSwordCollide(7, ref SwordLCollide);
				SwordLCollide.Active(targetMask);
				_collideBullet.BackToPool();
				if (UseSK7)
				{
					SK7Phase = 1;
				}
				break;
			case SubStatus.Phase3:
				SwordLCollide.BackToPool();
				break;
			case SubStatus.Phase4:
				ShowFX(5);
				SetSwordCollide(8, ref SwordLCollide);
				SwordLCollide.Active(targetMask);
				_collideBullet.BackToPool();
				if (UseSK7)
				{
					SetRSword(true);
					if (SK7Phase == 1)
					{
						SK7Phase = 2;
					}
					else if (SK7Phase == 3)
					{
						SK7Phase = 4;
					}
				}
				break;
			case SubStatus.Phase5:
				SwordLCollide.BackToPool();
				break;
			case SubStatus.Phase6:
				SetRSword();
				break;
			case SubStatus.Phase9:
				ShowFX(8);
				SetRSword(true);
				SetSwordCollide(9, ref SwordLCollide);
				SwordLCollide.Active(targetMask);
				_collideBullet.BackToPool();
				_hasShake = false;
				if (UseSK7)
				{
					SK7Phase = 5;
				}
				break;
			case SubStatus.Phase10:
				if (!UseSK7)
				{
					RockFall = true;
				}
				RockShootTime = 1;
				NextCloseFrame = GameLogicUpdateManager.GameFrame + 20;
				NextRockFrame = GameLogicUpdateManager.GameFrame;
				SetRSword();
				_collideBullet.Active(targetMask);
				SwordLCollide.BackToPool();
				break;
			}
			break;
		case MainStatus.Skill5:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if ((bool)Target)
				{
					TargetPos = new VInt3(Target.transform.position);
					UpdateDirection();
					if ((float)Math.Abs(TargetPos.x - Controller.LogicPosition.x) < 2000f)
					{
						SetStatus(MainStatus.Skill5, SubStatus.Phase2);
						break;
					}
					ShowFX(6);
					PlaySE("BossSE02", "bs020_omgzero10_lp");
					_velocity.x = DashSpeed * base.direction;
				}
				else
				{
					SetStatus(MainStatus.Skill5, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase1:
				ShowFX(12);
				break;
			case SubStatus.Phase2:
				ShowFX(7);
				StopFx(12);
				ShowFX(13);
				PlaySE("BossSE02", "bs020_omgzero10_stop");
				PlaySE("BossSE02", "bs020_omgzero13");
				SetSwordCollide(11, ref SwordRCollide);
				SwordRCollide.Active(targetMask);
				SwordRCollide.HitCallback = StunHitCallBack;
				_collideBullet.BackToPool();
				SetRSword(true);
				_velocity = SK5Jump;
				_velocity.x *= base.direction;
				if (UseSK7 && SK7Phase == 6)
				{
					SK7Phase = 7;
					_animator.speed = 1f;
					_velocity.y = SK7Phase7Jump;
					if (FinalJump)
					{
						_velocity.x = SK7Phase7JumpX * base.direction;
					}
					else
					{
						_velocity.x = 0;
					}
				}
				break;
			case SubStatus.Phase4:
				StopFx(13);
				_collideBullet.Active(targetMask);
				SwordRCollide.BackToPool();
				SwordRCollide.HitCallback = null;
				SetRSword();
				break;
			case SubStatus.Phase6:
				if ((bool)targetOC && !UseSK7)
				{
					targetOC.SetStun(false);
					targetOC = null;
					IsHitPlayer = false;
					_isCatching = false;
				}
				_velocity = VInt3.zero;
				break;
			}
			break;
		case MainStatus.Skill6:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				SetRSword(true);
				break;
			case SubStatus.Phase2:
				SetSwordCollide(9, ref SwordRCollide);
				SwordRCollide.Active(targetMask);
				_collideBullet.BackToPool();
				break;
			}
			break;
		case MainStatus.Skill7:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				UseSK7 = true;
				IsNewGravity = false;
				SK7Phase = 0;
				_animator.speed = 2f;
				SwordRCollide.HitCallback = StunHitCallBack;
				SwordLCollide.HitCallback = StunHitCallBack;
				if ((bool)Target)
				{
					TargetPos = new VInt3(Target.transform.position);
					UpdateDirection();
					int num = Math.Abs(TargetPos.x - Controller.LogicPosition.x);
					int num2 = OrangeBattleUtility.FP_Gravity * GameLogicUpdateManager.g_fixFrameLenFP / 1000;
					if (num < 8000)
					{
						int num3 = -(10500 * 2) / num2;
						int num4 = num * 20 / num3;
						SK7Jump.x = num4 * base.direction;
						_velocity = SK7Jump;
					}
					else
					{
						int num5 = 12000;
						int num6 = num * 20 / num5;
						int num7 = -21000 / num2;
						float num8 = (float)num6 / (float)num7;
						int num9 = (int)(10500f / num8);
						GravityNew = -num2 - (int)((float)(-num2) / (num8 * num8));
						IsNewGravity = true;
						SK7Jump.y = num9 + GravityNew;
						SK7Jump.x = 12000 * base.direction;
						_velocity = SK7Jump;
					}
					PlaySE("BossSE02", "bs020_omgzero11");
				}
				else
				{
					SetStatus(MainStatus.Skill4, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase1:
				ShowFX(14);
				break;
			case SubStatus.Phase2:
				StopFx(14);
				_velocity.x = 0;
				if ((bool)Target)
				{
					TargetPos = new VInt3(Target.transform.position);
					UpdateDirection();
				}
				break;
			case SubStatus.Phase10:
				UseSK7 = false;
				IsNewGravity = false;
				SK7Phase = 0;
				_animator.speed = 1f;
				SwordRCollide.HitCallback = null;
				SwordLCollide.HitCallback = null;
				_collideBullet.Active(targetMask);
				if ((bool)targetOC)
				{
					targetOC.SetStun(false);
					targetOC = null;
					IsHitPlayer = false;
					AllowAction = 10;
				}
				SetStatus(MainStatus.Idle);
				break;
			}
			break;
		case MainStatus.Die:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				base.AllowAutoAim = false;
				_velocity.x = 0;
				if (Controller.Collisions.below)
				{
					SetStatus(MainStatus.Die, SubStatus.Phase2);
				}
				else
				{
					SetStatus(MainStatus.Die, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				IgnoreGravity = true;
				_bDeadPlayCompleted = true;
				break;
			case SubStatus.Phase2:
				_velocity = VInt3.zero;
				_bDeadPlayCompleted = true;
				break;
			case SubStatus.Phase3:
			{
				AI_STATE aiState = AiState;
				if (aiState == AI_STATE.mob_002)
				{
					StartCoroutine(BossDieFlow(GetTargetPoint(), "FX_BOSS_EXPLODE2", false, false));
				}
				else
				{
					StartCoroutine(BossDieFlow(GetTargetPoint()));
				}
				break;
			}
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
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_DEBUT_LOOP;
				break;
			case SubStatus.Phase1:
				return;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_DEBUT;
				break;
			case SubStatus.Phase3:
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
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_SKILL0_START;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_SKILL0_LOOP;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_SKILL0_END;
				break;
			}
			break;
		case MainStatus.Skill1:
			switch (_subStatus)
			{
			default:
				return;
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_SKILL1_START;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_SKILL1_LOOP;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_SKILL1_END;
				break;
			}
			break;
		case MainStatus.Skill2:
		case MainStatus.Skill8:
			switch (_subStatus)
			{
			default:
				return;
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_SKILL2_START1;
				break;
			case SubStatus.Phase1:
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
			case SubStatus.Phase2:
				return;
			}
			break;
		case MainStatus.Skill3:
			switch (_subStatus)
			{
			default:
				return;
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_SKILL3_START1;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_SKILL3_LOOP1;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_SKILL3_START2;
				break;
			case SubStatus.Phase3:
				_currentAnimationId = AnimationID.ANI_SKILL3_LOOP2;
				break;
			case SubStatus.Phase4:
				_currentAnimationId = AnimationID.ANI_SKILL3_START3;
				break;
			case SubStatus.Phase5:
				_currentAnimationId = AnimationID.ANI_SKILL3_LOOP3;
				break;
			case SubStatus.Phase6:
				_currentAnimationId = AnimationID.ANI_SKILL3_END3;
				break;
			}
			break;
		case MainStatus.Skill4:
			switch (_subStatus)
			{
			default:
				return;
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_SKILL5_START1;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_SKILL5_LOOP1;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_SKILL4_START1;
				break;
			case SubStatus.Phase3:
				_currentAnimationId = AnimationID.ANI_SKILL4_LOOP1;
				break;
			case SubStatus.Phase4:
				_currentAnimationId = AnimationID.ANI_SKILL4_START2;
				break;
			case SubStatus.Phase5:
				_currentAnimationId = AnimationID.ANI_SKILL4_LOOP2;
				break;
			case SubStatus.Phase6:
				_currentAnimationId = AnimationID.ANI_SKILL4_END2;
				break;
			case SubStatus.Phase7:
				_currentAnimationId = AnimationID.ANI_SKILL4_START3;
				break;
			case SubStatus.Phase8:
				_currentAnimationId = AnimationID.ANI_SKILL4_LOOP3;
				break;
			case SubStatus.Phase9:
				_currentAnimationId = AnimationID.ANI_SKILL4_START4;
				break;
			case SubStatus.Phase10:
				_currentAnimationId = AnimationID.ANI_SKILL4_LOOP4;
				break;
			case SubStatus.Phase11:
				_currentAnimationId = AnimationID.ANI_SKILL4_END4;
				break;
			}
			break;
		case MainStatus.Skill5:
			switch (_subStatus)
			{
			default:
				return;
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_SKILL5_START1;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_SKILL5_LOOP1;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_SKILL5_START2;
				break;
			case SubStatus.Phase3:
				_currentAnimationId = AnimationID.ANI_SKILL5_LOOP2;
				break;
			case SubStatus.Phase4:
				_currentAnimationId = AnimationID.ANI_SKILL5_START3;
				break;
			case SubStatus.Phase5:
				_currentAnimationId = AnimationID.ANI_SKILL5_LOOP3;
				break;
			case SubStatus.Phase6:
				_currentAnimationId = AnimationID.ANI_SKILL5_END3;
				break;
			}
			break;
		case MainStatus.Skill6:
			switch (_subStatus)
			{
			default:
				return;
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_SKILL6_START;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_SKILL6_LOOP;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_SKILL6_END;
				break;
			}
			break;
		case MainStatus.Skill7:
			switch (_subStatus)
			{
			default:
				return;
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_SKILL7_START;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_SKILL7_LOOP;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_SKILL7_END;
				break;
			}
			break;
		case MainStatus.Die:
			switch (_subStatus)
			{
			default:
				return;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_HURT;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_DEAD;
				break;
			}
			break;
		}
		_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
	}

	private void UpdateRandomState(MainStatus value = MainStatus.Idle)
	{
		MainStatus mainStatus = value;
		if (mainStatus == MainStatus.Idle)
		{
			switch (_mainStatus)
			{
			case MainStatus.Debut:
				SetStatus(MainStatus.Idle);
				break;
			case MainStatus.Idle:
				if (TargetPos.y > Controller.LogicPosition.y + 2500)
				{
					WallATK = true;
					mainStatus = ((Math.Abs(TargetPos.x - Controller.LogicPosition.x) >= 500) ? ((MainStatus)RandomCard(2)) : MainStatus.Skill8);
				}
				else
				{
					WallATK = false;
					RangedATK = Math.Abs(Target.transform.position.x - _transform.position.x) > 6f;
					mainStatus = (MainStatus)RandomCard(2);
				}
				break;
			}
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
				if (_introReady)
				{
					SetStatus(MainStatus.Debut, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if (Controller.Collisions.below)
				{
					PlaySE("BossSE02", "bs020_omgzero01_stop");
					PlaySE("BossSE02", "bs020_omgzero02");
					PlaySE("BossSE02", "bs020_omgzero15");
					IgnoreGravity = false;
					SetStatus(MainStatus.Debut, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Debut, SubStatus.Phase3);
				}
				break;
			case SubStatus.Phase3:
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
			if (AiTimer.GetMillisecond() <= 100 * AllowAction || bWaitNetStatus)
			{
				break;
			}
			AllowAction = 1;
			Target = _enemyAutoAimSystem.GetClosetPlayer();
			if ((bool)Target)
			{
				TargetPos = Target.Controller.LogicPosition;
				if ((int)Hp < HpPhase1 && !HpPhase1Used)
				{
					HpPhase1Used = true;
					UseSK7 = true;
				}
				else if ((int)Hp < HpPhase2 && !HpPhase2Used)
				{
					HpPhase2Used = true;
					UseSK7 = true;
				}
				else
				{
					if ((int)Hp >= HpPhase3 || HpPhase3Used)
					{
						UpdateRandomState();
						break;
					}
					HpPhase3Used = true;
					UseSK7 = true;
				}
				if (UseSK7)
				{
					if (TargetPos.y > Controller.LogicPosition.y + 2500)
					{
						UpdateRandomState(MainStatus.Skill5);
					}
					else
					{
						UpdateRandomState(MainStatus.Skill7);
					}
				}
			}
			else if (CheckHost())
			{
				UploadEnemyStatus(5);
			}
			break;
		case MainStatus.Skill0:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (_currentFrame > 1.5f)
				{
					SetStatus(MainStatus.Skill0, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if (ShootTime == 0 && _currentFrame > 2f)
				{
					SetStatus(MainStatus.Skill0, SubStatus.Phase2);
				}
				else if (ShootTime > 0 && _currentFrame > NextFrame)
				{
					BulletBase.TryShotBullet(EnemyWeapons[1].BulletData, ShootPoint, Vector3.right * base.direction, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
					ShootTime--;
					NextFrame += 1f;
				}
				break;
			case SubStatus.Phase2:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill1);
				}
				break;
			}
			break;
		case MainStatus.Skill1:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (_currentFrame > NextFrame && ShootTime > 0 && !UseSK7)
				{
					ChargeFX[1].Stop();
					ShootTime--;
					Vector3 worldPos3 = new Vector3(1.2f * (float)base.direction, 0.8f, 0f) + _transform.position;
					BulletBase.TryShotBullet(EnemyWeapons[2].BulletData, worldPos3, Vector3.right * base.direction, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
				}
				else if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill1, SubStatus.Phase1);
				}
				if (!UsingSword && _currentFrame > 0.28f)
				{
					UsingSword = SetLSword(true);
				}
				break;
			case SubStatus.Phase1:
				if (_currentFrame > 0.1f)
				{
					_collideBullet.Active(targetMask);
					SwordRCollide.BackToPool();
					SetStatus(MainStatus.Skill1, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				if (!(_currentFrame > 1f))
				{
					break;
				}
				UsingSword = SetLSword();
				if (UseSK7)
				{
					if (SK7Phase == 3)
					{
						SetStatus(MainStatus.Skill4, SubStatus.Phase4);
					}
					else if (SK7Phase == 6)
					{
						SetStatus(MainStatus.Skill5, SubStatus.Phase2);
					}
				}
				else
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
				if (_currentFrame > 1.1f)
				{
					SetStatus(MainStatus.Skill2, SubStatus.Phase3);
				}
				break;
			case SubStatus.Phase3:
				if (_currentFrame > NextFrame && ShootTime > 0)
				{
					ShootTime--;
					float num = 180 / (ShootNum - 1);
					for (int j = 0; j < ShootNum; j++)
					{
						Vector3 vector2 = Quaternion.Euler(0f, 0f, num * (float)j) * Vector3.right;
						Vector3 worldPos2 = vector2 * 0.5f + Vector3.up * 0.5f + _transform.position;
						BulletBase.TryShotBullet(EnemyWeapons[5].BulletData, worldPos2, vector2, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
					}
				}
				else if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill2, SubStatus.Phase4);
				}
				break;
			case SubStatus.Phase4:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill2, SubStatus.Phase5);
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
		case MainStatus.Skill8:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill8, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill8, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				if (_currentFrame > 1.1f)
				{
					SetStatus(MainStatus.Skill8, SubStatus.Phase3);
				}
				break;
			case SubStatus.Phase3:
				if (ShootTime > 0)
				{
					ShootTime--;
					ShowFX(1);
					Skill8Collide.Active(targetMask);
				}
				else if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill8, SubStatus.Phase4);
				}
				break;
			case SubStatus.Phase4:
				if (_currentFrame > 1.5f)
				{
					Skill8Collide.BackToPool();
					SetStatus(MainStatus.Skill8, SubStatus.Phase5);
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
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill3, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if (_velocity.y <= 2000)
				{
					SetStatus(MainStatus.Skill3, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill3, SubStatus.Phase3);
				}
				break;
			case SubStatus.Phase3:
				if (ShootTime == 0 && _currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill3, SubStatus.Phase4);
				}
				else if (ShootTime > 0 && _currentFrame > NextFrame)
				{
					ShootTime--;
					NextFrame += 1f;
					Vector3 vector4 = new Vector3(0f, 0.5f, 0f) + _transform.position;
					for (int i = 0; i < 8; i++)
					{
						Vector3 vector = Quaternion.Euler(0f, 0f, (float)(i * 45) + 22.5f) * Vector3.up;
						Vector3 worldPos = new Vector3(0f, 0.5f, 0f) + vector * 0.5f + _transform.position;
						BulletBase.TryShotBullet(EnemyWeapons[2].BulletData, worldPos, vector, null, selfBuffManager.sBuffStatus, EnemyData, targetMask)._transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
					}
				}
				break;
			case SubStatus.Phase4:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill3, SubStatus.Phase5);
				}
				break;
			case SubStatus.Phase5:
				if (Controller.Collisions.below)
				{
					SetStatus(MainStatus.Skill3, SubStatus.Phase6);
				}
				break;
			case SubStatus.Phase6:
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
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill4, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if ((Controller.LogicPosition.x - (TargetPos.x - 1500 * base.direction)) * base.direction > 0)
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
				if (_currentFrame > 0.1f)
				{
					SetStatus(MainStatus.Skill4, SubStatus.Phase4);
				}
				break;
			case SubStatus.Phase4:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill4, SubStatus.Phase5);
				}
				break;
			case SubStatus.Phase5:
				if (_currentFrame > 0.1f)
				{
					SetStatus(MainStatus.Skill4, SubStatus.Phase6);
				}
				break;
			case SubStatus.Phase6:
				if (_currentFrame > 1f)
				{
					if (UseSK7 && SK7Phase == 2)
					{
						SetStatus(MainStatus.Skill1);
					}
					else
					{
						SetStatus(MainStatus.Skill4, SubStatus.Phase7);
					}
				}
				break;
			case SubStatus.Phase7:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill4, SubStatus.Phase8);
				}
				break;
			case SubStatus.Phase8:
				if (_currentFrame > 0.1f)
				{
					SetStatus(MainStatus.Skill4, SubStatus.Phase9);
				}
				break;
			case SubStatus.Phase9:
				if (_currentFrame > 0.75f && !_hasShake)
				{
					_hasShake = true;
					Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.CAMERA_SHAKE, 2f, false);
				}
				else if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill4, SubStatus.Phase10);
				}
				break;
			case SubStatus.Phase10:
				if (_currentFrame > 0.1f)
				{
					SetStatus(MainStatus.Skill4, SubStatus.Phase11);
				}
				break;
			case SubStatus.Phase11:
				if (_currentFrame > 1f)
				{
					if (UseSK7)
					{
						SetStatus(MainStatus.Skill1);
					}
					else
					{
						SetStatus(MainStatus.Idle);
					}
				}
				break;
			}
			break;
		case MainStatus.Skill5:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill5, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if ((Controller.LogicPosition.x - (TargetPos.x - 1500 * base.direction)) * base.direction > 0)
				{
					SetStatus(MainStatus.Skill5, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill5, SubStatus.Phase3);
				}
				if (UseSK7 && SK7Phase == 7)
				{
					_velocity.y += DuringJump;
					if (FinalJump)
					{
						_velocity.x += DuringJumpX * base.direction;
					}
					if ((bool)targetOC)
					{
						_isCatching = true;
					}
				}
				break;
			case SubStatus.Phase3:
				if (_velocity.y <= 0)
				{
					if ((bool)targetOC && UseSK7 && SK7Phase == 7)
					{
						_isCatching = false;
						PlayerGravity = _velocity.y;
					}
					SetStatus(MainStatus.Skill5, SubStatus.Phase4);
				}
				if (UseSK7 && SK7Phase == 7 && _velocity.y > 0)
				{
					_velocity.y += DuringJump;
					if (FinalJump)
					{
						_velocity.x += DuringJumpX * base.direction;
					}
				}
				break;
			case SubStatus.Phase4:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill5, SubStatus.Phase5);
				}
				break;
			case SubStatus.Phase5:
				if (Controller.Collisions.below)
				{
					SetStatus(MainStatus.Skill5, SubStatus.Phase6);
				}
				if (UseSK7 && SK7Phase == 7)
				{
					_velocity.y += DuringJump;
					if (FinalJump)
					{
						_velocity.x += DuringJumpX * base.direction;
					}
				}
				break;
			case SubStatus.Phase6:
				if (!(_currentFrame > 1f))
				{
					break;
				}
				if (UseSK7)
				{
					if (SK7Phase == 7)
					{
						SetStatus(MainStatus.Skill7, SubStatus.Phase10);
					}
					else if (SK7Phase == 0)
					{
						_animator.speed = 2f;
						SwordRCollide.HitCallback = StunHitCallBack;
						SwordLCollide.HitCallback = StunHitCallBack;
						SetStatus(MainStatus.Skill7, SubStatus.Phase1);
					}
				}
				else
				{
					SetStatus(MainStatus.Idle);
				}
				break;
			}
			break;
		case MainStatus.Skill6:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill6, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill6, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				if (_currentFrame > 1f)
				{
					_collideBullet.Active(targetMask);
					SwordRCollide.BackToPool();
					RockFall = true;
					RockShootTime = 3;
					NextCloseFrame = 0f;
					NextRockFrame = 15f;
					SetRSword();
					SetStatus(MainStatus.Idle);
				}
				break;
			}
			break;
		case MainStatus.Skill7:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (IsNewGravity)
				{
					_velocity.y += GravityNew;
				}
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill7, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
			{
				if (IsNewGravity)
				{
					_velocity.y += GravityNew;
				}
				Vector2 point = new Vector3(0.3f * (float)base.direction, 0f, 0f) + Controller.GetCenterPos();
				Vector2 size = Controller.Collider2D.size;
				Collider2D collider2D = Physics2D.OverlapBox(point, size, 0f, LayerMask.GetMask("Player"));
				if ((bool)collider2D)
				{
					_velocity = VInt3.zero;
					TargetPos = new VInt3(collider2D.transform.position);
					UpdateDirection();
					_collideBullet.BackToPool();
					StopFx(14);
					SetStatus(MainStatus.Skill4, SubStatus.Phase2);
				}
				else if (Controller.Collisions.below)
				{
					SetStatus(MainStatus.Skill7, SubStatus.Phase2);
				}
				break;
			}
			case SubStatus.Phase2:
				if (_currentFrame > 1f)
				{
					if (IsHitPlayer)
					{
						SetStatus(MainStatus.Skill4, SubStatus.Phase2);
					}
					else
					{
						SetStatus(MainStatus.Skill7, SubStatus.Phase10);
					}
				}
				break;
			}
			break;
		case MainStatus.Die:
			switch (_subStatus)
			{
			case SubStatus.Phase1:
				if (_currentFrame > 1f)
				{
					if (nDeadCount > 10)
					{
						SetStatus(MainStatus.Die, SubStatus.Phase3);
					}
					else
					{
						nDeadCount++;
					}
				}
				break;
			case SubStatus.Phase2:
				if (_currentFrame > 1f)
				{
					if (nDeadCount > 10)
					{
						SetStatus(MainStatus.Die, SubStatus.Phase3);
					}
					else
					{
						nDeadCount++;
					}
				}
				break;
			}
			break;
		}
		if (RockFall)
		{
			if ((float)GameLogicUpdateManager.GameFrame > NextCloseFrame)
			{
				RockTarget.SetParent(_transform);
				PredictSpriteRenderer.enabled = false;
				if (RockShootTime < 1)
				{
					RockFall = false;
				}
			}
			if ((float)GameLogicUpdateManager.GameFrame > NextRockFrame && RockShootTime > 0)
			{
				RockShootTime--;
				Target = _enemyAutoAimSystem.GetClosetPlayer();
				BulletBase bulletBase;
				if ((bool)Target)
				{
					Vector3 vector3 = new Vector3(base.direction, -5f);
					RockTarget.SetParentNull();
					RockTarget.position = Target._transform.position;
					if (Target.Controller.Collisions.below)
					{
						RockTarget.localRotation = Quaternion.Euler(90f, 90f, 0f);
					}
					else
					{
						RockTarget.localRotation = Quaternion.Euler(0f, 0f, 0f);
					}
					PredictSpriteRenderer.enabled = true;
					bulletBase = BulletBase.TryShotBullet(EnemyWeapons[10].BulletData, Target.transform.position - 4f * vector3, vector3, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
				}
				else
				{
					RockTarget.position = new Vector3(3 * base.direction, 0f, 0f) + _transform.position;
					RockTarget.localRotation = Quaternion.Euler(90f, 90f, 0f);
					PredictSpriteRenderer.enabled = true;
					bulletBase = BulletBase.TryShotBullet(EnemyWeapons[10].BulletData, new Vector3(4 * base.direction, 15f, 0f) + _transform.position, Vector3.down, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
				}
				if ((bool)bulletBase)
				{
					bulletBase.HitCallback = RockHitCallBack;
				}
			}
		}
		if (IsHitPlayer && (bool)targetOC && !targetOC.Controller.Collisions.below && !targetOC.IsJacking)
		{
			VInt vInt = OrangeBattleUtility.FP_Gravity * GameLogicUpdateManager.g_fixFrameLenFP / 1000;
			PlayerGravity += vInt * (VInt)1f / 1000;
			if (UseSK7 && SK7Phase == 7)
			{
				PlayerGravity += (int)((float)DuringJump * 0.5f);
			}
			targetOC.AddForce(new VInt3(0, PlayerGravity, 0));
		}
	}

	public void UpdateFunc()
	{
		if (!Activate && _mainStatus != MainStatus.Debut)
		{
			return;
		}
		if (UseSK7 && SK7Phase <= 7 && _isCatching && (bool)targetOC)
		{
			if ((int)targetOC.Hp > 0)
			{
				targetOC._transform.position = new Vector3(base.direction, 0.5f, 0f) + _transform.position;
				targetOC.Controller.LogicPosition = new VInt3(targetOC._transform.position);
			}
			else
			{
				_isCatching = false;
				targetOC = null;
				IsHitPlayer = false;
			}
		}
		base.transform.localPosition = Vector3.MoveTowards(base.transform.localPosition, Controller.LogicPosition.vec3, distanceDelta);
	}

	public override void SetActive(bool isActive)
	{
		base.SetActive(isActive);
		if (isActive)
		{
			HpPhase1 = (int)MaxHp / 10 * HpPhase1;
			HpPhase2 = (int)MaxHp / 10 * HpPhase2;
			HpPhase3 = (int)MaxHp / 10 * HpPhase3;
			SetMaxGravity(OrangeBattleUtility.FP_MaxGravity * 2);
			IgnoreGravity = true;
			_collideBullet.UpdateBulletData(EnemyWeapons[0].BulletData);
			_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			_collideBullet.Active(targetMask);
			Skill8Collide.UpdateBulletData(EnemyWeapons[4].BulletData);
			Skill8Collide.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			SwordRCollide.BackToPool();
			SwordLCollide.BackToPool();
			RangedSKC = new List<int>(DefaultRangedSkillCard);
			WallSKC = new List<int>(DefaultWallSkillCard);
			ShortSKC = new List<int>(DefaultShortSkillCard);
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
		base.transform.localScale = new Vector3(_transform.localScale.x, _transform.localScale.y, _transform.localScale.z);
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
		bool flag = false;
		if (RangedATK)
		{
			if (RangedSKC.ToArray().Length < 1)
			{
				RangedSKC = new List<int>(DefaultRangedSkillCard);
				flag = true;
			}
			if (!flag)
			{
				int num = RangedSKC[OrangeBattleUtility.Random(0, RangedSKC.ToArray().Length)];
				RangedSKC.Remove(num);
				return num + StartPos;
			}
			RangedATK = false;
			return RandomCard(2);
		}
		if (WallATK)
		{
			if (WallSKC.ToArray().Length < 1)
			{
				WallSKC = new List<int>(DefaultWallSkillCard);
			}
			int num2 = WallSKC[OrangeBattleUtility.Random(0, WallSKC.ToArray().Length)];
			WallSKC.Remove(num2);
			return num2 + StartPos;
		}
		if (ShortSKC.ToArray().Length < 1)
		{
			ShortSKC = new List<int>(DefaultShortSkillCard);
		}
		int num3 = ShortSKC[OrangeBattleUtility.Random(0, ShortSKC.ToArray().Length)];
		ShortSKC.Remove(num3);
		return num3 + StartPos;
	}

	protected override void DeadBehavior(ref HurtPassParam tHurtPassParam)
	{
		if (_mainStatus != MainStatus.Die)
		{
			_animator.speed = 1f;
			if ((bool)_collideBullet)
			{
				SwordLCollide.BackToPool();
				SwordRCollide.BackToPool();
				Skill8Collide.BackToPool();
				_collideBullet.BackToPool();
			}
			RockFall = false;
			StageUpdate.SlowStage();
			SetColliderEnable(false);
			ReleaseOC();
			_isCatching = (IsHitPlayer = false);
			targetOC = null;
			CloseAllFX();
			SetStatus(MainStatus.Die);
		}
	}

	private bool SetGun(bool active = false)
	{
		Gun.SetActive(active);
		return active;
	}

	private bool SetLSword(bool active = false)
	{
		SwordLMain.SetActive(active);
		SwordLSub.SetActive(active);
		return active;
	}

	private bool SetRSword(bool active = false)
	{
		SwordRMain.SetActive(active);
		SwordRSub.SetActive(active);
		return active;
	}

	private void SetSwordCollide(int UseWeapon, ref CollideBullet sword)
	{
		if (!UseSK7)
		{
			sword.UpdateBulletData(EnemyWeapons[UseWeapon].BulletData);
		}
		else
		{
			SKILL_TABLE bulletData = EnemyWeapons[UseWeapon].BulletData;
			bulletData.f_EFFECT_X /= 2f;
			sword.UpdateBulletData(bulletData);
			bulletData.f_EFFECT_X *= 2f;
		}
		sword.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
	}

	private void StunHitCallBack(object obj)
	{
		if (obj == null)
		{
			return;
		}
		if (targetOC != null)
		{
			if (UseSK7 && SK7Phase > 0)
			{
				RaycastHit2D raycastHit2D = OrangeBattleUtility.RaycastIgnoreSelf(Controller.GetRealCenterPos(), Vector2.right * base.direction, 5f, Controller.collisionMask, _transform);
				float num = Math.Abs(_transform.position.x - targetOC.transform.position.x);
				if ((bool)raycastHit2D && num < 2.5f)
				{
					FinalJump = false;
					_transform.position += new Vector3(0.25f * (float)(-base.direction), 0f, 0f);
					Controller.LogicPosition.x += 250 * -base.direction;
				}
				else if (num < 2.5f)
				{
					FinalJump = true;
					targetOC.transform.position += new Vector3(0.25f * (float)base.direction, 0f, 0f);
					targetOC.Controller.LogicPosition.x += 250 * base.direction;
				}
			}
			return;
		}
		if (SK7Phase < NoCatckSK7Phase)
		{
			Collider2D collider2D = obj as Collider2D;
			if (collider2D != null)
			{
				targetOC = OrangeBattleUtility.GetHitTargetOrangeCharacter(collider2D);
				if (targetOC != null)
				{
					targetOC.direction = base.direction * -1;
				}
			}
		}
		if (targetOC != null && (int)targetOC.Hp > 0 && (int)Hp > 0)
		{
			if ((bool)targetOC.IsUnBreakX())
			{
				targetOC = null;
				return;
			}
			PlayerGravity = 0;
			IsHitPlayer = true;
			_isCatching = true;
			targetOC.SetStun(true);
		}
		else if ((targetOC != null && (int)targetOC.Hp < 0) || (int)Hp < 0)
		{
			IsHitPlayer = false;
			targetOC.SetStun(false);
			if (targetOC.IsJacking)
			{
				ReleaseOC();
				targetOC = null;
			}
		}
	}

	private void ShowFX(int Fx)
	{
		SkillFX[Fx].transform.localEulerAngles = new Vector3(SkillFX[Fx].transform.eulerAngles.x, 90 * base.direction, SkillFX[Fx].transform.eulerAngles.z);
		SkillFX[Fx].Play();
	}

	private void StopFx(int Fx)
	{
		SkillFX[Fx].Stop();
	}

	private void RockHitCallBack(object param)
	{
		Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.CAMERA_SHAKE, 2f, false);
	}

	private void ReleaseOC()
	{
		if (!targetOC)
		{
			return;
		}
		targetOC.SetStun(false);
		if (targetOC.IsJacking)
		{
			if (targetOC.transform.parent != null)
			{
				targetOC._transform.SetParentNull();
			}
			targetOC._transform.position = new Vector3(targetOC._transform.position.x, targetOC._transform.position.y, 0f);
			targetOC._transform.rotation = Quaternion.Euler(Vector3.zero);
		}
	}

	private void CloseAllFX()
	{
		for (int i = 0; i < SkillFX.Length; i++)
		{
			SkillFX[i].Stop();
		}
		for (int j = 0; j < ChargeFX.Length; j++)
		{
			ChargeFX[j].Stop();
		}
	}

	public override void UpdateEnemyID(int id)
	{
		base.UpdateEnemyID(id);
		UpdateAIState();
	}
}
