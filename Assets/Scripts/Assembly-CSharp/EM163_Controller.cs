using CallbackDefs;
using Newtonsoft.Json;
using UnityEngine;

public class EM163_Controller : EnemyControllerBase, IManagedUpdateBehavior
{
	private enum MainStatus
	{
		Idle = 0,
		Move = 1,
		Skill0 = 2,
		Hurt = 3
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
		ANI_Skill0 = 1,
		ANI_HURT = 2,
		MAX_ANIMATION_ID = 3
	}

	private MainStatus _mainStatus;

	private SubStatus _subStatus;

	private AnimationID _currentAnimationId;

	private float _currentFrame;

	private int[] _animationHash;

	private bool isPatrol;

	private bool _patrolIsLoop;

	private float MoveDis;

	private Vector3[] _patrolPaths = new Vector3[0];

	private int _patrolIndex;

	private Vector3 StartPos;

	private int MoveSpeed = 5000;

	private bool CanAtk;

	private Vector3 AtkPos;

	[SerializeField]
	private Transform ShootPos;

	[SerializeField]
	private float ShootTime = 2f;

	[SerializeField]
	private float ShootFrame;

	private float ShotAngle;

	private readonly int _HashAngle = Animator.StringToHash("Angle");

	[SerializeField]
	private Transform LaserObj;

	[SerializeField]
	private ParticleSystem LaserFx;

