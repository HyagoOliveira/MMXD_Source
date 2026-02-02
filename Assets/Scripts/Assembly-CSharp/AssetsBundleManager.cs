#define RELEASE
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Better;
using CallbackDefs;
using Newtonsoft.Json;
using StageLib;
using UnityEngine;
using UnityEngine.Networking;

public class AssetsBundleManager : MonoBehaviourSingleton<AssetsBundleManager>
{
	public delegate void OnAsyncLoadAssetComplete<T>(T asset) where T : UnityEngine.Object;

	public delegate void OnAsyncLoadGameobjectComplete<GameObject>(GameObject go);

	public enum AssetKeepMode
	{
		KEEP_IN_SCENE = 0,
		KEEP_ALWAYS = 1,
		KEEP_NO = 2
	}

	public class AssetbundleInfo
	{
		public AssetBundle Bundle;

		public AssetKeepMode KeepMode;

		public string bundleName;

		public AssetbundleInfo(AssetBundle p_bundle, AssetKeepMode p_keep)
		{
			Bundle = p_bundle;
			KeepMode = p_keep;
		}
	}

	private enum FileInfo
	{
		NOT_EXIST = 0,
		LOAD_OVERRIDE = 1,
		LOAD_FROM_DISK = 2,
		LOAD_FROM_SERVER = 3
	}

	private readonly int DOWNLOAD_LIMIT_MAX = 5;

	private readonly long TRIGGER_GC_MB = 4194304L;

	private readonly long TRIGGER_CACHE_HIGH_USAGE = 1048576L;

	private static readonly string MANIFEST_NAME = "AssetBundleManifest";

	public static readonly string PLAYERPREFS_AB_KEY = "OrangeAssetBundle";

	private ServerConfig serverScriptableObject;

	private AssetBundleScriptableObject assetBundleScriptableObject;

	private bool isLoadManifest;

	private string path = string.Empty;

	private AssetBundleManifest manifest;

	private System.Collections.Generic.Dictionary<string, AssetbundleInfo> dictBundleInfo = new Better.Dictionary<string, AssetbundleInfo>(StringComparer.OrdinalIgnoreCase);

	private System.Collections.Generic.Dictionary<string, AssetbundleId> dictBundleID = new Better.Dictionary<string, AssetbundleId>(StringComparer.OrdinalIgnoreCase);

	private System.Collections.Generic.Dictionary<string, uint> dictLocalBundleCrc = new Better.Dictionary<string, uint>(StringComparer.OrdinalIgnoreCase);

	private string strPlatformName = "StandaloneWindows";

	private List<string> DownloadingList = new List<string>();

	private int retryCount;

	private WaitForSeconds m_retryTime;

	private string persistentDataPath = string.Empty;

	private int lastSaveCount = -1;

	private System.Collections.Generic.HashSet<string> bundleKeepMap = new System.Collections.Generic.HashSet<string>();

	public bool IsLoadManifest
	{
		get
		{
			return isLoadManifest;
		}
	}

	public string Path
	{
		get
		{
			return path;
		}
	}

	public float DownloadProgress { get; private set; }

	private void OnEnable()
	{
		Singleton<GenericEventManager>.Instance.AttachEvent(EventManager.ID.PATCH_CHANGE, OnPatchChange);
	}

	private void OnDisable()
	{
		Singleton<GenericEventManager>.Instance.DetachEvent(EventManager.ID.PATCH_CHANGE, OnPatchChange);
	}

	public void Init()
	{
		persistentDataPath = Application.persistentDataPath;
        UnityEngine.Debug.Log(persistentDataPath);
        isLoadManifest = false;
		serverScriptableObject = ManagedSingleton<ServerConfig>.Instance;
		assetBundleScriptableObject = AssetBundleScriptableObject.Instance;
		m_retryTime = new WaitForSeconds(assetBundleScriptableObject.m_retryTime);
		path = string.Empty;
		string bundlePath_StandaloneWindows = assetBundleScriptableObject.m_bundlePath_StandaloneWindows;
		if (assetBundleScriptableObject.m_useDebugLocalPath)
		{
			path = Application.streamingAssetsPath + "/";
		}
		else
		{
			path = new StringBuilder(path).Append(serverScriptableObject.PatchUrl).Append(bundlePath_StandaloneWindows.Replace("AssetBundles", "AssetBundlesEncrypt")).ToString();
		}
		StartCoroutine(OnStartLoadManifest());
	}

