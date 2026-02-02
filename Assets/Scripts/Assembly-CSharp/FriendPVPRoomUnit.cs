#define RELEASE
using UnityEngine;
using UnityEngine.UI;
using cm;

public class FriendPVPRoomUnit : ScrollIndexCallback
{
	[SerializeField]
	private Text textRoomName;

	[SerializeField]
	private Text textRoomLevel;

	[SerializeField]
	private FriendPVPSelectRoom Parent;

	[SerializeField]
	private Button joinBtn;

	private int idx;

	private bool AddHandler;

	public override void ScrollCellIndex(int p_idx)
	{
		idx = p_idx;
		OrangeText componentInChildren = joinBtn.GetComponentInChildren<OrangeText>();
		textRoomName.text = Parent.listRoomData[p_idx].RoomName;
		joinBtn.interactable = true;
		componentInChildren.color = new Color(1f, 1f, 1f);
	}

	public void OnClickBtnJoinRoomBtn()
	{
		if (!Parent.IsControlBlockEnabled())
		{
			Parent.EnableControlBlock(true);
			Debug.Log("OnClickBtnJoinRoomBtn");
			RoomData roomData = Parent.listRoomData[idx];
			MonoBehaviourSingleton<OrangeMatchManager>.Instance.JoinRoomFriendBattle(roomData.Ip, roomData.Port, roomData.RoomId, roomData.Capacity);
			AddHandler = true;
			MonoBehaviourSingleton<SignalDispatcher>.Instance.AddHandler(CM.RSJoinPrepareRoom, OnRSJoinPrepareRoom);
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK17);
		}
	}

	private void OnRSJoinPrepareRoom(object res)
	{
		Parent.EnableControlBlock();
		AddHandler = false;
		MonoBehaviourSingleton<SignalDispatcher>.Instance.RemoveHandler(CM.RSJoinPrepareRoom, OnRSJoinPrepareRoom);
		if (!(res is RSJoinPrepareRoom))
		{
			return;
		}
		FriendPVPSelectRoom componentInParent = GetComponentInParent<FriendPVPSelectRoom>();
		if (((RSJoinPrepareRoom)res).Result != 62000)
		{
			MonoBehaviourSingleton<OrangeGameManager>.Instance.ShowConfirmMsg("ROOM_CANNOT_JOIN");
			componentInParent.OnClickBtnRefresh();
			return;
		}
		MonoBehaviourSingleton<OrangeMatchManager>.Instance.SingleMatch = false;
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_FriendPVPRoomMain", delegate(FriendPVPRoomMain ui)
		{
			ui.Setup(false, Parent.listRoomData[idx].RoomId, "", Parent.GetSelectStage);
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
