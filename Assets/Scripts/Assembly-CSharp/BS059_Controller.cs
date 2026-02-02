#define RELEASE
using System;
using System.Collections.Generic;
using CallbackDefs;
using CodeStage.AntiCheat.ObscuredTypes;
using Newtonsoft.Json;
using StageLib;
using UnityEngine;

public class BS059_Controller : EnemyControllerBase, IManagedUpdateBehavior
{
	private class ChargShot
	{
		public ParticleSystem m_chargeshot;

		private bool play { get; set; }

		public bool isPlaying
		{
			get
			{
				return m_chargeshot.isPlaying;
			}
		}

		public ChargShot()
		{
			play = false;
		}

		~ChargShot()
		{
			if (play)
			{
				MonoBehaviourSingleton<AudioManager>.Instance.Play("BossSE02", "bs014_copyx08_stop");
			}
		}

		public void Play()
		{
			if (!play)
			{
				play = true;
				MonoBehaviourSingleton<AudioManager>.Instance.Play("BossSE02", "bs014_copyx08_lp");
			}
			m_chargeshot.Play();
		}

		public void Stop()
		{
			if (play)
			{
				MonoBehaviourSingleton<AudioManager>.Instance.Play("BossSE02", "bs014_copyx08_stop");
				play = false;
			}
			m_chargeshot.Stop();
		}
	}

	private enum MainStatus
	{
		Idle = 0,
		Fall = 1,
		Debut = 2,
		Hurt = 3,
		SpecialMotion = 4,
		Approaching = 5,
		Skill0 = 6,
		Skill1 = 7,
		Skill2 = 8,
		Skill3 = 9,
		Skill4 = 10,
		Skill5 = 11,
		Skill6 = 12,
		Skill7 = 13,
		Die = 14
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
		ANI_JUMP = 1,
		ANI_FALL = 2,
		ANI_DEBUT_LOOP = 3,
		ANI_DEBUT_END = 4,
		ANI_SKILL0_JUMPSTART = 5,
		ANI_SKILL0_JUMPLOOP = 6,
		ANI_SKILL0_START = 7,
		ANI_SKILL0_LOOP = 8,
		ANI_SKILL0_END = 9,
		ANI_SKILL1_JUMP_START = 10,
		ANI_SKILL1_JUMP_LOOP = 11,
		ANI_SKILL1_CRUSH_START = 12,
		ANI_SKILL1_CRUSH_LOOP = 13,
		ANI_SKILL1_FALL_START = 14,
		ANI_SKILL1_FALL_LOOP = 15,
		ANI_SKILL1_FALL_END = 16,
		ANI_SKILL2_START = 17,
		ANI_SKILL2_LOOP = 18,
		ANI_SKILL2_END = 19,
		ANI_SKILL3_START = 20,
		ANI_SKILL3_LOOP = 21,
		ANI_SKILL3_END = 22,
		ANI_SKILL3_FIRE_START = 23,
		ANI_SKILL3_FIRE_LOOP = 24,
		ANI_SKILL3_FIRE_END = 25,
		ANI_SKILL3_ELECTRIC_START = 26,
		ANI_SKILL3_ELECTRIC_LOOP = 27,
		ANI_SKILL3_ELECTRIC_END = 28,
		ANI_SKILL4_JUMP_START = 29,
		ANI_SKILL4_JUMP_LOOP = 30,
		ANI_SKILL4_SHOOT_START = 31,
		ANI_SKILL4_SHOOT_LOOP = 32,
		ANI_SKILL4_FALL_START = 33,
		ANI_SKILL4_FALL_LOOP = 34,
		ANI_SKILL4_FALL_END = 35,
		ANI_SKILL5_JUMP_START = 36,
		ANI_SKILL5_JUMP_LOOP = 37,
		ANI_SKILL5_SHOOT_START = 38,
		ANI_SKILL5_SHOOT_LOOP = 39,
		ANI_SKILL5_FALL_START = 40,
		ANI_SKILL5_FALL_LOOP = 41,
		ANI_SKILL5_FALL_END = 42,
		ANI_SKILL7_START = 43,
		ANI_SKILL7_LOOP = 44,
		ANI_SKILL7_END = 45,
		ANI_HURT = 46,
		ANI_DEAD = 47,
		MAX_ANIMATION_ID = 48
	}

	private enum ElementStatus
	{
		Normal = 0,
		Ice = 1,
		Electric = 2,
		Fire = 3
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

	private int[] SkillWeightArray = new int[4] { 2, 1, 2, 2 };

	private int jumpmode;

	private readonly int _HashAngle = Animator.StringToHash("angle");

	private CollideBullet SlidingKickCollider;

	private CollideBullet StarCrushCollider;

	private int ExpectedHP;

	private ElementStatus _elementStatus;

	private List<int> elementarray = new List<int> { 0, 1, 2, 3 };

	private int HpStatus = 4;

	private int HpStep = 5;

	public int Cureable = 1;

	private Transform ShootTransform;

	private Transform ElectricChargeShootTransform;

	private Transform[] TransformMeshs = new Transform[5];

	private bool _IsTransformed;

	private bool _IsTransformedFx;

	public bool _isCharge;

	private bool StartTracking;

	private int ShootTimes;

	private float NextShotTime;

	private float FireBallInterval = 0.25f;

	private BS059_IceSplit IceBullet;

	private OrangeTimer ChargeTimer;

	private Vector3 BulletVector;

	private int nDeadCount;

	private bool bIsClearStage;

	private ParticleSystem use_skill0;

	private ParticleSystem use_skill1;

	private ChipSystem mchip_maoh;

	private ChargShot use_chargeshot;

	private int WalkSpeed = Mathf.RoundToInt(OrangeBattleUtility.PlayerWalkSpeed * OrangeBattleUtility.PPU * OrangeBattleUtility.FPS * 1000f * 1.2f);

	private int JumpSpeed = Mathf.RoundToInt(OrangeBattleUtility.PlayerJumpSpeed * OrangeBattleUtility.PPU * OrangeBattleUtility.FPS * 1000f);

	private int DashSpeed = Mathf.RoundToInt(OrangeBattleUtility.PlayerDashSpeed * OrangeBattleUtility.PPU * OrangeBattleUtility.FPS * 1000f * 1.2f);

