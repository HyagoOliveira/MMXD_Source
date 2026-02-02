using UnityEngine;

public class CH062_XFieldInvisibleBullet : BasicBullet
{
	[SerializeField]
	private float _enableTriggerHitDelay;

	private float _triggerHitDelayTimer;

	public override void OnStart()
	{
		base.OnStart();
		_triggerHitDelayTimer = ((_enableTriggerHitDelay > 0f) ? _enableTriggerHitDelay : 0f);
	}

	public override void OnTriggerHit(Collider2D col)
	{
		if (!(_triggerHitDelayTimer > 0f))
		{
			base.OnTriggerHit(col);
		}
	}

	protected override void MoveBullet()
	{
		if (_triggerHitDelayTimer <= 0f)
		{
			base.MoveBullet();
		}
		else
		{
			_triggerHitDelayTimer -= Time.deltaTime;
		}
	}
}
