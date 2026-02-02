using System;
using CallbackDefs;
using Newtonsoft.Json;
using StageLib;
using UnityEngine;

public class BOSS_ControllerBase : EnemyControllerBase, IManagedUpdateBehavior
{
	private enum MainStatus
	{
		Idle = 0,
		Dead = 1,
		Hurt = 2,
		IdleWaitNet = 3
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
		MAX_ANIMATION_ID = 5
	}

	private MainStatus _mainStatus;

	private SubStatus _subStatus;

	private AnimationID _currentAnimationId;

	public int MoveSpeed = 3;

	public int DashSpeed = 5;

	private readonly int _hashHspd = Animator.StringToHash("fHspd");

	private new CollideBullet _collideBullet;

	private float _currentFrame;

	private int[] _animationHash;

	private Transform _modelTransform;

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
		base.AimTransform = OrangeBattleUtility.FindChildRecursive(_transform, "Bip001 Spine");
		_animator = GetComponentInChildren<Animator>();
		_modelTransform = OrangeBattleUtility.FindChildRecursive(_transform, "model", true);
		_collideBullet = base.AimTransform.gameObject.AddOrGetComponent<CollideBullet>();
		_animationHash = new int[5];
		_animationHash[0] = Animator.StringToHash("Idle");
		_animationHash[4] = Animator.StringToHash("Dead");
		_animationHash[3] = Animator.StringToHash("Hurt");
		_animationHash[1] = Animator.StringToHash("Skill0");
		_animationHash[2] = Animator.StringToHash("Skill1");
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
			_velocity.x = 0;
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
			if (_currentFrame > 1f && Controller.Collisions.below)
			{
				UpdateRandomState();
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
			default:
				throw new ArgumentOutOfRangeException();
			case SubStatus.Normal:
			case SubStatus.End:
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
		if (Activate)
		{
			base.transform.localPosition = Vector3.MoveTowards(base.transform.localPosition, Controller.LogicPosition.vec3, distanceDelta);
			_animator.SetFloat(_hashHspd, _velocity.vec3.x / (float)MoveSpeed);
		}
	}

	private void UpdateRandomState()
	{
		MainStatus mainStatus = (MainStatus)OrangeBattleUtility.Random(0, 1);
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
		MainStatus mainStatus = _mainStatus;
		if ((uint)mainStatus <= 3u)
		{
			if (_collideBullet.IsActivate)
			{
				_collideBullet.IsDestroy = true;
			}
			return;
		}
		throw new ArgumentOutOfRangeException();
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
		case MainStatus.Hurt:
			_currentAnimationId = AnimationID.ANI_HURT;
			break;
		default:
			throw new ArgumentOutOfRangeException();
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
		_modelTransform.eulerAngles = Vector3.up * base.direction * 90f;
		base.transform.position = pos;
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
}
