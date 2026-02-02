using StageLib;
using UnityEngine;
using UnityEngine.UI;

public class DNAExchange : OrangeUIBase
{
	public StageLoadIcon[] RSkillImg;

	public Text[] RSkillText;

	public Text[] RSkillMsgText;

	public Text ChangeDesc;

	public NetCharacterDNAInfo refNCDNAI;

	private bool bNetLock;

	public void Setup(NetCharacterDNAInfo NCDNAI)
	{
		refNCDNAI = NCDNAI;
		int[] array = new int[2] { NCDNAI.SkillID, NCDNAI.PulledSkillID };
		string[] array2 = new string[2];
		for (int i = 0; i < 2; i++)
		{
			SKILL_TABLE value;
			ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT.TryGetValue(array[i], out value);
			RSkillImg[i].CheckLoadT<Sprite>(AssetBundleScriptableObject.Instance.GetIconSkill(value.s_ICON), value.s_ICON);
			array2[i] = ManagedSingleton<OrangeTextDataManager>.Instance.SKILLTEXT_TABLE_DICT.GetL10nValue(value.w_NAME);
			RSkillText[i].text = array2[i];
			array2[i] = "<color=#0080C4>" + array2[i] + "</color>";
			string l10nValue = ManagedSingleton<OrangeTextDataManager>.Instance.SKILLTEXT_TABLE_DICT.GetL10nValue(value.w_TIP);
			float num = 100f;
			int n_EFFECT = value.n_EFFECT;
			if (n_EFFECT == 1)
			{
				num = value.f_EFFECT_X + 0f * value.f_EFFECT_Y;
			}
			RSkillMsgText[i].text = string.Format(l10nValue.Replace("{0}", "{0:0.00}"), num);
		}
		ChangeDesc.text = string.Format(ManagedSingleton<OrangeTextDataManager>.Instance.LOCALIZATION_TABLE_DICT.GetL10nValue("ARMSSKILL_WARN_2"), array2[0], array2[1]);
		base._EscapeEvent = EscapeEvent.CUSTOM;
	}

	protected override void DoCustomEscapeEvent()
	{
		ReChangeRSkillGo();
	}

	public void Update()
	{
		RSkillMsgText[0].alignByGeometry = false;
		RSkillMsgText[1].alignByGeometry = false;
	}

	public override void OnClickCloseBtn()
	{
		if (!bNetLock)
		{
			base.OnClickCloseBtn();
		}
	}

	public void ReChangeRSkillGo()
	{
		if (!bNetLock)
		{
			bNetLock = true;
			ManagedSingleton<PlayerNetManager>.Instance.DNAChangeSkill(refNCDNAI.CharacterID, refNCDNAI.SlotID, true, delegate(NetCharacterDNAInfo DNAInfo)
			{
				refNCDNAI = DNAInfo;
				bNetLock = false;
				OnClickCloseBtn();
			});
		}
	}

	public void ReChangeRSkillNo()
	{
		if (!bNetLock)
		{
			bNetLock = true;
			ManagedSingleton<PlayerNetManager>.Instance.DNAChangeSkill(refNCDNAI.CharacterID, refNCDNAI.SlotID, false, delegate
			{
				refNCDNAI = null;
				bNetLock = false;
				OnClickCloseBtn();
			});
		}
	}
}
