using CodeStage.AntiCheat.ObscuredTypes;
using StageLib;
using UnityEngine;

public class RideBaseObj : StageObjBase
{
	public int nRideID;

	public OrangeCharacter MasterPilot;

	public ObjInfoBar tObjInfoBar;

	public Controller2D Controller;

	public Transform SeatTransform;

	protected LockRangeObj mLockRangeObj;

	protected LockRangeObj tMasterPilotLockRangeObj;

	protected bool bCanRide = true;

	private OrangeCriSource _ss;

	public OrangeCriSource SoundSource
	{
		get
		{
			if (_ss == null)
			{
				_ss = base.gameObject.GetComponent<OrangeCriSource>();
				if (_ss == null)
				{
					_ss = base.gameObject.AddComponent<OrangeCriSource>();
					_ss.Initial(OrangeSSType.HIT);
				}
			}
			return _ss;
		}
	}

	public virtual void LogicUpdateCall()
	{
	}

	public virtual void LogicUpdatePrepare()
	{
	}

	public virtual void Update_AutoAim(PlayerAutoAimSystem mPAAS)
	{
	}

	public virtual void UnRide(bool bDisable)
	{
	}

	public virtual void StopRideObj()
	{
	}

	public override void SetStun(bool enable, bool bCheckOtherObj = true)
	{
	}

	public override void SetNoMove(bool enable, bool bCheckOtherObj = true)
	{
	}

	public override ObscuredInt GetCurrentWeaponCheck()
	{
		if (MasterPilot != null)
		{
			return MasterPilot.GetCurrentWeaponCheck();
		}
		return base.GetCurrentWeaponCheck();
	}

	public override int GetSOBType()
	{
		return 3;
	}
}
