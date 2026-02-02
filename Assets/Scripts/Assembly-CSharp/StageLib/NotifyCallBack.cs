using System;

namespace StageLib
{
	public class NotifyCallBack
	{
		private NotifyCallBack tSelf;

		public Action cb;

		public Action<NotifyCallBack> cbparam;

		public int nParam0;

		public NotifyCallBack()
		{
			tSelf = this;
		}

		public void CallCB()
		{
			if (cb != null)
			{
				cb();
			}
			if (cbparam != null)
			{
				cbparam(this);
			}
			tSelf = null;
		}
	}
}
