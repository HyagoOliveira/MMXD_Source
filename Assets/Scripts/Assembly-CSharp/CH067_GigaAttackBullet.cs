using System.Collections;
using UnityEngine;

public class CH067_GigaAttackBullet : BasicBullet
{
	protected int[] _nRandomTable = new int[100]
	{
		2864, 4221, 66, 3190, 7546, 1679, 183, 3410, 2458, 5788,
		8382, 942, 7323, 8768, 2615, 1636, 7772, 5295, 2470, 818,
		6291, 4129, 3876, 6979, 2666, 801, 6131, 2773, 9777, 5803,
		627, 6482, 6649, 6580, 1508, 7293, 4063, 2841, 7044, 992,
		6274, 4893, 7491, 5807, 3997, 5713, 9343, 5292, 4218, 210,
		5183, 8151, 9504, 4161, 9642, 6748, 8138, 8365, 9414, 2993,
		5493, 335, 9921, 4136, 6572, 584, 4088, 2531, 5829, 1139,
		3290, 706, 7913, 1687, 9222, 636, 2769, 5604, 6251, 8443,
		3224, 5856, 6869, 6502, 9816, 4875, 907, 523, 6493, 9620,
		3841, 8777, 6300, 9761, 4675, 195, 163, 7355, 1617, 6247
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
			CaluStartPosition(_nRandIndex, _nCount, _vOriginalPoint, out pPos, out pDirection);
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
		}
	}

	private IEnumerator CallSubBullet()
	{
		ShootSubBullet(_nOneShootNum - 1);
		while (MonoBehaviourSingleton<OrangeGameManager>.Instance.IsGamePause)
		{
			yield return CoroutineDefine._waitForEndOfFrame;
		}
		yield return new WaitForSeconds(_nNextShootTime);
		for (int i = 0; i < 5; i++)
		{
			ShootSubBullet(_nOneShootNum);
			while (MonoBehaviourSingleton<OrangeGameManager>.Instance.IsGamePause)
			{
				yield return CoroutineDefine._waitForEndOfFrame;
			}
			yield return new WaitForSeconds(_nNextShootTime);
		}
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

	public void CaluStartPosition(int randIndex, int count, Vector3 originalPos, out Vector3 pos, out Vector3 dir)
	{
		string[] array = BulletData.s_FIELD.Split(',');
		Vector2 vector = new Vector2(float.Parse(array[3]), float.Parse(array[4]));
		float num = vector.x * 2f * (float)_nRandomTable[randIndex] / 10000f;
		float x = originalPos.x - vector.x + num;
		float y;
		if ((count & 1) == 0)
		{
			dir = Vector3.down;
			y = originalPos.y + vector.y;
		}
		else
		{
			dir = Vector3.up;
			y = originalPos.y - vector.y;
		}
		pos = new Vector3(x, y, originalPos.z);
	}

	public override void SubBullet()
	{
		CH067_GigaAttackBullet poolObj = MonoBehaviourSingleton<PoolManager>.Instance.GetPoolObj<CH067_GigaAttackBullet>(BulletData.s_MODEL);
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
		Vector3 pos = _vOriginalPoint;
		int num = _nRandIndex + _nCount;
		int nCount = _nCount;
		if (num >= _nRandomTable.Length)
		{
			num -= _nRandomTable.Length;
		}
		_nCount++;
		poolObj.transform.SetPositionAndRotation(_vOriginalPoint, Quaternion.identity);
		poolObj.CaluStartPosition(num, nCount, _vOriginalPoint, out pos, out Direction);
		poolObj.Active(pos, Direction, TargetMask);
	}
}
