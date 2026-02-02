using System;
using UnityEngine;
using UnityEngine.UI;
using enums;

[RequireComponent(typeof(Toggle))]
public class GuildChangePrivilegeToggleHelper : MonoBehaviour
{
	[HideInInspector]
	public Toggle Toggle;

	public GuildPrivilege Privilege;

	[SerializeField]
	private GuildPrivilegeHelper _privilegeHelper;

	public event Action<GuildPrivilege> OnToggleEvent;

	private void OnEnable()
	{
		Toggle = GetComponent<Toggle>();
		_privilegeHelper.Setup(Privilege);
	}

	private void OnDestroy()
	{
		this.OnToggleEvent = null;
	}

	public void OnValueChanged()
	{
		Action<GuildPrivilege> onToggleEvent = this.OnToggleEvent;
		if (onToggleEvent != null)
		{
			onToggleEvent(Privilege);
		}
	}
}
