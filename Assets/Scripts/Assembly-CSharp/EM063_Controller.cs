using System;
using CallbackDefs;
using Newtonsoft.Json;
using UnityEngine;

public class EM063_Controller : EnemyControllerBase, IManagedUpdateBehavior
{
	private enum MainStatus
	{
		Idle = 0,
		Skill1 = 1,
		Skill2 = 2,
		Dance = 3,
		IdleWaitNet = 4
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
		ANI_Skill1 = 1,
		ANI_Skill2 = 2,
		ANI_HURT = 3,
		ANI_DANCE = 4,
		MAX_ANIMATION_ID = 5
	}

	[SerializeField]
	private MainStatus _mainStatus;

	[SerializeField]
	private AnimationID _currentAnimationId;

	[SerializeField]
	private float _currentFrame;

	private int[] _animationHash;

	public int WalkSpeed;

	private readonly int[] AttackCDFrame = new int[3]
	{
		(int)(2.5f / GameLogicUpdateManager.m_fFrameLen),
		(int)(2.5f / GameLogicUpdateManager.m_fFrameLen),
		(int)(4f / GameLogicUpdateManager.m_fFrameLen)
	};

	private int CDOverFrame;

	private int logicFrameNow;

	private bool _isAction;

	private Vector3 LastTargetPos = new Vector3(0f, 0f, 0f);

	private CollideBullet IceSpearCollideBullet;

	public float IceSpearDis = 4f;

	private Transform SpearTransform;

