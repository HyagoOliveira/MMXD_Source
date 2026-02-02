#define RELEASE
using System;
using CallbackDefs;
using CodeStage.AntiCheat.ObscuredTypes;
using RootMotion.FinalIK;
using StageLib;
using UnityEngine;

public class beeHelicopterController : EnemyControllerBase, IManagedUpdateBehavior
{
	private enum MainStatus
	{
		IDLE = 0,
		EXPLODE = 1,
		MGUN = 2,
		MISSILE = 3,
		EGG = 4,
		IdleWaitNet = 5,
		MAX_STATUS = 6
	}

	private enum Weapon
	{
		GUN = 0,
		MISSILE = 1,
		EGG = 2
	}

	private AimIK _aimIk;

	private Transform _shootTransform;

	private Vector3 _shootDirection;

	private float _originalXPos;

	private float _targetXPos;

	private float _originalYPos;

	private int[] _animatorHash;

	private Transform _propellerTransform;

	private Transform _gunTransform;

	private Transform _gunShootPointTransform;

	private Transform _leftMissileTransform;

	private Transform _leftMissileShootPointTransform;

	private Transform _rightMissileTransform;

	private Transform _rightMissileShootPointTransform;

	private Transform _assTransform;

	private readonly Vector3 _propellerSpeed = new Vector3(0f, 2700f, 0f);

	private Vector3 _floatVelocity;

	private int _triggerFloorFlag = 10;

	private MainStatus _mainStatus;

	private void OnEnable()
	{
		MonoBehaviourSingleton<UpdateManager>.Instance.AddUpdate(this);
	}

	private new void OnDisable()
	{
		MonoBehaviourSingleton<UpdateManager>.Instance.RemoveUpdate(this);
	}

