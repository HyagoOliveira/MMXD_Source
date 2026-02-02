using System;
using UnityEngine;
using UnityEngine.UI;

public class WantedMissionInfoHelper : OrangeChildUIBase
{
	[SerializeField]
	private Text _textTitle;

	[SerializeField]
	private Text _textDesc;

	[SerializeField]
	private GameObject _goDispatch;

	[SerializeField]
	private GameObject _goReceive;

	[SerializeField]
	private GameObject _goRefresh;

	[SerializeField]
	private GameObject _goComplete;

	[SerializeField]
	private GameObject _goReceived;

	private int _index;

	private CommonConsumeMsgUI _consumeMsgUI;

	private sbyte _slot
	{
		get
		{
			return (sbyte)(_index + 1);
		}
	}

	private int _completeCost
	{
		get
		{
			long serverUnixTimeNowUTC = MonoBehaviourSingleton<OrangeGameManager>.Instance.ServerUnixTimeNowUTC;
			return Mathf.CeilToInt((float)(CapUtility.UnixTimeToDate(Singleton<WantedSystem>.Instance.WantedTargetInfoCacheList[_index].WantedInfo.FinishTime) - CapUtility.UnixTimeToDate(serverUnixTimeNowUTC)).TotalSeconds / (float)OrangeConst.GUILD_WANTED_RATIO) * OrangeConst.GUILD_WANTED_FAST;
		}
	}

	public event Action<int> OnClickDispatchEvent;

	private void OnDestroy()
	{
		if (_consumeMsgUI != null)
		{
			_consumeMsgUI.OnClickCloseBtn();
			_consumeMsgUI = null;
		}
	}

	public void Setup(int index, WANTED_TABLE wantedAttrData, WantedTargetState targetState)
	{
		_index = index;
		_textTitle.text = ManagedSingleton<OrangeTextDataManager>.Instance.WANTEDTEXT_TABLE_DICT.GetL10nValue(wantedAttrData.w_NAME);
		_textDesc.text = ManagedSingleton<OrangeTextDataManager>.Instance.WANTEDTEXT_TABLE_DICT.GetL10nValue(wantedAttrData.w_TIP);
		_goDispatch.SetActive(false);
		_goReceive.SetActive(false);
		_goReceived.SetActive(false);
		_goRefresh.SetActive(false);
		_goComplete.SetActive(false);
		switch (targetState)
		{
		case WantedTargetState.Normal:
			_goDispatch.SetActive(true);
			_goRefresh.SetActive(true);
			break;
		case WantedTargetState.Started:
			_goComplete.SetActive(true);
			break;
		case WantedTargetState.Finished:
			_goReceive.SetActive(true);
			break;
		case WantedTargetState.Received:
			_goReceived.SetActive(true);
			break;
		}
	}

	public void OnClickDispatchButton()
	{
		Action<int> onClickDispatchEvent = this.OnClickDispatchEvent;
		if (onClickDispatchEvent != null)
		{
			onClickDispatchEvent(_index);
		}
	}

	public void OnClickReceiveButton()
	{
		Singleton<WantedSystem>.Instance.ReqReceiveWanted(_slot);
	}

	public void OnClickCompleteButton()
	{
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI<CommonConsumeMsgUI>("UI_CommonConsumeMsg", OnCompleteConfirmUILoaded);
	}

	private void OnCompleteConfirmUILoaded(CommonConsumeMsgUI ui)
	{
		int completeCost = _completeCost;
		ui.YesSE = SystemSE.CRI_SYSTEMSE_SYS_STORE01;
		ui.Setup(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_TIP"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("GUILD_WANTED_WARN_2", completeCost), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_OK"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_CANCEL"), ManagedSingleton<PlayerHelper>.Instance.GetTotalJewel(), completeCost, OnConfirmComplete, OnCloseConsumeUI);
		_consumeMsgUI = ui;
	}

	private void OnConfirmComplete()
	{
		Singleton<WantedSystem>.Instance.ReqReceiveWanted(_slot, true);
		OnCloseConsumeUI();
	}

	public void OnClickRefreshButton()
	{
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI<CommonConsumeMsgUI>("UI_CommonConsumeMsg", OnRefreshConfirmUILoaded);
	}

	private void OnRefreshConfirmUILoaded(CommonConsumeMsgUI ui)
	{
		int gUILD_WANTED_RESET = OrangeConst.GUILD_WANTED_RESET;
		ui.YesSE = SystemSE.CRI_SYSTEMSE_SYS_STORE01;
		ui.Setup(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_TIP"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("GUILD_WANTED_WARN_1", gUILD_WANTED_RESET), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_OK"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_CANCEL"), ManagedSingleton<PlayerHelper>.Instance.GetTotalJewel(), gUILD_WANTED_RESET, OnConfirmRefresh, OnCloseConsumeUI);
		_consumeMsgUI = ui;
	}

	private void OnConfirmRefresh()
	{
		WantedMainUI uI = MonoBehaviourSingleton<UIManager>.Instance.GetUI<WantedMainUI>("UI_WantedMain");
		if (uI != null)
		{
			uI.bFirstMute = true;
		}
		Singleton<WantedSystem>.Instance.ReqRefreshWantedSlot(_slot);
		OnCloseConsumeUI();
	}

	private void OnCloseConsumeUI()
	{
		_consumeMsgUI = null;
	}
}
