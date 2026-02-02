using System.Collections.Generic;
using UnityEngine;

public class EM088EnemyDieCollider : EnemyDieCollider
{
	private List<Vector3> OriginalPos = new List<Vector3>();

	public override void ActiveExplosion(bool p_isSimulatorMode = false)
	{
		OriginalPos.Clear();
		for (int i = 0; i < Rigids.Length; i++)
		{
			OriginalPos.Add(Rigids[i].transform.localPosition);
		}
		base.ActiveExplosion(p_isSimulatorMode);
	}

	public override void BackToPool()
	{
		for (int i = 0; i < Rigids.Length; i++)
		{
			Rigids[i].isKinematic = true;
			Rigids[i].transform.localPosition = OriginalPos[i];
			Rigids[i].transform.rotation = Quaternion.identity;
		}
		if (!isSimulatorMode)
		{
			MonoBehaviourSingleton<PoolManager>.Instance.BackToPool(this, Name);
		}
	}
}
