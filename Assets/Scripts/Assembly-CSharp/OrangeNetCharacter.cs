#define RELEASE
using System.Collections;
using System.Collections.Generic;
using System.IO;
using StageLib;
using UnityEngine;

public class OrangeNetCharacter : OrangeCharacter
{
	protected OcSyncData tOcSyncData = new OcSyncData();

	private InputInfo _prevRecvSyncInputInfo = new InputInfo();

	private Queue<InputInfo> commandQueue = new Queue<InputInfo>();

	private List<BulletBase.NetBulletData> listBullets = new List<BulletBase.NetBulletData>();

	private bool bWaitNetDead;

	private Coroutine deadWaitCoroutine;

	private float fNoCommandTimeSum;

	public readonly int nWaitFrameCount = 80;

	private bool bNetIdleStatus;

	public override void SetLocalPlayer(bool isLocal)
	{
		Debug.LogError("錯誤的嘗試切換OrangeNetCharacter的LocalPlayer身分!!");
	}

	protected new void Start()
	{
		base.Start();
		tOcSyncData.dicUpdateTimer[ESyncData.WEAPON1_CHARGE] = PlayerWeapons[0].ChargeTimer;
		tOcSyncData.dicUpdateTimer[ESyncData.WEAPON1_LASTUSE] = PlayerWeapons[0].LastUseTimer;
		tOcSyncData.dicUpdateTimer[ESyncData.WEAPON2_CHARGE] = PlayerWeapons[1].ChargeTimer;
		tOcSyncData.dicUpdateTimer[ESyncData.WEAPON2_LASTUSE] = PlayerWeapons[1].LastUseTimer;
		tOcSyncData.dicUpdateTimer[ESyncData.SKILL1_CHARGE] = PlayerSkills[0].ChargeTimer;
		tOcSyncData.dicUpdateTimer[ESyncData.SKILL1_LASTUSE] = PlayerSkills[0].LastUseTimer;
		tOcSyncData.dicUpdateTimer[ESyncData.SKILL2_CHARGE] = PlayerSkills[1].ChargeTimer;
		tOcSyncData.dicUpdateTimer[ESyncData.SKILL2_LASTUSE] = PlayerSkills[1].LastUseTimer;
	}

