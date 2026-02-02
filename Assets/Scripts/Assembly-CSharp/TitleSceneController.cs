public class TitleSceneController : OrangeSceneController
{
	protected override void SceneInit()
	{
		SingletonManager.Reset();
		MonoBehaviourSingleton<CBSocketClient>.Instance.Disconnect();
		MonoBehaviourSingleton<CCSocketClient>.Instance.Disconnect();
		MonoBehaviourSingleton<CMSocketClient>.Instance.Disconnect();
		MonoBehaviourSingleton<SignalDispatcher>.Instance.Clean();
		MonoBehaviourSingleton<OrangeGameManager>.Instance.Clean();
		MonoBehaviourSingleton<GameServerService>.Instance.ClearCommand();
		Singleton<GuildSystem>.Instance.ClearCacheData();
		Singleton<PowerTowerSystem>.Instance.ClearCacheList();
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_Title_3", delegate(TitleNewUI ui)
		{
			ui.Setup();
		});
	}
}
