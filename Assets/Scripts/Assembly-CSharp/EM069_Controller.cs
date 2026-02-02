using System;
using CallbackDefs;
using UnityEngine;

public class EM069_Controller : EnemyControllerBase, IManagedUpdateBehavior
{
	private enum MainStatus
	{
		IDLE = 0,
		HURT = 1,
		TURN = 2,
		SHOOT = 3,
		IdleWaitNet = 4,
		MAX_STATUS = 5
	}

	private enum SubStatus
	{
		Phase0 = 0,
		Phase1 = 1,
		Phase2 = 2
	}

	private BulletBase _currentBullet;

	private Transform _shootTransform;

	private int[] _animatorHash;

	private MainStatus _mainStatus;

	private SubStatus _subStatus;

	private float leftDir = -90f;

	private float rightDir = 90f;

	private int shootPhase;

	private float _currentFrame;

	private int _turnID = -1;

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
		base.AllowAutoAim = false;
		Transform[] target = _transform.GetComponentsInChildren<Transform>(true);
		ModelTransform = OrangeBattleUtility.FindChildRecursive(ref target, "model", true);
		base.AimTransform = OrangeBattleUtility.FindChildRecursive(ref target, "EM069_Mesh");
		_animator = GetComponentInChildren<Animator>();
		_animatorHash = new int[5];
		OrangeBattleUtility.FindChildRecursive(ref target, "Block", true).gameObject.AddOrGetComponent<StageObjParam>().nSubPartID = 1;
		GuardTransform.Add(1);
		for (int i = 0; i < 5; i++)
		{
			_animatorHash[i] = Animator.StringToHash("idle");
		}
		_animatorHash[0] = Animator.StringToHash("EM069@idle_loop");
		_animatorHash[3] = Animator.StringToHash("EM069@atk_start");
		_shootTransform = OrangeBattleUtility.FindChildRecursive(ref target, "ShootPoint");
	}

	public override void UpdateEnemyID(int id)
	{
		base.UpdateEnemyID(id);
		if (null == _enemyAutoAimSystem)
		{
			OrangeBattleUtility.AddEnemyAutoAimSystem(_transform, out _enemyAutoAimSystem);
		}
		_enemyAutoAimSystem.UpdateAimRange(EnemyWeapons[1].BulletData.f_DISTANCE);
	}

	public override void SetActive(bool isActive)
	{
		if (isActive)
		{
			InGame = true;
			_characterMaterial.Appear(delegate
			{
				Activate = true;
				Hp = EnemyData.n_HP;
				SetStatus(MainStatus.IDLE);
				Controller.enabled = true;
				SetColliderEnable(true);
				AiTimer.TimerStart();
				base.SetActive(Activate);
			});
			return;
		}
		if ((bool)_currentBullet)
		{
			_currentBullet.BackToPool();
		}
		Activate = false;
		MonoBehaviourSingleton<FxManager>.Instance.UpdateFx(FxArray, false);
		selfBuffManager.StopLoopSE();
		Controller.enabled = false;
		SetColliderEnable(false);
		AiTimer.TimerStop();
		LeanTween.cancel(base.gameObject);
		_characterMaterial.Disappear(delegate
		{
			InGame = false;
			MonoBehaviourSingleton<PoolManager>.Instance.BackToPool(this, EnemyData.s_MODEL);
		});
	}

	public void UpdateFunc()
	{
		if (Activate)
		{
			base.transform.localPosition = Vector3.MoveTowards(base.transform.localPosition, Controller.LogicPosition.vec3, distanceDelta);
		}
	}

	private void SetStatus(MainStatus mainStatus, SubStatus subStatus = SubStatus.Phase0)
	{
		_mainStatus = mainStatus;
		MainStatus mainStatus2 = _mainStatus;
		if (mainStatus2 == MainStatus.TURN)
		{
			_turnID = LeanTween.value(base.gameObject, (base.direction < 0) ? leftDir : rightDir, (base.direction > 0) ? leftDir : rightDir, 1f).setOnUpdate(delegate(float f)
			{
				ModelTransform.localEulerAngles = new Vector3(0f, f, 0f);
			}).setOnComplete((Action)delegate
			{
				_turnID = -1;
			})
				.uniqueId;
			PlaySE("EnemySE", 73, false, false);
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
		case MainStatus.TURN:
			if (_turnID == -1)
			{
				base.direction = -base.direction;
				SetStatus(MainStatus.IDLE);
			}
			break;
		case MainStatus.IDLE:
			if (AiTimer.GetMillisecond() < EnemyData.n_AI_TIMER)
			{
				break;
			}
			Target = _enemyAutoAimSystem.GetClosetPlayer();
			if (!(Target == null))
			{
				if (Math.Sign(Target._transform.position.x - _transform.position.x) != base.direction)
				{
					SetStatus(MainStatus.TURN);
				}
				else if (IsWeaponAvailable(1))
				{
					EnemyWeapons[1].MagazineRemain = EnemyWeapons[1].BulletData.n_MAGAZINE;
					SetStatus(MainStatus.SHOOT);
				}
			}
			break;
		case MainStatus.SHOOT:
		{
			int num = 1;
			if (EnemyWeapons[num].MagazineRemain > 0f && (!EnemyWeapons[num].LastUseTimer.IsStarted() || EnemyWeapons[num].LastUseTimer.GetMillisecond() > EnemyWeapons[num].BulletData.n_FIRE_SPEED))
			{
				EnemyWeapons[num].LastUseTimer.TimerStart();
				EnemyWeapons[num].MagazineRemain -= 1f;
			}
			if (_currentFrame > 0.5f && shootPhase == 0)
			{
				PlaySE("EnemySE", 67);
				_currentBullet = BulletBase.TryShotBullet(EnemyWeapons[num].BulletData, _shootTransform, Vector3.right * base.direction, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
				shootPhase = 1;
			}
			else if (_currentFrame > 0.75f && shootPhase == 1)
			{
				shootPhase = 2;
			}
			if (_currentFrame > 1.1f)
			{
				shootPhase = 5;
			}
			if (EnemyWeapons[num].MagazineRemain == 0f && shootPhase == 5)
			{
				SetStatus(MainStatus.IDLE);
				AiTimer.TimerStart();
				shootPhase = 0;
			}
			break;
		}
		default:
			SetStatus(MainStatus.IDLE);
			throw new ArgumentOutOfRangeException();
		case MainStatus.IdleWaitNet:
			break;
		}
		if (_mainStatus == MainStatus.SHOOT && shootPhase == 0)
		{
			_animator.Play(_animatorHash[(int)_mainStatus]);
		}
	}

	public override void UpdateStatus(int nSet, string sMSg, Callback tCB = null)
	{
		SetStatus((MainStatus)nSet);
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
		ModelTransform.localEulerAngles = new Vector3(0f, (base.direction > 0) ? rightDir : leftDir, 0f);
		base.transform.position = pos;
	}
}
