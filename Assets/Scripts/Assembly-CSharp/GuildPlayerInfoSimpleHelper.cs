using UnityEngine;
using UnityEngine.UI;
using enums;

public class GuildPlayerInfoSimpleHelper : MonoBehaviour
{
	[SerializeField]
	private Text _textLevel;

	[SerializeField]
	private Text _textPlayerName;

	[SerializeField]
	private Transform _playerIconRoot;

	[SerializeField]
	private GuildPrivilegeHelper _privilegeHelper;

	public GuildPrivilege Privilege { get; private set; }

	public void Setup(string playerId)
	{
		GuildUIHelper.SetPlayerHUDData(playerId, _textPlayerName, _textLevel, _playerIconRoot);
		NetMemberInfo memberInfo;
		if (Singleton<GuildSystem>.Instance.TryGetMemberInfo(playerId, out memberInfo))
		{
			Privilege = (GuildPrivilege)memberInfo.Privilege;
			_privilegeHelper.Setup(Privilege);
		}
	}

	public void Setup(string playerId, GuildPrivilege privilege)
	{
		GuildUIHelper.SetPlayerHUDData(playerId, _textPlayerName, _textLevel, _playerIconRoot);
		_privilegeHelper.Setup(privilege);
		Privilege = privilege;
	}
}
