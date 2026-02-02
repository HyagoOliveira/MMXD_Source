using CallbackDefs;
using Newtonsoft.Json;
using StageLib;
using UnityEngine;

public class EM044_Controller : EnemyControllerBase, IManagedUpdateBehavior
{
	private enum MainStatus
	{
		Idle = 0,
		Fly = 1,
		Hurt = 2,
		Skill_0 = 3,
		IdleWaitNet = 4
	}

	private enum SubStatus
	{
		Phase0 = 0,
		Phase1 = 1,
		Phase2 = 2,
		Phase3 = 3,
		Phase4 = 4,
		Phase5 = 5,
		Phase6 = 6,
		MAX_SUBSTATUS = 7
	}

	public enum AnimationID
	{
		ANI_IDLE = 0,
		ANI_MOVE_UPWARD_LOOP = 1,
		ANI_MOVE_DOWNWARD_LOOP = 2,
		ANI_HURT = 3,
		ANI_TURN_FRONT_TO_UP = 4,
		ANI_TURN_UP_TO_FRONT = 5,
		ANI_TURN_FRONT_TO_DOWN = 6,
		ANI_TURN_DOWN_TO_FRONT = 7,
		MAX_ANIMATION_ID = 8
	}

	private Transform _shootPoint;

	private ParticleSystem _rearBooster;

	private ParticleSystem _sideBooster;

	private Vector3 _shootDirection;

	private MainStatus _mainStatus;

	private SubStatus _subStatus;

	private AnimationID _currentAnimationId;

	private float _currentFrame;

	private int[] _animationHash;

	private bool _patrolIsLoop;

	private Vector3[] _patrolPaths = new Vector3[0];

	private int _patrolIndex;

	private bool _comeBack;

	private bool _endPatrol;

	private Vector3 _lockPos;

	private float _gunRotationFrame;

	private float _setFloatPosY;

	private float dirRotation = 90f;

	[SerializeField]
	private int _skill_01_wait = 300;

	[SerializeField]
	private float _flySpeed = 1.5f;

	[SerializeField]
	private float _easeSpeedMax = 3f;

	[SerializeField]
	private float _easeSpeedMin = 0.5f;

	[SerializeField]
	private float _easeDistMax = 1.5f;

	[SerializeField]
	private float _easeDistMin = 0.5f;

	private float rot;

	private Vector3 StartPos;

