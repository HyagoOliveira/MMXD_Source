using System;
using UnityEngine;
using enums;

public class GuildPlayerInfoBeforeAfterHelper : MonoBehaviour
{
	[SerializeField]
	private GuildPlayerInfoSimpleHelper _playerInfoBefore;

	[SerializeField]
	private GuildPlayerInfoSimpleHelper _playerInfoAfter;

	public Action OnPlayerInfoAfterChange;

	public void SetPlayerInfoBefore(string playerId)
	{
		_playerInfoBefore.Setup(playerId);
	}

	public void SetPlayerInfoBefore(string playerId, GuildPrivilege privilege)
	{
		_playerInfoBefore.Setup(playerId, privilege);
	}

	public void SetPlayerInfoAfter(string playerId)
	{
		_playerInfoAfter.Setup(playerId);
	}

	public void SetPlayerInfoAfter(string playerId, GuildPrivilege privilege)
	{
		if (_playerInfoAfter.Privilege != privilege)
		{
			Action onPlayerInfoAfterChange = OnPlayerInfoAfterChange;
			if (onPlayerInfoAfterChange != null)
			{
				onPlayerInfoAfterChange();
			}
		}
		_playerInfoAfter.Setup(playerId, privilege);
	}
}
