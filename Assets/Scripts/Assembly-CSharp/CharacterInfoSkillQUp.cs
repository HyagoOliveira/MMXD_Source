using NaughtyAttributes;
using StageLib;
using UnityEngine;
using UnityEngine.UI;
using enums;

public class CharacterInfoSkillQUp : OrangeUIBase
{
	public Button GoQuickSkillUp;

	public Slider SkillLvSlider;

	public Text qlvtitlemsg;

	public StageLoadIcon[] QSkillImage;

	public Text QSkillText;

	public Text QSkillMsgText;

	public Text QSkillScribt;

	public Text qlvcondition;

	public Text[] info4numtext;

	[BoxGroup("Sound")]
	[SerializeField]
	private SystemSE m_clickExpItem;

	private int nSkillID;

	private int nQucikLV;

	private int nPlayerLV;

	private NetCharacterSkillInfo refNetCharacterSkillInfo;

	private CHARACTER_TABLE tCHARACTER_TABLE;

	private SKILL_TABLE tSKILL_TABLE;

	private UpgradeEffect m_upgradeEffect;

	public void Setup(NetCharacterSkillInfo tNetCharacterSkillInfo)
	{
		refNetCharacterSkillInfo = tNetCharacterSkillInfo;
		ManagedSingleton<OrangeDataManager>.Instance.CHARACTER_TABLE_DICT.TryGetValue(tNetCharacterSkillInfo.CharacterID, out tCHARACTER_TABLE);
		switch (tNetCharacterSkillInfo.Slot)
		{
		case 1:
			nSkillID = tCHARACTER_TABLE.n_SKILL1;
			break;
		case 2:
			nSkillID = tCHARACTER_TABLE.n_SKILL2;
			break;
		}
		ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT.TryGetValue(nSkillID, out tSKILL_TABLE);
		StageLoadIcon[] qSkillImage = QSkillImage;
		for (int i = 0; i < qSkillImage.Length; i++)
		{
			qSkillImage[i].CheckLoadT<Sprite>(AssetBundleScriptableObject.Instance.GetIconSkill(tSKILL_TABLE.s_ICON), tSKILL_TABLE.s_ICON);
		}
		QSkillText.text = ManagedSingleton<OrangeTextDataManager>.Instance.SKILLTEXT_TABLE_DICT.GetL10nValue(tSKILL_TABLE.w_NAME);
		nQucikLV = 0;
		nPlayerLV = ManagedSingleton<PlayerHelper>.Instance.GetLV();
		GoQuickSkillUp.interactable = false;
		InitQSkillData();
		base.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
		base._EscapeEvent = EscapeEvent.CLOSE_UI;
	}

	public int GetNeedPlayerLV(int nTargetLV)
	{
		int num = 0;
		int num2 = 0;
		while (num < nTargetLV)
		{
			if (ManagedSingleton<OrangeDataManager>.Instance.EXP_TABLE_DICT.ContainsKey(num2 + 1))
			{
				num2++;
				switch (refNetCharacterSkillInfo.Slot)
				{
				case 1:
					num = ManagedSingleton<OrangeDataManager>.Instance.EXP_TABLE_DICT[num2].n_SKILL1_LV;
					break;
				case 2:
					num = ManagedSingleton<OrangeDataManager>.Instance.EXP_TABLE_DICT[num2].n_SKILL2_LV;
					break;
				}
				continue;
			}
			return -1;
		}
		return num2;
	}

