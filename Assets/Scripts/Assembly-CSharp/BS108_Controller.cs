#define RELEASE
using System;
using System.Collections;
using System.Collections.Generic;
using CallbackDefs;
using NaughtyAttributes;
using Newtonsoft.Json;
using StageLib;
using UnityEngine;

public class BS108_Controller : EnemyControllerBase, IManagedUpdateBehavior
{
	private enum MainStatus
	{
		Idle = 0,
		Debut = 1,
		Jump = 2,
		Skill0 = 3,
		Skill1 = 4,
		Skill2 = 5,
		Skill3 = 6,
		Skill4 = 7,
		Skill5 = 8,
		Skill6 = 9,
		JumpShoot = 10,
		Dead = 11,
		Hurt = 12,
		IdleWaitNet = 13
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

	private enum SkillPattern
	{
		State1 = 0,
		State2 = 1,
		State3 = 2,
		State4 = 3,
		State5 = 4
	}

	public enum AnimationID
	{
		ANI_DEBUT = 0,
		ANI_DEBUTA = 1,
		ANI_IDLE = 2,
		ANI_JUMP_START = 3,
		ANI_JUMP_LOOP = 4,
		ANI_JUMP_END = 5,
		ANI_SHOOT_BEGIN = 6,
		ANI_SHOOT_LOOP = 7,
		ANI_JUMPSHOOT_BEGIN = 8,
		ANI_JUMPSHOOT_LOOP = 9,
		ANI_SLIDE_BEGIN = 10,
		ANI_SLIDE_LOOP = 11,
		ANI_SLIDE_END = 12,
		ANI_BLIZZARD_BEGIN = 13,
		ANI_BLIZZARD_LOOP = 14,
		ANI_BLIZZARD_END = 15,
		ANI_ICESCULPTURE_START = 16,
		ANI_ICESCULPTURE_LOOP = 17,
		ANI_ICESCULPTURE_END = 18,
		ANI_SKILL0_START = 19,
		ANI_SKILL0_LOOP = 20,
		ANI_SKILL0_END = 21,
		ANI_SKILL1_START1 = 22,
		ANI_SKILL1_LOOP1 = 23,
		ANI_SKILL1_START2 = 24,
		ANI_SKILL1_LOOP2 = 25,
		ANI_SKILL1_END2 = 26,
		ANI_SKILL1_START3 = 27,
		ANI_SKILL1_START4 = 28,
		ANI_SKILL1_LOOP4 = 29,
		ANI_HURT = 30,
		ANI_DEAD = 31,
		MAX_ANIMATION_ID = 32
	}

	private SkillPattern skillpattern;

	[SerializeField]
	[NaughtyAttributes.ReadOnly]
	private MainStatus _mainStatus;

	[SerializeField]
	[NaughtyAttributes.ReadOnly]
	private SubStatus _subStatus;

	[SerializeField]
	[NaughtyAttributes.ReadOnly]
	private AnimationID _currentAnimationId;

	public int MoveSpeed = 8;

	public int DashSpeed = 15;

	public int JumpSpeed = 22;

	private int _bulletCount;

	[SerializeField]
	[NaughtyAttributes.ReadOnly]
	private float _currentFrame;

	private int[] _animationHash;

	private Transform _mouthTransform;

	private Transform _fixedTransform;

	public Transform _HandleTransform;

	public Transform _GrabPointTransform;

	private readonly int _hashVspd = Animator.StringToHash("fVspd");

	private Vector3 StartPos;

	private Vector3 EndPos;

	[SerializeField]
	private Transform ShootPos;

	private Vector3 MaxPos;

	private Vector3 MinPos;

	private int ActionTimes;

	private float ActionAnimatorFrame;

	private int ActionFrame;

	private bool HasActed;

	private float ShotAngle;

	private readonly int _HashAngle = Animator.StringToHash("Angle");

	[SerializeField]
	private int Skill0ShootTimes = 1;

	[SerializeField]
	private float Skill0ShootFrame = 0.4f;

	[SerializeField]
	private int SKill1JumpFrame = 10;

	[SerializeField]
	private int Skill1ShootTimes = 3;

	[SerializeField]
	private int Skill6ShootTimes = 5;

	private int Skill0UseTimes;

	private int Skill1UseTimes;

	private bool Skill1Front;

	private int Gravity = OrangeBattleUtility.FP_Gravity * GameLogicUpdateManager.g_fixFrameLenFP / 1000;

	private int SpawnCount;

	private bool CanReduce;

	private int Skill4ReduceSpeed = 1000;

	[Header("Debug用")]
	[SerializeField]
	private bool DebugMode;

	[SerializeField]
	private MainStatus NextSkill;

	public int AILevel = 2;

	private Action IntoBack;

	private List<EnemyControllerBase> OnStageIceSculpture;

	private bool StageIceSculpture;

	private int[] SouceAI = new int[5] { 2, 5, 6, 7, 8 };

	private int[] AIRaund = new int[6] { 0, 100, 200, 450, 700, 1000 };

	private List<int> NowAiList;

	private int OldCommand;

	private bool IsChipInfoAnim;

	private bool _bDeadCallResult = true;

	private bool _bMultiBoss;

	private bool noBlizzard;

	private float nowDistance;

	[SerializeField]
	public float distance = 1f;

	private float lastPosX;

	private bool PlayBossFlow;

