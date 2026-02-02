using System;
using System.Linq;
using CallbackDefs;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.UI;

public class SweepDialog : OrangeUIBase
{
	[SerializeField]
	private Slider m_valueSlider;

	[SerializeField]
	private Button m_confirmBtn;

	[SerializeField]
	private OrangeText m_sliderText;

	[SerializeField]
	private OrangeText m_item1ValueBefore;

	[SerializeField]
	private OrangeText m_item1ValueAfter;

	[SerializeField]
	private OrangeText m_item2ValueBefore;

	[SerializeField]
	private OrangeText m_item2ValueAfter;

	[SerializeField]
	private OrangeText m_sweepCountText;

	[SerializeField]
	private OrangeText m_skipDiveProgramText;

	[SerializeField]
	private OrangeText m_eventStaminaText;

	[SerializeField]
	private Image m_skipDiveIcon;

	[SerializeField]
	private Image m_eventAPIcon;

	[BoxGroup("Sound")]
	[SerializeField]
	private SystemSE m_okSE;

	[BoxGroup("Sound")]
	[SerializeField]
	private SystemSE m_btnSE;

	private STAGE_TABLE m_stageTable;

	private StageInfo m_stageInfo;

	private ITEM_TABLE m_skipDiveTable;

	private ITEM_TABLE m_eventStaminaTable;

	private ITEM_TABLE m_dailyChallengeTable;

	private Callback m_callback;

	public void Setup(STAGE_TABLE stageTable, Callback callback = null)
	{
		m_stageTable = stageTable;
		m_callback = callback;
		ManagedSingleton<PlayerNetManager>.Instance.dicStage.TryGetValue(stageTable.n_ID, out m_stageInfo);
		ManagedSingleton<ExtendDataHelper>.Instance.ITEM_TABLE_DICT.TryGetValue(OrangeConst.ITEMID_SWEEP, out m_skipDiveTable);
		ManagedSingleton<ExtendDataHelper>.Instance.ITEM_TABLE_DICT.TryGetValue(OrangeConst.ITEMID_EVENTAP, out m_eventStaminaTable);
		ManagedSingleton<ExtendDataHelper>.Instance.ITEM_TABLE_DICT.TryGetValue(OrangeConst.ITEMID_CHALLENGE_COUNT, out m_dailyChallengeTable);
		m_skipDiveProgramText.text = ManagedSingleton<OrangeTextDataManager>.Instance.ITEMTEXT_TABLE_DICT.GetL10nValue(m_skipDiveTable.w_NAME);
		if (m_stageTable.n_TYPE == 4)
		{
			m_eventStaminaText.text = ManagedSingleton<OrangeTextDataManager>.Instance.ITEMTEXT_TABLE_DICT.GetL10nValue(m_eventStaminaTable.w_NAME);
			MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(AssetBundleScriptableObject.Instance.GetIconItem(m_eventStaminaTable.s_ICON), m_eventStaminaTable.s_ICON, delegate(Sprite Obj)
			{
				m_eventAPIcon.sprite = Obj;
			});
		}
		else
		{
			m_eventStaminaText.text = ManagedSingleton<OrangeTextDataManager>.Instance.ITEMTEXT_TABLE_DICT.GetL10nValue(m_dailyChallengeTable.w_NAME);
			MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(AssetBundleScriptableObject.Instance.GetIconItem(m_dailyChallengeTable.s_ICON), m_dailyChallengeTable.s_ICON, delegate(Sprite Obj)
			{
				m_eventAPIcon.sprite = Obj;
			});
		}
		m_valueSlider.value = 0f;
		UpdateSweepCount();
		base._EscapeEvent = EscapeEvent.CLOSE_UI;
	}

	private int GetMaxSweepCount()
	{
		int itemValue = ManagedSingleton<PlayerHelper>.Instance.GetItemValue(OrangeConst.ITEMID_SWEEP);
		int eventStamina = ManagedSingleton<PlayerHelper>.Instance.GetEventStamina();
		int result = 0;
		if (m_stageTable.n_TYPE == 4 && m_stageTable.n_AP != 0)
		{
			result = (int)Mathf.Min(itemValue, Mathf.Floor(eventStamina / m_stageTable.n_AP));
		}
		else if (m_stageInfo != null)
		{
			result = Mathf.Min(itemValue, ManagedSingleton<StageHelper>.Instance.GetAvailableChallengeCount(m_stageTable));
		}
		return result;
	}

