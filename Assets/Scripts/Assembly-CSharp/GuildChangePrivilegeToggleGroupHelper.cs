using System;
using System.Linq;
using UnityEngine;
using enums;

public class GuildChangePrivilegeToggleGroupHelper : MonoBehaviour
{
	private GuildChangePrivilegeToggleHelper[] _toggleHelpers;

	[HideInInspector]
	public GuildPrivilege PrivilegeSelected { get; private set; }

	public event Action OnSelectionChangedEvent;

	private void OnDestroy()
	{
		this.OnSelectionChangedEvent = null;
	}

	public void Setup(GuildPrivilege privilege, GuildPrivilege privilegeOptions)
	{
		if (_toggleHelpers == null)
		{
			_toggleHelpers = GetComponentsInChildren<GuildChangePrivilegeToggleHelper>();
			GuildChangePrivilegeToggleHelper[] toggleHelpers = _toggleHelpers;
			foreach (GuildChangePrivilegeToggleHelper guildChangePrivilegeToggleHelper in toggleHelpers)
			{
				guildChangePrivilegeToggleHelper.OnToggleEvent += OnSelectPrivilegeEvent;
				if (!privilegeOptions.HasFlag(guildChangePrivilegeToggleHelper.Privilege))
				{
					guildChangePrivilegeToggleHelper.gameObject.SetActive(false);
				}
			}
		}
		if (_toggleHelpers != null)
		{
			SelectPrivilege(privilege);
		}
	}

	private void SelectPrivilege(GuildPrivilege privilege)
	{
		PrivilegeSelected = privilege;
		GuildChangePrivilegeToggleHelper guildChangePrivilegeToggleHelper = _toggleHelpers.FirstOrDefault((GuildChangePrivilegeToggleHelper toggleHelper) => toggleHelper.Privilege == privilege);
		if (guildChangePrivilegeToggleHelper != null)
		{
			guildChangePrivilegeToggleHelper.Toggle.isOn = true;
		}
	}

	private void OnSelectPrivilegeEvent(GuildPrivilege privilege)
	{
		PrivilegeSelected = privilege;
		Action onSelectionChangedEvent = this.OnSelectionChangedEvent;
		if (onSelectionChangedEvent != null)
		{
			onSelectionChangedEvent();
		}
	}
}