	public void OnPatchChange()
	{
		isLoadManifest = false;
		Init();
	}

    [Obsolete]
    private IEnumerator OnStartLoadManifest()
	{
		while (!Caching.ready)
		{
			yield return CoroutineDefine._waitForEndOfFrame;
		}
		SaveData localdata = MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData;
		string reqConfigPath = new StringBuilder(path).Append(AssetBundleScriptableObject.CONFIG_NAME).UrlAntiCache();
		UnityWebRequest reqConfig = UnityWebRequest.Get(reqConfigPath);
		yield return reqConfig.SendWebRequest();
		while (!reqConfig.isDone)
		{
			yield return CoroutineDefine._waitForEndOfFrame;
		}
		if (reqConfig.isHttpError || reqConfig.isNetworkError)
		{
			Debug.LogError("reqConfig loading fail. path:" + reqConfigPath);
			MonoBehaviourSingleton<OrangeGameManager>.Instance.ShowRetryMsg(delegate
			{
				StartCoroutine(OnStartLoadManifest());
			}, delegate
			{
				MonoBehaviourSingleton<OrangeGameManager>.Instance.Quit();
			});
			yield break;
		}
		AssetbundleConfig assetbundleConfig = (AssetbundleConfig)JsonConvert.DeserializeObject(AesCrypto.Decode(reqConfig.downloadHandler.text), typeof(AssetbundleConfig));
		localdata.AssetsDate = assetbundleConfig.date;
		foreach (AssetbundleId item in assetbundleConfig.ListAssetbundleId)
		{
			item.SetKeys();
			dictBundleID[item.name] = item;
		}
		string @string = PlayerPrefs.GetString(PLAYERPREFS_AB_KEY, string.Empty);
		dictLocalBundleCrc = new Better.Dictionary<string, uint>();
		if (!string.IsNullOrEmpty(@string))
		{
			try
			{
				System.Collections.Generic.Dictionary<string, uint> dictionary = (System.Collections.Generic.Dictionary<string, uint>)JsonConvert.DeserializeObject(@string, typeof(System.Collections.Generic.Dictionary<string, uint>));
				dictLocalBundleCrc = dictionary;
			}
			catch
			{
				Debug.LogWarning("[AssetBundleManager] dictLocalBundleCrc Parser fail.");
			}
		}
		string manifestPath = ((!assetBundleScriptableObject.m_useDebugLocalPath) ? new StringBuilder(path).Append(strPlatformName).UrlAntiCache() : new StringBuilder(path).Append(strPlatformName).ToString());
		UnityWebRequest request = UnityWebRequestAssetBundle.GetAssetBundle(manifestPath);
		yield return request.SendWebRequest();
		while (!request.isDone)
		{
			yield return CoroutineDefine._waitForEndOfFrame;
		}
		if (request.isHttpError || request.isNetworkError)
		{
			Debug.LogError("manifest loading fail. path:" + manifestPath);
			MonoBehaviourSingleton<OrangeGameManager>.Instance.ShowRetryMsg(delegate
			{
				StartCoroutine(OnStartLoadManifest());
			}, delegate
			{
				MonoBehaviourSingleton<OrangeGameManager>.Instance.Quit();
			});
			yield break;
		}
		AssetBundle assetBundle = (request.downloadHandler as DownloadHandlerAssetBundle).assetBundle;
		manifest = assetBundle.LoadAsset<AssetBundleManifest>(MANIFEST_NAME);
		if (manifest == null)
		{
			Debug.LogError(string.Format("Can't get manifest...PATH : [{0}]", manifestPath));
		}
		else
		{
			Debug.Log("loadManifestOK");
			isLoadManifest = true;
		}
		request.Dispose();
		assetBundle.Unload(false);
		yield return CoroutineDefine._waitForEndOfFrame;
	}

