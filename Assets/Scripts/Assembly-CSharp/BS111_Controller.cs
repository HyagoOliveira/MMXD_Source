using System;
using CallbackDefs;
using NaughtyAttributes;
using Newtonsoft.Json;
using StageLib;
using UnityEngine;

public class BS111_Controller : EnemyControllerBase, IManagedUpdateBehavior
{
	private enum MainStatus
	{
		Idle = 0,
		Run = 1,
		DashJump = 2,
		VerticalBullet = 3,
		HorizontalBullet = 4,
		Skill5 = 5,
		SuperJump = 6,
		Debut = 7,
		Dead = 8,
		Hurt = 9
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
		MAX_SUBSTATUS = 13
	}

	public enum AnimationID
	{
		ANI_IDLE = 0,
		ANI_DEBUT = 1,
		ANI_SHOUT = 2,
		ANI_RUN = 3,
		ANI_BREAK = 4,
		ANI_SKILL0_START = 5,
		ANI_SKILL0_LEFT = 6,
		ANI_SKILL0_RIGHT = 7,
		ANI_SKILL1_START = 8,
		ANI_SKILL1_JUMP = 9,
		ANI_SKILL1_ATTACK = 10,
		ANI_SKILL1_FALL = 11,
		ANI_SKILL1_LAND = 12,
		ANI_SKILL2_START = 13,
		ANI_SKILL2_JUMP = 14,
		ANI_SKILL2_ATTACK = 15,
		ANI_SKILL2_FALL = 16,
		ANI_SKILL2_LAND = 17,
		ANI_SKILL5_START1 = 18,
		ANI_SKILL5_LOOP1 = 19,
		ANI_SKILL5_START2 = 20,
		ANI_SKILL5_LOOP2 = 21,
		ANI_SKILL5_START3 = 22,
		ANI_SKILL5_START4 = 23,
		ANI_SKILL5_START5 = 24,
		ANI_SKILL5_LOOP5 = 25,
		ANI_SKILL5_START6 = 26,
		ANI_SKILL5_END3 = 27,
		ANI_SKILL5_END4 = 28,
		ANI_HURT = 29,
		ANI_DEAD = 30,
		MAX_ANIMATION_ID = 31
	}

	private const bool IsForceSE = true;

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

	public int RunSpeed = 11500;

	public VInt3 DashJumpSpeed = new VInt3(9000, 11500, 0);

	public VInt3 VerticalBulletJumpSpeed = new VInt3(4000, 17500, 0);

	public VInt3 HorizontalBulletJumpSpeed = new VInt3(0, 10000, 0);

	private Transform _shootPoint;

	private readonly int _hashHspd = Animator.StringToHash("fHspd");

	private readonly int _hashVspd = Animator.StringToHash("fVspd");

	private readonly int _hashBelow = Animator.StringToHash("bBelow");

	private readonly int _hashMainStatus = Animator.StringToHash("iMainStatus");

	private readonly int _hashSubStatus = Animator.StringToHash("iSubStatus");

	private int[] _animationHash;

	private int _currentHash;

	private int _JumpTrigger;

	private int _dashJumpTriggerCount;

	private bool IsChipInfoAnim;

	private bool _bDeadCallResult = true;

	private bool _bSummon999;

	private OrangeTimer _summonTimer;

	private int _SummonTime = 20000;

	private ParticleSystem _efx_Run;

	private ParticleSystem _efx_DashJump;

	[Header("總力戰用")]
	private Vector3 StartPos;

	private Vector3 EndPos;

	private int ActionTimes;

	private bool HasActed;

	private float ShotAngle;

	[SerializeField]
	private int RushSpeed = 12000;

	[SerializeField]
	private int Skill5AtkRounds = 3;

	[SerializeField]
	private int Skill5AtkTimes = 4;

	private int Skill5ActionRounds;

	private VInt3 lastVelocity;

	[SerializeField]
	private ParticleSystem Skill5UseFx1;

	[SerializeField]
	private ParticleSystem Skill5UseFx2;

	[Header("Debug用")]
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
				NextSkill = MainStatus.VerticalBullet;
				break;
			case "Skill1":
				NextSkill = MainStatus.HorizontalBullet;
				break;
			case "Skill2":
				NextSkill = MainStatus.Skill5;
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

