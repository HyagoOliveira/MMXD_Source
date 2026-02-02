#define RELEASE
using UnityEngine;

public class RushCollideBullet : CollideBullet
{
	public override void Active(Vector3 pPos, Vector3 pDirection, LayerMask pTargetMask, IAimTarget pTarget = null)
	{
		Debug.LogWarning("發射這個子彈請用Transform");
		base.Active(pPos, pDirection, pTargetMask, pTarget);
		SyncInfoToOwner(pDirection, pTarget);
	}

	public override void Active(Transform pTransform, Vector3 pDirection, LayerMask pTargetMask, IAimTarget pTarget = null)
	{
		base.Active(pTransform, pDirection, pTargetMask, pTarget);
		_transform.SetParent(pTransform);
		_transform.localPosition = Vector3.zero;
		_transform.localRotation = Quaternion.identity;
		SyncInfoToOwner(pDirection, pTarget);
	}

	protected void SyncInfoToOwner(Vector3 direction, IAimTarget target)
	{
		if (refPBMShoter == null || refPBMShoter.SOB == null)
		{
			return;
		}
		OrangeCharacter orangeCharacter = refPBMShoter.SOB as OrangeCharacter;
		CharacterControlBase component = refPBMShoter.SOB.GetComponent<CharacterControlBase>();
		if (!(orangeCharacter == null) && !(component == null))
		{
			component.SetRushBullet(this);
			if (!orangeCharacter.IsLocalPlayer)
			{
				component.SyncSkillDirection(direction, target);
			}
		}
	}
}
