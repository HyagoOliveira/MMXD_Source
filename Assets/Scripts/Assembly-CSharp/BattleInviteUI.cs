using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using cm;

public class BattleInviteUI : OrangeUIBase
{
	private readonly int INVITE_MAX_TIME = 15;

	[SerializeField]
	private OrangeText textInviteMsg;

	private BattleInviteInfo battleInviteInfo;

	private STAGE_TABLE stage;

	private bool _bFriendPVPMode;

	private bool alreadyGo;

	public Queue<BattleInviteInfo> QueueInvite { get; set; }

	protected override void Awake()
	{
		base.Awake();
		alreadyGo = false;
		MonoBehaviourSingleton<SignalDispatcher>.Instance.AddHandler(CM.RSJoinPrepareRoom, OnRSJoinPrepareRoom);
		QueueInvite = new Queue<BattleInviteInfo>();
	}

	private void OnDestroy()
	{
		MonoBehaviourSingleton<SignalDispatcher>.Instance.RemoveHandler(CM.RSJoinPrepareRoom, OnRSJoinPrepareRoom);
	}

	public void Setup(BattleInviteInfo p_battleInviteInfo, STAGE_TABLE p_stage, bool bFriendPVPMode = false)
	{
		battleInviteInfo = p_battleInviteInfo;
		stage = p_stage;
		string l10nValue = ManagedSingleton<OrangeTextDataManager>.Instance.STAGETEXT_TABLE_DICT.GetL10nValue(p_stage.w_NAME);
		textInviteMsg.text = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COOP_BATTLE_INVITE"), battleInviteInfo.InviterName, l10nValue);
		StartCoroutine(OnStartCountdown());
		_bFriendPVPMode = bFriendPVPMode;
	}

	public void OnClickGoCoop()
	{
		if (MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.CheckCardCountMax() || alreadyGo)
		{
			return;
		}
		alreadyGo = true;
		QueueInvite.Clear();
		StopAllCoroutines();
		StageHelper.StageJoinCondition condition = StageHelper.StageJoinCondition.NONE;
		if (ManagedSingleton<StageHelper>.Instance.IsStageConditionOK(stage, ref condition))
		{
			if (_bFriendPVPMode)
			{
				FriendPVPRoomMain uI = MonoBehaviourSingleton<UIManager>.Instance.GetUI<FriendPVPRoomMain>("UI_FriendPVPRoomMain");
				if (null != uI)
				{
					uI.effectTypeClose = UIManager.EffectType.NONE;
					uI.OnClickCloseBtn();
				}
			}
			else
			{
				CoopRoomMainUI uI2 = MonoBehaviourSingleton<UIManager>.Instance.GetUI<CoopRoomMainUI>("UI_CoopRoomMain");
				if (null != uI2)
				{
					uI2.effectTypeClose = UIManager.EffectType.NONE;
					uI2.OnClickCloseBtn();
				}
			}
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK17);
			MonoBehaviourSingleton<OrangeMatchManager>.Instance.Host = battleInviteInfo.Host;
			MonoBehaviourSingleton<OrangeMatchManager>.Instance.Port = battleInviteInfo.Port;
			MonoBehaviourSingleton<OrangeMatchManager>.Instance.MatchServerLogout();
			MonoBehaviourSingleton<OrangeMatchManager>.Instance.MatchServerLogin(delegate
			{
				MonoBehaviourSingleton<OrangeMatchManager>.Instance.SelectStageData = stage;
				if (_bFriendPVPMode)
				{
					MonoBehaviourSingleton<OrangeMatchManager>.Instance.JoinRoomFriendBattle(battleInviteInfo.Host, battleInviteInfo.Port, battleInviteInfo.RoomId, battleInviteInfo.Capacity);
				}
				else
				{
					MonoBehaviourSingleton<OrangeMatchManager>.Instance.JoinRoom(battleInviteInfo.Host, battleInviteInfo.Port, battleInviteInfo.RoomId, battleInviteInfo.Capacity);
				}
			});
		}
		else
		{
			ManagedSingleton<StageHelper>.Instance.DisplayConditionInfo(stage, condition);
			OnClickCloseBtn();
		}
	}

	private void OnRSJoinPrepareRoom(object res)
	{
		if (res is RSJoinPrepareRoom)
		{
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
				if (Singleton<GuildSystem>.Instance.MainSceneController != null)
				{
					CloseGuildUIBeforeJoin();
				}
				else
				{
					MonoBehaviourSingleton<UIManager>.Instance.BackToHometop(true, false, BackToHometopCB);
				}
			}, OrangeSceneManager.LoadingType.WHITE);
		}
		else
		{
			OnClickCloseBtn();
		}
	}

	private void CloseGuildUIBeforeJoin()
	{
		GuildMainUI uI = MonoBehaviourSingleton<UIManager>.Instance.GetUI<GuildMainUI>("UI_GuildMain");
		if (null != uI)
		{
			GuildUIHelper.BackToHometop(uI);
			StartCoroutine(JoinPrepareRoomHelper());
		}
	}

	private IEnumerator JoinPrepareRoomHelper()
	{
		while (MonoBehaviourSingleton<OrangeSceneManager>.Instance.NowScene != "hometop")
		{
			yield return new WaitForEndOfFrame();
		}
		MonoBehaviourSingleton<UIManager>.Instance.BackToHometop(true, false, BackToHometopCB);
	}

	private void BackToHometopCB()
	{
		MonoBehaviourSingleton<OrangeMatchManager>.Instance.SelectStageData = stage;
		MonoBehaviourSingleton<OrangeMatchManager>.Instance.StageID = stage.n_ID;
		if (_bFriendPVPMode)
		{
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_FriendPVPRoomMain", delegate(FriendPVPRoomMain ui)
			{
				MonoBehaviourSingleton<UIManager>.Instance.CloseLoadingUI(null, 0.3f);
				ui.Setup(false, battleInviteInfo.RoomId, "", stage);
			});
			return;
		}
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_CoopRoomMain", delegate(CoopRoomMainUI ui)
		{
			MonoBehaviourSingleton<UIManager>.Instance.CloseLoadingUI(null, 0.3f);
			ui.StageTable = MonoBehaviourSingleton<OrangeMatchManager>.Instance.SelectStageData;
			ui.IsRoomMaster = false;
			ui.RoomId = battleInviteInfo.RoomId;
			ui.Setup();
		});
	}

	private IEnumerator OnStartCountdown()
	{
		for (int nowTime = INVITE_MAX_TIME; nowTime > 0; nowTime--)
		{
			yield return CoroutineDefine._1sec;
		}
		OnClickCancelBtn();
	}

	public void OnClickCancelBtn()
	{
		if (!alreadyGo)
		{
			StopAllCoroutines();
			GetNextInvite();
		}
	}

	private void GetNextInvite()
	{
		if (QueueInvite.Count > 0)
		{
			BattleInviteInfo battleInviteInfo = QueueInvite.Dequeue();
			if (battleInviteInfo.RoomId != this.battleInviteInfo.RoomId)
			{
				STAGE_TABLE p_stage = null;
				if (ManagedSingleton<OrangeTableHelper>.Instance.GetStage(battleInviteInfo.StageId, out p_stage))
				{
					Setup(battleInviteInfo, p_stage);
					return;
				}
			}
			GetNextInvite();
		}
		else
		{
			OnClickCloseBtn();
		}
	}

	public override void OnClickCloseBtn()
	{
		StopAllCoroutines();
		base.OnClickCloseBtn();
	}
}
