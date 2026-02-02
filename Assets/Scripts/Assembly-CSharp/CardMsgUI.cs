using CallbackDefs;
using UnityEngine;
using UnityEngine.UI;

public class CardMsgUI : OrangeUIBase
{
	[SerializeField]
	private Button btnYes1;

	[SerializeField]
	private Button btnYes2;

	[SerializeField]
	private Button btnNo;

	[SerializeField]
	private Button btnNo2;

	[SerializeField]
	private Text textTitle;

	[SerializeField]
	private Text textDesc;

	[SerializeField]
	private Text textYes1;

	[SerializeField]
	private Text textYes2;

	[SerializeField]
	private Text textNo;

	private Callback m_okCb1;

	private Callback m_okCb2;

	private Callback m_noCb;

	private bool onlyConfirmBtn;

	public SystemSE OpenSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP;

	public SystemSE Yes1SE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;

	public SystemSE Yes2SE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;

	public SystemSE NoSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;

	public bool MuteSE;

	protected override void Awake()
	{
		base.Awake();
		UpdateFont();
	}

	private void Start()
	{
		base._EscapeEvent = EscapeEvent.CLOSE_UI;
	}

	public void SetupCardMsg(string p_title, string p_desc, string p_textYes1, string p_textYes2, string p_textNo, Callback p_okCb1, Callback p_okCb2, Callback p_noCb = null)
	{
		textTitle.text = p_title;
		textDesc.text = p_desc.Replace("\\n", "\n");
		textYes1.text = p_textYes1;
		textYes2.text = p_textYes2;
		textNo.text = p_textNo;
		btnYes1.gameObject.SetActive(true);
		btnYes2.gameObject.SetActive(true);
		btnNo.gameObject.SetActive(true);
		btnNo2.gameObject.SetActive(true);
		m_okCb1 = p_okCb1;
		m_okCb2 = p_okCb2;
		m_noCb = p_noCb;
		onlyConfirmBtn = false;
		if (MonoBehaviourSingleton<AudioManager>.Instance.IsInitSystemSE)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(OpenSE);
		}
	}

	public void OnClickYes1()
	{
		if (!MonoBehaviourSingleton<AudioManager>.Instance.IsInitSystemSE || MuteSE)
		{
			base.CloseSE = SystemSE.NONE;
		}
		else
		{
			base.CloseSE = Yes1SE;
		}
		m_okCb1.CheckTargetToInvoke();
		base.OnClickCloseBtn();
	}

	public void OnClickYes2()
	{
		if (!MonoBehaviourSingleton<AudioManager>.Instance.IsInitSystemSE || MuteSE)
		{
			base.CloseSE = SystemSE.NONE;
		}
		else
		{
			base.CloseSE = Yes2SE;
		}
		m_okCb2.CheckTargetToInvoke();
		base.OnClickCloseBtn();
	}

	public override void OnClickCloseBtn()
	{
		base.CloseSE = (MonoBehaviourSingleton<AudioManager>.Instance.IsInitSystemSE ? SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL : SystemSE.NONE);
		if (MuteSE)
		{
			base.CloseSE = SystemSE.NONE;
		}
		base.OnClickCloseBtn();
		m_noCb.CheckTargetToInvoke();
	}

	private void UpdateFont()
	{
		Font languageFont = MonoBehaviourSingleton<LocalizationManager>.Instance.LanguageFont;
		textTitle.font = languageFont;
		textDesc.font = languageFont;
		textYes1.font = languageFont;
		textYes2.font = languageFont;
		textNo.font = languageFont;
	}
}
