#define RELEASE
using UnityEngine;

public class EM017_Controller : EnemyControllerBase, IManagedUpdateBehavior
{
	protected Transform _shootPoint;

	protected int _magazine;

	public int MaxMagazine = 20;

	[SerializeField]
	private int MoveSpeed = 2000;

	private bool isMoving;

	private bool CanAtk;

	private bool NeedMoving;

	private bool isAtking;

	private float distance;

	private Vector3 StartPos;

	private Vector3[] _patrolPaths = new Vector3[0];

	private int _patrolIndex;

	private Vector3 NextPos
	{
		get
		{
			return _patrolPaths[_patrolIndex % _patrolPaths.Length];
		}
	}

	private void OnEnable()
	{
		MonoBehaviourSingleton<UpdateManager>.Instance.AddUpdate(this);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		MonoBehaviourSingleton<UpdateManager>.Instance.RemoveUpdate(this);
	}

	protected override void Awake()
	{
		base.Awake();
		AiTimer.TimerStart();
		_shootPoint = OrangeBattleUtility.FindChildRecursive(_transform, "ShootPoint", true);
		_globalWaypoints = new float[2];
		base.AimPoint = new Vector3(0.2f, 0.3f, 0f);
		OrangeBattleUtility.AddEnemyAutoAimSystem(_transform, out _enemyAutoAimSystem);
		_enemyAutoAimSystem.SetColliderType(EnemyAutoAimSystem.ColliderType.Box);
		_enemyAutoAimSystem.UpdateAimRange(20f, 8f);
	}

	public override void LogicUpdate()
	{
		if (!Activate)
		{
			return;
		}
		base.LogicUpdate();
		if (IsStun)
		{
			return;
		}
		if (!Target)
		{
			Target = _enemyAutoAimSystem.GetClosetPlayer();
		}
		if (NeedMoving && !isAtking)
		{
			if ((bool)Target && Mathf.Abs(Target._transform.position.y - _transform.position.y) < 2f)
			{
				_velocity = VInt3.zero;
				isMoving = false;
				CanAtk = true;
			}
			else
			{
				if (Vector2.Distance(StartPos, _transform.position) > distance)
				{
					_patrolIndex++;
				}
				StartPos = _transform.position;
				distance = Vector2.Distance(StartPos, NextPos);
				_velocity = new VInt3((NextPos - StartPos).normalized) * MoveSpeed * 0.001f;
				isMoving = true;
				CanAtk = false;
			}
			if (isMoving)
			{
				if (Vector2.Distance(StartPos, _transform.position) > distance)
				{
					_patrolIndex++;
					StartPos = _transform.position;
					distance = Vector2.Distance(StartPos, NextPos);
					_velocity = new VInt3((NextPos - StartPos).normalized) * MoveSpeed * 0.001f;
				}
				return;
			}
		}
		if (AiTimer.GetMillisecond() > 3000)
		{
			AiTimer.TimerStart();
			_magazine = MaxMagazine;
			_velocity.y = 0;
		}
		if (_magazine > 0)
		{
			if (NeedMoving)
			{
				isAtking = true;
			}
			BulletBase.TryShotBullet(EnemyWeapons[0].BulletData, _shootPoint, Vector3.right * base.direction, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
			_magazine--;
			return;
		}
		if (NeedMoving)
		{
			isAtking = false;
			if (CanAtk)
			{
				return;
			}
		}
		_velocity.y = Mathf.RoundToInt(CalculateVerticalMovement() * 1000f);
	}

	public void UpdateFunc()
	{
		if (Activate)
		{
			base.transform.localPosition = Vector3.MoveTowards(base.transform.localPosition, Controller.LogicPosition.vec3, distanceDelta);
		}
	}

	public override void SetActive(bool isActive)
	{
		IgnoreGravity = true;
		base.SetActive(isActive);
		float y = _transform.position.y;
		_globalWaypoints[0] = y + 0.25f;
		_globalWaypoints[1] = y - 0.25f;
		if (NeedMoving)
		{
			isMoving = true;
			CanAtk = false;
			isAtking = false;
			StartPos = _transform.position;
			distance = Vector2.Distance(StartPos, _patrolPaths[0]);
			_velocity = new VInt3((_patrolPaths[0] - StartPos).normalized) * MoveSpeed * 0.001f;
		}
	}

	public override void UpdateEnemyID(int id)
	{
		base.UpdateEnemyID(id);
		UpdateAIState();
		AI_STATE aiState = AiState;
		if (aiState == AI_STATE.mob_002)
		{
			NeedMoving = true;
		}
		else
		{
			NeedMoving = false;
		}
	}

	public override void SetPatrolPath(bool isLoop, int nMoveSpeed, Vector3[] paths)
	{
		if (paths.Length == 0)
		{
			Debug.Log("Cause exception!!");
			return;
		}
		base.SetPatrolPath(isLoop, nMoveSpeed, paths);
		if (nMoveSpeed > 0)
		{
			MoveSpeed = nMoveSpeed;
		}
		_patrolPaths = new Vector3[paths.Length];
		for (int i = 0; i < paths.Length; i++)
		{
			_patrolPaths[i] = paths[i];
		}
		if (_patrolPaths.Length < 2)
		{
			Debug.LogError("EM017 小怪需要至少長度為2的巡邏路線決定生長方向跟長度");
		}
		_transform.position = _patrolPaths[0];
		Controller.LogicPosition = new VInt3(_transform.position);
		NeedMoving = true;
		isMoving = true;
	}
}
