using System;
using System.Collections.Generic;
using CallbackDefs;
using NaughtyAttributes;
using Newtonsoft.Json;
using StageLib;
using UnityEngine;

public class BS064_Controller : EnemyControllerBase, IManagedUpdateBehavior
{
	private enum MainStatus
	{
		Idle = 0,
		Debut = 1,
		Skill0 = 2,
		Skill1 = 3,
		Die = 4
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
		MAX_SUBSTATUS = 7
	}

	public enum AnimationID
	{
		ANI_IDLE = 0,
		ANI_DEBUT = 1,
		ANI_Skill0_START = 2,
		ANI_Skill0_LOOP = 3,
		ANI_Skill0_END = 4,
		ANI_Skill1_START1 = 5,
		ANI_Skill1_LOOP1 = 6,
		ANI_Skill1_LOOP2 = 7,
		ANI_Skill1_END2 = 8,
		ANI_TURN = 9,
		ANI_HURT = 10,
		ANI_DEAD = 11,
		MAX_ANIMATION_ID = 12
	}

	[SerializeField]
	[NaughtyAttributes.ReadOnly]
	private MainStatus _mainStatus;

	[SerializeField]
	[NaughtyAttributes.ReadOnly]
	private SubStatus _subStatus;

	[SerializeField]
	[NaughtyAttributes.ReadOnly]
	private AnimationID _currentAnimationId;

	[SerializeField]
	[NaughtyAttributes.ReadOnly]
	private float _currentFrame;

	private int[] _animationHash;

	private int[] SkillWeightArray = new int[2] { 10, 30 };

	private List<int> SkillCard = new List<int>();

	private Vector3 LastTargetPos = new Vector3(0f, 0f, 0f);

	[SerializeField]
	public GameObject[] RenderModes;

	private bool CanSummon;

	[SerializeField]
	private float SummonTime = 20f;

	private int SummonFrame;

	[SerializeField]
	private bool DebugMode;

	[SerializeField]
	private MainStatus NextSkill;

	[Header("登場特規")]
	private int MovingFrame;

	[Header("加速衝擊")]
	[SerializeField]
	private int RushSpeed = 7500;

	[SerializeField]
	private VInt3 BackSpeed = new VInt3(-2000, 4000, 0);

	[Header("鑽頭飛彈")]
	[SerializeField]
	private int Skill1ShootTimes = 4;

	[SerializeField]
	private Transform ShootPos;

	[SerializeField]
	private Transform AimPos;

	[SerializeField]
	private SkinnedMeshRenderer DrillMeshL;

	[SerializeField]
	private ParticleSystem[] DashFx = new ParticleSystem[2];

	[SerializeField]
	private float MinAngle = 30f;

	[SerializeField]
	private float MaxAngle = 140f;

	private float ShotAngle;

	private float NextAngle;

	private readonly int _HashAngle = Animator.StringToHash("Angle");

	private Vector3 EndPos;

	private float ShootFrame;

	private int ShootTimes;

	private bool HasShot;

	private bool bSkill0SE;

	private void OnEnable()
	{
		MonoBehaviourSingleton<UpdateManager>.Instance.AddUpdate(this);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		MonoBehaviourSingleton<UpdateManager>.Instance.RemoveUpdate(this);
	}

	protected void ChangeDebugMode()
	{
		DebugMode = !DebugMode;
	}

	protected void ChangeSetSkill(object[] param)
	{
		string text = param[0] as string;
		if (!(text == string.Empty))
		{
			switch (text)
			{
			case "Idle":
				NextSkill = MainStatus.Idle;
				break;
			case "Skill0":
				NextSkill = MainStatus.Skill0;
				break;
			case "Skill1":
				NextSkill = MainStatus.Skill1;
				break;
			}
		}
	}

	protected virtual void HashAnimation()
	{
		_animationHash = new int[12];
		_animationHash[0] = Animator.StringToHash("BS064@idle_loop");
		_animationHash[1] = Animator.StringToHash("BS064@debut");
		_animationHash[2] = Animator.StringToHash("BS064@skill_01_start");
		_animationHash[3] = Animator.StringToHash("BS064@skill_01_loop");
		_animationHash[4] = Animator.StringToHash("BS064@skill_01_end");
		_animationHash[5] = Animator.StringToHash("BS064@skill_02_step1_start");
		_animationHash[6] = Animator.StringToHash("BS064@skill_02_step1_loop");
		_animationHash[7] = Animator.StringToHash("BS064@skill_02_step2_start");
		_animationHash[8] = Animator.StringToHash("BS064@skill_02_step2_end");
		_animationHash[9] = Animator.StringToHash("BS064@turn_around1");
		_animationHash[10] = Animator.StringToHash("BS064@hurt_loop");
		_animationHash[11] = Animator.StringToHash("BS064@dead");
	}

