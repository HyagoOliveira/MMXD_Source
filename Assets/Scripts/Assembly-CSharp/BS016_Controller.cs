using System;
using System.Collections;
using CallbackDefs;
using NaughtyAttributes;
using Newtonsoft.Json;
using StageLib;
using UnityEngine;

public class BS016_Controller : EnemyControllerBase, IManagedUpdateBehavior
{
	private enum MainStatus
	{
		Idle = 0,
		Debut = 1,
		Walk = 2,
		Dead = 3,
		Punch = 4,
		DashAttack = 5,
		SemiCharge = 6,
		Charge = 7,
		Trap = 8,
		SceAct = 9
	}

	private enum SubStatus
	{
		Phase0 = 0,
		Phase1 = 1,
		Phase2 = 2,
		Phase3 = 3,
		MAX_SUBSTATUS = 4
	}

	public enum AnimationID
	{
		ANI_DEBUT = 0,
		ANI_IDLE = 1,
		ANI_WALK = 2,
		ANI_DEAD = 3,
		ANI_PUNCH_START = 4,
		ANI_PUNCH_LOOP = 5,
		ANI_PUNCH_END = 6,
		ANI_DASH_START = 7,
		ANI_DASH_LOOP = 8,
		ANI_DASH_END = 9,
		ANI_SEMICHARGE_START = 10,
		ANI_SEMICHARGE_END = 11,
		ANI_CHARGE_START = 12,
		ANI_CHARGE_LOOP = 13,
		ANI_CHARGE_END = 14,
		ANI_TRAP_START = 15,
		ANI_TRAP_LOOP = 16,
		ANI_TRAP_END = 17,
		ANI_HURT = 18,
		MAX_ANIMATION_ID = 19
	}

	[SerializeField]
	protected Transform[] ModelMesh;

	[SerializeField]
	protected Transform HipJoint;

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

	private readonly int _hashHspd = Animator.StringToHash("fHspd");

	private int _bulletCount;

	private CollideBullet _punchCollideBullet;

	private ParticleSystem _ChargeEffect;

	public int WalkSpeed = 1500;

	public int DashSpeed = 7500;

	public int TrapShiftX = 6000;

	public int TrapShiftY = 54000;

	public float punchDistance = 2.8f;

	private Transform _LeftShootPoint;

	private Transform _RightShootPoint;

	private Transform _CannonShootPoint;

	private Transform _TrapShootPoint;

	private ParticleSystem _efx_DashAttack1;

	private ParticleSystem _efx_DashAttack2;

	private int nActFrameCount;

	private int nSaveEventID;

	private bool _bDeadCallResult = true;

	private bool _bMultiBoss;

	private SKILL_TABLE tSceBulletSkill;

	private bool playStartVoice;

	private int _comboIndex;

	private MainStatus[] _combos = new MainStatus[8]
	{
		MainStatus.Trap,
		MainStatus.SemiCharge,
		MainStatus.Charge,
		MainStatus.Trap,
		MainStatus.DashAttack,
		MainStatus.SemiCharge,
		MainStatus.SemiCharge,
		MainStatus.Charge
	};

