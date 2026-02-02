using System.IO;
using Newtonsoft.Json;

public class PerBuff
{
	[JsonIgnore]
	public CONDITION_TABLE refCTable;

	[JsonIgnore]
	public float fLeftTime;

	[JsonIgnore]
	public bool bWaitNetSyncAdd;

	[JsonIgnore]
	public bool bWaitNetDel;

	[JsonIgnore]
	public bool bWaitNetSyncStack;

	[JsonIgnore]
	public float fWaitNetSyncTimeOut = 2f;

	[JsonIgnore]
	public bool bWaitNetSyncTime;

	[JsonProperty("@a")]
	public int nBuffID;

	[JsonProperty("@b")]
	public int nStack;

	[JsonProperty("@c")]
	public float fDuration;

	[JsonProperty("@d")]
	public int nOtherParam1;

	[JsonProperty("@e")]
	public string sPlayerID = "";

	public void PartialWrite(BinaryWriter bw)
	{
		bw.Write(nBuffID);
		bw.Write(nStack);
		bw.WriteExFloat(fDuration);
		bw.Write(nOtherParam1);
		bw.WriteExString(sPlayerID);
	}

	public void PartialRead(BinaryReader br)
	{
		nBuffID = br.ReadInt32();
		nStack = br.ReadInt32();
		fDuration = br.ReadExFloat();
		nOtherParam1 = br.ReadInt32();
		sPlayerID = br.ReadExString();
	}

	public bool IsBuffOk()
	{
		return !bWaitNetDel;
	}
}
