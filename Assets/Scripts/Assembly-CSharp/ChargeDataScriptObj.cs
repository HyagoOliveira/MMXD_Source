using System.Collections.Generic;
using UnityEngine;

public class ChargeDataScriptObj : ScriptableObject
{
	public List<ChargeData> listChargeDatas = new List<ChargeData>();

	public void InitDefaultData()
	{
		listChargeDatas.Add(new ChargeData("default", "prefab/fx/chargefx", ChargeData.ChargeFXRoot.Bip, "fxduring_chargeshot_000_start", "fxduring_chargeshot_001_loop"));
	}

	public ChargeData GetChargeData(string sCharacterID)
	{
		for (int num = listChargeDatas.Count - 1; num >= 0; num--)
		{
			if (listChargeDatas[num].sCharacterStr == sCharacterID)
			{
				return listChargeDatas[num];
			}
		}
		for (int num2 = listChargeDatas.Count - 1; num2 >= 0; num2--)
		{
			if (listChargeDatas[num2].sCharacterStr == "default")
			{
				return listChargeDatas[num2];
			}
		}
		return new ChargeData(sCharacterID, "", ChargeData.ChargeFXRoot.Bip, "", "");
	}
}
