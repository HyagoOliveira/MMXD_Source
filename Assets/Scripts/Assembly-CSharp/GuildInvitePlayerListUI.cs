using UnityEngine;
using UnityEngine.UI;

public class GuildInvitePlayerListUI : OrangeUIBase
{
	private const int SCROLL_VISUAL_COUNT = 5;

	[SerializeField]
	private LoopVerticalScrollRect _scrollRect;

	[SerializeField]
	private GuildInvitePlayerCell _scrollCell;

	[SerializeField]
	private GameObject _emptyHint;

	[SerializeField]
	private Text _textInviteCount;

	private void OnEnable()
	{
		Singleton<GuildSystem>.Instance.OnCancelInvitePlayerEvent += OnCancelInvitePlayerEvent;
	}

	private void OnDisable()
	{
		Singleton<GuildSystem>.Instance.OnCancelInvitePlayerEvent -= OnCancelInvitePlayerEvent;
	}

	public void OnClickCancelAllInvitePlayerBtn()
	{
		PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_CANCEL);
		Singleton<GuildSystem>.Instance.ReqCancelInvitePlayer();
	}

	public void Setup()
	{
		base._EscapeEvent = EscapeEvent.CLOSE_UI;
		base.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
		RefreshList();
	}

	private void OnCancelInvitePlayerEvent(Code ackCode)
	{
		if (ackCode == Code.GUILD_REFUSE_INVITE_SUCCESS)
		{
			RefreshList();
		}
	}

	private void RefreshList()
	{
		int count = Singleton<GuildSystem>.Instance.InvitePlayerListCache.Count;
		GameObject emptyHint = _emptyHint;
		if ((object)emptyHint != null)
		{
			emptyHint.SetActive(count == 0);
		}
		_textInviteCount.text = string.Format("{0}/{1}", count, OrangeConst.GUILD_HALLINVITE_MAX);
		LoopVerticalScrollRect scrollRect = _scrollRect;
		if ((object)scrollRect != null)
		{
			scrollRect.OrangeInit(_scrollCell, 5, Singleton<GuildSystem>.Instance.InvitePlayerListCache.Count);
		}
	}
}
