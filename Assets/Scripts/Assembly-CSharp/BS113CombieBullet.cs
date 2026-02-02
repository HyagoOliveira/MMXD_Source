using UnityEngine;

public class BS113CombieBullet : SeaHorseSkill04Bullet
{
	public Vector3 hitBulletPos;

	public bool isHitBullet;

	protected override void Awake()
	{
		base.Awake();
		_rigidbody2D = base.gameObject.AddOrGetComponent<Rigidbody2D>();
		_rigidbody2D.bodyType = RigidbodyType2D.Kinematic;
		_rigidbody2D.useFullKinematicContacts = true;
		HitBlockFX = "fxhit_bs113seahorse_hit_ground_m";
	}

	public override void Active(Transform pTransform, Vector3 pDirection, LayerMask pTargetMask, IAimTarget pTarget = null)
	{
		base.Active(pTransform, pDirection, pTargetMask, pTarget);
		UseMask = (int)UseMask | (int)BulletScriptableObjectInstance.BulletLayerMaskBullet;
		isHitBullet = false;
		hitBulletPos = Vector3.zero;
	}

	public override void Active(Vector3 pPos, Vector3 pDirection, LayerMask pTargetMask, IAimTarget pTarget = null)
	{
		base.Active(pPos, pDirection, pTargetMask, pTarget);
		UseMask = (int)UseMask | (int)BulletScriptableObjectInstance.BulletLayerMaskBullet;
		isHitBullet = false;
		hitBulletPos = Vector3.zero;
	}

	public override void OnTriggerHit(Collider2D col)
	{
		if (bIsEnd || !ManagedSingleton<StageHelper>.Instance.bEnemyActive || MonoBehaviourSingleton<OrangeGameManager>.Instance.IsGamePause || col.isTrigger || ((1 << col.gameObject.layer) & (int)UseMask) == 0 || (((uint)BulletData.n_FLAG & (true ? 1u : 0u)) != 0 && ((1 << col.gameObject.layer) & (int)BlockMask) != 0 && !col.GetComponent<StageHurtObj>()))
		{
			return;
		}
		BS113CombieBullet component = col.GetComponent<BS113CombieBullet>();
		if (component != null)
		{
			if (component.gameObject.GetInstanceID() != base.gameObject.GetInstanceID() && !component.isHitBullet)
			{
				if (isHitBullet)
				{
					BackToPool();
				}
				else if (!(component.refPBMShoter.SOB == null) && !(refPBMShoter.SOB == null) && !(component.refPBMShoter.SOB.sNetSerialID == refPBMShoter.SOB.sNetSerialID))
				{
					isHitBullet = (component.isHitBullet = true);
					hitBulletPos = component._transform.position;
					component.hitBulletPos = _transform.position;
					BackToPool();
					component.BackToPool();
				}
			}
			return;
		}
		StageObjParam component2 = col.GetComponent<StageObjParam>();
		if (component2 != null && component2.tLinkSOB != null)
		{
			if (((1 << col.gameObject.layer) & (int)BlockMask) == 0 && (int)component2.tLinkSOB.Hp > 0)
			{
				Hit(col);
			}
		}
		else if ((BulletData.n_FLAG & 0x1000) == 0 || ((1 << col.gameObject.layer) & (int)BlockMask) == 0 || !col.GetComponent<StageHurtObj>())
		{
			Hit(col);
		}
	}
}
