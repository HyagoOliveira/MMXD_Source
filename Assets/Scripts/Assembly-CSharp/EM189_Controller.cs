using CallbackDefs;
using StageLib;
using UnityEngine;

public class EM189_Controller : EnemyControllerBase, IManagedUpdateBehavior
{
	protected enum MainStatus
	{
		Idle = 0,
		Move = 1,
		Skill = 2,
		Dead = 3,
		WaitNet = 4
	}

	protected enum SubStatus
	{
		Phase0 = 0,
		Phase1 = 1,
		Phase2 = 2,
		MAX_SUBSTATUS = 3
	}

	[SerializeField]
	protected Transform modelTransform;

	[SerializeField]
	private Transform collideBulletTransform;

	[SerializeField]
	private ParticleSystem fxLoop;

	[SerializeField]
	private ParticleSystem fxEnd;

	[SerializeField]
	private float skillDuration = 2f;

	[SerializeField]
	private float attachTime = 0.5f;

	private bool isPatrol;

	private bool _patrolIsLoop;

	private float MoveDis;

	private Vector3[] _patrolPaths = new Vector3[0];

	private bool _comeBack;

	private int _patrolIndex;

	private Vector3 StartPos;

	private Vector3 NextPos;

	private Vector3 MoveDirection;

	private float distance;

	private int MoveSpeed = 5000;

	private MainStatus mainStatus;

	private SubStatus subStatus;

	private MainStatus cacheStatus;

	private float currentDirection;

	private CharacterMaterial material;

	private int stunStack;

	private OrangeCharacter chaseTarget;

