using System;
using CallbackDefs;
using Newtonsoft.Json;
using StageLib;
using UnityEngine;

public class BS090_Controller : EnemyControllerBase, IManagedUpdateBehavior
{
	private enum MainStatus
	{
		Idle = 0,
		Debut = 1,
		Hurt = 2,
		Dead = 3,
		AirIdle = 4,
		Skill0 = 5,
		Skill1 = 6,
		Skill2 = 7,
		Skill3 = 8,
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

	[SerializeField]
	private bool DebugMode;

	[SerializeField]
	private MainStatus NextSkill;

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
		MainStatus.Skill0,
		MainStatus.Skill1
	};

	private MainStatus[] AirStatus = new MainStatus[3]
	{
		MainStatus.Idle,
		MainStatus.Skill2,
		MainStatus.Skill3
	};

	private float _currentFrame;

	private Vector3 groundPos;

	private Transform _cameraTransform;

	private ParticleSystem matkefx;

	private ParticleSystem tornadoefx;

	private ParticleSystem Lwingefx;

	private ParticleSystem Rwingefx;

	private CollideBullet tornadobullet;

	private int fxFrame;

	private bool IsChipInfoAnim;

	private bool _bDeadCallResult = true;

	private int ShootFrame;

	private float[] ShootPosY = new float[3] { 0.6f, 1.5f, 2.4f };

	private int ShootTimes;

	private int lastpos;

	private int nDeadCount;

	private bool hasShoot;

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

	private bool _isGround;

	private MainStatus _previousStatus;

	private int _jobCounter;

	private LayerMask collisionMask;

	private LayerMask wallkickMask;

