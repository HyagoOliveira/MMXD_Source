using System;
using CallbackDefs;
using UnityEngine;

public class EM031_Controller : EnemyControllerBase, IManagedUpdateBehavior
{
	private enum MainStatus
	{
		IDLE = 0,
		HURT = 1,
		RUN = 2,
		SHOOT = 3,
		IdleWaitNet = 4,
		MAX_STATUS = 5
	}

	public int RunSpeed = 3000;

	private Transform _shootTransform;

	private int[] _animatorHash;

	private Transform _leftGunTransform;

	private Transform _rightGunTransform;

	private bool _isShoot;

	private MainStatus _mainStatus;

	private float _currentFrame;

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
		Transform[] target = _transform.GetComponentsInChildren<Transform>(true);
		ModelTransform = OrangeBattleUtility.FindChildRecursive(ref target, "model", true);
		base.AimTransform = OrangeBattleUtility.FindChildRecursive(ref target, "Collider");
		_animator = GetComponentInChildren<Animator>();
		_animatorHash = new int[5];
		for (int i = 0; i < 5; i++)
		{
			_animatorHash[i] = Animator.StringToHash("idle");
		}
		_animatorHash[0] = Animator.StringToHash("EM031@idle_loop");
		_animatorHash[1] = Animator.StringToHash("EM031@hurt_loop");
		_animatorHash[2] = Animator.StringToHash("EM031@run_loop");
		_animatorHash[3] = Animator.StringToHash("EM031@skill_01");
		_leftGunTransform = OrangeBattleUtility.FindChildRecursive(ref target, "em031_Lear");
		_rightGunTransform = OrangeBattleUtility.FindChildRecursive(ref target, "em031_Rear");
		if (null == _enemyAutoAimSystem)
		{
			OrangeBattleUtility.AddEnemyAutoAimSystem(_transform, out _enemyAutoAimSystem);
		}
	}

	public override void SetActive(bool isActive)
	{
		base.SetActive(isActive);
		if (isActive)
		{
			SetStatus(MainStatus.IDLE);
		}
	}

	public void UpdateFunc()
	{
		if (!Activate)
		{
			return;
		}
		base.transform.localPosition = Vector3.MoveTowards(base.transform.localPosition, Controller.LogicPosition.vec3, distanceDelta);
		_currentFrame = _animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
		switch (_mainStatus)
		{
		case MainStatus.RUN:
			if ((double)_currentFrame >= 0.2 && (double)_currentFrame < 0.8)
			{
				_velocity.x = RunSpeed * base.direction;
			}
			else
			{
				_velocity.x = 0;
			}
			break;
		case MainStatus.SHOOT:
			if (!_isShoot && (double)_currentFrame >= 0.64)
			{
				BulletBase.TryShotBullet(EnemyWeapons[1].BulletData, _leftGunTransform, Vector3.right * base.direction, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
				_isShoot = true;
			}
			break;
		}
	}

	private void SetStatus(MainStatus mainStatus)
	{
		_mainStatus = mainStatus;
		switch (_mainStatus)
		{
		case MainStatus.IDLE:
			_velocity.x = 0;
			break;
		case MainStatus.SHOOT:
			_isShoot = false;
			break;
		}
		_animator.Play(_animatorHash[(int)_mainStatus]);
	}

	public override void LogicUpdate()
	{
		if (!Activate || !_enemyAutoAimSystem)
		{
			return;
		}
		base.LogicUpdate();
		_currentFrame = _animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
		switch (_mainStatus)
		{
		case MainStatus.IDLE:
			Target = _enemyAutoAimSystem.GetClosetPlayer();
			if ((bool)Target && Math.Sign(Target._transform.position.x - _transform.position.x) != base.direction)
			{
				base.direction = -base.direction;
				ModelTransform.localScale = new Vector3(ModelTransform.localScale.x, ModelTransform.localScale.y, Mathf.Abs(ModelTransform.localScale.z) * (float)base.direction);
			}
			if ((bool)Target)
			{
				Vector3 vector = _transform.position - Target.GetTargetPoint();
				if (Math.Abs(vector.x) < EnemyWeapons[1].BulletData.f_DISTANCE && Math.Abs(vector.y) < 1f)
				{
					SetStatus(MainStatus.SHOOT);
				}
				else
				{
					SetStatus(MainStatus.RUN);
				}
			}
			break;
		case MainStatus.SHOOT:
			if (_currentFrame >= 1f)
			{
				SetStatus(MainStatus.IDLE);
			}
			break;
		case MainStatus.RUN:
			if (_currentFrame >= 1f)
			{
				SetStatus(MainStatus.IDLE);
			}
			break;
		default:
			SetStatus(MainStatus.IDLE);
			throw new ArgumentOutOfRangeException();
		case MainStatus.IdleWaitNet:
			break;
		}
	}

	public override void UpdateStatus(int nSet, string sMSg, Callback tCB = null)
	{
		if (nSet == 0)
		{
			SetStatus(MainStatus.SHOOT);
			return;
		}
		throw new ArgumentOutOfRangeException();
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
		ModelTransform.localScale = new Vector3(ModelTransform.localScale.x, ModelTransform.localScale.y, Mathf.Abs(ModelTransform.localScale.z) * (float)base.direction);
		base.transform.position = pos;
	}
}
