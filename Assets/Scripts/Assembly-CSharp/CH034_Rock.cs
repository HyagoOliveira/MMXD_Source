using System.Collections;
using UnityEngine;

public class CH034_Rock : BasicBullet
{
	[SerializeField]
	protected float shiftTime = 0.25f;

	[SerializeField]
	protected float shiftUp = 10f;

	[SerializeField]
	protected Vector3 subBulletShiftPos = new Vector3(1f, 0f, 0f);

	protected int characterDirection = 1;

	protected bool setStartPoint;

	protected Vector3 startPoint = Vector3.zero;

	public override void Active(Vector3 pPos, Vector3 pDirection, LayerMask pTargetMask, IAimTarget pTarget = null)
	{
		setStartPoint = false;
		if (!isSubBullet)
		{
			pPos += Vector3.up * shiftUp;
			startPoint = pPos;
			setStartPoint = true;
			characterDirection = ((pDirection.x > 0f) ? 1 : (-1));
		}
		base.Active(pPos, Vector3.down, pTargetMask, pTarget);
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
			StartCoroutine(CallSubRock());
		}
	}

	private IEnumerator CallSubRock()
	{
		while (MonoBehaviourSingleton<OrangeGameManager>.Instance.IsGamePause)
		{
			yield return CoroutineDefine._waitForEndOfFrame;
		}
		yield return new WaitForSeconds(shiftTime);
		CreateSubRock(MasterPosition + subBulletShiftPos * characterDirection * 2f);
		while (MonoBehaviourSingleton<OrangeGameManager>.Instance.IsGamePause)
		{
			yield return CoroutineDefine._waitForEndOfFrame;
		}
		yield return new WaitForSeconds(shiftTime);
		CreateSubRock(MasterPosition + subBulletShiftPos * characterDirection * 1f);
	}

	protected override void MoveBullet()
	{
		if (setStartPoint)
		{
			setStartPoint = false;
			_transform.position = startPoint;
		}
		base.MoveBullet();
	}

	protected void CreateSubRock(Vector3 pos)
	{
		CH034_Rock poolObj = MonoBehaviourSingleton<PoolManager>.Instance.GetPoolObj<CH034_Rock>(BulletData.s_MODEL);
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
		poolObj.UpdateBulletData(BulletData, Owner);
		poolObj.BulletLevel = BulletLevel;
		poolObj.isSubBullet = true;
		poolObj.SetBulletAtk(weaponStatus, tBuffStatus);
		poolObj.transform.SetPositionAndRotation(pos, Quaternion.identity);
		poolObj.Active(pos, Direction, TargetMask);
	}
}
