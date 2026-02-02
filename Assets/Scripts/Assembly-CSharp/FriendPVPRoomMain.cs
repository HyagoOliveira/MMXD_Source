#define RELEASE
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CallbackDefs;
using OrangeApi;
using OrangeAudio;
using OrangeSocket;
using UnityEngine;
using UnityEngine.UI;
using cb;
using cc;
using cm;
using enums;

public class FriendPVPRoomMain : OrangeUIBase
{
	[SerializeField]
	private CoopRoomMember roomMembers;

	[SerializeField]
	private LoopHorizontalScrollRect scrollRect;

	[SerializeField]
	private GameObject startBtn;

	[SerializeField]
	private GameObject readyBtn;

	[SerializeField]
	private GameObject battleSettingBtn;

	[SerializeField]
	private Text MemberReadyText;

	[SerializeField]
	private OrangeText inviteIDText;

	[SerializeField]
	private LoopVerticalScrollRect friendScrollRect;

	[SerializeField]
	private CoopRoomFriendUnit friendUnit;

	[SerializeField]
	private GameObject invitePage;

	[SerializeField]
	private Toggle friendToggle;

	[SerializeField]
	private Toggle guildToggle;

	[SerializeField]
	private Toggle zoneToggle;

	[SerializeField]
	private GameObject localBroadcastBlock;

	[SerializeField]
	private GameObject serverBroadcastBlock;

	[SerializeField]
	private Button localBroadcastBtn;

	[SerializeField]
	private Button serverBroadcastBtn;

	[SerializeField]
	private Canvas canvasNoResultMsg;

	[SerializeField]
	private GameObject chatObj;

	[SerializeField]
	private OrangeText debugString;

	public List<long> ListInviteTime = new List<long>();

	public bool IsNetLock;

	public float fNetLockTime;

	public const float fNetLockTimeOut = 10f;

	private bool _assignHandler;

	private bool _bLockControl;

	private string _roomID;

	private string _inviteCode;

	private bool _bIsHost = true;

	private STAGE_TABLE _stageTable;

	private int _CDTweenVoice;

	private int _timeoutCountDownTween;

	private int _maxRoomMember = 2;

	private float _debugTimeoutTimer;

	private bool _bIsContinue;

	private int _timeoutContinueTween;

	private int _communityBroadcastTween;

	private CommonUI _waitForConnection;

	private int otherCharacterID = -1;

	private Toggle _currentToggle;

	private bool bMute;

	public List<SocketFriendInfo> ListFriend { get; private set; }

	public STAGE_TABLE StageTable
	{
		get
		{
			return _stageTable;
		}
	}

	public string RoomId
	{
		get
		{
			return _roomID;
		}
	}

	public bool IsHost
	{
		get
		{
			return _bIsHost;
		}
	}

	public bool IsPrepared { get; private set; }

