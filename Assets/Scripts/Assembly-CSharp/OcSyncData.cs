using System;
using System.Collections.Generic;
using System.IO;

public class OcSyncData
{
	public SortedDictionary<ESyncData, UpdateTimer> dicUpdateTimer = new SortedDictionary<ESyncData, UpdateTimer>();

	public SortedDictionary<ESyncData, int> dicIntParams = new SortedDictionary<ESyncData, int>();

	public SortedDictionary<ESyncData, string> dicStringParams = new SortedDictionary<ESyncData, string>();

	public List<PerBuff> listPerBuff = new List<PerBuff>();

	public OcSyncData()
	{
		foreach (ESyncData value in Enum.GetValues(typeof(ESyncData)))
		{
			if (value >= ESyncData.WEAPON1_CHARGE && value <= ESyncData.SKILL2_LASTUSE)
			{
				dicUpdateTimer[value] = new UpdateTimer();
			}
			else if (value >= ESyncData.INT_PARAMS_WEAPON1_MAGZ && value <= ESyncData.INT_PARAMS_BUFF_MEASURE)
			{
				dicIntParams[value] = 0;
			}
			else if (value >= ESyncData.STRING_PARAMS_SERIALID && value <= ESyncData.STRING_PARAMS_SERIALID)
			{
				dicStringParams[value] = "";
			}
		}
	}

	public int MakeRuntimeDiff(OcSyncData compareData)
	{
		int num = 0;
		foreach (ESyncData value in Enum.GetValues(typeof(ESyncData)))
		{
			if (value >= ESyncData.WEAPON1_CHARGE && value <= ESyncData.SKILL2_LASTUSE)
			{
				if (dicUpdateTimer[value] != compareData.dicUpdateTimer[value])
				{
					num |= (int)value;
				}
			}
			else if (value >= ESyncData.INT_PARAMS_WEAPON1_MAGZ && value <= ESyncData.INT_PARAMS_BUFF_MEASURE)
			{
				if (dicIntParams[value] != compareData.dicIntParams[value])
				{
					num |= (int)value;
				}
			}
			else if (value >= ESyncData.STRING_PARAMS_SERIALID && value <= ESyncData.STRING_PARAMS_SERIALID)
			{
				if (dicStringParams[value] != compareData.dicStringParams[value])
				{
					num |= (int)value;
				}
			}
			else if (value >= ESyncData.LIST_BUFF && value <= ESyncData.LIST_BUFF)
			{
				num |= (int)value;
			}
		}
		return num;
	}

	public void RecordByRuntimeDiff(BinaryWriter bw, int runtimeDiff)
	{
		bw.Write(runtimeDiff);
		foreach (KeyValuePair<ESyncData, UpdateTimer> item in dicUpdateTimer)
		{
			if ((int)((uint)runtimeDiff & (uint)item.Key) > 0)
			{
				item.Value.Write(bw);
			}
		}
		foreach (KeyValuePair<ESyncData, int> dicIntParam in dicIntParams)
		{
			if ((int)((uint)runtimeDiff & (uint)dicIntParam.Key) > 0)
			{
				bw.Write(dicIntParam.Value);
			}
		}
		foreach (KeyValuePair<ESyncData, string> dicStringParam in dicStringParams)
		{
			if ((int)((uint)runtimeDiff & (uint)dicStringParam.Key) > 0)
			{
				bw.WriteExString(dicStringParam.Value);
			}
		}
		if ((runtimeDiff & 0x2000000) > 0)
		{
			bw.Write((byte)listPerBuff.Count);
			for (int i = 0; i < listPerBuff.Count; i++)
			{
				listPerBuff[i].PartialWrite(bw);
			}
		}
	}

	public void CombineRuntimeDiff(BinaryReader br)
	{
		int num = br.ReadInt32();
		foreach (ESyncData value in Enum.GetValues(typeof(ESyncData)))
		{
			if ((int)((uint)num & (uint)value) <= 0)
			{
				continue;
			}
			if (value >= ESyncData.WEAPON1_CHARGE && value <= ESyncData.SKILL2_LASTUSE)
			{
				dicUpdateTimer[value].Read(br);
			}
			else if (value >= ESyncData.INT_PARAMS_WEAPON1_MAGZ && value <= ESyncData.INT_PARAMS_BUFF_MEASURE)
			{
				dicIntParams[value] = br.ReadInt32();
			}
			else if (value >= ESyncData.STRING_PARAMS_SERIALID && value <= ESyncData.STRING_PARAMS_SERIALID)
			{
				dicStringParams[value] = br.ReadExString();
			}
			else if (value >= ESyncData.LIST_BUFF && value <= ESyncData.LIST_BUFF)
			{
				listPerBuff = new List<PerBuff>();
				byte b = br.ReadByte();
				for (int i = 0; i < b; i++)
				{
					PerBuff perBuff = new PerBuff();
					perBuff.PartialRead(br);
					listPerBuff.Add(perBuff);
				}
			}
		}
	}
}
