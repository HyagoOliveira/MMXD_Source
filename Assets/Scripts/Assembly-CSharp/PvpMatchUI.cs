#define RELEASE
using System;
using OrangeSocket;
using UnityEngine;
using UnityEngine.UI;
using cm;
using enums;

public class PvpMatchUI : OrangeUIBase
{
	[SerializeField]
	private Text connectTime;

	private long time;

	private int matchUid = -1;

	private int cancelUid = -1;

	private int nReMatchTime;

	private bool isIgnoreFirstSE = true;

	public bool matchSucess;

	private void OnApplicationPause(bool pause)
	{
		if (pause)
		{
			if (matchUid != -1)
			{
				LeanTween.cancel(ref matchUid);
			}
			if (cancelUid != -1)
			{
				LeanTween.cancel(ref cancelUid);
			}
			MonoBehaviourSingleton<CMSocketClient>.Instance.SendProtocol(FlatBufferCMHelper.CreateRQStopPVPMatching());
			matchUid = -1;
			cancelUid = -1;
		}
		else
		{
			Match();
		}
	}

	public void Init()
	{
		Match();
		InvokeRepeating("UpdateConnectting", 1f, 1f);
		base._EscapeEvent = EscapeEvent.CLOSE_UI;
	}

	private void UpdateConnectting()
	{
		time++;
		connectTime.text = default(DateTime).AddSeconds(time).ToString("ss");
	}

	private void OnStartMatch()
	{
		matchUid = -1;
		if (!base.IsLock)
		{
			Debug.Log("[PvpMatchUI] OnStartMatch");
			MonoBehaviourSingleton<SignalDispatcher>.Instance.AddHandler(CM.RSPVPPersonnelStartMatching, OnRSPVPPersonnelStartMatching, 0, true);
			int pvptier = 0;
			if (MonoBehaviourSingleton<OrangeMatchManager>.Instance.PvpGameType == PVPGameType.OneVSOneSeason)
			{
				pvptier = ManagedSingleton<PlayerHelper>.Instance.GetSeasonTier();
				ManagedSingleton<StageHelper>.Instance.nStageEndGoUI = StageHelper.STAGE_END_GO.SEASONRANDOMMATCHING;
			}
			else
			{
				ManagedSingleton<StageHelper>.Instance.nStageEndGoUI = StageHelper.STAGE_END_GO.PVPRANDOMMATCHING;
			}
			MonoBehaviourSingleton<OrangeMatchManager>.Instance.LastRqPvpMatchType = MonoBehaviourSingleton<OrangeMatchManager>.Instance.PvpMatchType;
			MonoBehaviourSingleton<CMSocketClient>.Instance.SendProtocol(FlatBufferCMHelper.CreateRQPVPPersonnelStartMatching(pvptier, (int)MonoBehaviourSingleton<OrangeMatchManager>.Instance.LastRqPvpMatchType, MonoBehaviourSingleton<OrangeMatchManager>.Instance.StageID, MonoBehaviourSingleton<OrangeMatchManager>.Instance.SelfSealedBattleSetting));
		}
	}

	private void Match()
	{
		if (!isIgnoreFirstSE)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK17);
			isIgnoreFirstSE = true;
		}
		else
		{
			isIgnoreFirstSE = false;
		}
		time = 0L;
		matchUid = LeanTween.value(base.gameObject, 1f, 0f, 2f).setOnComplete(OnStartMatch).uniqueId;
		cancelUid = LeanTween.value(base.gameObject, 1f, 0f, OrangeConst.PVP_MATCHING_TIME).setOnComplete(MatchTimeoutDialog).uniqueId;
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_ACCESS01_LP);
		Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.SD_HOME_BGM);
	}

	private void OnRSPVPPersonnelStartMatching(object res)
	{
		if (res is RSPVPPersonnelStartMatching)
		{
			RSPVPPersonnelStartMatching rSPVPPersonnelStartMatching = (RSPVPPersonnelStartMatching)res;
			if (rSPVPPersonnelStartMatching.Result != 60100)
			{
				ManagedSingleton<StageHelper>.Instance.nStageEndGoUI = StageHelper.STAGE_END_GO.HOMETOP;
				RemoveHandler();
				MonoBehaviourSingleton<OrangeGameManager>.Instance.ShowErrorMsg((Code)rSPVPPersonnelStartMatching.Result, false);
			}
		}
	}

	private string CreateRandomName()
	{
		int max = ManagedSingleton<OrangeTextDataManager>.Instance.RANDOMNAME_TABLE_DICT.Count / 2 + 1;
		string p_key = "PREFIX_" + UnityEngine.Random.Range(1, max);
		string p_key2 = "POSTFIX_" + UnityEngine.Random.Range(1, max);
		return ManagedSingleton<OrangeTextDataManager>.Instance.RANDOMNAME_TABLE_DICT.GetL10nValue(p_key) + ManagedSingleton<OrangeTextDataManager>.Instance.RANDOMNAME_TABLE_DICT.GetL10nValue(p_key2);
	}

	private void MatchTimeoutDialog()
	{
		cancelUid = -1;
		nReMatchTime++;
		if (!MonoBehaviourSingleton<CMSocketClient>.Instance.Connected())
		{
			MonoBehaviourSingleton<OrangeMatchManager>.Instance.Host = ManagedSingleton<ServerConfig>.Instance.ServerSetting.Match.Host;
			MonoBehaviourSingleton<OrangeMatchManager>.Instance.Port = ManagedSingleton<ServerConfig>.Instance.ServerSetting.Match.Port;
			MonoBehaviourSingleton<OrangeMatchManager>.Instance.MatchServerLogin(delegate
			{
				MonoBehaviourSingleton<UIManager>.Instance.CloseLoadingUI(null);
			});
		}
		else
		{
			MonoBehaviourSingleton<CMSocketClient>.Instance.SendProtocol(FlatBufferCMHelper.CreateRQStopPVPMatching());
		}
		MonoBehaviourSingleton<UIManager>.Instance.LoadResourceUI("UI_CommonMsg", delegate(CommonUI ui)
		{
			ui.SetupYesNO(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("PVP_REMATCH_TITLE"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("PVP_REMATCH_MSG"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_OK"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_CANCEL"), Match, RemoveHandler);
		});
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_ACCESS01_STOP);
	}

	public void RemoveHandler()
	{
		LeanTween.cancel(ref matchUid);
		LeanTween.cancel(ref cancelUid);
		MonoBehaviourSingleton<CMSocketClient>.Instance.SendProtocol(FlatBufferCMHelper.CreateRQStopPVPMatching());
		if (!matchSucess)
		{
			OnClickCloseBtn();
		}
		else
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_ACCESS01_STOP);
		}
	}

	public override void OnClickCloseBtn()
	{
		base.OnClickCloseBtn();
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_ACCESS01_STOP);
	}

	public void OnClickCloseBtnSE()
	{
		if (!base.IsLock)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_BACK01);
			if (!matchSucess)
			{
				ManagedSingleton<StageHelper>.Instance.nStageEndGoUI = StageHelper.STAGE_END_GO.HOMETOP;
			}
			RemoveHandler();
		}
	}
}
