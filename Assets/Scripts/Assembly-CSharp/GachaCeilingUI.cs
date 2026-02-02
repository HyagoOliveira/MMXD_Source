using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.UI;

public class GachaCeilingUI : OrangeUIBase
{
	private enum UI_TYPE
	{
		EXCHANGE = 0,
		RULE = 1,
		COUNT = 2
	}

	private UI_TYPE uiType;

	[SerializeField]
	private OrangeText iconCostVal;

	[SerializeField]
	private Canvas[] tabCanvas = new Canvas[2];

	[SerializeField]
	private ItemBoxTab[] tabs = new ItemBoxTab[2];

	[BoxGroup("Rule")]
	[SerializeField]
	private OrangeText textRule;

	[BoxGroup("Exchange")]
	[SerializeField]
	private LoopVerticalScrollRect scrollRect;

	[SerializeField]
	private GachaCeilingUIUnit unit;

	public List<GACHALIST_TABLE> NowGachaList { get; private set; }

	public List<GACHA_TABLE> ListExchange { get; private set; }

	public int TotalSetupDrawCount { get; private set; }

	public void Setup(List<GACHALIST_TABLE> p_nowGachaList, List<GACHA_TABLE> p_listExchange)
	{
		base.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
		textRule.alignByGeometry = false;
		NowGachaList = p_nowGachaList;
		ListExchange = p_listExchange;
		TotalSetupDrawCount = ManagedSingleton<ExtendDataHelper>.Instance.GetGachaDrawCount(NowGachaList);
		iconCostVal.text = TotalSetupDrawCount.ToString();
		int num = 2;
		for (int i = 0; i < num; i++)
		{
			UI_TYPE changeType = (UI_TYPE)i;
			tabs[i].AddBtnCB(delegate
			{
				MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_CURSOR07);
				uiType = changeType;
				UpdateUI();
			});
		}
		scrollRect.OrangeInit(unit, 8, ListExchange.Count);
		tabs[(int)uiType].UpdateState(true);
		UpdateUI();
		base._EscapeEvent = EscapeEvent.CLOSE_UI;
	}

	private void UpdateUI()
	{
		int num = 2;
		int num2 = (int)uiType;
		for (int i = 0; i < num; i++)
		{
			tabCanvas[i].enabled = i == num2;
			tabs[i].UpdateState(i != num2);
		}
	}
}
