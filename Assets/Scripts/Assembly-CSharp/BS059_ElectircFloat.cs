using UnityEngine;

public class BS059_ElectircFloat : BasicBullet, ILogicUpdate
{
	private Vector2 Gravity;

	private VInt3 nowPos;

	private float distanceDelta;

	private OrangeTimer CountTimer;

	private bool StartTracking;

	private int _direction = 1;

	private EnemyAutoAimSystem EnemyAIS;

	public override void Active(Vector3 pPos, Vector3 pDirection, LayerMask pTargetMask, IAimTarget pTarget = null)
	{
		base.Active(pPos, pDirection, pTargetMask, pTarget);
		MonoBehaviourSingleton<GameLogicUpdateManager>.Instance.AddUpdate(this);
		CountTimer = OrangeTimerManager.GetTimer();
		CountTimer.TimerStart();
		OrangeBattleUtility.AddEnemyAutoAimSystem(base.transform, out EnemyAIS);
		EnemyAIS.UpdateAimRange(30f);
		StartTracking = false;
		if (pDirection.x > 0f)
		{
			_direction = 1;
		}
		else
		{
			_direction = -1;
		}
		StartTracking = false;
	}

	public void LogicUpdate()
	{
		if (Target == null)
		{
			Target = EnemyAIS.GetClosetPlayer();
		}
		if (!StartTracking && CountTimer.GetMillisecond() > 200)
		{
			Velocity = new Vector3(4f, 0f, 0f);
			Direction = new Vector3(Direction.x, 0f, 0f);
			CountTimer.TimerStop();
			OrangeTimerManager.ReturnTimer(CountTimer);
			_transform.eulerAngles = new Vector3(0f, 0f, Vector2.SignedAngle(Vector2.right, Vector2.right * _direction));
			StartTracking = true;
		}
		else
		{
			if (!StartTracking)
			{
				return;
			}
			if (Target != null)
			{
				if (Target.AimTransform.position.y != _transform.position.y)
				{
					Velocity = new Vector3(4f, (float)_direction * (Target.AimTransform.position.y - _transform.position.y) / 2f, 0f);
				}
			}
			else
			{
				Velocity = new Vector3(4f, 0f, 0f);
			}
		}
	}

	public override void BackToPool()
	{
		MonoBehaviourSingleton<GameLogicUpdateManager>.Instance.RemoveUpdate(this);
		base.BackToPool();
	}
}
