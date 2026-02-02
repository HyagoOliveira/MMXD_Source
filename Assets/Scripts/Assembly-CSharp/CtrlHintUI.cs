using CallbackDefs;
using UnityEngine.UI;

public class CtrlHintUI : OrangeUIBase
{
	public Text[] BtnText;

	protected override void Awake()
	{
		base.Awake();
	}

	public void Setup()
	{
		MonoBehaviourSingleton<UpdateManager>.Instance.Pause = true;
	}

	public void OpenSettingUI()
	{
		Callback tmpcloseCB = closeCB;
		closeCB = null;
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_GameInputEditor", delegate(GameInputEditor ui)
		{
			ui.closeCB = tmpcloseCB;
			ui.Setup();
		});
	}

	public override void OnClickCloseBtn()
	{
		base.OnClickCloseBtn();
		MonoBehaviourSingleton<UpdateManager>.Instance.Pause = false;
	}
}
