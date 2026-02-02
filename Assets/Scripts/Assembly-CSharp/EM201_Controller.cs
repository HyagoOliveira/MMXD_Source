#define RELEASE
using System;
using CallbackDefs;
using Newtonsoft.Json;
using StageLib;
using UnityEngine;

public class EM201_Controller : EnemyControllerBase, IManagedUpdateBehavior
{
	private enum MainStatus
	{
		Idle = 0,
		Debut = 1,
		FindPlayer = 2,
		Jump = 3,
		Skill0 = 4,
		Skill1 = 5,
		Skill2 = 6,
		Skill3 = 7,
		Skill4 = 8,
		Skill5 = 9,
		Skill6 = 10,
		NextAction = 11,
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
		MAX_SUBSTATUS = 6
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
		ANI_IDLE_LOOP = 0,
		ANI_DEBUT = 1,
		ANI_HURT_LOOP = 2,
		ANI_JUMP_START = 3,
		ANI_JUMP_LOOP = 4,
		ANI_JUMP_TO_FALL = 5,
		ANI_FALL_LOOP = 6,
		ANI_LANDING = 7,
		ANI_SKILL_01_PREPARE_START = 8,
		ANI_SKILL_01_PREPARE_LOOP = 9,
		ANI_SKILL_01_ATK1_START = 10,
		ANI_SKILL_01_ATK1_LOOP = 11,
		ANI_SKILL_01_ATK2_START = 12,
		ANI_SKILL_01_ATK2_LOOP = 13,
		ANI_SKILL_01_END = 14,
		ANI_SKILL_02_START = 15,
		ANI_SKILL_02_END = 16,
		ANI_SKILL_03_JUMP_START = 17,
		ANI_SKILL_03_JUMP_LOOP = 18,
		ANI_SKILL_03_JUMP_ATK_START = 19,
		ANI_SKILL_03_JUMP_ATK_LOOP = 20,
		ANI_SKILL_03_JUMP_ATK_END = 21,
		ANI_SKILL_03_FALL_LOOP = 22,
		ANI_SKILL_04_PREPARE_START = 23,
		ANI_SKILL_04_PREPARE_LOOP = 24,
		ANI_SKILL_04_ATK_START = 25,
		ANI_SKILL_04_ATK_LOOP = 26,
		ANI_SKILL_04_ATK_END = 27,
		ANI_SKILL5_START1 = 28,
		ANI_SKILL5_LOOP1 = 29,
		ANI_SKILL5_START2 = 30,
		ANI_SKILL5_LOOP2 = 31,
		ANI_SKILL5_END = 32,
		MAX_ANIMATION_ID = 33
	}

	[SerializeField]
	private MainStatus _mainStatus;

	[SerializeField]
	private SubStatus _subStatus;

	[SerializeField]
	private AnimationID _currentAnimationId;

	[SerializeField]
	private float _currentFrame;

	private int[] _animationHash;

	private Transform _tfModelMelt;

	private Transform _tfModelSolid;

	private Transform _tfSkill01ShootPoint;

	private CollideBullet _skill02collideBullet;

	private ParticleSystem _skill02ParticleSystem;

	private ParticleSystem _skill02PPS;

	private ParticleSystem _skill03ParticleSystem;

	private int _defJumpSpeedY = 18;

	private MainStatus _lateAction;

	private SeaHorseSkill01Bullet _skill01Bullet;

	private SeaHorseSkill04Bullet _skill04BulletA;

	private SeaHorseSkill04Bullet _skill04BulletB;

	private int _diveTime;

	private float _rushDiveSpeed = 18f;

	[SerializeField]
	private float _rushDiveSpeedStart = 18f;

	[SerializeField]
	private float _rushDiveSpeedPlus = 4f;

	private Vector3 _rushDiveDir;

	private Vector3 _rushDiveJumpPoint;

	private bool _checkVenomTime;

	private int _skill01Count;

	[SerializeField]
	private MainStatus _deubgNextAI = MainStatus.Debut;

	[SerializeField]
	private int _debugDistanceX;

	private OrangeTimer _summonTimer;

	private bool _bDeadCallResult = true;

	[Header("通用")]
	private Vector3 StartPos;

	private Vector3 EndPos;

	private Vector3 MaxPos;

	private Vector3 MinPos;

	private Vector3 CenterPos;

	private float GroundYPos;

	private int ActionTimes;

	private float ActionAnimatorFrame;

	private int ActionFrame;

	private bool HasActed;

	[Header("跳躍")]
	[SerializeField]
	private float Skill4JumpTime = 0.5f;

	private int Gravity = OrangeBattleUtility.FP_Gravity * GameLogicUpdateManager.g_fixFrameLenFP / 1000;

	[Header("噴灑")]
	[SerializeField]
	private int Skill5AtkTime = 1;

	private CollideBullet Skill5CollideBullet;

	[SerializeField]
	private string Skill5BulletObj = "Skill5Collider";

	[SerializeField]
	private ParticleSystem Skill5Fx;

	[Header("AI控制")]
	public bool isActing;

	public bool isSkill1ING;

	public bool isNeedSkill5;

