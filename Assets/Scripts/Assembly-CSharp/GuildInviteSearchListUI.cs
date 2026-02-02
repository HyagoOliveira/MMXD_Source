using UnityEngine;
using UnityEngine.UI;

public class GuildInviteSearchListUI : OrangeUIBase
{
	private const int SCROLL_VISUAL_COUNT = 5;

	[SerializeField]
	private LoopVerticalScrollRect _scrollRect;

	[SerializeField]
	private GuildInviteSearchPlayerCell _scrollCell;

	[HideInInspector]
	public string TargetPlayerId;

	public void Setup(string targetPlayerId)
	{
		base._EscapeEvent = EscapeEvent.CLOSE_UI;
		TargetPlayerId = targetPlayerId;
		LoopVerticalScrollRect scrollRect = _scrollRect;
		if ((object)scrollRect != null)
		{
			scrollRect.OrangeInit(_scrollCell, 5, 1);
		}
	}
}
