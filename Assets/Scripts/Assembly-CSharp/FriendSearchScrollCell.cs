using UnityEngine;
using UnityEngine.UI;

public class FriendSearchScrollCell : ScrollIndexCallback
{
	private FriendSearchListUI parentFriendSearchListUI;

	[SerializeField]
	private Text PlayerName;

	[SerializeField]
	private Text PlayerLevel;

	[SerializeField]
	private GameObject PlayerIcon;

	private int idx;

	private void Start()
	{
	}

	private void Update()
	{
	}

	public override void ScrollCellIndex(int p_idx)
	{
		parentFriendSearchListUI = GetComponentInParent<FriendSearchListUI>();
		idx = p_idx;
		base.name = "search_" + p_idx;
		PlayerName.text = parentFriendSearchListUI.GetPlayerName(idx);
		PlayerLevel.text = "Lv" + parentFriendSearchListUI.GetPlayerLevel(idx);
		int playerIcon = parentFriendSearchListUI.GetPlayerIcon(idx);
		MonoBehaviourSingleton<OrangeCommunityManager>.Instance.SetPlayerIcon(PlayerIcon.transform, playerIcon, new Vector3(0.7f, 0.7f, 0.7f), false);
	}

	public void OnSendInviteRequest()
	{
		if (MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicFriendInviteRequest.Count >= OrangeConst.FRIEND_INVITE_LIMIT)
		{
			MonoBehaviourSingleton<UIManager>.Instance.LoadResourceUI("UI_CommonMsg", delegate(CommonUI ui)
			{
				ui.OpenSE = SystemSE.CRI_SYSTEMSE_SYS_ERROR;
				ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
				ui.SetupConfirm(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_TIP"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("SOCIAL_ADD_FRIEND_LIMIT"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_OK"), null);
			});
		}
		else
		{
			parentFriendSearchListUI.OnSendInviteRequest(idx);
		}
	}
}
