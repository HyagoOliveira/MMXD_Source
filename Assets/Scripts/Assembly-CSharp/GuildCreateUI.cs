#define RELEASE
using UnityEngine;
using UnityEngine.UI;
using enums;

public class GuildCreateUI : OrangeUIBase
{
	[SerializeField]
	private CommonGuildBadge _guildBadge;

	[SerializeField]
	private InputField _inputGuildName;

	[SerializeField]
	private InputField _inputGuildDesc;

	[SerializeField]
	private InputField _inputGuildAnnounce;

	[SerializeField]
	private PanelPowerDemandHelper _powerDemand;

	[SerializeField]
	private Toggle _toggleFreeJoin;

	[SerializeField]
	private Toggle _toggleNeedApply;

	[SerializeField]
	private Text _textNote;

	[SerializeField]
	private Text _textCreateCost;

	private Toggle _currentToggle;

	public void OnEnable()
	{
		Singleton<GuildSystem>.Instance.OnCreateGuildEvent += OnCreateGuildEvent;
	}

	public void OnDisable()
	{
		Singleton<GuildSystem>.Instance.OnCreateGuildEvent -= OnCreateGuildEvent;
	}

	public void Setup()
	{
		base._EscapeEvent = EscapeEvent.CLOSE_UI;
		_powerDemand.Setup(0, OrangeConst.GUILD_POWER_MAX);
		_toggleFreeJoin.isOn = true;
		_toggleNeedApply.isOn = false;
		_currentToggle = _toggleFreeJoin;
		_toggleFreeJoin.onValueChanged.AddListener(OnToggleFreeJoinValueChanged);
		_toggleNeedApply.onValueChanged.AddListener(OnToggleNeedApplyValueChanged);
		_textNote.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("GUILD_FOUND_NOTE", OrangeConst.GUILD_FOUND_LEVEL, OrangeConst.GUILD_FOUND_GEM);
		_textCreateCost.text = string.Format("x{0}", OrangeConst.GUILD_FOUND_GEM);
		_guildBadge.Setup(0, 0f);
	}

	private void OnToggleFreeJoinValueChanged(bool isOn)
	{
		if (isOn)
		{
			TogglePlaySE(_toggleFreeJoin);
		}
	}

	private void OnToggleNeedApplyValueChanged(bool isOn)
	{
		if (isOn)
		{
			TogglePlaySE(_toggleNeedApply);
		}
	}

	public void OnClickSubmitBtn()
	{
		if (string.IsNullOrWhiteSpace(_inputGuildName.text))
		{
			CommonUIHelper.ShowCommonTipUI("GUILD_NAME_ERROR");
			return;
		}
		if (OrangeDataReader.Instance.IsContainForbiddenName(_inputGuildName.text))
		{
			CommonUIHelper.ShowCommonTipUI("NAME_ERROR");
			return;
		}
		string message = _inputGuildAnnounce.text;
		OrangeDataReader.Instance.BlurChatMessage(ref message);
		_inputGuildAnnounce.text = message;
		message = _inputGuildDesc.text;
		OrangeDataReader.Instance.BlurChatMessage(ref message);
		_inputGuildDesc.text = message;
		base.CloseSE = SystemSE.NONE;
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI<CommonConsumeMsgUI>("UI_CommonConsumeMsg", OnCommonConsumeUILoaded);
	}

	private void OnCommonConsumeUILoaded(CommonConsumeMsgUI ui)
	{
		int gUILD_FOUND_GEM = OrangeConst.GUILD_FOUND_GEM;
		ui.YesSE = SystemSE.NONE;
		ui.CloseSE = SystemSE.NONE;
		ui.Setup(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_TIP"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("GUILD_FOUND_PROMPT", gUILD_FOUND_GEM), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_YES"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_NO"), ManagedSingleton<PlayerHelper>.Instance.GetTotalJewel(), gUILD_FOUND_GEM, OnGuildCreateConfirm);
	}

	private void OnGuildCreateConfirm()
	{
		GuildSystem instance = Singleton<GuildSystem>.Instance;
		InputField inputGuildName = _inputGuildName;
		string guildName = (((object)inputGuildName != null) ? inputGuildName.text : null) ?? string.Empty;
		InputField inputGuildDesc = _inputGuildDesc;
		string introdution = (((object)inputGuildDesc != null) ? inputGuildDesc.text : null) ?? string.Empty;
		InputField inputGuildAnnounce = _inputGuildAnnounce;
		instance.ReqCreateGuild(guildName, introdution, (((object)inputGuildAnnounce != null) ? inputGuildAnnounce.text : null) ?? string.Empty, _guildBadge.BadgeIndex, _guildBadge.BadgeColor, (!_toggleFreeJoin.isOn) ? GuildApplyType.Apply : GuildApplyType.Free, _powerDemand.Value);
	}

	public void OnClickEditBadgeBtn()
	{
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI<GuildBadgeEditorUI>("UI_GuildBadgeEditor", OnEditorUILoaded);
	}

	private void OnEditorUILoaded(GuildBadgeEditorUI ui)
	{
		PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
		ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
		ui.OnConfirmEvent += OnBadgeColorModified;
		ui.Setup(_guildBadge.BadgeIndex, _guildBadge.BadgeColor);
	}

	private void OnBadgeColorModified(int badgeIndex, float badgeColor)
	{
		_guildBadge.SetBadgeIndex(badgeIndex);
		_guildBadge.SetBadgeColor(badgeColor);
	}

	private void OnCreateGuildEvent(Code ackCode)
	{
		Debug.Log("[OnCreateGuildEvent]");
		if (ackCode == Code.GUILD_CREATE_SUCCESS)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_STORE01);
			if (Singleton<GuildSystem>.Instance.GuildInfoCache != null)
			{
				OnClickCloseBtn();
			}
		}
	}

	private void TogglePlaySE(Toggle nowToggle)
	{
		if (_currentToggle != nowToggle)
		{
			PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_OK01);
			_currentToggle = nowToggle;
		}
	}
}
