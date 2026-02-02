public class SwitchSceneController : OrangeSceneController
{
	protected override void SceneInit()
	{
		MonoBehaviourSingleton<UIManager>.Instance.Connecting(true);
		MonoBehaviourSingleton<OrangeSceneManager>.Instance.ChangeScene("title", OrangeSceneManager.LoadingType.DEFAULT, delegate
		{
			MonoBehaviourSingleton<UIManager>.Instance.Connecting(false);
		});
	}
}
