using NaughtyAttributes;
using UnityEngine;
using UnityEngine.UI;

internal class WeaponIconBase : IconBase
{
	public enum WeaponEquipType
	{
		UnEquip = 0,
		Main = 1,
		Sub = 2
	}

	private string rare_asset_name = "rare_{0}_{1}";

	private string bgName = "bg";

	private string frameName = "frame";

	private string small = "_small";

	[SerializeField]
	private bool SmallVer = true;

	[SerializeField]
	private Image imgRareBg;

	[SerializeField]
	private Image[] imgStar;

	[SerializeField]
	private Text textLv;

	[SerializeField]
	private Text textEvo;

	[SerializeField]
	private Image imgWeaponMain;

	[SerializeField]
	private Image imgWeaponSub;

	[HideIf("SmallVer")]
	[SerializeField]
	private OrangeRareText textRare;

	[HideIf("SmallVer")]
	[SerializeField]
	private OrangeText textName;

	[HideIf("SmallVer")]
	[SerializeField]
	private Image imgChip;

	[ShowIf("SmallVer")]
	[SerializeField]
	private Image imgRareFrame;

	private WEAPON_TABLE weapon;

	private NetWeaponInfo netWeapon;

	public void SetOtherInfo(NetWeaponInfo p_netWeapon, WeaponEquipType p_weaponEquipType)
	{
		netWeapon = p_netWeapon;
		weapon = ManagedSingleton<OrangeDataManager>.Instance.WEAPON_TABLE_DICT[netWeapon.WeaponID];
		if (SmallVer)
		{
			SetRareInfo(imgRareFrame, string.Format(rare_asset_name, weapon.n_RARITY, frameName + small));
			SetRareInfo(imgRareBg, string.Format(rare_asset_name, weapon.n_RARITY, bgName + small));
		}
		else
		{
			textRare.UpdateaRare(weapon.n_RARITY);
			SetRareInfo(imgRareBg, string.Format(rare_asset_name, weapon.n_RARITY, bgName));
			string l10nValue = ManagedSingleton<OrangeTextDataManager>.Instance.WEAPONTEXT_TABLE_DICT.GetL10nValue(weapon.w_NAME);
			string[] array = ManagedSingleton<OrangeMathf>.Instance.GetWarpString(l10nValue, textName).Split('\n');
			if (array.Length > 1)
			{
				textName.text = array[0] + "...";
			}
			else
			{
				textName.text = l10nValue;
			}
		}
		switch (p_weaponEquipType)
		{
		case WeaponEquipType.UnEquip:
			imgWeaponMain.gameObject.SetActive(false);
			imgWeaponSub.gameObject.SetActive(false);
			break;
		case WeaponEquipType.Main:
			imgWeaponMain.gameObject.SetActive(true);
			imgWeaponSub.gameObject.SetActive(false);
			break;
		case WeaponEquipType.Sub:
			imgWeaponMain.gameObject.SetActive(false);
			imgWeaponSub.gameObject.SetActive(true);
			break;
		}
		SetStar(netWeapon.Star);
		SetChip(netWeapon.Chip);
		textLv.text = ManagedSingleton<OrangeTableHelper>.Instance.GetWeaponRank(netWeapon.Exp).ToString();
		textEvo.text = 1.ToString();
	}

	private void SetStar(int p_star)
	{
		for (int i = 0; i < imgStar.Length; i++)
		{
			if (p_star > i)
			{
				imgStar[i].color = white;
			}
		}
	}

	private void SetChip(int p_chipId)
	{
		DISC_TABLE value = null;
		ManagedSingleton<OrangeDataManager>.Instance.DISC_TABLE_DICT.TryGetValue(p_chipId, out value);
	}
}
