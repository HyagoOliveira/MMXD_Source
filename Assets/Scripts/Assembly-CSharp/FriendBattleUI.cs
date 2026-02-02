#define RELEASE
using System;
using System.Linq;
using CallbackDefs;
using OrangeAudio;
using OrangeSocket;
using UnityEngine;
using UnityEngine.UI;
using cb;
using cm;
using enums;

public class FriendBattleUI : OrangeUIBase
{
	public enum RoleType
	{
		HOST = 0,
		GUEST = 1,
		MAX = 2
	}

	[SerializeField]
	private GameObject refCommonIconBase;

	[SerializeField]
	private GameObject refPlayerIconBase;

	[SerializeField]
	private Transform roomUIRoot;

	[SerializeField]
	private Transform InputUIRoot;

	[SerializeField]
	private Transform InputInviteIDRoot;

	[SerializeField]
	private Transform playerInfoRoot;

	[SerializeField]
	private Transform roomIDDisplayRoot;

	[SerializeField]
	private OrangeText hostName;

	[SerializeField]
	private Transform guestInfoRoot;

	[SerializeField]
	private OrangeText guestName;

	[SerializeField]
	private Image yellowCircle;

	[SerializeField]
	private Transform hostIcon;

	[SerializeField]
	private Transform guestIcon;

	[SerializeField]
	private OrangeText roomIDDisplayText;

	[SerializeField]
	private Text roomIDInputText;

	[SerializeField]
	private Text inviteIDInputText;

	[SerializeField]
	private Button executeBtn;

	[SerializeField]
	private Transform hostIconGrid;

	[SerializeField]
	private Transform guestIconGrid;

	private bool bAccessSEPlay;

	private PlayerIconBase _hostPlayerIcon;

	private CommonIconBase _hostMainWeaponIcon;

	private CommonIconBase _hostSubWeaponIcon;

	private CommonIconBase _hostCharacterIcon;

	private PlayerIconBase _guestPlayerIcon;

	private CommonIconBase _guestMainWeaponIcon;

	private CommonIconBase _guestSubWeaponIcon;

	private CommonIconBase _guestCharacterIcon;

	private string _roomID;

	private bool _bIsSelfHost = true;

	private bool _assignHandler;

	private int _friendpvpCountDownTween;

	private int _CDTweenVoice;

	private bool _bLockControl;

	private bool _bRoomStarted;

	private bool bJoinBtn;

