using System;
using CallbackDefs;
using CodeStage.AntiCheat.ObscuredTypes;
using UnityEngine;

public class EM092_Controller : EnemyControllerBase, IManagedUpdateBehavior
{
	private enum MainStatus
	{
		Idle = 0,
		Jump = 1,
		Fall = 2,
		Walk = 3,
		Run = 4,
		Hurt = 5
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
		ANI_JUMP = 1,
		ANI_FALL = 2,
		ANI_LAND = 3,
		ANI_WALK = 4,
		ANI_RUN = 5,
		ANI_Hurt = 6,
		MAX_ANIMATION_ID = 7
	}

	private MainStatus _mainStatus;

	private SubStatus _subStatus;

	private AnimationID _currentAnimationId;

	private float _currentFrame;

	private int[] _animationHash;

	private BS013_Controller _parentController;

	private int WalkSpeed = 4000;

	private bool isClimbing;

	private int nextdirection;

	private OrangeTimer RunTimer;

	private float LimitY;

	private BoxCollider2D boxcollider;

	private BoxCollider2D enemycollider;

	private void OnEnable()
	{
		MonoBehaviourSingleton<UpdateManager>.Instance.AddUpdate(this);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		MonoBehaviourSingleton<UpdateManager>.Instance.RemoveUpdate(this);
	}

	protected virtual void HashAnimation()
	{
		_animationHash = new int[7];
		_animationHash[0] = Animator.StringToHash("EM092@stand_loop");
		_animationHash[1] = Animator.StringToHash("EM092@jump_loop");
		_animationHash[2] = Animator.StringToHash("EM092@fall_loop");
		_animationHash[3] = Animator.StringToHash("EM092@landing");
		_animationHash[4] = Animator.StringToHash("EM092@walk_loop");
		_animationHash[5] = Animator.StringToHash("EM092@run_loop");
	}

	protected override void Awake()
	{
		base.Awake();
		_collideBullet = base.gameObject.AddOrGetComponent<CollideBullet>();
		Transform[] target = _transform.GetComponentsInChildren<Transform>(true);
		ModelTransform = OrangeBattleUtility.FindChildRecursive(ref target, "model", true);
		boxcollider = base.transform.gameObject.GetComponent<BoxCollider2D>();
		_animator = ModelTransform.GetComponent<Animator>();
		_collideBullet = OrangeBattleUtility.FindChildRecursive(ref target, "Collider").gameObject.AddOrGetComponent<CollideBullet>();
		enemycollider = _collideBullet.gameObject.GetComponent<BoxCollider2D>();
		HashAnimation();
		base.AimPoint = new Vector3(0f, 0f, 0f);
		OrangeBattleUtility.AddEnemyAutoAimSystem(_transform, out _enemyAutoAimSystem);
		_enemyAutoAimSystem.SetColliderType(EnemyAutoAimSystem.ColliderType.Box);
		_enemyAutoAimSystem.UpdateAimRange(0f, 0f);
		RunTimer = OrangeTimerManager.GetTimer();
		base.AllowAutoAim = false;
	}

	protected override void Start()
	{
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_em_explosion", 10);
	}

	public override void UpdateStatus(int nSet, string smsg, Callback tCB = null)
	{
		bWaitNetStatus = false;
	}

