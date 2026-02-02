using System;
using UnityEngine;

public class PoolSimple : PoolBaseObject
{
	public override void BackToPool()
	{
		MonoBehaviourSingleton<PoolManager>.Instance.BackToPool(this, "simpleobject");
	}

	public void DosomeThing()
	{
		base.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
		LeanTween.moveX(base.gameObject, UnityEngine.Random.Range(-9.99f, 9.99f), 5f).setOnComplete((Action)delegate
		{
			BackToPool();
		});
	}
}
