using System;
using StageLib;
using UnityEngine;

public class EM170_Controller : EnemyControllerBase, IManagedUpdateBehavior
{
	protected enum MainStatus
	{
		Idle = 0,
		Walk = 1,
		Shoot = 2,
		EventIdle = 3,
		IdleWaitNet = 4
	}

	protected enum SubStatus
	{
		Phase0 = 0,
		Phase1 = 1,
		Phase2 = 2,
		Phase3 = 3,
		MAX_SUBSTATUS = 4
	}

	public enum AnimationID
	{
		ANI_IDLE = 0,
		ANI_WALK = 1,
		ANI_SHOOT = 2,
		ANI_HURT = 3,
		MAX_ANIMATION_ID = 4
	}

	public int WalkSpeed = 3000;

	protected float _shootRange = 5f;

	protected Transform _shieldTransform;

	protected Transform _shootTransform;

	protected MainStatus _mainStatus;

	protected SubStatus _subStatus;

	protected AnimationID _currentAnimationId;

	protected float _currentFrame;

	protected int[] _animationHash;

	protected bool _shootDone;

	[SerializeField]
	private float BornWaitTime = 2f;

	private int BornWaitFrame;

	[SerializeField]
	private bool CanMove;

	private int AngryFrame;

	private int WaitAngryFrame;

	private Transform LineUpObj;

	private ObjInfoBar InfoBar;

	private int EventID = -1;

	protected virtual void UpdateDirection(int forceDirection = 0)
	{
		if (!IsStun)
		{
			if (forceDirection != 0)
			{
				base.direction = forceDirection;
			}
			else if (OrangeBattleUtility.CurrentCharacter != null && OrangeBattleUtility.CurrentCharacter.transform.position.x > _transform.position.x)
			{
				base.direction = 1;
			}
			else
			{
				base.direction = -1;
			}
			ModelTransform.localScale = new Vector3(ModelTransform.localScale.x, ModelTransform.localScale.y, Mathf.Abs(ModelTransform.localScale.z) * (float)base.direction);
		}
	}

	protected void SetStatus(MainStatus mainStatus, SubStatus subStatus = SubStatus.Phase0)
	{
		_mainStatus = mainStatus;
		_subStatus = subStatus;
		switch (_mainStatus)
		{
		case MainStatus.Idle:
			_velocity.x = 0;
			_shieldTransform.gameObject.SetActive(true);
			break;
		case MainStatus.Walk:
			_velocity.x = base.direction * WalkSpeed;
			_shieldTransform.gameObject.SetActive(true);
			break;
		case MainStatus.Shoot:
			_velocity.x = 0;
			_shieldTransform.gameObject.SetActive(false);
			_shootDone = false;
			break;
		case MainStatus.EventIdle:
			WaitAngryFrame = GameLogicUpdateManager.GameFrame + AngryFrame;
			if ((bool)LineUpObj)
			{
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_lineup_angry_frame", LineUpObj, Quaternion.identity, Array.Empty<object>());
			}
			break;
		}
		AiTimer.TimerStart();
		UpdateAnimation();
	}

	private void UpdateAnimation()
	{
		switch (_mainStatus)
		{
		case MainStatus.Idle:
		case MainStatus.EventIdle:
			_currentAnimationId = AnimationID.ANI_IDLE;
			break;
		case MainStatus.Walk:
			_currentAnimationId = AnimationID.ANI_WALK;
			break;
		case MainStatus.Shoot:
			_currentAnimationId = AnimationID.ANI_SHOOT;
			break;
		}
		_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
	}

