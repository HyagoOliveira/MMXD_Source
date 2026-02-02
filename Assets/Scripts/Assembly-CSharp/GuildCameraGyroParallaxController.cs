using UnityEngine;

public class GuildCameraGyroParallaxController : MonoBehaviour
{
	private class CameraGyroParam
	{
		public float PositionRecoveryRate;

		public float RotationRecoveryRate;

		public float PositionGyroFactorX;

		public float PositionGyroFactorY;

		public float RotationGyroFactorX;

		public float RotationGyroFactorY;

		public float PositionMaxOffsetX;

		public float PositionMaxOffsetY;

		public float RotationMaxOffsetX;

		public float RotationMaxOffsetY;
	}

	private static CameraGyroParam GyroParam;

	private bool _showGUI;

	public bool IsPause;

	[SerializeField]
	private bool _isTestMode;

	[SerializeField]
	[Range(-100f, 100f)]
	private float _testForceX;

	[SerializeField]
	[Range(-100f, 100f)]
	private float _testForceY;

	[SerializeField]
	private float UpateInterval = 0.02f;

	[SerializeField]
	[Range(0f, 1f)]
	private float _positionRecoveryRate;

	[SerializeField]
	[Range(0f, 1f)]
	private float _rotationRecoveryRate;

	[SerializeField]
	[Range(-10f, 10f)]
	private float _positionGyroFactorX;

	[SerializeField]
	[Range(-10f, 10f)]
	private float _positionGyroFactorY;

	[SerializeField]
	[Range(-10f, 10f)]
	private float _rotationGyroFactorX;

	[SerializeField]
	[Range(-10f, 10f)]
	private float _rotationGyroFactorY;

	[SerializeField]
	[Range(0f, 100f)]
	private float _positionMaxOffsetX;

	[SerializeField]
	[Range(0f, 100f)]
	private float _positionMaxOffsetY;

	[SerializeField]
	[Range(0f, 100f)]
	private float _rotationMaxOffsetX;

	[SerializeField]
	[Range(0f, 100f)]
	private float _rotationMaxOffsetY;

	private Transform _transform;

	private Gyroscope _gyro;

	private Vector3 _positionOrigin;

	private Vector3 _rotationOrigin;

	private void Awake()
	{
		_transform = base.transform;
		_positionOrigin = _transform.position;
		_rotationOrigin = _transform.rotation.eulerAngles;
		if (SystemInfo.supportsGyroscope)
		{
			_gyro = Input.gyro;
			_gyro.updateInterval = UpateInterval;
		}
		if (GyroParam == null)
		{
			GyroParam = new CameraGyroParam
			{
				PositionRecoveryRate = _positionRecoveryRate,
				RotationRecoveryRate = _rotationRecoveryRate,
				PositionGyroFactorX = _positionGyroFactorX,
				PositionGyroFactorY = _positionGyroFactorY,
				RotationGyroFactorX = _rotationGyroFactorX,
				RotationGyroFactorY = _rotationGyroFactorY,
				PositionMaxOffsetX = _positionMaxOffsetX,
				PositionMaxOffsetY = _positionMaxOffsetY,
				RotationMaxOffsetX = _rotationMaxOffsetX,
				RotationMaxOffsetY = _rotationMaxOffsetY
			};
		}
		else
		{
			_positionRecoveryRate = GyroParam.PositionRecoveryRate;
			_rotationRecoveryRate = GyroParam.RotationRecoveryRate;
			_positionGyroFactorX = GyroParam.PositionGyroFactorX;
			_positionGyroFactorY = GyroParam.PositionGyroFactorY;
			_rotationGyroFactorX = GyroParam.RotationGyroFactorX;
			_rotationGyroFactorY = GyroParam.RotationGyroFactorY;
			_positionMaxOffsetX = GyroParam.PositionMaxOffsetX;
			_positionMaxOffsetY = GyroParam.PositionMaxOffsetY;
			_rotationMaxOffsetX = GyroParam.RotationMaxOffsetX;
			_rotationMaxOffsetY = GyroParam.RotationMaxOffsetY;
		}
	}

	private void OnEnable()
	{
		if (_gyro != null)
		{
			_gyro.enabled = true;
		}
	}

	private void OnDisable()
	{
		if (_gyro != null)
		{
			_gyro.enabled = false;
		}
	}

	private void LateUpdate()
	{
		if (!IsPause && (_isTestMode || _gyro != null))
		{
			Vector3 vector = (_isTestMode ? new Vector3(_testForceX, _testForceY) : _gyro.rotationRateUnbiased);
			Vector3 position = _transform.position;
			position += new Vector3(vector.x * _positionGyroFactorX, vector.y * _positionGyroFactorY).yxz();
			position = new Vector3(Mathf.Clamp(position.x, _positionOrigin.x - _positionMaxOffsetX, _positionOrigin.x + _positionMaxOffsetX), Mathf.Clamp(position.y, _positionOrigin.y - _positionMaxOffsetY, _positionOrigin.y + _positionMaxOffsetY), position.z);
			position = Vector3.Lerp(position, _positionOrigin, _positionRecoveryRate);
			_transform.position = position;
			Vector3 eulerAngles = _transform.rotation.eulerAngles;
			if (eulerAngles.x > 180f)
			{
				eulerAngles.x -= 360f;
			}
			if (eulerAngles.y > 180f)
			{
				eulerAngles.y -= 360f;
			}
			eulerAngles += new Vector3(vector.y * _rotationGyroFactorY, vector.x * _rotationGyroFactorX).yxz();
			eulerAngles = new Vector3(Mathf.Clamp(eulerAngles.x, _rotationOrigin.x - _rotationMaxOffsetX, _rotationOrigin.x + _rotationMaxOffsetX), Mathf.Clamp(eulerAngles.y, _rotationOrigin.y - _rotationMaxOffsetY, _rotationOrigin.y + _rotationMaxOffsetY), eulerAngles.z);
			eulerAngles = Vector3.Lerp(eulerAngles, _rotationOrigin, _rotationRecoveryRate);
			_transform.rotation = Quaternion.Euler(eulerAngles);
		}
	}
}
