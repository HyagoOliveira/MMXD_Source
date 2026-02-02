using System;

public class NotificationManager : MonoBehaviourSingleton<NotificationManager>
{
	private bool hasNotify;

	private void Awake()
	{
		CleanNotification();
	}

	private void CleanNotification()
	{
		hasNotify = false;
	}

	public void NotificationMessage(string p_title, string p_msg, DateTime p_date)
	{
		hasNotify = true;
	}

	private void OnApplicationPause(bool paused)
	{
		if (paused)
		{
			if (!MonoBehaviourSingleton<OrangeGameManager>.Instance.IsLogin)
			{
				return;
			}
			DateTime now = DateTime.Now;
			SettingNotify settingNotify = MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.Setting.SettingNotify;
			if (settingNotify.AP)
			{
				int stamina = ManagedSingleton<PlayerHelper>.Instance.GetStamina();
				int num = (ManagedSingleton<PlayerHelper>.Instance.GetStaminaLimit() - stamina) * 60 * OrangeConst.AP_RECOVER_TIME;
				if (num > 0)
				{
					NotificationMessage(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("GAME_TITLE"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("MESSAGE_PUSH_AP"), now.AddSeconds(num));
				}
			}
			if (settingNotify.EP)
			{
				int eventStamina = ManagedSingleton<PlayerHelper>.Instance.GetEventStamina();
				int num2 = (OrangeConst.EP_MAX - eventStamina) * 60 * OrangeConst.EP_RECOVER_TIME;
				if (num2 > 0)
				{
					NotificationMessage(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("GAME_TITLE"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("MESSAGE_PUSH_EP"), now.AddSeconds(num2));
				}
			}
			if (!settingNotify.Research)
			{
				return;
			}
			{
				foreach (int item in ManagedSingleton<ResearchHelper>.Instance.GetListFinishTime())
				{
					NotificationMessage(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("GAME_TITLE"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("MESSAGE_PUSH_RESEARCH"), CapUtility.UnixTimeToDate(item).ToLocalTime());
				}
				return;
			}
		}
		if (!paused && hasNotify)
		{
			CleanNotification();
		}
	}
}
