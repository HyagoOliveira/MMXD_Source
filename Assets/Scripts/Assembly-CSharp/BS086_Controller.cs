#define RELEASE
using System;
using System.Collections.Generic;
using CallbackDefs;
using NaughtyAttributes;
using Newtonsoft.Json;
using StageLib;
using UnityEngine;

public class BS086_Controller : EnemyControllerBase, IManagedUpdateBehavior
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
		Skill9 = 11,
		Die = 12
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
		Phase15 = 15,
		Phase16 = 16,
		Phase17 = 17,
		Phase18 = 18,
		Phase19 = 19,
		Phase20 = 20,
		Phase21 = 21,
		MAX_SUBSTATUS = 22
	}

	public enum AnimationID
	{
		ANI_IDLE = 0,
		ANI_DEBUT1 = 1,
		ANI_DEBUT2 = 2,
		ANI_DEBUT3 = 3,
		ANI_DEBUT4 = 4,
		ANI_SKILL0_START1 = 5,
		ANI_SKILL0_LOOP1 = 6,
		ANI_SKILL0_START2 = 7,
		ANI_SKILL0_LOOP2 = 8,
		ANI_SKILL0_START3 = 9,
		ANI_SKILL0_END3 = 10,
		ANI_SKILL1_START1 = 11,
		ANI_SKILL1_LOOP1 = 12,
		ANI_SKILL1_START2 = 13,
		ANI_SKILL1_END2 = 14,
		ANI_SKILL2_START = 15,
		ANI_SKILL2_LOOP = 16,
		ANI_SKILL2_END = 17,
		ANI_SKILL3_START = 18,
		ANI_SKILL3_LOOP = 19,
		ANI_SKILL3_END = 20,
		ANI_SKILL4_START1 = 21,
		ANI_SKILL4_LOOP1 = 22,
		ANI_SKILL4_START2 = 23,
		ANI_SKILL4_LOOP2 = 24,
		ANI_SKILL4_END = 25,
		ANI_SKILL5_START = 26,
		ANI_SKILL5_LOOP = 27,
		ANI_SKILL5_END = 28,
		ANI_SKILL6_START = 29,
		ANI_SKILL6_LOOP = 30,
		ANI_SKILL6_END = 31,
		ANI_SKILL7_START = 32,
		ANI_SKILL7_LOOP = 33,
		ANI_SKILL7_END = 34,
		ANI_SKILL8_START1 = 35,
		ANI_SKILL8_LOOP1 = 36,
		ANI_SKILL8_START2 = 37,
		ANI_SKILL8_LOOP2 = 38,
		ANI_SKILL8_START3 = 39,
		ANI_SKILL8_LOOP3 = 40,
		ANI_SKILL8_END3 = 41,
		ANI_SKILL9_START1 = 42,
		ANI_SKILL9_LOOP1 = 43,
		ANI_SKILL9_END1 = 44,
		ANI_SKILL9_START2 = 45,
		ANI_SKILL9_LOOP2 = 46,
		ANI_SKILL9_START3 = 47,
		ANI_SKILL9_LOOP3 = 48,
		ANI_SKILL9_END3 = 49,
		ANI_SKILL9_START4 = 50,
		ANI_SKILL9_LOOP4 = 51,
		ANI_SKILL9_START5 = 52,
		ANI_SKILL9_LOOP5 = 53,
		ANI_SKILL9_END5 = 54,
		ANI_SKILL9_START6 = 55,
		ANI_SKILL9_LOOP6 = 56,
		ANI_SKILL9_END6 = 57,
		ANI_SKILL9_START7 = 58,
		ANI_SKILL9_LOOP7 = 59,
		ANI_SKILL9_START8 = 60,
		ANI_SKILL9_LOOP8 = 61,
		ANI_SKILL9_END8 = 62,
		ANI_HURT = 63,
		ANI_DEAD = 64,
		ANI_LEAVE1 = 65,
		ANI_LEAVE2 = 66,
		MAX_ANIMATION_ID = 67
	}

	private enum UseSkill
	{
		BodyCollide = 0,
		Sk0Bullet1 = 1,
		Sk0Bullet2 = 2,
		Sk1Bullet = 3,
		Sk2Slash = 4,
		Sk3Bullet = 5,
		Sk4Bullet = 6,
		Sk5Bullet = 7,
		Sk6Bullet = 8,
		Sk4BulletEX = 9,
		Sk7Claw = 10,
		Sk8Slash = 11,
		Sk9Slash = 12
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

	private int[] DefaultSkillCard = new int[6] { 0, 1, 2, 3, 4, 5 };

	private static int[] DefaultRangedSkillCard = new int[3] { 3, 4, 5 };

	private List<int> RangedSKC = new List<int>();

	private List<int> SkillCard = new List<int>();

	private Vector3 LastTargetPos = new Vector3(0f, 0f, 0f);

	[Header("通用")]
	[SerializeField]
	private CatchPlayerTool CatchTool;

	private Vector3 MaxPos;

	private Vector3 MinPos;

	[Header("武器 Mesh")]
	[SerializeField]
	private GameObject HandGunL;

	[SerializeField]
	private GameObject HandGunR;

	[SerializeField]
	private GameObject LHand;

	[SerializeField]
	private GameObject RHand;

	[SerializeField]
	private GameObject BackSowrd;

	[SerializeField]
	private GameObject HandSowrd;

	[SerializeField]
	private GameObject LHandSowrd;

	private bool UsingLGun;

	private bool UsingRGun;

	private bool UsingSword;

	[Header("特效物件")]
	[SerializeField]
	private ParticleSystem Charge1;

	[SerializeField]
	private ParticleSystem Charge2;

	[SerializeField]
	private ParticleSystem RedAura;

	[Header("待機等待Frame")]
	[SerializeField]
	private float IdleWaitTime = 0.2f;

	private int IdleWaitFrame;

	[Header("三連擊")]
	[SerializeField]
	private Transform LShootPos;

	[SerializeField]
	private Transform RShootPos;

	[Header("單發能源炮")]
	[SerializeField]
	private float ChargeTime = 0.5f;

	private int ChargeFrame;

	private Vector2 BulletVelocity = Vector3.zero;

	private readonly int _HashAngle = Animator.StringToHash("angle");

	[Header("衝刺攻擊")]
	[SerializeField]
	private int DashSpeed = 20000;

	[SerializeField]
	private ParticleSystem DashFX;

	[Header("落鳳破")]
	[SerializeField]
	private int ShootNum = 4;

	[Header("幻夢零")]
	[SerializeField]
	private float Sk4HoldTime = 0.5f;

	[SerializeField]
	private ParticleSystem Sk4Mob02FX1;

	private int Sk4HoldFrame;

	[Header("真月輪")]
	private ShingetsurinBullet skill5bullet;

	[Header("連續砲彈")]
	[SerializeField]
	private int DefaultSk6AtkTimes = 10;

	private int Sk6AtkTimes;

	private int Sk6AtkFrame;

	private int Sk6ActionFrame;

	[Header("血紅終結")]
	private int SKill7MoveSpeed = OrangeCharacter.DashSpeed * 2;

	[SerializeField]
	private ParticleSystem Sk7FX1;

	[SerializeField]
	private ParticleSystem Sk7FX2;

	private Vector3 EndPos;

	[SerializeField]
	private bool Sk7UseNewMotion;

	[Header("弧形劍舞")]
	private VInt3 SK3Jump = new VInt3(7200, 14400, 0);

	private CollideBullet SwordRCollide;

	private CollideBullet SwordLCollide;

	[SerializeField]
	private ParticleSystem Sk8FX;

	[Header("亂舞")]
	private bool IsHitPlayer;

	private int PlayerGravity;

	public VInt3 SK9Jump = new VInt3(12000, 10500, 0);

	private int SK9Phase;

	private VInt3 SK9Jump2 = new VInt3(2400, 20000, 0);

	public int SK9FinalJump = 9600;

	public int DuringJump = 1500;

	public int SK9FinalJumpX = 3200;

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

	[SerializeField]
	private ParticleSystem Sk9FX1;

	[SerializeField]
	private ParticleSystem Sk9FX2;

	[SerializeField]
	private ParticleSystem Sk9FX3;

	[SerializeField]
	private ParticleSystem Sk9FX4;

	[Header("突襲")]
	[SerializeField]
	private int Sk10Spd = 18000;

	[Header("斬躪刃")]
	[SerializeField]
	private int Sk11UseTimes = 2;

	private float ShootFrame;

	private bool HasShot;

	[SerializeField]
	private ParticleSystem AuraFX2;

	[SerializeField]
	private bool DebugMode;

	[SerializeField]
	private MainStatus NextSkill;

	private bool bAuraSE;

	private Vector3 NowPos
	{
		get
		{
			return _transform.position;
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
			case "Skill5":
				NextSkill = MainStatus.Skill5;
				break;
			case "Skill6":
				NextSkill = MainStatus.Skill6;
				break;
			case "Skill7":
				NextSkill = MainStatus.Skill7;
				break;
			case "Skill8":
				NextSkill = MainStatus.Skill8;
				break;
			case "Skill9":
				NextSkill = MainStatus.Skill9;
				break;
			}
		}
	}

	protected virtual void HashAnimation()
	{
		_animationHash = new int[67];
		_animationHash[1] = Animator.StringToHash("BS086@debut_step1");
		_animationHash[2] = Animator.StringToHash("BS086@debut_step2");
		_animationHash[3] = Animator.StringToHash("BS086@debut_step3");
		_animationHash[4] = Animator.StringToHash("BS086@debut_step4");
		_animationHash[0] = Animator.StringToHash("BS086@idle_loop");
		_animationHash[5] = Animator.StringToHash("BS086@skill_01_step1_start");
		_animationHash[6] = Animator.StringToHash("BS086@skill_01_step1_loop");
		_animationHash[7] = Animator.StringToHash("BS086@skill_01_step2_start");
		_animationHash[8] = Animator.StringToHash("BS086@skill_01_step2_loop");
		_animationHash[9] = Animator.StringToHash("BS086@skill_01_step3_start");
		_animationHash[10] = Animator.StringToHash("BS086@skill_01_step3_end");
		_animationHash[11] = Animator.StringToHash("BS086@skill_02_step1_start");
		_animationHash[12] = Animator.StringToHash("BS086@skill_02_step1_loop");
		_animationHash[13] = Animator.StringToHash("BS086@skill_02_step2_start");
		_animationHash[14] = Animator.StringToHash("BS086@skill_02_step2_end");
		_animationHash[15] = Animator.StringToHash("BS086@skill_03_start");
		_animationHash[16] = Animator.StringToHash("BS086@skill_03_loop");
		_animationHash[17] = Animator.StringToHash("BS086@skill_03_end");
		_animationHash[18] = Animator.StringToHash("BS086@skill_04_start");
		_animationHash[19] = Animator.StringToHash("BS086@skill_04_loop");
		_animationHash[20] = Animator.StringToHash("BS086@skill_04_end");
		_animationHash[21] = Animator.StringToHash("BS086@skill_05_step1_start");
		_animationHash[22] = Animator.StringToHash("BS086@skill_05_step1_loop");
		_animationHash[23] = Animator.StringToHash("BS086@skill_05_step2_start");
		_animationHash[24] = Animator.StringToHash("BS086@skill_05_step2_loop");
		_animationHash[25] = Animator.StringToHash("BS086@skill_05_step2_end");
		_animationHash[26] = Animator.StringToHash("BS086@skill_06_start");
		_animationHash[27] = Animator.StringToHash("BS086@skill_06_loop");
		_animationHash[28] = Animator.StringToHash("BS086@skill_06_end");
		_animationHash[29] = Animator.StringToHash("ch073_skill_01_stand_start");
		_animationHash[30] = Animator.StringToHash("ch073_skill_01_stand_loop");
		_animationHash[31] = Animator.StringToHash("ch073_skill_01_stand_end");
		_animationHash[35] = Animator.StringToHash("BS060@skill_04_step1_start");
		_animationHash[36] = Animator.StringToHash("BS060@skill_04_step1_loop");
		_animationHash[37] = Animator.StringToHash("BS060@skill_04_step2_start");
		_animationHash[38] = Animator.StringToHash("BS060@skill_04_step2_loop");
		_animationHash[39] = Animator.StringToHash("BS060@skill_04_step3_start");
		_animationHash[40] = Animator.StringToHash("BS060@skill_04_step3_loop");
		_animationHash[41] = Animator.StringToHash("BS060@skill_04_step3_end");
		_animationHash[42] = Animator.StringToHash("BS060@skill_08_start");
		_animationHash[43] = Animator.StringToHash("BS060@skill_08_loop");
		_animationHash[44] = Animator.StringToHash("BS060@skill_08_end");
		_animationHash[45] = Animator.StringToHash("BS060@skill_05_step1_start");
		_animationHash[46] = Animator.StringToHash("BS060@skill_05_step1_loop");
		_animationHash[47] = Animator.StringToHash("BS060@skill_05_step2_start");
		_animationHash[48] = Animator.StringToHash("BS060@skill_05_step2_loop");
		_animationHash[49] = Animator.StringToHash("BS060@skill_05_step2_end");
		_animationHash[50] = Animator.StringToHash("BS060@skill_05_step3_start");
		_animationHash[51] = Animator.StringToHash("BS060@skill_05_step3_loop");
		_animationHash[52] = Animator.StringToHash("BS060@skill_05_step4_start");
		_animationHash[53] = Animator.StringToHash("BS060@skill_05_step4_loop");
		_animationHash[54] = Animator.StringToHash("BS060@skill_05_step4_end");
		_animationHash[55] = Animator.StringToHash("BS060@skill_02_start");
		_animationHash[56] = Animator.StringToHash("BS060@skill_02_loop");
		_animationHash[57] = Animator.StringToHash("BS060@skill_02_end");
		_animationHash[58] = Animator.StringToHash("BS060@skill_06_step2_start");
		_animationHash[59] = Animator.StringToHash("BS060@skill_06_step2_loop");
		_animationHash[60] = Animator.StringToHash("BS060@skill_06_step3_start");
		_animationHash[61] = Animator.StringToHash("BS060@skill_06_step3_loop");
		_animationHash[62] = Animator.StringToHash("BS060@skill_06_step3_end");
		if (!Sk7UseNewMotion)
		{
			_animationHash[32] = Animator.StringToHash("ch019_skill_01_start");
			_animationHash[33] = Animator.StringToHash("ch019_skill_01_loop");
			_animationHash[34] = Animator.StringToHash("ch019_skill_01_end");
		}
		else
		{
			_animationHash[32] = Animator.StringToHash("BS086@skill_08_start");
			_animationHash[33] = Animator.StringToHash("BS086@skill_08_loop");
			_animationHash[34] = Animator.StringToHash("BS086@skill_08_end");
		}
		_animationHash[63] = Animator.StringToHash("BS086@hurt_loop");
		_animationHash[64] = Animator.StringToHash("BS086@dead");
		_animationHash[65] = Animator.StringToHash("BS086@dead_step1");
		_animationHash[66] = Animator.StringToHash("BS086@dead_step2");
	}

	private void LoadParts(ref Transform[] childs)
	{
		ModelTransform = OrangeBattleUtility.FindChildRecursive(ref childs, "model", true);
		_animator = ModelTransform.GetComponent<Animator>();
		_collideBullet = OrangeBattleUtility.FindChildRecursive(ref childs, "Collider", true).gameObject.AddOrGetComponent<CollideBullet>();
		SwordLCollide = OrangeBattleUtility.FindChildRecursive(ref childs, "LSwordCollide", true).gameObject.AddOrGetComponent<CollideBullet>();
		SwordRCollide = OrangeBattleUtility.FindChildRecursive(ref childs, "RSwordCollide", true).gameObject.AddOrGetComponent<CollideBullet>();
		if (LShootPos == null)
		{
			LShootPos = OrangeBattleUtility.FindChildRecursive(ref childs, "L WeaponPoint", true);
		}
		if (RShootPos == null)
		{
			RShootPos = OrangeBattleUtility.FindChildRecursive(ref childs, "R WeaponPoint", true);
		}
		if (BackSowrd == null)
		{
			BackSowrd = OrangeBattleUtility.FindChildRecursive(ref childs, "SaberMesh_OnBack_m", true).gameObject;
		}
		if (HandSowrd == null)
		{
			HandSowrd = OrangeBattleUtility.FindChildRecursive(ref childs, "BS086_Saber", true).gameObject;
		}
		if (LHandSowrd == null)
		{
			LHandSowrd = OrangeBattleUtility.FindChildRecursive(ref childs, "BS086_Saber_L", true).gameObject;
		}
		if (HandGunL == null)
		{
			HandGunL = OrangeBattleUtility.FindChildRecursive(ref childs, "BusterMesh_L_m", true).gameObject;
		}
		if (HandGunR == null)
		{
			HandGunR = OrangeBattleUtility.FindChildRecursive(ref childs, "BusterMesh_R_m", true).gameObject;
		}
		if (LHand == null)
		{
			LHand = OrangeBattleUtility.FindChildRecursive(ref childs, "HandMesh_L_m", true).gameObject;
		}
		if (RHand == null)
		{
			RHand = OrangeBattleUtility.FindChildRecursive(ref childs, "HandMesh_R_m", true).gameObject;
		}
		if (RedAura == null)
		{
			RedAura = OrangeBattleUtility.FindChildRecursive(ref childs, "AuraFX", true).GetComponent<ParticleSystem>();
		}
		if (Charge1 == null)
		{
			Charge1 = OrangeBattleUtility.FindChildRecursive(ref childs, "charge1", true).GetComponent<ParticleSystem>();
		}
		if (Charge2 == null)
		{
			Charge2 = OrangeBattleUtility.FindChildRecursive(ref childs, "charge2", true).GetComponent<ParticleSystem>();
		}
		if (DashFX == null)
		{
			DashFX = OrangeBattleUtility.FindChildRecursive(ref childs, "DashFX", true).GetComponent<ParticleSystem>();
		}
		if (Sk4Mob02FX1 == null)
		{
			Sk4Mob02FX1 = OrangeBattleUtility.FindChildRecursive(ref childs, "Sk4Mob02FX1", true).GetComponent<ParticleSystem>();
		}
		if (Sk7FX1 == null)
		{
			Sk7FX1 = OrangeBattleUtility.FindChildRecursive(ref childs, "Sk7FX1", true).GetComponent<ParticleSystem>();
		}
		if (Sk7FX2 == null)
		{
			Sk7FX2 = OrangeBattleUtility.FindChildRecursive(ref childs, "Sk7FX2", true).GetComponent<ParticleSystem>();
		}
		if (AuraFX2 == null)
		{
			AuraFX2 = OrangeBattleUtility.FindChildRecursive(ref childs, "AuraFX2", true).GetComponent<ParticleSystem>();
		}
		if (CatchTool == null)
		{
			CatchTool = ModelTransform.GetComponent<CatchPlayerTool>();
		}
		if ((bool)CatchTool)
		{
			CatchTool.DieReleaseMastetPos = true;
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
		FallDownSE = new string[2] { "BossSE03", "bs030_via02" };
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
				UseLGun(false);
				UseRGun(false);
				TakeSword(false);
				break;
			}
			break;
		case MainStatus.Idle:
			UseLGun(false);
			UseRGun(false);
			TakeSword(false);
			TakeLSword(false);
			SwordRCollide.HitCallback = null;
			SwordLCollide.HitCallback = null;
			_velocity.x = 0;
			IdleWaitFrame = GameLogicUpdateManager.GameFrame + (int)(IdleWaitTime * 20f);
			break;
		case MainStatus.Skill0:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_velocity = VInt3.zero;
				UseRGun(true);
				break;
			case SubStatus.Phase1:
				ShootFrame = 0f;
				HasShot = false;
				break;
			case SubStatus.Phase2:
				UseRGun(false);
				UseLGun(true);
				break;
			case SubStatus.Phase3:
				ShootFrame = 0.1f;
				HasShot = false;
				break;
			case SubStatus.Phase4:
				ShootFrame = 0.4f;
				HasShot = false;
				UseLGun(false);
				TakeSword(true);
				break;
			}
			break;
		case MainStatus.Skill1:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				UseLGun(true);
				_velocity = VInt3.zero;
				SwitchFx(Charge1, true);
				break;
			case SubStatus.Phase1:
				_collideBullet.UpdateBulletData(EnemyWeapons[GetWeaponID(UseSkill.Sk2Slash)].BulletData);
				_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
				_collideBullet.Active(targetMask);
				ChargeFrame = GameLogicUpdateManager.GameFrame + (int)(ChargeTime * 20f);
				break;
			case SubStatus.Phase2:
				_collideBullet.UpdateBulletData(EnemyWeapons[GetWeaponID(UseSkill.BodyCollide)].BulletData);
				_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
				_collideBullet.Active(targetMask);
				SwitchFx(Charge1, false);
				SwitchFx(Charge2, true);
				ShootFrame = 0.2f;
				HasShot = false;
				break;
			}
			break;
		case MainStatus.Skill2:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				Target = _enemyAutoAimSystem.GetClosetPlayer();
				if ((bool)Target && Target._transform.position.y > _transform.position.y + 5f && Mathf.Abs(Target._transform.position.x - _transform.position.x) < 5f)
				{
					UpdateDirection(-base.direction);
				}
				TakeSword(true);
				_velocity = VInt3.zero;
				break;
			case SubStatus.Phase1:
				SwitchFx(DashFX, true);
				PlaySE("BossSE03", "bs030_via03");
				IsInvincible = true;
				_velocity.x = DashSpeed * base.direction;
				break;
			case SubStatus.Phase2:
				SwitchFx(DashFX, false);
				PlaySE("BossSE03", "bs030_via04");
				IsInvincible = false;
				_velocity = VInt3.zero;
				break;
			}
			break;
		case MainStatus.Skill3:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_velocity = VInt3.zero;
				break;
			case SubStatus.Phase1:
				ShootFrame = 0f;
				HasShot = false;
				break;
			}
			break;
		case MainStatus.Skill4:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				TakeSword(true);
				_velocity = VInt3.zero;
				SwitchFx(RedAura, true);
				PlaySE("BossSE03", "bs030_via08_lp");
				bAuraSE = true;
				break;
			case SubStatus.Phase1:
				Sk4HoldFrame = GameLogicUpdateManager.GameFrame + (int)(Sk4HoldTime * 20f);
				break;
			case SubStatus.Phase2:
			{
				ShootFrame = 0.15f;
				HasShot = false;
				AI_STATE aiState = AiState;
				if ((uint)(aiState - 1) <= 1u && (bool)Sk4Mob02FX1)
				{
					SwitchFx(Sk4Mob02FX1, true);
				}
				break;
			}
			case SubStatus.Phase4:
				PlaySE("BossSE03", "bs030_via08_stop");
				bAuraSE = false;
				SwitchFx(RedAura, false);
				break;
			}
			break;
		case MainStatus.Skill5:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_velocity = VInt3.zero;
				SwitchFx(RedAura, true);
				PlaySE("BossSE03", "bs030_via08_lp");
				bAuraSE = true;
				break;
			case SubStatus.Phase1:
				ShootFrame = 0.2f;
				HasShot = false;
				break;
			case SubStatus.Phase2:
				SwitchFx(RedAura, false);
				PlaySE("BossSE03", "bs030_via08_stop");
				bAuraSE = false;
				break;
			}
			break;
		case MainStatus.Skill6:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				UpdateDirection();
				UseLGun(true);
				Sk6AtkTimes = DefaultSk6AtkTimes;
				Sk6AtkFrame = EnemyWeapons[2].BulletData.n_FIRE_SPEED * 20 / 1000;
				break;
			case SubStatus.Phase1:
				Sk6ActionFrame = GameLogicUpdateManager.GameFrame + Sk6AtkFrame;
				break;
			case SubStatus.Phase2:
				UseLGun(false);
				break;
			}
			break;
		case MainStatus.Skill7:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_velocity = VInt3.zero;
				break;
			case SubStatus.Phase1:
				if (!Target)
				{
					Target = _enemyAutoAimSystem.GetClosetPlayer();
				}
				if (!Target)
				{
					UpdateDirection(-base.direction);
					EndPos = _transform.position + Vector3.right * 5f * base.direction;
				}
				if ((bool)Target)
				{
					TargetPos = Target.Controller.LogicPosition;
					UpdateDirection();
					EndPos = Target._transform.position + Vector3.right * base.direction;
				}
				_velocity.x = SKill7MoveSpeed * base.direction;
				if ((bool)Sk7FX1)
				{
					SwitchFx(Sk7FX1, true);
				}
				if ((bool)Sk7FX2)
				{
					SwitchFx(Sk7FX2, true);
				}
				_collideBullet.UpdateBulletData(EnemyWeapons[GetWeaponID(UseSkill.Sk7Claw)].BulletData);
				_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
				_collideBullet.Active(targetMask);
				break;
			case SubStatus.Phase2:
				if ((bool)Sk7FX1)
				{
					SwitchFx(Sk7FX1, false);
				}
				if ((bool)Sk7FX2)
				{
					SwitchFx(Sk7FX2, false);
				}
				_collideBullet.UpdateBulletData(EnemyWeapons[GetWeaponID(UseSkill.BodyCollide)].BulletData);
				_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
				_collideBullet.Active(targetMask);
				_velocity = VInt3.zero;
				break;
			}
			break;
		case MainStatus.Skill8:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				TakeLSword(true);
				_velocity = SK3Jump;
				_velocity.x *= base.direction;
				PlayBossSE("BossSE03", "bs030_via24");
				break;
			case SubStatus.Phase2:
				IgnoreGravity = true;
				_velocity.x = 0;
				break;
			case SubStatus.Phase3:
				HasShot = false;
				SwitchFx(Sk8FX, true);
				SwordLCollide.Active(targetMask);
				_collideBullet.BackToPool();
				break;
			case SubStatus.Phase4:
				TakeLSword(false);
				IgnoreGravity = false;
				_collideBullet.Active(targetMask);
				SwordLCollide.BackToPool();
				break;
			case SubStatus.Phase6:
				_velocity = VInt3.zero;
				break;
			}
			break;
		case MainStatus.Skill9:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				IsNewGravity = false;
				SK9Phase = 0;
				_animator.speed = 2f;
				SwordRCollide.HitCallback = CatchPlayer;
				SwordLCollide.HitCallback = CatchPlayer;
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
						SK9Jump.x = num4 * base.direction;
						_velocity = SK9Jump;
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
						SK9Jump.y = num9 + GravityNew;
						SK9Jump.x = 12000 * base.direction;
						_velocity = SK9Jump;
					}
					PlaySE("BossSE03", "bs030_via01");
					PlayBossSE("BossSE03", "bs030_via25");
				}
				else
				{
					SetStatus(MainStatus.Skill9, SubStatus.Phase3);
				}
				break;
			case SubStatus.Phase2:
				_velocity.x = 0;
				if ((bool)Target)
				{
					TargetPos = new VInt3(Target.transform.position);
					UpdateDirection();
				}
				break;
			case SubStatus.Phase3:
				UpdateDirection();
				SwitchFx(Sk9FX1, true);
				_velocity.x = 0;
				PlaySE("BossSE03", "bs030_via06");
				TakeLSword(true);
				SwordLCollide.Active(targetMask);
				_collideBullet.BackToPool();
				SK9Phase++;
				break;
			case SubStatus.Phase4:
				SwordLCollide.BackToPool();
				break;
			case SubStatus.Phase5:
				SwitchFx(Sk9FX2, true);
				PlaySE("BossSE03", "bs030_via06");
				TakeLSword(true);
				SwordLCollide.Active(targetMask);
				_collideBullet.BackToPool();
				SK9Phase++;
				break;
			case SubStatus.Phase6:
				SwordLCollide.BackToPool();
				break;
			case SubStatus.Phase7:
				TakeLSword(false);
				break;
			case SubStatus.Phase10:
				SwitchFx(Sk9FX4, true);
				PlaySE("BossSE03", "bs030_via20");
				TakeLSword(true);
				SwordLCollide.Active(targetMask);
				_collideBullet.BackToPool();
				SK9Phase++;
				break;
			case SubStatus.Phase11:
				TakeLSword(false);
				_collideBullet.Active(targetMask);
				SwordLCollide.BackToPool();
				break;
			case SubStatus.Phase13:
				SK9Phase++;
				PlaySE("BossSE03", "bs030_via05");
				break;
			case SubStatus.Phase16:
				SwitchFx(Sk9FX3, true);
				PlaySE("BossSE03", "bs030_via21");
				TakeSword(true);
				SwordRCollide.Active(targetMask);
				SwordRCollide.HitCallback = CatchPlayer;
				_collideBullet.BackToPool();
				_velocity = SK9Jump2;
				_velocity.x *= base.direction;
				if (SK9Phase == 6)
				{
					SK9Phase = 7;
					_animator.speed = 1f;
					_velocity.y = SK9FinalJump;
					if (FinalJump)
					{
						_velocity.x = SK9FinalJumpX * base.direction;
					}
					else
					{
						_velocity.x = 0;
					}
				}
				break;
			case SubStatus.Phase18:
				SwordRCollide.BackToPool();
				SwordRCollide.HitCallback = null;
				TakeLSword(false);
				SwordLCollide.BackToPool();
				SwordLCollide.HitCallback = null;
				break;
			case SubStatus.Phase20:
				if (CatchTool.IsCatching)
				{
					ReleaseTarget();
				}
				_velocity = VInt3.zero;
				break;
			case SubStatus.Phase21:
				_collideBullet.Active(targetMask);
				IsNewGravity = false;
				SK9Phase = 0;
				_animator.speed = 1f;
				_collideBullet.Active(targetMask);
				if (CatchTool.IsCatching)
				{
					ReleaseTarget();
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
				_velocity = VInt3.zero;
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
				if (bAuraSE)
				{
					PlayBossSE("BossSE03", "bs030_via08_stop");
					bAuraSE = false;
				}
				if (!Controller.Collisions.below)
				{
					SetStatus(MainStatus.Die, SubStatus.Phase3);
					return;
				}
				base.DeadPlayCompleted = true;
				break;
			case SubStatus.Phase1:
				UpdateDirection(0, true);
				break;
			case SubStatus.Phase2:
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fx_bs086_teleport_out", Controller.GetRealCenterPos(), Quaternion.identity, new object[1] { Vector3.one });
				SwitchFx(AuraFX2, false);
				BackToPool();
				break;
			case SubStatus.Phase3:
				_velocity = new VInt3(-500 * base.direction, 1500, 0);
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
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_DEBUT1;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_DEBUT2;
				return;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_DEBUT3;
				break;
			case SubStatus.Phase3:
				_currentAnimationId = AnimationID.ANI_DEBUT4;
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
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_SKILL0_START1;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_SKILL0_LOOP1;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_SKILL0_START2;
				break;
			case SubStatus.Phase3:
				_currentAnimationId = AnimationID.ANI_SKILL0_LOOP2;
				break;
			case SubStatus.Phase4:
				_currentAnimationId = AnimationID.ANI_SKILL0_START3;
				break;
			case SubStatus.Phase5:
				_currentAnimationId = AnimationID.ANI_SKILL0_END3;
				break;
			}
			break;
		case MainStatus.Skill1:
			switch (_subStatus)
			{
			default:
				return;
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_SKILL1_START1;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_SKILL1_LOOP1;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_SKILL1_START2;
				break;
			case SubStatus.Phase3:
				_currentAnimationId = AnimationID.ANI_SKILL1_END2;
				break;
			}
			break;
		case MainStatus.Skill2:
			switch (_subStatus)
			{
			default:
				return;
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_SKILL2_START;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_SKILL2_LOOP;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_SKILL2_END;
				break;
			}
			break;
		case MainStatus.Skill3:
			switch (_subStatus)
			{
			default:
				return;
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_SKILL3_START;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_SKILL3_LOOP;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_SKILL3_END;
				break;
			}
			break;
		case MainStatus.Skill4:
			switch (_subStatus)
			{
			default:
				return;
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_SKILL4_START1;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_SKILL4_LOOP1;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_SKILL4_START2;
				break;
			case SubStatus.Phase3:
				_currentAnimationId = AnimationID.ANI_SKILL4_LOOP2;
				break;
			case SubStatus.Phase4:
				_currentAnimationId = AnimationID.ANI_SKILL4_END;
				break;
			}
			break;
		case MainStatus.Skill5:
			switch (_subStatus)
			{
			default:
				return;
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_SKILL5_START;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_SKILL5_LOOP;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_SKILL5_END;
				break;
			}
			break;
		case MainStatus.Skill6:
			switch (_subStatus)
			{
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
		case MainStatus.Skill8:
			switch (_subStatus)
			{
			default:
				return;
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_SKILL8_START1;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_SKILL8_LOOP1;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_SKILL8_START2;
				break;
			case SubStatus.Phase3:
				_currentAnimationId = AnimationID.ANI_SKILL8_LOOP2;
				break;
			case SubStatus.Phase4:
				_currentAnimationId = AnimationID.ANI_SKILL8_START3;
				break;
			case SubStatus.Phase5:
				_currentAnimationId = AnimationID.ANI_SKILL8_LOOP3;
				break;
			case SubStatus.Phase6:
				_currentAnimationId = AnimationID.ANI_SKILL8_END3;
				break;
			}
			break;
		case MainStatus.Skill9:
			switch (_subStatus)
			{
			default:
				return;
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_SKILL9_START1;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_SKILL9_LOOP1;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_SKILL9_END1;
				break;
			case SubStatus.Phase3:
				_currentAnimationId = AnimationID.ANI_SKILL9_START2;
				break;
			case SubStatus.Phase4:
				_currentAnimationId = AnimationID.ANI_SKILL9_LOOP2;
				break;
			case SubStatus.Phase5:
				_currentAnimationId = AnimationID.ANI_SKILL9_START3;
				break;
			case SubStatus.Phase6:
				_currentAnimationId = AnimationID.ANI_SKILL9_LOOP3;
				break;
			case SubStatus.Phase7:
				_currentAnimationId = AnimationID.ANI_SKILL9_END3;
				break;
			case SubStatus.Phase8:
				_currentAnimationId = AnimationID.ANI_SKILL9_START4;
				break;
			case SubStatus.Phase9:
				_currentAnimationId = AnimationID.ANI_SKILL9_LOOP4;
				break;
			case SubStatus.Phase10:
				_currentAnimationId = AnimationID.ANI_SKILL9_START5;
				break;
			case SubStatus.Phase11:
				_currentAnimationId = AnimationID.ANI_SKILL9_LOOP5;
				break;
			case SubStatus.Phase12:
				_currentAnimationId = AnimationID.ANI_SKILL9_END5;
				break;
			case SubStatus.Phase13:
				_currentAnimationId = AnimationID.ANI_SKILL9_START6;
				break;
			case SubStatus.Phase14:
				_currentAnimationId = AnimationID.ANI_SKILL9_LOOP6;
				break;
			case SubStatus.Phase15:
				_currentAnimationId = AnimationID.ANI_SKILL9_END6;
				break;
			case SubStatus.Phase16:
				_currentAnimationId = AnimationID.ANI_SKILL9_START7;
				break;
			case SubStatus.Phase17:
				_currentAnimationId = AnimationID.ANI_SKILL9_LOOP7;
				break;
			case SubStatus.Phase18:
				_currentAnimationId = AnimationID.ANI_SKILL9_START8;
				break;
			case SubStatus.Phase19:
				_currentAnimationId = AnimationID.ANI_SKILL9_LOOP8;
				break;
			case SubStatus.Phase20:
				_currentAnimationId = AnimationID.ANI_SKILL9_END8;
				break;
			}
			break;
		case MainStatus.Die:
			switch (_subStatus)
			{
			default:
				return;
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_LEAVE1;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_LEAVE2;
				break;
			case SubStatus.Phase3:
				_currentAnimationId = AnimationID.ANI_HURT;
				break;
			case SubStatus.Phase2:
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
				break;
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
				if (Controller.Collisions.below)
				{
					SetStatus(MainStatus.Debut, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if (_currentFrame > 1f)
				{
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
				else if (_currentFrame > 0.9f && UsingSword)
				{
					TakeSword(false);
				}
				else if (_currentFrame > 0.18f && _currentFrame < 0.9f && !UsingSword)
				{
					TakeSword(true);
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
		{
			if (bWaitNetStatus || IdleWaitFrame >= GameLogicUpdateManager.GameFrame)
			{
				break;
			}
			Target = _enemyAutoAimSystem.GetClosetPlayer();
			if ((bool)Target)
			{
				UpdateDirection();
				UpdateRandomState();
				break;
			}
			AI_STATE aiState = AiState;
			if ((uint)(aiState - 1) <= 1u)
			{
				UpdateRandomState(MainStatus.Skill7);
			}
			else
			{
				UpdateRandomState(MainStatus.Skill2);
			}
			break;
		}
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
				if (!HasShot && _currentFrame > ShootFrame)
				{
					HasShot = true;
					BulletBase.TryShotBullet(EnemyWeapons[GetWeaponID(UseSkill.Sk0Bullet1)].BulletData, RShootPos.position, Vector3.right * base.direction, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
					SetStatus(MainStatus.Skill0, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill0, SubStatus.Phase3);
				}
				break;
			case SubStatus.Phase3:
				if (!HasShot && _currentFrame > ShootFrame)
				{
					HasShot = true;
					BulletBase.TryShotBullet(EnemyWeapons[GetWeaponID(UseSkill.Sk0Bullet1)].BulletData, LShootPos.position, Vector3.right * base.direction, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
				}
				else if (_currentFrame > 0.8f)
				{
					SetStatus(MainStatus.Skill0, SubStatus.Phase4);
				}
				break;
			case SubStatus.Phase4:
				if (!HasShot && _currentFrame > ShootFrame)
				{
					HasShot = true;
					PlayBossSE("BossSE03", "bs030_via07");
					BulletBase.TryShotBullet(EnemyWeapons[GetWeaponID(UseSkill.Sk0Bullet2)].BulletData, Controller.GetCenterPos() + Vector3.right * base.direction, Vector3.right * base.direction, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
				}
				else if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill0, SubStatus.Phase5);
				}
				break;
			case SubStatus.Phase5:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Idle);
				}
				else if (_currentFrame > 0.36f && UsingSword)
				{
					TakeSword(false);
				}
				break;
			}
			break;
		case MainStatus.Skill1:
			if (_subStatus != SubStatus.Phase3)
			{
				BulletVelocity = Vector3.right * base.direction;
				Target = _enemyAutoAimSystem.GetClosetPlayer();
				if ((bool)Target)
				{
					TargetPos = new VInt3(Target._transform.position);
					UpdateDirection();
					BulletVelocity = Target._transform.position - LShootPos.position + Vector3.up * 0.5f;
				}
				float value = Vector3.Angle(BulletVelocity.normalized, Vector3.right * base.direction);
				_animator.SetFloat(_HashAngle, value);
			}
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill1, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if (ChargeFrame < GameLogicUpdateManager.GameFrame)
				{
					SetStatus(MainStatus.Skill1, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				if (!HasShot && _currentFrame > ShootFrame)
				{
					HasShot = true;
					BulletBase.TryShotBullet(EnemyWeapons[GetWeaponID(UseSkill.Sk1Bullet)].BulletData, LShootPos.position + (Vector3)BulletVelocity.normalized, BulletVelocity, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
					SwitchFx(Charge2, false);
				}
				else if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill1, SubStatus.Phase3);
				}
				break;
			case SubStatus.Phase3:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Idle);
				}
				else if (_currentFrame > 0.5f && UsingLGun)
				{
					UseLGun(false);
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
				if (Controller.Collisions.left || Controller.Collisions.right)
				{
					SetStatus(MainStatus.Skill2, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Idle);
				}
				else if (_currentFrame > 0.7f && UsingSword)
				{
					TakeSword(false);
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
				if (!HasShot && _currentFrame > ShootFrame)
				{
					HasShot = true;
					float num = 90 / (ShootNum - 1);
					for (int j = 0; j < ShootNum; j++)
					{
						Vector3 vector2 = Quaternion.Euler(0f, 0f, num * (float)j * (float)base.direction) * (Vector3.right * base.direction);
						Vector3 worldPos3 = vector2 * 0.5f + Vector3.up * 0.5f + _transform.position;
						BulletBase.TryShotBullet(EnemyWeapons[GetWeaponID(UseSkill.Sk3Bullet)].BulletData, worldPos3, vector2, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
					}
				}
				else if (_currentFrame > 0.5f)
				{
					SetStatus(MainStatus.Skill3, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
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
				if (Sk4HoldFrame < GameLogicUpdateManager.GameFrame)
				{
					SetStatus(MainStatus.Skill4, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				if (!HasShot && _currentFrame > ShootFrame)
				{
					HasShot = true;
					AI_STATE aiState = AiState;
					if ((uint)(aiState - 1) <= 1u)
					{
						BulletBase.TryShotBullet(EnemyWeapons[GetWeaponID(UseSkill.Sk4BulletEX)].BulletData, HandSowrd.transform, Vector3.right * base.direction, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
					}
					else
					{
						BulletBase.TryShotBullet(EnemyWeapons[GetWeaponID(UseSkill.Sk4Bullet)].BulletData, HandSowrd.transform, Vector3.right * base.direction, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
					}
				}
				else if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill4, SubStatus.Phase3);
				}
				break;
			case SubStatus.Phase3:
				if (_currentFrame > 0.5f)
				{
					SetStatus(MainStatus.Skill4, SubStatus.Phase4);
				}
				break;
			case SubStatus.Phase4:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Idle);
				}
				else if (_currentFrame > 0.1f && UsingSword)
				{
					TakeSword(false);
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
				if (!HasShot && _currentFrame > ShootFrame)
				{
					HasShot = true;
					BulletVelocity = Vector2.right * base.direction;
					Target = _enemyAutoAimSystem.GetClosetPlayer();
					if ((bool)Target)
					{
						BulletVelocity = Target.Controller.GetRealCenterPos().xy() - RShootPos.position.xy();
					}
					skill5bullet = BulletBase.TryShotBullet(EnemyWeapons[GetWeaponID(UseSkill.Sk5Bullet)].BulletData, RShootPos, BulletVelocity.normalized, null, selfBuffManager.sBuffStatus, EnemyData, targetMask) as ShingetsurinBullet;
				}
				else if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill5, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				if (_currentFrame > 1f)
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
				if (GameLogicUpdateManager.GameFrame > Sk6ActionFrame)
				{
					Vector3 worldPos2 = RShootPos.position + Vector3.up * OrangeBattleUtility.Random(-1, 2) * 0.1f + Vector3.right * ((float)base.direction * 1.5f);
					BulletBase.TryShotBullet(EnemyWeapons[GetWeaponID(UseSkill.Sk6Bullet)].BulletData, worldPos2, Vector3.right * base.direction, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
					if (--Sk6AtkTimes > 0)
					{
						Sk6ActionFrame += Sk6AtkFrame;
					}
					else
					{
						SetStatus(MainStatus.Skill6, SubStatus.Phase2);
					}
				}
				break;
			case SubStatus.Phase2:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Idle);
				}
				break;
			}
			break;
		case MainStatus.Skill7:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill7, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if ((_transform.position.x - EndPos.x) * (float)base.direction > 0f || (base.direction == 1 && Controller.Collisions.right) || (base.direction == -1 && Controller.Collisions.left))
				{
					SetStatus(MainStatus.Skill7, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
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
				if (_velocity.y <= 2000)
				{
					SetStatus(MainStatus.Skill8, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill8, SubStatus.Phase3);
				}
				break;
			case SubStatus.Phase3:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill8, SubStatus.Phase4);
				}
				if (!HasShot && _currentFrame > 0.2f)
				{
					PlayBossSE("BossSE03", "bs030_via19");
					HasShot = true;
					Vector3 vector3 = new Vector3(0f, 0.5f, 0f) + _transform.position;
					for (int i = 0; i < 8; i++)
					{
						Vector3 vector = Quaternion.Euler(0f, 0f, (float)(i * 45) + 22.5f) * Vector3.up;
						Vector3 worldPos = new Vector3(0f, 0.5f, 0f) + vector * 0.5f + _transform.position;
						BulletBase.TryShotBullet(EnemyWeapons[GetWeaponID(UseSkill.Sk0Bullet2)].BulletData, worldPos, vector, null, selfBuffManager.sBuffStatus, EnemyData, targetMask)._transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
					}
				}
				break;
			case SubStatus.Phase4:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill8, SubStatus.Phase5);
				}
				break;
			case SubStatus.Phase5:
				if (Controller.Collisions.below)
				{
					SetStatus(MainStatus.Skill8, SubStatus.Phase6);
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
		case MainStatus.Skill9:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (IsNewGravity)
				{
					_velocity.y += GravityNew;
				}
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill9, SubStatus.Phase1);
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
					SetStatus(MainStatus.Skill9, SubStatus.Phase3);
				}
				else if (Controller.Collisions.below)
				{
					SetStatus(MainStatus.Skill9, SubStatus.Phase2);
				}
				break;
			}
			case SubStatus.Phase2:
				if (_currentFrame > 1f)
				{
					if (IsHitPlayer)
					{
						SetStatus(MainStatus.Skill9, SubStatus.Phase3);
					}
					else
					{
						SetStatus(MainStatus.Skill9, SubStatus.Phase21);
					}
				}
				break;
			case SubStatus.Phase3:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill9, SubStatus.Phase4);
				}
				break;
			case SubStatus.Phase4:
				if (_currentFrame > 0.1f)
				{
					SetStatus(MainStatus.Skill9, SubStatus.Phase5);
				}
				break;
			case SubStatus.Phase5:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill9, SubStatus.Phase6);
				}
				break;
			case SubStatus.Phase6:
				if (_currentFrame > 0.1f)
				{
					SetStatus(MainStatus.Skill9, SubStatus.Phase7);
				}
				break;
			case SubStatus.Phase7:
				if (_currentFrame > 1f)
				{
					if (SK9Phase == 2)
					{
						SetStatus(MainStatus.Skill9, SubStatus.Phase13);
					}
					else
					{
						SetStatus(MainStatus.Skill9, SubStatus.Phase8);
					}
				}
				break;
			case SubStatus.Phase8:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill9, SubStatus.Phase9);
				}
				break;
			case SubStatus.Phase9:
				if (_currentFrame > 0.1f)
				{
					SetStatus(MainStatus.Skill9, SubStatus.Phase10);
				}
				break;
			case SubStatus.Phase10:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill9, SubStatus.Phase11);
				}
				break;
			case SubStatus.Phase11:
				if (_currentFrame > 0.1f)
				{
					SetStatus(MainStatus.Skill9, SubStatus.Phase12);
				}
				break;
			case SubStatus.Phase12:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill9, SubStatus.Phase13);
				}
				break;
			case SubStatus.Phase13:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill9, SubStatus.Phase14);
				}
				if (!UsingSword && _currentFrame > 0.28f)
				{
					TakeSword(true);
					SwordRCollide.Active(targetMask);
					_collideBullet.BackToPool();
				}
				break;
			case SubStatus.Phase14:
				if (_currentFrame > 0.1f)
				{
					_collideBullet.Active(targetMask);
					SwordRCollide.BackToPool();
					SetStatus(MainStatus.Skill9, SubStatus.Phase15);
				}
				break;
			case SubStatus.Phase15:
				if (_currentFrame > 1f)
				{
					TakeSword(false);
					if (SK9Phase == 3)
					{
						SetStatus(MainStatus.Skill9, SubStatus.Phase5);
					}
					else if (SK9Phase == 6)
					{
						SetStatus(MainStatus.Skill9, SubStatus.Phase16);
					}
				}
				break;
			case SubStatus.Phase16:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill9, SubStatus.Phase17);
				}
				if (SK9Phase == 7)
				{
					_velocity.y += DuringJump;
					if (FinalJump)
					{
						_velocity.x += DuringJumpX * base.direction;
					}
				}
				break;
			case SubStatus.Phase17:
				if (_velocity.y <= 0)
				{
					if (CatchTool.IsCatching && SK9Phase == 7)
					{
						ReleaseTarget();
						PlayerGravity = _velocity.y;
					}
					SetStatus(MainStatus.Skill9, SubStatus.Phase18);
				}
				if (SK9Phase == 7 && _velocity.y > 0)
				{
					_velocity.y += DuringJump;
					if (FinalJump)
					{
						_velocity.x += DuringJumpX * base.direction;
					}
				}
				break;
			case SubStatus.Phase18:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill9, SubStatus.Phase19);
				}
				break;
			case SubStatus.Phase19:
				if (Controller.Collisions.below)
				{
					SetStatus(MainStatus.Skill9, SubStatus.Phase20);
				}
				if (SK9Phase == 7)
				{
					_velocity.y += DuringJump;
					if (FinalJump)
					{
						_velocity.x += DuringJumpX * base.direction;
					}
				}
				break;
			case SubStatus.Phase20:
				if (_currentFrame > 1f)
				{
					if (SK9Phase == 7)
					{
						SetStatus(MainStatus.Skill9, SubStatus.Phase21);
						break;
					}
					_animator.speed = 2f;
					SwordRCollide.HitCallback = CatchPlayer;
					SwordLCollide.HitCallback = CatchPlayer;
					SetStatus(MainStatus.Skill9, SubStatus.Phase1);
				}
				break;
			}
			if (CatchTool.IsCatching)
			{
				CatchTool.MoveTarget();
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
			case SubStatus.Phase3:
				if (Controller.Collisions.below)
				{
					if (nDeadCount > 2)
					{
						SetStatus(MainStatus.Die);
					}
					else
					{
						nDeadCount++;
					}
				}
				break;
			case SubStatus.Phase2:
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
			_collideBullet.UpdateBulletData(EnemyWeapons[GetWeaponID(UseSkill.BodyCollide)].BulletData);
			_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			_collideBullet.Active(targetMask);
			SwordLCollide.UpdateBulletData(EnemyWeapons[GetWeaponID(UseSkill.Sk9Slash)].BulletData);
			SwordLCollide.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			SwordRCollide.UpdateBulletData(EnemyWeapons[GetWeaponID(UseSkill.Sk9Slash)].BulletData);
			SwordRCollide.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			TakeSword(false);
			TakeLSword(false);
			CheckRoomSize();
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
		if ((bool)Target && Target._transform.position.y > _transform.position.y + 5f)
		{
			if (Mathf.Abs(Target._transform.position.x - _transform.position.x) < 5f)
			{
				if (OrangeBattleUtility.Random(0, 100) < 50)
				{
					return 7;
				}
				return 4;
			}
			if (RangedSKC.ToArray().Length < 1)
			{
				RangedSKC = new List<int>(DefaultRangedSkillCard);
			}
			int num = RangedSKC[OrangeBattleUtility.Random(0, RangedSKC.ToArray().Length)];
			RangedSKC.Remove(num);
			return num + StartPos;
		}
		if (SkillCard.ToArray().Length < 1)
		{
			SkillCard = new List<int>(DefaultSkillCard);
			AI_STATE aiState = AiState;
			if (aiState == AI_STATE.mob_003)
			{
				DefaultSkillCard = new int[7] { 0, 4, 5, 6, 7, 8, 9 };
			}
		}
		int num2 = SkillCard[OrangeBattleUtility.Random(0, SkillCard.ToArray().Length)];
		SkillCard.Remove(num2);
		return num2 + StartPos;
	}

	protected override void DeadBehavior(ref HurtPassParam tHurtPassParam)
	{
		if (_mainStatus != MainStatus.Die)
		{
			_animator.speed = 1f;
			if ((bool)_collideBullet)
			{
				_collideBullet.BackToPool();
			}
			if ((bool)skill5bullet && !skill5bullet.bIsEnd)
			{
				skill5bullet.BackToPool();
			}
			StageUpdate.SlowStage();
			SetColliderEnable(false);
			TakeSword(false);
			TakeLSword(false);
			UseLGun(false);
			UseRGun(false);
			StopAllFX();
			IgnoreGravity = false;
			AI_STATE aiState = AiState;
			if (aiState == AI_STATE.mob_003 && CatchTool.IsCatching)
			{
				ReleaseTarget();
			}
			PlayBossSE("BossSE03", "bs030_via23");
			SetStatus(MainStatus.Die);
		}
	}

	private void CatchPlayer(object obj)
	{
		if (obj == null || CatchTool.IsCatching)
		{
			return;
		}
		Collider2D collider2D = obj as Collider2D;
		if (collider2D != null)
		{
			OrangeCharacter hitTargetOrangeCharacter = OrangeBattleUtility.GetHitTargetOrangeCharacter(collider2D);
			if (hitTargetOrangeCharacter != null)
			{
				CatchTool.CatchTarget(hitTargetOrangeCharacter);
				CatchTool.PosOffset = new Vector3(1.5f * (float)base.direction, 0.5f - (hitTargetOrangeCharacter.GetTargetPoint().y - hitTargetOrangeCharacter._transform.position.y), 0f);
				hitTargetOrangeCharacter.direction = -base.direction;
			}
		}
		if (CatchTool.IsCatching)
		{
			SwordRCollide.HitCallback = HitCameraShake;
			SwordLCollide.HitCallback = HitCameraShake;
		}
	}

	private void ReleaseTarget()
	{
		if (CatchTool.TargetOC._transform.position.x > MaxPos.x - 0.8f || CatchTool.TargetOC._transform.position.x < MinPos.x + 0.8f)
		{
			CatchTool.ReleaseTarget(NowPos);
		}
		else
		{
			CatchTool.ReleaseTarget();
		}
	}

	private void HitCameraShake(object obj)
	{
		Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.CAMERA_SHAKE, 0.5f, false);
	}

	private void CheckRoomSize()
	{
		int layerMask = (1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockLayer) | (1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockEnemyLayer) | (1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockPlayerLayer);
		Vector3 vector = new Vector3(_transform.position.x, _transform.position.y + 1f, 0f);
		RaycastHit2D raycastHit2D = OrangeBattleUtility.RaycastIgnoreSelf(vector, Vector3.left, 30f, layerMask, _transform);
		RaycastHit2D raycastHit2D2 = OrangeBattleUtility.RaycastIgnoreSelf(vector, Vector3.right, 30f, layerMask, _transform);
		bool flag = false;
		if (!raycastHit2D)
		{
			flag = true;
			Debug.LogError("沒有偵測到左邊界，之後一些技能無法準確判斷位置");
		}
		if (!raycastHit2D2)
		{
			flag = true;
			Debug.LogError("沒有偵測到右邊界，之後一些技能無法準確判斷位置");
		}
		if (flag)
		{
			MaxPos = new Vector3(NowPos.x + 3f, NowPos.y + 8f, 0f);
			MinPos = new Vector3(NowPos.x - 6f, NowPos.y, 0f);
		}
		else
		{
			MaxPos = new Vector3(raycastHit2D2.point.x, NowPos.y + 8f, 0f);
			MinPos = new Vector3(raycastHit2D.point.x, NowPos.y, 0f);
		}
	}

	private void StopAllFX()
	{
		if ((bool)Charge1)
		{
			SwitchFx(Charge1, false);
		}
		if ((bool)Charge2)
		{
			SwitchFx(Charge2, false);
		}
		if ((bool)RedAura)
		{
			SwitchFx(RedAura, false);
		}
		if ((bool)DashFX)
		{
			SwitchFx(DashFX, false);
		}
		if ((bool)Sk7FX1)
		{
			SwitchFx(Sk7FX1, false);
		}
		if ((bool)Sk8FX)
		{
			SwitchFx(Sk8FX, false);
		}
		if ((bool)Sk9FX1)
		{
			SwitchFx(Sk9FX1, false);
		}
		if ((bool)Sk9FX2)
		{
			SwitchFx(Sk9FX2, false);
		}
		if ((bool)Sk9FX3)
		{
			SwitchFx(Sk9FX3, false);
		}
		if ((bool)Sk9FX4)
		{
			SwitchFx(Sk9FX4, false);
		}
	}

	private void TakeSword(bool take)
	{
		HandSowrd.SetActive(take);
		BackSowrd.SetActive(!take);
		UsingSword = take;
	}

	private void TakeLSword(bool take)
	{
		LHandSowrd.SetActive(take);
		BackSowrd.SetActive(!take);
		UsingSword = take;
	}

	private void UseLGun(bool use)
	{
		HandGunL.SetActive(use);
		LHand.SetActive(!use);
		UsingLGun = use;
	}

	private void UseRGun(bool use)
	{
		HandGunR.SetActive(use);
		RHand.SetActive(!use);
		UsingRGun = use;
	}

	public override void UpdateEnemyID(int id)
	{
		base.UpdateEnemyID(id);
		UpdateAIState();
		switch (AiState)
		{
		case AI_STATE.mob_002:
			DefaultSkillCard = new int[6] { 0, 6, 7, 3, 4, 5 };
			DefaultRangedSkillCard = new int[3] { 3, 4, 5 };
			if ((bool)AuraFX2)
			{
				SwitchFx(AuraFX2, true);
			}
			break;
		case AI_STATE.mob_003:
			DefaultSkillCard = new int[6] { 0, 4, 5, 6, 7, 8 };
			DefaultRangedSkillCard = new int[3] { 4, 5, 8 };
			break;
		default:
			DefaultSkillCard = new int[6] { 0, 1, 2, 3, 4, 5 };
			DefaultRangedSkillCard = new int[3] { 3, 4, 5 };
			if ((bool)AuraFX2)
			{
				SwitchFx(AuraFX2, false);
			}
			break;
		}
	}

	private void SwitchFx(ParticleSystem Fx, bool onoff)
	{
		if ((bool)Fx)
		{
			if (onoff)
			{
				Fx.Play();
				return;
			}
			Fx.Stop();
			Fx.Clear();
		}
		else
		{
			Debug.Log(string.Concat("特效載入有誤，目前狀態是 ", _mainStatus, "的階段 ", _subStatus));
		}
	}

	private int GetWeaponID(UseSkill skill)
	{
		switch (AiState)
		{
		case AI_STATE.mob_001:
			switch (skill)
			{
			case UseSkill.BodyCollide:
				return 0;
			case UseSkill.Sk0Bullet1:
				return 1;
			case UseSkill.Sk0Bullet2:
				return 2;
			case UseSkill.Sk1Bullet:
				return 3;
			case UseSkill.Sk2Slash:
				return 4;
			case UseSkill.Sk3Bullet:
				return 5;
			case UseSkill.Sk4Bullet:
				return 6;
			case UseSkill.Sk5Bullet:
				return 7;
			}
			break;
		case AI_STATE.mob_002:
			switch (skill)
			{
			case UseSkill.BodyCollide:
				return 0;
			case UseSkill.Sk0Bullet1:
				return 1;
			case UseSkill.Sk0Bullet2:
				return 2;
			case UseSkill.Sk1Bullet:
				return 3;
			case UseSkill.Sk2Slash:
				return 4;
			case UseSkill.Sk3Bullet:
				return 5;
			case UseSkill.Sk4Bullet:
				return 6;
			case UseSkill.Sk5Bullet:
				return 7;
			case UseSkill.Sk6Bullet:
				return 8;
			case UseSkill.Sk4BulletEX:
				return 9;
			case UseSkill.Sk7Claw:
				return 10;
			}
			break;
		case AI_STATE.mob_003:
			switch (skill)
			{
			case UseSkill.BodyCollide:
				return 0;
			case UseSkill.Sk0Bullet1:
				return 1;
			case UseSkill.Sk0Bullet2:
				return 2;
			case UseSkill.Sk5Bullet:
				return 3;
			case UseSkill.Sk6Bullet:
				return 4;
			case UseSkill.Sk4BulletEX:
				return 5;
			case UseSkill.Sk7Claw:
				return 6;
			case UseSkill.Sk8Slash:
				return 7;
			case UseSkill.Sk9Slash:
				return 8;
			}
			break;
		}
		Debug.LogError("找不到對應的技能，檢查狀態是否正確。 現在狀態是：" + _mainStatus);
		return 0;
	}
}
