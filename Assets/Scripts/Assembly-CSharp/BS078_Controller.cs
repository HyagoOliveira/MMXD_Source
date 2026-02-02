#define RELEASE
using System;
using System.Collections.Generic;
using CallbackDefs;
using CodeStage.AntiCheat.ObscuredTypes;
using NaughtyAttributes;
using Newtonsoft.Json;
using StageLib;
using UnityEngine;

public class BS078_Controller : EnemyControllerBase, IManagedUpdateBehavior
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
		Phase9 = 9,
		Phase10 = 10,
		Phase11 = 11,
		Phase12 = 12,
		Phase13 = 13,
		Phase14 = 14,
		Phase15 = 15,
		MAX_SUBSTATUS = 16
	}

	private enum SkillPattern
	{
		State1 = 1,
		State2 = 2,
		State3 = 3,
		State4 = 4,
		State5 = 5
	}

	public enum AnimationID
	{
		ANI_IDLE = 0,
		ANI_DEBUT = 1,
		ANI_WALK = 2,
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
		ANI_SKILL3_END2 = 18,
		ANI_SKILL4_START1 = 19,
		ANI_SKILL4_LOOP1 = 20,
		ANI_SKILL4_START2 = 21,
		ANI_SKILL4_LOOP2 = 22,
		ANI_SKILL4_START3 = 23,
		ANI_SKILL4_LOOP3 = 24,
		ANI_SKILL4_START4 = 25,
		ANI_SKILL4_LOOP4 = 26,
		ANI_SKILL4_END4 = 27,
		ANI_SKILL4_START5 = 28,
		ANI_SKILL4_LOOP5 = 29,
		ANI_SKILL4_START6 = 30,
		ANI_SKILL4_LOOP6 = 31,
		ANI_SKILL4_END6 = 32,
		ANI_HURT = 33,
		ANI_DEAD = 34,
		MAX_ANIMATION_ID = 35
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

	private bool hasCheckRoom;

	[SerializeField]
	private float IdleWaitTime = 0.2f;

	private int IdleWaitFrame;

	[Header("電擊彈")]
	[SerializeField]
	private int Skill0ShootTimes = 3;

	[SerializeField]
	private float Skill0ShootFrame = 0.4f;

	[SerializeField]
	private float Skill0Angle = 25f;

	[Header("放機雷")]
	[SerializeField]
	private float Skill1ShootFrame = 0.4f;

	[SerializeField]
	private static int Skill1ShootTimes = 1;

	private Vector3[] Skill1BulletPoint = new Vector3[8];

	private Dictionary<int, LocateBullet> Skill1Bullet = new Dictionary<int, LocateBullet>();

	[Header("收機雷")]
	[SerializeField]
	private float Skill2ShootFrame = 0.4f;

	[SerializeField]
	private static int Skill2ShootTimes = 1;

	private List<EnergyBullet> Skill2Bullet = new List<EnergyBullet>();

	[Header("飛跳")]
	[SerializeField]
	private float Skill3ActFrame1 = 0.45f;

	[SerializeField]
	private float Skill3JumpTime1 = 0.75f;

	[SerializeField]
	private float Skill3ActFrame2 = 0.45f;

	[SerializeField]
	private float Skill3JumpTime2 = 0.25f;

	[SerializeField]
	private float Skill3GravityMulti = 0.8f;

	[Header("電擊衝撞")]
	[SerializeField]
	private float Skill4ChargeTime = 1f;

	[SerializeField]
	private int Skill4DashSpeed = 12500;

	[SerializeField]
	private ParticleSystem Skill4UseFX1;

	[SerializeField]
	private ParticleSystem Skill4UseFX2;

	[SerializeField]
	private ParticleSystem Skill4UseFX3;

	[SerializeField]
	private ParticleSystem[] Skill4ShieldFX = new ParticleSystem[2];

	[SerializeField]
	private Transform[] Skill4Shield = new Transform[2];

	private CollideBullet[] Skill4Collide = new CollideBullet[2];

	[SerializeField]
	private int MaxShieldHp = 5;

	[SerializeField]
	private float Skill4ShootInterval = 0.2f;

	private int Skill4ShootFrame;

	private int[] Skill4ShieldHp = new int[2];

	private int Skill4ShootAngle;

	[Header("AI控制")]
	[SerializeField]
	[NaughtyAttributes.ReadOnly]
	private SkillPattern skillpattern = SkillPattern.State1;

	[SerializeField]
	private bool isEXMode;

	[SerializeField]
	private float AIXDis = 5f;

	[SerializeField]
	private bool DebugMode;

	[SerializeField]
	private MainStatus NextSkill;

	private bool bPlayLP;

	private bool bShieldLP;

	private bool bSkill3End;

	private Vector3 NowPos
	{
		get
		{
			return _transform.position;
		}
	}

	private int Gravity
	{
		get
		{
			MainStatus mainStatus = _mainStatus;
			if (mainStatus == MainStatus.Skill3)
			{
				return (int)(Skill3GravityMulti * (float)(OrangeBattleUtility.FP_Gravity * GameLogicUpdateManager.g_fixFrameLenFP) / 1000f);
			}
			return OrangeBattleUtility.FP_Gravity * GameLogicUpdateManager.g_fixFrameLenFP / 1000;
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
		_animationHash = new int[35];
		_animationHash[0] = Animator.StringToHash("BS078@idl_loop");
		_animationHash[1] = Animator.StringToHash("BS078@debut");
		_animationHash[2] = Animator.StringToHash("BS078@walk_loop");
		_animationHash[3] = Animator.StringToHash("BS078@VOLT_CATFISH_SKILL1_CASTING1");
		_animationHash[4] = Animator.StringToHash("BS078@VOLT_CATFISH_SKILL1_CASTLOOP1");
		_animationHash[5] = Animator.StringToHash("BS078@VOLT_CATFISH_SKILL1_CASTOUT1");
		_animationHash[6] = Animator.StringToHash("BS078@VOLT_CATFISH_SKILL2_CASTING1");
		_animationHash[7] = Animator.StringToHash("BS078@VOLT_CATFISH_SKILL2_CASTLOOP1");
		_animationHash[8] = Animator.StringToHash("BS078@VOLT_CATFISH_SKILL2_CASTOUT1");
		_animationHash[9] = Animator.StringToHash("BS078@VOLT_CATFISH_SKILL3_CASTING1");
		_animationHash[10] = Animator.StringToHash("BS078@VOLT_CATFISH_SKILL3_CASTLOOP1");
		_animationHash[11] = Animator.StringToHash("BS078@VOLT_CATFISH_SKILL3_CASTING2");
		_animationHash[12] = Animator.StringToHash("BS078@VOLT_CATFISH_SKILL3_CASTLOOP2");
		_animationHash[13] = Animator.StringToHash("BS078@VOLT_CATFISH_SKILL3_CASTOUT1");
		_animationHash[14] = Animator.StringToHash("BS078@VOLT_CATFISH_SKILL4_CASTING1");
		_animationHash[15] = Animator.StringToHash("BS078@VOLT_CATFISH_SKILL4_CASTLOOP1");
		_animationHash[16] = Animator.StringToHash("BS078@VOLT_CATFISH_SKILL1_CASTING2");
		_animationHash[17] = Animator.StringToHash("BS078@VOLT_CATFISH_SKILL4_CASTLOOP2");
		_animationHash[18] = Animator.StringToHash("BS078@VOLT_CATFISH_SKILL4_CASTOUT1");
		_animationHash[19] = Animator.StringToHash("BS078@VOLT_CATFISH_SKILL5_CASTING1");
		_animationHash[20] = Animator.StringToHash("BS078@VOLT_CATFISH_SKILL5_CASTLOOP1");
		_animationHash[21] = Animator.StringToHash("BS078@VOLT_CATFISH_SKILL5_CASTING2");
		_animationHash[22] = Animator.StringToHash("BS078@VOLT_CATFISH_SKILL5_CASTLOOP2");
		_animationHash[23] = Animator.StringToHash("BS078@VOLT_CATFISH_SKILL5_CASTING3");
		_animationHash[24] = Animator.StringToHash("BS078@VOLT_CATFISH_SKILL5_CASTLOOP3");
		_animationHash[25] = Animator.StringToHash("BS078@VOLT_CATFISH_SKILL5_CASTING4");
		_animationHash[26] = Animator.StringToHash("BS078@VOLT_CATFISH_SKILL5_CASTLOOP4");
		_animationHash[27] = Animator.StringToHash("BS078@VOLT_CATFISH_SKILL5_CASTOUT1");
		_animationHash[28] = Animator.StringToHash("BS078@VOLT_CATFISH_SKILL5_CASTING5");
		_animationHash[29] = Animator.StringToHash("BS078@VOLT_CATFISH_SKILL5_CASTLOOP5");
		_animationHash[30] = Animator.StringToHash("BS078@VOLT_CATFISH_SKILL5_CASTING6");
		_animationHash[31] = Animator.StringToHash("BS078VOLT_CATFISH_SKILL5_CASTLOOP6");
		_animationHash[32] = Animator.StringToHash("BS078VOLT_CATFISH_SKILL5_CASTOUT2");
		_animationHash[33] = Animator.StringToHash("BS078@hurt");
		_animationHash[34] = Animator.StringToHash("BS078@dead");
	}

	private void LoadParts(ref Transform[] childs)
	{
		ModelTransform = OrangeBattleUtility.FindChildRecursive(ref childs, "model", true);
		_animator = ModelTransform.GetComponent<Animator>();
		_collideBullet = OrangeBattleUtility.FindChildRecursive(ref childs, "Collider", true).gameObject.AddOrGetComponent<CollideBullet>();
		for (int i = 0; i < Skill4Shield.Length; i++)
		{
			if (!Skill4Shield[i])
			{
				Skill4Shield[i] = OrangeBattleUtility.FindChildRecursive(ref childs, "ShieldCollider" + (i + 1), true);
			}
			if ((bool)Skill4Shield[i])
			{
				Skill4Shield[i].gameObject.AddOrGetComponent<StageObjParam>().nSubPartID = i + 1;
				GuardTransform.Add(i + 1);
			}
		}
		if (ShootPos == null)
		{
			ShootPos = OrangeBattleUtility.FindChildRecursive(ref childs, "ShootPoint_Head", true);
		}
		if (Skill4Shield[0] == null)
		{
			Skill4Shield[0] = OrangeBattleUtility.FindChildRecursive(ref childs, "ShieldR", true);
		}
		if (Skill4Shield[1] == null)
		{
			Skill4Shield[1] = OrangeBattleUtility.FindChildRecursive(ref childs, "ShieldL", true);
		}
		Skill4Collide[0] = OrangeBattleUtility.FindChildRecursive(ref childs, "ShieldCollider1", true).gameObject.AddOrGetComponent<CollideBullet>();
		Skill4Collide[1] = OrangeBattleUtility.FindChildRecursive(ref childs, "ShieldCollider2", true).gameObject.AddOrGetComponent<CollideBullet>();
		if (Skill4UseFX1 == null)
		{
			Skill4UseFX1 = OrangeBattleUtility.FindChildRecursive(ref childs, "Skill4UseFX1", true).GetComponent<ParticleSystem>();
		}
		if (Skill4UseFX2 == null)
		{
			Skill4UseFX2 = OrangeBattleUtility.FindChildRecursive(ref childs, "Skill4UseFX2", true).GetComponent<ParticleSystem>();
		}
		if (Skill4UseFX3 == null)
		{
			Skill4UseFX3 = OrangeBattleUtility.FindChildRecursive(ref childs, "Skill4UseFX3", true).GetComponent<ParticleSystem>();
		}
		if (Skill4ShieldFX[0] == null)
		{
			Skill4ShieldFX[0] = OrangeBattleUtility.FindChildRecursive(ref childs, "Skill4ShieldFX1", true).GetComponent<ParticleSystem>();
		}
		if (Skill4ShieldFX[1] == null)
		{
			Skill4ShieldFX[1] = OrangeBattleUtility.FindChildRecursive(ref childs, "Skill4ShieldFX2", true).GetComponent<ParticleSystem>();
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
			if (netSyncData.nParam0 != 0)
			{
				skillpattern = (SkillPattern)netSyncData.nParam0;
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
			}
			break;
		case MainStatus.Idle:
			_velocity.x = 0;
			IdleWaitFrame = GameLogicUpdateManager.GameFrame + (int)(IdleWaitTime * 20f);
			if (!hasCheckRoom)
			{
				CheckRoomSize();
			}
			break;
		case MainStatus.Skill0:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_velocity = VInt3.zero;
				break;
			case SubStatus.Phase1:
				ActionAnimatorFrame = Skill0ShootFrame;
				HasActed = false;
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
				ActionAnimatorFrame = Skill1ShootFrame;
				HasActed = false;
				break;
			}
			break;
		case MainStatus.Skill2:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				ActionAnimatorFrame = Skill2ShootFrame;
				HasActed = false;
				_velocity = VInt3.zero;
				break;
			case SubStatus.Phase1:
				HasActed = false;
				break;
			case SubStatus.Phase2:
				PlayBossSE("BossSE05", "bs046_namazu05");
				BulletBase.TryShotBullet(EnemyWeapons[10].BulletData, NowPos - Vector3.right * 0.4f + Vector3.up * 1.5f, Vector3.up, null, selfBuffManager.sBuffStatus, EnemyData, targetMask).BackCallback = Skill2Thunder;
				BulletBase.TryShotBullet(EnemyWeapons[10].BulletData, NowPos + Vector3.right * 0.4f + Vector3.up * 1.5f, Vector3.up, null, selfBuffManager.sBuffStatus, EnemyData, targetMask).BackCallback = Skill2Thunder;
				break;
			}
			break;
		case MainStatus.Skill3:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				bSkill3End = false;
				EndPos = TargetPos.vec3;
				UpdateDirection();
				EndPos -= Vector3.right * base.direction * 1f;
				if ((EndPos.x - NowPos.x) * (float)base.direction < 0f)
				{
					EndPos = NowPos + Vector3.right * 3f * base.direction;
				}
				HasActed = false;
				ActionAnimatorFrame = Skill3ActFrame1;
				break;
			case SubStatus.Phase2:
				_collideBullet.UpdateBulletData(EnemyWeapons[9].BulletData);
				_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
				_collideBullet.Active(targetMask);
				break;
			case SubStatus.Phase4:
				_velocity = VInt3.zero;
				EndPos += Vector3.right * base.direction * 1.5f;
				HasActed = false;
				ActionAnimatorFrame = Skill3ActFrame2;
				_collideBullet.UpdateBulletData(EnemyWeapons[0].BulletData);
				_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
				_collideBullet.Active(targetMask);
				break;
			}
			break;
		case MainStatus.Skill4:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				SwitchFx(Skill4UseFX1, true);
				break;
			case SubStatus.Phase1:
				Skill4ShootAngle = (int)Hp % 360;
				SwitchFx(Skill4UseFX2, true);
				SwitchFx(Skill4UseFX3, true);
				Skill4ShootFrame = GameLogicUpdateManager.GameFrame + (int)(Skill4ShootInterval * 20f);
				_collideBullet.UpdateBulletData(EnemyWeapons[6].BulletData);
				_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
				_collideBullet.Active(targetMask);
				break;
			case SubStatus.Phase3:
				SwitchShield(true);
				break;
			case SubStatus.Phase5:
				IgnoreGravity = true;
				ActionFrame = GameLogicUpdateManager.GameFrame + (int)(Skill4ChargeTime * 20f);
				break;
			case SubStatus.Phase6:
				EndPos = GetTargetPos();
				UpdateDirection();
				_velocity = new VInt3(Skill4DashSpeed * base.direction, 0, 0);
				break;
			case SubStatus.Phase7:
				IgnoreGravity = false;
				break;
			case SubStatus.Phase8:
				SwitchShield(false);
				_velocity = VInt3.zero;
				_collideBullet.UpdateBulletData(EnemyWeapons[0].BulletData);
				_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
				_collideBullet.Active(targetMask);
				break;
			case SubStatus.Phase9:
				EndPos = GetTargetPos();
				UpdateDirection();
				_velocity = new VInt3((EndPos - NowPos).normalized) * Skill4DashSpeed * 0.001f;
				break;
			case SubStatus.Phase11:
				IgnoreGravity = false;
				_velocity = VInt3.zero;
				SwitchShield(false);
				break;
			case SubStatus.Phase13:
				_velocity = VInt3.zero;
				_collideBullet.UpdateBulletData(EnemyWeapons[0].BulletData);
				_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
				_collideBullet.Active(targetMask);
				break;
			}
			break;
		case MainStatus.Die:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (bPlayLP)
				{
					PlayBossSE("BossSE05", "bs046_namazu11_stop");
					bPlayLP = false;
				}
				if (bShieldLP)
				{
					PlayBossSE("BossSE05", "bs046_namazu12_stop");
					bShieldLP = false;
				}
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
				StartCoroutine(BossDieFlow(_transform.position));
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
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_DEBUT;
				break;
			case SubStatus.Phase1:
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
				_currentAnimationId = AnimationID.ANI_SKILL2_END2;
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
				_currentAnimationId = AnimationID.ANI_SKILL4_LOOP3;
				break;
			case SubStatus.Phase6:
				_currentAnimationId = AnimationID.ANI_SKILL4_START4;
				break;
			case SubStatus.Phase7:
				_currentAnimationId = AnimationID.ANI_SKILL4_LOOP4;
				break;
			case SubStatus.Phase8:
				_currentAnimationId = AnimationID.ANI_SKILL4_END4;
				break;
			case SubStatus.Phase9:
				_currentAnimationId = AnimationID.ANI_SKILL4_START4;
				break;
			case SubStatus.Phase10:
				_currentAnimationId = AnimationID.ANI_SKILL4_LOOP4;
				break;
			case SubStatus.Phase11:
				_currentAnimationId = AnimationID.ANI_SKILL4_START6;
				break;
			case SubStatus.Phase12:
				_currentAnimationId = AnimationID.ANI_SKILL4_LOOP6;
				break;
			case SubStatus.Phase13:
				_currentAnimationId = AnimationID.ANI_SKILL4_END6;
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
				mainStatus = (MainStatus)RandomCard(2);
				switch (skillpattern)
				{
				case SkillPattern.State1:
					mainStatus = MainStatus.Skill3;
					skillpattern = SkillPattern.State2;
					break;
				case SkillPattern.State2:
					EndPos = GetTargetPos();
					mainStatus = ((Math.Abs(EndPos.x - NowPos.x) > AIXDis) ? MainStatus.Skill0 : MainStatus.Skill3);
					skillpattern = SkillPattern.State3;
					break;
				case SkillPattern.State3:
					mainStatus = ((Skill1Bullet.Count == 0) ? MainStatus.Skill1 : MainStatus.Skill2);
					skillpattern = ((!isEXMode) ? SkillPattern.State1 : SkillPattern.State4);
					break;
				case SkillPattern.State4:
					mainStatus = MainStatus.Skill4;
					skillpattern = SkillPattern.State1;
					break;
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
			UploadEnemyStatus((int)mainStatus, false, new object[1] { (int)skillpattern });
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
				if (_currentFrame > 1f)
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
				Target = _enemyAutoAimSystem.GetClosetPlayer();
				if ((bool)Target)
				{
					UpdateDirection();
					UpdateNextState();
				}
				else
				{
					UpdateNextState(MainStatus.Skill2);
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
				if (!HasActed && _currentFrame > ActionAnimatorFrame)
				{
					HasActed = true;
					for (int k = 0; k < Skill0ShootTimes; k++)
					{
						(BulletBase.TryShotBullet(EnemyWeapons[1].BulletData, ShootPos.position + new Vector3(0.65f * (float)base.direction, -0.8f, 0f), Quaternion.Euler(0f, 0f, Skill0Angle * (float)base.direction * (float)k) * Vector3.right * base.direction, null, selfBuffManager.sBuffStatus, EnemyData, targetMask) as BS045_ThunderBullet).SetEndPos(MaxPos.x - 1f);
					}
				}
				if (_currentFrame > 1f)
				{
					if (--ActionTimes > 0)
					{
						SetStatus(MainStatus.Skill0, SubStatus.Phase1);
					}
					else
					{
						SetStatus(MainStatus.Skill0, SubStatus.Phase2);
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
				if (!HasActed && _currentFrame > ActionAnimatorFrame)
				{
					HasActed = true;
					PlayBossSE("BossSE05", "bs046_namazu06");
					for (int j = 0; j < Skill1BulletPoint.Length; j++)
					{
						LocateBullet value2;
						if (!Skill1Bullet.TryGetValue(j, out value2) || !(value2 != null))
						{
							EndPos = Skill1BulletPoint[j];
							Vector3 pDirection2 = EndPos - ShootPos.position;
							value2 = BulletBase.TryShotBullet(EnemyWeapons[2].BulletData, ShootPos.position, pDirection2, null, selfBuffManager.sBuffStatus, EnemyData, targetMask) as LocateBullet;
							if ((bool)value2)
							{
								value2.FreeDISTANCE = 0.1f;
								value2.SetEndPos(EndPos);
								Skill1Bullet.Add(j, value2);
							}
						}
					}
					if (Skill1Bullet.Count < Skill1BulletPoint.Length)
					{
						Debug.LogError("機雷數量少於 " + Skill1BulletPoint.Length + " 個屬於不正常現象，請回報BUG");
					}
				}
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill1, SubStatus.Phase2);
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
					if (Skill1Bullet.Count > 0)
					{
						PlayBossSE("BossSE05", "bs046_namazu07");
					}
					HasActed = true;
					for (int i = 0; i < Skill1Bullet.Count; i++)
					{
						LocateBullet value = null;
						Skill1Bullet.TryGetValue(i, out value);
						if ((bool)value)
						{
							EndPos = value._transform.position;
							Vector3 pDirection = EndPos - ShootPos.position;
							BS082_SummonBullet obj = BulletBase.TryShotBullet(EnemyWeapons[3].BulletData, ShootPos.position, pDirection, null, selfBuffManager.sBuffStatus, EnemyData, targetMask) as BS082_SummonBullet;
							obj.FreeDISTANCE = Vector2.Distance(ShootPos.position, EndPos);
							obj.SummonID = i;
							obj.BackCallback = Skill1BulletCharge;
						}
						else
						{
							Debug.LogError("找不到子彈ID " + i);
						}
					}
				}
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill2, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
			{
				bool flag = false;
				if (Skill1Bullet.Count <= 0)
				{
					flag = true;
				}
				foreach (KeyValuePair<int, LocateBullet> item in Skill1Bullet)
				{
					if (!item.Value.bIsEnd)
					{
						flag = false;
						break;
					}
					flag = true;
				}
				if (!flag)
				{
					break;
				}
				Skill1Bullet.Clear();
				if (!HasActed)
				{
					HasActed = true;
					foreach (EnergyBullet item2 in Skill2Bullet)
					{
						item2.StartShoot();
					}
				}
				flag = false;
				if (Skill2Bullet.Count <= 0)
				{
					flag = true;
				}
				foreach (EnergyBullet item3 in Skill2Bullet)
				{
					if (!item3.bIsEnd)
					{
						flag = false;
						break;
					}
					flag = true;
				}
				if (flag)
				{
					Skill2Bullet.Clear();
					if (flag)
					{
						SetStatus(MainStatus.Skill2, SubStatus.Phase2);
					}
				}
				break;
			}
			case SubStatus.Phase2:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill2, SubStatus.Phase3);
				}
				break;
			case SubStatus.Phase3:
				if (_currentFrame > 1f)
				{
					if (--ActionTimes > 0)
					{
						SetStatus(MainStatus.Skill2, SubStatus.Phase2);
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
					SetStatus(MainStatus.Idle);
				}
				break;
			}
			break;
		case MainStatus.Skill3:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (!HasActed && _currentFrame > ActionAnimatorFrame)
				{
					PlayBossSE("BossSE05", "bs046_namazu03");
					HasActed = true;
					int num2 = (int)(Skill3JumpTime1 * 20f);
					ActionFrame = GameLogicUpdateManager.GameFrame + (int)((double)num2 * 1.0);
					_velocity = new VInt3(CalXSpeed(NowPos.x, EndPos.x, Skill3JumpTime1, 1.8f), Math.Abs(Gravity * num2), 0);
				}
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill3, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if (GameLogicUpdateManager.GameFrame >= ActionFrame || _velocity.y <= 2000)
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
				if (Controller.Collisions.below)
				{
					PlayBossSE("BossSE05", "bs046_namazu02");
					SetStatus(MainStatus.Skill3, SubStatus.Phase4);
				}
				break;
			case SubStatus.Phase4:
				if (HasActed && Controller.Collisions.below && !bSkill3End)
				{
					bSkill3End = true;
					PlayBossSE("BossSE05", "bs046_namazu01");
					_velocity = VInt3.zero;
				}
				if (!HasActed && _currentFrame > ActionAnimatorFrame)
				{
					HasActed = true;
					int num = (int)(Skill3JumpTime2 * 20f);
					ActionFrame = GameLogicUpdateManager.GameFrame + (int)((double)num * 1.0);
					_velocity = new VInt3(CalXSpeed(NowPos.x, EndPos.x, Skill3JumpTime2), Math.Abs(Gravity * num), 0);
				}
				if (_currentFrame > 1f)
				{
					_velocity = VInt3.zero;
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
					if (!bPlayLP)
					{
						PlayBossSE("BossSE05", "bs046_namazu11_lp");
						bPlayLP = true;
					}
				}
				break;
			case SubStatus.Phase1:
				if (GameLogicUpdateManager.GameFrame >= Skill4ShootFrame)
				{
					Skill4ShootAngle += 130;
					Skill4ShootAngle %= 360;
					Skill4ShootFrame = GameLogicUpdateManager.GameFrame + (int)(Skill4ShootInterval * 20f);
					EndPos = ShootPos.position + Vector3.up * 3.6f + Quaternion.Euler(0f, 0f, Skill4ShootAngle) * (Vector3.up * 4f);
					BulletBase.TryShotBullet(EnemyWeapons[8].BulletData, ShootPos.position, EndPos, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
				}
				if (_currentFrame > 10f)
				{
					if (bPlayLP)
					{
						PlayBossSE("BossSE05", "bs046_namazu11_stop");
						bPlayLP = false;
					}
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
				if (GameLogicUpdateManager.GameFrame >= ActionFrame)
				{
					EndPos = GetTargetPos();
					if (EndPos.y < NowPos.y + 3f)
					{
						SetStatus(MainStatus.Skill4, SubStatus.Phase6);
					}
					else
					{
						SetStatus(MainStatus.Skill4, SubStatus.Phase9);
					}
				}
				break;
			case SubStatus.Phase6:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill4, SubStatus.Phase7);
				}
				break;
			case SubStatus.Phase7:
				if (Controller.Collisions.right || Controller.Collisions.left)
				{
					PlayBossSE("BossSE05", "bs046_namazu04");
					Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.CAMERA_SHAKE, 1f, false);
					SetStatus(MainStatus.Skill4, SubStatus.Phase8);
				}
				break;
			case SubStatus.Phase8:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Idle);
				}
				break;
			case SubStatus.Phase9:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill4, SubStatus.Phase10);
				}
				break;
			case SubStatus.Phase10:
				if (Controller.Collisions.right || Controller.Collisions.left || Controller.Collisions.above)
				{
					PlayBossSE("BossSE05", "bs046_namazu04");
					Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.CAMERA_SHAKE, 1f, false);
					SetStatus(MainStatus.Skill4, SubStatus.Phase11);
				}
				break;
			case SubStatus.Phase11:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill4, SubStatus.Phase12);
				}
				break;
			case SubStatus.Phase12:
				if (Controller.Collisions.below)
				{
					SetStatus(MainStatus.Skill4, SubStatus.Phase13);
				}
				break;
			case SubStatus.Phase13:
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
					SetStatus(MainStatus.Die, SubStatus.Phase3);
				}
				break;
			case SubStatus.Phase2:
				if (_currentFrame > 4f)
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
			Skill4Collide[0].UpdateBulletData(EnemyWeapons[7].BulletData);
			Skill4Collide[0].SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			Skill4Collide[1].UpdateBulletData(EnemyWeapons[7].BulletData);
			Skill4Collide[1].SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			SwitchShield(false);
			SetMaxGravity(OrangeBattleUtility.FP_MaxGravity * 2);
			if ((bool)_enemyCollider[1])
			{
				_enemyCollider[1].SetColliderEnable(false);
			}
			if ((bool)_enemyCollider[2])
			{
				_enemyCollider[2].SetColliderEnable(false);
			}
			hasCheckRoom = false;
			isEXMode = false;
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
		if (_mainStatus == MainStatus.Die)
		{
			return;
		}
		if ((bool)_collideBullet)
		{
			_collideBullet.BackToPool();
		}
		SwitchFx(Skill4UseFX1, false);
		SwitchFx(Skill4UseFX2, false);
		SwitchFx(Skill4UseFX3, false);
		SwitchFx(Skill4ShieldFX[0], false);
		SwitchFx(Skill4ShieldFX[1], false);
		SwitchShield(false);
		foreach (KeyValuePair<int, LocateBullet> item in Skill1Bullet)
		{
			if (!item.Value.bIsEnd)
			{
				item.Value.SetBackToPool();
			}
		}
		foreach (EnergyBullet item2 in Skill2Bullet)
		{
			if (!item2.bIsEnd)
			{
				item2.SetBackToPool();
			}
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

	private int CalXSpeed(float startx, float endx, float jumptime, float timeoffset = 1f)
	{
		int num = (int)((float)Math.Abs((int)(jumptime * 20f)) * timeoffset);
		return (int)((endx - startx) * 1000f * 20f / (float)num);
	}

	private void CheckShieldAllStop()
	{
		if (bShieldLP && Skill4ShieldHp[0] < 1 && Skill4ShieldHp[1] < 1)
		{
			PlayBossSE("BossSE05", "bs046_namazu12_stop");
			bShieldLP = false;
		}
	}

	public override ObscuredInt Hurt(HurtPassParam tHurtPassParam)
	{
		if (GuardTransform.Contains(tHurtPassParam.nSubPartID))
		{
			int num = tHurtPassParam.nSubPartID - 1;
			Skill4ShieldHp[num]--;
			if (Skill4ShieldHp[num] < 1)
			{
				Skill4Shield[num].gameObject.SetActive(false);
			}
			CheckShieldAllStop();
			return Hp;
		}
		tHurtPassParam.dmg = selfBuffManager.ReduceDmgByEnergyShild(tHurtPassParam.dmg);
		OrangeBattleUtility.UpdateEnemyHp(ref Hp, ref tHurtPassParam.dmg);
		if (!InGame)
		{
			Debug.LogWarning("[Enemy] InGame Flag is false.");
			return Hp;
		}
		UpdateHurtAction();
		if ((int)Hp > 0)
		{
			base.IsHidden = false;
			if ((bool)_characterMaterial)
			{
				_characterMaterial.Hurt();
			}
		}
		else
		{
			base.IsHidden = true;
			DeadBehavior(ref tHurtPassParam);
			MonoBehaviourSingleton<LegionManager>.Instance.callVibrator();
		}
		if ((int)Hp <= (int)MaxHp / 2)
		{
			isEXMode = true;
		}
		return Hp;
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
			return;
		}
		MaxPos = new Vector3(raycastHit2D2.point.x, raycastHit2D3.point.y, 0f);
		MinPos = new Vector3(raycastHit2D.point.x, raycastHit2D4.point.y, 0f);
		CenterPos = (MaxPos + MinPos) / 2f;
		Skill1BulletPoint[0] = new Vector3(MinPos.x + 1.5f, MaxPos.y - 1.5f);
		Skill1BulletPoint[1] = new Vector3(MinPos.x + 1.5f, MinPos.y + 1.5f);
		Skill1BulletPoint[2] = new Vector3(CenterPos.x, MaxPos.y - 1.5f);
		Skill1BulletPoint[3] = new Vector3(CenterPos.x, MinPos.y + 1.5f);
		Skill1BulletPoint[4] = new Vector3(MaxPos.x - 1.5f, MaxPos.y - 1.5f);
		Skill1BulletPoint[5] = new Vector3(MaxPos.x - 1.5f, MinPos.y + 1.5f);
		Skill1BulletPoint[6] = new Vector3(MinPos.x + 1.5f, CenterPos.y);
		Skill1BulletPoint[7] = new Vector3(MaxPos.x - 1.5f, CenterPos.y);
		hasCheckRoom = true;
	}

	private void Skill1BulletCharge(object obj)
	{
		BS082_SummonBullet bS082_SummonBullet = null;
		if (obj != null)
		{
			bS082_SummonBullet = obj as BS082_SummonBullet;
		}
		LocateBullet value;
		if (bS082_SummonBullet == null)
		{
			Debug.LogError("子彈資料有誤。");
		}
		else if (Skill1Bullet.TryGetValue(bS082_SummonBullet.SummonID, out value))
		{
			Vector3 normalized = (ShootPos.position - value._transform.position).normalized;
			EnergyBullet energyBullet = BulletBase.TryShotBullet(EnemyWeapons[4].BulletData, value._transform.position, normalized, null, selfBuffManager.sBuffStatus, EnemyData, targetMask) as EnergyBullet;
			energyBullet.FreeDISTANCE = Vector2.Distance(ShootPos.position, value._transform.position);
			energyBullet.BackCallback = Skill1BulletBack;
			Skill2Bullet.Add(energyBullet);
			value.SetBackToPool();
		}
		else
		{
			Debug.LogError("找不到對應的機雷，ID " + bS082_SummonBullet.SummonID + " 請回報BUG");
		}
	}

	private void Skill1BulletBack(object obj)
	{
		PlayBossSE("BossSE05", "bs046_namazu09");
	}

	private void Skill2Thunder(object obj)
	{
		BulletBase bulletBase = null;
		if (obj != null)
		{
			bulletBase = obj as BulletBase;
		}
		if (bulletBase == null)
		{
			Debug.LogError("子彈資料有誤。");
		}
		else if (bulletBase._transform.position.x > NowPos.x)
		{
			BulletBase.TryShotBullet(EnemyWeapons[5].BulletData, bulletBase._transform.position, Vector3.right, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
		}
		else
		{
			BulletBase.TryShotBullet(EnemyWeapons[5].BulletData, bulletBase._transform.position, Vector3.left, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
		}
	}

	private void SwitchShield(bool onoff)
	{
		for (int i = 0; i < Skill4Shield.Length; i++)
		{
			Skill4Shield[i].gameObject.SetActive(onoff);
			SwitchFx(Skill4ShieldFX[i], onoff);
			if (onoff)
			{
				Skill4ShieldHp[i] = MaxShieldHp;
			}
			else
			{
				Skill4ShieldHp[i] = 0;
			}
		}
		if (onoff)
		{
			if (!bShieldLP)
			{
				PlayBossSE("BossSE05", "bs046_namazu12_lp");
				bShieldLP = true;
			}
			Skill4Collide[0].Active(targetMask);
			Skill4Collide[1].Active(targetMask);
			return;
		}
		if (bShieldLP)
		{
			PlayBossSE("BossSE05", "bs046_namazu12_stop");
			bShieldLP = false;
		}
		Skill4Collide[0].BackToPool();
		Skill4Collide[1].BackToPool();
		SwitchFx(Skill4UseFX2, false);
		SwitchFx(Skill4UseFX3, false);
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
}