	protected virtual void HashAnimation()
	{
		_animationHash[0] = Animator.StringToHash("BS059@idle_loop");
		_animationHash[3] = Animator.StringToHash("BS059@standby_loop");
		_animationHash[4] = Animator.StringToHash("BS059@debut");
		_animationHash[5] = Animator.StringToHash("BS059@skill_01_step1_start");
		_animationHash[6] = Animator.StringToHash("BS059@skill_01_step1_loop");
		_animationHash[7] = Animator.StringToHash("BS059@skill_01_step2_start");
		_animationHash[8] = Animator.StringToHash("BS059@skill_01_step2_loop");
		_animationHash[9] = Animator.StringToHash("BS059@skill_01_step2_end");
		_animationHash[10] = Animator.StringToHash("BS059@skill_02_step1_start");
		_animationHash[11] = Animator.StringToHash("BS059@skill_02_step1_loop");
		_animationHash[12] = Animator.StringToHash("BS059@skill_02_step2_start");
		_animationHash[13] = Animator.StringToHash("BS059@skill_02_step2_loop");
		_animationHash[14] = Animator.StringToHash("BS059@skill_02_step3_start");
		_animationHash[15] = Animator.StringToHash("BS059@skill_02_step3_loop");
		_animationHash[16] = Animator.StringToHash("BS059@skill_02_step3_end");
		_animationHash[43] = Animator.StringToHash("BS059@skill_03_start");
		_animationHash[44] = Animator.StringToHash("BS059@skill_03_loop");
		_animationHash[45] = Animator.StringToHash("BS059@skill_03_end");
		_animationHash[17] = Animator.StringToHash("BS059@skill_04_start");
		_animationHash[18] = Animator.StringToHash("BS059@skill_04_loop");
		_animationHash[19] = Animator.StringToHash("BS059@skill_04_end");
		_animationHash[20] = Animator.StringToHash("BS059@skill_04_start");
		_animationHash[21] = Animator.StringToHash("BS059@skill_04_loop");
		_animationHash[22] = Animator.StringToHash("BS059@skill_04_end");
		_animationHash[23] = Animator.StringToHash("BS059@skill_06_start");
		_animationHash[24] = Animator.StringToHash("BS059@skill_06_loop");
		_animationHash[25] = Animator.StringToHash("BS059@skill_06_end");
		_animationHash[26] = Animator.StringToHash("BS059@skill_07_start");
		_animationHash[27] = Animator.StringToHash("BS059@skill_07_loop");
		_animationHash[28] = Animator.StringToHash("BS059@skill_07_end");
		_animationHash[29] = Animator.StringToHash("BS059@skill_05_step1_start");
		_animationHash[30] = Animator.StringToHash("BS059@skill_05_step1_loop");
		_animationHash[31] = Animator.StringToHash("BS059@skill_05_step2_start");
		_animationHash[32] = Animator.StringToHash("BS059@skill_05_step2_loop");
		_animationHash[33] = Animator.StringToHash("BS059@skill_05_step3_start");
		_animationHash[34] = Animator.StringToHash("BS059@skill_05_step3_loop");
		_animationHash[35] = Animator.StringToHash("BS059@skill_05_step3_end");
		_animationHash[36] = Animator.StringToHash("BS059@skill_05_step1_start");
		_animationHash[37] = Animator.StringToHash("BS059@skill_05_step1_loop");
		_animationHash[38] = Animator.StringToHash("BS059@skill_05_step2_start");
		_animationHash[39] = Animator.StringToHash("BS059@skill_05_step2_loop");
		_animationHash[40] = Animator.StringToHash("BS059@skill_05_step3_start");
		_animationHash[41] = Animator.StringToHash("BS059@skill_05_step3_loop");
		_animationHash[42] = Animator.StringToHash("BS059@skill_05_step3_end");
		_animationHash[46] = Animator.StringToHash("BS059@hurt_loop");
		_animationHash[47] = Animator.StringToHash("BS059@death");
		_animationHash[1] = Animator.StringToHash("BS059@jump_loop");
		_animationHash[2] = Animator.StringToHash("BS059@fall_loop");
	}

	protected override void Awake()
	{
		base.Awake();
		Transform[] target = _transform.GetComponentsInChildren<Transform>(true);
		ModelTransform = OrangeBattleUtility.FindChildRecursive(ref target, "model", true);
		_collideBullet = OrangeBattleUtility.FindChildRecursive(ref target, "Bip", true).gameObject.AddOrGetComponent<CollideBullet>();
		ShootTransform = OrangeBattleUtility.FindChildRecursive(ref target, "ShootPoint", true);
		ElectricChargeShootTransform = OrangeBattleUtility.FindChildRecursive(ref target, "R BusterPoint", true);
		StarCrushCollider = OrangeBattleUtility.FindChildRecursive(ref target, "Bip Spine", true).gameObject.AddOrGetComponent<CollideBullet>();
		SlidingKickCollider = OrangeBattleUtility.FindChildRecursive(ref target, "Bip Footsteps").gameObject.AddOrGetComponent<CollideBullet>();
		TransformMeshs[0] = OrangeBattleUtility.FindChildRecursive(ref target, "BS059_CopyX_BodyMesh", true);
		TransformMeshs[1] = OrangeBattleUtility.FindChildRecursive(ref target, "BS059_CopyX_BusterMesh", true);
		TransformMeshs[2] = OrangeBattleUtility.FindChildRecursive(ref target, "BS059_CopyX_HandMesh_L", true);
		TransformMeshs[3] = OrangeBattleUtility.FindChildRecursive(ref target, "BS059_CopyX_HandMesh_R", true);
		TransformMeshs[4] = OrangeBattleUtility.FindChildRecursive(ref target, "BS059_CopyX_FullMesh", true);
		for (int i = 0; i < 4; i++)
		{
			TransformMeshs[i].gameObject.SetActive(false);
		}
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxtrn_start_001", 2);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_copy_x_Skill7_000", 2);
		Transform transform = OrangeBattleUtility.FindChildRecursive(ref target, "fxuse_copy_x_skill0", true);
		if (transform != null)
		{
			use_skill0 = transform.GetComponent<ParticleSystem>();
		}
		transform = OrangeBattleUtility.FindChildRecursive(ref target, "fxuse_copy_x_Skill1", true);
		if (transform != null)
		{
			use_skill1 = transform.GetComponent<ParticleSystem>();
		}
		transform = OrangeBattleUtility.FindChildRecursive(ref target, "chip_maoh", true);
		if (transform != null)
		{
			mchip_maoh = transform.GetComponent<ChipSystem>();
		}
		transform = OrangeBattleUtility.FindChildRecursive(ref target, "fxduring_copy_x_chargeshot_001", true);
		if (transform != null)
		{
			use_chargeshot = new ChargShot();
			use_chargeshot.m_chargeshot = transform.GetComponent<ParticleSystem>();
		}
		_animator = ModelTransform.GetComponent<Animator>();
		_animationHash = new int[48];
		HashAnimation();
		base.AimPoint = new Vector3(0f, 1.2f, 0f);
		OrangeBattleUtility.AddEnemyAutoAimSystem(_transform, out _enemyAutoAimSystem);
		_enemyAutoAimSystem.UpdateAimRange(20f);
		AiTimer.TimerStart();
		ChargeTimer = OrangeTimerManager.GetTimer();
		Controller.Collisions.below = true;
	}

