using System.Collections.Generic;
using Better;
using CallbackDefs;

internal interface IOrangeIAP
{
	void DoPurchase(SHOP_TABLE p_shopTable, OrangeProduct p_product);

	void Init(Callback initCB);

	Better.Dictionary<string, OrangeProduct> GetDictProductVaild(ref List<string> listVaildProduct);
}
