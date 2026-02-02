using UnityEngine;

public class EM053_Controller : EnemyControllerBase, IManagedUpdateBehavior
{
	private enum MainStatus
	{
		Idle = 0,
		Spin = 1,
		IdleWaitNet = 2
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
		ANI_PUNCH = 2,
		ANI_HURT = 3,
		MAX_ANIMATION_ID = 4
	}

	public int MoveSpeed = 3000;

	public int RotateSpeed = 300;

	private float ShiftRange = 4f;

	private Animator _mainAnimator;

	private Animator _subAnimator;

	private readonly int _hashSpike = Animator.StringToHash("fSpike");

	private Transform _mainObj;

	private Transform _subObj;

	private CollideBullet _mainCollideBullet;

	private CollideBullet _subCollideBullet;

	private MainStatus _mainStatus;

	private SubStatus _subStatus;

	private AnimationID _currentAnimationId;

	private float _currentFrame;

	private int[] _animationHash;

	private int _fromWaypointIndexPrev;

	private Vector3 targetPos;

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
		IsInvincible = true;
		StageObjParam[] componentsInChildren = GetComponentsInChildren<StageObjParam>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].tLinkSOB = null;
		}
		_mainAnimator = GetComponentInChildren<Animator>();
		Transform[] target = _transform.GetComponentsInChildren<Transform>(true);
		_mainObj = OrangeBattleUtility.FindChildRecursive(ref target, "mainObj", true);
		_subObj = OrangeBattleUtility.FindChildRecursive(ref target, "subObj", true);
		_subCollideBullet = _mainObj.gameObject.AddOrGetComponent<CollideBullet>();
		_mainCollideBullet = _subObj.gameObject.AddOrGetComponent<CollideBullet>();
		base.AllowAutoAim = false;
		_globalWaypoints = new float[2];
		_easeSpeed = 3f;
	}

	public override void LogicUpdate()
	{
		if (!Activate)
		{
			return;
		}
		base.LogicUpdate();
		if ((_velocity.x > 0 && Controller.Collisions.right) || (_velocity.x < 0 && Controller.Collisions.left) || CheckMoveFall(_velocity))
		{
			_velocity = new VInt3(-_velocity.x, 0, 0);
			PlaySE("EnemySE", 5, visible);
		}
		if (_fromWaypointIndex != _fromWaypointIndexPrev && _fromWaypointIndex == 0)
		{
			RaycastHit2D raycastHit2D = OrangeBattleUtility.RaycastIgnoreSelf(_subObj.position, Vector2.up, ShiftRange, Controller.collisionMask, _transform);
			if ((bool)raycastHit2D)
			{
				_globalWaypoints[1] = _subObj.position.y + raycastHit2D.distance;
			}
			else
			{
				_globalWaypoints[1] = _subObj.position.y + ShiftRange;
			}
		}
		if (Controller.Collisions.below && !Controller.CollisionsOld.below)
		{
			_globalWaypoints[0] = _transform.position.y;
			_globalWaypoints[1] = _transform.position.y + ShiftRange;
		}
		_fromWaypointIndexPrev = _fromWaypointIndex;
		_mainObj.Rotate(Vector3.up * RotateSpeed * GameLogicUpdateManager.g_fixFrameLenFP.scalar);
		_subObj.Rotate(Vector3.down * RotateSpeed * GameLogicUpdateManager.g_fixFrameLenFP.scalar);
		targetPos = _subObj.position;
		targetPos.y = CalculateVerticalMovement(true);
	}

	public void UpdateFunc()
	{
		if (Activate)
		{
			base.transform.localPosition = Vector3.MoveTowards(base.transform.localPosition, Controller.LogicPosition.vec3, distanceDelta);
			_subObj.position = Vector3.MoveTowards(_subObj.position, new Vector3(_transform.position.x, targetPos.y, 0f), distanceDelta);
		}
	}

	public override void SetActive(bool isActive)
	{
		base.SetActive(isActive);
		_mainAnimator.enabled = isActive;
		if (isActive)
		{
			_subCollideBullet.UpdateBulletData(EnemyWeapons[0].BulletData);
			_subCollideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			_subCollideBullet.Active(targetMask);
			_mainCollideBullet.UpdateBulletData(EnemyWeapons[0].BulletData);
			_mainCollideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			_mainCollideBullet.Active(targetMask);
			_globalWaypoints[0] = _subObj.position.y;
			_globalWaypoints[1] = _subObj.position.y + ShiftRange;
			_velocity.x = MoveSpeed * base.direction;
		}
		else
		{
			_subCollideBullet.BackToPool();
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
