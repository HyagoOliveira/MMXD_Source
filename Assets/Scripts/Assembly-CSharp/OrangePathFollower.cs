using System;
using PathCreation;
using UnityEngine;

public abstract class OrangePathFollower<T> : MonoBehaviour where T : OrangePathFollower<T>
{
	protected readonly float REACH_DIST_THRESHOLD = 0.1f;

	[SerializeField]
	protected EndOfPathInstruction _endOfPathInstruction;

	[SerializeField]
	protected float _moveSpeed = 5f;

	protected float _distanceTravelled;

	protected bool _checkEndPoint;

	protected float _startDelayTime;

	protected float _endDelayTime;

	protected PathCreator _pathCreator { get; private set; }

	public event Action<PathCreator, T> OnReachPathEndPointEvent;

	public virtual void OnDestroy()
	{
		this.OnReachPathEndPointEvent = null;
	}

	public virtual void SetPathCreator(PathCreator pathCreator, float startDelayTime, float endDelayTime, bool isReverse = false)
	{
		if (_pathCreator != null)
		{
			_pathCreator.pathUpdated -= OnPathChanged;
		}
		_pathCreator = pathCreator;
		if (_pathCreator != null)
		{
			_pathCreator.pathUpdated += OnPathChanged;
			base.transform.position = ((isReverse && _endOfPathInstruction == EndOfPathInstruction.Reverse) ? _pathCreator.path.GetPoint(_pathCreator.path.NumPoints - 1) : _pathCreator.path.GetPoint(0));
			_distanceTravelled = ((isReverse && _endOfPathInstruction == EndOfPathInstruction.Reverse) ? _pathCreator.path.length : 0f);
			_startDelayTime = startDelayTime;
			_endDelayTime = endDelayTime;
			_checkEndPoint = false;
		}
	}

	public virtual void Update()
	{
		if (_pathCreator == null)
		{
			return;
		}
		VertexPath path = _pathCreator.path;
		if (Vector3.Distance(base.transform.position, path.GetPoint(0)) < REACH_DIST_THRESHOLD || Vector3.Distance(base.transform.position, path.GetPoint(path.NumPoints - 1)) < REACH_DIST_THRESHOLD)
		{
			if (!_checkEndPoint)
			{
				if (CheckStartDelay())
				{
					UpdatePositionAndRotation();
					return;
				}
			}
			else
			{
				if (CheckEndDelay())
				{
					return;
				}
				Action<PathCreator, T> onReachPathEndPointEvent = this.OnReachPathEndPointEvent;
				if (onReachPathEndPointEvent != null)
				{
					onReachPathEndPointEvent(_pathCreator, (T)this);
				}
			}
		}
		else
		{
			_checkEndPoint = true;
		}
		_distanceTravelled += _moveSpeed * Time.deltaTime;
		UpdatePositionAndRotation();
	}

	protected virtual void OnPathChanged()
	{
		_distanceTravelled = _pathCreator.path.GetClosestDistanceAlongPath(base.transform.position);
	}

	protected virtual bool CheckStartDelay()
	{
		if (_startDelayTime >= 0f)
		{
			_startDelayTime -= Time.deltaTime;
			return true;
		}
		return false;
	}

	protected virtual bool CheckEndDelay()
	{
		if (_endDelayTime >= 0f)
		{
			_endDelayTime -= Time.deltaTime;
			return true;
		}
		return false;
	}

	protected void UpdatePositionAndRotation()
	{
		VertexPath path = _pathCreator.path;
		base.transform.position = path.GetPointAtDistance(_distanceTravelled, _endOfPathInstruction);
		base.transform.rotation = path.GetRotationAtDistance(_distanceTravelled, _endOfPathInstruction);
	}
}
