using System;
using StageLib;
using UnityEngine;

public class SwellBullet : BasicBullet
{
	[SerializeField]
	private Vector2 overrideColliderOffset;

	[SerializeField]
	private Vector2 overrideColliderSize;

	private Vector3 scaleOfBegin;

	private Vector3 scaleOfEnd;

	[SerializeField]
	private float EndScale;

	[SerializeField]
	private float scalingTime;

	[SerializeField]
	private float lifeTime;

	[SerializeField]
	private float OpenCollideTime;

	private float OpenCollideTimer;

	private bool HasOpenCollide;

	private bool isScaling;

	private Vector3 scalingDelta;

	private Vector3 scalingSpeed;

	private Vector3 currentScale;

	[SerializeField]
	private bool isAutoBurst;

	[SerializeField]
	private float AutoBurstTime;

	private int AutoBurstFrame;

	public override void Active(Vector3 pPos, Vector3 pDirection, LayerMask pTargetMask, IAimTarget pTarget = null)
	{
		base.Active(pPos, pDirection, pTargetMask, pTarget);
		Initialize();
		if (OpenCollideTime > 0f)
		{
			OpenCollideTimer = 0f;
			HasOpenCollide = false;
			_capsuleCollider.enabled = false;
		}
		else
		{
			HasOpenCollide = true;
		}
		if (isAutoBurst)
		{
			AutoBurstFrame = GameLogicUpdateManager.GameFrame + (int)(AutoBurstTime * 20f);
		}
	}

	public override void Active(Transform pTransform, Vector3 pDirection, LayerMask pTargetMask, IAimTarget pTarget = null)
	{
		base.Active(pTransform, pDirection, pTargetMask, pTarget);
		Initialize();
		if (OpenCollideTime > 0f)
		{
			OpenCollideTimer = 0f;
			HasOpenCollide = false;
			_capsuleCollider.enabled = false;
		}
		else
		{
			HasOpenCollide = true;
		}
		if (isAutoBurst)
		{
			AutoBurstFrame = GameLogicUpdateManager.GameFrame + (int)(AutoBurstTime * 20f);
		}
	}

	private void Initialize()
	{
		scaleOfBegin = _transform.localScale;
		scaleOfEnd = _transform.localScale * EndScale;
		isScaling = true;
		currentScale = _transform.localScale;
		if (scalingTime == 0f)
		{
			scalingTime = 1E-06f;
		}
		scalingSpeed = (scaleOfEnd - _transform.localScale) / scalingTime;
		scalingDelta = AbsVector3(_transform.localScale * EndScale - scaleOfBegin);
		_capsuleCollider.direction = CapsuleDirection2D.Vertical;
		StartCoroutine(StageResManager.TweenFloatCoroutine(0f, 1f, lifeTime, null, delegate
		{
			Stop();
		}));
	}

	private Vector3 AbsVector3(Vector3 vector)
	{
		vector.x = Mathf.Abs(vector.x);
		vector.y = Mathf.Abs(vector.y);
		vector.z = Mathf.Abs(vector.z);
		return vector;
	}

	protected override void MoveBullet()
	{
		if (!HasOpenCollide)
		{
			OpenCollideTimer += Time.deltaTime;
			if (OpenCollideTimer > OpenCollideTime)
			{
				HasOpenCollide = true;
				_capsuleCollider.enabled = true;
			}
		}
		base.MoveBullet();
		if (isScaling)
		{
			Vector3 vector = scalingSpeed * Time.deltaTime;
			scalingDelta -= AbsVector3(vector);
			if (scalingDelta.x <= 0f)
			{
				isScaling = false;
				currentScale = scaleOfEnd;
			}
			else
			{
				currentScale += vector;
			}
		}
		else
		{
			currentScale = scaleOfEnd;
		}
		_transform.localScale = currentScale;
		_capsuleCollider.offset = overrideColliderOffset;
		_capsuleCollider.size = overrideColliderSize;
	}

	protected override void Stop()
	{
		if (!(FxEnd == "") && FxEnd != null)
		{
			MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FxEnd, _transform.position, _transform.rotation, Array.Empty<object>());
			if (needPlayEndSE)
			{
				PlaySE(_HitGuardSE[0], _HitGuardSE[1], isForceSE);
			}
			_transform.localScale = scaleOfBegin;
			_capsuleCollider.offset = Vector2.zero;
			_capsuleCollider.size = Vector2.one * 0.01f;
			BackToPool();
			for (int i = 0; i < bulletFxArray.Length; i++)
			{
				bulletFxArray[i].Clear();
			}
		}
	}

	public override void LateUpdateFunc()
	{
		if (isAutoBurst && GameLogicUpdateManager.GameFrame > AutoBurstFrame)
		{
			Phase = BulletPhase.BackToPool;
		}
		base.LateUpdateFunc();
	}
}
