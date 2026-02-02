using UnityEngine;

public class CH126_LandBullet : BasicBullet
{
	[SerializeField]
	private Transform transformFx;

	[SerializeField]
	private float offsetY = 1.5f;

	[SerializeField]
	private float offsetX = -1f;

	public override void Active(Transform pTransform, Vector3 pDirection, LayerMask pTargetMask, IAimTarget pTarget = null)
	{
		base.Active(pTransform, pDirection, pTargetMask, pTarget);
		base.transform.localScale = new Vector3(1f, 1f, 1f);
		UpdateFxOffset();
	}

	public override void Active(Vector3 pPos, Vector3 pDirection, LayerMask pTargetMask, IAimTarget pTarget = null)
	{
		base.Active(pPos, pDirection, pTargetMask, pTarget);
		base.transform.localScale = new Vector3(1f, 1f, 1f);
		UpdateFxOffset();
	}

	private void UpdateFxOffset()
	{
		if (Direction.x > 0f)
		{
			transformFx.transform.localPosition = new Vector3(offsetX, 0f - offsetY, 0f);
			_capsuleCollider.offset = new Vector2(offsetX, offsetY);
		}
		else
		{
			transformFx.transform.localPosition = new Vector3(offsetX, offsetY, 0f);
			_capsuleCollider.offset = new Vector2(offsetX, 0f - offsetY);
		}
		transformFx.localRotation = Quaternion.identity;
	}
}
