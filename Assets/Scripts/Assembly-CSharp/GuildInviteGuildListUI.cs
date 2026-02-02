#define RELEASE
using UnityEngine;
using UnityEngine.UI;

public class GuildInviteGuildListUI : OrangeUIBase
{
	private const int SCROLL_VISUAL_COUNT = 5;

	[SerializeField]
	private LoopVerticalScrollRect _scrollRect;

	[SerializeField]
	private GuildInviteGuildCell _scrollCell;

	[SerializeField]
	private GameObject _emptyHint;

	[SerializeField]
	private Text _textInviteLimit;

	private GuildCell<GuildInviteGuildListUI> _lastReqCell;

	public void OnEnable()
	{
		Singleton<GuildSystem>.Instance.OnGetInviteGuildListEvent += OnGetInviteGuildListEvent;
		Singleton<GuildSystem>.Instance.OnRefuseGuildInviteEvent += OnRefuseGuildInviteEvent;
		Singleton<GuildSystem>.Instance.OnAgreeGuildInviteEvent += OnAgreeGuildInviteEvent;
	}

	public void OnDisable()
	{
		Singleton<GuildSystem>.Instance.OnGetInviteGuildListEvent -= OnGetInviteGuildListEvent;
		Singleton<GuildSystem>.Instance.OnRefuseGuildInviteEvent -= OnRefuseGuildInviteEvent;
		Singleton<GuildSystem>.Instance.OnAgreeGuildInviteEvent -= OnAgreeGuildInviteEvent;
	}

	public void Setup()
	{
		Debug.Log("[Setup]");
		base._EscapeEvent = EscapeEvent.CLOSE_UI;
		_scrollRect.ClearCells();
		_textInviteLimit.text = string.Format("0/{0}", OrangeConst.GUILD_INVITE_MAX);
		Singleton<GuildSystem>.Instance.ReqGetInviteGuildList();
	}

	private void OnGetInviteGuildListEvent(Code ackCode)
	{
		Debug.Log("[OnGetInviteGuildListEvent]");
		RefreshCells();
	}

	public void OnClickOneAgreeBtn(GuildCell<GuildInviteGuildListUI> item, int guildId)
	{
		PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_OK17);
		_lastReqCell = item;
		Singleton<GuildSystem>.Instance.ReqAgreeGuildInvite(guildId);
	}

	public void OnClickOneRefuseBtn(GuildCell<GuildInviteGuildListUI> item, int guildId)
	{
		PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_CANCEL);
		_lastReqCell = item;
		Singleton<GuildSystem>.Instance.ReqRefuseGuildInvite(guildId);
	}

	private void RefreshCells()
	{
		_emptyHint.SetActive(Singleton<GuildSystem>.Instance.InviteGuildListCache.Count == 0);
		_scrollRect.ClearCells();
		_scrollRect.OrangeInit(_scrollCell, 5, Singleton<GuildSystem>.Instance.InviteGuildListCache.Count);
		_textInviteLimit.text = string.Format("{0}/{1}", Singleton<GuildSystem>.Instance.InviteGuildListCache.Count, OrangeConst.GUILD_INVITE_MAX);
	}

	private void OnAgreeGuildInviteEvent(Code ackCode)
	{
		switch (ackCode)
		{
		case Code.GUILD_AGREE_INVITE_SUCCESS:
			OnClickCloseBtn();
			break;
		case Code.GUILD_INVITE_NOT_FOUND_DATA:
			CommonUIHelper.ShowCommonTipUI("GUILD_HALL_INVITECANCEL", true, RefreshCells);
			break;
		case Code.GUILD_MEMBER_MAX:
			CommonUIHelper.ShowCommonTipUI("GUILD_HALL_INVITEFAIL", true, RefreshCells);
			break;
		default:
			Debug.LogWarning(string.Format("Unhandled AckCode : {0}", ackCode));
			break;
		}
	}

	private void OnRefuseGuildInviteEvent(Code ackCode)
	{
		switch (ackCode)
		{
		case Code.GUILD_REFUSE_INVITE_SUCCESS:
			RefreshCells();
			break;
		case Code.GUILD_INVITE_NOT_FOUND_DATA:
			CommonUIHelper.ShowCommonTipUI("GUILD_HALL_INVITECANCEL", true, RefreshCells);
			break;
		default:
			Debug.LogWarning(string.Format("Unhandled AckCode : {0}", ackCode));
			break;
		}
	}

	public void OnClickGuildInfoBtn(int guildId)
	{
		MonoBehaviourSingleton<UIManager>.Instance.Connecting(true);
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_GuildMemberList", delegate(GuildMemberListUI ui)
		{
			ui.Setup(guildId);
		});
	}
}
