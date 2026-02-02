using System;
using CallbackDefs;
using Newtonsoft.Json;
using UnityEngine;

public class EM183_Controller : EnemyControllerBase, IManagedUpdateBehavior
{
	private enum MainStatus
	{
		Idle = 0,
		Skill0 = 1,
		Hurt = 2
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

	private Vector3 AtkPos;

	[SerializeField]
	private Transform ShootPos;

	[SerializeField]
	private float ShootTime = 2f;

	[SerializeField]
	private float ShootFrame;

	[SerializeField]
	private float LaserLength = 4f;

	[SerializeField]
	private float LaserSpeed = 2f;

	[SerializeField]
	private float LaserMaxLength = 20f;

	private float LaserScale = 1f;

	private float ShotAngle;

	private readonly int _HashAngle = Animator.StringToHash("Angle");

	[SerializeField]
	private Transform LaserObj;

	private CollideBullet LaserCollide;

	[SerializeField]
	private ParticleSystem LaserFx;

	[SerializeField]
	private ParticleSystem LaserLineFx;

	[Header("待機")]
	[SerializeField]
	private float IdleWaitTime = 1f;

	private int IdleWaitFrame;

	private OrangeTimer HitBlockTimer;

	private int hitCircTick;

	private float ShootDis;

	private bool ThroughWall;

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
			LaserFx = OrangeBattleUtility.FindChildRecursive(ref childs, "fxduring_greenLaser_000_(work)", true).gameObject.AddOrGetComponent<ParticleSystem>();
		}
		if (LaserLineFx == null)
		{
			LaserLineFx = OrangeBattleUtility.FindChildRecursive(ref childs, "loop", true).gameObject.AddOrGetComponent<ParticleSystem>();
		}
		if (LaserObj == null)
		{
			LaserObj = OrangeBattleUtility.FindChildRecursive(ref childs, "LaserCollider", true);
		}
		LaserCollide = LaserObj.gameObject.AddOrGetComponent<CollideBullet>();
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
		HitBlockTimer = OrangeTimerManager.GetTimer();
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
		if (smsg[0] == '{')
		{
			SetStatus((MainStatus)nSet);
		}
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
			LaserCollide.BackToPool();
			LaserLineFx.gameObject.SetActive(false);
			IdleWaitFrame = GameLogicUpdateManager.GameFrame + (int)(IdleWaitTime * 20f);
			break;
		case MainStatus.Skill0:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				LaserFx.transform.localScale = new Vector3(1f, 1f, 0f);
				LaserCollide._transform.localScale = new Vector3(0f, 1f, 1f);
				LaserCollide.Active(targetMask);
				if (!ThroughWall)
				{
					ShootDis = CalcuLaserLength(ShootPos.position, base.direction);
					LaserScale = ShootDis / LaserLength;
					if (LaserScale > LaserMaxLength / LaserLength)
					{
						LaserScale = LaserMaxLength / LaserLength;
					}
				}
				else
				{
					LaserScale = ShootDis / LaserLength;
				}
				LeanTween.value(base.gameObject, 0f, LaserScale, LaserScale / LaserSpeed).setOnUpdate(delegate(float f)
				{
					LaserFx.transform.localScale = new Vector3(1f, 1f, f);
					LaserCollide._transform.localScale = new Vector3(f, 1f, 1f);
				}).setOnComplete((Action)delegate
				{
					LaserFx.transform.localScale = new Vector3(1f, 1f, LaserScale);
					LaserCollide._transform.localScale = new Vector3(LaserScale, 1f, 1f);
					HitBlockTimer.TimerStart();
				});
				LaserFx.Play();
				LaserLineFx.gameObject.SetActive(true);
				break;
			case SubStatus.Phase1:
				ShootFrame = GameLogicUpdateManager.GameFrame + (int)(ShootTime * 20f);
				break;
			case SubStatus.Phase2:
				HitBlockTimer.TimerStop();
				LaserCollide.BackToPool();
				ShootFrame = GameLogicUpdateManager.GameFrame + (int)(ShootTime * 5f);
				if ((bool)LaserFx)
				{
					LaserFx.Stop();
				}
				LaserLineFx.gameObject.SetActive(false);
				break;
			}
			break;
		}
		AiTimer.TimerStart();
		UpdateAnimation();
	}

	private void UpdateAnimation()
	{
	}

	public override void LogicUpdate()
	{
		if (!Activate)
		{
			return;
		}
		BaseLogicUpdate();
		switch (_mainStatus)
		{
		case MainStatus.Idle:
			if (!bWaitNetStatus && IdleWaitFrame < GameLogicUpdateManager.GameFrame)
			{
				Target = _enemyAutoAimSystem.GetClosetPlayer();
				if ((bool)Target && CheckHost())
				{
					UploadEnemyStatus(1);
				}
			}
			break;
		case MainStatus.Skill0:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (LaserFx.transform.localScale.z >= LaserScale)
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
			if (HitBlockTimer.GetMillisecond() > hitCircTick)
			{
				HitBlockTimer.TimerStart();
				base.SoundSource.PlaySE("HitSE", "ht_guard03");
			}
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
			LaserCollide.UpdateBulletData(EnemyWeapons[1].BulletData);
			LaserCollide.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			if (AiState == AI_STATE.mob_001)
			{
				SetColliderEnable(false);
			}
			string[] array = EnemyWeapons[1].BulletData.s_FIELD.Split(',');
			hitCircTick = int.Parse(array[6]);
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
		ModelTransform.localScale = new Vector3(_transform.localScale.x * (float)base.direction, _transform.localScale.y, _transform.localScale.z);
		_transform.position = pos;
	}

	public override void SetPatrolPath(bool isLoop, int nMoveSpeed, Vector3[] paths)
	{
	}

	public override void SetFloatParameter(float param)
	{
		ShootDis = param;
		if (ShootDis != 0f)
		{
			ThroughWall = true;
		}
	}

	public void SetDie()
	{
		if ((bool)LaserFx)
		{
			LaserFx.Stop();
		}
		LaserLineFx.gameObject.SetActive(false);
		HurtPassParam hurtPassParam = new HurtPassParam();
		hurtPassParam.dmg = Hp;
		Hurt(hurtPassParam);
	}

	private float CalcuLaserLength(Vector3 RayOrigin, int direct = 1)
	{
		int layerMask = 1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockLayer;
		RaycastHit2D raycastHit2D = OrangeBattleUtility.RaycastIgnoreSelf(RayOrigin, Vector3.right * direct, 20f, layerMask, _transform);
		if ((bool)raycastHit2D)
		{
			return Mathf.Abs(raycastHit2D.point.x - ShootPos.position.x);
		}
		return 20f;
	}

	public override void UpdateEnemyID(int id)
	{
		base.UpdateEnemyID(id);
		UpdateAIState();
	}
}
