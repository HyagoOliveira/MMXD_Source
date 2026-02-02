#define RELEASE
using CallbackDefs;
using Newtonsoft.Json;
using StageLib;
using UnityEngine;

public class EM167_Controller : EnemyControllerBase, IManagedUpdateBehavior
{
	private enum MainStatus
	{
		Idle = 0,
		Debut = 1,
		Skill0 = 2,
		Skill1 = 3,
		Die = 4,
		Hurt = 5
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
		ANI_SKILL0_START = 1,
		ANI_SKILL0_LOOP = 2,
		ANI_SKILL0_END = 3,
		ANI_SKILL1_START1 = 4,
		ANI_SKILL1_LOOP1 = 5,
		ANI_SKILL1_START2 = 6,
		ANI_SKILL1_LOOP2 = 7,
		ANI_SKILL1_END2 = 8,
		ANI_HURT = 9,
		MAX_ANIMATION_ID = 10
	}

	private MainStatus _mainStatus;

	private SubStatus _subStatus;

	private AnimationID _currentAnimationId;

	private float _currentFrame;

	private int[] _animationHash;

	private BossCorpsTool CorpsTool;

	private BS094_Controller boss;

	[SerializeField]
	private float IdleWaitTime = 2f;

	private int IdleWaitFrame;

	private float ShootFrame;

	private bool HasShot;

	private Vector3 ShotPos;

	private Vector3 EndPos;

	[Header("登場")]
	[SerializeField]
	private float DebutTime1 = 0.3f;

	[SerializeField]
	private float DebutTime2 = 0.2f;

	private int DebutFrame;

	[SerializeField]
	private ParticleSystem Debut1;

	[SerializeField]
	private ParticleSystem Debut2;

	[Header("射擊")]
	[SerializeField]
	private int Skill0ShootTimes = 3;

	[SerializeField]
	private Transform TailShotPos;

	[Header("衝刺")]
	[SerializeField]
	private int RushSpeed = 9000;

	[SerializeField]
	private ParticleSystem RushFX;

	[Header("退場")]
	[SerializeField]
	private float LeaveTime1 = 0.3f;

	[SerializeField]
	private float LeaveTime2 = 0.2f;

	private int LeaveFrame;

	[SerializeField]
	private ParticleSystem Leave1;