	private float MoveDis = 500f;

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
		Transform[] target = _transform.GetComponentsInChildren<Transform>(true);
		ModelTransform = OrangeBattleUtility.FindChildRecursive(ref target, "model", true);
		_rearBooster = OrangeBattleUtility.FindChildRecursive(ref target, "RearBooster", true).GetComponent<ParticleSystem>();
		_sideBooster = OrangeBattleUtility.FindChildRecursive(ref target, "SideBooster", true).GetComponent<ParticleSystem>();
		_animator = GetComponentInChildren<Animator>();
		_collideBullet = base.gameObject.AddOrGetComponent<CollideBullet>();
		_shootPoint = OrangeBattleUtility.FindChildRecursive(_transform, "ShootPoint", true);
		_globalWaypoints = new float[2];
		_easeSpeed = _easeSpeedMin;
		base.AimPoint = default(Vector3);
		OrangeBattleUtility.AddEnemyAutoAimSystem(_transform, out _enemyAutoAimSystem);
		_animationHash = new int[8];
		_animationHash[0] = Animator.StringToHash("EM044@idle_loop");
		_animationHash[1] = Animator.StringToHash("EM044@move_upward_loop");
		_animationHash[2] = Animator.StringToHash("EM044@move_downward_loop");
		_animationHash[3] = Animator.StringToHash("EM044@hurt_loop");
		_animationHash[4] = Animator.StringToHash("EM044@turn_front_to_up");
		_animationHash[5] = Animator.StringToHash("EM044@turn_up_to_front");
		_animationHash[6] = Animator.StringToHash("EM044@turn_front_to_down");
		_animationHash[7] = Animator.StringToHash("EM044@turn_down_to_front");
		_mainStatus = MainStatus.Idle;
		_subStatus = SubStatus.Phase0;
		IgnoreGravity = true;
		_patrolIndex = 0;
		_comeBack = false;
		_endPatrol = false;
		_rearBooster.Stop();
		_sideBooster.Stop();
		AiTimer.TimerStart();
	}

	public override void UpdateStatus(int nSet, string sMsg, Callback tCB = null)
	{
		bWaitNetStatus = false;
		if ((int)Hp <= 0)
		{
			return;
		}
		SubStatus subStatus = SubStatus.Phase0;
		if (sMsg != null && sMsg != "")
		{
			NetSyncData netSyncData = JsonConvert.DeserializeObject<NetSyncData>(sMsg);
			Controller.LogicPosition.x = netSyncData.SelfPosX;
			Controller.LogicPosition.y = netSyncData.SelfPosY;
			Controller.LogicPosition.z = netSyncData.SelfPosZ;
			if (nSet != 1)
			{
				TargetPos.x = netSyncData.TargetPosX;
				TargetPos.y = netSyncData.TargetPosY;
				TargetPos.z = netSyncData.TargetPosZ;
			}
			if (netSyncData.bSetHP)
			{
				Hp = netSyncData.nHP;
			}
			if (netSyncData.nParam0 != 0)
			{
				subStatus = (SubStatus)netSyncData.nParam0;
			}
		}
		SetStatus((MainStatus)nSet, subStatus);
	}

	private void UpdateRandomState(bool bReShot = false)
	{
		if (StageUpdate.bIsHost)
		{
			if (bWaitNetStatus)
			{
				return;
			}
		}
		else if (bWaitNetStatus)
		{
			bWaitNetStatus = false;
		}
		if (!StageUpdate.bIsHost)
		{
			return;
		}
		MainStatus nSetKey = MainStatus.Fly;
		Target = _enemyAutoAimSystem.GetClosetPlayer();
		if ((bool)Target)
		{
			TargetPos = new VInt3(Target.AimTransform.position);
		}
		NetSyncData netSyncData = new NetSyncData();
		netSyncData.TargetPosX = TargetPos.x;
		netSyncData.TargetPosY = TargetPos.y;
		netSyncData.TargetPosZ = TargetPos.z;
		netSyncData.SelfPosX = Controller.LogicPosition.x;
		netSyncData.SelfPosY = Controller.LogicPosition.y;
		netSyncData.SelfPosZ = Controller.LogicPosition.z;
		netSyncData.nParam0 = 0;
		if ((bool)Target)
		{
			nSetKey = MainStatus.Skill_0;
			if (bReShot)
			{
				netSyncData.nParam0 = 2;
			}
		}
		bWaitNetStatus = true;
		StageUpdate.RegisterSendAndRun(sNetSerialID, (int)nSetKey, JsonConvert.SerializeObject(netSyncData));
	}

	public override void LogicUpdate()
	{
		if (!Activate || !_enemyAutoAimSystem)
		{
			return;
		}
		BaseLogicUpdate();
		_currentFrame = _animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
		switch (_mainStatus)
		{
		case MainStatus.Idle:
			_velocity = VInt3.zero;
			if (AiTimer.GetMillisecond() > EnemyData.n_AI_TIMER)
			{
				UpdateRandomState();
			}
			break;
		case MainStatus.Fly:
			Target = _enemyAutoAimSystem.GetClosetPlayer();
			if ((bool)Target)
			{
				_velocity = VInt3.zero;
				UpdateRandomState();
				return;
			}
			switch (_subStatus)
			{
			case SubStatus.Phase0:
			{
				if (ModelTransform.eulerAngles.y != dirRotation)
				{
					_velocity = VInt3.zero;
					break;
				}
				float num = Vector3.Distance(TargetPos.vec3, Controller.LogicPosition.vec3);
				if (num < 0.05f || num < distanceDelta * 2f)
				{
					SetStatus(MainStatus.Fly, SubStatus.Phase1);
					break;
				}
				Vector3 normalized = (TargetPos - Controller.LogicPosition).vec3.normalized;
				_velocity = new VInt3(normalized * _flySpeed);
				if (Mathf.Abs(_velocity.x) > 50)
				{
					int num2 = Mathf.RoundToInt((CalculateVerticalMovement(true) - _setFloatPosY) * 1000f);
					_velocity.y += num2;
				}
				break;
			}
			case SubStatus.Phase1:
				_velocity.x = 0;
				_velocity.y = Mathf.RoundToInt(CalculateVerticalMovement() * 1000f);
				break;
			}
			break;
		case MainStatus.Skill_0:
			_velocity = VInt3.zero;
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				dirRotation = ((Controller.LogicPosition.x > TargetPos.x) ? 270 : 90);
				if (ModelTransform.eulerAngles.y == dirRotation && TargetIsFoward())
				{
					SetStatus(MainStatus.Skill_0, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
			case SubStatus.Phase5:
				if (_currentFrame > _gunRotationFrame - 0.1f)
				{
					SetStatus(MainStatus.Skill_0, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				if (AiTimer.GetMillisecond() > _skill_01_wait)
				{
					SetStatus(MainStatus.Skill_0, SubStatus.Phase3);
				}
				break;
			case SubStatus.Phase3:
				if (AiTimer.GetMillisecond() <= EnemyWeapons[1].BulletData.n_RELOAD)
				{
					break;
				}
				if (TargetIsFoward())
				{
					Vector3 vector = _lockPos - TargetPos.vec3;
					Vector3 vector2 = new Vector3(0f, 0f, Vector2.SignedAngle(Vector2.right, vector));
					Vector3 vector3 = TargetPos.vec3 - _transform.position;
					Vector3 vector4 = new Vector3(0f, 0f, Vector2.SignedAngle(Vector2.right, vector3));
					if ((vector2.z > 0f && vector4.z < 0f) || (vector2.z < 0f && vector4.z > 0f))
					{
						SetStatus(MainStatus.Skill_0, SubStatus.Phase4);
					}
					else
					{
						SetStatus(MainStatus.Skill_0, SubStatus.Phase5);
					}
				}
				else
				{
					SetStatus(MainStatus.Skill_0, SubStatus.Phase4);
				}
				break;
			case SubStatus.Phase4:
				if (_currentFrame >= 1f)
				{
					UpdateRandomState();
				}
				break;
			case SubStatus.Phase6:
				UpdateRandomState();
				break;
			}
			break;
		}
		if (Mathf.Abs(_velocity.x) > 10)
		{
			_rearBooster.Play();
			_sideBooster.Stop();
		}
		else
		{
			_rearBooster.Stop();
			_sideBooster.Play();
		}
		if (ModelTransform.eulerAngles.y > dirRotation)
		{
			rot = ModelTransform.eulerAngles.y - 180f;
			rot = Mathf.Clamp(rot, 90f, 270f);
		}
		else if (ModelTransform.eulerAngles.y < dirRotation)
		{
			rot = ModelTransform.eulerAngles.y + 180f;
			rot = Mathf.Clamp(rot, 90f, 270f);
		}
	}

	public void UpdateFunc()
	{
		if (Activate)
		{
			ModelTransform.localEulerAngles = Vector3.MoveTowards(ModelTransform.localEulerAngles, new Vector3(ModelTransform.localEulerAngles.x, rot, ModelTransform.localEulerAngles.z), Time.deltaTime * 5f * 45f);
			base.transform.localPosition = Vector3.MoveTowards(base.transform.localPosition, Controller.LogicPosition.vec3, distanceDelta);
			if (_mainStatus == MainStatus.Fly && _subStatus == SubStatus.Phase0 && Vector3.Distance(StartPos, _transform.position) > MoveDis)
			{
				_velocity = VInt3.zero;
				SetStatus(MainStatus.Fly, SubStatus.Phase1);
			}
		}
	}

	public override void SetActive(bool isActive)
	{
		base.SetActive(isActive);
		if (isActive)
		{
			_collideBullet.UpdateBulletData(EnemyWeapons[0].BulletData);
			_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			_collideBullet.Active(targetMask);
			_patrolIndex = 0;
			_comeBack = false;
			_endPatrol = false;
			SetStatus(MainStatus.Idle);
		}
		else
		{
			_patrolPaths = new Vector3[0];
			_rearBooster.Stop();
			_sideBooster.Stop();
			_collideBullet.BackToPool();
		}
		_easeSpeed = _easeSpeedMin;
		_setFloatPosY = _transform.position.y;
		_globalWaypoints[0] = _setFloatPosY + _easeDistMin;
		_globalWaypoints[1] = _setFloatPosY - _easeDistMin;
	}

	public override void SetPositionAndRotation(Vector3 pos, bool bBack)
	{
		if (bBack)
		{
			dirRotation = 270f;
		}
		else
		{
			dirRotation = 90f;
		}
		base.transform.position = pos;
		ModelTransform.localRotation = Quaternion.Euler(0f, dirRotation, 0f);
	}

	public override void UpdateEnemyID(int id)
	{
		base.UpdateEnemyID(id);
		if (null == _enemyAutoAimSystem)
		{
			OrangeBattleUtility.AddEnemyAutoAimSystem(_transform, out _enemyAutoAimSystem);
		}
		_enemyAutoAimSystem.UpdateAimRange(EnemyWeapons[1].BulletData.f_DISTANCE);
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

	private bool TargetIsFoward()
	{
		if ((TargetPos.x <= Controller.LogicPosition.x && dirRotation == 270f) || (TargetPos.x > Controller.LogicPosition.x && dirRotation == 90f))
		{
			return true;
		}
		return false;
	}

	private void UpdateDirection(int diffRange)
	{
		int num = Controller.LogicPosition.x - TargetPos.x;
		if (Mathf.Abs(num) > diffRange)
		{
			dirRotation = ((num > 0) ? 270f : 90f);
		}
	}

	private void SetStatus(MainStatus mainStatus, SubStatus subStatus = SubStatus.Phase0)
	{
		_mainStatus = mainStatus;
		_subStatus = subStatus;
		float num = _easeDistMin;
		switch (_mainStatus)
		{
		case MainStatus.Idle:
			_velocity = VInt3.zero;
			break;
		case MainStatus.Fly:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				StartPos = Controller.LogicPosition.vec3;
				if (_patrolPaths.Length == 0)
				{
					TargetPos = Controller.LogicPosition;
					_easeSpeed = _easeSpeedMin;
				}
				else
				{
					TargetPos = new VInt3(_patrolPaths[_patrolIndex]);
					UpdateDirection(50);
					_easeSpeed = _easeSpeedMax;
					num = _easeDistMax;
				}
				MoveDis = Vector3.Distance(StartPos, TargetPos.vec3);
				break;
			case SubStatus.Phase1:
				if (_patrolPaths.Length <= 1)
				{
					break;
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
				StartPos = Controller.LogicPosition.vec3;
				TargetPos = new VInt3(_patrolPaths[_patrolIndex]);
				MoveDis = Vector3.Distance(StartPos, TargetPos.vec3);
				UpdateDirection(50);
				num = _easeDistMax;
				_subStatus = SubStatus.Phase0;
				break;
			}
			_setFloatPosY = _transform.position.y;
			_globalWaypoints[0] = _setFloatPosY + num;
			_globalWaypoints[1] = _setFloatPosY - num;
			break;
		case MainStatus.Skill_0:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_velocity = VInt3.zero;
				break;
			case SubStatus.Phase1:
			case SubStatus.Phase5:
				_lockPos = TargetPos.vec3;
				break;
			case SubStatus.Phase3:
				_shootDirection = (_lockPos - _shootPoint.position).normalized;
				BulletBase.TryShotBullet(EnemyWeapons[1].BulletData, _shootPoint, _shootDirection, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
				break;
			}
			break;
		}
		AiTimer.TimerStart();
		UpdateAnimation();
	}

	private void UpdateAnimation()
	{
		switch (_mainStatus)
		{
		case MainStatus.Idle:
			_currentAnimationId = AnimationID.ANI_IDLE;
			_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
			break;
		case MainStatus.Fly:
			_currentAnimationId = AnimationID.ANI_IDLE;
			_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
			break;
		case MainStatus.Skill_0:
			switch (_subStatus)
			{
			case SubStatus.Phase1:
			{
				Vector3 vector2 = _lockPos - _transform.position;
				Vector3 vector3 = new Vector3(0f, 0f, Vector2.SignedAngle(Vector2.right, vector2));
				if (vector3.z >= 0f)
				{
					_currentAnimationId = AnimationID.ANI_TURN_FRONT_TO_UP;
					_gunRotationFrame = ((vector3.z > 90f) ? ((180f - vector3.z) / 90f) : (vector3.z / 90f));
				}
				else
				{
					_currentAnimationId = AnimationID.ANI_TURN_FRONT_TO_DOWN;
					float num3 = Mathf.Abs(vector3.z);
					_gunRotationFrame = ((num3 > 90f) ? ((180f - num3) / 90f) : (num3 / 90f));
				}
				_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
				_animator.enabled = true;
				break;
			}
			case SubStatus.Phase2:
				_animator.Play(_animationHash[(int)_currentAnimationId], 0, _gunRotationFrame);
				_animator.enabled = false;
				break;
			case SubStatus.Phase4:
				_animator.enabled = true;
				if (_currentAnimationId == AnimationID.ANI_TURN_FRONT_TO_UP)
				{
					_currentAnimationId = AnimationID.ANI_TURN_UP_TO_FRONT;
					if (_currentFrame < 1f)
					{
						_animator.Play(_animationHash[(int)_currentAnimationId], 0, 1f - _currentFrame);
					}
					else
					{
						_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
					}
				}
				else if (_currentAnimationId == AnimationID.ANI_TURN_FRONT_TO_DOWN)
				{
					_currentAnimationId = AnimationID.ANI_TURN_DOWN_TO_FRONT;
					if (_currentFrame < 1f)
					{
						_animator.Play(_animationHash[(int)_currentAnimationId], 0, 1f - _currentFrame);
					}
					else
					{
						_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
					}
				}
				else
				{
					_currentAnimationId = AnimationID.ANI_IDLE;
					_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
				}
				break;
			case SubStatus.Phase5:
			{
				Vector3 vector = _lockPos - _transform.position;
				float num = Mathf.Abs(new Vector3(0f, 0f, Vector2.SignedAngle(Vector2.right, vector)).z);
				float num2 = ((num > 90f) ? ((180f - num) / 90f) : (num / 90f));
				if (_currentAnimationId == AnimationID.ANI_TURN_UP_TO_FRONT || _currentAnimationId == AnimationID.ANI_TURN_DOWN_TO_FRONT)
				{
					num2 = 1f - num2;
				}
				if (num2 > _currentFrame + 0.1f)
				{
					_gunRotationFrame = num2;
					_animator.enabled = true;
				}
				else if (num2 < _currentFrame - 0.1f)
				{
					if (_currentAnimationId == AnimationID.ANI_TURN_FRONT_TO_UP)
					{
						_currentAnimationId = AnimationID.ANI_TURN_UP_TO_FRONT;
					}
					else if (_currentAnimationId == AnimationID.ANI_TURN_UP_TO_FRONT)
					{
						_currentAnimationId = AnimationID.ANI_TURN_FRONT_TO_UP;
					}
					else if (_currentAnimationId == AnimationID.ANI_TURN_FRONT_TO_DOWN)
					{
						_currentAnimationId = AnimationID.ANI_TURN_DOWN_TO_FRONT;
					}
					else if (_currentAnimationId == AnimationID.ANI_TURN_DOWN_TO_FRONT)
					{
						_currentAnimationId = AnimationID.ANI_TURN_FRONT_TO_DOWN;
					}
					_gunRotationFrame = 1f - num2;
					_animator.Play(_animationHash[(int)_currentAnimationId], 0, 1f - _currentFrame);
					_animator.enabled = true;
				}
				else
				{
					SetStatus(MainStatus.Skill_0, SubStatus.Phase6);
				}
				break;
			}
			case SubStatus.Phase3:
				break;
			}
			break;
		case MainStatus.Hurt:
			break;
		}
	}
}
