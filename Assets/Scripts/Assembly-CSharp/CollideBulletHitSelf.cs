using UnityEngine;

public class CollideBulletHitSelf : CollideBullet
{
	protected override void OnTriggerStay2D(Collider2D col)
	{
		if (!ManagedSingleton<StageHelper>.Instance.bEnemyActive || MonoBehaviourSingleton<OrangeGameManager>.Instance.IsGamePause)
		{
			return;
		}
		Transform root = _transform.root;
		if (!IsActivate || (root != MonoBehaviourSingleton<PoolManager>.Instance.transform && !col.transform.IsChildOf(root) && BulletData.n_TARGET != 3) || ((1 << col.gameObject.layer) & (int)TargetMask) == 0 || col.isTrigger)
		{
			return;
		}
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
			else if (col.gameObject.GetComponentInParent<StageHurtObj>() != null)
			{
				Hit(col);
			}
		}
	}
}
