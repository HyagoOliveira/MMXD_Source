using TwitterKit.Unity;
using UnityEngine;

namespace TwitterKit.Internal
{
	public class TwitterInit : MonoBehaviour
	{
		private static TwitterInit instance;

		private void Awake()
		{
			if (instance == null)
			{
				AwakeOnce();
				instance = this;
				Object.DontDestroyOnLoad(this);
			}
			else if (instance != this)
			{
				Object.Destroy(base.gameObject);
			}
		}

		private void AwakeOnce()
		{
			Twitter.AwakeInit();
		}
	}
}
