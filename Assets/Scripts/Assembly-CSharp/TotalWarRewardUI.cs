using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.UI;

public class TotalWarRewardUI : OrangeUIBase
{
	private EVENT_TABLE tET;

	[Header("TotalWarRecordUI")]
	public GameObject refCommonIconBase;

	public GameObject ItemBarScoreGroup;

	public GameObject ItemBarStageGroup;

	public GameObject[] ItemRankGroup;

	public GameObject[] LabelRank;

	public GameObject[] BattleRank;

	public Toggle[] Toggle;

	public GameObject ScoreRewardGroup;

	public RectTransform SVContent01;

	public GameObject ScoreNumRoot;

	public Text ScoreNumRootText;

	public Text ScoreNumRootNum;

	public Text HintMsgText;

	public Text HintMsgText2;

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

	[SerializeField]
	[ReadOnly]
	private Toggle currentToggle;

	public Vector2 spacing = Vector2.zero;

	public Vector3 ToggleSpacing = Vector2.zero;

	private Vector2 currentPos = Vector2.zero;

	private bool bNextLine;

	private Vector2 lastsizeDelta;

	private int[] nNowScore = new int[3];

	private int nNowRank;

	private STAGE_TABLE tNowStage;

	private int m_rewardAvailableCount;

	private int nNowRewardIndex;

	private bool b_first = true;

	private bool bCanGet;

	private List<NetRewardInfo> rewardList = new List<NetRewardInfo>();

