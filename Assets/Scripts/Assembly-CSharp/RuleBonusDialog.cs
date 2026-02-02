using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.UI;
using enums;

public class RuleBonusDialog : OrangeUIBase
{
	private enum RuleType
	{
		RULE = 0,
		BONUS = 1,
		CHALLENGE = 2,
		NONE = 3
	}

	[SerializeField]
	private ScrollRect m_scrollViewRect;

	[SerializeField]
	private Transform[] m_labelRankRef;

	[SerializeField]
	private RewardPopupUIUnit m_iconRef;

	[SerializeField]
	private WrapRectComponent m_wrapRect;

	[SerializeField]
	private Toggle m_ruleTab;

	[SerializeField]
	private Toggle m_bonusTab;

	[SerializeField]
	private Toggle m_challengeTab;

	[SerializeField]
	private OrangeText m_bossRushTip;

	[SerializeField]
	private Transform m_bossRushTipGroup;

	[BoxGroup("Sound")]
	[SerializeField]
	private SystemSE m_clickTapSE;

	private const int m_maxIconPerRow = 5;

	private bool m_bAlignLeft = true;

	private Vector2 m_currentPos = new Vector2(0f, 0f);

	private Vector2 m_spacing = new Vector2(25f, 15f);

	private int m_rowCount;

	private List<EVENT_TABLE> eventTable_list = new List<EVENT_TABLE>();

	private bool m_bIgnoreToggleSE;

	private BossRushInfo m_bossRushInfo;

	private STAGE_TABLE m_stageTable;

	private RuleType _currentRuleType = RuleType.NONE;

