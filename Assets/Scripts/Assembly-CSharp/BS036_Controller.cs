#define RELEASE
using System;
using CallbackDefs;
using Newtonsoft.Json;
using StageLib;
using UnityEngine;

public class BS036_Controller : EnemyControllerBase, IManagedUpdateBehavior
{
	private enum MainStatus
	{
		Idle = 0,
		Debut = 1,
		Hurt = 2,
		Dead = 3,
		AirIdle = 4,
		SkillWind = 5,
		SkillTornadoBuster = 6,
		SkillDash = 7,
		SkillEgg = 8,
		IdleWaitNet = 9,
		MAX_STATUS = 10
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

	public enum AnimationID
	{
		ANI_IDLE_GROUND = 0,
		ANI_GROUND_TO_AIR = 1,
		ANI_IDLE_AIR = 2,
		ANI_LAND = 3,
		ANI_DEBUT = 4,
		ANI_HURT = 5,
		ANI_RUN = 6,
		ANI_DEAD = 7,
		ANI_SKILL_DASH_START = 8,
		ANI_SKILL_DASH_UP_LOOP = 9,
		ANI_SKILL_DASH_SIDE_LOOP = 10,
		ANI_SKILL_DASH_DOWNSIDE_LOOP = 11,
		ANI_SKILL_DASH_UPSIDE_LOOP = 12,
		ANI_SKILL_DASH_TURN = 13,
		ANI_SKILL_WIND_START = 14,
		ANI_SKILL_WIND_LOOP = 15,
		ANI_SKILL_WIND_END = 16,
		ANI_SKILL_TORNADO_START = 17,
		ANI_SKILL_TORNADO_LOOP = 18,
		ANI_SKILL_TORNADO_END = 19,
		ANI_SKILL_EGG_START = 20,
		MAX_ANIMATION_ID = 21
	}

	private AnimationID _currentAnimationId;

	public int RunSpeed = 3000;

	public int DashSpeed = 15000;

	private readonly int _hashHspd = Animator.StringToHash("fHspd");

	private readonly int _hashVspd = Animator.StringToHash("fVspd");

	private Transform _mouthShootTransform;

	private Transform _busterShootTransform;

	private int[] _animatorHash;

	private MainStatus _mainStatus;

	private SubStatus _subStatus;

	private MainStatus[] GroundStatus = new MainStatus[3]
	{
		MainStatus.AirIdle,
		MainStatus.SkillWind,
		MainStatus.SkillTornadoBuster
	};

	private MainStatus[] AirStatus = new MainStatus[3]
	{
		MainStatus.Idle,
		MainStatus.SkillDash,
		MainStatus.SkillEgg
	};

	private float _currentFrame;

	private Vector3 groundPos;

	private Transform _cameraTransform;

	private ParticleSystem matkefx;

	private bool IsChipInfoAnim;

	private bool _bDeadCallResult = true;

	private bool CanSumon;

	private string DashStep = "";

	private readonly SubStatus[] _dashDirection = new SubStatus[4]
	{
		SubStatus.Phase3,
		SubStatus.Phase4,
		SubStatus.Phase10,
		SubStatus.Phase13
	};

	private Vector3 distance;

	private int dashAttackStatus;

	private int dashAttackCount;

	private float _distance = 2.8f;

	private int useWeapon;

	private bool _isGround;

	private MainStatus _previousStatus;

	private int _jobCounter;

	private LayerMask collisionMask;

	private LayerMask wallkickMask;

