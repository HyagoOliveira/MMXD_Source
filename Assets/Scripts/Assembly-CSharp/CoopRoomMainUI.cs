#define RELEASE
using System;
using System.Collections.Generic;
using System.Linq;
using CallbackDefs;
using OrangeAudio;
using OrangeSocket;
using StageLib;
using UnityEngine;
using UnityEngine.UI;
using cc;
using cm;
using enums;

internal class CoopRoomMainUI : OrangeUIBase
{
	[SerializeField]
	private CoopRoomMember roomMembers;

	[SerializeField]
	private LoopHorizontalScrollRect scrollRect;

	[SerializeField]
	private GameObject MasterBtn;

	[SerializeField]
	private GameObject MemberBtn;

	[SerializeField]
	private Text MembetReadyText;

	[SerializeField]
	private RectTransform ChallengeModeParent;

	[SerializeField]
	private ItemBoxTab tabInvite;

	[SerializeField]
	private ItemBoxTab tabStageInfo;

	[SerializeField]
	private GameObject stagePanel;

	[SerializeField]
	private GameObject invitePanel;

	[SerializeField]
	private OrangeText textStageName;

	[SerializeField]
	private OrangeText textLvReq;

	[SerializeField]
	private OrangeText textPrivate;

	[SerializeField]
	private OrangeText textCp;

	[SerializeField]
	private OrangeText textRewardCount;

	[SerializeField]
	private GameObject objGetReward;

	[SerializeField]
	private ItemIconWithAmount rewardIcon;

	[SerializeField]
	private RectTransform rewardParent;

	[SerializeField]
	private RectTransform extraParent;

	[SerializeField]
	private OrangeText textExtra;

	[SerializeField]
	private LoopVerticalScrollRect friendScrollRect;

	[SerializeField]
	private CoopRoomFriendUnit friendUnit;

	[SerializeField]
	private BonusInfoSub bonusInfoSubMenu;

	[SerializeField]
	private BonusInfoTag bonusTag;

	[SerializeField]
	private GameObject chatObj;

	[SerializeField]
	private Canvas canvasNoResultMsg;

	public Callback RoomRefreshCB;

	private List<ItemIconWithAmount> listReward = new List<ItemIconWithAmount>();

	public List<long> ListInviteTime = new List<long>();

	public bool IsNetLock;

	public float fNetLockTime;

	public const float fNetLockTimeOut = 10f;

	private bool assignHandler;

	private int curren_stageRewardId;

	private OrangeBgExt m_bgExt;

	private int otherCharacterID = -1;

	private bool IgnoreFirstSE = true;

	private bool bLock;

	private bool isAlreadyLeave;

	private int _communityBroadcastTween;

	public List<SocketFriendInfo> ListFriend { get; private set; }

	public STAGE_TABLE StageTable { get; set; }

	public string RoomId { get; set; }

	public bool IsRoomMaster { get; set; }

	public bool IsPrepare { get; private set; }

	public void Setup(NetSealBattleSettingInfo p_netSealBattleSettingInfo = null)
	{
		if (chatObj != null)
		{
			chatObj.SetActive(MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.Setting.Community);
		}
		ListFriend = SortFriendList(MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicFriend.Values.ToList());
		for (int i = 0; i < ListFriend.Count; i++)
		{
			ListInviteTime.Add(0L);
		}
		m_bgExt = Background as OrangeBgExt;
		m_bgExt.ChangeBackground(StageTable.s_BG);
		if (IsRoomMaster && ManagedSingleton<PlayerNetManager>.Instance.dicStage != null && !ManagedSingleton<PlayerNetManager>.Instance.dicStage.ContainsKey(StageTable.n_ID))
		{
			MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.bCurrentCoopChallengeMode = false;
		}
		if (IsRoomMaster)
		{
			MonoBehaviourSingleton<CMSocketClient>.Instance.SendProtocol(FlatBufferCMHelper.CreateRQChangeRoomSetting((short)StageTable.n_TYPE, StageTable.n_ID, MonoBehaviourSingleton<OrangeMatchManager>.Instance.bIsPublic, MonoBehaviourSingleton<OrangeMatchManager>.Instance.sCondition, MonoBehaviourSingleton<OrangeMatchManager>.Instance.sRoomName, MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.bCurrentCoopChallengeMode));
		}
		else
		{
			Transform obj2 = ChallengeModeParent.transform.Find("Toggle/ImgOn");
			Toggle componentInChildren = ChallengeModeParent.GetComponentInChildren<Toggle>();
			if ((bool)componentInChildren)
			{
				bLock = true;
				componentInChildren.isOn = MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.bCurrentCoopChallengeMode;
				bLock = false;
			}
			obj2.gameObject.SetActive(MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.bCurrentCoopChallengeMode);
		}
		MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(AssetBundleScriptableObject.Instance.m_uiPath + "background/Bg_CoopRoom", "Bg_CoopRoom", delegate(GameObject obj)
		{
			if ((bool)Background && null != obj)
			{
				UnityEngine.Object.Instantiate(obj, Background.transform, false);
			}
		});
		AddAllHandler();
		MonoBehaviourSingleton<OrangeMatchManager>.Instance.ListMemberInfo.Clear();
		if (p_netSealBattleSettingInfo != null)
		{
			MonoBehaviourSingleton<OrangeMatchManager>.Instance.ListMemberInfo.Add(new MemberInfo(MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify, ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.Nickname, 0, p_netSealBattleSettingInfo));
			scrollRect.OrangeInit(roomMembers, 2, MonoBehaviourSingleton<OrangeMatchManager>.Instance.ListMemberInfo.Count);
		}
		MonoBehaviourSingleton<VoiceChatManager>.Instance.SetVoiceServerName(RoomId);
		MembetReadyText.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("UI_ROOM_SELECT_READY");
		if (IsRoomMaster)
		{
			IsPrepare = true;
			MonoBehaviourSingleton<CMSocketClient>.Instance.SendProtocol(FlatBufferCMHelper.CreateRQSetPreparedOrNot(IsPrepare));
		}
		MasterBtn.SetActive(IsRoomMaster);
		MemberBtn.SetActive(!IsRoomMaster);
		OnClickTabStageInfo();
	}

