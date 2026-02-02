using UnityEngine;
using UnityEngine.UI;

namespace StageLib
{
	public class LoadIconCallBack
	{
		private LoadIconCallBack tSelf;

		public Image TargetImage;

		public LoadIconCallBack()
		{
			tSelf = this;
		}

		public void LoadCB(Object asset)
		{
			Sprite sprite = asset as Sprite;
			TargetImage.sprite = sprite;
			tSelf = null;
		}
	}
}
