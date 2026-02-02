using System;
using CallbackDefs;
using Newtonsoft.Json;
using UnityEngine;

public class EM065_Controller : EnemyControllerBase, IManagedUpdateBehavior
{
	private enum MainStatus
	{
		Idle = 0,
		SK_1 = 1,
		Walk = 2,
		Ski = 3,
		Brake = 4,
		Explosion = 5,
		Hurt = 6
	}

	private enum SubStatus
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
		ANI_SKI = 2,
		ANI_HURT = 3,
		ANI_BRAKE = 4,
		ANI_SK_1 = 5,
		MAX_ANIMATION_ID = 6
	}

	private MainStatus _mainStatus;

	private SubStatus _subStatus;

	private AnimationID _currentAnimationId;

	private float _currentFrame;

	private int[] _animationHash;

	private bool _isSkiActive;

	private bool _isBrakeActive;

	private bool _isExplosion;

	public int WalkSpeed = 1750;

	public float SK_1_Distance = 20f;

	public float SpeedUpFrame = 1f;

	private Transform ExplosionCollide;

	private CollideBullet ExplosionBullet;

	private float RayLength = 5f;

	private float DeadFrame;

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
		ModelTransform = OrangeBattleUtility.FindChildRecursive(ref target, "model", true);
		ExplosionCollide = OrangeBattleUtility.FindChildRecursive(ref target, "ExplosionCollide", true);
		ExplosionBullet = ExplosionCollide.gameObject.AddOrGetComponent<CollideBullet>();
		_animationHash = new int[6];
		_animationHash[0] = Animator.StringToHash("EM065@idle_loop");
		_animationHash[1] = Animator.StringToHash("EM065@walk_loop");
		_animationHash[2] = Animator.StringToHash("EM065@run_loop");
		_animationHash[4] = Animator.StringToHash("EM065@brake");
		_animationHash[5] = Animator.StringToHash("EM065@skill_01");
		_animationHash[3] = Animator.StringToHash("EM065@hurt_loop");
		_mainStatus = MainStatus.Idle;
		_subStatus = SubStatus.Phase0;
		base.AimPoint = new Vector3(0f, 0.5f, 0f);
		OrangeBattleUtility.AddEnemyAutoAimSystem(_transform, out _enemyAutoAimSystem);
		_enemyAutoAimSystem.SetColliderType(EnemyAutoAimSystem.ColliderType.Box);
		_enemyAutoAimSystem.UpdateAimRange(20f, 8f);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("Explosion", 3);
		FallDownSE = new string[2] { "EnemySE02", "em035_knot" };
	}

	public override void UpdateStatus(int nSet, string smsg, Callback tCB = null)
	{
		bWaitNetStatus = false;
		if ((int)Hp <= 0)
		{
			return;
		}
		if (smsg != null && smsg != "")
		{
			NetSyncData netSyncData = JsonConvert.DeserializeObject<NetSyncData>(smsg);
			Controller.LogicPosition.x = netSyncData.SelfPosX;
			Controller.LogicPosition.y = netSyncData.SelfPosY;
			Controller.LogicPosition.z = netSyncData.SelfPosZ;
			TargetPos.x = netSyncData.TargetPosX;
			TargetPos.y = netSyncData.TargetPosY;
			TargetPos.z = netSyncData.TargetPosZ;
			if (netSyncData.bSetHP)
			{
				Hp = netSyncData.nHP;
			}
		}
		SetStatus((MainStatus)nSet);
	}

	private void UpdateDirection(int forceDirection = 0)
	{
		if (!_isSkiActive && !_isBrakeActive)
		{
			if (forceDirection != 0)
			{
				base.direction = forceDirection;
			}
			else if (TargetPos.x > Controller.LogicPosition.x)
			{
				base.direction = 1;
			}
			else
			{
				base.direction = -1;
			}
			ModelTransform.localScale = new Vector3(ModelTransform.localScale.x, ModelTransform.localScale.y, Mathf.Abs(ModelTransform.localScale.z) * (float)(-base.direction));
		}
	}

	private void SetStatus(MainStatus mainStatus, SubStatus subStatus = SubStatus.Phase0)
	{
		_mainStatus = mainStatus;
		_subStatus = subStatus;
		switch (_mainStatus)
		{
		case MainStatus.Idle:
			_isSkiActive = false;
			_isBrakeActive = false;
			_velocity.x = 0;
			break;
		case MainStatus.Brake:
			_isBrakeActive = true;
			break;
		case MainStatus.SK_1:
			_velocity.x = 0;
			break;
		case MainStatus.Explosion:
			_isExplosion = false;
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
			_currentAnimationId = AnimationID.ANI_IDLE;
			break;
		case MainStatus.Walk:
			_currentAnimationId = AnimationID.ANI_WALK;
			break;
		case MainStatus.Ski:
			_currentAnimationId = AnimationID.ANI_SKI;
			break;
		case MainStatus.Brake:
			_currentAnimationId = AnimationID.ANI_BRAKE;
			break;
		case MainStatus.SK_1:
			_currentAnimationId = AnimationID.ANI_SK_1;
			break;
		case MainStatus.Explosion:
			_currentAnimationId = AnimationID.ANI_HURT;
			break;
		}
		_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
	}

	public override void LogicUpdate()
	{
		if (!Activate)
		{
			return;
		}
		base.LogicUpdate();
		_currentFrame = _animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
		MainStatus mainStatus = MainStatus.Idle;
		switch (_mainStatus)
		{
		case MainStatus.Idle:
			Target = _enemyAutoAimSystem.GetClosetPlayer();
			if ((bool)Target && !OrangeBattleUtility.RaycastIgnoreSelf(Controller.GetCenterPos(), Vector2.right * base.direction, RayLength, Controller.collisionMask, _transform) && Mathf.Abs(Target._transform.position.x - _transform.position.x) < SK_1_Distance)
			{
				mainStatus = MainStatus.SK_1;
			}
			break;
		case MainStatus.Ski:
			if ((bool)OrangeBattleUtility.RaycastIgnoreSelf(Controller.GetCenterPos() + Vector3.down * 0.3f, Vector2.right * base.direction, RayLength, Controller.collisionMask, _transform) || Controller.Collisions.left || Controller.Collisions.right)
			{
				if (_velocity.x * base.direction > 6000)
				{
					mainStatus = MainStatus.Explosion;
				}
				break;
			}
			Target = _enemyAutoAimSystem.GetClosetPlayer();
			if ((bool)Target)
			{
				_isSkiActive = (((Target._transform.position.x - _transform.position.x) * (float)base.direction > 0f) ? true : false);
			}
			else
			{
				_isSkiActive = true;
			}
			if (_isSkiActive)
			{
				_velocity.x += (int)((float)(base.direction * WalkSpeed) * 0.1f);
			}
			break;
		case MainStatus.Walk:
			Target = _enemyAutoAimSystem.GetClosetPlayer();
			if ((bool)Target)
			{
				_isSkiActive = (((Target._transform.position.x - _transform.position.x) * (float)base.direction > 0f) ? true : false);
			}
			else
			{
				_isSkiActive = true;
			}
			_velocity.x += (int)((float)(base.direction * WalkSpeed) * 0.2f);
			if (_currentFrame > SpeedUpFrame)
			{
				SetStatus(MainStatus.Ski);
			}
			if (CheckMoveFall(_velocity))
			{
				SetStatus(MainStatus.Idle);
			}
			break;
		case MainStatus.Brake:
			_velocity.x -= (int)((float)(base.direction * WalkSpeed) * 0.3f);
			if (_velocity.x * base.direction < 0)
			{
				_velocity.x = 0;
				SetStatus(MainStatus.Idle);
			}
			break;
		case MainStatus.SK_1:
			if (_currentFrame > 1f)
			{
				SetStatus(MainStatus.Walk);
			}
			break;
		case MainStatus.Explosion:
			if (!_isExplosion && (Controller.Collisions.left || Controller.Collisions.right))
			{
				_velocity.x = 0;
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("FX_MOB_EXPLODE0", ExplosionCollide.transform, Quaternion.identity, new Vector3(1.5f, 1.5f, 1.5f), Array.Empty<object>());
				ExplosionBullet.Active(targetMask);
				ModelTransform.localScale = new Vector3(0f, 0f, 0f);
				HurtPassParam hurtPassParam = new HurtPassParam();
				hurtPassParam.dmg = MaxHp;
				Hurt(hurtPassParam);
				_isExplosion = true;
			}
			break;
		}
		if (mainStatus != 0 && CheckHost())
		{
			UploadEnemyStatus((int)mainStatus);
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
		_animator.enabled = isActive;
		if (isActive)
		{
			_isSkiActive = false;
			_isBrakeActive = false;
			_isExplosion = false;
			ModelTransform.localScale = new Vector3(1f, 1f, base.direction);
			SetStatus(MainStatus.Idle);
			base.AllowAutoAim = true;
			ExplosionBullet.UpdateBulletData(EnemyWeapons[1].BulletData);
			ExplosionBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			ExplosionBullet.BackToPool();
			_collideBullet.UpdateBulletData(EnemyWeapons[0].BulletData);
			_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			_collideBullet.Active(targetMask);
		}
		else
		{
			_collideBullet.BackToPool();
			_isSkiActive = false;
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
		ModelTransform.localScale = new Vector3(_transform.localScale.x, _transform.localScale.y, _transform.localScale.z * (float)base.direction);
		ModelTransform.localEulerAngles = new Vector3(0f, 90 + 15 * base.direction, 0f);
		_transform.position = pos;
	}

	protected override void DeadBehavior(ref HurtPassParam tHurtPassParam)
	{
		if (!_isExplosion)
		{
			Explosion();
		}
		PlaySE(ExplodeSE[0], ExplodeSE[1]);
		BackToPool();
		StageObjParam component = GetComponent<StageObjParam>();
		if (component != null && component.nEventID != 0)
		{
			EventManager.StageEventCall stageEventCall = new EventManager.StageEventCall();
			stageEventCall.nID = component.nEventID;
			component.nEventID = 0;
			Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_EVENT_CALL, stageEventCall);
		}
	}
}
