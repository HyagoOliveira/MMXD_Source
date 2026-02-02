using System.Collections.Generic;
using CallbackDefs;
using OrangeApi;
using UnityEngine;
using UnityEngine.UI;
using enums;

public class ChargeStaminaUI : OrangeUIBase
{
	[SerializeField]
	private Text textTitle;

	[SerializeField]
	private Text textRemainCount;

	[SerializeField]
	private Text textOwnedCount;

	[SerializeField]
	private ItemIconWithAmount jewelItemIcon;

	[SerializeField]
	private ItemIconWithAmount staminaItemIcon;

	[SerializeField]
	private Button btnBuy;

	[SerializeField]
	private Image imgBuyCover;

	private ChargeType selectedType;

	private Callback callback;

	private int storedStageID;

	private int storedCostItemID;

	private int storedRewardAmount;

	private bool popupGotoDialogWhenClickBuy;

	private bool isPlayOneSE = true;

	private Transform objLawBtn;

	private void OnEnable()
	{
		Singleton<CrusadeSystem>.Instance.OnAddChallengeCountEvent += OnAddCrusadeChallengeCountEvent;
	}

	private void OnDisable()
	{
		Singleton<CrusadeSystem>.Instance.OnAddChallengeCountEvent -= OnAddCrusadeChallengeCountEvent;
	}

	private void OnAddCrusadeChallengeCountEvent(NetChargeInfo info)
	{
		Setup(ChargeType.CrusadeChallenge);
	}

	public void Reset()
	{
		btnBuy.interactable = true;
		imgBuyCover.gameObject.SetActive(false);
		popupGotoDialogWhenClickBuy = false;
		storedStageID = 0;
		storedCostItemID = 0;
		storedRewardAmount = 0;
	}

	public void CustomSetup(ChargeType type, int costAmount, int rewardAmount, int costItemID, int rewardItemID, string title, string desc, string desc1, Callback cb = null)
	{
		Reset();
		selectedType = type;
		storedCostItemID = costItemID;
		callback = cb;
		textTitle.text = title;
		textRemainCount.text = desc;
		textOwnedCount.text = desc1;
		ITEM_TABLE item = null;
		if (ManagedSingleton<OrangeTableHelper>.Instance.GetItem(costItemID, out item))
		{
			jewelItemIcon.Setup(0, AssetBundleScriptableObject.Instance.GetIconItem(item.s_ICON), item.s_ICON);
			jewelItemIcon.SetRare(item.n_RARE);
			jewelItemIcon.SetAmount(costAmount);
		}
		if (ManagedSingleton<OrangeTableHelper>.Instance.GetItem(rewardItemID, out item))
		{
			staminaItemIcon.Setup(0, AssetBundleScriptableObject.Instance.GetIconItem(item.s_ICON), item.s_ICON);
			staminaItemIcon.SetRare(item.n_RARE);
			staminaItemIcon.SetAmount(rewardAmount);
		}
		if (costItemID == OrangeConst.ITEMID_FREE_JEWEL)
		{
			if (costAmount > ManagedSingleton<PlayerHelper>.Instance.GetTotalJewel())
			{
				popupGotoDialogWhenClickBuy = true;
			}
		}
		else if (costAmount > ManagedSingleton<PlayerHelper>.Instance.GetItemValue(costItemID))
		{
			if (costItemID == OrangeConst.ITEMID_JEWEL)
			{
				popupGotoDialogWhenClickBuy = true;
			}
			else
			{
				btnBuy.interactable = false;
				imgBuyCover.gameObject.SetActive(true);
			}
		}
		LoadLawBtn();
		base._EscapeEvent = EscapeEvent.CLOSE_UI;
	}

