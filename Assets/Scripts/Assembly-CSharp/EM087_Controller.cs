using CallbackDefs;
using Newtonsoft.Json;
using StageLib;
using UnityEngine;

public class EM087_Controller : EnemyControllerBase, IManagedUpdateBehavior
{
	private enum MainStatus
	{
		Idle = 0,
		Summon = 1,
		Hurt = 2,
		IdleWaitNet = 3
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
		ANI_OPEN_LOOP = 1,
		ANI_CLOSE = 2,
		ANI_OPEN = 3,
		ANI_HURT = 4,
		MAX_ANIMATION_ID = 5
	}

	private MainStatus _mainStatus;

	private SubStatus _subStatus;

	private AnimationID _currentAnimationId;

	private float _currentFrame;

	private int[] _animationHash;

	[SerializeField]
	private int FirstSummon = 5;

	[SerializeField]
	private float SummonTime = 5f;

	[SerializeField]
	private int SummonNums = 2;

	private int SummonFrame;

	private bool CanSummon;

	[SerializeField]
	private float OpenDoorTime = 1f;

	private int OpenFrame;

	private int BeeNetID;

	[SerializeField]
	private Transform BeePosL;

	[SerializeField]
	private Transform BeePosR;

	private float SummonAngle;

	private int BornLR = 1;

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
		_animationHash = new int[5];
		_animationHash[0] = Animator.StringToHash("EM087@door_closed_loop");
		_animationHash[3] = Animator.StringToHash("EM087@open_door");
		_animationHash[1] = Animator.StringToHash("EM087@door_opened_loop");
		_animationHash[2] = Animator.StringToHash("EM087@close_door");
	}

	private void LoadParts(ref Transform[] childs)
	{
		ModelTransform = OrangeBattleUtility.FindChildRecursive(ref childs, "model", true);
		_animator = ModelTransform.GetComponent<Animator>();
		_collideBullet = OrangeBattleUtility.FindChildRecursive(ref childs, "Collider", true).gameObject.AddOrGetComponent<CollideBullet>();
		if (BeePosL == null)
		{
			BeePosL = OrangeBattleUtility.FindChildRecursive(ref childs, "Bone002", true);
		}
		if (BeePosR == null)
		{
			BeePosR = OrangeBattleUtility.FindChildRecursive(ref childs, "Bone003", true);
		}
	}

	protected override void Awake()
	{
		base.Awake();
		Transform[] childs = _transform.GetComponentsInChildren<Transform>(true);
		HashAnimator();
		LoadParts(ref childs);
		base.AimPoint = Vector3.up * 1.5f;
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
			break;
		case MainStatus.Summon:
			switch (_subStatus)
			{
			case SubStatus.Phase1:
				SummonBee(SummonNums);
				OpenFrame = GameLogicUpdateManager.GameFrame + (int)(OpenDoorTime * 20f);
				break;
			case SubStatus.Phase3:
				SummonFrame = GameLogicUpdateManager.GameFrame + (int)(SummonTime * 20f);
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
			_currentAnimationId = AnimationID.ANI_IDLE;
			break;
		case MainStatus.Summon:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_OPEN;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_OPEN_LOOP;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_CLOSE;
				break;
			case SubStatus.Phase3:
				_currentAnimationId = AnimationID.ANI_IDLE;
				break;
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
		if (IsStun)
		{
			return;
		}
		_currentFrame = _animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
		switch (_mainStatus)
		{
		case MainStatus.Idle:
			Target = _enemyAutoAimSystem.GetClosetPlayer();
			if (!bWaitNetStatus && (bool)Target)
			{
				SummonBee(FirstSummon);
				UploadEnemyStatus(1);
			}
			break;
		case MainStatus.Summon:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Summon, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if (OpenFrame < GameLogicUpdateManager.GameFrame)
				{
					SetStatus(MainStatus.Summon, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Summon, SubStatus.Phase3);
				}
				break;
			case SubStatus.Phase3:
				if (SummonFrame < GameLogicUpdateManager.GameFrame)
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
		if (isActive)
		{
			RaycastHit2D raycastHit2D = OrangeBattleUtility.RaycastIgnoreSelf(_transform.position, Vector2.up, 8f, LayerMask.GetMask("Block", "SemiBlock"), _transform);
			if ((bool)raycastHit2D)
			{
				_transform.position = raycastHit2D.point + Vector2.down * 2.88f;
				Controller.LogicPosition = new VInt3(_transform.position);
			}
			IgnoreGravity = true;
			CanSummon = true;
			_collideBullet.UpdateBulletData(EnemyWeapons[0].BulletData);
			_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			_collideBullet.Active(targetMask);
			SetStatus(MainStatus.Idle);
		}
		else
		{
			CanSummon = false;
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
		_transform.position = pos;
	}

	private void SummonBee(int num)
	{
		if (!CanSummon)
		{
			return;
		}
		for (int i = 0; i < num; i++)
		{
			EnemyControllerBase enemyControllerBase = StageUpdate.StageSpawnEnemyByMob(ManagedSingleton<OrangeDataManager>.Instance.MOB_TABLE_DICT[(int)EnemyWeapons[1].BulletData.f_EFFECT_X], sNetSerialID + BeeNetID, 16);
			BeeNetID++;
			EM096_Controller obj = enemyControllerBase as EM096_Controller;
			bool bBack = base.direction == -1;
			Target = _enemyAutoAimSystem.GetClosetPlayer();
			if ((bool)Target)
			{
				bBack = !(Target._transform.position.x > _transform.position.x);
			}
			Vector3 position = BeePosL.position;
			if (BornLR == 1)
			{
				position = BeePosR.position;
			}
			Vector3 vector = position;
			vector = Quaternion.Euler(0f, 0f, SummonAngle) * Vector3.up * BornLR + _transform.position;
			obj.SetPositionAndRotation(position, bBack);
			obj.SetAimRange(_enemyAutoAimSystem.Range.x + 8f);
			obj.SetActive(true);
			obj.MoveToPosition(vector);
			BornLR *= -1;
			SummonAngle = (SummonAngle + 18f) % 180f;
		}
	}
}
