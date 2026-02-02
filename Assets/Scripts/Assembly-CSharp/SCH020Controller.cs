using System;
using CallbackDefs;
using Newtonsoft.Json;
using StageLib;
using UnityEngine;

public class SCH020Controller : PetControllerBase
{
	public enum MainStatus
	{
		Idle = 0,
		Launch = 1,
		Ready = 2,
		Destruct = 3,
		BackToPool = 4
	}

	public delegate bool CanShootBitCallBack();

	protected string _sModelName;

	protected long _nLifeTime;

	protected long _nDebutTime;

	protected int _nBulletLv = 1;

	protected SKILL_TABLE _tSkill0_Table;

	protected WeaponStatus _wsWeaponStatus;

	protected OrangeTimer _otLifeTimer;

	protected new MainStatus _mainStatus;

	protected float _fAngle;

	protected float _fAngleSpeed = 180f;

	public CanShootBitCallBack _cbCanShoot;

	protected IAimTarget _aimTarget;

	public Action _cbShoot;

	protected override void Awake()
	{
		base.Awake();
		_otLifeTimer = OrangeTimerManager.GetTimer();
	}

	public override void SetParams(string modelName, long lifeTime, int bulletSkillId, WeaponStatus weaponStatus, long debutTime = 0L)
	{
		_sModelName = modelName;
		_nLifeTime = lifeTime;
		_nDebutTime = debutTime;
		_wsWeaponStatus = weaponStatus;
		SetFollowOffset(Vector3.zero);
		SetStatus(MainStatus.Idle);
		if (_follow_Player != null)
		{
			base.TargetMask = _follow_Player.TargetMask;
			_autoAim.targetMask = base.TargetMask;
			if (ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT.TryGetNewValue(bulletSkillId, out _tSkill0_Table))
			{
				_follow_Player.tRefPassiveskill.ReCalcuSkill(ref _tSkill0_Table);
			}
		}
	}

	public void SetSkillLv(int lv)
	{
		_nBulletLv = lv;
	}

	public void SetFollowAngle(float angle)
	{
		_fAngle = angle;
	}

	protected override void Initialize()
	{
		base.Initialize();
		InitSkill();
		_autoAim.SetEnable(false);
		_autoAim.targetMask = base.TargetMask;
		_autoAim.SetIgnoreInsideScreen(true);
		_autoAim.SetUseManualTarget(false);
	}

	protected void InitSkill()
	{
		PreloadBullet();
	}

	protected override void AfterActive()
	{
		_otLifeTimer.TimerStart();
		_autoAim.UpdateAimRange(PetWeapons[0].BulletData.f_DISTANCE);
		_autoAim.ClearTargetList();
		_autoAim.SetEnable(true);
		if (activeSE != null && activeSE.Length == 2)
		{
			base.SoundSource.PlaySE(activeSE[0], activeSE[1]);
		}
	}

	protected override void AfterDeactive()
	{
		if (unactiveSE != null && unactiveSE.Length == 2)
		{
			base.SoundSource.PlaySE(unactiveSE[0], unactiveSE[1]);
		}
	}

	public override void UpdateFunc()
	{
		if (Activate && _mainStatus == MainStatus.Launch && _follow_Player != null)
		{
			_fAngle += Time.deltaTime * _fAngleSpeed;
			Vector3 position = _follow_Player.AimPosition + Quaternion.Euler(0f, _fAngle, 0f) * Vector3.right;
			_transform.position = position;
		}
	}

	public override void LogicUpdate()
	{
		if (Activate)
		{
			base.LogicUpdate();
			switch (_mainStatus)
			{
			case MainStatus.Idle:
				LogicUpdateIdle();
				break;
			case MainStatus.Launch:
				LogicUpdateLaunch();
				break;
			case MainStatus.Ready:
				SetStatus(MainStatus.Destruct);
				break;
			case MainStatus.Destruct:
				DestructToPool();
				break;
			case MainStatus.BackToPool:
				break;
			}
		}
	}

	protected void LogicUpdateIdle()
	{
		if (_otLifeTimer.GetMillisecond() > _nDebutTime)
		{
			SetStatus(MainStatus.Launch);
		}
	}

	protected void LogicUpdateLaunch()
	{
		if (_otLifeTimer.GetMillisecond() > _nLifeTime)
		{
			SetStatus(MainStatus.Destruct);
		}
		else if ((bool)_follow_Player && _follow_Player.IsLocalPlayer)
		{
			_aimTarget = null;
			if ((bool)_follow_Player.PlayerAutoAimSystem && IsWithinRange(_follow_Player.PlayerAutoAimSystem.AutoAimTarget))
			{
				_aimTarget = _follow_Player.PlayerAutoAimSystem.AutoAimTarget;
			}
			if (_aimTarget == null && _autoAim.AutoAimTarget != null)
			{
				_aimTarget = _autoAim.AutoAimTarget;
			}
			if (_aimTarget != null && _cbCanShoot != null && _cbCanShoot())
			{
				SetStatus(MainStatus.Ready);
			}
		}
	}

