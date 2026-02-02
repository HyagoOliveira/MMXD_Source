using CallbackDefs;
using Newtonsoft.Json;
using UnityEngine;

public class EM133_Controller : EnemyControllerBase, IManagedUpdateBehavior
{
	private enum MainStatus
	{
		Idle = 0,
		Hurt = 1,
		Skill0 = 2,
		Skill1 = 3
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
		ANI_MOVE = 1,
		ANI_THROW = 2,
		ANI_HURT = 3,
		MAX_ANIMATION_ID = 4
	}

	private MainStatus _mainStatus;

	private SubStatus _subStatus;

	private AnimationID _currentAnimationId;

	private float _currentFrame;

	private int[] _animationHash;

	private bool isBombMode = true;

	[SerializeField]
	private int MoveSpd = 2000;

	[SerializeField]
	private float SpdMulti = 1.5f;

	[SerializeField]
	private SkinnedMeshRenderer BombMesh;

	[SerializeField]
	private Transform ShootPos;

	[SerializeField]
	private float Skill0WaitTime = 1f;

	[SerializeField]
	private float Skill0Dis = 1f;

	[SerializeField]
	private float Skill0Height = 3f;

	[SerializeField]
	private float BulletDis = 3.5f;

	private int WaitFrame;

	private Vector3 EndPos;

	private Vector3 NowPos
	{
		get
		{
			return _transform.position + Vector3.up;
		}
	}

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
		_animationHash = new int[4];
		_animationHash[0] = Animator.StringToHash("EM133@idle_loop");
		_animationHash[1] = Animator.StringToHash("EM133@move_loop");
		_animationHash[2] = Animator.StringToHash("EM133@throw_bomb");
		_animationHash[3] = Animator.StringToHash("EM133@hurt_loop");
	}

	private void LoadParts(ref Transform[] childs)
	{
		ModelTransform = OrangeBattleUtility.FindChildRecursive(ref childs, "model", true);
		_animator = ModelTransform.GetComponent<Animator>();
		_collideBullet = OrangeBattleUtility.FindChildRecursive(ref childs, "Collider", true).gameObject.AddOrGetComponent<CollideBullet>();
		if (BombMesh == null)
		{
			BombMesh = OrangeBattleUtility.FindChildRecursive(ref childs, "EM133_BodyMesh", true).gameObject.AddOrGetComponent<SkinnedMeshRenderer>();
		}
		if (ShootPos == null)
		{
			ShootPos = OrangeBattleUtility.FindChildRecursive(ref childs, "Bone_Bomb", true);
		}
	}

	protected override void Awake()
	{
		base.Awake();
		Transform[] childs = _transform.GetComponentsInChildren<Transform>(true);
		HashAnimator();
		LoadParts(ref childs);
		base.AimPoint = Vector3.up;
		OrangeBattleUtility.AddEnemyAutoAimSystem(_transform, out _enemyAutoAimSystem);
		_enemyAutoAimSystem.UpdateAimRange(20f);
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
		ModelTransform.localEulerAngles = new Vector3(0f, 90 - base.direction * 20, 0f);
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
			WaitFrame = GameLogicUpdateManager.GameFrame + (int)(Skill0WaitTime * 20f);
			break;
		case MainStatus.Skill0:
		{
			SubStatus subStatus2 = _subStatus;
			if (subStatus2 != 0 && subStatus2 == SubStatus.Phase1)
			{
				_velocity = VInt3.zero;
			}
			break;
		}
		case MainStatus.Skill1:
			if (_subStatus == SubStatus.Phase0)
			{
				BombMesh.enabled = false;
				isBombMode = false;
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
			break;
		case MainStatus.Skill0:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_MOVE;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_THROW;
				break;
			}
			break;
		case MainStatus.Skill1:
			if (_subStatus == SubStatus.Phase0)
			{
				_currentAnimationId = AnimationID.ANI_MOVE;
			}
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
			Target = _enemyAutoAimSystem.GetClosetPlayer();
			if (!bWaitNetStatus && (bool)Target && WaitFrame < GameLogicUpdateManager.GameFrame)
			{
				UploadEnemyStatus(isBombMode ? 2 : 3);
			}
			break;
		case MainStatus.Skill0:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
			{
				EndPos = GetTargetPos();
				UpdateDirection();
				if (!Target)
				{
					SetStatus(MainStatus.Idle);
					break;
				}
				int layerMask = (1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockLayer) | ManagedSingleton<OrangeLayerManager>.Instance.WallKickMask;
				RaycastHit2D raycastHit2D = Physics2D.Raycast(EndPos, Vector2.up, Skill0Height, layerMask);
				EndPos.y = (raycastHit2D ? raycastHit2D.point.y : (EndPos.y + Skill0Height));
				_velocity = new VInt3((EndPos - NowPos).normalized) * MoveSpd * 0.001f;
				if (Vector2.Distance(EndPos, NowPos) < 0.5f)
				{
					SetStatus(MainStatus.Skill0, SubStatus.Phase1);
				}
				break;
			}
			case SubStatus.Phase1:
				if (isBombMode && _currentFrame > 0.4f)
				{
					isBombMode = false;
					BombMesh.enabled = false;
					DropBullet obj = BulletBase.TryShotBullet(EnemyWeapons[1].BulletData, ShootPos, Vector3.right, null, selfBuffManager.sBuffStatus, EnemyData, targetMask) as DropBullet;
					obj.SetVelocity(Vector3.right * base.direction * Skill0Dis);
					obj.FreeDISTANCE = BulletDis;
				}
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Idle);
				}
				break;
			}
			break;
		case MainStatus.Skill1:
			if (_subStatus == SubStatus.Phase0)
			{
				EndPos = GetTargetPos();
				UpdateDirection();
				if (!Target)
				{
					SetStatus(MainStatus.Idle);
				}
				else
				{
					_velocity = new VInt3((EndPos - NowPos).normalized) * MoveSpd * SpdMulti * 0.001f;
				}
			}
			break;
		case MainStatus.Hurt:
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
			IgnoreGravity = true;
			_collideBullet.UpdateBulletData(EnemyWeapons[0].BulletData);
			_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			_collideBullet.Active(targetMask);
			BombMesh.enabled = true;
			isBombMode = true;
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
		ModelTransform.localEulerAngles = new Vector3(0f, 90 - base.direction * 20, 0f);
		ModelTransform.localScale = new Vector3(ModelTransform.localScale.x, ModelTransform.localScale.y, Mathf.Abs(ModelTransform.localScale.z) * (float)base.direction);
		_transform.position = pos;
	}

	private Vector3 GetTargetPos(bool realcenter = false)
	{
		if (!Target)
		{
			Target = _enemyAutoAimSystem.GetClosetPlayer();
		}
		if ((bool)Target)
		{
			if (realcenter)
			{
				TargetPos = new VInt3(Target.Controller.GetRealCenterPos());
			}
			else
			{
				TargetPos = new VInt3(Target.GetTargetPoint() + Vector3.up * 0.15f);
			}
			return TargetPos.vec3;
		}
		return NowPos + Vector3.right * 3f * base.direction;
	}
}
