using CallbackDefs;
using Newtonsoft.Json;
using UnityEngine;

public class EM061_Controller : EnemyControllerBase, IManagedUpdateBehavior
{
	private enum MainStatus
	{
		Idle = 0,
		Walk = 1,
		Roll = 2,
		Land = 3,
		Transform = 4,
		TransformR = 5,
		IdleWaitNet = 6
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
		ANI_IDLE = 0,
		ANI_WALK = 1,
		ANI_ROLL = 2,
		ANI_HURT = 3,
		ANI_LAND = 4,
		ANI_TRANSFORM = 5,
		ANI_TRANSFORMR = 6,
		MAX_ANIMATION_ID = 7
	}

	private MainStatus _mainStatus;

	private SubStatus _subStatus;

	private AnimationID _currentAnimationId;

	private float _currentFrame;

	private int[] _animationHash;

	protected bool _isRollActive;

	private bool _isBufferActive;

	private float StartBufferFrame;

	private float StopBufferFrame = 0.5f;

	private float StartRollFrame;

	private float StopFrame = 2.5f;

	public int WalkSpeed = 1750;

	public float RollDistance = 4f;

	public float RollFrame = 1f;

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
		Transform[] target = _transform.GetComponentsInChildren<Transform>(true);
		ModelTransform = OrangeBattleUtility.FindChildRecursive(ref target, "model", true);
		_animationHash = new int[7];
		_animationHash[0] = Animator.StringToHash("EM061@idle_loop");
		_animationHash[1] = Animator.StringToHash("EM061@walk_loop");
		_animationHash[2] = Animator.StringToHash("EM061@spawn_0_loop");
		_animationHash[3] = Animator.StringToHash("EM061@hurt_loop");
		_animationHash[4] = Animator.StringToHash("EM061@spawn_1_landing");
		_animationHash[5] = Animator.StringToHash("EM061@spawn_3_transform");
		_animationHash[6] = Animator.StringToHash("EM061@spawn_4_transform");
		_mainStatus = MainStatus.Idle;
		_subStatus = SubStatus.Phase0;
		base.AimPoint = new Vector3(0f, 0.5f, 0f);
		OrangeBattleUtility.AddEnemyAutoAimSystem(_transform, out _enemyAutoAimSystem);
		_enemyAutoAimSystem.SetColliderType(EnemyAutoAimSystem.ColliderType.Box);
		_enemyAutoAimSystem.UpdateAimRange(10f, 5f);
		FallDownSE = new string[2] { "EnemySE02", "em035_knot" };
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

	private void UpdateDirection(int forceDirection = 0)
	{
		if (!_isRollActive && !_isBufferActive)
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
		case MainStatus.Land:
			_velocity.x = 0;
			break;
		case MainStatus.Transform:
			_velocity.x = 0;
			break;
		case MainStatus.TransformR:
			_velocity.x = 0;
			break;
		}
		AiTimer.TimerStart();
		UpdateAnimation();
	}

	private void UpdateAnimation()
	{
		switch (_mainStatus)
		{
		case MainStatus.Idle:
			_currentAnimationId = AnimationID.ANI_IDLE;
			break;
		case MainStatus.Walk:
			_currentAnimationId = AnimationID.ANI_WALK;
			break;
		case MainStatus.Roll:
			_currentAnimationId = AnimationID.ANI_ROLL;
			break;
		case MainStatus.Land:
			_currentAnimationId = AnimationID.ANI_LAND;
			break;
		case MainStatus.Transform:
			_currentAnimationId = AnimationID.ANI_TRANSFORM;
			break;
		case MainStatus.TransformR:
			_currentAnimationId = AnimationID.ANI_TRANSFORMR;
			break;
		}
		_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
	}

	public override void LogicUpdate()
	{
		if (!Activate)
		{
			return;
		}
		base.LogicUpdate();
		_currentFrame = _animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
		MainStatus mainStatus = MainStatus.Idle;
		switch (_mainStatus)
		{
		case MainStatus.Idle:
			Target = _enemyAutoAimSystem.GetClosetPlayer();
			if ((bool)Target)
			{
				UpdateDirection();
				if (Mathf.Abs(Target._transform.position.x - _transform.position.x) < RollDistance)
				{
					mainStatus = MainStatus.TransformR;
				}
				else if (!CheckMoveFall(_velocity + VInt3.signRight * base.direction * WalkSpeed))
				{
					mainStatus = MainStatus.Walk;
				}
			}
			break;
		case MainStatus.Roll:
			if (!_isBufferActive && !_isRollActive)
			{
				StartRollFrame = _currentFrame;
			}
			if (_isBufferActive)
			{
				UpdateDirection();
				_velocity.x = base.direction * WalkSpeed * 3;
				if (_currentFrame - StartBufferFrame > StopBufferFrame)
				{
					SetStatus(MainStatus.Land);
					_isBufferActive = false;
				}
				break;
			}
			if ((bool)Target)
			{
				_isRollActive = (((Target._transform.position.x - _transform.position.x) * (float)base.direction > 0f) ? true : false);
			}
			if (_isRollActive)
			{
				_isRollActive = ((Mathf.Abs(_currentFrame - StartRollFrame) < StopFrame) ? true : false);
			}
			if (_isRollActive)
			{
				UpdateDirection();
				_velocity.x = base.direction * WalkSpeed * 3;
				if (CheckMoveFall(_velocity))
				{
					SetStatus(MainStatus.Land);
				}
			}
			else
			{
				_isBufferActive = true;
				StartBufferFrame = _currentFrame;
			}
			break;
		case MainStatus.Walk:
			if ((bool)Target)
			{
				UpdateDirection();
				_velocity.x = base.direction * WalkSpeed;
				if (_currentFrame > RollFrame)
				{
					mainStatus = MainStatus.TransformR;
				}
				if (CheckMoveFall(_velocity))
				{
					SetStatus(MainStatus.Idle);
				}
			}
			else
			{
				SetStatus(MainStatus.Idle);
			}
			break;
		case MainStatus.Land:
			SetStatus(MainStatus.Transform);
			break;
		case MainStatus.Transform:
			if (_currentFrame > 1f)
			{
				SetStatus(MainStatus.Idle);
			}
			else if (_currentFrame > 0.3f && IsInvincible)
			{
				IsInvincible = false;
			}
			break;
		case MainStatus.TransformR:
			if (_currentFrame > 1f)
			{
				PlaySE("EnemySE02", "em026_spikemaru");
				IsInvincible = true;
				SetStatus(MainStatus.Roll);
			}
			break;
		}
		if (mainStatus != 0 && CheckHost())
		{
			UploadEnemyStatus((int)mainStatus);
		}
	}

	public void UpdateFunc()
	{
		if (Activate)
		{
			base.transform.localPosition = Vector3.MoveTowards(base.transform.localPosition, Controller.LogicPosition.vec3, distanceDelta);
		}
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
		}
		else
		{
			_collideBullet.BackToPool();
			_isRollActive = false;
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
}