	public void Setup(int nEventID, int[] nScore, STAGE_TABLE tStage, int nRank, bool inbCanGet)
	{
		if (!ManagedSingleton<OrangeDataManager>.Instance.EVENT_TABLE_DICT.TryGetValue(nEventID, out tET))
		{
			tET = new EVENT_TABLE();
		}
		nNowScore = nScore;
		tNowStage = tStage;
		nNowRank = nRank;
		ScoreNumRootText.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("TOTALWAR_RANK_POINT");
		ScoreNumRootText.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("EVENT_STAGE_RANKING");
		ScoreNumRootText.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("RAID_CRUSADE_STEP");
		ScoreNumRootNum.text = nScore.ToString();
		BtnGetAll.gameObject.SetActive(false);
		ScoreNumRoot.SetActive(false);
		HintMsgText.text = "";
		b_first = true;
		bCanGet = inbCanGet;
		SwitchToggleBtns(15);
		base._EscapeEvent = EscapeEvent.CLOSE_UI;
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
					flag = true;
				}
			}
			else
			{
				Toggle[i].gameObject.SetActive(false);
			}
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

	private void InitPointRewaed(int nPoint, int nCondition, int nConditionX)
	{
		m_rewardAvailableCount = 0;
		KeyValuePair<int, MISSION_TABLE>[] array = (from q in ManagedSingleton<OrangeDataManager>.Instance.MISSION_TABLE_DICT
			where q.Value.n_TYPE == 102 && q.Value.n_SUB_TYPE == nPoint && q.Value.n_CONDITION == nCondition && q.Value.n_CONDITION_X == nConditionX
			orderby q.Value.n_CONDITION_Y
			select q).ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			bool bCompleted = ManagedSingleton<MissionHelper>.Instance.CheckMissionCompleted(array[i].Value.n_ID) && bCanGet;
			bool bRetrieved = ManagedSingleton<MissionHelper>.Instance.CheckMissionRewardRetrieved(array[i].Value.n_ID) && bCanGet;
			if (array[i].Value.n_ITEMID_1 != 0)
			{
				AddItemWithGetBtn(array[i].Value.n_ITEMID_1, array[i].Value.n_ITEMCOUNT_1, array[i].Value.n_CONDITION_Y, array[i].Value.n_ID, bCompleted, bRetrieved);
			}
			if (array[i].Value.n_ITEMID_2 != 0)
			{
				AddItemWithGetBtn(array[i].Value.n_ITEMID_2, array[i].Value.n_ITEMCOUNT_2, array[i].Value.n_CONDITION_Y, array[i].Value.n_ID, bCompleted, bRetrieved);
			}
			if (array[i].Value.n_ITEMID_3 != 0)
			{
				AddItemWithGetBtn(array[i].Value.n_ITEMID_3, array[i].Value.n_ITEMCOUNT_3, array[i].Value.n_CONDITION_Y, array[i].Value.n_ID, bCompleted, bRetrieved);
			}
		}
		BtnGetAll.gameObject.SetActive(m_rewardAvailableCount > 0);
		HintMsgText2.gameObject.SetActive(m_rewardAvailableCount == 0);
	}

	public void OneRType0Page(bool bIsOn)
	{
		if (bIsOn)
		{
			if (currentToggle != Toggle[0])
			{
				PlaySystemSECheckFirst(m_ToggleBtn);
				currentToggle = Toggle[0];
			}
			InitLineStatus();
			ScoreNumRoot.SetActive(true);
			ScoreNumRootText.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("TOTALWAR_RANK_POINT");
			ScoreNumRootNum.text = nNowScore[0].ToString();
			HintMsgText.text = "";
			nNowRewardIndex = 127;
			InitPointRewaed(tET.n_POINT, 127, tET.n_ID);
		}
	}

	public void OneRType1Page(bool bIsOn)
	{
		if (bIsOn)
		{
			if (currentToggle != Toggle[1])
			{
				PlaySystemSECheckFirst(m_ToggleBtn);
				currentToggle = Toggle[1];
			}
			InitLineStatus();
			ScoreNumRoot.SetActive(true);
			ScoreNumRootText.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("TOTALWAR_RANK_POINT");
			ScoreNumRootNum.text = nNowScore[1].ToString();
			HintMsgText.text = "";
			nNowRewardIndex = 128;
			InitPointRewaed(tET.n_POINT, 128, tET.n_ID);
		}
	}

	public void OneRType2Page(bool bIsOn)
	{
		if (bIsOn)
		{
			if (currentToggle != Toggle[2])
			{
				PlaySystemSECheckFirst(m_ToggleBtn);
				currentToggle = Toggle[2];
			}
			InitLineStatus();
			ScoreNumRoot.SetActive(true);
			ScoreNumRootText.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("TOTALWAR_RANK_POINT");
			ScoreNumRootNum.text = nNowScore[2].ToString();
			HintMsgText.text = "";
			nNowRewardIndex = 129;
			InitPointRewaed(tET.n_POINT, 129, tET.n_ID);
		}
	}

	public void OneRType3Page(bool bIsOn)
	{
		if (!bIsOn)
		{
			return;
		}
		if (currentToggle != Toggle[3])
		{
			PlaySystemSECheckFirst(m_ToggleBtn);
			currentToggle = Toggle[3];
		}
		InitLineStatus();
		BtnGetAll.gameObject.SetActive(false);
		HintMsgText2.gameObject.SetActive(false);
		ScoreNumRoot.SetActive(true);
		ScoreNumRootText.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("EVENT_STAGE_RANKING");
		ScoreNumRootNum.text = nNowRank.ToString();
		nNowRewardIndex = 0;
		HintMsgText.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("RESULT_REWARD_SEND_TIP");
		KeyValuePair<int, MISSION_TABLE>[] array = (from q in ManagedSingleton<OrangeDataManager>.Instance.MISSION_TABLE_DICT
			where q.Value.n_TYPE == 113 && q.Value.n_CONDITION_X == 0 && q.Value.n_SUB_TYPE == tET.n_RANKING
			orderby q.Value.n_CONDITION_Y
			select q).ToArray();
		List<List<MISSION_TABLE>> list = new List<List<MISSION_TABLE>>();
		bool flag = false;
		for (int i = 0; i < array.Length; i++)
		{
			flag = false;
			for (int j = 0; j < list.Count; j++)
			{
				if (array[i].Value.n_CONDITION_Y == list[j][0].n_CONDITION_Y && array[i].Value.n_CONDITION_Z == list[j][0].n_CONDITION_Z)
				{
					list[j].Add(array[i].Value);
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				List<MISSION_TABLE> list2 = new List<MISSION_TABLE>();
				list2.Add(array[i].Value);
				list.Add(list2);
			}
		}
		int num = 0;
		for (int k = 0; k < list.Count; k++)
		{
			num = ((k >= 4) ? 4 : (k + 1));
			AddRank(num, list[k][0].n_CONDITION_Y, list[k][0].n_CONDITION_Z);
			for (int l = 0; l < list[k].Count; l++)
			{
				if (list[k][l].n_ITEMID_1 != 0)
				{
					AddRankItem(list[k][l].n_ITEMID_1, list[k][l].n_ITEMCOUNT_1, k, num);
				}
				if (list[k][l].n_ITEMID_2 != 0)
				{
					AddRankItem(list[k][l].n_ITEMID_2, list[k][l].n_ITEMCOUNT_2, k, num);
				}
				if (list[k][l].n_ITEMID_3 != 0)
				{
					AddRankItem(list[k][l].n_ITEMID_3, list[k][l].n_ITEMCOUNT_3, k, num);
				}
			}
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
			BtnGetAll.interactable = m_rewardAvailableCount > 0;
		});
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
		b_first = true;
		switch (nNowRewardIndex)
		{
		case 127:
			OneRType0Page(true);
			break;
		case 128:
			OneRType1Page(true);
			break;
		case 129:
			OneRType2Page(true);
			break;
		case 0:
			OneRType3Page(true);
			break;
		}
	}

	private void AddItemWithGetBtn(int itemId, int amount, int score, int n_ID, bool bCompleted, bool bRetrieved)
	{
		lastsizeDelta = ((RectTransform)ItemBarScoreGroup.transform).sizeDelta;
		Transform transform = Object.Instantiate(ItemBarScoreGroup.transform, SVContent01);
		transform.localPosition = currentPos;
		transform.transform.Find("TextItemName").GetComponent<OrangeText>().text = ItemIconHelper(transform, itemId, amount);
		transform.transform.Find("TextScore").GetComponent<OrangeText>().text = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("TOTALWAR_RANK_POINTS"), score);
		Button getBtn = transform.transform.Find("BtnGet").GetComponent<Button>();
		Button gotBtn = transform.transform.Find("BtnGot").GetComponent<Button>();
		transform.gameObject.SetActive(true);
		if (!bCompleted)
		{
			gotBtn.gameObject.SetActive(false);
			getBtn.gameObject.SetActive(false);
		}
		else
		{
			gotBtn.gameObject.SetActive(bRetrieved);
			getBtn.gameObject.SetActive(!bRetrieved);
			if (!bRetrieved)
			{
				m_rewardAvailableCount++;
			}
		}
		getBtn.onClick.AddListener(delegate
		{
			getBtn.gameObject.SetActive(false);
			OnGetReward(gotBtn, n_ID);
		});
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

	private string ItemIconHelper(Transform target, int itemId, int amount)
	{
		ItemIconWithAmount componentInChildren = target.GetComponentInChildren<ItemIconWithAmount>();
		ITEM_TABLE item;
		if (ManagedSingleton<OrangeTableHelper>.Instance.GetItem(itemId, out item))
		{
			if (item.n_TYPE == 5 && item.n_TYPE_X == 1 && (int)item.f_VALUE_Y > 0)
			{
				CARD_TABLE value = null;
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
		ITEM_TABLE item = null;
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

	private void PlaySystemSECheckFirst(SystemSE cueid)
	{
		if (b_first)
		{
			b_first = false;
		}
		else
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(cueid);
		}
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
		}
	}

	public List<int> CollectReceivableEventMissionList()
	{
		if (nNowRewardIndex == 0)
		{
			return new List<int>();
		}
		KeyValuePair<int, MISSION_TABLE>[] array = (from q in ManagedSingleton<OrangeDataManager>.Instance.MISSION_TABLE_DICT
			where q.Value.n_TYPE == 102 && q.Value.n_SUB_TYPE == tET.n_POINT && q.Value.n_CONDITION == nNowRewardIndex && q.Value.n_CONDITION_X == tET.n_ID
			orderby q.Value.n_CONDITION_Y
			select q).ToArray();
		List<int> list = new List<int>();
		KeyValuePair<int, MISSION_TABLE>[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			KeyValuePair<int, MISSION_TABLE> keyValuePair = array2[i];
			if (!ManagedSingleton<MissionHelper>.Instance.CheckMissionRewardRetrieved(keyValuePair.Value.n_ID) && ManagedSingleton<MissionHelper>.Instance.CheckMissionCompleted(keyValuePair.Value.n_ID))
			{
				list.Add(keyValuePair.Value.n_ID);
			}
			if (list.Count >= 20)
			{
				break;
			}
		}
		return list;
	}
}
