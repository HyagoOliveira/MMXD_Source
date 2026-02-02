using System.Collections.Generic;
using System.Linq;
using enums;

public class ServiceHelper : ManagedSingleton<ServiceHelper>
{
	private MultiMap<int, SERVICE_TABLE> mmapPresetByServiceGroup = new MultiMap<int, SERVICE_TABLE>();

	private SERVICE_TABLE currentService;

	public SERVICE_TABLE CurrentTable
	{
		get
		{
			return currentService;
		}
	}

	public override void Initialize()
	{
		mmapPresetByServiceGroup.Clear();
		foreach (SERVICE_TABLE value in ManagedSingleton<OrangeDataManager>.Instance.SERVICE_TABLE_DICT.Values)
		{
			mmapPresetByServiceGroup.Add(value.n_GROUP, value);
		}
	}

	public override void Reset()
	{
		base.Reset();
		Initialize();
	}

	public override void Dispose()
	{
	}

	public List<int> GetServiceGroupList()
	{
		return mmapPresetByServiceGroup.Keys.ToList();
	}

	public bool HasBoughtServiceGroup(int groupID)
	{
		List<int> list = new List<int>();
		foreach (int key in ManagedSingleton<PlayerNetManager>.Instance.dicService.Keys)
		{
			if (ManagedSingleton<OrangeDataManager>.Instance.SERVICE_TABLE_DICT.ContainsKey(key))
			{
				list.Add(ManagedSingleton<OrangeDataManager>.Instance.SERVICE_TABLE_DICT[key].n_GROUP);
			}
		}
		list = list.Distinct().ToList();
		return list.Any((int x) => x == groupID);
	}

	public int GetServiceBonusValue(ServiceType serviceType, StageType stageType = StageType.None, int stageMain = 0, BonusType bonusType = BonusType.BONUS_NONE)
	{
		SERVICE_TABLE value = null;
		currentService = null;
		int num = 0;
		foreach (KeyValuePair<int, ServiceInfo> item in ManagedSingleton<PlayerNetManager>.Instance.dicService)
		{
			if (item.Value.netServiceInfo.ExpireTime < MonoBehaviourSingleton<OrangeGameManager>.Instance.ServerUnixTimeNowUTC || !ManagedSingleton<OrangeDataManager>.Instance.SERVICE_TABLE_DICT.TryGetValue(item.Key, out value) || (int)(short)value.n_TYPE != (int)serviceType)
			{
				continue;
			}
			switch (serviceType)
			{
			case ServiceType.ResearchSpeedup:
				num += value.n_TYPE_1;
				if (num > 100)
				{
					num = 100;
				}
				break;
			case ServiceType.ResearchAPIncrease:
				num += value.n_TYPE_1 - 100;
				break;
			case ServiceType.FriendApCountIncrease:
				num += value.n_TYPE_1;
				break;
			case ServiceType.StageBonus:
				if ((int)stageType == (short)value.n_TYPE_1 && (stageMain == value.n_TYPE_2 || value.n_TYPE_2 == 0) && (int)bonusType == (short)value.n_TYPE_3)
				{
					num = ((bonusType != BonusType.BONUS_APREDUCE) ? (num + (value.n_TYPE_4 - 100)) : (num + value.n_TYPE_4));
				}
				break;
			}
			currentService = value;
		}
		return num;
	}

	public bool GetServiceRemainTime(int p_serviceId, out string p_remainTimeText)
	{
		p_remainTimeText = string.Empty;
		ServiceInfo value = null;
		if (!ManagedSingleton<PlayerNetManager>.Instance.dicService.TryGetValue(p_serviceId, out value))
		{
			return false;
		}
		if (value.netServiceInfo.ExpireTime < MonoBehaviourSingleton<OrangeGameManager>.Instance.ServerUnixTimeNowUTC)
		{
			return false;
		}
		p_remainTimeText = OrangeGameUtility.GetRemainTimeText(value.netServiceInfo.ExpireTime);
		return true;
	}
}
