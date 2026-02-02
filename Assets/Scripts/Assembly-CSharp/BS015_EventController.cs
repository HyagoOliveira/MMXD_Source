using System;
using CodeStage.AntiCheat.ObscuredTypes;
using StageLib;
using UnityEngine;

public class BS015_EventController : EnemyControllerBase, IManagedUpdateBehavior
{
	private enum MainStatus
	{
		Idle = 0,
		DashAttack = 1,
		SwingAttack = 2,
		Dead = 3,
		SwingAttackStart = 4,
		Hurt = 5,
		Turn = 6,
		IdleWaitNet = 7
	}

	private enum SubStatus
	{
		Prepare = 0,
		Normal = 1,
		End = 2,
		MAX_SUBSTATUS = 3
	}

	public enum AnimationID
	{
		ANI_IDLE = 0,
		ANI_SKILL0 = 1,
		ANI_SKILL1 = 2,
		ANI_HURT = 3,
		ANI_DEAD = 4,
		ANI_TURN = 5,
		ANI_ACCELERATE = 6,
		ANI_SKILL1START = 7,
		MAX_ANIMATION_ID = 8
	}

	private MainStatus _mainStatus;

	private SubStatus _subStatus;

	private AnimationID _currentAnimationId;

	public int MoveSpeed = 3000;

	public int DashSpeed = 5000;

	private readonly int _hashHspd = Animator.StringToHash("fHspd");

	private float _currentFrame;

	private int[] _animationHash;

	private Transform _efxpointTransform;

	private Transform _efxpointTransform2;

