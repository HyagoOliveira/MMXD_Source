using UnityEngine;
using UnityEngine.UI;

public class CrusadeRecordGroup : MonoBehaviour
{
	[SerializeField]
	private CommonIconBase _commonIconSource;

	[SerializeField]
	private Transform _playerIconRoot;

	[SerializeField]
	private Transform _mainWeaponRoot;

	[SerializeField]
	private Transform _subWeaponRoot;

	[SerializeField]
	private Text _textRecordTime;

	[SerializeField]
	private Text _textRecordScore;

	public void Setup(NetCharacterInfo characterInfo, NetWeaponInfo weaponInfoMain, NetWeaponInfo weaponInfoSub, int battleTime, long score)
	{
		CHARACTER_TABLE cHARACTER_TABLE = ManagedSingleton<OrangeDataManager>.Instance.CHARACTER_TABLE_DICT[characterInfo.CharacterID];
		CommonIconBase commonIconBase = Object.Instantiate(_commonIconSource, _playerIconRoot);
		CommonIconBase commonIcon = Object.Instantiate(_commonIconSource, _mainWeaponRoot);
		CommonIconBase commonIcon2 = Object.Instantiate(_commonIconSource, _subWeaponRoot);
		commonIconBase.Setup(0, AssetBundleScriptableObject.Instance.GetIconCharacter("icon_" + cHARACTER_TABLE.s_ICON), "icon_" + cHARACTER_TABLE.s_ICON);
		commonIconBase.SetOtherInfo(characterInfo);
		SetWeaponIcon(commonIcon, weaponInfoMain, CommonIconBase.WeaponEquipType.Main);
		SetWeaponIcon(commonIcon2, weaponInfoSub, CommonIconBase.WeaponEquipType.Sub);
		_textRecordTime.text = DateTimeHelper.FromEpochLocalTime(battleTime).ToFullDateString();
		_textRecordScore.text = score.ToString();
	}

	private void SetWeaponIcon(CommonIconBase commonIcon, NetWeaponInfo weaponInfo, CommonIconBase.WeaponEquipType weaponEquipType)
	{
		WEAPON_TABLE value;
		if (!ManagedSingleton<OrangeDataManager>.Instance.WEAPON_TABLE_DICT.TryGetValue(weaponInfo.WeaponID, out value))
		{
			commonIcon.Setup(0, "", "");
			commonIcon.SetOtherInfo(null, weaponEquipType);
			return;
		}
		commonIcon.gameObject.SetActive(true);
		commonIcon.Setup(0, AssetBundleScriptableObject.Instance.m_iconWeapon, value.s_ICON);
		if (weaponInfo != null)
		{
			commonIcon.SetOtherInfo(weaponInfo, weaponEquipType);
		}
		else
		{
			commonIcon.SetOtherInfo(null, weaponEquipType);
		}
	}
}
