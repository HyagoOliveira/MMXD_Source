#define RELEASE
using System.Collections.Generic;
using System.Linq;
using Coffee.UIExtensions;
using UnityEngine;
using UnityEngine.UI;
using enums;

public class PowerTowerUI : OrangeUIBase
{
	private enum TabIndex
	{
		None = 0,
		PowerPillar = 1,
		Ore = 2
	}

	[Header("LocalizationKeys")]
	[SerializeField]
	private string _keyTabPowerPillar;

	[SerializeField]
	private string _keyTabOreType;

	[Space]
	[SerializeField]
	private Transform _storageRoot;

	private StorageComponent _storage;

	private List<StorageInfo> _listStorage = new List<StorageInfo>();

	[SerializeField]
	private Text _textPowerTowerLevel;

	[SerializeField]
	private Text _textGuildMoney;

	[SerializeField]
	private GuildScoreInfoHelper _scoreInfoHelper;

	[SerializeField]
	private Button _buttonRankup;

	[SerializeField]
	private UIShiny _buttonRankupShiny;

	[SerializeField]
	private Text _textRankup;

	[SerializeField]
	private GameObject _goRankupDisable;

	[SerializeField]
	private GameObject _goRankupLock;

	[SerializeField]
	private GameObject _goRankupRedDot;

	[SerializeField]
	private PowerPillarChildUI _uiPowerPillar;

	[SerializeField]
	private OreListChildUI _uiOreList;

	private TabIndex _currentTabIndex;

	private bool _isInitializing = true;

	private bool _hasPowerTowerRankupPrivilege;

	public POWER_TABLE PowerTowerAttrData { get; private set; }

	private void OnEnable()
	{
		Singleton<GenericEventManager>.Instance.AttachEvent(EventManager.ID.BACK_TO_HOMETOP, OnBackToHometop);
		Singleton<PowerTowerSystem>.Instance.OnRankupEvent += OnRankupEvent;
		Singleton<PowerTowerSystem>.Instance.OnSocketPowerTowerRankupEvent += OnSocketPowerTowerRankupEvent;
		Singleton<PowerTowerSystem>.Instance.OnOpenPowerPillarEvent += OnOpenPowerPillarEvent;
		Singleton<PowerTowerSystem>.Instance.OnSocketPowerPillarChangedEvent += OnSocketPowerPillarChangedEvent;
		Singleton<PowerTowerSystem>.Instance.OnOreChangedEvent += OnOreChangedEvent;
		Singleton<PowerTowerSystem>.Instance.OnSocketOreChangedEvent += OnSocketOreChangedEvent;
		Singleton<GuildSystem>.Instance.OnSocketMemberPrivilegeChangedEvent += OnSocketMemberPrivilegeChangedEvent;
		Singleton<GuildSystem>.Instance.OnSocketHeaderPowerChangedEvent += OnSocketHeaderPowerChangedEvent;
	}

	private void OnDisable()
	{
		Singleton<GenericEventManager>.Instance.DetachEvent(EventManager.ID.BACK_TO_HOMETOP, OnBackToHometop);
		Singleton<PowerTowerSystem>.Instance.OnRankupEvent -= OnRankupEvent;
		Singleton<PowerTowerSystem>.Instance.OnSocketPowerTowerRankupEvent -= OnSocketPowerTowerRankupEvent;
		Singleton<PowerTowerSystem>.Instance.OnOpenPowerPillarEvent -= OnOpenPowerPillarEvent;
		Singleton<PowerTowerSystem>.Instance.OnSocketPowerPillarChangedEvent -= OnSocketPowerPillarChangedEvent;
		Singleton<PowerTowerSystem>.Instance.OnOreChangedEvent -= OnOreChangedEvent;
		Singleton<PowerTowerSystem>.Instance.OnSocketOreChangedEvent -= OnSocketOreChangedEvent;
		Singleton<GuildSystem>.Instance.OnSocketMemberPrivilegeChangedEvent -= OnSocketMemberPrivilegeChangedEvent;
		Singleton<GuildSystem>.Instance.OnSocketHeaderPowerChangedEvent -= OnSocketHeaderPowerChangedEvent;
	}

	public void Setup()
	{
		CheckPowerTowerRankupPrivilege();
		RefreshTowerInfo();
		RegenStorageTab(TabIndex.PowerPillar);
	}

