using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplashBullet : BulletBase, ILogicUpdate
{
	private new enum ColliderType
	{
		CIRCLE = 0,
		BOX = 1
	}

	private int LogicActiveTime = 5;

	private int LogicTrigger;

	public bool IsActivate;

	private Collider2D _collider;

	private HashSet<Transform> _ignoreList;

	private ColliderType colliderType;

	private CircleCollider2D _circleCollider2D;

	private BoxCollider2D _boxCollider2D;

	private void OnTriggerStay2D(Collider2D col)
	{
		if (ManagedSingleton<StageHelper>.Instance.bEnemyActive && !MonoBehaviourSingleton<OrangeGameManager>.Instance.IsGamePause && ((1 << col.gameObject.layer) & (int)UseMask) != 0)
		{
			Hit(col);
		}
	}

	protected override void Awake()
	{
		base.Awake();
		Rigidbody2D rigidbody2D = base.gameObject.AddOrGetComponent<Rigidbody2D>();
		rigidbody2D.bodyType = RigidbodyType2D.Kinematic;
		rigidbody2D.useFullKinematicContacts = true;
		if ((bool)base.gameObject.GetComponent<BoxCollider2D>())
		{
			colliderType = ColliderType.BOX;
			_boxCollider2D = base.gameObject.GetComponent<BoxCollider2D>();
			_collider = _boxCollider2D;
			_boxCollider2D.size = new Vector2(0.5f, 0.5f);
			_circleCollider2D = null;
		}
		else
		{
			colliderType = ColliderType.CIRCLE;
			_circleCollider2D = base.gameObject.AddOrGetComponent<CircleCollider2D>();
			_collider = _circleCollider2D;
			_circleCollider2D.radius = 0.5f;
			_boxCollider2D = null;
		}
		_collider.isTrigger = true;
		_ignoreList = new HashSet<Transform>();
	}

	public void Active(LayerMask pTargetMask, bool activeEffect = true)
	{
		TargetMask = pTargetMask;
		UseMask = (int)BlockMask | (int)pTargetMask;
		_collider.enabled = true;
		LogicTrigger = GameLogicUpdateManager.GameFrame + LogicActiveTime;
		if (activeEffect && FxImpact != string.Empty)
		{
			MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FxImpact, _transform.position, BulletQuaternion, Array.Empty<object>());
		}
		PlayUseSE();
		MonoBehaviourSingleton<GameLogicUpdateManager>.Instance.AddUpdate(this);
	}

	public override void Active(Transform pTransform, Vector3 pDirection, LayerMask pTargetMask, IAimTarget pTarget = null)
	{
		TargetMask = pTargetMask;
		UseMask = (int)BlockMask | (int)pTargetMask;
		_collider.enabled = true;
		LogicTrigger = GameLogicUpdateManager.GameFrame + LogicActiveTime;
		PlayUseSE();
		MonoBehaviourSingleton<GameLogicUpdateManager>.Instance.AddUpdate(this);
		base.transform.SetParent(pTransform);
		base.transform.localPosition = Vector3.zero;
		ColliderType colliderType = this.colliderType;
		if (colliderType == ColliderType.CIRCLE || colliderType != ColliderType.BOX)
		{
			base.transform.rotation = Quaternion.identity;
			return;
		}
		base.transform.rotation = Quaternion.identity;
		float z = Vector2.SignedAngle(Vector2.right, pDirection);
		base.transform.eulerAngles = new Vector3(0f, 0f, z);
	}

	protected override IEnumerator OnStartMove()
	{
		yield return null;
	}

	public override void BackToPool()
	{
		MonoBehaviourSingleton<GameLogicUpdateManager>.Instance.RemoveUpdate(this);
		IsActivate = false;
		_collider.enabled = false;
		ClearList();
		base.BackToPool();
	}

	public void ClearList()
	{
		_ignoreList.Clear();
	}

	public override void Hit(Collider2D col)
	{
		Transform transform = col.transform;
		if (!CheckHitList(ref _ignoreList, transform))
		{
			_ignoreList.Add(transform);
			CaluDmg(BulletData, transform, 0f, 0.5f);
			HitCallback.CheckTargetToInvoke(transform);
		}
	}

	public override void UpdateBulletData(SKILL_TABLE pData, string owner = "", int nInRecordID = 0, int nNetID = 0, int nDirection = 1)
	{
		base.UpdateBulletData(pData, owner, nInRecordID, nNetID);
		ColliderType colliderType = this.colliderType;
		if (colliderType == ColliderType.CIRCLE || colliderType != ColliderType.BOX)
		{
			if ((bool)_circleCollider2D)
			{
				_circleCollider2D.radius = BulletData.f_RANGE;
			}
		}
		else if ((bool)_boxCollider2D)
		{
			string[] array = pData.s_FIELD.Split(',');
			float result;
			float.TryParse(array[3], out result);
			float result2;
			float.TryParse(array[4], out result2);
			_boxCollider2D.size = new Vector2(result, result2);
			_boxCollider2D.offset = new Vector2(result / 2f, 0f);
		}
	}

	public void SetOffset(Vector2 offset)
	{
		switch (colliderType)
		{
		case ColliderType.BOX:
			_boxCollider2D.offset = offset;
			break;
		case ColliderType.CIRCLE:
			_circleCollider2D.offset = offset;
			break;
		}
	}

	public void LogicUpdate()
	{
		if (GameLogicUpdateManager.GameFrame >= LogicTrigger)
		{
			BackToPool();
		}
	}

	private void OnDisable()
	{
		MonoBehaviourSingleton<GameLogicUpdateManager>.Instance.RemoveUpdate(this);
	}
}
