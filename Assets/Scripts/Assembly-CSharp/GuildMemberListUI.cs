#define RELEASE
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class GuildMemberListUI : OrangeUIBase
{
	protected const int SCROLL_VISUAL_COUNT = 3;

	[SerializeField]
	private Text _textMemberCount;

	[SerializeField]
	private LoopVerticalScrollRect _scrollRect;

	[SerializeField]
	private GuildMemberCell _scrollCell;

	[SerializeField]
	private GameObject _emptyHint;

	public SocketGuildInfo GuildInfoCache { get; private set; }

	public List<SocketGuildMemberInfo> MemberInfoListCache { get; private set; }

	private void OnEnable()
	{
	}

	private void OnDisable()
	{
	}

	public void Setup(int guildId)
	{
		Singleton<GuildSystem>.Instance.SendSocketGetGuildInfoReq(new List<int> { guildId }, OnGetSocketGuildInfoRes);
	}

	private void OnGetSocketGuildInfoRes(List<SocketGuildInfo> guildInfoList)
	{
		if (guildInfoList == null || guildInfoList.Count == 0)
		{
			MonoBehaviourSingleton<UIManager>.Instance.Connecting(false);
			Debug.LogError("Failed to get SocketGuildInfo");
			Debug.LogError("開發中舊公會可能發生");
		}
		else
		{
			GuildInfoCache = guildInfoList[0];
			Singleton<GuildSystem>.Instance.SendSocketGetGuildPlayerIdListReq(GuildInfoCache.GuildId, OnGetSocketGuildPlayerIdListRes);
		}
	}

	private void OnGetSocketGuildPlayerIdListRes(List<string> playerIdList)
	{
		if (playerIdList == null || playerIdList.Count == 0)
		{
			MonoBehaviourSingleton<UIManager>.Instance.Connecting(false);
			Debug.LogError("Failed to get PlayerIdList");
			Debug.LogError("開發中舊公會可能發生");
		}
		else
		{
			Singleton<GuildSystem>.Instance.SendSocketGetGuildMemberInfoReq(playerIdList, OnGetSocketGuildMemberInfoRes);
		}
	}

	private void OnGetSocketGuildMemberInfoRes(List<SocketGuildMemberInfo> memberInfoList)
	{
		if (memberInfoList == null || memberInfoList.Count == 0)
		{
			MonoBehaviourSingleton<UIManager>.Instance.Connecting(false);
			Debug.LogError("Failed to get MemberInfoList");
			Debug.LogError("開發中舊公會可能發生");
			return;
		}
		MemberInfoListCache = memberInfoList;
		GuildSetting guildSetting;
		if (GuildSetting.TryGetSettingByGuildRank((GuildInfoCache.GuildRank <= 0) ? 1 : GuildInfoCache.GuildRank, out guildSetting))
		{
			_textMemberCount.text = string.Format("{0}/{1}", memberInfoList.Count, guildSetting.MemberLimit);
		}
		else
		{
			Debug.LogError(string.Format("Failed to get {0} of rank {1}", "GuildSetting", GuildInfoCache.GuildRank));
			_textMemberCount.text = string.Format("{0}/NA", memberInfoList.Count);
		}
		IEnumerable<string> targetIds = memberInfoList.Select((SocketGuildMemberInfo memberInfo) => memberInfo.PlayerId);
		Singleton<GuildSystem>.Instance.RefreshBusyStatusAndSearchHUD(targetIds, OnRefreshBusyStatusAndHUD);
	}

	private void OnRefreshBusyStatusAndHUD()
	{
		RefreshCells();
	}

	private void RefreshCells()
	{
		_emptyHint.SetActive(MemberInfoListCache.Count == 0);
		_scrollRect.ClearCells();
		_scrollRect.OrangeInit(_scrollCell, 3, MemberInfoListCache.Count);
		MonoBehaviourSingleton<UIManager>.Instance.Connecting(false);
	}

	public void OnClickMemberCell(string playerId, Vector2 tarPos)
	{
		Debug.Log(string.Format("PlayerId = {0}, Pos = {1}", playerId, tarPos));
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_GuildSubMenu", delegate(GuildSubMenuUI ui)
		{
			ui.Setup(playerId, tarPos, false);
		});
	}
}
