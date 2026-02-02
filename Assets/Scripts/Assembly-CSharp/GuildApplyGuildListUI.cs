#define RELEASE
using UnityEngine;
using UnityEngine.UI;

public class GuildApplyGuildListUI : OrangeUIBase
{
	protected const int SCROLL_VISUAL_COUNT = 3;

	[SerializeField]
	private LoopVerticalScrollRect _scrollRect;

	[SerializeField]
	private GuildApplyGuildCell _scrollCell;

	[SerializeField]
	private GameObject _emptyHint;

	private void OnEnable()
	{
		Singleton<GuildSystem>.Instance.OnCancelJoinGuildEvent += OnCancelJoinGuildEvent;
	}

	private void OnDisable()
	{
		Singleton<GuildSystem>.Instance.OnCancelJoinGuildEvent -= OnCancelJoinGuildEvent;
	}

	public void Setup()
	{
		Debug.Log("[Setup]");
		base._EscapeEvent = EscapeEvent.CLOSE_UI;
		RefreshCells();
	}

	public void OnClickGuildInfoBtn(int guildId)
	{
		MonoBehaviourSingleton<UIManager>.Instance.Connecting(true);
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_GuildMemberList", delegate(GuildMemberListUI ui)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
			ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
			ui.Setup(guildId);
		});
	}

	public void OnClickOneCancelApplyBtn(int guildId)
	{
		Singleton<GuildSystem>.Instance.ReqCancelJoinGuild(guildId);
	}

	private void OnGetApplyGuildListEvent(Code ackCode)
	{
		Debug.Log("[OnGetApplyGuildListEvent]");
		RefreshCells();
	}

	private void OnCancelJoinGuildEvent(Code ackCode)
	{
		Debug.Log("[OnCancelJoinGuildEvent]");
		if (ackCode == Code.GUILD_REFUSE_JOIN_APPLY_SUCCESS)
		{
			RefreshCells();
		}
	}

	private void RefreshCells()
	{
		_emptyHint.SetActive(Singleton<GuildSystem>.Instance.ApplyGuildListCache.Count == 0);
		_scrollRect.ClearCells();
		_scrollRect.OrangeInit(_scrollCell, 3, Singleton<GuildSystem>.Instance.ApplyGuildListCache.Count);
	}
}
