using System;
using UnityEngine;
using UnityEngine.UI;
using enums;

public class GuildChangePresidentListUI : OrangeUIBase
{
	private const int SCROLL_VISUAL_COUNT = 5;

	[SerializeField]
	private GuildPlayerInfoBeforeAfterHelper _playerInfoHelper;

	[SerializeField]
	private LoopVerticalScrollRect _scrollRect;

	[SerializeField]
	private GuildChangePresidentCell _scrollCell;

	[SerializeField]
	private Button _buttonConfirm;

	public string SelectedPlayerId { get; private set; }

	public event Action OnItemSelected;

	public void OnEnable()
	{
		Singleton<GuildSystem>.Instance.OnChangeMemberPrivilegeEvent += OnChangeMemberPrivilegeEvent;
		_buttonConfirm.interactable = false;
		string leaderPlayerID = Singleton<GuildSystem>.Instance.GuildInfoCache.LeaderPlayerID;
		_playerInfoHelper.SetPlayerInfoBefore(leaderPlayerID);
		_playerInfoHelper.SetPlayerInfoAfter(leaderPlayerID);
	}

	public void OnDisable()
	{
		Singleton<GuildSystem>.Instance.OnChangeMemberPrivilegeEvent -= OnChangeMemberPrivilegeEvent;
		this.OnItemSelected = null;
	}

	public void Setup()
	{
		base._EscapeEvent = EscapeEvent.CLOSE_UI;
		base.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
		LoopVerticalScrollRect scrollRect = _scrollRect;
		if ((object)scrollRect != null)
		{
			scrollRect.OrangeInit(_scrollCell, 5, Singleton<GuildSystem>.Instance.MemberInfoListCache.Count);
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

	public void OnClickCellSelectBtn(string playerId)
	{
		SelectedPlayerId = playerId;
		_playerInfoHelper.SetPlayerInfoAfter(playerId, GuildPrivilege.GuildLeader);
		_buttonConfirm.interactable = true;
		Action onItemSelected = this.OnItemSelected;
		if (onItemSelected != null)
		{
			onItemSelected();
		}
	}

	public void OnClickConfirmBtn()
	{
		PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_OK17);
		base.CloseSE = SystemSE.NONE;
		Singleton<GuildSystem>.Instance.ReqChangeMemberPrivilege(SelectedPlayerId, 1);
	}

	private void OnChangeMemberPrivilegeEvent(Code ackCode)
	{
		OnClickCloseBtn();
	}
}
