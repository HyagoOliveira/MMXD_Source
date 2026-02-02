using System.Collections;
using UnityEngine;

public class CH106_BeamBullet : BeamBullet, ILogicUpdate
{
	protected enum BEAM_STATUS
	{
		Shoot = 0,
		TurnUp = 1
	}

	[SerializeField]
	protected bool bAutoTurn;

	protected bool bInit;

	protected BEAM_STATUS nStatus;

	protected int nShootDirection = 1;

	protected CH106_Controller _pOwner;

	protected bool bGamePause;

	protected bool bStartTurn;

	protected float oldAngle;

	protected float defLength = 8f;

	[SerializeField]
	private LineRenderer fxLine01;

	[SerializeField]
	private LineRenderer fxLine01A;

	[SerializeField]
	private LineRenderer fxLine02;

	[SerializeField]
	private LineRenderer fxLine02A;

	[SerializeField]
	private ParticleSystem fxLightning;

	[SerializeField]
	private ParticleSystem fxSs1;

	[SerializeField]
	private ParticleSystem fxLL001;

	[SerializeField]
	private ParticleSystem fxLL002;

	[SerializeField]
	private ParticleSystem fxLL001A;

	[SerializeField]
	private ParticleSystem fxLL002A;

	[SerializeField]
	private LineRenderer fxLightning00;

	[SerializeField]
	private LineRenderer fxLightning00_Black;

	[SerializeField]
	private CH048_BeamSectorCollider secortCollider;

	public bool temp = true;

	protected float _fStartAngle;

	protected void OnApplicationPause(bool pause)
	{
		bGamePause = pause;
		if (bGamePause)
		{
			_clearTimer.TimerPause();
			_durationTimer.TimerPause();
		}
		else
		{
			_clearTimer.TimerResume();
			_durationTimer.TimerResume();
		}
	}

	protected override IEnumerator OnStartMove()
	{
		IsActivate = true;
		_hitCollider.enabled = true;
		MonoBehaviourSingleton<GameLogicUpdateManager>.Instance.AddUpdate(this);
		_clearTimer.TimerReset();
		_clearTimer.TimerStart();
		_durationTimer.TimerReset();
		_durationTimer.TimerStart();
		if (refPBMShoter.SOB != null)
		{
			_pOwner = refPBMShoter.SOB.GetComponent<CH106_Controller>();
		}
		while (!IsDestroy)
		{
			if (_clearTimer.GetMillisecond() >= _hurtCycle)
			{
				_clearTimer.TimerStart();
				_ignoreList.Clear();
				_rigidbody2D.WakeUp();
			}
			if (_duration != -1 && _durationTimer.GetMillisecond() >= _duration && !bGamePause)
			{
				IsDestroy = true;
				if (!isSubBullet)
				{
					bool flag = true;
					if (_pOwner != null)
					{
						flag = _pOwner.BeamStartTurn();
					}
					if (flag)
					{
						CH106_BeamBullet cH106_BeamBullet = CreateSubBeam();
						if ((bool)cH106_BeamBullet)
						{
							cH106_BeamBullet.DirectonTurn(1);
						}
					}
				}
			}
			if (AlwaysFaceCamera)
			{
				base.transform.LookAt(_mainCamera.transform.position, -Vector3.up);
			}
			yield return CoroutineDefine._waitForEndOfFrame;
		}
		BackToPool();
		yield return null;
	}

	protected override void Update_Effect()
	{
		bIsEnd = false;
		if (!bInit)
		{
			bInit = true;
			float num = ((BoxCollider2D)_hitCollider).size.x - defLength;
			fxEndpoint.localPosition += new Vector3(0f, 0f, num);
			fxLine01.SetPosition(0, new Vector3(fxLine01.GetPosition(0).x - num, 0f, fxLine01.GetPosition(0).z));
			fxLine01A.SetPosition(0, new Vector3(fxLine01A.GetPosition(0).x - num, 0f, fxLine01A.GetPosition(0).z));
			fxLine02.SetPosition(0, new Vector3(fxLine02.GetPosition(0).x - num, 0f, fxLine02.GetPosition(0).z));
			fxLine02A.SetPosition(0, new Vector3(fxLine02A.GetPosition(0).x - num, 0f, fxLine02A.GetPosition(0).z));
			fxLightning00.SetPosition(0, new Vector3(fxLightning00.GetPosition(0).x - num, 0f, fxLightning00.GetPosition(0).z));
			fxLightning00_Black.SetPosition(0, new Vector3(fxLightning00_Black.GetPosition(0).x - num, 0f, fxLightning00_Black.GetPosition(0).z));
			SetLightning(ref fxLightning, num);
			SetSS1(ref fxSs1, num);
			SetSS1(ref fxLL001, num);
			SetSS1(ref fxLL002, num);
			SetSS1(ref fxLL001A, num);
			SetSS1(ref fxLL002A, num);
		}
	}

	private void SetLightning(ref ParticleSystem ps, float difLength)
	{
		ParticleSystem.MainModule main = ps.main;
		ParticleSystem.MinMaxCurve startSizeY = main.startSizeY;
		startSizeY.constantMin = (main.startSizeY.constantMin + difLength) * 0.75f;
		startSizeY.constantMax = (main.startSizeY.constantMax + difLength) * 0.75f;
		main.startSizeY = startSizeY;
	}

