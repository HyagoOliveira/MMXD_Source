using UnityEngine;

public class StageObjParam : MonoBehaviour
{
	public int nSCEID;

	public int nEventID;

	public int nSubPartID;

	public StageObjBase tLinkSOB;

	public RuntimeAnimatorController tDefaultAnimator;

	public static EnemyControllerBase GetEnemyControllBase(Transform tTrans)
	{
		StageObjParam component = tTrans.GetComponent<StageObjParam>();
		EnemyControllerBase enemyControllerBase = null;
		if (component != null)
		{
			return component.tLinkSOB as EnemyControllerBase;
		}
		return tTrans.GetComponent<EnemyControllerBase>();
	}
}
