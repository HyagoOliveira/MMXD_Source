#define RELEASE
using System;
using System.Collections;
using System.Collections.Generic;
using CallbackDefs;
using CodeStage.AntiCheat.ObscuredTypes;
using Newtonsoft.Json;
using StageLib;
using UnityEngine;
using enums;

public abstract class BulletBase : PoolBaseObject
{
	public class DmgStack
	{
		[JsonProperty("a")]
		public string sPlayerID;

		[JsonProperty("b")]
		public string sShotPlayerID;

		[JsonProperty("c")]
		public int nSkillID;

		[JsonProperty("d")]
		public ObscuredInt nDmg;

		[JsonProperty("e")]
		public ObscuredInt nRecordID;

		[JsonProperty("f")]
		public ObscuredInt nNetID;

		[JsonProperty("g")]
		public string sOwner;

		[JsonProperty("h")]
		public ObscuredInt nSubPartID;

		[JsonProperty("i")]
		public int nDamageType;

		[JsonProperty("j")]
		public int nWeaponCheck;

		[JsonProperty("k")]
		public int nWeaponType;

		[JsonProperty("l")]
		public ObscuredInt nHP = 0;

		[JsonProperty("m")]
		public ObscuredInt nEndHP = 0;

		[JsonProperty("n")]
		public ObscuredInt nEnergyShield = 0;

		[JsonProperty("o")]
		public ObscuredInt nDmgHP = 0;

		[JsonProperty("p")]
		public ObscuredInt nHealHP = 0;

		[JsonProperty("q")]
		public int nLastHitStatus;

		[JsonProperty("r")]
		public float fCriDmgFactor;

		[JsonProperty("s")]
		public float fLastDmgFactor;

		[JsonProperty("t")]
		public int nLastCriPercent;

		[JsonProperty("u")]
		public float fLastBlockFactor;

		[JsonProperty("v")]
		public int nLastBuffFactor;

		[JsonProperty("w")]
		public Vector3 vHitBack = Vector3.zero;

		[JsonProperty("x")]
		public int nBreakEnergyShieldBuffID;
	}

	public class NetBulletData
	{
		[JsonProperty("a")]
		public string sNetSerialID;

		[JsonProperty("b")]
		public ObscuredInt nHP;

		[JsonProperty("c")]
		public ObscuredInt nATK;

		[JsonProperty("d")]
		public ObscuredInt nCRI;

		[JsonProperty("e")]
		public ObscuredInt nHIT;

		[JsonProperty("f")]
		public ObscuredInt nWeaponCheck;

		[JsonProperty("g")]
		public ObscuredInt nWeaponType;

		[JsonProperty("h")]
		public ObscuredInt nCriDmgPercent;

		[JsonProperty("i")]
		public ObscuredInt nReduceBlockPercent;

		[JsonProperty("j")]
		public ObscuredFloat fAtkDmgPercent;

		[JsonProperty("k")]
		public ObscuredFloat fCriPercent;

		[JsonProperty("l")]
		public ObscuredFloat fCriDmgPercent;

		[JsonProperty("m")]
		public Vector3 vPos;

		[JsonProperty("n")]
		public Vector3 vShotDir;

		[JsonProperty("o")]
		public int nTargetMask;

		[JsonProperty("p")]
		public int nRecordNO;

		[JsonProperty("q")]
		public int nBulletID;

		[JsonProperty("r")]
		public int nSkillID;

		[JsonProperty("s")]
		public string tNetSkillTable;

		[JsonProperty("t")]
		public int nDirect = 1;

		[JsonProperty("u")]
		public string sTargerNetID = "";

		[JsonProperty("v")]
		public string sShotTransPath = "";

		[JsonProperty("w")]
		public ObscuredFloat fMissPercent;

		[JsonProperty("x")]
		public Vector3 vTargetAimPos;
	}

	[Flags]
	public enum BulletFlag
	{
		None = 0,
		ThroughWall = 1,
		CollideBullet = 2,
		Through = 4,
		Splash = 8,
		HitShield = 0x10,
		Break = 0x20,
		IsPlayer = 0x40,
		CollideBulletExit = 0x80,
		Ignore_StageHurtObj = 0x1000
	}

	public enum HIT_TYPE
	{
		UNBREAK = 0,
		MISS = 1,
		NORMAL = 2,
		HEAL = 3,
		TRIGGERONLY = 4,
		UNBREAKX = 5
	}

	public enum ColliderType
	{
		Box = 0,
		Circle = 1,
		Undefined = 2
	}

	private enum CaluType
	{
		None = 0,
		Character = 1,
		Pet = 2
	}

	public class ShotBullerParam
	{
		public SKILL_TABLE tSkillTable;

		public Transform pTransform;

		public Vector3 vShotPos;

		public Vector3 pDirection;

		public WeaponStatus weaponStatus;

		public PerBuffManager.BuffStatus tBuffStatus;

		public MOB_TABLE refMOB_TABLE;

		public LayerMask pTargetMask;

		public bool forcePlaySE;

		public int nRecordID;

		public int nNetID;

		public int nBulletLV = 1;

		public int nDirection = 1;

		public string owner;

		public void ZeroParam()
		{
			tSkillTable = null;
			pTransform = null;
			vShotPos = Vector3.zero;
			pDirection = Vector3.zero;
			weaponStatus = null;
			tBuffStatus = null;
			refMOB_TABLE = null;
			pTargetMask = 0;
			forcePlaySE = false;
			nRecordID = 0;
			nNetID = 0;
			nBulletLV = 1;
			nDirection = 1;
			owner = "";
		}
	}

    [Obsolete]
    public CallbackObj HitCallback;
    [Obsolete]
    public CallbackObj BackCallback;
    [Obsolete]
    public CallbackObj HitBlockCallback;

	public readonly Quaternion BulletQuaternion = Quaternion.Euler(0f, 0f, 0f);

	public bool isForceSE;

	public bool isMuteSE;

	[HideInInspector]
	public bool needPlayEndSE;

	[HideInInspector]
	public bool needWeaponImpactSE = true;

	[HideInInspector]
	public bool isHitBlock;

	private bool hitShield;

	[HideInInspector]
	public bool isBossBullet;

	[HideInInspector]
	public bool isPetBullet;

	[HideInInspector]
	public bool bIsEnd = true;

	[HideInInspector]
	protected int BulletID;

	[HideInInspector]
	public int Hp;

	[HideInInspector]
	public int MaxHp = 1;

	[HideInInspector]
	public int BulletLevel = 1;

	protected Vector3 Direction = Vector3.right;

	[SerializeField]
	protected bool isMirror;

	protected Vector3 Velocity = Vector3.zero;

	private int tempHp;

	private int currentHp;

	private VisualDamage.DamageType damageType;

	private int nLastHitStatus;

	private float fLastDmgFactor;

	private int nLastCriPercent;

	private float fLastBlockFactor;

	private int nLastBuffFactor;

	private int nBaseAtk;

	private int nBaseDmg;

	private int nDmg;

	protected BulletScriptableObject BulletScriptableObjectInstance;

	protected SKILL_TABLE BulletData;

	protected TRACKING_TABLE TrackingData;

	protected bool activeTracking;

	protected IAimTarget Target;

	protected bool ForceAIS;

	protected NeutralAutoAimSystem NeutralAIS;

	protected OrangeTimer ActivateTimer;

	public bool isSubBullet;

	protected string Owner = "";

	protected Coroutine CoroutineMove;

	[HideInInspector]
	public LayerMask BlockMask;

	[HideInInspector]
	public LayerMask BulletMask;

	[HideInInspector]
	public LayerMask TargetMask;

	public LayerMask UseMask;

	[HideInInspector]
	public Transform _transform;

	protected Transform MasterTransform;

	protected Vector3 MasterPosition;

	protected string FxMuzzleFlare;

	protected string FxImpact;

	protected string FxEnd;

	protected bool visible;

	private bool preVisible;

	protected bool checkLoopSE;

	private Renderer meshRenderer;

	protected bool isBuffTrigger;

	private OrangeCriSource _ss;

	protected OrangeCharacter MainOC;

	protected string[] _UseSE;

	protected string[] _HitSE;

	[HideInInspector]
	public string[] _HitGuardSE;

	[HideInInspector]
	public string[] _HitReflectSE;

	public bool UseExtraCollider;

	[HideInInspector]
	public int nHp;

	[HideInInspector]
	public int nAtk;

	[HideInInspector]
	public int nCri;

	[HideInInspector]
	public int nHit;

	[HideInInspector]
	public int nCriDmgPercent;

	[HideInInspector]
	public int nReduceBlockPercent;

	[HideInInspector]
	public float fDmgFactor = 100f;

	[HideInInspector]
	public float fCriFactor = 100f;

	[HideInInspector]
	public float fCriDmgFactor = 100f;

	[HideInInspector]
	public float fMissFactor;

	[HideInInspector]
	public PerBuffManager refPBMShoter;

	[HideInInspector]
	public RefPassiveskill refPSShoter;

	[HideInInspector]
	public int nWeaponCheck;

	[HideInInspector]
	public int nWeaponType;

	[HideInInspector]
	public int nThrough;

	[HideInInspector]
	public int nRecordID;

	[HideInInspector]
	public int nNetID;

	[HideInInspector]
	protected HurtPassParam tHurtPassParam = new HurtPassParam();

	[HideInInspector]
	private string sShotPlayerID = "";

	[HideInInspector]
	private OrangeCharacter tShotOC;

	[HideInInspector]
	private Vector2 xy;

	[HideInInspector]
	protected StageObjParam tStageObjParam;

	[HideInInspector]
	public static DmgStack tNetDmgStack = new DmgStack();

	[SerializeField]
	protected ParticleSystem[] bulletFxArray = new ParticleSystem[0];

	protected int nOriginalATK;

	protected int nOriginalCRI;

	protected int reflectCount;

	private Callback PlayUSECB;

	protected Transform hitTarget;

	protected bool bCanUseInEventBullet = true;

	protected Vector3 targetPos = new Vector3(10000f, 10000f, 0f);

	private const string PREFIX_FT = "BB#";

	private const string PREFIX_FB = "FB#";

	public Vector3 GetDirection
	{
		get
		{
			return Direction;
		}
	}

	public SKILL_TABLE GetBulletData
	{
		get
		{
			return BulletData;
		}
	}

	public TRACKING_TABLE GetTrackingData
	{
		get
		{
			return TrackingData;
		}
	}

	[SerializeField]
	public OrangeCriSource SoundSource
	{
		get
		{
			if (_ss == null)
			{
				_ss = base.gameObject.GetComponent<OrangeCriSource>();
				if (_ss == null)
				{
					_ss = base.gameObject.AddComponent<OrangeCriSource>();
					if (base.gameObject.name.Contains("enemy_bs"))
					{
						_ss.Initial(OrangeSSType.BOSS);
					}
					else if (base.gameObject.name.Contains("enemy_em"))
					{
						_ss.Initial(OrangeSSType.ENEMY);
					}
					else
					{
						_ss.Initial(OrangeSSType.HIT);
					}
				}
			}
			return _ss;
		}
		set
		{
			_ss = value;
		}
	}

	public bool isVisiable
	{
		get
		{
			return visible;
		}
	}

	public virtual Vector3 GetCreateBulletPosition
	{
		get
		{
			return _transform.position;
		}
	}

	public virtual Vector3 GetCreateBulletShotDir
	{
		get
		{
			return Direction;
		}
	}

	protected abstract IEnumerator OnStartMove();

	public abstract void Hit(Collider2D col);

