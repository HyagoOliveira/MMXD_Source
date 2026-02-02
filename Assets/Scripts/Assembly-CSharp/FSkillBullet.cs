using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FSkillBullet : BulletBase, IManagedLateUpdateBehavior
{
	public enum BulletPhase
	{
		Normal = 0,
		Splash = 1,
		Boomerang = 2,
		Result = 3,
		BackToPool = 4
	}

	[HideInInspector]
	public BulletPhase Phase = BulletPhase.BackToPool;

	public float DefaultRadius = 0.25f;

	protected HashSet<Transform> _hitList;

	protected CapsuleCollider2D _capsuleCollider;

	protected BoxCollider2D _extraCollider;

	public float FreeDISTANCE;

	private float offset;

	private Vector3 oldPos;

	private Vector3 beginPosition;

	protected Vector2 _colliderOffset;

	protected Vector2 _colliderSize;

	private Vector3 vTmpVelocity;

	private Rigidbody2D _rigidbody2D;

	private int _splashWaitFrame;

	private int _extraColliderWaitFrame;

	private Collider2D _hitCollider;

	private long _duration = -1L;

	private long _hurtCycle;

	private OrangeTimer _durationTimer;

	private OrangeTimer _clearTimer;

	public string fx_end_play = "";

	protected bool hasReflect;

	protected Vector2 reflectPoint = Vector2.zero;

	protected Quaternion reflectRotation = Quaternion.identity;

	protected Transform lastReflectTrans;

	protected List<Transform> returingList = new List<Transform>();

	private int AimID = -1;

	public Vector3 From = Vector3.right;

	private Transform lastHit;

	protected override void Awake()
	{
		base.Awake();
		Phase = BulletPhase.Normal;
		_rigidbody2D = base.gameObject.AddOrGetComponent<Rigidbody2D>();
		_rigidbody2D.bodyType = RigidbodyType2D.Kinematic;
		_rigidbody2D.useFullKinematicContacts = true;
		_capsuleCollider = base.gameObject.AddOrGetComponent<CapsuleCollider2D>();
		_capsuleCollider.isTrigger = true;
		_extraCollider = base.gameObject.AddOrGetComponent<BoxCollider2D>();
		if (_extraCollider != null)
		{
			_extraCollider.size = new Vector2(1f, 0.3f);
			_extraCollider.offset = new Vector2(-1f, 0f);
			_extraCollider.isTrigger = true;
		}
		_colliderSize = Vector2.one * DefaultRadius * 2f;
		_colliderOffset = Vector2.zero;
		_hitList = new HashSet<Transform>();
		_capsuleCollider.enabled = false;
		_clearTimer = OrangeTimerManager.GetTimer();
		_durationTimer = OrangeTimerManager.GetTimer();
		if (!fx_end_play.Equals(""))
		{
			MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(fx_end_play, 2);
		}
	}

	private void OnTriggerEnter2D(Collider2D col)
	{
		if (!ManagedSingleton<StageHelper>.Instance.bEnemyActive || MonoBehaviourSingleton<OrangeGameManager>.Instance.IsGamePause || col.isTrigger || ((1 << col.gameObject.layer) & (int)UseMask) == 0)
		{
			return;
		}
		int layer = col.gameObject.layer;
		if (BulletData.n_FLAG == 1 && col.transform.gameObject.GetComponent<StageHurtObj>() == null && layer == ManagedSingleton<OrangeLayerManager>.Instance.BlockLayer)
		{
			return;
		}
		StageObjParam component = col.GetComponent<StageObjParam>();
		if (component != null && component.tLinkSOB != null)
		{
			if ((int)component.tLinkSOB.Hp > 0)
			{
				Hit(col);
			}
		}
		else if (!_extraCollider.enabled)
		{
			Hit(col);
		}
	}

	private void OnTriggerStay2D(Collider2D col)
	{
		if (!ManagedSingleton<StageHelper>.Instance.bEnemyActive || MonoBehaviourSingleton<OrangeGameManager>.Instance.IsGamePause || col.isTrigger || ((1 << col.gameObject.layer) & (int)UseMask) == 0)
		{
			return;
		}
		int layer = col.gameObject.layer;
		if (BulletData.n_FLAG == 1 && col.transform.gameObject.GetComponent<StageHurtObj>() == null && layer == ManagedSingleton<OrangeLayerManager>.Instance.BlockLayer)
		{
			return;
		}
		StageObjParam component = col.GetComponent<StageObjParam>();
		if (component != null && component.tLinkSOB != null)
		{
			if ((int)component.tLinkSOB.Hp > 0)
			{
				Hit(col);
			}
		}
		else if (!_extraCollider.enabled)
		{
			Hit(col);
		}
	}

	public void UPdata_MapObject_Mask()
	{
		if (BulletData.n_FLAG == 1)
		{
			BlockMask = BulletScriptableObjectInstance.BulletLayerMaskObstacle;
		}
	}

	public virtual void SubBullet_Single_Horizontal(Vector3 mPos, float currentAngle, float mFreeDISTANCE)
	{
		PoolManager instance = MonoBehaviourSingleton<PoolManager>.Instance;
		if (currentAngle != 0f)
		{
			Vector3 pPos = new Vector3(mPos.x + currentAngle, mPos.y, mPos.z);
			FSkillBullet poolObj = instance.GetPoolObj<FSkillBullet>(BulletData.s_MODEL);
			poolObj.UpdateBulletData(BulletData, Owner);
			poolObj.UPdata_MapObject_Mask();
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
			poolObj.isSubBullet = true;
			poolObj.FreeDISTANCE = mFreeDISTANCE;
			poolObj.Active(pPos, Direction, TargetMask);
		}
	}

	private IEnumerator SubBullet_Horizontal_Time(Vector3 SourcePos)
	{
		yield return new WaitForSeconds(0.2f);
		while (MonoBehaviourSingleton<OrangeGameManager>.Instance.IsGamePause)
		{
			yield return CoroutineDefine._waitForEndOfFrame;
		}
		float OneAngle = 2.5f;
		float StargeAngle = (float)((BulletData.n_NUM_SHOOT - 1) / 2) * OneAngle;
		float num = StargeAngle - 5f * OneAngle;
		if (num != 0f)
		{
			SubBullet_Single_Horizontal(SourcePos, num, 14f);
		}
		yield return new WaitForSeconds(0.2f);
		while (MonoBehaviourSingleton<OrangeGameManager>.Instance.IsGamePause)
		{
			yield return CoroutineDefine._waitForEndOfFrame;
		}
		num = StargeAngle - 3f * OneAngle;
		if (num != 0f)
		{
			SubBullet_Single_Horizontal(SourcePos, num, 16f);
		}
		yield return new WaitForSeconds(0.2f);
		while (MonoBehaviourSingleton<OrangeGameManager>.Instance.IsGamePause)
		{
			yield return CoroutineDefine._waitForEndOfFrame;
		}
		num = StargeAngle - 1f * OneAngle;
		if (num != 0f)
		{
			SubBullet_Single_Horizontal(SourcePos, num, 14f);
		}
		yield return new WaitForSeconds(0.2f);
		while (MonoBehaviourSingleton<OrangeGameManager>.Instance.IsGamePause)
		{
			yield return CoroutineDefine._waitForEndOfFrame;
		}
		num = StargeAngle - 4f * OneAngle;
		if (num != 0f)
		{
			SubBullet_Single_Horizontal(SourcePos, num, 15f);
		}
		yield return new WaitForSeconds(0.2f);
		while (MonoBehaviourSingleton<OrangeGameManager>.Instance.IsGamePause)
		{
			yield return CoroutineDefine._waitForEndOfFrame;
		}
		num = StargeAngle - 0f * OneAngle;
		if (num != 0f)
		{
			SubBullet_Single_Horizontal(SourcePos, num, 14f);
		}
		yield return new WaitForSeconds(0.2f);
		while (MonoBehaviourSingleton<OrangeGameManager>.Instance.IsGamePause)
		{
			yield return CoroutineDefine._waitForEndOfFrame;
		}
		num = StargeAngle - 1f * OneAngle;
		if (num != 0f)
		{
			SubBullet_Single_Horizontal(SourcePos, num, 13f);
		}
	}

	public virtual void SubBullet_Horizontal()
	{
		PoolManager instance = MonoBehaviourSingleton<PoolManager>.Instance;
		float num = BulletData.f_ANGLE / (float)(BulletData.n_NUM_SHOOT - 1);
		float num2 = (float)((BulletData.n_NUM_SHOOT - 1) / 2) * num;
		for (int i = 0; i < BulletData.n_NUM_SHOOT; i++)
		{
			float num3 = num2 - (float)i * num;
			if (num3 != 0f)
			{
				Vector3 pPos = new Vector3(MasterTransform.position.x + num3, MasterTransform.position.y, MasterTransform.position.z);
				FSkillBullet poolObj = instance.GetPoolObj<FSkillBullet>(BulletData.s_MODEL);
				poolObj.UpdateBulletData(BulletData, Owner);
				poolObj.UPdata_MapObject_Mask();
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
				poolObj.isSubBullet = true;
				poolObj.Active(pPos, Direction, TargetMask);
			}
		}
	}

	public virtual void SubBullet()
	{
		PoolManager instance = MonoBehaviourSingleton<PoolManager>.Instance;
		float num = BulletData.f_ANGLE / (float)(BulletData.n_NUM_SHOOT - 1);
		float num2 = (float)((BulletData.n_NUM_SHOOT - 1) / 2) * num;
		for (int i = 0; i < BulletData.n_NUM_SHOOT; i++)
		{
			float num3 = num2 - (float)i * num;
			if (num3 != 0f)
			{
				Vector3 pDirection = Quaternion.Euler(0f, 0f, num3) * Direction;
				FSkillBullet poolObj = instance.GetPoolObj<FSkillBullet>(BulletData.s_MODEL);
				poolObj.UpdateBulletData(BulletData, Owner);
				poolObj.UPdata_MapObject_Mask();
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
				poolObj.isSubBullet = true;
				poolObj.Active(MasterTransform, pDirection, TargetMask);
			}
		}
	}

	protected override IEnumerator OnStartMove()
	{
		yield return null;
	}

	public override void OnStart()
	{
		base.OnStart();
		MonoBehaviourSingleton<UpdateManager>.Instance.AddLateUpdate(this);
		if (BulletData.n_NUM_SHOOT > 1 && !isSubBullet)
		{
			if (BulletData.n_SHOTLINE == 0)
			{
				SubBullet();
			}
			else if (BulletData.n_SHOTLINE == 1)
			{
				SubBullet_Horizontal();
			}
			else if (BulletData.n_SHOTLINE == 2)
			{
				StartCoroutine(SubBullet_Horizontal_Time(new Vector3(MasterTransform.position.x, MasterTransform.position.y, MasterTransform.position.z)));
				FreeDISTANCE = 17f;
			}
		}
		if (BulletData.n_SHOTLINE == 1)
		{
			Direction = new Vector3(0f, Direction.x, 0f);
			_transform.eulerAngles = new Vector3(0f, 0f, 0f);
			_transform.position = new Vector3(_transform.position.x, _transform.position.y - 10f, _transform.position.z);
			Velocity = new Vector3(0f, BulletData.n_SPEED, 0f);
		}
		else if (BulletData.n_SHOTLINE == 2)
		{
			Direction = new Vector3(0.9f, -1f, 0f);
			_transform.eulerAngles = new Vector3(0f, 0f, Vector2.SignedAngle(Vector2.right, Direction));
			_transform.position = new Vector3(_transform.position.x + 7.5f, _transform.position.y + 10f, _transform.position.z);
			Velocity = new Vector3(0f, -BulletData.n_SPEED, 0f);
		}
		_capsuleCollider.enabled = true;
		if (_extraCollider != null && (int)TargetMask == (int)ManagedSingleton<OrangeLayerManager>.Instance.PlayerUseMask)
		{
			_extraCollider.enabled = true;
			_extraColliderWaitFrame = 2;
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
		if (BulletData.n_TYPE == 5)
		{
			if (_hitCollider != null)
			{
				UnityEngine.Object.Destroy(_hitCollider);
			}
			if (BulletData.s_FIELD == "null")
			{
				_hitCollider = base.gameObject.AddOrGetComponent<Collider2D>();
			}
			else
			{
				string[] array = BulletData.s_FIELD.Split(',');
				string text = array[0];
				if (!(text == "0"))
				{
					if (text == "1")
					{
						_hitCollider = base.gameObject.AddComponent<CircleCollider2D>();
						((CircleCollider2D)_hitCollider).radius = float.Parse(array[3]);
					}
				}
				else
				{
					_hitCollider = base.gameObject.AddComponent<BoxCollider2D>();
					((BoxCollider2D)_hitCollider).size = new Vector2(float.Parse(array[3]), float.Parse(array[4]));
				}
				_duration = long.Parse(array[5]);
				_hurtCycle = long.Parse(array[6]);
				_hitCollider.offset = new Vector2(float.Parse(array[1]), float.Parse(array[2]));
			}
			_hitCollider.isTrigger = true;
			_capsuleCollider.enabled = false;
			_extraCollider.enabled = false;
			_clearTimer.TimerStart();
			_durationTimer.TimerStart();
		}
		if ((bool)MasterTransform && !string.IsNullOrEmpty(FxMuzzleFlare) && !isSubBullet)
		{
			if (BulletData.n_USE_FX_FOLLOW == 0)
			{
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FxMuzzleFlare, MasterTransform.position, _transform.rotation * BulletQuaternion, Array.Empty<object>());
			}
			else if (BulletData.n_USE_FX_FOLLOW == 2)
			{
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FxMuzzleFlare, MasterTransform.position, Quaternion.identity, Array.Empty<object>());
			}
			else
			{
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FxMuzzleFlare, MasterTransform, BulletQuaternion, Array.Empty<object>());
			}
		}
	}

	public virtual void OnDisable()
	{
		MonoBehaviourSingleton<UpdateManager>.Instance.RemoveLateUpdate(this);
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

	private void BulletReflect()
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

	private void ShootTypeUpdate()
	{
		switch (Phase)
		{
		case BulletPhase.Normal:
		{
			if (_extraColliderWaitFrame > 0)
			{
				_extraColliderWaitFrame--;
			}
			else if (_extraCollider != null)
			{
				_extraCollider.enabled = false;
			}
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
			if (Mathf.Abs(Vector3.Distance(beginPosition, _transform.position)) > num)
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
					_capsuleCollider.size = Vector2.one * BulletData.f_RANGE * 2f;
					Phase = BulletPhase.Splash;
					_splashWaitFrame = 2;
				}
			}
			break;
		}
		case BulletPhase.Boomerang:
		{
			Vector3 vector = -(_transform.position - MasterTransform.position).normalized;
			_transform.eulerAngles = new Vector3(0f, 0f, Vector2.SignedAngle(Vector2.right, vector));
			Direction = vector;
			MoveBullet();
			if (Vector2.Distance(_transform.position, MasterTransform.position) < 0.3f)
			{
				if (BulletData.f_RANGE == 0f)
				{
					Phase = BulletPhase.Result;
					break;
				}
				_capsuleCollider.size = Vector2.one * BulletData.f_RANGE * 2f;
				Phase = BulletPhase.Splash;
				_splashWaitFrame = 2;
			}
			break;
		}
		case BulletPhase.Splash:
			if (_splashWaitFrame > 0)
			{
				_splashWaitFrame--;
			}
			else
			{
				Phase = BulletPhase.Result;
			}
			break;
		case BulletPhase.Result:
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
				if (lastReflectTrans != null && count == 1)
				{
					GenerateEndFx();
				}
				else if (_hitList.Count != 0 || BulletData.f_RANGE != 0f)
				{
					GenerateImpactFx();
				}
				else
				{
					GenerateEndFx();
				}
			}
			Phase = BulletPhase.BackToPool;
			break;
		case BulletPhase.BackToPool:
			Stop();
			BackToPool();
			break;
		}
		if (activeTracking && TrackingData != null && ActivateTimer.GetMillisecond() >= TrackingData.n_BEGINTIME_1 && ActivateTimer.GetMillisecond() < TrackingData.n_ENDTIME_1)
		{
			IAimTarget closetPlayer = NeutralAIS.GetClosetPlayer();
			if (closetPlayer != null && AimID == -1)
			{
				DoAim(closetPlayer.AimTransform);
			}
		}
	}

	private void CollideTypeUpdate()
	{
		_hitCollider.enabled = true;
		_transform.position = MasterTransform.position;
		if (_clearTimer.GetMillisecond() >= _hurtCycle)
		{
			_clearTimer.TimerStart();
			_hitList.Clear();
			_rigidbody2D.WakeUp();
		}
		if (_duration != -1 && _durationTimer.GetMillisecond() > _duration)
		{
			BackToPool();
			if (!fx_end_play.Equals(""))
			{
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(fx_end_play, MasterTransform, Quaternion.identity, Array.Empty<object>());
			}
		}
	}

	private void BuffTypeUpdate()
	{
		CaluDmg(BulletData, MasterTransform.root.transform);
		if (BulletData.n_CONDITION_ID == 0)
		{
			PlaySE(_HitSE[0], _HitSE[1], isForceSE);
			MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(BulletData.s_HIT_FX, MasterTransform.root.transform, Quaternion.identity, Array.Empty<object>());
		}
		Stop();
		BackToPool();
	}

	public void LateUpdateFunc()
	{
		if (!ManagedSingleton<StageHelper>.Instance.bEnemyActive || MonoBehaviourSingleton<OrangeGameManager>.Instance.IsGamePause)
		{
			if (BulletData.n_TYPE == 5)
			{
				_transform.position = MasterTransform.position;
			}
			return;
		}
		switch (BulletData.n_TYPE)
		{
		case 1:
			ShootTypeUpdate();
			break;
		case 5:
			CollideTypeUpdate();
			break;
		case 7:
			BuffTypeUpdate();
			break;
		default:
			ShootTypeUpdate();
			break;
		}
	}

	private void DoAim(Transform target = null)
	{
		if ((bool)target)
		{
			float num = Vector2.SignedAngle(From, Direction);
			float num2 = Vector2.SignedAngle(From, (target.position - _transform.position).normalized);
			if (Mathf.Abs(num2 - num) > 180f)
			{
				num2 = (float)(-Math.Sign(num2)) * (360f - Mathf.Abs(num2));
			}
			AimID = LeanTween.value(base.gameObject, num, num2, (float)(100 - TrackingData.n_POWER) * 0.01f).setOnUpdate(delegate(float f)
			{
				Direction = new Vector3(Mathf.Cos(f * ((float)Math.PI / 180f)), Mathf.Sin(f * ((float)Math.PI / 180f)), 0f).normalized;
				_transform.eulerAngles = new Vector3(0f, 0f, f);
			}).setOnComplete((Action)delegate
			{
				AimID = -1;
			})
				.uniqueId;
		}
	}

	public override void Hit(Collider2D col)
	{
		if (CheckHitList(ref _hitList, col.transform))
		{
			return;
		}
		int num = 1 << col.gameObject.layer;
		switch (Phase)
		{
		case BulletPhase.Normal:
		case BulletPhase.Boomerang:
			if (!_hitList.Contains(col.transform))
			{
				lastHit = col.transform;
				_hitList.Add(col.transform);
				if (BulletData.n_TYPE == 5)
				{
					CaluDmg(BulletData, col.transform);
				}
			}
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
				GenerateImpactFx();
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
					if (isHitBlock && lastReflectTrans != col.transform)
					{
						_transform.rotation = reflectRotation;
						_hitList.Clear();
						_hitList.Add(col.transform);
					}
					else
					{
						Phase = BulletPhase.Result;
					}
				}
				else if (BulletData.n_TYPE != 5)
				{
					Phase = BulletPhase.Result;
				}
			}
			else
			{
				_capsuleCollider.size = Vector2.one * BulletData.f_RANGE * 2f;
				Phase = BulletPhase.Splash;
				_splashWaitFrame = 2;
			}
			break;
		case BulletPhase.Splash:
			if ((num & (int)TargetMask) != 0 && !_hitList.Contains(col.transform))
			{
				_hitList.Add(col.transform);
				if (BulletData.n_TYPE == 5)
				{
					CaluDmg(BulletData, col.transform);
				}
			}
			break;
		case BulletPhase.Result:
			break;
		}
	}

	public override void BackToPool()
	{
		if (!bIsEnd)
		{
			MonoBehaviourSingleton<UpdateManager>.Instance.RemoveLateUpdate(this);
			ActivateTimer.TimerStop();
			if ((bool)_hitCollider)
			{
				_hitCollider.enabled = false;
			}
			Phase = BulletPhase.Normal;
			_hitList.Clear();
			_rigidbody2D.Sleep();
			_capsuleCollider.enabled = false;
			isSubBullet = false;
			FreeDISTANCE = 0f;
			ResetReflect();
			base.BackToPool();
		}
	}

	private void GenerateImpactFx()
	{
		Quaternion quaternion = Quaternion.FromToRotation(Vector3.back, Direction);
		RaycastHit2D raycastHit2D = Physics2D.Raycast(oldPos, Direction, offset);
		if ((bool)raycastHit2D)
		{
			_transform.position = raycastHit2D.point;
			MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FxImpact, raycastHit2D.point, quaternion * BulletQuaternion, Array.Empty<object>());
		}
		else
		{
			MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FxImpact, _transform.position, quaternion * BulletQuaternion, Array.Empty<object>());
		}
		if (isHitBlock || needPlayEndSE)
		{
			PlaySE(_HitGuardSE[0], _HitGuardSE[1]);
		}
	}

	private void GenerateEndFx()
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
		if (needPlayEndSE)
		{
			PlaySE(_HitGuardSE[0], _HitGuardSE[1]);
		}
	}

	private void MoveBullet()
	{
		if (_rigidbody2D != null)
		{
			_rigidbody2D.WakeUp();
		}
		oldPos = _transform.position;
		vTmpVelocity = Velocity * Time.deltaTime;
		if (hasReflect)
		{
			float num = Math.Max(0.5f, _capsuleCollider.size.x);
			if (Vector2.Distance(oldPos, reflectPoint) <= num)
			{
				if (_HitReflectSE != null && _HitReflectSE.Length > 1)
				{
					PlaySE(_HitReflectSE[0], _HitReflectSE[1]);
				}
				else if (_HitGuardSE != null)
				{
					PlaySE(_HitGuardSE[0], _HitGuardSE[1]);
				}
				hasReflect = false;
				_transform.SetPositionAndRotation(reflectPoint, reflectRotation);
				_hitList.Clear();
				_hitList.Add(lastReflectTrans);
			}
			_transform.Translate(vTmpVelocity);
		}
		else
		{
			_transform.Translate(vTmpVelocity);
		}
		offset = vTmpVelocity.magnitude;
	}

	public override void Active(Vector3 pPos, Vector3 pDirection, LayerMask pTargetMask, IAimTarget pTarget = null)
	{
		ActivateTimer.TimerStart();
		if (activeTracking)
		{
			NeutralAIS.UpdateAimRange(TrackingData.f_RANGE);
		}
		base.Active(pPos, pDirection, pTargetMask, pTarget);
	}

	public override void Active(Transform pTransform, Vector3 pDirection, LayerMask pTargetMask, IAimTarget pTarget = null)
	{
		ActivateTimer.TimerStart();
		if (activeTracking)
		{
			NeutralAIS.UpdateAimRange(TrackingData.f_RANGE);
		}
		base.Active(pTransform, pDirection, pTargetMask, pTarget);
	}
}
