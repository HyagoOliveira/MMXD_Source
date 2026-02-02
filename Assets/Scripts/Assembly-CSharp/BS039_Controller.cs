#define RELEASE
using System;
using System.Collections;
using CallbackDefs;
using NaughtyAttributes;
using Newtonsoft.Json;
using StageLib;
using UnityEngine;

public class BS039_Controller : EnemyControllerBase, IManagedUpdateBehavior
{
	public enum MainStatus
	{
		Idle = 0,
		Debut = 1,
		Hurt = 2,
		Dead = 3,
		Jump = 4,
		ComboSwitch = 5,
		Skill0 = 6,
		Skill1 = 7,
		Skill2 = 8,
		Skill3 = 9,
		Skill4 = 10,
		Skill5 = 11,
		IdleWaitNet = 12,
		IdleChip = 13,
		MAX_STATUS = 14
	}

	private enum SubStatus
	{
		Phase0 = 0,
		Phase1 = 1,
		Phase2 = 2,
		Phase3 = 3,
		Phase4 = 4,
		MAX_SUBSTATUS = 5
	}

	public enum AnimationID
	{
		ANI_IDLE = 0,
		ANI_DEBUT = 1,
		ANI_HURT = 2,
		ANI_RUN = 3,
		ANI_JUMP_START = 4,
		ANI_JUMP = 5,
		ANI_FALL_LOOP = 6,
		ANI_LAND = 7,
		ANI_DEAD = 8,
		ANI_SKILL0_START = 9,
		ANI_SKILL0_LOOP = 10,
		ANI_SKILL0_END = 11,
		ANI_SKILL1_START = 12,
		ANI_SKILL1_LOOP = 13,
		ANI_SKILL1_END = 14,
		ANI_SKILL2_START = 15,
		ANI_SKILL2_LOOP = 16,
		ANI_SKILL2_END = 17,
		ANI_SKILL3_START = 18,
		ANI_SKILL3_LOOP = 19,
		ANI_SKILL3_END = 20,
		ANI_SKILL3_FALL = 21,
		ANI_SKILL4_START = 22,
		ANI_SKILL4_LOOP = 23,
		ANI_SKILL4_END = 24,
		ANI_SKILL5_START = 25,
		ANI_SKILL5_LOOP = 26,
		ANI_SKILL5_END = 27,
		ANI_SKILL6_START = 28,
		ANI_SKILL6_LOOP = 29,
		ANI_SKILL6_END = 30,
		ANI_SKILL7_START = 31,
		ANI_SKILL7_LOOP = 32,
		ANI_SKILL7_END = 33,
		ANI_SKILL8_START = 34,
		ANI_SKILL8_LOOP = 35,
		ANI_SKILL8_END = 36,
		MAX_ANIMATION_ID = 37
	}

	private MainStatus[] randomStatus_after_idle = new MainStatus[3]
	{
		MainStatus.Jump,
		MainStatus.Skill2,
		MainStatus.Skill4
	};

	private MainStatus[] randomStatus_after_jump = new MainStatus[3]
	{
		MainStatus.Skill2,
		MainStatus.Skill4,
		MainStatus.ComboSwitch
	};

	private MainStatus[] randomStatus_after_skl02 = new MainStatus[1] { MainStatus.ComboSwitch };

	private MainStatus[] randomStatus_after_skl04 = new MainStatus[3]
	{
		MainStatus.Skill2,
		MainStatus.Skill2,
		MainStatus.Skill4
	};

	private MainStatus[] comboStatus_0_0 = new MainStatus[3]
	{
		MainStatus.Skill0,
		MainStatus.Skill1,
		MainStatus.Jump
	};

	private MainStatus[] comboStatus_0_1 = new MainStatus[4]
	{
		MainStatus.Skill1,
		MainStatus.Skill0,
		MainStatus.Skill3,
		MainStatus.Jump
	};

	private MainStatus[] comboStatus_1_0 = new MainStatus[5]
	{
		MainStatus.Skill0,
		MainStatus.Skill1,
		MainStatus.Skill0,
		MainStatus.Skill4,
		MainStatus.Skill5
	};

	private MainStatus[] comboStatus_1_1 = new MainStatus[5]
	{
		MainStatus.Skill2,
		MainStatus.Skill1,
		MainStatus.Skill2,
		MainStatus.Skill3,
		MainStatus.Jump
	};

	private bool isCombo;

	private int nowComboSklIdx;

	private MainStatus[] nowUseCombo;

	public VInt2 UpperPunchSpeed = new VInt2(10000, 13000);

	public VInt2 JumpKickUpSpeed = new VInt2(2000, 14000);

	public VInt2 JumpKickDownSpeed = new VInt2(18000, 14000);

	private int JumpSpeed = 20000;

	private int JumpSpeedX = 14500;

	private readonly int _hashVspd = Animator.StringToHash("fVspd");

	private CollideBullet _collideBulletSkill2;

	private CollideBullet _collideBulletSkill3;

	private ParticleSystem _skill2ParticleSystem;

	private ParticleSystem _skill3ParticleSystem;

	private ParticleSystem _skill5ParticleSystem;

	private int[] _animatorHash;

	[SerializeField]
	[NaughtyAttributes.ReadOnly]
	private MainStatus _previousStatus;

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

