using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicBullet : BulletBase, IManagedLateUpdateBehavior
{
	public enum BulletPhase
	{
		Normal = 0,
		Splash = 1,
		Boomerang = 2,
		Result = 3,
		BackToPool = 4
	}

	public enum TrackPriority
	{
		EnemyFirst = 0,
		PlayerFirst = 1,
		NearFirst = 2
	}

	[HideInInspector]
	public BulletPhase Phase = BulletPhase.BackToPool;

	public float DefaultRadiusX = 0.25f;

	public float DefaultRadiusY = 0.25f;

	public CapsuleDirection2D ColliderDirection = CapsuleDirection2D.Horizontal;

	protected HashSet<Transform> _hitList;

	protected CapsuleCollider2D _capsuleCollider;

	public float FreeDISTANCE;

	protected float offset;

	protected Vector3 oldPos;

	protected Vector3 beginPosition;

	protected Vector2 _colliderOffset;

	protected Vector2 _colliderSize;

	protected Rigidbody2D _rigidbody2D;

	protected float _splashWaitLogicFrame;

	protected BulletExtraCollision ExtraCollider;

	private SineEffect _sineEffect;

	public float StartTime;

	public float amplitude = 0.3f;

	public float omega = 30f;

	public float shootTime;

	public Dictionary<Transform, int> HitCount = new Dictionary<Transform, int>();

	private List<Transform> returingList = new List<Transform>();

	protected float heapDistance;

	protected float lineDistance;

	protected Vector2 lastPosition = new Vector2(0f, 0f);

	protected float lastWaveDistance;

	public bool Position_Scale;

	public float Max_Position = 3f;

	public Transform Scale_Obj;

	protected TrackPriority trackPriority;

	public Vector3 From = Vector3.right;

	protected Transform lastHit;

	protected bool hasReflect;

	protected Vector2 reflectPoint = Vector2.zero;

	protected Quaternion reflectRotation = Quaternion.identity;

	protected Transform lastReflectTrans;

	protected override void Awake()
	{
		base.Awake();
		_sineEffect = GetComponentInChildren<SineEffect>();
		Phase = BulletPhase.Normal;
		_rigidbody2D = base.gameObject.AddOrGetComponent<Rigidbody2D>();
		_rigidbody2D.bodyType = RigidbodyType2D.Kinematic;
		_rigidbody2D.useFullKinematicContacts = true;
		_capsuleCollider = base.gameObject.AddOrGetComponent<CapsuleCollider2D>();
		_capsuleCollider.isTrigger = true;
		if (ManagedSingleton<ServerStatusHelper>.Instance.FixBulletCollider)
		{
			_capsuleCollider.direction = ColliderDirection;
		}
		else
		{
			_capsuleCollider.direction = CapsuleDirection2D.Horizontal;
		}
		_colliderSize = new Vector2(DefaultRadiusX * 2f, DefaultRadiusY * 2f);
		_colliderOffset = Vector2.zero;
		_hitList = new HashSet<Transform>();
		_capsuleCollider.enabled = false;
	}

	public virtual void OnDisable()
	{
		MonoBehaviourSingleton<UpdateManager>.Instance.RemoveLateUpdate(this);
	}

	protected virtual void OnTriggerEnter2D(Collider2D col)
	{
		OnTriggerHit(col);
	}

	protected virtual void OnTriggerStay2D(Collider2D col)
	{
		OnTriggerHit(col);
	}

	public virtual void OnTriggerHit(Collider2D col)
	{
		if (!ManagedSingleton<StageHelper>.Instance.bEnemyActive || MonoBehaviourSingleton<OrangeGameManager>.Instance.IsGamePause || col.isTrigger || ((1 << col.gameObject.layer) & (int)UseMask) == 0 || (((uint)BulletData.n_FLAG & (true ? 1u : 0u)) != 0 && ((1 << col.gameObject.layer) & (int)BlockMask) != 0 && !col.GetComponent<StageHurtObj>()))
		{
			return;
		}
		StageObjParam component = col.GetComponent<StageObjParam>();
		if (component != null && component.tLinkSOB != null)
		{
			if (((1 << col.gameObject.layer) & (int)BlockMask) == 0 && (int)component.tLinkSOB.Hp > 0)
			{
				Hit(col);
			}
		}
		else if ((BulletData.n_FLAG & 0x1000) == 0 || ((1 << col.gameObject.layer) & (int)BlockMask) == 0 || !col.GetComponent<StageHurtObj>())
		{
			Hit(col);
		}
	}

	public virtual void SubBullet()
	{
		BasicBullet basicBullet = null;
		PoolManager instance = MonoBehaviourSingleton<PoolManager>.Instance;
		float num = BulletData.f_ANGLE / (float)(BulletData.n_NUM_SHOOT - 1);
		float num2 = (float)((BulletData.n_NUM_SHOOT - 1) / 2) * num;
		for (int i = 0; i < BulletData.n_NUM_SHOOT; i++)
		{
			Vector3 direction = Direction;
			switch (BulletData.n_SHOTLINE)
			{
			default:
			{
				float num3 = num2 - (float)i * num;
				if (num3 == 0f)
				{
					continue;
				}
				direction = Quaternion.Euler(0f, 0f, num3) * Direction;
				break;
			}
			case 11:
				if (i == 0)
				{
					continue;
				}
				direction = CaluShootLine11ShootDirection(i);
				break;
			case 14:
				if (i == 0)
				{
					continue;
				}
				direction = CaluShootLine14ShootDirection(i);
				break;
			}
			basicBullet = instance.GetPoolObj<BasicBullet>(BulletData.s_MODEL);
			basicBullet.UpdateBulletData(BulletData, Owner);
			basicBullet.BulletLevel = BulletLevel;
			basicBullet.nHp = nHp;
			basicBullet.nAtk = nAtk;
			basicBullet.nCri = nCri;
			basicBullet.nHit = nHit;
			basicBullet.nWeaponCheck = nWeaponCheck;
			basicBullet.nWeaponType = nWeaponType;
			basicBullet.nCriDmgPercent = nCriDmgPercent;
			basicBullet.nReduceBlockPercent = nReduceBlockPercent;
			basicBullet.fDmgFactor = fDmgFactor;
			basicBullet.fCriFactor = fCriFactor;
			basicBullet.fCriDmgFactor = fCriDmgFactor;
			basicBullet.fMissFactor = fMissFactor;
			basicBullet.refPBMShoter = refPBMShoter;
			basicBullet.refPSShoter = refPSShoter;
			basicBullet.nRecordID = nRecordID;
			basicBullet.nNetID = nNetID + 100 + i;
			Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_BULLET_REGISTER, basicBullet);
			basicBullet.isMuteSE = isMuteSE;
			basicBullet.isSubBullet = true;
			basicBullet.isPetBullet = isPetBullet;
			basicBullet.HitCount = HitCount;
			if ((bool)MasterTransform)
			{
				basicBullet.Active(MasterTransform, direction, TargetMask, Target);
			}
			else
			{
				basicBullet.Active(MasterPosition, direction, TargetMask, Target);
			}
		}
	}

	protected Vector3 CaluShootLine11ShootDirection(int index)
	{
		float z = 0f;
		if (BulletData.n_NUM_SHOOT > 1)
		{
			float num = BulletData.f_ANGLE / (float)(BulletData.n_NUM_SHOOT - 1);
			z = BulletData.f_ANGLE / 2f - (float)index * num;
		}
		return Quaternion.Euler(0f, 0f, z) * Direction;
	}

	protected Vector3 CaluShootLine14ShootDirection(int index)
	{
		float z = 0f;
		if (BulletData.n_NUM_SHOOT > 1)
		{
			z = BulletData.f_ANGLE / (float)BulletData.n_NUM_SHOOT * (float)index;
		}
		return Quaternion.Euler(0f, 0f, z) * Direction;
	}

	protected override IEnumerator OnStartMove()
	{
		yield return null;
	}

	public void CreateSubBullet()
	{
		if (BulletData.n_LINK_SKILL != 0 && BulletData.n_SHOTLINE == 5)
		{
			SKILL_TABLE sKILL_TABLE = ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT[BulletData.n_LINK_SKILL];
			if (sKILL_TABLE.n_TYPE == 1)
			{
				BasicBullet poolObj = MonoBehaviourSingleton<PoolManager>.Instance.GetPoolObj<BasicBullet>(sKILL_TABLE.s_MODEL);
				poolObj.UpdateBulletData(sKILL_TABLE, Owner);
				poolObj.BulletLevel = BulletLevel;
				poolObj.SetBulletAtk(new WeaponStatus
				{
					nHP = nHp,
					nATK = nOriginalATK,
					nCRI = nOriginalCRI,
					nHIT = nHit - refPSShoter.GetAddStatus(8, nWeaponCheck),
					nCriDmgPercent = nCriDmgPercent,
					nReduceBlockPercent = nReduceBlockPercent,
					nWeaponCheck = nWeaponCheck,
					nWeaponType = nWeaponType
				}, new PerBuffManager.BuffStatus
				{
					fAtkDmgPercent = fDmgFactor - 100f,
					fCriPercent = fCriFactor - 100f,
					fCriDmgPercent = fCriDmgFactor - 100f,
					fMissPercent = 0f,
					refPBM = refPBMShoter,
					refPS = refPSShoter
				});
				poolObj.Active(base.transform, Direction, TargetMask);
			}
		}
	}

	public void WaveSub_Bullet(float om, bool isSub)
	{
		BasicBullet poolObj = MonoBehaviourSingleton<PoolManager>.Instance.GetPoolObj<BasicBullet>(BulletData.s_MODEL);
		poolObj.UpdateBulletData(BulletData, Owner);
		poolObj.BulletLevel = BulletLevel;
		poolObj.nHp = nHp;
		poolObj.nAtk = nAtk;
		poolObj.nCri = nCri;
		poolObj.nHit = nHit;
		poolObj.nWeaponCheck = nWeaponCheck;
		poolObj.nWeaponType = nWeaponType;
		poolObj.nCriDmgPercent = nCriDmgPercent;
		poolObj.nReduceBlockPercent = nReduceBlockPercent;
		poolObj.fDmgFactor = fDmgFactor;
		poolObj.fCriFactor = fCriFactor;
		poolObj.fCriDmgFactor = fCriDmgFactor;
		poolObj.fMissFactor = fMissFactor;
		poolObj.refPBMShoter = refPBMShoter;
		poolObj.refPSShoter = refPSShoter;
		poolObj.nRecordID = nRecordID;
		poolObj.nNetID = nNetID + 100;
		poolObj.omega = om;
		poolObj.isSubBullet = isSub;
		poolObj.HitCount = HitCount;
		poolObj.isPetBullet = isPetBullet;
		Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_BULLET_REGISTER, poolObj);
		poolObj.Active(MasterTransform, Direction, TargetMask);
	}

	public void DuplicateBullet(Vector3 shootDirection)
	{
		BasicBullet poolObj = MonoBehaviourSingleton<PoolManager>.Instance.GetPoolObj<BasicBullet>(BulletData.s_MODEL);
		poolObj.UpdateBulletData(BulletData, Owner);
		poolObj.BulletLevel = BulletLevel;
		poolObj.nHp = nHp;
		poolObj.nAtk = nAtk;
		poolObj.nCri = nCri;
		poolObj.nHit = nHit;
		poolObj.nWeaponCheck = nWeaponCheck;
		poolObj.nWeaponType = nWeaponType;
		poolObj.nCriDmgPercent = nCriDmgPercent;
		poolObj.nReduceBlockPercent = nReduceBlockPercent;
		poolObj.fDmgFactor = fDmgFactor;
		poolObj.fCriFactor = fCriFactor;
		poolObj.fCriDmgFactor = fCriDmgFactor;
		poolObj.fMissFactor = fMissFactor;
		poolObj.refPBMShoter = refPBMShoter;
		poolObj.refPSShoter = refPSShoter;
		poolObj.nRecordID = nRecordID;
		poolObj.nNetID = nNetID + 100;
		poolObj.isSubBullet = true;
		poolObj.HitCount = HitCount;
		poolObj.isPetBullet = isPetBullet;
		poolObj.Active(MasterTransform, shootDirection, TargetMask, Target);
	}

	private IEnumerator First_Sub_Skill(float mg)
	{
		for (int ii = 0; ii < 5; ii++)
		{
			WaveSub_Bullet(mg, true);
			while (MonoBehaviourSingleton<OrangeGameManager>.Instance.IsGamePause)
			{
				yield return CoroutineDefine._waitForEndOfFrame;
			}
			yield return new WaitForSeconds(0.04f);
		}
	}

	private IEnumerator First_Sub_SkillM()
	{
		StartCoroutine(First_Sub_Skill(30f));
		while (MonoBehaviourSingleton<OrangeGameManager>.Instance.IsGamePause)
		{
			yield return CoroutineDefine._waitForEndOfFrame;
		}
		yield return new WaitForSeconds(0.1f);
		StartCoroutine(First_Sub_Skill(-30f));
		while (MonoBehaviourSingleton<OrangeGameManager>.Instance.IsGamePause)
		{
			yield return CoroutineDefine._waitForEndOfFrame;
		}
		yield return new WaitForSeconds(0.1f);
		StartCoroutine(First_Sub_Skill(30f));
		while (MonoBehaviourSingleton<OrangeGameManager>.Instance.IsGamePause)
		{
			yield return CoroutineDefine._waitForEndOfFrame;
		}
		yield return new WaitForSeconds(0.1f);
	}

	private IEnumerator ShootingAway(int nums, float time, bool lockTarget)
	{
		StageObjBase myTarget = null;
		if (lockTarget && Target != null)
		{
			myTarget = Target as StageObjBase;
		}
		time = Mathf.Max(time, 0.01f);
		for (int i = 0; i < nums; i++)
		{
			while (MonoBehaviourSingleton<OrangeGameManager>.Instance.IsGamePause)
			{
				yield return CoroutineDefine._waitForEndOfFrame;
			}
			yield return new WaitForSeconds(time);
			Vector3 shootDirection = Direction;
			if ((bool)myTarget && (int)myTarget.Hp > 0)
			{
				shootDirection = myTarget.AimPosition - MasterTransform.position;
			}
			DuplicateBullet(shootDirection);
		}
	}

	public override void OnStart()
	{
		base.OnStart();
		MonoBehaviourSingleton<UpdateManager>.Instance.AddLateUpdate(this);
		StartTime = Time.timeSinceLevelLoad;
		if ((bool)_sineEffect)
		{
			_sineEffect.StartTime = StartTime;
		}
		if (BulletData.n_ROLLBACK == 2 && bulletFxArray.Length != 0)
		{
			bulletFxArray[0].transform.eulerAngles = new Vector3(0f, 0f, 0f);
		}
		if (BulletData.n_NUM_SHOOT > 1 && !isSubBullet)
		{
			isMuteSE = true;
			switch (BulletData.n_SHOTLINE)
			{
			case 0:
			case 8:
				HitCount.Clear();
				SubBullet();
				break;
			case 11:
			{
				HitCount.Clear();
				SubBullet();
				Vector3 vector2 = CaluShootLine11ShootDirection(0);
				_transform.eulerAngles = new Vector3(0f, 0f, Vector2.SignedAngle(Vector2.right, vector2));
				Direction = vector2;
				break;
			}
			case 14:
			{
				HitCount.Clear();
				SubBullet();
				Vector3 vector = CaluShootLine14ShootDirection(0);
				_transform.eulerAngles = new Vector3(0f, 0f, Vector2.SignedAngle(Vector2.right, vector));
				Direction = vector;
				break;
			}
			}
			isMuteSE = false;
		}
		if (BulletData.n_SHOTLINE == 3)
		{
			amplitude = 0.9f;
			if (!isSubBullet)
			{
				HitCount.Clear();
				StartCoroutine(First_Sub_SkillM());
			}
			shootTime = Time.fixedTime;
		}
		else if (BulletData.n_SHOTLINE == 4)
		{
			if (!isSubBullet)
			{
				omega = 30f;
				HitCount.Clear();
				WaveSub_Bullet(0f - omega, true);
			}
			shootTime = Time.fixedTime;
		}
		else if (BulletData.n_SHOTLINE == 6)
		{
			amplitude = 3.8f;
			if (!isSubBullet)
			{
				omega = 8f;
				HitCount.Clear();
				WaveSub_Bullet(0f - omega, true);
			}
			shootTime = Time.fixedTime;
			if (!ManagedSingleton<ServerStatusHelper>.Instance.FixBulletCollider)
			{
				_capsuleCollider.direction = CapsuleDirection2D.Vertical;
			}
		}
		else if (!isSubBullet && BulletData.n_SHOTLINE == 7)
		{
			string s_CONTI = BulletData.s_CONTI;
			int result = 1;
			float result2 = 0f;
			if (ManagedSingleton<OrangeTableHelper>.Instance.IsNullOrEmpty(s_CONTI))
			{
				result = 3;
				result2 = 0.04f;
			}
			else
			{
				string[] array = s_CONTI.Split(',');
				float.TryParse(array[0], out result2);
				int.TryParse(array[1], out result);
				result--;
			}
			HitCount.Clear();
			StartCoroutine(ShootingAway(result, result2, true));
		}
		_capsuleCollider.enabled = true;
		_capsuleCollider.size = new Vector2(DefaultRadiusX * 2f, DefaultRadiusY * 2f);
		if ((int)TargetMask == (int)ManagedSingleton<OrangeLayerManager>.Instance.PlayerUseMask)
		{
			if (ExtraCollider == null)
			{
				OrangeBattleUtility.AddBulletExtraCollision(this, out ExtraCollider);
			}
			ExtraCollider.AliveFrame = GameLogicUpdateManager.GameFrame + 1;
		}
		if (BulletData.n_THROUGH > 0)
		{
			nThrough = BulletData.n_THROUGH / 100;
		}
		else
		{
			nThrough = 0;
		}
		beginPosition = _transform.position;
		reflectPoint = beginPosition;
		lastPosition = beginPosition;
		heapDistance = 0f;
		lineDistance = 0f;
		lastWaveDistance = 0f;
		if ((bool)MasterTransform && !string.IsNullOrEmpty(FxMuzzleFlare) && !isSubBullet)
		{
			if (BulletData.n_USE_FX_FOLLOW == 0)
			{
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FxMuzzleFlare, MasterTransform.position, _transform.rotation * BulletQuaternion, Array.Empty<object>());
			}
			else
			{
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FxMuzzleFlare, MasterTransform, BulletQuaternion, Array.Empty<object>());
			}
		}
	}

	private void Set_Scale_Effect(float curren_Distance, float max_Distance, bool isback)
	{
		if (Position_Scale && BulletData.n_ROLLBACK > 0 && Scale_Obj != null)
		{
			float num = (Max_Position - 1f) * (curren_Distance / max_Distance);
			Scale_Obj.localScale = new Vector3(num + 1f, num + 1f, num + 1f);
			if (_capsuleCollider != null && num > 1f)
			{
				_capsuleCollider.size = new Vector2(DefaultRadiusX * 2f * num, DefaultRadiusY * 2f * num);
			}
		}
	}

	public virtual void LateUpdateFunc()
	{
		if (ManagedSingleton<StageHelper>.Instance.bEnemyActive)
		{
			switch (Phase)
			{
			case BulletPhase.Normal:
				PhaseNormal();
				TrackingTarget();
				break;
			case BulletPhase.Boomerang:
				PhaseBoomerang();
				TrackingTarget();
				break;
			case BulletPhase.Splash:
				PhaseSplash();
				break;
			case BulletPhase.Result:
				PhaseResult();
				break;
			case BulletPhase.BackToPool:
				PhaseBackToPool();
				break;
			}
		}
	}

	protected virtual void PhaseNormal()
	{
		UpdateExtraCollider();
		if (!hasReflect)
		{
			BulletReflect();
		}
		MoveBullet();
		float num = BulletData.f_DISTANCE;
		if (FreeDISTANCE > 0f)
		{
			num = FreeDISTANCE;
		}
		if (BulletData.n_SHOTLINE == 6)
		{
			heapDistance = lineDistance;
		}
		else if (BulletData.n_SHOTLINE == 3 || BulletData.n_SHOTLINE == 4)
		{
			heapDistance = lineDistance / 0.9125f;
		}
		else
		{
			heapDistance += Vector2.Distance(lastPosition, _transform.position);
		}
		lastPosition = _transform.position;
		Set_Scale_Effect(heapDistance, num, false);
		if (heapDistance > num)
		{
			CheckRollBack();
		}
	}

	protected virtual void UpdateExtraCollider()
	{
		if (ExtraCollider != null && GameLogicUpdateManager.GameFrame > ExtraCollider.AliveFrame)
		{
			ExtraCollider.AliveFrame = 0;
		}
	}

	protected virtual void PhaseBoomerang()
	{
		Vector3 vector = ((!(MasterTransform != _transform) || !(MasterTransform != null)) ? (-(_transform.position - MasterPosition).normalized) : (-(_transform.position - MasterTransform.position).normalized));
		_transform.eulerAngles = new Vector3(0f, 0f, Vector2.SignedAngle(Vector2.right, vector));
		if (BulletData.n_ROLLBACK == 2 && bulletFxArray.Length != 0)
		{
			if (vector.x < 0f)
			{
				bulletFxArray[0].transform.localRotation = new Quaternion(0f, 0f, -1f, 0f);
			}
			else
			{
				bulletFxArray[0].transform.localRotation = new Quaternion(0f, 0f, 0f, 0f);
			}
		}
		Direction = vector;
		MoveBullet();
		float num = 0f;
		num = ((!(MasterTransform != _transform) || !(MasterTransform != null)) ? Vector2.Distance(_transform.position, MasterPosition) : Vector2.Distance(_transform.position, MasterTransform.position));
		Set_Scale_Effect(num, BulletData.f_DISTANCE, true);
		if (num < 1f)
		{
			if (BulletData.f_RANGE == 0f)
			{
				Phase = BulletPhase.Result;
			}
			else
			{
				SetPhaseToSplash();
			}
		}
	}

	protected void SetPhaseToSplash()
	{
		_capsuleCollider.size = Vector2.one * BulletData.f_RANGE * 2f;
		Phase = BulletPhase.Splash;
		_splashWaitLogicFrame = GameLogicUpdateManager.GameFrame + 1;
	}

	protected virtual void PhaseSplash()
	{
		if ((float)GameLogicUpdateManager.GameFrame > _splashWaitLogicFrame)
		{
			Phase = BulletPhase.Result;
			tHurtPassParam.BulletFlg |= BulletFlag.Splash;
		}
	}

	protected virtual void PhaseResult()
	{
		if (BulletData.n_THROUGH == 0)
		{
			foreach (Transform hit in _hitList)
			{
				CaluDmg(BulletData, hit);
				if (nThrough > 0)
				{
					nThrough--;
				}
			}
		}
		if (BulletData.n_TYPE != 3)
		{
			int count = _hitList.Count;
			if (count == 0)
			{
				if (BulletData.f_RANGE != 0f)
				{
					GenerateImpactFx();
				}
				else
				{
					GenerateEndFx();
				}
			}
			else if (lastReflectTrans != null && count == 1)
			{
				GenerateEndFx();
			}
			else if (BulletData.f_RANGE != 0f)
			{
				GenerateImpactFx();
			}
			else
			{
				if (BulletData.n_ROLLBACK == 0)
				{
					GenerateImpactFx(false);
				}
				GenerateEndFx();
			}
		}
		Phase = BulletPhase.BackToPool;
	}

	protected virtual void PhaseBackToPool()
	{
		Stop();
		BackToPool();
	}

	protected virtual void CheckRollBack()
	{
		if (BulletData.n_ROLLBACK > 0)
		{
			Phase = BulletPhase.Boomerang;
			hasReflect = false;
			_hitList.Clear();
		}
		else if (BulletData.f_RANGE == 0f)
		{
			Phase = BulletPhase.Result;
		}
		else
		{
			SetPhaseToSplash();
		}
	}

	protected virtual void TrackingTarget()
	{
		if (activeTracking && TrackingData != null && ActivateTimer.GetMillisecond() >= TrackingData.n_BEGINTIME_1 && ActivateTimer.GetMillisecond() < TrackingData.n_ENDTIME_1)
		{
			FindTarget(trackPriority);
			if (Target != null)
			{
				DoAim(Target);
			}
		}
	}

	protected virtual void FindTarget(TrackPriority trackPriority)
	{
		if ((Target != null && (Target as StageObjBase).IsAlive()) || refPBMShoter == null || refPBMShoter.SOB == null)
		{
			return;
		}
		if (refPBMShoter != null && refPBMShoter.SOB.GetSOBType() == 1 && refPBMShoter.SOB.sNetSerialID != MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify)
		{
			GetTargetByPerGameSaveData();
			return;
		}
		switch (trackPriority)
		{
		case TrackPriority.EnemyFirst:
			if (Target == null && ((int)TargetMask & (1 << ManagedSingleton<OrangeLayerManager>.Instance.EnemyLayer)) != 0)
			{
				Target = NeutralAIS.GetClosetEnemy();
			}
			if (Target == null && ((int)TargetMask & (1 << ManagedSingleton<OrangeLayerManager>.Instance.PlayerLayer)) != 0)
			{
				Target = NeutralAIS.GetClosetPlayer();
			}
			if (Target == null && (refPBMShoter.SOB == null || refPBMShoter.SOB.gameObject.layer != ManagedSingleton<OrangeLayerManager>.Instance.PvpPlayerLayer))
			{
				Target = NeutralAIS.GetClosetPvpPlayer();
			}
			break;
		case TrackPriority.PlayerFirst:
			if (Target == null && ((int)TargetMask & (1 << ManagedSingleton<OrangeLayerManager>.Instance.PlayerLayer)) != 0)
			{
				Target = NeutralAIS.GetClosetPlayer();
			}
			if (Target == null && (refPBMShoter.SOB == null || refPBMShoter.SOB.gameObject.layer != ManagedSingleton<OrangeLayerManager>.Instance.PvpPlayerLayer))
			{
				Target = NeutralAIS.GetClosetPvpPlayer();
			}
			if (Target == null && ((int)TargetMask & (1 << ManagedSingleton<OrangeLayerManager>.Instance.EnemyLayer)) != 0)
			{
				Target = NeutralAIS.GetClosetEnemy();
			}
			break;
		case TrackPriority.NearFirst:
		{
			IAimTarget aimTarget = null;
			IAimTarget aimTarget2 = null;
			IAimTarget aimTarget3 = null;
			float num = float.MaxValue;
			if (((int)TargetMask & (1 << ManagedSingleton<OrangeLayerManager>.Instance.EnemyLayer)) != 0)
			{
				aimTarget = NeutralAIS.GetClosetEnemy();
			}
			if (((int)TargetMask & (1 << ManagedSingleton<OrangeLayerManager>.Instance.PlayerLayer)) != 0)
			{
				aimTarget2 = NeutralAIS.GetClosetPlayer();
			}
			if (refPBMShoter.SOB == null || refPBMShoter.SOB.gameObject.layer != ManagedSingleton<OrangeLayerManager>.Instance.PvpPlayerLayer)
			{
				aimTarget3 = NeutralAIS.GetClosetPvpPlayer();
			}
			if (aimTarget != null)
			{
				num = Vector2.Distance(_transform.position.xy(), aimTarget.AimPosition.xy());
				Target = aimTarget;
			}
			if (aimTarget2 != null)
			{
				float num2 = Vector2.Distance(_transform.position.xy(), aimTarget2.AimPosition.xy());
				if (num2 < num)
				{
					num = num2;
					Target = aimTarget2;
				}
			}
			if (aimTarget3 != null && aimTarget3 != aimTarget2)
			{
				float num3 = Vector2.Distance(_transform.position.xy(), aimTarget3.AimPosition.xy());
				if (num3 < num)
				{
					num = num3;
					Target = aimTarget3;
				}
			}
			break;
		}
		}
		SendTargetMsg();
	}

	protected void DoAim(IAimTarget target = null)
	{
		if (target == null)
		{
			return;
		}
		float num = Vector2.SignedAngle(From, Direction);
		if (!(target.AimTransform == null))
		{
			float num2 = Vector2.SignedAngle(From, (target.AimTransform.position + target.AimPoint - _transform.position).normalized);
			float num3 = Mathf.Abs(num2 - num);
			if (num3 > 180f)
			{
				num2 = (float)(-Math.Sign(num2)) * (360f - Mathf.Abs(num2));
				num3 = Mathf.Abs(num2 - num);
			}
			float num4 = num + (float)Math.Sign(num2 - num) * Math.Min(num3, 0.01f * Time.deltaTime * 720f * (float)TrackingData.n_POWER);
			Direction = new Vector3(Mathf.Cos(num4 * ((float)Math.PI / 180f)), Mathf.Sin(num4 * ((float)Math.PI / 180f)), 0f).normalized;
			_transform.eulerAngles = new Vector3(0f, 0f, num4);
		}
	}

	protected bool Check_IS_DAMAGE_COUNT(Collider2D col)
	{
		if ((col.gameObject.layer == ManagedSingleton<OrangeLayerManager>.Instance.PlayerLayer || col.gameObject.layer == ManagedSingleton<OrangeLayerManager>.Instance.PvpPlayerLayer || col.gameObject.layer == ManagedSingleton<OrangeLayerManager>.Instance.EnemyLayer) && BulletData.n_NUM_SHOOT > 1 && Phase != BulletPhase.Splash)
		{
			int value = -1;
			HitCount.TryGetValue(col.transform, out value);
			if (value == -1)
			{
				HitCount.Add(col.transform, 1);
			}
			else
			{
				HitCount[col.transform] = value + 1;
			}
			foreach (KeyValuePair<Transform, int> item in HitCount)
			{
				if (BulletData.n_DAMAGE_COUNT != 0 && item.Value > BulletData.n_DAMAGE_COUNT)
				{
					return true;
				}
			}
		}
		return false;
	}

	public override void Hit(Collider2D col)
	{
		if (CheckHitList(ref _hitList, col.transform) || Check_IS_DAMAGE_COUNT(col))
		{
			return;
		}
		int num = 1 << col.gameObject.layer;
		switch (Phase)
		{
		case BulletPhase.Normal:
		case BulletPhase.Boomerang:
			HitCheck(col);
			if ((num & (int)TargetMask) != 0)
			{
				isHitBlock = false;
			}
			else if (col.gameObject.GetComponent<StageHurtObj>() == null)
			{
				isHitBlock = true;
			}
			else
			{
				needWeaponImpactSE = (isHitBlock = false);
			}
			if (nThrough > 0 && (num & (int)TargetMask) != 0)
			{
				if (lastHit != null)
				{
					CaluDmg(BulletData, lastHit);
				}
				GenerateImpactFx(false);
				nThrough--;
				if (nThrough == 0)
				{
					Phase = BulletPhase.BackToPool;
				}
			}
			else if (nThrough > 0 && lastHit != null && lastHit.gameObject.GetComponent<StageHurtObj>() != null)
			{
				CaluDmg(BulletData, lastHit);
				GenerateImpactFx();
				nThrough--;
				if (nThrough == 0)
				{
					Phase = BulletPhase.BackToPool;
				}
			}
			else if (BulletData.f_RANGE == 0f)
			{
				if (hasReflect)
				{
					hasReflect = false;
					if (isHitBlock)
					{
						_transform.rotation = reflectRotation;
						PlayReflectSE();
						_hitList.Clear();
						_hitList.Add(col.transform);
					}
					else
					{
						Phase = BulletPhase.Result;
					}
				}
				else
				{
					Phase = BulletPhase.Result;
				}
			}
			else
			{
				SetPhaseToSplash();
			}
			break;
		case BulletPhase.Splash:
			if ((num & (int)TargetMask) != 0 && !_hitList.Contains(col.transform))
			{
				_hitList.Add(col.transform);
				if (HitCallback != null)
				{
					HitCallback(col);
				}
			}
			break;
		case BulletPhase.Result:
			break;
		}
	}

	protected virtual void HitCheck(Collider2D col)
	{
		if (!_hitList.Contains(col.transform))
		{
			lastHit = col.transform;
			_hitList.Add(col.transform);
			if (HitCallback != null)
			{
				HitCallback(col);
			}
		}
	}

	public override void BackToPool()
	{
		MonoBehaviourSingleton<UpdateManager>.Instance.RemoveLateUpdate(this);
		ActivateTimer.TimerStop();
		Phase = BulletPhase.Normal;
		_hitList.Clear();
		_rigidbody2D.Sleep();
		_capsuleCollider.enabled = false;
		isSubBullet = false;
		FreeDISTANCE = 0f;
		isBuffTrigger = false;
		ResetReflect();
		base.BackToPool();
	}

	protected virtual void GenerateImpactFx(bool bPlaySE = true)
	{
		Quaternion quaternion = Quaternion.FromToRotation(Vector3.back, Direction);
		RaycastHit2D raycastHit2D = Physics2D.Raycast(oldPos, Direction, offset);
		if ((bool)raycastHit2D)
		{
			MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FxImpact, raycastHit2D.point, quaternion * BulletQuaternion, Array.Empty<object>());
		}
		else
		{
			MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FxImpact, _transform.position, quaternion * BulletQuaternion, Array.Empty<object>());
		}
		if (bPlaySE)
		{
			TryPlaySE();
		}
	}

	protected void TryPlaySE()
	{
		if (isHitBlock || needPlayEndSE || needWeaponImpactSE)
		{
			PlaySE(_HitGuardSE[0], _HitGuardSE[1], isForceSE);
		}
	}

	protected virtual void GenerateEndFx(bool bPlaySE = true)
	{
		if (bPlaySE && isHitBlock)
		{
			PlaySE(_HitGuardSE[0], _HitGuardSE[1], isForceSE);
		}
		if (!(FxEnd == "") && FxEnd != null)
		{
			Quaternion quaternion = Quaternion.FromToRotation(Vector3.back, Direction);
			RaycastHit2D raycastHit2D = Physics2D.Raycast(oldPos, Direction, offset);
			if ((bool)raycastHit2D)
			{
				_transform.position = raycastHit2D.point;
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FxEnd, raycastHit2D.point, quaternion * BulletQuaternion, Array.Empty<object>());
			}
			else
			{
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FxEnd, _transform.position, quaternion * BulletQuaternion, Array.Empty<object>());
			}
		}
	}

	protected void PlayReflectSE()
	{
		if (_HitReflectSE != null && _HitReflectSE.Length > 1)
		{
			PlaySE(_HitReflectSE[0], _HitReflectSE[1]);
		}
		else if (_HitGuardSE != null)
		{
			PlaySE(_HitGuardSE[0], _HitGuardSE[1]);
		}
	}

	protected virtual void MoveBullet()
	{
		if (_rigidbody2D != null)
		{
			_rigidbody2D.WakeUp();
		}
		oldPos = _transform.position;
		Vector3 translation = Velocity * Time.deltaTime;
		if (hasReflect)
		{
			float num = Math.Max(0.5f, _capsuleCollider.size.x);
			if (Vector2.Distance(oldPos, reflectPoint) <= num)
			{
				PlayReflectSE();
				hasReflect = false;
				_transform.SetPositionAndRotation(reflectPoint, reflectRotation);
				_hitList.Clear();
				_hitList.Add(lastReflectTrans);
			}
			_transform.Translate(translation);
		}
		else
		{
			_transform.Translate(translation);
		}
		switch (BulletData.n_SHOTLINE)
		{
		case 3:
		case 4:
		{
			lineDistance += Vector2.Distance(lastPosition, _transform.position);
			float num3 = amplitude * Mathf.Sin(omega * (Time.fixedTime - shootTime)) * Time.timeScale;
			_transform.position = new Vector3(_transform.position.x, _transform.position.y + num3 - lastWaveDistance, _transform.position.z);
			lastWaveDistance = num3;
			break;
		}
		case 6:
		{
			lineDistance += Vector2.Distance(lastPosition, _transform.position);
			float num2 = amplitude * Mathf.Sin(omega * (Time.fixedTime - shootTime)) * Time.timeScale;
			_transform.position = new Vector3(_transform.position.x, MasterPosition.y + num2, _transform.position.z);
			break;
		}
		}
		offset = Math.Max(translation.magnitude, DefaultRadiusX * 2f);
		_colliderOffset.x = (0f - offset) / 2f;
		_colliderSize.x = offset;
		_capsuleCollider.size = _colliderSize;
	}

	public override void Active(Vector3 pPos, Vector3 pDirection, LayerMask pTargetMask, IAimTarget pTarget = null)
	{
		ActivateTimer.TimerStart();
		if (activeTracking)
		{
			Target = pTarget;
			if (TrackingData != null)
			{
				NeutralAIS.UpdateAimRange(TrackingData.f_RANGE);
				switch (TrackingData.n_TARGET)
				{
				case 0:
					trackPriority = TrackPriority.EnemyFirst;
					break;
				case 1:
					trackPriority = TrackPriority.PlayerFirst;
					break;
				case 2:
					trackPriority = TrackPriority.NearFirst;
					break;
				default:
					trackPriority = TrackPriority.EnemyFirst;
					break;
				}
			}
		}
		base.Active(pPos, pDirection, pTargetMask, pTarget);
	}

	public override void Active(Transform pTransform, Vector3 pDirection, LayerMask pTargetMask, IAimTarget pTarget = null)
	{
		ActivateTimer.TimerStart();
		if (activeTracking)
		{
			Target = pTarget;
			if (TrackingData != null)
			{
				NeutralAIS.UpdateAimRange(TrackingData.f_RANGE);
				switch (TrackingData.n_TARGET)
				{
				case 0:
					trackPriority = TrackPriority.EnemyFirst;
					break;
				case 1:
					trackPriority = TrackPriority.PlayerFirst;
					break;
				case 2:
					trackPriority = TrackPriority.NearFirst;
					break;
				default:
					trackPriority = TrackPriority.EnemyFirst;
					break;
				}
			}
		}
		base.Active(pTransform, pDirection, pTargetMask, pTarget);
	}

	public virtual void ChangeDirection(Vector3 newDirection)
	{
		_transform.eulerAngles = newDirection;
	}

	protected bool IsStageHurtObject(Collider2D collider)
	{
		if ((bool)collider)
		{
			if (collider.GetComponent<StageHurtObj>() != null)
			{
				return true;
			}
			return false;
		}
		return false;
	}

	protected virtual void BulletReflect()
	{
		if (reflectCount <= 0 || Velocity.Equals(Vector3.zero))
		{
			hasReflect = false;
			return;
		}
		Vector3 vector = ((Velocity.x > 0f) ? Vector3.right : Vector3.left);
		Vector3 vector2 = _transform.TransformDirection(vector);
		bool flag = false;
		RaycastHit2D[] array = Physics2D.RaycastAll(reflectPoint, vector2, float.PositiveInfinity, 1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockLayer);
		for (int i = 0; i < array.Length; i++)
		{
			RaycastHit2D raycastHit2D = array[i];
			if (!IsStageHurtObject(raycastHit2D.collider))
			{
				reflectCount--;
				reflectPoint = raycastHit2D.point + raycastHit2D.normal * (_colliderSize.x * 0.5f);
				Vector2 vector3 = Vector2.Reflect(vector2, raycastHit2D.normal);
				reflectRotation = Quaternion.FromToRotation(vector, vector3) * BulletQuaternion;
				lastReflectTrans = raycastHit2D.transform;
				hasReflect = true;
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			reflectCount = 0;
			hasReflect = false;
		}
	}

	public List<Transform> GetHitList()
	{
		returingList.Clear();
		foreach (Transform hit in _hitList)
		{
			returingList.Add(hit);
		}
		return returingList;
	}

	private void ResetReflect()
	{
		hasReflect = false;
		reflectPoint = Vector2.zero;
		reflectRotation = Quaternion.identity;
	}
}