	[SerializeField]
	private ParticleSystem Leave2;

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
		_animationHash = new int[10];
		_animationHash[0] = Animator.StringToHash("EM167@idle_loop");
		_animationHash[1] = Animator.StringToHash("EM167@skill_01_start");
		_animationHash[2] = Animator.StringToHash("EM167@skill_01_loop");
		_animationHash[3] = Animator.StringToHash("EM167@skill_01_end");
		_animationHash[4] = Animator.StringToHash("EM167@skill_02_step1_start");
		_animationHash[5] = Animator.StringToHash("EM167@skill_02_step1_loop");
		_animationHash[6] = Animator.StringToHash("EM167@skill_02_step2_start");
		_animationHash[7] = Animator.StringToHash("EM167@skill_02_step2_loop");
		_animationHash[8] = Animator.StringToHash("EM167@skill_02_step2_end");
		_animationHash[9] = Animator.StringToHash("EM167@hurt_loop");
	}

	private void LoadParts(ref Transform[] childs)
	{
		ModelTransform = OrangeBattleUtility.FindChildRecursive(ref childs, "model", true);
		_animator = ModelTransform.GetComponent<Animator>();
		_collideBullet = OrangeBattleUtility.FindChildRecursive(ref childs, "Collider", true).gameObject.AddOrGetComponent<CollideBullet>();
		base.AimTransform = OrangeBattleUtility.FindChildRecursive(ref childs, "Bip001 Neck", true);
		if (TailShotPos == null)
		{
			TailShotPos = OrangeBattleUtility.FindChildRecursive(ref childs, "tail004_nub", true);
		}
		if (Debut1 == null)
		{
			Debut1 = OrangeBattleUtility.FindChildRecursive(ref childs, "Debut1", true).GetComponent<ParticleSystem>();
		}
		if (Debut2 == null)
		{
			Debut2 = OrangeBattleUtility.FindChildRecursive(ref childs, "Debut2", true).GetComponent<ParticleSystem>();
		}
		if (RushFX == null)
		{
			RushFX = OrangeBattleUtility.FindChildRecursive(ref childs, "RushFX", true).GetComponent<ParticleSystem>();
		}
		if (Leave1 == null)
		{
			Leave1 = OrangeBattleUtility.FindChildRecursive(ref childs, "Leave1", true).GetComponent<ParticleSystem>();
		}
		if (Leave2 == null)
		{
			Leave2 = OrangeBattleUtility.FindChildRecursive(ref childs, "Leave2", true).GetComponent<ParticleSystem>();
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
		_enemyAutoAimSystem.UpdateAimRange(15f);
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
		case MainStatus.Debut:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				base.AllowAutoAim = false;
				SwitchFx(Debut1, true);
				SetColliderEnable(false);
				DebutFrame = GameLogicUpdateManager.GameFrame + (int)(DebutTime1 * 20f);
				break;
			case SubStatus.Phase1:
				SwitchFx(Debut2, true);
				DebutFrame = GameLogicUpdateManager.GameFrame + (int)(DebutTime2 * 20f);
				break;
			}
			break;
		case MainStatus.Idle:
			base.AllowAutoAim = true;
			_velocity = VInt3.zero;
			IdleWaitFrame = GameLogicUpdateManager.GameFrame + (int)(IdleWaitTime * 20f);
			break;
		case MainStatus.Skill0:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_velocity = VInt3.zero;
				if (!Target)
				{
					Target = _enemyAutoAimSystem.GetClosetPlayer();
				}
				if ((bool)Target)
				{
					TargetPos = new VInt3(Target.Controller.GetRealCenterPos());
				}
				else
				{
					TargetPos = new VInt3(_transform.position + Vector3.right * 4f * base.direction);
				}
				EndPos = TargetPos.vec3;
				UpdateDirection();
				break;
			case SubStatus.Phase1:
				ShootFrame = 0.75f;
				HasShot = false;
				break;
			}
			break;
		case MainStatus.Skill1:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_velocity = VInt3.zero;
				break;
			case SubStatus.Phase3:
				SwitchFx(RushFX, true);
				if (!Target)
				{
					Target = _enemyAutoAimSystem.GetClosetPlayer();
				}
				if ((bool)Target)
				{
					TargetPos = new VInt3(Target.Controller.GetRealCenterPos());
					UpdateDirection();
				}
				else
				{
					UpdateDirection(-base.direction);
				}
				_velocity.x = RushSpeed * base.direction;
				_collideBullet.UpdateBulletData(EnemyWeapons[2].BulletData);
				_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
				_collideBullet.Active(targetMask);
				break;
			case SubStatus.Phase4:
				_velocity = VInt3.zero;
				_collideBullet.UpdateBulletData(EnemyWeapons[0].BulletData);
				_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
				_collideBullet.Active(targetMask);
				break;
			}
			break;
		case MainStatus.Die:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				base.AllowAutoAim = false;
				_velocity = VInt3.zero;
				SetColliderEnable(false);
				_characterMaterial.Disappear();
				SwitchFx(Leave2, true);
				LeaveFrame = GameLogicUpdateManager.GameFrame + (int)(LeaveTime2 * 20f);
				break;
			case SubStatus.Phase1:
				SwitchFx(Leave1, true);
				LeaveFrame = GameLogicUpdateManager.GameFrame + (int)(LeaveTime1 * 20f);
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
		case MainStatus.Debut:
			if (_subStatus == SubStatus.Phase0)
			{
				_currentAnimationId = AnimationID.ANI_IDLE;
				break;
			}
			return;
		case MainStatus.Idle:
			_currentAnimationId = AnimationID.ANI_IDLE;
			break;
		case MainStatus.Skill0:
			switch (_subStatus)
			{
			default:
				return;
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_SKILL0_START;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_SKILL0_LOOP;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_SKILL0_END;
				break;
			}
			break;
		case MainStatus.Skill1:
			switch (_subStatus)
			{
			default:
				return;
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_SKILL1_START1;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_SKILL1_LOOP1;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_SKILL1_START2;
				break;
			case SubStatus.Phase3:
				_currentAnimationId = AnimationID.ANI_SKILL1_LOOP2;
				break;
			case SubStatus.Phase4:
				_currentAnimationId = AnimationID.ANI_SKILL1_END2;
				break;
			}
			break;
		case MainStatus.Die:
			if (_subStatus == SubStatus.Phase0)
			{
				_currentAnimationId = AnimationID.ANI_IDLE;
				break;
			}
			return;
		}
		_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
	}

	private void UpdateRandomState(MainStatus status = MainStatus.Idle)
	{
		MainStatus mainStatus = status;
		if (status == MainStatus.Idle && _mainStatus == MainStatus.Idle)
		{
			mainStatus = ((OrangeBattleUtility.Random(0, 20) < 10) ? MainStatus.Skill0 : MainStatus.Skill1);
		}
		if (mainStatus != 0 && CheckHost())
		{
			UploadEnemyStatus((int)mainStatus);
		}
	}

	public override void LogicUpdate()
	{
		if ((int)boss.Hp <= 0 && _mainStatus != MainStatus.Die)
		{
			HurtPassParam hurtPassParam = new HurtPassParam();
			DeadBehavior(ref hurtPassParam);
		}
		if (!Activate)
		{
			return;
		}
		BaseLogicUpdate();
		_currentFrame = _animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
		switch (_mainStatus)
		{
		case MainStatus.Debut:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (GameLogicUpdateManager.GameFrame > DebutFrame)
				{
					SwitchFx(Debut1, false);
					SetColliderEnable(true);
					SetStatus(MainStatus.Debut, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if (GameLogicUpdateManager.GameFrame > DebutFrame)
				{
					SwitchFx(Debut2, false);
					SetStatus(MainStatus.Debut, SubStatus.Phase2);
					_characterMaterial.Appear();
					SetStatus(MainStatus.Idle);
				}
				break;
			}
			break;
		case MainStatus.Idle:
			if (!bWaitNetStatus && IdleWaitFrame < GameLogicUpdateManager.GameFrame)
			{
				Target = _enemyAutoAimSystem.GetClosetPlayer();
				if ((bool)Target)
				{
					UpdateDirection();
					UpdateRandomState();
				}
				else
				{
					UpdateRandomState(MainStatus.Skill1);
				}
			}
			break;
		case MainStatus.Skill0:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill0, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if (!HasShot && _currentFrame > ShootFrame)
				{
					HasShot = true;
					ShotPos = TailShotPos.position;
					BulletBase.TryShotBullet(EnemyWeapons[1].BulletData, ShotPos + Vector3.right * base.direction * 0.5f, EndPos - ShotPos, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
				}
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Idle);
				}
				break;
			case SubStatus.Phase2:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Idle);
				}
				break;
			}
			break;
		case MainStatus.Skill1:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill1, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill1, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill1, SubStatus.Phase3);
				}
				break;
			case SubStatus.Phase3:
				if (Controller.Collisions.right || Controller.Collisions.left)
				{
					SwitchFx(RushFX, false);
					Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.CAMERA_SHAKE, 2f, false);
					SetStatus(MainStatus.Skill1, SubStatus.Phase4);
				}
				break;
			case SubStatus.Phase4:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Idle);
				}
				break;
			}
			break;
		case MainStatus.Die:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (GameLogicUpdateManager.GameFrame > LeaveFrame)
				{
					SwitchFx(Leave2, false);
					SetStatus(MainStatus.Die, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if (GameLogicUpdateManager.GameFrame > LeaveFrame)
				{
					BackToPool();
					SwitchFx(Leave1, false);
					SetStatus(MainStatus.Die, SubStatus.Phase2);
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
		base.SetActive(isActive);
		if (isActive)
		{
			_collideBullet.UpdateBulletData(EnemyWeapons[0].BulletData);
			_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			_collideBullet.Active(targetMask);
			if (CorpsTool == null || CorpsTool.Member == null)
			{
				foreach (StageUpdate.EnemyCtrlID runEnemy in StageUpdate.runEnemys)
				{
					if ((bool)runEnemy.mEnemy && runEnemy.mEnemy is BS053_Controller)
					{
						BS094_Controller bS094_Controller = runEnemy.mEnemy as BS094_Controller;
						if ((bool)bS094_Controller)
						{
							bS094_Controller.SetHasSpawn();
						}
					}
				}
				if (CorpsTool.Member != null)
				{
					CorpsTool.SetDebutOver();
				}
			}
			_characterMaterial.Disappear();
			SetStatus(MainStatus.Debut);
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
		ModelTransform.localScale = new Vector3(ModelTransform.localScale.x, ModelTransform.localScale.y, Mathf.Abs(ModelTransform.localScale.z) * (float)base.direction);
		_transform.position = pos;
	}

	public void JoinCorps(BossCorpsTool corps, BS094_Controller bs)
	{
		boss = bs;
		CorpsTool = corps;
	}

	protected override void DeadBehavior(ref HurtPassParam tHurtPassParam)
	{
		if (_mainStatus != MainStatus.Die)
		{
			if ((bool)_collideBullet)
			{
				_collideBullet.BackToPool();
			}
			SetColliderEnable(false);
			if ((bool)Debut1)
			{
				Debut1.Stop();
			}
			if ((bool)Debut2)
			{
				Debut2.Stop();
			}
			if ((bool)RushFX)
			{
				RushFX.Stop();
			}
			SetStatus(MainStatus.Die);
		}
	}

	public override void BackToPool()
	{
		if (CorpsTool != null)
		{
			CorpsTool.RemoveFromList(0);
		}
		base.BackToPool();
	}

	private void SwitchFx(ParticleSystem Fx, bool onoff)
	{
		if ((bool)Fx)
		{
			if (onoff)
			{
				Fx.Play();
			}
			else
			{
				Fx.Stop();
			}
		}
		else
		{
			Debug.Log("特效載入有誤，目前狀態是 " + _mainStatus);
		}
	}
}
