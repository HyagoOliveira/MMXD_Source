#define RELEASE
using System;
using System.Collections.Generic;
using CallbackDefs;
using NaughtyAttributes;
using Newtonsoft.Json;
using StageLib;
using UnityEngine;

public class BS116_Controller : EnemyControllerBase, IManagedUpdateBehavior
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
		MAX_SUBSTATUS = 10
	}

	private enum SkillPattern
	{
		State1 = 1,
		State2 = 2,
		State3 = 3,
		State4 = 4,
		State5 = 5,
		State6 = 6,
		State7 = 7
	}

	public enum AnimationID
	{
		ANI_IDLE = 0,
		ANI_DEBUT = 1,
		ANI_SKILL0_START1 = 2,
		ANI_SKILL0_LOOP1 = 3,
		ANI_SKILL0_LOOP2 = 4,
		ANI_SKILL0_END2 = 5,
		ANI_SKILL0_START3 = 6,
		ANI_SKILL0_LOOP3 = 7,
		ANI_SKILL0_END3 = 8,
		ANI_SKILL1_START1 = 9,
		ANI_SKILL1_LOOP1 = 10,
		ANI_SKILL1_END1 = 11,
		ANI_SKILL2_START = 12,
		ANI_SKILL2_END = 13,
		ANI_SKILL3_START = 14,
		ANI_SKILL3_LOOP = 15,
		ANI_SKILL3_END = 16,
		ANI_SKILL4_START = 17,
		ANI_SKILL4_LOOP = 18,
		ANI_SKILL4_END = 19,
		ANI_SKILL5_START1 = 20,
		ANI_SKILL5_END1 = 21,
		ANI_SKILL5_LOOP2 = 22,
		ANI_SKILL5_START3 = 23,
		ANI_SKILL5_LOOP3 = 24,
		ANI_SKILL5_END3 = 25,
		ANI_SKILL6_START = 26,
		ANI_SKILL6_LOOP = 27,
		ANI_SKILL6_END = 28,
		ANI_SKILL7_LOOP1 = 29,
		ANI_SKILL7_LOOP2 = 30,
		ANI_SKILL8_START1 = 31,
		ANI_SKILL8_LOOP1 = 32,
		ANI_SKILL8_START2 = 33,
		ANI_SKILL8_LOOP2 = 34,
		ANI_SKILL8_END2 = 35,
		ANI_HURT = 36,
		ANI_DEAD = 37,
		MAX_ANIMATION_ID = 38
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

	private float MoveDis;

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

	private float NextAngle;

	private readonly int _HashAngle = Animator.StringToHash("Angle");

	[Header("衝刺攻擊")]
	[SerializeField]
	private float fSkill0JumpTime = 1f;

	[SerializeField]
	private int nSkill0DashSpd = 7500;

	[SerializeField]
	private float fSkill0AnimateActFrame = 0.67f;

	[SerializeField]
	private ParticleSystem psSkill0UseFX1;

	[SerializeField]
	private ParticleSystem psSkill0UseFX2;

	[SerializeField]
	private string sSkill1HandObjName = "Skill1Collide";

	private CollideBullet Skill1Collide;

	private int Gravity = OrangeBattleUtility.FP_Gravity * GameLogicUpdateManager.g_fixFrameLenFP / 1000;

	[Header("蓄氣光彈")]
	[SerializeField]
	private float fSkill1ChargeTime = 1.5f;

	[SerializeField]
	private float fSkill1ActFrame = 0.05f;

	[SerializeField]
	private float fSkill1IdleTime = 3f;

	[SerializeField]
	private ParticleSystem psSkill1ChargeFX;

	[Header("光彈")]
	[SerializeField]
	private float fSkill2ActFrame = 0.05f;

	[SerializeField]
	private int nSkill2ActTimes = 2;

	[Header("呼叫雷射機")]
	[SerializeField]
	private float fSkill3ActFrame = 0.5f;

	[SerializeField]
	private int nSkill3ActTimes = 2;

	private int SpawnCount;

	[Header("VAVA肩砲")]
	[SerializeField]
	private float fSkill4ActTime = 1f;

	[SerializeField]
	private int nSkill4ActTimes = 3;

	[SerializeField]
	private Transform Skill4ShootPos;

	private List<BulletBase> Skill4BulletList = new List<BulletBase>();

	[Header("抓取")]
	[SerializeField]
	private CatchPlayerTool CatchTool;

	[SerializeField]
	private Transform CatchTransform;

	[SerializeField]
	private int nSkill5ActTimes = 2;

	[SerializeField]
	private float fSkill5ChargeTime = 0.8f;

	[SerializeField]
	private int nSkill5ThrowSpeed = 4000;

	[SerializeField]
	private float fSkill5ThrowTime = 2f;

	private int nSkill5ActionFrame;

	[SerializeField]
	private string sSkill5HandObjName = "Skill5Collide";

	private CollideBullet Skill5Collide;

	[SerializeField]
	private float fSkill5IdleTime = 0.5f;

	[Header("揮拳")]
	[SerializeField]
	private int nSkill6ActTimes = 1;

	[Header("移動")]
	[SerializeField]
	private float fJudgeDis = 6f;

	[SerializeField]
	private int nWalkSpd = 1500;

	[SerializeField]
	private float fWalkTime = 3f;

	[SerializeField]
	private float fPunchDis = 1.5f;

	[SerializeField]
	private int nDashSpd = 7500;

	[Header("跳躍")]
	[SerializeField]
	private float fJumpTime = 1f;

	[SerializeField]
	private float fWallDis = 1f;

	[Header("待機等待Frame")]
	[SerializeField]
	private float IdleWaitTime = 0.2f;

	private int IdleWaitFrame;

	[Header("AI控制")]
	[SerializeField]
	[NaughtyAttributes.ReadOnly]
	private SkillPattern skillpattern = SkillPattern.State1;

	private SkillPattern nextState = SkillPattern.State1;

	[Header("Debug用")]
	[SerializeField]
	private bool DebugMode;

	[SerializeField]
	private MainStatus NextSkill;

	private bool bPlaySE05;

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
		_animationHash = new int[38];
		_animationHash[0] = Animator.StringToHash("BS116@idle_loop");
		_animationHash[1] = Animator.StringToHash("BS116@debut");
		_animationHash[2] = Animator.StringToHash("BS116@jump_start");
		_animationHash[3] = Animator.StringToHash("BS116@jump_loop");
		_animationHash[4] = Animator.StringToHash("BS116@fall_loop");
		_animationHash[5] = Animator.StringToHash("BS116@fall_end");
		_animationHash[6] = Animator.StringToHash("BS116@skill_01_start");
		_animationHash[7] = Animator.StringToHash("BS116@skill_01_loop");
		_animationHash[8] = Animator.StringToHash("BS116@skill_01_end");
		_animationHash[9] = Animator.StringToHash("BS116@skill_02_start");
		_animationHash[10] = Animator.StringToHash("BS116@skill_02_loop");
		_animationHash[11] = Animator.StringToHash("BS116@skill_02_end");
		_animationHash[12] = Animator.StringToHash("BS116@skill_03_start");
		_animationHash[13] = Animator.StringToHash("BS116@skill_03_end");
		_animationHash[14] = Animator.StringToHash("BS116@idle_loop");
		_animationHash[15] = Animator.StringToHash("BS116@idle_loop");
		_animationHash[16] = Animator.StringToHash("BS116@idle_loop");
		_animationHash[17] = Animator.StringToHash("BS116@VAVA-MK-II_SKILL4_CASTING1");
		_animationHash[18] = Animator.StringToHash("BS116@VAVA-MK-II_SKILL4_CASTLOOP1");
		_animationHash[19] = Animator.StringToHash("BS116@VAVA-MK-II_SKILL4_CASTOUT1");
		_animationHash[20] = Animator.StringToHash("BS116@VAVA-MK-II_SKILL5_CASTING1");
		_animationHash[21] = Animator.StringToHash("BS116@VAVA-MK-II_SKILL5_CASTOUT2");
		_animationHash[22] = Animator.StringToHash("BS116@VAVA-MK-II_SKILL5_CASTLOOP1");
		_animationHash[23] = Animator.StringToHash("BS116@VAVA-MK-II_SKILL5_CASTING2");
		_animationHash[24] = Animator.StringToHash("BS116@VAVA-MK-II_SKILL5_CASTLOOP2");
		_animationHash[25] = Animator.StringToHash("BS116@VAVA-MK-II_SKILL5_CASTOUT1");
		_animationHash[26] = Animator.StringToHash("BS116@skill_01s_start");
		_animationHash[27] = Animator.StringToHash("BS116@skill_01s_loop");
		_animationHash[28] = Animator.StringToHash("BS116@skill_01s_end");
		_animationHash[29] = Animator.StringToHash("BS116@run_loop");
		_animationHash[30] = Animator.StringToHash("BS116@sprint_loop");
		_animationHash[31] = Animator.StringToHash("BS116@jump_start");
		_animationHash[32] = Animator.StringToHash("BS116@jump_loop");
		_animationHash[33] = Animator.StringToHash("BS116@fall_start");
		_animationHash[34] = Animator.StringToHash("BS116@fall_loop");
		_animationHash[35] = Animator.StringToHash("BS116@fall_end");
		_animationHash[36] = Animator.StringToHash("BS116@hurt_loop");
		_animationHash[37] = Animator.StringToHash("BS116@dead");
	}

	private void LoadParts(ref Transform[] childs)
	{
		ModelTransform = OrangeBattleUtility.FindChildRecursive(ref childs, "model", true);
		_animator = ModelTransform.GetComponent<Animator>();
		_collideBullet = OrangeBattleUtility.FindChildRecursive(ref childs, "Collider", true).gameObject.AddOrGetComponent<CollideBullet>();
		Skill1Collide = OrangeBattleUtility.FindChildRecursive(ref childs, sSkill1HandObjName, true).gameObject.AddOrGetComponent<CollideBullet>();
		Skill5Collide = OrangeBattleUtility.FindChildRecursive(ref childs, sSkill5HandObjName, true).gameObject.AddOrGetComponent<CollideBullet>();
		if (ShootPos == null)
		{
			ShootPos = OrangeBattleUtility.FindChildRecursive(ref childs, "ShootPoint_C", true);
		}
		if (Skill4ShootPos == null)
		{
			Skill4ShootPos = OrangeBattleUtility.FindChildRecursive(ref childs, "sub_weapon_shootpoint", true);
		}
		if (psSkill0UseFX1 == null)
		{
			psSkill0UseFX1 = OrangeBattleUtility.FindChildRecursive(ref childs, "psSkill0UseFX1", true).GetComponent<ParticleSystem>();
		}
		if (psSkill0UseFX2 == null)
		{
			psSkill0UseFX2 = OrangeBattleUtility.FindChildRecursive(ref childs, "psSkill0UseFX2", true).GetComponent<ParticleSystem>();
		}
		if (psSkill1ChargeFX == null)
		{
			psSkill1ChargeFX = OrangeBattleUtility.FindChildRecursive(ref childs, "psSkill1ChargeFX", true).GetComponent<ParticleSystem>();
		}
	}

	protected override void Awake()
	{
		base.Awake();
		Transform[] childs = _transform.GetComponentsInChildren<Transform>(true);
		LoadParts(ref childs);
		HashAnimation();
		base.AimPoint = new Vector3(0f, 1.2f, 0f);
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
			UpdateDirection();
			_velocity.x = 0;
			IdleWaitFrame = GameLogicUpdateManager.GameFrame + (int)(IdleWaitTime * 20f);
			break;
		case MainStatus.Skill0:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_velocity = VInt3.zero;
				EndPos = GetTargetPos();
				switch (skillpattern)
				{
				default:
					if (!DebugMode)
					{
						Debug.LogError("此階段不該進來衝刺攻擊，請回報");
					}
					break;
				case SkillPattern.State3:
					SetStatus(MainStatus.Skill0, SubStatus.Phase4);
					UpdateDirection();
					return;
				case SkillPattern.State7:
					if (Math.Abs(EndPos.y - NowPos.y) < 1.5f)
					{
						SetStatus(MainStatus.Skill0, SubStatus.Phase4);
						UpdateDirection();
						return;
					}
					_velocity.y = (int)(fSkill0JumpTime * 20f * (float)(-Gravity));
					break;
				}
				break;
			case SubStatus.Phase2:
				_velocity = VInt3.zero;
				IgnoreGravity = false;
				break;
			case SubStatus.Phase3:
				SwitchCollideBullet(_collideBullet, true, 0);
				break;
			case SubStatus.Phase4:
				HasActed = false;
				ActionAnimatorFrame = fSkill0AnimateActFrame;
				break;
			case SubStatus.Phase5:
				SwitchCollideBullet(Skill1Collide, true, 3);
				break;
			case SubStatus.Phase6:
				SwitchCollideBullet(_collideBullet, true, 0);
				_velocity = VInt3.zero;
				IgnoreGravity = false;
				break;
			}
			break;
		case MainStatus.Skill1:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_velocity = VInt3.zero;
				UpdateDirection();
				break;
			case SubStatus.Phase1:
				PlayBossSE("BossSE", 126);
				SwitchFx(psSkill1ChargeFX, true);
				ActionFrame = GameLogicUpdateManager.GameFrame + (int)(fSkill1ChargeTime * 20f);
				break;
			case SubStatus.Phase2:
				ActionAnimatorFrame = fSkill1ActFrame;
				HasActed = false;
				break;
			case SubStatus.Phase3:
				ActionFrame = GameLogicUpdateManager.GameFrame + (int)(fSkill1IdleTime * 20f);
				break;
			}
			break;
		case MainStatus.Skill2:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_velocity = VInt3.zero;
				UpdateDirection();
				break;
			case SubStatus.Phase1:
				ActionTimes = nSkill2ActTimes;
				ActionAnimatorFrame = fSkill2ActFrame;
				HasActed = false;
				break;
			}
			break;
		case MainStatus.Skill3:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_velocity = VInt3.zero;
				UpdateDirection((!(NowPos.x > CenterPos.x)) ? 1 : (-1));
				ActionTimes = nSkill3ActTimes;
				break;
			case SubStatus.Phase1:
				ActionAnimatorFrame = fSkill3ActFrame;
				HasActed = false;
				break;
			}
			break;
		case MainStatus.Skill4:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				Skill4BulletList.Clear();
				ActionTimes = nSkill4ActTimes;
				_velocity = VInt3.zero;
				UpdateDirection();
				break;
			case SubStatus.Phase1:
				ActionFrame = GameLogicUpdateManager.GameFrame + (int)(fSkill4ActTime * 20f);
				HasActed = false;
				break;
			}
			break;
		case MainStatus.Skill5:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_velocity = VInt3.zero;
				ActionTimes = nSkill5ActTimes;
				UpdateDirection();
				CatchTool.CatchTransform = CatchTransform;
				SwitchCollideBullet(_collideBullet, false);
				break;
			case SubStatus.Phase1:
				if (CatchTool.IsCatching)
				{
					CatchTool.ReleaseTarget();
				}
				break;
			case SubStatus.Phase2:
				HasActed = false;
				SwitchCollideBullet(Skill5Collide, true);
				break;
			case SubStatus.Phase3:
				SwitchCollideBullet(Skill5Collide, false);
				break;
			case SubStatus.Phase4:
				SwitchFx(psSkill1ChargeFX, true);
				ActionFrame = GameLogicUpdateManager.GameFrame + (int)(fSkill5ChargeTime * 20f);
				break;
			case SubStatus.Phase5:
				SwitchFx(psSkill1ChargeFX, false);
				BulletBase.TryShotBullet(EnemyWeapons[1].BulletData, ShootPos, new Vector3(5f, 1f, 0f).normalized, null, selfBuffManager.sBuffStatus, EnemyData, targetMask, true);
				nSkill5ActionFrame = GameLogicUpdateManager.GameFrame + (int)(fSkill5ThrowTime * 20f);
				break;
			case SubStatus.Phase6:
				ActionFrame = GameLogicUpdateManager.GameFrame + (int)(fSkill5IdleTime * 20f);
				SwitchCollideBullet(_collideBullet, true);
				break;
			}
			break;
		case MainStatus.Skill6:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_velocity = VInt3.zero;
				UpdateDirection();
				SwitchCollideBullet(Skill1Collide, true, 7);
				break;
			case SubStatus.Phase2:
				SwitchCollideBullet(Skill1Collide, false);
				break;
			}
			break;
		case MainStatus.Skill7:
			StartPos = NowPos;
			EndPos = GetTargetPos();
			UpdateDirection();
			EndPos -= Vector3.right * base.direction;
			MoveDis = Math.Abs(EndPos.x - StartPos.x);
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_velocity.x = nWalkSpd * base.direction;
				switch (skillpattern)
				{
				case SkillPattern.State2:
					if (MoveDis > fJudgeDis)
					{
						PlayBossSE("BossSE", "bs004_vava16");
						SetStatus(MainStatus.Skill7, SubStatus.Phase1);
						return;
					}
					break;
				case SkillPattern.State5:
					ActionFrame = GameLogicUpdateManager.GameFrame + (int)(fWalkTime * 20f);
					break;
				}
				break;
			case SubStatus.Phase1:
				_velocity.x = nDashSpd * base.direction;
				break;
			}
			break;
		case MainStatus.Skill8:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
			{
				PlayBossSE("BossSE", "bs004_vava02");
				EndPos = ((GetTargetPos().x > CenterPos.x) ? (MaxPos - Vector3.right * fWallDis) : (MinPos + Vector3.right * fWallDis));
				int num = (int)(fJumpTime * 20f);
				ActionFrame = GameLogicUpdateManager.GameFrame + num / 2;
				_velocity = new VInt3(CalXSpeed(NowPos.x, EndPos.x, fJumpTime, 2f), Math.Abs(Gravity * num), 0);
				break;
			}
			}
			break;
		case MainStatus.Die:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (bPlaySE05)
				{
					base.SoundSource.StopAll();
					bPlaySE05 = false;
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
				StartCoroutine(BossDieFlow(_transform.position, "FX_BOSS_EXPLODE2", false, false));
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
				_currentAnimationId = AnimationID.ANI_IDLE;
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
			default:
				return;
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_SKILL0_START1;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_SKILL0_LOOP1;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_SKILL0_LOOP2;
				break;
			case SubStatus.Phase3:
				_currentAnimationId = AnimationID.ANI_SKILL0_END2;
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
			default:
				return;
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_SKILL1_START1;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_SKILL1_LOOP1;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_SKILL1_END1;
				break;
			case SubStatus.Phase3:
				_currentAnimationId = AnimationID.ANI_IDLE;
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
				_currentAnimationId = AnimationID.ANI_SKILL4_START;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_SKILL4_LOOP;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_SKILL4_END;
				break;
			case SubStatus.Phase3:
				_currentAnimationId = AnimationID.ANI_IDLE;
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
				_currentAnimationId = AnimationID.ANI_SKILL5_END1;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_SKILL5_LOOP2;
				break;
			case SubStatus.Phase3:
				_currentAnimationId = AnimationID.ANI_SKILL5_START3;
				break;
			case SubStatus.Phase4:
				_currentAnimationId = AnimationID.ANI_SKILL5_LOOP3;
				break;
			case SubStatus.Phase5:
				_currentAnimationId = AnimationID.ANI_SKILL5_END3;
				break;
			case SubStatus.Phase6:
				_currentAnimationId = AnimationID.ANI_IDLE;
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
				_currentAnimationId = AnimationID.ANI_SKILL7_LOOP1;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_SKILL7_LOOP2;
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
				_currentAnimationId = AnimationID.ANI_SKILL8_END2;
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
				if (DebugMode)
				{
					mainStatus = NextSkill;
					break;
				}
				skillpattern = nextState;
				switch (skillpattern)
				{
				case SkillPattern.State1:
					mainStatus = MainStatus.Skill4;
					nextState = SkillPattern.State3;
					break;
				case SkillPattern.State2:
					mainStatus = MainStatus.Skill7;
					nextState = SkillPattern.State3;
					break;
				case SkillPattern.State3:
					mainStatus = MainStatus.Skill0;
					nextState = SkillPattern.State4;
					break;
				case SkillPattern.State4:
					mainStatus = MainStatus.Skill3;
					nextState = SkillPattern.State5;
					break;
				case SkillPattern.State5:
					mainStatus = MainStatus.Skill7;
					nextState = SkillPattern.State6;
					break;
				case SkillPattern.State6:
					mainStatus = MainStatus.Skill8;
					nextState = SkillPattern.State7;
					break;
				case SkillPattern.State7:
					mainStatus = MainStatus.Skill1;
					nextState = SkillPattern.State1;
					break;
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
					PlaySE("BossSE", 129);
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
				UpdateNextState();
			}
			else
			{
				UpdateNextState();
			}
			break;
		case MainStatus.Skill0:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (_currentFrame > 1f || Math.Abs(EndPos.y - NowPos.y) < 1.5f)
				{
					SetStatus(MainStatus.Skill0, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if (GameLogicUpdateManager.GameFrame >= ActionFrame || Math.Abs(EndPos.y - NowPos.y) < 1.5f)
				{
					IgnoreGravity = true;
					SetStatus(MainStatus.Skill0, SubStatus.Phase4);
				}
				break;
			case SubStatus.Phase2:
				if (Controller.Collisions.below)
				{
					SetStatus(MainStatus.Skill0, SubStatus.Phase3);
				}
				break;
			case SubStatus.Phase3:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Idle);
				}
				break;
			case SubStatus.Phase4:
				if (!HasActed && _currentFrame > ActionAnimatorFrame)
				{
					_velocity.x = nSkill0DashSpd * base.direction;
					HasActed = true;
					SwitchFx(psSkill0UseFX1, true);
					SwitchFx(psSkill0UseFX2, true);
					SwitchCollideBullet(Skill1Collide, true, 3);
					if (!bPlaySE05)
					{
						PlayBossSE("BossSE", 29);
						bPlaySE05 = true;
					}
				}
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill0, SubStatus.Phase5);
				}
				break;
			case SubStatus.Phase5:
				if (Controller.Collisions.right || Controller.Collisions.left)
				{
					SetStatus(MainStatus.Skill0, Controller.Collisions.below ? SubStatus.Phase6 : SubStatus.Phase2);
					SwitchFx(psSkill0UseFX1, false);
					SwitchFx(psSkill0UseFX2, false);
					SwitchCollideBullet(Skill1Collide, false);
					SwitchCollideBullet(_collideBullet, true);
					Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.CAMERA_SHAKE, 0.5f, false);
					MonoBehaviourSingleton<OrangeBattleUtility>.Instance.SetLockWallJump(2000);
					SwitchCollideBullet(Skill1Collide, false);
					if (bPlaySE05)
					{
						PlayBossSE("BossSE", 124);
						bPlaySE05 = false;
					}
					PlayBossSE("BossSE", "bs004_vava04");
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
		case MainStatus.Skill1:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				EndPos = GetTargetPos();
				NextAngle = Vector3.Angle((EndPos - ShootPos.position).normalized, Vector3.up);
				ShotAngle = Mathf.Lerp(ShotAngle, NextAngle, 0.3f);
				_animator.SetFloat(_HashAngle, ShotAngle);
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill1, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				EndPos = GetTargetPos();
				NextAngle = Vector3.Angle((EndPos - ShootPos.position).normalized, Vector3.up);
				ShotAngle = Mathf.Lerp(ShotAngle, NextAngle, 0.3f);
				_animator.SetFloat(_HashAngle, ShotAngle);
				UpdateDirection();
				if (GameLogicUpdateManager.GameFrame >= ActionFrame)
				{
					SetStatus(MainStatus.Skill1, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				if (!HasActed && _currentFrame > ActionAnimatorFrame)
				{
					HasActed = true;
					PlayBossSE("BossSE", 127);
					SwitchFx(psSkill1ChargeFX, false);
					Vector3 pDirection2 = EndPos - ShootPos.position;
					BulletBase.TryShotBullet(EnemyWeapons[1].BulletData, ShootPos, pDirection2, null, selfBuffManager.sBuffStatus, EnemyData, targetMask, true);
				}
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill1, SubStatus.Phase3);
				}
				break;
			case SubStatus.Phase3:
				if (GameLogicUpdateManager.GameFrame >= ActionFrame)
				{
					SetStatus(MainStatus.Skill0);
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
				if (!HasActed && _currentFrame > ActionAnimatorFrame)
				{
					HasActed = true;
					PlayBossSE("BossSE", 30);
					BulletBase.TryShotBullet(EnemyWeapons[2].BulletData, ShootPos, Vector3.right * base.direction, null, selfBuffManager.sBuffStatus, EnemyData, targetMask, true);
				}
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
				if (!HasActed && _currentFrame > ActionAnimatorFrame)
				{
					HasActed = true;
					EndPos = GetTargetPos();
					EM199_Controller eM199_Controller = SpawnEnemy(GetTargetPoint());
					if ((bool)eM199_Controller)
					{
						eM199_Controller.SetEndDegree(100 + ActionTimes * 30);
						eM199_Controller.SetRotateDirection(-base.direction);
						eM199_Controller.SetPositionAndRotation(GetTargetPoint(), base.direction == 1);
						eM199_Controller.SetActive(true);
						switch (ActionTimes)
						{
						case 1:
							eM199_Controller.SetSkill0AtkPos(2);
							break;
						case 2:
							eM199_Controller.SetSkill0AtkPos(6);
							break;
						case 3:
							eM199_Controller.SetSkill0AtkPos(8);
							break;
						case 4:
							eM199_Controller.SetSkill0AtkPos(4);
							break;
						}
					}
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
					SetStatus(MainStatus.Idle);
				}
				break;
			}
			break;
		case MainStatus.Skill4:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				EndPos = GetTargetPos();
				NextAngle = Vector3.Angle((EndPos - ShootPos.position).normalized, Vector3.up);
				ShotAngle = Mathf.Lerp(ShotAngle, NextAngle, 0.3f);
				_animator.SetFloat(_HashAngle, ShotAngle);
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill4, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				EndPos = GetTargetPos();
				UpdateDirection();
				NextAngle = Vector3.Angle((EndPos - ShootPos.position).normalized, Vector3.up);
				ShotAngle = Mathf.Lerp(ShotAngle, NextAngle, 0.3f);
				_animator.SetFloat(_HashAngle, ShotAngle);
				if (GameLogicUpdateManager.GameFrame >= ActionFrame)
				{
					Vector3 pDirection = EndPos - Skill4ShootPos.position;
					BulletBase bulletBase = BulletBase.TryShotBullet(EnemyWeapons[4].BulletData, Skill4ShootPos, pDirection, null, selfBuffManager.sBuffStatus, EnemyData, targetMask, true);
					bulletBase.HitCallback = Skill4HitPlayer;
					if ((bool)bulletBase)
					{
						Skill4BulletList.Add(bulletBase);
					}
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
					SetStatus(MainStatus.Skill4, SubStatus.Phase3);
				}
				break;
			case SubStatus.Phase3:
			{
				bool flag = true;
				foreach (BulletBase skill4Bullet in Skill4BulletList)
				{
					if (!skill4Bullet.bIsEnd)
					{
						flag = false;
						break;
					}
				}
				if (flag || nextState == SkillPattern.State2)
				{
					Skill4BulletList.Clear();
					SetStatus(MainStatus.Idle);
				}
				break;
			}
			}
			break;
		case MainStatus.Skill5:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (_currentFrame > 1f)
				{
					if (CatchTool.IsCatching)
					{
						SetStatus(MainStatus.Skill5, SubStatus.Phase2);
					}
					else
					{
						SetStatus(MainStatus.Skill5, SubStatus.Phase1);
					}
				}
				break;
			case SubStatus.Phase1:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Idle);
				}
				break;
			case SubStatus.Phase2:
				if (!HasActed && _currentFrame > ActionAnimatorFrame)
				{
					HasActed = true;
					PlayBossSE("BossSE", "bs004_vava01");
					MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxhit_common_000", CatchTransform, Quaternion.identity, null);
				}
				if (_currentFrame > 1f)
				{
					if (--ActionTimes > 0)
					{
						SetStatus(MainStatus.Skill5, SubStatus.Phase2);
					}
					else
					{
						SetStatus(MainStatus.Skill5, SubStatus.Phase3);
					}
				}
				break;
			case SubStatus.Phase3:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill5, SubStatus.Phase4);
				}
				break;
			case SubStatus.Phase4:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill5, SubStatus.Phase5);
				}
				break;
			case SubStatus.Phase5:
				if (CatchTool.IsCatching && GameLogicUpdateManager.GameFrame > nSkill5ActionFrame)
				{
					CatchTool.ReleaseTarget();
				}
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill5, SubStatus.Phase6);
				}
				break;
			case SubStatus.Phase6:
				if (CatchTool.IsCatching && GameLogicUpdateManager.GameFrame > nSkill5ActionFrame)
				{
					CatchTool.ReleaseTarget();
				}
				if (GameLogicUpdateManager.GameFrame >= ActionFrame)
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
					SetStatus(MainStatus.Skill6, SubStatus.Phase2);
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
					SetStatus(MainStatus.Idle);
				}
				break;
			}
			break;
		case MainStatus.Skill7:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				switch (skillpattern)
				{
				case SkillPattern.State5:
					EndPos = GetTargetPos();
					if (Math.Abs(NowPos.x - EndPos.x) < fPunchDis)
					{
						SetStatus(MainStatus.Skill6);
					}
					else if (GameLogicUpdateManager.GameFrame >= ActionFrame)
					{
						SetStatus(MainStatus.Idle);
					}
					break;
				case SkillPattern.State2:
					if (Math.Abs(GetTargetPoint().x - StartPos.x) > MoveDis || Controller.Collisions.right || Controller.Collisions.left)
					{
						SetStatus(MainStatus.Skill5);
					}
					break;
				default:
					if (!DebugMode)
					{
						Debug.LogError("這個階段不該進來走路狀態，請回報");
					}
					break;
				}
				break;
			case SubStatus.Phase1:
				if (Math.Abs(GetTargetPoint().x - StartPos.x) > MoveDis || Controller.Collisions.right || Controller.Collisions.left)
				{
					SetStatus(MainStatus.Skill5);
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
				if (GameLogicUpdateManager.GameFrame >= ActionFrame)
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
				if (Controller.Collisions.below)
				{
					SetStatus(MainStatus.Skill8, SubStatus.Phase4);
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
		if (!Activate && _mainStatus != MainStatus.Debut)
		{
			return;
		}
		base.transform.localPosition = Vector3.MoveTowards(base.transform.localPosition, Controller.LogicPosition.vec3, distanceDelta);
		MainStatus mainStatus = _mainStatus;
		if (mainStatus == MainStatus.Skill5)
		{
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (!CatchTool.IsCatching)
				{
					Collider2D collider2D = Physics2D.OverlapBox(NowPos + Vector3.right * base.direction, Vector2.one * 3f, 0f, LayerMask.GetMask("Player"));
					if ((bool)collider2D && (int)Hp > 0)
					{
						PlayBossSE("BossSE", "bs004_vava15");
						OrangeCharacter component = collider2D.GetComponent<OrangeCharacter>();
						CatchTool.CatchTarget(component, true, true);
						CatchTool.PosOffset = new Vector3(0f, 0.5f - (component.GetTargetPoint().y - component._transform.position.y), 0f);
					}
				}
				if (CatchTool.IsCatching)
				{
					CatchTool.MoveTarget();
				}
				break;
			case SubStatus.Phase2:
			case SubStatus.Phase3:
			case SubStatus.Phase4:
				if (CatchTool.IsCatching)
				{
					CatchTool.MoveTarget();
				}
				break;
			case SubStatus.Phase5:
				if (_currentFrame > 0.05f && CatchTool.IsCatching)
				{
					if (CatchTool.TargetOC.Controller.Collisions.right || CatchTool.TargetOC.Controller.Collisions.left)
					{
						CatchTool.MoveTargetWithForce(new VInt3(nSkill5ThrowSpeed * base.direction, Gravity, 0));
					}
					else
					{
						CatchTool.MoveTargetWithForce(new VInt3(nSkill5ThrowSpeed * base.direction, 800, 0));
					}
				}
				else if (CatchTool.IsCatching)
				{
					CatchTool.MoveTarget();
				}
				break;
			case SubStatus.Phase6:
				if (CatchTool.IsCatching)
				{
					if (CatchTool.TargetOC.Controller.Collisions.right || CatchTool.TargetOC.Controller.Collisions.left)
					{
						CatchTool.MoveTargetWithForce(new VInt3(nSkill5ThrowSpeed * base.direction, Gravity, 0));
					}
					else
					{
						CatchTool.MoveTargetWithForce(new VInt3(nSkill5ThrowSpeed * base.direction, 800, 0));
					}
				}
				break;
			case SubStatus.Phase1:
				break;
			}
		}
		else if (CatchTool.IsCatching)
		{
			if (CatchTool.TargetOC.Controller.Collisions.right || CatchTool.TargetOC.Controller.Collisions.left)
			{
				CatchTool.MoveTargetWithForce(new VInt3(nSkill5ThrowSpeed * base.direction, Gravity, 0));
			}
			else
			{
				CatchTool.MoveTargetWithForce(new VInt3(nSkill5ThrowSpeed * base.direction, 800, 0));
			}
			if (GameLogicUpdateManager.GameFrame > nSkill5ActionFrame)
			{
				CatchTool.ReleaseTarget();
			}
		}
	}

	public override void SetActive(bool isActive)
	{
		base.SetActive(isActive);
		if (isActive)
		{
			SwitchCollideBullet(_collideBullet, true, 0);
			SwitchCollideBullet(Skill1Collide, false, 3);
			SwitchCollideBullet(Skill5Collide, false, 5);
			SetMaxGravity(OrangeBattleUtility.FP_MaxGravity * 2);
			CheckRoomSize();
			SetStatus(MainStatus.Debut);
		}
		else
		{
			_collideBullet.BackToPool();
		}
	}

	protected override void DeadBehavior(ref HurtPassParam tHurtPassParam)
	{
		if (_mainStatus == MainStatus.Die)
		{
			return;
		}
		IgnoreGravity = false;
		if ((bool)_collideBullet)
		{
			_collideBullet.BackToPool();
		}
		List<EM199_Controller> list = new List<EM199_Controller>();
		for (int i = 0; i < StageUpdate.runEnemys.Count; i++)
		{
			EM199_Controller eM199_Controller = StageUpdate.runEnemys[i].mEnemy as EM199_Controller;
			if ((bool)eM199_Controller)
			{
				list.Add(eM199_Controller);
			}
		}
		foreach (EM199_Controller item in list)
		{
			item.SetDie();
		}
		if (CatchTool.IsCatching)
		{
			CatchTool.ReleaseTarget();
		}
		StageUpdate.SlowStage();
		SetColliderEnable(false);
		SetStatus(MainStatus.Die);
	}

	public override void UpdateEnemyID(int id)
	{
		base.UpdateEnemyID(id);
		UpdateAIState();
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

	private MainStatus AIPhaseTree()
	{
		AI_STATE aiState = AiState;
		MainStatus mainStatus = _mainStatus;
		SubStatus subStatus = _subStatus;
		return MainStatus.Idle;
	}

	private int CalXSpeed(float startx, float endx, float jumptime, float timeoffset = 1f)
	{
		int num = (int)((float)Math.Abs((int)(jumptime * 20f)) * timeoffset);
		return (int)((endx - startx) * 1000f * 20f / (float)num);
	}

	private void CheckRoomSize()
	{
		int layerMask = (1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockLayer) | (1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockEnemyLayer) | (1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockPlayerLayer) | (1 << ManagedSingleton<OrangeLayerManager>.Instance.WallKickMask);
		Vector3 vector = new Vector3(_transform.position.x, _transform.position.y + 1f, 0f);
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

	private EM199_Controller SpawnEnemy(Vector3 SpawnPos)
	{
		int num = 8;
		MOB_TABLE enemy = GetEnemy((int)EnemyWeapons[num].BulletData.f_EFFECT_X);
		if (enemy == null)
		{
			Debug.LogError("要生成的怪物資料有誤，生怪技能ID " + num + " 怪物GroupID " + EnemyWeapons[num].BulletData.f_EFFECT_X);
			return null;
		}
		EnemyControllerBase enemyControllerBase = StageUpdate.StageSpawnEnemyByMob(enemy, sNetSerialID + SpawnCount);
		SpawnCount++;
		if ((bool)enemyControllerBase)
		{
			PlayBossSE("BossSE", "bs004_vava14");
			return enemyControllerBase as EM199_Controller;
		}
		return null;
	}

	private MOB_TABLE GetEnemy(int nGroupID)
	{
		MOB_TABLE[] mobArrayFromGroup = ManagedSingleton<OrangeTableHelper>.Instance.GetMobArrayFromGroup(nGroupID);
		for (int i = 0; i < mobArrayFromGroup.Length; i++)
		{
			if (mobArrayFromGroup[i].n_DIFFICULTY == StageUpdate.gDifficulty)
			{
				return mobArrayFromGroup[i];
			}
		}
		return null;
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
			if (collide.IsActivate)
			{
				collide.BackToPool();
			}
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
		else if (collide.IsActivate)
		{
			collide.BackToPool();
		}
	}

	private void Skill4HitPlayer(object obj)
	{
		if (obj == null || CatchTool.IsCatching)
		{
			return;
		}
		Collider2D collider2D = obj as Collider2D;
		if (!(collider2D != null) || !(OrangeBattleUtility.GetHitTargetOrangeCharacter(collider2D) != null))
		{
			return;
		}
		nextState = SkillPattern.State2;
		foreach (BulletBase skill4Bullet in Skill4BulletList)
		{
			skill4Bullet.HitCallback = null;
		}
	}
}
