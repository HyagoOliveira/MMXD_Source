using System;
using System.Collections;
using System.Collections.Generic;
using OrangeApi;
using OrangeAudio;
using StageLib;
using UnityEngine;

public class PvpBarUI : OrangeUIBase, IManagedLateUpdateBehavior
{
	private string format1 = ":";

	private string format2 = "D2";

	private float timeTotal = 300f;

	private float timeNow;

	private float timeLast;

	private float diff;

	private int minutes;

	private int seconds;

	private int milliseconds;

	[SerializeField]
	private OrangeText textTime;

	[SerializeField]
	private PvpBarUnit unitBlue;

	[SerializeField]
	private PvpBarUnit unitRed;

	private bool bIsSeason;

	private Dictionary<string, Coroutine> DicRebornCoroutine = new Dictionary<string, Coroutine>();

	private bool isEnd;

	private void OnDisable()
	{
		MonoBehaviourSingleton<UpdateManager>.Instance.RemoveLateUpdate(this);
		Singleton<GenericEventManager>.Instance.DetachEvent(EventManager.ID.STAGE_UPDATE_PLAYER_LIST, UpdatePlayer);
		Singleton<GenericEventManager>.Instance.DetachEvent<OrangeCharacter, bool>(EventManager.ID.STAGE_PLAYER_DESTROY_ED, UnRegisterPlayer);
	}

	public void Setup(int bluelife = -1, int redlife = -1, float settime = 0f)
	{
		STAGE_TABLE value = null;
		if (ManagedSingleton<OrangeDataManager>.Instance.STAGE_TABLE_DICT.TryGetValue(ManagedSingleton<StageHelper>.Instance.nLastStageID, out value))
		{
			timeTotal = value.n_TIME;
			bIsSeason = value.n_MAIN == 90000 && value.n_SUB == 1;
		}
		if (settime != 0f)
		{
			timeTotal = settime;
		}
		timeNow = timeTotal;
		UpdateTimeText();
		int playerTeam = MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.GetPlayerTeam(MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify);
		unitBlue.Init(this, playerTeam == 1, OrangeConst.PVP_1VS1_CONTINUE, bluelife);
		unitRed.Init(this, playerTeam == 2, OrangeConst.PVP_1VS1_CONTINUE, redlife);
		for (int i = 0; i < MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.ListMemberInfo.Count; i++)
		{
			MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.ListMemberInfo[i].nLifePercent = 100;
		}
		diff = timeTotal + Time.timeSinceLevelLoad;
		timeNow = timeTotal;
		UpdatePlayer();
		Singleton<GenericEventManager>.Instance.AttachEvent(EventManager.ID.STAGE_UPDATE_PLAYER_LIST, UpdatePlayer);
		Singleton<GenericEventManager>.Instance.AttachEvent<OrangeCharacter, bool>(EventManager.ID.STAGE_PLAYER_DESTROY_ED, UnRegisterPlayer);
		MonoBehaviourSingleton<UpdateManager>.Instance.AddLateUpdate(this);
	}

	public void SetTime(float fTimeSet)
	{
		timeNow = fTimeSet;
		diff = timeNow + Time.timeSinceLevelLoad;
	}

	public bool OnGetIsSeason()
	{
		return bIsSeason;
	}

	public void SetSeasonBattleInfo(OrangeCharacter oc)
	{
		for (int num = StageUpdate.runPlayers.Count - 1; num >= 0; num--)
		{
			if (StageUpdate.runPlayers[num].sPlayerID.IndexOf("NPC_") < 0 && (int)StageUpdate.runPlayers[num].Hp > 0)
			{
				SeasonBattleInfoReq seasonBattleInfoReq = new SeasonBattleInfoReq();
				seasonBattleInfoReq.PlayerID = StageUpdate.runPlayers[num].sPlayerID;
				seasonBattleInfoReq.CharacterId = StageUpdate.runPlayers[num].CharacterID;
				seasonBattleInfoReq.HP = StageUpdate.runPlayers[num].Hp;
				if (MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.GetPlayerTeam(StageUpdate.runPlayers[num].sPlayerID) == 1)
				{
					if (MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify == StageUpdate.runPlayers[num].sPlayerID)
					{
						seasonBattleInfoReq.KillCount = unitRed.GetBeKillLife();
						seasonBattleInfoReq.BeKilledCount = unitBlue.GetBeKillLife() + 1;
					}
					else
					{
						seasonBattleInfoReq.KillCount = unitBlue.GetBeKillLife() + 1;
						seasonBattleInfoReq.BeKilledCount = unitRed.GetBeKillLife();
					}
				}
				else if (MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify == StageUpdate.runPlayers[num].sPlayerID)
				{
					seasonBattleInfoReq.KillCount = unitBlue.GetBeKillLife();
					seasonBattleInfoReq.BeKilledCount = unitRed.GetBeKillLife() + 1;
				}
				else
				{
					seasonBattleInfoReq.KillCount = unitRed.GetBeKillLife() + 1;
					seasonBattleInfoReq.BeKilledCount = unitBlue.GetBeKillLife();
				}
				MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.listCmdSeasonBattleInfoReq.Add(seasonBattleInfoReq);
				MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.bSeasonCmdFlag = false;
				break;
			}
		}
	}

