using CallbackDefs;
using StageLib;
using UnityEngine;

public class EM041_Controller : EnemyControllerBase, IManagedUpdateBehavior
{
	protected enum MainStatus
	{
		Idle = 0,
		Walk = 1,
		Punch = 2,
		IdleWaitNet = 3
	}

	protected enum SubStatus
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
		ANI_PUNCH = 2,
		ANI_HURT = 3,
		MAX_ANIMATION_ID = 4
	}

	protected MainStatus _mainStatus;

	protected SubStatus _subStatus;

	protected AnimationID _currentAnimationId;

	protected float _currentFrame;

	protected int[] _animationHash;

	protected readonly int _hashHspd = Animator.StringToHash("fHspd");

	protected CollideBullet _punchCollideBullet;

	protected bool _isPunchActive;

	public int WalkSpeed = 1500;

	public float punchDistance = 2.8f;

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
		Transform transform = OrangeBattleUtility.FindChildRecursive(ref target, "punchCollider", true);
		_punchCollideBullet = transform.gameObject.AddOrGetComponent<CollideBullet>();
		_animationHash = new int[4];
		_animationHash[0] = Animator.StringToHash("EM041@idle_loop");
		_animationHash[1] = Animator.StringToHash("EM041@run_loop");
		_animationHash[2] = Animator.StringToHash("EM041@skill_01");
		_animationHash[3] = Animator.StringToHash("EM041@hurt_loop");
		_mainStatus = MainStatus.Idle;
		_subStatus = SubStatus.Phase0;
		base.AimPoint = new Vector3(0f, 1f, 0f);
		OrangeBattleUtility.AddEnemyAutoAimSystem(_transform, out _enemyAutoAimSystem);
		_enemyAutoAimSystem.SetColliderType(EnemyAutoAimSystem.ColliderType.Box);
		_enemyAutoAimSystem.UpdateAimRange(10f, 5f);
		FallDownSE = new string[2] { "EnemySE", "em007_preon02" };
	}

	public override void UpdateStatus(int nSet, string smsg, Callback tCB = null)
	{
		SetStatus((MainStatus)nSet);
	}

	private void UpdateDirection(int forceDirection = 0)
	{
		if (forceDirection != 0)
		{
			base.direction = forceDirection;
		}
		else if (StageUpdate.gbIsNetGame)
		{
			if (Target.Controller.LogicPosition.x > Controller.LogicPosition.x)
			{
				base.direction = 1;
			}
			else
			{
				base.direction = -1;
			}
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

	private void SetStatus(MainStatus mainStatus, SubStatus subStatus = SubStatus.Phase0)
	{
		_mainStatus = mainStatus;
		_subStatus = subStatus;
		switch (_mainStatus)
		{
		case MainStatus.Idle:
			_velocity.x = 0;
			break;
		case MainStatus.Punch:
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
		case MainStatus.Punch:
			_currentAnimationId = AnimationID.ANI_PUNCH;
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
		switch (_mainStatus)
		{
		case MainStatus.Idle:
			Target = _enemyAutoAimSystem.GetClosetPlayer();
			if ((bool)Target)
			{
				UpdateDirection();
				if (Mathf.Abs(Target._transform.position.x - _transform.position.x) < punchDistance)
				{
					SetStatus(MainStatus.Punch);
				}
				else if (!CheckMoveFall(_velocity + VInt3.signRight * base.direction * WalkSpeed))
				{
					SetStatus(MainStatus.Walk);
				}
			}
			break;
		case MainStatus.Punch:
			if (_currentFrame >= 1f)
			{
				SetStatus(MainStatus.Idle);
			}
			else if (_currentFrame > 0.6f)
			{
				if (_isPunchActive)
				{
					_punchCollideBullet.BackToPool();
					_isPunchActive = false;
				}
			}
			else if (_currentFrame > 0.16f && !_isPunchActive)
			{
				_punchCollideBullet.Active(targetMask);
				_isPunchActive = true;
			}
			break;
		case MainStatus.Walk:
			Target = _enemyAutoAimSystem.GetClosetPlayer();
			if ((bool)Target)
			{
				UpdateDirection();
				_velocity.x = base.direction * WalkSpeed;
				if (Mathf.Abs(Target._transform.position.x - _transform.position.x) < punchDistance)
				{
					SetStatus(MainStatus.Punch);
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
		}
	}

	public void UpdateFunc()
	{
		if (Activate)
		{
			base.transform.localPosition = Vector3.MoveTowards(base.transform.localPosition, Controller.LogicPosition.vec3, distanceDelta);
			_animator.SetFloat(_hashHspd, (float)Mathf.Abs(_velocity.x) * 0.001f);
		}
	}

	public override void SetActive(bool isActive)
	{
		base.SetActive(isActive);
		_animator.enabled = isActive;
		if (isActive)
		{
			_punchCollideBullet.UpdateBulletData(EnemyWeapons[0].BulletData);
			_punchCollideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			_punchCollideBullet.transform.localEulerAngles = new Vector3(90f, 90f, 0f);
			_collideBullet.UpdateBulletData(EnemyWeapons[1].BulletData);
			_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			_collideBullet.Active(targetMask);
		}
		else
		{
			_collideBullet.BackToPool();
			_punchCollideBullet.BackToPool();
			_isPunchActive = false;
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