	public override void LogicUpdate()
	{
		base.LogicUpdate();
		if (!IsDead())
		{
			while (commandQueue.Count > 0)
			{
				InputInfo inputInfo = commandQueue.Dequeue();
				if (base.IsLocalPlayer)
				{
					while (commandQueue.Count > 0 && inputInfo.nRecordNO == commandQueue.Peek().nRecordNO)
					{
						inputInfo = commandQueue.Dequeue();
					}
					if (commandQueue.Count > 8)
					{
						while (commandQueue.Count > 5)
						{
							inputInfo = commandQueue.Dequeue();
						}
					}
					return;
				}
				if (!ManagedSingleton<InputStorage>.Instance.CheckInputDataNO(base.sPlayerID.ToString(), inputInfo.nRecordNO))
				{
					continue;
				}
				ManagedSingleton<InputStorage>.Instance.SetInputData(base.sPlayerID.ToString(), inputInfo);
				UpdateValueNet();
				ShootBulletByRecordNo(inputInfo.nRecordNO);
				if (commandQueue.Count <= 8)
				{
					return;
				}
				while (commandQueue.Count > 5)
				{
					inputInfo = commandQueue.Dequeue();
					if (ManagedSingleton<InputStorage>.Instance.CheckInputDataNO(base.sPlayerID.ToString(), inputInfo.nRecordNO))
					{
						ManagedSingleton<InputStorage>.Instance.SetInputData(base.sPlayerID.ToString(), inputInfo);
						UpdateValueNet();
						ShootBulletByRecordNo(inputInfo.nRecordNO);
					}
				}
				return;
			}
		}
		else
		{
			ShootBulletByRecordNo();
		}
		fNoCommandTimeSum += GameLogicUpdateManager.m_fFrameLen;
		if (!base.IsLocalPlayer && (!bNetIdleStatus || !bNeedUpdateAlways) && (fNoCommandTimeSum > GameLogicUpdateManager.m_fFrameLen * (float)nWaitFrameCount || MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.CheckPlayerPause(sNetSerialID)))
		{
			bNetIdleStatus = true;
			if (!bIsNpcCpy && !IsDead())
			{
				ManagedSingleton<InputStorage>.Instance.ResetPlayerInput(base.sPlayerID);
				InputInfo inputInfo2 = ManagedSingleton<InputStorage>.Instance.GetInputInfo(base.sPlayerID);
				inputInfo2.tOcSyncData = new OcSyncData();
				inputInfo2.tOcSyncData.dicIntParams[ESyncData.INT_PARAMS_WEAPON1_MAGZ] = (int)PlayerWeapons[0].MagazineRemain;
				inputInfo2.tOcSyncData.dicIntParams[ESyncData.INT_PARAMS_WEAPON2_MAGZ] = (int)PlayerWeapons[1].MagazineRemain;
				inputInfo2.tOcSyncData.dicIntParams[ESyncData.INT_PARAMS_SKILL1_MAGZ] = (int)PlayerSkills[0].MagazineRemain;
				inputInfo2.tOcSyncData.dicIntParams[ESyncData.INT_PARAMS_SKILL2_MAGZ] = (int)PlayerSkills[1].MagazineRemain;
				inputInfo2.tOcSyncData.dicIntParams[ESyncData.INT_PARAMS_CURRENT_WEAPON] = base.WeaponCurrent;
				inputInfo2.tOcSyncData.dicIntParams[ESyncData.INT_PARAMS_BUFF_MEASURE] = selfBuffManager.nMeasureNow;
				inputInfo2.tOcSyncData.dicUpdateTimer[ESyncData.WEAPON1_CHARGE] = PlayerWeapons[0].ChargeTimer;
				inputInfo2.tOcSyncData.dicUpdateTimer[ESyncData.WEAPON1_LASTUSE] = PlayerWeapons[0].LastUseTimer;
				inputInfo2.tOcSyncData.dicUpdateTimer[ESyncData.WEAPON2_CHARGE] = PlayerWeapons[1].ChargeTimer;
				inputInfo2.tOcSyncData.dicUpdateTimer[ESyncData.WEAPON2_LASTUSE] = PlayerWeapons[1].LastUseTimer;
				inputInfo2.tOcSyncData.dicUpdateTimer[ESyncData.SKILL1_CHARGE] = PlayerSkills[0].ChargeTimer;
				inputInfo2.tOcSyncData.dicUpdateTimer[ESyncData.SKILL1_LASTUSE] = PlayerSkills[0].LastUseTimer;
				inputInfo2.tOcSyncData.dicUpdateTimer[ESyncData.SKILL2_CHARGE] = PlayerSkills[1].ChargeTimer;
				inputInfo2.tOcSyncData.dicUpdateTimer[ESyncData.SKILL2_LASTUSE] = PlayerSkills[1].LastUseTimer;
				inputInfo2.tOcSyncData.dicStringParams[ESyncData.STRING_PARAMS_SERIALID] = "";
				inputInfo2.tOcSyncData.listPerBuff = selfBuffManager.GetSelfSyncBuffList();
				inputInfo2.UpdatePos = base.Controller.LogicPosition.vec3;
				bNeedUpdateAlways = true;
				StopPlayer();
				UpdateValueNet();
			}
			else
			{
				bNeedUpdateAlways = true;
			}
		}
	}

	public void RecvNetworkRuntimeData(BinaryReader br)
	{
		InputInfo inputInfo = new InputInfo();
		_prevRecvSyncInputInfo.CopyTo(inputInfo);
		inputInfo.CombineRuntimeDiff(br);
		RecvNetworkRuntimeData(inputInfo);
		inputInfo.CopyTo(_prevRecvSyncInputInfo);
	}

