using UnityEngine;

public abstract class WantedMemberSelectCellHelperBase : ScrollIndexCallback
{
	[SerializeField]
	protected PlayerIconBase _playerIcon;

	[SerializeField]
	protected CommonIconBase _commonIcon;

	[SerializeField]
	protected GameObject _goSelectBorder;

	[SerializeField]
	protected ImageSpriteSwitcher _imageSelectIndex;

	[SerializeField]
	protected GameObject _goTipDepartured;
}
