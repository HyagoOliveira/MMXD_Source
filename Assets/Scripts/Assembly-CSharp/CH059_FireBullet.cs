using System.Collections;

public class CH059_FireBullet : CollideBullet
{
	public override void BackToPool()
	{
		IsDestroy = false;
		IsActivate = false;
		_hitCount.Clear();
		if ((bool)_hitCollider)
		{
			_hitCollider.enabled = false;
		}
		MonoBehaviourSingleton<FxManager>.Instance.UpdateFx(bulletFxArray, false);
		_rigidbody2D.Sleep();
		isBuffTrigger = false;
		if (_UseSE != null && _UseSE[2] != "")
		{
			base.SoundSource.PlaySE(_UseSE[0], _UseSE[2]);
		}
		Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_BULLET_UNREGISTER, this);
		StartCoroutine(WaitParticleVanish());
	}

	private IEnumerator WaitParticleVanish()
	{
		yield return CoroutineDefine._1sec;
		if (bNeedBackPoolColliderBullet)
		{
			MonoBehaviourSingleton<PoolManager>.Instance.BackToPool(this, "PoolColliderBullet");
		}
		else if (bNeedBackPoolModelName)
		{
			MonoBehaviourSingleton<PoolManager>.Instance.BackToPool(this, itemName);
		}
	}
}
