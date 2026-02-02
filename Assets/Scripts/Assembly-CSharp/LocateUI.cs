using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class LocateUI : OrangeUIBase
{
	[SerializeField]
	private LoopVerticalScrollRect scrollRect;

	[SerializeField]
	private LocateUiUnit unit;

	public List<AREA_TABLE> ListOpenArea { get; private set; }

	public void Setup(int selectId)
	{
		base.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
		ListOpenArea = (from x in ManagedSingleton<OrangeDataManager>.Instance.AREA_TABLE_DICT.Values
			where x.n_FLAG == 1
			orderby x.n_ID == selectId descending
			select x).ToList();
		scrollRect.OrangeInit(unit, 4, ListOpenArea.Count);
	}

	public void OnClickUnit(int idx)
	{
		AREA_TABLE select = ListOpenArea[idx];
		if (select.n_ID == 999)
		{
			MonoBehaviourSingleton<UIManager>.Instance.LoadResourceUI("UI_CommonMsg", delegate(CommonUI ui)
			{
				ui.SetupConfirmByKey("COMMON_TIP", "UNAVAILABLE_AREA_SUPPORT", "COMMON_RETURN", delegate
				{
					ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_HOME;
					MonoBehaviourSingleton<UIManager>.Instance.OpenLoadingUI(delegate
					{
						MonoBehaviourSingleton<UIManager>.Instance.CloseAllUI(delegate
						{
							MonoBehaviourSingleton<AssetsBundleManager>.Instance.UnloadAllBundleCache(delegate
							{
								new Action(MonoBehaviourSingleton<OrangeGameManager>.Instance.BackSplashAction)();
							}, true);
						});
					});
				});
			});
		}
		else
		{
			base.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_OK17;
			OnClickCloseBtn();
			MonoBehaviourSingleton<UIManager>.Instance.GetOrLoadUI("UI_TermsOfUse", delegate(TermsOfUseUI ui)
			{
				ui.Setup(select);
			});
		}
	}
}
