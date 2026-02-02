#define RELEASE
using System;
using CallbackDefs;
using Newtonsoft.Json;
using UnityEngine;

public class EM089_Controller : EnemyControllerBase, IManagedUpdateBehavior
{
	private enum MainStatus
	{
		Idle = 0,
		Run = 1,
		Hurt = 2,
		SwitchStatus = 9999
	}

	private enum SubStatus
	{
		Phase0 = 0,
		Phase1 = 1,
		Phase2 = 2,
		Phase3 = 3,
		Phase4 = 4,
		Phase5 = 5,
		MAX_SUBSTATUS = 6
	}

	public enum AnimationID
	{
		ANI_IDLE = 0,
		ANI_RUN_LOOP = 1,
		ANI_HURT = 2,
		MAX_ANIMATION_ID = 3
	}

	private MainStatus _mainStatus;

	private SubStatus _subStatus;

	private AnimationID _currentAnimationId;

	private float _currentFrame;

	private int[] _animationHash;

	[SerializeField]
	private float WaitTime;

	[SerializeField]
	private int WaitFrame;

	private bool isPatrol;

	private bool _patrolIsLoop;

	private float distance;

	private Vector3[] _patrolPaths = new Vector3[0];

	private int _patrolIndex;

	private VInt3 nextVelocity;

	private float jetAngle;

	private bool floatbelow;

	[SerializeField]
	private int _flySpeed = 4500;

	private readonly int _HashAngle = Animator.StringToHash("angle");

	[SerializeField]
	private float TerrainJudge = 0.6f;

	private void OnEnable()
	{
		MonoBehaviourSingleton<UpdateManager>.Instance.AddUpdate(this);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		MonoBehaviourSingleton<UpdateManager>.Instance.RemoveUpdate(this);
	}

	private void HashAnimator()
	{
		_animationHash = new int[3];
		_animationHash[0] = Animator.StringToHash("EM089@idle_loop");
		_animationHash[1] = Animator.StringToHash("EM089@run_loop");
		_animationHash[2] = Animator.StringToHash("EM089@hurt_loop");
	}

	private void LoadParts(ref Transform[] childs)
	{
		ModelTransform = OrangeBattleUtility.FindChildRecursive(ref childs, "model", true);
		_animator = ModelTransform.GetComponent<Animator>();
		_collideBullet = OrangeBattleUtility.FindChildRecursive(ref childs, "Collider", true).gameObject.AddOrGetComponent<CollideBullet>();
	}

	protected override void Awake()
	{
		base.Awake();
		Transform[] childs = _transform.GetComponentsInChildren<Transform>(true);
		HashAnimator();
		LoadParts(ref childs);
		base.AimPoint = new Vector3(0f, 1f, 0f);
		OrangeBattleUtility.AddEnemyAutoAimSystem(_transform, out _enemyAutoAimSystem);
		_enemyAutoAimSystem.UpdateAimRange(10f);
		AiTimer.TimerStart();
	}

