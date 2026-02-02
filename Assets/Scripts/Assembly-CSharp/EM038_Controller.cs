using System;
using UnityEngine;

public class EM038_Controller : EnemyControllerBase, IManagedUpdateBehavior
{
	private enum MainStatus
	{
		Idle = 0,
		Aim = 1,
		HURT = 2
	}

	private enum SubStatus
	{
		Phase0 = 0,
		Phase1 = 1,
		Phase2 = 2,
		Phase3 = 3,
		MAX_SUBSTATUS = 4
	}

	private MainStatus _mainStatus;

	private SubStatus _subStatus;

	private int flyDirection = 1;

	private Transform _bodyBone;

	private Transform _laserTransform;

	private Transform _leftWingTransform;

	private Transform _rightWingTransform;

	private Transform _shootPoint;

	private float _currentAngle;

	private float _previousAngle;

	private OrangeCharacter currentTarget;

	private void SetStatus(MainStatus mainStatus, SubStatus subStatus = SubStatus.Phase0)
	{
		_mainStatus = mainStatus;
		_subStatus = subStatus;
		switch (_mainStatus)
		{
		case MainStatus.Aim:
			_velocity.y = 0;
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_previousAngle = Vector2.SignedAngle(Vector2.right, Vector2.right);
				_currentAngle = Vector2.SignedAngle(Vector2.left, Vector2.right);
				LeanTween.value(base.gameObject, _previousAngle, _currentAngle, 0.5f).setOnUpdate(delegate(float f)
				{
					if (!IsStun)
					{
						_bodyBone.localEulerAngles = Vector2.left * f;
					}
				}).setOnComplete((Action)delegate
				{
					if (!IsStun)
					{
						_currentAngle = -180f;
						SetStatus(MainStatus.Aim, SubStatus.Phase1);
					}
				});
				LeanTween.value(base.gameObject, 90f, 720f, 0.5f).setOnUpdate(delegate(float f)
				{
					if (!IsStun)
					{
						_leftWingTransform.localEulerAngles = Vector2.left * f;
						_rightWingTransform.localEulerAngles = Vector2.left * f;
					}
				});
				break;
			case SubStatus.Phase1:
				currentTarget = Target;
				if (!currentTarget)
				{
					if (!IsStun)
					{
						SetStatus(MainStatus.Aim, SubStatus.Phase3);
					}
					break;
				}
				_previousAngle = _currentAngle;
				_currentAngle = Vector2.SignedAngle(Vector2.left, _bodyBone.position - currentTarget.AimTransform.position);
				LeanTween.value(base.gameObject, _previousAngle, _currentAngle, 0.5f).setOnUpdate(delegate(float f)
				{
					if (!IsStun)
					{
						_bodyBone.localEulerAngles = Vector2.left * f;
					}
				}).setOnComplete((Action)delegate
				{
					if (!IsStun)
					{
						BulletBase.TryShotBullet(EnemyWeapons[0].BulletData, _shootPoint, _bodyBone.forward * -1f, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
						SetStatus(MainStatus.Aim, SubStatus.Phase2);
					}
				});
				break;
			case SubStatus.Phase2:
				currentTarget = Target;
				if (!currentTarget)
				{
					if (!IsStun)
					{
						SetStatus(MainStatus.Aim, SubStatus.Phase3);
					}
					break;
				}
				_previousAngle = _currentAngle;
				_currentAngle = Vector2.SignedAngle(Vector2.left, _bodyBone.position - currentTarget.AimTransform.position);
				LeanTween.value(base.gameObject, _previousAngle, _currentAngle, 0.5f).setOnUpdate(delegate(float f)
				{
					if (!IsStun)
					{
						_bodyBone.localEulerAngles = Vector2.left * f;
					}
				}).setOnComplete((Action)delegate
				{
					if (!IsStun)
					{
						BulletBase.TryShotBullet(EnemyWeapons[0].BulletData, _shootPoint, _bodyBone.forward * -1f, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
						SetStatus(MainStatus.Aim, SubStatus.Phase3);
					}
				});
				break;
			case SubStatus.Phase3:
				_previousAngle = _currentAngle;
				_currentAngle = Vector2.SignedAngle(Vector2.right, Vector2.right);
				LeanTween.value(base.gameObject, _previousAngle, _currentAngle, 0.5f).setOnUpdate(delegate(float f)
				{
					if (!IsStun)
					{
						_bodyBone.localEulerAngles = Vector2.left * f;
					}
				}).setOnComplete((Action)delegate
				{
					if (!IsStun)
					{
						SetStatus(MainStatus.Idle);
					}
				});
				LeanTween.value(base.gameObject, 0f, 90f, 0.5f).setOnUpdate(delegate(float f)
				{
					if (!IsStun)
					{
						_leftWingTransform.localEulerAngles = Vector2.left * f;
						_rightWingTransform.localEulerAngles = Vector2.left * f;
					}
				});
				break;
			}
			break;
		}
		AiTimer.TimerStart();
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

	protected override void Awake()
	{
		base.Awake();
		DeadCallback = OnDead;
		Transform[] target = _transform.GetComponentsInChildren<Transform>(true);
		_shootPoint = OrangeBattleUtility.FindChildRecursive(ref target, "ShootPoint", true);
		_bodyBone = OrangeBattleUtility.FindChildRecursive(ref target, "body_bone_sub", true);
		_laserTransform = OrangeBattleUtility.FindChildRecursive(ref target, "EM038_RayMesh", true);
		_rightWingTransform = OrangeBattleUtility.FindChildRecursive(ref target, "R_wing_bone", true);
		_leftWingTransform = OrangeBattleUtility.FindChildRecursive(ref target, "L_wing_bone", true);
		OrangeBattleUtility.AddEnemyAutoAimSystem(_transform, out _enemyAutoAimSystem);
		_enemyAutoAimSystem.UpdateAimRange(8.5f);
		_laserTransform.gameObject.SetActive(false);
		_globalWaypoints = new float[2];
		_easeSpeed = 1f;
	}

	protected override void SetStunStatus(bool enable)
	{
		IsStunStatus = false;
		if (enable)
		{
			SetStatus(MainStatus.HURT);
		}
		else
		{
			SetStatus(MainStatus.Idle);
		}
	}

	public override void LogicUpdate()
	{
		if (!Activate)
		{
			return;
		}
		IgnoreGravity = true;
		base.LogicUpdate();
		switch (_mainStatus)
		{
		case MainStatus.Idle:
			_velocity.y = Mathf.RoundToInt(CalculateVerticalMovement() * 1000f);
			if ((bool)Target && AiTimer.GetMillisecond() > 2000)
			{
				SetStatus(MainStatus.Aim);
			}
			break;
		default:
			throw new ArgumentOutOfRangeException();
		case MainStatus.Aim:
		case MainStatus.HURT:
			break;
		}
	}

	public void UpdateFunc()
	{
		if (!Activate)
		{
			return;
		}
		switch (_mainStatus)
		{
		case MainStatus.Aim:
			switch (_subStatus)
			{
			default:
				throw new ArgumentOutOfRangeException();
			case SubStatus.Phase0:
			case SubStatus.Phase1:
			case SubStatus.Phase2:
			case SubStatus.Phase3:
			case SubStatus.MAX_SUBSTATUS:
				break;
			}
			break;
		default:
			throw new ArgumentOutOfRangeException();
		case MainStatus.Idle:
		case MainStatus.HURT:
			break;
		}
		base.transform.localPosition = Vector3.MoveTowards(base.transform.localPosition, Controller.LogicPosition.vec3, distanceDelta);
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
		_currentAngle = 0f;
		_previousAngle = 0f;
		RaycastHit2D raycastHit2D = Physics2D.Raycast(_transform.position, Vector2.up, 1.5f, Controller.collisionMask);
		RaycastHit2D raycastHit2D2 = Physics2D.Raycast(_transform.position, Vector2.down, 1.5f, Controller.collisionMask);
		if (!((bool)raycastHit2D | (bool)raycastHit2D2))
		{
			_globalWaypoints[0] = _transform.position.y - 1.5f;
			_globalWaypoints[1] = _transform.position.y + 1.5f;
		}
		else if ((bool)raycastHit2D & (bool)raycastHit2D2)
		{
			float num = raycastHit2D.distance + raycastHit2D2.distance;
			float num2 = _transform.position.y - raycastHit2D2.distance + num / 2f;
			_globalWaypoints[0] = num2 - num / 2f;
			_globalWaypoints[1] = num2 + num / 2f;
		}
		else if ((bool)raycastHit2D)
		{
			float num3 = _transform.position.y + raycastHit2D.distance - 1.5f;
			_globalWaypoints[0] = num3 - 1.5f;
			_globalWaypoints[1] = num3 + 1.5f;
		}
		else
		{
			float num4 = _transform.position.y - raycastHit2D2.distance + 1.5f;
			_globalWaypoints[0] = num4 - 1.5f;
			_globalWaypoints[1] = num4 + 1.5f;
		}
	}

	public override void SetActive(bool isActive)
	{
		base.SetActive(isActive);
		if (isActive)
		{
			SetStatus(MainStatus.Idle);
			_bodyBone.localEulerAngles = Vector2.right * 0f;
			_leftWingTransform.localEulerAngles = Vector2.right * 90f;
			_rightWingTransform.localEulerAngles = Vector2.right * 90f;
		}
	}

	public override void OnTargetEnter(OrangeCharacter target)
	{
		if (!Target)
		{
			Target = target;
		}
	}

	public override void OnTargetExit(OrangeCharacter target)
	{
		if (target == Target)
		{
			Target = _enemyAutoAimSystem.GetClosetPlayer();
		}
	}

	private void OnDead()
	{
		LeanTween.cancel(base.gameObject);
	}
}
