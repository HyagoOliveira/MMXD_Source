#define RELEASE
using System;
using CallbackDefs;
using UnityEngine;
using UnityEngine.UI;

public class GuildJoinUI : GuildListUIBase<GuildJoinUI, GuildJoinGuildCell>
{
	[SerializeField]
	private GameObject _goApplyCountOld;

	[SerializeField]
	private Text _textApplyCountOld;

	[SerializeField]
	private RectTransform _rectApplyCount;

	[SerializeField]
	private Text _textApplyCount;

	private GuildCell<GuildJoinUI> _lastReqCell;

	protected override void OnEnable()
	{
		base.OnEnable();
		Singleton<GuildSystem>.Instance.OnGetApplyGuildListEvent += OnGetApplyGuildListEvent;
		Singleton<GuildSystem>.Instance.OnJoinGuildEvent += OnJoinGuildEvent;
		Singleton<GuildSystem>.Instance.OnCancelJoinGuildEvent += OnCancelJoinGuildEvent;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		Singleton<GuildSystem>.Instance.OnGetApplyGuildListEvent -= OnGetApplyGuildListEvent;
		Singleton<GuildSystem>.Instance.OnJoinGuildEvent -= OnJoinGuildEvent;
		Singleton<GuildSystem>.Instance.OnCancelJoinGuildEvent -= OnCancelJoinGuildEvent;
	}

	public override void Setup()
	{
		base.Setup();
		_goApplyCountOld.SetActive(false);
		_rectApplyCount.gameObject.SetActive(true);
		RefreshApplyCount();
		Singleton<GuildSystem>.Instance.ReqGetApplyGuildList();
	}

	public void OnClickApplyListBtn()
	{
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI<GuildApplyGuildListUI>("UI_GuildApplyGuildList", OnApplyListUILoaded);
	}

	private void OnApplyListUILoaded(GuildApplyGuildListUI ui)
	{
		PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
		ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
		ui.Setup();
		ui.closeCB = (Callback)Delegate.Combine(ui.closeCB, new Callback(OnApplyListUIClosed));
	}

	private void OnApplyListUIClosed()
	{
		RefreshPage();
	}

	protected override void SearchGuild()
	{
		Singleton<GuildSystem>.Instance.ReqSearchGuild(_inputSearchString, Singleton<GuildSystem>.Instance.SearchGuildListCache.Count, ManagedSingleton<PlayerHelper>.Instance.GetBattlePower());
	}

	public void OnClickOneApplyBtn(GuildCell<GuildJoinUI> item, NetGuildInfo guildInfo)
	{
		_lastReqCell = item;
		if (Singleton<GuildSystem>.Instance.ApplyGuildListCache.Count >= OrangeConst.GUILD_APPLY_MAX)
		{
			CommonUIHelper.ShowCommonTipUI("GUILD_SCAN_CHECK");
		}
		else if (guildInfo.MemberCount >= item.MemberLimit)
		{
			CommonUIHelper.ShowCommonTipUI("GUILD_HALL_INVITEFAIL");
		}
		else if (guildInfo.PowerDemand > ManagedSingleton<PlayerHelper>.Instance.GetBattlePower())
		{
			CommonUIHelper.ShowCommonTipUI("GUILD_POWER_LACK");
		}
		else if (guildInfo.ApplyType != 0)
		{
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_GuildApplyConfirm", delegate(GuildApplyConfirmUI ui)
			{
				PlaySE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
				ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
				ui.Setup(guildInfo);
			});
		}
		else
		{
			PlaySE(SystemSE.CRI_SYSTEMSE_SYS_OK17);
			Singleton<GuildSystem>.Instance.ReqJoinGuild(guildInfo.GuildID, ManagedSingleton<PlayerHelper>.Instance.GetBattlePower());
		}
	}

	public void OnClickOneCancelApplyBtn(GuildCell<GuildJoinUI> item, int guildId)
	{
		_lastReqCell = item;
		Singleton<GuildSystem>.Instance.ReqCancelJoinGuild(guildId);
	}

	private void OnGetApplyGuildListEvent(Code ackCode)
	{
		Debug.Log("[OnGetApplyGuildListEvent]");
		SearchGuild();
	}

	private void OnJoinGuildEvent(Code ackCode)
	{
		Debug.Log(string.Format("[{0}] AckCode = {1}", "OnJoinGuildEvent", ackCode));
		switch (ackCode)
		{
		case Code.GUILD_JOIN_FREE_ADD_SUCCESS:
		case Code.GUILD_JOIN_APPLY_SUCCESS:
		{
			if (Singleton<GuildSystem>.Instance.GuildInfoCache != null)
			{
				OnClickCloseBtn();
				break;
			}
			GuildCell<GuildJoinUI> lastReqCell = _lastReqCell;
			if ((object)lastReqCell != null)
			{
				lastReqCell.RefreshCell();
			}
			_lastReqCell = null;
			RefreshApplyCount();
			break;
		}
		case Code.GUILD_LEAVE_COOLING_FAIL:
			CommonUIHelper.ShowCommonTipUI(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("GUILD_SETUP_WARN1", OrangeConst.GUILD_INVITE_TIME), false);
			break;
		case Code.GUILD_MEMBER_MAX:
			CommonUIHelper.ShowCommonTipUI("GUILD_HALL_INVITEFAIL");
			break;
		default:
			Debug.LogWarning(string.Format("Unhandled AckCode : {0}", ackCode));
			break;
		}
	}

	private void OnCancelJoinGuildEvent(Code ackCode)
	{
		Debug.Log(string.Format("[{0}] AckCode = {1}", "OnCancelJoinGuildEvent", ackCode));
		if (ackCode == Code.GUILD_REFUSE_JOIN_APPLY_SUCCESS)
		{
			GuildCell<GuildJoinUI> lastReqCell = _lastReqCell;
			if ((object)lastReqCell != null)
			{
				lastReqCell.RefreshCell();
			}
			_lastReqCell = null;
			RefreshApplyCount();
		}
	}

	protected override void RefreshPage()
	{
		base.RefreshPage();
		RefreshApplyCount();
	}

	private void RefreshApplyCount()
	{
		_textApplyCountOld.text = string.Format("{0}/{1}", Singleton<GuildSystem>.Instance.ApplyGuildListCache.Count, OrangeConst.GUILD_APPLY_MAX);
		_textApplyCount.text = string.Format("{0}/{1}", Singleton<GuildSystem>.Instance.ApplyGuildListCache.Count, OrangeConst.GUILD_APPLY_MAX);
		LayoutRebuilder.ForceRebuildLayoutImmediate(_rectApplyCount);
	}
}
