using CallbackDefs;
using UnityEngine;

public class EM140_Controller : EnemyControllerBase, IManagedUpdateBehavior
{
	private enum MainStatus
	{
		Idle = 0,
		Fall = 1
	}

	private enum SubStatus
	{
		Phase0 = 0,
		Phase1 = 1,
		Phase2 = 2,
		Phase3 = 3,
		MAX_SUBSTATUS = 4
	}

	private MainStatus _mainStatus;

	private SubStatus _subStatus;

	private float DelayFrame;

	[SerializeField]
	private float VibrationDis = 0.08f;

	private int VDirection = 1;

	[SerializeField]
	private float VibrationTime = 0.5f;

	private int FallFrame;

	private float DistanceX = 0.6f;

	private void OnEnable()
	{
		MonoBehaviourSingleton<UpdateManager>.Instance.AddUpdate(this);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		MonoBehaviourSingleton<UpdateManager>.Instance.RemoveUpdate(this);
	}

	protected override void Awake()
	{
		base.Awake();
		Transform[] target = _transform.GetComponentsInChildren<Transform>(true);
		ModelTransform = OrangeBattleUtility.FindChildRecursive(ref target, "model", true);
		_collideBullet = OrangeBattleUtility.FindChildRecursive(ref target, "Collider").gameObject.AddOrGetComponent<CollideBullet>();
		OrangeBattleUtility.AddEnemyAutoAimSystem(_transform, out _enemyAutoAimSystem);
		_enemyAutoAimSystem.SetColliderType(EnemyAutoAimSystem.ColliderType.Box);
		_enemyAutoAimSystem.UpdateAimRange(5f, 10f);
		StageObjParam[] componentsInChildren = GetComponentsInChildren<StageObjParam>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].tLinkSOB = null;
		}
		base.AllowAutoAim = false;
	}

	public override void UpdateStatus(int nSet, string smsg, Callback tCB = null)
	{
		bWaitNetStatus = false;
	}

	private void SetStatus(MainStatus mainStatus, SubStatus subStatus = SubStatus.Phase0)
	{
		_mainStatus = mainStatus;
		_subStatus = subStatus;
		switch (_mainStatus)
		{
		case MainStatus.Idle:
			IgnoreGravity = true;
			break;
		case MainStatus.Fall:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				IgnoreGravity = true;
				_velocity = VInt3.zero;
				FallFrame = GameLogicUpdateManager.GameFrame + (int)(VibrationTime * 20f);
				break;
			case SubStatus.Phase1:
				IgnoreGravity = false;
				break;
			}
			break;
		}
		AiTimer.TimerStart();
	}

	public override void LogicUpdate()
	{
		if (!Activate || (float)GameLogicUpdateManager.GameFrame < DelayFrame)
		{
			return;
		}
		base.LogicUpdate();
		switch (_mainStatus)
		{
		case MainStatus.Idle:
			Target = _enemyAutoAimSystem.GetClosetPlayer();
			if ((bool)Target)
			{
				SetStatus(MainStatus.Fall);
			}
			break;
		case MainStatus.Fall:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (GameLogicUpdateManager.GameFrame > FallFrame)
				{
					SetStatus(MainStatus.Fall, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if (CheckHitBlock())
				{
					Suicide();
				}
				break;
			}
			break;
		}
	}

	private void UpdateDirection(int forceDirection = 0)
	{
		int direction2 = base.direction;
		if (forceDirection != 0)
		{
			base.direction = forceDirection;
		}
		ModelTransform.localScale = new Vector3(ModelTransform.localScale.x, ModelTransform.localScale.y, Mathf.Abs(ModelTransform.localScale.z) * (float)base.direction);
	}

	public void UpdateFunc()
	{
		if (Activate && !((float)GameLogicUpdateManager.GameFrame < DelayFrame))
		{
			base.transform.localPosition = Vector3.MoveTowards(base.transform.localPosition, Controller.LogicPosition.vec3, distanceDelta);
			if (_mainStatus == MainStatus.Fall && _subStatus == SubStatus.Phase0)
			{
				ModelTransform.position = new Vector3((float)VDirection * VibrationDis, 0f, 0f) + ModelTransform.position;
				VDirection *= -1;
			}
		}
	}

	public override void SetActive(bool isActive)
	{
		base.SetActive(isActive);
		bDeadShock = false;
		if (isActive)
		{
			DelayFrame = GameLogicUpdateManager.GameFrame + 30;
			_collideBullet.UpdateBulletData(EnemyWeapons[0].BulletData);
			_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			_collideBullet.HitCallback = IceHit;
			_collideBullet.Active(targetMask);
			ModelTransform.localScale = new Vector3(1.3f, 1.2f, 1.3f);
			SetStatus(MainStatus.Idle);
			IgnoreGravity = true;
		}
		else
		{
			ModelTransform.eulerAngles = new Vector3(0f, -90f, 0f);
			_collideBullet.BackToPool();
		}
	}

	public override void SetPositionAndRotation(Vector3 pos, bool bBack)
	{
		if (bBack)
		{
			base.direction = -1;
		}
		else
		{
			base.direction = 1;
		}
		base.transform.localScale = new Vector3(_transform.localScale.x, _transform.localScale.y, _transform.localScale.z);
		base.transform.position = pos;
	}

	private void IceHit(object obj)
	{
		if (obj != null)
		{
			Suicide();
		}
	}

	private Vector3 GetCenterPos()
	{
		Vector3 position = base.transform.position;
		position.x += Controller.Collider2D.offset.x * base.transform.localScale.x;
		position.y += Controller.Collider2D.offset.y * base.transform.localScale.y;
		return position;
	}

	private bool CheckHitBlock()
	{
		Vector2 point = GetCenterPos();
		Vector2 size = Controller.Collider2D.size * 0.8f;
		if ((bool)Physics2D.OverlapBox(point, size, 0f, LayerMask.GetMask("Block")))
		{
			return true;
		}
		return false;
	}

	private void Suicide()
	{
		Hp = 0;
		Hurt(new HurtPassParam());
	}
}
