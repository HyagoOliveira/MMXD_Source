#define RELEASE
public class PowerTowerSetting
{
	public int PowerTowerLevel { get; private set; }

	public int MaxPowerPillarCount { get; private set; }

	public PowerTowerSetting(POWER_TABLE data)
	{
		PowerTowerLevel = data.n_ID;
		MaxPowerPillarCount = data.n_POWER_LIMIT;
	}

	public static bool TryGetSettingByRank(int rank, out PowerTowerSetting setting)
	{
		POWER_TABLE value;
		if (!ManagedSingleton<OrangeDataManager>.Instance.POWER_TABLE_DICT.TryGetValue(rank, out value))
		{
			Debug.LogWarning(string.Format("Invalid PowerTower Rank {0}", rank));
			setting = null;
			return false;
		}
		setting = new PowerTowerSetting(value);
		return true;
	}
}
