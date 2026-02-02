#define RELEASE
using UnityEngine;
using UnityEngine.UI;

public class RewardListCellHelper : MonoBehaviour
{
	[SerializeField]
	private ImageSpriteSwitcher _imageBGSwitcher;

	[SerializeField]
	private ImageSpriteSwitcher _imageXSwitcher;

	[SerializeField]
	private Text _textItemName;

	[SerializeField]
	private Text _textItemSet;

	[SerializeField]
	private CommonIconBase _rewardIcon;

	public void Setup(int itemId, int amoutMin, int amoutMax, int bgIndex = 0, int xIndex = 0, string itemSet = "")
	{
		SetItemName(itemId);
		SetBackground(bgIndex, xIndex);
		_textItemSet.text = itemSet;
		CommonUIHelper.SetCommonIcon(_rewardIcon, itemId, amoutMin, amoutMax, OnClickRewardIcon);
	}

	public void SetItemName(int itemId)
	{
		string itemName = string.Empty;
		ITEM_TABLE item;
		if (ManagedSingleton<OrangeTableHelper>.Instance.GetItem(itemId, out item))
		{
			itemName = ManagedSingleton<OrangeTextDataManager>.Instance.ITEMTEXT_TABLE_DICT.GetL10nValue(item.w_NAME);
		}
		SetItemName(itemName);
	}

	public void SetItemName(string itemName)
	{
		_textItemName.text = itemName;
	}

	public void SetBackground(int bgIndex, int xIndex)
	{
		_imageBGSwitcher.ChangeImage(bgIndex);
		_imageXSwitcher.ChangeImage(xIndex);
	}

	private void OnClickRewardIcon(int itemId)
	{
		ITEM_TABLE itemAttrData;
		if (!ManagedSingleton<ExtendDataHelper>.Instance.ITEM_TABLE_DICT.TryGetValue(itemId, out itemAttrData))
		{
			Debug.LogError(string.Format("Invalid ItemId : {0}", itemId));
			return;
		}
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_ItemInfo", delegate(ItemInfoUI ui)
		{
			if (itemAttrData.n_TYPE == 5 && itemAttrData.n_TYPE_X == 1 && (int)itemAttrData.f_VALUE_Y > 0)
			{
				CARD_TABLE value;
				if (ManagedSingleton<OrangeDataManager>.Instance.CARD_TABLE_DICT.TryGetValue((int)itemAttrData.f_VALUE_Y, out value))
				{
					ui.CanShowHow2Get = false;
					ui.Setup(value, itemAttrData);
				}
			}
			else
			{
				ui.CanShowHow2Get = false;
				ui.Setup(itemAttrData);
			}
		});
	}
}
