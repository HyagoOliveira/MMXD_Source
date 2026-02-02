using System;
using System.Collections.Generic;
using CallbackDefs;
using Newtonsoft.Json;
using StageLib;
using UnityEngine;

public class SCH016Controller : PetControllerBase
{
	public enum MainStatus
	{
		Idle = 0,
		Launch = 1,
		Ready = 2,
		Destruct = 3,
		BackToPool = 4
	}

	protected string _sModelName;

	protected long _nLifeTime;

	protected long _nDebutTime;

	protected int _nBulletLv = 1;

	protected SKILL_TABLE _tSkill0_Table;

	protected WeaponStatus _wsWeaponStatus;

	protected OrangeTimer _otLifeTimer;

	protected new MainStatus _mainStatus;

	protected HashSet<Transform> _hitList = new HashSet<Transform>();

	protected Transform _tfTrapCollider;

	protected Rigidbody2D _rigidbody2D;

	protected Collider2D _cTrapCollider;

	protected ParticleSystem _psBody;

	protected SCH016CustomBody _customBody;

	public string[] sExplodeSE;

	public string[] sActiveSE2;

	public string DestructFx { get; set; } = string.Empty;


	public string[] DestructSE { get; set; }

	protected override void Awake()
	{
		base.Awake();
		Transform transform = OrangeBattleUtility.FindChildRecursive(base.transform, "TrapCollider", true);
		if (transform == null)
		{
			GameObject obj = new GameObject();
			obj.transform.SetParent(base.transform);
			obj.transform.localPosition = Vector3.zero;
			obj.transform.eulerAngles = Vector3.zero;
			obj.transform.localScale = Vector3.one;
			obj.name = "TrapCollider";
			transform = obj.transform;
		}
		transform.gameObject.AddOrGetComponent<SCH016TrapTrigger>().SCH016Controller = this;
		_tfTrapCollider = transform;
		_rigidbody2D = base.gameObject.AddOrGetComponent<Rigidbody2D>();
		_rigidbody2D.bodyType = RigidbodyType2D.Kinematic;
		_rigidbody2D.useFullKinematicContacts = true;
		_psBody = base.transform.GetComponentInChildren<ParticleSystem>();
		_customBody = base.transform.GetComponentInChildren<SCH016CustomBody>();
		SetVisible(false);
		_otLifeTimer = OrangeTimerManager.GetTimer();
	}

