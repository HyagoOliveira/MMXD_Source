#define RELEASE
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CH061_TornadoBullet : CollideBullet
{
	[SerializeField]
	[Range(0f, 1f)]
	private float moveTrigger = 0.5f;

	protected Vector2 lastPosition = new Vector2(0f, 0f);

	protected float heapDistance;

	protected bool bEditPause;

	public override void Active(Vector3 pPos, Vector3 pDirection, LayerMask pTargetMask, IAimTarget pTarget = null)
	{
		_transform.position = pPos;
		MonoBehaviourSingleton<FxManager>.Instance.UpdateFx(bulletFxArray, true);
		_rigidbody2D.WakeUp();
		heapDistance = 0f;
		lastPosition = _transform.position;
		Direction = pDirection;
		if (Direction.x > 0f)
		{
			Velocity = new Vector3(BulletData.n_SPEED, 0f, 0f);
		}
		else
		{
			Velocity = new Vector3(-BulletData.n_SPEED, 0f, 0f);
		}
		Active(pTargetMask);
	}

	public override void Active(Transform pTransform, Vector3 pDirection, LayerMask pTargetMask, IAimTarget pTarget = null)
	{
		Debug.Log("NOPE");
		_transform.position = pTransform.position;
		MonoBehaviourSingleton<FxManager>.Instance.UpdateFx(bulletFxArray, true);
		_rigidbody2D.WakeUp();
		heapDistance = 0f;
		lastPosition = _transform.position;
		Direction = pDirection;
		if (Direction.x > 0f)
		{
			Velocity = new Vector3(BulletData.n_SPEED, 0f, 0f);
		}
		else
		{
			Velocity = new Vector3(-BulletData.n_SPEED, 0f, 0f);
		}
		Active(pTargetMask);
	}

	protected override IEnumerator OnStartMove()
	{
		double hitCnt = 1.0;
		IsActivate = true;
		_hitCollider.enabled = true;
		_clearTimer.TimerStart();
		_durationTimer.TimerStart();
		_ignoreList.Clear();
		_hitCount.Clear();
		if (useHitGuardCount())
		{
			_hitGuardTimer.TimerStart();
		}
		PlayCycleSE();
		while (!IsDestroy)
		{
			if (_clearTimer.GetMillisecond() >= _hurtCycle || _forceClearList)
			{
				_forceClearList = false;
				_clearTimer.TimerStart();
				_ignoreList.Clear();
				_rigidbody2D.WakeUp();
				if (BulletData.n_DAMAGE_COUNT != 0)
				{
					foreach (KeyValuePair<Transform, int> item in _hitCount)
					{
						if (item.Value >= BulletData.n_DAMAGE_COUNT)
						{
							_ignoreList.Add(item.Key);
						}
					}
				}
				if (hitCnt < MaxHit)
				{
					PlayCycleSE();
					hitCnt += 1.0;
				}
			}
			if (AlwaysFaceCamera)
			{
				base.transform.LookAt(_mainCamera.transform.position, -Vector3.up);
			}
			if ((base.gameObject.layer == ManagedSingleton<OrangeLayerManager>.Instance.PlayerLayer || base.gameObject.layer == ManagedSingleton<OrangeLayerManager>.Instance.RenderPlayer) && isCharacter_root)
			{
				base.gameObject.layer = ManagedSingleton<OrangeLayerManager>.Instance.BulletLayer;
			}
			if (_duration != -1)
			{
				if (_durationTimer.GetMillisecond() >= _duration)
				{
					IsDestroy = true;
					break;
				}
				if ((float)_durationTimer.GetMillisecond() >= (float)_duration * moveTrigger)
				{
					MoveBullet();
				}
			}
			yield return CoroutineDefine._waitForEndOfFrame;
		}
		BackToPool();
		yield return null;
	}

	protected void OnApplicationPause(bool isPause)
	{
		PauseGame(isPause);
	}

	protected void PauseGame(bool isPause)
	{
		if (isPause)
		{
			_clearTimer.TimerPause();
			_durationTimer.TimerPause();
		}
		else
		{
			_clearTimer.TimerPause();
			_durationTimer.TimerResume();
		}
	}

	public override void BackToPool()
	{
		base.BackToPool();
		heapDistance = 0f;
	}

	protected void MoveBullet()
	{
		if (!(heapDistance >= BulletData.f_DISTANCE) && !MonoBehaviourSingleton<OrangeGameManager>.Instance.IsGamePause)
		{
			Vector3 translation = Velocity * Time.deltaTime;
			_transform.Translate(translation);
			heapDistance += Vector2.Distance(lastPosition, _transform.position);
			lastPosition = _transform.position;
		}
	}
}
