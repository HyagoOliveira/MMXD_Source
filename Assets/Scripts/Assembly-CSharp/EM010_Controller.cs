using System;
using CallbackDefs;
using StageLib;
using UnityEngine;

public class EM010_Controller : EnemyControllerBase, IManagedUpdateBehavior
{
	private enum MainStatus
	{
		IDLE = 0,
		HURT = 1,
		RUN = 2,
		JUMP_START = 3,
		JUMP_LOOP = 4,
		LAND = 5,
		SLASH = 6,
		IdleWaitNet = 7,
		MAX_STATUS = 8
	}

	public int RunSpeed = 10000;

	public int JumpSpeed = 10000;

	private readonly int _hashHspd = Animator.StringToHash("fHspd");

	private readonly int _hashVspd = Animator.StringToHash("fVspd");

	private Transform _shootTransform;

	private int[] _animatorHash;

	private CollideBullet _mouthCollideBullet;

	private CollideBullet _bodyCollideBullet;

	private MainStatus _mainStatus;

	private float _currentFrame;

	private int logictimes;

	private int updatetimes;

	private Transform ocTrans;

	private bool useDebutSE = true;

	private bool _jumpFlag;

	public float _delay = 0.6f;

	private Vector3 _distance;

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
		EnemyID = 0;
		base.Awake();
		Transform[] target = _transform.GetComponentsInChildren<Transform>(true);
		ModelTransform = OrangeBattleUtility.FindChildRecursive(ref target, "model", true);
		base.AimTransform = OrangeBattleUtility.FindChildRecursive(ref target, "ShootPoint_Body");
		_bodyCollideBullet = OrangeBattleUtility.FindChildRecursive(ref target, "ShootPoint_Body").gameObject.AddOrGetComponent<CollideBullet>();
		_mouthCollideBullet = OrangeBattleUtility.FindChildRecursive(ref target, "ShootPoint_Mouth").gameObject.AddOrGetComponent<CollideBullet>();
		_animator = GetComponentInChildren<Animator>();
		_animatorHash = new int[8];
		for (int i = 0; i < 8; i++)
		{
			_animatorHash[i] = Animator.StringToHash("idle");
		}
		_animatorHash[0] = Animator.StringToHash("EM010@idle_loop");
		_animatorHash[1] = Animator.StringToHash("EM010@hurt_loop");
		_animatorHash[2] = Animator.StringToHash("EM010@run_loop");
		_animatorHash[3] = Animator.StringToHash("EM010@skill_03_jump_start");
		_animatorHash[4] = Animator.StringToHash("EM010@skill_03_jump_loop");
		_animatorHash[5] = Animator.StringToHash("EM010@skill_03_landing");
		_animatorHash[6] = Animator.StringToHash("EM010@skill_02");
	}

	public override void UpdateEnemyID(int id)
	{
		base.UpdateEnemyID(id);
		if (null == _enemyAutoAimSystem)
		{
			OrangeBattleUtility.AddEnemyAutoAimSystem(_transform, out _enemyAutoAimSystem);
		}
		_enemyAutoAimSystem.UpdateAimRange(15f);
	}

	public override void SetActive(bool isActive)
	{
		if (isActive)
		{
			InGame = true;
			_characterMaterial.Appear(delegate
			{
				_bodyCollideBullet.UpdateBulletData(EnemyWeapons[0].BulletData);
				_bodyCollideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
				_bodyCollideBullet.isForceSE = true;
				_bodyCollideBullet.Active(targetMask);
				_mouthCollideBullet.UpdateBulletData(EnemyWeapons[1].BulletData);
				_mouthCollideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
				Hp = EnemyData.n_HP;
				SetStatus(MainStatus.IDLE);
				Controller.enabled = true;
				SetColliderEnable(true);
				AiTimer.TimerStart();
				useDebutSE = true;
				InGame = true;
				_transform.SetParent(null);
				Controller.LogicPosition = new VInt3(_transform.localPosition);
				_velocityExtra = VInt3.zero;
				MonoBehaviourSingleton<GameLogicUpdateManager>.Instance.AddUpdate(this);
				MonoBehaviourSingleton<FxManager>.Instance.UpdateFx(FxArray, true);
				Activate = ManagedSingleton<StageHelper>.Instance.bEnemyActive;
				if (visible)
				{
					useDebutSE = false;
				}
				if (useDebutSE)
				{
					ocTrans = StageUpdate.GetMainPlayerTrans();
					float num = ((!ocTrans) ? 13f : Vector2.Distance(_transform.position, ocTrans.position));
					if (num < 12f)
					{
						PlaySE("EnemySE", 70, true);
						useDebutSE = false;
					}
				}
			});
		}
		else
		{
			Activate = false;
			MonoBehaviourSingleton<FxManager>.Instance.UpdateFx(FxArray, false);
			selfBuffManager.StopLoopSE();
			Controller.enabled = false;
			SetColliderEnable(false);
			AiTimer.TimerStop();
			_bodyCollideBullet.BackToPool();
			_mouthCollideBullet.BackToPool();
			_velocity = VInt3.zero;
			LeanTween.cancel(base.gameObject);
			_characterMaterial.Disappear(delegate
			{
				InGame = false;
				MonoBehaviourSingleton<GameLogicUpdateManager>.Instance.RemoveUpdate(this);
				MonoBehaviourSingleton<PoolManager>.Instance.BackToPool(this, EnemyData.s_MODEL);
			});
		}
	}

	public void UpdateFunc()
	{
		if (Activate)
		{
			MainStatus mainStatus = _mainStatus;
			if (mainStatus == MainStatus.JUMP_START && (double)_currentFrame >= 0.43 && !_jumpFlag)
			{
				_jumpFlag = true;
				_velocity.y = JumpSpeed;
			}
			_animator.SetFloat(_hashHspd, (float)_velocity.x * 0.001f);
			_animator.SetFloat(_hashVspd, (float)_velocity.y * 0.001f);
			base.transform.localPosition = Vector3.MoveTowards(base.transform.localPosition, Controller.LogicPosition.vec3, distanceDelta);
		}
	}

	private void SetStatus(MainStatus mainStatus)
	{
		_mainStatus = mainStatus;
		switch (_mainStatus)
		{
		case MainStatus.IDLE:
			_bodyCollideBullet.UpdateBulletData(EnemyWeapons[0].BulletData);
			_bodyCollideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			_mouthCollideBullet.BackToPool();
			break;
		case MainStatus.RUN:
			_velocity.x = base.direction * RunSpeed;
			break;
		case MainStatus.JUMP_START:
			_bodyCollideBullet.UpdateBulletData(EnemyWeapons[1].BulletData);
			_bodyCollideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			_bodyCollideBullet.BackToPool();
			_bodyCollideBullet.Active(targetMask);
			_jumpFlag = false;
			break;
		case MainStatus.LAND:
			_jumpFlag = false;
			LeanTween.value(base.gameObject, _velocity.x, 0f, _delay).setOnUpdate(delegate(float f)
			{
				_velocity.x = (int)f;
			}).setOnComplete((Action)delegate
			{
				_jumpFlag = true;
			});
			break;
		case MainStatus.SLASH:
			_velocity.x = 0;
			break;
		case MainStatus.HURT:
		case MainStatus.JUMP_LOOP:
		case MainStatus.IdleWaitNet:
			break;
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
		if (visible)
		{
			useDebutSE = false;
		}
		float num = ((!ocTrans) ? 13f : Vector2.Distance(_transform.position, ocTrans.position));
		if (useDebutSE && num < 12f && !visible)
		{
			PlaySE("EnemySE", 70, true);
			useDebutSE = false;
		}
		switch (_mainStatus)
		{
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
					base.direction = -base.direction;
					ModelTransform.localScale = new Vector3(ModelTransform.localScale.x, ModelTransform.localScale.y, Mathf.Abs(ModelTransform.localScale.z) * (float)base.direction);
				}
				_distance = _transform.position - Target._transform.position;
				if (Math.Abs(_distance.x) < 2.3f && Math.Abs(_distance.y) < 2f && Controller.Collisions.below)
				{
					SetStatus(MainStatus.SLASH);
				}
				else
				{
					SetStatus(MainStatus.RUN);
				}
			}
			break;
		case MainStatus.SLASH:
			if (_currentFrame >= 0.28f && !_mouthCollideBullet.IsActivate)
			{
				_mouthCollideBullet.transform.localScale = Vector3.one;
				_mouthCollideBullet.Active(targetMask);
			}
			if (_currentFrame >= 1f)
			{
				SetStatus(MainStatus.IDLE);
			}
			break;
		case MainStatus.RUN:
			Target = _enemyAutoAimSystem.GetClosetPlayer();
			if (Target == null)
			{
				SetStatus(MainStatus.LAND);
				break;
			}
			if (Math.Sign(Target._transform.position.x - _transform.position.x) != base.direction)
			{
				SetStatus(MainStatus.LAND);
			}
			_distance = _transform.position - Target._transform.position;
			if (Math.Abs(_distance.x) < 3f && Math.Abs(_distance.y) < 2f)
			{
				SetStatus(MainStatus.JUMP_START);
			}
			break;
		case MainStatus.JUMP_START:
			if (_currentFrame >= 1f)
			{
				SetStatus(MainStatus.JUMP_LOOP);
			}
			break;
		case MainStatus.JUMP_LOOP:
			if ((bool)Controller.BelowInBypassRange)
			{
				SetStatus(MainStatus.LAND);
			}
			break;
		case MainStatus.LAND:
			if (_currentFrame >= 1f && _jumpFlag)
			{
				SetStatus(MainStatus.IDLE);
			}
			break;
		default:
			SetStatus(MainStatus.IDLE);
			break;
		case MainStatus.IdleWaitNet:
			break;
		}
		_animator.Play(_animatorHash[(int)_mainStatus]);
	}

	public override void UpdateStatus(int nSet, string sMSg, Callback tCB = null)
	{
		if (nSet != 0)
		{
			throw new ArgumentOutOfRangeException();
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
		ModelTransform.localScale = new Vector3(ModelTransform.localScale.x, ModelTransform.localScale.y, Mathf.Abs(ModelTransform.localScale.z) * (float)base.direction);
		base.transform.position = pos;
	}
}
