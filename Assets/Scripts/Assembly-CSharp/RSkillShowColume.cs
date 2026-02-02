using System.Collections.Generic;
using System.Linq;
using StageLib;
using UnityEngine;
using UnityEngine.UI;

public class RSkillShowColume : ScrollIndexCallback
{
	public GameObject[] RSkill;

	public StageLoadIcon[] RSkillImg;

	public Text[] RSkillText;

	public Text[] RSkillMsgText;

	private void LateUpdate()
	{
		Text[] rSkillMsgText = RSkillMsgText;
		foreach (Text text in rSkillMsgText)
		{
			if (text.alignByGeometry)
			{
				text.alignByGeometry = false;
			}
		}
	}

	public override void ScrollCellIndex(int p_idx)
	{
		WeaponInfoUI weaponInfoUI = null;
		Transform parent = base.transform.parent;
		while (parent != null)
		{
			weaponInfoUI = parent.GetComponent<WeaponInfoUI>();
			if (weaponInfoUI != null)
			{
				break;
			}
			parent = parent.parent;
		}
		if (weaponInfoUI == null)
		{
			return;
		}
		WEAPON_TABLE tWEAPON_TABLE = ManagedSingleton<OrangeDataManager>.Instance.WEAPON_TABLE_DICT[weaponInfoUI.nTargetWeaponID];
		List<RANDOMSKILL_TABLE> list = null;
		if (tWEAPON_TABLE.n_DIVE != 0)
		{
			IEnumerable<RANDOMSKILL_TABLE> source = ManagedSingleton<OrangeDataManager>.Instance.RANDOMSKILL_TABLE_DICT.Values.Where((RANDOMSKILL_TABLE obj) => obj.n_GROUP == tWEAPON_TABLE.n_DIVE);
			if (source.Count() > 0)
			{
				list = source.ToList();
			}
		}
		if (list != null)
		{
			int num = p_idx * 2;
			int i;
			for (i = 0; i < RSkill.Length && i + num < list.Count; i++)
			{
				SKILL_TABLE value;
				ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT.TryGetValue(list[num + i].n_SKILL, out value);
				RSkill[i].SetActive(true);
				RSkillImg[i].CheckLoad(AssetBundleScriptableObject.Instance.GetIconSkill(value.s_ICON), value.s_ICON);
				RSkillText[i].text = ManagedSingleton<OrangeTextDataManager>.Instance.SKILLTEXT_TABLE_DICT.GetL10nValue(value.w_NAME);
				string l10nValue = ManagedSingleton<OrangeTextDataManager>.Instance.SKILLTEXT_TABLE_DICT.GetL10nValue(value.w_TIP);
				float num2 = 100f;
				int n_EFFECT = value.n_EFFECT;
				if (n_EFFECT == 1)
				{
					num2 = value.f_EFFECT_X + 0f * value.f_EFFECT_Y;
				}
				RSkillMsgText[i].text = string.Format(l10nValue.Replace("{0}", "{0:0.00}"), num2);
			}
			for (; i < RSkill.Length; i++)
			{
				RSkill[i].SetActive(false);
			}
		}
		else
		{
			for (int num3 = RSkill.Length - 1; num3 >= 0; num3--)
			{
				RSkill[num3].SetActive(false);
			}
		}
	}
}
