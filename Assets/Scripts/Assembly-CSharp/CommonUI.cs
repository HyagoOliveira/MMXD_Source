using CallbackDefs;
using UnityEngine;
using UnityEngine.UI;

public class CommonUI : OrangeUIBase
{
	[SerializeField]
	private Button btnYes;

	[SerializeField]
	private Button btnNo;

	[SerializeField]
	private Button btnNo2;

	[SerializeField]
	private Text textTitle;

	[SerializeField]
	private Text textDesc;

	[SerializeField]
	private Text textYes;

	[SerializeField]
	private Text textNo;

	private Callback m_okCb;

	private Callback m_noCb;

	private bool onlyConfirmBtn;

	public SystemSE OpenSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP;

	public SystemSE YesSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;

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

	public void SetupYesNO(string p_title, string p_desc, string p_textYes, string p_textNo, Callback p_okCb, Callback p_noCb = null)
	{
		textTitle.text = p_title;
		textDesc.text = p_desc.Replace("\\n", "\n");
		textYes.text = p_textYes;
		textNo.text = p_textNo;
		btnYes.gameObject.SetActive(true);
		btnNo.gameObject.SetActive(true);
		btnNo2.gameObject.SetActive(true);
		m_okCb = p_okCb;
		m_noCb = p_noCb;
		onlyConfirmBtn = false;
		if (MonoBehaviourSingleton<AudioManager>.Instance.IsInitSystemSE)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(OpenSE);
		}
	}

	public void SetupYesNoByKey(string p_titleKey, string p_descKey, string p_textYesKey, string p_textNoKey, Callback p_okCb, Callback p_noCb = null)
	{
		textTitle.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr(p_titleKey);
		textDesc.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr(p_descKey).Replace("\\n", "\n");
		textYes.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr(p_textYesKey);
		textNo.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr(p_textNoKey);
		btnYes.gameObject.SetActive(true);
		btnNo.gameObject.SetActive(true);
		btnNo2.gameObject.SetActive(true);
		m_okCb = p_okCb;
		m_noCb = p_noCb;
		onlyConfirmBtn = false;
		if (MonoBehaviourSingleton<AudioManager>.Instance.IsInitSystemSE)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(OpenSE);
		}
	}

	public void SetupConfirm(string p_title, string p_desc, string p_yesText, Callback p_cb)
	{
		textTitle.text = p_title;
		textDesc.text = p_desc.Replace("\\n", "\n");
		textYes.text = p_yesText;
		btnYes.gameObject.SetActive(true);
		m_okCb = p_cb;
		onlyConfirmBtn = true;
		if (MonoBehaviourSingleton<AudioManager>.Instance.IsInitSystemSE)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(OpenSE);
		}
	}

	public void SetupConfirmByKey(string p_titleKey, string p_descKey, string p_yesTextKey, Callback p_cb)
	{
		textTitle.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr(p_titleKey);
		textDesc.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr(p_descKey).Replace("\\n", "\n");
		textYes.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr(p_yesTextKey);
		btnYes.gameObject.SetActive(true);
		m_okCb = p_cb;
		onlyConfirmBtn = true;
		if (MonoBehaviourSingleton<AudioManager>.Instance.IsInitSystemSE)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(OpenSE);
		}
	}

	public void OnClickYes()
	{
		if (!MonoBehaviourSingleton<AudioManager>.Instance.IsInitSystemSE || MuteSE)
		{
			base.CloseSE = SystemSE.NONE;
		}
		else
		{
			base.CloseSE = YesSE;
		}
		m_okCb.CheckTargetToInvoke();
		base.OnClickCloseBtn();
	}

	public override void OnClickCloseBtn()
	{
		if (onlyConfirmBtn)
		{
			YesSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
			OnClickYes();
			return;
		}
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
		textYes.font = languageFont;
		textNo.font = languageFont;
	}
}