	protected override void Start()
	{
		base.Start();
		Transform[] target = _transform.GetComponentsInChildren<Transform>(true);
		base.AimTransform = OrangeBattleUtility.FindChildRecursive(ref target, "TriggerPoint_Body");
		_animator = GetComponentInChildren<Animator>();
		ModelTransform = OrangeBattleUtility.FindChildRecursive(ref target, "model", true);
		_collideBullet = OrangeBattleUtility.FindChildRecursive(ref target, "CollideBullet", true).gameObject.AddOrGetComponent<CollideBullet>();
		_shootPoint = OrangeBattleUtility.FindChildRecursive(ref target, "ShootPoint", true);
		_efx_Run = OrangeBattleUtility.FindChildRecursive(ref target, "efx_Hurt", true).GetComponent<ParticleSystem>();
		_efx_DashJump = OrangeBattleUtility.FindChildRecursive(ref target, "efx_DashJump", true).GetComponent<ParticleSystem>();
		Skill5UseFx1 = OrangeBattleUtility.FindChildRecursive(ref target, "Skill5UseFx1", true).GetComponent<ParticleSystem>();
		Skill5UseFx2 = OrangeBattleUtility.FindChildRecursive(ref target, "Skill5UseFx2", true).GetComponent<ParticleSystem>();
		OrangeBattleUtility.AddEnemyAutoAimSystem(_transform, out _enemyAutoAimSystem);
		_animationHash = new int[31];
		_animationHash[1] = Animator.StringToHash("BS032@debut");
		_animationHash[0] = Animator.StringToHash("BS032@idle_loop");
		_animationHash[2] = Animator.StringToHash("BS032@shout");
		_animationHash[3] = Animator.StringToHash("BS032@run_loop");
		_animationHash[4] = Animator.StringToHash("BS032@break");
		_animationHash[30] = Animator.StringToHash("BS032@dead");
		_animationHash[29] = Animator.StringToHash("BS032@hurt_loop");
		_animationHash[5] = Animator.StringToHash("BS032@skill_01_jump_start");
		_animationHash[6] = Animator.StringToHash("BS032@skill_01_jump_left_landing");
		_animationHash[7] = Animator.StringToHash("BS032@skill_01_jump_right_landing");
		_animationHash[8] = Animator.StringToHash("BS032@skill_02_jump_start");
		_animationHash[9] = Animator.StringToHash("BS032@skill_02_jump_loop");
		_animationHash[10] = Animator.StringToHash("BS032@skill_02_jump_atk");
		_animationHash[11] = Animator.StringToHash("BS032@skill_02_fall_loop");
		_animationHash[12] = Animator.StringToHash("BS032@skill_02_landing");
		_animationHash[13] = Animator.StringToHash("BS032@skill_03_jump_start");
		_animationHash[14] = Animator.StringToHash("BS032@skill_03_jump_loop");
		_animationHash[15] = Animator.StringToHash("BS032@skill_03_jump_atk");
		_animationHash[16] = Animator.StringToHash("BS032@skill_03_fall_loop");
		_animationHash[17] = Animator.StringToHash("BS032@skill_03_landing");
		_animationHash[18] = Animator.StringToHash("BS111@OSTRICH_SKILL5_CASTING1");
		_animationHash[19] = Animator.StringToHash("BS111@OSTRICH_SKILL5_CASTLOOP1");
		_animationHash[20] = Animator.StringToHash("BS111@OSTRICH_SKILL5_CASTING2");
		_animationHash[21] = Animator.StringToHash("BS111@OSTRICH_SKILL5_CASTLOOP2");
		_animationHash[22] = Animator.StringToHash("BS111@OSTRICH_SKILL5_CASTING3_1");
		_animationHash[23] = Animator.StringToHash("BS111@OSTRICH_SKILL5_CASTING3_2");
		_animationHash[24] = Animator.StringToHash("BS111@OSTRICH_SKILL5_CASTING4");
		_animationHash[25] = Animator.StringToHash("BS111@OSTRICH_SKILL5_CASTLOOP3");
		_animationHash[26] = Animator.StringToHash("BS111@OSTRICH_SKILL5_CASTING5");
		_animationHash[27] = Animator.StringToHash("BS111@OSTRICH_SKILL5_CASTOUT1_1");
		_animationHash[28] = Animator.StringToHash("BS111@OSTRICH_SKILL5_CASTOUT1_2");
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_headcrush_001", 2);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_ostrich_001", 2);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxstory_explode_000", 10);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("FX_BOSS_EXPLODE2");
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuseTarget", 10);
		if (_summonTimer == null)
		{
			_summonTimer = OrangeTimerManager.GetTimer();
		}
		base.direction = 1;
		if (IsChipInfoAnim)
		{
			SetStatus(MainStatus.Idle);
		}
		else
		{
			SetStatus(MainStatus.Debut);
		}
		_bDeadPlayCompleted = false;
	}

	public override void SetChipInfoAnim()
	{
		IsChipInfoAnim = true;
	}

	public override void UpdateEnemyID(int id)
	{
		base.UpdateEnemyID(id);
		UpdateAIState();
		_enemyAutoAimSystem.UpdateAimRange(50f);
		switch (AiState)
		{
		case AI_STATE.mob_002:
			_bDeadCallResult = false;
			break;
		case AI_STATE.mob_003:
			_bDeadCallResult = false;
			_bSummon999 = true;
			break;
		default:
			_bDeadCallResult = true;
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
		if (sMsg != null && sMsg != "")
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
		{
			SubStatus subStatus2 = _subStatus;
			if (subStatus2 == SubStatus.Phase3 && IntroCallBack != null)
			{
				IntroCallBack();
			}
			break;
		}
		case MainStatus.Dead:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_velocity = VInt3.zero;
				if (!Controller.Collisions.below)
				{
					IgnoreGravity = true;
				}
				if (_efx_Run.isPlaying)
				{
					_efx_Run.Stop();
				}
				if (_efx_DashJump.isPlaying)
				{
					_efx_DashJump.Stop();
				}
				base.AllowAutoAim = false;
				_collideBullet.BackToPool();
				_summonTimer.TimerStop();
				_velocity.x = 0;
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
		case MainStatus.Idle:
			_velocity.x = 0;
			IgnoreGravity = false;
			break;
		case MainStatus.Run:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				IgnoreGravity = false;
				break;
			case SubStatus.Phase1:
				_velocity.x = base.direction * RunSpeed;
				break;
			}
			break;
		case MainStatus.DashJump:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				IgnoreGravity = false;
				_JumpTrigger = 0;
				break;
			case SubStatus.Phase2:
				_JumpTrigger = 0;
				_velocity = new VInt3(DashJumpSpeed.x * base.direction, DashJumpSpeed.y, 0);
				break;
			case SubStatus.Phase3:
				_JumpTrigger = 0;
				_velocity = new VInt3(DashJumpSpeed.x * base.direction, DashJumpSpeed.y, 0);
				break;
			}
			break;
		case MainStatus.VerticalBullet:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				IgnoreGravity = false;
				_JumpTrigger = 0;
				break;
			case SubStatus.Phase1:
				_JumpTrigger = 0;
				break;
			case SubStatus.Phase2:
				IgnoreGravity = true;
				_velocity = VInt3.zero;
				_JumpTrigger = 0;
				break;
			case SubStatus.Phase3:
				_JumpTrigger = 0;
				break;
			case SubStatus.Phase4:
				_velocity.x = 0;
				_JumpTrigger = 0;
				break;
			}
			break;
		case MainStatus.HorizontalBullet:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				IgnoreGravity = false;
				_JumpTrigger = 0;
				break;
			case SubStatus.Phase1:
				_JumpTrigger = 0;
				break;
			case SubStatus.Phase2:
				IgnoreGravity = true;
				_velocity = VInt3.zero;
				_JumpTrigger = 0;
				break;
			case SubStatus.Phase3:
				_JumpTrigger = 0;
				break;
			case SubStatus.Phase4:
				_velocity.x = 0;
				_JumpTrigger = 0;
				break;
			}
			break;
		case MainStatus.Skill5:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				PlayBossSE("BossSE", "bs009_sonic09");
				Skill5ActionRounds = Skill5AtkRounds;
				_velocity = VInt3.zero;
				IgnoreGravity = true;
				break;
			case SubStatus.Phase2:
				ActionTimes = Skill5AtkTimes;
				PlayBossSE("BossSE", "bs009_sonic10");
				RushTarget();
				break;
			case SubStatus.Phase4:
				SwitchSkill5Fx(false);
				ModelTransform.localRotation = Quaternion.Euler(0f, 90f, 0f);
				_velocity = VInt3.zero;
				if (--Skill5ActionRounds <= 0)
				{
					SetStatus(MainStatus.Skill5, SubStatus.Phase9);
					return;
				}
				ActionTimes = Skill5AtkTimes;
				break;
			case SubStatus.Phase5:
				UpdateDirection(-base.direction);
				break;
			case SubStatus.Phase6:
				SwitchSkill5Fx(false);
				ModelTransform.localRotation = Quaternion.Euler(0f, 90f, 0f);
				_velocity = VInt3.zero;
				if (--Skill5ActionRounds <= 0)
				{
					SetStatus(MainStatus.Skill5, SubStatus.Phase11);
					return;
				}
				ActionTimes = Skill5AtkTimes;
				break;
			case SubStatus.Phase7:
				UpdateDirection(-base.direction);
				break;
			case SubStatus.Phase8:
				HasActed = false;
				break;
			case SubStatus.Phase9:
				SwitchSkill5Fx(false);
				ModelTransform.localRotation = Quaternion.Euler(0f, 90f, 0f);
				_velocity = VInt3.zero;
				IgnoreGravity = false;
				break;
			case SubStatus.Phase10:
				UpdateDirection(-base.direction);
				break;
			case SubStatus.Phase11:
				_velocity = VInt3.zero;
				IgnoreGravity = false;
				break;
			}
			break;
		case MainStatus.Hurt:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_velocity = new VInt3(-_velocity.x, IntMath.Abs(_velocity.x) * 2, 0);
				break;
			case SubStatus.Phase1:
				_velocity.x = 0;
				break;
			}
			break;
		}
		AiTimer.TimerStart();
		UpdateAnimation();
		UpdateCollider();
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
		if (_bSummon999 && _summonTimer.GetMillisecond() > _SummonTime)
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
					PlayBossSE("BossSE", 105);
					SetStatus(_mainStatus, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if ((double)_currentFrame > 1.0 && _introReady)
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
				if ((double)_currentFrame > 1.0)
				{
					SetStatus(_mainStatus, SubStatus.Phase4);
				}
				break;
			case SubStatus.Phase4:
				if (_unlockReady)
				{
					SetStatus(MainStatus.Idle);
				}
				break;
			}
			break;
		case MainStatus.Dead:
			if (_subStatus == SubStatus.Phase0 && _currentFrame > 0.21f)
			{
				SetStatus(_mainStatus, SubStatus.Phase1);
			}
			break;
		case MainStatus.Idle:
			if (Controller.Collisions.below && (AiState != AI_STATE.mob_002 || AiTimer.GetMillisecond() >= 1000) && OrangeBattleUtility.ListPlayer.Count > 0)
			{
				UpdateRandomState();
			}
			break;
		case MainStatus.Run:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (_currentFrame >= 1f)
				{
					SetStatus(_mainStatus, SubStatus.Phase1);
					if (!_efx_Run.isPlaying)
					{
						_efx_Run.Play();
					}
				}
				break;
			case SubStatus.Phase1:
				if (Math.Sign(TargetPos.x - Controller.LogicPosition.x) != base.direction)
				{
					SetStatus(_mainStatus, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				if (_efx_Run.isPlaying)
				{
					_efx_Run.Stop();
					PlayBossSE("BossSE", 100);
				}
				_velocity.x = Mathf.RoundToInt((float)(base.direction * RunSpeed) * (1f - _currentFrame));
				if (_currentFrame >= 1f)
				{
					SetStatus(MainStatus.Idle);
				}
				break;
			}
			break;
		case MainStatus.VerticalBullet:
			switch (_subStatus)
			{
			default:
			{
				int num = 13;
				break;
			}
			case SubStatus.Phase0:
				if ((double)_currentFrame >= 0.83 && _JumpTrigger == 0)
				{
					_JumpTrigger = 1;
					_velocity = new VInt3(VerticalBulletJumpSpeed.x * base.direction, VerticalBulletJumpSpeed.y, 0);
					MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_headcrush_001", base.transform.position, Quaternion.identity, Array.Empty<object>());
					PlayBossSE("BossSE", 101);
				}
				if (_currentFrame >= 1f)
				{
					SetStatus(_mainStatus, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if (_velocity.y <= 0)
				{
					SetStatus(_mainStatus, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				if (_currentFrame >= 0.52f && _JumpTrigger == 0)
				{
					_JumpTrigger = 1;
					MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_ostrich_001", base.AimTransform.position, Quaternion.identity, Array.Empty<object>());
					BulletBase.TryShotBullet(EnemyWeapons[4].BulletData, _shootPoint, Vector3.up, null, selfBuffManager.sBuffStatus, EnemyData, targetMask, false, true);
					PlayBossSE("BossSE", 103);
					LeanTween.value(base.gameObject, 0f, 1f, 1f).setOnComplete((Action)delegate
					{
						Vector3 vec = TargetPos.vec3;
						vec.y += 10f;
						vec.x -= 8f;
						for (int i = 0; i < 4; i++)
						{
							RaycastHit2D[] array = Physics2D.RaycastAll(vec, Vector3.down, 200f, LayerMask.GetMask("Block"));
							if (array.Length != 0)
							{
								Vector3 vector = new Vector3(vec.x, vec.y, vec.z);
								Vector3 vector2 = new Vector3(array[0].point.x, array[0].point.y, vec.z);
								float distance = Mathf.Abs(Vector3.Distance(vector, vector2));
								Vector3 vector3 = (vector.xy() - vector2.xy()).normalized;
								MonoBehaviourSingleton<FxManager>.Instance.PlayReturn<psSwingTarget>("fxuseTarget", vector, Quaternion.Euler(0f, 0f, Vector2.SignedAngle(Vector2.right, -vector3)), Array.Empty<object>()).SetEffect(distance, new Color(1f, 0.2f, 0.8f, 0.7f), new Color(1f, 0.2f, 0.8f), 1.5f);
							}
							BulletBase.TryShotBullet(EnemyWeapons[2].BulletData, vec, Vector3.down, null, selfBuffManager.sBuffStatus, EnemyData, targetMask, true);
							vec.x += 4f;
						}
						PlayBossSE("BossSE", "bs009_sonic11");
					});
				}
				else if (_currentFrame >= 0.92f && _JumpTrigger == 1)
				{
					_JumpTrigger = 2;
					IgnoreGravity = false;
					_velocity = new VInt3(VerticalBulletJumpSpeed.x * base.direction, 0, 0);
				}
				if (_currentFrame >= 1f)
				{
					SetStatus(_mainStatus, SubStatus.Phase3);
				}
				break;
			case SubStatus.Phase3:
				if (Controller.Collisions.below)
				{
					PlayBossSE("BossSE", 105);
					SetStatus(_mainStatus, SubStatus.Phase4);
				}
				break;
			case SubStatus.Phase4:
				if (_currentFrame >= 1f)
				{
					SetStatus(MainStatus.Idle);
				}
				break;
			}
			break;
		case MainStatus.DashJump:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if ((double)_currentFrame >= 0.68 && _JumpTrigger == 0)
				{
					_JumpTrigger = 1;
					MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_headcrush_001", base.transform.position, Quaternion.identity, Array.Empty<object>());
					if (!_efx_DashJump.isPlaying)
					{
						_efx_DashJump.Play();
					}
					_velocity = new VInt3(DashJumpSpeed.x * base.direction, DashJumpSpeed.y, 0);
					PlayBossSE("BossSE", 101);
				}
				if (_currentFrame >= 1f)
				{
					SetStatus(_mainStatus, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
			case SubStatus.Phase2:
				if (!Controller.Collisions.below)
				{
					break;
				}
				_JumpTrigger++;
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_headcrush_001", base.transform.position, Quaternion.identity, Array.Empty<object>());
				if (_JumpTrigger > _dashJumpTriggerCount)
				{
					if (Math.Sign(TargetPos.x - Controller.LogicPosition.x) != base.direction)
					{
						SetStatus(_mainStatus, SubStatus.Phase4);
					}
					else
					{
						SetStatus(_mainStatus, SubStatus.Phase3);
					}
				}
				else
				{
					_velocity = VInt3.zero;
				}
				break;
			case SubStatus.Phase3:
				if (!Controller.Collisions.below)
				{
					break;
				}
				_JumpTrigger++;
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_headcrush_001", base.transform.position, Quaternion.identity, Array.Empty<object>());
				if (_JumpTrigger > _dashJumpTriggerCount)
				{
					if (Math.Sign(TargetPos.x - Controller.LogicPosition.x) != base.direction)
					{
						SetStatus(_mainStatus, SubStatus.Phase4);
					}
					else
					{
						SetStatus(_mainStatus, SubStatus.Phase2);
					}
				}
				else
				{
					_velocity = VInt3.zero;
				}
				break;
			case SubStatus.Phase4:
				if (_efx_DashJump.isPlaying)
				{
					_efx_DashJump.Stop();
					PlayBossSE("BossSE", 100);
				}
				_velocity.x = Mathf.RoundToInt((float)(base.direction * RunSpeed) * (1f - _currentFrame));
				if (_currentFrame >= 1f)
				{
					SetStatus(MainStatus.Idle);
				}
				break;
			}
			break;
		case MainStatus.HorizontalBullet:
			switch (_subStatus)
			{
			default:
			{
				int num2 = 13;
				break;
			}
			case SubStatus.Phase0:
				if ((double)_currentFrame >= 0.83 && _JumpTrigger == 0)
				{
					_JumpTrigger = 1;
					MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_headcrush_001", base.transform.position, Quaternion.identity, Array.Empty<object>());
					_velocity = new VInt3(HorizontalBulletJumpSpeed.x * base.direction, HorizontalBulletJumpSpeed.y, 0);
					PlayBossSE("BossSE", 101);
				}
				if (_currentFrame >= 1f)
				{
					SetStatus(_mainStatus, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if (_velocity.y <= 0)
				{
					SetStatus(_mainStatus, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				if (_currentFrame >= 0.62f && _JumpTrigger == 0)
				{
					Vector3 normalized = (TargetPos.vec3 - _shootPoint.position).normalized;
					_JumpTrigger = 1;
					BulletBase.TryShotBullet(EnemyWeapons[3].BulletData, _shootPoint, normalized, null, selfBuffManager.sBuffStatus, EnemyData, targetMask, true);
				}
				else if (_currentFrame >= 0.92f && _JumpTrigger == 1)
				{
					_JumpTrigger = 2;
					IgnoreGravity = false;
					_velocity = new VInt3(HorizontalBulletJumpSpeed.x * base.direction, 0, 0);
				}
				if (_currentFrame >= 1f)
				{
					SetStatus(_mainStatus, SubStatus.Phase3);
				}
				break;
			case SubStatus.Phase3:
				if (Controller.Collisions.below)
				{
					PlayBossSE("BossSE", 105);
					SetStatus(_mainStatus, SubStatus.Phase4);
				}
				break;
			case SubStatus.Phase4:
				if (_currentFrame >= 1f)
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
				if (_currentFrame >= 1f)
				{
					SetStatus(MainStatus.Skill5, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if (_currentFrame >= 1f)
				{
					SetStatus(MainStatus.Skill5, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				if (_currentFrame >= 1f)
				{
					SetStatus(MainStatus.Skill5, SubStatus.Phase3);
				}
				break;
			case SubStatus.Phase3:
				if (Controller.Collisions.left || Controller.Collisions.right)
				{
					if (--ActionTimes > 0)
					{
						PlayBossSE("BossSE", "bs009_sonic10");
						Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.CAMERA_SHAKE, 0.5f, false);
						RushTarget();
						SetStatus(MainStatus.Skill5, SubStatus.Phase3);
					}
					else
					{
						SetStatus(MainStatus.Skill5, SubStatus.Phase6);
					}
				}
				else if (Controller.Collisions.above)
				{
					PlayBossSE("BossSE", "bs009_sonic10");
					Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.CAMERA_SHAKE, 0.5f, false);
					_velocity.y = -lastVelocity.y;
					lastVelocity = _velocity;
					ModelTransform.localRotation = Quaternion.Euler(0f - ShotAngle, 90f, 0f);
				}
				else if (Controller.Collisions.below)
				{
					if (--ActionTimes > 0)
					{
						PlayBossSE("BossSE", "bs009_sonic10");
						RushTarget();
						SetStatus(MainStatus.Skill5, SubStatus.Phase3);
					}
					else
					{
						SetStatus(MainStatus.Skill5, SubStatus.Phase4);
					}
				}
				break;
			case SubStatus.Phase4:
				if (_currentFrame >= 1f)
				{
					SetStatus(MainStatus.Skill5, SubStatus.Phase5);
				}
				break;
			case SubStatus.Phase5:
				if (_currentFrame >= 1f)
				{
					RushTarget();
					SetStatus(MainStatus.Skill5, SubStatus.Phase3);
				}
				break;
			case SubStatus.Phase6:
				if (_currentFrame >= 1f)
				{
					SetStatus(MainStatus.Skill5, SubStatus.Phase7);
				}
				break;
			case SubStatus.Phase7:
				if (_currentFrame >= 1f)
				{
					SetStatus(MainStatus.Skill5, SubStatus.Phase8);
				}
				break;
			case SubStatus.Phase8:
				if (!HasActed && _currentFrame > 0.6f)
				{
					PlayBossSE("BossSE", "bs009_sonic10");
					RushTarget();
					SetStatus(MainStatus.Skill5, SubStatus.Phase3);
				}
				break;
			case SubStatus.Phase9:
				if (_currentFrame >= 1f)
				{
					SetStatus(MainStatus.Skill5, SubStatus.Phase10);
				}
				break;
			case SubStatus.Phase10:
				if (_currentFrame >= 1f)
				{
					SetStatus(MainStatus.Idle);
				}
				break;
			case SubStatus.Phase11:
				if (Controller.Collisions.below)
				{
					PlayBossSE("BossSE", "bs009_sonic07");
					SetStatus(MainStatus.Skill5, SubStatus.Phase12);
				}
				break;
			case SubStatus.Phase12:
				if (_currentFrame >= 1f)
				{
					SetStatus(MainStatus.Idle);
				}
				break;
			}
			break;
		case MainStatus.Hurt:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (Controller.Collisions.below)
				{
					SetStatus(_mainStatus, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
			case SubStatus.Phase2:
				break;
			}
			break;
		case MainStatus.SuperJump:
			break;
		}
	}

	public void UpdateFunc()
	{
		if (Activate || _mainStatus == MainStatus.Debut)
		{
			base.transform.localPosition = Vector3.MoveTowards(base.transform.localPosition, Controller.LogicPosition.vec3, distanceDelta);
		}
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
			nSetKey = (MainStatus)OrangeBattleUtility.Random(3, 6);
			if (DebugMode)
			{
				nSetKey = NextSkill;
			}
			Target = _enemyAutoAimSystem.GetClosetPlayer();
			if (Target == null)
			{
				return;
			}
			TargetPos = Target.Controller.LogicPosition;
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

	private void UpdateCollider()
	{
		switch (_mainStatus)
		{
		case MainStatus.Idle:
		case MainStatus.DashJump:
		case MainStatus.Hurt:
			_collideBullet.UpdateBulletData(EnemyWeapons[0].BulletData);
			break;
		case MainStatus.Run:
		case MainStatus.Skill5:
			_collideBullet.UpdateBulletData(EnemyWeapons[1].BulletData);
			break;
		default:
			return;
		}
		_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
		_collideBullet.Active(targetMask);
	}

	private void UpdateAnimation()
	{
		switch (_mainStatus)
		{
		case MainStatus.Debut:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_SKILL1_FALL;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_SKILL1_LAND;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_DEBUT;
				break;
			case SubStatus.Phase3:
			case SubStatus.Phase4:
				return;
			}
			break;
		case MainStatus.Dead:
			if (_subStatus == SubStatus.Phase0)
			{
				_currentAnimationId = ((!Controller.Collisions.below) ? AnimationID.ANI_HURT : AnimationID.ANI_DEAD);
				break;
			}
			return;
		case MainStatus.Idle:
			_currentAnimationId = AnimationID.ANI_IDLE;
			break;
		case MainStatus.Run:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_SHOUT;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_RUN;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_BREAK;
				break;
			}
			break;
		case MainStatus.DashJump:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_SKILL0_START;
				break;
			case SubStatus.Phase1:
				return;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_SKILL0_LEFT;
				break;
			case SubStatus.Phase3:
				_currentAnimationId = AnimationID.ANI_SKILL0_RIGHT;
				break;
			case SubStatus.Phase4:
				_currentAnimationId = AnimationID.ANI_BREAK;
				break;
			}
			break;
		case MainStatus.VerticalBullet:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_SKILL1_START;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_SKILL1_JUMP;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_SKILL1_ATTACK;
				break;
			case SubStatus.Phase3:
				_currentAnimationId = AnimationID.ANI_SKILL1_FALL;
				break;
			case SubStatus.Phase4:
				_currentAnimationId = AnimationID.ANI_SKILL1_LAND;
				break;
			}
			break;
		case MainStatus.HorizontalBullet:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_SKILL2_START;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_SKILL2_JUMP;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_SKILL2_ATTACK;
				break;
			case SubStatus.Phase3:
				_currentAnimationId = AnimationID.ANI_SKILL2_FALL;
				break;
			case SubStatus.Phase4:
				_currentAnimationId = AnimationID.ANI_SKILL2_LAND;
				break;
			}
			break;
		case MainStatus.Skill5:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_SKILL5_START1;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_SKILL5_LOOP1;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_SKILL5_START2;
				break;
			case SubStatus.Phase3:
				_currentAnimationId = AnimationID.ANI_SKILL5_LOOP2;
				break;
			case SubStatus.Phase4:
				_currentAnimationId = AnimationID.ANI_SKILL5_START3;
				break;
			case SubStatus.Phase5:
				_currentAnimationId = AnimationID.ANI_SKILL5_START4;
				break;
			case SubStatus.Phase6:
				_currentAnimationId = AnimationID.ANI_SKILL5_START5;
				break;
			case SubStatus.Phase7:
				_currentAnimationId = AnimationID.ANI_SKILL5_LOOP5;
				break;
			case SubStatus.Phase8:
				_currentAnimationId = AnimationID.ANI_SKILL5_START6;
				break;
			case SubStatus.Phase9:
				_currentAnimationId = AnimationID.ANI_SKILL5_END3;
				break;
			case SubStatus.Phase10:
				_currentAnimationId = AnimationID.ANI_SKILL5_END4;
				break;
			case SubStatus.Phase11:
				_currentAnimationId = AnimationID.ANI_SKILL2_FALL;
				break;
			case SubStatus.Phase12:
				_currentAnimationId = AnimationID.ANI_SKILL2_LAND;
				break;
			}
			break;
		case MainStatus.Hurt:
			_currentAnimationId = AnimationID.ANI_HURT;
			break;
		}
		_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
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
		IntroCallBack = cb;
		_introReady = true;
	}

	protected override void DeadBehavior(ref HurtPassParam tHurtPassParam)
	{
		_velocity = VInt3.zero;
		if (_mainStatus != MainStatus.Dead)
		{
			if ((bool)_collideBullet)
			{
				_collideBullet.BackToPool();
			}
			SwitchSkill5Fx(false);
			StageUpdate.SlowStage();
			SetStatus(MainStatus.Dead);
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
			_summonTimer.TimerStart();
			_collideBullet.UpdateBulletData(EnemyWeapons[0].BulletData);
			_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			_collideBullet.Active(targetMask);
		}
		else
		{
			_summonTimer.TimerStop();
			_collideBullet.BackToPool();
		}
	}

	private Vector3 GetTargetPos(bool realcenter = false)
	{
		if (!Target)
		{
			Target = _enemyAutoAimSystem.GetClosetPlayer();
		}
		if ((bool)Target)
		{
			if (realcenter)
			{
				TargetPos = new VInt3(Target.Controller.GetRealCenterPos());
			}
			else
			{
				TargetPos = Target.Controller.LogicPosition;
			}
			return TargetPos.vec3;
		}
		return NowPos + Vector3.right * 3f * base.direction;
	}

	private void RushTarget()
	{
		EndPos = GetTargetPos();
		UpdateDirection();
		Vector3 vector = EndPos - Controller.LogicPosition.vec3;
		vector.z = 0f;
		_velocity = new VInt3(vector.normalized * RushSpeed) * 0.001f;
		lastVelocity = _velocity;
		ShotAngle = Vector2.Angle(Vector2.right * base.direction, vector);
		if (_velocity.y > 0)
		{
			ShotAngle = 0f - ShotAngle;
		}
		ShotAngle *= base.direction;
		SwitchSkill5Fx(true);
		ModelTransform.localRotation = Quaternion.Euler(ShotAngle, 90f, 0f);
	}

	private void SwitchSkill5Fx(bool onoff)
	{
		if (onoff)
		{
			if ((bool)Skill5UseFx1)
			{
				Skill5UseFx1.Play();
			}
			if ((bool)Skill5UseFx2)
			{
				Skill5UseFx2.Play();
			}
			return;
		}
		if ((bool)Skill5UseFx1)
		{
			Skill5UseFx1.Clear();
			Skill5UseFx1.Stop();
		}
		if ((bool)Skill5UseFx2)
		{
			Skill5UseFx2.Clear();
			Skill5UseFx2.Stop();
		}
	}
}
