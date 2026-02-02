public class StageNoiseUI : OrangeUIBase, ILogicUpdate
{
	private int endFrame;

	public void Setup(float playFrame)
	{
		MonoBehaviourSingleton<GameLogicUpdateManager>.Instance.RemoveUpdate(this);
		endFrame = GameLogicUpdateManager.GameFrame + MonoBehaviourSingleton<GameLogicUpdateManager>.Instance.TimeToLogicFrame(playFrame);
		MonoBehaviourSingleton<GameLogicUpdateManager>.Instance.AddUpdate(this);
	}

	private void OnDisable()
	{
		MonoBehaviourSingleton<GameLogicUpdateManager>.Instance.RemoveUpdate(this);
	}

	public void LogicUpdate()
	{
		if (GameLogicUpdateManager.GameFrame >= endFrame)
		{
			MonoBehaviourSingleton<GameLogicUpdateManager>.Instance.RemoveUpdate(this);
			OnClickCloseBtn();
		}
	}
}
