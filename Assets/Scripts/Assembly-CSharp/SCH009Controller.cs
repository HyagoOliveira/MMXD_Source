using System;
using CallbackDefs;
using Newtonsoft.Json;
using StageLib;
using UnityEngine;

public class SCH009Controller : PetControllerBase
{
	public enum MainStatus
	{
		Idle = 0,
		Launch = 1,
		Ready = 2,
		Destruct = 3,
		ShootCommand = 10
	}

	protected long _nLifeTime;

	protected OrangeTimer _lifeTimer;

	[SerializeField]
	protected float _moveSpeed = 10f;

	protected new MainStatus _mainStatus;

	protected VInt3 _movePosition;

	public override string[] GetPetDependAnimations()
	{
		return new string[1] { "SCH009@step_2_loop" };
	}

	protected override void Awake()
	{
		base.Awake();
		_lifeTimer = OrangeTimerManager.GetTimer();
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_subchr_000", 2);
	}

	public void SetParam(long lifeTime, float moveSpeed)
	{
		_nLifeTime = lifeTime;
		_moveSpeed = moveSpeed;
		base.TargetMask = _follow_Player.TargetMask;
		PreloadBullet();
	}

	public override void SetActive(bool isActive)
	{
		Controller.enabled = isActive;
		Activate = isActive;
		_velocity = VInt3.zero;
		_velocityExtra = VInt3.zero;
		base.transform.gameObject.SetActive(isActive);
		if (isActive)
		{
			_lifeTimer.TimerStart();
			_autoAim.UpdateAimRange(PetWeapons[0].BulletData.f_DISTANCE);
			_autoAim.SetEnable(true);
			_autoAim.targetMask = base.TargetMask;
			for (int i = 0; i < EquippedWeaponNum; i++)
			{
				PetWeapons[i].LastUseTimer.TimerStart();
				PetWeapons[i].MagazineRemain = PetWeapons[i].BulletData.n_MAGAZINE;
			}
			SetStatus(MainStatus.Launch);
		}
		else
		{
			_autoAim.SetEnable(false);
			_autoAim.ClearTargetList();
		}
	}

	public void FollowPlayerDead()
	{
		SetStatus(MainStatus.Destruct);
	}

	public override void UpdateStatus(int nSet, string smsg, Callback tCB = null)
	{
		NetSyncData tSendb = JsonConvert.DeserializeObject<NetSyncData>(smsg);
		switch (nSet)
		{
		case 1:
			SyncLunckStatus(tSendb);
			break;
		case 2:
			SyncReadyStatus(tSendb);
			break;
		case 3:
			SyncDestructStatus(tSendb);
			break;
		case 10:
			SyncShootCommand(tSendb);
			break;
		}
	}

	protected void SetStatus(MainStatus status)
	{
		if (_mainStatus != status)
		{
			_mainStatus = status;
			switch (_mainStatus)
			{
			case MainStatus.Launch:
				SetLaunchStatus();
				break;
			case MainStatus.Ready:
				SetReadyStatus();
				break;
			case MainStatus.Destruct:
				SetDestructStatus();
				break;
			}
		}
	}

	private void SetLaunchStatus()
	{
		if (_follow_Player.IsLocalPlayer)
		{
			_movePosition = new VInt3(_transform.position);
			NetSyncData netSyncData = new NetSyncData();
			netSyncData.SelfPosX = Controller.LogicPosition.x;
			netSyncData.SelfPosY = Controller.LogicPosition.y;
			netSyncData.SelfPosZ = Controller.LogicPosition.z;
			netSyncData.TargetPosX = _movePosition.x;
			netSyncData.TargetPosY = _movePosition.y;
			netSyncData.TargetPosZ = _movePosition.z;
			StageUpdate.RegisterPetSendAndRun(sNetSerialID, (int)_mainStatus, JsonConvert.SerializeObject(netSyncData), true);
		}
		SetAnimateId(PetHumanBase.PetAnimateId.ANI_SKILL_START);
	}

	private void SyncLunckStatus(NetSyncData tSendb)
	{
		if (!_follow_Player.IsLocalPlayer)
		{
			_mainStatus = MainStatus.Launch;
			Controller.LogicPosition.x = tSendb.SelfPosX;
			Controller.LogicPosition.y = tSendb.SelfPosY;
			Controller.LogicPosition.z = tSendb.SelfPosZ;
		}
		_movePosition.x = tSendb.TargetPosX;
		_movePosition.y = tSendb.TargetPosY;
		_movePosition.z = tSendb.TargetPosZ;
	}

