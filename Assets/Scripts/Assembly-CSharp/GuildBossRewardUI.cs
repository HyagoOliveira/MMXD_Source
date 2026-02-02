using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.UI;

public class GuildBossRewardUI : OrangeUIBase
{
	private EVENT_TABLE _eventAttrData;

	public GameObject refCommonIconBase;

	public CrusadeRewardItemBarScoreHelper ItemBarScoreHelper;

	public GameObject ItemBarStageGroup;

	public GameObject[] ItemRankGroup;

	public GameObject[] LabelRank;

	public GameObject[] BattleRank;

	public Toggle[] Toggle;

	public bool[] ToggleStatus = new bool[3];

	public GameObject ScoreRewardGroup;

	public RectTransform SVContent01;

	public GameObject ScoreNumRoot;

	public Text ScoreNumRootText;

	public Text ScoreNumRootNum;

	public Text HintMsgText;

	public Button BtnGetAll;

	[BoxGroup("Sound")]
	[Tooltip("取得獎勵")]
	[SerializeField]
	private SystemSE m_getItem = SystemSE.CRI_SYSTEMSE_SYS_OK17;

	[BoxGroup("Sound")]
	[SerializeField]
	private SystemSE m_getAllItem = SystemSE.CRI_SYSTEMSE_SYS_OK17;

	[BoxGroup("Sound")]
	[Tooltip("切換tab")]
	[SerializeField]
	private SystemSE m_ToggleBtn = SystemSE.CRI_SYSTEMSE_SYS_CURSOR07;

	public Vector2 spacing = Vector2.zero;

	public Vector3 ToggleSpacing = Vector2.zero;

	private Vector2 currentPos = Vector2.zero;

	private bool bNextLine;

	private Vector2 lastsizeDelta;

	private long _score;

	private int _step;

	private int _rank;

	private STAGE_TABLE _stageAttrData;

	private int _rewardAvailableCount;

	private bool _isFirst = true;

	private List<int> _rewardIDToReceive = new List<int>();

	private List<NetRewardInfo> _rewardListCache = new List<NetRewardInfo>();

	private List<CrusadeRewardItemBarScoreHelper> _itemBarScoreHelperList = new List<CrusadeRewardItemBarScoreHelper>();

	public void Setup(int eventId, long score, int step, STAGE_TABLE stageAttrData, int rank)
	{
		base._EscapeEvent = EscapeEvent.CLOSE_UI;
		EVENT_TABLE value;
		if (!ManagedSingleton<OrangeDataManager>.Instance.EVENT_TABLE_DICT.TryGetValue(eventId, out value))
		{
			value = new EVENT_TABLE();
		}
		_eventAttrData = value;
		_score = score;
		_stageAttrData = stageAttrData;
		_step = step;
		_rank = rank;
		ScoreNumRootText.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("RAID_CONTRIBUTION");
		ScoreNumRootText.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("EVENT_STAGE_RANKING");
		ScoreNumRootText.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("RAID_CRUSADE_STEP");
		ScoreNumRootNum.text = score.ToString();
		BtnGetAll.gameObject.SetActive(false);
		ScoreNumRoot.SetActive(false);
		HintMsgText.text = "";
		_isFirst = true;
		SwitchToggleBtns(15);
	}

	private void SwitchToggleBtns(int nSwitchBits)
	{
		if (Toggle.Length == 0)
		{
			return;
		}
		Vector3 localPosition = Toggle[0].transform.localPosition;
		bool flag = false;
		for (int i = 0; i < Toggle.Length; i++)
		{
			if ((nSwitchBits & (1 << i)) != 0)
			{
				Toggle[i].gameObject.SetActive(true);
				Toggle[i].transform.localPosition = localPosition;
				localPosition += ToggleSpacing;
				if (!flag)
				{
					Toggle[i].isOn = true;
					ToggleStatus[i] = true;
					flag = true;
				}
			}
			else
			{
				Toggle[i].gameObject.SetActive(false);
			}
		}
	}

