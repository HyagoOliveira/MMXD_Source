using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CH079_PoisonpharBullet : CollideBullet
{
	protected FxBase _fxPoison;

	public override void Active(Vector3 pPos, Vector3 pDirection, LayerMask pTargetMask, IAimTarget pTarget = null)
	{
		base.Active(pPos, pDirection, pTargetMask, pTarget);
		_fxPoison = MonoBehaviourSingleton<FxManager>.Instance.PlayReturn<FxBase>("fxuse_poisonphar_000", _transform.position, Quaternion.identity, Array.Empty<object>());
	}

	public override void Active(Transform pTransform, Vector3 pDirection, LayerMask pTargetMask, IAimTarget pTarget = null)
	{
		base.Active(pTransform, pDirection, pTargetMask, pTarget);
		_fxPoison = MonoBehaviourSingleton<FxManager>.Instance.PlayReturn<FxBase>("fxuse_poisonphar_000", _transform.position, Quaternion.identity, Array.Empty<object>());
	}

	public new void Active(Transform pTransform, Quaternion pQuaternion, LayerMask pTargetMask, bool follow, IAimTarget pTarget = null)
	{
		base.Active(pTransform, pQuaternion, pTargetMask, follow, pTarget);
		_fxPoison = MonoBehaviourSingleton<FxManager>.Instance.PlayReturn<FxBase>("fxuse_poisonphar_000", _transform.position, Quaternion.identity, Array.Empty<object>());
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
			if (MonoBehaviourSingleton<OrangeGameManager>.Instance.IsGamePause)
			{
				yield return CoroutineDefine._waitForEndOfFrame;
				continue;
			}
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
				if (_mainCamera == null)
				{
					_mainCamera = MonoBehaviourSingleton<OrangeSceneManager>.Instance.MainCamera;
				}
				if (_mainCamera != null)
				{
					base.transform.LookAt(_mainCamera.transform.position, -Vector3.up);
				}
			}
			if ((base.gameObject.layer == ManagedSingleton<OrangeLayerManager>.Instance.PlayerLayer || base.gameObject.layer == ManagedSingleton<OrangeLayerManager>.Instance.RenderPlayer) && isCharacter_root)
			{
				base.gameObject.layer = ManagedSingleton<OrangeLayerManager>.Instance.BulletLayer;
			}
			if (_duration != -1 && _durationTimer.GetMillisecond() >= _duration)
			{
				IsDestroy = true;
				break;
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
		if ((bool)_fxPoison)
		{
			_fxPoison.BackToPool();
			_fxPoison = null;
		}
	}
}
