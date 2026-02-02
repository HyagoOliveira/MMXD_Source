using System;
using System.Collections.Generic;
using UnityEngine;

public class LocateBullet : BasicBullet
{
	private bool hasTurn;

	private bool hasStop;

	private Vector3 EndPos;

	private float MoveDis;

	[SerializeField]
	[Tooltip("存活時間")]
	private float LifeTime = 3f;

	private int LifeFrame;

	[SerializeField]
	private bool CanMultiHit;

	[SerializeField]
	private Transform RocketTarget;

	[SerializeField]
	private SpriteRenderer PredictSpriteRenderer;

	private int HitFrame;

	public bool isHasArrived
	{
		get
		{
			return hasStop;
		}
	}

	public override void Active(Transform pTransform, Vector3 pDirection, LayerMask pTargetMask, IAimTarget pTarget = null)
	{
		base.Active(pTransform, pDirection, pTargetMask, pTarget);
		hasTurn = false;
		hasStop = false;
		EndPos = pTransform.position + Vector3.up * 3f;
	}

	public override void Active(Vector3 pPos, Vector3 pDirection, LayerMask pTargetMask, IAimTarget pTarget = null)
	{
		base.Active(pPos, pDirection, pTargetMask, pTarget);
		hasTurn = false;
		hasStop = false;
		EndPos = pPos + Vector3.up * 3f;
	}

	public override void BackToPool()
	{
		base.BackToPool();
	}

	public override void Hit(Collider2D col)
	{
		base.Hit(col);
	}

	public override void LateUpdateFunc()
	{
		if (!ManagedSingleton<StageHelper>.Instance.bEnemyActive)
		{
			return;
		}
		switch (Phase)
		{
		case BulletPhase.Normal:
		{
			UpdateExtraCollider();
			MoveBullet();
			float num = BulletData.f_DISTANCE;
			if (!hasTurn && FreeDISTANCE > 0f)
			{
				num = FreeDISTANCE;
			}
			if (hasTurn)
			{
				num = MoveDis;
			}
			if (BulletData.n_SHOTLINE == 6)
			{
				heapDistance = lineDistance;
			}
			else
			{
				heapDistance += Vector2.Distance(lastPosition, _transform.position);
			}
			lastPosition = _transform.position;
			if (heapDistance > num)
			{
				if (!hasTurn && !hasStop)
				{
					hasTurn = true;
					heapDistance = 0f;
					MoveDis = Vector2.Distance(_transform.position, EndPos);
					Direction = EndPos - _transform.position;
					_transform.eulerAngles = new Vector3(0f, 0f, Vector2.SignedAngle(Vector2.right, Direction));
				}
				else if (!hasStop)
				{
					hasStop = true;
					Velocity = Vector3.zero;
					LifeFrame = GameLogicUpdateManager.GameFrame + (int)(LifeTime * 20f);
				}
			}
			break;
		}
		case BulletPhase.Splash:
			PhaseSplash();
			break;
		case BulletPhase.Result:
			if (BulletData.n_THROUGH == 0)
			{
				foreach (Transform hit in _hitList)
				{
					CaluDmg(BulletData, hit);
					if (nThrough > 0)
					{
						nThrough--;
					}
				}
			}
			if (BulletData.n_TYPE != 3)
			{
				if (_hitList.Count != 0 && BulletData.f_RANGE != 0f)
				{
					needPlayEndSE = true;
					GenerateEndFx();
				}
				else
				{
					GenerateEndFx();
				}
			}
			Phase = BulletPhase.BackToPool;
			break;
		case BulletPhase.BackToPool:
			Stop();
			BackToPool();
			break;
		}
		TrackingTarget();
		if (hasStop && GameLogicUpdateManager.GameFrame > LifeFrame && Phase != BulletPhase.BackToPool && !bIsEnd)
		{
			Phase = BulletPhase.Result;
		}
	}

	public override void OnTriggerHit(Collider2D col)
	{
		if (!ManagedSingleton<StageHelper>.Instance.bEnemyActive || MonoBehaviourSingleton<OrangeGameManager>.Instance.IsGamePause || col.isTrigger || ((1 << col.gameObject.layer) & (int)UseMask) == 0 || (((uint)BulletData.n_FLAG & (true ? 1u : 0u)) != 0 && ((1 << col.gameObject.layer) & (int)BlockMask) != 0 && !col.GetComponent<StageHurtObj>()))
		{
			return;
		}
		StageObjParam component = col.GetComponent<StageObjParam>();
		if (component != null && component.tLinkSOB != null)
		{
			if (((1 << col.gameObject.layer) & (int)BlockMask) == 0 && (int)component.tLinkSOB.Hp > 0)
			{
				Hit(col);
			}
		}
		else
		{
			Hit(col);
		}
	}

	public void SetEndPos(Vector3 endpos)
	{
		EndPos = endpos;
	}

	public void SetBackToPool()
	{
		Stop();
		BackToPool();
	}

	protected override bool CheckHitList(ref HashSet<Transform> hitList, Transform newHit)
	{
		if (hitList.Contains(newHit))
		{
			if (CanMultiHit)
			{
				if (GameLogicUpdateManager.GameFrame > HitFrame)
				{
					HitFrame = GameLogicUpdateManager.GameFrame + 10;
					nThrough++;
					return false;
				}
				return true;
			}
			return true;
		}
		StageObjParam component = newHit.GetComponent<StageObjParam>();
		if ((bool)component)
		{
			OrangeCharacter orangeCharacter = component.tLinkSOB as OrangeCharacter;
			if ((bool)orangeCharacter)
			{
				CharacterControlBase component2 = orangeCharacter.GetComponent<CharacterControlBase>();
				if ((bool)component2)
				{
					foreach (Transform hit in hitList)
					{
						if (component2.CheckMyShield(hit))
						{
							return true;
						}
					}
				}
			}
		}
		else
		{
			PlayerCollider component3 = newHit.GetComponent<PlayerCollider>();
			if (component3 != null && component3.IsDmgReduceShield())
			{
				Transform dmgReduceOwnerTransform = component3.GetDmgReduceOwnerTransform();
				if (hitList.Contains(dmgReduceOwnerTransform))
				{
					return true;
				}
			}
		}
		if (CanMultiHit)
		{
			HitFrame = GameLogicUpdateManager.GameFrame + 10;
		}
		return false;
	}

	protected override void GenerateEndFx(bool bPlaySE = true)
	{
		if (bPlaySE)
		{
			PlaySE(_HitGuardSE[0], _HitGuardSE[1], isForceSE);
		}
		if (!(FxEnd == "") && FxEnd != null)
		{
			Quaternion quaternion = Quaternion.FromToRotation(Vector3.back, Direction);
			RaycastHit2D raycastHit2D = Physics2D.Raycast(oldPos, Direction, offset);
			if ((bool)raycastHit2D)
			{
				_transform.position = raycastHit2D.point;
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FxEnd, raycastHit2D.point, quaternion * BulletQuaternion, Array.Empty<object>());
			}
			else
			{
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FxEnd, _transform.position, quaternion * BulletQuaternion, Array.Empty<object>());
			}
		}
	}
}
