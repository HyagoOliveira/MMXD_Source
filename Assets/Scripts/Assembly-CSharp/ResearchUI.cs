using System;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.UI;
using enums;

public class ResearchUI : OrangeUIBase
{
	public enum ResearchPageType : short
	{
		FREE_RESEARCH = 0,
		NORMAL_RESEARCH = 1,
		EVENT_RESEARCH = 2
	}

	[Header("Left Banner")]
	[SerializeField]
	private Transform storageRoot;

	private List<StorageInfo> listStorage = new List<StorageInfo>();

	[Header("Center Top")]
	[SerializeField]
	private Text textResearchLv;

	[SerializeField]
	private Text textResearchExp;

	[SerializeField]
	private Transform trmExpRoot;

	[SerializeField]
	private Image imgExpUnit;

	[SerializeField]
	private Text textNaviTalk;

	[SerializeField]
	private Transform trmNaviParent;

	[SerializeField]
	private Text textResearchItemCount;

	[SerializeField]
	private ResearchIcon[] iconResearch;

	[SerializeField]
	private Button buttonAddSlot;

	[Header("Center")]
	[SerializeField]
	private GameObject objFreeResearchTab;

	[SerializeField]
	private GameObject objNormalResearchTab;

	[SerializeField]
	private GameObject objAPIncreaseService;

	[SerializeField]
	private Text textServiceAPIncrease;

	[SerializeField]
	private GameObject objAPRemainTimeTip;

	[SerializeField]
	private Text textAPRemainTime;

	[SerializeField]
	private GameObject objTimeBoostService;

	[SerializeField]
	private Text textServiceTimeBoost;

	[SerializeField]
	private GameObject objBoostRemainTimeTip;

	[SerializeField]
	private Text textBoostRemainTime;

	[SerializeField]
	private LoopVerticalScrollRect scrollRect;

	[SerializeField]
	private ResearchCell researchCell;

	[SerializeField]
	private Transform trmFreeResearchRoot;

	[SerializeField]
	private FreeResearchCell freeResearchCell;

	[BoxGroup("Sound")]
	[SerializeField]
	private SystemSE m_clickItemIcon;

	[BoxGroup("Sound")]
	[SerializeField]
	private SystemSE m_error;

	private const int TOTAL_EXPBAR_UNIT = 38;

	private NAVI_MENU NowVoice;

	private bool IgnoreSwtichTabSE;

	public NAVI_MENU SpecialVoice;

	private ResearchPageType SelectedPageType { get; set; }

	private int CurrentParameterValue { get; set; }

	public void Setup(ResearchPageType type = ResearchPageType.FREE_RESEARCH, int paramValue = -1)
	{
		SetupButtonListener();
		if (!ManagedSingleton<PlayerHelper>.Instance.TutorialDone(OrangeConst.TUTORIAL_RESEARCH))
		{
			type = ResearchPageType.NORMAL_RESEARCH;
			paramValue = 1;
		}
		SelectedPageType = type;
		CurrentParameterValue = paramValue;
		CreateNewStorageTab(type, paramValue);
		UpdateResearchTopInfo();
		if (!(trmNaviParent.GetComponentInChildren<StandNaviDb>() == null))
		{
			return;
		}
		MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(string.Format(AssetBundleScriptableObject.Instance.m_dragonbones_chdb, "ch_navi_1"), "ch_navi_1_db", delegate(GameObject obj)
		{
			StandNaviDb component = UnityEngine.Object.Instantiate(obj, trmNaviParent, false).GetComponent<StandNaviDb>();
			if ((bool)component)
			{
				component.Setup(StandNaviDb.NAVI_DB_TYPE.NORMAL);
			}
		});
	}

