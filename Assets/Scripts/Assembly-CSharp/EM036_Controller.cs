using System.Collections;
using CallbackDefs;
using StageLib;
using UnityEngine;

public class EM036_Controller : EnemyControllerBase, IManagedUpdateBehavior
{
	private enum MainStatus
	{
		Idle = 0,
		Walk = 1,
		Jump = 2,
		Skill_01 = 3,
		Skill_02 = 4,
		Skill_03 = 5,
		CDTime = 6,
		IdleWaitNet = 7
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
		ANI_IDLE_LOOP = 0,
		ANI_WALK_LOOP = 1,
		ANI_RUN_LOOP = 2,
		ANI_HURT_LOOP = 3,
		ANI_JUMP_START = 4,
		ANI_JUMP_LOOP = 5,
		ANI_JUMP_TO_FALL = 6,
		ANI_FALL_LOOP = 7,
		ANI_LANDING = 8,
		ANI_SKILL_01_START = 9,
		ANI_SKILL_01_END = 10,
		ANI_SKILL_02_LOOP = 11,
		MAX_ANIMATION_ID = 12
	}

	private MainStatus _mainStatus;

	private SubStatus _subStatus;

	private AnimationID _currentAnimationId;

	private float _currentFrame;

	private int[] _animationHash;

	private bool _shootSkill_01;

	private int _cdTime = 1000;

	[SerializeField]
	private int _walkSpeed = 700;

	[SerializeField]
	private VInt2 _jupmpSpeed = new VInt2(1000, 800);

	[SerializeField]
	private int _dashSpeed = 1200;

	[SerializeField]
	private Transform _shootTransform1;

	[SerializeField]
	private Transform _shootTransform2;

	[SerializeField]
	private float _sensorAngle = 70f;

	[SerializeField]
	private int _sensorDistance1 = 8;

	[SerializeField]
	private GameObject _weaponRender;

	[SerializeField]
	private float[] _cdTimeDef = new float[3] { 1f, 2f, 3f };

	[SerializeField]
	private int[] _cdRateDef = new int[3] { 40, 40, 20 };

	private int _nSummonEventId = 999;

	private Vector3 _rayExtra = new Vector3(0f, 1f, 0f);

	private Vector3 _rayWallCheck = new Vector3(0f, 2f, 0f);

	private RaycastHit2D _hit;

	private void OnEnable()
	{
		MonoBehaviourSingleton<UpdateManager>.Instance.AddUpdate(this);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		StopCoroutine("playeffect");
		MonoBehaviourSingleton<UpdateManager>.Instance.RemoveUpdate(this);
	}

	protected override void Awake()
	{
		base.Awake();
		Transform[] target = _transform.GetComponentsInChildren<Transform>(true);
		ModelTransform = OrangeBattleUtility.FindChildRecursive(ref target, "model", true);
		_animator = GetComponentInChildren<Animator>();
		_collideBullet = base.gameObject.AddOrGetComponent<CollideBullet>();
		base.AimPoint = new Vector3(0f, 1f, 0f);
		OrangeBattleUtility.AddEnemyAutoAimSystem(_transform, out _enemyAutoAimSystem);
		_animationHash = new int[12];
		_animationHash[0] = Animator.StringToHash("EM036@idle_loop");
		_animationHash[1] = Animator.StringToHash("EM036@walk_loop");
		_animationHash[2] = Animator.StringToHash("EM036@run_loop");
		_animationHash[3] = Animator.StringToHash("EM036@hurt_loop");
		_animationHash[4] = Animator.StringToHash("EM036@jump_start");
		_animationHash[5] = Animator.StringToHash("EM036@jump_loop");
		_animationHash[6] = Animator.StringToHash("EM036@jump_to_fall");
		_animationHash[7] = Animator.StringToHash("EM036@fall_loop");
		_animationHash[8] = Animator.StringToHash("EM036@landing");
		_animationHash[9] = Animator.StringToHash("EM036@skill_01_start");
		_animationHash[10] = Animator.StringToHash("EM036@skill_01_end");
		_animationHash[11] = Animator.StringToHash("EM036@skill_02_loop");
		SetStatus(MainStatus.Idle);
		AiTimer.TimerStart();
	}

	public override void UpdateStatus(int nSet, string smsg, Callback tCB = null)
	{
		SetStatus((MainStatus)nSet);
	}

