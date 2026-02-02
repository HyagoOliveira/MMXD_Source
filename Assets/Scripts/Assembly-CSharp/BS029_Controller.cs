using System;
using System.Collections.Generic;
using StageLib;
using UnityEngine;

public class BS029_Controller : EnemyControllerBase, IManagedUpdateBehavior
{
	private enum MainStatus
	{
		Idle = 0,
		Debut = 1,
		Run = 2,
		Jump = 3,
		Belly = 4,
		Barrier = 5,
		Hurt = 6,
		Dead = 7,
		IdleWaitNet = 8,
		MAX_STATUS = 9
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

	private enum BarrierStatus
	{
		Outside = 0,
		Inside = 1,
		ReverseInside = 2
	}

	public enum AnimationID
	{
		ANI_IDLE = 0,
		ANI_DEBUT = 1,
		ANI_HURT = 2,
		ANI_RUN = 3,
		ANI_DEAD = 4,
		ANI_JUMP_START = 5,
		ANI_JUMP_LOOP = 6,
		ANI_LAND = 7,
		ANI_SKILL0_START = 8,
		ANI_SKILL0_END = 9,
		ANI_SKILL1_START = 10,
		ANI_SKILL1_LOOP = 11,
		ANI_SKILL1_END = 12,
		ANI_SKILL2_START = 13,
		ANI_SKILL2_LOOP = 14,
		ANI_SKILL2_END = 15,
		MAX_ANIMATION_ID = 16
	}

	private AnimationID _currentAnimationId;

	public int RunSpeed = 3000;

	public int JumpSpeed = 15000;

	private int _prevDirection = 1;

	private readonly int _hashHspd = Animator.StringToHash("fHspd");

	private readonly int _hashVspd = Animator.StringToHash("fVspd");

	private bool _isBarrierBusy;

	private readonly Stack<BarrierStatus> _barrierStatuses = new Stack<BarrierStatus>();

	private BarrierStatus _currentBarrierStatus;

	private Transform _handShootTransform;

	private Transform _chestShootTransform;

	private int[] _animatorHash;

	public float BarrierInsideDistance = 1.8f;

	public float BarrierOutsideDistance = 20f;

	private CollideBullet _leftCollideBullet;

	private CollideBullet _rightCollideBullet;

	private MainStatus _mainStatus;

	private SubStatus _subStatus;

	private float _currentFrame;

	private bool _bDeadCallResult = true;

	private bool _jumpFlag;

	private Vector3 distance;

	private int useWeapon;

	private bool throwDone;

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
		_leftCollideBullet = OrangeBattleUtility.FindChildRecursive(ref target, "BarrierLeft").gameObject.AddOrGetComponent<CollideBullet>();
		_rightCollideBullet = OrangeBattleUtility.FindChildRecursive(ref target, "BarrierRight").gameObject.AddOrGetComponent<CollideBullet>();
		_handShootTransform = OrangeBattleUtility.FindChildRecursive(ref target, "ShootPoint_Cannon");
		_chestShootTransform = OrangeBattleUtility.FindChildRecursive(ref target, "ShootPoint_Chest");
		_animator = GetComponentInChildren<Animator>();
		_animatorHash = new int[16];
		for (int i = 0; i < 16; i++)
		{
			_animatorHash[i] = Animator.StringToHash("idle");
		}
		_animatorHash[0] = Animator.StringToHash("BS029@idle_loop");
		_animatorHash[5] = Animator.StringToHash("BS029@skill_01_jump_start");
		_animatorHash[6] = Animator.StringToHash("BS029@skill_01_jump_loop");
		_animatorHash[7] = Animator.StringToHash("BS029@skill_01_landing");
		_animatorHash[1] = Animator.StringToHash("BS029@debut");
		_animatorHash[2] = Animator.StringToHash("BS029@hurt_loop");
		_animatorHash[4] = Animator.StringToHash("BS029@dead");
		_animatorHash[3] = Animator.StringToHash("BS029@run_loop");
		_animatorHash[8] = Animator.StringToHash("BS029@skill_01_jump_atk_start");
		_animatorHash[9] = Animator.StringToHash("BS029@skill_01_jump_atk_end");
		_animatorHash[10] = Animator.StringToHash("BS029@skill_02_start");
		_animatorHash[11] = Animator.StringToHash("BS029@skill_02_loop");
		_animatorHash[12] = Animator.StringToHash("BS029@skill_02_end");
		_animatorHash[13] = Animator.StringToHash("BS029@skill_03_atk_start");
		_animatorHash[14] = Animator.StringToHash("BS029@skill_03_atk_loop");
		_animatorHash[15] = Animator.StringToHash("BS029@skill_03_atk_end");
		_maxGravity *= 2;
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxstory_explode_000", 10);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("FX_BOSS_EXPLODE2");
		SetStatus(MainStatus.Debut);
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
		AI_STATE aiState = AiState;
		if ((uint)(aiState - 1) <= 1u)
		{
			_bDeadCallResult = false;
		}
		else
		{
			_bDeadCallResult = true;
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
			_collideBullet.UpdateBulletData(EnemyWeapons[0].BulletData);
			_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			_collideBullet.Active(targetMask);
			_leftCollideBullet.UpdateBulletData(EnemyWeapons[3].BulletData);
			_leftCollideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			_leftCollideBullet.Active(targetMask);
			_rightCollideBullet.UpdateBulletData(EnemyWeapons[3].BulletData);
			_rightCollideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			_rightCollideBullet.Active(targetMask);
			base.SoundSource.ForcePlaySE("BossSE", "bs103_dkm07");
		}
		else
		{
			_collideBullet.BackToPool();
			_leftCollideBullet.BackToPool();
			_rightCollideBullet.BackToPool();
		}
	}

