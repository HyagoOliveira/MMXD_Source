using UnityEngine;
using UnityEngine.UI;

public abstract class GuildCell<T> : ScrollIndexCallback where T : OrangeUIBase
{
	[SerializeField]
	protected Text _guildName;

	[SerializeField]
	protected Text _presidentName;

	[SerializeField]
	protected Text _guildIntroduction;

	[SerializeField]
	protected CommonGuildBadge _guildBadge;

	[SerializeField]
	protected ImageSpriteSwitcher _rankImage;

	[SerializeField]
	protected Text _memberCount;

	[SerializeField]
	protected Text _applyType;

	[SerializeField]
	protected Text _applyLimit;

	[SerializeField]
	protected GameObject _cancelBtn;

	[SerializeField]
	protected GameObject _applyBtn;

	[SerializeField]
	protected GameObject _agreeBtn;

	[SerializeField]
	protected GameObject _refuseBtn;

	[SerializeField]
	protected GameObject _appliedImg;

	protected int _idx;

	protected int _guildId;

	protected T _parentUI;

	public int MemberLimit { get; protected set; }
}
