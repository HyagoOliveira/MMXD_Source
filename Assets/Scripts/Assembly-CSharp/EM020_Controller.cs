using System;
using UnityEngine;

public class EM020_Controller : EnemyControllerBase, IManagedUpdateBehavior
{
	public int MaxSpeed = 9000;

	public int Accelerate = 300;

	public float DefaultRotateShift = -35f;

	public float DefaultRotate = 90f;

	private Vector3 _modelRotate;

	private readonly int _hashHspd = Animator.StringToHash("fHspd");

	private Transform[] exhaustTransforms;

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
		base.AimTransform = OrangeBattleUtility.FindChildRecursive(ref target, "body_bone", true);
		base.AimPoint = Vector3.up * 0.2f;
		exhaustTransforms = OrangeBattleUtility.FindAllChildRecursive(ref target, "exhaust");
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxduring_EM020_000", 4);
	}

	public override void LogicUpdate()
	{
		if (!Activate)
		{
			return;
		}
		base.LogicUpdate();
		PlaySE("EnemySE", 30);
		if ((_velocity.x > 0 && Controller.Collisions.right) || (_velocity.x < 0 && Controller.Collisions.left))
		{
			_velocity = new VInt3(0, IntMath.Abs(_velocity.x), 0);
			base.direction = -base.direction;
			_modelRotate.y = DefaultRotateShift + (float)base.direction * DefaultRotate;
			ModelTransform.localEulerAngles = _modelRotate;
			return;
		}
		if (_velocity.x == 0)
		{
			PlaySE("EnemySE", 31);
		}
		if (Mathf.Abs(_velocity.x) < MaxSpeed)
		{
			_velocity.x += Accelerate * base.direction;
		}
		else
		{
			_velocity.x = MaxSpeed * base.direction;
		}
	}

	public void UpdateFunc()
	{
		if (Activate)
		{
			base.transform.localPosition = Vector3.MoveTowards(base.transform.localPosition, Controller.LogicPosition.vec3, distanceDelta);
			_animator.SetFloat(_hashHspd, Mathf.Abs((float)_velocity.x * 0.001f));
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
			Transform[] array = exhaustTransforms;
			foreach (Transform pTransform in array)
			{
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxduring_EM020_000", pTransform, Quaternion.identity, Array.Empty<object>());
			}
		}
		else
		{
			_velocity = VInt3.zero;
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
		_modelRotate = ModelTransform.localEulerAngles;
		_modelRotate.y = DefaultRotateShift + (float)base.direction * DefaultRotate;
		ModelTransform.localEulerAngles = _modelRotate;
		_transform.position = pos;
	}
}