	public override void SetParams(string modelName, long lifeTime, int bulletSkillId, WeaponStatus weaponStatus, long debutTime = 0L)
	{
		_sModelName = modelName;
		_nLifeTime = lifeTime;
		_nDebutTime = debutTime;
		_wsWeaponStatus = weaponStatus;
		SetFollowOffset(Vector3.zero);
		if (_follow_Player != null)
		{
			base.TargetMask = _follow_Player.TargetMask;
			base.transform.position = _follow_Player.transform.position;
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

	protected void SetSize(SKILL_TABLE skillTable)
	{
		if (skillTable == null || !(skillTable.s_FIELD != "null"))
		{
			return;
		}
		string[] array = skillTable.s_FIELD.Split(',');
		string text = array[0];
		if (!(text == "0"))
		{
			if (text == "1")
			{
				_cTrapCollider = _tfTrapCollider.gameObject.AddOrGetComponent<CircleCollider2D>();
				((CircleCollider2D)_cTrapCollider).radius = float.Parse(array[3]);
				_cTrapCollider.offset = new Vector2(float.Parse(array[1]), float.Parse(array[2]));
			}
		}
		else
		{
			_cTrapCollider = _tfTrapCollider.gameObject.AddOrGetComponent<BoxCollider2D>();
			((BoxCollider2D)_cTrapCollider).size = new Vector2(float.Parse(array[3]), float.Parse(array[4]));
		}
		_cTrapCollider.isTrigger = true;
		_cTrapCollider.enabled = true;
	}

	protected override void Initialize()
	{
		base.Initialize();
		InitSkill();
	}

	protected void UpdateHitBox()
	{
		int n_TYPE = PetTable.n_TYPE;
		if ((uint)(n_TYPE - 2) <= 1u)
		{
			SetCollider2D(true);
		}
		else
		{
			SetCollider2D(false);
		}
	}

	protected void InitSkill()
	{
		PreloadBullet();
	}

	protected override void AfterActive()
	{
		bool flag = ((MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.IsMultiply && !MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.IsPvp) ? true : false);
		if (_follow_Player != null && (_follow_Player.IsLocalPlayer || flag))
		{
			if (sActiveSE2 != null && sActiveSE2.Length > 1)
			{
				base.SoundSource.PlaySE(sActiveSE2[0], sActiveSE2[1]);
			}
			if (activeSE != null && sActiveSE2.Length > 1)
			{
				base.SoundSource.PlaySE(activeSE[0], activeSE[1]);
			}
			SetVisible(true);
		}
		else
		{
			SetVisible(false);
		}
		if (_tSkill0_Table != null)
		{
			SetSize(_tSkill0_Table);
		}
		else
		{
			SetSize(PetWeapons[0].BulletData);
		}
		UpdateHitBox();
		_otLifeTimer.TimerStart();
		_rigidbody2D.WakeUp();
		_cTrapCollider.enabled = true;
		_nDeactiveType = 0;
		SetStatus(MainStatus.Ready);
	}

	protected override void AfterDeactive()
	{
		base.AfterDeactive();
		if (unactiveSE != null && unactiveSE.Length > 1)
		{
			base.SoundSource.PlaySE(unactiveSE[0], unactiveSE[1]);
		}
	}

	public override void UpdateFunc()
	{
		bool activate = Activate;
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
		case MainStatus.Destruct:
			if (!string.IsNullOrEmpty(DestructFx))
			{
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(DestructFx, base.transform.position, Quaternion.identity, Array.Empty<object>());
			}
			if (DestructSE != null && DestructSE.Length >= 2)
			{
				base.SoundSource.PlaySE(DestructSE[0], DestructSE[1]);
			}
			SetStatus(MainStatus.BackToPool);
			return;
		case MainStatus.BackToPool:
			DestructToPool();
			return;
		}
		if (_otLifeTimer.GetMillisecond() <= _nLifeTime)
		{
			return;
		}
		_nDeactiveType = 1;
		if (base.bOnPauseFirstUpdate)
		{
			if (!StageUpdate.bWaitReconnect)
			{
				SetStatus(MainStatus.BackToPool);
			}
		}
		else if (_follow_Player.IsLocalPlayer)
		{
			SetStatus(MainStatus.Destruct);
		}
	}

	public override void BackToPool()
	{
		_nBulletLv = 1;
		_nLifeTime = 0L;
		_otLifeTimer.TimerStop();
		_hitList.Clear();
		_rigidbody2D.Sleep();
		_cTrapCollider.enabled = false;
		SetCollider2D(false);
		SetActive(false);
		SetStatus(MainStatus.Idle);
		base.BackToPool();
	}

	protected virtual void DestructToPool()
	{
		BackToPool();
	}

	private void PreloadBullet()
	{
		PetTable = ManagedSingleton<OrangeDataManager>.Instance.PET_TABLE_DICT[PetID];
		SKILL_TABLE[] petAllSkillData = ManagedSingleton<OrangeTableHelper>.Instance.GetPetAllSkillData(PetTable);
		foreach (SKILL_TABLE p_table in petAllSkillData)
		{
			if (!(p_table.s_MODEL == "DUMMY") && !MonoBehaviourSingleton<PoolManager>.Instance.ExistsInPool<BulletBase>(p_table.s_MODEL))
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
			SyncLunckStatus(tSendb);
			break;
		case 2:
			SyncReadyStatus(tSendb);
			break;
		case 3:
			SyncDestructStatus(tSendb);
			break;
		case 4:
			SyncSelfPosAndMainStatusMessage(nSet, tSendb);
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
			case MainStatus.BackToPool:
				SendSelfPosAndMainStatusMessage();
				break;
			}
		}
	}

	protected void SetLaunchStatus()
	{
	}

	protected void SyncLunckStatus(NetSyncData tSendb)
	{
		if (!_follow_Player.IsLocalPlayer)
		{
			_mainStatus = MainStatus.Launch;
		}
	}

	protected void SetReadyStatus()
	{
	}

	protected void SyncReadyStatus(NetSyncData tSendb)
	{
		if (!_follow_Player.IsLocalPlayer)
		{
			_mainStatus = MainStatus.Ready;
		}
	}

	protected void SetDestructStatus()
	{
		CreateSkillBullet();
		SendSelfPosAndMainStatusMessage();
		if (!string.IsNullOrEmpty(DestructFx))
		{
			MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(DestructFx, base.transform.position, Quaternion.identity, Array.Empty<object>());
		}
		if (DestructSE != null && DestructSE.Length >= 2)
		{
			base.SoundSource.PlaySE(DestructSE[0], DestructSE[1]);
		}
	}

