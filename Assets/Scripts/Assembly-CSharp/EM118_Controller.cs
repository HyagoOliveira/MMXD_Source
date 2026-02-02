using UnityEngine;

public class EM118_Controller : EnemyControllerBase, IManagedUpdateBehavior
{
	public int WheelSpeed = 5950;

	public int JumpSpeed = 13500;

	[SerializeField]
	private float RotateSpeed = 0.3f;

	private readonly int _hashHspd = Animator.StringToHash("fHspd");

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
		_collideBullet = OrangeBattleUtility.FindChildRecursive(_transform, "CollideBullet", true).gameObject.AddOrGetComponent<CollideBullet>();
		base.AimPoint = new Vector3(0f, 0.8f, 0f);
	}

	public override void LogicUpdate()
	{
		if (Activate)
		{
			base.LogicUpdate();
			if ((_velocity.x > 0 && Controller.Collisions.right) || (_velocity.x < 0 && Controller.Collisions.left))
			{
				_velocity = new VInt3(-_velocity.x, IntMath.Abs(_velocity.x) * 2, 0);
				PlaySE(EnemySE02.CRI_ENEMYSE02_EM030_PUMPKIN01);
			}
			if (Controller.Collisions.below)
			{
				_velocity.y = JumpSpeed;
				PlaySE(EnemySE02.CRI_ENEMYSE02_EM030_PUMPKIN01);
			}
		}
	}

	public void UpdateFunc()
	{
		if (Activate)
		{
			base.transform.localPosition = Vector3.MoveTowards(base.transform.localPosition, Controller.LogicPosition.vec3, distanceDelta);
			_animator.SetFloat(_hashHspd, (float)_velocity.x * 0.001f * RotateSpeed);
		}
	}

	public override void SetActive(bool isActive)
	{
		base.SetActive(isActive);
		_animator.enabled = isActive;
		if (isActive)
		{
			base.AllowAutoAim = false;
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
