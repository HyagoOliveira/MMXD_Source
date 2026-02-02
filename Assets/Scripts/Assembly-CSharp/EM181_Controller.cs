#define RELEASE
using CallbackDefs;
using Newtonsoft.Json;
using UnityEngine;

public class EM181_Controller : EnemyControllerBase, IManagedUpdateBehavior
{
	private enum MainStatus
	{
		Idle = 0,
		Debut = 1,
		Skill0 = 2,
		Skill1 = 3,
		Die = 4,
		IdleWaitNet = 5
	}

	private enum SubStatus
	{
		Phase0 = 0,
		Phase1 = 1,
		Phase2 = 2,
		Phase3 = 3,
		Phase4 = 4,
		Phase5 = 5,
		Phase6 = 6,
		Phase7 = 7,
		MAX_SUBSTATUS = 8
	}

	public enum AnimationID
	{
		ANI_IDLE = 0,
		ANI_DEBUT = 1,
		ANI_Skill1_START1 = 2,
		ANI_Skill1_LOOP1 = 3,
		ANI_Skill1_START2 = 4,
		ANI_Skill1_END = 5,
		ANI_DEAD = 6,
		MAX_ANIMATION_ID = 7
	}

	private enum SpikeAnimationID
	{
		ANI_ON = 0,
		ANI_OFF = 1,
		MAX_ID = 2
	}

	private MainStatus _mainStatus;

	private SubStatus _subStatus;

	private AnimationID _currentAnimationId;

	private float _currentFrame;

	private int[] _animationHash;

	[SerializeField]
	private int MoveSpeed = 5000;

	[SerializeField]
	private int BackSpeed = 6000;

	private float distance;

	private Vector3 StartPos;

	private Vector3 EndPos;

	[SerializeField]
	private ParticleSystem BoosterFX;

	[SerializeField]
	private SkinnedMeshRenderer HandMesh;

	private Transform BossHand;

	private BossCorpsTool CorpsTool;

	private MainStatus NextSkill;

	[SerializeField]
	private float WaitTime;

	private int WaitFrame;

