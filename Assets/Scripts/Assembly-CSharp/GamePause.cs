using StageLib;

public class GamePause : OrangeUIBase
{
	public void Setup()
	{
	}

	public override void DoJoystickEvent()
	{
		StageUpdate stageUpdate = StageResManager.GetStageUpdate();
		if ((!(stageUpdate != null) || !stageUpdate.IsEnd) && MonoBehaviourSingleton<OrangeGameManager>.Instance.IsGamePause)
		{
			MonoBehaviourSingleton<InputManager>.Instance.ManualUpdateStartButton();
			if (ManagedSingleton<InputStorage>.Instance.IsPressed(MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify, ButtonId.START))
			{
				OnClickCloseBtn();
			}
		}
	}
}
