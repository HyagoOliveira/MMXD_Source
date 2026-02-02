#define RELEASE
using System;
using System.Collections.Generic;
using System.Linq;
using CallbackDefs;
using Coffee.UIExtensions;
using NaughtyAttributes;
using Steamworks;
using UnityEngine;
using UnityEngine.UI;
using enums;

public class HometopUI : OrangeUIBase
{
	public enum Board
	{
		MAIN = 0,
		PVE = 1,
		EQUIP = 2,
		PVP = 3,
		LAB = 4,
		CHAR = 5,
		GUILD = 6,
		COUNT = 7
	}

	[SerializeField]
	private HometopResidentUI hometopResidentUI;

	[SerializeField]
	private OrangeText powetText;

	[SerializeField]
	private Transform bannerRoot;

	[SerializeField]
	private Transform bannerDotRoot;

	[SerializeField]
	private GameObject eventRoot;

	[SerializeField]
	private EventCell eventCell;

	[SerializeField]
	private GameObject EventRoot2;

	[SerializeField]
	private EventCell eventCell2;

	[SerializeField]
	private RectTransform[] arrLeftWindowBg;

	[SerializeField]
	private RectTransform goLeftWindow;

	[SerializeField]
	private UIFlip leftArrow;

	private readonly float[] windowWidth = new float[2] { 79f, 201f };

	private readonly Vector2[] windowScale = new Vector2[2]
	{
		new Vector2(0f, 1f),
		new Vector2(1f, 1f)
	};

	private bool isWindowsOpen = true;

	[SerializeField]
	private ModelRotateDrag objDrag;

	[SerializeField]
	public GameObject objChat;

	private HometopSceneController hometopSceneController;

	[BoxGroup("Sound")]
	[SerializeField]
	private SystemSE m_clickBoard;

	[BoxGroup("Sound")]
	[SerializeField]
	private SystemSE m_clickSub;

	[BoxGroup("Sound")]
	[SerializeField]
	private SystemSE m_clickLeft;

	[BoxGroup("Sound")]
	[SerializeField]
	private SystemSE m_addJewelSE;

	private Dictionary<int, List<EVENT_TABLE>> events = new Dictionary<int, List<EVENT_TABLE>>();

	private Dictionary<int, List<EVENT_TABLE>> event2s = new Dictionary<int, List<EVENT_TABLE>>();

	[SerializeField]
	private Image imgBannerDot;

	[SerializeField]
	private Sprite[] sprDot;

	private List<Image> listBannerDot = new List<Image>();

	[SerializeField]
	private Button addMoneyBtn;

	[SerializeField]
	private Transform addMoneySeparator;

	[SerializeField]
	private BanedUI banedUI;

	private bool alreadySetup;

	private bool clickEventWeb;

	private long lastMailChkTime;

	private long mailChkRate = 3000000000L;

	private OrangeBannerComponent bannerComponent;

	private List<BANNER_TABLE> listBanner;

	[SerializeField]
	private Canvas[] arrayBoard = new Canvas[7];

	private Board nowBoard;

	[HideInInspector]
	public CanvasGroup OwnCanvasGroup { get; private set; }

