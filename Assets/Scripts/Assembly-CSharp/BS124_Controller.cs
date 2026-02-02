#define RELEASE
using System;
using System.Collections.Generic;
using CallbackDefs;
using Newtonsoft.Json;
using StageLib;
using UnityEngine;

public class BS124_Controller : EnemyControllerBase, IManagedUpdateBehavior
{
	private enum MainStatus
	{
		Idle = 0,
		Debut = 1,
		Dead = 2,
		Walk = 3,
		Jump = 4,
		JumpToWall = 5,
		GroundTongue = 6,
		Spike = 7,
		WallIdle = 8,
		WallTongue = 9,
		WallMoveTop = 10,
		WallTailBeam = 11,
		WallToFall = 12,
		MoveToPos = 13
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
		MAX_SUBSTATUS = 8
	}

	public enum AnimationID
	{
		ANI_IDLE = 0,
		ANI_DEBUT = 1,
		ANI_DEAD = 2,
		ANI_WALK = 3,
		ANI_SHOOT = 4,
		ANI_HURT = 5,
		ANI_GROUND_TONGUE_START = 6,
		ANI_GROUND_TONGUE_END = 7,
		ANI_WALL_WALK = 8,
		ANI_WALL_TONGUE_START = 9,
		ANI_WALL_TONGUE_END = 10,
		ANI_WALL_TAILBEAM_START = 11,
		ANI_WALL_TAILBEAM_LOOP = 12,
		ANI_WALL_TAILBEAM_END = 13,
		ANI_JUMP_START = 14,
		ANI_JUMP_LOOP = 15,
		ANI_LAND = 16,
		ANI_WALLIDLE_START = 17,
		ANI_SPIKE_JUMP_START = 18,
		ANI_SPIKE_JUMP_LOOP = 19,
		ANI_SPIKE_START = 20,
		ANI_SPIKE_LOOP = 21,
		ANI_SPIKE_END = 22,
		MAX_ANIMATION_ID = 23
	}

	private readonly int _hashVspd = Animator.StringToHash("fVspd");

	private CollideBullet _tongueCollideBullet;

	private Transform _tailShootPoint;

	[SerializeField]
	private MainStatus _mainStatus;

	[SerializeField]
	private SubStatus _subStatus;

	private AnimationID _currentAnimationId;

	private readonly MainStatus[] _groundStatus = new MainStatus[4]
	{
		MainStatus.Jump,
		MainStatus.JumpToWall,
		MainStatus.JumpToWall,
		MainStatus.Spike
	};

	private readonly MainStatus[] _wallStatus = new MainStatus[3]
	{
		MainStatus.WallTongue,
		MainStatus.WallMoveTop,
		MainStatus.WallToFall
	};

	private float _currentFrame;

	private int[] _animationHash;

	private bool _isStealth;

	private bool _isWall;

	private Vector3 _targetPos;

	public VInt WalkSpeed = 1500;

	public VInt MoveSpeed = 8000;

	public VInt DashSpeed = 15000;

	public VInt JumpSpeed = 22000;

	public float groundTongueDistance = 5f;

	private Vector3 leftTop;

	private Vector3 rightTop;

	private Vector3 leftMid;

	private Vector3 rightMid;

	private bool IsChipInfoAnim;

	private bs030_effect mhide_effect;

	private bool _bDeadCallResult = true;

	private MainStatus lastStatus;

	private int nActionTimes;

	private int nActionFrame;

	private int nAIActionTimes;

	private bool bHasCopy = true;

	private float fMoveDis;

	private Vector3 CenterPos = Vector3.zero;

	private Vector3 StartPos;

	private Vector3 EndPos;

	private Vector3 EndPosMob003;

	public bool bHasOver;

	private List<BS124_Controller> CopyList = new List<BS124_Controller>();

	private Vector3 WallTongueOffset = Vector3.up;

	private int SpawnCount;

	[Header("Debug用")]
	[SerializeField]
	private bool DebugMode;

	[SerializeField]
	private MainStatus NextSkill;

	[SerializeField]
	private ParticleSystem CopyFX;

	private bool _triggerFlag;

	private int _shootCount;

	private bool freshSetStatus;

	private int WallTongueCount;

	private Vector3 NowPos
	{
		get
		{
			return _transform.position;
		}
	}

