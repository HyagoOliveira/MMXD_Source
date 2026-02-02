using System;
using UnityEngine;
using UnityEngine.UI;

namespace StageLib
{
	public class StageLoadIcon : Image
	{
		public string loadname = "";

		private string nowname = "";

		private Action endcb;

		protected override void Start()
		{
			base.Start();
		}

		public void CheckLoad(string path, string targetname, Action ecb = null)
		{
			loadname = targetname;
			if (nowname != loadname)
			{
				nowname = loadname;
				endcb = null;
				if (ecb != null)
				{
					endcb = ecb;
				}
				LoadIconCBWithName loadIconCBWithName = new LoadIconCBWithName();
				loadIconCBWithName.loadname = loadname;
				loadIconCBWithName.loadcb = LoadCB;
				MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad<Sprite>(path, nowname, loadIconCBWithName.LoadCB);
			}
		}

		public void CheckLoadT<T>(string path, string targetname, Action ecb = null) where T : UnityEngine.Object
		{
			loadname = targetname;
			if (nowname != loadname)
			{
				nowname = loadname;
				endcb = null;
				if (ecb != null)
				{
					endcb = ecb;
				}
				LoadIconCBWithName loadIconCBWithName = new LoadIconCBWithName();
				loadIconCBWithName.loadname = loadname;
				loadIconCBWithName.loadcb = LoadCB;
				MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad<T>(path, nowname, loadIconCBWithName.LoadCB);
			}
		}

		public void CheckLoadPerfab(string path, string targetname, Action ecb = null)
		{
			loadname = targetname;
			if (nowname != loadname)
			{
				nowname = loadname;
				endcb = null;
				if (ecb != null)
				{
					endcb = ecb;
				}
				LoadGameObjectCBWithName loadGameObjectCBWithName = new LoadGameObjectCBWithName();
				loadGameObjectCBWithName.loadname = loadname;
				loadGameObjectCBWithName.loadcb = LoadCBPrefab;
				MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetGameObjectAndAsyncLoad(path, nowname, loadGameObjectCBWithName.LoadCB);
			}
			else
			{
				endcb = null;
				if (ecb != null)
				{
					endcb = ecb;
					endcb();
				}
			}
		}

		private void LoadCB(UnityEngine.Object asset, string tloadname)
		{
			Sprite sprite = asset as Sprite;
			if (!(sprite == null) && !(tloadname != loadname))
			{
				base.sprite = sprite;
				if (endcb != null)
				{
					endcb();
				}
			}
		}

		private void LoadCBPrefab(GameObject go, string tloadname)
		{
			if (!(go == null) && !(tloadname != loadname))
			{
				for (int num = base.transform.childCount - 1; num >= 0; num--)
				{
					UnityEngine.Object.Destroy(base.transform.GetChild(num).gameObject);
				}
				StandBase component = go.GetComponent<StandBase>();
				if (component != null)
				{
					component.Setup(base.transform);
				}
				else
				{
					go.transform.SetParent(base.transform, false);
				}
				if (endcb != null)
				{
					endcb();
				}
			}
		}
	}
}