	private void CreateNewStorageTab(ResearchPageType type = ResearchPageType.FREE_RESEARCH, int paramValue = -1)
	{
		EXP_TABLE researchRowByExp = ManagedSingleton<ResearchHelper>.Instance.GetResearchRowByExp(ManagedSingleton<PlayerHelper>.Instance.GetResearchExp());
		ManagedSingleton<ResearchHelper>.Instance.GetResearchRowByLevel(researchRowByExp.n_ID - 1);
		ClearStorage();
		int p_defaultIdx = 0;
		foreach (EVENT_TABLE item in ManagedSingleton<ExtendDataHelper>.Instance.GetEventTableByType(enums.EventType.EVENT_RESEARCH, MonoBehaviourSingleton<OrangeGameManager>.Instance.ServerUnixTimeNowUTC))
		{
			StorageInfo storageInfo = new StorageInfo("RESEARCH_TAB_EVENT", false, 0, OnClickTab, DisplayNew);
			storageInfo.Param = new object[2]
			{
				ResearchPageType.EVENT_RESEARCH,
				item.n_TYPE_X
			};
			listStorage.Add(storageInfo);
			if (type == ResearchPageType.EVENT_RESEARCH && item.n_TYPE_X == paramValue)
			{
				p_defaultIdx = listStorage.Count - 1;
			}
		}
		StorageInfo storageInfo2 = new StorageInfo("RESEARCH_TAB_FREE", false, 0, OnClickTab, DisplayNew);
		storageInfo2.Param = new object[2]
		{
			ResearchPageType.FREE_RESEARCH,
			0
		};
		listStorage.Add(storageInfo2);
		if (type == ResearchPageType.FREE_RESEARCH)
		{
			p_defaultIdx = listStorage.Count - 1;
		}
		for (int num = researchRowByExp.n_ID; num > 0; num--)
		{
			StorageInfo storageInfo3 = new StorageInfo(GetSubTextByLevel(num), false, 0, OnClickTab, DisplayNew);
			storageInfo3.Param = new object[2]
			{
				ResearchPageType.NORMAL_RESEARCH,
				num
			};
			listStorage.Add(storageInfo3);
			if (type == ResearchPageType.NORMAL_RESEARCH && num == paramValue)
			{
				p_defaultIdx = listStorage.Count - 1;
			}
		}
		StorageGenerator.Load("StorageComp00", listStorage, p_defaultIdx, 0, storageRoot);
	}

	private bool DisplayNew(object[] param)
	{
		if (param == null || param.Length == 0)
		{
			return false;
		}
		ResearchPageType researchPageType = (ResearchPageType)param[0];
		int level = (int)param[1];
		if (researchPageType == ResearchPageType.FREE_RESEARCH)
		{
			return ManagedSingleton<ResearchHelper>.Instance.IsAnyFreeResearchCouldBeRetrieved();
		}
		return ManagedSingleton<ResearchHelper>.Instance.IsAnyNormalResearchDoneByLevel(level);
	}

