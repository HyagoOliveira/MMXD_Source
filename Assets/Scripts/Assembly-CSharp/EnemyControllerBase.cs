#define RELEASE
using System;
using System.Collections;
using System.Collections.Generic;
using CallbackDefs;
using CodeStage.AntiCheat.ObscuredTypes;
using Newtonsoft.Json;
using StageLib;
using UnityEngine;

[RequireComponent(typeof(Controller2D))]
public abstract class EnemyControllerBase : StageNetSyncObj, IAimTarget, ILogicUpdate
{
	protected enum AI_STATE
	{
		mob_001 = 0,
		mob_002 = 1,
		mob_003 = 2,
		mob_004 = 3,
		mob_005 = 4,
		mob_006 = 5,
		mob_007 = 6,
		mob_008 = 7,
		mob_009 = 8,
		mob_010 = 9
	}

	[Serializable]
	private class ExplosionFxInfo
	{
		[SerializeField]
		private bool enable;

		[SerializeField]
		private float offsetX;

		[SerializeField]
		private float offsetY;

		[SerializeField]
		private float size;

		public void Play(Vector3 position)
		{
			if (enable)
			{
				string p_fxName = "FX_MOB_EXPLODE0";
				string gStageName = StageUpdate.gStageName;
				if (gStageName == "stage04_1401_e1" || gStageName == "stage04_3301_e1")
				{
					p_fxName = "fxuse_mob_explode_000";
				}
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(p_fxName, new Vector3(position.x + offsetX, position.y + offsetY, position.z), Quaternion.identity, Array.Empty<object>());
			}
		}
	}

	protected AI_STATE AiState;

	protected OrangeTimer AiTimer;

	[HideInInspector]
	public int EnemyID;

	protected bool SkillFlag;

	[HideInInspector]
	public bool InGame;

	public float fAIWorkRange;

	public MOB_TABLE EnemyData;

	protected PoolManager _poolManager;

	public Controller2D Controller;

	protected bool _introReady;

	protected bool _unlockReady;

	protected Action IntroCallBack;

	public Callback DeadCallback;

	public float distanceDelta;

	protected VInt3 _velocity;

	protected VInt3 _velocityExtra;

	protected VInt3 _velocityShift;

	protected VInt _maxGravity;

	protected VInt3 TargetPos;

	protected bool IgnoreGravity_bak;

	public bool IgnoreGravity;

	protected bool IgnoreGlobalVelocity;

	protected CollideBullet _collideBullet;

	protected EnemyCollider[] _enemyCollider;

	protected CharacterMaterial _characterMaterial;

	protected int EquippedWeaponNum;

	protected WeaponStruct[] EnemyWeapons;

	protected int[] BulletOrder;

	protected OrangeTimer HSTimer;

	public Animator[] Animators;

	private float _shakeTime;

	private bool isShake;

	private const float ShakeTimeMax = 0.2f;

	private const float ShakeMovement = 0.1f;

	private const float Factor = 1f;

	private float _shakeLevel = 1.5f;

	private Vector3 BackPosition = Vector3.zero;

	[HideInInspector]
	public string[] GuardSE;

	[HideInInspector]
	public string[] ExplodeSE;

	[HideInInspector]
	public OrangeCharacter Target;

	[HideInInspector]
	public ObscuredInt[] PartHp;

	[SerializeField]
	private ExplosionFxInfo explosionFxInfo;

	[SerializeField]
	protected EnemyDieCollider ExplosionPart;

	protected LayerMask friendMask;

	protected LayerMask targetMask;

	protected LayerMask neutralMask;

	protected int preDirection = -1;

	protected float _easeSpeed = 0.25f;

	protected float[] _globalWaypoints;

	protected int _fromWaypointIndex;

	protected float _percentBetweenWaypoints;

	protected float _moveSpeedMultiplier;

	protected bool IsStun;

	protected bool IsStunStatus;

	[SerializeField]
	public Animator _animator;

	protected EnemyAutoAimSystem _enemyAutoAimSystem;

	public bool DisableMoveFall;

	protected bool visible;

	protected bool preBelow;

	private string Bornfx_Name;

	private Vector3 Bornfx_XY_SCL = new Vector3(0f, 0f, 1f);

	protected OrangeCharacter MainOC;

	protected bool KeepSpawnedMob;

	protected int MaxAllowedSubMob = -1;

	protected List<EnemyControllerBase> SpawnedMobList = new List<EnemyControllerBase>();

	public bool bDeadShock = true;

	[SerializeField]
	public string[] FallDownSE;

	protected bool _bDeadPlayCompleted = true;

	protected bool _bImmunityDeadArea;

	private OrangeCriSource _SSource;

	public bool bNeedDead;

	[SerializeField]
	protected ParticleSystem[] FxArray = new ParticleSystem[0];

	private Renderer meshRenderer;

	private bool alreadyStart;

	protected bool isFall;

	public bool DeadPlayCompleted
	{
		get
		{
			return _bDeadPlayCompleted;
		}
		set
		{
			_bDeadPlayCompleted = value;
		}
	}

	public OrangeCriSource SoundSource
	{
		get
		{
			if (_SSource == null)
			{
				_SSource = base.gameObject.GetComponent<OrangeCriSource>();
				if (_SSource == null)
				{
					_SSource = base.gameObject.AddComponent<OrangeCriSource>();
					if (base.gameObject.name.Contains("enemy_bs"))
					{
						_SSource.Initial(OrangeSSType.BOSS);
					}
					else
					{
						_SSource.Initial(OrangeSSType.ENEMY);
					}
				}
			}
			return _SSource;
		}
		set
		{
			OrangeCriSource component = base.gameObject.GetComponent<OrangeCriSource>();
			if (component != null)
			{
				UnityEngine.Object.Destroy(component);
			}
			_SSource = value;
		}
	}

	protected EnemyControllerBase SpawnSubMob(int id, Vector3 targetPosition, int targetDirection)
	{
		if (MaxAllowedSubMob != -1 && MaxAllowedSubMob >= SpawnedMobList.Count)
		{
			return null;
		}
		MOB_TABLE value;
		ManagedSingleton<OrangeDataManager>.Instance.MOB_TABLE_DICT.TryGetValue(id, out value);
		if (value == null)
		{
			return null;
		}
		EnemyControllerBase enemyControllerBase = StageUpdate.StageSpawnEnemyByMob(value, sNetSerialID + "0");
		if (!enemyControllerBase)
		{
			return null;
		}
		enemyControllerBase.UpdateEnemyID(value.n_ID);
		enemyControllerBase.SetPositionAndRotation(targetPosition, targetDirection == -1);
		enemyControllerBase.SetActive(true);
		SpawnedMobList.Add(enemyControllerBase);
		return enemyControllerBase;
	}