	private Vector3 triggerPosition;

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
		ModelTransform = modelTransform;
		mainStatus = MainStatus.Idle;
		subStatus = SubStatus.Phase0;
		IgnoreGravity = true;
		base.AllowAutoAim = true;
		material = GetComponent<CharacterMaterial>();
		_collideBullet = collideBulletTransform.gameObject.AddOrGetComponent<CollideBullet>();
		if (_enemyAutoAimSystem == null)
		{
			OrangeBattleUtility.AddEnemyAutoAimSystem(_transform, out _enemyAutoAimSystem);
		}
		_enemyAutoAimSystem.UpdateAimRange(5f);
		AiTimer.TimerStart();
	}

	public override void LogicUpdate()
	{
		if (Activate)
		{
			BaseLogicUpdate();
			UpdateStatusLogic();
			UpdateWaitNetStatus();
		}
	}

	public void UpdateFunc()
	{
		if (Activate)
		{
			if (modelTransform.eulerAngles.y != currentDirection)
			{
				modelTransform.localRotation = Quaternion.Euler(0f, currentDirection, 0f);
			}
			base.transform.localPosition = Vector3.MoveTowards(base.transform.localPosition, Controller.LogicPosition.vec3, distanceDelta);
		}
	}

	public override void SetActive(bool _isActive)
	{
		base.SetActive(_isActive);
		if (_isActive)
		{
			mainStatus = MainStatus.Idle;
			subStatus = SubStatus.Phase0;
			currentDirection = 135f;
			modelTransform.localRotation = Quaternion.Euler(0f, currentDirection, 0f);
			_collideBullet.UpdateBulletData(EnemyWeapons[0].BulletData);
			_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			_collideBullet.Active(targetMask);
			stunStack = 0;
		}
		fxLoop.Stop();
	}

	protected void UpdateWaitNetStatus()
	{
		if (bWaitNetStatus && (!StageUpdate.gbIsNetGame || !StageUpdate.bIsHost))
		{
			bWaitNetStatus = false;
			SetStatus(cacheStatus);
		}
	}

	protected void RegisterStatus(MainStatus _mainStatus)
	{
		cacheStatus = _mainStatus;
		if (StageUpdate.gbIsNetGame)
		{
			if (!bWaitNetStatus && StageUpdate.bIsHost)
			{
				StageUpdate.RegisterSendAndRun(sNetSerialID, (int)_mainStatus);
				mainStatus = MainStatus.WaitNet;
				bWaitNetStatus = true;
			}
		}
		else
		{
			if (bWaitNetStatus)
			{
				bWaitNetStatus = false;
			}
			SetStatus(_mainStatus);
		}
	}

	public override void UpdateStatus(int _nSet, string _smsg, Callback _callback = null)
	{
		SetStatus((MainStatus)_nSet);
	}

	public override void SetPositionAndRotation(Vector3 _pos, bool _back)
	{
		Controller.LogicPosition = new VInt3(_pos);
		base.transform.position = _pos;
	}

	protected void SetStatus(MainStatus _mainStatus, SubStatus _subStatus = SubStatus.Phase0)
	{
		mainStatus = _mainStatus;
		subStatus = _subStatus;
		switch (mainStatus)
		{
		case MainStatus.Idle:
			_velocity = VInt3.zero;
			break;
		case MainStatus.Move:
			if (!isPatrol)
			{
				break;
			}
			if (!_comeBack)
			{
				_patrolIndex++;
			}
			else
			{
				_patrolIndex--;
			}
			if ((!_comeBack && _patrolIndex >= _patrolPaths.Length) || (_comeBack && _patrolIndex < 0))
			{
				if (!_patrolIsLoop)
				{
					_velocity = VInt3.zero;
					isPatrol = false;
					break;
				}
				_comeBack = !_comeBack;
				_patrolIndex += (_comeBack ? (-2) : 2);
			}
			NextPos = _patrolPaths[_patrolIndex];
			StartPos = _transform.position;
			MoveDis = Vector2.Distance(StartPos, NextPos);
			MoveDirection = (NextPos - StartPos).normalized;
			distance = Vector2.Distance(StartPos, NextPos);
			_velocity = new VInt3(MoveDirection * MoveSpeed * 0.001f);
			if (MoveDirection.x < 0f)
			{
				currentDirection = 165f;
			}
			else
			{
				currentDirection = 90f;
			}
			break;
		case MainStatus.Skill:
			switch (subStatus)
			{
			case SubStatus.Phase0:
				PlaySE(EnemySE.CRI_ENEMYSE_EM016_MIRU01);
				base.SoundSource.PlaySE("EnemySE", "em016_miru01", 0.5f);
				fxLoop.Play();
				material.Disappear();
				triggerPosition = _transform.position;
				chaseTarget = Target;
				if (chaseTarget != null)
				{
					_velocity = new VInt3((chaseTarget.Controller.GetRealCenterPos() - triggerPosition) / attachTime);
					chaseTarget.SetStun(true);
					stunStack++;
				}
				else
				{
					_velocity = VInt3.zero;
				}
				MoveDirection = (chaseTarget.Controller.GetRealCenterPos() - _transform.position).normalized;
				if (MoveDirection.x < 0f)
				{
					currentDirection = 165f;
				}
				else
				{
					currentDirection = 90f;
				}
				break;
			case SubStatus.Phase1:
				_velocity = VInt3.zero;
				break;
			}
			break;
		case MainStatus.Dead:
			fxLoop.Stop();
			fxEnd.Play();
			Hp = 0;
			Hurt(new HurtPassParam());
			break;
		}
		AiTimer.TimerStart();
	}

	protected override void DeadBehavior(ref HurtPassParam tHurtPassParam)
	{
		if (chaseTarget != null)
		{
			for (int i = 0; i < stunStack; i++)
			{
				chaseTarget.SetStun(false);
			}
		}
		chaseTarget = null;
		_collideBullet.BackToPool();
		base.DeadBehavior(ref tHurtPassParam);
		PlaySE("HitSE", "ht_dead02");
	}

	private void UpdateStatusLogic()
	{
		switch (mainStatus)
		{
		case MainStatus.Idle:
			_velocity = VInt3.zero;
			RegisterStatus(MainStatus.Move);
			break;
		case MainStatus.Move:
			if (!Target)
			{
				Target = _enemyAutoAimSystem.GetClosetPlayer();
			}
			if ((bool)Target && Vector2.Distance(_transform.position, Target._transform.position) <= 1f)
			{
				RegisterStatus(MainStatus.Skill);
				isPatrol = false;
			}
			else if (isPatrol && Vector2.Distance(_transform.position, StartPos) > MoveDis)
			{
				RegisterStatus(MainStatus.Move);
			}
			break;
		case MainStatus.Skill:
			switch (subStatus)
			{
			case SubStatus.Phase0:
				distance = 999f;
				if (chaseTarget != null)
				{
					distance = Vector2.Distance(_transform.position, chaseTarget.Controller.GetRealCenterPos());
					_velocity = new VInt3((chaseTarget.Controller.GetRealCenterPos() - triggerPosition) / attachTime);
				}
				if ((float)AiTimer.GetMillisecond() > attachTime * 1000f || distance < 0.2f)
				{
					SetStatus(mainStatus, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if (chaseTarget != null)
				{
					Controller.LogicPosition = new VInt3(chaseTarget.Controller.GetRealCenterPos());
					_transform.position = chaseTarget.Controller.GetRealCenterPos();
				}
				if ((float)AiTimer.GetMillisecond() > skillDuration * 1000f)
				{
					RegisterStatus(MainStatus.Dead);
				}
				break;
			}
			break;
		case MainStatus.Dead:
			break;
		}
	}

	public override void SetPatrolPath(bool isLoop, int nMoveSpeed, Vector3[] paths)
	{
		base.SetPatrolPath(isLoop, nMoveSpeed, paths);
		_patrolIsLoop = isLoop;
		if (nMoveSpeed > 0)
		{
			MoveSpeed = nMoveSpeed;
		}
		_patrolPaths = new Vector3[paths.Length];
		for (int i = 0; i < paths.Length; i++)
		{
			_patrolPaths[i] = paths[i];
		}
		if (_patrolPaths.Length != 0)
		{
			isPatrol = true;
		}
	}
}
