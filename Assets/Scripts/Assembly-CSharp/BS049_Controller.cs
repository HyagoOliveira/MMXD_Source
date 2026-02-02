#define RELEASE
using System;
using CallbackDefs;
using NaughtyAttributes;
using Newtonsoft.Json;
using StageLib;
using UnityEngine;

public class BS049_Controller : EnemyControllerBase, IManagedUpdateBehavior
{
	private enum MainStatus
	{
		Idle = 0,
		Debut = 1,
		Skill0 = 2,
		Skill2 = 3,
		Skill4 = 4,
		Skill3 = 5,
		SpecialSkill0 = 6,
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
		MAX_SUBSTATUS = 7
	}

	public enum AnimationID
	{
		ANI_IDLE = 0,
		ANI_DEBUT_LOOP = 1,
		ANI_DEBUT_END = 2,
		ANI_Skill0_START = 3,
		ANI_Skill0_SHOOT = 4,
		ANI_Skill0_END = 5,
		ANI_Skill1_START = 6,
		ANI_Skill1_SHOOT = 7,
		ANI_Skill1_END = 8,
		ANI_Skill2_START = 9,
		ANI_Skill2_RUSH = 10,
		ANI_Skill2_END = 11,
		ANI_Skill2_CATCH = 12,
		ANI_Skill2_HOLD = 13,
		ANI_Skill2_THROW_END = 14,
		ANI_Skill3_START1 = 15,
		ANI_Skill3_LOOP1 = 16,
		ANI_Skill3_START2 = 17,
		ANI_Skill3_LOOP2 = 18,
		ANI_Skill3_END = 19,
		ANI_Skill4_START = 20,
		ANI_Skill4_JUPMLOOP = 21,
		ANI_Skill4_DIVE = 22,
		ANI_Skill4_DIVELOOP = 23,
		ANI_Skill4_LAND = 24,
		ANI_HURT = 25,
		ANI_DEAD = 26,
		MAX_ANIMATION_ID = 27
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

	private Vector3 LastTargetPos = new Vector3(0f, 0f, 0f);

	private Transform HandGun;

	private Transform CatchHand;

	private float MagneticBulletSpeed = 8f;

	private int ShootTimes = 1;

	private int AnglePattern;

	private Vector3 BulletVector;

	private readonly int _HashAngle = Animator.StringToHash("angle");

	private readonly int _HashVelocity_y = Animator.StringToHash("velocity_y");

	private MainStatus _lateAction;

	private bool _isFirstAction = true;

	private float GunAngle;

	private OrangeCharacter targetOC;

	private CollideBullet DashAttack;

	private CollideBullet HeavyPress;

	private CollideBullet _collideBulletWall;

	private int nDeadCount;

	private bool DeadCallResult = true;

	private bool CanSummon;

	private int[] WeightArray1 = new int[3] { 4, 4, 2 };

	private int[] WeightArray2 = new int[2] { 4, 4 };

	private bool hardVer;

	public bool BlackHoleExist;

	private int[] HardWeightArray1 = new int[4] { 4, 4, 2, 4 };

	private int[] HardWeightArray2 = new int[4] { 4, 4, 0, 4 };

	public int DashSpeed = 12000;

	public int BodyBlowForce = 12000;

	private bool _isHitPlayer;

	private bool _isBodyBlowed;

	private bool _isCatching;

	private bool _isBlowedForce;

	private int ResetVelocityFrame;

	private int HeavyPress_X;

	public int HeavyPressJumpForce = 120000;

	private bool _isJumped;

	private bool HaveFury;

	private bool _isShoot;

	[SerializeField]
	private float ChargeTime = 1.5f;

	private int ChargeFrame;

	private Vector3 RoomCenter = Vector3.zero;

	private int GravityType;

	[SerializeField]
	public GameObject[] RenderModes;

	[SerializeField]
	private bool DebugMode;

	[SerializeField]
	private bool CanContinue = true;

	[SerializeField]
	private MainStatus NextSkill;

	[SerializeField]
	private int SetAngle = 2;

	[SerializeField]
	private int SetGravityType;

	private bool bSE05;

	private bool IsChipInfoAnim;

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

	public override void SetChipInfoAnim()
	{
		IsChipInfoAnim = true;
	}

