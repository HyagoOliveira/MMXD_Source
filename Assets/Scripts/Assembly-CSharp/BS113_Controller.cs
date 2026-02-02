#define RELEASE
using System;
using CallbackDefs;
using CodeStage.AntiCheat.ObscuredTypes;
using NaughtyAttributes;
using Newtonsoft.Json;
using StageLib;
using UnityEngine;

public class BS113_Controller : EnemyControllerBase, IManagedUpdateBehavior
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
		Phase6 = 6,
		MAX_SUBSTATUS = 7
	}

	private enum SkillPattern
	{
		Start = 0,
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

	private BS113CombieBullet _skill04BulletA;

	private BS113CombieBullet _skill04BulletB;

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

	private float ShotAngle;

	private readonly int _HashAngle = Animator.StringToHash("Angle");

	private int SpawnCount;

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

	private EM201_Controller EM201;

	[Header("AI控制")]
	[SerializeField]
	[NaughtyAttributes.ReadOnly]
	private SkillPattern skillpattern;

	private bool FarEndPos;

	private bool UseDoubleShot;

	private int State1UseTimes;

	private int State2UseTimes;

	private int State3UseTimes;

	private int State4UseTimes;

	private int State5UseTimes;

	private int State6UseTimes;

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
		_collideBullet = OrangeBattleUtility.FindChildRecursive(ref target, "Bip001 Spine").gameObject.AddOrGetComponent<CollideBullet>();
		_skill02collideBullet = OrangeBattleUtility.FindChildRecursive(ref target, "fxuse_seahorse_002").gameObject.AddOrGetComponent<CollideBullet>();
		_skill02ParticleSystem = OrangeBattleUtility.FindChildRecursive(ref target, "fxuse_seahorse_002_(work)", true).GetComponent<ParticleSystem>();
		_skill03ParticleSystem = OrangeBattleUtility.FindChildRecursive(ref target, "fxduring_seahorse_roll", true).GetComponent<ParticleSystem>();
		_skill02ParticleSystem.Stop();
		_skill02PPS = OrangeBattleUtility.FindChildRecursive(ref target, "PS_Explosion", true).GetComponent<ParticleSystem>();
		_skill03ParticleSystem.Stop();
		Skill5CollideBullet = OrangeBattleUtility.FindChildRecursive(ref target, Skill5BulletObj).gameObject.AddOrGetComponent<CollideBullet>();
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
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxhit_bs113seahorse_hit_ground_m", 5);
		_summonTimer = OrangeTimerManager.GetTimer();
		_mainStatus = MainStatus.Idle;
		_subStatus = SubStatus.Phase0;
		_tfModelMelt.gameObject.SetActive(false);
		_tfModelSolid.gameObject.SetActive(false);
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
		MainStatus nSetKey = MainStatus.Idle;
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
				switch (skillpattern)
				{
				case SkillPattern.Start:
				case SkillPattern.State1:
					if (State1UseTimes > 1)
					{
						State1UseTimes = 0;
						skillpattern = SkillPattern.State2;
						goto case SkillPattern.State2;
					}
					State1UseTimes++;
					nSetKey = MainStatus.Skill3;
					break;
				case SkillPattern.State2:
					if (State2UseTimes > 0)
					{
						State2UseTimes = 0;
						skillpattern = SkillPattern.State3;
						goto case SkillPattern.State3;
					}
					State2UseTimes++;
					FarEndPos = true;
					nSetKey = MainStatus.Skill1;
					break;
				case SkillPattern.State3:
					if (State3UseTimes > 0)
					{
						State3UseTimes = 0;
						skillpattern = SkillPattern.State4;
						goto case SkillPattern.State4;
					}
					State3UseTimes++;
					FarEndPos = true;
					nSetKey = MainStatus.Skill2;
					break;
				case SkillPattern.State4:
					if (!EM201 || ((bool)EM201 && (int)EM201.Hp <= 0))
					{
						EM201 = null;
						skillpattern = SkillPattern.State1;
						goto case SkillPattern.Start;
					}
					if (State4UseTimes > 0)
					{
						State4UseTimes = 0;
						skillpattern = SkillPattern.State5;
						goto case SkillPattern.State5;
					}
					nSetKey = MainStatus.Skill3;
					State4UseTimes++;
					break;
				case SkillPattern.State5:
					if (!EM201 || ((bool)EM201 && (int)EM201.Hp <= 0))
					{
						EM201 = null;
						skillpattern = SkillPattern.State1;
						goto case SkillPattern.Start;
					}
					if (State5UseTimes > 0)
					{
						State5UseTimes = 0;
						skillpattern = SkillPattern.State6;
						goto case SkillPattern.State6;
					}
					FarEndPos = false;
					nSetKey = MainStatus.Skill2;
					State5UseTimes++;
					break;
				case SkillPattern.State6:
					if (!EM201 || ((bool)EM201 && (int)EM201.Hp <= 0))
					{
						EM201 = null;
						skillpattern = SkillPattern.State1;
						goto case SkillPattern.Start;
					}
					if (State6UseTimes > 0)
					{
						State6UseTimes = 0;
						skillpattern = SkillPattern.State4;
						goto case SkillPattern.State4;
					}
					nSetKey = MainStatus.Skill0;
					State6UseTimes++;
					break;
				}
				break;
			case MainStatus.FindPlayer:
				nSetKey = MainStatus.Skill0;
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
			StageUpdate.RegisterSendAndRun(sNetSerialID, (int)nSetKey, JsonConvert.SerializeObject(netSyncData));
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
			if (AiTimer.GetMillisecond() > ((EM201 == null) ? (EnemyData.n_AI_TIMER / 2) : EnemyData.n_AI_TIMER) && (!EM201 || ((bool)EM201 && !EM201.isActing)))
			{
				SetStatus(MainStatus.NextAction);
			}
			break;
		case MainStatus.NextAction:
			UpdateRandomState();
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
			{
				if (AiTimer.GetMillisecond() <= _diveTime)
				{
					break;
				}
				SkillPattern skillPattern = skillpattern;
				if (skillPattern == SkillPattern.State5)
				{
					Pause_skill02PPS();
					if ((bool)EM201 && (!EM201 || EM201.isNeedSkill5))
					{
						break;
					}
				}
				PlayBossSE("BossSE02", "bs013_toxic13");
				AcidLiquor(false);
				SetStatus(MainStatus.Skill1, SubStatus.Phase3);
				Pause_skill02PPS();
				break;
			}
			case SubStatus.Phase3:
				if (AiTimer.GetMillisecond() > 100)
				{
					SetStatus(MainStatus.Skill1, SubStatus.Phase4);
					Play_skill02PPS();
				}
				break;
			case SubStatus.Phase4:
				if (AiTimer.GetMillisecond() > 1000)
				{
					PlayBossSE("BossSE02", "bs013_toxic16");
					SetStatus(MainStatus.Skill1, SubStatus.Phase5);
				}
				break;
			case SubStatus.Phase5:
				if (_currentFrame > 1f)
				{
					switch (skillpattern)
					{
					case SkillPattern.State2:
						SetStatus(MainStatus.Skill0);
						break;
					case SkillPattern.State3:
					case SkillPattern.State5:
						SetStatus(MainStatus.Skill1, SubStatus.Phase6);
						break;
					default:
						Debug.LogError("Skill1不該來這邊");
						SetStatus(MainStatus.Idle);
						break;
					}
				}
				break;
			case SubStatus.Phase6:
				if (!EM201 || ((bool)EM201 && !EM201.isSkill1ING))
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
			{
				SkillPattern skillPattern = skillpattern;
				if (skillPattern == SkillPattern.State5)
				{
					if (!EM201 || ((bool)EM201 && EM201.isSkill5ING))
					{
						SetStatus(MainStatus.Skill2, SubStatus.Phase5);
					}
				}
				else if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill2, SubStatus.Phase5);
				}
				if (_currentFrame > 0.5f && !_skill03ParticleSystem.isPlaying)
				{
					_skill03ParticleSystem.Play();
				}
				break;
			}
			case SubStatus.Phase5:
				_rushDiveSpeed += (float)Mathf.Abs((VInt)_rushDiveSpeedPlus * GameLogicUpdateManager.g_fixFrameLenFP) * 0.001f * 0.001f;
				_velocity = new VInt3(_rushDiveDir.normalized * _rushDiveSpeed);
				if (Controller.Collisions.below || Controller.Collisions.above || Controller.Collisions.right || Controller.Collisions.left || AiTimer.GetMillisecond() > 1000)
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
					SkillPattern skillPattern = skillpattern;
					if (skillPattern == SkillPattern.State5)
					{
						Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.CAMERA_SHAKE, 0.5f, false);
					}
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
					switch (skillpattern)
					{
					case SkillPattern.State1:
						SetStatus(MainStatus.Skill4);
						break;
					case SkillPattern.State4:
						SetStatus(MainStatus.Idle);
						break;
					default:
						Debug.LogError("Skill3不該來這邊");
						SetStatus(MainStatus.Idle);
						break;
					}
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
					SetStatus(MainStatus.Idle);
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
			if ((bool)EM201 && (int)EM201.Hp <= 0)
			{
				EM201 = null;
			}
			base.transform.localPosition = Vector3.MoveTowards(base.transform.localPosition, Controller.LogicPosition.vec3, distanceDelta);
		}
	}

	public override void SetActive(bool isActive)
	{
		base.SetActive(isActive);
		if (isActive)
		{
			UseDoubleShot = false;
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
			SetStatus(MainStatus.Debut);
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
			_bDeadCallResult = false;
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
		if (_mainStatus != MainStatus.Die)
		{
			if ((bool)_collideBullet)
			{
				_collideBullet.BackToPool();
			}
			if ((bool)EM201)
			{
				HurtPassParam hurtPassParam = new HurtPassParam();
				hurtPassParam.dmg = EM201.MaxHp;
				EM201.Hurt(hurtPassParam);
			}
			StageUpdate.SlowStage();
			SetColliderEnable(false);
			SetStatus(MainStatus.Die);
		}
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
			{
				SkillPattern skillPattern = skillpattern;
				if (skillPattern == SkillPattern.State6 && (bool)EM201 && _skill01Count < 1)
				{
					EM201.SetStatusByMission(4);
				}
				UpdateDirection((!(_transform.position.x > CenterPos.x)) ? 1 : (-1));
				_velocity = VInt3.zero;
				ChangeToStandModel();
				break;
			}
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
			{
				_tfModelMelt.gameObject.SetActive(false);
				_diveTime = 50;
				SkillPattern skillPattern = skillpattern;
				if (skillPattern == SkillPattern.State3)
				{
					EndPos = _transform.position;
					if (EndPos.x > MaxPos.x - 2.5f)
					{
						EndPos.x = MaxPos.x - 2.5f;
					}
					else if (EndPos.x < MinPos.x + 2.5f)
					{
						EndPos.x = MinPos.x + 2.5f;
					}
					EndPos.y = MinPos.y;
					EM201 = SpawnEnemy(EndPos);
					EM201.SetIsFarEndPos(false);
					EM201.UseDoubleShot = (UseDoubleShot ? true : false);
				}
				break;
			}
			case SubStatus.Phase3:
			{
				VInt3 endPos = GetEndPos(FarEndPos);
				Controller.LogicPosition = endPos;
				break;
			}
			case SubStatus.Phase4:
				UpdateDirection((!(_transform.position.x > CenterPos.x)) ? 1 : (-1));
				_tfModelSolid.gameObject.SetActive(true);
				break;
			case SubStatus.Phase5:
				UpdateDirection((!(_transform.position.x > CenterPos.x)) ? 1 : (-1));
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
			{
				SkillPattern skillPattern = skillpattern;
				if (skillPattern == SkillPattern.State5 && (bool)EM201)
				{
					EM201.SetIsFarEndPos(true);
					EM201.isNeedSkill5 = true;
					EM201.SetStatusByMission(5);
				}
				_velocity = VInt3.zero;
				_rushDiveJumpPoint = _transform.position;
				ChangeToStandModel();
				break;
			}
			case SubStatus.Phase1:
				PlayBossSE("BossSE02", "bs013_toxic03");
				_velocity = CalculateJumpVelocity(0, 0, _defJumpSpeedY);
				break;
			case SubStatus.Phase2:
				PlayBossSE("BossSE02", "bs013_toxic14");
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
				if (!Target)
				{
					Target = _enemyAutoAimSystem.GetClosetPlayer();
				}
				if ((bool)Target)
				{
					TargetPos = Target.Controller.LogicPosition;
				}
				VInt3 targetPos = TargetPos;
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
			{
				SkillPattern skillPattern = skillpattern;
				if (skillPattern == SkillPattern.State4 && (bool)EM201)
				{
					EM201.SetStatusByMission(7);
				}
				UpdateDirection((!(_transform.position.x > CenterPos.x)) ? 1 : (-1));
				_velocity = VInt3.zero;
				ChangeToStandModel();
				break;
			}
			case SubStatus.Phase1:
				_skill04BulletA = (BS113CombieBullet)BulletBase.TryShotBullet(EnemyWeapons[1].BulletData, _transform.position + Vector3.up, Vector3.right * base.direction, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
				_skill04BulletA.BackCallback = CombieBulletBackCB;
				if (UseDoubleShot)
				{
					_skill04BulletB = (BS113CombieBullet)BulletBase.TryShotBullet(EnemyWeapons[1].BulletData, _transform.position + Vector3.up, Vector3.right * base.direction, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
					_skill04BulletB.BackCallback = CombieBulletBackCB;
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
			case SubStatus.Phase2:
				Skill5CollideBullet.Active(targetMask);
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
				base.DeadPlayCompleted = true;
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
			case SubStatus.Phase6:
				_currentAnimationId = AnimationID.ANI_IDLE_LOOP;
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

	private EM201_Controller SpawnEnemy(Vector3 SpawnPos)
	{
		int num = 4;
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
			enemyControllerBase.SetPositionAndRotation(SpawnPos, base.direction == 1);
			enemyControllerBase.SetActive(true);
		}
		return enemyControllerBase as EM201_Controller;
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

	public override ObscuredInt Hurt(HurtPassParam tHurtPassParam)
	{
		ObscuredInt obscuredInt = base.Hurt(tHurtPassParam);
		if (!UseDoubleShot && (int)obscuredInt <= (int)MaxHp / 2 && (int)Hp >= 0)
		{
			UseDoubleShot = true;
			if ((bool)EM201)
			{
				EM201.UseDoubleShot = true;
			}
		}
		return obscuredInt;
	}

	private void CheckRoomSize()
	{
		int layerMask = (1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockLayer) | (1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockEnemyLayer) | (1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockPlayerLayer) | (1 << ManagedSingleton<OrangeLayerManager>.Instance.WallKickMask);
		Vector3 vector = new Vector3(_transform.position.x - 4f, _transform.position.y, 0f);
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

	private void CombieBulletBackCB(object obj)
	{
		BS113CombieBullet bS113CombieBullet = null;
		if (obj != null)
		{
			bS113CombieBullet = obj as BS113CombieBullet;
		}
		if (bS113CombieBullet == null)
		{
			Debug.LogError("子彈資料有誤。");
		}
		else if (bS113CombieBullet.isHitBullet)
		{
			Vector3 worldPos = (bS113CombieBullet._transform.position + bS113CombieBullet.hitBulletPos) / 2f;
			SwellBullet swellBullet = BulletBase.TryShotBullet(EnemyWeapons[5].BulletData, worldPos, Vector3.up, null, selfBuffManager.sBuffStatus, EnemyData, targetMask) as SwellBullet;
			PlayBossSE("BossSE02", "bs013_toxic17");
			if ((bool)swellBullet)
			{
				swellBullet.BackCallback = BigBulletBackCB;
			}
		}
	}

	private void BigBulletBackCB(object obj)
	{
		BasicBullet basicBullet = null;
		if (obj != null)
		{
			basicBullet = obj as BasicBullet;
		}
		if (basicBullet == null)
		{
			Debug.LogError("子彈資料有誤。");
			return;
		}
		PlayBossSE("BossSE02", "bs013_toxic18");
		for (int i = 0; i < 8; i++)
		{
			Vector3 pDirection = Quaternion.Euler(0f, 0f, i * 45) * Vector3.up;
			Vector3 position = basicBullet._transform.position;
			BulletBase.TryShotBullet(EnemyWeapons[6].BulletData, position, pDirection, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
		}
	}
}