	private void UpdateRandomState()
	{
		MainStatus mainStatus = MainStatus.Idle;
		if (AiState == AI_STATE.mob_002)
		{
			MainStatus mainStatus2 = _mainStatus;
			mainStatus = ((mainStatus2 != MainStatus.Skill_03) ? MainStatus.Skill_03 : MainStatus.CDTime);
		}
		else
		{
			switch (_mainStatus)
			{
			case MainStatus.Idle:
				mainStatus = MainStatus.Walk;
				break;
			case MainStatus.Walk:
				mainStatus = ((!TargetIsTooNear()) ? ((DisableMoveFall || OrangeBattleUtility.Random(0, 100) >= 50) ? MainStatus.Skill_01 : MainStatus.Jump) : MainStatus.Skill_02);
				break;
			case MainStatus.Jump:
				mainStatus = MainStatus.Skill_02;
				break;
			case MainStatus.Skill_01:
				switch (_subStatus)
				{
				case SubStatus.Phase0:
					mainStatus = ((!Target || DisableMoveFall) ? MainStatus.Skill_02 : MainStatus.Jump);
					break;
				case SubStatus.Phase2:
					mainStatus = MainStatus.CDTime;
					break;
				}
				break;
			case MainStatus.Skill_02:
				mainStatus = MainStatus.CDTime;
				break;
			case MainStatus.CDTime:
				mainStatus = ((!Target) ? MainStatus.Walk : ((!TargetIsTooNear()) ? ((DisableMoveFall || OrangeBattleUtility.Random(0, 100) >= 50) ? MainStatus.Skill_01 : MainStatus.Jump) : MainStatus.Skill_02));
				break;
			}
		}
		if (StageUpdate.gbIsNetGame)
		{
			if (StageUpdate.bIsHost)
			{
				StageUpdate.RegisterSendAndRun(sNetSerialID, (int)mainStatus);
				_mainStatus = MainStatus.IdleWaitNet;
			}
		}
		else
		{
			SetStatus(mainStatus);
		}
	}

