using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using CallbackDefs;
using Newtonsoft.Json;
using OrangeApi;
using UnityEngine;
using UnityEngine.UI;
using enums;

public class QualifingUI : OrangeUIBase
{
	public class ItemInfo
	{
		public int m_ItemID;

		public int m_ItemCount;
	}

	[SerializeField]
	private Image SocreBarImage;

	[SerializeField]
	private Text SocreBarText;

	[SerializeField]
	private Text RankNameText;

	[SerializeField]
	private Text WinInfoText;

	[SerializeField]
	private Text TitleText;

	[SerializeField]
	private Button GoButton;

	[SerializeField]
	private GameObject RewardListObject;

	[SerializeField]
	private GameObject RecordListObject;

	[SerializeField]
	private Canvas canvasNoResultMsg;

	[SerializeField]
	private GameObject RewardCell;

	[SerializeField]
	private GameObject RankCell;

	[SerializeField]
	private GameObject ScrollRectContent;

	[SerializeField]
	private Text CurrentRankText;

	[SerializeField]
	private Text HighestScoreRankText;

	[SerializeField]
	private Text EventDateText;

	[SerializeField]
	private GameObject RankIconBase;

	[SerializeField]
	private Text BestRankText;

	[SerializeField]
	private LoopVerticalScrollRect ScrollRect;

	[SerializeField]
	private QualifingScrollCell ScrollCell;

	[SerializeField]
	private Transform RankPanelMiniRoot;

	[SerializeField]
	private RawImage[] tModeImgs;

	private RenderTextureObj[] textureObjs = new RenderTextureObj[3];

	[SerializeField]
	private GameObject NewSeasonAnimeRoot;

	[SerializeField]
	private Text NewSeasonText;

	[SerializeField]
	private Image[] NewSeasonRankIcon;

	[SerializeField]
	private Image[] NewSeasonStarIcon;

	[SerializeField]
	private Animator NewSeasonAnimator;

	[SerializeField]
	private Image[] RankIconImage;

	[SerializeField]
	private Image[] RankStarIconImage;

	[SerializeField]
	private GameObject earth_2D;

	private List<NetSeasonInfo> NetSeasonInfo;

	private int m_SeasonID;

	private EVENT_TABLE m_EventTable;

	private int m_Score;

	private int m_RankingInTier;

	private int m_SeasonHighestScore;

	private int m_HaveReward;

	private HUNTERRANK_TABLE m_RankTable;

	private RankPanelMini m_RankPanelMini;

	private RetrievePVPRecordRes m_PVPRecordRes;

	private bool m_bScoreMax;

	private bool bLockLoginMatch;

	private string[] HUNTER_RANK_STR_MAIN_KEY = new string[8] { "HUNTER_RANK_MAIN_1", "HUNTER_RANK_MAIN_2", "HUNTER_RANK_MAIN_3", "HUNTER_RANK_MAIN_4", "HUNTER_RANK_MAIN_5", "HUNTER_RANK_MAIN_6", "HUNTER_RANK_MAIN_7", "HUNTER_RANK_MAIN_8" };

	private string[] HUNTER_RANK_STR_SUB_KEY = new string[5] { "HUNTER_RANK_SUB_1", "HUNTER_RANK_SUB_2", "HUNTER_RANK_SUB_3", "HUNTER_RANK_SUB_4", "HUNTER_RANK_SUB_5" };

	private string[] HUNTER_RANK_STR_NO_COLOR_MAIN_KEY = new string[8] { "HUNTER_RANK_MAIN_1_DIRECT", "HUNTER_RANK_MAIN_2_DIRECT", "HUNTER_RANK_MAIN_3_DIRECT", "HUNTER_RANK_MAIN_4_DIRECT", "HUNTER_RANK_MAIN_5_DIRECT", "HUNTER_RANK_MAIN_6_DIRECT", "HUNTER_RANK_MAIN_7_DIRECT", "HUNTER_RANK_MAIN_8_DIRECT" };

	private string[] HUNTER_RANK_STR_NO_COLOR_SUB_KEY = new string[5] { "HUNTER_RANK_SUB_1_DIRECT", "HUNTER_RANK_SUB_2_DIRECT", "HUNTER_RANK_SUB_3_DIRECT", "HUNTER_RANK_SUB_4_DIRECT", "HUNTER_RANK_SUB_5_DIRECT" };

