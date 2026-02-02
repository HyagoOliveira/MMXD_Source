#define RELEASE
using System;
using UnityEngine;
using UnityEngine.UI;

public class GuildInviteConfirmUI : OrangeUIBase
{
	[SerializeField]
	private Text _textPlayerName;

	[SerializeField]
	private Text _textPlayePower;

	[SerializeField]
	private Text _textPlayeLevel;

	[SerializeField]
	private GameObject _playerIconRoot;

	[SerializeField]
	private InputField _inputInviteMsg;

	private string _targetPlayerId;

	public event Action OnInviteConfirmEvent;

	public void OnDestroy()
	{
		this.OnInviteConfirmEvent = null;
	}

	public void OnEnable()
	{
		Singleton<GuildSystem>.Instance.OnInvitePlayerEvent += OnInvitePlayerEvent;
	}

	public void OnDisable()
	{
		Singleton<GuildSystem>.Instance.OnInvitePlayerEvent -= OnInvitePlayerEvent;
	}

	public void Setup(string tarPlayerId)
	{
		base._EscapeEvent = EscapeEvent.CLOSE_UI;
		_targetPlayerId = tarPlayerId;
		SetPlayerInfo();
	}

	private void SetPlayerInfo()
	{
		SocketPlayerHUD playerHUD;
		if (MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicHUD.TryGetValue(_targetPlayerId, out playerHUD))
		{
			_textPlayerName.text = playerHUD.m_Name;
			_textPlayePower.text = playerHUD.m_Power.ToString();
			_textPlayeLevel.text = string.Format("Lv{0}", playerHUD.m_Level);
			CommonAssetHelper.LoadPlayerIcon(_playerIconRoot, delegate(PlayerIconBase playerIcon)
			{
				CommonUIHelper.SetPlayerIcon(playerIcon, playerHUD.m_IconNumber, 0.8f);
			});
		}
		else
		{
			_textPlayerName.text = "---";
			_textPlayePower.text = "0";
			_textPlayeLevel.text = "Lv-";
		}
	}

	public void OnClickConfirmBtn()
	{
		SocketPlayerHUD value;
		if (MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicHUD.TryGetValue(_targetPlayerId, out value) && value.m_Level < OrangeConst.OPENRANK_GUILD)
		{
			CommonUIHelper.ShowCommonTipUI("GUILD_HALL_WARN6", true, OnConfirmPlayerLevelNotEnough);
			return;
		}
		PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_OK17);
		base.CloseSE = SystemSE.NONE;
		Singleton<GuildSystem>.Instance.ReqInvitePlayer(_targetPlayerId, _inputInviteMsg.text);
	}

	private void OnInvitePlayerEvent(Code ackCode)
	{
		switch (ackCode)
		{
		case Code.GUILD_LEAVE_COOLING_FAIL:
			CommonUIHelper.ShowCommonTipUI(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("GUILD_SETUP_WARN2", OrangeConst.GUILD_INVITE_TIME), false);
			break;
		case Code.GUILD_MEMBER_MAX:
			CommonUIHelper.ShowCommonTipUI("GUILD_SETUP_WARN4");
			break;
		default:
			Debug.LogWarning(string.Format("Unhandled AckCode : {0}", ackCode));
			break;
		case Code.GUILD_INVITE_SUCCESS:
			break;
		}
		Action onInviteConfirmEvent = this.OnInviteConfirmEvent;
		if (onInviteConfirmEvent != null)
		{
			onInviteConfirmEvent();
		}
		OnClickCloseBtn();
	}

	private void OnConfirmPlayerLevelNotEnough()
	{
		Action onInviteConfirmEvent = this.OnInviteConfirmEvent;
		if (onInviteConfirmEvent != null)
		{
			onInviteConfirmEvent();
		}
		OnClickCloseBtn();
	}
}
