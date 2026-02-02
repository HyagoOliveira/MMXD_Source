#define RELEASE
using System;
using System.Collections.Generic;
using System.Linq;
using CallbackDefs;
using UnityEngine;
using UnityEngine.UI;
using cm;

public class FriendPVPSelectRoom : OrangeUIBase
{
	[SerializeField]
	private ScrollRect storageRect;

	[SerializeField]
	private LoopVerticalScrollRect scrollRect;

	[SerializeField]
	private FriendPVPRoomUnit roomSelectUIUnit;

	[SerializeField]
	private OrangeText textRewardCount;

	[SerializeField]
	private GameObject channelObj;

	[SerializeField]
	private Toggle allToggle;

	[SerializeField]
	private Toggle friendToggle;

	[SerializeField]
	private Toggle guildToggle;

	[SerializeField]
	private Button refreshBtn;

	[SerializeField]
	private GameObject controlBlock;

	[SerializeField]
	private Canvas canvasNoResultMsg;

	public List<RoomData> listRoomData = new List<RoomData>();

	private STAGE_TABLE _stageTable;

	private int _controlBlockTweenId;

	private Toggle _currentToggle;

	public STAGE_TABLE GetSelectStage
	{
		get
		{
			return _stageTable;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		MonoBehaviourSingleton<SignalDispatcher>.Instance.AddHandler(CM.RSPVPPrepareRoomList, OnRSPVPPrepareRoomList);
		MonoBehaviourSingleton<SignalDispatcher>.Instance.AddHandler(CM.RSPVPPrepareTargetRoomList, OnRSPVPPrepareTargetRoomList);
		backToHometopCB = (Callback)Delegate.Combine(backToHometopCB, new Callback(Clear));
		if ((bool)allToggle)
		{
			allToggle.onValueChanged.AddListener(delegate
			{
				OnClickToggleAll();
			});
		}
		if ((bool)friendToggle)
		{
			friendToggle.onValueChanged.AddListener(delegate
			{
				OnClickToggleFriend();
			});
		}
		if ((bool)guildToggle)
		{
			guildToggle.onValueChanged.AddListener(delegate
			{
				OnClickToggleGuild();
			});
		}
		if ((bool)refreshBtn)
		{
			refreshBtn.interactable = false;
		}
		EnableControlBlock(true);
	}

	public void Setup(STAGE_TABLE stageTable)
	{
		_stageTable = stageTable;
		channelObj.SetActive(MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.Setting.Community);
		OnClickBtnRefresh();
		_currentToggle = allToggle;
		EnableControlBlock();
	}

	private void OnDestroy()
	{
		LeanTween.cancel(ref _controlBlockTweenId);
	}

	public void EnableControlBlock(bool bEnable = false)
	{
		float delayTime = 5f;
		LeanTween.cancel(ref _controlBlockTweenId);
		if (!controlBlock)
		{
			return;
		}
		controlBlock.SetActive(bEnable);
		if (!bEnable)
		{
			return;
		}
		_controlBlockTweenId = LeanTween.delayedCall(base.gameObject, delayTime, (Action)delegate
		{
			if (this != null)
			{
				controlBlock.SetActive(false);
			}
		}).uniqueId;
	}

	public bool IsControlBlockEnabled()
	{
		return controlBlock.activeSelf;
	}

	public void OnClickBtnRefresh()
	{
		int pvpTier = 0;
		int pvpType = 4;
		if ((bool)refreshBtn)
		{
			refreshBtn.interactable = false;
		}
		if (allToggle.isOn)
		{
			MonoBehaviourSingleton<OrangeMatchManager>.Instance.SearchFriendPVPRoomList(_stageTable, pvpTier, pvpType, OnRSPVPPrepareRoomList);
		}
		else if (friendToggle.isOn)
		{
			string[] targetIDList = MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicFriend.Keys.ToArray();
			MonoBehaviourSingleton<OrangeMatchManager>.Instance.SearchFriendPVPTargetRoomList(_stageTable, pvpTier, pvpType, targetIDList, OnRSPVPPrepareTargetRoomList);
		}
		else if (guildToggle.isOn)
		{
			string[] targetIDList2 = Singleton<GuildSystem>.Instance.MemberInfoListCache.Select((NetMemberInfo p) => p.MemberId).ToArray();
			MonoBehaviourSingleton<OrangeMatchManager>.Instance.SearchFriendPVPTargetRoomList(_stageTable, pvpTier, pvpType, targetIDList2, OnRSPVPPrepareTargetRoomList);
		}
	}

	public void OnClickBtnJoinRoom()
	{
		PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_OK17);
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_FriendBattle", delegate(FriendBattleUI friendBattleUI)
		{
			ManagedSingleton<PlayerNetManager>.Instance.SealBattleSettingReq(ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.StandbyChara, delegate(string setting)
			{
				MonoBehaviourSingleton<OrangeMatchManager>.Instance.SelfSealedBattleSetting = setting;
				friendBattleUI.Setup(FriendBattleUI.RoleType.GUEST);
			});
		});
	}

	public void OnClickBtnCreateRoom()
	{
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_FriendPVPCreateRoom", delegate(FriendPVPCreateRoom friendPVPCreateRoom)
		{
			PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
			ManagedSingleton<PlayerNetManager>.Instance.SealBattleSettingReq(ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.StandbyChara, delegate(string setting)
			{
				MonoBehaviourSingleton<OrangeMatchManager>.Instance.SelfSealedBattleSetting = setting;
				FriendPVPCreateRoom friendPVPCreateRoom2 = friendPVPCreateRoom;
				friendPVPCreateRoom2.createRoomCB = (Callback)Delegate.Combine(friendPVPCreateRoom2.createRoomCB, new Callback(OnClickCloseBtn));
				friendPVPCreateRoom.Setup(_stageTable.n_ID);
			});
		});
	}

	private void ToggleSE(Toggle nowToggle)
	{
		if (_currentToggle != nowToggle)
		{
			PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_CURSOR01);
		}
		_currentToggle = nowToggle;
	}

	public void OnClickToggleAll()
	{
		if (allToggle.isOn)
		{
			Debug.Log("Toggle All");
			ToggleSE(allToggle);
			OnClickBtnRefresh();
		}
	}

	public void OnClickToggleFriend()
	{
		if (friendToggle.isOn)
		{
			Debug.Log("Toggle Friend");
			ToggleSE(friendToggle);
			OnClickBtnRefresh();
		}
	}

	public void OnClickToggleGuild()
	{
		if (guildToggle.isOn)
		{
			Debug.Log("Toggle Guild");
			ToggleSE(guildToggle);
			OnClickBtnRefresh();
		}
	}

	private void Clear()
	{
		MonoBehaviourSingleton<SignalDispatcher>.Instance.RemoveHandler(CM.RSPVPPrepareTargetRoomList, OnRSPVPPrepareTargetRoomList);
		MonoBehaviourSingleton<SignalDispatcher>.Instance.RemoveHandler(CM.RSPVPPrepareRoomList, OnRSPVPPrepareRoomList);
	}

	public override void OnClickCloseBtn()
	{
		Clear();
		base.OnClickCloseBtn();
	}

	private void OnRSPVPPrepareRoomList(object res)
	{
		scrollRect.ClearCells();
		listRoomData.Clear();
		if (res is RSPVPPrepareRoomList)
		{
			RSPVPPrepareRoomList rSPVPPrepareRoomList = (RSPVPPrepareRoomList)res;
			for (int i = 0; i < rSPVPPrepareRoomList.Roomcount; i++)
			{
				RoomData item = new RoomData(rSPVPPrepareRoomList.Roomid(i), "", "", rSPVPPrepareRoomList.Roomname(i), rSPVPPrepareRoomList.Capacity(i), rSPVPPrepareRoomList.Current(i), rSPVPPrepareRoomList.Ip(i), rSPVPPrepareRoomList.Port(i));
				listRoomData.Add(item);
			}
			scrollRect.OrangeInit(roomSelectUIUnit, 5, listRoomData.Count);
			canvasNoResultMsg.enabled = listRoomData.Count == 0;
		}
		if ((bool)refreshBtn)
		{
			refreshBtn.interactable = true;
		}
	}

	private void OnRSPVPPrepareTargetRoomList(object res)
	{
		scrollRect.ClearCells();
		listRoomData.Clear();
		if (res is RSPVPPrepareTargetRoomList)
		{
			RSPVPPrepareTargetRoomList rSPVPPrepareTargetRoomList = (RSPVPPrepareTargetRoomList)res;
			for (int i = 0; i < rSPVPPrepareTargetRoomList.Roomcount; i++)
			{
				RoomData item = new RoomData(rSPVPPrepareTargetRoomList.Roomid(i), "", "", rSPVPPrepareTargetRoomList.Roomname(i), rSPVPPrepareTargetRoomList.Capacity(i), rSPVPPrepareTargetRoomList.Current(i), rSPVPPrepareTargetRoomList.Ip(i), rSPVPPrepareTargetRoomList.Port(i));
				listRoomData.Add(item);
			}
			scrollRect.OrangeInit(roomSelectUIUnit, 5, listRoomData.Count);
			canvasNoResultMsg.enabled = listRoomData.Count == 0;
		}
		if ((bool)refreshBtn)
		{
			refreshBtn.interactable = true;
		}
	}

	public void OnClickChannel()
	{
		Debug.Log("OnClickChannel");
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_Channel", delegate(ChannelUI ui)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
			ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
			ui.Setup();
		});
	}
}
