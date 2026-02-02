using System;
using UnityEngine;
using UnityEngine.UI;
using enums;

public class GuildChangePrivilegeReplaceConfirmUI : OrangeUIBase
{
	[SerializeField]
	private Text _textNote;

	[SerializeField]
	private GuildPlayerInfoBeforeAfterHelper _playerInfoController;

	public event Action OnConfirmEvent;

	public void OnEnable()
	{
	}

	public void OnDisable()
	{
		this.OnConfirmEvent = null;
	}

	public void Setup(string memberId)
	{
		base._EscapeEvent = EscapeEvent.CLOSE_UI;
		SocketPlayerHUD value;
		string text = (MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicHUD.TryGetValue(memberId, out value) ? value.m_Name : "---");
		_textNote.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("GUILD_HALL_RANKREMOVE", text);
		_playerInfoController.SetPlayerInfoBefore(memberId);
		_playerInfoController.SetPlayerInfoAfter(memberId, GuildPrivilege.GuildMember);
	}

	public void OnClickConfirmBtn()
	{
		Action onConfirmEvent = this.OnConfirmEvent;
		if (onConfirmEvent != null)
		{
			onConfirmEvent();
		}
		OnClickCloseBtn();
	}
}
