#define RELEASE
using System;
using UnityEngine;
using UnityEngine.UI;
using enums;

public class RankingScrollCell : ScrollIndexCallback
{
	private RankingMainUI parentRankingMainUI;

	[SerializeField]
	private Text Rank;

	[SerializeField]
	private Text Name;

	[SerializeField]
	private Text Score;

	[SerializeField]
	private Image[] Badges;

	[SerializeField]
	private Image[] BadgeBGs;

	[SerializeField]
	private Image TouchImage;

	[SerializeField]
	private GameObject PlayerIcon;

	[SerializeField]
	private Transform PlayerSignRoot;

	[SerializeField]
	private GameObject SignObject;

	[SerializeField]
	private GameObject _guildObject;

	[SerializeField]
	private CommonGuildBadge _guildBadge;

	[SerializeField]
	private Text _guildName;

	[SerializeField]
	private GuildPrivilegeHelper _guildPrivilegeHelper;

	private int idx;

	private string PlayerID;

	private string PlayerName;

	private int PlayerRank;

	private int PlayerScore;

	private int StandbyChara;

	private int StandbyCharSkin;

	private int MainWeaponID;

	private int MainWeaponSkin;

	private int BastWeaponID;

	private int BastWeaponSkin;

	private int MAX_RANK = 20000000;

	private Color32[] colors = new Color32[4]
	{
		new Color32(byte.MaxValue, 230, 93, byte.MaxValue),
		new Color32(208, 228, byte.MaxValue, byte.MaxValue),
		new Color32(251, 219, 214, byte.MaxValue),
		new Color32(0, 170, byte.MaxValue, byte.MaxValue)
	};

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

	private void SetGuildInfo(string playerId)
	{
		SocketGuildMemberInfo value;
		SocketGuildInfo value2;
		if (!MonoBehaviourSingleton<OrangeCommunityManager>.Instance.SocketGuildMemberInfoCache.TryGetValue(playerId, out value) || value.GuildId == 0)
		{
			SetGuildInfo(null, null);
		}
		else if (!MonoBehaviourSingleton<OrangeCommunityManager>.Instance.SocketGuildInfoCache.TryGetValue(value.GuildId, out value2))
		{
			Debug.LogError(string.Format("Failed to get GuildInfo {0} of Player {1}", value.GuildId, value.PlayerId));
			SetGuildInfo(null, null);
		}
		else
		{
			SetGuildInfo(value2, value);
		}
	}

	private void SetGuildInfo(SocketGuildInfo guildInfo, SocketGuildMemberInfo memberInfo)
	{
		if (guildInfo != null)
		{
			_guildObject.SetActive(true);
			_guildName.text = guildInfo.GuildName;
			_guildPrivilegeHelper.Setup((GuildPrivilege)memberInfo.GuildPrivilege);
			_guildBadge.Setup(guildInfo.GuildBadge, 0f);
		}
		else
		{
			_guildObject.SetActive(false);
		}
	}

	public void Awake()
	{
		_guildObject.SetActive(false);
	}

	private void Start()
	{
	}

	private void Update()
	{
		if ((bool)parentRankingMainUI)
		{
			if (PlayerID == parentRankingMainUI.GetCurrectTouchPlayerID())
			{
				TouchImage.enabled = true;
			}
			else
			{
				TouchImage.enabled = false;
			}
		}
	}

	public void SetItemData(int rank, string name, int score, bool bScoreToTime = false)
	{
		Rank.text = ((rank >= 999) ? "999+" : string.Concat(rank + 1));
		Name.text = name;
		if (bScoreToTime)
		{
			TimeSpan timeSpan = TimeSpan.FromMilliseconds(score);
			Score.text = string.Format("{0:00}:{1:00}.{2:00}", timeSpan.Minutes, timeSpan.Seconds, timeSpan.Milliseconds / 10);
		}
		else
		{
			Score.text = score.ToString();
		}
		if (rank >= MAX_RANK || rank < 0)
		{
			Rank.text = "----";
			Score.text = "----";
		}
		Score.color = colors[3];
		if (rank < 3 && rank >= 0)
		{
			Score.color = colors[rank];
		}
		for (int i = 0; i < BadgeBGs.Length; i++)
		{
			Badges[i].enabled = false;
			BadgeBGs[i].enabled = false;
			if (rank == i)
			{
				Badges[i].enabled = true;
				BadgeBGs[i].enabled = true;
			}
		}
		if (!parentRankingMainUI.GetShowEventRankingFlag())
		{
			return;
		}
		int currentSelectEventID = parentRankingMainUI.GetCurrentSelectEventID();
		if (!ManagedSingleton<ExtendDataHelper>.Instance.EVENT_TABLE_DICT.ContainsKey(currentSelectEventID))
		{
			return;
		}
		EVENT_TABLE eVENT_TABLE = ManagedSingleton<ExtendDataHelper>.Instance.EVENT_TABLE_DICT[currentSelectEventID];
		if (6 == eVENT_TABLE.n_TYPE)
		{
			if (score > 8640000)
			{
				Score.text = "----";
			}
			else
			{
				Score.text = OrangeGameUtility.GetRemainTimeTextDetail(score);
			}
		}
	}

