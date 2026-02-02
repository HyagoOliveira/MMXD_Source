using System;
using CallbackDefs;
using CodeStage.AntiCheat.ObscuredTypes;
using UnityEngine;

public class EM082_Controller : EnemyControllerBase, IManagedUpdateBehavior
{
	private enum WallSide
	{
		Up = 1,
		Down = 2,
		Left = 3,
		Right = 4,
		None = 5
	}

	private enum MainStatus
	{
		Idle = 0,
		Walk = 1,
		Atk = 2,
		Hurt = 3
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
		ANI_ATK = 2,
		ANI_Hurt = 3,
		MAX_ANIMATION_ID = 4
	}

	private MainStatus _mainStatus;

	private SubStatus _subStatus;

	private AnimationID _currentAnimationId;

	private float _currentFrame;

	private int[] _animationHash;

	private WallSide _wallside = WallSide.None;

	[SerializeField]
	private int WalkSpeed = 4000;

	private float NextPosX;

	private int WalkFrame;

	private Transform ModelOffest;

	private bool isPatrol;

	private bool _patrolIsLoop;

	private Vector3[] _patrolPaths = new Vector3[0];

	private int _patrolIndex;

	private float MaxPoint = float.NegativeInfinity;

	private float MinPoint = float.PositiveInfinity;

	private Transform ShootPoint;

	private float ShootFrame;

	private bool HasShot;

	private bool isAtking;

