using CallbackDefs;
using UnityEngine;

public class EM195_Controller : EnemyControllerBase, IManagedUpdateBehavior
{
	private enum WallSide
	{
		Up = 1,
		Down = 2,
		Left = 3,
		Right = 4,
		None = 5
	}

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

	private WallSide _wallside = WallSide.Down;

	private float DelayFrame;

	[SerializeField]
	private float VibrationDis = 0.08f;

	private int VDirection = 1;

	[SerializeField]
	private float VibrationTime = 0.5f;

	private int FallFrame;

	private float DistanceX = 0.6f;

	[Header("")]
	[SerializeField]
	private float LifeTime = 3f;

	private int LifeFrame;

	private bool hasSetWall;

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
		{
			WallSide wallside = _wallside;
			if (wallside == WallSide.Up)
			{
				if (GameLogicUpdateManager.GameFrame > LifeFrame)
				{
					SetStatus(MainStatus.Fall);
				}
			}
			else if (GameLogicUpdateManager.GameFrame > LifeFrame)
			{
				Suicide();
			}
			break;
		}
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
		ModelTransform.localScale = new Vector3(ModelTransform.localScale.x, ModelTransform.localScale.y, Mathf.Abs(ModelTransform.localScale.z));
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
			_collideBullet.Active(targetMask);
			ModelTransform.localScale = Vector3.one * 0.3f;
			SetStatus(MainStatus.Idle);
			IgnoreGravity = true;
			IgnoreGlobalVelocity = true;
			if (!hasSetWall)
			{
				SetWallSide();
			}
			LifeFrame = GameLogicUpdateManager.GameFrame + (int)(LifeTime * 20f);
		}
		else
		{
			_collideBullet.BackToPool();
			hasSetWall = false;
		}
	}

	public override void SetPositionAndRotation(Vector3 pos, bool bBack)
	{
		_transform.localScale = new Vector3(_transform.localScale.x, _transform.localScale.y, _transform.localScale.z);
		_transform.position = pos;
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
		Vector3 position = _transform.position;
		position.x += Controller.Collider2D.offset.x * base.transform.localScale.x;
		position.y += Controller.Collider2D.offset.y * base.transform.localScale.y;
		return position;
	}

	private bool CheckHitBlock()
	{
		Vector2 point = GetCenterPos() + Vector3.down * 0.5f;
		Vector2 size = Controller.Collider2D.size * 0.5f;
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

	public void SetWallSide(int wallside = 0)
	{
		int layerMask = (1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockLayer) | (1 << ManagedSingleton<OrangeLayerManager>.Instance.SemiBlockLayer);
		Vector3 euler = Vector3.zero;
		switch ((WallSide)wallside)
		{
		case WallSide.Up:
		{
			euler = new Vector3(90f, 90f, 0f);
			_wallside = (WallSide)wallside;
			Controller.Collider2D.offset = new Vector2(0f, -0.3f);
			RaycastHit2D raycastHit2D = Physics2D.Raycast(_transform.position, Vector2.up, 10f, layerMask);
			_transform.position = raycastHit2D.point;
			break;
		}
		case WallSide.Down:
		{
			euler = new Vector3(-90f, 90f, 0f);
			_wallside = (WallSide)wallside;
			Controller.Collider2D.offset = new Vector2(0f, 0.3f);
			RaycastHit2D raycastHit2D2 = Physics2D.Raycast(_transform.position, Vector2.down, 10f, layerMask);
			_transform.position = raycastHit2D2.point;
			break;
		}
		case WallSide.Left:
		{
			euler = new Vector3(0f, 90f, 0f);
			_wallside = (WallSide)wallside;
			Controller.Collider2D.offset = new Vector2(0.3f, 0f);
			RaycastHit2D raycastHit2D3 = Physics2D.Raycast(_transform.position, Vector2.left, 10f, layerMask);
			_transform.position = raycastHit2D3.point;
			break;
		}
		case WallSide.Right:
		{
			euler = new Vector3(0f, -90f, 0f);
			_wallside = (WallSide)wallside;
			Controller.Collider2D.offset = new Vector2(-0.3f, 0f);
			RaycastHit2D raycastHit2D4 = Physics2D.Raycast(_transform.position, Vector2.right, 10f, layerMask);
			_transform.position = raycastHit2D4.point;
			break;
		}
		default:
		{
			RaycastHit2D raycastHit2D = Physics2D.Raycast(_transform.position, Vector2.up, 3f, layerMask);
			RaycastHit2D raycastHit2D2 = Physics2D.Raycast(_transform.position, Vector2.down, 3f, layerMask);
			RaycastHit2D raycastHit2D3 = Physics2D.Raycast(_transform.position, Vector2.left, 3f, layerMask);
			RaycastHit2D raycastHit2D4 = Physics2D.Raycast(_transform.position, Vector2.right, 3f, layerMask);
			float num = 5f;
			if ((bool)raycastHit2D && num > raycastHit2D.distance)
			{
				num = raycastHit2D.distance;
				_wallside = WallSide.Up;
				euler = new Vector3(90f, 90f, 0f);
				Controller.Collider2D.offset = new Vector2(0f, -0.3f);
				_transform.position = raycastHit2D.point;
			}
			if ((bool)raycastHit2D2 && num > raycastHit2D2.distance)
			{
				num = raycastHit2D2.distance;
				_wallside = WallSide.Down;
				euler = new Vector3(-90f, 90f, 0f);
				Controller.Collider2D.offset = new Vector2(0f, 0.3f);
				_transform.position = raycastHit2D2.point;
			}
			if ((bool)raycastHit2D3 && num > raycastHit2D3.distance)
			{
				num = raycastHit2D3.distance;
				_wallside = WallSide.Left;
				euler = new Vector3(0f, 90f, 0f);
				Controller.Collider2D.offset = new Vector2(0.3f, 0f);
				_transform.position = raycastHit2D3.point;
			}
			if ((bool)raycastHit2D4 && num > raycastHit2D4.distance)
			{
				num = raycastHit2D4.distance;
				_wallside = WallSide.Right;
				euler = new Vector3(0f, -90f, 0f);
				Controller.Collider2D.offset = new Vector2(-0.3f, 0f);
				_transform.position = raycastHit2D4.point;
			}
			break;
		}
		}
		Controller.LogicPosition = new VInt3(_transform.position);
		ModelTransform.rotation = Quaternion.Euler(euler);
		_collideBullet.UpdateBulletData(EnemyWeapons[0].BulletData);
		_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
		_collideBullet.Active(targetMask);
		hasSetWall = true;
	}
}
