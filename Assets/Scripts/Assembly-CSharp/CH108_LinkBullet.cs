using UnityEngine;

public class CH108_LinkBullet : BasicBullet
{
	[SerializeField]
	private int PlayerSkillsIdx = 1;

	protected SKILL_TABLE _tLinkSkill;

	protected OrangeTimer _otMineTimer;

	protected int _nMineCount;

	public override Vector3 GetCreateBulletPosition
	{
		get
		{
			return base.transform.position;
		}
	}

	public override Vector3 GetCreateBulletShotDir
	{
		get
		{
			Vector3 result = base.GetCreateBulletShotDir;
			OrangeCharacter orangeCharacter = refPBMShoter.SOB as OrangeCharacter;
			if (orangeCharacter == null)
			{
				return result;
			}
			if (orangeCharacter.PlayerAutoAimSystem.AutoAimTarget != null && Vector2.Distance(orangeCharacter.PlayerAutoAimSystem.AutoAimTarget.AimPosition, base.transform.position) < _tLinkSkill.f_DISTANCE)
			{
				result = (orangeCharacter.PlayerAutoAimSystem.AutoAimTarget.AimPosition - base.transform.position).normalized;
			}
			return result;
		}
	}

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
		if (!(orangeCharacter == null) && orangeCharacter.IsLocalPlayer && _tLinkSkill != null && _otMineTimer.GetMillisecond() >= _tLinkSkill.n_FIRE_SPEED)
		{
			if (_tLinkSkill.n_EFFECT == 1 && _nMineCount < _tLinkSkill.n_MAGAZINE)
			{
				_otMineTimer.TimerStart();
				_nMineCount++;
				CreateBulletDetail(_tLinkSkill);
			}
			else
			{
				_otMineTimer.TimerStop();
			}
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