	protected void DestroyAllSpawnedMob()
	{
		foreach (EnemyControllerBase spawnedMob in SpawnedMobList)
		{
			if (spawnedMob.InGame)
			{
				spawnedMob.Hp = 0;
				spawnedMob.Hurt(new HurtPassParam());
			}
		}
		SpawnedMobList.Clear();
	}

	protected float Ease(float x)
	{
		return Mathf.Pow(x, 2f) / (Mathf.Pow(x, 2f) + Mathf.Pow(1f - x, 2f));
	}

	protected float CalculateVerticalMovement(bool raw = false)
	{
		_fromWaypointIndex %= _globalWaypoints.Length;
		int num = (_fromWaypointIndex + 1) % _globalWaypoints.Length;
		float num2 = Mathf.Abs(_globalWaypoints[_fromWaypointIndex] - _globalWaypoints[num]);
		_percentBetweenWaypoints += GameLogicUpdateManager.m_fFrameLen * _easeSpeed / num2;
		_percentBetweenWaypoints = Mathf.Clamp01(_percentBetweenWaypoints);
		float t = Ease(_percentBetweenWaypoints);
		float num3 = Mathf.Lerp(_globalWaypoints[_fromWaypointIndex], _globalWaypoints[num], t);
		if (_percentBetweenWaypoints >= 1f)
		{
			_percentBetweenWaypoints = 0f;
			_fromWaypointIndex++;
		}
		if (raw)
		{
			return num3;
		}
		return num3 - _transform.position.y;
	}

