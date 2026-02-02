using System.Collections;
using UnityEngine;

public class CH048_BeamBullet : BeamBullet, IManagedLateUpdateBehavior, ILogicUpdate
{
	protected enum BEAM_STATUS
	{
		Shoot = 0,
		TurnUp = 1,
		TurnDown = 2
	}

	protected bool bInit;

	protected BEAM_STATUS nStatus;

	protected float fTurnStart;

	protected float fTurnValue;

	protected float fMaxTurnValue;

	protected int nShootDirection = 1;

	protected float fOldTurnAngle;

	protected float defLength = 4.52f;

	[SerializeField]
	private LineRenderer fxLine01;

	[SerializeField]
	private LineRenderer fxLine011;

	[SerializeField]
	private LineRenderer fxLine022;

	[SerializeField]
	private ParticleSystem fxLine02;

	[SerializeField]
	private ParticleSystem fxLightning_000;

	[SerializeField]
	private ParticleSystem fxLightning_001;

	[SerializeField]
	private ParticleSystem fxLightning_002;

	[SerializeField]
	private ParticleSystem fxLightning_003;

	[SerializeField]
	private CH048_BeamSectorCollider secortCollider;

	[SerializeField]
	private string subBeamName = "p_eyebeam_001";

	[SerializeField]
	protected float turnSpeed = 0.02f;