	private Vector3 AtkVelocity = Vector3.right;

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
		if (HandMesh == null)
		{
			HandMesh = OrangeBattleUtility.FindChildRecursive(ref childs, "model", true).GetComponent<SkinnedMeshRenderer>();
		}
		if (BoosterFX == null)
		{
			BoosterFX = OrangeBattleUtility.FindChildRecursive(ref childs, "BoosterFX", true).GetComponent<ParticleSystem>();
		}
	}

	protected override void Awake()
	{
		base.Awake();
		Transform[] childs = _transform.GetComponentsInChildren<Transform>(true);
		LoadParts(ref childs);
		_mainStatus = MainStatus.Idle;
		base.AimPoint = new Vector3(0f, 0f, 0f);
		base.AllowAutoAim = false;
		OrangeBattleUtility.AddEnemyAutoAimSystem(_transform, out _enemyAutoAimSystem);
		_enemyAutoAimSystem.UpdateAimRange(20f, 20f);
		StageObjParam[] componentsInChildren = GetComponentsInChildren<StageObjParam>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].tLinkSOB = null;
		}
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
		ModelTransform.rotation = Quaternion.Euler(0f, 90 * base.direction, 0f);
	}

	private void SetStatus(MainStatus mainStatus, SubStatus subStatus = SubStatus.Phase0)
	{
		_mainStatus = mainStatus;
		_subStatus = subStatus;
		switch (_mainStatus)
		{
		case MainStatus.Debut:
			_velocity = VInt3.zero;
			break;
		case MainStatus.Idle:
			_velocity = VInt3.zero;
			break;
		case MainStatus.Skill0:
			if (_subStatus != 0)
			{
				break;
			}
			if ((bool)BoosterFX)
			{
				BoosterFX.Play();
			}
			if (Target == null)
			{
				Target = _enemyAutoAimSystem.GetClosetPlayer();
			}
			if ((bool)Target)
			{
				TargetPos = new VInt3(Target._transform.position);
				AtkVelocity = (Target.Controller.GetCenterPos().xy() - _transform.position.xy()).normalized;
				if (AtkVelocity.x > 0f)
				{
					AtkVelocity = new Vector3(0f, AtkVelocity.y, 0f).normalized;
				}
			}
			else
			{
				AtkVelocity = Vector3.right * base.direction;
			}
			_velocity = new VInt3(AtkVelocity) * MoveSpeed * 0.001f;
			StartPos = _transform.position;
			PlaySE("BossSE04", "bs037_inary01");
			break;
		case MainStatus.Skill1:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
			{
				if ((bool)BoosterFX)
				{
					BoosterFX.Play();
				}
				MonoBehaviourSingleton<OrangeBattleUtility>.Instance.ChangeRenderLayer(new GameObject[1] { HandMesh.gameObject }, ManagedSingleton<OrangeLayerManager>.Instance.RenderSPEnemy);
				object[] values = CorpsTool.Master.GetValues(new object[2]
				{
					0,
					_transform.gameObject.GetInstanceID()
				});
				if (values != null)
				{
					if (values[0] != null)
					{
						BossHand = (Transform)values[0];
					}
					if (BossHand == null)
					{
						Debug.LogError("取得回歸位置失敗，請程式檢查原因");
						return;
					}
				}
				PlaySE("BossSE04", "bs037_inary01");
				break;
			}
			}
			break;
		case MainStatus.Die:
			_velocity = VInt3.zero;
			_collideBullet.BackToPool();
			break;
		}
		AiTimer.TimerStart();
	}

	private void UpdateNextState(int Step = -1, bool needSync = true)
	{
		if (needSync && !bWaitNetStatus)
		{
			int mission = -1;
			if (!CorpsTool.MissionComplete())
			{
				mission = CorpsTool.ReceiveMission();
			}
			SetMission(mission);
			UploadStatus(NextSkill);
		}
	}

	public bool SetMission(int mission = -1)
	{
		switch (mission)
		{
		case -1:
			NextSkill = MainStatus.Idle;
			return true;
		case 0:
			NextSkill = MainStatus.Skill0;
			return true;
		case 1:
			NextSkill = MainStatus.Skill1;
			return true;
		default:
			return false;
		}
	}

	private void UploadStatus(MainStatus status)
	{
		if (status != 0)
		{
			if (CheckHost())
			{
				UploadEnemyStatus((int)status);
			}
		}
		else
		{
			SetStatus(MainStatus.Idle);
		}
	}

	public override void LogicUpdate()
	{
		if (!Activate)
		{
			return;
		}
		base.LogicUpdate();
		switch (_mainStatus)
		{
		case MainStatus.Idle:
			if (!bWaitNetStatus && !CorpsTool.MissionComplete())
			{
				UpdateNextState();
			}
			break;
		case MainStatus.Skill0:
			if (_subStatus == SubStatus.Phase0 && (Controller.Collisions.below || Controller.Collisions.left || Controller.Collisions.right || Controller.Collisions.above))
			{
				Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.CAMERA_SHAKE, 2f, false);
				_velocity = VInt3.zero;
				SetStatus(MainStatus.Idle);
			}
			break;
		case MainStatus.Skill1:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if ((bool)BossHand)
				{
					if (Vector2.Distance(BossHand.position, _transform.position) < 0.3f)
					{
						SetStatus(MainStatus.Skill1, SubStatus.Phase1);
						break;
					}
					float num = Vector3.Distance(BossHand.position, _transform.position) / ((float)MoveSpeed * 0.001f);
					BackSpeed = (int)((float)MoveSpeed * num * 1.5f);
					_velocity = new VInt3((BossHand.position - _transform.position).normalized) * BackSpeed * 0.001f;
				}
				break;
			case SubStatus.Phase1:
				PlaySE("BossSE04", "bs037_inary02");
				SetStatus(MainStatus.Idle);
				break;
			case SubStatus.Phase2:
				break;
			}
			break;
		case MainStatus.Debut:
		case MainStatus.Die:
			break;
		}
	}

	public void UpdateFunc()
	{
		if (Activate)
		{
			_transform.localPosition = Vector3.MoveTowards(base.transform.position, Controller.LogicPosition.vec3, distanceDelta);
		}
	}

	public override void SetActive(bool isActive)
	{
		base.AllowAutoAim = false;
		base.SetActive(isActive);
		if (isActive)
		{
			MonoBehaviourSingleton<OrangeBattleUtility>.Instance.ChangeRenderLayer(new GameObject[1] { HandMesh.gameObject }, ManagedSingleton<OrangeLayerManager>.Instance.RenderSPEnemy);
			if ((bool)BoosterFX)
			{
				BoosterFX.Play();
			}
			IsInvincible = true;
			IgnoreGravity = true;
			_collideBullet.UpdateBulletData(EnemyWeapons[0].BulletData);
			_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			_collideBullet.Active(targetMask);
			SetStatus(MainStatus.Idle);
		}
		else
		{
			if ((bool)BoosterFX)
			{
				BoosterFX.Stop();
			}
			if ((bool)_collideBullet)
			{
				_collideBullet.BackToPool();
			}
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
		ModelTransform.rotation = Quaternion.Euler(0f, 90 * base.direction, 0f);
		_transform.position = pos;
	}

	protected override void UpdateGravity()
	{
		if (!IgnoreGravity)
		{
			if ((_velocity.y < 0 && Controller.Collisions.below) || (_velocity.y > 0 && Controller.Collisions.above))
			{
				_velocity.y = 0;
			}
			_velocity.y += OrangeBattleUtility.FP_Gravity * GameLogicUpdateManager.g_fixFrameLenFP / 1000;
			_velocity.y = IntMath.Sign(_velocity.y) * IntMath.Min(IntMath.Abs(_velocity.y), IntMath.Abs(_maxGravity.i * 3));
		}
	}

	public void JoinCorps(BossCorpsTool corps)
	{
		CorpsTool = corps;
	}

	public void SetDead()
	{
		SetStatus(MainStatus.Die);
	}

	public void SetIdle()
	{
		SetStatus(MainStatus.Idle);
	}

	public void SetLRHand(bool isleft)
	{
		if (isleft)
		{
			ModelTransform.localScale = new Vector3(0f - Mathf.Abs(ModelTransform.localScale.x), ModelTransform.localScale.y, ModelTransform.localScale.z);
		}
		else
		{
			ModelTransform.localScale = new Vector3(Mathf.Abs(ModelTransform.localScale.x), ModelTransform.localScale.y, ModelTransform.localScale.z);
		}
	}

	public bool ForceExecuteMission()
	{
		return false;
	}
}
