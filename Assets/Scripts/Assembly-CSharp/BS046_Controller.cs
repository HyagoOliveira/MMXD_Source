using System;
using CallbackDefs;
using NaughtyAttributes;
using Newtonsoft.Json;
using StageLib;
using UnityEngine;

public class BS046_Controller : EnemyControllerBase, IManagedUpdateBehavior
{
	private enum MainStatus
	{
		Idle = 0,
		Debut = 1,
		FindPlayer = 2,
		Run = 3,
		Jump = 4,
		Skill_01 = 5,
		Skill_02 = 6,
		Skill_03 = 7,
		Skill_04 = 8,
		Skill_05 = 9,
		NextAction = 10,
		Die = 11,
		IdleWaitNet = 12
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

	public enum AnimationID
	{
		ANI_IDLE_LOOP = 0,
		ANI_DEBUT = 1,
		ANI_RUN_LOOP = 2,
		ANI_HURT_LOOP = 3,
		ANI_JUMP_START = 4,
		ANI_JUMP_LOOP = 5,
		ANI_JUMP_TO_FALL = 6,
		ANI_FALL_LOOP = 7,
		ANI_LANDING = 8,
		ANI_DEAD = 9,
		ANI_SKILL_01_START = 10,
		ANI_SKILL_01_LOOP = 11,
		ANI_SKILL_01_END = 12,
		ANI_SKILL_02_START = 13,
		ANI_SKILL_02_LOOP = 14,
		ANI_SKILL_02_END = 15,
		ANI_SKILL_03_JUMP_START = 16,
		ANI_SKILL_03_JUMP_LOOP = 17,
		ANI_SKILL_03_JUMP_ATK_START = 18,
		ANI_SKILL_03_JUMP_ATK_END = 19,
		ANI_SKILL_03_FALL = 20,
		ANI_SKILL_03_LANDING = 21,
		ANI_SKILL_04_PREPARE_START = 22,
		ANI_SKILL_04_PREPARE_LOOP = 23,
		ANI_SKILL_04_DIVE_START = 24,
		ANI_SKILL_04_DIVE_LOOP = 25,
		ANI_SKILL_04_DIVE_ATK_LOOP = 26,
		ANI_SKILL_04_DIVE_ATK_END = 27,
		ANI_SKILL_04_DIVE_END = 28,
		ANI_SKILL_05_START = 29,
		ANI_SKILL_05_LOOP = 30,
		ANI_SKILL_05_END = 31,
		MAX_ANIMATION_ID = 32
	}

	private enum ReloadType
	{
		Right_Hand = 0,
		Left_Hand = 1,
		Head = 2
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

	private int[] _animationHash;

	private Transform _tfHeadDrill;

	private Transform _tfHeadDrillRootBone;

	private Transform _tfLeftDrill;

	private Transform _tfLeftDrillRootBone;

	private Transform _tfRightDrill;

	private Transform _tfRightDrillRootBone;

	private Transform _tfShootPointL;

	private Transform _tfShootPointR;

	private ParticleSystem _efx_Skill01;

	[SerializeField]
	private int _runSpeed = 4;

	[SerializeField]
	private VInt2 _jupmpSpeed = new VInt2(3, 600);

	private int _runTime;

	private bool _diveMode;

	private VInt3 _divePosition;

	private int _skill05Count;

	private bool _drillReloadCompleted = true;

	private OrangeTimer skill05Timer;