	private float jumpDistance;

	private int _collideBulletId = -1;

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
			case "Skill1":
				NextSkill = MainStatus.Skill1;
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
			case "Skill5":
				NextSkill = MainStatus.Skill5;
				break;
			case "Skill6":
				NextSkill = MainStatus.Skill6;
				break;
			}
		}
	}

	protected override void Start()
	{
		base.Start();
		OrangeBattleUtility.AddEnemyAutoAimSystem(_transform, out _enemyAutoAimSystem);
		_enemyAutoAimSystem.UpdateAimRange(20f);
		Transform[] target = _transform.GetComponentsInChildren<Transform>(true);
		base.AimTransform = OrangeBattleUtility.FindChildRecursive(ref target, "Bip001 Spine");
		_animator = GetComponentInChildren<Animator>();
		ModelTransform = OrangeBattleUtility.FindChildRecursive(ref target, "model", true);
		_mouthTransform = OrangeBattleUtility.FindChildRecursive(ref target, "MouthShootPoint", true);
		ShootPos = OrangeBattleUtility.FindChildRecursive(ref target, "MouthShootPoint", true);
		_fixedTransform = OrangeBattleUtility.FindChildRecursive(ref target, "FixedShootPoint", true);
		if (_HandleTransform == null)
		{
			_HandleTransform = OrangeBattleUtility.FindChildRecursive(ref target, "handle", true);
		}
		if (_GrabPointTransform == null)
		{
			_GrabPointTransform = OrangeBattleUtility.FindChildRecursive(ref target, "GrabPoint", true);
		}
		_collideBullet = base.AimTransform.gameObject.AddOrGetComponent<CollideBullet>();
		_collideBullet.isForceSE = (_collideBullet.isBossBullet = true);
		_animationHash = new int[32];
		_animationHash[0] = Animator.StringToHash("BS031@debut");
		_animationHash[1] = Animator.StringToHash("BS031@debuta");
		_animationHash[2] = Animator.StringToHash("BS031@idle_loop");
		_animationHash[31] = Animator.StringToHash("BS031@dead");
		_animationHash[30] = Animator.StringToHash("BS031@hurt_loop");
		_animationHash[3] = Animator.StringToHash("BS031@jump_start");
		_animationHash[4] = Animator.StringToHash("BS031@jump_loop");
		_animationHash[5] = Animator.StringToHash("BS031@jump_end");
		_animationHash[13] = Animator.StringToHash("BS031@skill_01_start");
		_animationHash[14] = Animator.StringToHash("BS031@skill_01_loop");
		_animationHash[15] = Animator.StringToHash("BS031@skill_01_end");
		_animationHash[6] = Animator.StringToHash("BS031@skill_02_start");
		_animationHash[7] = Animator.StringToHash("BS031@skill_02_loop");
		_animationHash[8] = Animator.StringToHash("BS031@skill_02s_start");
		_animationHash[9] = Animator.StringToHash("BS031@skill_02s_end");
		_animationHash[10] = Animator.StringToHash("BS031@skill_03_start");
		_animationHash[11] = Animator.StringToHash("BS031@skill_03_loop");
		_animationHash[12] = Animator.StringToHash("BS031@skill_03_end");
		_animationHash[16] = Animator.StringToHash("BS031@skill_02_start");
		_animationHash[17] = Animator.StringToHash("BS031@skill_04_loop");
		_animationHash[18] = Animator.StringToHash("BS031@skill_04_end");
		_animationHash[19] = Animator.StringToHash("BS031@skill_06_start");
		_animationHash[20] = Animator.StringToHash("BS031@skill_06_loop");
		_animationHash[21] = Animator.StringToHash("BS031@skill_06_end");
		_animationHash[22] = Animator.StringToHash("BS031@skill_05_jump_start");
		_animationHash[23] = Animator.StringToHash("BS031@skill_05_jump_loop");
		_animationHash[24] = Animator.StringToHash("BS031@skill_05_jump_fall");
		_animationHash[25] = Animator.StringToHash("BS031@skill_05_fall_loop");
		_animationHash[26] = Animator.StringToHash("BS031@jump_end");
		_animationHash[27] = Animator.StringToHash("BS031@skill_05_jump_turn01");
		_animationHash[28] = Animator.StringToHash("BS031@skill_05_jump_turn02");
		_animationHash[29] = Animator.StringToHash("BS031@skill_05_fall_atk");
		base.direction = 1;
		if (IsChipInfoAnim)
		{
			if ((bool)_HandleTransform)
			{
				_HandleTransform.gameObject.SetActive(false);
			}
			SetStatus(MainStatus.Idle);
		}
		else
		{
			SetStatus(MainStatus.Debut);
		}
		SetMaxGravity(OrangeBattleUtility.FP_MaxGravity * 2);
		if (AILevel >= 1)
		{
			OnStageIceSculpture = new List<EnemyControllerBase>();
			NowAiList = new List<int>();
		}
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxstory_explode_000", 10);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("FX_BOSS_EXPLODE2");
		_bDeadPlayCompleted = false;
	}

	public override void SetChipInfoAnim()
	{
		IsChipInfoAnim = true;
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
			_bMultiBoss = true;
			break;
		case AI_STATE.mob_003:
			_bDeadCallResult = false;
			noBlizzard = true;
			_bMultiBoss = true;
			break;
		default:
			_bDeadCallResult = false;
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
			_collideBulletId = -1;
			break;
		case MainStatus.Skill4:
			if (_collideBulletId != 3)
			{
				_collideBulletId = 3;
				_collideBullet.UpdateBulletData(EnemyWeapons[3].BulletData);
				_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
				if (_bMultiBoss)
				{
					_collideBullet.Active(targetMask);
				}
				else
				{
					_collideBullet.Active(neutralMask);
				}
			}
			break;
		default:
			if (_collideBulletId != 0)
			{
				_collideBulletId = 0;
				_collideBullet.UpdateBulletData(EnemyWeapons[0].BulletData);
				_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
				_collideBullet.Active(targetMask);
			}
			break;
		}
		switch (_mainStatus)
		{
		case MainStatus.Idle:
		case MainStatus.IdleWaitNet:
			Target = _enemyAutoAimSystem.GetClosetPlayer();
			if ((bool)Target)
			{
				TargetPos = Target.Controller.LogicPosition;
				UpdateDirection();
			}
			_velocity.x = 0;
			break;
		case MainStatus.Debut:
			switch (_subStatus)
			{
			case SubStatus.Phase1:
				PlaySE("BossSE", 41);
				break;
			}
			break;
		case MainStatus.Jump:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				jumpDistance = _transform.position.x;
				break;
			case SubStatus.Phase1:
				_velocity.x = base.direction * MoveSpeed * 1000;
				_velocity.y = JumpSpeed * 1000;
				PlaySE("BossSE", 40);
				break;
			case SubStatus.Phase2:
				jumpDistance = Mathf.Abs(jumpDistance - _transform.position.x);
				_velocity.x = 0;
				PlaySE("BossSE", 41);
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			break;
		case MainStatus.Skill0:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				ActionTimes = Skill0ShootTimes;
				break;
			case SubStatus.Phase1:
				HasActed = false;
				ActionAnimatorFrame = Skill0ShootFrame;
				break;
			}
			break;
		case MainStatus.Skill1:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				PlayBossSE("BossSE", "bs006_peng01");
				if (Skill1Front)
				{
					if (base.direction == 1)
					{
						int num = (int)(Mathf.Abs(MaxPos.x - _transform.position.x - Controller.Collider2D.size.x) * 1000f * 20f / (float)SKill1JumpFrame);
						_velocity = new VInt3(num * base.direction, SKill1JumpFrame * -Gravity, 0);
					}
					else
					{
						int num2 = (int)(Mathf.Abs(_transform.position.x - MinPos.x - Controller.Collider2D.size.x) * 1000f * 20f / (float)SKill1JumpFrame);
						_velocity = new VInt3(num2 * base.direction, SKill1JumpFrame * -Gravity, 0);
					}
				}
				else
				{
					_velocity.y = -Gravity * SKill1JumpFrame;
				}
				break;
			case SubStatus.Phase1:
				ActionTimes = Skill1ShootTimes;
				break;
			case SubStatus.Phase4:
				PlayBossSE("BossSE", "bs006_peng02");
				Skill1Front = !Skill1Front;
				break;
			case SubStatus.Phase5:
				_velocity.x = 0;
				break;
			case SubStatus.Phase6:
				UpdateDirection(-base.direction);
				break;
			case SubStatus.Phase7:
				HasActed = false;
				break;
			}
			break;
		case MainStatus.Skill6:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				ActionTimes = Skill6ShootTimes;
				break;
			case SubStatus.Phase1:
				HasActed = false;
				ActionAnimatorFrame = Skill0ShootFrame;
				break;
			}
			break;
		case MainStatus.Skill2:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				jumpDistance = _GrabPointTransform.position.x - _transform.position.x;
				UpdateDirection(Math.Sign(jumpDistance));
				break;
			case SubStatus.Phase1:
				_velocity.x = Mathf.RoundToInt(jumpDistance * 1.8f * 1000f);
				_velocity.y = JumpSpeed * 1000;
				PlaySE("BossSE", 40);
				break;
			case SubStatus.Phase2:
				Debug.Log("Aim distance = " + jumpDistance);
				_velocity.x = 0;
				break;
			case SubStatus.Phase3:
			{
				_velocity.x = 0;
				_velocity.y = 0;
				PlaySE("BossSE", 44);
				PlaySE("BossSE", 45);
				if (base.direction == 1)
				{
					PlaySE("BossSE", 47);
				}
				else
				{
					PlaySE("BossSE", 46);
				}
				Vector3 position = MonoBehaviourSingleton<OrangeSceneManager>.Instance.MainCamera.transform.position;
				position.z = 0f;
				position.y -= 4f;
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxduring_PENGUIN_000", position, (base.direction == 1) ? OrangeBattleUtility.QuaternionNormal : OrangeBattleUtility.QuaternionReverse, Array.Empty<object>());
				OrangeBattleUtility.GlobalVelocityExtra = VInt3.right * base.direction * 5;
				Controller.LogicPosition.x = Mathf.RoundToInt((_GrabPointTransform.position.x - 0.05f * (float)base.direction) * 1000f);
				Controller.LogicPosition.y = Mathf.RoundToInt((_GrabPointTransform.position.y - 1.7f) * 1000f);
				break;
			}
			case SubStatus.Phase4:
				OrangeBattleUtility.GlobalVelocityExtra = VInt3.zero;
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			break;
		case MainStatus.Skill3:
			if (_subStatus == SubStatus.Phase0)
			{
				_bulletCount = 0;
			}
			break;
		case MainStatus.Skill5:
			if (_subStatus == SubStatus.Phase0)
			{
				_bulletCount = 0;
				RaycastHit2D raycastHit2D = Physics2D.Raycast(new Vector3(_transform.position.x, _transform.position.y + 1f, 0f), Vector2.right * base.direction, float.PositiveInfinity, 1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockLayer);
				if ((bool)raycastHit2D && raycastHit2D.distance < 1.4f)
				{
					base.direction *= -1;
					UpdateDirection(base.direction);
				}
			}
			break;
		case MainStatus.Skill4:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				PlaySE("BossSE", 51);
				_velocity.x = 0;
				break;
			case SubStatus.Phase1:
				CanReduce = false;
				break;
			case SubStatus.Phase2:
				_velocity.x = 0;
				break;
			}
			break;
		case MainStatus.Dead:
			OrangeBattleUtility.GlobalVelocityExtra = VInt3.zero;
			if (!Controller.Collisions.below)
			{
				IgnoreGravity = true;
			}
			_velocity.x = 0;
			_collideBullet.BackToPool();
			OrangeBattleUtility.LockPlayer();
			break;
		}
		AiTimer.TimerStart();
		UpdateAnimation();
		UpdateCollider();
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
		if (_mainStatus != MainStatus.Skill2 || _subStatus != SubStatus.Phase3)
		{
			IgnoreGravity = false;
		}
		else
		{
			IgnoreGravity = true;
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
				if ((double)_currentFrame > 1.0)
				{
					SetStatus(_mainStatus, SubStatus.Phase3);
				}
				break;
			case SubStatus.Phase3:
				if ((double)_currentFrame > 1.0)
				{
					SetStatus(MainStatus.Idle);
				}
				break;
			case SubStatus.Phase4:
				SetStatus(MainStatus.Idle);
				if (IntoBack != null)
				{
					IntoBack();
				}
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			break;
		case MainStatus.Idle:
			if (Activate)
			{
				if (AILevel >= 1)
				{
					if (!bWaitNetStatus && Controller.Collisions.below)
					{
						UpdateRandomState();
					}
				}
				else if (!bWaitNetStatus && _currentFrame > 1f && Controller.Collisions.below)
				{
					UpdateRandomState();
				}
			}
			else if (_currentFrame > 1f)
			{
				SetStatus(_mainStatus);
			}
			break;
		case MainStatus.Jump:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (_currentFrame > 1f && Controller.Collisions.below)
				{
					SetStatus(_mainStatus, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if (_currentFrame > 1f && Controller.Collisions.below)
				{
					SetStatus(_mainStatus, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				if (_currentFrame > 1f && Controller.Collisions.below)
				{
					SetStatus(MainStatus.Idle);
				}
				break;
			default:
				throw new ArgumentOutOfRangeException();
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
				if (!HasActed && _currentFrame > ActionAnimatorFrame)
				{
					HasActed = true;
					EndPos = GetTargetPos();
					UpdateDirection();
					ShotAngle = Vector2.Angle(Vector2.up, EndPos - ShootPos.position);
					_animator.SetFloat(_HashAngle, ShotAngle);
					Vector3 pDirection2 = EndPos - ShootPos.position;
					BulletBase.TryShotBullet(EnemyWeapons[6].BulletData, ShootPos.position, pDirection2, null, selfBuffManager.sBuffStatus, EnemyData, targetMask).BackCallback = SpawnIcethorn;
				}
				if (_currentFrame > 1f)
				{
					if (--ActionTimes > 0)
					{
						SetStatus(MainStatus.Skill0, _subStatus);
					}
					else
					{
						SetStatus(MainStatus.Skill0, SubStatus.Phase2);
					}
				}
				break;
			case SubStatus.Phase2:
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
					SetStatus(MainStatus.Skill1, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if (_velocity.y < -Gravity * 2)
				{
					if (Skill1Front)
					{
						SetStatus(MainStatus.Skill1, SubStatus.Phase5);
					}
					else
					{
						SetStatus(MainStatus.Skill1, SubStatus.Phase2);
					}
				}
				break;
			case SubStatus.Phase2:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill1, SubStatus.Phase7);
				}
				break;
			case SubStatus.Phase3:
				if (Controller.Collisions.below)
				{
					SetStatus(MainStatus.Skill1, SubStatus.Phase4);
				}
				break;
			case SubStatus.Phase4:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Idle);
				}
				break;
			case SubStatus.Phase5:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill1, SubStatus.Phase6);
				}
				break;
			case SubStatus.Phase6:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill1, SubStatus.Phase7);
				}
				break;
			case SubStatus.Phase7:
				if (!HasActed && _currentFrame > ActionAnimatorFrame)
				{
					HasActed = true;
					_animator.SetFloat(_HashAngle, 90f);
					BulletBase.TryShotBullet(EnemyWeapons[7].BulletData, ShootPos.position, Vector3.right * base.direction, null, selfBuffManager.sBuffStatus, EnemyData, targetMask).BackCallback = SpawnIcethorn;
				}
				if (Controller.Collisions.below)
				{
					SetStatus(MainStatus.Skill1, SubStatus.Phase4);
				}
				else if (_currentFrame > 1f)
				{
					if (--ActionTimes > 0)
					{
						SetStatus(MainStatus.Skill1, _subStatus);
					}
					else
					{
						SetStatus(MainStatus.Skill1, SubStatus.Phase3);
					}
				}
				break;
			}
			break;
		case MainStatus.Skill6:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill6, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if (!HasActed && _currentFrame > ActionAnimatorFrame)
				{
					HasActed = true;
					float num4 = (MaxPos.x - MinPos.x) / (float)(Skill6ShootTimes + 1);
					EndPos = new Vector3(MaxPos.x - (float)ActionTimes * num4, MaxPos.y, 0f);
					TargetPos = new VInt3(EndPos);
					UpdateDirection();
					ShotAngle = Vector2.Angle(Vector2.up, EndPos - ShootPos.position);
					_animator.SetFloat(_HashAngle, ShotAngle);
					Vector3 pDirection = EndPos - ShootPos.position;
					BulletBase.TryShotBullet(EnemyWeapons[2].BulletData, ShootPos.position, pDirection, null, selfBuffManager.sBuffStatus, EnemyData, targetMask).BackCallback = SpawnIcethorn;
				}
				if (_currentFrame > 1f)
				{
					if (--ActionTimes > 0)
					{
						SetStatus(MainStatus.Skill6, _subStatus);
					}
					else
					{
						SetStatus(MainStatus.Skill6, SubStatus.Phase2);
					}
				}
				break;
			case SubStatus.Phase2:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill2);
				}
				break;
			}
			break;
		case MainStatus.Skill2:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (_currentFrame > 1f && Controller.Collisions.below)
				{
					SetStatus(_mainStatus, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
			{
				float num3 = Vector3.Distance(base.AimTransform.position, _GrabPointTransform.position);
				Debug.Log("Distance = " + num3);
				if (num3 < 0.9f)
				{
					SetStatus(_mainStatus, SubStatus.Phase3);
					if (AILevel >= 2)
					{
						if (AiState == AI_STATE.mob_002)
						{
							MonoBehaviourSingleton<OrangeBattleUtility>.Instance.SetLockWallJump(7500);
						}
						else
						{
							MonoBehaviourSingleton<OrangeBattleUtility>.Instance.SetLockWallJump(15000);
						}
					}
				}
				if (_currentFrame > 1f && Controller.Collisions.below)
				{
					SetStatus(_mainStatus, SubStatus.Phase2);
				}
				break;
			}
			case SubStatus.Phase2:
				if (Controller.Collisions.below)
				{
					PlaySE("BossSE", 41);
					SetStatus(MainStatus.Idle);
				}
				break;
			case SubStatus.Phase3:
				if (_currentFrame > 3f)
				{
					SetStatus(_mainStatus, SubStatus.Phase4);
				}
				break;
			case SubStatus.Phase4:
				if (_currentFrame > 1f && Controller.Collisions.below)
				{
					SetStatus(_mainStatus, SubStatus.Phase2);
				}
				break;
			}
			break;
		case MainStatus.Skill4:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				lastPosX = base.transform.localPosition.x;
				if (_currentFrame > 1f)
				{
					SetStatus(_mainStatus, SubStatus.Phase1);
				}
				else if (_currentFrame > 0.73f && _velocity.x == 0)
				{
					_velocity.x = base.direction * DashSpeed * 1000;
				}
				break;
			case SubStatus.Phase1:
			{
				if ((_velocity.x < 0 && Controller.Collisions.left) || (_velocity.x > 0 && Controller.Collisions.right))
				{
					PlaySE("BossSE", 53);
					_velocity.x *= -1;
					base.direction *= -1;
					CanReduce = true;
					UpdateDirection(base.direction);
				}
				float x = base.transform.localPosition.x;
				float num2 = Mathf.Abs(x - lastPosX);
				if (nowDistance + num2 > distance)
				{
					PlaySE("BossSE", 52);
					nowDistance = 0f;
				}
				else
				{
					nowDistance += num2;
				}
				lastPosX = x;
				if (Math.Abs(_velocity.x) <= Skill4ReduceSpeed)
				{
					SetStatus(_mainStatus, SubStatus.Phase2);
				}
				else if (CanReduce)
				{
					_velocity.x -= base.direction * Skill4ReduceSpeed;
				}
				break;
			}
			case SubStatus.Phase2:
				if (_currentFrame > 1f && Controller.Collisions.below)
				{
					SetStatus(MainStatus.Idle);
				}
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			break;
		case MainStatus.Skill3:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				SetStatus(_mainStatus, SubStatus.Phase1);
				break;
			case SubStatus.Phase1:
				if (_currentFrame > 1f)
				{
					SetStatus(_mainStatus, SubStatus.Phase2);
					BulletBase.TryShotBullet(EnemyWeapons[2].BulletData, _fixedTransform.position, base.direction * Vector3.right, null, selfBuffManager.sBuffStatus, EnemyData, targetMask).BackCallback = SpawnIcethorn;
				}
				break;
			case SubStatus.Phase2:
				if (_currentFrame > 1f)
				{
					if (_bulletCount < 3)
					{
						SetStatus(_mainStatus, SubStatus.Phase1);
						UpdateDirection();
						_bulletCount++;
					}
					else
					{
						SetStatus(MainStatus.Idle);
					}
				}
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			break;
		case MainStatus.Skill5:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (_currentFrame > 1f)
				{
					SetStatus(_mainStatus, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
			{
				if (AiTimer.GetMillisecond() > 2000)
				{
					SetStatus(_mainStatus, SubStatus.Phase2);
					break;
				}
				if (!EnemyWeapons[4].LastUseTimer.IsStarted() || EnemyWeapons[4].LastUseTimer.GetMillisecond() > EnemyWeapons[4].BulletData.n_FIRE_SPEED)
				{
					EnemyWeapons[4].LastUseTimer.TimerStart();
					BulletBase.TryShotBullet(EnemyWeapons[4].BulletData, _mouthTransform, base.direction * Vector3.right, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
				}
				if (EnemyWeapons[5].LastUseTimer.IsStarted() && EnemyWeapons[5].LastUseTimer.GetMillisecond() <= EnemyWeapons[5].BulletData.n_FIRE_SPEED)
				{
					break;
				}
				EnemyWeapons[5].LastUseTimer.TimerStart();
				EnemyControllerBase enemyControllerBase = StageUpdate.StageSpawnEnemyByMob(ManagedSingleton<OrangeDataManager>.Instance.MOB_TABLE_DICT[(int)EnemyWeapons[5].BulletData.f_EFFECT_X], sNetSerialID + "0");
				if ((bool)enemyControllerBase)
				{
					PlaySE("BossSE", 49);
					StartCoroutine(FinishIceSculptureSE());
					RaycastHit2D raycastHit2D = Physics2D.Raycast(new Vector3(_transform.position.x, _transform.position.y + 1f, 0f), Vector2.right * base.direction, float.PositiveInfinity, 1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockLayer);
					float num = (float)(1 + _bulletCount) * 1.4f;
					if ((bool)raycastHit2D && raycastHit2D.distance < num + 0.7f)
					{
						num = raycastHit2D.distance - 0.7f;
					}
					enemyControllerBase.UpdateEnemyID(enemyControllerBase.EnemyID);
					enemyControllerBase.SetPositionAndRotation(_transform.position + Vector3.right * base.direction * num, base.direction == -1);
					enemyControllerBase.SetActive(true);
					if (_bMultiBoss)
					{
						EM005_Controller component = enemyControllerBase.gameObject.GetComponent<EM005_Controller>();
						if ((bool)component)
						{
							component.SetParentPenguin2(this);
						}
					}
					if (AILevel >= 1)
					{
						OnStageIceSculpture.Add(enemyControllerBase);
						StageIceSculpture = true;
					}
				}
				_bulletCount++;
				break;
			}
			case SubStatus.Phase2:
				if (_currentFrame > 1f)
				{
					if (AILevel >= 1)
					{
						SetStatus(MainStatus.Idle);
					}
					else
					{
						SetStatus(MainStatus.Skill2);
					}
				}
				break;
			default:
				throw new ArgumentOutOfRangeException();
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
			default:
				throw new ArgumentOutOfRangeException();
			case SubStatus.Phase1:
			case SubStatus.Phase2:
				break;
			}
			break;
		case MainStatus.Dead:
			if (_currentFrame > 0.3f && !PlayBossFlow)
			{
				PlayBossFlow = true;
				ExploderEffect();
			}
			break;
		case MainStatus.JumpShoot:
			break;
		}
	}

	public void UpdateFunc()
	{
		if (Activate || _mainStatus == MainStatus.Debut)
		{
			_animator.SetFloat(_hashVspd, _velocity.vec3.y);
			base.transform.localPosition = Vector3.MoveTowards(base.transform.localPosition, Controller.LogicPosition.vec3, distanceDelta);
		}
	}

	private MainStatus CheckNewAi(MainStatus tmp)
	{
		MainStatus mainStatus = tmp;
		if (StageIceSculpture)
		{
			int num = OrangeBattleUtility.Random(0, 100);
			bool flag = false;
			for (int i = 0; i < OnStageIceSculpture.Count; i++)
			{
				if (OnStageIceSculpture[i].Activate)
				{
					flag = true;
				}
			}
			if (num > 25 && flag)
			{
				mainStatus = MainStatus.Skill2;
				StageIceSculpture = false;
				OnStageIceSculpture.Clear();
			}
			else
			{
				mainStatus = MainStatus.Skill4;
				if (!flag)
				{
					StageIceSculpture = false;
					OnStageIceSculpture.Clear();
				}
			}
		}
		else
		{
			if (mainStatus == MainStatus.Skill3 && OldCommand == 6)
			{
				NowAiList.Clear();
				for (int j = 0; j < SouceAI.Length; j++)
				{
					if (SouceAI[j] != OldCommand)
					{
						NowAiList.Add(SouceAI[j]);
					}
				}
				int[] array = NowAiList.ToArray();
				mainStatus = (MainStatus)array[OrangeBattleUtility.Random(0, array.Length)];
			}
			if (mainStatus == MainStatus.Skill2)
			{
				StageIceSculpture = false;
				OnStageIceSculpture.Clear();
			}
		}
		return mainStatus;
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
			switch (skillpattern)
			{
			case SkillPattern.State1:
				if (OrangeBattleUtility.Random(0, 100) < 25)
				{
					mainStatus = MainStatus.Skill5;
					skillpattern = SkillPattern.State2;
					break;
				}
				skillpattern = SkillPattern.State3;
				goto case SkillPattern.State3;
			case SkillPattern.State2:
				mainStatus = MainStatus.Skill6;
				skillpattern = SkillPattern.State3;
				break;
			case SkillPattern.State3:
				if (OrangeBattleUtility.Random(0, 100) < 50)
				{
					mainStatus = MainStatus.Skill0;
					Skill0UseTimes++;
				}
				else
				{
					mainStatus = MainStatus.Skill1;
					Skill1UseTimes++;
				}
				skillpattern = SkillPattern.State4;
				break;
			case SkillPattern.State4:
				if (OrangeBattleUtility.Random(0, 100) < 50)
				{
					mainStatus = MainStatus.Skill0;
					Skill0UseTimes++;
				}
				else
				{
					mainStatus = MainStatus.Skill1;
					Skill1UseTimes++;
				}
				skillpattern = SkillPattern.State5;
				break;
			case SkillPattern.State5:
				if (Skill0UseTimes > 1 || Skill1UseTimes > 1)
				{
					Skill0UseTimes = (Skill1UseTimes = 0);
					mainStatus = ((OrangeBattleUtility.Random(0, 100) >= 50) ? MainStatus.Skill4 : MainStatus.Skill6);
					skillpattern = SkillPattern.State1;
					break;
				}
				skillpattern = SkillPattern.State3;
				goto case SkillPattern.State3;
			}
			if (DebugMode)
			{
				mainStatus = NextSkill;
			}
			Target = StageUpdate.GetNearestPlayerByVintPos(Controller.LogicPosition, (int)fAIWorkRange);
			if (Target == null)
			{
				return;
			}
			if (Target.UsingVehicle && Target.refRideBaseObj != null)
			{
				TargetPos = Target.refRideBaseObj.Controller.LogicPosition;
			}
			else
			{
				TargetPos = Target.Controller.LogicPosition;
			}
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
			if (noBlizzard && mainStatus == MainStatus.Skill2)
			{
				mainStatus = (MainStatus)OrangeBattleUtility.Random(6, 9);
			}
			StageUpdate.RegisterSendAndRun(sNetSerialID, (int)mainStatus, JsonConvert.SerializeObject(netSyncData));
		}
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

	private void UpdateCollider()
	{
		MainStatus mainStatus = _mainStatus;
		if (mainStatus != 0)
		{
			MainStatus mainStatus2 = mainStatus - 11;
			int num = 2;
		}
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
				_currentAnimationId = AnimationID.ANI_JUMP_LOOP;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_JUMP_END;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_DEBUTA;
				break;
			}
			break;
		case MainStatus.Idle:
		case MainStatus.IdleWaitNet:
			_currentAnimationId = AnimationID.ANI_IDLE;
			break;
		case MainStatus.Jump:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_JUMP_START;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_JUMP_LOOP;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_JUMP_END;
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			break;
		case MainStatus.Skill0:
		case MainStatus.Skill6:
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
				_currentAnimationId = AnimationID.ANI_SKILL1_START1;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_SKILL1_LOOP1;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_SKILL1_START2;
				break;
			case SubStatus.Phase3:
				_currentAnimationId = AnimationID.ANI_SKILL1_LOOP2;
				break;
			case SubStatus.Phase4:
				_currentAnimationId = AnimationID.ANI_SKILL1_END2;
				break;
			case SubStatus.Phase5:
				_currentAnimationId = AnimationID.ANI_SKILL1_START3;
				break;
			case SubStatus.Phase6:
				_currentAnimationId = AnimationID.ANI_SKILL1_START4;
				break;
			case SubStatus.Phase7:
				_currentAnimationId = AnimationID.ANI_SKILL1_LOOP4;
				break;
			}
			break;
		case MainStatus.Skill2:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_JUMP_START;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_JUMP_LOOP;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_JUMP_END;
				break;
			case SubStatus.Phase3:
				_currentAnimationId = AnimationID.ANI_BLIZZARD_BEGIN;
				break;
			case SubStatus.Phase4:
				_currentAnimationId = AnimationID.ANI_BLIZZARD_END;
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			break;
		case MainStatus.Skill4:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_SLIDE_BEGIN;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_SLIDE_LOOP;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_SLIDE_END;
				break;
			default:
				throw new ArgumentOutOfRangeException();
			case SubStatus.Phase3:
			case SubStatus.Phase4:
			case SubStatus.Phase5:
			case SubStatus.Phase6:
			case SubStatus.Phase7:
			case SubStatus.Phase8:
			case SubStatus.Phase9:
			case SubStatus.MAX_SUBSTATUS:
				break;
			}
			break;
		case MainStatus.Skill3:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_IDLE;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_SHOOT_BEGIN;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_SHOOT_LOOP;
				break;
			}
			break;
		case MainStatus.Skill5:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_ICESCULPTURE_START;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_ICESCULPTURE_LOOP;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_ICESCULPTURE_END;
				break;
			}
			break;
		case MainStatus.Dead:
			_currentAnimationId = ((!Controller.Collisions.below) ? AnimationID.ANI_HURT : AnimationID.ANI_DEAD);
			break;
		case MainStatus.Hurt:
			_currentAnimationId = AnimationID.ANI_HURT;
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
		_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
	}

	public override void SetActive(bool isActive)
	{
		base.SetActive(isActive);
		if (isActive)
		{
			IgnoreGlobalVelocity = true;
			_HandleTransform.SetParentNull();
			_HandleTransform.position = new Vector3(974f, 20.1f, 0f);
			_HandleTransform.GetComponent<CharacterMaterial>().Appear();
			Vector3 position = new Vector3((MaxPos.x + MinPos.x) / 2f, MaxPos.y, 0f);
			_HandleTransform.position = position;
			_collideBullet.UpdateBulletData(EnemyWeapons[0].BulletData);
			_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			_collideBullet.Active(targetMask);
		}
		else
		{
			_collideBullet.BackToPool();
		}
	}

	private void CheckRoomSize()
	{
		Vector3 position = _transform.position;
		RaycastHit2D raycastHit2D = Physics2D.Raycast(position, Vector2.left, 100f, Controller.collisionMask);
		RaycastHit2D raycastHit2D2 = Physics2D.Raycast(position, Vector2.right, 100f, Controller.collisionMask);
		RaycastHit2D raycastHit2D3 = Physics2D.Raycast(position, Vector2.up, 100f, Controller.collisionMask);
		RaycastHit2D raycastHit2D4 = Physics2D.Raycast(position, Vector2.down, 100f, Controller.collisionMask);
		if ((bool)raycastHit2D2 && (bool)raycastHit2D && (bool)raycastHit2D3)
		{
			MaxPos = new Vector3(raycastHit2D2.point.x, raycastHit2D3.point.y, 0f);
			MinPos = new Vector3(raycastHit2D.point.x, raycastHit2D4.point.y, 0f);
		}
	}

	public override void BossIntro(Action cb)
	{
		if (_mainStatus == MainStatus.Debut)
		{
			SetStatus(_mainStatus, SubStatus.Phase4);
			IntoBack = cb;
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
		_transform.position = pos;
		CheckRoomSize();
	}

	protected override void DeadBehavior(ref HurtPassParam tHurtPassParam)
	{
		if (_mainStatus != MainStatus.Dead)
		{
			if ((bool)_collideBullet)
			{
				_collideBullet.BackToPool();
			}
			StageUpdate.SlowStage();
			SetStatus(MainStatus.Dead);
		}
	}

	private IEnumerator FinishIceSculptureSE()
	{
		yield return new WaitForSeconds(0.7f);
		PlayBossSE("BossSE", 50);
	}

	public void ExploderEffect()
	{
		if (_bDeadCallResult)
		{
			StartCoroutine(BossDieFlow(base.AimTransform));
		}
		else
		{
			StartCoroutine(BossDieFlow(base.AimTransform, "FX_BOSS_EXPLODE2", false, false));
		}
	}

	public bool IsSliding()
	{
		if (_mainStatus == MainStatus.Skill4)
		{
			return true;
		}
		return false;
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
				TargetPos = new VInt3(Target.GetTargetPoint() + Vector3.up * 0.15f);
			}
			return TargetPos.vec3;
		}
		return _transform.position + Vector3.right * 3f * base.direction;
	}

	private void SpawnIcethorn(object obj)
	{
		BasicBullet basicBullet = null;
		if (obj != null)
		{
			basicBullet = obj as BasicBullet;
		}
		if (basicBullet == null)
		{
			Debug.LogError("子彈資料有誤。");
		}
		else
		{
			if (!basicBullet.isHitBlock)
			{
				return;
			}
			int num = 9;
			Vector3 position = basicBullet._transform.position;
			MOB_TABLE enemy = GetEnemy((int)EnemyWeapons[num].BulletData.f_EFFECT_X);
			if (enemy == null)
			{
				Debug.LogError("要生成的怪物資料有誤，生怪技能ID " + num + " 怪物GroupID " + EnemyWeapons[num].BulletData.f_EFFECT_X);
				return;
			}
			EM195_Controller eM195_Controller = StageUpdate.StageSpawnEnemyByMob(enemy, sNetSerialID + SpawnCount) as EM195_Controller;
			SpawnCount++;
			if (!eM195_Controller)
			{
				return;
			}
			eM195_Controller.SetPositionAndRotation(position, base.direction == 1);
			eM195_Controller.SetActive(true);
			if (basicBullet.GetBulletData.n_ID == EnemyWeapons[2].BulletData.n_ID)
			{
				eM195_Controller.SetWallSide(1);
				return;
			}
			float num2 = 5f;
			int wallSide = 0;
			if (Mathf.Abs(MaxPos.y - basicBullet._transform.position.y) < num2)
			{
				num2 = Mathf.Abs(MaxPos.y - basicBullet._transform.position.y);
				wallSide = 1;
			}
			if (Mathf.Abs(MinPos.y - basicBullet._transform.position.y) < num2)
			{
				num2 = Mathf.Abs(MinPos.y - basicBullet._transform.position.y);
				wallSide = 2;
			}
			if (Mathf.Abs(MinPos.x - basicBullet._transform.position.x) < num2)
			{
				num2 = Mathf.Abs(MinPos.x - basicBullet._transform.position.x);
				wallSide = 3;
			}
			if (Mathf.Abs(MaxPos.x - basicBullet._transform.position.x) < num2)
			{
				num2 = Mathf.Abs(MaxPos.x - basicBullet._transform.position.x);
				wallSide = 4;
			}
			eM195_Controller.SetWallSide(wallSide);
		}
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
}
