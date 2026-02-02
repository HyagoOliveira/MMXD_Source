using System;
using CallbackDefs;
using StageLib;
using UnityEngine;

public class gunvoltController : EnemyControllerBase, IManagedUpdateBehavior
{
	private enum MainStatus
	{
		IDLE = 0,
		HURT = 1,
		MISSILE = 2,
		THUNDERBOLT = 3,
		IdleWaitNet = 4,
		MAX_STATUS = 5
	}

	private enum Weapon
	{
		THUNDERBOLT = 0,
		MISSILE = 1
	}

	private Transform _shootTransform;

	private int[] _animatorHash;

	private Transform _leftMissileTransform;

	private Transform _rightMissileTransform;

	private Transform _leftThunderboltTransform;

	private Transform _rightThunderboltTransform;

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
		base.AimTransform = OrangeBattleUtility.FindChildRecursive(ref target, "hara_TR");
		_animator = GetComponentInChildren<Animator>();
		_animatorHash = new int[5];
		for (int i = 0; i < 5; i++)
		{
			_animatorHash[i] = Animator.StringToHash("idle");
		}
		_animatorHash[0] = Animator.StringToHash("EM018@idle_loop");
		_animatorHash[1] = Animator.StringToHash("EM018@hurt_loop");
		_animatorHash[3] = Animator.StringToHash("EM018@skill_02");
		_animatorHash[2] = Animator.StringToHash("EM018@skill_01");
		_leftMissileTransform = OrangeBattleUtility.FindChildRecursive(ref target, "missile_L");
		_rightMissileTransform = OrangeBattleUtility.FindChildRecursive(ref target, "missile_R");
		_leftThunderboltTransform = OrangeBattleUtility.FindChildRecursive(ref target, "denji_L");
		_rightThunderboltTransform = OrangeBattleUtility.FindChildRecursive(ref target, "denji_R");
	}

	protected override void Start()
	{
		base.Start();
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

	public override void OnToggleCharacterMaterial(bool appear)
	{
		if (appear)
		{
			_mainStatus = MainStatus.IDLE;
		}
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
		if (!Activate || !_enemyAutoAimSystem)
		{
			return;
		}
		base.LogicUpdate();
		_currentFrame = _animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
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
				if (!IsWeaponAvailable(num2) || _enemyAutoAimSystem.GetClosetPlayer() == null)
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
					_mainStatus = MainStatus.MISSILE;
					break;
				case 1:
					_mainStatus = MainStatus.THUNDERBOLT;
					break;
				default:
					throw new ArgumentOutOfRangeException();
				}
				break;
			}
			break;
		}
		case MainStatus.MISSILE:
		{
			int num = 0;
			if (EnemyWeapons[num].MagazineRemain > 0f && (!EnemyWeapons[num].LastUseTimer.IsStarted() || EnemyWeapons[num].LastUseTimer.GetMillisecond() > EnemyWeapons[num].BulletData.n_FIRE_SPEED))
			{
				switch ((int)EnemyWeapons[num].MagazineRemain % 2)
				{
				case 0:
					if ((double)_currentFrame > 0.31)
					{
						BasicBullet basicBullet = (BasicBullet)BulletBase.TryShotBullet(EnemyWeapons[num].BulletData, _leftMissileTransform, Vector3.right * base.direction, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
						EnemyWeapons[num].LastUseTimer.TimerStart();
						EnemyWeapons[num].MagazineRemain -= 1f;
					}
					break;
				case 1:
					if ((double)_currentFrame > 0.62)
					{
						BasicBullet basicBullet2 = (BasicBullet)BulletBase.TryShotBullet(EnemyWeapons[num].BulletData, _rightMissileTransform, Vector3.right * base.direction, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
						EnemyWeapons[num].LastUseTimer.TimerStart();
						EnemyWeapons[num].MagazineRemain -= 1f;
					}
					break;
				}
			}
			if (_currentFrame > 1f)
			{
				_mainStatus = MainStatus.IDLE;
				AiTimer.TimerStart();
			}
			break;
		}
		case MainStatus.THUNDERBOLT:
		{
			int num = 1;
			if (EnemyWeapons[num].MagazineRemain > 0f && _currentFrame > 0.35f && (!EnemyWeapons[num].LastUseTimer.IsStarted() || EnemyWeapons[num].LastUseTimer.GetMillisecond() > EnemyWeapons[num].BulletData.n_FIRE_SPEED))
			{
				BulletBase.TryShotBullet(EnemyWeapons[num].BulletData, _leftThunderboltTransform, Vector3.right * base.direction, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
				BulletBase.TryShotBullet(EnemyWeapons[num].BulletData, _rightThunderboltTransform, Vector3.right * base.direction, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
				EnemyWeapons[num].LastUseTimer.TimerStart();
				EnemyWeapons[num].MagazineRemain -= 2f;
			}
			if (_currentFrame > 1f)
			{
				_mainStatus = MainStatus.IDLE;
				AiTimer.TimerStart();
			}
			break;
		}
		default:
			_mainStatus = MainStatus.IDLE;
			throw new ArgumentOutOfRangeException();
		case MainStatus.IdleWaitNet:
			break;
		}
		_animator.Play(_animatorHash[(int)_mainStatus]);
	}

	public override void UpdateStatus(int nSet, string sMSg, Callback tCB = null)
	{
		switch (nSet)
		{
		case 0:
			_mainStatus = MainStatus.MISSILE;
			_animator.Play(_animatorHash[(int)_mainStatus]);
			break;
		case 1:
			_mainStatus = MainStatus.THUNDERBOLT;
			_animator.Play(_animatorHash[(int)_mainStatus]);
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	private new bool IsWeaponAvailable(int weaponID)
	{
		if (EnemyWeapons[weaponID].LastUseTimer.IsStarted() && !(EnemyWeapons[weaponID].MagazineRemain > 0f))
		{
			return EnemyWeapons[weaponID].LastUseTimer.GetMillisecond() > EnemyWeapons[weaponID].BulletData.n_RELOAD;
		}
		return true;
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
