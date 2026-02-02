#define RELEASE
using UnityEngine;
using UnityEngine.UI;
using enums;

public class GuildSetupChildUI : OrangeChildUIBase
{
	[SerializeField]
	private InputField _inputGuildName;

	[SerializeField]
	private Text _textCostGuildNameEdit;

	[SerializeField]
	private CommonGuildBadge _guildBadge;

	[SerializeField]
	private Text _textCostGuildBadgeEdit;

	[SerializeField]
	private PanelPowerDemandHelper _powerDemand;

	[SerializeField]
	private InputField _inputIntroduction;

	[SerializeField]
	private InputField _inputAnnouncement;

	[SerializeField]
	private Toggle _toggleFreeJoin;

	[SerializeField]
	private Toggle _toggleNeedApply;

	[SerializeField]
	private Toggle _toggleAgreePrivilege;

	[SerializeField]
	private Toggle _toggleInvitePrivilege;

	[SerializeField]
	private Toggle _toggleKickMemberPrivilege;

	[SerializeField]
	private Toggle _togglePowerTowerTogglePrivilege;

	[SerializeField]
	private Toggle _togglePowerTowerUpgradePrivilege;

	[SerializeField]
	private Toggle _toggleOreLevelUpPrivilege;

	private bool _isInitializing = true;

	private Toggle _currentJoinToggle;

	private OrangeUIBase customUI;

	public void OnEnable()
	{
		Singleton<GuildSystem>.Instance.OnChangeGuildNameEvent += OnChangeGuildNameEvent;
		Singleton<GuildSystem>.Instance.OnChangeGuildPowerDemandEvent += OnChangeGuildPowerDemandEvent;
		Singleton<GuildSystem>.Instance.OnChangeGuildAnnouncementEvent += OnChangeGuildAnnouncementEvent;
		Singleton<GuildSystem>.Instance.OnChangeGuildIntroductionEvent += OnChangeGuildIntroductionEvent;
		Singleton<GuildSystem>.Instance.OnChangeGuildHeaderPowerEvent += OnChangeGuildHeaderPowerEvent;
	}

	public void OnDisable()
	{
		Singleton<GuildSystem>.Instance.OnChangeGuildNameEvent -= OnChangeGuildNameEvent;
		Singleton<GuildSystem>.Instance.OnChangeGuildPowerDemandEvent -= OnChangeGuildPowerDemandEvent;
		Singleton<GuildSystem>.Instance.OnChangeGuildAnnouncementEvent -= OnChangeGuildAnnouncementEvent;
		Singleton<GuildSystem>.Instance.OnChangeGuildIntroductionEvent -= OnChangeGuildIntroductionEvent;
		Singleton<GuildSystem>.Instance.OnChangeGuildHeaderPowerEvent -= OnChangeGuildHeaderPowerEvent;
	}

	public override void Setup()
	{
		NetGuildInfo guildInfoCache = Singleton<GuildSystem>.Instance.GuildInfoCache;
		_inputGuildName.text = guildInfoCache.GuildName;
		_textCostGuildNameEdit.text = string.Format("x{0}", OrangeConst.GUILD_CHANGE_NAME);
		_textCostGuildBadgeEdit.text = string.Format("x{0}", OrangeConst.GUILD_CHANGE_BADGE);
		_inputIntroduction.text = guildInfoCache.Introduction;
		_inputAnnouncement.text = guildInfoCache.Board;
		switch ((GuildApplyType)(short)guildInfoCache.ApplyType)
		{
		case GuildApplyType.Free:
			_currentJoinToggle = _toggleFreeJoin;
			break;
		case GuildApplyType.Apply:
			_currentJoinToggle = _toggleNeedApply;
			break;
		}
		_toggleFreeJoin.isOn = _currentJoinToggle == _toggleFreeJoin;
		_toggleNeedApply.isOn = _currentJoinToggle == _toggleNeedApply;
		GuildHeaderPower guildHeaderPower = (GuildHeaderPower)guildInfoCache.HeaderPower;
		_toggleAgreePrivilege.isOn = guildHeaderPower.HasFlag(GuildHeaderPower.Apply);
		_toggleInvitePrivilege.isOn = guildHeaderPower.HasFlag(GuildHeaderPower.Invite);
		_togglePowerTowerTogglePrivilege.isOn = guildHeaderPower.HasFlag(GuildHeaderPower.TowerSwitch);
		_togglePowerTowerUpgradePrivilege.isOn = guildHeaderPower.HasFlag(GuildHeaderPower.TowerLevelup);
		_toggleOreLevelUpPrivilege.isOn = guildHeaderPower.HasFlag(GuildHeaderPower.OreLevelup);
		_toggleKickMemberPrivilege.isOn = guildHeaderPower.HasFlag(GuildHeaderPower.KickMember);
		_powerDemand.Setup(guildInfoCache.PowerDemand, OrangeConst.GUILD_POWER_MAX);
		_guildBadge.SetBadgeIndex(guildInfoCache.Badge);
		_guildBadge.SetBadgeColor((float)guildInfoCache.BadgeColor / 360f);
		_isInitializing = false;
		MonoBehaviourSingleton<UIManager>.Instance.Connecting(false);
	}

