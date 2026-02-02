using UnityEngine;

public class CH075_BouquetBullet : BasicBullet
{
	[SerializeField]
	private Transform _tfWork;

	public override void Active(Vector3 pPos, Vector3 pDirection, LayerMask pTargetMask, IAimTarget pTarget = null)
	{
		base.Active(pPos, pDirection, pTargetMask, pTarget);
		_tfWork.localEulerAngles = new Vector3(_tfWork.localEulerAngles.x, _tfWork.localEulerAngles.y, _transform.localEulerAngles.z);
	}

	public override void Active(Transform pTransform, Vector3 pDirection, LayerMask pTargetMask, IAimTarget pTarget = null)
	{
		base.Active(pTransform, pDirection, pTargetMask, pTarget);
		_tfWork.localEulerAngles = new Vector3(_tfWork.localEulerAngles.x, _tfWork.localEulerAngles.y, _transform.localEulerAngles.z);
	}
}