	private void PlaySystemSECheckFirst(SystemSE cueid)
	{
		if (_isFirst)
		{
			_isFirst = false;
		}
		else
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(cueid);
		}
	}

	private void InitLineStatus()
	{
		currentPos = Vector2.zero;
		bNextLine = false;
		for (int num = SVContent01.childCount - 1; num >= 0; num--)
		{
			Object.Destroy(SVContent01.GetChild(num).gameObject);
		}
	}

	public void OnScoreRewardPage(bool bIsOn)
	{
		if (bIsOn)
		{
			if (ToggleStatus[0])
			{
				return;
			}
			ToggleStatus[0] = true;
			PlaySystemSECheckFirst(m_ToggleBtn);
			InitLineStatus();
			BtnGetAll.gameObject.SetActive(true);
			ScoreNumRoot.SetActive(true);
			ScoreNumRootText.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("RAID_CONTRIBUTION");
			ScoreNumRootNum.text = _score.ToString();
			HintMsgText.text = "";
			_rewardAvailableCount = 0;
			KeyValuePair<int, MISSION_TABLE>[] array = (from data in ManagedSingleton<OrangeDataManager>.Instance.MISSION_TABLE_DICT
				where data.Value.n_TYPE == 112 && data.Value.n_SUB_TYPE == _eventAttrData.n_POINT && data.Value.n_CONDITION == 123 && data.Value.n_CONDITION_X == _eventAttrData.n_ID
				orderby data.Value.n_CONDITION_Y
				select data).ToArray();
			for (int i = 0; i < array.Length; i++)
			{
				KeyValuePair<int, MISSION_TABLE> keyValuePair = array[i];
				bool bCompleted = ManagedSingleton<MissionHelper>.Instance.CheckMissionCompletedLong(keyValuePair.Value.n_ID);
				bool bRetrieved = ManagedSingleton<MissionHelper>.Instance.CheckMissionRewardRetrieved(keyValuePair.Value.n_ID);
				if (keyValuePair.Value.n_ITEMID_1 != 0)
				{
					AddItemWithGetBtn(keyValuePair.Value.n_ITEMID_1, keyValuePair.Value.n_ITEMCOUNT_1, keyValuePair.Value.n_CONDITION_Y, keyValuePair.Value.n_ID, bCompleted, bRetrieved);
				}
				if (keyValuePair.Value.n_ITEMID_2 != 0)
				{
					AddItemWithGetBtn(keyValuePair.Value.n_ITEMID_2, keyValuePair.Value.n_ITEMCOUNT_2, keyValuePair.Value.n_CONDITION_Y, keyValuePair.Value.n_ID, bCompleted, bRetrieved);
				}
				if (keyValuePair.Value.n_ITEMID_3 != 0)
				{
					AddItemWithGetBtn(keyValuePair.Value.n_ITEMID_3, keyValuePair.Value.n_ITEMCOUNT_3, keyValuePair.Value.n_CONDITION_Y, keyValuePair.Value.n_ID, bCompleted, bRetrieved);
				}
			}
			BtnGetAll.interactable = _rewardAvailableCount > 0;
		}
		else
		{
			ToggleStatus[0] = false;
		}
	}

	public void OnRankPage(bool bIsOn)
	{
		if (bIsOn)
		{
			if (ToggleStatus[1])
			{
				return;
			}
			ToggleStatus[1] = true;
			PlaySystemSECheckFirst(m_ToggleBtn);
			InitLineStatus();
			BtnGetAll.gameObject.SetActive(false);
			KeyValuePair<int, MISSION_TABLE>[] array = (from data in ManagedSingleton<OrangeDataManager>.Instance.MISSION_TABLE_DICT
				where data.Value.n_TYPE == 111 && data.Value.n_SUB_TYPE == _eventAttrData.n_RANKING
				orderby data.Value.n_CONDITION_Y
				select data).ToArray();
			ScoreNumRoot.SetActive(true);
			ScoreNumRootText.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("EVENT_STAGE_RANKING");
			ScoreNumRootNum.text = _rank.ToString();
			HintMsgText.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("RESULT_REWARD_SEND_TIP");
			List<List<MISSION_TABLE>> list = new List<List<MISSION_TABLE>>();
			bool flag = false;
			KeyValuePair<int, MISSION_TABLE>[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				KeyValuePair<int, MISSION_TABLE> keyValuePair = array2[i];
				flag = false;
				foreach (List<MISSION_TABLE> item in list)
				{
					if (keyValuePair.Value.n_CONDITION_Y == item[0].n_CONDITION_Y && keyValuePair.Value.n_CONDITION_Z == item[0].n_CONDITION_Z)
					{
						item.Add(keyValuePair.Value);
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					List<MISSION_TABLE> list2 = new List<MISSION_TABLE>();
					list2.Add(keyValuePair.Value);
					list.Add(list2);
				}
			}
			int num = 0;
			for (int j = 0; j < list.Count; j++)
			{
				List<MISSION_TABLE> list3 = list[j];
				num = ((j < 4) ? (j + 1) : 4);
				AddRank(num, list3[0].n_CONDITION_Y, list3[0].n_CONDITION_Z);
				for (int k = 0; k < list3.Count; k++)
				{
					MISSION_TABLE mISSION_TABLE = list3[k];
					if (mISSION_TABLE.n_ITEMID_1 != 0)
					{
						AddRankItem(mISSION_TABLE.n_ITEMID_1, mISSION_TABLE.n_ITEMCOUNT_1, j, num);
					}
					if (mISSION_TABLE.n_ITEMID_2 != 0)
					{
						AddRankItem(mISSION_TABLE.n_ITEMID_2, mISSION_TABLE.n_ITEMCOUNT_2, j, num);
					}
					if (mISSION_TABLE.n_ITEMID_3 != 0)
					{
						AddRankItem(mISSION_TABLE.n_ITEMID_3, mISSION_TABLE.n_ITEMCOUNT_3, j, num);
					}
				}
			}
		}
		else
		{
			ToggleStatus[1] = false;
		}
	}

	public void OnChallengePage(bool bIsOn)
	{
		if (bIsOn)
		{
			if (!ToggleStatus[2])
			{
				ToggleStatus[2] = true;
				PlaySystemSECheckFirst(m_ToggleBtn);
				InitLineStatus();
				BtnGetAll.gameObject.SetActive(false);
				ScoreNumRoot.SetActive(false);
				HintMsgText.text = "";
				List<GACHA_TABLE> listGachaByGroup = ManagedSingleton<ExtendDataHelper>.Instance.GetListGachaByGroup(_stageAttrData.n_GET_REWARD);
				for (int i = 0; i < listGachaByGroup.Count; i++)
				{
					GACHA_TABLE gACHA_TABLE = listGachaByGroup[i];
					AddItemNoBtn(gACHA_TABLE.n_REWARD_ID, gACHA_TABLE.n_AMOUNT_MAX, i);
				}
			}
		}
		else
		{
			ToggleStatus[2] = false;
		}
	}

	private void AddItemWithGetBtn(int itemId, int amount, int score, int n_ID, bool bCompleted, bool bRetrieved)
	{
		lastsizeDelta = ((RectTransform)ItemBarScoreHelper.transform).sizeDelta;
		CrusadeRewardItemBarScoreHelper crusadeRewardItemBarScoreHelper = Object.Instantiate(ItemBarScoreHelper, SVContent01);
		crusadeRewardItemBarScoreHelper.transform.localPosition = currentPos;
		crusadeRewardItemBarScoreHelper.Setup(itemId, n_ID, amount, score, bCompleted, bRetrieved);
		crusadeRewardItemBarScoreHelper.OnGetOneRewardEvent += OnGetOneRewardEvent;
		if (bCompleted && !bRetrieved)
		{
			_rewardAvailableCount++;
		}
		_itemBarScoreHelperList.Add(crusadeRewardItemBarScoreHelper);
		SVContent01.sizeDelta = new Vector2(SVContent01.sizeDelta.x, Mathf.Abs(currentPos.y - lastsizeDelta.y - spacing.y));
		if (!bNextLine)
		{
			currentPos.x += lastsizeDelta.x + spacing.x;
		}
		else
		{
			currentPos.x = 0f;
			currentPos.y -= lastsizeDelta.y + spacing.y;
		}
		bNextLine = !bNextLine;
	}

	private void AddRank(int ranknum, int minx, int maxx)
	{
		if (ranknum > 0 && ranknum <= LabelRank.Length)
		{
			if (bNextLine)
			{
				currentPos.x = 0f;
				currentPos.y -= lastsizeDelta.y + spacing.y;
				bNextLine = false;
			}
			lastsizeDelta = ((RectTransform)LabelRank[ranknum - 1].transform).sizeDelta;
			Transform transform = Object.Instantiate(LabelRank[ranknum - 1].transform, SVContent01);
			transform.localPosition = currentPos;
			if (minx == maxx)
			{
				transform.transform.Find("TextLabel").GetComponent<Text>().text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("EVENT_STAGE_RANKING") + minx;
			}
			else
			{
				transform.transform.Find("TextLabel").GetComponent<Text>().text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("EVENT_STAGE_RANKING") + minx + "~" + maxx;
			}
			transform.gameObject.SetActive(true);
			SVContent01.sizeDelta = new Vector2(SVContent01.sizeDelta.x, Mathf.Abs(currentPos.y - lastsizeDelta.y - spacing.y));
			currentPos.x = 0f;
			currentPos.y -= lastsizeDelta.y + spacing.y;
		}
	}

	private void AddBattleRank(int ranknum, int nRank)
	{
		if (ranknum > 0 && ranknum <= BattleRank.Length)
		{
			if (bNextLine)
			{
				currentPos.x = 0f;
				currentPos.y -= lastsizeDelta.y + spacing.y;
				bNextLine = false;
			}
			lastsizeDelta = ((RectTransform)BattleRank[ranknum - 1].transform).sizeDelta;
			Transform obj = Object.Instantiate(BattleRank[ranknum - 1].transform, SVContent01);
			obj.localPosition = currentPos;
			obj.transform.Find("TextLabel").GetComponent<Text>().text = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("CRUSADE_REWARD_STEP"), nRank);
			obj.gameObject.SetActive(true);
			SVContent01.sizeDelta = new Vector2(SVContent01.sizeDelta.x, Mathf.Abs(currentPos.y - lastsizeDelta.y - spacing.y));
			currentPos.x = 0f;
			currentPos.y -= lastsizeDelta.y + spacing.y;
		}
	}

	private void AddItemNoBtn(int itemId, int amount, int n_ID)
	{
		lastsizeDelta = ((RectTransform)ItemBarStageGroup.transform).sizeDelta;
		Transform transform = Object.Instantiate(ItemBarStageGroup.transform, SVContent01);
		transform.localPosition = currentPos;
		transform.transform.Find("TextItemName").GetComponent<OrangeText>().text = ItemIconHelper(transform, itemId, amount);
		transform.gameObject.SetActive(true);
		SVContent01.sizeDelta = new Vector2(SVContent01.sizeDelta.x, Mathf.Abs(currentPos.y - lastsizeDelta.y - spacing.y));
		if (!bNextLine)
		{
			currentPos.x += lastsizeDelta.x + spacing.x;
		}
		else
		{
			currentPos.x = 0f;
			currentPos.y -= lastsizeDelta.y + spacing.y;
		}
		bNextLine = !bNextLine;
	}

	private void AddRankItem(int itemId, int amount, int n_ID, int ranknum)
	{
		if (ranknum > 0 && ranknum <= ItemRankGroup.Length)
		{
			lastsizeDelta = ((RectTransform)ItemRankGroup[ranknum - 1].transform).sizeDelta;
			Transform transform = Object.Instantiate(ItemRankGroup[ranknum - 1].transform, SVContent01);
			transform.localPosition = currentPos;
			transform.transform.Find("TextItemName").GetComponent<OrangeText>().text = ItemIconHelper(transform, itemId, amount);
			transform.gameObject.SetActive(true);
			SVContent01.sizeDelta = new Vector2(SVContent01.sizeDelta.x, Mathf.Abs(currentPos.y - lastsizeDelta.y - spacing.y));
			if (!bNextLine)
			{
				currentPos.x += lastsizeDelta.x + spacing.x;
			}
			else
			{
				currentPos.x = 0f;
				currentPos.y -= lastsizeDelta.y + spacing.y;
			}
			bNextLine = !bNextLine;
		}
	}

	private void OnGetOneRewardEvent(int n_ID)
	{
		_rewardListCache.Clear();
		_rewardIDToReceive.Clear();
		_rewardIDToReceive.Add(n_ID);
		StartBatchReceive();
	}

	public void ShowRewardPopup()
	{
		if (_rewardListCache != null && _rewardListCache.Count > 0)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(m_getItem);
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_RewardPopup", delegate(RewardPopopUI ui)
			{
				ui.Setup(_rewardListCache);
			});
		}
		OnScoreRewardPage(true);
	}

	public void OnGetAllReward()
	{
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(m_getAllItem);
		_rewardListCache.Clear();
		_rewardIDToReceive.Clear();
		foreach (CrusadeRewardItemBarScoreHelper itemBarScoreHelper in _itemBarScoreHelperList)
		{
			if (itemBarScoreHelper.IsCompleted && !itemBarScoreHelper.IsRetrieved)
			{
				_rewardIDToReceive.Add(itemBarScoreHelper.ID);
				itemBarScoreHelper.SetRetrieving();
			}
		}
		StartBatchReceive();
	}

	private void StartBatchReceive()
	{
		List<int> list = new List<int>();
		while (_rewardIDToReceive.Count > 0 && list.Count < 20)
		{
			list.Add(_rewardIDToReceive[0]);
			_rewardIDToReceive.RemoveAt(0);
		}
		if (list.Count > 0)
		{
			MonoBehaviourSingleton<OrangeGameManager>.Instance.ReceiveMissionRewardReq(list, OnReceiveMissionRewardRes);
			return;
		}
		foreach (CrusadeRewardItemBarScoreHelper itemBarScoreHelper in _itemBarScoreHelperList)
		{
			if (itemBarScoreHelper.IsRetrieving)
			{
				itemBarScoreHelper.SetRetrived();
			}
		}
		ShowRewardPopup();
	}

	private void OnReceiveMissionRewardRes(object p_param)
	{
		List<NetRewardInfo> list = p_param as List<NetRewardInfo>;
		_rewardListCache.AddRange(list);
		_rewardAvailableCount = IntMath.Max(_rewardAvailableCount - list.Count, 0);
		BtnGetAll.interactable = _rewardAvailableCount > 0;
		StartBatchReceive();
	}

	public string ItemIconHelper(Transform target, int itemId, int amount)
	{
		ItemIconWithAmount componentInChildren = target.GetComponentInChildren<ItemIconWithAmount>();
		ITEM_TABLE item;
		if (ManagedSingleton<OrangeTableHelper>.Instance.GetItem(itemId, out item))
		{
			if (item.n_TYPE == 5 && item.n_TYPE_X == 1 && (int)item.f_VALUE_Y > 0)
			{
				CARD_TABLE value;
				if (ManagedSingleton<OrangeDataManager>.Instance.CARD_TABLE_DICT.TryGetValue((int)item.f_VALUE_Y, out value))
				{
					string s_ICON = value.s_ICON;
					string p_bundleName = AssetBundleScriptableObject.Instance.m_iconCard + string.Format(AssetBundleScriptableObject.Instance.m_icon_card_s_format, value.n_PATCH);
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

	private void OnClickItem(int p_idx)
	{
		ITEM_TABLE item;
		if (!ManagedSingleton<OrangeTableHelper>.Instance.GetItem(p_idx, out item))
		{
			return;
		}
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_ItemInfo", delegate(ItemInfoUI ui)
		{
			if (item.n_TYPE == 5 && item.n_TYPE_X == 1 && (int)item.f_VALUE_Y > 0)
			{
				CARD_TABLE value = null;
				if (ManagedSingleton<OrangeDataManager>.Instance.CARD_TABLE_DICT.TryGetValue((int)item.f_VALUE_Y, out value))
				{
					ui.CanShowHow2Get = false;
					ui.Setup(value, item);
				}
			}
			else
			{
				ui.CanShowHow2Get = false;
				ui.Setup(item);
			}
		});
	}
}