	public int EventCount
	{
		get
		{
			return eventRoot.transform.childCount;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		OwnCanvasGroup = GetComponent<CanvasGroup>();
		alreadySetup = false;
		UpdateWindowSize();
		if (addMoneyBtn != null)
		{
			addMoneyBtn.gameObject.SetActive(false);
		}
		if (addMoneySeparator != null)
		{
			addMoneySeparator.gameObject.SetActive(true);
		}
		StartCoroutine(ManagedSingleton<CharacterHelper>.Instance.CheckCharacterUpgrades());
	}

	private void OnEnable()
	{
		Singleton<GenericEventManager>.Instance.AttachEvent<bool>(EventManager.ID.UPDATE_HOMETOP_CANVAS, SetCanvas);
		Singleton<GenericEventManager>.Instance.AttachEvent<int>(EventManager.ID.UPDATE_BATTLE_POWER, OnUpdateBattlePower);
		Singleton<GenericEventManager>.Instance.AttachEvent(EventManager.ID.UPDATE_BANNER, RefreashBanner);
	}

	private void OnDisable()
	{
		Singleton<GenericEventManager>.Instance.DetachEvent<bool>(EventManager.ID.UPDATE_HOMETOP_CANVAS, SetCanvas);
		Singleton<GenericEventManager>.Instance.DetachEvent<int>(EventManager.ID.UPDATE_BATTLE_POWER, OnUpdateBattlePower);
		Singleton<GenericEventManager>.Instance.DetachEvent(EventManager.ID.UPDATE_BANNER, RefreashBanner);
	}

	public void Setup(HometopSceneController p_hometopSceneController, bool dayChange = false)
	{
		if (alreadySetup)
		{
			return;
		}
		alreadySetup = true;
		base._EscapeEvent = EscapeEvent.CUSTOM;
		hometopSceneController = p_hometopSceneController;
		if ((bool)hometopSceneController && (bool)objDrag)
		{
			objDrag.SetModelTransform(hometopSceneController.GetCharacterPos());
		}
		hometopResidentUI.UpdateValue();
		OnUpdateBattlePower(ManagedSingleton<PlayerHelper>.Instance.GetBattlePower());
		LoadBanner();
		CheckEvent();
		objChat.SetActive(MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.Setting.Community);
		MonoBehaviourSingleton<OrangeGameManager>.Instance.IsGamePause = false;
		if (dayChange)
		{
			Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.SD_HOME_BGM);
			return;
		}
		StageHelper.STAGE_END_GO nStageEndGoUI = ManagedSingleton<StageHelper>.Instance.nStageEndGoUI;
		if ((uint)(nStageEndGoUI - 4) > 1u && (uint)(nStageEndGoUI - 11) > 2u && nStageEndGoUI != StageHelper.STAGE_END_GO.TOTALWAR)
		{
			Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.SD_HOME_BGM);
		}
	}

	public void CheckEvent()
	{
		long now = MonoBehaviourSingleton<OrangeGameManager>.Instance.ServerUnixTimeNowUTC;
		(from x in ManagedSingleton<ExtendDataHelper>.Instance.EVENT_TABLE_DICT.Values
			where x.n_HOMETOP == 1
			select x into p
			where ManagedSingleton<OrangeTableHelper>.Instance.IsOpeningDate(p.s_BEGIN_TIME, (p.s_REMAIN_TIME == "null") ? p.s_END_TIME : p.s_REMAIN_TIME, now)
			select p).ToList().ForEach(delegate(EVENT_TABLE tbl)
		{
			if (events.ContainsKey(tbl.n_TYPE))
			{
				events[tbl.n_TYPE].Add(tbl);
			}
			else
			{
				events.Add(tbl.n_TYPE, new List<EVENT_TABLE> { tbl });
			}
		});
		if (events.Keys.Count != 0)
		{
			eventRoot.SetActive(true);
			CreateEventCellToGO(eventCell, events, eventRoot);
		}
		else
		{
			eventRoot.transform.DetachChildren();
		}
		List<MISSION_TABLE> list = ManagedSingleton<OrangeDataManager>.Instance.MISSION_TABLE_DICT.Values.Where((MISSION_TABLE x) => x.s_CREATE_TIME != "null" && x.n_SUB_TYPE == 15 && (!ManagedSingleton<PlayerNetManager>.Instance.dicMission.ContainsKey(x.n_ID) || ManagedSingleton<PlayerNetManager>.Instance.dicMission[x.n_ID].netMissionInfo.Received == 0)).ToList();
		bool flag = false;
		if (list.Count > 0 && CapUtility.DateToUnixTime(ManagedSingleton<OrangeTableHelper>.Instance.ParseDate(list[0].s_CREATE_TIME)) < ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.PlayerCreateTime)
		{
			flag = true;
		}
		ManagedSingleton<ExtendDataHelper>.Instance.EVENT_TABLE_DICT.Values.Where((EVENT_TABLE x) => x.n_TYPE == 9 && x.n_TYPE_X == 15 && ManagedSingleton<OrangeTableHelper>.Instance.IsOpeningDate(x.s_BEGIN_TIME, (x.s_REMAIN_TIME == "null") ? x.s_END_TIME : x.s_REMAIN_TIME, now)).ToList().ForEach(delegate(EVENT_TABLE tbl)
		{
			if (event2s.ContainsKey(tbl.n_TYPE))
			{
				event2s[tbl.n_TYPE].Add(tbl);
			}
			else
			{
				event2s.Add(tbl.n_TYPE, new List<EVENT_TABLE> { tbl });
			}
		});
		if (flag && event2s.Keys.Count != 0)
		{
			EventRoot2.SetActive(true);
			CreateEventCellToGO(eventCell2, event2s, EventRoot2);
		}
		else
		{
			EventRoot2.transform.DetachChildren();
		}
	}

	private void CreateEventCellToGO(EventCell tObj, Dictionary<int, List<EVENT_TABLE>> tEvents, GameObject tRoot)
	{
		Dictionary<int, List<EVENT_TABLE>>.Enumerator enumerator = tEvents.GetEnumerator();
		while (enumerator.MoveNext())
		{
			List<EVENT_TABLE> eventTables = enumerator.Current.Value;
			EVENT_TABLE eVENT_TABLE = eventTables[0];
			enums.EventType eventType = (enums.EventType)eVENT_TABLE.n_TYPE;
			switch (eventType)
			{
			case enums.EventType.EVENT_CRUSADE:
				if (!Singleton<GuildSystem>.Instance.HasGuild)
				{
					continue;
				}
				break;
			case enums.EventType.EVENT_TOTALWAR:
				if (OrangeConst.TOTALWAR_LV_LIMIT > ManagedSingleton<PlayerHelper>.Instance.GetLV())
				{
					continue;
				}
				break;
			}
			GameObject gameObject = UnityEngine.Object.Instantiate(tObj.transform.gameObject, tRoot.transform, false);
			gameObject.SetActive(true);
			EventCell cell = gameObject.GetComponent<EventCell>();
			OrangeTableHelper.EventsDef eDef = ManagedSingleton<OrangeTableHelper>.Instance.GetEventsDef(eventType);
			cell.SetType(eDef);
			string bundleName = "";
			switch (eventType)
			{
			case enums.EventType.EVENT_DROP:
				bundleName = AssetBundleScriptableObject.Instance.GetIconItem(eVENT_TABLE.s_IMG);
				break;
			case enums.EventType.EVENT_LABO:
			case enums.EventType.EVENT_MISSION:
				bundleName = AssetBundleScriptableObject.Instance.m_texture_ui_hometop;
				break;
			case enums.EventType.EVENT_RAID_BOSS:
				if (ManagedSingleton<PlayerHelper>.Instance.GetUseCheatPlugIn())
				{
					cell.btnIcon.interactable = false;
					OrangeGameUtility.CreateLockObj(cell.transform, UIOpenChk.ChkBanEnum.OPENBAN_RAIDBOSS);
					banedUI.BtnRaid = cell;
				}
				bundleName = AssetBundleScriptableObject.Instance.m_texture_ui_hometop;
				break;
			case enums.EventType.EVENT_CRUSADE:
				bundleName = AssetBundleScriptableObject.Instance.m_texture_ui_hometop;
				break;
			case enums.EventType.EVENT_TOTALWAR:
				bundleName = AssetBundleScriptableObject.Instance.m_texture_ui_hometop;
				break;
			}
			string assetName = eVENT_TABLE.s_IMG;
			if (eDef.sIconStr != "")
			{
				assetName = eDef.sIconStr;
			}
			MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(bundleName, assetName, delegate(Sprite s)
			{
				if (s != null)
				{
					cell.imgIcon.sprite = s;
					if (eDef.bResetSize)
					{
						cell.imgIcon.SetNativeSize();
					}
				}
			});
			if (eDef.sDBEffect != "")
			{
				MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad("dragonbones/" + eDef.sDBEffect, eDef.sDBEffect, delegate(GameObject db)
				{
					if (db != null)
					{
						GameObject obj = UnityEngine.Object.Instantiate(db, cell.transform);
						obj.transform.SetSiblingIndex(eDef.nSibIndex);
						obj.transform.position = cell.imgIcon.transform.position;
					}
					else
					{
						Debug.LogError("null");
					}
				});
			}
			if (!string.IsNullOrEmpty(eVENT_TABLE.w_NAME))
			{
				cell.txtEventName.UpdateText(eVENT_TABLE.w_NAME);
				if (eDef.nFontSize != 0)
				{
					cell.txtEventName.fontSize = eDef.nFontSize;
				}
			}
			else
			{
				cell.WordBG = false;
			}
			cell.btnIcon.onClick.AddListener(delegate
			{
				eDef.m_action(eventTables);
			});
		}
	}

	public void OnClickAddStamina()
	{
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_ChargeStamina", delegate(ChargeStaminaUI ui)
		{
			ui.Setup(ChargeType.ActionPoint);
		});
	}

	public void OnClickAddMoney()
	{
	}

	public void OnClickAddJewel()
	{
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_ShopTop", delegate(ShopTopUI shopUI)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(m_addJewelSE);
			shopUI.Setup(ShopTopUI.ShopSelectTab.directproduct);
		});
	}

	public void OnClickMenuCharacterBtn()
	{
		OnClickSystemSE(m_clickBoard);
		Debug.Log("OnClickCharacterBtn");
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_CharacterInfo_Select", delegate(CharacterInfoSelect ui)
		{
			ui.closeCB = (Callback)Delegate.Combine(ui.closeCB, new Callback(OnUpdateHometopData));
			ui.Setup();
			OnClickBoard_To_Main();
		});
	}

	public void OnClickMenuCardBtn()
	{
		OnClickSystemSE(m_clickSub);
		Debug.Log("OnClickCardBtn");
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_CardMain", delegate(CardMainUI ui)
		{
			ui.closeCB = (Callback)Delegate.Combine(ui.closeCB, new Callback(OnUpdateHometopData));
			OnClickBoard_To_Main();
		});
	}

	public void OnClickGuildBtn()
	{
		if (MonoBehaviourSingleton<OrangeCommunityManager>.Instance.OnCheckCommunityServerConnected())
		{
			OwnCanvasGroup.blocksRaycasts = false;
			Singleton<GuildSystem>.Instance.OnGetCheckGuildStateOnceEvent += OnGetCheckGuildStateEvent;
			Singleton<GuildSystem>.Instance.ReqCheckGuildState();
		}
	}

	private void OnGetCheckGuildStateEvent()
	{
		if (Singleton<GuildSystem>.Instance.HasGuild)
		{
			PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_OK04);
			Singleton<GuildSystem>.Instance.OnGetGuildInfoOnceEvent += OnGetGuildInfoEvent;
			Singleton<GuildSystem>.Instance.ReqGetGuildInfo(true);
		}
		else
		{
			OnClickBoard_To_Guild();
			OwnCanvasGroup.blocksRaycasts = true;
		}
	}

	private void OnGetGuildInfoEvent()
	{
		OwnCanvasGroup.blocksRaycasts = true;
	}

	public void OnClickShopBtn()
	{
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK16);
		Debug.Log("OnClickShopBtn");
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_ShopTop", delegate(ShopTopUI ui)
		{
			ui.closeCB = (Callback)Delegate.Combine(ui.closeCB, new Callback(OnUpdateHometopData));
			ui.Setup(ShopTopUI.ShopSelectTab.item_shop);
		});
	}

	public void OnClickGharaBtn()
	{
		OnClickSystemSE(m_clickBoard);
		MonoBehaviourSingleton<UIManager>.Instance.OpenLoadingUI(delegate
		{
			Debug.Log("OnClickGharaBtn");
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_Gacha", delegate(GachaUI ui)
			{
				ui.closeCB = (Callback)Delegate.Combine(ui.closeCB, new Callback(OnUpdateHometopData));
				ui.Setup();
			});
		});
	}

	public void OnClickQuestBtn()
	{
		OnClickSystemSE(m_clickBoard);
		Debug.Log("OnClickQuestBtn");
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_Mission", delegate(MissionUI ui)
		{
			ui.closeCB = (Callback)Delegate.Combine(ui.closeCB, new Callback(OnUpdateHometopData));
			ui.Setup();
		});
	}

	public void OnClickPowerBtn()
	{
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK16);
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_Guide", delegate(GuideUI ui)
		{
			ui.closeCB = (Callback)Delegate.Combine(ui.closeCB, new Callback(OnUpdateHometopData));
			ui.Setup();
		});
	}

	public void OnClickMailBtn()
	{
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK16);
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_MailSelect", delegate(MailSelectUI ui)
		{
			ui.closeCB = (Callback)Delegate.Combine(ui.closeCB, new Callback(OnUpdateHometopData));
			ui.Setup();
		});
	}

	public void OnClickNoticeBtn()
	{
		string platformLan = MonoBehaviourSingleton<LocalizationManager>.Instance.GetPlatformLan();
		string url = string.Format(ManagedSingleton<ServerConfig>.Instance.ServerSetting.Platform.Notice, platformLan);
		CtcWebView webView = null;
		CtcWebView.Create<CtcWebView>(out webView, url, null, false, "", null, null, null, null, m_clickLeft);
	}

	public void OnOpenNoticePopup()
	{
		string platformLan = MonoBehaviourSingleton<LocalizationManager>.Instance.GetPlatformLan();
		string url = string.Format(ManagedSingleton<ServerConfig>.Instance.ServerSetting.Platform.Popup, platformLan);
		CtcWebViewNotice webView = null;
		CtcWebView.Create<CtcWebViewNotice>(out webView, url, null, false, "", null, null, null, null, SystemSE.NONE, "UI_WebView_Notice");
	}

	public void OnClickEventWebBtn()
	{
		if (!clickEventWeb)
		{
			clickEventWeb = true;
			MonoBehaviourSingleton<OrangeGameManager>.Instance.RetrieveResetTime(delegate
			{
				string eventUrl = ManagedSingleton<ServerConfig>.Instance.GetEventUrl();
				CtcWebView webView = null;
				CtcWebView.Create<CtcWebView>(out webView, eventUrl, null, false, "", null, null, null, null, m_clickLeft);
				SteamFriends.OnGameOverlayActivated += OnEventWebCallback;
				clickEventWeb = false;
			});
		}
	}

	private void OnEventWebCallback(bool bActivated)
	{
		Debug.Log("Overlay activated = " + bActivated);
		if (bActivated)
		{
			return;
		}
		SteamFriends.OnGameOverlayActivated -= OnEventWebCallback;
		ManagedSingleton<PlayerNetManager>.Instance.RetrieveOperationDeliveryItemReq(delegate(List<NetRewardInfo> obj)
		{
			if (obj != null)
			{
				List<NetRewardInfo> rewardList = obj;
				if (rewardList != null && rewardList.Count > 0)
				{
					MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_RewardPopup", delegate(RewardPopopUI ui)
					{
						ui.Setup(rewardList);
					});
				}
			}
		});
	}

	public void OnClickIllustrationBtn()
	{
		OnClickSystemSE(m_clickLeft);
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_Illustration", delegate(IllustrationUI ui)
		{
			Debug.Log("OnClickGalleryhBtn");
			ui.closeCB = (Callback)Delegate.Combine(ui.closeCB, new Callback(OnUpdateHometopData));
			ui.Setup();
		});
	}

	public void OnClickPrizeBtn()
	{
		OnClickSystemSE(m_clickLeft);
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_PrizeTop", delegate(PrizeTopUI ui)
		{
			Debug.Log("OnClickPrizeBtn");
			ui.closeCB = (Callback)Delegate.Combine(ui.closeCB, new Callback(OnUpdateHometopData));
			ui.Setup();
		});
	}

	public void OnClickRankingBtn()
	{
		if (!MonoBehaviourSingleton<OrangeCommunityManager>.Instance.OnCheckCommunityServerConnected())
		{
			return;
		}
		if (!MonoBehaviourSingleton<OrangeCommunityManager>.Instance.RankingUIFlag)
		{
			OnClickSystemSE(SystemSE.CRI_SYSTEMSE_SYS_ERROR);
			return;
		}
		OnClickSystemSE(m_clickLeft);
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_RankingMain", delegate(RankingMainUI ui)
		{
			ui.closeCB = (Callback)Delegate.Combine(ui.closeCB, new Callback(OnUpdateHometopData));
			ui.Setup();
		});
	}

	public void OnClickFriendBtn()
	{
		Debug.Log("OnClickFriendBtn");
		OnClickSystemSE(m_clickLeft);
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_FriendMain", delegate(FriendMainUI ui)
		{
			ui.closeCB = (Callback)Delegate.Combine(ui.closeCB, new Callback(OnUpdateHometopData));
			ui.Setup();
		});
	}

	public void OnClickPlayerInfoBtn()
	{
		Debug.Log("OnClickPlayerInfoBtn");
		OnClickSystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_PlayerInfoMain", delegate(PlayerInfoMainUI ui)
		{
			ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
			ui.closeCB = (Callback)Delegate.Combine(ui.closeCB, new Callback(OnUpdateHometopData));
			ui.Setup(MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify);
		});
	}

	public void OnClickChannelBtn()
	{
		Debug.Log("OnClickChannelBtn");
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_Channel", delegate(ChannelUI ui)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
			ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
			ui.closeCB = (Callback)Delegate.Combine(ui.closeCB, new Callback(OnUpdateHometopData));
			ui.Setup();
		});
	}

	public void OnClickMinigameBtn()
	{
	}

	public void OnUpdateBattlePower(int val)
	{
		powetText.text = val.ToString();
	}

	public void OnUpdateHometopData()
	{
		if (MonoBehaviourSingleton<GameServerService>.Instance.DayChange)
		{
			MonoBehaviourSingleton<UIManager>.Instance.BackToHometop();
			return;
		}
		if (!ManagedSingleton<MailHelper>.Instance.DisplayHint && DateTime.UtcNow.Ticks - lastMailChkTime >= mailChkRate)
		{
			lastMailChkTime = DateTime.UtcNow.Ticks;
			ManagedSingleton<PlayerNetManager>.Instance.RetrieveNewMailCountReq(delegate
			{
				Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.UPDATE_HOMETOP_HINT, IconHintChk.HINT_TYPE.MAIL);
			});
		}
		Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.UPDATE_HOMETOP_HINT, IconHintChk.HINT_TYPE.RESEARCH);
		Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.UPDATE_HOMETOP_HINT, IconHintChk.HINT_TYPE.FRIEND);
		Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.UPDATE_HOMETOP_HINT, IconHintChk.HINT_TYPE.GUIDE);
		Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.UPDATE_HOMETOP_HINT, IconHintChk.HINT_TYPE.GALLERY);
		MonoBehaviourSingleton<OrangeGameManager>.Instance.DisplayPowerupPerform();
		Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.UPDATE_HOMETOP_HINT, IconHintChk.HINT_TYPE.CHARACTER);
		Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.UPDATE_HOMETOP_HINT, IconHintChk.HINT_TYPE.EQUIP);
		Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.UPDATE_HOMETOP_HINT, IconHintChk.HINT_TYPE.WEAPON);
		Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.UPDATE_HOMETOP_HINT, IconHintChk.HINT_TYPE.CHIP);
		Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.UPDATE_HOMETOP_HINT, IconHintChk.HINT_TYPE.FSSKILL);
		Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.UPDATE_HOMETOP_HINT, IconHintChk.HINT_TYPE.RESEARCHALL);
		objChat.SetActive(MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.Setting.Community);
	}

	public void LoadBanner()
	{
		listBanner = ManagedSingleton<ExtendDataHelper>.Instance.GetBannerListByOpening();
		if (listBanner != null && listBanner.Count != 0)
		{
			for (int i = 0; i < listBanner.Count; i++)
			{
				Image image = UnityEngine.Object.Instantiate(imgBannerDot, base.transform);
				image.transform.SetParent(bannerDotRoot, false);
				listBannerDot.Add(image);
			}
			MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad("UI/BannerComp", "BannerComp", delegate(GameObject obj)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(obj, base.transform);
				bannerComponent = gameObject.GetComponent<OrangeBannerComponent>();
				bannerComponent.BannerOffsetCB = UpdateBannerDot;
				bannerComponent.Setup(bannerRoot, listBanner, new Vector2Int(512, 128), new Vector2(20f, 0f));
			});
		}
	}

	private void RefreashBanner()
	{
		if ((bool)bannerComponent)
		{
			bannerComponent.BannerOffsetCB = null;
			UnityEngine.Object.Destroy(bannerComponent.gameObject);
			bannerComponent = null;
		}
		for (int i = 0; i < listBannerDot.Count; i++)
		{
			UnityEngine.Object.Destroy(listBannerDot[i].gameObject);
		}
		listBannerDot.Clear();
		LoadBanner();
	}

	private void UpdateBannerDot(int idx)
	{
		for (int i = 0; i < listBannerDot.Count; i++)
		{
			listBannerDot[i].sprite = ((idx == i) ? sprDot[1] : sprDot[0]);
		}
	}

	public void OnClickSettingBtn()
	{
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK16);
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_Setting", delegate(SettingUI ui)
		{
			ui.closeCB = (Callback)Delegate.Combine(ui.closeCB, new Callback(OnUpdateHometopData));
			ui.Setup();
		});
	}

	public void OnClickBtnLeft()
	{
		isWindowsOpen = !isWindowsOpen;
		if (isWindowsOpen)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP01);
		}
		else
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL01);
		}
		UpdateWindowSize();
	}

	private void UpdateWindowSize()
	{
		goLeftWindow.localScale = (isWindowsOpen ? windowScale[1] : windowScale[0]);
		leftArrow.horizontal = !isWindowsOpen;
		leftArrow.enabled = false;
		leftArrow.enabled = true;
		float x = (isWindowsOpen ? windowWidth[1] : windowWidth[0]);
		RectTransform[] array = arrLeftWindowBg;
		foreach (RectTransform rectTransform in array)
		{
			rectTransform.sizeDelta = new Vector2(x, rectTransform.sizeDelta.y);
		}
	}

	public void OnClick_PVE_BtnStory()
	{
		OnClickSystemSE(m_clickSub);
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_StoryStageSelect", delegate(StoryStageSelectUI ui)
		{
			ui.closeCB = (Callback)Delegate.Combine(ui.closeCB, new Callback(OnUpdateHometopData));
			ui.Setup();
			OnClickBoard_To_Main();
		});
	}

	public void OnClick_PVE_BtnEvent()
	{
		OnClickSystemSE(m_clickSub);
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_EventStage", delegate(EventStageMain ui)
		{
			ui.closeCB = (Callback)Delegate.Combine(ui.closeCB, new Callback(OnUpdateHometopData));
			ui.Setup();
			OnClickBoard_To_Main();
		});
	}

	public void OnClick_PVE_BtnMultiplay()
	{
		OnClickSystemSE(m_clickSub);
		if (!MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.CheckCardCountMax() && !ManagedSingleton<EquipHelper>.Instance.ShowEquipmentLimitReachedDialog())
		{
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_CoopStageSelectUI", delegate(CoopStageSelectUI ui)
			{
				ui.closeCB = (Callback)Delegate.Combine(ui.closeCB, new Callback(OnUpdateHometopData));
				ui.Setup();
				OnClickBoard_To_Main();
			});
		}
	}

	public void OnClick_PVE_BtnChallenge()
	{
		OnClickSystemSE(m_clickSub);
		MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.bTowerBase = false;
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_BossChallenge", delegate(UI_Challenge ui)
		{
			ui.closeCB = (Callback)Delegate.Combine(ui.closeCB, new Callback(OnUpdateHometopData));
			OnClickBoard_To_Main();
		});
	}

	public void OnClick_PVP_BtnPvpMain()
	{
		OnClickSystemSE(m_clickSub);
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_PvpRoomSelect", delegate(PvpRoomSelectUI ui)
		{
			ui.closeCB = (Callback)Delegate.Combine(ui.closeCB, new Callback(OnUpdateHometopData));
			ui.Setup();
			OnClickBoard_To_Main();
		});
	}

	public void OnClick_PVP_BtnPvpRank()
	{
		OnClickSystemSE(m_clickSub);
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_Qualifing", delegate(QualifingUI ui)
		{
			ui.closeCB = (Callback)Delegate.Combine(ui.closeCB, new Callback(OnUpdateHometopData));
			ui.Setup();
			OnClickBoard_To_Main();
		});
	}

	public void OnClick_LAB_BtnItem()
	{
		OnClickSystemSE(m_clickSub);
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_Research", delegate(ResearchUI ui)
		{
			ui.closeCB = (Callback)Delegate.Combine(ui.closeCB, new Callback(OnUpdateHometopData));
			ui.Setup();
			OnClickBoard_To_Main();
		});
	}

	public void OnClick_LAB_BtnSkill()
	{
		OnClickSystemSE(m_clickSub);
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_FinalStrikeMain", delegate(FinalStrikeMain ui)
		{
			ui.closeCB = (Callback)Delegate.Combine(ui.closeCB, new Callback(OnUpdateHometopData));
			ui.Setup();
			OnClickBoard_To_Main();
		});
	}

	public void OnClick_LAB_BtnBackup()
	{
		OnClickSystemSE(m_clickSub);
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_BackupSystem", delegate(BackupSystemUI ui)
		{
			ui.closeCB = (Callback)Delegate.Combine(ui.closeCB, new Callback(OnUpdateHometopData));
			ui.Setup();
			OnClickBoard_To_Main();
		});
	}

	public void OnClick_Equip_BtnWeaponUI()
	{
		OnClickSystemSE(m_clickSub);
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_WEAPONMAIN", delegate(WeaponMainUI ui)
		{
			ui.closeCB = (Callback)Delegate.Combine(ui.closeCB, new Callback(OnUpdateHometopData));
			OnClickBoard_To_Main();
		});
	}

	public void OnClick_Equip_BtnArmorUI()
	{
		OnClickSystemSE(m_clickSub);
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_ItemBox", delegate(ItemBoxUI ui)
		{
			ui.closeCB = (Callback)Delegate.Combine(ui.closeCB, new Callback(OnUpdateHometopData));
			ui.Setup(ItemType.Currency);
			OnClickBoard_To_Main();
		});
	}

	public void OnClick_Equip_BtnChipUI()
	{
		OnClickSystemSE(m_clickSub);
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_CHIPMAIN", delegate(ChipMainUI ui)
		{
			ui.closeCB = (Callback)Delegate.Combine(ui.closeCB, new Callback(OnUpdateHometopData));
			OnClickBoard_To_Main();
		});
	}

	public void OnClickClose_SubBoard()
	{
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_BACK01);
		OnClickBoard_To_Main();
	}

	public void OnClickBoard_To_Main()
	{
		UpdateBoard(Board.MAIN);
	}

	public void OnClickBoard_To_Pve()
	{
		OnClickSystemSE(m_clickBoard);
		UpdateBoard(Board.PVE);
	}

	public void OnClickBoard_To_Equip()
	{
		OnClickSystemSE(m_clickBoard);
		UpdateBoard(Board.EQUIP);
	}

	public void OnClickBoard_To_Pvp()
	{
		OnClickSystemSE(m_clickBoard);
		UpdateBoard(Board.PVP);
	}

	public void OnClickBoard_To_Lab()
	{
		OnClickSystemSE(m_clickBoard);
		UpdateBoard(Board.LAB);
	}

	public void OnClickBoard_To_Character()
	{
		OnClickSystemSE(m_clickBoard);
		UpdateBoard(Board.CHAR);
	}

	public void OnClickBoard_To_Guild()
	{
		OnClickSystemSE(m_clickBoard);
		UpdateBoard(Board.GUILD);
	}

	protected override void DoCustomEscapeEvent()
	{
		if (nowBoard == Board.MAIN)
		{
			if (isWindowsOpen)
			{
				OnClickBtnLeft();
				return;
			}
			MonoBehaviourSingleton<OrangeGameManager>.Instance.ShowCommonMsg(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("RETURN_LOGIN_CONFIRM"), delegate
			{
				MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_HOME);
				MonoBehaviourSingleton<UIManager>.Instance.CloseAllUI(delegate
				{
					MonoBehaviourSingleton<OrangeSceneManager>.Instance.ChangeScene("switch", OrangeSceneManager.LoadingType.DEFAULT, null, false);
				});
			}, null);
		}
		else
		{
			UpdateBoard(Board.MAIN);
		}
	}

	private void UpdateBoard(Board nextBoard)
	{
		if (nowBoard != nextBoard)
		{
			for (int i = 0; i < 7; i++)
			{
				arrayBoard[i].enabled = nextBoard == (Board)i;
			}
			nowBoard = nextBoard;
		}
	}

	public void OnClickSystemSE(SystemSE cuid)
	{
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(cuid);
	}

	public override void SetCanvas(bool enable)
	{
		base.SetCanvas(enable);
		if (bannerComponent != null)
		{
			bannerComponent.Pause = !enable;
		}
		Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.UPDATE_HOMETOP_RENDER, enable);
	}
}
