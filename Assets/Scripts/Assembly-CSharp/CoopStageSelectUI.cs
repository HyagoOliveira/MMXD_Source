using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using enums;

internal class CoopStageSelectUI : OrangeUIBase
{
	private enum Condition
	{
		NONE = 0,
		AP = 1,
		RANK = 2,
		PRE = 3,
		COUNT = 4
	}

	public LoopVerticalScrollRect scrollRect;

	[SerializeField]
	private CoopStageSelectUIUnit unit;

	[SerializeField]
	private GameObject goRightPanel;

	[SerializeField]
	private GameObject goBottomPanel;

	[SerializeField]
	private OrangeText textStageTile;

	[SerializeField]
	private OrangeText textLvReq;

	[SerializeField]
	private OrangeText textCp;

	[SerializeField]
	private Image[] imgClear;

	[SerializeField]
	private OrangeText[] textClear;

	[SerializeField]
	private ItemIconBase rewardIcon;

	[SerializeField]
	private ItemIconWithAmount rewardIconWithAmount;

	[SerializeField]
	private RectTransform rewardParent;

	[SerializeField]
	private RectTransform starRoot;

	[SerializeField]
	private OrangeText textRewardCount;

	private StarConditionComponent starConditionComponent;

	[SerializeField]
	private Sprite[] sprImgClear;

	private Color[] clearTextColor = new Color[2]
	{
		new Color(13f / 15f, 13f / 15f, 0.8862745f),
		new Color(0.95686275f, 0.654902f, 1f / 85f)
	};

	[SerializeField]
	private BonusInfoSub bonusInfoSubMenu;

	[SerializeField]
	private Color[] bonusColor;

	[SerializeField]
	private BonusInfoTag bonusTag;

	[SerializeField]
	private RectTransform extraParent;

	[SerializeField]
	private OrangeText textExtra;

	[SerializeField]
	private Image imgCreateSingleRoom;

	private bool singlePlay;

	public bool IgnoreFristSE = true;

	private List<STAGE_TABLE> listStageData = new List<STAGE_TABLE>();

	private List<NetStageInfo> listNetStageInfo = new List<NetStageInfo>();

	private List<IconBase> listReward = new List<IconBase>();

	private CoopStageSelectUIUnit selectUnit;

	private int nowSelect;

	private OrangeBgExt m_bgExt;

	private int curren_stageRewardId;

	private OrangeScrollSePlayer scrollSePlayer;

	private readonly int displayMinCount = 4;

	private bool isConnecting;

	public int LinkSelectStageMainId { get; set; }

	public int LinkSelectStageSubId { get; set; }

	protected override void Awake()
	{
		base.Awake();
		LinkSelectStageMainId = -1;
		LinkSelectStageSubId = -1;
		scrollRect.ThresholdPlus = 30f;
	}