	private void UpdateSweepCount()
	{
		m_valueSlider.maxValue = GetMaxSweepCount();
		if (m_valueSlider.value > m_valueSlider.maxValue)
		{
			m_valueSlider.value = m_valueSlider.maxValue;
		}
		int itemValue = ManagedSingleton<PlayerHelper>.Instance.GetItemValue(OrangeConst.ITEMID_SWEEP);
		m_item1ValueBefore.text = itemValue.ToString();
		m_item1ValueAfter.text = ((float)itemValue - m_valueSlider.value).ToString();
		m_sliderText.text = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("SWEEP_COUNT_SELECT"), m_valueSlider.value);
		if (m_stageTable.n_TYPE == 4)
		{
			int eventStamina = ManagedSingleton<PlayerHelper>.Instance.GetEventStamina();
			m_item2ValueBefore.text = eventStamina.ToString();
			m_item2ValueAfter.text = ((float)eventStamina - m_valueSlider.value * (float)m_stageTable.n_AP).ToString();
		}
		else if (m_stageInfo != null)
		{
			StageInfo value = null;
			int num = 0;
			foreach (STAGE_TABLE item in ManagedSingleton<OrangeDataManager>.Instance.STAGE_TABLE_DICT.Values.Where((STAGE_TABLE x) => x.n_TYPE == 2 && x.n_MAIN == m_stageTable.n_MAIN).ToList())
			{
				if (ManagedSingleton<PlayerNetManager>.Instance.dicStage.TryGetValue(item.n_ID, out value))
				{
					num += value.netStageInfo.ClearCount;
				}
			}
			m_item2ValueBefore.text = (m_stageTable.n_PLAY_COUNT - num).ToString();
			m_item2ValueAfter.text = ((float)(m_stageTable.n_PLAY_COUNT - num) - m_valueSlider.value).ToString();
		}
		m_confirmBtn.interactable = m_valueSlider.value > 0f;
	}

	public void OnClickPlusBtn()
	{
		if (m_valueSlider.value + 1f <= m_valueSlider.maxValue)
		{
			m_valueSlider.value += 1f;
			UpdateSweepCount();
		}
	}

	public void OnClickMinusBtn()
	{
		if (m_valueSlider.value - 1f >= m_valueSlider.minValue)
		{
			m_valueSlider.value -= 1f;
			UpdateSweepCount();
		}
	}

	public void OnClickMaxBtn()
	{
		if (m_valueSlider.value != m_valueSlider.maxValue)
		{
			m_valueSlider.value = m_valueSlider.maxValue;
			UpdateSweepCount();
		}
	}

	public void OnClickMinBtn()
	{
		if (m_valueSlider.value != m_valueSlider.minValue)
		{
			m_valueSlider.value = m_valueSlider.minValue;
			UpdateSweepCount();
		}
	}

	public void OnChangeSlider()
	{
		UpdateSweepCount();
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(m_btnSE);
	}

	public void OnClickOkBtn()
	{
		if (ManagedSingleton<EquipHelper>.Instance.ShowEquipmentLimitReachedDialog())
		{
			return;
		}
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(m_okSE);
		MonoBehaviourSingleton<OrangeGameManager>.Instance.StageSweepReq(m_stageTable.n_ID, (int)m_valueSlider.value, delegate(object res)
		{
			NetRewardsEntity reward = res as NetRewardsEntity;
			if (reward.RewardList.Count > 0)
			{
				MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_RewardPopup", delegate(RewardPopopUI ui)
				{
					if (MonoBehaviourSingleton<OrangeGameManager>.Instance.IsLvUp)
					{
						ui.closeCB = (Callback)Delegate.Combine(ui.closeCB, new Callback(SweepComplete));
					}
					ui.Setup(reward.RewardList);
				});
			}
			else
			{
				SweepComplete();
			}
			if (GetMaxSweepCount() == 0)
			{
				OnClickCloseBtn();
			}
			else
			{
				UpdateSweepCount();
			}
			Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.CHARGE_STAMINA);
		});
	}

	private void SweepComplete()
	{
		MonoBehaviourSingleton<OrangeGameManager>.Instance.DisplayLvPerform(delegate
		{
			OnClickCloseBtn();
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