	private void RefreshTowerInfo()
	{
		NetGuildInfo guildInfoCache = Singleton<GuildSystem>.Instance.GuildInfoCache;
		if (guildInfoCache == null)
		{
			return;
		}
		GuildSetting guildSetting = Singleton<GuildSystem>.Instance.GuildSetting;
		int powerTower = guildInfoCache.PowerTower;
		_textPowerTowerLevel.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("GUILD_POWER_LEVEL", powerTower);
		_textGuildMoney.text = guildInfoCache.Money.ToString("#,0");
		_scoreInfoHelper.Setup(guildInfoCache, guildSetting);
		POWER_TABLE value;
		if (!ManagedSingleton<OrangeDataManager>.Instance.POWER_TABLE_DICT.TryGetValue(powerTower, out value))
		{
			Debug.LogError("No AttrData");
			return;
		}
		PowerTowerAttrData = value;
		POWER_TABLE value2;
		if (!ManagedSingleton<OrangeDataManager>.Instance.POWER_TABLE_DICT.TryGetValue(powerTower + 1, out value2))
		{
			_buttonRankup.interactable = false;
			_buttonRankupShiny.enabled = false;
			_textRankup.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("GUILD_POWERTOWER_FULLLEVEL");
			_goRankupDisable.SetActive(true);
			_goRankupLock.SetActive(false);
			_goRankupRedDot.SetActive(false);
			return;
		}
		_buttonRankup.interactable = true;
		_textRankup.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("GUILD_SETUP_POWERTOWERUP");
		if (guildSetting.MaxPowerTowerLevel <= powerTower)
		{
			_buttonRankupShiny.enabled = false;
			_goRankupDisable.SetActive(true);
			_goRankupLock.SetActive(true);
			_goRankupRedDot.SetActive(false);
		}
		else
		{
			_buttonRankupShiny.enabled = true;
			_goRankupDisable.SetActive(false);
			_goRankupLock.SetActive(false);
			RefreshTowerRankupRedDot();
		}
	}

	private void RefreshTowerRankupRedDot()
	{
		NetGuildInfo guildInfoCache = Singleton<GuildSystem>.Instance.GuildInfoCache;
		GuildSetting guildSetting = Singleton<GuildSystem>.Instance.GuildSetting;
		int powerTower = guildInfoCache.PowerTower;
		_goRankupRedDot.SetActive(guildSetting.MaxPowerTowerLevel > powerTower && guildInfoCache.Score >= PowerTowerAttrData.n_POWER_INTEGRAL && guildInfoCache.Money >= PowerTowerAttrData.n_POWER_MONEY);
	}

	private void RefreshTabRedDot()
	{
		_storage.UpdateHint();
	}

	private void RegenStorageTab(TabIndex selectTabIndex)
	{
		if (_storage != null)
		{
			Object.Destroy(_storage.gameObject);
		}
		_listStorage.Clear();
		_listStorage.Add(new StorageInfo(_keyTabPowerPillar, false, 0, OnClickTab)
		{
			Param = new object[1] { TabIndex.PowerPillar }
		});
		_listStorage.Add(new StorageInfo(_keyTabOreType, false, 0, OnClickTab, TabOreListIsNewChecker)
		{
			Param = new object[1] { TabIndex.Ore }
		});
		int p_defaultIdx = _listStorage.FindIndex((StorageInfo storageInfo) => (TabIndex)storageInfo.Param[0] == selectTabIndex);
		StorageGenerator.Load("StorageComp00", _listStorage, p_defaultIdx, 0, _storageRoot, OnStorageLoaded);
	}

	private void OnStorageLoaded(GameObject goStorage)
	{
		_storage = goStorage.GetComponent<StorageComponent>();
	}

	private void OnClickTab(object p_param)
	{
		StorageInfo storageInfo = (StorageInfo)p_param;
		SelectTab((TabIndex)storageInfo.Param[0]);
	}

	private void SelectTab(TabIndex tabIndex)
	{
		Debug.Log(string.Format("[{0}] {1}", "SelectTab", tabIndex));
		if (_currentTabIndex == tabIndex)
		{
			return;
		}
		_currentTabIndex = tabIndex;
		switch (_currentTabIndex)
		{
		case TabIndex.PowerPillar:
			_uiOreList.CloseUI();
			if (!_isInitializing)
			{
				Singleton<PowerTowerSystem>.Instance.OnGetPowerPillarInfoOnceEvent += OnGetPowerPillarInfoEvent;
				Singleton<PowerTowerSystem>.Instance.ReqGetPowerPillarInfo();
			}
			else
			{
				SwitchToPowerPillarUI();
				_isInitializing = false;
			}
			break;
		case TabIndex.Ore:
			_uiPowerPillar.CloseUI();
			Singleton<PowerTowerSystem>.Instance.OnGetOreInfoOnceEvent += OnGetOreInfoEvent;
			Singleton<PowerTowerSystem>.Instance.ReqGetOreInfo();
			break;
		}
	}