	public void Setup(ChargeType type, int stageID = 0)
	{
		Reset();
		selectedType = type;
		storedStageID = stageID;
		storedCostItemID = OrangeConst.ITEMID_FREE_JEWEL;
		int usedCount = GetUsedCount(type, stageID);
		int cost = 0;
		int reward = 0;
		int num = ManagedSingleton<PlayerHelper>.Instance.GetItemValue(storedCostItemID);
		if (storedCostItemID == OrangeConst.ITEMID_FREE_JEWEL)
		{
			num = ManagedSingleton<PlayerHelper>.Instance.GetTotalJewel();
		}
		CaculateNextBuyStep(usedCount, type, out cost, out reward);
		if (ManagedSingleton<PlayerHelper>.Instance.GetTotalJewel() < cost)
		{
			popupGotoDialogWhenClickBuy = true;
		}
		storedRewardAmount = reward;
		int p_itemId = 0;
		int num2 = 0;
		int num3 = 0;
		string p_key = string.Empty;
		switch (type)
		{
		case ChargeType.ActionPoint:
			p_itemId = OrangeConst.ITEMID_AP;
			p_key = "CHARGE_STAMINA_TITLE";
			num2 = int.MaxValue;
			num3 = ManagedSingleton<PlayerHelper>.Instance.GetStamina();
			break;
		case ChargeType.Gold:
			p_itemId = OrangeConst.ITEMID_MONEY;
			p_key = "CHARGE_MONEY_TITLE";
			num2 = ManagedSingleton<OrangeTableHelper>.Instance.GetItemMax(OrangeConst.ITEMID_MONEY);
			num3 = ManagedSingleton<PlayerHelper>.Instance.GetItemValue(OrangeConst.ITEMID_MONEY);
			break;
		case ChargeType.EventActionPoint:
			p_itemId = OrangeConst.ITEMID_EVENTAP;
			p_key = "CHARGE_EVENTAP_TITLE";
			num2 = int.MaxValue;
			num3 = ManagedSingleton<PlayerHelper>.Instance.GetEventStamina();
			break;
		case ChargeType.BossChallenge:
			p_itemId = OrangeConst.ITEMID_CHALLENGE_COUNT;
			p_key = "CHARGE_CHALLENGE_TITLE";
			num2 = int.MaxValue;
			num3 = 0;
			break;
		case ChargeType.RaidBossChallenge:
			p_itemId = OrangeConst.ITEMID_CHALLENGE_COUNT;
			p_key = "CHARGE_CHALLENGE_TITLE";
			num2 = int.MaxValue;
			num3 = 0;
			break;
		case ChargeType.CrusadeChallenge:
			p_itemId = OrangeConst.ITEMID_CHALLENGE_COUNT;
			p_key = "CHARGE_CHALLENGE_TITLE";
			num2 = int.MaxValue;
			num3 = 0;
			break;
		}
		textTitle.text = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr(p_key));
		ITEM_TABLE item = null;
		if (ManagedSingleton<OrangeTableHelper>.Instance.GetItem(p_itemId, out item))
		{
			jewelItemIcon.SetAmount(cost);
			staminaItemIcon.Setup(0, AssetBundleScriptableObject.Instance.GetIconItem(item.s_ICON), item.s_ICON);
			staminaItemIcon.SetRare(item.n_RARE);
			staminaItemIcon.SetAmount(reward);
		}
		int totalValidCount = GetTotalValidCount(selectedType);
		textRemainCount.text = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("CHARGE_STAMINA_DESC"), totalValidCount - usedCount, totalValidCount);
		textOwnedCount.text = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("CHARGE_ITEM_DESC"), ManagedSingleton<OrangeTableHelper>.Instance.GetItemName(storedCostItemID), num);
		if (totalValidCount - usedCount <= 0 || num3 + reward > num2)
		{
			btnBuy.interactable = false;
			imgBuyCover.gameObject.SetActive(true);
		}
		if (isPlayOneSE)
		{
			isPlayOneSE = false;
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
		}
		base.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
		LoadLawBtn();
		base._EscapeEvent = EscapeEvent.CLOSE_UI;
	}

	private int GetUsedCount(ChargeType type, int stageID)
	{
		int result = 0;
		if (type != ChargeType.BossChallenge)
		{
			if (ManagedSingleton<PlayerNetManager>.Instance.dicCharge.ContainsKey((int)selectedType))
			{
				NetChargeInfo netChargeInfo = ManagedSingleton<PlayerNetManager>.Instance.dicCharge[(int)selectedType].netChargeInfo;
				if (netChargeInfo != null && !MonoBehaviourSingleton<OrangeGameManager>.Instance.IsPassedResetDate(netChargeInfo.LastChargeTime, ResetRule.DailyReset))
				{
					result = netChargeInfo.Count;
				}
			}
		}
		else
		{
			result = ManagedSingleton<StageHelper>.Instance.GetExtraResetCount(stageID);
		}
		return result;
	}

	public void OnClickBuy()
	{
		if (popupGotoDialogWhenClickBuy)
		{
			if (storedCostItemID != OrangeConst.ITEMID_FREE_JEWEL && storedCostItemID != OrangeConst.ITEMID_JEWEL)
			{
				return;
			}
			MonoBehaviourSingleton<OrangeGameManager>.Instance.ShowCommonMsg(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("DIAMOND_OUT"), delegate
			{
				ShopTopUI uI2 = MonoBehaviourSingleton<UIManager>.Instance.GetUI<ShopTopUI>("UI_ShopTop");
				if (uI2 != null)
				{
					OnClickCloseBtn();
					uI2.GoShop(ShopTopUI.ShopSelectTab.directproduct);
				}
				else
				{
					MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_ShopTop", delegate(ShopTopUI ui)
					{
						OnClickCloseBtn();
						ui.Setup(ShopTopUI.ShopSelectTab.directproduct);
					});
				}
			}, null);
			return;
		}
		switch (selectedType)
		{
		case ChargeType.ActionPoint:
		case ChargeType.Gold:
		case ChargeType.EventActionPoint:
			if (selectedType == ChargeType.ActionPoint && storedRewardAmount + ManagedSingleton<PlayerHelper>.Instance.GetStamina() > OrangeConst.MAX_AP)
			{
				MonoBehaviourSingleton<OrangeGameManager>.Instance.ShowTipDialogByKey("APMAX_MESSAGE", 1f);
				break;
			}
			if (selectedType == ChargeType.EventActionPoint && storedRewardAmount + ManagedSingleton<PlayerHelper>.Instance.GetEventStamina() > OrangeConst.EP_MAX)
			{
				MonoBehaviourSingleton<OrangeGameManager>.Instance.ShowTipDialogByKey("EPMAX_MESSAGE", 1f);
				break;
			}
			ManagedSingleton<PlayerNetManager>.Instance.ChargeEnergyReq(selectedType, delegate
			{
				Setup(selectedType);
				Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.CHARGE_STAMINA);
				switch (selectedType)
				{
				default:
					MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_STORE01);
					break;
				case ChargeType.Gold:
					MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_STORE01);
					break;
				}
			});
			break;
		case ChargeType.BossChallenge:
			ManagedSingleton<PlayerNetManager>.Instance.StageLimitResetReq(storedStageID, delegate
			{
				Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.CHARGE_STAMINA);
				base.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_STORE01;
				OnClickCloseBtn();
			});
			break;
		case ChargeType.RaidBossChallenge:
			PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_OK17);
			ManagedSingleton<PlayerNetManager>.Instance.RaidBossLimitResetReq(delegate(RaidBossLimitResetRes res)
			{
				WorldBossEventUI uI = MonoBehaviourSingleton<UIManager>.Instance.GetUI<WorldBossEventUI>("UI_WORLDBOSSEVENT");
				if (uI != null)
				{
					uI.UpdateRBPlayerInfo(res.RBPlayerInfo);
				}
				PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_STORE01);
				Setup(ChargeType.RaidBossChallenge);
			});
			break;
		case ChargeType.CrusadeChallenge:
			if (Singleton<CrusadeSystem>.Instance.CheckInEventTime())
			{
				MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_STORE01);
				Singleton<CrusadeSystem>.Instance.AddChallengeCount();
			}
			else
			{
				CommonUIHelper.ShowCommonTipUI("CANNOT_BUY_CHALLENGE_COUNT");
			}
			break;
		case ChargeType.ResearchSlot:
			ManagedSingleton<PlayerNetManager>.Instance.UnlockResearchSlotReq((ResearchSlot)(ManagedSingleton<PlayerNetManager>.Instance.researchInfo.dicResearch.Count + 1), delegate
			{
				callback.CheckTargetToInvoke();
				OnClickCloseBtn();
			});
			break;
		case ChargeType.CardStorageSlot:
			ManagedSingleton<PlayerNetManager>.Instance.ExpandCardStorageReq(1, delegate
			{
				callback.CheckTargetToInvoke();
				OnClickCloseBtn();
			});
			break;
		case ChargeType.CardDeploySlot:
			ManagedSingleton<PlayerNetManager>.Instance.ExpandCardDeploySlotReq(1, delegate
			{
				callback.CheckTargetToInvoke();
				OnClickCloseBtn();
			});
			break;
		}
	}

	public override void OnClickCloseBtn()
	{
		base.OnClickCloseBtn();
	}

	public void CaculateNextBuyStep(int currentCount, ChargeType type, out int cost, out int reward)
	{
		cost = 0;
		reward = 0;
		BUYSTEP_TABLE value = null;
		if (!ManagedSingleton<OrangeDataManager>.Instance.BUYSTEP_TABLE_DICT.TryGetValue((int)ChargeTypeToBuyStepType(type), out value))
		{
			return;
		}
		List<int> list = new List<int> { value.n_STEP1, value.n_STEP2, value.n_STEP3, value.n_STEP4, value.n_STEP5 };
		List<int> list2 = new List<int> { value.n_STEP1_COST, value.n_STEP2_COST, value.n_STEP3_COST, value.n_STEP4_COST, value.n_STEP5_COST };
		if (list2.Count != list2.Count)
		{
			return;
		}
		int num = 0;
		for (int i = 0; i < list.Count && list[i] != 0 && list2[i] != 0; i++)
		{
			num += list[i];
			if (currentCount < num)
			{
				cost = list2[i];
				reward = value.n_BUY_COUNT;
				break;
			}
		}
	}

	public BuyStepType ChargeTypeToBuyStepType(ChargeType type)
	{
		switch (type)
		{
		case ChargeType.ActionPoint:
			return BuyStepType.BuyActionPoint;
		case ChargeType.Gold:
			return BuyStepType.BuyGold;
		case ChargeType.BossChallenge:
			return BuyStepType.BuyBossChallenge;
		case ChargeType.RaidBossChallenge:
			return BuyStepType.BuyRaidBossChallenge;
		case ChargeType.CrusadeChallenge:
			return BuyStepType.BuyCrusadeChallenge;
		case ChargeType.EventActionPoint:
			return BuyStepType.BuyEventActionPoint;
		default:
			return BuyStepType.BuyActionPoint;
		}
	}

	public int GetTotalValidCount(ChargeType type)
	{
		BUYSTEP_TABLE value = null;
		if (ManagedSingleton<OrangeDataManager>.Instance.BUYSTEP_TABLE_DICT.TryGetValue((int)type, out value))
		{
			List<int> obj = new List<int> { value.n_STEP1, value.n_STEP2, value.n_STEP3, value.n_STEP4, value.n_STEP5 };
			int num = 0;
			{
				foreach (int item in obj)
				{
					num += item;
				}
				return num;
			}
		}
		return 0;
	}

	private void LoadLawBtn()
	{
		if ((storedCostItemID == OrangeConst.ITEMID_FREE_JEWEL || storedCostItemID == OrangeConst.ITEMID_JEWEL) && objLawBtn == null)
		{
			CommonAssetHelper.LoadLawObj("BtnJPLaw", base.transform, new Vector3(0f, -380f, 0f), delegate(Transform t)
			{
				objLawBtn = t;
			});
		}
	}
}
