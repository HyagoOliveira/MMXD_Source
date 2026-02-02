#define RELEASE
using System.Collections.Generic;
using System.Linq;
using OrangeApi;
using OrangeSocket;
using UnityEngine;
using UnityEngine.UI;
using cc;
using enums;

public class RankingMainUI : OrangeUIBase
{
	public class GuildRankingData
	{
		public int Rank;

		public NetGuildInfo GuildInfo;

		public long Score;
	}

	private enum RANKING_BOARD_TEXT
	{
		TYPE_0 = 0,
		TYPE_1 = 1,
		TYPE_2 = 2,
		TYPE_3 = 3,
		TYPE_4 = 4,
		TYPE_5 = 5,
		TYPE_6 = 6,
		TYPE_7 = 7,
		TYPE_8 = 8,
		TYPE_9 = 9,
		TYPE_10 = 10,
		TYPE_11 = 11,
		TYPE_12 = 12,
		TYPE_13 = 13,
		TYPE_14 = 14,
		TYPE_CRUSADE = 15,
		TYPE_15 = 16
	}

	[SerializeField]
	private LoopVerticalScrollRect _scrollRect;

	[SerializeField]
	private RankingScrollCell _playerRankingScrollCell;

	[SerializeField]
	private GuildRankingScrollCell _guildRankingScrollCell;

	[SerializeField]
	private VerticalLayoutGroup Content;

	[SerializeField]
	private GameObject ButtonGroup;

	[SerializeField]
	private Button WorldBtn;

	[SerializeField]
	private Button FriendBtn;

	[SerializeField]
	private Text[] SelectTopInfoText;

	[SerializeField]
	private Button CurrentBtn;

	[SerializeField]
	private Button PersonalBtn;

	[SerializeField]
	private Scrollbar ScrollbarObj;

	private CHARACTER_TABLE characterTable;

	private CharacterInfo characterInfo;

	private CharacterInfoBackground characterInfoBackground;

	private RenderTextureObj textureObj;

	private CharacterInfoSelect characterInfoSelect;

	private int? currentSelectionIndex;

	[SerializeField]
	private RawImage tModeImg;

	[SerializeField]
	private Text[] BoardName;

	[SerializeField]
	private Text[] BoardMessage;

	[SerializeField]
	private Text TargetRank;

	[SerializeField]
	private Text TargetName;

	[SerializeField]
	private Image[] Badges;

	[SerializeField]
	public Text PlayerRankText;

	[SerializeField]
	public Text PlayerNameText;

	[SerializeField]
	public Text PlayerScoreText;

	[SerializeField]
	private Image[] PlayerBadges;

	[SerializeField]
	private Image[] PlayerBadgeBGs;

	private List<GameObject> mItemList = new List<GameObject>();

	private int CurrentTarget;

	private int CurrentType;

	private bool CurrentSelect;

	private bool CurrentStartSelect;

	private int CurrentStart;

	private int CurrentEnd;

	private int CurrentTotalCount;

	private string CurrectTouchPlayerID;

	private bool isChangeModeRead = true;

	private bool isShowEventRanking;

	private List<StorageInfo> listStorage = new List<StorageInfo>();

	[SerializeField]
	private Transform storageRoot;

	private CanvasGroup _storageCanvas;

	private bool GetFlag = true;

	private Color32[] colors = new Color32[2]
	{
		new Color32(52, 47, 58, byte.MaxValue),
		new Color32(185, 234, byte.MaxValue, byte.MaxValue)
	};

	private List<EventRankingInfo> eventRankingInfoList;

	private int CurrentSelectEventID;

	private int CurrentSelectEventType;

	private string[] TempPlayerIDList;

	private STAGE_TABLE bossRushStageTable;

	public int CurrectTouchIndex { get; private set; }

	public List<GuildRankingData> GuildRankingDataCache { get; set; } = new List<GuildRankingData>();


	public bool GetShowEventRankingFlag()
	{
		return isShowEventRanking;
	}

	protected override void Awake()
	{
		base.Awake();
		_storageCanvas = storageRoot.GetComponent<CanvasGroup>();
	}

	public void Setup(int EventID = 0)
	{
		MonoBehaviourSingleton<SignalDispatcher>.Instance.AddHandler(CC.RSGetPlayerHUDList, OnCreateRSGetPlayerHUDListCallback);
		Singleton<GenericEventManager>.Instance.AttachEvent(EventManager.ID.UI_RANKING_CHARACTER_CHANGE, NotifyCharacterChange);
		CurrentType = 1;
		CurrentStartSelect = false;
		PersonalBtn.gameObject.SetActive(CurrentStartSelect);
		CurrentBtn.gameObject.SetActive(!CurrentStartSelect);
		CurrentSelect = true;
		CurrentTarget = (CurrentSelect ? 1 : 0);
		WorldBtn.interactable = !CurrentSelect;
		FriendBtn.interactable = CurrentSelect;
		CreateNewStorageTab(CurrentSelect, EventID);
	}

