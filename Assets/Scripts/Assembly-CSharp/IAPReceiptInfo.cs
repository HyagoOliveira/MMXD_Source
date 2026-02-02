using System.Collections.Generic;
using System.Linq;
using enums;

public class IAPReceiptInfo
{
	public List<NetIAPReceiptInfo> ListNetIAPReceiptInfo = new List<NetIAPReceiptInfo>();

	public List<NetIAPReceiptInfo> GetReceiptByStoreType(IAPStoreType storeType)
	{
		sbyte confirmType = (sbyte)storeType;
		return ListNetIAPReceiptInfo.Where((NetIAPReceiptInfo x) => x.StoreType == confirmType).ToList();
	}
}
