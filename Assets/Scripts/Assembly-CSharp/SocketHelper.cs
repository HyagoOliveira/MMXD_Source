#define RELEASE
public class SocketHelper : ManagedSingleton<SocketHelper>
{
	public override void Dispose()
	{
	}

	public override void Initialize()
	{
	}

	public void UpdateHUD(string playerId, string hud)
	{
		SocketPlayerHUD socketPlayerHUD = null;
		if (hud != null)
		{
			socketPlayerHUD = JsonHelper.Deserialize<SocketPlayerHUD>(hud);
		}
		if (hud == null || socketPlayerHUD == null)
		{
			socketPlayerHUD = new SocketPlayerHUD
			{
				m_PlayerId = playerId
			};
		}
		UpdateHUD(playerId, socketPlayerHUD);
	}

	public void UpdateHUD(string playerId, SocketPlayerHUD playerHUD)
	{
		if (playerId != playerHUD.m_PlayerId)
		{
			Debug.Log("Invalid HUD info update " + playerId + " <=> " + playerHUD.m_PlayerId + " [" + playerHUD.m_Name + "].");
		}
		else if (MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicHUD.ContainsKey(playerId))
		{
			MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicHUD[playerId] = playerHUD;
		}
		else
		{
			MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicHUD.Add(playerId, playerHUD);
		}
	}
}
