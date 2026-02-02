using UnityEngine;

namespace StageLib
{
	[ExecuteInEditMode]
	public class StageGroupRoot : MonoBehaviour
	{
		private bool bInit;

		private void Awake()
		{
			if (!bInit)
			{
				bInit = true;
				base.gameObject.name = "Group" + GetInstanceID();
			}
		}

		public void SetSGroupID(string sgname)
		{
			bInit = true;
			base.gameObject.name = sgname;
		}

		public string GetSGroupID()
		{
			if (!bInit)
			{
				base.gameObject.name = "Group" + GetInstanceID();
				bInit = true;
			}
			return base.gameObject.name;
		}
	}
}
