using System;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

public class ServerConfig : ManagedSingleton<ServerConfig>
{
	public enum WorldServer
	{
		ASIA = 0,
		JP = 1,
		COUNT = 2
	}

	public class WorldServerInfo
	{
		public WorldServer WorldServer;

		public string AppVer;

		public string Domain;
	}

	private WorldServerInfo[] WorldServerInfos = new WorldServerInfo[2];

	public GameServerInfo ServerSetting;

	public WorldServer NowServer { get; set; }

	public string APP_VERSION
	{
		get
		{
			return WorldServerInfos[(int)NowServer].AppVer;
		}
	}

	public string SettingUrl
	{
		get
		{
			return string.Format("http://{0}/{1}.bin?{2}", WorldServerInfos[(int)NowServer].Domain, WorldServerInfos[(int)NowServer].AppVer, DateTime.Now.ToString("yyyyMMddHHmmss"));
		}
	}

	public int PatchVer { get; set; }

	public string PatchUrl
	{
		get
		{
			return string.Format("{0}/{1}/", ServerSetting.Patch, PatchVer);
		}
	}

	public override void Initialize()
	{
		WorldServerInfos[0] = new WorldServerInfo
		{
			WorldServer = WorldServer.ASIA,
			AppVer = "5.1.0",
			Domain = "rxdres.capcom.com.tw"
		};
		WorldServerInfos[1] = new WorldServerInfo
		{
			WorldServer = WorldServer.JP,
			AppVer = "5.1.0",
			Domain = "rxdres.capcom.co.jp"
		};
	}

	public override void Dispose()
	{
	}

	public string GetPreviousSelectedServerUrl()
	{
		foreach (GameServerGameInfo item in ServerSetting.Game)
		{
			foreach (GameServerZoneInfo item2 in item.Zone)
			{
				if (item2.ID == MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.LastChoseServiceZoneID && item2.Status != 3)
				{
					return item2.Host;
				}
			}
		}
		return string.Empty;
	}

	public string GetCustomerUrl()
	{
		SaveData saveData = MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData;
		if (string.IsNullOrEmpty(saveData.DisplayPlayerId()))
		{
			return ServerSetting.Customer;
		}
		string customer = ServerSetting.Customer;
		string text = "fm06fu.6yjo45j04fu06";
		string urlLanSupport = MonoBehaviourSingleton<LocalizationManager>.Instance.GetUrlLanSupport();
		string text2 = "rxd";
		int lastChoseServiceZoneID = saveData.LastChoseServiceZoneID;
		string currentPlayerID = saveData.CurrentPlayerID;
		string text3 = Application.platform.ToString();
		string text4 = DateTime.UtcNow.AddHours(8.0).ToString("yyyy-MM-dd HH:mm:ss");
		string platformChkCode = GetPlatformChkCode(text + text2 + currentPlayerID + text4 + lastChoseServiceZoneID);
		return string.Format(customer, urlLanSupport, text2, lastChoseServiceZoneID, currentPlayerID, text3, text4, platformChkCode).Replace(" ", "%20");
	}

	public string GetEventUrl()
	{
		string @event = ServerSetting.Platform.Event;
		string platformLan = MonoBehaviourSingleton<LocalizationManager>.Instance.GetPlatformLan();
		string arg = (string.IsNullOrEmpty(MonoBehaviourSingleton<OrangeGameManager>.Instance.WebToken) ? string.Empty : MonoBehaviourSingleton<OrangeGameManager>.Instance.WebToken);
		return string.Format(@event, platformLan, arg).Replace(" ", "%20");
	}

	public string GetEventReddotUrl()
	{
		string notification = ServerSetting.Platform.Notification;
		SaveData saveData = MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData;
		string platformLan = MonoBehaviourSingleton<LocalizationManager>.Instance.GetPlatformLan();
		string text = saveData.LastChoseServiceZoneID.ToString();
		string currentPlayerID = saveData.CurrentPlayerID;
		string text2 = Application.platform.ToString();
		string text3 = DateTime.UtcNow.AddHours(8.0).ToString("yyyy-MM-dd HH:mm:ss");
		string platformChkCode = GetPlatformChkCode("fe,spdw,e,ozpo,fx," + platformLan + text + currentPlayerID + text2 + text3);
		return string.Format(notification, platformLan, text, currentPlayerID, text2, text3, platformChkCode).Replace(" ", "%20");
	}

	public string GetPlatformChkCode(string str)
	{
		string empty = string.Empty;
		using (SHA256Managed sHA256Managed = new SHA256Managed())
		{
			StringBuilder stringBuilder = new StringBuilder();
			byte[] array = sHA256Managed.ComputeHash(Encoding.UTF8.GetBytes(str));
			for (int i = 0; i < array.Length; i++)
			{
				stringBuilder.Append(array[i].ToString("x2"));
			}
			return stringBuilder.ToString();
		}
	}
}
