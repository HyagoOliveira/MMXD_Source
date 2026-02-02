#define RELEASE
using System;
using System.Collections.Generic;
using System.Linq;
using CallbackDefs;
using DragonBones;
using OrangeApi;
using UnityEngine;
using UnityEngine.UI;
using enums;

public class Minigame01UI : OrangeUIBase
{
	public enum RewardT
	{
		ResultReward = 0,
		RankingReward = 1
	}

	public enum SectionT
	{
		BastResult = 0,
		NormalResult = 3,
		Rank1 = 0,
		Rank2 = 1,
		Rank3 = 2,
		Rank4 = 3,
		RankOther = 4
	}

	public enum ColorT
	{
		ToggleEnable = 0,
		ToggleDisable = 1,
		Rank1 = 2,
		Rank2 = 3,
		Rank3 = 4,
		Rank4 = 5,
		Green = 6,
		Orange = 7,
		Gray = 8,
		Blue = 9
	}

	public class CListNode
	{
		public ITEM_TABLE itemTbl;

		public LABOEVENT_TABLE table;

		public int cnt;

		public int amount;

		public string section;

		public int sectionIdx;

		public CListNode()
		{
			table = null;
			itemTbl = null;
			cnt = 0;
			amount = 0;
			section = "";
			sectionIdx = 0;
		}
	}

	[SerializeField]
	public Image[] materialIcons;

	[SerializeField]
	public OrangeText[] materialNames;

	[SerializeField]
	public OrangeText[] materialNumber;

	[SerializeField]
	public OrangeText[] EventTimeInfo;

	[SerializeField]
	public Button inputMaterial;

	[SerializeField]
	public Button rule;

	[SerializeField]
	public Button reword;

	[SerializeField]
	public Button shop;

	[SerializeField]
	public Color[] typeColor;

	[SerializeField]
	private UnityArmatureComponent LabEvent_Effect;

	[SerializeField]
	private OrangeText EventTimes;

	[SerializeField]
	private UnityEngine.Transform RankPanelMiniRoot;

	[Header("InputMaterialSubDialog")]
	[SerializeField]
	public GameObject materialSelectDialog;

	[SerializeField]
	public OrangeText selectMaterialName;

	[SerializeField]
	public OrangeText importMaterialCount;

	[SerializeField]
	private Mini01Material[] subMaterial;

	[SerializeField]
	public Slider subCountSlider;

	[SerializeField]
	public Button subSelectedOKBtn;

	[SerializeField]
	private GameObject normalResultRoot;

	[SerializeField]
	private RewardPopupUIUnit rewardUnit;

	[Header("RewordSubDialog")]
	[SerializeField]
	public GameObject rewordDialog;

	[SerializeField]
	public LoopVerticalScrollRect lvsRect;

	[SerializeField]
	public Toggle[] rewordType;

	[SerializeField]
	public OrangeText[] rewordTypeText;

	[SerializeField]
	public GameObject btmTitle;

	[SerializeField]
	public OrangeText tempRankingNum;

	[SerializeField]
	public OrangeText dateTime;

	[SerializeField]
	public RectTransform lvsContent;

	[SerializeField]
	public GameObject m_cellSample;

	[SerializeField]
	private GameObject initIMG;

	private NetRewardInfo nReword = new NetRewardInfo();

	private RankPanelMini m_RankPanelMini;

	private int m_SeasonID;

	private List<EventRankingInfo> eventRankingInfoList = new List<EventRankingInfo>();

	private EventRankingInfo nEventRankingInfo;

	private Image[] rewordImages;

	private int impSelIdx;

	[HideInInspector]
	public List<CListNode> m_nodeList = new List<CListNode>();

	[HideInInspector]
	public List<CListNode> m_rankingNodeList = new List<CListNode>();

	private int[] labConst = new int[4]
	{
		OrangeConst.LABOEVENT_ITEM_1,
		OrangeConst.LABOEVENT_ITEM_2,
		OrangeConst.LABOEVENT_ITEM_3,
		OrangeConst.LABOEVENT_ITEM_4
	};

	[HideInInspector]
	public List<ITEM_TABLE> itemsList = new List<ITEM_TABLE>();

	[HideInInspector]
	public List<int> inportNumList = new List<int> { 0, 0, 0, 0 };

	[HideInInspector]
	public List<int> inportMaxList = new List<int> { 0, 0, 0, 0 };