	[SerializeField]
	private MainStatus _deubgNextAI = MainStatus.Debut;

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
		OrangeBattleUtility.AddEnemyAutoAimSystem(_transform, out _enemyAutoAimSystem);
		_enemyAutoAimSystem.UpdateAimRange(20f);
		base.AimPoint = new Vector3(0f, 0.75f, 0f);
		_tfHeadDrill = OrangeBattleUtility.FindChildRecursive(ref target, "BS046_DrillMeshHead_U_m", true);
		_tfHeadDrillRootBone = _tfHeadDrill.GetComponent<SkinnedMeshRenderer>().rootBone;
		_tfLeftDrill = OrangeBattleUtility.FindChildRecursive(ref target, "BS046_DrillMeshLeft_U_m", true);
		_tfLeftDrillRootBone = _tfLeftDrill.GetComponent<SkinnedMeshRenderer>().rootBone;
		_tfRightDrill = OrangeBattleUtility.FindChildRecursive(ref target, "BS046_DrillMeshRight_U_m", true);
		_tfRightDrillRootBone = _tfRightDrill.GetComponent<SkinnedMeshRenderer>().rootBone;
		_tfShootPointL = OrangeBattleUtility.FindChildRecursive(ref target, "ShootPoint_L", true);
		_tfShootPointR = OrangeBattleUtility.FindChildRecursive(ref target, "ShootPoint_R", true);
		_efx_Skill01 = OrangeBattleUtility.FindChildRecursive(ref target, "fxuse_drill-man_000", true).GetComponentInChildren<ParticleSystem>();
		_animationHash = new int[32];
		_animationHash[0] = Animator.StringToHash("BS046@idle_loop");
		_animationHash[1] = Animator.StringToHash("BS046@debut");
		_animationHash[2] = Animator.StringToHash("BS046@run_loop");
		_animationHash[3] = Animator.StringToHash("BS046@hurt_loop");
		_animationHash[4] = Animator.StringToHash("BS046@jump_start");
		_animationHash[5] = Animator.StringToHash("BS046@jump_loop");
		_animationHash[6] = Animator.StringToHash("BS046@jump_to_fall");
		_animationHash[7] = Animator.StringToHash("BS046@fall_loop");
		_animationHash[8] = Animator.StringToHash("BS046@landing");
		_animationHash[9] = Animator.StringToHash("BS046@dead");
		_animationHash[10] = Animator.StringToHash("BS046@skill_01_start");
		_animationHash[11] = Animator.StringToHash("BS046@skill_01_loop");
		_animationHash[12] = Animator.StringToHash("BS046@skill_01_end");
		_animationHash[13] = Animator.StringToHash("BS046@skill_02_start");
		_animationHash[14] = Animator.StringToHash("BS046@skill_02_loop");
		_animationHash[15] = Animator.StringToHash("BS046@skill_02_end");
		_animationHash[16] = Animator.StringToHash("BS046@skill_03_jump_start");
		_animationHash[17] = Animator.StringToHash("BS046@skill_03_jump_loop");
		_animationHash[18] = Animator.StringToHash("BS046@skill_03_jump_atk_start");
		_animationHash[19] = Animator.StringToHash("BS046@skill_03_jump_atk_end");
		_animationHash[20] = Animator.StringToHash("BS046@skill_03_fall_loop");
		_animationHash[21] = Animator.StringToHash("BS046@skill_03_landing");
		_animationHash[24] = Animator.StringToHash("BS046@skill_04_dive_start");
		_animationHash[25] = Animator.StringToHash("BS046@skill_04_dive_loop");
		_animationHash[22] = Animator.StringToHash("BS046@skill_04_prepare_start");
		_animationHash[23] = Animator.StringToHash("BS046@skill_04_prepare_loop");
		_animationHash[26] = Animator.StringToHash("BS046@skill_04_dive_atk_loop");
		_animationHash[27] = Animator.StringToHash("BS046@skill_04_dive_atk_end");
		_animationHash[28] = Animator.StringToHash("BS046@skill_04_dive_end");
		_animationHash[29] = Animator.StringToHash("BS046@skill_05_start");
		_animationHash[30] = Animator.StringToHash("BS046@skill_05_loop");
		_animationHash[31] = Animator.StringToHash("BS046@skill_05_end");
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxstory_explode_000", 10);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("FX_BOSS_EXPLODE2");
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuseTarget", 6);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_drill-man_002");
		_mainStatus = MainStatus.Idle;
		_subStatus = SubStatus.Phase0;
		AiTimer.TimerStart();
		skill05Timer = OrangeTimerManager.GetTimer();
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
		SetStatus((MainStatus)nSet);
	}

