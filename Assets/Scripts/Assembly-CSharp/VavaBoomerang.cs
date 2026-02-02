using UnityEngine;

public class VavaBoomerang : ShingetsurinBullet
{
	public Transform model;

	private Vector3 defaultModelScale;

	protected override void Awake()
	{
		base.Awake();
		defaultModelScale = model.localScale;
	}

	public override void LogicUpdate()
	{
		if (!ManagedSingleton<StageHelper>.Instance.bEnemyActive)
		{
			return;
		}
		float f_DISTANCE = BulletData.f_DISTANCE;
		switch (Phase)
		{
		case BulletPhase.Normal:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				nowLogicFrame++;
				nowPos += new VInt3(_speed * timeDelta.scalar);
				distanceDelta = Vector3.Distance(base.transform.localPosition, nowPos.vec3) * (Time.deltaTime / GameLogicUpdateManager.m_fFrameLen);
				if (FreeDISTANCE > 0f)
				{
					float freeDISTANCE = FreeDISTANCE;
				}
				if (nowLogicFrame > endLogicFrame || CheckHitTarget())
				{
					_bClearHit = false;
					nowLogicFrame = 0;
					endLogicFrame = (int)(1.5f / GameLogicUpdateManager.m_fFrameLen);
					_subStatus = SubStatus.Phase1;
				}
				break;
			case SubStatus.Phase1:
				nowLogicFrame++;
				_capsuleCollider.offset = Vector2.zero;
				if (Target != null)
				{
					StageObjBase stageObjBase = Target as StageObjBase;
					if (stageObjBase != null && (int)stageObjBase.Hp <= 0)
					{
						Target = null;
					}
				}
				FindTarget();
				if (!_bClearHit && (float)nowLogicFrame > 0.5f / GameLogicUpdateManager.m_fFrameLen)
				{
					_bClearHit = true;
					_hitList.Clear();
				}
				else
				{
					if (nowLogicFrame <= endLogicFrame)
					{
						break;
					}
					if (Target == null)
					{
						if (Vector3.Distance(_transform.position, MasterTransform.position) >= 0.5f)
						{
							Vector3 vector = -(_transform.position - MasterTransform.position);
							vector.z = 0f;
							Direction = ((vector == Vector3.zero) ? Direction : vector.normalized);
						}
						_transform.eulerAngles = new Vector3(0f, 0f, Vector2.SignedAngle(Vector2.right, Direction));
					}
					else
					{
						if (Vector3.Distance(Target.AimTransform.position + Target.AimPoint, _transform.position) >= 0.5f)
						{
							Vector3 vector2 = Target.AimTransform.position + Target.AimPoint - _transform.position;
							vector2.z = 0f;
							Direction = ((vector2 == Vector3.zero) ? Direction : vector2.normalized);
						}
						_transform.eulerAngles = new Vector3(0f, 0f, Vector2.SignedAngle(Vector2.right, Direction));
					}
					Phase = BulletPhase.Result;
				}
				break;
			}
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
				if (_hitList.Count != 0 || BulletData.f_RANGE != 0f)
				{
					GenerateImpactFx();
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
		model.rotation = Quaternion.Euler(0f, 90f, 0f);
		if (Direction.x < 0f)
		{
			model.localScale = new Vector3(defaultModelScale.x, defaultModelScale.y, 0f - defaultModelScale.z);
		}
		else
		{
			model.localScale = new Vector3(defaultModelScale.x, defaultModelScale.y, defaultModelScale.z);
		}
	}
}
