using System;
using System.Collections.Generic;
using CallbackDefs;
using UnityEngine;
using UnityEngine.UI;
using enums;

public class ResearchCell : ScrollIndexCallback
{
	[SerializeField]
	private GameObject objNotOpen;

	[SerializeField]
	private Text textNotOpen;

	[SerializeField]
	private GameObject objCell;

	[Header("Left Info")]
	[SerializeField]
	private GameObject objTargetItemPos;

	[SerializeField]
	private Text textResearchItemName;

	[SerializeField]
	private Text textResearchItemCount;

	[SerializeField]
	private Text textResearchItemExp;

	[Header("Right Info")]
	[SerializeField]
	private Text textResearchRemainTime;

	[SerializeField]
	private Button btnResearch;

	[SerializeField]
	private Text textButtonResearch;

	[SerializeField]
	private Image imgButtonJewel;

	[SerializeField]
	private Image imgRetrieve;

	[Header("Center Info")]
	[SerializeField]
	private GameObject[] transMaterialItemPos = new GameObject[5];

	private CommonIconBase[] arrIcon = new CommonIconBase[5];

	private const int MAX_MATERIAL = 5;

	[SerializeField]
	private Image[] imgProgress;

	private SystemSE ClickOKSE = SystemSE.CRI_SYSTEMSE_SYS_OK17;

	private int listIndex;

	private bool payToRetrieve;

	private Dictionary<int, int> dicMaterialNeeded = new Dictionary<int, int>();

	public override void ResetStatus()
	{
		listIndex = 0;
		payToRetrieve = false;
		objCell.gameObject.SetActive(true);
		objNotOpen.gameObject.SetActive(true);
		textResearchItemCount.gameObject.SetActive(true);
		textResearchItemExp.gameObject.SetActive(true);
		imgButtonJewel.gameObject.SetActive(false);
		imgRetrieve.gameObject.SetActive(false);
		dicMaterialNeeded.Clear();
		for (int i = 0; i < 5; i++)
		{
			if (arrIcon[i] != null)
			{
				arrIcon[i].gameObject.SetActive(false);
			}
		}
	}

