#define RELEASE
using System;
using System.Collections.Generic;
using CallbackDefs;
using Newtonsoft.Json;
using StageLib;
using UnityEngine;

public class SCH006Controller : PetControllerBase
{
	public enum MainStatus
	{
		Idle = 0,
		Launch = 1,
		Ready = 2,
		Destruct = 3,
		BackToPool = 4
	}

	protected MainStatus m_mainStatus;

	private OrangeTimer m_statusTimer;

	private OrangeTimer m_attackTimer;

	private string m_modelName;

	private float m_lifeTime;

	protected int m_bulletSkillId;

	protected WeaponStatus m_weaponStatus;

	protected Vector3 m_startPos = new Vector3(0f, 0f, 0f);

	private int m_tweenId = -1;

	private float m_milliSecToNextStatus;

	protected SKILL_TABLE m_bulletSkillTable;

	protected int m_skillLevel = 1;

	protected List<SKILL_TABLE> listBulletSkillTable = new List<SKILL_TABLE>();

	protected ParticleSystem m_forceFieldEffect;

	protected bool m_checkTargetInRange;

	protected int m_bulletCountNow;

	protected int m_bulletCountMax;

	protected NetSyncData tSendb = new NetSyncData();

	protected VInt3 m_targetPos;

	protected bool useRandomSkl;

	private int fixedAimDistance = 15;

	protected bool isLocalPlayer = true;

	private int nowBulletIdx = -1;

	protected NetSyncData receiveSyncData = new NetSyncData();

	public Vector3 StartOffset { get; set; } = new Vector3(0f, 1f, 0f);


	public Vector3 EndOffset { get; set; } = new Vector3(0f, 2.5f, 0f);


	public int SyncBulletIdx { get; set; } = -1;


	public override string[] GetPetDependAnimations()
	{
		return new string[3] { "SCH006@start", "SCH006@loop", "SCH006@end" };
	}

	public override bool CheckIsLocalPlayer()
	{
		return isLocalPlayer;
	}

	public override void Set_follow_Player(OrangeCharacter mOC, bool linkBuffManager = true)
	{
		base.Set_follow_Player(mOC, linkBuffManager);
		isLocalPlayer = _follow_Player != null && _follow_Player.IsLocalPlayer;
	}

	public void ForceSetLocalPlayer(bool isLocalPlayer)
	{
		this.isLocalPlayer = isLocalPlayer;
	}

	protected override void Start()
	{
		base.Start();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		LeanTween.cancel(ref m_tweenId);
	}

	public override void UpdateFunc()
	{
		bool activate = Activate;
	}

	public override void LogicUpdate()
	{
		base.LogicUpdate();
		if (!Activate)
		{
			return;
		}
		if (!CheckIsLocalPlayer())
		{
			switch (m_mainStatus)
			{
			case MainStatus.Ready:
				if ((float)m_statusTimer.GetMillisecond() > m_lifeTime)
				{
					SetStatus(m_mainStatus + 1);
				}
				break;
			case MainStatus.Destruct:
				SetStatus(m_mainStatus + 1);
				break;
			}
		}
		else if (m_bulletSkillTable == null)
		{
			Debug.Log("m_bulletSkillTable is null.");
		}
		else
		{
			if ((float)m_statusTimer.GetMillisecond() > m_milliSecToNextStatus)
			{
				SetStatus(m_mainStatus + 1);
			}
			MainStatus mainStatus = m_mainStatus;
			if (mainStatus == MainStatus.Ready)
			{
				Status_Ready();
			}
		}
	}

	protected void Status_Ready()
	{
		if (m_attackTimer.GetMillisecond() <= m_bulletSkillTable.n_FIRE_SPEED)
		{
			return;
		}
		if (m_bulletCountNow >= m_bulletCountMax)
		{
			SetStatus(MainStatus.Destruct);
			return;
		}
		if (m_checkTargetInRange)
		{
			CheckAimTargetInRange();
		}
		if (_autoAim.AutoAimTarget == null)
		{
			IAimTarget closestTarget = _autoAim.GetClosestTarget();
			_autoAim.SetTarget(closestTarget, true);
		}
		if (_autoAim.AutoAimTarget != null)
		{
			if (CheckIsLocalPlayer())
			{
				SendSyncData(MainStatus.Ready);
			}
			AttackTarget();
			m_attackTimer.TimerStart();
		}
	}

