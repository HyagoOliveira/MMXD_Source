using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class FriendInviteScrollCell : ScrollIndexCallback
{
	private FriendInviteListUI parentFriendInviteListUI;

	[SerializeField]
	private Text PlayerName;

	[SerializeField]
	private GameObject[] ConfirmBtn;

	[SerializeField]
	private Text RewardText;

	[SerializeField]
	private Text RewardBtnText;

	[SerializeField]
	private GameObject PlayerIcon;

	[SerializeField]
	private Text PlayerLevelText;

	[SerializeField]
	private Transform PlayerSignRoot;

	[SerializeField]
	private GameObject SignObject;

	private int idx;

	private string PlayerID;

	public void SetPlayerSignIcon(int n_ID = 0, bool bOwner = false)
	{
		if (PlayerSignRoot != null && SignObject != null)
		{
			int childCount = PlayerSignRoot.transform.childCount;
			for (int i = 0; i < childCount; i++)
			{
				Object.Destroy(PlayerSignRoot.transform.GetChild(i).gameObject);
			}
			if (n_ID > 0)
			{
				GameObject obj = Object.Instantiate(SignObject, PlayerSignRoot.position, new Quaternion(0f, 0f, 0f, 0f));
				obj.transform.SetParent(PlayerSignRoot);
				obj.transform.localScale = new Vector3(1f, 1f, 1f);
				obj.GetComponent<CommonSignBase>().SetupSign(n_ID, bOwner);
			}
		}
	}

	private void Start()
	{
	}

	private void Update()
	{
	}

	private void SetConfirmButton(int typ)
	{
		for (int i = 0; i < ConfirmBtn.Length; i++)
		{
			ConfirmBtn[i].SetActive(false);
		}
		ConfirmBtn[typ].SetActive(true);
	}

	public override void ScrollCellIndex(int p_idx)
	{
		parentFriendInviteListUI = GetComponentInParent<FriendInviteListUI>();
		int num = parentFriendInviteListUI.OnGetCurrentType();
		SetConfirmButton(num);
		if (1 == num)
		{
			idx = p_idx;
			base.name = "reward_" + p_idx;
			RewardBtnText.text = "x" + OrangeConst.GIFT_AP;
			RewardText.text = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("SOCIAL_GIFT_AP"), OrangeConst.GIFT_AP.ToString());
			SocketPlayerHUD socketPlayerHUD = JsonHelper.Deserialize<SocketPlayerHUD>(MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicFriendRewardReceive.Values.ToList()[p_idx].FriendPlayerHUD);
			PlayerName.text = socketPlayerHUD.m_Name;
			PlayerID = socketPlayerHUD.m_PlayerId;
			PlayerLevelText.text = "Lv" + socketPlayerHUD.m_Level;
			MonoBehaviourSingleton<OrangeCommunityManager>.Instance.SetPlayerIcon(PlayerIcon.transform, socketPlayerHUD.m_IconNumber, new Vector3(0.7f, 0.7f, 0.7f), false);
		}
		else
		{
			idx = p_idx;
			base.name = "invite_" + p_idx;
			PlayerID = parentFriendInviteListUI.GetPlayerID(idx);
			PlayerLevelText.text = "Lv" + parentFriendInviteListUI.GetPlayerLevel(idx);
			PlayerName.text = parentFriendInviteListUI.GetPlayerName(idx);
			int playerIcon = parentFriendInviteListUI.GetPlayerIcon(idx);
			MonoBehaviourSingleton<OrangeCommunityManager>.Instance.SetPlayerIcon(PlayerIcon.transform, playerIcon, new Vector3(0.7f, 0.7f, 0.7f), false);
		}
	}

	public void OnCancelInviteRequest()
	{
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_CANCEL);
		parentFriendInviteListUI.OnCancelInviteRequest(idx);
	}

	public void OnUseReward()
	{
		parentFriendInviteListUI.OnUseReward(PlayerID);
	}
}
