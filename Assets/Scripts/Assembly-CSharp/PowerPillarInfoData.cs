using System;
using JsonFx.Json;

public class PowerPillarInfoData : IEquatable<PowerPillarInfoData>
{
	public int PillarID { get; private set; }

	public int OreID { get; private set; }

	[JsonIgnore]
	public OreInfoData OreInfo { get; private set; }

	public DateTime ExpireTime { get; private set; }

	public PowerPillarInfoData()
	{
	}

	public PowerPillarInfoData(DateTime datumTime, NetPowerPillarInfo pillarInfo)
	{
		PillarID = pillarInfo.PillarID;
		OreID = pillarInfo.OreID;
		ExpireTime = ((pillarInfo.OreID > 0) ? datumTime.AddSeconds(pillarInfo.OpenTime) : DateTime.MaxValue);
		RefreshOreInfo();
	}

	public void RefreshOreInfo()
	{
		ORE_TABLE value;
		OreInfo = ((OreID > 0 && ManagedSingleton<OrangeDataManager>.Instance.ORE_TABLE_DICT.TryGetValue(OreID, out value)) ? new OreInfoData(value) : null);
	}

	public bool Equals(PowerPillarInfoData other)
	{
		if (PillarID == other.PillarID)
		{
			return OreID == other.OreID;
		}
		return false;
	}
}
