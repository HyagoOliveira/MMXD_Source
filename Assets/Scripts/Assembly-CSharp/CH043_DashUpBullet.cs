#define RELEASE
using UnityEngine;

public class CH043_DashUpBullet : CollideBullet
{
	public override void Active(Transform pTransform, Vector3 pDirection, LayerMask pTargetMask, IAimTarget pTarget = null)
	{
		Debug.Log("NOPE");
		MonoBehaviourSingleton<FxManager>.Instance.UpdateFx(bulletFxArray, true);
		_rigidbody2D.WakeUp();
		Active(pTargetMask);
		_transform.SetParent(pTransform);
		_transform.localPosition = Vector3.zero;
	}
}
