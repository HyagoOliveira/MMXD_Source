using System;
using System.Collections;
using UnityEngine;

public class CH112_BarrageBullet : BasicBullet
{
	[SerializeField]
	private string Fx = "fxuse_barrage_000";

	private static readonly int[] _nRandomTable = new int[200]
	{
		36, 1, 232, 301, 209, 313, 21, 18, 54, 305,
		90, 141, 272, 270, 52, 207, 188, 328, 164, 85,
		221, 95, 104, 329, 225, 25, 131, 152, 80, 191,
		137, 217, 138, 275, 187, 299, 339, 205, 63, 304,
		236, 352, 239, 99, 27, 331, 16, 156, 57, 107,
		45, 158, 84, 220, 165, 116, 292, 291, 145, 163,
		268, 101, 319, 348, 6, 56, 222, 40, 140, 120,
		344, 315, 127, 173, 149, 279, 13, 150, 181, 262,
		247, 324, 233, 20, 254, 34, 336, 310, 212, 73,
		7, 77, 60, 249, 322, 333, 118, 126, 151, 109,
		105, 239, 204, 107, 266, 163, 209, 360, 65, 81,
		90, 262, 86, 321, 329, 269, 197, 290, 44, 303,
		175, 171, 272, 188, 185, 109, 103, 318, 196, 147,
		62, 349, 170, 253, 52, 190, 168, 11, 176, 165,
		33, 220, 343, 285, 108, 332, 21, 82, 293, 283,
		227, 63, 200, 334, 78, 280, 274, 211, 23, 133,
		257, 131, 193, 48, 313, 91, 7, 241, 34, 20,
		64, 240, 244, 137, 325, 157, 77, 357, 74, 268,
		10, 249, 230, 328, 25, 76, 316, 340, 5, 320,
		243, 245, 134, 37, 173, 68, 194, 248, 18, 214
	};

	protected int _nCount;

	protected int _nRandIndex;

	protected int _nOneShootNum = 1;

	protected float _nNextShootTime = 0.1f;

	protected Vector3 _vOriginalPoint = Vector3.zero;

	public override void Active(Vector3 pPos, Vector3 pDirection, LayerMask pTargetMask, IAimTarget pTarget = null)
	{
		if (!isSubBullet)
		{
			_nRandIndex = nRecordID % _nRandomTable.Length;
			_nCount = 0;
			_nOneShootNum = BulletData.n_NUM_SHOOT / 5;
			_vOriginalPoint = pPos;
			_nCount++;
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
			PlaySE("SkillSE_ERATO", "er_god");
			MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(Fx, _vOriginalPoint, Quaternion.identity, Array.Empty<object>());
		}
	}

	private IEnumerator CallSubBullet()
	{
		WaitForSeconds waitForNext = new WaitForSeconds(_nNextShootTime);
		for (int i = 0; i < 5; i++)
		{
			ShootSubBullet(_nOneShootNum);
			while (MonoBehaviourSingleton<OrangeGameManager>.Instance.IsGamePause)
			{
				yield return CoroutineDefine._waitForEndOfFrame;
			}
			yield return waitForNext;
		}
		ShootSubBullet(_nOneShootNum - 1);
	}

	protected void ShootSubBullet(int num)
	{
		for (int i = 0; i < num; i++)
		{
			if (_nCount >= BulletData.n_NUM_SHOOT)
			{
				break;
			}
			SubBullet();
		}
	}

	public override void BackToPool()
	{
		base.BackToPool();
		_nCount = 0;
		_nRandIndex = 0;
	}

	public override void SubBullet()
	{
		CH112_BarrageBullet poolObj = MonoBehaviourSingleton<PoolManager>.Instance.GetPoolObj<CH112_BarrageBullet>(BulletData.s_MODEL);
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
		int num = _nRandIndex + _nCount;
		if (num >= _nRandomTable.Length)
		{
			num = 0;
		}
		_nCount++;
		poolObj.transform.SetPositionAndRotation(_vOriginalPoint, Quaternion.identity);
		poolObj.Active(pDirection: Quaternion.Euler(0f, 0f, _nRandomTable[num]) * Direction, pPos: _vOriginalPoint, pTargetMask: TargetMask);
	}
}