	protected override void Awake()
	{
		base.Awake();
		backToHometopCB = (Callback)Delegate.Combine(backToHometopCB, new Callback(Clear));
		OrangeMatchManager instance = MonoBehaviourSingleton<OrangeMatchManager>.Instance;
		instance.OnDisconnectEvent = (Action)Delegate.Combine(instance.OnDisconnectEvent, new Action(RemoveAllHandler));
	}

    [Obsolete]
    private void AddAllHandler()
	{
		if (!assignHandler)
		{
			assignHandler = true;
			OrangeMatchManager.NetCBData onRSSetPreparedOrNotCB = MonoBehaviourSingleton<OrangeMatchManager>.Instance.OnRSSetPreparedOrNotCB;
			onRSSetPreparedOrNotCB.tCB = (CallbackObj)Delegate.Combine(onRSSetPreparedOrNotCB.tCB, new CallbackObj(OnRSSetPreparedOrNot));
			OrangeMatchManager.NetCBData onNTJoinPrepareRoomCB = MonoBehaviourSingleton<OrangeMatchManager>.Instance.OnNTJoinPrepareRoomCB;
			onNTJoinPrepareRoomCB.tCB = (CallbackObj)Delegate.Combine(onNTJoinPrepareRoomCB.tCB, new CallbackObj(OnNTJoinPrepareRoom));
			OrangeMatchManager.NetCBData onNTLeavePrepareRoomCB = MonoBehaviourSingleton<OrangeMatchManager>.Instance.OnNTLeavePrepareRoomCB;
			onNTLeavePrepareRoomCB.tCB = (CallbackObj)Delegate.Combine(onNTLeavePrepareRoomCB.tCB, new CallbackObj(OnNTLeavePrepareRoom));
			OrangeMatchManager.NetCBData onNTPVEPrepareRoomStartCB = MonoBehaviourSingleton<OrangeMatchManager>.Instance.OnNTPVEPrepareRoomStartCB;
			onNTPVEPrepareRoomStartCB.tCB = (CallbackObj)Delegate.Combine(onNTPVEPrepareRoomStartCB.tCB, new CallbackObj(OnNTPVEPrepareRoomStart));
			OrangeMatchManager.NetCBData onNTPrepareRoomOwnerChangeCB = MonoBehaviourSingleton<OrangeMatchManager>.Instance.OnNTPrepareRoomOwnerChangeCB;
			onNTPrepareRoomOwnerChangeCB.tCB = (CallbackObj)Delegate.Combine(onNTPrepareRoomOwnerChangeCB.tCB, new CallbackObj(OnNTPrepareRoomOwnerChange));
			OrangeMatchManager.NetCBData onRSChangePreparedSettingCB = MonoBehaviourSingleton<OrangeMatchManager>.Instance.OnRSChangePreparedSettingCB;
			onRSChangePreparedSettingCB.tCB = (CallbackObj)Delegate.Combine(onRSChangePreparedSettingCB.tCB, new CallbackObj(OnRSChangePreparedSetting));
			OrangeMatchManager.NetCBData onNTChangeRoomSettingCB = MonoBehaviourSingleton<OrangeMatchManager>.Instance.OnNTChangeRoomSettingCB;
			onNTChangeRoomSettingCB.tCB = (CallbackObj)Delegate.Combine(onNTChangeRoomSettingCB.tCB, new CallbackObj(OnNTChangeRoomSetting));
			OrangeMatchManager.NetCBData onNTRoomOwnerCB = MonoBehaviourSingleton<OrangeMatchManager>.Instance.OnNTRoomOwnerCB;
			onNTRoomOwnerCB.tCB = (CallbackObj)Delegate.Combine(onNTRoomOwnerCB.tCB, new CallbackObj(OnNTRoomOwner));
			MonoBehaviourSingleton<SignalDispatcher>.Instance.AddHandler(CC.NTCommunityBroadcastMessage, OnNTCommunityBroadcastMessage);
		}
	}