	private void UpdateRandomState()
	{
		MainStatus mainStatus = MainStatus.Idle;
		if (StageUpdate.bIsHost)
		{
			float num = Mathf.Abs((float)TargetPos.x - _transform.position.x);
			switch (_mainStatus)
			{
			case MainStatus.Debut:
				mainStatus = MainStatus.Idle;
				break;
			case MainStatus.FindPlayer:
				mainStatus = MainStatus.Run;
				break;
			case MainStatus.Idle:
				mainStatus = (MainStatus)OrangeBattleUtility.Random(3, 10);
				if (mainStatus == MainStatus.Skill_03 && num < 4f)
				{
					mainStatus = MainStatus.Run;
				}
				break;
			case MainStatus.Run:
				mainStatus = (MainStatus)OrangeBattleUtility.Random(4, 10);
				if (mainStatus == MainStatus.Skill_03 && num < 4f)
				{
					mainStatus = MainStatus.Skill_01;
				}
				break;
			case MainStatus.Jump:
				mainStatus = (MainStatus)OrangeBattleUtility.Random(5, 8);
				if (mainStatus == MainStatus.Skill_03 && num < 4f)
				{
					mainStatus = MainStatus.Skill_01;
				}
				break;
			case MainStatus.NextAction:
				mainStatus = (MainStatus)OrangeBattleUtility.Random(3, 10);
				if (mainStatus == MainStatus.Skill_03 && num < 4f)
				{
					mainStatus = MainStatus.Skill_01;
				}
				else if (mainStatus == MainStatus.Jump)
				{
					mainStatus = MainStatus.Run;
				}
				break;
			}
		}
		else
		{
			bWaitNetStatus = false;
		}
		if (StageUpdate.gbIsNetGame)
		{
			if (StageUpdate.bIsHost)
			{
				NetSyncData netSyncData = new NetSyncData();
				netSyncData.TargetPosX = TargetPos.x;
				netSyncData.TargetPosY = TargetPos.y;
				netSyncData.TargetPosZ = TargetPos.z;
				netSyncData.SelfPosX = Controller.LogicPosition.x;
				netSyncData.SelfPosY = Controller.LogicPosition.y;
				netSyncData.SelfPosZ = Controller.LogicPosition.z;
				StageUpdate.RegisterSendAndRun(sNetSerialID, (int)mainStatus, JsonConvert.SerializeObject(netSyncData));
				_mainStatus = MainStatus.IdleWaitNet;
			}
		}
		else
		{
			SetStatus(mainStatus);
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
		if (_diveMode)
		{
			BaseUpdate();
			UpdateGravity();
			Controller.AddLogicPosition(_velocity);
			distanceDelta = Vector3.Distance(base.transform.localPosition, Controller.LogicPosition.vec3) * (Time.deltaTime / GameLogicUpdateManager.m_fFrameLen);
			_velocityExtra = VInt3.zero;
			_velocityShift = VInt3.zero;
		}
		else
		{
			base.LogicUpdate();
		}
		_currentFrame = _animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
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
					UpdateRandomState();
				}
				break;
			}
			break;
		case MainStatus.FindPlayer:
			Target = _enemyAutoAimSystem.GetClosetPlayer();
			if ((bool)Target)
			{
				_enemyAutoAimSystem.UpdateAimRange(20f);
				TargetPos = Target.Controller.LogicPosition;
				UpdateDirection();
				UpdateRandomState();
			}
			else
			{
				SetStatus(MainStatus.Run);
			}
			break;
		case MainStatus.Idle:
			if (AiTimer.GetMillisecond() > EnemyData.n_AI_TIMER)
			{
				SetStatus(MainStatus.NextAction);
			}
			break;
		case MainStatus.NextAction:
			Target = _enemyAutoAimSystem.GetClosetPlayer();
			if ((bool)Target)
			{
				TargetPos = Target.Controller.LogicPosition;
				UpdateDirection();
				UpdateRandomState();
			}
			else
			{
				SetStatus(MainStatus.FindPlayer);
			}
			break;
		case MainStatus.Run:
			if (AiTimer.GetMillisecond() > _runTime || Controller.Collisions.left || Controller.Collisions.right)
			{
				if ((bool)Target)
				{
					TargetPos = Target.Controller.LogicPosition;
					UpdateRandomState();
				}
				else
				{
					SetStatus(MainStatus.NextAction);
				}
			}
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
				if (_velocity.y < 7000)
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
					Target = _enemyAutoAimSystem.GetClosetPlayer();
					if ((bool)Target)
					{
						TargetPos = Target.Controller.LogicPosition;
						UpdateDirection();
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
		case MainStatus.Skill_01:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill_01, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill_01, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				if (_currentFrame > 0.35f && !_tfRightDrill.gameObject.activeSelf)
				{
					ReloadDrill(ReloadType.Right_Hand);
				}
				else if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.NextAction);
				}
				break;
			}
			break;
		case MainStatus.Skill_02:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill_02, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill_02, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				if ((double)_currentFrame > 0.7 && !_tfLeftDrill.gameObject.activeSelf)
				{
					ReloadDrill(ReloadType.Left_Hand);
				}
				else if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.NextAction);
				}
				break;
			}
			break;
		case MainStatus.Skill_03:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill_03, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if (_velocity.y <= 0)
				{
					SetStatus(MainStatus.Skill_03, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				if (_currentFrame > 0.5f && _tfLeftDrill.gameObject.activeSelf)
				{
					_tfLeftDrill.gameObject.SetActive(false);
					Vector3 vector = new Vector3(1.5f * (float)base.direction, -1f, 0f);
					BulletBase.TryShotBullet(EnemyWeapons[2].BulletData, _tfShootPointL.position, vector.normalized, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
				}
				else if (_currentFrame > 1.2f)
				{
					SetStatus(MainStatus.Skill_03, SubStatus.Phase3);
				}
				break;
			case SubStatus.Phase3:
				if (Controller.Collisions.below)
				{
					SetStatus(MainStatus.Skill_03, SubStatus.Phase4);
				}
				break;
			case SubStatus.Phase4:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.NextAction);
				}
				break;
			}
			break;
		case MainStatus.Skill_04:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (_currentFrame > 1f)
				{
					PlaySE("BossSE03", "bs024_drill02");
					SetStatus(MainStatus.Skill_04, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if (AiTimer.GetMillisecond() < 500)
				{
					_velocity.y = -30;
				}
				else if (AiTimer.GetMillisecond() < 2500)
				{
					if (base.AllowAutoAim && AiTimer.GetMillisecond() > 1500)
					{
						base.AllowAutoAim = false;
						base.VanishStatus = true;
					}
					_velocity.y = -100;
				}
				else
				{
					SetStatus(MainStatus.Skill_04, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill_04, SubStatus.Phase3);
				}
				break;
			case SubStatus.Phase3:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill_04, SubStatus.Phase4);
				}
				break;
			case SubStatus.Phase4:
				if (_currentFrame > 1f && AiTimer.GetMillisecond() > _runTime)
				{
					SetStatus(MainStatus.Skill_04, SubStatus.Phase5);
				}
				else if ((bool)Target && Target.Controller.Collisions.below && AiTimer.GetMillisecond() < _runTime - 500)
				{
					_divePosition = Target.Controller.LogicPosition;
				}
				break;
			case SubStatus.Phase5:
				if (_diveMode)
				{
					if (!base.AllowAutoAim && Controller.LogicPosition.y > _divePosition.y - 500)
					{
						base.AllowAutoAim = true;
						base.VanishStatus = false;
						Vector3 position = _transform.position;
						position.y = _divePosition.vec3.y;
						MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_drill-man_002", position, Quaternion.Euler(0f, 0f, 0f), Array.Empty<object>());
					}
					if (Controller.LogicPosition.y > _divePosition.y)
					{
						_velocity.y = _jupmpSpeed.y * 1000;
						_diveMode = false;
						IgnoreGravity = false;
					}
				}
				else if (!_diveMode && _velocity.y <= 7000)
				{
					SetStatus(MainStatus.Skill_04, SubStatus.Phase6);
				}
				break;
			case SubStatus.Phase6:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Jump, SubStatus.Phase3);
				}
				break;
			}
			break;
		case MainStatus.Skill_05:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (_currentFrame > 1f)
				{
					_drillReloadCompleted = true;
					SetStatus(MainStatus.Skill_05, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if (_currentFrame > 1f)
				{
					_skill05Count--;
					if (_skill05Count > 0)
					{
						SetStatus(MainStatus.Skill_05, SubStatus.Phase1);
					}
					else
					{
						SetStatus(MainStatus.Skill_05, SubStatus.Phase2);
					}
				}
				break;
			case SubStatus.Phase2:
				if (_currentFrame > 1f && _drillReloadCompleted)
				{
					SetStatus(MainStatus.Skill_05, SubStatus.Phase3);
				}
				break;
			case SubStatus.Phase3:
				if (_currentFrame > 1f)
				{
					IsInvincible = false;
					SetStatus(MainStatus.NextAction);
				}
				break;
			}
			break;
		case MainStatus.Die:
			if (_diveMode && Controller.LogicPosition.y > _divePosition.y + 100)
			{
				_diveMode = false;
				_velocity.y = 0;
				IgnoreGravity = false;
			}
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (_currentFrame > 0.5f)
				{
					SetStatus(MainStatus.Die, SubStatus.Phase1);
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
			if (skill05Timer.IsStarted() && skill05Timer.GetMillisecond() > 3000)
			{
				PlaySE("WeaponSE", "wep_fly_missile01_stop");
				skill05Timer.TimerStop();
			}
		}
	}

	public override void SetActive(bool isActive)
	{
		if (!isActive)
		{
			LeanTween.cancel(base.gameObject);
		}
		base.SetActive(isActive);
		if (isActive)
		{
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
			StageUpdate.SlowStage();
			SetColliderEnable(false);
			SetStatus(MainStatus.Die);
		}
	}

	private void SetStatus(MainStatus mainStatus, SubStatus subStatus = SubStatus.Phase0)
	{
		_mainStatus = mainStatus;
		_subStatus = subStatus;
		switch (_mainStatus)
		{
		case MainStatus.Debut:
		{
			SubStatus subStatus2 = _subStatus;
			if (subStatus2 == SubStatus.Phase1 && IntroCallBack != null)
			{
				IntroCallBack();
			}
			break;
		}
		case MainStatus.Idle:
			IsInvincible = false;
			_velocity.x = 0;
			break;
		case MainStatus.FindPlayer:
			IsInvincible = false;
			_enemyAutoAimSystem.UpdateAimRange(200f);
			break;
		case MainStatus.Run:
			IsInvincible = false;
			_runTime = OrangeBattleUtility.Random(500, 2000);
			_velocity.x = base.direction * _runSpeed * 1000;
			break;
		case MainStatus.Jump:
			IsInvincible = false;
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_velocity = VInt3.zero;
				break;
			case SubStatus.Phase1:
				_velocity.x = base.direction * _jupmpSpeed.x * 1000;
				_velocity.y = _jupmpSpeed.y * 1000;
				break;
			case SubStatus.Phase4:
				_velocity = VInt3.zero;
				break;
			}
			break;
		case MainStatus.Skill_01:
			IsInvincible = false;
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_velocity = VInt3.zero;
				_efx_Skill01.Play();
				Target = _enemyAutoAimSystem.GetClosetPlayer();
				if ((bool)Target)
				{
					TargetPos = Target.Controller.LogicPosition;
					UpdateDirection();
				}
				break;
			case SubStatus.Phase2:
				_tfRightDrill.gameObject.SetActive(false);
				_efx_Skill01.Stop();
				BulletBase.TryShotBullet(EnemyWeapons[1].BulletData, _tfShootPointR.position, base.direction * Vector3.right, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
				break;
			}
			break;
		case MainStatus.Skill_02:
			IsInvincible = false;
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_velocity = VInt3.zero;
				Target = _enemyAutoAimSystem.GetClosetPlayer();
				if ((bool)Target)
				{
					TargetPos = Target.Controller.LogicPosition;
					UpdateDirection();
				}
				break;
			case SubStatus.Phase2:
				_tfLeftDrill.gameObject.SetActive(false);
				BulletBase.TryShotBullet(EnemyWeapons[2].BulletData, _tfShootPointL.position, base.direction * Vector3.right, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
				break;
			}
			break;
		case MainStatus.Skill_03:
			IsInvincible = false;
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_velocity = VInt3.zero;
				break;
			case SubStatus.Phase1:
				_velocity.y = _jupmpSpeed.y * 1000;
				break;
			case SubStatus.Phase2:
				IgnoreGravity = true;
				break;
			case SubStatus.Phase3:
				IgnoreGravity = false;
				ReloadDrill(ReloadType.Left_Hand);
				break;
			case SubStatus.Phase4:
				_velocity = VInt3.zero;
				break;
			}
			break;
		case MainStatus.Skill_04:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				IsInvincible = true;
				_divePosition = Controller.LogicPosition;
				_velocity = VInt3.zero;
				break;
			case SubStatus.Phase1:
				_diveMode = true;
				IgnoreGravity = true;
				_velocity.y = -30;
				break;
			case SubStatus.Phase2:
				if ((bool)Target)
				{
					VInt3 logicPosition = Target.Controller.LogicPosition;
					logicPosition.y = (Target.Controller.Collisions.below ? (Target.Controller.LogicPosition.y + 500) : (_divePosition.y + 500));
					RaycastHit2D raycastHit2D3 = Physics2D.Raycast(logicPosition.vec3, Vector2.left, 5f, Controller.collisionMask);
					RaycastHit2D raycastHit2D4 = Physics2D.Raycast(logicPosition.vec3, Vector2.right, 5f, Controller.collisionMask);
					float num = (raycastHit2D3 ? (logicPosition.vec3.x - raycastHit2D3.distance + 1f) : (logicPosition.vec3.x - 5f));
					float num2 = (raycastHit2D4 ? (logicPosition.vec3.x + raycastHit2D4.distance - 1f) : (logicPosition.vec3.x + 5f));
					CameraControl component = MonoBehaviourSingleton<OrangeSceneManager>.Instance.MainCamera.GetComponent<CameraControl>();
					if (num < component.CurrentLockRange.MinX - ManagedSingleton<StageHelper>.Instance.fCameraWHalf + 1f)
					{
						num = component.CurrentLockRange.MinX - ManagedSingleton<StageHelper>.Instance.fCameraWHalf + 1f;
					}
					if (num2 > component.CurrentLockRange.MaxX + ManagedSingleton<StageHelper>.Instance.fCameraWHalf - 1f)
					{
						num2 = component.CurrentLockRange.MaxX + ManagedSingleton<StageHelper>.Instance.fCameraWHalf - 1f;
					}
					_divePosition.x = Mathf.RoundToInt(OrangeBattleUtility.Random(num, num2) * 1000f);
					_divePosition.y = (Target.Controller.Collisions.below ? Target.Controller.LogicPosition.y : _divePosition.y);
					PlayBossSE("BossSE03", "bs024_drill03");
				}
				Controller.LogicPosition = _divePosition;
				_velocity = VInt3.zero;
				break;
			case SubStatus.Phase3:
				base.AllowAutoAim = true;
				break;
			case SubStatus.Phase4:
				base.AllowAutoAim = false;
				_runTime = OrangeBattleUtility.Random(1500, 2500);
				break;
			case SubStatus.Phase5:
			{
				VInt3 divePosition = _divePosition;
				divePosition.y += 500;
				RaycastHit2D raycastHit2D = Physics2D.Raycast(divePosition.vec3, Vector2.left, 5f, Controller.collisionMask);
				RaycastHit2D raycastHit2D2 = Physics2D.Raycast(divePosition.vec3, Vector2.right, 5f, Controller.collisionMask);
				if ((bool)raycastHit2D && raycastHit2D.distance < 0.5f)
				{
					_divePosition.x += 500;
				}
				if ((bool)raycastHit2D2 && raycastHit2D2.distance < 0.5f)
				{
					_divePosition.x -= 500;
				}
				Controller.SetLogicPosition(_divePosition - VInt3.up * 3);
				_velocity.y = _jupmpSpeed.y;
				IsInvincible = false;
				PlayBossSE("BossSE03", "bs024_drill04");
				break;
			}
			}
			break;
		case MainStatus.Skill_05:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				IsInvincible = true;
				_velocity = VInt3.zero;
				_skill05Count = 6;
				break;
			case SubStatus.Phase1:
				_tfRightDrill.gameObject.SetActive(false);
				PlaySE("WeaponSE", "wep_fly_misile04");
				BulletBase.TryShotBullet(EnemyWeapons[5].BulletData, _tfShootPointR, Vector3.up, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
				break;
			case SubStatus.Phase2:
				ReloadDrill(ReloadType.Right_Hand, 0.15f);
				break;
			case SubStatus.Phase3:
			{
				Transform transform = MonoBehaviourSingleton<OrangeSceneManager>.Instance.MainCamera.transform;
				Vector3[] array = new Vector3[14]
				{
					new Vector3(transform.position.x - 12f, _transform.position.y + 5f, _transform.position.z),
					new Vector3(transform.position.x + 12f, _transform.position.y + 5f, _transform.position.z),
					new Vector3(transform.position.x + 12f, _transform.position.y + 1.5f, _transform.position.z),
					new Vector3(transform.position.x - 12f, _transform.position.y + 1.5f, _transform.position.z),
					new Vector3(transform.position.x - 3f, _transform.position.y + 12f, _transform.position.z),
					new Vector3(transform.position.x - 3f, _transform.position.y - 12f, _transform.position.z),
					new Vector3(transform.position.x + 3f, _transform.position.y + 12f, _transform.position.z),
					new Vector3(transform.position.x + 3f, _transform.position.y - 12f, _transform.position.z),
					new Vector3(transform.position.x - 12f, _transform.position.y + 10.5f, _transform.position.z),
					new Vector3(transform.position.x + 12f, _transform.position.y - 4f, _transform.position.z),
					new Vector3(transform.position.x - 12f, _transform.position.y - 4f, _transform.position.z),
					new Vector3(transform.position.x + 12f, _transform.position.y + 10.5f, _transform.position.z),
					new Vector3(transform.position.x, _transform.position.y + 12f, _transform.position.z),
					new Vector3(transform.position.x, _transform.position.y - 12f, _transform.position.z)
				};
				for (int i = 0; i < 14; i += 2)
				{
					Vector3 vector = (array[i].xy() - array[i + 1].xy()).normalized;
					MonoBehaviourSingleton<FxManager>.Instance.PlayReturn<psSwingTarget>("fxuseTarget", array[i], Quaternion.Euler(0f, 0f, Vector2.SignedAngle(Vector2.right, -vector)), Array.Empty<object>()).SetEffect(24f, new Color(1f, 0.2f, 0.8f, 0.7f), new Color(1f, 0.2f, 0.8f), 1f);
					BulletBase.TryShotBullet(EnemyWeapons[5].BulletData, array[i], -vector, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
				}
				PlaySE("WeaponSE", "wep_fly_missile01_lp");
				skill05Timer.TimerStart();
				break;
			}
			}
			break;
		case MainStatus.Die:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				base.AllowAutoAim = false;
				_collideBullet.BackToPool();
				_velocity.x = 0;
				OrangeBattleUtility.LockPlayer();
				if (!Controller.Collisions.below)
				{
					IgnoreGravity = true;
					SetStatus(MainStatus.Die, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase1:
			{
				AI_STATE aiState = AiState;
				if ((uint)(aiState - 1) <= 1u)
				{
					StartCoroutine(BossDieFlow(base.AimTransform, "FX_BOSS_EXPLODE2", false, false));
				}
				else
				{
					StartCoroutine(BossDieFlow(base.AimTransform));
				}
				break;
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
		case MainStatus.Run:
			_currentAnimationId = AnimationID.ANI_RUN_LOOP;
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
		case MainStatus.Skill_01:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_SKILL_01_START;
				_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_SKILL_01_LOOP;
				_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_SKILL_01_END;
				_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
				break;
			}
			break;
		case MainStatus.Skill_02:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_SKILL_02_START;
				_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_SKILL_02_LOOP;
				_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_SKILL_02_END;
				_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
				break;
			}
			break;
		case MainStatus.Skill_03:
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
				_currentAnimationId = AnimationID.ANI_SKILL_03_FALL;
				_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
				break;
			case SubStatus.Phase4:
				_currentAnimationId = AnimationID.ANI_SKILL_03_LANDING;
				_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
				break;
			}
			break;
		case MainStatus.Skill_04:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_SKILL_04_PREPARE_START;
				_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_SKILL_04_PREPARE_LOOP;
				_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_SKILL_04_DIVE_START;
				_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
				break;
			case SubStatus.Phase3:
				_currentAnimationId = AnimationID.ANI_SKILL_04_DIVE_LOOP;
				_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
				break;
			case SubStatus.Phase4:
				_currentAnimationId = AnimationID.ANI_SKILL_04_DIVE_END;
				_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
				break;
			case SubStatus.Phase5:
				_currentAnimationId = AnimationID.ANI_SKILL_04_DIVE_ATK_LOOP;
				_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
				break;
			case SubStatus.Phase6:
				_currentAnimationId = AnimationID.ANI_SKILL_04_DIVE_ATK_END;
				_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
				break;
			}
			break;
		case MainStatus.Skill_05:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_SKILL_05_START;
				_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_SKILL_05_LOOP;
				_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
				break;
			case SubStatus.Phase3:
				_currentAnimationId = AnimationID.ANI_SKILL_05_END;
				_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
				break;
			case SubStatus.Phase2:
				break;
			}
			break;
		case MainStatus.Die:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_DEAD;
				_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_HURT_LOOP;
				_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
				break;
			}
			break;
		case MainStatus.FindPlayer:
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
	}

	private void ReloadDrill(ReloadType type, float time = 0.25f)
	{
		_drillReloadCompleted = false;
		switch (type)
		{
		case ReloadType.Right_Hand:
			_tfRightDrill.gameObject.SetActive(true);
			LeanTween.value(base.gameObject, _tfRightDrillRootBone.localPosition.x + 0.25f, _tfRightDrillRootBone.localPosition.x, 0.25f).setOnUpdate(delegate(float f)
			{
				_tfRightDrillRootBone.localPosition = new Vector3(f, _tfRightDrillRootBone.localPosition.y, _tfRightDrillRootBone.localPosition.z);
			}).setOnComplete((Action)delegate
			{
				_drillReloadCompleted = true;
			});
			break;
		case ReloadType.Left_Hand:
			_tfLeftDrill.gameObject.SetActive(true);
			LeanTween.value(base.gameObject, _tfLeftDrillRootBone.localPosition.x + 0.25f, _tfLeftDrillRootBone.localPosition.x, 0.25f).setOnUpdate(delegate(float f)
			{
				_tfLeftDrillRootBone.localPosition = new Vector3(f, _tfLeftDrillRootBone.localPosition.y, _tfLeftDrillRootBone.localPosition.z);
			}).setOnComplete((Action)delegate
			{
				_drillReloadCompleted = true;
			});
			break;
		case ReloadType.Head:
			_tfHeadDrill.gameObject.SetActive(true);
			LeanTween.value(base.gameObject, _tfHeadDrillRootBone.localPosition.x + 0.25f, _tfHeadDrillRootBone.localPosition.x, 0.25f).setOnUpdate(delegate(float f)
			{
				_tfHeadDrillRootBone.localPosition = new Vector3(f, _tfHeadDrillRootBone.localPosition.y, _tfHeadDrillRootBone.localPosition.z);
			}).setOnComplete((Action)delegate
			{
				_drillReloadCompleted = true;
			});
			break;
		}
	}

	public override void UpdateEnemyID(int id)
	{
		base.UpdateEnemyID(id);
		UpdateAIState();
		AI_STATE aiState = AiState;
		if (aiState == AI_STATE.mob_003)
		{
			base.DeadPlayCompleted = true;
		}
	}
}
