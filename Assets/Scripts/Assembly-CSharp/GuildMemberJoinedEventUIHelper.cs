using UnityEngine;

public class GuildMemberJoinedEventUIHelper : OrangePartialUIHelperBase
{
	private enum ActionType
	{
		CloseUI = 0,
		ChangeScene = 1
	}

	[SerializeField]
	private ActionType _actionType;

	protected virtual void OnEnable()
	{
		Singleton<GuildSystem>.Instance.OnSocketMemberJoinedEvent += OnSocketMemberJoinedEvent;
	}

	protected virtual void OnDisable()
	{
		Singleton<GuildSystem>.Instance.OnSocketMemberJoinedEvent -= OnSocketMemberJoinedEvent;
	}

	private void OnSocketMemberJoinedEvent()
	{
		switch (_actionType)
		{
		case ActionType.CloseUI:
			_mainUI.OnClickCloseBtn();
			break;
		case ActionType.ChangeScene:
			Singleton<GuildSystem>.Instance.ConfirmChangeScene();
			break;
		}
	}
}
