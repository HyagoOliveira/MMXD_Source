using System;
using CallbackDefs;
using Newtonsoft.Json;
using StageLib;
using UnityEngine;

public class BS030_Controller : EnemyControllerBase, IManagedUpdateBehavior
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
		WallToFall = 12
	}

	private enum SubStatus
	{
		Phase0 = 0,
		Phase1 = 1,
		Phase2 = 2,
		Phase3 = 3,
		Phase4 = 4,
		Phase5 = 5,
		MAX_SUBSTATUS = 6
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

	private MainStatus _mainStatus;

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

	private bool IsChipInfoAnim;

	private bs030_effect mhide_effect;

	private bool _bDeadCallResult = true;

	private bool _triggerFlag;

	private int _shootCount;

	private bool freshSetStatus;

	private int WallTongueCount;

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
		_tailShootPoint = OrangeBattleUtility.FindChildRecursive(ref target, "TailShootPoint", true);
		mhide_effect = OrangeBattleUtility.FindChildRecursive(ref target, "efx_bs030", true).GetComponent<bs030_effect>();
		_animator = GetComponentInChildren<Animator>();
		_collideBullet = base.gameObject.AddOrGetComponent<CollideBullet>();
		_collideBullet.isForceSE = (_collideBullet.isBossBullet = true);
		_tongueCollideBullet = OrangeBattleUtility.FindChildRecursive(ref target, "TongueCollider", true).gameObject.AddOrGetComponent<CollideBullet>();
		_tongueCollideBullet.isForceSE = (_tongueCollideBullet.isBossBullet = true);
		base.AimTransform = OrangeBattleUtility.FindChildRecursive(ref target, "Bip Spine1", true);
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
			UpdateRandomState();
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
				if (_currentFrame >= 1f)
				{
					SetStatus(MainStatus.Idle);
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
					SetStatus(MainStatus.Idle);
				}
				break;
			}
			break;
		case MainStatus.WallIdle:
			if (_currentFrame >= 1f)
			{
				UpdateRandomState();
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
					SetStatus(_mainStatus, SubStatus.Phase3);
				}
				break;
			case SubStatus.Phase3:
				if (_currentFrame >= 5f)
				{
					SetStatus(_mainStatus, SubStatus.Phase4);
				}
				if (!EnemyWeapons[3].LastUseTimer.IsStarted() || EnemyWeapons[3].LastUseTimer.GetMillisecond() > EnemyWeapons[3].BulletData.n_FIRE_SPEED)
				{
					EnemyWeapons[3].LastUseTimer.TimerStart();
					BulletBase.TryShotBullet(EnemyWeapons[3].BulletData, new Vector2(OrangeBattleUtility.Random(leftTop.x, rightTop.x), leftTop.y - 0.5f), Vector3.down, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
				}
				break;
			case SubStatus.Phase4:
				if (_currentFrame >= 1f)
				{
					SetStatus(MainStatus.Jump, SubStatus.Phase1);
				}
				break;
			}
			break;
		case MainStatus.WallTongue:
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
				if (_currentFrame >= 1f)
				{
					SetStatus(MainStatus.WallIdle);
				}
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
				if (_currentFrame >= 1f)
				{
					_shootCount--;
					if (_shootCount > 0)
					{
						SetStatus(_mainStatus, SubStatus.Phase1);
					}
					else
					{
						SetStatus(MainStatus.WallIdle);
					}
				}
				break;
			}
			break;
		case MainStatus.WallMoveTop:
			switch (_subStatus)
			{
			case SubStatus.Phase1:
				if (Controller.Collisions.above || (_velocity.x > 0 && Controller.Collisions.right) || (_velocity.x < 0 && Controller.Collisions.left))
				{
					SetStatus(_mainStatus, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase0:
			case SubStatus.Phase2:
			case SubStatus.Phase3:
			case SubStatus.Phase4:
			case SubStatus.Phase5:
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
		}
		else
		{
			_collideBullet.BackToPool();
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
		AI_STATE aiState2 = AiState;
		if (aiState2 == AI_STATE.mob_002)
		{
			_bDeadCallResult = false;
		}
		else
		{
			_bDeadCallResult = true;
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
		ModelTransform.localScale = new Vector3(ModelTransform.localScale.x, ModelTransform.localScale.y, Mathf.Abs(ModelTransform.localScale.z) * (float)base.direction);
		base.transform.position = pos;
		UpdateDirection((!bBack) ? 1 : (-1));
		Vector3 position = _transform.position;
		RaycastHit2D raycastHit2D = Physics2D.Raycast(position, Vector2.left, 100f, Controller.collisionMask);
		RaycastHit2D raycastHit2D2 = Physics2D.Raycast(position, Vector2.right, 100f, Controller.collisionMask);
		RaycastHit2D raycastHit2D3 = Physics2D.Raycast(position, Vector2.up, 100f, Controller.collisionMask);
		if ((bool)raycastHit2D2 && (bool)raycastHit2D && (bool)raycastHit2D3)
		{
			leftTop = new Vector3(position.x - raycastHit2D.distance + 0.25f, position.y + raycastHit2D3.distance - 0.25f, 0f);
			rightTop = new Vector3(position.x + raycastHit2D2.distance - 0.25f, position.y + raycastHit2D3.distance - 0.25f, 0f);
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
			}
			break;
		case MainStatus.WallTongue:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				SetStealth(true);
				_triggerFlag = false;
				_targetPos = TargetPos.vec3;
				_targetPos.x -= (float)base.direction * 2.7f;
				_targetPos.y += 1f;
				break;
			case SubStatus.Phase1:
				UpdateDirection();
				_velocity = VInt3.zero;
				SetStealth(false);
				break;
			case SubStatus.Phase2:
				_tongueCollideBullet.BackToPool();
				break;
			}
			break;
		case MainStatus.WallMoveTop:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
			{
				Vector3 vector = leftTop;
				if (TargetPos.vec3.x > _transform.position.x)
				{
					vector = rightTop;
				}
				_velocity = new VInt3((vector - _transform.position).normalized * (float)MoveSpeed);
				SetStatus(_mainStatus, SubStatus.Phase1);
				break;
			}
			case SubStatus.Phase2:
				_shootCount = 3;
				SetStatus(MainStatus.WallTailBeam);
				break;
			default:
				throw new ArgumentOutOfRangeException();
			case SubStatus.Phase1:
			case SubStatus.Phase3:
			case SubStatus.Phase4:
			case SubStatus.Phase5:
			case SubStatus.MAX_SUBSTATUS:
				break;
			}
			break;
		case MainStatus.WallTailBeam:
			SetStealth(false);
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_triggerFlag = false;
				break;
			case SubStatus.Phase1:
				_tongueCollideBullet.BackToPool();
				_triggerFlag = false;
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
		MainStatus mainStatus = MainStatus.Idle;
		if (StageUpdate.bIsHost)
		{
			if (bWaitNetStatus)
			{
				return;
			}
			Target = _enemyAutoAimSystem.GetClosetPlayer();
			if (Target == null)
			{
				if (_mainStatus != MainStatus.Dead)
				{
					SetStatus(mainStatus);
				}
				return;
			}
			TargetPos = Target.Controller.LogicPosition;
			if (WallTongueCount < 3 && _mainStatus == MainStatus.WallIdle && (float)Mathf.Abs(TargetPos.x - Controller.LogicPosition.x) < groundTongueDistance * 1000f)
			{
				mainStatus = MainStatus.WallTongue;
			}
			else
			{
				int num = 10;
				do
				{
					mainStatus = (_isWall ? _wallStatus[OrangeBattleUtility.Random(0, _wallStatus.Length)] : _groundStatus[OrangeBattleUtility.Random(0, _groundStatus.Length)]);
					num--;
				}
				while (mainStatus == _mainStatus && num > 0);
			}
		}
		else if (bWaitNetStatus)
		{
			bWaitNetStatus = false;
		}
		if (IsChipInfoAnim)
		{
			mainStatus = MainStatus.Idle;
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
			SetStatus(MainStatus.Dead);
		}
	}

	private void SetStealth(bool activate)
	{
		if (_isStealth != activate)
		{
			_isStealth = activate;
			mhide_effect.ActiveEffect(activate, 0.25f, 0.25f);
			base.AllowAutoAim = !activate;
			SetColliderEnable(!activate);
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
	}
}
