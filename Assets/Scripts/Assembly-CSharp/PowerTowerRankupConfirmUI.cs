#define RELEASE
using System;
using System.Collections.Generic;

public class PowerTowerRankupConfirmUI : GuildUpgradeConfirmUIBase
{
	protected override void OnEnable()
	{
		base.OnEnable();
		Singleton<PowerTowerSystem>.Instance.OnSocketPowerTowerRankupEvent += OnSocketPowerTowerRankupEvent;
	}

	protected override void OnDisable()
	{
		Singleton<PowerTowerSystem>.Instance.OnSocketPowerTowerRankupEvent -= OnSocketPowerTowerRankupEvent;
		base.OnDisable();
	}

	public void Setup(int rankBefore, int rankAfter, string title, int ownScore, int ownMoney, int requireScore, int requireMoney, Action onYesCB, Action onNoCB = null)
	{
		PowerTowerSetting setting;
		if (!PowerTowerSetting.TryGetSettingByRank(rankBefore, out setting))
		{
			Debug.LogError(string.Format("Invalid PowerTowerRank : {0} of {1}", rankBefore, "PowerTowerSetting"));
			return;
		}
		PowerTowerSetting setting2;
		if (!PowerTowerSetting.TryGetSettingByRank(rankAfter, out setting2))
		{
			Debug.LogError(string.Format("Invalid PowerTowerRank : {0} of {1}", rankAfter, "PowerTowerSetting"));
			return;
		}
		List<UpgradeInfo> list = new List<UpgradeInfo>();
		if (setting.MaxPowerPillarCount != setting2.MaxPowerPillarCount)
		{
			list.Add(new UpgradeInfo(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("GUILD_POWERTOWER_MAX"), setting.MaxPowerPillarCount, setting2.MaxPowerPillarCount));
		}
		Setup(title, ownScore, ownMoney, requireScore, requireMoney, list, onYesCB, onNoCB);
	}

	private void OnSocketPowerTowerRankupEvent()
	{
		OnClickCloseBtn();
	}
}
