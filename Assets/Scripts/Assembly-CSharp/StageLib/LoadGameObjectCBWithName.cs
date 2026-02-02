using System;
using UnityEngine;

namespace StageLib
{
	internal class LoadGameObjectCBWithName
	{
		private LoadGameObjectCBWithName tSelf;

		public string loadname = "";

		public Action<GameObject, string> loadcb;

		public LoadGameObjectCBWithName()
		{
			tSelf = this;
		}

		public void LoadCB(GameObject asset)
		{
			if (loadcb != null)
			{
				loadcb(asset, loadname);
			}
			tSelf = null;
		}
	}
}
