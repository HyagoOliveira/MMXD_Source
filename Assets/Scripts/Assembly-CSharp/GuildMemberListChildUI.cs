using UnityEngine;
using UnityEngine.UI;

public class GuildMemberListChildUI : OrangeChildUIBase
{
	private const int SCROLL_VISUAL_COUNT = 5;

	[SerializeField]
	private Text _textMemberCount;

	[SerializeField]
	private LoopVerticalScrollRect _scrollRect;

	[SerializeField]
	private GuildLobbyMemberCell _scrollCell;

	private GuildLobbyUI _parentUI;

	private void OnEnable()
	{
		Singleton<GuildSystem>.Instance.OnChangeMemberPrivilegeEvent += OnChangeMemberPrivilegeEvent;
		Singleton<GuildSystem>.Instance.OnKickMemberEvent += OnKickMemberEvent;
		Singleton<GuildSystem>.Instance.OnSocketMemberPrivilegeChangedEvent += OnSocketMemberPrivilegeChangedEvent;
	}

	private void OnDisable()
	{
		Singleton<GuildSystem>.Instance.OnChangeMemberPrivilegeEvent -= OnChangeMemberPrivilegeEvent;
		Singleton<GuildSystem>.Instance.OnKickMemberEvent -= OnKickMemberEvent;
		Singleton<GuildSystem>.Instance.OnSocketMemberPrivilegeChangedEvent -= OnSocketMemberPrivilegeChangedEvent;
	}

	public override void Setup()
	{
		if (_parentUI == null)
		{
			_parentUI = base.gameObject.GetComponentInParent<GuildLobbyUI>();
		}
		RefreshCells();
	}

	private void RefreshMemberList()
	{
		Singleton<GuildSystem>.Instance.ReqGetMemberInfoList();
	}

	private void RefreshCells()
	{
		if (Singleton<GuildSystem>.Instance.GuildInfoCache != null)
		{
			Clear();
			int count = Singleton<GuildSystem>.Instance.MemberInfoListCache.Count;
			_textMemberCount.text = string.Format("{0}/{1}", count, Singleton<GuildSystem>.Instance.GuildSetting.MemberLimit);
			LoopVerticalScrollRect scrollRect = _scrollRect;
			if ((object)scrollRect != null)
			{
				scrollRect.OrangeInit(_scrollCell, 5, count);
			}
		}
	}

	public void Clear()
	{
		LoopVerticalScrollRect scrollRect = _scrollRect;
		if ((object)scrollRect != null)
		{
			scrollRect.ClearCells();
		}
	}

	public void OnClickMemberCell(string playerId, Vector2 tarPos)
	{
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_GuildSubMenu", delegate(GuildSubMenuUI ui)
		{
			ui.Setup(playerId, tarPos, true);
		});
	}

	private void OnChangeMemberPrivilegeEvent(Code ackCode)
	{
		if (ackCode == Code.GUILD_CHANGE_PRIVILEGE_FAIL || ackCode == Code.GUILD_PRIVILEGE_TYPE_FAIL)
		{
			CommonUIHelper.ShowCommonTipUI("GUILD_HALL_RANKALTER", true, RefreshMemberList);
		}
		else
		{
			RefreshCells();
		}
	}

	private void OnKickMemberEvent(Code ackCode, string memberId)
	{
		RefreshCells();
	}

	private void OnSocketMemberPrivilegeChangedEvent(bool isSelfPrivilegeChanged)
	{
		if (isSelfPrivilegeChanged)
		{
			RefreshCells();
		}
	}
}