	protected override void Start()
	{
		base.Start();
		_characterMaterial.UpdateTex(4);
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
				return;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_DEBUT_END;
				break;
			}
			break;
		case MainStatus.Idle:
			_currentAnimationId = AnimationID.ANI_IDLE;
			break;
		case MainStatus.Die:
			switch (_subStatus)
			{
			default:
				return;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_HURT;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_DEAD;
				break;
			}
			break;
		case MainStatus.Hurt:
			_currentAnimationId = AnimationID.ANI_HURT;
			break;
		case MainStatus.Approaching:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_JUMP;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_FALL;
				break;
			}
			break;
		case MainStatus.Skill0:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_SKILL0_JUMPSTART;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_SKILL0_JUMPLOOP;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_SKILL0_START;
				break;
			case SubStatus.Phase3:
				_currentAnimationId = AnimationID.ANI_SKILL0_LOOP;
				break;
			case SubStatus.Phase4:
				_currentAnimationId = AnimationID.ANI_SKILL0_END;
				break;
			}
			break;
		case MainStatus.Skill1:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_SKILL1_JUMP_START;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_SKILL1_JUMP_LOOP;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_SKILL1_CRUSH_START;
				break;
			case SubStatus.Phase3:
				_currentAnimationId = AnimationID.ANI_SKILL1_CRUSH_LOOP;
				break;
			case SubStatus.Phase4:
				_currentAnimationId = AnimationID.ANI_SKILL1_FALL_START;
				break;
			case SubStatus.Phase5:
				_currentAnimationId = AnimationID.ANI_SKILL1_FALL_LOOP;
				break;
			case SubStatus.Phase6:
				_currentAnimationId = AnimationID.ANI_SKILL1_FALL_END;
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
				_currentAnimationId = AnimationID.ANI_SKILL2_END;
				break;
			}
			break;
		case MainStatus.Skill3:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				switch (_elementStatus)
				{
				case ElementStatus.Normal:
				case ElementStatus.Ice:
					_currentAnimationId = AnimationID.ANI_SKILL3_START;
					break;
				case ElementStatus.Electric:
					_currentAnimationId = AnimationID.ANI_SKILL3_ELECTRIC_START;
					break;
				case ElementStatus.Fire:
					_currentAnimationId = AnimationID.ANI_SKILL3_FIRE_START;
					break;
				}
				break;
			case SubStatus.Phase1:
				switch (_elementStatus)
				{
				case ElementStatus.Normal:
				case ElementStatus.Ice:
					_currentAnimationId = AnimationID.ANI_SKILL3_LOOP;
					break;
				case ElementStatus.Electric:
					_currentAnimationId = AnimationID.ANI_SKILL3_ELECTRIC_LOOP;
					break;
				case ElementStatus.Fire:
					_currentAnimationId = AnimationID.ANI_SKILL3_FIRE_LOOP;
					break;
				}
				break;
			case SubStatus.Phase2:
				switch (_elementStatus)
				{
				case ElementStatus.Normal:
				case ElementStatus.Ice:
					_currentAnimationId = AnimationID.ANI_SKILL3_END;
					break;
				case ElementStatus.Electric:
					_currentAnimationId = AnimationID.ANI_SKILL3_ELECTRIC_END;
					break;
				case ElementStatus.Fire:
					_currentAnimationId = AnimationID.ANI_SKILL3_FIRE_END;
					break;
				}
				break;
			}
			break;
		case MainStatus.Skill4:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_SKILL4_JUMP_START;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_SKILL4_JUMP_LOOP;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_SKILL4_SHOOT_START;
				break;
			case SubStatus.Phase3:
				_currentAnimationId = AnimationID.ANI_SKILL4_SHOOT_LOOP;
				break;
			case SubStatus.Phase4:
				_currentAnimationId = AnimationID.ANI_SKILL4_FALL_START;
				break;
			case SubStatus.Phase5:
				_currentAnimationId = AnimationID.ANI_SKILL4_FALL_LOOP;
				break;
			case SubStatus.Phase6:
				_currentAnimationId = AnimationID.ANI_SKILL4_FALL_END;
				break;
			}
			break;
		case MainStatus.Skill5:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_SKILL5_JUMP_START;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_SKILL5_JUMP_LOOP;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_SKILL5_SHOOT_START;
				break;
			case SubStatus.Phase3:
				_currentAnimationId = AnimationID.ANI_SKILL5_SHOOT_LOOP;
				break;
			case SubStatus.Phase4:
				_currentAnimationId = AnimationID.ANI_SKILL5_FALL_START;
				break;
			case SubStatus.Phase5:
				_currentAnimationId = AnimationID.ANI_SKILL5_FALL_LOOP;
				break;
			case SubStatus.Phase6:
				_currentAnimationId = AnimationID.ANI_SKILL5_FALL_END;
				break;
			}
			break;
		case MainStatus.Skill7:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_SKILL7_START;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_SKILL7_LOOP;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_SKILL7_END;
				break;
			}
			break;
		}
		_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
	}

	private void UpdateDirection(int forceDirection = 0)
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
			IsInvincible = false;
			break;
		case MainStatus.Die:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				base.AllowAutoAim = false;
				_velocity.x = 0;
				if (Controller.Collisions.below)
				{
					SetStatus(MainStatus.Die, SubStatus.Phase2);
				}
				else
				{
					SetStatus(MainStatus.Die, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
			{
				STAGE_TABLE value;
				ManagedSingleton<OrangeDataManager>.Instance.STAGE_TABLE_DICT.TryGetValue(ManagedSingleton<StageHelper>.Instance.nLastStageID, out value);
				if (value != null && (ManagedSingleton<PlayerNetManager>.Instance.dicStage.ContainsKey(ManagedSingleton<StageHelper>.Instance.nLastStageID) || (value != null && value.n_DIFFICULTY > 1)))
				{
					bIsClearStage = true;
				}
				if (!bIsClearStage)
				{
					_velocity.x = 1000 * -base.direction;
					_velocity.y = 4500;
				}
				else
				{
					IgnoreGravity = true;
					_bDeadPlayCompleted = true;
				}
				break;
			}
			case SubStatus.Phase2:
				_bDeadPlayCompleted = true;
				_velocity = VInt3.zero;
				break;
			case SubStatus.Phase3:
				switch (AiState)
				{
				case AI_STATE.mob_002:
					StartCoroutine(BossDieFlow(GetTargetPoint(), "FX_BOSS_EXPLODE2", false, false));
					break;
				case AI_STATE.mob_003:
					StartCoroutine(BossDieFlow(GetTargetPoint(), "FX_BOSS_EXPLODE2", false, false));
					break;
				case AI_STATE.mob_004:
					Debug.LogError("這個 AI編碼 不該死亡，AI編碼是： mob_00" + (int)(AiState + 1));
					break;
				default:
					StartCoroutine(BossDieFlow(GetTargetPoint()));
					break;
				}
				break;
			}
			break;
		case MainStatus.Hurt:
			Target = _enemyAutoAimSystem.GetClosetPlayer();
			UpdateDirection();
			_velocity.y += 1000;
			_velocity.x = 3000;
			break;
		case MainStatus.Approaching:
			if (_subStatus != 0)
			{
				break;
			}
			PlayBossSE("BossSE02", "bs014_copyx21");
			_velocity.y += (int)((float)JumpSpeed * 1.1f);
			if (Target == null)
			{
				SetStatus(MainStatus.Idle);
				break;
			}
			_velocity.x = Target.Controller.LogicPosition.x - Controller.LogicPosition.x;
			if (_velocity.x < 0)
			{
				_velocity.x += 1000;
			}
			else
			{
				_velocity.x -= 1000;
			}
			break;
		case MainStatus.Skill0:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				PlaySE("BossSE02", "bs014_copyx06");
				_velocity.y += (int)((float)JumpSpeed * 0.8f);
				_velocity.x = -base.direction * WalkSpeed / 8;
				break;
			case SubStatus.Phase2:
				SlidingKickCollider.Active(targetMask);
				_collideBullet.BackToPool();
				UpdateDirection();
				_velocity.x = base.direction * DashSpeed;
				break;
			}
			break;
		case MainStatus.Skill1:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				PlayBossSE("BossSE02", "bs014_copyx21");
				jumpmode = OrangeBattleUtility.Random(1, 3);
				if (jumpmode == 1)
				{
					_velocity.y = (int)((float)JumpSpeed * 0.8f);
					_velocity.x = WalkSpeed * base.direction / 4;
				}
				else if (jumpmode == 2)
				{
					StartTracking = true;
					_velocity.y = (int)((float)JumpSpeed * 2f);
					_velocity.x = WalkSpeed * base.direction / 4;
				}
				break;
			case SubStatus.Phase2:
				_velocity.y = JumpSpeed / 4;
				_velocity.x = 0;
				break;
			case SubStatus.Phase3:
				PlayBossSE("BossSE02", "bs014_copyx04");
				StarCrushCollider.Active(targetMask);
				_collideBullet.BackToPool();
				IgnoreGravity = true;
				_velocity.x = (int)((float)(DashSpeed * base.direction) * 1.2f);
				break;
			case SubStatus.Phase4:
				StartTracking = false;
				StarCrushCollider.BackToPool();
				_collideBullet.Active(targetMask);
				IgnoreGravity = false;
				_velocity.x = WalkSpeed / 8;
				break;
			}
			break;
		case MainStatus.Skill2:
			_velocity.x = 0;
			switch (_subStatus)
			{
			case SubStatus.Phase0:
			{
				ElementStatus elementStatus = _elementStatus;
				if (elementStatus == ElementStatus.Fire)
				{
					_animator.SetFloat(_HashAngle, 0f);
				}
				else
				{
					SetAimDirection();
				}
				break;
			}
			case SubStatus.Phase1:
				switch (_elementStatus)
				{
				case ElementStatus.Normal:
					ShootTimes = 3;
					NextShotTime = 0f;
					break;
				case ElementStatus.Ice:
				case ElementStatus.Electric:
					ShootTimes = 2;
					NextShotTime = 0f;
					break;
				case ElementStatus.Fire:
					ShootTimes = 6;
					NextShotTime = 0f;
					break;
				}
				break;
			}
			break;
		case MainStatus.Skill3:
			_velocity.x = 0;
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				SetAimDirection();
				break;
			case SubStatus.Phase1:
				switch (_elementStatus)
				{
				case ElementStatus.Normal:
				case ElementStatus.Fire:
					PlaySE("BossSE02", "bs014_copyx13");
					ShootTimes = 1;
					break;
				case ElementStatus.Ice:
					ShootTimes = 4;
					NextShotTime = 0f;
					break;
				case ElementStatus.Electric:
					PlayBossSE("BossSE02", "bs014_copyx10");
					ShootTimes = 3;
					NextShotTime = 0f;
					break;
				}
				break;
			}
			break;
		case MainStatus.Skill4:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				PlaySE("BossSE02", "bs014_copyx21");
				_velocity.y = (int)((float)JumpSpeed * 1f);
				_velocity.x = WalkSpeed * base.direction / 4;
				break;
			case SubStatus.Phase2:
			{
				_velocity.y = JumpSpeed / 4;
				_velocity.x = 0;
				ElementStatus elementStatus = _elementStatus;
				if (elementStatus == ElementStatus.Fire)
				{
					_animator.SetFloat(_HashAngle, -45f);
				}
				else
				{
					SetAimDirection();
				}
				break;
			}
			case SubStatus.Phase3:
				switch (_elementStatus)
				{
				case ElementStatus.Normal:
					ShootTimes = 3;
					NextShotTime = 0f;
					break;
				case ElementStatus.Ice:
				case ElementStatus.Electric:
					ShootTimes = 2;
					NextShotTime = 0f;
					break;
				case ElementStatus.Fire:
					ShootTimes = 6;
					NextShotTime = 0f;
					break;
				}
				break;
			}
			break;
		case MainStatus.Skill5:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				PlayBossSE("BossSE02", "bs014_copyx21");
				_velocity.y = (int)((float)JumpSpeed * 1f);
				_velocity.x = WalkSpeed * base.direction / 4;
				break;
			case SubStatus.Phase2:
				_velocity.y = JumpSpeed / 4;
				_velocity.x = 0;
				SetAimDirection();
				break;
			case SubStatus.Phase3:
				switch (_elementStatus)
				{
				case ElementStatus.Normal:
					ShootTimes = 1;
					break;
				case ElementStatus.Ice:
					ShootTimes = 4;
					NextShotTime = 0f;
					break;
				}
				break;
			}
			break;
		case MainStatus.Skill6:
			PlaySE("BossSE02", "bs014_copyx01");
			base.SoundSource.PlaySE("BossSE02", "bs014_copyx03", 0.8f);
			UpdateDirection();
			IsInvincible = true;
			MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxtrn_start_001", base.transform.position, Quaternion.Euler(0f, 0f, 0f), Array.Empty<object>());
			mchip_maoh.ActiveChipSkill(false);
			break;
		case MainStatus.Skill7:
			UpdateDirection();
			IsInvincible = true;
			MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_copy_x_Skill7_000", base.transform.position, Quaternion.Euler(0f, 0f, 0f), Array.Empty<object>());
			break;
		}
		AiTimer.TimerStart();
		UpdateAnimation();
	}

	private void UpdateRandomState()
	{
		MainStatus mainStatus = MainStatus.Idle;
		if (!_isCharge && !ChargeTimer.IsStarted())
		{
			if (OrangeBattleUtility.Random(0, 5) < 1)
			{
				if (use_chargeshot != null)
				{
					use_chargeshot.Play();
				}
				ChargeTimer.TimerStart();
			}
		}
		else if (ChargeTimer.GetMillisecond() > 500)
		{
			_isCharge = true;
			ChargeTimer.TimerStop();
		}
		switch (_mainStatus)
		{
		case MainStatus.Debut:
			SetStatus(MainStatus.Idle);
			break;
		case MainStatus.Idle:
		{
			float num = Mathf.Abs(Target.transform.position.x - _transform.position.x);
			float num2 = Mathf.Abs(Target.transform.position.y - _transform.position.y);
			int[] skillWeightArray = SkillWeightArray;
			mainStatus = (MainStatus)WeightRandom(skillWeightArray, 4);
			if ((double)num2 > 3.0 && (mainStatus == MainStatus.Approaching || mainStatus == MainStatus.Skill0))
			{
				skillWeightArray = new int[4] { 2, 0, 0, 2 };
				mainStatus = (MainStatus)WeightRandom(skillWeightArray, 4);
			}
			if (mainStatus == MainStatus.SpecialMotion)
			{
				if (ChargeTimer.IsStarted())
				{
					skillWeightArray = new int[4] { 0, 1, 2, 2 };
					mainStatus = (MainStatus)WeightRandom(skillWeightArray, 4);
				}
				else
				{
					mainStatus = (MainStatus)OrangeBattleUtility.Random(8, 12);
					SkillWeightArray[0]--;
				}
			}
			else
			{
				SkillWeightArray[0] = 2;
			}
			if ((double)num < 6.0 && mainStatus == MainStatus.Approaching && (int)Hp > (int)MaxHp / 2)
			{
				mainStatus = MainStatus.Skill0;
			}
			else if ((double)num > 5.0 && mainStatus == MainStatus.Skill0)
			{
				mainStatus = MainStatus.Approaching;
			}
			else if ((mainStatus == MainStatus.Skill2 || mainStatus == MainStatus.Skill4) && _isCharge)
			{
				mainStatus++;
			}
			else if ((mainStatus == MainStatus.Skill3 || mainStatus == MainStatus.Skill5) && !_isCharge)
			{
				mainStatus--;
			}
			if ((_elementStatus == ElementStatus.Fire || _elementStatus == ElementStatus.Electric) && mainStatus == MainStatus.Skill5)
			{
				mainStatus -= 2;
			}
			break;
		}
		}
		if (mainStatus != 0 && CheckHost())
		{
			UploadEnemyStatus((int)mainStatus);
		}
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
			UpdateDirection();
		}
		SetStatus((MainStatus)nSet);
	}

	private Color getHdrColor(int index)
	{
		float num = 1f;
		Color color = new Color(0f, 0.4220434f, 9.494677f);
		switch (index)
		{
		case 0:
			num = Mathf.Pow(1f, 3.664044f);
			color = new Color(0f * num, 0.4220434f * num, 9.494677f * num, 0.5f);
			break;
		case 1:
			num = Mathf.Pow(1f, 3.090198f);
			color = new Color(6.378749f * num, 0.6318572f * num, 0.8777828f * num, 0.5f);
			break;
		case 2:
			num = Mathf.Pow(1f, 2.423059f);
			color = new Color(0f * num, 4.017043f * num, 0.3858284f * num, 0.5f);
			break;
		case 3:
			num = Mathf.Pow(1f, 2.778829f);
			color = new Color(0f * num, 4.117768f * num, 5.140484f * num, 0.5f);
			break;
		default:
			num = Mathf.Pow(1f, 3.664044f);
			color = new Color(0f * num, 0.4220434f * num, 9.494677f * num, 0.5f);
			break;
		}
		return color;
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
				if (_introReady)
				{
					SetStatus(MainStatus.Debut, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				if (_currentFrame > 1f && IntroCallBack != null)
				{
					IntroCallBack();
					if (!bWaitNetStatus)
					{
						UpdateRandomState();
					}
				}
				if ((double)_currentFrame > 0.4 && !_IsTransformedFx)
				{
					_IsTransformedFx = true;
					PlaySE("BossSE02", "bs014_copyx01");
					base.SoundSource.PlaySE("BossSE02", "bs014_copyx03", 0.8f);
					MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxtrn_start_001", base.transform.position, Quaternion.Euler(0f, 0f, 0f), Array.Empty<object>());
				}
				if (_currentFrame > 0.8f && !_IsTransformed)
				{
					_IsTransformed = true;
					for (int i = 0; i < 4; i++)
					{
						TransformMeshs[i].gameObject.SetActive(true);
					}
					_characterMaterial.UpdateTex(0);
					mchip_maoh.Init();
					mchip_maoh.isSetColor = true;
					mchip_maoh.MeshActiveColor = getHdrColor(0);
					mchip_maoh.ActiveChipSkill(true);
				}
				break;
			}
			break;
		case MainStatus.Idle:
			if (bWaitNetStatus)
			{
				break;
			}
			if (HpStatus > (int)Hp * HpStep / (int)MaxHp)
			{
				if (elementarray.ToArray().Length == 1)
				{
					elementarray = new List<int> { 0, 1, 2, 3 };
				}
				elementarray.Remove((int)_elementStatus);
				HpStatus = (int)Hp * HpStep / (int)MaxHp;
				if ((int)Hp < (int)MaxHp / 2 && Cureable > 0 && AiState != AI_STATE.mob_004)
				{
					ExpectedHP = (int)Hp + (int)MaxHp / 2;
					Cureable--;
					SetStatus(MainStatus.Skill7);
					PlaySE("BossSE02", "bs014_copyx16");
				}
				else
				{
					_elementStatus = (ElementStatus)elementarray[OrangeBattleUtility.Random(0, elementarray.ToArray().Length)];
					SetStatus(MainStatus.Skill6);
				}
			}
			else
			{
				if (!((float)AiTimer.GetMillisecond() > 100f))
				{
					break;
				}
				Target = _enemyAutoAimSystem.GetClosetPlayer();
				if ((bool)Target)
				{
					if ((int)Hp < (int)MaxHp / 2 && Cureable == 0)
					{
						SkillWeightArray = new int[4] { 2, 0, 2, 2 };
					}
					UpdateDirection();
					UpdateRandomState();
					AiTimer.TimerStop();
				}
				else if (CheckHost())
				{
					UploadEnemyStatus(6);
				}
			}
			break;
		case MainStatus.Hurt:
			if (_currentFrame > 0.5f)
			{
				SetStatus(MainStatus.Idle);
			}
			break;
		case MainStatus.Approaching:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Approaching, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if (Controller.Collisions.below)
				{
					PlayBossSE("BossSE02", "bs014_copyx22");
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
				if (Controller.Collisions.below)
				{
					SetStatus(MainStatus.Skill0, SubStatus.Phase2);
					if (use_skill0 != null && !use_skill0.isPlaying)
					{
						use_skill0.Play();
					}
				}
				break;
			case SubStatus.Phase2:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill0, SubStatus.Phase3);
				}
				break;
			case SubStatus.Phase3:
				if (AiTimer.GetMillisecond() > 600 || Controller.Collisions.left || Controller.Collisions.right)
				{
					AiTimer.TimerStop();
					SlidingKickCollider.BackToPool();
					SetStatus(MainStatus.Skill0, SubStatus.Phase4);
					_velocity.x = 0;
					if (use_skill0 != null)
					{
						use_skill0.Stop();
					}
				}
				break;
			case SubStatus.Phase4:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Idle);
					_collideBullet.Active(targetMask);
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
				if (_velocity.y < 0)
				{
					SetStatus(MainStatus.Skill1, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				if (_velocity.y < 0)
				{
					SetStatus(MainStatus.Skill1, SubStatus.Phase3);
					if (use_skill1 != null && !use_skill1.isPlaying)
					{
						use_skill1.Play();
					}
				}
				break;
			case SubStatus.Phase3:
				_velocity.y /= 2;
				if (jumpmode == 1)
				{
					if (_currentFrame > 6f || Controller.Collisions.left || Controller.Collisions.right)
					{
						SetStatus(MainStatus.Skill1, SubStatus.Phase4);
						if (use_skill1 != null)
						{
							use_skill1.Stop();
						}
					}
				}
				else
				{
					if (jumpmode != 2)
					{
						break;
					}
					if (Target == null)
					{
						SetStatus(MainStatus.Idle);
					}
					else if ((_transform.position.x < Target.transform.position.x - 1f && base.direction == -1) || (_transform.position.x > Target.transform.position.x + 1f && base.direction == 1) || Controller.Collisions.left || Controller.Collisions.right)
					{
						if (_isCharge || ChargeTimer.IsStarted())
						{
							SetStatus(MainStatus.Skill1, SubStatus.Phase4);
						}
						else
						{
							_velocity.x = 0;
							base.direction *= -1;
							UpdateDirection();
							IgnoreGravity = false;
							SetStatus(MainStatus.Skill4, SubStatus.Phase1);
						}
						if (use_skill1 != null)
						{
							use_skill1.Stop();
						}
					}
				}
				break;
			case SubStatus.Phase4:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill1, SubStatus.Phase5);
				}
				break;
			case SubStatus.Phase5:
				if (Controller.Collisions.below)
				{
					_velocity.x = 0;
					PlayBossSE("BossSE02", "bs014_copyx22");
					SetStatus(MainStatus.Skill1, SubStatus.Phase6);
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
				switch (_elementStatus)
				{
				case ElementStatus.Normal:
					if (_currentFrame > 1f || ShootTimes == 0)
					{
						SetStatus(MainStatus.Skill2, SubStatus.Phase2);
					}
					else if (_currentFrame > NextShotTime && ShootTimes > 0)
					{
						ShootTimes--;
						ShootBullet(EnemyWeapons[4].BulletData, TargetPos);
						NextShotTime = _currentFrame + 0.12f;
					}
					break;
				case ElementStatus.Electric:
					if (_currentFrame > 2f || ShootTimes == 0)
					{
						SetStatus(MainStatus.Skill2, SubStatus.Phase2);
					}
					else if (_currentFrame > NextShotTime && ShootTimes > 0)
					{
						if (Target == null)
						{
							SetStatus(MainStatus.Idle);
							break;
						}
						TargetPos = Target.Controller.LogicPosition;
						SetAimDirection();
						ShootTimes--;
						ShootBullet(EnemyWeapons[8].BulletData, TargetPos);
						NextShotTime = _currentFrame + 1f;
					}
					break;
				case ElementStatus.Ice:
					if (_currentFrame > 2f || ShootTimes == 0)
					{
						SetStatus(MainStatus.Skill2, SubStatus.Phase2);
					}
					else if (_currentFrame > NextShotTime && ShootTimes > 0)
					{
						if (Target == null)
						{
							SetStatus(MainStatus.Idle);
							break;
						}
						TargetPos = Target.Controller.LogicPosition;
						SetAimDirection();
						ShootTimes--;
						PlayBossSE("BossSE02", "bs014_copyx19");
						IceBullet = ShootBullet(EnemyWeapons[10].BulletData, TargetPos) as BS059_IceSplit;
						IceBullet.splitlevel = 1;
						NextShotTime = _currentFrame + 0.8f;
					}
					break;
				case ElementStatus.Fire:
					if (_currentFrame > 1f || ShootTimes == 0)
					{
						SetStatus(MainStatus.Skill2, SubStatus.Phase2);
						if (use_chargeshot != null)
						{
							_isCharge = true;
							use_chargeshot.Play();
						}
					}
					else if (_currentFrame > NextShotTime && ShootTimes > 0)
					{
						NextShotTime = _currentFrame + 0.1f;
						ShootTimes--;
						BulletBase.TryShotBullet(EnemyWeapons[6].BulletData, ShootTransform.position, _transform.position + new Vector3(4f * (float)base.direction, 0.5f, 0f), null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
					}
					break;
				}
				break;
			case SubStatus.Phase2:
				if (_currentFrame > 1f)
				{
					ElementStatus elementStatus = _elementStatus;
					if (elementStatus == ElementStatus.Fire)
					{
						SetStatus(MainStatus.Skill3);
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
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill3, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				switch (_elementStatus)
				{
				case ElementStatus.Normal:
					if (_currentFrame > 1f || ShootTimes == 0)
					{
						SetStatus(MainStatus.Skill3, SubStatus.Phase2);
					}
					else if (ShootTimes > 0)
					{
						ShootTimes--;
						ShootBullet(EnemyWeapons[5].BulletData, TargetPos);
						if (use_chargeshot != null)
						{
							use_chargeshot.Stop();
						}
					}
					break;
				case ElementStatus.Electric:
					if (_currentFrame > 3f || ShootTimes == 0)
					{
						SetStatus(MainStatus.Skill3, SubStatus.Phase2);
					}
					else if (_currentFrame > NextShotTime && ShootTimes > 0)
					{
						ShootTimes--;
						BulletBase.TryShotBullet(EnemyWeapons[9].BulletData, ElectricChargeShootTransform.position, Vector3.left, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
						BulletBase.TryShotBullet(EnemyWeapons[9].BulletData, ElectricChargeShootTransform.position, Vector3.right, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
						NextShotTime = _currentFrame + 0.8f;
						if (use_chargeshot != null)
						{
							use_chargeshot.Stop();
						}
					}
					break;
				case ElementStatus.Ice:
					if (_currentFrame > 2f || ShootTimes == 0)
					{
						SetStatus(MainStatus.Skill3, SubStatus.Phase2);
					}
					else if (_currentFrame > NextShotTime && ShootTimes > 0)
					{
						ShootTimes--;
						ShootBullet(EnemyWeapons[11].BulletData, TargetPos);
						NextShotTime = _currentFrame + 0.3f;
						if (use_chargeshot != null)
						{
							use_chargeshot.Stop();
						}
					}
					break;
				case ElementStatus.Fire:
					if (_currentFrame > 1f || ShootTimes == 0)
					{
						SetStatus(MainStatus.Skill3, SubStatus.Phase2);
					}
					else if (_currentFrame > 0.3f && ShootTimes > 0)
					{
						ShootTimes--;
						for (int j = 0; j < 8; j++)
						{
							BulletBase.TryShotBullet(EnemyWeapons[7].BulletData, ShootTransform.position, _transform.position + new Vector3(FireBallInterval * (float)j - FireBallInterval * 3.5f, 1f, 0f), null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
						}
						if (use_chargeshot != null)
						{
							use_chargeshot.Stop();
						}
					}
					break;
				}
				break;
			case SubStatus.Phase2:
				if (_currentFrame > 1f)
				{
					_isCharge = false;
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
				if (_velocity.y < 0)
				{
					SetStatus(MainStatus.Skill4, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				if (_velocity.y < 0)
				{
					SetStatus(MainStatus.Skill4, SubStatus.Phase3);
				}
				break;
			case SubStatus.Phase3:
				switch (_elementStatus)
				{
				case ElementStatus.Normal:
					if (_currentFrame > 1f || ShootTimes == 0)
					{
						SetStatus(MainStatus.Skill4, SubStatus.Phase4);
					}
					if (_currentFrame > NextShotTime && ShootTimes > 0)
					{
						ShootTimes--;
						ShootBullet(EnemyWeapons[4].BulletData, TargetPos);
						Recoil();
						NextShotTime = _currentFrame + 0.12f;
					}
					break;
				case ElementStatus.Electric:
					if (_currentFrame > 1f || ShootTimes == 0)
					{
						SetStatus(MainStatus.Skill4, SubStatus.Phase4);
					}
					else if (_currentFrame > NextShotTime && ShootTimes > 0)
					{
						if (Target == null)
						{
							SetStatus(MainStatus.Idle);
							break;
						}
						TargetPos = Target.Controller.LogicPosition;
						SetAimDirection();
						ShootTimes--;
						ShootBullet(EnemyWeapons[8].BulletData, TargetPos);
						Recoil();
						NextShotTime = _currentFrame + 0.4f;
					}
					break;
				case ElementStatus.Ice:
					if (_currentFrame > 1f || ShootTimes == 0)
					{
						SetStatus(MainStatus.Skill4, SubStatus.Phase4);
					}
					else if (_currentFrame > NextShotTime && ShootTimes > 0)
					{
						if (Target == null)
						{
							SetStatus(MainStatus.Idle);
							break;
						}
						TargetPos = Target.Controller.LogicPosition;
						SetAimDirection();
						ShootTimes--;
						PlayBossSE("BossSE02", "bs014_copyx19");
						IceBullet = ShootBullet(EnemyWeapons[10].BulletData, TargetPos) as BS059_IceSplit;
						IceBullet.splitlevel = 1;
						Recoil();
						NextShotTime = _currentFrame + 0.4f;
					}
					break;
				case ElementStatus.Fire:
					if (_currentFrame > 1f || ShootTimes == 0)
					{
						IgnoreGravity = false;
						Recoil();
						SetStatus(MainStatus.Skill4, SubStatus.Phase4);
						if (use_chargeshot != null)
						{
							_isCharge = true;
							use_chargeshot.Play();
						}
					}
					else if (_currentFrame > NextShotTime && ShootTimes > 0)
					{
						IgnoreGravity = true;
						NextShotTime = _currentFrame + 0.1f;
						ShootTimes--;
						BulletBase.TryShotBullet(EnemyWeapons[6].BulletData, ShootTransform.position, _transform.position + new Vector3(3f * (float)base.direction, -1.6f, 0f), null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
					}
					break;
				}
				break;
			case SubStatus.Phase4:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill4, SubStatus.Phase5);
				}
				break;
			case SubStatus.Phase5:
				if (Controller.Collisions.below)
				{
					_velocity.x = 0;
					PlayBossSE("BossSE02", "bs014_copyx22");
					SetStatus(MainStatus.Skill4, SubStatus.Phase6);
				}
				break;
			case SubStatus.Phase6:
				if (_currentFrame > 1f)
				{
					ElementStatus elementStatus = _elementStatus;
					if (elementStatus == ElementStatus.Fire)
					{
						SetStatus(MainStatus.Skill3);
					}
					else
					{
						SetStatus(MainStatus.Idle);
					}
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
				if (_velocity.y < 0)
				{
					SetStatus(MainStatus.Skill5, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				if (_velocity.y < 0)
				{
					SetStatus(MainStatus.Skill5, SubStatus.Phase3);
				}
				break;
			case SubStatus.Phase3:
				switch (_elementStatus)
				{
				case ElementStatus.Normal:
					if (_currentFrame > 1f || ShootTimes == 0)
					{
						SetStatus(MainStatus.Skill5, SubStatus.Phase4);
					}
					else if (ShootTimes > 0)
					{
						_isCharge = false;
						ShootTimes--;
						ShootBullet(EnemyWeapons[5].BulletData, TargetPos);
						Recoil();
						if (use_chargeshot != null)
						{
							use_chargeshot.Stop();
						}
					}
					break;
				case ElementStatus.Ice:
					if (_currentFrame > 2f || ShootTimes == 0)
					{
						_isCharge = false;
						IgnoreGravity = false;
						Recoil();
						SetStatus(MainStatus.Skill5, SubStatus.Phase4);
					}
					else if (_currentFrame > NextShotTime && ShootTimes > 0)
					{
						IgnoreGravity = true;
						ShootTimes--;
						ShootBullet(EnemyWeapons[11].BulletData, TargetPos);
						NextShotTime = _currentFrame + 0.3f;
						if (use_chargeshot != null)
						{
							use_chargeshot.Stop();
						}
					}
					break;
				case ElementStatus.Electric:
				case ElementStatus.Fire:
					SetStatus(MainStatus.Skill5, SubStatus.Phase4);
					break;
				}
				break;
			case SubStatus.Phase4:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill5, SubStatus.Phase5);
				}
				break;
			case SubStatus.Phase5:
				if (Controller.Collisions.below)
				{
					_velocity.x = 0;
					PlayBossSE("BossSE02", "bs014_copyx22");
					SetStatus(MainStatus.Skill5, SubStatus.Phase6);
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
		case MainStatus.Skill6:
			if (_currentFrame > 1f)
			{
				_characterMaterial.UpdateTex((int)_elementStatus % 4);
				SetStatus(MainStatus.Idle);
				mchip_maoh.isSetColor = true;
				if (_elementStatus == ElementStatus.Normal)
				{
					mchip_maoh.MeshActiveColor = getHdrColor(0);
				}
				else if (_elementStatus == ElementStatus.Fire)
				{
					mchip_maoh.MeshActiveColor = getHdrColor(1);
				}
				else if (_elementStatus == ElementStatus.Electric)
				{
					mchip_maoh.MeshActiveColor = getHdrColor(3);
				}
				else if (_elementStatus == ElementStatus.Ice)
				{
					mchip_maoh.MeshActiveColor = getHdrColor(2);
				}
				mchip_maoh.ActiveChipSkill(true);
			}
			break;
		case MainStatus.Skill7:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill7, SubStatus.Phase1);
					PlaySE("BossSE02", "bs014_copyx17_lp");
				}
				break;
			case SubStatus.Phase1:
			{
				HurtPassParam hurtPassParam = new HurtPassParam();
				hurtPassParam.dmg = -((int)MaxHp / 100);
				Hurt(hurtPassParam);
				if ((int)Hp >= ExpectedHP)
				{
					HpStatus = (int)Hp * HpStep / (int)MaxHp;
					SetStatus(MainStatus.Idle);
					PlaySE("BossSE02", "bs014_copyx17_stop");
				}
				break;
			}
			}
			break;
		case MainStatus.Die:
			switch (_subStatus)
			{
			case SubStatus.Phase1:
				if (!bIsClearStage)
				{
					if (Controller.Collisions.below)
					{
						SetStatus(MainStatus.Die, SubStatus.Phase2);
					}
				}
				else if (_currentFrame > 0.5f)
				{
					if (nDeadCount > 10)
					{
						SetStatus(MainStatus.Die, SubStatus.Phase3);
					}
					else
					{
						nDeadCount++;
					}
				}
				break;
			case SubStatus.Phase2:
				if (_currentFrame > 1f)
				{
					if (nDeadCount > 10)
					{
						SetStatus(MainStatus.Die, SubStatus.Phase3);
					}
					else
					{
						nDeadCount++;
					}
				}
				break;
			}
			break;
		case MainStatus.Fall:
		case MainStatus.SpecialMotion:
			break;
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

	public void UpdateFunc()
	{
		if (Activate || _mainStatus == MainStatus.Debut || _mainStatus == MainStatus.Die)
		{
			base.transform.localPosition = Vector3.MoveTowards(base.transform.localPosition, Controller.LogicPosition.vec3, distanceDelta);
		}
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

	public override void SetActive(bool isActive)
	{
		base.SetActive(isActive);
		if (isActive)
		{
			_collideBullet.UpdateBulletData(EnemyWeapons[0].BulletData);
			_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			_collideBullet.Active(targetMask);
			SlidingKickCollider.UpdateBulletData(EnemyWeapons[1].BulletData);
			SlidingKickCollider.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			StarCrushCollider.UpdateBulletData(EnemyWeapons[2].BulletData);
			StarCrushCollider.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			SetStatus(MainStatus.Debut);
		}
		else
		{
			_collideBullet.BackToPool();
			SlidingKickCollider.BackToPool();
			StarCrushCollider.BackToPool();
		}
	}

	public override ObscuredInt Hurt(HurtPassParam tHurtPassParam)
	{
		if (IsInvincible && _mainStatus != MainStatus.Skill7)
		{
			return Hp;
		}
		if (_mainStatus == MainStatus.Skill7 && (int)tHurtPassParam.dmg > 0)
		{
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
			if ((bool)_characterMaterial)
			{
				_characterMaterial.Hurt();
			}
		}
		else
		{
			DeadBehavior(ref tHurtPassParam);
		}
		return Hp;
	}

	protected BulletBase ShootBullet(SKILL_TABLE BulletData, VInt3 TargetPos)
	{
		return BulletBase.TryShotBullet(BulletData, ShootTransform.position, BulletVector.normalized, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
	}

	protected void SetAimDirection()
	{
		BulletVector = new Vector3((float)TargetPos.x - ShootTransform.position.x * 1000f, (float)TargetPos.y - ShootTransform.position.y * 1000f + 1000f, 0f);
		float num = Vector3.Angle(BulletVector.normalized, Vector3.right * base.direction);
		if (num > 90f)
		{
			BulletVector = new Vector3(0f, 1f, 0f);
			num = 90f;
		}
		else if (num < -90f)
		{
			BulletVector = new Vector3(0f, -1f, 0f);
			num = -90f;
		}
		if (BulletVector.y < 0f)
		{
			num *= -1f;
		}
		_animator.SetFloat(_HashAngle, num);
	}

	protected void Recoil()
	{
		_velocity.y += JumpSpeed / 2;
		_velocity.x = -base.direction * WalkSpeed / 8;
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
		if (IgnoreGravity)
		{
			if (!(Target == null) && StartTracking && !(Target.transform.position.y - _transform.position.y < 0f))
			{
				_velocity.y = (int)((Target.transform.position.y - _transform.position.y) * 1000f);
			}
			return;
		}
		if ((_velocity.y < 0 && Controller.Collisions.below) || (_velocity.y > 0 && Controller.Collisions.above))
		{
			_velocity.y = 0;
		}
		_velocity.y += OrangeBattleUtility.FP_Gravity * GameLogicUpdateManager.g_fixFrameLenFP / 1000;
		_velocity.y = IntMath.Sign(_velocity.y) * IntMath.Min(IntMath.Abs(_velocity.y), IntMath.Abs(_maxGravity.i));
	}

	protected override void DeadBehavior(ref HurtPassParam tHurtPassParam)
	{
		IgnoreGravity = false;
		if (_mainStatus != MainStatus.Die)
		{
			switch (AiState)
			{
			case AI_STATE.mob_002:
				base.DeadPlayCompleted = false;
				break;
			case AI_STATE.mob_003:
				base.DeadPlayCompleted = true;
				break;
			default:
				base.DeadPlayCompleted = false;
				break;
			case AI_STATE.mob_004:
				break;
			}
			if ((bool)_collideBullet)
			{
				_collideBullet.BackToPool();
			}
			if ((bool)SlidingKickCollider)
			{
				SlidingKickCollider.BackToPool();
			}
			if ((bool)StarCrushCollider)
			{
				StarCrushCollider.BackToPool();
			}
			if (use_skill0.isPlaying)
			{
				use_skill0.Stop();
			}
			if (use_skill1.isPlaying)
			{
				use_skill1.Stop();
			}
			if (use_chargeshot.isPlaying)
			{
				use_chargeshot.Stop();
			}
			StageUpdate.SlowStage();
			SetColliderEnable(false);
			SetStatus(MainStatus.Die);
		}
	}

	private int WeightRandom(int[] WeightArray, int SkillStart)
	{
		int num = 0;
		int num2 = 0;
		int num3 = WeightArray.Length;
		for (int i = 0; i < num3; i++)
		{
			num2 += WeightArray[i];
		}
		int num4 = OrangeBattleUtility.Random(0, num2);
		for (int j = 0; j < num3; j++)
		{
			num += WeightArray[j];
			if (num4 < num)
			{
				return j + SkillStart;
			}
		}
		return 0;
	}

	public override void UpdateEnemyID(int id)
	{
		base.UpdateEnemyID(id);
		UpdateAIState();
	}
}