	private void OnDestroy()
	{
		RemoveAllHandler();
		OrangeMatchManager instance = MonoBehaviourSingleton<OrangeMatchManager>.Instance;
		instance.OnDisconnectEvent = (Action)Delegate.Remove(instance.OnDisconnectEvent, new Action(RemoveAllHandler));
		LeanTween.cancel(ref _communityBroadcastTween, false);
	}

    [Obsolete]
    private void RemoveAllHandler()
	{
		if (assignHandler)
		{
			assignHandler = false;
			OrangeMatchManager.NetCBData onRSSetPreparedOrNotCB = MonoBehaviourSingleton<OrangeMatchManager>.Instance.OnRSSetPreparedOrNotCB;
			onRSSetPreparedOrNotCB.tCB = (CallbackObj)Delegate.Remove(onRSSetPreparedOrNotCB.tCB, new CallbackObj(OnRSSetPreparedOrNot));
			OrangeMatchManager.NetCBData onNTJoinPrepareRoomCB = MonoBehaviourSingleton<OrangeMatchManager>.Instance.OnNTJoinPrepareRoomCB;
			onNTJoinPrepareRoomCB.tCB = (CallbackObj)Delegate.Remove(onNTJoinPrepareRoomCB.tCB, new CallbackObj(OnNTJoinPrepareRoom));
			OrangeMatchManager.NetCBData onNTLeavePrepareRoomCB = MonoBehaviourSingleton<OrangeMatchManager>.Instance.OnNTLeavePrepareRoomCB;
			onNTLeavePrepareRoomCB.tCB = (CallbackObj)Delegate.Remove(onNTLeavePrepareRoomCB.tCB, new CallbackObj(OnNTLeavePrepareRoom));
			OrangeMatchManager.NetCBData onNTPVEPrepareRoomStartCB = MonoBehaviourSingleton<OrangeMatchManager>.Instance.OnNTPVEPrepareRoomStartCB;
			onNTPVEPrepareRoomStartCB.tCB = (CallbackObj)Delegate.Remove(onNTPVEPrepareRoomStartCB.tCB, new CallbackObj(OnNTPVEPrepareRoomStart));
			OrangeMatchManager.NetCBData onNTPrepareRoomOwnerChangeCB = MonoBehaviourSingleton<OrangeMatchManager>.Instance.OnNTPrepareRoomOwnerChangeCB;
			onNTPrepareRoomOwnerChangeCB.tCB = (CallbackObj)Delegate.Remove(onNTPrepareRoomOwnerChangeCB.tCB, new CallbackObj(OnNTPrepareRoomOwnerChange));
			OrangeMatchManager.NetCBData onRSChangePreparedSettingCB = MonoBehaviourSingleton<OrangeMatchManager>.Instance.OnRSChangePreparedSettingCB;
			onRSChangePreparedSettingCB.tCB = (CallbackObj)Delegate.Remove(onRSChangePreparedSettingCB.tCB, new CallbackObj(OnRSChangePreparedSetting));
			OrangeMatchManager.NetCBData onNTChangeRoomSettingCB = MonoBehaviourSingleton<OrangeMatchManager>.Instance.OnNTChangeRoomSettingCB;
			onNTChangeRoomSettingCB.tCB = (CallbackObj)Delegate.Remove(onNTChangeRoomSettingCB.tCB, new CallbackObj(OnNTChangeRoomSetting));
			OrangeMatchManager.NetCBData onNTRoomOwnerCB = MonoBehaviourSingleton<OrangeMatchManager>.Instance.OnNTRoomOwnerCB;
			onNTRoomOwnerCB.tCB = (CallbackObj)Delegate.Remove(onNTRoomOwnerCB.tCB, new CallbackObj(OnNTRoomOwner));
			MonoBehaviourSingleton<SignalDispatcher>.Instance.RemoveHandler(CC.NTCommunityBroadcastMessage, OnNTCommunityBroadcastMessage);
		}
	}