	private LayerMask collisionMaskThrough;

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
			}
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
		base.AimTransform = OrangeBattleUtility.FindChildRecursive(ref target, "Bip");
		matkefx = OrangeBattleUtility.FindChildRecursive(ref target, "atkefx").GetComponent<ParticleSystem>();
		tornadoefx = OrangeBattleUtility.FindChildRecursive(ref target, "tornadoefx").GetComponent<ParticleSystem>();
		Lwingefx = OrangeBattleUtility.FindChildRecursive(ref target, "fxduring_wing_L").GetComponent<ParticleSystem>();
		Rwingefx = OrangeBattleUtility.FindChildRecursive(ref target, "fxduring_wing_R").GetComponent<ParticleSystem>();
		_collideBullet = OrangeBattleUtility.FindChildRecursive(ref target, "CollideBullet").gameObject.AddOrGetComponent<CollideBullet>();
		tornadobullet = OrangeBattleUtility.FindChildRecursive(ref target, "tornadobullet").gameObject.AddOrGetComponent<CollideBullet>();
		_mouthShootTransform = OrangeBattleUtility.FindChildRecursive(ref target, "ShootPoint_Mouth");
		_busterShootTransform = OrangeBattleUtility.FindChildRecursive(ref target, "ShootPoint_Hand");
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
		base.SoundSource.Initial(OrangeSSType.BOSS);
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
			tornadobullet.UpdateBulletData(EnemyWeapons[2].BulletData);
			tornadobullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
		}
		else
		{
			_velocity = VInt3.zero;
			tornadobullet.BackToPool();
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
		case MainStatus.Skill2:
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
				if (matkefx.isPlaying)
				{
					matkefx.Stop();
				}
				AiTimer.TimerStart();
				Target = _enemyAutoAimSystem.GetClosetPlayer();
				if (dashAttackCount >= 3 || Target == null)
				{
					Controller.SetLogicPosition((int)groundPos.x * 1000, (int)(groundPos.y * 1000f) + 10000);
					SetWingFX(true);
					SetStatus(MainStatus.AirIdle, SubStatus.Phase2);
				}
				else
				{
					SetWingFX(false);
					SubStatus subStatus2 = _dashDirection[OrangeBattleUtility.Random(0, _dashDirection.Length)];
					dashAttackStatus = 0;
					dashAttackCount++;
					SetStatus(_mainStatus, subStatus2);
				}
				break;
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
		case MainStatus.Skill1:
			fxFrame = (int)(tornadoefx.main.duration * 20f) + GameLogicUpdateManager.GameFrame;
			UpdateMagazine(-1, true);
			break;
		case MainStatus.Skill3:
			hasShoot = false;
			UpdateMagazine(-1, true);
			break;
		case MainStatus.Skill0:
			switch (_subStatus)
			{
			case SubStatus.Phase1:
			{
				OrangeBattleUtility.GlobalVelocityExtra = VInt3.right * base.direction * 5;
				Vector3 position = _cameraTransform.position;
				position.z = 0f;
				position.y -= 4f;
				AiTimer.TimerStart();
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxduring_flame-eagle_wind_000", position, (base.direction == -1) ? OrangeBattleUtility.QuaternionNormal : OrangeBattleUtility.QuaternionReverse, Array.Empty<object>());
				ShootTimes = EnemyWeapons[4].BulletData.n_MAGAZINE;
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
				OrangeBattleUtility.GlobalVelocityExtra = VInt3.zero;
				_collideBullet.BackToPool();
				base.DeadPlayCompleted = true;
				base.AllowAutoAim = false;
				_velocity = VInt3.zero;
				OrangeBattleUtility.LockPlayer();
				PlayBossSE("BossSE", 15);
				break;
			case SubStatus.Phase1:
				_velocity.y = RunSpeed;
				MonoBehaviourSingleton<AudioManager>.Instance.Play("HitSE", 104);
				BattleInfoUI.Instance.ShowExplodeBG(base.gameObject);
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
		if ((!Activate || !_enemyAutoAimSystem) && _mainStatus != MainStatus.Debut)
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
				if (AiTimer.GetMillisecond() >= EnemyData.n_AI_TIMER)
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
				UpdateRandomState();
				break;
			}
			break;
		case MainStatus.Skill3:
			if (_subStatus != 0)
			{
				break;
			}
			if (_currentFrame > 1f * (float)EnemyWeapons[3].BulletData.n_MAGAZINE && EnemyWeapons[3].MagazineRemain <= 0f)
			{
				SetStatus(MainStatus.AirIdle, SubStatus.Phase2);
			}
			else if (_currentFrame % 1f > 0.5f && EnemyWeapons[3].MagazineRemain > 0f && !hasShoot)
			{
				Vector3 pDirection = Vector3.right * base.direction + Vector3.down;
				if ((bool)Target)
				{
					pDirection = Target.Controller.GetCenterPos() - _mouthShootTransform.position;
				}
				BulletBase.TryShotBullet(EnemyWeapons[3].BulletData, _mouthShootTransform, pDirection, null, selfBuffManager.sBuffStatus, EnemyData, targetMask, true);
				hasShoot = true;
				EnemyWeapons[3].LastUseTimer.TimerStart();
				EnemyWeapons[3].MagazineRemain -= 1f;
			}
			else if (_currentFrame % 1f < 0.5f && hasShoot)
			{
				hasShoot = false;
			}
			break;
		case MainStatus.Skill1:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if ((double)_currentFrame > 1.0)
				{
					SetStatus(_mainStatus, SubStatus.Phase1);
					PlayBossSE("BossSE", 14);
					tornadoefx.Play();
					tornadobullet.Active(targetMask);
				}
				break;
			case SubStatus.Phase1:
				if (GameLogicUpdateManager.GameFrame > fxFrame)
				{
					SetStatus(_mainStatus, SubStatus.Phase2);
					AiTimer.TimerStart();
					tornadoefx.Stop();
					tornadobullet.BackToPool();
				}
				break;
			case SubStatus.Phase2:
				if ((double)_currentFrame > 1.0)
				{
					PlayBossSE("BossSE", 15);
					SetStatus(MainStatus.Idle, SubStatus.Phase2);
				}
				break;
			}
			break;
		case MainStatus.Skill2:
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
						SetWingFX(false);
						SetStatus(_mainStatus, SubStatus.Phase2);
					}
				}
				else if (dashAttackStatus == 0)
				{
					SetWingFX(true);
					dashAttackStatus = 1;
				}
				break;
			case SubStatus.Phase10:
				if (!Lwingefx.isPlaying || !Rwingefx.isPlaying)
				{
					SetWingFX(true);
				}
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
					SetWingFX(false);
				}
				break;
			case SubStatus.Phase13:
				if (!Lwingefx.isPlaying || !Rwingefx.isPlaying)
				{
					SetWingFX(true);
				}
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
					SetWingFX(false);
				}
				break;
			case SubStatus.Phase2:
				break;
			}
			break;
		case MainStatus.Skill0:
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
				if (GameLogicUpdateManager.GameFrame >= ShootFrame && ShootTimes > 0)
				{
					ShootTimes--;
					lastpos = (lastpos + OrangeBattleUtility.Random(1, 3)) % ShootPosY.Length;
					Vector3 worldPos = _transform.position + new Vector3(3f, ShootPosY[lastpos], 0f);
					BulletBase.TryShotBullet(EnemyWeapons[4].BulletData, worldPos, Vector3.left, null, selfBuffManager.sBuffStatus, EnemyData, targetMask, true);
					ShootFrame = GameLogicUpdateManager.GameFrame + EnemyWeapons[4].BulletData.n_FIRE_SPEED * 20 / 1000;
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
				if (nDeadCount > 10)
				{
					SetStatus(_mainStatus, SubStatus.Phase1);
				}
				else
				{
					nDeadCount++;
				}
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
		}
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
				_currentAnimationId = AnimationID.ANI_IDLE_AIR;
				break;
			}
			return;
		case MainStatus.Hurt:
			_currentAnimationId = AnimationID.ANI_HURT;
			break;
		case MainStatus.Skill2:
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
		case MainStatus.Skill0:
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
		case MainStatus.Skill1:
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
		case MainStatus.Skill3:
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

	protected override void DeadBehavior(ref HurtPassParam tHurtPassParam)
	{
		if (_mainStatus != MainStatus.Dead)
		{
			if ((bool)_collideBullet)
			{
				_collideBullet.BackToPool();
			}
			if ((bool)tornadobullet)
			{
				tornadobullet.BackToPool();
			}
			if ((bool)matkefx)
			{
				matkefx.Stop();
			}
			if ((bool)tornadoefx)
			{
				tornadoefx.Stop();
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
		if (DebugMode)
		{
			mainStatus2 = NextSkill;
		}
		if (StageUpdate.gbIsNetGame)
		{
			if (StageUpdate.bIsHost)
			{
				StageUpdate.RegisterSendAndRun(sNetSerialID, (int)mainStatus2);
				SetStatus(MainStatus.IdleWaitNet);
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
		SetStatus(mainStatus2);
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

	private void SetWingFX(bool on)
	{
		Lwingefx.gameObject.SetActive(on);
		Rwingefx.gameObject.SetActive(on);
	}
}
