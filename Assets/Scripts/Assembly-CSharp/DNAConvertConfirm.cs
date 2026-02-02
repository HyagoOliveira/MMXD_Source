using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DNAConvertConfirm : ItemInfoBase
{
	private string sellFormat = string.Empty;

	private string sellValueFormat = "x {0}";

	[SerializeField]
	private Text textSell;

	[SerializeField]
	private Slider slider;

	[SerializeField]
	private Text textSellValue;

	[SerializeField]
	private IconBase iconBase;

	[SerializeField]
	private OrangeText totalDNAValue;

	private int amountMax;

	private int amountCache;

	private bool isSelling;

	private DNAConvert parentUI;

	private float sliderAddValue;

	private SystemSE BtnSE = SystemSE.CRI_SYSTEMSE_SYS_OK03;

	private int amountNow;

	protected override void Awake()
	{
		base.Awake();
		sellFormat = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("DNA_POINT_CONVERT");
	}

	public override void Setup(ITEM_TABLE p_item, NetItemInfo p_netItem = null, int p_requestCount = 0)
	{
		base.Setup(p_item, p_netItem);
		parentUI = MonoBehaviourSingleton<UIManager>.Instance.GetUI<DNAConvert>("UI_DNAConvert");
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
		totalDNAValue.text = string.Format(sellValueFormat, 0);
		slider.onValueChanged.AddListener(OnSliderValueChange);
		base._EscapeEvent = EscapeEvent.CLOSE_UI;
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
			totalDNAValue.text = string.Format(sellValueFormat, amountCache * item.n_DNA_CONVERT);
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
		List<NetDNATransferUnit> list = new List<NetDNATransferUnit>();
		NetDNATransferUnit netDNATransferUnit = new NetDNATransferUnit();
		netDNATransferUnit.ItemID = item.n_ID;
		netDNATransferUnit.ItemCount = amountCache;
		list.Add(netDNATransferUnit);
		ManagedSingleton<PlayerNetManager>.Instance.TransferDNAReq(list, delegate(List<NetRewardInfo> p_param)
		{
			ItemInfo value = null;
			parentUI.UpdateTotalDNAPoint();
			if (ManagedSingleton<PlayerNetManager>.Instance.dicItem.TryGetValue(netItem.ItemID, out value))
			{
				if (value.netItemInfo.Stack <= 0)
				{
					parentUI.UpdateItemScrollRect(true);
					OnClickCloseBtn();
				}
				else
				{
					parentUI.UpdateItemScrollRect();
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

	public void OnClickMaxBtn()
	{
		slider.value = 1f;
	}

	public void OnClickMinBtn()
	{
		slider.value += (float)(-amountNow) * sliderAddValue;
	}
}
