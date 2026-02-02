using CallbackDefs;
using UnityEngine;
using UnityEngine.UI;

public class DNAPurchase : OrangeUIBase
{
	[SerializeField]
	private Button shopButton;

	[SerializeField]
	private Button convertButton;

	private Callback _cbShop;

	private Callback _cbConvert;

	private Callback _cbClose;

	public SystemSE OpenSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP;

	public SystemSE YesSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;

	public SystemSE NoSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;

	public bool MuteSE;

	public void Setup(Callback cbShop, Callback cbConvert, Callback cbClose)
	{
		_cbShop = cbShop;
		_cbConvert = cbConvert;
		_cbClose = cbClose;
		if (MonoBehaviourSingleton<AudioManager>.Instance.IsInitSystemSE)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(OpenSE);
		}
		base._EscapeEvent = EscapeEvent.CLOSE_UI;
	}

	public void OnClickShop()
	{
		if (!MonoBehaviourSingleton<AudioManager>.Instance.IsInitSystemSE || MuteSE)
		{
			base.CloseSE = SystemSE.NONE;
		}
		else
		{
			base.CloseSE = YesSE;
		}
		_cbShop.CheckTargetToInvoke();
		base.OnClickCloseBtn();
	}

	public void OnClickConvert()
	{
		if (!MonoBehaviourSingleton<AudioManager>.Instance.IsInitSystemSE || MuteSE)
		{
			base.CloseSE = SystemSE.NONE;
		}
		else
		{
			base.CloseSE = YesSE;
		}
		_cbConvert.CheckTargetToInvoke();
		base.OnClickCloseBtn();
	}

	public override void OnClickCloseBtn()
	{
		base.CloseSE = (MonoBehaviourSingleton<AudioManager>.Instance.IsInitSystemSE ? SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL : SystemSE.NONE);
		if (MuteSE)
		{
			base.CloseSE = SystemSE.NONE;
		}
		_cbClose.CheckTargetToInvoke();
		base.OnClickCloseBtn();
	}
}
