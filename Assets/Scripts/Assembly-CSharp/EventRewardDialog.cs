#define RELEASE
using System.Collections.Generic;
using System.Linq;
using CallbackDefs;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.UI;
using enums;

public class EventRewardDialog : OrangeUIBase
{
	public enum TabType
	{
		Score = 0,
		Rank = 1,
		Stage = 2,
		Gacha = 3,
		SpeedPower = 4,
		SpeedChallenge = 5,
		DeepRecord = 6,
		none = 255
	}

	private enum RANK
	{
		R1 = 0,
		R2 = 1,
		R3 = 2,
		R4 = 3,
		MAX = 4
	}

	[SerializeField]
	private Transform m_itemBarScoreRef;

	[SerializeField]
	private Transform m_itemBarStageRef;

	[SerializeField]
	private Transform[] m_itemBarRankRef;

	[SerializeField]
	private Transform[] m_labelRankRef;

	[SerializeField]
	private RectTransform m_scrollViewContentRect;

	[SerializeField]
	private Scrollbar m_verticalScrollbar;

	[SerializeField]
	private Transform m_scoreRankParent;

	[SerializeField]
	private OrangeText m_scoreRankLabel;

	[SerializeField]
	private OrangeText m_scoreRankText;

	[SerializeField]
	private OrangeText m_rewardNoteText;

	[SerializeField]
	private OrangeText m_rewardNoteSpeedText;

	[SerializeField]
	private Button m_getAllBtn;

	[SerializeField]
	private Toggle[] m_rewardToggles;

	[SerializeField]
	private GameObject m_eventTimeRoot;

	[SerializeField]
	private OrangeText m_eventTimeText;

	[Header("Basic Reward")]
	[SerializeField]
	private Transform m_basicRewardGroup;

	[Header("Box Gacha")]
	[SerializeField]
	private Transform m_boxGachaGroup;

	[SerializeField]
	private RectTransform m_scrollViewGachaList;

	[SerializeField]
	private OrangeText m_phaseNumber;

	[SerializeField]
	private OrangeText m_dialogText;

	[SerializeField]
	private OrangeText m_captionDrawX1;

	[SerializeField]
	private OrangeText m_captionDrawX10;

	[SerializeField]
	private OrangeText m_coinPerDrawX1;

	[SerializeField]
	private OrangeText m_coinPerDrawX10;

	[SerializeField]
	private OrangeText m_coinOwned;

	[SerializeField]
	private Button m_drawBtnx1;

	[SerializeField]
	private Button m_drawBtnx10;

	[SerializeField]
	private Button m_gachaResetBtn;

	[SerializeField]
	private Image[] m_coinIcons;

	[SerializeField]
	private OrangeText m_itemsLeft;

	private EventRankingInfo m_eventRankingInfo;

	private BOXGACHA_TABLE m_currentBoxGachaTable;

	private int m_boxGachaTargetRewardID;

	private bool m_bBoxGachaTargetRewardObtained;

	private List<NetBoxGachaRecord> m_netBoxGachaRecords;

	private int m_currentPhase;

	private int m_maxPhaseCount;

	private int m_lastPhaseMaxDrawCount = 100;

	private int m_defaultMaxDrawCount = 100;

	private bool m_bAlignLeft = true;

	private bool m_bScoreItemCollectable;

	private Vector2 m_currentPos = new Vector2(0f, 0f);

	private Vector2 m_spacing = new Vector2(40f, 7f);

	private EVENT_TABLE m_eventTable;

	private STAGE_TABLE m_stageTable;

	private int m_rewardAvailableCount;

	private int m_multiDrawCountAvailable;

	private bool m_bAllRewardsObtained;

	private TabType m_currentSelectedTab = TabType.none;

	[BoxGroup("Sound")]
	[SerializeField]
	private SystemSE m_clickTapSE;

	[BoxGroup("Sound")]
	[SerializeField]
	private SystemSE m_getItem;

	[BoxGroup("Sound")]
	[SerializeField]
	private SystemSE m_getAllItem;

	private List<NetRewardInfo> rewardList = new List<NetRewardInfo>();

	private bool bSpeedEventRanking;

	private bool bSpeedRewardEvent;

	private int SpeedRewardEventSub = 1;

	private int CurrentSpeedPowerRank;

	private int CurrentSpeedChallengeRank;

	private MissionType SpeedEventType = MissionType.TAStageRanking;

	private int deepRecordEventType = 114;

	public void Setup(EVENT_TABLE eventTable, STAGE_TABLE stageTable, EventRankingInfo eventRankingInfo)
	{
		m_eventTable = eventTable;
		m_stageTable = stageTable;
		m_eventRankingInfo = eventRankingInfo;
		List<Toggle> list = new List<Toggle>();
		Vector3 localPosition = m_rewardToggles[3].transform.localPosition;
		float num = 222f;
		m_gachaResetBtn.gameObject.SetActive(false);
		if (eventTable.n_BOXGACHA != 0)
		{
			m_rewardToggles[3].gameObject.SetActive(true);
			m_rewardToggles[3].transform.localPosition = new Vector3(localPosition.x + num * (float)list.Count, localPosition.y, localPosition.z);
			list.Add(m_rewardToggles[3]);
		}
		if (eventTable.n_POINT != 0)
		{
			m_rewardToggles[0].gameObject.SetActive(true);
			m_rewardToggles[0].transform.localPosition = new Vector3(localPosition.x + num * (float)list.Count, localPosition.y, localPosition.z);
			list.Add(m_rewardToggles[0]);
		}
		if (eventTable.n_RANKING != 0)
		{
			m_rewardToggles[1].gameObject.SetActive(true);
			m_rewardToggles[1].transform.localPosition = new Vector3(localPosition.x + num * (float)list.Count, localPosition.y, localPosition.z);
			list.Add(m_rewardToggles[1]);
		}
		if (ManagedSingleton<ExtendDataHelper>.Instance.GetListGachaByGroup(m_stageTable.n_GET_REWARD).Count > 0)
		{
			m_rewardToggles[2].gameObject.SetActive(true);
			m_rewardToggles[2].transform.localPosition = new Vector3(localPosition.x + num * (float)list.Count, localPosition.y, localPosition.z);
			list.Add(m_rewardToggles[2]);
		}
		if (list.Count > 0)
		{
			list[0].isOn = true;
		}
		base.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
		base._EscapeEvent = EscapeEvent.CLOSE_UI;
	}