	private void DrawCharModel(int nCharID, int nWeaponID, int nCharSkin)
	{
		if (null != textureObj)
		{
			Object.Destroy(textureObj.gameObject);
		}
		ModelRotateDrag objDrag = tModeImg.GetComponent<ModelRotateDrag>();
		MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad("prefab/RenderTextureObj", "RenderTextureObj", delegate(GameObject obj)
		{
			textureObj = Object.Instantiate(obj, Vector3.zero, Quaternion.identity).GetComponent<RenderTextureObj>();
			if (nCharID <= 0)
			{
				Debug.LogWarning(string.Format("[{0}] Invalid CharaID {1} and force set to 1", "DrawCharModel", nCharID));
				nCharID = 1;
			}
			if (nWeaponID <= 0)
			{
				Debug.LogWarning(string.Format("[{0}] Invalid WeaponID {1} and force set to 1", "DrawCharModel", nWeaponID));
				nWeaponID = 1;
			}
			CHARACTER_TABLE value;
			if (!ManagedSingleton<OrangeDataManager>.Instance.CHARACTER_TABLE_DICT.TryGetValue(nCharID, out value))
			{
				Debug.LogError(string.Format("[{0}] Invalid CharaID {1} of {2}", "DrawCharModel", nCharID, "CHARACTER_TABLE_DICT"));
			}
			else
			{
				SKIN_TABLE value2;
				if (!ManagedSingleton<OrangeDataManager>.Instance.SKIN_TABLE_DICT.TryGetValue(nCharSkin, out value2))
				{
					Debug.LogWarning(string.Format("[{0}] Invalid SkinID {1} of {2} and will be null", "DrawCharModel", nCharSkin, "SKIN_TABLE_DICT"));
				}
				WEAPON_TABLE value3;
				if (ManagedSingleton<OrangeDataManager>.Instance.WEAPON_TABLE_DICT.TryGetValue(nWeaponID, out value3))
				{
					if (value3.n_ENABLE_FLAG == 0)
					{
						Debug.LogWarning(string.Format("[{0}] WeaponID {1} not enabled and force set to 1", "DrawCharModel", nWeaponID));
						nWeaponID = 1;
						ManagedSingleton<OrangeDataManager>.Instance.WEAPON_TABLE_DICT.TryGetValue(nWeaponID, out value3);
					}
				}
				else
				{
					Debug.LogWarning(string.Format("[{0}] Invalid WeaponID {1} of {2} and will be null", "DrawCharModel", nWeaponID, "WEAPON_TABLE_DICT"));
				}
				if (textureObj.RenderPosition != null)
				{
					textureObj.AssignNewRender(value, value3, value2, new Vector3(0f, -0.6f, 5f), tModeImg);
					if ((bool)objDrag)
					{
						objDrag.SetModelTransform(textureObj.RenderPosition);
					}
				}
				else
				{
					Debug.LogError("[DrawCharModel] RenderPosition is null");
				}
			}
		});
	}

	public void NotifyCharacterChange()
	{
		isChangeModeRead = true;
		MonoBehaviourSingleton<UIManager>.Instance.Block(false);
	}

	private void OnEnable()
	{
		Singleton<GenericEventManager>.Instance.AttachEvent(EventManager.ID.BACK_TO_HOMETOP, OnBackToHometop);
		Singleton<CrusadeSystem>.Instance.OnRetrieveEventRankingEvent += OnRetrieveCrusadeRankingEvent;
	}

	private void OnDisable()
	{
		Singleton<GenericEventManager>.Instance.DetachEvent(EventManager.ID.BACK_TO_HOMETOP, OnBackToHometop);
		Singleton<CrusadeSystem>.Instance.OnRetrieveEventRankingEvent -= OnRetrieveCrusadeRankingEvent;
	}

	private void OnDestroy()
	{
		MonoBehaviourSingleton<SignalDispatcher>.Instance.RemoveHandler(CC.RSGetPlayerHUDList, OnCreateRSGetPlayerHUDListCallback);
		Singleton<GenericEventManager>.Instance.DetachEvent(EventManager.ID.UI_RANKING_CHARACTER_CHANGE, NotifyCharacterChange);
		if (null != textureObj)
		{
			textureObj.SetCameraActive(false);
			Object.Destroy(textureObj.gameObject);
		}
		tModeImg.color = new Color32(0, 0, 0, 0);
	}

	private void SelectBoardText(int typ)
	{
		for (int i = 0; i < BoardMessage.Length; i++)
		{
			BoardMessage[i].enabled = false;
			BoardName[i].enabled = false;
		}
		BoardMessage[typ].enabled = true;
		BoardName[typ].enabled = true;
	}

	public void SetRankingList()
	{
		if (CurrentTarget == 0)
		{
			OrangeCommunityManager.m_RankingInfo = new List<SocketRankingInfo>(MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicFriendRankingList[(RankType)CurrentType]);
		}
		else
		{
			OrangeCommunityManager.m_RankingInfo = new List<SocketRankingInfo>(MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicRankingList[(RankType)CurrentType]);
		}
		string currentPlayerID = MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.CurrentPlayerID;
		List<string> list = OrangeCommunityManager.m_RankingInfo.Select((SocketRankingInfo rankingInfo) => rankingInfo.m_PlayerId).Distinct().ToList();
		if (!list.Contains(currentPlayerID))
		{
			list.Add(currentPlayerID);
		}
		Singleton<GuildSystem>.Instance.RefreshCommunityPlayerGuildInfoCache(list, InitializeRankingList);
	}

