#define RELEASE
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Better;
using CallbackDefs;
using CriWare;
using Newtonsoft.Json;
using OrangeAudio;
using UnityEngine;

public class AudioManager : MonoBehaviourSingleton<AudioManager>
{
	public enum ACBName_ID
	{
		BGM00 = 0,
		BGM01 = 1,
		BGM02 = 2,
		SYSTEM_SE = 3,
		SYSTEM_SE02 = 4,
		BATTLE_SE = 5,
		BATTLE_SE02 = 6,
		WEAPON_SE = 7,
		WEAPON_SE02 = 8,
		HIT_SE = 9,
		BOSS_SE = 10,
		BOSS_SE02 = 11,
		ENEMY_SE = 12,
		ENEMY_SE02 = 13,
		NAVI_TEXT = 14,
		NAVI_MENU = 15,
		STORY_AXL = 16
	}

	public enum EnemySE_ID
	{
		enemy_wheel_hit = 36,
		em009_wing = 43,
		em020_idle = 30,
		em020_exhaust = 31,
		em020_break = 32
	}

	public enum BossSE_ID
	{
		bs009_hitblockSE = 55,
		bs009_walk = 56,
		bs009_land = 57,
		bs009_drive = 58,
		bs009_jump = 59,
		bs009_turn = 60,
		bs009_claw = 61,
		bs009_shoot = 62,
		bs009_tail_loop = 63,
		bs009_tail_stop = 64,
		bs009_plasma_loop = 65,
		bs009_plasma_stop = 66,
		bs009_tailcharge = 67,
		bs009_slider = 68,
		bs010_hit_ground1 = 1,
		bs010_hit_ground2 = 2,
		bs010_appear = 3,
		bs019_land = 35,
		bs019_eye = 36,
		bs019_walk = 37,
		bs019_round = 38,
		bs019_roundStop = 39,
		bs031_jump = 40,
		bs031_land = 41,
		bs031_bullet = 42,
		bs031_bullet_hit = 43,
		bs031_switch = 44,
		bs031_blizzard = 45,
		bs031_blizzard_left = 46,
		bs031_blizzard_right = 47,
		bs031_ice_break = 48,
		bs031_ice_generate = 49,
		bs031_ice_finish = 50,
		bs031_slide_begin = 51,
		bs031_slide_loop = 52,
		bs031_slide_hit = 53,
		bs011_shoot = 116,
		bs011_sun_move = 117,
		bs011_sun_merge = 118,
		bs011_door_loop = 119,
		bs011_door_stop = 120,
		bs011_door_done = 121,
		bs011_door_press = 122,
		bs011_eye_open = 123,
		bs034_returnClaw = 203,
		bs034_land = 206,
		bs102_molb01_lp = 209,
		bs102_molb01_stop = 210,
		bs102_molb02 = 211,
		bs102_molb03 = 212,
		bs102_molb04_lp = 213,
		bs102_molb04_stop = 214,
		bs102_molb05 = 215
	}

