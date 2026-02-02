#define RELEASE
using System;
using CallbackDefs;
using CodeStage.AntiCheat.ObscuredTypes;
using NaughtyAttributes;
using Newtonsoft.Json;
using StageLib;
using UnityEngine;

public class BS095_Controller : EnemyControllerBase, IManagedUpdateBehavior
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
		MAX_SUBSTATUS = 9
	}

	public enum AnimationID
	{
		ANI_IDLE = 0,
		ANI_DEBUT = 1,
		ANI_DEBUT_LOOP = 2,
		ANI_DASH_START = 3,
		ANI_DASH_LOOP = 4,
		ANI_DASH_END = 5,
		ANI_JUMP_START1 = 6,
		ANI_JUMP_LOOP1 = 7,
		ANI_JUMP_START2 = 8,
		ANI_JUMP_LOOP2 = 9,
		ANI_JUMP_END2 = 10,
		ANI_SKILL0_LOOP1 = 11,
		ANI_SKILL0_LOOP2 = 12,
		ANI_SKILL1_START1 = 13,
		ANI_SKILL1_LOOP1 = 14,
		ANI_SKILL1_END1 = 15,
		ANI_SKILL1_START2 = 16,
		ANI_SKILL1_LOOP2 = 17,
		ANI_SKILL1_START3 = 18,
		ANI_SKILL1_LOOP3 = 19,
		ANI_SKILL1_END3 = 20,
		ANI_SKILL2_START = 21,
		ANI_SKILL2_LOOP = 22,
		ANI_SKILL2_END = 23,
		ANI_SKILL3_START1 = 24,
		ANI_SKILL3_LOOP1 = 25,
		ANI_SKILL3_START2 = 26,
		ANI_SKILL3_LOOP2 = 27,
		ANI_SKILL3_START3 = 28,
		ANI_SKILL3_LOOP3 = 29,
		ANI_SKILL3_END3 = 30,
		ANI_SKILL4_START1 = 31,
		ANI_SKILL4_LOOP1 = 32,
		ANI_SKILL4_START2 = 33,
		ANI_SKILL4_LOOP2 = 34,
		ANI_SKILL4_END2 = 35,
		ANI_SKILL5_START = 36,
		ANI_SKILL5_LOOP = 37,
		ANI_SKILL5_END = 38,
		ANI_SKILL6_START1 = 39,
		ANI_SKILL6_LOOP1 = 40,
		ANI_SKILL6_START2 = 41,
		ANI_SKILL6_LOOP2 = 42,
		ANI_SKILL6_END2 = 43,
		ANI_FALL = 44,
		ANI_LAND = 45,
		ANI_HURT = 46,
		ANI_DEAD = 47,
		ANI_DEAD_LOOP = 48,
		MAX_ANIMATION_ID = 49
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

	[Header("Debug用")]
	[SerializeField]
	private bool DebugMode;

	[SerializeField]
	private MainStatus NextSkill;

	private int nDeadCount;

	private int[] _animationHash;

	private Vector3 LastTargetPos = new Vector3(0f, 0f, 0f);

	[Header("Mesh")]
	[SerializeField]
	private SkinnedMeshRenderer HandMeshL;

	[SerializeField]
	private SkinnedMeshRenderer HandMeshR;

	[SerializeField]
	private SkinnedMeshRenderer GunMeshL;

	[SerializeField]
	private SkinnedMeshRenderer GunMeshR;

	[Header("待機")]
	[SerializeField]
	private float IdleWaitTime = 0.2f;

	private int IdleWaitFrame;

	[Header("通用")]
	private Vector3 StartPos;

	private Vector3 EndPos;

	private float CenterXPos;

	private int ActionTimes;

	private float ActionAnimatorFrame;

	private int ActionFrame;

	private bool HasActed;

	private float ShotAngle;

	private readonly int _HashAngle = Animator.StringToHash("Angle");

	[SerializeField]
	private int JumpSpeed = Mathf.RoundToInt(OrangeBattleUtility.PlayerJumpSpeed * OrangeBattleUtility.PPU * OrangeBattleUtility.FPS * 1000f);

	private int DashSpeed = Mathf.RoundToInt(OrangeBattleUtility.PlayerDashSpeed * OrangeBattleUtility.PPU * OrangeBattleUtility.FPS * 1000f);

	[SerializeField]
	private float DashSpeedMulti = 1.4f;

	private int WalkSpeed = Mathf.RoundToInt(OrangeBattleUtility.PlayerWalkSpeed * OrangeBattleUtility.PPU * OrangeBattleUtility.FPS * 1000f);

	[SerializeField]
	private Transform ShootPointL;

	[SerializeField]
	private Transform ShootPointR;

	[Header("手砲攻擊")]
	[SerializeField]
	private int Skill0ShootTimes = 10;

	[SerializeField]
	private float Skill0ShootFrame;

	[SerializeField]
	private float Skill0ChargeTime1 = 0.5f;

	[SerializeField]
	private ParticleSystem Skill0ChargeFX1;

	[SerializeField]
	private ParticleSystem Skill0ChargeFX2;

	[Header("抓取")]
	private CollideBullet Skill1Collide;

	[SerializeField]
	private string Skill1CollideObjName = "Skill1Collide";

	[SerializeField]
	private Transform CatchTransform;

	[SerializeField]
	private CatchPlayerTool CatchTool;

	[SerializeField]
	private string DashFXName = "OBJ_DASH_SMOKE";

	[SerializeField]
	private int Skill1ThrowSpeed = 15000;

	[SerializeField]
	private float Skill1ThrowTime = 1f;

	[SerializeField]
	private ParticleSystem Skill1ChipFX;

	[SerializeField]
	private ParticleSystem Skill1ThunderFX;

	[Header("一甲頭槌")]
	[SerializeField]
	private int Skill2JumpSpeed = 18000;

	[SerializeField]
	private int Skill2MoveSpeed = 5000;

	[SerializeField]
	private ParticleSystem Skill2FX;

	[Header("二甲爆破")]
	[SerializeField]
	private int Skill3JumpSpeed = 18000;

	[SerializeField]
	private float Skill3ChargeTime = 1f;

	[SerializeField]
	private float Skill3AtkTime = 1f;

	[SerializeField]
	private ParticleSystem Skill3FX1;

	[SerializeField]
	private ParticleSystem Skill3FX2;

	[Header("三甲集氣砲")]
	[SerializeField]
	private int Skill4JumpSpeed = 12000;

	[SerializeField]
	private float Skill4ChargeTime = 1f;

	[SerializeField]
	private ParticleSystem Skill4ChargeFX1;

	[SerializeField]
	private ParticleSystem Skill4ChargeFX2;

	[Header("究甲集氣砲")]
	[SerializeField]
	private float Skill5ChargeTime = 1f;

	[SerializeField]
	private ParticleSystem Skill5ChargeFX;

	[Header("究甲衝刺")]
	[SerializeField]
	private float Skill6StrikeTime = 0.8f;

	[SerializeField]
	private int Skill6StrikeSpeed = 12000;

	[SerializeField]
	private int Skill6JumpSpeed = 12000;

	[SerializeField]
	private ParticleSystem Skill6FX1;

	[SerializeField]
	private ParticleSystem Skill6FX2;

	[Header("死亡")]
	[SerializeField]
	private string TeleportOutFxName = "fx_bs086_teleport_out";

	[Header("AI流程控制")]
	private bool IsUltimate;

	private int BuffState;

	[SerializeField]
	private int MaxBuffState = 3;

	private bool UseSkill4;

	private CollideBulletHitSelf BuffCollide1;

	private CollideBulletHitSelf BuffCollide2;

	[SerializeField]
	private string BuffCollideName1 = "BuffCollide1";

	[SerializeField]
	private string BuffCollideName2 = "BuffCollide2";

	private bool UseUltimate;

	private bool HaveNear;

	private bool HaveMid;

	[SerializeField]
	private int MaxFarDisTimes = 4;

	private int FarDisCount;

	[SerializeField]
	private int MaxCatchFailTimes = 2;

	private int CatchFailCount;

	[SerializeField]
	private float FarDistance = 6f;

	[SerializeField]
	private float MidDistance = 3f;

	[SerializeField]
	private float Skill2HighDis = 4f;

	private Vector3 NowPos
	{
		get
		{
			return _transform.position;
		}
	}

	private Vector3 ShootPosL
	{
		get
		{
			return ShootPointL.position + Vector3.right * base.direction;
		}
	}

	private Vector3 ShootPosR
	{
		get
		{
			return ShootPointR.position + Vector3.right * base.direction;
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
			}
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

	protected virtual void HashAnimation()
	{
		_animationHash = new int[49];
		_animationHash[0] = Animator.StringToHash("BS095@idle_loop");
		_animationHash[1] = Animator.StringToHash("BS095@debut");
		_animationHash[2] = Animator.StringToHash("BS095@debut_standby_loop");
		_animationHash[3] = Animator.StringToHash("BS095@dash_start");
		_animationHash[4] = Animator.StringToHash("BS095@dash_loop");
		_animationHash[5] = Animator.StringToHash("BS095@dash_end");
		_animationHash[6] = Animator.StringToHash("BS095@jump_start");
		_animationHash[7] = Animator.StringToHash("BS095@jump_loop");
		_animationHash[8] = Animator.StringToHash("BS095@fall_start");
		_animationHash[9] = Animator.StringToHash("BS095@fall_loop");
		_animationHash[10] = Animator.StringToHash("BS095@landing");
		_animationHash[11] = Animator.StringToHash("BS095@skill_01_run_loop");
		_animationHash[12] = Animator.StringToHash("BS095@skill_01_stand");
		_animationHash[13] = Animator.StringToHash("BS095@skill_02_step1_start");
		_animationHash[14] = Animator.StringToHash("BS095@skill_02_step1_loop");
		_animationHash[15] = Animator.StringToHash("BS095@skill_02_step1_end");
		_animationHash[16] = Animator.StringToHash("BS095@skill_02_step2_start");
		_animationHash[17] = Animator.StringToHash("BS095@skill_02_step2_loop");
		_animationHash[18] = Animator.StringToHash("BS095@skill_02_step3_start");
		_animationHash[19] = Animator.StringToHash("BS095@skill_02_step3_loop");
		_animationHash[20] = Animator.StringToHash("BS095@skill_02_step3_end");
		_animationHash[21] = Animator.StringToHash("BS095@skill_03_start");
		_animationHash[22] = Animator.StringToHash("BS095@skill_03_loop");
		_animationHash[23] = Animator.StringToHash("BS095@skill_03_end");
		_animationHash[24] = Animator.StringToHash("BS095@skill_04_step1_start");
		_animationHash[25] = Animator.StringToHash("BS095@skill_04_step1_loop");
		_animationHash[26] = Animator.StringToHash("BS095@skill_04_step2_start");
		_animationHash[27] = Animator.StringToHash("BS095@skill_04_step2_loop");
		_animationHash[28] = Animator.StringToHash("BS095@skill_04_step3_start");
		_animationHash[29] = Animator.StringToHash("BS095@skill_04_step3_loop");
		_animationHash[30] = Animator.StringToHash("BS095@skill_04_step3_end");
		_animationHash[31] = Animator.StringToHash("BS095@skill_05_step1_start");
		_animationHash[32] = Animator.StringToHash("BS095@skill_05_step1_loop");
		_animationHash[33] = Animator.StringToHash("BS095@skill_05_step2_start");
		_animationHash[34] = Animator.StringToHash("BS095@skill_05_step2_loop");
		_animationHash[35] = Animator.StringToHash("BS095@skill_05_step2_end");
		_animationHash[36] = Animator.StringToHash("BS095@skill_06_stand_start");
		_animationHash[37] = Animator.StringToHash("BS095@skill_06_stand_loop");
		_animationHash[38] = Animator.StringToHash("BS095@skill_06_stand_end");
		_animationHash[39] = Animator.StringToHash("BS095@skill_07_step1_start");
		_animationHash[40] = Animator.StringToHash("BS095@skill_07_step1_loop");
		_animationHash[41] = Animator.StringToHash("BS095@skill_07_step2_start");
		_animationHash[42] = Animator.StringToHash("BS095@skill_07_step2_loop");
		_animationHash[43] = Animator.StringToHash("BS095@skill_07_step2_end");
		_animationHash[44] = Animator.StringToHash("BS095@fall_loop");
		_animationHash[45] = Animator.StringToHash("BS095@landing");
		_animationHash[46] = Animator.StringToHash("BS095@hurt_loop");
		_animationHash[47] = Animator.StringToHash("BS095@dead");
		_animationHash[48] = Animator.StringToHash("BS095@dead_step1");
	}

	private void LoadParts(ref Transform[] childs)
	{
		ModelTransform = OrangeBattleUtility.FindChildRecursive(ref childs, "model", true);
		_animator = ModelTransform.GetComponent<Animator>();
		CatchTool = _transform.GetComponent<CatchPlayerTool>();
		_collideBullet = OrangeBattleUtility.FindChildRecursive(ref childs, "Collider", true).gameObject.AddOrGetComponent<CollideBullet>();
		Skill1Collide = OrangeBattleUtility.FindChildRecursive(ref childs, Skill1CollideObjName, true).gameObject.AddOrGetComponent<CollideBullet>();
		BuffCollide1 = OrangeBattleUtility.FindChildRecursive(ref childs, BuffCollideName1, true).gameObject.AddOrGetComponent<CollideBulletHitSelf>();
		BuffCollide2 = OrangeBattleUtility.FindChildRecursive(ref childs, BuffCollideName2, true).gameObject.AddOrGetComponent<CollideBulletHitSelf>();
		if (HandMeshL == null)
		{
			HandMeshL = OrangeBattleUtility.FindChildRecursive(ref childs, "HandMesh_L_c", true).gameObject.AddOrGetComponent<SkinnedMeshRenderer>();
		}
		if (HandMeshR == null)
		{
			HandMeshR = OrangeBattleUtility.FindChildRecursive(ref childs, "HandMesh_R_c", true).gameObject.AddOrGetComponent<SkinnedMeshRenderer>();
		}
		if (GunMeshL == null)
		{
			GunMeshL = OrangeBattleUtility.FindChildRecursive(ref childs, "BusterMesh_L_m", true).gameObject.AddOrGetComponent<SkinnedMeshRenderer>();
		}
		if (GunMeshR == null)
		{
			GunMeshR = OrangeBattleUtility.FindChildRecursive(ref childs, "BusterMesh_R_m", true).gameObject.AddOrGetComponent<SkinnedMeshRenderer>();
		}
		if (Skill0ChargeFX1 == null)
		{
			Skill0ChargeFX1 = OrangeBattleUtility.FindChildRecursive(ref childs, "Skill0ChargeFX1", true).gameObject.AddOrGetComponent<ParticleSystem>();
		}
		if (Skill0ChargeFX2 == null)
		{
			Skill0ChargeFX2 = OrangeBattleUtility.FindChildRecursive(ref childs, "Skill0ChargeFX2", true).gameObject.AddOrGetComponent<ParticleSystem>();
		}
		if (Skill1ChipFX == null)
		{
			Skill1ChipFX = OrangeBattleUtility.FindChildRecursive(ref childs, "Skill1ChipFX", true).gameObject.AddOrGetComponent<ParticleSystem>();
		}
		if (Skill1ThunderFX == null)
		{
			Skill1ThunderFX = OrangeBattleUtility.FindChildRecursive(ref childs, "Skill1ThunderFX", true).gameObject.AddOrGetComponent<ParticleSystem>();
		}
		if (Skill2FX == null)
		{
			Skill2FX = OrangeBattleUtility.FindChildRecursive(ref childs, "Skill2FX", true).gameObject.AddOrGetComponent<ParticleSystem>();
		}
		if (Skill3FX1 == null)
		{
			Skill3FX1 = OrangeBattleUtility.FindChildRecursive(ref childs, "Skill3FX1", true).gameObject.AddOrGetComponent<ParticleSystem>();
		}
		if (Skill3FX2 == null)
		{
			Skill3FX2 = OrangeBattleUtility.FindChildRecursive(ref childs, "Skill3FX2", true).gameObject.AddOrGetComponent<ParticleSystem>();
		}
		if (Skill4ChargeFX1 == null)
		{
			Skill4ChargeFX1 = OrangeBattleUtility.FindChildRecursive(ref childs, "Skill4ChargeFX1", true).gameObject.AddOrGetComponent<ParticleSystem>();
		}
		if (Skill4ChargeFX2 == null)
		{
			Skill4ChargeFX2 = OrangeBattleUtility.FindChildRecursive(ref childs, "Skill4ChargeFX2", true).gameObject.AddOrGetComponent<ParticleSystem>();
		}
		if (Skill5ChargeFX == null)
		{
			Skill5ChargeFX = OrangeBattleUtility.FindChildRecursive(ref childs, "Skill5ChargeFX", true).gameObject.AddOrGetComponent<ParticleSystem>();
		}
		if (Skill6FX1 == null)
		{
			Skill6FX1 = OrangeBattleUtility.FindChildRecursive(ref childs, "Skill6FX1", true).gameObject.AddOrGetComponent<ParticleSystem>();
		}
		if (Skill6FX2 == null)
		{
			Skill6FX2 = OrangeBattleUtility.FindChildRecursive(ref childs, "Skill6FX2", true).gameObject.AddOrGetComponent<ParticleSystem>();
		}
		if (CatchTransform == null)
		{
			CatchTransform = OrangeBattleUtility.FindChildRecursive(ref childs, "L WeaponPoint", true);
		}
		if (ShootPointL == null)
		{
			ShootPointL = OrangeBattleUtility.FindChildRecursive(ref childs, "L BusterPoint", true);
		}
		if (ShootPointR == null)
		{
			ShootPointR = OrangeBattleUtility.FindChildRecursive(ref childs, "R BusterPoint", true);
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
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(DashFXName, 3);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(TeleportOutFxName, 3);
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
				IgnoreGravity = true;
				break;
			case SubStatus.Phase1:
				_velocity.y = -1500;
				break;
			case SubStatus.Phase2:
				IgnoreGravity = false;
				break;
			case SubStatus.Phase4:
				IdleWaitFrame = GameLogicUpdateManager.GameFrame + 20;
				break;
			}
			break;
		case MainStatus.Idle:
			SwitchMesh(HandMeshL, true);
			SwitchMesh(GunMeshL, false);
			_velocity.x = 0;
			IdleWaitFrame = GameLogicUpdateManager.GameFrame + (int)(IdleWaitTime * 20f);
			break;
		case MainStatus.Skill0:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				ActionTimes = Skill0ShootTimes;
				SetStatus(MainStatus.Skill0, SubStatus.Phase2);
				SwitchFx(Skill0ChargeFX1, true);
				base.SoundSource.PlaySE("BossSE04", "bs040_kai00");
				return;
			case SubStatus.Phase1:
				HasActed = false;
				ActionAnimatorFrame = Skill0ShootFrame;
				break;
			case SubStatus.Phase2:
				SwitchMesh(HandMeshL, false);
				SwitchMesh(GunMeshL, true);
				HasActed = false;
				if (ActionTimes < 2)
				{
					SwitchFx(Skill0ChargeFX1, false);
					SwitchFx(Skill0ChargeFX2, true);
				}
				ActionAnimatorFrame = Skill0ShootFrame;
				break;
			}
			break;
		case MainStatus.Skill1:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				CatchTool.CatchTransform = CatchTransform;
				break;
			case SubStatus.Phase1:
				PlaySE("BossSE04", "bs040_kai12");
				_velocity.x = (int)((float)DashSpeed * DashSpeedMulti) * base.direction;
				EndPos = GetTargetPos();
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(DashFXName, ModelTransform, Quaternion.identity, Array.Empty<object>());
				break;
			case SubStatus.Phase2:
				PlaySE("BossSE04", "bs040_kai03");
				SwitchFx(Skill1ThunderFX, true);
				CatchFailCount = 0;
				_velocity = VInt3.zero;
				CatchTool.PosOffset = new Vector3(0f, 0f - CatchTool.TargetOC.Controller.Collider2D.size.y + 0.2f, 0f);
				break;
			case SubStatus.Phase3:
				SwitchFx(Skill1ChipFX, true);
				break;
			case SubStatus.Phase4:
				PlaySE("BossSE04", "bs040_kai04");
				Skill1Collide.Active(targetMask);
				break;
			case SubStatus.Phase5:
				if (BuffState < MaxBuffState && !IsUltimate)
				{
					BuffState++;
				}
				if (BuffState >= MaxBuffState && UseUltimate)
				{
					UseUltimate = false;
				}
				ActionFrame = GameLogicUpdateManager.GameFrame + (int)(Skill1ThrowTime * 20f);
				break;
			case SubStatus.Phase6:
			{
				if (Skill1Collide.IsActivate)
				{
					Skill1Collide.BackToPool();
				}
				int layerMask = (1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockLayer) | (1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockEnemyLayer) | (1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockPlayerLayer) | (1 << ManagedSingleton<OrangeLayerManager>.Instance.WallKickMask);
				Vector3 vector = NowPos + Vector3.up;
				RaycastHit2D raycastHit2D = OrangeBattleUtility.RaycastIgnoreSelf(vector, Vector3.left, 30f, layerMask, _transform);
				RaycastHit2D raycastHit2D2 = OrangeBattleUtility.RaycastIgnoreSelf(vector, Vector3.right, 30f, layerMask, _transform);
				if (Mathf.Abs(NowPos.x - raycastHit2D.point.x) > Mathf.Abs(NowPos.x - raycastHit2D2.point.x))
				{
					UpdateDirection(1);
				}
				else
				{
					UpdateDirection(-1);
				}
				_velocity.x = (int)((float)DashSpeed * DashSpeedMulti) * base.direction;
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(DashFXName, ModelTransform, Quaternion.identity, Array.Empty<object>());
				PlaySE("BossSE04", "bs040_kai12");
				break;
			}
			case SubStatus.Phase7:
				CatchFailCount++;
				_velocity = VInt3.zero;
				PlaySE("BossSE04", "bs040_kai13");
				break;
			}
			break;
		case MainStatus.Skill2:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				EndPos = GetTargetPos();
				if (EndPos.y - NowPos.y < Skill2HighDis)
				{
					SetStatus(MainStatus.Skill2, SubStatus.Phase2);
					return;
				}
				_velocity.y = JumpSpeed;
				base.SoundSource.PlaySE("BossSE04", "bs040_kai16");
				break;
			case SubStatus.Phase2:
				PlaySE("BossSE04", "bs040_kai05");
				base.SoundSource.PlaySE("BossSE04", "bs040_kai16");
				_collideBullet.UpdateBulletData(EnemyWeapons[4].BulletData);
				_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
				_collideBullet.Active(targetMask);
				_velocity.y = Skill2JumpSpeed;
				_velocity.x = Skill2MoveSpeed * base.direction;
				break;
			case SubStatus.Phase3:
				SwitchFx(Skill2FX, true);
				break;
			case SubStatus.Phase4:
				_collideBullet.UpdateBulletData(EnemyWeapons[0].BulletData);
				_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
				_collideBullet.Active(targetMask);
				_velocity.x = 0;
				SwitchFx(Skill2FX, false);
				break;
			case SubStatus.Phase6:
				PlaySE("BossSE04", "bs040_kai14");
				break;
			}
			break;
		case MainStatus.Skill3:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_velocity.y = Skill3JumpSpeed;
				base.SoundSource.PlaySE("BossSE04", "bs040_kai16");
				break;
			case SubStatus.Phase2:
				IsInvincible = true;
				IgnoreGravity = true;
				break;
			case SubStatus.Phase3:
				PlaySE("BossSE04", "bs040_kai06");
				SwitchFx(Skill3FX1, true);
				ActionFrame = GameLogicUpdateManager.GameFrame + (int)(Skill3ChargeTime * 20f);
				break;
			case SubStatus.Phase4:
				PlaySE("BossSE04", "bs040_kai07");
				SwitchFx(Skill3FX2, true);
				break;
			case SubStatus.Phase5:
				_collideBullet.UpdateBulletData(EnemyWeapons[5].BulletData);
				_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
				_collideBullet.Active(targetMask);
				_collideBullet._transform.localScale = Vector3.up;
				LeanTween.scale(_collideBullet.gameObject, Vector3.one * 0.7f, 1f);
				ActionFrame = GameLogicUpdateManager.GameFrame + (int)(Skill3AtkTime * 20f);
				break;
			case SubStatus.Phase6:
				_collideBullet.UpdateBulletData(EnemyWeapons[0].BulletData);
				_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
				_collideBullet.Active(targetMask);
				IgnoreGravity = false;
				IsInvincible = false;
				break;
			case SubStatus.Phase8:
				PlaySE("BossSE04", "bs040_kai14");
				break;
			}
			break;
		case MainStatus.Skill4:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				SwitchFx(Skill4ChargeFX1, true);
				_velocity.y = Skill4JumpSpeed;
				base.SoundSource.PlaySE("BossSE04", "bs040_kai16");
				base.SoundSource.PlaySE("BossSE04", "bs040_kai17");
				break;
			case SubStatus.Phase2:
				IgnoreGravity = true;
				SwitchMesh(HandMeshR, false);
				SwitchMesh(GunMeshR, true);
				SwitchFx(Skill4ChargeFX1, false);
				SwitchFx(Skill4ChargeFX2, true);
				break;
			case SubStatus.Phase3:
				BulletBase.TryShotBullet(EnemyWeapons[6].BulletData, ShootPosR, Vector3.right * base.direction, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
				break;
			case SubStatus.Phase4:
				SwitchMesh(HandMeshL, false);
				SwitchMesh(GunMeshL, true);
				SwitchMesh(HandMeshR, true);
				SwitchMesh(GunMeshR, false);
				break;
			case SubStatus.Phase5:
				BulletBase.TryShotBullet(EnemyWeapons[7].BulletData, ShootPosL, Vector3.right * base.direction, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
				break;
			case SubStatus.Phase6:
				IgnoreGravity = false;
				SwitchMesh(HandMeshL, true);
				SwitchMesh(GunMeshL, false);
				SwitchFx(Skill4ChargeFX2, false);
				break;
			case SubStatus.Phase7:
				PlaySE("BossSE04", "bs040_kai14");
				break;
			}
			break;
		case MainStatus.Skill5:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				SwitchMesh(HandMeshL, false);
				SwitchMesh(GunMeshL, true);
				SwitchFx(Skill5ChargeFX, true);
				base.SoundSource.PlaySE("BossSE04", "bs040_kai00");
				break;
			case SubStatus.Phase1:
				ActionFrame = GameLogicUpdateManager.GameFrame + (int)(Skill5ChargeTime * 20f);
				break;
			case SubStatus.Phase2:
				EndPos = GetTargetPos();
				ShotAngle = Vector2.Angle(Vector2.up, EndPos - ShootPosL);
				_animator.SetFloat(_HashAngle, ShotAngle);
				BulletBase.TryShotBullet(EnemyWeapons[8].BulletData, ShootPosL, EndPos - ShootPosL, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
				SwitchFx(Skill5ChargeFX, false);
				break;
			}
			break;
		case MainStatus.Skill6:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_velocity.y = Skill6JumpSpeed;
				break;
			case SubStatus.Phase2:
				_collideBullet.UpdateBulletData(EnemyWeapons[9].BulletData);
				_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
				_collideBullet.Active(targetMask);
				IgnoreGravity = true;
				break;
			case SubStatus.Phase3:
				SwitchFx(Skill6FX1, true);
				SwitchFx(Skill6FX2, true);
				_velocity.x = Skill6StrikeSpeed * base.direction;
				ActionFrame = GameLogicUpdateManager.GameFrame + (int)(Skill6StrikeTime * 20f);
				break;
			case SubStatus.Phase4:
				_collideBullet.UpdateBulletData(EnemyWeapons[0].BulletData);
				_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
				_collideBullet.Active(targetMask);
				_velocity = VInt3.zero;
				IgnoreGravity = false;
				break;
			case SubStatus.Phase6:
				PlaySE("BossSE04", "bs040_kai14");
				break;
			}
			break;
		case MainStatus.Skill7:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				IsUltimate = true;
				BuffCollide1.Active(friendMask);
				BuffCollide2.Active(friendMask);
				Skill1Collide.Active(targetMask);
				SwitchFx(Skill1ThunderFX, true);
				break;
			case SubStatus.Phase1:
				IsUltimate = true;
				SwitchFx(Skill1ChipFX, true);
				ActionFrame = GameLogicUpdateManager.GameFrame + (int)(Skill1ThrowTime * 20f);
				break;
			case SubStatus.Phase2:
				IsUltimate = true;
				break;
			}
			break;
		case MainStatus.Skill8:
			switch (_subStatus)
			{
			case SubStatus.Phase1:
				_velocity.x = (int)((float)DashSpeed * DashSpeedMulti) * base.direction;
				EndPos = GetTargetPos();
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(DashFXName, ModelTransform, Quaternion.identity, Array.Empty<object>());
				break;
			case SubStatus.Phase2:
				_velocity = VInt3.zero;
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
				EndPos = GetTargetPos();
				UpdateDirection();
				IgnoreGravity = false;
				if (!Controller.Collisions.below)
				{
					SetStatus(MainStatus.Die, SubStatus.Phase3);
				}
				break;
			case SubStatus.Phase1:
				_velocity = VInt3.zero;
				break;
			case SubStatus.Phase2:
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(TeleportOutFxName, Controller.GetRealCenterPos(), Quaternion.identity, new object[1] { Vector3.one });
				BackToPool();
				break;
			case SubStatus.Phase3:
				_velocity.x = 2500 * -base.direction;
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
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_DEBUT_LOOP;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_DEBUT;
				break;
			case SubStatus.Phase3:
				_currentAnimationId = AnimationID.ANI_IDLE;
				break;
			case SubStatus.Phase1:
				return;
			}
			break;
		case MainStatus.Idle:
			_currentAnimationId = AnimationID.ANI_IDLE;
			break;
		case MainStatus.Skill0:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				return;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_SKILL0_LOOP1;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_SKILL0_LOOP2;
				break;
			}
			break;
		case MainStatus.Skill1:
			switch (_subStatus)
			{
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
				_currentAnimationId = AnimationID.ANI_SKILL1_LOOP2;
				break;
			case SubStatus.Phase4:
				_currentAnimationId = AnimationID.ANI_SKILL1_START3;
				break;
			case SubStatus.Phase5:
				_currentAnimationId = AnimationID.ANI_SKILL1_LOOP3;
				break;
			case SubStatus.Phase6:
				_currentAnimationId = AnimationID.ANI_SKILL1_LOOP1;
				break;
			case SubStatus.Phase7:
				_currentAnimationId = AnimationID.ANI_SKILL1_END1;
				break;
			}
			break;
		case MainStatus.Skill2:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_JUMP_START1;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_JUMP_LOOP1;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_SKILL2_START;
				break;
			case SubStatus.Phase3:
				_currentAnimationId = AnimationID.ANI_SKILL2_LOOP;
				break;
			case SubStatus.Phase4:
				_currentAnimationId = AnimationID.ANI_SKILL2_END;
				break;
			case SubStatus.Phase5:
				_currentAnimationId = AnimationID.ANI_FALL;
				break;
			case SubStatus.Phase6:
				_currentAnimationId = AnimationID.ANI_LAND;
				break;
			case SubStatus.Phase7:
				_currentAnimationId = AnimationID.ANI_DASH_LOOP;
				break;
			case SubStatus.Phase8:
				_currentAnimationId = AnimationID.ANI_DASH_END;
				break;
			}
			break;
		case MainStatus.Skill3:
			switch (_subStatus)
			{
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
			case SubStatus.Phase7:
				_currentAnimationId = AnimationID.ANI_FALL;
				break;
			case SubStatus.Phase8:
				_currentAnimationId = AnimationID.ANI_LAND;
				break;
			}
			break;
		case MainStatus.Skill4:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_JUMP_START1;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_JUMP_LOOP1;
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
				_currentAnimationId = AnimationID.ANI_FALL;
				break;
			case SubStatus.Phase7:
				_currentAnimationId = AnimationID.ANI_LAND;
				break;
			}
			break;
		case MainStatus.Skill5:
			switch (_subStatus)
			{
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
				_currentAnimationId = AnimationID.ANI_SKILL6_START1;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_SKILL6_LOOP1;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_SKILL6_START2;
				break;
			case SubStatus.Phase3:
				_currentAnimationId = AnimationID.ANI_SKILL6_LOOP2;
				break;
			case SubStatus.Phase4:
				_currentAnimationId = AnimationID.ANI_SKILL6_END2;
				break;
			case SubStatus.Phase5:
				_currentAnimationId = AnimationID.ANI_FALL;
				break;
			case SubStatus.Phase6:
				_currentAnimationId = AnimationID.ANI_LAND;
				break;
			}
			break;
		case MainStatus.Skill7:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_SKILL1_START3;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_SKILL1_LOOP3;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_SKILL1_END3;
				break;
			}
			break;
		case MainStatus.Skill8:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_SKILL1_START1;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_SKILL1_LOOP1;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_SKILL1_END1;
				break;
			}
			break;
		case MainStatus.Die:
			switch (_subStatus)
			{
			default:
				return;
			case SubStatus.Phase0:
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_IDLE;
				break;
			case SubStatus.Phase3:
				_currentAnimationId = AnimationID.ANI_HURT;
				break;
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
				mainStatus = ((UseUltimate && !IsUltimate) ? MainStatus.Skill7 : (Target ? ((!(Mathf.Abs(Target._transform.position.x - NowPos.x) > FarDistance)) ? ((!(Mathf.Abs(Target._transform.position.x - NowPos.x) > MidDistance)) ? ChooseNear() : ChooseMid()) : ChooseFar()) : MainStatus.Skill0));
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
				if (_introReady)
				{
					SetStatus(MainStatus.Debut, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if (Controller.Collisions.below)
				{
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
					SetStatus(MainStatus.Debut, SubStatus.Phase4);
				}
				break;
			case SubStatus.Phase4:
				if (!bWaitNetStatus && IdleWaitFrame < GameLogicUpdateManager.GameFrame)
				{
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
					UpdateRandomState(MainStatus.Skill8);
				}
			}
			break;
		case MainStatus.Skill0:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				SetStatus(MainStatus.Skill0, SubStatus.Phase2);
				break;
			case SubStatus.Phase2:
				if (!HasActed && _currentFrame > ActionAnimatorFrame)
				{
					SwitchFx(Skill0ChargeFX2, false);
					HasActed = true;
					EndPos = GetTargetPos();
					ShotAngle = Vector2.Angle(Vector2.up, EndPos - ShootPosL);
					_animator.SetFloat(_HashAngle, ShotAngle);
					Vector3 pDirection = EndPos - ShootPosL;
					if (ActionTimes == 1)
					{
						BulletBase.TryShotBullet(EnemyWeapons[2].BulletData, ShootPosL, pDirection, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
					}
					else
					{
						BulletBase.TryShotBullet(EnemyWeapons[1].BulletData, ShootPosL, pDirection, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
					}
				}
				if (_currentFrame > 1f)
				{
					if (--ActionTimes > 0)
					{
						SetStatus(MainStatus.Skill0, _subStatus);
					}
					else
					{
						SetStatus(MainStatus.Idle);
					}
				}
				break;
			case SubStatus.Phase1:
				break;
			}
			break;
		case MainStatus.Skill1:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill1, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase2:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill1, SubStatus.Phase3);
				}
				break;
			case SubStatus.Phase3:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill1, SubStatus.Phase4);
				}
				break;
			case SubStatus.Phase4:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill1, SubStatus.Phase5);
				}
				break;
			case SubStatus.Phase5:
				if (CatchTool.IsCatching && GameLogicUpdateManager.GameFrame > ActionFrame)
				{
					CatchTool.ReleaseTarget();
				}
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill1, SubStatus.Phase6);
				}
				break;
			case SubStatus.Phase6:
				if (CatchTool.IsCatching && GameLogicUpdateManager.GameFrame > ActionFrame)
				{
					CatchTool.ReleaseTarget();
				}
				if (Controller.Collisions.right || Controller.Collisions.left)
				{
					SetStatus(MainStatus.Skill1, SubStatus.Phase7);
				}
				break;
			case SubStatus.Phase7:
				if (_currentFrame > 1f)
				{
					UpdateDirection(-base.direction);
					SetStatus(MainStatus.Idle);
				}
				break;
			case SubStatus.Phase1:
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
				if (_velocity.y <= 0)
				{
					_collideBullet.UpdateBulletData(EnemyWeapons[4].BulletData);
					_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
					_collideBullet.Active(targetMask);
					_velocity.y = Skill2JumpSpeed;
					_velocity.x = Skill2MoveSpeed * base.direction;
					PlaySE("BossSE04", "bs040_kai05");
					SetStatus(MainStatus.Skill2, SubStatus.Phase3);
				}
				break;
			case SubStatus.Phase2:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill2, SubStatus.Phase3);
				}
				break;
			case SubStatus.Phase3:
				if (_velocity.y <= 0)
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
				if (Controller.Collisions.below)
				{
					SetStatus(MainStatus.Skill2, SubStatus.Phase6);
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
				if (_velocity.y <= 1000)
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
				if (GameLogicUpdateManager.GameFrame > ActionFrame)
				{
					SetStatus(MainStatus.Skill3, SubStatus.Phase4);
				}
				break;
			case SubStatus.Phase4:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill3, SubStatus.Phase5);
				}
				break;
			case SubStatus.Phase5:
				if (GameLogicUpdateManager.GameFrame > ActionFrame)
				{
					SetStatus(MainStatus.Skill3, SubStatus.Phase6);
				}
				break;
			case SubStatus.Phase6:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill3, SubStatus.Phase7);
				}
				break;
			case SubStatus.Phase7:
				if (Controller.Collisions.below)
				{
					SetStatus(MainStatus.Skill3, SubStatus.Phase8);
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
		case MainStatus.Skill4:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (_currentFrame > 1f)
				{
					if (_velocity.y <= 1000)
					{
						SetStatus(MainStatus.Skill4, SubStatus.Phase2);
					}
					else
					{
						SetStatus(MainStatus.Skill4, SubStatus.Phase1);
					}
				}
				break;
			case SubStatus.Phase1:
				if (_velocity.y <= 1000)
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
				if (_currentFrame > 2f)
				{
					EndPos = GetTargetPos();
					ShotAngle = Vector2.Angle(Vector2.up, EndPos - ShootPosL);
					_animator.SetFloat(_HashAngle, ShotAngle);
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
				if (_currentFrame > 2f)
				{
					EndPos = GetTargetPos();
					ShotAngle = Vector2.Angle(Vector2.up, EndPos - ShootPosL);
					_animator.SetFloat(_HashAngle, ShotAngle);
					SetStatus(MainStatus.Skill4, SubStatus.Phase6);
				}
				break;
			case SubStatus.Phase6:
				if (Controller.Collisions.below)
				{
					SetStatus(MainStatus.Skill4, SubStatus.Phase7);
				}
				break;
			case SubStatus.Phase7:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Idle);
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
				if (GameLogicUpdateManager.GameFrame > ActionFrame)
				{
					EndPos = GetTargetPos();
					ShotAngle = Vector2.Angle(Vector2.up, EndPos - ShootPosL);
					_animator.SetFloat(_HashAngle, ShotAngle);
					SetStatus(MainStatus.Skill5, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				if (_currentFrame > 1f)
				{
					SwitchMesh(HandMeshL, true);
					SwitchMesh(GunMeshL, false);
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
				if (_velocity.y <= 1000)
				{
					SetStatus(MainStatus.Skill6, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill6, SubStatus.Phase3);
				}
				break;
			case SubStatus.Phase3:
				if (GameLogicUpdateManager.GameFrame > ActionFrame)
				{
					SetStatus(MainStatus.Skill6, SubStatus.Phase4);
				}
				break;
			case SubStatus.Phase4:
				if (Controller.Collisions.below)
				{
					SetStatus(MainStatus.Skill6, SubStatus.Phase6);
				}
				else if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill6, SubStatus.Phase5);
				}
				break;
			case SubStatus.Phase5:
				if (Controller.Collisions.below)
				{
					SetStatus(MainStatus.Skill6, SubStatus.Phase6);
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
		case MainStatus.Skill7:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill1, SubStatus.Phase5);
				}
				break;
			case SubStatus.Phase1:
				if (_currentFrame > 1.5f)
				{
					SetStatus(MainStatus.Skill1, SubStatus.Phase6);
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
					PlaySE("BossSE04", "bs040_kai12");
				}
				break;
			case SubStatus.Phase1:
				if ((NowPos.x - EndPos.x) * (float)base.direction > 0.5f || (base.direction == 1 && Controller.Collisions.right) || (base.direction == -1 && Controller.Collisions.left))
				{
					PlaySE("BossSE04", "bs040_kai13");
					SetStatus(MainStatus.Skill8, SubStatus.Phase2);
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
		case MainStatus.Die:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (_currentFrame > 1f)
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
				if (_currentFrame > 0.4f)
				{
					SetStatus(MainStatus.Die, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase3:
				if (Controller.Collisions.below)
				{
					SetStatus(MainStatus.Die);
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
		if (!Activate && _mainStatus != MainStatus.Debut)
		{
			return;
		}
		base.transform.localPosition = Vector3.MoveTowards(base.transform.localPosition, Controller.LogicPosition.vec3, distanceDelta);
		if (_mainStatus != MainStatus.Skill1)
		{
			return;
		}
		switch (_subStatus)
		{
		case SubStatus.Phase1:
		{
			Collider2D collider2D = Physics2D.OverlapBox(NowPos + Vector3.right * base.direction, Vector2.one, 0f, LayerMask.GetMask("Player"));
			if ((bool)collider2D && (int)Hp > 0)
			{
				OrangeCharacter component = collider2D.GetComponent<OrangeCharacter>();
				CatchTool.CatchTarget(component);
			}
			if (CatchTool.IsCatching)
			{
				SetStatus(MainStatus.Skill1, SubStatus.Phase2);
			}
			else if ((NowPos.x - EndPos.x) * (float)base.direction > 0.5f || (base.direction == 1 && Controller.Collisions.right) || (base.direction == -1 && Controller.Collisions.left))
			{
				SetStatus(MainStatus.Skill1, SubStatus.Phase7);
			}
			break;
		}
		case SubStatus.Phase2:
		case SubStatus.Phase3:
			if (CatchTool.IsCatching)
			{
				CatchTool.MoveTarget();
			}
			break;
		case SubStatus.Phase4:
		case SubStatus.Phase5:
			if (CatchTool.IsCatching)
			{
				CatchTool.MoveTargetWithForce(new VInt3(0, Skill1ThrowSpeed, 0));
			}
			if ((bool)CatchTool.TargetOC && CatchTool.TargetOC.Controller.Collisions.above)
			{
				CatchTool.ReleaseTarget();
			}
			break;
		}
	}

	public override void SetActive(bool isActive)
	{
		base.SetActive(isActive);
		if (isActive)
		{
			SetMaxGravity(OrangeBattleUtility.FP_MaxGravity * 2);
			ModelTransform.localScale = new Vector3(1.2f, 1.2f, 1.2f * (float)base.direction);
			UseUltimate = false;
			HaveNear = false;
			HaveMid = false;
			SwitchMesh(GunMeshL, false);
			SwitchMesh(GunMeshR, false);
			_collideBullet.UpdateBulletData(EnemyWeapons[0].BulletData);
			_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			_collideBullet.Active(targetMask);
			Skill1Collide.UpdateBulletData(EnemyWeapons[3].BulletData);
			Skill1Collide.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			BuffCollide1.UpdateBulletData(EnemyWeapons[10].BulletData, "", base.gameObject.GetInstanceID());
			BuffCollide1.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			SKILL_TABLE pData = ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT[EnemyWeapons[10].BulletData.n_LINK_SKILL];
			BuffCollide2.UpdateBulletData(pData, "", base.gameObject.GetInstanceID());
			BuffCollide2.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			SetStatus(MainStatus.Debut);
		}
		else
		{
			_collideBullet.BackToPool();
		}
	}

	protected override void DeadBehavior(ref HurtPassParam tHurtPassParam)
	{
		if (_mainStatus != MainStatus.Die)
		{
			CloseAllFX();
			if ((bool)_collideBullet)
			{
				_collideBullet.BackToPool();
			}
			if ((bool)Skill1Collide)
			{
				Skill1Collide.BackToPool();
			}
			if ((bool)BuffCollide1)
			{
				BuffCollide1.BackToPool();
			}
			if ((bool)BuffCollide2)
			{
				BuffCollide2.BackToPool();
			}
			if (CatchTool.IsCatching && GameLogicUpdateManager.GameFrame > ActionFrame)
			{
				CatchTool.ReleaseTarget();
			}
			StageUpdate.SlowStage();
			SetColliderEnable(false);
			SetStatus(MainStatus.Die);
		}
	}

	private Vector3 GetTargetPos(bool realcenter = false)
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
				TargetPos = new VInt3(Target.GetTargetPoint() + Vector3.up * 0.15f);
			}
			return TargetPos.vec3;
		}
		return NowPos + Vector3.right * 3f * base.direction;
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
			Debug.Log(string.Concat("特效載入有誤，目前狀態是 ", _mainStatus, "的階段 ", _subStatus));
		}
	}

	private void SwitchMesh(SkinnedMeshRenderer Mesh, bool onoff)
	{
		if ((bool)Mesh)
		{
			Mesh.enabled = onoff;
			return;
		}
		Debug.Log(string.Concat("Mesh載入有誤，目前狀態是 ", _mainStatus, "的階段 ", _subStatus));
	}

	public override ObscuredInt Hurt(HurtPassParam tHurtPassParam)
	{
		ObscuredInt result = base.Hurt(tHurtPassParam);
		if (!UseUltimate && (int)Hp <= (int)MaxHp / 2 && BuffState < MaxBuffState)
		{
			UseUltimate = true;
		}
		return result;
	}

	private MainStatus ChooseFar()
	{
		HaveMid = false;
		HaveNear = false;
		if (FarDisCount >= MaxFarDisTimes)
		{
			return MainStatus.Skill8;
		}
		FarDisCount++;
		MainStatus mainStatus = MainStatus.Idle;
		if (OrangeBattleUtility.Random(0, 100) < 80 || IsUltimate || BuffState > 2)
		{
			if (BuffState <= 2)
			{
				mainStatus = ((!IsUltimate) ? MainStatus.Skill0 : MainStatus.Skill5);
			}
			else
			{
				mainStatus = ((!UseSkill4) ? MainStatus.Skill0 : MainStatus.Skill4);
				UseSkill4 = !UseSkill4;
			}
		}
		else
		{
			mainStatus = MainStatus.Skill1;
		}
		return mainStatus;
	}

	private MainStatus ChooseMid()
	{
		HaveNear = false;
		FarDisCount = 0;
		if (HaveMid)
		{
			HaveMid = false;
			return ChooseFar();
		}
		HaveMid = true;
		MainStatus mainStatus = MainStatus.Idle;
		if (BuffState < 2 && !IsUltimate)
		{
			if (CatchFailCount < MaxCatchFailTimes)
			{
				return MainStatus.Skill1;
			}
			return ChooseFar();
		}
		if (BuffState > 1 && IsUltimate)
		{
			if (OrangeBattleUtility.Random(0, 100) < 50)
			{
				return MainStatus.Skill3;
			}
			return MainStatus.Skill6;
		}
		if (IsUltimate)
		{
			return MainStatus.Skill6;
		}
		if (OrangeBattleUtility.Random(0, 100) < 60 || BuffState > 2)
		{
			return MainStatus.Skill3;
		}
		return MainStatus.Skill1;
	}

	private MainStatus ChooseNear()
	{
		HaveMid = false;
		FarDisCount = 0;
		if (HaveNear)
		{
			HaveNear = false;
			return ChooseMid();
		}
		HaveNear = true;
		MainStatus mainStatus = MainStatus.Idle;
		if (BuffState < 1)
		{
			return ChooseMid();
		}
		return MainStatus.Skill2;
	}

	private void CloseAllFX()
	{
		if ((bool)Skill0ChargeFX1)
		{
			Skill0ChargeFX1.Stop();
		}
		if ((bool)Skill0ChargeFX2)
		{
			Skill0ChargeFX2.Stop();
		}
		if ((bool)Skill1ChipFX)
		{
			Skill1ChipFX.Stop();
		}
		if ((bool)Skill1ThunderFX)
		{
			Skill1ThunderFX.Stop();
		}
		if ((bool)Skill2FX)
		{
			Skill2FX.Stop();
		}
		if ((bool)Skill3FX1)
		{
			Skill3FX1.Stop();
		}
		if ((bool)Skill3FX2)
		{
			Skill3FX2.Stop();
		}
		if ((bool)Skill4ChargeFX1)
		{
			Skill4ChargeFX1.Stop();
		}
		if ((bool)Skill4ChargeFX2)
		{
			Skill4ChargeFX2.Stop();
		}
		if ((bool)Skill5ChargeFX)
		{
			Skill5ChargeFX.Stop();
		}
		if ((bool)Skill6FX1)
		{
			Skill6FX1.Stop();
		}
		if ((bool)Skill6FX2)
		{
			Skill6FX2.Stop();
		}
	}
}