	protected virtual void HashAnimation()
	{
		_animationHash[0] = Animator.StringToHash("BS049@idle_loop");
		_animationHash[1] = Animator.StringToHash("BS049@debut_fall_loop");
		_animationHash[2] = Animator.StringToHash("BS049@debut");
		_animationHash[3] = Animator.StringToHash("BS049@skill_01_start");
		_animationHash[4] = Animator.StringToHash("BS049@skill_01_loop");
		_animationHash[5] = Animator.StringToHash("BS049@skill_01_end");
		_animationHash[6] = Animator.StringToHash("BS049@skill_02_start");
		_animationHash[7] = Animator.StringToHash("BS049@skill_02_loop");
		_animationHash[8] = Animator.StringToHash("BS049@skill_02_end");
		_animationHash[9] = Animator.StringToHash("BS049@skill_03_step1_start");
		_animationHash[10] = Animator.StringToHash("BS049@skill_03_step1_loop");
		_animationHash[11] = Animator.StringToHash("BS049@skill_03_step1_end");
		_animationHash[12] = Animator.StringToHash("BS049@skill_03_step2_start");
		_animationHash[13] = Animator.StringToHash("BS049@skill_03_step2_loop");
		_animationHash[14] = Animator.StringToHash("BS049@skill_03_step2_end");
		_animationHash[15] = Animator.StringToHash("BS049@skill_04_step1_start");
		_animationHash[16] = Animator.StringToHash("BS049@skill_04_step1_loop");
		_animationHash[17] = Animator.StringToHash("BS049@skill_04_step2_start");
		_animationHash[18] = Animator.StringToHash("BS049@skill_04_step2_loop");
		_animationHash[19] = Animator.StringToHash("BS049@skill_04_step2_end");
		_animationHash[20] = Animator.StringToHash("BS049@skill_05_step1_start");
		_animationHash[21] = Animator.StringToHash("BS049@skill_05_step1_loop");
		_animationHash[22] = Animator.StringToHash("BS049@skill_05_step2_start");
		_animationHash[23] = Animator.StringToHash("BS049@skill_05_step2_loop");
		_animationHash[24] = Animator.StringToHash("BS049@skill_05_step2_end");
		_animationHash[25] = Animator.StringToHash("BS049@hurt_loop");
		_animationHash[26] = Animator.StringToHash("BS049@dead");
	}

