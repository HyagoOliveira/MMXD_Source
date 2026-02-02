using UnityEngine;

public class ShieldBullet : CollideBullet, IManagedLateUpdateBehavior
{
	[SerializeField]
	protected int nShieldBuffId;

	protected bool bWaitShieldSync;

	private bool bIsLocalPlayer = true;

	private bool bClearBulletByDisconnet = true;

	public override void Active(Transform pTransform, Vector3 pDirection, LayerMask pTargetMask, IAimTarget pTarget = null)
	{
		MonoBehaviourSingleton<UpdateManager>.Instance.AddLateUpdate(this);
		base.Active(pTransform, pDirection, pTargetMask, pTarget);
	}

	public override void Active(Vector3 pPos, Vector3 pDirection, LayerMask pTargetMask, IAimTarget pTarget = null)
	{
		MonoBehaviourSingleton<UpdateManager>.Instance.AddLateUpdate(this);
		base.Active(pPos, pDirection, pTargetMask, pTarget);
	}

	public void LateUpdateFunc()
	{
		if (refPBMShoter == null)
		{
			BackToPool();
		}
		else if (bWaitShieldSync)
		{
			if (refPBMShoter.CheckHasEffectByCONDITIONID(nShieldBuffId))
			{
				bWaitShieldSync = false;
			}
		}
		else if (!refPBMShoter.CheckHasEffectByCONDITIONID(nShieldBuffId))
		{
			BackToPool();
		}
	}

	public override void BackToPoolByDisconnet()
	{
		if (bClearBulletByDisconnet)
		{
			BackToPool();
		}
	}

	public override void BackToPool()
	{
		base.BackToPool();
		MonoBehaviourSingleton<UpdateManager>.Instance.RemoveLateUpdate(this);
		nShieldBuffId = 0;
	}

	public void BindBuffId(int buffId, bool isLocalPlayer, bool isClearBulletByDisconnet = true)
	{
		nShieldBuffId = buffId;
		bWaitShieldSync = !isLocalPlayer;
		bIsLocalPlayer = isLocalPlayer;
		bClearBulletByDisconnet = isClearBulletByDisconnet;
	}

	public override void Hit(Collider2D col)
	{
		if (!MonoBehaviourSingleton<StageSyncManager>.Instance.bPauseAllPlayerInput)
		{
			base.Hit(col);
		}
	}
}
