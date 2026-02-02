using System.Collections;
using UnityEngine;

public class CH072_CallLightningBullet : BasicBullet
{
	protected int _nCount;

	protected int _nShootTimes = 1;

	protected int _nOneShootNum = 2;

	protected float _nNextShootTime = 0.2f;

	protected Vector3 _vOriginalPoint = Vector3.zero;

	public override void Active(Vector3 pPos, Vector3 pDirection, LayerMask pTargetMask, IAimTarget pTarget = null)
	{
		if (!isSubBullet)
		{
			_nCount = 0;
			_nShootTimes = BulletData.n_NUM_SHOOT / 2;
			_vOriginalPoint = pPos;
			pDirection = Vector3.down;
		}
		base.Active(pPos, pDirection, pTargetMask, pTarget);
	}

	public override void Active(Transform pTransform, Vector3 pDirection, LayerMask pTargetMask, IAimTarget pTarget = null)
	{
		Active(pTransform.position, pDirection, pTargetMask, pTarget);
	}

	public override void OnStart()
	{
		base.OnStart();
		if (!isSubBullet)
		{
			StartCoroutine(CallSubBullet());
		}
	}

	protected override void MoveBullet()
	{
	}

	public override void OnTriggerHit(Collider2D col)
	{
	}

	public override void BackToPool()
	{
		base.BackToPool();
		_nCount = 0;
	}

	private IEnumerator CallSubBullet()
	{
		for (int i = 0; i < _nShootTimes; i++)
		{
			while (MonoBehaviourSingleton<OrangeGameManager>.Instance.IsGamePause || MonoBehaviourSingleton<OrangeGameManager>.Instance.bLastGamePause)
			{
				yield return CoroutineDefine._waitForEndOfFrame;
			}
			ShootSubBullet(_nOneShootNum);
			yield return new WaitForSeconds(_nNextShootTime);
		}
		BackToPool();
	}

	protected void ShootSubBullet(int num)
	{
		for (int i = 0; i < num; i++)
		{
			if (_nCount >= BulletData.n_NUM_SHOOT)
			{
				break;
			}
			CallLightning();
		}
	}

	public void CaluStartPosition(int count, Vector3 originalPos, out Vector3 pos)
	{
		string[] array = BulletData.s_FIELD.Split(',');
		Vector2 vector = new Vector2(float.Parse(array[3]), float.Parse(array[4]));
		int num = count >> 1;
		float num2 = 1f + (vector.x - 1f) * (float)num * 2f / (float)BulletData.n_NUM_SHOOT;
		if ((count & 1) == 1)
		{
			num2 = 0f - num2;
		}
		float x = originalPos.x + num2;
		float y = _vOriginalPoint.y - 4.5f;
		pos = new Vector3(x, y, originalPos.z);
	}

	public override void SubBullet()
	{
	}

	public void CallLightning()
	{
		int n_LINK_SKILL = BulletData.n_LINK_SKILL;
		SKILL_TABLE tSKILL_TABLE = ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT[n_LINK_SKILL];
		if (refPBMShoter.SOB as OrangeCharacter != null)
		{
			(refPBMShoter.SOB as OrangeCharacter).tRefPassiveskill.ReCalcuSkill(ref tSKILL_TABLE);
		}
		CollideBullet poolObj = MonoBehaviourSingleton<PoolManager>.Instance.GetPoolObj<CollideBullet>(tSKILL_TABLE.s_MODEL);
		WeaponStatus weaponStatus = new WeaponStatus
		{
			nHP = nHp,
			nATK = nOriginalATK,
			nCRI = nOriginalCRI,
			nHIT = nHit - refPSShoter.GetAddStatus(8, nWeaponCheck),
			nCriDmgPercent = nCriDmgPercent,
			nReduceBlockPercent = nReduceBlockPercent,
			nWeaponCheck = nWeaponCheck,
			nWeaponType = nWeaponType
		};
		PerBuffManager.BuffStatus tBuffStatus = new PerBuffManager.BuffStatus
		{
			fAtkDmgPercent = fDmgFactor - 100f,
			fCriPercent = fCriFactor - 100f,
			fCriDmgPercent = fCriDmgFactor - 100f,
			fMissPercent = fMissFactor,
			refPBM = refPBMShoter,
			refPS = refPSShoter
		};
		poolObj.UpdateBulletData(tSKILL_TABLE, Owner);
		poolObj.BulletLevel = BulletLevel;
		poolObj.isSubBullet = true;
		poolObj.SetBulletAtk(weaponStatus, tBuffStatus);
		Vector3 pos = _vOriginalPoint;
		poolObj.transform.SetPositionAndRotation(_vOriginalPoint, Quaternion.identity);
		CaluStartPosition(_nCount, _vOriginalPoint, out pos);
		poolObj.Active(pos, Direction, TargetMask);
		_nCount++;
	}
}
