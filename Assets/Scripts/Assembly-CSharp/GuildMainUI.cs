using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using enums;

public class GuildMainUI : OrangeUIBase
{
	[SerializeField]
	private Text _guildName;

	[SerializeField]
	private Text _guildAnnouncement;

	[SerializeField]
	private Text _guildMoney;

	[SerializeField]
	private Text _guildDonate;

	[SerializeField]
	private CommonGuildBadge _guildBadge;

	[SerializeField]
	private GuildRankInfoHelper _guildRankInfoHelper;

	[SerializeField]
	private GuildEddieRewardProgressHelper _eddieRewardProgressHelper;

	[SerializeField]
	private GameObject _chatUIObj;

	public void Start()
	{
		MonoBehaviourSingleton<UIManager>.Instance.OnUILinkPrepareEvent += OnUILinkPrepareEvent;
		Singleton<GenericEventManager>.Instance.AttachEvent(EventManager.ID.BACK_TO_HOMETOP, OnBackToHometop);
		Singleton<GuildSystem>.Instance.OnGetGuildInfoEvent += OnGetGuildInfoEvent;
		Singleton<GuildSystem>.Instance.OnRankupGuildEvent += OnRankupGuildEvent;
		Singleton<GuildSystem>.Instance.OnChangeGuildNameEvent += OnChangeGuildNameEvent;
		Singleton<GuildSystem>.Instance.OnChangeGuildAnnouncementEvent += OnChangeGuildAnnouncementEvent;
		Singleton<GuildSystem>.Instance.OnEditBadgeEvent += OnEditBadgeEvent;
		Singleton<GuildSystem>.Instance.OnDonateEvent += OnDonateEvent;
		Singleton<GuildSystem>.Instance.OnEddieDonateEvent += OnEddieDonateEvent;
		Singleton<GuildSystem>.Instance.OnLeaveGuildEvent += OnLeaveGuildEvent;
		Singleton<GuildSystem>.Instance.OnRemoveGuildEvent += OnRemoveGuildEvent;
		Singleton<GuildSystem>.Instance.OnGetEddieBoxGachaRecordEvent += OnGetEddieBoxGachaRecordEvent;
	}

	private void OnDestroy()
	{
		MonoBehaviourSingleton<UIManager>.Instance.OnUILinkPrepareEvent -= OnUILinkPrepareEvent;
		Singleton<GenericEventManager>.Instance.DetachEvent(EventManager.ID.BACK_TO_HOMETOP, OnBackToHometop);
		Singleton<GuildSystem>.Instance.OnGetGuildInfoEvent -= OnGetGuildInfoEvent;
		Singleton<GuildSystem>.Instance.OnRankupGuildEvent -= OnRankupGuildEvent;
		Singleton<GuildSystem>.Instance.OnChangeGuildNameEvent -= OnChangeGuildNameEvent;
		Singleton<GuildSystem>.Instance.OnChangeGuildAnnouncementEvent -= OnChangeGuildAnnouncementEvent;
		Singleton<GuildSystem>.Instance.OnEditBadgeEvent -= OnEditBadgeEvent;
		Singleton<GuildSystem>.Instance.OnDonateEvent -= OnDonateEvent;
		Singleton<GuildSystem>.Instance.OnEddieDonateEvent -= OnEddieDonateEvent;
		Singleton<GuildSystem>.Instance.OnLeaveGuildEvent -= OnLeaveGuildEvent;
		Singleton<GuildSystem>.Instance.OnRemoveGuildEvent -= OnRemoveGuildEvent;
		Singleton<GuildSystem>.Instance.OnGetEddieBoxGachaRecordEvent -= OnGetEddieBoxGachaRecordEvent;
		if (TurtorialUI.IsTutorialing())
		{
			TurtorialUI.ForceCloseTutorial();
			DialogUI uI = MonoBehaviourSingleton<UIManager>.Instance.GetUI<DialogUI>("UI_Dialog");
			if ((object)uI != null)
			{
				uI.OnClickCloseBtn();
			}
		}
	}

	private void OnUILinkPrepareEvent()
	{
		Singleton<GuildSystem>.Instance.CloseGuildLobbyScene(delegate
		{
			PlayHomeBgm();
		});
	}

	protected override void OnBackToHometop()
	{
		base.OnClickCloseBtn();
	}

	public override void OnClickCloseBtn()
	{
		GuildUIHelper.OpenLoadingUI(delegate
		{
			PlayHomeBgm();
			base.OnClickCloseBtn();
		});
	}

	public void Setup()
	{
		MonoBehaviourSingleton<AudioManager>.Instance.PlayBGM("BGM02", "bgm_sys_guild");
		_chatUIObj.SetActive(MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.Setting.Community);
		OnGetGuildInfoEvent(Singleton<GuildSystem>.Instance.GuildInfoCache);
	}

	public void RefreshGuildInfo()
	{
		OnGetGuildInfoEvent(Singleton<GuildSystem>.Instance.GuildInfoCache);
	}