	private void SetReadyStatus()
	{
		if (_follow_Player.IsLocalPlayer)
		{
			NetSyncData netSyncData = new NetSyncData();
			netSyncData.SelfPosX = Controller.LogicPosition.x;
			netSyncData.SelfPosY = Controller.LogicPosition.y;
			netSyncData.SelfPosZ = Controller.LogicPosition.z;
			StageUpdate.RegisterPetSendAndRun(sNetSerialID, (int)_mainStatus, JsonConvert.SerializeObject(netSyncData), true);
		}
		_velocity = VInt3.zero;
	}

	private void SyncReadyStatus(NetSyncData tSendb)
	{
		if (!_follow_Player.IsLocalPlayer)
		{
			_mainStatus = MainStatus.Ready;
			Controller.LogicPosition.x = tSendb.SelfPosX;
			Controller.LogicPosition.y = tSendb.SelfPosY;
			Controller.LogicPosition.z = tSendb.SelfPosZ;
		}
		_velocity = VInt3.zero;
	}

	private void SetDestructStatus()
	{
		if (_follow_Player.IsLocalPlayer)
		{
			NetSyncData netSyncData = new NetSyncData();
			netSyncData.SelfPosX = Controller.LogicPosition.x;
			netSyncData.SelfPosY = Controller.LogicPosition.y;
			netSyncData.SelfPosZ = Controller.LogicPosition.z;
			StageUpdate.RegisterPetSendAndRun(sNetSerialID, (int)_mainStatus, JsonConvert.SerializeObject(netSyncData), true);
		}
	}

	private void SyncDestructStatus(NetSyncData tSendb)
	{
		if (!_follow_Player.IsLocalPlayer)
		{
			_mainStatus = MainStatus.Destruct;
			Controller.LogicPosition.x = tSendb.SelfPosX;
			Controller.LogicPosition.y = tSendb.SelfPosY;
			Controller.LogicPosition.z = tSendb.SelfPosZ;
		}
	}

	private void SyncShootCommand(NetSyncData tSendb)
	{
		if (!_follow_Player.IsLocalPlayer)
		{
			_mainStatus = MainStatus.Ready;
			Controller.LogicPosition.x = tSendb.SelfPosX;
			Controller.LogicPosition.y = tSendb.SelfPosY;
			Controller.LogicPosition.z = tSendb.SelfPosZ;
			if (tSendb.sParam0 != "")
			{
				_autoAim.SetTargetByNetSerialID(tSendb.sParam0);
				_autoAim.SetUpdate(false);
				if (_autoAim.AutoAimTarget != null)
				{
					UpdateSkillCD(0);
					CreateSkillBullet();
				}
			}
		}
		_velocity = VInt3.zero;
	}

	public override void UpdateFunc()
	{
		if (Activate)
		{
			_transform.localPosition = Vector3.MoveTowards(_transform.localPosition, Controller.LogicPosition.vec3, distanceDelta);
			_currentFrame = 0f;
		}
	}

	public override void UpdateDirecion()
	{
		if (_autoAim.AutoAimTarget != null)
		{
			int num = new VInt3(_autoAim.GetTargetPoint()).x - Controller.LogicPosition.x;
			if (num > 0 && direction != 1)
			{
				direction = 1;
				SetPositionAndRotation(_transform.position, false);
			}
			else if (num < 0 && direction != -1)
			{
				direction = -1;
				SetPositionAndRotation(_transform.position, true);
			}
		}
	}

	public override void LogicUpdate()
	{
		if (!Activate)
		{
			return;
		}
		base.LogicUpdate();
		switch (_mainStatus)
		{
		case MainStatus.Launch:
			if (Mathf.Abs(Controller.LogicPosition.x - _movePosition.x) < 500 && Mathf.Abs(Controller.LogicPosition.y - _movePosition.y) < 500)
			{
				SetStatus(MainStatus.Ready);
			}
			else
			{
				UpdateMove();
			}
			break;
		case MainStatus.Ready:
			UpadteReady();
			break;
		case MainStatus.Destruct:
			UpdateDestruct();
			break;
		}
		if (_lifeTimer.GetMillisecond() > _nLifeTime)
		{
			SetStatus(MainStatus.Destruct);
		}
	}

