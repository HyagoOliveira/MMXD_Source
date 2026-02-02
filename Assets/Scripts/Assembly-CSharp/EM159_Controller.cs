#define RELEASE
using CallbackDefs;
using Newtonsoft.Json;
using UnityEngine;

public class EM159_Controller : EnemyControllerBase, IManagedUpdateBehavior, IF_ForceExecute
{
	private enum MainStatus
	{
		Idle = 0,
		Debut = 1,
		Skill0 = 2,
		Skill1 = 3,
		Skill2 = 4,
		Skill3 = 5,
		Skill4 = 6,
		Die = 7,
		IdleWaitNet = 8
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
	[Tooltip("手迴轉時間")]
	private float TurnedTime;

	[SerializeField]
	private ParticleSystem BoosterFX;

	private OrangeCharacter targetOC;

	private bool isCatching;

	[SerializeField]
	private SkinnedMeshRenderer HandMesh;

	[SerializeField]
	private Transform RocketTarget;

	[SerializeField]
	private SpriteRenderer PredictSpriteRenderer;

	private Transform BossHand;

	private BossCorpsTool CorpsTool;

	private MainStatus NextSkill;

	[SerializeField]
	private float WaitTime;

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

	protected virtual void HashAnimation()
	{
		_animationHash = new int[7];
		_animationHash[0] = Animator.StringToHash("BS093_HAND@idle_loop");
		_animationHash[2] = Animator.StringToHash("BS093_HAND@skill_04_step1_start");
		_animationHash[3] = Animator.StringToHash("BS093_HAND@skill_04_step1_loop");
		_animationHash[4] = Animator.StringToHash("BS093_HAND@skill_04_step2_start");
		_animationHash[5] = Animator.StringToHash("BS093_HAND@skill_04_step2_end");
	}