	protected virtual void Awake()
	{
		ActivateTimer = OrangeTimerManager.GetTimer();
		Hp = MaxHp;
		_transform = base.transform;
		BulletScriptableObjectInstance = BulletScriptableObject.Instance;
		if (BulletID != 0)
		{
			UpdateBulletData(ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT[BulletID]);
		}
		MonoBehaviourSingleton<FxManager>.Instance.UpdateFx(bulletFxArray, false);
		meshRenderer = OrangeGameUtility.AddOrGetRenderer<Renderer>(base.gameObject);
		FXSoundPlayer[] componentsInChildren = base.gameObject.GetComponentsInChildren<FXSoundPlayer>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].SoundSource = SoundSource;
		}
	}

	public virtual void OnStart()
	{
		bIsEnd = false;
		tHurtPassParam.BulletFlg = BulletFlag.None;
	}

	public virtual void Active(Vector3 pPos, Vector3 pDirection, LayerMask pTargetMask, IAimTarget pTarget = null)
	{
		if (meshRenderer == null)
		{
			meshRenderer = OrangeGameUtility.AddOrGetRenderer<Renderer>(base.gameObject);
		}
		Hp = MaxHp;
		nOriginalATK = nAtk;
		nOriginalCRI = nCri;
		MasterTransform = null;
		MasterPosition = pPos;
		if (!isMirror)
		{
			_transform.eulerAngles = new Vector3(0f, 0f, Vector2.SignedAngle(Vector2.right, pDirection));
		}
		else
		{
			float num = Vector2.SignedAngle(Vector2.right, pDirection);
			_transform.eulerAngles = new Vector3(0f, 0f, Vector2.SignedAngle(Vector2.right, pDirection));
			if (num < 0f)
			{
				num += 360f;
			}
			if (num > 90f && num < 270f)
			{
				_transform.localScale = new Vector3(1f, -1f, 1f);
			}
			else
			{
				_transform.localScale = Vector3.one;
			}
		}
		_transform.position = pPos;
		Direction = pDirection;
		Velocity = new Vector3(BulletData.n_SPEED, 0f, 0f);
		if ((pTargetMask.value & BulletScriptableObjectInstance.BulletLayerMaskPlayer.value) != 0)
		{
			UseExtraCollider = false;
		}
		TargetMask = pTargetMask;
		UseMask = (int)BlockMask | (int)pTargetMask;
		if (CoroutineMove != null)
		{
			StopCoroutine(CoroutineMove);
		}
		CoroutineMove = StartCoroutine(OnStartMove());
		OnStart();
		MonoBehaviourSingleton<FxManager>.Instance.UpdateFx(bulletFxArray, true);
		isHitBlock = (needWeaponImpactSE = false);
		SoundSource.UpdateDistanceCall();
		if (isForceSE)
		{
			ForcePlayUSE();
		}
		else
		{
			PlayUseSE();
		}
	}

	public virtual void Active(Transform pTransform, Vector3 pDirection, LayerMask pTargetMask, IAimTarget pTarget = null)
	{
		if (meshRenderer == null)
		{
			meshRenderer = OrangeGameUtility.AddOrGetRenderer<Renderer>(pTransform.gameObject);
		}
		pDirection = UpdateDirectionByShotline(pDirection);
		Hp = MaxHp;
		nOriginalATK = nAtk;
		nOriginalCRI = nCri;
		MasterTransform = pTransform;
		MasterPosition = pTransform.position;
		if (!isMirror)
		{
			_transform.eulerAngles = new Vector3(0f, 0f, Vector2.SignedAngle(Vector2.right, pDirection));
		}
		else
		{
			float num = Vector2.SignedAngle(Vector2.right, pDirection);
			_transform.eulerAngles = new Vector3(0f, 0f, Vector2.SignedAngle(Vector2.right, pDirection));
			if (num < 0f)
			{
				num += 360f;
			}
			if (num > 90f && num < 270f)
			{
				_transform.localScale = new Vector3(1f, -1f, 1f);
			}
			else
			{
				_transform.localScale = Vector3.one;
			}
		}
		_transform.position = pTransform.position;
		Direction = pDirection;
		Velocity = new Vector3(BulletData.n_SPEED, 0f, 0f);
		if ((pTargetMask.value & BulletScriptableObjectInstance.BulletLayerMaskPlayer.value) != 0)
		{
			UseExtraCollider = false;
		}
		TargetMask = pTargetMask;
		UseMask = (int)BlockMask | (int)pTargetMask;
		OnStart();
		SoundSource.UpdateDistanceCall();
		CoroutineMove = StartCoroutine(OnStartMove());
		MonoBehaviourSingleton<FxManager>.Instance.UpdateFx(bulletFxArray, true);
		isHitBlock = (needWeaponImpactSE = false);
		PlayUseSE();
	}

	public virtual void TargetAimPositionOnActive(Vector3 targetAimPosition)
	{
	}

	protected Vector3 UpdateDirectionByShotline(Vector3 pDirection)
	{
		if (BulletData == null)
		{
			return pDirection;
		}
		switch (BulletData.n_SHOTLINE)
		{
		case 6:
		case 8:
			if (!isSubBullet)
			{
				pDirection = ((pDirection.x >= 0f) ? Vector3.right : Vector3.left);
			}
			break;
		case 12:
			if (!isSubBullet)
			{
				pDirection = Vector3.down;
			}
			break;
		}
		return pDirection;
	}

	public virtual void PlayUseSE(bool force = false)
	{
		if (!isMuteSE)
		{
			SoundSource.PlaySE(_UseSE[0], _UseSE[1]);
		}
	}

	public void PauseUseSE(bool bPause)
	{
		if (!isMuteSE)
		{
			SoundSource.Pause(bPause);
		}
	}

	public void UpdateFx()
	{
		MonoBehaviourSingleton<FxManager>.Instance.UpdateFx(bulletFxArray, true);
	}

	protected virtual void Stop()
	{
	}

	public void SetTartget(IAimTarget target)
	{
		Target = target;
	}

	public virtual void UpdateBulletData(SKILL_TABLE pData, string owner = "", int nInRecordID = 0, int nInNetID = 0, int nDirection = 1)
	{
		BulletID = pData.n_ID;
		nNetID = nInNetID;
		nRecordID = nInRecordID;
		Owner = owner;
		BulletData = pData;
		if (BulletData.n_TRACKING > 0 || ForceAIS)
		{
			if (BulletData.n_TRACKING > 0)
			{
				TrackingData = ManagedSingleton<OrangeDataManager>.Instance.TRACKING_TABLE_DICT[BulletData.n_TRACKING];
			}
			OrangeBattleUtility.AddNeutralAutoAimSystem(base.transform, out NeutralAIS);
			activeTracking = true;
		}
		else
		{
			activeTracking = false;
			if ((bool)NeutralAIS)
			{
				UnityEngine.Object.Destroy(NeutralAIS);
				NeutralAIS = null;
			}
		}
		string s_REBOUND = BulletData.s_REBOUND;
		if (ManagedSingleton<OrangeTableHelper>.Instance.IsNullOrEmpty(s_REBOUND))
		{
			reflectCount = 0;
		}
		else
		{
			string[] array = BulletData.s_REBOUND.Split(',');
			if (array.Length >= 1)
			{
				int.TryParse(array[0], out reflectCount);
			}
		}
		FxMuzzleFlare = BulletData.s_USE_FX;
		FxImpact = BulletData.s_HIT_FX;
		FxEnd = BulletData.s_VANISH_FX;
		if (FxMuzzleFlare == "null")
		{
			FxMuzzleFlare = string.Empty;
		}
		else
		{
			MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(FxMuzzleFlare, 5);
		}
		if (FxImpact == "null")
		{
			FxImpact = string.Empty;
		}
		else
		{
			MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(FxImpact, 5);
		}
		if (FxEnd == "null")
		{
			FxEnd = string.Empty;
		}
		else
		{
			MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(FxEnd, 5);
		}
		BlockMask = BulletScriptableObjectInstance.BulletLayerMaskObstacle;
		if (((uint)BulletData.n_FLAG & 2u) != 0)
		{
			BulletMask = BulletScriptableObjectInstance.BulletLayerMaskBullet;
		}
		_UseSE = ManagedSingleton<OrangeTableHelper>.Instance.ParseSE(BulletData.s_USE_SE);
		_HitSE = ManagedSingleton<OrangeTableHelper>.Instance.ParseSE(BulletData.s_HIT_SE);
		if (_UseSE[0] != "" && (_UseSE[1].EndsWith("_lp") || _UseSE[1].EndsWith("_lg")))
		{
			checkLoopSE = true;
		}
		GetHistGuardSE();
	}

	public void SetOwnerName(string name)
	{
		Owner = name;
	}

	public void ForcePlayUSE()
	{
		if (!isMuteSE && _UseSE != null && !(_UseSE[0] == ""))
		{
			SoundSource.ForcePlaySE(_UseSE[0], _UseSE[1]);
		}
	}

	protected virtual void BeatenTarget(Transform bullet, Transform target)
	{
	}

	public virtual int Hurt(int dmg, WeaponType wpnType = WeaponType.Buster)
	{
		Hp -= dmg;
		if (Hp > 0)
		{
			switch (wpnType)
			{
			}
		}
		else
		{
			if (wpnType != WeaponType.Buster && wpnType != WeaponType.Melee)
			{
				int num = 32;
			}
			BackToPool();
		}
		return Hp;
	}

	public virtual void BackToPoolByDisconnet()
	{
		BackToPool();
	}

	public override void BackToPool()
	{
		if (BackCallback != null)
		{
			BackCallback(this);
			BackCallback = null;
		}
		HitCallback = null;
		if (_UseSE != null && _UseSE[0] != "" && _UseSE[2] != "")
		{
			SoundSource.PlaySE(_UseSE[0], _UseSE[2]);
		}
		targetPos = new Vector3(10000f, 10000f, 0f);
		StopFx();
		isPetBullet = (isBossBullet = (isBuffTrigger = false));
		Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_BULLET_UNREGISTER, this);
		bIsEnd = true;
		hitShield = false;
		if (StageUpdate.gbStageReady)
		{
			base.BackToPool();
		}
		else
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	protected virtual void StopFx()
	{
		MonoBehaviourSingleton<FxManager>.Instance.UpdateFx(bulletFxArray, false);
	}

	public virtual void SyncBulletStatus(int nSet, string smsg)
	{
	}

	public void SetBulletAtk(WeaponStatus weaponStatus, PerBuffManager.BuffStatus tBuffStatus, MOB_TABLE refMOB_TABLE = null)
	{
		if (weaponStatus != null && refMOB_TABLE == null)
		{
			nHp = weaponStatus.nHP;
			nAtk = weaponStatus.nATK;
			nCri = weaponStatus.nCRI;
			nHit = weaponStatus.nHIT;
			nCriDmgPercent = weaponStatus.nCriDmgPercent;
			nReduceBlockPercent = weaponStatus.nReduceBlockPercent;
			nWeaponCheck = weaponStatus.nWeaponCheck;
			nWeaponType = weaponStatus.nWeaponType;
		}
		else if (refMOB_TABLE != null)
		{
			nHp = refMOB_TABLE.n_HP;
			nAtk = refMOB_TABLE.n_ATK;
			nCri = refMOB_TABLE.n_CRI;
			nHit = refMOB_TABLE.n_HIT;
			nCriDmgPercent = refMOB_TABLE.n_CRIDMG;
			nReduceBlockPercent = refMOB_TABLE.n_PARRY_RESIST;
			int[] array = new int[12]
			{
				refMOB_TABLE.n_SKILL_1, refMOB_TABLE.n_SKILL_2, refMOB_TABLE.n_SKILL_3, refMOB_TABLE.n_SKILL_4, refMOB_TABLE.n_SKILL_5, refMOB_TABLE.n_SKILL_6, refMOB_TABLE.n_SKILL_7, refMOB_TABLE.n_SKILL_8, refMOB_TABLE.n_SKILL_9, refMOB_TABLE.n_SKILL_10,
				refMOB_TABLE.n_SKILL_11, refMOB_TABLE.n_SKILL_12
			};
			nWeaponCheck = 0;
			for (int i = 0; i < array.Length; i++)
			{
				if (BulletData.n_ID == array[i])
				{
					nWeaponCheck = 1 << i;
				}
			}
			nWeaponType = 0;
			BulletLevel = 0;
			Owner = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_MOB_NAME");
		}
		fDmgFactor = 100f + tBuffStatus.fAtkDmgPercent;
		fCriFactor = 100f + tBuffStatus.fCriPercent;
		fCriDmgFactor = 100f + tBuffStatus.fCriDmgPercent;
		fMissFactor = tBuffStatus.fMissPercent;
		refPBMShoter = tBuffStatus.refPBM;
		if (refPBMShoter == null)
		{
			refPBMShoter = new PerBuffManager();
		}
		refPSShoter = tBuffStatus.refPS;
		if (refPSShoter == null)
		{
			refPSShoter = new RefPassiveskill();
		}
		Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_BULLET_REGISTER, this);
		nOriginalATK = nAtk;
		nAtk = GetFinalATK(nAtk, ref refPSShoter, nWeaponCheck);
		nOriginalCRI = nCri;
		bool isPlayer = ((refPBMShoter.SOB as OrangeCharacter != null) ? true : false);
		nCri = GetFinalCRI(nCri, ref refPSShoter, nWeaponCheck, isPlayer);
		nHit += refPSShoter.GetAddStatus(8, nWeaponCheck);
	}

	public void TriggerBuff(SKILL_TABLE pData, PerBuffManager targetBuffManager, int nAtkParam, bool playSE = false)
	{
		string sAdderID = "";
		if (refPBMShoter.SOB != null)
		{
			sAdderID = refPBMShoter.SOB.sNetSerialID;
		}
		if (targetBuffManager != null)
		{
			if (pData.n_CONDITION_ID != 0)
			{
				if (pData.n_CONDITION_TARGET != 1)
				{
					if (pData.n_CONDITION_RATE >= OrangeBattleUtility.Random(0, 10000))
					{
						targetBuffManager.AddBuff(pData.n_CONDITION_ID, nAtkParam, 0, pData.n_ID, false, sAdderID, 4);
					}
				}
				else if (pData.n_CONDITION_TARGET == 1 && pData.n_CONDITION_RATE >= OrangeBattleUtility.Random(0, 10000))
				{
					refPBMShoter.AddBuff(pData.n_CONDITION_ID, nAtkParam, 0, pData.n_ID, false, sAdderID, 4);
				}
			}
			targetPos = targetBuffManager.SOB.AimPosition;
		}
		if (nWeaponCheck != 0)
		{
			refPSShoter.HitSkillTrigger(pData, nWeaponCheck, refPBMShoter, targetBuffManager, nAtkParam, nAtk, isPetBullet, CreateBulletDetail);
		}
	}

	public void CreateBulletDetail(string bulletData)
	{
		SKILL_TABLE sKILL_TABLE = new SKILL_TABLE();
		sKILL_TABLE.ConvertFromString(bulletData);
		CreateBulletDetail(sKILL_TABLE);
	}

	public void CreateBulletDetail(SKILL_TABLE bulletData)
	{
		BulletBase bulletBase = null;
		PoolManager instance = MonoBehaviourSingleton<PoolManager>.Instance;
		switch ((BulletType)(short)bulletData.n_TYPE)
		{
		case BulletType.Continuous:
			bulletBase = instance.GetPoolObj<ContinuousBullet>(bulletData.s_MODEL);
			break;
		case BulletType.Spray:
			bulletBase = instance.GetPoolObj<SprayBullet>(bulletData.s_MODEL);
			break;
		case BulletType.Collide:
			bulletBase = instance.GetPoolObj<CollideBullet>(bulletData.s_MODEL);
			((CollideBullet)bulletBase).bNeedBackPoolModelName = true;
			break;
		case BulletType.LrColliderBulle:
			bulletBase = instance.GetPoolObj<LrColliderBullet>(bulletData.s_MODEL);
			break;
		case BulletType.GroundMissile:
			bulletBase = instance.GetPoolObj<GroundMissileBullet>(bulletData.s_MODEL);
			break;
		default:
			bulletBase = instance.GetPoolObj<BulletBase>(bulletData.s_MODEL);
			if (bulletBase.GetType() == typeof(SlicingBullet))
			{
				bulletBase.tStageObjParam = tStageObjParam;
			}
			break;
		}
		if (!bulletBase)
		{
			return;
		}
		if (tShotOC != null)
		{
			bulletBase.UpdateBulletData(bulletData, Owner, 0, 0, tShotOC.direction);
		}
		else
		{
			bulletBase.UpdateBulletData(bulletData, Owner);
		}
		WeaponStatus objFromPool = StageResManager.GetObjFromPool<WeaponStatus>();
		objFromPool.nHP = nHp;
		objFromPool.nATK = nOriginalATK;
		objFromPool.nCRI = nOriginalCRI;
		objFromPool.nHIT = nHit - refPSShoter.GetAddStatus(8, nWeaponCheck);
		objFromPool.nCriDmgPercent = nCriDmgPercent;
		objFromPool.nReduceBlockPercent = nReduceBlockPercent;
		objFromPool.nWeaponCheck = nWeaponCheck;
		objFromPool.nWeaponType = nWeaponType;
		PerBuffManager.BuffStatus objFromPool2 = StageResManager.GetObjFromPool<PerBuffManager.BuffStatus>();
		objFromPool2.fAtkDmgPercent = fDmgFactor - 100f;
		objFromPool2.fCriPercent = fCriFactor - 100f;
		objFromPool2.fCriDmgPercent = fCriDmgFactor - 100f;
		objFromPool2.fMissPercent = fMissFactor;
		objFromPool2.refPBM = refPBMShoter;
		objFromPool2.refPS = refPSShoter;
		bulletBase.SetBulletAtk(objFromPool, objFromPool2);
		bulletBase.BulletLevel = BulletLevel;
		int n_TRIGGER = bulletData.n_TRIGGER;
		if (n_TRIGGER != 32)
		{
			bulletBase.transform.position = GetCreateBulletPosition;
		}
		else
		{
			bulletBase.transform.position = targetPos;
		}
		bulletBase.nRecordID = nRecordID;
		bulletBase.nNetID = (nNetID + 1) * 1000;
		bulletBase.BeatenTarget(_transform, hitTarget);
		bulletBase.isPetBullet = isPetBullet;
		IAimTarget pTarget = null;
		if (bulletData.n_TRACKING > 0 && tStageObjParam != null && tStageObjParam.tLinkSOB != null)
		{
			pTarget = tStageObjParam.tLinkSOB as IAimTarget;
		}
		bulletBase.Active(bulletBase.transform, GetCreateBulletShotDir, TargetMask, pTarget);
		NetBulletData objFromPool3 = StageResManager.GetObjFromPool<NetBulletData>();
		objFromPool3.sNetSerialID = refPBMShoter.SOB.sNetSerialID;
		objFromPool3.nHP = nHp;
		objFromPool3.nATK = nAtk;
		objFromPool3.nCRI = nCri;
		objFromPool3.nHIT = nHit;
		objFromPool3.nWeaponCheck = nWeaponCheck;
		objFromPool3.nWeaponType = nWeaponType;
		objFromPool3.nCriDmgPercent = nCriDmgPercent;
		objFromPool3.nReduceBlockPercent = nReduceBlockPercent;
		objFromPool3.fAtkDmgPercent = objFromPool2.fAtkDmgPercent;
		objFromPool3.fCriPercent = objFromPool2.fCriPercent;
		objFromPool3.fCriDmgPercent = objFromPool2.fCriDmgPercent;
		objFromPool3.fMissPercent = objFromPool2.fMissPercent;
		objFromPool3.vPos = bulletBase.transform.position;
		objFromPool3.vShotDir = GetCreateBulletShotDir;
		if (tShotOC != null)
		{
			objFromPool3.nDirect = tShotOC.direction;
		}
		objFromPool3.nTargetMask = TargetMask;
		objFromPool3.nRecordNO = nRecordID;
		objFromPool3.nBulletID = bulletBase.nNetID;
		objFromPool3.nSkillID = 0;
		objFromPool3.tNetSkillTable = "";
		if (ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT.ContainsKey(bulletData.n_ID))
		{
			objFromPool3.nSkillID = bulletData.n_ID;
			SKILL_TABLE sKILL_TABLE = ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT[bulletData.n_ID];
			if (!sKILL_TABLE.EqualValue(bulletData))
			{
				Dictionary<int, object> dictionary = bulletData.MakeDiffDictionary(sKILL_TABLE);
				NetDiffDataPacker netDiffDataPacker = new NetDiffDataPacker();
				foreach (KeyValuePair<int, object> item in dictionary)
				{
					netDiffDataPacker.Add(new NetDiffData
					{
						key = item.Key,
						value = item.Value
					});
				}
				objFromPool3.tNetSkillTable = JsonHelper.Serialize(netDiffDataPacker);
			}
		}
		objFromPool3.sShotTransPath = "";
		if (bulletData.n_TRACKING > 0 && tStageObjParam != null && tStageObjParam.tLinkSOB != null)
		{
			objFromPool3.sTargerNetID = tStageObjParam.tLinkSOB.sNetSerialID;
			objFromPool3.vTargetAimPos = tStageObjParam.tLinkSOB.AimPosition;
		}
		else
		{
			objFromPool3.sTargerNetID = "";
			objFromPool3.vTargetAimPos = Vector3.zero;
		}
		StageUpdate.SyncStageObj(4, 2, JsonConvert.SerializeObject(objFromPool3, Formatting.None, new ObscuredValueConverter()), true);
		StageResManager.BackObjToPool(objFromPool);
		StageResManager.BackObjToPool(objFromPool2);
		StageResManager.BackObjToPool(objFromPool3);
	}

	private float GetSkillDmgBuff(SKILL_TABLE pData, PerBuffManager targetBuffManager)
	{
		float num = 0f;
		num += (float)refPSShoter.GetSkillDmgBuff(pData, nWeaponCheck, nWeaponType, refPBMShoter, targetBuffManager, isPetBullet);
		if (targetBuffManager != null)
		{
			num += targetBuffManager.GetEffectXByIDParam(109, pData.n_ID, refPBMShoter);
		}
		return num;
	}

	private int GetSkillResistanceBuff(SKILL_TABLE pData, StageObjBase tSOB, int nDmg)
	{
		int num = 0;
		if (tSOB.tRefPassiveskill != null)
		{
			num += tSOB.tRefPassiveskill.GetSkillResistanceSPBuff(pData, nWeaponCheck, tSOB.Hp, tSOB.MaxHp, refPBMShoter, tSOB.selfBuffManager, isPetBullet);
		}
		return num;
	}

	private static float GetSkillDmgBuff(StageObjBase tSOB, DmgStack tDmgStack, SKILL_TABLE pData, PerBuffManager targetBuffManager, bool isPetBullet)
	{
		float num = 0f;
		num += (float)tSOB.tRefPassiveskill.GetSkillDmgBuff(pData, tDmgStack.nWeaponCheck, tDmgStack.nWeaponType, tSOB.selfBuffManager, targetBuffManager, isPetBullet);
		if (targetBuffManager != null)
		{
			num += targetBuffManager.GetEffectXByIDParam(109, pData.n_ID, tSOB.selfBuffManager);
		}
		return num;
	}

	private static int GetSkillResistanceBuff(SKILL_TABLE pData, StageObjBase tSOBShooter, StageObjBase tSOB, DmgStack tDmgStack, bool isPetBullet)
	{
		int num = 0;
		if (tSOB.tRefPassiveskill != null)
		{
			num += tSOB.tRefPassiveskill.GetSkillResistanceSPBuff(pData, tDmgStack.nWeaponCheck, tSOB.Hp, tSOB.MaxHp, tSOBShooter.selfBuffManager, tSOB.selfBuffManager, isPetBullet);
		}
		return num;
	}

	public static int CalclDmgOnlyByData(StageObjBase tShotSOB, WeaponStatus tWS, int BulletLevel, SKILL_TABLE pData, StageObjBase tSOB, DmgStack tDmgStack, bool bShowLog = false)
	{
		int num = 0;
		int nCurrentWeapon = 0;
		if (((uint)tDmgStack.nLastHitStatus & 0x40u) != 0)
		{
			nCurrentWeapon = 1;
		}
		if (tDmgStack.nLastHitStatus == 0)
		{
			return 0;
		}
		if (tDmgStack.nLastHitStatus == 4)
		{
			return Mathf.RoundToInt(((float)GetFinalATK(tWS.nATK, ref tShotSOB.tRefPassiveskill, tDmgStack.nWeaponCheck) * pData.f_EFFECT_X + pData.f_EFFECT_Y + (float)(int)tSOB.MaxHp * pData.f_EFFECT_Z) * 0.01f);
		}
		if (((uint)tDmgStack.nLastHitStatus & (true ? 1u : 0u)) != 0)
		{
			if (((uint)tDmgStack.nLastHitStatus & 0x20u) != 0)
			{
				return 0;
			}
			switch (pData.n_EFFECT)
			{
			case 1:
				if (bShowLog)
				{
					Debug.LogError("Skill " + pData.n_ID);
					Debug.LogError("tSOB.GetDEF() " + tSOB.GetDEF(nCurrentWeapon));
					Debug.LogError("AddAtkStatus " + tShotSOB.tRefPassiveskill.GetAddStatus(2, tDmgStack.nWeaponCheck));
					Debug.LogError("RatioAtkStatus " + tShotSOB.tRefPassiveskill.GetRatioStatus(2, tDmgStack.nWeaponCheck));
					Debug.LogError("tWS.nATK " + tWS.nATK);
					Debug.LogError("tDmgStack.fLastDmgFactor " + tDmgStack.fLastDmgFactor);
				}
				num = Mathf.RoundToInt((float)(GetFinalATK(tWS.nATK, ref tShotSOB.tRefPassiveskill, tDmgStack.nWeaponCheck) - (int)tSOB.GetDEF(nCurrentWeapon)) * tDmgStack.fLastDmgFactor * 0.01f);
				num = Mathf.RoundToInt((float)num * (pData.f_EFFECT_X + (float)BulletLevel * pData.f_EFFECT_Y) * 0.01f);
				break;
			case 12:
			case 13:
				num = ((pData.n_EFFECT != 12) ? Mathf.RoundToInt(((float)(int)tSOB.MaxHp + tDmgStack.fLastDmgFactor) * pData.f_EFFECT_X * 0.01f) : Mathf.RoundToInt((float)(int)tSOB.MaxHp * pData.f_EFFECT_X * 0.01f));
				break;
			case 30:
				num = Mathf.RoundToInt((float)(int)tSOB.MaxHp * pData.f_EFFECT_X * 0.01f) + Mathf.RoundToInt((float)(int)tSOB.Hp * pData.f_EFFECT_Y * 0.01f);
				if ((float)num >= (float)(int)tSOB.Hp - pData.f_EFFECT_Z)
				{
					num = Mathf.RoundToInt((float)(int)tSOB.Hp - pData.f_EFFECT_Z);
				}
				break;
			default:
				if (bShowLog)
				{
					Debug.LogError("Skill " + pData.n_ID);
					Debug.LogError("tSOB.GetDEF() " + tSOB.GetDEF(nCurrentWeapon));
					Debug.LogError("AddAtkStatus " + tShotSOB.tRefPassiveskill.GetAddStatus(2, tDmgStack.nWeaponCheck));
					Debug.LogError("RatioAtkStatus " + tShotSOB.tRefPassiveskill.GetRatioStatus(2, tDmgStack.nWeaponCheck));
					Debug.LogError("tWS.nATK " + tWS.nATK);
					Debug.LogError("tDmgStack.fLastDmgFactor " + tDmgStack.fLastDmgFactor);
				}
				num = Mathf.RoundToInt((float)(GetFinalATK(tWS.nATK, ref tShotSOB.tRefPassiveskill, tDmgStack.nWeaponCheck) - (int)tSOB.GetDEF(nCurrentWeapon)) * tDmgStack.fLastDmgFactor * 0.01f);
				num = Mathf.RoundToInt((float)num * (pData.f_EFFECT_X + (float)BulletLevel * pData.f_EFFECT_Y) * 0.01f);
				break;
			}
			if (pData.n_EFFECT != 12 && pData.n_EFFECT != 13)
			{
				if (((uint)tDmgStack.nLastHitStatus & 2u) != 0)
				{
					num = Mathf.RoundToInt((float)num * ((10000f + (float)tDmgStack.nLastCriPercent) / 10000f));
					num = Mathf.RoundToInt((float)num * tDmgStack.fCriDmgFactor * 0.01f);
				}
				if (((uint)tDmgStack.nLastHitStatus & 8u) != 0)
				{
					num = Mathf.RoundToInt((float)num * ((10000f - tDmgStack.fLastBlockFactor) / 10000f));
				}
				num = Mathf.RoundToInt((float)num * ((float)tDmgStack.nLastBuffFactor * 0.0001f));
				if (bShowLog)
				{
					Debug.LogError("tDmgStack.nLastCriPercent " + tDmgStack.nLastCriPercent);
					Debug.LogError("tDmgStack.fLastBlockFactor " + tDmgStack.fLastBlockFactor);
					Debug.LogError("tDmgStack.nLastBuffFactor " + tDmgStack.nLastBuffFactor);
				}
			}
			return num;
		}
		return 0;
	}

	public int CalclDmgOnlyByLastHitStatus(StageObjBase tSOB, DmgStack tDmgStack, bool bShowLog = false)
	{
		nDmg = 0;
		int nCurrentWeapon = 0;
		if (((uint)tDmgStack.nLastHitStatus & 0x40u) != 0)
		{
			nCurrentWeapon = 1;
		}
		if (tDmgStack.nLastHitStatus == 0)
		{
			return 0;
		}
		if (tDmgStack.nLastHitStatus == 4)
		{
			nDmg = Mathf.RoundToInt(((float)nAtk * BulletData.f_EFFECT_X + BulletData.f_EFFECT_Y + (float)(int)tSOB.MaxHp * BulletData.f_EFFECT_Z) * 0.01f);
			return nDmg;
		}
		if (((uint)tDmgStack.nLastHitStatus & (true ? 1u : 0u)) != 0)
		{
			if (((uint)tDmgStack.nLastHitStatus & 0x20u) != 0)
			{
				return 0;
			}
			switch (BulletData.n_EFFECT)
			{
			case 1:
				if (bShowLog)
				{
					Debug.LogError("Skill " + BulletData.n_ID);
					Debug.LogError("tSOB.GetDEF() " + tSOB.GetDEF(nCurrentWeapon));
					Debug.LogError("AddAtkStatus " + refPSShoter.GetAddStatus(2, tDmgStack.nWeaponCheck));
					Debug.LogError("RatioAtkStatus " + refPSShoter.GetRatioStatus(2, tDmgStack.nWeaponCheck));
					Debug.LogError("tWS.nATK " + nAtk);
					Debug.LogError("tDmgStack.fLastDmgFactor " + tDmgStack.fLastDmgFactor);
				}
				nDmg = Mathf.RoundToInt((float)(nAtk - (int)tSOB.GetDEF(nCurrentWeapon)) * tDmgStack.fLastDmgFactor * 0.01f);
				nDmg = Mathf.RoundToInt((float)nDmg * (BulletData.f_EFFECT_X + (float)BulletLevel * BulletData.f_EFFECT_Y) * 0.01f);
				break;
			case 12:
			case 13:
				if (BulletData.n_EFFECT == 12)
				{
					nDmg = Mathf.RoundToInt((float)(int)tSOB.MaxHp * BulletData.f_EFFECT_X * 0.01f);
				}
				else
				{
					nDmg = Mathf.RoundToInt(((float)(int)tSOB.MaxHp + tDmgStack.fLastDmgFactor) * BulletData.f_EFFECT_X * 0.01f);
				}
				break;
			case 30:
				nDmg = Mathf.RoundToInt((float)(int)tSOB.MaxHp * BulletData.f_EFFECT_X * 0.01f) + Mathf.RoundToInt((float)(int)tSOB.Hp * BulletData.f_EFFECT_Y * 0.01f);
				if ((float)nDmg >= (float)(int)tSOB.Hp - BulletData.f_EFFECT_Z)
				{
					nDmg = Mathf.RoundToInt((float)(int)tSOB.Hp - BulletData.f_EFFECT_Z);
				}
				break;
			default:
				if (bShowLog)
				{
					Debug.LogError("Skill " + BulletData.n_ID);
					Debug.LogError("tSOB.GetDEF() " + tSOB.GetDEF(nCurrentWeapon));
					Debug.LogError("AddAtkStatus " + refPSShoter.GetAddStatus(2, tDmgStack.nWeaponCheck));
					Debug.LogError("RatioAtkStatus " + refPSShoter.GetRatioStatus(2, tDmgStack.nWeaponCheck));
					Debug.LogError("tWS.nATK " + nAtk);
					Debug.LogError("tDmgStack.fLastDmgFactor " + tDmgStack.fLastDmgFactor);
				}
				nDmg = Mathf.RoundToInt((float)(nAtk - (int)tSOB.GetDEF(nCurrentWeapon)) * tDmgStack.fLastDmgFactor * 0.01f);
				nDmg = Mathf.RoundToInt((float)nDmg * (BulletData.f_EFFECT_X + (float)BulletLevel * BulletData.f_EFFECT_Y) * 0.01f);
				break;
			}
			if (BulletData.n_EFFECT != 12 && BulletData.n_EFFECT != 13)
			{
				if (((uint)tDmgStack.nLastHitStatus & 2u) != 0)
				{
					nDmg = Mathf.RoundToInt((float)nDmg * ((10000f + (float)tDmgStack.nLastCriPercent) / 10000f));
					nDmg = Mathf.RoundToInt((float)nDmg * tDmgStack.fCriDmgFactor * 0.01f);
				}
				if (((uint)tDmgStack.nLastHitStatus & 8u) != 0)
				{
					nDmg = Mathf.RoundToInt((float)nDmg * ((10000f - tDmgStack.fLastBlockFactor) / 10000f));
				}
				nDmg = Mathf.RoundToInt((float)nDmg * ((float)tDmgStack.nLastBuffFactor * 0.0001f));
				if (bShowLog)
				{
					Debug.LogError("tDmgStack.nLastCriPercent " + tDmgStack.nLastCriPercent);
					Debug.LogError("tDmgStack.fLastBlockFactor " + tDmgStack.fLastBlockFactor);
					Debug.LogError("tDmgStack.nLastBuffFactor " + tDmgStack.nLastBuffFactor);
				}
			}
			return nDmg;
		}
		return 0;
	}

	protected void ReflashMissFactor()
	{
		if (refPBMShoter != null)
		{
			fMissFactor = refPBMShoter.sBuffStatus.fMissPercent;
		}
	}

	private int CalclDmgOnly(SKILL_TABLE pData, StageObjBase tSOB)
	{
		int nCurrentWeapon = 0;
		nDmg = 0;
		nBaseAtk = 0;
		damageType = VisualDamage.DamageType.Normal;
		nLastHitStatus = 1;
		if ((int)tSOB.GetCurrentWeapon() == 1)
		{
			nCurrentWeapon = 1;
			nLastHitStatus |= 64;
		}
		switch (pData.n_EFFECT)
		{
		case 0:
			if ((bool)tSOB.IsUnBreakX() && pData.n_TARGET != 2 && pData.n_TARGET != 3)
			{
				nLastHitStatus |= 128;
				return 5;
			}
			nLastHitStatus = 0;
			return 4;
		case 10:
			nLastHitStatus = 0;
			return 4;
		case 2:
			nDmg = Mathf.RoundToInt(((float)nAtk * pData.f_EFFECT_X + pData.f_EFFECT_Y + (float)(int)tSOB.MaxHp * pData.f_EFFECT_Z) * 0.01f);
			nLastHitStatus = 4;
			nDmg = (int)((float)(nDmg * (100 + refPBMShoter.sBuffStatus.nHealEnhance)) / 100f);
			return 3;
		case 3:
		case 5:
		case 6:
		case 14:
		case 16:
			nLastHitStatus = 16;
			return 1;
		case 4:
		case 7:
		case 8:
			return 4;
		case 18:
		case 19:
		case 20:
			if ((bool)tSOB.IsUnBreakX() && pData.n_TARGET != 2 && pData.n_TARGET != 3)
			{
				nLastHitStatus |= 128;
				return 5;
			}
			return 4;
		case 24:
			if ((bool)tSOB.IsUnBreakX())
			{
				return 5;
			}
			if (tSOB.GetSOBType() == 1 && StageUpdate.bIsHost)
			{
				OrangeCharacter orangeCharacter = tSOB as OrangeCharacter;
				if ((orangeCharacter.bNeedUpdateAlways || MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.CheckPlayerPause(orangeCharacter.sNetSerialID)) && orangeCharacter != null && !orangeCharacter.IsDead() && !orangeCharacter.IsInvincible)
				{
					bool flag2 = true;
					if (pData.f_EFFECT_X != 0f && !orangeCharacter.selfBuffManager.CheckHasEffectByCONDITIONID((int)pData.f_EFFECT_X))
					{
						flag2 = false;
					}
					if (flag2)
					{
						orangeCharacter.Controller.LogicPosition = new VInt3(refPBMShoter.SOB._transform.position);
						orangeCharacter._transform.position = orangeCharacter.Controller.LogicPosition.vec3;
					}
				}
			}
			return 4;
		case 26:
			nLastHitStatus = 0;
			return 4;
		default:
		{
			bool flag = false;
			if (pData.n_EFFECT == 12 || pData.n_EFFECT == 13 || pData.n_EFFECT == 30)
			{
				flag = true;
			}
			else if ((int)tSOB.GetDOD(nCurrentWeapon) - nHit < OrangeBattleUtility.Random(0, 10000) && fMissFactor * 100f <= (float)OrangeBattleUtility.Random(0, 10000))
			{
				flag = true;
			}
			if (flag)
			{
				if (pData.n_EFFECT != 13)
				{
					if ((bool)tSOB.IsUnBreakX())
					{
						nLastHitStatus |= 128;
						return 5;
					}
					if ((bool)tSOB.IsUnBreak() || (tSOB.IsInvincible && tHurtPassParam.LVMax != 9999))
					{
						nLastHitStatus |= 32;
						return 0;
					}
				}
				switch (pData.n_EFFECT)
				{
				case 1:
					if (pData.s_FIELD != "null")
					{
						fLastDmgFactor = 100f + refPBMShoter.sBuffStatus.fAtkDmgPercent - tSOB.selfBuffManager.sBuffStatus.fReduceDmgPercent;
					}
					else
					{
						fLastDmgFactor = fDmgFactor - tSOB.selfBuffManager.sBuffStatus.fReduceDmgPercent;
					}
					nBaseAtk = Mathf.RoundToInt((float)(nAtk - (int)tSOB.GetDEF(nCurrentWeapon)) * fLastDmgFactor * 0.01f);
					nBaseDmg = Mathf.RoundToInt((float)nBaseAtk * (pData.f_EFFECT_X + (float)BulletLevel * pData.f_EFFECT_Y) * 0.01f);
					nDmg = nBaseDmg;
					if (pData.f_EFFECT_Z != 0f && bCanUseInEventBullet)
					{
						EventManager.StageEventCall stageEventCall = new EventManager.StageEventCall();
						stageEventCall.nID = (int)pData.f_EFFECT_Z;
						Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_EVENT_CALL, stageEventCall);
					}
					break;
				case 12:
				case 13:
					if (pData.n_EFFECT == 12)
					{
						nBaseAtk = (nBaseDmg = (nDmg = Mathf.RoundToInt((float)(int)tSOB.MaxHp * pData.f_EFFECT_X * 0.01f)));
						fLastDmgFactor = (int)tSOB.MaxHp;
					}
					else
					{
						nBaseAtk = (nBaseDmg = (nDmg = Mathf.RoundToInt((float)((int)tSOB.MaxHp + tSOB.selfBuffManager.sBuffStatus.nEnergyShield) * pData.f_EFFECT_X * 0.01f)));
						fLastDmgFactor = tSOB.selfBuffManager.sBuffStatus.nEnergyShield;
					}
					if (pData.f_EFFECT_Z != 0f)
					{
						EventManager.StageEventCall stageEventCall2 = new EventManager.StageEventCall();
						stageEventCall2.nID = (int)pData.f_EFFECT_Z;
						Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_EVENT_CALL, stageEventCall2);
					}
					break;
				case 30:
					nBaseAtk = (nBaseDmg = (nDmg = Mathf.RoundToInt((float)(int)tSOB.MaxHp * pData.f_EFFECT_X * 0.01f) + Mathf.RoundToInt((float)(int)tSOB.Hp * pData.f_EFFECT_Y * 0.01f)));
					if ((float)nDmg >= (float)(int)tSOB.Hp - pData.f_EFFECT_Z)
					{
						nBaseAtk = (nBaseDmg = (nDmg = Mathf.RoundToInt((float)(int)tSOB.Hp - pData.f_EFFECT_Z)));
					}
					fLastDmgFactor = (int)tSOB.MaxHp;
					break;
				default:
					if (pData.s_FIELD != "null")
					{
						fLastDmgFactor = 100f + refPBMShoter.sBuffStatus.fAtkDmgPercent - tSOB.selfBuffManager.sBuffStatus.fReduceDmgPercent;
					}
					else
					{
						fLastDmgFactor = fDmgFactor - tSOB.selfBuffManager.sBuffStatus.fReduceDmgPercent;
					}
					nBaseAtk = Mathf.RoundToInt((float)(nAtk - (int)tSOB.GetDEF(nCurrentWeapon)) * fLastDmgFactor * 0.01f);
					nBaseDmg = Mathf.RoundToInt((float)nBaseAtk * (pData.f_EFFECT_X + (float)BulletLevel * pData.f_EFFECT_Y) * 0.01f);
					nDmg = nBaseDmg;
					break;
				}
				if (pData.n_EFFECT != 12 && pData.n_EFFECT != 13)
				{
					nLastCriPercent = nCri;
					if ((float)(nLastCriPercent - (int)tSOB.GetReduceCriPercent(nCurrentWeapon)) * fCriFactor >= (float)OrangeBattleUtility.Random(0, 1000000))
					{
						nLastHitStatus |= 2;
						damageType = VisualDamage.DamageType.Cri;
						nLastCriPercent = nCriDmgPercent;
						if (refPBMShoter.SOB as OrangeCharacter != null)
						{
							nLastCriPercent += OrangeConst.PLAYER_CRIDMG_BASE;
						}
						nLastCriPercent = ManagedSingleton<StageHelper>.Instance.StatusCorrection(nLastCriPercent, StageHelper.STAGE_RULE_STATUS.CRIDMG);
						nLastCriPercent = Convert.ToInt32((float)nLastCriPercent * refPSShoter.GetRatioStatus(6, nWeaponCheck));
						nLastCriPercent += refPSShoter.GetAddStatus(6, nWeaponCheck) - (int)tSOB.GetReduceCriDmgPercent(nCurrentWeapon);
						nDmg = Mathf.RoundToInt((float)nDmg * ((10000f + (float)nLastCriPercent) / 10000f));
						nDmg = Mathf.RoundToInt((float)nDmg * fCriDmgFactor * 0.01f);
					}
					if ((int)tSOB.GetBlock() - nReduceBlockPercent >= OrangeBattleUtility.Random(0, 10000))
					{
						nLastHitStatus |= 8;
						damageType = VisualDamage.DamageType.Reduce;
						fLastBlockFactor = (int)tSOB.GetBlockDmgPercent();
						nDmg = Mathf.RoundToInt((float)nDmg * ((10000f - fLastBlockFactor) / 10000f));
					}
					if (nThrough < pData.n_THROUGH / 100)
					{
						nLastBuffFactor = pData.n_THROUGH_DAMAGE;
						tHurtPassParam.BulletFlg |= BulletFlag.Through;
					}
					else
					{
						nLastBuffFactor = 100;
						tHurtPassParam.BulletFlg ^= BulletFlag.Through;
					}
					nLastBuffFactor = Mathf.RoundToInt((float)nLastBuffFactor * (100f + GetSkillDmgBuff(pData, tSOB.selfBuffManager) - (float)GetSkillResistanceBuff(pData, tSOB, nDmg)));
					nDmg = Mathf.RoundToInt((float)nDmg * ((float)nLastBuffFactor * 0.0001f));
					nBaseAtk = Mathf.RoundToInt((float)nBaseAtk * ((float)nLastBuffFactor * 0.0001f));
				}
				return 2;
			}
			nLastHitStatus = 16;
			return 1;
		}
		}
	}

	public bool CaluDmg(SKILL_TABLE pData, Transform tTrans, float x = 0f, float y = 0f)
	{
		sShotPlayerID = "";
		xy = tTrans.position.xy();
		xy.x += x;
		xy.y += y;
		hitTarget = tTrans;
		hitShield = false;
		tStageObjParam = tTrans.GetComponent<StageObjParam>();
		if (tStageObjParam == null)
		{
			PlayerCollider component = tTrans.GetComponent<PlayerCollider>();
			if (component != null && component.IsDmgReduceShield())
			{
				hitShield = true;
				tStageObjParam = component.GetDmgReduceOwner();
			}
		}
		if ((bool)(tShotOC = refPBMShoter.SOB as OrangeCharacter))
		{
			if (refPBMShoter.SOB.sNetSerialID != MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify)
			{
				if (tStageObjParam != null && tStageObjParam.tLinkSOB != null)
				{
					OrangeCharacter orangeCharacter = tStageObjParam.tLinkSOB as OrangeCharacter;
					EnemyControllerBase enemyControllerBase = tStageObjParam.tLinkSOB as EnemyControllerBase;
					for (int i = 0; i < tStageObjParam.tLinkSOB.listDmgStack.Count; i++)
					{
						DmgStack dmgStack = tStageObjParam.tLinkSOB.listDmgStack[i];
						if ((int)dmgStack.nRecordID != nRecordID || (int)dmgStack.nNetID != nNetID)
						{
							continue;
						}
						nBaseDmg = (int)dmgStack.nHP - (int)dmgStack.nEndHP;
						bool num = tStageObjParam.tLinkSOB.RunDmgStack(dmgStack);
						if (num)
						{
							needPlayEndSE = (needWeaponImpactSE = false);
						}
						if (nBaseDmg <= 0)
						{
							if (enemyControllerBase != null)
							{
								SoundSource.PlaySE(enemyControllerBase.GuardSE[0], enemyControllerBase.GuardSE[1]);
								needWeaponImpactSE = false;
							}
						}
						else
						{
							PlayHitSE(orangeCharacter, tTrans);
						}
						tStageObjParam.tLinkSOB.listDmgStack.RemoveAt(i);
						return num;
					}
					if (pData != null && orangeCharacter != null && pData.n_EFFECT == 24 && orangeCharacter.sNetSerialID == MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify && orangeCharacter != null && !orangeCharacter.IsDead() && !orangeCharacter.IsInvincible && !orangeCharacter.IsUnBreakX())
					{
						bool flag = true;
						if (pData.f_EFFECT_X != 0f && !orangeCharacter.selfBuffManager.CheckHasEffectByCONDITIONID((int)pData.f_EFFECT_X))
						{
							flag = false;
						}
						if (flag)
						{
							orangeCharacter.Controller.LogicPosition = new VInt3(refPBMShoter.SOB._transform.position);
							orangeCharacter._transform.position = orangeCharacter.Controller.LogicPosition.vec3;
						}
					}
				}
				if ((bool)tStageObjParam)
				{
					PlayHitSE(tStageObjParam.GetComponent<OrangeCharacter>(), tTrans);
				}
				else
				{
					StageHurtObj component2 = tTrans.gameObject.GetComponent<StageHurtObj>();
					if (component2 != null)
					{
						nDmg = 0;
						int num2 = 0;
						if (!component2.IsCanAtk(refPBMShoter.SOB, pData))
						{
							return false;
						}
						int n_EFFECT = pData.n_EFFECT;
						if (n_EFFECT != 1 && (uint)(n_EFFECT - 12) <= 1u)
						{
							num2 = (nBaseAtk = (nDmg = Mathf.RoundToInt((float)component2.nMaxHP * pData.f_EFFECT_X * 0.01f)));
						}
						else
						{
							num2 = Mathf.RoundToInt((float)nAtk * fDmgFactor * 0.01f);
							nDmg = Mathf.RoundToInt((float)num2 * (pData.f_EFFECT_X + (float)BulletLevel * pData.f_EFFECT_Y) * 0.01f);
						}
						if ((float)nCri * fCriFactor >= (float)OrangeBattleUtility.Random(0, 1000000))
						{
							Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.CAMERA_SHAKE, 1f, false);
							nDmg = Mathf.RoundToInt((float)nDmg * fCriDmgFactor * 0.01f);
						}
						if (nThrough < pData.n_THROUGH / 100)
						{
							nDmg = Mathf.RoundToInt((float)(nDmg * pData.n_THROUGH_DAMAGE) * 0.01f);
						}
						float skillDmgBuff = GetSkillDmgBuff(pData, null);
						nDmg = Mathf.RoundToInt((float)nDmg * (100f + skillDmgBuff) * 0.01f);
						if (component2.tempHP > nDmg)
						{
							PlayHitSE(null, tTrans);
						}
					}
				}
				if (!StageUpdate.bIsHost || !tShotOC.bNeedUpdateAlways)
				{
					return false;
				}
			}
			sShotPlayerID = refPBMShoter.SOB.sNetSerialID;
		}
		if (StageUpdate.bWaitReconnect)
		{
			return false;
		}
		if (tStageObjParam != null && tStageObjParam.tLinkSOB != null && ((1 << tStageObjParam.gameObject.layer) & (int)BlockMask) == 0)
		{
			OrangeCharacter orangeCharacter2 = tStageObjParam.tLinkSOB as OrangeCharacter;
			EnemyControllerBase enemyControllerBase2 = tStageObjParam.tLinkSOB as EnemyControllerBase;
			tNetDmgStack.vHitBack = Vector3.zero;
			if (orangeCharacter2 != null)
			{
				xy.y += 1.5f;
			}
			if ((int)tStageObjParam.tLinkSOB.Hp > 0)
			{
				if (tShotOC == null)
				{
					if (orangeCharacter2 != null)
					{
						if (orangeCharacter2.sPlayerID != MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify && (!StageUpdate.bIsHost || !orangeCharacter2.bNeedUpdateAlways))
						{
							PlayHitSE(orangeCharacter2, tTrans);
							return false;
						}
					}
					else if (enemyControllerBase2 != null && !StageUpdate.bIsHost)
					{
						return false;
					}
				}
				if (tStageObjParam.tLinkSOB.selfBuffManager.CheclIgnoreSkill(pData))
				{
					return false;
				}
				if (tStageObjParam.tLinkSOB.tRefPassiveskill != null && tStageObjParam.tLinkSOB.tRefPassiveskill.CheclIgnoreSkill(pData, tStageObjParam.tLinkSOB.GetCurrentWeaponCheck()))
				{
					return false;
				}
				tHurtPassParam.LVMax = pData.n_LVMAX;
				int num3 = CalclDmgOnly(pData, tStageObjParam.tLinkSOB);
				if (num3 == 2)
				{
					if (pData.n_HITBACK > 0)
					{
						tNetDmgStack.vHitBack = tStageObjParam.tLinkSOB.HitBack(pData.n_HITBACK, Direction);
					}
					bool flag2 = false;
					if (nDmg <= 0)
					{
						nDmg = 1;
					}
					if (damageType == VisualDamage.DamageType.Cri)
					{
						if (nBaseDmg <= 0)
						{
							nBaseDmg = 1;
						}
						Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.CAMERA_SHAKE, (float)nDmg / (float)nBaseDmg, false);
					}
					tempHp = tStageObjParam.tLinkSOB.Hp;
					tHurtPassParam.dmg = nDmg;
					tHurtPassParam.nSubPartID = tStageObjParam.nSubPartID;
					tHurtPassParam.owner = Owner;
					tHurtPassParam.wpnType = (WeaponType)nWeaponType;
					tHurtPassParam.vBulletDis = xy - base.transform.position.xy();
					tHurtPassParam.S_Direction = Direction;
					tHurtPassParam.Skill_Type = pData.n_TYPE;
					BulletFlag bulletFlag = (BulletFlag)pData.n_FLAG;
					if (hitShield)
					{
						bulletFlag |= BulletFlag.HitShield;
					}
					else if ((bulletFlag & BulletFlag.HitShield) == BulletFlag.HitShield)
					{
						bulletFlag ^= BulletFlag.HitShield;
					}
					if (tShotOC != null)
					{
						bulletFlag |= BulletFlag.IsPlayer;
					}
					tHurtPassParam.BulletFlg = bulletFlag;
					tNetDmgStack.nEnergyShield = tStageObjParam.tLinkSOB.selfBuffManager.sBuffStatus.nEnergyShield;
					tNetDmgStack.nBreakEnergyShieldBuffID = 0;
					if ((int)tNetDmgStack.nEnergyShield > 0 && nDmg >= (int)tNetDmgStack.nEnergyShield)
					{
						tNetDmgStack.nBreakEnergyShieldBuffID = tStageObjParam.tLinkSOB.selfBuffManager.GetEnergyShildBuffId();
					}
					currentHp = tStageObjParam.tLinkSOB.Hurt(tHurtPassParam);
					tempHp = (int)tStageObjParam.tLinkSOB.MaxHp - (int)tStageObjParam.tLinkSOB.DmgHp + (int)tStageObjParam.tLinkSOB.HealHp;
					if (currentHp <= 0)
					{
						flag2 = true;
						currentHp = 0;
						needPlayEndSE = (needWeaponImpactSE = false);
						if (enemyControllerBase2 != null && enemyControllerBase2.EnemyData.n_GET_CONDITION != -1)
						{
							refPBMShoter.AddBuff(enemyControllerBase2.EnemyData.n_GET_CONDITION, nBaseAtk, enemyControllerBase2.EnemyData.n_HP, pData.n_ID);
						}
					}
					tNetDmgStack.sPlayerID = tStageObjParam.tLinkSOB.sNetSerialID;
					tNetDmgStack.sShotPlayerID = sShotPlayerID;
					tNetDmgStack.nSkillID = pData.n_ID;
					tNetDmgStack.nDmg = nDmg;
					if (nNetID == 0)
					{
						tNetDmgStack.nRecordID = tStageObjParam.tLinkSOB.GetNowRecordNO();
					}
					else
					{
						tNetDmgStack.nRecordID = nRecordID;
					}
					tNetDmgStack.nNetID = nNetID;
					tNetDmgStack.sOwner = Owner;
					tNetDmgStack.nSubPartID = tStageObjParam.nSubPartID;
					tNetDmgStack.nDamageType = (int)damageType;
					tNetDmgStack.nWeaponCheck = nWeaponCheck;
					tNetDmgStack.nWeaponType = nWeaponType;
					tNetDmgStack.nHP = tempHp;
					tNetDmgStack.nEndHP = currentHp;
					StageObjBase tLinkSOB = tStageObjParam.tLinkSOB;
					tLinkSOB.DmgHp = (int)tLinkSOB.DmgHp + (tempHp - currentHp);
					tNetDmgStack.nDmgHP = tStageObjParam.tLinkSOB.DmgHp;
					tNetDmgStack.nHealHP = tStageObjParam.tLinkSOB.HealHp;
					tNetDmgStack.nLastHitStatus = nLastHitStatus;
					tNetDmgStack.fCriDmgFactor = fCriDmgFactor;
					tNetDmgStack.fLastDmgFactor = fLastDmgFactor;
					tNetDmgStack.nLastCriPercent = nLastCriPercent;
					tNetDmgStack.fLastBlockFactor = fLastBlockFactor;
					tNetDmgStack.nLastBuffFactor = nLastBuffFactor;
					StageUpdate.SyncStageObj(0, 0, JsonConvert.SerializeObject(tNetDmgStack, Formatting.None, new ObscuredValueConverter()), true);
					if (BattleInfoUI.Instance != null && (BattleInfoUI.Instance.NowStageTable.n_TYPE == 9 || BattleInfoUI.Instance.NowStageTable.n_TYPE == 10))
					{
						if (enemyControllerBase2 != null && ManagedSingleton<OrangeTableHelper>.Instance.IsBoss(enemyControllerBase2.EnemyData))
						{
							MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.AddPlayerDMG(sShotPlayerID, tNetDmgStack.nDmg, (int)tNetDmgStack.nHP - (int)tNetDmgStack.nEndHP, orangeCharacter2 != null);
						}
					}
					else
					{
						MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.AddPlayerDMG(sShotPlayerID, tNetDmgStack.nDmg, (int)tNetDmgStack.nHP - (int)tNetDmgStack.nEndHP, orangeCharacter2 != null);
					}
					if (tempHp == currentHp)
					{
						Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.SHOW_DAMAGE, xy, 0, tStageObjParam.tLinkSOB.GetSOBLayerMask(), VisualDamage.DamageType.Reduce);
						if (enemyControllerBase2 != null)
						{
							enemyControllerBase2.SoundSource.PlaySE(enemyControllerBase2.GuardSE[0], enemyControllerBase2.GuardSE[1]);
							needWeaponImpactSE = false;
						}
						else
						{
							SoundSource.PlaySE("HitSE", "ht_guard02");
							needWeaponImpactSE = false;
						}
					}
					else
					{
						if (currentHp > 0)
						{
							Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.SHOW_DAMAGE, xy, tempHp - currentHp, tStageObjParam.tLinkSOB.GetSOBLayerMask(), damageType);
						}
						else
						{
							Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.SHOW_DAMAGE, xy, nDmg - (int)tNetDmgStack.nEnergyShield, tStageObjParam.tLinkSOB.GetSOBLayerMask(), damageType);
						}
						if (!flag2)
						{
							RideArmorController rideArmorController = null;
							if (tStageObjParam.tLinkSOB != null)
							{
								rideArmorController = tStageObjParam.tLinkSOB as RideArmorController;
							}
							if (rideArmorController != null)
							{
								rideArmorController.PlaySeByDict(RideArmorController.MainStatus.HURT);
							}
							else if (_HitSE != null)
							{
								PlayHitSE(orangeCharacter2, tTrans);
							}
						}
					}
					if (orangeCharacter2 != null)
					{
						TriggerBuff(pData, orangeCharacter2.selfBuffManager, nBaseAtk, orangeCharacter2.IsLocalPlayer || visible);
					}
					else
					{
						TriggerBuff(pData, tStageObjParam.tLinkSOB.selfBuffManager, nBaseAtk, visible);
					}
					if (flag2 && !tStageObjParam.tLinkSOB.bIsNpcCpy)
					{
						CaluType caluType = CaluType.None;
						if (refPBMShoter.SOB as OrangeCharacter != null)
						{
							caluType = CaluType.Character;
						}
						else if (refPBMShoter.SOB as PetControllerBase != null)
						{
							caluType = CaluType.Pet;
						}
						if (caluType != 0)
						{
							if (enemyControllerBase2 != null)
							{
								BattleInfoUI.Instance.UpdateKillScoreUI(enemyControllerBase2.EnemyData.n_SCORE);
								MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.AddPlayerKillEnemyNum(sShotPlayerID);
							}
							if (orangeCharacter2 != null)
							{
								MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.AddPlayerKillNum(sShotPlayerID);
							}
							if (StageResManager.GetStageUpdate() != null && caluType == CaluType.Character)
							{
								StageResManager.GetStageUpdate().TriggerStageQuest(pData, refPBMShoter, tStageObjParam.tLinkSOB.selfBuffManager);
							}
						}
					}
					return flag2;
				}
				if (num3 == 1)
				{
					damageType = VisualDamage.DamageType.Miss;
					tNetDmgStack.sPlayerID = tStageObjParam.tLinkSOB.sNetSerialID;
					tNetDmgStack.sShotPlayerID = sShotPlayerID;
					tNetDmgStack.nDmg = 0;
					if (nNetID == 0)
					{
						tNetDmgStack.nRecordID = tStageObjParam.tLinkSOB.GetNowRecordNO();
					}
					else
					{
						tNetDmgStack.nRecordID = nRecordID;
					}
					tNetDmgStack.nNetID = nNetID;
					tNetDmgStack.sOwner = Owner;
					tNetDmgStack.nSubPartID = tStageObjParam.nSubPartID;
					tNetDmgStack.nDamageType = (int)damageType;
					tNetDmgStack.nWeaponType = nWeaponType;
					tNetDmgStack.nHP = tStageObjParam.tLinkSOB.Hp;
					tNetDmgStack.nEndHP = tStageObjParam.tLinkSOB.Hp;
					tNetDmgStack.nDmgHP = tStageObjParam.tLinkSOB.DmgHp;
					tNetDmgStack.nHealHP = tStageObjParam.tLinkSOB.HealHp;
					tNetDmgStack.nSkillID = 0;
					tNetDmgStack.nBreakEnergyShieldBuffID = 0;
					StageUpdate.SyncStageObj(0, 0, JsonConvert.SerializeObject(tNetDmgStack, Formatting.None, new ObscuredValueConverter()), true);
					Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.SHOW_DAMAGE, xy, 0, tStageObjParam.tLinkSOB.GetSOBLayerMask(), damageType);
				}
				else if (num3 == 4)
				{
					if (enemyControllerBase2 != null)
					{
						TriggerBuff(pData, tStageObjParam.tLinkSOB.selfBuffManager, nAtk, visible);
					}
					else if (orangeCharacter2 != null)
					{
						TriggerBuff(pData, orangeCharacter2.selfBuffManager, nAtk, orangeCharacter2.IsLocalPlayer || visible);
					}
					RefPassiveskill.TriggerSkill(pData, 1, 0, refPBMShoter, tStageObjParam.tLinkSOB.selfBuffManager, nAtk);
				}
				else if (num3 == 3)
				{
					tempHp = tStageObjParam.tLinkSOB.Hp;
					tStageObjParam.tLinkSOB.Heal(nDmg);
					StageObjBase tLinkSOB2 = tStageObjParam.tLinkSOB;
					tLinkSOB2.HealHp = (int)tLinkSOB2.HealHp + ((int)tStageObjParam.tLinkSOB.Hp - tempHp);
					damageType = VisualDamage.DamageType.Recover;
					Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.SHOW_DAMAGE, xy, nDmg, tStageObjParam.tLinkSOB.GetSOBLayerMask(), damageType);
					tNetDmgStack.sPlayerID = tStageObjParam.tLinkSOB.sNetSerialID;
					tNetDmgStack.sShotPlayerID = sShotPlayerID;
					tNetDmgStack.nDmg = -nDmg;
					if (nNetID == 0)
					{
						tNetDmgStack.nRecordID = tStageObjParam.tLinkSOB.GetNowRecordNO();
					}
					else
					{
						tNetDmgStack.nRecordID = nRecordID;
					}
					tNetDmgStack.nNetID = nNetID;
					tNetDmgStack.sOwner = Owner;
					tNetDmgStack.nSubPartID = tStageObjParam.nSubPartID;
					tNetDmgStack.nDamageType = (int)damageType;
					tNetDmgStack.nWeaponType = nWeaponType;
					tNetDmgStack.nHP = tempHp;
					tNetDmgStack.nEndHP = tStageObjParam.tLinkSOB.Hp;
					tNetDmgStack.nDmgHP = tStageObjParam.tLinkSOB.DmgHp;
					tNetDmgStack.nHealHP = tStageObjParam.tLinkSOB.HealHp;
					tNetDmgStack.nSkillID = 0;
					tNetDmgStack.nBreakEnergyShieldBuffID = 0;
					StageUpdate.SyncStageObj(0, 0, JsonConvert.SerializeObject(tNetDmgStack, Formatting.None, new ObscuredValueConverter()), true);
					if (enemyControllerBase2 != null)
					{
						TriggerBuff(pData, tStageObjParam.tLinkSOB.selfBuffManager, nBaseAtk, visible);
					}
					else if (orangeCharacter2 != null)
					{
						TriggerBuff(pData, orangeCharacter2.selfBuffManager, nBaseAtk, orangeCharacter2.IsLocalPlayer || visible);
					}
				}
				else
				{
					if (num3 == 0)
					{
						if (enemyControllerBase2 != null)
						{
							TriggerBuff(pData, tStageObjParam.tLinkSOB.selfBuffManager, nAtk, visible);
						}
						else if (orangeCharacter2 != null)
						{
							TriggerBuff(pData, orangeCharacter2.selfBuffManager, nAtk, orangeCharacter2.IsLocalPlayer || visible);
						}
					}
					damageType = VisualDamage.DamageType.Reduce;
					tNetDmgStack.sPlayerID = tStageObjParam.tLinkSOB.sNetSerialID;
					tNetDmgStack.sShotPlayerID = sShotPlayerID;
					tNetDmgStack.nDmg = 0;
					if (nNetID == 0)
					{
						tNetDmgStack.nRecordID = tStageObjParam.tLinkSOB.GetNowRecordNO();
					}
					else
					{
						tNetDmgStack.nRecordID = nRecordID;
					}
					tNetDmgStack.nNetID = nNetID;
					tNetDmgStack.sOwner = Owner;
					tNetDmgStack.nSubPartID = tStageObjParam.nSubPartID;
					tNetDmgStack.nDamageType = (int)damageType;
					tNetDmgStack.nWeaponType = nWeaponType;
					tNetDmgStack.nHP = tStageObjParam.tLinkSOB.Hp;
					tNetDmgStack.nEndHP = tStageObjParam.tLinkSOB.Hp;
					tNetDmgStack.nDmgHP = tStageObjParam.tLinkSOB.DmgHp;
					tNetDmgStack.nHealHP = tStageObjParam.tLinkSOB.HealHp;
					tNetDmgStack.nSkillID = 0;
					tNetDmgStack.nBreakEnergyShieldBuffID = 0;
					StageUpdate.SyncStageObj(0, 0, JsonConvert.SerializeObject(tNetDmgStack, Formatting.None, new ObscuredValueConverter()), true);
					Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.SHOW_DAMAGE, xy, 0, tStageObjParam.tLinkSOB.GetSOBLayerMask(), damageType);
					SoundSource.PlaySE("HitSE", "ht_guard02");
					needWeaponImpactSE = false;
				}
			}
		}
		else if (tTrans.gameObject.layer == ManagedSingleton<OrangeLayerManager>.Instance.BulletLayer)
		{
			BulletBase component3 = tTrans.gameObject.GetComponent<BulletBase>();
			if ((bool)component3)
			{
				int num4 = 0;
				int n_EFFECT2 = pData.n_EFFECT;
				num4 = Mathf.RoundToInt(pData.f_EFFECT_X + (float)BulletLevel * pData.f_EFFECT_Y);
				if (component3.Hurt(nAtk * num4 / 100, (WeaponType)nWeaponType) <= 0)
				{
					return true;
				}
				return false;
			}
		}
		else if (sShotPlayerID != "" || (sShotPlayerID == "" && StageUpdate.bIsHost))
		{
			StageHurtObj component4 = tTrans.gameObject.GetComponent<StageHurtObj>();
			if (component4 != null)
			{
				nDmg = 0;
				int num5 = 0;
				if (!component4.IsCanAtk(refPBMShoter.SOB, pData))
				{
					return false;
				}
				switch (pData.n_EFFECT)
				{
				case 12:
				case 13:
					num5 = (nBaseAtk = (nDmg = Mathf.RoundToInt((float)component4.nMaxHP * pData.f_EFFECT_X * 0.01f)));
					break;
				case 30:
					nDmg = Mathf.RoundToInt((float)component4.nMaxHP * pData.f_EFFECT_X * 0.01f) + Mathf.RoundToInt((float)component4.nMaxHP * pData.f_EFFECT_Y * 0.01f);
					if ((float)nDmg >= (float)component4.nMaxHP - pData.f_EFFECT_Z)
					{
						nDmg = Mathf.RoundToInt((float)component4.nMaxHP - pData.f_EFFECT_Z);
					}
					break;
				case 0:
				case 24:
					nDmg = 0;
					break;
				default:
					num5 = Mathf.RoundToInt((float)nAtk * fDmgFactor * 0.01f);
					nDmg = Mathf.RoundToInt((float)num5 * (pData.f_EFFECT_X + (float)BulletLevel * pData.f_EFFECT_Y) * 0.01f);
					break;
				}
				if ((float)nCri * fCriFactor >= (float)OrangeBattleUtility.Random(0, 1000000))
				{
					Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.CAMERA_SHAKE, 1f, false);
					nDmg = Mathf.RoundToInt((float)nDmg * fCriDmgFactor * 0.01f);
				}
				if (nThrough < pData.n_THROUGH / 100)
				{
					nDmg = Mathf.RoundToInt((float)(nDmg * pData.n_THROUGH_DAMAGE) * 0.01f);
				}
				float skillDmgBuff2 = GetSkillDmgBuff(pData, null);
				nDmg = Mathf.RoundToInt((float)nDmg * (100f + skillDmgBuff2) * 0.01f);
				component4.Hurt(nDmg, Direction, pData.n_ID, refPBMShoter.SOB);
				if (component4.tempHP > 0)
				{
					PlayHitSE(null, tTrans);
				}
			}
			else
			{
				HitBlockCallback.CheckTargetToInvoke(tTrans);
			}
			TriggerBuff(pData, null, 0);
		}
		return false;
	}

	public static BulletBase TryShotBullet(ShotBullerParam tShotBullerParam)
	{
		BulletBase bulletBase = null;
		switch ((BulletType)(short)tShotBullerParam.tSkillTable.n_TYPE)
		{
		case BulletType.Continuous:
			bulletBase = MonoBehaviourSingleton<PoolManager>.Instance.GetPoolObj<ContinuousBullet>(tShotBullerParam.tSkillTable.s_MODEL);
			break;
		case BulletType.Spray:
			bulletBase = MonoBehaviourSingleton<PoolManager>.Instance.GetPoolObj<SprayBullet>(tShotBullerParam.tSkillTable.s_MODEL);
			break;
		case BulletType.Collide:
			bulletBase = MonoBehaviourSingleton<PoolManager>.Instance.GetPoolObj<CollideBullet>(tShotBullerParam.tSkillTable.s_MODEL);
			((CollideBullet)bulletBase).bNeedBackPoolModelName = true;
			break;
		case BulletType.LrColliderBulle:
			bulletBase = MonoBehaviourSingleton<PoolManager>.Instance.GetPoolObj<LrColliderBullet>(tShotBullerParam.tSkillTable.s_MODEL);
			break;
		default:
			bulletBase = MonoBehaviourSingleton<PoolManager>.Instance.GetPoolObj<BulletBase>(tShotBullerParam.tSkillTable.s_MODEL);
			break;
		}
		if ((bool)bulletBase)
		{
			bulletBase.UpdateBulletData(tShotBullerParam.tSkillTable, tShotBullerParam.owner, tShotBullerParam.nRecordID, tShotBullerParam.nNetID, tShotBullerParam.nDirection);
			bulletBase.SetBulletAtk(tShotBullerParam.weaponStatus, tShotBullerParam.tBuffStatus, tShotBullerParam.refMOB_TABLE);
			bulletBase.BulletLevel = tShotBullerParam.nBulletLV;
			if (tShotBullerParam.pTransform != null)
			{
				bulletBase.Active(tShotBullerParam.pTransform, tShotBullerParam.pDirection, tShotBullerParam.pTargetMask);
			}
			else
			{
				bulletBase.Active(tShotBullerParam.vShotPos, tShotBullerParam.pDirection, tShotBullerParam.pTargetMask);
			}
		}
		return bulletBase;
	}

	public static BulletBase TryShotBullet(SKILL_TABLE tSkillTable, Transform pTransform, Vector3 pDirection, WeaponStatus weaponStatus, PerBuffManager.BuffStatus tBuffStatus, MOB_TABLE refMOB_TABLE, LayerMask pTargetMask, bool forcePlaySE = false, bool muteSE = false, bool isPetBullet = false, int bulletLevel = -1)
	{
		BulletBase bulletBase = null;
		switch ((BulletType)(short)tSkillTable.n_TYPE)
		{
		case BulletType.Continuous:
			bulletBase = MonoBehaviourSingleton<PoolManager>.Instance.GetPoolObj<ContinuousBullet>(tSkillTable.s_MODEL);
			break;
		case BulletType.Spray:
			bulletBase = MonoBehaviourSingleton<PoolManager>.Instance.GetPoolObj<SprayBullet>(tSkillTable.s_MODEL);
			break;
		case BulletType.Collide:
			bulletBase = MonoBehaviourSingleton<PoolManager>.Instance.GetPoolObj<CollideBullet>(tSkillTable.s_MODEL);
			break;
		case BulletType.LrColliderBulle:
			bulletBase = MonoBehaviourSingleton<PoolManager>.Instance.GetPoolObj<LrColliderBullet>(tSkillTable.s_MODEL);
			break;
		default:
			bulletBase = MonoBehaviourSingleton<PoolManager>.Instance.GetPoolObj<BulletBase>(tSkillTable.s_MODEL);
			break;
		}
		if ((bool)bulletBase)
		{
			string owner = "";
			if (refMOB_TABLE != null)
			{
				owner = refMOB_TABLE.n_ID.ToString();
			}
			bulletBase.UpdateBulletData(tSkillTable, owner);
			bulletBase.SetBulletAtk(weaponStatus, tBuffStatus, refMOB_TABLE);
			bulletBase.isMuteSE = muteSE;
			if (refMOB_TABLE != null)
			{
				bulletBase.isBossBullet = refMOB_TABLE.n_TYPE == 1 || refMOB_TABLE.n_TYPE == 2;
			}
			if (tSkillTable.s_HIT_BLOCK_SE != "null" && tSkillTable.n_THROUGH < 1000)
			{
				bulletBase.needPlayEndSE = true;
			}
			if (isPetBullet)
			{
				bulletBase.SetPetBullet();
			}
			if (bulletLevel > -1)
			{
				bulletBase.BulletLevel = bulletLevel;
			}
			bulletBase.Active(pTransform, pDirection, pTargetMask);
		}
		return bulletBase;
	}

	public static BulletBase TryShotBullet(SKILL_TABLE tSkillTable, Vector3 worldPos, Vector3 pDirection, WeaponStatus weaponStatus, PerBuffManager.BuffStatus tBuffStatus, MOB_TABLE refMOB_TABLE, LayerMask pTargetMask, bool forcePlaySE = false, bool muteSE = false)
	{
		BulletBase bulletBase = null;
		switch ((BulletType)(short)tSkillTable.n_TYPE)
		{
		case BulletType.Continuous:
			bulletBase = MonoBehaviourSingleton<PoolManager>.Instance.GetPoolObj<ContinuousBullet>(tSkillTable.s_MODEL);
			break;
		case BulletType.Spray:
			bulletBase = MonoBehaviourSingleton<PoolManager>.Instance.GetPoolObj<SprayBullet>(tSkillTable.s_MODEL);
			break;
		case BulletType.Collide:
			bulletBase = MonoBehaviourSingleton<PoolManager>.Instance.GetPoolObj<CollideBullet>(tSkillTable.s_MODEL);
			break;
		case BulletType.LrColliderBulle:
			bulletBase = MonoBehaviourSingleton<PoolManager>.Instance.GetPoolObj<LrColliderBullet>(tSkillTable.s_MODEL);
			break;
		default:
			bulletBase = MonoBehaviourSingleton<PoolManager>.Instance.GetPoolObj<BulletBase>(tSkillTable.s_MODEL);
			break;
		}
		if ((bool)bulletBase)
		{
			string owner = refMOB_TABLE.n_ID.ToString();
			bulletBase.UpdateBulletData(tSkillTable, owner);
			bulletBase.SetBulletAtk(weaponStatus, tBuffStatus, refMOB_TABLE);
			bulletBase.isForceSE = forcePlaySE;
			bulletBase.isMuteSE = muteSE;
			bulletBase.isBossBullet = refMOB_TABLE.n_TYPE == 1 || refMOB_TABLE.n_TYPE == 2;
			if (tSkillTable.s_HIT_BLOCK_SE != "null")
			{
				bulletBase.needPlayEndSE = false;
			}
			bulletBase.Active(worldPos, pDirection, pTargetMask);
		}
		return bulletBase;
	}

	public static T TryShotBullet<T>(SKILL_TABLE tSkillTable, Vector3 worldPos, Vector3 pDirection, WeaponStatus weaponStatus, PerBuffManager.BuffStatus tBuffStatus, MOB_TABLE refMOB_TABLE, LayerMask pTargetMask, bool forcePlaySE = false, bool muteSE = false) where T : BulletBase
	{
		BulletBase bulletBase = null;
		switch ((BulletType)(short)tSkillTable.n_TYPE)
		{
		case BulletType.Continuous:
			bulletBase = MonoBehaviourSingleton<PoolManager>.Instance.GetPoolObj<ContinuousBullet>(tSkillTable.s_MODEL);
			break;
		case BulletType.Spray:
			bulletBase = MonoBehaviourSingleton<PoolManager>.Instance.GetPoolObj<SprayBullet>(tSkillTable.s_MODEL);
			break;
		case BulletType.Collide:
			bulletBase = MonoBehaviourSingleton<PoolManager>.Instance.GetPoolObj<CollideBullet>(tSkillTable.s_MODEL);
			break;
		case BulletType.LrColliderBulle:
			bulletBase = MonoBehaviourSingleton<PoolManager>.Instance.GetPoolObj<LrColliderBullet>(tSkillTable.s_MODEL);
			break;
		default:
			bulletBase = MonoBehaviourSingleton<PoolManager>.Instance.GetPoolObj<BulletBase>(tSkillTable.s_MODEL);
			break;
		}
		if ((bool)bulletBase)
		{
			string owner = refMOB_TABLE.n_ID.ToString();
			bulletBase.UpdateBulletData(tSkillTable, owner);
			bulletBase.SetBulletAtk(weaponStatus, tBuffStatus, refMOB_TABLE);
			bulletBase.isForceSE = forcePlaySE;
			bulletBase.isMuteSE = muteSE;
			bulletBase.isBossBullet = refMOB_TABLE.n_TYPE == 1 || refMOB_TABLE.n_TYPE == 2;
			if (tSkillTable.s_HIT_BLOCK_SE != "null")
			{
				bulletBase.needPlayEndSE = true;
			}
			bulletBase.Active(worldPos, pDirection, pTargetMask);
		}
		return bulletBase as T;
	}

	public static void PreloadBullet<T>(SKILL_TABLE tSkillTable) where T : BulletBase
	{
		if (!(tSkillTable.s_MODEL == "DUMMY") && !MonoBehaviourSingleton<PoolManager>.Instance.ExistsInPool<T>(tSkillTable.s_MODEL))
		{
			MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad("prefab/bullet/" + tSkillTable.s_MODEL, tSkillTable.s_MODEL, delegate(GameObject obj)
			{
				T component = obj.GetComponent<T>();
				MonoBehaviourSingleton<PoolManager>.Instance.CreatePoolBaseLocal<T>(UnityEngine.Object.Instantiate(component), tSkillTable.s_MODEL, 5);
			});
		}
	}

	protected int GetAttack()
	{
		int n_EFFECT = BulletData.n_EFFECT;
		if (n_EFFECT == 1)
		{
			return Mathf.RoundToInt(BulletData.f_EFFECT_X + (float)BulletLevel * BulletData.f_EFFECT_Y);
		}
		return 0;
	}

	protected int GetAttack(SKILL_TABLE pData)
	{
		int n_EFFECT = pData.n_EFFECT;
		if (n_EFFECT == 1)
		{
			return Mathf.RoundToInt(pData.f_EFFECT_X + (float)BulletLevel * pData.f_EFFECT_Y);
		}
		return 0;
	}

	public void PlaySE(string s_acb, string cueName, bool ForceTrigger = false, Transform hitPoint = null)
	{
		if (ForceTrigger)
		{
			SoundSource.ForcePlaySE(s_acb, cueName);
		}
		else
		{
			SoundSource.PlaySE(s_acb, cueName);
		}
	}

	protected void GetHistGuardSE()
	{
		_HitGuardSE = ManagedSingleton<OrangeTableHelper>.Instance.ParseSE(BulletData.s_HIT_BLOCK_SE);
		if (_HitGuardSE != null)
		{
			if (_HitGuardSE[0] == "" || _HitGuardSE[0] == "null")
			{
				_HitGuardSE = new string[2]
				{
					"HitSE",
					AudioManager.FormatEnum2Name(HitSE.CRI_HITSE_HT_GUARD03.ToString())
				};
			}
			else
			{
				needPlayEndSE = true;
			}
		}
	}

	protected virtual void PlayHitSE(OrangeCharacter oc = null, Transform hitPoint = null)
	{
		if (isHitBlock)
		{
			BulletType bulletType = (BulletType)BulletData.n_TYPE;
			if ((uint)(bulletType - 4) > 1u)
			{
				return;
			}
		}
		if (_HitSE[0] == "HitSE" || _HitSE[0] == "BossSE")
		{
			if (oc != null)
			{
				if (oc.IsLocalPlayer)
				{
					if (oc.UseHitSE)
					{
						SoundSource.PlaySE("HitSE", "ht_player01");
					}
					else
					{
						SoundSource.PlaySE(_HitSE[0], _HitSE[1]);
					}
				}
				else if (oc.UseHitSE)
				{
					if (_HitSE[0] == "HitSE")
					{
						if (_HitSE[1].EndsWith("01") || _HitSE[1].EndsWith("03"))
						{
							string[] array = _HitSE[1].Split('0');
							if (array[0] == "ht_player")
							{
								array[0] = "ht_trw";
							}
							SoundSource.PlaySE(_HitSE[0], array[0] + "02");
						}
						else
						{
							SoundSource.PlaySE(_HitSE[0], _HitSE[1]);
						}
					}
					else
					{
						SoundSource.PlaySE(_HitSE[0], _HitSE[1]);
					}
				}
			}
			else
			{
				needPlayEndSE = false;
				if (_HitSE[1] == "ht_player01")
				{
					SoundSource.PlaySE("HitSE", "ht_trw02");
				}
				else if (tStageObjParam == null || tStageObjParam.tLinkSOB == null || tStageObjParam.tLinkSOB as RideArmorController == null)
				{
					SoundSource.PlaySE(_HitSE[0], _HitSE[1]);
				}
			}
		}
		else
		{
			SoundSource.PlaySE(_HitSE[0], _HitSE[1]);
		}
		needWeaponImpactSE = false;
	}

	private void OnBecameVisible()
	{
		preVisible = visible;
		visible = true;
	}

	private void OnBecameInvisible()
	{
		preVisible = visible;
		visible = false;
	}

	protected virtual bool CheckHitList(ref HashSet<Transform> hitList, Transform newHit)
	{
		if (hitList.Contains(newHit))
		{
			return true;
		}
		StageObjParam component = newHit.GetComponent<StageObjParam>();
		if ((bool)component)
		{
			OrangeCharacter orangeCharacter = component.tLinkSOB as OrangeCharacter;
			if ((bool)orangeCharacter)
			{
				CharacterControlBase component2 = orangeCharacter.GetComponent<CharacterControlBase>();
				if ((bool)component2)
				{
					foreach (Transform hit in hitList)
					{
						if (component2.CheckMyShield(hit))
						{
							return true;
						}
					}
				}
			}
		}
		else
		{
			PlayerCollider component3 = newHit.GetComponent<PlayerCollider>();
			if (component3 != null && component3.IsDmgReduceShield())
			{
				Transform dmgReduceOwnerTransform = component3.GetDmgReduceOwnerTransform();
				if (hitList.Contains(dmgReduceOwnerTransform))
				{
					return true;
				}
			}
		}
		return false;
	}

	protected Collider2D CheckHitTargetAndShield(Collider2D hitCollider, Collider2D target)
	{
		Collider2D collider2D = null;
		Collider2D collider2D2 = null;
		StageObjParam component = target.GetComponent<StageObjParam>();
		if ((bool)component)
		{
			collider2D = target;
			OrangeCharacter orangeCharacter = component.tLinkSOB as OrangeCharacter;
			if ((bool)orangeCharacter)
			{
				CharacterControlBase component2 = orangeCharacter.GetComponent<CharacterControlBase>();
				if ((bool)component2)
				{
					PlayerCollider myShield = component2.GetMyShield(true);
					if ((bool)myShield)
					{
						collider2D2 = myShield.GetCollider2D();
					}
				}
			}
		}
		else
		{
			collider2D2 = target;
			PlayerCollider component3 = target.GetComponent<PlayerCollider>();
			if ((bool)component3 && (bool)component3.GetDmgReduceOwner())
			{
				OrangeCharacter orangeCharacter2 = component3.GetDmgReduceOwner().tLinkSOB as OrangeCharacter;
				if ((bool)orangeCharacter2)
				{
					collider2D = orangeCharacter2.Controller.Collider2D;
				}
			}
		}
		if ((bool)collider2D && (bool)collider2D2 && hitCollider.IsTouching(collider2D) && hitCollider.IsTouching(collider2D2))
		{
			float num = Vector3.Distance(hitCollider.transform.position, collider2D.transform.position);
			float num2 = Vector3.Distance(hitCollider.transform.position, collider2D2.transform.position);
			if (num < num2)
			{
				return collider2D;
			}
			return collider2D2;
		}
		return target;
	}

	public void SetPetBullet()
	{
		isPetBullet = true;
	}

	public static int GetFinalATK(int originalAtk, ref RefPassiveskill refPassiveSkill, int weaponCheck)
	{
		if (refPassiveSkill == null)
		{
			return originalAtk;
		}
		return Convert.ToInt32((float)originalAtk * refPassiveSkill.GetRatioStatus(2, weaponCheck)) + refPassiveSkill.GetAddStatus(2, weaponCheck);
	}

	public static int GetFinalCRI(int originalCri, ref RefPassiveskill refPassiveSkill, int weaponCheck, bool isPlayer)
	{
		if (refPassiveSkill == null)
		{
			return originalCri;
		}
		int num = originalCri;
		if (isPlayer)
		{
			num += OrangeConst.PLAYER_CRI_BASE;
		}
		num = ManagedSingleton<StageHelper>.Instance.StatusCorrection(num, StageHelper.STAGE_RULE_STATUS.CRI);
		num = Convert.ToInt32((float)num * refPassiveSkill.GetRatioStatus(4, weaponCheck));
		return num + refPassiveSkill.GetAddStatus(4, weaponCheck);
	}

	protected void GetTargetByPerGameSaveData()
	{
		if (!StageUpdate.gbIsNetGame)
		{
			return;
		}
		List<string> perGameSaveData = StageUpdate.GetPerGameSaveData();
		for (int num = perGameSaveData.Count - 1; num >= 0; num--)
		{
			if (perGameSaveData[num].StartsWith("BB#"))
			{
				string[] array = perGameSaveData[num].Substring("BB#".Length).Split(',');
				int result;
				int.TryParse(array[0], out result);
				if (result == nNetID)
				{
					int.TryParse(array[1], out result);
					if (result == nRecordID)
					{
						Target = StageResManager.GetStageUpdate().GetSOBByNetSerialID(array[2]) as IAimTarget;
						perGameSaveData.RemoveAt(num);
						break;
					}
				}
			}
		}
	}

	protected void SendTargetMsg()
	{
		if (StageUpdate.gbIsNetGame && Target != null)
		{
			StageObjBase stageObjBase = Target as StageObjBase;
			if (stageObjBase != null)
			{
				string smsg = "BB#" + nNetID + "," + nRecordID + "," + stageObjBase.sNetSerialID;
				StageUpdate.SyncStageObj(4, 7, smsg, true);
			}
		}
	}

	protected bool GetBulletFlagByPerGameSaveData(int nSet)
	{
		if (!StageUpdate.gbIsNetGame)
		{
			return false;
		}
		List<string> perGameSaveData = StageUpdate.GetPerGameSaveData();
		for (int num = perGameSaveData.Count - 1; num >= 0; num--)
		{
			if (perGameSaveData[num].StartsWith("FB#"))
			{
				string[] array = perGameSaveData[num].Substring("FB#".Length).Split(',');
				int result;
				int.TryParse(array[0], out result);
				if (result == nNetID)
				{
					int.TryParse(array[1], out result);
					if (result == nRecordID)
					{
						int.TryParse(array[2], out result);
						if (result == nSet)
						{
							perGameSaveData.RemoveAt(num);
							return true;
						}
					}
				}
			}
		}
		return false;
	}

	public void SendBulletFlagMsg(int nSet)
	{
		if (StageUpdate.gbIsNetGame)
		{
			string smsg = "FB#" + nNetID + "," + nRecordID + "," + nSet;
			StageUpdate.SyncStageObj(4, 7, smsg, true);
		}
	}
}
