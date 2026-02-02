using UnityEngine;

public class TipResultUI : OrangeUIBase
{
	[SerializeField]
	private OrangeText textTip;

	public void Setup(string msg)
	{
		textTip.text = msg;
		textTip.alignByGeometry = false;
		base.CloseSE = SystemSE.NONE;
		LeanTween.delayedCall(base.gameObject, 3f, OnClickCloseBtn);
		LeanTween.delayedCall(base.gameObject, 0.2f, PlayEffectSE);
		LeanTween.delayedCall(base.gameObject, 2.6f, PlayCloseSE);
	}

	public void PlayCloseSE()
	{
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL);
	}

	public void PlayEffectSE()
	{
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_EFFECT04);
	}
}
