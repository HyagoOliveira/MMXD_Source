#define RELEASE
using System;
using System.Collections.Generic;
using CallbackDefs;
using NaughtyAttributes;
using Newtonsoft.Json;
using StageLib;
using UnityEngine;

public class BS077_Controller : EnemyControllerBase, IManagedUpdateBehavior
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
		Die = 8
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
		MAX_SUBSTATUS = 10
	}

	public enum AnimationID
	{
		ANI_IDLE = 0,
		ANI_DEBUT = 1,
		ANI_DEBUT_LOOP = 2,
		ANI_WALK = 3,
		ANI_DEFENSE = 4,
		ANI_FALL = 5,
		ANI_LAND = 6,
		ANI_SKILL0_START = 7,
		ANI_SKILL0_LOOP = 8,
		ANI_SKILL0_END = 9,
		ANI_SKILL1_START1 = 10,
		ANI_SKILL1_LOOP1 = 11,
		ANI_SKILL1_START2 = 12,
		ANI_SKILL1_LOOP2 = 13,
		ANI_SKILL1_END2 = 14,
		ANI_SKILL2_START1 = 15,
		ANI_SKILL2_LOOP1 = 16,
		ANI_SKILL2_START2 = 17,
		ANI_SKILL2_LOOP2 = 18,
		ANI_SKILL2_START3 = 19,
		ANI_SKILL2_START4 = 20,
		ANI_SKILL2_START5 = 21,
		ANI_SKILL2_LOOP5 = 22,
		ANI_SKILL2_END5 = 23,
		ANI_SKILL3_START1 = 24,
		ANI_SKILL3_LOOP1 = 25,
		ANI_SKILL3_START2 = 26,
		ANI_SKILL3_LOOP2 = 27,
		ANI_SKILL3_END2 = 28,
		ANI_SKILL4_START = 29,
		ANI_SKILL4_LOOP = 30,
		ANI_SKILL4_END = 31,
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

	private int nDeadCount;

	private int[] _animationHash;

	private int[] DefaultSkillCard = new int[2] { 1, 2 };

	private static int[] DefaultRangedSkillCard = new int[3] { 0, 3, 4 };

	private bool isRanged = true;

	private List<int> RangedSKC = new List<int>();

	private List<int> SkillCard = new List<int>();

	private Vector3 LastTargetPos = new Vector3(0f, 0f, 0f);

	[Header("通用")]
	private Vector3 StartPos;

	private Vector3 EndPos;

	private float fMoveDis;

	[SerializeField]
	private Transform ShootPos;

	private Vector3 MaxPos;

	private Vector3 MinPos;

	private float GroundYPos;

	private int ActionTimes;

	private float ActionAnimatorFrame;

	private int ActionFrame;

	private bool HasActed;

	[SerializeField]
	private SkinnedMeshRenderer CloakMesh1;

	[SerializeField]
	private SkinnedMeshRenderer CloakMesh2;

	[SerializeField]
	private SkinnedMeshRenderer ShoulderArmorMesh;

	[Header("追蹤彈")]
	[SerializeField]
	private int nSk0ActTimes = 5;

	[SerializeField]
	private float fSk0IntervalAngle = 45f;

	[SerializeField]
	private float fSk0ActIntervalTime = 0.2f;

	private List<BS066_Skill2Bullet> Sk0BulletList = new List<BS066_Skill2Bullet>();

	[Header("衝刺揮斬")]
	[SerializeField]
	private int nSk1Spd = 18000;

	private CollideBullet SwordCollider;

	[SerializeField]
	private string sSwordColliderObj = "SwordCollider";

	[SerializeField]
	private SkinnedMeshRenderer SwordMesh;

	[SerializeField]
	private ParticleSystem SwordFX;

	[SerializeField]
	private ParticleSystem psSk1UseFX1;

	[SerializeField]
	private ParticleSystem psSk1UseFX2;

	[Header("三角踢")]
	[SerializeField]
	private int nSk2ActTimes = 2;

	[SerializeField]
	private int nSk2Spd = 18000;

	[SerializeField]
	private float fSk2JumpAngle = 30f;

	[SerializeField]
	private string sSk2UseFX = "fxuse_SIGMA_001";

	[SerializeField]
	private ParticleSystem psSk2UseFX;

	[Header("電磁波")]
	[SerializeField]
	private SkinnedMeshRenderer GunMesh;

	[SerializeField]
	private SkinnedMeshRenderer HandMesh;

	[Header("火焰彈")]
	[SerializeField]
	private int nSk4ActTimes = 3;

	[Header("格擋")]
	[SerializeField]
	private float fSk5ActTime = 2f;

	[Header("待機等待Frame")]
	[SerializeField]
	private float IdleWaitTime = 0.2f;

	private int IdleWaitFrame;

	[Header("Debug用")]
	[SerializeField]
	private bool DebugMode;

	[SerializeField]
	private MainStatus NextSkill;

	private Vector3 NowPos
	{
		get
		{
			return _transform.position;
		}
	}

	private void SwitchFallDownSE(bool turn)
	{
		FallDownSE = new string[0];
		if (turn)
		{
			FallDownSE = new string[2] { "BossSE06", "bs053_sigma02" };
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
		_animationHash = new int[34];
		_animationHash[0] = Animator.StringToHash("BS0767@idl_loop");
		_animationHash[1] = Animator.StringToHash("BS077@debut");
		_animationHash[2] = Animator.StringToHash("BS077@debut_loop");
		_animationHash[3] = Animator.StringToHash("BS077@walk_loop");
		_animationHash[4] = Animator.StringToHash("BS077@defense_loop");
		_animationHash[5] = Animator.StringToHash("buster_fall_loop");
		_animationHash[6] = Animator.StringToHash("buster_landing");
		_animationHash[7] = Animator.StringToHash("BS077@SIGMA_SKILL1_CASTING1");
		_animationHash[8] = Animator.StringToHash("BS077@SIGMA_SKILL1_CASTLOOP1");
		_animationHash[9] = Animator.StringToHash("BS077@SIGMA_SKILL1_CASTOUT1");
		_animationHash[10] = Animator.StringToHash("ch038_skill_01_stand_1st_start");
		_animationHash[11] = Animator.StringToHash("ch038_skill_01_stand_1st_loop");
		_animationHash[12] = Animator.StringToHash("ch038_skill_01_stand_1st_atk_start");
		_animationHash[13] = Animator.StringToHash("ch038_skill_01_stand_1st_atk_loop");
		_animationHash[14] = Animator.StringToHash("ch038_skill_01_stand_1st_atk_end");
		_animationHash[15] = Animator.StringToHash("BS077@SIGMA_SKILL3_CASTING1");
		_animationHash[16] = Animator.StringToHash("BS077@SIGMA_SKILL3_CASTLOOP1");
		_animationHash[17] = Animator.StringToHash("BS077@SIGMA_SKILL3_CASTING2");
		_animationHash[18] = Animator.StringToHash("BS077@SIGMA_SKILL3_CASTLOOP2");
		_animationHash[19] = Animator.StringToHash("BS077@SIGMA_SKILL3_CASTING3");
		_animationHash[20] = Animator.StringToHash("BS077@SIGMA_SKILL3_CASTING4");
		_animationHash[21] = Animator.StringToHash("BS077@SIGMA_SKILL3_CASTING5");
		_animationHash[22] = Animator.StringToHash("BS077@SIGMA_SKILL3_CASTLOOP3");
		_animationHash[23] = Animator.StringToHash("BS077@SIGMA_SKILL3_CASTOUT1");
		_animationHash[24] = Animator.StringToHash("BS077@SIGMA_SKILL4_CASTING1");
		_animationHash[25] = Animator.StringToHash("BS077@SIGMA_SKILL4_CASTLOOP1");
		_animationHash[26] = Animator.StringToHash("BS077@SIGMA_SKILL4_CASTING2");
		_animationHash[27] = Animator.StringToHash("BS077@SIGMA_SKILL4_CASTLOOP2");
		_animationHash[28] = Animator.StringToHash("BS077@SIGMA_SKILL4_CASTOUT1");
		_animationHash[29] = Animator.StringToHash("BS077@SIGMA_SKILL5_CASTING1");
		_animationHash[30] = Animator.StringToHash("BS077@SIGMA_SKILL5_CASTLOOP1");
		_animationHash[31] = Animator.StringToHash("BS077@SIGMA_SKILL5_CASTOUT1");
		_animationHash[32] = Animator.StringToHash("BS077@hurt");
		_animationHash[33] = Animator.StringToHash("BS077@dead");
	}

	private void LoadParts(ref Transform[] childs)
	{
		ModelTransform = OrangeBattleUtility.FindChildRecursive(ref childs, "model", true);
		_animator = ModelTransform.GetComponent<Animator>();
		_collideBullet = OrangeBattleUtility.FindChildRecursive(ref childs, "Collider", true).gameObject.AddOrGetComponent<CollideBullet>();
		SwordCollider = OrangeBattleUtility.FindChildRecursive(ref childs, "SwordCollider", true).gameObject.AddOrGetComponent<CollideBullet>();
		if (ShootPos == null)
		{
			ShootPos = OrangeBattleUtility.FindChildRecursive(ref childs, "Bip001 Neck", true);
		}
		if (psSk1UseFX1 == null)
		{
			psSk1UseFX1 = OrangeBattleUtility.FindChildRecursive(ref childs, "psSk1UseFX1", true).GetComponent<ParticleSystem>();
		}
		if (psSk1UseFX2 == null)
		{
			psSk1UseFX2 = OrangeBattleUtility.FindChildRecursive(ref childs, "psSk1UseFX2", true).GetComponent<ParticleSystem>();
		}
		if (psSk2UseFX == null)
		{
			psSk2UseFX = OrangeBattleUtility.FindChildRecursive(ref childs, "psSk2UseFX", true).GetComponent<ParticleSystem>();
		}
		if (SwordFX == null)
		{
			SwordFX = OrangeBattleUtility.FindChildRecursive(ref childs, "SwordFX", true).GetComponent<ParticleSystem>();
		}
		if (CloakMesh1 == null)
		{
			CloakMesh1 = OrangeBattleUtility.FindChildRecursive(ref childs, "CloakInsideMesh_c", true).gameObject.AddOrGetComponent<SkinnedMeshRenderer>();
		}
		if (CloakMesh2 == null)
		{
			CloakMesh2 = OrangeBattleUtility.FindChildRecursive(ref childs, "CloakOutsideMesh_c", true).gameObject.AddOrGetComponent<SkinnedMeshRenderer>();
		}
		if (ShoulderArmorMesh == null)
		{
			ShoulderArmorMesh = OrangeBattleUtility.FindChildRecursive(ref childs, "ShoulderArmorMesh_m", true).gameObject.AddOrGetComponent<SkinnedMeshRenderer>();
		}
		if (SwordMesh == null)
		{
			SwordMesh = OrangeBattleUtility.FindChildRecursive(ref childs, "SaberMesh_m", true).gameObject.AddOrGetComponent<SkinnedMeshRenderer>();
		}
		if (HandMesh == null)
		{
			HandMesh = OrangeBattleUtility.FindChildRecursive(ref childs, "HandMesh_R_m", true).gameObject.AddOrGetComponent<SkinnedMeshRenderer>();
		}
		if (GunMesh == null)
		{
			GunMesh = OrangeBattleUtility.FindChildRecursive(ref childs, "SaberMesh_R_m", true).gameObject.AddOrGetComponent<SkinnedMeshRenderer>();
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
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(sSk2UseFX, 3);
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
		}
		IsInvincible = false;
		SwitchMesh(SwordMesh, false);
		SwitchFx(SwordFX, false);
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
			case SubStatus.Phase1:
				HasActed = false;
				PlayBossSE06("bs053_sigma01");
				break;
			case SubStatus.Phase2:
				PlayBossSE06("bs053_sigma02");
				SwitchMesh(CloakMesh1, false);
				SwitchMesh(CloakMesh2, false);
				SwitchMesh(ShoulderArmorMesh, false);
				break;
			}
			break;
		case MainStatus.Idle:
			IsInvincible = true;
			SwitchMesh(SwordMesh, true);
			SwitchFx(SwordFX, true);
			_velocity.x = 0;
			IdleWaitFrame = GameLogicUpdateManager.GameFrame + (int)(IdleWaitTime * 20f);
			break;
		case MainStatus.Skill0:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				PlayBossSE06("bs053_sigma03");
				_velocity = VInt3.zero;
				ActionTimes = 0;
				break;
			case SubStatus.Phase1:
			{
				StartPos = NowPos + Vector3.up * 1.2f;
				Sk0BulletList.Clear();
				for (int i = 0; i < nSk0ActTimes; i++)
				{
					EndPos = StartPos + Quaternion.Euler(0f, 0f, (float)i * fSk0IntervalAngle) * (Vector3.left * 1.5f);
					BS066_Skill2Bullet bS066_Skill2Bullet = BulletBase.TryShotBullet(EnemyWeapons[1].BulletData, StartPos, EndPos, null, selfBuffManager.sBuffStatus, EnemyData, targetMask) as BS066_Skill2Bullet;
					bS066_Skill2Bullet.SetNextTrackTime(1f + 0.2f * (float)i);
					Sk0BulletList.Add(bS066_Skill2Bullet);
				}
				break;
			}
			}
			break;
		case MainStatus.Skill1:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				SwitchFallDownSE(false);
				PlayBossSE06("bs053_sigma08");
				IgnoreGravity = true;
				StartPos = NowPos;
				EndPos = GetTargetPos();
				UpdateDirection();
				if (Mathf.Abs(EndPos.x - NowPos.x) > 0.75f)
				{
					EndPos -= Vector3.right * base.direction * 0.75f;
				}
				fMoveDis = Vector2.Distance(StartPos, EndPos);
				SwitchMesh(SwordMesh, true);
				SwitchFx(SwordFX, true);
				break;
			case SubStatus.Phase2:
				PlayBossSE06("bs053_sigma09");
				_velocity = VInt3.zero;
				SwitchCollideBullet(SwordCollider, true, 2);
				SwitchFx(psSk1UseFX1, true);
				SwitchFx(psSk1UseFX2, false);
				break;
			case SubStatus.Phase4:
				SwitchMesh(SwordMesh, false);
				SwitchFx(SwordFX, false);
				IgnoreGravity = false;
				SwitchCollideBullet(SwordCollider, false);
				break;
			case SubStatus.Phase5:
				SwitchMesh(SwordMesh, false);
				SwitchFx(SwordFX, false);
				IgnoreGravity = false;
				SwitchCollideBullet(SwordCollider, false);
				break;
			}
			break;
		case MainStatus.Skill2:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_velocity = VInt3.zero;
				ActionTimes = nSk2ActTimes;
				EndPos = GetTargetPos();
				UpdateDirection();
				IgnoreGravity = true;
				SwitchMesh(SwordMesh, true);
				SwitchFx(SwordFX, true);
				SwitchCollideBullet(_collideBullet, true, 3);
				break;
			case SubStatus.Phase2:
				UpdateDirection(-base.direction);
				_velocity = VInt3.zero;
				break;
			case SubStatus.Phase4:
				_velocity = new VInt3(Quaternion.Euler(0f, 0f, fSk2JumpAngle * (float)(-base.direction)) * Vector3.up * nSk2Spd * 0.001f);
				SwitchFx(sSk2UseFX, NowPos);
				break;
			case SubStatus.Phase5:
				SwitchMesh(SwordMesh, false);
				SwitchFx(SwordFX, false);
				IgnoreGravity = false;
				_velocity = VInt3.zero;
				SwitchCollideBullet(_collideBullet, true, 0);
				break;
			case SubStatus.Phase6:
				SwitchMesh(SwordMesh, false);
				SwitchFx(SwordFX, false);
				IgnoreGravity = false;
				_velocity = VInt3.zero;
				SwitchCollideBullet(_collideBullet, true, 0);
				break;
			}
			break;
		case MainStatus.Skill3:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				EndPos = GetTargetPos();
				UpdateDirection();
				break;
			case SubStatus.Phase2:
				SwitchMesh(GunMesh, true);
				SwitchMesh(HandMesh, false);
				EndPos = GetTargetPos();
				UpdateDirection();
				break;
			case SubStatus.Phase3:
				BulletBase.TryShotBullet(EnemyWeapons[4].BulletData, ShootPos.position + Vector3.right * 0.6f * base.direction + Vector3.up * 0.3f, Vector3.right * base.direction, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
				break;
			case SubStatus.Phase4:
				SwitchMesh(HandMesh, true);
				SwitchMesh(GunMesh, false);
				break;
			}
			break;
		case MainStatus.Skill4:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				PlayBossSE06("bs053_sigma12");
				SwitchMesh(GunMesh, true);
				SwitchMesh(HandMesh, false);
				ActionTimes = nSk4ActTimes;
				EndPos = GetTargetPos();
				UpdateDirection();
				break;
			case SubStatus.Phase1:
			{
				BS105Skill0Bullet obj = BulletBase.TryShotBullet(EnemyWeapons[5].BulletData, ShootPos.position + Vector3.right * 1f * base.direction, Vector3.right * base.direction, null, selfBuffManager.sBuffStatus, EnemyData, targetMask) as BS105Skill0Bullet;
				obj.BackCallback = Skill4WallFire;
				float num = ActionTimes % 3 - 1;
				obj.SetAmplitude(num * 0.8f * (float)base.direction);
				break;
			}
			case SubStatus.Phase2:
				SwitchMesh(HandMesh, true);
				SwitchMesh(GunMesh, false);
				break;
			}
			break;
		case MainStatus.Skill5:
			if (_subStatus == SubStatus.Phase0)
			{
				SwitchMesh(SwordMesh, true);
				SwitchFx(SwordFX, true);
				ActionFrame = GameLogicUpdateManager.GameFrame + (int)(fSk5ActTime * 20f);
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
				if (!Controller.Collisions.below)
				{
					SetStatus(MainStatus.Die, SubStatus.Phase2);
					return;
				}
				break;
			case SubStatus.Phase1:
				if (AiState != 0)
				{
					StartCoroutine(BossDieFlow(_transform.position, "FX_BOSS_EXPLODE2", false, false));
				}
				break;
			case SubStatus.Phase2:
				IgnoreGravity = false;
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
				_currentAnimationId = AnimationID.ANI_DEBUT_LOOP;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_DEBUT;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_IDLE;
				break;
			}
			break;
		case MainStatus.Idle:
			_currentAnimationId = AnimationID.ANI_DEFENSE;
			break;
		case MainStatus.Skill0:
			switch (_subStatus)
			{
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
				_currentAnimationId = AnimationID.ANI_SKILL1_END2;
				break;
			case SubStatus.Phase5:
				_currentAnimationId = AnimationID.ANI_FALL;
				break;
			case SubStatus.Phase6:
				_currentAnimationId = AnimationID.ANI_LAND;
				break;
			}
			break;
		case MainStatus.Skill2:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_SKILL2_START1;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_SKILL2_LOOP1;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_SKILL2_START2;
				break;
			case SubStatus.Phase3:
				_currentAnimationId = AnimationID.ANI_SKILL2_LOOP2;
				break;
			case SubStatus.Phase4:
				_currentAnimationId = AnimationID.ANI_SKILL2_START3;
				break;
			case SubStatus.Phase5:
				_currentAnimationId = AnimationID.ANI_SKILL2_START4;
				break;
			case SubStatus.Phase6:
				_currentAnimationId = AnimationID.ANI_SKILL2_START5;
				break;
			case SubStatus.Phase7:
				_currentAnimationId = AnimationID.ANI_SKILL2_LOOP5;
				break;
			case SubStatus.Phase8:
				_currentAnimationId = AnimationID.ANI_SKILL2_END5;
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
				_currentAnimationId = AnimationID.ANI_SKILL3_END2;
				break;
			}
			break;
		case MainStatus.Skill4:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_SKILL4_START;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_SKILL4_LOOP;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_SKILL4_END;
				break;
			}
			break;
		case MainStatus.Skill5:
			if (_subStatus == SubStatus.Phase0)
			{
				_currentAnimationId = AnimationID.ANI_DEFENSE;
			}
			break;
		case MainStatus.Die:
			switch (_subStatus)
			{
			default:
				return;
			case SubStatus.Phase0:
				if (AiState == AI_STATE.mob_001)
				{
					_currentAnimationId = AnimationID.ANI_IDLE;
				}
				else
				{
					_currentAnimationId = AnimationID.ANI_DEAD;
				}
				break;
			case SubStatus.Phase1:
				if (AiState == AI_STATE.mob_001)
				{
					_currentAnimationId = AnimationID.ANI_IDLE;
					break;
				}
				return;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_FALL;
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
				mainStatus = (MainStatus)RandomCard(2);
				isRanged = !isRanged;
				break;
			}
		}
		if (DebugMode)
		{
			mainStatus = NextSkill;
		}
		if (mainStatus != 0)
		{
			IsInvincible = false;
			SwitchMesh(SwordMesh, false);
			SwitchFx(SwordFX, false);
			if (CheckHost())
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
				if (_introReady)
				{
					SetStatus(MainStatus.Debut, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if (!HasActed && _currentFrame > 0.85f)
				{
					HasActed = true;
					SwitchMesh(SwordMesh, true);
					SwitchFx(SwordFX, true);
				}
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Debut, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
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
			if (bWaitNetStatus || IdleWaitFrame >= GameLogicUpdateManager.GameFrame)
			{
				break;
			}
			Target = _enemyAutoAimSystem.GetClosetPlayer();
			if (!Target)
			{
				for (int i = 0; i < StageUpdate.runPlayers.Count; i++)
				{
					if ((int)StageUpdate.runPlayers[i].Hp > 0)
					{
						Target = StageUpdate.runPlayers[i];
						break;
					}
				}
			}
			if ((bool)Target)
			{
				UpdateDirection();
				UpdateRandomState();
				IsInvincible = false;
				SwitchMesh(SwordMesh, false);
				SwitchFx(SwordFX, false);
			}
			else
			{
				UpdateRandomState();
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
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill0, SubStatus.Phase2);
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
		case MainStatus.Skill1:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (_velocity == VInt3.zero)
				{
					_velocity = new VInt3((EndPos - NowPos).normalized) * nSk1Spd * 0.001f;
					SwitchFx(psSk1UseFX2, true);
				}
				if (Vector2.Distance(NowPos, StartPos) >= fMoveDis || Controller.Collisions.left || Controller.Collisions.right || Controller.Collisions.above)
				{
					int num = 0;
					int layerMask = (1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockLayer) | (1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockEnemyLayer) | (1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockPlayerLayer) | (1 << ManagedSingleton<OrangeLayerManager>.Instance.WallKickMask);
					RaycastHit2D raycastHit2D = OrangeBattleUtility.RaycastIgnoreSelf(EndPos, Vector3.left, 0.61f, layerMask, _transform);
					RaycastHit2D raycastHit2D2 = OrangeBattleUtility.RaycastIgnoreSelf(EndPos, Vector3.right, 0.61f, layerMask, _transform);
					if ((bool)raycastHit2D)
					{
						num = 1;
					}
					if ((bool)raycastHit2D2)
					{
						num = -1;
					}
					_transform.position = Vector3.right * ((float)num * 0.3f) + EndPos;
					Controller.LogicPosition = new VInt3(Vector3.right * ((float)num * 0.3f) + EndPos);
					SetStatus(MainStatus.Skill1, SubStatus.Phase2);
				}
				else if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill1, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if (Vector2.Distance(NowPos, StartPos) >= fMoveDis || Controller.Collisions.left || Controller.Collisions.right || Controller.Collisions.above)
				{
					int num2 = 0;
					int layerMask2 = (1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockLayer) | (1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockEnemyLayer) | (1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockPlayerLayer) | (1 << ManagedSingleton<OrangeLayerManager>.Instance.WallKickMask);
					RaycastHit2D raycastHit2D3 = OrangeBattleUtility.RaycastIgnoreSelf(EndPos, Vector3.left, 0.61f, layerMask2, _transform);
					RaycastHit2D raycastHit2D4 = OrangeBattleUtility.RaycastIgnoreSelf(EndPos, Vector3.right, 0.61f, layerMask2, _transform);
					if ((bool)raycastHit2D3)
					{
						num2 = 1;
					}
					if ((bool)raycastHit2D4)
					{
						num2 = -1;
					}
					_transform.position = Vector3.right * ((float)num2 * 0.3f) + EndPos;
					Controller.LogicPosition = new VInt3(Vector3.right * ((float)num2 * 0.3f) + EndPos);
					SetStatus(MainStatus.Skill1, SubStatus.Phase2);
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
					if (NowPos.y < MinPos.y + 0.1f)
					{
						IgnoreGravity = false;
						SwitchCollideBullet(SwordCollider, false);
						SwitchMesh(SwordMesh, false);
						SwitchFx(SwordFX, false);
						SetStatus(MainStatus.Skill1, SubStatus.Phase4);
					}
					else
					{
						SetStatus(MainStatus.Skill1, SubStatus.Phase5);
					}
				}
				break;
			case SubStatus.Phase4:
				if (_currentFrame > 1f)
				{
					SwitchFallDownSE(true);
					SetStatus(MainStatus.Idle);
				}
				break;
			case SubStatus.Phase5:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill1, SubStatus.Phase6);
				}
				break;
			case SubStatus.Phase6:
				if (_currentFrame > 1f)
				{
					SwitchFallDownSE(true);
					SetStatus(MainStatus.Idle);
				}
				break;
			}
			break;
		case MainStatus.Skill2:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (_velocity == VInt3.zero)
				{
					_velocity = new VInt3(Quaternion.Euler(0f, 0f, fSk2JumpAngle * (float)(-base.direction)) * Vector3.up * nSk2Spd * 0.001f);
					SwitchFx(sSk2UseFX, NowPos);
				}
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill2, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if (Controller.Collisions.below)
				{
					SetStatus(MainStatus.Skill2, SubStatus.Phase5);
				}
				if (Controller.Collisions.left || Controller.Collisions.right)
				{
					PlayBossSE06("bs053_sigma06");
					if (--ActionTimes > 0)
					{
						SetStatus(MainStatus.Skill2, SubStatus.Phase2);
					}
					else
					{
						SetStatus(MainStatus.Skill2, SubStatus.Phase6);
					}
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
					SetStatus(MainStatus.Skill2, SubStatus.Phase4);
				}
				break;
			case SubStatus.Phase4:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill2, SubStatus.Phase1);
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
					SetStatus(MainStatus.Skill2, SubStatus.Phase7);
				}
				break;
			case SubStatus.Phase7:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill2, SubStatus.Phase8);
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
				if (_currentFrame > 1f)
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
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill3, SubStatus.Phase4);
				}
				break;
			case SubStatus.Phase4:
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
				if (_currentFrame > 1f)
				{
					if (--ActionTimes > 0)
					{
						SetStatus(MainStatus.Skill4, SubStatus.Phase1);
					}
					else
					{
						SetStatus(MainStatus.Skill4, SubStatus.Phase2);
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
		case MainStatus.Skill5:
			if (_subStatus == SubStatus.Phase0 && GameLogicUpdateManager.GameFrame >= ActionFrame)
			{
				SwitchMesh(SwordMesh, false);
				SwitchFx(SwordFX, false);
				SetStatus(MainStatus.Idle);
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
					SetStatus(MainStatus.Die, SubStatus.Phase3);
				}
				break;
			case SubStatus.Phase2:
				if (Controller.Collisions.below)
				{
					SetStatus(MainStatus.Die, SubStatus.Phase1);
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
			CheckRoomSize();
			SwitchCollideBullet(_collideBullet, true, 0);
			SwitchCollideBullet(SwordCollider, false, 2);
			SwitchMesh(SwordMesh, false);
			SetStatus(MainStatus.Debut);
		}
		else
		{
			_collideBullet.BackToPool();
		}
	}

	private int RandomCard(int StartPos)
	{
		if (isRanged)
		{
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
		}
		int num2 = SkillCard[OrangeBattleUtility.Random(0, SkillCard.ToArray().Length)];
		SkillCard.Remove(num2);
		return num2 + StartPos;
	}

	protected override void DeadBehavior(ref HurtPassParam tHurtPassParam)
	{
		if (_mainStatus == MainStatus.Die)
		{
			return;
		}
		if ((bool)_collideBullet)
		{
			_collideBullet.BackToPool();
		}
		if ((bool)SwordCollider)
		{
			SwordCollider.BackToPool();
		}
		SwitchFallDownSE(true);
		StageUpdate.SlowStage();
		SetColliderEnable(false);
		SetStatus(MainStatus.Die);
		IsInvincible = false;
		SwitchMesh(HandMesh, true);
		SwitchMesh(GunMesh, false);
		SwitchMesh(SwordMesh, false);
		SwitchFx(SwordFX, false);
		SwitchFx(psSk1UseFX2, false);
		foreach (BS066_Skill2Bullet sk0Bullet in Sk0BulletList)
		{
			if (sk0Bullet != null && !sk0Bullet.bIsEnd)
			{
				sk0Bullet.BackToPool();
			}
		}
	}

	private Vector3 GetTargetPos(bool realcenter = false)
	{
		if (!Target)
		{
			Target = _enemyAutoAimSystem.GetClosetPlayer();
		}
		if (!Target)
		{
			for (int i = 0; i < StageUpdate.runPlayers.Count; i++)
			{
				if ((bool)StageUpdate.runPlayers[i])
				{
					Target = StageUpdate.runPlayers[i];
					break;
				}
			}
		}
		if ((bool)Target)
		{
			if (realcenter)
			{
				TargetPos = new VInt3(Target.Controller.GetRealCenterPos());
			}
			else
			{
				TargetPos = Target.Controller.LogicPosition;
			}
			return TargetPos.vec3;
		}
		return NowPos + Vector3.right * 3f * base.direction;
	}

	private void SwitchCollideBullet(CollideBullet collide, bool onoff, int weaponid = -1, int targetlayer = 0)
	{
		if (!collide)
		{
			return;
		}
		if (weaponid != -1)
		{
			collide.UpdateBulletData(EnemyWeapons[weaponid].BulletData);
			collide.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
		}
		if (onoff)
		{
			if (!collide.IsActivate)
			{
				switch (targetlayer)
				{
				case 1:
					collide.Active(friendMask);
					break;
				case 2:
					collide.Active(neutralMask);
					break;
				default:
					collide.Active(targetMask);
					break;
				}
			}
		}
		else if (collide.IsActivate)
		{
			collide.BackToPool();
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

	private void SwitchFx(string Fx, Vector3 pos)
	{
		MonoBehaviourSingleton<FxManager>.Instance.PlayReturn<FxBase>(Fx, pos, Quaternion.Euler(0f, 0f, 0f), new object[1]
		{
			new Vector3(1f, 1f, 1f)
		}).transform.localScale = new Vector3(1f * (float)base.direction, 1f, 1f);
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

	private void CheckRoomSize()
	{
		int layerMask = (1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockLayer) | (1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockEnemyLayer) | (1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockPlayerLayer) | (1 << ManagedSingleton<OrangeLayerManager>.Instance.WallKickMask);
		RaycastHit2D raycastHit2D = OrangeBattleUtility.RaycastIgnoreSelf(new Vector3(_transform.position.x, _transform.position.y + 1f, 0f), Vector3.down, 30f, layerMask, _transform);
		bool flag = false;
		if (!raycastHit2D)
		{
			flag = true;
			Debug.LogError("沒有偵測到地板，之後一些技能無法準確判斷位置");
		}
		if (flag)
		{
			MaxPos = new Vector3(NowPos.x + 3f, NowPos.y + 6f, 0f);
			MinPos = new Vector3(NowPos.x - 6f, NowPos.y, 0f);
		}
		else
		{
			MinPos = new Vector3(_transform.position.x, raycastHit2D.point.y, 0f);
		}
	}

	private void Skill4WallFire(object obj)
	{
		BasicBullet basicBullet = null;
		if (obj != null)
		{
			basicBullet = obj as BasicBullet;
		}
		if (basicBullet == null)
		{
			Debug.LogError("子彈資料有誤。");
		}
		else if (basicBullet.isHitBlock)
		{
			BulletBase.TryShotBullet(EnemyWeapons[6].BulletData, basicBullet._transform.position, Vector3.up, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
		}
	}

	public override void UpdateEnemyID(int id)
	{
		base.UpdateEnemyID(id);
		UpdateAIState();
	}
}
