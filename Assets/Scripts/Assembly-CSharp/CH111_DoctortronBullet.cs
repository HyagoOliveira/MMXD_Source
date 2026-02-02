using UnityEngine;

public class CH111_DoctortronBullet : BasicBullet
{
	public override void Active(Vector3 pPos, Vector3 pDirection, LayerMask pTargetMask, IAimTarget pTarget = null)
	{
		base.Active(pPos, pDirection, pTargetMask, pTarget);
		UpdateRotation();
	}

	public override void Active(Transform pTransform, Vector3 pDirection, LayerMask pTargetMask, IAimTarget pTarget = null)
	{
		base.Active(pTransform, pDirection, pTargetMask, pTarget);
		UpdateRotation();
	}

	private void UpdateRotation()
	{
		float num = 0f;
		float y = _transform.localEulerAngles.y;
		float num2 = _transform.localEulerAngles.z;
		if (refPBMShoter != null && refPBMShoter.SOB != null)
		{
			num = ((refPBMShoter.SOB._characterDirection == CharacterDirection.LEFT) ? 180 : 0);
		}
		else if (num2 > 90f && num2 < 270f)
		{
			num = 180f;
		}
		if (num == 180f)
		{
			num2 *= -1f;
		}
		_transform.localEulerAngles = new Vector3(num, y, num2);
	}
}
