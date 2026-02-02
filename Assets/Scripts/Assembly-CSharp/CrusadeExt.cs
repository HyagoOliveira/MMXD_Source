public static class CrusadeExt
{
	public static NetCharacterInfo ToNetCharacterInfo(this CrusadeCharacterInfo characterInfo)
	{
		return new NetCharacterInfo
		{
			CharacterID = characterInfo.CharacterID,
			Star = characterInfo.Star,
			Skin = characterInfo.Skin
		};
	}

	public static NetWeaponInfo ToNetWeaponInfo(this CrusadeWeaponInfo weaponInfo)
	{
		return new NetWeaponInfo
		{
			WeaponID = weaponInfo.WeaponID,
			Exp = weaponInfo.Exp,
			Star = weaponInfo.Star,
			Skin = weaponInfo.Skin
		};
	}
}
