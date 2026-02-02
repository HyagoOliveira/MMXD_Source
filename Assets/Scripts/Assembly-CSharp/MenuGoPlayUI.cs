#define RELEASE
public class MenuGoPlayUI : OrangeUIBase
{
	public void OnClickBtnStory()
	{
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK01);
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_StoryStageSelect", delegate(StoryStageSelectUI ui)
		{
			ui.Setup();
		});
	}

	public void OnClickBtnEvent()
	{
		Debug.Log("OnClickBtnEvent");
		MonoBehaviourSingleton<UIManager>.Instance.NotOpenMsgUI();
	}

	public void OnClickBtnMultiplay()
	{
		Debug.Log("OnClickBtnMultiplay");
		closeCB = delegate
		{
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_CoopStageSelectUI", delegate(CoopStageSelectUI ui)
			{
				ui.Setup();
			});
		};
		base.OnClickCloseBtn();
	}

	public void OnClickBtnChallenge()
	{
		Debug.Log("OnClickBtnChallenge");
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI<UI_Challenge>("UI_BossChallenge");
	}

	public override void OnClickCloseBtn()
	{
		base.OnClickCloseBtn();
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