	public bool isSkill5ING;

	public bool FarEndPos;

	public bool UseDoubleShot;

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

	private void OnEnable()
	{
		MonoBehaviourSingleton<UpdateManager>.Instance.AddUpdate(this);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		MonoBehaviourSingleton<UpdateManager>.Instance.RemoveUpdate(this);
	}

	protected override void Awake()
	{
		base.Awake();
		Transform[] target = _transform.GetComponentsInChildren<Transform>(true);
		ModelTransform = OrangeBattleUtility.FindChildRecursive(ref target, "model", true);
		_animator = GetComponentInChildren<Animator>();
		_collideBullet = OrangeBattleUtility.FindChildRecursive(ref target, "BodyCollider").gameObject.AddOrGetComponent<CollideBullet>();
		_skill02collideBullet = OrangeBattleUtility.FindChildRecursive(ref target, "fxuse_seahorse_002").gameObject.AddOrGetComponent<CollideBullet>();
		_skill02ParticleSystem = OrangeBattleUtility.FindChildRecursive(ref target, "fxuse_seahorse_002_(work)", true).GetComponent<ParticleSystem>();
		_skill03ParticleSystem = OrangeBattleUtility.FindChildRecursive(ref target, "fxduring_seahorse_roll", true).GetComponent<ParticleSystem>();
		_skill02ParticleSystem.Stop();
		_skill02PPS = OrangeBattleUtility.FindChildRecursive(ref target, "PS_Explosion", true).GetComponent<ParticleSystem>();
		_skill03ParticleSystem.Stop();
		Skill5CollideBullet = OrangeBattleUtility.FindChildRecursive(ref target, Skill5BulletObj).gameObject.AddOrGetComponent<CollideBullet>();
		Skill5Fx = OrangeBattleUtility.FindChildRecursive(ref target, "fxduring_Seahorse_002", true).GetComponent<ParticleSystem>();
		OrangeBattleUtility.AddEnemyAutoAimSystem(_transform, out _enemyAutoAimSystem);
		_enemyAutoAimSystem.UpdateAimRange(20f);
		base.AimPoint = new Vector3(0f, 0.75f, 0f);
		SetMaxGravity(OrangeBattleUtility.FP_MaxGravity * 2);
		_tfModelMelt = OrangeBattleUtility.FindChildRecursive(ref target, "model_melt", true);
		_tfModelSolid = OrangeBattleUtility.FindChildRecursive(ref target, "model_solid", true);
		_tfSkill01ShootPoint = OrangeBattleUtility.FindChildRecursive(ref target, "skill01ShootPoint", true);
		_animationHash = new int[33];
		_animationHash[0] = Animator.StringToHash("BS040@idle_loop");
		_animationHash[1] = Animator.StringToHash("BS040@debut");
		_animationHash[2] = Animator.StringToHash("BS040@hurt_loop");
		_animationHash[3] = Animator.StringToHash("BS040@jump_start");
		_animationHash[4] = Animator.StringToHash("BS040@jump_loop");
		_animationHash[5] = Animator.StringToHash("BS040@jump_to_fall");
		_animationHash[6] = Animator.StringToHash("BS040@fall_loop");
		_animationHash[7] = Animator.StringToHash("BS040@landing");
		_animationHash[8] = Animator.StringToHash("BS040@skill_01_prepare_start");
		_animationHash[9] = Animator.StringToHash("BS040@skill_01_prepare_loop");
		_animationHash[10] = Animator.StringToHash("BS040@skill_01_atk1_start");
		_animationHash[11] = Animator.StringToHash("BS040@skill_01_atk1_loop");
		_animationHash[12] = Animator.StringToHash("BS040@skill_01_atk2_start");
		_animationHash[13] = Animator.StringToHash("BS040@skill_01_atk2_loop");
		_animationHash[14] = Animator.StringToHash("BS040@skill_01_end");
		_animationHash[15] = Animator.StringToHash("BS040@skill_02_start");
		_animationHash[16] = Animator.StringToHash("BS040@skill_02_end");
		_animationHash[17] = Animator.StringToHash("BS040@skill_03_jump_start");
		_animationHash[18] = Animator.StringToHash("BS040@skill_03_jump_loop");
		_animationHash[19] = Animator.StringToHash("BS040@skill_03_jump_atk_start");
		_animationHash[20] = Animator.StringToHash("BS040@skill_03_jump_atk_loop");
		_animationHash[21] = Animator.StringToHash("BS040@skill_03_jump_atk_end");
		_animationHash[22] = Animator.StringToHash("BS040@skill_03_fall_loop");
		_animationHash[23] = Animator.StringToHash("BS040@skill_04_prepare_start");
		_animationHash[24] = Animator.StringToHash("BS040@skill_04_prepare_loop");
		_animationHash[25] = Animator.StringToHash("BS040@skill_04_atk_start");
		_animationHash[26] = Animator.StringToHash("BS040@skill_04_atk_loop");
		_animationHash[27] = Animator.StringToHash("BS040@skill_04_atk_end");
		_animationHash[28] = Animator.StringToHash("BS040@Seahorse_SKILL5_CASTING1");
		_animationHash[29] = Animator.StringToHash("BS040@Seahorse_SKILL5_CASTLOOP1");
		_animationHash[30] = Animator.StringToHash("BS040@Seahorse_SKILL5_CASTING2");
		_animationHash[31] = Animator.StringToHash("BS040@Seahorse_SKILL5_CASTLOOP2");
		_animationHash[32] = Animator.StringToHash("BS040@Seahorse_SKILL5_CASTOUT1");
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxstory_explode_000", 10);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("FX_BOSS_EXPLODE2");
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxhit_seahorse_hit_ground_m", 5);
		_summonTimer = OrangeTimerManager.GetTimer();
		_mainStatus = MainStatus.Idle;
		_subStatus = SubStatus.Phase0;
		AiTimer.TimerStart();
	}

