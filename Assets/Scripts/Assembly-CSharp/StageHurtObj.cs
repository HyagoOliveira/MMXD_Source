using System.Collections;
using System.Collections.Generic;
using StageLib;
using UnityEngine;

public class StageHurtObj : MonoBehaviour
{
	public delegate int StageHurtObjCB(int dmg, Vector3 vDir, int nSkillID, StageObjBase tShotSOB);

	public Animation[] tAnimations;

	public int nBitParam;

	private Coroutine tHurtCoroutine;

	public List<int> listSkillID = new List<int>();

	public StageSceneObjParam[] tSSOPs;

	public MapCollisionEvent tMapCollisionEvent;

	public int nMaxHP;

	public int tempHP
	{
		get
		{
			if (tMapCollisionEvent != null)
			{
				return (int)tMapCollisionEvent.fParam0;
			}
			return 0;
		}
	}

	public event StageHurtObjCB HurtCB;

	public bool IsCanAtk(StageObjBase tSOB, SKILL_TABLE tSKILL_TABLE)
	{
		if (((uint)nBitParam & (true ? 1u : 0u)) != 0 && tSOB as OrangeCharacter != null)
		{
			return false;
		}
		if (((uint)nBitParam & 2u) != 0 && tSOB as EnemyControllerBase != null)
		{
			return false;
		}
		if (listSkillID.Count > 0)
		{
			if (listSkillID.Contains(tSKILL_TABLE.n_ID))
			{
				return true;
			}
			return false;
		}
		return true;
	}

	public int Hurt(int dmg, Vector3 vDir, int nSkillID, StageObjBase tShotSOB)
	{
		int result = 0;
		if (tHurtCoroutine == null)
		{
			tHurtCoroutine = StartCoroutine(HurtCoroutine());
		}
		if (this.HurtCB != null)
		{
			result = this.HurtCB(dmg, vDir, nSkillID, tShotSOB);
		}
		return result;
	}

	public void BrkoenAll()
	{
		if (!(tMapCollisionEvent != null))
		{
			return;
		}
		float num = nMaxHP;
		if (tMapCollisionEvent.fParam1 != 0f)
		{
			for (; num * (100f - tMapCollisionEvent.fParam1) * 0.01f < (float)tempHP; num += (float)nMaxHP)
			{
			}
			StageSceneObjParam[] array = tSSOPs;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].SwitchB2DInStageSceneObj(false);
			}
		}
		if (this.HurtCB != null)
		{
			this.HurtCB((int)num, Vector3.zero, 1, null);
		}
	}

	private IEnumerator HurtCoroutine()
	{
		yield return CoroutineDefine._waitForEndOfFrame;
		for (int i = 0; i < tSSOPs.Length; i++)
		{
			tSSOPs[i].SetRimMin(0.3f);
		}
		yield return new WaitForSeconds(0.04f);
		for (int j = 0; j < tSSOPs.Length; j++)
		{
			tSSOPs[j].SetRimMin(1f);
		}
		tHurtCoroutine = null;
	}
}
