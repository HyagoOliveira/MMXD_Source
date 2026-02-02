using UnityEngine;

public class EnergyBullet : BasicBullet
{
	private bool _chargeOver;

	[SerializeField]
	private bool AutoCharge;

	[SerializeField]
	private float AutoChargeTime;

	private int AutoShootFrame;

	public override void Active(Transform pTransform, Vector3 pDirection, LayerMask pTargetMask, IAimTarget pTarget = null)
	{
		if (AutoCharge)
		{
			AutoShootFrame = GameLogicUpdateManager.GameFrame + (int)(AutoChargeTime * 20f);
		}
		base.Active(pTransform, pDirection, pTargetMask, pTarget);
	}

	public override void Active(Vector3 pPos, Vector3 pDirection, LayerMask pTargetMask, IAimTarget pTarget = null)
	{
		if (AutoCharge)
		{
			AutoShootFrame = GameLogicUpdateManager.GameFrame + (int)(AutoChargeTime * 20f);
		}
		base.Active(pPos, pDirection, pTargetMask, pTarget);
	}

	protected override void MoveBullet()
	{
		if (!_chargeOver && AutoCharge && GameLogicUpdateManager.GameFrame > AutoShootFrame)
		{
			StartShoot();
		}
		if (_chargeOver)
		{
			base.MoveBullet();
		}
	}

	public override void BackToPool()
	{
		_chargeOver = false;
		base.BackToPool();
	}

	public void GoBack()
	{
		if (!bIsEnd)
		{
			Phase = BulletPhase.BackToPool;
		}
	}

	public void StartShoot(bool canshot = true)
	{
		if (!bIsEnd)
		{
			_chargeOver = canshot;
		}
	}

	public void SetBackToPool()
	{
		Stop();
		BackToPool();
	}
}