	protected override IEnumerator OnStartMove()
	{
		IsActivate = true;
		_hitCollider.enabled = true;
		_clearTimer.TimerReset();
		_clearTimer.TimerStart();
		_durationTimer.TimerReset();
		_durationTimer.TimerStart();
		if (MonoBehaviourSingleton<OrangeGameManager>.Instance.IsGamePause)
		{
			_clearTimer.TimerPause();
			_durationTimer.TimerPause();
		}
		while (!IsDestroy)
		{
			if (_clearTimer.GetMillisecond() >= _hurtCycle)
			{
				_clearTimer.TimerStart();
				_ignoreList.Clear();
				_rigidbody2D.WakeUp();
			}
			if (_duration != -1 && _durationTimer.GetMillisecond() >= _duration)
			{
				IsDestroy = true;
				if (!isSubBullet)
				{
					CreateSubBeam().DirectonTurn(1);
					CreateSubBeam().DirectonTurn(2);
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

	protected CH048_BeamBullet CreateSubBeam()
	{
		int n_LINK_SKILL = BulletData.n_LINK_SKILL;
		SKILL_TABLE tSKILL_TABLE = ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT[n_LINK_SKILL];
		if (refPBMShoter.SOB as OrangeCharacter != null)
		{
			(refPBMShoter.SOB as OrangeCharacter).tRefPassiveskill.ReCalcuSkill(ref tSKILL_TABLE);
		}
		CH048_BeamBullet cH048_BeamBullet = null;
		if (MonoBehaviourSingleton<PoolManager>.Instance.IsPreload(subBeamName))
		{
			cH048_BeamBullet = MonoBehaviourSingleton<PoolManager>.Instance.GetPoolObj<CH048_BeamBullet>(subBeamName);
		}
		if (cH048_BeamBullet == null)
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
		cH048_BeamBullet.UpdateBulletData(tSKILL_TABLE, Owner);
		cH048_BeamBullet.SetBulletAtk(weaponStatus, buffStatus);
		cH048_BeamBullet.BulletLevel = BulletLevel;
		cH048_BeamBullet.isSubBullet = true;
		cH048_BeamBullet.transform.SetPositionAndRotation(_transform.position, Quaternion.identity);
		cH048_BeamBullet.Active(_transform.position, Direction, TargetMask);
		return cH048_BeamBullet;
	}

	protected override void Update_Effect()
	{
		MonoBehaviourSingleton<UpdateManager>.Instance.AddLateUpdate(this);
		MonoBehaviourSingleton<GameLogicUpdateManager>.Instance.AddUpdate(this);
		bIsEnd = false;
		if (!bInit)
		{
			bInit = true;
			float num = ((BoxCollider2D)_hitCollider).size.x - defLength;
			fxEndpoint.localPosition += new Vector3(0f, 0f, num);
			if ((bool)fxLine01)
			{
				fxLine01.SetPosition(0, new Vector3(fxLine01.GetPosition(0).x - num, 0f, 0f));
			}
			if ((bool)fxLine011)
			{
				fxLine011.SetPosition(0, new Vector3(fxLine011.GetPosition(0).x - num, 0f, 0f));
			}
			if ((bool)fxLine022)
			{
				fxLine022.SetPosition(0, new Vector3(fxLine022.GetPosition(0).x - num, 0f, 0f));
			}
			if ((bool)fxLine02)
			{
				SetLine02(ref fxLine02, num);
			}
			if ((bool)fxLightning_000)
			{
				fxLightning_000.transform.localPosition = fxLightning_000.transform.localPosition - new Vector3(num, 0f, 0f);
			}
			if ((bool)fxLightning_002)
			{
				SetLightning(ref fxLightning_002, num);
			}
			if ((bool)fxLightning_003)
			{
				SetLightning(ref fxLightning_003, num);
			}
		}
	}

	private void SetLine02(ref ParticleSystem ps, float difLength)
	{
		ParticleSystem.MainModule main = ps.main;
		ParticleSystem.MinMaxCurve startSizeY = main.startSizeY;
		ps.transform.localPosition = ps.transform.localPosition - new Vector3(difLength, 0f, 0f);
		startSizeY.constantMin += difLength;
		main.startSizeY = startSizeY;
	}

	private void SetLightning(ref ParticleSystem ps, float difLength)
	{
		ParticleSystem.MainModule main = ps.main;
		ParticleSystem.MinMaxCurve startSizeX = main.startSizeX;
		startSizeX.constantMin = main.startSizeX.constantMin + difLength;
		startSizeX.constantMax = main.startSizeX.constantMax + difLength;
		main.startSizeX = startSizeX;
	}

	public virtual void LateUpdateFunc()
	{
		if (nStatus == BEAM_STATUS.TurnUp)
		{
			Vector3 localEulerAngle = base.transform.localEulerAngles;
			fTurnValue = Mathf.Min(fTurnValue + Time.deltaTime * turnSpeed, fMaxTurnValue);
			float z = fTurnStart + fTurnValue * (float)nShootDirection;
			base.transform.localEulerAngles = new Vector3(0f, 0f, z);
		}
		else if (nStatus == BEAM_STATUS.TurnDown)
		{
			Vector3 localEulerAngle2 = base.transform.localEulerAngles;
			fTurnValue = Mathf.Min(fTurnValue + Time.deltaTime * turnSpeed, fMaxTurnValue);
			float z2 = fTurnStart + fTurnValue * (float)(-nShootDirection);
			base.transform.localEulerAngles = new Vector3(0f, 0f, z2);
		}
	}

	public void LogicUpdate()
	{
		if (nStatus == BEAM_STATUS.TurnUp)
		{
			float num = fOldTurnAngle;
			float num2 = (fOldTurnAngle = base.transform.localEulerAngles.z);
			if ((bool)secortCollider && secortCollider.enabled)
			{
				if (nShootDirection == 1)
				{
					secortCollider.UpdateAngle(num, num2);
				}
				else
				{
					secortCollider.UpdateAngle(num2, num);
				}
			}
		}
		else
		{
			if (nStatus != BEAM_STATUS.TurnDown)
			{
				return;
			}
			float num3 = fOldTurnAngle;
			float num4 = (fOldTurnAngle = base.transform.localEulerAngles.z);
			if ((bool)secortCollider && secortCollider.enabled)
			{
				if (nShootDirection == 1)
				{
					secortCollider.UpdateAngle(num4, num3);
				}
				else
				{
					secortCollider.UpdateAngle(num3, num4);
				}
			}
		}
	}

	public void DirectonTurn(int direction)
	{
		if (nStatus == BEAM_STATUS.Shoot)
		{
			switch (direction)
			{
			case 1:
				nStatus = BEAM_STATUS.TurnUp;
				ActiveExtraCollider();
				break;
			case 2:
				nStatus = BEAM_STATUS.TurnDown;
				ActiveExtraCollider();
				break;
			}
			fTurnStart = base.transform.localEulerAngles.z;
			fTurnValue = 0f;
			fMaxTurnValue = Mathf.Clamp(turnSpeed * (float)_duration * 0.001f, 0f, 180f);
			fOldTurnAngle = fTurnStart;
			if (base.transform.localEulerAngles.z < 90f || base.transform.localEulerAngles.z > 270f)
			{
				nShootDirection = 1;
			}
			else
			{
				nShootDirection = -1;
			}
		}
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

	public override void BackToPool()
	{
		MonoBehaviourSingleton<UpdateManager>.Instance.RemoveLateUpdate(this);
		MonoBehaviourSingleton<GameLogicUpdateManager>.Instance.RemoveUpdate(this);
		nStatus = BEAM_STATUS.Shoot;
		fTurnValue = 0f;
		if ((bool)secortCollider)
		{
			secortCollider.Disable();
		}
		base.BackToPool();
	}
}
