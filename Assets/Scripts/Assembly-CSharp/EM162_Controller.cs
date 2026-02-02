using CallbackDefs;
using Newtonsoft.Json;
using UnityEngine;

public class EM162_Controller : EnemyControllerBase, IManagedUpdateBehavior
{
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

	private MainStatus _mainStatus;

	private SubStatus _subStatus;

	private AnimationID _currentAnimationId;

	private float _currentFrame;

	private int[] _animationHash;

	[SerializeField]
	private int MoveSpeed = 4000;

	[SerializeField]
	private float MoveLength = 4f;

	private Vector3 StartPos;

	[SerializeField]
	private int StabSpeed = 8000;

	[SerializeField]
	private float ReadyStabTime = 0.5f;

	private int ReadyStabFrame;

	[SerializeField]
	private float IdleTime = 1f;

	private int IdleFrame;

	[SerializeField]
	private ParticleSystem BornFX;

	[SerializeField]
	private float BornFxTime = 0.5f;

	private int ActionFrame;

	[SerializeField]
	private SkinnedMeshRenderer MainMesh;

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
	}

	protected override void Awake()
	{
		base.Awake();
		Transform[] childs = _transform.GetComponentsInChildren<Transform>(true);
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
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_velocity.x = MoveSpeed * base.direction;
				break;
			case SubStatus.Phase1:
				_velocity = VInt3.zero;
				ReadyStabFrame = GameLogicUpdateManager.GameFrame + (int)(ReadyStabTime * 20f);
				break;
			case SubStatus.Phase2:
				_velocity = VInt3.zero;
				IdleFrame = GameLogicUpdateManager.GameFrame + (int)(IdleTime * 20f);
				break;
			}
			break;
		case MainStatus.Born:
			SetColliderEnable(false);
			ActionFrame = GameLogicUpdateManager.GameFrame + (int)(BornFxTime * 20f);
			MainMesh.enabled = false;
			SwitchFX(BornFX);
			break;
		case MainStatus.Skill0:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_velocity = VInt3.zero;
				_velocity.y = -StabSpeed;
				break;
			case SubStatus.Phase1:
				IdleFrame = GameLogicUpdateManager.GameFrame + (int)(IdleTime * 20f);
				break;
			case SubStatus.Phase2:
				_velocity = VInt3.zero;
				_velocity.y = MoveSpeed;
				break;
			}
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
		case MainStatus.Idle:
			Target = _enemyAutoAimSystem.GetClosetPlayer();
			if (!bWaitNetStatus && (bool)Target)
			{
				UploadEnemyStatus(1);
			}
			break;
		case MainStatus.Move:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if ((bool)Physics2D.Raycast(_transform.position + Vector3.right * (Controller.Collider2D.size.x / 2f), Vector2.down, 8f, targetMask))
				{
					SetStatus(MainStatus.Move, SubStatus.Phase1);
				}
				else if ((bool)Physics2D.Raycast(_transform.position + Vector3.left * (Controller.Collider2D.size.x / 2f), Vector2.down, 8f, targetMask))
				{
					SetStatus(MainStatus.Move, SubStatus.Phase1);
				}
				else if ((_transform.position.x - (StartPos.x + MoveLength * (float)base.direction)) * (float)base.direction > 0f)
				{
					base.direction *= -1;
					SetStatus(MainStatus.Move, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase1:
				if (GameLogicUpdateManager.GameFrame > ReadyStabFrame)
				{
					SetStatus(MainStatus.Skill0);
				}
				break;
			case SubStatus.Phase2:
				if (GameLogicUpdateManager.GameFrame > IdleFrame)
				{
					SetStatus(MainStatus.Move);
				}
				break;
			}
			break;
		case MainStatus.Born:
			if (GameLogicUpdateManager.GameFrame > ActionFrame)
			{
				SetColliderEnable(true);
				MainMesh.enabled = true;
				SwitchFX(BornFX, false);
				SetStatus(MainStatus.Idle);
				PlaySE("BossSE04", "bs039_hellsig04");
			}
			break;
		case MainStatus.Skill0:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (Controller.Collisions.below)
				{
					SetStatus(MainStatus.Skill0, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if (GameLogicUpdateManager.GameFrame > IdleFrame)
				{
					SetStatus(MainStatus.Skill0, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				if (_transform.position.y > StartPos.y)
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
		base.SetActive(isActive);
		IgnoreGravity = true;
		if (isActive)
		{
			ModelTransform.localScale = new Vector3(0.75f, 0.75f, 0.75f);
			_collideBullet.UpdateBulletData(EnemyWeapons[0].BulletData);
			_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			_collideBullet.Active(targetMask);
			StartPos = _transform.position;
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
		ModelTransform.localScale = new Vector3(_transform.localScale.x, _transform.localScale.y, _transform.localScale.z * (float)base.direction);
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
