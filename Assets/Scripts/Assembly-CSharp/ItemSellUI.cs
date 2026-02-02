using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemSellUI : ItemInfoBase
{
	private string sellFormat = "{0}/{1}";

	private string sellValueFormat = "x {0}";

	[SerializeField]
	private Text textSell;

	[SerializeField]
	private Slider slider;

	[SerializeField]
	private Text textSellValue;

	[SerializeField]
	private IconBase iconBase;

	private int amountMax;

	private int amountCache;

	private bool isSelling;

	private float sliderAddValue;

	private SystemSE BtnSE = SystemSE.CRI_SYSTEMSE_SYS_OK03;

	private int amountNow;

	public override void Setup(ITEM_TABLE p_item, NetItemInfo p_netItem = null, int p_requestCount = 0)
	{
		base.Setup(p_item, p_netItem);
		amountMax = netItem.Stack;
		ITEM_TABLE sellGetItem = null;
		if (ManagedSingleton<OrangeTableHelper>.Instance.GetItem(item.n_SELL_ID, out sellGetItem))
		{
			iconBase.Setup(0, AssetBundleScriptableObject.Instance.GetIconItem(sellGetItem.s_ICON), sellGetItem.s_ICON, delegate
			{
				MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_ItemInfo", delegate(ItemInfoUI ui)
				{
					ui.Setup(sellGetItem);
				});
			});
		}
		sliderAddValue = 1f / (float)amountMax;
		textSell.text = string.Format(sellFormat, 0, amountMax);
		textSellValue.text = string.Format(sellValueFormat, 0);
		slider.onValueChanged.AddListener(OnSliderValueChange);
	}

	private void OnDisable()
	{
		slider.onValueChanged.RemoveListener(OnSliderValueChange);
	}

	private void OnSliderValueChange(float val)
	{
		amountNow = (int)((float)amountMax * val + 0.5f);
		if (amountNow != amountCache && amountNow >= 0 && amountNow <= amountMax)
		{
			if (!isSelling)
			{
				MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(BtnSE);
			}
			amountCache = amountNow;
			textSell.text = string.Format(sellFormat, amountCache, amountMax);
			textSellValue.text = string.Format(sellValueFormat, amountCache * item.n_SELL_COUNT);
			itemIcon.SetAmount(amountMax);
		}
	}

	protected override void OnClickSold()
	{
		if (amountCache <= 0 || isSelling)
		{
			return;
		}
		isSelling = true;
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_STORE02);
		ManagedSingleton<PlayerNetManager>.Instance.ItemSellReq(netItem.ItemID, amountCache, delegate(List<NetRewardInfo> p_param)
		{
			ItemInfo value = null;
			if (ManagedSingleton<PlayerNetManager>.Instance.dicItem.TryGetValue(netItem.ItemID, out value))
			{
				if (value.netItemInfo.Stack <= 0)
				{
					OnClickCloseBtn();
				}
				else
				{
					netItem = value.netItemInfo;
					amountMax = netItem.Stack;
					sliderAddValue = 1f / (float)amountMax;
					slider.value = 0f;
					isSelling = false;
				}
				List<NetRewardInfo> rewardList = p_param;
				if (rewardList != null && rewardList.Count > 0)
				{
					MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_RewardPopup", delegate(RewardPopopUI ui)
					{
						ui.Setup(rewardList);
					});
				}
			}
			else
			{
				OnClickCloseBtn();
			}
		});
	}

	public void OnClickAddBtn(int i)
	{
		if (i > 0)
		{
			slider.value += (float)i * sliderAddValue;
		}
		else if (i < 0)
		{
			slider.value += (float)i * sliderAddValue;
		}
	}

	public void OnClickMaxSellBtn()
	{
		slider.value = 1f;
	}

	public void OnClickMinSellBtn()
	{
		slider.value += (float)(-amountNow) * sliderAddValue;
	}
}
