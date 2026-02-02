using CallbackDefs;
using Newtonsoft.Json;
using UnityEngine;

public class EM160_Controller : EnemyControllerBase, IManagedUpdateBehavior
{
	private enum MainStatus
	{
		Idle = 0,
		Born = 1,
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
		ANI_SHOOT = 1,
		ANI_HURT = 2,
		MAX_ANIMATION_ID = 3
	}

	private MainStatus _mainStatus;

	private SubStatus _subStatus;

	private AnimationID _currentAnimationId;

	private float _currentFrame;

	private int[] _animationHash;

	[SerializeField]
	private Transform ShootTransform;

	[SerializeField]
	private float IdleTime = 3f;

	[SerializeField]
	private ParticleSystem BornFX;

	[SerializeField]
	private float BornFxTime = 0.5f;

	private int ActionFrame;

	[SerializeField]
	private SkinnedMeshRenderer MainMesh;

	[SerializeField]
	private ParticleSystem RocketFX;

	[SerializeField]
	private ParticleSystem FlashFX;

	private int IdleFrame;

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
		_animationHash[0] = Animator.StringToHash("EM160@idle_loop");
	}

	private void LoadParts(ref Transform[] childs)
	{
		ModelTransform = OrangeBattleUtility.FindChildRecursive(ref childs, "model", true);
		_animator = ModelTransform.GetComponent<Animator>();
		_collideBullet = OrangeBattleUtility.FindChildRecursive(ref childs, "Collider", true).gameObject.AddOrGetComponent<CollideBullet>();
		if (ShootTransform == null)
		{
			ShootTransform = OrangeBattleUtility.FindChildRecursive(ref childs, "ShootTransform", true);
		}
	}

	protected override void Awake()
	{
		base.Awake();
		Transform[] childs = _transform.GetComponentsInChildren<Transform>(true);
		HashAnimator();
		LoadParts(ref childs);
		base.AimPoint = new Vector3(0f, 0.5f, 0f);
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
		ModelTransform.localEulerAngles = new Vector3(0f, 90 + base.direction * 45, 0f);
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
			IdleFrame = GameLogicUpdateManager.GameFrame + (int)(IdleTime * 20f);
			break;
		case MainStatus.Born:
			SetColliderEnable(false);
			ActionFrame = GameLogicUpdateManager.GameFrame + (int)(BornFxTime * 20f);
			MainMesh.enabled = false;
			SwitchFX(BornFX);
			SwitchFX(RocketFX, false);
			SwitchFX(FlashFX, false);
			break;
		case MainStatus.Skill0:
			_velocity = VInt3.zero;
			if ((bool)Target)
			{
				TargetPos = Target.Controller.LogicPosition;
				UpdateDirection();
				BulletBase.TryShotBullet(EnemyWeapons[1].BulletData, ShootTransform, Target.Controller.GetRealCenterPos() - ShootTransform.position, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
			}
			break;
		}
		AiTimer.TimerStart();
		UpdateAnimation();
	}

	private void UpdateAnimation()
	{
		MainStatus mainStatus = _mainStatus;
		if ((uint)mainStatus <= 2u)
		{
			_currentAnimationId = AnimationID.ANI_IDLE;
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
			Target = _enemyAutoAimSystem.GetClosetPlayer();
			if (!bWaitNetStatus && (bool)Target && GameLogicUpdateManager.GameFrame > IdleFrame)
			{
				UploadEnemyStatus(2);
			}
			break;
		case MainStatus.Born:
			if (GameLogicUpdateManager.GameFrame > ActionFrame)
			{
				SetColliderEnable(true);
				MainMesh.enabled = true;
				SwitchFX(BornFX, false);
				SwitchFX(RocketFX);
				SwitchFX(FlashFX);
				SetStatus(MainStatus.Idle);
				PlaySE("BossSE04", "bs039_hellsig04");
			}
			break;
		case MainStatus.Skill0:
			SetStatus(MainStatus.Idle);
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
		IgnoreGravity = true;
		if (isActive)
		{
			ModelTransform.localScale = new Vector3(0.75f, 0.75f, 0.75f);
			_collideBullet.UpdateBulletData(EnemyWeapons[0].BulletData);
			_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			_collideBullet.Active(targetMask);
			SetStatus(MainStatus.Born);
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
		ModelTransform.localEulerAngles = new Vector3(0f, 90 + base.direction * 45, 0f);
		ModelTransform.localScale = new Vector3(ModelTransform.localScale.x, ModelTransform.localScale.y, Mathf.Abs(ModelTransform.localScale.z) * (float)base.direction);
		_transform.position = pos;
	}

	private void SwitchFX(ParticleSystem fx, bool onoff = true)
	{
		if ((bool)fx)
		{
			if (onoff)
			{
				fx.Play();
			}
			else
			{
				fx.Stop();
			}
		}
	}

	public void SetDie()
	{
		HurtPassParam hurtPassParam = new HurtPassParam();
		hurtPassParam.dmg = Hp;
		Hurt(hurtPassParam);
	}
}
