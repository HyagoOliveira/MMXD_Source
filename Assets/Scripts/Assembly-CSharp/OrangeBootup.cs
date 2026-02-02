#define RELEASE
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Threading;
using CallbackDefs;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

public class OrangeBootup : MonoBehaviour
{
	private bool goNext;

	private readonly string MAINTHREAD_NAME = "MainThread";

	private IEnumerator Start()
	{
		Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
		JsonConvert.DefaultSettings = () => new JsonSerializerSettings
		{
			Formatting = Formatting.Indented,
			ReferenceLoopHandling = ReferenceLoopHandling.Ignore
		};
		if (string.Compare(Thread.CurrentThread.Name, MAINTHREAD_NAME) < 0)
		{
			Thread.CurrentThread.Name = MAINTHREAD_NAME;
		}
		Debug.Log(string.Format("Client Protocol Version = [{0}/{1}/{2}]", ApiCommon.ProtocolVersionIOS, ApiCommon.ProtocolVersionAndroid, SocketCommon.ProtocolVersion));
		OrangeConst.Reader = OrangeDataReader.Instance;
		OrangeDataManager.Reader = OrangeDataReader.Instance;
		OrangeTextDataManager.Reader = OrangeDataReader.Instance;
		MonoBehaviourSingleton<OrangeWebRequestLoad>.Instance.Init();
		MonoBehaviourSingleton<OrangeGameManager>.Instance.InitResolution();
		yield return CoroutineDefine._0_3sec;
		MonoBehaviourSingleton<UIManager>.Instance.Refresh();
		MonoBehaviourSingleton<UIManager>.Instance.UpdateLoadingBlock(false);
		MonoBehaviourSingleton<RogManager>.Instance.Init();
		MonoBehaviourSingleton<LegionManager>.Instance.Init();
		CheckInternet();
		while (!goNext)
		{
			yield return CoroutineDefine._waitForEndOfFrame;
		}
		bool getBasicServerSetting = false;
		StartCoroutine(GetBasicServerSetting(delegate
		{
			getBasicServerSetting = true;
		}));
		while (!getBasicServerSetting)
		{
			yield return CoroutineDefine._waitForEndOfFrame;
		}
		MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.Load();
		Application.targetFrameRate = MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.Setting.FrameRate;
		Debug.LogFormat("Set targetFrameRate : {0}", Application.targetFrameRate);
		QualitySettings.vSyncCount = 1;
		if (!MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.Setting.vsync)
		{
			QualitySettings.vSyncCount = 0;
			Application.targetFrameRate = -1;
		}
		if (MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.PatchVer != ManagedSingleton<ServerConfig>.Instance.PatchVer)
		{
			MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.PatchVer = ManagedSingleton<ServerConfig>.Instance.PatchVer;
			OrangeDataReader.Instance.DeleteTableAll();
		}
		Debug.LogFormat("LoadDesignsData");
		yield return OrangeDataReader.Instance.LoadDesignsData();
		Debug.LogFormat("LoadForbiddenInfo");
		yield return OrangeDataReader.Instance.LoadForbiddenInfo();
		Debug.LogFormat("OrangeTextDataManager    Initialize");
		ManagedSingleton<OrangeTextDataManager>.Instance.Initialize();
		Debug.LogFormat("SetupWebService");
		ManagedSingleton<PlayerNetManager>.Instance.SetupWebService();
		MonoBehaviourSingleton<AssetsBundleManager>.Instance.Init();
		while (!MonoBehaviourSingleton<AssetsBundleManager>.Instance.IsLoadManifest)
		{
			yield return CoroutineDefine._waitForEndOfFrame;
		}
		MonoBehaviourSingleton<AudioManager>.Instance.Init();
		while (!MonoBehaviourSingleton<AudioManager>.Instance.IsInitSystemSE)
		{
			yield return CoroutineDefine._waitForEndOfFrame;
		}
		MonoBehaviourSingleton<OrangeCriRelayManager>.Instance.Init();
		while (!MonoBehaviourSingleton<OrangeCriRelayManager>.Instance.InitOK)
		{
			yield return CoroutineDefine._waitForEndOfFrame;
		}
		MonoBehaviourSingleton<LocalizationManager>.Instance.Setup();
		MonoBehaviourSingleton<LocalizationManager>.Instance.LoadFontAndDefaultAssets();
		while (!MonoBehaviourSingleton<LocalizationManager>.Instance.InitOK)
		{
			yield return CoroutineDefine._waitForEndOfFrame;
		}
		MonoBehaviourSingleton<LocalizationManager>.Instance.LoadCRC();
		while (!MonoBehaviourSingleton<LocalizationManager>.Instance.IsCrcLoad)
		{
			yield return CoroutineDefine._waitForEndOfFrame;
		}
		MonoBehaviourSingleton<AudioManager>.Instance.PreloadInitAudio();
		while (!MonoBehaviourSingleton<AudioManager>.Instance.IsInitAll)
		{
			yield return CoroutineDefine._waitForEndOfFrame;
		}
		if (!MonoBehaviourSingleton<SteamManager>.Instance.Startup())
		{
			MonoBehaviourSingleton<UIManager>.Instance.LoadResourceUI("UI_CommonMsg", delegate(CommonUI ui)
			{
				ui.SetupConfirmByKey("COMMON_TIP", "STEAM_NOT_STARTUP_FROM_STEAMPANEL", "COMMON_OK", delegate
				{
					MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_HOME);
					MonoBehaviourSingleton<UIManager>.Instance.CloseAllUI(delegate
					{
						Application.Quit();
					});
				});
			}, true, true);
			yield break;
		}
		OrangeConst.ConstInit();
		ManagedSingleton<OrangeDataManager>.Instance.Initialize();
		SingletonManager.Startup();
		foreach (Type item in CapUtility.CollectSubTypeOf(new List<Type>
		{
			typeof(IReflector),
			typeof(IManager)
		}))
		{
			PropertyInfo propertyRecursive = CapUtility.GetPropertyRecursive("Instance", item, typeof(MonoBehaviour), BindingFlags.Static | BindingFlags.Public);
			if (propertyRecursive != null)
			{
				object obj2 = null;
				propertyRecursive.GetValue(obj2, null);
			}
		}
		MonoBehaviourSingleton<StageSyncManager>.Instance.InitStageEvents();
		yield return CoroutineDefine._waitForEndOfFrame;
		MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad("shader/orangesvc", "OrangeSVC", delegate(UnityEngine.Object obj)
		{
			StartCoroutine(WarmUpSystemNeed(obj));
		}, AssetsBundleManager.AssetKeepMode.KEEP_ALWAYS);
	}

    [Obsolete]
    private IEnumerator GetBasicServerSetting(Callback p_cb)
	{
		string selectWorld = MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.SelectWorld;
		if (string.IsNullOrEmpty(selectWorld) || !Enum.IsDefined(typeof(ServerConfig.WorldServer), selectWorld))
		{
			ManagedSingleton<ServerConfig>.Instance.NowServer = ServerConfig.WorldServer.ASIA;
			MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.SelectWorld = ManagedSingleton<ServerConfig>.Instance.NowServer.ToString();
			MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.Save();
			yield return CoroutineDefine._waitForEndOfFrame;
		}
		ServerConfig.WorldServer nowServer = (ServerConfig.WorldServer)Enum.Parse(typeof(ServerConfig.WorldServer), MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.SelectWorld);
		ManagedSingleton<ServerConfig>.Instance.NowServer = nowServer;
		string settingUrl = ManagedSingleton<ServerConfig>.Instance.SettingUrl;
		UnityWebRequest www = UnityWebRequest.Get(settingUrl);
		www.timeout = HttpSetting.Timeout;
		yield return www.SendWebRequest();
		while (!www.isDone)
		{
			yield return CoroutineDefine._waitForEndOfFrame;
		}
		if (www.isNetworkError || www.isHttpError)
		{
			MonoBehaviourSingleton<OrangeGameManager>.Instance.ShowRetryMsg("NETWORK_UNAVAILABLE", delegate
			{
				StartCoroutine(GetBasicServerSetting(p_cb));
			}, delegate
			{
				MonoBehaviourSingleton<OrangeGameManager>.Instance.Quit();
			});
			yield break;
		}
		string text = AesCrypto.Decode(www.downloadHandler.text);
		Debug.Log(text);
		ManagedSingleton<ServerConfig>.Instance.ServerSetting = JsonHelper.Deserialize<GameServerInfo>(text);
		if (ManagedSingleton<ServerConfig>.Instance.ServerSetting == null)
		{
			MonoBehaviourSingleton<OrangeGameManager>.Instance.ShowRetryMsg("NETWORK_UNAVAILABLE", delegate
			{
				StartCoroutine(GetBasicServerSetting(p_cb));
			}, delegate
			{
				MonoBehaviourSingleton<OrangeGameManager>.Instance.Quit();
			});
		}
		else
		{
			MonoBehaviourSingleton<GlobalServerService>.Instance.ServerUrl = ManagedSingleton<ServerConfig>.Instance.ServerSetting.Global;
			MonoBehaviourSingleton<OrangeGameManager>.Instance.GetPatchVersion(delegate
			{
				p_cb.CheckTargetToInvoke();
			});
		}
	}

	private IEnumerator WarmUpSystemNeed(UnityEngine.Object obj)
	{
		ShaderVariantCollection svc = obj as ShaderVariantCollection;
		svc.WarmUp();
		while (!svc.isWarmedUp)
		{
			Debug.Log("isWarmed...");
			yield return CoroutineDefine._waitForEndOfFrame;
		}
		while (ManagedSingleton<ServerConfig>.Instance.ServerSetting == null)
		{
			Debug.Log("Waiting for ServerList......");
			yield return CoroutineDefine._waitForEndOfFrame;
		}
		MonoBehaviourSingleton<UIManager>.Instance.Connecting(true);
		MonoBehaviourSingleton<OrangeSceneManager>.Instance.ChangeScene("switch", OrangeSceneManager.LoadingType.DEFAULT, delegate
		{
			MonoBehaviourSingleton<UIManager>.Instance.Connecting(false);
		}, false);
	}

	private void CheckInternet()
	{
		if (Application.internetReachability == NetworkReachability.NotReachable)
		{
			MonoBehaviourSingleton<UIManager>.Instance.LoadResourceUI("UI_CommonMsg", delegate(CommonUI ui)
			{
				ui.OpenSE = SystemSE.CRI_SYSTEMSE_SYS_ERROR;
				ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
				ui.SetupYesNoByKey("NETWORK_NOT_REACHABLE_TITLE", "NETWORK_UNAVAILABLE", "COMMON_YES", "COMMON_NO", delegate
				{
					Invoke("CheckInternet", 1f);
				}, delegate
				{
					MonoBehaviourSingleton<OrangeGameManager>.Instance.Quit();
				});
			});
		}
		else
		{
			goNext = true;
		}
	}
}
