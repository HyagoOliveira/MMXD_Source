using System;
using UnityEngine;

public class RandomReflectBullet : BasicBullet
{
	private int NextAngle;

	private int PreNextAngle;

	private int RandonSeed = 1;

	[SerializeField]
	private bool NeedPreDict = true;

	private Vector3 NextDir = Vector3.right;

	private float PreDictLength = 10f;

	private float PreDictTime = 1f;

	private Vector3 PreDictPos;

	private float TotalLength;

	private int TotalPreReflect;

	[SerializeField]
	private string PreTargetFx = "fxuseTarget";

	public override void Hit(Collider2D col)
	{
		if (CheckHitList(ref _hitList, col.transform) || Check_IS_DAMAGE_COUNT(col))
		{
			return;
		}
		int num = 1 << col.gameObject.layer;
		switch (Phase)
		{
		case BulletPhase.Normal:
		case BulletPhase.Boomerang:
			HitCheck(col);
			if ((num & (int)TargetMask) != 0)
			{
				isHitBlock = false;
			}
			else if (col.gameObject.GetComponent<StageHurtObj>() == null)
			{
				isHitBlock = true;
			}
			else
			{
				needWeaponImpactSE = (isHitBlock = false);
			}
			if (nThrough > 0 && (num & (int)TargetMask) != 0)
			{
				if (lastHit != null)
				{
					CaluDmg(BulletData, lastHit);
				}
				GenerateImpactFx();
				nThrough--;
				if (nThrough == 0)
				{
					Phase = BulletPhase.BackToPool;
				}
			}
			else if (nThrough > 0 && lastHit != null && lastHit.gameObject.GetComponent<StageHurtObj>() != null)
			{
				CaluDmg(BulletData, lastHit);
				GenerateImpactFx();
				nThrough--;
				if (nThrough == 0)
				{
					Phase = BulletPhase.BackToPool;
				}
			}
			else if (BulletData.f_RANGE == 0f)
			{
				Phase = BulletPhase.Result;
			}
			else
			{
				SetPhaseToSplash();
			}
			break;
		case BulletPhase.Splash:
			if ((num & (int)TargetMask) != 0 && !_hitList.Contains(col.transform))
			{
				_hitList.Add(col.transform);
				if (HitCallback != null)
				{
					HitCallback(col);
				}
			}
			break;
		case BulletPhase.Result:
			break;
		}
	}

	protected override void BulletReflect()
	{
		if (reflectCount <= 0 || Velocity.Equals(Vector3.zero))
		{
			hasReflect = false;
			return;
		}
		Vector3 vector = ((Velocity.x > 0f) ? Vector3.right : Vector3.left);
		Vector3 vector2 = _transform.TransformDirection(vector);
		bool flag = false;
		RaycastHit2D[] array = Physics2D.RaycastAll(reflectPoint, vector2, float.PositiveInfinity, 1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockLayer);
		for (int i = 0; i < array.Length; i++)
		{
			RaycastHit2D raycastHit2D = array[i];
			if (IsStageHurtObject(raycastHit2D.collider))
			{
				continue;
			}
			if (NeedPreDict && TotalPreReflect < reflectCount)
			{
				PreDictLength = ((BulletData.f_DISTANCE - TotalLength > raycastHit2D.distance) ? raycastHit2D.distance : (BulletData.f_DISTANCE - TotalLength));
				PreDictTime = PreDictLength / (float)BulletData.n_SPEED;
				NextDir = vector2;
				PreDictPos = reflectPoint;
				while (TotalLength < BulletData.f_DISTANCE && TotalPreReflect < reflectCount && CalcuRoute())
				{
				}
			}
			reflectCount--;
			reflectPoint = raycastHit2D.point + raycastHit2D.normal * (_colliderSize.x * 0.5f);
			Vector3 toDirection = Quaternion.Euler(0f, 0f, GetNextAngle(RandonSeed)) * raycastHit2D.normal;
			reflectRotation = Quaternion.FromToRotation(vector, toDirection) * BulletQuaternion;
			lastReflectTrans = raycastHit2D.transform;
			hasReflect = true;
			flag = true;
			break;
		}
		if (!flag)
		{
			reflectCount = 0;
			hasReflect = false;
		}
	}

	private bool CalcuRoute()
	{
		RaycastHit2D[] array = Physics2D.RaycastAll(PreDictPos, NextDir, float.PositiveInfinity, 1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockLayer);
		for (int i = 0; i < array.Length; i++)
		{
			RaycastHit2D raycastHit2D = array[i];
			if (!IsStageHurtObject(raycastHit2D.collider))
			{
				PreDictLength = ((BulletData.f_DISTANCE - TotalLength > raycastHit2D.distance) ? raycastHit2D.distance : (BulletData.f_DISTANCE - TotalLength));
				TotalLength += PreDictLength;
				TotalPreReflect++;
				MonoBehaviourSingleton<FxManager>.Instance.PlayReturn<psSwingTarget>(PreTargetFx, PreDictPos, Quaternion.Euler(0f, 0f, Vector2.SignedAngle(Vector3.right, NextDir)), Array.Empty<object>()).SetEffect(PreDictLength, new Color(0.6f, 0f, 0.5f, 0.7f), new Color(0.6f, 0f, 0.5f), PreDictTime, 0.5f);
				Vector3 nextDir = Quaternion.Euler(0f, 0f, GetPreNextAngle(RandonSeed)) * raycastHit2D.normal;
				NextDir = nextDir;
				PreDictPos = raycastHit2D.point + raycastHit2D.normal * (_colliderSize.x * 0.5f);
				return true;
			}
		}
		return false;
	}

	public override void OnStart()
	{
		NextAngle = 0;
		PreNextAngle = 0;
		TotalPreReflect = 0;
		TotalLength = 0f;
		base.OnStart();
	}

	public void SetSeed(int seed)
	{
		RandonSeed = seed;
	}

	private int GetNextAngle(int seed)
	{
		NextAngle = ((NextAngle + 147) * 369 * seed + 258) % 180;
		if (NextAngle < 0)
		{
			NextAngle *= -1;
		}
		PreNextAngle = NextAngle;
		return NextAngle - 90;
	}

	private int GetPreNextAngle(int seed)
	{
		PreNextAngle = ((PreNextAngle + 147) * 369 * seed + 258) % 180;
		if (PreNextAngle < 0)
		{
			PreNextAngle *= -1;
		}
		return PreNextAngle - 90;
	}
}
