using System.Collections.Generic;
using UnityEngine;

public class GuildRedDotHelper : MonoBehaviour
{
	public enum EventType
	{
		None = 0,
		NewGuild = 1,
		NewInviteGuild = 2,
		NewApplyPlayer = 3,
		HasEddieReward = 4
	}

	[SerializeField]
	private List<EventType> _checkType;

	private void Awake()
	{
		Singleton<GenericEventManager>.Instance.AttachEvent<bool>(EventManager.ID.UPDATE_GUILD_HINT, OnUpdateHint);
		OnUpdateHint(false);
	}

	private void OnDestroy()
	{
		Singleton<GenericEventManager>.Instance.DetachEvent<bool>(EventManager.ID.UPDATE_GUILD_HINT, OnUpdateHint);
	}

	private void OnUpdateHint(bool isSocketEvent)
	{
		bool active = false;
		if (isSocketEvent && _checkType.Contains(EventType.NewGuild) && Singleton<GuildSystem>.Instance.HasGuild)
		{
			active = true;
		}
		if (!Singleton<GuildSystem>.Instance.HasGuild)
		{
			if (_checkType.Contains(EventType.NewInviteGuild) && Singleton<GuildSystem>.Instance.HasInviteGuild)
			{
				active = true;
			}
		}
		else
		{
			if (_checkType.Contains(EventType.NewApplyPlayer) && Singleton<GuildSystem>.Instance.InviteGuildListCache.Count > 0)
			{
				active = true;
			}
			if (_checkType.Contains(EventType.HasEddieReward) && Singleton<GuildSystem>.Instance.HasEddieReward)
			{
				active = true;
			}
		}
		base.gameObject.SetActive(active);
	}
}
