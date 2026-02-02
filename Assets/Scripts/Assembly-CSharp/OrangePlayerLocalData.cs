#define RELEASE
using System;
using Newtonsoft.Json;
using UnityEngine;

public class OrangePlayerLocalData : MonoBehaviourSingleton<OrangePlayerLocalData>
{
	public static string DefaultPlayerID = "DefaultPlayerID";

	public static string SAVE_KEY = "SteamSaveData";

	private static string SAVE_IDENTIFY = "SteamOnlyID";

	private SaveData saveData;

	public static string StreamingAssetsPath
	{
		get
		{
			return "file://" + Application.streamingAssetsPath + "/";
		}
	}

	public string IDFA { get; private set; } = "";


	public SaveData SaveData
	{
		get
		{
			if (saveData == null)
			{
				Load();
			}
			return saveData;
		}
	}

	public OrangePlayerLocalData Load()
	{
		if (!Application.RequestAdvertisingIdentifierAsync(delegate(string advertisingId, bool trackingEnabled, string error)
		{
			Debug.Log("advertisingId " + advertisingId + " " + trackingEnabled + " " + error);
			if (trackingEnabled)
			{
				IDFA = advertisingId;
			}
		}))
		{
			Debug.Log("IDFA is not supported in current platform.");
		}
		if (PlayerPrefs.HasKey(SAVE_KEY))
		{
			string value = AesCrypto.Decode(PlayerPrefs.GetString(SAVE_KEY));
			saveData = JsonConvert.DeserializeObject<SaveData>(value);
			if (saveData == null || saveData.UID == string.Empty)
			{
				Debug.LogError("資料異常,重抓UID");
				CreateNewOne();
			}
		}
		else
		{
			CreateNewOne();
		}
		return this;
	}

	private void CreateNewOne()
	{
		if (Application.isPlaying)
		{
			MonoBehaviourSingleton<OrangeWebRequestLoad>.Instance.DeletePersistentDataAll();
		}
		Caching.ClearCache();
		PlayerPrefs.DeleteAll();
		saveData = new SaveData();
		saveData.UID = GetOnlyIdentify();
		SaveData.PatchVer = ManagedSingleton<ServerConfig>.Instance.PatchVer;
		SetBattleSettingDefault(ref saveData.DefaultSetting);
		Save();
	}

	public void Save()
	{
		if (saveData != null)
		{
			PlayerPrefs.SetFloat("ORANGE_SETTING_SE_VOLUME", saveData.Setting.SoundVol);
			string value = AesCrypto.Encode(JsonConvert.SerializeObject(saveData));
			PlayerPrefs.SetString(SAVE_KEY, value);
			PlayerPrefs.Save();
		}
	}

	public void Clear()
	{
		Caching.ClearCache();
		MonoBehaviourSingleton<OrangeWebRequestLoad>.Instance.DeletePersistentDataAll();
		PlayerPrefs.DeleteAll();
		saveData = null;
	}

	public void Logout()
	{
		ManagedSingleton<PlayerNetManager>.Instance.AccountInfo = new AccountInfo();
		PlayerPrefs.DeleteKey(SAVE_IDENTIFY);
		saveData = new SaveData();
		saveData.UID = GetOnlyIdentify();
		SaveData.PatchVer = ManagedSingleton<ServerConfig>.Instance.PatchVer;
		SetBattleSettingDefault(ref saveData.DefaultSetting);
		Save();
	}

	public static string CreateUniqueID()
	{
		string text = string.Format("{0}{1}", SystemInfo.deviceUniqueIdentifier, (DateTime.Now.Ticks / 10000000).ToString());
		if (text.Length > 80)
		{
			text = text.Substring(text.Length - 80, 80);
		}
		PlayerPrefs.SetString(SAVE_IDENTIFY, text);
		return text;
	}

	private static string GetOnlyIdentify()
	{
		string text = PlayerPrefs.GetString(SAVE_IDENTIFY);
		if (string.IsNullOrEmpty(text))
		{
			text = CreateUniqueID();
		}
		return text;
	}

	public static void SetBattleSettingDefault(ref Setting setting)
	{
		setting.SoundVol = 1f;
		setting.BgmVol = 1f;
		setting.VseVol = 1f;
		setting.DmgVisible = 1;
		setting.HpVisible = 1;
		setting.JumpClassic = 0;
		setting.AutoCharge = 1;
		setting.AutoAim = 1;
		setting.AimLine = 1;
		setting.AimFirst = 1;
		setting.SlashClassic = 0;
		setting.ShootClassic = 0;
		setting.AimManual = 1;
		setting.DoubleTapThrough = 0;
		setting.DoubleTapDash = 1;
		setting.UITrans = 0f;
		setting.ButtonTip = 1;
	}

	public static void SetHometopSettingDefault(ref Setting setting)
	{
		setting = new Setting();
		int frameRate = 60;
		MonoBehaviourSingleton<OrangeGameManager>.Instance.ScreenRate = setting.ResolutionRate;
		MonoBehaviourSingleton<OrangeGameManager>.Instance.SetDesignContentScale();
		DeviceHelper.IsROGPhone(out frameRate);
		Application.targetFrameRate = frameRate;
		QualitySettings.vSyncCount = 1;
		if (!setting.vsync)
		{
			QualitySettings.vSyncCount = 0;
			Application.targetFrameRate = -1;
		}
	}

	public NetControllerSetting GetControllerSetting()
	{
		Setting setting = SaveData.Setting;
		return new NetControllerSetting
		{
			JumpClassic = (sbyte)setting.JumpClassic,
			AutoCharge = (sbyte)setting.AutoCharge,
			AutoAim = (sbyte)setting.AutoAim,
			AimFirst = (sbyte)setting.AimFirst,
			SlashClassic = (sbyte)setting.SlashClassic,
			ShootClassic = (sbyte)setting.ShootClassic,
			ManualAim = (sbyte)setting.AimManual,
			AimLine = (sbyte)setting.AimLine,
			DClickDash = (sbyte)setting.DoubleTapDash
		};
	}
}
