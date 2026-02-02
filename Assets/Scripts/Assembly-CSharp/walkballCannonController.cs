using CallbackDefs;
using RootMotion.FinalIK;
using StageLib;
using UnityEngine;

public class walkballCannonController : EnemyControllerBase, IManagedUpdateBehavior
{
	private AimIK _aimIk;

	private Transform _shootTransform;

	private Vector3 _shootDirection;

	private OrangeTimer _attackTimer;

	private bool bShotReady;

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
		_attackTimer = OrangeTimerManager.GetTimer();
		base.AimPoint = new Vector3(0f, 1.6f, 0f);
		_aimIk = GetComponentInChildren<AimIK>();
		_animator = GetComponentInChildren<Animator>();
		_shootTransform = OrangeBattleUtility.FindChildRecursive(base.transform, "ShootPoint");
	}

	protected override void Start()
	{
		base.Start();
		_aimIk.solver.IKPositionWeight = 1f;
	}

	public override void UpdateEnemyID(int id)
	{
		base.UpdateEnemyID(id);
		if (null == _enemyAutoAimSystem)
		{
			OrangeBattleUtility.AddEnemyAutoAimSystem(_transform, out _enemyAutoAimSystem);
		}
		_enemyAutoAimSystem.UpdateAimRange(EnemyWeapons[0].BulletData.f_DISTANCE);
	}

	public void UpdateFunc()
	{
		if (Activate)
		{
			base.transform.localPosition = Vector3.MoveTowards(base.transform.localPosition, Controller.LogicPosition.vec3, distanceDelta);
		}
	}

	public override void LogicUpdate()
	{
		if (!Activate)
		{
			return;
		}
		UpdateMagazine();
		base.LogicUpdate();
		if (IsStun)
		{
			return;
		}
		Target = _enemyAutoAimSystem.GetClosetPlayer();
		if ((bool)Target)
		{
			_aimIk.solver.target = Target.AimTransform;
		}
		else
		{
			_aimIk.solver.target = null;
		}
		if ((bool)Target && _attackTimer.GetMillisecond() > EnemyWeapons[0].BulletData.n_FIRE_SPEED)
		{
			_attackTimer.TimerStart();
			if (StageUpdate.bIsHost)
			{
				StageUpdate.RegisterSendAndRun(sNetSerialID, 0);
			}
		}
		if ((bool)Target && bShotReady)
		{
			bShotReady = false;
			_shootDirection = (Target.AimTransform.position - _shootTransform.position).normalized;
			BulletBase.TryShotBullet(EnemyWeapons[0].BulletData, _shootTransform, _shootDirection, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
		}
		LogicLateUpdate();
	}

	public override void UpdateStatus(int nSet, string sMsg, Callback tCB = null)
	{
		bShotReady = true;
	}

	public void LogicLateUpdate()
	{
		if ((bool)Target && Activate)
		{
			base.direction = ((Target.transform.position.x <= _transform.position.x) ? 1 : (-1));
			_transform.eulerAngles = new Vector3(0f, 90 + 90 * base.direction, 0f);
		}
	}

	public override void SetActive(bool isActive)
	{
		base.SetActive(isActive);
		if (isActive)
		{
			_animator.enabled = true;
			_attackTimer.TimerStart();
			_aimIk.enabled = true;
		}
		else
		{
			_animator.enabled = false;
			_attackTimer.TimerStop();
			_aimIk.enabled = false;
		}
	}

	public override void BackToPool()
	{
		IsStun = false;
		SetActive(false);
	}
}