	private bool isFalled;

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
		_animator = GetComponentInChildren<Animator>();
		_collideBullet = base.gameObject.AddOrGetComponent<CollideBullet>();
		_collideBullet.isForceSE = (_collideBullet.isBossBullet = true);
		Transform[] target = _transform.GetComponentsInChildren<Transform>(true);
		ModelTransform = OrangeBattleUtility.FindChildRecursive(ref target, "model", true);
		_LeftShootPoint = OrangeBattleUtility.FindChildRecursive(ref target, "ShootPoint_L", true);
		_RightShootPoint = OrangeBattleUtility.FindChildRecursive(ref target, "ShootPoint_R", true);
		_CannonShootPoint = OrangeBattleUtility.FindChildRecursive(ref target, "ShootPoint_C", true);
		_TrapShootPoint = OrangeBattleUtility.FindChildRecursive(ref target, "ShootPoint_Trap", true);
		_ChargeEffect = OrangeBattleUtility.FindChildRecursive(ref target, "ChargeEffect", true).GetComponent<ParticleSystem>();
		_efx_DashAttack1 = OrangeBattleUtility.FindChildRecursive(ref target, "efx_DashAttack1", true).GetComponent<ParticleSystem>();
		_efx_DashAttack2 = OrangeBattleUtility.FindChildRecursive(ref target, "efx_DashAttack2", true).GetComponent<ParticleSystem>();
		_punchCollideBullet = _RightShootPoint.gameObject.AddOrGetComponent<CollideBullet>();
		_punchCollideBullet.isForceSE = (_punchCollideBullet.isBossBullet = true);
		_animationHash = new int[19];
		_animationHash[1] = Animator.StringToHash("BS016@idle_loop");
		_animationHash[2] = Animator.StringToHash("BS016@run2_loop");
		_animationHash[3] = Animator.StringToHash("BS016@dead");
		_animationHash[0] = Animator.StringToHash("BS016@debut");
		_animationHash[4] = Animator.StringToHash("BS016@skill_01s_start");
		_animationHash[5] = Animator.StringToHash("BS016@skill_01s_loop");
		_animationHash[6] = Animator.StringToHash("BS016@skill_01s_end");
		_animationHash[7] = Animator.StringToHash("BS016@skill_01_start");
		_animationHash[8] = Animator.StringToHash("BS016@skill_01_loop");
		_animationHash[9] = Animator.StringToHash("BS016@skill_01_end");
		_animationHash[12] = Animator.StringToHash("BS016@skill_02_start");
		_animationHash[13] = Animator.StringToHash("BS016@skill_02_loop");
		_animationHash[14] = Animator.StringToHash("BS016@skill_02_end");
		_animationHash[10] = Animator.StringToHash("BS016@skill_03_start");
		_animationHash[11] = Animator.StringToHash("BS016@skill_03_end");
		_animationHash[15] = Animator.StringToHash("BS016@skill_04_start");
		_animationHash[16] = Animator.StringToHash("BS016@skill_04_loop");
		_animationHash[17] = Animator.StringToHash("BS016@skill_04_end");
		_animationHash[18] = Animator.StringToHash("BS016@hurt_loop");
		SetStatus(MainStatus.Idle);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxstory_explode_000", 10);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("FX_BOSS_EXPLODE2");
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxhit_vava_mk2_003", 2);
		base.AimPoint = new Vector3(0f, 1f, 0f);
		OrangeBattleUtility.AddEnemyAutoAimSystem(_transform, out _enemyAutoAimSystem);
		_enemyAutoAimSystem.UpdateAimRange(20f);
		tSceBulletSkill = ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT[900005];
		StageResManager.LoadBulletBySkillTable(tSceBulletSkill);
		MonoBehaviourSingleton<AudioManager>.Instance.PreloadAtomSource("CharaSE_VAVA", 2);
		_bDeadPlayCompleted = false;
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

