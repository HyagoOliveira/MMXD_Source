#define RELEASE
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using enums;

public static class GuildUIHelper
{
	private const float LOADING_FADE_TIME = 0.3f;

	public static bool IsStateLockDebugMode;

	public static bool IsLoadingUIOpended { get; set; }

	public static bool IsStateLocked { get; private set; }

	public static event Action OnStateUnlockedEvent;

	public static bool CheckCameraFOV(ref float fov)
	{
		if (1920f / (float)MonoBehaviourSingleton<OrangeGameManager>.Instance.ScreenWidth * (float)MonoBehaviourSingleton<OrangeGameManager>.Instance.ScreenHeight < 1080f)
		{
			float num = fov * ((float)Math.PI / 180f);
			float num2 = 1.7777778f;
			float num3 = 2f * Mathf.Atan(Mathf.Tan(num / 2f) / num2);
			float num4 = (float)MonoBehaviourSingleton<OrangeGameManager>.Instance.ScreenWidth / (float)MonoBehaviourSingleton<OrangeGameManager>.Instance.ScreenHeight;
			float num5 = 2f * Mathf.Atan(Mathf.Tan(num3 / 2f) * num4);
			fov = num5 * 57.29578f;
			return true;
		}
		return false;
	}

	public static bool BackToHometop(OrangeUIBase ui)
	{
		bool isLoadingUIOpended = IsLoadingUIOpended;
		if (!isLoadingUIOpended)
		{
			OpenLoadingUI(delegate
			{
				Topbar componentInChildren = ui.GetComponentInChildren<Topbar>(true);
				if (componentInChildren != null)
				{
					componentInChildren.OnClickBackToHometop();
				}
			});
		}
		Debug.Log(string.Format("[{0}] UIName = {1}, CanBackToHometop = {2}", "BackToHometop", ui.name, isLoadingUIOpended));
		return isLoadingUIOpended;
	}

	public static void OpenLoadingUI(Action onFinished, float fadeInTime = 0.3f)
	{
		Debug.Log(string.Format("[{0}] Start, IsLoadingUIOpended = {1}", "OpenLoadingUI", IsLoadingUIOpended));
		if (!IsLoadingUIOpended)
		{
			MonoBehaviourSingleton<UIManager>.Instance.OpenLoadingUI(delegate
			{
				Debug.Log("[OpenLoadingUI] End");
				IsLoadingUIOpended = true;
				Action action2 = onFinished;
				if (action2 != null)
				{
					action2();
				}
			}, OrangeSceneManager.LoadingType.BLACK, fadeInTime);
		}
		else
		{
			Debug.Log("[OpenLoadingUI] End");
			Action action = onFinished;
			if (action != null)
			{
				action();
			}
		}
	}

	public static void CloseLoadingUI(Action onFinished, float fadeOutTime = 0.3f)
	{
		Debug.Log(string.Format("[{0}] Start, IsLoadingUIOpended = {1}", "CloseLoadingUI", IsLoadingUIOpended));
		if (IsLoadingUIOpended)
		{
			MonoBehaviourSingleton<UIManager>.Instance.CloseLoadingUI(delegate
			{
				Debug.Log("[CloseLoadingUI] End");
				IsLoadingUIOpended = false;
				Action action2 = onFinished;
				if (action2 != null)
				{
					action2();
				}
			}, fadeOutTime);
		}
		else
		{
			Debug.Log("[CloseLoadingUI] End");
			Action action = onFinished;
			if (action != null)
			{
				action();
			}
		}
	}

	public static void LockState(Action onFinished = null)
	{
		if (IsStateLocked)
		{
			if (onFinished != null)
			{
				onFinished();
			}
		}
		else
		{
			IsStateLocked = true;
			DoStateAction(onFinished);
		}
	}

	public static void UnlockState()
	{
		if (!IsStateLocked)
		{
			return;
		}
		if (GuildUIHelper.OnStateUnlockedEvent != null)
		{
			Action onStateUnlockedEvent = GuildUIHelper.OnStateUnlockedEvent;
			if (onStateUnlockedEvent != null)
			{
				onStateUnlockedEvent();
			}
			GuildUIHelper.OnStateUnlockedEvent = null;
		}
		IsStateLocked = false;
	}

	public static void ForceUnlockState()
	{
		Debug.LogError("ForceUnlockState");
		UnlockState();
		MonoBehaviourSingleton<UIManager>.Instance.Block(false);
		MonoBehaviourSingleton<UIManager>.Instance.Connecting(false);
	}

	public static void DoStateAction(Action action)
	{
		if (IsStateLockDebugMode)
		{
			MonoBehaviourSingleton<CoroutineHelper>.Instance.DelayAction(action, 3f);
		}
		else if (action != null)
		{
			action();
		}
	}

	private static IEnumerator DelayActionCoroutine(Action onFinished)
	{
		yield return new WaitForSeconds(3f);
		if (onFinished != null)
		{
			onFinished();
		}
	}

	public static void SetPlayerHUDData(string playerId, Text textName, Text textLevel, GameObject iconRoot, float iconScale = 0.7f)
	{
		SetPlayerHUDData(playerId, textName, textLevel, iconRoot.transform, iconScale);
	}

	public static void SetPlayerHUDData(string playerId, Text textName, Text textLevel, Transform iconRoot, float iconScale = 0.7f)
	{
		SocketPlayerHUD playerHUD;
		if (MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicHUD.TryGetValue(playerId, out playerHUD))
		{
			textName.text = playerHUD.m_Name;
			textLevel.text = string.Format("Lv{0}", playerHUD.m_Level);
			CommonAssetHelper.LoadPlayerIcon(iconRoot, delegate(PlayerIconBase playerIcon)
			{
				CommonUIHelper.SetPlayerIcon(playerIcon, playerHUD.m_IconNumber, iconScale);
			});
		}
		else
		{
			textName.text = "---";
			textLevel.text = "Lv-";
		}
	}

	public static void SetOnlineStatus(string playerId, OnlineStatusHelper onlineStatusController)
	{
		int value;
		if (!Singleton<GuildSystem>.Instance.PlayerBusyStatusCache.TryGetValue(playerId, out value))
		{
			value = 0;
		}
		onlineStatusController.SetOnlineStatus(value);
	}

	public static void SetGuildBadge(int badgeIndex, float badgeColor, ImageSpriteSwitcher badgeSpriteSwitcher, ImageColorController badgeColorController)
	{
		badgeSpriteSwitcher.ChangeImage(badgeIndex);
		badgeColorController.SetHSVColor(badgeColor);
	}

	public static bool SetCommunitySocketGuildInfo(string playerId, CommonGuildBadge guildBadge, Text guildName, GuildPrivilegeHelper guildPrivilege)
	{
		SocketGuildMemberInfo value;
		if (!MonoBehaviourSingleton<OrangeCommunityManager>.Instance.SocketGuildMemberInfoCache.TryGetValue(playerId, out value) || value.GuildId <= 0)
		{
			return false;
		}
		SocketGuildInfo value2;
		if (!MonoBehaviourSingleton<OrangeCommunityManager>.Instance.SocketGuildInfoCache.TryGetValue(value.GuildId, out value2))
		{
			Debug.LogError(string.Format("Failed to get GuildInfo {0} of Player {1}", value.GuildId, value.PlayerId));
			return false;
		}
		guildBadge.Setup(value2.GuildBadge, 0f);
		guildName.text = value2.GuildName;
		guildPrivilege.Setup((GuildPrivilege)value.GuildPrivilege);
		return true;
	}
}
