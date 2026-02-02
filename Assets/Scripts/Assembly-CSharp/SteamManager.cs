#define RELEASE
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Steamworks;
using Steamworks.Data;
using enums;

public class SteamManager : MonoBehaviourSingleton<SteamManager>
{
	[CompilerGenerated]
	private readonly uint _003CAppID_003Ek__BackingField = 1582620u;

	private bool _initialized;

	private Dictionary<SteamAchievementStageCondition, SteamAchievement> dicStageAchievement = new Dictionary<SteamAchievementStageCondition, SteamAchievement>
	{
		{
			SteamAchievementStageCondition.StagePassed_01_6,
			SteamAchievement.ST_ACHIEVEMENT_02
		},
		{
			SteamAchievementStageCondition.StagePassed_02_6,
			SteamAchievement.ST_ACHIEVEMENT_03
		},
		{
			SteamAchievementStageCondition.StagePassed_03_6,
			SteamAchievement.ST_ACHIEVEMENT_04
		},
		{
			SteamAchievementStageCondition.StagePassed_04_6,
			SteamAchievement.ST_ACHIEVEMENT_05
		},
		{
			SteamAchievementStageCondition.StagePassed_05_6,
			SteamAchievement.ST_ACHIEVEMENT_06
		},
		{
			SteamAchievementStageCondition.StagePassed_06_6,
			SteamAchievement.ST_ACHIEVEMENT_07
		},
		{
			SteamAchievementStageCondition.StagePassed_07_6,
			SteamAchievement.ST_ACHIEVEMENT_08
		},
		{
			SteamAchievementStageCondition.StagePassed_08_6,
			SteamAchievement.ST_ACHIEVEMENT_09
		},
		{
			SteamAchievementStageCondition.StagePassed_09_6,
			SteamAchievement.ST_ACHIEVEMENT_10
		},
		{
			SteamAchievementStageCondition.StagePassed_10_6,
			SteamAchievement.ST_ACHIEVEMENT_11
		},
		{
			SteamAchievementStageCondition.StagePassed_11_6,
			SteamAchievement.ST_ACHIEVEMENT_12
		},
		{
			SteamAchievementStageCondition.StagePassed_12_6,
			SteamAchievement.ST_ACHIEVEMENT_13
		},
		{
			SteamAchievementStageCondition.StagePassed_13_6,
			SteamAchievement.ST_ACHIEVEMENT_14
		},
		{
			SteamAchievementStageCondition.StagePassed_14_6,
			SteamAchievement.ST_ACHIEVEMENT_15
		},
		{
			SteamAchievementStageCondition.StagePassed_15_6,
			SteamAchievement.ST_ACHIEVEMENT_16
		}
	};

	public uint AppID
	{
		[CompilerGenerated]
		get
		{
			return _003CAppID_003Ek__BackingField;
		}
	}

	public bool SteamConnected { get; private set; }

	public bool TrustedAccount
	{
		get
		{
			if (!SteamClient.IsValid || !SteamClient.IsLoggedOn)
			{
				return false;
			}
			return !SteamApps.IsVACBanned;
		}
	}

	public bool Startup()
	{
		if (_initialized)
		{
			return false;
		}
		try
		{
			Debug.Log(string.Format("SteamManager AppID={0}!", AppID));
			SteamClient.Init(AppID);
			Debug.Log(string.Format("SteamClient AppId={0}!", SteamClient.AppId));
			Debug.Log("SteamClient User=" + SteamClient.Name + "!");
			Debug.Log(string.Format("SteamClient SteamId={0}!", SteamClient.SteamId));
			Debug.Log(string.Format("SteamClient Level={0}!", SteamUser.SteamLevel));
			Debug.Log("SteamClient Language=" + SteamApps.GameLanguage + "!");
			Debug.Log("SteamClient Country=" + SteamUtils.IpCountry + "!");
			Debug.Log("SteamClient Currency=" + SteamInventory.Currency + "!");
			Debug.Log("SteamClient Ticket=" + GetUserTicket() + "!");
			SteamUser.OnMicroTxnAuthorizationResponse += MonoBehaviourSingleton<OrangeIAP>.Instance.OnMicroTxnAuthorizationResponse;
			SteamUserStats.OnAchievementProgress += OnAchievementChanged;
			SteamUser.OnSteamServersConnected += OnSteamServersConnected;
			SteamUser.OnSteamServersDisconnected += OnSteamServersDisconnected;
			SteamConnected = true;
			_initialized = true;
		}
		catch (Exception ex)
		{
			Debug.Log("SteamClient Init fail => " + ex.Message + " stacks => " + ex.StackTrace);
			return false;
		}
		return true;
	}

