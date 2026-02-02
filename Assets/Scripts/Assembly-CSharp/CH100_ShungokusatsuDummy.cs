#define RELEASE
using UnityEngine;

public class CH100_ShungokusatsuDummy : CollideBullet
{
	private OrangeCharacter _player;

	private CH100_Controller _owner;

	public override void Active(Vector3 pPos, Vector3 pDirection, LayerMask pTargetMask, IAimTarget pTarget = null)
	{
		Debug.LogWarning("射發這個子彈請用Transform");
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
		if (refPBMShoter != null && !(refPBMShoter.SOB == null))
		{
			_player = refPBMShoter.SOB as OrangeCharacter;
			_owner = refPBMShoter.SOB.GetComponent<CH100_Controller>();
			if (!(_player == null) && !(_owner == null) && !_player.IsLocalPlayer)
			{
				_owner.SyncSkillDirection(direction, target);
			}
		}
	}

	protected override void OnTriggerStay2D(Collider2D col)
	{
		if (!(_player == null) && _player.CurMainStatus == OrangeCharacter.MainStatus.SKILL && _player.CurSubStatus == OrangeCharacter.SubStatus.SKILL1_1)
		{
			base.OnTriggerStay2D(col);
		}
	}

	public override void BackToPool()
	{
		base.BackToPool();
		_player = null;
		_owner = null;
		MonoBehaviourSingleton<PoolManager>.Instance.BackToPool((PoolBaseObject)this, itemName);
	}
}