	public void RecvNetworkRuntimeData(InputInfo info)
	{
		commandQueue.Enqueue(info);
		fNoCommandTimeSum = 0f;
		bNetIdleStatus = false;
		if ((int)Hp > 0)
		{
			if (CheckActStatusEvt != null && CheckActStatusEvt(11, -1))
			{
				bNeedUpdateAlways = true;
			}
			else
			{
				bNeedUpdateAlways = false;
			}
		}
	}

	public void UpdateValueNet()
	{
		InputInfo inputInfo = ManagedSingleton<InputStorage>.Instance.GetInputInfo(base.sPlayerID);
		base.EventLockInputingNet = inputInfo.bLockInput;
		nBulletRecordID = 1;
		PlayerWeapons[0].ChargeTimer = inputInfo.tOcSyncData.dicUpdateTimer[ESyncData.WEAPON1_CHARGE];
		PlayerWeapons[0].LastUseTimer = inputInfo.tOcSyncData.dicUpdateTimer[ESyncData.WEAPON1_LASTUSE];
		PlayerWeapons[0].MagazineRemain = inputInfo.tOcSyncData.dicIntParams[ESyncData.INT_PARAMS_WEAPON1_MAGZ];
		PlayerWeapons[1].ChargeTimer = inputInfo.tOcSyncData.dicUpdateTimer[ESyncData.WEAPON2_CHARGE];
		PlayerWeapons[1].LastUseTimer = inputInfo.tOcSyncData.dicUpdateTimer[ESyncData.WEAPON2_LASTUSE];
		PlayerWeapons[1].MagazineRemain = inputInfo.tOcSyncData.dicIntParams[ESyncData.INT_PARAMS_WEAPON2_MAGZ];
		PlayerSkills[0].ChargeTimer = inputInfo.tOcSyncData.dicUpdateTimer[ESyncData.SKILL1_CHARGE];
		PlayerSkills[0].LastUseTimer = inputInfo.tOcSyncData.dicUpdateTimer[ESyncData.SKILL1_LASTUSE];
		PlayerSkills[0].MagazineRemain = inputInfo.tOcSyncData.dicIntParams[ESyncData.INT_PARAMS_SKILL1_MAGZ];
		PlayerSkills[1].ChargeTimer = inputInfo.tOcSyncData.dicUpdateTimer[ESyncData.SKILL2_CHARGE];
		PlayerSkills[1].LastUseTimer = inputInfo.tOcSyncData.dicUpdateTimer[ESyncData.SKILL2_LASTUSE];
		PlayerSkills[1].MagazineRemain = inputInfo.tOcSyncData.dicIntParams[ESyncData.INT_PARAMS_SKILL2_MAGZ];
		if (inputInfo.tOcSyncData.dicIntParams[ESyncData.INT_PARAMS_CURRENT_WEAPON] != base.WeaponCurrent)
		{
			PlayerPressSelect();
		}
		if (selfBuffManager.nMeasureNow != inputInfo.tOcSyncData.dicIntParams[ESyncData.INT_PARAMS_BUFF_MEASURE])
		{
			selfBuffManager.AddMeasure(inputInfo.tOcSyncData.dicIntParams[ESyncData.INT_PARAMS_BUFF_MEASURE] - selfBuffManager.nMeasureNow);
		}
		if (inputInfo.tOcSyncData.dicStringParams[ESyncData.STRING_PARAMS_SERIALID] != "")
		{
			StageUpdate stageUpdate = StageResManager.GetStageUpdate();
			base.IAimTargetLogicUpdate = stageUpdate.GetSOBByNetSerialID(inputInfo.tOcSyncData.dicStringParams[ESyncData.STRING_PARAMS_SERIALID]) as IAimTarget;
		}
		else
		{
			base.IAimTargetLogicUpdate = null;
		}
		selfBuffManager.SyncByNetBuff(inputInfo.tOcSyncData.listPerBuff);
		if (!LockInput && !base.bLockInputCtrl)
		{
			base.Controller.LogicPosition = new VInt3(inputInfo.UpdatePos);
			base.transform.localPosition = inputInfo.UpdatePos;
			vLastMovePt = inputInfo.UpdatePos;
		}
		if (!base.bLockInputCtrl)
		{
			UpdateAimDirection2(inputInfo.ShootDir);
			UpdateValue();
		}
	}

