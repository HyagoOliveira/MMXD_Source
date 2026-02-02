#define RELEASE
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DNAConvert : OrangeUIBase
{
	[SerializeField]
	private LoopVerticalScrollRect scrollRect;

	[SerializeField]
	private DNAConvertUnit itemIconRef;

	[SerializeField]
	private OrangeText totalDNACount;

	private int visualCount = 40;

	private List<NetItemInfo> listNetItemInfo = new List<NetItemInfo>();

	public void Setup()
	{
		UpdateTotalDNAPoint();
		UpdateItemScrollRect(true);
		base._EscapeEvent = EscapeEvent.CLOSE_UI;
	}

	public NetItemInfo GetItemInfo(int index)
	{
		if (listNetItemInfo.Count > index)
		{
			return listNetItemInfo[index];
		}
		return null;
	}

	private void GetNetItemInfoList()
	{
		int num = 4;
		int num2 = 1;
		listNetItemInfo.Clear();
		foreach (ItemInfo value in ManagedSingleton<PlayerNetManager>.Instance.dicItem.Values)
		{
			if (value.netItemInfo.Stack > 0)
			{
				ITEM_TABLE item = null;
				if (ManagedSingleton<OrangeTableHelper>.Instance.GetItem(value.netItemInfo.ItemID, out item) && item.n_TYPE == num && item.n_TYPE_X == num2)
				{
					listNetItemInfo.Add(value.netItemInfo);
				}
			}
		}
	}

	public void OnClickUnit(int index)
	{
		Debug.Log("Index = " + index);
		if (listNetItemInfo.Count <= index)
		{
			return;
		}
		NetItemInfo netItemInfo = listNetItemInfo[index];
		ITEM_TABLE itemTable = null;
		if (ManagedSingleton<OrangeTableHelper>.Instance.GetItem(netItemInfo.ItemID, out itemTable))
		{
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_DNAConvertConfirm", delegate(DNAConvertConfirm ui)
			{
				ui.Setup(itemTable, listNetItemInfo[index]);
			});
		}
	}

	public void UpdateItemScrollRect(bool bRebuild = false)
	{
		GetNetItemInfoList();
		if (bRebuild)
		{
			scrollRect.ClearCells();
			scrollRect.OrangeInit(itemIconRef, visualCount, listNetItemInfo.Count);
		}
		else
		{
			scrollRect.RefreshCells();
		}
	}

	public void UpdateTotalDNAPoint()
	{
		ItemInfo value;
		if (ManagedSingleton<PlayerNetManager>.Instance.dicItem.TryGetValue(OrangeConst.ITEMID_DNA_POINT, out value))
		{
			totalDNACount.text = value.netItemInfo.Stack.ToString();
		}
		else
		{
			totalDNACount.text = "0";
		}
	}
}