	protected void Sync_Status_Ready()
	{
		if (_autoAim.AutoAimTarget != null)
		{
			AttackTarget();
		}
	}

	protected override void Awake()
	{
		activeSE = new string[2] { "WeaponSE", "wep_etc_dmfld02_lp" };
		unactiveSE = new string[2] { "WeaponSE", "wep_etc_dmfld02_stop" };
		base.Awake();
		m_statusTimer = OrangeTimerManager.GetTimer();
		m_attackTimer = OrangeTimerManager.GetTimer();
		m_forceFieldEffect = GetComponentInChildren<ParticleSystem>(true);
	}

	public override void SetActive(bool isActive)
	{
		base.SetActive(isActive);
		Controller.enabled = isActive;
		Activate = isActive;
		if (isActive)
		{
			_autoAim.SetUpdate(CheckIsLocalPlayer());
			base.transform.position = m_startPos;
			SetStatus(MainStatus.Launch);
		}
		else
		{
			_autoAim.ClearTargetList();
			SetStatus(MainStatus.BackToPool);
		}
	}

	protected override void AfterActive()
	{
		int num = activeSE.Length;
		float num2 = 0f;
		float num3 = 0f;
		switch (num)
		{
		case 0:
			break;
		case 1:
			if (!string.IsNullOrEmpty(activeSE[0]))
			{
				bool flag = activeSE[0] == "null";
			}
			break;
		case 2:
			base.SoundSource.PlaySE(activeSE[0], activeSE[1]);
			break;
		case 3:
			num2 = float.Parse(activeSE[2]);
			base.SoundSource.PlaySE(activeSE[0], activeSE[1], num2);
			break;
		case 4:
			num3 = float.Parse(activeSE[3]);
			num2 = float.Parse(activeSE[2]);
			base.SoundSource.AddLoopSE(activeSE[0], activeSE[1], num3, num2);
			break;
		}
	}

	protected override void AfterDeactive()
	{
		switch (unactiveSE.Length)
		{
		case 0:
			break;
		case 1:
			if (!string.IsNullOrEmpty(unactiveSE[0]))
			{
				bool flag = unactiveSE[0] == "null";
			}
			break;
		case 2:
		case 3:
			if (unactiveSE.Length > 1)
			{
				base.SoundSource.PlaySE(unactiveSE[0], unactiveSE[1]);
			}
			break;
		case 4:
			base.SoundSource.RemoveLoopSE(activeSE[0], activeSE[1]);
			if (unactiveSE.Length > 1)
			{
				base.SoundSource.PlaySE(unactiveSE[0], unactiveSE[1]);
			}
			break;
		}
	}

	public override void SetPositionAndRotation(Vector3 pos, bool bBack)
	{
		direction = 1;
		base.transform.position = pos;
		Controller.LogicPosition = new VInt3(base.transform.position);
	}

	public override void SetParams(string modelName, long lifeTime, int bulletSkillId, WeaponStatus weaponStatus, long debutTime = 0L)
	{
		m_modelName = modelName;
		m_lifeTime = lifeTime;
		m_bulletSkillId = bulletSkillId;
		m_weaponStatus = weaponStatus;
		if (_follow_Player != null)
		{
			base.TargetMask = _follow_Player.TargetMask;
			m_startPos = _follow_Player.transform.position;
		}
		listBulletSkillTable = new List<SKILL_TABLE>();
		SKILL_TABLE value = null;
		if (!ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT.TryGetNewValue(m_bulletSkillId, out value))
		{
			Debug.Log("Error retrieving skill table");
			return;
		}
		if (_follow_Player != null)
		{
			_follow_Player.tRefPassiveskill.ReCalcuSkill(ref value);
		}
		m_bulletSkillTable = value;
		m_bulletCountMax = value.n_MAGAZINE;
		listBulletSkillTable.Add(value);
		PreloadBullet(value);
		CheckComboBullet(value);
	}

	public void SetCurrentBullet(int idx)
	{
		if (listBulletSkillTable.Count > idx && Activate && (m_bulletSkillTable == null || m_bulletSkillTable.n_ID != listBulletSkillTable[idx].n_ID))
		{
			m_bulletSkillTable = listBulletSkillTable[idx];
			if (_follow_Player != null)
			{
				_follow_Player.tRefPassiveskill.ReCalcuSkill(ref m_bulletSkillTable);
			}
			m_bulletCountMax = m_bulletSkillTable.n_MAGAZINE;
			nowBulletIdx = idx;
			m_bulletSkillId = m_bulletSkillTable.n_ID;
		}
	}

