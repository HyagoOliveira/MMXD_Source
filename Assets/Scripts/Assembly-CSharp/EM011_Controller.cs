using System;
using CodeStage.AntiCheat.ObscuredTypes;
using UnityEngine;

public class EM011_Controller : EnemyControllerBase, IManagedUpdateBehavior
{
	private enum MainStatus
	{
		Idle = 0,
		Open = 1,
		IdleWaitNet = 2
	}

	private enum SubStatus
	{
		Phase0 = 0,
		Phase1 = 1,
		Phase2 = 2,
		Phase3 = 3,
		MAX_SUBSTATUS = 4
	}

	public enum AnimationID
	{
		ANI_IDLE = 0,
		ANI_OPEN = 1,
		ANI_OPENED = 2,
		MAX_ANIMATION_ID = 3
	}

	private BS016_Controller _parentController;

	private readonly Vector3 _spinSpeed = new Vector3(0f, 0f, 1350f);

	private readonly Vector2 _defaultSize = new Vector2(0.25f, 0.5f);

	private int _targetY;

	private BoxCollider2D _blockCollider;

	private new EnemyCollider _enemyCollider;

	private Transform _spikeTransform;

	private MainStatus _mainStatus;

	private SubStatus _subStatus;

	private AnimationID _currentAnimationId;

	private float _currentFrame;

	private int[] _animationHash;

	private void OnEnable()
	{
		MonoBehaviourSingleton<UpdateManager>.Instance.AddUpdate(this);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		MonoBehaviourSingleton<UpdateManager>.Instance.RemoveUpdate(this);
	}

