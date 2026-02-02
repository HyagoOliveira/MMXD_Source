using UnityEngine;

public class WaveBullet : BasicBullet
{
	[Header("透過 週期、振幅決定子彈路線波形狀")]
	[SerializeField]
	[Tooltip("週期")]
	private float Cycle;

	[SerializeField]
	[Tooltip("振幅")]
	private float Amplitude;

	[SerializeField]
	[Tooltip("先左或右")]
	private bool LeftFirst = true;

	[SerializeField]
	private float SpeedMulti = 0.4f;

	private bool nextLeft;

	private Vector3 ShootDir;

	private Vector3 centerPos;

	private float moveDis;

	private Vector3 moveVec;

	private Vector3 WaveVector;

	private float RotateAngle;

	private bool needRotateBullet;

	public override void Active(Vector3 pPos, Vector3 pDirection, LayerMask pTargetMask, IAimTarget pTarget = null)
	{
		base.Active(pPos, pDirection, pTargetMask, pTarget);
		ShootDir = pDirection;
		centerPos = pPos;
		WaveVector = new Vector3(Mathf.Abs(Cycle / 4f), Mathf.Abs(Amplitude), 0f);
		RotateAngle = Vector2.Angle(Vector2.right, WaveVector);
		RotateBullet(LeftFirst);
		Velocity *= SpeedMulti;
	}

	public override void Active(Transform pTransform, Vector3 pDirection, LayerMask pTargetMask, IAimTarget pTarget = null)
	{
		base.Active(pTransform, pDirection, pTargetMask, pTarget);
		ShootDir = pDirection;
		centerPos = pTransform.position;
		WaveVector = new Vector3(Mathf.Abs(Cycle / 4f), Mathf.Abs(Amplitude), 0f);
		RotateAngle = Vector2.Angle(Vector2.right, WaveVector);
		RotateBullet(LeftFirst);
		Velocity *= SpeedMulti;
	}

	private void RotateBullet(bool isLeft)
	{
		Vector3 right = Vector3.right;
		if (isLeft)
		{
			nextLeft = false;
			right = Quaternion.Euler(0f, 0f, 0f - RotateAngle) * ShootDir;
		}
		else
		{
			nextLeft = true;
			right = Quaternion.Euler(0f, 0f, 0f - RotateAngle) * ShootDir;
		}
		if (needRotateBullet)
		{
			if (!isMirror)
			{
				_transform.eulerAngles = new Vector3(0f, 0f, Vector2.SignedAngle(Vector2.right, right));
			}
			else
			{
				float num = Vector2.SignedAngle(Vector2.right, right);
				_transform.eulerAngles = new Vector3(0f, 0f, Vector2.SignedAngle(Vector2.right, right));
				if (num < 0f)
				{
					num += 360f;
				}
				if (num > 90f && num < 270f)
				{
					_transform.localScale = new Vector3(1f, -1f, 1f);
				}
				else
				{
					_transform.localScale = Vector3.one;
				}
			}
		}
		Direction = right;
	}

	protected override void PhaseNormal()
	{
		UpdateExtraCollider();
		if (!hasReflect)
		{
			BulletReflect();
		}
		MoveBullet();
		float num = BulletData.f_DISTANCE;
		if (FreeDISTANCE > 0f)
		{
			num = FreeDISTANCE;
		}
		heapDistance += Vector2.Distance(lastPosition, _transform.position);
		lastPosition = _transform.position;
		if (heapDistance > num)
		{
			CheckRollBack();
		}
	}

	protected override void MoveBullet()
	{
		if (_rigidbody2D != null)
		{
			_rigidbody2D.WakeUp();
		}
		oldPos = _transform.position;
		Vector3 translation = Velocity * Time.deltaTime;
		_transform.Translate(translation);
		int n_SHOTLINE = BulletData.n_SHOTLINE;
		int num4 = 6;
		lineDistance += Vector2.Distance(lastPosition, _transform.position);
		centerPos += ShootDir.normalized * Vector2.Distance(lastPosition, _transform.position);
		float num = lineDistance % Cycle / (Cycle / 4f);
		int num2 = (int)num;
		float num3 = num % 1f;
		switch (num2)
		{
		case 0:
			moveVec = new Vector3(0f, num3 * Amplitude, 0f);
			_transform.position = centerPos + Quaternion.Euler(0f, 0f, Vector2.SignedAngle(Vector2.right, ShootDir)) * moveVec;
			break;
		case 1:
			moveVec = new Vector3(0f, (1f - num3) * Amplitude, 0f);
			_transform.position = centerPos + Quaternion.Euler(0f, 0f, Vector2.SignedAngle(Vector2.right, ShootDir)) * moveVec;
			break;
		case 2:
			moveVec = new Vector3(0f, num3 * (0f - Amplitude), 0f);
			_transform.position = centerPos + Quaternion.Euler(0f, 0f, Vector2.SignedAngle(Vector2.right, ShootDir)) * moveVec;
			break;
		case 3:
			moveVec = new Vector3(0f, (1f - num3) * (0f - Amplitude), 0f);
			_transform.position = centerPos + Quaternion.Euler(0f, 0f, Vector2.SignedAngle(Vector2.right, ShootDir)) * moveVec;
			break;
		}
		offset = Mathf.Max(translation.magnitude, DefaultRadiusX * 2f);
		_colliderOffset.x = (0f - offset) / 2f;
		_colliderSize.x = offset;
		_capsuleCollider.size = _colliderSize;
	}
}
