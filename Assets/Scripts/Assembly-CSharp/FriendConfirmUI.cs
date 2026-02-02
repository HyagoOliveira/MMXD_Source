using CallbackDefs;
using UnityEngine;

public class FriendConfirmUI : OrangeUIBase
{
	private Callback m_okCb;

	private Callback m_noCb;

	[SerializeField]
	private OrangeText TitleText;

	[SerializeField]
	private OrangeText MessageText;

	[SerializeField]
	private OrangeText ConfirmText;

	[SerializeField]
	private OrangeText CancelText;

	public void SetupYesNO(string p_title, string p_msg, string p_textYes, string p_textNo, Callback p_okCb, Callback p_noCb = null)
	{
		TitleText.text = p_title;
		MessageText.text = p_msg;
		ConfirmText.text = p_textYes;
		CancelText.text = p_textNo;
		m_okCb = p_okCb;
		m_noCb = p_noCb;
		base._EscapeEvent = EscapeEvent.CUSTOM;
	}

	public void OnClickYes()
	{
		m_okCb.CheckTargetToInvoke();
		base.OnClickCloseBtn();
	}

	public void OnClickNo()
	{
		m_noCb.CheckTargetToInvoke();
		base.OnClickCloseBtn();
	}

	public void OnClickCancel()
	{
		base.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
		base.OnClickCloseBtn();
	}

	protected override void DoCustomEscapeEvent()
	{
		OnClickCancel();
	}
}
