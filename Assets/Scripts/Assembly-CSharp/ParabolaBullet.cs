using System;
using System.Collections;
using UnityEngine;

public class ParabolaBullet : BulletBase, ILogicUpdate, IManagedUpdateBehavior
{
	private enum BulletState
	{
		MOVEING = 0,
		RECYCLE = 1
	}

	private BulletState bulletState = BulletState.RECYCLE;

	private Vector2 speed;

	private Vector2 Gravity;

	private VInt3 nowPos;

	private VInt3 endPos;

	private VInt timeDelta;

	protected float dropSpd = 6.5f;

	protected float g = -30f;

	private int nowLogicFrame;

	private int endLogicFrame;

	private int logicLength;

	private Collider2D hitCol;

	private float distanceDelta;

	protected bool isHit;

	public int EndLogicFrame
	{
		get
		{
			return endLogicFrame;
		}
		set
		{
			endLogicFrame = value;
		}
	}

	public int NowLogicFrame
	{
		get
		{
			return nowLogicFrame;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		Rigidbody2D rigidbody2D = base.gameObject.AddOrGetComponent<Rigidbody2D>();
		rigidbody2D.bodyType = RigidbodyType2D.Kinematic;
		rigidbody2D.useFullKinematicContacts = true;
		hitCol = GetComponent<CircleCollider2D>();
		hitCol.enabled = false;
	}

	public override void Active(Vector3 pPos, Vector3 pDirection, LayerMask pTargetMask, IAimTarget pTarget = null)
	{
		TargetMask = pTargetMask;
		_transform.SetParentNull();
		nowPos = new VInt3(_transform.localPosition);
		endPos = new VInt3(pPos);
		float num = Vector3.Distance(nowPos.vec3, endPos.vec3) / dropSpd;
		logicLength = (int)(num / GameLogicUpdateManager.m_fFrameLen);
		timeDelta = new VInt(num / (float)logicLength);
		speed = new Vector2((endPos.vec3.x - nowPos.vec3.x) / num, (endPos.vec3.y - nowPos.vec3.y) / num - 0.5f * g * num);
		Gravity = Vector2.zero;
		nowLogicFrame = 0;
		endLogicFrame = nowLogicFrame + logicLength;
		hitCol.enabled = true;
		bulletState = BulletState.MOVEING;
		MonoBehaviourSingleton<GameLogicUpdateManager>.Instance.AddUpdate(this);
		MonoBehaviourSingleton<UpdateManager>.Instance.AddUpdate(this);
		PlaySE(_UseSE[0], _UseSE[1], true);
		isHit = false;
	}

	private void OnCollisionStay2D(Collision2D col)
	{
		if (ManagedSingleton<StageHelper>.Instance.bEnemyActive && !MonoBehaviourSingleton<OrangeGameManager>.Instance.IsGamePause)
		{
			if (((1 << col.gameObject.layer) & (int)TargetMask) != 0)
			{
				Hit(col.collider);
			}
			if (((1 << col.gameObject.layer) & (1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockLayer)) != 0)
			{
				isHit = true;
			}
		}
	}

	public override void Hit(Collider2D col)
	{
		CaluDmg(BulletData, col.transform, 0f, 0.5f);
		if (FxImpact != string.Empty)
		{
			MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FxImpact, _transform.position, BulletQuaternion, Array.Empty<object>());
		}
		BackToPool();
	}

	public void LogicUpdate()
	{
		nowLogicFrame++;
		if (bulletState != 0)
		{
			int num = 1;
		}
		else if (nowLogicFrame >= endLogicFrame)
		{
			nowPos = endPos;
			BackToPool();
		}
		else
		{
			Gravity.y = g * ((float)nowLogicFrame * timeDelta.scalar);
			nowPos += new VInt3((speed + Gravity) * timeDelta.scalar);
			distanceDelta = Vector3.Distance(base.transform.localPosition, nowPos.vec3) * (Time.deltaTime / GameLogicUpdateManager.m_fFrameLen);
		}
	}

	public virtual void UpdateFunc()
	{
		_transform.localPosition = Vector3.MoveTowards(_transform.localPosition, nowPos.vec3, distanceDelta);
	}

	public override void BackToPool()
	{
		hitCol.enabled = false;
		bulletState = BulletState.RECYCLE;
		MonoBehaviourSingleton<GameLogicUpdateManager>.Instance.RemoveUpdate(this);
		MonoBehaviourSingleton<UpdateManager>.Instance.RemoveUpdate(this);
		base.BackToPool();
	}

	private void OnDestroy()
	{
		MonoBehaviourSingleton<GameLogicUpdateManager>.Instance.RemoveUpdate(this);
		MonoBehaviourSingleton<UpdateManager>.Instance.RemoveUpdate(this);
	}

	protected override IEnumerator OnStartMove()
	{
		yield return null;
	}
}
