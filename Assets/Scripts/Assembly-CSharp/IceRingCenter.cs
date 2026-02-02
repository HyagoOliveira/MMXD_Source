using System;
using System.Collections.Generic;
using UnityEngine;

public class IceRingCenter : LogicBasicBullet
{
	protected enum Phase
	{
		POPUP = 0,
		MOVE = 1
	}

	protected Phase _phase;

	[SerializeField]
	protected List<IceRingBullet> _ringBullets = new List<IceRingBullet>();

	[SerializeField]
	protected float _radiusMin = 0.3f;

	[SerializeField]
	protected float _radianBig = 3f;

	[SerializeField]
	protected float _radianSmall = 1f;

	[SerializeField]
	private int popupTime = 30;

	[SerializeField]
	private float rollingSpeed = 480f;

	[SerializeField]
	protected float _tRadiusMinX = 0.3f;

	[SerializeField]
	protected float _tRadius = 1f;

	[SerializeField]
	protected float _tRotateParam;

	protected int _direction = 1;

	protected const float _radian = (float)Math.PI / 180f;

	protected override void DoActive(IAimTarget pTarget)
	{
		MonoBehaviourSingleton<GameLogicUpdateManager>.Instance.AddUpdate(this);
		MonoBehaviourSingleton<UpdateManager>.Instance.AddLateUpdate(this);
		if (activeTracking)
		{
			Target = pTarget;
			NeutralAIS.UpdateAimRange(TrackingData.f_RANGE);
		}
		_ringBullets.Clear();
		_direction = ((!(Direction.x >= 0f)) ? 1 : (-1));
		_transform.localRotation = Quaternion.identity;
		ActivateTimer.TimerStart();
		_lastPosition = _transform.localPosition;
		CaluLogicFrame(BulletData.n_SPEED, BulletData.f_DISTANCE, Direction);
		PlayUseFx();
	}

	protected override void MoveBullet()
	{
		if (_phase == Phase.POPUP)
		{
			if (isSubBullet)
			{
				return;
			}
			long ticks = ActivateTimer.GetTicks(1);
			int count = _ringBullets.Count;
			int num = (int)(ticks * BulletData.n_NUM_SHOOT / 21);
			if (num > BulletData.n_NUM_SHOOT)
			{
				num = BulletData.n_NUM_SHOOT;
			}
			if (count < BulletData.n_NUM_SHOOT)
			{
				for (int i = count; i < num; i++)
				{
					CreateRingBullet();
				}
			}
			if (ticks < popupTime)
			{
				return;
			}
			for (int j = 0; j < _ringBullets.Count; j++)
			{
				if (_ringBullets[j] != null)
				{
					_ringBullets[j].nRecordID = nRecordID;
					_ringBullets[j].nNetID = nNetID * 100 + j;
					_ringBullets[j].EnableColider(true);
				}
			}
			CaluLogicFrame(BulletData.n_SPEED, BulletData.f_DISTANCE, Direction);
			_phase = Phase.MOVE;
		}
		else
		{
			long num2 = ActivateTimer.GetTicks(1) - popupTime;
			_tRadiusMinX = Mathf.Min(1f, _radiusMin + (float)num2 / 30f);
			if (BulletData.n_SHOTLINE == 9)
			{
				_tRadius = Mathf.Min(_radianSmall, 1f + (float)num2 / 30f);
			}
			else
			{
				_tRadius = Mathf.Min(_radianBig, 1f + (float)num2 / 30f);
			}
			MoveTypeLine();
		}
	}

	protected void CreateRingBullet()
	{
		int n_LINK_SKILL = BulletData.n_LINK_SKILL;
		SKILL_TABLE tSKILL_TABLE = ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT[n_LINK_SKILL];
		if (refPBMShoter.SOB as OrangeCharacter != null)
		{
			(refPBMShoter.SOB as OrangeCharacter).tRefPassiveskill.ReCalcuSkill(ref tSKILL_TABLE);
		}
		IceRingBullet iceRingBullet = CreateSubBullet<IceRingBullet>(tSKILL_TABLE);
		iceRingBullet.EnableColider(false);
		iceRingBullet.transform.parent = _transform;
		iceRingBullet.transform.localPosition = Vector3.zero;
		iceRingBullet.transform.localRotation = Quaternion.Euler(Vector3.zero);
		_ringBullets.Add(iceRingBullet);
	}

	public override void BackToPool()
	{
		base.BackToPool();
		_phase = Phase.POPUP;
		_tRadius = 1f;
		_tRadiusMinX = _radiusMin;
		_tRotateParam = 0f;
		for (int i = 0; i < _ringBullets.Count; i++)
		{
			if (!_ringBullets[i].bIsEnd)
			{
				_ringBullets[i].BackToPool();
			}
		}
		_ringBullets.Clear();
	}

	public override void LateUpdateFunc()
	{
		base.LateUpdateFunc();
		float num = 360f / (float)BulletData.n_NUM_SHOOT;
		_tRotateParam += (float)_direction * rollingSpeed * Time.deltaTime;
		for (int i = 0; i < _ringBullets.Count; i++)
		{
			if (!(_ringBullets[i] == null) && !_ringBullets[i].bIsEnd)
			{
				_ringBullets[i].transform.localPosition = new Vector3(_tRadius * _tRadiusMinX * Mathf.Cos((num * (float)i + _tRotateParam) * ((float)Math.PI / 180f)), _tRadius * Mathf.Sin((num * (float)i + _tRotateParam) * ((float)Math.PI / 180f)), 0f);
			}
		}
	}
}