	public bool UpdateSeasonBattleInfo(OrangeCharacter oc)
	{
		if (!bIsSeason)
		{
			return true;
		}
		SeasonBattleInfoReq seasonBattleInfoReq = new SeasonBattleInfoReq();
		seasonBattleInfoReq.PlayerID = oc.sPlayerID;
		seasonBattleInfoReq.CharacterId = oc.CharacterID;
		seasonBattleInfoReq.HP = oc.Hp;
		if (MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.GetPlayerTeam(oc.sPlayerID) == 1)
		{
			if (MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify == oc.sPlayerID)
			{
				seasonBattleInfoReq.KillCount = unitRed.GetBeKillLife();
				seasonBattleInfoReq.BeKilledCount = unitBlue.GetBeKillLife() + 1;
			}
			else
			{
				seasonBattleInfoReq.KillCount = unitBlue.GetBeKillLife() + 1;
				seasonBattleInfoReq.BeKilledCount = unitRed.GetBeKillLife();
			}
		}
		else if (MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify == oc.sPlayerID)
		{
			seasonBattleInfoReq.KillCount = unitBlue.GetBeKillLife();
			seasonBattleInfoReq.BeKilledCount = unitRed.GetBeKillLife() + 1;
		}
		else
		{
			seasonBattleInfoReq.KillCount = unitRed.GetBeKillLife() + 1;
			seasonBattleInfoReq.BeKilledCount = unitBlue.GetBeKillLife();
		}
		if (seasonBattleInfoReq.KillCount >= 3 && seasonBattleInfoReq.BeKilledCount >= 3)
		{
			return false;
		}
		if (!MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.bSeasonCmdFlag)
		{
			return false;
		}
		MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.listCmdSeasonBattleInfoReq.Add(seasonBattleInfoReq);
		MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.OnSetSeasonKillCount(seasonBattleInfoReq.KillCount, seasonBattleInfoReq.BeKilledCount);
		if (seasonBattleInfoReq.KillCount >= 3 || seasonBattleInfoReq.BeKilledCount >= 3)
		{
			MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.bSeasonCmdFlag = false;
		}
		return true;
	}

	private void UnRegisterPlayer(OrangeCharacter tOC, bool bNeedRemove)
	{
		if (bNeedRemove)
		{
			return;
		}
		int playerTeam = MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.GetPlayerTeam(tOC.sPlayerID);
		int num = 0;
		num = ((playerTeam != 1) ? unitRed.GetNowLife() : unitBlue.GetNowLife());
		if (num > 0 && !DicRebornCoroutine.ContainsKey(tOC.sPlayerID))
		{
			if (OnGetIsSeason())
			{
				DicRebornCoroutine.Add(tOC.sPlayerID, StartCoroutine(WaitAndChangeCharacter(tOC, OrangeConst.PVP_1VS1_CONTINUE_TIME)));
			}
			else
			{
				DicRebornCoroutine.Add(tOC.sPlayerID, StartCoroutine(WaitAndReBorn(tOC, OrangeConst.PVP_1VS1_CONTINUE_TIME)));
			}
		}
	}

