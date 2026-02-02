using UnityEngine;

public class BerserkSwordBullet : CollideBullet
{
	private Vector3 _position = Vector3.zero;

	private Transform _beatenTarget;

	public override void Active(Vector3 pPos, Vector3 pDirection, LayerMask pTargetMask, IAimTarget pTarget = null)
	{
		_transform.position = pPos;
		if (_position != Vector3.zero)
		{
			_transform.position = _position;
		}
		else if ((bool)_beatenTarget)
		{
			StageObjParam component = _beatenTarget.transform.GetComponent<StageObjParam>();
			if ((bool)component)
			{
				_transform.position = component.tLinkSOB.AimPosition;
			}
			else
			{
				_transform.position = _beatenTarget.position;
			}
		}
		MonoBehaviourSingleton<FxManager>.Instance.UpdateFx(bulletFxArray, true);
		_rigidbody2D.WakeUp();
		Active(pTargetMask);
	}

	public override void Active(Transform pTransform, Vector3 pDirection, LayerMask pTargetMask, IAimTarget pTarget = null)
	{
		_transform.position = pTransform.position;
		if (_position != Vector3.zero)
		{
			_transform.position = _position;
		}
		else if ((bool)_beatenTarget)
		{
			StageObjParam component = _beatenTarget.transform.GetComponent<StageObjParam>();
			if ((bool)component)
			{
				_transform.position = component.tLinkSOB.AimPosition;
			}
			else
			{
				_transform.position = _beatenTarget.position;
			}
		}
		MonoBehaviourSingleton<FxManager>.Instance.UpdateFx(bulletFxArray, true);
		_rigidbody2D.WakeUp();
		Active(pTargetMask);
	}

	protected override void BeatenTarget(Transform bullet, Transform target)
	{
		_position = Vector3.zero;
		if ((bool)bullet && (bool)target)
		{
			Collider2D component = bullet.GetComponent<Collider2D>();
			if ((bool)component)
			{
				StageObjParam component2 = target.transform.GetComponent<StageObjParam>();
				if ((bool)component2)
				{
					_position = component.bounds.ClosestPoint(component2.tLinkSOB.AimPosition);
				}
				else
				{
					_position = component.bounds.ClosestPoint(target.position);
				}
				_transform.position = _position;
			}
		}
		_beatenTarget = target;
	}
}
