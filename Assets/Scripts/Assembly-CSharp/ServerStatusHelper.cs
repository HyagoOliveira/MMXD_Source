#define RELEASE
using System.Collections.Generic;
using CallbackDefs;
using OrangeApi;

public class ServerStatusHelper : ManagedSingleton<ServerStatusHelper>
{
	private HashSet<int> m_offlineServerIDHash = new HashSet<int>();

	private HashSet<int> m_overloadServerIDHash = new HashSet<int>();

	private RetrieveServerStatusRes m_serverStatusRes;

	private NetControlSetting m_controlInfo;

	private bool m_bUpdated;

	public bool IsSendDeviceInfo
	{
		get
		{
			if (m_controlInfo != null && m_controlInfo.RequireDeviceInfo > 0)
			{
				return true;
			}
			return false;
		}
	}

	public bool EnableLockStep
	{
		get
		{
			if (m_controlInfo != null && m_controlInfo.LockStepSync > 0)
			{
				return true;
			}
			return false;
		}
	}

	public bool FixBulletCollider
	{
		get
		{
			if (m_controlInfo != null)
			{
				return m_controlInfo.FixBulletCollider > 0;
			}
			return false;
		}
	}

	public override void Initialize()
	{
	}

	public override void Dispose()
	{
	}

	public bool IsUpdated()
	{
		return m_bUpdated;
	}

	public void UpdateServerStatus(Callback p_cb = null)
	{
		m_offlineServerIDHash.Clear();
		m_overloadServerIDHash.Clear();
		RetrieveServerStatusReq req = new RetrieveServerStatusReq
		{
			DeviceType = (sbyte)MonoBehaviourSingleton<OrangeGameManager>.Instance.GetDeviceType()
		};
		MonoBehaviourSingleton<GlobalServerService>.Instance.SendRequest(req, delegate(RetrieveServerStatusRes res)
		{
			m_serverStatusRes = res;
			m_controlInfo = res.ControlInfo;
			MonoBehaviourSingleton<CBSocketClient>.Instance.SetSyncNetworkFrequency(m_controlInfo.CollectSyncTime);
			MonoBehaviourSingleton<CBSocketClient>.Instance.SetAsyncNetworkFrequency(m_controlInfo.CollectAsyncTime);
			if (m_serverStatusRes != null)
			{
				foreach (short item in m_serverStatusRes.MaintenanceInfo.ZoneID)
				{
					m_offlineServerIDHash.Add(item);
				}
				foreach (short item2 in m_serverStatusRes.OverloadInfo.ZoneID)
				{
					m_overloadServerIDHash.Add(item2);
				}
			}
			m_bUpdated = true;
			p_cb.CheckTargetToInvoke();
		});
	}

	public bool IsWhitelistedUser()
	{
		string publicIPAddress = MonoBehaviourSingleton<LocateManager>.Instance.GetPublicIPAddress();
		foreach (string item in m_serverStatusRes.MaintenanceInfo.Whitelist.UID)
		{
			if (string.Compare(MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.UID, item) == 0)
			{
				return true;
			}
		}
		foreach (string item2 in m_serverStatusRes.MaintenanceInfo.Whitelist.IP)
		{
			if (string.Compare(publicIPAddress, item2) == 0)
			{
				return true;
			}
		}
		return false;
	}

	public bool IsZoneOffline(int zoneID)
	{
		if (m_serverStatusRes == null)
		{
			return true;
		}
		return m_offlineServerIDHash.Contains(zoneID);
	}

	public bool IsZoneOverload(int zoneID)
	{
		if (m_serverStatusRes == null)
		{
			return true;
		}
		return m_overloadServerIDHash.Contains(zoneID);
	}

	public string GetMaintenanceStartTime()
	{
		return m_serverStatusRes.MaintenanceInfo.Start;
	}

	public string GetMaintenanceEndTime()
	{
		return m_serverStatusRes.MaintenanceInfo.End;
	}

	public int GetBestZoneID()
	{
		if (m_serverStatusRes.BestServerInfo != null)
		{
			return m_serverStatusRes.BestServerInfo.ZoneID;
		}
		return 0;
	}

	public string GetTimeZone()
	{
		if (m_serverStatusRes.MaintenanceInfo.TzOffset.Length != 5)
		{
			Debug.Log("Timezone offset string is invalid, " + m_serverStatusRes.MaintenanceInfo.TzOffset);
			return "(UTC+8)";
		}
		string arg = m_serverStatusRes.MaintenanceInfo.TzOffset.Substring(0, 1);
		int num = int.Parse(m_serverStatusRes.MaintenanceInfo.TzOffset.Substring(1, 2));
		int num2 = int.Parse(m_serverStatusRes.MaintenanceInfo.TzOffset.Substring(3, 2));
		string text = string.Format("(UTC{0}{1})", arg, num.ToString());
		if (num2 != 0)
		{
			text = text + "." + num2;
		}
		return text;
	}
}
