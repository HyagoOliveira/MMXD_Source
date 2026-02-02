using CallbackDefs;
using StageLib;
using UnityEngine;

public class EM049_Controller : EnemyControllerBase, IManagedUpdateBehavior
{
	private enum MainStatus
	{
		Idle = 0,
		Empty_Idle = 1,
		Skill_0 = 2,
		IdleWaitNet = 3
	}

	private enum SubStatus
	{
		Phase0 = 0,
		Phase1 = 1,
		Phase2 = 2,
		Phase3 = 3,
		Phase4 = 4,
		Phase5 = 5
	}

	public enum AnimationID
	{
		ANI_IDLE = 0,
		ANI_EMPTY_IDLE = 1,
		ANI_SKILL_0 = 2,
		ANI_SKILL_1 = 3,
		ANI_SKILL_2 = 4,
		ANI_SKILL_3 = 5,
		ANI_SKILL_4 = 6,
		ANI_SKILL_5 = 7,
		MAX_ANIMATION_ID = 8
	}

	private Transform _shootPoint;

	private MainStatus _mainStatus;

	private SubStatus _subStatus;

	private AnimationID _currentAnimationId;

	private float _currentFrame;

	private int[] _animationHash;

	private float _animatorSpeed = 1f;

	private int _chargeOilTime = 3000;

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
		_animator = GetComponentInChildren<Animator>();
		_collideBullet = base.gameObject.AddOrGetComponent<CollideBullet>();
		_shootPoint = OrangeBattleUtility.FindChildRecursive(_transform, "ShootPoint", true);
		_globalWaypoints = new float[2];
		base.AimPoint = new Vector3(0.2f, 0.3f, 0f);
		OrangeBattleUtility.AddEnemyAutoAimSystem(_transform, out _enemyAutoAimSystem);
		_enemyAutoAimSystem.UpdateAimRange(10f);
		_animationHash = new int[8];
		_animationHash[0] = Animator.StringToHash("EM049@idle_full_loop");
		_animationHash[1] = Animator.StringToHash("EM049@idle_empty_loop");
		_animationHash[2] = Animator.StringToHash("EM049@atk_fuel_0_loop");
		_animationHash[3] = Animator.StringToHash("EM049@atk_fuel_1_loop");
		_animationHash[4] = Animator.StringToHash("EM049@atk_fuel_2_loop");
		_animationHash[5] = Animator.StringToHash("EM049@atk_fuel_3_loop");
		_animationHash[6] = Animator.StringToHash("EM049@atk_fuel_4_loop");
		_animationHash[7] = Animator.StringToHash("EM049@atk_fuel_5_loop");
		_mainStatus = MainStatus.Idle;
		_subStatus = SubStatus.Phase0;
		AiTimer.TimerStart();
	}

	public override void UpdateStatus(int nSet, string smsg, Callback tCB = null)
	{
		SetStatus((MainStatus)nSet);
	}

	private void UpdateRandomSetate()
	{
		if (StageUpdate.gbIsNetGame)
		{
			if (StageUpdate.bIsHost)
			{
				StageUpdate.RegisterSendAndRun(sNetSerialID, 2);
				_mainStatus = MainStatus.IdleWaitNet;
			}
		}
		else
		{
			SetStatus(MainStatus.Skill_0);
		}
	}

	public override void LogicUpdate()
	{
		if (!Activate)
		{
			return;
		}
		base.LogicUpdate();
		_animator.speed = _animatorSpeed;
		_currentFrame = _animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
		switch (_mainStatus)
		{
		case MainStatus.Idle:
			Target = _enemyAutoAimSystem.GetClosetPlayer();
			if ((bool)Target && AiTimer.GetMillisecond() > EnemyData.n_AI_TIMER)
			{
				UpdateRandomSetate();
			}
			break;
		case MainStatus.Empty_Idle:
			if (AiTimer.GetMillisecond() >= _chargeOilTime)
			{
				SetStatus(MainStatus.Idle);
			}
			break;
		case MainStatus.Skill_0:
			if (EnemyWeapons[1].MagazineRemain > 0f && (!EnemyWeapons[1].LastUseTimer.IsStarted() || EnemyWeapons[1].LastUseTimer.GetMillisecond() > EnemyWeapons[1].BulletData.n_FIRE_SPEED))
			{
				((SprayBullet)BulletBase.TryShotBullet(EnemyWeapons[1].BulletData, _shootPoint, Vector3.right * base.direction, null, selfBuffManager.sBuffStatus, EnemyData, targetMask)).needPlayEndSE = true;
				EnemyWeapons[1].LastUseTimer.TimerStart();
				EnemyWeapons[1].MagazineRemain -= 1f;
			}
			if (_currentFrame > 1f && EnemyWeapons[1].MagazineRemain == 0f)
			{
				SetStatus(MainStatus.Empty_Idle);
			}
			break;
		case MainStatus.IdleWaitNet:
			break;
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
			_collideBullet.UpdateBulletData(EnemyWeapons[0].BulletData);
			_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			_collideBullet.Active(targetMask);
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
		ModelTransform.localScale = new Vector3(ModelTransform.localScale.x, ModelTransform.localScale.y, Mathf.Abs(ModelTransform.localScale.z) * (float)base.direction);
		base.transform.position = pos;
	}

	private void SetStatus(MainStatus mainStatus, SubStatus subStatus = SubStatus.Phase0)
	{
		_mainStatus = mainStatus;
		_subStatus = subStatus;
		switch (_mainStatus)
		{
		case MainStatus.Empty_Idle:
			_chargeOilTime = EnemyWeapons[1].BulletData.n_RELOAD;
			break;
		case MainStatus.Skill_0:
			if (_subStatus == SubStatus.Phase0)
			{
				EnemyWeapons[1].MagazineRemain = EnemyWeapons[1].BulletData.n_MAGAZINE;
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
			_currentAnimationId = AnimationID.ANI_IDLE;
			_animatorSpeed = 1f;
			break;
		case MainStatus.Empty_Idle:
			_currentAnimationId = AnimationID.ANI_EMPTY_IDLE;
			_animatorSpeed = 1f;
			break;
		case MainStatus.Skill_0:
			if (_subStatus == SubStatus.Phase0)
			{
				_currentAnimationId = AnimationID.ANI_SKILL_5;
				_animatorSpeed = 7000f / (float)(EnemyWeapons[1].BulletData.n_FIRE_SPEED * (EnemyWeapons[1].BulletData.n_MAGAZINE - 1));
			}
			break;
		}
		_animator.speed = _animatorSpeed;
		_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
	}
}
