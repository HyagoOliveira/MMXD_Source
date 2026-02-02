#define RELEASE
using System;
using System.Linq;
using UnityEngine;
using enums;

[RequireComponent(typeof(ImageSpriteSwitcher))]
public class GuildPrivilegeHelper : MonoBehaviour
{
	[Serializable]
	private class GuildPrivilegeSetting
	{
		public GuildPrivilege Privilege;

		public string LocalizationKey;

		public Color Color;

		public int SpriteIndex;
	}

	[SerializeField]
	private GuildPrivilege _privilege;

	[SerializeField]
	private OrangeText _textPrivilege;

	private ImageSpriteSwitcher _privilegeSprite;

	[SerializeField]
	private GuildPrivilegeSetting[] PrivilegeSettings;

	public void OnValidate()
	{
		Setup(_privilege, true);
	}

	public void Setup(GuildPrivilege privilege, bool isValidate = false)
	{
		_privilege = privilege;
		if (_privilegeSprite == null)
		{
			_privilegeSprite = GetComponent<ImageSpriteSwitcher>();
		}
		GuildPrivilegeSetting guildPrivilegeSetting = PrivilegeSettings.FirstOrDefault((GuildPrivilegeSetting setting) => setting.Privilege == privilege);
		if (guildPrivilegeSetting != null)
		{
			if (!isValidate)
			{
				_textPrivilege.IsLocalizationText = true;
				_textPrivilege.LocalizationKey = guildPrivilegeSetting.LocalizationKey;
				_textPrivilege.UpdateTextImmediate();
			}
			_textPrivilege.color = guildPrivilegeSetting.Color;
			_privilegeSprite.ChangeImage(guildPrivilegeSetting.SpriteIndex);
		}
		else
		{
			Debug.LogError(string.Format("Setting of Privilege {0} not found", privilege));
		}
	}
}
