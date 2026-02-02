using UnityEngine;

public class CH048_BoomerangBullet : LogicBasicBullet
{
	private enum Phase
	{
		POPUP = 0,
		WAIT = 1,
		MOVE = 2
	}

	private Phase _phase;

	protected override void DoActive(IAimTarget pTarget)
	{
		base.DoActive(pTarget);
		Target = pTarget;
		if (!isSubBullet && BulletData.n_NUM_SHOOT > 1)
		{
			SetDirection(0, this);
			for (int i = 1; i < BulletData.n_NUM_SHOOT; i++)
			{
				CreateBoomerangBullet(i);
			}
		}
	}

	protected void CreateBoomerangBullet(int index)
	{
		CH048_BoomerangBullet cH048_BoomerangBullet = CreateSubBullet<CH048_BoomerangBullet>(BulletData);
		if ((bool)cH048_BoomerangBullet)
		{
			cH048_BoomerangBullet.SetTartget(Target);
			SetDirection(index, cH048_BoomerangBullet);
		}
	}

	protected void SetDirection(int index, CH048_BoomerangBullet bullet)
	{
		if (!(bullet == null))
		{
			float z = 0f;
			if (BulletData.n_NUM_SHOOT > 1)
			{
				float num = BulletData.f_ANGLE / (float)(BulletData.n_NUM_SHOOT - 1);
				z = BulletData.f_ANGLE / 2f - (float)index * num;
			}
			Vector3 popupDirction = Quaternion.Euler(0f, 0f, z) * Vector3.up;
			bullet.SetPopupDirction(popupDirction);
		}
	}

	protected override void MoveBullet()
	{
		if (_phase == Phase.POPUP)
		{
			MoveTypeLine();
			if (mainPhase == BulletPhase.End)
			{
				mainPhase = BulletPhase.Move;
				_phase = Phase.WAIT;
				_transform.eulerAngles = Vector3.zero;
			}
		}
		else if (_phase == Phase.WAIT && ActivateTimer.GetMillisecond() > 1000)
		{
			AttackTarget();
		}
		else if (_phase == Phase.MOVE)
		{
			MoveTypeLine();
		}
	}

	public override void BackToPool()
	{
		base.BackToPool();
		_phase = Phase.POPUP;
	}

	public void SetPopupDirction(Vector3 direction)
	{
		if (!isMirror)
		{
			_transform.eulerAngles = new Vector3(0f, 0f, Vector2.SignedAngle(Vector2.right, direction));
		}
		else
		{
			float num = Vector2.SignedAngle(Vector2.right, direction);
			_transform.eulerAngles = new Vector3(0f, 0f, Vector2.SignedAngle(Vector2.right, direction));
			if (num > 90f && num < 270f)
			{
				_transform.localScale = new Vector3(1f, -1f, 1f);
			}
			else
			{
				_transform.localScale = Vector3.one;
			}
		}
		Direction = direction;
		CaluLogicFrame(BulletData.n_SPEED, 1.5f, direction);
	}

	public void AttackTarget()
	{
		if (!bIsEnd && mainPhase == BulletPhase.Move)
		{
			if (Target != null && Target.Activate && !(Target.AimTransform == null))
			{
				Direction = (Target.AimPosition.xy() - base.transform.position.xy()).normalized;
			}
			_transform.eulerAngles = new Vector3(0f, 0f, Vector2.SignedAngle(Vector2.right, Direction));
			CaluLogicFrame(BulletData.n_SPEED, BulletData.f_DISTANCE, Direction);
			_phase = Phase.MOVE;
		}
	}
}
