using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class GuideUiUnit : ScrollIndexCallback
{
	[SerializeField]
	private Image icon;

	[SerializeField]
	private OrangeText textTitle;

	[SerializeField]
	private OrangeText textTip;

	[SerializeField]
	private Image rank;

	[SerializeField]
	private Image imgProgressFg;

	[SerializeField]
	private OrangeText textProgressValue;

	[SerializeField]
	private GameObject groupGuideVisable;

	[SerializeField]
	private GameObject groupGuideDisable;

	[SerializeField]
	private OrangeText textLockMsg;

	[SerializeField]
	private GuideUI parent;

	private GuideUI.GuidePowerUnitInfo powerInfo;

	private GuideUI.GuideObtainUnitInfo obtainInfo;

	[SerializeField]
	private GameObject[] arrPowerGo;

	private RectTransform rt;

	private bool isCalling;

	private void Awake()
	{
		rt = GetComponent<RectTransform>();
	}

	public override void ScrollCellIndex(int p_idx)
	{
		switch (parent._GuideType)
		{
		case GuideUI.GuideType.POWER:
		{
			obtainInfo = null;
			GameObject[] array = arrPowerGo;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].SetActive(true);
			}
			powerInfo = parent.GetPowerChildInfo(p_idx);
			textTitle.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr(powerInfo.GuideTable.w_TITLE);
			textTip.text = string.Empty;
			if (powerInfo.IsVaild)
			{
				textProgressValue.text = powerInfo.Power.ToString();
				imgProgressFg.fillAmount = parent.SetRankProgress(powerInfo.Power, powerInfo.SuggestPower, ref rank);
			}
			else
			{
				textLockMsg.text = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("GUIDE_OPEN_RANK"), powerInfo.GuideTable.n_PLAYER_RANK);
			}
			groupGuideVisable.SetActive(powerInfo.IsVaild);
			groupGuideDisable.SetActive(!powerInfo.IsVaild);
			icon.sprite = MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssstSync<Sprite>("ui/UI_Guide", powerInfo.GuideTable.s_ICON);
			break;
		}
		case GuideUI.GuideType.OBTAIN:
		{
			powerInfo = null;
			groupGuideVisable.SetActive(true);
			groupGuideDisable.SetActive(false);
			GameObject[] array = arrPowerGo;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].SetActive(false);
			}
			textProgressValue.text = string.Empty;
			obtainInfo = parent.GetObtainChildInfo(p_idx);
			textTitle.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr(obtainInfo.GuideTable.w_TITLE);
			textTip.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr(obtainInfo.GuideTable.w_TIP);
			icon.sprite = MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssstSync<Sprite>("ui/UI_Guide", obtainInfo.GuideTable.s_ICON);
			UpdateActiveState(parent.ToggleSelectIdx);
			break;
		}
		}
		icon.SetNativeSize();
	}

	public bool UpdateActiveState(int toggleIdx)
	{
		if (obtainInfo == null)
		{
			return false;
		}
		base.gameObject.SetActive(toggleIdx == obtainInfo.GuideTable.n_TYPE);
		if (!base.gameObject.activeSelf)
		{
			rt.RebuildLayout();
			return false;
		}
		return true;
	}

	public void OnClickGoBtn()
	{
		if (isCalling)
		{
			return;
		}
		if (powerInfo != null)
		{
			if (powerInfo.ClickCB == null)
			{
				MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_ERROR);
			}
			else
			{
				MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK13);
			}
		}
		else if (obtainInfo.ClickCB == null)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_ERROR);
		}
		else
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK13);
		}
		isCalling = true;
		OrangeUIAnimation component = GetComponent<OrangeUIAnimation>();
		if ((bool)component)
		{
			component.PlayAnimation(GoClickCB);
		}
		else
		{
			GoClickCB();
		}
	}

	private void GoClickCB()
	{
		switch (parent._GuideType)
		{
		case GuideUI.GuideType.POWER:
			powerInfo.ClickCB.CheckTargetToInvoke();
			break;
		case GuideUI.GuideType.OBTAIN:
			obtainInfo.ClickCB.CheckTargetToInvoke();
			break;
		}
		isCalling = false;
	}
}
