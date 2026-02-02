using UnityEngine;

public class CH043_subBuster : LogicBasicBullet
{
	[SerializeField]
	private ParticleSystem _ballYellow;

	[SerializeField]
	private ParticleSystem _ballGreen;

	protected bool _goBack = true;

	protected float _angle = 10f;

	protected Vector3 _shotDirection;

	protected bool _checkDirection = true;

	[SerializeField]
	private float _goBackDistance = 3f;

	[SerializeField]
	private float _goBackTime = 1f;

	private float _timeStart;

	private float _timeEnd;

	private Vector3 _startPos;

	[SerializeField]
	protected float _initVelocity = 1f;

	[SerializeField]
	protected float _acceleration;

	protected float _maxSpeed = 30f;

	protected float _currentSpeed;

	protected float _lastFrameTime;

	public void SetParam(float angle)
	{
		_angle = angle;
		if (_angle > 0f)
		{
			_ballYellow.Play();
			_ballGreen.Stop();
		}
		else
		{
			_ballYellow.Stop();
			_ballGreen.Play();
		}
		_goBack = true;
		_timeStart = Time.time;
		_timeEnd = _timeStart + _goBackTime;
		_startPos = _transform.localPosition;
		_shotDirection = Quaternion.Euler(0f, 0f, _angle * -1f) * Direction * -1f;
	}

	protected override void DoActive(IAimTarget pTarget)
	{
		base.DoActive(pTarget);
		_maxSpeed = BulletData.n_SPEED;
		_lastFrameTime = Time.fixedTime;
	}

	protected override void MoveBullet()
	{
		if (_goBack)
		{
			MoveGoBack();
		}
		else if (CheckTracking())
		{
			MoveTracking();
		}
		else
		{
			MoveGoForward();
		}
		_lastPosition = _transform.localPosition;
	}

	public override void BackToPool()
	{
		base.BackToPool();
		_currentSpeed = 0f;
		_checkDirection = true;
	}

	protected void MoveGoBack()
	{
		float num3 = (Time.fixedTime - _lastFrameTime) * 0.34f / GameLogicUpdateManager.m_fFrameLen;
		float value = Mathf.InverseLerp(_timeStart, _timeEnd, Time.time);
		float num = EaseOutCubic(0f, 1f, value);
		_nowPos = new VInt3(_startPos + _shotDirection * num * _goBackDistance);
		float num2 = Vector3.Distance(base.transform.localPosition, _nowPos.vec3);
		_distanceDelta = num2 * (Time.deltaTime / GameLogicUpdateManager.m_fFrameLen);
		if (num == 1f)
		{
			_goBack = false;
			_currentSpeed = _initVelocity;
			_shotDirection = Quaternion.Euler(0f, 0f, _angle * 0.5f) * Direction;
			_distanceDelta = 0.01f;
			_endLogicFrame = _nowLogicFrame + 10;
		}
		_lastFrameTime = Time.fixedTime;
	}

	protected void MoveGoForward()
	{
		_nowLogicFrame++;
		if (_nowLogicFrame > _endLogicFrame)
		{
			mainPhase = BulletPhase.End;
			return;
		}
		float num = (Time.fixedTime - _lastFrameTime) * 0.34f / GameLogicUpdateManager.m_fFrameLen;
		_currentSpeed = Mathf.Clamp(_currentSpeed + _acceleration * num, 0.1f, _maxSpeed);
		CaluLogicFrame(_currentSpeed, BulletData.f_DISTANCE - _moveDistance, _shotDirection);
		_nowPos += new VInt3(_speed * _timeDelta.scalar);
		float num2 = Vector3.Distance(base.transform.localPosition, _nowPos.vec3);
		_distanceDelta = num2 * (Time.deltaTime / GameLogicUpdateManager.m_fFrameLen);
		_moveDistance += num2;
		_lastFrameTime = Time.fixedTime;
	}

	protected void MoveTracking()
	{
		_moveDistance += Vector3.Distance(_lastPosition, _transform.localPosition);
		if (_moveDistance > BulletData.f_DISTANCE)
		{
			mainPhase = BulletPhase.End;
			return;
		}
		if (Target == null && ((int)TargetMask & (1 << ManagedSingleton<OrangeLayerManager>.Instance.EnemyLayer)) != 0)
		{
			Target = NeutralAIS.GetClosetEnemy();
		}
		if (Target == null && ((int)TargetMask & (1 << ManagedSingleton<OrangeLayerManager>.Instance.PlayerLayer)) != 0)
		{
			Target = NeutralAIS.GetClosetPlayer();
		}
		if (Target == null && (refPBMShoter.SOB == null || refPBMShoter.SOB.gameObject.layer != ManagedSingleton<OrangeLayerManager>.Instance.PvpPlayerLayer))
		{
			Target = NeutralAIS.GetClosetPvpPlayer();
		}
		if (Target != null)
		{
			if (_checkDirection)
			{
				_checkDirection = false;
				if (_transform.position.x < Target.AimTransform.position.x + Target.AimPoint.x)
				{
					Direction = Vector3.right;
				}
				else
				{
					Direction = Vector3.left;
				}
			}
			DoAim(Target);
			_shotDirection = Direction;
		}
		float num = (Time.fixedTime - _lastFrameTime) * 0.34f / GameLogicUpdateManager.m_fFrameLen;
		_currentSpeed = Mathf.Clamp(_currentSpeed + _acceleration * num, 0.1f, _maxSpeed);
		_speed = new Vector3(_currentSpeed, 0f, 0f);
	}
}