	private LayerMask collisionMaskThrough;

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
		base.AimTransform = OrangeBattleUtility.FindChildRecursive(ref target, "Bip");
		matkefx = OrangeBattleUtility.FindChildRecursive(ref target, "atkefx").GetComponent<ParticleSystem>();
		_collideBullet = OrangeBattleUtility.FindChildRecursive(ref target, "CollideBullet").gameObject.AddOrGetComponent<CollideBullet>();
		_mouthShootTransform = OrangeBattleUtility.FindChildRecursive(ref target, "ShootPoint_Mouth");
		_busterShootTransform = OrangeBattleUtility.FindChildRecursive(ref target, "ShootHand");
		_animator = GetComponentInChildren<Animator>();
		_animatorHash = new int[21];
		for (int i = 0; i < 21; i++)
		{
			_animatorHash[i] = Animator.StringToHash("idle");
		}
		_animatorHash[0] = Animator.StringToHash("BS036@idle_ground_loop");
		_animatorHash[2] = Animator.StringToHash("BS036@idle_air_loop");
		_animatorHash[1] = Animator.StringToHash("BS036@rise_start");
		_animatorHash[4] = Animator.StringToHash("BS036@debut");
		_animatorHash[5] = Animator.StringToHash("BS036@hurt_loop");
		_animatorHash[7] = Animator.StringToHash("BS036@dead");
		_animatorHash[6] = Animator.StringToHash("BS036@run_forward_loop");
		_animatorHash[3] = Animator.StringToHash("BS036@landing");
		_animatorHash[8] = Animator.StringToHash("BS036@skill_01_start");
		_animatorHash[9] = Animator.StringToHash("BS036@skill_01_loop0");
		_animatorHash[10] = Animator.StringToHash("BS036@skill_01_loop1");
		_animatorHash[11] = Animator.StringToHash("BS036@skill_01_loop2");
		_animatorHash[12] = Animator.StringToHash("BS036@skill_01_loop3");
		_animatorHash[13] = Animator.StringToHash("BS036@skill_01_turn");
		_animatorHash[14] = Animator.StringToHash("BS036@skill_02_start");
		_animatorHash[15] = Animator.StringToHash("BS036@skill_02_loop");
		_animatorHash[16] = Animator.StringToHash("BS036@skill_02_end");
		_animatorHash[17] = Animator.StringToHash("BS036@skill_03_start");
		_animatorHash[18] = Animator.StringToHash("BS036@skill_03_loop");
		_animatorHash[19] = Animator.StringToHash("BS036@skill_03_end");
		_animatorHash[20] = Animator.StringToHash("BS036@skill_04_start");
		collisionMask = Controller.collisionMask;
		wallkickMask = Controller.wallkickMask;
		collisionMaskThrough = Controller.collisionMaskThrough;
		Camera mainCamera = MonoBehaviourSingleton<OrangeSceneManager>.Instance.MainCamera;
		if ((bool)mainCamera)
		{
			_cameraTransform = mainCamera.transform;
		}
		IgnoreGravity = true;
		IgnoreGravity_bak = IgnoreGravity;
		SetStatus(MainStatus.Debut);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxstory_explode_000", 10);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("FX_BOSS_EXPLODE2");
	}

