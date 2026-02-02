using UnityEngine;
using UnityEngine.UI;

public class ItemHowToGetUIUnit : ScrollIndexCallback
{
	[SerializeField]
	private ItemHowToGetUI parent;

	[SerializeField]
	private OrangeText textTitle;

	[SerializeField]
	private OrangeText textMain;

	[SerializeField]
	private GameObject goBlock;

	[SerializeField]
	private Image imgTitleBG;

	[SerializeField]
	private GameObject groupDayLimit;

	[SerializeField]
	private OrangeText textDayLimit;

	private ItemHowToGetUI.Data useData;

	public override void ScrollCellIndex(int idx)
	{
		useData = parent.GetChildInfo(idx);
		if (useData.IsOpen)
		{
			if (goBlock.activeSelf)
			{
				goBlock.SetActive(false);
			}
			textTitle.text = ManagedSingleton<OrangeTextDataManager>.Instance.HOWTOGETTEXT_TABLE_DICT.GetL10nValue(useData.HowToGetTable.w_NAME);
			textMain.text = ManagedSingleton<OrangeTextDataManager>.Instance.HOWTOGETTEXT_TABLE_DICT.GetL10nValue(useData.HowToGetTable.w_TIP);
		}
		else
		{
			goBlock.SetActive(true);
			textTitle.text = "???";
		}
		if (!string.IsNullOrEmpty(useData.countLimit))
		{
			groupDayLimit.SetActive(true);
			textDayLimit.text = useData.countLimit;
		}
	}

	public override void BackToPool()
	{
		groupDayLimit.SetActive(false);
		base.BackToPool();
	}

	public void OnClickUnit()
	{
		if (useData.IsOpen && (useData.HowToGetTable.n_UILINK != 4 || !MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.CheckCardCountMax()))
		{
			ManagedSingleton<UILinkHelper>.Instance.LoadUI(useData.HowToGetTable);
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK17);
		}
	}
}
