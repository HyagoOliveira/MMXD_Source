using System.Collections;
using CallbackDefs;
using UnityEngine;

public class ParabolaBulletForTryShoot : BasicBullet, ILogicUpdate, IManagedUpdateBehavior
{
	protected enum BulletState
	{
		MOVEING = 0,
		RECYCLE = 1
	}

	protected BulletState bulletState = BulletState.RECYCLE;

	protected Vector2 speed;

	protected Vector2 Gravity;

	protected VInt3 nowPos;

	protected VInt3 endPos;

	protected VInt timeDelta;

	[SerializeField]
	protected float dropSpd = 6.5f;

	[SerializeField]
	protected float g = -30f;

	protected int nowLogicFrame;

	protected int endLogicFrame;

	protected int logicFrame;

	[SerializeField]
	protected float DisplacementY = 0.5f;

	[SerializeField]
	protected float time;

	protected float bulletspeed;

	[SerializeField]
	protected bool IgnoreCeiling;

	protected float distanceDelta;

	protected bool isHit;
    [System.Obsolete]
    public CallbackObjs HitCallbackObjs;

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

	public override void Active(Vector3 pStartPos, Vector3 pEndPos, LayerMask pTargetMask, IAimTarget pTarget = null)
	{
		_transform.SetParentNull();
		_transform.position = pStartPos;
		TargetMask = pTargetMask;
		UseMask = (int)BlockMask | (int)pTargetMask;
		OnStart();
		MonoBehaviourSingleton<FxManager>.Instance.UpdateFx(bulletFxArray, true);
		isHitBlock = (needWeaponImpactSE = true);
		nowPos = new VInt3(_transform.localPosition);
		endPos = new VInt3(pEndPos);
		bulletspeed = 0f;
		if (time == 0f)
		{
			bulletspeed = Vector3.Distance(nowPos.vec3, endPos.vec3) / dropSpd;
		}
		else
		{
			bulletspeed = time;
		}
		logicFrame = (int)(bulletspeed / GameLogicUpdateManager.m_fFrameLen);
		timeDelta = new VInt(bulletspeed / (float)logicFrame);
		speed = new Vector2((endPos.vec3.x - nowPos.vec3.x) / bulletspeed, (endPos.vec3.y - nowPos.vec3.y) / bulletspeed - DisplacementY * g * bulletspeed);
		Gravity = Vector2.zero;
		nowLogicFrame = 0;
		endLogicFrame = nowLogicFrame + logicFrame;
		bulletState = BulletState.MOVEING;
		MonoBehaviourSingleton<GameLogicUpdateManager>.Instance.AddUpdate(this);
		MonoBehaviourSingleton<UpdateManager>.Instance.AddUpdate(this);
		base.transform.position = pStartPos;
		base.SoundSource.UpdateDistanceCall();
		PlayUseSE();
		isHit = false;
	}

	public virtual void LogicUpdate()
	{
		nowLogicFrame++;
		if (bulletState != 0)
		{
			int num = 1;
			return;
		}
		Gravity.y = g * ((float)nowLogicFrame * timeDelta.scalar);
		nowPos += new VInt3((speed + Gravity) * timeDelta.scalar);
		distanceDelta = Vector3.Distance(base.transform.localPosition, nowPos.vec3) * (Time.deltaTime / GameLogicUpdateManager.m_fFrameLen);
	}

	public virtual void UpdateFunc()
	{
		_transform.localPosition = Vector3.MoveTowards(_transform.localPosition, nowPos.vec3, distanceDelta);
	}

	public override void BackToPool()
	{
		if (IgnoreCeiling && isHitBlock && nowLogicFrame < endLogicFrame)
		{
			Phase = BulletPhase.Normal;
			return;
		}
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

	protected override void HitCheck(Collider2D col)
	{
		if (!_hitList.Contains(col.transform))
		{
			lastHit = col.transform;
			_hitList.Add(col.transform);
			if (HitCallback != null)
			{
				HitCallback(col);
			}
			if (HitCallbackObjs != null)
			{
				HitCallbackObjs(col, base.gameObject.GetInstanceID());
			}
		}
	}

	public void SetSpeed(Vector2 newSpeed)
	{
		speed = newSpeed;
	}

	protected override void OnTriggerEnter2D(Collider2D col)
	{
		base.OnTriggerEnter2D(col);
	}

	protected override void OnTriggerStay2D(Collider2D col)
	{
		base.OnTriggerStay2D(col);
	}

	public override void OnTriggerHit(Collider2D col)
	{
		base.OnTriggerHit(col);
	}

	protected override void GenerateImpactFx(bool bPlaySE = true)
	{
		base.GenerateImpactFx(bPlaySE);
	}

	protected override void GenerateEndFx(bool bPlaySE = true)
	{
		base.GenerateEndFx(bPlaySE);
	}
}
