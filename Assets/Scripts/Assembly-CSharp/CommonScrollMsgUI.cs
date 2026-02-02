using UnityEngine;

public class CommonScrollMsgUI : OrangeUIBase
{
	[SerializeField]
	private OrangeText textTitle;

	[SerializeField]
	private OrangeText textMsg;

	public void Setup(string p_title, string p_msg)
	{
		textTitle.text = p_title;
		textMsg.alignByGeometry = false;
		textMsg.text = p_msg;
		base._EscapeEvent = EscapeEvent.CLOSE_UI;
	}
}