	public void OnClickRankupBtn()
	{
		NetGuildInfo guildInfoCache = Singleton<GuildSystem>.Instance.GuildInfoCache;
		GuildSetting guildSetting = Singleton<GuildSystem>.Instance.GuildSetting;
		int powerTower = guildInfoCache.PowerTower;
		if (!_hasPowerTowerRankupPrivilege)
		{
			CommonUIHelper.ShowCommonTipUI("GUILD_POWERTOWER_ERROR");
		}
		else if (guildSetting.MaxPowerTowerLevel <= powerTower)
		{
			string guildRankString = Singleton<GuildSystem>.Instance.GetGuildRankString(guildInfoCache.Rank + 1);
			MonoBehaviourSingleton<OrangeGameManager>.Instance.ShowTipDialog(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("GUILD_POWERTOWER_WARN_1", guildRankString));
		}
		else
		{
			PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI<PowerTowerRankupConfirmUI>("UI_PowerTowerRankupConfirm", OnRankupConfirmUILoaded);
		}
	}

	private void OnRankupConfirmUILoaded(PowerTowerRankupConfirmUI ui)
	{
		NetGuildInfo guildInfoCache = Singleton<GuildSystem>.Instance.GuildInfoCache;
		ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
		ui.Setup(guildInfoCache.PowerTower, guildInfoCache.PowerTower + 1, MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("GUILD_POWERTOWER_LEVELUP", guildInfoCache.PowerTower + 1), guildInfoCache.Score, guildInfoCache.Money, PowerTowerAttrData.n_POWER_INTEGRAL, PowerTowerAttrData.n_POWER_MONEY, delegate
		{
			ui.CloseSE = SystemSE.NONE;
			OnConfirmUpgradeEvent();
		});
	}

	private void OnConfirmUpgradeEvent()
	{
		PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_STORE02);
		Singleton<PowerTowerSystem>.Instance.ReqRankup();
	}

	private void OnGetPowerPillarInfoEvent()
	{
		SwitchToPowerPillarUI();
	}

	private void OnGetOreInfoEvent()
	{
		SwitchToOreListUI();
	}

	private void OnRankupEvent(Code ackCode)
	{
		if (ackCode == Code.GUILD_GET_POWER_TOWER_RANK_UP_SUCCESS)
		{
			RefreshTowerInfo();
			RefreshTabRedDot();
		}
	}

	private void OnSocketPowerTowerRankupEvent()
	{
		RefreshTowerInfo();
		RefreshTabRedDot();
	}

	private void OnOpenPowerPillarEvent(Code ackCode)
	{
		if (ackCode == Code.GUILD_ORE_OPEN_SUCCESS)
		{
			RefreshTowerInfo();
		}
	}

	private void OnSocketPowerPillarChangedEvent()
	{
		RefreshTowerInfo();
	}

	private void OnOreChangedEvent()
	{
		RefreshTowerInfo();
		RefreshTabRedDot();
	}

	private void OnSocketOreChangedEvent()
	{
		RefreshTowerInfo();
		RefreshTabRedDot();
	}

	private void OnSocketMemberPrivilegeChangedEvent(bool isSelfPrivilegeChanged)
	{
		if (isSelfPrivilegeChanged)
		{
			CheckPowerTowerRankupPrivilege();
		}
	}

	private void OnSocketHeaderPowerChangedEvent()
	{
		CheckPowerTowerRankupPrivilege();
	}

	private void SwitchToPowerPillarUI()
	{
		_uiPowerPillar.OpenUI();
		_uiPowerPillar.Setup();
	}

	private void SwitchToOreListUI()
	{
		_uiOreList.OpenUI();
		_uiOreList.Setup();
	}

	private void CheckPowerTowerRankupPrivilege()
	{
		bool isLeader;
		bool isHeader;
		GuildHeaderPower headerPower;
		Singleton<GuildSystem>.Instance.CheckGuildPrivilege(out isLeader, out isHeader, out headerPower);
		_hasPowerTowerRankupPrivilege = isLeader || (isHeader && headerPower.HasFlag(GuildHeaderPower.TowerLevelup));
	}

	private bool TabOreListIsNewChecker(object[] param)
	{
		NetGuildInfo guildInfo = Singleton<GuildSystem>.Instance.GuildInfoCache;
		return Singleton<PowerTowerSystem>.Instance.OreInfoDataListCache.Any(delegate(OreInfoData infoData)
		{
			bool hasNextLevel;
			bool canLevelUp;
			infoData.CheckLevelUpState(guildInfo, out hasNextLevel, out canLevelUp);
			return hasNextLevel && canLevelUp;
		});
	}
}
