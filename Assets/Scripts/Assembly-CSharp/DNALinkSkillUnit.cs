using UnityEngine;
using UnityEngine.UI;

public class DNALinkSkillUnit : ScrollIndexCallback
{
	[SerializeField]
	private SkillButton skillButton;

	[SerializeField]
	private SkillButtonDecorator skillBtnDecorator;

	[SerializeField]
	private CommonIconBase itemIcon;

	[SerializeField]
	private DNALink parentUI;

	[SerializeField]
	private OrangeText percentTextBlue;

	[SerializeField]
	private OrangeText percentTextGreen;

	[SerializeField]
	private OrangeText descriptionText;

	[SerializeField]
	private GameObject selectionTick;

	[SerializeField]
	private WrapRectComponent descriptionRoot;

	private int _idx;

	public override void ScrollCellIndex(int p_idx)
	{
		_idx = p_idx;
		int num = 0;
		if ((bool)parentUI)
		{
			base.gameObject.SetActive(true);
			NetCharacterDNAInfo skillInfo = parentUI.GetSkillInfo(_idx);
			SKILL_TABLE value;
			ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT.TryGetValue(skillInfo.SkillID, out value);
			if (skillInfo.SlotID == 4)
			{
				num = OrangeConst.DNA_INHERIT_4;
			}
			else if (skillInfo.SlotID == 5)
			{
				num = OrangeConst.DNA_INHERIT_5;
			}
			else if (skillInfo.SlotID == 6)
			{
				num = OrangeConst.DNA_INHERIT_6;
			}
			else if (skillInfo.SlotID == 7)
			{
				num = OrangeConst.DNA_INHERIT_7;
			}
			else if (skillInfo.SlotID == 8)
			{
				num = OrangeConst.DNA_INHERIT_8;
			}
			skillButton.Setup(skillInfo.SkillID, SkillButton.StatusType.DEFAULT);
			skillButton.GetComponentInChildren<Button>().onClick.RemoveAllListeners();
			skillBtnDecorator.Setup(SkillButtonDecorator.StyleType.UNLOCKED);
			descriptionText.text = string.Format("<color=#FEC207>{0}</color>\n", skillButton.GetSkillName());
			descriptionText.text += string.Format(skillButton.GetSkillDescription(), skillButton.GetSkillEffect().ToString("0.00"));
			percentTextBlue.text = num / 100 + "%";
			percentTextGreen.text = (num + OrangeConst.DNA_PROBABILITY_UP) / 100 + "%";
			if ((bool)descriptionRoot)
			{
				descriptionRoot.gameObject.SetActive(false);
				descriptionRoot.gameObject.SetActive(true);
			}
			itemIcon.SetupItem(OrangeConst.ITEMID_DNA_SP_ITEM, 0, OnClickUseSPItem);
			selectionTick.SetActive(false);
			percentTextBlue.gameObject.SetActive(true);
			percentTextGreen.gameObject.SetActive(false);
		}
	}

	private void OnClickUseSPItem(int idx)
	{
		parentUI.PlaySEOK01();
		if (selectionTick.activeSelf)
		{
			selectionTick.SetActive(false);
			percentTextBlue.gameObject.SetActive(true);
			percentTextGreen.gameObject.SetActive(false);
		}
		else
		{
			selectionTick.SetActive(true);
			percentTextBlue.gameObject.SetActive(false);
			percentTextGreen.gameObject.SetActive(true);
		}
		NetCharacterDNAInfo skillInfo = parentUI.GetSkillInfo(_idx);
		parentUI.OnClickSkillUseSPItem(skillInfo.SlotID);
	}
}
