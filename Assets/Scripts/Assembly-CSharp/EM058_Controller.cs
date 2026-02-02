using UnityEngine;

public class EM058_Controller : EnemyControllerBase, IManagedUpdateBehavior
{
	public enum MainStatus
	{
		Idle = 0,
		Walk = 1,
		Roll = 2,
		Open = 3,
		Morph = 4,
		MAX_STATUS = 5
	}

	public enum SubStatus
	{
		Phase0 = 0,
		Phase1 = 1,
		Phase2 = 2,
		Phase3 = 3,
		Phase4 = 4
	}

	public enum AnimationID
	{
		ANI_IDLE = 0,
		ANI_WALK = 1,
		ANI_OPEN = 2,
		ANI_CLOSE = 3,
		ANI_ROLL = 4,
		ANI_MORPH = 5,
		MAX_ANIMATION_ID = 6
	}

	public readonly int walkSpeedHash = Animator.StringToHash("fWalkSpeed");

	protected int[] _animationHash;

	protected MainStatus _mainStatus = MainStatus.Roll;

	protected SubStatus _subStatus;

	protected AnimationID _currentAnimationId;

	protected float _currentFrame;

	protected static int _walkSpeed = 1500;

	protected static int _rollSpeed = 2000;

	protected Vector2 _normalOffset = new Vector2(0f, 0.8f);

	protected Vector2 _normalSize = new Vector2(1f, 1.6f);

	protected Vector2 _ballOffset = new Vector2(0f, 0.4f);

	protected Vector2 _ballSize = new Vector2(1f, 0.8f);

	protected OrangeTimer _rollRecycleTimer;

	protected void OnEnable()
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
		_rollRecycleTimer = OrangeTimerManager.GetTimer();
		Transform[] target = base.transform.GetComponentsInChildren<Transform>(true);
		ModelTransform = OrangeBattleUtility.FindChildRecursive(ref target, "model", true);
		_animationHash = new int[6];
		base.AimTransform = OrangeBattleUtility.FindChildRecursive(ref target, "body_TR", true);
		_collideBullet = OrangeBattleUtility.FindChildRecursive(ref target, "CollideBullet", true).gameObject.AddOrGetComponent<CollideBullet>();
		_animator = GetComponentInChildren<Animator>();
		_animationHash[0] = Animator.StringToHash("EM058@idle_loop");
		_animationHash[1] = Animator.StringToHash("EM058@run_loop");
		_animationHash[4] = Animator.StringToHash("EM058@roll_loop");
		_animationHash[2] = Animator.StringToHash("EM058@roll_change");
		_animationHash[5] = Animator.StringToHash("EM058@change");
		_animationHash[3] = Animator.StringToHash("EM058@roll_change");
	}

	public override void UpdateEnemyID(int id)
	{
		base.UpdateEnemyID(id);
		if (!_enemyAutoAimSystem)
		{
			OrangeBattleUtility.AddEnemyAutoAimSystem(_transform, out _enemyAutoAimSystem);
		}
		_enemyAutoAimSystem.UpdateAimRange(2f);
	}

	public void UpdateFunc()
	{
		if (Activate)
		{
			base.transform.localPosition = Vector3.MoveTowards(base.transform.localPosition, Controller.LogicPosition.vec3, distanceDelta);
		}
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
		case MainStatus.Walk:
			if ((Controller.Collisions.left && base.direction == -1) || (Controller.Collisions.right && base.direction == 1))
			{
				base.direction = -base.direction;
				ModelTransform.localEulerAngles = new Vector3(0f, 90 * base.direction, 0f);
				_velocity.x = -_velocity.x;
			}
			if (!Controller.BelowInBypassRange)
			{
				SetStatus(MainStatus.Roll);
			}
			break;
		case MainStatus.Roll:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (_rollRecycleTimer.GetMillisecond() > 10000)
				{
					bNeedDead = true;
				}
				if ((bool)Controller.BelowInBypassRange)
				{
					SetStatus(_mainStatus, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if (_currentFrame >= 3f && (bool)Controller.BelowInBypassRange)
				{
					SetStatus(MainStatus.Open);
				}
				break;
			}
			break;
		case MainStatus.Open:
			if (_currentFrame >= 1f)
			{
				SetStatus(MainStatus.Morph);
			}
			break;
		case MainStatus.Morph:
			if (_currentFrame >= 1f)
			{
				SetStatus(MainStatus.Walk);
			}
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
		ModelTransform.localEulerAngles = new Vector3(0f, 90 * base.direction, 0f);
		_animator.SetFloat(walkSpeedHash, (float)_walkSpeed * 0.001f);
		base.transform.position = pos;
	}

	public override void BackToPool()
	{
		base.BackToPool();
		_rollRecycleTimer.TimerStop();
	}

	protected void SetStatus(MainStatus mainStatus, SubStatus subStatus = SubStatus.Phase0)
	{
		_mainStatus = mainStatus;
		_subStatus = subStatus;
		switch (_mainStatus)
		{
		case MainStatus.Idle:
			_velocity.x = 0;
			break;
		case MainStatus.Walk:
			_velocity.x = base.direction * _walkSpeed;
			break;
		case MainStatus.Roll:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
			{
				Vector2 offset = new Vector2(_ballOffset.x * (float)base.direction, _ballOffset.y);
				Controller.SetColliderBox(ref offset, ref _ballSize);
				_rollRecycleTimer.TimerStart();
				break;
			}
			case SubStatus.Phase1:
				_rollRecycleTimer.TimerStop();
				_velocity.x = base.direction * _rollSpeed;
				break;
			}
			break;
		case MainStatus.Open:
			_velocity.x = 0;
			break;
		case MainStatus.Morph:
			_velocity.x = 0;
			Controller.SetColliderBox(ref _normalOffset, ref _normalSize);
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
		case MainStatus.Open:
			_currentAnimationId = AnimationID.ANI_OPEN;
			break;
		case MainStatus.Morph:
			_currentAnimationId = AnimationID.ANI_MORPH;
			break;
		}
		_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
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
			SetStatus(MainStatus.Roll);
		}
		else
		{
			_collideBullet.BackToPool();
		}
	}
}
