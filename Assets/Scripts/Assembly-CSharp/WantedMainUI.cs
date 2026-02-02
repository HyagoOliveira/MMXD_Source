using System;
using System.Collections.Generic;
using CallbackDefs;
using UnityEngine;
using enums;

public class WantedMainUI : OrangeUIBase
{
	[SerializeField]
	private WantedRateInfoHelper _rateInfoHelper;

	[SerializeField]
	private WantedRewardInfoHelper _rewardInfoHelper;

	[SerializeField]
	private WantedMissionBackgroundHelper _missionBackgroundHelper;

	[SerializeField]
	private WantedMissionInfoHelper _missionInfoHelper;

	[SerializeField]
	private WantedMissionDetailHelper _missionDetailHelper;

	[SerializeField]
	private WantedTargetSelectHelper _targetSelectHelper;

	[SerializeField]
	private WantedMemberSelectHelper _memberSelectHelper;

	private NetWantedInfo _wantedInfo;

	private WANTED_TABLE _wantedAttrData;

	private WANTED_SUCCESS_TABLE _successAttrData;

	private List<WantedMemberInfo> _wantedMemberInfoCache = new List<WantedMemberInfo>();

	public bool bFirstMute = true;

	private int _lastSelectedIndex = -1;

	private void OnEnable()
	{
		Singleton<GenericEventManager>.Instance.AttachEvent(EventManager.ID.BACK_TO_HOMETOP, OnBackToHometop);
		Singleton<WantedSystem>.Instance.OnWantedStartEvent += OnWantedStartEvent;
		Singleton<WantedSystem>.Instance.OnReceiveWantedEvent += OnReceiveWantedEvent;
		Singleton<WantedSystem>.Instance.OnRefreshWantedSlotEvent += OnRefreshWantedSlotEvent;
	}

	private void OnDisable()
	{
		Singleton<GenericEventManager>.Instance.DetachEvent(EventManager.ID.BACK_TO_HOMETOP, OnBackToHometop);
		Singleton<WantedSystem>.Instance.OnWantedStartEvent -= OnWantedStartEvent;
		Singleton<WantedSystem>.Instance.OnReceiveWantedEvent -= OnReceiveWantedEvent;
		Singleton<WantedSystem>.Instance.OnRefreshWantedSlotEvent -= OnRefreshWantedSlotEvent;
	}

	public void Setup()
	{
		_missionInfoHelper.OnClickDispatchEvent += OnClickMissionDispatchEvent;
		_missionDetailHelper.OnClickCloseEvent += OnCloseMissionDetailEvent;
		_targetSelectHelper.OnTargetClickedEvent += OnWantedTargetClickedEvent;
		_memberSelectHelper.OnMemberListUpdatedEvent += OnMemberListUpdatedEvent;
		_rateInfoHelper.OpenUI();
		_rewardInfoHelper.OpenUI();
		_missionInfoHelper.OpenUI();
		_missionDetailHelper.CloseUI();
		_targetSelectHelper.OpenUI();
		_memberSelectHelper.CloseUI();
		RefreshTargetUI();
		Singleton<WantedSystem>.Instance.RefreshSortedCharacterList();
	}

	private void OnClickMissionDispatchEvent(int index)
	{
		PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_OK17);
		_missionDetailHelper.Setup(_wantedAttrData);
		_memberSelectHelper.Setup(index, _wantedAttrData);
		_missionInfoHelper.CloseUI();
		_missionDetailHelper.OpenUI();
		_targetSelectHelper.CloseUI();
		_memberSelectHelper.OpenUI();
	}

	private void OnCloseMissionDetailEvent()
	{
		_missionInfoHelper.OpenUI();
		_missionDetailHelper.CloseUI();
		_targetSelectHelper.OpenUI();
		_memberSelectHelper.CloseUI();
		_targetSelectHelper.CheckTargetUnitsCoroutine();
		_rateInfoHelper.StartRandomRate();
	}

	private void OnWantedTargetClickedEvent(int index, WantedTargetState targetState)
	{
		OnSelectWantedTarget(index, targetState);
	}

	private void OnMemberListUpdatedEvent(WantedConditionFlag conditionFlag, int conditionLevel)
	{
		_rateInfoHelper.Setup(_successAttrData, conditionFlag, conditionLevel);
		_missionDetailHelper.SetConditionFlag(conditionFlag);
	}

	private void OnSelectWantedTarget(int index, WantedTargetState targetState)
	{
		if (bFirstMute)
		{
			bFirstMute = false;
		}
		else if (_lastSelectedIndex != index)
		{
			PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_OK01);
		}
		_lastSelectedIndex = index;
		WantedTargetInfo wantedTargetInfo = Singleton<WantedSystem>.Instance.WantedTargetInfoCacheList[index];
		_wantedInfo = wantedTargetInfo.WantedInfo;
		_wantedAttrData = wantedTargetInfo.WantedAttrData;
		_successAttrData = wantedTargetInfo.SuccessAttrData;
		_targetSelectHelper.SelectTarget(index);
		_missionBackgroundHelper.Setup(_wantedAttrData);
		_missionInfoHelper.Setup(index, _wantedAttrData, targetState);
		_rewardInfoHelper.Setup(_wantedAttrData, targetState == WantedTargetState.Received);
		_rateInfoHelper.StartRandomRate();
	}

	private void OnWantedStartEvent(Code ackCode)
	{
		if (ackCode == Code.WANTED_START_SUCCESS)
		{
			_missionInfoHelper.OpenUI();
			_missionDetailHelper.CloseUI();
			_targetSelectHelper.OpenUI();
			RefreshTargetUI();
			_rateInfoHelper.StartRandomRate();
		}
	}

	private void OnReceiveWantedEvent(Code ackCode, List<NetRewardInfo> rewardInfoList, WantedSuccessType successType)
	{
		if (ackCode == Code.WANTED_RECEIVE_SUCCESS)
		{
			ShowRewardPopup(rewardInfoList, successType);
			RefreshTargetUI();
		}
	}

	private void OnRefreshWantedSlotEvent(Code ackCode)
	{
		if (ackCode == Code.WANTED_REFRESH_SLOT_SUCCESS)
		{
			RefreshTargetUI();
		}
	}

	private void ShowRewardPopup(List<NetRewardInfo> rewardInfoList, WantedSuccessType successType)
	{
		if (rewardInfoList != null && rewardInfoList.Count > 0)
		{
			string title = string.Empty;
			NewRewardPopupUI.PopupTheme popupTheme = NewRewardPopupUI.PopupTheme.GoldenWithGlow;
			switch (successType)
			{
			case WantedSuccessType.NormalSuccess:
				title = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("GUILD_WANTED_RATECOMMON");
				popupTheme = NewRewardPopupUI.PopupTheme.Blue;
				break;
			case WantedSuccessType.BigSuccess:
				title = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("GUILD_WANTED_RATEGOOD");
				popupTheme = NewRewardPopupUI.PopupTheme.Purple;
				break;
			case WantedSuccessType.HugeSuccess:
				title = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("GUILD_WANTED_RATESUPER");
				break;
			}
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_NewRewardPopup", delegate(NewRewardPopupUI ui)
			{
				ui.Setup(rewardInfoList, 0f, popupTheme);
				ui.ChangeTitle(title);
				ui.closeCB = (Callback)Delegate.Combine(ui.closeCB, new Callback(RefreshTargetUI));
			});
		}
	}

	private void RefreshTargetUI()
	{
		_targetSelectHelper.Setup();
	}
}
