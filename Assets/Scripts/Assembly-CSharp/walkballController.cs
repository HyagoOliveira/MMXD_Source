using System;
using UnityEngine;

public class walkballController : EnemyControllerBase, IManagedUpdateBehavior
{
	public enum MainStatus
	{
		Idle = 0,
		Walk = 1,
		Roll = 2,
		Open = 3,
		Morph = 4,
		Close = 5,
		MAX_STATUS = 6
	}

	public readonly int walkSpeedHash = Animator.StringToHash("fWalkSpeed");

	private CharacterMaterial _rollMaterial;

	private Animator _ballAnimator;

	private int[] _animatorHash;

	private MainStatus _mainStatus = MainStatus.Roll;

	private bool _animEnd;

	private static float _walkSpeed = 1.5f;

	private static float _rollSpeed = 2f;

	private Transform _rollModelTransform;

	private OrangeTimer rollRecycleTimer;

	private readonly Vector2 _normalOffset = new Vector2(0f, 0.3f);

	private readonly Vector2 _ballOffset = new Vector2(0f, 1.9f);

	private bool isBall;

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
		rollRecycleTimer = OrangeTimerManager.GetTimer();
		Transform[] target = base.transform.GetComponentsInChildren<Transform>(true);
		ModelTransform = OrangeBattleUtility.FindChildRecursive(ref target, "model", true);
		_rollModelTransform = OrangeBattleUtility.FindChildRecursive(ref target, "model_roll", true);
		_rollMaterial = _rollModelTransform.GetComponent<CharacterMaterial>();
		_animatorHash = new int[6];
		base.AimTransform = OrangeBattleUtility.FindChildRecursive(ref target, "body_TR", true);
		_animator = GetComponentInChildren<Animator>();
		_ballAnimator = _rollModelTransform.GetComponentInChildren<Animator>();
		_animatorHash[0] = Animator.StringToHash("idle");
		_animatorHash[1] = Animator.StringToHash("walk");
		_animatorHash[2] = Animator.StringToHash("roll");
		_animatorHash[3] = Animator.StringToHash("open");
		_animatorHash[4] = Animator.StringToHash("morph");
		_animatorHash[5] = Animator.StringToHash("close");
		_animator.SetFloat(walkSpeedHash, _walkSpeed);
		SetBall(true);
	}

	public override void UpdateEnemyID(int id)
	{
		base.UpdateEnemyID(id);
		if (!_enemyAutoAimSystem)
		{
			OrangeBattleUtility.AddEnemyAutoAimSystem(_transform, out _enemyAutoAimSystem);
		}
		_enemyAutoAimSystem.UpdateAimRange(2f);
	}

	public void UpdateFunc()
	{
		if (Activate)
		{
			base.transform.localPosition = Vector3.MoveTowards(base.transform.localPosition, Controller.LogicPosition.vec3, distanceDelta);
		}
	}

	public override void LogicUpdate()
	{
		if (!Activate)
		{
			return;
		}
		base.LogicUpdate();
		_animEnd = _animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f;
		if ((Controller.Collisions.left && base.direction == -1) || (Controller.Collisions.right && base.direction == 1))
		{
			base.direction = -base.direction;
			ModelTransform.localEulerAngles = new Vector3(0f, 90 * base.direction, 0f);
		}
		switch (_mainStatus)
		{
		case MainStatus.Idle:
			_velocity.x = 0;
			if (!Controller.Collisions.below)
			{
				SetBall(true);
				_animator.Play(0, -1, 0f);
				_ballAnimator.Play(0, -1, 0f);
				_mainStatus = MainStatus.Roll;
			}
			break;
		case MainStatus.Walk:
			_velocity.x = Mathf.RoundToInt((float)base.direction * _walkSpeed * 1000f);
			if (!Controller.Collisions.below)
			{
				SetBall(true);
				_animator.Play(0, -1, 0f);
				_ballAnimator.Play(0, -1, 0f);
				_mainStatus = MainStatus.Roll;
			}
			break;
		case MainStatus.Roll:
			_animEnd = _ballAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f;
			if (!rollRecycleTimer.IsStarted())
			{
				rollRecycleTimer.TimerStart();
			}
			else if (rollRecycleTimer.GetMillisecond() > 10000)
			{
				bNeedDead = true;
			}
			if (Controller.Collisions.below && !Controller.CollisionsOld.below)
			{
				_velocity.x = Mathf.RoundToInt((float)base.direction * _rollSpeed * 1000f);
				rollRecycleTimer.TimerStop();
				_animator.Play(0, -1, 0f);
				_ballAnimator.Play(0, -1, 0f);
			}
			else if (_animEnd && Controller.Collisions.below && Controller.CollisionsOld.below)
			{
				_velocity.x = 0;
				rollRecycleTimer.TimerStop();
				_mainStatus = MainStatus.Open;
				_animator.Play(0, -1, 0f);
				_ballAnimator.Play(0, -1, 0f);
			}
			break;
		case MainStatus.Open:
			_animEnd = _ballAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f;
			_velocity.x = 0;
			if (_animEnd)
			{
				_mainStatus = MainStatus.Morph;
				_animator.Play(0, -1, 0f);
				_ballAnimator.Play(0, -1, 0f);
				SetBall(false);
			}
			break;
		case MainStatus.Morph:
			_animEnd = _animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f;
			if (_animEnd)
			{
				_mainStatus = MainStatus.Walk;
				_animator.Play(0, -1, 0f);
				if (_ballAnimator.gameObject.activeSelf)
				{
					_ballAnimator.Play(0, -1, 0f);
				}
			}
			break;
		case MainStatus.Close:
			_animEnd = _ballAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f;
			_velocity.x = 0;
			if (_animEnd)
			{
				_mainStatus = MainStatus.Roll;
				_animator.Play(0, -1, 0f);
				_ballAnimator.Play(0, -1, 0f);
				SetBall(true);
			}
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
		_animator.Play(_animatorHash[(int)_mainStatus]);
		if (_ballAnimator.isActiveAndEnabled)
		{
			_ballAnimator.Play(_animatorHash[(int)_mainStatus]);
		}
	}

    [Obsolete]
    public override void SetActive(bool isActive)
	{
		if (isActive)
		{
			InGame = true;
			_rollMaterial.Appear();
			_characterMaterial.Appear(delegate
			{
				Activate = true;
				Hp = EnemyData.n_HP;
				Controller.enabled = true;
				_animator.enabled = true;
				SetColliderEnable(true);
				base.SetActive(Activate);
			});
			return;
		}
		Activate = false;
		base.SetActive(false);
		Controller.enabled = false;
		SetColliderEnable(false);
		_animator.enabled = false;
		_rollMaterial.Disappear();
		_characterMaterial.Disappear(delegate
		{
			rollRecycleTimer.TimerStop();
			InGame = false;
			_velocity = VInt3.zero;
			SetBall(true);
			_mainStatus = MainStatus.Roll;
			_animator.enabled = true;
			base.direction = -1;
			_animator.ForceStateNormalizedTime(0f);
			_ballAnimator.ForceStateNormalizedTime(0f);
			Controller.Reset();
			MonoBehaviourSingleton<PoolManager>.Instance.BackToPool(this, EnemyData.s_MODEL);
		});
	}

	private void SetBall(bool status)
	{
		Renderer[] renderer = _characterMaterial.GetRenderer();
		for (int i = 0; i < renderer.Length; i++)
		{
			renderer[i].enabled = !status;
		}
		_rollModelTransform.gameObject.SetActive(status);
		isBall = status;
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
		ModelTransform.localEulerAngles = new Vector3(0f, 90 * base.direction, 0f);
		base.transform.position = pos;
	}

	public override void BackToPool()
	{
		rollRecycleTimer.TimerStop();
		SetActive(false);
	}
}
