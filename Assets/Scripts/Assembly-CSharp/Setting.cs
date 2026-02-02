using Newtonsoft.Json;
using OrangeAudio;
using OrangeSocket;
using UnityEngine;

public class Setting
{
	[JsonProperty("a")]
	public float ResolutionRate = 0.75f;

	[JsonProperty("b")]
	public float BgmVol = 1f;

	[JsonProperty("c")]
	public float SoundVol = 1f;

	[JsonProperty("d")]
	public float VseVol = 1f;

	[JsonProperty("e")]
	public float UITrans;

	[JsonProperty("f")]
	public int DmgVisible = 1;

	[JsonProperty("g")]
	public int HpVisible = 1;

	[JsonProperty("h")]
	public int JumpClassic;

	[JsonProperty("i")]
	public int AutoCharge = 1;

	[JsonProperty("j")]
	public int AutoAim = 1;

	[JsonProperty("k")]
	public int AimLine = 1;

	[JsonProperty("l")]
	public int AimFirst = 1;

	[JsonProperty("m")]
	public int AimManual = 1;

	[JsonProperty("n")]
	public int DoubleTapThrough;

	[JsonProperty("o")]
	public int DoubleTapDash = 1;

	[JsonProperty("p")]
	public int ButtonTip = 1;

	[JsonProperty("q")]
	public bool sw;

	[JsonProperty("r")]
	public bool vsync = true;

	[JsonProperty("s")]
	public bool fullscreen;

	[JsonProperty("t")]
	public int scrEffect = 1;

	[JsonProperty("u")]
	public int SlashClassic;

	[JsonProperty("v")]
	public int ShootClassic;

	[JsonProperty("w")]
	public int FrameRate = 60;

	[JsonProperty("x")]
	public bool Community = true;

	[JsonProperty("y")]
	public bool IsNoSleep = true;

	[JsonProperty("z")]
	public SettingNotify SettingNotify = new SettingNotify();

	[JsonProperty("windowmoderesolution")]
	public Vector2 WindowModeResolution = new Vector2(1280f, 720f);

	[JsonProperty("friendpvphostid")]
	public string FriendPVPHostID = string.Empty;

	[JsonProperty("bosschallengepage")]
	public int BossChallengePage;

	public Setting(bool ModelSetup = false)
	{
		if (ModelSetup)
		{
			JumpClassic = 0;
			AutoCharge = 1;
			AutoAim = 1;
			AimFirst = 1;
			SlashClassic = 0;
			ShootClassic = 0;
			AimManual = 1;
			AimLine = 1;
			DoubleTapDash = 1;
			return;
		}
		ResolutionRate = 0.75f;
		BgmVol = 1f;
		SoundVol = 1f;
		VseVol = 1f;
		UITrans = 0f;
		DmgVisible = 1;
		HpVisible = 1;
		JumpClassic = 0;
		AutoCharge = 1;
		AutoAim = 1;
		AimLine = 1;
		AimFirst = 1;
		AimManual = 1;
		DoubleTapThrough = 0;
		DoubleTapDash = 1;
		SlashClassic = 0;
		ShootClassic = 0;
		sw = false;
		DeviceHelper.IsROGPhone(out FrameRate);
		Community = true;
		IsNoSleep = true;
		scrEffect = 1;
		ButtonTip = 1;
		vsync = true;
		fullscreen = false;
		SettingNotify = new SettingNotify();
		WindowModeResolution = new Vector2(1280f, 720f);
	}

	public void SetToDevice()
	{
		MonoBehaviourSingleton<OrangeGameManager>.Instance.ScreenRate = ResolutionRate;
		MonoBehaviourSingleton<OrangeGameManager>.Instance.SetDesignContentScale();
		Application.targetFrameRate = FrameRate;
		QualitySettings.vSyncCount = 1;
		if (!MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.Setting.vsync)
		{
			QualitySettings.vSyncCount = 0;
			Application.targetFrameRate = -1;
		}
		MonoBehaviourSingleton<AudioManager>.Instance.SetVol(AudioChannelType.BGM, BgmVol);
		MonoBehaviourSingleton<AudioManager>.Instance.SetVol(AudioChannelType.Sound, SoundVol);
		MonoBehaviourSingleton<AudioManager>.Instance.SetVol(AudioChannelType.Voice, VseVol);
		MonoBehaviourSingleton<CCSocketClient>.Instance.SendProtocol(FlatBufferCCHelper.CreateRQChatSetShield((!Community) ? 1 : 0));
		Screen.sleepTimeout = (IsNoSleep ? (-2) : (-1));
	}
}
