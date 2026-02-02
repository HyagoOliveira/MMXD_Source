using UnityEngine;

public class EM079_Controller : EM017_Controller
{
	private enum MainStatus
	{
		Idle = 0,
		Move = 1,
		Skill_0 = 2,
		Skill_0_reload = 3
	}

	private MainStatus _mainStatus;

	private bool _patrolIsLoop;

	private Vector3[] _patrolPaths = new Vector3[0];

	private int _patrolIndex;

	private bool _comeBack;

	private bool _endPatrol;

	[SerializeField]
	private float _flySpeed = 1.5f;

	protected override void Awake()
	{
		base.Awake();
		Transform[] target = _transform.GetComponentsInChildren<Transform>(true);
		ModelTransform = OrangeBattleUtility.FindChildRecursive(ref target, "model", true);
		_patrolIndex = 0;
		_comeBack = false;
		_endPatrol = false;
		SetStatus(MainStatus.Idle);
	}

	public override void SetActive(bool isActive)
	{
		base.SetActive(isActive);
		if (isActive)
		{
			_patrolIndex = 0;
			_endPatrol = false;
			_comeBack = false;
			SetStatus(MainStatus.Idle);
		}
		else
		{
			_patrolPaths = new Vector3[0];
		}
	}

	public override void LogicUpdate()
	{
		if (!Activate)
		{
			return;
		}
		IgnoreGravity = true;
		BaseLogicUpdate();
		if (IsStun)
		{
			return;
		}
		switch (_mainStatus)
		{
		case MainStatus.Idle:
			if (AiTimer.GetMillisecond() > EnemyData.n_AI_TIMER)
			{
				SetStatus(MainStatus.Move);
			}
			break;
		case MainStatus.Move:
		{
			float num = Vector3.Distance(TargetPos.vec3, Controller.LogicPosition.vec3);
			if (num < 0.05f || num < distanceDelta * 2f)
			{
				SetStatus(MainStatus.Skill_0);
				break;
			}
			Vector3 normalized = (TargetPos - Controller.LogicPosition).vec3.normalized;
			_velocity = new VInt3(normalized * _flySpeed);
			break;
		}
		case MainStatus.Skill_0:
			if (_magazine > 0)
			{
				BulletBase.TryShotBullet(EnemyWeapons[0].BulletData, _shootPoint, Vector3.right * base.direction, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
				_magazine--;
			}
			else
			{
				SetStatus(MainStatus.Skill_0_reload);
			}
			break;
		case MainStatus.Skill_0_reload:
			_velocity.y = Mathf.RoundToInt(CalculateVerticalMovement() * 1000f);
			if (AiTimer.GetMillisecond() > EnemyWeapons[0].BulletData.n_RELOAD)
			{
				GotoNextPatrolPoint();
				if (_endPatrol)
				{
					SetStatus(MainStatus.Skill_0);
				}
				else
				{
					SetStatus(MainStatus.Move);
				}
			}
			break;
		}
	}

	public override void SetPositionAndRotation(Vector3 pos, bool bBack)
	{
		if (bBack)
		{
			base.direction = -1;
		}
		else
		{
			base.direction = 1;
		}
		ModelTransform.localScale = new Vector3(ModelTransform.localScale.x, ModelTransform.localScale.y, Mathf.Abs(ModelTransform.localScale.z) * (float)base.direction);
		base.transform.position = pos;
	}

	public override void SetPatrolPath(bool isLoop, int nMoveSpeed, Vector3[] paths)
	{
		base.SetPatrolPath(isLoop, nMoveSpeed, paths);
		_patrolIsLoop = isLoop;
		if (nMoveSpeed > 0)
		{
			_flySpeed = (float)nMoveSpeed * 0.001f;
		}
		_patrolPaths = new Vector3[paths.Length];
		for (int i = 0; i < paths.Length; i++)
		{
			_patrolPaths[i] = paths[i];
		}
	}

	private void SetStatus(MainStatus mainStatus)
	{
		_mainStatus = mainStatus;
		switch (_mainStatus)
		{
		case MainStatus.Idle:
			_magazine = MaxMagazine;
			_velocity = VInt3.zero;
			break;
		case MainStatus.Move:
			if (_patrolPaths.Length == 0)
			{
				_velocity = VInt3.zero;
			}
			else if (_patrolPaths.Length == 1)
			{
				TargetPos = new VInt3(_patrolPaths[0]);
			}
			else
			{
				TargetPos = new VInt3(_patrolPaths[_patrolIndex]);
				Vector3 normalized = (TargetPos - Controller.LogicPosition).vec3.normalized;
				_velocity = new VInt3(normalized * _flySpeed);
			}
			UpdateDirection(50);
			break;
		case MainStatus.Skill_0:
			_velocity = VInt3.zero;
			break;
		case MainStatus.Skill_0_reload:
		{
			_magazine = MaxMagazine;
			_velocity = VInt3.zero;
			float y = _transform.position.y;
			_globalWaypoints[0] = y + 0.25f;
			_globalWaypoints[1] = y - 0.25f;
			break;
		}
		}
		AiTimer.TimerStart();
	}

	private void UpdateDirection(int diffRange)
	{
		int num = Controller.LogicPosition.x - TargetPos.x;
		if (Mathf.Abs(num) > diffRange)
		{
			base.direction = ((num <= 0) ? 1 : (-1));
		}
		ModelTransform.localScale = new Vector3(ModelTransform.localScale.x, ModelTransform.localScale.y, Mathf.Abs(ModelTransform.localScale.z) * (float)base.direction);
	}

	private void GotoNextPatrolPoint()
	{
		if (_patrolPaths.Length == 1)
		{
			_endPatrol = true;
		}
		else
		{
			if (_patrolPaths.Length <= 1)
			{
				return;
			}
			if (_patrolIsLoop)
			{
				_patrolIndex++;
				if (_patrolIndex >= _patrolPaths.Length)
				{
					_patrolIndex = 0;
				}
			}
			else
			{
				if ((!_comeBack && _patrolIndex + 1 >= _patrolPaths.Length) || (_comeBack && _patrolIndex == 0))
				{
					_comeBack = !_comeBack;
				}
				_patrolIndex += ((!_comeBack) ? 1 : (-1));
			}
		}
	}
}
