using CallbackDefs;
using UnityEngine;

public class EM165_Controller : EnemyControllerBase, IManagedUpdateBehavior
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
		Atk = 1,
		Hurt = 2
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

	private WallSide _wallside = WallSide.None;

	private BoxCollider2D boxcollider;

	private BoxCollider2D enemycollider;

	[SerializeField]
	private Transform ShootPoint;

	[SerializeField]
	private float ShootTime = 2f;

	private int ShootFrame;

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
		Transform[] target = _transform.GetComponentsInChildren<Transform>(true);
		ModelTransform = OrangeBattleUtility.FindChildRecursive(ref target, "model", true);
		boxcollider = base.gameObject.GetComponent<BoxCollider2D>();
		_animator = ModelTransform.GetComponent<Animator>();
		_collideBullet = OrangeBattleUtility.FindChildRecursive(ref target, "Collider").gameObject.AddOrGetComponent<CollideBullet>();
		if (ShootPoint == null)
		{
			ShootPoint = OrangeBattleUtility.FindChildRecursive(ref target, "ShootPoint");
		}
		enemycollider = _collideBullet.gameObject.GetComponent<BoxCollider2D>();
		HashAnimation();
		base.AimPoint = new Vector3(0f, 0.5f, 0f);
		OrangeBattleUtility.AddEnemyAutoAimSystem(_transform, out _enemyAutoAimSystem);
		_enemyAutoAimSystem.SetColliderType(EnemyAutoAimSystem.ColliderType.Box);
		_enemyAutoAimSystem.UpdateAimRange(20f, 8f);
		base.AllowAutoAim = true;
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
		case MainStatus.Atk:
			_velocity = VInt3.zero;
			ShootFrame = GameLogicUpdateManager.GameFrame + (int)(ShootTime * 20f);
			BulletBase.TryShotBullet(EnemyWeapons[1].BulletData, ShootPoint, GetTargetPos() - ShootPoint.position, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
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
		case MainStatus.Atk:
			_currentAnimationId = AnimationID.ANI_IDLE;
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
				SetStatus(MainStatus.Atk);
			}
			break;
		case MainStatus.Atk:
			if (!Target)
			{
				SetStatus(MainStatus.Idle);
			}
			else if (GameLogicUpdateManager.GameFrame > ShootFrame)
			{
				SetStatus(MainStatus.Atk);
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
		ModelTransform.rotation = Quaternion.Euler(euler);
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
			_collideBullet.UpdateBulletData(EnemyWeapons[0].BulletData);
			_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			_collideBullet.Active(targetMask);
			ModelTransform.localScale = new Vector3(5f, 5f, 5f);
			_animator = ModelTransform.GetComponent<Animator>();
			base.AimPoint = new Vector3(0f, 0.15f, 0f);
			base.AllowAutoAim = true;
			IgnoreGravity = false;
			SetWallSide();
		}
		else
		{
			SetColliderOffset(new Vector2(0f, 0.13f));
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
				Controller.Collider2D.offset = new Vector2(0f, -0.2f);
			}
			if ((bool)raycastHit2D2 && num > raycastHit2D2.distance)
			{
				num = raycastHit2D2.distance;
				_wallside = WallSide.Down;
				euler = new Vector3(0f, 90 * base.direction, 0f);
				Controller.Collider2D.offset = new Vector2(0f, 0.2f);
			}
			if ((bool)raycastHit2D3 && num > raycastHit2D3.distance)
			{
				num = raycastHit2D3.distance;
				_wallside = WallSide.Left;
				euler = new Vector3(90 * -base.direction, 0f, -90f);
				Controller.Collider2D.offset = new Vector2(0.2f, 0f);
			}
			if ((bool)raycastHit2D4 && num > raycastHit2D4.distance)
			{
				num = raycastHit2D4.distance;
				_wallside = WallSide.Right;
				euler = new Vector3(90 * -base.direction, 0f, 90f);
				Controller.Collider2D.offset = new Vector2(-0.2f, 0f);
			}
			break;
		}
		}
		ModelTransform.rotation = Quaternion.Euler(euler);
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

	private void SetColliderOffset(Vector2 offset)
	{
		boxcollider.offset = offset;
		enemycollider.offset = offset;
	}

	private bool CheckMove()
	{
		if (!DisableMoveFall)
		{
			return false;
		}
		VInt3 vInt = _velocity * GameLogicUpdateManager.m_fFrameLen;
		Vector3 vector;
		Vector3 vector2 = (vector = Vector3.zero);
		switch (_wallside)
		{
		case WallSide.Up:
		{
			Vector3 centerPos4 = GetCenterPos();
			Vector2 size = Controller.Collider2D.size;
			float num = Mathf.Abs(vInt.vec3.x);
			vector2 = centerPos4 + Vector3.right * base.direction * num;
			vector = Vector3.up;
			break;
		}
		case WallSide.Down:
		{
			Vector3 centerPos3 = GetCenterPos();
			Vector2 size = Controller.Collider2D.size;
			float num = Mathf.Abs(vInt.vec3.x);
			vector2 = centerPos3 + new Vector3(size.x / 2f * (float)base.direction, (0f - size.y) / 2f, 0f) + Vector3.right * base.direction * num;
			vector = Vector3.down;
			break;
		}
		case WallSide.Left:
		{
			Vector3 centerPos2 = GetCenterPos();
			Vector2 size = Controller.Collider2D.size;
			float num = Mathf.Abs(vInt.vec3.y);
			vector2 = centerPos2 + new Vector3((0f - size.x) / 2f, size.y / 2f * (float)base.direction, 0f) + Vector3.up * base.direction * num;
			vector = Vector3.left;
			break;
		}
		case WallSide.Right:
		{
			Vector3 centerPos = GetCenterPos();
			Vector2 size = Controller.Collider2D.size;
			float num = Mathf.Abs(vInt.vec3.y);
			vector2 = centerPos + new Vector3(size.x / 2f, size.y / 2f * (float)base.direction, 0f) + Vector3.up * base.direction * num;
			vector = Vector3.right;
			break;
		}
		}
		if (!Physics2D.Raycast(vector2, vector, 0.3f, Controller.collisionMask))
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

	private Vector3 GetTargetPos(bool realcenter = true)
	{
		if (!Target)
		{
			Target = _enemyAutoAimSystem.GetClosetPlayer();
		}
		if ((bool)Target)
		{
			if (realcenter)
			{
				TargetPos = new VInt3(Target.Controller.GetRealCenterPos());
			}
			else
			{
				TargetPos = new VInt3(Target.GetTargetPoint());
			}
			return TargetPos.vec3;
		}
		return _transform.position + Vector3.down * 3f;
	}
}
