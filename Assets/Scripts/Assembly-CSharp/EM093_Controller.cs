using CallbackDefs;
using CodeStage.AntiCheat.ObscuredTypes;
using StageLib;
using UnityEngine;

public class EM093_Controller : EnemyControllerBase, IManagedUpdateBehavior
{
	private Transform FlashLight;

	private Transform BulletBody;

	private bool _isMoving = true;

	private int DestoryFrame;

	private int DestoryTime = 15;

	private BS049_Controller _parentController;

	private VInt3 GlobalVelocity;

	private bool _isDead;

	private bool _isLive;

	private CollideBullet ExplosionBullet;

	private bool vertical;

	private bool horizon;

	public bool _needExp;

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
		_collideBullet = base.gameObject.AddOrGetComponent<CollideBullet>();
		Transform[] target = _transform.GetComponentsInChildren<Transform>(true);
		ModelTransform = OrangeBattleUtility.FindChildRecursive(ref target, "model", true);
		ExplosionBullet = ModelTransform.gameObject.AddOrGetComponent<CollideBullet>();
		FlashLight = OrangeBattleUtility.FindChildRecursive(ref target, "glow01", true);
		BulletBody = OrangeBattleUtility.FindChildRecursive(ref target, "p_MANDARELA-BB_000-FBX", true);
		base.AimPoint = new Vector3(0f, 0f, 0f);
		OrangeBattleUtility.AddEnemyAutoAimSystem(_transform, out _enemyAutoAimSystem);
		_enemyAutoAimSystem.SetColliderType(EnemyAutoAimSystem.ColliderType.Box);
		_enemyAutoAimSystem.UpdateAimRange(0f, 0f);
		base.AllowAutoAim = false;
	}

	public override void UpdateStatus(int nSet, string smsg, Callback tCB = null)
	{
		bWaitNetStatus = false;
	}

	public override void LogicUpdate()
	{
		if ((GameLogicUpdateManager.GameFrame > DestoryFrame && _isLive) || _needExp)
		{
			Hp = 0;
			Hurt(new HurtPassParam());
		}
		else if (GameLogicUpdateManager.GameFrame > DestoryFrame && _isDead)
		{
			ExplosionBullet.BackToPool();
			BackToPool();
			return;
		}
		if (!Activate)
		{
			return;
		}
		base.LogicUpdate();
		if (CheckHost())
		{
			UploadEnemyStatus(0);
		}
		if (_isMoving && !_isDead)
		{
			ModelTransform.Rotate(new Vector3(0f, 0f, 10f));
			if (Controller.Collisions.left)
			{
				horizon = true;
				GlobalVelocity = VInt3.signLeft * 2000;
				ModelTransform.localPosition = new Vector3(-0.25f, 0f, 0f);
				OrangeBattleUtility.GlobalVelocityExtra += GlobalVelocity;
				_enemyCollider[0].SetSize(new Vector2(0.2f, 0.5f));
				horizon = true;
				SetCohere(new Vector3(0f, -90f, 0f));
			}
			else if (Controller.Collisions.above)
			{
				vertical = true;
				GlobalVelocity = VInt3.signUp * 1000;
				ModelTransform.localPosition = new Vector3(0f, 0.25f, 0f);
				OrangeBattleUtility.GlobalGravityExtra += GlobalVelocity;
				_enemyCollider[0].SetSize(new Vector2(0.5f, 0.2f));
				SetCohere(new Vector3(-90f, 0f, 0f));
			}
			else if (Controller.Collisions.right)
			{
				horizon = true;
				GlobalVelocity = VInt3.signRight * 2000;
				ModelTransform.localPosition = new Vector3(0.25f, 0f, 0f);
				OrangeBattleUtility.GlobalVelocityExtra += GlobalVelocity;
				_enemyCollider[0].SetSize(new Vector2(0.2f, 0.5f));
				SetCohere(new Vector3(0f, 90f, 0f));
			}
			else if (Controller.Collisions.below)
			{
				vertical = true;
				GlobalVelocity = VInt3.signDown * 1000;
				ModelTransform.localPosition = new Vector3(0f, -0.25f, 0f);
				OrangeBattleUtility.GlobalGravityExtra += GlobalVelocity;
				_enemyCollider[0].SetSize(new Vector2(0.5f, 0.2f));
				SetCohere(new Vector3(90f, 0f, 0f));
			}
		}
		if ((int)_parentController.Hp <= 0 && !_isDead)
		{
			Hp = 0;
			Hurt(new HurtPassParam());
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
		IgnoreGravity = true;
		IgnoreGlobalVelocity = true;
		ModelTransform.localEulerAngles = Vector3.zero;
		ModelTransform.localPosition = Vector3.zero;
		_transform.position = new Vector3(_transform.position.x, _transform.position.y, 0f);
		bDeadShock = false;
		if (isActive)
		{
			_isMoving = true;
			_isLive = true;
			_needExp = false;
			_isDead = false;
			vertical = false;
			horizon = false;
			_collideBullet.UpdateBulletData(EnemyWeapons[0].BulletData);
			_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			_collideBullet.Active(targetMask);
			ExplosionBullet.UpdateBulletData(EnemyWeapons[1].BulletData);
			ExplosionBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			DeadCallback = RevertGlobalVelocity;
			DestoryFrame = GameLogicUpdateManager.GameFrame + DestoryTime * 20;
			BulletBody.gameObject.SetActive(true);
		}
		else
		{
			GlobalVelocity = VInt3.zero;
			_collideBullet.BackToPool();
			ExplosionBullet.BackToPool();
		}
		FlashLight.gameObject.SetActive(false);
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
		base.transform.localEulerAngles = new Vector3(0f, 0f, 0f);
		base.transform.position = pos;
	}

	private void SetCohere(Vector3 eulerangles)
	{
		_isMoving = false;
		FlashLight.gameObject.SetActive(true);
		ModelTransform.localEulerAngles = eulerangles;
		_velocity = VInt3.zero;
		PlaySE("BossSE02", "bs019_mandar02");
	}

	public override ObscuredInt Hurt(HurtPassParam tHurtPassParam)
	{
		ObscuredInt obscuredInt = base.Hurt(tHurtPassParam);
		if ((int)obscuredInt <= 0)
		{
			StageFXParam stageFXParam = new StageFXParam();
			stageFXParam.bMute = true;
			MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxstory_explode_000", _transform, Quaternion.identity, new Vector3(0.5f, 0.5f, 0.5f), new object[1] { stageFXParam });
		}
		return obscuredInt;
	}

	public void SetParent(BS049_Controller parent)
	{
		_parentController = parent;
	}

	public void RevertGlobalVelocity()
	{
		if (vertical)
		{
			OrangeBattleUtility.GlobalGravityExtra -= GlobalVelocity;
		}
		if (horizon)
		{
			OrangeBattleUtility.GlobalVelocityExtra -= GlobalVelocity;
		}
		GlobalVelocity = VInt3.zero;
	}

	protected override void UpdateGravity()
	{
	}

	protected override void DeadBehavior(ref HurtPassParam tHurtPassParam)
	{
		PlaySE(ExplodeSE[0], ExplodeSE[1]);
		Explosion();
		if (DeadCallback != null)
		{
			DeadCallback();
		}
		StageObjParam component = GetComponent<StageObjParam>();
		if (component != null && component.nEventID != 0)
		{
			EventManager.StageEventCall stageEventCall = new EventManager.StageEventCall();
			stageEventCall.nID = component.nEventID;
			component.nEventID = 0;
			Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_EVENT_CALL, stageEventCall);
		}
		_isDead = true;
		_isLive = false;
		_needExp = false;
		DestoryFrame = GameLogicUpdateManager.GameFrame + 10;
		_velocity = VInt3.zero;
		_collideBullet.BackToPool();
		ExplosionBullet.Active(targetMask);
		BulletBody.gameObject.SetActive(false);
	}
}