	private void ToggleGroupSE(Toggle nowToggle)
	{
		if (nowToggle != _currentJoinToggle)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.Play("SystemSE", 2);
			_currentJoinToggle = nowToggle;
		}
	}

	public void OnClickChangeNameBtn()
	{
		if (string.IsNullOrWhiteSpace(_inputGuildName.text))
		{
			CommonUIHelper.ShowCommonTipUI("GUILD_NAME_ERROR");
		}
		else if (OrangeDataReader.Instance.IsContainForbiddenName(_inputGuildName.text))
		{
			CommonUIHelper.ShowCommonTipUI("NAME_ERROR");
		}
		else if (!(_inputGuildName.text == Singleton<GuildSystem>.Instance.GuildInfoCache.GuildName))
		{
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI<CommonConsumeMsgUI>("UI_CommonConsumeMsg", OnChangeNameConfirmUILoaded);
		}
	}

	private void OnChangeNameConfirmUILoaded(CommonConsumeMsgUI ui)
	{
		int gUILD_CHANGE_NAME = OrangeConst.GUILD_CHANGE_NAME;
		customUI = ui;
		ui.Setup(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_TIP"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("GUILD_SETUP_REVISE", gUILD_CHANGE_NAME), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_YES"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_NO"), ManagedSingleton<PlayerHelper>.Instance.GetTotalJewel(), gUILD_CHANGE_NAME, OnChangeNameConfirm);
	}

	private void OnChangeNameConfirm()
	{
		if (!string.IsNullOrWhiteSpace(_inputGuildName.text))
		{
			customUI.CloseSE = SystemSE.NONE;
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_STORE01);
		}
		Singleton<GuildSystem>.Instance.ReqChangeGuildName(_inputGuildName.text);
	}

	public void OnClickChangePowerDemandBtn()
	{
		int value = _powerDemand.Value;
		if (value != Singleton<GuildSystem>.Instance.GuildInfoCache.PowerDemand)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK17);
		}
		Singleton<GuildSystem>.Instance.ReqChangeGuildPowerDemand(value);
	}

	public void OnClickChangeAnnouncementBtn()
	{
		if (!string.IsNullOrWhiteSpace(_inputAnnouncement.text) && Singleton<GuildSystem>.Instance.GuildInfoCache.Board != _inputAnnouncement.text)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK17);
		}
		string message = _inputAnnouncement.text;
		OrangeDataReader.Instance.BlurChatMessage(ref message);
		_inputAnnouncement.text = message;
		Singleton<GuildSystem>.Instance.ReqChangeGuildAnnouncement(_inputAnnouncement.text);
	}

	public void OnClickChangeIntrodutionBtn()
	{
		if (!string.IsNullOrWhiteSpace(_inputIntroduction.text) && Singleton<GuildSystem>.Instance.GuildInfoCache.Introduction != _inputIntroduction.text)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK17);
		}
		string message = _inputIntroduction.text;
		OrangeDataReader.Instance.BlurChatMessage(ref message);
		_inputIntroduction.text = message;
		Singleton<GuildSystem>.Instance.ReqChangeGuildIntroduction(_inputIntroduction.text);
	}

	public void OnClickChangePresidentBtn()
	{
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI<GuildChangePresidentListUI>("UI_GuildChangePresidentList", OnChangePresidentListUILoaded);
	}

	private void OnChangePresidentListUILoaded(GuildChangePresidentListUI ui)
	{
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
		ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
		ui.Setup();
	}

	public void OnClickEditGuildBadgeBtn()
	{
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI<GuildBadgeEditorUI>("UI_GuildBadgeEditor", OnEditGuildBadgeUILoaded);
	}

	private void OnEditGuildBadgeUILoaded(GuildBadgeEditorUI ui)
	{
		NetGuildInfo guildInfoCache = Singleton<GuildSystem>.Instance.GuildInfoCache;
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
		ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
		ui.Setup(_guildBadge.BadgeIndex, _guildBadge.BadgeColor, false);
		ui.OnConfirmEvent += OnBadgeColorModified;
	}

	public void OnClickRemoveGuildBtn()
	{
		if (Singleton<CrusadeSystem>.Instance.CheckInEventTime())
		{
			MonoBehaviourSingleton<OrangeGameManager>.Instance.ShowTipDialog(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("GUILD_WARN_DISBAND"));
		}
		else
		{
			MonoBehaviourSingleton<UIManager>.Instance.LoadResourceUI<CommonUI>("UI_CommonMsg", OnRemoveGuildConfirmUILoaded);
		}
	}

	private void OnRemoveGuildConfirmUILoaded(CommonUI ui)
	{
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
		ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_OK17;
		ui.SetupYesNO(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_TIP"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("GUILD_SETUP_DISBANDCONFIRM"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_OK"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_CANCEL"), OnRemoveGuildConfirm, delegate
		{
			ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
		});
	}

	private void OnRemoveGuildConfirm()
	{
		Singleton<GuildSystem>.Instance.ReqRemoveGuild();
	}

	public void OnBadgeColorModified(int badgeIndex, float badgeColor)
	{
		_guildBadge.SetBadgeIndex(badgeIndex);
		_guildBadge.SetBadgeColor(badgeColor);
	}

	public void OnToggleFreeJoinValueChanged()
	{
		if (_toggleFreeJoin.isOn && !_isInitializing)
		{
			ToggleGroupSE(_toggleFreeJoin);
			Singleton<GuildSystem>.Instance.ReqChangeGuildApplyType(GuildApplyType.Free);
		}
	}

	public void OnToggleNeedApplyValueChanged()
	{
		if (_toggleNeedApply.isOn && !_isInitializing)
		{
			ToggleGroupSE(_toggleNeedApply);
			Singleton<GuildSystem>.Instance.ReqChangeGuildApplyType(GuildApplyType.Apply);
		}
	}

	public void OnToggleHeaderPowerValueChanged()
	{
		GuildHeaderPower guildHeaderPower = GuildHeaderPower.None;
		if (_toggleAgreePrivilege.isOn)
		{
			guildHeaderPower |= GuildHeaderPower.Apply;
		}
		if (_toggleInvitePrivilege.isOn)
		{
			guildHeaderPower |= GuildHeaderPower.Invite;
		}
		if (_togglePowerTowerTogglePrivilege.isOn)
		{
			guildHeaderPower |= GuildHeaderPower.TowerSwitch;
		}
		if (_togglePowerTowerUpgradePrivilege.isOn)
		{
			guildHeaderPower |= GuildHeaderPower.TowerLevelup;
		}
		if (_toggleOreLevelUpPrivilege.isOn)
		{
			guildHeaderPower |= GuildHeaderPower.OreLevelup;
		}
		if (_toggleKickMemberPrivilege.isOn)
		{
			guildHeaderPower |= GuildHeaderPower.KickMember;
		}
		if (!_isInitializing)
		{
			Singleton<GuildSystem>.Instance.ReqChangeGuildHeaderPower((int)guildHeaderPower);
		}
	}

	private void OnChangeGuildNameEvent(Code ackCode, NetGuildInfo guildInfo)
	{
		Debug.Log(string.Format("[{0}] AckCode = {1}", "OnChangeGuildNameEvent", ackCode));
		if (ackCode == Code.GUILD_CHANGE_NAME_SUCCESS && _inputGuildName != null)
		{
			_inputGuildName.text = guildInfo.GuildName;
		}
	}

	private void OnChangeGuildPowerDemandEvent(Code ackCode, NetGuildInfo guildInfo)
	{
		Debug.Log(string.Format("[{0}] AckCode = {1}", "OnChangeGuildPowerDemandEvent", ackCode));
		if (ackCode == Code.GUILD_CHANGE_POWER_DEMAND_SUCCESS && _powerDemand != null)
		{
			_powerDemand.Value = guildInfo.PowerDemand;
		}
	}

	private void OnChangeGuildAnnouncementEvent(Code ackCode, NetGuildInfo guildInfo)
	{
		Debug.Log(string.Format("[{0}] AckCode = {1}", "OnChangeGuildAnnouncementEvent", ackCode));
		if (ackCode == Code.GUILD_CHANGE_BOARD_SUCCESS && _inputAnnouncement != null)
		{
			_inputAnnouncement.text = guildInfo.Board;
		}
	}

	private void OnChangeGuildIntroductionEvent(Code ackCode, NetGuildInfo guildInfo)
	{
		Debug.Log(string.Format("[{0}] AckCode = {1}", "OnChangeGuildIntroductionEvent", ackCode));
		if (ackCode == Code.GUILD_CHANGE_INTRODUCTION_SUCCESS && _inputIntroduction != null)
		{
			_inputIntroduction.text = guildInfo.Introduction;
		}
	}

	private void OnChangeGuildHeaderPowerEvent(Code ackCode, NetGuildInfo guildInfo)
	{
		Debug.Log(string.Format("[{0}] AckCode = {1}", "OnChangeGuildHeaderPowerEvent", ackCode));
	}
}
