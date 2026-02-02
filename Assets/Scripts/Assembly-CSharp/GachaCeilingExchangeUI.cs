using OrangeApi;
using UnityEngine;
using UnityEngine.UI;
using enums;

public class GachaCeilingExchangeUI : OrangeUIBase
{
	[SerializeField]
	private ItemIconBase iconExchange;

	[SerializeField]
	private OrangeText textExchangeMsg;

	[SerializeField]
	private Image imgCost;

	[SerializeField]
	private OrangeText textCostBefore;

	[SerializeField]
	private OrangeText textCostAfter;

	[SerializeField]
	private OrangeText textUnlock;

	[SerializeField]
	private Canvas canvasRareRoot;

	[SerializeField]
	private Button btnExchange;

	private GACHALIST_TABLE gachaListTable;

	private GACHA_TABLE exchangeData;

	private int costBefore;

	private int costAfter = -1;

	protected override void Awake()
	{
		base.Awake();
		btnExchange.interactable = false;
	}

	public void Setup(GACHALIST_TABLE p_gachaListTable, GACHA_TABLE p_exchangeData, int p_drawCount)
	{
		base.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
		textExchangeMsg.alignByGeometry = false;
		gachaListTable = p_gachaListTable;
		exchangeData = p_exchangeData;
		costBefore = p_drawCount;
		costAfter = costBefore - OrangeConst.GACHA_SELECT_MAX;
		textCostBefore.text = costBefore.ToString();
		textCostAfter.text = costAfter.ToString();
		if (costAfter >= 0)
		{
			textCostAfter.color = Color.white;
			btnExchange.interactable = true;
		}
		else
		{
			textCostAfter.color = Color.red;
			btnExchange.interactable = false;
		}
		SetExchangeRewardData();
		base._EscapeEvent = EscapeEvent.CLOSE_UI;
	}

	private void SetExchangeRewardData()
	{
		switch ((RewardType)(short)exchangeData.n_REWARD_TYPE)
		{
		case RewardType.Weapon:
			SetWeaponData(exchangeData.n_REWARD_ID);
			break;
		case RewardType.Character:
			SetCharacterData(exchangeData.n_REWARD_ID);
			break;
		}
	}

	private void SetWeaponData(int weaponId)
	{
		WEAPON_TABLE weaponTable = ManagedSingleton<OrangeTableHelper>.Instance.GetWeaponTable(weaponId);
		iconExchange.Setup(0, AssetBundleScriptableObject.Instance.m_iconWeapon, weaponTable.s_ICON);
		iconExchange.SetRare(weaponTable.n_RARITY);
		textExchangeMsg.text = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("GACHA_SELECT_CONFIRM_TIP"), ManagedSingleton<OrangeTextDataManager>.Instance.WEAPONTEXT_TABLE_DICT.GetL10nValue(weaponTable.w_NAME));
		if ((bool)canvasRareRoot)
		{
			canvasRareRoot.enabled = ((weaponTable.n_RARITY >= 5) ? true : false);
		}
		SetUnlockInfo(ManagedSingleton<PlayerNetManager>.Instance.dicWeapon.ContainsKey(weaponTable.n_ID));
	}

	private void SetCharacterData(int characterId)
	{
		CHARACTER_TABLE characterTable = ManagedSingleton<OrangeTableHelper>.Instance.GetCharacterTable(characterId);
		iconExchange.Setup(0, AssetBundleScriptableObject.Instance.GetIconCharacter("icon_" + characterTable.s_ICON), "icon_" + characterTable.s_ICON);
		iconExchange.SetRare(characterTable.n_RARITY);
		textExchangeMsg.text = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("GACHA_SELECT_CONFIRM_TIP"), ManagedSingleton<OrangeTextDataManager>.Instance.CHARATEXT_TABLE_DICT.GetL10nValue(characterTable.w_NAME));
		if ((bool)canvasRareRoot)
		{
			canvasRareRoot.enabled = ((characterTable.n_RARITY >= 5) ? true : false);
		}
		SetUnlockInfo(ManagedSingleton<PlayerNetManager>.Instance.dicCharacter.ContainsKey(characterTable.n_ID));
	}

	private void SetUnlockInfo(bool isUnlock)
	{
		textUnlock.text = (isUnlock ? MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("UI_UNLOCKED") : string.Empty);
	}

	public void OnClickExchange()
	{
		if (costAfter < 0)
		{
			return;
		}
		ManagedSingleton<PlayerNetManager>.Instance.SetupGachaReq(gachaListTable.n_ID, exchangeData.n_ID, delegate(SetupGachaRes p_param)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_STORE02);
			MonoBehaviourSingleton<OrangeSceneManager>.Instance.ChangeScene("Gacha", OrangeSceneManager.LoadingType.TIP, delegate
			{
				MonoBehaviourSingleton<UIManager>.Instance.CloseAllUI(delegate
				{
					OrangeSceneManager.FindObjectOfTypeCustom<GachaSceneController>().Init(p_param.RewardEntities, true);
				});
			}, false);
		});
	}

	public override void OnClickCloseBtn()
	{
		base.OnClickCloseBtn();
	}
}