	private List<ItemInfo> ItemInfoList = new List<ItemInfo>();

	public void OnCloseNewSeasonAnime()
	{
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL);
		NewSeasonAnimeRoot.SetActive(false);
	}

	public void OnShowNewSeasonAnime(bool b, int n_ID)
	{
		if (!b)
		{
			return;
		}
		NetSeasonInfo netSeasonInfo = NetSeasonInfo[n_ID - 1];
		HUNTERRANK_TABLE hunterRankTable = MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.GetHunterRankTable(netSeasonInfo.Score);
		if (hunterRankTable.n_ID > m_RankTable.n_ID)
		{
			NewSeasonAnimeRoot.SetActive(true);
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_RANKDOWN);
			NewSeasonText.text = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("NEW_SEASON_START"), GetHunterRankNameStr(m_RankTable.n_MAIN_RANK));
			NewSeasonRankIcon[0].sprite = RankIconImage[hunterRankTable.n_MAIN_RANK].sprite;
			NewSeasonStarIcon[0].sprite = RankStarIconImage[hunterRankTable.n_SUB_RANK].sprite;
			NewSeasonRankIcon[1].sprite = RankIconImage[m_RankTable.n_MAIN_RANK].sprite;
			NewSeasonStarIcon[1].sprite = RankStarIconImage[m_RankTable.n_SUB_RANK].sprite;
			NewSeasonAnimator.Play("Down");
			LeanTween.delayedCall(3.8f, (Action)delegate
			{
				NewSeasonAnimator.enabled = false;
			});
		}
	}

	public string GetHunterRankNameStr(int RankMain, int RankSub, int RankNumber)
	{
		string text = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr(HUNTER_RANK_STR_MAIN_KEY[RankMain - 1]), RankMain.ToString());
		string text2 = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr(HUNTER_RANK_STR_SUB_KEY[RankSub - 1]), RankSub.ToString());
		string text3 = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("HUNTER_RANK_RANKING"), RankNumber.ToString());
		return text + text2 + text3;
	}

	public string GetHunterRankNameStrNoColor(int RankMain, int RankSub)
	{
		string text = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr(HUNTER_RANK_STR_NO_COLOR_MAIN_KEY[RankMain - 1]), RankMain.ToString());
		string text2 = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr(HUNTER_RANK_STR_NO_COLOR_SUB_KEY[RankSub - 1]), RankSub.ToString());
		return text + text2;
	}

	public string GetHunterRankNameStr(int RankMain, int RankSub)
	{
		string text = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr(HUNTER_RANK_STR_MAIN_KEY[RankMain - 1]), RankMain.ToString());
		string text2 = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr(HUNTER_RANK_STR_SUB_KEY[RankSub - 1]), RankSub.ToString());
		return text + text2;
	}

	public string GetHunterRankNameStr(int RankMain)
	{
		return string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr(HUNTER_RANK_STR_MAIN_KEY[RankMain - 1]), RankMain.ToString());
	}

	private void DeleteTextureObjs()
	{
		earth_2D.SetActive(false);
		for (int i = 0; i < textureObjs.Length; i++)
		{
			if (null != textureObjs[i])
			{
				textureObjs[i].SetCameraActive(false);
				UnityEngine.Object.Destroy(textureObjs[i].gameObject);
			}
			tModeImgs[i].color = new Color32(0, 0, 0, 0);
		}
	}

	private void DrawCharModel(int nCharID, int nWeaponID, int idx)
	{
		if (null != textureObjs[idx])
		{
			UnityEngine.Object.Destroy(textureObjs[idx].gameObject);
		}
		SKIN_TABLE skinTable = null;
		int skin = ManagedSingleton<PlayerNetManager>.Instance.dicCharacter[nCharID].netInfo.Skin;
		if (ManagedSingleton<OrangeDataManager>.Instance.SKIN_TABLE_DICT.ContainsKey(skin))
		{
			skinTable = ManagedSingleton<OrangeDataManager>.Instance.SKIN_TABLE_DICT[skin];
		}
		MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad("prefab/RenderTextureObj", "RenderTextureObj", delegate(GameObject obj)
		{
			textureObjs[idx] = UnityEngine.Object.Instantiate(obj, Vector3.zero, Quaternion.identity).GetComponent<RenderTextureObj>();
			textureObjs[idx].CharacterDebutForceLoop = true;
			if (!ManagedSingleton<OrangeDataManager>.Instance.CHARACTER_TABLE_DICT.ContainsKey(nCharID) || nCharID <= 0)
			{
				nCharID = 1;
			}
			textureObjs[idx].AssignNewRender(ManagedSingleton<OrangeDataManager>.Instance.CHARACTER_TABLE_DICT[nCharID], null, skinTable, new Vector3(0f, -0.6f, 5f), tModeImgs[idx], idx);
			Camera renderCamera = textureObjs[idx].renderCamera;
			renderCamera.orthographic = true;
			renderCamera.orthographicSize = 2.2f;
		});
	}

	private void OnDestroy()
	{
		StopAllCoroutines();
		DeleteTextureObjs();
	}

	public override void OnClickCloseBtn()
	{
		DeleteTextureObjs();
		base.OnClickCloseBtn();
	}

	public void Setup()
	{
		m_bScoreMax = false;
		MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.SeasonCharaterList.Clear();
		Dictionary<int, int> dicSeasonPrepareCharacterIDs = MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.DicSeasonPrepareCharacterIDs;
		bool flag = true;
		foreach (KeyValuePair<int, int> item in dicSeasonPrepareCharacterIDs)
		{
			if (item.Value > 0)
			{
				DrawCharModel(item.Value, 0, item.Key);
				flag = false;
			}
			MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.SeasonCharaterList.Add(item.Value);
		}
		if (flag)
		{
			dicSeasonPrepareCharacterIDs[0] = OrangeConst.INITIAL_CHARA_ID;
			DrawCharModel(OrangeConst.INITIAL_CHARA_ID, 0, 0);
		}
		UpdateCurrentSeasonID();
		if (m_SeasonID == 0)
		{
			MonoBehaviourSingleton<UIManager>.Instance.LoadResourceUI("UI_CommonMsg", delegate(CommonUI ui)
			{
				ui.SetupConfirmByKey("COMMON_TIP", "MESSAGE_ACT_ERROR", "COMMON_OK", delegate
				{
					ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
					DeleteTextureObjs();
					_003C_003En__0();
				});
			});
			return;
		}
		for (int i = 0; i < RankPanelMiniRoot.transform.childCount; i++)
		{
			UnityEngine.Object.Destroy(RankPanelMiniRoot.transform.GetChild(i).gameObject);
		}
		MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(AssetBundleScriptableObject.Instance.m_uiPath + "UI_RankPanelMini", "UI_RankPanelMini", delegate(GameObject asset)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(asset, RankPanelMiniRoot);
			m_RankPanelMini = gameObject.GetComponent<RankPanelMini>();
			ManagedSingleton<PlayerNetManager>.Instance.RetrieveEventRankingReq(m_SeasonID, 1, 10, delegate(List<EventRankingInfo> eventRankingInfoList)
			{
				if (m_RankPanelMini != null)
				{
					m_RankPanelMini.gameObject.SetActive(true);
					m_RankPanelMini.Setup(eventRankingInfoList, m_SeasonID);
				}
			});
		});
		ManagedSingleton<PlayerNetManager>.Instance.RetrievePVPRecordReq(1, delegate(RetrievePVPRecordRes res)
		{
			m_PVPRecordRes = res;
			UpdateWinInfoText();
		});
		ManagedSingleton<PlayerNetManager>.Instance.RetrieveSeasonInfoReq(delegate(RetrieveSeasonInfoRes res)
		{
			m_RankingInTier = res.RankingInTier;
			m_Score = 0;
			m_SeasonHighestScore = 0;
			m_HaveReward = 0;
			InitSeasonInfo();
			NetSeasonInfo = res.SeasonInfoList;
			for (int j = 0; j < NetSeasonInfo.Count; j++)
			{
				if (NetSeasonInfo[j].SeasonID == m_SeasonID)
				{
					m_Score = NetSeasonInfo[j].Score;
					m_SeasonHighestScore = NetSeasonInfo[j].SeasonHighestScore;
					m_HaveReward = NetSeasonInfo[j].HaveReward;
					UpdateHunterRankTable();
					OnShowNewSeasonAnime(res.IsNew == 1 && j > 0, j);
					break;
				}
			}
			UpdateQualifingUI();
		});
		MonoBehaviourSingleton<UIManager>.Instance.CloseLoadingUI(null);
		StartCoroutine(OnSetup());
	}

	private IEnumerator OnSetup()
	{
		yield return CoroutineDefine._waitForEndOfFrame;
	}

	private void InitSeasonInfo()
	{
		List<HUNTERRANK_TABLE> list = ManagedSingleton<OrangeDataManager>.Instance.HUNTERRANK_TABLE_DICT.Values.ToList();
		m_RankTable = list[0];
		MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.CurrentHunterRankTable = m_RankTable;
		m_Score = 0;
		m_SeasonHighestScore = 0;
		m_HaveReward = 0;
	}

	private void UpdateWinInfoText()
	{
		string text = "{0}" + MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_WIN");
		text = text + "{1}" + MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_LOSE");
		text += "<color=#60ff00>({2}%)</color>";
		int totalWins = m_PVPRecordRes.TotalWins;
		int totalRecordCount = m_PVPRecordRes.TotalRecordCount;
		int num = totalRecordCount - totalWins;
		int num2 = ((totalRecordCount > 0) ? (totalWins * 100 / totalRecordCount) : 0);
		WinInfoText.text = string.Format(text, totalWins, num, num2);
	}

	public bool CheckCurrentSeasonEvent()
	{
		long serverUnixTimeNowUTC = MonoBehaviourSingleton<OrangeGameManager>.Instance.ServerUnixTimeNowUTC;
		List<EVENT_TABLE> eventTableByType = ManagedSingleton<ExtendDataHelper>.Instance.GetEventTableByType(enums.EventType.EVENT_PVPSEASON, serverUnixTimeNowUTC);
		if (eventTableByType == null || eventTableByType.Count == 0)
		{
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_Tip", delegate(TipUI tipUI)
			{
				string str = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("RANKING_SEASON_OUTDATE");
				tipUI.Setup(str, true);
			});
			return false;
		}
		return true;
	}

	private void UpdateCurrentSeasonID()
	{
		long serverUnixTimeNowUTC = MonoBehaviourSingleton<OrangeGameManager>.Instance.ServerUnixTimeNowUTC;
		List<EVENT_TABLE> list = ManagedSingleton<ExtendDataHelper>.Instance.GetEventTableByType(enums.EventType.EVENT_PVPSEASON, serverUnixTimeNowUTC);
		if (list == null || list.Count == 0)
		{
			list = ManagedSingleton<ExtendDataHelper>.Instance.GetEventTableRemainByType(enums.EventType.EVENT_PVPSEASON, serverUnixTimeNowUTC);
			if (list == null || list.Count == 0)
			{
				m_SeasonID = 0;
				return;
			}
			GoButton.interactable = false;
		}
		EVENT_TABLE eVENT_TABLE = list[0];
		m_SeasonID = eVENT_TABLE.n_ID;
		m_EventTable = eVENT_TABLE;
	}

	private void UpdateQualifingUI()
	{
		TitleText.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr(m_EventTable.w_NAME);
		float x = Mathf.Clamp((float)m_Score / (float)m_RankTable.n_PT_MAX, 0f, 1f);
		SocreBarImage.transform.localScale = new Vector3(x, 1f, 1f);
		if (m_bScoreMax)
		{
			SocreBarText.text = string.Format("{0}/---", m_Score);
		}
		else
		{
			SocreBarText.text = string.Format("{0}/{1}", m_Score, m_RankTable.n_PT_MAX + 1);
		}
		RankNameText.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("BEST_HUNTER_RANK");
		CurrentRankText.text = GetHunterRankNameStr(m_RankTable.n_MAIN_RANK, m_RankTable.n_SUB_RANK, m_RankingInTier);
		HUNTERRANK_TABLE hunterRankTable = MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.GetHunterRankTable(m_SeasonHighestScore);
		HighestScoreRankText.text = GetHunterRankNameStr(hunterRankTable.n_MAIN_RANK, hunterRankTable.n_SUB_RANK);
		EventDateText.text = OrangeGameUtility.DisplayDatePeriod(m_EventTable.s_BEGIN_TIME, m_EventTable.s_END_TIME);
		RankIconBase.GetComponent<RankIconBase>().Setup(m_RankTable.n_MAIN_RANK, m_RankTable.n_SUB_RANK);
	}

	public void UpdateHunterRankTable()
	{
		List<HUNTERRANK_TABLE> list = ManagedSingleton<OrangeDataManager>.Instance.HUNTERRANK_TABLE_DICT.Values.ToList();
		for (int i = 0; i < list.Count; i++)
		{
			if (i == list.Count - 1)
			{
				m_bScoreMax = true;
			}
			m_RankTable = list[i];
			MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.CurrentHunterRankTable = m_RankTable;
			if (m_Score <= list[i].n_PT_MAX)
			{
				break;
			}
		}
	}

	public void OnClickRules()
	{
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
		STAGE_TABLE stage = null;
		if (ManagedSingleton<OrangeTableHelper>.Instance.GetStage(900001, out stage))
		{
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_CommonScrollMsg", delegate(CommonScrollMsgUI ui)
			{
				ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
				ui.Setup(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_RULE"), ManagedSingleton<OrangeTextDataManager>.Instance.STAGETEXT_TABLE_DICT.GetL10nValue(stage.w_TIP));
			});
		}
	}

	public void OnClickRanking()
	{
	}

	private float MakeRewardList(int rank, string RankNameText)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(RankCell, ScrollRectContent.transform.position, new Quaternion(0f, 0f, 0f, 0f));
		gameObject.transform.SetParent(ScrollRectContent.transform);
		gameObject.transform.localScale = new Vector3(1f, 1f, 1f);
		gameObject.SetActive(true);
		GameObject gLG = gameObject.GetComponent<RankScrollCell>().GetGLG();
		gameObject.GetComponent<RankScrollCell>().SetRankText(rank, RankNameText);
		int childCount = gLG.transform.childCount;
		for (int i = 0; i < childCount; i++)
		{
			UnityEngine.Object.Destroy(gLG.transform.GetChild(i).gameObject);
		}
		for (int j = 0; j < ItemInfoList.Count; j++)
		{
			ITEM_TABLE tbl = ManagedSingleton<ExtendDataHelper>.Instance.ITEM_TABLE_DICT[ItemInfoList[j].m_ItemID];
			GameObject obj = UnityEngine.Object.Instantiate(RewardCell, gLG.transform.position, new Quaternion(0f, 0f, 0f, 0f));
			obj.transform.SetParent(gLG.transform);
			obj.transform.localScale = new Vector3(1f, 1f, 1f);
			obj.SetActive(true);
			obj.GetComponent<RankRewardScrollCell>().SetupItem(rank, 1, ItemInfoList[j].m_ItemCount, tbl);
		}
		int num = (int)Math.Ceiling((float)ItemInfoList.Count / 2f);
		float num2 = 160 * num + 95;
		Vector2 sizeDelta = gameObject.GetComponent<RectTransform>().sizeDelta;
		gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(sizeDelta.x, num2);
		return num2 + 10f;
	}

	public void OnCickRewardBtn()
	{
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
		MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad<GameObject>(AssetBundleScriptableObject.Instance.m_uiPath + "CommonIconBaseSmall", "CommonIconBaseSmall", delegate
		{
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_PvpReward", delegate(PvpRewardUI ui)
			{
				ui.Setup();
			});
		});
	}

	public void OnShowRewardListUI(bool b)
	{
		if (b)
		{
			List<int> list = (from q in ManagedSingleton<OrangeDataManager>.Instance.MISSION_TABLE_DICT
				where q.Value.n_TYPE == 104
				select q.Key).ToList();
			ItemInfoList.Clear();
			int childCount = ScrollRectContent.transform.childCount;
			for (int i = 0; i < childCount; i++)
			{
				UnityEngine.Object.Destroy(ScrollRectContent.transform.GetChild(i).gameObject);
			}
			float num = 0f;
			for (int j = 0; j < list.Count; j++)
			{
				if (ManagedSingleton<OrangeDataManager>.Instance.MISSION_TABLE_DICT.ContainsKey(list[j]))
				{
					MISSION_TABLE mISSION_TABLE = ManagedSingleton<OrangeDataManager>.Instance.MISSION_TABLE_DICT[list[j]];
					if (mISSION_TABLE.n_ITEMID_1 != 0)
					{
						ItemInfo itemInfo = new ItemInfo();
						itemInfo.m_ItemID = mISSION_TABLE.n_ITEMID_1;
						itemInfo.m_ItemCount = mISSION_TABLE.n_ITEMCOUNT_1;
						ItemInfoList.Add(itemInfo);
					}
					if (mISSION_TABLE.n_ITEMID_2 != 0)
					{
						ItemInfo itemInfo2 = new ItemInfo();
						itemInfo2.m_ItemID = mISSION_TABLE.n_ITEMID_2;
						itemInfo2.m_ItemCount = mISSION_TABLE.n_ITEMCOUNT_2;
						ItemInfoList.Add(itemInfo2);
					}
					if (mISSION_TABLE.n_ITEMID_3 != 0)
					{
						ItemInfo itemInfo3 = new ItemInfo();
						itemInfo3.m_ItemID = mISSION_TABLE.n_ITEMID_3;
						itemInfo3.m_ItemCount = mISSION_TABLE.n_ITEMCOUNT_3;
						ItemInfoList.Add(itemInfo3);
					}
					if (ItemInfoList.Count > 0 && (list.Count <= j + 1 || !ManagedSingleton<OrangeDataManager>.Instance.MISSION_TABLE_DICT.ContainsKey(list[j + 1]) || ManagedSingleton<OrangeDataManager>.Instance.MISSION_TABLE_DICT[list[j + 1]].n_CONDITION_Y != mISSION_TABLE.n_CONDITION_Y))
					{
						string rankNameText = GetHunterRankNameStrNoColor(ManagedSingleton<OrangeDataManager>.Instance.HUNTERRANK_TABLE_DICT[mISSION_TABLE.n_CONDITION_Y].n_MAIN_RANK, ManagedSingleton<OrangeDataManager>.Instance.HUNTERRANK_TABLE_DICT[mISSION_TABLE.n_CONDITION_Y].n_SUB_RANK) + " ~ " + GetHunterRankNameStrNoColor(ManagedSingleton<OrangeDataManager>.Instance.HUNTERRANK_TABLE_DICT[mISSION_TABLE.n_CONDITION_Z].n_MAIN_RANK, ManagedSingleton<OrangeDataManager>.Instance.HUNTERRANK_TABLE_DICT[mISSION_TABLE.n_CONDITION_Z].n_SUB_RANK);
						num += MakeRewardList(ManagedSingleton<OrangeDataManager>.Instance.HUNTERRANK_TABLE_DICT[mISSION_TABLE.n_CONDITION_Y].n_MAIN_RANK, rankNameText);
						ItemInfoList.Clear();
					}
				}
			}
			Vector2 sizeDelta = ScrollRectContent.GetComponent<RectTransform>().sizeDelta;
			ScrollRectContent.GetComponent<RectTransform>().sizeDelta = new Vector2(sizeDelta.x, num);
			HUNTERRANK_TABLE hunterRankTable = MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.GetHunterRankTable(m_SeasonHighestScore);
			BestRankText.text = GetHunterRankNameStr(hunterRankTable.n_MAIN_RANK, hunterRankTable.n_SUB_RANK);
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
		}
		else
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL);
		}
		RewardListObject.SetActive(b);
	}

	public void OnShowRerocdListUI(bool b)
	{
		RecordListObject.SetActive(b);
		if (b)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
			RetrievePVPRecord();
		}
		else
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL);
		}
	}

	public void OnPreparing()
	{
		if (bLockLoginMatch)
		{
			MonoBehaviourSingleton<OrangeGameManager>.Instance.ShowTipDialog(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("TIP_WAITFORMATCH"), 42);
			return;
		}
		MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.CurrentScore = m_Score;
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK13);
		bool flag = CheckCurrentSeasonEvent();
		if (!flag)
		{
			GoButton.interactable = flag;
			return;
		}
		MonoBehaviourSingleton<OrangeMatchManager>.Instance.StageID = 900001;
		MonoBehaviourSingleton<OrangeMatchManager>.Instance.PvpGameType = PVPGameType.OneVSOneSeason;
		MonoBehaviourSingleton<OrangeMatchManager>.Instance.PvpMatchType = PVPMatchType.OneVSOne;
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_GoCheck", delegate(GoCheckUI ui)
		{
			MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.SeasonCharaterList.Clear();
			Dictionary<int, int> dicSeasonPrepareCharacterIDs = MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.DicSeasonPrepareCharacterIDs;
			foreach (KeyValuePair<int, int> item in dicSeasonPrepareCharacterIDs)
			{
				MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.SeasonCharaterList.Add(item.Value);
			}
			ManagedSingleton<PlayerHelper>.Instance.UpdateMatchHunterRankTable(m_Score);
			ui.Setup(MonoBehaviourSingleton<OrangeMatchManager>.Instance.StageID);
			ui.SetSeasonInfo(dicSeasonPrepareCharacterIDs.Values.ToArray());
			ui.bJustReturnToLastUI = true;
			ui.bFromQualifingUI = true;
			ui.closeCB = (Callback)Delegate.Combine(ui.closeCB, (Callback)delegate
			{
				CheckSelfData();
			});
			DeleteTextureObjs();
			ui.destroyCB = (Callback)Delegate.Combine(ui.destroyCB, new Callback(Setup));
		});
	}

	private void CheckSelfData()
	{
		Setup();
		bool flag = CheckCurrentSeasonEvent();
		if (!flag)
		{
			GoButton.interactable = flag;
			return;
		}
		if (!MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.CheckSeasonCharaterList())
		{
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_Tip", delegate(TipUI tipUI)
			{
				string str = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("RANKING_PREPARE_FAILED");
				tipUI.Setup(str, true);
			});
			return;
		}
		bLockLoginMatch = true;
		MonoBehaviourSingleton<OrangeMatchManager>.Instance.Host = ManagedSingleton<ServerConfig>.Instance.ServerSetting.Match.Host;
		MonoBehaviourSingleton<OrangeMatchManager>.Instance.Port = ManagedSingleton<ServerConfig>.Instance.ServerSetting.Match.Port;
		MonoBehaviourSingleton<OrangeMatchManager>.Instance.MatchServerLogin(delegate
		{
			MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.OnInitSeasonCharaterMaxHPList();
			ManagedSingleton<PlayerNetManager>.Instance.SealBattleSettingReq(MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.SeasonCharaterList, delegate(string setting)
			{
				bLockLoginMatch = false;
				MonoBehaviourSingleton<OrangeMatchManager>.Instance.SelfSealedBattleSetting = setting;
				MonoBehaviourSingleton<OrangeMatchManager>.Instance.PVPRandomMatching();
				PlayerPrefs.SetString("SeasonCharaterList", JsonConvert.SerializeObject(MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.SeasonCharaterList));
			}, MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.nMainWeaponID, MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.nSubWeaponID);
		}, false);
	}

	public void RetrievePVPRecord()
	{
		ManagedSingleton<PlayerNetManager>.Instance.RetrievePVPRecordReq(1, delegate(RetrievePVPRecordRes res)
		{
			m_PVPRecordRes = res;
			UpdateWinInfoText();
			ScrollRect.ClearCells();
			ScrollRect.OrangeInit(ScrollCell, 4, m_PVPRecordRes.PVPRecordList.Count);
			if ((bool)canvasNoResultMsg)
			{
				canvasNoResultMsg.enabled = m_PVPRecordRes.PVPRecordList.Count == 0;
			}
		});
	}

	public NetPVPRecord OnGetNetRecordList(int idx)
	{
		return m_PVPRecordRes.PVPRecordList[idx];
	}

	public override void SetCanvas(bool enable)
	{
		base.SetCanvas(enable);
		for (int i = 0; i < textureObjs.Length; i++)
		{
			if (null != textureObjs[i])
			{
				textureObjs[i].SetCameraActive(enable);
			}
		}
	}

	protected override bool IsEscapeVisible()
	{
		if (RewardListObject.activeSelf)
		{
			return false;
		}
		if (RecordListObject.activeSelf)
		{
			return false;
		}
		return base.IsEscapeVisible();
	}

	[CompilerGenerated]
	[DebuggerHidden]
	private void _003C_003En__0()
	{
		base.OnClickCloseBtn();
	}
}
