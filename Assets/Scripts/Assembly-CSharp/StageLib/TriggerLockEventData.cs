using UnityEngine;

namespace StageLib
{
	public class TriggerLockEventData
	{
		public string sTriggerPlayerID = "";

		public string sSyncID = "";

		public Vector3? vTriggerPos;

		public float fMin;

		public float fMax;

		public float fBtn;

		public float fTop;

		public float? fSpeed;

		public int? nType;

		public bool bLockNet;

		public bool? bSetFocus;

		public float? fOY;

		public float fWaitLockTime;

		public bool bSlowWhenMove;
	}
}
