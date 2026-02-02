using System;
using UnityEngine;

public class ModelStatusCC : MonoBehaviour
{
	[Serializable]
	public class ActTimeData
	{
		public int nActIndex;

		public float fActionTime;

		public float fWaitTime;

		public bool bTriggerSkill;

		public bool bShowFX;

		public bool bFXFloow;

		public string sFxName;

		public int vx;

		public int vy;

		public bool bFly;

		public bool bZeroX = true;

		public bool bZeroY;

		public bool bZeroCollideBullet;
	}

	[Serializable]
	public class ModelStatusData
	{
		public string[] sAnimationNames;

		public ActTimeData[] fActTimeDatas;
	}

	[Serializable]
	public class ExplosionFxInfo
	{
		[SerializeField]
		private bool enable;

		[SerializeField]
		private float offsetX;

		[SerializeField]
		private float offsetY;

		[SerializeField]
		private float size;

		public void Play(Vector3 position)
		{
			if (enable)
			{
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("FX_MOB_EXPLODE0", new Vector3(position.x + offsetX, position.y + offsetY, position.z), Quaternion.identity, Array.Empty<object>());
			}
		}
	}

	public ModelStatusData[] ModelStatusDatas = new ModelStatusData[12];

	[SerializeField]
	public ExplosionFxInfo explosionFxInfo;

	[SerializeField]
	public EnemyDieCollider ExplosionPart;
}
