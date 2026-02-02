#define RELEASE
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Better;
using CallbackDefs;
using CriWare;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

public class OrangeWebRequestLoad : MonoBehaviourSingleton<OrangeWebRequestLoad>
{
	public enum LoadType
	{
		TABLE = 0,
		ACB = 1,
		L10N_TEXTURE = 2,
		BASIS_L10N_TEXTURE = 3,
		UNIQUE = 4,
		AWB = 5,
		OPERATION_TABLE = 6
	}

	[Flags]
	public enum LoadingFlg
	{
		NONE = 0,
		SAVE_TO_LOCAL = 1,
		RELOAD = 2,
		READ_ALL_BYTE = 4,
		TEXT_DEFAULT = 6,
		ACF = 3
	}

	private static readonly string PLAYERPREFS_WEBREQUESTLOAD_KEY = "OrangeWebRequestLoad";

	private string platform = "Android";

	private System.Collections.Generic.Dictionary<string, string> dictLocalFileCrc = new Better.Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

	private List<string> listHandlerFile = new List<string>();

	private string persistentDataPath = string.Empty;

	private StringBuilder localPath = new StringBuilder();

	private int loadingMax = 3;

	private int loadingNow;

	public string Platform
	{
		get
		{
			return platform;
		}
	}

	public float DownloadProgress { get; private set; }

	private void Awake()
	{
		persistentDataPath = Application.persistentDataPath;
		Debug.Log(persistentDataPath);
	}

	public void Init()
	{
		string @string = PlayerPrefs.GetString(PLAYERPREFS_WEBREQUESTLOAD_KEY, string.Empty);
		dictLocalFileCrc = new Better.Dictionary<string, string>();
		if (!string.IsNullOrEmpty(@string))
		{
			try
			{
				System.Collections.Generic.Dictionary<string, string> dictionary = (System.Collections.Generic.Dictionary<string, string>)JsonConvert.DeserializeObject(@string, typeof(System.Collections.Generic.Dictionary<string, string>));
				dictLocalFileCrc = dictionary;
			}
			catch
			{
				Debug.LogWarning("[OrangeWebRequestLoad] dictLocalFileCrc Parser fail.");
			}
		}
	}

	public void Load(LoadType p_loadType, string p_fileName, LoadingFlg p_loadingFlg, Callback<byte[], string> p_cb, string serverCrc = "")
	{
		string text = MD5Crypto.Encode(p_loadType.ToString() + p_fileName);
		DownloadProgress = 0f;
		localPath = new StringBuilder().Append(persistentDataPath).Append('/').Append(text);
		string text2 = localPath.ToString();
		if (p_loadingFlg.HasFlag(LoadingFlg.SAVE_TO_LOCAL) && File.Exists(text2))
		{
			byte[] bytes2;
			if (p_loadingFlg.HasFlag(LoadingFlg.RELOAD))
			{
				StartCoroutine(OnStartLoad(p_loadType, p_fileName, p_loadingFlg, p_cb, serverCrc));
			}
			else if (serverCrc != "")
			{
				string fileCRC = GetFileCRC(text);
				byte[] bytes;
				if (!serverCrc.Equals(fileCRC))
				{
					StartCoroutine(OnStartLoad(p_loadType, p_fileName, p_loadingFlg, p_cb, serverCrc));
				}
				else if (ReadAllBytes(text2, p_loadingFlg, out bytes))
				{
					p_cb.CheckTargetToInvoke(bytes, text2);
				}
				else
				{
					StartCoroutine(OnStartLoad(p_loadType, p_fileName, p_loadingFlg, p_cb, serverCrc));
				}
			}
			else if (ReadAllBytes(text2, p_loadingFlg, out bytes2))
			{
				p_cb.CheckTargetToInvoke(bytes2, text2);
			}
			else
			{
				StartCoroutine(OnStartLoad(p_loadType, p_fileName, p_loadingFlg, p_cb, serverCrc));
			}
		}
		else
		{
			StartCoroutine(OnStartLoad(p_loadType, p_fileName, p_loadingFlg, p_cb, serverCrc));
		}
	}

	public bool IsFileNeedToDownload(LoadType p_loadType, string p_fileName, string serverCrc)
	{
		string value = string.Empty;
		string key = MD5Crypto.Encode(p_loadType.ToString() + p_fileName);
		bool flag = false;
		if (dictLocalFileCrc.TryGetValue(key, out value))
		{
			if (serverCrc != "" && !serverCrc.Equals(value))
			{
				return true;
			}
			return false;
		}
		return true;
	}