	private void SetStatus(MainStatus mainStatus, SubStatus subStatus = SubStatus.Phase0)
	{
		_mainStatus = mainStatus;
		_subStatus = subStatus;
		switch (_mainStatus)
		{
		case MainStatus.Walk:
			if (isClimbing)
			{
				_velocity.y = WalkSpeed;
				break;
			}
			_velocity.x = WalkSpeed * base.direction;
			_velocity.y = 0;
			break;
		case MainStatus.Run:
			RunTimer.TimerStart();
			if (isClimbing)
			{
				_velocity.y = (int)((float)WalkSpeed * 1.5f);
				break;
			}
			_velocity.x = (int)((float)WalkSpeed * 1.5f * (float)base.direction);
			_velocity.y = 0;
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
		case MainStatus.Fall:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_FALL;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_LAND;
				break;
			}
			break;
		case MainStatus.Walk:
			_currentAnimationId = AnimationID.ANI_WALK;
			break;
		case MainStatus.Run:
			_currentAnimationId = AnimationID.ANI_RUN;
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
		case MainStatus.Fall:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (Controller.Collisions.below)
				{
					SetStatus(MainStatus.Fall, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if (_currentFrame > 1f)
				{
					isClimbing = false;
					base.direction = nextdirection;
					UpdateDirection(base.direction);
					SetStatus(MainStatus.Run);
				}
				if (Controller.Collisions.right || Controller.Collisions.left)
				{
					_velocity.x = -_velocity.x;
				}
				break;
			}
			break;
		case MainStatus.Walk:
			if (Controller.Collisions.right || Controller.Collisions.left)
			{
				if (!isClimbing)
				{
					IgnoreGravity = true;
					isClimbing = true;
					_velocity.y = WalkSpeed;
				}
				if (Controller.Collisions.right)
				{
					SetColliderOffset(new Vector2(-0.4f, 0f));
				}
				else if (Controller.Collisions.left)
				{
					SetColliderOffset(new Vector2(0.4f, 0f));
				}
			}
			if (isClimbing && ModelTransform.eulerAngles.x < 90f)
			{
				ModelTransform.eulerAngles += new Vector3(15f, 0f, 0f);
			}
			break;
		case MainStatus.Run:
			if (RunTimer.GetMillisecond() > 500)
			{
				RunTimer.TimerStop();
				SetStatus(MainStatus.Walk);
			}
			if (Controller.Collisions.right || Controller.Collisions.left)
			{
				if (!isClimbing)
				{
					IgnoreGravity = true;
					isClimbing = true;
				}
				if (Controller.Collisions.right)
				{
					SetColliderOffset(new Vector2(-0.4f, 0f));
				}
				else if (Controller.Collisions.left)
				{
					SetColliderOffset(new Vector2(0.4f, 0f));
				}
				SetStatus(MainStatus.Walk);
			}
			break;
		}
		if (_transform.position.y > LimitY)
		{
			HurtPassParam hurtPassParam = new HurtPassParam();
			hurtPassParam.dmg = MaxHp;
			DeadBehavior(ref hurtPassParam);
		}
	}

	private void UpdateDirection(int forceDirection = 0)
	{
		if (forceDirection != 0)
		{
			base.direction = forceDirection;
		}
		ModelTransform.eulerAngles = new Vector3(0f, -base.direction * 90, 0f);
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
		bDeadShock = false;
		if (isActive)
		{
			SetParameter(1, OrangeBattleUtility.Random(0, 2) * 2 - 1, new VInt3(600, 7000, 0));
			IgnoreGravity = false;
			_collideBullet.UpdateBulletData(EnemyWeapons[0].BulletData);
			_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			_collideBullet.Active(targetMask);
		}
		else
		{
			SetColliderOffset(new Vector2(0f, 0.4f));
			ModelTransform.eulerAngles = new Vector3(0f, -90f, 0f);
			_collideBullet.BackToPool();
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

	public void SetParameter(int borndirection, int walkdirection, VInt3 velocity)
	{
		_velocity = velocity;
		base.direction = borndirection;
		nextdirection = walkdirection;
		LimitY = _transform.position.y + 2f;
		_transform.position = new Vector3(_transform.position.x, _transform.position.y, 0f);
		SetStatus(MainStatus.Fall);
	}

	public override ObscuredInt Hurt(HurtPassParam tHurtPassParam)
	{
		ObscuredInt obscuredInt = base.Hurt(tHurtPassParam);
		if ((int)obscuredInt <= 0)
		{
			MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_em_explosion", _transform, Quaternion.identity, new Vector3(0.5f, 0.5f, 0.5f), Array.Empty<object>());
		}
		return obscuredInt;
	}

	public void SetParent(BS013_Controller parent)
	{
		_parentController = parent;
	}

	protected override void UpdateGravity()
	{
		if (!IgnoreGravity)
		{
			if ((_velocity.y < 0 && Controller.Collisions.below) || (_velocity.y > 0 && Controller.Collisions.above))
			{
				_velocity.y = 0;
			}
			_velocity.y += OrangeBattleUtility.FP_Gravity * GameLogicUpdateManager.g_fixFrameLenFP / 1000;
			_velocity.y = IntMath.Sign(_velocity.y) * IntMath.Min(IntMath.Abs(_velocity.y), IntMath.Abs(_maxGravity.i));
		}
	}

	private void SetColliderOffset(Vector2 offset)
	{
		boxcollider.offset = offset;
		enemycollider.offset = offset;
	}
}
