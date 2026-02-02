using UnityEngine;

public class wheelController : EnemyLoopBase, IManagedUpdateBehavior
{
	public int WheelSpeed = 3;

	private readonly int _hashHspd = Animator.StringToHash("fHspd");

	private AnimatorSoundHelper animatorSoundHelper;

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
		animatorSoundHelper = GetComponentInChildren<AnimatorSoundHelper>();
	}

	public override void LogicUpdate()
	{
		if (Activate)
		{
			base.LogicUpdate();
			if ((_velocity.x > 0 && Controller.Collisions.right) || (_velocity.x < 0 && Controller.Collisions.left))
			{
				_velocity = new VInt3(-_velocity.x, IntMath.Abs(_velocity.x) * 2, 0);
				animatorSoundHelper.PlayEnemySE(36);
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

	public override void SetActive(bool isActive)
	{
		base.SetActive(isActive);
		_animator.enabled = isActive;
		if (isActive)
		{
			_collideBullet.UpdateBulletData(EnemyWeapons[0].BulletData);
			_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			_collideBullet.Active(targetMask);
			_velocity.x = WheelSpeed * 1000 * base.direction;
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