	private void ToggleSE(Toggle nowToggle)
	{
		if (_currentToggle != nowToggle && !bMute)
		{
			PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_OK01);
		}
		bMute = false;
		_currentToggle = nowToggle;
	}

	protected override void Awake()
	{
		base.Awake();
		OrangeMatchManager instance = MonoBehaviourSingleton<OrangeMatchManager>.Instance;
		instance.OnDisconnectEvent = (Action)Delegate.Combine(instance.OnDisconnectEvent, new Action(RemoveAllHandler));
		friendToggle.onValueChanged.AddListener(delegate(bool f)
		{
			ClickFriendToggle(f);
		});
		guildToggle.onValueChanged.AddListener(delegate(bool f)
		{
			ClickGuildToggle(f);
		});
		zoneToggle.onValueChanged.AddListener(delegate(bool f)
		{
			ClickZoneToggle(f);
		});
		friendToggle.onValueChanged.AddListener(delegate
		{
			ToggleSE(friendToggle);
		});
		guildToggle.onValueChanged.AddListener(delegate
		{
			ToggleSE(guildToggle);
		});
		zoneToggle.onValueChanged.AddListener(delegate
		{
			ToggleSE(zoneToggle);
		});
		localBroadcastBtn.gameObject.AddOrGetComponent<OrangeCriSource>().Initial(OrangeSSType.SYSTEM);
		serverBroadcastBtn.gameObject.AddOrGetComponent<OrangeCriSource>().Initial(OrangeSSType.SYSTEM);
	}

	public void Setup(bool bIsHost, string roomID, string inviteCode, STAGE_TABLE stageTable, bool bIsContinue = false)
	{
		_bIsHost = bIsHost;
		_stageTable = stageTable;
		_roomID = roomID;
		_inviteCode = inviteCode;
		_bIsContinue = bIsContinue;
		if (chatObj != null)
		{
			chatObj.SetActive(MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.Setting.Community);
		}
		Singleton<GuildSystem>.Instance.ReqGetGuildInfo();
		if (_bIsContinue)
		{
			StartContinueRoomTimeoutCountDown();
		}
		else
		{
			StartRoomTimeoutCountdown();
		}
		AddAllHandler();
		_currentToggle = friendToggle;
		if (inviteIDText != null)
		{
			if (bIsHost)
			{
				inviteIDText.text = _inviteCode;
			}
			else
			{
				MonoBehaviourSingleton<CMSocketClient>.Instance.SendProtocol(FlatBufferCMHelper.CreateRQPVPPrepareRoomInfo(roomID));
			}
		}
		ListFriend = SortFriendList(MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicFriend.Values.ToList());
		for (int i = 0; i < ListFriend.Count; i++)
		{
			ListInviteTime.Add(0L);
		}
		UpdateInviteInfo();
		MonoBehaviourSingleton<OrangeMatchManager>.Instance.ListMemberInfo.Clear();
		NetSealBattleSettingInfo selfNetSealBattleSettingInfo = MonoBehaviourSingleton<OrangeMatchManager>.Instance.GetSelfNetSealBattleSettingInfo();
		if (selfNetSealBattleSettingInfo != null)
		{
			MemberInfo item = new MemberInfo(MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify, ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.Nickname, 0, selfNetSealBattleSettingInfo);
			MonoBehaviourSingleton<OrangeMatchManager>.Instance.ListMemberInfo.Add(item);
			UpdateRoomMember();
		}
		IsPrepared = _bIsHost;
		MonoBehaviourSingleton<CMSocketClient>.Instance.SendProtocol(FlatBufferCMHelper.CreateRQSetPreparedOrNot(IsPrepared));
		startBtn.SetActive(_bIsHost);
		readyBtn.SetActive(!_bIsHost);
		zoneToggle.gameObject.SetActive(IsHost);
	}

	private List<SocketFriendInfo> SortFriendList(List<SocketFriendInfo> friendList)
	{
		friendList.Sort((SocketFriendInfo x, SocketFriendInfo y) => (x.Busy <= 30 || y.Busy <= 30) ? x.Busy.CompareTo(y.Busy) : y.Busy.CompareTo(x.Busy));
		return friendList;
	}

	private void StartContinueRoomTimeoutCountDown()
	{
		MonoBehaviourSingleton<UIManager>.Instance.LoadResourceUI("UI_CommonMsg", delegate(CommonUI commomMsg)
		{
			_waitForConnection = commomMsg;
			commomMsg.SetupConfirm(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_TIP"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("PVP_FRIEND_PLAYERBACK"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_CANCEL"), delegate
			{
				if (MonoBehaviourSingleton<OrangeMatchManager>.Instance.ListMemberInfo.Count < _maxRoomMember)
				{
					OnClickCloseBtn();
				}
			});
			LeanTween.cancel(ref _timeoutContinueTween, false);
			_timeoutContinueTween = LeanTween.delayedCall(base.gameObject, (float)OrangeConst.PVP_RETURN_TIME, (Action)delegate
			{
				commomMsg.OnClickCloseBtn();
				MonoBehaviourSingleton<FriendPVPHelper>.Instance.DisplayOpponentLeftMsg();
			}).uniqueId;
		});
	}

	private void ClickFriendToggle(bool bEnable)
	{
		friendScrollRect.gameObject.SetActive(bEnable);
		invitePage.SetActive(!bEnable);
		if (friendToggle.isOn)
		{
			ListFriend = SortFriendList(MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicFriend.Values.ToList());
			UpdateInviteInfo();
		}
	}

	private void ClickGuildToggle(bool bEnable)
	{
		friendScrollRect.gameObject.SetActive(bEnable);
		invitePage.SetActive(!bEnable);
		if (guildToggle.isOn)
		{
			friendScrollRect.ClearCells();
			Singleton<GuildSystem>.Instance.OnGetCheckGuildStateOnceEvent += OnGetCheckGuildStateEvent;
			Singleton<GuildSystem>.Instance.ReqCheckGuildState();
		}
	}

	private void ClickZoneToggle(bool bEnable)
	{
		friendScrollRect.gameObject.SetActive(!bEnable);
		invitePage.SetActive(bEnable);
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

	public void OnClickBtnPrepare()
	{
		Debug.Log("OnClickBtnPrepare");
		if (_bLockControl)
		{
			return;
		}
		_bLockControl = true;
		if (!IsHost)
		{
			IsPrepared = !IsPrepared;
			if ((bool)battleSettingBtn)
			{
				battleSettingBtn.GetComponent<Button>().interactable = !IsPrepared;
			}
			if (IsPrepared)
			{
				MemberReadyText.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("UI_ROOM_SELECT_CANCEL_READY");
				MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_CURSOR05);
			}
			else
			{
				MemberReadyText.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("UI_ROOM_SELECT_READY");
				MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_CANCEL);
			}
			MonoBehaviourSingleton<CMSocketClient>.Instance.SendProtocol(FlatBufferCMHelper.CreateRQSetPreparedOrNot(IsPrepared));
		}
	}

	public void OnClickBtnStart()
	{
		if (_bLockControl)
		{
			return;
		}
		_bLockControl = true;
		if (ManagedSingleton<EquipHelper>.Instance.ShowEquipmentLimitReachedDialog())
		{
			return;
		}
		Debug.Log("OnClickBtnStart");
		if (!_bIsHost)
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
		MonoBehaviourSingleton<SignalDispatcher>.Instance.AddHandler(CM.RSPVPFriendRoomStart, delegate(object res)
		{
			if (res is RSPVPFriendRoomStart)
			{
				RSPVPFriendRoomStart rs = (RSPVPFriendRoomStart)res;
				if (rs.Result != 66000)
				{
					_bLockControl = false;
					IsNetLock = false;
					if (rs.Result == 66053)
					{
						MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_Tip", delegate(TipUI ui)
						{
							ui.Setup(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("ROOM_MEMBER_READY"), true);
						});
					}
					else
					{
						MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_Tip", delegate(TipUI ui)
						{
							string p_msg = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("GAME_ERROR_MSG_DESC"), rs.Result, ((Code)rs.Result).ToString());
							ui.Setup(p_msg, true);
						});
					}
				}
			}
		}, 0, true);
		MonoBehaviourSingleton<CMSocketClient>.Instance.SendProtocol(FlatBufferCMHelper.CreateRQPVPFriendRoomStart(0, 4, MonoBehaviourSingleton<OrangeMatchManager>.Instance.StageID));
	}

	private void SaveHostIDToLocal()
	{
		if (IsHost)
		{
			MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.Setting.FriendPVPHostID = MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify;
		}
		else
		{
			foreach (MemberInfo item in MonoBehaviourSingleton<OrangeMatchManager>.Instance.ListMemberInfo)
			{
				if (item.PlayerId != MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify)
				{
					MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.Setting.FriendPVPHostID = item.PlayerId;
					break;
				}
			}
		}
		Debug.Log("SaveHostIDToLocal: Host = " + MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.Setting.FriendPVPHostID);
		MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.Save();
	}

	private IEnumerator OnStartCountDown(Button button, GameObject objInviteLimit, long val)
	{
		button.interactable = false;
		objInviteLimit.SetActive(true);
		OrangeCriSource ocs = button.GetComponent<OrangeCriSource>();
		ocs.PlaySE("SystemSE", "sys_access01_lp");
		Text textInviteLimitTime = objInviteLimit.GetComponentInChildren<Text>();
		if ((bool)textInviteLimitTime)
		{
			textInviteLimitTime.text = val.ToString();
			while (val > 0)
			{
				yield return CoroutineDefine._1sec;
				val--;
				textInviteLimitTime.text = val.ToString();
			}
			objInviteLimit.SetActive(false);
			ocs.PlaySE("SystemSE", "sys_access01_stop");
			button.interactable = true;
		}
	}

	private void UpdateInviteInfo()
	{
		friendScrollRect.ClearCells();
		friendScrollRect.OrangeInit(friendUnit, 4, ListFriend.Count);
		canvasNoResultMsg.enabled = ListFriend.Count == 0;
	}

	private void UpdateRoomMember()
	{
		scrollRect.OrangeInit(roomMembers, MonoBehaviourSingleton<OrangeMatchManager>.Instance.ListMemberInfo.Count, MonoBehaviourSingleton<OrangeMatchManager>.Instance.ListMemberInfo.Count);
		if (IsHost)
		{
			startBtn.GetComponent<Button>().interactable = MonoBehaviourSingleton<OrangeMatchManager>.Instance.ListMemberInfo.Count == _maxRoomMember;
		}
	}

	private void ForceCloseUI()
	{
		_bLockControl = false;
		OnClickCloseBtn();
	}

	public override void OnClickCloseBtn()
	{
		if (!_bLockControl)
		{
			_bLockControl = true;
			localBroadcastBtn.GetComponent<OrangeCriSource>().StopAll();
			serverBroadcastBtn.GetComponent<OrangeCriSource>().StopAll();
			MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicChatLog[ChatChannel.TeamChannel].Clear();
			MonoBehaviourSingleton<CMSocketClient>.Instance.SendProtocol(FlatBufferCMHelper.CreateRQLeavePrepareRoom());
			base.OnClickCloseBtn();
		}
	}

	public void OnClickCopyRoomID()
	{
		if (!_bLockControl)
		{
			_bLockControl = true;
			GUIUtility.systemCopyBuffer = inviteIDText.text;
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_Tip", delegate(TipUI tipUI)
			{
				tipUI.alertSE = 18;
				tipUI.Setup(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COPY_ROOMID"), true);
				_bLockControl = false;
			});
		}
	}

	private void OnDestroy()
	{
		LeanTween.cancel(ref _timeoutCountDownTween, false);
		LeanTween.cancel(ref _timeoutContinueTween, false);
		LeanTween.cancel(ref _communityBroadcastTween, false);
		_bLockControl = false;
		RemoveAllHandler();
		OrangeMatchManager instance = MonoBehaviourSingleton<OrangeMatchManager>.Instance;
		instance.OnDisconnectEvent = (Action)Delegate.Remove(instance.OnDisconnectEvent, new Action(RemoveAllHandler));
	}

	public void OnClickBattleSetting()
	{
		if (_bLockControl)
		{
			return;
		}
		_bLockControl = true;
		if (!_bIsHost && IsPrepared)
		{
			_bLockControl = false;
			return;
		}
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_GoCheck", delegate(GoCheckUI ui)
		{
			ui.Setup(MonoBehaviourSingleton<OrangeMatchManager>.Instance.StageID);
			ui.EnableBackToHometop = false;
			ui.destroyCB = (Callback)Delegate.Combine(ui.destroyCB, new Callback(CheckSelfData));
			ui.closeCB = (Callback)Delegate.Combine(ui.closeCB, new Callback(CheckSelfData));
			ui.bJustReturnToLastUI = true;
			ui.bIsHaveRoom = true;
			_bLockControl = false;
		});
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK13);
	}

	private void OnNTJoinPrepareRoom(object res)
	{
		MonoBehaviourSingleton<OrangeMatchManager>.Instance.PvpMatchType = PVPMatchType.FriendOneVSOne;
		MonoBehaviourSingleton<OrangeMatchManager>.Instance.ListMemberInfo.Clear();
		StartRoomTimeoutCountdown();
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
					bool bIsHost = _bIsHost;
				}
				MemberInfo memberInfo = new MemberInfo(nTJoinPrepareRoom.Playerid(i), nTJoinPrepareRoom.NickName(i), 0, netSealBattleSettingInfo);
				if (nTJoinPrepareRoom.Playerid(i) != MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify && !_bIsHost)
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
		if (IsHost && MonoBehaviourSingleton<OrangeMatchManager>.Instance.ListMemberInfo.Count >= _maxRoomMember)
		{
			zoneToggle.gameObject.SetActive(false);
			if (_bIsContinue)
			{
				StopWaitingForGuest();
			}
		}
		if (IsHost)
		{
			IsPrepared = true;
			MonoBehaviourSingleton<CMSocketClient>.Instance.SendProtocol(FlatBufferCMHelper.CreateRQSetPreparedOrNot(IsPrepared));
		}
		startBtn.SetActive(IsHost);
		readyBtn.SetActive(!IsHost);
		UpdateRoomMember();
	}

	private void StopWaitingForGuest()
	{
		float delayTime = 1f;
		LeanTween.delayedCall(base.gameObject, delayTime, (Action)delegate
		{
			if (!(this == null) && _waitForConnection != null)
			{
				StartCoroutine(WaitToCloseDialog());
				LeanTween.cancel(ref _timeoutContinueTween, false);
			}
		});
	}

	private IEnumerator WaitToCloseDialog()
	{
		while (_waitForConnection.IsLock)
		{
			yield return CoroutineDefine._waitForEndOfFrame;
		}
		_waitForConnection.OnClickCloseBtn();
		_waitForConnection = null;
	}

	private void OnNTLeavePrepareRoom(object res)
	{
		StartRoomTimeoutCountdown();
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
			if (IsHost && MonoBehaviourSingleton<OrangeMatchManager>.Instance.ListMemberInfo.Count <= 1)
			{
				zoneToggle.gameObject.SetActive(true);
			}
			UpdateRoomMember();
		}
	}

	private void OnRSSetPreparedOrNot(object res)
	{
		if (res is RSSetPreparedOrNot)
		{
			int result = ((RSSetPreparedOrNot)res).Result;
			int num = 65000;
		}
		_bLockControl = false;
	}

	private void OnRSPVPPrepareRoomInfo(object res)
	{
		if (res is RSPVPPrepareRoomInfo)
		{
			RSPVPPrepareRoomInfo rSPVPPrepareRoomInfo = (RSPVPPrepareRoomInfo)res;
			if (rSPVPPrepareRoomInfo.Result == 60100)
			{
				inviteIDText.text = rSPVPPrepareRoomInfo.Invitecode;
			}
		}
	}

	private void OnNTCommunityBroadcastMessage(object res)
	{
		LeanTween.cancel(ref _communityBroadcastTween, false);
		_communityBroadcastTween = LeanTween.delayedCall(base.gameObject, 2f, (Action)delegate
		{
			if (this != null)
			{
				ClickFriendToggle(friendToggle.isOn);
				ClickGuildToggle(guildToggle.isOn);
				ClickZoneToggle(zoneToggle.isOn);
			}
		}).uniqueId;
	}

	private void OnNTPrepareRoomOwnerChange(object res)
	{
		StartRoomTimeoutCountdown();
		if (res is NTPrepareRoomOwnerChange)
		{
			_bIsHost = ((((NTPrepareRoomOwnerChange)res).Playerid == MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify) ? true : false);
			if (IsHost)
			{
				IsPrepared = true;
				MonoBehaviourSingleton<CMSocketClient>.Instance.SendProtocol(FlatBufferCMHelper.CreateRQSetPreparedOrNot(IsPrepared));
			}
			startBtn.SetActive(IsHost);
			readyBtn.SetActive(!IsHost);
			zoneToggle.gameObject.SetActive(IsHost);
		}
	}

	private void StartRoomTimeoutCountdown()
	{
		if (IsHost)
		{
			_debugTimeoutTimer = OrangeConst.PVP_FRIEND_READYTIME;
			LeanTween.cancel(ref _timeoutCountDownTween, false);
			_timeoutCountDownTween = LeanTween.delayedCall(base.gameObject, (float)OrangeConst.PVP_FRIEND_READYTIME, (Action)delegate
			{
				ForceCloseUI();
				MonoBehaviourSingleton<OrangeGameManager>.Instance.ShowConfirmMsg("PVP_FRIEND_READYOVER");
			}).uniqueId;
		}
	}

    [Obsolete]
    private void AddAllHandler()
	{
		if (!_assignHandler)
		{
			_assignHandler = true;
			OrangeMatchManager.NetCBData onRSSetPreparedOrNotCB = MonoBehaviourSingleton<OrangeMatchManager>.Instance.OnRSSetPreparedOrNotCB;
			onRSSetPreparedOrNotCB.tCB = (CallbackObj)Delegate.Combine(onRSSetPreparedOrNotCB.tCB, new CallbackObj(OnRSSetPreparedOrNot));
			OrangeMatchManager.NetCBData onNTJoinPrepareRoomCB = MonoBehaviourSingleton<OrangeMatchManager>.Instance.OnNTJoinPrepareRoomCB;
			onNTJoinPrepareRoomCB.tCB = (CallbackObj)Delegate.Combine(onNTJoinPrepareRoomCB.tCB, new CallbackObj(OnNTJoinPrepareRoom));
			OrangeMatchManager.NetCBData onNTLeavePrepareRoomCB = MonoBehaviourSingleton<OrangeMatchManager>.Instance.OnNTLeavePrepareRoomCB;
			onNTLeavePrepareRoomCB.tCB = (CallbackObj)Delegate.Combine(onNTLeavePrepareRoomCB.tCB, new CallbackObj(OnNTLeavePrepareRoom));
			OrangeMatchManager.NetCBData onNTPVPFriendRoomStartCB = MonoBehaviourSingleton<OrangeMatchManager>.Instance.OnNTPVPFriendRoomStartCB;
			onNTPVPFriendRoomStartCB.tCB = (CallbackObj)Delegate.Combine(onNTPVPFriendRoomStartCB.tCB, new CallbackObj(OnNTPVPFriendRoomStart));
			OrangeMatchManager.NetCBData onNTPrepareRoomOwnerChangeCB = MonoBehaviourSingleton<OrangeMatchManager>.Instance.OnNTPrepareRoomOwnerChangeCB;
			onNTPrepareRoomOwnerChangeCB.tCB = (CallbackObj)Delegate.Combine(onNTPrepareRoomOwnerChangeCB.tCB, new CallbackObj(OnNTPrepareRoomOwnerChange));
			OrangeMatchManager.NetCBData onRSChangePreparedSettingCB = MonoBehaviourSingleton<OrangeMatchManager>.Instance.OnRSChangePreparedSettingCB;
			onRSChangePreparedSettingCB.tCB = (CallbackObj)Delegate.Combine(onRSChangePreparedSettingCB.tCB, new CallbackObj(OnRSChangePreparedSetting));
			OrangeMatchManager.NetCBData onNTInviteCodeChangeCB = MonoBehaviourSingleton<OrangeMatchManager>.Instance.OnNTInviteCodeChangeCB;
			onNTInviteCodeChangeCB.tCB = (CallbackObj)Delegate.Combine(onNTInviteCodeChangeCB.tCB, new CallbackObj(OnNTInviteCodeChange));
			OrangeMatchManager.NetCBData onNTRoomOwnerCB = MonoBehaviourSingleton<OrangeMatchManager>.Instance.OnNTRoomOwnerCB;
			onNTRoomOwnerCB.tCB = (CallbackObj)Delegate.Combine(onNTRoomOwnerCB.tCB, new CallbackObj(OnNTRoomOwner));
			MonoBehaviourSingleton<SignalDispatcher>.Instance.AddHandler(CB.RSLeaveBattleRoom, OnRSLeaveBattleRoom);
			MonoBehaviourSingleton<SignalDispatcher>.Instance.AddHandler(CM.RSPVPPrepareRoomInfo, OnRSPVPPrepareRoomInfo);
			MonoBehaviourSingleton<SignalDispatcher>.Instance.AddHandler(CC.NTCommunityBroadcastMessage, OnNTCommunityBroadcastMessage);
			Singleton<GuildSystem>.Instance.OnGetMemberInfoListEvent += OnGetMemberInfoListEvent;
		}
	}

    [Obsolete]
    private void RemoveAllHandler()
	{
		if (_assignHandler)
		{
			_assignHandler = false;
			OrangeMatchManager.NetCBData onRSSetPreparedOrNotCB = MonoBehaviourSingleton<OrangeMatchManager>.Instance.OnRSSetPreparedOrNotCB;
			onRSSetPreparedOrNotCB.tCB = (CallbackObj)Delegate.Remove(onRSSetPreparedOrNotCB.tCB, new CallbackObj(OnRSSetPreparedOrNot));
			OrangeMatchManager.NetCBData onNTJoinPrepareRoomCB = MonoBehaviourSingleton<OrangeMatchManager>.Instance.OnNTJoinPrepareRoomCB;
			onNTJoinPrepareRoomCB.tCB = (CallbackObj)Delegate.Remove(onNTJoinPrepareRoomCB.tCB, new CallbackObj(OnNTJoinPrepareRoom));
			OrangeMatchManager.NetCBData onNTLeavePrepareRoomCB = MonoBehaviourSingleton<OrangeMatchManager>.Instance.OnNTLeavePrepareRoomCB;
			onNTLeavePrepareRoomCB.tCB = (CallbackObj)Delegate.Remove(onNTLeavePrepareRoomCB.tCB, new CallbackObj(OnNTLeavePrepareRoom));
			OrangeMatchManager.NetCBData onNTPVPFriendRoomStartCB = MonoBehaviourSingleton<OrangeMatchManager>.Instance.OnNTPVPFriendRoomStartCB;
			onNTPVPFriendRoomStartCB.tCB = (CallbackObj)Delegate.Remove(onNTPVPFriendRoomStartCB.tCB, new CallbackObj(OnNTPVPFriendRoomStart));
			OrangeMatchManager.NetCBData onNTPrepareRoomOwnerChangeCB = MonoBehaviourSingleton<OrangeMatchManager>.Instance.OnNTPrepareRoomOwnerChangeCB;
			onNTPrepareRoomOwnerChangeCB.tCB = (CallbackObj)Delegate.Remove(onNTPrepareRoomOwnerChangeCB.tCB, new CallbackObj(OnNTPrepareRoomOwnerChange));
			OrangeMatchManager.NetCBData onRSChangePreparedSettingCB = MonoBehaviourSingleton<OrangeMatchManager>.Instance.OnRSChangePreparedSettingCB;
			onRSChangePreparedSettingCB.tCB = (CallbackObj)Delegate.Remove(onRSChangePreparedSettingCB.tCB, new CallbackObj(OnRSChangePreparedSetting));
			OrangeMatchManager.NetCBData onNTInviteCodeChangeCB = MonoBehaviourSingleton<OrangeMatchManager>.Instance.OnNTInviteCodeChangeCB;
			onNTInviteCodeChangeCB.tCB = (CallbackObj)Delegate.Remove(onNTInviteCodeChangeCB.tCB, new CallbackObj(OnNTInviteCodeChange));
			OrangeMatchManager.NetCBData onNTRoomOwnerCB = MonoBehaviourSingleton<OrangeMatchManager>.Instance.OnNTRoomOwnerCB;
			onNTRoomOwnerCB.tCB = (CallbackObj)Delegate.Remove(onNTRoomOwnerCB.tCB, new CallbackObj(OnNTRoomOwner));
			MonoBehaviourSingleton<SignalDispatcher>.Instance.RemoveHandler(CB.RSLeaveBattleRoom, OnRSLeaveBattleRoom);
			MonoBehaviourSingleton<SignalDispatcher>.Instance.RemoveHandler(CM.RSPVPPrepareRoomInfo, OnRSPVPPrepareRoomInfo);
			MonoBehaviourSingleton<SignalDispatcher>.Instance.RemoveHandler(CC.NTCommunityBroadcastMessage, OnNTCommunityBroadcastMessage);
			Singleton<GuildSystem>.Instance.OnGetMemberInfoListEvent -= OnGetMemberInfoListEvent;
		}
	}

	private void OnNTPVPFriendRoomStart(object res)
	{
		Debug.Log("OnNTPVPFriendRoomStart received!");
		_bLockControl = true;
		if (res is NTPVPFriendRoomStart)
		{
			NTPVPFriendRoomStart rs = (NTPVPFriendRoomStart)res;
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK07);
			SaveHostIDToLocal();
			LeanTween.cancel(ref _timeoutCountDownTween, false);
			LeanTween.cancel(ref _CDTweenVoice, false);
			PVPStart(rs);
		}
	}

	private void PVPStart(NTPVPFriendRoomStart rs)
	{
		ManagedSingleton<StageHelper>.Instance.nStageEndGoUI = StageHelper.STAGE_END_GO.PVPROOMSELECT;
		MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.ReadyToGoPVP(0, PVPMatchType.FriendOneVSOne, rs.Ip, rs.Port, _roomID, delegate
		{
			Debug.Log("ReadyToGoPVP failed");
			ForceCloseUI();
		}, delegate
		{
			Debug.Log("ReadyToGoPVP successful");
		});
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

	private void OnRSLeaveBattleRoom(object res)
	{
		Debug.Log("Failed to start match, closing room...");
		MonoBehaviourSingleton<UIManager>.Instance.LoadResourceUI("UI_CommonMsg", delegate(CommonUI ui)
		{
			ui.effectTypeClose = UIManager.EffectType.EXPAND;
			ui.YesSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
			ui.SetupConfirm(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_TIP"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("NETWORK_NOT_REACHABLE_TITLE"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_OK"), delegate
			{
				ForceCloseUI();
			});
		});
	}

	private void OnNTInviteCodeChange(object res)
	{
		Debug.Log("Invite code change");
		if (res is NTInviteCodeChange)
		{
			NTInviteCodeChange nTInviteCodeChange = (NTInviteCodeChange)res;
			Debug.Log("Invite code = " + nTInviteCodeChange.Invitecode);
			_inviteCode = nTInviteCodeChange.Invitecode;
			if (inviteIDText != null)
			{
				inviteIDText.text = _inviteCode;
			}
		}
	}

	private void OnNTRoomOwner(object res)
	{
		Debug.Log("OnNTRoomOwner");
		_bIsHost = true;
		IsPrepared = _bIsHost;
		MonoBehaviourSingleton<CMSocketClient>.Instance.SendProtocol(FlatBufferCMHelper.CreateRQSetPreparedOrNot(IsPrepared));
		startBtn.SetActive(_bIsHost);
		readyBtn.SetActive(!_bIsHost);
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

	public void OnSendInviteZone()
	{
		PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_OK17);
		SendInviteToChannel(ChatChannel.ZoneChannel);
	}

	public void OnSendInviteCrossZone()
	{
		PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_OK17);
		SendInviteToChannel(ChatChannel.CrossZoneChannel);
	}

	private void SendInviteToChannel(ChatChannel channel)
	{
		if (channel == ChatChannel.ZoneChannel)
		{
			StartCoroutine(OnStartCountDown(localBroadcastBtn, localBroadcastBlock, OrangeConst.PVP_NUMBERSHOW_TIME));
			string message = "";
			if (_stageTable.n_MAIN == 90001 && _stageTable.n_SUB == 1)
			{
				message = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("PVP_FRIEND_NUMBERSHOW1"), _inviteCode);
			}
			else if (_stageTable.n_MAIN == 90001 && _stageTable.n_SUB == 2)
			{
				message = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("PVP_FRIEND_NUMBERSHOW2"), _inviteCode);
			}
			SendMessageToChannel(channel, message);
		}
		else
		{
			if (channel != ChatChannel.CrossZoneChannel)
			{
				return;
			}
			StartCoroutine(OnStartCountDown(serverBroadcastBtn, serverBroadcastBlock, OrangeConst.PVP_NUMBERSHOW_TIME));
			string inviteMsg = "";
			if (_stageTable.n_MAIN == 90001 && _stageTable.n_SUB == 1)
			{
				inviteMsg = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("PVP_FRIEND_NUMBERSHOW1"), _inviteCode);
			}
			else if (_stageTable.n_MAIN == 90001 && _stageTable.n_SUB == 2)
			{
				inviteMsg = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("PVP_FRIEND_NUMBERSHOW2"), _inviteCode);
			}
			if (ManagedSingleton<PlayerHelper>.Instance.GetItemValue(OrangeConst.ITEMID_CHANNEL_SERVER) > 0)
			{
				ManagedSingleton<PlayerNetManager>.Instance.RetrieveUseChatItemReq(3, delegate(RetrieveUseChatItemRes res)
				{
					SendMessageToChannel(channel, inviteMsg, res.Token);
				});
				return;
			}
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_Tip", delegate(TipUI tipUI)
			{
				string str = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("MATERIAL_NOT_ENOUGH");
				tipUI.Setup(str, true);
			});
		}
	}

	private void SendMessageToChannel(ChatChannel channel, string message, string token = "")
	{
		if (!MonoBehaviourSingleton<OrangeCommunityManager>.Instance.OnCheckCommunityServerConnected())
		{
			return;
		}
		if (IsReachable())
		{
			string message2 = JsonHelper.Serialize(new MessageStruct
			{
				NickName = ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.Nickname,
				RichText = message,
				EmotionIconID = -1,
				EmotionPkgID = -1
			});
			MonoBehaviourSingleton<CCSocketClient>.Instance.SendProtocol(FlatBufferCCHelper.CreateRQChatSendMessage((int)channel, MonoBehaviourSingleton<GameServerService>.Instance.ServiceZoneID, "", message2, token));
		}
		else
		{
			MonoBehaviourSingleton<UIManager>.Instance.LoadResourceUI("UI_CommonMsg", delegate(CommonUI ui)
			{
				ui.OpenSE = SystemSE.CRI_SYSTEMSE_SYS_ERROR;
				ui.SetupConfirm(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_TIP"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("NETWORK_NOT_REACHABLE_TITLE"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_OK"), delegate
				{
				});
			});
		}
	}

	private bool IsReachable()
	{
		return Application.internetReachability != NetworkReachability.NotReachable;
	}

	private void OnGetMemberInfoListEvent(Code ackCode)
	{
		if (ackCode != Code.GUILD_GET_GUILD_MEMBER_LIST_SUCCESS)
		{
			return;
		}
		List<SocketFriendInfo> list = new List<SocketFriendInfo>();
		int value = 0;
		foreach (NetMemberInfo item in Singleton<GuildSystem>.Instance.MemberInfoListCache)
		{
			if (item.MemberId != MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify)
			{
				SocketFriendInfo socketFriendInfo = new SocketFriendInfo();
				Singleton<GuildSystem>.Instance.PlayerBusyStatusCache.TryGetValue(item.MemberId, out value);
				socketFriendInfo.FriendPlayerID = item.MemberId;
				socketFriendInfo.Busy = value;
				list.Add(socketFriendInfo);
			}
		}
		ListFriend = SortFriendList(list);
		UpdateInviteInfo();
	}

	private void OnGetCheckGuildStateEvent()
	{
		if (Singleton<GuildSystem>.Instance.HasGuild)
		{
			Singleton<GuildSystem>.Instance.ReqGetMemberInfoList();
		}
	}
}
