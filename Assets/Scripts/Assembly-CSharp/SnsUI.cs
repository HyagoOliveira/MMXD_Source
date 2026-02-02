using UnityEngine;
using UnityEngine.UI;

public class SnsUI : OrangeUIBase
{
	[SerializeField]
	private Button btnApple;

	[SerializeField]
	private Button btnLINE;

	[SerializeField]
	private Button btnFacebook;

	[SerializeField]
	private Button btnTwitter;

	[SerializeField]
	private Button btnSteam;

	[SerializeField]
	private Button btnGuest;

	public bool IsLoginSuccess { get; set; }

	protected override void Awake()
	{
		base.Awake();
		IsLoginSuccess = false;
		base.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
		btnLINE.gameObject.SetActive(false);
		btnFacebook.gameObject.SetActive(false);
		btnTwitter.gameObject.SetActive(false);
		btnGuest.gameObject.SetActive(false);
		btnSteam.gameObject.SetActive(true);
		if (!MonoBehaviourSingleton<AppleLoginManager>.Instance.IsSupportAppleLogin)
		{
			btnApple.gameObject.SetActive(false);
		}
	}

	public void OnClick_SNS_LINE()
	{
		ManagedSingleton<SNSHelper>.Instance.Login(SNS_TYPE.LINE, delegate(object success)
		{
			if ((bool)success)
			{
				OnClickCloseBtn();
				OpenSelectZoneUI();
			}
		});
	}

	public void OnClick_SNS_FACEBOOK()
	{
		ManagedSingleton<SNSHelper>.Instance.Login(SNS_TYPE.FACEBOOK, delegate(object success)
		{
			if ((bool)success)
			{
				OnClickCloseBtn();
				OpenSelectZoneUI();
			}
		});
	}

	public void OnClick_SNS_TWITTER()
	{
		ManagedSingleton<SNSHelper>.Instance.Login(SNS_TYPE.TWITTER, delegate(object success)
		{
			if ((bool)success)
			{
				OnClickCloseBtn();
				OpenSelectZoneUI();
			}
		});
	}

	public void OnClick_SNS_APPLE()
	{
		ManagedSingleton<SNSHelper>.Instance.Login(SNS_TYPE.APPLE, delegate(object success)
		{
			if ((bool)success)
			{
				OnClickCloseBtn();
				OpenSelectZoneUI();
			}
		});
	}

	public void OnClick_ORANGE_GUEST()
	{
		ManagedSingleton<SNSHelper>.Instance.Login(SNS_TYPE.STEAM, delegate(object success)
		{
			if ((bool)success)
			{
				OnClickCloseBtn();
				OpenSelectZoneUI();
			}
		});
	}

	public void OnClick_ORANGE_ACCOUNT_INHERIT()
	{
		ManagedSingleton<SNSHelper>.Instance.Login(SNS_TYPE.INHERIT, delegate(object success)
		{
			IsLoginSuccess = (bool)success;
			if ((bool)success)
			{
				base.CloseSE = SystemSE.NONE;
				OpenSelectZoneUI();
				OnClickCloseBtn();
			}
		});
	}

	private void OpenSelectZoneUI()
	{
		TitleNewUI uI = MonoBehaviourSingleton<UIManager>.Instance.GetUI<TitleNewUI>("UI_Title_3");
		if (uI != null)
		{
			uI.OnClickZoneSelectUI(false);
		}
	}

	private void SelectBestZone()
	{
		ManagedSingleton<SNSHelper>.Instance.SelectBestZone();
	}
}
