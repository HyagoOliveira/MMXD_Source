using System;
using System.Collections.Generic;
using UnityEngine;

public class CH031_SKL01 : CollideBullet
{
	private enum HitCycle
	{
		IGNORE = 0,
		FIRST = 1,
		CHECK_HIT_CYCLE = 2
	}

	private HitCycle hitCycle;

	private OrangeTimer timerHitCycle;

	public override void UpdateBulletData(SKILL_TABLE pData, string owner = "", int nInRecordID = 0, int nInNetID = 0, int nDirection = 1)
	{
		base.UpdateBulletData(pData, owner, nInRecordID, nInNetID, nDirection);
		hitCycle = HitCycle.IGNORE;
		if (pData.n_EFFECT == 4)
		{
			timerHitCycle = OrangeTimerManager.GetTimer();
			hitCycle = HitCycle.FIRST;
			timerHitCycle.TimerStart();
		}
	}

	public override void Hit(Collider2D col)
	{
		switch (hitCycle)
		{
		case HitCycle.FIRST:
			hitCycle = HitCycle.CHECK_HIT_CYCLE;
			break;
		case HitCycle.CHECK_HIT_CYCLE:
			if (timerHitCycle.GetMillisecond() < _hurtCycle)
			{
				return;
			}
			timerHitCycle.TimerStart();
			break;
		}
		if (CheckHitList(ref _ignoreList, col.transform))
		{
			return;
		}
		if (HitCallback != null)
		{
			base.HitTarget = col;
			HitCallback(col);
		}
		else
		{
			base.HitTarget = null;
		}
		int value = -1;
		_hitCount.TryGetValue(col.transform, out value);
		if (value == -1)
		{
			_hitCount.Add(col.transform, 1);
		}
		else
		{
			_hitCount[col.transform] = value + 1;
		}
		_ignoreList.Add(col.transform);
		if (FxImpact != "null")
		{
			bool flag = false;
			StageObjParam component = col.transform.GetComponent<StageObjParam>();
			if ((bool)component)
			{
				IAimTarget aimTarget = component.tLinkSOB as IAimTarget;
				if (aimTarget != null && aimTarget.AimTransform != null)
				{
					MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FxImpact, aimTarget.AimTransform.position + aimTarget.AimPoint, BulletQuaternion, Array.Empty<object>());
					flag = true;
				}
			}
			if (!flag)
			{
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FxImpact, col.transform.position, BulletQuaternion, Array.Empty<object>());
			}
		}
		CaluDmg(BulletData, col.transform, 0f, 0.5f);
	}

	private void OnTriggerExit2D(Collider2D col)
	{
		if (BulletData.n_TARGET == 3)
		{
			Transform transform = col.transform;
			OrangeCharacter component = col.transform.GetComponent<OrangeCharacter>();
			if ((bool)component && _hitCount.ContainsKey(transform))
			{
				component.selfBuffManager.RemoveBuffByCONDITIONID(BulletData.n_CONDITION_ID);
			}
			if (_hitCount.ContainsKey(transform))
			{
				_hitCount.Remove(transform);
			}
			_ignoreList.Remove(transform);
		}
	}

	public override void BackToPool()
	{
		foreach (KeyValuePair<Transform, int> item in _hitCount)
		{
			if (item.Key != null)
			{
				OrangeCharacter component = item.Key.GetComponent<OrangeCharacter>();
				if ((bool)component)
				{
					component.selfBuffManager.RemoveBuffByCONDITIONID(BulletData.n_CONDITION_ID);
				}
			}
		}
		base.BackToPool();
		if (hitCycle != 0)
		{
			OrangeTimerManager.ReturnTimer(timerHitCycle);
		}
		hitCycle = HitCycle.IGNORE;
	}
}