	public override void UpdateStatus(int nSet, string smsg, Callback tCB = null)
	{
		bWaitNetStatus = false;
		if ((int)Hp <= 0 || smsg == null || !(smsg != ""))
		{
			return;
		}
		if (smsg[0] == '{')
		{
			NetSyncData netSyncData = JsonConvert.DeserializeObject<NetSyncData>(smsg);
			Controller.LogicPosition.x = netSyncData.SelfPosX;
			Controller.LogicPosition.y = netSyncData.SelfPosY;
			Controller.LogicPosition.z = netSyncData.SelfPosZ;
			TargetPos.x = netSyncData.TargetPosX;
			TargetPos.y = netSyncData.TargetPosY;
			TargetPos.z = netSyncData.TargetPosZ;
			if (netSyncData.bSetHP)
			{
				Hp = netSyncData.nHP;
			}
			SetStatus((MainStatus)nSet);
		}
		else if (nSet == 9999 && smsg == "NoDrop")
		{
			DisableMoveFall = true;
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
		case MainStatus.Run:
			if (isPatrol && (_patrolIsLoop || _patrolIndex < _patrolPaths.Length))
			{
				switch (_subStatus)
				{
				case SubStatus.Phase0:
				{
					int num = _patrolIndex % _patrolPaths.Length;
					nextVelocity = new VInt3((_patrolPaths[num] - _transform.position).normalized) * _flySpeed;
					distance = Vector3.Distance(_patrolPaths[num], _transform.position);
					break;
				}
				case SubStatus.Phase1:
				{
					int num = _patrolIndex % _patrolPaths.Length;
					_velocity = nextVelocity;
					UpdateDirection();
					break;
				}
				case SubStatus.Phase2:
					_velocity = VInt3.zero;
					WaitFrame = GameLogicUpdateManager.GameFrame + (int)(WaitTime * 20f);
					break;
				}
				break;
			}
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (!Controller.Collisions.below)
				{
					floatbelow = true;
					jetAngle = 90f;
					nextVelocity = VInt3.signDown * _flySpeed;
				}
				else
				{
					floatbelow = false;
					jetAngle = 0f;
					nextVelocity = VInt3.signRight * base.direction * _flySpeed;
				}
				break;
			case SubStatus.Phase1:
				_velocity = nextVelocity;
				break;
			case SubStatus.Phase2:
				_velocity = VInt3.zero;
				_animator.SetFloat(_HashAngle, jetAngle);
				WaitFrame = GameLogicUpdateManager.GameFrame + (int)(WaitTime * 20f);
				break;
			}
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
			_currentAnimationId = AnimationID.ANI_IDLE;
			_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
			break;
		case MainStatus.Run:
			_currentAnimationId = AnimationID.ANI_RUN_LOOP;
			_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
			break;
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
			Target = _enemyAutoAimSystem.GetClosetPlayer();
			if (!bWaitNetStatus && (bool)Target)
			{
				UploadEnemyStatus(1);
			}
			break;
		case MainStatus.Run:
			if (isPatrol)
			{
				switch (_subStatus)
				{
				case SubStatus.Phase0:
					_animator.SetFloat(_HashAngle, jetAngle);
					SetStatus(MainStatus.Run, SubStatus.Phase1);
					break;
				case SubStatus.Phase1:
					if (distance <= 0f)
					{
						SetStatus(MainStatus.Run, SubStatus.Phase2);
					}
					break;
				case SubStatus.Phase2:
					if (WaitFrame < GameLogicUpdateManager.GameFrame)
					{
						_patrolIndex++;
						if (!_patrolIsLoop && _patrolIndex >= _patrolPaths.Length)
						{
							SetStatus(MainStatus.Idle);
						}
						else
						{
							SetStatus(MainStatus.Run);
						}
					}
					break;
				}
				break;
			}
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_animator.SetFloat(_HashAngle, jetAngle);
				SetStatus(MainStatus.Run, SubStatus.Phase1);
				break;
			case SubStatus.Phase1:
				if (Controller.Collisions.below && CheckMoveFall(_velocity))
				{
					IgnoreGravity = false;
					SetStatus(MainStatus.Run, SubStatus.Phase2);
				}
				else if (floatbelow && Controller.Collisions.below)
				{
					jetAngle = 0f;
					IgnoreGravity = false;
					SetStatus(MainStatus.Run, SubStatus.Phase2);
				}
				else if (!floatbelow && !Controller.Collisions.below)
				{
					jetAngle = 90f;
					IgnoreGravity = true;
					SetStatus(MainStatus.Run, SubStatus.Phase2);
				}
				else if (Controller.Collisions.left || Controller.Collisions.right)
				{
					SetStatus(MainStatus.Run, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				if (WaitFrame < GameLogicUpdateManager.GameFrame)
				{
					if (jetAngle != 90f)
					{
						UpdateDirection(-base.direction);
					}
					SetStatus(MainStatus.Run);
				}
				break;
			}
			break;
		}
	}

