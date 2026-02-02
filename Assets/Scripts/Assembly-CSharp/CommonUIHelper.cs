using CallbackDefs;
using UnityEngine;

public static class CommonUIHelper
{
	public static void SetPlayerIcon(PlayerIconBase playerIcon, int iconNumber, float scale = 1f)
	{
		playerIcon.Setup(iconNumber);
		playerIcon.transform.localScale = new Vector3(scale, scale, scale);
	}

    [System.Obsolete]
    public static void SetCommonIcon(CommonIconBase commonIcon, int rewardId, int rewardCount, CallbackIdx onClick = null)
	{
		SetCommonIcon(commonIcon, rewardId, rewardCount, rewardCount, onClick);
	}

    [System.Obsolete]
    public static void SetCommonIcon(CommonIconBase commonIcon, int rewardId, int rewardCountMin, int rewardCountMax, CallbackIdx onClick = null)
	{
		ITEM_TABLE item;
		if (ManagedSingleton<OrangeTableHelper>.Instance.GetItem(rewardId, out item))
		{
			if (item.n_TYPE == 5 && item.n_TYPE_X == 1 && (int)item.f_VALUE_Y > 0)
			{
				commonIcon.SetItemWithAmountForCard(rewardId, rewardCountMin, rewardCountMax, onClick);
			}
			else
			{
				commonIcon.SetItemWithAmount(rewardId, rewardCountMin, rewardCountMax, onClick);
			}
		}
	}

	public static void ShowCommonTipUI(string tips, bool isLocalizationKey = true, Callback callback = null)
	{
		MonoBehaviourSingleton<UIManager>.Instance.LoadResourceUI("UI_CommonMsg", delegate(CommonUI ui)
		{
			ui.OpenSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP;
			ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
			ui.SetupConfirm(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_TIP"), isLocalizationKey ? MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr(tips) : tips, MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_OK"), callback);
		});
	}
}
