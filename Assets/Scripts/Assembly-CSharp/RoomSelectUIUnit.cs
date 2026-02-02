#define RELEASE
using UnityEngine;
using UnityEngine.UI;
using cm;

public class RoomSelectUIUnit : ScrollIndexCallback
{
	[SerializeField]
	private Text textRoomName;

	[SerializeField]
	private Text textRoomLevel;

	[SerializeField]
	private RoomSelectUI Parent;

	[SerializeField]
	private Button joinBtn;

	private int idx;

	private bool AddHandler;

	public override void ScrollCellIndex(int p_idx)
	{
		idx = p_idx;
		int result = 0;
		OrangeText componentInChildren = joinBtn.GetComponentInChildren<OrangeText>();
		string[] array = Parent.listRoomData[p_idx].Condition.Split(',');
		textRoomName.text = Parent.listRoomData[p_idx].RoomName;
		textRoomLevel.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("RANK_REQUIRE") + array[0];
		joinBtn.interactable = true;
		componentInChildren.color = new Color(1f, 1f, 1f);
		if (!int.TryParse(array[0], out result) || result > ManagedSingleton<PlayerHelper>.Instance.GetLV())
		{
			componentInChildren.color = new Color(0.6f, 0.6f, 0.6f);
			joinBtn.interactable = false;
		}
	}

	public void OnClickBtnJoinRoomBtn()
	{
		Debug.Log("OnClickBtnJoinRoomBtn");
		RoomData roomData = Parent.listRoomData[idx];
		MonoBehaviourSingleton<OrangeMatchManager>.Instance.JoinRoom(roomData.Ip, roomData.Port, roomData.RoomId, roomData.Capacity);
		AddHandler = true;
		MonoBehaviourSingleton<SignalDispatcher>.Instance.AddHandler(CM.RSJoinPrepareRoom, OnRSJoinPrepareRoom);
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK17);
	}

	private void OnRSJoinPrepareRoom(object res)
	{
		AddHandler = false;
		MonoBehaviourSingleton<SignalDispatcher>.Instance.RemoveHandler(CM.RSJoinPrepareRoom, OnRSJoinPrepareRoom);
		if (!(res is RSJoinPrepareRoom))
		{
			return;
		}
		RoomSelectUI parentRoomSelectUI = GetComponentInParent<RoomSelectUI>();
		RSJoinPrepareRoom rSJoinPrepareRoom = (RSJoinPrepareRoom)res;
		MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.bCurrentCoopChallengeMode = rSJoinPrepareRoom.Ischallenge;
		if (rSJoinPrepareRoom.Result != 62000)
		{
			MonoBehaviourSingleton<OrangeGameManager>.Instance.ShowConfirmMsg("ROOM_CANNOT_JOIN");
			parentRoomSelectUI.OnClickBtnRefresh();
			return;
		}
		MonoBehaviourSingleton<OrangeMatchManager>.Instance.SingleMatch = false;
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_CoopRoomMain", delegate(CoopRoomMainUI ui)
		{
			ui.StageTable = Parent.GetSelectStage;
			ui.IsRoomMaster = false;
			ui.RoomId = Parent.listRoomData[idx].RoomId;
			ui.Setup();
			ui.RoomRefreshCB = parentRoomSelectUI.OnClickBtnRefresh;
		});
	}

	private void OnDestroy()
	{
		if (AddHandler)
		{
			AddHandler = false;
			MonoBehaviourSingleton<SignalDispatcher>.Instance.RemoveHandler(CM.RSJoinPrepareRoom, OnRSJoinPrepareRoom);
		}
	}
}