	private List<EVENT_TABLE> labEventTbl = new List<EVENT_TABLE>();

	private List<MISSION_TABLE> rewordTbl = new List<MISSION_TABLE>();

	private List<LABOEVENT_TABLE> laboTbl = new List<LABOEVENT_TABLE>();

	[HideInInspector]
	public string[] localStringKeys = new string[5] { "LABOEVENT_RESULT_NORMAL", "LABOEVENT_RESULT_BEST", "UI_OWNED", "LABOEVENT_ITEM_AMOUNT", "EVENT_STAGE_RANKING" };

	private int totalInportCount;

	private bool initOK;

	private int currentRewardID;

	private int SliderValue
	{
		get
		{
			return (int)subCountSlider.value;
		}
		set
		{
			subCountSlider.onValueChanged.RemoveAllListeners();
			subCountSlider.value = value;
			subCountSlider.onValueChanged.AddListener(OnSliderChanging);
		}
	}

	public void Setup()
	{
		rewordImages = initIMG.GetComponentsInChildren<Image>();
		initOK = false;
		long serverUnixTimeNowUTC = MonoBehaviourSingleton<OrangeGameManager>.Instance.ServerUnixTimeNowUTC;
		labEventTbl = ManagedSingleton<ExtendDataHelper>.Instance.GetEventTableByType(enums.EventType.EVENT_LABO, serverUnixTimeNowUTC);
		if (labEventTbl == null || labEventTbl.Count == 0)
		{
			labEventTbl = ManagedSingleton<ExtendDataHelper>.Instance.GetEventTableRemainByType(enums.EventType.EVENT_LABO, serverUnixTimeNowUTC);
		}
		if (labEventTbl == null || labEventTbl.Count == 0)
		{
			return;
		}
		EVENT_TABLE etbl = labEventTbl[0];
		m_SeasonID = etbl.n_ID;
		laboTbl = (from p in ManagedSingleton<OrangeDataManager>.Instance.LABOEVENT_TABLE_DICT
			where p.Value.n_GROUP == etbl.n_TYPE_X
			select p into s
			select s.Value).ToList();
		rewordTbl = (from p in ManagedSingleton<OrangeDataManager>.Instance.MISSION_TABLE_DICT
			where p.Value.n_TYPE == 105 && p.Value.n_SUB_TYPE == etbl.n_RANKING
			select p into s
			select s.Value).ToList();
		itemsList = (from p in ManagedSingleton<ExtendDataHelper>.Instance.ITEM_TABLE_DICT
			where labConst.Any((int a) => a == p.Value.n_ID)
			select p into o
			select o.Value).ToList();
		inportNumList = new List<int> { 0, 0, 0, 0 };
		btmTitle.SetActive(false);
		initRewordNodes();
		totalInportCount = 0;
		for (int i = 0; i < 4; i++)
		{
			setItemImage(i);
			setItemString(i);
			inportNumList[i] = 0;
		}
		EventTimes.text = OrangeGameUtility.DisplayDatePeriod(etbl.s_BEGIN_TIME, etbl.s_END_TIME);
		IsEventOpening();
	}

