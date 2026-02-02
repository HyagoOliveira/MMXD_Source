using UnityEngine;

public class EM179_Controller : EnemyLoopBase, IManagedUpdateBehavior, IManagedLateUpdateBehavior
{
	public int WheelSpeed = 5950;

	public int JumpSpeed = 13500;

	public Vector3 RotateSpeed = new Vector3(1200f, 0f, 0f);

	protected Vector3 RotateValue = Vector3.zero;

	private readonly int _hashHspd = Animator.StringToHash("fHspd");

	protected Transform _bodyBone;

	private void OnEnable()
	{
		MonoBehaviourSingleton<UpdateManager>.Instance.AddUpdate(this);
		MonoBehaviourSingleton<UpdateManager>.Instance.AddLateUpdate(this);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		MonoBehaviourSingleton<UpdateManager>.Instance.RemoveUpdate(this);
		MonoBehaviourSingleton<UpdateManager>.Instance.RemoveLateUpdate(this);
	}

	protected override void Awake()
	{
		base.Awake();
		Transform[] target = _transform.GetComponentsInChildren<Transform>(true);
		_bodyBone = OrangeBattleUtility.FindChildRecursive(ref target, "body_bone");
		_animator = GetComponentInChildren<Animator>();
		_collideBullet = base.gameObject.AddOrGetComponent<CollideBullet>();
		base.AimPoint = new Vector3(0f, 0.7f, 0f);
	}

	public override void LogicUpdate()
	{
		if (Activate)
		{
			base.LogicUpdate();
			if ((_velocity.x > 0 && Controller.Collisions.right) || (_velocity.x < 0 && Controller.Collisions.left))
			{
				_velocity = new VInt3(-_velocity.x, IntMath.Abs(_velocity.x) * 2, 0);
				PlaySE("EnemySE", 36, visible);
			}
		}
	}

	public override void UpdateFunc()
	{
		if (Activate)
		{
			base.UpdateFunc();
			base.transform.localPosition = Vector3.MoveTowards(base.transform.localPosition, Controller.LogicPosition.vec3, distanceDelta);
			_animator.SetFloat(_hashHspd, (float)_velocity.x * 0.001f);
		}
	}

	public void LateUpdateFunc()
	{
		if ((int)Hp > 0)
		{
			int num = ((_velocity.x > 0) ? 1 : (-1));
			RotateValue += RotateSpeed * num * Time.deltaTime;
			RotateValue.x %= 360f;
			_bodyBone.localEulerAngles = RotateValue;
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
			_velocity.x = WheelSpeed * base.direction;
		}
		else
		{
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
}