	protected void OnEnable()
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
		_shieldTransform = OrangeBattleUtility.FindChildRecursive(ref target, "ShieldPoint", true);
		_shieldTransform.gameObject.AddOrGetComponent<StageObjParam>().nSubPartID = 1;
		GuardTransform.Add(1);
		LineUpObj = OrangeBattleUtility.FindChildRecursive(base.transform, "LineUp");
		_shootTransform = OrangeBattleUtility.FindChildRecursive(ref target, "ShootPoint", true);
		_animator = GetComponentInChildren<Animator>();
		OrangeBattleUtility.AddEnemyAutoAimSystem(_transform, out _enemyAutoAimSystem);
		_enemyAutoAimSystem.UpdateAimRange(10f);
		base.AimPoint = Vector3.up * 0.8f;
		_animationHash = new int[4];
		_animationHash[0] = Animator.StringToHash("EM027@idle_loop");
		_animationHash[1] = Animator.StringToHash("EM027@run_loop");
		_animationHash[2] = Animator.StringToHash("EM027@skill_01");
		_animationHash[3] = Animator.StringToHash("EM027@hurt_loop");
		_mainStatus = MainStatus.Idle;
		_subStatus = SubStatus.Phase0;
		AiTimer.TimerStart();
	}

	protected override void Start()
	{
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_lineup_angry_frame", 3);
	}

	public override void LogicUpdate()
	{
		if (!Activate)
		{
			return;
		}
		base.LogicUpdate();
		if (GameLogicUpdateManager.GameFrame < BornWaitFrame)
		{
			return;
		}
		_currentFrame = _animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
		switch (_mainStatus)
		{
		case MainStatus.Idle:
			if (!CanMove)
			{
				if (EnemyWeapons[0].LastUseTimer.GetMillisecond() > EnemyWeapons[0].BulletData.n_RELOAD)
				{
					SetStatus(MainStatus.Shoot);
				}
				break;
			}
			Target = _enemyAutoAimSystem.GetClosetPlayer();
			if (!Target)
			{
				break;
			}
			UpdateDirection();
			if (Mathf.Abs(Target._transform.position.x - _transform.position.x) < _shootRange)
			{
				if (EnemyWeapons[0].LastUseTimer.GetMillisecond() > EnemyWeapons[0].BulletData.n_RELOAD)
				{
					SetStatus(MainStatus.Shoot);
				}
			}
			else if (!CheckMoveFall(_velocity + VInt3.signRight * base.direction * WalkSpeed))
			{
				SetStatus(MainStatus.Walk);
			}
			break;
		case MainStatus.Shoot:
			if (_currentFrame >= 1f)
			{
				SetStatus(MainStatus.Idle);
			}
			if (_currentFrame > 0.46f && !_shootDone)
			{
				_shootDone = true;
				EnemyWeapons[0].LastUseTimer.TimerStart();
				BulletBase.TryShotBullet(EnemyWeapons[0].BulletData, _shootTransform, base.direction * Vector3.right, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
			}
			break;
		case MainStatus.Walk:
			Target = _enemyAutoAimSystem.GetClosetPlayer();
			if ((bool)Target)
			{
				UpdateDirection();
				_velocity.x = base.direction * WalkSpeed;
				if (CheckMoveFall(_velocity))
				{
					SetStatus(MainStatus.Idle);
				}
				if (Mathf.Abs(Target._transform.position.x - _transform.position.x) < _shootRange)
				{
					SetStatus((EnemyWeapons[0].LastUseTimer.GetMillisecond() > EnemyWeapons[0].BulletData.n_RELOAD) ? MainStatus.Shoot : MainStatus.Idle);
				}
			}
			else
			{
				SetStatus(MainStatus.Idle);
			}
			break;
		case MainStatus.EventIdle:
			if (WaitAngryFrame < GameLogicUpdateManager.GameFrame)
			{
				if ((bool)InfoBar)
				{
					InfoBar.gameObject.SetActive(true);
				}
				SetColliderEnable(true);
				base.AllowAutoAim = true;
				SetStatus(MainStatus.Idle);
			}
			break;
		}
	}

	public override void FalldownUpdate()
	{
		if (!isFall)
		{
			if (IsStun)
			{
				isFall = !preBelow;
			}
			else
			{
				isFall = !Controller.Collisions.below;
			}
		}
		else if (Controller.Collisions.below)
		{
			isFall = false;
			PlaySE("EnemySE", "em007_preon02");
		}
	}

	public void UpdateFunc()
	{
		if (Activate)
		{
			base.transform.localPosition = Vector3.MoveTowards(base.transform.localPosition, Controller.LogicPosition.vec3, distanceDelta);
		}
	}

	public override void SetActive(bool isActive)
	{
		base.SetActive(isActive);
		if (isActive)
		{
			Singleton<GenericEventManager>.Instance.AttachEvent<EventManager.StageEventCall>(EventManager.ID.STAGE_EVENT_CALL, StartMove);
			InfoBar = _transform.GetComponentInChildren<ObjInfoBar>();
			if ((bool)InfoBar)
			{
				InfoBar.gameObject.SetActive(false);
			}
			CanMove = false;
			BornWaitFrame = GameLogicUpdateManager.GameFrame + (int)(BornWaitTime * 20f);
		}
		else
		{
			Singleton<GenericEventManager>.Instance.DetachEvent<EventManager.StageEventCall>(EventManager.ID.STAGE_EVENT_CALL, StartMove);
		}
	}

	public override void UpdateEnemyID(int id)
	{
		base.UpdateEnemyID(id);
		EnemyWeapons[0].LastUseTimer += (float)(EnemyWeapons[0].BulletData.n_RELOAD + 1);
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
		base.transform.position = pos;
		UpdateDirection(base.direction);
	}

	public void StartMove(EventManager.StageEventCall tStageEventCall)
	{
		if (EventID == -1 || tStageEventCall.nID != EventID)
		{
			return;
		}
		CanMove = true;
		for (int i = 0; i < StageUpdate.runEnemys.Count; i++)
		{
			if (StageUpdate.runEnemys[i].mEnemy.gameObject.GetInstanceID() == base.gameObject.GetInstanceID())
			{
				StageUpdate.runEnemys[i].nEnemyBitParam = StageUpdate.runEnemys[i].nEnemyBitParam | 1;
			}
		}
		SetStatus(MainStatus.EventIdle);
	}

	public override void SetEventCtrlID(int eventid)
	{
		EventID = eventid;
	}
}
