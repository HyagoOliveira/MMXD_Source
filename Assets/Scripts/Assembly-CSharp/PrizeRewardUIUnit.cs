using CallbackDefs;
using UnityEngine;
using UnityEngine.UI;
using enums;

public class PrizeRewardUIUnit : MonoBehaviour
{
	[SerializeField]
	private Image bg0;

	[SerializeField]
	private Image bg1;

	[SerializeField]
	private OrangeText textTitle;

	[SerializeField]
	private OrangeText textName;

	[SerializeField]
	private RewardPopupUIUnit RewardUnit;

    [System.Obsolete]
    public void Setup(Sprite sprite0, Sprite sprite1, string titleKey, Color colorTitle, NetRewardInfo rewardInfo)
	{
		bg0.overrideSprite = sprite0;
		bg1.overrideSprite = sprite1;
		textTitle.color = colorTitle;
		textTitle.UpdateText(titleKey);
		string bundlePath = string.Empty;
		string assetPath = string.Empty;
		int rare = 0;
		MonoBehaviourSingleton<OrangeGameManager>.Instance.GetRewardSpritePath(rewardInfo, ref bundlePath, ref assetPath, ref rare);
		CallbackIdx p_cb = null;
		switch ((RewardType)rewardInfo.RewardType)
		{
		case RewardType.Item:
		{
			ITEM_TABLE item = ManagedSingleton<ExtendDataHelper>.Instance.ITEM_TABLE_DICT[rewardInfo.RewardID];
			rare = item.n_RARE;
			textName.text = ManagedSingleton<OrangeTextDataManager>.Instance.ITEMTEXT_TABLE_DICT.GetL10nValue(item.w_NAME);
			RewardUnit.SetPieceActive(item.n_TYPE == 4);
			p_cb = delegate
			{
				MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_ItemInfo", delegate(ItemInfoUI ui)
				{
					ui.CanShowHow2Get = false;
					ui.Setup(item);
				});
			};
			break;
		}
		case RewardType.Weapon:
		{
			WEAPON_TABLE wEAPON_TABLE = ManagedSingleton<OrangeDataManager>.Instance.WEAPON_TABLE_DICT[rewardInfo.RewardID];
			textName.text = ManagedSingleton<OrangeTextDataManager>.Instance.WEAPONTEXT_TABLE_DICT.GetL10nValue(wEAPON_TABLE.w_NAME);
			break;
		}
		case RewardType.Character:
		{
			CHARACTER_TABLE cHARACTER_TABLE = ManagedSingleton<OrangeDataManager>.Instance.CHARACTER_TABLE_DICT[rewardInfo.RewardID];
			textName.text = ManagedSingleton<OrangeTextDataManager>.Instance.CHARATEXT_TABLE_DICT.GetL10nValue(cHARACTER_TABLE.w_NAME);
			break;
		}
		case RewardType.Equipment:
		{
			EQUIP_TABLE eQUIP_TABLE = ManagedSingleton<OrangeDataManager>.Instance.EQUIP_TABLE_DICT[rewardInfo.RewardID];
			textName.text = ManagedSingleton<OrangeTextDataManager>.Instance.EQUIPTEXT_TABLE_DICT.GetL10nValue(eQUIP_TABLE.w_NAME);
			break;
		}
		case RewardType.Chip:
		{
			DISC_TABLE dISC_TABLE = ManagedSingleton<OrangeDataManager>.Instance.DISC_TABLE_DICT[rewardInfo.RewardID];
			textName.text = ManagedSingleton<OrangeTextDataManager>.Instance.DISCTEXT_TABLE_DICT.GetL10nValue(dISC_TABLE.w_NAME);
			break;
		}
		}
		RewardUnit.IgonreTween();
		RewardUnit.Setup(0, bundlePath, assetPath, rare, rewardInfo.Amount, p_cb);
	}
}
