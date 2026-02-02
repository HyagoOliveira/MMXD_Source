using UnityEngine;

public class EM132_Controller : EnemyControllerBase, IManagedUpdateBehavior
{
	private enum MainStatus
	{
		Init = 0,
		Reset = 1,
		FlyIn = 2,
		FlyOut = 3
	}

	public float InitAimRangeX = 10f;

	public float InitAimRangeY = 100f;

	public float ResetRange = 50f;

	public float TargetPosShiftX = 3f;

	public float TargetPosShiftY = 3f;

	public int LogicFrameFly = 25;

	public int LogicFrameRotateBornStart = 25;

	public int LogicFrameRotateBornEnd = 10;

	public int LogicFrameRotateAttack = 5;

	public float MoveSpeedDiffX = 0.02f;

	public float MoveSpeedDiffY = 0.1f;

	[Range(0.5f, 1.5f)]
	public float MoveSpeedAccX = 0.8f;

	[Range(0.5f, 1.5f)]
	public float MoveSpeedAccY = 1f;

	private Vector2 _moveSpeedDiff;

	private Vector2 _moveSpeedAcc;

	private Vector2 _posFlyShift;

	private Vector2 _moveSpeedOrigin;

	private float _aimRangeOrigin;

	private MainStatus _mainStatus;

	private VInt3 _targetPos = VInt3.zero;

	private Vector2 _moveSpeed = Vector2.zero;

	private int _logicFrame;

	private Transform _modelTransform;

	private Quaternion _modelRotationOrigin;

	private Transform _shootPointTransform;