	public void LoadAssets(string[] abName, Callback p_cb, AssetKeepMode p_keepMode = AssetKeepMode.KEEP_IN_SCENE, bool p_checkSize = true)
	{
		MonoBehaviourSingleton<UIManager>.Instance.UpdateLoadingBlock(true);
		for (int i = 0; i < abName.Length; i++)
		{
			abName[i] = abName[i].ToLower();
		}
		StartCoroutine(OnStartLoadAssets(abName, p_cb, p_keepMode, p_checkSize));
	}

	private IEnumerator OnStartLoadAssets(string[] abName, Callback p_cb, AssetKeepMode p_keepMode = AssetKeepMode.KEEP_IN_SCENE, bool p_checkSize = true)
	{
		while (!Caching.ready)
		{
			yield return CoroutineDefine._waitForEndOfFrame;
		}
		while (!isLoadManifest)
		{
			yield return CoroutineDefine._waitForEndOfFrame;
		}
		bool goNext = true;
		if (p_checkSize)
		{
			float totalFileSize = 0f;
			foreach (string text in abName)
			{
				if (!dictLocalBundleCrc.ContainsKey(text))
				{
					totalFileSize += GetBundleSize(text);
				}
			}
			if (totalFileSize > 0f)
			{
				goNext = false;
				string format = ((totalFileSize > 1048576f) ? "F2" : "F4");
				totalFileSize /= 1048576f;
				MonoBehaviourSingleton<UIManager>.Instance.LoadResourceUI("UI_CommonMsg", delegate(CommonUI ui)
				{
					MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
					ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
					string text2 = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("NETWORK_DOWNLOAD_DESC_1"), totalFileSize.ToString(format));
					if (totalFileSize > 50f)
					{
						text2 = string.Format("{0}\n{1}", text2, MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("NETWORK_DOWNLOAD_DESC_2"));
					}
					ui.MuteSE = true;
					ui.SetupConfirm(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("NETWORK_DOWNLOAD_TITLE"), text2, MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_OK"), delegate
					{
						MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK17);
						goNext = true;
					});
				}, true);
			}
		}
		while (!goNext)
		{
			yield return CoroutineDefine._waitForEndOfFrame;
		}
		int length = abName.Length;
		for (int i = 0; i < length; i++)
		{
			if (i % DOWNLOAD_LIMIT_MAX > 0)
			{
				yield return CoroutineDefine._waitForEndOfFrame;
			}
			else
			{
				SavePlayerPrefsCRC();
			}
			while (DownloadingList.Count > DOWNLOAD_LIMIT_MAX)
			{
				yield return CoroutineDefine._waitForEndOfFrame;
			}
			StartCoroutine(OnStartLoadSingleAsset(abName[i], p_keepMode));
			Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.UPDATE_LOADING_PROGRESS, (float)i / (float)length);
		}
		while (DownloadingList.Count > 0)
		{
			yield return CoroutineDefine._waitForEndOfFrame;
		}
		if (p_cb != null)
		{
			p_cb();
		}
		MonoBehaviourSingleton<UIManager>.Instance.UpdateLoadingBlock(false);
		Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.UPDATE_DOWNLOAD_BAR, false);
	}

    [Obsolete]
    private IEnumerator OnStartLoadSingleAsset(string bundleName, AssetKeepMode keepMode = AssetKeepMode.KEEP_IN_SCENE)
	{
		bool isLoaded = false;
		bool isNoKeep = keepMode == AssetKeepMode.KEEP_NO;
		AssetbundleId id = null;
		if (!dictBundleID.TryGetValue(bundleName, out id))
		{
			Debug.LogError("Id is null, bundleName = " + bundleName);
		}
		if (!isNoKeep && keepMode == AssetKeepMode.KEEP_IN_SCENE && id.size >= TRIGGER_CACHE_HIGH_USAGE)
		{
			keepMode = AssetKeepMode.KEEP_ALWAYS;
		}
		if (!isNoKeep)
		{
			while (!isLoadManifest || !Caching.ready)
			{
				yield return CoroutineDefine._waitForEndOfFrame;
			}
			while (DownloadingList.Contains(bundleName))
			{
				isLoaded = true;
				yield return CoroutineDefine._waitForEndOfFrame;
			}
		}
		if (!isLoaded)
		{
			DownloadingList.Add(bundleName);
			if (!isNoKeep)
			{
				string[] subAssets = manifest.GetDirectDependencies(bundleName);
				foreach (string text in subAssets)
				{
					if (!dictBundleInfo.ContainsKey(text))
					{
						yield return OnStartLoadSingleAsset(text, keepMode);
					}
				}
			}
			if (!dictBundleInfo.ContainsKey(bundleName))
			{
				string key = string.Empty;
				FileInfo fileInfo = GetFileInfo(bundleName, out key, out id);
				AssetBundle assetBundle = null;
				uint crc = 0u;
				if (id == null)
				{
					Debug.LogError("Id is null, bundleName = " + bundleName);
				}
				else
				{
					crc = id.crc;
				}
				switch (fileInfo)
				{
				case FileInfo.NOT_EXIST:
					Debug.LogWarning("Target Assetbundle doesn't exist. Name:" + bundleName);
					DownloadingList.Remove(bundleName);
					Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.UPDATE_DOWNLOAD_BAR, false);
					yield break;
				case FileInfo.LOAD_OVERRIDE:
					MonoBehaviourSingleton<OrangeWebRequestLoad>.Instance.DeletePersistentData(id.hash);
					break;
				case FileInfo.LOAD_FROM_DISK:
				{
					string text2 = System.IO.Path.Combine(persistentDataPath, id.hash);
					if (File.Exists(text2))
					{
						if (id.size > TRIGGER_GC_MB)
						{
							GC.Collect();
						}
						byte[] bytes = File.ReadAllBytes(text2);
						AssetBundleStream.ReadBytes(id.Keys, ref bytes);
						AssetBundleCreateRequest createRequest2 = AssetBundle.LoadFromMemoryAsync(bytes);
						while (!createRequest2.isDone)
						{
							yield return null;
						}
						assetBundle = createRequest2.assetBundle;
						if (assetBundle != null)
						{
							AssetbundleInfo value = new AssetbundleInfo(assetBundle, keepMode);
							dictBundleInfo.Add(bundleName, value);
						}
					}
					break;
				}
				}
				DownloadProgress = 0f;
				if (assetBundle == null)
				{
					lastSaveCount = -1;
					Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.UPDATE_DOWNLOAD_BAR, true);
					UnityWebRequest request = new UnityWebRequest(new StringBuilder(path).AppendEscapeDataString(bundleName));
					if (isNoKeep)
					{
						request.downloadHandler = new DownloadHandlerFile(System.IO.Path.Combine(persistentDataPath, id.hash));
					}
					else
					{
						request.downloadHandler = new DownloadHandlerBuffer();
					}
					request.SendWebRequest();
					while (!request.isDone)
					{
						DownloadProgress = request.downloadProgress;
						Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.UPDATE_DOWNLOAD_BAR, true);
						yield return CoroutineDefine._waitForEndOfFrame;
					}
					yield return request;
					if (request.isHttpError || request.isNetworkError)
					{
						request.downloadHandler.Dispose();
						request.Dispose();
						if (isNoKeep)
						{
							MonoBehaviourSingleton<OrangeWebRequestLoad>.Instance.DeletePersistentData(id.hash);
						}
						DownloadProgress = 0f;
						if (retryCount > assetBundleScriptableObject.m_retryCountMax)
						{
							bool goNext = false;
							Debug.LogError("AssetBundle loading fail,bundleName:" + bundleName);
							MonoBehaviourSingleton<OrangeGameManager>.Instance.ShowRetryMsg(delegate
							{
								retryCount = 0;
								goNext = true;
							}, delegate
							{
								if (MonoBehaviourSingleton<OrangeSceneManager>.Instance.NowScene == "bootup")
								{
									MonoBehaviourSingleton<OrangeGameManager>.Instance.Quit();
								}
								else
								{
									ReturnToTitle();
								}
							});
							while (!goNext)
							{
								yield return CoroutineDefine._waitForEndOfFrame;
							}
							DownloadingList.Remove(bundleName);
							yield return StartCoroutine(OnStartLoadSingleAsset(bundleName, keepMode));
						}
						else
						{
							yield return m_retryTime;
							retryCount++;
							DownloadingList.Remove(bundleName);
							yield return StartCoroutine(OnStartLoadSingleAsset(bundleName, keepMode));
						}
					}
					else
					{
						DownloadProgress = 1f;
						Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.UPDATE_DOWNLOAD_BAR, true);
						retryCount = 0;
						switch (keepMode)
						{
						case AssetKeepMode.KEEP_IN_SCENE:
						case AssetKeepMode.KEEP_ALWAYS:
						{
							byte[] bytes2 = request.downloadHandler.data;
							AssetBundleStream.ReadBytes(id.Keys, ref bytes2);
							AssetBundleCreateRequest createRequest2 = AssetBundle.LoadFromMemoryAsync(bytes2);
							while (!createRequest2.isDone)
							{
								yield return null;
							}
							assetBundle = createRequest2.assetBundle;
							if (assetBundle == null)
							{
								StopAllCoroutines();
								DownloadingList.Clear();
								MonoBehaviourSingleton<UIManager>.Instance.LoadResourceUI("UI_CommonMsg", delegate(CommonUI ui)
								{
									ui.OpenSE = SystemSE.CRI_SYSTEMSE_SYS_ERROR;
									ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
									ui.SetupConfirmByKey("COMMON_TIP", "GAME_ERROR_MSG_RETURN", "COMMON_OK", delegate
									{
										ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_HOME;
										ReturnToTitle();
									});
								}, true);
								yield break;
							}
							dictLocalBundleCrc[key] = crc;
							Save(request.downloadHandler.data, id.hash);
							AssetbundleInfo value = new AssetbundleInfo(assetBundle, keepMode);
							dictBundleInfo.Add(bundleName, value);
							break;
						}
						case AssetKeepMode.KEEP_NO:
							dictLocalBundleCrc[key] = crc;
							break;
						}
						request.downloadHandler.Dispose();
						request.Dispose();
					}
					if (id.size > TRIGGER_GC_MB)
					{
						GC.Collect();
					}
				}
			}
			DownloadingList.Remove(bundleName);
		}
		if (DownloadingList.Count == 0)
		{
			SavePlayerPrefsCRC();
		}
	}

	public bool IsDownloading()
	{
		return DownloadingList.Count > 0;
	}

	private void SavePlayerPrefsCRC()
	{
		if (dictLocalBundleCrc.Count != lastSaveCount)
		{
			string value = JsonConvert.SerializeObject(dictLocalBundleCrc);
			PlayerPrefs.SetString(PLAYERPREFS_AB_KEY, value);
			PlayerPrefs.Save();
			lastSaveCount = dictLocalBundleCrc.Count;
		}
	}

	public bool IsBundleExistInList(string bundleName)
	{
		return dictBundleID.ContainsKey(bundleName);
	}

	public uint GetCrcByBundleName(string bundleName)
	{
		if (IsBundleExistInList(bundleName))
		{
			return dictBundleID[bundleName].crc;
		}
		return 0u;
	}

	public void LoadAllAssetBundle(Callback p_cb)
	{
		long totalFileSize = GetSizeNotyetDownload();
		if (totalFileSize == 0L)
		{
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_Tip", delegate(TipUI tipUI)
			{
				tipUI.Setup(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("TIP_ALL_DOWNLOADED_IS_NEW"), true);
				p_cb.CheckTargetToInvoke();
			});
			return;
		}
		totalFileSize /= 1048576L;
		MonoBehaviourSingleton<UIManager>.Instance.LoadResourceUI("UI_CommonMsg", delegate(CommonUI ui)
		{
			string text = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("NETWORK_DOWNLOAD_DESC_1"), totalFileSize.ToString("F2"));
			if (totalFileSize > 50)
			{
				text = string.Format("{0}\n{1}", text, MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("NETWORK_DOWNLOAD_DESC_2"));
			}
			ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
			ui.YesSE = SystemSE.CRI_SYSTEMSE_SYS_OK17;
			ui.SetupYesNO(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("NETWORK_DOWNLOAD_TITLE"), text, MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_OK"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_CANCEL"), delegate
			{
				MonoBehaviourSingleton<UIManager>.Instance.OpenLoadingUI(delegate
				{
					StartCoroutine(OnLoadAllAssetBundle(p_cb));
				}, OrangeSceneManager.LoadingType.FULL);
			}, delegate
			{
				p_cb.CheckTargetToInvoke();
			});
		}, true);
	}

	private IEnumerator OnLoadAllAssetBundle(Callback p_cb)
	{
		while (!Caching.ready)
		{
			yield return CoroutineDefine._waitForEndOfFrame;
		}
		if (!isLoadManifest)
		{
			yield return CoroutineDefine._waitForEndOfFrame;
		}
		List<string> listNotyetDownload = GetListNotyetDownload();
		LoadAssets(listNotyetDownload.ToArray(), delegate
		{
			if (p_cb.Target != null)
			{
				p_cb();
			}
			MonoBehaviourSingleton<UIManager>.Instance.CloseLoadingUI(null);
		}, AssetKeepMode.KEEP_NO, false);
		yield return CoroutineDefine._waitForEndOfFrame;
	}

	public void GetAssetAndAsyncLoad<T>(string bundleName, string assetName, OnAsyncLoadAssetComplete<T> p_cb, AssetKeepMode keepMode = AssetKeepMode.KEEP_IN_SCENE) where T : UnityEngine.Object
	{
		AssetbundleInfo value = null;
		if (dictBundleInfo.TryGetValue(bundleName, out value))
		{
			T asset = value.Bundle.LoadAsset<T>(assetName.ToLower());
			p_cb(asset);
			return;
		}
		if (StageUpdate.gbStageReady)
		{
			Debug.Log("Load AB + " + bundleName + " " + assetName);
		}
		if (!dictBundleID.ContainsKey(bundleName))
		{
			p_cb(null);
			return;
		}
		MonoBehaviourSingleton<UIManager>.Instance.UpdateLoadingBlock(true);
		StartCoroutine(AsyncLoadAsset(bundleName.ToLower(), assetName.ToLower(), p_cb, keepMode));
	}

	public void GetGameObjectAndAsyncLoad(string bundleName, string assetName, OnAsyncLoadGameobjectComplete<GameObject> p_cb, AssetKeepMode keepMode = AssetKeepMode.KEEP_IN_SCENE, bool unloadImmediate = true)
	{
		GetAssetAndAsyncLoad(bundleName, assetName, delegate(GameObject obj)
		{
			if (obj != null)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(obj);
				if (unloadImmediate)
				{
					UnloadSingleBundleCache(bundleName);
				}
				gameObject.name = assetName;
				p_cb(gameObject);
			}
			else
			{
				Debug.LogWarning("[AssetBundleManager] GetGameObjectAndAsyncLoad Fail!!");
				p_cb(new GameObject(bundleName));
			}
		});
	}

	private IEnumerator AsyncLoadAsset<T>(string bundleName, string assetName, OnAsyncLoadAssetComplete<T> p_cb, AssetKeepMode keepMode) where T : UnityEngine.Object
	{
		yield return OnStartLoadSingleAsset(bundleName, keepMode);
		GetAssetAndAsyncLoad(bundleName, assetName, p_cb);
		Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.UPDATE_DOWNLOAD_BAR, false);
		MonoBehaviourSingleton<UIManager>.Instance.UpdateLoadingBlock(false);
	}

	public T GetAssstSync<T>(string bundleName, string assetName) where T : UnityEngine.Object
	{
		AssetbundleInfo value = null;
		if (dictBundleInfo.TryGetValue(bundleName, out value))
		{
			return value.Bundle.LoadAsset<T>(assetName.ToLower());
		}
		return null;
	}

	public void UnloadAllBundleCache(Callback p_cb, bool forceClear = false)
	{
		Debug.Log("[AssetBundleManager] UnloadAllBundleCache.");
		StartCoroutine(OnStartUnloadBundleCache(p_cb, forceClear));
	}

	private IEnumerator OnStartUnloadBundleCache(Callback p_cb, bool forceClear)
	{
		int count = 0;
		int waitCount = 1;
		List<string> list = new List<string>(dictBundleInfo.Keys);
		bundleKeepMap.Clear();
		if (!forceClear)
		{
			foreach (KeyValuePair<string, AssetbundleInfo> item2 in dictBundleInfo)
			{
				string key2 = item2.Key;
				AssetbundleInfo value = item2.Value;
				if (item2.Value.KeepMode != AssetKeepMode.KEEP_ALWAYS)
				{
					continue;
				}
				string[] allDependencies = manifest.GetAllDependencies(key2);
				foreach (string item in allDependencies)
				{
					if (!bundleKeepMap.Contains(item))
					{
						bundleKeepMap.Add(item);
					}
				}
			}
		}
		foreach (string key in list)
		{
			AssetbundleInfo assetbundleInfo = dictBundleInfo[key];
			if ((forceClear || assetbundleInfo.KeepMode != AssetKeepMode.KEEP_ALWAYS) && !bundleKeepMap.Contains(key))
			{
				assetbundleInfo.Bundle.Unload(false);
				assetbundleInfo.Bundle = null;
				if (count > waitCount)
				{
					count = 0;
					yield return CoroutineDefine._waitForEndOfFrame;
				}
				else
				{
					count++;
				}
				dictBundleInfo.Remove(key);
			}
		}
		yield return CoroutineDefine._waitForEndOfFrame;
		if (forceClear)
		{
			AssetBundle.UnloadAllAssetBundles(true);
			Resources.UnloadUnusedAssets();
			MonoBehaviourSingleton<StageMaterialManager>.Instance.Clear();
		}
		if (p_cb != null)
		{
			p_cb();
		}
	}

	public void UnloadSingleBundleCache(string p_bundleName)
	{
		AssetbundleInfo value;
		if (dictBundleInfo.TryGetValue(p_bundleName, out value))
		{
			if (value.KeepMode != AssetKeepMode.KEEP_ALWAYS)
			{
				value.Bundle.Unload(false);
				value = null;
				dictBundleInfo.Remove(p_bundleName);
			}
		}
		else
		{
			Debug.LogWarning("can't find Bundle key : " + p_bundleName);
		}
	}

	public List<string> GetListNotyetDownload()
	{
		List<string> list = new List<string>();
		foreach (AssetbundleId value in dictBundleID.Values)
		{
			if (!dictBundleInfo.ContainsKey(value.name) && !ManagedSingleton<OrangeTableHelper>.Instance.ABIgonreFromTable(value.name))
			{
				string key = string.Empty;
				AssetbundleId id = null;
				FileInfo fileInfo = GetFileInfo(value.name, out key, out id);
				if (fileInfo == FileInfo.LOAD_OVERRIDE || fileInfo == FileInfo.LOAD_FROM_SERVER)
				{
					list.Add(value.name);
				}
			}
		}
		return list;
	}

	public long GetSizeNotyetDownload()
	{
		long num = 0L;
		foreach (AssetbundleId value2 in dictBundleID.Values)
		{
			uint value = 0u;
			if (dictLocalBundleCrc.TryGetValue(value2.name, out value))
			{
				if (value != value2.crc)
				{
					num += value2.size;
				}
			}
			else
			{
				num += value2.size;
			}
		}
		return num;
	}

	public long GetBundleSize(string bundleName)
	{
		AssetbundleId value = null;
		if (dictBundleID.TryGetValue(bundleName, out value))
		{
			return value.size;
		}
		return 0L;
	}

	private void Save(byte[] data, string name)
	{
		string text = System.IO.Path.Combine(persistentDataPath, name);
		if (!Directory.Exists(System.IO.Path.GetDirectoryName(text)))
		{
			Directory.CreateDirectory(System.IO.Path.GetDirectoryName(text));
		}
		File.WriteAllBytes(text, data);
	}

	private void ReturnToTitle()
	{
		MonoBehaviourSingleton<UIManager>.Instance.UpdateLoadingBlock(false);
		StopAllCoroutines();
		DownloadingList.Clear();
		MonoBehaviourSingleton<OrangeSceneManager>.Instance.ChangeScene("switch", OrangeSceneManager.LoadingType.DEFAULT, null, false);
	}

	private FileInfo GetFileInfo(string p_bundleName, out string key, out AssetbundleId id)
	{
		FileInfo fileInfo = FileInfo.NOT_EXIST;
		if (!dictBundleID.TryGetValue(p_bundleName, out id))
		{
			key = string.Empty;
			return FileInfo.NOT_EXIST;
		}
		key = p_bundleName;
		uint value = 0u;
		if (dictLocalBundleCrc.TryGetValue(p_bundleName, out value))
		{
			return (value != id.crc) ? FileInfo.LOAD_OVERRIDE : FileInfo.LOAD_FROM_DISK;
		}
		return FileInfo.LOAD_FROM_SERVER;
	}
}
