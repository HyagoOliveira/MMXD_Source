using UnityEngine;

public class BS050_BrickBullet : BasicBullet
{
	private TrailRenderer Trail;

	private int ActiveFrame;

	protected override void Awake()
	{
		base.Awake();
		Transform[] target = _transform.GetComponentsInChildren<Transform>(true);
		Trail = OrangeBattleUtility.FindChildRecursive(ref target, "Trail", true).gameObject.GetComponent<TrailRenderer>();
		Trail.enabled = false;
	}

	public override void Active(Transform pTransform, Vector3 pDirection, LayerMask pTargetMask, IAimTarget pTarget = null)
	{
		base.Active(pTransform, pDirection, pTargetMask, pTarget);
		ActiveFrame = GameLogicUpdateManager.GameFrame;
	}

	public override void Active(Vector3 pPos, Vector3 pDirection, LayerMask pTargetMask, IAimTarget pTarget = null)
	{
		base.Active(pPos, pDirection, pTargetMask, pTarget);
		ActiveFrame = GameLogicUpdateManager.GameFrame;
	}

	public override void LateUpdateFunc()
	{
		if (!ManagedSingleton<StageHelper>.Instance.bEnemyActive)
		{
			return;
		}
		if (GameLogicUpdateManager.GameFrame > ActiveFrame && !Trail.enabled)
		{
			Trail.enabled = true;
		}
		switch (Phase)
		{
		case BulletPhase.Normal:
		{
			UpdateExtraCollider();
			MoveBullet();
			float num = BulletData.f_DISTANCE;
			if (FreeDISTANCE > 0f)
			{
				num = FreeDISTANCE;
			}
			heapDistance += Vector2.Distance(lastPosition, _transform.position);
			lastPosition = _transform.position;
			if (heapDistance > num)
			{
				if (BulletData.n_ROLLBACK > 0)
				{
					Phase = BulletPhase.Boomerang;
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
			break;
		}
		case BulletPhase.Boomerang:
		{
			Vector3 vector = ((!(MasterTransform != _transform)) ? (-(_transform.position - MasterPosition).normalized) : (-(_transform.position - MasterTransform.position).normalized));
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
			float num2 = 0f;
			num2 = ((!(MasterTransform != _transform)) ? Vector2.Distance(_transform.position, MasterPosition) : Vector2.Distance(_transform.position, MasterTransform.position));
			if (num2 < 1f)
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
			break;
		}
		case BulletPhase.Splash:
			PhaseSplash();
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
			GenerateEndFx();
			Phase = BulletPhase.BackToPool;
			break;
		case BulletPhase.BackToPool:
			Stop();
			BackToPool();
			break;
		}
		if (activeTracking && TrackingData != null && ActivateTimer.GetMillisecond() >= TrackingData.n_BEGINTIME_1 && ActivateTimer.GetMillisecond() < TrackingData.n_ENDTIME_1)
		{
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
			if (Target != null)
			{
				DoAim(Target);
			}
		}
	}

	public override void BackToPool()
	{
		base.BackToPool();
		Trail.enabled = false;
	}
}
