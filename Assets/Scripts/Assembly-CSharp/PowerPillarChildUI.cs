#define RELEASE
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using enums;

public class PowerPillarChildUI : OrangeChildUIBase
{
	private const int SCROLL_VISUAL_COUNT = 5;

	[SerializeField]
	private Text _textEnabledCount;

	[SerializeField]
	private LoopHorizontalScrollRect _scrollRect;

	[SerializeField]
	private PowerPillarCell _scrollCell;

	private int _pillarIdCache;

	private OreInfoData _oreInfoDataCache;

	private Coroutine _coroutineCellTime;

	private DateTime _refreshTime;

	private bool _hasPowerPillarPrivilege;

	private CommonUI _commonUI;

	public List<PowerPillarInfoData> PowerPillarInfoDataList { get; private set; }

	private void OnEnable()
	{
		Singleton<PowerTowerSystem>.Instance.OnGetOreInfoEvent += OnGetOreInfoEvent;
		Singleton<PowerTowerSystem>.Instance.OnOpenPowerPillarEvent += OnOpenPowerPillarEvent;
		Singleton<PowerTowerSystem>.Instance.OnClosePowerPillarEvent += OnClosePowerPillarEvent;
		Singleton<PowerTowerSystem>.Instance.OnPowerPillarChangedEvent += OnPowerPillarChangedEvent;
		Singleton<PowerTowerSystem>.Instance.OnSocketPowerTowerRankupEvent += OnSocketPowerTowerRankupEvent;
		Singleton<PowerTowerSystem>.Instance.OnSocketPowerPillarChangedEvent += OnSocketPowerPillarChangedEvent;
		Singleton<GuildSystem>.Instance.OnSocketMemberPrivilegeChangedEvent += OnSocketMemberPrivilegeChangedEvent;
		Singleton<GuildSystem>.Instance.OnSocketHeaderPowerChangedEvent += OnSocketHeaderPowerChangedEvent;
	}

	private void OnDisable()
	{
		CommonUI commonUI = _commonUI;
		if ((object)commonUI != null)
		{
			commonUI.OnClickCloseBtn();
		}
		Singleton<PowerTowerSystem>.Instance.OnGetOreInfoEvent -= OnGetOreInfoEvent;
		Singleton<PowerTowerSystem>.Instance.OnOpenPowerPillarEvent -= OnOpenPowerPillarEvent;
		Singleton<PowerTowerSystem>.Instance.OnClosePowerPillarEvent -= OnClosePowerPillarEvent;
		Singleton<PowerTowerSystem>.Instance.OnPowerPillarChangedEvent -= OnPowerPillarChangedEvent;
		Singleton<PowerTowerSystem>.Instance.OnSocketPowerTowerRankupEvent -= OnSocketPowerTowerRankupEvent;
		Singleton<PowerTowerSystem>.Instance.OnSocketPowerPillarChangedEvent -= OnSocketPowerPillarChangedEvent;
		Singleton<GuildSystem>.Instance.OnSocketMemberPrivilegeChangedEvent -= OnSocketMemberPrivilegeChangedEvent;
		Singleton<GuildSystem>.Instance.OnSocketHeaderPowerChangedEvent -= OnSocketHeaderPowerChangedEvent;
		if (_coroutineCellTime != null)
		{
			StopCoroutine(_coroutineCellTime);
			_coroutineCellTime = null;
		}
	}

	public override void Setup()
	{
		RefreshPowerPillarList(true);
	}

	public void OnClickOneOpenPillarBtn(int pillarId)
	{
		if (_hasPowerPillarPrivilege)
		{
			_pillarIdCache = pillarId;
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
			Singleton<PowerTowerSystem>.Instance.ReqGetOreInfo();
		}
		else
		{
			CommonUIHelper.ShowCommonTipUI("GUILD_POWERPILLAR_ERROR");
		}
	}

	public void OnClickOneClosePillarBtn(OreInfoData oreInfoData)
	{
		if (_hasPowerPillarPrivilege)
		{
			_oreInfoDataCache = oreInfoData;
			MonoBehaviourSingleton<UIManager>.Instance.LoadResourceUI<CommonUI>("UI_CommonMsg", OnConfirmDisableUILoaded);
		}
		else
		{
			CommonUIHelper.ShowCommonTipUI("GUILD_POWERPILLAR_ERROR");
		}
	}