	protected void SyncDestructStatus(NetSyncData tSendb)
	{
		if (!_follow_Player.IsLocalPlayer)
		{
			_mainStatus = MainStatus.Destruct;
			Controller.LogicPosition.x = tSendb.SelfPosX;
			Controller.LogicPosition.y = tSendb.SelfPosY;
			Controller.LogicPosition.z = tSendb.SelfPosZ;
			CreateSkillBullet();
			if (!string.IsNullOrEmpty(DestructFx))
			{
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(DestructFx, base.transform.position, Quaternion.identity, Array.Empty<object>());
			}
			if (DestructSE != null && DestructSE.Length >= 2)
			{
				base.SoundSource.PlaySE(DestructSE[0], DestructSE[1]);
			}
		}
	}

	protected void SendSelfPosAndMainStatusMessage()
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

	protected void SyncSelfPosAndMainStatusMessage(int nSet, NetSyncData tSendb)
	{
		if (!_follow_Player.IsLocalPlayer)
		{
			Controller.LogicPosition.x = tSendb.SelfPosX;
			Controller.LogicPosition.y = tSendb.SelfPosY;
			Controller.LogicPosition.z = tSendb.SelfPosZ;
			_mainStatus = (MainStatus)nSet;
		}
	}

	private void SetVisible(bool _visible)
	{
		if ((bool)_psBody)
		{
			if (_visible)
			{
				_psBody.Play();
			}
			else
			{
				_psBody.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
			}
		}
		if ((bool)_customBody)
		{
			_customBody.SetVisible(_visible);
		}
	}

	public void OnTriggerHit(Collider2D col)
	{
		if (ManagedSingleton<StageHelper>.Instance.bEnemyActive && !MonoBehaviourSingleton<OrangeGameManager>.Instance.IsGamePause && _follow_Player.IsLocalPlayer && !base.bOnPauseFirstUpdate && !col.isTrigger && ((1 << col.gameObject.layer) & (int)base.TargetMask) != 0 && !CheckHitList(ref _hitList, col.transform) && _mainStatus == MainStatus.Ready)
		{
			if (sExplodeSE != null && sExplodeSE.Length >= 2)
			{
				base.SoundSource.PlaySE(sExplodeSE[0], sExplodeSE[1]);
			}
			_nDeactiveType = 2;
			SetStatus(MainStatus.Destruct);
		}
	}

	protected bool CheckHitList(ref HashSet<Transform> hitList, Transform newHit)
	{
		if (hitList.Contains(newHit))
		{
			return true;
		}
		StageObjParam component = newHit.GetComponent<StageObjParam>();
		if ((bool)component)
		{
			OrangeCharacter orangeCharacter = component.tLinkSOB as OrangeCharacter;
			if ((bool)orangeCharacter)
			{
				CharacterControlBase component2 = orangeCharacter.GetComponent<CharacterControlBase>();
				if ((bool)component2)
				{
					foreach (Transform hit in hitList)
					{
						if (component2.CheckMyShield(hit))
						{
							return true;
						}
					}
				}
			}
		}
		else
		{
			PlayerCollider component3 = newHit.GetComponent<PlayerCollider>();
			if (component3 != null && component3.IsDmgReduceShield() && hitList.Contains(component3.GetDmgReduceOwnerTransform()))
			{
				return true;
			}
		}
		return false;
	}

	protected void CreateSkillBullet()
	{
		Vector3 right = Vector3.right;
		SKILL_TABLE tSkillTable = ((_tSkill0_Table == null) ? PetWeapons[0].BulletData : _tSkill0_Table);
		BulletBase bulletBase = ((!_follow_Player) ? BulletBase.TryShotBullet(tSkillTable, _transform, right, _wsWeaponStatus, selfBuffManager.sBuffStatus, null, base.TargetMask, false, false, true) : BulletBase.TryShotBullet(tSkillTable, _transform, right, _wsWeaponStatus, _follow_Player.selfBuffManager.sBuffStatus, null, base.TargetMask, false, false, true));
		if ((bool)bulletBase && _follow_Player != null)
		{
			bulletBase.BulletLevel = _nBulletLv;
			bulletBase.SetOwnerName(_follow_Player.sPlayerName);
		}
	}
}