	public void UpdateFunc()
	{
		if (Activate || _mainStatus == MainStatus.Debut)
		{
			_animator.SetFloat(_hashVspd, (float)_velocity.y * 0.001f);
			base.transform.localPosition = Vector3.MoveTowards(base.transform.localPosition, Controller.LogicPosition.vec3, distanceDelta);
		}
	}

	private void SetStatus(MainStatus mainStatus, SubStatus subStatus = SubStatus.Phase0)
	{
		_mainStatus = mainStatus;
		_subStatus = subStatus;
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
		case MainStatus.Idle:
			_velocity.x = 0;
			break;
		case MainStatus.Run:
			_velocity.x = base.direction * RunSpeed;
			break;
		case MainStatus.Jump:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_velocity = VInt3.zero;
				UpdateMagazine(1, true);
				break;
			case SubStatus.Phase1:
				_velocity.x = base.direction * RunSpeed;
				_velocity.y = JumpSpeed;
				break;
			case SubStatus.Phase2:
				IgnoreGravity = true;
				_velocity = VInt3.zero;
				UpdateDirection();
				_prevDirection = base.direction;
				break;
			case SubStatus.Phase3:
				UpdateDirection(_prevDirection);
				_velocity.x = base.direction * RunSpeed;
				_velocity.y = 0;
				IgnoreGravity = false;
				break;
			case SubStatus.Phase4:
				_velocity.x = 0;
				break;
			}
			break;
		case MainStatus.Belly:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				UpdateMagazine(2, true);
				break;
			default:
				throw new ArgumentOutOfRangeException();
			case SubStatus.Phase1:
			case SubStatus.Phase2:
			case SubStatus.Phase3:
			case SubStatus.Phase4:
			case SubStatus.Phase5:
			case SubStatus.MAX_SUBSTATUS:
				break;
			}
			break;
		case MainStatus.Dead:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
			{
				base.AllowAutoAim = false;
				_velocity.x = 0;
				AI_STATE aiState = AiState;
				if (aiState == AI_STATE.mob_003)
				{
					base.DeadPlayCompleted = true;
				}
				OrangeBattleUtility.LockPlayer();
				if (!Controller.Collisions.below)
				{
					SetStatus(MainStatus.Dead, SubStatus.Phase2);
				}
				break;
			}
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
			case SubStatus.Phase2:
				IgnoreGravity = true;
				break;
			}
			break;
		}
		UpdateAnimation();
		UpdateBarrierStatus();
		AiTimer.TimerStart();
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
		if ((!Activate || !_enemyAutoAimSystem) && _mainStatus != MainStatus.Debut)
		{
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
				if (_introReady)
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
		case MainStatus.Idle:
			Target = _enemyAutoAimSystem.GetClosetPlayer();
			UpdateDirection();
			if ((bool)Target)
			{
				UpdateRandomState();
			}
			break;
		case MainStatus.Run:
			if (AiTimer.GetMillisecond() > 500)
			{
				SetStatus(MainStatus.Idle);
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
				else if (_currentFrame > 0.75f && (bool)Controller.BelowInBypassRange)
				{
					_velocity.x = base.direction * RunSpeed;
					_velocity.y = JumpSpeed;
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
					SetStatus(_mainStatus, (EnemyWeapons[useWeapon].MagazineRemain > 0f) ? SubStatus.Phase2 : SubStatus.Phase3);
				}
				else if (_currentFrame > 0.5f)
				{
					useWeapon = 1;
					if (!EnemyWeapons[useWeapon].LastUseTimer.IsStarted() || EnemyWeapons[useWeapon].LastUseTimer.GetMillisecond() > EnemyWeapons[useWeapon].BulletData.n_FIRE_SPEED)
					{
						Transform handShootTransform = _handShootTransform;
						BulletBase.TryShotBullet(EnemyWeapons[useWeapon].BulletData, handShootTransform, (Target.GetTargetPoint() - handShootTransform.position).normalized, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
						EnemyWeapons[useWeapon].LastUseTimer.TimerStart();
						EnemyWeapons[useWeapon].MagazineRemain -= 1f;
					}
				}
				break;
			case SubStatus.Phase3:
				if ((bool)Controller.BelowInBypassRange)
				{
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
		case MainStatus.Belly:
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
					SetStatus(MainStatus.Idle);
				}
				else if (_currentFrame > 0.3f && EnemyWeapons[useWeapon].MagazineRemain > 0f)
				{
					useWeapon = 2;
					if (!EnemyWeapons[useWeapon].LastUseTimer.IsStarted() || EnemyWeapons[useWeapon].LastUseTimer.GetMillisecond() > EnemyWeapons[useWeapon].BulletData.n_FIRE_SPEED)
					{
						Transform chestShootTransform = _chestShootTransform;
						BulletBase.TryShotBullet(EnemyWeapons[useWeapon].BulletData, chestShootTransform, (Target.GetTargetPoint() - chestShootTransform.position).normalized, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
						EnemyWeapons[useWeapon].LastUseTimer.TimerStart();
						EnemyWeapons[useWeapon].MagazineRemain -= 1f;
					}
				}
				break;
			}
			break;
		case MainStatus.Barrier:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if ((double)_currentFrame > 1.0)
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
					SetStatus(MainStatus.Idle);
				}
				break;
			}
			break;
		case MainStatus.Dead:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if ((double)_currentFrame > 0.4)
				{
					SetStatus(_mainStatus, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase2:
				if ((double)_currentFrame > 4.0)
				{
					SetStatus(_mainStatus, SubStatus.Phase1);
				}
				break;
			}
			break;
		default:
			SetStatus(MainStatus.Idle);
			break;
		case MainStatus.Hurt:
		case MainStatus.IdleWaitNet:
			break;
		}
		if (!_isBarrierBusy && _barrierStatuses.Count != 0)
		{
			SetBarrierStatus(_barrierStatuses.Pop());
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
		case MainStatus.Idle:
		case MainStatus.IdleWaitNet:
			_currentAnimationId = AnimationID.ANI_IDLE;
			break;
		case MainStatus.Dead:
			switch (_subStatus)
			{
			default:
				return;
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_DEAD;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_HURT;
				break;
			}
			break;
		case MainStatus.Hurt:
			_currentAnimationId = AnimationID.ANI_HURT;
			break;
		case MainStatus.Run:
			_currentAnimationId = AnimationID.ANI_RUN;
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
				_currentAnimationId = AnimationID.ANI_SKILL0_START;
				break;
			case SubStatus.Phase3:
				_currentAnimationId = AnimationID.ANI_JUMP_LOOP;
				break;
			case SubStatus.Phase4:
				_currentAnimationId = AnimationID.ANI_LAND;
				break;
			}
			break;
		case MainStatus.Belly:
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
		case MainStatus.Barrier:
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
		default:
			throw new ArgumentOutOfRangeException();
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
		PlaySE(ExplodeSE[0], ExplodeSE[1]);
		SetStatus(MainStatus.Dead);
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
		MainStatus mainStatus = (MainStatus)OrangeBattleUtility.Random(2, 6);
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

	private void UpdateBarrierStatus()
	{
		switch (_mainStatus)
		{
		case MainStatus.Barrier:
		{
			SubStatus subStatus = _subStatus;
			if (subStatus == SubStatus.Phase2)
			{
				_barrierStatuses.Clear();
				_barrierStatuses.Push(BarrierStatus.Inside);
				base.SoundSource.PlaySE("BossSE", "bs103_dkm03_lp");
			}
			break;
		}
		case MainStatus.Run:
		case MainStatus.Jump:
		case MainStatus.Hurt:
		case MainStatus.Dead:
		{
			SubStatus subStatus = _subStatus;
			if (subStatus == SubStatus.Phase0 || subStatus == SubStatus.Phase2)
			{
				_barrierStatuses.Clear();
				_barrierStatuses.Push(BarrierStatus.Outside);
			}
			break;
		}
		case MainStatus.Idle:
		case MainStatus.Debut:
		case MainStatus.Belly:
		case MainStatus.IdleWaitNet:
			break;
		}
	}

	private void SetBarrierStatus(BarrierStatus barrierStatus)
	{
		if (_currentBarrierStatus == barrierStatus)
		{
			return;
		}
		_currentBarrierStatus = barrierStatus;
		switch (_currentBarrierStatus)
		{
		case BarrierStatus.Outside:
			_isBarrierBusy = true;
			LeanTween.value(base.gameObject, _leftCollideBullet._transform.localPosition.x, 0f - BarrierOutsideDistance, 1f).setOnUpdate(delegate(float f)
			{
				Vector3 localPosition2 = _leftCollideBullet._transform.localPosition;
				localPosition2.x = f;
				_leftCollideBullet._transform.localPosition = localPosition2;
			}).setOnComplete((Action)delegate
			{
				base.SoundSource.PlaySE("BossSE", "bs103_dkm03_stop");
				_isBarrierBusy = false;
			});
			LeanTween.value(base.gameObject, _rightCollideBullet._transform.localPosition.x, BarrierOutsideDistance, 1f).setOnUpdate(delegate(float f)
			{
				Vector3 localPosition = _rightCollideBullet._transform.localPosition;
				localPosition.x = f;
				_rightCollideBullet._transform.localPosition = localPosition;
			});
			break;
		case BarrierStatus.Inside:
			_isBarrierBusy = true;
			LeanTween.value(base.gameObject, _leftCollideBullet._transform.localPosition.x, 0f - BarrierInsideDistance, 1f).setOnUpdate(delegate(float f)
			{
				Vector3 localPosition4 = _leftCollideBullet._transform.localPosition;
				localPosition4.x = f;
				_leftCollideBullet._transform.localPosition = localPosition4;
			}).setOnComplete((Action)delegate
			{
				if (_barrierStatuses.Count == 0)
				{
					_barrierStatuses.Push(BarrierStatus.ReverseInside);
				}
				_isBarrierBusy = false;
			});
			LeanTween.value(base.gameObject, _rightCollideBullet._transform.localPosition.x, BarrierInsideDistance, 1f).setOnUpdate(delegate(float f)
			{
				Vector3 localPosition3 = _rightCollideBullet._transform.localPosition;
				localPosition3.x = f;
				_rightCollideBullet._transform.localPosition = localPosition3;
			});
			break;
		case BarrierStatus.ReverseInside:
			_isBarrierBusy = true;
			LeanTween.value(base.gameObject, _leftCollideBullet._transform.localPosition.x, BarrierInsideDistance, 1f).setOnUpdate(delegate(float f)
			{
				Vector3 localPosition6 = _leftCollideBullet._transform.localPosition;
				localPosition6.x = f;
				_leftCollideBullet._transform.localPosition = localPosition6;
			}).setOnComplete((Action)delegate
			{
				if (_barrierStatuses.Count == 0)
				{
					_barrierStatuses.Push(BarrierStatus.Inside);
				}
				_isBarrierBusy = false;
			});
			LeanTween.value(base.gameObject, _rightCollideBullet._transform.localPosition.x, 0f - BarrierInsideDistance, 1f).setOnUpdate(delegate(float f)
			{
				Vector3 localPosition5 = _rightCollideBullet._transform.localPosition;
				localPosition5.x = f;
				_rightCollideBullet._transform.localPosition = localPosition5;
			});
			break;
		}
	}
}