	protected override void Awake()
	{
		base.Awake();
		Transform[] target = _transform.GetComponentsInChildren<Transform>(true);
		ModelTransform = OrangeBattleUtility.FindChildRecursive(ref target, "model", true);
		_animator = ModelTransform.GetComponent<Animator>();
		HandGun = OrangeBattleUtility.FindChildRecursive(ref target, "L_Hand_jnt", true);
		CatchHand = OrangeBattleUtility.FindChildRecursive(ref target, "CatchPoint", true);
		_collideBullet = OrangeBattleUtility.FindChildRecursive(ref target, "Bip Spine").gameObject.AddOrGetComponent<CollideBullet>();
		DashAttack = OrangeBattleUtility.FindChildRecursive(ref target, "DashCollider").gameObject.AddOrGetComponent<CollideBullet>();
		HeavyPress = OrangeBattleUtility.FindChildRecursive(ref target, "PressCollider").gameObject.AddOrGetComponent<CollideBullet>();
		_collideBulletWall = OrangeBattleUtility.FindChildRecursive(ref target, "WallCollider").gameObject.AddOrGetComponent<CollideBullet>();
		_animationHash = new int[27];
		HashAnimation();
		base.AimPoint = new Vector3(0f, 1.2f, 0f);
		OrangeBattleUtility.AddEnemyAutoAimSystem(_transform, out _enemyAutoAimSystem);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fx_dash_smoke", 3);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxstory_explode_000", 10);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("FX_BOSS_EXPLODE2");
		_enemyAutoAimSystem.UpdateAimRange(20f);
		base.SoundSource.Initial(OrangeSSType.BOSS);
		AiTimer.TimerStart();
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
			switch ((MainStatus)nSet)
			{
			case MainStatus.Skill0:
				AnglePattern = netSyncData.nParam0;
				break;
			case MainStatus.Skill3:
				GravityType = netSyncData.nParam0;
				break;
			}
			UpdateDirection();
		}
		SetStatus((MainStatus)nSet);
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
		ModelTransform.localScale = new Vector3(ModelTransform.localScale.x, ModelTransform.localScale.y, Mathf.Abs(ModelTransform.localScale.z) * (float)(-base.direction));
	}

	private void SetStatus(MainStatus mainStatus, SubStatus subStatus = SubStatus.Phase0)
	{
		if (IsChipInfoAnim)
		{
			_mainStatus = MainStatus.Idle;
			return;
		}
		_mainStatus = mainStatus;
		_subStatus = subStatus;
		if (mainStatus != 0)
		{
			_lateAction = _mainStatus;
		}
		switch (_mainStatus)
		{
		case MainStatus.Debut:
		{
			SubStatus subStatus2 = _subStatus;
			if (subStatus2 == SubStatus.Phase2 && IntroCallBack != null)
			{
				IntroCallBack();
			}
			break;
		}
		case MainStatus.Idle:
			UpdateDirection();
			_velocity.x = 0;
			break;
		case MainStatus.Skill0:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
			{
				if (CanSummon)
				{
					CallEventEnemyPoint(999);
				}
				UpdateDirection();
				int num = 1;
				switch ((AnglePattern != 0) ? AnglePattern : OrangeBattleUtility.Random(1, 5))
				{
				case 1:
					GunAngle = -15f;
					break;
				case 2:
					GunAngle = 0f;
					break;
				case 3:
					GunAngle = 30f;
					break;
				case 4:
					GunAngle = -90f;
					break;
				}
				BulletVector = Quaternion.Euler(0f, 0f, GunAngle * (float)base.direction) * Vector3.right * base.direction;
				_animator.SetFloat(_HashAngle, GunAngle);
				ShootTimes = 1;
				break;
			}
			}
			break;
		case MainStatus.Skill2:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				UpdateDirection();
				DashAttack.HitCallback = Skill2HitCallBack;
				break;
			case SubStatus.Phase1:
				PlaySE("BossSE02", "bs019_mandar11_lp");
				bSE05 = true;
				DashAttack.Active(targetMask);
				_velocity.x = DashSpeed * base.direction;
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fx_dash_smoke", _transform, Quaternion.identity, Array.Empty<object>());
				break;
			case SubStatus.Phase2:
				_velocity.x = 0;
				break;
			case SubStatus.Phase3:
				_velocity.x = 0;
				break;
			case SubStatus.Phase5:
				_velocity.x = 0;
				break;
			}
			break;
		case MainStatus.Skill3:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				IsInvincible = true;
				Target = _enemyAutoAimSystem.GetClosetPlayer();
				if ((bool)Target)
				{
					TargetPos = new VInt3(Target._transform.position);
					UpdateDirection();
				}
				UpdateDirection(-base.direction);
				DashAttack.HitCallback = null;
				break;
			case SubStatus.Phase1:
				PlaySE("BossSE02", "bs019_mandar11_lp");
				bSE05 = true;
				DashAttack.Active(targetMask);
				_velocity.x = DashSpeed * base.direction;
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fx_dash_smoke", _transform, Quaternion.identity, Array.Empty<object>());
				break;
			case SubStatus.Phase2:
				PlaySE("BossSE02", "bs019_mandar11_stop");
				bSE05 = false;
				UpdateDirection();
				break;
			case SubStatus.Phase3:
				ChargeFrame = GameLogicUpdateManager.GameFrame + (int)(ChargeTime * 20f);
				break;
			case SubStatus.Phase5:
				ChargeFrame = GameLogicUpdateManager.GameFrame + (int)(ChargeTime * 20f / 3f);
				break;
			case SubStatus.Phase6:
			{
				Vector3 pDirection = RoomCenter - HandGun.position;
				PlaySE("BossSE02", "bs019_mandar10");
				if (GravityType == 0)
				{
					(BulletBase.TryShotBullet(EnemyWeapons[5].BulletData, HandGun.position, pDirection, null, selfBuffManager.sBuffStatus, EnemyData, targetMask) as BasicBullet).FreeDISTANCE = Vector2.Distance(RoomCenter, HandGun.position);
				}
				else if (GravityType == 1)
				{
					(BulletBase.TryShotBullet(EnemyWeapons[6].BulletData, HandGun.position, pDirection, null, selfBuffManager.sBuffStatus, EnemyData, targetMask) as BasicBullet).FreeDISTANCE = Vector2.Distance(RoomCenter, HandGun.position);
				}
				break;
			}
			}
			break;
		case MainStatus.Skill4:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				HeavyPress_X = (int)((float)(Target.Controller.LogicPosition.x - Controller.LogicPosition.x) * 1.2f);
				HeavyPress.Active(targetMask);
				_collideBullet.BackToPool();
				break;
			case SubStatus.Phase2:
				_enemyCollider[0].SetSize(new Vector2(2f, 0.8f));
				break;
			case SubStatus.Phase4:
				_velocity = VInt3.zero;
				_isJumped = false;
				break;
			}
			break;
		case MainStatus.SpecialSkill0:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (CanSummon)
				{
					CallEventEnemyPoint(999);
				}
				UpdateDirection();
				BulletVector = Quaternion.Euler(0f, 0f, -15 * base.direction) * Vector3.right * base.direction;
				GunAngle = -15f;
				_animator.SetFloat(_HashAngle, GunAngle);
				ShootTimes = 3;
				break;
			case SubStatus.Phase1:
				_isShoot = false;
				break;
			}
			break;
		case MainStatus.Die:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				base.AllowAutoAim = false;
				_velocity = VInt3.zero;
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
				IgnoreGravity = true;
				break;
			case SubStatus.Phase2:
				_velocity = VInt3.zero;
				break;
			case SubStatus.Phase3:
				if (DeadCallResult)
				{
					StartCoroutine(BossDieFlow(GetTargetPoint()));
				}
				else
				{
					StartCoroutine(BossDieFlow(GetTargetPoint(), "FX_BOSS_EXPLODE2", false, false));
				}
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
			default:
				return;
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_DEBUT_LOOP;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_DEBUT_END;
				break;
			}
			break;
		case MainStatus.Idle:
			_currentAnimationId = AnimationID.ANI_IDLE;
			break;
		case MainStatus.Skill0:
			switch (_subStatus)
			{
			default:
				return;
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_Skill0_START;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_Skill0_SHOOT;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_Skill0_END;
				break;
			}
			break;
		case MainStatus.Skill2:
			switch (_subStatus)
			{
			default:
				return;
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_Skill2_START;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_Skill2_RUSH;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_Skill2_END;
				break;
			case SubStatus.Phase3:
				_currentAnimationId = AnimationID.ANI_Skill2_CATCH;
				break;
			case SubStatus.Phase4:
				_currentAnimationId = AnimationID.ANI_Skill2_HOLD;
				break;
			case SubStatus.Phase5:
				_currentAnimationId = AnimationID.ANI_Skill2_THROW_END;
				break;
			case SubStatus.Phase6:
				_currentAnimationId = AnimationID.ANI_IDLE;
				break;
			}
			break;
		case MainStatus.Skill3:
			switch (_subStatus)
			{
			default:
				return;
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_Skill2_START;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_Skill2_RUSH;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_Skill3_START1;
				break;
			case SubStatus.Phase3:
				_currentAnimationId = AnimationID.ANI_Skill3_LOOP1;
				break;
			case SubStatus.Phase4:
				_currentAnimationId = AnimationID.ANI_Skill3_START2;
				break;
			case SubStatus.Phase5:
				_currentAnimationId = AnimationID.ANI_Skill3_LOOP2;
				break;
			case SubStatus.Phase6:
				_currentAnimationId = AnimationID.ANI_Skill3_END;
				break;
			}
			break;
		case MainStatus.Skill4:
			switch (_subStatus)
			{
			default:
				return;
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_Skill4_START;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_Skill4_JUPMLOOP;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_Skill4_DIVE;
				break;
			case SubStatus.Phase3:
				_currentAnimationId = AnimationID.ANI_Skill4_DIVELOOP;
				break;
			case SubStatus.Phase4:
				_currentAnimationId = AnimationID.ANI_Skill4_LAND;
				break;
			}
			break;
		case MainStatus.SpecialSkill0:
			switch (_subStatus)
			{
			default:
				return;
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_Skill1_START;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_Skill1_SHOOT;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_Skill1_END;
				break;
			}
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
		}
		_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
	}

	private void UpdateRandomState(MainStatus nextstatus = MainStatus.Idle)
	{
		MainStatus mainStatus = nextstatus;
		if (hardVer && BlackHoleExist)
		{
			if (GravityType == 0)
			{
				mainStatus = MainStatus.Skill2;
			}
			else if (GravityType == 1)
			{
				mainStatus = MainStatus.Skill4;
			}
		}
		switch (_mainStatus)
		{
		case MainStatus.Debut:
			SetStatus(MainStatus.Idle);
			break;
		case MainStatus.Idle:
			if (mainStatus == MainStatus.Idle)
			{
				if (hardVer)
				{
					mainStatus = (MainStatus)((_lateAction != MainStatus.Skill4) ? WeightRandom(HardWeightArray1, 2) : WeightRandom(HardWeightArray2, 2));
				}
				else if ((int)MaxHp / (int)Hp < 2 || HaveFury)
				{
					mainStatus = (MainStatus)((_lateAction != MainStatus.Skill4) ? WeightRandom(WeightArray1, 2) : WeightRandom(WeightArray2, 2));
				}
				else
				{
					mainStatus = MainStatus.SpecialSkill0;
					HaveFury = true;
				}
				if (_isFirstAction)
				{
					_isFirstAction = false;
					mainStatus = MainStatus.Skill0;
				}
			}
			break;
		}
		if (DebugMode)
		{
			if (CanContinue)
			{
				mainStatus = NextSkill;
				AnglePattern = SetAngle;
			}
			else
			{
				mainStatus = MainStatus.Idle;
			}
		}
		if (mainStatus == MainStatus.Idle || !CheckHost())
		{
			return;
		}
		switch (mainStatus)
		{
		case MainStatus.Skill0:
			AnglePattern = OrangeBattleUtility.Random(1, 5);
			if (DebugMode)
			{
				AnglePattern = SetAngle;
			}
			UploadEnemyStatus((int)mainStatus, false, new object[1] { AnglePattern });
			break;
		case MainStatus.Skill3:
			GravityType = OrangeBattleUtility.Random(0, 20) / 10;
			if (DebugMode)
			{
				GravityType = SetGravityType;
				SetGravityType = (SetGravityType + 1) % 2;
			}
			UploadEnemyStatus((int)mainStatus, false, new object[1] { GravityType });
			break;
		default:
			UploadEnemyStatus((int)mainStatus);
			break;
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
		if ((!Activate && _mainStatus != MainStatus.Debut) || IsChipInfoAnim)
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
				if (_currentFrame > 1f && _introReady)
				{
					SetStatus(MainStatus.Debut, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				if (_introReady)
				{
					if (!bWaitNetStatus)
					{
						UpdateRandomState();
					}
					SetStatus(MainStatus.Idle);
				}
				break;
			}
			break;
		case MainStatus.Idle:
			if (!bWaitNetStatus)
			{
				Target = _enemyAutoAimSystem.GetClosetPlayer();
				if ((bool)Target)
				{
					TargetPos = Target.Controller.LogicPosition;
					UpdateDirection();
					UpdateRandomState();
				}
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
				else
				{
					if (!(_currentFrame > 0.6f) || ShootTimes <= 0)
					{
						break;
					}
					ShootTimes--;
					MOB_TABLE mOB_TABLE2 = ManagedSingleton<OrangeDataManager>.Instance.MOB_TABLE_DICT[(int)EnemyWeapons[3].BulletData.f_EFFECT_X];
					EnemyControllerBase enemyControllerBase2 = StageUpdate.StageSpawnEnemyByMob(mOB_TABLE2, sNetSerialID + "0");
					if ((bool)enemyControllerBase2)
					{
						EM093_Controller eM093_Controller2 = new EM093_Controller();
						eM093_Controller2 = enemyControllerBase2.gameObject.GetComponent<EM093_Controller>();
						if ((bool)eM093_Controller2)
						{
							eM093_Controller2.UpdateEnemyID(mOB_TABLE2.n_ID);
							eM093_Controller2.SetPositionAndRotation(HandGun.position, base.direction == -1);
							if (GunAngle > 90f)
							{
								BulletVector = Vector3.up;
							}
							else if (GunAngle < -90f)
							{
								BulletVector = Vector3.down;
							}
							VInt3 velocity2 = new VInt3(BulletVector.normalized * MagneticBulletSpeed);
							eM093_Controller2.SetVelocity(velocity2);
							eM093_Controller2.SetParent(this);
							eM093_Controller2.SetActive(true);
							PlaySE("BossSE02", "bs019_mandar01");
						}
					}
					SetStatus(MainStatus.Skill0, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase2:
				if (_currentFrame > 1f)
				{
					ShootTimes = 1;
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
				if (_isHitPlayer)
				{
					base.SoundSource.StopAll();
					DashAttack.HitCallback = null;
					DashAttack.BackToPool();
					SetStatus(MainStatus.Skill2, SubStatus.Phase3);
				}
				else if (Controller.Collisions.left || Controller.Collisions.right)
				{
					base.SoundSource.StopAll();
					DashAttack.HitCallback = null;
					if (!_isHitPlayer)
					{
						PlaySE("BossSE02", "bs019_mandar03");
					}
					Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.CAMERA_SHAKE, 2f, false);
					MonoBehaviourSingleton<OrangeBattleUtility>.Instance.SetLockWallJump();
					SetStatus(MainStatus.Skill2, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				if (_currentFrame > 1f)
				{
					_isHitPlayer = false;
					_isCatching = false;
					_isBodyBlowed = false;
					targetOC = null;
					SetStatus(MainStatus.Idle);
				}
				else if (_currentFrame > 0.5f && DashAttack.IsActivate)
				{
					DashAttack.HitCallback = null;
					DashAttack.BackToPool();
				}
				break;
			case SubStatus.Phase3:
				if (_currentFrame > 1f)
				{
					PlaySE("BossSE02", "bs019_mandar04");
					DashAttack.ForceClearList();
					SetStatus(MainStatus.Skill2, SubStatus.Phase4);
				}
				else if (_currentFrame > 0.1f && !_isCatching)
				{
					CatchPlayer();
				}
				break;
			case SubStatus.Phase4:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill2, SubStatus.Phase5);
				}
				break;
			case SubStatus.Phase5:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill2, SubStatus.Phase6);
				}
				if (_currentFrame > 0.03f && !_isBodyBlowed)
				{
					Skill2BodyBlow();
					PlaySE("BossSE02", "bs019_mandar05");
				}
				break;
			case SubStatus.Phase6:
				if (GameLogicUpdateManager.GameFrame > ResetVelocityFrame && (bool)targetOC)
				{
					targetOC.SetStun(false);
					if (targetOC.Controller.HitWallCallbackLeft != null)
					{
						OrangeBattleUtility.GlobalVelocityExtra -= new VInt3(BodyBlowForce * base.direction, 0, 0);
						_isBlowedForce = false;
						targetOC.Controller.HitWallCallbackLeft = (targetOC.Controller.HitWallCallbackRight = null);
					}
					targetOC = null;
					SetStatus(MainStatus.Idle);
				}
				else if (!targetOC)
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
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill3, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if (Controller.Collisions.left || Controller.Collisions.right)
				{
					DashAttack.BackToPool();
					SetStatus(MainStatus.Skill3, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill3, SubStatus.Phase3);
				}
				break;
			case SubStatus.Phase3:
				if (GameLogicUpdateManager.GameFrame >= ChargeFrame)
				{
					SetStatus(MainStatus.Skill3, SubStatus.Phase4);
				}
				break;
			case SubStatus.Phase4:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill3, SubStatus.Phase5);
				}
				break;
			case SubStatus.Phase5:
				if (GameLogicUpdateManager.GameFrame >= ChargeFrame)
				{
					SetStatus(MainStatus.Skill3, SubStatus.Phase6);
				}
				break;
			case SubStatus.Phase6:
				IsInvincible = false;
				if (_currentFrame > 1f)
				{
					if (GravityType == 0)
					{
						UpdateRandomState(MainStatus.Skill2);
					}
					else if (GravityType == 1)
					{
						UpdateRandomState(MainStatus.Skill4);
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
				if (_currentFrame > 0.8f && !_isJumped)
				{
					_velocity = new VInt3(HeavyPress_X, HeavyPressJumpForce, 0);
					_isJumped = true;
					PlaySE("BossSE02", "bs019_mandar06");
				}
				break;
			case SubStatus.Phase1:
				if (_velocity.y < 2000)
				{
					SetStatus(MainStatus.Skill4, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				_animator.SetFloat(_HashVelocity_y, (float)_velocity.y / 1000f);
				if (_velocity.y < -4000)
				{
					SetStatus(MainStatus.Skill4, SubStatus.Phase3);
				}
				break;
			case SubStatus.Phase3:
				if (_currentFrame > 1f || Controller.Collisions.below)
				{
					PlaySE("BossSE02", "bs019_mandar07");
					SetStatus(MainStatus.Skill4, SubStatus.Phase4);
				}
				break;
			case SubStatus.Phase4:
				if (_currentFrame > 1f)
				{
					_collideBullet.Active(targetMask);
					HeavyPress.BackToPool();
					SetStatus(MainStatus.Idle);
					_enemyCollider[0].SetSize(new Vector2(2f, 3f));
				}
				if (_currentFrame > 0.6f)
				{
					_enemyCollider[0].SetSize(new Vector2(2f, 3f * (_currentFrame * 2f - 1f)));
				}
				break;
			}
			break;
		case MainStatus.SpecialSkill0:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.SpecialSkill0, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if (_currentFrame > 1f && ShootTimes < 1)
				{
					SetStatus(MainStatus.SpecialSkill0, SubStatus.Phase2);
				}
				else if (_currentFrame > 1f && ShootTimes > 0)
				{
					SetStatus(MainStatus.SpecialSkill0, SubStatus.Phase1);
				}
				else
				{
					if (!((double)_currentFrame > 0.15) || ShootTimes <= 0 || _isShoot)
					{
						break;
					}
					_isShoot = true;
					ShootTimes--;
					GunAngle = 15 - ShootTimes * 15;
					_animator.SetFloat(_HashAngle, GunAngle);
					MOB_TABLE mOB_TABLE = ManagedSingleton<OrangeDataManager>.Instance.MOB_TABLE_DICT[(int)EnemyWeapons[3].BulletData.f_EFFECT_X];
					EnemyControllerBase enemyControllerBase = StageUpdate.StageSpawnEnemyByMob(mOB_TABLE, sNetSerialID + "0");
					if ((bool)enemyControllerBase)
					{
						EM093_Controller eM093_Controller = new EM093_Controller();
						eM093_Controller = enemyControllerBase.gameObject.GetComponent<EM093_Controller>();
						if ((bool)eM093_Controller)
						{
							eM093_Controller.UpdateEnemyID(mOB_TABLE.n_ID);
							eM093_Controller.SetPositionAndRotation(HandGun.position, base.direction == -1);
							VInt3 velocity = new VInt3(BulletVector.normalized * MagneticBulletSpeed);
							eM093_Controller.SetVelocity(velocity);
							BulletVector = Quaternion.Euler(0f, 0f, 15 * base.direction) * BulletVector;
							eM093_Controller.SetParent(this);
							eM093_Controller.SetActive(true);
							PlaySE("BossSE02", "bs019_mandar01");
						}
					}
				}
				break;
			case SubStatus.Phase2:
				if (_currentFrame > 1f)
				{
					Target = _enemyAutoAimSystem.GetClosetPlayer();
					if ((bool)Target)
					{
						TargetPos = Target.Controller.LogicPosition;
						UpdateDirection();
					}
					SetStatus(MainStatus.Skill2);
				}
				break;
			}
			break;
		case MainStatus.Die:
			switch (_subStatus)
			{
			case SubStatus.Phase1:
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
			case SubStatus.Phase2:
				if (_currentFrame > 0.5f)
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
		}
		if (_isCatching)
		{
			if ((bool)targetOC)
			{
				targetOC._transform.position = Vector3.down * (targetOC.Controller.Collider2D.size.y / 2f) + CatchHand.position;
				targetOC.Controller.LogicPosition = new VInt3(targetOC._transform.position);
			}
			else
			{
				_isCatching = false;
			}
		}
	}

	private void Skill2HitCallBack(object obj)
	{
		if (obj == null || targetOC != null)
		{
			return;
		}
		targetOC = OrangeBattleUtility.GetHitTargetOrangeCharacter(DashAttack.HitTarget);
		if ((bool)OrangeBattleUtility.RaycastIgnoreSelf(Controller.GetCenterPos(), Vector2.right * base.direction, 2f, Controller.collisionMask, _transform))
		{
			UpdateDirection(-base.direction);
		}
		if (targetOC != null && (int)targetOC.Hp > 0 && (int)Hp > 0 && !targetOC.IsStun && !_isCatching)
		{
			if ((bool)targetOC.IsUnBreakX())
			{
				targetOC = null;
				return;
			}
			targetOC.SetStun(true);
			_isHitPlayer = true;
			_isBodyBlowed = false;
		}
	}

	private void CatchPlayer()
	{
		PlayBossSE("BossSE02", "bs019_mandar12");
		MonoBehaviourSingleton<OrangeBattleUtility>.Instance.ChangeRenderLayer(RenderModes, ManagedSingleton<OrangeLayerManager>.Instance.RenderPlayer);
		_isCatching = true;
		_isHitPlayer = false;
	}

	private void Skill2BodyBlow()
	{
		if ((int)targetOC.Hp <= 0)
		{
			_isCatching = false;
			targetOC = null;
		}
		if (targetOC != null && _isCatching)
		{
			if (targetOC.transform.parent != null)
			{
				targetOC._transform.SetParentNull();
			}
			MonoBehaviourSingleton<OrangeBattleUtility>.Instance.ChangeRenderLayer(RenderModes, ManagedSingleton<OrangeLayerManager>.Instance.RenderEnemy);
			OrangeBattleUtility.GlobalVelocityExtra += new VInt3(BodyBlowForce * base.direction, 0, 0);
			_isBlowedForce = true;
			ResetVelocityFrame = GameLogicUpdateManager.GameFrame + 100;
			targetOC.Controller.HitWallCallbackLeft = (targetOC.Controller.HitWallCallbackRight = OnPlayerCollideWall);
			_collideBulletWall.transform.position = targetOC.transform.position;
			_collideBulletWall.Active(targetMask);
		}
		_isBodyBlowed = true;
		_isCatching = false;
	}

	private void OnPlayerCollideWall()
	{
		if ((bool)targetOC)
		{
			targetOC.SetStun(false);
			if (targetOC.Controller.HitWallCallbackLeft != null)
			{
				targetOC.Controller.HitWallCallbackLeft = (targetOC.Controller.HitWallCallbackRight = null);
				OrangeBattleUtility.GlobalVelocityExtra -= new VInt3(BodyBlowForce * base.direction, 0, 0);
				_isBlowedForce = false;
				_collideBulletWall.transform.position = targetOC.transform.position;
				_collideBulletWall.Active(targetMask);
			}
			targetOC = null;
		}
	}

	private void OnPlayerCollideWallCB(object obj)
	{
		_collideBulletWall.BackToPool();
	}

	public void UpdateFunc()
	{
		if ((Activate || _mainStatus == MainStatus.Debut) && !IsChipInfoAnim)
		{
			base.transform.localPosition = Vector3.MoveTowards(base.transform.localPosition, Controller.LogicPosition.vec3, distanceDelta);
		}
	}

	public override void SetActive(bool isActive)
	{
		IgnoreGlobalVelocity = true;
		base.SetActive(isActive);
		if (isActive)
		{
			CheckRoomSize();
			_collideBullet.UpdateBulletData(EnemyWeapons[0].BulletData);
			_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			_collideBullet.Active(targetMask);
			DashAttack.UpdateBulletData(EnemyWeapons[1].BulletData);
			DashAttack.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			HeavyPress.UpdateBulletData(EnemyWeapons[2].BulletData);
			HeavyPress.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			_collideBulletWall.UpdateBulletData(EnemyWeapons[4].BulletData);
			_collideBulletWall.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			_collideBulletWall.HitCallback = OnPlayerCollideWallCB;
			SetStatus(MainStatus.Debut);
		}
		else
		{
			_collideBullet.BackToPool();
			DashAttack.BackToPool();
			HeavyPress.BackToPool();
			_collideBulletWall.BackToPool();
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
		base.transform.localScale = new Vector3(_transform.localScale.x, _transform.localScale.y, _transform.localScale.z * -1f);
		base.transform.position = pos;
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

	private int WeightRandom(int[] SkillArray, int SkillStart)
	{
		int num = 0;
		int num2 = 0;
		int num3 = SkillArray.Length;
		for (int i = 0; i < num3; i++)
		{
			num2 += SkillArray[i];
		}
		int num4 = OrangeBattleUtility.Random(0, num2);
		for (int j = 0; j < num3; j++)
		{
			num += SkillArray[j];
			if (num4 < num)
			{
				return j + SkillStart;
			}
		}
		return 0;
	}

	protected override void DeadBehavior(ref HurtPassParam tHurtPassParam)
	{
		if (_mainStatus == MainStatus.Die)
		{
			return;
		}
		if (_isBlowedForce)
		{
			OrangeBattleUtility.GlobalVelocityExtra -= new VInt3(BodyBlowForce * base.direction, 0, 0);
			_isBlowedForce = false;
		}
		if ((bool)targetOC || _isCatching)
		{
			targetOC.SetStun(false);
			targetOC = null;
			_isCatching = false;
		}
		if ((bool)_collideBullet)
		{
			_collideBullet.BackToPool();
		}
		if ((bool)DashAttack)
		{
			DashAttack.BackToPool();
		}
		if ((bool)HeavyPress)
		{
			HeavyPress.BackToPool();
		}
		if ((bool)_collideBulletWall)
		{
			_collideBulletWall.BackToPool();
		}
		if (BlackHoleExist)
		{
			Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.CLOSE_FX);
		}
		BlackHoleExist = false;
		foreach (StageUpdate.EnemyCtrlID runEnemy in StageUpdate.runEnemys)
		{
			if ((bool)runEnemy.mEnemy && runEnemy.mEnemy is EM093_Controller)
			{
				(runEnemy.mEnemy as EM093_Controller)._needExp = true;
			}
		}
		StageUpdate.SlowStage();
		SetColliderEnable(false);
		SetStatus(MainStatus.Die);
	}

	public override void UpdateEnemyID(int id)
	{
		base.UpdateEnemyID(id);
		UpdateAIState();
		switch (AiState)
		{
		case AI_STATE.mob_002:
			DeadCallResult = false;
			CanSummon = true;
			base.DeadPlayCompleted = false;
			break;
		case AI_STATE.mob_003:
			DeadCallResult = false;
			break;
		case AI_STATE.mob_004:
			hardVer = true;
			DeadCallResult = true;
			base.DeadPlayCompleted = true;
			SetMaxGravity(OrangeBattleUtility.FP_MaxGravity * 2);
			break;
		case AI_STATE.mob_005:
			hardVer = true;
			DeadCallResult = false;
			base.DeadPlayCompleted = true;
			SetMaxGravity(OrangeBattleUtility.FP_MaxGravity * 2);
			break;
		default:
			DeadCallResult = true;
			base.DeadPlayCompleted = true;
			break;
		}
	}

	private void CallEventEnemyPoint(int nID)
	{
		EventManager.StageEventCall stageEventCall = new EventManager.StageEventCall();
		stageEventCall.nID = nID;
		stageEventCall.tTransform = OrangeBattleUtility.CurrentCharacter.transform;
		Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_EVENT_CALL, stageEventCall);
	}

	private void CheckRoomSize()
	{
		RaycastHit2D raycastHit2D = OrangeBattleUtility.RaycastIgnoreSelf(Controller.GetCenterPos(), Vector2.left, 20f, 1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockLayer, _transform);
		RaycastHit2D raycastHit2D2 = OrangeBattleUtility.RaycastIgnoreSelf(Controller.GetCenterPos(), Vector2.right, 20f, 1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockLayer, _transform);
		if ((bool)raycastHit2D && (bool)raycastHit2D2)
		{
			RoomCenter.x = (raycastHit2D2.point.x + raycastHit2D.point.x) / 2f;
		}
		else
		{
			Debug.LogError("BS049 BB 沒有偵測到左邊或右邊的牆壁，請確認\n1.出生位置\n2.關卡地板Layer\n3.想要解決問題嗎，我把BUG都放在這裡，自己尋找原因吧\n4.找 Hank 叫他解決");
		}
		RaycastHit2D raycastHit2D3 = OrangeBattleUtility.RaycastIgnoreSelf(_transform.position, Vector2.down, 20f, 1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockLayer, _transform);
		if ((bool)raycastHit2D3)
		{
			RoomCenter.y = raycastHit2D3.transform.position.y + 7.5f;
		}
		else
		{
			Debug.LogError("BS049 BB 沒有偵測到地板，請確認\n1.出生位置\n2.關卡地板Layer\n3.想要解決問題嗎，我把BUG都放在這裡，自己尋找原因吧\n4.找 Hank 叫他解決");
		}
		RoomCenter.z = 0f;
	}
}
