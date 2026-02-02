using UnityEngine;

public class BS105Skill0Bullet : BasicBullet
{
	private float MoveDistance;

	private Vector3 centerPos;

	private float Amplitude;

	public override void Active(Vector3 pPos, Vector3 pDirection, LayerMask pTargetMask, IAimTarget pTarget = null)
	{
		base.Active(pPos, pDirection, pTargetMask, pTarget);
		centerPos = pPos;
		MoveDistance = 0f;
		Amplitude = 0f;
	}

	public override void Active(Transform pTransform, Vector3 pDirection, LayerMask pTargetMask, IAimTarget pTarget = null)
	{
		base.Active(pTransform, pDirection, pTargetMask, pTarget);
		centerPos = pTransform.position;
		MoveDistance = 0f;
		Amplitude = 0f;
	}

	protected override void MoveBullet()
	{
		if (_rigidbody2D != null)
		{
			_rigidbody2D.WakeUp();
		}
		oldPos = _transform.position;
		float deltaTime = Time.deltaTime;
		Vector3 translation = Velocity * deltaTime;
		if (Mathf.Abs(MoveDistance) < Mathf.Abs(Amplitude))
		{
			MoveDistance += Amplitude * deltaTime * 8f;
		}
		else
		{
			MoveDistance = Amplitude;
		}
		_transform.Translate(translation);
		lineDistance += Vector2.Distance(lastPosition, _transform.position);
		centerPos += Direction.normalized * Vector2.Distance(lastPosition, _transform.position);
		_transform.position = centerPos + Quaternion.Euler(0f, 0f, Vector2.SignedAngle(Vector2.right, Direction)) * Vector3.up * MoveDistance;
		offset = Mathf.Max(translation.magnitude, DefaultRadiusX * 2f);
		_colliderOffset.x = (0f - offset) / 2f;
		_colliderSize.x = offset;
		_capsuleCollider.size = _colliderSize;
	}

	public void SetAmplitude(float amplitude)
	{
		Amplitude = amplitude;
		base.amplitude = amplitude;
	}
}