	private IEnumerator WaitAndReBorn(OrangeCharacter oc, float fTime)
	{
		while (fTime > 0f)
		{
			yield return CoroutineDefine._waitForEndOfFrame;
			if (!MonoBehaviourSingleton<OrangeGameManager>.Instance.IsGamePause)
			{
				fTime -= Time.deltaTime;
			}
		}
		if (oc.sPlayerID == MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify)
		{
			MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.RebornPlayer(oc);
		}
		else
		{
			fTime = OrangeConst.PVP_3VS3_CONTINUE_TIME - OrangeConst.PVP_1VS1_CONTINUE_TIME;
			while ((int)oc.Hp <= 0)
			{
				if (fTime <= 0f && oc.bNeedUpdateAlways)
				{
					MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.RebornPlayer(oc);
				}
				yield return CoroutineDefine._waitForEndOfFrame;
				fTime -= Time.deltaTime;
			}
		}
		DicRebornCoroutine.Remove(oc.sPlayerID);
	}

	private IEnumerator WaitAndChangeCharacter(OrangeCharacter oc, float fTime)
	{
		yield return CoroutineDefine._waitForEndOfFrame;
		string sPlayerID = oc.sPlayerID;
		Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_PLAYER_DESTROY_ED, oc, true);
		while (fTime > 0f)
		{
			yield return CoroutineDefine._waitForEndOfFrame;
			if (!MonoBehaviourSingleton<OrangeGameManager>.Instance.IsGamePause)
			{
				fTime -= Time.deltaTime;
			}
		}
		oc.SetActiveFalse();
		if (sPlayerID == MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify)
		{
			MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.ChangeNextCharacter(sPlayerID);
			StageUpdate.SyncStageObj(3, 9, sPlayerID, true);
		}
		else
		{
			MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.ChangeNextCharacter(sPlayerID);
		}
		DicRebornCoroutine.Remove(oc.sPlayerID);
	}

	private void UpdatePlayer()
	{
		if (isEnd || StageUpdate.runPlayers.Count == 0)
		{
			return;
		}
		foreach (OrangeCharacter runPlayer in StageUpdate.runPlayers)
		{
			if (!MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.HasPlayer(runPlayer.sPlayerID))
			{
				runPlayer.HurtActions -= unitBlue.UpdateBar;
				runPlayer.HurtActions -= unitRed.UpdateBar;
			}
			else if ((int)runPlayer.Hp <= 0)
			{
				if (!(runPlayer.sPlayerID == MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify))
				{
					continue;
				}
				bool flag = false;
				if (MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.GetPlayerTeam(runPlayer.sPlayerID) == 1)
				{
					if (unitBlue.GetNowLife() > 0)
					{
						flag = true;
					}
				}
				else if (unitRed.GetNowLife() > 0)
				{
					flag = true;
				}
				if (flag && !DicRebornCoroutine.ContainsKey(runPlayer.sPlayerID))
				{
					if (OnGetIsSeason())
					{
						DicRebornCoroutine.Add(runPlayer.sPlayerID, StartCoroutine(WaitAndChangeCharacter(runPlayer, OrangeConst.PVP_1VS1_CONTINUE_TIME)));
					}
					else
					{
						DicRebornCoroutine.Add(runPlayer.sPlayerID, StartCoroutine(WaitAndReBorn(runPlayer, OrangeConst.PVP_1VS1_CONTINUE_TIME)));
					}
				}
			}
			else if (MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.GetPlayerTeam(runPlayer.sPlayerID) == 1)
			{
				unitBlue.SetHp(runPlayer.sPlayerID, runPlayer.Hp, runPlayer.MaxHp);
				runPlayer.HurtActions -= unitBlue.UpdateBar;
				runPlayer.HurtActions += unitBlue.UpdateBar;
			}
			else
			{
				unitRed.SetHp(runPlayer.sPlayerID, runPlayer.Hp, runPlayer.MaxHp);
				runPlayer.HurtActions -= unitRed.UpdateBar;
				runPlayer.HurtActions += unitRed.UpdateBar;
			}
		}
		unitBlue.CalculateFill();
		unitRed.CalculateFill();
		if (unitBlue.GetNowLife() == 0 && unitRed.GetNowLife() == 0)
		{
			BattleEnd(2);
		}
		else if (unitBlue.GetNowLife() == 0)
		{
			if (MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.GetPlayerTeam(MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify) == 1)
			{
				BattleEnd(0);
			}
			else
			{
				BattleEnd(1);
			}
		}
		else if (unitRed.GetNowLife() == 0)
		{
			if (MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.GetPlayerTeam(MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify) == 1)
			{
				BattleEnd(1);
			}
			else
			{
				BattleEnd(0);
			}
		}
	}

	public void LateUpdateFunc()
	{
		timeNow = diff - Time.timeSinceLevelLoad;
		if (timeLast != timeNow)
		{
			UpdateTimeText();
		}
	}

	private void UpdateTimeText()
	{
		if ((int)timeNow % 2 == 0)
		{
			StageUpdate.SyncStageObj(3, 4, (timeNow - MonoBehaviourSingleton<StageSyncManager>.Instance.HostAvgDelayTime).ToString());
		}
		if (!isEnd && timeNow <= 0f)
		{
			int num = 0;
			num = ((unitBlue.GetNowLife() != unitRed.GetNowLife()) ? ((unitBlue.GetNowLife() > unitRed.GetNowLife()) ? 1 : 0) : ((!unitBlue.IsLiveHpFull() || !unitRed.IsLiveHpFull()) ? ((unitBlue.GetHpNow() > unitRed.GetHpNow()) ? 1 : 0) : 2));
			if (num != 2 && MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.GetPlayerTeam(MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify) != 1)
			{
				num = 1 - num;
			}
			BattleEnd(num);
			textTime.text = 0.ToString(format2) + format1 + 0.ToString(format2) + format1 + 0.ToString(format2);
		}
		else
		{
			timeLast = timeNow;
			minutes = (int)(timeLast / 60f) % 60;
			seconds = (int)(timeLast % 60f);
			milliseconds = (int)(timeLast * 100f) % 100;
			textTime.text = minutes.ToString(format2) + format1 + seconds.ToString(format2) + format1 + milliseconds.ToString(format2);
		}
	}

	public void BattleEnd(int nWinType)
	{
		if (isEnd)
		{
			return;
		}
		isEnd = true;
		MonoBehaviourSingleton<UpdateManager>.Instance.Pause = true;
		MonoBehaviourSingleton<GameLogicUpdateManager>.Instance.Stop();
		MonoBehaviourSingleton<OrangeGameManager>.Instance.IsGamePause = true;
		if (!bIsSeason && StageUpdate.runPlayers.Count == 0)
		{
			return;
		}
		foreach (OrangeCharacter runPlayer in StageUpdate.runPlayers)
		{
			runPlayer.HurtActions -= unitBlue.UpdateBar;
			runPlayer.HurtActions -= unitRed.UpdateBar;
		}
		LeanTween.value(0.1f, 1f, 2f).setOnUpdate(delegate(float f)
		{
			Time.timeScale = f;
		}).setOnComplete((Action)delegate
		{
			MonoBehaviourSingleton<AudioManager>.Instance.Stop(AudioChannelType.Sound);
			foreach (OrangeCharacter runPlayer2 in StageUpdate.runPlayers)
			{
				runPlayer2.StopAllLoopSE();
			}
			if (StageUpdate.bIsHost)
			{
				MonoBehaviourSingleton<StageSyncManager>.Instance.HostSendBattleEndToOther(nWinType, bIsSeason);
				MonoBehaviourSingleton<StageSyncManager>.Instance.SendStageEndAndOpenPvpEndUI(nWinType, bIsSeason);
			}
			else
			{
				if (bIsSeason)
				{
					MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.nWinType = nWinType;
				}
				MonoBehaviourSingleton<StageSyncManager>.Instance.RequestGetBattleEndMsg();
			}
		})
			.setIgnoreTimeScale(true);
	}

	public void SyncPlayerLife()
	{
		string text = "";
		text = unitBlue.GetNowLife() + "," + unitRed.GetNowLife() + "," + timeNow;
		StageUpdate.SyncStageObj(3, 8, text);
	}

	public void UpdageLifeByNet(int bluelife, int redlife, float settime)
	{
		if (bluelife > 0 && redlife > 0)
		{
			isEnd = false;
		}
		int playerTeam = MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.GetPlayerTeam(MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify);
		unitBlue.Init(this, playerTeam == 1, 3, bluelife);
		unitRed.Init(this, playerTeam == 2, 3, redlife);
		UpdateTimeText();
		timeNow = settime;
		diff = settime + Time.timeSinceLevelLoad;
		unitBlue.CalculateFill();
		unitRed.CalculateFill();
	}
}