	public void UpdateFunc()
	{
		if (!Activate)
		{
			return;
		}
		Vector3 localPosition = _transform.localPosition;
		_transform.localPosition = Vector3.MoveTowards(_transform.localPosition, Controller.LogicPosition.vec3, distanceDelta);
		if (!isPatrol)
		{
			return;
		}
		MainStatus mainStatus = _mainStatus;
		if (mainStatus == MainStatus.Run)
		{
			SubStatus subStatus = _subStatus;
			if (subStatus == SubStatus.Phase1)
			{
				distance -= Vector3.Distance(localPosition, _transform.localPosition);
			}
		}
	}

	public override void SetActive(bool isActive)
	{
		base.SetActive(isActive);
		if (isActive)
		{
			_collideBullet.UpdateBulletData(EnemyWeapons[0].BulletData);
			_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			_collideBullet.Active(targetMask);
			IgnoreGravity = true;
			_patrolIndex = 0;
			isPatrol = false;
			SetStatus(MainStatus.Idle);
			base.SoundSource.ActivePlaySE("EnemySE02", "em027_deathguard_lp");
		}
		else
		{
			base.SoundSource.PlaySE("EnemySE02", "em027_deathguard_stop");
			_patrolPaths = new Vector3[0];
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
		ModelTransform.localScale = new Vector3(_transform.localScale.x, _transform.localScale.y, _transform.localScale.z * (float)base.direction);
		ModelTransform.localEulerAngles = new Vector3(0f, 90f, 0f);
		_transform.position = pos;
	}

	public override void UpdateEnemyID(int id)
	{
		base.UpdateEnemyID(id);
		UpdateAIState();
		if (null == _enemyAutoAimSystem)
		{
			OrangeBattleUtility.AddEnemyAutoAimSystem(_transform, out _enemyAutoAimSystem);
		}
	}

	public override void SetPatrolPath(bool isLoop, int nMoveSpeed, Vector3[] paths)
	{
	}

	private void UpdateDirection(int forceDirection = 0)
	{
		if (forceDirection != 0)
		{
			base.direction = forceDirection;
		}
		else if (TargetPos.x > Controller.LogicPosition.x)
		{
			base.direction = 1;
		}
		else
		{
			base.direction = -1;
		}
		ModelTransform.localScale = new Vector3(ModelTransform.localScale.x, ModelTransform.localScale.y, Mathf.Abs(ModelTransform.localScale.z) * (float)base.direction);
	}

	protected override bool CheckMoveFall(VInt3 velocity)
	{
		if (!DisableMoveFall)
		{
			return false;
		}
		float num = 0.3f;
		AI_STATE aiState = AiState;
		if (aiState == AI_STATE.mob_002)
		{
			num = TerrainJudge;
		}
		float y = Controller.GetBounds().size.y;
		VInt3 vInt = velocity * GameLogicUpdateManager.m_fFrameLen;
		float num2 = Mathf.Abs(vInt.vec3.x);
		int num3 = Math.Sign(vInt.vec3.x);
		Controller2D.RaycastOrigins raycastOrigins = Controller.GetRaycastOrigins();
		Vector2 vector = ((num3 == -1) ? raycastOrigins.topLeft : raycastOrigins.topRight);
		if ((bool)OrangeBattleUtility.RaycastIgnoreSelf(vector, Vector2.right * num3, num2, (int)Controller.collisionMask | (int)Controller.collisionMaskThrough, _transform))
		{
			return false;
		}
		Vector2 vector2 = vector + Vector2.right * num3 * num2;
		RaycastHit2D raycastHit2D = OrangeBattleUtility.RaycastIgnoreSelf(vector2, Vector2.down, y + num, (int)Controller.collisionMask | (int)Controller.collisionMaskThrough, _transform);
		Debug.DrawLine(vector, vector2, Color.cyan, 0.5f);
		if ((bool)raycastHit2D)
		{
			Debug.DrawLine(vector2, raycastHit2D.point, Color.cyan, 0.5f);
			return false;
		}
		Debug.DrawLine(vector2, vector2 + Vector2.down * (y + num), Color.cyan, 0.5f);
		return true;
	}
}
