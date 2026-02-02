#define RELEASE
using System;
using CodeStage.AntiCheat.ObscuredTypes;
using UnityEngine;

public class EM019_Controller : EnemyLoopBase, IManagedUpdateBehavior
{
	public int WheelSpeed = 6000;

	public int Accelerate = 500;

	public float RayLength = 3f;

	private readonly int _hashHspd = Animator.StringToHash("fHspd");

	private Transform _pilotColliderTransform;

	private Transform _carTransform;

	private Transform _shootPoint;

	private Transform _wheelTransform;

	private Transform _pilotTransform;

	private BoxCollider2D standPlatform;

	private PlatformController _platformController;

	private ObscuredInt _pilotHp;

	private int _leanTweenID;

	private int playerDirection;

	private int playerDirectionPrev;

	private Vector3 _centerPos;

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
		_pilotColliderTransform = OrangeBattleUtility.FindChildRecursive(ref target, "PilotCollider", true);
		_pilotColliderTransform.gameObject.AddOrGetComponent<StageObjParam>().nSubPartID = 1;
		_carTransform = OrangeBattleUtility.FindChildRecursive(ref target, "Collider", true);
		_wheelTransform = OrangeBattleUtility.FindChildRecursive(ref target, "em019_wheel", true);
		base.AimTransform = OrangeBattleUtility.FindChildRecursive(ref target, "DriverPoint", true);
		ModelTransform = OrangeBattleUtility.FindChildRecursive(ref target, "model", true);
		_shootPoint = OrangeBattleUtility.FindChildRecursive(ref target, "ShootPoint", true);
		_pilotTransform = OrangeBattleUtility.FindChildRecursive(ref target, "EM019_DriverMesh", true);
		standPlatform = OrangeBattleUtility.FindChildRecursive(ref target, "BoxCollider", true).GetComponent<BoxCollider2D>();
		_leanTweenID = -1;
		AiTimer.TimerStart();
		_platformController = GetComponentInChildren<PlatformController>();
		OrangeBattleUtility.AddEnemyAutoAimSystem(_transform, out _enemyAutoAimSystem);
		_enemyAutoAimSystem.UpdateAimRange(10f);
	}

	public override void LogicUpdate()
	{
		if (!Activate)
		{
			return;
		}
		base.LogicUpdate();
		if (IsStun)
		{
			return;
		}
		if (_leanTweenID == -1 && (int)_pilotHp > 0)
		{
			_centerPos = Controller.GetCenterPos();
			Target = _enemyAutoAimSystem.GetClosetPlayer();
			if ((bool)Target)
			{
				playerDirection = Math.Sign(Target.GetTargetPoint().x - _centerPos.x);
			}
			if ((bool)OrangeBattleUtility.RaycastIgnoreSelf(_centerPos, Vector2.right * base.direction, RayLength, Controller.collisionMask, _transform) || (playerDirection != base.direction && playerDirection != playerDirectionPrev))
			{
				EM019_Drift();
			}
			if (AiTimer.GetMillisecond() > 1000 && (bool)OrangeBattleUtility.RaycastIgnoreSelf(_centerPos, Vector2.right * base.direction, RayLength, targetMask, _transform))
			{
				AiTimer.TimerStart();
				BulletBase.TryShotBullet(EnemyWeapons[0].BulletData, _shootPoint, Vector3.right * base.direction, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
			}
		}
		if ((int)_pilotHp <= 0 && ((_velocity.x > 0 && Controller.Collisions.right) || (_velocity.x < 0 && Controller.Collisions.left)))
		{
			Hp = 0;
			Hurt(new HurtPassParam());
		}
		if (Mathf.Abs(_velocity.x + Accelerate * base.direction) < WheelSpeed)
		{
			_velocity.x += Accelerate * base.direction;
		}
		playerDirectionPrev = playerDirection;
	}

	private void EM019_Drift()
	{
		MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("OBJ_DASH_SMOKE", _wheelTransform, Quaternion.identity, Array.Empty<object>());
		_leanTweenID = LeanTween.value(base.gameObject, base.direction * 90, -base.direction * 110, 0.5f).setOnUpdate(delegate(float f)
		{
			ModelTransform.eulerAngles = Vector3.up * f;
		}).setOnComplete((Action)delegate
		{
			base.direction = -base.direction;
			LeanTween.value(base.gameObject, base.direction * 110, base.direction * 90, 0.2f).setOnUpdate(delegate(float f)
			{
				ModelTransform.eulerAngles = Vector3.up * f;
			}).setOnComplete((Action)delegate
			{
				_leanTweenID = -1;
				PlaySE("EnemySE", 38, visible);
				ContinueIdleSETime();
			});
		})
			.id;
		PlaySE("EnemySE", 41, visible);
		PauseIdleSETime();
	}

	public override void UpdateFunc()
	{
		base.UpdateFunc();
		if ((int)_pilotHp <= 0)
		{
			Vector3 velocity = Vector3.MoveTowards(base.transform.localPosition, Controller.LogicPosition.vec3, distanceDelta) - base.transform.localPosition;
			_platformController.ManualUpdatePhase1(velocity);
		}
		base.transform.localPosition = Vector3.MoveTowards(base.transform.localPosition, Controller.LogicPosition.vec3, distanceDelta);
		if ((int)_pilotHp <= 0)
		{
			_platformController.ManualUpdatePhase2();
		}
	}

	public override void SetActive(bool isActive)
	{
		if (!isActive)
		{
			LeanTween.cancel(base.gameObject);
		}
		base.SetActive(isActive);
		_animator.enabled = isActive;
		if (isActive)
		{
			_collideBullet.UpdateBulletData(EnemyWeapons[1].BulletData);
			_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			_collideBullet.Active(targetMask);
			_velocity.x = WheelSpeed * base.direction;
			TogglePilot(true);
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
		ModelTransform.eulerAngles = Vector3.up * 90f * base.direction;
		base.transform.position = pos;
	}

	private void TogglePilot(bool enable)
	{
		bool flag = _pilotTransform.gameObject.activeSelf != enable;
		_pilotTransform.gameObject.SetActive(enable);
		base.AllowAutoAim = enable;
		standPlatform.enabled = !enable;
		if (enable)
		{
			_collideBullet.Active(targetMask);
		}
		else if (flag)
		{
			PlaySE(ExplodeSE[0], ExplodeSE[1]);
			MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("FX_MOB_EXPLODE0", _pilotTransform.position, Quaternion.identity, Array.Empty<object>());
			_collideBullet.BackToPool();
		}
	}

	public override ObscuredInt Hurt(HurtPassParam tHurtPassParam)
	{
		tHurtPassParam.dmg = selfBuffManager.ReduceDmgByEnergyShild(tHurtPassParam.dmg);
		OrangeBattleUtility.UpdateEnemyHp(ref Hp, ref tHurtPassParam.dmg);
		if (tHurtPassParam.nSubPartID == 1)
		{
			OrangeBattleUtility.UpdateEnemyHp(ref _pilotHp, ref tHurtPassParam.dmg);
			if ((int)_pilotHp <= 0)
			{
				TogglePilot(false);
			}
		}
		if (!InGame)
		{
			Debug.LogWarning("[Enemy] InGame Flag is false.");
			return Hp;
		}
		UpdateHurtAction();
		if ((int)Hp > 0)
		{
			_characterMaterial.Hurt();
		}
		else
		{
			PlaySE(ExplodeSE[0], ExplodeSE[1]);
			Explosion();
			Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.CAMERA_SHAKE, 1f, false);
			BackToPool();
		}
		return Hp;
	}

	public override void UpdateEnemyID(int id)
	{
		base.UpdateEnemyID(id);
		_pilotHp = EnemyData.n_HP / 2;
	}
}