	[SerializeField]
	private bool _enableDebugStatus;

	[SerializeField]
	private MainStatus _debugStatus = MainStatus.Skill2;

	public float DebugShift = 8f;

	private float StageLeftPosX;

	private int iStageLeftPosX;

	private int iStageLeftPosX_Jump;

	private float StageRightPosX;

	private int iStageRightPosX;

	private int iStageRightPosX_Jump;

	private Transform _mouthShootTransform;

	private Transform _hadokenShootTransform;

	private Transform _handShootTransform;

	private Transform _footShootTransform;

	private OrangeTimer _summonTimer;

	private bool _bDeadCallResult = true;

	private VInt3 jumpEndPos;

	private bool IsChipInfoAnim;

	private int nDeadCount;

	private bool bHasActed;

	private bool bHasSwitch;

	private BulletBase MHRBullet;

	[SerializeField]
	private float fSkill5CDTime = 25f;

	private int nSkill5CDFrame;

	private Vector3 distance;

	private int useWeapon;

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
		base.AimTransform = OrangeBattleUtility.FindChildRecursive(ref target, "BodyCollider");
		_collideBullet = OrangeBattleUtility.FindChildRecursive(ref target, "BodyCollider").gameObject.AddOrGetComponent<CollideBullet>();
		_mouthShootTransform = OrangeBattleUtility.FindChildRecursive(ref target, "ShootPoint_Mouth");
		_hadokenShootTransform = OrangeBattleUtility.FindChildRecursive(ref target, "ShootPoint_Handoken");
		_handShootTransform = OrangeBattleUtility.FindChildRecursive(ref target, "ShootPoint_Hand_R");
		_footShootTransform = OrangeBattleUtility.FindChildRecursive(ref target, "ShootPoint_Foot_L");
		_collideBulletSkill2 = _footShootTransform.gameObject.AddOrGetComponent<CollideBullet>();
		_collideBulletSkill3 = _handShootTransform.gameObject.AddOrGetComponent<CollideBullet>();
		_skill2ParticleSystem = OrangeBattleUtility.FindChildRecursive(ref target, "fxduring_skill2", true).GetComponent<ParticleSystem>();
		_skill3ParticleSystem = OrangeBattleUtility.FindChildRecursive(ref target, "fxduring_skill3", true).GetComponent<ParticleSystem>();
		_skill5ParticleSystem = OrangeBattleUtility.FindChildRecursive(ref target, "fxduring_skill5", true).GetComponent<ParticleSystem>();
		_animator = GetComponentInChildren<Animator>();
		_animatorHash = new int[37];
		for (int i = 0; i < 37; i++)
		{
			_animatorHash[i] = Animator.StringToHash("BS039@idle_loop");
		}
		_animatorHash[0] = Animator.StringToHash("BS039@idle_loop");
		_animatorHash[4] = Animator.StringToHash("BS039@jump_start");
		_animatorHash[5] = Animator.StringToHash("BS039@jump_loop");
		_animatorHash[6] = Animator.StringToHash("BS039@fall_loop");
		_animatorHash[7] = Animator.StringToHash("BS039@landing");
		_animatorHash[1] = Animator.StringToHash("BS039@debut");
		_animatorHash[2] = Animator.StringToHash("BS039@hurt_loop");
		_animatorHash[8] = Animator.StringToHash("BS039@dead");
		_animatorHash[3] = Animator.StringToHash("BS039@run_forward_loop");
		_animatorHash[9] = Animator.StringToHash("BS039@skill_01_stand_atk_start");
		_animatorHash[10] = Animator.StringToHash("BS039@skill_01_stand_atk_loop");
		_animatorHash[11] = Animator.StringToHash("BS039@skill_01_stand_atk_end");
		_animatorHash[12] = Animator.StringToHash("BS039@skill_01_crouch_atk_start");
		_animatorHash[13] = Animator.StringToHash("BS039@skill_01_crouch_atk_loop");
		_animatorHash[14] = Animator.StringToHash("BS039@skill_01_crouch_atk_end");
		_animatorHash[15] = Animator.StringToHash("BS039@skill_02_jump_start");
		_animatorHash[16] = Animator.StringToHash("BS039@skill_02_jump_loop");
		_animatorHash[17] = Animator.StringToHash("BS039@skill_02_atk_landing");
		_animatorHash[18] = Animator.StringToHash("BS039@skill_03_rising_start");
		_animatorHash[19] = Animator.StringToHash("BS039@skill_03_rising_loop");
		_animatorHash[20] = Animator.StringToHash("BS039@skill_03_rising_to_fall");
		_animatorHash[21] = Animator.StringToHash("BS039@skill_03_fall_loop");
		_animatorHash[22] = Animator.StringToHash("BS039@skill_04_atk_start");
		_animatorHash[23] = Animator.StringToHash("BS039@skill_04_atk_loop");
		_animatorHash[24] = Animator.StringToHash("BS039@skill_04_atk_end");
		_animatorHash[25] = Animator.StringToHash("BS039@skill_05_atk_start");
		_animatorHash[26] = Animator.StringToHash("BS039@skill_05_atk_loop");
		_animatorHash[27] = Animator.StringToHash("BS039@skill_05_atk_end");
		_summonTimer = OrangeTimerManager.GetTimer();
		SetStatus(MainStatus.Debut);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("FX_BOSS_EXPLODE2");
	}

	public override void SetChipInfoAnim()
	{
		SetStatus(MainStatus.IdleChip);
		IsChipInfoAnim = true;
		UpdateAnimation();
	}

	public override void UpdateEnemyID(int id)
	{
		base.UpdateEnemyID(id);
		if (null == _enemyAutoAimSystem)
		{
			OrangeBattleUtility.AddEnemyAutoAimSystem(_transform, out _enemyAutoAimSystem);
		}
		_enemyAutoAimSystem.UpdateAimRange(50f);
		UpdateAIState();
		switch (AiState)
		{
		case AI_STATE.mob_002:
		case AI_STATE.mob_004:
			_bDeadCallResult = false;
			break;
		case AI_STATE.mob_005:
			comboStatus_1_0 = new MainStatus[4]
			{
				MainStatus.Skill0,
				MainStatus.Skill1,
				MainStatus.Skill0,
				MainStatus.Skill4
			};
			_bDeadCallResult = false;
			break;
		default:
			_bDeadCallResult = true;
			break;
		}
		WeaponStruct[] enemyWeapons = EnemyWeapons;
		for (int i = 0; i < enemyWeapons.Length; i++)
		{
			SKILL_TABLE bulletData = enemyWeapons[i].BulletData;
			if (bulletData.n_MAGAZINE > 5 && !ManagedSingleton<OrangeTableHelper>.Instance.IsDummyOrEmpty(bulletData.s_MODEL))
			{
				MonoBehaviourSingleton<PoolManager>.Instance.ExpandPoolItem<PoolBaseObject>(bulletData.s_MODEL, 15);
			}
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
			_collideBulletSkill2.UpdateBulletData(EnemyWeapons[2].BulletData);
			_collideBulletSkill2.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			_collideBulletSkill3.UpdateBulletData(EnemyWeapons[3].BulletData);
			_collideBulletSkill3.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			if (AiState == AI_STATE.mob_002)
			{
				_summonTimer.TimerStart();
			}
			else
			{
				_summonTimer.TimerStop();
			}
			if (EnemyData.s_MODEL == "enemy_bs118")
			{
				bHasSwitch = false;
				base.SoundSource.PlaySE("BossSE02", "bs015_magma10");
			}
		}
		else
		{
			_collideBullet.BackToPool();
			_collideBulletSkill2.BackToPool();
			_collideBulletSkill3.BackToPool();
			_summonTimer.TimerStop();
		}
	}

	public void UpdateFunc()
	{
		if (Activate || _mainStatus == MainStatus.Debut || (_mainStatus == MainStatus.Dead && _subStatus == SubStatus.Phase3))
		{
			float value = _velocity.vec3.y + OrangeBattleUtility.Gravity * Time.deltaTime;
			value = (float)Math.Sign(value) * Math.Min(Math.Abs(value), Math.Abs(_maxGravity.scalar));
			_animator.SetFloat(_hashVspd, value);
			base.transform.localPosition = Vector3.MoveTowards(base.transform.localPosition, Controller.LogicPosition.vec3, distanceDelta);
		}
	}

	private void SetStatus(MainStatus mainStatus, SubStatus subStatus = SubStatus.Phase0)
	{
		if (IsChipInfoAnim)
		{
			return;
		}
		if (mainStatus == MainStatus.ComboSwitch)
		{
			isCombo = true;
			nowComboSklIdx = -1;
			if (OrangeBattleUtility.Random(1, 100) % 2 == 0)
			{
				nowUseCombo = (((float)(int)Hp / (float)(int)MaxHp >= 0.5f) ? comboStatus_0_0 : comboStatus_1_0);
			}
			else
			{
				nowUseCombo = (((float)(int)Hp / (float)(int)MaxHp >= 0.5f) ? comboStatus_0_1 : comboStatus_1_1);
			}
			UpdateRandomState();
			return;
		}
		_previousStatus = _mainStatus;
		_mainStatus = mainStatus;
		_subStatus = subStatus;
		if (EnemyWeapons != null)
		{
			UpdateMagazine(-1, true);
		}
		switch (_mainStatus)
		{
		case MainStatus.Debut:
		{
			SubStatus subStatus2 = _subStatus;
			if (subStatus2 == SubStatus.Phase3 && IntroCallBack != null)
			{
				IntroCallBack();
			}
			break;
		}
		case MainStatus.Idle:
			Target = _enemyAutoAimSystem.GetClosetPlayer();
			_velocity.x = 0;
			UpdateDirection();
			UpdateRandomState();
			break;
		case MainStatus.Jump:
			switch (subStatus)
			{
			case SubStatus.Phase0:
			{
				base.SoundSource.PlaySE("BossSE02", "bs015_magma01");
				SkillFlag = false;
				VInt3 logicPosition = Controller.LogicPosition;
				int num = IntMath.Abs(logicPosition.x - iStageLeftPosX_Jump);
				int num2 = IntMath.Abs(logicPosition.x - iStageRightPosX_Jump);
				jumpEndPos = new VInt3((num > num2) ? iStageLeftPosX_Jump : iStageRightPosX_Jump, logicPosition.y, logicPosition.z);
				UpdateDirection(Math.Sign(jumpEndPos.x - Controller.LogicPosition.x));
				break;
			}
			case SubStatus.Phase1:
				_velocity.x = JumpSpeedX * base.direction;
				_velocity.y = JumpSpeed;
				break;
			case SubStatus.Phase2:
				_velocity.x = 0;
				break;
			}
			break;
		case MainStatus.Skill0:
		case MainStatus.Skill1:
			if (subStatus == SubStatus.Phase0)
			{
				base.SoundSource.PlaySE("BossSE02", "bs015_magma03");
				useWeapon = 1;
				EnemyWeapons[useWeapon].MagazineRemain = 3f;
			}
			break;
		case MainStatus.Skill2:
			switch (subStatus)
			{
			case SubStatus.Phase0:
				base.SoundSource.PlaySE("BossSE02", "bs015_magma01");
				SkillFlag = false;
				break;
			case SubStatus.Phase2:
				_velocity.x = 0;
				break;
			case SubStatus.Phase3:
				base.SoundSource.PlaySE("BossSE02", "bs015_magma04");
				Target = _enemyAutoAimSystem.GetClosetPlayer();
				UpdateDirection();
				_velocity.x = base.direction * JumpKickDownSpeed.x;
				_velocity.y = -JumpKickDownSpeed.y;
				_maxGravity = JumpKickDownSpeed.y;
				_skill2ParticleSystem.gameObject.SetActive(true);
				_skill2ParticleSystem.Play();
				_collideBullet.BackToPool();
				_collideBulletSkill2.Active(targetMask);
				CheckOutRange();
				break;
			case SubStatus.Phase4:
				base.SoundSource.PlaySE("BossSE02", "bs015_magma02");
				_skill2ParticleSystem.Stop();
				_skill2ParticleSystem.gameObject.SetActive(false);
				_collideBulletSkill2.BackToPool();
				_collideBullet.Active(targetMask);
				_maxGravity = OrangeBattleUtility.FP_MaxGravity;
				break;
			}
			break;
		case MainStatus.Skill3:
			switch (subStatus)
			{
			case SubStatus.Phase0:
				base.SoundSource.PlaySE("BossSE02", "bs015_magma05");
				SkillFlag = false;
				_collideBullet.BackToPool();
				break;
			case SubStatus.Phase2:
				_skill3ParticleSystem.Stop();
				_skill3ParticleSystem.gameObject.SetActive(false);
				IgnoreGravity = true;
				break;
			case SubStatus.Phase3:
				_collideBulletSkill3.BackToPool();
				_collideBullet.Active(targetMask);
				break;
			case SubStatus.Phase4:
				base.SoundSource.PlaySE("BossSE02", "bs015_magma02");
				break;
			}
			break;
		case MainStatus.Skill5:
			switch (subStatus)
			{
			case SubStatus.Phase1:
				IsInvincible = true;
				_skill5ParticleSystem.Play();
				base.AllowAutoAim = false;
				break;
			case SubStatus.Phase3:
				IsInvincible = false;
				_skill5ParticleSystem.Stop();
				base.AllowAutoAim = true;
				break;
			}
			break;
		case MainStatus.Dead:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (!Controller.Collisions.below)
				{
					IgnoreGravity = true;
				}
				_summonTimer.TimerStop();
				_collideBullet.BackToPool();
				base.AllowAutoAim = false;
				_velocity.x = 0;
				OrangeBattleUtility.LockPlayer();
				if (AiState != AI_STATE.mob_002 && AiState != AI_STATE.mob_004 && EnemyData.s_MODEL != "enemy_bs118")
				{
					base.DeadPlayCompleted = true;
				}
				_currentFrame = 0f;
				if (!bHasSwitch && EnemyData.s_MODEL == "enemy_bs118")
				{
					nDeadCount = 0;
					_velocity = VInt3.zero;
					bHasActed = false;
					bHasSwitch = true;
					if (!Controller.Collisions.below)
					{
						IgnoreGravity = false;
						SetStatus(MainStatus.Dead, SubStatus.Phase3);
					}
					else
					{
						SetStatus(MainStatus.Dead, SubStatus.Phase4);
					}
					return;
				}
				break;
			case SubStatus.Phase2:
				if (_bDeadCallResult)
				{
					StartCoroutine(BossDieFlow(base.AimTransform));
				}
				else
				{
					StartCoroutine(BossDieFlow(base.AimTransform, "FX_BOSS_EXPLODE2", false, false));
				}
				break;
			case SubStatus.Phase4:
				base.DeadPlayCompleted = true;
				break;
			}
			break;
		}
		UpdateAnimation();
		AiTimer.TimerStart();
	}

	public override void LogicUpdate()
	{
		if (_mainStatus == MainStatus.Debut || (_mainStatus == MainStatus.Dead && _subStatus == SubStatus.Phase3))
		{
			BaseUpdate();
			UpdateGravity();
			Controller.Move((_velocity + _velocityExtra) * GameLogicUpdateManager.m_fFrameLen + _velocityShift);
			distanceDelta = Vector3.Distance(base.transform.localPosition, Controller.LogicPosition.vec3) * (Time.deltaTime / GameLogicUpdateManager.m_fFrameLen);
			_velocityExtra = VInt3.zero;
			_velocityShift = VInt3.zero;
		}
		if ((!Activate || !_enemyAutoAimSystem) && _mainStatus != MainStatus.Debut && (_mainStatus != MainStatus.Dead || _subStatus != SubStatus.Phase3))
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
				if (Controller.Collisions.below)
				{
					base.SoundSource.PlaySE("BossSE02", "bs015_magma02", 0.1f);
					base.SoundSource.PlaySE("BossSE02", "bs015_magma01", 1.3f);
					base.SoundSource.PlaySE("BossSE02", "bs015_magma00", 2.6f);
					SetStatus(_mainStatus, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if ((double)_currentFrame > 1.0)
				{
					SetStatus(_mainStatus, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				if (_introReady)
				{
					SetStatus(_mainStatus, SubStatus.Phase3);
				}
				break;
			case SubStatus.Phase3:
				if (_unlockReady)
				{
					StageLeftPosX = base.transform.position.x - 14.404f;
					StageRightPosX = base.transform.position.x + 4.10709f;
					iStageLeftPosX_Jump = new VInt(StageLeftPosX + 3f).i;
					iStageRightPosX_Jump = new VInt(StageRightPosX - 3f).i;
					iStageLeftPosX = new VInt(StageLeftPosX + 1f).i;
					iStageRightPosX = new VInt(StageRightPosX - 1f).i;
					Debug.LogFormat("{0},{1},{2}", base.transform.position.x.ToString(), StageLeftPosX, StageRightPosX);
					AI_STATE aiState = AiState;
					if (aiState == AI_STATE.mob_005)
					{
						nSkill5CDFrame = GameLogicUpdateManager.GameFrame + (int)(fSkill5CDTime * 20f);
					}
					SetStatus(MainStatus.Idle);
				}
				break;
			}
			break;
		case MainStatus.Idle:
		case MainStatus.IdleWaitNet:
			if ((int)Hp <= 0)
			{
				SetStatus(MainStatus.Dead);
			}
			break;
		case MainStatus.Jump:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if ((double)_currentFrame >= 1.0)
				{
					SetStatus(MainStatus.Jump, SubStatus.Phase1);
				}
				else if (!SkillFlag && (double)_currentFrame >= 0.5)
				{
					SkillFlag = true;
					_velocity.x += 500;
					_velocity.y += 500;
					CheckOutRange();
				}
				break;
			case SubStatus.Phase1:
				CheckOutRangeJump();
				if ((double)_currentFrame > 1.0 && Controller.Collisions.below)
				{
					SetStatus(MainStatus.Jump, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				if ((bool)Controller.BelowInBypassRange)
				{
					base.SoundSource.PlaySE("BossSE02", "bs015_magma02");
					SetStatus(MainStatus.Jump, SubStatus.Phase3);
				}
				break;
			case SubStatus.Phase3:
				if ((double)_currentFrame >= 1.0)
				{
					SetStatus(MainStatus.Idle);
				}
				break;
			}
			break;
		case MainStatus.Dead:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (nDeadCount > 5)
				{
					if (EnemyData.s_MODEL == "enemy_bs118" && !bHasActed && _currentFrame > 0.47f)
					{
						bHasActed = true;
						base.SoundSource.PlaySE("BossSE02", "bs015_magma10");
					}
				}
				else
				{
					nDeadCount++;
				}
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Dead, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Dead, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase3:
				if (Controller.Collisions.below)
				{
					if (nDeadCount > 1)
					{
						SetStatus(MainStatus.Dead, SubStatus.Phase4);
					}
					else
					{
						nDeadCount++;
					}
				}
				break;
			case SubStatus.Phase4:
				if (_currentFrame > 0.5f)
				{
					if (nDeadCount > 2)
					{
						SetStatus(MainStatus.Dead);
					}
					else
					{
						nDeadCount++;
					}
				}
				break;
			case SubStatus.Phase2:
				break;
			}
			break;
		case MainStatus.Skill0:
		case MainStatus.Skill1:
			useWeapon = 1;
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if ((double)_currentFrame >= 1.0)
				{
					SetStatus(_mainStatus, SubStatus.Phase1);
				}
				else
				{
					if (!(_currentFrame >= 0.7f))
					{
						break;
					}
					_animator.speed = 0f;
					if (EnemyWeapons[useWeapon].MagazineRemain > 0f)
					{
						if (!EnemyWeapons[useWeapon].LastUseTimer.IsStarted() || EnemyWeapons[useWeapon].LastUseTimer.GetMillisecond() > EnemyWeapons[useWeapon].BulletData.n_FIRE_SPEED)
						{
							BulletBase.TryShotBullet(EnemyWeapons[useWeapon].BulletData, _hadokenShootTransform, Vector3.right * base.direction, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
							EnemyWeapons[useWeapon].LastUseTimer.TimerStart();
							EnemyWeapons[useWeapon].MagazineRemain -= 1f;
						}
					}
					else if (EnemyWeapons[useWeapon].MagazineRemain == 0f)
					{
						_animator.speed = 1f;
					}
				}
				break;
			case SubStatus.Phase1:
				if ((double)_currentFrame >= 0.5)
				{
					SetStatus(_mainStatus, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				if ((double)_currentFrame >= 0.3)
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
				if ((double)_currentFrame >= 1.0)
				{
					SetStatus(MainStatus.Skill2, SubStatus.Phase1);
				}
				else if (_currentFrame > 0.7f && !SkillFlag)
				{
					SkillFlag = true;
					_velocity.x = JumpKickUpSpeed.x * base.direction;
					_velocity.y = JumpKickUpSpeed.y;
					CheckOutRange();
				}
				break;
			case SubStatus.Phase1:
				CheckOutRange();
				if (_velocity.y <= 0)
				{
					SetStatus(MainStatus.Skill2, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				SetStatus(MainStatus.Skill2, SubStatus.Phase3);
				break;
			case SubStatus.Phase3:
				if ((bool)Controller.BelowInBypassRange)
				{
					SetStatus(MainStatus.Skill2, SubStatus.Phase4);
				}
				else
				{
					CheckOutRange();
				}
				break;
			case SubStatus.Phase4:
				_velocity.x = 0;
				if ((double)_currentFrame >= 1.0)
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
				if ((double)_currentFrame >= 1.0 && SkillFlag)
				{
					SetStatus(_mainStatus, SubStatus.Phase1);
				}
				else if (_currentFrame > 0.66f && !SkillFlag)
				{
					SkillFlag = true;
					_velocity.x = UpperPunchSpeed.x * base.direction;
					_velocity.y = UpperPunchSpeed.y;
					_skill3ParticleSystem.gameObject.SetActive(true);
					_skill3ParticleSystem.Play();
					_collideBulletSkill3.Active(targetMask);
				}
				else
				{
					CheckOutRange();
				}
				break;
			case SubStatus.Phase1:
				if (_velocity.y <= 0)
				{
					_velocity.x = 0;
					SetStatus(MainStatus.Skill3, SubStatus.Phase2);
				}
				else
				{
					CheckOutRange();
				}
				break;
			case SubStatus.Phase2:
				if ((double)_currentFrame >= 1.0)
				{
					IgnoreGravity = false;
					SetStatus(MainStatus.Skill3, SubStatus.Phase3);
				}
				else if ((AiState == AI_STATE.mob_003 || AiState == AI_STATE.mob_004) && AiTimer.GetMillisecond() > 200)
				{
					IgnoreGravity = false;
					SetStatus(MainStatus.Skill2, SubStatus.Phase3);
				}
				break;
			case SubStatus.Phase3:
				if ((bool)Controller.BelowInBypassRange)
				{
					SetStatus(MainStatus.Skill3, SubStatus.Phase4);
				}
				break;
			case SubStatus.Phase4:
				if ((double)_currentFrame >= 1.0)
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
				if ((double)_currentFrame >= 1.0 && AiTimer.GetMillisecond() > 1500)
				{
					base.SoundSource.PlaySE("BossSE02", "bs015_magma06");
					SetStatus(_mainStatus, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				useWeapon = 4;
				if (EnemyWeapons[useWeapon].MagazineRemain > 0f && (!EnemyWeapons[useWeapon].LastUseTimer.IsStarted() || EnemyWeapons[useWeapon].LastUseTimer.GetMillisecond() > EnemyWeapons[useWeapon].BulletData.n_FIRE_SPEED))
				{
					BulletBase.TryShotBullet(EnemyWeapons[useWeapon].BulletData, _mouthShootTransform, Vector3.right * base.direction, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
					EnemyWeapons[useWeapon].LastUseTimer.TimerStart();
					EnemyWeapons[useWeapon].MagazineRemain -= 1f;
				}
				if (EnemyWeapons[useWeapon].MagazineRemain == 0f)
				{
					SetStatus(_mainStatus, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				if ((double)_currentFrame >= 1.0)
				{
					SetStatus(MainStatus.Idle);
				}
				break;
			case SubStatus.Phase3:
				break;
			}
			break;
		case MainStatus.Skill5:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if ((double)_currentFrame >= 1.0)
				{
					base.SoundSource.PlaySE("BossSE02", "bs015_magma07");
					SetStatus(_mainStatus, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
			{
				AI_STATE aiState = AiState;
				if (aiState == AI_STATE.mob_005)
				{
					useWeapon = 5;
				}
				else
				{
					useWeapon = 4;
				}
				if (EnemyWeapons[useWeapon].MagazineRemain > 0f && (!EnemyWeapons[useWeapon].LastUseTimer.IsStarted() || EnemyWeapons[useWeapon].LastUseTimer.GetMillisecond() > EnemyWeapons[useWeapon].BulletData.n_FIRE_SPEED))
				{
					BulletBase.TryShotBullet(EnemyWeapons[useWeapon].BulletData, _mouthShootTransform, Vector3.up, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
					EnemyWeapons[useWeapon].LastUseTimer.TimerStart();
					EnemyWeapons[useWeapon].MagazineRemain -= 1f;
					Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.CAMERA_SHAKE, 0.8f, false);
				}
				if (AiTimer.GetMillisecond() > 1500)
				{
					StartCoroutine("PlayFireDown");
					SetStatus(_mainStatus, SubStatus.Phase2);
				}
				break;
			}
			case SubStatus.Phase2:
				useWeapon = 5;
				if (!EnemyWeapons[useWeapon].LastUseTimer.IsStarted() || EnemyWeapons[useWeapon].LastUseTimer.GetMillisecond() > EnemyWeapons[useWeapon].BulletData.n_FIRE_SPEED)
				{
					EnemyWeapons[useWeapon].LastUseTimer.TimerStart();
					MHRBullet = BulletBase.TryShotBullet(EnemyWeapons[useWeapon].BulletData, new Vector2(OrangeBattleUtility.Random(StageLeftPosX + DebugShift, StageRightPosX + DebugShift), _transform.position.y + 10f), Vector3.down + Vector3.left, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
					Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.CAMERA_SHAKE, 0.8f, false);
					if (EnemyData.s_MODEL == "enemy_bs118")
					{
						base.SoundSource.PlaySE("BossSE02", "bs015_magma09_lg");
					}
				}
				if (AiTimer.GetMillisecond() > 6000)
				{
					if (MHRBullet != null)
					{
						MHRBullet.BackCallback = PlaySE_bs015_magma09_stop;
					}
					MHRBullet = null;
					SetStatus(_mainStatus, SubStatus.Phase3);
				}
				break;
			case SubStatus.Phase3:
			{
				AI_STATE aiState = AiState;
				if (aiState == AI_STATE.mob_005)
				{
					if ((double)_currentFrame >= 1.0)
					{
						nSkill5CDFrame = GameLogicUpdateManager.GameFrame + (int)(fSkill5CDTime * 20f);
						SetStatus(MainStatus.Idle);
					}
				}
				else if ((double)_currentFrame >= 1.0)
				{
					SetStatus(MainStatus.Idle);
				}
				break;
			}
			}
			break;
		default:
			SetStatus(MainStatus.Idle);
			break;
		case MainStatus.Hurt:
			break;
		}
	}

	private IEnumerator PlayFireDown()
	{
		int count = 0;
		while (count != 10)
		{
			if (MonoBehaviourSingleton<UpdateManager>.Instance.Pause)
			{
				yield return new WaitForEndOfFrame();
				continue;
			}
			base.SoundSource.PlaySE("BossSE02", "bs015_magma08");
			yield return new WaitForSeconds(0.8f);
			count++;
		}
	}

	private void UpdateAnimation()
	{
		switch (_mainStatus)
		{
		case MainStatus.Debut:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_JUMP;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_DEBUT;
				break;
			case SubStatus.Phase2:
			case SubStatus.Phase3:
				return;
			default:
				throw new ArgumentOutOfRangeException();
			}
			break;
		case MainStatus.Idle:
		case MainStatus.IdleWaitNet:
		case MainStatus.IdleChip:
			_currentAnimationId = AnimationID.ANI_IDLE;
			break;
		case MainStatus.Dead:
			switch (_subStatus)
			{
			default:
				return;
			case SubStatus.Phase0:
				_currentAnimationId = ((!Controller.Collisions.below) ? AnimationID.ANI_HURT : AnimationID.ANI_DEAD);
				break;
			case SubStatus.Phase3:
				_currentAnimationId = AnimationID.ANI_HURT;
				break;
			case SubStatus.Phase4:
				_currentAnimationId = AnimationID.ANI_IDLE;
				break;
			case SubStatus.Phase1:
			case SubStatus.Phase2:
				return;
			}
			break;
		case MainStatus.Hurt:
			_currentAnimationId = AnimationID.ANI_HURT;
			break;
		case MainStatus.Jump:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_JUMP_START;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_JUMP;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_FALL_LOOP;
				break;
			case SubStatus.Phase3:
				_currentAnimationId = AnimationID.ANI_LAND;
				break;
			}
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
				_currentAnimationId = AnimationID.ANI_SKILL2_START;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_SKILL2_LOOP;
				break;
			case SubStatus.Phase2:
				return;
			case SubStatus.Phase3:
				_currentAnimationId = AnimationID.ANI_SKILL2_LOOP;
				break;
			case SubStatus.Phase4:
				_currentAnimationId = AnimationID.ANI_SKILL2_END;
				break;
			}
			break;
		case MainStatus.Skill3:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_SKILL3_START;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_SKILL3_LOOP;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_SKILL3_END;
				break;
			case SubStatus.Phase3:
				_currentAnimationId = AnimationID.ANI_SKILL3_FALL;
				break;
			case SubStatus.Phase4:
				_currentAnimationId = AnimationID.ANI_LAND;
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
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_SKILL5_START;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_SKILL5_LOOP;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_SKILL5_LOOP;
				break;
			case SubStatus.Phase3:
				_currentAnimationId = AnimationID.ANI_SKILL5_END;
				break;
			}
			break;
		}
		_animator.Play(_animatorHash[(int)_currentAnimationId], 0, 0f);
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
		base.transform.position = pos;
	}

	public override void BossIntro(Action cb)
	{
		if (_mainStatus == MainStatus.Debut)
		{
			_introReady = true;
			IntroCallBack = cb;
		}
	}

	protected override void DeadBehavior(ref HurtPassParam tHurtPassParam)
	{
		if ((bool)_collideBullet)
		{
			_collideBullet.BackToPool();
		}
		if ((bool)_collideBulletSkill2)
		{
			_collideBulletSkill2.BackToPool();
		}
		if ((bool)_collideBulletSkill3)
		{
			_collideBulletSkill3.BackToPool();
		}
		if ((bool)_skill2ParticleSystem)
		{
			_skill2ParticleSystem.Stop();
		}
		if ((bool)_skill3ParticleSystem)
		{
			_skill3ParticleSystem.Stop();
		}
		if ((bool)_skill5ParticleSystem)
		{
			_skill5ParticleSystem.Stop();
		}
		if (_mainStatus != MainStatus.Dead)
		{
			_animator.speed = 1f;
			StageUpdate.SlowStage();
			AI_STATE aiState = AiState;
			if (aiState != AI_STATE.mob_005)
			{
				PlaySE(ExplodeSE[0], ExplodeSE[1]);
			}
			SetStatus(MainStatus.Dead);
		}
	}

	protected virtual void UpdateDirection(int forceDirection = 0)
	{
		if (forceDirection != 0)
		{
			base.direction = forceDirection;
		}
		else if (Target != null && Target.transform.position.x > _transform.position.x)
		{
			base.direction = 1;
		}
		else
		{
			base.direction = -1;
		}
		ModelTransform.localScale = new Vector3(ModelTransform.localScale.x, ModelTransform.localScale.y, Mathf.Abs(ModelTransform.localScale.z) * (float)base.direction);
	}

	protected void UpdateRandomState()
	{
		MainStatus mainStatus = MainStatus.Idle;
		if (isCombo)
		{
			if (nowComboSklIdx + 1 >= nowUseCombo.Length || nowUseCombo == null)
			{
				isCombo = false;
				nowComboSklIdx = 0;
				UpdateRandomState();
				return;
			}
			nowComboSklIdx++;
			mainStatus = nowUseCombo[nowComboSklIdx];
		}
		else
		{
			AI_STATE aiState = AiState;
			if (aiState != AI_STATE.mob_005)
			{
				if ((float)(int)Hp / (float)(int)MaxHp >= 0.5f)
				{
					switch (_previousStatus)
					{
					case MainStatus.Idle:
						mainStatus = randomStatus_after_idle[OrangeBattleUtility.Random(0, randomStatus_after_idle.Length)];
						break;
					case MainStatus.Jump:
						mainStatus = randomStatus_after_jump[OrangeBattleUtility.Random(0, randomStatus_after_jump.Length)];
						break;
					case MainStatus.Skill2:
						mainStatus = randomStatus_after_skl02[OrangeBattleUtility.Random(0, randomStatus_after_skl02.Length)];
						break;
					case MainStatus.Skill4:
						mainStatus = randomStatus_after_skl04[OrangeBattleUtility.Random(0, randomStatus_after_skl04.Length)];
						break;
					}
				}
				else
				{
					mainStatus = MainStatus.ComboSwitch;
				}
			}
			else if (GameLogicUpdateManager.GameFrame > nSkill5CDFrame)
			{
				mainStatus = MainStatus.Skill5;
			}
			else if ((float)(int)Hp / (float)(int)MaxHp >= 0.5f)
			{
				switch (_previousStatus)
				{
				case MainStatus.Idle:
					mainStatus = randomStatus_after_idle[OrangeBattleUtility.Random(0, randomStatus_after_idle.Length)];
					break;
				case MainStatus.Jump:
					mainStatus = randomStatus_after_jump[OrangeBattleUtility.Random(0, randomStatus_after_jump.Length)];
					break;
				case MainStatus.Skill2:
					mainStatus = randomStatus_after_skl02[OrangeBattleUtility.Random(0, randomStatus_after_skl02.Length)];
					break;
				case MainStatus.Skill4:
					mainStatus = randomStatus_after_skl04[OrangeBattleUtility.Random(0, randomStatus_after_skl04.Length)];
					break;
				}
			}
			else
			{
				mainStatus = MainStatus.ComboSwitch;
			}
		}
		if (StageUpdate.gbIsNetGame)
		{
			if (StageUpdate.bIsHost)
			{
				StageUpdate.RegisterSendAndRun(sNetSerialID, (int)mainStatus);
				SetStatus(MainStatus.IdleWaitNet);
			}
		}
		else
		{
			SetStatus(mainStatus);
		}
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

	private void CheckOutRangeJump()
	{
		int num = int.MaxValue;
		num = ((base.direction != 1) ? IntMath.Abs(Controller.LogicPosition.x - iStageLeftPosX_Jump) : IntMath.Abs(Controller.LogicPosition.x - iStageRightPosX_Jump));
		if (num <= 1000)
		{
			_velocity.x = 0;
		}
	}

	private void CheckOutRange()
	{
		int num = int.MaxValue;
		num = ((base.direction != 1) ? IntMath.Abs(Controller.LogicPosition.x - iStageLeftPosX) : IntMath.Abs(Controller.LogicPosition.x - iStageRightPosX));
		if (num <= 1000)
		{
			_velocity.x = 0;
		}
	}

	private void PlaySE_bs015_magma09_stop(object obj)
	{
		if (EnemyData.s_MODEL == "enemy_bs118")
		{
			base.SoundSource.PlaySE("BossSE02", "bs015_magma09_stop");
		}
	}
}
