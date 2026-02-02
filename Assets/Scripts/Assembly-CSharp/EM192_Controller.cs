using System;
using CallbackDefs;
using UnityEngine;

public class EM192_Controller : EnemyControllerBase, IManagedUpdateBehavior
{
	private enum MainStatus
	{
		Idle = 0,
		Die = 1
	}

	private enum SubStatus
	{
		Phase0 = 0,
		Phase1 = 1,
		Phase2 = 2,
		Phase3 = 3,
		MAX_SUBSTATUS = 4
	}

	private MainStatus _mainStatus;

	private SubStatus _subStatus;

	[SerializeField]
	private float TimeToDead = 1f;

	private int DeadFrame;

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
		_collideBullet = OrangeBattleUtility.FindChildRecursive(ref target, "Collider", true).gameObject.AddOrGetComponent<CollideBullet>();
		OrangeBattleUtility.AddEnemyAutoAimSystem(_transform, out _enemyAutoAimSystem);
		_enemyAutoAimSystem.SetColliderType(EnemyAutoAimSystem.ColliderType.Box);
		_enemyAutoAimSystem.UpdateAimRange(20f, 8f);
		base.AllowAutoAim = false;
	}

	protected override void Start()
	{
	}

	public override void UpdateStatus(int nSet, string smsg, Callback tCB = null)
	{
		bWaitNetStatus = false;
	}

	private void SetStatus(MainStatus mainStatus, SubStatus subStatus = SubStatus.Phase0)
	{
		_mainStatus = mainStatus;
		_subStatus = subStatus;
		switch (_mainStatus)
		{
		case MainStatus.Idle:
			_velocity.x = 0;
			break;
		case MainStatus.Die:
			DeadFrame = GameLogicUpdateManager.GameFrame + (int)(TimeToDead * 20f);
			break;
		}
		AiTimer.TimerStart();
	}

	public override void LogicUpdate()
	{
		if (Activate)
		{
			base.LogicUpdate();
			MainStatus mainStatus = _mainStatus;
			if (mainStatus != 0 && mainStatus == MainStatus.Die && GameLogicUpdateManager.GameFrame > DeadFrame)
			{
				Hp = 0;
				Hurt(new HurtPassParam());
			}
		}
	}

	protected override void DeadBehavior(ref HurtPassParam tHurtPassParam)
	{
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

	private void UpdateDirection(int forceDirection = 0)
	{
		if (forceDirection != 0)
		{
			base.direction = forceDirection;
		}
		ModelTransform.localScale = new Vector3(ModelTransform.localScale.x, ModelTransform.localScale.y, Mathf.Abs(ModelTransform.localScale.z) * (float)(-base.direction));
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
		IgnoreGravity = true;
		base.SetActive(isActive);
		bDeadShock = false;
		base.AllowAutoAim = false;
		if (isActive)
		{
			_collideBullet.UpdateBulletData(EnemyWeapons[0].BulletData);
			_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			_collideBullet.Active(targetMask);
			SetColliderEnable(false);
			SetStatus(MainStatus.Idle);
		}
		else
		{
			_collideBullet.BackToPool();
		}
		_characterMaterial.Appear(null, 0f);
	}

	public override void SetPositionAndRotation(Vector3 pos, bool back)
	{
		if (back)
		{
			base.direction = -1;
		}
		else
		{
			base.direction = 1;
		}
		ModelTransform.localEulerAngles = new Vector3(ModelTransform.localEulerAngles.x, -14 + (90 - 90 * base.direction), ModelTransform.localEulerAngles.z);
		ModelTransform.localPosition = Vector3.right * 0.56f * -base.direction;
		_transform.position = pos;
	}

	public void SetDead(bool bossdead = false)
	{
		MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("FX_MOB_EXPLODE0", ModelTransform, Quaternion.identity, Vector3.one, Array.Empty<object>());
		if (!bossdead)
		{
			_collideBullet.UpdateBulletData(EnemyWeapons[1].BulletData);
			_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			_collideBullet.Active(targetMask);
		}
		else
		{
			_collideBullet.BackToPool();
		}
		SetStatus(MainStatus.Die);
	}
}
