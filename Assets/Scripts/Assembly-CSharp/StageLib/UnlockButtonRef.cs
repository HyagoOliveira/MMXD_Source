using System;
using UnityEngine;
using UnityEngine.UI;

namespace StageLib
{
	[Serializable]
	public class UnlockButtonRef : ButtonRef
	{
		public Image LockImg;

		public Text MsgText;

		public GameObject SelectObj;
	}
}
