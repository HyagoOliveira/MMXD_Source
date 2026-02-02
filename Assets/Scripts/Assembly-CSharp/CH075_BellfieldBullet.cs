using UnityEngine;

public class CH075_BellfieldBullet : CollideBullet
{
	[SerializeField]
	private Transform _tfWork;

	public override void Active(Vector3 pPos, Vector3 pDirection, LayerMask pTargetMask, IAimTarget pTarget = null)
	{
		base.Active(pPos, pDirection, pTargetMask, pTarget);
		_tfWork.localRotation = _transform.localRotation;
	}

	public override void Active(Transform pTransform, Vector3 pDirection, LayerMask pTargetMask, IAimTarget pTarget = null)
	{
		base.Active(pTransform, pDirection, pTargetMask, pTarget);
		_tfWork.localRotation = _transform.localRotation;
	}
}
