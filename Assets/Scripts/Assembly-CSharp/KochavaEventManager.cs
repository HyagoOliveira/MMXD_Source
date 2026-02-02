public class KochavaEventManager : MonoBehaviourSingleton<KochavaEventManager>
{
	private string EVENT_NAME_PURCHASE = "Orange_Purchase";

	public void SendEvent_NewAccount()
	{
		Kochava.Event @event = new Kochava.Event(EVENT_NAME_PURCHASE);
		@event.SetCustomValue("game_uid", MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.CurrentPlayerID);
		Kochava.Tracker.SendEvent(@event);
	}

	private void OnEnable()
	{
		Singleton<GenericEventManager>.Instance.AttachEvent(EventManager.ID.PLAYER_LEVEL_UP, SendEvent_LevelComplete);
	}

	private void OnDisable()
	{
		Singleton<GenericEventManager>.Instance.DetachEvent(EventManager.ID.PLAYER_LEVEL_UP, SendEvent_LevelComplete);
	}

	public void SendEvent_Purchase(int shopItemId)
	{
		SHOP_TABLE value = null;
		if (ManagedSingleton<OrangeDataManager>.Instance.SHOP_TABLE_DICT.TryGetValue(shopItemId, out value))
		{
			string empty = string.Empty;
			empty = "iOS";
			Kochava.Event @event = new Kochava.Event(EVENT_NAME_PURCHASE);
			@event.SetCustomValue("price", value.n_COIN_MOUNT);
			@event.SetCustomValue("platform", empty);
			Kochava.Tracker.SendEvent(@event);
		}
	}

	private void SendEvent_LevelComplete()
	{
		int lV = ManagedSingleton<PlayerHelper>.Instance.GetLV();
		if (lV == 17 || lV == 22)
		{
			Kochava.Tracker.SendEvent(new Kochava.Event(Kochava.EventType.LevelComplete)
			{
				userId = MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.CurrentPlayerID,
				name = lV.ToString(),
				duration = MonoBehaviourSingleton<OrangeGameManager>.Instance.ServerUnixTimeNowUTC
			});
		}
	}
}
