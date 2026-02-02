using PathCreation;
using UnityEngine;

public class GuildCameraPathFollower : OrangePathFollower<GuildCameraPathFollower>
{
	public float MoveTime = 0.57f;

	private Vector3 _positionOrigin;

	private Quaternion _rotationOrigin;

	private float _pathLength;

	private Vector3 _lastAngle;

	private Transform _transform;

	private void Awake()
	{
		_transform = base.transform;
		_positionOrigin = _transform.position;
		_rotationOrigin = _transform.rotation;
	}

	public void SetPathCreator(PathCreator pathCreator, Vector3 lastAngle)
	{
		SetPathCreator(pathCreator, 0f, 0f);
		_lastAngle = lastAngle;
		Vector3 vector = pathCreator.bezierPath[0];
		Vector3 vector2 = pathCreator.bezierPath[pathCreator.bezierPath.NumPoints - 1];
		_pathLength = (new Vector3(vector2.x, vector2.y, vector2.z) - new Vector3(vector.x, vector.y, vector.z)).magnitude;
		_moveSpeed = _pathLength / MoveTime;
		_distanceTravelled = 0f;
	}

	public void ClearPath()
	{
		SetPathCreator(null, 0f, 0f);
		_distanceTravelled = 0f;
		_transform.position = _positionOrigin;
		_transform.rotation = _rotationOrigin;
	}

	private void LateUpdate()
	{
		if (!(base._pathCreator == null))
		{
			Vector3 euler = Vector3.Lerp(_rotationOrigin.eulerAngles, _lastAngle, _distanceTravelled / _pathLength);
			_transform.rotation = Quaternion.Euler(euler);
		}
	}
}