	private void SetSS1(ref ParticleSystem ps, float difLength)
	{
		ParticleSystem.MainModule main = ps.main;
		ParticleSystem.MinMaxCurve startSizeX = main.startSizeX;
		startSizeX.constantMin = main.startSizeX.constantMin + difLength * 0.5f;
		startSizeX.constantMax = main.startSizeX.constantMax + difLength * 0.5f;
		main.startSizeX = startSizeX;
	}

	protected CH106_BeamBullet CreateSubBeam()
	{
		int n_LINK_SKILL = BulletData.n_LINK_SKILL;
		SKILL_TABLE tSKILL_TABLE = ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT[n_LINK_SKILL];
		if (refPBMShoter.SOB as OrangeCharacter != null)
		{
			(refPBMShoter.SOB as OrangeCharacter).tRefPassiveskill.ReCalcuSkill(ref tSKILL_TABLE);
		}
		CH106_BeamBullet cH106_BeamBullet = null;
		if (MonoBehaviourSingleton<PoolManager>.Instance.IsPreload("p_valstraxlaser_000_01"))
		{
			cH106_BeamBullet = MonoBehaviourSingleton<PoolManager>.Instance.GetPoolObj<CH106_BeamBullet>("p_valstraxlaser_000_01");
		}
		if (cH106_BeamBullet == null)
		{
			return null;
		}
		WeaponStatus weaponStatus = new WeaponStatus();
		weaponStatus.nHP = nHp;
		weaponStatus.nATK = nOriginalATK;
		weaponStatus.nCRI = nOriginalCRI;
		weaponStatus.nHIT = nHit - refPSShoter.GetAddStatus(8, nWeaponCheck);
		weaponStatus.nCriDmgPercent = nCriDmgPercent;
		weaponStatus.nReduceBlockPercent = nReduceBlockPercent;
		weaponStatus.nWeaponCheck = nWeaponCheck;
		weaponStatus.nWeaponType = nWeaponType;
		PerBuffManager.BuffStatus buffStatus = new PerBuffManager.BuffStatus();
		buffStatus.fAtkDmgPercent = fDmgFactor - 100f;
		buffStatus.fCriPercent = fCriFactor - 100f;
		buffStatus.fCriDmgPercent = fCriDmgFactor - 100f;
		buffStatus.fMissPercent = fMissFactor;
		buffStatus.refPBM = refPBMShoter;
		buffStatus.refPS = refPSShoter;
		cH106_BeamBullet.UpdateBulletData(tSKILL_TABLE, Owner);
		cH106_BeamBullet.SetBulletAtk(weaponStatus, buffStatus);
		cH106_BeamBullet.BulletLevel = BulletLevel;
		cH106_BeamBullet.isSubBullet = true;
		cH106_BeamBullet.transform.SetPositionAndRotation(_transform.position, Quaternion.identity);
		cH106_BeamBullet.Active(_transform.position, Direction, TargetMask);
		return cH106_BeamBullet;
	}

	public void DirectonTurn(int direction)
	{
		if (nStatus != 0 || direction != 1)
		{
			return;
		}
		nStatus = BEAM_STATUS.TurnUp;
		ActiveExtraCollider();
		if (!bAutoTurn)
		{
			return;
		}
		_fStartAngle = base.transform.localEulerAngles.z;
		if (base.transform.localEulerAngles.z == 270f)
		{
			if (refPBMShoter.SOB != null && refPBMShoter.SOB.direction == 1)
			{
				_fStartAngle = base.transform.localEulerAngles.z - 360f;
			}
		}
		else if (base.transform.localEulerAngles.z > 270f)
		{
			_fStartAngle = base.transform.localEulerAngles.z - 360f;
		}
		if (base.transform.localEulerAngles.z < 90f || base.transform.localEulerAngles.z > 270f)
		{
			nShootDirection = 1;
		}
		else
		{
			nShootDirection = -1;
		}
		bStartTurn = true;
		oldAngle = base.transform.localEulerAngles.z;
		LeanTween.value(base.transform.gameObject, _fStartAngle, 90f, (float)(_duration - 50) * 0.001f).setOnUpdate(delegate(float val)
		{
			base.transform.localEulerAngles = new Vector3(0f, 0f, val);
		}).setEaseInQuart();
	}

	private void ActiveExtraCollider()
	{
		if ((bool)secortCollider)
		{
			BoxCollider2D boxCollider2D = _hitCollider as BoxCollider2D;
			if ((bool)boxCollider2D)
			{
				secortCollider.Active(this, boxCollider2D.size.x);
			}
		}
	}

	public void LogicUpdate()
	{
		if (bStartTurn)
		{
			float z = base.transform.localEulerAngles.z;
			if (nShootDirection == 1)
			{
				secortCollider.UpdateAngle(oldAngle, z);
			}
			else
			{
				secortCollider.UpdateAngle(z, oldAngle);
			}
			oldAngle = z;
		}
	}

	public override void BackToPool()
	{
		if (!isSubBullet && _pOwner != null)
		{
			_pOwner.BeamStartTurn();
		}
		nStatus = BEAM_STATUS.Shoot;
		_pOwner = null;
		bStartTurn = false;
		MonoBehaviourSingleton<GameLogicUpdateManager>.Instance.RemoveUpdate(this);
		if ((bool)secortCollider)
		{
			secortCollider.Disable();
		}
		base.BackToPool();
	}
}
