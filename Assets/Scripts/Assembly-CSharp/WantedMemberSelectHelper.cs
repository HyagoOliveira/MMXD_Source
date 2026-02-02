using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using enums;

public class WantedMemberSelectHelper : OrangeChildUIBase
{
	[SerializeField]
	private WantedMemberSelectUnitHelper[] _memberSelectUnits;

	[SerializeField]
	private WantedMemberSelectUnitHelper _friendSelectUnit;

	[SerializeField]
	private Text _textTimeHint;

	[SerializeField]
	private Text _textAPHint;

	[SerializeField]
	private Button _buttonAuto;

	[SerializeField]
	private Button _buttonFriend;

	[SerializeField]
	private Text _textHelpCount;

	[SerializeField]
	private Color _colorHelpCountZero;

	[SerializeField]
	private Button _buttonDeparture;

	private int _index;

	private WANTED_TABLE _wantedAttrData;

	private WantedConditionFlag _conditionFlag;

	private int _conditionLevel;

	private List<WantedMemberInfo> _memberInfoListCache = new List<WantedMemberInfo>();

	private WantedMemberInfo _friendInfoCache;

	private OrangeUIBase _confirmUI;

	public event Action<WantedConditionFlag, int> OnMemberListUpdatedEvent;

	private void Awake()
	{
		WantedMemberSelectUnitHelper[] memberSelectUnits = _memberSelectUnits;
		for (int i = 0; i < memberSelectUnits.Length; i++)
		{
			memberSelectUnits[i].OnSelectButtonClicked += OnMemberUnitClicked;
		}
		_friendSelectUnit.OnSelectButtonClicked += OnFriendUnitClicked;
	}

	private void OnDestroy()
	{
		if (_confirmUI != null)
		{
			_confirmUI.OnClickCloseBtn();
			_confirmUI = null;
		}
	}

	private void OnEnable()
	{
		Singleton<WantedSystem>.Instance.OnWantedStartEvent += OnWantedStartEvent;
	}

	private void OnDisable()
	{
		Singleton<WantedSystem>.Instance.OnWantedStartEvent -= OnWantedStartEvent;
	}

	public void Setup(int index, WANTED_TABLE wantedAttrData)
	{
		_index = index;
		_wantedAttrData = wantedAttrData;
		ResetMembers();
		TimeSpan timeSpan = TimeSpan.FromSeconds(wantedAttrData.n_WANTEDTIME);
		_textTimeHint.text = string.Format("{0:00}:{1:00}:{2:00}", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);
		_textAPHint.text = string.Format("{0}", wantedAttrData.n_WANTEDAP);
		int num = OrangeConst.GUILD_WANTED_HELP - Singleton<WantedSystem>.Instance.UsedHelpCount;
		_textHelpCount.text = ((num > 0) ? string.Format("{0}/{1}", num, OrangeConst.GUILD_WANTED_HELP) : string.Format("<color=#{0}>{1}</color>/{2}", ColorUtility.ToHtmlStringRGB(_colorHelpCountZero), num, OrangeConst.GUILD_WANTED_HELP));
	}

	public override void CloseUI()
	{
		base.CloseUI();
	}

	private void ResetMembers()
	{
		_memberInfoListCache.Clear();
		_friendInfoCache = null;
		RefreshUnits();
	}

	private void RefreshUnits(bool needRefreshCondition = true)
	{
		ResetUnits();
		for (int i = 0; i < _memberInfoListCache.Count; i++)
		{
			WantedMemberSelectUnitHelper obj = _memberSelectUnits[i];
			WantedMemberInfo memberInfo = _memberInfoListCache[i];
			obj.Setup(memberInfo);
		}
		if (_friendInfoCache != null)
		{
			_friendSelectUnit.Setup(_friendInfoCache);
		}
		if (needRefreshCondition)
		{
			RefreshConditionFlag();
		}
		SendUpdateMemberListEvent();
	}

	private void ResetUnits()
	{
		WantedMemberSelectUnitHelper[] memberSelectUnits = _memberSelectUnits;
		for (int i = 0; i < memberSelectUnits.Length; i++)
		{
			memberSelectUnits[i].Reset();
		}
		_friendSelectUnit.Reset();
	}

