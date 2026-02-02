using System.Globalization;
using CallbackDefs;
using UnityEngine;
using UnityEngine.UI;

public class InputPasswordUI : OrangeUIBase
{
	private enum PasswordMode
	{
		SETPASSWORD = 0,
		CHANGEPASSWORD = 1,
		DELETEACCOUNT = 2
	}

	[SerializeField]
	private Transform m_passwordSet;

	[SerializeField]
	private Button m_confirmBtn;

	[SerializeField]
	private Transform m_passwordChangeGroup;

	[SerializeField]
	private Transform m_passwordOld;

	[SerializeField]
	private Transform m_passwordNew;

	[SerializeField]
	private Transform m_passwordConfirm;

	private const string DELETE_ACCOUNT_PASSWORD = "ROCKMANXDIVE";

	[SerializeField]
	private Transform m_passwordDeleteAccountGroup;

	[SerializeField]
	private Transform m_descText;

	[SerializeField]
	private Text m_titleText;

	private InputField m_inputFieldDelete;

	private PasswordMode m_currentMode;

	private InputField m_inputFieldSet;

	private InputField m_inputFieldOld;

	private InputField m_inputFieldNew;

	private InputField m_inputFieldConfirm;
    [System.Obsolete]
    private CallbackObjs m_callback;

    [System.Obsolete]
    public void SetupDeletePassword(CallbackObjs p_cb)
	{
		m_passwordSet.gameObject.SetActive(false);
		m_passwordChangeGroup.gameObject.SetActive(false);
		m_passwordDeleteAccountGroup.gameObject.SetActive(true);
		m_currentMode = PasswordMode.DELETEACCOUNT;
		GetInputField();
		m_callback = p_cb;
		m_titleText.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("DELAC_TITLE");
	}

    [System.Obsolete]
    public void SetupNewPassword(CallbackObjs p_cb)
	{
		m_passwordSet.gameObject.SetActive(true);
		m_passwordChangeGroup.gameObject.SetActive(false);
		if ((bool)m_passwordDeleteAccountGroup)
		{
			m_passwordDeleteAccountGroup.gameObject.SetActive(false);
		}
		m_currentMode = PasswordMode.SETPASSWORD;
		GetInputField();
		m_callback = p_cb;
	}

    [System.Obsolete]
    public void SetupChangePassword(CallbackObjs p_cb)
	{
		m_passwordSet.gameObject.SetActive(false);
		m_passwordChangeGroup.gameObject.SetActive(true);
		if ((bool)m_passwordDeleteAccountGroup)
		{
			m_passwordDeleteAccountGroup.gameObject.SetActive(false);
		}
		m_currentMode = PasswordMode.CHANGEPASSWORD;
		GetInputField();
		m_callback = p_cb;
	}

	private void GetInputField()
	{
		m_inputFieldSet = m_passwordSet.GetComponentInChildren<InputField>();
		m_inputFieldOld = m_passwordOld.GetComponentInChildren<InputField>();
		m_inputFieldNew = m_passwordNew.GetComponentInChildren<InputField>();
		m_inputFieldConfirm = m_passwordConfirm.GetComponentInChildren<InputField>();
		m_inputFieldDelete = m_passwordDeleteAccountGroup.GetComponentInChildren<InputField>();
		if ((bool)m_inputFieldSet)
		{
			m_inputFieldSet.onValidateInput = CheckForEmoji;
		}
		if ((bool)m_inputFieldOld)
		{
			m_inputFieldOld.onValidateInput = CheckForEmoji;
		}
		if ((bool)m_inputFieldNew)
		{
			m_inputFieldNew.onValidateInput = CheckForEmoji;
		}
		if ((bool)m_inputFieldConfirm)
		{
			m_inputFieldConfirm.onValidateInput = CheckForEmoji;
		}
		if ((bool)m_inputFieldDelete)
		{
			m_inputFieldDelete.onValidateInput = CheckForEmoji;
		}
	}

	public void OnValueChangeSetPassword()
	{
		m_confirmBtn.interactable = !string.IsNullOrEmpty(m_inputFieldSet.text);
	}

	public void OnValueChangeChangePassword()
	{
		if (string.IsNullOrEmpty(m_inputFieldOld.text) || string.IsNullOrEmpty(m_inputFieldNew.text) || string.IsNullOrEmpty(m_inputFieldConfirm.text))
		{
			m_confirmBtn.interactable = false;
			return;
		}
		bool interactable = string.Compare(m_inputFieldNew.text, m_inputFieldConfirm.text) == 0;
		m_confirmBtn.interactable = interactable;
	}

	public void OnValueChangeDeleteAccount()
	{
		bool interactable = string.Compare(m_inputFieldDelete.text, "ROCKMANXDIVE", false) == 0;
		m_confirmBtn.interactable = interactable;
	}

	public void OnClickOKBtn()
	{
		if (m_currentMode == PasswordMode.SETPASSWORD)
		{
			m_callback.CheckTargetToInvoke(m_inputFieldSet.text);
		}
		else if (m_currentMode == PasswordMode.DELETEACCOUNT)
		{
			m_callback.CheckTargetToInvoke(m_inputFieldSet.text);
		}
		else
		{
			m_callback.CheckTargetToInvoke(m_inputFieldOld.text, m_inputFieldNew.text);
		}
		OnClickCloseBtn();
	}

	public override void OnClickCloseBtn()
	{
		base.OnClickCloseBtn();
	}

	private char CheckForEmoji(string test, int charIndex, char addedChar)
	{
		if (char.GetUnicodeCategory(addedChar) == UnicodeCategory.Surrogate)
		{
			return '\0';
		}
		return addedChar;
	}
}
