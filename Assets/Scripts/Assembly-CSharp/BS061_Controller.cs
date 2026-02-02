#define RELEASE
using System;
using System.Collections.Generic;
using CallbackDefs;
using NaughtyAttributes;
using Newtonsoft.Json;
using StageLib;
using UnityEngine;

public class BS061_Controller : EnemyControllerBase, IManagedUpdateBehavior, IF_Master
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
		MAX_SUBSTATUS = 10
	}

	public enum AnimationID
	{
		ANI_IDLE = 0,
		ANI_DEBUT = 1,
		ANI_SKILL0_START1 = 2,
		ANI_SKILL0_LOOP1 = 3,
		ANI_SKILL0_START2 = 4,
		ANI_SKILL0_LOOP2 = 5,
		ANI_SKILL0_START3 = 6,
		ANI_SKILL0_LOOP3 = 7,
		ANI_SKILL0_END3 = 8,
		ANI_SKILL1_START = 9,
		ANI_SKILL1_LOOP = 10,
		ANI_SKILL1_END = 11,
		ANI_SKILL2_LOOP1 = 12,
		ANI_SKILL2_START2 = 13,
		ANI_SKILL2_LOOP2 = 14,
		ANI_SKILL2_LOOP3 = 15,
		ANI_SKILL2_END3 = 16,
		ANI_SKILL3_START1 = 17,
		ANI_SKILL3_LOOP1 = 18,
		ANI_SKILL3_START2 = 19,
		ANI_SKILL3_LOOP2 = 20,
		ANI_SKILL3_END2 = 21,
		ANI_HURT = 22,
		ANI_DEAD = 23,
		MAX_ANIMATION_ID = 24
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

	private int[] DefaultSkillCard = new int[3] { 2, 3, 4 };

	private List<int> SkillCard = new List<int>();

	private Vector3 LastTargetPos = new Vector3(0f, 0f, 0f);

	[SerializeField]
	private float IdleWaitTime = 0.2f;

	private int IdleWaitFrame;

	[Header("通用")]
	private int ActionTimes;

	private int SkillRunTimes;

	private float ActionAnimatorFrame;

	private int ActionFrame;

	private bool HasActed;

	private Vector3 ShotPos;

	private Vector3 EndPos;

	[SerializeField]
	private float MinX;

	[SerializeField]
	private float MaxX;

	[SerializeField]
	private Vector3 CenterPos;

	[SerializeField]
	private float GroundYPos;

	[SerializeField]
	private int JumpSpeed = 15000;

	[SerializeField]
	private int RunSpeed = 9000;

	[SerializeField]
	private int BackJumpSpeed = 12000;

	[SerializeField]
	private ParticleSystem CycleFX;

	private bool HasCheckRoomSize;

	private int Gravity = OrangeBattleUtility.FP_Gravity * GameLogicUpdateManager.g_fixFrameLenFP / 1000;

	[Header("彩色分身")]
	[SerializeField]
	private ParticleSystem ShinyFX;

	[SerializeField]
	private float WallPosOffset;

	[SerializeField]
	private int Skill0ShootTimes = 6;

	[SerializeField]
	private int Skill0JumpSpeed = 10000;

	[SerializeField]
	private Transform Skill0SpawnPos;

	private int SpawnCount;

	private bool HasUseSkill0;

	[Header("漫天毒霧")]
	[SerializeField]
	private string MistFXName;

	[SerializeField]
	private int Skill1ActionTimes = 2;

	[SerializeField]
	private float Skill1JumpTime = 0.5f;

	private EM176_Controller CloneShadow;

	private BossCorpsTool CloneCorp;

	[Header("跑跑跳跳")]
	[SerializeField]
	private int Skill2ActionTimes = 3;

	[Header("倒立毒霧")]
	[SerializeField]
	private int Skill3RunTimes = 2;

	[SerializeField]
	private int Skill3ActionTimes = 2;

	[SerializeField]
	private float Skill3RotateTime = 1f;

	[SerializeField]
	private int Skill3RotateSpeed = 15000;

	[SerializeField]
	private int Skill3JumpSpeed = 9000;

	private int StartDirection = 1;

	[Header("倒立毒霧-追玩家")]
	[SerializeField]
	private int Skill4RunTimes = 4;

	[SerializeField]
	private bool DebugMode;

	[SerializeField]
	private MainStatus NextSkill;

	private bool CanSummon;

	private bool bLPPlaying;

	private Vector3 NowPos
	{
		get
		{
			return _transform.position;
		}
	}

	private void Playbs038_mushroom03_lp(bool bPlay)
	{
		if (bPlay)
		{
			PlaySE("BossSE04", "bs038_mushroom03_lp");
			bLPPlaying = true;
		}
		else
		{
			PlaySE("BossSE04", "bs038_mushroom03_stop");
			bLPPlaying = false;
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

	protected virtual void HashAnimation()
	{
		_animationHash = new int[24];
		_animationHash[0] = Animator.StringToHash("BS061@idle_loop");
		_animationHash[1] = Animator.StringToHash("BS061@debut");
		_animationHash[2] = Animator.StringToHash("BS061@skill_01_step1_start");
		_animationHash[3] = Animator.StringToHash("BS061@skill_01_step1_loop");
		_animationHash[4] = Animator.StringToHash("BS061@skill_01_step2_start");
		_animationHash[5] = Animator.StringToHash("BS061@skill_01_step2_loop");
		_animationHash[6] = Animator.StringToHash("BS061@skill_01_step3_start");
		_animationHash[7] = Animator.StringToHash("BS061@skill_01_step3_loop");
		_animationHash[8] = Animator.StringToHash("BS061@skill_01_step3_end");
		_animationHash[9] = Animator.StringToHash("BS061@skill_03_start");
		_animationHash[10] = Animator.StringToHash("BS061@skill_03_loop");
		_animationHash[11] = Animator.StringToHash("BS061@skill_03_end");
		_animationHash[12] = Animator.StringToHash("BS061@run_loop");
		_animationHash[13] = Animator.StringToHash("BS061@skill_01_step1_start");
		_animationHash[14] = Animator.StringToHash("BS061@skill_01_step1_loop");
		_animationHash[15] = Animator.StringToHash("BS061@skill_01_step3_loop");
		_animationHash[16] = Animator.StringToHash("BS061@skill_01_step3_end");
		_animationHash[17] = Animator.StringToHash("BS061@skill_02_start");
		_animationHash[18] = Animator.StringToHash("BS061@skill_02_loop");
		_animationHash[19] = Animator.StringToHash("BS061@skill_04_start");
		_animationHash[20] = Animator.StringToHash("BS061@skill_04_loop");
		_animationHash[21] = Animator.StringToHash("BS061@skill_04_end");
		_animationHash[22] = Animator.StringToHash("BS061@hurt_loop");
		_animationHash[23] = Animator.StringToHash("BS061@death");
	}

	private void LoadParts(ref Transform[] childs)
	{
		ModelTransform = OrangeBattleUtility.FindChildRecursive(ref childs, "model", true);
		_animator = ModelTransform.GetComponent<Animator>();
		_collideBullet = OrangeBattleUtility.FindChildRecursive(ref childs, "Collider", true).gameObject.AddOrGetComponent<CollideBullet>();
		if (CycleFX == null)
		{
			CycleFX = OrangeBattleUtility.FindChildRecursive(ref childs, "CycleFX", true).GetComponent<ParticleSystem>();
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
		FallDownSE = new string[2] { "BossSE04", "bs038_mushroom02" };
		AiTimer.TimerStart();
	}

	protected override void Start()
	{
		base.Start();
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(MistFXName);
	}

	public override void UpdateStatus(int nSet, string smsg, Callback tCB = null)
	{
		bWaitNetStatus = false;
		if ((int)Hp <= 0)
		{
			return;
		}
		_velocity = VInt3.zero;
		if (!HasCheckRoomSize)
		{
			CheckRoomSize();
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
			if (netSyncData.nParam0 != -1)
			{
				SpawnCount = netSyncData.nParam0;
			}
			UpdateDirection();
		}
		SetColliderEnable(true);
		IgnoreGravity = false;
		switch ((MainStatus)nSet)
		{
		case MainStatus.Skill0:
		case MainStatus.Skill1:
			HasUseSkill0 = true;
			break;
		case MainStatus.Skill2:
		case MainStatus.Skill3:
		case MainStatus.Skill4:
			HasUseSkill0 = false;
			break;
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
			break;
		case MainStatus.Skill0:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_collideBullet.UpdateBulletData(EnemyWeapons[0].BulletData);
				_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
				_collideBullet.Active(targetMask);
				HasUseSkill0 = true;
				ActionAnimatorFrame = 0.5f;
				HasActed = false;
				break;
			case SubStatus.Phase2:
				UpdateDirection(-base.direction);
				IgnoreGravity = true;
				ActionTimes = Skill0ShootTimes;
				break;
			case SubStatus.Phase3:
				SpawnEnemy(4, Skill0SpawnPos.position);
				break;
			case SubStatus.Phase4:
				IgnoreGravity = false;
				break;
			}
			break;
		case MainStatus.Skill1:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_collideBullet.UpdateBulletData(EnemyWeapons[0].BulletData);
				_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
				_collideBullet.Active(targetMask);
				if (NowPos.x > CenterPos.x)
				{
					EndPos.x = MinX;
					UpdateDirection(-1);
				}
				else
				{
					EndPos.x = MaxX;
					UpdateDirection(1);
				}
				break;
			case SubStatus.Phase1:
			{
				ActionTimes = Skill1ActionTimes;
				int num = (int)(Skill1JumpTime * 20f);
				ActionFrame = GameLogicUpdateManager.GameFrame + num;
				_velocity = new VInt3(CalXSpeed(NowPos.x, CenterPos.x, Gravity * num), Math.Abs(Gravity * num), 0);
				break;
			}
			case SubStatus.Phase2:
				_velocity = VInt3.zero;
				IgnoreGravity = true;
				IsInvincible = true;
				break;
			case SubStatus.Phase3:
				UpdateDirection(-base.direction);
				_velocity.x = CalXSpeed(NowPos.x, EndPos.x, Gravity * (int)(Skill1JumpTime * 20f));
				IgnoreGravity = false;
				IsInvincible = false;
				break;
			case SubStatus.Phase4:
			{
				if (NowPos.x < CenterPos.x)
				{
					EndPos.x = MaxX - Controller.Collider2D.size.x / 2f - 0.1f;
				}
				else
				{
					EndPos.x = MinX + Controller.Collider2D.size.x / 2f + 0.1f;
				}
				EndPos.y = GroundYPos + 0.05f;
				SpawnEnemy(5, EndPos);
				AI_STATE aiState = AiState;
				if (aiState == AI_STATE.mob_002 && CanSummon)
				{
					MonoBehaviourSingleton<OrangeBattleUtility>.Instance.CallSummonEnemyEvent(_transform);
				}
				break;
			}
			}
			break;
		case MainStatus.Skill2:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_collideBullet.UpdateBulletData(EnemyWeapons[0].BulletData);
				_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
				_collideBullet.Active(targetMask);
				if (!CloneShadow || CloneCorp == null)
				{
					CloneShadow = TryFindClone();
				}
				if (CloneCorp != null && (bool)CloneShadow && (int)CloneShadow.Hp > 0)
				{
					CloneCorp.SendMission(0);
				}
				ActionTimes = Skill2ActionTimes;
				SetStatus(MainStatus.Skill2, SubStatus.Phase1);
				break;
			case SubStatus.Phase1:
				if (!HasCheckRoomSize)
				{
					CheckRoomSize();
				}
				if (Mathf.Abs(MinX - NowPos.x) > Math.Abs(MaxX - NowPos.x))
				{
					UpdateDirection(-1);
					EndPos.x = MaxX;
				}
				else
				{
					UpdateDirection(1);
					EndPos.x = MinX;
				}
				_velocity.x = RunSpeed * base.direction;
				break;
			case SubStatus.Phase2:
				UpdateDirection(-base.direction);
				break;
			case SubStatus.Phase3:
				_velocity = new VInt3(CalXSpeed(NowPos.x, EndPos.x, JumpSpeed, 2f), JumpSpeed, 0);
				break;
			}
			break;
		case MainStatus.Skill3:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (!CloneShadow || CloneCorp == null)
				{
					CloneShadow = TryFindClone();
				}
				if (CloneCorp != null && (bool)CloneShadow && (int)CloneShadow.Hp > 0)
				{
					CloneCorp.SendMission(1);
				}
				SkillRunTimes = Skill3RunTimes;
				SetStatus(MainStatus.Skill3, SubStatus.Phase1);
				if (Mathf.Abs(MinX - NowPos.x) > Math.Abs(MaxX - NowPos.x))
				{
					UpdateDirection(-1);
				}
				else
				{
					UpdateDirection(1);
				}
				StartDirection = base.direction;
				break;
			case SubStatus.Phase1:
				Playbs038_mushroom03_lp(true);
				_collideBullet.UpdateBulletData(EnemyWeapons[1].BulletData);
				_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
				_collideBullet.Active(targetMask);
				if (Mathf.Abs(MinX - NowPos.x) > Math.Abs(MaxX - NowPos.x))
				{
					UpdateDirection(-1);
				}
				else
				{
					UpdateDirection(1);
				}
				break;
			case SubStatus.Phase2:
				CycleFX.Play();
				ActionFrame = GameLogicUpdateManager.GameFrame + (int)(Skill3RotateTime * 20f);
				break;
			case SubStatus.Phase3:
				ActionTimes = Skill3ActionTimes;
				_velocity.x = Skill3RotateSpeed * base.direction;
				break;
			case SubStatus.Phase4:
				UpdateDirection(-base.direction);
				_velocity.x = Skill3RotateSpeed * base.direction;
				_velocity.y = Skill3JumpSpeed;
				break;
			case SubStatus.Phase5:
				_collideBullet.UpdateBulletData(EnemyWeapons[3].BulletData);
				_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
				_collideBullet.Active(targetMask);
				CycleFX.Stop();
				break;
			case SubStatus.Phase6:
				(BulletBase.TryShotBullet(EnemyWeapons[2].BulletData, NowPos, Vector3.zero, null, selfBuffManager.sBuffStatus, EnemyData, targetMask) as CollideBullet).bNeedBackPoolModelName = true;
				break;
			case SubStatus.Phase7:
				UpdateDirection(StartDirection);
				if (base.direction == 1)
				{
					EndPos.x = MinX;
				}
				else
				{
					EndPos.x = MaxX;
				}
				_velocity = new VInt3(CalXSpeed(NowPos.x, EndPos.x, BackJumpSpeed, 2f), BackJumpSpeed, 0);
				break;
			case SubStatus.Phase9:
				_velocity = VInt3.zero;
				break;
			}
			break;
		case MainStatus.Skill4:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (!CloneShadow || CloneCorp == null)
				{
					CloneShadow = TryFindClone();
				}
				if (CloneCorp != null && (bool)CloneShadow && (int)CloneShadow.Hp > 0)
				{
					CloneCorp.SendMission(2);
				}
				SkillRunTimes = Skill4RunTimes;
				SetStatus(MainStatus.Skill4, SubStatus.Phase1);
				if (Mathf.Abs(MinX - NowPos.x) > Math.Abs(MaxX - NowPos.x))
				{
					UpdateDirection(-1);
				}
				else
				{
					UpdateDirection(1);
				}
				StartDirection = base.direction;
				break;
			case SubStatus.Phase1:
				_collideBullet.UpdateBulletData(EnemyWeapons[1].BulletData);
				_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
				_collideBullet.Active(targetMask);
				if (Mathf.Abs(MinX - NowPos.x) > Math.Abs(MaxX - NowPos.x))
				{
					UpdateDirection(-1);
				}
				else
				{
					UpdateDirection(1);
				}
				break;
			case SubStatus.Phase2:
				CycleFX.Play();
				ActionFrame = GameLogicUpdateManager.GameFrame + (int)(Skill3RotateTime * 20f);
				break;
			case SubStatus.Phase3:
				ActionTimes = Skill3ActionTimes;
				_velocity.x = Skill3RotateSpeed * base.direction;
				break;
			case SubStatus.Phase4:
				UpdateDirection(-base.direction);
				_velocity.x = Skill3RotateSpeed * base.direction;
				_velocity.y = Skill3JumpSpeed;
				break;
			case SubStatus.Phase5:
				Playbs038_mushroom03_lp(false);
				_collideBullet.UpdateBulletData(EnemyWeapons[3].BulletData);
				_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
				_collideBullet.Active(targetMask);
				_velocity = VInt3.zero;
				CycleFX.Stop();
				break;
			case SubStatus.Phase6:
				(BulletBase.TryShotBullet(EnemyWeapons[2].BulletData, NowPos, Vector3.zero, null, selfBuffManager.sBuffStatus, EnemyData, targetMask) as CollideBullet).bNeedBackPoolModelName = true;
				break;
			case SubStatus.Phase7:
				UpdateDirection(StartDirection);
				if (base.direction == 1)
				{
					EndPos.x = MinX;
				}
				else
				{
					EndPos.x = MaxX;
				}
				_velocity = new VInt3(CalXSpeed(NowPos.x, EndPos.x, BackJumpSpeed, 2f), BackJumpSpeed, 0);
				break;
			case SubStatus.Phase9:
				_velocity = VInt3.zero;
				break;
			}
			break;
		case MainStatus.Die:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
			{
				base.AllowAutoAim = false;
				_velocity = VInt3.zero;
				nDeadCount = 0;
				if (!Controller.Collisions.below)
				{
					SetStatus(MainStatus.Die, SubStatus.Phase2);
				}
				AI_STATE aiState = AiState;
				if (aiState == AI_STATE.mob_003)
				{
					base.DeadPlayCompleted = true;
				}
				break;
			}
			case SubStatus.Phase1:
			{
				AI_STATE aiState = AiState;
				if ((uint)(aiState - 1) <= 1u)
				{
					StartCoroutine(BossDieFlow(GetTargetPoint(), "FX_BOSS_EXPLODE2", false, false));
				}
				else
				{
					StartCoroutine(BossDieFlow(_transform.position));
				}
				break;
			}
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
				_currentAnimationId = AnimationID.ANI_SKILL0_START1;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_SKILL0_LOOP1;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_SKILL1_LOOP;
				break;
			case SubStatus.Phase3:
				_currentAnimationId = AnimationID.ANI_SKILL0_LOOP3;
				break;
			case SubStatus.Phase4:
				_currentAnimationId = AnimationID.ANI_SKILL0_END3;
				break;
			}
			break;
		case MainStatus.Skill2:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				return;
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
				_currentAnimationId = AnimationID.ANI_SKILL2_LOOP3;
				break;
			case SubStatus.Phase5:
				_currentAnimationId = AnimationID.ANI_SKILL2_END3;
				break;
			}
			break;
		case MainStatus.Skill3:
		case MainStatus.Skill4:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				return;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_SKILL3_START1;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_SKILL3_LOOP1;
				break;
			case SubStatus.Phase3:
			case SubStatus.Phase4:
				return;
			case SubStatus.Phase5:
				_currentAnimationId = AnimationID.ANI_SKILL3_LOOP2;
				break;
			case SubStatus.Phase6:
				_currentAnimationId = AnimationID.ANI_SKILL3_END2;
				break;
			case SubStatus.Phase7:
				_currentAnimationId = AnimationID.ANI_SKILL0_START1;
				break;
			case SubStatus.Phase8:
				_currentAnimationId = AnimationID.ANI_SKILL2_LOOP3;
				break;
			case SubStatus.Phase9:
				_currentAnimationId = AnimationID.ANI_SKILL2_END3;
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
				if (!(CloneShadow != null) || (int)CloneShadow.Hp <= 0)
				{
					mainStatus = ((!HasUseSkill0) ? MainStatus.Skill0 : MainStatus.Skill1);
				}
				else if (CloneCorp.CheckMissionProgress())
				{
					HasUseSkill0 = false;
					mainStatus = (MainStatus)RandomCard(2);
				}
				else
				{
					mainStatus = MainStatus.Idle;
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
			UploadEnemyStatus((int)mainStatus, false, new object[1] { SpawnCount });
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
					SetColliderEnable(true);
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
				if (!HasActed && _currentFrame > ActionAnimatorFrame)
				{
					if (!Target)
					{
						EndPos = GetTargetPos();
					}
					if ((bool)Target)
					{
						UpdateDirection();
						if (base.direction == 1)
						{
							EndPos.x = MaxX;
						}
						else
						{
							EndPos.x = MinX;
						}
					}
					else if (Mathf.Abs(EndPos.x - NowPos.x) > Math.Abs(MaxX - NowPos.x))
					{
						UpdateDirection(-1);
						EndPos.x = MinX;
					}
					else
					{
						UpdateDirection(1);
						EndPos.x = MaxX;
					}
					_velocity = new VInt3(CalXSpeed(NowPos.x, EndPos.x, Skill0JumpSpeed, 1.6f), Skill0JumpSpeed, 0);
					HasActed = true;
				}
				if (_currentFrame > 1f)
				{
					PlaySE("BossSE04", "bs038_mushroom01");
					SetStatus(MainStatus.Skill0, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if ((base.direction == 1 && Controller.Collisions.right) || (base.direction == -1 && Controller.Collisions.left))
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
				if (_currentFrame > 1f)
				{
					if (--ActionTimes > 0)
					{
						SetStatus(MainStatus.Skill0, SubStatus.Phase3);
					}
					else
					{
						SetStatus(MainStatus.Skill0, SubStatus.Phase4);
					}
				}
				break;
			case SubStatus.Phase4:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill0, SubStatus.Phase5);
				}
				break;
			case SubStatus.Phase5:
				if (Controller.Collisions.below)
				{
					SetStatus(MainStatus.Skill0, SubStatus.Phase6);
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
				if (_currentFrame > 1f)
				{
					PlaySE("BossSE04", "bs038_mushroom01");
					SetStatus(MainStatus.Skill1, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if (GameLogicUpdateManager.GameFrame >= ActionFrame && _velocity.y <= 10)
				{
					MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(MistFXName, CenterPos + Vector3.down * 1f, Quaternion.identity, new object[1] { Vector3.one });
					SetStatus(MainStatus.Skill1, SubStatus.Phase2);
					Playbs038_mushroom03_lp(true);
				}
				break;
			case SubStatus.Phase2:
				if (_currentFrame > 1f)
				{
					if (--ActionTimes > 0)
					{
						SetStatus(MainStatus.Skill1, SubStatus.Phase2);
						break;
					}
					Playbs038_mushroom03_lp(false);
					SetStatus(MainStatus.Skill1, SubStatus.Phase3);
				}
				break;
			case SubStatus.Phase3:
				if (Controller.Collisions.below)
				{
					SetStatus(MainStatus.Skill1, SubStatus.Phase4);
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
		case MainStatus.Skill2:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				SetStatus(MainStatus.Skill2, SubStatus.Phase1);
				break;
			case SubStatus.Phase1:
				if ((base.direction == 1 && Controller.Collisions.right) || (base.direction == -1 && Controller.Collisions.left))
				{
					SetStatus(MainStatus.Skill2, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				if (_currentFrame > 1f)
				{
					PlaySE("BossSE04", "bs038_mushroom01");
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
				if (Controller.Collisions.below)
				{
					SetStatus(MainStatus.Skill2, SubStatus.Phase5);
				}
				break;
			case SubStatus.Phase5:
				if (_currentFrame > 1f)
				{
					if (--ActionTimes > 0)
					{
						SetStatus(MainStatus.Skill2, SubStatus.Phase1);
					}
					else
					{
						SetStatus(MainStatus.Idle);
					}
				}
				break;
			}
			break;
		case MainStatus.Skill3:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				SetStatus(MainStatus.Skill3, SubStatus.Phase1);
				break;
			case SubStatus.Phase1:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill3, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				if (GameLogicUpdateManager.GameFrame >= ActionFrame)
				{
					SetStatus(MainStatus.Skill3, SubStatus.Phase3);
				}
				break;
			case SubStatus.Phase3:
				if ((base.direction == 1 && Controller.Collisions.right) || (base.direction == -1 && Controller.Collisions.left))
				{
					PlaySE("BossSE04", "bs038_mushroom04");
					CameraShake();
					SetStatus(MainStatus.Skill3, SubStatus.Phase4);
				}
				break;
			case SubStatus.Phase4:
				if ((base.direction == 1 && Controller.Collisions.right) || (base.direction == -1 && Controller.Collisions.left))
				{
					PlaySE("BossSE04", "bs038_mushroom04");
					CameraShake();
					if (--ActionTimes > 0)
					{
						SetStatus(MainStatus.Skill3, SubStatus.Phase4);
						break;
					}
					SetStatus(MainStatus.Skill3, SubStatus.Phase5);
					Playbs038_mushroom03_lp(false);
				}
				break;
			case SubStatus.Phase5:
				if (Controller.Collisions.below)
				{
					CameraShake();
					SetStatus(MainStatus.Skill3, SubStatus.Phase6);
				}
				break;
			case SubStatus.Phase6:
				if (_currentFrame > 1f)
				{
					if (--SkillRunTimes > 0)
					{
						SetStatus(MainStatus.Skill3, SubStatus.Phase1);
					}
					else
					{
						SetStatus(MainStatus.Idle);
					}
				}
				break;
			case SubStatus.Phase7:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill3, SubStatus.Phase8);
				}
				break;
			case SubStatus.Phase8:
				if (Controller.Collisions.below)
				{
					SetStatus(MainStatus.Skill3, SubStatus.Phase9);
				}
				break;
			case SubStatus.Phase9:
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
				SetStatus(MainStatus.Skill4, SubStatus.Phase1);
				break;
			case SubStatus.Phase1:
				if (_currentFrame > 1f)
				{
					Playbs038_mushroom03_lp(true);
					SetStatus(MainStatus.Skill4, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				if (GameLogicUpdateManager.GameFrame >= ActionFrame)
				{
					SetStatus(MainStatus.Skill4, SubStatus.Phase3);
				}
				break;
			case SubStatus.Phase3:
				if ((base.direction == 1 && Controller.Collisions.right) || (base.direction == -1 && Controller.Collisions.left))
				{
					PlaySE("BossSE04", "bs038_mushroom04");
					CameraShake();
					SetStatus(MainStatus.Skill4, SubStatus.Phase4);
				}
				break;
			case SubStatus.Phase4:
				if (ActionTimes == 1)
				{
					if (!Target)
					{
						Target = _enemyAutoAimSystem.GetClosetPlayer();
					}
					if ((bool)Target && NowPos.y > Target._transform.position.y - 1f && Math.Abs(NowPos.x - Target._transform.position.x) < 0.5f)
					{
						SetStatus(MainStatus.Skill4, SubStatus.Phase5);
					}
				}
				if ((base.direction == 1 && Controller.Collisions.right) || (base.direction == -1 && Controller.Collisions.left))
				{
					PlaySE("BossSE04", "bs038_mushroom04");
					CameraShake();
					if (--ActionTimes > 0)
					{
						SetStatus(MainStatus.Skill4, SubStatus.Phase4);
					}
					else
					{
						SetStatus(MainStatus.Skill4, SubStatus.Phase5);
					}
				}
				break;
			case SubStatus.Phase5:
				if (Controller.Collisions.below)
				{
					CameraShake();
					SetStatus(MainStatus.Skill4, SubStatus.Phase6);
				}
				break;
			case SubStatus.Phase6:
				if (_currentFrame > 1f)
				{
					if (--SkillRunTimes > 0)
					{
						SetStatus(MainStatus.Skill4, SubStatus.Phase1);
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
				if (Controller.Collisions.below)
				{
					SetStatus(MainStatus.Skill4, SubStatus.Phase9);
				}
				break;
			case SubStatus.Phase9:
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
			ModelTransform.localScale = new Vector3(0.8f, 0.75f, 0.8f * (float)base.direction);
			_collideBullet.UpdateBulletData(EnemyWeapons[0].BulletData);
			_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			_collideBullet.Active(targetMask);
			HasCheckRoomSize = false;
			CheckRoomSize();
			HasUseSkill0 = false;
			SetColliderEnable(false);
			SetStatus(MainStatus.Debut);
		}
		else
		{
			_collideBullet.BackToPool();
		}
	}

	public override void UpdateEnemyID(int id)
	{
		base.UpdateEnemyID(id);
		UpdateAIState();
		SetMaxGravity(OrangeBattleUtility.FP_MaxGravity * 2);
		AI_STATE aiState = AiState;
		if (aiState == AI_STATE.mob_002)
		{
			CanSummon = true;
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
		CanSummon = false;
		for (int i = 0; i < StageUpdate.runEnemys.Count; i++)
		{
			EM176_Controller eM176_Controller = StageUpdate.runEnemys[i].mEnemy as EM176_Controller;
			if ((bool)eM176_Controller)
			{
				eM176_Controller.SetDead();
			}
		}
		StageUpdate.SlowStage();
		SetColliderEnable(false);
		SetStatus(MainStatus.Die);
	}

	private void CheckRoomSize()
	{
		int layerMask = (1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockLayer) | (1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockEnemyLayer) | (1 << ManagedSingleton<OrangeLayerManager>.Instance.WallKickMask);
		Vector3 vector = new Vector3(NowPos.x, NowPos.y + 5f, 0f);
		RaycastHit2D raycastHit2D = OrangeBattleUtility.RaycastIgnoreSelf(vector, Vector3.left, 30f, layerMask, _transform);
		RaycastHit2D raycastHit2D2 = OrangeBattleUtility.RaycastIgnoreSelf(vector, Vector3.right, 30f, layerMask, _transform);
		RaycastHit2D raycastHit2D3 = OrangeBattleUtility.RaycastIgnoreSelf(vector, Vector3.up, 30f, layerMask, _transform);
		RaycastHit2D raycastHit2D4 = OrangeBattleUtility.RaycastIgnoreSelf(vector, Vector3.down, 30f, layerMask, _transform);
		if (!raycastHit2D4)
		{
			Debug.LogError("沒有偵測到地板，之後一些技能無法準確判斷位置");
			return;
		}
		GroundYPos = raycastHit2D4.point.y;
		float num = 0f;
		Vector3 vector2 = new Vector3(y: (!raycastHit2D3) ? (GroundYPos + 12f) : raycastHit2D3.point.y, x: raycastHit2D2.point.x, z: 0f);
		Vector3 vector3 = new Vector3(raycastHit2D.point.x, GroundYPos, 0f);
		MinX = vector3.x;
		MaxX = vector2.x;
		CenterPos = (vector2 + vector3) / 2f;
		HasCheckRoomSize = true;
	}

	private int CalXSpeed(float startx, float endx, int jumpspd, float timeoffset = 1f)
	{
		int num = (int)((float)Math.Abs(jumpspd / Gravity) * timeoffset);
		return (int)((endx - startx) * 1000f * 20f / (float)num);
	}

	private EM176_Controller SpawnEnemy(int weapon, Vector3 spawnpos)
	{
		MOB_TABLE enemy = GetEnemy((int)EnemyWeapons[weapon].BulletData.f_EFFECT_X);
		if (enemy == null)
		{
			Debug.LogError("要生成的怪物資料有誤，生怪技能ID " + weapon + " 怪物GroupID " + EnemyWeapons[weapon].BulletData.f_EFFECT_X);
			return null;
		}
		EnemyControllerBase enemyControllerBase = StageUpdate.StageSpawnEnemyByMob(enemy, sNetSerialID + SpawnCount);
		SpawnCount++;
		if ((bool)enemyControllerBase)
		{
			if (weapon == 4)
			{
				enemyControllerBase.SetPositionAndRotation(spawnpos, base.direction == -1);
			}
			else
			{
				enemyControllerBase.SetPositionAndRotation(spawnpos, base.direction != -1);
			}
			EM176_Controller eM176_Controller = enemyControllerBase as EM176_Controller;
			if ((bool)eM176_Controller)
			{
				RegisterClone(eM176_Controller);
			}
			enemyControllerBase.SetActive(true);
			return eM176_Controller;
		}
		return null;
	}

	private MOB_TABLE GetEnemy(int nGroupID)
	{
		MOB_TABLE[] mobArrayFromGroup = ManagedSingleton<OrangeTableHelper>.Instance.GetMobArrayFromGroup(nGroupID);
		if (mobArrayFromGroup.Length != 0)
		{
			for (int i = 0; i < mobArrayFromGroup.Length; i++)
			{
				if (mobArrayFromGroup[i].n_DIFFICULTY == StageUpdate.gDifficulty)
				{
					return mobArrayFromGroup[i];
				}
			}
		}
		MOB_TABLE mob = ManagedSingleton<OrangeTableHelper>.Instance.GetMob(nGroupID);
		if (mob != null)
		{
			return mob;
		}
		return null;
	}

	private void CameraShake()
	{
		Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.CAMERA_SHAKE, 2f, false);
	}

	public void ReportObjects(object[] value)
	{
	}

	public object[] GetValues(object[] param = null)
	{
		return null;
	}

	public void RegisterClone(EM176_Controller em176)
	{
		if ((int)em176.Hp > 0)
		{
			CloneShadow = em176;
			CloneCorp = new BossCorpsTool();
			CloneCorp.CorpsBackCallback = CloneDead;
			CloneShadow.JoinCorps(CloneCorp);
		}
	}

	private EM176_Controller TryFindClone()
	{
		for (int i = 0; i < StageUpdate.runEnemys.Count; i++)
		{
			EM176_Controller eM176_Controller = StageUpdate.runEnemys[i].mEnemy as EM176_Controller;
			if ((bool)eM176_Controller)
			{
				CloneCorp = new BossCorpsTool(eM176_Controller);
				CloneCorp.CorpsBackCallback = CloneDead;
				eM176_Controller.JoinCorps(CloneCorp);
				return eM176_Controller;
			}
		}
		CloneCorp = null;
		SpawnCount--;
		if (NowPos.x < CenterPos.x)
		{
			EndPos.x = MaxX - Controller.Collider2D.size.x / 2f - 0.1f;
		}
		else
		{
			EndPos.x = MinX + Controller.Collider2D.size.x / 2f + 0.1f;
		}
		EndPos.y = GroundYPos + 0.05f;
		EM176_Controller eM176_Controller2 = SpawnEnemy(5, EndPos);
		if ((bool)eM176_Controller2)
		{
			return eM176_Controller2;
		}
		Debug.Log("生成分身失敗，收到的 SpawnCount 是 " + SpawnCount);
		return null;
	}

	private void CloneDead(object obj)
	{
		CloneCorp = null;
		CloneShadow = null;
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
}