	public override bool CheckActStatus(int mainstatus, int substatus)
	{
		if (substatus == -1 && _mainStatus == (MainStatus)mainstatus)
		{
			return true;
		}
		if (_mainStatus == (MainStatus)mainstatus && _subStatus == (SubStatus)substatus)
		{
			return true;
		}
		return false;
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

	private void SettingDirection()
	{
		switch (_comboIndex)
		{
		case 1:
		case 3:
		case 4:
		case 7:
		case 8:
			UpdateDirection(base.direction);
			break;
		case 2:
		case 5:
		case 6:
			UpdateDirection(base.direction * -1);
			break;
		}
	}

	private void SetStatus(MainStatus mainStatus, SubStatus subStatus = SubStatus.Phase0)
	{
		_mainStatus = mainStatus;
		_subStatus = subStatus;
		switch (_mainStatus)
		{
		case MainStatus.Idle:
			_velocity.x = 0;
			break;
		case MainStatus.Dead:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (!Controller.Collisions.below)
				{
					IgnoreGravity = true;
				}
				_collideBullet.BackToPool();
				base.AllowAutoAim = false;
				_ChargeEffect.Stop();
				_efx_DashAttack1.Stop();
				_efx_DashAttack2.Stop();
				_velocity.x = 0;
				_comboIndex = 0;
				OrangeBattleUtility.LockPlayer();
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
		case MainStatus.Punch:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_bulletCount = 0;
				break;
			}
			_velocity.x = 0;
			break;
		case MainStatus.DashAttack:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_bulletCount = 0;
				if (AiState != AI_STATE.mob_003)
				{
					UpdateDirection();
				}
				break;
			case SubStatus.Phase1:
				PlaySE("BossSE", 29);
				break;
			case SubStatus.Phase2:
				PlaySE("BossSE", 28);
				_efx_DashAttack1.Stop();
				_efx_DashAttack2.Stop();
				_punchCollideBullet.BackToPool();
				break;
			}
			break;
		case MainStatus.Charge:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_bulletCount = 0;
				break;
			case SubStatus.Phase1:
				PlaySE("BossSE", 126);
				_ChargeEffect.Play();
				break;
			case SubStatus.Phase2:
				PlaySE("BossSE", 127);
				_ChargeEffect.Stop();
				BulletBase.TryShotBullet(EnemyWeapons[1].BulletData, _CannonShootPoint, Vector3.right * base.direction, null, selfBuffManager.sBuffStatus, EnemyData, targetMask, true);
				BulletBase.TryShotBullet(EnemyWeapons[2].BulletData, _CannonShootPoint, Vector3.right * base.direction, null, selfBuffManager.sBuffStatus, EnemyData, targetMask, true);
				break;
			}
			break;
		case MainStatus.SemiCharge:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_bulletCount = 0;
				break;
			case SubStatus.Phase1:
				PlaySE("BossSE", 30);
				BulletBase.TryShotBullet(EnemyWeapons[2].BulletData, _CannonShootPoint, Vector3.right * base.direction, null, selfBuffManager.sBuffStatus, EnemyData, targetMask, true);
				break;
			}
			break;
		case MainStatus.Trap:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_bulletCount = 0;
				if (AiState == AI_STATE.mob_002 || AiState == AI_STATE.mob_003)
				{
					MonoBehaviourSingleton<OrangeBattleUtility>.Instance.CallSummonEnemyEvent(_transform);
				}
				break;
			}
			break;
		case MainStatus.SceAct:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_bulletCount = 0;
				break;
			case SubStatus.Phase1:
				PlaySE("BossSE", 126);
				_ChargeEffect.Play();
				break;
			case SubStatus.Phase2:
				PlaySE("BossSE", 127);
				_ChargeEffect.Stop();
				BulletBase.TryShotBullet(tSceBulletSkill, _CannonShootPoint, Vector3.right * base.direction, null, selfBuffManager.sBuffStatus, EnemyData, targetMask, true);
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
			_currentAnimationId = AnimationID.ANI_DEBUT;
			break;
		case MainStatus.Idle:
			_currentAnimationId = AnimationID.ANI_IDLE;
			break;
		case MainStatus.Walk:
			_currentAnimationId = AnimationID.ANI_WALK;
			break;
		case MainStatus.Punch:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_PUNCH_START;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_PUNCH_LOOP;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_PUNCH_END;
				break;
			}
			break;
		case MainStatus.DashAttack:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_DASH_START;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_DASH_LOOP;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_DASH_END;
				break;
			}
			break;
		case MainStatus.Charge:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_CHARGE_START;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_CHARGE_LOOP;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_CHARGE_END;
				break;
			}
			break;
		case MainStatus.SemiCharge:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_SEMICHARGE_START;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_SEMICHARGE_END;
				break;
			}
			break;
		case MainStatus.Trap:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_bulletCount = 0;
				break;
			}
			break;
		case MainStatus.Dead:
			if (_subStatus == SubStatus.Phase0)
			{
				_currentAnimationId = ((!Controller.Collisions.below) ? AnimationID.ANI_HURT : AnimationID.ANI_DEAD);
				break;
			}
			return;
		case MainStatus.SceAct:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_CHARGE_START;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_CHARGE_LOOP;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_CHARGE_END;
				break;
			}
			break;
		}
		_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
	}

	public override void LogicUpdate()
	{
		_currentFrame = _animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
		if (Activate)
		{
			nActFrameCount++;
			base.LogicUpdate();
			switch (_mainStatus)
			{
			case MainStatus.Idle:
				Target = _enemyAutoAimSystem.GetClosetPlayer();
				if ((bool)Target)
				{
					UpdateRandomState();
				}
				break;
			case MainStatus.Punch:
				switch (_subStatus)
				{
				case SubStatus.Phase0:
					if (_currentFrame >= 1f)
					{
						SetStatus(_mainStatus, SubStatus.Phase2);
					}
					else
					{
						if (!(_currentFrame > 0.67f) || _bulletCount != 0)
						{
							break;
						}
						PlaySE("BossSE", 25);
						_bulletCount++;
						if (_bMultiBoss)
						{
							_punchCollideBullet.Active(targetMask);
						}
						else
						{
							_punchCollideBullet.Active(neutralMask);
						}
						if ((bool)Physics2D.Raycast(new Vector3(base.AimTransform.position.x, base.AimTransform.position.y + 1f, base.AimTransform.position.z), Vector2.right * base.direction, 10f, Controller.collisionMask))
						{
							MonoBehaviourSingleton<OrangeBattleUtility>.Instance.SetLockWallJump(2000);
							if (base.direction == 1)
							{
								MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxhit_vava_mk2_003", _RightShootPoint.position, Quaternion.Euler(0f, 0f, 180f), Array.Empty<object>());
								Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.CAMERA_SHAKE, 2f, false);
							}
							else
							{
								MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxhit_vava_mk2_003", _RightShootPoint.position, Quaternion.Euler(0f, 0f, 0f), Array.Empty<object>());
								Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.CAMERA_SHAKE, 2f, false);
							}
						}
					}
					break;
				case SubStatus.Phase2:
					if (_currentFrame >= 1f)
					{
						_punchCollideBullet.BackToPool();
						SetStatus(MainStatus.Idle);
					}
					break;
				case SubStatus.Phase1:
					break;
				}
				break;
			case MainStatus.DashAttack:
				switch (_subStatus)
				{
				case SubStatus.Phase0:
					if (_currentFrame >= 1f)
					{
						SetStatus(_mainStatus, SubStatus.Phase1);
					}
					else if (_currentFrame > 0.67f && _bulletCount == 0)
					{
						if (!_efx_DashAttack1.isPlaying)
						{
							_RightShootPoint.localPosition = Vector3.zero;
							_RightShootPoint.localRotation = Quaternion.identity;
							_efx_DashAttack1.Play();
							_efx_DashAttack2.Play();
						}
						_bulletCount++;
						_velocity.x = base.direction * DashSpeed;
						if (_bMultiBoss)
						{
							_punchCollideBullet.Active(targetMask);
						}
						else
						{
							_punchCollideBullet.Active(neutralMask);
						}
					}
					break;
				case SubStatus.Phase1:
					if ((_velocity.x > 0 && Controller.Collisions.right) || (_velocity.x < 0 && Controller.Collisions.left))
					{
						if (Controller.Collisions.left)
						{
							MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxhit_vava_mk2_003", _RightShootPoint.position, Quaternion.Euler(0f, 0f, 180f), Array.Empty<object>());
						}
						else
						{
							MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxhit_vava_mk2_003", _RightShootPoint.position, Quaternion.Euler(0f, 0f, 0f), Array.Empty<object>());
						}
						Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.CAMERA_SHAKE, 2f, false);
						MonoBehaviourSingleton<OrangeBattleUtility>.Instance.SetLockWallJump();
						DestroyAllSpawnedMob();
						PlaySE("BossSE", 124);
						SetStatus(_mainStatus, SubStatus.Phase2);
					}
					break;
				case SubStatus.Phase2:
					if (_currentFrame >= 1f)
					{
						SetStatus(MainStatus.Idle);
					}
					break;
				}
				break;
			case MainStatus.Walk:
				if (AiState != AI_STATE.mob_003)
				{
					UpdateDirection();
				}
				_velocity.x = base.direction * WalkSpeed;
				if ((float)Mathf.Abs(TargetPos.x - Controller.LogicPosition.x) < punchDistance * 1000f)
				{
					SetStatus(MainStatus.Punch);
				}
				break;
			case MainStatus.Charge:
				switch (_subStatus)
				{
				case SubStatus.Phase0:
					if (_currentFrame >= 1f)
					{
						SetStatus(_mainStatus, SubStatus.Phase1);
					}
					break;
				case SubStatus.Phase1:
					if (AiTimer.GetMillisecond() > 1500)
					{
						SetStatus(_mainStatus, SubStatus.Phase2);
					}
					break;
				case SubStatus.Phase2:
					if (_currentFrame >= 1f)
					{
						SetStatus(MainStatus.Idle);
					}
					break;
				}
				break;
			case MainStatus.SemiCharge:
				switch (_subStatus)
				{
				case SubStatus.Phase0:
					if (_currentFrame >= 1f)
					{
						SetStatus(_mainStatus, SubStatus.Phase1);
					}
					break;
				case SubStatus.Phase1:
					if (_currentFrame >= 1f)
					{
						SetStatus(MainStatus.Idle);
					}
					break;
				}
				break;
			case MainStatus.Trap:
				switch (_subStatus)
				{
				case SubStatus.Phase0:
					if (_currentFrame >= 1f)
					{
						SetStatus(_mainStatus, SubStatus.Phase1);
					}
					break;
				case SubStatus.Phase1:
					if (_bulletCount == 3 && _currentFrame >= 1f)
					{
						SetStatus(_mainStatus, SubStatus.Phase2);
					}
					else
					{
						if (EnemyWeapons[4].LastUseTimer.IsStarted() && EnemyWeapons[4].LastUseTimer.GetMillisecond() <= EnemyWeapons[4].BulletData.n_FIRE_SPEED)
						{
							break;
						}
						EnemyWeapons[4].LastUseTimer.TimerStart();
						MOB_TABLE mOB_TABLE = ManagedSingleton<OrangeDataManager>.Instance.MOB_TABLE_DICT[(int)EnemyWeapons[4].BulletData.f_EFFECT_X];
						EnemyControllerBase enemyControllerBase = StageUpdate.StageSpawnEnemyByMob(mOB_TABLE, sNetSerialID + "#" + _bulletCount);
						if ((bool)enemyControllerBase)
						{
							PlaySE("BossSE", 23);
							enemyControllerBase.UpdateEnemyID(mOB_TABLE.n_ID);
							enemyControllerBase.SetPositionAndRotation(_TrapShootPoint.position, base.direction == 1);
							enemyControllerBase.SetActive(true);
							enemyControllerBase.SetVelocity(new VInt3(base.direction * (_bulletCount * TrapShiftX + 4000), TrapShiftY, 0));
							if (AiState == AI_STATE.mob_003)
							{
								(enemyControllerBase as EM011_Controller).SetParentVAVA(this);
							}
							SpawnedMobList.Add(enemyControllerBase);
						}
						_bulletCount++;
					}
					break;
				case SubStatus.Phase2:
					if (_currentFrame >= 1f && AiTimer.GetMillisecond() > 2000)
					{
						SetStatus(MainStatus.DashAttack);
					}
					break;
				}
				break;
			case MainStatus.Dead:
				if (_subStatus == SubStatus.Phase0)
				{
					if (_animator.speed == 0f)
					{
						_animator.speed = 1f;
					}
					if ((double)_currentFrame > 0.4)
					{
						SetStatus(_mainStatus, SubStatus.Phase1);
					}
				}
				break;
			case MainStatus.SceAct:
				switch (_subStatus)
				{
				case SubStatus.Phase0:
					if (_currentFrame >= 1f)
					{
						SetStatus(_mainStatus, SubStatus.Phase1);
					}
					break;
				case SubStatus.Phase1:
					if (AiTimer.GetMillisecond() > 1500)
					{
						PlaySE("BossSE", 128);
						SetStatus(_mainStatus, SubStatus.Phase2);
					}
					break;
				case SubStatus.Phase2:
					if (_currentFrame >= 1f)
					{
						SetStatus(_mainStatus, SubStatus.Phase3);
					}
					break;
				}
				break;
			case MainStatus.Debut:
				break;
			}
			return;
		}
		BaseUpdate();
		MainStatus mainStatus = _mainStatus;
		if (mainStatus == MainStatus.Debut && (double)_currentFrame > 1.0)
		{
			if (IntroCallBack != null)
			{
				IntroCallBack();
			}
			PlaySE("BossSE", 129);
			SetStatus(MainStatus.Idle);
		}
		UpdateGravity();
		Controller.Move((_velocity + _velocityExtra) * GameLogicUpdateManager.m_fFrameLen + _velocityShift);
		distanceDelta = Vector3.Distance(base.transform.localPosition, Controller.LogicPosition.vec3) * (Time.deltaTime / GameLogicUpdateManager.m_fFrameLen);
		_velocityExtra = VInt3.zero;
		_velocityShift = VInt3.zero;
	}

	public void UpdateFunc()
	{
		base.transform.localPosition = Vector3.MoveTowards(base.transform.localPosition, Controller.LogicPosition.vec3, distanceDelta);
		if (!isFalled && Controller.Collisions.below)
		{
			isFalled = true;
			PlaySE("BossSE", 27);
		}
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
			Target = _enemyAutoAimSystem.GetClosetPlayer();
			TargetPos = Target.Controller.LogicPosition;
			if (AiState != AI_STATE.mob_003)
			{
				mainStatus = ((!((float)Mathf.Abs(TargetPos.x - Controller.LogicPosition.x) < punchDistance * 1000f)) ? ((MainStatus)OrangeBattleUtility.Random(5, 9)) : MainStatus.Punch);
			}
			else
			{
				if (_comboIndex >= _combos.Length)
				{
					_comboIndex = 0;
				}
				mainStatus = _combos[_comboIndex];
				_comboIndex++;
			}
			StageObjParam component = GetComponent<StageObjParam>();
			if (component != null && component.nEventID != 0 && nActFrameCount > 300)
			{
				nSaveEventID = component.nEventID;
				component.nEventID = 0;
				mainStatus = MainStatus.SceAct;
				Transform[] modelMesh = ModelMesh;
				for (int i = 0; i < modelMesh.Length; i++)
				{
					modelMesh[i].gameObject.layer = ManagedSingleton<OrangeLayerManager>.Instance.RenderPlayer;
				}
				OrangeBattleUtility.FindChildRecursive(_transform, "fxduring_shield_003", true).parent = HipJoint;
				playStartVoice = true;
			}
		}
		else if (bWaitNetStatus)
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
				bWaitNetStatus = true;
				StageUpdate.RegisterSendAndRun(sNetSerialID, (int)mainStatus, JsonConvert.SerializeObject(netSyncData));
			}
		}
		else
		{
			if (AiState == AI_STATE.mob_003)
			{
				SettingDirection();
			}
			else
			{
				UpdateDirection();
			}
			SetStatus(mainStatus);
		}
	}

	public override void SetActive(bool isActive)
	{
		base.SetActive(isActive);
		_animator.enabled = isActive;
		if (isActive)
		{
			_punchCollideBullet.UpdateBulletData(EnemyWeapons[3].BulletData);
			_punchCollideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			_collideBullet.UpdateBulletData(EnemyWeapons[0].BulletData);
			_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			_collideBullet.Active(targetMask);
		}
		else
		{
			_collideBullet.BackToPool();
			_punchCollideBullet.BackToPool();
		}
	}

	public override void UpdateEnemyID(int id)
	{
		base.UpdateEnemyID(id);
		AI_STATE aiState = AI_STATE.mob_001;
		if (EnemyData.s_AI != "null")
		{
			aiState = (AI_STATE)Enum.Parse(typeof(AI_STATE), EnemyData.s_AI);
		}
		AiState = aiState;
		switch (AiState)
		{
		case AI_STATE.mob_002:
			_bDeadCallResult = false;
			break;
		case AI_STATE.mob_003:
			_bDeadCallResult = false;
			_bMultiBoss = true;
			break;
		default:
			_bDeadCallResult = true;
			break;
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
		ModelTransform.localScale = new Vector3(_transform.localScale.x, _transform.localScale.y, Mathf.Abs(_transform.localScale.z) * (float)base.direction);
		base.transform.position = pos;
	}

	public override void BossIntro(Action cb)
	{
		IntroCallBack = cb;
		StartCoroutine(WaitDebutCoroutine());
	}

	private IEnumerator WaitDebutCoroutine()
	{
		while (!Controller.Collisions.below)
		{
			yield return CoroutineDefine._waitForEndOfFrame;
		}
		SetStatus(MainStatus.Debut);
		Transform[] modelMesh = ModelMesh;
		for (int i = 0; i < modelMesh.Length; i++)
		{
			modelMesh[i].gameObject.layer = ManagedSingleton<OrangeLayerManager>.Instance.RenderEnemy;
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
			if ((bool)_punchCollideBullet)
			{
				_punchCollideBullet.BackToPool();
			}
			DestroyAllSpawnedMob();
			StageUpdate.SlowStage();
			SetColliderEnable(false);
			SetStatus(MainStatus.Dead);
			PlaySE("BossSE", 125);
			PlaySE("BossSE", 127);
		}
	}

	public bool IsDashAttack()
	{
		if (_mainStatus == MainStatus.DashAttack)
		{
			return true;
		}
		return false;
	}

	public Collider2D GetPunchCollider()
	{
		return _punchCollideBullet.GetHitCollider();
	}
}
