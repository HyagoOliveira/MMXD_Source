using UnityEngine;

namespace StageLib
{
	[DisallowMultipleComponent]
	public abstract class StageSLBase : MonoBehaviour
	{
		[HideInInspector]
		public string sSyncID = "";

		public abstract int GetTypeID();

		public abstract string GetTypeString();

		public abstract string GetSaveString();

		public abstract void LoadByString(string sLoad);

		public virtual bool IsNeedClip()
		{
			return true;
		}

		public virtual bool IsNeedCheckClipAlone()
		{
			return false;
		}

		public virtual bool IsMapDependObj()
		{
			return false;
		}

		public virtual bool IsCanAddChild()
		{
			return false;
		}

		public abstract void OnSyncStageObj(string sIDKey, int nKey1, string smsg);

		public abstract void SyncNowStatus();

		public static string[] GetSyncKeys(StageSLBase tStageSLBase)
		{
			return GetSyncKeys(tStageSLBase.sSyncID);
		}

		public static string[] GetSyncKeys(string sInSyncID)
		{
			int num = sInSyncID.IndexOf('-');
			string[] array = new string[2];
			if (num != -1)
			{
				array[0] = sInSyncID.Substring(0, num);
				array[1] = sInSyncID.Substring(num + 1);
			}
			else
			{
				array[0] = sInSyncID;
				array[1] = "";
			}
			return array;
		}

		protected string GetBoolSaveStr(bool bSet)
		{
			if (bSet)
			{
				return "1";
			}
			return "0";
		}

		protected bool GetBoolBySaveStr(string sSave)
		{
			if (sSave == "1")
			{
				return true;
			}
			return false;
		}

		protected string GetVector3SaveStr(Vector3 tV)
		{
			return tV.x.ToString("0.00000") + "," + tV.y.ToString("0.00000") + "," + tV.z.ToString("0.00000");
		}

		protected Vector3 GetVector3BySaveStr(string[] sSaves, int nIndex = 0)
		{
			return new Vector3(float.Parse(sSaves[nIndex]), float.Parse(sSaves[nIndex + 1]), float.Parse(sSaves[nIndex + 2]));
		}

		protected void TryLoadFloat(string[] splitstrs, ref int nIndex, out float fSet)
		{
			fSet = 0f;
			if (splitstrs.Length > nIndex)
			{
				float.TryParse(splitstrs[nIndex++], out fSet);
			}
		}
	}
}