	private void UpdateResearchTopInfo()
	{
		EXP_TABLE researchRowByExp = ManagedSingleton<ResearchHelper>.Instance.GetResearchRowByExp(ManagedSingleton<PlayerHelper>.Instance.GetResearchExp());
		EXP_TABLE researchRowByLevel = ManagedSingleton<ResearchHelper>.Instance.GetResearchRowByLevel(researchRowByExp.n_ID - 1);
		string text = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("RESEARCH_LEVEL"), researchRowByExp.n_ID);
		textResearchLv.text = text;
		while (true)
		{
			Image componentInChildren = trmExpRoot.GetComponentInChildren<Image>();
			if (!componentInChildren)
			{
				break;
			}
			UnityEngine.Object.DestroyImmediate(componentInChildren.gameObject);
		}
		if (researchRowByLevel != null)
		{
			float b = (float)(ManagedSingleton<PlayerHelper>.Instance.GetResearchExp() - researchRowByLevel.n_TOTAL_RESEARCHEXP) / (float)researchRowByExp.n_RESEARCHEXP;
			b = Mathf.Min(1f, b);
			int num = Convert.ToInt32(Mathf.Floor(b * 38f));
			imgExpUnit.gameObject.SetActive(true);
			for (int i = 0; i < num; i++)
			{
				UnityEngine.Object.Instantiate(imgExpUnit, trmExpRoot);
			}
			imgExpUnit.gameObject.SetActive(false);
		}
		textResearchExp.text = string.Format("{0}/{1}", ManagedSingleton<PlayerHelper>.Instance.GetResearchExp() - researchRowByLevel.n_TOTAL_RESEARCHEXP, researchRowByExp.n_RESEARCHEXP);
		int num2 = 0;
		foreach (KeyValuePair<int, NetResearchInfo> item in ManagedSingleton<PlayerNetManager>.Instance.researchInfo.dicResearch)
		{
			if (item.Value.StartTime != 0 && item.Value.FinishTime != 0)
			{
				num2++;
			}
		}
		string text2 = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("RESEARCHING_COUNT"), num2, ManagedSingleton<PlayerNetManager>.Instance.researchInfo.dicResearch.Count);
		textResearchItemCount.text = text2;
		bool flag = false;
		int num3 = 0;
		foreach (KeyValuePair<int, NetResearchInfo> item2 in ManagedSingleton<PlayerNetManager>.Instance.researchInfo.dicResearch)
		{
			if (iconResearch[num3] != null)
			{
				iconResearch[num3].gameObject.SetActive(true);
				RESEARCH_TABLE value = null;
				ITEM_TABLE value2 = null;
				if (ManagedSingleton<OrangeDataManager>.Instance.RESEARCH_TABLE_DICT.TryGetValue(item2.Value.ResearchID, out value))
				{
					if (ManagedSingleton<ExtendDataHelper>.Instance.ITEM_TABLE_DICT.TryGetValue(value.n_ITEMID, out value2))
					{
						string empty = string.Empty;
						string s_ICON = value2.s_ICON;
						if (ManagedSingleton<OrangeTableHelper>.Instance.IsCard(value2))
						{
							CARD_TABLE value3 = null;
							if (!ManagedSingleton<OrangeDataManager>.Instance.CARD_TABLE_DICT.TryGetValue((int)value2.f_VALUE_Y, out value3))
							{
								iconResearch[num3].Show(false, false);
								iconResearch[num3].SetResearchDefault();
								continue;
							}
							ManagedSingleton<OrangeTableHelper>.Instance.GetCardTypeIndex(value3.n_TYPE);
							empty = AssetBundleScriptableObject.Instance.m_iconCard + string.Format(AssetBundleScriptableObject.Instance.m_icon_card_s_format, value3.n_PATCH);
							s_ICON = value3.s_ICON;
							iconResearch[num3].SetSize(new Vector2(100f, 100f));
						}
						else
						{
							empty = AssetBundleScriptableObject.Instance.GetIconItem(s_ICON);
							iconResearch[num3].SetSize(new Vector2(106f, 106f));
						}
						iconResearch[num3].Show(true, value2.n_TYPE == 4);
						iconResearch[num3].Setup(item2.Value.ResearchID, empty, s_ICON, OnQuickRetrieveBtn);
						iconResearch[num3].SetRare(value2.n_RARE);
						iconResearch[num3].SetAmount(value.n_ITEMCOUNT);
						iconResearch[num3].SetProgress((int)MonoBehaviourSingleton<OrangeGameManager>.Instance.ServerUnixTimeNowUTC - item2.Value.StartTime, item2.Value.FinishTime - item2.Value.StartTime);
						if (!flag)
						{
							flag = ((MonoBehaviourSingleton<OrangeGameManager>.Instance.ServerUnixTimeNowUTC <= item2.Value.FinishTime) ? true : false);
						}
					}
				}
				else
				{
					iconResearch[num3].Show(false, false);
					iconResearch[num3].SetResearchDefault();
				}
			}
			num3++;
		}
		buttonAddSlot.gameObject.SetActive(5 != ManagedSingleton<PlayerNetManager>.Instance.researchInfo.dicResearch.Count);
	}

	private void ClearStorage()
	{
		listStorage.Clear();
		while (true)
		{
			StorageComponent componentInChildren = GetComponentInChildren<StorageComponent>();
			if ((bool)componentInChildren)
			{
				UnityEngine.Object.DestroyImmediate(componentInChildren.gameObject);
				continue;
			}
			break;
		}
	}

	private string GetSubTextByLevel(int level)
	{
		if (level > 0)
		{
			return string.Format("RESEARCH_TAB_RESEARCH{0}", level);
		}
		return string.Empty;
	}

	public void OnClickTab(object p_param)
	{
		StorageInfo storageInfo = (StorageInfo)p_param;
		if ((ResearchPageType)storageInfo.Param[0] == SelectedPageType)
		{
			if (CurrentParameterValue == -1 || (int)storageInfo.Param[1] == CurrentParameterValue)
			{
				IgnoreSwtichTabSE = false;
			}
			else
			{
				IgnoreSwtichTabSE = true;
			}
		}
		else
		{
			IgnoreSwtichTabSE = true;
		}
		OnTabSelected((ResearchPageType)storageInfo.Param[0], (int)storageInfo.Param[1]);
	}

	private void SetupFreeResearchTab()
	{
		while (true)
		{
			FreeResearchCell componentInChildren = trmFreeResearchRoot.GetComponentInChildren<FreeResearchCell>();
			if (!componentInChildren)
			{
				break;
			}
			UnityEngine.Object.DestroyImmediate(componentInChildren.gameObject);
		}
		foreach (ResearchHelper.FreeSearchItem value in Enum.GetValues(typeof(ResearchHelper.FreeSearchItem)))
		{
			FreeResearchCell freeResearchCell = UnityEngine.Object.Instantiate(this.freeResearchCell, trmFreeResearchRoot);
			if (!(freeResearchCell != null))
			{
				continue;
			}
			NetFreeResearchInfo info = null;
			foreach (NetFreeResearchInfo item in ManagedSingleton<PlayerNetManager>.Instance.researchInfo.listFreeResearch)
			{
				if (item.ResearchID == (int)value)
				{
					info = item;
					break;
				}
			}
			freeResearchCell.gameObject.SetActive(true);
			freeResearchCell.Setup((int)value, info);
		}
		UpdateNaviTalk();
		int serviceBonusValue = ManagedSingleton<ServiceHelper>.Instance.GetServiceBonusValue(ServiceType.ResearchAPIncrease);
		objAPIncreaseService.SetActive((serviceBonusValue != 0) ? true : false);
		textServiceAPIncrease.text = string.Format("+{0}%", serviceBonusValue);
	}

	private void SetupResearchTab(int paramValue)
	{
		ManagedSingleton<ResearchHelper>.Instance.CollectUIViewData(paramValue);
		scrollRect.ClearCells();
		scrollRect.OrangeInit(researchCell, 4, ManagedSingleton<ResearchHelper>.Instance.FillteredDataCount);
		UpdateNaviTalk();
		int serviceBonusValue = ManagedSingleton<ServiceHelper>.Instance.GetServiceBonusValue(ServiceType.ResearchSpeedup);
		objTimeBoostService.SetActive((serviceBonusValue != 0) ? true : false);
		textServiceTimeBoost.text = string.Format("-{0}%", serviceBonusValue);
	}

	public void OnTabSelected(ResearchPageType type, int paramValue)
	{
		SelectedPageType = type;
		CurrentParameterValue = paramValue;
		objFreeResearchTab.gameObject.SetActive(type == ResearchPageType.FREE_RESEARCH);
		objNormalResearchTab.gameObject.SetActive(type != ResearchPageType.FREE_RESEARCH);
		if (type == ResearchPageType.FREE_RESEARCH)
		{
			SetupFreeResearchTab();
		}
		else
		{
			SetupResearchTab(paramValue);
		}
	}

	public void GetUnlockMaterailItem(int materialID, out int itemID, out int itemAmount)
	{
		itemID = 0;
		itemAmount = 0;
		MATERIAL_TABLE value = null;
		if (ManagedSingleton<OrangeDataManager>.Instance.MATERIAL_TABLE_DICT.TryGetValue(materialID, out value))
		{
			itemID = value.n_MATERIAL_1;
			itemAmount = value.n_MATERIAL_MOUNT1;
		}
	}

	public void OnUnlockResearchSlot()
	{
		int[] array = new int[5]
		{
			OrangeConst.RESEARCH_SLOT_MATERIAL1,
			OrangeConst.RESEARCH_SLOT_MATERIAL2,
			OrangeConst.RESEARCH_SLOT_MATERIAL3,
			OrangeConst.RESEARCH_SLOT_MATERIAL4,
			OrangeConst.RESEARCH_SLOT_MATERIAL5
		};
		int count = ManagedSingleton<PlayerNetManager>.Instance.researchInfo.dicResearch.Count;
		MATERIAL_TABLE row = null;
		if (!ManagedSingleton<OrangeDataManager>.Instance.MATERIAL_TABLE_DICT.TryGetValue(array[count], out row))
		{
			return;
		}
		int num = ManagedSingleton<PlayerHelper>.Instance.GetItemValue(row.n_MATERIAL_1);
		if (row.n_MATERIAL_1 == OrangeConst.ITEMID_FREE_JEWEL)
		{
			num = ManagedSingleton<PlayerHelper>.Instance.GetTotalJewel();
		}
		string itemName = ManagedSingleton<OrangeTableHelper>.Instance.GetItemName(row.n_MATERIAL_1);
		string title = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("RESEARCH_SLOT_EXPANSION");
		string msg = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("RESEARCH_SLOT_EXPANSION_CONFIRM"), itemName, row.n_MATERIAL_MOUNT1);
		string desc = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("CHARGE_ITEM_DESC"), itemName, num);
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_ChargeStamina", delegate(ChargeStaminaUI ui)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
			ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
			ui.CustomSetup(ChargeType.ResearchSlot, row.n_MATERIAL_MOUNT1, 1, row.n_MATERIAL_1, OrangeConst.ITEMID_RESEARCH_SLOT, title, msg, desc, delegate
			{
				if (row.n_MATERIAL_1 == OrangeConst.ITEMID_MONEY)
				{
					ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_STORE02;
				}
				else if (row.n_MATERIAL_1 == OrangeConst.ITEMID_FREE_JEWEL || row.n_MATERIAL_1 == OrangeConst.ITEMID_JEWEL)
				{
					ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_STORE01;
				}
				SpecialVoice = NAVI_MENU.CRI_NAVI_MENU_RICO_MENU20;
				Setup(SelectedPageType, CurrentParameterValue);
			});
		});
	}

	public void UpdateResearchInfo()
	{
		Setup(SelectedPageType, CurrentParameterValue);
		UpdateHint();
	}

	public void UpdateTopBarInfo()
	{
		UpdateResearchTopInfo();
	}

	public void UpdateHint()
	{
		StorageComponent componentInChildren = GetComponentInChildren<StorageComponent>();
		if ((bool)componentInChildren)
		{
			componentInChildren.UpdateHint();
		}
	}

	public void OnQuickRetrieveBtn(int researchID)
	{
		NetResearchInfo researchNetInfo = ManagedSingleton<ResearchHelper>.Instance.GetResearchInfo(researchID);
		if (researchNetInfo == null)
		{
			return;
		}
		if (researchNetInfo.FinishTime > MonoBehaviourSingleton<OrangeGameManager>.Instance.ServerUnixTimeNowUTC)
		{
			MonoBehaviourSingleton<UIManager>.Instance.LoadResourceUI("UI_CommonMsg", delegate(CommonUI ui)
			{
				int num = researchNetInfo.FinishTime - Convert.ToInt32(MonoBehaviourSingleton<OrangeGameManager>.Instance.ServerUnixTimeNowUTC);
				int jewelCost = (int)Math.Ceiling((float)num / 3600f) * OrangeConst.RESEARCH_BOOST_JEWEL;
				string itemName = ManagedSingleton<OrangeTableHelper>.Instance.GetItemName(OrangeConst.ITEMID_FREE_JEWEL);
				string p_desc = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("RESEARCH_FAST_COMBINE_CONFIRM"), itemName, jewelCost);
				ui.SetupYesNO(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_TIP"), p_desc, MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_OK"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_CANCEL"), delegate
				{
					ReceiveResearch(researchID, jewelCost);
				});
			});
		}
		else
		{
			ReceiveResearch(researchID, 0);
		}
	}

	public void ReceiveResearch(int researchID, int currentJewelCost)
	{
		NetResearchInfo researchInfo = ManagedSingleton<ResearchHelper>.Instance.GetResearchInfo(researchID);
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
		SpecialVoice = NAVI_MENU.CRI_NAVI_MENU_RICO_MENU21;
		ManagedSingleton<PlayerNetManager>.Instance.ReceiveResearch((ResearchSlot)researchInfo.Slot, currentJewelCost > 0, delegate(List<NetRewardInfo> p_param)
		{
			SendMessageUpwards("UpdateResearchInfo", SendMessageOptions.DontRequireReceiver);
			List<NetRewardInfo> rewardList = p_param;
			if (rewardList != null && rewardList.Count > 0)
			{
				MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(m_clickItemIcon);
				MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_RewardPopup", delegate(RewardPopopUI ui)
				{
					ui.Setup(rewardList);
				});
			}
		});
	}

	public void UpdateNaviTalk()
	{
		ResearchHelper.ResearchNaviTalk researchNaviTalk = ManagedSingleton<ResearchHelper>.Instance.GetNormalResearchTalk();
		if (SelectedPageType == ResearchPageType.FREE_RESEARCH)
		{
			researchNaviTalk = ManagedSingleton<ResearchHelper>.Instance.GetFreeResearchTalk();
		}
		string empty = string.Empty;
		bool flag = false;
		switch (researchNaviTalk)
		{
		case ResearchHelper.ResearchNaviTalk.FREE_RESEARCH_GET:
			empty = "RESEARCH_TALK_GET";
			if (NowVoice != NAVI_MENU.CRI_NAVI_MENU_RICO_MENU23)
			{
				NowVoice = NAVI_MENU.CRI_NAVI_MENU_RICO_MENU23;
			}
			else
			{
				flag = true;
			}
			break;
		case ResearchHelper.ResearchNaviTalk.FREE_RESEARCH_MISS:
			empty = "RESEARCH_TALK_MISS";
			if (NowVoice != NAVI_MENU.CRI_NAVI_MENU_RICO_MENU19)
			{
				NowVoice = NAVI_MENU.CRI_NAVI_MENU_RICO_MENU19;
			}
			else
			{
				flag = true;
			}
			break;
		case ResearchHelper.ResearchNaviTalk.FREE_RESEARCH_ING:
			empty = "RESEARCH_TALK_ING";
			if (NowVoice != NAVI_MENU.CRI_NAVI_MENU_RICO_MENU22)
			{
				NowVoice = NAVI_MENU.CRI_NAVI_MENU_RICO_MENU22;
			}
			else
			{
				flag = true;
			}
			break;
		case ResearchHelper.ResearchNaviTalk.FREE_RESEARCH_ALLCLEAR:
			empty = "RESEARCH_TALK_ALLCLEAR";
			if (NowVoice != NAVI_MENU.CRI_NAVI_MENU_RICO_MENU19)
			{
				NowVoice = NAVI_MENU.CRI_NAVI_MENU_RICO_MENU19;
			}
			else
			{
				flag = true;
			}
			break;
		case ResearchHelper.ResearchNaviTalk.RESEARCH_FINISH:
			empty = "RESEARCH_TALK_FINISH";
			if (NowVoice != NAVI_MENU.CRI_NAVI_MENU_RICO_MENU23)
			{
				NowVoice = NAVI_MENU.CRI_NAVI_MENU_RICO_MENU23;
			}
			else
			{
				flag = true;
			}
			break;
		case ResearchHelper.ResearchNaviTalk.RESEARCH_ING:
			empty = "RESEARCH_TALK_ING";
			if (NowVoice != NAVI_MENU.CRI_NAVI_MENU_RICO_MENU22)
			{
				NowVoice = NAVI_MENU.CRI_NAVI_MENU_RICO_MENU22;
			}
			else
			{
				flag = true;
			}
			break;
		default:
			empty = "RESEARCH_TALK_DEFAULT";
			if (NowVoice != NAVI_MENU.CRI_NAVI_MENU_RICO_MENU19)
			{
				NowVoice = NAVI_MENU.CRI_NAVI_MENU_RICO_MENU19;
			}
			else
			{
				flag = true;
			}
			break;
		}
		textNaviTalk.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr(empty);
		if (!IgnoreSwtichTabSE)
		{
			if (SpecialVoice != 0)
			{
				NowVoice = SpecialVoice;
				flag = false;
			}
			if (!flag)
			{
				StartCoroutine(DelayPlaySE(0.5f));
			}
		}
		else
		{
			IgnoreSwtichTabSE = false;
		}
	}

	private IEnumerator DelayPlaySE(float delay)
	{
		yield return new WaitForSeconds(delay);
		MonoBehaviourSingleton<AudioManager>.Instance.Play("NAVI_MENU", (int)NowVoice);
		SpecialVoice = NAVI_MENU.NONE;
	}

	public void SetSpecialVoice(NAVI_MENU key)
	{
		SpecialVoice = key;
	}

	private void OnDestroy()
	{
		MonoBehaviourSingleton<AudioManager>.Instance.Stop("NAVI_MENU");
	}

	private void OnEnable()
	{
		Singleton<GenericEventManager>.Instance.AttachEvent(EventManager.ID.CHANGE_DAY, DayChange);
		Singleton<GenericEventManager>.Instance.AttachEvent<NAVI_MENU>(EventManager.ID.UI_RESEARCH_COMPLETE_VOICE, SetSpecialVoice);
	}

	private void OnDisable()
	{
		Singleton<GenericEventManager>.Instance.DetachEvent(EventManager.ID.CHANGE_DAY, DayChange);
		Singleton<GenericEventManager>.Instance.DetachEvent<NAVI_MENU>(EventManager.ID.UI_RESEARCH_COMPLETE_VOICE, SetSpecialVoice);
	}

	public void DayChange()
	{
		MonoBehaviourSingleton<UIManager>.Instance.BackToHometop();
	}

	public void SetupButtonListener()
	{
		UIEventListener componentInChildren = objAPIncreaseService.GetComponentInChildren<UIEventListener>();
		if ((bool)componentInChildren)
		{
			componentInChildren.OnPressDown = delegate
			{
				if (ManagedSingleton<ServiceHelper>.Instance.GetServiceBonusValue(ServiceType.ResearchAPIncrease) != 0)
				{
					objAPRemainTimeTip.SetActive(true);
					string p_remainTimeText2;
					if (ManagedSingleton<ServiceHelper>.Instance.CurrentTable != null && ManagedSingleton<ServiceHelper>.Instance.GetServiceRemainTime(ManagedSingleton<ServiceHelper>.Instance.CurrentTable.n_ID, out p_remainTimeText2))
					{
						textAPRemainTime.text = p_remainTimeText2;
					}
				}
			};
			componentInChildren.OnPressUp = delegate
			{
				objAPRemainTimeTip.SetActive(false);
			};
		}
		UIEventListener componentInChildren2 = objTimeBoostService.GetComponentInChildren<UIEventListener>();
		if (!componentInChildren2)
		{
			return;
		}
		componentInChildren2.OnPressDown = delegate
		{
			if (ManagedSingleton<ServiceHelper>.Instance.GetServiceBonusValue(ServiceType.ResearchSpeedup) != 0)
			{
				objBoostRemainTimeTip.SetActive(true);
				string p_remainTimeText;
				if (ManagedSingleton<ServiceHelper>.Instance.CurrentTable != null && ManagedSingleton<ServiceHelper>.Instance.GetServiceRemainTime(ManagedSingleton<ServiceHelper>.Instance.CurrentTable.n_ID, out p_remainTimeText))
				{
					textBoostRemainTime.text = p_remainTimeText;
				}
			}
		};
		componentInChildren2.OnPressUp = delegate
		{
			objBoostRemainTimeTip.SetActive(false);
		};
	}
}
