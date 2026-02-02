public class GuildConfirmChangeSceneEventUIHelper : OrangePartialUIHelperBase
{
	protected virtual void OnEnable()
	{
		Singleton<GuildSystem>.Instance.OnConfirmChangeSceneEvent += OnConfirmChangeSceneEvent;
	}

	protected virtual void OnDisable()
	{
		Singleton<GuildSystem>.Instance.OnConfirmChangeSceneEvent -= OnConfirmChangeSceneEvent;
	}

	private void OnConfirmChangeSceneEvent()
	{
		_mainUI.OnClickCloseBtn();
	}
}
