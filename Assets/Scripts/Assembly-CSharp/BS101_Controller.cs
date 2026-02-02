#define RELEASE
using System;
using System.Collections.Generic;
using CallbackDefs;
using CodeStage.AntiCheat.ObscuredTypes;
using NaughtyAttributes;
using Newtonsoft.Json;
using StageLib;
using UnityEngine;

public class BS101_Controller : EnemyControllerBase, IManagedUpdateBehavior
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
		Die = 9
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
		ANI_IDLE1 = 0,
		ANI_IDLE2 = 1,
		ANI_DEBUT = 2,
		ANI_FALL = 3,
		ANI_LAND = 4,
		ANI_DASH_START = 5,
		ANI_DASH_LOOP = 6,
		ANI_DASH_END = 7,
		ANI_SKILL0_START1 = 8,
		ANI_SKILL0_LOOP1 = 9,
		ANI_SKILL0_START2 = 10,
		ANI_SKILL0_LOOP2 = 11,
		ANI_SKILL0_START3 = 12,
		ANI_SKILL0_END3 = 13,
		ANI_SKILL0_START4 = 14,
		ANI_SKILL0_END4 = 15,
		ANI_SKILL1_START = 16,
		ANI_SKILL1_LOOP = 17,
		ANI_SKILL1_END = 18,
		ANI_SKILL2_START = 19,
		ANI_SKILL2_LOOP = 20,
		ANI_SKILL2_END = 21,
		ANI_SKILL3_START = 22,
		ANI_SKILL3_LOOP = 23,
		ANI_SKILL3_END = 24,
		ANI_SKILL4_START1 = 25,
		ANI_SKILL4_LOOP1 = 26,
		ANI_SKILL4_START2 = 27,
		ANI_SKILL4_LOOP2 = 28,
		ANI_SKILL4_END2 = 29,
		ANI_SKILL5_START = 30,
		ANI_SKILL6_START = 31,
		ANI_HURT = 32,
		ANI_DEAD = 33,
		MAX_ANIMATION_ID = 34
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

	[Header("AI控制")]
	[SerializeField]
	private bool EXMode;

	private bool UseSword;

	[SerializeField]
	private float AIJudgeDisX = 6f;

	private int EM192Count;

	private int UseSkillCount;

	[SerializeField]
	private bool DebugMode;

	[SerializeField]
	private MainStatus NextSkill;

	private int nDeadCount;

	private int[] _animationHash;

	private int[] DefaultSkillCard = new int[6] { 0, 1, 2, 3, 4, 5 };

	private static int[] DefaultRangedSkillCard = new int[3] { 3, 4, 5 };

	private List<int> RangedSKC = new List<int>();

	private List<int> SkillCard = new List<int>();

	private Vector3 LastTargetPos = new Vector3(0f, 0f, 0f);

	[Header("Mesh")]
	[SerializeField]
	private SkinnedMeshRenderer BodyMesh;

	[SerializeField]
	private SkinnedMeshRenderer GlassMesh;

	[SerializeField]
	private SkinnedMeshRenderer HandMeshL;

	[SerializeField]
	private SkinnedMeshRenderer HandMeshR;

	[SerializeField]
	private GameObject SwordObj;

	[Header("通用")]
	private Vector3 StartPos;

	private Vector3 EndPos;

	private float MaxXPos;

	private float MinXPos;

	private float GroundYPos;

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

	[Header("待機")]
	[SerializeField]
	private float IdleWaitTime = 0.2f;

	private int IdleWaitFrame;

	[Header("衝刺三段斬")]
	private CollideBullet SwordCollide;

	[SerializeField]
	private string DashFXName = "OBJ_DASH_SMOKE";

	[SerializeField]
	private string SwordCollideObjName;

	[SerializeField]
	private int Skill0DashSpeed = 9000;

	[SerializeField]
	private float Skill0ChargeTime = 1f;

	private bool Skill0_1stSlashHit;

	[SerializeField]
	private ParticleSystem Skill0SlashFX1;

	[SerializeField]
	private ParticleSystem Skill0SlashFX2;

	[SerializeField]
	private ParticleSystem Skill0SlashFX3;

	[Header("劍重擊")]
	[SerializeField]
	private int Skill1JumpSpeed = 15000;

	[SerializeField]
	private int Skill1Speed = 2000;

	[SerializeField]
	private ParticleSystem Skill1ChargeFX;

	[SerializeField]
	private int Skill1BackSpeed = 4000;

	[SerializeField]
	private int Skill1BackJumpSpeed = 4000;

	private int SpawnCount;

	[Header("瞬間拔刀斬")]
	[SerializeField]
	private string Skill2PreFxName = "fxuse_burai_007";

	[SerializeField]
	private float Skill2ChargeTime = 1f;

	[SerializeField]
	private float Skill2HideTime = 0.2f;

	[SerializeField]
	private float Skill2ShowTime = 0.7f;

	[Header("拳彈")]
	[SerializeField]
	private int Skill3ShootTimes = 10;

	[SerializeField]
	private int Skill3EXShootTimes = 5;

	[SerializeField]
	private Transform ShootPointR;

	[Header("漫天拳彈")]
	[SerializeField]
	private float Skill4ChargeTime = 1f;

	[SerializeField]
	private int Skill4ShootTimes = 12;

	[SerializeField]
	private float Skill4Interval = 0.1f;

	[SerializeField]
	private float Skill4WaitTime = 1f;

	[SerializeField]
	private float Skill4XFloat = 30f;

	[SerializeField]
	private float Skill4YFloat = 30f;

	[SerializeField]
	private Transform ShootPointL;

	[Header("召喚劍")]
	[SerializeField]
	private ParticleSystem Skill5SummonFX1;

	[SerializeField]
	private ParticleSystem Skill5SummonFX2;

	[Header("召喚拳套")]
	[SerializeField]
	private ParticleSystem Skill6SummonFX1;

	[SerializeField]
	private ParticleSystem Skill6SummonFX2;

	[Header("其他特效")]
	[SerializeField]
	private ParticleSystem AuraFX;

	[SerializeField]
	private ParticleSystem ShadowFX;

	[Header("死亡")]
	[SerializeField]
	private string TeleportOutFxName = "fx_bs086_teleport_out";

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
		ModelTransform.localScale = new Vector3(ModelTransform.localScale.x, ModelTransform.localScale.y, ModelTransform.localScale.z * (float)base.direction);
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
		_animationHash = new int[34];
		_animationHash[0] = Animator.StringToHash("BS101@idle_1_loop");
		_animationHash[1] = Animator.StringToHash("BS101@idle_2_loop");
		_animationHash[2] = Animator.StringToHash("BS101@debut");
		_animationHash[3] = Animator.StringToHash("BS101@fall_loop");
		_animationHash[4] = Animator.StringToHash("BS101@landing");
		_animationHash[5] = Animator.StringToHash("BS101@dash_start");
		_animationHash[6] = Animator.StringToHash("BS101@dash_loop");
		_animationHash[7] = Animator.StringToHash("BS101@dash_end");
		_animationHash[8] = Animator.StringToHash("BS101@skill_01_step1_start");
		_animationHash[9] = Animator.StringToHash("BS101@skill_01_step1_loop");
		_animationHash[10] = Animator.StringToHash("BS101@skill_01_step2_start");
		_animationHash[11] = Animator.StringToHash("BS101@skill_01_step2_loop");
		_animationHash[12] = Animator.StringToHash("BS101@skill_01_step3_start");
		_animationHash[13] = Animator.StringToHash("BS101@skill_01_step3_end");
		_animationHash[14] = Animator.StringToHash("BS101@skill_01_step4_start");
		_animationHash[15] = Animator.StringToHash("BS101@skill_01_step4_end");
		_animationHash[16] = Animator.StringToHash("BS101@skill_02_start");
		_animationHash[17] = Animator.StringToHash("BS101@skill_02_loop");
		_animationHash[18] = Animator.StringToHash("BS101@skill_02_end");
		_animationHash[19] = Animator.StringToHash("BS101@skill_03_start");
		_animationHash[20] = Animator.StringToHash("BS101@skill_03_loop");
		_animationHash[21] = Animator.StringToHash("BS101@skill_03_end");
		_animationHash[22] = Animator.StringToHash("BS101@skill_04_start");
		_animationHash[23] = Animator.StringToHash("BS101@skill_04_loop");
		_animationHash[24] = Animator.StringToHash("BS101@skill_04_end");
		_animationHash[25] = Animator.StringToHash("BS101@skill_05_step1_start");
		_animationHash[26] = Animator.StringToHash("BS101@skill_05_step1_loop");
		_animationHash[27] = Animator.StringToHash("BS101@skill_05_step2_start");
		_animationHash[28] = Animator.StringToHash("BS101@skill_05_step2_loop");
		_animationHash[29] = Animator.StringToHash("BS101@skill_05_step2_end");
		_animationHash[30] = Animator.StringToHash("BS101@summon_blade");
		_animationHash[31] = Animator.StringToHash("BS101@summon_punch");
		_animationHash[32] = Animator.StringToHash("BS101@hurt_loop");
		_animationHash[33] = Animator.StringToHash("BS101@dead");
	}

	private void LoadParts(ref Transform[] childs)
	{
		ModelTransform = OrangeBattleUtility.FindChildRecursive(ref childs, "model", true);
		_animator = ModelTransform.GetComponent<Animator>();
		_collideBullet = OrangeBattleUtility.FindChildRecursive(ref childs, "Collider", true).gameObject.AddOrGetComponent<CollideBullet>();
		SwordCollide = OrangeBattleUtility.FindChildRecursive(ref childs, SwordCollideObjName, true).gameObject.AddOrGetComponent<CollideBullet>();
		if (BodyMesh == null)
		{
			BodyMesh = OrangeBattleUtility.FindChildRecursive(ref childs, "BodyMesh_m", true).gameObject.AddOrGetComponent<SkinnedMeshRenderer>();
		}
		if (GlassMesh == null)
		{
			GlassMesh = OrangeBattleUtility.FindChildRecursive(ref childs, "GlassesMesh_m", true).gameObject.AddOrGetComponent<SkinnedMeshRenderer>();
		}
		if (HandMeshL == null)
		{
			HandMeshL = OrangeBattleUtility.FindChildRecursive(ref childs, "HandMesh_L_m", true).gameObject.AddOrGetComponent<SkinnedMeshRenderer>();
		}
		if (HandMeshR == null)
		{
			HandMeshR = OrangeBattleUtility.FindChildRecursive(ref childs, "HandMesh_R_m", true).gameObject.AddOrGetComponent<SkinnedMeshRenderer>();
		}
		if (SwordObj == null)
		{
			SwordObj = OrangeBattleUtility.FindChildRecursive(ref childs, "SaberMesh_m", true).gameObject;
		}
		if (Skill0SlashFX1 == null)
		{
			Skill0SlashFX1 = OrangeBattleUtility.FindChildRecursive(ref childs, "Skill0SlashFX1", true).gameObject.AddOrGetComponent<ParticleSystem>();
		}
		if (Skill0SlashFX2 == null)
		{
			Skill0SlashFX2 = OrangeBattleUtility.FindChildRecursive(ref childs, "Skill0SlashFX2", true).gameObject.AddOrGetComponent<ParticleSystem>();
		}
		if (Skill0SlashFX3 == null)
		{
			Skill0SlashFX3 = OrangeBattleUtility.FindChildRecursive(ref childs, "Skill0SlashFX3", true).gameObject.AddOrGetComponent<ParticleSystem>();
		}
		if (Skill1ChargeFX == null)
		{
			Skill1ChargeFX = OrangeBattleUtility.FindChildRecursive(ref childs, "Skill1ChargeFX", true).gameObject.AddOrGetComponent<ParticleSystem>();
		}
		if (Skill5SummonFX1 == null)
		{
			Skill5SummonFX1 = OrangeBattleUtility.FindChildRecursive(ref childs, "Skill5SummonFX1", true).gameObject.AddOrGetComponent<ParticleSystem>();
		}
		if (Skill5SummonFX2 == null)
		{
			Skill5SummonFX2 = OrangeBattleUtility.FindChildRecursive(ref childs, "Skill5SummonFX2", true).gameObject.AddOrGetComponent<ParticleSystem>();
		}
		if (Skill6SummonFX1 == null)
		{
			Skill6SummonFX1 = OrangeBattleUtility.FindChildRecursive(ref childs, "Skill6SummonFX1", true).gameObject.AddOrGetComponent<ParticleSystem>();
		}
		if (Skill6SummonFX2 == null)
		{
			Skill6SummonFX2 = OrangeBattleUtility.FindChildRecursive(ref childs, "Skill6SummonFX2", true).gameObject.AddOrGetComponent<ParticleSystem>();
		}
		if (AuraFX == null)
		{
			AuraFX = OrangeBattleUtility.FindChildRecursive(ref childs, "AuraFX", true).gameObject.AddOrGetComponent<ParticleSystem>();
		}
		if (ShadowFX == null)
		{
			ShadowFX = OrangeBattleUtility.FindChildRecursive(ref childs, "ShadowFX", true).gameObject.AddOrGetComponent<ParticleSystem>();
		}
		if (ShootPointR == null)
		{
			ShootPointR = OrangeBattleUtility.FindChildRecursive(ref childs, "R BusterPoint", true);
		}
		if (ShootPointL == null)
		{
			ShootPointL = OrangeBattleUtility.FindChildRecursive(ref childs, "L BusterPoint", true);
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
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(Skill2PreFxName, 2);
		base.Start();
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

	private void PlayBossSE05(string cue)
	{
		PlaySE("BossSE05", cue);
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
			case SubStatus.Phase1:
				SwordObj.SetActive(true);
				break;
			}
			break;
		case MainStatus.Idle:
			_velocity.x = 0;
			_collideBullet.Active(targetMask);
			SwordCollide.BackToPool();
			IdleWaitFrame = GameLogicUpdateManager.GameFrame + (int)(IdleWaitTime * 20f);
			break;
		case MainStatus.Skill0:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_velocity = VInt3.zero;
				break;
			case SubStatus.Phase1:
				ActionFrame = GameLogicUpdateManager.GameFrame + (int)(Skill0ChargeTime * 20f);
				break;
			case SubStatus.Phase2:
				PlayBossSE05("bs041_burai03");
				EndPos = GetTargetPos();
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(DashFXName, ModelTransform, Quaternion.identity, Array.Empty<object>());
				break;
			case SubStatus.Phase3:
				_velocity.x = (int)((float)DashSpeed * DashSpeedMulti) * base.direction;
				break;
			case SubStatus.Phase4:
				_collideBullet.BackToPool();
				_velocity = VInt3.zero;
				Skill0_1stSlashHit = false;
				SwitchFx(Skill0SlashFX1, true);
				PlayBossSE05("bs041_burai14");
				SwordCollide.UpdateBulletData(EnemyWeapons[1].BulletData);
				SwordCollide.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
				SwordCollide.Active(targetMask);
				SwordCollide.HitCallback = Skill0_1stSlashHitCallBack;
				break;
			case SubStatus.Phase5:
				SwordCollide.HitCallback = null;
				_velocity = VInt3.zero;
				break;
			case SubStatus.Phase6:
				SwitchFx(Skill0SlashFX2, true);
				PlayBossSE05("bs041_burai14");
				SwordCollide.UpdateBulletData(EnemyWeapons[2].BulletData);
				SwordCollide.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
				SwordCollide.Active(targetMask);
				SwordCollide.HitCallback = null;
				break;
			case SubStatus.Phase7:
				SwitchFx(Skill0SlashFX3, true);
				PlayBossSE05("bs041_burai14");
				SwordCollide.UpdateBulletData(EnemyWeapons[3].BulletData);
				SwordCollide.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
				SwordCollide.Active(targetMask);
				break;
			}
			break;
		case MainStatus.Skill1:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				HasActed = false;
				ActionAnimatorFrame = 0.2f;
				SwitchFx(Skill1ChargeFX, true);
				PlayBossSE05("bs041_burai01");
				break;
			case SubStatus.Phase2:
				PlayBossSE05("bs041_burai02");
				PlayBossSE05("bs041_burai09");
				_velocity = VInt3.zero;
				break;
			case SubStatus.Phase3:
				UseSword = false;
				SwordObj.SetActive(false);
				SwitchFx(Skill1ChargeFX, false);
				SpawnEnemy(_transform.position + Vector3.right * base.direction + Vector3.up * 0.4f);
				(BulletBase.TryShotBullet(EnemyWeapons[4].BulletData, _transform.position + Vector3.right * base.direction, Vector3.zero, null, selfBuffManager.sBuffStatus, EnemyData, targetMask) as CollideBullet).bNeedBackPoolModelName = true;
				PlayBossSE05("bs041_burai10");
				_velocity.x = Skill1BackSpeed * -base.direction;
				_velocity.y = Skill1BackJumpSpeed;
				break;
			}
			break;
		case MainStatus.Skill2:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				PlayBossSE05("bs041_burai15");
				SwitchFx(ShadowFX, true);
				ShowHideMesh(false);
				_collideBullet.BackToPool();
				ActionFrame = GameLogicUpdateManager.GameFrame + (int)(Skill2HideTime * 20f);
				break;
			case SubStatus.Phase1:
			{
				IgnoreGravity = true;
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(Skill2PreFxName, NowPos, Quaternion.identity, new object[1] { Vector3.one });
				EndPos = GetTargetPos();
				int num = CheckBlock(EndPos);
				if (num != 0)
				{
					_transform.position = Target._transform.position - Vector3.right * num;
					Controller.LogicPosition = new VInt3(_transform.position);
					UpdateDirection(num);
				}
				else
				{
					_transform.position = Target._transform.position + Vector3.right * base.direction;
					Controller.LogicPosition = new VInt3(_transform.position);
					UpdateDirection(-base.direction);
				}
				ActionFrame = GameLogicUpdateManager.GameFrame + (int)(Skill2ShowTime * 20f);
				break;
			}
			case SubStatus.Phase3:
				ActionFrame = GameLogicUpdateManager.GameFrame + (int)(Skill2ChargeTime * 20f);
				break;
			case SubStatus.Phase4:
				PlayBossSE05("bs041_burai14");
				SwitchFx(Skill0SlashFX3, true);
				SwordCollide.UpdateBulletData(EnemyWeapons[5].BulletData);
				SwordCollide.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
				SwordCollide.Active(targetMask);
				_collideBullet.BackToPool();
				break;
			case SubStatus.Phase5:
				PlayBossSE05("bs041_burai15");
				SwitchFx(ShadowFX, true);
				SwordCollide.BackToPool();
				ShowHideMesh(false);
				ActionFrame = GameLogicUpdateManager.GameFrame + (int)(Skill2HideTime * 20f);
				break;
			case SubStatus.Phase6:
				IgnoreGravity = false;
				if (base.direction == 1)
				{
					_transform.position = new Vector3(MaxXPos - 3f, GroundYPos, 0f);
					Controller.LogicPosition = new VInt3(_transform.position);
				}
				else
				{
					_transform.position = new Vector3(MinXPos + 3f, GroundYPos, 0f);
					Controller.LogicPosition = new VInt3(_transform.position);
				}
				UpdateDirection(-base.direction);
				SwitchFx(ShadowFX, true);
				_collideBullet.Active(targetMask);
				ShowHideMesh();
				ActionFrame = GameLogicUpdateManager.GameFrame + (int)(Skill2HideTime * 20f);
				break;
			}
			break;
		case MainStatus.Skill3:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (EXMode)
				{
					ActionTimes = Skill3EXShootTimes;
				}
				else
				{
					ActionTimes = Skill3ShootTimes;
				}
				break;
			case SubStatus.Phase1:
			{
				EndPos = GetTargetPos();
				UpdateDirection();
				ShotAngle = Vector2.Angle(Vector2.up, EndPos - ShootPosR);
				_animator.SetFloat(_HashAngle, ShotAngle);
				Vector3 pDirection = EndPos - ShootPosR;
				BulletBase.TryShotBullet(EnemyWeapons[6].BulletData, ShootPosR, pDirection, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
				break;
			}
			}
			break;
		case MainStatus.Skill4:
			switch (_subStatus)
			{
			case SubStatus.Phase1:
				ActionFrame = GameLogicUpdateManager.GameFrame + (int)(Skill4ChargeTime * 20f);
				break;
			case SubStatus.Phase2:
				ActionTimes = Skill4ShootTimes;
				break;
			case SubStatus.Phase3:
				ActionFrame = GameLogicUpdateManager.GameFrame + (int)(Skill4Interval * 20f);
				StartPos = _transform.position + Vector3.up * (1.8f + (float)OrangeBattleUtility.Random(-5, 11) / 10f) + Vector3.right * ((float)OrangeBattleUtility.Random(-15, 16) / 10f);
				EnemyWeapons[7].BulletData.s_USE_SE = "BossSE05,bs041_burai13";
				BulletBase.TryShotBullet(EnemyWeapons[7].BulletData, StartPos, Vector3.up, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
				break;
			case SubStatus.Phase4:
				ActionFrame = GameLogicUpdateManager.GameFrame + (int)(Skill4WaitTime * 20f);
				break;
			case SubStatus.Phase5:
				ActionFrame = GameLogicUpdateManager.GameFrame + (int)(Skill4Interval * 20f);
				if (base.direction == -1)
				{
					StartPos = new Vector3(MaxXPos, _transform.position.y, 0f);
				}
				else
				{
					StartPos = new Vector3(MinXPos, _transform.position.y, 0f);
				}
				StartPos += Vector3.up * (8f + OrangeBattleUtility.Random(0f - Skill4YFloat, Skill4YFloat + 40f) / 10f) + Vector3.right * (OrangeBattleUtility.Random(0f - Skill4XFloat, Skill4XFloat) / 10f);
				EnemyWeapons[7].BulletData.s_USE_SE = "BossSE05,bs041_burai12";
				BulletBase.TryShotBullet(EnemyWeapons[7].BulletData, StartPos, Vector3.right * 2f * base.direction + Vector3.down, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
				break;
			}
			break;
		case MainStatus.Skill5:
			PlayBossSE05("bs041_burai08");
			UseSword = true;
			SwitchFx(Skill5SummonFX1, true);
			SwitchFx(Skill5SummonFX2, true);
			break;
		case MainStatus.Skill6:
			UseSword = false;
			PlayBossSE05("bs041_burai11");
			if (base.direction == -1)
			{
				SwitchFx(Skill6SummonFX1, true);
			}
			else
			{
				SwitchFx(Skill6SummonFX2, true);
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
				MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(TeleportOutFxName, 1, delegate
				{
					FxBase poolObj = MonoBehaviourSingleton<PoolManager>.Instance.GetPoolObj<FxBase>(TeleportOutFxName);
					poolObj.SEName = new string[2] { "BossSE05", "bs041_burai05" };
					Vector3 realCenterPos = Controller.GetRealCenterPos();
					poolObj.transform.SetParent(null);
					poolObj.transform.SetPositionAndRotation(realCenterPos, Quaternion.identity);
					MonoBehaviourSingleton<FxManager>.Instance.RegisterFxBase(poolObj);
					poolObj.Active(Vector3.one);
				});
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
				_currentAnimationId = AnimationID.ANI_FALL;
				break;
			case SubStatus.Phase1:
				PlayBossSE05("bs041_burai02");
				_currentAnimationId = AnimationID.ANI_DEBUT;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_IDLE2;
				break;
			}
			break;
		case MainStatus.Idle:
			if (UseSword)
			{
				_currentAnimationId = AnimationID.ANI_IDLE2;
			}
			else
			{
				_currentAnimationId = AnimationID.ANI_IDLE1;
			}
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
			case SubStatus.Phase6:
				_currentAnimationId = AnimationID.ANI_SKILL0_START4;
				break;
			case SubStatus.Phase7:
				_currentAnimationId = AnimationID.ANI_SKILL0_END4;
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
			switch (_subStatus)
			{
			default:
				return;
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
				_currentAnimationId = AnimationID.ANI_IDLE2;
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
			case SubStatus.Phase6:
				_currentAnimationId = AnimationID.ANI_SKILL4_END2;
				break;
			case SubStatus.Phase4:
			case SubStatus.Phase5:
				return;
			}
			break;
		case MainStatus.Skill5:
			_currentAnimationId = AnimationID.ANI_SKILL5_START;
			break;
		case MainStatus.Skill6:
			_currentAnimationId = AnimationID.ANI_SKILL6_START;
			break;
		case MainStatus.Die:
			switch (_subStatus)
			{
			default:
				return;
			case SubStatus.Phase0:
			case SubStatus.Phase1:
				if (UseSword)
				{
					_currentAnimationId = AnimationID.ANI_IDLE2;
				}
				else
				{
					_currentAnimationId = AnimationID.ANI_IDLE1;
				}
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
				if (EM192Count > 2)
				{
					EM192Count = 0;
					mainStatus = MainStatus.Skill6;
				}
				else if (UseSword)
				{
					EndPos = GetTargetPos();
					if (Mathf.Abs(EndPos.x - NowPos.x) < AIJudgeDisX)
					{
						mainStatus = MainStatus.Skill1;
					}
					else if (UseSkillCount > 1)
					{
						UseSkillCount = 0;
						mainStatus = MainStatus.Skill1;
					}
					else
					{
						mainStatus = ((!EXMode) ? MainStatus.Skill0 : ((OrangeBattleUtility.Random(0, 10) >= 5) ? MainStatus.Skill2 : MainStatus.Skill0));
						UseSkillCount++;
					}
				}
				else
				{
					UseSkillCount = 0;
					mainStatus = ((OrangeBattleUtility.Random(0, 100) >= 50) ? MainStatus.Skill6 : MainStatus.Skill5);
				}
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
				else if (UseSword)
				{
					UpdateRandomState(MainStatus.Skill0);
				}
				else
				{
					UpdateRandomState(MainStatus.Skill6);
				}
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
				if (GameLogicUpdateManager.GameFrame > ActionFrame)
				{
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
				if ((NowPos.x - EndPos.x) * (float)base.direction > -2f || (base.direction == 1 && Controller.Collisions.right) || (base.direction == -1 && Controller.Collisions.left))
				{
					PlayBossSE05("bs041_burai04");
					SetStatus(MainStatus.Skill0, SubStatus.Phase4);
				}
				break;
			case SubStatus.Phase4:
				if (_currentFrame > 1f)
				{
					if (Skill0_1stSlashHit)
					{
						SetStatus(MainStatus.Skill0, SubStatus.Phase6);
					}
					else
					{
						SetStatus(MainStatus.Skill0, SubStatus.Phase5);
					}
				}
				break;
			case SubStatus.Phase5:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Idle);
				}
				break;
			case SubStatus.Phase6:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill0, SubStatus.Phase7);
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
		case MainStatus.Skill1:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (!HasActed && _currentFrame > ActionAnimatorFrame)
				{
					HasActed = true;
					_velocity.y = Skill1JumpSpeed;
					_velocity.x = Skill1Speed * base.direction;
				}
				if (HasActed && _velocity.y <= 1000)
				{
					SetStatus(MainStatus.Skill1, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if (Controller.Collisions.below)
				{
					Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.CAMERA_SHAKE, 1.5f, false);
					SetStatus(MainStatus.Skill1, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				if (_currentFrame > 0.5f)
				{
					SetStatus(MainStatus.Skill1, SubStatus.Phase3);
				}
				break;
			case SubStatus.Phase3:
				if (_currentFrame > 0.9f || Controller.Collisions.below)
				{
					_velocity.x = 0;
				}
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
				if (GameLogicUpdateManager.GameFrame > ActionFrame)
				{
					SetStatus(MainStatus.Skill2, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if (GameLogicUpdateManager.GameFrame > ActionFrame)
				{
					_collideBullet.Active(targetMask);
					SwitchFx(ShadowFX, true);
					ShowHideMesh();
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
				if (GameLogicUpdateManager.GameFrame > ActionFrame)
				{
					SetStatus(MainStatus.Skill2, SubStatus.Phase4);
				}
				break;
			case SubStatus.Phase4:
				if (_currentFrame > 1f)
				{
					if (!Controller.Collisions.below)
					{
						SetStatus(MainStatus.Skill2, SubStatus.Phase5);
						break;
					}
					IgnoreGravity = false;
					SetStatus(MainStatus.Idle);
				}
				break;
			case SubStatus.Phase5:
				if (GameLogicUpdateManager.GameFrame > ActionFrame)
				{
					SetStatus(MainStatus.Skill2, SubStatus.Phase6);
				}
				break;
			case SubStatus.Phase6:
				if (GameLogicUpdateManager.GameFrame > ActionFrame)
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
				if (!HasActed && _currentFrame > ActionAnimatorFrame)
				{
					HasActed = true;
					EndPos = GetTargetPos();
					ShotAngle = Vector2.Angle(Vector2.up, EndPos - ShootPosL);
					Vector3 pDirection = EndPos - ShootPosL;
					if ((EndPos.x - ShootPosL.x) * (float)base.direction < 0f)
					{
						ShotAngle = 90f;
						pDirection = Vector3.right * base.direction;
					}
					_animator.SetFloat(_HashAngle, ShotAngle);
					BulletBase.TryShotBullet(EnemyWeapons[2].BulletData, ShootPosL, pDirection, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
				}
				if (_currentFrame > 1f)
				{
					if (--ActionTimes > 0)
					{
						SetStatus(MainStatus.Skill3, SubStatus.Phase1);
					}
					else
					{
						SetStatus(MainStatus.Skill3, SubStatus.Phase2);
					}
				}
				break;
			case SubStatus.Phase2:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill5);
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
				if (GameLogicUpdateManager.GameFrame > ActionFrame)
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
				if (GameLogicUpdateManager.GameFrame > ActionFrame)
				{
					if (--ActionTimes > 0)
					{
						SetStatus(MainStatus.Skill4, SubStatus.Phase3);
						break;
					}
					ActionTimes = Skill4ShootTimes;
					SetStatus(MainStatus.Skill4, SubStatus.Phase4);
				}
				break;
			case SubStatus.Phase4:
				if (GameLogicUpdateManager.GameFrame > ActionFrame)
				{
					SetStatus(MainStatus.Skill4, SubStatus.Phase5);
				}
				break;
			case SubStatus.Phase5:
				if (GameLogicUpdateManager.GameFrame > ActionFrame)
				{
					if (--ActionTimes > 0)
					{
						SetStatus(MainStatus.Skill4, SubStatus.Phase5);
					}
					else
					{
						SetStatus(MainStatus.Skill4, SubStatus.Phase6);
					}
				}
				break;
			case SubStatus.Phase6:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill3);
				}
				break;
			}
			break;
		case MainStatus.Skill5:
			if (_subStatus == SubStatus.Phase0)
			{
				if (!SwordObj.activeSelf && _currentFrame > 0.6f)
				{
					SwordObj.SetActive(true);
				}
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Idle);
				}
			}
			break;
		case MainStatus.Skill6:
		{
			if (_subStatus != 0 || !(_currentFrame > 1f))
			{
				break;
			}
			List<EM192_Controller> list = new List<EM192_Controller>();
			for (int i = 0; i < StageUpdate.runEnemys.Count; i++)
			{
				EM192_Controller eM192_Controller = StageUpdate.runEnemys[i].mEnemy as EM192_Controller;
				if ((bool)eM192_Controller)
				{
					list.Add(eM192_Controller);
				}
			}
			for (int j = 0; j < list.Count; j++)
			{
				list[j].SetDead();
			}
			EM192Count = 0;
			if (EXMode)
			{
				SetStatus(MainStatus.Skill4);
			}
			else
			{
				SetStatus(MainStatus.Skill3);
			}
			break;
		}
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
			UseSword = true;
			EXMode = false;
			UseSkillCount = 0;
			EM192Count = 0;
			SwordObj.SetActive(false);
			CheckRoomSize();
			SetMaxGravity(OrangeBattleUtility.FP_MaxGravity * 2);
			_collideBullet.UpdateBulletData(EnemyWeapons[0].BulletData);
			_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			_collideBullet.Active(targetMask);
			SetStatus(MainStatus.Debut);
		}
		else
		{
			_collideBullet.BackToPool();
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
		return num + StartPos;
	}

	protected override void DeadBehavior(ref HurtPassParam tHurtPassParam)
	{
		CloseAllFX();
		IgnoreGravity = false;
		if (_mainStatus == MainStatus.Die)
		{
			return;
		}
		if ((bool)_collideBullet)
		{
			_collideBullet.BackToPool();
		}
		if ((bool)SwordCollide)
		{
			SwordCollide.BackToPool();
		}
		List<EM192_Controller> list = new List<EM192_Controller>();
		for (int i = 0; i < StageUpdate.runEnemys.Count; i++)
		{
			EM192_Controller eM192_Controller = StageUpdate.runEnemys[i].mEnemy as EM192_Controller;
			if ((bool)eM192_Controller)
			{
				list.Add(eM192_Controller);
			}
		}
		for (int j = 0; j < list.Count; j++)
		{
			list[j].SetDead(true);
		}
		StageUpdate.SlowStage();
		SetColliderEnable(false);
		SetStatus(MainStatus.Die);
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

	private void ShowHideMesh(bool show = true)
	{
		base.AllowAutoAim = show;
		SwitchMesh(BodyMesh, show);
		SwitchMesh(GlassMesh, show);
		SwitchMesh(HandMeshL, show);
		SwitchMesh(HandMeshR, show);
		SwordObj.SetActive(show);
		if (EXMode)
		{
			if (show)
			{
				AuraFX.Play();
			}
			else
			{
				AuraFX.Stop();
			}
		}
		SetColliderEnable(show);
	}

	private int CheckBlock(Vector3 RayOrigin)
	{
		int layerMask = (1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockLayer) | (1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockEnemyLayer) | (1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockPlayerLayer) | (1 << ManagedSingleton<OrangeLayerManager>.Instance.WallKickMask);
		RaycastHit2D raycastHit2D = OrangeBattleUtility.RaycastIgnoreSelf(RayOrigin, Vector3.left, 2f, layerMask, _transform);
		RaycastHit2D raycastHit2D2 = OrangeBattleUtility.RaycastIgnoreSelf(RayOrigin, Vector3.right, 2f, layerMask, _transform);
		if ((bool)raycastHit2D)
		{
			return -1;
		}
		if ((bool)raycastHit2D2)
		{
			return 1;
		}
		return 0;
	}

	public override ObscuredInt Hurt(HurtPassParam tHurtPassParam)
	{
		ObscuredInt result = base.Hurt(tHurtPassParam);
		if (!EXMode && (int)Hp <= (int)MaxHp / 2 && (int)Hp > 0)
		{
			EXMode = true;
			AuraFX.Play();
		}
		return result;
	}

	private void Skill0_1stSlashHitCallBack(object obj)
	{
		if (obj == null)
		{
			return;
		}
		Collider2D collider2D = obj as Collider2D;
		if (collider2D != null)
		{
			Target = OrangeBattleUtility.GetHitTargetOrangeCharacter(collider2D);
			if (Target != null)
			{
				Skill0_1stSlashHit = true;
			}
		}
	}

	private void CheckRoomSize()
	{
		int layerMask = (1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockLayer) | (1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockEnemyLayer) | (1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockPlayerLayer) | (1 << ManagedSingleton<OrangeLayerManager>.Instance.WallKickMask);
		Vector3 vector = new Vector3(_transform.position.x, _transform.position.y + 3f, 0f);
		RaycastHit2D raycastHit2D = OrangeBattleUtility.RaycastIgnoreSelf(vector, Vector3.left, 30f, layerMask, _transform);
		RaycastHit2D raycastHit2D2 = OrangeBattleUtility.RaycastIgnoreSelf(vector, Vector3.right, 30f, layerMask, _transform);
		RaycastHit2D raycastHit2D3 = OrangeBattleUtility.RaycastIgnoreSelf(vector, Vector3.down, 30f, layerMask, _transform);
		if (!raycastHit2D2 || !raycastHit2D)
		{
			Debug.LogError("沒有偵測左右牆壁，之後一些技能無法準確判斷位置");
			MinXPos = _transform.position.x - 12f;
			MaxXPos = _transform.position.x + 8f;
			GroundYPos = _transform.position.y;
		}
		else
		{
			MaxXPos = raycastHit2D2.point.x;
			MinXPos = raycastHit2D.point.x;
			GroundYPos = raycastHit2D3.point.y;
		}
	}

	private void CloseAllFX()
	{
		if ((bool)Skill0SlashFX1)
		{
			Skill0SlashFX1.Stop();
		}
		if ((bool)Skill0SlashFX2)
		{
			Skill0SlashFX2.Stop();
		}
		if ((bool)Skill0SlashFX3)
		{
			Skill0SlashFX3.Stop();
		}
		if ((bool)Skill1ChargeFX)
		{
			Skill1ChargeFX.Stop();
		}
		if ((bool)Skill5SummonFX1)
		{
			Skill5SummonFX1.Stop();
		}
		if ((bool)Skill5SummonFX2)
		{
			Skill5SummonFX2.Stop();
		}
		if ((bool)Skill6SummonFX1)
		{
			Skill6SummonFX1.Stop();
		}
		if ((bool)Skill6SummonFX2)
		{
			Skill6SummonFX2.Stop();
		}
		if ((bool)ShadowFX)
		{
			ShadowFX.Stop();
		}
		if ((bool)AuraFX)
		{
			AuraFX.Stop();
		}
	}

	private void SpawnEnemy(Vector3 pos)
	{
		EnemyControllerBase enemyControllerBase = StageUpdate.StageSpawnEnemyByMob(ManagedSingleton<OrangeDataManager>.Instance.MOB_TABLE_DICT[(int)EnemyWeapons[8].BulletData.f_EFFECT_X], sNetSerialID + SpawnCount);
		SpawnCount++;
		enemyControllerBase.SetPositionAndRotation(pos, base.direction != 1);
		enemyControllerBase.SetActive(true);
		EM192Count++;
	}
}
