using System.Collections.Generic;
using UnityEngine;
using enums;

public class BonusInfoTag : MonoBehaviour
{
	[SerializeField]
	private GameObject[] sampleObj;

	[SerializeField]
	private OrangeText[] sampleText;

	[SerializeField]
	private RollingContent rollingContent;

	[SerializeField]
	private BonusInfoSub infoSub;

	private void Start()
	{
	}

	public void ClearContent()
	{
		rollingContent.Clear();
	}

	public void StopRolling()
	{
		rollingContent.StopRolling();
	}

	public bool SetActive(bool act)
	{
		if (rollingContent.m_sample.childCount > 0)
		{
			base.transform.gameObject.SetActive(act);
			return true;
		}
		base.transform.gameObject.SetActive(false);
		return false;
	}

	public void Setup(List<BonusInfoSub.InfoLable> info, bool bUseSPID = false)
	{
		info.GetEnumerator();
		info.ForEach(delegate(BonusInfoSub.InfoLable lab)
		{
			AddBonusTag((BonusType)lab.bonusType, lab.sValue);
		});
	}

	public void StartRolling()
	{
		rollingContent.Setup();
		rollingContent.StartRolling();
	}

	public void AddBonusTag(BonusType type, int val)
	{
		int num = (int)(type - 1);
		string text = "";
		switch (type)
		{
		case BonusType.BONUS_EXP:
		case BonusType.BONUS_DROPAMOUNT:
		case BonusType.BONUS_SPFORCES:
			text = "+" + val + "%";
			break;
		case BonusType.BONUS_GOLD:
		case BonusType.BONUS_PROF:
		case BonusType.BONUS_APREDUCE:
		case BonusType.BONUS_DROPRATE:
			text = ((val < 0) ? (val + "%") : ("+" + val + "%"));
			break;
		}
		sampleText[num].text = text;
		Object.Instantiate(sampleObj[num], rollingContent.m_sample, false);
	}

	public void OnTagClick()
	{
		if (infoSub != null)
		{
			infoSub.OnShowSubMenu(MonoBehaviourSingleton<UIManager>.Instance.CanvasUI.GetComponent<RectTransform>().position);
			infoSub.content.transform.localPosition = new Vector3(0f, 0f, 0f);
		}
	}
}
