using CallbackDefs;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.UI;

public class EPRatioDialog : OrangeUIBase
{
	[SerializeField]
	private Slider m_valueSlider;

	[SerializeField]
	private Button m_confirmBtn;

	[SerializeField]
	private OrangeText m_sliderText;

	[SerializeField]
	private OrangeText m_itemValueBefore;

	[SerializeField]
	private OrangeText m_itemValueAfter;

	[BoxGroup("Sound")]
	[SerializeField]
	private SystemSE m_okSE;

	[BoxGroup("Sound")]
	[SerializeField]
	private SystemSE m_btnSE;

	private int m_epRatioMax = 5;

	private int m_epCost;

	private STAGE_TABLE m_stageTable;

	private Callback m_callback;

	public void Setup(STAGE_TABLE stageTable, Callback callback = null)
	{
		m_stageTable = stageTable;
		m_epCost = stageTable.n_AP;
		m_valueSlider.value = 0f;
		UpdateEPRatio();
	}

	private int GetUsableMaxEPRatio()
	{
		int num = 0;
		int eventStamina = ManagedSingleton<PlayerHelper>.Instance.GetEventStamina();
		if (m_epCost != 0)
		{
			num = (int)Mathf.Floor(eventStamina / m_epCost);
		}
		if (num > OrangeConst.EPCOST_RATIO_MAX)
		{
			num = OrangeConst.EPCOST_RATIO_MAX;
		}
		return num;
	}

	private void UpdateEPRatio()
	{
		m_valueSlider.maxValue = GetUsableMaxEPRatio();
		if (m_valueSlider.value > m_valueSlider.maxValue)
		{
			m_valueSlider.value = m_valueSlider.maxValue;
		}
		int eventStamina = ManagedSingleton<PlayerHelper>.Instance.GetEventStamina();
		m_itemValueBefore.text = eventStamina.ToString();
		m_itemValueAfter.text = ((float)eventStamina - m_valueSlider.value * (float)m_epCost).ToString();
		m_sliderText.text = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("EPCOST_RATIO_SELECT"), m_valueSlider.value);
		m_confirmBtn.interactable = m_valueSlider.value > 0f;
	}

	public void OnClickPlusBtn()
	{
		if (m_valueSlider.value + 1f <= m_valueSlider.maxValue)
		{
			m_valueSlider.value += 1f;
			UpdateEPRatio();
		}
	}

	public void OnClickMinusBtn()
	{
		if (m_valueSlider.value - 1f >= m_valueSlider.minValue)
		{
			m_valueSlider.value -= 1f;
			UpdateEPRatio();
		}
	}

	public void OnClickMaxBtn()
	{
		if (m_valueSlider.value != m_valueSlider.maxValue)
		{
			m_valueSlider.value = m_valueSlider.maxValue;
			UpdateEPRatio();
		}
	}

	public void OnClickMinBtn()
	{
		if (m_valueSlider.value != m_valueSlider.minValue)
		{
			m_valueSlider.value = m_valueSlider.minValue;
			UpdateEPRatio();
		}
	}

	public void OnChangeSlider()
	{
		UpdateEPRatio();
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(m_btnSE);
	}

	public void OnClickOkBtn()
	{
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(m_okSE);
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_GoCheck", delegate(GoCheckUI ui)
		{
			ui.Setup(m_stageTable);
			ui.SetEPRatio((int)m_valueSlider.value);
			ManagedSingleton<StageHelper>.Instance.nStageEndGoUI = StageHelper.STAGE_END_GO.ACTIVITYEVENT;
			ManagedSingleton<StageHelper>.Instance.activityEventStageMainID = m_stageTable.n_MAIN;
		});
	}

	public override void OnClickCloseBtn()
	{
		if (m_callback != null)
		{
			m_callback.CheckTargetToInvoke();
		}
		base.OnClickCloseBtn();
	}
}