	private void CheckComboBullet(SKILL_TABLE p_table)
	{
		int n_COMBO_SKILL = p_table.n_COMBO_SKILL;
		if (n_COMBO_SKILL > 0)
		{
			SKILL_TABLE value = null;
			if (ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT.TryGetNewValue(n_COMBO_SKILL, out value) && !listBulletSkillTable.Contains(value))
			{
				listBulletSkillTable.Add(value);
				CheckComboBullet(value);
				PreloadBullet(value);
			}
		}
	}

	private void PreloadBullet(SKILL_TABLE p_table)
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

	public override void ReplaceListBulletSkillTable(List<SKILL_TABLE> list, bool useRandomSkl = true)
	{
		listBulletSkillTable = list;
		foreach (SKILL_TABLE item in listBulletSkillTable)
		{
			PreloadBullet(item);
		}
		this.useRandomSkl = useRandomSkl;
	}

	protected void RandCurrentSkill()
	{
		if (!useRandomSkl)
		{
			return;
		}
		useRandomSkl = false;
		int num = nowBulletIdx;
		if (SyncBulletIdx == -1)
		{
			int num2 = OrangeBattleUtility.Random(0, 10001);
			for (int i = 0; i < listBulletSkillTable.Count; i++)
			{
				if (num2 < listBulletSkillTable[i].n_TRIGGER_RATE)
				{
					num = i;
					break;
				}
			}
		}
		else
		{
			num = SyncBulletIdx;
		}
		SetCurrentBullet(num);
		UseFixedData();
		SyncBulletIdx = num;
	}

	private void UseFixedData()
	{
		_autoAim.UpdateAimRange(fixedAimDistance);
		m_attackTimer.SetMillisecondsOffset(m_bulletSkillTable.n_FIRE_SPEED * 2);
	}

	protected virtual void AttackTarget()
	{
		Vector3 normalized = (_autoAim.GetTargetPoint() - base.transform.position).normalized;
		BulletBase bulletBase = BulletBase.TryShotBullet(m_bulletSkillTable, base.transform, normalized, m_weaponStatus, selfBuffManager.sBuffStatus, null, base.TargetMask, false, false, true, m_skillLevel);
		if (bulletBase != null && _follow_Player != null)
		{
			bulletBase.SetOwnerName(_follow_Player.sPlayerName);
		}
		m_bulletCountNow++;
	}

	protected void SetStatus(MainStatus mainStatus)
	{
		if (m_mainStatus != mainStatus)
		{
			m_mainStatus = mainStatus;
			switch (m_mainStatus)
			{
			case MainStatus.Launch:
				SkillLaunch();
				break;
			case MainStatus.Ready:
				SkillReady();
				break;
			case MainStatus.Destruct:
				SkillDestruct();
				break;
			case MainStatus.BackToPool:
				SkillEnd();
				break;
			}
		}
	}

	protected virtual void SkillLaunch()
	{
		float num = 1f;
		m_milliSecToNextStatus = num * 1000f;
		m_statusTimer.TimerStart();
		SetAnimateId(PetHumanBase.PetAnimateId.ANI_SKILL_START);
		_autoAim.SetEnable(false);
		_autoAim.targetMask = base.TargetMask;
		if (CheckIsLocalPlayer())
		{
			Vector3 position = base.transform.position;
			LeanTween.cancel(ref m_tweenId);
			m_tweenId = LeanTween.value(base.gameObject, position + StartOffset, position + EndOffset, num).setOnUpdate(delegate(Vector3 pos)
			{
				SetPositionAndRotation(pos, false);
				SendSyncData(MainStatus.Launch);
			}).setEaseOutExpo()
				.uniqueId;
		}
		if ((bool)m_forceFieldEffect)
		{
			m_forceFieldEffect.gameObject.SetActive(true);
		}
	}

	private void SkillReady()
	{
		LeanTween.cancel(ref m_tweenId);
		m_bulletCountNow = 0;
		m_milliSecToNextStatus = m_lifeTime;
		m_statusTimer.TimerStart();
		m_attackTimer.TimerStart();
		SetAnimateId((PetHumanBase.PetAnimateId)5u);
		_autoAim.UpdateAimRange(m_bulletSkillTable.f_DISTANCE);
		_autoAim.SetEnable(true);
		RandCurrentSkill();
		if (CheckIsLocalPlayer())
		{
			SendSyncData(MainStatus.Ready);
		}
	}

