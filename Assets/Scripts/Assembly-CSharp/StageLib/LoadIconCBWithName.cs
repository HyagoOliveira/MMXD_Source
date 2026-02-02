using System;
using UnityEngine;

namespace StageLib
{
	internal class LoadIconCBWithName
	{
		private LoadIconCBWithName tSelf;

		public string loadname = "";

		public Action<UnityEngine.Object, string> loadcb;

		public LoadIconCBWithName()
		{
			tSelf = this;
		}

		public void LoadCB(UnityEngine.Object asset)
		{
			if (loadcb != null)
			{
				loadcb(asset, loadname);
			}
			tSelf = null;
		}
	}
}
