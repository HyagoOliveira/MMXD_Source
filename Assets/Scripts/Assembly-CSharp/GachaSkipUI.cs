public class GachaSkipUI : OrangeUIBase
{
	public void OnClickBtnSkipAll()
	{
		if (!base.IsLock)
		{
			Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.GACHA_SKIP, -1);
			OnClickCloseBtn();
		}
	}

	public void OnClickBtnSkipOnce()
	{
		if (!base.IsLock)
		{
			Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.GACHA_SKIP, 1);
		}
	}

	private void OnEnable()
	{
		Singleton<GenericEventManager>.Instance.AttachEvent(EventManager.ID.SWITCH_SCENE, Clear);
	}

	private void OnDisable()
	{
		Singleton<GenericEventManager>.Instance.DetachEvent(EventManager.ID.SWITCH_SCENE, Clear);
	}

	private void Clear()
	{
		OnClickCloseBtn();
	}
}