	private void ShowEventOutDate()
	{
		if (inputMaterial.gameObject.activeSelf)
		{
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_Tip", delegate(TipUI tipUI)
			{
				string str = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("EVENT_OUTDATE");
				tipUI.Setup(str, true);
			});
		}
	}

	private bool IsEventOpening()
	{
		long serverUnixTimeNowUTC = MonoBehaviourSingleton<OrangeGameManager>.Instance.ServerUnixTimeNowUTC;
		bool num = ManagedSingleton<OrangeTableHelper>.Instance.IsOpeningDate(labEventTbl[0].s_BEGIN_TIME, labEventTbl[0].s_END_TIME, serverUnixTimeNowUTC);
		if (!num)
		{
			ShowEventOutDate();
			inputMaterial.gameObject.SetActive(false);
			return num;
		}
		inputMaterial.gameObject.SetActive(true);
		return num;
	}

	public Color GetColor(ColorT ct)
	{
		return typeColor[(int)ct];
	}

	public Color GetSpriteType(RewardT rt, SectionT st, out Sprite sct, out Sprite fg, out Sprite bg)
	{
		int num = 0;
		sct = (fg = (bg = rewordImages[0].sprite));
		num = (int)st;
		if (num >= 4)
		{
			num = 3;
		}
		sct = rewordImages[num].sprite;
		fg = rewordImages[num + 8].sprite;
		bg = rewordImages[num + 4].sprite;
		return typeColor[num + 2];
	}

	private void setItemImage(int idx)
	{
		ITEM_TABLE iTEM_TABLE = itemsList[idx];
		MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(AssetBundleScriptableObject.Instance.GetIconItem(iTEM_TABLE.s_ICON), iTEM_TABLE.s_ICON, delegate(Sprite obj)
		{
			if ((bool)obj)
			{
				materialIcons[idx].sprite = obj;
			}
		});
	}

	private void setItemString(int idx)
	{
		ITEM_TABLE iTEM_TABLE = itemsList[idx];
		materialNames[idx].text = ManagedSingleton<OrangeTextDataManager>.Instance.ITEMTEXT_TABLE_DICT.GetL10nValue(iTEM_TABLE.w_NAME);
		ItemInfo value = null;
		string str = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("UI_OWNED");
		if (ManagedSingleton<PlayerNetManager>.Instance.dicItem.TryGetValue(iTEM_TABLE.n_ID, out value))
		{
			materialNumber[idx].text = str + " " + value.netItemInfo.Stack;
			totalInportCount += value.netItemInfo.Stack;
		}
		else
		{
			materialNumber[idx].text = str + " 0";
		}
	}

	public void updateUserItems()
	{
		totalInportCount = 0;
		for (int i = 0; i < 4; i++)
		{
			setItemString(i);
		}
	}

	private void addRankNode(int itemId, int Num, string title)
	{
		ITEM_TABLE value;
		if (!ManagedSingleton<ExtendDataHelper>.Instance.ITEM_TABLE_DICT.TryGetValue(itemId, out value))
		{
			return;
		}
		int count = m_rankingNodeList.Count;
		CListNode cListNode = new CListNode();
		if (count > 0)
		{
			CListNode cListNode2 = m_rankingNodeList[count - 1];
			if (title != cListNode2.section)
			{
				cListNode.sectionIdx = cListNode2.sectionIdx + 1;
				if (count % 2 == 1)
				{
					cListNode.section = cListNode2.section;
					cListNode.sectionIdx = cListNode2.sectionIdx;
					m_rankingNodeList.Add(cListNode);
					cListNode = new CListNode();
					cListNode.sectionIdx = cListNode2.sectionIdx + 1;
				}
			}
		}
		cListNode.itemTbl = value;
		cListNode.amount = Num;
		cListNode.section = title;
		m_rankingNodeList.Add(cListNode);
	}

	private void initRewordNodes()
	{
		m_rankingNodeList.Clear();
		m_nodeList.Clear();
		string tmpRank = "";
		MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(AssetBundleScriptableObject.Instance.m_uiPath + "UI_RankPanelMini", "UI_RankPanelMini", delegate(GameObject asset)
		{
			ManagedSingleton<PlayerNetManager>.Instance.GetLaboEventRankingReq(m_SeasonID, 1, 10, delegate(GetLaboEventRankingRes res)
			{
				if (res.ReturnCode == 75001)
				{
					while (RankPanelMiniRoot.childCount != 0)
					{
						UnityEngine.Object.DestroyImmediate(RankPanelMiniRoot.GetChild(0).gameObject);
					}
					GameObject gameObject = UnityEngine.Object.Instantiate(asset, RankPanelMiniRoot);
					m_RankPanelMini = gameObject.GetComponent<RankPanelMini>();
					eventRankingInfoList = res.EventRankingList;
					nEventRankingInfo = res.EventRanking;
					m_RankPanelMini.gameObject.SetActive(true);
					m_RankPanelMini.Setup(eventRankingInfoList, m_SeasonID);
				}
			});
		});
		CListNode prvNode;
		CListNode nd;
		ITEM_TABLE itemTbl;
		laboTbl.ForEach(delegate(LABOEVENT_TABLE tbl)
		{
			int count = m_nodeList.Count;
			tmpRank = ((tbl.n_ITEM_LIMIT == 0) ? localStringKeys[0] : localStringKeys[1]);
			if (count > 0)
			{
				prvNode = m_nodeList[count - 1];
				if (tmpRank != prvNode.section && count % 2 == 1)
				{
					nd = new CListNode();
					nd.section = prvNode.section;
					m_nodeList.Add(nd);
				}
			}
			if (ManagedSingleton<ExtendDataHelper>.Instance.ITEM_TABLE_DICT.TryGetValue(tbl.n_ITEM, out itemTbl))
			{
				nd = new CListNode();
				nd.itemTbl = itemTbl;
				if (tbl.n_ITEM_LIMIT != 0)
				{
					nd.cnt = ManagedSingleton<MissionHelper>.Instance.GetMissionCounter(tbl.n_COUNTER);
				}
				nd.amount = tbl.n_ITEM_COUNT;
				nd.table = tbl;
				nd.section = tmpRank;
				m_nodeList.Add(nd);
			}
		});
		rewordTbl.ForEach(delegate(MISSION_TABLE tbl)
		{
			tmpRank = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr(localStringKeys[4]) + "{0}-", tbl.n_CONDITION_Y);
			if (tbl.n_CONDITION_Z != -1)
			{
				tmpRank += tbl.n_CONDITION_Z;
			}
			if (tbl.n_ITEMID_1 != 0)
			{
				addRankNode(tbl.n_ITEMID_1, tbl.n_ITEMCOUNT_1, tmpRank);
			}
			if (tbl.n_ITEMID_2 != 0)
			{
				addRankNode(tbl.n_ITEMID_2, tbl.n_ITEMCOUNT_2, tmpRank);
			}
			if (tbl.n_ITEMID_3 != 0)
			{
				addRankNode(tbl.n_ITEMID_3, tbl.n_ITEMCOUNT_3, tmpRank);
			}
		});
	}

	public void ResizeRewardMask()
	{
	}

	private void OnEnable()
	{
		Singleton<GenericEventManager>.Instance.AttachEvent(EventManager.ID.LIBRARY_UPDATE_MAIN_UI, UpdateMainUI);
		Singleton<GenericEventManager>.Instance.AttachEvent(EventManager.ID.SWITCH_SCENE, Clear);
	}

	private void OnDisable()
	{
		Singleton<GenericEventManager>.Instance.DetachEvent(EventManager.ID.LIBRARY_UPDATE_MAIN_UI, UpdateMainUI);
		Singleton<GenericEventManager>.Instance.DetachEvent(EventManager.ID.SWITCH_SCENE, Clear);
	}

	private void Clear()
	{
		OnClickCloseBtn();
	}

	private void UpdateMainUI()
	{
		IsEventOpening();
		initRewordNodes();
		updateUserItems();
	}

	public void OnRewordClick()
	{
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
		rewordDialog.SetActive(true);
		OnToggleChange(0);
	}

	public void OnShopClick()
	{
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK17);
		ManagedSingleton<UILinkHelper>.Instance.LoadUI(36, delegate
		{
		});
	}

	public void OnRewordCloseClick()
	{
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL);
		rewordDialog.SetActive(false);
		lvsRect.totalCount = 0;
		IsEventOpening();
		initRewordNodes();
	}

	public void ShowSubMenu(string pid, string pname, Vector3 wposition)
	{
	}

	private void Start()
	{
	}

	private void OnDestroy()
	{
	}

	private void CheckImportMax()
	{
		int num = 0;
		for (int i = 0; i < 4; i++)
		{
			ITEM_TABLE iTEM_TABLE = itemsList[i];
			ItemInfo value;
			if (ManagedSingleton<PlayerNetManager>.Instance.dicItem.TryGetValue(iTEM_TABLE.n_ID, out value))
			{
				num = value.netItemInfo.Stack;
			}
			if (num > OrangeConst.LABOEVENT_ITEM_MAX)
			{
				num = OrangeConst.LABOEVENT_ITEM_MAX;
			}
			inportMaxList[i] = num;
			if (inportNumList[i] > num)
			{
				inportNumList[i] = num;
			}
		}
	}

	private void setupImportItems()
	{
		for (int i = 0; i < 4; i++)
		{
			ITEM_TABLE iTEM_TABLE = itemsList[i];
			subMaterial[i].Icon = iTEM_TABLE.s_ICON;
		}
		OnSelectMaterial(0);
	}

	public void updateImportItems()
	{
		totalInportCount = 0;
		for (int i = 0; i < 4; i++)
		{
			subMaterial[i].Selected = i == impSelIdx;
			totalInportCount += inportNumList[i];
			subMaterial[i].Count = inportNumList[i];
		}
		selectMaterialName.text = ManagedSingleton<OrangeTextDataManager>.Instance.ITEMTEXT_TABLE_DICT.GetL10nValue(itemsList[impSelIdx].w_NAME);
		importMaterialCount.text = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr(localStringKeys[3]), inportNumList[impSelIdx], inportMaxList[impSelIdx]);
		subCountSlider.onValueChanged.RemoveAllListeners();
		subCountSlider.maxValue = inportMaxList[impSelIdx];
		subCountSlider.value = inportNumList[impSelIdx];
		subCountSlider.onValueChanged.AddListener(OnSliderChanging);
		subSelectedOKBtn.interactable = totalInportCount > 0;
	}

	public void OnClickImportMaterialBtn()
	{
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
		CheckImportMax();
		impSelIdx = 0;
		setupImportItems();
		materialSelectDialog.SetActive(true);
	}

	private void OnSliderChanging(float v)
	{
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK03);
		inportNumList[impSelIdx] = (int)subCountSlider.value;
		updateImportItems();
	}

	public void OnSelectMaterial(int idx)
	{
		if (impSelIdx != idx)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK01);
		}
		impSelIdx = idx;
		updateImportItems();
	}

	public void OnAddImportClick()
	{
		int num = inportNumList[impSelIdx];
		if (num < inportMaxList[impSelIdx])
		{
			inportNumList[impSelIdx] = num + 1;
			updateImportItems();
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK03);
		}
	}

	public void OnDelImportClick()
	{
		int num = inportNumList[impSelIdx];
		if (num > 0)
		{
			inportNumList[impSelIdx] = num - 1;
			updateImportItems();
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK03);
		}
	}

	public void OnAddMaxImportClick()
	{
		if (inportNumList[impSelIdx] < inportMaxList[impSelIdx])
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK03);
		}
		inportNumList[impSelIdx] = inportMaxList[impSelIdx];
		updateImportItems();
	}

	public void OnDelMaxImportClick()
	{
		if (inportNumList[impSelIdx] > 0)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK03);
		}
		inportNumList[impSelIdx] = 0;
		updateImportItems();
	}

	private void OnClickUnit(int p_idx)
	{
		switch ((RewardType)nReword.RewardType)
		{
		case RewardType.Item:
		{
			ITEM_TABLE item = null;
			if (ManagedSingleton<ExtendDataHelper>.Instance.ITEM_TABLE_DICT.TryGetValue(nReword.RewardID, out item))
			{
				MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_ItemInfo", delegate(ItemInfoUI ui)
				{
					ui.CanShowHow2Get = false;
					ui.Setup(item);
				});
			}
			break;
		}
		case RewardType.Character:
		{
			CHARACTER_TABLE character = null;
			if (ManagedSingleton<OrangeDataManager>.Instance.CHARACTER_TABLE_DICT.TryGetValue(nReword.RewardID, out character))
			{
				MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_ItemInfo", delegate(ItemInfoUI ui)
				{
					ui.CanShowHow2Get = false;
					ui.Setup(character);
				});
			}
			break;
		}
		case RewardType.Weapon:
		{
			WEAPON_TABLE weapon = null;
			if (ManagedSingleton<OrangeDataManager>.Instance.WEAPON_TABLE_DICT.TryGetValue(nReword.RewardID, out weapon))
			{
				MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_ItemInfo", delegate(ItemInfoUI ui)
				{
					ui.CanShowHow2Get = false;
					ui.Setup(weapon);
				});
			}
			break;
		}
		}
	}

	public void OnImportOkClick()
	{
		if (!IsEventOpening())
		{
			initRewordNodes();
			materialSelectDialog.SetActive(false);
			return;
		}
		LABOEVENT_TABLE labRec = laboTbl.Find((LABOEVENT_TABLE tbl) => tbl.n_MIN_1 <= inportNumList[0] && inportNumList[0] <= tbl.n_MAX_1 && tbl.n_MIN_2 <= inportNumList[1] && inportNumList[1] <= tbl.n_MAX_2 && tbl.n_MIN_3 <= inportNumList[2] && inportNumList[2] <= tbl.n_MAX_3 && tbl.n_MIN_4 <= inportNumList[3] && inportNumList[3] <= tbl.n_MAX_4);
		if (labRec == null)
		{
			Debug.LogError("");
			updateUserItems();
			return;
		}
		ManagedSingleton<MissionHelper>.Instance.GetMissionCounter(labRec.n_COUNTER);
		ManagedSingleton<PlayerNetManager>.Instance.ComposeLaboEventReq(labRec.n_ID, inportNumList, delegate(ComposeLaboEventRes res)
		{
			if (res.ReturnCode != 75000)
			{
				UnityEngine.Debug.LogError("LOBOEVENT_GET_LABOEVENT_TABLE_FAIL");
				ShowEventOutDate();
				initRewordNodes();
				materialSelectDialog.SetActive(false);
			}
			else
			{
				MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK05);
				MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_RewardPopup", delegate(RewardPopopUI ui)
				{
					if (labRec.n_ITEM_LIMIT != 0)
					{
						ui.titleText.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("LABOEVENT_RESULT_BEST");
						ui.titleText.fontStyle = FontStyle.Italic;
					}
					else
					{
						ui.normalReward.SetActive(true);
						ui.orgDbPos.SetActive(false);
					}
					ui.closeCB = (Callback)Delegate.Combine(ui.closeCB, (Callback)delegate
					{
						IsEventOpening();
						initRewordNodes();
					});
					ui.Setup(res.RewardEntities.RewardList);
					updateUserItems();
				});
			}
		});
		materialSelectDialog.SetActive(false);
	}

	public void onNormalResultOKClick()
	{
		normalResultRoot.SetActive(false);
		updateUserItems();
	}

	public void OnToggleChange(int idx)
	{
		if (idx == 0 && rewordType[idx].isOn)
		{
			if (currentRewardID != idx)
			{
				MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_CURSOR07);
				currentRewardID = idx;
			}
			btmTitle.SetActive(false);
			lvsRect.totalCount = m_nodeList.Count / 2;
			lvsRect.totalCount += m_nodeList.Count & 1;
			lvsRect.RefillCells();
			rewordTypeText[0].color = typeColor[0];
			rewordTypeText[1].color = typeColor[1];
		}
		if (idx != 1 || !rewordType[idx].isOn)
		{
			return;
		}
		if (currentRewardID != idx)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_CURSOR07);
			currentRewardID = idx;
		}
		tempRankingNum.text = nEventRankingInfo.Ranking.ToString();
		ManagedSingleton<OrangeTableHelper>.Instance.ServerDateToUTC(labEventTbl[0].s_BEGIN_TIME);
		long num = ManagedSingleton<OrangeTableHelper>.Instance.ServerDateToUTC(labEventTbl[0].s_END_TIME);
		long timestamp = nEventRankingInfo.Score;
		long serverUnixTimeNowUTC = MonoBehaviourSingleton<OrangeGameManager>.Instance.ServerUnixTimeNowUTC;
		dateTime.text = "";
		if (nEventRankingInfo.Ranking != 0 && num > serverUnixTimeNowUTC)
		{
			bool bAllClear = true;
			m_nodeList.ForEach(delegate(CListNode node)
			{
				if (node.section == localStringKeys[1] && node.table != null && node.cnt != node.table.n_ITEM_LIMIT)
				{
					bAllClear = false;
				}
			});
			if (bAllClear)
			{
				dateTime.text = OrangeGameUtility.GetRemainTimeTextDetail(timestamp);
			}
		}
		btmTitle.SetActive(true);
		lvsRect.totalCount = m_rankingNodeList.Count / 2;
		lvsRect.totalCount += m_rankingNodeList.Count & 1;
		lvsRect.RefillCells();
		rewordTypeText[0].color = typeColor[1];
		rewordTypeText[1].color = typeColor[0];
	}

	public void OnImportDialogCloseClick()
	{
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL);
		if (!IsEventOpening())
		{
			initRewordNodes();
		}
		materialSelectDialog.SetActive(false);
	}

	public void OnClickRules()
	{
		string titleStr = null;
		string ruleStr = null;
		titleStr = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_RULE");
		ruleStr = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("LABOEVENT_RULE");
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_CommonScrollMsg", delegate(CommonScrollMsgUI ui)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
			ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
			ui.closeCB = (Callback)Delegate.Combine(ui.closeCB, (Callback)delegate
			{
				IsEventOpening();
				initRewordNodes();
			});
			ui.Setup(titleStr, ruleStr);
		});
	}
}
