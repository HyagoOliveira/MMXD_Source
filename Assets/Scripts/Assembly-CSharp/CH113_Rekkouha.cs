using System;
using System.Collections;
using UnityEngine;

public class CH113_Rekkouha : BasicBullet
{
	private const int COLOR_SHINY_COUNT = 3;

	[SerializeField]
	private Color[] colorShinys = new Color[3];

	[SerializeField]
	private ParticleSystem[] psNeedShiny = new ParticleSystem[0];

	[SerializeField]
	private float _nOffsetRateX = 1.5f;

	[SerializeField]
	private float _nOffsetRateY = 2f;

	[SerializeField]
	private float _nPlusOffsetY = 7f;

	[SerializeField]
	private Vector3 _vShootDir = Vector3.down;

	private Coroutine tShotCoroutine;

	protected float[] _nOffsetTable = new float[31]
	{
		0f, 1f, -1f, 2f, -2f, 3f, -3f, 4f, -4f, 5f,
		-5f, 6f, -6f, 7f, -7f, 8f, -8f, 9f, -9f, 10f,
		-10f, 11f, -11f, 12f, -12f, 13f, -13f, 14f, -14f, 15f,
		-15f
	};

	protected int _nCount;

	protected int _nIndex;

	protected int _nOneShootNum = 1;

	protected float _nNextShootTime = 0.1f;

	protected Vector3 _vOriginalPoint = Vector3.zero;

	public override void Active(Vector3 pPos, Vector3 pDirection, LayerMask pTargetMask, IAimTarget pTarget = null)
	{
		if (!isSubBullet)
		{
			_nIndex = 0;
			_nCount = 0;
			_nOneShootNum = BulletData.n_NUM_SHOOT;
			_vOriginalPoint = pPos;
			CaluStartPosition(_nIndex, _nCount, _vOriginalPoint, out pPos, out pDirection);
			CaluParticleColor(_nCount);
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
			tShotCoroutine = StartCoroutine(CallSubBullet());
		}
	}

	private IEnumerator CallSubBullet()
	{
		WaitForSeconds waitNextShootTime = new WaitForSeconds(_nNextShootTime);
		ShootSubBullet(_nOneShootNum - 1);
		while (MonoBehaviourSingleton<OrangeGameManager>.Instance.IsGamePause)
		{
			yield return CoroutineDefine._waitForEndOfFrame;
		}
		yield return waitNextShootTime;
		for (int i = 0; i < 5; i++)
		{
			ShootSubBullet(_nOneShootNum);
			while (MonoBehaviourSingleton<OrangeGameManager>.Instance.IsGamePause)
			{
				yield return CoroutineDefine._waitForEndOfFrame;
			}
			yield return waitNextShootTime;
		}
		tShotCoroutine = null;
	}

	protected void ShootSubBullet(int num)
	{
		int n_NUM_SHOOT = BulletData.n_NUM_SHOOT;
		for (int i = 0; i < num; i++)
		{
			if (_nCount >= n_NUM_SHOOT)
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
		_nIndex = 0;
		if (tShotCoroutine != null)
		{
			StopCoroutine(tShotCoroutine);
			tShotCoroutine = null;
		}
	}

	public void CaluStartPosition(int nIdx, int count, Vector3 originalPos, out Vector3 pos, out Vector3 dir)
	{
		float x = originalPos.x + _nOffsetTable[nIdx] * _nOffsetRateX;
		float y = originalPos.y + Mathf.Abs(_nOffsetTable[nIdx]) * _nOffsetRateY + _nPlusOffsetY;
		dir = _vShootDir;
		pos = new Vector3(x, y, 0f);
	}

	private void CaluParticleColor(int count)
	{
		ParticleSystem[] array = psNeedShiny;
		foreach (ParticleSystem obj in array)
		{
			int num = count / 2 % 3;
			ParticleSystem.ColorOverLifetimeModule colorOverLifetime = obj.colorOverLifetime;
			colorOverLifetime.enabled = true;
			Gradient gradient = new Gradient();
			gradient.SetKeys(new GradientColorKey[3]
			{
				new GradientColorKey(colorShinys[GetLoopIdx(num, 3)], 0.333f),
				new GradientColorKey(colorShinys[GetLoopIdx(num + 1, 3)], 0.667f),
				new GradientColorKey(colorShinys[GetLoopIdx(num + 2, 3)], 1f)
			}, new GradientAlphaKey[2]
			{
				new GradientAlphaKey(1f, 0f),
				new GradientAlphaKey(1f, 1f)
			});
			colorOverLifetime.color = gradient;
		}
	}

	private int GetLoopIdx(int nowVal, int maxVal)
	{
		if (nowVal >= maxVal)
		{
			nowVal = 0;
		}
		return nowVal;
	}

	public override void SubBullet()
	{
		CH113_Rekkouha poolObj = MonoBehaviourSingleton<PoolManager>.Instance.GetPoolObj<CH113_Rekkouha>(BulletData.s_MODEL);
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
		int num = _nIndex + _nCount;
		int nCount = _nCount;
		if (num >= _nOffsetTable.Length)
		{
			num -= _nOffsetTable.Length;
		}
		_nCount++;
		poolObj.transform.SetPositionAndRotation(_vOriginalPoint, Quaternion.identity);
		poolObj.CaluStartPosition(num, nCount, _vOriginalPoint, out pos, out Direction);
		poolObj.CaluParticleColor(nCount + 1);
		poolObj.Active(pos, Direction, TargetMask);
	}

	protected override void GenerateImpactFx(bool bPlaySE = true)
	{
		if (Phase == BulletPhase.Normal)
		{
			if ((bool)lastHit && !ManagedSingleton<OrangeTableHelper>.Instance.IsNullOrEmpty(FxImpact))
			{
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FxImpact, lastHit.position, Quaternion.identity, Array.Empty<object>());
			}
			if (bPlaySE)
			{
				TryPlaySE();
			}
		}
	}
}
