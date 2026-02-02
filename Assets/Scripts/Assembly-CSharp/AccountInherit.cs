using System.Globalization;
using CallbackDefs;
using OrangeApi;
using UnityEngine;
using UnityEngine.UI;
using enums;

public class AccountInherit : OrangeUIBase
{
	[SerializeField]
	private InputField m_inputAccount;

	[SerializeField]
	private InputField m_inputPassword;
    [System.Obsolete]
    public CallbackObjs OnAccountInheritSuccess;

	private void Start()
	{
	}

	public void Setup()
	{
		m_inputPassword.onValidateInput = CheckForEmoji;
	}

	public void OnClickOKBtn()
	{
		if (string.IsNullOrEmpty(m_inputAccount.text) || string.IsNullOrEmpty(m_inputPassword.text))
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_ERROR);
			return;
		}
		ManagedSingleton<SNSHelper>.Instance.RetrieveSNSLinkUIDReq(m_inputAccount.text, m_inputPassword.text, AccountSourceType.Unity, delegate(object p_param)
		{
			if (((RetrieveSNSLinkUIDRes)p_param).Code == 40200)
			{
				ConfirmInheritAccount((RetrieveSNSLinkUIDRes)p_param, m_inputPassword.text, m_inputAccount.text);
			}
			else
			{
				MonoBehaviourSingleton<UIManager>.Instance.LoadResourceUI("UI_CommonMsg", delegate(CommonUI ui)
				{
					ui.OpenSE = SystemSE.CRI_SYSTEMSE_SYS_ERROR;
					ui.SetupConfirm(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_TIP"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("INHERIT_FAILED"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_OK"), delegate
					{
						MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL);
					});
				});
			}
		});
	}

	private void ConfirmInheritAccount(RetrieveSNSLinkUIDRes res, string password, string inheritCode)
	{
		MonoBehaviourSingleton<UIManager>.Instance.LoadResourceUI("UI_CommonMsg", delegate(CommonUI ui)
		{
			ui.SetupYesNO(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_TIP"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("INHERIT_WARNING"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_YES"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_NO"), delegate
			{
				ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_OK17;
				MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.UID = res.UniqueIdentifier;
				MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.Birth = res.BirthTime;
				MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.Locate = res.Region;
				MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.InheritCode = inheritCode.ToUpper();
				MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.Save();
				MonoBehaviourSingleton<UIManager>.Instance.CloseUI(this);
				OnAccountInheritSuccess.CheckTargetToInvoke(res.UniqueIdentifier, password);
			});
		});
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