	public string GetUserSteamID()
	{
		if (!SteamClient.IsValid || !SteamClient.IsLoggedOn || !SteamClient.SteamId.IsValid)
		{
			Debug.LogError("SteamClient init failed or not loggedOn yet.");
			return "Error SteamID";
		}
		return SteamClient.SteamId.ToString();
	}

	public string GetUserTicket()
	{
		if (!SteamClient.IsValid || !SteamClient.IsLoggedOn || !SteamClient.SteamId.IsValid)
		{
			Debug.LogError("SteamClient init failed or not loggedOn yet.");
			return string.Empty;
		}
		return BitConverter.ToString(SteamUser.GetAuthSessionTicket().Data).Replace("-", string.Empty);
	}

	public void StatOperation(SteamStatCounter counter, StatOperate op, int val = 0)
	{
		StatOperation(counter.ToString(), op, val);
	}

	public void StatOperation(string counter, StatOperate op, int val = 0)
	{
		Stat stat = new Stat(counter);
		switch (op)
		{
		case StatOperate.ADD:
			stat.Add(val);
			break;
		case StatOperate.SET:
			stat.Set(val);
			break;
		case StatOperate.GET:
			Debug.Log(string.Format("StatName[{0}] Val[{1}]", counter, stat.GetInt()));
			break;
		}
	}

	public void AchievedAchievement(SteamAchievement achievement)
	{
		AchievedAchievement(achievement.ToString());
	}

	public void AchievedAchievement(string achievement)
	{
		Achievement achievement2 = new Achievement(achievement);
		if (!achievement2.State)
		{
			achievement2.Trigger();
			Debug.Log(achievement + " Achieved!!");
		}
		else
		{
			Debug.Log(achievement + " already Achieved!!");
		}
	}

	private void OnAchievementChanged(Achievement ach, int currentProgress, int progress)
	{
		Debug.Log(ach.Name + " => " + ach.Description);
		if (ach.State)
		{
			Debug.Log(ach.Name + " WAS UNLOCKED!");
		}
	}

	private void OnGameOverlayActivated(bool activated)
	{
		Debug.Log(string.Format("OnGameOverlayActivated => {0}", activated));
	}

	private void OnSteamServersConnected()
	{
		Debug.Log("OnSteamServersConnected");
		SteamConnected = true;
	}

	private void OnSteamServersDisconnected()
	{
		Debug.Log("OnSteamServersDisconnected");
		SteamConnected = false;
	}

	public void Update()
	{
		if (SteamClient.IsValid && SteamClient.IsLoggedOn)
		{
			SteamClient.RunCallbacks();
		}
	}

	private void OnDisable()
	{
		Debug.Log("OnDisable SteamClient Shutdown");
		if (!SteamClient.IsLoggedOn)
		{
			return;
		}
		try
		{
			if (SteamClient.IsLoggedOn)
			{
				SteamClient.Shutdown();
			}
		}
		catch (Exception ex)
		{
			Debug.Log("SteamManager OnDisable cause exception = " + ex.Message + ". ");
		}
	}

	private void OnApplicationQuit()
	{
		Debug.Log("OnApplicationQuit SteamClient Shutdown");
		try
		{
			if (SteamClient.IsLoggedOn)
			{
				SteamClient.Shutdown();
			}
		}
		catch (Exception ex)
		{
			Debug.Log("SteamManager OnApplicationQuit cause exception = " + ex.Message + ". ");
		}
	}

	public void TriggerStagePassed(int stageID, int star)
	{
		foreach (KeyValuePair<SteamAchievementStageCondition, SteamAchievement> item in dicStageAchievement)
		{
			if (Convert.ToInt32(item.Key) == stageID)
			{
				AchievedAchievement(item.Value);
			}
		}
		STAGE_TABLE value;
		if (!ManagedSingleton<OrangeDataManager>.Instance.STAGE_TABLE_DICT.TryGetValue(stageID, out value))
		{
			return;
		}
		if (value.n_TYPE == 5)
		{
			AchievedAchievement(SteamAchievement.ST_ACHIEVEMENT_17);
		}
		if (value.n_TYPE == 7 && value.n_DIFFICULTY >= 2)
		{
			int starAmount = ManagedSingleton<StageHelper>.Instance.GetStarAmount(star);
			if (starAmount >= 2)
			{
				AchievedAchievement(SteamAchievement.ST_ACHIEVEMENT_22);
			}
			if (starAmount >= 3)
			{
				AchievedAchievement(SteamAchievement.ST_ACHIEVEMENT_23);
			}
		}
	}

	public void TriggerDeepElementGot(int count)
	{
		StatOperation(SteamStatCounter.DEEP_ELEMENT_COUNT, StatOperate.SET, count);
	}
}