	private bool bCopyOver
	{
		get
		{
			for (int i = 0; i < StageUpdate.runEnemys.Count; i++)
			{
				BS124_Controller component = StageUpdate.runEnemys[i].mEnemy.GetComponent<BS124_Controller>();
				if ((bool)component && component.AiState != 0 && !component.bHasOver)
				{
					return false;
				}
			}
			return true;
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
				NextSkill = MainStatus.WallTongue;
				break;
			case "Skill1":
				NextSkill = MainStatus.WallMoveTop;
				break;
			case "Skill2":
				NextSkill = MainStatus.Spike;
				break;
			case "Skill3":
				NextSkill = MainStatus.Jump;
				break;
			case "Skill4":
				NextSkill = MainStatus.JumpToWall;
				break;
			case "Skill5":
				NextSkill = MainStatus.WallToFall;
				break;
			}
		}
	}

	protected override void Awake()
	{
		base.Awake();
		Transform[] target = _transform.GetComponentsInChildren<Transform>(true);
		ModelTransform = OrangeBattleUtility.FindChildRecursive(ref target, "model", true);
		_tailShootPoint = OrangeBattleUtility.FindChildRecursive(ref target, "TailShootPoint", true);
		mhide_effect = OrangeBattleUtility.FindChildRecursive(ref target, "efx_bs030", true).GetComponent<bs030_effect>();
		_animator = GetComponentInChildren<Animator>();
		_collideBullet = base.gameObject.AddOrGetComponent<CollideBullet>();
		_collideBullet.isForceSE = (_collideBullet.isBossBullet = true);
		_tongueCollideBullet = OrangeBattleUtility.FindChildRecursive(ref target, "TongueCollider", true).gameObject.AddOrGetComponent<CollideBullet>();
		_tongueCollideBullet.isForceSE = (_tongueCollideBullet.isBossBullet = true);
		base.AimTransform = OrangeBattleUtility.FindChildRecursive(ref target, "Bip Spine1", true);
		if (CopyFX == null)
		{
			CopyFX = OrangeBattleUtility.FindChildRecursive(ref target, "CopyFX", true).GetComponent<ParticleSystem>();
		}
		_animationHash = new int[23];
		_animationHash[0] = Animator.StringToHash("BS030@idle_loop");
		_animationHash[1] = Animator.StringToHash("BS030@debut");
		_animationHash[2] = Animator.StringToHash("BS030@dead");
		_animationHash[3] = Animator.StringToHash("BS030@run_loop");
		_animationHash[4] = Animator.StringToHash("BS030@skill_01");
		_animationHash[5] = Animator.StringToHash("BS030@hurt_loop");
		_animationHash[17] = Animator.StringToHash("BS030@skill4_grab_move_start");
		_animationHash[14] = Animator.StringToHash("BS030@jump_start");
		_animationHash[15] = Animator.StringToHash("BS030@jump_loop");
		_animationHash[16] = Animator.StringToHash("BS030@landing");
		_animationHash[8] = Animator.StringToHash("BS030@skill4_grab_move_loop");
		_animationHash[11] = Animator.StringToHash("BS030@skill3_grab_atk_start");
		_animationHash[12] = Animator.StringToHash("BS030@skill3_grab_atk_end");
		_animationHash[13] = Animator.StringToHash("BS030@skill3_grab_atk_to_grab");
		_animationHash[6] = Animator.StringToHash("BS030@skill5_start");
		_animationHash[7] = Animator.StringToHash("BS030@skill5_end");
		_animationHash[9] = Animator.StringToHash("BS030@skill2_grab_atk_start");
		_animationHash[10] = Animator.StringToHash("BS030@skill2_grab_atk_end");
		_animationHash[18] = Animator.StringToHash("BS030@skill1_jump_start");
		_animationHash[19] = Animator.StringToHash("BS030@skill1_jump_loop");
		_animationHash[20] = Animator.StringToHash("BS030 @skill1_hang_atk_start");
		_animationHash[21] = Animator.StringToHash("BS030@skill1_hang_atk_loop");
		_animationHash[22] = Animator.StringToHash("BS030@skill1_hang_atk_end");
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxstory_explode_000", 10);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("FX_BOSS_EXPLODE2");
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_headcrush_001", 2);
		OrangeBattleUtility.AddEnemyAutoAimSystem(_transform, out _enemyAutoAimSystem);
		_enemyAutoAimSystem.UpdateAimRange(20f);
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
		SetStatus(MainStatus.Idle);
		UpdateAnimation();
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
		IgnoreGravity = _isWall;
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
			if (_subStatus == SubStatus.Phase0 && (double)_currentFrame > 0.4)
			{
				SetStatus(_mainStatus, SubStatus.Phase1);
			}
			break;
		case MainStatus.Idle:
			if (_currentFrame >= 1f)
			{
				switch (AiState)
				{
				case AI_STATE.mob_002:
					BackToPool();
					break;
				case AI_STATE.mob_003:
					BackToPool();
					break;
				case AI_STATE.mob_004:
					BackToPool();
					break;
				default:
					UpdateRandomState();
					break;
				}
			}
			break;
		case MainStatus.Jump:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (_currentFrame >= 1f)
				{
					SetStatus(_mainStatus, SubStatus.Phase1);
				}
				else if (!_triggerFlag && _currentFrame >= 0.72f)
				{
					_triggerFlag = true;
					_velocity.x = base.direction * MoveSpeed.i;
					_velocity.y = JumpSpeed.i;
					if (!_isStealth)
					{
						MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_headcrush_001", base.transform.position, Quaternion.identity, Array.Empty<object>());
					}
				}
				break;
			case SubStatus.Phase1:
				if (Controller.Collisions.below)
				{
					SetStatus(_mainStatus, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				if (!(_currentFrame >= 1f))
				{
					break;
				}
				switch (AiState)
				{
				case AI_STATE.mob_002:
					if (--nActionTimes > 0)
					{
						SetStatus(_mainStatus);
					}
					else
					{
						SetStatus(MainStatus.Idle);
					}
					break;
				case AI_STATE.mob_003:
					BackToPool();
					break;
				case AI_STATE.mob_004:
					BackToPool();
					break;
				default:
					SetStatus(MainStatus.Idle);
					break;
				}
				break;
			}
			break;
		case MainStatus.JumpToWall:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (_currentFrame >= 1f)
				{
					SetStatus(_mainStatus, SubStatus.Phase1);
				}
				else if (!_triggerFlag && _currentFrame >= 0.72f)
				{
					_triggerFlag = true;
					_velocity.x = base.direction * MoveSpeed;
					_velocity.y = JumpSpeed.i;
					if (!_isStealth)
					{
						MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_headcrush_001", base.transform.position, Quaternion.identity, Array.Empty<object>());
					}
				}
				break;
			case SubStatus.Phase1:
				if (_velocity.y <= 0)
				{
					PlayBossSE("BossSE", 73);
					SetStatus(MainStatus.WallIdle);
				}
				break;
			}
			break;
		case MainStatus.WallToFall:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (Controller.Collisions.below)
				{
					SetStatus(_mainStatus, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if (_currentFrame >= 1f)
				{
					AI_STATE aiState = AiState;
					if (aiState == AI_STATE.mob_002)
					{
						SetStatus(MainStatus.Jump);
					}
					else
					{
						SetStatus(MainStatus.Idle);
					}
				}
				break;
			}
			break;
		case MainStatus.WallIdle:
			if (!(_currentFrame >= 1f))
			{
				break;
			}
			switch (AiState)
			{
			case AI_STATE.mob_002:
			{
				if (nActionTimes <= 0)
				{
					BackToPool();
					break;
				}
				bHasOver = false;
				int num = OrangeBattleUtility.Random(0, 150);
				if (num < 50)
				{
					SetStatus(MainStatus.WallTongue);
				}
				else if (num < 100)
				{
					SetStatus(MainStatus.WallMoveTop);
				}
				else
				{
					SetStatus(MainStatus.WallToFall);
				}
				break;
			}
			case AI_STATE.mob_003:
				if (nActionTimes <= 0)
				{
					BackToPool();
				}
				else
				{
					bHasOver = true;
				}
				break;
			case AI_STATE.mob_004:
				if (nActionTimes <= 0)
				{
					BackToPool();
				}
				else
				{
					bHasOver = true;
				}
				break;
			default:
				UpdateRandomState();
				break;
			}
			break;
		case MainStatus.GroundTongue:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (_currentFrame >= 1f)
				{
					SetStatus(_mainStatus, SubStatus.Phase1);
				}
				else if (!_triggerFlag && _currentFrame >= 0.77f)
				{
					_tongueCollideBullet._transform.localRotation = Quaternion.identity;
					_tongueCollideBullet._transform.localPosition = Vector3.zero;
					_tongueCollideBullet.Active(targetMask);
					_triggerFlag = true;
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
		case MainStatus.Spike:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (_currentFrame >= 1f)
				{
					SetStatus(_mainStatus, SubStatus.Phase1);
				}
				else if (!_triggerFlag && _currentFrame >= 0.72f)
				{
					_triggerFlag = true;
					_velocity.x = 0;
					_velocity.y = JumpSpeed.i;
					if (!_isStealth)
					{
						MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_headcrush_001", base.transform.position, Quaternion.identity, Array.Empty<object>());
					}
				}
				break;
			case SubStatus.Phase1:
				if (_velocity.y <= 0)
				{
					SetStatus(_mainStatus, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				if (_currentFrame >= 1f)
				{
					PlayBossSE("BossSE", 75);
					SetStatus(_mainStatus, (!bHasCopy) ? SubStatus.Phase3 : SubStatus.Phase5);
				}
				break;
			case SubStatus.Phase3:
				if (_currentFrame >= 5f)
				{
					if (!bHasCopy)
					{
						if (OrangeBattleUtility.Random(0, 150) < 50)
						{
							SetStealth(false);
							bHasCopy = !bHasCopy;
							CopyList.Clear();
							SetStatus(_mainStatus, SubStatus.Phase5);
						}
						else
						{
							SetStatus(_mainStatus, SubStatus.Phase4);
						}
					}
					else if (bHasCopy && bCopyOver)
					{
						SetStatus(_mainStatus, SubStatus.Phase4);
					}
				}
				if (!EnemyWeapons[3].LastUseTimer.IsStarted() || EnemyWeapons[3].LastUseTimer.GetMillisecond() > EnemyWeapons[3].BulletData.n_FIRE_SPEED)
				{
					EnemyWeapons[3].LastUseTimer.TimerStart();
					BulletBase.TryShotBullet(EnemyWeapons[3].BulletData, new Vector2(OrangeBattleUtility.Random(leftTop.x, rightTop.x), leftTop.y + 1f), Vector3.down, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
				}
				break;
			case SubStatus.Phase4:
				if (_currentFrame >= 1f)
				{
					CopyBack();
					SetStatus(MainStatus.Jump, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase5:
				if (bHasCopy && bCopyOver)
				{
					SetStealth(false);
					SetStatus(_mainStatus, SubStatus.Phase3);
				}
				break;
			}
			break;
		case MainStatus.WallTongue:
			if (AiState == AI_STATE.mob_001 && bHasCopy && nActionTimes > 0 && GameLogicUpdateManager.GameFrame > nActionFrame)
			{
				nActionTimes--;
				if (CopyList.Count >= nActionTimes + 1)
				{
					CopyList[nActionTimes].SetCopyStatus((int)_mainStatus);
				}
				nActionFrame += 20;
			}
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if ((_velocity.x > 0 && Controller.Collisions.right) || (_velocity.x < 0 && Controller.Collisions.left) || (_velocity.y > 0 && Controller.Collisions.above) || Vector2.Distance(_targetPos, _transform.position) < 1f)
				{
					SetStatus(_mainStatus, SubStatus.Phase1);
				}
				else
				{
					_velocity = new VInt3((_targetPos - _transform.position).normalized * (float)MoveSpeed);
				}
				break;
			case SubStatus.Phase1:
				if (_currentFrame >= 1f)
				{
					SetStatus(_mainStatus, SubStatus.Phase2);
				}
				else if (!_triggerFlag && _currentFrame >= 0.56f)
				{
					_tongueCollideBullet._transform.localRotation = Quaternion.identity;
					_tongueCollideBullet._transform.localPosition = Vector3.zero;
					_tongueCollideBullet.Active(targetMask);
					_triggerFlag = true;
				}
				break;
			case SubStatus.Phase2:
				if (!(_currentFrame >= 1f))
				{
					break;
				}
				if (!_isStealth)
				{
					SetStealth(true);
				}
				switch (AiState)
				{
				case AI_STATE.mob_002:
					if (--nActionTimes > 0)
					{
						SetStatus(_mainStatus);
					}
					else
					{
						SetStatus(MainStatus.WallIdle);
					}
					break;
				case AI_STATE.mob_003:
					if (--nActionTimes > 0)
					{
						SetStatus(MainStatus.WallIdle);
					}
					else
					{
						BackToPool();
					}
					break;
				case AI_STATE.mob_004:
					BackToPool();
					break;
				default:
					if (bHasCopy && bCopyOver && --nAIActionTimes > 0)
					{
						SetStatus(_mainStatus);
					}
					if (!bHasCopy || (bHasCopy && bCopyOver && nActionTimes <= 0))
					{
						CopyBack();
						SetStatus(MainStatus.WallIdle);
					}
					break;
				}
				break;
			case SubStatus.Phase5:
				if (bHasCopy && bCopyOver)
				{
					SetStatus(_mainStatus);
				}
				break;
			case SubStatus.Phase3:
			case SubStatus.Phase4:
				break;
			}
			break;
		case MainStatus.WallTailBeam:
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
					SetStatus(_mainStatus, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				if (!(_currentFrame >= 1f))
				{
					break;
				}
				_shootCount--;
				if (_shootCount > 0)
				{
					SetStatus(_mainStatus, SubStatus.Phase1);
					break;
				}
				switch (AiState)
				{
				case AI_STATE.mob_002:
					if (--nActionTimes > 0)
					{
						_shootCount = 3;
						SetStatus(_mainStatus);
					}
					else
					{
						BackToPool();
					}
					break;
				case AI_STATE.mob_003:
					BackToPool();
					break;
				case AI_STATE.mob_004:
					if (nActionTimes > 0)
					{
						SetStatus(MainStatus.WallIdle);
					}
					else
					{
						BackToPool();
					}
					break;
				default:
					if (!bHasCopy)
					{
						CopyBack();
						SetStatus(MainStatus.WallIdle);
					}
					else
					{
						SetStatus(_mainStatus, SubStatus.Phase5);
					}
					break;
				}
				break;
			case SubStatus.Phase5:
				if (bHasCopy && bCopyOver)
				{
					if (--nActionTimes > 0)
					{
						SetStatus(MainStatus.WallMoveTop);
					}
					else
					{
						SetStatus(MainStatus.WallIdle);
					}
				}
				break;
			case SubStatus.Phase3:
			case SubStatus.Phase4:
				break;
			}
			break;
		case MainStatus.WallMoveTop:
			switch (_subStatus)
			{
			case SubStatus.Phase1:
				switch (AiState)
				{
				case AI_STATE.mob_001:
					if (!bHasCopy)
					{
						_shootCount = 3;
						if ((_velocity.x > 0 && Controller.Collisions.right) || (_velocity.x < 0 && Controller.Collisions.left) || Vector2.Distance(NowPos, StartPos) >= fMoveDis)
						{
							SetStatus(_mainStatus, SubStatus.Phase2);
						}
						break;
					}
					switch (nActionTimes)
					{
					case 4:
						if (_velocity.x < 0 && Controller.Collisions.left && Vector2.Distance(NowPos, StartPos) >= fMoveDis)
						{
							_shootCount = 0;
							SetStatus(_mainStatus, SubStatus.Phase2);
						}
						break;
					case 3:
						_shootCount = 2;
						SetStatus(MainStatus.WallTailBeam);
						break;
					case 2:
						_shootCount = 0;
						SetStatus(_mainStatus, SubStatus.Phase2);
						break;
					case 1:
						_shootCount = 2;
						SetStatus(MainStatus.WallTailBeam);
						break;
					}
					break;
				case AI_STATE.mob_002:
					if ((_velocity.x > 0 && Controller.Collisions.right) || (_velocity.x < 0 && Controller.Collisions.left) || Vector2.Distance(NowPos, StartPos) >= fMoveDis)
					{
						SetStatus(_mainStatus, SubStatus.Phase2);
					}
					break;
				case AI_STATE.mob_004:
					switch (nActionTimes)
					{
					case 4:
						if (_velocity.x > 0 && Controller.Collisions.right && Vector2.Distance(NowPos, StartPos) >= fMoveDis)
						{
							_shootCount = 0;
							SetStatus(_mainStatus, SubStatus.Phase2);
						}
						break;
					case 3:
						nActionTimes--;
						_shootCount = 2;
						break;
					case 2:
						if (Vector2.Distance(NowPos, StartPos) >= fMoveDis)
						{
							_shootCount = 0;
							_transform.position = EndPos;
							Controller.LogicPosition = new VInt3(NowPos);
							SetStatus(_mainStatus, SubStatus.Phase2);
						}
						break;
					case 1:
						_shootCount = 2;
						SetStatus(_mainStatus, SubStatus.Phase2);
						break;
					}
					break;
				case AI_STATE.mob_003:
					break;
				}
				break;
			case SubStatus.Phase2:
				switch (AiState)
				{
				case AI_STATE.mob_002:
					SetStatus(MainStatus.WallTailBeam);
					break;
				case AI_STATE.mob_004:
					bHasOver = true;
					break;
				default:
					if (!bHasCopy || bCopyOver)
					{
						if (_shootCount > 0)
						{
							SetStatus(MainStatus.WallTailBeam);
						}
						else
						{
							SetStatus(MainStatus.WallTailBeam, SubStatus.Phase5);
						}
					}
					break;
				}
				break;
			case SubStatus.Phase5:
				if (bHasCopy && bCopyOver)
				{
					SetStatus(_mainStatus);
				}
				break;
			case SubStatus.Phase0:
			case SubStatus.Phase3:
			case SubStatus.Phase4:
				break;
			}
			break;
		case MainStatus.MoveToPos:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				SetStatus(_mainStatus, SubStatus.Phase1);
				break;
			case SubStatus.Phase1:
				if (Vector2.Distance(NowPos, StartPos) >= fMoveDis)
				{
					_velocity = VInt3.zero;
					SetStatus(MainStatus.WallIdle);
				}
				break;
			}
			break;
		case MainStatus.Walk:
			break;
		}
	}

	public void UpdateFunc()
	{
		if (!Activate && _mainStatus != MainStatus.Debut)
		{
			return;
		}
		_currentFrame = _animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
		switch (_mainStatus)
		{
		case MainStatus.WallMoveTop:
			switch (_subStatus)
			{
			}
			break;
		case MainStatus.WallTailBeam:
			switch (_subStatus)
			{
			case SubStatus.Phase1:
				if (!freshSetStatus && !_triggerFlag && _currentFrame >= 0.52f)
				{
					_triggerFlag = true;
					Vector3 normalized = (TargetPos.vec3 - _tailShootPoint.position).normalized;
					UpdateDirection();
					BulletBase.TryShotBullet(EnemyWeapons[2].BulletData, _tailShootPoint, normalized, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
				}
				break;
			default:
				throw new ArgumentOutOfRangeException();
			case SubStatus.Phase0:
			case SubStatus.Phase2:
			case SubStatus.Phase3:
			case SubStatus.Phase5:
			case SubStatus.MAX_SUBSTATUS:
				break;
			}
			break;
		}
		base.transform.localPosition = Vector3.MoveTowards(base.transform.localPosition, Controller.LogicPosition.vec3, distanceDelta);
		_animator.SetFloat(_hashVspd, (float)_velocity.y * 0.001f);
		freshSetStatus = false;
	}

	public override void SetActive(bool isActive)
	{
		base.SetActive(isActive);
		_animator.enabled = isActive;
		if (isActive)
		{
			_collideBullet.UpdateBulletData(EnemyWeapons[0].BulletData);
			_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			_collideBullet.Active(targetMask);
			_tongueCollideBullet.UpdateBulletData(EnemyWeapons[1].BulletData);
			_tongueCollideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			CopyFX.Clear();
			CopyFX.Stop();
			switch (AiState)
			{
			case AI_STATE.mob_001:
				SetStatus(MainStatus.Debut);
				break;
			case AI_STATE.mob_002:
			case AI_STATE.mob_003:
				nActionTimes = 2;
				bHasOver = false;
				SetColliderEnable(false);
				SetStatus(MainStatus.MoveToPos);
				_characterMaterial.ChangeDissolveTime(0f);
				break;
			case AI_STATE.mob_004:
				nActionTimes = 4;
				bHasOver = false;
				SetColliderEnable(false);
				SetStatus(MainStatus.MoveToPos);
				_characterMaterial.ChangeDissolveTime(0f);
				break;
			}
		}
		else
		{
			_collideBullet.BackToPool();
		}
	}

	public override void UpdateEnemyID(int id)
	{
		base.UpdateEnemyID(id);
		base.UpdateEnemyID(id);
		UpdateAIState();
		AI_STATE aiState = AiState;
		AI_STATE aI_STATE = aiState - 1;
		int num = 2;
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
		UpdateDirection((!bBack) ? 1 : (-1));
		Vector3 position = _transform.position;
		RaycastHit2D raycastHit2D = Physics2D.Raycast(position, Vector2.left, 100f, Controller.collisionMask);
		RaycastHit2D raycastHit2D2 = Physics2D.Raycast(position, Vector2.right, 100f, Controller.collisionMask);
		RaycastHit2D raycastHit2D3 = Physics2D.Raycast(position, Vector2.up, 100f, Controller.collisionMask);
		if ((bool)raycastHit2D2 && (bool)raycastHit2D && (bool)raycastHit2D3)
		{
			leftTop = new Vector3(position.x - raycastHit2D.distance + 0.5f, position.y + raycastHit2D3.distance - 1.5f, 0f);
			rightTop = new Vector3(position.x + raycastHit2D2.distance - 0.5f, position.y + raycastHit2D3.distance - 1.5f, 0f);
			leftMid = new Vector3(position.x - raycastHit2D.distance + 0.5f, position.y + raycastHit2D3.distance - 5f, 0f);
			rightMid = new Vector3(position.x + raycastHit2D2.distance - 0.5f, position.y + raycastHit2D3.distance - 5f, 0f);
			CenterPos = (leftMid + rightMid) / 2f;
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
		freshSetStatus = true;
		switch (_mainStatus)
		{
		case MainStatus.Debut:
		{
			SubStatus subStatus2 = _subStatus;
			if (subStatus2 == SubStatus.Phase4 && IntroCallBack != null)
			{
				IntroCallBack();
			}
			break;
		}
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
			_isWall = false;
			_velocity.x = 0;
			break;
		case MainStatus.Walk:
			_isWall = false;
			break;
		case MainStatus.Jump:
			_isWall = false;
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_triggerFlag = false;
				GetTargetPos();
				UpdateDirection();
				break;
			case SubStatus.Phase2:
				_velocity.x = 0;
				break;
			}
			break;
		case MainStatus.JumpToWall:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_triggerFlag = false;
				break;
			}
			break;
		case MainStatus.WallToFall:
			_isWall = false;
			SetStealth(false);
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_triggerFlag = false;
				break;
			case SubStatus.Phase1:
				_velocity.x = 0;
				break;
			}
			break;
		case MainStatus.WallIdle:
			_velocity = VInt3.zero;
			_isWall = true;
			SetStealth(true);
			switch (AiState)
			{
			case AI_STATE.mob_002:
				bHasOver = true;
				break;
			case AI_STATE.mob_003:
			case AI_STATE.mob_004:
				bHasOver = false;
				break;
			}
			break;
		case MainStatus.GroundTongue:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_triggerFlag = false;
				break;
			case SubStatus.Phase1:
				_tongueCollideBullet.BackToPool();
				break;
			}
			break;
		case MainStatus.Spike:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_triggerFlag = false;
				break;
			case SubStatus.Phase2:
				SetStealth(true);
				_isWall = true;
				_velocity = VInt3.zero;
				break;
			case SubStatus.Phase4:
				SetStealth(false);
				break;
			case SubStatus.Phase5:
			{
				CopyList.Clear();
				base.SoundSource.PlaySE("BossSE", "bs008_kame11");
				BS124_Controller bS124_Controller6 = SpawnEnemy(NowPos);
				CopyList.Add(bS124_Controller6);
				bS124_Controller6.SetActive(true);
				break;
			}
			}
			break;
		case MainStatus.WallTongue:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				SetStealth(true);
				if (AiState == AI_STATE.mob_001)
				{
					if (bHasCopy && CopyList.Count < 1)
					{
						SetStatus(_mainStatus, SubStatus.Phase5);
						return;
					}
					if (bHasCopy)
					{
						nActionTimes = 2;
						nActionFrame = GameLogicUpdateManager.GameFrame + 20;
					}
				}
				_triggerFlag = false;
				_targetPos = GetTargetPos();
				_targetPos = TargetPos.vec3;
				_targetPos += WallTongueOffset;
				break;
			case SubStatus.Phase1:
				UpdateDirection();
				_velocity = VInt3.zero;
				SetStealth(false);
				break;
			case SubStatus.Phase2:
				_tongueCollideBullet.BackToPool();
				break;
			case SubStatus.Phase5:
			{
				nAIActionTimes = 2;
				CopyList.Clear();
				base.SoundSource.PlaySE("BossSE", "bs008_kame11");
				BS124_Controller bS124_Controller4 = SpawnEnemy(_transform.position, 6);
				CopyList.Add(bS124_Controller4);
				bS124_Controller4.SetMob003EndPos((leftMid + CenterPos) / 2f);
				bS124_Controller4.SetActive(true);
				BS124_Controller bS124_Controller5 = SpawnEnemy(_transform.position, 6);
				CopyList.Add(bS124_Controller5);
				bS124_Controller5.SetMob003EndPos((rightMid + CenterPos) / 2f);
				bS124_Controller5.SetActive(true);
				break;
			}
			}
			break;
		case MainStatus.WallMoveTop:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
			{
				TargetPos = new VInt3(GetTargetPos());
				if (AiState == AI_STATE.mob_001)
				{
					if (bHasCopy && CopyList.Count < 1)
					{
						SetStatus(_mainStatus, SubStatus.Phase5);
						return;
					}
					if (bHasCopy && CopyList.Count > 0)
					{
						for (int j = 0; j < CopyList.Count; j++)
						{
							BS124_Controller bS124_Controller3 = CopyList[j];
							if ((bool)bS124_Controller3 && bS124_Controller3.AiState == AI_STATE.mob_004)
							{
								bS124_Controller3.SetCopyStatus((int)_mainStatus);
							}
						}
					}
				}
				EndPos = leftTop;
				AI_STATE aiState = AiState;
				if (aiState == AI_STATE.mob_004)
				{
					if (nActionTimes > 3)
					{
						EndPos = rightMid;
					}
					else if (nActionTimes > 2)
					{
						EndPos = leftMid;
					}
					else if (nActionTimes > 1)
					{
						EndPos = CenterPos;
					}
					else
					{
						EndPos = NowPos;
					}
				}
				else if (bHasCopy && AiState == AI_STATE.mob_001)
				{
					if (nActionTimes > 3)
					{
						EndPos = leftTop;
					}
					else if (nActionTimes > 2)
					{
						EndPos = rightTop;
					}
					else if (nActionTimes > 1)
					{
						EndPos = rightTop;
					}
					else
					{
						EndPos = leftTop;
					}
				}
				else if (TargetPos.vec3.x > _transform.position.x)
				{
					EndPos = rightTop;
				}
				_velocity = new VInt3((EndPos - _transform.position).normalized * (float)MoveSpeed);
				_velocity.z = 0;
				StartPos = NowPos;
				fMoveDis = Vector2.Distance(StartPos, EndPos) - 1f;
				SetStatus(_mainStatus, SubStatus.Phase1);
				break;
			}
			case SubStatus.Phase2:
				_velocity = VInt3.zero;
				switch (AiState)
				{
				case AI_STATE.mob_002:
					_shootCount = 3;
					SetStatus(MainStatus.WallTailBeam);
					break;
				case AI_STATE.mob_004:
					nActionTimes--;
					bHasOver = true;
					break;
				default:
					if (!bHasCopy || bCopyOver)
					{
						if (_shootCount > 0)
						{
							SetStatus(MainStatus.WallTailBeam);
						}
						else
						{
							SetStatus(MainStatus.WallTailBeam, SubStatus.Phase5);
						}
					}
					break;
				}
				break;
			case SubStatus.Phase5:
			{
				CopyList.Clear();
				base.SoundSource.PlaySE("BossSE", "bs008_kame11");
				BS124_Controller bS124_Controller2 = SpawnEnemy(_transform.position, 7);
				CopyList.Add(bS124_Controller2);
				bS124_Controller2.SetActive(true);
				break;
			}
			default:
				throw new ArgumentOutOfRangeException();
			case SubStatus.Phase1:
			case SubStatus.Phase3:
			case SubStatus.Phase4:
			case SubStatus.MAX_SUBSTATUS:
				break;
			}
			break;
		case MainStatus.WallTailBeam:
			SetStealth(false);
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				SetStealth(false);
				if (bHasCopy && CopyList.Count > 0)
				{
					for (int i = 0; i < CopyList.Count; i++)
					{
						BS124_Controller bS124_Controller = CopyList[i];
						if ((bool)bS124_Controller && bS124_Controller.AiState == AI_STATE.mob_004)
						{
							bS124_Controller.SetCopyStatus((int)_mainStatus);
						}
					}
				}
				if (NowPos.x > CenterPos.x)
				{
					UpdateDirection(-1);
				}
				else
				{
					UpdateDirection(1);
				}
				_triggerFlag = false;
				_targetPos = GetTargetPos(true);
				break;
			case SubStatus.Phase1:
				_tongueCollideBullet.BackToPool();
				_triggerFlag = false;
				break;
			case SubStatus.Phase5:
				SetStealth(true);
				break;
			}
			break;
		case MainStatus.MoveToPos:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_isWall = true;
				break;
			case SubStatus.Phase1:
				bHasOver = false;
				switch (AiState)
				{
				case AI_STATE.mob_002:
				case AI_STATE.mob_004:
					if (NowPos.x > CenterPos.x)
					{
						UpdateDirection(-1);
					}
					else
					{
						UpdateDirection(1);
					}
					EndPos = NowPos + Vector3.right * 2f * base.direction;
					break;
				case AI_STATE.mob_003:
					EndPos = EndPosMob003;
					break;
				}
				StartPos = NowPos;
				_velocity = new VInt3((EndPos - StartPos).normalized) * MoveSpeed * 0.001f;
				fMoveDis = Vector2.Distance(StartPos, EndPos) - 0.3f;
				break;
			}
			break;
		default:
			throw new ArgumentOutOfRangeException();
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
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_JUMP_LOOP;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_LAND;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_DEBUT;
				break;
			case SubStatus.Phase3:
			case SubStatus.Phase4:
				return;
			default:
				throw new ArgumentOutOfRangeException();
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
		case MainStatus.Walk:
			_currentAnimationId = AnimationID.ANI_WALK;
			break;
		case MainStatus.Jump:
		case MainStatus.JumpToWall:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_JUMP_START;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_JUMP_LOOP;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_LAND;
				break;
			}
			break;
		case MainStatus.WallIdle:
			_currentAnimationId = AnimationID.ANI_WALLIDLE_START;
			break;
		case MainStatus.GroundTongue:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_GROUND_TONGUE_START;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_GROUND_TONGUE_END;
				break;
			}
			break;
		case MainStatus.WallToFall:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_JUMP_LOOP;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_LAND;
				break;
			}
			break;
		case MainStatus.Spike:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_SPIKE_JUMP_START;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_SPIKE_JUMP_LOOP;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_SPIKE_START;
				break;
			case SubStatus.Phase3:
			case SubStatus.Phase5:
				_currentAnimationId = AnimationID.ANI_SPIKE_LOOP;
				break;
			case SubStatus.Phase4:
				_currentAnimationId = AnimationID.ANI_SPIKE_END;
				break;
			}
			break;
		case MainStatus.WallTongue:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_WALL_WALK;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_WALL_TONGUE_START;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_WALL_TONGUE_END;
				break;
			case SubStatus.Phase5:
				_currentAnimationId = AnimationID.ANI_WALLIDLE_START;
				break;
			}
			break;
		case MainStatus.WallMoveTop:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_WALL_WALK;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_WALL_WALK;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_WALL_WALK;
				break;
			case SubStatus.Phase3:
				_currentAnimationId = AnimationID.ANI_WALL_WALK;
				break;
			case SubStatus.Phase5:
				_currentAnimationId = AnimationID.ANI_WALLIDLE_START;
				break;
			}
			break;
		case MainStatus.WallTailBeam:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_WALL_TAILBEAM_START;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_WALL_TAILBEAM_LOOP;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_WALL_TAILBEAM_END;
				break;
			case SubStatus.Phase5:
				_currentAnimationId = AnimationID.ANI_WALLIDLE_START;
				break;
			}
			break;
		case MainStatus.MoveToPos:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_WALLIDLE_START;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_WALL_WALK;
				break;
			}
			break;
		default:
			throw new ArgumentOutOfRangeException();
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

	private void UpdateRandomState()
	{
		if (AiState != 0)
		{
			return;
		}
		MainStatus mainStatus = MainStatus.Idle;
		if (StageUpdate.bIsHost)
		{
			if (bWaitNetStatus)
			{
				return;
			}
			TargetPos = Target.Controller.LogicPosition;
			if (nAIActionTimes < 1)
			{
				CopyList.Clear();
				bHasCopy = !bHasCopy;
			}
			switch (_mainStatus)
			{
			case MainStatus.Idle:
				if (nAIActionTimes > 0)
				{
					nAIActionTimes--;
					mainStatus = (bHasCopy ? MainStatus.Spike : ((OrangeBattleUtility.Random(0, 100) < 50) ? MainStatus.Jump : MainStatus.Spike));
				}
				else if (!bHasCopy)
				{
					int num2 = OrangeBattleUtility.Random(0, 150);
					if (num2 < 50)
					{
						mainStatus = MainStatus.Jump;
						break;
					}
					if (num2 < 100)
					{
						mainStatus = MainStatus.Spike;
						break;
					}
					mainStatus = MainStatus.JumpToWall;
					nAIActionTimes = 1;
				}
				else if (OrangeBattleUtility.Random(0, 100) < 50)
				{
					mainStatus = ((lastStatus == MainStatus.Spike) ? MainStatus.JumpToWall : MainStatus.Spike);
					if (mainStatus == MainStatus.JumpToWall)
					{
						nAIActionTimes = 1;
					}
				}
				else
				{
					mainStatus = MainStatus.JumpToWall;
					nAIActionTimes = 1;
				}
				break;
			case MainStatus.WallIdle:
			{
				if (nAIActionTimes > 0)
				{
					nAIActionTimes--;
					mainStatus = ((OrangeBattleUtility.Random(0, 100) < 50) ? MainStatus.WallTongue : MainStatus.WallMoveTop);
					break;
				}
				int num = OrangeBattleUtility.Random(0, 150);
				if (num < 50)
				{
					mainStatus = MainStatus.WallTongue;
					break;
				}
				if (num < 100)
				{
					mainStatus = MainStatus.WallMoveTop;
					break;
				}
				mainStatus = MainStatus.WallToFall;
				nAIActionTimes = 1;
				break;
			}
			default:
				mainStatus = ((_mainStatus == MainStatus.Idle) ? MainStatus.Spike : MainStatus.WallTongue);
				break;
			}
		}
		else if (bWaitNetStatus)
		{
			bWaitNetStatus = false;
		}
		if (DebugMode)
		{
			mainStatus = NextSkill;
		}
		if (bHasCopy && mainStatus == MainStatus.WallMoveTop)
		{
			nActionTimes = 4;
		}
		if (IsChipInfoAnim)
		{
			mainStatus = MainStatus.Idle;
		}
		lastStatus = mainStatus;
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
			UpdateDirection();
			SetStatus(mainStatus);
		}
		if (mainStatus == MainStatus.WallTongue)
		{
			WallTongueCount++;
		}
		else
		{
			WallTongueCount = 0;
		}
	}

	public override void BossIntro(Action cb)
	{
		IntroCallBack = cb;
		_introReady = true;
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
			SetColliderEnable(false);
			CopyBack();
			SetStatus(MainStatus.Dead);
		}
	}

	public void SetStealth(bool activate)
	{
		if (_isStealth == activate)
		{
			return;
		}
		_isStealth = activate;
		mhide_effect.ActiveEffect(activate, 0.25f, 0.25f);
		base.AllowAutoAim = !activate;
		if (AiState == AI_STATE.mob_001)
		{
			SetColliderEnable(!activate);
		}
		else
		{
			if (activate)
			{
				CopyFX.Clear();
				CopyFX.Stop();
			}
			else
			{
				CopyFX.Play();
			}
			SetColliderEnable(false);
		}
		if (activate)
		{
			_collideBullet.BackToPool();
			PlayBossSE("BossSE", 71);
		}
		else
		{
			_collideBullet.Active(targetMask);
			PlayBossSE("BossSE", 70);
		}
	}

	private BS124_Controller SpawnEnemy(Vector3 SpawnPos, int weapon = 5)
	{
		MOB_TABLE enemy = GetEnemy((int)EnemyWeapons[weapon].BulletData.f_EFFECT_X);
		if (enemy == null)
		{
			Debug.LogError("要生成的怪物資料有誤，生怪技能ID " + weapon + " 怪物GroupID " + EnemyWeapons[weapon].BulletData.f_EFFECT_X);
			return null;
		}
		EnemyControllerBase enemyControllerBase = StageUpdate.StageSpawnEnemyByMob(enemy, sNetSerialID + SpawnCount);
		SpawnCount++;
		if ((bool)enemyControllerBase)
		{
			enemyControllerBase.SetPositionAndRotation(NowPos, base.direction == -1);
			return enemyControllerBase as BS124_Controller;
		}
		return null;
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

	public void SetMob003EndPos(Vector3 endpos)
	{
		EndPosMob003 = endpos;
	}

	public void SetTongueOffset(Vector3 offset)
	{
		WallTongueOffset = offset;
	}

	public void SetCopyStatus(int status)
	{
		bHasOver = false;
		SetStatus((MainStatus)status);
	}

	private Vector3 GetTargetPos(bool realcenter = false)
	{
		if (!Target)
		{
			Target = _enemyAutoAimSystem.GetClosetPlayer();
		}
		if (!Target)
		{
			for (int i = 0; i < StageUpdate.runPlayers.Count; i++)
			{
				if ((bool)StageUpdate.runPlayers[i])
				{
					Target = StageUpdate.runPlayers[i];
					break;
				}
			}
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
		return NowPos + Vector3.up * 1f;
	}

	private void CopyBack()
	{
		foreach (BS124_Controller copy in CopyList)
		{
			if (copy.Activate)
			{
				copy.BackToPool();
			}
		}
		CopyList.Clear();
	}
}