	private void InitializeRankingList()
	{
		SocketRankingTypeInfo socketRankingTypeInfo = ((CurrentTarget != 0) ? MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicPlayerRank[(RankType)CurrentType] : MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicPlayerFriendRank[(RankType)CurrentType]);
		if (_scrollRect != null)
		{
			_scrollRect.ClearCells();
		}
		_scrollRect.OrangeInit(_playerRankingScrollCell, 5, OrangeCommunityManager.m_RankingInfo.Count);
		PlayerNameText.text = ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.Nickname;
		PlayerScoreText.text = string.Concat(socketRankingTypeInfo.m_Score);
		string text = ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.Nickname;
		int num = ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.MainWeaponID;
		if (CurrentType == 3)
		{
			num = MonoBehaviourSingleton<OrangeCommunityManager>.Instance.OnGetBestWeaponID(false);
			text = ManagedSingleton<OrangeTextDataManager>.Instance.WEAPONTEXT_TABLE_DICT.GetL10nValue(ManagedSingleton<OrangeDataManager>.Instance.WEAPON_TABLE_DICT[num].w_NAME);
		}
		_playerRankingScrollCell.PlayerSetCellData(socketRankingTypeInfo.m_Rank, MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify, text, socketRankingTypeInfo.m_Score, ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.StandbyChara, num);
		MonoBehaviourSingleton<OrangeCommunityManager>.Instance.OnGetBestWeaponID(false);
		for (int i = 0; i < PlayerBadges.Length; i++)
		{
			PlayerBadges[i].enabled = false;
			PlayerBadgeBGs[i].enabled = false;
		}
		if (socketRankingTypeInfo.m_Rank < 3)
		{
			PlayerBadges[socketRankingTypeInfo.m_Rank].enabled = true;
			PlayerBadgeBGs[socketRankingTypeInfo.m_Rank].enabled = true;
		}
		PlayerRankText.text = ((socketRankingTypeInfo.m_Rank >= 999) ? "999+" : string.Concat(socketRankingTypeInfo.m_Rank + 1));
		if (CurrentStart == 0)
		{
			CurrectTouchIndex = 0;
			SetDefaultRankingScrollCellMessage(0);
		}
		_storageCanvas.blocksRaycasts = true;
		MonoBehaviourSingleton<UIManager>.Instance.Connecting(false);
	}

	public void SetPersonalRankingList()
	{
		SocketRankingTypeInfo RankingInfo = new SocketRankingTypeInfo();
		if (CurrentTarget == 0)
		{
			OrangeCommunityManager.m_RankingInfo = new List<SocketRankingInfo>(MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicFriendRankingList[(RankType)CurrentType]);
			RankingInfo = MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicPlayerFriendRank[(RankType)CurrentType];
		}
		else
		{
			OrangeCommunityManager.m_RankingInfo = new List<SocketRankingInfo>(MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicPersonalRankingList[(RankType)CurrentType]);
			RankingInfo = MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicPlayerRank[(RankType)CurrentType];
		}
		if (_scrollRect != null)
		{
			_scrollRect.ClearCells();
		}
		_scrollRect.OrangeInit(_playerRankingScrollCell, 5, OrangeCommunityManager.m_RankingInfo.Count);
		PlayerNameText.text = ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.Nickname;
		PlayerScoreText.text = string.Concat(RankingInfo.m_Score);
		string text = ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.Nickname;
		int num = ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.MainWeaponID;
		if (CurrentType == 3)
		{
			num = MonoBehaviourSingleton<OrangeCommunityManager>.Instance.OnGetBestWeaponID(false);
			text = ManagedSingleton<OrangeTextDataManager>.Instance.WEAPONTEXT_TABLE_DICT.GetL10nValue(ManagedSingleton<OrangeDataManager>.Instance.WEAPON_TABLE_DICT[num].w_NAME);
		}
		_playerRankingScrollCell.PlayerSetCellData(RankingInfo.m_Rank, MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify, text, RankingInfo.m_Score, ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.StandbyChara, num);
		MonoBehaviourSingleton<OrangeCommunityManager>.Instance.OnGetBestWeaponID(false);
		for (int i = 0; i < PlayerBadges.Length; i++)
		{
			PlayerBadges[i].enabled = false;
			PlayerBadgeBGs[i].enabled = false;
		}
		if (RankingInfo.m_Rank < 3)
		{
			PlayerBadges[RankingInfo.m_Rank].enabled = true;
			PlayerBadgeBGs[RankingInfo.m_Rank].enabled = true;
		}
		PlayerRankText.text = ((RankingInfo.m_Rank >= 999) ? "999+" : string.Concat(RankingInfo.m_Rank + 1));
		if (RankingInfo.m_Rank > 10 && CurrentTarget != 0)
		{
			_scrollRect.SrollToCell(10, 5000f, delegate
			{
				SetDefaultRankingScrollCellMessage(10);
			});
		}
		else
		{
			_scrollRect.SrollToCell(RankingInfo.m_Rank, 3000f, delegate
			{
				int defaultRankingScrollCellMessage = ((RankingInfo.m_Rank > 0) ? RankingInfo.m_Rank : 0);
				SetDefaultRankingScrollCellMessage(defaultRankingScrollCellMessage);
			});
		}
		_storageCanvas.blocksRaycasts = true;
		MonoBehaviourSingleton<UIManager>.Instance.Connecting(false);
	}

	public void OnRankGetList(int typ)
	{
		CurrentStart = 0;
		CurrentEnd = 49;
		CurrentType = typ;
		SelectBoardText(typ);
		if (!CurrentStartSelect)
		{
			SetRankingList();
		}
		else
		{
			SetPersonalRankingList();
		}
	}

	public void OnRankScrollGetDownList()
	{
	}

	public void OnRankScrollGetUpList()
	{
	}

	public void OnScrollBarChange()
	{
		if (!CurrentStartSelect)
		{
			float value = ScrollbarObj.value;
			if (value < 0.1f && CurrentStart > 0 && ScrollbarObj.size < 0.45f)
			{
				ScrollbarObj.value = 0.5f;
				OnRankScrollGetUpList();
			}
			else if (value > 0.9f)
			{
				ScrollbarObj.value = 0.9f;
				OnRankScrollGetDownList();
			}
		}
	}

	private void ChangeGetListFlag()
	{
		GetFlag = true;
	}

	public void OnScrollRectChange()
	{
	}

	public void CheckRankingListIndex(int idx)
	{
		if (!CurrentStartSelect)
		{
			if (isShowEventRanking)
			{
				GetFlag = false;
				OnRankScrollGetDownList();
				Invoke("ChangeGetListFlag", 1f);
			}
			else if (idx == CurrentEnd && CurrentEnd < CurrentTotalCount)
			{
				GetFlag = false;
				OnRankScrollGetDownList();
				Invoke("ChangeGetListFlag", 1f);
			}
		}
	}

