public class QuestTopUI : OrangeUIBase
{
	public override void OnClickCloseBtn()
	{
		base.OnClickCloseBtn();
	}

	public void OnGoToGhara()
	{
		MonoBehaviourSingleton<UIManager>.Instance.OpenLoadingUI(delegate
		{
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_Gacha", delegate(GachaUI ui)
			{
				ui.Setup();
			});
		});
	}
}