	public void OnClickChannelBtn()
	{
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_Channel", delegate(ChannelUI ui)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
			ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
			ui.Setup(ChatChannel.GuildChannel);
		});
	}

	public void OnClickEddieDonateBtn()
	{
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI<GuildEddieDonateUI>("UI_GuildEddieDonate", OnEddieDonateUILoaded);
	}

	private void OnEddieDonateUILoaded(GuildEddieDonateUI ui)
	{
		int zenny = ManagedSingleton<PlayerHelper>.Instance.GetZenny();
		int maxValue = IntMath.Min(zenny, OrangeConst.GUILD_BOX_EDDIEDONATEMAX);
		PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
		ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
		ui.Setup(maxValue, zenny);
	}

	private void OnGetGuildInfoEvent(Code ackCode)
	{
		if (ackCode == Code.GUILD_GET_INFO_SUCCESS)
		{
			RefreshGuildInfo();
		}
	}

	private void OnRankupGuildEvent(Code ackCode, NetGuildInfo guildInfo)
	{
		if (ackCode == Code.GUILD_RANK_UP_SUCCESS)
		{
			OnGetGuildInfoEvent(guildInfo);
		}
	}

	private void OnChangeGuildNameEvent(Code ackCode, NetGuildInfo guildInfo)
	{
		if (ackCode == Code.GUILD_CHANGE_NAME_SUCCESS)
		{
			OnGetGuildInfoEvent(guildInfo);
		}
	}

	private void OnChangeGuildAnnouncementEvent(Code ackCode, NetGuildInfo guildInfo)
	{
		if (ackCode == Code.GUILD_CHANGE_BOARD_SUCCESS)
		{
			OnGetGuildInfoEvent(guildInfo);
		}
	}

	private void OnEditBadgeEvent(Code ackCode, NetGuildInfo guildInfo)
	{
		if (ackCode == Code.GUILD_EDIT_BADGE_SUCCESS)
		{
			OnGetGuildInfoEvent(guildInfo);
		}
	}

	private void OnDonateEvent(Code ackCode, NetGuildInfo guildInfo)
	{
		if (ackCode == Code.GUILD_DONATE_SUCCESS)
		{
			OnGetGuildInfoEvent(guildInfo);
		}
	}

	private void OnEddieDonateEvent(Code ackCode, NetGuildInfo guildInfo)
	{
		if (ackCode == Code.GUILD_EDDIE_DONATE_SUCCESS)
		{
			OnGetGuildInfoEvent(guildInfo);
		}
	}

	private void OnGetGuildInfoEvent(NetGuildInfo guildInfo)
	{
		if (guildInfo != null)
		{
			_guildName.text = guildInfo.GuildName;
			_guildAnnouncement.text = guildInfo.Board;
			_guildBadge.SetBadgeIndex(guildInfo.Badge);
			_guildBadge.SetBadgeColor((float)guildInfo.BadgeColor / 360f);
			GuildSetting guildSetting = Singleton<GuildSystem>.Instance.GuildSetting;
			_guildRankInfoHelper.Setup(guildInfo, guildSetting, Singleton<GuildSystem>.Instance.SelfMemberInfo.Privilege == 1);
			_guildMoney.text = guildInfo.Money.ToString("#,0");
			_guildDonate.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("GUILD_BOX_TODAY", guildInfo.EddieMoney.ToString("#,0"));
			_eddieRewardProgressHelper.Setup(guildInfo.EddieMoney, guildSetting.EddieDonateSettings);
		}
	}

	private void OnLeaveGuildEvent(Code ackCode)
	{
		if ((uint)(ackCode - 105300) <= 2u)
		{
			OnClickCloseBtn();
		}
	}

	private void OnRemoveGuildEvent(Code ackCode)
	{
		if (ackCode == Code.GUILD_REMOVE_SUCCESS)
		{
			OnClickCloseBtn();
		}
	}

	public void OnClickEddieRewardBtn()
	{
		PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
		Singleton<GuildSystem>.Instance.ReqGetEddieBoxGachaRecord();
	}

	private void OnGetEddieBoxGachaRecordEvent(Code ackCode, int rank, int donateValue, List<NetEddieBoxGachaRecord> boxList, string mvpPlayerId)
	{
		if (ackCode != Code.GUILD_GET_EDDIE_BOX_GACHA_RECORD_SUCCESS)
		{
			return;
		}
		IEnumerable<string> targetIds = boxList.Select((NetEddieBoxGachaRecord record) => record.PlayerID);
		Singleton<GuildSystem>.Instance.SearchHUD(targetIds, delegate
		{
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_GuildEddieReward", delegate(GuildEddieRewardUI ui)
			{
				ui.Setup(rank, donateValue, boxList, mvpPlayerId);
			});
		});
	}

	private void PlayHomeBgm()
	{
		Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.SD_HOME_BGM);
	}
}
