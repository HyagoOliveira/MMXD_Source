using System;

namespace StageLib
{
	public class RunUpdateClass
	{
		private float fNowUpdateTimeDelta;

		public Action<RunUpdateClass> tUpdateCB;

		public Action<RunUpdateClass> tEndCB;

		public bool bIsEnd;

		public object[] oParams;

		public float[] fParams;

		private RunUpdateClass refSelf;

		public RunUpdateClass()
		{
			fNowUpdateTimeDelta = 0f;
			refSelf = this;
		}

		public void EndCallBack()
		{
			if (tEndCB != null)
			{
				tEndCB(this);
				refSelf = null;
			}
		}

		public void UpdateCall(EventPointBase tEPB)
		{
			fNowUpdateTimeDelta += 0.016f;
			if (fNowUpdateTimeDelta > 0.016f)
			{
				while (fNowUpdateTimeDelta > 0.016f)
				{
					tUpdateCB(this);
					fNowUpdateTimeDelta -= 0.016f;
				}
				fNowUpdateTimeDelta = 0f;
			}
			if (bIsEnd)
			{
				tEPB.SiCoroutineUpdate = (Action<EventPointBase>)Delegate.Remove(tEPB.SiCoroutineUpdate, new Action<EventPointBase>(UpdateCall));
				refSelf = null;
			}
		}
	}
}