	public void Setup(RoleType role)
	{
		_hostCharacterIcon = UnityEngine.Object.Instantiate(refCommonIconBase, hostIconGrid).GetComponent<CommonIconBase>();
		_hostMainWeaponIcon = UnityEngine.Object.Instantiate(refCommonIconBase, hostIconGrid).GetComponent<CommonIconBase>();
		_hostSubWeaponIcon = UnityEngine.Object.Instantiate(refCommonIconBase, hostIconGrid).GetComponent<CommonIconBase>();
		_guestCharacterIcon = UnityEngine.Object.Instantiate(refCommonIconBase, guestIconGrid).GetComponent<CommonIconBase>();
		_guestMainWeaponIcon = UnityEngine.Object.Instantiate(refCommonIconBase, guestIconGrid).GetComponent<CommonIconBase>();
		_guestSubWeaponIcon = UnityEngine.Object.Instantiate(refCommonIconBase, guestIconGrid).GetComponent<CommonIconBase>();
		_hostPlayerIcon = UnityEngine.Object.Instantiate(refPlayerIconBase, hostIcon).GetComponent<PlayerIconBase>();
		_guestPlayerIcon = UnityEngine.Object.Instantiate(refPlayerIconBase, guestIcon).GetComponent<PlayerIconBase>();
		_hostPlayerIcon.transform.localScale = new Vector3(0.8f, 0.8f);
		_guestPlayerIcon.transform.localScale = new Vector3(0.8f, 0.8f);
		executeBtn.interactable = false;
		MonoBehaviourSingleton<OrangeMatchManager>.Instance.SelectStageData = ManagedSingleton<OrangeDataManager>.Instance.STAGE_TABLE_DICT[MonoBehaviourSingleton<OrangeMatchManager>.Instance.StageID];
		switch (role)
		{
		case RoleType.HOST:
		{
			PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_ACCESS01_LP);
			bAccessSEPlay = true;
			_bIsSelfHost = true;
			roomUIRoot.gameObject.SetActive(true);
			InputUIRoot.gameObject.SetActive(false);
			playerInfoRoot.gameObject.SetActive(true);
			roomIDDisplayRoot.gameObject.SetActive(true);
			guestInfoRoot.gameObject.SetActive(false);
			roomIDDisplayText.text = "---";
			CHARACTER_TABLE cHARACTER_TABLE2 = ManagedSingleton<OrangeDataManager>.Instance.CHARACTER_TABLE_DICT[ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.StandbyChara];
			CharacterInfo characterInfo2 = ManagedSingleton<PlayerNetManager>.Instance.dicCharacter[ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.StandbyChara];
			string text2 = ((characterInfo2.netInfo.Skin > 0) ? ManagedSingleton<OrangeDataManager>.Instance.SKIN_TABLE_DICT[characterInfo2.netInfo.Skin].s_ICON : cHARACTER_TABLE2.s_ICON);
			_hostCharacterIcon.Setup(0, AssetBundleScriptableObject.Instance.GetIconCharacter("icon_" + text2), "icon_" + text2);
			_hostCharacterIcon.SetOtherInfo(characterInfo2.netInfo);
			WeaponInfo value3;
			ManagedSingleton<PlayerNetManager>.Instance.dicWeapon.TryGetValue(ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.MainWeaponID, out value3);
			WeaponInfo value4;
			ManagedSingleton<PlayerNetManager>.Instance.dicWeapon.TryGetValue(ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.SubWeaponID, out value4);
			SetWeaponIcon(_hostMainWeaponIcon, (value3 == null) ? null : value3.netInfo, CommonIconBase.WeaponEquipType.Main);
			SetWeaponIcon(_hostSubWeaponIcon, (value4 == null) ? null : value4.netInfo, CommonIconBase.WeaponEquipType.Sub);
			hostName.text = ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.Nickname;
			_hostPlayerIcon.Setup(ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.PortraitID);
			CreateRoom();
			roomIDDisplayText.transform.parent.gameObject.SetActive(true);
			executeBtn.gameObject.SetActive(true);
			break;
		}
		case RoleType.GUEST:
		{
			PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
			_bIsSelfHost = false;
			roomUIRoot.gameObject.SetActive(false);
			InputUIRoot.gameObject.SetActive(true);
			playerInfoRoot.gameObject.SetActive(false);
			roomIDDisplayRoot.gameObject.SetActive(false);
			CHARACTER_TABLE cHARACTER_TABLE = ManagedSingleton<OrangeDataManager>.Instance.CHARACTER_TABLE_DICT[ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.StandbyChara];
			CharacterInfo characterInfo = ManagedSingleton<PlayerNetManager>.Instance.dicCharacter[ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.StandbyChara];
			string text = ((characterInfo.netInfo.Skin > 0) ? ManagedSingleton<OrangeDataManager>.Instance.SKIN_TABLE_DICT[characterInfo.netInfo.Skin].s_ICON : cHARACTER_TABLE.s_ICON);
			_guestCharacterIcon.Setup(0, AssetBundleScriptableObject.Instance.GetIconCharacter("icon_" + text), "icon_" + text);
			_guestCharacterIcon.SetOtherInfo(characterInfo.netInfo);
			WeaponInfo value;
			ManagedSingleton<PlayerNetManager>.Instance.dicWeapon.TryGetValue(ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.MainWeaponID, out value);
			WeaponInfo value2;
			ManagedSingleton<PlayerNetManager>.Instance.dicWeapon.TryGetValue(ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.SubWeaponID, out value2);
			SetWeaponIcon(_guestMainWeaponIcon, (value == null) ? null : value.netInfo, CommonIconBase.WeaponEquipType.Main);
			SetWeaponIcon(_guestSubWeaponIcon, (value2 == null) ? null : value2.netInfo, CommonIconBase.WeaponEquipType.Sub);
			guestName.text = ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.Nickname;
			_guestPlayerIcon.Setup(ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.PortraitID);
			roomIDDisplayText.transform.parent.gameObject.SetActive(false);
			executeBtn.gameObject.SetActive(false);
			InputUIRoot.gameObject.SetActive(false);
			InputInviteIDRoot.gameObject.SetActive(true);
			break;
		}
		}
		AddAllHandler();
		base._EscapeEvent = EscapeEvent.CLOSE_UI;
	}

	private void CreateRoom()
	{
		MonoBehaviourSingleton<OrangeMatchManager>.Instance.Host = ManagedSingleton<ServerConfig>.Instance.ServerSetting.Match.Host;
		MonoBehaviourSingleton<OrangeMatchManager>.Instance.Port = ManagedSingleton<ServerConfig>.Instance.ServerSetting.Match.Port;
		MonoBehaviourSingleton<OrangeMatchManager>.Instance.PvpGameType = PVPGameType.OneVSOneBattle;
		MonoBehaviourSingleton<OrangeMatchManager>.Instance.PvpMatchType = PVPMatchType.FriendOneVSOne;
		MonoBehaviourSingleton<OrangeMatchManager>.Instance.MatchServerLogin(delegate
		{
			MonoBehaviourSingleton<OrangeMatchManager>.Instance.CreatePVPPrepareRoom(MonoBehaviourSingleton<OrangeMatchManager>.Instance.PvpMatchType, MonoBehaviourSingleton<OrangeMatchManager>.Instance.StageID, 0, true, "PVP Room 1.0", false, OnRSCreatePVPPrepareRoom);
		});
	}

	private void OnRSCreatePVPPrepareRoom(object res)
	{
		if (!(res is RSCreatePVPPrepareRoom))
		{
			return;
		}
		RSCreatePVPPrepareRoom rSCreatePVPPrepareRoom = (RSCreatePVPPrepareRoom)res;
		if (rSCreatePVPPrepareRoom.Result != 61000)
		{
			MonoBehaviourSingleton<OrangeGameManager>.Instance.ShowErrorMsg((Code)rSCreatePVPPrepareRoom.Result, false);
			return;
		}
		NetSealBattleSettingInfo netSealBattleSettingInfo = null;
		if (!ManagedSingleton<PlayerHelper>.Instance.ParserUnsealedBattleSetting(rSCreatePVPPrepareRoom.Unsealedbattlesetting, out netSealBattleSettingInfo))
		{
			MonoBehaviourSingleton<OrangeGameManager>.Instance.ShowErrorMsg(Code.MATCH_CREATEROOM_FAIL, false);
			return;
		}
		MonoBehaviourSingleton<OrangeMatchManager>.Instance.Host = rSCreatePVPPrepareRoom.Ip;
		MonoBehaviourSingleton<OrangeMatchManager>.Instance.Port = rSCreatePVPPrepareRoom.Port;
		_roomID = rSCreatePVPPrepareRoom.Roomid;
		int num = rSCreatePVPPrepareRoom.Roomid.IndexOf("PVP[");
		roomIDDisplayText.text = rSCreatePVPPrepareRoom.Roomid.Substring(num + 4, 12);
		MonoBehaviourSingleton<OrangeMatchManager>.Instance.SingleMatch = false;
	}

	private void CloseUI()
	{
		_bLockControl = false;
		OnClickCloseBtn();
	}

	public override void OnClickCloseBtn()
	{
		if (!_bLockControl)
		{
			_bLockControl = true;
			if (bAccessSEPlay)
			{
				PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_ACCESS01_STOP);
				bAccessSEPlay = false;
			}
			MonoBehaviourSingleton<CMSocketClient>.Instance.SendProtocol(FlatBufferCMHelper.CreateRQLeavePrepareRoom());
			PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL);
			base.OnClickCloseBtn();
		}
	}

	public void OnClickOKBtn()
	{
		if (MonoBehaviourSingleton<OrangeMatchManager>.Instance.ListMemberInfo.Count != 2 || _bLockControl)
		{
			return;
		}
		_bLockControl = true;
		executeBtn.interactable = false;
		MonoBehaviourSingleton<SignalDispatcher>.Instance.AddHandler(CM.RSPVPFriendRoomStart, delegate(object res)
		{
			if (res is RSPVPFriendRoomStart)
			{
				RSPVPFriendRoomStart rSPVPFriendRoomStart = (RSPVPFriendRoomStart)res;
				if (rSPVPFriendRoomStart.Result != 66000 && rSPVPFriendRoomStart.Result == 66053)
				{
					MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_Tip", delegate(TipUI ui)
					{
						ui.Setup(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("ROOM_MEMBER_READY"), true);
					});
				}
			}
		}, 0, true);
		MonoBehaviourSingleton<CMSocketClient>.Instance.SendProtocol(FlatBufferCMHelper.CreateRQPVPFriendRoomStart(0, 4, MonoBehaviourSingleton<OrangeMatchManager>.Instance.StageID));
	}

	public void OnClickJoinBtn()
	{
		if (!_bLockControl)
		{
			_bLockControl = true;
			bJoinBtn = true;
			MonoBehaviourSingleton<CMSocketClient>.Instance.SendProtocol(FlatBufferCMHelper.CreateRQPVPInviteCodeMatch(0, 4, MonoBehaviourSingleton<OrangeMatchManager>.Instance.StageID, inviteIDInputText.text.ToUpper()));
		}
	}

	private void OnRSPVPPrepareRoomInfo(object res)
	{
		_bLockControl = false;
		if (res is RSPVPPrepareRoomInfo)
		{
			RSPVPPrepareRoomInfo rSPVPPrepareRoomInfo = (RSPVPPrepareRoomInfo)res;
			if (rSPVPPrepareRoomInfo.Result != 60100)
			{
				Debug.Log("Get friend PVP room info failed.");
				MonoBehaviourSingleton<OrangeGameManager>.Instance.ShowConfirmMsg("PVP_FRIEND_ROOMERROR");
				return;
			}
			MonoBehaviourSingleton<OrangeMatchManager>.Instance.PvpMatchType = (PVPMatchType)rSPVPPrepareRoomInfo.Pvptype;
			Debug.Log("OnRSPVPPrepareRoomInfo received, IP = " + rSPVPPrepareRoomInfo.Ip + ", port = " + rSPVPPrepareRoomInfo.Port + ", Pvptype = " + rSPVPPrepareRoomInfo.Pvptype);
			MonoBehaviourSingleton<OrangeMatchManager>.Instance.JoinRoomFriendBattle(rSPVPPrepareRoomInfo.Ip, rSPVPPrepareRoomInfo.Port, _roomID, rSPVPPrepareRoomInfo.Capacity);
		}
	}

	public void OnClickCopyRoomID()
	{
		if (!_bLockControl)
		{
			_bLockControl = true;
			GUIUtility.systemCopyBuffer = roomIDDisplayText.text;
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_Tip", delegate(TipUI tipUI)
			{
				tipUI.alertSE = 18;
				tipUI.Setup(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COPY_ROOMID"), true);
				_bLockControl = false;
			});
		}
	}

    [Obsolete]
    private void OnRSJoinPrepareRoom(object res)
	{
		_bLockControl = false;
		if (!(res is RSJoinPrepareRoom))
		{
			return;
		}
		OrangeMatchManager.NetCBData onNTJoinPrepareRoomCB = MonoBehaviourSingleton<OrangeMatchManager>.Instance.OnNTJoinPrepareRoomCB;
		onNTJoinPrepareRoomCB.tCB = (CallbackObj)Delegate.Remove(onNTJoinPrepareRoomCB.tCB, new CallbackObj(OnNTJoinPrepareRoom));
		RSJoinPrepareRoom rSJoinPrepareRoom = (RSJoinPrepareRoom)res;
		if (rSJoinPrepareRoom.Result != 62000)
		{
			MonoBehaviourSingleton<OrangeGameManager>.Instance.ShowConfirmMsg("ROOM_CANNOT_JOIN");
			base.CloseSE = SystemSE.NONE;
			OnClickCloseBtn();
			return;
		}
		MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.bCurrentCoopChallengeMode = rSJoinPrepareRoom.Ischallenge;
		MonoBehaviourSingleton<OrangeMatchManager>.Instance.SingleMatch = false;
		MonoBehaviourSingleton<UIManager>.Instance.OpenLoadingUI(delegate
		{
			MonoBehaviourSingleton<UIManager>.Instance.BackToHometop(true, false, delegate
			{
				MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_FriendPVPRoomMain", delegate(FriendPVPRoomMain ui)
				{
					MonoBehaviourSingleton<UIManager>.Instance.CloseLoadingUI(null, 0.3f);
					ui.Setup(false, _roomID, inviteIDInputText.text, MonoBehaviourSingleton<OrangeMatchManager>.Instance.SelectStageData);
				});
			});
		}, OrangeSceneManager.LoadingType.WHITE);
	}

	private void OnNTJoinPrepareRoom(object res)
	{
		_bLockControl = false;
		MonoBehaviourSingleton<OrangeMatchManager>.Instance.ListMemberInfo.Clear();
		executeBtn.gameObject.SetActive(true);
		executeBtn.interactable = _bIsSelfHost;
		OrangeText componentInChildren = executeBtn.GetComponentInChildren<OrangeText>();
		if ((bool)componentInChildren)
		{
			componentInChildren.text = (_bIsSelfHost ? MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("FUNCTION_STARTER") : MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("PVP_FRIEND_READY"));
		}
		if (_bIsSelfHost)
		{
			LeanTween.cancel(ref _friendpvpCountDownTween, false);
			_friendpvpCountDownTween = LeanTween.delayedCall((float)OrangeConst.PVP_FRIEND_READYTIME, (Action)delegate
			{
				CloseUI();
				MonoBehaviourSingleton<OrangeGameManager>.Instance.ShowConfirmMsg("PVP_FRIEND_READYOVER");
			}).uniqueId;
		}
		if (res is NTJoinPrepareRoom)
		{
			NTJoinPrepareRoom nTJoinPrepareRoom = (NTJoinPrepareRoom)res;
			int unsealedbattlesettingLength = nTJoinPrepareRoom.UnsealedbattlesettingLength;
			for (int i = 0; i < unsealedbattlesettingLength; i++)
			{
				NetSealBattleSettingInfo setting = null;
				if (ManagedSingleton<PlayerHelper>.Instance.ParserUnsealedBattleSetting(nTJoinPrepareRoom.Unsealedbattlesetting(i), out setting))
				{
					MonoBehaviourSingleton<OrangeMatchManager>.Instance.ListMemberInfo.Add(new MemberInfo(nTJoinPrepareRoom.Playerid(i), nTJoinPrepareRoom.NickName(i), 0, setting));
					if (nTJoinPrepareRoom.Playerid(i) != MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify)
					{
						if (!bJoinBtn)
						{
							PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_CURSOR05);
						}
						LeanTween.cancel(ref _CDTweenVoice, false);
						_CDTweenVoice = LeanTween.delayedCall(0.8f, (Action)delegate
						{
							PlayCharVoice(setting);
						}).uniqueId;
						UpdateRoomMember(setting, !_bIsSelfHost);
					}
					else
					{
						UpdateRoomMember(setting, _bIsSelfHost);
					}
					continue;
				}
				CloseUI();
				break;
			}
		}
		bJoinBtn = false;
	}

	private void OnNTLeavePrepareRoom(object res)
	{
		if (!(res is NTLeavePrepareRoom))
		{
			return;
		}
		NTLeavePrepareRoom rs = (NTLeavePrepareRoom)res;
		Debug.Log("Leave:" + rs.Playerid);
		if (_bRoomStarted)
		{
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_Tip", delegate(TipUI ui)
			{
				ui.Setup(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("PVP_FRIEND_RETRUN"), true);
			});
			return;
		}
		MemberInfo memberInfo = null;
		memberInfo = MonoBehaviourSingleton<OrangeMatchManager>.Instance.ListMemberInfo.First((MemberInfo x) => x.PlayerId == rs.Playerid);
		if (memberInfo == null)
		{
			return;
		}
		MonoBehaviourSingleton<OrangeMatchManager>.Instance.ListMemberInfo.Remove(memberInfo);
		PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_ACCESS01_LP);
		bAccessSEPlay = true;
		guestInfoRoot.gameObject.SetActive(false);
		roomIDDisplayText.transform.parent.gameObject.SetActive(true);
		if (_bIsSelfHost)
		{
			executeBtn.interactable = false;
			return;
		}
		MonoBehaviourSingleton<UIManager>.Instance.LoadResourceUI("UI_CommonMsg", delegate(CommonUI ui)
		{
			ui.effectTypeClose = UIManager.EffectType.EXPAND;
			ui.YesSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
			ui.SetupConfirm(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_TIP"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("PVP_FRIEND_ROOMCLOSE"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_OK"), delegate
			{
				CloseUI();
			});
		});
	}

	private void OnNTPVPFriendRoomStart(object res)
	{
		Debug.Log("OnNTPVPFriendRoomStart received!");
		_bRoomStarted = true;
		_bLockControl = true;
		if (res is NTPVPFriendRoomStart)
		{
			NTPVPFriendRoomStart rs = (NTPVPFriendRoomStart)res;
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK07);
			LeanTween.cancel(ref _CDTweenVoice, false);
			_CDTweenVoice = LeanTween.delayedCall(3f, (Action)delegate
			{
				PVPStart(rs);
			}).uniqueId;
		}
	}

	private void OnRSPVPInviteCodeMatch(object res)
	{
		Debug.Log("OnRSPVPInviteCodeMatch received!");
		if (res is RSPVPInviteCodeMatch)
		{
			RSPVPInviteCodeMatch rSPVPInviteCodeMatch = (RSPVPInviteCodeMatch)res;
			Debug.Log("RoomID = " + rSPVPInviteCodeMatch.Roomid + ", result = " + rSPVPInviteCodeMatch.Result);
			if (!string.IsNullOrEmpty(rSPVPInviteCodeMatch.Roomid))
			{
				_roomID = rSPVPInviteCodeMatch.Roomid;
				MonoBehaviourSingleton<CMSocketClient>.Instance.SendProtocol(FlatBufferCMHelper.CreateRQPVPPrepareRoomInfo(_roomID));
			}
			else
			{
				Debug.Log("OnRSPVPInviteCodeMatch failed: " + rSPVPInviteCodeMatch.Result);
				MonoBehaviourSingleton<OrangeGameManager>.Instance.ShowConfirmMsg("ROOM_UNAVAILABLE");
				_bLockControl = false;
			}
		}
	}

	private void PVPStart(NTPVPFriendRoomStart rs)
	{
		ManagedSingleton<StageHelper>.Instance.nStageEndGoUI = StageHelper.STAGE_END_GO.PVPROOMSELECT;
		MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.ReadyToGoPVP(0, PVPMatchType.FriendOneVSOne, rs.Ip, rs.Port, _roomID, delegate
		{
			Debug.Log("ReadyToGoPVP failed");
			CloseUI();
		}, delegate
		{
			Debug.Log("ReadyToGoPVP successful");
		});
	}

	private void UpdateRoomMember(NetSealBattleSettingInfo battleInfo, bool bIsHost)
	{
		playerInfoRoot.gameObject.SetActive(true);
		if (bAccessSEPlay)
		{
			PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_ACCESS01_STOP);
			bAccessSEPlay = false;
		}
		guestInfoRoot.gameObject.SetActive(true);
		roomIDDisplayText.transform.parent.gameObject.SetActive(false);
		CHARACTER_TABLE cHARACTER_TABLE = ManagedSingleton<OrangeDataManager>.Instance.CHARACTER_TABLE_DICT[battleInfo.CharacterList[0].CharacterID];
		string text = ((battleInfo.CharacterList[0].Skin > 0) ? ManagedSingleton<OrangeDataManager>.Instance.SKIN_TABLE_DICT[battleInfo.CharacterList[0].Skin].s_ICON : cHARACTER_TABLE.s_ICON);
		if (bIsHost)
		{
			_hostCharacterIcon.Setup(0, AssetBundleScriptableObject.Instance.GetIconCharacter("icon_" + text), "icon_" + text);
			_hostCharacterIcon.SetOtherInfo(battleInfo.CharacterList[0]);
			SetWeaponIcon(_hostMainWeaponIcon, battleInfo.MainWeaponInfo, CommonIconBase.WeaponEquipType.Main);
			SetWeaponIcon(_hostSubWeaponIcon, battleInfo.SubWeaponInfo, CommonIconBase.WeaponEquipType.Sub);
			hostName.text = battleInfo.PlayerInfo.Nickname;
			_hostPlayerIcon.Setup(battleInfo.PlayerInfo.PortraitID);
		}
		else
		{
			_guestCharacterIcon.Setup(0, AssetBundleScriptableObject.Instance.GetIconCharacter("icon_" + text), "icon_" + text);
			_guestCharacterIcon.SetOtherInfo(battleInfo.CharacterList[0]);
			SetWeaponIcon(_guestMainWeaponIcon, battleInfo.MainWeaponInfo, CommonIconBase.WeaponEquipType.Main);
			SetWeaponIcon(_guestSubWeaponIcon, battleInfo.SubWeaponInfo, CommonIconBase.WeaponEquipType.Sub);
			guestName.text = battleInfo.PlayerInfo.Nickname;
			_guestPlayerIcon.Setup(battleInfo.PlayerInfo.PortraitID);
		}
	}

	private void PlayCharVoice(NetSealBattleSettingInfo setting)
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

	private void SetWeaponIcon(CommonIconBase icon, NetWeaponInfo netWeaponInfo, CommonIconBase.WeaponEquipType type)
	{
		WEAPON_TABLE value = null;
		icon.gameObject.SetActive(true);
		if (netWeaponInfo == null || netWeaponInfo.WeaponID == 0 || !ManagedSingleton<OrangeDataManager>.Instance.WEAPON_TABLE_DICT.TryGetValue(netWeaponInfo.WeaponID, out value))
		{
			icon.Setup(0, "", "");
			icon.SetOtherInfo(null, type);
			return;
		}
		if (type == CommonIconBase.WeaponEquipType.Main)
		{
			icon.Setup(0, AssetBundleScriptableObject.Instance.m_iconWeapon, value.s_ICON);
		}
		else
		{
			icon.Setup(0, AssetBundleScriptableObject.Instance.m_iconWeapon, value.s_ICON);
		}
		icon.SetOtherInfoWeaponFB(netWeaponInfo, type);
	}

	private void OnDestroy()
	{
		LeanTween.cancel(ref _friendpvpCountDownTween, false);
		LeanTween.cancel(ref _CDTweenVoice, false);
		_bLockControl = false;
		RemoveAllHandler();
		OrangeMatchManager instance = MonoBehaviourSingleton<OrangeMatchManager>.Instance;
		instance.OnDisconnectEvent = (Action)Delegate.Remove(instance.OnDisconnectEvent, new Action(RemoveAllHandler));
	}

    [Obsolete]
    private void AddAllHandler()
	{
		if (!_assignHandler)
		{
			_assignHandler = true;
			OrangeMatchManager.NetCBData onNTJoinPrepareRoomCB = MonoBehaviourSingleton<OrangeMatchManager>.Instance.OnNTJoinPrepareRoomCB;
			onNTJoinPrepareRoomCB.tCB = (CallbackObj)Delegate.Combine(onNTJoinPrepareRoomCB.tCB, new CallbackObj(OnNTJoinPrepareRoom));
			OrangeMatchManager.NetCBData onNTLeavePrepareRoomCB = MonoBehaviourSingleton<OrangeMatchManager>.Instance.OnNTLeavePrepareRoomCB;
			onNTLeavePrepareRoomCB.tCB = (CallbackObj)Delegate.Combine(onNTLeavePrepareRoomCB.tCB, new CallbackObj(OnNTLeavePrepareRoom));
			OrangeMatchManager.NetCBData onNTPVPFriendRoomStartCB = MonoBehaviourSingleton<OrangeMatchManager>.Instance.OnNTPVPFriendRoomStartCB;
			onNTPVPFriendRoomStartCB.tCB = (CallbackObj)Delegate.Combine(onNTPVPFriendRoomStartCB.tCB, new CallbackObj(OnNTPVPFriendRoomStart));
			OrangeMatchManager.NetCBData onRSPVPInviteCodeMatchCB = MonoBehaviourSingleton<OrangeMatchManager>.Instance.OnRSPVPInviteCodeMatchCB;
			onRSPVPInviteCodeMatchCB.tCB = (CallbackObj)Delegate.Combine(onRSPVPInviteCodeMatchCB.tCB, new CallbackObj(OnRSPVPInviteCodeMatch));
			MonoBehaviourSingleton<SignalDispatcher>.Instance.AddHandler(CB.RSLeaveBattleRoom, OnRSLeaveBattleRoom);
			MonoBehaviourSingleton<SignalDispatcher>.Instance.AddHandler(CM.RSPVPPrepareRoomInfo, OnRSPVPPrepareRoomInfo, 0, true);
			MonoBehaviourSingleton<SignalDispatcher>.Instance.AddHandler(CM.RSJoinPrepareRoom, OnRSJoinPrepareRoom);
		}
	}

    [Obsolete]
    private void RemoveAllHandler()
	{
		if (_assignHandler)
		{
			_assignHandler = false;
			OrangeMatchManager.NetCBData onNTJoinPrepareRoomCB = MonoBehaviourSingleton<OrangeMatchManager>.Instance.OnNTJoinPrepareRoomCB;
			onNTJoinPrepareRoomCB.tCB = (CallbackObj)Delegate.Remove(onNTJoinPrepareRoomCB.tCB, new CallbackObj(OnNTJoinPrepareRoom));
			OrangeMatchManager.NetCBData onNTLeavePrepareRoomCB = MonoBehaviourSingleton<OrangeMatchManager>.Instance.OnNTLeavePrepareRoomCB;
			onNTLeavePrepareRoomCB.tCB = (CallbackObj)Delegate.Remove(onNTLeavePrepareRoomCB.tCB, new CallbackObj(OnNTLeavePrepareRoom));
			OrangeMatchManager.NetCBData onNTPVPFriendRoomStartCB = MonoBehaviourSingleton<OrangeMatchManager>.Instance.OnNTPVPFriendRoomStartCB;
			onNTPVPFriendRoomStartCB.tCB = (CallbackObj)Delegate.Remove(onNTPVPFriendRoomStartCB.tCB, new CallbackObj(OnNTPVPFriendRoomStart));
			OrangeMatchManager.NetCBData onRSPVPInviteCodeMatchCB = MonoBehaviourSingleton<OrangeMatchManager>.Instance.OnRSPVPInviteCodeMatchCB;
			onRSPVPInviteCodeMatchCB.tCB = (CallbackObj)Delegate.Remove(onRSPVPInviteCodeMatchCB.tCB, new CallbackObj(OnRSPVPInviteCodeMatch));
			MonoBehaviourSingleton<SignalDispatcher>.Instance.RemoveHandler(CB.RSLeaveBattleRoom, OnRSLeaveBattleRoom);
			MonoBehaviourSingleton<SignalDispatcher>.Instance.RemoveHandler(CM.RSPVPPrepareRoomInfo, OnRSPVPPrepareRoomInfo);
			MonoBehaviourSingleton<SignalDispatcher>.Instance.RemoveHandler(CM.RSJoinPrepareRoom, OnRSJoinPrepareRoom);
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
				CloseUI();
			});
		});
	}
}
