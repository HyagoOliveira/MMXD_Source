using CallbackDefs;
using Newtonsoft.Json;
using StageLib;
using UnityEngine;

public class EM062_Controller : EnemyControllerBase, IManagedUpdateBehavior
{
	protected enum MainStatus
	{
		Idle = 0,
		Fly = 1,
		Hurt = 2,
		Shoot = 3
	}

	protected enum SubStatus
	{
		Phase0 = 0,
		Phase1 = 1,
		Phase2 = 2,
		Phase3 = 3,
		Phase4 = 4,
		Phase5 = 5,
		MAX_SUBSTATUS = 6
	}

	private enum AnimationID
	{
		ANI_IDLE_CLOSING = 0,
		ANI_IDLE_OPENING = 1,
		ANI_HURT = 2,
		ANI_OPEN_SHIELD = 3,
		ANI_CLOSE_SHIELD = 4,
		MAX_ANIMATION_ID = 5
	}

	[SerializeField]
	protected Transform modelTransform;

	[SerializeField]
	protected Transform shootPointTransform;

	[SerializeField]
	protected Transform shieldTransform;

	[SerializeField]
	protected int skill_01_cooldown = 3000;

	[SerializeField]
	protected float flySpeed = 3f;

	protected MainStatus mainStatus;

	protected SubStatus subStatus;

	protected float currentFrame;

	private AnimationID currentAnimationId;

	protected int[] animationHash;

	protected Vector3 shootDirection;

	protected float selfDirection = 90f;

	protected int flyTimer;

	protected Vector3[] patrolPaths = new Vector3[0];

	protected int patrolIndex;

	protected bool patrolIsLoop;

	protected bool comeBack;

	protected bool endPatrol;

	private float rot;

	protected Vector3 StartPos;