	protected override void Awake()
	{
		base.Awake();
		_animator = GetComponentInChildren<Animator>();
		_animatorHash = new int[6];
		for (int i = 0; i < 6; i++)
		{
			_animatorHash[i] = Animator.StringToHash("em10_idle");
		}
		_animatorHash[0] = Animator.StringToHash("em10_idle");
		_animatorHash[2] = Animator.StringToHash("em10_mgun");
		_animatorHash[4] = Animator.StringToHash("em10_egg");
		_animatorHash[3] = Animator.StringToHash("em10_missile");
		_animatorHash[1] = Animator.StringToHash("em10_die");
		_propellerTransform = OrangeBattleUtility.FindChildRecursive(base.transform, "propeller", true);
		_gunTransform = OrangeBattleUtility.FindChildRecursive(base.transform, "gun_TR");
		_gunShootPointTransform = OrangeBattleUtility.FindChildRecursive(_gunTransform, "ShootPoint");
		_assTransform = OrangeBattleUtility.FindChildRecursive(base.transform, "siri5");
		_leftMissileTransform = OrangeBattleUtility.FindChildRecursive(base.transform, "maeasi_L");
		_leftMissileShootPointTransform = OrangeBattleUtility.FindChildRecursive(_leftMissileTransform, "ShootPoint");
		_rightMissileTransform = OrangeBattleUtility.FindChildRecursive(base.transform, "maeasi_R");
		_rightMissileShootPointTransform = OrangeBattleUtility.FindChildRecursive(_rightMissileTransform, "ShootPoint");
		_globalWaypoints = new float[2];
		_aimIk = GetComponentInChildren<AimIK>();
		base.AimPoint = new Vector3(0f, 0f, 0f);
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

	public override void SetActive(bool isActive)
	{
		if (isActive)
		{
			InGame = true;
			IgnoreGravity = true;
			_characterMaterial.Appear(delegate
			{
				_originalXPos = _transform.position.x;
				_originalYPos = _transform.position.y;
				_targetXPos = _originalXPos;
				_globalWaypoints[0] = _originalYPos + 0.25f;
				_globalWaypoints[1] = _originalYPos - 0.25f;
				Activate = true;
				Hp = EnemyData.n_HP;
				_mainStatus = MainStatus.IDLE;
				Controller.enabled = true;
				_aimIk.enabled = true;
				SetColliderEnable(true);
				AiTimer.TimerStart();
				base.SetActive(Activate);
			});
		}
		else
		{
			Activate = false;
			Controller.enabled = false;
			_aimIk.enabled = false;
			SetColliderEnable(false);
			AiTimer.TimerStop();
			base.SetActive(Activate);
			_characterMaterial.Disappear(delegate
			{
				InGame = false;
				MonoBehaviourSingleton<PoolManager>.Instance.BackToPool(this, EnemyData.s_MODEL);
			});
		}
	}

	public void UpdateFunc()
	{
		if (Activate)
		{
			if (_mainStatus != MainStatus.EXPLODE)
			{
				_propellerTransform.localEulerAngles += _propellerSpeed * Time.deltaTime;
			}
			base.transform.localPosition = Vector3.MoveTowards(base.transform.localPosition, Controller.LogicPosition.vec3, distanceDelta);
		}
	}

	public override void LogicUpdate()
	{
		if (!Activate || !_enemyAutoAimSystem)
		{
			return;
		}
		base.LogicUpdate();
		switch (_mainStatus)
		{
		case MainStatus.IDLE:
		{
			if (AiTimer.GetMillisecond() < EnemyData.n_AI_TIMER)
			{
				break;
			}
			ShuffleArray(ref BulletOrder);
			int[] bulletOrder = BulletOrder;
			foreach (int num2 in bulletOrder)
			{
				if (!IsWeaponAvailable(num2))
				{
					continue;
				}
				EnemyWeapons[num2].MagazineRemain = EnemyWeapons[num2].BulletData.n_MAGAZINE;
				if (StageUpdate.gbIsNetGame)
				{
					if (StageUpdate.bIsHost)
					{
						StageUpdate.RegisterSendAndRun(sNetSerialID, num2);
						_mainStatus = MainStatus.IdleWaitNet;
					}
					break;
				}
				switch (num2)
				{
				case 0:
					_mainStatus = MainStatus.MGUN;
					break;
				case 1:
					_mainStatus = MainStatus.MISSILE;
					break;
				case 2:
					_mainStatus = MainStatus.EGG;
					break;
				default:
					throw new ArgumentOutOfRangeException();
				}
				break;
			}
			break;
		}
		case MainStatus.MGUN:
		{
			int num = 0;
			if (EnemyWeapons[num].MagazineRemain > 0f)
			{
				if (!EnemyWeapons[num].LastUseTimer.IsStarted() || EnemyWeapons[num].LastUseTimer.GetMillisecond() > EnemyWeapons[num].BulletData.n_FIRE_SPEED)
				{
					_shootDirection = (_gunShootPointTransform.position - _gunTransform.position).normalized;
					BulletBase.TryShotBullet(EnemyWeapons[num].BulletData, _gunShootPointTransform, _shootDirection, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
					EnemyWeapons[num].LastUseTimer.TimerStart();
					EnemyWeapons[num].MagazineRemain -= EnemyWeapons[num].BulletData.n_USE_COST;
				}
			}
			else
			{
				_mainStatus = MainStatus.IDLE;
				AiTimer.TimerStart();
			}
			break;
		}
		case MainStatus.MISSILE:
		{
			int num = 1;
			if (EnemyWeapons[num].MagazineRemain > 0f)
			{
				if (EnemyWeapons[num].LastUseTimer.IsStarted() && EnemyWeapons[num].LastUseTimer.GetMillisecond() <= EnemyWeapons[num].BulletData.n_FIRE_SPEED)
				{
					break;
				}
				Vector3 vector = Vector3.zero;
				Target = _enemyAutoAimSystem.GetClosetPlayer();
				if ((bool)Target)
				{
					vector = Target.AimTransform.position;
				}
				switch ((int)EnemyWeapons[num].MagazineRemain % 2)
				{
				case 0:
					_shootDirection = (vector - _leftMissileShootPointTransform.position).normalized;
					if (Vector2.Angle(Vector3.left, _shootDirection) >= 45f || vector == Vector3.zero)
					{
						_shootDirection = Vector3.left;
					}
					BulletBase.TryShotBullet(EnemyWeapons[num].BulletData, _leftMissileShootPointTransform, _shootDirection, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
					break;
				case 1:
					_shootDirection = (vector - _rightMissileShootPointTransform.position).normalized;
					if (Vector2.Angle(Vector3.left, _shootDirection) >= 45f || vector == Vector3.zero)
					{
						_shootDirection = Vector3.left;
					}
					BulletBase.TryShotBullet(EnemyWeapons[num].BulletData, _rightMissileShootPointTransform, _shootDirection, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
					break;
				}
				EnemyWeapons[num].LastUseTimer.TimerStart();
				EnemyWeapons[num].MagazineRemain -= 1f;
			}
			else
			{
				_mainStatus = MainStatus.IDLE;
				AiTimer.TimerStart();
			}
			break;
		}
		case MainStatus.EGG:
		{
			int num = 2;
			MOB_TABLE mOB_TABLE = ManagedSingleton<OrangeDataManager>.Instance.MOB_TABLE_DICT[(int)EnemyWeapons[num].BulletData.f_EFFECT_X];
			if (EnemyWeapons[num].MagazineRemain > 0f)
			{
				if (!EnemyWeapons[num].LastUseTimer.IsStarted() || EnemyWeapons[num].LastUseTimer.GetMillisecond() > EnemyWeapons[num].BulletData.n_FIRE_SPEED)
				{
					EnemyControllerBase enemyControllerBase = StageUpdate.StageSpawnEnemyByMob(mOB_TABLE, sNetSerialID + "0");
					if ((bool)enemyControllerBase)
					{
						enemyControllerBase.UpdateEnemyID(mOB_TABLE.n_ID);
						enemyControllerBase.SetPositionAndRotation(_assTransform.position + Vector3.left * 1.385f, true);
						enemyControllerBase.SetActive(true);
					}
					EnemyWeapons[num].LastUseTimer.TimerStart();
					EnemyWeapons[num].MagazineRemain -= 1f;
				}
			}
			else
			{
				_mainStatus = MainStatus.IDLE;
				AiTimer.TimerStart();
			}
			break;
		}
		case MainStatus.EXPLODE:
		{
			ExcludePlayer(0, -1);
			ExcludeEnemy(0, -1);
			IgnoreGravity = false;
			if (!Controller.Collisions.below)
			{
				break;
			}
			RaycastHit2D raycastHit2D = Controller.SolidMeeting(0f, -1f);
			if (!raycastHit2D)
			{
				break;
			}
			Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.CAMERA_SHAKE, 5f, false);
			_characterMaterial.UpdateTex(0);
			FallingFloor component = raycastHit2D.collider.GetComponent<FallingFloor>();
			if ((bool)component)
			{
				if (_triggerFloorFlag > 0)
				{
					_triggerFloorFlag--;
					component.TriggerFall();
					break;
				}
				Debug.Log("Found FallingFloor");
				base.gameObject.layer = ManagedSingleton<OrangeLayerManager>.Instance.ObstacleLayer;
				Activate = false;
				base.enabled = false;
			}
			else
			{
				Debug.Log("NotFound FallingFloor");
				base.gameObject.layer = ManagedSingleton<OrangeLayerManager>.Instance.ObstacleLayer;
				Activate = false;
				base.enabled = false;
			}
			break;
		}
		default:
			throw new ArgumentOutOfRangeException();
		case MainStatus.IdleWaitNet:
			break;
		}
		if (_mainStatus != MainStatus.EXPLODE)
		{
			_velocity.y = Mathf.RoundToInt(CalculateVerticalMovement() * 1000f);
			UpdateMagazine();
		}
		if (_transform.position.x > _targetXPos)
		{
			_velocity.x = -3000;
		}
		else
		{
			_velocity.x = 0;
		}
		_animator.Play(_animatorHash[(int)_mainStatus]);
	}

	public override void UpdateStatus(int nSet, string sMsg, Callback tCB = null)
	{
		switch (nSet)
		{
		case 0:
			_mainStatus = MainStatus.MGUN;
			break;
		case 1:
			_mainStatus = MainStatus.MISSILE;
			break;
		case 2:
			_mainStatus = MainStatus.EGG;
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	public override void OnTargetEnter(OrangeCharacter target)
	{
		base.OnTargetEnter(target);
		_aimIk.solver.target = Target.AimTransform;
	}

	public override void OnTargetExit(OrangeCharacter target)
	{
		base.OnTargetExit(target);
		_aimIk.solver.target = null;
	}

	private new bool IsWeaponAvailable(int weaponID)
	{
		if (EnemyWeapons[weaponID].LastUseTimer.IsStarted() && !(EnemyWeapons[weaponID].MagazineRemain > 0f))
		{
			return EnemyWeapons[weaponID].LastUseTimer.GetMillisecond() > EnemyWeapons[weaponID].BulletData.n_RELOAD;
		}
		return true;
	}

	public override ObscuredInt Hurt(HurtPassParam tHurtPassParam)
	{
		OrangeBattleUtility.UpdateEnemyHp(ref Hp, ref tHurtPassParam.dmg, base.UpdateHurtAction);
		if ((int)Hp > 0)
		{
			_targetXPos = _originalXPos - 5f * (1f - (float)(int)Hp / (float)EnemyData.n_HP);
			_characterMaterial.Hurt();
		}
		else
		{
			_velocity.y = 0;
			EnemyCollider[] enemyCollider = _enemyCollider;
			for (int i = 0; i < enemyCollider.Length; i++)
			{
				enemyCollider[i].gameObject.layer = ManagedSingleton<OrangeLayerManager>.Instance.DefaultLayer;
			}
			_mainStatus = MainStatus.EXPLODE;
		}
		return Hp;
	}
}