	public void Setup(STAGE_TABLE stageTable)
	{
		m_stageTable = stageTable;
		int num = 0;
		int currentIconIndex = 0;
		m_scrollViewRect.content.sizeDelta = new Vector2(m_scrollViewRect.content.sizeDelta.x, 0f);
		m_challengeTab.gameObject.SetActive(false);
		if (stageTable.n_TYPE == 8)
		{
			m_scrollViewRect.GetComponent<RectTransform>().sizeDelta = new Vector2(m_scrollViewRect.GetComponent<RectTransform>().sizeDelta.x, 470f);
			SetToggleText(m_bonusTab, "POINT_ADDITION");
			m_bonusTab.interactable = true;
			IEnumerable<EVENT_TABLE> source = ManagedSingleton<ExtendDataHelper>.Instance.EVENT_TABLE_DICT.Values.Where((EVENT_TABLE x) => x.n_TYPE == 11 && x.n_TYPE_X == 8 && x.n_TYPE_Y == stageTable.n_MAIN);
			if (source.Count() > 0)
			{
				m_bonusTab.interactable = source.ElementAt(0).n_POINT != 0;
				if (ManagedSingleton<PlayerNetManager>.Instance.dicBossRushInfo.ContainsKey(source.ElementAt(0).n_ID))
				{
					m_bossRushInfo = ManagedSingleton<PlayerNetManager>.Instance.dicBossRushInfo[source.ElementAt(0).n_ID];
					Vector2 sizeDelta = m_iconRef.GetComponent<RectTransform>().sizeDelta;
					AddLabel(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("CLEAR_CHARACTER"));
					AddBossRushUsedCharacterIcon(ref currentIconIndex, sizeDelta, m_bossRushInfo.netBRInfo.UsedCharacterIDList);
					AddLabel(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("CLEAR_WEAPON"));
					AddBossRushUsedWeaponIcon(ref currentIconIndex, sizeDelta, m_bossRushInfo.netBRInfo.UsedWeaponIDList);
				}
			}
		}
		else
		{
			SetToggleText(m_bonusTab, "EVENT_BONUS");
			if (m_stageTable.n_SUB == 2)
			{
				m_challengeTab.gameObject.SetActive(true);
				SetToggleText(m_challengeTab, "SPEEDRUN_CHALLENGE");
			}
			IEnumerable<EVENT_TABLE> enumerable = null;
			switch (stageTable.n_TYPE)
			{
			case 4:
				enumerable = ManagedSingleton<ExtendDataHelper>.Instance.EVENT_TABLE_DICT.Values.Where((EVENT_TABLE x) => x.n_TYPE == 4 && x.n_TYPE_X == 4 && x.n_TYPE_Y == stageTable.n_MAIN);
				break;
			case 9:
				enumerable = ManagedSingleton<ExtendDataHelper>.Instance.EVENT_TABLE_DICT.Values.Where((EVENT_TABLE x) => x.n_TYPE == 4 && x.n_TYPE_X == 9 && x.n_TYPE_Y == stageTable.n_MAIN);
				break;
			case 10:
				enumerable = ManagedSingleton<ExtendDataHelper>.Instance.EVENT_TABLE_DICT.Values.Where((EVENT_TABLE x) => x.n_TYPE == 4 && x.n_TYPE_X == 10 && x.n_TYPE_Y == stageTable.n_MAIN);
				break;
			case 11:
			case 12:
			case 13:
				enumerable = ManagedSingleton<ExtendDataHelper>.Instance.EVENT_TABLE_DICT.Values.Where((EVENT_TABLE x) => x.n_TYPE == 4 && x.n_TYPE_X == stageTable.n_TYPE && x.n_TYPE_Y == stageTable.n_MAIN);
				break;
			default:
				enumerable = ManagedSingleton<ExtendDataHelper>.Instance.EVENT_TABLE_DICT.Values.Where((EVENT_TABLE x) => x.n_TYPE == 4 && x.n_TYPE_X == 4 && x.n_TYPE_Y == stageTable.n_MAIN);
				break;
			}
			m_bonusTab.interactable = enumerable.Count() > 0;
			foreach (EVENT_TABLE item in (from x in enumerable.Where((EVENT_TABLE x) => x.n_BONUS_TYPE == 6).ToList()
				orderby x.n_BONUS_RATE descending, x.n_ID
				select x).ToList())
			{
				if (item.n_BONUS_RATE != num)
				{
					ITEM_TABLE value = null;
					ManagedSingleton<ExtendDataHelper>.Instance.ITEM_TABLE_DICT.TryGetValue(item.n_DROP_ITEM, out value);
					string l10nValue = ManagedSingleton<OrangeTextDataManager>.Instance.ITEMTEXT_TABLE_DICT.GetL10nValue(value.w_NAME);
					num = item.n_BONUS_RATE;
					string labelText = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("EVENT_DROP_BOOST"), l10nValue, num - 100);
					AddLabel(labelText);
				}
				AddIcon(item);
			}
			List<EVENT_TABLE> list = (from x in enumerable.Where((EVENT_TABLE x) => x.n_BONUS_TYPE == 7).ToList()
				orderby x.n_BONUS_RATE descending, x.n_ID
				select x).ToList();
			num = 0;
			foreach (EVENT_TABLE item2 in list)
			{
				if (item2.n_BONUS_RATE != num)
				{
					num = item2.n_BONUS_RATE;
					string labelText2 = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("EVENT_DAMAGE_BOOST"), num - 100);
					AddLabel(labelText2);
				}
				AddIcon(item2);
			}
		}
		OnClickRuleToggle(false);
		_currentRuleType = RuleType.RULE;
		base._EscapeEvent = EscapeEvent.CLOSE_UI;
	}

	private void AddBossRushUsedCharacterIcon(ref int currentIconIndex, Vector2 iconSizeDelta, List<string> usedCharacterIDList)
	{
		foreach (string usedCharacterID in usedCharacterIDList)
		{
			int num = int.Parse(usedCharacterID);
			if (num == 0)
			{
				currentIconIndex++;
				continue;
			}
			CHARACTER_TABLE value = null;
			ManagedSingleton<OrangeDataManager>.Instance.CHARACTER_TABLE_DICT.TryGetValue(num, out value);
			RewardPopupUIUnit rewardPopupUIUnit = Object.Instantiate(m_iconRef, m_scrollViewRect.content);
			rewardPopupUIUnit.transform.localPosition = m_currentPos;
			rewardPopupUIUnit.Setup(currentIconIndex, AssetBundleScriptableObject.Instance.GetIconCharacter("icon_" + value.s_ICON), "icon_" + value.s_ICON, value.n_RARITY, 0, OnClickUnitBossRush);
			rewardPopupUIUnit.IgonreTween();
			currentIconIndex++;
			m_scrollViewRect.content.sizeDelta = new Vector2(m_scrollViewRect.content.sizeDelta.x, Mathf.Abs(m_currentPos.y - iconSizeDelta.y - m_spacing.y));
			m_rowCount++;
			if (m_rowCount < 5)
			{
				m_currentPos.x += iconSizeDelta.x + m_spacing.x;
				m_bAlignLeft = false;
				continue;
			}
			m_currentPos.x = 0f;
			m_currentPos.y -= iconSizeDelta.y + m_spacing.y;
			m_bAlignLeft = true;
			m_rowCount = 0;
		}
	}

	private void AddBossRushUsedWeaponIcon(ref int currentIconIndex, Vector2 iconSizeDelta, List<string> usedWeaponIDList)
	{
		string iconWeapon = AssetBundleScriptableObject.Instance.m_iconWeapon;
		foreach (string usedWeaponID in usedWeaponIDList)
		{
			int num = int.Parse(usedWeaponID);
			if (num == 0)
			{
				currentIconIndex++;
				continue;
			}
			WEAPON_TABLE value = null;
			ManagedSingleton<OrangeDataManager>.Instance.WEAPON_TABLE_DICT.TryGetValue(num, out value);
			RewardPopupUIUnit rewardPopupUIUnit = Object.Instantiate(m_iconRef, m_scrollViewRect.content);
			rewardPopupUIUnit.transform.localPosition = m_currentPos;
			rewardPopupUIUnit.Setup(currentIconIndex, iconWeapon, value.s_ICON, value.n_RARITY, 0, OnClickUnitBossRush);
			rewardPopupUIUnit.IgonreTween();
			currentIconIndex++;
			m_scrollViewRect.content.sizeDelta = new Vector2(m_scrollViewRect.content.sizeDelta.x, Mathf.Abs(m_currentPos.y - iconSizeDelta.y - m_spacing.y));
			m_rowCount++;
			if (m_rowCount < 5)
			{
				m_currentPos.x += iconSizeDelta.x + m_spacing.x;
				m_bAlignLeft = false;
				continue;
			}
			m_currentPos.x = 0f;
			m_currentPos.y -= iconSizeDelta.y + m_spacing.y;
			m_bAlignLeft = true;
			m_rowCount = 0;
		}
	}

	private void SetToggleText(Toggle toggle, string key)
	{
		OrangeText[] componentsInChildren = toggle.GetComponentsInChildren<OrangeText>(true);
		foreach (OrangeText obj in componentsInChildren)
		{
			bool activeSelf = obj.gameObject.activeSelf;
			obj.gameObject.SetActive(true);
			obj.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr(key);
			obj.gameObject.SetActive(activeSelf);
		}
	}

	public void OnClickRuleToggle(bool bPlaySE = true)
	{
		if (_currentRuleType != 0)
		{
			if (bPlaySE && !m_bIgnoreToggleSE)
			{
				MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(m_clickTapSE);
			}
			m_bossRushTipGroup.gameObject.SetActive(false);
			m_scrollViewRect.gameObject.SetActive(false);
			m_wrapRect.gameObject.SetActive(true);
			_currentRuleType = RuleType.RULE;
			if (m_stageTable.w_TIP == "null")
			{
				m_wrapRect.SetText(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("EVENT_STAGE_RULE_LIMITED"));
			}
			else
			{
				m_wrapRect.SetText(ManagedSingleton<OrangeTextDataManager>.Instance.STAGETEXT_TABLE_DICT.GetL10nValue(m_stageTable.w_TIP));
			}
			m_bIgnoreToggleSE = false;
		}
	}

	public void OnClickBonusToggle(bool bPlaySE = true)
	{
		if (_currentRuleType != RuleType.BONUS)
		{
			if (bPlaySE && !m_bIgnoreToggleSE)
			{
				MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(m_clickTapSE);
			}
			if (m_stageTable.n_TYPE == 8)
			{
				m_bossRushTipGroup.gameObject.SetActive(true);
				m_bossRushTip.text = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("POINT_ADDITION_TIP"), OrangeConst.BOSSRUSH_BOOST);
			}
			m_scrollViewRect.gameObject.SetActive(true);
			m_wrapRect.gameObject.SetActive(false);
			_currentRuleType = RuleType.BONUS;
			m_bIgnoreToggleSE = false;
		}
	}

	public void OnClickChallengeToggle(bool bPlaySE = true)
	{
		if (_currentRuleType != RuleType.CHALLENGE)
		{
			if (bPlaySE && !m_bIgnoreToggleSE)
			{
				MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(m_clickTapSE);
			}
			m_bossRushTipGroup.gameObject.SetActive(false);
			m_scrollViewRect.gameObject.SetActive(false);
			m_wrapRect.gameObject.SetActive(true);
			_currentRuleType = RuleType.CHALLENGE;
			m_wrapRect.SetText(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("CHALLENGE_STAGE_TIP"));
			m_bIgnoreToggleSE = false;
		}
	}

	private void AddLabel(string labelText)
	{
		Transform obj = Object.Instantiate(m_labelRankRef[3], m_scrollViewRect.content);
		Vector2 sizeDelta = obj.GetComponent<RectTransform>().sizeDelta;
		OrangeText componentInChildren = obj.GetComponentInChildren<OrangeText>();
		if ((bool)componentInChildren)
		{
			componentInChildren.text = labelText;
		}
		if (!m_bAlignLeft)
		{
			Vector2 sizeDelta2 = m_iconRef.GetComponent<RectTransform>().sizeDelta;
			m_currentPos.y -= sizeDelta2.y + m_spacing.y;
		}
		m_currentPos.x = 0f;
		m_currentPos.y -= 15f;
		m_bAlignLeft = true;
		obj.transform.localPosition = m_currentPos;
		m_scrollViewRect.content.sizeDelta = new Vector2(m_scrollViewRect.content.sizeDelta.x, Mathf.Abs(m_currentPos.y - sizeDelta.y - m_spacing.y));
		m_currentPos.y -= sizeDelta.y + m_spacing.y;
		m_rowCount = 0;
	}

	private void OnClickUnit(int p_idx)
	{
		SpType spType = (SpType)eventTable_list[p_idx].n_SP_TYPE;
		int n_SP_ID = eventTable_list[p_idx].n_SP_ID;
		switch (spType)
		{
		case SpType.SP_WEAPON:
		{
			WEAPON_TABLE weaponTable = null;
			if (ManagedSingleton<OrangeDataManager>.Instance.WEAPON_TABLE_DICT.TryGetValue(n_SP_ID, out weaponTable))
			{
				MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_ItemInfo", delegate(ItemInfoUI ui)
				{
					ui.CanShowHow2Get = false;
					ui.Setup(weaponTable);
				});
			}
			break;
		}
		case SpType.SP_CHARACTER:
		{
			CHARACTER_TABLE characterTable = null;
			if (ManagedSingleton<OrangeDataManager>.Instance.CHARACTER_TABLE_DICT.TryGetValue(n_SP_ID, out characterTable))
			{
				MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_ItemInfo", delegate(ItemInfoUI ui)
				{
					ui.CanShowHow2Get = false;
					ui.Setup(characterTable);
				});
			}
			break;
		}
		}
	}

	private void OnClickUnitBossRush(int p_idx)
	{
		int count = m_bossRushInfo.netBRInfo.UsedCharacterIDList.Count;
		if (p_idx < count)
		{
			int key = int.Parse(m_bossRushInfo.netBRInfo.UsedCharacterIDList[p_idx]);
			CHARACTER_TABLE characterTable = null;
			if (ManagedSingleton<OrangeDataManager>.Instance.CHARACTER_TABLE_DICT.TryGetValue(key, out characterTable))
			{
				MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_ItemInfo", delegate(ItemInfoUI ui)
				{
					ui.CanShowHow2Get = false;
					ui.Setup(characterTable);
				});
			}
			return;
		}
		int key2 = int.Parse(m_bossRushInfo.netBRInfo.UsedWeaponIDList[p_idx - count]);
		WEAPON_TABLE weaponTable = null;
		if (ManagedSingleton<OrangeDataManager>.Instance.WEAPON_TABLE_DICT.TryGetValue(key2, out weaponTable))
		{
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_ItemInfo", delegate(ItemInfoUI ui)
			{
				ui.CanShowHow2Get = false;
				ui.Setup(weaponTable);
			});
		}
	}

	private void AddIcon(EVENT_TABLE eventTable)
	{
		SpType spType = (SpType)eventTable.n_SP_TYPE;
		int n_SP_ID = eventTable.n_SP_ID;
		Vector2 sizeDelta = m_iconRef.GetComponent<RectTransform>().sizeDelta;
		string iconWeapon = AssetBundleScriptableObject.Instance.m_iconWeapon;
		RewardPopupUIUnit rewardPopupUIUnit = Object.Instantiate(m_iconRef, m_scrollViewRect.content);
		rewardPopupUIUnit.transform.localPosition = m_currentPos;
		switch (spType)
		{
		case SpType.SP_WEAPON:
		{
			WEAPON_TABLE value2 = null;
			ManagedSingleton<OrangeDataManager>.Instance.WEAPON_TABLE_DICT.TryGetValue(n_SP_ID, out value2);
			rewardPopupUIUnit.Setup(eventTable_list.Count, iconWeapon, value2.s_ICON, value2.n_RARITY, 0, OnClickUnit);
			rewardPopupUIUnit.IgonreTween();
			eventTable_list.Add(eventTable);
			break;
		}
		case SpType.SP_CHARACTER:
		{
			CHARACTER_TABLE value = null;
			ManagedSingleton<OrangeDataManager>.Instance.CHARACTER_TABLE_DICT.TryGetValue(n_SP_ID, out value);
			rewardPopupUIUnit.Setup(eventTable_list.Count, AssetBundleScriptableObject.Instance.GetIconCharacter("icon_" + value.s_ICON), "icon_" + value.s_ICON, value.n_RARITY, 0, OnClickUnit);
			rewardPopupUIUnit.IgonreTween();
			eventTable_list.Add(eventTable);
			break;
		}
		}
		m_scrollViewRect.content.sizeDelta = new Vector2(m_scrollViewRect.content.sizeDelta.x, Mathf.Abs(m_currentPos.y - sizeDelta.y - m_spacing.y));
		m_rowCount++;
		if (m_rowCount < 5)
		{
			m_currentPos.x += sizeDelta.x + m_spacing.x;
			m_bAlignLeft = false;
			return;
		}
		m_currentPos.x = 0f;
		m_currentPos.y -= sizeDelta.y + m_spacing.y;
		m_bAlignLeft = true;
		m_rowCount = 0;
	}

	public void ForceBonusTab()
	{
		m_bIgnoreToggleSE = true;
		m_bonusTab.isOn = true;
	}
}
