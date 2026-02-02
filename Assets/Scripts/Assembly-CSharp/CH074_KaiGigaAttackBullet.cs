using UnityEngine;

public class CH074_KaiGigaAttackBullet : BasicBullet
{
	[SerializeField]
	protected TrailRenderer _trFx;

	[SerializeField]
	protected float delayEmitting;

	public override void OnStart()
	{
		base.OnStart();
		if (delayEmitting > 0f)
		{
			LeanTween.delayedCall(base.gameObject, delayEmitting, SetEmittingOn);
		}
		else
		{
			SetEmittingOn();
		}
	}

	private void SetEmittingOn()
	{
		_trFx.emitting = true;
		_trFx.enabled = true;
	}

	private void SetEmittingOff()
	{
		_trFx.emitting = false;
		_trFx.enabled = false;
	}

	public override void BackToPool()
	{
		LeanTween.cancel(base.gameObject);
		SetEmittingOff();
		base.BackToPool();
	}
}
