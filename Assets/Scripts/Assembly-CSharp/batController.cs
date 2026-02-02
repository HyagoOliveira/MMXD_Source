using CallbackDefs;
using Newtonsoft.Json;
using StageLib;
using UnityEngine;

public class batController : EnemyControllerBase, IManagedUpdateBehavior
{
	public float FlySpeed = 3f;

	protected readonly int _hashHspd = Animator.StringToHash("fHspd");

	protected Vector3 _direction;

	protected Vector3 dirLeft = new Vector3(0f, 180f, 0f);

	protected Vector3 dirRight = new Vector3(0f, 135f, 0f);

	[SerializeField]
	private float Mob02MaxDis = 8f;

	[SerializeField]
	private float Mob02MinDis = 3f;

	[SerializeField]
	private float Mob02CD = 3f;

	private int Mob02CDFrame;

	[SerializeField]
	private float Mob02IdleCD = 0.5f;

	private int Mob02IdleCDFrame;

	private ParticleSystem Mob002FX;

	public override void UpdateEnemyID(int id)
	{
		base.UpdateEnemyID(id);
		UpdateAIState();
		AI_STATE aiState = AiState;
		int num = 1;
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
		_animator = GetComponentInChildren<Animator>();
		ModelTransform = OrangeBattleUtility.FindChildRecursive(_transform, "model", true);
		_collideBullet = OrangeBattleUtility.FindChildRecursive(_transform, "CollideBullet", true).gameObject.AddOrGetComponent<CollideBullet>();
		base.AimTransform = OrangeBattleUtility.FindChildRecursive(_transform, "body_ctrl", true);
		if (null == _enemyAutoAimSystem)
		{
			OrangeBattleUtility.AddEnemyAutoAimSystem(_transform, out _enemyAutoAimSystem);
		}
		_enemyAutoAimSystem.UpdateAimRange(30f);
		IgnoreGravity = true;
	}

	public override void UpdateStatus(int nSet, string smsg, Callback tCB = null)
	{
		if (smsg != null && smsg != "")
		{
			NetSyncData netSyncData = JsonConvert.DeserializeObject<NetSyncData>(smsg);
			TargetPos.x = netSyncData.TargetPosX;
			TargetPos.y = netSyncData.TargetPosY;
			TargetPos.z = netSyncData.TargetPosZ;
			Controller.LogicPosition.x = netSyncData.SelfPosX;
			Controller.LogicPosition.y = netSyncData.SelfPosY;
			Controller.LogicPosition.z = netSyncData.SelfPosZ;
			_transform.position = Controller.LogicPosition.vec3;
		}
		_direction = (TargetPos - Controller.LogicPosition).vec3.normalized;
		_velocity = new VInt3(_direction * FlySpeed);
		ModelTransform.eulerAngles = ((_direction.x < 0f) ? dirLeft : dirRight);
		bWaitNetStatus = false;
		AiTimer.TimerStart();
	}

	private void UpdateRandomState()
	{
		if (StageUpdate.bIsHost)
		{
			if (bWaitNetStatus)
			{
				return;
			}
			TargetPos = VInt3.zero;
			_direction = TargetPos.vec3;
			_velocity = new VInt3(_direction * FlySpeed);
			VInt3 zero = VInt3.zero;
			OrangeCharacter orangeCharacter = null;
			orangeCharacter = StageUpdate.GetNearestPlayerByVintPos(Controller.LogicPosition, (int)fAIWorkRange);
			if (orangeCharacter != null)
			{
				NetSyncData netSyncData = new NetSyncData();
				if (orangeCharacter.UsingVehicle && orangeCharacter.refRideBaseObj != null)
				{
					netSyncData.TargetPosX = orangeCharacter.refRideBaseObj.Controller.LogicPosition.x;
					netSyncData.TargetPosY = orangeCharacter.refRideBaseObj.Controller.LogicPosition.y;
					netSyncData.TargetPosZ = orangeCharacter.refRideBaseObj.Controller.LogicPosition.z;
				}
				else
				{
					netSyncData.TargetPosX = orangeCharacter.Controller.LogicPosition.x;
					netSyncData.TargetPosY = orangeCharacter.Controller.LogicPosition.y;
					netSyncData.TargetPosZ = orangeCharacter.Controller.LogicPosition.z;
				}
				netSyncData.SelfPosX = Controller.LogicPosition.x;
				netSyncData.SelfPosY = Controller.LogicPosition.y;
				netSyncData.SelfPosZ = Controller.LogicPosition.z;
				bWaitNetStatus = true;
				StageUpdate.RegisterSendAndRun(sNetSerialID, 0, JsonConvert.SerializeObject(netSyncData));
			}
		}
		else
		{
			bWaitNetStatus = false;
		}
	}

