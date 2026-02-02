#define RELEASE
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CoopRoomFriendUnit : ScrollIndexCallback
{
	[SerializeField]
	private CoopRoomMainUI parent;

	[SerializeField]
	private FriendPVPRoomMain parentFriendPVP;

	[SerializeField]
	private CommonIconBase imgPlayerIcon;

	[SerializeField]
	private OrangeText textLv;

	[SerializeField]
	private OrangeText textPlayerName;

	[SerializeField]
	private OrangeText textStatus;

	[SerializeField]
	private OrangeText textPower;

	[SerializeField]
	private Button btnInvite;

	[SerializeField]
	private Color colorOnline;

	[SerializeField]
	private Color colorOffline;

	[SerializeField]
	private GameObject objInviteLimit;

	[SerializeField]
	private Text textInvitelimitTime;

	[SerializeField]
	private Transform PlayerSignRoot;

	[SerializeField]
	private OrangeText textStatus_5_3;

	[SerializeField]
	private GameObject SignObject;

	[SerializeField]
	private OrangeCriSource SoundSource;

	public int idx = -1;

	private SocketPlayerHUD pHUD;

	private string msg = string.Empty;

	public void SetPlayerSignIcon(int n_ID = 0, bool bOwner = false)
	{
		if (PlayerSignRoot != null && SignObject != null)
		{
			int childCount = PlayerSignRoot.transform.childCount;
			for (int i = 0; i < childCount; i++)
			{
				UnityEngine.Object.Destroy(PlayerSignRoot.transform.GetChild(i).gameObject);
			}
			if (n_ID > 0)
			{
				GameObject obj = UnityEngine.Object.Instantiate(SignObject, PlayerSignRoot.position, new Quaternion(0f, 0f, 0f, 0f));
				obj.transform.SetParent(PlayerSignRoot);
				obj.transform.localScale = new Vector3(1f, 1f, 1f);
				obj.GetComponent<CommonSignBase>().SetupSign(n_ID, bOwner);
			}
		}
	}

	public override void ScrollCellIndex(int p_idx)
	{
		StopAllCoroutines();
		if (SoundSource == null)
		{
			SoundSource = base.gameObject.AddOrGetComponent<OrangeCriSource>();
			SoundSource.Initial(OrangeSSType.SYSTEM);
		}
		idx = p_idx;
		SocketFriendInfo listFriend = GetListFriend(idx);
		pHUD = MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicHUD[listFriend.FriendPlayerID];
		if (ManagedSingleton<PlayerHelper>.Instance.GetOnlineStatus(listFriend.Busy, out msg))
		{
			btnInvite.gameObject.SetActive(true);
			textStatus.text = msg;
			textStatus.color = colorOnline;
			textStatus_5_3.text = msg;
			textStatus_5_3.color = colorOnline;
			long num = GetListInviteTime(idx) - CapUtility.DateToUnixTime(DateTime.UtcNow);
			if (num > 0)
			{
				objInviteLimit.SetActive(true);
				StartCoroutine(OnStartCountDown(num));
			}
			else
			{
				objInviteLimit.gameObject.SetActive(false);
			}
		}
		else
		{
			btnInvite.gameObject.SetActive(false);
			objInviteLimit.SetActive(false);
			textStatus.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("STATUS_OFFLINE");
			textStatus.color = colorOffline;
			textStatus_5_3.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("STATUS_OFFLINE");
			textStatus_5_3.color = colorOffline;
		}
		imgPlayerIcon.SetupItem(pHUD.m_IconNumber, idx);
		textLv.text = "Lv" + pHUD.m_Level;
		textPlayerName.text = pHUD.m_Name.ToString();
		textPower.text = pHUD.m_Power.ToString();
		PlayerSignRoot.gameObject.SetActive(pHUD.m_TitleNumber > 0);
		SetPlayerSignIcon(pHUD.m_TitleNumber);
		textStatus.gameObject.SetActive(false);
		textStatus_5_3.gameObject.SetActive(true);
	}

	public void OnClickBtnInvite()
	{
		if (MonoBehaviourSingleton<OrangeMatchManager>.Instance.Capacity <= MonoBehaviourSingleton<OrangeMatchManager>.Instance.ListMemberInfo.Count)
		{
			MonoBehaviourSingleton<OrangeGameManager>.Instance.ShowTipDialog(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("ROOM_MEMBER_FULL"), 31);
		}
		else if (pHUD != null)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK17);
			MonoBehaviourSingleton<OrangeCommunityManager>.Instance.OnSendBattleInvite(pHUD.m_PlayerId, MonoBehaviourSingleton<OrangeMatchManager>.Instance.Host, MonoBehaviourSingleton<OrangeMatchManager>.Instance.Port, GetRoomId(), GetStageTableID(), MonoBehaviourSingleton<OrangeMatchManager>.Instance.Capacity, parentFriendPVP != null);
			SetListInviteTime(idx, CapUtility.DateToUnixTime(DateTime.UtcNow.AddSeconds(10.0)));
			StartCoroutine(OnStartCountDown(10L));
		}
	}

	private IEnumerator OnStartCountDown(long val)
	{
		objInviteLimit.SetActive(true);
		SoundSource.PlaySE("SystemSE", 33);
		textInvitelimitTime.text = val.ToString();
		while (val > 0)
		{
			yield return CoroutineDefine._1sec;
			val--;
			textInvitelimitTime.text = val.ToString();
		}
		SoundSource.PlaySE("SystemSE", 24);
		objInviteLimit.SetActive(false);
	}

	private void OnDisable()
	{
		try
		{
			if ((bool)SoundSource)
			{
				SoundSource.StopAll();
			}
		}
		catch (Exception ex)
		{
			Debug.Log("Unknow exception problem " + ex.Message);
		}
	}

	private SocketFriendInfo GetListFriend(int index)
	{
		if ((bool)parent)
		{
			return parent.ListFriend[index];
		}
		if ((bool)parentFriendPVP)
		{
			return parentFriendPVP.ListFriend[index];
		}
		return null;
	}

	private long GetListInviteTime(int index)
	{
		if ((bool)parent)
		{
			return parent.ListInviteTime[index];
		}
		if ((bool)parentFriendPVP)
		{
			return parentFriendPVP.ListInviteTime[index];
		}
		return 0L;
	}

	private void SetListInviteTime(int index, long time)
	{
		if ((bool)parent)
		{
			parent.ListInviteTime[index] = time;
		}
		else if ((bool)parentFriendPVP)
		{
			parentFriendPVP.ListInviteTime[index] = time;
		}
	}

	private string GetRoomId()
	{
		if ((bool)parent)
		{
			return parent.RoomId;
		}
		if ((bool)parentFriendPVP)
		{
			return parentFriendPVP.RoomId;
		}
		return null;
	}

	private int GetStageTableID()
	{
		if ((bool)parent)
		{
			return parent.StageTable.n_ID;
		}
		if ((bool)parentFriendPVP)
		{
			return parentFriendPVP.StageTable.n_ID;
		}
		return 0;
	}
}