	[SerializeField]
	private ParticleSystem LaserLineFx;

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
		_animationHash[0] = Animator.StringToHash("EM163@idle_loop");
		_animationHash[1] = Animator.StringToHash("EM163@skill_loop");
	}

	private void LoadParts(ref Transform[] childs)
	{
		ModelTransform = OrangeBattleUtility.FindChildRecursive(ref childs, "model", true);
		_animator = ModelTransform.GetComponent<Animator>();
		_collideBullet = OrangeBattleUtility.FindChildRecursive(ref childs, "Collider", true).gameObject.AddOrGetComponent<CollideBullet>();
		if (ShootPos == null)
		{
			ShootPos = OrangeBattleUtility.FindChildRecursive(ref childs, "ShootPoint", true);
		}
		if (LaserFx == null)
		{
			LaserFx = OrangeBattleUtility.FindChildRecursive(ref childs, "fxduring_Big_Illumina_002_(work)", true).gameObject.AddOrGetComponent<ParticleSystem>();
		}
		if (LaserLineFx == null)
		{
			LaserLineFx = OrangeBattleUtility.FindChildRecursive(ref childs, "fxduring_Big_Illumina_001_(work)", true).gameObject.AddOrGetComponent<ParticleSystem>();
		}
		if (LaserObj == null)
		{
			LaserObj = OrangeBattleUtility.FindChildRecursive(ref childs, "LaserCollider", true);
		}
	}

	protected override void Awake()
	{
		base.Awake();
		Transform[] childs = _transform.GetComponentsInChildren<Transform>(true);
		HashAnimator();
		LoadParts(ref childs);
		base.AimPoint = Vector3.zero;
		OrangeBattleUtility.AddEnemyAutoAimSystem(_transform, out _enemyAutoAimSystem);
		_enemyAutoAimSystem.UpdateAimRange(8f);
		AiTimer.TimerStart();
	}

	public override void UpdateStatus(int nSet, string smsg, Callback tCB = null)
	{
		bWaitNetStatus = false;
		if ((int)Hp <= 0)
		{
			return;
		}
		if (smsg != null && smsg != "")
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
		}
		SetStatus((MainStatus)nSet);
	}

	private void UpdateDirection(int forceDirection = 0, bool back = false)
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
		if (back)
		{
			base.direction = -base.direction;
		}
		ModelTransform.localScale = new Vector3(ModelTransform.localScale.x, ModelTransform.localScale.y, Mathf.Abs(ModelTransform.localScale.z) * (float)base.direction);
	}

	private void SetStatus(MainStatus mainStatus, SubStatus subStatus = SubStatus.Phase0)
	{
		_mainStatus = mainStatus;
		_subStatus = subStatus;
		switch (_mainStatus)
		{
		case MainStatus.Idle:
			_velocity = VInt3.zero;
			_collideBullet.BackToPool();
			break;
		case MainStatus.Move:
			StartPos = _transform.position;
			if (_patrolPaths.Length > 1)
			{
				MoveDis = Vector3.Distance(_patrolPaths[1], _transform.position);
			}
			_velocity = new VInt3((_patrolPaths[1] - _transform.position).normalized) * MoveSpeed * 0.001f;
			break;
		case MainStatus.Skill0:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				LaserFx.Play();
				ShootFrame = GameLogicUpdateManager.GameFrame + (int)(ShootTime * 20f);
				if (AtkPos.x < _transform.position.x)
				{
					UpdateDirection(-1);
				}
				else
				{
					UpdateDirection(1);
				}
				ShotAngle = Vector3.Angle((AtkPos - ShootPos.position).normalized, Vector3.up);
				LaserObj.localRotation = Quaternion.Euler(ShotAngle, 0f, 0f);
				_animator.SetFloat(_HashAngle, ShotAngle);
				break;
			case SubStatus.Phase1:
				ShootFrame = GameLogicUpdateManager.GameFrame + (int)(ShootTime * 20f);
				_collideBullet.Active(targetMask);
				LaserLineFx.Play();
				break;
			case SubStatus.Phase2:
				ShootFrame = GameLogicUpdateManager.GameFrame + (int)(ShootTime * 5f);
				if ((bool)LaserFx)
				{
					LaserFx.Stop();
				}
				if ((bool)LaserLineFx)
				{
					LaserLineFx.Stop();
				}
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
		case MainStatus.Move:
			_currentAnimationId = AnimationID.ANI_IDLE;
			break;
		case MainStatus.Skill0:
			_currentAnimationId = AnimationID.ANI_Skill0;
			break;
		}
		_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
	}

	public override void LogicUpdate()
	{
		if (!Activate)
		{
			return;
		}
		BaseLogicUpdate();
		_currentFrame = _animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
		switch (_mainStatus)
		{
		case MainStatus.Idle:
			if (!CanAtk)
			{
				SetStatus(MainStatus.Move);
			}
			break;
		case MainStatus.Move:
			if (Vector3.Distance(_transform.position, StartPos) > MoveDis)
			{
				CanAtk = true;
				SetStatus(MainStatus.Idle);
			}
			break;
		case MainStatus.Skill0:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if ((float)GameLogicUpdateManager.GameFrame > ShootFrame)
				{
					SetStatus(MainStatus.Skill0, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if ((float)GameLogicUpdateManager.GameFrame > ShootFrame)
				{
					SetStatus(MainStatus.Skill0, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				if ((float)GameLogicUpdateManager.GameFrame > ShootFrame)
				{
					SetStatus(MainStatus.Idle);
				}
				break;
			}
			break;
		}
	}

	public void UpdateFunc()
	{
		if (Activate)
		{
			Vector3 localPosition = _transform.localPosition;
			_transform.localPosition = Vector3.MoveTowards(_transform.localPosition, Controller.LogicPosition.vec3, distanceDelta);
		}
	}

	public override void SetActive(bool isActive)
	{
		base.AllowAutoAim = false;
		base.SetActive(isActive);
		if (isActive)
		{
			IgnoreGravity = true;
			_collideBullet.UpdateBulletData(EnemyWeapons[0].BulletData);
			_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			_collideBullet.Active(targetMask);
			SetColliderEnable(false);
			isPatrol = false;
			CanAtk = false;
			SetStatus(MainStatus.Idle);
		}
		else
		{
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
		_transform.position = pos;
	}

	public void SetAtkPos(Vector3 atkpos)
	{
		if (CanAtk)
		{
			AtkPos = atkpos;
			SetStatus(MainStatus.Skill0);
		}
	}

	public override void SetPatrolPath(bool isLoop, int nMoveSpeed, Vector3[] paths)
	{
		base.SetPatrolPath(isLoop, nMoveSpeed, paths);
		_patrolIsLoop = isLoop;
		if (nMoveSpeed > 0)
		{
			MoveSpeed = nMoveSpeed;
		}
		_patrolPaths = new Vector3[paths.Length];
		for (int i = 0; i < paths.Length; i++)
		{
			_patrolPaths[i] = paths[i];
		}
		if (_patrolPaths.Length != 0)
		{
			isPatrol = true;
		}
	}

	public void SetDie()
	{
		if ((bool)LaserFx)
		{
			LaserFx.Stop();
		}
		if ((bool)LaserLineFx)
		{
			LaserLineFx.Stop();
		}
		HurtPassParam hurtPassParam = new HurtPassParam();
		hurtPassParam.dmg = Hp;
		Hurt(hurtPassParam);
	}
}
