#define RELEASE
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class OrangeDataReader : CapDataReader
{
	private struct WhiteMsg
	{
		public int StartIdx;

		public string Msg;
	}

	protected static OrangeDataReader instance;

	private readonly string OrangeData = "OrangeData";

	private readonly string OrangeTextData = "OrangeTextData";

	private readonly string ExOrangeData = "ExOrangeData";

	private readonly string ExOrangeTextData = "ExOrangeTextData";

	private readonly string LocalTextData = "LocalTextData";

	private readonly List<string> listDesignData = new List<string>();

	private readonly List<string> listOperationData = new List<string>();

	private readonly Dictionary<string, uint> dicOrangeDataCRC = new Dictionary<string, uint>();

	private OperationForbiddenInfo operationInfo;

	private List<string> listWhiteMessage = new List<string>();

	public readonly char blurSymbol = '*';

	public static OrangeDataReader Instance
	{
		get
		{
			if (instance == null)
			{
				instance = new OrangeDataReader();
			}
			return instance;
		}
	}

	public OrangeDataReader()
	{
		listDesignData.Add(OrangeData);
		listDesignData.Add(OrangeTextData);
		listOperationData.Add(ExOrangeData);
		listOperationData.Add(ExOrangeTextData);
	}

	public void LoadExtraData(string slicedRawData, string extendRawData, List<string> allowedExtendTable)
	{
		try
		{
			bool flag = false;
			bool flag2 = false;
			try
			{
				if (!string.IsNullOrEmpty(slicedRawData))
				{
					Debug.Log("載入企劃切割資料");
					ReadServerData(slicedRawData, false, null, false);
					flag = true;
					flag2 = true;
				}
				if (!string.IsNullOrEmpty(extendRawData))
				{
					Debug.Log("載入營運延伸資料");
					ReadServerData(extendRawData, false, allowedExtendTable, false);
					flag2 = true;
				}
			}
			catch (Exception ex)
			{
				Debug.LogError("企劃切割資料或營運延伸資料載入失敗，Ex RawData Error=" + ex.Message + " Stack=" + ex.StackTrace);
			}
			finally
			{
				if (flag)
				{
					OrangeConst.ConstInit();
				}
				if (flag2)
				{
					ManagedSingleton<OrangeDataManager>.Instance.Initialize();
				}
			}
		}
		catch (Exception ex2)
		{
			Debug.LogError("OrangeConst.ConstInit() / OrangeDataManager.Instance.Initialize() Cause Exception=" + ex2.Message + " Stack=" + ex2.StackTrace);
		}
	}

	public void LoadExtData(string slicedRawData)
	{
		try
		{
			Debug.Log("載入企劃切割資料");
			byte[] encryptedData = Convert.FromBase64String(slicedRawData);
			ReadStream(encryptedData, null, false);
		}
		catch (Exception ex)
		{
			Debug.LogError("LoadExtData Cause Exception=" + ex.Message + " Stack=" + ex.StackTrace);
		}
	}

	public void ReadServerData(string rawData, bool registCRC = true, List<string> allowTables = null, bool forceReplaceTable = true)
	{
		byte[] array = Convert.FromBase64String(rawData);
		if (registCRC)
		{
			RegistCRC(OrangeData, Crc32.Compute(array));
		}
		ReadStream(array, allowTables, forceReplaceTable);
	}

	public IEnumerator LoadDesignsData()
	{
		int loadCount = listDesignData.Count;
		tablesByName.Clear();
		dicConstData.Clear();
		foreach (string dataName in listDesignData)
		{
			MonoBehaviourSingleton<OrangeWebRequestLoad>.Instance.Load(OrangeWebRequestLoad.LoadType.TABLE, dataName, OrangeWebRequestLoad.LoadingFlg.TEXT_DEFAULT, delegate(byte[] p_param0, string p_param1)
			{
				Debug.Log(dataName + " downloaded!!");
				RegistCRC(dataName, Crc32.Compute(p_param0));
				try
				{
					ReadStream(p_param0);
				}
				catch (Exception ex)
				{
					Debug.LogError("Exception catched message=" + ex.Message + " stack=" + ex.StackTrace);
				}
				loadCount--;
			});
		}
		while (loadCount > 0)
		{
			yield return null;
		}
		Debug.Log("LoadDesignsData done!");
		yield return null;
	}

	public IEnumerator LoadOperationData(List<string> allowTables = null)
	{
		int loadCount = listDesignData.Count;
		foreach (string dataName in listOperationData)
		{
			string p_fileName = string.Format(ManagedSingleton<ServerConfig>.Instance.ServerSetting.Patch + "/" + dataName + ".bin?" + DateTime.Now.ToString("yyyyMMddHHmmss"));
			MonoBehaviourSingleton<OrangeWebRequestLoad>.Instance.Load(OrangeWebRequestLoad.LoadType.OPERATION_TABLE, p_fileName, OrangeWebRequestLoad.LoadingFlg.TEXT_DEFAULT, delegate(byte[] p_param0, string p_param1)
			{
				if (p_param0 != null)
				{
					Debug.Log(dataName + " downloaded!!");
					RegistCRC(dataName, Crc32.Compute(p_param0));
					try
					{
						List<string> list = ((dataName == ExOrangeData) ? allowTables : null);
						Debug.Log(string.Format("營運允許的TableCount={0}", (list != null) ? list.Count.ToString() : "No Limited"));
						ReadStream(p_param0, list, false);
					}
					catch (Exception ex)
					{
						Debug.LogError("營運" + dataName + ".bin載入失敗 message=" + ex.Message + " stack=" + ex.StackTrace);
					}
				}
				else
				{
					Debug.Log(dataName + ".bin NOT FOUND. Skip it!!");
				}
				loadCount--;
			});
		}
		while (loadCount > 0)
		{
			yield return null;
		}
		Debug.Log("LoadOperationData done!");
		yield return null;
	}

	public void DeleteTableAll()
	{
		foreach (string listDesignDatum in listDesignData)
		{
			MonoBehaviourSingleton<OrangeWebRequestLoad>.Instance.DeletePersistentData(OrangeWebRequestLoad.LoadType.TABLE, listDesignDatum);
		}
		foreach (string listOperationDatum in listOperationData)
		{
			MonoBehaviourSingleton<OrangeWebRequestLoad>.Instance.DeletePersistentData(OrangeWebRequestLoad.LoadType.OPERATION_TABLE, listOperationDatum);
		}
		dicOrangeDataCRC.Clear();
	}

	public uint GetOrangeDataCRC()
	{
		return GetRegistedDataCRC(OrangeData);
	}

	public uint GetExOrangeDataCRC()
	{
		return GetRegistedDataCRC(ExOrangeData);
	}

	private uint GetRegistedDataCRC(string name)
	{
		if (dicOrangeDataCRC.ContainsKey(name))
		{
			return dicOrangeDataCRC[name];
		}
		return 0u;
	}

	private void RegistCRC(string name, uint crc32)
	{
		if (dicOrangeDataCRC.ContainsKey(name))
		{
			dicOrangeDataCRC[name] = crc32;
		}
		else
		{
			dicOrangeDataCRC.Add(name, crc32);
		}
		Debug.Log(string.Format("RegistCRC Target={0} CRC={1}", name, crc32));
	}

	public void ReadTextDataLocal()
	{
		string text = string.Format(OrangePlayerLocalData.StreamingAssetsPath + "Designs/{0}.bin", LocalTextData);
		if (!string.IsNullOrEmpty(text))
		{
			base.sourceFile = text;
			using (UnityWebRequest unityWebRequest = UnityWebRequest.Get(text))
			{
				unityWebRequest.SendWebRequest();
				while (!unityWebRequest.isDone)
				{
				}
				RegistCRC(OrangeTextData, Crc32.Compute(unityWebRequest.downloadHandler.data));
				ReadStream(unityWebRequest.downloadHandler.data);
				return;
			}
		}
		Debug.LogError(text + " IsNullOrEmpty");
	}

	public void SetOperationForbbidenInfo(byte[] rawData)
	{
		if (rawData != null)
		{
			string @string = Encoding.UTF8.GetString(rawData);
			operationInfo = JsonHelper.Deserialize<OperationForbiddenInfo>(@string);
		}
	}

	public bool IsContainForbiddenName(string name, bool allowCharacterName = true)
	{
		if (operationInfo == null || operationInfo.ForbiddenName == null)
		{
			return false;
		}
		if (allowCharacterName)
		{
			name = UpdateForbiddenNameByCharacterText(name);
		}
		foreach (string item in operationInfo.ForbiddenName)
		{
			if (name.IndexOf(item, StringComparison.OrdinalIgnoreCase) != -1)
			{
				return true;
			}
		}
		return false;
	}

	private string UpdateForbiddenNameByCharacterText(string name)
	{
		foreach (LOCALIZATION_TABLE value in ManagedSingleton<OrangeTextDataManager>.Instance.CHARATEXT_TABLE_DICT.Values)
		{
			int num = name.IndexOf(value.w_CHT, StringComparison.OrdinalIgnoreCase);
			if (num != -1)
			{
				name = name.Remove(num, value.w_CHT.Length);
			}
			num = name.IndexOf(value.w_ENG, StringComparison.OrdinalIgnoreCase);
			if (num != -1)
			{
				name = name.Remove(num, value.w_ENG.Length);
			}
			num = name.IndexOf(value.w_JP, StringComparison.OrdinalIgnoreCase);
			if (num != -1)
			{
				name = name.Remove(num, value.w_JP.Length);
			}
			num = name.IndexOf(value.w_THA, StringComparison.OrdinalIgnoreCase);
			if (num != -1)
			{
				name = name.Remove(num, value.w_THA.Length);
			}
		}
		return name;
	}

	private List<string> GetWhiteMessage()
	{
		if (operationInfo == null || operationInfo.ForbiddenMessage == null || ManagedSingleton<OrangeTextDataManager>.Instance.CHARATEXT_TABLE_DICT == null)
		{
			return new List<string>();
		}
		if (listWhiteMessage.Count == 0)
		{
			foreach (LOCALIZATION_TABLE value in ManagedSingleton<OrangeTextDataManager>.Instance.CHARATEXT_TABLE_DICT.Values)
			{
				listWhiteMessage.Add(value.w_CHT);
				listWhiteMessage.Add(value.w_ENG);
				listWhiteMessage.Add(value.w_JP);
				listWhiteMessage.Add(value.w_THA);
			}
			if (operationInfo.WhiteMessage != null)
			{
				listWhiteMessage.AddRange(operationInfo.WhiteMessage);
			}
		}
		return listWhiteMessage;
	}

	public void BlurChatMessage(ref string message, bool allowCharacterName = true)
	{
		if (operationInfo == null || operationInfo.ForbiddenMessage == null)
		{
			return;
		}
		List<WhiteMsg> list = new List<WhiteMsg>();
		foreach (string item2 in GetWhiteMessage())
		{
			int num = message.IndexOf(item2, StringComparison.OrdinalIgnoreCase);
			if (num != -1)
			{
				for (int num2 = num; num2 != -1; num2 = message.IndexOf(item2, StringComparison.OrdinalIgnoreCase))
				{
					WhiteMsg item = default(WhiteMsg);
					item.StartIdx = num2;
					item.Msg = message.Substring(num2, item2.Length);
					message = message.Remove(num2, item2.Length);
					list.Add(item);
				}
			}
		}
		foreach (string item3 in operationInfo.ForbiddenMessage)
		{
			string empty = string.Empty;
			message = message.Replace(item3, empty.PadRight(item3.Length, blurSymbol));
		}
		list.Reverse();
		foreach (WhiteMsg item4 in list)
		{
			message = message.Insert(item4.StartIdx, item4.Msg);
		}
	}

	public IEnumerator LoadForbiddenInfo()
	{
		bool isForbiddenLoaded = false;
		string forbiddenFile = "forbiddenInfo.json";
		string remotePath = string.Empty;
		remotePath = new StringBuilder().Append(ManagedSingleton<ServerConfig>.Instance.PatchUrl).Append("Designs/").Append(forbiddenFile)
			.ToString();
		MonoBehaviourSingleton<OrangeWebRequestLoad>.Instance.Load(OrangeWebRequestLoad.LoadType.UNIQUE, remotePath, OrangeWebRequestLoad.LoadingFlg.TEXT_DEFAULT, delegate(byte[] p_param0, string p_param1)
		{
			Debug.Log(remotePath + " downloaded!!");
			if (p_param0 != null)
			{
				SetOperationForbbidenInfo(p_param0);
			}
			isForbiddenLoaded = true;
		});
		float totalWaitingTime = 0f;
		float intervalTime = 0.1f;
		while (!isForbiddenLoaded)
		{
			totalWaitingTime += intervalTime;
			if (totalWaitingTime > 5f)
			{
				Debug.Log(string.Format("Download {0} failed!", forbiddenFile));
				break;
			}
			yield return new WaitForSecondsRealtime(intervalTime);
		}
		Debug.Log("LoadForbiddenInfo done!");
	}
}
