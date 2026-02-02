using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;

namespace StageLib
{
	public class StageData
	{
		public int nVer;

		public float fStageClipWidth;

		public List<StageGroupData> Datas = new List<StageGroupData>();

		public string ConvertJSON()
		{
			string s = JsonConvert.SerializeObject(new StageDataJson
			{
				nVer = nVer,
				fStageClipWidth = fStageClipWidth,
				Datas = Datas
			}, Formatting.None, JsonHelper.IgnoreLoopSetting());
			return AesCrypto.Encode(Convert.ToBase64String(LZ4Helper.EncodeWithHeader(Encoding.UTF8.GetBytes(s))));
		}

		public static StageData LoadByJSONStr(string str)
		{
			StageDataJson stageDataJson = null;
			string text = AesCrypto.Decode(str);
			if (text == "")
			{
				stageDataJson = JsonUtility.FromJson<StageDataJson>(str);
			}
			else
			{
				byte[] bytes = LZ4Helper.DecodeWithHeader(Convert.FromBase64String(text));
				stageDataJson = JsonUtility.FromJson<StageDataJson>(Encoding.UTF8.GetString(bytes));
			}
			return new StageData
			{
				Datas = stageDataJson.Datas,
				nVer = stageDataJson.nVer,
				fStageClipWidth = stageDataJson.fStageClipWidth
			};
		}

		public string ConvertJSON_HumanRead()
		{
			return "";
		}
	}
}