    [Obsolete]
    private IEnumerator OnStartLoad(LoadType p_loadType, string p_fileName, LoadingFlg p_loadingFlg, Callback<byte[], string> p_cb, string crc)
	{
		while (loadingNow >= loadingMax)
		{
			yield return CoroutineDefine._waitForEndOfFrame;
		}
		int waitFrame = loadingNow;
		loadingNow++;
		while (waitFrame > 0)
		{
			yield return CoroutineDefine._waitForEndOfFrame;
			waitFrame--;
		}
		CommonResLoadingBar loadingBar = MonoBehaviourSingleton<UIManager>.Instance.LoadResourceUISync<CommonResLoadingBar>("UI_CommonResLoadingBar");
		string empty = string.Empty;
		string empty2 = string.Empty;
		string empty3 = string.Empty;
		byte[] bytes = null;
		string url;
		bool p_showBlocker;
		switch (p_loadType)
		{
		default:
			url = p_fileName;
			p_showBlocker = false;
			break;
		case LoadType.TABLE:
			url = GetTablePath(p_fileName);
			p_showBlocker = false;
			break;
		case LoadType.ACB:
		case LoadType.AWB:
			url = GetCriwarePath(p_fileName, p_loadType);
			p_showBlocker = true;
			break;
		case LoadType.L10N_TEXTURE:
		case LoadType.BASIS_L10N_TEXTURE:
			url = GetL10nPath(p_fileName, p_loadType);
			p_showBlocker = false;
			break;
		}
		string encodeName = MD5Crypto.Encode(p_loadType.ToString() + p_fileName);
		string savePath = string.Format("{0}/{1}", persistentDataPath, encodeName);
		loadingBar.Setup(p_showBlocker);
		while (listHandlerFile.Contains(savePath))
		{
			yield return CoroutineDefine._0_3sec;
		}
		yield return CoroutineDefine._waitForEndOfFrame;
		if (p_loadingFlg.HasFlag(LoadingFlg.SAVE_TO_LOCAL) && !p_loadingFlg.HasFlag(LoadingFlg.RELOAD) && crc == GetLocalFileCRC(encodeName) && ReadAllBytes(savePath, p_loadingFlg, out bytes))
		{
			loadingNow--;
			p_cb.CheckTargetToInvoke(bytes, savePath);
			loadingBar.OnClickCloseBtn();
			yield break;
		}
		if (!listHandlerFile.Contains(savePath))
		{
			DeletePersistentData(p_loadType, p_fileName);
		}
		using (UnityWebRequest www = new UnityWebRequest(url))
		{
			if (p_loadingFlg.HasFlag(LoadingFlg.SAVE_TO_LOCAL))
			{
				listHandlerFile.Add(savePath);
				www.downloadHandler = new DownloadHandlerFile(savePath);
			}
			else
			{
				www.downloadHandler = new DownloadHandlerBuffer();
			}
			www.SendWebRequest();
			while (!www.isDone)
			{
				loadingBar.UpdateFill(www.downloadProgress);
				DownloadProgress = www.downloadProgress;
				yield return CoroutineDefine._waitForEndOfFrame;
			}
			DownloadProgress = 1f;
			loadingBar.OnClickCloseBtn();
			if (www.isNetworkError || www.isHttpError)
			{
				switch (p_loadType)
				{
				case LoadType.TABLE:
				case LoadType.UNIQUE:
					MonoBehaviourSingleton<OrangeGameManager>.Instance.ShowErrorMsg(Code.SYSTEM_INVALID_VERSION);
					Debug.LogError("url:" + url + " " + www.error);
					break;
				case LoadType.OPERATION_TABLE:
					p_cb.CheckTargetToInvoke(null, null);
					Debug.LogError("url:" + url + " " + www.error);
					break;
				case LoadType.ACB:
				case LoadType.AWB:
					p_cb.CheckTargetToInvoke(null, null);
					Debug.LogError("url:" + url + " " + www.error);
					break;
				case LoadType.L10N_TEXTURE:
				case LoadType.BASIS_L10N_TEXTURE:
					p_cb.CheckTargetToInvoke(null, null);
					Debug.LogWarning("url:" + url + " " + www.error);
					break;
				}
				www.downloadHandler.Dispose();
				www.Dispose();
				if (p_loadingFlg.HasFlag(LoadingFlg.SAVE_TO_LOCAL))
				{
					DeletePersistentData(encodeName);
					yield return CoroutineDefine._waitForEndOfFrame;
					listHandlerFile.Remove(savePath);
				}
			}
			else if (p_loadingFlg.HasFlag(LoadingFlg.SAVE_TO_LOCAL))
			{
				www.downloadHandler.Dispose();
				www.Dispose();
				if (!string.IsNullOrEmpty(crc))
				{
					dictLocalFileCrc[encodeName] = crc;
				}
				if (p_loadingFlg.HasFlag(LoadingFlg.READ_ALL_BYTE))
				{
					bytes = File.ReadAllBytes(savePath);
				}
				p_cb.CheckTargetToInvoke(bytes, savePath);
				listHandlerFile.Remove(savePath);
			}
			else
			{
				p_cb.CheckTargetToInvoke(www.downloadHandler.data, savePath);
				www.downloadHandler.Dispose();
				www.Dispose();
			}
			loadingNow--;
		}
		if (loadingNow <= 0)
		{
			string value = JsonConvert.SerializeObject(dictLocalFileCrc);
			PlayerPrefs.SetString(PLAYERPREFS_WEBREQUESTLOAD_KEY, value);
			MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.Save();
		}
	}