	private void LoadParts(ref Transform[] childs)
	{
		ModelTransform = OrangeBattleUtility.FindChildRecursive(ref childs, "model", true);
		_animator = ModelTransform.GetComponent<Animator>();
		_collideBullet = OrangeBattleUtility.FindChildRecursive(ref childs, "Collider", true).gameObject.AddOrGetComponent<CollideBullet>();
		if (ShootPos == null)
		{
			ShootPos = OrangeBattleUtility.FindChildRecursive(ref childs, "L WeaponPoint", true);
		}
		if (AimPos == null)
		{
			AimPos = OrangeBattleUtility.FindChildRecursive(ref childs, "AimPos", true);
		}
		if (DrillMeshL == null)
		{
			DrillMeshL = OrangeBattleUtility.FindChildRecursive(ref childs, "BS064_DrillMesh_L", true).gameObject.GetComponent<SkinnedMeshRenderer>();
		}
		if (DashFx[0] == null)
		{
			DashFx[0] = OrangeBattleUtility.FindChildRecursive(ref childs, "DashFxL", true).gameObject.GetComponent<ParticleSystem>();
		}
		if (DashFx[1] == null)
		{
			DashFx[1] = OrangeBattleUtility.FindChildRecursive(ref childs, "DashFxR", true).gameObject.GetComponent<ParticleSystem>();
		}
	}

	protected override void Awake()
	{
		base.Awake();
		Transform[] childs = _transform.GetComponentsInChildren<Transform>(true);
		LoadParts(ref childs);
		HashAnimation();
		base.AimTransform = _enemyCollider[0].transform;
		base.AimPoint = new Vector3(0f, 0f, 0f);
		OrangeBattleUtility.AddEnemyAutoAimSystem(_transform, out _enemyAutoAimSystem);
		_enemyAutoAimSystem.UpdateAimRange(20f);
		base.SoundSource.Initial(OrangeSSType.BOSS);
		FallDownSE = new string[2] { "BossSE04", "bs111_hell01_stop" };
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
		ModelTransform.localScale = new Vector3(ModelTransform.localScale.x, ModelTransform.localScale.y, Mathf.Abs(ModelTransform.localScale.z) * (float)base.direction);
	}

