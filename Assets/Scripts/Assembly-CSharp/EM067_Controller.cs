using CallbackDefs;
using NaughtyAttributes;
using Newtonsoft.Json;
using StageLib;
using UnityEngine;

public class EM067_Controller : EnemyControllerBase, IManagedUpdateBehavior
{
	private enum MainStatus
	{
		Idle = 0,
		Walk = 1,
		Punch = 2,
		DashAttack = 3
	}

	private enum SubStatus
	{
		Phase0 = 0,
		Phase1 = 1,
		Phase2 = 2,
		Phase3 = 3,
		Phase4 = 4,
		MAX_SUBSTATUS = 5
	}

	public enum AnimationID
	{
		ANI_IDLE = 0,
		ANI_WALK = 1,
		ANI_ATTACK = 2,
		ANI_DASH_ATTACK_START = 3,
		ANI_DASH_ATTACK_LOOP = 4,
		ANI_DASH_ATTACK_END = 5,
		MAX_ANIMATION_ID = 6
	}

	[SerializeField]
	private Transform modelTransform;

	[SerializeField]
	private Transform punchHandTransform;

	[SerializeField]
	private ParticleSystem fxDashAttack;

	[SerializeField]
	private int walkSpeed = 1500;

	[SerializeField]
	private int dashSpeed = 20000;

	[SerializeField]
	private int punchDistance = 2500;

	[SerializeField]
	private int dashDistance = 6000;

	[SerializeField]
	private float aimRange = 30f;

	[SerializeField]
	[NaughtyAttributes.ReadOnly]
	private MainStatus mainStatus;

	[SerializeField]
	[NaughtyAttributes.ReadOnly]
	private SubStatus subStatus;

	[SerializeField]
	[NaughtyAttributes.ReadOnly]
	private AnimationID _currentAnimationId;

	private int[] animationHash;

	private CollideBullet punchCollideBullet;

	private int bulletCount;

	private int currentDistance;

	private int walkTime;