	public void Setup()
	{
		m_bgExt = Background as OrangeBgExt;
		listStageData = ManagedSingleton<OrangeTableHelper>.Instance.GetListStageByType(StageType.TeamUp);
		scrollSePlayer = scrollRect.content.GetComponent<OrangeScrollSePlayer>();
		scrollSePlayer.enabled = false;
		int lV = ManagedSingleton<PlayerHelper>.Instance.GetLV();
		textRewardCount.text = string.Format("{0}/{1}", ManagedSingleton<StageHelper>.Instance.GetCoopRewardCount(), OrangeConst.CORP_REWARD_COUNT);
		List<STAGE_TABLE> list = new List<STAGE_TABLE>();
		foreach (STAGE_TABLE listStageDatum in listStageData)
		{
			if (listStageDatum.n_RANK <= lV)
			{
				int num = 0;
				if (listStageDatum.s_PRE != "null")
				{
					num = int.Parse(listStageDatum.s_PRE);
				}
				if (num == 0 || ManagedSingleton<OrangeDataManager>.Instance.STAGE_TABLE_DICT.ContainsKey(num))
				{
					list.Add(listStageDatum);
				}
			}
		}
		listStageData = list;
		if (listStageData.Count < 1)
		{
			goRightPanel.SetActive(false);
			goBottomPanel.SetActive(false);
			return;
		}
		int selectIdx = 0;
		if (LinkSelectStageMainId != -1)
		{
			for (int i = 0; i < listStageData.Count; i++)
			{
				if (CheckSelectStageExist(listStageData[i]))
				{
					selectIdx = i;
					break;
				}
			}
		}
		MonoBehaviourSingleton<AssetsBundleManager>.Instance.LoadAssets(new string[1] { AssetBundleScriptableObject.Instance.m_iconStageBg }, delegate
		{
			MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad("UI/StarConditionComp", "StarConditionComp", delegate(GameObject obj)
			{
				GameObject gameObject = Object.Instantiate(obj, starRoot);
				starConditionComponent = gameObject.GetComponent<StarConditionComponent>();
			});
			scrollRect.OrangeInit(unit, listStageData.Count, listStageData.Count);
			scrollRect.ApplySnap(1, 0, selectIdx, OnBeginDragCB, OnEndDragCB);
		}, AssetsBundleManager.AssetKeepMode.KEEP_IN_SCENE, false);
		bonusInfoSubMenu.SetupInfo(listStageData[0].n_ID);
		bonusTag.Setup(bonusInfoSubMenu.dicBonusEvent);
		if (bonusTag.SetActive(true))
		{
			bonusTag.StartRolling();
		}
	}

	private bool CheckSelectStageExist(STAGE_TABLE stageTable)
	{
		if (stageTable.n_MAIN == LinkSelectStageMainId)
		{
			if (LinkSelectStageSubId == -1)
			{
				return true;
			}
			if (stageTable.n_SUB == LinkSelectStageSubId)
			{
				return true;
			}
		}
		return false;
	}

	private void OnBeginDragCB(object p_param)
	{
		RectTransform rectTransform = p_param as RectTransform;
		if (null != rectTransform)
		{
			CoopStageSelectUIUnit component = rectTransform.GetComponent<CoopStageSelectUIUnit>();
			if (null != component)
			{
				component.SetSubUnitData(component.StageData, component.NetStageInfo);
				LayoutRebuilder.ForceRebuildLayoutImmediate(scrollRect.content);
			}
			scrollSePlayer.enabled = true;
			scrollSePlayer.ResetDis();
		}
		imgCreateSingleRoom.raycastTarget = false;
	}

