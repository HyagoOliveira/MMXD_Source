using CallbackDefs;
using NaughtyAttributes;
using Newtonsoft.Json;
using UnityEngine;

public class EM161_Controller : EnemyControllerBase, IManagedUpdateBehavior
{
	private enum WallSide
	{
		Up = 1,
		Down = 2,
		None = 3
	}

	private enum MainStatus
	{
		Idle = 0,
		Move = 1,
		Born = 2,
		Skill0 = 3,
		Hurt = 4
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
		ANI_FLOWER = 1,
		ANI_HURT = 2,
		MAX_ANIMATION_ID = 3
	}

	[SerializeField]
	[ReadOnly]
	private MainStatus _mainStatus;

	[SerializeField]
	[ReadOnly]
	private SubStatus _subStatus;

	[SerializeField]
	[ReadOnly]
	private AnimationID _currentAnimationId;

	[SerializeField]
	[ReadOnly]
	private float _currentFrame;

	private int[] _animationHash;

	[SerializeField]
	[ReadOnly]
	private WallSide _wallside = WallSide.None;

	[SerializeField]
	private int MoveSpeed = 750;

	[SerializeField]
	private Transform ShootTransform;

	[SerializeField]
	private float IdleTime = 3f;

	private int IdleFrame;

	[SerializeField]
	private ParticleSystem BornFX;

	[SerializeField]
	private float BornFxTime = 0.5f;

	private int ActionFrame;

	[SerializeField]
	private SkinnedMeshRenderer MainMesh;

	[SerializeField]
	private ParticleSystem FlashFX;

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
		_animationHash[0] = Animator.StringToHash("EM161@idle_loop");
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
		case MainStatus.Move:
			_velocity.x = MoveSpeed * base.direction;
			break;
		case MainStatus.Born:
			SetColliderEnable(false);
			ActionFrame = GameLogicUpdateManager.GameFrame + (int)(BornFxTime * 20f);
			MainMesh.enabled = false;
			SwitchFX(BornFX);
			SwitchFX(FlashFX, false);
			break;
		case MainStatus.Skill0:
			_velocity = VInt3.zero;
			if ((bool)Target)
			{
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
		if ((uint)mainStatus <= 3u)
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
				switch (_wallside)
				{
				case WallSide.Up:
					UploadEnemyStatus(3);
					break;
				case WallSide.Down:
					UploadEnemyStatus(1);
					break;
				case WallSide.None:
					break;
				}
			}
			break;
		case MainStatus.Move:
			if (!Target)
			{
				Target = _enemyAutoAimSystem.GetClosetPlayer();
			}
			if ((bool)Target)
			{
				TargetPos = Target.Controller.LogicPosition;
				UpdateDirection();
				_velocity.x = MoveSpeed * base.direction;
			}
			break;
		case MainStatus.Born:
			if (GameLogicUpdateManager.GameFrame > ActionFrame)
			{
				SetColliderEnable(true);
				MainMesh.enabled = true;
				SwitchFX(BornFX, false);
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
		if (isActive)
		{
			ModelTransform.localScale = new Vector3(0.75f, 0.75f, 0.75f);
			_collideBullet.UpdateBulletData(EnemyWeapons[0].BulletData);
			_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			_collideBullet.Active(targetMask);
			SetWallSide(2);
			SetStatus(MainStatus.Born);
		}
		else
		{
			_velocity = VInt3.zero;
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
		ModelTransform.localScale = new Vector3(_transform.localScale.x, _transform.localScale.y, _transform.localScale.z * (float)base.direction);
		_transform.position = pos;
	}

	protected override void UpdateGravity()
	{
		if (!IgnoreGravity)
		{
			WallSide wallside = _wallside;
			if ((uint)(wallside - 1) <= 1u && ((_velocity.y < 0 && Controller.Collisions.below) || (_velocity.y > 0 && Controller.Collisions.above)))
			{
				_velocity.y = 0;
			}
			int num = 0;
			num = OrangeBattleUtility.FP_Gravity * GameLogicUpdateManager.g_fixFrameLenFP / 1000;
			switch (_wallside)
			{
			case WallSide.Up:
				_velocity.y -= num;
				break;
			case WallSide.Down:
				_velocity.y += num;
				break;
			}
		}
	}

	public void SetWallSide(int wallside = 0)
	{
		Vector3 zero = Vector3.zero;
		switch ((WallSide)wallside)
		{
		case WallSide.Up:
			_wallside = (WallSide)wallside;
			Controller.Collider2D.offset = new Vector2(0f, -1f);
			return;
		case WallSide.Down:
			_wallside = (WallSide)wallside;
			Controller.Collider2D.offset = new Vector2(0f, 1f);
			return;
		}
		RaycastHit2D raycastHit2D = Physics2D.Raycast(_transform.position, Vector2.up, 3f, (int)Controller.collisionMask | (int)Controller.collisionMaskThrough);
		RaycastHit2D raycastHit2D2 = Physics2D.Raycast(_transform.position, Vector2.down, 3f, (int)Controller.collisionMask | (int)Controller.collisionMaskThrough);
		float num = 5f;
		if ((bool)raycastHit2D && num > raycastHit2D.distance)
		{
			num = raycastHit2D.distance;
			_wallside = WallSide.Up;
			new Vector3(180f, 90 * -base.direction, 0f);
			Controller.Collider2D.offset = new Vector2(0f, -0.2f);
		}
		if ((bool)raycastHit2D2 && num > raycastHit2D2.distance)
		{
			num = raycastHit2D2.distance;
			_wallside = WallSide.Down;
			new Vector3(0f, 90 * base.direction, 0f);
			Controller.Collider2D.offset = new Vector2(0f, 0.2f);
		}
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