	protected void UpdateMove()
	{
		VInt3 vInt = _movePosition - Controller.LogicPosition;
		vInt.z = 0;
		_velocity.x = (int)(vInt.vec3.normalized.x * _moveSpeed * 1000f);
		_velocity.y = (int)(vInt.vec3.normalized.y * _moveSpeed * 1000f);
		if ((float)Mathf.Abs(vInt.x) < Mathf.Abs((float)_velocity.x * GameLogicUpdateManager.m_fFrameLen))
		{
			_velocity.x = vInt.x * 5;
		}
		if ((float)Mathf.Abs(vInt.y) < Mathf.Abs((float)_velocity.y * GameLogicUpdateManager.m_fFrameLen))
		{
			_velocity.y = vInt.y * 5;
		}
	}

	protected void UpadteReady()
	{
		if (!_follow_Player.IsLocalPlayer)
		{
			return;
		}
		_autoAim.UpdatePriorityTarget();
		_autoAim.SetUpdate(true);
		if (PetWeapons[0].LastUseTimer.GetMillisecond() < PetWeapons[0].BulletData.n_FIRE_SPEED)
		{
			return;
		}
		if (PetWeapons[0].MagazineRemain > 0f)
		{
			if (_autoAim.AutoAimTarget != null)
			{
				IAimTarget autoAimTarget = _autoAim.AutoAimTarget;
				string empty = string.Empty;
				if (autoAimTarget as StageObjBase != null)
				{
					empty = (autoAimTarget as StageObjBase).sNetSerialID;
				}
				NetSyncData netSyncData = new NetSyncData();
				netSyncData.SelfPosX = Controller.LogicPosition.x;
				netSyncData.SelfPosY = Controller.LogicPosition.y;
				netSyncData.SelfPosZ = Controller.LogicPosition.z;
				netSyncData.sParam0 = empty;
				StageUpdate.RegisterPetSendAndRun(sNetSerialID, 10, JsonConvert.SerializeObject(netSyncData), true);
				UpdateSkillCD(0);
				CreateSkillBullet();
			}
		}
		else
		{
			SetStatus(MainStatus.Destruct);
		}
	}

	private void UpdateDestruct()
	{
		MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_subchr_000", base.transform.position, Quaternion.identity, Array.Empty<object>());
		SetActive(false);
	}

	protected void CreateSkillBullet()
	{
		Vector3 normalized = (_autoAim.GetTargetPoint() - base.transform.position).normalized;
		BulletBase bulletBase = BulletBase.TryShotBullet(PetWeapons[0].BulletData, _transform, normalized, _follow_Player.PlayerSkills[follow_skill_id].weaponStatus, selfBuffManager.sBuffStatus, null, base.TargetMask, false, false, true);
		if ((bool)bulletBase && _follow_Player != null)
		{
			bulletBase.BulletLevel = _follow_Player.PlayerSkills[follow_skill_id].SkillLV;
			bulletBase.SetOwnerName(_follow_Player.sPlayerName);
		}
	}

	public override void UpdateSkillCD(int skillIndex)
	{
		PetWeapons[skillIndex].MagazineRemain -= PetWeapons[skillIndex].BulletData.n_USE_COST;
		PetWeapons[skillIndex].LastUseTimer.TimerStart();
	}

	private void PreloadBullet()
	{
		PetTable = ManagedSingleton<OrangeDataManager>.Instance.PET_TABLE_DICT[PetID];
		SKILL_TABLE[] petAllSkillData = ManagedSingleton<OrangeTableHelper>.Instance.GetPetAllSkillData(PetTable);
		foreach (SKILL_TABLE p_table in petAllSkillData)
		{
			if (!(p_table.s_MODEL == "DUMMY") && !MonoBehaviourSingleton<PoolManager>.Instance.ExistsInPool<BasicBullet>(p_table.s_MODEL))
			{
				MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad("prefab/bullet/" + p_table.s_MODEL, p_table.s_MODEL, delegate(GameObject obj)
				{
					BulletBase component = obj.GetComponent<BulletBase>();
					MonoBehaviourSingleton<PoolManager>.Instance.CreatePoolBaseLocal<BulletBase>(UnityEngine.Object.Instantiate(component), p_table.s_MODEL, 5);
				});
			}
		}
	}
}
