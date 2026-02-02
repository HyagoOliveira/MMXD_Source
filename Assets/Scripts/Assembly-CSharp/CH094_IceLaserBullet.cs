using UnityEngine;

public class CH094_IceLaserBullet : BasicBullet
{
	protected SKILL_TABLE _tLinkSkill;

	protected OrangeTimer _otMineTimer;

	protected int _nMineCount;

	protected override void Awake()
	{
		base.Awake();
		_otMineTimer = OrangeTimerManager.GetTimer();
	}

	public override void OnStart()
	{
		base.OnStart();
		if (BulletData != null && BulletData.n_LINK_SKILL > 0)
		{
			_tLinkSkill = ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT[BulletData.n_LINK_SKILL];
			OrangeCharacter orangeCharacter = refPBMShoter.SOB as OrangeCharacter;
			if ((bool)orangeCharacter)
			{
				orangeCharacter.tRefPassiveskill.ReCalcuSkill(ref _tLinkSkill);
			}
		}
	}

	public override void Active(Vector3 pPos, Vector3 pDirection, LayerMask pTargetMask, IAimTarget pTarget = null)
	{
		base.Active(pPos, pDirection, pTargetMask, pTarget);
		_otMineTimer.TimerStart();
		_nMineCount = 0;
	}

	public override void Active(Transform pTransform, Vector3 pDirection, LayerMask pTargetMask, IAimTarget pTarget = null)
	{
		base.Active(pTransform, pDirection, pTargetMask, pTarget);
		_otMineTimer.TimerStart();
		_nMineCount = 0;
	}

	protected override void PhaseNormal()
	{
		base.PhaseNormal();
		OrangeCharacter orangeCharacter = refPBMShoter.SOB as OrangeCharacter;
		if (orangeCharacter == null || !orangeCharacter.IsLocalPlayer || _tLinkSkill == null || _otMineTimer.GetMillisecond() < _tLinkSkill.n_FIRE_SPEED)
		{
			return;
		}
		_otMineTimer.GetMillisecond();
		if (_tLinkSkill.n_EFFECT == 16 && _nMineCount < _tLinkSkill.n_MAGAZINE)
		{
			_otMineTimer.TimerStart();
			_nMineCount++;
			_otMineTimer.GetMillisecond();
			CharacterControlBase component = orangeCharacter.GetComponent<CharacterControlBase>();
			if ((bool)component)
			{
				component.CallPet((int)_tLinkSkill.f_EFFECT_X, false, -1, _transform.position);
			}
		}
		else
		{
			_otMineTimer.TimerStop();
		}
	}

	public override void BackToPool()
	{
		base.BackToPool();
		_tLinkSkill = null;
		_otMineTimer.TimerStop();
		_nMineCount = 0;
	}
}
