using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CH103_XmastreeBullet : CollideBullet, IManagedLateUpdateBehavior
{
	public enum BulletPhase
	{
		Normal = 0,
		NonParent = 1,
		Splash = 2
	}

	public enum TrackPriority
	{
		EnemyFirst = 0,
		PlayerFirst = 1,
		NearFirst = 2
	}

	[SerializeField]
	private BulletPhase _nPhaseCanTriggerHit = BulletPhase.Splash;

	[SerializeField]
	private float _delaySE = 0.8f;

	[SerializeField]
	private Transform _tfStar;

	private ParticleSystem _psStart;

	protected BulletPhase _nPhase;

	protected Vector3 _vLocalPosition = Vector3.zero;

	protected Transform _tfTrackingTarget;

	protected int trackingEndFrame;

	protected int trackingFreezeFrame;

	protected bool CanTriggerHit
	{
		get
		{
			return _nPhase == _nPhaseCanTriggerHit;
		}
	}

	public override void PlayUseSE(bool force = false)
	{
		if (!isMuteSE)
		{
			base.SoundSource.PlaySE(_UseSE[0], _UseSE[1], _delaySE);
		}
	}

	protected new virtual void OnTriggerStay2D(Collider2D col)
	{
		if (CanTriggerHit)
		{
			base.OnTriggerStay2D(col);
		}
	}

	public override void UpdateBulletData(SKILL_TABLE pData, string owner = "", int nInRecordID = 0, int nInNetID = 0, int nDirection = 1)
	{
		base.UpdateBulletData(pData, owner, nInRecordID, nInNetID, nDirection);
		if (BulletData.n_TRACKING > 0)
		{
			TrackingData = ManagedSingleton<OrangeDataManager>.Instance.TRACKING_TABLE_DICT[BulletData.n_TRACKING];
			activeTracking = true;
		}
		if (NeutralAIS == null)
		{
			OrangeBattleUtility.AddNeutralAutoAimSystem(base.transform, out NeutralAIS);
		}
		trackingEndFrame = GameLogicUpdateManager.GameFrame + (int)((float)TrackingData.n_ENDTIME_1 * 0.001f / GameLogicUpdateManager.m_fFrameLen);
		trackingFreezeFrame = GameLogicUpdateManager.GameFrame + (int)((float)TrackingData.n_ENDTIME_2 * 0.001f / GameLogicUpdateManager.m_fFrameLen);
	}

	public override void Active(Vector3 pPos, Vector3 pDirection, LayerMask pTargetMask, IAimTarget pTarget = null)
	{
		_transform.position = pPos;
		MasterPosition = pPos;
		MonoBehaviourSingleton<FxManager>.Instance.UpdateFx(bulletFxArray, true);
		_rigidbody2D.WakeUp();
		_nPhase = BulletPhase.Normal;
		SetTarget(pTarget);
		SetStar();
		Active(pTargetMask);
	}

	public override void Active(Transform pTransform, Vector3 pDirection, LayerMask pTargetMask, IAimTarget pTarget = null)
	{
		_transform.position = pTransform.position;
		MasterPosition = pTransform.position;
		MonoBehaviourSingleton<FxManager>.Instance.UpdateFx(bulletFxArray, true);
		_rigidbody2D.WakeUp();
		_nPhase = BulletPhase.Normal;
		SetTarget(pTarget);
		SetStar();
		Active(pTargetMask);
	}

	protected void SetTarget(IAimTarget pTarget)
	{
		Target = pTarget;
		FindTarget(TrackPriority.PlayerFirst);
		if (IsTargetWithinRange(Target))
		{
			OrangeCharacter orangeCharacter = Target as OrangeCharacter;
			EnemyControllerBase enemyControllerBase = Target as EnemyControllerBase;
			if ((bool)orangeCharacter)
			{
				_tfTrackingTarget = orangeCharacter.transform;
				_vLocalPosition = Vector3.zero;
				_transform.position = _tfTrackingTarget.position;
			}
			else if ((bool)enemyControllerBase)
			{
				if (enemyControllerBase.AimTransform != null && Vector2.Distance(enemyControllerBase.AimPosition, enemyControllerBase.transform.position) > 3f)
				{
					_tfTrackingTarget = enemyControllerBase.AimTransform;
					_vLocalPosition = enemyControllerBase.AimPoint + Vector3.down * 3f;
					_transform.position = _tfTrackingTarget.position + _vLocalPosition;
				}
				else
				{
					_tfTrackingTarget = enemyControllerBase.transform;
					_vLocalPosition = Vector3.zero;
					_transform.position = _tfTrackingTarget.position + _vLocalPosition;
				}
			}
			else
			{
				_tfTrackingTarget = Target.AimTransform;
				_transform.position = _tfTrackingTarget.position;
			}
		}
		else
		{
			Target = null;
		}
	}

	protected void SetStar()
	{
		if ((bool)_tfStar)
		{
			if (_psStart == null)
			{
				_psStart = _tfStar.GetComponentInChildren<ParticleSystem>();
			}
			Velocity = Vector3.down * 3f / ((float)TrackingData.n_ENDTIME_2 * 0.001f);
			_tfStar.transform.localPosition = Vector3.up * 3f;
			if ((bool)_psStart)
			{
				_psStart.Play(true);
			}
		}
	}

	protected void FindTarget(TrackPriority trackPriority)
	{
		if (Target != null)
		{
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
	}

	protected virtual bool IsTargetWithinRange(IAimTarget aimTarget)
	{
		if (aimTarget != null)
		{
			return Vector2.Distance(MasterPosition, Target.AimTransform.position + Target.AimPoint) <= BulletData.f_DISTANCE;
		}
		return false;
	}

	protected override IEnumerator OnStartMove()
	{
		MonoBehaviourSingleton<UpdateManager>.Instance.AddLateUpdate(this);
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
			if (MonoBehaviourSingleton<OrangeGameManager>.Instance.IsGamePause)
			{
				yield return CoroutineDefine._waitForEndOfFrame;
				continue;
			}
			RunUpdate();
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

	public virtual void LateUpdateFunc()
	{
		if (_nPhase == BulletPhase.Normal && Target != null && (bool)_tfTrackingTarget)
		{
			_transform.position = _tfTrackingTarget.position + _vLocalPosition;
		}
	}

	protected void OnApplicationPause(bool isPause)
	{
		PauseGame(isPause);
	}

	protected void PauseGame(bool isPause)
	{
		if (isPause)
		{
			_clearTimer.TimerPause();
			_durationTimer.TimerPause();
		}
		else
		{
			_clearTimer.TimerPause();
			_durationTimer.TimerResume();
		}
	}

	public override void BackToPool()
	{
		MonoBehaviourSingleton<UpdateManager>.Instance.RemoveLateUpdate(this);
		base.BackToPool();
	}

	protected void RunUpdate()
	{
		switch (_nPhase)
		{
		case BulletPhase.Normal:
			PhaseNormal();
			break;
		case BulletPhase.NonParent:
			PhaseNonParent();
			break;
		case BulletPhase.Splash:
			PhaseSplash();
			break;
		}
	}

	protected void PhaseNormal()
	{
		MoveBullet();
		if (GameLogicUpdateManager.GameFrame >= trackingEndFrame)
		{
			_transform.SetParentNull();
			_transform.localRotation = Quaternion.identity;
			_nPhase = BulletPhase.NonParent;
		}
	}

	protected void PhaseNonParent()
	{
		MoveBullet();
		if (GameLogicUpdateManager.GameFrame >= trackingFreezeFrame)
		{
			EnableHitCollider();
			_durationTimer.TimerStart();
			if ((bool)_tfStar)
			{
				_tfStar.localPosition = Vector3.zero;
			}
			if ((bool)_psStart)
			{
				_psStart.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
			}
			_nPhase = BulletPhase.Splash;
		}
	}

	protected void PhaseSplash()
	{
	}

	protected void MoveBullet()
	{
		if (GameLogicUpdateManager.GameFrame < trackingFreezeFrame && (bool)_tfStar)
		{
			Vector3 translation = Velocity * Time.deltaTime;
			_tfStar.transform.Translate(translation);
		}
	}

	protected void EnableHitCollider()
	{
		_hitCollider.enabled = true;
		_clearTimer.TimerStart();
		_ignoreList.Clear();
		_hitCount.Clear();
	}
}