	public void OnClickAutoButton()
	{
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK17);
		_memberInfoListCache.Clear();
		_friendInfoCache = null;
		_conditionFlag = WantedConditionFlag.None;
		_conditionLevel = 0;
		ResetUnits();
		List<NetCharacterInfo> characterInfoList;
		WantedConditionFlag conditionFlag;
		int conditionLevel;
		if (Singleton<WantedSystem>.Instance.TryGetRecommendedMemberList(_wantedAttrData, out characterInfoList, out conditionFlag, out conditionLevel))
		{
			OnConfirmRecommendedMemberList(characterInfoList, conditionFlag, conditionLevel);
		}
		else
		{
			CommonUIHelper.ShowCommonTipUI("GUILD_WANTED_WARN_3");
		}
	}

	public void OnClickFriendButton()
	{
		OnFriendUnitClicked();
	}

	public void OnClickDepartureButton()
	{
		if (ManagedSingleton<PlayerHelper>.Instance.GetStamina() < _wantedAttrData.n_WANTEDAP)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
			MonoBehaviourSingleton<UIManager>.Instance.LoadResourceUI<CommonUI>("UI_CommonMsg", OnConfirmBuyAPUILoaded);
		}
		else if (!_conditionFlag.HasFlag(WantedConditionFlag.BasicCondition))
		{
			CommonUIHelper.ShowCommonTipUI("GUILD_WANTED_INCOMPATIBLE");
		}
		else
		{
			Singleton<WantedSystem>.Instance.ReqWantedStart((sbyte)(_index + 1), _memberInfoListCache.Select((WantedMemberInfo memberInfo) => memberInfo.CharacterInfo.CharacterID).ToList(), (_friendInfoCache != null) ? _friendInfoCache.HelpInfo : null);
		}
	}

	private void OnMemberUnitClicked()
	{
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI<WantedMemberSelectUI>("UI_WantedMemberSelect", OnMemberSelectUILoaded);
	}

	private void OnFriendUnitClicked()
	{
		if (Singleton<WantedSystem>.Instance.UsedHelpCount < OrangeConst.GUILD_WANTED_HELP)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI<WantedFriendSelectUI>("UI_WantedFriendSelect", OnFriendSelectUILoaded);
		}
		else
		{
			CommonUIHelper.ShowCommonTipUI("GUILD_WANTED_WARN_4");
		}
	}

	private void OnWantedStartEvent(Code ackCode)
	{
		if (ackCode == Code.WANTED_START_SUCCESS)
		{
			CloseUI();
		}
	}

	private void OnMemberSelectUILoaded(WantedMemberSelectUI ui)
	{
		ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
		ui.Setup(_memberInfoListCache.Select((WantedMemberInfo memberInfo) => memberInfo.CharacterInfo).ToList(), Singleton<WantedSystem>.Instance.SortType, Singleton<WantedSystem>.Instance.IsSortDescend);
		ui.OnSortTypeChangedEvent += OnSortTypeChangedEvent;
		ui.OnConfirmMembersEvent += OnConfirmMemberList;
	}

	private void OnFriendSelectUILoaded(WantedFriendSelectUI ui)
	{
		ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
		ui.Setup(_friendInfoCache, Singleton<WantedSystem>.Instance.SortType, Singleton<WantedSystem>.Instance.IsSortDescend);
		ui.OnSortTypeChangedEvent += OnSortTypeChangedEvent;
		ui.OnConfirmFriendEvent += OnConfirmFriend;
	}

	private void OnConfirmBuyAPUILoaded(CommonUI ui)
	{
		ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
		ui.YesSE = SystemSE.NONE;
		ui.SetupYesNoByKey("COMMON_TIP", "STAMINA_OUT", "COMMON_OK", "COMMON_CANCEL", OnConfirmChargeStamina, OnMsgUIClosed);
		_confirmUI = ui;
	}

	private void OnChargeStaminaUILoaded(ChargeStaminaUI ui)
	{
		ui.Setup(ChargeType.ActionPoint);
		ui.closeCB = OnMsgUIClosed;
		_confirmUI = ui;
	}

	private void OnSortTypeChangedEvent(CharacterHelper.SortType sortType, bool isSortDescend)
	{
		Singleton<WantedSystem>.Instance.SortType = sortType;
		Singleton<WantedSystem>.Instance.IsSortDescend = isSortDescend;
	}

	private void OnConfirmRecommendedMemberList(List<NetCharacterInfo> characterInfoList, WantedConditionFlag conditionFlag, int conditionLevel)
	{
		_conditionFlag = conditionFlag;
		_conditionLevel = conditionLevel;
		_friendInfoCache = null;
		_memberInfoListCache = characterInfoList.Select((NetCharacterInfo characterInfo) => new WantedMemberInfo
		{
			CharacterInfo = characterInfo
		}).ToList();
		RefreshUnits(false);
	}

	private void OnConfirmMemberList(List<NetCharacterInfo> characterInfoList)
	{
		_memberInfoListCache = characterInfoList.Select((NetCharacterInfo characterInfo) => new WantedMemberInfo
		{
			CharacterInfo = characterInfo
		}).ToList();
		RefreshUnits();
	}

	private void OnConfirmFriend(WantedMemberInfo wantedMemberInfo)
	{
		_friendInfoCache = wantedMemberInfo;
		RefreshUnits();
	}

	private void RefreshConditionFlag()
	{
		List<NetCharacterInfo> list = _memberInfoListCache.Select((WantedMemberInfo memberInfo) => memberInfo.CharacterInfo).ToList();
		if (_friendInfoCache != null)
		{
			list.Add(_friendInfoCache.CharacterInfo);
		}
		WantedConditionFlag conditionFlag;
		int conditionLevel;
		Singleton<WantedSystem>.Instance.CheckConditionFlag(_wantedAttrData, list, out conditionFlag, out conditionLevel);
		_conditionFlag = conditionFlag;
		_conditionLevel = conditionLevel;
	}

	private void SendUpdateMemberListEvent()
	{
		Action<WantedConditionFlag, int> onMemberListUpdatedEvent = this.OnMemberListUpdatedEvent;
		if (onMemberListUpdatedEvent != null)
		{
			onMemberListUpdatedEvent(_conditionFlag, _conditionLevel);
		}
	}

	private void OnConfirmChargeStamina()
	{
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI<ChargeStaminaUI>("UI_ChargeStamina", OnChargeStaminaUILoaded);
	}

	private void OnMsgUIClosed()
	{
		_confirmUI = null;
	}
}
