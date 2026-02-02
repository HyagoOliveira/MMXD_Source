using UnityEngine;

public class CH043_1stChargeBuster : LogicBasicBullet
{
	[SerializeField]
	private float _slowTime = 1f;

	private bool _startMove = true;

	private float _currentSpeed = 1f;

	[SerializeField]
	private float _acceleration = 1f;

	[SerializeField]
	private float _startSpeed = 4f;

	[SerializeField]
	protected ParticleSystem _particleA;

	[SerializeField]
	protected ParticleSystem _particleB;

	[SerializeField]
	private float _pingpongTime = 0.2f;

	protected override void DoActive(IAimTarget pTarget)
	{
		base.DoActive(pTarget);
		_startMove = true;
		_currentSpeed = _startSpeed;
		if ((bool)_particleA)
		{
			LeanTween.value(_particleA.gameObject, 0.5f, -0.5f, _pingpongTime).setOnUpdate(delegate(float val)
			{
				_particleA.transform.localPosition = new Vector3(_particleA.transform.localPosition.x, val, 0f);
			}).setEaseInOutCubic()
				.setLoopPingPong();
		}
		if ((bool)_particleB)
		{
			LeanTween.value(_particleB.gameObject, -0.5f, 0.5f, _pingpongTime).setOnUpdate(delegate(float val)
			{
				_particleB.transform.localPosition = new Vector3(_particleB.transform.localPosition.x, val, 0f);
			}).setEaseInOutCubic()
				.setLoopPingPong();
		}
	}

	public override void BackToPool()
	{
		base.BackToPool();
		if ((bool)_particleA)
		{
			LeanTween.cancel(_particleA.gameObject);
		}
		if ((bool)_particleB)
		{
			LeanTween.cancel(_particleB.gameObject);
		}
	}

	public override void LogicUpdate()
	{
		base.LogicUpdate();
		if (GetBulletFlagByPerGameSaveData(2))
		{
			ChangeCrossShot();
		}
	}

	protected override void MoveBullet()
	{
		if (_startMove)
		{
			MoveSlow();
		}
		else
		{
			MoveTypeLine();
		}
	}

	protected void MoveSlow()
	{
		if ((float)ActivateTimer.GetMillisecond() > _slowTime * 1000f)
		{
			_startMove = false;
			_lastPosition = _transform.localPosition;
			CaluLogicFrame(BulletData.n_SPEED, BulletData.f_DISTANCE - _moveDistance, Direction);
			return;
		}
		float num = Time.deltaTime / GameLogicUpdateManager.m_fFrameLen;
		_currentSpeed = Mathf.Clamp(_currentSpeed + _acceleration * num, 0.1f, 10f);
		CaluLogicFrame(_currentSpeed, BulletData.f_DISTANCE - _moveDistance, Direction);
		_nowPos += new VInt3(_speed * _timeDelta.scalar);
		float num2 = Vector3.Distance(base.transform.localPosition, _nowPos.vec3);
		_distanceDelta = num2 * num;
		_moveDistance += num2;
	}

	public void ChangeCrossShot()
	{
		int n_LINK_SKILL = BulletData.n_LINK_SKILL;
		SKILL_TABLE tSKILL_TABLE = ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT[n_LINK_SKILL];
		if (refPBMShoter.SOB as OrangeCharacter != null)
		{
			(refPBMShoter.SOB as OrangeCharacter).tRefPassiveskill.ReCalcuSkill(ref tSKILL_TABLE);
		}
		BulletBase bulletBase = CreateSubBullet<BulletBase>(tSKILL_TABLE);
		SKILL_TABLE tSKILL_TABLE2 = ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT[bulletBase.GetBulletData.n_LINK_SKILL];
		if (refPBMShoter.SOB as OrangeCharacter != null)
		{
			(refPBMShoter.SOB as OrangeCharacter).tRefPassiveskill.ReCalcuSkill(ref tSKILL_TABLE2);
		}
		float f_ANGLE = tSKILL_TABLE2.f_ANGLE;
		float[] array = new float[4]
		{
			f_ANGLE,
			0f - f_ANGLE,
			f_ANGLE * 2f,
			(0f - f_ANGLE) * 2f
		};
		for (int i = 0; i < 4; i++)
		{
			CreateSubBullet<CH043_subBuster>(tSKILL_TABLE2).SetParam(array[i]);
		}
		Stop();
		BackToPool();
	}
}
