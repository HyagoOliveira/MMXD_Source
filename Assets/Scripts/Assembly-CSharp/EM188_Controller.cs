using UnityEngine;

public class EM188_Controller : EnemyControllerBase, IManagedUpdateBehavior, IManagedLateUpdateBehavior
{
	public int WheelSpeed = 5950;

	public int JumpSpeed = 13500;

	protected int _nJumpCount;

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
		_collideBullet = OrangeBattleUtility.FindChildRecursive(_transform, "CollideBullet", true).gameObject.AddOrGetComponent<CollideBullet>();
		base.AimPoint = new Vector3(0f, 0.8f, 0f);
	}

	public override void LogicUpdate()
	{
		if (!Activate)
		{
			return;
		}
		base.LogicUpdate();
		if ((_velocity.x > 0 && Controller.Collisions.right) || (_velocity.x < 0 && Controller.Collisions.left))
		{
			_velocity = new VInt3(-_velocity.x, _velocity.y, 0);
			PlaySE("EnemySE", 36, visible);
		}
		if (Controller.Collisions.below)
		{
			_nJumpCount++;
			if (_nJumpCount < 4)
			{
				_velocity.y = Mathf.RoundToInt((float)JumpSpeed * (1f - 0.5f * (float)_nJumpCount / 4f));
				PlaySE("EnemySE", 36, visible);
			}
			else
			{
				_velocity.y = 0;
			}
		}
	}

	public void UpdateFunc()
	{
		if (Activate)
		{
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
		_nJumpCount = 0;
		if (isActive)
		{
			_collideBullet.UpdateBulletData(EnemyWeapons[0].BulletData);
			_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			_collideBullet.Active(targetMask);
			int num = ((OrangeBattleUtility.Random(0, 100) >= 50) ? 1 : (-1));
			_velocity.x = WheelSpeed * base.direction * num;
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
