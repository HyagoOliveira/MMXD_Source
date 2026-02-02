using UnityEngine;
using UnityEngine.UI;
using enums;

public class GachaCeilingUIUnit : ScrollIndexCallback
{
	[SerializeField]
	protected GachaCeilingUI parent;

	[SerializeField]
	private ItemIconBase iconExchange;

	[SerializeField]
	private OrangeText textName;

	[SerializeField]
	private Text iconCostValue;

	[SerializeField]
	private Image imgUnlock;

	[SerializeField]
	private OrangeText textUnlock;

	[SerializeField]
	private Canvas canvasRareRoot;

	[HideInInspector]
	public int NowIdx = -1;

	private bool rewardExist;

	private GACHA_TABLE exchangeData;

	public override void ScrollCellIndex(int p_idx)
	{
		NowIdx = p_idx;
		rewardExist = false;
		exchangeData = parent.ListExchange[NowIdx];
		switch ((RewardType)(short)exchangeData.n_REWARD_TYPE)
		{
		case RewardType.Weapon:
			SetWeaponData(exchangeData.n_REWARD_ID);
			SetCostData();
			break;
		case RewardType.Character:
			SetCharacterData(exchangeData.n_REWARD_ID);
			SetCostData();
			break;
		default:
			SetEmptyData();
			break;
		}
	}

	private void SetWeaponData(int weaponId)
	{
		WEAPON_TABLE weaponTable = ManagedSingleton<OrangeTableHelper>.Instance.GetWeaponTable(weaponId);
		if (weaponTable == null)
		{
			SetEmptyData();
			return;
		}
		SetUnlockInfo(ManagedSingleton<PlayerNetManager>.Instance.dicWeapon.ContainsKey(weaponTable.n_ID));
		iconExchange.gameObject.SetActive(true);
		iconExchange.Setup(0, AssetBundleScriptableObject.Instance.m_iconWeapon, weaponTable.s_ICON);
		iconExchange.SetRare(weaponTable.n_RARITY);
		textName.text = ManagedSingleton<OrangeTextDataManager>.Instance.WEAPONTEXT_TABLE_DICT.GetL10nValue(weaponTable.w_NAME);
		if ((bool)canvasRareRoot)
		{
			canvasRareRoot.enabled = ((weaponTable.n_RARITY >= 5) ? true : false);
		}
		rewardExist = true;
	}

	private void SetCharacterData(int characterId)
	{
		CHARACTER_TABLE characterTable = ManagedSingleton<OrangeTableHelper>.Instance.GetCharacterTable(characterId);
		if (characterTable == null)
		{
			SetEmptyData();
			return;
		}
		SetUnlockInfo(ManagedSingleton<PlayerNetManager>.Instance.dicCharacter.ContainsKey(characterTable.n_ID));
		iconExchange.gameObject.SetActive(true);
		iconExchange.Setup(0, AssetBundleScriptableObject.Instance.GetIconCharacter("icon_" + characterTable.s_ICON), "icon_" + characterTable.s_ICON);
		iconExchange.SetRare(characterTable.n_RARITY);
		textName.text = ManagedSingleton<OrangeTextDataManager>.Instance.CHARATEXT_TABLE_DICT.GetL10nValue(characterTable.w_NAME);
		if ((bool)canvasRareRoot)
		{
			canvasRareRoot.enabled = ((characterTable.n_RARITY >= 5) ? true : false);
		}
		rewardExist = true;
	}

	private void SetEmptyData()
	{
		canvasRareRoot.enabled = false;
		iconExchange.gameObject.SetActive(false);
		SetUnlockInfo(false);
	}

	private void SetCostData()
	{
		iconCostValue.text = "x" + OrangeConst.GACHA_SELECT_MAX;
	}

	private void SetUnlockInfo(bool isUnlock)
	{
		imgUnlock.color = (isUnlock ? Color.white : Color.clear);
		textUnlock.text = (isUnlock ? MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("UI_UNLOCKED") : string.Empty);
	}

	public void OnClickBtnExchange()
	{
		if (rewardExist)
		{
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_GachaCeilingExchange", delegate(GachaCeilingExchangeUI ui)
			{
				MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
				ui.Setup(parent.NowGachaList[0], exchangeData, parent.TotalSetupDrawCount);
			});
		}
	}
}