	public override void ScrollCellIndex(int p_idx)
	{
		ResetStatus();
		listIndex = p_idx;
		RESEARCH_TABLE row = ManagedSingleton<ResearchHelper>.Instance.GetUIViewData(listIndex);
		if (row == null)
		{
			return;
		}
		ITEM_TABLE item = null;
		if (!ManagedSingleton<OrangeTableHelper>.Instance.GetItem(row.n_ITEMID, out item))
		{
			return;
		}
		NetResearchRecord researchRecord = ManagedSingleton<ResearchHelper>.Instance.GetResearchRecord(row.n_ID);
		string[] array = new string[4] { "RESEARCH_RESET_0", "RESEARCH_RESET_1", "RESEARCH_RESET_2", "RESEARCH_RESET_3" };
		int num = row.n_LIMIT;
		if (researchRecord != null && !MonoBehaviourSingleton<OrangeGameManager>.Instance.IsPassedResetDate(researchRecord.LastReseachTime, (ResetRule)row.n_RESET_RULE))
		{
			num -= researchRecord.Count;
		}
		textResearchItemCount.text = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr(array[row.n_RESET_RULE]), num, row.n_LIMIT);
		textResearchItemExp.text = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("RESEARCH_EXP"), row.n_GET_EXP);
		textResearchItemName.text = ManagedSingleton<OrangeTextDataManager>.Instance.ITEMTEXT_TABLE_DICT.GetL10nValue(item.w_NAME);
		if (row.n_RANK > ManagedSingleton<PlayerHelper>.Instance.GetLV())
		{
			objCell.gameObject.SetActive(false);
			textResearchItemCount.gameObject.SetActive(false);
			textResearchItemExp.gameObject.SetActive(false);
			textNotOpen.text = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("RESEARCH_CONDITION"), row.n_RANK);
		}
		else
		{
			objNotOpen.gameObject.SetActive(false);
			NetResearchInfo researchInfo = ManagedSingleton<ResearchHelper>.Instance.GetResearchInfo(row.n_ID);
			if (researchInfo != null)
			{
				textResearchRemainTime.text = OrangeGameUtility.GetRemainTimeText(researchInfo.FinishTime, true, true);
				int num2 = 2;
				if (researchInfo.FinishTime < MonoBehaviourSingleton<OrangeGameManager>.Instance.ServerUnixTimeNowUTC + num2)
				{
					textResearchRemainTime.text = string.Empty;
					textButtonResearch.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("RESEARCH_RETRIEVE");
					imgRetrieve.gameObject.SetActive(true);
				}
				else
				{
					textButtonResearch.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("RESEARCH_FAST_COMBINE");
					imgButtonJewel.gameObject.SetActive(true);
					payToRetrieve = true;
				}
			}
			else
			{
				int serviceBonusValue = ManagedSingleton<ServiceHelper>.Instance.GetServiceBonusValue(ServiceType.ResearchSpeedup);
				int num3 = Convert.ToInt32(Math.Ceiling((float)(row.n_TIME * (100 - serviceBonusValue)) / 100f));
				textResearchRemainTime.text = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("RESEARCH_TIME"), OrangeGameUtility.GetTimeText(num3, true, true));
				textButtonResearch.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("RESEARCH_COMBINE");
			}
		}
		MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(AssetBundleScriptableObject.Instance.m_uiPath + "CommonIconBaseSmall", "CommonIconBaseSmall", delegate(GameObject asset)
		{
			if (asset != null)
			{
				if (row.n_RANK <= ManagedSingleton<PlayerHelper>.Instance.GetLV())
				{
					for (int i = 0; i < 5; i++)
					{
						GameObject gameObject = transMaterialItemPos[i];
						CommonIconBase componentInChildren = gameObject.GetComponentInChildren<CommonIconBase>(true);
						if (null == componentInChildren)
						{
							UnityEngine.Object.Instantiate(asset, gameObject.transform);
							componentInChildren = gameObject.GetComponentInChildren<CommonIconBase>();
							componentInChildren.gameObject.transform.localScale = new Vector3(0.7f, 0.7f, 1f);
							componentInChildren.gameObject.SetActive(false);
						}
						arrIcon[i] = componentInChildren;
						CreateMaterialIcon(row.n_MATERIAL, i);
					}
				}
				CommonIconBase componentInChildren2 = objTargetItemPos.GetComponentInChildren<CommonIconBase>(true);
				if (componentInChildren2 == null)
				{
					UnityEngine.Object.Instantiate(asset, objTargetItemPos.transform);
					componentInChildren2 = objTargetItemPos.GetComponentInChildren<CommonIconBase>();
					componentInChildren2.gameObject.transform.localScale = new Vector3(0.95f, 0.95f, 1f);
					componentInChildren2.gameObject.SetActive(true);
				}
				if (item.n_TYPE == 5 && item.n_TYPE_X == 1 && (int)item.f_VALUE_Y > 0)
				{
					componentInChildren2.SetItemWithAmountForCard(item.n_ID, row.n_ITEMCOUNT, OnClickItem);
				}
				else
				{
					componentInChildren2.SetItemWithAmount(item.n_ID, row.n_ITEMCOUNT, OnClickItem);
				}
			}
		});
	}

	private void OnClickItem(int p_idx)
	{
		ITEM_TABLE item = null;
		if (!ManagedSingleton<ExtendDataHelper>.Instance.ITEM_TABLE_DICT.TryGetValue(p_idx, out item))
		{
			return;
		}
		if (item.n_TYPE == 5 && item.n_TYPE_X == 1 && (int)item.f_VALUE_Y > 0)
		{
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_CardInfo", delegate(CardInfoUI ui)
			{
				ui.PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
				ui.bOnlyShowBasic = true;
				ui.bNeedInitList = true;
				ui.nTargetCardSeqID = 0;
				ui.nTargetCardID = (int)item.f_VALUE_Y;
			});
			return;
		}
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_ItemInfo", delegate(ItemInfoUI ui)
		{
			int value;
			dicMaterialNeeded.TryGetValue(p_idx, out value);
			ui.CanShowHow2Get = true;
			ui.Setup(item, null, value);
			ui.closeCB = (Callback)Delegate.Combine(ui.closeCB, (Callback)delegate
			{
				RESEARCH_TABLE uIViewData = ManagedSingleton<ResearchHelper>.Instance.GetUIViewData(listIndex);
				if (uIViewData != null)
				{
					for (int i = 0; i < 5; i++)
					{
						CreateMaterialIcon(uIViewData.n_MATERIAL, i);
					}
				}
			});
		});
	}

	public void CreateMaterialIcon(int materialID, int idx)
	{
		MATERIAL_TABLE value = null;
		ITEM_TABLE value2 = null;
		if (!(transMaterialItemPos[idx] == null) && idx < 5 && ManagedSingleton<OrangeDataManager>.Instance.MATERIAL_TABLE_DICT.TryGetValue(materialID, out value))
		{
			List<int> list = new List<int> { value.n_MATERIAL_1, value.n_MATERIAL_2, value.n_MATERIAL_3, value.n_MATERIAL_4, value.n_MATERIAL_5 };
			if (ManagedSingleton<ExtendDataHelper>.Instance.ITEM_TABLE_DICT.TryGetValue(list[idx], out value2))
			{
				arrIcon[idx].gameObject.SetActive(true);
				int p_value = arrIcon[idx].SetupMaterial(value.n_ID, idx, OnClickItem);
				dicMaterialNeeded.ContainsAdd(value2.n_ID, p_value);
			}
			else
			{
				arrIcon[idx].gameObject.SetActive(false);
			}
		}
	}

	public void ReceiveResearch(int currentJewelCost)
	{
		RESEARCH_TABLE uIViewData = ManagedSingleton<ResearchHelper>.Instance.GetUIViewData(listIndex);
		if (uIViewData == null)
		{
			return;
		}
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(ClickOKSE);
		NetResearchInfo researchInfo = ManagedSingleton<ResearchHelper>.Instance.GetResearchInfo(uIViewData.n_ID);
		if (researchInfo == null)
		{
			return;
		}
		if (currentJewelCost > 0 && ManagedSingleton<PlayerHelper>.Instance.GetTotalJewel() < currentJewelCost)
		{
			MonoBehaviourSingleton<OrangeGameManager>.Instance.ShowCommonMsg(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("DIAMOND_OUT"), delegate
			{
				MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_ShopTop", delegate(ShopTopUI ui)
				{
					ui.Setup(ShopTopUI.ShopSelectTab.directproduct);
				});
			}, null);
			return;
		}
		ManagedSingleton<PlayerNetManager>.Instance.ReceiveResearch((ResearchSlot)researchInfo.Slot, currentJewelCost > 0, delegate(List<NetRewardInfo> p_param)
		{
			SendMessageUpwards("UpdateResearchInfo", SendMessageOptions.DontRequireReceiver);
			List<NetRewardInfo> rewardList = p_param;
			if (rewardList != null && rewardList.Count > 0)
			{
				MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_RewardPopup", delegate(RewardPopopUI ui)
				{
					ui.Setup(rewardList);
				});
			}
		});
	}

	public void OnClickRetrieveBtn()
	{
		RESEARCH_TABLE row = ManagedSingleton<ResearchHelper>.Instance.GetUIViewData(listIndex);
		if (row == null)
		{
			return;
		}
		NetResearchInfo researchNetInfo = ManagedSingleton<ResearchHelper>.Instance.GetResearchInfo(row.n_ID);
		if (researchNetInfo != null)
		{
			if ((researchNetInfo.FinishTime > MonoBehaviourSingleton<OrangeGameManager>.Instance.ServerUnixTimeNowUTC) ? true : false)
			{
				MonoBehaviourSingleton<UIManager>.Instance.LoadResourceUI("UI_CommonMsg", delegate(CommonUI ui)
				{
					int num2 = researchNetInfo.FinishTime - Convert.ToInt32(MonoBehaviourSingleton<OrangeGameManager>.Instance.ServerUnixTimeNowUTC);
					int jewelCost = (int)Math.Ceiling((float)num2 / 3600f) * OrangeConst.RESEARCH_BOOST_JEWEL;
					string arg = ManagedSingleton<OrangeTableHelper>.Instance.GetItemName(OrangeConst.ITEMID_FREE_JEWEL);
					string p_desc = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("RESEARCH_FAST_COMBINE_CONFIRM"), arg, jewelCost);
					ui.SetupYesNO(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_TIP"), p_desc, MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_OK"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_CANCEL"), delegate
					{
						ClickOKSE = SystemSE.CRI_SYSTEMSE_SYS_STORE01;
						Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.UI_RESEARCH_COMPLETE_VOICE, NAVI_MENU.CRI_NAVI_MENU_RICO_MENU29);
						ReceiveResearch(jewelCost);
					});
				});
			}
			else
			{
				Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.UI_RESEARCH_COMPLETE_VOICE, NAVI_MENU.CRI_NAVI_MENU_RICO_MENU21);
				ReceiveResearch(0);
			}
			return;
		}
		NetResearchRecord researchRecord = ManagedSingleton<ResearchHelper>.Instance.GetResearchRecord(row.n_ID);
		int num = row.n_LIMIT;
		if (researchRecord != null && !MonoBehaviourSingleton<OrangeGameManager>.Instance.IsPassedResetDate(researchRecord.LastReseachTime, (ResetRule)row.n_RESET_RULE))
		{
			num -= researchRecord.Count;
		}
		if (num <= 0)
		{
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_Tip", delegate(TipUI tipUI)
			{
				tipUI.Setup(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("RESEARCH_REACH_LIMIT"), true);
			});
			return;
		}
		int idleResearchSlot = ManagedSingleton<ResearchHelper>.Instance.GetIdleResearchSlot();
		if (idleResearchSlot == int.MaxValue)
		{
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_Tip", delegate(TipUI tipUI)
			{
				tipUI.Setup(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("RESEARCH_SLOT_FULL"), true);
			});
			return;
		}
		int firstNotEnoughItemID = 0;
		if (!ManagedSingleton<PlayerHelper>.Instance.CheckMaterialEnough(row.n_MATERIAL, out firstNotEnoughItemID))
		{
			ITEM_TABLE item = null;
			if (!ManagedSingleton<ExtendDataHelper>.Instance.ITEM_TABLE_DICT.TryGetValue(firstNotEnoughItemID, out item))
			{
				return;
			}
			if (firstNotEnoughItemID == OrangeConst.ITEMID_MONEY)
			{
				MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_Tip", delegate(TipUI tipUI)
				{
					tipUI.Setup(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("SYSTEM_NOT_OEPN"), true);
				});
				return;
			}
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_ItemInfo", delegate(ItemInfoUI ui)
			{
				ui.Setup(item);
				ui.closeCB = (Callback)Delegate.Combine(ui.closeCB, (Callback)delegate
				{
					for (int i = 0; i < 5; i++)
					{
						CreateMaterialIcon(row.n_MATERIAL, i);
					}
				});
			});
			return;
		}
		if (row.n_EVENT != 0)
		{
			List<EVENT_TABLE> eventTableByType = ManagedSingleton<ExtendDataHelper>.Instance.GetEventTableByType(enums.EventType.EVENT_RESEARCH, MonoBehaviourSingleton<OrangeGameManager>.Instance.ServerUnixTimeNowUTC);
			bool flag = false;
			foreach (EVENT_TABLE item2 in eventTableByType)
			{
				if (item2.n_ID == row.n_EVENT)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_Tip", delegate(TipUI tipUI)
				{
					tipUI.Setup(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("RESEARCH_EVENT_FINISHED"), true);
				});
				return;
			}
		}
		ManagedSingleton<PlayerNetManager>.Instance.ResearchStart(row.n_ID, (ResearchSlot)idleResearchSlot, delegate
		{
			SendMessageUpwards("UpdateResearchInfo", SendMessageOptions.DontRequireReceiver);
		});
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK10);
	}
}
