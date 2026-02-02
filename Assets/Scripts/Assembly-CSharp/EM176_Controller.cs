#define RELEASE
using System;
using CallbackDefs;
using NaughtyAttributes;
using Newtonsoft.Json;
using StageLib;
using UnityEngine;

public class EM176_Controller : EnemyControllerBase, IManagedUpdateBehavior
{
	private enum MainStatus
	{
		Idle = 0,
		Skill0 = 1,
		Skill1 = 2,
		Skill2 = 3,
		Die = 4
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

	private Vector3 LastTargetPos = new Vector3(0f, 0f, 0f);

	[SerializeField]
	private float IdleWaitTime = 0.2f;

	private int WaitFrame;

	[Header("通用")]
	private int ActionTimes;

	private int SkillRunTimes;

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

	[SerializeField]
	private int ToDeadCount = 3;

	private int DeadCount;

	[Header("跑跑跳跳")]
	[SerializeField]
	private int Skill2ActionTimes = 3;

	[Header("倒立毒霧")]
	[SerializeField]
	private float Skill3WaitTime = 1f;

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
	private float Skill4WaitTime = 1f;

	[SerializeField]
	private int Skill4RunTimes = 4;

	private BossCorpsTool CorpsTool;

	private MainStatus NextSkill;

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
		base.AimTransform = ModelTransform;
		base.AimPoint = new Vector3(0f, 1f, 0f);
		OrangeBattleUtility.AddEnemyAutoAimSystem(_transform, out _enemyAutoAimSystem);
		_enemyAutoAimSystem.UpdateAimRange(20f);
		FallDownSE = new string[2] { "BossSE04", "bs038_mushroom02" };
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
			if (netSyncData.nParam0 != 0)
			{
				DeadCount = netSyncData.nParam0;
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
		case MainStatus.Idle:
			_velocity.x = 0;
			WaitFrame = GameLogicUpdateManager.GameFrame + (int)(IdleWaitTime * 20f);
			if (DeadCount >= ToDeadCount)
			{
				SetDead();
			}
			break;
		case MainStatus.Skill0:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_collideBullet.UpdateBulletData(EnemyWeapons[0].BulletData);
				_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
				_collideBullet.Active(targetMask);
				ActionTimes = Skill2ActionTimes;
				SetStatus(MainStatus.Skill0, SubStatus.Phase1);
				break;
			case SubStatus.Phase1:
				if (!HasCheckRoomSize)
				{
					CheckRoomSize();
				}
				if (Mathf.Abs(MinX - NowPos.x) > Math.Abs(MaxX - NowPos.x))
				{
					UpdateDirection(-1);
					EndPos.x = MinX;
				}
				else
				{
					UpdateDirection(1);
					EndPos.x = MaxX;
				}
				PlaySE("BossSE04", "bs038_mushroom01");
				break;
			case SubStatus.Phase2:
				_velocity = new VInt3(CalXSpeed(NowPos.x, EndPos.x, JumpSpeed, 2f), JumpSpeed, 0);
				break;
			case SubStatus.Phase5:
				UpdateDirection(-base.direction);
				_velocity.x = RunSpeed * base.direction;
				break;
			}
			break;
		case MainStatus.Skill1:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				SkillRunTimes = Skill3RunTimes;
				if (Mathf.Abs(MinX - NowPos.x) > Math.Abs(MaxX - NowPos.x))
				{
					UpdateDirection(-1);
				}
				else
				{
					UpdateDirection(1);
				}
				StartDirection = base.direction;
				WaitFrame = GameLogicUpdateManager.GameFrame + (int)(Skill3WaitTime * 20f);
				SetStatus(MainStatus.Skill1, SubStatus.Phase1);
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
				ActionFrame = GameLogicUpdateManager.GameFrame + (int)(Skill3RotateTime * 20f) + (int)(Skill3WaitTime * 20f);
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
				Playbs038_mushroom03_lp(false);
				CycleFX.Stop();
				break;
			case SubStatus.Phase6:
				(BulletBase.TryShotBullet(EnemyWeapons[2].BulletData, NowPos, Vector3.zero, null, selfBuffManager.sBuffStatus, EnemyData, targetMask) as CollideBullet).bNeedBackPoolModelName = true;
				break;
			case SubStatus.Phase7:
				PlaySE("BossSE04", "bs038_mushroom01");
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
		case MainStatus.Skill2:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				SkillRunTimes = Skill4RunTimes;
				if (Mathf.Abs(MinX - NowPos.x) > Math.Abs(MaxX - NowPos.x))
				{
					UpdateDirection(-1);
				}
				else
				{
					UpdateDirection(1);
				}
				StartDirection = base.direction;
				WaitFrame = GameLogicUpdateManager.GameFrame + (int)(Skill4WaitTime * 20f);
				SetStatus(MainStatus.Skill2, SubStatus.Phase1);
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
				ActionFrame = GameLogicUpdateManager.GameFrame + (int)(Skill3RotateTime * 20f) + (int)(Skill4WaitTime * 20f);
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
				_velocity = VInt3.zero;
				Playbs038_mushroom03_lp(false);
				CycleFX.Stop();
				break;
			case SubStatus.Phase6:
				(BulletBase.TryShotBullet(EnemyWeapons[2].BulletData, NowPos, Vector3.zero, null, selfBuffManager.sBuffStatus, EnemyData, targetMask) as CollideBullet).bNeedBackPoolModelName = true;
				break;
			case SubStatus.Phase7:
				PlaySE("BossSE04", "bs038_mushroom01");
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
		case MainStatus.Idle:
			_currentAnimationId = AnimationID.ANI_IDLE;
			break;
		case MainStatus.Skill0:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				return;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_SKILL2_START2;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_SKILL2_LOOP2;
				break;
			case SubStatus.Phase3:
				_currentAnimationId = AnimationID.ANI_SKILL2_LOOP3;
				break;
			case SubStatus.Phase4:
				_currentAnimationId = AnimationID.ANI_SKILL2_END3;
				break;
			case SubStatus.Phase5:
				_currentAnimationId = AnimationID.ANI_SKILL2_LOOP1;
				break;
			}
			break;
		case MainStatus.Skill1:
		case MainStatus.Skill2:
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
			if (_subStatus == SubStatus.Phase0)
			{
				_currentAnimationId = AnimationID.ANI_HURT;
				break;
			}
			return;
		}
		_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
	}

	private void UpdateNextState(int Step = -1, bool needSync = true)
	{
		if (needSync && !bWaitNetStatus)
		{
			int mission = -1;
			if (!CorpsTool.MissionComplete())
			{
				mission = CorpsTool.ReceiveMission();
			}
			SetMission(mission);
			UploadStatus(NextSkill);
		}
	}

	public bool SetMission(int mission = -1)
	{
		switch (mission)
		{
		case -1:
			NextSkill = MainStatus.Idle;
			return true;
		case 0:
			NextSkill = MainStatus.Skill0;
			return true;
		case 1:
			NextSkill = MainStatus.Skill1;
			return true;
		case 2:
			NextSkill = MainStatus.Skill2;
			return true;
		default:
			NextSkill = MainStatus.Idle;
			return true;
		}
	}

	private void UploadStatus(MainStatus status)
	{
		if (status != 0)
		{
			DeadCount++;
			if (CheckHost())
			{
				UploadEnemyStatus((int)status, false, new object[1] { DeadCount });
			}
		}
		else
		{
			SetStatus(MainStatus.Idle);
		}
	}

	public override void LogicUpdate()
	{
		if (!Activate)
		{
			return;
		}
		base.LogicUpdate();
		_currentFrame = _animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
		switch (_mainStatus)
		{
		case MainStatus.Idle:
			if (bWaitNetStatus || WaitFrame >= GameLogicUpdateManager.GameFrame)
			{
				break;
			}
			Target = _enemyAutoAimSystem.GetClosetPlayer();
			if ((bool)Target)
			{
				UpdateDirection();
				if (!bWaitNetStatus && !CorpsTool.MissionComplete())
				{
					UpdateNextState();
				}
			}
			break;
		case MainStatus.Skill0:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				SetStatus(MainStatus.Skill0, SubStatus.Phase1);
				break;
			case SubStatus.Phase1:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill0, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				if (_velocity.y <= 0)
				{
					SetStatus(MainStatus.Skill0, SubStatus.Phase3);
				}
				break;
			case SubStatus.Phase3:
				if (Controller.Collisions.below)
				{
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
				if ((base.direction == 1 && Controller.Collisions.right) || (base.direction == -1 && Controller.Collisions.left))
				{
					if (--ActionTimes > 0)
					{
						SetStatus(MainStatus.Skill0, SubStatus.Phase1);
					}
					else
					{
						SetStatus(MainStatus.Idle);
					}
				}
				break;
			}
			break;
		case MainStatus.Skill1:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (GameLogicUpdateManager.GameFrame >= WaitFrame)
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
				if (GameLogicUpdateManager.GameFrame >= ActionFrame)
				{
					SetStatus(MainStatus.Skill1, SubStatus.Phase3);
				}
				break;
			case SubStatus.Phase3:
				if ((base.direction == 1 && Controller.Collisions.right) || (base.direction == -1 && Controller.Collisions.left))
				{
					CameraShake();
					SetStatus(MainStatus.Skill1, SubStatus.Phase4);
				}
				break;
			case SubStatus.Phase4:
				if ((base.direction == 1 && Controller.Collisions.right) || (base.direction == -1 && Controller.Collisions.left))
				{
					CameraShake();
					if (--ActionTimes > 0)
					{
						SetStatus(MainStatus.Skill1, SubStatus.Phase4);
					}
					else
					{
						SetStatus(MainStatus.Skill1, SubStatus.Phase5);
					}
				}
				break;
			case SubStatus.Phase5:
				if (Controller.Collisions.below)
				{
					CameraShake();
					SetStatus(MainStatus.Skill1, SubStatus.Phase6);
				}
				break;
			case SubStatus.Phase6:
				if (_currentFrame > 1f)
				{
					if (--SkillRunTimes > 0)
					{
						SetStatus(MainStatus.Skill1, SubStatus.Phase1);
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
					SetStatus(MainStatus.Skill1, SubStatus.Phase8);
				}
				break;
			case SubStatus.Phase8:
				if (Controller.Collisions.below)
				{
					SetStatus(MainStatus.Skill1, SubStatus.Phase9);
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
		case MainStatus.Skill2:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (GameLogicUpdateManager.GameFrame >= WaitFrame)
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
				if (GameLogicUpdateManager.GameFrame >= ActionFrame)
				{
					SetStatus(MainStatus.Skill2, SubStatus.Phase3);
				}
				break;
			case SubStatus.Phase3:
				if ((base.direction == 1 && Controller.Collisions.right) || (base.direction == -1 && Controller.Collisions.left))
				{
					CameraShake();
					SetStatus(MainStatus.Skill2, SubStatus.Phase4);
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
						SetStatus(MainStatus.Skill2, SubStatus.Phase5);
					}
				}
				if ((base.direction == 1 && Controller.Collisions.right) || (base.direction == -1 && Controller.Collisions.left))
				{
					CameraShake();
					if (--ActionTimes > 0)
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
				if (Controller.Collisions.below)
				{
					CameraShake();
					SetStatus(MainStatus.Skill2, SubStatus.Phase6);
				}
				break;
			case SubStatus.Phase6:
				if (_currentFrame > 1f)
				{
					if (--SkillRunTimes > 0)
					{
						SetStatus(MainStatus.Skill2, SubStatus.Phase1);
					}
					else
					{
						SetStatus(MainStatus.Skill2, SubStatus.Phase7);
					}
				}
				break;
			case SubStatus.Phase7:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill2, SubStatus.Phase8);
				}
				break;
			case SubStatus.Phase8:
				if (Controller.Collisions.below)
				{
					SetStatus(MainStatus.Skill2, SubStatus.Phase9);
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
			if (_subStatus != 0)
			{
				int num = 1;
			}
			break;
		}
	}

	public void UpdateFunc()
	{
		if (Activate)
		{
			base.transform.localPosition = Vector3.MoveTowards(base.transform.localPosition, Controller.LogicPosition.vec3, distanceDelta);
		}
	}

	public override void SetActive(bool isActive)
	{
		base.SetActive(isActive);
		SetColliderEnable(false);
		if (isActive)
		{
			DeadCount = 0;
			base.AllowAutoAim = true;
			ModelTransform.localScale = new Vector3(0.8f, 0.75f, 0.8f * (float)base.direction);
			_collideBullet.UpdateBulletData(EnemyWeapons[0].BulletData);
			_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			_collideBullet.Active(targetMask);
			HasCheckRoomSize = false;
			CheckRoomSize();
			if (CorpsTool == null)
			{
				FindBossAndJoin();
			}
			PlaySE("BossSE04", "bs038_mushroom05");
			SetStatus(MainStatus.Idle);
		}
		else
		{
			CorpsTool = null;
			_collideBullet.BackToPool();
		}
	}

	public override void UpdateEnemyID(int id)
	{
		base.UpdateEnemyID(id);
		UpdateAIState();
		SetMaxGravity(OrangeBattleUtility.FP_MaxGravity * 2);
		AI_STATE aiState = AiState;
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

	protected override void DeadBehavior(ref HurtPassParam tHurtPassParam)
	{
		base.DeadBehavior(ref tHurtPassParam);
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

	private void CameraShake()
	{
		Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.CAMERA_SHAKE, 2f, false);
	}

	public void JoinCorps(BossCorpsTool corps)
	{
		CorpsTool = corps;
	}

	private void FindBossAndJoin()
	{
		for (int i = 0; i < StageUpdate.runEnemys.Count; i++)
		{
			BS061_Controller bS061_Controller = StageUpdate.runEnemys[i].mEnemy as BS061_Controller;
			if ((bool)bS061_Controller)
			{
				bS061_Controller.RegisterClone(this);
				return;
			}
		}
		Debug.LogError("分身是被同步生出來的，且註冊Boss失敗");
	}

	public override void BackToPool()
	{
		CorpsTool.CorpsBackCallback(this);
		base.BackToPool();
	}

	public void SetDead()
	{
		HurtPassParam hurtPassParam = new HurtPassParam();
		hurtPassParam.dmg = MaxHp;
		Hurt(hurtPassParam);
	}
}
