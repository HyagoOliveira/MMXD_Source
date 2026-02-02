using UnityEngine;

public class PlayerInfoScrollCell : ScrollIndexCallback
{
	[SerializeField]
	private GameObject IconRoot;

	[SerializeField]
	private StarClearComponent EquipStar;

	[SerializeField]
	private OrangeText Level;

	private CHARACTER_TABLE characterTable;

	private CharacterInfoUI parentCharacterInfoUI;

	private void Start()
	{
	}

	private void Update()
	{
	}

	public override void ScrollCellIndex(int p_idx)
	{
	}

	private void ClickImgCB(int p_idx)
	{
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK12);
	}

	public void SetupCharacter(CharacterInfo tCharacterInfo, int StandbyCharID)
	{
		MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(AssetBundleScriptableObject.Instance.m_uiPath + "CommonIconBaseSmall", "CommonIconBaseSmall", delegate(GameObject asset)
		{
			CommonIconBase component = Object.Instantiate(asset, base.transform).GetComponent<CommonIconBase>();
			EquipStar.gameObject.SetActive(false);
			component.SetupCharacterForPlayerInfo(tCharacterInfo);
			component.EnableIsPlayingBadge(tCharacterInfo.netInfo.CharacterID == StandbyCharID);
		});
	}

	public void SetupWeapon(WeaponInfo tWeaponInfo, NetWeaponInfo inputNetWeapon, CommonIconBase.WeaponEquipType typ)
	{
		int weaponID = inputNetWeapon.WeaponID;
		WEAPON_TABLE tWeapon_TABLE = ManagedSingleton<OrangeDataManager>.Instance.WEAPON_TABLE_DICT[weaponID];
		MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(AssetBundleScriptableObject.Instance.m_uiPath + "CommonIconBaseSmall", "CommonIconBaseSmall", delegate(GameObject asset)
		{
			CommonIconBase component = Object.Instantiate(asset, base.transform).GetComponent<CommonIconBase>();
			EquipStar.gameObject.SetActive(false);
			component.SetPlayerWeaponInfo(tWeaponInfo, inputNetWeapon, typ, false, -1, false);
			component.Setup(0, AssetBundleScriptableObject.Instance.m_iconWeapon, tWeapon_TABLE.s_ICON);
		});
	}

	public void SetupChip(ChipInfo tChipInfo, NetChipInfo inputNetChipInfo)
	{
		int chipID = inputNetChipInfo.ChipID;
		DISC_TABLE tDISC_TABLE = ManagedSingleton<OrangeDataManager>.Instance.DISC_TABLE_DICT[chipID];
		MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(AssetBundleScriptableObject.Instance.m_uiPath + "CommonIconBaseSmall", "CommonIconBaseSmall", delegate(GameObject asset)
		{
			CommonIconBase component = Object.Instantiate(asset, base.transform).GetComponent<CommonIconBase>();
			EquipStar.gameObject.SetActive(false);
			component.SetPlayerChipInfo(tChipInfo, inputNetChipInfo);
			component.Setup(0, AssetBundleScriptableObject.Instance.m_iconChip, tDISC_TABLE.s_ICON);
		});
	}

	public void SetupEquip(NetEquipmentInfo inputNetEquipInfo)
	{
		int equipItemID = inputNetEquipInfo.EquipItemID;
		EQUIP_TABLE tEQUIP_TABLE = ManagedSingleton<OrangeDataManager>.Instance.EQUIP_TABLE_DICT[equipItemID];
		MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(AssetBundleScriptableObject.Instance.m_uiPath + "CommonIconBaseSmall", "CommonIconBaseSmall", delegate(GameObject asset)
		{
			CommonIconBase component = Object.Instantiate(asset, IconRoot.transform).GetComponent<CommonIconBase>();
			EquipStar.gameObject.SetActive(true);
			int[] equipRank = ManagedSingleton<EquipHelper>.Instance.GetEquipRank(inputNetEquipInfo);
			EquipStar.SetActiveStar(equipRank[3]);
			Level.text = tEQUIP_TABLE.n_LV.ToString();
			component.SetEquipInfo(inputNetEquipInfo);
			component.Setup(0, AssetBundleScriptableObject.Instance.m_iconEquip, tEQUIP_TABLE.s_ICON);
		});
	}
}
