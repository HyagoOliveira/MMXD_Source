using UnityEngine;

public class StoreHelper : ManagedSingleton<StoreHelper>
{
	private readonly long ratingExpiredTime = 31556926L;

	public override void Initialize()
	{
	}

	public override void Dispose()
	{
	}

	public void OpenStoreReview()
	{
		long serverUnixTimeNowUTC = MonoBehaviourSingleton<OrangeGameManager>.Instance.ServerUnixTimeNowUTC;
		long ratingExpiredTime2 = MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.RatingExpiredTime;
	}

	public void OpenMarket()
	{
		MonoBehaviourSingleton<UIManager>.Instance.LoadResourceUI("UI_CommonMsg", delegate(CommonUI ui)
		{
			ui.CloseSE = SystemSE.NONE;
			ui.MuteSE = true;
			ui.SetupConfirmByKey("COMMON_TIP", "APPLICATION_HAS_NEW_VERSION", "COMMON_OK", delegate
			{
				Application.Quit();
				MonoBehaviourSingleton<UIManager>.Instance.CloseAllUI(delegate
				{
					MonoBehaviourSingleton<OrangeSceneManager>.Instance.ChangeScene("switch", OrangeSceneManager.LoadingType.DEFAULT, null, false);
				});
			});
		});
	}
}
