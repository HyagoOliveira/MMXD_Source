using UnityEngine;

public class CH043_2ndChargeBuster : LogicBasicBullet
{
	protected override void DoActive(IAimTarget pTarget)
	{
		base.DoActive(pTarget);
		UseMask = (int)UseMask | (int)BulletScriptableObjectInstance.BulletLayerMaskBullet;
	}

	public override void LogicUpdate()
	{
		base.LogicUpdate();
		if (GetBulletFlagByPerGameSaveData(1))
		{
			BackToPool();
		}
	}

	public override void OnTriggerHit(Collider2D col)
	{
		if (!ManagedSingleton<StageHelper>.Instance.bEnemyActive || MonoBehaviourSingleton<OrangeGameManager>.Instance.IsGamePause || col.isTrigger || ((1 << col.gameObject.layer) & (int)UseMask) == 0 || (((uint)BulletData.n_FLAG & (true ? 1u : 0u)) != 0 && ((1 << col.gameObject.layer) & (int)BlockMask) != 0 && !col.GetComponent<StageHurtObj>()))
		{
			return;
		}
		CH043_1stChargeBuster component = col.GetComponent<CH043_1stChargeBuster>();
		if (component != null)
		{
			if (!(component.refPBMShoter.SOB == null) && !(refPBMShoter.SOB == null) && component.refPBMShoter.SOB.sNetSerialID == refPBMShoter.SOB.sNetSerialID && refPBMShoter.SOB.sNetSerialID == MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify)
			{
				SendBulletFlagMsg(1);
				component.SendBulletFlagMsg(2);
				component.ChangeCrossShot();
				BackToPool();
			}
			return;
		}
		StageObjParam component2 = col.GetComponent<StageObjParam>();
		if (component2 != null && component2.tLinkSOB != null)
		{
			if ((int)component2.tLinkSOB.Hp > 0)
			{
				Hit(col);
			}
		}
		else
		{
			Hit(col);
		}
	}
}
