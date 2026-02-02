using System;
using StageLib;
using UnityEngine;

public class TsunamiBullet : BasicBullet
{
	[SerializeField]
	private Vector2 overrideColliderOffset;

	[SerializeField]
	private Vector2 overrideColliderSize;

	[SerializeField]
	private Vector3 scaleOfBegin;

	[SerializeField]
	private Vector3 scaleOfEnd;

	[SerializeField]
	private float scalingTime;

	[SerializeField]
	private float lifeTime;

	private bool isScaling;

	private Vector3 scalingDelta;

	private Vector3 scalingSpeed;

	private Vector3 currentScale;

	public override void Active(Vector3 pPos, Vector3 pDirection, LayerMask pTargetMask, IAimTarget pTarget = null)
	{
		base.Active(pPos, pDirection, pTargetMask, pTarget);
		Initialize();
	}

	public override void Active(Transform pTransform, Vector3 pDirection, LayerMask pTargetMask, IAimTarget pTarget = null)
	{
		base.Active(pTransform, pDirection, pTargetMask, pTarget);
		Initialize();
	}

	private void Initialize()
	{
		isScaling = true;
		currentScale = scaleOfBegin;
		if (scalingTime == 0f)
		{
			scalingTime = 1E-06f;
		}
		scalingSpeed = (scaleOfEnd - scaleOfBegin) / scalingTime;
		scalingDelta = AbsVector3(scaleOfEnd - scaleOfBegin);
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
}