	[Serializable]
	public class LoopInfoDic : UnitySerializedDictionary<string, LoopInfo>
	{
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass99_0
	{
		public AudioManager _003C_003E4__this;

		public string p_acbName;

		public Callback p_cb;

		public string awbPath;

		public string crcAcb;

		internal void _003CPreloadAtomSource_003Eg__PreloadAcbComplete_007C0(byte[] p_param0, string p_param1)
		{
			if (_003C_003E4__this.orangePool.ContainsKey(p_acbName))
			{
				p_cb.CheckTargetToInvoke();
				return;
			}
			if (p_param1 == null)
			{
				p_cb.CheckTargetToInvoke();
				return;
			}
			_003C_003E4__this.orangePool.Add(p_acbName, null);
			CriAtom.AddCueSheet(p_acbName, p_param1, awbPath);
			GameObject gameObject = new GameObject(p_acbName);
			gameObject.transform.SetParent(_003C_003E4__this.transform);
			CriAtomSource criAtomSource = null;
			criAtomSource = gameObject.AddComponent<CriAtomSource>();
			criAtomSource.use3dPositioning = false;
			criAtomSource.cueSheet = p_acbName.ToString();
			float vol = _003C_003E4__this.GetVol(_003C_003E4__this.GetChannel(p_acbName));
			criAtomSource.volume = vol;
			_003C_003E4__this.orangePool[p_acbName] = criAtomSource;
			p_cb.CheckTargetToInvoke();
		}

		internal void _003CPreloadAtomSource_003Eb__1(byte[] p_param0, string p_param1)
		{
			awbPath = p_param1;
			MonoBehaviourSingleton<OrangeWebRequestLoad>.Instance.Load(OrangeWebRequestLoad.LoadType.ACB, p_acbName, OrangeWebRequestLoad.LoadingFlg.SAVE_TO_LOCAL, _003CPreloadAtomSource_003Eg__PreloadAcbComplete_007C0, crcAcb);
		}
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass118_0
	{
		public string[] param;

		public AudioManager _003C_003E4__this;

		internal void _003CPlayHomeBgm_003Eg__playBGM_007C1()
		{
			_003C_003E4__this.PlayBGM(param[0], param[1]);
		}
	}

	[SerializeField]
	private DebugDevAudioText DebugDevAudioText;

	[SerializeField]
	public bool IsBossAppear;

	private CriWareInitializer criWareInitializer;

	public System.Collections.Generic.Dictionary<AudioChannelType, float> channelVolum = new System.Collections.Generic.Dictionary<AudioChannelType, float>
	{
		{
			AudioChannelType.BGM,
			1f
		},
		{
			AudioChannelType.Sound,
			1f
		},
		{
			AudioChannelType.Voice,
			1f
		}
	};

	private System.Collections.Generic.Dictionary<AudioChannelType, string> C2C = new System.Collections.Generic.Dictionary<AudioChannelType, string>
	{
		{
			AudioChannelType.BGM,
			"BGM"
		},
		{
			AudioChannelType.Sound,
			"SE"
		},
		{
			AudioChannelType.Voice,
			"VOICE"
		}
	};

	public const string AUDIO_FILE_INFO = "audiofileinfo";

	public const string BGM00 = "BGM00";

	public const string BGM01 = "BGM01";

	public const string BGM02 = "BGM02";

	public const string SYSTEM_SE = "SystemSE";

	public const string SYSTEM_SE02 = "SystemSE02";

	public const string BATTLE_SE = "BattleSE";

	public const string BATTLE_SE02 = "BattleSE02";

	public const string WEAPON_SE = "WeaponSE";

	public const string WEAPON_SE02 = "WeaponSE02";

	public const string HIT_SE = "HitSE";

	public const string BOSS_SE = "BossSE";

	public const string BOSS_SE02 = "BossSE02";

	public const string ENEMY_SE = "EnemySE";

	public const string ENEMY_SE02 = "EnemySE02";

	public const string NAVI_TEXT = "NAVI_TEXT";

	public const string NAVI_MENU = "NAVI_MENU";

	public const string STORY_AXL = "STORY_AXL";

	public const string BOSS_SE03 = "BossSE03";

	public const string BATTLE_SE03 = "BattleSE03";

	public const string WEAPON_SE03 = "WeaponSE03";

	public const string ENEMY_SE03 = "EnemySE03";

	public const string BOSS_SE04 = "BossSE04";

	public const string BOSS_SE05 = "BossSE05";

	public const string BOSS_SE06 = "BossSE06";

	private System.Collections.Generic.Dictionary<string, OrangeCrcInfo> CrcDict = new Better.Dictionary<string, OrangeCrcInfo>(StringComparer.OrdinalIgnoreCase);

	public string bgmSheet;

	public string bgmCue;

	public CriAtomExPlayback currentBGM;

	private GameObject OrangePool;

	private GameObject ExceptSEPool;

	private bool isActiveWindow = true;

	private const int MaxOCPNum = 100;

	[SerializeField]
	public List<OrangeCriPoint> OCPList = new List<OrangeCriPoint>();

	[SerializeField]
	private int _CurrentOCP;

	private int frameCnt;

	private System.Collections.Generic.Dictionary<string, CriAtomSource> extSEDic = new System.Collections.Generic.Dictionary<string, CriAtomSource>();

	public string[] m_preLoadAcb = new string[21]
	{
		"BGM00", "BGM01", "SystemSE02", "BattleSE", "BattleSE02", "WeaponSE", "WeaponSE02", "HitSE", "BossSE", "BossSE02",
		"EnemySE", "EnemySE02", "NAVI_TEXT", "NAVI_MENU", "STORY_AXL", "BossSE03", "WeaponSE03", "EnemySE03", "BossSE04", "BossSE05",
		"BossSE06"
	};

	private string beatSyncAcb;

	private string beatSyncCueName;

	private CriAtomExPlayback nullpb;

	public LoopInfoDic LoopDic = new LoopInfoDic();

	[SerializeField]
	private List<OrangeCriSource> SourceList = new List<OrangeCriSource>();

	[SerializeField]
	public bool IsInitAll { get; set; }

	[SerializeField]
	public bool IsInitSystemSE { get; set; }

	[SerializeField]
	public bool IsNowBGMPlaying
	{
		get
		{
			return currentBGM.status != CriAtomExPlayback.Status.Removed;
		}
	}

	public System.Collections.Generic.Dictionary<string, CriAtomSource> orangePool { get; private set; } = new System.Collections.Generic.Dictionary<string, CriAtomSource>(StringComparer.OrdinalIgnoreCase);


	private string srcAcfPath
	{
		get
		{
			return ManagedSingleton<ServerConfig>.Instance.PatchUrl + "CriWare/" + MonoBehaviourSingleton<OrangeWebRequestLoad>.Instance.Platform + "/Assets/StreamingAssets/ORANGE_SOUND.acf";
		}
	}

	private string srcConfigPath
	{
		get
		{
			return ManagedSingleton<ServerConfig>.Instance.PatchUrl + "CriWare/" + MonoBehaviourSingleton<OrangeWebRequestLoad>.Instance.Platform + "/Assets/StreamingAssets/audiofileinfo";
		}
	}

	private void Update()
	{
		if (!IsInitAll)
		{
			return;
		}
		frameCnt++;
		if (frameCnt < 2)
		{
			return;
		}
		frameCnt = 0;
		int i = 0;
		for (int count = OCPList.Count; i < count; i++)
		{
			if (OCPList[i].e_LoopType == OrangeCriPoint.LOOPTYPE.NONE)
			{
				switch (OCPList[i].Player.GetStatus())
				{
				case CriAtomExPlayer.Status.Playing:
					OCPList[i].Update();
					break;
				case CriAtomExPlayer.Status.Prep:
					OCPList[i].Update();
					break;
				default:
					OCPList[i].Reset();
					break;
				case CriAtomExPlayer.Status.Stop:
					break;
				}
			}
			else
			{
				OCPList[i].Update();
			}
		}
	}

	public void PlayExceptSE(string acb, string cue)
	{
		float num = 0f;
		if (GetAcb(acb, cue) != null)
		{
			CueInfo cueInfo = new CueInfo(acb, cue);
			string key = acb + "," + cueInfo.sCueKey;
			if (!extSEDic.ContainsKey(key))
			{
				GameObject obj = new GameObject(key);
				obj.transform.SetParent(ExceptSEPool.transform);
				CriAtomSource criAtomSource = obj.AddComponent<CriAtomSource>();
				criAtomSource.use3dPositioning = false;
				criAtomSource.cueSheet = acb;
				num = GetVol(GetChannel(acb));
				criAtomSource.volume = num;
				criAtomSource.Play(cue);
				extSEDic.Add(key, criAtomSource);
			}
			else if (cueInfo.eType == CueType.CT_STOP)
			{
				orangePool[cueInfo.sAcb].Play(cue);
				extSEDic[key].Stop();
			}
			else
			{
				extSEDic[key].Play(cue);
			}
		}
	}

	private void Awake()
	{
		IsInitAll = false;
		IsInitSystemSE = false;
	}

	private void OnEnable()
	{
		if (IsInitAll)
		{
			ResumeAll();
		}
		Singleton<GenericEventManager>.Instance.AttachEvent(EventManager.ID.PATCH_CHANGE, OnPatchChange);
	}

	private void OnDisable()
	{
		if (IsInitAll)
		{
			PauseAll();
		}
		Singleton<GenericEventManager>.Instance.DetachEvent(EventManager.ID.PATCH_CHANGE, OnPatchChange);
	}

	public void OnPatchChange()
	{
		LoadConfig(delegate
		{
			LoadAcf(null);
		});
	}

	public void OnApplicationFocus(bool focus)
	{
		Debug.Log("Focus Window : " + focus);
		isActiveWindow = focus;
		if (IsInitAll)
		{
			if (isActiveWindow)
			{
				ResetAllVolum();
				return;
			}
			SetVol(AudioChannelType.BGM, 0f);
			SetVol(AudioChannelType.Sound, 0f);
			SetVol(AudioChannelType.Voice, 0f);
		}
	}

	private void OnApplicationPause(bool pause)
	{
		if (IsInitAll)
		{
			if (pause)
			{
				PauseAll();
			}
			else
			{
				ResumeAll();
			}
		}
	}

	public void Init(Callback p_cb = null)
	{
		string[] acbs = new string[2] { "SystemSE", "BattleSE" };
		int[] chls = new int[2] { 2, 2 };
		Singleton<GenericEventManager>.Instance.AttachEvent<int, string, string>(EventManager.ID.STAGE_PLAYBGM, NotifyPlayBGM);
		Singleton<GenericEventManager>.Instance.AttachEvent<int, string, string>(EventManager.ID.STAGE_PLAYBGM_BEAT_SYNC, NotifyPlayBGM_BeatSync);
		orangePool.Clear();
		CrcDict.Clear();
		CriAtomPlugin.InitializeLibrary();
		CriWareInitializer[] array = UnityEngine.Object.FindObjectsOfType<CriWareInitializer>();
		criWareInitializer = array[0];
		CriAtomExBeatSync.OnCallback += BeatSyncCB;
		LoadConfig(delegate
		{
			LoadAcf(delegate
			{
				OrangePool = new GameObject("OrangePool");
				OrangePool.transform.SetParent(base.transform);
				ExceptSEPool = new GameObject("ExceptSEPool");
				ExceptSEPool.transform.SetParent(base.transform);
				while (OCPList.Count() < 100)
				{
					OrangeCriPoint item = new OrangeCriPoint();
					OCPList.Add(item);
				}
				MonoBehaviourSingleton<AudioManager>.Instance.OnStartPreloadAtomSource(acbs, chls, delegate
				{
					IsInitSystemSE = true;
					p_cb.CheckTargetToInvoke();
				}, true, true);
			});
		});
		Singleton<GenericEventManager>.Instance.AttachEvent(EventManager.ID.SD_HOME_BGM, PlayHomeBgm);
	}

	private void LoadAcf(Callback p_cb)
	{
		MonoBehaviourSingleton<OrangeWebRequestLoad>.Instance.Load(OrangeWebRequestLoad.LoadType.UNIQUE, srcAcfPath, OrangeWebRequestLoad.LoadingFlg.ACF, delegate(byte[] objs, string path2)
		{
			CriAtomEx.RegisterAcf(null, path2);
			p_cb.CheckTargetToInvoke();
		});
	}

	private void LoadConfig(Callback p_cb)
	{
		MonoBehaviourSingleton<OrangeWebRequestLoad>.Instance.Load(OrangeWebRequestLoad.LoadType.UNIQUE, srcConfigPath, OrangeWebRequestLoad.LoadingFlg.TEXT_DEFAULT, delegate(byte[] config, string path)
		{
			CrcDict.Clear();
			foreach (OrangeCrcInfo item in JsonConvert.DeserializeObject<List<OrangeCrcInfo>>(AesCrypto.Decode(Encoding.UTF8.GetString(config))))
			{
				CrcDict.Add(item.Name, item);
			}
			p_cb.CheckTargetToInvoke();
		});
	}

	public void ResetAllVolum()
	{
		Setting setting = MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.Setting;
		if (isActiveWindow)
		{
			SetVol(AudioChannelType.BGM, setting.BgmVol);
			SetVol(AudioChannelType.Sound, setting.SoundVol);
			SetVol(AudioChannelType.Voice, setting.VseVol);
		}
		else
		{
			SetVol(AudioChannelType.BGM, 0f);
			SetVol(AudioChannelType.Sound, 0f);
			SetVol(AudioChannelType.Voice, 0f);
		}
	}

	public void PreloadInitAudio()
	{
		PreloadAtomSource(m_preLoadAcb, new int[23]
		{
			1, 1, 2, 2, 2, 2, 2, 2, 2, 2,
			2, 2, 3, 3, 3, 3, 2, 2, 2, 2,
			2, 2, 2
		}, delegate
		{
			IsInitAll = true;
			ResetAllVolum();
		}, true);
	}

	public int GetChannel(string acb)
	{
		int result = 2;
		if (acb.Contains("BGM"))
		{
			result = 1;
		}
		else if (acb.Contains("VOICE") || acb.Contains("NAVI") || acb.Contains("STORY"))
		{
			result = 3;
		}
		return result;
	}

	public CriAtomExPlayback Play(string s_acb, int n_cueId)
	{
		if (s_acb == null || s_acb == "" || s_acb == "null")
		{
			return nullpb;
		}
		if (!CrcDict.ContainsKey(s_acb))
		{
			return nullpb;
		}
		if (!orangePool.ContainsKey(s_acb))
		{
			PreloadAtomSource(s_acb, delegate
			{
				if (orangePool.ContainsKey(s_acb))
				{
					PlayOnly(s_acb, n_cueId);
				}
			});
			return nullpb;
		}
		return PlayOnly(s_acb, n_cueId);
	}

	public CriAtomExPlayback Play(string s_acb, string cueName)
	{
		if (s_acb == null || s_acb == "" || s_acb == "null")
		{
			return nullpb;
		}
		if (!CrcDict.ContainsKey(s_acb))
		{
			return nullpb;
		}
		if (!orangePool.ContainsKey(s_acb))
		{
			PreloadAtomSource(s_acb, delegate
			{
				if (orangePool.ContainsKey(s_acb))
				{
					PlayOnly(s_acb, cueName);
				}
			});
			return nullpb;
		}
		return PlayOnly(s_acb, cueName);
	}

	private CriAtomExPlayback PlayOnly(string s_acb, int n_cueId)
	{
		CriAtomExAcb acb = CriAtom.GetAcb(s_acb);
		CriAtomEx.CueInfo info = default(CriAtomEx.CueInfo);
		if (!acb.GetCueInfo(n_cueId, out info))
		{
			return nullpb;
		}
		float vol = GetVol(GetChannel(s_acb));
		if (vol <= 0f)
		{
			return nullpb;
		}
		CriAtomSource criAtomSource = orangePool[s_acb];
		criAtomSource.volume = vol;
		return criAtomSource.Play(n_cueId);
	}

	private CriAtomExPlayback PlayOnly(string s_acb, string cueName)
	{
		CriAtomExAcb acb = CriAtom.GetAcb(s_acb);
		CriAtomEx.CueInfo info = default(CriAtomEx.CueInfo);
		if (!acb.GetCueInfo(cueName, out info))
		{
			return nullpb;
		}
		float vol = GetVol(GetChannel(s_acb));
		if (vol < 0f)
		{
			return nullpb;
		}
		CriAtomSource criAtomSource = orangePool[s_acb];
		criAtomSource.volume = vol;
		return criAtomSource.Play(cueName);
	}

	public CriAtomExPlayback PlaySE(Transform trans, string acb, string cue, float maxdis = 0f)
	{
		OrangeCriPoint aPoint = GetAPoint();
		if (aPoint != null)
		{
			if (maxdis == 0f)
			{
				maxdis = 13f;
			}
			return aPoint.Play(maxdis, trans, acb, cue);
		}
		return nullpb;
	}

	public void PlayBGM(string s_acb, string cueName, bool stopBGM = true)
	{
		if (!CrcDict.ContainsKey(s_acb))
		{
			return;
		}
		switch (cueName)
		{
		case "":
			return;
		case "null":
			return;
		}
		if (bgmCue == cueName)
		{
			return;
		}
		if (CriAtom.GetAcb(s_acb) != null)
		{
			if (stopBGM && IsNowBGMPlaying)
			{
				currentBGM.Stop();
			}
			if (!stopBGM)
			{
				beatSyncAcb = s_acb;
				beatSyncCueName = cueName;
				return;
			}
			orangePool[s_acb].cueName = cueName;
			bgmSheet = s_acb;
			bgmCue = cueName;
			float vol = GetVol(1);
			orangePool[s_acb].volume = vol;
			currentBGM = orangePool[s_acb].Play(cueName);
		}
		else
		{
			string tempAcb = s_acb;
			string tempCueName = cueName;
			PreloadAtomSource(s_acb, 1, delegate
			{
				PlayBGM(tempAcb, tempCueName);
			});
		}
	}

	private void BeatSyncCB(ref CriAtomExBeatSync.Info info)
	{
		if (!string.IsNullOrEmpty(beatSyncAcb) && !string.IsNullOrEmpty(beatSyncAcb) && info.beatCount != 0)
		{
			PlayBGM(beatSyncAcb, beatSyncCueName);
			beatSyncAcb = string.Empty;
			beatSyncCueName = string.Empty;
		}
	}

	public void PlayBGM(string s_acb, int n_cueId)
	{
		if (!CrcDict.ContainsKey(s_acb))
		{
			return;
		}
		CriAtomExAcb acb = CriAtom.GetAcb(s_acb);
		if (acb != null)
		{
			CriAtomEx.CueInfo info = default(CriAtomEx.CueInfo);
			if (acb.GetCueInfo(n_cueId, out info) && !(bgmCue == info.name))
			{
				if (IsNowBGMPlaying)
				{
					currentBGM.Stop();
				}
				orangePool[s_acb].cueName = info.name;
				bgmSheet = s_acb;
				bgmCue = info.name;
				float vol = GetVol(1);
				orangePool[s_acb].volume = vol;
				currentBGM = orangePool[s_acb].Play(info.name);
			}
		}
		else
		{
			string tempAcb = s_acb;
			int tempCueName = n_cueId;
			PreloadAtomSource(s_acb, 1, delegate
			{
				PlayBGM(tempAcb, tempCueName);
			});
		}
	}

	public void PlayBattleSE(BattleSE cueId)
	{
		Play("BattleSE", (int)cueId);
	}

	public void SetPan3dAngle(float ang)
	{
	}

	public void PlaySystemSE(SystemSE cueId)
	{
		Play("SystemSE", (int)cueId);
	}

	public void PlaySystemSE(string acb, SystemSE02 cueId)
	{
		Play(acb, (int)cueId);
	}

	public void PlaySystemSE(SystemSE02 cueId)
	{
		Play("SystemSE02", (int)cueId);
	}

	public void PlaySE_AfterCallback(string s_acb, int cueId, Callback p_cb, bool lockInput = false)
	{
		EmptyBlockUI blockUI = null;
		if (lockInput)
		{
			blockUI = MonoBehaviourSingleton<UIManager>.Instance.LoadResourceUISync<EmptyBlockUI>("UI_EmptyBlock", true);
			blockUI.SetBlock(true);
		}
		Play(s_acb, cueId);
		CriAtomExAcb acb = CriAtom.GetAcb(s_acb);
		CriAtomEx.CueInfo info = default(CriAtomEx.CueInfo);
		if (acb.GetCueInfo(cueId, out info))
		{
			LeanTween.delayedCall((float)info.length * 0.001f, (Action)delegate
			{
				if ((bool)blockUI)
				{
					blockUI.OnClickCloseBtn();
					blockUI = null;
				}
				p_cb.CheckTargetToInvoke();
			});
		}
		else
		{
			if ((bool)blockUI)
			{
				blockUI.OnClickCloseBtn();
				blockUI = null;
			}
			p_cb.CheckTargetToInvoke();
		}
	}

	public void PlaySystemSE(string cueName)
	{
		Play("SystemSE", cueName);
	}

	public void PreloadAtomSource(string[] p_acbNames, int[] p_channels, Callback p_cb = null, bool checkSize = false, bool updateProgress = true)
	{
		long totalFileSize = 0L;
		for (int i = 0; i < p_acbNames.Length; i++)
		{
			OrangeCrcInfo value = null;
			if (CrcDict.TryGetValue(p_acbNames[i], out value) && MonoBehaviourSingleton<OrangeWebRequestLoad>.Instance.IsFileNeedToDownload(OrangeWebRequestLoad.LoadType.ACB, p_acbNames[i], value.Crc))
			{
				totalFileSize += value.Size;
			}
		}
		if (checkSize && totalFileSize > 0)
		{
			totalFileSize /= 1048576L;
			MonoBehaviourSingleton<UIManager>.Instance.LoadResourceUI("UI_CommonMsg", delegate(CommonUI ui)
			{
				string text = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("NETWORK_DOWNLOAD_DESC_1"), totalFileSize.ToString("F2"));
				if (totalFileSize > 50)
				{
					text = string.Format("{0}\n{1}", text, MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("NETWORK_DOWNLOAD_DESC_2"));
				}
				ui.MuteSE = true;
				ui.SetupConfirm(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("NETWORK_DOWNLOAD_TITLE"), text, MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_OK"), delegate
				{
					MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK17);
					if (!MonoBehaviourSingleton<UIManager>.Instance.IsLoading)
					{
						MonoBehaviourSingleton<UIManager>.Instance.OpenLoadingUI(delegate
						{
							OnStartPreloadAtomSource(p_acbNames, p_channels, p_cb, true, updateProgress);
						}, OrangeSceneManager.LoadingType.FULL);
					}
					else
					{
						OnStartPreloadAtomSource(p_acbNames, p_channels, p_cb, false, updateProgress);
					}
				});
			}, true);
		}
		else
		{
			OnStartPreloadAtomSource(p_acbNames, p_channels, p_cb, false, updateProgress);
		}
	}