	public void AddBulletToUpdateShot(BulletBase.NetBulletData tNBD)
	{
		listBullets.Add(tNBD);
		if (IsDead())
		{
			ShootBulletByRecordNo();
		}
	}

	public void ShootBulletByRecordNo(int recordNo = int.MaxValue)
	{
		StageUpdate stageUpdate = StageResManager.GetStageUpdate();
		for (int num = listBullets.Count - 1; num >= 0; num--)
		{
			if (listBullets[num].nRecordNO <= recordNo)
			{
				stageUpdate.ShotBulletByNetBulletData(listBullets[num]);
				listBullets.RemoveAt(num);
			}
		}
	}

	public override void DerivedContinueCall()
	{
		base.DerivedContinueCall();
		ClearCommandQueue();
	}

	protected override void TriggerDeadAfterHurt(string killer)
	{
		bWaitNetDead = true;
		if (deadWaitCoroutine != null)
		{
			StopCoroutine(deadWaitCoroutine);
		}
		deadWaitCoroutine = StartCoroutine(WaitDead());
	}

	protected IEnumerator WaitDead(float fWaitTimeSet = 1f)
	{
		float fWaitTime = fWaitTimeSet;
		yield return CoroutineDefine._waitForEndOfFrame;
		while (bWaitNetDead)
		{
			fWaitTime -= Time.deltaTime;
			if (fWaitTime <= 0f)
			{
				break;
			}
			yield return CoroutineDefine._waitForEndOfFrame;
		}
		if (bWaitNetDead)
		{
			PlayerDead();
			bWaitNetDead = false;
		}
		deadWaitCoroutine = null;
	}

	public override void DieFromServer()
	{
		if (bWaitNetDead || (int)Hp > 0)
		{
			bWaitNetDead = false;
			if (deadWaitCoroutine != null)
			{
				StopCoroutine(deadWaitCoroutine);
			}
			deadWaitCoroutine = null;
			PlayerDead();
		}
	}

	public void ClearCommandQueue()
	{
		fNoCommandTimeSum = 0f;
		commandQueue.Clear();
		listBullets.Clear();
	}

	public void ResetSyncDataTimer(ESyncData key)
	{
		if (tOcSyncData != null)
		{
			switch (key)
			{
			case ESyncData.WEAPON1_CHARGE:
				tOcSyncData.dicUpdateTimer[ESyncData.WEAPON1_CHARGE] = PlayerWeapons[0].ChargeTimer;
				break;
			case ESyncData.WEAPON1_LASTUSE:
				tOcSyncData.dicUpdateTimer[ESyncData.WEAPON1_LASTUSE] = PlayerWeapons[0].LastUseTimer;
				break;
			case ESyncData.WEAPON2_CHARGE:
				tOcSyncData.dicUpdateTimer[ESyncData.WEAPON2_CHARGE] = PlayerWeapons[1].ChargeTimer;
				break;
			case ESyncData.WEAPON2_LASTUSE:
				tOcSyncData.dicUpdateTimer[ESyncData.WEAPON2_LASTUSE] = PlayerWeapons[1].LastUseTimer;
				break;
			case ESyncData.SKILL1_CHARGE:
				tOcSyncData.dicUpdateTimer[ESyncData.SKILL1_CHARGE] = PlayerSkills[0].ChargeTimer;
				break;
			case ESyncData.SKILL1_LASTUSE:
				tOcSyncData.dicUpdateTimer[ESyncData.SKILL1_LASTUSE] = PlayerSkills[0].LastUseTimer;
				break;
			case ESyncData.SKILL2_CHARGE:
				tOcSyncData.dicUpdateTimer[ESyncData.SKILL2_CHARGE] = PlayerSkills[1].ChargeTimer;
				break;
			case ESyncData.SKILL2_LASTUSE:
				tOcSyncData.dicUpdateTimer[ESyncData.SKILL2_LASTUSE] = PlayerSkills[1].LastUseTimer;
				break;
			}
		}
	}
}
