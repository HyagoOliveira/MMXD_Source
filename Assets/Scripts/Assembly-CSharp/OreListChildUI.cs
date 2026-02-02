#define RELEASE
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using enums;

public class OreListChildUI : OrangeChildUIBase
{
	private const int SCROLL_VISUAL_COUNT = 5;

	[SerializeField]
	private LoopVerticalScrollRect _scrollRect;

	[SerializeField]
	private OreCell _scrollCell;

	private bool _hasOreLevelUpPrivilege;

	private OreInfoData _oreInfoDataCache;

	public List<OreInfoData> OreInfoDataList { get; private set; }

	private void OnEnable()
	{
		Singleton<PowerTowerSystem>.Instance.OnOreChangedEvent += OnOreChangedEvent;
		Singleton<PowerTowerSystem>.Instance.OnSocketOreChangedEvent += OnSocketOreChangedEvent;
		Singleton<PowerTowerSystem>.Instance.OnSocketPowerTowerRankupEvent += OnSocketPowerTowerRankupEvent;
		Singleton<GuildSystem>.Instance.OnSocketMemberPrivilegeChangedEvent += OnSocketMemberPrivilegeChangedEvent;
		Singleton<GuildSystem>.Instance.OnSocketHeaderPowerChangedEvent += OnSocketHeaderPowerChangedEvent;
	}

	private void OnDisable()
	{
		Singleton<PowerTowerSystem>.Instance.OnOreChangedEvent -= OnOreChangedEvent;
		Singleton<PowerTowerSystem>.Instance.OnSocketOreChangedEvent -= OnSocketOreChangedEvent;
		Singleton<PowerTowerSystem>.Instance.OnSocketPowerTowerRankupEvent -= OnSocketPowerTowerRankupEvent;
		Singleton<GuildSystem>.Instance.OnSocketMemberPrivilegeChangedEvent -= OnSocketMemberPrivilegeChangedEvent;
		Singleton<GuildSystem>.Instance.OnSocketHeaderPowerChangedEvent -= OnSocketHeaderPowerChangedEvent;
	}

	public override void Setup()
	{
		CheckOreLevelUpPrivilege();
		RefreshOreList();
	}

	public void OnClickOneLevelUpBtn(OreInfoData oreInfoData)
	{
		if (_hasOreLevelUpPrivilege)
		{
			_oreInfoDataCache = oreInfoData;
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI<OreLevelUpConfirmUI>("UI_OreLevelUpConfirm", OnLevelUpConfirmUILoaded);
		}
		else
		{
			CommonUIHelper.ShowCommonTipUI("GUILD_ORE_ERROR");
		}
	}

	private void OnLevelUpConfirmUILoaded(OreLevelUpConfirmUI ui)
	{
		ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
		ui.Setup(_oreInfoDataCache.ItemID, MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("GUILD_ORE_WARN_1"), _oreInfoDataCache.LevelUpMoney);
		ui.OnConfirmEvent += OnConfirmLevelUpEvent;
	}

	private void OnConfirmLevelUpEvent()
	{
		Singleton<PowerTowerSystem>.Instance.ReqOreLevelUp(_oreInfoDataCache.Group, _oreInfoDataCache.Level);
	}

	private void OnOreChangedEvent()
	{
		Debug.Log("[OnOreChangedEvent]");
		RefreshOreList();
	}

	private void OnSocketOreChangedEvent()
	{
		Debug.Log("[OnSocketOreChangedEvent]");
		RefreshOreList();
	}

	private void OnSocketPowerTowerRankupEvent()
	{
		Debug.Log("[OnSocketPowerTowerRankupEvent]");
		RefreshOreList();
	}

	private void RefreshOreList()
	{
		OreInfoDataList = Singleton<PowerTowerSystem>.Instance.OreInfoDataListCache.ToList();
		_scrollRect.OrangeInit(_scrollCell, 5, OreInfoDataList.Count());
	}

	private void OnSocketMemberPrivilegeChangedEvent(bool isSelfPrivilegeChanged)
	{
		if (isSelfPrivilegeChanged)
		{
			CheckOreLevelUpPrivilege();
		}
	}

	private void OnSocketHeaderPowerChangedEvent()
	{
		CheckOreLevelUpPrivilege();
	}

	private void CheckOreLevelUpPrivilege()
	{
		bool isLeader;
		bool isHeader;
		GuildHeaderPower headerPower;
		Singleton<GuildSystem>.Instance.CheckGuildPrivilege(out isLeader, out isHeader, out headerPower);
		_hasOreLevelUpPrivilege = isLeader || (isHeader && headerPower.HasFlag(GuildHeaderPower.OreLevelup));
	}
}