	private void SetStatus(MainStatus mainStatus, SubStatus subStatus = SubStatus.Phase0)
	{
		_mainStatus = mainStatus;
		_subStatus = subStatus;
		switch (_mainStatus)
		{
		case MainStatus.Idle:
			_blockCollider.enabled = false;
			_blockCollider.offset = Vector2.zero;
			_blockCollider.size = _defaultSize;
			_enemyCollider.SetOffset(Vector2.zero);
			_enemyCollider.SetSize(_defaultSize);
			break;
		case MainStatus.Open:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				LeanTween.value(base.gameObject, ModelTransform.localEulerAngles.z, (base.direction == 1) ? 720f : (-360f), 1f).setOnUpdate(delegate(float f)
				{
					Vector3 localEulerAngles = ModelTransform.localEulerAngles;
					localEulerAngles.z = f;
					ModelTransform.localEulerAngles = localEulerAngles;
				}).setOnComplete((Action)delegate
				{
					SetStatus(_mainStatus, SubStatus.Phase1);
				});
				break;
			case SubStatus.Phase1:
				_collideBullet.BackToPool();
				break;
			case SubStatus.Phase2:
			{
				_blockCollider.enabled = true;
				Vector2 point = (Vector2)_blockCollider.transform.position + _blockCollider.offset;
				Vector2 size = _blockCollider.size;
				Collider2D collider2D = Physics2D.OverlapBox(point, size, 0f, LayerMask.GetMask("Player"));
				if ((bool)collider2D)
				{
					OrangeCharacter component = collider2D.gameObject.GetComponent<OrangeCharacter>();
					if ((bool)component)
					{
						Vector3 centerPos = component.Controller.GetCenterPos();
						Vector2 size2 = component.Controller.Collider2D.size;
						centerPos = ((!(centerPos.x > point.x)) ? new Vector3(point.x - (size.x + size2.x) / 2f - 0.01f, component._transform.position.y, component._transform.position.z) : new Vector3(point.x + (size.x + size2.x) / 2f + 0.01f, component._transform.position.y, component._transform.position.z));
						component._transform.position = centerPos;
						component.Controller.LogicPosition = new VInt3(component._transform.position);
					}
				}
				break;
			}
			}
			break;
		}
		AiTimer.TimerStart();
		UpdateAnimation();
	}

	private void UpdateAnimation()
	{
		bool flag = false;
		switch (_mainStatus)
		{
		case MainStatus.Idle:
			_currentAnimationId = AnimationID.ANI_IDLE;
			break;
		case MainStatus.Open:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_IDLE;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_OPEN;
				break;
			case SubStatus.Phase2:
				flag = true;
				break;
			}
			break;
		}
		if (!flag)
		{
			_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
		}
	}

	protected override void Awake()
	{
		base.Awake();
		_animator = GetComponentInChildren<Animator>();
		_collideBullet = base.gameObject.AddOrGetComponent<CollideBullet>();
		Transform[] target = _transform.GetComponentsInChildren<Transform>(true);
		ModelTransform = OrangeBattleUtility.FindChildRecursive(ref target, "model", true);
		_spikeTransform = OrangeBattleUtility.FindChildRecursive(ref target, "Dummy005", true);
		_enemyCollider = OrangeBattleUtility.FindChildRecursive(ref target, "Collider", true).GetComponent<EnemyCollider>();
		_blockCollider = OrangeBattleUtility.FindChildRecursive(ref target, "BlockCollider", true).GetComponent<BoxCollider2D>();
		_blockCollider.enabled = false;
		_animationHash = new int[3];
		_animationHash[0] = Animator.StringToHash("no_extending");
		_animationHash[1] = Animator.StringToHash("extending");
		_animationHash[2] = Animator.StringToHash("extended");
		SetStatus(MainStatus.Idle);
	}

	public override void LogicUpdate()
	{
		if (!Activate)
		{
			return;
		}
		BaseUpdate();
		_currentFrame = _animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
		if (_blockCollider.enabled && _parentController != null && _parentController.IsDashAttack() && (bool)_parentController.GetPunchCollider() && _enemyCollider.IsTouching(_parentController.GetPunchCollider()))
		{
			Hp = 0;
			Hurt(new HurtPassParam());
		}
		switch (_mainStatus)
		{
		case MainStatus.Idle:
			if (_velocity.y <= 0 && Controller.LogicPosition.y <= _targetY)
			{
				_velocity = VInt3.zero;
				SetStatus(MainStatus.Open);
				break;
			}
			if ((_velocity.x > 0 && Controller.Collisions.right) || (_velocity.x < 0 && Controller.Collisions.left) || Controller.Collisions.below)
			{
				Hp = 0;
				Hurt(new HurtPassParam());
			}
			UpdateGravity();
			break;
		case MainStatus.Open:
			switch (_subStatus)
			{
			case SubStatus.Phase1:
			{
				Vector2 offset = _blockCollider.offset;
				Vector2 size = _blockCollider.size;
				offset.y = (_spikeTransform.localPosition.z + 0.2f) / 2f;
				size.y = Mathf.Abs(_spikeTransform.localPosition.z) + 0.2f;
				_blockCollider.offset = offset;
				_blockCollider.size = size;
				_enemyCollider.SetOffset(offset);
				_enemyCollider.SetSize(size);
				if (_currentFrame >= 1f)
				{
					SetStatus(_mainStatus, SubStatus.Phase2);
				}
				break;
			}
			}
			break;
		}
		Controller.Move((_velocity + _velocityExtra) * GameLogicUpdateManager.m_fFrameLen);
		distanceDelta = Vector3.Distance(base.transform.localPosition, Controller.LogicPosition.vec3) * (Time.deltaTime / GameLogicUpdateManager.m_fFrameLen);
		_velocityExtra = VInt3.zero;
		_velocityShift = VInt3.zero;
	}

	public void UpdateFunc()
	{
		if (!Activate)
		{
			return;
		}
		switch (_mainStatus)
		{
		case MainStatus.Idle:
			ModelTransform.localEulerAngles += _spinSpeed * base.direction * Time.deltaTime;
			break;
		case MainStatus.Open:
			if (_subStatus != 0)
			{
				int num = 1;
			}
			break;
		}
		base.transform.localPosition = Vector3.MoveTowards(base.transform.localPosition, Controller.LogicPosition.vec3, distanceDelta);
	}

	public override void SetActive(bool isActive)
	{
		InGame = isActive;
		Controller.enabled = isActive;
		_animator.enabled = isActive;
		SetColliderEnable(isActive);
		if (isActive)
		{
			AiTimer.TimerStart();
			_transform.SetParent(null);
			Controller.LogicPosition = new VInt3(_transform.localPosition);
			_targetY = Controller.LogicPosition.y + 1000;
			_velocityExtra = VInt3.zero;
			MonoBehaviourSingleton<GameLogicUpdateManager>.Instance.AddUpdate(this);
			MonoBehaviourSingleton<FxManager>.Instance.UpdateFx(FxArray, true);
			_collideBullet.UpdateBulletData(EnemyWeapons[0].BulletData);
			_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			_collideBullet.Active(targetMask);
			SetStatus(MainStatus.Idle);
			if ((bool)_characterMaterial)
			{
				_characterMaterial.Appear();
			}
		}
		else
		{
			Hp = 0;
			UpdateHurtAction();
			AiTimer.TimerStop();
			MonoBehaviourSingleton<GameLogicUpdateManager>.Instance.RemoveUpdate(this);
			MonoBehaviourSingleton<FxManager>.Instance.UpdateFx(FxArray, false);
			selfBuffManager.StopLoopSE();
			_blockCollider.enabled = false;
			_collideBullet.BackToPool();
			LeanTween.cancel(base.gameObject);
			if ((bool)_characterMaterial)
			{
				_characterMaterial.Disappear(delegate
				{
					if (!InGame)
					{
						MonoBehaviourSingleton<PoolManager>.Instance.BackToPool(this, EnemyData.s_MODEL);
					}
				});
			}
		}
		Activate = ManagedSingleton<StageHelper>.Instance.bEnemyActive && isActive;
		if (!isActive)
		{
			bNeedDead = false;
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
		base.transform.localScale = new Vector3(_transform.localScale.x, _transform.localScale.y, _transform.localScale.z * -1f);
		base.transform.position = pos;
	}

	public override ObscuredInt Hurt(HurtPassParam tHurtPassParam)
	{
		return base.Hurt(tHurtPassParam);
	}

	public void SetParentVAVA(BS016_Controller parent)
	{
		_parentController = parent;
	}
}