	private void ClearScrollView()
	{
		foreach (Transform item in m_scrollViewContentRect)
		{
			Object.Destroy(item.gameObject);
		}
		m_bAlignLeft = true;
		m_currentPos = new Vector2(0f, 0f);
		m_verticalScrollbar.value = 1f;
	}

	private void OnClickItem(int p_idx)
	{
		ITEM_TABLE item = null;
		if (!ManagedSingleton<OrangeTableHelper>.Instance.GetItem(p_idx, out item))
		{
			return;
		}
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_ItemInfo", delegate(ItemInfoUI ui)
		{
			ui.CanShowHow2Get = false;
			if (item.n_TYPE == 5 && item.n_TYPE_X == 1 && (int)item.f_VALUE_Y > 0)
			{
				CARD_TABLE value = null;
				if (ManagedSingleton<OrangeDataManager>.Instance.CARD_TABLE_DICT.TryGetValue((int)item.f_VALUE_Y, out value))
				{
					PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
					ui.Setup(value, item);
				}
			}
			else
			{
				ui.Setup(item);
			}
		});
	}

	private string ItemIconHelper(Transform target, int itemId, int amount)
	{
		ItemIconWithAmount componentInChildren = target.GetComponentInChildren<ItemIconWithAmount>();
		ITEM_TABLE item;
		if (ManagedSingleton<OrangeTableHelper>.Instance.GetItem(itemId, out item))
		{
			if (item.n_TYPE == 5 && item.n_TYPE_X == 1 && (int)item.f_VALUE_Y > 0)
			{
				float f_VALUE_Y = item.f_VALUE_Y;
				CARD_TABLE value = null;
				if (ManagedSingleton<OrangeDataManager>.Instance.CARD_TABLE_DICT.TryGetValue((int)item.f_VALUE_Y, out value))
				{
					string p_bundleName = AssetBundleScriptableObject.Instance.m_iconCard + string.Format(AssetBundleScriptableObject.Instance.m_icon_card_s_format, value.n_PATCH);
					string s_ICON = value.s_ICON;
					componentInChildren.Setup(itemId, p_bundleName, s_ICON, OnClickItem);
					componentInChildren.SetCardType(value);
				}
			}
			else
			{
				componentInChildren.Setup(itemId, AssetBundleScriptableObject.Instance.GetIconItem(item.s_ICON), item.s_ICON, OnClickItem);
			}
			componentInChildren.SetRare(item.n_RARE);
			componentInChildren.SetAmount(amount);
			return ManagedSingleton<OrangeTextDataManager>.Instance.ITEMTEXT_TABLE_DICT.GetL10nValue(item.w_NAME);
		}
		return null;
	}

	public void ShowRewardPopup()
	{
		if (rewardList != null && rewardList.Count > 0)
		{
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_RewardPopup", delegate(RewardPopopUI ui)
			{
				ui.Setup(rewardList);
			});
		}
	}

	public void OnGetReward(Button btn, int n_ID)
	{
		rewardList.Clear();
		MonoBehaviourSingleton<OrangeGameManager>.Instance.ReceiveMissionRewardReq(n_ID, delegate(object p_param)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(m_getItem);
			rewardList.AddRange(p_param as List<NetRewardInfo>);
			ShowRewardPopup();
			btn.gameObject.SetActive(true);
			m_rewardAvailableCount = ((m_rewardAvailableCount > 0) ? (m_rewardAvailableCount - 1) : 0);
			m_getAllBtn.interactable = m_rewardAvailableCount > 0;
			Debug.Log("m_rewardAvailableCount = " + m_rewardAvailableCount);
		});
	}

	public void OnGetAllReward()
	{
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(m_getAllItem);
		rewardList.Clear();
		StartReceiveAll();
	}

	public void StartReceiveAll()
	{
		List<int> list = CollectReceivableEventMissionList();
		if (list.Count > 0)
		{
			MonoBehaviourSingleton<OrangeGameManager>.Instance.ReceiveMissionRewardReq(list, delegate(object p_param)
			{
				rewardList.AddRange(p_param as List<NetRewardInfo>);
				StartReceiveAll();
			});
		}
		else
		{
			ShowRewardPopup();
			m_currentSelectedTab = TabType.none;
			OnClickScoreToggle(true);
		}
	}

	public List<int> CollectReceivableEventMissionList()
	{
		MissionType missionType = GetMissionType(m_eventTable, m_currentSelectedTab);
		List<MISSION_TABLE> list = ManagedSingleton<OrangeDataManager>.Instance.MISSION_TABLE_DICT.Values.Where((MISSION_TABLE x) => x.n_TYPE == (int)missionType && x.n_SUB_TYPE == m_eventTable.n_POINT).ToList();
		List<int> list2 = new List<int>();
		foreach (MISSION_TABLE item in list)
		{
			if (!ManagedSingleton<MissionHelper>.Instance.CheckMissionRewardRetrieved(item.n_ID) && ManagedSingleton<MissionHelper>.Instance.CheckMissionCompleted(item.n_ID))
			{
				list2.Add(item.n_ID);
			}
			if (list2.Count >= 20)
			{
				break;
			}
		}
		return list2;
	}

	private void AddItemBarScore(int itemId, int amount, int score, int n_ID, bool bCompleted, bool bRetrieved)
	{
		Vector2 sizeDelta = m_itemBarScoreRef.GetComponent<RectTransform>().sizeDelta;
		Transform transform = Object.Instantiate(m_itemBarScoreRef, m_scrollViewContentRect);
		transform.transform.localPosition = m_currentPos;
		transform.transform.Find("TextItemName").GetComponent<OrangeText>().text = ItemIconHelper(transform, itemId, amount);
		transform.transform.Find("TextScore").GetComponent<OrangeText>().text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("EVENT_STAGE_POINT") + ": " + score;
		Button getBtn = transform.transform.Find("BtnGet").GetComponent<Button>();
		Button gotBtn = transform.transform.Find("BtnGot").GetComponent<Button>();
		if (!bCompleted)
		{
			gotBtn.gameObject.SetActive(false);
			getBtn.gameObject.SetActive(false);
		}
		else
		{
			gotBtn.gameObject.SetActive(bRetrieved);
			getBtn.gameObject.SetActive(!bRetrieved);
		}
		getBtn.onClick.AddListener(delegate
		{
			getBtn.gameObject.SetActive(false);
			OnGetReward(gotBtn, n_ID);
		});
		m_scrollViewContentRect.sizeDelta = new Vector2(m_scrollViewContentRect.sizeDelta.x, Mathf.Abs(m_currentPos.y - sizeDelta.y - m_spacing.y));
		if (m_bAlignLeft)
		{
			m_currentPos.x += sizeDelta.x + m_spacing.x;
		}
		else
		{
			m_currentPos.x = 0f;
			m_currentPos.y -= sizeDelta.y + m_spacing.y;
		}
		m_bAlignLeft = !m_bAlignLeft;
	}

	private void AddLabelRank(RANK rank, int startRank, int endRank)
	{
		Transform transform = Object.Instantiate(m_labelRankRef[(int)rank], m_scrollViewContentRect);
		OrangeText componentInChildren = transform.GetComponentInChildren<OrangeText>();
		if ((bool)componentInChildren)
		{
			string str = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("EVENT_STAGE_RANKING");
			if (startRank == endRank)
			{
				componentInChildren.text = string.Format("{0}{1}", str, startRank);
			}
			else
			{
				componentInChildren.text = string.Format("{0}{1}~{2}{3}", str, startRank, str, endRank);
			}
		}
		if (!m_bAlignLeft)
		{
			Vector2 sizeDelta = m_itemBarRankRef[(int)rank].GetComponent<RectTransform>().sizeDelta;
			m_currentPos.y -= sizeDelta.y + m_spacing.y;
		}
		m_currentPos.x = 0f;
		m_currentPos.y -= 15f;
		m_bAlignLeft = true;
		transform.transform.localPosition = m_currentPos;
		Vector2 sizeDelta2 = transform.GetComponent<RectTransform>().sizeDelta;
		m_scrollViewContentRect.sizeDelta = new Vector2(m_scrollViewContentRect.sizeDelta.x, Mathf.Abs(m_currentPos.y - sizeDelta2.y - m_spacing.y));
		m_currentPos.y -= sizeDelta2.y + m_spacing.y;
	}

	private void AddItemBarRank(RANK rank, int itemId, int amount, bool bBtnGot = true)
	{
		Transform transform = Object.Instantiate(m_itemBarRankRef[(int)rank], m_scrollViewContentRect);
		transform.transform.localPosition = m_currentPos;
		Vector2 sizeDelta = transform.GetComponent<RectTransform>().sizeDelta;
		transform.transform.Find("TextItemName").GetComponent<OrangeText>().text = ItemIconHelper(transform, itemId, amount);
		m_scrollViewContentRect.sizeDelta = new Vector2(m_scrollViewContentRect.sizeDelta.x, Mathf.Abs(m_currentPos.y - sizeDelta.y - m_spacing.y));
		if (m_bAlignLeft)
		{
			m_currentPos.x += sizeDelta.x + m_spacing.x;
		}
		else
		{
			m_currentPos.x = 0f;
			m_currentPos.y -= sizeDelta.y + m_spacing.y;
		}
		m_bAlignLeft = !m_bAlignLeft;
	}

	private void AddItemBarStage(int itemId, int amount)
	{
		Transform transform = Object.Instantiate(m_itemBarStageRef, m_scrollViewContentRect);
		transform.transform.localPosition = m_currentPos;
		Vector2 sizeDelta = transform.GetComponent<RectTransform>().sizeDelta;
		transform.transform.Find("TextItemName").GetComponent<OrangeText>().text = ItemIconHelper(transform, itemId, amount);
		m_scrollViewContentRect.sizeDelta = new Vector2(m_scrollViewContentRect.sizeDelta.x, Mathf.Abs(m_currentPos.y - sizeDelta.y - m_spacing.y));
		if (m_bAlignLeft)
		{
			m_currentPos.x += sizeDelta.x + m_spacing.x;
		}
		else
		{
			m_currentPos.x = 0f;
			m_currentPos.y -= sizeDelta.y + m_spacing.y;
		}
		m_bAlignLeft = !m_bAlignLeft;
	}

	private int GetTotalScore()
	{
		if (m_eventTable.n_TYPE == 11)
		{
			return ManagedSingleton<MissionHelper>.Instance.GetBossRushScore(m_eventTable.n_ID);
		}
		if (m_eventTable.n_TYPE == 1)
		{
			return ManagedSingleton<MissionHelper>.Instance.GetMissionProgressCount(m_eventTable.n_COUNTER);
		}
		return 0;
	}

	private MissionType GetMissionType(EVENT_TABLE eventTable, TabType tabType)
	{
		switch (tabType)
		{
		case TabType.Rank:
			if (eventTable.n_TYPE == 11)
			{
				return MissionType.BossRushReward;
			}
			if (eventTable.n_TYPE == 1)
			{
				return MissionType.EventRankingReward;
			}
			break;
		case TabType.Score:
			return MissionType.EventScoreReward;
		}
		return MissionType.Daily;
	}

	public void OnClickScoreToggle(bool bIsOn)
	{
		if (m_currentSelectedTab == TabType.Score)
		{
			return;
		}
		if (m_currentSelectedTab != TabType.none && m_rewardToggles[0].isOn)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(m_clickTapSE);
		}
		m_basicRewardGroup.gameObject.SetActive(true);
		m_boxGachaGroup.gameObject.SetActive(false);
		m_bScoreItemCollectable = false;
		m_currentSelectedTab = TabType.Score;
		ClearScrollView();
		m_scoreRankParent.gameObject.SetActive(true);
		m_scoreRankLabel.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("EVENT_STAGE_POINT");
		m_getAllBtn.gameObject.SetActive(true);
		m_rewardNoteText.gameObject.SetActive(false);
		MissionType missionType = GetMissionType(m_eventTable, TabType.Score);
		List<MISSION_TABLE> list = ManagedSingleton<OrangeDataManager>.Instance.MISSION_TABLE_DICT.Values.Where((MISSION_TABLE x) => x.n_TYPE == (int)missionType && x.n_SUB_TYPE == m_eventTable.n_POINT).ToList();
		if (list.Count != 0)
		{
			m_scoreRankText.text = GetTotalScore().ToString();
			m_rewardAvailableCount = 0;
			foreach (MISSION_TABLE item in list)
			{
				int n_ITEMID_ = item.n_ITEMID_1;
				int n_ITEMCOUNT_ = item.n_ITEMCOUNT_1;
				int n_CONDITION_Y = item.n_CONDITION_Y;
				int n_ID = item.n_ID;
				bool flag = ManagedSingleton<MissionHelper>.Instance.CheckMissionCompleted(n_ID);
				bool flag2 = ManagedSingleton<MissionHelper>.Instance.CheckMissionRewardRetrieved(n_ID);
				AddItemBarScore(n_ITEMID_, n_ITEMCOUNT_, n_CONDITION_Y, n_ID, flag, flag2);
				if (flag && !flag2)
				{
					m_bScoreItemCollectable = true;
					m_rewardAvailableCount++;
				}
			}
		}
		m_getAllBtn.interactable = m_bScoreItemCollectable;
	}

	public void OnClickRankToggle(bool bIsOn)
	{
		if (m_currentSelectedTab == TabType.Rank)
		{
			return;
		}
		if (m_currentSelectedTab != TabType.none && m_rewardToggles[1].isOn)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(m_clickTapSE);
		}
		m_basicRewardGroup.gameObject.SetActive(true);
		m_boxGachaGroup.gameObject.SetActive(false);
		m_currentSelectedTab = TabType.Rank;
		ClearScrollView();
		m_scoreRankParent.gameObject.SetActive(true);
		m_scoreRankLabel.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("EVENT_STAGE_RANKING");
		m_getAllBtn.gameObject.SetActive(false);
		m_rewardNoteText.gameObject.SetActive(true);
		MissionType missionType = GetMissionType(m_eventTable, TabType.Rank);
		List<MISSION_TABLE> list = ManagedSingleton<OrangeDataManager>.Instance.MISSION_TABLE_DICT.Values.Where((MISSION_TABLE x) => x.n_TYPE == (int)missionType && x.n_SUB_TYPE == m_eventTable.n_RANKING).ToList();
		if (list.Count == 0)
		{
			return;
		}
		m_scoreRankText.text = m_eventRankingInfo.Ranking.ToString();
		int n_CONDITION_Y = list[0].n_CONDITION_Y;
		int n_CONDITION_Z = list[0].n_CONDITION_Z;
		RANK rANK = RANK.R1;
		AddLabelRank(rANK, n_CONDITION_Y, n_CONDITION_Z);
		foreach (MISSION_TABLE item in list)
		{
			if (n_CONDITION_Y != item.n_CONDITION_Y || n_CONDITION_Z != item.n_CONDITION_Z)
			{
				n_CONDITION_Y = item.n_CONDITION_Y;
				n_CONDITION_Z = item.n_CONDITION_Z;
				if (rANK != RANK.R4)
				{
					rANK++;
				}
				AddLabelRank(rANK, n_CONDITION_Y, n_CONDITION_Z);
			}
			int n_ITEMID_ = item.n_ITEMID_1;
			int n_ITEMCOUNT_ = item.n_ITEMCOUNT_1;
			int n_ITEMID_2 = item.n_ITEMID_2;
			int n_ITEMCOUNT_2 = item.n_ITEMCOUNT_2;
			int n_ITEMID_3 = item.n_ITEMID_3;
			int n_ITEMCOUNT_3 = item.n_ITEMCOUNT_3;
			if (n_ITEMID_ != 0)
			{
				AddItemBarRank(rANK, n_ITEMID_, n_ITEMCOUNT_);
			}
			if (n_ITEMID_2 != 0)
			{
				AddItemBarRank(rANK, n_ITEMID_2, n_ITEMCOUNT_2);
			}
			if (n_ITEMID_3 != 0)
			{
				AddItemBarRank(rANK, n_ITEMID_3, n_ITEMCOUNT_3);
			}
		}
	}

	public void OnClickStageToggle(bool bIsOn)
	{
		if (m_currentSelectedTab == TabType.Stage)
		{
			return;
		}
		if (m_currentSelectedTab != TabType.none && m_rewardToggles[2].isOn)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(m_clickTapSE);
		}
		m_basicRewardGroup.gameObject.SetActive(true);
		m_boxGachaGroup.gameObject.SetActive(false);
		m_currentSelectedTab = TabType.Stage;
		ClearScrollView();
		m_scoreRankParent.gameObject.SetActive(false);
		m_getAllBtn.gameObject.SetActive(false);
		m_rewardNoteText.gameObject.SetActive(false);
		ITEM_TABLE item = null;
		foreach (GACHA_TABLE item2 in ManagedSingleton<ExtendDataHelper>.Instance.GetListGachaByGroup(m_stageTable.n_GET_REWARD))
		{
			if (ManagedSingleton<OrangeTableHelper>.Instance.GetItem(item2.n_REWARD_ID, out item))
			{
				AddItemBarStage(item.n_ID, 1);
			}
		}
	}

	public void OnClickGachaToggle(bool bIsOn)
	{
		if (m_currentSelectedTab != TabType.Gacha)
		{
			if (m_currentSelectedTab != TabType.none && m_rewardToggles[3].isOn)
			{
				MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(m_clickTapSE);
			}
			m_basicRewardGroup.gameObject.SetActive(false);
			m_boxGachaGroup.gameObject.SetActive(true);
			m_currentSelectedTab = TabType.Gacha;
			ClearScrollView();
			UpdateGachaInfoMain(true);
		}
	}

	private void UpdateGachaInfoMain(bool bWelcomeMsg, Callback pcb = null)
	{
		int i = 0;
		int num = 0;
		BOXGACHA_TABLE value = null;
		BoxGachaStatus value2 = null;
		List<BOXGACHA_TABLE> list = ManagedSingleton<OrangeDataManager>.Instance.BOXGACHA_TABLE_DICT.Values.Where((BOXGACHA_TABLE x) => x.n_GROUP == m_eventTable.n_BOXGACHA).ToList();
		m_bAllRewardsObtained = false;
		m_drawBtnx1.interactable = true;
		m_drawBtnx10.interactable = true;
		m_gachaResetBtn.interactable = false;
		if (list.Count <= 0)
		{
			Debug.LogWarning("BoxGachaTable data error!");
			return;
		}
		if (ManagedSingleton<PlayerNetManager>.Instance.dicBoxGachaStatus.TryGetValue(m_eventTable.n_ID, out value2))
		{
			num = value2.netBoxGachaStatus.CycleCounts;
			ManagedSingleton<OrangeDataManager>.Instance.BOXGACHA_TABLE_DICT.TryGetValue(value2.netBoxGachaStatus.CurrentBoxGachaID, out value);
			for (i = 0; i < list.Count && list[i].n_ID != value.n_ID; i++)
			{
			}
		}
		else
		{
			value = list[0];
		}
		m_currentBoxGachaTable = value;
		m_maxPhaseCount = list.Count;
		m_currentPhase = i + 1 + num;
		ITEM_TABLE value3 = null;
		if (ManagedSingleton<ExtendDataHelper>.Instance.ITEM_TABLE_DICT.TryGetValue(m_currentBoxGachaTable.n_COIN_ID, out value3))
		{
			MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(AssetBundleScriptableObject.Instance.GetIconItem(value3.s_ICON), value3.s_ICON, delegate(Sprite obj)
			{
				for (int j = 0; j < m_coinIcons.Length; j++)
				{
					m_coinIcons[j].sprite = obj;
				}
			});
		}
		UpdateGachaItemList(value.n_ID, value.n_GACHA, pcb);
		int itemValue = ManagedSingleton<PlayerHelper>.Instance.GetItemValue(value.n_COIN_ID);
		m_coinOwned.text = itemValue.ToString();
		m_phaseNumber.text = m_currentPhase.ToString();
		if (bWelcomeMsg)
		{
			m_dialogText.text = GetRicoWelcomeMsg(value, itemValue);
		}
	}

	private void UpdateGachaItemList(int boxGachaTable_nID, int boxGachaTable_nGacha, Callback pcb)
	{
		int totalItemCount = 0;
		int totalItemObtained = 0;
		Vector2 itemPos = new Vector2(0f, 0f);
		Vector2 sizeDelta = m_itemBarRankRef[0].GetComponent<RectTransform>().sizeDelta;
		List<BOXGACHACONTENT_TABLE> listBoxGachaContentTable = ManagedSingleton<OrangeDataManager>.Instance.BOXGACHACONTENT_TABLE_DICT.Values.Where((BOXGACHACONTENT_TABLE x) => x.n_GROUP == boxGachaTable_nGacha).ToList();
		foreach (Transform scrollViewGacha in m_scrollViewGachaList)
		{
			Object.Destroy(scrollViewGacha.gameObject);
		}
		ManagedSingleton<PlayerNetManager>.Instance.RetrieveBoxGachaRecordReq(m_eventTable.n_ID, boxGachaTable_nID, delegate(int param, List<NetBoxGachaRecord> records)
		{
			m_netBoxGachaRecords = records;
			m_boxGachaTargetRewardID = 0;
			foreach (BOXGACHACONTENT_TABLE boxGachaContentTable in listBoxGachaContentTable)
			{
				if (boxGachaContentTable.n_PICKUP == 1)
				{
					m_boxGachaTargetRewardID = boxGachaContentTable.n_REWARD_ID;
					if (m_netBoxGachaRecords.Find((NetBoxGachaRecord x) => x.GachaID == boxGachaContentTable.n_ID) != null)
					{
						m_gachaResetBtn.interactable = true;
					}
				}
				Transform transform = ((boxGachaContentTable.n_PICKUP == 1) ? Object.Instantiate(m_itemBarRankRef[0], m_scrollViewGachaList) : Object.Instantiate(m_itemBarRankRef[3], m_scrollViewGachaList));
				transform.transform.localPosition = itemPos;
				transform.transform.Find("TextItemName").GetComponent<OrangeText>().text = ItemIconHelper(transform, boxGachaContentTable.n_REWARD_ID, boxGachaContentTable.n_AMOUNT_MAX);
				OrangeText component = transform.transform.Find("TextItemSets").GetComponent<OrangeText>();
				Transform transform2 = transform.transform.Find("CheckMark");
				if ((bool)transform2)
				{
					transform2.gameObject.SetActive(false);
				}
				NetBoxGachaRecord netBoxGachaRecord = m_netBoxGachaRecords.Find((NetBoxGachaRecord x) => x.GachaID == boxGachaContentTable.n_ID);
				if (netBoxGachaRecord == null)
				{
					component.text = string.Format("{0}/{1}", boxGachaContentTable.n_TOTAL, boxGachaContentTable.n_TOTAL);
				}
				else
				{
					bool active = netBoxGachaRecord.Count >= boxGachaContentTable.n_TOTAL;
					component.text = string.Format("{0}/{1}", boxGachaContentTable.n_TOTAL - netBoxGachaRecord.Count, boxGachaContentTable.n_TOTAL);
					if ((bool)transform2)
					{
						transform2.gameObject.SetActive(active);
					}
					totalItemObtained += netBoxGachaRecord.Count;
				}
				totalItemCount += boxGachaContentTable.n_TOTAL;
				itemPos.y -= sizeDelta.y + m_spacing.y;
			}
			AddDummyItemToGachaList(ref itemPos, sizeDelta);
			if (totalItemObtained >= totalItemCount)
			{
				Debug.Log("No more items.");
				m_gachaResetBtn.interactable = true;
				m_bAllRewardsObtained = true;
			}
			UpdateGachaDrawButtons(totalItemCount - totalItemObtained);
			m_itemsLeft.text = totalItemCount - totalItemObtained + "/" + totalItemCount;
			m_scrollViewGachaList.sizeDelta = new Vector2(m_scrollViewGachaList.sizeDelta.x, 0f - itemPos.y);
			pcb.CheckTargetToInvoke();
		});
	}

	private void AddDummyItemToGachaList(ref Vector2 itemPos, Vector2 sizeDelta)
	{
		Transform transform = Object.Instantiate(m_itemBarRankRef[3], m_scrollViewGachaList);
		transform.transform.localPosition = itemPos;
		transform.transform.Find("TextItemName").GetComponent<OrangeText>().text = ItemIconHelper(transform, OrangeConst.BOX_GACHA_DUMMY_ID, OrangeConst.BOX_GACHA_DUMMY_COUNT);
		OrangeText component = transform.transform.Find("TextItemSets").GetComponent<OrangeText>();
		Transform transform2 = transform.transform.Find("CheckMark");
		if ((bool)transform2)
		{
			transform2.gameObject.SetActive(false);
		}
		component.text = "";
		itemPos.y -= sizeDelta.y + m_spacing.y;
	}

	private void UpdateGachaDrawButtons(int itemsLeft)
	{
		int itemValue = ManagedSingleton<PlayerHelper>.Instance.GetItemValue(m_currentBoxGachaTable.n_COIN_ID);
		if (m_currentPhase < m_maxPhaseCount)
		{
			m_multiDrawCountAvailable = ((itemsLeft >= m_defaultMaxDrawCount) ? m_defaultMaxDrawCount : itemsLeft);
		}
		else
		{
			m_multiDrawCountAvailable = ((itemsLeft >= m_lastPhaseMaxDrawCount) ? m_lastPhaseMaxDrawCount : itemsLeft);
		}
		if (itemValue < m_currentBoxGachaTable.n_COIN_MOUNT * m_multiDrawCountAvailable)
		{
			m_multiDrawCountAvailable = Mathf.FloorToInt(itemValue / m_currentBoxGachaTable.n_COIN_MOUNT);
		}
		if (itemsLeft > 0)
		{
			if (m_multiDrawCountAvailable == 0)
			{
				m_drawBtnx1.interactable = false;
			}
			if (m_multiDrawCountAvailable == 0)
			{
				m_drawBtnx10.interactable = false;
			}
		}
		string str = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("BOX_REWARD_DRAW");
		m_captionDrawX1.text = string.Format(str, 1);
		m_captionDrawX10.text = string.Format(str, (m_multiDrawCountAvailable <= 1) ? 1 : m_multiDrawCountAvailable);
		m_coinPerDrawX1.text = string.Format("x{0}", m_currentBoxGachaTable.n_COIN_MOUNT);
		m_coinPerDrawX10.text = string.Format("x{0}", m_currentBoxGachaTable.n_COIN_MOUNT * ((m_multiDrawCountAvailable <= 1) ? 1 : m_multiDrawCountAvailable));
	}

	private string GetRicoWelcomeMsg(BOXGACHA_TABLE boxGachaTable, int coinOwned)
	{
		if (coinOwned == 0)
		{
			return string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("DIALOG_BOX_NOITEM"), ManagedSingleton<OrangeTableHelper>.Instance.GetItemName(boxGachaTable.n_COIN_ID));
		}
		return MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("DIALOG_BOX_TRY");
	}

	private string GetRicoCongratsMsg(bool bTargetRewardObtained)
	{
		if (bTargetRewardObtained)
		{
			return MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("DIALOG_BOX_TARGET");
		}
		return MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("DIALOG_BOX_NORMAL");
	}

	public void OnClickGachaMultiDraw()
	{
		OnClickGachaDraw(m_multiDrawCountAvailable);
	}

	public void OnClickGachaDraw(int count)
	{
		if (m_bAllRewardsObtained)
		{
			AllRewardsObtained();
			return;
		}
		count = ((count > m_multiDrawCountAvailable) ? m_multiDrawCountAvailable : count);
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK05);
		ManagedSingleton<PlayerNetManager>.Instance.BoxGachaReq(count, m_currentBoxGachaTable.n_ID, m_eventTable.n_ID, delegate(int param, NetRewardsEntity reward)
		{
			bool bTargetRewardObtained = false;
			if (m_boxGachaTargetRewardID != 0)
			{
				bTargetRewardObtained = reward.RewardList.Find((NetRewardInfo x) => x.RewardID == m_boxGachaTargetRewardID) != null;
			}
			m_dialogText.text = GetRicoCongratsMsg(bTargetRewardObtained);
			UpdateGachaInfoMain(false, delegate
			{
				if (reward.RewardList.Count > 0)
				{
					MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_RewardPopup", delegate(RewardPopopUI ui)
					{
						if (m_bAllRewardsObtained)
						{
							ui.closeCB = AllRewardsObtained;
						}
						else if (bTargetRewardObtained)
						{
							ui.closeCB = TargetRewardObtained;
						}
						ui.Setup(reward.RewardList);
						ui.ChangeTitle(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("GET_REWARD2"));
					});
				}
			});
		});
	}

	public void TargetRewardObtained()
	{
		MonoBehaviourSingleton<UIManager>.Instance.LoadResourceUI("UI_CommonMsg", delegate(CommonUI ui)
		{
			ui.OpenSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP;
			ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
			ui.SetupYesNO(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_TIP"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("REWARD_RESET_TARGET_TIP"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_YES"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_NO"), delegate
			{
				ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_OK17;
				ManagedSingleton<PlayerNetManager>.Instance.ResetBoxGachaStatusReq(m_eventTable.n_ID, delegate
				{
					UpdateGachaInfoMain(true);
				});
			});
		});
	}

	private void AllRewardsObtained()
	{
		MonoBehaviourSingleton<UIManager>.Instance.LoadResourceUI("UI_CommonMsg", delegate(CommonUI ui)
		{
			ui.OpenSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP;
			ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
			ui.SetupConfirm(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_TIP"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("REWARD_RESET_EMPTY"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_OK"), delegate
			{
				ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_OK17;
				ManagedSingleton<PlayerNetManager>.Instance.ResetBoxGachaStatusReq(m_eventTable.n_ID, delegate
				{
					UpdateGachaInfoMain(true);
				});
			});
		});
	}

	public void OnClickGachaReset()
	{
		MonoBehaviourSingleton<UIManager>.Instance.LoadResourceUI("UI_CommonMsg", delegate(CommonUI ui)
		{
			ui.OpenSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP;
			ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
			ui.SetupYesNO(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_TIP"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("REWARD_RESET_CONFIRM_TIP"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_YES"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_NO"), delegate
			{
				ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_OK17;
				ManagedSingleton<PlayerNetManager>.Instance.ResetBoxGachaStatusReq(m_eventTable.n_ID, delegate
				{
					UpdateGachaInfoMain(true);
				});
			});
		});
	}

	public void OnClickGachaList()
	{
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_BoxGachaList", delegate(BoxGachaList ui)
		{
			ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
			ui.Setup(m_eventTable, m_netBoxGachaRecords);
		});
	}

	public void SpeedEventSetup(EVENT_TABLE eventTable, STAGE_TABLE stageTable, EventRankingInfo eventRankingInfo, int prank = 0, int crank = 0, int mode = 1, bool bEvent = false)
	{
		m_eventTable = eventTable;
		m_stageTable = stageTable;
		List<Toggle> list = new List<Toggle>();
		Vector3 localPosition = m_rewardToggles[3].transform.localPosition;
		float num = 222f;
		bSpeedEventRanking = true;
		bSpeedRewardEvent = bEvent;
		SpeedRewardEventSub = ((mode == 1) ? mode : 2);
		SpeedEventType = (bEvent ? MissionType.TAStageRankingUpgrade : MissionType.TAStageRanking);
		CurrentSpeedPowerRank = prank;
		CurrentSpeedChallengeRank = crank;
		if (eventTable != null)
		{
			m_eventTimeRoot.SetActive(true);
			m_eventTimeText.text = OrangeGameUtility.DisplayDatePeriod(eventTable.s_BEGIN_TIME, eventTable.s_END_TIME);
		}
		m_rewardToggles[4].gameObject.SetActive(true);
		m_rewardToggles[4].transform.localPosition = new Vector3(localPosition.x + num * (float)list.Count, localPosition.y, localPosition.z);
		list.Add(m_rewardToggles[4]);
		m_rewardToggles[5].gameObject.SetActive(true);
		m_rewardToggles[5].transform.localPosition = new Vector3(localPosition.x + num * (float)list.Count, localPosition.y, localPosition.z);
		list.Add(m_rewardToggles[5]);
		if (list.Count > 0)
		{
			list[0].isOn = true;
		}
		base._EscapeEvent = EscapeEvent.CLOSE_UI;
	}

	public void OnClickSpeedPowerToggle(bool bIsOn)
	{
		if (m_currentSelectedTab != TabType.SpeedPower)
		{
			if (m_currentSelectedTab != TabType.none && m_rewardToggles[4].isOn)
			{
				MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(m_clickTapSE);
			}
			m_currentSelectedTab = TabType.SpeedPower;
			SpeedRewardEventSub = 1;
			OnClickSpeedToggle(bIsOn);
		}
	}

	public void OnClickSpeedChallengeToggle(bool bIsOn)
	{
		if (m_currentSelectedTab != TabType.SpeedChallenge)
		{
			if (m_currentSelectedTab != TabType.none && m_rewardToggles[5].isOn)
			{
				MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(m_clickTapSE);
			}
			m_currentSelectedTab = TabType.SpeedChallenge;
			SpeedRewardEventSub = 2;
			OnClickSpeedToggle(bIsOn);
		}
	}

	public void OnClickSpeedToggle(bool bIsOn)
	{
		m_basicRewardGroup.gameObject.SetActive(true);
		m_boxGachaGroup.gameObject.SetActive(false);
		ClearScrollView();
		m_scoreRankParent.gameObject.SetActive(true);
		m_scoreRankLabel.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("EVENT_STAGE_RANKING");
		m_getAllBtn.gameObject.SetActive(false);
		m_rewardNoteText.gameObject.SetActive(false);
		m_rewardNoteSpeedText.gameObject.SetActive(true);
		MissionType missionType = SpeedEventType;
		List<MISSION_TABLE> list = ManagedSingleton<OrangeDataManager>.Instance.MISSION_TABLE_DICT.Values.Where((MISSION_TABLE x) => x.n_TYPE == (int)missionType && x.n_SUB_TYPE == SpeedRewardEventSub).ToList();
		if (list.Count == 0)
		{
			return;
		}
		if (bSpeedEventRanking)
		{
			if (SpeedRewardEventSub == 1)
			{
				m_scoreRankText.text = ((CurrentSpeedPowerRank > 0) ? CurrentSpeedPowerRank.ToString() : "---");
			}
			else
			{
				m_scoreRankText.text = ((CurrentSpeedChallengeRank > 0) ? CurrentSpeedChallengeRank.ToString() : "---");
			}
		}
		int n_CONDITION_Y = list[0].n_CONDITION_Y;
		int n_CONDITION_Z = list[0].n_CONDITION_Z;
		RANK rANK = RANK.R1;
		AddLabelRank(rANK, n_CONDITION_Y, n_CONDITION_Z);
		foreach (MISSION_TABLE item in list)
		{
			if (n_CONDITION_Y != item.n_CONDITION_Y || n_CONDITION_Z != item.n_CONDITION_Z)
			{
				n_CONDITION_Y = item.n_CONDITION_Y;
				n_CONDITION_Z = item.n_CONDITION_Z;
				if (rANK != RANK.R4)
				{
					rANK++;
				}
				AddLabelRank(rANK, n_CONDITION_Y, n_CONDITION_Z);
			}
			int n_ITEMID_ = item.n_ITEMID_1;
			int n_ITEMCOUNT_ = item.n_ITEMCOUNT_1;
			int n_ITEMID_2 = item.n_ITEMID_2;
			int n_ITEMCOUNT_2 = item.n_ITEMCOUNT_2;
			int n_ITEMID_3 = item.n_ITEMID_3;
			int n_ITEMCOUNT_3 = item.n_ITEMCOUNT_3;
			if (n_ITEMID_ != 0)
			{
				AddItemBarRank(rANK, n_ITEMID_, n_ITEMCOUNT_);
			}
			if (n_ITEMID_2 != 0)
			{
				AddItemBarRank(rANK, n_ITEMID_2, n_ITEMCOUNT_2);
			}
			if (n_ITEMID_3 != 0)
			{
				AddItemBarRank(rANK, n_ITEMID_3, n_ITEMCOUNT_3);
			}
		}
	}

	public void DeepRecordSetup()
	{
		List<Toggle> list = new List<Toggle>();
		Vector3 localPosition = m_rewardToggles[3].transform.localPosition;
		float num = 222f;
		Toggle toggle = m_rewardToggles[6];
		toggle.gameObject.SetActive(true);
		toggle.transform.localPosition = new Vector3(localPosition.x + num * (float)list.Count, localPosition.y, localPosition.z);
		list.Add(toggle);
		if (list.Count > 0)
		{
			list[0].isOn = true;
		}
		base.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
		base._EscapeEvent = EscapeEvent.CLOSE_UI;
	}

	public void OnClickDeepRecordToggle(bool bIsOn)
	{
		if (m_currentSelectedTab == TabType.DeepRecord)
		{
			return;
		}
		m_currentSelectedTab = TabType.DeepRecord;
		m_basicRewardGroup.gameObject.SetActive(true);
		m_boxGachaGroup.gameObject.SetActive(false);
		ClearScrollView();
		m_scoreRankParent.gameObject.SetActive(true);
		m_scoreRankLabel.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("EVENT_STAGE_RANKING");
		m_getAllBtn.gameObject.SetActive(false);
		m_rewardNoteText.gameObject.SetActive(true);
		m_rewardNoteSpeedText.gameObject.SetActive(false);
		int type = deepRecordEventType;
		List<MISSION_TABLE> list = ManagedSingleton<OrangeDataManager>.Instance.MISSION_TABLE_DICT.Values.Where((MISSION_TABLE x) => x.n_TYPE == type).ToList();
		if (list.Count == 0)
		{
			return;
		}
		m_scoreRankText.text = ManagedSingleton<DeepRecordHelper>.Instance.PlayerInfo.Rank.ToString();
		int n_CONDITION_Y = list[0].n_CONDITION_Y;
		int n_CONDITION_Z = list[0].n_CONDITION_Z;
		RANK rANK = RANK.R1;
		AddLabelRank(rANK, n_CONDITION_Y, n_CONDITION_Z);
		foreach (MISSION_TABLE item in list)
		{
			if (n_CONDITION_Y != item.n_CONDITION_Y || n_CONDITION_Z != item.n_CONDITION_Z)
			{
				n_CONDITION_Y = item.n_CONDITION_Y;
				n_CONDITION_Z = item.n_CONDITION_Z;
				if (rANK != RANK.R4)
				{
					rANK++;
				}
				AddLabelRank(rANK, n_CONDITION_Y, n_CONDITION_Z);
			}
			int n_ITEMID_ = item.n_ITEMID_1;
			int n_ITEMCOUNT_ = item.n_ITEMCOUNT_1;
			int n_ITEMID_2 = item.n_ITEMID_2;
			int n_ITEMCOUNT_2 = item.n_ITEMCOUNT_2;
			int n_ITEMID_3 = item.n_ITEMID_3;
			int n_ITEMCOUNT_3 = item.n_ITEMCOUNT_3;
			if (n_ITEMID_ != 0)
			{
				AddItemBarRank(rANK, n_ITEMID_, n_ITEMCOUNT_);
			}
			if (n_ITEMID_2 != 0)
			{
				AddItemBarRank(rANK, n_ITEMID_2, n_ITEMCOUNT_2);
			}
			if (n_ITEMID_3 != 0)
			{
				AddItemBarRank(rANK, n_ITEMID_3, n_ITEMCOUNT_3);
			}
		}
	}
}
