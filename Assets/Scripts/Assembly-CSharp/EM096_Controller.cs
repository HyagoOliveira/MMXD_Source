using CallbackDefs;
using Newtonsoft.Json;
using UnityEngine;

public class EM096_Controller : EnemyLoopBase, IManagedUpdateBehavior
{
	private enum MainStatus
	{
		Idle = 0,
		Move = 1,
		Atk = 2,
		Hurt = 3
	}

	private enum SubStatus
	{
		Phase0 = 0,
		Phase1 = 1,
		Phase2 = 2,
		MAX_SUBSTATUS = 3
	}

	public enum AnimationID
	{
		ANI_IDLE = 0,
		ANI_ATK = 1,
		ANI_HURT = 2,
		MAX_ANIMATION_ID = 3
	}

	private MainStatus _mainStatus;

	private SubStatus _subStatus;

	private AnimationID _currentAnimationId;

	private float _currentFrame;

	private int[] _animationHash;

	private float AimRange = 30f;

	private float distance;

	[SerializeField]
	private int AtkSpeed = 3000;

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
		_animationHash[0] = Animator.StringToHash("EM096@idle_loop");
		_animationHash[1] = Animator.StringToHash("EM096@move_loop");
		_animationHash[2] = Animator.StringToHash("EM096@hurt_loop");
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
		base.AimPoint = Vector3.zero;
		OrangeBattleUtility.AddEnemyAutoAimSystem(_transform, out _enemyAutoAimSystem);
		AiTimer.TimerStart();
		base.SoundSource.Initial(OrangeSSType.ENEMY);
		base.SoundSource.MaxDistance = 16f;
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
			break;
		case MainStatus.Atk:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_velocity = new VInt3(-2000 * base.direction, 4000, 0);
				break;
			case SubStatus.Phase1:
			{
				Vector3 vector = Vector3.right * base.direction;
				Target = _enemyAutoAimSystem.GetClosetPlayer();
				if ((bool)Target)
				{
					vector = (Target._transform.position - _transform.position).normalized;
				}
				_velocity = new VInt3(vector) * ((float)AtkSpeed * 0.001f);
				if (vector.x > 0f)
				{
					UpdateDirection(1);
				}
				else
				{
					UpdateDirection(-1);
				}
				float num = Vector3.Angle(Vector3.up, vector);
				ModelTransform.localRotation = Quaternion.Euler(num % 180f, ModelTransform.localEulerAngles.y, ModelTransform.localEulerAngles.z);
				distance = 30f;
				break;
			}
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
		case MainStatus.Atk:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_IDLE;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_ATK;
				break;
			}
			break;
		case MainStatus.Hurt:
			_currentAnimationId = AnimationID.ANI_IDLE;
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
			if (!bWaitNetStatus && (bool)Target)
			{
				UploadEnemyStatus(2);
			}
			break;
		case MainStatus.Move:
			if (distance <= 0f)
			{
				SetStatus(MainStatus.Idle);
			}
			break;
		case MainStatus.Atk:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_velocity += new VInt3(100 * base.direction, -300, 0);
				if (_velocity.x > 1000 * base.direction && _velocity.y < -3000)
				{
					SetStatus(MainStatus.Atk, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if (distance <= 0f)
				{
					HurtPassParam hurtPassParam = new HurtPassParam();
					hurtPassParam.dmg = MaxHp;
					Hurt(hurtPassParam);
				}
				break;
			}
			break;
		}
	}

	public override void UpdateFunc()
	{
		if (!Activate)
		{
			return;
		}
		base.UpdateFunc();
		Vector3 localPosition = _transform.localPosition;
		_transform.localPosition = Vector3.MoveTowards(_transform.localPosition, Controller.LogicPosition.vec3, distanceDelta);
		switch (_mainStatus)
		{
		case MainStatus.Move:
			distance -= Vector3.Distance(localPosition, _transform.localPosition);
			break;
		case MainStatus.Atk:
			if (_subStatus == SubStatus.Phase1)
			{
				distance -= Vector3.Distance(localPosition, _transform.localPosition);
			}
			break;
		}
	}

	public override void SetActive(bool isActive)
	{
		base.SetActive(isActive);
		if (isActive)
		{
			_enemyAutoAimSystem.UpdateAimRange(AimRange);
			ModelTransform.localEulerAngles = new Vector3(0f, 90 * base.direction, 0f);
			IgnoreGravity = true;
			_collideBullet.UpdateBulletData(EnemyWeapons[0].BulletData);
			_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			_collideBullet.Active(targetMask);
			SetStatus(MainStatus.Idle);
		}
		else
		{
			_collideBullet.BackToPool();
		}
	}

	public void SetAimRange(float range)
	{
		AimRange = range;
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
		ModelTransform.localEulerAngles = new Vector3(0f, 90 * base.direction, 0f);
		_transform.position = pos;
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
		ModelTransform.localEulerAngles = new Vector3(0f, 90 * base.direction, 0f);
	}

	public void MoveToPosition(Vector3 pos)
	{
		distance = Vector3.Distance(pos, _transform.position);
		_velocity = new VInt3((pos - _transform.position).normalized);
		UploadEnemyStatus(1);
	}
}