	protected virtual void Awake()
	{
		HSTimer = OrangeTimerManager.GetTimer();
		Singleton<GenericEventManager>.Instance.AttachEvent<GameObject, StageCtrlInsTruction>(EventManager.ID.STAGE_OBJ_CTRL, ObjCtrl);
		Singleton<GenericEventManager>.Instance.AttachEvent<NotifyCallBack, NotifyCallBack>(EventManager.ID.STAGE_OBJ_CTRL_PLAYSHOWANI, ObjCtrl_PlayShowWani);
		Singleton<GenericEventManager>.Instance.AttachEvent<StageCtrlInsTruction, string>(EventManager.ID.STAGE_OBJ_CTRL_SYNC_HP, ObjCtrl_SyncHP);
		Singleton<GenericEventManager>.Instance.AttachEvent<string, int, string>(EventManager.ID.STAGE_OBJ_CTRL_ENEMY_ACTION, ObjCtrl_EnemyAction);
		Collider2D[] componentsInChildren = GetComponentsInChildren<Collider2D>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].gameObject.AddOrGetComponent<StageObjParam>().tLinkSOB = this;
		}
		EnemyCollider[] componentsInChildren2 = GetComponentsInChildren<EnemyCollider>();
		for (int i = 0; i < componentsInChildren2.Length; i++)
		{
			componentsInChildren2[i].gameObject.AddOrGetComponent<StageObjParam>().tLinkSOB = this;
		}
		GuardTransform = new List<int>();
		AiTimer = OrangeTimerManager.GetTimer();
		_transform = base.transform;
		_enemyCollider = GetComponentsInChildren<EnemyCollider>();
		base.AllowAutoAim = true;
		base.AimTransform = _transform;
		base.AutoAimType = AimTargetType.Enemy;
		Controller = GetComponent<Controller2D>();
		_characterMaterial = GetComponent<CharacterMaterial>();
		if ((bool)_characterMaterial)
		{
			_characterMaterial.DefaultDissolveValue = 1;
		}
		_velocity = VInt3.zero;
		_maxGravity = OrangeBattleUtility.FP_MaxGravity;
		friendMask = ManagedSingleton<OrangeLayerManager>.Instance.PlayerUseMask;
		targetMask = ManagedSingleton<OrangeLayerManager>.Instance.EnemyUseMask;
		neutralMask = (int)ManagedSingleton<OrangeLayerManager>.Instance.EnemyUseMask | (int)ManagedSingleton<OrangeLayerManager>.Instance.PlayerUseMask;
		_poolManager = MonoBehaviourSingleton<PoolManager>.Instance;
		if ((bool)ExplosionPart)
		{
			MonoBehaviourSingleton<PoolManager>.Instance.CreatePoolBaseLocal<EnemyDieCollider>(UnityEngine.Object.Instantiate(ExplosionPart), ExplosionPart.Name, 1);
		}
		if (EnemyID != 0)
		{
			UpdateEnemyID(EnemyID);
		}
		selfBuffManager.Init(this);
		meshRenderer = OrangeGameUtility.AddOrGetRenderer<Renderer>(base.gameObject);
		Animators = base.gameObject.GetComponentsInChildren<Animator>();
		AnimatorSoundHelper[] componentsInChildren3 = base.gameObject.GetComponentsInChildren<AnimatorSoundHelper>();
		for (int i = 0; i < componentsInChildren3.Length; i++)
		{
			componentsInChildren3[i].SoundSource = SoundSource;
		}
	}

	public override void ResetStatus()
	{
		if (!alreadyStart)
		{
			alreadyStart = true;
			Start();
		}
	}

	protected virtual void Start()
	{
		Singleton<GenericEventManager>.Instance.AttachEvent<OrangeCharacter>(EventManager.ID.LOCAL_PLAYER_SPWAN, EventPlayerSpawn);
	}

	protected virtual void OnDestroy()
	{
		Singleton<GenericEventManager>.Instance.DetachEvent<OrangeCharacter>(EventManager.ID.LOCAL_PLAYER_SPWAN, EventPlayerSpawn);
		Singleton<GenericEventManager>.Instance.DetachEvent<GameObject, StageCtrlInsTruction>(EventManager.ID.STAGE_OBJ_CTRL, ObjCtrl);
		Singleton<GenericEventManager>.Instance.DetachEvent<NotifyCallBack, NotifyCallBack>(EventManager.ID.STAGE_OBJ_CTRL_PLAYSHOWANI, ObjCtrl_PlayShowWani);
		Singleton<GenericEventManager>.Instance.DetachEvent<StageCtrlInsTruction, string>(EventManager.ID.STAGE_OBJ_CTRL_SYNC_HP, ObjCtrl_SyncHP);
		Singleton<GenericEventManager>.Instance.DetachEvent<string, int, string>(EventManager.ID.STAGE_OBJ_CTRL_ENEMY_ACTION, ObjCtrl_EnemyAction);
	}

	public void BaseLogicUpdate(bool ignoreFrameLength = false)
	{
		if (!Activate)
		{
			return;
		}
		float num = (ignoreFrameLength ? 1f : GameLogicUpdateManager.m_fFrameLen);
		VInt3 vInt = (IgnoreGlobalVelocity ? VInt3.zero : OrangeBattleUtility.GlobalVelocityExtra);
		BaseUpdate();
		UpdateGravity();
		if (EnemyData.n_TYPE == 0)
		{
			if (IsStun)
			{
				if ((bool)_animator)
				{
					if (!IsStunStatus)
					{
						_animator.speed = 0f;
					}
					else
					{
						_animator.speed = _moveSpeedMultiplier;
					}
				}
				Controller.Move(_velocityExtra * num);
			}
			else
			{
				if ((bool)_animator)
				{
					_animator.speed = _moveSpeedMultiplier;
				}
				Controller.Move((Get_MulBuff_Velocity(_velocity) + _velocityExtra + vInt) * num);
			}
		}
		else
		{
			Controller.Move((_velocity + _velocityExtra) * num + _velocityShift);
		}
		distanceDelta = Vector3.Distance(base.transform.localPosition, Controller.LogicPosition.vec3) * (Time.deltaTime / GameLogicUpdateManager.m_fFrameLen);
		_velocityExtra = VInt3.zero;
		_velocityShift = VInt3.zero;
		FalldownUpdate();
	}

	public virtual void FalldownUpdate()
	{
		if (!isFall)
		{
			isFall = !Controller.Collisions.below;
		}
		else if (Controller.Collisions.below)
		{
			isFall = false;
			if (FallDownSE != null && FallDownSE.Length >= 2)
			{
				PlaySE(FallDownSE[0], FallDownSE[1]);
			}
		}
	}

	public virtual void LogicUpdate()
	{
		BaseLogicUpdate();
	}

	private void Shake()
	{
		if (_shakeTime <= 0f)
		{
			base.transform.position = BackPosition;
			isShake = false;
		}
		else
		{
			base.transform.position = new Vector3(base.transform.position.x + UnityEngine.Random.insideUnitSphere.x * 0.1f * _shakeLevel, base.transform.position.y, base.transform.position.z);
			_shakeTime -= Time.deltaTime * 1f;
		}
	}

	public void ActiveShake(float time)
	{
		BackPosition = base.transform.position;
		_shakeTime = time;
		isShake = true;
	}

	protected virtual void UpdateGravity()
	{
		if (isShake)
		{
			Shake();
		}
		if (IgnoreGravity_bak != IgnoreGravity)
		{
			if (IgnoreGravity)
			{
				_velocity.y = 0;
			}
			IgnoreGravity_bak = IgnoreGravity;
		}
		if (!IgnoreGravity)
		{
			if ((_velocity.y < 0 && Controller.Collisions.below) || (_velocity.y > 0 && Controller.Collisions.above))
			{
				_velocity.y = 0;
			}
			_velocity.y += OrangeBattleUtility.FP_Gravity * GameLogicUpdateManager.g_fixFrameLenFP / 1000;
			_velocity.y = IntMath.Sign(_velocity.y) * IntMath.Min(IntMath.Abs(_velocity.y), IntMath.Abs(_maxGravity.i));
		}
	}

	public virtual void Explosion()
	{
		if ((bool)ExplosionPart)
		{
			EnemyDieCollider poolObj = MonoBehaviourSingleton<PoolManager>.Instance.GetPoolObj<EnemyDieCollider>(ExplosionPart.Name);
			poolObj.transform.position = base.transform.position;
			poolObj.ActiveExplosion();
		}
		explosionFxInfo.Play(AimPosition);
	}

	public void PartsExplode(Transform partsTransform)
	{
		if ((bool)ExplosionPart)
		{
			EnemyDieCollider poolObj = MonoBehaviourSingleton<PoolManager>.Instance.GetPoolObj<EnemyDieCollider>(ExplosionPart.Name);
			poolObj.transform.position = partsTransform.position;
			poolObj.ActiveExplosion();
		}
		explosionFxInfo.Play(partsTransform.position);
	}

	public override Vector3 HitBack(int BackPos, Vector3 dir)
	{
		Vector3 vector;
		if (EnemyData.n_TYPE == 0)
		{
			vector = new Vector3(5 * BackPos, 0f, 0f) * dir.x;
			_velocityExtra += new VInt3(vector);
		}
		else
		{
			vector = Vector3.zero;
		}
		return vector;
	}

	public override void HitBackV3(Vector3 vHitBackNet)
	{
		_velocityExtra += new VInt3(vHitBackNet);
	}

	public VInt3 Get_MulBuff_Velocity(VInt3 mVY)
	{
		if (_moveSpeedMultiplier != 1f)
		{
			return mVY * _moveSpeedMultiplier;
		}
		return mVY;
	}

	protected virtual void SetStunStatus(bool enable)
	{
		IsStunStatus = false;
	}

	public override void SetStun(bool enable, bool bCheckOtherObj = true)
	{
		IsStun = enable;
		SetStunStatus(enable);
		preBelow = Controller.Collisions.below;
	}

	public override int GetNowRecordNO()
	{
		return -1;
	}

	public void BaseUpdate()
	{
		if (bNeedDead)
		{
			if ((int)Hp > 0)
			{
				Hp = 0;
				Hurt(new HurtPassParam());
			}
			bNeedDead = false;
		}
		selfBuffManager.UpdateBuffTime();
		_moveSpeedMultiplier = 1f + selfBuffManager.sBuffStatus.fMoveSpeed * 0.01f;
		for (int i = 0; i < EquippedWeaponNum; i++)
		{
			EnemyWeapons[i].LastUseTimer += GameLogicUpdateManager.m_fFrameLenMS;
		}
		CheckDmgStackByRecordNO();
	}

	public virtual void ObjCtrl(GameObject tObj, StageCtrlInsTruction tSCE)
	{
		if (EnemyID == 0)
		{
			return;
		}
		switch (tSCE.tStageCtrl)
		{
		case 18:
			enemylock();
			break;
		case 19:
			Unlock();
			break;
		case 23:
			if (IsAlive())
			{
				bNeedDead = true;
			}
			break;
		}
	}

	public void ObjCtrl_PlayShowWani(NotifyCallBack tNCB, NotifyCallBack tNCB2)
	{
		if (EnemyID != 0 && (int)Hp > 0)
		{
			if (ManagedSingleton<OrangeTableHelper>.Instance.IsBoss(EnemyData))
			{
				tNCB2.CallCB();
			}
			BossIntro(tNCB.CallCB);
		}
	}

	public void ObjCtrl_SyncHP(StageCtrlInsTruction tSCE, string sTmpNetID)
	{
		if (EnemyID != 0 && sTmpNetID == sNetSerialID && (int)Hp > 0)
		{
			int num = (int)tSCE.nParam2;
			Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.SHOW_DAMAGE, base.transform.position.xy(), num, BulletScriptableObject.Instance.BulletLayerMaskEnemy, VisualDamage.DamageType.Normal);
			HurtPassParam hurtPassParam = new HurtPassParam();
			hurtPassParam.dmg = num;
			Hurt(hurtPassParam);
		}
	}

	public void ObjCtrl_EnemyAction(string sTmpNetID, int nTmpID, string nTmpMsg)
	{
		if (EnemyID != 0 && sTmpNetID == sNetSerialID)
		{
			UpdateStatus(nTmpID, nTmpMsg);
		}
	}

	public override bool IsAlive()
	{
		if (InGame)
		{
			return (int)Hp > 0;
		}
		return false;
	}

	public virtual void enemylock()
	{
		if (InGame)
		{
			Activate = false;
		}
	}

	public virtual void Unlock()
	{
		_unlockReady = true;
		base.AllowAutoAim = true;
		SetColliderEnable(true);
		if (InGame && (int)Hp > 0)
		{
			Activate = true;
		}
	}

	public virtual void BossIntro(Action cb)
	{
		if (ManagedSingleton<OrangeTableHelper>.Instance.IsBoss(EnemyData))
		{
			cb();
		}
	}

	protected bool IsWeaponAvailable(int weaponID)
	{
		if (EnemyWeapons[weaponID].LastUseTimer.IsStarted() && !(EnemyWeapons[weaponID].MagazineRemain > 0f))
		{
			return EnemyWeapons[weaponID].LastUseTimer.GetMillisecond() > EnemyWeapons[weaponID].BulletData.n_RELOAD;
		}
		return true;
	}

	public override int GetSOBType()
	{
		return 2;
	}

	public override string GetSOBName()
	{
		return EnemyData.s_NAME;
	}

	public override ObscuredInt GetDOD(int nCurrentWeapon)
	{
		return EnemyData.n_DODGE;
	}

	public override ObscuredInt GetDEF(int nCurrentWeapon)
	{
		return EnemyData.n_DEF;
	}

	public override ObscuredInt GetReduceCriPercent(int nCurrentWeapon)
	{
		return EnemyData.n_CRI_RESIST;
	}

	public override ObscuredInt GetReduceCriDmgPercent(int nCurrentWeapon)
	{
		return EnemyData.n_CRIDMG_RESIST;
	}

	public override ObscuredInt GetBlock()
	{
		return EnemyData.n_PARRY;
	}

	public override ObscuredInt GetBlockDmgPercent()
	{
		return EnemyData.n_PARRY_DEF;
	}

	public ObscuredInt BaseHurt(HurtPassParam tHurtPassParam)
	{
		if (GuardTransform.Contains(tHurtPassParam.nSubPartID))
		{
			return Hp;
		}
		tHurtPassParam.dmg = selfBuffManager.ReduceDmgByEnergyShild(tHurtPassParam.dmg);
		OrangeBattleUtility.UpdateEnemyHp(ref Hp, ref tHurtPassParam.dmg);
		if (!InGame)
		{
			Debug.LogWarning("[Enemy] InGame Flag is false.");
			return Hp;
		}
		UpdateHurtAction();
		if ((int)Hp > 0)
		{
			base.IsHidden = false;
			if ((bool)_characterMaterial)
			{
				_characterMaterial.Hurt();
			}
		}
		else
		{
			base.IsHidden = true;
			DeadBehavior(ref tHurtPassParam);
			MonoBehaviourSingleton<LegionManager>.Instance.callVibrator();
		}
		return Hp;
	}

	protected virtual void DeadBehavior(ref HurtPassParam tHurtPassParam)
	{
		PlaySE(ExplodeSE[0], ExplodeSE[1]);
		Explosion();
		if (DeadCallback != null)
		{
			DeadCallback();
		}
		if (tHurtPassParam.wpnType != 0 && bDeadShock)
		{
			Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.CAMERA_SHAKE, 1f, false);
		}
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

	public override ObscuredInt Hurt(HurtPassParam tHurtPassParam)
	{
		return BaseHurt(tHurtPassParam);
	}

	protected virtual void EventPlayerSpawn(OrangeCharacter tPlayer)
	{
	}

	public virtual void OnTargetEnter(OrangeCharacter target)
	{
		Target = target;
	}

	public virtual void OnTargetExit(OrangeCharacter target)
	{
		Target = null;
	}

	protected void ShuffleArray(ref int[] ary)
	{
		for (int i = 0; i < ary.Length; i++)
		{
			int num = ary[i];
			int num2 = OrangeBattleUtility.Random(i, ary.Length);
			ary[i] = ary[num2];
			ary[num2] = num;
		}
	}

	public virtual void UpdateEnemyID(int id)
	{
		EnemyID = id;
		EnemyData = ManagedSingleton<OrangeDataManager>.Instance.MOB_TABLE_DICT[EnemyID];
		Hp = EnemyData.n_HP;
		MaxHp = EnemyData.n_HP;
		DmgHp = 0;
		HealHp = 0;
		GuardSE = ManagedSingleton<OrangeTableHelper>.Instance.ParseSE(EnemyData.s_BLOCK_SE);
		ExplodeSE = ManagedSingleton<OrangeTableHelper>.Instance.ParseSE(EnemyData.s_DIE_SE);
		SetAimRange(EnemyData.f_CHECK_RANGE, EnemyData.f_CHECK_RANGE, 0f, 0f);
		SKILL_TABLE[] enemyAllSkillData = ManagedSingleton<OrangeTableHelper>.Instance.GetEnemyAllSkillData(EnemyData);
		EquippedWeaponNum = enemyAllSkillData.Length;
		if (EnemyData.s_BORNFX != "null")
		{
			string[] array = EnemyData.s_BORNFX.Split(',');
			if (array.Length >= 4)
			{
				Bornfx_Name = array[0];
				Bornfx_XY_SCL = new Vector3(float.Parse(array[1]), float.Parse(array[2]), float.Parse(array[3]));
				MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(Bornfx_Name, 2);
			}
		}
		if (ManagedSingleton<OrangeTableHelper>.Instance.IsBoss(EnemyData))
		{
			DeadPlayCompleted = false;
		}
		if (EquippedWeaponNum > 0)
		{
			EnemyWeapons = new WeaponStruct[EquippedWeaponNum];
			BulletOrder = new int[EquippedWeaponNum];
			for (int i = 0; i < EquippedWeaponNum; i++)
			{
				EnemyWeapons[i] = new WeaponStruct();
				EnemyWeapons[i].BulletData = enemyAllSkillData[i];
				EnemyWeapons[i].MagazineRemain = enemyAllSkillData[i].n_MAGAZINE;
				EnemyWeapons[i].LastUseTimer = new UpdateTimer();
				BulletOrder[i] = i;
			}
			ShuffleArray(ref BulletOrder);
			tRefPassiveskill = new RefPassiveskill();
			tRefPassiveskill.AddPassivesSkill(EnemyData.n_INITIAL_SKILL1);
			tRefPassiveskill.AddPassivesSkill(EnemyData.n_INITIAL_SKILL2);
			tRefPassiveskill.AddPassivesSkill(EnemyData.n_INITIAL_SKILL3);
			selfBuffManager.Init(this);
		}
	}

	public void SetAimRange(float fSet, float fSetY, float fOffsetX, float fOffsetY)
	{
		SetAimRange(new Vector2(fOffsetX, fOffsetY), new Vector2(fSet, fSetY));
	}

	public void SetAimRange(Vector2 offset, Vector2 size)
	{
		if (size.x > 0f)
		{
			fAIWorkRange = size.x * size.x;
			if ((bool)_enemyAutoAimSystem)
			{
				_enemyAutoAimSystem.UpdateAimRange(offset, size);
			}
		}
	}

	public override void BackToPool()
	{
		IsStun = false;
		SetActive(false);
	}

	public virtual void SetChipInfoAnim()
	{
	}

	public virtual void SetActive(bool isActive)
	{
		DoSetActive(isActive);
	}

	protected void DoSetActive(bool isActive)
	{
		if (IsStun)
		{
			SetStun(false);
		}
		try
		{
			if (StageUpdate.gStageName == "stage04_1401_e1" || StageUpdate.gStageName == "stage04_3301_e1")
			{
				MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_mob_explode_000", 5);
			}
		}
		catch (Exception)
		{
		}
		InGame = isActive;
		Controller.enabled = isActive;
		SetColliderEnable(isActive);
		MonoBehaviourSingleton<FxManager>.Instance.UpdateFx(FxArray, isActive);
		if (isActive)
		{
			AiTimer.TimerStart();
			_transform.SetParent(null);
			Controller.LogicPosition = new VInt3(_transform.localPosition);
			_velocityExtra = VInt3.zero;
			MonoBehaviourSingleton<GameLogicUpdateManager>.Instance.AddUpdate(this);
			if (Bornfx_Name != null)
			{
				FxBase fxBase = MonoBehaviourSingleton<FxManager>.Instance.PlayReturn<FxBase>(Bornfx_Name, base.transform, Quaternion.identity, Array.Empty<object>());
				if ((bool)fxBase)
				{
					fxBase.transform.localScale = new Vector3(Bornfx_XY_SCL.z, Bornfx_XY_SCL.z, Bornfx_XY_SCL.z);
					fxBase.transform.localPosition = ((base._characterDirection == CharacterDirection.LEFT) ? new Vector3(Bornfx_XY_SCL.x, Bornfx_XY_SCL.y, 0f) : new Vector3(0f - Bornfx_XY_SCL.x, Bornfx_XY_SCL.y, 0f));
				}
			}
			if ((bool)_characterMaterial)
			{
				_characterMaterial.Appear(delegate
				{
					OnToggleCharacterMaterial(isActive);
				});
			}
			else
			{
				OnToggleCharacterMaterial(isActive);
			}
			if (base.gameObject.name.Contains("_bs"))
			{
				MonoBehaviourSingleton<AudioManager>.Instance.IsBossAppear = true;
			}
		}
		else
		{
			Hp = 0;
			UpdateHurtAction();
			AiTimer.TimerStop();
			MonoBehaviourSingleton<GameLogicUpdateManager>.Instance.RemoveUpdate(this);
			selfBuffManager.StopLoopSE();
			SoundSource.StopAll();
			if (base.gameObject.name.Contains("_bs"))
			{
				MonoBehaviourSingleton<AudioManager>.Instance.IsBossAppear = false;
			}
			if ((bool)_characterMaterial)
			{
				_characterMaterial.Disappear(delegate
				{
					OnToggleCharacterMaterial(true);
					if (!InGame)
					{
						MonoBehaviourSingleton<PoolManager>.Instance.BackToPool(this, EnemyData.s_MODEL);
					}
				});
			}
			else
			{
				OnToggleCharacterMaterial(false);
				MonoBehaviourSingleton<PoolManager>.Instance.BackToPool(this, EnemyData.s_MODEL);
			}
			if (KeepSpawnedMob)
			{
				SpawnedMobList.Clear();
			}
			else
			{
				DestroyAllSpawnedMob();
			}
		}
		Activate = ManagedSingleton<StageHelper>.Instance.bEnemyActive && isActive;
		if (!isActive)
		{
			bNeedDead = false;
		}
	}

	public virtual void OnToggleCharacterMaterial(bool appear)
	{
	}

	public virtual void OnlySwitchMaterial(bool bEnable)
	{
		if ((bool)_characterMaterial)
		{
			_characterMaterial.Appear(delegate
			{
				OnToggleCharacterMaterial(bEnable);
			});
		}
		else
		{
			OnToggleCharacterMaterial(bEnable);
		}
	}

	protected virtual void OnDisable()
	{
		MonoBehaviourSingleton<GameLogicUpdateManager>.Instance.RemoveUpdate(this);
	}

	protected void UpdateMagazine(int id = -1, bool forceUpdate = false)
	{
		for (int i = 0; i < EnemyWeapons.Length; i++)
		{
			if (forceUpdate || EnemyWeapons[i].LastUseTimer.GetMillisecond() >= EnemyWeapons[i].BulletData.n_RELOAD)
			{
				EnemyWeapons[i].MagazineRemain = EnemyWeapons[i].BulletData.n_MAGAZINE;
			}
		}
	}

	public void ExcludePlayer(int x, int y)
	{
		RaycastHit2D raycastHit2D = Controller.ObjectMeeting(x, y, BulletScriptableObject.Instance.BulletLayerMaskPlayer);
		if (!raycastHit2D)
		{
			return;
		}
		OrangeCharacter component = raycastHit2D.transform.GetComponent<OrangeCharacter>();
		if (component != null)
		{
			float num;
			if (component.transform.position.x > Controller.transform.position.x)
			{
				num = Controller._raycastOrigins.bottomRight.x - component.Controller._raycastOrigins.bottomLeft.x;
				Debug.LogWarning("Right  " + num);
			}
			else
			{
				num = Controller._raycastOrigins.bottomLeft.x - component.Controller._raycastOrigins.bottomRight.x;
				Debug.LogWarning("Left  " + num);
			}
			component.AddShift(new VInt3(Mathf.RoundToInt(num * 1000f), 0, 0));
			return;
		}
		RideArmorController component2 = raycastHit2D.transform.GetComponent<RideArmorController>();
		if ((bool)component2)
		{
			float num2;
			if (component2.transform.position.x > Controller.transform.position.x)
			{
				num2 = Controller._raycastOrigins.bottomRight.x - component2.Controller._raycastOrigins.bottomLeft.x;
				Debug.LogWarning("Right  " + num2);
			}
			else
			{
				num2 = Controller._raycastOrigins.bottomLeft.x - component2.Controller._raycastOrigins.bottomRight.x;
				Debug.LogWarning("Left  " + num2);
			}
			component2.AddShift(new VInt3(Mathf.RoundToInt(num2 * 1000f), 0, 0));
		}
	}

	public void ExcludeEnemy(int x, int y)
	{
		RaycastHit2D raycastHit2D = Controller.ObjectMeeting(x, y, BulletScriptableObject.Instance.BulletLayerMaskEnemy);
		if ((bool)raycastHit2D)
		{
			EnemyControllerBase componentInParent = raycastHit2D.transform.GetComponentInParent<EnemyControllerBase>();
			componentInParent.Hp = 0;
			componentInParent.Hurt(new HurtPassParam());
		}
	}

	public void AddForce(VInt3 pForce)
	{
		_velocityExtra += pForce;
	}

	public void AddShift(VInt3 pForce)
	{
		_velocityShift += pForce;
	}

	public void SetColliderEnable(bool enable)
	{
		EnemyCollider[] enemyCollider = _enemyCollider;
		for (int i = 0; i < enemyCollider.Length; i++)
		{
			enemyCollider[i].SetColliderEnable(enable);
		}
	}

	public virtual void SetPositionAndRotation(Vector3 pos, bool bBack)
	{
		if (bBack)
		{
			base._characterDirection = CharacterDirection.LEFT;
			base.transform.SetPositionAndRotation(pos, new Quaternion(0f, 180f, 0f, 0f));
			Controller.LogicPosition = new VInt3(_transform.position);
		}
		else
		{
			base._characterDirection = CharacterDirection.RIGHT;
			base.transform.SetPositionAndRotation(pos, Quaternion.identity);
			Controller.LogicPosition = new VInt3(_transform.position);
		}
	}

	public void SetMaxGravity(VInt value)
	{
		_maxGravity = value;
	}

	public void SetVelocity(VInt3 value)
	{
		_velocity = value;
	}

	protected virtual bool CheckMoveFall(VInt3 velocity)
	{
		if (!DisableMoveFall)
		{
			return false;
		}
		float y = Controller.GetBounds().size.y;
		VInt3 vInt = velocity * GameLogicUpdateManager.m_fFrameLen;
		float num = Mathf.Abs(vInt.vec3.x);
		int num2 = Math.Sign(vInt.vec3.x);
		Controller2D.RaycastOrigins raycastOrigins = Controller.GetRaycastOrigins();
		Vector2 vector = ((num2 == -1) ? raycastOrigins.topLeft : raycastOrigins.topRight);
		if ((bool)OrangeBattleUtility.RaycastIgnoreSelf(vector, Vector2.right * num2, num, (int)Controller.collisionMask | (int)Controller.collisionMaskThrough, _transform))
		{
			return false;
		}
		Vector2 vector2 = vector + Vector2.right * num2 * num;
		RaycastHit2D raycastHit2D = OrangeBattleUtility.RaycastIgnoreSelf(vector2, Vector2.down, y + 0.3f, (int)Controller.collisionMask | (int)Controller.collisionMaskThrough, _transform);
		Debug.DrawLine(vector, vector2, Color.cyan, 0.5f);
		if ((bool)raycastHit2D)
		{
			Debug.DrawLine(vector2, raycastHit2D.point, Color.cyan, 0.5f);
			return false;
		}
		Debug.DrawLine(vector2, vector2 + Vector2.down * (y + 0.3f), Color.cyan, 0.5f);
		return true;
	}

	public IEnumerator HitStop()
	{
		if ((int)Hp > 0)
		{
			HSTimer.TimerReset();
			HSTimer.TimerStart();
			bool[] AnimatorStatuses = new bool[Animators.Length];
			for (int i = 0; i < Animators.Length; i++)
			{
				AnimatorStatuses[i] = Animators[i].enabled;
				Animators[i].enabled = false;
			}
			while ((float)HSTimer.GetMillisecond() < OrangeBattleUtility.HitStopTime / 2f && (int)Hp > 0)
			{
				yield return CoroutineDefine._waitForEndOfFrame;
			}
			for (int j = 0; j < Animators.Length; j++)
			{
				Animators[j].enabled = AnimatorStatuses[j];
			}
		}
	}

	protected IEnumerator BossDieExplosion(Vector3 pos, bool updatePos = false, Transform parent = null)
	{
		string fx = "fxstory_explode_000";
		MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(fx, GetRandomPos(pos), Quaternion.identity, Array.Empty<object>());
		yield return StageUpdate.WaitGamePauseProcessTime(0.2f);
		MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(fx, GetRandomPos(pos), Quaternion.identity, Array.Empty<object>());
		yield return StageUpdate.WaitGamePauseProcessTime(0.2f);
		MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(fx, GetRandomPos(pos), Quaternion.identity, Array.Empty<object>());
		yield return StageUpdate.WaitGamePauseProcessTime(0.2f);
		if (updatePos)
		{
			pos = parent.position;
		}
		Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.CAMERA_SHAKE, 1f, false);
		MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(fx, GetRandomPos(pos), Quaternion.identity, Array.Empty<object>());
		MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(fx, GetRandomPos(pos), Quaternion.identity, Array.Empty<object>());
		yield return StageUpdate.WaitGamePauseProcessTime(0.2f);
		Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.CAMERA_SHAKE, 1f, false);
		MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(fx, GetRandomPos(pos), Quaternion.identity, Array.Empty<object>());
		yield return StageUpdate.WaitGamePauseProcessTime(0.1f);
		Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.CAMERA_SHAKE, 1f, false);
		MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(fx, GetRandomPos(pos), Quaternion.identity, Array.Empty<object>());
		yield return StageUpdate.WaitGamePauseProcessTime(0.1f);
		if (updatePos)
		{
			pos = parent.position;
		}
		Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.CAMERA_SHAKE, 2f, false);
		MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(fx, GetRandomPos(pos, 10f, 10f), Quaternion.identity, Array.Empty<object>());
		yield return StageUpdate.WaitGamePauseProcessTime(0.1f);
		Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.CAMERA_SHAKE, 2f, false);
		MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(fx, GetRandomPos(pos, 10f, 10f), Quaternion.identity, Array.Empty<object>());
		yield return StageUpdate.WaitGamePauseProcessTime(0.2f);
		Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.CAMERA_SHAKE, 2f, false);
		MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(fx, GetRandomPos(pos, 10f, 10f), Quaternion.identity, Array.Empty<object>());
		yield return StageUpdate.WaitGamePauseProcessTime(0.3f);
		Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.CAMERA_SHAKE, 2f, false);
		MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(fx, GetRandomPos(pos, 10f, 10f), Quaternion.identity, Array.Empty<object>());
		MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(fx, GetRandomPos(pos, 10f, 10f), Quaternion.identity, Array.Empty<object>());
		yield return StageUpdate.WaitGamePauseProcessTime(0.1f);
		Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.CAMERA_SHAKE, 2f, false);
		MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(fx, GetRandomPos(pos, 10f, 10f), Quaternion.identity, Array.Empty<object>());
		yield return StageUpdate.WaitGamePauseProcessTime(0.2f);
		if (updatePos)
		{
			pos = parent.position;
		}
		int j = 0;
		while (j < 16)
		{
			int num = OrangeBattleUtility.Random(0, 3);
			for (int k = 0; k < num; k++)
			{
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(fx, GetRandomPos(pos, 15f, 10f), Quaternion.identity, Array.Empty<object>());
			}
			if (num > 0)
			{
				Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.CAMERA_SHAKE, 2f, false);
			}
			j++;
			yield return StageUpdate.WaitGamePauseProcessTime(0.1f);
		}
		j = 0;
		while (j < 5)
		{
			Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.CAMERA_SHAKE, 2f, false);
			j++;
			yield return StageUpdate.WaitGamePauseProcessTime(0.2f);
		}
	}

	protected IEnumerator BossDieFlow(Vector3 TargetTF, string Effect = "FX_BOSS_EXPLODE2", bool showWinPose = true, bool needStageEnd = true)
	{
		if (MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.Setting.scrEffect == 2)
		{
			StartCoroutine(BossDieExplosion(TargetTF));
			yield return StageUpdate.WaitGamePauseProcessTime(1.5f);
		}
		else
		{
			StartCoroutine(MBossExplosionSE());
		}
		PlaySE("HitSE", 104);
		MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(Effect, TargetTF, Quaternion.identity, Array.Empty<object>());
		BattleInfoUI.Instance.ShowExplodeBG(base.gameObject, showWinPose, needStageEnd);
	}

	protected IEnumerator BossDieFlow(Transform transForm, string Effect = "FX_BOSS_EXPLODE2", bool showWinPose = true, bool needStageEnd = true)
	{
		if (MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.Setting.scrEffect == 2)
		{
			MonoBehaviourSingleton<LegionManager>.Instance.callVibrator(3000);
			if (base.AimTransform == null)
			{
				StartCoroutine(BossDieExplosion(_transform.position, true, _transform));
			}
			else
			{
				StartCoroutine(BossDieExplosion(base.AimTransform.position, true, base.AimTransform));
			}
			yield return StageUpdate.WaitGamePauseProcessTime(1.5f);
		}
		else
		{
			MonoBehaviourSingleton<LegionManager>.Instance.callVibrator(3000);
			StartCoroutine(MBossExplosionSE());
		}
		PlaySE("HitSE", 104);
		MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(Effect, transForm, Quaternion.identity, Array.Empty<object>());
		BattleInfoUI.Instance.ShowExplodeBG(base.gameObject, showWinPose, needStageEnd);
	}

	protected IEnumerator MBossExplosionSE()
	{
		int i = 0;
		while (i < 7)
		{
			switch (OrangeBattleUtility.Random(0, 2))
			{
			case 0:
				PlaySE("HitSE", 101);
				yield return new WaitForSeconds(0.3f);
				break;
			case 1:
				PlaySE("HitSE", 101);
				yield return new WaitForSeconds(0.2f);
				break;
			}
			i++;
			PlaySE("HitSE", 101);
			yield return StageUpdate.WaitGamePauseProcessTime(0.2f);
		}
	}

	private Vector3 GetRandomPos(Vector3 oriPos, float MaxRangeX = 5f, float MaxRangeY = 3f)
	{
		float num = MaxRangeX / 2f;
		float num2 = MaxRangeY / 2f;
		return new Vector3(oriPos.x + (OrangeBattleUtility.Random(0f, MaxRangeX) - num), oriPos.y + (OrangeBattleUtility.Random(0f, MaxRangeY) - num2), oriPos.z);
	}

	public void PlaySE(EnemySE cueid, bool ForceTrigger = false)
	{
		PlaySE("EnemySE", (int)cueid, ForceTrigger);
	}

	public void PlaySE(EnemySE02 cueid, bool ForceTrigger = false)
	{
		PlaySE("EnemySE02", (int)cueid, ForceTrigger);
	}

	public void PlaySE(EnemySE03 cueid, bool ForceTrigger = false)
	{
		PlaySE("EnemySE03", (int)cueid, ForceTrigger);
	}

	public void PlaySE(string s_acb, int n_cueId, bool ForceTrigger = false, bool UseDisCheck = true)
	{
		if (!MonoBehaviourSingleton<UIManager>.Instance.IsLoading)
		{
			if (ForceTrigger)
			{
				SoundSource.ForcePlaySE(s_acb, n_cueId);
			}
			else
			{
				SoundSource.PlaySE(s_acb, n_cueId);
			}
		}
	}

	public override void PlaySE(string s_acb, string cueName, float delay = 0f, bool ForceTrigger = false, bool UseDisCheck = true)
	{
		if (!MonoBehaviourSingleton<UIManager>.Instance.IsLoading)
		{
			if (ForceTrigger)
			{
				SoundSource.ForcePlaySE(s_acb, cueName);
			}
			else
			{
				SoundSource.PlaySE(s_acb, cueName, delay);
			}
		}
	}

	public void PlaySE(string s_acb, string cueName, bool ForceTrigger = false, bool UseDisCheck = true)
	{
		if (!MonoBehaviourSingleton<UIManager>.Instance.IsLoading)
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
	}

	public void PlayBossSE(string s_acb, int n_cueId)
	{
		SoundSource.PlaySE(s_acb, n_cueId);
	}

	public void PlayBossSE(string s_acb, string cueName)
	{
		SoundSource.ForcePlaySE(s_acb, cueName);
	}

	protected void PlayTurnSE(string category, int se)
	{
		if (base.direction != preDirection && base.direction != 0)
		{
			PlayBossSE(category, se);
			preDirection = base.direction;
		}
	}

	public void PlayBossSE(string cue, float delay = 0f)
	{
		SoundSource.PlaySE("BossSE", cue, delay);
	}

	public void PlayBossSE03(string cue, float delay = 0f)
	{
		SoundSource.PlaySE("BossSE03", cue, delay);
	}

	public void PlayBossSE06(string cue, float delay = 0f)
	{
		SoundSource.PlaySE("BossSE06", cue, delay);
	}

	private void OnBecameVisible()
	{
		visible = true;
	}

	private void OnBecameInvisible()
	{
		visible = false;
	}

	public override Vector3 GetTargetPoint()
	{
		return base.AimTransform.position + base.AimPoint;
	}

	public virtual void SetPatrolPath(bool isLoop, int nMoveSpeed, Vector3[] paths)
	{
	}

	public virtual void SetEventCtrlID(int eventid)
	{
	}

	public virtual void SetFloatParameter(float param)
	{
	}

	public virtual void SetSummonEventID(int nSummonEventId)
	{
	}

	public virtual void SetStageCustomParams(int nStageCustomType, int[] nStageCustomParams)
	{
	}

	public virtual bool IsImmunityDeadArea()
	{
		return _bImmunityDeadArea;
	}

	public override Vector2 GetDamageTextPos()
	{
		if (!base.AimTransform)
		{
			return base.transform.position.xy();
		}
		return base.AimTransform.position.xy();
	}

	public virtual bool CheckHost()
	{
		if (StageUpdate.bIsHost)
		{
			if (bWaitNetStatus)
			{
				return false;
			}
			return true;
		}
		bWaitNetStatus = false;
		return false;
	}

	protected virtual void UploadEnemyStatus(int status = -1, bool SetHp = false, object[] Param = null, object[] SParam = null)
	{
		UploadEnemyStatus(status, false, SetHp, Param, SParam);
	}

	protected virtual void UploadEnemyStatus(int status, bool chkhost, bool SetHp = false, object[] Param = null, object[] SParam = null)
	{
		if (status != -1)
		{
			NetSyncData netSyncData = new NetSyncData();
			if (Target == null && _enemyAutoAimSystem != null)
			{
				Target = _enemyAutoAimSystem.GetClosetPlayer();
			}
			if ((bool)Target)
			{
				TargetPos = new VInt3(Target.transform.position);
				netSyncData.TargetPosX = TargetPos.x;
				netSyncData.TargetPosY = TargetPos.y;
				netSyncData.TargetPosZ = TargetPos.z;
			}
			netSyncData.SelfPosX = Controller.LogicPosition.x;
			netSyncData.SelfPosY = Controller.LogicPosition.y;
			netSyncData.SelfPosZ = Controller.LogicPosition.z;
			if (SetHp)
			{
				netSyncData.bSetHP = true;
				netSyncData.nHP = Hp;
			}
			if (Param != null)
			{
				netSyncData.nParam0 = (int)Param[0];
			}
			if (SParam != null)
			{
				netSyncData.sParam0 = SParam[0].ToString();
			}
			bWaitNetStatus = true;
			StageUpdate.RegisterSendAndRun(sNetSerialID, status, JsonConvert.SerializeObject(netSyncData), chkhost);
		}
		else
		{
			Debug.LogError("MainStatus值為空");
		}
	}

	protected virtual VInt3 CalculateJumpVelocity(int logicX, int logicY, int minJumpSpeed = 16)
	{
		VInt3 zero = VInt3.zero;
		float num = logicY * 100 / Mathf.Abs(OrangeBattleUtility.FP_Gravity.i);
		if (num < (float)minJumpSpeed)
		{
			num = minJumpSpeed;
		}
		zero.x = (int)((float)(logicX * Mathf.Abs(OrangeBattleUtility.FP_Gravity.i)) / (num * 2f * 1000f));
		zero.x *= base.direction;
		zero.y = Mathf.RoundToInt(num * 1000f);
		return zero;
	}

	protected virtual void UpdateAIState()
	{
		AI_STATE aiState = AI_STATE.mob_001;
		if (EnemyData.s_AI != "null")
		{
			aiState = (AI_STATE)Enum.Parse(typeof(AI_STATE), EnemyData.s_AI);
		}
		AiState = aiState;
	}
}
