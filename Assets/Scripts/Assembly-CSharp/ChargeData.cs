using System;

[Serializable]
public class ChargeData
{
	public enum ChargeFXRoot
	{
		Bip = 0,
		Model = 1
	}

	public string sCharacterStr;

	public string sABDlPath;

	public ChargeFXRoot FXRoot;

	public string sChargeStartFX;

	public string sChargeLV1FX;

	public string sChargeLV2FX;

	public string sChargeLV3FX;

	public string sChargeStartFX2;

	public string sChargeLV1FX2;

	public string sChargeLV2FX2;

	public string sChargeLV3FX2;

	public int nUpdateInAdvanceLV1FX;

	public int nUpdateInAdvanceLV2FX;

	public int nUpdateInAdvanceLV3FX;

	public int nUpdateInAdvanceLV1FX2;

	public int nUpdateInAdvanceLV2FX2;

	public int nUpdateInAdvanceLV3FX2;

	public ChargeData(string isCharacterStr, string isABDlPath, ChargeFXRoot fxRoot, string isChargeLV1FX, string isChargeLV2FX, string isChargeLV3FX = "", string isChargeStartFX = "", string isChargeLV1FX2 = "", string isChargeLV2FX2 = "", string isChargeLV3FX2 = "", string isChargeStartFX2 = "")
	{
		sCharacterStr = isCharacterStr;
		sABDlPath = isABDlPath;
		FXRoot = fxRoot;
		sChargeStartFX = isChargeStartFX;
		sChargeLV1FX = isChargeLV1FX;
		sChargeLV2FX = isChargeLV2FX;
		sChargeLV3FX = isChargeLV3FX;
		sChargeStartFX2 = isChargeStartFX2;
		sChargeLV1FX2 = isChargeLV1FX2;
		sChargeLV2FX2 = isChargeLV2FX2;
		sChargeLV3FX2 = isChargeLV3FX2;
	}
}