	public void OnShowTargetInfo()
	{
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_CURSOR08);
		OrangeCommunityManager.m_TargetRankingInfo.m_PlayerId = PlayerID;
		OrangeCommunityManager.m_TargetRankingInfo.m_Name = PlayerName;
		OrangeCommunityManager.m_TargetRankingInfo.m_Rank = PlayerRank;
		OrangeCommunityManager.m_TargetRankingInfo.m_Score = PlayerScore;
		OrangeCommunityManager.m_TargetRankingInfo.m_StandbyCharID = StandbyChara;
		OrangeCommunityManager.m_TargetRankingInfo.m_MainWeaponID = MainWeaponID;
		OrangeCommunityManager.m_TargetRankingInfo.m_BastWeaponID = BastWeaponID;
		OrangeCommunityManager.m_TargetRankingInfo.m_StandbyCharSkin = StandbyCharSkin;
		parentRankingMainUI.GetRankingScrollCellMessage(idx);
	}

	public override void ScrollCellIndex(int p_idx)
	{
		if (parentRankingMainUI == null)
		{
			parentRankingMainUI = GetComponentInParent<RankingMainUI>();
		}
		idx = p_idx;
		base.name = "rank" + p_idx;
		PlayerID = OrangeCommunityManager.m_RankingInfo[p_idx].m_PlayerId;
		SocketPlayerHUD value = null;
		if (!MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicHUD.TryGetValue(PlayerID, out value))
		{
			value = new SocketPlayerHUD();
			value.m_Name = PlayerID;
		}
		PlayerName = value.m_Name;
		PlayerRank = OrangeCommunityManager.m_RankingInfo[p_idx].m_Rank;
		PlayerRank = ((PlayerRank < 0) ? MAX_RANK : PlayerRank);
		PlayerScore = OrangeCommunityManager.m_RankingInfo[p_idx].m_Score;
		StandbyChara = OrangeCommunityManager.m_RankingInfo[p_idx].m_PlayerModelID;
		StandbyCharSkin = OrangeCommunityManager.m_RankingInfo[p_idx].m_PlayerModelSkin;
		BastWeaponID = OrangeCommunityManager.m_RankingInfo[p_idx].m_BestWeaponModelID;
		if (BastWeaponID <= 0)
		{
			BastWeaponID = OrangeCommunityManager.m_RankingInfo[p_idx].m_MainWeaponModelID;
		}
		if (parentRankingMainUI.OnGetCurrentType() == 3)
		{
			PlayerName = GetWeaponName(BastWeaponID);
		}
		SetItemData(PlayerRank, PlayerName, PlayerScore, OrangeCommunityManager.m_RankingInfo[p_idx].m_bConvertScoreToTime);
		MainWeaponID = value.m_MainWeaponID;
		MainWeaponSkin = value.m_MainWeaponSkin;
		int iconNumber = value.m_IconNumber;
		MonoBehaviourSingleton<OrangeCommunityManager>.Instance.SetPlayerIcon(PlayerIcon.transform, iconNumber, new Vector3(0.7f, 0.7f, 0.7f), false);
		parentRankingMainUI.CheckRankingListIndex(PlayerRank);
		PlayerSignRoot.gameObject.SetActive(value.m_TitleNumber > 0);
		SetPlayerSignIcon(value.m_TitleNumber);
		SetGuildInfo(OrangeCommunityManager.m_RankingInfo[p_idx].m_PlayerId);
	}

	public void PlayerSetCellData(int rank, string pid, string name, int score, int StandbyChar, int wid, bool bScoreToTime = false)
	{
		parentRankingMainUI = GetComponentInParent<RankingMainUI>();
		PlayerID = pid;
		PlayerName = name;
		PlayerRank = rank;
		PlayerRank = ((PlayerRank < 0) ? MAX_RANK : PlayerRank);
		StandbyChara = StandbyChar;
		StandbyCharSkin = ManagedSingleton<PlayerNetManager>.Instance.dicCharacter[StandbyChara].netInfo.Skin;
		BastWeaponID = wid;
		if (BastWeaponID <= 0)
		{
			BastWeaponID = ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.MainWeaponID;
		}
		if (parentRankingMainUI.OnGetCurrentType() == 3)
		{
			PlayerName = GetWeaponName(BastWeaponID);
			score = ManagedSingleton<StatusHelper>.Instance.GetWeaponStatus(BastWeaponID).nBattlePower;
		}
		MainWeaponID = ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.MainWeaponID;
		SetItemData(PlayerRank, name, score, bScoreToTime);
		int portraitID = ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.PortraitID;
		MonoBehaviourSingleton<OrangeCommunityManager>.Instance.SetPlayerIcon(PlayerIcon.transform, portraitID, new Vector3(0.7f, 0.7f, 0.7f), true);
		PlayerSignRoot.gameObject.SetActive(ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.TitleID > 0);
		SetPlayerSignIcon(ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.TitleID);
		SetGuildInfo(pid);
	}

	public string GetWeaponName(int wid)
	{
		return ManagedSingleton<OrangeTextDataManager>.Instance.WEAPONTEXT_TABLE_DICT.GetL10nValue(ManagedSingleton<OrangeDataManager>.Instance.WEAPON_TABLE_DICT[wid].w_NAME);
	}
}