	private Vector3 NextPos
	{
		get
		{
			return _patrolPaths[_patrolIndex % _patrolPaths.Length];
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

	protected virtual void HashAnimation()
	{
		_animationHash = new int[4];
		_animationHash[0] = Animator.StringToHash("EM082@idle_loop");
		_animationHash[1] = Animator.StringToHash("EM082@walk_loop");
		_animationHash[2] = Animator.StringToHash("EM082@shot");
		_animationHash[3] = Animator.StringToHash("EM082@hurt_loop");
	}

	private void LoadParts(ref Transform[] childs)
	{
		ModelOffest = OrangeBattleUtility.FindChildRecursive(ref childs, "ModelOffest", true);
		ModelTransform = OrangeBattleUtility.FindChildRecursive(ref childs, "model", true);
		ShootPoint = OrangeBattleUtility.FindChildRecursive(ref childs, "ShootPoint", true);
		_animator = ModelTransform.GetComponent<Animator>();
		_collideBullet = OrangeBattleUtility.FindChildRecursive(ref childs, "Collider").gameObject.AddOrGetComponent<CollideBullet>();
	}

	protected override void Awake()
	{
		base.Awake();
		Transform[] childs = _transform.GetComponentsInChildren<Transform>(true);
		LoadParts(ref childs);
		HashAnimation();
		base.AimPoint = new Vector3(0f, 0.5f, 0f);
		OrangeBattleUtility.AddEnemyAutoAimSystem(_transform, out _enemyAutoAimSystem);
		_enemyAutoAimSystem.SetColliderType(EnemyAutoAimSystem.ColliderType.Box);
		_enemyAutoAimSystem.UpdateAimRange(20f, 8f);
		base.AllowAutoAim = true;
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
		case MainStatus.Idle:
			_velocity.x = 0;
			break;
		case MainStatus.Walk:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_velocity = VInt3.zero;
				WalkFrame = 0;
				if (Target == null)
				{
					Target = _enemyAutoAimSystem.GetClosetPlayer();
				}
				if ((bool)Target)
				{
					isAtking = true;
				}
				else
				{
					isAtking = false;
				}
				switch (_wallside)
				{
				case WallSide.Up:
				case WallSide.Down:
					if (isAtking)
					{
						TargetPos = Target.Controller.LogicPosition;
						if (Target._transform.position.x > _transform.position.x)
						{
							UpdateDirection(1);
						}
						else
						{
							UpdateDirection(-1);
						}
						if (isPatrol)
						{
							if (_transform.position.x < MinPoint)
							{
								UpdateDirection(1);
							}
							if (_transform.position.x > MaxPoint)
							{
								UpdateDirection(-1);
							}
						}
					}
					else if (Controller.Collisions.right || Controller.Collisions.left)
					{
						UpdateDirection(-base.direction);
					}
					else
					{
						if ((NextPos.x - _transform.position.x) * (float)base.direction < 0f)
						{
							_patrolIndex++;
						}
						if (NextPos.x > _transform.position.x)
						{
							UpdateDirection(1);
						}
						else
						{
							UpdateDirection(-1);
						}
					}
					break;
				case WallSide.Left:
				case WallSide.Right:
					if (isAtking)
					{
						TargetPos = Target.Controller.LogicPosition;
						if (Target._transform.position.y > _transform.position.y)
						{
							UpdateDirection(1);
						}
						else
						{
							UpdateDirection(-1);
						}
						if (isPatrol)
						{
							if (_transform.position.y < MinPoint)
							{
								UpdateDirection(1);
							}
							if (_transform.position.y > MaxPoint)
							{
								UpdateDirection(-1);
							}
						}
					}
					else if (Controller.Collisions.above || Controller.Collisions.below)
					{
						UpdateDirection(-base.direction);
					}
					else if (isPatrol)
					{
						if ((NextPos.y - _transform.position.y) * (float)base.direction < 0f)
						{
							_patrolIndex++;
						}
						if (NextPos.y > _transform.position.y)
						{
							UpdateDirection(1);
						}
						else
						{
							UpdateDirection(-1);
						}
					}
					break;
				}
				if (CheckMove())
				{
					UpdateDirection(-base.direction);
				}
				break;
			case SubStatus.Phase1:
				WalkFrame = 0;
				switch (_wallside)
				{
				case WallSide.Up:
				case WallSide.Down:
					_velocity = VInt3.signRight * base.direction * WalkSpeed;
					break;
				case WallSide.Left:
				case WallSide.Right:
					_velocity = VInt3.signUp * base.direction * WalkSpeed;
					break;
				}
				break;
			case SubStatus.Phase2:
				_velocity = VInt3.zero;
				break;
			}
			break;
		case MainStatus.Atk:
			switch (_wallside)
			{
			case WallSide.Up:
			case WallSide.Left:
				UpdateDirection(-1);
				break;
			case WallSide.Down:
			case WallSide.Right:
				UpdateDirection(1);
				break;
			}
			HasShot = false;
			ShootFrame = 0.4f;
			_velocity = VInt3.zero;
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
			if (_subStatus == SubStatus.Phase0)
			{
				_currentAnimationId = AnimationID.ANI_WALK;
				break;
			}
			return;
		case MainStatus.Atk:
			_currentAnimationId = AnimationID.ANI_ATK;
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
				isAtking = true;
			}
			SetStatus(MainStatus.Walk);
			break;
		case MainStatus.Walk:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (WalkFrame > 3)
				{
					SetStatus(MainStatus.Walk, SubStatus.Phase1);
				}
				WalkFrame++;
				break;
			case SubStatus.Phase1:
				if (WalkFrame > 3)
				{
					SetStatus(MainStatus.Walk, SubStatus.Phase2);
				}
				WalkFrame++;
				break;
			case SubStatus.Phase2:
				if (_currentFrame > 1f)
				{
					if ((bool)Target)
					{
						SetStatus(MainStatus.Atk);
					}
					else
					{
						SetStatus(MainStatus.Walk);
					}
				}
				break;
			}
			break;
		case MainStatus.Atk:
			if (!HasShot && _currentFrame > ShootFrame)
			{
				HasShot = true;
				Vector2 vector = Vector2.right * base.direction;
				Target = _enemyAutoAimSystem.GetClosetPlayer();
				if ((bool)Target)
				{
					vector = Target.Controller.GetRealCenterPos().xy() - ShootPoint.position.xy();
				}
				BulletBase.TryShotBullet(EnemyWeapons[1].BulletData, ShootPoint, vector.normalized, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
			}
			if (_currentFrame > 1f)
			{
				SetStatus(MainStatus.Walk);
			}
			break;
		}
	}

	private void UpdateDirection(int forceDirection = 0)
	{
		int direction2 = base.direction;
		if (forceDirection != 0)
		{
			base.direction = forceDirection;
		}
		else
		{
			switch (_wallside)
			{
			case WallSide.Up:
			case WallSide.Down:
				if (TargetPos.x > Controller.LogicPosition.x)
				{
					base.direction = 1;
				}
				else
				{
					base.direction = -1;
				}
				break;
			case WallSide.Left:
			case WallSide.Right:
				if (TargetPos.y > Controller.LogicPosition.y)
				{
					base.direction = 1;
				}
				else
				{
					base.direction = -1;
				}
				break;
			}
		}
		Vector3 euler = Vector3.zero;
		switch (_wallside)
		{
		case WallSide.Up:
			euler = new Vector3(180f, 90 * -base.direction, 0f);
			break;
		case WallSide.Down:
			euler = new Vector3(0f, 90 * base.direction, 0f);
			break;
		case WallSide.Left:
			euler = new Vector3(90 * -base.direction, 0f, -90f);
			break;
		case WallSide.Right:
			euler = new Vector3(90 * -base.direction, 0f, 90f);
			break;
		}
		ModelOffest.rotation = Quaternion.Euler(euler);
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
		bDeadShock = false;
		if (isActive)
		{
			isPatrol = false;
			_collideBullet.UpdateBulletData(EnemyWeapons[0].BulletData);
			_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			_collideBullet.Active(targetMask);
			_animator = ModelTransform.GetComponent<Animator>();
			base.AimPoint = new Vector3(0f, 0.15f, 0f);
			SetWallSide();
		}
		else
		{
			ModelOffest.eulerAngles = new Vector3(0f, 0f, 0f);
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
		base.transform.localScale = new Vector3(_transform.localScale.x, _transform.localScale.y, _transform.localScale.z);
		base.transform.position = pos;
	}

	public void SetWallSide(int wallside = 0)
	{
		Vector3 euler = Vector3.zero;
		switch ((WallSide)wallside)
		{
		case WallSide.Up:
			euler = new Vector3(180f, 90 * -base.direction, 0f);
			_wallside = (WallSide)wallside;
			Controller.Collider2D.offset = new Vector2(0f, -0.2f);
			break;
		case WallSide.Down:
			euler = new Vector3(0f, 90 * base.direction, 0f);
			_wallside = (WallSide)wallside;
			Controller.Collider2D.offset = new Vector2(0f, 0.2f);
			break;
		case WallSide.Left:
			euler = new Vector3(90 * -base.direction, 0f, -90f);
			_wallside = (WallSide)wallside;
			Controller.Collider2D.offset = new Vector2(0.2f, 0f);
			break;
		case WallSide.Right:
			euler = new Vector3(90 * -base.direction, 0f, 90f);
			_wallside = (WallSide)wallside;
			Controller.Collider2D.offset = new Vector2(-0.2f, 0f);
			break;
		default:
		{
			RaycastHit2D raycastHit2D = Physics2D.Raycast(_transform.position, Vector2.up, 3f, (int)Controller.collisionMask | (int)Controller.collisionMaskThrough);
			RaycastHit2D raycastHit2D2 = Physics2D.Raycast(_transform.position, Vector2.down, 3f, (int)Controller.collisionMask | (int)Controller.collisionMaskThrough);
			RaycastHit2D raycastHit2D3 = Physics2D.Raycast(_transform.position, Vector2.left, 3f, (int)Controller.collisionMask | (int)Controller.collisionMaskThrough);
			RaycastHit2D raycastHit2D4 = Physics2D.Raycast(_transform.position, Vector2.right, 3f, (int)Controller.collisionMask | (int)Controller.collisionMaskThrough);
			float num = 5f;
			if ((bool)raycastHit2D && num > raycastHit2D.distance)
			{
				num = raycastHit2D.distance;
				_wallside = WallSide.Up;
				euler = new Vector3(180f, 90 * -base.direction, 0f);
			}
			if ((bool)raycastHit2D2 && num > raycastHit2D2.distance)
			{
				num = raycastHit2D2.distance;
				_wallside = WallSide.Down;
				euler = new Vector3(0f, 90 * base.direction, 0f);
			}
			if ((bool)raycastHit2D3 && num > raycastHit2D3.distance)
			{
				num = raycastHit2D3.distance;
				_wallside = WallSide.Left;
				euler = new Vector3(90 * -base.direction, 0f, -90f);
			}
			if ((bool)raycastHit2D4 && num > raycastHit2D4.distance)
			{
				num = raycastHit2D4.distance;
				_wallside = WallSide.Right;
				euler = new Vector3(90 * -base.direction, 0f, 90f);
			}
			break;
		}
		}
		ModelOffest.rotation = Quaternion.Euler(euler);
	}

	public override ObscuredInt Hurt(HurtPassParam tHurtPassParam)
	{
		ObscuredInt obscuredInt = base.Hurt(tHurtPassParam);
		if ((int)obscuredInt <= 0)
		{
			Explosion();
			string pFxName = "fxuse_em_explosion";
			MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(pFxName, _transform, Quaternion.identity, new Vector3(0.5f, 0.5f, 0.5f), Array.Empty<object>());
		}
		return obscuredInt;
	}

	protected override void UpdateGravity()
	{
		if (IgnoreGravity)
		{
			return;
		}
		switch (_wallside)
		{
		case WallSide.Up:
		case WallSide.Down:
			if ((_velocity.y < 0 && Controller.Collisions.below) || (_velocity.y > 0 && Controller.Collisions.above))
			{
				_velocity.y = 0;
			}
			break;
		case WallSide.Left:
		case WallSide.Right:
			if ((_velocity.x < 0 && Controller.Collisions.left) || (_velocity.x > 0 && Controller.Collisions.right))
			{
				_velocity.x = 0;
			}
			break;
		}
		int num = 0;
		num = OrangeBattleUtility.FP_Gravity * GameLogicUpdateManager.g_fixFrameLenFP / 1000;
		switch (_wallside)
		{
		case WallSide.Up:
			_velocity.y -= num;
			break;
		case WallSide.Down:
			_velocity.y += num;
			break;
		case WallSide.Left:
			_velocity.x += num;
			break;
		case WallSide.Right:
			_velocity.x -= num;
			break;
		}
	}

	private bool CheckMove()
	{
		if (!DisableMoveFall)
		{
			return false;
		}
		VInt3 vInt = VInt3.one * WalkSpeed * 0.001f * GameLogicUpdateManager.m_fFrameLen * 1.5f;
		float num = 0f;
		Vector3 vector2;
		Vector3 vector;
		Vector3 zero;
		Vector3 vector3 = (vector2 = (vector = (zero = Vector3.zero)));
		zero = GetCenterPos();
		switch (_wallside)
		{
		case WallSide.Up:
		{
			Vector2 size = Controller.Collider2D.size;
			num = Mathf.Abs(vInt.vec3.x);
			vector3 = zero + new Vector3(size.x / 2f * (float)base.direction, size.y / 2f, 0f) + Vector3.right * base.direction * num;
			vector2 = Vector3.up;
			vector = Vector3.right * base.direction;
			break;
		}
		case WallSide.Down:
		{
			Vector2 size = Controller.Collider2D.size;
			num = Mathf.Abs(vInt.vec3.x);
			vector3 = zero + new Vector3(size.x / 2f * (float)base.direction, (0f - size.y) / 2f, 0f) + Vector3.right * base.direction * num;
			vector2 = Vector3.down;
			vector = Vector3.right * base.direction;
			break;
		}
		case WallSide.Left:
		{
			Vector2 size = Controller.Collider2D.size;
			num = Mathf.Abs(vInt.vec3.y);
			vector3 = zero + new Vector3((0f - size.x) / 2f, size.y / 2f * (float)base.direction, 0f) + Vector3.up * base.direction * num;
			vector2 = Vector3.left;
			vector = Vector3.up * base.direction;
			break;
		}
		case WallSide.Right:
		{
			Vector2 size = Controller.Collider2D.size;
			num = Mathf.Abs(vInt.vec3.y);
			vector3 = zero + new Vector3(size.x / 2f, size.y / 2f * (float)base.direction, 0f) + Vector3.up * base.direction * num;
			vector2 = Vector3.right;
			vector = Vector3.up * base.direction;
			break;
		}
		}
		if (!Physics2D.Raycast(vector3, vector2, 0.3f, Controller.collisionMask))
		{
			return true;
		}
		if ((bool)Physics2D.Raycast(zero, vector, num, Controller.collisionMask))
		{
			return true;
		}
		return false;
	}

	private Vector3 GetCenterPos()
	{
		Vector3 position = base.transform.position;
		position.x += Controller.Collider2D.offset.x * base.transform.localScale.x;
		position.y += Controller.Collider2D.offset.y * base.transform.localScale.y;
		return position;
	}

	public override void SetPatrolPath(bool isLoop, int nMoveSpeed, Vector3[] paths)
	{
		base.SetPatrolPath(isLoop, nMoveSpeed, paths);
		_patrolIsLoop = isLoop;
		_patrolPaths = new Vector3[paths.Length];
		for (int i = 0; i < paths.Length; i++)
		{
			_patrolPaths[i] = paths[i];
		}
		if (_patrolPaths.Length > 1)
		{
			isPatrol = true;
		}
		if (!isPatrol)
		{
			return;
		}
		_transform.position = new Vector3(_patrolPaths[0].x, _transform.position.y, _transform.position.z);
		Controller.LogicPosition = new VInt3(_transform.position);
		switch (_wallside)
		{
		case WallSide.Up:
		case WallSide.Down:
		{
			for (int k = 0; k < _patrolPaths.Length; k++)
			{
				if (_patrolPaths[k].x > MaxPoint)
				{
					MaxPoint = _patrolPaths[k].x;
				}
				if (_patrolPaths[k].x < MinPoint)
				{
					MinPoint = _patrolPaths[k].x;
				}
			}
			break;
		}
		case WallSide.Left:
		case WallSide.Right:
		{
			for (int j = 0; j < _patrolPaths.Length; j++)
			{
				if (_patrolPaths[j].y > MaxPoint)
				{
					MaxPoint = _patrolPaths[j].y;
				}
				if (_patrolPaths[j].y < MinPoint)
				{
					MinPoint = _patrolPaths[j].y;
				}
			}
			break;
		}
		}
	}
}