	public override void LogicUpdate()
	{
		if (!Activate)
		{
			return;
		}
		base.LogicUpdate();
		BaseUpdate();
		AI_STATE aiState = AiState;
		if (aiState == AI_STATE.mob_002)
		{
			if (!Target)
			{
				Target = _enemyAutoAimSystem.GetClosetPlayer();
			}
			if (!Target)
			{
				for (int i = 0; i < StageUpdate.runPlayers.Count; i++)
				{
					if ((bool)StageUpdate.runPlayers[i])
					{
						Target = StageUpdate.runPlayers[i];
						break;
					}
				}
			}
			if ((bool)Target)
			{
				TargetPos = Target.Controller.LogicPosition;
			}
			if (GameLogicUpdateManager.GameFrame > Mob02CDFrame && Vector2.Distance(TargetPos.vec3, _transform.position) < Mob02MaxDis && Vector2.Distance(TargetPos.vec3, _transform.position) > Mob02MinDis)
			{
				Mob02CDFrame = GameLogicUpdateManager.GameFrame + (int)(Mob02CD * 20f);
				Mob02IdleCDFrame = GameLogicUpdateManager.GameFrame + (int)(Mob02IdleCD * 20f);
				_velocity = VInt3.zero;
				_direction = (TargetPos - Controller.LogicPosition).vec3.normalized;
				BulletBase.TryShotBullet(EnemyWeapons[1].BulletData, _transform.position + Vector3.up * 0.25f, _direction, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
			}
			else if (GameLogicUpdateManager.GameFrame >= Mob02IdleCDFrame && AiTimer.GetMillisecond() > EnemyData.n_AI_TIMER)
			{
				UpdateRandomState();
			}
		}
		else if (AiTimer.GetMillisecond() > EnemyData.n_AI_TIMER)
		{
			UpdateRandomState();
		}
		_velocityExtra = VInt3.zero;
		_velocityShift = VInt3.zero;
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
		base.SetActive(isActive);
		if (isActive)
		{
			ModelTransform.eulerAngles = ((_direction.x < 0f) ? new Vector3(0f, 180f, 0f) : new Vector3(0f, 135f, 0f));
			bWaitNetStatus = false;
			_animator.enabled = true;
			_collideBullet.UpdateBulletData(EnemyWeapons[0].BulletData);
			_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			_collideBullet.isForceSE = true;
			_collideBullet.Active(targetMask);
			AiTimer.TimerStart();
			AiTimer.SetMillisecondsOffset(EnemyData.n_AI_TIMER + 1);
			AI_STATE aiState = AiState;
			if (aiState == AI_STATE.mob_002)
			{
				Mob002FX = OrangeBattleUtility.FindChildRecursive(_transform, "Mob002FX", true).gameObject.AddOrGetComponent<ParticleSystem>();
				if (Mob002FX != null)
				{
					Mob002FX.Play();
				}
			}
		}
		else
		{
			Target = null;
			TargetPos = VInt3.zero;
			_direction = TargetPos.vec3;
			_velocity = new VInt3(_direction * FlySpeed);
			_collideBullet.BackToPool();
			_animator.enabled = false;
			AI_STATE aiState = AiState;
			if (aiState == AI_STATE.mob_002 && Mob002FX != null)
			{
				Mob002FX.Clear();
				Mob002FX.Stop();
			}
		}
	}

	public override void SetPositionAndRotation(Vector3 pos, bool bBack)
	{
		_transform.SetPositionAndRotation(pos, Quaternion.identity);
		Controller.LogicPosition = new VInt3(_transform.position);
		_direction = (bBack ? Vector3.left : Vector3.right);
		_velocity = new VInt3(_direction * FlySpeed);
		ModelTransform.eulerAngles = ((_direction.x < 0f) ? new Vector3(0f, 180f, 0f) : new Vector3(0f, 135f, 0f));
	}
}