	private bool tempFlag;

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
		_animator = GetComponentInChildren<Animator>();
		_collideBullet = base.gameObject.AddOrGetComponent<CollideBullet>();
		punchCollideBullet = punchHandTransform.gameObject.AddOrGetComponent<CollideBullet>();
		animationHash = new int[6];
		animationHash[0] = Animator.StringToHash("EM067@stand_loop");
		animationHash[1] = Animator.StringToHash("EM067@walk_loop");
		animationHash[2] = Animator.StringToHash("EM067@stand_attack");
		animationHash[3] = Animator.StringToHash("EM067@dash_atk_start");
		animationHash[4] = Animator.StringToHash("EM067@dash_atk_loop");
		animationHash[5] = Animator.StringToHash("EM067@dash_atk_end");
		FallDownSE = new string[2] { "EnemySE02", "em036_ride02" };
		SetStatus(MainStatus.Idle);
		base.AimPoint = new Vector3(0f, 1f, 0f);
		OrangeBattleUtility.AddEnemyAutoAimSystem(_transform, out _enemyAutoAimSystem);
		_enemyAutoAimSystem.UpdateAimRange(aimRange);
	}

	public override void SetActive(bool _isActive)
	{
		base.SetActive(_isActive);
		_animator.enabled = _isActive;
		if (_isActive)
		{
			_collideBullet.UpdateBulletData(EnemyWeapons[0].BulletData);
			_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			_collideBullet.Active(targetMask);
			punchCollideBullet.UpdateBulletData(EnemyWeapons[1].BulletData);
			punchCollideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
		}
		else
		{
			_collideBullet.BackToPool();
		}
	}

	public override void UpdateStatus(int _nSet, string _smsg, Callback _tCB = null)
	{
		bWaitNetStatus = false;
		if ((int)Hp <= 0)
		{
			return;
		}
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
			UpdateDirection();
		}
		SetStatus((MainStatus)_nSet);
	}

	public override bool CheckActStatus(int _mainstatus, int _substatus)
	{
		if (_substatus == -1 && mainStatus == (MainStatus)_mainstatus)
		{
			return true;
		}
		if (mainStatus == (MainStatus)_mainstatus && subStatus == (SubStatus)_substatus)
		{
			return true;
		}
		return false;
	}

	private void UpdateDirection(int _forceDirection = 0)
	{
		if (_forceDirection != 0)
		{
			base.direction = _forceDirection;
		}
		else if (TargetPos.x > Controller.LogicPosition.x)
		{
			base.direction = 1;
		}
		else
		{
			base.direction = -1;
		}
		modelTransform.localScale = new Vector3(modelTransform.localScale.x, modelTransform.localScale.y, Mathf.Abs(modelTransform.localScale.z) * (float)base.direction);
	}

	private void SetStatus(MainStatus _mainStatus, SubStatus _subStatus = SubStatus.Phase0)
	{
		mainStatus = _mainStatus;
		subStatus = _subStatus;
		switch (mainStatus)
		{
		case MainStatus.Idle:
			_velocity.x = 0;
			_animator.Play(animationHash[0], 0, 0f);
			break;
		case MainStatus.Walk:
			walkTime = Random.Range(1000, 3000);
			tempFlag = false;
			break;
		case MainStatus.Punch:
			_velocity.x = 0;
			if (subStatus == SubStatus.Phase0)
			{
				bulletCount = 0;
				_animator.Play(animationHash[2], 0, 0f);
			}
			break;
		case MainStatus.DashAttack:
			switch (subStatus)
			{
			case SubStatus.Phase0:
				PlaySE(EnemySE02.CRI_ENEMYSE02_EM036_RIDE03);
				UpdateDirection();
				_animator.Play(animationHash[3], 0, 0f);
				break;
			case SubStatus.Phase1:
				_animator.Play(animationHash[4], 0, 0f);
				break;
			case SubStatus.Phase2:
				_animator.Play(animationHash[5], 0, 0f);
				break;
			case SubStatus.Phase4:
				fxDashAttack.Stop();
				punchCollideBullet.BackToPool();
				break;
			}
			break;
		}
		AiTimer.TimerStart();
	}

	public override void LogicUpdate()
	{
		float normalizedTime = _animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
		if (Activate)
		{
			base.LogicUpdate();
			switch (mainStatus)
			{
			case MainStatus.Idle:
				Target = _enemyAutoAimSystem.GetClosetPlayer();
				if ((bool)Target)
				{
					UpdateRandomState();
				}
				break;
			case MainStatus.Walk:
				Target = _enemyAutoAimSystem.GetClosetPlayer();
				if (Target != null)
				{
					UpdateDirection();
					_velocity.x = base.direction * walkSpeed;
					if (CheckMoveFall(_velocity))
					{
						_velocity.x = 0;
						SetStatus(MainStatus.Idle);
						break;
					}
					if (!tempFlag)
					{
						_animator.Play(animationHash[1], 0, 0f);
						tempFlag = true;
					}
					Target = _enemyAutoAimSystem.GetClosetPlayer();
					TargetPos = Target.Controller.LogicPosition;
					if (AiTimer.GetMillisecond() > walkTime)
					{
						SetStatus(MainStatus.Idle);
					}
					else if (Mathf.Abs(TargetPos.x - Controller.LogicPosition.x) < punchDistance)
					{
						SetStatus(MainStatus.Punch);
					}
				}
				else
				{
					SetStatus(MainStatus.Idle);
				}
				break;
			case MainStatus.Punch:
				switch (subStatus)
				{
				case SubStatus.Phase0:
					if (normalizedTime >= 1f)
					{
						SetStatus(mainStatus, SubStatus.Phase1);
					}
					else if (normalizedTime > 0.65f)
					{
						if (punchCollideBullet.IsActivate)
						{
							punchCollideBullet.BackToPool();
						}
					}
					else if (normalizedTime > 0.15f && bulletCount == 0)
					{
						bulletCount++;
						punchCollideBullet.Active(targetMask);
					}
					break;
				case SubStatus.Phase1:
					if (AiTimer.GetMillisecond() > 500)
					{
						punchCollideBullet.BackToPool();
						SetStatus(MainStatus.Idle);
					}
					break;
				}
				break;
			case MainStatus.DashAttack:
				switch (subStatus)
				{
				case SubStatus.Phase0:
					if (normalizedTime >= 1f)
					{
						SetStatus(mainStatus, SubStatus.Phase1);
					}
					break;
				case SubStatus.Phase1:
					if (AiTimer.GetMillisecond() > 500)
					{
						SetStatus(mainStatus, SubStatus.Phase2);
					}
					break;
				case SubStatus.Phase2:
					if (!fxDashAttack.isPlaying)
					{
						punchHandTransform.localPosition = Vector3.zero;
						punchHandTransform.localRotation = Quaternion.identity;
						fxDashAttack.Play();
					}
					currentDistance = 0;
					if (CheckMoveFall(_velocity))
					{
						_velocity.x = 0;
					}
					else
					{
						_velocity.x = base.direction * dashSpeed;
					}
					punchCollideBullet.Active(targetMask);
					SetStatus(mainStatus, SubStatus.Phase3);
					break;
				case SubStatus.Phase3:
					currentDistance += (int)((float)((_velocity.x < 0) ? (-_velocity.x) : _velocity.x) * GameLogicUpdateManager.m_fFrameLen);
					if (currentDistance >= dashDistance || CheckMoveFall(_velocity) || _velocity.x == 0)
					{
						_velocity.x = 0;
						SetStatus(mainStatus, SubStatus.Phase4);
					}
					break;
				case SubStatus.Phase4:
					if (AiTimer.GetMillisecond() > 500)
					{
						SetStatus(MainStatus.Idle);
					}
					break;
				}
				break;
			}
		}
		else
		{
			BaseUpdate();
			UpdateGravity();
			Controller.Move((_velocity + _velocityExtra) * GameLogicUpdateManager.m_fFrameLen + _velocityShift);
			distanceDelta = Vector3.Distance(base.transform.localPosition, Controller.LogicPosition.vec3) * (Time.deltaTime / GameLogicUpdateManager.m_fFrameLen);
			_velocityExtra = VInt3.zero;
			_velocityShift = VInt3.zero;
		}
	}

	public void UpdateFunc()
	{
		if (CheckMoveFall(_velocity))
		{
			_velocity.x = 0;
		}
		base.transform.localPosition = Vector3.MoveTowards(base.transform.localPosition, Controller.LogicPosition.vec3, distanceDelta);
	}

	private void UpdateRandomState()
	{
		MainStatus nSetKey = MainStatus.Idle;
		if (StageUpdate.bIsHost)
		{
			if (bWaitNetStatus)
			{
				return;
			}
			Target = _enemyAutoAimSystem.GetClosetPlayer();
			TargetPos = Target.Controller.LogicPosition;
			int num = Mathf.Abs(TargetPos.x - Controller.LogicPosition.x);
			nSetKey = ((num < punchDistance) ? MainStatus.Punch : ((num >= dashDistance) ? MainStatus.Walk : ((Random.Range(0, 100) >= 50) ? MainStatus.Walk : MainStatus.DashAttack)));
		}
		else if (bWaitNetStatus)
		{
			bWaitNetStatus = false;
		}
		if (StageUpdate.gbIsNetGame)
		{
			if (StageUpdate.bIsHost)
			{
				NetSyncData netSyncData = new NetSyncData();
				netSyncData.TargetPosX = TargetPos.x;
				netSyncData.TargetPosY = TargetPos.y;
				netSyncData.TargetPosZ = TargetPos.z;
				netSyncData.SelfPosX = Controller.LogicPosition.x;
				netSyncData.SelfPosY = Controller.LogicPosition.y;
				netSyncData.SelfPosZ = Controller.LogicPosition.z;
				bWaitNetStatus = true;
				StageUpdate.RegisterSendAndRun(sNetSerialID, (int)nSetKey, JsonConvert.SerializeObject(netSyncData));
			}
		}
		else
		{
			UpdateDirection();
			SetStatus(nSetKey);
		}
	}

	public override void SetPositionAndRotation(Vector3 _pos, bool _back)
	{
		if (_back)
		{
			base.direction = -1;
		}
		else
		{
			base.direction = 1;
		}
		modelTransform.localScale = new Vector3(_transform.localScale.x, _transform.localScale.y, Mathf.Abs(_transform.localScale.z) * (float)base.direction);
		base.transform.position = _pos;
	}

	private void DestoryReset()
	{
		if (!Controller.Collisions.below)
		{
			IgnoreGravity = true;
		}
		_collideBullet.BackToPool();
		base.AllowAutoAim = false;
		fxDashAttack.Stop();
		_velocity = VInt3.zero;
	}
}