	public override void LogicUpdate()
	{
		if (!Activate || !_enemyAutoAimSystem)
		{
			return;
		}
		BaseLogicUpdate();
		_currentFrame = _animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
		switch (_mainStatus)
		{
		case MainStatus.Idle:
			if (AiTimer.GetMillisecond() > EnemyData.n_AI_TIMER)
			{
				FaceToTarget();
				UpdateRandomState();
			}
			break;
		case MainStatus.Walk:
			Target = _enemyAutoAimSystem.GetClosetPlayer();
			if ((bool)Target)
			{
				FaceToTarget();
				UpdateRandomState();
			}
			else if (CheckMoveFall(_velocity + VInt3.signRight * base.direction * _walkSpeed))
			{
				_velocity.x = 0;
			}
			break;
		case MainStatus.Jump:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Jump, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if (Controller.Collisions.below)
				{
					SetStatus(MainStatus.Jump, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				if (_currentFrame > 1f)
				{
					UpdateRandomState();
				}
				break;
			}
			break;
		case MainStatus.Skill_01:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				Target = _enemyAutoAimSystem.GetClosetPlayer();
				if ((bool)Target)
				{
					if (AiTimer.GetMillisecond() > 3000)
					{
						FaceToTarget();
						UpdateRandomState();
					}
					else if (SectorSensor.Look(Controller.LogicPosition.vec3 + _rayExtra, base.direction, _sensorAngle, _sensorDistance1, targetMask, out _hit))
					{
						RaycastHit2D raycastHit2D = Physics2D.Raycast(Controller.LogicPosition.vec3 + _rayWallCheck, Vector2.right * base.direction, _sensorDistance1, 1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockLayer);
						if (!raycastHit2D || _hit.distance < raycastHit2D.distance)
						{
							_velocity = VInt3.zero;
							SetStatus(MainStatus.Skill_01, SubStatus.Phase1);
						}
					}
					else if (SectorSensor.Look(Controller.LogicPosition.vec3 + _rayExtra, -base.direction, _sensorAngle, _sensorDistance1, targetMask, out _hit))
					{
						RaycastHit2D raycastHit2D2 = Physics2D.Raycast(Controller.LogicPosition.vec3 + _rayWallCheck, Vector2.right * -base.direction, _sensorDistance1, 1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockLayer);
						if (!raycastHit2D2 || _hit.distance < raycastHit2D2.distance)
						{
							_velocity = VInt3.zero;
							UpdateDirection(-base.direction);
							SetStatus(MainStatus.Skill_01, SubStatus.Phase1);
						}
					}
				}
				else
				{
					UpdateRandomState();
				}
				break;
			case SubStatus.Phase1:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill_01, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				if (_currentFrame > 1f)
				{
					_weaponRender.SetActive(true);
					_cdTime = EnemyWeapons[1].BulletData.n_RELOAD;
					FaceToTarget();
					UpdateRandomState();
				}
				else if (!_shootSkill_01 && (double)_currentFrame > 0.04)
				{
					_shootSkill_01 = true;
					Shoot_Skill_01();
				}
				break;
			}
			break;
		case MainStatus.Skill_02:
			if (CheckMoveFall(_velocity + VInt3.signRight * base.direction * _walkSpeed))
			{
				_velocity.x = 0;
			}
			else
			{
				_velocity.x = _dashSpeed * base.direction;
			}
			if (_currentFrame > 3f)
			{
				StopCoroutine("playeffect");
				_cdTime = EnemyWeapons[1].BulletData.n_RELOAD;
				UpdateRandomState();
			}
			break;
		case MainStatus.Skill_03:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill_03, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if (_currentFrame > 1f)
				{
					UseSkill3Affter();
					SetStatus(MainStatus.CDTime);
				}
				break;
			}
			break;
		case MainStatus.CDTime:
			if (AiTimer.GetMillisecond() > _cdTime)
			{
				FaceToTarget();
				UpdateRandomState();
			}
			break;
		}
	}

	public void UpdateFunc()
	{
		if (Activate)
		{
			base.transform.localPosition = Vector3.MoveTowards(base.transform.localPosition, Controller.LogicPosition.vec3, distanceDelta);
		}
	}

	public override void SetActive(bool isActive)
	{
		base.SetActive(isActive);
		if (isActive)
		{
			SetCollideBullet();
			return;
		}
		StopCoroutine("playeffect");
		_collideBullet.BackToPool();
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
		UpdateDirection(base.direction);
		base.transform.position = pos;
	}

	public override void UpdateEnemyID(int id)
	{
		base.UpdateEnemyID(id);
		UpdateAIState();
		if (null == _enemyAutoAimSystem)
		{
			OrangeBattleUtility.AddEnemyAutoAimSystem(_transform, out _enemyAutoAimSystem);
		}
		_enemyAutoAimSystem.UpdateAimRange(EnemyWeapons[1].BulletData.f_DISTANCE);
		AI_STATE aiState = AiState;
		if (aiState == AI_STATE.mob_002)
		{
			_weaponRender.gameObject.SetActive(false);
		}
	}

	public override void SetSummonEventID(int nSummonEventId)
	{
		if (nSummonEventId != 0)
		{
			_nSummonEventId = nSummonEventId;
		}
	}

	private void SetStatus(MainStatus mainStatus, SubStatus subStatus = SubStatus.Phase0)
	{
		_mainStatus = mainStatus;
		_subStatus = subStatus;
		switch (_mainStatus)
		{
		case MainStatus.Idle:
			_velocity = VInt3.zero;
			break;
		case MainStatus.Walk:
			_velocity.x = _walkSpeed * base.direction;
			break;
		case MainStatus.Jump:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				base.SoundSource.PlaySE("EnemySE02", "em022_monkey01");
				_velocity = VInt3.zero;
				break;
			case SubStatus.Phase1:
				_velocity.x = base.direction * _jupmpSpeed.x * 1000;
				_velocity.y = _jupmpSpeed.y * 1000;
				break;
			case SubStatus.Phase2:
				_velocity = VInt3.zero;
				break;
			}
			break;
		case MainStatus.Skill_01:
			if (_subStatus == SubStatus.Phase0)
			{
				_shootSkill_01 = false;
				_velocity = VInt3.zero;
			}
			break;
		case MainStatus.Skill_02:
			Shoot_Skill_02();
			if (CheckMoveFall(_velocity + VInt3.signRight * base.direction * _walkSpeed))
			{
				_velocity.x = 0;
			}
			else
			{
				_velocity.x = _dashSpeed * base.direction;
			}
			break;
		case MainStatus.Skill_03:
		{
			_velocity = VInt3.zero;
			SubStatus subStatus2 = _subStatus;
			if (subStatus2 == SubStatus.Phase1)
			{
				Shoot_Skill_03();
			}
			break;
		}
		case MainStatus.CDTime:
			SetCollideBullet();
			_velocity = VInt3.zero;
			break;
		}
		AiTimer.TimerStart();
		UpdateAnimation();
	}

	private void UpdateAnimation()
	{
		switch (_mainStatus)
		{
		case MainStatus.Idle:
			_currentAnimationId = AnimationID.ANI_IDLE_LOOP;
			_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
			break;
		case MainStatus.Walk:
			_currentAnimationId = AnimationID.ANI_WALK_LOOP;
			_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
			break;
		case MainStatus.Jump:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_JUMP_START;
				_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_JUMP_LOOP;
				_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_LANDING;
				_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
				break;
			}
			break;
		case MainStatus.Skill_01:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_IDLE_LOOP;
				_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_SKILL_01_START;
				_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_SKILL_01_END;
				_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
				break;
			}
			break;
		case MainStatus.Skill_02:
			_currentAnimationId = AnimationID.ANI_SKILL_02_LOOP;
			_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
			break;
		case MainStatus.Skill_03:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_SKILL_01_START;
				_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_SKILL_01_END;
				_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
				break;
			}
			break;
		case MainStatus.CDTime:
			_currentAnimationId = AnimationID.ANI_IDLE_LOOP;
			_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
			break;
		}
	}

	private void UpdateDirection(int newDirection)
	{
		base.direction = newDirection;
		ModelTransform.localScale = new Vector3(ModelTransform.localScale.x, ModelTransform.localScale.y, Mathf.Abs(ModelTransform.localScale.z) * (float)base.direction);
		Vector3 localPosition = _shootTransform1.localPosition;
		Vector3 localPosition2 = _shootTransform2.localPosition;
		localPosition.x = ((base.direction > 0) ? Mathf.Abs(localPosition.x) : (Mathf.Abs(localPosition.x) * (float)base.direction));
		localPosition2.x = ((base.direction > 0) ? Mathf.Abs(localPosition2.x) : (Mathf.Abs(localPosition2.x) * (float)base.direction));
		_shootTransform1.localPosition = localPosition;
		_shootTransform2.localPosition = localPosition2;
	}

	private void FaceToTarget()
	{
		Target = _enemyAutoAimSystem.GetClosetPlayer();
		if ((bool)Target)
		{
			if (Target.transform.position.x > _transform.position.x)
			{
				UpdateDirection(1);
			}
			else
			{
				UpdateDirection(-1);
			}
		}
	}

	private bool TargetIsTooNear()
	{
		if ((bool)Target)
		{
			float num = 0f;
			num = ((!(Target.transform.position.x < _transform.position.x)) ? Mathf.Abs(Target.transform.position.x - base.transform.position.x) : Mathf.Abs(_transform.position.x - Target.transform.position.x));
			if (num < Mathf.Abs(_shootTransform2.localPosition.x))
			{
				return true;
			}
		}
		return false;
	}

	private void SetCollideBullet()
	{
		_collideBullet.UpdateBulletData(EnemyWeapons[0].BulletData);
		_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
		_collideBullet.Active(targetMask);
	}

	private void Shoot_Skill_01()
	{
		SKILL_TABLE skillTable = EnemyWeapons[1].BulletData;
		AxeBullet bullet = MonoBehaviourSingleton<PoolManager>.Instance.GetPoolObj<AxeBullet>(skillTable.s_MODEL);
		_weaponRender.SetActive(false);
		if ((bool)bullet)
		{
			string owner = EnemyData.n_ID.ToString();
			bullet.UpdateBulletData(skillTable, owner);
			bullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			bullet.transform.SetPositionAndRotation(_shootTransform1.transform.position, Quaternion.identity);
			bullet.Active(_hit.point, Vector3.right * base.direction, targetMask, null);
			return;
		}
		MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad("prefab/bullet/" + skillTable.s_MODEL, skillTable.s_MODEL, delegate(GameObject obj)
		{
			GameObject gameObject = Object.Instantiate(obj);
			MonoBehaviourSingleton<PoolManager>.Instance.CreatePoolBaseLocal<AxeBullet>(gameObject.GetComponent<AxeBullet>(), skillTable.s_MODEL);
			bullet = MonoBehaviourSingleton<PoolManager>.Instance.GetPoolObj<AxeBullet>(skillTable.s_MODEL);
			if ((bool)bullet)
			{
				string owner2 = EnemyData.n_ID.ToString();
				bullet.UpdateBulletData(skillTable, owner2);
				bullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
				bullet.transform.position = _shootTransform1.transform.position;
				bullet.Active(_hit.point, Vector3.right * base.direction, targetMask, null);
			}
		});
	}

	private void Shoot_Skill_02()
	{
		_collideBullet.UpdateBulletData(EnemyWeapons[2].BulletData);
		_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
		_collideBullet.Active(targetMask);
		StartCoroutine("playeffect");
	}

	private void Shoot_Skill_03()
	{
		MonoBehaviourSingleton<OrangeBattleUtility>.Instance.CallSummonEnemyEvent(_transform, _nSummonEventId);
	}

	private void UseSkill3Affter()
	{
		int num = OrangeBattleUtility.Random(0, 100);
		int num2 = 0;
		_cdTime = 1000;
		for (int i = 0; i < _cdTimeDef.Length; i++)
		{
			num2 += _cdRateDef[i];
			if (num < num2)
			{
				_cdTime = Mathf.RoundToInt(_cdTimeDef[i] * 1000f);
				break;
			}
		}
	}

	private IEnumerator playeffect()
	{
		while (true)
		{
			if (!MonoBehaviourSingleton<UpdateManager>.Instance.Pause)
			{
				base.SoundSource.PlaySE("EnemySE02", "em022_monkey02");
			}
			yield return new WaitForSeconds(0.4f);
		}
	}
}