	private ParabolaBulletForTryShoot SnowBall;

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
		_animator = GetComponentInChildren<Animator>();
		_collideBullet = base.gameObject.AddOrGetComponent<CollideBullet>();
		Transform[] target = _transform.GetComponentsInChildren<Transform>(true);
		ModelTransform = OrangeBattleUtility.FindChildRecursive(ref target, "model", true);
		SpearTransform = OrangeBattleUtility.FindChildRecursive(ref target, "SpearPos", true);
		Transform transform = OrangeBattleUtility.FindChildRecursive(ref target, "SpearCollider", true);
		IceSpearCollideBullet = transform.gameObject.AddOrGetComponent<CollideBullet>();
		_animationHash = new int[5];
		_animationHash[0] = Animator.StringToHash("EM063@idle_loop");
		_animationHash[1] = Animator.StringToHash("EM063@skill_01");
		_animationHash[2] = Animator.StringToHash("EM063@skill_02");
		_animationHash[3] = Animator.StringToHash("EM063@hurt_loop");
		_animationHash[4] = Animator.StringToHash("EM063@dance");
		_mainStatus = MainStatus.Idle;
		base.AimPoint = new Vector3(0f, 1f, 0f);
		OrangeBattleUtility.AddEnemyAutoAimSystem(_transform, out _enemyAutoAimSystem);
		_enemyAutoAimSystem.SetColliderType(EnemyAutoAimSystem.ColliderType.Box);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_e_at_000", 3);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxhit_e_at_000", 3);
		FallDownSE = new string[2] { "BattleSE", "bt_ridearmor02" };
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
			UpdateDirection();
		}
		SetStatus((MainStatus)nSet);
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

	private void SetStatus(MainStatus mainStatus, SubStatus subStatus = SubStatus.Phase0)
	{
		_mainStatus = mainStatus;
		switch (_mainStatus)
		{
		case MainStatus.Idle:
			UpdateDirection();
			_velocity.x = 0;
			break;
		case MainStatus.Skill1:
			UpdateDirection();
			break;
		case MainStatus.Skill2:
			UpdateDirection();
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
			break;
		case MainStatus.Skill1:
			_currentAnimationId = AnimationID.ANI_Skill1;
			break;
		case MainStatus.Skill2:
			_currentAnimationId = AnimationID.ANI_Skill2;
			break;
		case MainStatus.Dance:
			_currentAnimationId = AnimationID.ANI_DANCE;
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
		base.LogicUpdate();
		_currentFrame = _animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
		MainStatus mainStatus = MainStatus.Idle;
		switch (_mainStatus)
		{
		case MainStatus.Idle:
			logicFrameNow = GameLogicUpdateManager.GameFrame;
			if (logicFrameNow > CDOverFrame && !bWaitNetStatus)
			{
				if (TrySkill2())
				{
					mainStatus = MainStatus.Skill2;
				}
				else if (TrySkill1())
				{
					mainStatus = MainStatus.Skill1;
				}
			}
			break;
		case MainStatus.Skill1:
			if (_currentFrame > 1f)
			{
				CDOverFrame = logicFrameNow + AttackCDFrame[0];
				SetStatus(MainStatus.Idle);
				_isAction = false;
			}
			else if (_currentFrame > 0.25f && !_isAction)
			{
				ThrowBullet(LastTargetPos);
				_isAction = true;
			}
			break;
		case MainStatus.Skill2:
			if (_currentFrame > 1f)
			{
				IceSpearCollideBullet.BackToPool();
				CDOverFrame = logicFrameNow + AttackCDFrame[1];
				SetStatus(MainStatus.Idle);
				_isAction = false;
			}
			else if (_currentFrame > 0.3f && !_isAction)
			{
				SkillCollideBullet(IceSpearCollideBullet);
				_isAction = true;
			}
			break;
		case MainStatus.Dance:
			if (_currentFrame > 1f)
			{
				_isAction = false;
				CDOverFrame = logicFrameNow + AttackCDFrame[2];
				SetStatus(MainStatus.Idle);
			}
			break;
		}
		if (mainStatus != 0 && CheckHost())
		{
			UploadEnemyStatus((int)mainStatus);
		}
	}

	protected virtual bool TrySkill1()
	{
		Target = _enemyAutoAimSystem.GetClosetPlayer();
		if ((bool)Target)
		{
			TargetPos = new VInt3(Target._transform.position);
			if (Vector2.Angle(new Vector2(Mathf.Abs(Target._transform.position.x - _transform.position.x), Mathf.Abs(Target._transform.position.y - _transform.position.y)), Vector2.right) < 50f)
			{
				LastTargetPos = TargetPos.vec3;
				return true;
			}
		}
		return false;
	}

	protected virtual bool TrySkill2()
	{
		Target = _enemyAutoAimSystem.GetClosetPlayer();
		if ((bool)Target && Mathf.Abs(Target._transform.position.x - _transform.position.x) < IceSpearDis)
		{
			return true;
		}
		return false;
	}

	protected virtual void ThrowBullet(Vector3 EndPoint)
	{
		SKILL_TABLE bulletDatum = EnemyWeapons[1].BulletData;
		SnowBall = BulletBase.TryShotBullet(EnemyWeapons[1].BulletData, _transform.position + new Vector3(0.5f * (float)base.direction, 0.2f, 0f), EndPoint + new Vector3(0f, 1f, 0f), null, selfBuffManager.sBuffStatus, EnemyData, targetMask) as ParabolaBulletForTryShoot;
		PlaySE("WeaponSE", "wep_trw_throw01");
		SnowBall.HitCallback = OnHitPlayer;
	}

	protected virtual void SkillCollideBullet(CollideBullet collidebullet)
	{
		collidebullet.Active(targetMask);
		MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_e_at_000", SpearTransform, Quaternion.Euler(0f, -90f, 0f), new Vector3(0.8f, 0.8f, 0.8f), Array.Empty<object>());
		_isAction = true;
	}

	protected virtual void OnHitPlayer(object obj)
	{
		MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxhit_e_at_000", SnowBall.transform, Quaternion.Euler(0f, 0f, 0f), new Vector3(0.8f, 0.8f, 0.8f), Array.Empty<object>());
		Collider2D collider2D = obj as Collider2D;
		if (collider2D != null && (bool)collider2D.transform.GetComponent<OrangeCharacter>())
		{
			SetStatus(MainStatus.Dance);
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
		_animator.enabled = isActive;
		if (isActive)
		{
			IceSpearCollideBullet.UpdateBulletData(EnemyWeapons[2].BulletData);
			IceSpearCollideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			IceSpearCollideBullet.BackToPool();
			_collideBullet.UpdateBulletData(EnemyWeapons[0].BulletData);
			_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			_collideBullet.Active(targetMask);
			Target = null;
			_isAction = false;
			SetStatus(MainStatus.Idle);
		}
		else
		{
			_collideBullet.BackToPool();
			IceSpearCollideBullet.BackToPool();
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
		ModelTransform.localEulerAngles = new Vector3(0f, 96f, 0f);
		ModelTransform.localScale = new Vector3(ModelTransform.localScale.x, ModelTransform.localScale.y, Mathf.Abs(ModelTransform.localScale.z) * (float)base.direction);
		_transform.position = pos;
	}
}
