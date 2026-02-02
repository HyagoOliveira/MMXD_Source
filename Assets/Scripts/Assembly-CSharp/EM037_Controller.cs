using System;
using UnityEngine;

public class EM037_Controller : EnemyControllerBase, IManagedUpdateBehavior
{
	private enum MainStatus
	{
		Shoot = 0,
		Turn = 1,
		IdleWaitNet = 2
	}

	private enum SubStatus
	{
		Angle0 = 0,
		Angle1 = 1,
		Angle2 = 2,
		Angle3 = 3,
		MAX_SUBSTATUS = 4
	}

	public enum AnimationID
	{
		ANI_SHOOT0 = 0,
		ANI_SHOOT1 = 1,
		ANI_SHOOT2 = 2,
		ANI_SHOOT3 = 3,
		ANI_TURN0 = 4,
		ANI_TURN1 = 5,
		ANI_TURN2 = 6,
		ANI_TURN3 = 7,
		MAX_ANIMATION_ID = 8
	}

	private MainStatus _mainStatus;

	private SubStatus _subStatus;

	private AnimationID _currentAnimationId;

	private float _currentFrame;

	private int[] _animationHash;

	private Transform[] _shootPointTransforms;

	private readonly Vector3 _upLeft = new Vector3(-1f, 1f, 0f);

	private readonly Vector3 _upRight = new Vector3(1f, 1f, 0f);

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
		_animationHash = new int[8];
		_animationHash[4] = Animator.StringToHash("EM037@turn0");
		_animationHash[5] = Animator.StringToHash("EM037@turn1");
		_animationHash[6] = Animator.StringToHash("EM037@turn2");
		_animationHash[7] = Animator.StringToHash("EM037@turn3");
		_animationHash[0] = Animator.StringToHash("EM037@shoot0");
		_animationHash[1] = Animator.StringToHash("EM037@shoot1");
		_animationHash[2] = Animator.StringToHash("EM037@shoot2");
		_animationHash[3] = Animator.StringToHash("EM037@shoot3");
		_shootPointTransforms = new Transform[4];
		Transform[] target = _transform.GetComponentsInChildren<Transform>(true);
		_shootPointTransforms[0] = OrangeBattleUtility.FindChildRecursive(ref target, "ShootPoint0", true);
		_shootPointTransforms[1] = OrangeBattleUtility.FindChildRecursive(ref target, "ShootPoint1", true);
		_shootPointTransforms[2] = OrangeBattleUtility.FindChildRecursive(ref target, "ShootPoint2", true);
		_shootPointTransforms[3] = OrangeBattleUtility.FindChildRecursive(ref target, "ShootPoint3", true);
		_mainStatus = MainStatus.Shoot;
		_subStatus = SubStatus.Angle0;
		base.AimPoint = new Vector3(0f, 0.45f, 0f);
		Controller.Collisions.below = true;
	}

	private void SetStatus(MainStatus mainStatus, SubStatus subStatus)
	{
		_mainStatus = mainStatus;
		_subStatus = subStatus;
		switch (_mainStatus)
		{
		case MainStatus.Shoot:
			switch (_subStatus)
			{
			case SubStatus.Angle3:
				BulletBase.TryShotBullet(EnemyWeapons[0].BulletData, _shootPointTransforms[1], base.transform.localRotation * _upLeft, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
				BulletBase.TryShotBullet(EnemyWeapons[0].BulletData, _shootPointTransforms[2], base.transform.localRotation * _upRight, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
				break;
			case SubStatus.Angle0:
				BulletBase.TryShotBullet(EnemyWeapons[1].BulletData, _shootPointTransforms[0], base.transform.localRotation * Vector3.left, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
				BulletBase.TryShotBullet(EnemyWeapons[1].BulletData, _shootPointTransforms[3], base.transform.localRotation * Vector3.right, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
				break;
			case SubStatus.Angle1:
				BulletBase.TryShotBullet(EnemyWeapons[0].BulletData, _shootPointTransforms[2], base.transform.localRotation * _upLeft, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
				BulletBase.TryShotBullet(EnemyWeapons[0].BulletData, _shootPointTransforms[1], base.transform.localRotation * _upRight, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
				break;
			case SubStatus.Angle2:
				BulletBase.TryShotBullet(EnemyWeapons[1].BulletData, _shootPointTransforms[3], base.transform.localRotation * Vector3.left, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
				BulletBase.TryShotBullet(EnemyWeapons[1].BulletData, _shootPointTransforms[0], base.transform.localRotation * Vector3.right, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
				break;
			}
			break;
		default:
			throw new ArgumentOutOfRangeException();
		case MainStatus.Turn:
			break;
		}
		AiTimer.TimerStart();
		UpdateAnimation();
	}

	private void UpdateAnimation()
	{
		switch (_mainStatus)
		{
		case MainStatus.Shoot:
			switch (_subStatus)
			{
			case SubStatus.Angle0:
				_currentAnimationId = AnimationID.ANI_SHOOT0;
				break;
			case SubStatus.Angle1:
				_currentAnimationId = AnimationID.ANI_SHOOT1;
				break;
			case SubStatus.Angle2:
				_currentAnimationId = AnimationID.ANI_SHOOT2;
				break;
			case SubStatus.Angle3:
				_currentAnimationId = AnimationID.ANI_SHOOT3;
				break;
			}
			break;
		case MainStatus.Turn:
			switch (_subStatus)
			{
			case SubStatus.Angle0:
				_currentAnimationId = AnimationID.ANI_TURN0;
				break;
			case SubStatus.Angle1:
				_currentAnimationId = AnimationID.ANI_TURN1;
				break;
			case SubStatus.Angle2:
				_currentAnimationId = AnimationID.ANI_TURN2;
				break;
			case SubStatus.Angle3:
				_currentAnimationId = AnimationID.ANI_TURN3;
				break;
			}
			break;
		default:
			throw new ArgumentOutOfRangeException();
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
		case MainStatus.Shoot:
			if (_currentFrame >= 1f)
			{
				SetStatus(MainStatus.Turn, (_subStatus + 1 != SubStatus.MAX_SUBSTATUS) ? (_subStatus + 1) : SubStatus.Angle0);
			}
			break;
		case MainStatus.Turn:
			if (_currentFrame >= 1f)
			{
				SetStatus(MainStatus.Shoot, _subStatus);
			}
			break;
		default:
			throw new ArgumentOutOfRangeException();
		case MainStatus.IdleWaitNet:
			break;
		}
	}

	public void UpdateFunc()
	{
		if (Activate)
		{
			_transform.position = Controller.LogicPosition.vec3;
		}
	}

	public override void SetActive(bool isActive)
	{
		base.SetActive(isActive);
		_animator.enabled = isActive;
		IgnoreGravity = true;
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
		_transform.localRotation = Quaternion.identity;
		_transform.localScale = new Vector3(_transform.localScale.x, _transform.localScale.y, _transform.localScale.z * -1f);
		_transform.position = pos;
		Controller.LogicPosition = new VInt3(_transform.position);
	}
}