	protected bool IsWithinRange(IAimTarget target)
	{
		if (target != null && Vector2.Distance(target.AimPosition, _transform.position) < PetWeapons[0].BulletData.f_DISTANCE)
		{
			return true;
		}
		return false;
	}

	protected override void UpdateFollowPos()
	{
		if (FollowEnabled)
		{
			bool flag = _follow_Player == null;
		}
	}

	protected virtual void DestructToPool()
	{
		_nBulletLv = 1;
		_nLifeTime = 0L;
		_otLifeTimer.TimerStop();
		_cbCanShoot = null;
		if (_cbShoot != null)
		{
			_cbShoot();
		}
		SetCollider2D(false);
		SetActive(false);
		MonoBehaviourSingleton<PoolManager>.Instance.BackToPool(this, _sModelName);
		SetStatus(MainStatus.Idle);
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

	public override void UpdateStatus(int nSet, string smsg, Callback tCB = null)
	{
		NetSyncData tSendb = JsonConvert.DeserializeObject<NetSyncData>(smsg);
		switch (nSet)
		{
		case 1:
			SyncLaunchStatus(tSendb);
			break;
		case 2:
			SyncReadyStatus(tSendb);
			break;
		case 3:
			SyncDestructStatus(tSendb);
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
			case MainStatus.Idle:
				break;
			}
		}
	}

	protected virtual void SetLaunchStatus()
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

	protected virtual void SyncLaunchStatus(NetSyncData tSendb)
	{
		if (!_follow_Player.IsLocalPlayer)
		{
			_mainStatus = MainStatus.Launch;
			Controller.LogicPosition.x = tSendb.SelfPosX;
			Controller.LogicPosition.y = tSendb.SelfPosY;
			Controller.LogicPosition.z = tSendb.SelfPosZ;
		}
	}

	protected virtual void SetReadyStatus()
	{
		CreateSkillBullet(_aimTarget, _aimTarget.AimPosition);
		if (_follow_Player.IsLocalPlayer)
		{
			string empty = string.Empty;
			VInt3 vInt = new VInt3(_aimTarget.AimPosition - MonoBehaviourSingleton<PoolManager>.Instance.transform.position);
			if (_aimTarget as StageObjBase != null)
			{
				empty = (_aimTarget as StageObjBase).sNetSerialID;
			}
			NetSyncData netSyncData = new NetSyncData();
			netSyncData.SelfPosX = Controller.LogicPosition.x;
			netSyncData.SelfPosY = Controller.LogicPosition.y;
			netSyncData.SelfPosZ = Controller.LogicPosition.z;
			netSyncData.TargetPosX = vInt.x;
			netSyncData.TargetPosY = vInt.y;
			netSyncData.TargetPosZ = vInt.z;
			netSyncData.sParam0 = empty;
			StageUpdate.RegisterPetSendAndRun(sNetSerialID, (int)_mainStatus, JsonConvert.SerializeObject(netSyncData), true);
		}
	}

	protected virtual void SyncReadyStatus(NetSyncData tSendb)
	{
		if (!_follow_Player.IsLocalPlayer)
		{
			_mainStatus = MainStatus.Ready;
			Controller.LogicPosition.x = tSendb.SelfPosX;
			Controller.LogicPosition.y = tSendb.SelfPosY;
			Controller.LogicPosition.z = tSendb.SelfPosZ;
			VInt3 vInt = default(VInt3);
			vInt.x = tSendb.TargetPosX;
			vInt.y = tSendb.TargetPosY;
			vInt.z = tSendb.TargetPosZ;
			_autoAim.SetTargetByNetSerialID(tSendb.sParam0);
			CreateSkillBullet(_autoAim.AutoAimTarget, vInt.vec3);
		}
	}

	protected virtual void SetDestructStatus()
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

	protected virtual void SyncDestructStatus(NetSyncData tSendb)
	{
		if (!_follow_Player.IsLocalPlayer)
		{
			_mainStatus = MainStatus.Destruct;
			Controller.LogicPosition.x = tSendb.SelfPosX;
			Controller.LogicPosition.y = tSendb.SelfPosY;
			Controller.LogicPosition.z = tSendb.SelfPosZ;
		}
	}

	protected void CreateSkillBullet(IAimTarget target, Vector3 targetPos)
	{
		Vector3 right = Vector3.right;
		SKILL_TABLE tSkillTable = ((_tSkill0_Table == null) ? PetWeapons[0].BulletData : _tSkill0_Table);
		right = ((target == null) ? (targetPos - _transform.position).normalized : (target.AimPosition - _transform.position).normalized);
		BulletBase bulletBase = ((!_follow_Player) ? BulletBase.TryShotBullet(tSkillTable, _transform, right, _wsWeaponStatus, selfBuffManager.sBuffStatus, null, base.TargetMask, false, false, true) : BulletBase.TryShotBullet(tSkillTable, _transform, right, _wsWeaponStatus, _follow_Player.selfBuffManager.sBuffStatus, null, base.TargetMask, false, false, true));
		if ((bool)bulletBase && _follow_Player != null)
		{
			bulletBase.BulletLevel = _nBulletLv;
			bulletBase.SetOwnerName(_follow_Player.sPlayerName);
		}
	}
}