	protected bool isStarting;

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
		base.AllowAutoAim = false;
		Transform[] target = _transform.GetComponentsInChildren<Transform>(true);
		base.AimTransform = OrangeBattleUtility.FindChildRecursive(ref target, "Body_jnt");
		_animator = GetComponentInChildren<Animator>();
		ModelTransform = OrangeBattleUtility.FindChildRecursive(ref target, "model", true);
		_efxpointTransform = OrangeBattleUtility.FindChildRecursive(ref target, "efxpoint", true);
		_efxpointTransform2 = OrangeBattleUtility.FindChildRecursive(ref target, "efxpoint2", true);
		_collideBullet = OrangeBattleUtility.FindChildRecursive(ref target, "Mace_ctrl", true).gameObject.AddOrGetComponent<CollideBullet>();
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxhit_maoh_000", 2);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_bs15_atk", 2);
		_animationHash = new int[8];
		_animationHash[0] = Animator.StringToHash("Idle");
		_animationHash[4] = Animator.StringToHash("Dead");
		_animationHash[3] = Animator.StringToHash("Hurt");
		_animationHash[1] = Animator.StringToHash("Skill0");
		_animationHash[2] = Animator.StringToHash("Skill1");
		_animationHash[5] = Animator.StringToHash("Turn");
		_animationHash[6] = Animator.StringToHash("Accelerate");
		_animationHash[7] = Animator.StringToHash("Skill1_loop");
		base.direction = 1;
		SetStatus(MainStatus.Idle);
	}

	private void SetStatus(MainStatus mainStatus, SubStatus subStatus = SubStatus.Prepare)
	{
		_mainStatus = mainStatus;
		_subStatus = subStatus;
		switch (_mainStatus)
		{
		case MainStatus.Idle:
		case MainStatus.IdleWaitNet:
			PlaySE("BossSE", 213);
			_velocity.x = 0;
			break;
		case MainStatus.DashAttack:
			switch (_subStatus)
			{
			case SubStatus.Prepare:
				_velocity.x = 0;
				break;
			case SubStatus.Normal:
				_velocity.x = Math.Sign(base.direction) * DashSpeed;
				break;
			default:
				throw new ArgumentOutOfRangeException();
			case SubStatus.End:
				break;
			}
			break;
		case MainStatus.SwingAttack:
		case MainStatus.SwingAttackStart:
			PlaySE("BossSE", 214);
			PlaySE("BossSE", 209);
			_velocity.x = Math.Sign(base.direction) * MoveSpeed;
			break;
		case MainStatus.Dead:
			_velocity.x = 0;
			break;
		case MainStatus.Hurt:
			switch (_subStatus)
			{
			case SubStatus.Prepare:
				_velocity = new VInt3(-_velocity.x, IntMath.Abs(_velocity.x) * 2, 0);
				break;
			case SubStatus.Normal:
				_velocity.x = 0;
				break;
			default:
				throw new ArgumentOutOfRangeException("subStatus", subStatus, null);
			case SubStatus.End:
				break;
			}
			break;
		case MainStatus.Turn:
			_velocity.x = 0;
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
		AiTimer.TimerStart();
		UpdateAnimation();
		UpdateCollider();
	}

	public override void LogicUpdate()
	{
		if (!Activate)
		{
			return;
		}
		base.LogicUpdate();
		_currentFrame = _animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
		switch (_mainStatus)
		{
		case MainStatus.Idle:
		{
			if (!(_currentFrame > 1f) || !Controller.Collisions.below)
			{
				break;
			}
			Vector2 origin = base.AimTransform.position;
			for (int i = 0; i < 5; i++)
			{
				if ((bool)Physics2D.Raycast(origin, Vector2.right * base.direction, 100f, targetMask))
				{
					UpdateRandomState();
					break;
				}
				origin.y -= 1f;
			}
			break;
		}
		case MainStatus.DashAttack:
			switch (_subStatus)
			{
			case SubStatus.Prepare:
				if ((double)_currentFrame > 1.0)
				{
					SetStatus(_mainStatus, SubStatus.Normal);
				}
				break;
			default:
				throw new ArgumentOutOfRangeException();
			case SubStatus.Normal:
			case SubStatus.End:
				break;
			}
			break;
		case MainStatus.SwingAttack:
		case MainStatus.SwingAttackStart:
			if ((double)_currentFrame > 1.0)
			{
				if (_mainStatus == MainStatus.SwingAttack)
				{
					SetStatus(MainStatus.SwingAttackStart);
					MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_bs15_atk", _efxpointTransform2, Quaternion.Euler(0f, 0f, 0f), Array.Empty<object>());
					MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxhit_maoh_000", _efxpointTransform2, Quaternion.Euler(0f, 0f, 0f), Array.Empty<object>());
				}
				else
				{
					SetStatus(MainStatus.SwingAttack);
				}
			}
			break;
		case MainStatus.Hurt:
			switch (_subStatus)
			{
			case SubStatus.Prepare:
				if (Controller.Collisions.below)
				{
					SetStatus(_mainStatus, SubStatus.Normal);
				}
				break;
			case SubStatus.Normal:
				if ((double)_currentFrame > 2.0)
				{
					SetStatus(MainStatus.Turn);
				}
				break;
			default:
				throw new ArgumentOutOfRangeException();
			case SubStatus.End:
				break;
			}
			break;
		case MainStatus.Turn:
			switch (_subStatus)
			{
			case SubStatus.Prepare:
				LeanTween.value(base.direction * 90, -base.direction * 90, 1f).setOnUpdate(delegate(float f)
				{
					ModelTransform.eulerAngles = Vector3.up * f;
				}).setOnComplete((Action)delegate
				{
					base.direction *= -1;
					SetStatus(_mainStatus, SubStatus.End);
				});
				SetStatus(_mainStatus, SubStatus.Normal);
				break;
			case SubStatus.End:
				SetStatus(MainStatus.Idle);
				break;
			case SubStatus.Normal:
				break;
			}
			break;
		default:
			throw new ArgumentOutOfRangeException();
		case MainStatus.Dead:
			break;
		}
	}

	public void UpdateFunc()
	{
		if (!Activate)
		{
			if (isStarting)
			{
				PlaySE("BossSE", 214);
				PlaySE("BossSE", 210);
				isStarting = false;
			}
			return;
		}
		if (!isStarting)
		{
			PlaySE("BossSE", 213);
			isStarting = (BattleInfoUI.Instance.IsBossAppear = base.gameObject.activeSelf);
		}
		base.transform.localPosition = Vector3.MoveTowards(base.transform.localPosition, Controller.LogicPosition.vec3, distanceDelta);
		_animator.SetFloat(_hashHspd, (float)_velocity.x / (float)MoveSpeed);
	}

	private void UpdateRandomState()
	{
		MainStatus mainStatus = (MainStatus)OrangeBattleUtility.Random(2, 3);
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

	private void UpdateCollider()
	{
		switch (_mainStatus)
		{
		case MainStatus.Idle:
		case MainStatus.Dead:
		case MainStatus.Hurt:
		case MainStatus.Turn:
		case MainStatus.IdleWaitNet:
			if (_collideBullet.IsActivate)
			{
				_collideBullet.IsDestroy = true;
			}
			break;
		case MainStatus.DashAttack:
			switch (_subStatus)
			{
			case SubStatus.Prepare:
				_collideBullet.UpdateBulletData(EnemyWeapons[0].BulletData);
				_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
				_collideBullet.BackToPool();
				_collideBullet.Active(targetMask);
				break;
			default:
				throw new ArgumentOutOfRangeException();
			case SubStatus.Normal:
			case SubStatus.End:
				break;
			}
			break;
		case MainStatus.SwingAttack:
		case MainStatus.SwingAttackStart:
			switch (_subStatus)
			{
			case SubStatus.Prepare:
				_collideBullet.UpdateBulletData(EnemyWeapons[1].BulletData);
				_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
				_collideBullet.BackToPool();
				_collideBullet.Active(targetMask);
				break;
			default:
				throw new ArgumentOutOfRangeException();
			case SubStatus.Normal:
			case SubStatus.End:
				break;
			}
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	private void UpdateAnimation()
	{
		switch (_mainStatus)
		{
		case MainStatus.Idle:
		case MainStatus.IdleWaitNet:
			_currentAnimationId = AnimationID.ANI_IDLE;
			break;
		case MainStatus.Dead:
			_currentAnimationId = AnimationID.ANI_DEAD;
			break;
		case MainStatus.DashAttack:
			switch (_subStatus)
			{
			case SubStatus.Prepare:
				_currentAnimationId = AnimationID.ANI_ACCELERATE;
				break;
			case SubStatus.Normal:
				_currentAnimationId = AnimationID.ANI_SKILL0;
				break;
			}
			break;
		case MainStatus.SwingAttack:
			_currentAnimationId = AnimationID.ANI_SKILL1;
			break;
		case MainStatus.SwingAttackStart:
			_currentAnimationId = AnimationID.ANI_SKILL1START;
			break;
		case MainStatus.Hurt:
			_currentAnimationId = AnimationID.ANI_HURT;
			break;
		case MainStatus.Turn:
			_currentAnimationId = AnimationID.ANI_TURN;
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
		_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
	}

	private void LateUpdate()
	{
		if (isStarting && BattleInfoUI.Instance != null && BattleInfoUI.Instance.IsPlayClearBgm)
		{
			PlaySE("BossSE", 214);
			PlaySE("BossSE", 210);
			isStarting = false;
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
		ModelTransform.eulerAngles = Vector3.up * base.direction * 90f;
		base.transform.position = pos;
	}

	public override ObscuredInt Hurt(HurtPassParam tHurtPassParam)
	{
		if ((int)Hp <= 0)
		{
			if (DeadCallback != null)
			{
				DeadCallback();
			}
			BackToPool();
			StageObjParam component = GetComponent<StageObjParam>();
			if (component != null && component.nEventID != 0)
			{
				EventManager.StageEventCall stageEventCall = new EventManager.StageEventCall();
				stageEventCall.nID = component.nEventID;
				component.nEventID = 0;
				Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_EVENT_CALL, stageEventCall);
			}
		}
		return Hp;
	}
}
