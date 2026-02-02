using UnityEngine;

public class CH062_BeamExplosion : BasicBullet
{
	private SKILL_TABLE _skillAttrData;

	private OrangeTimer _timer;

	private OrangeCharacter _character;

	private int _triggerCount;

	protected override void Awake()
	{
		base.Awake();
		_timer = OrangeTimerManager.GetTimer();
	}

	public override void OnStart()
	{
		base.OnStart();
		if (_skillAttrData == null && BulletData != null && BulletData.n_LINK_SKILL > 0)
		{
			_skillAttrData = ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT[BulletData.n_LINK_SKILL];
			_character = refPBMShoter.SOB as OrangeCharacter;
			_character.tRefPassiveskill.ReCalcuSkill(ref _skillAttrData);
		}
	}

	public override void Active(Vector3 pPos, Vector3 pDirection, LayerMask pTargetMask, IAimTarget pTarget = null)
	{
		base.Active(pPos, pDirection, pTargetMask, pTarget);
		_timer.TimerReset();
		_timer.TimerStart();
		_triggerCount = 0;
	}

	public override void Active(Transform pTransform, Vector3 pDirection, LayerMask pTargetMask, IAimTarget pTarget = null)
	{
		base.Active(pTransform, pDirection, pTargetMask, pTarget);
		_timer.TimerReset();
		_timer.TimerStart();
		_triggerCount = 0;
	}

	protected override void PhaseNormal()
	{
		base.PhaseNormal();
		if (_skillAttrData != null && _timer.GetMillisecond() >= _skillAttrData.n_FIRE_SPEED)
		{
			if (_triggerCount < _skillAttrData.n_MAGAZINE)
			{
				_timer.TimerStart();
				_triggerCount++;
				CreateExplosion();
			}
			else
			{
				_timer.TimerStop();
			}
		}
	}

	public override void BackToPool()
	{
		base.BackToPool();
		_timer.TimerStop();
		_timer.TimerReset();
		_triggerCount = 0;
	}

	private void CreateExplosion()
	{
		_character.tRefPassiveskill.ReCalcuSkill(ref _skillAttrData);
		CollideBullet poolObj = MonoBehaviourSingleton<PoolManager>.Instance.GetPoolObj<CollideBullet>(_skillAttrData.s_MODEL);
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
		poolObj.UpdateBulletData(_skillAttrData, Owner);
		poolObj.BulletLevel = BulletLevel;
		poolObj.isSubBullet = true;
		poolObj.SetBulletAtk(weaponStatus, tBuffStatus);
		poolObj.Active(base.transform.position, Direction, TargetMask);
	}
}