	private void SetStatus(MainStatus mainStatus, SubStatus subStatus = SubStatus.Phase0)
	{
		_mainStatus = mainStatus;
		_subStatus = subStatus;
		switch (_mainStatus)
		{
		case MainStatus.Debut:
		{
			SubStatus subStatus2 = _subStatus;
			if (subStatus2 == SubStatus.Phase2 && IntroCallBack != null)
			{
				IntroCallBack();
			}
			break;
		}
		case MainStatus.Idle:
			_velocity = VInt3.zero;
			break;
		case MainStatus.Skill0:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_collideBullet.UpdateBulletData(EnemyWeapons[1].BulletData);
				_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
				_collideBullet.Active(targetMask);
				SwitchDashFx();
				_velocity = VInt3.zero;
				PlaySE("BossSE04", "bs111_hell01_lp");
				bSkill0SE = true;
				break;
			case SubStatus.Phase1:
				_velocity.x = RushSpeed * base.direction;
				break;
			case SubStatus.Phase2:
				_collideBullet.UpdateBulletData(EnemyWeapons[0].BulletData);
				_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
				_collideBullet.Active(targetMask);
				SwitchDashFx(false);
				MonoBehaviourSingleton<OrangeBattleUtility>.Instance.SetLockWallJump();
				_velocity = new VInt3(BackSpeed.x * base.direction, BackSpeed.y, 0);
				PlaySE("BossSE04", "bs111_hell01_stop");
				bSkill0SE = false;
				break;
			case SubStatus.Phase3:
				PlaySE("BossSE04", "bs111_hell03");
				_velocity = VInt3.zero;
				break;
			}
			break;
		case MainStatus.Skill1:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_velocity = VInt3.zero;
				ShootTimes = Skill1ShootTimes;
				ShotAngle = 90f;
				break;
			case SubStatus.Phase2:
				ShootFrame = 0f;
				HasShot = false;
				break;
			}
			break;
		case MainStatus.Die:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				base.AllowAutoAim = false;
				_velocity = VInt3.zero;
				break;
			case SubStatus.Phase1:
				StartCoroutine(BossDieFlow(GetTargetPoint(), "FX_BOSS_EXPLODE2", false, false));
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
		{
			SubStatus subStatus = _subStatus;
			if (subStatus == SubStatus.Phase1)
			{
				PlayBossSE("BossSE04", "bs111_hell00");
				_currentAnimationId = AnimationID.ANI_DEBUT;
				break;
			}
			return;
		}
		case MainStatus.Idle:
			_currentAnimationId = AnimationID.ANI_IDLE;
			break;
		case MainStatus.Skill0:
			switch (_subStatus)
			{
			default:
				return;
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_Skill0_START;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_Skill0_LOOP;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_Skill0_END;
				break;
			case SubStatus.Phase3:
				_currentAnimationId = AnimationID.ANI_TURN;
				break;
			}
			break;
		case MainStatus.Skill1:
			switch (_subStatus)
			{
			default:
				return;
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_Skill1_START1;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_Skill1_LOOP1;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_Skill1_LOOP2;
				break;
			case SubStatus.Phase3:
				_currentAnimationId = AnimationID.ANI_Skill1_END2;
				break;
			}
			break;
		case MainStatus.Die:
			if (_subStatus == SubStatus.Phase0)
			{
				_currentAnimationId = AnimationID.ANI_DEAD;
				_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
			}
			break;
		}
		_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
	}

	private void UpdateRandomState()
	{
		MainStatus mainStatus = MainStatus.Idle;
		switch (_mainStatus)
		{
		case MainStatus.Debut:
			SetStatus(MainStatus.Idle);
			break;
		case MainStatus.Idle:
			mainStatus = (MainStatus)WeightRandom(SkillWeightArray, 2);
			break;
		}
		if (DebugMode)
		{
			mainStatus = NextSkill;
		}
		if (mainStatus != 0 && CheckHost())
		{
			UploadEnemyStatus((int)mainStatus);
		}
	}

	public override void LogicUpdate()
	{
		if (_mainStatus == MainStatus.Debut)
		{
			BaseUpdate();
			UpdateGravity();
			Controller.Move((_velocity + _velocityExtra) * GameLogicUpdateManager.m_fFrameLen + _velocityShift);
			distanceDelta = Vector3.Distance(base.transform.localPosition, Controller.LogicPosition.vec3) * (Time.deltaTime / GameLogicUpdateManager.m_fFrameLen);
			_velocityExtra = VInt3.zero;
			_velocityShift = VInt3.zero;
		}
		if (!Activate && _mainStatus != MainStatus.Debut)
		{
			return;
		}
		base.LogicUpdate();
		_currentFrame = _animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
		switch (_mainStatus)
		{
		case MainStatus.Debut:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (_introReady)
				{
					SwitchDashFx();
					SetStatus(MainStatus.Debut, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if (_currentFrame > 0.47f && DashFx[0].isPlaying)
				{
					SwitchDashFx(false);
				}
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Debut, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				if (IntroCallBack != null)
				{
					IntroCallBack();
					if (!bWaitNetStatus)
					{
						UpdateRandomState();
					}
					else
					{
						SetStatus(MainStatus.Idle);
					}
				}
				break;
			}
			break;
		case MainStatus.Idle:
			if (!bWaitNetStatus)
			{
				UpdateRandomState();
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
				if (Controller.Collisions.right || Controller.Collisions.left)
				{
					Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.CAMERA_SHAKE, 2f, false);
					SetStatus(MainStatus.Skill0, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				if (Controller.Collisions.below && _currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill0, SubStatus.Phase3);
				}
				break;
			case SubStatus.Phase3:
				if (_currentFrame > 0.9f)
				{
					UpdateDirection(-base.direction);
					SetStatus(MainStatus.Idle);
				}
				break;
			}
			break;
		case MainStatus.Skill1:
		{
			EndPos = GetTargetPos();
			float num = Vector2.Angle(Vector2.right * base.direction, EndPos - AimPos.position);
			if (90f - num < MinAngle && EndPos.y > AimPos.position.y)
			{
				NextAngle = MinAngle;
			}
			else if (90f + num > MaxAngle && EndPos.y <= AimPos.position.y)
			{
				NextAngle = MaxAngle;
			}
			else if (EndPos.y > AimPos.position.y)
			{
				NextAngle = 90f - num;
			}
			else if (EndPos.y <= AimPos.position.y)
			{
				NextAngle = 90f + num;
			}
			ShotAngle = Mathf.Lerp(ShotAngle, NextAngle, 0.3f);
			_animator.SetFloat(_HashAngle, ShotAngle);
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
				if (!HasShot && _currentFrame > ShootFrame)
				{
					HasShot = true;
					BulletBase.TryShotBullet(EnemyWeapons[2].BulletData, ShootPos.position, Quaternion.Euler(0f, 0f, ShotAngle * (float)(-base.direction)) * Vector3.up, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
					DrillMeshL.enabled = false;
				}
				else if (!DrillMeshL.enabled && _currentFrame > 0.3f)
				{
					DrillMeshL.enabled = true;
				}
				if (_currentFrame > 1f)
				{
					if (--ShootTimes > 0)
					{
						SetStatus(MainStatus.Skill1, SubStatus.Phase2);
					}
					else
					{
						SetStatus(MainStatus.Skill1, SubStatus.Phase3);
					}
				}
				break;
			case SubStatus.Phase3:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Idle);
				}
				break;
			}
			break;
		}
		case MainStatus.Die:
			if (_subStatus == SubStatus.Phase0 && _currentFrame > 0.5f)
			{
				SetStatus(MainStatus.Die, SubStatus.Phase1);
			}
			break;
		}
		if (CanSummon && GameLogicUpdateManager.GameFrame > SummonFrame)
		{
			MonoBehaviourSingleton<OrangeBattleUtility>.Instance.CallSummonEnemyEvent(_transform);
			SummonFrame = GameLogicUpdateManager.GameFrame + (int)(SummonTime * 20f);
		}
	}

	public void UpdateFunc()
	{
		if (Activate || _mainStatus == MainStatus.Debut)
		{
			base.transform.localPosition = Vector3.MoveTowards(base.transform.localPosition, Controller.LogicPosition.vec3, distanceDelta);
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

	public override void BossIntro(Action cb)
	{
		IntroCallBack = cb;
		_introReady = true;
	}

	public override void Unlock()
	{
		_unlockReady = true;
		base.AllowAutoAim = true;
		if ((int)Hp > 0)
		{
			SetColliderEnable(true);
		}
		if (InGame)
		{
			Activate = true;
		}
		SummonFrame = GameLogicUpdateManager.GameFrame + (int)(SummonTime * 20f);
		AI_STATE aiState = AiState;
		if (aiState != 0 && aiState == AI_STATE.mob_002)
		{
			CanSummon = true;
		}
	}

	private int WeightRandom(int[] WeightArray, int SkillStart)
	{
		int num = 0;
		int num2 = 0;
		int num3 = WeightArray.Length;
		for (int i = 0; i < num3; i++)
		{
			num2 += WeightArray[i];
		}
		int num4 = OrangeBattleUtility.Random(0, num2);
		for (int j = 0; j < num3; j++)
		{
			num += WeightArray[j];
			if (num4 < num)
			{
				return j + SkillStart;
			}
		}
		return 0;
	}

	protected override void DeadBehavior(ref HurtPassParam tHurtPassParam)
	{
		if (_mainStatus != MainStatus.Die)
		{
			CanSummon = false;
			if ((bool)_collideBullet)
			{
				_collideBullet.BackToPool();
			}
			StageUpdate.SlowStage();
			SetColliderEnable(false);
			SetStatus(MainStatus.Die);
		}
	}

	private Vector3 GetTargetPos()
	{
		if (!Target)
		{
			Target = _enemyAutoAimSystem.GetClosetPlayer();
		}
		if ((bool)Target)
		{
			TargetPos = new VInt3(Target.Controller.GetRealCenterPos() + Vector3.up * 0.2f);
			return TargetPos.vec3;
		}
		return _transform.position + Vector3.right * 3f * base.direction;
	}

	private void SwitchDashFx(bool onoff = true)
	{
		if (onoff)
		{
			DashFx[0].Play();
			DashFx[1].Play();
		}
		else
		{
			DashFx[0].Stop();
			DashFx[1].Stop();
		}
	}

	public override void UpdateEnemyID(int id)
	{
		base.UpdateEnemyID(id);
		UpdateAIState();
		switch (AiState)
		{
		case AI_STATE.mob_001:
			base.DeadPlayCompleted = false;
			break;
		case AI_STATE.mob_003:
			base.DeadPlayCompleted = false;
			break;
		}
	}
}