	public override void OnClickCloseBtn()
	{
		Clear();
		base.OnClickCloseBtn();
		RoomRefreshCB.CheckTargetToInvoke();
	}

	private void Clear()
	{
		MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicChatLog[ChatChannel.TeamChannel].Clear();
		MonoBehaviourSingleton<CMSocketClient>.Instance.SendProtocol(FlatBufferCMHelper.CreateRQLeavePrepareRoom());
	}

	public void OnClickBtnPrepare()
	{
		if (!IsRoomMaster)
		{
			Debug.Log("OnClickBtnPrepare");
			IsPrepare = !IsPrepare;
			if (IsPrepare)
			{
				MembetReadyText.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("UI_ROOM_SELECT_CANCEL_READY");
				MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_CURSOR05);
				EnableChallengeToggle(false);
			}
			else
			{
				MembetReadyText.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("UI_ROOM_SELECT_READY");
				MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_CANCEL);
				EnableChallengeToggle(true);
			}
			MonoBehaviourSingleton<CMSocketClient>.Instance.SendProtocol(FlatBufferCMHelper.CreateRQSetPreparedOrNot(IsPrepare));
		}
	}

	public void OnChallengeChange(bool bEnable)
	{
		if (!IsRoomMaster)
		{
			if (bLock)
			{
				return;
			}
			bLock = true;
			Toggle componentInChildren = ChallengeModeParent.GetComponentInChildren<Toggle>();
			if ((bool)componentInChildren)
			{
				PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_OK01);
				componentInChildren.isOn = !bEnable;
			}
			MonoBehaviourSingleton<UIManager>.Instance.LoadResourceUI("UI_CommonMsg", delegate(CommonUI ui)
			{
				ui.OpenSE = SystemSE.CRI_SYSTEMSE_SYS_ERROR;
				ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
				ui.SetupConfirmByKey("COMMON_TIP", "ROOM_OWNER_ONLY", "COMMON_OK", delegate
				{
					bLock = false;
				});
			}, true);
		}
		else
		{
			Transform obj = ChallengeModeParent.transform.Find("Toggle/ImgOn");
			MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.bCurrentCoopChallengeMode = bEnable;
			obj.gameObject.SetActive(bEnable);
			PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_OK01);
			MonoBehaviourSingleton<CMSocketClient>.Instance.SendProtocol(FlatBufferCMHelper.CreateRQChangeRoomSetting((short)StageTable.n_TYPE, StageTable.n_ID, MonoBehaviourSingleton<OrangeMatchManager>.Instance.bIsPublic, MonoBehaviourSingleton<OrangeMatchManager>.Instance.sCondition, MonoBehaviourSingleton<OrangeMatchManager>.Instance.sRoomName, bEnable));
		}
	}

	private void EnableChallengeToggle(bool bEnable)
	{
		Toggle componentInChildren = ChallengeModeParent.GetComponentInChildren<Toggle>();
		OrangeText componentInChildren2 = ChallengeModeParent.GetComponentInChildren<OrangeText>();
		if ((bool)componentInChildren && (bool)componentInChildren2)
		{
			Color color = componentInChildren2.color;
			componentInChildren.interactable = bEnable;
			componentInChildren2.color = (bEnable ? new Color(color.r, color.g, color.b, 1f) : new Color(color.r, color.g, color.b, 0.5f));
		}
	}

	public void OnClickBtnStart()
	{
		if (ManagedSingleton<EquipHelper>.Instance.ShowEquipmentLimitReachedDialog())
		{
			return;
		}
		Debug.Log("OnClickBtnStart");
		if (!IsRoomMaster)
		{
			return;
		}
		if (IsNetLock && Time.realtimeSinceStartup - fNetLockTime < 10f)
		{
			Debug.Log("Wait Net Message, Don't do anything");
			return;
		}
		IsNetLock = true;
		fNetLockTime = Time.realtimeSinceStartup;
		MonoBehaviourSingleton<SignalDispatcher>.Instance.AddHandler(CM.RSPVEPrepareRoomStart, delegate(object res)
		{
			if (res is RSPVEPrepareRoomStart)
			{
				RSPVEPrepareRoomStart rSPVEPrepareRoomStart = (RSPVEPrepareRoomStart)res;
				if (rSPVEPrepareRoomStart.Result != 66000)
				{
					IsNetLock = false;
					if (rSPVEPrepareRoomStart.Result == 66053)
					{
						MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_Tip", delegate(TipUI ui)
						{
							ui.Setup(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("ROOM_MEMBER_READY"), true);
						});
					}
					else if (StageTable.n_SINGLEPLAY == 0 && MonoBehaviourSingleton<OrangeMatchManager>.Instance.ListMemberInfo.Count <= 1)
					{
						MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_Tip", delegate(TipUI ui)
						{
							ui.Setup(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COOP_ROOM_MEMBER_NOT_ENOUGH"), true);
						});
					}
				}
				else
				{
					MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK07);
				}
			}
		}, 0, true);
		MonoBehaviourSingleton<CMSocketClient>.Instance.SendProtocol(FlatBufferCMHelper.CreateRQPVEPrepareRoomStart());
	}

	private void UpdateRoomMember()
	{
		scrollRect.OrangeInit(roomMembers, MonoBehaviourSingleton<OrangeMatchManager>.Instance.ListMemberInfo.Count, MonoBehaviourSingleton<OrangeMatchManager>.Instance.ListMemberInfo.Count);
	}

	public void OnClickTabInvite()
	{
		tabInvite.UpdateState(false);
		tabStageInfo.UpdateState(true);
		stagePanel.SetActive(false);
		invitePanel.SetActive(true);
		UpdateInviteInfo();
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_CURSOR07);
	}

	public void OnClickTabStageInfo()
	{
		tabInvite.UpdateState(true);
		tabStageInfo.UpdateState(false);
		stagePanel.SetActive(true);
		invitePanel.SetActive(false);
		UpdateStageInfo();
		if (!IgnoreFirstSE)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_CURSOR07);
		}
		else
		{
			IgnoreFirstSE = false;
		}
	}

	public void OnClickGetRewardBtn()
	{
		objGetReward.SetActive(!objGetReward.activeSelf);
	}

	private void UpdateInviteInfo()
	{
		friendScrollRect.ClearCells();
		friendScrollRect.OrangeInit(friendUnit, 4, ListFriend.Count);
		canvasNoResultMsg.enabled = ListFriend.Count == 0;
	}

	private void UpdateStageInfo()
	{
		textStageName.text = ManagedSingleton<OrangeTextDataManager>.Instance.STAGETEXT_TABLE_DICT.GetL10nValue(StageTable.w_NAME);
		textLvReq.text = "";
		textPrivate.text = "";
		textCp.text = StageTable.n_CP.ToString();
		StageInfo value = null;
		bool flag = ManagedSingleton<PlayerNetManager>.Instance.dicStage.TryGetValue(StageTable.n_ID, out value);
		SetRewardList((flag && value.netStageInfo.Star > 0) ? StageTable.n_FIRST_REWARD : StageTable.n_GET_REWARD);
		textRewardCount.text = string.Format("{0}/{1}", ManagedSingleton<StageHelper>.Instance.GetCoopRewardCount(), OrangeConst.CORP_REWARD_COUNT);
		if (flag)
		{
			SetExtraList(value.netStageInfo.StageID);
			if (StageTable.n_SECRET != 0 && ChallengeModeParent != null)
			{
				ChallengeModeParent.gameObject.SetActive(true);
			}
		}
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

	private void SetRewardList(int p_stageRewardId)
	{
		foreach (ItemIconWithAmount item in listReward)
		{
			UnityEngine.Object.Destroy(item.gameObject);
		}
		listReward.Clear();
		curren_stageRewardId = p_stageRewardId;
		List<GACHA_TABLE> listGachaByGroup = ManagedSingleton<ExtendDataHelper>.Instance.GetListGachaByGroup(p_stageRewardId);
		int count = listGachaByGroup.Count;
		for (int i = 0; i < count; i++)
		{
			ItemIconWithAmount itemIconWithAmount = UnityEngine.Object.Instantiate(rewardIcon, rewardParent);
			if (i >= listGachaByGroup.Count)
			{
				itemIconWithAmount.Clear();
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
				itemIconWithAmount.Setup(i, bundlePath, assetPath, OnClickUnit);
				itemIconWithAmount.SetRare(rare);
				itemIconWithAmount.ClearAmount();
			}
			listReward.Add(itemIconWithAmount);
		}
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
			UnityEngine.Object.Destroy(child);
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
					ItemIconWithAmount itemIconWithAmount = UnityEngine.Object.Instantiate(rewardIcon, extraParent);
					OrangeText orangeText = UnityEngine.Object.Instantiate(textExtra, itemIconWithAmount.transform);
					orangeText.transform.localPosition = new Vector3(300f, 0f, 0f);
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

	public void GoToGoCheck()
	{
		if (IsRoomMaster || !IsPrepare)
		{
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_GoCheck", delegate(GoCheckUI ui)
			{
				ui.Setup(StageTable);
				ui.EnableBackToHometop = false;
				ui.destroyCB = (Callback)Delegate.Combine(ui.destroyCB, new Callback(CheckSelfData));
				ui.bJustReturnToLastUI = true;
				ui.bIsHaveRoom = true;
				ui.closeCB = (Callback)Delegate.Combine(ui.closeCB, new Callback(CheckSelfData));
			});
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK13);
		}
	}

	private void CheckSelfData()
	{
		int count = MonoBehaviourSingleton<OrangeMatchManager>.Instance.ListMemberInfo.Count;
		for (int i = 0; i < count; i++)
		{
			if (MonoBehaviourSingleton<OrangeMatchManager>.Instance.ListMemberInfo[i].PlayerId == MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify)
			{
				ManagedSingleton<PlayerNetManager>.Instance.SealBattleSettingReq(ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.StandbyChara, delegate(string setting)
				{
					MonoBehaviourSingleton<OrangeMatchManager>.Instance.SelfSealedBattleSetting = setting;
					MonoBehaviourSingleton<CMSocketClient>.Instance.SendProtocol(FlatBufferCMHelper.CreateRQChangePrepareSetting(MonoBehaviourSingleton<OrangeMatchManager>.Instance.SelfSealedBattleSetting));
				});
				break;
			}
		}
	}

	public override bool CanBuy()
	{
		if (!IsRoomMaster)
		{
			return !IsPrepare;
		}
		return true;
	}

	public void PlayCharVoice(NetSealBattleSettingInfo setting)
	{
		CHARACTER_TABLE value;
		if (ManagedSingleton<OrangeDataManager>.Instance.CHARACTER_TABLE_DICT.TryGetValue(setting.CharacterList[0].CharacterID, out value))
		{
			string acb = AudioLib.GetVoice(ref value);
			MonoBehaviourSingleton<AudioManager>.Instance.PreloadAtomSource(acb, 3, delegate
			{
				MonoBehaviourSingleton<AudioManager>.Instance.Play(acb, 2);
			});
		}
	}

	public void OnClickChat()
	{
		Debug.Log("OnClickChannelBtn");
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_Channel", delegate(ChannelUI ui)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
			ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
			ui.SetupTeam();
		});
	}

	private void OnApplicationPause(bool pause)
	{
		if (isAlreadyLeave || !pause)
		{
			return;
		}
		isAlreadyLeave = true;
		MonoBehaviourSingleton<CMSocketClient>.Instance.SendProtocol(FlatBufferCMHelper.CreateRQLeavePrepareRoom());
		MonoBehaviourSingleton<UIManager>.Instance.LoadResourceUI("UI_CommonMsg", delegate(CommonUI ui)
		{
			ui.OpenSE = SystemSE.CRI_SYSTEMSE_SYS_ERROR;
			ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
			ui.SetupConfirmByKey("COMMON_TIP", "ROOM_KICKED", "COMMON_OK", delegate
			{
				OnClickCloseBtn();
			});
		});
	}

	private List<SocketFriendInfo> SortFriendList(List<SocketFriendInfo> friendList)
	{
		friendList.Sort((SocketFriendInfo x, SocketFriendInfo y) => (x.Busy <= 30 || y.Busy <= 30) ? x.Busy.CompareTo(y.Busy) : y.Busy.CompareTo(x.Busy));
		return friendList;
	}

	private void OnNTCommunityBroadcastMessage(object res)
	{
		LeanTween.cancel(ref _communityBroadcastTween, false);
		_communityBroadcastTween = LeanTween.delayedCall(2f, (Action)delegate
		{
			if (this != null && invitePanel.activeSelf)
			{
				ListFriend = SortFriendList(MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicFriend.Values.ToList());
				UpdateInviteInfo();
			}
		}).uniqueId;
	}

	private void OnRSSetPreparedOrNot(object res)
	{
		if (res is RSSetPreparedOrNot)
		{
			int result = ((RSSetPreparedOrNot)res).Result;
			int num = 65000;
		}
	}

	private void OnRSChangePreparedSetting(object res)
	{
		if (res is NTChangePrepareSetting)
		{
			NTChangePrepareSetting nTChangePrepareSetting = (NTChangePrepareSetting)res;
			NetSealBattleSettingInfo netSealBattleSettingInfo = null;
			if (!ManagedSingleton<PlayerHelper>.Instance.ParserUnsealedBattleSetting(nTChangePrepareSetting.Unsealedbattlesetting, out netSealBattleSettingInfo))
			{
				Debug.LogError("ParserUnsealedBattleSetting Fail!");
				OnClickCloseBtn();
				return;
			}
			int count = MonoBehaviourSingleton<OrangeMatchManager>.Instance.ListMemberInfo.Count;
			for (int i = 0; i < count; i++)
			{
				if (!(MonoBehaviourSingleton<OrangeMatchManager>.Instance.ListMemberInfo[i].PlayerId == nTChangePrepareSetting.Playerid))
				{
					continue;
				}
				MonoBehaviourSingleton<OrangeMatchManager>.Instance.ListMemberInfo[i].netSealBattleSettingInfo = netSealBattleSettingInfo;
				UpdateRoomMember();
				if (nTChangePrepareSetting.Playerid != MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify)
				{
					if (MonoBehaviourSingleton<OrangeMatchManager>.Instance.ListMemberInfo[i].bPrepared)
					{
						MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_CURSOR05);
					}
					if (otherCharacterID != netSealBattleSettingInfo.CharacterList[0].CharacterID)
					{
						PlayCharVoice(netSealBattleSettingInfo);
						otherCharacterID = netSealBattleSettingInfo.CharacterList[0].CharacterID;
					}
				}
				break;
			}
		}
		if (res is RSChangePrepareSetting && ((RSChangePrepareSetting)res).Result != 64000)
		{
			UpdateRoomMember();
		}
	}

	private void OnNTJoinPrepareRoom(object res)
	{
		MonoBehaviourSingleton<OrangeMatchManager>.Instance.ListMemberInfo.Clear();
		if (!(res is NTJoinPrepareRoom))
		{
			return;
		}
		NTJoinPrepareRoom nTJoinPrepareRoom = (NTJoinPrepareRoom)res;
		int unsealedbattlesettingLength = nTJoinPrepareRoom.UnsealedbattlesettingLength;
		for (int i = 0; i < unsealedbattlesettingLength; i++)
		{
			NetSealBattleSettingInfo netSealBattleSettingInfo = null;
			if (ManagedSingleton<PlayerHelper>.Instance.ParserUnsealedBattleSetting(nTJoinPrepareRoom.Unsealedbattlesetting(i), out netSealBattleSettingInfo))
			{
				if (MonoBehaviourSingleton<OrangeMatchManager>.Instance.ListMemberInfo.Count == 0)
				{
					bool isRoomMaster = IsRoomMaster;
				}
				MemberInfo memberInfo = new MemberInfo(nTJoinPrepareRoom.Playerid(i), nTJoinPrepareRoom.NickName(i), 0, netSealBattleSettingInfo);
				if (nTJoinPrepareRoom.Playerid(i) != MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify && !IsRoomMaster)
				{
					memberInfo.bPrepared = true;
				}
				MonoBehaviourSingleton<OrangeMatchManager>.Instance.ListMemberInfo.Add(memberInfo);
				if (nTJoinPrepareRoom.Playerid(i) != MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify)
				{
					PlayCharVoice(netSealBattleSettingInfo);
					otherCharacterID = netSealBattleSettingInfo.CharacterList[0].CharacterID;
				}
				continue;
			}
			OnClickCloseBtn();
			return;
		}
		if (IsRoomMaster)
		{
			IsPrepare = true;
		}
		MasterBtn.SetActive(IsRoomMaster);
		MemberBtn.SetActive(!IsRoomMaster);
		MonoBehaviourSingleton<CMSocketClient>.Instance.SendProtocol(FlatBufferCMHelper.CreateRQSetPreparedOrNot(IsPrepare));
		UpdateRoomMember();
	}

	private void OnNTLeavePrepareRoom(object res)
	{
		if (res is NTLeavePrepareRoom)
		{
			NTLeavePrepareRoom rs = (NTLeavePrepareRoom)res;
			Debug.Log("Leave:" + rs.Playerid);
			MemberInfo memberInfo = null;
			memberInfo = MonoBehaviourSingleton<OrangeMatchManager>.Instance.ListMemberInfo.First((MemberInfo x) => x.PlayerId == rs.Playerid);
			if (memberInfo != null)
			{
				MonoBehaviourSingleton<OrangeMatchManager>.Instance.ListMemberInfo.Remove(memberInfo);
			}
			UpdateRoomMember();
		}
	}

	private void OnNTPVEPrepareRoomStart(object res)
	{
		if (!(res is NTPVEPrepareRoomStart))
		{
			return;
		}
		if (!IsRoomMaster)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK07);
		}
		NTPVEPrepareRoomStart nTPVEPrepareRoomStart = (NTPVEPrepareRoomStart)res;
		Debug.Log(string.Format("Ip:{0},Port:{1},Roomid:{2},Stagetype:{3},Stageid:{4}", nTPVEPrepareRoomStart.Ip, nTPVEPrepareRoomStart.Port, nTPVEPrepareRoomStart.Roomid, nTPVEPrepareRoomStart.Stagetype, nTPVEPrepareRoomStart.Stageid));
		MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.bCurrentCoopChallengeMode = nTPVEPrepareRoomStart.Challenge;
		if (MonoBehaviourSingleton<OrangeMatchManager>.Instance.SingleMatch)
		{
			ManagedSingleton<StageHelper>.Instance.nLastStageID = nTPVEPrepareRoomStart.Stageid;
			STAGE_TABLE sTAGE_TABLE = ManagedSingleton<OrangeDataManager>.Instance.STAGE_TABLE_DICT[nTPVEPrepareRoomStart.Stageid];
			if (sTAGE_TABLE.n_STAGE_RULE > 0)
			{
				ManagedSingleton<StageHelper>.Instance.nLastStageRuleID_Status = sTAGE_TABLE.n_STAGE_RULE;
			}
			else
			{
				ManagedSingleton<StageHelper>.Instance.nLastStageRuleID_Status = 0;
			}
			if (MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.bCurrentCoopChallengeMode && sTAGE_TABLE.n_TYPE == 5 && sTAGE_TABLE.n_SECRET != 0)
			{
				ManagedSingleton<StageHelper>.Instance.nLastStageRuleID_Status = sTAGE_TABLE.n_SECRET;
			}
			ManagedSingleton<StageHelper>.Instance.nStageEndGoUI = StageHelper.STAGE_END_GO.COOPSTAGESELECT;
			StageUpdate.SetStageName(sTAGE_TABLE.s_STAGE, sTAGE_TABLE.n_DIFFICULTY);
			MonoBehaviourSingleton<OrangeSceneManager>.Instance.ChangeScene("StageTest", OrangeSceneManager.LoadingType.STAGE, null, false);
		}
		else
		{
			ManagedSingleton<StageHelper>.Instance.nStageEndGoUI = StageHelper.STAGE_END_GO.COOPSTAGESELECT;
			MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.ReadyToGoPVE(nTPVEPrepareRoomStart.Ip, nTPVEPrepareRoomStart.Port, nTPVEPrepareRoomStart.Roomid, nTPVEPrepareRoomStart.Stagetype, nTPVEPrepareRoomStart.Stageid);
		}
	}

	private void OnNTPrepareRoomOwnerChange(object res)
	{
		if (res is NTPrepareRoomOwnerChange)
		{
			IsRoomMaster = ((((NTPrepareRoomOwnerChange)res).Playerid == MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify) ? true : false);
			if (IsRoomMaster)
			{
				IsPrepare = true;
				MonoBehaviourSingleton<CMSocketClient>.Instance.SendProtocol(FlatBufferCMHelper.CreateRQSetPreparedOrNot(IsPrepare));
			}
			MasterBtn.SetActive(IsRoomMaster);
			MemberBtn.SetActive(!IsRoomMaster);
		}
	}

	private void OnNTChangeRoomSetting(object res)
	{
		if (res is NTChangeRoomSetting)
		{
			NTChangeRoomSetting nTChangeRoomSetting = (NTChangeRoomSetting)res;
			MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.bCurrentCoopChallengeMode = nTChangeRoomSetting.Ischallenge;
			Toggle componentInChildren = ChallengeModeParent.GetComponentInChildren<Toggle>();
			if ((bool)componentInChildren && componentInChildren.isOn != nTChangeRoomSetting.Ischallenge)
			{
				bLock = true;
				componentInChildren.isOn = nTChangeRoomSetting.Ischallenge;
				bLock = false;
			}
			ChallengeModeParent.transform.Find("Toggle/ImgOn").gameObject.SetActive(nTChangeRoomSetting.Ischallenge);
		}
		if (res is RSChangeRoomSetting)
		{
			RSChangeRoomSetting rSChangeRoomSetting = (RSChangeRoomSetting)res;
		}
	}

	private void OnNTRoomOwner(object res)
	{
		Debug.Log("OnNTRoomOwner");
		IsRoomMaster = true;
		IsPrepare = IsRoomMaster;
		MonoBehaviourSingleton<CMSocketClient>.Instance.SendProtocol(FlatBufferCMHelper.CreateRQSetPreparedOrNot(IsPrepare));
		MasterBtn.SetActive(IsRoomMaster);
		MemberBtn.SetActive(!IsRoomMaster);
	}
}