	private void OnStartPreloadAtomSource(string[] p_acbNames, int[] p_channels, Callback p_cb, bool closeLoading, bool updateProgress)
	{
		int now = 0;
		int max = p_acbNames.Length;
		for (int i = 0; i < max; i++)
		{
			PreloadAtomSource(p_acbNames[i], p_channels[i], delegate
			{
				now++;
				if (now >= max)
				{
					if (updateProgress)
					{
						Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.UPDATE_LOADING_PROGRESS, 1f);
					}
					if (closeLoading)
					{
						MonoBehaviourSingleton<UIManager>.Instance.CloseLoadingUI(delegate
						{
							p_cb.CheckTargetToInvoke();
						});
					}
					else
					{
						p_cb.CheckTargetToInvoke();
					}
				}
				else if (updateProgress)
				{
					Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.UPDATE_LOADING_PROGRESS, (float)now / (float)max);
				}
			});
		}
	}

	public long GetFileSize(string p_acbName, bool checkIsLoaded = true)
	{
		OrangeCrcInfo value = null;
		if (CrcDict.TryGetValue(p_acbName, out value))
		{
			if (checkIsLoaded)
			{
				if (MonoBehaviourSingleton<OrangeWebRequestLoad>.Instance.IsFileNeedToDownload(OrangeWebRequestLoad.LoadType.ACB, p_acbName, value.Crc))
				{
					return value.Size;
				}
				return 0L;
			}
			return value.Size;
		}
		return 0L;
	}

	public void PreloadAtomSource(string p_acbName, Callback p_cb = null)
	{
		int channel = GetChannel(p_acbName);
		PreloadAtomSource(p_acbName, channel, p_cb);
	}

	public void PreloadAtomSource(string p_acbName, int p_channel, Callback p_cb = null)
	{
		_003C_003Ec__DisplayClass99_0 CS_0024_003C_003E8__locals0 = new _003C_003Ec__DisplayClass99_0();
		CS_0024_003C_003E8__locals0._003C_003E4__this = this;
		CS_0024_003C_003E8__locals0.p_acbName = p_acbName;
		CS_0024_003C_003E8__locals0.p_cb = p_cb;
		if (string.IsNullOrEmpty(CS_0024_003C_003E8__locals0.p_acbName))
		{
			Debug.LogWarning("acbName error => " + CS_0024_003C_003E8__locals0.p_acbName);
			CS_0024_003C_003E8__locals0.p_cb.CheckTargetToInvoke();
			return;
		}
		if (IsAtomSourceExist(CS_0024_003C_003E8__locals0.p_acbName))
		{
			CS_0024_003C_003E8__locals0.p_cb.CheckTargetToInvoke();
			return;
		}
		OrangeCrcInfo value = null;
		OrangeCrcInfo value2 = null;
		CS_0024_003C_003E8__locals0.crcAcb = string.Empty;
		string serverCrc = string.Empty;
		CS_0024_003C_003E8__locals0.awbPath = string.Empty;
		if (CrcDict.TryGetValue(CS_0024_003C_003E8__locals0.p_acbName, out value))
		{
			CS_0024_003C_003E8__locals0.crcAcb = value.Crc;
			if (CrcDict.TryGetValue(CS_0024_003C_003E8__locals0.p_acbName + ".awb", out value2))
			{
				serverCrc = value2.Crc;
			}
			if (value2 != null)
			{
				MonoBehaviourSingleton<OrangeWebRequestLoad>.Instance.Load(OrangeWebRequestLoad.LoadType.AWB, CS_0024_003C_003E8__locals0.p_acbName + ".awb", OrangeWebRequestLoad.LoadingFlg.SAVE_TO_LOCAL, delegate(byte[] p_param0, string p_param1)
				{
					CS_0024_003C_003E8__locals0.awbPath = p_param1;
					MonoBehaviourSingleton<OrangeWebRequestLoad>.Instance.Load(OrangeWebRequestLoad.LoadType.ACB, CS_0024_003C_003E8__locals0.p_acbName, OrangeWebRequestLoad.LoadingFlg.SAVE_TO_LOCAL, CS_0024_003C_003E8__locals0._003CPreloadAtomSource_003Eg__PreloadAcbComplete_007C0, CS_0024_003C_003E8__locals0.crcAcb);
				}, serverCrc);
			}
			else
			{
				CS_0024_003C_003E8__locals0.awbPath = string.Empty;
				MonoBehaviourSingleton<OrangeWebRequestLoad>.Instance.Load(OrangeWebRequestLoad.LoadType.ACB, CS_0024_003C_003E8__locals0.p_acbName, OrangeWebRequestLoad.LoadingFlg.SAVE_TO_LOCAL, CS_0024_003C_003E8__locals0._003CPreloadAtomSource_003Eg__PreloadAcbComplete_007C0, CS_0024_003C_003E8__locals0.crcAcb);
			}
		}
		else
		{
			Debug.LogWarningFormat("[CriAudioManager] PreloadAtomSource '{0}' CRC Not Exist.", CS_0024_003C_003E8__locals0.p_acbName);
			CS_0024_003C_003E8__locals0.p_cb.CheckTargetToInvoke();
		}
	}

	public bool IsAtomSourceExist(string p_acbName)
	{
		CriAtomSource value;
		orangePool.TryGetValue(p_acbName, out value);
		return value != null;
	}

	public float GetVol(int p_channelId)
	{
		Setting setting = MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.Setting;
		if (isActiveWindow)
		{
			switch (p_channelId)
			{
			default:
				return setting.BgmVol;
			case 2:
				return setting.SoundVol;
			case 3:
				return setting.VseVol;
			}
		}
		return 0f;
	}

	public void SetVol(AudioChannelType audioChannel, float vol)
	{
		string ch = C2C[audioChannel];
		if (ch == "VOICE")
		{
			(from p in orangePool
				where p.Key.Contains("VOICE") || p.Key.Contains("NAVI") || p.Key.Contains("STORY")
				select p into a
				select a.Value).ToList().ForEach(delegate(CriAtomSource item)
			{
				item.volume = vol;
			});
		}
		else
		{
			(from p in orangePool
				where p.Key.Contains(ch)
				select p into a
				select a.Value).ToList().ForEach(delegate(CriAtomSource item)
			{
				item.volume = vol;
			});
		}
		channelVolum[audioChannel] = vol;
	}

	public void StopBGM()
	{
		bgmSheet = string.Empty;
		bgmCue = string.Empty;
		currentBGM.Stop();
	}

	public void StopAllExceptSE()
	{
		foreach (KeyValuePair<string, CriAtomSource> item in extSEDic)
		{
			item.Value.Stop();
			UnityEngine.Object.DestroyImmediate(item.Value.gameObject);
		}
		extSEDic.Clear();
	}

	private void StopAllSE()
	{
		foreach (OrangeCriSource source in SourceList)
		{
			source.StopAll();
		}
		foreach (OrangeCriPoint oCP in OCPList)
		{
			if (oCP.Player.GetStatus() != 0)
			{
				oCP.Reset();
			}
		}
		LoopDic.Clear();
		foreach (KeyValuePair<string, CriAtomSource> item in orangePool)
		{
			if (item.Key.Contains("SE"))
			{
				item.Value.Stop();
			}
		}
	}

	private void StopAllVoice()
	{
		foreach (KeyValuePair<string, CriAtomSource> item in orangePool)
		{
			if (!item.Key.Contains("SE") && !item.Key.Contains("BGM"))
			{
				item.Value.Stop();
			}
		}
	}

	public void Stop(AudioChannelType audioChannelType)
	{
		switch (audioChannelType)
		{
		case AudioChannelType.BGM:
			StopBGM();
			break;
		case AudioChannelType.Sound:
			StopAllSE();
			break;
		case AudioChannelType.Voice:
			StopAllVoice();
			break;
		}
	}

	public void Stop(string s_acb)
	{
		foreach (KeyValuePair<string, CriAtomSource> item in orangePool)
		{
			if (item.Key.Contains(s_acb))
			{
				item.Value.Stop();
			}
		}
	}

	private void PauseBGM(bool sw)
	{
		currentBGM.Pause(sw);
	}

	private void PauseSE(bool sw)
	{
		(from p in orangePool
			where !p.Key.StartsWith("System") && p.Key.Contains("SE")
			select p into a
			select a.Value).ToList().ForEach(delegate(CriAtomSource source)
		{
			source.Pause(sw);
		});
		foreach (KeyValuePair<string, CriAtomSource> item in extSEDic)
		{
			item.Value.Pause(sw);
		}
	}

	private void PauseVOICE(bool sw)
	{
		(from p in orangePool
			where p.Key.Contains("VOICE")
			select p into a
			select a.Value).ToList().ForEach(delegate(CriAtomSource source)
		{
			source.Pause(sw);
		});
	}

	public void Pause(AudioChannelType audioChannelType)
	{
		switch (audioChannelType)
		{
		case AudioChannelType.BGM:
			PauseBGM(true);
			break;
		case AudioChannelType.Sound:
			PauseSE(true);
			break;
		case AudioChannelType.Voice:
			PauseVOICE(true);
			break;
		}
	}

	public void PauseAll()
	{
		PauseBGM(true);
		PauseSE(true);
		PauseVOICE(true);
	}

	public void Resume(AudioChannelType audioChannelType)
	{
		switch (audioChannelType)
		{
		case AudioChannelType.BGM:
			PauseBGM(false);
			break;
		case AudioChannelType.Sound:
			PauseSE(false);
			break;
		case AudioChannelType.Voice:
			PauseVOICE(false);
			break;
		}
	}

	public void ResumeAll()
	{
		PauseBGM(false);
		PauseSE(false);
		PauseVOICE(false);
	}

	public void NotifyPlayBGM(int channel, string sound, string cue)
	{
		if (sound == "0" || sound == "null")
		{
			return;
		}
		if (sound.Contains("BGM"))
		{
			PreloadAtomSource(sound, 1, delegate
			{
				PlayBGM(sound, cue);
			});
		}
		else
		{
			PreloadAtomSource(sound, 1, delegate
			{
				Play(sound, cue);
			});
		}
	}

	public void NotifyPlayBGM_BeatSync(int channel, string sound, string cue)
	{
		if (sound == "0" || sound == "null")
		{
			return;
		}
		if (sound.Contains("BGM"))
		{
			PreloadAtomSource(sound, 1, delegate
			{
				Debug.Log("[NotifyPlayBGM_BeatSync] BGM Preloaded.");
				PlayBGM(sound, cue, false);
			});
		}
		else
		{
			PreloadAtomSource(sound, 1, delegate
			{
				Play(sound, cue);
			});
		}
	}

	private void PlayHomeBgm()
	{
		EVENT_TABLE[] array = ManagedSingleton<OrangeDataManager>.Instance.EVENT_TABLE_DICT.Values.Where((EVENT_TABLE x) => x.n_TYPE == 14).ToArray();
		if (array != null)
		{
			long serverUnixTimeNowUTC = MonoBehaviourSingleton<OrangeGameManager>.Instance.ServerUnixTimeNowUTC;
			foreach (EVENT_TABLE eVENT_TABLE in array)
			{
				if (!ManagedSingleton<OrangeTableHelper>.Instance.IsNullOrEmpty(eVENT_TABLE.s_IMG2) && ManagedSingleton<OrangeTableHelper>.Instance.IsOpeningDate(eVENT_TABLE.s_BEGIN_TIME, eVENT_TABLE.s_END_TIME, serverUnixTimeNowUTC))
				{
					_003C_003Ec__DisplayClass118_0 _003C_003Ec__DisplayClass118_ = new _003C_003Ec__DisplayClass118_0();
					_003C_003Ec__DisplayClass118_._003C_003E4__this = this;
					_003C_003Ec__DisplayClass118_.param = eVENT_TABLE.s_IMG2.Split(',');
					if (_003C_003Ec__DisplayClass118_.param.Length >= 2)
					{
						PreloadAtomSource(_003C_003Ec__DisplayClass118_.param[0], 1, _003C_003Ec__DisplayClass118_._003CPlayHomeBgm_003Eg__playBGM_007C1);
						return;
					}
				}
			}
		}
		PlayBGM("BGM01", 4);
	}

	public static string FormatEnum2Name(string enumName)
	{
		string text = "";
		string[] array = enumName.Split('_');
		int num = 1;
		if (array[1] == "VOICE" || array[1] == "NAVI" || array[1] == "SKILLSE" || array[1] == "CHARASE")
		{
			num = 2;
		}
		for (int i = 0; i < array.Length; i++)
		{
			if (i > num)
			{
				text = ((i >= array.Length - 1) ? (text + array[i]) : (text + array[i] + "_"));
			}
		}
		return text.ToLower();
	}

	public CriAtomSource GetAtomSource(string s_acb)
	{
		if (orangePool.ContainsKey(s_acb))
		{
			return orangePool[s_acb];
		}
		return null;
	}

	public CriAtomExAcb GetAcb(string s_acb, string cuename)
	{
		if (string.IsNullOrEmpty(s_acb) || string.IsNullOrEmpty(cuename))
		{
			return null;
		}
		CriAtomSource value;
		if (!orangePool.TryGetValue(s_acb, out value))
		{
			return null;
		}
		return CriAtom.GetAcb(s_acb);
	}

	public OrangeCriPoint GetAPoint()
	{
		int i = 0;
		OrangeCriPoint orangeCriPoint = null;
		int count = OCPList.Count;
		if (_CurrentOCP >= count)
		{
			_CurrentOCP = 0;
		}
		for (; i < count; i++)
		{
			if (orangeCriPoint != null)
			{
				break;
			}
			if (OCPList[_CurrentOCP].playerpb.status == CriAtomExPlayback.Status.Removed && OCPList[_CurrentOCP].sourceObj == null)
			{
				orangeCriPoint = OCPList[_CurrentOCP];
			}
			_CurrentOCP++;
			if (_CurrentOCP >= count)
			{
				_CurrentOCP = 0;
			}
		}
		return orangeCriPoint;
	}

	public OrangeCriPoint RegLoop(OrangeCriSource obj, CueInfo info)
	{
		OrangeCriPoint orangeCriPoint = null;
		string sHalfKey = info.sHalfKey;
		if (!LoopDic.ContainsKey(sHalfKey))
		{
			LoopDic.Add(sHalfKey, new LoopInfo());
		}
		if (LoopDic[sHalfKey].point == null)
		{
			if (obj.f_vol > 0f)
			{
				orangeCriPoint = GetAPoint();
				LoopDic[sHalfKey].point = orangeCriPoint;
				LoopDic[sHalfKey].listSource.Insert(0, obj);
				return orangeCriPoint;
			}
			LoopDic[sHalfKey].listSource.Add(obj);
			return null;
		}
		LoopDic[sHalfKey].listSource.Add(obj);
		if (LoopDic[sHalfKey].point.sourceObj != null && LoopDic[sHalfKey].point.sourceObj.f_vol < obj.f_vol)
		{
			LoopDic[sHalfKey].point.sourceObj = obj;
		}
		return null;
	}

	public bool UnRegLoop(OrangeCriSource obj, CueInfo info)
	{
		string sHalfKey = info.sHalfKey;
		int num = -1;
		LoopInfo value;
		if (!LoopDic.TryGetValue(sHalfKey, out value))
		{
			return false;
		}
		if (value.point == null)
		{
			num = value.listSource.FindIndex((OrangeCriSource a) => a == obj);
			if (num == -1)
			{
				return true;
			}
			value.listSource.RemoveAt(num);
			return false;
		}
		if (value.listSource.Count == 0)
		{
			if (LoopDic[sHalfKey].point != null)
			{
				LoopDic[sHalfKey].point.Reset();
			}
			LoopDic[sHalfKey].point = null;
			LoopDic.Remove(sHalfKey);
			return true;
		}
		float num2 = 99f;
		OrangeCriSource orangeCriSource = null;
		foreach (OrangeCriSource item in value.listSource.FindAll((OrangeCriSource a) => a == obj))
		{
			if (item.f_vol <= num2)
			{
				num2 = item.f_vol;
				orangeCriSource = item;
			}
		}
		if (orangeCriSource != null)
		{
			LoopDic[sHalfKey].listSource.Remove(orangeCriSource);
			if (LoopDic[sHalfKey].listSource.Count == 0)
			{
				value.point.Reset();
				value.point = null;
				LoopDic.Remove(sHalfKey);
				return true;
			}
			int num3 = -1;
			float num4 = 0f;
			for (int i = 0; i < LoopDic[sHalfKey].listSource.Count; i++)
			{
				if (LoopDic[sHalfKey].listSource[i].f_vol > num4)
				{
					num3 = i;
					num4 = LoopDic[sHalfKey].listSource[i].f_vol;
				}
			}
			if (num3 == -1)
			{
				value.point.Reset();
				value.point = null;
				return true;
			}
			value.point.sourceObj = LoopDic[sHalfKey].listSource[num3];
			return false;
		}
		Debug.LogWarning("流程錯誤 Can't find stopse : " + sHalfKey);
		return true;
	}

	public void ReqPlay(OrangeCriSource obj)
	{
		CueInfo cueInfo = new CueInfo();
		foreach (string item in obj.seDic)
		{
			string[] array = item.Split(',');
			cueInfo.Parse(array[0], array[1]);
			LoopInfo value;
			if (LoopDic.TryGetValue(cueInfo.sHalfKey, out value) && value.point == null)
			{
				if (value.listSource.FindIndex((OrangeCriSource a) => a == obj) == -1)
				{
					Debug.LogWarning("ReqPlay can't find OBJ: " + cueInfo.sFullKey);
					break;
				}
				OrangeCriPoint orangeCriPoint = (value.point = GetAPoint());
				CriAtomExAcb acb = GetAcb(cueInfo.sAcb, cueInfo.sCueName);
				orangeCriPoint.CueInfo = cueInfo;
				orangeCriPoint.e_LoopType = OrangeCriPoint.LOOPTYPE.SE_LOOP;
				orangeCriPoint.sourceObj = obj;
				orangeCriPoint.Player.SetCue(acb, cueInfo.sCueName);
				orangeCriPoint.Play(cueInfo.sAcb, cueInfo.sCueName);
				break;
			}
		}
	}

	public void ReqStop(OrangeCriSource obj)
	{
		CueInfo cueInfo = new CueInfo();
		foreach (string item in obj.seDic)
		{
			string[] array = item.Split(',');
			cueInfo.Parse(array[0], array[1]);
			LoopInfo value;
			if (!LoopDic.TryGetValue(cueInfo.sHalfKey, out value) || value.point == null)
			{
				continue;
			}
			int num = -1;
			float num2 = 0f;
			int num3 = -1;
			for (int i = 0; i < value.listSource.Count; i++)
			{
				if (num == -1 && value.point.sourceObj == value.listSource[i])
				{
					num = i;
				}
				if (num3 == -1 && value.listSource[i] != value.point.sourceObj && num2 < value.listSource[i].f_vol)
				{
					num2 = value.listSource[i].f_vol;
					num3 = i;
				}
			}
			if (num == -1)
			{
				Debug.LogWarning("Can't find now source : " + cueInfo.sFullKey);
			}
			if (value.point.sourceObj == obj)
			{
				if (num3 == -1)
				{
					value.point.Reset();
					value.point = null;
					break;
				}
				value.point.sourceObj = value.listSource[num3];
			}
		}
	}

	public void PauseLoop(CueInfo mCueInfo, bool sw)
	{
		string key = mCueInfo.sAcb + "," + mCueInfo.sCueKey;
		LoopInfo value;
		if (LoopDic.TryGetValue(key, out value) && value.point != null)
		{
			value.point.Pause = sw;
		}
	}

	public void ClearBattleLoopList()
	{
		foreach (OrangeCriSource source in SourceList)
		{
			source.StopAll();
		}
		LoopDic.Clear();
		foreach (OrangeCriPoint oCP in OCPList)
		{
			oCP.Reset();
		}
	}

	public void RegisterSource(OrangeCriSource ocs)
	{
		if (!(SourceList.Find((OrangeCriSource p) => p.gameObject.GetInstanceID() == ocs.gameObject.GetInstanceID()) != null))
		{
			SourceList.Add(ocs);
		}
	}

	public void UnRegisterSource(OrangeCriSource ocs)
	{
		OrangeCriSource orangeCriSource = SourceList.Find((OrangeCriSource p) => p.gameObject.GetInstanceID() == ocs.gameObject.GetInstanceID());
		if (orangeCriSource != null)
		{
			SourceList.Remove(orangeCriSource);
		}
	}
}
