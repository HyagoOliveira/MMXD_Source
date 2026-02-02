using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using CallbackDefs;
using UnityEngine.Networking;

public class LocateManager : MonoBehaviourSingleton<LocateManager>
{
	public enum LocaleTarget
	{
		BelongAccountRegion = 0,
		BelongPVPRegion = 1
	}

	private string publicIP;

	private Dictionary<LocaleTarget, AREA_TABLE> dicCacheArea = new Dictionary<LocaleTarget, AREA_TABLE>();

	public bool GetLocalIPAddress(out string ip)
	{
		AddressFamily addressFamily = AddressFamily.InterNetwork;
		if (Socket.OSSupportsIPv6 && NetworkInterface.GetIsNetworkAvailable())
		{
			IPAddress[] addressList = Dns.GetHostEntry(Dns.GetHostName()).AddressList;
			foreach (IPAddress iPAddress in addressList)
			{
				if (iPAddress.AddressFamily == addressFamily)
				{
					ip = iPAddress.ToString();
					return true;
				}
			}
		}
		ip = "0.0.0.0";
		return false;
	}

	public string GetPublicIPAddress()
	{
		return publicIP;
	}

    [System.Obsolete]
    public void UpdatePublicIPAddress(CallbackObj p_cb)
	{
		if (string.IsNullOrEmpty(publicIP))
		{
			StartCoroutine(RetrieveIPFromWeb(p_cb));
		}
		else
		{
			p_cb.CheckTargetToInvoke(publicIP);
		}
	}

    [System.Obsolete]
    private IEnumerator RetrieveIPFromWeb(CallbackObj p_cb)
	{
		using (UnityWebRequest www = UnityWebRequest.Get("https://api.ipify.org/"))
		{
			www.timeout = HttpSetting.Timeout;
			yield return www.SendWebRequest();
			if (!www.isHttpError && !www.isNetworkError)
			{
				publicIP = www.downloadHandler.text;
				p_cb.CheckTargetToInvoke(publicIP);
			}
		}
	}

    [System.Obsolete]
    public void FindLocate(CallbackObj p_cb, LocaleTarget target)
	{
		if (dicCacheArea.ContainsKey(target))
		{
			p_cb.CheckTargetToInvoke(dicCacheArea[target]);
		}
		else
		{
			StartCoroutine(OnStartFindLocate(p_cb, target));
		}
	}

    [System.Obsolete]
    private IEnumerator OnStartFindLocate(CallbackObj p_cb, LocaleTarget target)
	{
		using (UnityWebRequest www = UnityWebRequest.Get("https://ip2c.org/?self"))
		{
			www.timeout = 10;
			yield return www.SendWebRequest();
			AREA_TABLE aREA_TABLE = null;
			if (!www.isHttpError && !www.isNetworkError)
			{
				string[] array = www.downloadHandler.text.Split(';');
				if (array.Length >= 2)
				{
					string areaCode = array[1];
					if (target != 0 && target == LocaleTarget.BelongPVPRegion)
					{
						aREA_TABLE = ManagedSingleton<OrangeDataManager>.Instance.AREA_TABLE_DICT.Values.FirstOrDefault((AREA_TABLE x) => x.s_CODE == areaCode);
					}
				}
			}
			if (aREA_TABLE == null)
			{
				aREA_TABLE = ManagedSingleton<OrangeDataManager>.Instance.AREA_TABLE_DICT.Values.FirstOrDefault((AREA_TABLE x) => x.s_CODE == "TW");
			}
			dicCacheArea[target] = aREA_TABLE;
			p_cb.CheckTargetToInvoke(aREA_TABLE);
		}
	}
}
