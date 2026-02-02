using System;
using CallbackDefs;

namespace StageLib
{
	[Serializable]
	public class StageCtrlInsTruction
	{
		public int tStageCtrl;

		public float fTime;

		public float fWait;

		public float nParam1;

		public float nParam2;

		public string sMsg;

		[NonSerialized]
		public Callback RemoveCB;
	}
}