	public override void SetChipInfoAnim()
	{
		IsChipInfoAnim = true;
		SetStatus(MainStatus.Idle);
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
			_bDeadCallResult = false;
			CanSumon = false;
			break;
		case AI_STATE.mob_003:
			CanSumon = true;
			_bDeadCallResult = false;
			break;
		default:
			_bDeadCallResult = true;
			CanSumon = false;
			break;
		}
	}

	public override void SetActive(bool isActive)
	{
		base.SetActive(isActive);
		if (isActive)
		{
			IgnoreGlobalVelocity = true;
			_collideBullet.UpdateBulletData(EnemyWeapons[0].BulletData);
			_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			_collideBullet.Active(targetMask);
		}
		else
		{
			_collideBullet.BackToPool();
		}
	}

	public void UpdateFunc()
	{
		if (Activate || _mainStatus == MainStatus.Debut)
		{
			_animator.SetFloat(_hashHspd, (float)_velocity.x * 0.001f);
			_animator.SetFloat(_hashVspd, (float)_velocity.y * 0.001f);
			base.transform.localPosition = Vector3.MoveTowards(base.transform.localPosition, Controller.LogicPosition.vec3, distanceDelta);
		}
	}

	private void SetStatus(MainStatus mainStatus, SubStatus subStatus = SubStatus.Phase0)
	{
		_mainStatus = mainStatus;
		_subStatus = subStatus;
		if (IsChipInfoAnim)
		{
			return;
		}
		switch (_mainStatus)
		{
		case MainStatus.Debut:
			switch (_subStatus)
			{
			case SubStatus.Phase1:
				_velocity.y = -RunSpeed;
				break;
			case SubStatus.Phase2:
				groundPos = _transform.position;
				break;
			case SubStatus.Phase4:
				if (IntroCallBack != null)
				{
					IntroCallBack();
				}
				break;
			}
			break;
		case MainStatus.Idle:
			base.AllowAutoAim = true;
			_collideBullet.UpdateBulletData(EnemyWeapons[0].BulletData);
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				SetControllerCollider(true);
				_velocity.x = 0;
				_velocity.y = -RunSpeed;
				break;
			case SubStatus.Phase2:
				_velocity.x = 0;
				_velocity.y = 0;
				break;
			}
			break;
		case MainStatus.AirIdle:
			base.AllowAutoAim = true;
			_collideBullet.UpdateBulletData(EnemyWeapons[0].BulletData);
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				SetControllerCollider(false);
				AiTimer.TimerStart();
				break;
			case SubStatus.Phase1:
				_velocity.x = 0;
				_velocity.y = RunSpeed;
				break;
			case SubStatus.Phase2:
				_velocity.x = 0;
				_velocity.y = -RunSpeed;
				UpdateDirection();
				break;
			case SubStatus.Phase3:
				_velocity.x = 0;
				_velocity.y = 0;
				break;
			}
			break;
		case MainStatus.SkillDash:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				base.AllowAutoAim = false;
				break;
			case SubStatus.Phase1:
				_velocity.y = DashSpeed;
				dashAttackCount = 0;
				_collideBullet.UpdateBulletData(EnemyWeapons[1].BulletData);
				break;
			case SubStatus.Phase2:
			{
				if (matkefx.isPlaying)
				{
					matkefx.Stop();
				}
				AiTimer.TimerStart();
				Target = _enemyAutoAimSystem.GetClosetPlayer();
				if (dashAttackCount >= 3 || Target == null || DashStep.Length <= 0)
				{
					Controller.SetLogicPosition((int)groundPos.x * 1000, (int)(groundPos.y * 1000f) + 10000);
					SetStatus(MainStatus.AirIdle, SubStatus.Phase2);
					break;
				}
				int num = 4;
				try
				{
					num = int.Parse(DashStep[0].ToString());
					DashStep = DashStep.Remove(0, 1);
				}
				catch
				{
					num = 4;
					Debug.LogError("散彈攻擊的位置序列中，含有非數字的符號。");
				}
				SubStatus subStatus2 = _dashDirection[num];
				dashAttackStatus = 0;
				dashAttackCount++;
				SetStatus(_mainStatus, subStatus2);
				break;
			}
			case SubStatus.Phase3:
				UpdateDirection(-1);
				Controller.SetLogicPosition(Target.Controller.LogicPosition.x - base.direction * 15000, Target.Controller.LogicPosition.y);
				_velocity.x = base.direction * DashSpeed;
				_velocity.y = 0;
				if (!matkefx.isPlaying)
				{
					matkefx.Play();
				}
				break;
			case SubStatus.Phase4:
				UpdateDirection(1);
				Controller.SetLogicPosition(Target.Controller.LogicPosition.x - base.direction * 15000, Target.Controller.LogicPosition.y);
				_velocity.x = base.direction * DashSpeed;
				_velocity.y = 0;
				if (!matkefx.isPlaying)
				{
					matkefx.Play();
				}
				break;
			case SubStatus.Phase5:
				Controller.SetLogicPosition(Target.Controller.LogicPosition.x, Target.Controller.LogicPosition.y - 15000);
				_velocity.x = 0;
				_velocity.y = DashSpeed;
				break;
			case SubStatus.Phase6:
				UpdateDirection(-1);
				Controller.SetLogicPosition(Target.Controller.LogicPosition.x - base.direction * 10606, Target.Controller.LogicPosition.y + 10606);
				_velocity.x = base.direction * DashSpeed;
				_velocity.y = -DashSpeed;
				break;
			case SubStatus.Phase7:
				UpdateDirection(-1);
				Controller.SetLogicPosition(Target.Controller.LogicPosition.x - base.direction * 10606, Target.Controller.LogicPosition.y - 10606);
				_velocity.x = base.direction * DashSpeed;
				_velocity.y = DashSpeed;
				break;
			case SubStatus.Phase8:
				UpdateDirection(1);
				Controller.SetLogicPosition(Target.Controller.LogicPosition.x - base.direction * 10606, Target.Controller.LogicPosition.y + 10606);
				_velocity.x = base.direction * DashSpeed;
				_velocity.y = -DashSpeed;
				break;
			case SubStatus.Phase9:
				UpdateDirection(1);
				Controller.SetLogicPosition(Target.Controller.LogicPosition.x - base.direction * 10606, Target.Controller.LogicPosition.y - 10606);
				_velocity.x = base.direction * DashSpeed;
				_velocity.y = DashSpeed;
				break;
			case SubStatus.Phase10:
				UpdateDirection(-1);
				Controller.SetLogicPosition(Target.Controller.LogicPosition.x - base.direction * 10606, Target.Controller.LogicPosition.y + 10606);
				_velocity.x = base.direction * DashSpeed;
				_velocity.y = -DashSpeed;
				if (!matkefx.isPlaying)
				{
					matkefx.Play();
				}
				PlayBossSE("BossSE", 8);
				break;
			case SubStatus.Phase11:
				LeanTween.value(base.gameObject, _velocity.vec3, Vector3.zero, 0.2f).setOnUpdate(delegate(Vector3 v)
				{
					_velocity = new VInt3(v);
					if (matkefx.isPlaying)
					{
						matkefx.Stop();
					}
				}).setOnComplete((Action)delegate
				{
					_velocity = VInt3.zero;
					if (matkefx.isPlaying)
					{
						matkefx.Stop();
					}
				});
				break;
			case SubStatus.Phase12:
				UpdateDirection(-1);
				_velocity.x = base.direction * DashSpeed;
				_velocity.y = DashSpeed;
				if (!matkefx.isPlaying)
				{
					matkefx.Play();
				}
				break;
			case SubStatus.Phase13:
				UpdateDirection(1);
				Controller.SetLogicPosition(Target.Controller.LogicPosition.x - base.direction * 10606, Target.Controller.LogicPosition.y + 10606);
				_velocity.x = base.direction * DashSpeed;
				_velocity.y = -DashSpeed;
				if (!matkefx.isPlaying)
				{
					matkefx.Play();
				}
				PlayBossSE("BossSE", 8);
				break;
			case SubStatus.Phase14:
				LeanTween.value(base.gameObject, _velocity.vec3, Vector3.zero, 0.2f).setOnUpdate(delegate(Vector3 v)
				{
					_velocity = new VInt3(v);
					if (matkefx.isPlaying)
					{
						matkefx.Stop();
					}
				}).setOnComplete((Action)delegate
				{
					_velocity = VInt3.zero;
					if (matkefx.isPlaying)
					{
						matkefx.Stop();
					}
				});
				break;
			case SubStatus.Phase15:
				UpdateDirection(1);
				_velocity.x = base.direction * DashSpeed;
				_velocity.y = DashSpeed;
				if (!matkefx.isPlaying)
				{
					matkefx.Play();
				}
				break;
			}
			break;
		case MainStatus.SkillTornadoBuster:
			UpdateMagazine(-1, true);
			if (_subStatus == SubStatus.Phase0 && CanSumon)
			{
				MonoBehaviourSingleton<OrangeBattleUtility>.Instance.CallSummonEnemyEvent(_transform);
			}
			break;
		case MainStatus.SkillEgg:
			UpdateMagazine(-1, true);
			break;
		case MainStatus.SkillWind:
			switch (_subStatus)
			{
			case SubStatus.Phase1:
			{
				OrangeBattleUtility.GlobalVelocityExtra = VInt3.right * base.direction * 5;
				Vector3 position = _cameraTransform.position;
				position.z = 0f;
				position.y -= 4f;
				AiTimer.TimerStart();
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxduring_eagle_001", position, (base.direction == -1) ? OrangeBattleUtility.QuaternionNormal : OrangeBattleUtility.QuaternionReverse, Array.Empty<object>());
				break;
			}
			case SubStatus.Phase2:
				OrangeBattleUtility.GlobalVelocityExtra = VInt3.zero;
				break;
			}
			_velocity.x = 0;
			break;
		case MainStatus.Dead:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (matkefx.isPlaying)
				{
					matkefx.Stop();
				}
				OrangeBattleUtility.GlobalVelocityExtra = VInt3.zero;
				_collideBullet.BackToPool();
				base.AllowAutoAim = false;
				_velocity = VInt3.zero;
				OrangeBattleUtility.LockPlayer();
				base.SoundSource.PlaySE("BossSE", 15);
				break;
			case SubStatus.Phase1:
				if (_bDeadCallResult)
				{
					StartCoroutine(BossDieFlow(base.AimTransform));
				}
				else
				{
					StartCoroutine(BossDieFlow(base.AimTransform, "FX_BOSS_EXPLODE2", false, false));
				}
				break;
			}
			break;
		}
		UpdateAnimation();
	}

	public override void LogicUpdate()
	{
		if (_mainStatus == MainStatus.Debut && !Activate)
		{
			BaseUpdate();
			_currentFrame = _animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
			MainStatus mainStatus = _mainStatus;
			if (mainStatus == MainStatus.Debut)
			{
				switch (_subStatus)
				{
				case SubStatus.Phase0:
					SetStatus(_mainStatus, SubStatus.Phase1);
					break;
				case SubStatus.Phase1:
					if (Controller.Collisions.below)
					{
						SetStatus(_mainStatus, SubStatus.Phase2);
					}
					break;
				case SubStatus.Phase2:
					if ((double)_currentFrame > 1.0)
					{
						SetStatus(_mainStatus, SubStatus.Phase3);
					}
					break;
				case SubStatus.Phase3:
					if (_introReady)
					{
						SetStatus(_mainStatus, SubStatus.Phase4);
					}
					break;
				case SubStatus.Phase4:
					if (_unlockReady)
					{
						SetStatus(MainStatus.Idle, SubStatus.Phase2);
					}
					break;
				}
			}
			else
			{
				SetStatus(MainStatus.Idle, SubStatus.Phase2);
			}
			UpdateGravity();
			Controller.Move((_velocity + _velocityExtra) * GameLogicUpdateManager.m_fFrameLen + _velocityShift);
			distanceDelta = Vector3.Distance(base.transform.localPosition, Controller.LogicPosition.vec3) * (Time.deltaTime / GameLogicUpdateManager.m_fFrameLen);
			_velocityExtra = VInt3.zero;
			_velocityShift = VInt3.zero;
		}
		if (!Activate || !_enemyAutoAimSystem || _mainStatus == MainStatus.Debut)
		{
			if (_mainStatus == MainStatus.Debut && _subStatus == SubStatus.Phase4 && _unlockReady)
			{
				SetStatus(MainStatus.Idle, SubStatus.Phase2);
			}
			return;
		}
		UpdateMagazine();
		base.LogicUpdate();
		_currentFrame = _animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
		switch (_mainStatus)
		{
		case MainStatus.Debut:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				SetStatus(_mainStatus, SubStatus.Phase1);
				break;
			case SubStatus.Phase1:
				if (Controller.Collisions.below)
				{
					SetStatus(_mainStatus, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				if ((double)_currentFrame > 1.0)
				{
					SetStatus(_mainStatus, SubStatus.Phase3);
				}
				break;
			case SubStatus.Phase3:
				if (_introReady)
				{
					SetStatus(_mainStatus, SubStatus.Phase4);
				}
				break;
			case SubStatus.Phase4:
				if (_unlockReady)
				{
					SetStatus(MainStatus.Idle, SubStatus.Phase2);
				}
				break;
			}
			break;
		case MainStatus.Idle:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (Controller.Collisions.below)
				{
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
				if (AiTimer.GetMillisecond() >= EnemyData.n_AI_TIMER && !bWaitNetStatus)
				{
					UpdateRandomState();
				}
				break;
			}
			break;
		case MainStatus.AirIdle:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if ((double)_currentFrame > 1.0)
				{
					SetStatus(_mainStatus, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if (_transform.position.y > groundPos.y + 3f)
				{
					SetStatus(_mainStatus, SubStatus.Phase3);
				}
				break;
			case SubStatus.Phase2:
				if (_transform.position.y < groundPos.y + 3f)
				{
					SetStatus(_mainStatus, SubStatus.Phase3);
				}
				break;
			case SubStatus.Phase3:
				if (!bWaitNetStatus)
				{
					UpdateRandomState();
				}
				break;
			}
			break;
		case MainStatus.SkillEgg:
			if (_subStatus == SubStatus.Phase0)
			{
				if ((double)_currentFrame > 1.0)
				{
					SetStatus(MainStatus.AirIdle, SubStatus.Phase2);
				}
				else if (_currentFrame > 0.5f && EnemyWeapons[useWeapon].MagazineRemain > 0f)
				{
					useWeapon = 3;
					Transform mouthShootTransform = _mouthShootTransform;
					BulletBase.TryShotBullet(EnemyWeapons[useWeapon].BulletData, mouthShootTransform, Vector3.right * base.direction + Vector3.down, null, selfBuffManager.sBuffStatus, EnemyData, targetMask, true);
					EnemyWeapons[useWeapon].LastUseTimer.TimerStart();
					EnemyWeapons[useWeapon].MagazineRemain -= 1f;
				}
			}
			break;
		case MainStatus.SkillTornadoBuster:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if ((double)_currentFrame > 1.0)
				{
					SetStatus(_mainStatus, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				useWeapon = 2;
				if (EnemyWeapons[useWeapon].MagazineRemain > 0f && _currentFrame > 1f && (!EnemyWeapons[useWeapon].LastUseTimer.IsStarted() || EnemyWeapons[useWeapon].LastUseTimer.GetMillisecond() > EnemyWeapons[useWeapon].BulletData.n_FIRE_SPEED))
				{
					Transform busterShootTransform = _busterShootTransform;
					BulletBase.TryShotBullet(EnemyWeapons[useWeapon].BulletData, busterShootTransform, Vector3.right * base.direction, null, selfBuffManager.sBuffStatus, EnemyData, targetMask, true);
					PlaySE("BossSE", 14);
					EnemyWeapons[useWeapon].LastUseTimer.TimerStart();
					EnemyWeapons[useWeapon].MagazineRemain -= 1f;
				}
				if (EnemyWeapons[useWeapon].MagazineRemain == 0f)
				{
					SetStatus(_mainStatus, SubStatus.Phase2);
					AiTimer.TimerStart();
				}
				break;
			case SubStatus.Phase2:
				if ((double)_currentFrame > 1.0)
				{
					base.SoundSource.PlaySE("BossSE", 15, 0.5f);
					SetStatus(MainStatus.Idle, SubStatus.Phase2);
				}
				break;
			}
			break;
		case MainStatus.SkillDash:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if ((double)_currentFrame > 1.0)
				{
					SetStatus(_mainStatus, SubStatus.Phase1);
				}
				else if (_currentFrame > 0.6f)
				{
					_velocity.y = DashSpeed;
				}
				break;
			case SubStatus.Phase1:
				if (_transform.position.y > groundPos.y + 10f)
				{
					SetStatus(_mainStatus, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase3:
			case SubStatus.Phase4:
			case SubStatus.Phase5:
			case SubStatus.Phase6:
			case SubStatus.Phase7:
			case SubStatus.Phase8:
			case SubStatus.Phase9:
				if (!OrangeBattleUtility.IsInsideScreen(_transform.position))
				{
					if (dashAttackStatus == 1 || AiTimer.GetMillisecond() > 5000)
					{
						SetStatus(_mainStatus, SubStatus.Phase2);
					}
				}
				else if (dashAttackStatus == 0)
				{
					dashAttackStatus = 1;
				}
				break;
			case SubStatus.Phase10:
				if (_transform.position.y < groundPos.y + _distance)
				{
					SetStatus(_mainStatus, SubStatus.Phase11);
				}
				break;
			case SubStatus.Phase11:
				if ((double)_currentFrame > 1.0 && _velocity == VInt3.zero)
				{
					SetStatus(_mainStatus, SubStatus.Phase12);
				}
				break;
			case SubStatus.Phase12:
				if (!OrangeBattleUtility.IsInsideScreen(_transform.position))
				{
					SetStatus(_mainStatus, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase13:
				if (_transform.position.y < groundPos.y + _distance)
				{
					SetStatus(_mainStatus, SubStatus.Phase14);
				}
				break;
			case SubStatus.Phase14:
				if ((double)_currentFrame > 1.0 && _velocity == VInt3.zero)
				{
					SetStatus(_mainStatus, SubStatus.Phase15);
				}
				break;
			case SubStatus.Phase15:
				if (!OrangeBattleUtility.IsInsideScreen(_transform.position))
				{
					SetStatus(_mainStatus, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				break;
			}
			break;
		case MainStatus.SkillWind:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if ((double)_currentFrame > 1.0)
				{
					SetStatus(_mainStatus, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if (AiTimer.GetMillisecond() > 5000)
				{
					SetStatus(_mainStatus, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				if ((double)_currentFrame > 1.0)
				{
					SetStatus(MainStatus.Idle, SubStatus.Phase2);
				}
				break;
			}
			break;
		case MainStatus.Dead:
			if (_subStatus == SubStatus.Phase0 && (double)_currentFrame > 0.4)
			{
				SetStatus(_mainStatus, SubStatus.Phase1);
			}
			break;
		default:
			SetStatus(MainStatus.Idle, SubStatus.Phase2);
			break;
		case MainStatus.IdleWaitNet:
			break;
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
			if (netSyncData.nParam0 != 0)
			{
				_jobCounter = netSyncData.nParam0;
			}
			if (netSyncData.sParam0 != null && netSyncData.sParam0 != string.Empty && nSet == 7)
			{
				DashStep = netSyncData.sParam0;
			}
		}
		_previousStatus = (MainStatus)nSet;
		SetStatus((MainStatus)nSet);
	}

	private void UpdateAnimation()
	{
		switch (_mainStatus)
		{
		case MainStatus.Debut:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_IDLE_GROUND;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_IDLE_AIR;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_LAND;
				break;
			case SubStatus.Phase3:
				_currentAnimationId = AnimationID.ANI_DEBUT;
				break;
			case SubStatus.Phase4:
				return;
			default:
				throw new ArgumentOutOfRangeException();
			}
			break;
		case MainStatus.Idle:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_IDLE_AIR;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_LAND;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_IDLE_GROUND;
				break;
			}
			break;
		case MainStatus.AirIdle:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_GROUND_TO_AIR;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_IDLE_AIR;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_IDLE_AIR;
				break;
			}
			break;
		case MainStatus.Dead:
			if (_subStatus == SubStatus.Phase0)
			{
				_currentAnimationId = (Controller.BelowInBypassRange ? AnimationID.ANI_DEAD : AnimationID.ANI_HURT);
				break;
			}
			return;
		case MainStatus.Hurt:
			_currentAnimationId = AnimationID.ANI_HURT;
			break;
		case MainStatus.SkillDash:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_SKILL_DASH_START;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_SKILL_DASH_UP_LOOP;
				break;
			case SubStatus.Phase3:
				_currentAnimationId = AnimationID.ANI_SKILL_DASH_SIDE_LOOP;
				break;
			case SubStatus.Phase4:
				_currentAnimationId = AnimationID.ANI_SKILL_DASH_SIDE_LOOP;
				break;
			case SubStatus.Phase5:
				_currentAnimationId = AnimationID.ANI_SKILL_DASH_UP_LOOP;
				break;
			case SubStatus.Phase6:
				_currentAnimationId = AnimationID.ANI_SKILL_DASH_DOWNSIDE_LOOP;
				break;
			case SubStatus.Phase7:
				_currentAnimationId = AnimationID.ANI_SKILL_DASH_UPSIDE_LOOP;
				break;
			case SubStatus.Phase8:
				_currentAnimationId = AnimationID.ANI_SKILL_DASH_DOWNSIDE_LOOP;
				break;
			case SubStatus.Phase9:
				_currentAnimationId = AnimationID.ANI_SKILL_DASH_UPSIDE_LOOP;
				break;
			case SubStatus.Phase10:
				_currentAnimationId = AnimationID.ANI_SKILL_DASH_DOWNSIDE_LOOP;
				break;
			case SubStatus.Phase11:
				_currentAnimationId = AnimationID.ANI_SKILL_DASH_TURN;
				break;
			case SubStatus.Phase12:
				_currentAnimationId = AnimationID.ANI_SKILL_DASH_UPSIDE_LOOP;
				break;
			case SubStatus.Phase13:
				_currentAnimationId = AnimationID.ANI_SKILL_DASH_DOWNSIDE_LOOP;
				break;
			case SubStatus.Phase14:
				_currentAnimationId = AnimationID.ANI_SKILL_DASH_TURN;
				break;
			case SubStatus.Phase15:
				_currentAnimationId = AnimationID.ANI_SKILL_DASH_UPSIDE_LOOP;
				break;
			}
			break;
		case MainStatus.SkillWind:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_SKILL_WIND_START;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_SKILL_WIND_LOOP;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_SKILL_WIND_END;
				break;
			}
			break;
		case MainStatus.SkillTornadoBuster:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_SKILL_TORNADO_START;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_SKILL_TORNADO_LOOP;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_SKILL_TORNADO_END;
				break;
			}
			break;
		case MainStatus.SkillEgg:
			_currentAnimationId = AnimationID.ANI_SKILL_EGG_START;
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
		if (_mainStatus != MainStatus.Dead)
		{
			CanSumon = false;
			if ((bool)_collideBullet)
			{
				_collideBullet.BackToPool();
			}
			StageUpdate.SlowStage();
			SetStatus(MainStatus.Dead);
		}
	}

	protected void UpdateRandomState()
	{
		MainStatus mainStatus = _mainStatus;
		if (mainStatus != 0 && mainStatus == MainStatus.AirIdle)
		{
			_isGround = false;
		}
		else
		{
			_isGround = true;
		}
		int num = 10;
		MainStatus mainStatus2;
		do
		{
			mainStatus2 = (_isGround ? GroundStatus[OrangeBattleUtility.Random(0, GroundStatus.Length)] : AirStatus[OrangeBattleUtility.Random(0, AirStatus.Length)]);
			num--;
		}
		while ((mainStatus2 == _previousStatus && num > 0) || ((mainStatus2 == MainStatus.Idle || mainStatus2 == MainStatus.AirIdle) && _jobCounter == 0));
		if (StageUpdate.gbIsNetGame)
		{
			if (mainStatus2 == MainStatus.Idle || mainStatus2 == MainStatus.AirIdle)
			{
				_jobCounter = 0;
			}
			else
			{
				_jobCounter++;
			}
			if (!CheckHost())
			{
				return;
			}
			if (mainStatus2 == MainStatus.SkillDash)
			{
				DashStep = "";
				for (int i = 0; i < 3; i++)
				{
					DashStep += OrangeBattleUtility.Random(0, _dashDirection.Length);
				}
				UploadEnemyStatus((int)mainStatus2, false, new object[1] { _jobCounter }, new object[1] { DashStep });
			}
			else
			{
				UploadEnemyStatus((int)mainStatus2, false, new object[1] { _jobCounter });
			}
			return;
		}
		if (mainStatus2 == MainStatus.Idle || mainStatus2 == MainStatus.AirIdle)
		{
			_jobCounter = 0;
		}
		else
		{
			_jobCounter++;
		}
		if (CheckHost())
		{
			if (mainStatus2 == MainStatus.SkillDash)
			{
				DashStep = "";
				for (int j = 0; j < 3; j++)
				{
					DashStep += OrangeBattleUtility.Random(0, _dashDirection.Length);
				}
				UploadEnemyStatus((int)mainStatus2, false, new object[1] { _jobCounter }, new object[1] { DashStep });
			}
			else
			{
				UploadEnemyStatus((int)mainStatus2, false, new object[1] { _jobCounter });
			}
		}
		_previousStatus = mainStatus2;
	}

	private void SetControllerCollider(bool enable)
	{
		if (!enable)
		{
			Controller.collisionMask = 0;
			Controller.wallkickMask = 0;
			Controller.collisionMaskThrough = 0;
		}
		else
		{
			Controller.collisionMask = collisionMask;
			Controller.wallkickMask = wallkickMask;
			Controller.collisionMaskThrough = collisionMaskThrough;
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
}
