using UnityEngine;

public class RotateHelper : MonoBehaviour
{
	public bool IsPause;

	[SerializeField]
	private bool _isRevolveMode;

	private bool _isRevolveModePrev;

	[SerializeField]
	private Space _coordinate;

	private Space _coordinatePrev;

	[SerializeField]
	private Vector3 _rotateCenter;

	private Vector3 _rotateCenterPrev;

	[SerializeField]
	private Vector3 _rotateAxis;

	private Vector3 _rotateAxisPrev;

	[SerializeField]
	private float _rotateSpeed;

	private Vector3 _originPosition;

	private Vector3 _originLocalPosition;

	private Quaternion _originRotation;

	private Vector3 _normalVector;

	private Transform _transform;

	private void Awake()
	{
		_transform = base.transform;
		_originRotation = _transform.rotation;
		_originLocalPosition = _transform.localPosition;
		_originPosition = _transform.position;
		RecalcNormalVector();
		BackupValue();
	}

	private void Update()
	{
		if (IsPause)
		{
			return;
		}
		Quaternion quaternion = Quaternion.AngleAxis(_rotateSpeed * Time.deltaTime, _rotateAxis);
		if (_isRevolveMode)
		{
			switch (_coordinate)
			{
			case Space.Self:
				_transform.rotation = quaternion * _transform.rotation;
				_normalVector = quaternion * _normalVector;
				_transform.localPosition = _rotateCenter + _normalVector;
				break;
			case Space.World:
				_transform.rotation = quaternion * _transform.rotation;
				_normalVector = quaternion * _normalVector;
				_transform.position = _rotateCenter + _normalVector;
				break;
			}
		}
		else
		{
			_transform.rotation = quaternion * _transform.rotation;
		}
	}

	public void StartRotate()
	{
		if (IsPause)
		{
			IsPause = false;
			Reset();
		}
	}

	public void StopRotate()
	{
		if (!IsPause)
		{
			IsPause = true;
			Reset();
		}
	}

	private void Reset()
	{
		if (!(_transform == null))
		{
			_transform.rotation = _originRotation;
			_transform.localPosition = _originLocalPosition;
		}
	}

	private void OnValidate()
	{
		if (_coordinate != _coordinatePrev || _isRevolveMode != _isRevolveModePrev || _rotateCenter != _rotateCenterPrev || _rotateAxis != _rotateAxisPrev)
		{
			RecalcNormalVector();
			Reset();
			BackupValue();
		}
	}

	private void BackupValue()
	{
		_coordinatePrev = _coordinate;
		_isRevolveModePrev = _isRevolveMode;
		_rotateCenterPrev = _rotateCenter;
		_rotateAxisPrev = _rotateAxis;
	}

	private void RecalcNormalVector()
	{
		switch (_coordinate)
		{
		case Space.Self:
			_normalVector = _originLocalPosition - _rotateCenter;
			break;
		case Space.World:
			_normalVector = _originPosition - _rotateCenter;
			break;
		}
	}
}