	private void LoadParts(ref Transform[] childs)
	{
		ModelTransform = OrangeBattleUtility.FindChildRecursive(ref childs, "model", true);
		_animator = ModelTransform.GetComponent<Animator>();
		_collideBullet = OrangeBattleUtility.FindChildRecursive(ref childs, "Collider", true).gameObject.AddOrGetComponent<CollideBullet>();
		if (HandMesh == null)
		{
			HandMesh = OrangeBattleUtility.FindChildRecursive(ref childs, "BS093_FreeHandMesh", true).GetComponent<SkinnedMeshRenderer>();
		}
		if (RocketTarget == null)
		{
			RocketTarget = OrangeBattleUtility.FindChildRecursive(ref childs, "RocketTarget", true);
		}
		if (PredictSpriteRenderer == null)
		{
			PredictSpriteRenderer = OrangeBattleUtility.FindChildRecursive(ref childs, "RocketTarget", true).GetComponent<SpriteRenderer>();
		}
		if ((bool)PredictSpriteRenderer)
		{
			PredictSpriteRenderer.enabled = false;
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
		HashAnimation();
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
		ModelTransform.rotation = Quaternion.Euler(0f, 90 + 90 * base.direction, 0f);
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
			if (_subStatus == SubStatus.Phase0)
			{
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
					UpdateDirection();
					AtkVelocity = (Target.Controller.GetCenterPos().xy() - _transform.position.xy()).normalized;
					distance = Vector2.Distance(_transform.position, Target._transform.position);
				}
				else
				{
					UpdateDirection(-base.direction);
					AtkVelocity = Vector3.right * base.direction;
					distance = 8f;
				}
				_velocity = new VInt3(AtkVelocity) * MoveSpeed * 0.001f;
				StartPos = _transform.position;
				PlaySE("BossSE04", "bs031_general06");
			}
			break;
		case MainStatus.Skill1:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if ((bool)BoosterFX)
				{
					BoosterFX.Play();
				}
				break;
			case SubStatus.Phase1:
				if (Target == null)
				{
					Target = _enemyAutoAimSystem.GetClosetPlayer();
				}
				if ((bool)Target)
				{
					TargetPos = new VInt3(Target._transform.position);
					UpdateDirection();
					AtkVelocity = (Target.Controller.GetCenterPos().xy() - _transform.position.xy()).normalized;
					distance = Vector2.Distance(_transform.position, Target._transform.position);
				}
				else
				{
					UpdateDirection(-base.direction);
					AtkVelocity = Vector3.right * base.direction;
					distance = 8f;
				}
				_velocity = new VInt3(AtkVelocity) * MoveSpeed * 0.001f;
				StartPos = _transform.position;
				PlaySE("BossSE04", "bs031_general04");
				break;
			case SubStatus.Phase2:
				_velocity = VInt3.zero;
				_collideBullet.BackToPool();
				break;
			case SubStatus.Phase3:
				_velocity = VInt3.zero;
				break;
			}
			break;
		case MainStatus.Skill2:
			if (_subStatus == SubStatus.Phase0)
			{
				if ((bool)BoosterFX)
				{
					BoosterFX.Play();
				}
				EndPos = _transform.position + Vector3.right * base.direction;
				TargetPos = new VInt3(EndPos);
				UpdateDirection();
				object[] values3 = CorpsTool.Master.GetValues(new object[2]
				{
					1,
					_transform.gameObject.GetInstanceID()
				});
				if (values3 != null && values3[0] != null)
				{
					EndPos = (Vector3)values3[0];
				}
				StartPos = _transform.position;
				distance = Vector3.Distance(EndPos, StartPos);
				AtkVelocity = EndPos - StartPos;
				_velocity = new VInt3(AtkVelocity.normalized) * (MoveSpeed / 2) * 0.001f;
			}
			break;
		case MainStatus.Skill3:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
			{
				if ((bool)BoosterFX)
				{
					BoosterFX.Play();
				}
				MonoBehaviourSingleton<OrangeBattleUtility>.Instance.ChangeRenderLayer(new GameObject[1] { HandMesh.gameObject }, ManagedSingleton<OrangeLayerManager>.Instance.RenderSPEnemy);
				object[] values2 = CorpsTool.Master.GetValues(new object[2]
				{
					0,
					_transform.gameObject.GetInstanceID()
				});
				if (values2 != null)
				{
					if (values2[0] != null)
					{
						BossHand = (Transform)values2[0];
					}
					if (BossHand == null)
					{
						Debug.LogError("取得回歸位置失敗，請程式檢查原因");
						return;
					}
					if (values2[1] != null)
					{
						UpdateDirection((int)values2[1]);
					}
				}
				PlaySE("BossSE04", "bs031_general06");
				break;
			}
			}
			break;
		case MainStatus.Skill4:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
			{
				EndPos = _transform.position + Vector3.right * base.direction;
				TargetPos = new VInt3(EndPos);
				UpdateDirection();
				ModelTransform.rotation = Quaternion.Euler(0f, 90 + 90 * base.direction, 45f);
				object[] values = CorpsTool.Master.GetValues(new object[2]
				{
					2,
					_transform.gameObject.GetInstanceID()
				});
				if (values != null && values[0] != null)
				{
					EndPos = (Vector3)values[0];
				}
				RocketTarget.SetParentNull();
				RocketTarget.position = EndPos + Vector3.down * 2f + Vector3.right * base.direction * 1f;
				RocketTarget.localRotation = Quaternion.Euler(90f, 0f, 0f);
				PredictSpriteRenderer.enabled = true;
				StartPos = _transform.position;
				distance = Vector3.Distance(EndPos, StartPos);
				AtkVelocity = EndPos - StartPos;
				_velocity = new VInt3(AtkVelocity.normalized) * (MoveSpeed * 3) * 0.001f;
				_collideBullet.UpdateBulletData(EnemyWeapons[1].BulletData);
				_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
				_collideBullet.Active(targetMask);
				break;
			}
			case SubStatus.Phase1:
				_velocity = VInt3.zero;
				break;
			}
			break;
		case MainStatus.Die:
			_velocity = VInt3.zero;
			_collideBullet.BackToPool();
			break;
		}
		AiTimer.TimerStart();
		UpdateAnimation();
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
		case 2:
			NextSkill = MainStatus.Skill2;
			return true;
		case 3:
			NextSkill = MainStatus.Skill3;
			return true;
		case 4:
			NextSkill = MainStatus.Skill4;
			return true;
		default:
			return false;
		}
	}

	private void UpdateAnimation()
	{
		switch (_mainStatus)
		{
		case MainStatus.Idle:
		case MainStatus.Debut:
		case MainStatus.Skill0:
		case MainStatus.Skill2:
		case MainStatus.Skill3:
		case MainStatus.Skill4:
			_currentAnimationId = AnimationID.ANI_IDLE;
			break;
		case MainStatus.Skill1:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_Skill1_START1;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_Skill1_LOOP1;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_Skill1_START2;
				if (isCatching)
				{
					_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
				}
				break;
			case SubStatus.Phase3:
				_currentAnimationId = AnimationID.ANI_Skill1_END;
				break;
			}
			break;
		case MainStatus.Die:
			if (_subStatus == SubStatus.Phase0)
			{
				_currentAnimationId = AnimationID.ANI_IDLE;
			}
			break;
		}
		if (!isCatching)
		{
			_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
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
		_currentFrame = _animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
		switch (_mainStatus)
		{
		case MainStatus.Idle:
			if (!bWaitNetStatus && !CorpsTool.MissionComplete())
			{
				UpdateNextState();
			}
			break;
		case MainStatus.Skill0:
			if (_subStatus == SubStatus.Phase0 && Vector2.Distance(StartPos, _transform.position) > distance)
			{
				_velocity = VInt3.zero;
				CorpsTool.Master.ReportObjects(new object[2] { 0, true });
				SetStatus(MainStatus.Idle);
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
			{
				isCatching = false;
				Collider2D collider2D = Physics2D.OverlapBox(_transform.position, Vector2.one, 0f, LayerMask.GetMask("Player"));
				if ((bool)collider2D)
				{
					TryCatchPlayer(collider2D);
				}
				if (isCatching)
				{
					SetStatus(MainStatus.Skill1, SubStatus.Phase2);
				}
				if (Vector2.Distance(StartPos, _transform.position) > distance)
				{
					SetStatus(MainStatus.Skill1, SubStatus.Phase3);
				}
				break;
			}
			case SubStatus.Phase2:
				CorpsTool.Master.ReportObjects(new object[2] { 0, true });
				SetStatus(MainStatus.Idle);
				break;
			case SubStatus.Phase3:
				if (_currentFrame > 1f)
				{
					CorpsTool.Master.ReportObjects(new object[2] { 0, true });
					SetStatus(MainStatus.Idle);
				}
				break;
			}
			break;
		case MainStatus.Skill2:
			if (_subStatus == SubStatus.Phase0 && Vector2.Distance(StartPos, _transform.position) > distance)
			{
				_velocity = VInt3.zero;
				CorpsTool.Master.ReportObjects(new object[2] { 0, true });
				SetStatus(MainStatus.Idle);
			}
			break;
		case MainStatus.Skill3:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if ((bool)BossHand)
				{
					if (Vector2.Distance(BossHand.position, _transform.position) < 0.3f)
					{
						SetStatus(MainStatus.Skill3, SubStatus.Phase1);
						break;
					}
					float num = Vector3.Distance(BossHand.position, _transform.position) / ((float)MoveSpeed * 0.001f);
					BackSpeed = (int)((float)MoveSpeed * num * 1.5f);
					_velocity = new VInt3((BossHand.position - _transform.position).normalized) * MoveSpeed * 0.001f;
				}
				break;
			case SubStatus.Phase1:
				CorpsTool.Master.ReportObjects(new object[2] { 0, true });
				SetStatus(MainStatus.Idle);
				break;
			case SubStatus.Phase2:
				break;
			}
			break;
		case MainStatus.Skill4:
			if (_subStatus != 0)
			{
				int num2 = 1;
			}
			else if (Vector2.Distance(StartPos, _transform.position) > distance)
			{
				MonoBehaviourSingleton<OrangeBattleUtility>.Instance.ChangeRenderLayer(new GameObject[1] { HandMesh.gameObject }, ManagedSingleton<OrangeLayerManager>.Instance.EnemyLayer);
				_velocity = VInt3.zero;
				CorpsTool.Master.ReportObjects(new object[2] { 0, true });
				RocketTarget.SetParent(_transform);
				PredictSpriteRenderer.enabled = false;
				Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.CAMERA_SHAKE, 2f, false);
				if ((bool)BoosterFX)
				{
					BoosterFX.Stop();
				}
				PlaySE("BossSE04", "bs031_general12");
				_collideBullet.UpdateBulletData(EnemyWeapons[0].BulletData);
				_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
				_collideBullet.Active(targetMask);
				SetStatus(MainStatus.Idle);
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
			ModelTransform.localScale = new Vector3(1.7f, 1.7f, 1.7f * (float)base.direction);
			MonoBehaviourSingleton<OrangeBattleUtility>.Instance.ChangeRenderLayer(new GameObject[1] { HandMesh.gameObject }, ManagedSingleton<OrangeLayerManager>.Instance.RenderSPEnemy);
			if ((bool)BoosterFX)
			{
				BoosterFX.Play();
			}
			IsInvincible = true;
			IgnoreGravity = true;
			isCatching = false;
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
		ModelTransform.rotation = Quaternion.Euler(0f, 90 + 90 * base.direction, 0f);
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
		CorpsTool.SetCanForceExecute(this);
	}

	public void SetDead()
	{
		SetStatus(MainStatus.Die);
	}

	public void SetIdle()
	{
		SetStatus(MainStatus.Idle);
	}

	private void TryCatchPlayer(Collider2D player)
	{
		if (!player)
		{
			return;
		}
		targetOC = OrangeBattleUtility.GetHitTargetOrangeCharacter(player);
		if ((bool)targetOC && !targetOC.IsStun)
		{
			if ((bool)targetOC.IsUnBreakX())
			{
				targetOC = null;
				return;
			}
			CorpsTool.Master.ReportObjects(new object[3] { 1, targetOC.sNetSerialID, _transform });
			MonoBehaviourSingleton<OrangeBattleUtility>.Instance.ChangeRenderLayer(new GameObject[1] { HandMesh.gameObject }, ManagedSingleton<OrangeLayerManager>.Instance.RenderEnemy);
			isCatching = true;
		}
	}

	public void SetLRHand(bool isleft)
	{
		if (isleft)
		{
			ModelTransform.localScale = new Vector3(ModelTransform.localScale.x, ModelTransform.localScale.y, 0f - Mathf.Abs(ModelTransform.localScale.z));
		}
		else
		{
			ModelTransform.localScale = new Vector3(ModelTransform.localScale.x, ModelTransform.localScale.y, Mathf.Abs(ModelTransform.localScale.z));
		}
	}

	public bool ForceExecuteMission()
	{
		return false;
	}

	public bool SetStopMission()
	{
		_velocity = VInt3.zero;
		MainStatus mainStatus = _mainStatus;
		if (mainStatus == MainStatus.Skill1)
		{
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				SetStatus(MainStatus.Idle);
				break;
			case SubStatus.Phase1:
				SetStatus(MainStatus.Skill1, SubStatus.Phase3);
				break;
			}
		}
		else
		{
			SetStatus(MainStatus.Idle);
		}
		return false;
	}
}
