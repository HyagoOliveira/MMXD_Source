using System.Collections;
using System.Collections.Generic;
using CallbackDefs;
using UnityEngine;

public class BombBullet : BulletBase, ILogicUpdate
{
	private enum BombState
	{
		WAIT = 0,
		DROP = 1,
		RECYCLE = 2,
		COUNT = 3
	}

	[HideInInspector]
	public float[] animationLength;

	[SerializeField]
	private SplashBullet splashBullet;

	public Callback hitCB;

	private BombState bombState;

	private int nowLogicFrame;

	private int endLogicFrame;

	private int logicLength;

	private VInt3 nowPos;

	private VInt3 direction = new VInt3(0, -1, 0);

	private float distanceDelta;

	protected List<Transform> _hitList;

	private Collider2D hitCol;

	private bool isPlayOneSE = true;

	public MOB_TABLE RefMob { get; set; }

	public PerBuffManager.BuffStatus RefBuffStatus { get; set; }

	protected override void Awake()
	{
		base.Awake();
		hitCol = GetComponent<BoxCollider2D>();
		hitCol.enabled = false;
		animationLength = new float[3];
		CreateSplashPoolObj();
	}

	public override void Active(Transform pTransform, Vector3 pDirection, LayerMask pTargetMask, IAimTarget pTarget = null)
	{
		bombState = BombState.WAIT;
		nowPos = new VInt3(_transform.localPosition);
		SetBulletAtk(null, RefBuffStatus, RefMob);
		new VInt(pTransform.localPosition.y);
		logicLength = (int)(animationLength[0] / GameLogicUpdateManager.m_fFrameLen);
		nowLogicFrame = GameLogicUpdateManager.GameFrame;
		endLogicFrame = nowLogicFrame + logicLength;
		TargetMask = pTargetMask;
		UseMask = (int)BlockMask | (int)pTargetMask;
		hitCol.enabled = true;
		MonoBehaviourSingleton<GameLogicUpdateManager>.Instance.AddUpdate(this);
		PlayUseSE();
		isPlayOneSE = true;
	}

	private void OnTriggerStay2D(Collider2D col)
	{
		if (ManagedSingleton<StageHelper>.Instance.bEnemyActive && !MonoBehaviourSingleton<OrangeGameManager>.Instance.IsGamePause && !col.isTrigger && ((1 << col.gameObject.layer) & (int)UseMask) != 0)
		{
			Hit(col);
		}
	}

	public override void Hit(Collider2D col)
	{
		hitCol.enabled = false;
		if (bombState != BombState.RECYCLE)
		{
			SetSplash();
			bombState = BombState.RECYCLE;
			int layer = col.gameObject.layer;
			if (isPlayOneSE)
			{
				PlaySE("HitSE", "ht_guard04", isForceSE);
				isPlayOneSE = false;
			}
		}
	}

	private void SetSplash()
	{
		SplashBullet poolObj = MonoBehaviourSingleton<PoolManager>.Instance.GetPoolObj<SplashBullet>(splashBullet.gameObject.name);
		poolObj._transform.SetParent(null);
		poolObj.UpdateBulletData(BulletData);
		poolObj.SetBulletAtk(null, RefBuffStatus, RefMob);
		poolObj._transform.position = _transform.position;
		poolObj.Active(TargetMask);
		hitCB.CheckTargetToInvoke();
		BackToPool();
	}

	private int Clamp(float p_val)
	{
		return Mathf.Clamp(Mathf.RoundToInt(p_val), -1, 1);
	}

	public void LogicUpdate()
	{
		switch (bombState)
		{
		case BombState.WAIT:
			if (nowLogicFrame >= endLogicFrame)
			{
				logicLength = (int)(animationLength[1] / GameLogicUpdateManager.m_fFrameLen);
				endLogicFrame = nowLogicFrame + (int)(animationLength[1] / GameLogicUpdateManager.m_fFrameLen);
				bombState = BombState.DROP;
			}
			break;
		case BombState.DROP:
			if (nowLogicFrame >= endLogicFrame)
			{
				BackToPool();
			}
			break;
		}
		nowLogicFrame = GameLogicUpdateManager.GameFrame;
	}

	public override void BackToPool()
	{
		bombState = BombState.RECYCLE;
		MonoBehaviourSingleton<GameLogicUpdateManager>.Instance.RemoveUpdate(this);
		hitCB.CheckTargetToInvoke();
		base.BackToPool();
	}

	private void OnDisable()
	{
		MonoBehaviourSingleton<GameLogicUpdateManager>.Instance.RemoveUpdate(this);
	}

	protected override IEnumerator OnStartMove()
	{
		yield return null;
	}

	private void CreateSplashPoolObj()
	{
		MonoBehaviourSingleton<PoolManager>.Instance.CreatePoolBaseLocal<SplashBullet>(Object.Instantiate(splashBullet), splashBullet.gameObject.name);
	}
}