	private string GetLocalFileCRC(string encodeName)
	{
		string value;
		if (dictLocalFileCrc.TryGetValue(encodeName, out value))
		{
			return value;
		}
		return "0";
	}

	private bool ReadAllBytes(string savePath, LoadingFlg loadingFlg, out byte[] bytes)
	{
		if (File.Exists(savePath) && new FileInfo(savePath).Length > 0)
		{
			if (loadingFlg.HasFlag(LoadingFlg.READ_ALL_BYTE))
			{
				bytes = File.ReadAllBytes(savePath);
			}
			else
			{
				bytes = new byte[0];
			}
			return true;
		}
		bytes = null;
		return false;
	}

	public void DeletePersistentData(LoadType p_loadType, string p_fileName)
	{
		localPath = new StringBuilder().Append(persistentDataPath).Append('/').Append(MD5Crypto.Encode(p_loadType.ToString() + p_fileName));
		if (File.Exists(localPath.ToString()))
		{
			File.Delete(localPath.ToString());
		}
	}

	public void DeletePersistentData(string p_hashName)
	{
		string text = Path.Combine(persistentDataPath, p_hashName);
		if (File.Exists(text.ToString()))
		{
			File.Delete(text.ToString());
		}
	}

	public void DeletePersistentDataAll()
	{
		foreach (string key in MonoBehaviourSingleton<AudioManager>.Instance.orangePool.Keys)
		{
			CriAtom.RemoveCueSheet(key);
		}
		PlayerPrefs.DeleteKey(AssetsBundleManager.PLAYERPREFS_AB_KEY);
		PlayerPrefs.DeleteKey(PLAYERPREFS_WEBREQUESTLOAD_KEY);
		try
		{
			string[] directories = Directory.GetDirectories(persistentDataPath);
			for (int i = 0; i < directories.Length; i++)
			{
				new DirectoryInfo(directories[i]).Delete(true);
			}
			directories = Directory.GetFiles(persistentDataPath);
			for (int i = 0; i < directories.Length; i++)
			{
				FileInfo fileInfo = new FileInfo(directories[i]);
				if (fileInfo.Extension == "")
				{
					fileInfo.Delete();
				}
			}
		}
		catch (Exception ex)
		{
			Debug.LogWarning("DeletePersistentDataAll fail = " + ex.Message + " stack = " + ex.StackTrace);
		}
	}

	private string GetCriwarePath(string cueSheetName, LoadType p_loadType)
	{
		return new StringBuilder(string.Empty).Append(ManagedSingleton<ServerConfig>.Instance.PatchUrl).Append("CriWare/").Append(platform)
			.Append("/Assets/StreamingAssets/")
			.Append(cueSheetName)
			.Append(GetExtension(p_loadType))
			.ToString();
	}

	private string GetTablePath(string tableName)
	{
		return new StringBuilder(string.Empty).Append(ManagedSingleton<ServerConfig>.Instance.PatchUrl).Append("Designs/").Append(tableName)
			.Append(GetExtension(LoadType.TABLE))
			.ToString();
	}

	private string GetL10nPath(string p_path, LoadType p_loadType)
	{
		return new StringBuilder(string.Empty).Append(ManagedSingleton<ServerConfig>.Instance.PatchUrl).Append(p_path).Append(GetExtension(p_loadType))
			.ToString();
	}

	private string GetExtension(LoadType loadType)
	{
		switch (loadType)
		{
		case LoadType.TABLE:
		case LoadType.OPERATION_TABLE:
			return ".bin";
		case LoadType.ACB:
			return ".acb";
		case LoadType.L10N_TEXTURE:
			return ".png";
		case LoadType.BASIS_L10N_TEXTURE:
			return ".basis";
		case LoadType.AWB:
			return string.Empty;
		default:
			return string.Empty;
		}
	}

	private string GetFileCRC(string fileName)
	{
		string value = string.Empty;
		dictLocalFileCrc.TryGetValue(fileName, out value);
		return value;
	}

	public void LoadRequestPath()
	{
	}

	public void LoadLicense()
	{
	}
}
