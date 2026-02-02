public class BattleTestScene : OrangeSceneController
{
	protected override void SceneInit()
	{
		MonoBehaviourSingleton<InputManager>.Instance.LoadVirtualPad();
		MonoBehaviourSingleton<PoolManager>.Instance.CreatePoolBase<VisualHpBar>("prefab/hpbar", "hpbar", 5, null);
		Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.BATTLE_START);
	}
}
