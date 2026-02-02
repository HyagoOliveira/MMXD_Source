#define RELEASE
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CH066_BigKobunBullet : CollideBullet
{
	[SerializeField]
	protected Transform ModelTransform;

	[SerializeField]
	protected Animator _animator;

	protected int[] _animationHash;

	[SerializeField]
	protected GameObject[] Emotions;

	[SerializeField]
	protected GameObject[] Weapons;

	protected bool bEditPause;

	protected Vector2 lastPosition = new Vector2(0f, 0f);

	protected float heapDistance;

	[SerializeField]
	protected float scaleSize = 7.2f;

	protected override void Awake()
	{
		base.Awake();
		Transform[] target = _transform.GetComponentsInChildren<Transform>(true);
		ModelTransform = OrangeBattleUtility.FindChildRecursive(ref target, "model", true);
		_animator = ModelTransform.GetComponent<Animator>();
		HashAnimation();
	}

	protected virtual void HashAnimation()
	{
		_animationHash = new int[3];
		_animationHash[0] = Animator.StringToHash("BS067@skill_5_start");
		_animationHash[1] = Animator.StringToHash("BS067@skill_5_loop");
		_animationHash[2] = Animator.StringToHash("BS067@skill_5_end");
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

	public override void Active(Vector3 pPos, Vector3 pDirection, LayerMask pTargetMask, IAimTarget pTarget = null)
	{
		_transform.position = pPos;
		MonoBehaviourSingleton<FxManager>.Instance.UpdateFx(bulletFxArray, true);
		_rigidbody2D.WakeUp();
		lastPosition = _transform.position;
		heapDistance = 0f;
		Direction = pDirection;
		if (Direction.x > 0f)
		{
			Velocity = new Vector3(BulletData.n_SPEED, 0f, 0f);
			ModelTransform.localScale = Vector3.one * scaleSize;
		}
		else
		{
			Velocity = new Vector3(-BulletData.n_SPEED, 0f, 0f);
			ModelTransform.localScale = new Vector3(scaleSize, scaleSize, 0f - scaleSize);
		}
		SetAnimation();
		SetEmotions(4);
		SetWeapon(1);
		Active(pTargetMask);
	}

	public override void Active(Transform pTransform, Vector3 pDirection, LayerMask pTargetMask, IAimTarget pTarget = null)
	{
		Debug.Log("NOPE");
		_transform.position = pTransform.position;
		MonoBehaviourSingleton<FxManager>.Instance.UpdateFx(bulletFxArray, true);
		_rigidbody2D.WakeUp();
		lastPosition = _transform.position;
		heapDistance = 0f;
		Direction = pDirection;
		if (Direction.x > 0f)
		{
			Velocity = new Vector3(BulletData.n_SPEED, 0f, 0f);
			ModelTransform.localScale = Vector3.one * scaleSize;
		}
		else
		{
			Velocity = new Vector3(-BulletData.n_SPEED, 0f, 0f);
			ModelTransform.localScale = new Vector3(scaleSize, scaleSize, 0f - scaleSize);
		}
		SetAnimation();
		SetEmotions(4);
		SetWeapon(1);
		Active(pTargetMask);
	}

	public override void UpdateBulletData(SKILL_TABLE pData, string owner = "", int nInRecordID = 0, int nInNetID = 0, int nDirection = 1)
	{
		base.UpdateBulletData(pData, owner, nInRecordID, nInNetID, nDirection);
		_transform.eulerAngles = Vector3.zero;
	}

	protected void SetAnimation()
	{
		_animator.Play(_animationHash[1], 0, 0f);
	}

	protected void SetEmotions(int type)
	{
		for (int i = 0; i < Emotions.Length; i++)
		{
			if (i == type)
			{
				Emotions[i].SetActive(true);
			}
			else
			{
				Emotions[i].SetActive(false);
			}
		}
	}

	protected void SetWeapon(int type)
	{
		for (int i = 0; i < Weapons.Length; i++)
		{
			if (i == type)
			{
				Weapons[i].SetActive(true);
			}
			else
			{
				Weapons[i].SetActive(false);
			}
		}
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
				MoveBullet();
			}
			yield return CoroutineDefine._waitForEndOfFrame;
		}
		BackToPool();
		yield return null;
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
