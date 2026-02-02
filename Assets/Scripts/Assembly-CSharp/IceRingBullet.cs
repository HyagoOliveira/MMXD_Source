public class IceRingBullet : LogicBasicBullet
{
	protected override void MoveBullet()
	{
	}

	public void EnableColider(bool enable)
	{
		if ((bool)_hitCollider)
		{
			_hitCollider.enabled = enable;
		}
	}
}