	private VInt3 _posOrigin;

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
		IgnoreGravity = true;
		_animator = GetComponentInChildren<Animator>();
		_collideBullet = base.gameObject.AddOrGetComponent<CollideBullet>();
		Transform[] target = _transform.GetComponentsInChildren<Transform>(true);
		_modelTransform = OrangeBattleUtility.FindChildRecursive(ref target, "model", true);
		_modelRotationOrigin = _modelTransform.rotation;
		base.AimTransform = _modelTransform;
		_shootPointTransform = OrangeBattleUtility.FindChildRecursive(ref target, "ShootPoint", true);
		OrangeBattleUtility.AddEnemyAutoAimSystem(_transform, out _enemyAutoAimSystem);
		_enemyAutoAimSystem.UpdateAimRange(InitAimRangeX, InitAimRangeY);
		_modelTransform.gameObject.SetLayer(ManagedSingleton<OrangeLayerManager>.Instance.RenderSPEnemy, true);
	}

	public override void LogicUpdate()
	{
		if (!Activate)
		{
			return;
		}
		if (_logicFrame > 0)
		{
			_logicFrame--;
		}
		BaseLogicUpdate();
		switch (_mainStatus)
		{
		case MainStatus.Init:
			if (UpdateTargetPos())
			{
				ResetPos();
				SetStatus(MainStatus.FlyIn, LogicFrameFly);
			}
			break;
		case MainStatus.Reset:
		{
			OrangeCharacter character;
			if (GetClosetPlayer(out character) && (character.Controller.LogicPosition - _posOrigin).vec3.magnitude <= ResetRange && UpdateTargetPos())
			{
				SetStatus(MainStatus.FlyIn, LogicFrameFly);
			}
			break;
		}
		case MainStatus.FlyIn:
			if (_logicFrame == 0)
			{
				SetStatus(MainStatus.FlyOut, LogicFrameFly);
				break;
			}
			if (_logicFrame > LogicFrameRotateBornEnd && _logicFrame <= LogicFrameRotateBornStart)
			{
				_modelTransform.Rotate(0f, 0f, 180f / (float)(LogicFrameRotateBornStart - LogicFrameRotateBornEnd));
			}
			else if (_logicFrame <= LogicFrameRotateAttack)
			{
				_modelTransform.Rotate(-90f / (float)LogicFrameRotateAttack, 0f, 0f);
			}
			_moveSpeed *= _moveSpeedAcc;
			break;
		case MainStatus.FlyOut:
			if (_logicFrame == 0)
			{
				SetStatus(MainStatus.Reset);
				break;
			}
			if (_logicFrame > LogicFrameFly - LogicFrameRotateAttack)
			{
				_modelTransform.Rotate(-90f / (float)LogicFrameRotateAttack, 0f, 0f);
			}
			_moveSpeed /= _moveSpeedAcc;
			break;
		}
		Controller.LogicPosition += new VInt3(_moveSpeed);
		distanceDelta = Vector3.Distance(_transform.position, Controller.LogicPosition.vec3) * (Time.deltaTime / GameLogicUpdateManager.m_fFrameLen);
	}

	private bool UpdateTargetPos()
	{
		OrangeCharacter character;
		if (GetClosetPlayer(out character))
		{
			_targetPos = character.Controller.LogicPosition + new VInt3(new Vector2(TargetPosShiftX, TargetPosShiftY));
			return true;
		}
		return false;
	}

	public void UpdateFunc()
	{
		_transform.position = Vector3.MoveTowards(_transform.position, Controller.LogicPosition.vec3, distanceDelta);
	}

	public override void SetActive(bool isActive)
	{
		base.SetActive(isActive);
		_animator.enabled = isActive;
		if (isActive)
		{
			_collideBullet.UpdateBulletData(EnemyWeapons[0].BulletData);
			_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			_collideBullet.Active(targetMask);
			SetStatus(MainStatus.Init);
		}
		else
		{
			_collideBullet.BackToPool();
		}
	}

	private void SetStatus(MainStatus mainStatus, int logicFrame = 0)
	{
		_mainStatus = mainStatus;
		_logicFrame = logicFrame;
		switch (mainStatus)
		{
		case MainStatus.Init:
			_moveSpeed = Vector2.zero;
			ResetSpeedParam();
			break;
		case MainStatus.Reset:
			_moveSpeed = Vector2.zero;
			ResetPos();
			_enemyAutoAimSystem.UpdateAimRange(_aimRangeOrigin, _aimRangeOrigin);
			break;
		case MainStatus.FlyIn:
			_moveSpeed = -_moveSpeedOrigin;
			Controller.LogicPosition = _targetPos + new VInt3(_posFlyShift);
			break;
		case MainStatus.FlyOut:
			ShootAimBullet();
			_moveSpeed = new Vector2(_moveSpeedDiff.x, 0f - _moveSpeedDiff.y);
			_targetPos += new VInt3(new Vector2(_posFlyShift.x, 0f - _posFlyShift.y));
			break;
		}
	}

	private void ResetPos()
	{
		Controller.LogicPosition = _targetPos + new VInt3(_posFlyShift);
		_transform.position = Controller.LogicPosition.vec3;
		_modelTransform.rotation = _modelRotationOrigin;
		_modelTransform.Rotate(180f, 0f, 0f);
	}

	private void ShootAimBullet()
	{
		Vector3 pDirection = Vector3.right;
		OrangeCharacter character;
		if (GetClosetPlayer(out character))
		{
			pDirection = (character.Controller.LogicPosition - Controller.LogicPosition).vec3;
		}
		BulletBase.TryShotBullet(EnemyWeapons[1].BulletData, _shootPointTransform, pDirection, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
	}

	private bool GetClosetPlayer(out OrangeCharacter character)
	{
		character = _enemyAutoAimSystem.GetClosetPlayer();
		return character != null;
	}

	private void ResetSpeedParam()
	{
		_posOrigin = Controller.LogicPosition;
		_moveSpeedDiff = new Vector2(MoveSpeedDiffX, MoveSpeedDiffY);
		_moveSpeedAcc = new Vector2(MoveSpeedAccX, MoveSpeedAccY);
		_posFlyShift = Vector2.zero;
		_moveSpeedOrigin = _moveSpeedDiff;
		for (int i = 0; i < LogicFrameFly; i++)
		{
			_posFlyShift += _moveSpeedOrigin;
			_moveSpeedOrigin /= _moveSpeedAcc;
		}
		_posFlyShift += _moveSpeedOrigin;
		_aimRangeOrigin = _posFlyShift.magnitude + 50f;
	}
}
