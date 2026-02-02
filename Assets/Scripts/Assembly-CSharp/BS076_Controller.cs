#define RELEASE
using System;
using System.Collections.Generic;
using CallbackDefs;
using NaughtyAttributes;
using Newtonsoft.Json;
using StageLib;
using UnityEngine;

public class BS076_Controller : EnemyControllerBase, IManagedUpdateBehavior
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
		ANI_RUN = 3,
		ANI_SKILL0_START1 = 4,
		ANI_SKILL0_LOOP1 = 5,
		ANI_SKILL0_START2 = 6,
		ANI_SKILL0_LOOP2 = 7,
		ANI_SKILL0_START3 = 8,
		ANI_SKILL0_LOOP3 = 9,
		ANI_SKILL0_END3 = 10,
		ANI_SKILL1_START = 11,
		ANI_SKILL1_LOOP = 12,
		ANI_SKILL1_END = 13,
		ANI_SKILL2_START1 = 14,
		ANI_SKILL2_LOOP1 = 15,
		ANI_SKILL2_START2 = 16,
		ANI_SKILL2_LOOP2 = 17,
		ANI_SKILL2_START3 = 18,
		ANI_SKILL2_LOOP3 = 19,
		ANI_SKILL2_END3 = 20,
		ANI_SKILL3_START1 = 21,
		ANI_SKILL3_LOOP1 = 22,
		ANI_SKILL3_START2 = 23,
		ANI_SKILL3_LOOP2 = 24,
		ANI_SKILL3_END2 = 25,
		ANI_SKILL4_START1 = 26,
		ANI_SKILL4_LOOP1 = 27,
		ANI_SKILL4_START2 = 28,
		ANI_SKILL4_LOOP2 = 29,
		ANI_SKILL4_START3 = 30,
		ANI_SKILL4_START4 = 31,
		ANI_SKILL4_LOOP4 = 32,
		ANI_SKILL4_END4 = 33,
		ANI_HURT = 34,
		ANI_DEAD = 35,
		MAX_ANIMATION_ID = 36
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

	[Header("通用")]
	private Vector3 StartPos;

	private Vector3 EndPos;

	[SerializeField]
	private Transform ShootPos;

	private Vector3 MaxPos;

	private Vector3 MinPos;

	private Vector3 CenterPos;

	private float GroundYPos;

	private int ActionTimes;

	private float ActionAnimatorFrame;

	private int ActionFrame;

	private bool HasActed;

	private float ShotAngle;

	private readonly int _HashAngle = Animator.StringToHash("Angle");

	[SerializeField]
	private SkinnedMeshRenderer HandMesh;

	[SerializeField]
	private SkinnedMeshRenderer GunMesh;

	[SerializeField]
	private SkinnedMeshRenderer SwordMesh1;

	[SerializeField]
	private SkinnedMeshRenderer SwordMesh2;

	[SerializeField]
	private float IdleWaitTime = 0.2f;

	private int IdleWaitFrame;

	[Header("迴旋手裡劍")]
	[SerializeField]
	private int Skill0ShootTimes = 1;

	[SerializeField]
	private float Skill0ShootFrame = 0.4f;

	[SerializeField]
	private ParticleSystem SwordFX;

	private BulletBase Skill0Bullet;

	[Header("迴旋手裡劍．改")]
	[SerializeField]
	private int Skill1ShootTimes = 4;

	[SerializeField]
	private float Skill1ShootTime = 1f;

	[SerializeField]
	private ParticleSystem Skill1FX;

	[Header("雷射投彈")]
	[SerializeField]
	private int Skill2ShootTimes = 3;

	[SerializeField]
	private float Skill2ShootFrame = 0.6f;

	[SerializeField]
	private float Skill2JumpTime = 0.75f;

	[SerializeField]
	private float Skill2ActFrame = 0.45f;

	[SerializeField]
	private float Skill2WallDis = 1f;

	[Header("爆發光束")]
	[SerializeField]
	private float Skill3AtkSpace = 1.5f;

	[SerializeField]
	private float Skill3ChargeTime = 1f;

	[SerializeField]
	private float Skill3AtkTime = 1f;

	[SerializeField]
	private float Skill3ActFrame = 0.35f;

	[SerializeField]
	private int Skill3Atknum = 4;

	[SerializeField]
	private ParticleSystem Skill3FX1;

	[SerializeField]
	private ParticleSystem Skill3FX2;

	[Header("燕返")]
	[SerializeField]
	private float Skill4ActFrame = 0.16f;

	[SerializeField]
	private float Skill4JumpTime = 0.5f;

	private CollideBullet Skill4CollideBullet;

	[SerializeField]
	private string Skill4BulletObj = "Skill4Collider";

	[SerializeField]
	private ParticleSystem Skill4FX;

	[SerializeField]
	private float Skill4GravityMulti = 0.75f;

	[SerializeField]
	private bool DebugMode;

	[SerializeField]
	private MainStatus NextSkill;

	private bool bPlayLP;

	private int bldCnt;

	private int Gravity
	{
		get
		{
			MainStatus mainStatus = _mainStatus;
			if (mainStatus == MainStatus.Skill4)
			{
				return (int)(Skill4GravityMulti * (float)(OrangeBattleUtility.FP_Gravity * GameLogicUpdateManager.g_fixFrameLenFP) / 1000f);
			}
			return OrangeBattleUtility.FP_Gravity * GameLogicUpdateManager.g_fixFrameLenFP / 1000;
		}
	}

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
		_animationHash = new int[36];
		_animationHash[0] = Animator.StringToHash("BS076@idl_loop");
		_animationHash[1] = Animator.StringToHash("BS076@debut");
		_animationHash[2] = Animator.StringToHash("BS076@debut_loop");
		_animationHash[3] = Animator.StringToHash("BS076@run_loop");
		_animationHash[4] = Animator.StringToHash("BS076@DYNAMO_SKILL1_CASTING1");
		_animationHash[5] = Animator.StringToHash("BS076@DYNAMO_SKILL1_CASTLOOP1");
		_animationHash[6] = Animator.StringToHash("BS076@DYNAMO_SKILL1_CASTING2");
		_animationHash[7] = Animator.StringToHash("BS076@DYNAMO_SKILL1_CASTLOOP2");
		_animationHash[8] = Animator.StringToHash("BS076@DYNAMO_SKILL1_CASTING3");
		_animationHash[9] = Animator.StringToHash("BS076@DYNAMO_SKILL1_CASTLOOP3");
		_animationHash[10] = Animator.StringToHash("BS076@DYNAMO_SKILL1_CASTOUT1");
		_animationHash[11] = Animator.StringToHash("BS076@DYNAMO_SKILL2_CASTING1");
		_animationHash[12] = Animator.StringToHash("BS076@DYNAMO_SKILL2_CASTLOOP1");
		_animationHash[13] = Animator.StringToHash("BS076@DYNAMO_SKILL2_CASTOUT1");
		_animationHash[14] = Animator.StringToHash("BS076@DYNAMO_SKILL3_CASTING1");
		_animationHash[15] = Animator.StringToHash("BS076@DYNAMO_SKILL3_CASTLOOP1");
		_animationHash[16] = Animator.StringToHash("BS076@DYNAMO_SKILL3_CASTING2");
		_animationHash[17] = Animator.StringToHash("BS076@DYNAMO_SKILL3_CASTLOOP2");
		_animationHash[18] = Animator.StringToHash("BS076@DYNAMO_SKILL3_CASTING3");
		_animationHash[19] = Animator.StringToHash("BS076@DYNAMO_SKILL3_CASTLOOP3");
		_animationHash[20] = Animator.StringToHash("BS076@DYNAMO_SKILL3_CASTOUT1");
		_animationHash[21] = Animator.StringToHash("BS076@DYNAMO_SKILL4_CASTING1");
		_animationHash[22] = Animator.StringToHash("BS076@DYNAMO_SKILL4_CASTLOOP1");
		_animationHash[23] = Animator.StringToHash("BS076@DYNAMO_SKILL4_CASTING2");
		_animationHash[24] = Animator.StringToHash("BS076@DYNAMO_SKILL4_CASTLOOP2");
		_animationHash[25] = Animator.StringToHash("BS076@DYNAMO_SKILL4_CASTOUT1");
		_animationHash[26] = Animator.StringToHash("BS076@DYNAMO_SKILL5_CASTING1");
		_animationHash[27] = Animator.StringToHash("BS076@DYNAMO_SKILL5_CASTLOOP1");
		_animationHash[28] = Animator.StringToHash("BS076@DYNAMO_SKILL5_CASTING2");
		_animationHash[29] = Animator.StringToHash("BS076@DYNAMO_SKILL5_CASTLOOP2");
		_animationHash[30] = Animator.StringToHash("BS076@DYNAMO_SKILL5_CASTING3");
		_animationHash[31] = Animator.StringToHash("BS076@DYNAMO_SKILL5_CASTING4");
		_animationHash[32] = Animator.StringToHash("BS076@DYNAMO_SKILL3_CASTLOOP3");
		_animationHash[33] = Animator.StringToHash("BS076@DYNAMO_SKILL3_CASTOUT1");
		_animationHash[34] = Animator.StringToHash("BS076@hurt");
		_animationHash[35] = Animator.StringToHash("BS076@dead");
	}

	private void LoadParts(ref Transform[] childs)
	{
		ModelTransform = OrangeBattleUtility.FindChildRecursive(ref childs, "model", true);
		_animator = ModelTransform.GetComponent<Animator>();
		_collideBullet = OrangeBattleUtility.FindChildRecursive(ref childs, "Collider", true).gameObject.AddOrGetComponent<CollideBullet>();
		Skill4CollideBullet = OrangeBattleUtility.FindChildRecursive(ref childs, Skill4BulletObj, true).gameObject.AddOrGetComponent<CollideBullet>();
		if (SwordFX == null)
		{
			SwordFX = OrangeBattleUtility.FindChildRecursive(ref childs, "SwordFX", true).GetComponent<ParticleSystem>();
		}
		if (Skill1FX == null)
		{
			Skill1FX = OrangeBattleUtility.FindChildRecursive(ref childs, "Skill1FX", true).GetComponent<ParticleSystem>();
		}
		if (Skill3FX1 == null)
		{
			Skill3FX1 = OrangeBattleUtility.FindChildRecursive(ref childs, "Skill3FX1", true).GetComponent<ParticleSystem>();
		}
		if (Skill3FX2 == null)
		{
			Skill3FX2 = OrangeBattleUtility.FindChildRecursive(ref childs, "Skill3FX2", true).GetComponent<ParticleSystem>();
		}
		if (Skill4FX == null)
		{
			Skill4FX = OrangeBattleUtility.FindChildRecursive(ref childs, "Skill4Fx", true).GetComponent<ParticleSystem>();
		}
		if (HandMesh == null)
		{
			HandMesh = OrangeBattleUtility.FindChildRecursive(ref childs, "BS076_HandMesh_U", true).gameObject.AddOrGetComponent<SkinnedMeshRenderer>();
		}
		if (GunMesh == null)
		{
			GunMesh = OrangeBattleUtility.FindChildRecursive(ref childs, "BS076_BusterMesh_U", true).gameObject.AddOrGetComponent<SkinnedMeshRenderer>();
		}
		if (SwordMesh1 == null)
		{
			SwordMesh1 = OrangeBattleUtility.FindChildRecursive(ref childs, "Saber_008_G_main", true).gameObject.AddOrGetComponent<SkinnedMeshRenderer>();
		}
		if (SwordMesh2 == null)
		{
			SwordMesh2 = OrangeBattleUtility.FindChildRecursive(ref childs, "Saber_008_G_sub", true).gameObject.AddOrGetComponent<SkinnedMeshRenderer>();
		}
	}

	protected override void Awake()
	{
		base.Awake();
		Transform[] childs = _transform.GetComponentsInChildren<Transform>(true);
		LoadParts(ref childs);
		HashAnimation();
		base.AimPoint = new Vector3(0f, 1f, 0f);
		OrangeBattleUtility.AddEnemyAutoAimSystem(_transform, out _enemyAutoAimSystem);
		_enemyAutoAimSystem.UpdateAimRange(20f);
		AiTimer.TimerStart();
	}

	protected override void Start()
	{
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
		ModelTransform.localEulerAngles = new Vector3(0f, 90 - base.direction * 20, 0f);
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
			}
			break;
		case MainStatus.Idle:
			_velocity.x = 0;
			IdleWaitFrame = GameLogicUpdateManager.GameFrame + (int)(IdleWaitTime * 20f);
			break;
		case MainStatus.Skill0:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_velocity = VInt3.zero;
				break;
			case SubStatus.Phase1:
				ActionTimes = Skill0ShootTimes;
				SwitchFx(SwordFX, true);
				break;
			case SubStatus.Phase2:
				ActionAnimatorFrame = Skill0ShootFrame;
				HasActed = false;
				break;
			case SubStatus.Phase3:
				SwitchFx(SwordFX, false);
				break;
			}
			break;
		case MainStatus.Skill1:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_velocity = VInt3.zero;
				ActionTimes = Skill1ShootTimes;
				break;
			case SubStatus.Phase1:
				SwitchFx(Skill1FX, true);
				switch (ActionTimes)
				{
				case 3:
					EndPos = new Vector3(MinPos.x + 4f, MaxPos.y - 2.5f, 0f);
					break;
				case 2:
					EndPos = new Vector3(MaxPos.x - 4f, MaxPos.y - 2.5f, 0f);
					break;
				case 1:
					EndPos = new Vector3(CenterPos.x, CenterPos.y - 1.5f, 0f);
					break;
				}
				ActionFrame = GameLogicUpdateManager.GameFrame + (int)(Skill1ShootTime * 20f);
				BulletBase.TryShotBullet(EnemyWeapons[2].BulletData, ShootPos.position + Vector3.right * 0.4f * base.direction + Vector3.down * 0.1f, EndPos, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
				break;
			case SubStatus.Phase2:
				SwitchFx(Skill1FX, false);
				break;
			}
			break;
		case MainStatus.Skill2:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				PlayBossSE("BossSE05", "bs047_dynamo02");
				_velocity = VInt3.zero;
				if (NowPos.x > CenterPos.x)
				{
					UpdateDirection(-1);
				}
				else
				{
					UpdateDirection(1);
				}
				HasActed = false;
				ActionAnimatorFrame = Skill2ActFrame;
				break;
			case SubStatus.Phase2:
				_velocity = VInt3.zero;
				IgnoreGravity = true;
				ActionTimes = Skill2ShootTimes;
				SwitchMesh(HandMesh, false);
				SwitchMesh(GunMesh, true);
				break;
			case SubStatus.Phase3:
				HasActed = false;
				ActionAnimatorFrame = Skill2ShootFrame;
				break;
			case SubStatus.Phase4:
				if (base.direction == -1)
				{
					EndPos.x = MinPos.x + Skill2WallDis;
				}
				else
				{
					EndPos.x = MaxPos.x - Skill2WallDis;
				}
				_velocity.x = CalXSpeed(NowPos.x, EndPos.x, Skill2JumpTime);
				IgnoreGravity = false;
				SwitchMesh(HandMesh, true);
				SwitchMesh(GunMesh, false);
				UpdateDirection(-base.direction);
				break;
			case SubStatus.Phase6:
				_velocity = VInt3.zero;
				break;
			}
			break;
		case MainStatus.Skill3:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				PlayBossSE("BossSE05", "bs047_dynamo06");
				_velocity = VInt3.zero;
				SwitchFx(Skill3FX1, true);
				break;
			case SubStatus.Phase1:
				ActionFrame = GameLogicUpdateManager.GameFrame + (int)(Skill3ChargeTime * 20f);
				break;
			case SubStatus.Phase2:
				HasActed = false;
				ActionAnimatorFrame = Skill3ActFrame;
				break;
			case SubStatus.Phase3:
				ActionFrame = GameLogicUpdateManager.GameFrame + (int)(Skill3AtkTime * 20f);
				break;
			case SubStatus.Phase4:
				SwitchFx(Skill3FX2, false);
				break;
			}
			break;
		case MainStatus.Skill4:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				PlayBossSE("BossSE05", "bs047_dynamo02");
				EndPos = GetTargetPos();
				UpdateDirection();
				EndPos -= Vector3.right * base.direction * 2f;
				HasActed = false;
				ActionAnimatorFrame = Skill4ActFrame;
				break;
			case SubStatus.Phase2:
				SwitchMesh(SwordMesh1, true);
				SwitchMesh(SwordMesh2, true);
				break;
			case SubStatus.Phase4:
				Skill4CollideBullet.Active(targetMask);
				SwitchFx(Skill4FX, true);
				IgnoreGravity = true;
				_velocity = VInt3.zero;
				break;
			case SubStatus.Phase5:
				Skill4CollideBullet.BackToPool();
				SwitchMesh(SwordMesh1, false);
				SwitchMesh(SwordMesh2, false);
				SwitchFx(Skill4FX, false);
				IgnoreGravity = false;
				EndPos += Vector3.right * base.direction * 3f;
				HasActed = false;
				ActionAnimatorFrame = Skill4ActFrame;
				break;
			case SubStatus.Phase7:
				_velocity = VInt3.zero;
				break;
			}
			break;
		case MainStatus.Die:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				PlayBossSE("BossSE05", "bs047_dynamo09");
				base.AllowAutoAim = false;
				_velocity = VInt3.zero;
				base.DeadPlayCompleted = true;
				nDeadCount = 0;
				if (!Controller.Collisions.below)
				{
					SetStatus(MainStatus.Die, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase1:
				StartCoroutine(BossDieFlow(GetTargetPoint(), "FX_BOSS_EXPLODE2", false, false));
				break;
			case SubStatus.Phase2:
				IgnoreGravity = true;
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
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_DEBUT;
				break;
			case SubStatus.Phase2:
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
				_currentAnimationId = AnimationID.ANI_SKILL0_LOOP3;
				break;
			case SubStatus.Phase6:
				_currentAnimationId = AnimationID.ANI_SKILL0_END3;
				break;
			}
			break;
		case MainStatus.Skill1:
			switch (_subStatus)
			{
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
				_currentAnimationId = AnimationID.ANI_SKILL2_LOOP3;
				break;
			case SubStatus.Phase6:
				_currentAnimationId = AnimationID.ANI_SKILL2_END3;
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
				_currentAnimationId = AnimationID.ANI_SKILL4_START3;
				break;
			case SubStatus.Phase5:
				_currentAnimationId = AnimationID.ANI_SKILL4_START4;
				break;
			case SubStatus.Phase6:
				_currentAnimationId = AnimationID.ANI_SKILL4_LOOP4;
				break;
			case SubStatus.Phase7:
				_currentAnimationId = AnimationID.ANI_SKILL4_END4;
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
			case SubStatus.Phase2:
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
				if (_introReady)
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
				if (!HasActed && _currentFrame > ActionAnimatorFrame)
				{
					HasActed = true;
					SwitchFx(SwordFX, false);
					Skill0Bullet = BulletBase.TryShotBullet(EnemyWeapons[1].BulletData, ShootPos.position + Vector3.right * 0.4f * base.direction + Vector3.down * 0.1f, Vector3.right * base.direction, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
				}
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill0, SubStatus.Phase3);
				}
				break;
			case SubStatus.Phase3:
				if (HasActed && (Skill0Bullet.bIsEnd || Skill0Bullet == null))
				{
					Skill0Bullet = null;
					SetStatus(MainStatus.Skill0, SubStatus.Phase4);
				}
				break;
			case SubStatus.Phase4:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill0, SubStatus.Phase5);
				}
				break;
			case SubStatus.Phase5:
				SetStatus(MainStatus.Skill0, SubStatus.Phase6);
				break;
			case SubStatus.Phase6:
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
					SetStatus(MainStatus.Skill1, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if (GameLogicUpdateManager.GameFrame > ActionFrame)
				{
					if (--ActionTimes > 0)
					{
						SetStatus(MainStatus.Skill1, SubStatus.Phase1);
					}
					else
					{
						SetStatus(MainStatus.Skill1, SubStatus.Phase2);
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
		case MainStatus.Skill2:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (!HasActed && _currentFrame > ActionAnimatorFrame)
				{
					HasActed = true;
					int num3 = (int)(Skill2JumpTime * 20f);
					ActionFrame = GameLogicUpdateManager.GameFrame + num3;
					_velocity = new VInt3(CalXSpeed(NowPos.x, CenterPos.x, Skill2JumpTime), Math.Abs(Gravity * num3), 0);
				}
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill2, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if (GameLogicUpdateManager.GameFrame >= ActionFrame && _velocity.y <= 10)
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
				if (!HasActed && _currentFrame > ActionAnimatorFrame)
				{
					HasActed = true;
					(BulletBase.TryShotBullet(EnemyWeapons[3].BulletData, ShootPos, Vector3.down, null, selfBuffManager.sBuffStatus, EnemyData, targetMask) as BasicBullet).BackCallback = Skill2BulletBack;
				}
				if (_currentFrame > 1f)
				{
					if (--ActionTimes > 0)
					{
						SetStatus(MainStatus.Skill2, SubStatus.Phase3);
					}
					else
					{
						SetStatus(MainStatus.Skill2, SubStatus.Phase4);
					}
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
				if (GameLogicUpdateManager.GameFrame >= ActionFrame)
				{
					SetStatus(MainStatus.Skill3, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				if (!HasActed && _currentFrame > ActionAnimatorFrame)
				{
					HasActed = true;
					SwitchFx(Skill3FX1, false);
					SwitchFx(Skill3FX2, true);
					PlayBossSE("BossSE05", "bs047_dynamo07");
					for (int i = 0; i < Skill3Atknum; i++)
					{
						EndPos = NowPos + Vector3.right * (Skill3AtkSpace * (float)i + 1f);
						(BulletBase.TryShotBullet(EnemyWeapons[5].BulletData, EndPos, Vector3.zero, null, selfBuffManager.sBuffStatus, EnemyData, targetMask) as CollideBullet).bNeedBackPoolModelName = true;
						EndPos = NowPos - Vector3.right * (Skill3AtkSpace * (float)i + 1f);
						(BulletBase.TryShotBullet(EnemyWeapons[5].BulletData, EndPos, Vector3.zero, null, selfBuffManager.sBuffStatus, EnemyData, targetMask) as CollideBullet).bNeedBackPoolModelName = true;
					}
					Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.CAMERA_SHAKE, 0.5f, false);
				}
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill3, SubStatus.Phase3);
				}
				break;
			case SubStatus.Phase3:
				if (GameLogicUpdateManager.GameFrame >= ActionFrame)
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
				if (!HasActed && _currentFrame > ActionAnimatorFrame)
				{
					HasActed = true;
					int num = (int)(Skill4JumpTime * 20f);
					ActionFrame = GameLogicUpdateManager.GameFrame + (int)((float)num * 1.6f);
					_velocity = new VInt3(CalXSpeed(NowPos.x, EndPos.x, Skill4JumpTime, 1.6f), Math.Abs(Gravity * num), 0);
				}
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill4, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				SetStatus(MainStatus.Skill4, SubStatus.Phase2);
				break;
			case SubStatus.Phase2:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill4, SubStatus.Phase3);
				}
				break;
			case SubStatus.Phase3:
				if (GameLogicUpdateManager.GameFrame >= ActionFrame && _velocity.y <= 10)
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
				if (!HasActed && _currentFrame > ActionAnimatorFrame)
				{
					HasActed = true;
					int num2 = (int)(Skill4JumpTime * 20f);
					ActionFrame = GameLogicUpdateManager.GameFrame + num2;
					_velocity = new VInt3(CalXSpeed(NowPos.x, EndPos.x, Skill4JumpTime), Math.Abs(Gravity * num2), 0);
					UpdateDirection(-base.direction);
				}
				if (HasActed && GameLogicUpdateManager.GameFrame >= ActionFrame && _velocity.y <= 10)
				{
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
			case SubStatus.Phase2:
				if (_currentFrame > 3f)
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
			_collideBullet.UpdateBulletData(EnemyWeapons[0].BulletData);
			_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			_collideBullet.Active(targetMask);
			Skill4CollideBullet.UpdateBulletData(EnemyWeapons[6].BulletData);
			Skill4CollideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			SwitchMesh(SwordMesh1, false);
			SwitchMesh(SwordMesh2, false);
			SetMaxGravity(OrangeBattleUtility.FP_MaxGravity * 2);
			CheckRoomSize();
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
		if (_mainStatus != MainStatus.Die)
		{
			if ((bool)_collideBullet)
			{
				_collideBullet.BackToPool();
			}
			if ((bool)Skill4CollideBullet)
			{
				Skill4CollideBullet.BackToPool();
			}
			SwitchFx(SwordFX, false);
			SwitchFx(Skill1FX, false);
			SwitchFx(Skill3FX1, false);
			SwitchFx(Skill3FX2, false);
			SwitchFx(Skill4FX, false);
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

	private int CalXSpeed(float startx, float endx, float jumptime, float timeoffset = 1f)
	{
		int num = (int)((float)Math.Abs((int)(jumptime * 20f)) * timeoffset);
		return (int)((endx - startx) * 1000f * 20f / (float)num);
	}

	private void Skill2BulletBack(object obj)
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
			(BulletBase.TryShotBullet(EnemyWeapons[4].BulletData, basicBullet._transform.position, Vector3.right, null, selfBuffManager.sBuffStatus, EnemyData, targetMask) as BS045_ThunderBullet).SetEndPos(MaxPos.x - 1f);
			(BulletBase.TryShotBullet(EnemyWeapons[4].BulletData, basicBullet._transform.position, Vector3.left, null, selfBuffManager.sBuffStatus, EnemyData, targetMask) as BS045_ThunderBullet).SetEndPos(MinPos.x + 1f);
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

	public override void UpdateEnemyID(int id)
	{
		base.UpdateEnemyID(id);
		UpdateAIState();
		AI_STATE aiState = AiState;
	}

	private void CheckRoomSize()
	{
		int layerMask = (1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockLayer) | (1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockEnemyLayer) | (1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockPlayerLayer) | (1 << ManagedSingleton<OrangeLayerManager>.Instance.WallKickMask);
		Vector3 vector = new Vector3(_transform.position.x - 4f, _transform.position.y + 4f, 0f);
		RaycastHit2D raycastHit2D = OrangeBattleUtility.RaycastIgnoreSelf(vector, Vector3.left, 30f, layerMask, _transform);
		RaycastHit2D raycastHit2D2 = OrangeBattleUtility.RaycastIgnoreSelf(vector, Vector3.right, 30f, layerMask, _transform);
		RaycastHit2D raycastHit2D3 = OrangeBattleUtility.RaycastIgnoreSelf(vector, Vector3.up, 30f, layerMask, _transform);
		RaycastHit2D raycastHit2D4 = OrangeBattleUtility.RaycastIgnoreSelf(vector, Vector3.down, 30f, layerMask, _transform);
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
		if (!raycastHit2D4)
		{
			flag = true;
			Debug.LogError("沒有偵測到地板，之後一些技能無法準確判斷位置");
		}
		if (!raycastHit2D3)
		{
			flag = true;
			Debug.LogError("沒有偵測到天花板，之後一些技能無法準確判斷位置");
		}
		if (flag)
		{
			MaxPos = new Vector3(NowPos.x + 3f, NowPos.y + 6f, 0f);
			MinPos = new Vector3(NowPos.x - 6f, NowPos.y, 0f);
		}
		else
		{
			MaxPos = new Vector3(raycastHit2D2.point.x, raycastHit2D3.point.y, 0f);
			MinPos = new Vector3(raycastHit2D.point.x, raycastHit2D4.point.y, 0f);
			CenterPos = (MaxPos + MinPos) / 2f;
		}
	}

	protected override void UpdateGravity()
	{
		if (IgnoreGravity_bak != IgnoreGravity)
		{
			if (IgnoreGravity)
			{
				_velocity.y = 0;
			}
			IgnoreGravity_bak = IgnoreGravity;
		}
		if (!IgnoreGravity)
		{
			if ((_velocity.y < 0 && Controller.Collisions.below) || (_velocity.y > 0 && Controller.Collisions.above))
			{
				_velocity.y = 0;
			}
			MainStatus mainStatus = _mainStatus;
			if (mainStatus == MainStatus.Skill4)
			{
				_velocity.y += (int)(Skill4GravityMulti * (float)(OrangeBattleUtility.FP_Gravity * GameLogicUpdateManager.g_fixFrameLenFP) / 1000f);
			}
			else
			{
				_velocity.y += OrangeBattleUtility.FP_Gravity * GameLogicUpdateManager.g_fixFrameLenFP / 1000;
			}
			_velocity.y = IntMath.Sign(_velocity.y) * IntMath.Min(IntMath.Abs(_velocity.y), IntMath.Abs(_maxGravity.i));
		}
	}
}