	private void OnConfirmDisableUILoaded(CommonUI ui)
	{
		ui.YesSE = SystemSE.CRI_SYSTEMSE_SYS_OK17;
		ui.SetupYesNO(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("GUILD_POWERPILLAR_SHUT"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("GUILD_POWERPILLAR_WARN_4") + "\n\n" + MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("GUILD_POWERPILLAR_WARN_5"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_OK"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_CANCEL"), OnConfirmClosePillarEvent, OnCommonUIClosed);
		_commonUI = ui;
	}

	private void OnConfirmClosePillarEvent()
	{
		if (_hasPowerPillarPrivilege)
		{
			Singleton<PowerTowerSystem>.Instance.ReqClosePowerPillar(_oreInfoDataCache.ID);
		}
		else
		{
			CommonUIHelper.ShowCommonTipUI("GUILD_POWERPILLAR_ERROR");
		}
	}

	private void OnCommonUIClosed()
	{
		_commonUI = null;
	}

	private void OnGetOreInfoEvent(Code ackCode)
	{
		if (ackCode == Code.GUILD_GET_ORE_INFO_SUCCESS)
		{
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI<OreSelectionUI>("UI_OreSelection", OnOreSelectionUILoaded);
		}
	}

	private void OnOreSelectionUILoaded(OreSelectionUI ui)
	{
		ui.Setup(_pillarIdCache);
	}

	private void OnOpenPowerPillarEvent(Code ackCode)
	{
		Debug.Log("[OnOpenPowerPillarEvent]");
		switch (ackCode)
		{
		default:
		{
			int num = 105704;
			break;
		}
		case Code.GUILD_ORE_OPENING_ERROR:
			CommonUIHelper.ShowCommonTipUI("GUILD_ORE_TURNON");
			break;
		case Code.GUILD_PILLAR_USED_ERROR:
			CommonUIHelper.ShowCommonTipUI("GUILD_POWERPILLAR_TURNON");
			break;
		}
	}

	private void OnClosePowerPillarEvent(Code ackCode)
	{
		Debug.Log("[OnClosePowerPillarEvent]");
		if ((uint)(ackCode - 105557) > 1u)
		{
			int num = 105705;
		}
		else
		{
			CommonUIHelper.ShowCommonTipUI("GUILD_POWERPILLAR_TURNOFF");
		}
	}

	private void OnPowerPillarChangedEvent()
	{
		Debug.Log("[OnPowerPillarChangedEvent]");
		RefreshPowerPillarList();
	}

	private void OnSocketPowerTowerRankupEvent()
	{
		Debug.Log("[OnSocketPowerTowerRankupEvent]");
		RefreshPowerPillarList();
	}

	private void OnSocketPowerPillarChangedEvent()
	{
		Debug.Log("[OnSocketPowerPillarChangedEvent]");
		RefreshPowerPillarList();
	}

	private void RefreshPowerPillarList(bool isInitializing = false)
	{
		if (_coroutineCellTime != null)
		{
			StopCoroutine(_coroutineCellTime);
			_coroutineCellTime = null;
		}
		CheckPowerPillarPrivilege();
		PowerPillarInfoDataList = Singleton<PowerTowerSystem>.Instance.PowerPillarInfoDataListCache.ToList();
		int count = PowerPillarInfoDataList.Count;
		List<PowerPillarInfoData> list = PowerPillarInfoDataList.Where((PowerPillarInfoData info) => info.OreInfo != null).ToList();
		int count2 = list.Count;
		_refreshTime = ((count2 > 0) ? list.Min((PowerPillarInfoData pillarInfo) => pillarInfo.ExpireTime) : DateTime.MinValue);
		_textEnabledCount.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("GUILD_POWERPILLAR_OPENING", count2, count);
		int p_totalCount = Singleton<PowerTowerSystem>.Instance.PowerTowerRankupSettings.Max((POWER_TABLE attrData) => attrData.n_POWER_LIMIT);
		if (isInitializing)
		{
			_scrollRect.OrangeInit(_scrollCell, 5, p_totalCount);
		}
		else
		{
			_scrollRect.RefreshCellsNew();
		}
		if (count2 > 0)
		{
			_coroutineCellTime = StartCoroutine(RefreshCellTimeCoroutine());
		}
	}

	private IEnumerator RefreshCellTimeCoroutine()
	{
		while (true)
		{
			_scrollRect.RefreshCellsNew();
			if (MonoBehaviourSingleton<OrangeGameManager>.Instance.ServerTimeNowUTC >= _refreshTime)
			{
				break;
			}
			yield return CoroutineDefine._1sec;
		}
		Singleton<PowerTowerSystem>.Instance.ReqGetPowerPillarInfo();
	}

	private void OnSocketMemberPrivilegeChangedEvent(bool isSelfPrivilegeChanged)
	{
		Debug.Log("[OnSocketMemberPrivilegeChangedEvent]");
		if (isSelfPrivilegeChanged)
		{
			CheckPowerPillarPrivilege();
		}
	}

	private void OnSocketHeaderPowerChangedEvent()
	{
		Debug.Log("[OnSocketHeaderPowerChangedEvent]");
		CheckPowerPillarPrivilege();
	}

	private void CheckPowerPillarPrivilege()
	{
		bool isLeader;
		bool isHeader;
		GuildHeaderPower headerPower;
		Singleton<GuildSystem>.Instance.CheckGuildPrivilege(out isLeader, out isHeader, out headerPower);
		_hasPowerPillarPrivilege = isLeader || (isHeader && headerPower.HasFlag(GuildHeaderPower.TowerSwitch));
	}
}
