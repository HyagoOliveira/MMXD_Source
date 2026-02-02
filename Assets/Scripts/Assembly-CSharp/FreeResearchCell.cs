using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using enums;

public class FreeResearchCell : MonoBehaviour
{
	[SerializeField]
	private Button btnRetrieveItem;

	[SerializeField]
	private Image imgRetrieve;

	[SerializeField]
	private Text textButtonText;

	[SerializeField]
	private Image imgButtonJewel;

	[SerializeField]
	private Text textItemName;

	[SerializeField]
	private Text textItemTime;

	[SerializeField]
	private Image imgTime;

	[SerializeField]
	private ItemIconBase itemIcon;

	[SerializeField]
	private GameObject researchUI;

	private SystemSE ClickOKSE = SystemSE.CRI_SYSTEMSE_SYS_OK17;

	private int researchID;

	private bool payToRetrieve;

	public void Setup(int index, NetFreeResearchInfo info)
	{
		imgRetrieve.gameObject.SetActive(false);
		imgButtonJewel.gameObject.SetActive(false);
		btnRetrieveItem.interactable = true;
		payToRetrieve = false;
		researchID = index;
		if (!ManagedSingleton<OrangeDataManager>.Instance.RESEARCH_TABLE_DICT.ContainsKey(researchID))
		{
			return;
		}
		RESEARCH_TABLE rESEARCH_TABLE = ManagedSingleton<OrangeDataManager>.Instance.RESEARCH_TABLE_DICT[researchID];
		string text = "";
		Color color = new Color(26f / 85f, 1f, 0f);
		string text2 = "";
		Color color2 = Color.white;
		string bundlePath = string.Empty;
		string assetPath = string.Empty;
		int rare = 0;
		if (rESEARCH_TABLE.n_GET_AP != 0)
		{
			int serviceBonusValue = ManagedSingleton<ServiceHelper>.Instance.GetServiceBonusValue(ServiceType.ResearchAPIncrease);
			int num = Convert.ToInt32(Math.Ceiling((float)(rESEARCH_TABLE.n_GET_AP * (100 + serviceBonusValue)) / 100f));
			text = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("RESEARCH_FREE_AP"), num);
			assetPath = "icon_ITEM_000_002";
			bundlePath = AssetBundleScriptableObject.Instance.GetIconItem(assetPath);
			rare = 3;
		}
		else if (rESEARCH_TABLE.n_ITEMID != 0)
		{
			ITEM_TABLE item = null;
			if (ManagedSingleton<OrangeTableHelper>.Instance.GetItem(rESEARCH_TABLE.n_ITEMID, out item))
			{
				text = ManagedSingleton<OrangeTableHelper>.Instance.GetItemName(rESEARCH_TABLE.n_ITEMID);
				color = new Color(1f, 46f / 51f, 31f / 85f);
				NetRewardInfo netGachaRewardInfo = new NetRewardInfo
				{
					RewardType = 1,
					RewardID = rESEARCH_TABLE.n_ITEMID,
					Amount = rESEARCH_TABLE.n_ITEMCOUNT
				};
				MonoBehaviourSingleton<OrangeGameManager>.Instance.GetRewardSpritePath(netGachaRewardInfo, ref bundlePath, ref assetPath, ref rare);
			}
		}
		itemIcon.Setup(0, bundlePath, assetPath);
		itemIcon.SetRare(rare);
		if (rESEARCH_TABLE.n_GET_TIME != 0)
		{
			int num2 = rESEARCH_TABLE.n_GET_TIME / 100;
			int num3 = rESEARCH_TABLE.n_GET_TIME % 100;
			int num4 = MonoBehaviourSingleton<OrangeGameManager>.Instance.serverInfo.DailyResetInfo.PreResetTime + num2 * 3600;
			int num5 = MonoBehaviourSingleton<OrangeGameManager>.Instance.serverInfo.DailyResetInfo.PreResetTime + num3 * 3600;
			DateTime dateTime = CapUtility.UnixTimeToDate(num4).ToLocalTime();
			DateTime dateTime2 = CapUtility.UnixTimeToDate(num5).ToLocalTime();
			text2 = string.Format("{0:00}:{2:00}-{1:00}:{3:00}", dateTime.Hour, dateTime2.Hour, dateTime.Minute, dateTime2.Minute);
			if (MonoBehaviourSingleton<OrangeGameManager>.Instance.ServerUnixTimeNowUTC >= num4 && MonoBehaviourSingleton<OrangeGameManager>.Instance.ServerUnixTimeNowUTC < num5)
			{
				textButtonText.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("RESEARCH_FREE_RETRIEVE");
				color2 = new Color(26f / 85f, 1f, 0f);
				imgRetrieve.gameObject.SetActive(true);
			}
			else if (MonoBehaviourSingleton<OrangeGameManager>.Instance.ServerUnixTimeNowUTC > num5)
			{
				textButtonText.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("RESEARCH_FREE_PAYTORETRIEVE");
				imgButtonJewel.gameObject.SetActive(true);
				color2 = new Color(1f, 16f / 85f, 0f);
				payToRetrieve = true;
			}
			else if (MonoBehaviourSingleton<OrangeGameManager>.Instance.ServerUnixTimeNowUTC < num4)
			{
				textButtonText.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("RESEARCH_FREE_RESEARCHING");
				btnRetrieveItem.interactable = false;
			}
		}
		else
		{
			text2 = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("RESEARCH_FREE_ONCE"), rESEARCH_TABLE.n_LIMIT);
			textButtonText.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("RESEARCH_FREE_RETRIEVE");
			imgRetrieve.gameObject.SetActive(true);
			imgTime.gameObject.SetActive(false);
		}
		textItemName.text = text;
		textItemName.color = color;
		textItemTime.text = text2;
		textItemTime.color = color2;
		imgTime.color = color2;
		if (info != null && info.ResearchID == researchID && !MonoBehaviourSingleton<OrangeGameManager>.Instance.IsPassedResetDate(info.LastRetrieveTime, ResetRule.DailyReset))
		{
			textButtonText.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("RESEARCH_FREE_RETRIEVED");
			btnRetrieveItem.interactable = false;
			imgButtonJewel.gameObject.SetActive(false);
			imgRetrieve.gameObject.SetActive(false);
		}
	}

	public void OnClickRetrieveBtn()
	{
		if (payToRetrieve)
		{
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_CommonConsumeMsg", delegate(CommonConsumeMsgUI ui)
			{
				string itemName = ManagedSingleton<OrangeTableHelper>.Instance.GetItemName(OrangeConst.ITEMID_FREE_JEWEL);
				string desc = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("RESEARCH_FREE_PAYTORETRIEVE_CONFIRM"), itemName, OrangeConst.RESEARCH_RESIGN_JEWEL);
				ui.Setup(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_TIP"), desc, MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_OK"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_CANCEL"), ManagedSingleton<PlayerHelper>.Instance.GetTotalJewel(), OrangeConst.RESEARCH_RESIGN_JEWEL, delegate
				{
					if (ManagedSingleton<PlayerHelper>.Instance.GetTotalJewel() < OrangeConst.RESEARCH_RESIGN_JEWEL)
					{
						MonoBehaviourSingleton<OrangeGameManager>.Instance.ShowCommonMsg(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("DIAMOND_OUT"), delegate
						{
							MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_ShopTop", delegate(ShopTopUI shopUI)
							{
								shopUI.Setup(ShopTopUI.ShopSelectTab.directproduct);
							});
						}, null);
					}
					else
					{
						ClickOKSE = SystemSE.CRI_SYSTEMSE_SYS_STORE01;
						RetrieveFreeResearch();
					}
				});
			});
		}
		else
		{
			RetrieveFreeResearch();
		}
	}

	public void RetrieveFreeResearch()
	{
		Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.UI_RESEARCH_COMPLETE_VOICE, NAVI_MENU.CRI_NAVI_MENU_RICO_MENU21);
		if (!ManagedSingleton<OrangeDataManager>.Instance.RESEARCH_TABLE_DICT.ContainsKey(researchID))
		{
			return;
		}
		RESEARCH_TABLE rESEARCH_TABLE = ManagedSingleton<OrangeDataManager>.Instance.RESEARCH_TABLE_DICT[researchID];
		if (rESEARCH_TABLE.n_GET_AP > 0 && rESEARCH_TABLE.n_GET_AP + ManagedSingleton<PlayerHelper>.Instance.GetStamina() > OrangeConst.MAX_AP)
		{
			MonoBehaviourSingleton<OrangeGameManager>.Instance.ShowTipDialogByKey("APMAX_MESSAGE", 1f);
			return;
		}
		int originalAP = ManagedSingleton<PlayerHelper>.Instance.GetStamina();
		int originalEP = ManagedSingleton<PlayerHelper>.Instance.GetEventStamina();
		ManagedSingleton<PlayerNetManager>.Instance.RetrieveFreeResearch(researchID, delegate(List<NetRewardInfo> p_param)
		{
			SendMessageUpwards("UpdateHint", SendMessageOptions.DontRequireReceiver);
			textButtonText.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("RESEARCH_FREE_RETRIEVED");
			btnRetrieveItem.interactable = false;
			imgButtonJewel.gameObject.SetActive(false);
			imgRetrieve.gameObject.SetActive(false);
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(ClickOKSE);
			List<NetRewardInfo> rewardList = p_param;
			int aPCount = Math.Max(ManagedSingleton<PlayerHelper>.Instance.GetStamina() - originalAP, 0);
			int ePCount = Math.Max(ManagedSingleton<PlayerHelper>.Instance.GetEventStamina() - originalEP, 0);
			MonoBehaviourSingleton<OrangeGameManager>.Instance.AddAPEPToRewardList(ref rewardList, aPCount, ePCount);
			if (rewardList != null && rewardList.Count > 0)
			{
				MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_RewardPopup", delegate(RewardPopopUI ui)
				{
					ui.Setup(rewardList);
				});
			}
			SendMessageUpwards("UpdateNaviTalk", SendMessageOptions.DontRequireReceiver);
		});
	}
}
