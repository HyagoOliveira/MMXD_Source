using UnityEngine;

public class CH043_3rdChargeBuster : LogicBasicBullet
{
	[SerializeField]
	protected float _initVelocity = 10f;

	[SerializeField]
	protected float _acceleration;

	protected float _maxSpeed = 30f;

	protected float _currentSpeed;

	protected float _lastFrameTime;

	protected override void DoActive(IAimTarget pTarget)
	{
		base.DoActive(pTarget);
		_currentSpeed = _initVelocity;
		_maxSpeed = BulletData.n_SPEED;
		CaluLogicFrame(_currentSpeed, BulletData.f_DISTANCE, Direction);
		_lastFrameTime = Time.fixedTime;
	}

	protected override void MoveBullet()
	{
		_nowLogicFrame++;
		if (_nowLogicFrame > _endLogicFrame)
		{
			mainPhase = BulletPhase.End;
			return;
		}
		float num = (Time.fixedTime - _lastFrameTime) * 0.34f / GameLogicUpdateManager.m_fFrameLen;
		_currentSpeed = Mathf.Clamp(_currentSpeed + _acceleration * num, 0.1f, _maxSpeed);
		CaluLogicFrame(_currentSpeed, BulletData.f_DISTANCE - _moveDistance, Direction);
		_nowPos += new VInt3(_speed * _timeDelta.scalar);
		float num2 = Vector3.Distance(base.transform.localPosition, _nowPos.vec3);
		_distanceDelta = num2 * (Time.deltaTime / GameLogicUpdateManager.m_fFrameLen);
		_moveDistance += num2;
		_lastFrameTime = Time.fixedTime;
	}
}
