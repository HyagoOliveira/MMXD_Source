using CallbackDefs;
using Newtonsoft.Json;
using UnityEngine;

public class EM175_Controller : EnemyControllerBase, IManagedUpdateBehavior
{
	private enum MainStatus
	{
		Born = 0,
		Fall = 1,
		Land = 2,
		Jump = 3,
		Rotate = 4,
		Shoot = 5,
		Hurt = 6
	}

	private enum SubStatus
	{
		Phase0 = 0,
		MAX_SUBSTATUS = 1
	}

	public enum AnimationID
	{
		ANI_IDLE = 0,
		ANI_FLOWER = 1,
		ANI_HURT = 2,
		MAX_ANIMATION_ID = 3
	}

	private MainStatus _mainStatus;

	private SubStatus _subStatus;

	private AnimationID _currentAnimationId;

	private float _currentFrame;

	private int[] _animationHash;

	[SerializeField]
	private int MoveSpeed = 8000;

	[SerializeField]
	private int JumpSpeed = 12000;

	[SerializeField]
	private ParticleSystem BornFX;

	[SerializeField]
	private float BornFxTime = 0.5f;

	[SerializeField]
	private ParticleSystem FallFX;

	[SerializeField]
	private ParticleSystem LandFX;

	[SerializeField]
	private ParticleSystem RotateFX;

	[SerializeField]
	private float RotateTime = 0.5f;

	[SerializeField]
	private float ShootTime = 5f;

	private int ActionFrame;

	private Vector3 RotateVelocity;

	private void OnEnable()
	{
		MonoBehaviourSingleton<UpdateManager>.Instance.AddUpdate(this);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		MonoBehaviourSingleton<UpdateManager>.Instance.RemoveUpdate(this);
	}

	private void LoadParts(ref Transform[] childs)
	{
		ModelTransform = OrangeBattleUtility.FindChildRecursive(ref childs, "model", true);
		_collideBullet = OrangeBattleUtility.FindChildRecursive(ref childs, "Collider", true).gameObject.AddOrGetComponent<CollideBullet>();
		if (BornFX == null)
		{
			BornFX = OrangeBattleUtility.FindChildRecursive(ref childs, "BornFX", true).GetComponent<ParticleSystem>();
		}
		if (FallFX == null)
		{
			FallFX = OrangeBattleUtility.FindChildRecursive(ref childs, "FallFX", true).GetComponent<ParticleSystem>();
		}
		if (LandFX == null)
		{
			LandFX = OrangeBattleUtility.FindChildRecursive(ref childs, "LandFX", true).GetComponent<ParticleSystem>();
		}
		if (RotateFX == null)
		{
			RotateFX = OrangeBattleUtility.FindChildRecursive(ref childs, "RotateFX", true).GetComponent<ParticleSystem>();
		}
	}

	protected override void Awake()
	{
		base.Awake();
		Transform[] childs = _transform.GetComponentsInChildren<Transform>(true);
		LoadParts(ref childs);
		base.AimPoint = Vector3.zero;
		OrangeBattleUtility.AddEnemyAutoAimSystem(_transform, out _enemyAutoAimSystem);
		_enemyAutoAimSystem.UpdateAimRange(18f);
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

	private void SetStatus(MainStatus mainStatus, SubStatus subStatus = SubStatus.Phase0)
	{
		_mainStatus = mainStatus;
		_subStatus = subStatus;
		switch (_mainStatus)
		{
		case MainStatus.Born:
			_velocity = VInt3.zero;
			IgnoreGravity = true;
			SetColliderEnable(false);
			ActionFrame = GameLogicUpdateManager.GameFrame + (int)(BornFxTime * 20f);
			SwitchFX(BornFX);
			break;
		case MainStatus.Fall:
			IgnoreGravity = false;
			SwitchFX(FallFX);
			break;
		case MainStatus.Land:
			ActionFrame = GameLogicUpdateManager.GameFrame + (int)(BornFxTime * 20f);
			SwitchFX(LandFX);
			SwitchFX(FallFX, false);
			break;
		case MainStatus.Jump:
			SwitchFX(FallFX);
			break;
		case MainStatus.Rotate:
			IgnoreGravity = true;
			_velocity = VInt3.zero;
			Controller.Collider2D.enabled = false;
			Controller.collisionMask = 0;
			ActionFrame = GameLogicUpdateManager.GameFrame + (int)(RotateTime * 20f);
			SwitchFX(RotateFX);
			SwitchFX(FallFX, false);
			break;
		case MainStatus.Shoot:
			ActionFrame = GameLogicUpdateManager.GameFrame + (int)(ShootTime * 20f);
			if (!Target)
			{
				Target = _enemyAutoAimSystem.GetClosetPlayer();
			}
			if ((bool)Target)
			{
				RotateVelocity = (Target.Controller.GetRealCenterPos() - Controller.GetRealCenterPos()).normalized;
			}
			else
			{
				RotateVelocity = new Vector3(base.direction, 1f, 0f).normalized;
			}
			_velocity = new VInt3(RotateVelocity * 0.001f * MoveSpeed);
			break;
		}
		AiTimer.TimerStart();
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
		case MainStatus.Born:
			if (GameLogicUpdateManager.GameFrame > ActionFrame)
			{
				PlaySE("BossSE04", "bs038_mushroom05");
				SwitchFX(BornFX, false);
				SetStatus(MainStatus.Fall);
			}
			break;
		case MainStatus.Fall:
			if (Controller.Collisions.below)
			{
				SetStatus(MainStatus.Land);
			}
			break;
		case MainStatus.Land:
			if (GameLogicUpdateManager.GameFrame > ActionFrame)
			{
				SwitchFX(LandFX, false);
				SetStatus(MainStatus.Jump);
			}
			break;
		case MainStatus.Jump:
			if (Controller.Collisions.below && _velocity.y <= 0)
			{
				SetStatus(MainStatus.Rotate);
			}
			break;
		case MainStatus.Rotate:
			_collideBullet.UpdateBulletData(EnemyWeapons[1].BulletData);
			_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			_collideBullet.Active(targetMask);
			if (GameLogicUpdateManager.GameFrame > ActionFrame)
			{
				SetStatus(MainStatus.Shoot);
			}
			break;
		case MainStatus.Shoot:
			if (GameLogicUpdateManager.GameFrame > ActionFrame)
			{
				Hp = 0;
				Hurt(new HurtPassParam());
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
		Controller.Collider2D.enabled = true;
		Controller.collisionMask = LayerMask.GetMask("Block");
		IgnoreGravity = true;
		if (isActive)
		{
			ModelTransform.localScale = new Vector3(0.8f, 0.75f, 0.8f * (float)base.direction);
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
		ModelTransform.localScale = new Vector3(Mathf.Abs(ModelTransform.localScale.x) * (float)base.direction, ModelTransform.localScale.y, ModelTransform.localScale.z);
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
}