	protected float MoveDis = 500f;

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
		_animator = GetComponentInChildren<Animator>();
		_collideBullet = base.gameObject.AddOrGetComponent<CollideBullet>();
		_globalWaypoints = new float[2];
		base.AimPoint = default(Vector3);
		OrangeBattleUtility.AddEnemyAutoAimSystem(_transform, out _enemyAutoAimSystem);
		shieldTransform.gameObject.AddOrGetComponent<StageObjParam>().nSubPartID = 1;
		GuardTransform.Add(1);
		animationHash = new int[5];
		animationHash[0] = Animator.StringToHash("EM062@idle1_loop");
		animationHash[1] = Animator.StringToHash("EM062@idle2_loop");
		animationHash[2] = Animator.StringToHash("EM062@hurt_loop");
		animationHash[3] = Animator.StringToHash("EM062@open_shield");
		animationHash[4] = Animator.StringToHash("EM062@close_shield");
		mainStatus = MainStatus.Idle;
		subStatus = SubStatus.Phase0;
		IgnoreGravity = true;
		patrolIndex = 0;
		comeBack = false;
		endPatrol = false;
		base.SoundSource.UpdateDistanceCall();
		AiTimer.TimerStart();
	}

	public override void LogicUpdate()
	{
		if (Activate && (bool)_enemyAutoAimSystem)
		{
			BaseLogicUpdate();
			UpdateStatusLogic();
			if (ModelTransform.eulerAngles.y > selfDirection)
			{
				rot = ModelTransform.eulerAngles.y - 180f;
				rot = Mathf.Clamp(rot, 90f, 270f);
			}
			else if (ModelTransform.eulerAngles.y < selfDirection)
			{
				rot = ModelTransform.eulerAngles.y + 180f;
				rot = Mathf.Clamp(rot, 90f, 270f);
			}
		}
	}

	public void UpdateFunc()
	{
		if (Activate)
		{
			ModelTransform.localEulerAngles = Vector3.MoveTowards(ModelTransform.localEulerAngles, new Vector3(ModelTransform.localEulerAngles.x, rot, ModelTransform.localEulerAngles.z), Time.deltaTime * 5f * 45f);
			base.transform.localPosition = Vector3.MoveTowards(base.transform.localPosition, Controller.LogicPosition.vec3, distanceDelta);
			if (mainStatus == MainStatus.Fly && subStatus == SubStatus.Phase0 && Vector3.Distance(StartPos, _transform.position) >= MoveDis)
			{
				_velocity = VInt3.zero;
				flyTimer += (int)AiTimer.GetMillisecond();
				SetStatus(MainStatus.Fly, SubStatus.Phase1);
			}
		}
	}

	public override void SetActive(bool _isActive)
	{
		if (!_isActive)
		{
			base.SoundSource.PlaySE("EnemySE02", "em027_deathguard_stop");
		}
		base.SetActive(_isActive);
		if (_isActive)
		{
			_collideBullet.UpdateBulletData(EnemyWeapons[0].BulletData);
			_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			_collideBullet.Active(targetMask);
			patrolIndex = 0;
			comeBack = false;
			endPatrol = false;
			SetStatus(MainStatus.Idle);
			base.SoundSource.ActivePlaySE("EnemySE02", "em027_deathguard_lp");
		}
		else
		{
			patrolPaths = new Vector3[0];
			_collideBullet.BackToPool();
		}
	}

	protected void RegisterStatus(MainStatus _mainStatus, SubStatus _subStatus = SubStatus.Phase0)
	{
		if (StageUpdate.bIsHost)
		{
			if (bWaitNetStatus)
			{
				return;
			}
			Target = _enemyAutoAimSystem.GetClosetPlayer();
			if (Target != null)
			{
				TargetPos = Target.Controller.LogicPosition;
			}
		}
		else if (bWaitNetStatus)
		{
			bWaitNetStatus = false;
		}
		if (StageUpdate.bIsHost)
		{
			NetSyncData netSyncData = new NetSyncData();
			netSyncData.TargetPosX = TargetPos.x;
			netSyncData.TargetPosY = TargetPos.y;
			netSyncData.TargetPosZ = TargetPos.z;
			netSyncData.SelfPosX = Controller.LogicPosition.x;
			netSyncData.SelfPosY = Controller.LogicPosition.y;
			netSyncData.SelfPosZ = Controller.LogicPosition.z;
			netSyncData.nParam0 = (int)_subStatus;
			if (Target != null)
			{
				netSyncData.sParam0 = Target.sNetSerialID;
			}
			else
			{
				netSyncData.sParam0 = "";
			}
			netSyncData.sParam0 = netSyncData.sParam0 + "," + selfDirection.ToString("0.000");
			netSyncData.sParam0 = netSyncData.sParam0 + "," + patrolIndex;
			bWaitNetStatus = true;
			StageUpdate.RegisterSendAndRun(sNetSerialID, (int)_mainStatus, JsonConvert.SerializeObject(netSyncData));
		}
	}

	public override void UpdateEnemyID(int _id)
	{
		base.UpdateEnemyID(_id);
		if (null == _enemyAutoAimSystem)
		{
			OrangeBattleUtility.AddEnemyAutoAimSystem(_transform, out _enemyAutoAimSystem);
		}
		_enemyAutoAimSystem.UpdateAimRange(EnemyWeapons[1].BulletData.f_DISTANCE);
	}

	public override void UpdateStatus(int _nSet, string _smsg, Callback _callback = null)
	{
		bWaitNetStatus = false;
		if ((int)Hp <= 0)
		{
			return;
		}
		SubStatus subStatus = SubStatus.Phase0;
		if (_smsg != null && _smsg != "")
		{
			NetSyncData netSyncData = JsonConvert.DeserializeObject<NetSyncData>(_smsg);
			Controller.LogicPosition.x = netSyncData.SelfPosX;
			Controller.LogicPosition.y = netSyncData.SelfPosY;
			Controller.LogicPosition.z = netSyncData.SelfPosZ;
			TargetPos.x = netSyncData.TargetPosX;
			TargetPos.y = netSyncData.TargetPosY;
			TargetPos.z = netSyncData.TargetPosZ;
			if (netSyncData.bSetHP)
			{
				Hp = netSyncData.nHP;
			}
			subStatus = (SubStatus)netSyncData.nParam0;
			string[] array = netSyncData.sParam0.Split(',');
			Target = StageUpdate.GetPlayerByID(array[0]);
			selfDirection = float.Parse(array[1]);
			patrolIndex = int.Parse(array[2]);
		}
		SetStatus((MainStatus)_nSet, subStatus);
	}

	public override void SetPositionAndRotation(Vector3 _pos, bool _back)
	{
		if (_back)
		{
			selfDirection = 270f;
		}
		else
		{
			selfDirection = 90f;
		}
		base.transform.position = _pos;
		ModelTransform.localRotation = Quaternion.Euler(0f, selfDirection, 0f);
	}

	public override void SetPatrolPath(bool _isLoop, int _MoveSpeed, Vector3[] _paths)
	{
		base.SetPatrolPath(_isLoop, _MoveSpeed, _paths);
		patrolIsLoop = _isLoop;
		if (_MoveSpeed > 0)
		{
			flySpeed = (float)_MoveSpeed * 0.001f;
		}
		patrolPaths = new Vector3[_paths.Length];
		for (int i = 0; i < _paths.Length; i++)
		{
			patrolPaths[i] = _paths[i];
		}
	}

	protected void SetStatus(MainStatus _mainStatus, SubStatus _subStatus = SubStatus.Phase0)
	{
		mainStatus = _mainStatus;
		subStatus = _subStatus;
		switch (mainStatus)
		{
		case MainStatus.Idle:
			_velocity = VInt3.zero;
			_animator.Play(animationHash[0], 0, 0f);
			break;
		case MainStatus.Fly:
			_animator.Play(animationHash[0], 0, 0f);
			switch (subStatus)
			{
			case SubStatus.Phase0:
				StartPos = Controller.LogicPosition.vec3;
				if (patrolPaths.Length == 0)
				{
					TargetPos = Controller.LogicPosition;
				}
				else
				{
					TargetPos = new VInt3(patrolPaths[patrolIndex]);
					selfDirection = ((TargetPos.x < Controller.LogicPosition.x) ? 270f : 90f);
				}
				MoveDis = Vector3.Distance(StartPos, TargetPos.vec3);
				break;
			case SubStatus.Phase1:
				if (patrolPaths.Length <= 1)
				{
					break;
				}
				if (patrolIsLoop)
				{
					patrolIndex++;
					if (patrolIndex >= patrolPaths.Length)
					{
						patrolIndex = 0;
					}
				}
				else
				{
					if ((!comeBack && patrolIndex + 1 >= patrolPaths.Length) || (comeBack && patrolIndex == 0))
					{
						comeBack = !comeBack;
					}
					patrolIndex += ((!comeBack) ? 1 : (-1));
				}
				StartPos = Controller.LogicPosition.vec3;
				TargetPos = new VInt3(patrolPaths[patrolIndex]);
				selfDirection = ((TargetPos.x < Controller.LogicPosition.x) ? 270f : 90f);
				MoveDis = Vector3.Distance(StartPos, TargetPos.vec3);
				subStatus = SubStatus.Phase0;
				break;
			}
			break;
		case MainStatus.Shoot:
			flyTimer = 0;
			switch (subStatus)
			{
			case SubStatus.Phase0:
				EnemyWeapons[1].MagazineRemain = EnemyWeapons[1].BulletData.n_MAGAZINE;
				_animator.Play(animationHash[3], 0, 0f);
				break;
			case SubStatus.Phase1:
				_animator.Play(animationHash[1], 0, 0f);
				break;
			case SubStatus.Phase2:
				if (Target != null && EnemyWeapons[1].MagazineRemain > 0f && Vector3.Dot(Target._transform.position - modelTransform.transform.position, modelTransform.forward) > 0f)
				{
					shootDirection = (Target.AimTransform.position - shootPointTransform.position).normalized;
					BulletBase.TryShotBullet(EnemyWeapons[1].BulletData, shootPointTransform, shootDirection, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
					EnemyWeapons[1].MagazineRemain -= 1f;
				}
				break;
			case SubStatus.Phase4:
				_animator.Play(animationHash[4], 0, 0f);
				break;
			case SubStatus.Phase5:
				_animator.Play(animationHash[0], 0, 0f);
				break;
			}
			break;
		}
		AiTimer.TimerStart();
	}

	protected virtual void UpdateStatusLogic()
	{
		currentFrame = _animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
		switch (mainStatus)
		{
		case MainStatus.Idle:
			_velocity = VInt3.zero;
			RegisterStatus(MainStatus.Fly);
			break;
		case MainStatus.Fly:
			switch (subStatus)
			{
			case SubStatus.Phase0:
				if (ModelTransform.eulerAngles.y != selfDirection)
				{
					_velocity = VInt3.zero;
					break;
				}
				Vector3.Distance(TargetPos.vec3, Controller.LogicPosition.vec3);
				if (AiTimer.GetMillisecond() > EnemyWeapons[1].BulletData.n_RELOAD - flyTimer)
				{
					_velocity = VInt3.zero;
					RegisterStatus(MainStatus.Shoot);
				}
				else if (Vector3.Distance(StartPos, _transform.position) >= MoveDis)
				{
					flyTimer += (int)AiTimer.GetMillisecond();
					SetStatus(MainStatus.Fly, SubStatus.Phase1);
				}
				else
				{
					Vector3 normalized = (TargetPos - Controller.LogicPosition).vec3.normalized;
					_velocity = new VInt3(normalized * flySpeed);
				}
				break;
			case SubStatus.Phase1:
				_velocity.x = 0;
				break;
			}
			break;
		case MainStatus.Shoot:
			_velocity = VInt3.zero;
			switch (subStatus)
			{
			case SubStatus.Phase0:
				if (currentFrame >= 1f)
				{
					RegisterStatus(MainStatus.Shoot, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if (AiTimer.GetMillisecond() >= EnemyWeapons[1].BulletData.n_FIRE_SPEED)
				{
					if (Target != null && Vector3.Dot(TargetPos.vec3 - modelTransform.transform.position, modelTransform.forward) > 0f)
					{
						SetStatus(MainStatus.Shoot, SubStatus.Phase2);
					}
					else
					{
						SetStatus(MainStatus.Shoot, SubStatus.Phase3);
					}
				}
				break;
			case SubStatus.Phase2:
				if (EnemyWeapons[1].MagazineRemain > 0f)
				{
					SetStatus(MainStatus.Shoot, SubStatus.Phase1);
				}
				else
				{
					SetStatus(MainStatus.Shoot, SubStatus.Phase3);
				}
				break;
			case SubStatus.Phase3:
				if ((float)AiTimer.GetMillisecond() >= EnemyWeapons[1].MagazineRemain)
				{
					SetStatus(MainStatus.Shoot, SubStatus.Phase4);
				}
				break;
			case SubStatus.Phase4:
				if (currentFrame >= 1f)
				{
					SetStatus(MainStatus.Shoot, SubStatus.Phase5);
				}
				break;
			case SubStatus.Phase5:
				RegisterStatus(MainStatus.Fly);
				break;
			}
			break;
		case MainStatus.Hurt:
			break;
		}
	}
}