	private void SkillDestruct()
	{
		m_milliSecToNextStatus = 600f;
		m_statusTimer.TimerStart();
		m_attackTimer.TimerStop();
		SetAnimateId((PetHumanBase.PetAnimateId)6u);
		_autoAim.SetEnable(false);
		SyncBulletIdx = -1;
		SendSyncData(MainStatus.Destruct);
	}

	private void SkillEnd()
	{
		m_milliSecToNextStatus = 0f;
		m_skillLevel = 1;
		m_statusTimer.TimerStop();
		nowBulletIdx = -1;
		SetActive(false);
		if ((bool)m_forceFieldEffect)
		{
			m_forceFieldEffect.gameObject.SetActive(false);
			MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("DistortionFx", base.transform.position, Quaternion.identity, Array.Empty<object>());
		}
		MonoBehaviourSingleton<PoolManager>.Instance.BackToPool(this, m_modelName);
		SendSyncData(MainStatus.BackToPool);
		isLocalPlayer = true;
	}

	private void SendSyncData(MainStatus sendStatus)
	{
		tSendb.Clear();
		IAimTarget autoAimTarget = _autoAim.AutoAimTarget;
		string empty;
		if (autoAimTarget as StageObjBase != null)
		{
			empty = (autoAimTarget as StageObjBase).sNetSerialID;
			Debug.LogWarning("target as StageObjBase != null : " + empty);
		}
		else
		{
			empty = string.Empty;
		}
		tSendb.SelfPosX = Controller.LogicPosition.x;
		tSendb.SelfPosY = Controller.LogicPosition.y;
		tSendb.SelfPosZ = Controller.LogicPosition.z;
		tSendb.TargetPosX = m_targetPos.x;
		tSendb.TargetPosY = m_targetPos.y;
		tSendb.TargetPosZ = m_targetPos.z;
		tSendb.nParam0 = m_bulletSkillId;
		tSendb.sParam0 = empty;
		StageUpdate.RegisterPetSendAndRun(sNetSerialID, (int)sendStatus, JsonConvert.SerializeObject(tSendb), true);
	}

	public override void UpdateStatus(int nSet, string smsg, Callback tCB = null)
	{
		if (!Activate || CheckIsLocalPlayer())
		{
			return;
		}
		receiveSyncData.Clear();
		receiveSyncData = JsonConvert.DeserializeObject<NetSyncData>(smsg);
		Controller.LogicPosition.x = receiveSyncData.SelfPosX;
		Controller.LogicPosition.y = receiveSyncData.SelfPosY;
		Controller.LogicPosition.z = receiveSyncData.SelfPosZ;
		m_targetPos.x = receiveSyncData.TargetPosX;
		m_targetPos.y = receiveSyncData.TargetPosY;
		m_targetPos.z = receiveSyncData.TargetPosZ;
		m_bulletSkillId = receiveSyncData.nParam0;
		if (m_bulletSkillTable == null && !ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT.TryGetValue(m_bulletSkillId, out m_bulletSkillTable))
		{
			Debug.Log("Error retrieving skill table");
		}
		switch (nSet)
		{
		case 1:
			SetPositionAndRotation(Controller.LogicPosition.vec3, false);
			break;
		case 2:
			if (receiveSyncData.sParam0 != "")
			{
				_autoAim.SetTargetByNetSerialID(receiveSyncData.sParam0);
				_autoAim.SetUpdate(false);
				Sync_Status_Ready();
				_autoAim.SetTarget((IAimTarget)null, false);
			}
			break;
		}
		SetStatus((MainStatus)nSet);
	}

	public void SetSkillLevel(int skillLevel)
	{
		m_skillLevel = skillLevel;
	}

	public void EnableCheckAimTargetInRange(bool enable)
	{
		m_checkTargetInRange = enable;
	}

	protected void CheckAimTargetInRange()
	{
		if (_autoAim.AutoAimTarget != null && !_autoAim.CheckTargetInList(_autoAim.AutoAimTarget))
		{
			_autoAim.SetTarget((IAimTarget)null, false);
		}
	}
}
