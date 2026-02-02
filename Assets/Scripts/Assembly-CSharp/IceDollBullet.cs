using UnityEngine;

public class IceDollBullet : BasicBullet, ILogicUpdate
{
	[SerializeField]
	protected ParticleSystem _iceDollModel;

	[SerializeField]
	protected float _springValue = 9.6f;

	[SerializeField]
	protected VInt _gravity = OrangeBattleUtility.FP_MaxGravity;

	protected int _direction;

	protected int _blockMask;

	protected RaycastHit2D[] _hits = new RaycastHit2D[5];

	private int nowLogicFrame;

	private int endLogicFrame;

	private int logicLength;

	private VInt timeDelta;

	private VInt3 nowPos;

	private VInt3 endPos;

	private Vector3 _speed = Vector3.zero;

	private float distanceDelta;

	protected override void Awake()
	{
		base.Awake();
		_blockMask = LayerMask.GetMask("Block", "SemiBlock");
	}

	private new void OnDisable()
	{
		MonoBehaviourSingleton<GameLogicUpdateManager>.Instance.RemoveUpdate(this);
	}

	public override void Active(Transform pTransform, Vector3 pDirection, LayerMask pTargetMask, IAimTarget pTarget = null)
	{
		base.Active(pTransform, pDirection, pTargetMask, pTarget);
		_transform.eulerAngles = new Vector3(0f, Vector2.SignedAngle(Vector2.right, pDirection), 0f);
		_capsuleCollider.direction = CapsuleDirection2D.Vertical;
		MonoBehaviourSingleton<GameLogicUpdateManager>.Instance.AddUpdate(this);
		CaluLogicFrame();
	}

	public override void Active(Vector3 pPos, Vector3 pDirection, LayerMask pTargetMask, IAimTarget pTarget = null)
	{
		base.Active(pPos, pDirection, pTargetMask, pTarget);
		_transform.eulerAngles = new Vector3(0f, Vector2.SignedAngle(Vector2.right, pDirection), 0f);
		_capsuleCollider.direction = CapsuleDirection2D.Vertical;
		MonoBehaviourSingleton<GameLogicUpdateManager>.Instance.AddUpdate(this);
		CaluLogicFrame();
	}

	protected void CaluLogicFrame()
	{
		float num = BulletData.f_DISTANCE / (float)BulletData.n_SPEED;
		logicLength = (int)(num / GameLogicUpdateManager.m_fFrameLen);
		timeDelta = new VInt(num / (float)logicLength);
		_direction = ((Direction.x > 0f) ? 1 : (-1));
		nowPos = new VInt3(_transform.localPosition);
		endPos = new VInt3(nowPos.vec3 + Vector3.right * ((float)_direction * BulletData.f_DISTANCE));
		_speed = new Vector3((endPos.vec3.x - nowPos.vec3.x) / num, 0f, 0f);
		nowLogicFrame = 0;
		endLogicFrame = nowLogicFrame + logicLength;
	}

	public void LogicUpdate()
	{
		if (!ManagedSingleton<StageHelper>.Instance.bEnemyActive)
		{
			return;
		}
		float f_DISTANCE = BulletData.f_DISTANCE;
		switch (Phase)
		{
		case BulletPhase.Normal:
			nowLogicFrame++;
			_speed.y += (float)_gravity.i * 0.001f * Time.deltaTime;
			nowPos += new VInt3(_speed * timeDelta.scalar);
			distanceDelta = Vector3.Distance(base.transform.localPosition, nowPos.vec3) * (Time.deltaTime / GameLogicUpdateManager.m_fFrameLen);
			if (nowLogicFrame > endLogicFrame)
			{
				if (BulletData.f_RANGE == 0f)
				{
					Phase = BulletPhase.Result;
				}
				else
				{
					SetPhaseToSplash();
				}
			}
			break;
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
		case BulletPhase.Boomerang:
			break;
		}
	}

	public override void LateUpdateFunc()
	{
		if (ManagedSingleton<StageHelper>.Instance.bEnemyActive && Phase == BulletPhase.Normal)
		{
			MoveBullet();
		}
	}

	protected override void MoveBullet()
	{
		if (_rigidbody2D != null)
		{
			_rigidbody2D.WakeUp();
		}
		oldPos = _transform.position;
		Vector3 vector = _speed.normalized * distanceDelta;
		int num = Physics2D.RaycastNonAlloc(_transform.position, Vector2.right * _direction, _hits, DefaultRadiusX + Mathf.Abs(vector.x) + 0.1f, _blockMask);
		for (int i = 0; i < num; i++)
		{
			if (!IsStageHurtObject(_hits[i].collider))
			{
				_direction *= -1;
				Direction.x *= -1f;
				_hitList.Clear();
				_speed.x *= -1f;
				nowPos.x = (int)((_transform.localPosition.x + (_hits[i].distance - DefaultRadiusX - 0.1f) * (float)_direction + _speed.x * timeDelta.scalar) * 1000f);
				_transform.eulerAngles = new Vector3(0f, Vector2.SignedAngle(Vector2.right, Direction), 0f);
				break;
			}
		}
		num = Physics2D.RaycastNonAlloc(_transform.position, Vector2.down, _hits, DefaultRadiusY + Mathf.Abs(vector.y) + 0.05f, _blockMask);
		for (int j = 0; j < num; j++)
		{
			if (!IsStageHurtObject(_hits[j].collider))
			{
				_speed.y = _springValue;
				nowPos.y = (int)((_transform.localPosition.y - _hits[j].distance + DefaultRadiusY + 0.05f + _speed.y * timeDelta.scalar) * 1000f);
				break;
			}
		}
		if (num != 0)
		{
			PlaySE("SkillSE_CH032_000", "ch032_icedoll01");
		}
		_transform.localPosition = Vector3.MoveTowards(_transform.localPosition, nowPos.vec3, distanceDelta);
	}

	public override void BackToPool()
	{
		base.BackToPool();
		MonoBehaviourSingleton<GameLogicUpdateManager>.Instance.RemoveUpdate(this);
	}

	protected new bool IsStageHurtObject(Collider2D collider)
	{
		if ((bool)collider)
		{
			if (collider.GetComponent<StageHurtObj>() != null)
			{
				return true;
			}
			return false;
		}
		return false;
	}
}
