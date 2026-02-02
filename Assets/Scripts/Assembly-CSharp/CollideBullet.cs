#define RELEASE
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollideBullet : BulletBase
{
	public new enum ColliderType
	{
		Box = 0,
		Circle = 1,
		UNKNOW = 2
	}

	public enum SpecialColliderID
	{
		X4UAPlasma = 10720,
		X4UAPlasma1 = 10721,
		X4UAPlasma2 = 10722,
		X4UAPlasma3 = 10723
	}

	private ColliderType colliderType = ColliderType.UNKNOW;

	[HideInInspector]
	public bool IsActivate;

	[HideInInspector]
	public bool IsDestroy;

	protected Camera _mainCamera;

	public bool AlwaysFaceCamera;

	protected Collider2D _hitCollider;

	protected HashSet<Transform> _ignoreList;

	protected OrangeTimer _clearTimer;

	protected OrangeTimer _hitGuardTimer;

	protected Rigidbody2D _rigidbody2D;

	protected long _duration = -1L;

	protected long _hurtCycle;

	protected OrangeTimer _durationTimer;

	protected readonly Dictionary<Transform, int> _hitCount = new Dictionary<Transform, int>();

	public double MaxHit = 5.0;

	public bool bNeedBackPoolColliderBullet;

	public bool bNeedBackPoolModelName;

	protected bool isCharacter_root;

	public string sCycleACB = "";

	public string sCycleCUE = "";

	public bool bUseRotation;

	private CollideBulletExit collideBulletExit;

	private bool isAwake;

	protected bool _forceClearList;

	public Collider2D HitTarget { get; protected set; }

	public void SetDuration(long time)
	{
		_duration = time;
	}

	protected virtual void OnTriggerStay2D(Collider2D col)
	{
		if (!ManagedSingleton<StageHelper>.Instance.bEnemyActive || MonoBehaviourSingleton<OrangeGameManager>.Instance.IsGamePause)
		{
			return;
		}
		Transform root = _transform.root;
		if (!IsActivate || (root != MonoBehaviourSingleton<PoolManager>.Instance.transform && col.transform.IsChildOf(root)))
		{
			return;
		}
		if (((1 << col.gameObject.layer) & (int)TargetMask) != 0 && !col.isTrigger)
		{
			StageObjParam component = col.GetComponent<StageObjParam>();
			if (component != null && component.tLinkSOB != null && ((1 << col.gameObject.layer) & (int)BlockMask) == 0)
			{
				if (component.tLinkSOB.GetSOBType() == 4)
				{
					PetControllerBase component2 = col.GetComponent<PetControllerBase>();
					if (component2 != null && component2.ignoreColliderBullet() && _transform.GetComponentInParent<EnemyControllerBase>() != null)
					{
						return;
					}
				}
				if ((int)component.tLinkSOB.Hp > 0)
				{
					Hit(col);
				}
			}
			else
			{
				PlayerCollider component3 = col.GetComponent<PlayerCollider>();
				if (component3 != null && component3.IsDmgReduceShield())
				{
					Hit(col);
				}
				else if ((BulletData.n_FLAG & 0x1000) == 0 && col.gameObject.GetComponentInParent<StageHurtObj>() != null)
				{
					Hit(col);
				}
			}
		}
		if (useHitGuardCount() && _hitGuardTimer.GetMillisecond() > _hurtCycle)
		{
			PlaySE(_HitGuardSE[0], _HitGuardSE[1], isForceSE);
			_hitGuardTimer.TimerStart();
		}
	}

	protected override void Awake()
	{
		if (!isAwake)
		{
			isAwake = true;
			base.Awake();
			_rigidbody2D = base.gameObject.AddOrGetComponent<Rigidbody2D>();
			_rigidbody2D.bodyType = RigidbodyType2D.Kinematic;
			_rigidbody2D.useFullKinematicContacts = true;
			IsDestroy = false;
			_ignoreList = new HashSet<Transform>();
			_mainCamera = MonoBehaviourSingleton<OrangeSceneManager>.Instance.MainCamera;
			_durationTimer = OrangeTimerManager.GetTimer();
			_clearTimer = OrangeTimerManager.GetTimer();
			_hitGuardTimer = OrangeTimerManager.GetTimer();
		}
	}

	public void Init()
	{
		Awake();
	}

	public void SetDirection(Vector3 newDir)
	{
		Direction = newDir;
	}

	public void Active(LayerMask pTargetMask)
	{
		IsDestroy = false;
		TargetMask = pTargetMask;
		if (BulletData.n_TARGET != 3)
		{
			TargetMask = (int)TargetMask | (int)BulletMask;
			TargetMask = (int)TargetMask | (int)BulletScriptableObjectInstance.BulletLayerMaskObstacle;
		}
		if (FxMuzzleFlare != "null")
		{
			MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FxMuzzleFlare, base.transform, Quaternion.identity, Array.Empty<object>());
		}
		if (CoroutineMove != null)
		{
			StopCoroutine(CoroutineMove);
		}
		bIsEnd = false;
		CoroutineMove = StartCoroutine(OnStartMove());
		_rigidbody2D.WakeUp();
		if ((base.gameObject.layer == ManagedSingleton<OrangeLayerManager>.Instance.PlayerLayer || base.gameObject.layer == ManagedSingleton<OrangeLayerManager>.Instance.RenderPlayer) && GetComponentInParent<OrangeCharacter>() != null)
		{
			base.gameObject.layer = ManagedSingleton<OrangeLayerManager>.Instance.BulletLayer;
		}
		if (GetComponentInParent<OrangeCharacter>() != null)
		{
			isCharacter_root = true;
		}
		base.SoundSource.UpdateDistanceCall();
		PlayUseSE();
	}

	public override void Active(Vector3 pPos, Vector3 pDirection, LayerMask pTargetMask, IAimTarget pTarget = null)
	{
		_transform.position = pPos;
		MonoBehaviourSingleton<FxManager>.Instance.UpdateFx(bulletFxArray, true);
		_rigidbody2D.WakeUp();
		Active(pTargetMask);
	}

	public override void Active(Transform pTransform, Vector3 pDirection, LayerMask pTargetMask, IAimTarget pTarget = null)
	{
		Debug.Log("NOPE");
		_transform.position = pTransform.position;
		MonoBehaviourSingleton<FxManager>.Instance.UpdateFx(bulletFxArray, true);
		_rigidbody2D.WakeUp();
		Active(pTargetMask);
	}

	public void Active(Transform pTransform, Quaternion pQuaternion, LayerMask pTargetMask, bool follow, IAimTarget pTarget = null)
	{
		Debug.Log("NOPE");
		Active(pTransform, Vector3.zero, pTargetMask, pTarget);
		if (follow)
		{
			Vector3 localScale = _transform.localScale;
			_transform.SetParent(pTransform);
			_transform.localPosition = Vector3.zero;
			_transform.localRotation = pQuaternion;
			_transform.localScale = localScale;
		}
		else
		{
			_transform.position = pTransform.position;
		}
	}

	protected void PlayCycleSE()
	{
		if (sCycleACB != "" && sCycleCUE != "")
		{
			base.SoundSource.PlaySE(sCycleACB, sCycleCUE);
		}
	}

	protected override IEnumerator OnStartMove()
	{
		double hitCnt = 1.0;
		IsActivate = true;
		_hitCollider.enabled = true;
		_clearTimer.TimerStart();
		_durationTimer.TimerStart();
		_ignoreList.Clear();
		_hitCount.Clear();
		if (useHitGuardCount())
		{
			_hitGuardTimer.TimerStart();
		}
		PlayCycleSE();
		while (!IsDestroy)
		{
			if (_clearTimer.GetMillisecond() >= _hurtCycle || _forceClearList)
			{
				_forceClearList = false;
				_clearTimer.TimerStart();
				_ignoreList.Clear();
				_rigidbody2D.WakeUp();
				if (BulletData.n_DAMAGE_COUNT != 0)
				{
					foreach (KeyValuePair<Transform, int> item in _hitCount)
					{
						if (item.Value >= BulletData.n_DAMAGE_COUNT)
						{
							_ignoreList.Add(item.Key);
						}
					}
				}
				if (hitCnt < MaxHit)
				{
					PlayCycleSE();
					hitCnt += 1.0;
				}
			}
			if (AlwaysFaceCamera)
			{
				if (_mainCamera == null)
				{
					_mainCamera = MonoBehaviourSingleton<OrangeSceneManager>.Instance.MainCamera;
				}
				if (_mainCamera != null)
				{
					base.transform.LookAt(_mainCamera.transform.position, -Vector3.up);
				}
			}
			if ((base.gameObject.layer == ManagedSingleton<OrangeLayerManager>.Instance.PlayerLayer || base.gameObject.layer == ManagedSingleton<OrangeLayerManager>.Instance.RenderPlayer) && isCharacter_root)
			{
				base.gameObject.layer = ManagedSingleton<OrangeLayerManager>.Instance.BulletLayer;
			}
			if (_duration != -1 && _durationTimer.GetMillisecond() >= _duration)
			{
				IsDestroy = true;
				break;
			}
			yield return CoroutineDefine._waitForEndOfFrame;
		}
		BackToPool();
		yield return null;
	}

	public void Reset_Duration_Time()
	{
		_duration = 0L;
		_ignoreList.Clear();
	}

	public override void BackToPool()
	{
		if (BackCallback != null)
		{
			BackCallback(this);
			BackCallback = null;
		}
		IsDestroy = false;
		IsActivate = false;
		bIsEnd = true;
		ClearInColliderBuff();
		_hitCount.Clear();
		if ((bool)_hitCollider)
		{
			_hitCollider.enabled = false;
		}
		targetPos = new Vector3(10000f, 10000f, 0f);
		MonoBehaviourSingleton<FxManager>.Instance.UpdateFx(bulletFxArray, false);
		_rigidbody2D.Sleep();
		isBuffTrigger = false;
		if (_UseSE != null && _UseSE[2] != "")
		{
			base.SoundSource.PlaySE(_UseSE[0], _UseSE[2]);
		}
		Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_BULLET_UNREGISTER, this);
		if (bNeedBackPoolColliderBullet)
		{
			MonoBehaviourSingleton<PoolManager>.Instance.BackToPool(this, "PoolColliderBullet");
		}
		else if (bNeedBackPoolModelName)
		{
			MonoBehaviourSingleton<PoolManager>.Instance.BackToPool(this, itemName);
		}
	}

	public override void Hit(Collider2D col)
	{
		if (BulletData.n_TARGET == 3)
		{
			OrangeCharacter component = col.transform.GetComponent<OrangeCharacter>();
			if ((bool)component)
			{
				if (component.sNetSerialID != refPBMShoter.SOB.sNetSerialID)
				{
					return;
				}
			}
			else
			{
				EnemyControllerBase component2 = col.transform.root.GetComponent<EnemyControllerBase>();
				col.transform.root.GetInstanceID();
				if ((bool)component2 && nRecordID != component2.gameObject.GetInstanceID())
				{
					return;
				}
			}
		}
		if (CheckHitList(ref _ignoreList, col.transform))
		{
			return;
		}
		if (HitCallback != null)
		{
			HitTarget = col;
			HitCallback(col);
		}
		else
		{
			HitTarget = null;
		}
		int value = -1;
		_hitCount.TryGetValue(col.transform, out value);
		if (value == -1)
		{
			_hitCount.Add(col.transform, 1);
		}
		else
		{
			_hitCount[col.transform] = value + 1;
		}
		_ignoreList.Add(col.transform);
		if (FxImpact != "null")
		{
			bool flag = false;
			if ((bool)col.transform.GetComponent<StageObjParam>())
			{
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FxImpact, col.transform.position, BulletQuaternion, Array.Empty<object>());
				flag = true;
			}
			if (!flag)
			{
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FxImpact, col.transform.position, BulletQuaternion, Array.Empty<object>());
			}
		}
		ReflashMissFactor();
		CaluDmg(BulletData, col.transform, 0f, 0.5f);
	}

	public override void UpdateBulletData(SKILL_TABLE pData, string owner = "", int nInRecordID = 0, int nInNetID = 0, int nDirection = 1)
	{
		BulletID = pData.n_ID;
		BulletData = pData;
		nNetID = nInNetID;
		nRecordID = nInRecordID;
		Owner = owner;
		if ((BulletData.n_FLAG & 1) == 0)
		{
			BlockMask = BulletScriptableObjectInstance.BulletLayerMaskObstacle;
		}
		if (((uint)BulletData.n_FLAG & 2u) != 0)
		{
			BulletMask = BulletScriptableObjectInstance.BulletLayerMaskBullet;
		}
		if (pData.s_FIELD == "null")
		{
			_hitCollider = base.gameObject.AddOrGetComponent<Collider2D>();
		}
		else
		{
			string[] array = pData.s_FIELD.Split(',');
			string text = array[0];
			if (!(text == "0"))
			{
				if (text == "1")
				{
					CheckLastBulletType(ColliderType.Circle);
					_hitCollider = base.gameObject.AddOrGetComponent<CircleCollider2D>();
					((CircleCollider2D)_hitCollider).radius = float.Parse(array[3]);
				}
			}
			else
			{
				CheckLastBulletType(ColliderType.Box);
				_hitCollider = base.gameObject.AddOrGetComponent<BoxCollider2D>();
				((BoxCollider2D)_hitCollider).size = new Vector2(float.Parse(array[3]), float.Parse(array[4]));
			}
			_duration = long.Parse(array[5]);
			_hurtCycle = long.Parse(array[6]);
			MaxHit = Math.Truncate((double)_duration / (double)_hurtCycle);
			_hitCollider.offset = new Vector2(float.Parse(array[1]), float.Parse(array[2]));
		}
		_hitCollider.isTrigger = true;
		if (!bUseRotation)
		{
			_transform.localScale = new Vector3(nDirection, 1f, 1f);
			_transform.eulerAngles = new Vector3(_transform.eulerAngles.x, 0f, _transform.eulerAngles.z);
		}
		else
		{
			float y = ((nDirection != 1) ? 180 : 0);
			_transform.eulerAngles = new Vector3(_transform.eulerAngles.x, y, _transform.eulerAngles.z);
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
		_UseSE = ManagedSingleton<OrangeTableHelper>.Instance.ParseSE(BulletData.s_USE_SE);
		if (_UseSE[0] != "" && (_UseSE[1].EndsWith("_lp") || _UseSE[1].EndsWith("_lg")))
		{
			checkLoopSE = true;
		}
		_HitSE = ManagedSingleton<OrangeTableHelper>.Instance.ParseSE(BulletData.s_HIT_SE);
		GetHistGuardSE();
		if (((uint)BulletData.n_FLAG & 0x80u) != 0)
		{
			collideBulletExit = base.gameObject.AddOrGetComponent<CollideBulletExit>();
			collideBulletExit.Setup(OnTargetExit);
		}
	}

	protected bool useHitGuardCount()
	{
		if (BulletID >= 10720)
		{
			return BulletID <= 10723;
		}
		return false;
	}

	public void ForceClearList()
	{
		_forceClearList = true;
	}

	public Vector2 GetOffset()
	{
		if ((bool)_hitCollider)
		{
			return _hitCollider.offset;
		}
		return Vector2.zero;
	}

	public void SetOffset(Vector2 offset)
	{
		if ((bool)_hitCollider)
		{
			_hitCollider.offset = offset;
		}
	}

	public void Sleep()
	{
		if (_hitCollider != null)
		{
			_hitCollider.enabled = false;
		}
	}

	public void WakeUp()
	{
		if (_hitCollider != null)
		{
			_hitCollider.enabled = true;
		}
	}

	public Collider2D GetHitCollider()
	{
		return _hitCollider;
	}

	private void CheckLastBulletType(ColliderType p_newColliderType)
	{
		if (p_newColliderType != colliderType)
		{
			Sleep();
			colliderType = p_newColliderType;
		}
	}

	public OrangeTimer GetClearTimer()
	{
		return _clearTimer;
	}

	private void ClearInColliderBuff()
	{
		if (!collideBulletExit || BulletData.n_TARGET != 3)
		{
			return;
		}
		collideBulletExit.Clear();
		foreach (KeyValuePair<Transform, int> item in _hitCount)
		{
			if (item.Key != null)
			{
				OrangeCharacter component = item.Key.GetComponent<OrangeCharacter>();
				if ((bool)component && component.sNetSerialID == refPBMShoter.SOB.sNetSerialID)
				{
					component.selfBuffManager.RemoveBuffByCONDITIONID(BulletData.n_CONDITION_ID);
				}
			}
		}
	}

	private void OnTargetExit(Transform target)
	{
		int n_TARGET = BulletData.n_TARGET;
		if (n_TARGET == 3)
		{
			OrangeCharacter component = target.GetComponent<OrangeCharacter>();
			if ((bool)component && component.sNetSerialID == refPBMShoter.SOB.sNetSerialID && _hitCount.ContainsKey(target))
			{
				component.selfBuffManager.RemoveBuffByCONDITIONID(BulletData.n_CONDITION_ID);
			}
			if (_hitCount.ContainsKey(target))
			{
				_hitCount.Remove(target);
			}
			_ignoreList.Remove(target);
		}
	}
}
