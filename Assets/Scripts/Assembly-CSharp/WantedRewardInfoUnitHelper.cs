using System;
using CallbackDefs;
using UnityEngine;
using UnityEngine.UI;

public class WantedRewardInfoUnitHelper : MonoBehaviour
{
	[SerializeField]
	private CommonIconBase _commonIcon;

	[SerializeField]
	private Text _textCount;

	[SerializeField]
	private GameObject _goReceived;

	private ITEM_TABLE _itemAttrData;

	private ItemInfoUI _itemInfoUI;

	public void Setup(int rewardId, int count, bool isReceived = false)
	{
		_itemAttrData = null;
		ITEM_TABLE item;
		if (ManagedSingleton<OrangeTableHelper>.Instance.GetItem(rewardId, out item))
		{
			_itemAttrData = item;
			if (item.n_TYPE == 5 && item.n_TYPE_X == 1 && (int)item.f_VALUE_Y > 0)
			{
				_commonIcon.SetupItemForCard(rewardId, 0, OnClickIcon);
			}
			else
			{
				_commonIcon.SetupItem(rewardId, 0, OnClickIcon);
			}
		}
		_textCount.text = string.Format("X {0}", count);
		_goReceived.SetActive(isReceived);
	}

	private void OnDestroy()
	{
		if (_itemInfoUI != null)
		{
			_itemInfoUI.OnClickCloseBtn();
			_itemInfoUI = null;
		}
	}

	private void OnClickIcon(int rewardId)
	{
		if (_itemAttrData != null)
		{
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI<ItemInfoUI>("UI_ItemInfo", OnItemInfoUILoaded);
		}
	}

	private void OnItemInfoUILoaded(ItemInfoUI ui)
	{
		ui.CanShowHow2Get = false;
		ui.Setup(_itemAttrData);
		ui.closeCB = (Callback)Delegate.Combine(ui.closeCB, new Callback(OnItemInfoUIClosed));
		_itemInfoUI = ui;
	}

	private void OnItemInfoUIClosed()
	{
		_itemInfoUI = null;
	}
}