	public void OnSelectTopInfo(bool selected)
	{
		if (isChangeModeRead)
		{
			isChangeModeRead = false;
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_CURSOR07);
			CurrentSelect = selected;
			WorldBtn.interactable = !CurrentSelect;
			FriendBtn.interactable = CurrentSelect;
			CurrentTarget = (CurrentSelect ? 1 : 0);
			CreateNewStorageTab(CurrentSelect);
			for (int i = 0; i < SelectTopInfoText.Length; i++)
			{
				SelectTopInfoText[i].color = colors[0];
			}
			SelectTopInfoText[CurrentTarget].color = colors[1];
		}
	}

	public void ClearChangeModeRead()
	{
		isChangeModeRead = true;
	}

	public void OnSelectCurrentInfo(bool selected)
	{
		if (isChangeModeRead)
		{
			isChangeModeRead = false;
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK01);
			CurrentStartSelect = selected;
			PersonalBtn.gameObject.SetActive(CurrentStartSelect);
			CurrentBtn.gameObject.SetActive(!CurrentStartSelect);
			if (isShowEventRanking)
			{
				OnGetEventRanking(CurrentSelectEventID);
			}
			else
			{
				OnRankGetList(CurrentType);
			}
		}
	}

	public override void OnClickCloseBtn()
	{
		if (null != textureObj)
		{
			if (!textureObj.IsCanDelete())
			{
				return;
			}
			Object.Destroy(textureObj.gameObject);
		}
		base.OnClickCloseBtn();
	}

	public void SetItemData(int rank, string name, int score)
	{
		TargetRank.text = ((rank >= 999) ? "999+" : string.Concat(rank + 1));
		TargetName.text = name;
		for (int i = 0; i < Badges.Length; i++)
		{
			Badges[i].enabled = false;
			if (rank == i)
			{
				Badges[i].enabled = true;
			}
		}
	}

	public int GetCurrectTouchIndex()
	{
		return CurrectTouchIndex;
	}

	public string GetCurrectTouchPlayerID()
	{
		return CurrectTouchPlayerID;
	}

	public void GetRankingScrollCellMessage(int idx)
	{
		CurrectTouchIndex = idx;
		if (CurrentType == 3)
		{
			DrawCharModel(OrangeCommunityManager.m_TargetRankingInfo.m_StandbyCharID, OrangeCommunityManager.m_TargetRankingInfo.m_BastWeaponID, OrangeCommunityManager.m_TargetRankingInfo.m_StandbyCharSkin);
		}
		else
		{
			DrawCharModel(OrangeCommunityManager.m_TargetRankingInfo.m_StandbyCharID, OrangeCommunityManager.m_TargetRankingInfo.m_MainWeaponID, OrangeCommunityManager.m_TargetRankingInfo.m_StandbyCharSkin);
		}
		CurrectTouchPlayerID = OrangeCommunityManager.m_TargetRankingInfo.m_PlayerId;
		SetItemData(OrangeCommunityManager.m_TargetRankingInfo.m_Rank, OrangeCommunityManager.m_TargetRankingInfo.m_Name, OrangeCommunityManager.m_TargetRankingInfo.m_Score);
	}

	public void SetDefaultRankingScrollCellMessage(int idx)
	{
		if (idx < 0 || idx >= OrangeCommunityManager.m_RankingInfo.Count())
		{
			Debug.LogWarning(string.Format("Index out of range {0}, Count = {1}", idx, OrangeCommunityManager.m_RankingInfo.Count()));
			return;
		}
		SocketRankingInfo socketRankingInfo = OrangeCommunityManager.m_RankingInfo[idx];
		if (CurrentType == 3)
		{
			DrawCharModel(socketRankingInfo.m_PlayerModelID, socketRankingInfo.m_BestWeaponModelID, socketRankingInfo.m_PlayerModelSkin);
		}
		else
		{
			DrawCharModel(socketRankingInfo.m_PlayerModelID, socketRankingInfo.m_MainWeaponModelID, socketRankingInfo.m_PlayerModelSkin);
		}
		CurrectTouchPlayerID = socketRankingInfo.m_PlayerId;
		SetItemData(socketRankingInfo.m_Rank, socketRankingInfo.m_PlayerName, socketRankingInfo.m_Score);
	}

	public void OnReRankingScrollCellMessage()
	{
		GetRankingScrollCellMessage(CurrectTouchIndex);
	}

	public void OnClickShowInfoBtn()
	{
		int currentSelectEventType = CurrentSelectEventType;
		if (currentSelectEventType == 15)
		{
			NetGuildInfo targetGuildInfo = ((CurrectTouchIndex >= 0) ? GuildRankingDataCache[CurrectTouchIndex].GuildInfo : Singleton<GuildSystem>.Instance.GuildInfoCache);
			if (targetGuildInfo != null)
			{
				MonoBehaviourSingleton<UIManager>.Instance.Connecting(true);
				PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
				MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_GuildMemberList", delegate(GuildMemberListUI ui)
				{
					ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
					ui.Setup(targetGuildInfo.GuildID);
				});
				return;
			}
		}
		if (CurrectTouchPlayerID != null)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_PlayerInfoMain", delegate(PlayerInfoMainUI ui)
			{
				ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
				ui.Setup(CurrectTouchPlayerID);
			});
		}
	}

	public List<EVENT_TABLE> GetSeasonEventTableRemain(long now)
	{
		return ManagedSingleton<ExtendDataHelper>.Instance.EVENT_TABLE_DICT.Values.Where((EVENT_TABLE x) => x.n_TYPE == 5 && ManagedSingleton<OrangeTableHelper>.Instance.IsOpeningDate(x.s_BEGIN_TIME, x.s_REMAIN_TIME, now)).ToList();
	}

	public List<EVENT_TABLE> GetEventTableRemain(long now)
	{
		List<EVENT_TABLE> list = ManagedSingleton<ExtendDataHelper>.Instance.EVENT_TABLE_DICT.Values.Where((EVENT_TABLE x) => x.n_RANKING > 0 && x.n_TYPE == 15 && ManagedSingleton<OrangeTableHelper>.Instance.IsOpeningDate(x.s_BEGIN_TIME, x.s_REMAIN_TIME, now)).ToList();
		list.AddRange(ManagedSingleton<ExtendDataHelper>.Instance.EVENT_TABLE_DICT.Values.Where((EVENT_TABLE x) => x.n_RANKING > 0 && x.n_TYPE != 15 && ManagedSingleton<OrangeTableHelper>.Instance.IsOpeningDate(x.s_BEGIN_TIME, x.s_REMAIN_TIME, now)));
		return list;
	}

	public void OnCreateRSGetPlayerHUDListCallback(object res)
	{
		int currentSelectEventType = CurrentSelectEventType;
		if (currentSelectEventType == 15 || !(res is RSGetPlayerHUDList))
		{
			return;
		}
		RSGetPlayerHUDList rSGetPlayerHUDList = (RSGetPlayerHUDList)res;
		CurrentTotalCount = rSGetPlayerHUDList.PlayerHUDLength;
		for (int i = 0; i < rSGetPlayerHUDList.PlayerHUDLength; i++)
		{
			ManagedSingleton<SocketHelper>.Instance.UpdateHUD(TempPlayerIDList[i], rSGetPlayerHUDList.PlayerHUD(i));
		}
		for (int j = 0; j < eventRankingInfoList.Count; j++)
		{
			SocketPlayerHUD value = null;
			if (!MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicHUD.TryGetValue(eventRankingInfoList[j].PlayerID, out value))
			{
				value = new SocketPlayerHUD();
				value.m_PlayerId = eventRankingInfoList[j].PlayerID;
				value.m_Name = eventRankingInfoList[j].PlayerID;
			}
			SocketRankingInfo socketRankingInfo = new SocketRankingInfo();
			socketRankingInfo.m_Rank = eventRankingInfoList[j].Ranking - 1;
			socketRankingInfo.m_PlayerId = value.m_PlayerId;
			socketRankingInfo.m_PlayerName = value.m_Name;
			socketRankingInfo.m_BestWeaponModelID = value.m_MainWeaponID;
			socketRankingInfo.m_PlayerModelID = value.m_StandbyCharID;
			socketRankingInfo.m_MainWeaponModelID = value.m_MainWeaponID;
			socketRankingInfo.m_PlayerModelSkin = value.m_StandbyCharSkin;
			socketRankingInfo.m_MainWeaponModelSkin = value.m_MainWeaponSkin;
			socketRankingInfo.m_Score = ConvertRankingScore(eventRankingInfoList[j].Score);
			socketRankingInfo.m_bConvertScoreToTime = CurrentSelectEventType == 11;
			OrangeCommunityManager.m_RankingInfo.Add(socketRankingInfo);
		}
		string currentPlayerID = MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.CurrentPlayerID;
		List<string> list = eventRankingInfoList.Select((EventRankingInfo rankingInfo) => rankingInfo.PlayerID).Distinct().ToList();
		if (!list.Contains(currentPlayerID))
		{
			list.Add(currentPlayerID);
		}
		Singleton<GuildSystem>.Instance.RefreshCommunityPlayerGuildInfoCache(list, InitializeRankingScrollCell);
	}

	private void InitializeRankingScrollCell()
	{
		if (_scrollRect == null)
		{
			return;
		}
		bool flag = false;
		if (CurrentStartSelect)
		{
			for (int i = 0; i < OrangeCommunityManager.m_RankingInfo.Count; i++)
			{
				if (OrangeCommunityManager.m_RankingInfo[i].m_PlayerId == MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify)
				{
					_scrollRect.OrangeInit(_playerRankingScrollCell, 5, OrangeCommunityManager.m_RankingInfo.Count, i);
					CurrectTouchIndex = i;
					SetDefaultRankingScrollCellMessage(i);
					flag = true;
					break;
				}
			}
		}
		if (!CurrentStartSelect || !flag)
		{
			_scrollRect.OrangeInit(_playerRankingScrollCell, 5, OrangeCommunityManager.m_RankingInfo.Count);
			if (CurrentStart == 0)
			{
				CurrectTouchIndex = 0;
				SetDefaultRankingScrollCellMessage(0);
			}
		}
		_storageCanvas.blocksRaycasts = true;
		MonoBehaviourSingleton<UIManager>.Instance.Connecting(false);
	}

	public int GetCurrentSelectEventID()
	{
		return CurrentSelectEventID;
	}

	public void OnGetEventRanking(int n_ID)
	{
		OrangeCommunityManager.m_RankingInfo.Clear();
		if (_scrollRect != null)
		{
			_scrollRect.ClearCells();
		}
		CurrentSelectEventID = n_ID;
		CurrentSelectEventType = ManagedSingleton<ExtendDataHelper>.Instance.EVENT_TABLE_DICT[n_ID].n_TYPE;
		CurrentType = 1;
		CurrentStart = 0;
		CurrentEnd = 99;
		if (CurrentSelectEventType == 6)
		{
			SelectBoardText(14);
			ManagedSingleton<PlayerNetManager>.Instance.GetPersonnelLaboEventRankingReq(n_ID, delegate(GetPersonnelLaboEventRankingRes res)
			{
				int ranking2 = res.EventRanking.Ranking;
				if (CurrentStartSelect && ranking2 > 0)
				{
					CurrentStart = ranking2 - 10;
					CurrentEnd = ranking2 + 10;
					if (CurrentStart < 0)
					{
						CurrentStart = 0;
						CurrentEnd = 10;
					}
				}
				ManagedSingleton<PlayerNetManager>.Instance.GetLaboEventRankingReq(n_ID, CurrentStart + 1, CurrentEnd + 1, delegate(GetLaboEventRankingRes rankingRes)
				{
					eventRankingInfoList = rankingRes.EventRankingList;
					if (eventRankingInfoList.Count > 0)
					{
						List<string> list2 = new List<string>();
						for (int k = 0; k < eventRankingInfoList.Count; k++)
						{
							list2.Add(eventRankingInfoList[k].PlayerID);
						}
						TempPlayerIDList = Enumerable.ToArray(list2);
						MonoBehaviourSingleton<CCSocketClient>.Instance.SendProtocol(FlatBufferCCHelper.CreateRQGetPlayerHUDList(TempPlayerIDList));
						MonoBehaviourSingleton<UIManager>.Instance.Connecting(true);
					}
					else
					{
						_storageCanvas.blocksRaycasts = true;
						MonoBehaviourSingleton<UIManager>.Instance.Connecting(false);
					}
					EventRankingInfo eventRanking = res.EventRanking;
					int num3 = eventRanking.Ranking - 1;
					PlayerNameText.text = ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.Nickname;
					PlayerScoreText.text = string.Concat(eventRanking.Score);
					PlayerRankText.text = ((num3 >= 999) ? "999+" : string.Concat(num3 + 1));
					string text2 = ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.Nickname;
					int num4 = ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.MainWeaponID;
					if (CurrentType == 3)
					{
						num4 = MonoBehaviourSingleton<OrangeCommunityManager>.Instance.OnGetBestWeaponID(false);
						text2 = ManagedSingleton<OrangeTextDataManager>.Instance.WEAPONTEXT_TABLE_DICT.GetL10nValue(ManagedSingleton<OrangeDataManager>.Instance.WEAPON_TABLE_DICT[num4].w_NAME);
					}
					MonoBehaviourSingleton<OrangeCommunityManager>.Instance.OnGetBestWeaponID(false);
					for (int l = 0; l < PlayerBadges.Length; l++)
					{
						PlayerBadges[l].enabled = false;
						PlayerBadgeBGs[l].enabled = false;
					}
					if (num3 < 3 && num3 >= 0)
					{
						PlayerBadges[num3].enabled = true;
						PlayerBadgeBGs[num3].enabled = true;
					}
					_playerRankingScrollCell.PlayerSetCellData(num3, MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify, text2, eventRanking.Score, ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.StandbyChara, num4);
				});
			});
			return;
		}
		bossRushStageTable = null;
		if (CurrentSelectEventType == 11)
		{
			SelectBoardText(14);
			bossRushStageTable = ManagedSingleton<OrangeDataManager>.Instance.STAGE_TABLE_DICT.Values.Where((STAGE_TABLE x) => x.n_MAIN == n_ID).FirstOrDefault();
		}
		else
		{
			SelectBoardText(13);
		}
		ManagedSingleton<PlayerNetManager>.Instance.RetrievePersonnelEventRankingReq(n_ID, delegate(EventRankingInfo res)
		{
			int ranking = res.Ranking;
			if (CurrentStartSelect && ranking > 0)
			{
				CurrentStart = ranking - 10;
				CurrentEnd = ranking + 10;
				if (CurrentStart < 0)
				{
					CurrentStart = 0;
					CurrentEnd = 10;
				}
			}
			ManagedSingleton<PlayerNetManager>.Instance.RetrieveEventRankingReq(n_ID, CurrentStart + 1, CurrentEnd + 1, delegate(List<EventRankingInfo> infoList)
			{
				eventRankingInfoList = infoList;
				if (eventRankingInfoList.Count > 0)
				{
					List<string> list = new List<string>();
					for (int j = 0; j < eventRankingInfoList.Count; j++)
					{
						list.Add(eventRankingInfoList[j].PlayerID);
					}
					TempPlayerIDList = Enumerable.ToArray(list);
					MonoBehaviourSingleton<CCSocketClient>.Instance.SendProtocol(FlatBufferCCHelper.CreateRQGetPlayerHUDList(TempPlayerIDList));
					MonoBehaviourSingleton<UIManager>.Instance.Connecting(true);
				}
				else
				{
					_storageCanvas.blocksRaycasts = true;
					MonoBehaviourSingleton<UIManager>.Instance.Connecting(false);
				}
			});
			int num = res.Ranking - 1;
			PlayerNameText.text = ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.Nickname;
			PlayerScoreText.text = string.Concat(res.Score);
			PlayerRankText.text = ((num >= 999) ? "999+" : string.Concat(num + 1));
			string text = ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.Nickname;
			int num2 = ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.MainWeaponID;
			if (CurrentType == 3)
			{
				num2 = MonoBehaviourSingleton<OrangeCommunityManager>.Instance.OnGetBestWeaponID(false);
				text = ManagedSingleton<OrangeTextDataManager>.Instance.WEAPONTEXT_TABLE_DICT.GetL10nValue(ManagedSingleton<OrangeDataManager>.Instance.WEAPON_TABLE_DICT[num2].w_NAME);
			}
			MonoBehaviourSingleton<OrangeCommunityManager>.Instance.OnGetBestWeaponID(false);
			for (int i = 0; i < PlayerBadges.Length; i++)
			{
				PlayerBadges[i].enabled = false;
				PlayerBadgeBGs[i].enabled = false;
			}
			if (num < 3 && num >= 0)
			{
				PlayerBadges[num].enabled = true;
				PlayerBadgeBGs[num].enabled = true;
			}
			_playerRankingScrollCell.PlayerSetCellData(num, MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify, text, ConvertRankingScore(res.Score), ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.StandbyChara, num2, CurrentSelectEventType == 11);
		});
	}

	private int ConvertRankingScore(int originalScore)
	{
		if (CurrentSelectEventType == 11 && bossRushStageTable != null)
		{
			return bossRushStageTable.n_TIME * 1000 - originalScore;
		}
		return originalScore;
	}

	private void CreateNewStorageTab(bool bShowAll, int EventID = 0)
	{
		listStorage.Clear();
		long serverUnixTimeNowUTC = MonoBehaviourSingleton<OrangeGameManager>.Instance.ServerUnixTimeNowUTC;
		int num = 0;
		int num2 = 0;
		int p_defaultSubIdx = 0;
		num2 = num;
		StorageInfo storageInfo = new StorageInfo("RANKING_PERSONAL", false, 4);
		storageInfo.Sub[0] = new StorageInfo("RANKING_PERSONAL_TOTALPOWER", false, 0, OnClickTab_111);
		storageInfo.Sub[1] = new StorageInfo("RANKING_PERSONAL_LEVEL", false, 0, OnClickTab_111);
		storageInfo.Sub[2] = new StorageInfo("RANKING_PERSONAL_WEAPON", false, 0, OnClickTab_111);
		storageInfo.Sub[3] = new StorageInfo("RANKING_PERSONAL_ACHIEVEMENT", false, 0, OnClickTab_111);
		storageInfo.Param = new object[1] { 0 };
		storageInfo.Sub[0].Param = new object[1] { 1 };
		storageInfo.Sub[1].Param = new object[1] { 2 };
		storageInfo.Sub[2].Param = new object[1] { 3 };
		storageInfo.Sub[3].Param = new object[1] { 4 };
		listStorage.Add(storageInfo);
		List<EVENT_TABLE> seasonEventTableRemain = GetSeasonEventTableRemain(serverUnixTimeNowUTC);
		int num3 = (bShowAll ? seasonEventTableRemain.Count : 0);
		if (bShowAll && num3 > 0)
		{
			int num4 = 0;
			num++;
			StorageInfo storageInfo2 = new StorageInfo("RANKING_PVP", false, num3);
			for (int i = 0; i < seasonEventTableRemain.Count; i++)
			{
				if (seasonEventTableRemain[i].n_TYPE == 5)
				{
					storageInfo2.Sub[num4] = new StorageInfo(seasonEventTableRemain[i].w_NAME, false, 0, OnClickTabEvent);
					storageInfo2.Sub[num4].Param = new object[2]
					{
						seasonEventTableRemain[i].n_ID,
						seasonEventTableRemain[i].n_TYPE
					};
					if (EventID == seasonEventTableRemain[i].n_ID)
					{
						num2 = num;
						p_defaultSubIdx = num4;
					}
				}
			}
			listStorage.Add(storageInfo2);
		}
		List<EVENT_TABLE> eventTableRemain = GetEventTableRemain(serverUnixTimeNowUTC);
		if (bShowAll && eventTableRemain.Count > 0)
		{
			num++;
			StorageInfo storageInfo3 = new StorageInfo("HOMETOP_EVENT", false, eventTableRemain.Count);
			storageInfo3.Param = new object[1] { 3 };
			for (int j = 0; j < eventTableRemain.Count; j++)
			{
				storageInfo3.Sub[j] = new StorageInfo(eventTableRemain[j].w_NAME, false, 0, OnClickTabEvent);
				storageInfo3.Sub[j].Param = new object[2]
				{
					eventTableRemain[j].n_ID,
					eventTableRemain[j].n_TYPE
				};
				if (EventID == eventTableRemain[j].n_ID)
				{
					num2 = num;
					p_defaultSubIdx = j;
				}
			}
			listStorage.Add(storageInfo3);
		}
		int childCount = storageRoot.transform.childCount;
		for (int k = 0; k < childCount; k++)
		{
			Object.Destroy(storageRoot.transform.GetChild(k).gameObject);
		}
		StorageGenerator.Load("StorageComp00", listStorage, num2, p_defaultSubIdx, storageRoot, delegate
		{
			Debug.Log("Load StorageComp00 Complete");
		});
	}

	public void OnClickTab_000(object p_param)
	{
		_storageCanvas.blocksRaycasts = false;
		int num = (int)((StorageInfo)p_param).Param[0];
	}

	public void OnClickTab_111(object p_param)
	{
		MonoBehaviourSingleton<UIManager>.Instance.Connecting(true);
		_storageCanvas.blocksRaycasts = false;
		ChangeRankingMode(true);
		CurrentSelectEventType = 0;
		isShowEventRanking = false;
		int typ = (int)((StorageInfo)p_param).Param[0];
		OnRankGetList(typ);
	}

	public void OnClickTab_222(object p_param)
	{
		MonoBehaviourSingleton<UIManager>.Instance.Connecting(true);
		_storageCanvas.blocksRaycasts = false;
		ChangeRankingMode(false);
		isShowEventRanking = false;
		int num = (int)((StorageInfo)p_param).Param[0];
	}

	public void OnClickTabEvent(object p_param)
	{
		MonoBehaviourSingleton<UIManager>.Instance.Connecting(true);
		_storageCanvas.blocksRaycasts = false;
		isShowEventRanking = true;
		StorageInfo obj = (StorageInfo)p_param;
		int n_ID = (int)obj.Param[0];
		if ((int)obj.Param[1] == 15)
		{
			OnClickTabCrusadeEvent(n_ID);
		}
		else
		{
			OnClickTabEvent(n_ID);
		}
	}

	private void OnClickTabEvent(int n_ID)
	{
		ChangeRankingMode(true);
		OnGetEventRanking(n_ID);
	}

	private void OnClickTabCrusadeEvent(int n_ID)
	{
		CurrentSelectEventID = n_ID;
		CurrentSelectEventType = ManagedSingleton<ExtendDataHelper>.Instance.EVENT_TABLE_DICT[n_ID].n_TYPE;
		SelectBoardText(15);
		ChangeRankingMode(false);
		_guildRankingScrollCell.Reset();
		if (_scrollRect != null)
		{
			_scrollRect.ClearCells();
		}
		Singleton<CrusadeSystem>.Instance.RetrieveEventRanking(n_ID, 1, 100);
	}

	private void OnRetrieveCrusadeRankingEvent(int eventId, List<CrusadeEventRankingInfo> rankingInfos)
	{
		GuildRankingDataCache.Clear();
		GuildRankingDataCache.AddRange(rankingInfos.Select(delegate(CrusadeEventRankingInfo rankingInfo)
		{
			NetGuildInfo guildInfo = rankingInfo.GuildInfo;
			return new GuildRankingData
			{
				Rank = rankingInfo.Ranking,
				GuildInfo = rankingInfo.GuildInfo,
				Score = rankingInfo.Score
			};
		}));
		if (_scrollRect != null)
		{
			_scrollRect.OrangeInit(_guildRankingScrollCell, 5, GuildRankingDataCache.Count);
		}
		_storageCanvas.blocksRaycasts = true;
		MonoBehaviourSingleton<UIManager>.Instance.Connecting(false);
		SetupGuildRankingBottomCell();
	}

	private void SetupGuildRankingBottomCell()
	{
		if (_guildRankingScrollCell == null)
		{
			return;
		}
		if (Singleton<GuildSystem>.Instance.HasGuild)
		{
			int num = GuildRankingDataCache.FindIndex((GuildRankingData rankingData) => rankingData.GuildInfo.GuildID == Singleton<GuildSystem>.Instance.GuildId);
			if (num >= 0)
			{
				GuildRankingData guildRankingData = GuildRankingDataCache[num];
				_guildRankingScrollCell.Setup(guildRankingData.GuildInfo, num, guildRankingData.Rank, guildRankingData.Score);
				_guildRankingScrollCell.SelectGuildRankingCell();
				return;
			}
			NetGuildInfo guildInfoCache = Singleton<GuildSystem>.Instance.GuildInfoCache;
			if (guildInfoCache != null)
			{
				_guildRankingScrollCell.Setup(guildInfoCache, num, -1, 0L);
				_guildRankingScrollCell.SelectGuildRankingCell();
			}
			else
			{
				Singleton<GuildSystem>.Instance.OnGetGuildInfoOnceEvent += SetupGuildRankingBottomCell;
				Singleton<GuildSystem>.Instance.ReqGetGuildInfo();
			}
		}
		else
		{
			_guildRankingScrollCell.Reset();
			_guildRankingScrollCell.SelectGuildRankingCell();
		}
	}

	public void SelectGuildRankingCell(int idx, bool isScollCell)
	{
		CurrectTouchIndex = idx;
		if (isScollCell)
		{
			MonoBehaviourSingleton<UIManager>.Instance.Block(true);
			GuildRankingData guildRankingData = GuildRankingDataCache[idx];
			OnSelectGuildRankingScrollCell(guildRankingData.GuildInfo);
			_guildRankingScrollCell.IsSelected = _guildRankingScrollCell.Index == idx;
		}
		else
		{
			if (_guildRankingScrollCell.GuildInfoCache != null)
			{
				MonoBehaviourSingleton<UIManager>.Instance.Block(true);
				OnSelectGuildRankingScrollCell(_guildRankingScrollCell.GuildInfoCache);
			}
			else
			{
				CurrectTouchPlayerID = MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.CurrentPlayerID;
				SocketPlayerHUD value;
				if (MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicHUD.TryGetValue(CurrectTouchPlayerID, out value))
				{
					MonoBehaviourSingleton<UIManager>.Instance.Block(true);
					DrawCharModel(value.m_StandbyCharID, value.m_MainWeaponID, value.m_StandbyCharSkin);
				}
			}
			_guildRankingScrollCell.IsSelected = true;
		}
		_scrollRect.RefreshCells();
	}

	private void OnSelectGuildRankingScrollCell(NetGuildInfo guildInfo)
	{
		string leaderPlayerID = guildInfo.LeaderPlayerID;
		SetCharModel(leaderPlayerID);
	}

	private void SetCharModel(string playerId)
	{
		if (playerId == string.Empty)
		{
			MonoBehaviourSingleton<OrangeGameManager>.Instance.ShowTipDialog(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("GUILD_HALL_WARN3"));
			MonoBehaviourSingleton<UIManager>.Instance.Block(false);
			if (textureObj != null)
			{
				Object.Destroy(textureObj.gameObject);
			}
			return;
		}
		Singleton<GuildSystem>.Instance.SearchHUD(playerId, delegate
		{
			SocketPlayerHUD value;
			if (MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicHUD.TryGetValue(playerId, out value))
			{
				DrawCharModel(value.m_StandbyCharID, value.m_MainWeaponID, value.m_StandbyCharSkin);
			}
			else
			{
				Debug.LogError("Unable to get PlayerHUD of PlayerId : " + playerId);
				MonoBehaviourSingleton<UIManager>.Instance.Block(false);
				if (textureObj != null)
				{
					Object.Destroy(textureObj.gameObject);
				}
			}
		});
	}

	public override void SetCanvas(bool enable)
	{
		base.SetCanvas(enable);
		if (textureObj != null)
		{
			textureObj.SetCameraActive(enable);
		}
	}

	public int OnGetCurrentType()
	{
		return CurrentType;
	}

	private void ChangeRankingMode(bool isPlayer)
	{
		ButtonGroup.SetActive(isPlayer);
		_playerRankingScrollCell.gameObject.SetActive(isPlayer);
		_guildRankingScrollCell.gameObject.SetActive(!isPlayer);
	}
}
