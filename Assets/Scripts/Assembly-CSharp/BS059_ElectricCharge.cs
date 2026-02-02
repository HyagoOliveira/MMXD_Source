using UnityEngine;

public class BS059_ElectricCharge : BasicBullet
{
	protected override void MoveBullet()
	{
		if (_rigidbody2D != null)
		{
			_rigidbody2D.WakeUp();
		}
		oldPos = _transform.position;
		Vector3 translation = Velocity * Time.deltaTime;
		_transform.Translate(translation);
		switch (BulletData.n_SHOTLINE)
		{
		case 3:
		case 4:
			_transform.position += base.transform.up * amplitude * Mathf.Sin(omega * Time.fixedTime) * Time.timeScale;
			break;
		case 6:
			_transform.position += base.transform.up * amplitude * Mathf.Sin(omega * (Time.fixedTime - shootTime)) * Time.timeScale;
			break;
		}
		offset = translation.magnitude;
		_capsuleCollider.offset = new Vector2(0f, 0f);
		_capsuleCollider.size = new Vector2(0.8f, 2.4f);
	}
}
