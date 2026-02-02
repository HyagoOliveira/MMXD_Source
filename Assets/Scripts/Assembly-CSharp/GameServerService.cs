#define RELEASE
using System;
using System.Text;
using Newtonsoft.Json.Linq;
using UnityEngine.Networking;
using enums;

public class GameServerService : OrangeServerService<GameServerService>
{
	private string playerIdentify = "LocalPlayer";

	private int serviceZoneID;

	public string PlayerIdentify
	{
		get
		{
			return playerIdentify;
		}
		set
		{
			Debug.Log(string.Format("PlayerIdentify is {0}.", value));
			playerIdentify = value;
		}
	}

	public int ServiceZoneID
	{
		get
		{
			return serviceZoneID;
		}
		set
		{
			Debug.Log(string.Format("Target ServiceZoneID is {0}.", value));
			serviceZoneID = value;
		}
	}

	public bool DayChange { get; set; }

	public new void Awake()
	{
		base.Awake();
	}

	protected override bool WWWRequestError(UnityWebRequest www, RequestCommand cmd)
	{
		if (base.WWWRequestError(www, cmd))
		{
			byte[] data = www.downloadHandler.data;
			int num = Convert.ToInt32(JObject.Parse(Encoding.ASCII.GetString(data))["_d"]);
			if (MonoBehaviourSingleton<OrangeGameManager>.Instance.IsPassedResetDate(num, ResetRule.DailyReset))
			{
				DayChanged(true);
			}
		}
		return true;
	}

	protected override void ParseServerResponse(RequestCommand cmd, IResponse res)
	{
		try
		{
			if (cmd.callbackEvent != null)
			{
				cmd.callbackEvent(res);
			}
		}
		catch (Exception ex)
		{
			throw ex;
		}
		OrangeGameResponse orangeGameResponse = res as OrangeGameResponse;
		if (orangeGameResponse == null || orangeGameResponse.ServerTime == 0)
		{
			MonoBehaviourSingleton<OrangeGameManager>.Instance.ShowErrorMsg(Code.SYSTEM_INVALID_VERSION);
			return;
		}
		Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.UPDATE_TOPBAR_DATA);
		ManagedSingleton<PlayerNetManager>.Instance.SetMissionProgress(orangeGameResponse);
		if (ManagedSingleton<ServerConfig>.Instance.PatchVer != orangeGameResponse.PatchVer || MonoBehaviourSingleton<OrangeGameManager>.Instance.GetServerDataCRC() != orangeGameResponse.DataCRC || MonoBehaviourSingleton<OrangeGameManager>.Instance.GetServerExDataCRC() != orangeGameResponse.ExDataCRC)
		{
			ManagedSingleton<ServerConfig>.Instance.PatchVer = orangeGameResponse.PatchVer;
			MonoBehaviourSingleton<UIManager>.Instance.LoadResourceUI("UI_CommonMsg", delegate(CommonUI ui)
			{
				ui.SetupConfirmByKey("COMMON_TIP", "SYSTEM_PATCHVER_CHANGED", "COMMON_OK", delegate
				{
					Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.PATCH_CHANGE);
					OrangeDataReader.Instance.DeleteTableAll();
					MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_HOME);
					MonoBehaviourSingleton<UIManager>.Instance.CloseAllUI(delegate
					{
						MonoBehaviourSingleton<OrangeSceneManager>.Instance.ChangeScene("switch", OrangeSceneManager.LoadingType.DEFAULT, null, false);
					});
				});
			}, true);
		}
		else if (MonoBehaviourSingleton<OrangeGameManager>.Instance.IsPassedResetDate(orangeGameResponse.ServerTime, ResetRule.DailyReset))
		{
			DayChanged();
		}
	}

	private void DayChanged(bool isForceToHomeTop = false)
	{
		MonoBehaviourSingleton<OrangeGameManager>.Instance.LoginToGameService(ManagedSingleton<ServerConfig>.Instance.GetPreviousSelectedServerUrl(), delegate
		{
			ManagedSingleton<MissionHelper>.Instance.ResetMonthlyActivityCounterID();
			DayChange = true;
			if (isForceToHomeTop)
			{
				if (MonoBehaviourSingleton<UIManager>.Instance.IsLoading)
				{
					MonoBehaviourSingleton<UIManager>.Instance.CloseLoadingUI(null);
				}
				MonoBehaviourSingleton<UIManager>.Instance.BackToHometop();
			}
			else
			{
				Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.CHANGE_DAY);
			}
		});
	}

	public void ClearCommand()
	{
		queRequest.Clear();
	}
}