	public void InitQSkillData()
	{
		int level = refNetCharacterSkillInfo.Level;
		int num = 0;
		int num2 = 0;
		EXP_TABLE value = null;
		int skillPoint = ManagedSingleton<PlayerHelper>.Instance.GetSkillPoint();
		int zenny = ManagedSingleton<PlayerHelper>.Instance.GetZenny();
		int num3 = 0;
		int num4 = 0;
		int num5 = 0;
		int num6 = 0;
		int num7 = 0;
		switch (refNetCharacterSkillInfo.Slot)
		{
		case 1:
			num7 = ManagedSingleton<OrangeDataManager>.Instance.EXP_TABLE_DICT[nPlayerLV].n_SKILL1_LV;
			break;
		case 2:
			num7 = ManagedSingleton<OrangeDataManager>.Instance.EXP_TABLE_DICT[nPlayerLV].n_SKILL2_LV;
			break;
		}
		while (true)
		{
			num2 = level + num + 1;
			if (num7 < num2 || tSKILL_TABLE.n_LVMAX < num2)
			{
				break;
			}
			if (!ManagedSingleton<OrangeDataManager>.Instance.EXP_TABLE_DICT.TryGetValue(level + num, out value))
			{
				value = new EXP_TABLE();
				value.n_SKILLUP_SP = 999999;
				value.n_SKILLUP_MONEY = 999999;
			}
			if (num3 + value.n_SKILLUP_SP > skillPoint || num4 + value.n_SKILLUP_MONEY > zenny)
			{
				break;
			}
			num++;
			num3 += value.n_SKILLUP_SP;
			num4 += value.n_SKILLUP_MONEY;
			if (nQucikLV >= num)
			{
				num5 += value.n_SKILLUP_SP;
				num6 += value.n_SKILLUP_MONEY;
			}
		}
		string l10nValue = ManagedSingleton<OrangeTextDataManager>.Instance.SKILLTEXT_TABLE_DICT.GetL10nValue(tSKILL_TABLE.w_TIP);
		int n_EFFECT = tSKILL_TABLE.n_EFFECT;
		if (n_EFFECT == 1 || n_EFFECT != 3)
		{
			float num8 = tSKILL_TABLE.f_EFFECT_X + (float)(level + nQucikLV) * tSKILL_TABLE.f_EFFECT_Y;
			QSkillScribt.text = string.Format(l10nValue, num8.ToString("0.00"));
			QSkillScribt.alignByGeometry = false;
		}
		else
		{
			float num8 = tSKILL_TABLE.f_EFFECT_Y + tSKILL_TABLE.f_EFFECT_Z * (float)(level + nQucikLV);
			QSkillScribt.text = string.Format(l10nValue, num8.ToString("0"));
			QSkillScribt.alignByGeometry = false;
		}
		QSkillMsgText.text = ManagedSingleton<OrangeTextDataManager>.Instance.LOCALIZATION_TABLE_DICT.GetL10nValue("RANKING_PERSONAL_LEVEL") + ":" + level;
		string format = "{0}";
		string format2 = "{0}";
		if (num == 0)
		{
			if (tSKILL_TABLE.n_LVMAX < level + 1)
			{
				qlvcondition.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("MASTERY_SKILL_LEVELMAX");
			}
			else if (num7 < level + 1)
			{
				num7 = GetNeedPlayerLV(level + 1);
				if (num7 != -1)
				{
					qlvcondition.text = "<color=#ff0000>" + string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("RESTRICT_PLAYER_RANK"), num7) + "</color>";
				}
				else
				{
					qlvcondition.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("MASTERY_SKILL_LEVELMAX");
				}
			}
			else if (value != null && (value.n_SKILLUP_SP > skillPoint || value.n_SKILLUP_MONEY > zenny))
			{
				num5 += value.n_SKILLUP_SP;
				num6 += value.n_SKILLUP_MONEY;
				if (value.n_SKILLUP_SP > skillPoint)
				{
					format = "<color=#ff0000>{0}</color>";
				}
				if (value.n_SKILLUP_MONEY > zenny)
				{
					format2 = "<color=#ff0000>{0}</color>";
				}
			}
			else
			{
				qlvcondition.text = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("RESTRICT_PLAYER_RANK"), 0);
			}
		}
		else
		{
			qlvcondition.text = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("RESTRICT_PLAYER_RANK"), GetNeedPlayerLV(level + nQucikLV));
		}
		qlvtitlemsg.text = string.Format(ManagedSingleton<OrangeTextDataManager>.Instance.LOCALIZATION_TABLE_DICT.GetL10nValue("TARGET_LV"), "<color=#4eff00>" + nQucikLV + "</color>", num);
		SkillLvSlider.minValue = 0f;
		SkillLvSlider.maxValue = num;
		SkillLvSlider.value = nQucikLV;
		info4numtext[0].text = string.Format(format, skillPoint);
		info4numtext[1].text = string.Format(format2, zenny);
		info4numtext[2].text = num5.ToString();
		info4numtext[3].text = num6.ToString();
		GoQuickSkillUp.interactable = nQucikLV > 0;
	}

	public void OnAddQSkillLV()
	{
		if ((float)nQucikLV < SkillLvSlider.maxValue)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(m_clickExpItem);
			nQucikLV++;
			InitQSkillData();
		}
	}

	public void OnDecreaseQSkillLV()
	{
		if (nQucikLV > 0)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(m_clickExpItem);
			nQucikLV--;
			InitQSkillData();
		}
	}

	public void OnMaxQSkillLV()
	{
		if ((float)nQucikLV != SkillLvSlider.maxValue)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(m_clickExpItem);
			nQucikLV = (int)SkillLvSlider.maxValue;
			InitQSkillData();
		}
	}

	public void OnMinQSkillLV()
	{
		if (nQucikLV != 0)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(m_clickExpItem);
			nQucikLV = 0;
			InitQSkillData();
		}
	}

	public void OnQSkillSilderChange(float value)
	{
		if ((int)Mathf.Round(SkillLvSlider.value) != nQucikLV)
		{
			nQucikLV = (int)Mathf.Round(SkillLvSlider.value);
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(m_clickExpItem);
			InitQSkillData();
		}
	}

	public void OnQSkillUp()
	{
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK17);
		ManagedSingleton<PlayerNetManager>.Instance.CharacterUpgradeSkillReq(refNetCharacterSkillInfo.CharacterID, (CharacterSkillSlot)refNetCharacterSkillInfo.Slot, refNetCharacterSkillInfo.Level + nQucikLV, delegate
		{
			CharacterInfo characterInfo = ManagedSingleton<PlayerNetManager>.Instance.dicCharacter.Value(refNetCharacterSkillInfo.CharacterID);
			refNetCharacterSkillInfo = characterInfo.netSkillDic[(CharacterSkillSlot)refNetCharacterSkillInfo.Slot];
			nQucikLV = 0;
			InitQSkillData();
			PlayUpgradeEffect();
		});
	}

	private void PlayUpgradeEffect()
	{
		if (m_upgradeEffect == null)
		{
			MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(AssetBundleScriptableObject.Instance.m_uiPath + "upgradeeffect", "UpgradeEffect", delegate(GameObject asset)
			{
				MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_GLOW02);
				GameObject gameObject = Object.Instantiate(asset, base.transform);
				m_upgradeEffect = gameObject.GetComponent<UpgradeEffect>();
				m_upgradeEffect.Play(QSkillImage[0].transform.position);
			});
		}
		else
		{
			m_upgradeEffect.Play(QSkillImage[0].transform.position);
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_GLOW02);
		}
	}
}
