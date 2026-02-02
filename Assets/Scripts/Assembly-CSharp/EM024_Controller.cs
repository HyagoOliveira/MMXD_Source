using System;
using CallbackDefs;
using StageLib;
using UnityEngine;

public class EM024_Controller : EnemyControllerBase, IManagedUpdateBehavior
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

	private Transform _shootTransform;

	private int[] _animatorHash;

	private Transform _leftGunTransform;

	private Transform _rightGunTransform;

	private MainStatus _mainStatus;

	private float leftDir = -130f;

	private float rightDir = 90f;

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
		Transform[] target = _transform.GetComponentsInChildren<Transform>(true);
		ModelTransform = OrangeBattleUtility.FindChildRecursive(ref target, "model", true);
		base.AimTransform = OrangeBattleUtility.FindChildRecursive(ref target, "AimTransform");
		_animator = GetComponentInChildren<Animator>();
		_animatorHash = new int[5];
		for (int i = 0; i < 5; i++)
		{
			_animatorHash[i] = Animator.StringToHash("EM024@idle_loop");
		}
		_animatorHash[0] = Animator.StringToHash("EM024@idle_loop");
		_animatorHash[1] = Animator.StringToHash("EM024@hurt_loop");
		_animatorHash[3] = Animator.StringToHash("EM024@shot");
		_leftGunTransform = OrangeBattleUtility.FindChildRecursive(ref target, "ShootPoint_L");
		_rightGunTransform = OrangeBattleUtility.FindChildRecursive(ref target, "ShootPoint_R");
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
			MonoBehaviourSingleton<GameLogicUpdateManager>.Instance.RemoveUpdate(this);
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

	private void SetStatus(MainStatus mainStatus)
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
			base.SoundSource.PlaySE("EnemySE02", "em024_cndriver02");
		}
	}

	protected override void SetStunStatus(bool enable)
	{
		IsStunStatus = true;
		if (enable)
		{
			SetStatus(MainStatus.HURT);
		}
		else
		{
			SetStatus(MainStatus.IDLE);
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
			if (Target == null)
			{
				break;
			}
			if (Math.Sign(Target._transform.position.x - _transform.position.x) != base.direction)
			{
				SetStatus(MainStatus.TURN);
			}
			else
			{
				if (!IsWeaponAvailable(1))
				{
					break;
				}
				EnemyWeapons[1].MagazineRemain = EnemyWeapons[1].BulletData.n_MAGAZINE;
				if (StageUpdate.gbIsNetGame)
				{
					if (StageUpdate.bIsHost)
					{
						StageUpdate.RegisterSendAndRun(sNetSerialID, 1);
						SetStatus(MainStatus.IdleWaitNet);
					}
				}
				else
				{
					SetStatus(MainStatus.SHOOT);
				}
			}
			break;
		case MainStatus.SHOOT:
		{
			int num = 1;
			if (EnemyWeapons[num].MagazineRemain > 0f && (!EnemyWeapons[num].LastUseTimer.IsStarted() || EnemyWeapons[num].LastUseTimer.GetMillisecond() > EnemyWeapons[num].BulletData.n_FIRE_SPEED))
			{
				int num2 = (int)EnemyWeapons[num].MagazineRemain % 2;
				BulletBase.TryShotBullet(pTransform: (num2 != 0 && num2 == 1) ? _rightGunTransform : _leftGunTransform, tSkillTable: EnemyWeapons[num].BulletData, pDirection: Vector3.right * base.direction, weaponStatus: null, tBuffStatus: selfBuffManager.sBuffStatus, refMOB_TABLE: EnemyData, pTargetMask: targetMask);
				EnemyWeapons[num].LastUseTimer.TimerStart();
				EnemyWeapons[num].MagazineRemain -= 1f;
			}
			if (EnemyWeapons[num].MagazineRemain == 0f)
			{
				SetStatus(MainStatus.IDLE);
				AiTimer.TimerStart();
			}
			break;
		}
		default:
			SetStatus(MainStatus.IDLE);
			throw new ArgumentOutOfRangeException();
		case MainStatus.HURT:
		case MainStatus.IdleWaitNet:
			break;
		}
		_animator.Play(_animatorHash[(int)_mainStatus]);
	}

	public override void UpdateStatus(int nSet, string sMSg, Callback tCB = null)
	{
		if (nSet == 0)
		{
			SetStatus(MainStatus.SHOOT);
			return;
		}
		throw new ArgumentOutOfRangeException();
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
