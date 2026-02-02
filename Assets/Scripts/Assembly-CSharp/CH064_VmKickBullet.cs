using System;
using UnityEngine;

public class CH064_VmKickBullet : BasicBullet
{
	[SerializeField]
	protected Transform _tfVMKick;

	[SerializeField]
	protected Transform _tfFBX;

	[SerializeField]
	protected Transform _tfFX;

	[SerializeField]
	protected Transform _pSubCollider;

	protected Vector3 _vVMKickRotRight = new Vector3(0f, 90f, 0f);

	protected Vector3 _vVMKickRotLeft = new Vector3(0f, -90f, -180f);

	public override void Active(Vector3 pPos, Vector3 pDirection, LayerMask pTargetMask, IAimTarget pTarget = null)
	{
		base.Active(pPos, pDirection, pTargetMask, pTarget);
		UpdateDirection(pDirection);
	}

	public override void Active(Transform pTransform, Vector3 pDirection, LayerMask pTargetMask, IAimTarget pTarget = null)
	{
		base.Active(pTransform, pDirection, pTargetMask, pTarget);
		UpdateDirection(pDirection);
	}

	protected void UpdateDirection(Vector3 pDirection)
	{
		if (pDirection.x > 0f)
		{
			_capsuleCollider.offset = new Vector2(0f, DefaultRadiusY);
			_tfVMKick.localEulerAngles = _vVMKickRotRight;
			_tfFBX.localEulerAngles = new Vector3(_tfFBX.localEulerAngles.x, _tfFBX.localEulerAngles.y, Mathf.Abs(_tfFBX.localEulerAngles.z));
			_tfFX.localPosition = new Vector3(_tfFX.localPosition.x, _tfFX.localPosition.y, Mathf.Abs(_tfFX.localPosition.z) * -1f);
			_pSubCollider.localEulerAngles = Vector3.zero;
		}
		else
		{
			_capsuleCollider.offset = new Vector2(0f, 0f - DefaultRadiusY);
			_tfVMKick.localEulerAngles = _vVMKickRotLeft;
			_tfFBX.localEulerAngles = new Vector3(_tfFBX.localEulerAngles.x, _tfFBX.localEulerAngles.y, Mathf.Abs(_tfFBX.localEulerAngles.z) * -1f);
			_tfFX.localPosition = new Vector3(_tfFX.localPosition.x, _tfFX.localPosition.y, Mathf.Abs(_tfFX.localPosition.z));
			_pSubCollider.localEulerAngles = new Vector3(0f, 0f, -180f);
		}
	}

	protected override void GenerateImpactFx(bool bPlaySE = true)
	{
		Quaternion identity = Quaternion.identity;
		RaycastHit2D raycastHit2D = Physics2D.Raycast(oldPos, Direction, offset);
		if ((bool)raycastHit2D)
		{
			_transform.position = raycastHit2D.point;
			MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FxImpact, raycastHit2D.point + new Vector2(0f, 1.5f), identity * BulletQuaternion, Array.Empty<object>());
		}
		else
		{
			MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FxImpact, _transform.position + new Vector3(0f, 1.5f, 0f), identity * BulletQuaternion, Array.Empty<object>());
		}
		if (isHitBlock || needPlayEndSE || needWeaponImpactSE)
		{
			PlaySE(_HitGuardSE[0], _HitGuardSE[1], isForceSE);
		}
	}
}
