using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace StageLib
{
	[Serializable]
	public class StageGroupData
	{
		[JsonConverter(typeof(FloatConverter))]
		public float fClipMinx;

		[JsonConverter(typeof(FloatConverter))]
		public float fClipMaxx;

		public List<StageObjData> Datas = new List<StageObjData>();
	}
}
