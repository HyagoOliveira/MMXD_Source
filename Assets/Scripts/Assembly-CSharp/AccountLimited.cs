using System;
using UnityEngine;
using UnityEngine.UI;
using enums;

public class AccountLimited : OrangeUIBase
{
	[SerializeField]
	private InputField m_inputAccount;

	[SerializeField]
	private InputField m_inputPassword;

	[SerializeField]
	private OrangeText m_errorMessage;

	public Action OnAccountLimitedSuccess;

	private void Start()
	{
	}

	public void Setup()
	{
	}

	public override void OnClickCloseBtn()
	{
		base.OnClickCloseBtn();
	}

	public void OnClickOKBtn()
	{
		if (string.IsNullOrEmpty(m_inputAccount.text))
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_ERROR);
			return;
		}
		ManagedSingleton<PlayerNetManager>.Instance.AccountInfo.ID = m_inputAccount.text;
		ManagedSingleton<PlayerNetManager>.Instance.AccountInfo.Secret = m_inputPassword.text;
		ManagedSingleton<PlayerNetManager>.Instance.AccountInfo.SourceType = AccountSourceType.Limited;
		OnAccountLimitedSuccess();
		base.CloseSE = SystemSE.NONE;
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK17);
		MonoBehaviourSingleton<UIManager>.Instance.CloseUI(this);
	}
}
