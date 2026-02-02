using UnityEngine;

public class AdhesionBullet : BasicBullet
{
	[SerializeField]
	private float AdhesionWallTime = 4f;

	private int AdhesionFrame;

	[SerializeField]
	private bool NeedAdhesionWall;

	private bool hasAdhesionWall;

	[SerializeField]
	private string[] StopSE;

	[SerializeField]
	private ParticleSystem[] NeedStopFx;

	[SerializeField]
	private ParticleSystem[] NeedPauseFx;

	public override void OnTriggerHit(Collider2D col)
	{
		if (!ManagedSingleton<StageHelper>.Instance.bEnemyActive || MonoBehaviourSingleton<OrangeGameManager>.Instance.IsGamePause || col.isTrigger || ((1 << col.gameObject.layer) & (int)UseMask) == 0)
		{
			return;
		}
		if (((uint)BulletData.n_FLAG & (true ? 1u : 0u)) != 0 && ((1 << col.gameObject.layer) & (int)BlockMask) != 0)
		{
			AdhesionWall();
			if (!hasAdhesionWall && !col.GetComponent<StageHurtObj>())
			{
				return;
			}
		}
		StageObjParam component = col.GetComponent<StageObjParam>();
		if (component != null && component.tLinkSOB != null)
		{
			if (((1 << col.gameObject.layer) & (int)BlockMask) == 0 && (int)component.tLinkSOB.Hp > 0)
			{
				Hit(col);
			}
		}
		else
		{
			Hit(col);
		}
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
				AdhesionWall();
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
				else if (!hasAdhesionWall)
				{
					Phase = BulletPhase.Result;
				}
			}
			else if (!hasAdhesionWall)
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

	private void AdhesionWall()
	{
		if (NeedAdhesionWall && !hasAdhesionWall)
		{
			ParticleSystem[] needPauseFx = NeedPauseFx;
			for (int i = 0; i < needPauseFx.Length; i++)
			{
				needPauseFx[i].Pause();
			}
			needPauseFx = NeedStopFx;
			foreach (ParticleSystem obj in needPauseFx)
			{
				obj.Clear();
				obj.Stop();
			}
			Velocity = Vector3.zero;
			hasAdhesionWall = true;
			AdhesionFrame = GameLogicUpdateManager.GameFrame + (int)(AdhesionWallTime * 20f);
			if (StopSE != null && StopSE.Length == 2)
			{
				PlaySE(StopSE[0], StopSE[1]);
			}
		}
	}

	public override void LateUpdateFunc()
	{
		if (hasAdhesionWall && GameLogicUpdateManager.GameFrame >= AdhesionFrame && Phase == BulletPhase.Normal)
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
		base.LateUpdateFunc();
	}

	public void SetResultPhase()
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

	public override void BackToPool()
	{
		hasAdhesionWall = false;
		AdhesionFrame = 0;
		base.BackToPool();
	}
}
