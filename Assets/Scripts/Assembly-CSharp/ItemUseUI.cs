using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemUseUI : ItemInfoBase
{
	private readonly string format = "{0}/{1}";

	[SerializeField]
	private Text textUse;

	[SerializeField]
	private Slider slider;

	private int amountMax;

	private int amountCache;

	private bool isUsing;

	private string prvString;

	private float sliderAddValue;

	private bool SEPlayOne;

	private int amountNow;

	public override void Setup(ITEM_TABLE p_item, NetItemInfo p_netItem = null, int p_requestCount = 0)
	{
		base.Setup(p_item, p_netItem);
		amountMax = Mathf.Min(netItem.Stack, OrangeConst.ITEM_USE_LIMIT);
		sliderAddValue = 1f / (float)amountMax;
		prvString = string.Format(format, 0, amountMax);
		textUse.text = prvString;
		SEPlayOne = true;
		OnSliderValueChange(1f);
		slider.value = 1f;
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
			amountCache = amountNow;
			setTextUse(string.Format(format, amountCache, amountMax));
		}
	}

	private void setTextUse(string str)
	{
		if (!(prvString == str))
		{
			if (!SEPlayOne)
			{
				MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK03);
			}
			else
			{
				SEPlayOne = false;
			}
			prvString = str;
			textUse.text = str;
		}
	}

	public void OnClickUse()
	{
		if (amountCache <= 0 || isUsing)
		{
			return;
		}
		isUsing = true;
		ManagedSingleton<PlayerNetManager>.Instance.ItemUseReq(netItem.ItemID, amountCache, (int)item.f_VALUE_X, delegate(List<NetRewardInfo> p_param)
		{
			Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.UPDATE_PLAYER_BOX);
			ItemInfo value = null;
			if (ManagedSingleton<PlayerNetManager>.Instance.dicItem.TryGetValue(netItem.ItemID, out value))
			{
				if (value.netItemInfo.Stack <= 0)
				{
					base.CloseSE = SystemSE.NONE;
					OnClickCloseBtn();
				}
				else
				{
					netItem = value.netItemInfo;
					amountMax = Mathf.Min(netItem.Stack, OrangeConst.ITEM_USE_LIMIT);
					sliderAddValue = 1f / (float)amountMax;
					OnSliderValueChange(1f);
					slider.value = 1f;
					isUsing = false;
					itemIcon.SetAmount(netItem.Stack);
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
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK05);
		SEPlayOne = true;
	}

	public void OnClickAddBtn(int i)
	{
		slider.value += (float)i * sliderAddValue;
	}

	public void OnClickMaxUseBtn()
	{
		slider.value = 1f;
	}

	public void OnClickMinUseBtn()
	{
		slider.value = 0f;
	}
}