	public override void UpdateStatus(int nSet, string sMsg, Callback tCB = null)
	{
		bWaitNetStatus = false;
		if ((int)Hp <= 0)
		{
			return;
		}
		if (!string.IsNullOrEmpty(sMsg))
		{
			NetSyncData netSyncData = JsonConvert.DeserializeObject<NetSyncData>(sMsg);
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
		UpdateDirection();
		SetStatus((MainStatus)nSet);
	}

	private void UpdateRandomState()
	{
		MainStatus mainStatus = MainStatus.Idle;
		if (StageUpdate.bIsHost)
		{
			if (bWaitNetStatus)
			{
				return;
			}
			switch (_mainStatus)
			{
			case MainStatus.Idle:
			case MainStatus.NextAction:
				mainStatus = (MainStatus)OrangeBattleUtility.Random(3, 11);
				if (mainStatus == MainStatus.Skill1 && _lateAction == MainStatus.Skill1)
				{
					mainStatus = ((!((float)(int)Hp > (float)(int)MaxHp * 0.5f)) ? MainStatus.Skill3 : MainStatus.Skill0);
				}
				break;
			case MainStatus.FindPlayer:
				mainStatus = MainStatus.Skill0;
				break;
			}
			Target = _enemyAutoAimSystem.GetClosetPlayer();
			if (Target == null)
			{
				return;
			}
			TargetPos = Target.Controller.LogicPosition;
			UpdateDirection();
		}
		else if (bWaitNetStatus)
		{
			bWaitNetStatus = false;
		}
		if (StageUpdate.bIsHost)
		{
			NetSyncData netSyncData = new NetSyncData();
			netSyncData.TargetPosX = TargetPos.x;
			netSyncData.TargetPosY = TargetPos.y;
			netSyncData.TargetPosZ = TargetPos.z;
			netSyncData.SelfPosX = Controller.LogicPosition.x;
			netSyncData.SelfPosY = Controller.LogicPosition.y;
			netSyncData.SelfPosZ = Controller.LogicPosition.z;
			bWaitNetStatus = true;
			StageUpdate.RegisterSendAndRun(sNetSerialID, (int)mainStatus, JsonConvert.SerializeObject(netSyncData));
		}
	}

	private void Play_skill02PPS()
	{
		_skill02PPS.Play();
		MonoBehaviourSingleton<FxManager>.Instance.RegisterFxBase(_skill02PPS);
	}

	private void Pause_skill02PPS()
	{
		_skill02PPS.Pause();
		MonoBehaviourSingleton<FxManager>.Instance.UnRegisterFxBase(_skill02PPS);
	}

	private void Stop_skill02PPS()
	{
		_skill02PPS.Stop();
		_skill02PPS.Clear();
		MonoBehaviourSingleton<FxManager>.Instance.UnRegisterFxBase(_skill02PPS);
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
		if (AiState == AI_STATE.mob_002 && _summonTimer.GetMillisecond() > 20000)
		{
			_summonTimer.TimerStart();
			MonoBehaviourSingleton<OrangeBattleUtility>.Instance.CallSummonEnemyEvent(_transform);
		}
		switch (_mainStatus)
		{
		case MainStatus.Debut:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (_currentFrame > 1f && _introReady)
				{
					SetStatus(MainStatus.Debut, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if (_unlockReady)
				{
					SetStatus(MainStatus.Idle);
				}
				break;
			}
			break;
		case MainStatus.FindPlayer:
			_enemyAutoAimSystem.UpdateAimRange(20f);
			UpdateRandomState();
			break;
		case MainStatus.Idle:
			if (AiTimer.GetMillisecond() > EnemyData.n_AI_TIMER)
			{
				isActing = false;
			}
			break;
		case MainStatus.NextAction:
			isActing = false;
			break;
		case MainStatus.Jump:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Jump, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if (_velocity.y < 3000)
				{
					SetStatus(MainStatus.Jump, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Jump, SubStatus.Phase3);
				}
				break;
			case SubStatus.Phase3:
				if (Controller.Collisions.below)
				{
					SetStatus(MainStatus.Jump, SubStatus.Phase4);
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
				if (_currentFrame > 0.2f && _skill01Bullet.IsIdle())
				{
					_skill01Bullet.Shoot();
					_skill01Count++;
				}
				if (_currentFrame > 1f)
				{
					if (UseDoubleShot && _skill01Count < 2)
					{
						SetStatus(MainStatus.Skill0);
					}
					else
					{
						SetStatus(MainStatus.Skill0, SubStatus.Phase3);
					}
				}
				break;
			case SubStatus.Phase3:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill0, SubStatus.Phase4);
				}
				break;
			case SubStatus.Phase4:
				if (_currentFrame > 4f)
				{
					SetStatus(MainStatus.Skill0, SubStatus.Phase5);
				}
				break;
			case SubStatus.Phase5:
				_skill01Count = 0;
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
					PlayBossSE("BossSE02", "bs013_toxic10");
					PlayBossSE("BossSE02", "bs013_toxic11");
					SetStatus(MainStatus.Skill1, SubStatus.Phase1);
					Play_skill02PPS();
					_checkVenomTime = true;
				}
				break;
			case SubStatus.Phase1:
				if (AiTimer.GetMillisecond() > 1000)
				{
					AcidLiquor(false);
					SetStatus(MainStatus.Skill1, SubStatus.Phase2);
					Pause_skill02PPS();
				}
				break;
			case SubStatus.Phase2:
				if (AiTimer.GetMillisecond() > _diveTime)
				{
					if (isNeedSkill5 && isSkill5ING)
					{
						isSkill5ING = false;
						isNeedSkill5 = false;
					}
					PlayBossSE("BossSE02", "bs013_toxic13");
					AcidLiquor(false);
					SetStatus(MainStatus.Skill1, SubStatus.Phase3);
					Pause_skill02PPS();
				}
				break;
			case SubStatus.Phase3:
				if (AiTimer.GetMillisecond() > 100)
				{
					SetStatus(MainStatus.Skill1, SubStatus.Phase4);
					Play_skill02PPS();
				}
				break;
			case SubStatus.Phase4:
				if (AiTimer.GetMillisecond() > 100)
				{
					PlayBossSE("BossSE02", "bs013_toxic16");
					SetStatus(MainStatus.Skill1, SubStatus.Phase5);
				}
				break;
			case SubStatus.Phase5:
				if (_currentFrame > 1f)
				{
					if (isNeedSkill5)
					{
						SetStatus(MainStatus.Skill5);
					}
					else
					{
						SetStatus(MainStatus.Idle);
					}
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
				if (_velocity.y < 0)
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
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill2, SubStatus.Phase4);
				}
				break;
			case SubStatus.Phase4:
				if (_currentFrame > 0.5f && !_skill03ParticleSystem.isPlaying)
				{
					_skill03ParticleSystem.Play();
				}
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill2, SubStatus.Phase5);
				}
				break;
			case SubStatus.Phase5:
				_rushDiveSpeed += (float)Mathf.Abs((VInt)_rushDiveSpeedPlus * GameLogicUpdateManager.g_fixFrameLenFP) * 0.001f * 0.001f;
				_velocity = new VInt3(_rushDiveDir.normalized * _rushDiveSpeed);
				if (Controller.Collisions.below || AiTimer.GetMillisecond() > 1000)
				{
					PlayBossSE("BossSE02", "bs013_toxic15_stop");
					AcidLiquor(true);
					_collideBullet.UpdateBulletData(EnemyWeapons[0].BulletData);
					_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
					_collideBullet.Active(targetMask);
					IgnoreGravity = false;
					base.AllowAutoAim = false;
					_velocity = VInt3.zero;
					_skill03ParticleSystem.Stop();
					ModelTransform.gameObject.SetActive(false);
					Play_skill02PPS();
					_checkVenomTime = true;
					SetStatus(MainStatus.Skill1, SubStatus.Phase2);
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
				else if (_skill04BulletA != null && _currentFrame > 0.15f && _skill04BulletA.IsIdle() && _currentFrame < 0.25f)
				{
					_skill04BulletA.Shoot();
				}
				else if (_skill04BulletB != null && _currentFrame > 0.55f && _skill04BulletB.IsIdle() && _currentFrame < 0.65f)
				{
					_skill04BulletB.Shoot();
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
				if (GameLogicUpdateManager.GameFrame >= ActionFrame && _velocity.y <= 10)
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
				if (Controller.Collisions.below)
				{
					SetStatus(MainStatus.Skill4, SubStatus.Phase4);
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
				if (_currentFrame > 5f)
				{
					SetStatus(MainStatus.Skill5, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill5, SubStatus.Phase3);
				}
				break;
			case SubStatus.Phase3:
				if (_currentFrame > 1f * (float)ActionTimes)
				{
					SetStatus(MainStatus.Skill5, SubStatus.Phase4);
				}
				break;
			case SubStatus.Phase4:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill1);
				}
				break;
			}
			break;
		case MainStatus.Die:
		{
			SubStatus subStatus = _subStatus;
			break;
		}
		}
		if (_checkVenomTime && _skill02PPS.time > 0.9f)
		{
			_checkVenomTime = false;
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
			UseDoubleShot = false;
			isActing = false;
			isSkill1ING = false;
			FarEndPos = false;
			_collideBullet.UpdateBulletData(EnemyWeapons[0].BulletData);
			_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			_collideBullet.Active(targetMask);
			_skill02collideBullet.UpdateBulletData(EnemyWeapons[3].BulletData);
			_skill02collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			Skill5CollideBullet.UpdateBulletData(EnemyWeapons[3].BulletData);
			Skill5CollideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			CheckRoomSize();
			if (AiState == AI_STATE.mob_002)
			{
				_summonTimer.TimerStart();
			}
			else
			{
				_summonTimer.TimerStop();
			}
			_tfModelMelt.gameObject.SetActive(false);
			_tfModelSolid.gameObject.SetActive(false);
			ModelTransform.gameObject.SetActive(false);
			SetStatus(MainStatus.Skill1, SubStatus.Phase2);
		}
		else
		{
			_collideBullet.BackToPool();
			_skill02collideBullet.BackToPool();
			_summonTimer.TimerStop();
		}
	}

	public override void UpdateEnemyID(int id)
	{
		base.UpdateEnemyID(id);
		UpdateAIState();
		AI_STATE aiState = AiState;
		if (aiState == AI_STATE.mob_002)
		{
			_bDeadCallResult = false;
		}
		else
		{
			_bDeadCallResult = true;
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
		UpdateDirection(base.direction);
		base.transform.position = pos;
	}

	public override void BossIntro(Action cb)
	{
		IntroCallBack = cb;
		_introReady = true;
	}

	protected override void DeadBehavior(ref HurtPassParam tHurtPassParam)
	{
		if ((bool)_collideBullet)
		{
			_collideBullet.BackToPool();
		}
		if ((bool)Skill5CollideBullet)
		{
			Skill5CollideBullet.BackToPool();
		}
		if ((bool)_skill01Bullet && _skill01Bullet.IsIdle())
		{
			_skill01Bullet.BackToPool();
		}
		if ((bool)_skill04BulletA && _skill04BulletA.IsIdle())
		{
			_skill04BulletA.BackToPool();
		}
		if ((bool)_skill04BulletB && _skill04BulletB.IsIdle())
		{
			_skill04BulletB.BackToPool();
		}
		base.DeadBehavior(ref tHurtPassParam);
	}

	private void ChangeToStandModel()
	{
		ModelTransform.gameObject.SetActive(true);
		_tfModelMelt.gameObject.SetActive(false);
		_tfModelSolid.gameObject.SetActive(false);
		Stop_skill02PPS();
		_skill02ParticleSystem.Stop(true);
		_skill03ParticleSystem.Stop(true);
	}

	private void SetStatus(MainStatus mainStatus, SubStatus subStatus = SubStatus.Phase0)
	{
		_mainStatus = mainStatus;
		_subStatus = subStatus;
		switch (_mainStatus)
		{
		case MainStatus.Debut:
		{
			_lateAction = _mainStatus;
			SubStatus subStatus2 = _subStatus;
			if (subStatus2 == SubStatus.Phase1 && IntroCallBack != null)
			{
				IntroCallBack();
			}
			break;
		}
		case MainStatus.FindPlayer:
			_lateAction = _mainStatus;
			_enemyAutoAimSystem.UpdateAimRange(200f);
			ChangeToStandModel();
			break;
		case MainStatus.Idle:
			_lateAction = _mainStatus;
			_velocity = VInt3.zero;
			ChangeToStandModel();
			break;
		case MainStatus.Jump:
			_lateAction = _mainStatus;
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				PlayBossSE("BossSE02", "bs013_toxic03");
				_velocity = VInt3.zero;
				ChangeToStandModel();
				break;
			case SubStatus.Phase1:
			{
				int logicX = Mathf.Abs(TargetPos.x - Controller.LogicPosition.x);
				int logicY = TargetPos.y - Controller.LogicPosition.y + 1500;
				_velocity = CalculateJumpVelocity(logicX, logicY, _defJumpSpeedY);
				break;
			}
			case SubStatus.Phase4:
				PlayBossSE("BossSE02", "bs013_toxic04");
				_velocity = VInt3.zero;
				break;
			}
			break;
		case MainStatus.Skill0:
			_lateAction = _mainStatus;
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				UpdateDirection((!(_transform.position.x > CenterPos.x)) ? 1 : (-1));
				_velocity = VInt3.zero;
				ChangeToStandModel();
				break;
			case SubStatus.Phase1:
			{
				Vector3 localPosition = _tfSkill01ShootPoint.localPosition;
				localPosition.x *= base.direction;
				_skill01Bullet = (SeaHorseSkill01Bullet)BulletBase.TryShotBullet(EnemyWeapons[2].BulletData, _transform.position + localPosition, Vector3.right * base.direction, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
				break;
			}
			}
			break;
		case MainStatus.Skill1:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				isSkill1ING = true;
				_velocity = VInt3.zero;
				ChangeToStandModel();
				break;
			case SubStatus.Phase1:
				IsInvincible = true;
				base.AllowAutoAim = false;
				AcidLiquor(true);
				_tfModelMelt.gameObject.SetActive(true);
				ModelTransform.gameObject.SetActive(false);
				break;
			case SubStatus.Phase2:
				isSkill1ING = true;
				_tfModelMelt.gameObject.SetActive(false);
				_diveTime = 50;
				break;
			case SubStatus.Phase3:
			{
				VInt3 endPos = GetEndPos(FarEndPos);
				if (isNeedSkill5)
				{
					if (!Target)
					{
						Target = _enemyAutoAimSystem.GetClosetPlayer();
					}
					if ((bool)Target)
					{
						EndPos = Target._transform.position;
					}
					else
					{
						EndPos = _transform.position;
					}
					Vector3 vector = EndPos + Vector3.up;
					RaycastHit2D raycastHit2D2 = Physics2D.Raycast(vector, Vector2.left, 4f, Controller.collisionMask);
					RaycastHit2D raycastHit2D3 = Physics2D.Raycast(vector, Vector2.right, 4f, Controller.collisionMask);
					float num2 = (raycastHit2D2 ? (raycastHit2D2.point.x + 1.5f) : (vector.x - 2f));
					float num3 = (raycastHit2D3 ? (raycastHit2D3.point.x - 1.5f) : (vector.x + 2f));
					CameraControl component = MonoBehaviourSingleton<OrangeSceneManager>.Instance.MainCamera.GetComponent<CameraControl>();
					if (num2 < component.CurrentLockRange.MinX - ManagedSingleton<StageHelper>.Instance.fCameraWHalf + 1f)
					{
						num2 = component.CurrentLockRange.MinX - ManagedSingleton<StageHelper>.Instance.fCameraWHalf + 1f;
					}
					if (num3 > component.CurrentLockRange.MaxX + ManagedSingleton<StageHelper>.Instance.fCameraWHalf - 1f)
					{
						num3 = component.CurrentLockRange.MaxX + ManagedSingleton<StageHelper>.Instance.fCameraWHalf - 1f;
					}
					endPos.x = (int)(OrangeBattleUtility.Random(num2, num3) * 1000f);
					endPos.y = Controller.LogicPosition.y;
					Controller.LogicPosition = endPos;
				}
				Controller.LogicPosition = endPos;
				UpdateDirection();
				break;
			}
			case SubStatus.Phase4:
				UpdateDirection((!(_transform.position.x > CenterPos.x)) ? 1 : (-1));
				_tfModelSolid.gameObject.SetActive(true);
				break;
			case SubStatus.Phase5:
				UpdateDirection((!(_transform.position.x > CenterPos.x)) ? 1 : (-1));
				isSkill1ING = false;
				IsInvincible = false;
				base.AllowAutoAim = true;
				_tfModelSolid.gameObject.SetActive(false);
				ModelTransform.gameObject.SetActive(true);
				_skill03ParticleSystem.Stop();
				_collideBullet.Active(targetMask);
				break;
			}
			break;
		case MainStatus.Skill2:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_velocity = VInt3.zero;
				_rushDiveJumpPoint = _transform.position;
				ChangeToStandModel();
				break;
			case SubStatus.Phase1:
				PlayBossSE("BossSE02", "bs013_toxic03");
				_velocity = CalculateJumpVelocity(0, 0, _defJumpSpeedY);
				break;
			case SubStatus.Phase2:
				IgnoreGravity = true;
				_velocity = VInt3.zero;
				_collideBullet.UpdateBulletData(EnemyWeapons[3].BulletData);
				_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
				_collideBullet.Active(targetMask);
				break;
			case SubStatus.Phase4:
				PlayBossSE("BossSE02", "bs013_toxic15_lp");
				UpdateDirection();
				break;
			case SubStatus.Phase5:
			{
				VInt3 targetPos = TargetPos;
				if (!Target.Controller.Collisions.below)
				{
					RaycastHit2D raycastHit2D = Physics2D.Raycast(targetPos.vec3, Vector2.down, 100f, Controller.collisionMask);
					if ((bool)raycastHit2D)
					{
						targetPos.y = (int)(raycastHit2D.collider.transform.position.y * 1000f);
					}
					else
					{
						targetPos.y = (int)(_rushDiveJumpPoint.y * 1000f);
					}
				}
				if (TargetPos.x >= Controller.LogicPosition.x)
				{
					_rushDiveDir = targetPos.vec3 + Vector3.right * 1.5f - Controller.LogicPosition.vec3;
				}
				else
				{
					_rushDiveDir = targetPos.vec3 - Vector3.right * 1.5f - Controller.LogicPosition.vec3;
				}
				_rushDiveSpeed = _rushDiveSpeedStart;
				_velocity = new VInt3(_rushDiveDir.normalized * _rushDiveSpeed);
				break;
			}
			}
			break;
		case MainStatus.Skill3:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				UpdateDirection((!(_transform.position.x > CenterPos.x)) ? 1 : (-1));
				_velocity = VInt3.zero;
				ChangeToStandModel();
				break;
			case SubStatus.Phase1:
				_skill04BulletA = (SeaHorseSkill04Bullet)BulletBase.TryShotBullet(EnemyWeapons[1].BulletData, _transform.position + Vector3.up, Vector3.right * base.direction, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
				if (UseDoubleShot)
				{
					_skill04BulletB = (SeaHorseSkill04Bullet)BulletBase.TryShotBullet(EnemyWeapons[1].BulletData, _transform.position + Vector3.up, Vector3.right * base.direction, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
				}
				break;
			}
			break;
		case MainStatus.Skill4:
			switch (_subStatus)
			{
			case SubStatus.Phase1:
			{
				if (!Target)
				{
					Target = _enemyAutoAimSystem.GetClosetPlayer();
				}
				if ((bool)Target)
				{
					EndPos = Target._transform.position;
				}
				else
				{
					EndPos = _transform.position;
				}
				if (NowPos.x > EndPos.x)
				{
					UpdateDirection(-1);
				}
				else
				{
					UpdateDirection(1);
				}
				int num = (int)(Skill4JumpTime * 20f);
				ActionFrame = GameLogicUpdateManager.GameFrame + num;
				_velocity = new VInt3(CalXSpeed(NowPos.x, (NowPos.x + EndPos.x) / 2f, Skill4JumpTime), Mathf.Abs(Gravity * num), 0);
				break;
			}
			case SubStatus.Phase2:
				if (base.direction == -1)
				{
					EndPos.x = MinPos.x + 0.5f;
				}
				else
				{
					EndPos.x = MaxPos.x - 0.5f;
				}
				_velocity.x = CalXSpeed(NowPos.x, EndPos.x, Skill4JumpTime);
				IgnoreGravity = false;
				break;
			case SubStatus.Phase4:
				_velocity = VInt3.zero;
				UpdateDirection(-base.direction);
				break;
			}
			break;
		case MainStatus.Skill5:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				ActionTimes = Skill5AtkTime;
				break;
			case SubStatus.Phase1:
				isSkill5ING = true;
				break;
			case SubStatus.Phase2:
				if ((bool)Skill5Fx)
				{
					Skill5Fx.Play();
				}
				PlayBossSE("BossSE02", "bs013_toxic19");
				Skill5CollideBullet.Active(targetMask);
				Skill5CollideBullet.transform.localRotation = Quaternion.identity;
				break;
			case SubStatus.Phase4:
				Skill5CollideBullet.BackToPool();
				if ((bool)Skill5Fx)
				{
					Skill5Fx.Stop();
				}
				break;
			}
			break;
		case MainStatus.Die:
			if (_subStatus == SubStatus.Phase0)
			{
				if (!Controller.Collisions.below)
				{
					IgnoreGravity = true;
				}
				if ((bool)_skill01Bullet && _skill01Bullet.IsIdle())
				{
					_skill01Bullet.BackToPool();
				}
				if ((bool)_skill04BulletA && _skill04BulletA.IsIdle())
				{
					_skill04BulletA.BackToPool();
				}
				if ((bool)_skill04BulletB && _skill04BulletB.IsIdle())
				{
					_skill04BulletB.BackToPool();
				}
				_collideBullet.BackToPool();
				_summonTimer.TimerStop();
				base.AllowAutoAim = false;
				_velocity.x = 0;
				OrangeBattleUtility.LockPlayer();
				if (_bDeadCallResult)
				{
					StartCoroutine(BossDieFlow(AimPosition));
				}
				else
				{
					StartCoroutine(BossDieFlow(AimPosition, "FX_BOSS_EXPLODE2", false, false));
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
			if (_subStatus == SubStatus.Phase0)
			{
				_currentAnimationId = AnimationID.ANI_DEBUT;
				_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
			}
			break;
		case MainStatus.Idle:
			_currentAnimationId = AnimationID.ANI_IDLE_LOOP;
			_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
			break;
		case MainStatus.Jump:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_JUMP_START;
				_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_JUMP_LOOP;
				_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_JUMP_TO_FALL;
				_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
				break;
			case SubStatus.Phase3:
				_currentAnimationId = AnimationID.ANI_FALL_LOOP;
				_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
				break;
			case SubStatus.Phase4:
				_currentAnimationId = AnimationID.ANI_LANDING;
				_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
				break;
			}
			break;
		case MainStatus.Skill0:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				PlayBossSE("BossSE02", "bs013_toxic05");
				_currentAnimationId = AnimationID.ANI_SKILL_01_PREPARE_START;
				_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_SKILL_01_PREPARE_LOOP;
				_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_SKILL_01_ATK1_START;
				_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
				break;
			case SubStatus.Phase3:
				_currentAnimationId = AnimationID.ANI_SKILL_01_ATK2_START;
				_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
				break;
			case SubStatus.Phase4:
				_currentAnimationId = AnimationID.ANI_SKILL_01_ATK2_LOOP;
				_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
				break;
			case SubStatus.Phase5:
				_currentAnimationId = AnimationID.ANI_SKILL_01_END;
				_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
				break;
			}
			break;
		case MainStatus.Skill1:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_SKILL_02_START;
				_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
				break;
			case SubStatus.Phase5:
				_currentAnimationId = AnimationID.ANI_SKILL_02_END;
				_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
				break;
			}
			break;
		case MainStatus.Skill2:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_SKILL_03_JUMP_START;
				_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_SKILL_03_JUMP_LOOP;
				_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_SKILL_03_JUMP_ATK_START;
				_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
				break;
			case SubStatus.Phase3:
				_currentAnimationId = AnimationID.ANI_SKILL_03_JUMP_ATK_LOOP;
				_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
				break;
			case SubStatus.Phase4:
				_currentAnimationId = AnimationID.ANI_SKILL_03_JUMP_ATK_END;
				_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
				break;
			case SubStatus.Phase5:
				_currentAnimationId = AnimationID.ANI_SKILL_03_FALL_LOOP;
				_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
				break;
			}
			break;
		case MainStatus.Skill3:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				PlayBossSE("BossSE02", "bs013_toxic05");
				_currentAnimationId = AnimationID.ANI_SKILL_04_PREPARE_START;
				_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_SKILL_04_PREPARE_LOOP;
				_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_SKILL_04_ATK_START;
				_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
				break;
			case SubStatus.Phase3:
				_currentAnimationId = AnimationID.ANI_SKILL_04_ATK_LOOP;
				_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
				break;
			case SubStatus.Phase4:
				_currentAnimationId = AnimationID.ANI_SKILL_04_ATK_END;
				_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
				break;
			}
			break;
		case MainStatus.Skill4:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_JUMP_START;
				_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_JUMP_LOOP;
				_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_JUMP_TO_FALL;
				_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
				break;
			case SubStatus.Phase3:
				_currentAnimationId = AnimationID.ANI_FALL_LOOP;
				_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
				break;
			case SubStatus.Phase4:
				_currentAnimationId = AnimationID.ANI_LANDING;
				_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
				break;
			}
			break;
		case MainStatus.Skill5:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_SKILL5_START1;
				_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_SKILL5_LOOP1;
				_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_SKILL5_START2;
				_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
				break;
			case SubStatus.Phase3:
				_currentAnimationId = AnimationID.ANI_SKILL5_LOOP2;
				_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
				break;
			case SubStatus.Phase4:
				_currentAnimationId = AnimationID.ANI_SKILL5_END;
				_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
				break;
			}
			break;
		case MainStatus.Die:
			if (_subStatus == SubStatus.Phase0)
			{
				_currentAnimationId = AnimationID.ANI_HURT_LOOP;
				_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
			}
			break;
		case MainStatus.FindPlayer:
		case MainStatus.Skill6:
		case MainStatus.NextAction:
			break;
		}
	}

	private void UpdateDirection(int forceDirection = 0)
	{
		if (forceDirection != 0)
		{
			base.direction = forceDirection;
		}
		else
		{
			int num = Controller.LogicPosition.x - TargetPos.x;
			base.direction = ((num <= 0) ? 1 : (-1));
		}
		ModelTransform.localScale = new Vector3(ModelTransform.localScale.x, ModelTransform.localScale.y, Mathf.Abs(ModelTransform.localScale.z) * (float)base.direction);
		_tfModelMelt.localScale = ModelTransform.localScale;
		_tfModelSolid.localScale = ModelTransform.localScale;
	}

	private void AcidLiquor(bool enable)
	{
		if (enable)
		{
			_skill02collideBullet.Active(targetMask);
			_skill02ParticleSystem.Play();
		}
		else
		{
			_skill02collideBullet.BackToPool();
			_skill02ParticleSystem.Stop();
		}
	}

	private int CalXSpeed(float startx, float endx, float jumptime, float timeoffset = 1f)
	{
		int num = (int)((float)Mathf.Abs((int)(jumptime * 20f)) * timeoffset);
		return (int)((endx - startx) * 1000f * 20f / (float)num);
	}

	private VInt3 GetEndPos(bool far = false)
	{
		if (!Target)
		{
			Target = _enemyAutoAimSystem.GetClosetPlayer();
		}
		if ((bool)Target)
		{
			EndPos = Target._transform.position;
		}
		else
		{
			EndPos = _transform.position;
		}
		VInt3 zero = VInt3.zero;
		if ((!far && EndPos.x < CenterPos.x) || (far && EndPos.x > CenterPos.x))
		{
			zero.x = (int)(MinPos.x + 1.5f) * 1000;
		}
		else
		{
			zero.x = (int)(MaxPos.x - 1.5f) * 1000;
		}
		zero.y = Controller.LogicPosition.y;
		return zero;
	}

	public void SetStatusByMission(int mainStatus, int subStatus = 0)
	{
		if (isActing)
		{
			Debug.LogError("分身動作尚未執行完畢就被設定狀態");
			return;
		}
		isActing = true;
		SetStatus((MainStatus)mainStatus, (SubStatus)subStatus);
	}

	public void SetIsFarEndPos(bool isfar)
	{
		FarEndPos = isfar;
	}

	private void CheckRoomSize()
	{
		int layerMask = (1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockLayer) | (1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockEnemyLayer) | (1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockPlayerLayer) | (1 << ManagedSingleton<OrangeLayerManager>.Instance.WallKickMask);
		Vector3 vector = new Vector3(_transform.position.x, _transform.position.y + 0.01f, 0f);
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
}
