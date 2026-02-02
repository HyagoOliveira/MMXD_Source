using UnityEngine;

public class TermsOfUseUI : OrangeUIBase
{
	[SerializeField]
	private OrangeText textLocate;

	private AREA_TABLE area;

	public void Setup(AREA_TABLE p_area = null)
	{
		base.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
		if (p_area == null)
		{
			MonoBehaviourSingleton<UIManager>.Instance.Connecting(true);
			MonoBehaviourSingleton<LocateManager>.Instance.FindLocate(delegate(object p_param)
			{
				area = (AREA_TABLE)p_param;
				UpdateLocateText();
				MonoBehaviourSingleton<UIManager>.Instance.Connecting(false);
			}, LocateManager.LocaleTarget.BelongAccountRegion);
		}
		else
		{
			area = p_area;
			UpdateLocateText();
		}
	}

	private void UpdateLocateText()
	{
		textLocate.text = ManagedSingleton<OrangeTextDataManager>.Instance.AREATEXT_TABLE_DICT.GetL10nValue(area.s_TEXT);
	}

	public void OnClickOpenLocAndCountry()
	{
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_Locate", delegate(LocateUI ui)
		{
			ui.Setup(area.n_ID);
		});
	}

	public void OnClickRuleBtn()
	{
		Application.OpenURL(string.Format(ManagedSingleton<ServerConfig>.Instance.ServerSetting.Platform.Service, area.s_RULE).Replace(" ", "%20"));
	}

	public void OnClickPolicyBtn()
	{
		Application.OpenURL(string.Format(ManagedSingleton<ServerConfig>.Instance.ServerSetting.Platform.Policy, area.s_PRIVACY).Replace(" ", "%20"));
	}

	public void OnClickOK()
	{
		MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.Locate = area.s_CODE;
		base.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_OK17;
		OnClickCloseBtn();
	}
}