	private void OnEndDragCB(object p_param)
	{
		(p_param as RectTransform).GetComponent<CoopStageSelectUIUnit>().SetMainUnitData();
		if (!IgnoreFristSE)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK01);
		}
		else
		{
			IgnoreFristSE = false;
		}
		scrollSePlayer.enabled = false;
	}

	public void OnClickUnit(RectTransform p_rect)
	{
		if (nowSelect != p_rect.GetComponent<CoopStageSelectUIUnit>().NowIdx)
		{
			if (null != selectUnit)
			{
				selectUnit.SetSubUnitData(selectUnit.StageData, selectUnit.NetStageInfo);
			}
			scrollSePlayer.enabled = true;
			scrollSePlayer.ResetDis();
			scrollRect.OnTween(p_rect);
		}
	}

	public void SetData(CoopStageSelectUIUnit p_unit, ref int idx)
	{
		STAGE_TABLE sTAGE_TABLE = listStageData[idx];
		StageInfo value = null;
		ManagedSingleton<PlayerNetManager>.Instance.dicStage.TryGetValue(sTAGE_TABLE.n_ID, out value);
		p_unit.SetSubUnitData(sTAGE_TABLE, value);
	}

	public void Rebuild(CoopStageSelectUIUnit p_unit)
	{
		RectTransform content = scrollRect.content;
		for (int i = 0; i < content.childCount; i++)
		{
			CoopStageSelectUIUnit component = content.GetChild(i).GetComponent<CoopStageSelectUIUnit>();
			if (null != component && component != p_unit)
			{
				component.SetUnitActive(false, 1);
				component.SetUnitActive(true, 0);
			}
		}
		nowSelect = p_unit.NowIdx;
		selectUnit = p_unit;
		UpdateWindows();
	}

	private void UpdateWindows()
	{
		STAGE_TABLE sTAGE_TABLE = listStageData[nowSelect];
		StageInfo value = null;
		m_bgExt.ChangeBackground(sTAGE_TABLE.s_BG);
		bool flag = ManagedSingleton<PlayerNetManager>.Instance.dicStage.TryGetValue(sTAGE_TABLE.n_ID, out value);
		SetRewardList((flag && value.netStageInfo.Star > 0) ? sTAGE_TABLE.n_FIRST_REWARD : sTAGE_TABLE.n_GET_REWARD);
		textStageTile.text = ManagedSingleton<OrangeTextDataManager>.Instance.STAGETEXT_TABLE_DICT.GetL10nValue(sTAGE_TABLE.w_NAME);
		textLvReq.text = sTAGE_TABLE.n_RANK.ToString();
		textCp.text = sTAGE_TABLE.n_CP.ToString();
		SetExtraList(sTAGE_TABLE.n_ID);
		string[] stageClearMsg = ManagedSingleton<StageHelper>.Instance.GetStageClearMsg(sTAGE_TABLE);
		for (int i = 0; i < imgClear.Length; i++)
		{
			textClear[i].text = stageClearMsg[i];
			if (flag)
			{
				if ((value.netStageInfo.Star & (1 << i)) != 0)
				{
					imgClear[i].sprite = sprImgClear[1];
					textClear[i].color = clearTextColor[1];
				}
				else
				{
					imgClear[i].sprite = sprImgClear[0];
					textClear[i].color = clearTextColor[0];
				}
			}
			else
			{
				imgClear[i].sprite = sprImgClear[0];
				textClear[i].color = clearTextColor[0];
			}
		}
		starConditionComponent.Setup(1003, sTAGE_TABLE.n_MAIN);
		singlePlay = sTAGE_TABLE.n_SINGLEPLAY == 1;
		imgCreateSingleRoom.raycastTarget = true;
		imgCreateSingleRoom.color = (singlePlay ? Color.white : new Color(1f, 1f, 1f, 0.39f));
	}

	private void OnClickUnit(int p_idx)
	{
		GACHA_TABLE gACHA_TABLE = ManagedSingleton<ExtendDataHelper>.Instance.GetListGachaByGroup(curren_stageRewardId)[p_idx];
		switch ((RewardType)(short)gACHA_TABLE.n_REWARD_TYPE)
		{
		case RewardType.Item:
		{
			ITEM_TABLE item = null;
			if (ManagedSingleton<ExtendDataHelper>.Instance.ITEM_TABLE_DICT.TryGetValue(gACHA_TABLE.n_REWARD_ID, out item))
			{
				MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_ItemInfo", delegate(ItemInfoUI ui)
				{
					ui.CanShowHow2Get = false;
					ui.Setup(item);
				});
			}
			break;
		}
		case RewardType.Equipment:
		{
			EQUIP_TABLE equip = null;
			if (ManagedSingleton<OrangeDataManager>.Instance.EQUIP_TABLE_DICT.TryGetValue(gACHA_TABLE.n_REWARD_ID, out equip))
			{
				MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_ItemInfo", delegate(ItemInfoUI ui)
				{
					ui.CanShowHow2Get = false;
					ui.Setup(equip);
				});
			}
			break;
		}
		}
	}

	private void SetRewardList(int p_stageRewardId)
	{
		foreach (IconBase item in listReward)
		{
			Object.Destroy(item.gameObject);
		}
		listReward.Clear();
		curren_stageRewardId = p_stageRewardId;
		List<GACHA_TABLE> listGachaByGroup = ManagedSingleton<ExtendDataHelper>.Instance.GetListGachaByGroup(p_stageRewardId);
		int num = ((listGachaByGroup.Count > 4) ? listGachaByGroup.Count : displayMinCount);
		for (int i = 0; i < num; i++)
		{
			ItemIconBase itemIconBase = Object.Instantiate(rewardIcon, rewardParent);
			if (i >= listGachaByGroup.Count)
			{
				itemIconBase.Clear();
			}
			else
			{
				GACHA_TABLE gACHA_TABLE = listGachaByGroup[i];
				NetRewardInfo netGachaRewardInfo = new NetRewardInfo
				{
					RewardType = (sbyte)gACHA_TABLE.n_REWARD_TYPE,
					RewardID = gACHA_TABLE.n_REWARD_ID,
					Amount = gACHA_TABLE.n_AMOUNT_MAX
				};
				string bundlePath = string.Empty;
				string assetPath = string.Empty;
				int rare = 0;
				MonoBehaviourSingleton<OrangeGameManager>.Instance.GetRewardSpritePath(netGachaRewardInfo, ref bundlePath, ref assetPath, ref rare);
				itemIconBase.Setup(i, bundlePath, assetPath, OnClickUnit);
				itemIconBase.SetRare(rare);
			}
			listReward.Add(itemIconBase);
		}
	}

	public void OnClickRoomSelectBtn()
	{
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK17);
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_RoomSelect", delegate(RoomSelectUI ui)
		{
			ui.Setup(listStageData, nowSelect);
		});
	}

	public void OnClickCreateRoomBtn()
	{
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_CreateRoom", delegate(CreateRoomUI ui)
		{
			base.IsVisible = false;
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
			ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
			ui.SetupRoomTypeDouble(ref listStageData, nowSelect);
		});
	}

	public void OnClickCreateSingleRoom()
	{
		if (isConnecting)
		{
			return;
		}
		if (singlePlay)
		{
			isConnecting = true;
			if (MonoBehaviourSingleton<OrangeMatchManager>.Instance.Disconnect)
			{
				MonoBehaviourSingleton<OrangeMatchManager>.Instance.Host = ManagedSingleton<ServerConfig>.Instance.ServerSetting.Match.Host;
				MonoBehaviourSingleton<OrangeMatchManager>.Instance.Port = ManagedSingleton<ServerConfig>.Instance.ServerSetting.Match.Port;
				MonoBehaviourSingleton<OrangeMatchManager>.Instance.MatchServerLogin(delegate
				{
					isConnecting = false;
					OnClickCreateSingleRoom();
				});
			}
			else
			{
				PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_OK17);
				MonoBehaviourSingleton<OrangeMatchManager>.Instance.SingleMatch = true;
				MonoBehaviourSingleton<OrangeMatchManager>.Instance.CraeteCoopRoom(listStageData[nowSelect], false, "", "", 1, OnRSCreatePVEPrepareRoom);
			}
		}
		else
		{
			MonoBehaviourSingleton<OrangeGameManager>.Instance.ShowTipDialogByKey("RESTRICT_CORP_SINGLEPLAY");
		}
	}

	private void OnRSCreatePVEPrepareRoom(object res)
	{
		MonoBehaviourSingleton<OrangeMatchManager>.Instance.OnCreateRoomMainUI(res, listStageData[nowSelect], null);
		isConnecting = false;
	}

	public override void OnClickCloseBtn()
	{
		if (!isConnecting)
		{
			MonoBehaviourSingleton<OrangeMatchManager>.Instance.MatchServerLogout();
			base.OnClickCloseBtn();
		}
	}

	public void UpdateTab(int nowIdx)
	{
		scrollRect.OrangeInit(unit, listStageData.Count, listStageData.Count);
		scrollRect.ApplySnap(1, 0, nowIdx, OnBeginDragCB, OnEndDragCB);
	}

	private void SetExtraList(int p_stageId)
	{
		int itemID = 0;
		int itemCount = 0;
		int num = 3;
		string arg = string.Format("<color=#1EFE00>{0}</color>", ManagedSingleton<OrangeTextDataManager>.Instance.LOCALIZATION_TABLE_DICT.GetL10nValue("CORP_MISSION_OK"));
		string arg2 = string.Format("<color=#DE0000>{0}</color>", ManagedSingleton<OrangeTextDataManager>.Instance.LOCALIZATION_TABLE_DICT.GetL10nValue("CORP_MISSION_NG"));
		List<GameObject> list = new List<GameObject>();
		foreach (Transform item in extraParent)
		{
			list.Add(item.gameObject);
		}
		list.ForEach(delegate(GameObject child)
		{
			Object.Destroy(child);
		});
		List<MISSION_TABLE> list2 = ManagedSingleton<OrangeDataManager>.Instance.MISSION_TABLE_DICT.Values.Where((MISSION_TABLE x) => x.n_TYPE == 5 && x.n_SUB_TYPE == p_stageId).ToList();
		List<MISSION_TABLE> collection = ManagedSingleton<OrangeDataManager>.Instance.MISSION_TABLE_DICT.Values.Where((MISSION_TABLE x) => x.n_TYPE == 5 && x.n_SUB_TYPE == 0).ToList();
		list2.AddRange(collection);
		if (list2.Count == 0)
		{
			return;
		}
		foreach (MISSION_TABLE item2 in list2)
		{
			int num2 = ManagedSingleton<MissionHelper>.Instance.GetMissionProgressCount(item2.n_ID);
			string l10nValue = ManagedSingleton<OrangeTextDataManager>.Instance.MISSIONTEXT_TABLE_DICT.GetL10nValue(item2.w_TIP);
			for (int i = 0; i < num; i++)
			{
				GetItemDataByIndex(item2, i, ref itemID, ref itemCount);
				if (itemID != 0)
				{
					ItemIconWithAmount itemIconWithAmount = Object.Instantiate(rewardIconWithAmount, extraParent);
					OrangeText orangeText = Object.Instantiate(textExtra, itemIconWithAmount.transform);
					orangeText.transform.localPosition = new Vector3(85f, 0f, 0f);
					if (item2.n_LIMIT > 0)
					{
						num2 = ((num2 > item2.n_LIMIT) ? item2.n_LIMIT : num2);
						orangeText.text = string.Format("{0} ({1}/{2})", l10nValue, num2, item2.n_LIMIT);
					}
					else
					{
						orangeText.text = string.Format("{0}", l10nValue);
					}
					if (num2 > 0)
					{
						orangeText.text = string.Format("{0}{1}", arg, orangeText.text);
					}
					else
					{
						orangeText.text = string.Format("{0}{1}", arg2, orangeText.text);
					}
					NetRewardInfo netGachaRewardInfo = new NetRewardInfo
					{
						RewardType = 1,
						RewardID = itemID,
						Amount = 1
					};
					string bundlePath = string.Empty;
					string assetPath = string.Empty;
					int rare = 0;
					MonoBehaviourSingleton<OrangeGameManager>.Instance.GetRewardSpritePath(netGachaRewardInfo, ref bundlePath, ref assetPath, ref rare);
					itemIconWithAmount.Setup(itemID, bundlePath, assetPath, OnClickExtraUnit);
					itemIconWithAmount.SetRare(rare);
					itemIconWithAmount.SetAmount(itemCount);
				}
			}
		}
	}

	private void GetItemDataByIndex(MISSION_TABLE missionTable, int index, ref int itemID, ref int itemCount)
	{
		switch (index)
		{
		case 0:
			itemID = missionTable.n_ITEMID_1;
			itemCount = missionTable.n_ITEMCOUNT_1;
			break;
		case 1:
			itemID = missionTable.n_ITEMID_2;
			itemCount = missionTable.n_ITEMCOUNT_2;
			break;
		case 2:
			itemID = missionTable.n_ITEMID_3;
			itemCount = missionTable.n_ITEMCOUNT_3;
			break;
		}
	}

	private void OnClickExtraUnit(int itemID)
	{
		ITEM_TABLE item = null;
		if (ManagedSingleton<ExtendDataHelper>.Instance.ITEM_TABLE_DICT.TryGetValue(itemID, out item))
		{
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_ItemInfo", delegate(ItemInfoUI ui)
			{
				ui.CanShowHow2Get = false;
				ui.Setup(item);
			});
		}
	}
}
