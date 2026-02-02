using UnityEngine;

public class CH132_GhostAttack : BasicBullet
{
	[SerializeField]
	protected TrailRenderer[] _trFxs;

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
		TrailRenderer[] trFxs = _trFxs;
		foreach (TrailRenderer obj in trFxs)
		{
			obj.emitting = true;
			obj.enabled = true;
		}
	}

	private void SetEmittingOff()
	{
		TrailRenderer[] trFxs = _trFxs;
		foreach (TrailRenderer obj in trFxs)
		{
			obj.emitting = false;
			obj.enabled = false;
		}
	}

	public override void BackToPool()
	{
		LeanTween.cancel(base.gameObject);
		SetEmittingOff();
		base.BackToPool();
	}
}
