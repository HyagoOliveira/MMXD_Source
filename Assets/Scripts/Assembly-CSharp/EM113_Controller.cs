using CallbackDefs;
using CodeStage.AntiCheat.ObscuredTypes;
using UnityEngine;

public class EM113_Controller : EnemyControllerBase, IManagedUpdateBehavior
{
	private BS051_Controller _parentController;

	private bool _isDead;

	private int DeadFrame;

	private int DeadTime = 15;

	protected internal bool _isBurning;

	protected internal bool _readyBurn;

	private int BurnTime = 10;

	private float StartBurnTime = 0.5f;

	private int StartBurnFrame;

	[SerializeField]
	private Collider2D BurnCollider;

	private BS051_FirePillar _firePillar;

	[SerializeField]
	private ParticleSystem ModelBody;

	[SerializeField]
	private ParticleSystem BlueFire;

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
		_collideBullet = OrangeBattleUtility.FindChildRecursive(ref target, "CollideBullet", true).gameObject.AddOrGetComponent<CollideBullet>();
		_firePillar = OrangeBattleUtility.FindChildRecursive(ref target, "FireCollide", true).gameObject.AddOrGetComponent<BS051_FirePillar>();
		LoadParts(target);
		base.AimPoint = new Vector3(0f, 0f, 0f);
		OrangeBattleUtility.AddEnemyAutoAimSystem(_transform, out _enemyAutoAimSystem);
		_enemyAutoAimSystem = null;
		_enemyCollider[0].gameObject.layer = ManagedSingleton<OrangeLayerManager>.Instance.PlayerLayer;
		base.AllowAutoAim = false;
	}

	public override void UpdateStatus(int nSet, string smsg, Callback tCB = null)
	{
		bWaitNetStatus = false;
	}

	public override void BackToPool()
	{
		base.BackToPool();
		base.SoundSource.StopAll();
	}

	public override void LogicUpdate()
	{
		if (!Activate)
		{
			return;
		}
		base.LogicUpdate();
		if (CheckHost())
		{
			UploadEnemyStatus(0);
		}
		if ((int)_parentController.Hp <= 0)
		{
			RemoveFromList();
			BackToPool();
		}
		if (_readyBurn && GameLogicUpdateManager.GameFrame > StartBurnFrame)
		{
			_readyBurn = false;
			_isBurning = true;
			_firePillar.Active(targetMask);
		}
		if (GameLogicUpdateManager.GameFrame > DeadFrame)
		{
			if (_isBurning)
			{
				_isBurning = false;
				_firePillar.HitCallback = null;
				_firePillar.BackToPool();
				BlueFire.Stop();
				DeadFrame += 20;
				PlaySE("BossSE03", "bs023_mammo07_stop");
			}
			else
			{
				_isDead = true;
				Hp = 0;
				Hurt(new HurtPassParam());
			}
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
		ModelTransform.localEulerAngles = new Vector3(0f, 0f, 0f);
		_transform.position = new Vector3(_transform.position.x, _transform.position.y, 0f);
		bDeadShock = false;
		if (isActive)
		{
			_isDead = false;
			_collideBullet.UpdateBulletData(EnemyWeapons[0].BulletData);
			_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			_collideBullet.Active(targetMask);
			_firePillar.UpdateBulletData(EnemyWeapons[1].BulletData);
			_firePillar.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			_firePillar.HitCallback = FireOnHit;
			_firePillar.BackToPool();
			ModelBody.Play();
			BlueFire.Stop();
			DeadFrame = GameLogicUpdateManager.GameFrame + DeadTime * 20;
			_isBurning = false;
			_readyBurn = false;
			DeadCallback = RemoveFromList;
			Collider2D[] componentsInChildren = GetComponentsInChildren<Collider2D>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].gameObject.AddOrGetComponent<StageObjParam>().tLinkSOB = this;
			}
		}
		else
		{
			_collideBullet.BackToPool();
			_firePillar.BackToPool();
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
		base.transform.localEulerAngles = new Vector3(0f, 0f, 0f);
		base.transform.position = pos;
	}

	public void SetParent(BS051_Controller parent)
	{
		_parentController = parent;
	}

	public void SetDeadSoon()
	{
		DeadFrame = GameLogicUpdateManager.GameFrame + 1;
	}

	public void RemoveFromList()
	{
		_parentController.RemoveKerosene(_transform);
	}

	protected override void DeadBehavior(ref HurtPassParam tHurtPassParam)
	{
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
		_velocity = VInt3.zero;
		_collideBullet.BackToPool();
		_firePillar.BackToPool();
		BackToPool();
	}

	private void LoadParts(Transform[] childs)
	{
		if (!ModelBody)
		{
			ModelBody = OrangeBattleUtility.FindChildRecursive(ref childs, "bodymodel", true).GetComponent<ParticleSystem>();
		}
		if (!BlueFire)
		{
			BlueFire = OrangeBattleUtility.FindChildRecursive(ref childs, "bluefire", true).GetComponent<ParticleSystem>();
		}
	}

	public void StartBurn()
	{
		ModelBody.Stop();
		BlueFire.Play();
		PlaySE("BossSE03", "bs023_mammo07_lp");
		if (!_readyBurn && !_isBurning)
		{
			_readyBurn = true;
			StartBurnFrame = GameLogicUpdateManager.GameFrame + (int)(StartBurnTime * 20f);
			DeadFrame = GameLogicUpdateManager.GameFrame + BurnTime * 20;
			_collideBullet.BackToPool();
			StageObjParam[] componentsInChildren = GetComponentsInChildren<StageObjParam>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].tLinkSOB = null;
			}
			RemoveFromList();
		}
	}

	private void FireOnHit(object obj)
	{
		if (obj == null)
		{
			return;
		}
		StageObjParam component = (obj as Collider2D).gameObject.GetComponent<StageObjParam>();
		if (!(component == null))
		{
			EM113_Controller eM113_Controller = component.tLinkSOB as EM113_Controller;
			if (!(eM113_Controller == null) && !eM113_Controller._readyBurn && !eM113_Controller._isBurning)
			{
				eM113_Controller.StartBurn();
			}
		}
	}

	public override ObscuredInt Hurt(HurtPassParam tHurtPassParam)
	{
		if (!_isDead)
		{
			return Hp;
		}
		return base.Hurt(tHurtPassParam);
	}
}
