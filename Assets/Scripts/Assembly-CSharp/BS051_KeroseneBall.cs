using System;
using UnityEngine;

public class BS051_KeroseneBall : ParabolaBulletForTryShoot
{
	protected bool isHitGround;

	protected bool isHitCeiling;

	private VInt3 nextPos;

	public override void LogicUpdate()
	{
		nowLogicFrame++;
		if (bulletState != 0)
		{
			int num = 1;
			return;
		}
		Gravity.y = g * ((float)nowLogicFrame * timeDelta.scalar);
		nextPos = nowPos;
		nextPos += new VInt3((speed + Gravity) * timeDelta.scalar);
		VInt2 vInt = new VInt2(nextPos.x - nowPos.x, nextPos.y - nowPos.y);
		float distance = Vector2.Distance(nextPos.vec3, nowPos.vec3);
		RaycastHit2D raycastHit2D = Physics2D.Raycast(nowPos.vec3, vInt.vec2, distance, BlockMask);
		if ((bool)raycastHit2D)
		{
			Vector2 normal = raycastHit2D.normal;
			Direction = -normal;
			if (Vector2.Angle(normal, Vector2.up) < 10f)
			{
				isHitGround = true;
			}
			if (reflectCount > 0 && !isHitGround)
			{
				reflectCount--;
				Vector2 vector = new Vector2(Mathf.Abs(speed.x), Mathf.Abs(speed.y));
				Vector2 vector2 = speed + vector * normal * 2f;
				SetSpeed(vector2);
				nextPos = nowPos + new VInt3((speed + Gravity) * timeDelta.scalar);
			}
			OnTriggerHit(raycastHit2D.collider);
		}
		nowPos = nextPos;
		distanceDelta = Vector3.Distance(base.transform.localPosition, nowPos.vec3) * (Time.deltaTime / GameLogicUpdateManager.m_fFrameLen);
	}

	public override void UpdateFunc()
	{
		base.UpdateFunc();
		if (isHitGround)
		{
			BackToPool();
		}
	}

	public override void BackToPool()
	{
		if (isHitBlock && reflectCount > 0 && !isHitGround)
		{
			Phase = BulletPhase.Normal;
			return;
		}
		MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FxEnd, _transform.position, Quaternion.identity * BulletQuaternion, Array.Empty<object>());
		isHitGround = false;
		base.BackToPool();
	}

	protected override void HitCheck(Collider2D col)
	{
		base.HitCheck(col);
	}

	protected override void GenerateImpactFx(bool bPlaySE = true)
	{
		Quaternion quaternion = Quaternion.FromToRotation(Vector3.back, Direction);
		RaycastHit2D raycastHit2D = Physics2D.Raycast(oldPos, Direction, offset);
		if ((bool)raycastHit2D)
		{
			_transform.position = raycastHit2D.point;
			if (isHitGround)
			{
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FxEnd, raycastHit2D.point, quaternion * BulletQuaternion, Array.Empty<object>());
			}
			else
			{
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FxImpact, raycastHit2D.point, quaternion * BulletQuaternion, Array.Empty<object>());
			}
		}
		else if (isHitGround)
		{
			MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FxEnd, raycastHit2D.point, quaternion * BulletQuaternion, Array.Empty<object>());
		}
		else
		{
			MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FxImpact, raycastHit2D.point, quaternion * BulletQuaternion, Array.Empty<object>());
		}
		if (isHitBlock || needPlayEndSE || needWeaponImpactSE)
		{
			PlaySE(_HitGuardSE[0], _HitGuardSE[1], isForceSE);
		}
	}
}
