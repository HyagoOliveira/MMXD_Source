#define RELEASE
using System;
using System.Collections;
using OrangeSocket;
using UnityEngine;
using cm;
using enums;

public class FriendPVPHelper : MonoBehaviourSingleton<FriendPVPHelper>
{
	private string _roomID;

	private float _exitTime;

	private CommonUI _connectingDialog;

	private int _roomFindDelayTween;

	private void ResetFindRoomTimer()
	{
		Debug.Log("ResetFindRoomTimer");
		_exitTime = Time.time + (float)OrangeConst.PVP_RETURN_TIME;
	}

	public void StartReconnectingToHost()
	{
		Debug.Log("StartReconnectingToHost");
		ResetFindRoomTimer();
		if (_connectingDialog != null)
		{
			Debug.Log("_connectingDialog not null, trying to close now...");
			_connectingDialog.OnClickCloseBtn();
		}
		MonoBehaviourSingleton<UIManager>.Instance.LoadResourceUI("UI_CommonMsg", delegate(CommonUI ui)
		{
			_connectingDialog = ui;
			ui.SetupConfirm(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_TIP"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("PVP_FRIEND_PLAYERBACK"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_CANCEL"), delegate
			{
				MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK17);
				CancelFindRoom();
				_connectingDialog = null;
			});
			MonoBehaviourSingleton<OrangeMatchManager>.Instance.PVPContinueRoomFind(MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.Setting.FriendPVPHostID, OnRSPVPContinueRoomFind);
		});
	}

	private void StopReconnectingToHost()
	{
		Debug.Log("StopReconnectingToHost");
		StartCoroutine(WaitToCloseDialog());
	}

	private IEnumerator WaitToCloseDialog()
	{
		Debug.Log("WaitToCloseDialog");
		while (_connectingDialog != null && _connectingDialog.IsLock)
		{
			Debug.Log("WaitToCloseDialog yield");
			yield return CoroutineDefine._waitForEndOfFrame;
		}
		_connectingDialog.OnClickCloseBtn();
		_connectingDialog = null;
	}

	public void CreatePrivateRoomAndWaitForGuest()
	{
		string roomName = FriendPVPCreateRoom.CreateDefaultRoomName();
		MonoBehaviourSingleton<OrangeMatchManager>.Instance.CreatePVPPrepareRoom(MonoBehaviourSingleton<OrangeMatchManager>.Instance.PvpMatchType, MonoBehaviourSingleton<OrangeMatchManager>.Instance.StageID, 0, false, roomName, true, OnRSCreatePVPPrepareRoom);
	}

	private void CancelFindRoom()
	{
		Debug.Log("CancelFindRoom");
		_exitTime = Time.time;
	}

	public void OnRSCreatePVPPrepareRoom(object res)
	{
		Debug.Log("OnRSCreatePVPPrepareRoom");
		if (!(res is RSCreatePVPPrepareRoom))
		{
			return;
		}
		RSCreatePVPPrepareRoom rs = (RSCreatePVPPrepareRoom)res;
		if (rs.Result != 61000)
		{
			MonoBehaviourSingleton<OrangeGameManager>.Instance.ShowErrorMsg((Code)rs.Result, false);
			return;
		}
		NetSealBattleSettingInfo netSealBattleSettingInfo = null;
		if (!ManagedSingleton<PlayerHelper>.Instance.ParserUnsealedBattleSetting(rs.Unsealedbattlesetting, out netSealBattleSettingInfo))
		{
			MonoBehaviourSingleton<OrangeGameManager>.Instance.ShowErrorMsg(Code.MATCH_CREATEROOM_FAIL, false);
			return;
		}
		MonoBehaviourSingleton<OrangeMatchManager>.Instance.Host = rs.Ip;
		MonoBehaviourSingleton<OrangeMatchManager>.Instance.Port = rs.Port;
		MonoBehaviourSingleton<OrangeMatchManager>.Instance.SingleMatch = false;
		_roomID = rs.Roomid;
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_FriendPVPRoomMain", delegate(FriendPVPRoomMain friendPVPRoomMain)
		{
			STAGE_TABLE value;
			ManagedSingleton<OrangeDataManager>.Instance.STAGE_TABLE_DICT.TryGetValue(MonoBehaviourSingleton<OrangeMatchManager>.Instance.StageID, out value);
			friendPVPRoomMain.Setup(true, rs.Roomid, rs.Invitecode, value, true);
		});
	}

	private void OnRSPVPContinueRoomFind(object res)
	{
		Debug.Log("OnRSPVPContinueRoomFind");
		float currentTime = Time.time;
		float delayTime = 2f;
		LeanTween.cancel(ref _roomFindDelayTween, false);
		if (!(res is RSPVPContinueRoomFind))
		{
			return;
		}
		RSPVPContinueRoomFind rSPVPContinueRoomFind = (RSPVPContinueRoomFind)res;
		if (rSPVPContinueRoomFind.Result != 62500)
		{
			if (currentTime < _exitTime)
			{
				_roomFindDelayTween = LeanTween.delayedCall(base.gameObject, delayTime, (Action)delegate
				{
					MonoBehaviourSingleton<OrangeMatchManager>.Instance.PVPContinueRoomFind(MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.Setting.FriendPVPHostID, OnRSPVPContinueRoomFind);
					Debug.Log("Retry continue room find, current time = " + currentTime + ", exit time = " + _exitTime);
				}).uniqueId;
			}
			else
			{
				StopReconnectingToHost();
				DisplayOpponentLeftMsg();
			}
		}
		else
		{
			StopReconnectingToHost();
			Debug.Log("OnRSPVPContinueRoomFind: Continue room found, ID = " + MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.Setting.FriendPVPHostID + ", rs.Result = " + rSPVPContinueRoomFind.Result);
			_roomID = rSPVPContinueRoomFind.Roomid;
			MonoBehaviourSingleton<SignalDispatcher>.Instance.AddHandler(CM.RSPVPPrepareRoomInfo, OnRSPVPPrepareRoomInfo, 0, true);
			MonoBehaviourSingleton<CMSocketClient>.Instance.SendProtocol(FlatBufferCMHelper.CreateRQPVPPrepareRoomInfo(rSPVPContinueRoomFind.Roomid));
		}
	}

	public void OnRSPVPPrepareRoomInfo(object res)
	{
		Debug.Log("OnRSPVPPrepareRoomInfo");
		if (res is RSPVPPrepareRoomInfo)
		{
			RSPVPPrepareRoomInfo rSPVPPrepareRoomInfo = (RSPVPPrepareRoomInfo)res;
			if (rSPVPPrepareRoomInfo.Result != 60100)
			{
				Debug.Log("Get friend PVP room info failed.  Error = " + rSPVPPrepareRoomInfo.Result);
				MonoBehaviourSingleton<OrangeGameManager>.Instance.ShowConfirmMsg("PVP_FRIEND_ROOMERROR");
				return;
			}
			MonoBehaviourSingleton<OrangeMatchManager>.Instance.PvpMatchType = (PVPMatchType)rSPVPPrepareRoomInfo.Pvptype;
			MonoBehaviourSingleton<OrangeMatchManager>.Instance.SelectStageData = ManagedSingleton<OrangeDataManager>.Instance.STAGE_TABLE_DICT[rSPVPPrepareRoomInfo.Stageid];
			Debug.Log("OnRSPVPPrepareRoomInfo received, IP = " + rSPVPPrepareRoomInfo.Ip + ", port = " + rSPVPPrepareRoomInfo.Port + ", Pvptype = " + rSPVPPrepareRoomInfo.Pvptype);
			_roomID = rSPVPPrepareRoomInfo.Roomid;
			MonoBehaviourSingleton<SignalDispatcher>.Instance.AddHandler(CM.RSJoinPrepareRoom, OnRSJoinPrepareRoom, 0, true);
			MonoBehaviourSingleton<OrangeMatchManager>.Instance.JoinRoomFriendBattle(rSPVPPrepareRoomInfo.Ip, rSPVPPrepareRoomInfo.Port, rSPVPPrepareRoomInfo.Roomid, rSPVPPrepareRoomInfo.Capacity);
		}
	}

	public void OnRSJoinPrepareRoom(object res)
	{
		if (!(res is RSJoinPrepareRoom))
		{
			return;
		}
		if (((RSJoinPrepareRoom)res).Result != 62000)
		{
			MonoBehaviourSingleton<OrangeGameManager>.Instance.ShowConfirmMsg("ROOM_CANNOT_JOIN");
			return;
		}
		MonoBehaviourSingleton<OrangeMatchManager>.Instance.SingleMatch = false;
		MonoBehaviourSingleton<UIManager>.Instance.OpenLoadingUI(delegate
		{
			MonoBehaviourSingleton<UIManager>.Instance.BackToHometop(true, false, delegate
			{
				MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_FriendPVPRoomMain", delegate(FriendPVPRoomMain ui)
				{
					MonoBehaviourSingleton<UIManager>.Instance.CloseLoadingUI(null, 0.3f);
					ui.Setup(false, _roomID, "", MonoBehaviourSingleton<OrangeMatchManager>.Instance.SelectStageData);
				});
			});
		}, OrangeSceneManager.LoadingType.WHITE);
	}

	public void DisplayOpponentLeftMsg()
	{
		MonoBehaviourSingleton<UIManager>.Instance.LoadResourceUI("UI_CommonMsg", delegate(CommonUI commonMsg)
		{
			commonMsg.SetupConfirm(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_TIP"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("PVP_FRIEND_ROOMCLOSED"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_OK"), null);
		});
	}
}
