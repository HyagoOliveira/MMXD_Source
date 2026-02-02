using UnityEngine;

public class BS051_FireBall : ParabolaBulletForTryShoot
{
	private EM113_Controller Kerosene;

	private bool _isHitSpecTarget;

	public override void OnTriggerHit(Collider2D col)
	{
		if (!ManagedSingleton<StageHelper>.Instance.bEnemyActive || MonoBehaviourSingleton<OrangeGameManager>.Instance.IsGamePause)
		{
			return;
		}
		_isHitSpecTarget = false;
		Kerosene = col.gameObject.GetComponent<EM113_Controller>();
		if ((bool)Kerosene && !Kerosene._isBurning)
		{
			_isHitSpecTarget = true;
		}
		if (((col.isTrigger || ((1 << col.gameObject.layer) & (int)UseMask) == 0) && !_isHitSpecTarget) || (((uint)BulletData.n_FLAG & (true ? 1u : 0u)) != 0 && ((1 << col.gameObject.layer) & (int)BlockMask) != 0 && !col.GetComponent<StageHurtObj>()))
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
		else
		{
			Hit(col);
		}
	}
}
