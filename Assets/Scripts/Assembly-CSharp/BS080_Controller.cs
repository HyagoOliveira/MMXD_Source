#define RELEASE
using System;
using CallbackDefs;
using CriWare;
using NaughtyAttributes;
using Newtonsoft.Json;
using StageLib;
using UnityEngine;

public class BS080_Controller : EnemyControllerBase, IManagedUpdateBehavior
{
	private enum MainStatus
	{
		Idle = 0,
		Debut = 1,
		Dash = 2,
		Skill0 = 3,
		Skill1 = 4,
		Skill2 = 5,
		Skill3 = 6,
		Die = 7
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
		Phase8 = 8,
		Phase9 = 9,
		MAX_SUBSTATUS = 10
	}

	public enum AnimationID
	{
		ANI_IDLE = 0,
		ANI_STAND = 1,
		ANI_DEBUT = 2,
		ANI_DASH_START = 3,
		ANI_DASH_LOOP = 4,
		ANI_DASH_END = 5,
		ANI_SKILL0_START1 = 6,
		ANI_SKILL0_LOOP1 = 7,
		ANI_SKILL0_START2 = 8,
		ANI_SKILL0_LOOP2 = 9,
		ANI_SKILL0_END2 = 10,
		ANI_SKILL0_START3 = 11,
		ANI_SKILL0_LOOP3 = 12,
		ANI_SKILL0_END3 = 13,
		ANI_SKILL1_START1 = 14,
		ANI_SKILL1_LOOP1 = 15,
		ANI_SKILL1_START2 = 16,
		ANI_SKILL1_LOOP2 = 17,
		ANI_SKILL1_START3 = 18,
		ANI_SKILL1_LOOP3 = 19,
		ANI_SKILL1_START4 = 20,
		ANI_SKILL1_LOOP4 = 21,
		ANI_SKILL1_END4 = 22,
		ANI_SKILL2_START1 = 23,
		ANI_SKILL2_LOOP1 = 24,
		ANI_SKILL2_START2 = 25,
		ANI_SKILL2_LOOP2 = 26,
		ANI_SKILL2_END2 = 27,
		ANI_SKILL3_START1 = 28,
		ANI_SKILL3_LOOP1 = 29,
		ANI_SKILL3_START2 = 30,
		ANI_SKILL3_LOOP2 = 31,
		ANI_SKILL3_START3 = 32,
		ANI_SKILL3_LOOP3 = 33,
		ANI_SKILL3_START4 = 34,
		ANI_SKILL3_LOOP4 = 35,
		ANI_SKILL3_END4 = 36,
		ANI_HURT = 37,
		ANI_DEAD = 38,
		MAX_ANIMATION_ID = 39
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

	private int nDeadCount;

	private int[] _animationHash;

	private int[] SkillCard = new int[4] { 35, 35, 15, 15 };

	private Vector3 LastTargetPos = new Vector3(0f, 0f, 0f);

	[SerializeField]
	private float IdleWaitTime = 0.2f;

	private int IdleWaitFrame;

	private float ShootFrame;

	private bool HasShot;

	private Vector3 EndPos;

	private bool CanMove;

	private bool Unlocked;

	private int EventID = -1;

	private float CenterXPos;

	private float HideTime = 1f;

	private int HideFrame;

	private float ShowTime = 0.5f;

	private int ShowFrame;

	[SerializeField]
	private string sSk0UseFX = "fxuse_colonel_s_001";

	[SerializeField]
	private int DashSpeed = 8000;

	private bool isDownSlash;

	[SerializeField]
	private CollideBullet SwordCollider;

	[SerializeField]
	private float AtkTime = 2.5f;

	private int AtkFrame;

	[SerializeField]
	private ParticleSystem ThunderFX;

	[SerializeField]
	private ParticleSystem ThunderChargeFX;

	[SerializeField]
	private float ShotDistance = 2.5f;

	private float RightXPos;

	private float LeftXPos;

	[SerializeField]
	private int JumpForce = 12000;

	[SerializeField]
	private SkinnedMeshRenderer[] MeshObjs = new SkinnedMeshRenderer[3];

	[SerializeField]
	private bool DebugMode;

	[SerializeField]
	private MainStatus NextSkill;

	private CriAtomExPlayback introSE;

	private ParticleSystem mfxuse_skill1;

	private ParticleSystem mskillEft;

	private ParticleSystem skilllatFX;

	private ParticleSystem skilllatFX1;

	private ParticleSystem skilllatFX2;

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
			case "Skill2":
				NextSkill = MainStatus.Skill2;
				break;
			case "Skill3":
				NextSkill = MainStatus.Skill3;
				break;
			}
		}
	}

	protected virtual void HashAnimation()
	{
		_animationHash = new int[39];
		_animationHash[0] = Animator.StringToHash("BS080@idle_loop");
		_animationHash[2] = Animator.StringToHash("BS080@debut");
		_animationHash[1] = Animator.StringToHash("BS080@line_up_loop");
		_animationHash[3] = Animator.StringToHash("BS080@dash_start");
		_animationHash[4] = Animator.StringToHash("BS080@dash_loop");
		_animationHash[5] = Animator.StringToHash("BS080@dash_ends");
		_animationHash[6] = Animator.StringToHash("BS080@skill_01_step1_start");
		_animationHash[7] = Animator.StringToHash("BS080@skill_01_step1_loop");
		_animationHash[8] = Animator.StringToHash("BS080@skill_01_step2_start");
		_animationHash[9] = Animator.StringToHash("BS080@skill_01_step2_loop");
		_animationHash[10] = Animator.StringToHash("BS080@skill_01_step2_end");
		_animationHash[11] = Animator.StringToHash("BS080@skill_01_step3_start");
		_animationHash[12] = Animator.StringToHash("BS080@skill_01_step3_loop");
		_animationHash[13] = Animator.StringToHash("BS080@skill_01_step3_end");
		_animationHash[14] = Animator.StringToHash("BS080@skill_02_step1_start");
		_animationHash[15] = Animator.StringToHash("BS080@skill_02_step1_loop");
		_animationHash[16] = Animator.StringToHash("BS080@skill_02_step2_start");
		_animationHash[17] = Animator.StringToHash("BS080@skill_02_step2_loop");
		_animationHash[18] = Animator.StringToHash("BS080@skill_02_step3_start");
		_animationHash[19] = Animator.StringToHash("BS080@skill_02_step3_loop");
		_animationHash[20] = Animator.StringToHash("BS080@skill_02_step4_start");
		_animationHash[21] = Animator.StringToHash("BS080@skill_02_step4_loop");
		_animationHash[22] = Animator.StringToHash("BS080@skill_02_step4_end");
		_animationHash[23] = Animator.StringToHash("BS080@skill_03_step1_start");
		_animationHash[24] = Animator.StringToHash("BS080@skill_03_step1_loop");
		_animationHash[25] = Animator.StringToHash("BS080@skill_03_step2_start");
		_animationHash[26] = Animator.StringToHash("BS080@skill_03_step2_loop");
		_animationHash[27] = Animator.StringToHash("BS080@skill_03_step2_end");
		_animationHash[28] = Animator.StringToHash("BS080@skill_04_step1_start");
		_animationHash[29] = Animator.StringToHash("BS080@skill_04_step1_loop");
		_animationHash[30] = Animator.StringToHash("BS080@skill_04_step2_start");
		_animationHash[31] = Animator.StringToHash("BS080@skill_04_step2_loop");
		_animationHash[32] = Animator.StringToHash("BS080@skill_04_step3_start");
		_animationHash[33] = Animator.StringToHash("BS080@skill_04_step3_loop");
		_animationHash[34] = Animator.StringToHash("BS080@skill_04_step4_start");
		_animationHash[35] = Animator.StringToHash("BS080@skill_04_step4_loop");
		_animationHash[36] = Animator.StringToHash("BS080@skill_04_step4_end");
		_animationHash[37] = Animator.StringToHash("BS080@hurt_loop");
		_animationHash[38] = Animator.StringToHash("BS080@dead");
	}

	private void LoadParts(ref Transform[] childs)
	{
		ModelTransform = OrangeBattleUtility.FindChildRecursive(ref childs, "model", true);
		_animator = ModelTransform.GetComponent<Animator>();
		_collideBullet = OrangeBattleUtility.FindChildRecursive(ref childs, "Collider", true).gameObject.AddOrGetComponent<CollideBullet>();
		if (SwordCollider == null)
		{
			SwordCollider = OrangeBattleUtility.FindChildRecursive(ref childs, "SwordCollider", true).gameObject.AddOrGetComponent<CollideBullet>();
		}
		if (ThunderFX == null)
		{
			ThunderFX = OrangeBattleUtility.FindChildRecursive(ref childs, "ThunderFX", true).gameObject.AddOrGetComponent<ParticleSystem>();
		}
		if (ThunderChargeFX == null)
		{
			ThunderChargeFX = OrangeBattleUtility.FindChildRecursive(ref childs, "ThunderChargeFX", true).gameObject.AddOrGetComponent<ParticleSystem>();
		}
		if (MeshObjs[0] == null)
		{
			MeshObjs[0] = OrangeBattleUtility.FindChildRecursive(ref childs, "BS080_BodyMesh_G", true).gameObject.AddOrGetComponent<SkinnedMeshRenderer>();
		}
		if (MeshObjs[1] == null)
		{
			MeshObjs[1] = OrangeBattleUtility.FindChildRecursive(ref childs, "BS080_SaberMesh_G", true).gameObject.AddOrGetComponent<SkinnedMeshRenderer>();
		}
		if (MeshObjs[2] == null)
		{
			MeshObjs[2] = OrangeBattleUtility.FindChildRecursive(ref childs, "BS080_SaberMesh_U", true).gameObject.AddOrGetComponent<SkinnedMeshRenderer>();
		}
	}

	protected override void Awake()
	{
		base.Awake();
		Transform[] childs = _transform.GetComponentsInChildren<Transform>(true);
		LoadParts(ref childs);
		HashAnimation();
		base.AimPoint = new Vector3(0f, 0.6f, 0f);
		OrangeBattleUtility.AddEnemyAutoAimSystem(_transform, out _enemyAutoAimSystem);
		_enemyAutoAimSystem.UpdateAimRange(20f);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(sSk0UseFX, 2);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_colonel_s_002", 2);
		Transform transform = OrangeBattleUtility.FindChildRecursive(ref childs, "fxuse_skill", true);
		if (transform != null)
		{
			mfxuse_skill1 = transform.GetComponent<ParticleSystem>();
		}
		transform = OrangeBattleUtility.FindChildRecursive(ref childs, "skill1eft", true);
		if (transform != null)
		{
			mskillEft = transform.GetComponent<ParticleSystem>();
		}
		transform = OrangeBattleUtility.FindChildRecursive(ref childs, "skilllatFX", true);
		if (transform != null)
		{
			skilllatFX = transform.GetComponent<ParticleSystem>();
		}
		transform = OrangeBattleUtility.FindChildRecursive(ref childs, "skilllatFX1", true);
		if (transform != null)
		{
			skilllatFX1 = transform.GetComponent<ParticleSystem>();
		}
		transform = OrangeBattleUtility.FindChildRecursive(ref childs, "skilllatFX2", true);
		if (transform != null)
		{
			skilllatFX2 = transform.GetComponent<ParticleSystem>();
		}
		AiTimer.TimerStart();
		FallDownSE = new string[2] { "BossSE03", "bs028_karnel02" };
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
			UpdateDirection();
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
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				CanMove = false;
				Unlocked = false;
				if (DebugMode)
				{
					SetStatus(MainStatus.Debut, SubStatus.Phase4);
				}
				break;
			case SubStatus.Phase2:
				PlaySE("BossSE03", "bs028_karnel03");
				UpdateDirection(1);
				_velocity.x = DashSpeed * base.direction;
				break;
			case SubStatus.Phase4:
				UpdateDirection(-1);
				break;
			}
			break;
		case MainStatus.Idle:
			_velocity.x = 0;
			IdleWaitFrame = GameLogicUpdateManager.GameFrame + (int)(IdleWaitTime * 20f);
			break;
		case MainStatus.Dash:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (_transform.position.x > CenterXPos)
				{
					UpdateDirection(1);
				}
				else
				{
					UpdateDirection(-1);
				}
				break;
			case SubStatus.Phase1:
				PlaySE("BossSE03", "bs028_karnel03");
				if (mfxuse_skill1 != null && !mfxuse_skill1.isPlaying)
				{
					mfxuse_skill1.Play();
				}
				_velocity.x = DashSpeed * base.direction;
				break;
			case SubStatus.Phase2:
				if (mfxuse_skill1 != null)
				{
					mfxuse_skill1.Stop();
				}
				_velocity.x = DashSpeed / 2 * base.direction;
				LeanTween.value(base.gameObject, _velocity.x, 500f * (float)base.direction, 1f).setOnUpdate(delegate(float f)
				{
					_velocity.x = (int)f;
				});
				break;
			}
			break;
		case MainStatus.Skill0:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_velocity = VInt3.zero;
				break;
			case SubStatus.Phase1:
				Stealth(true);
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(sSk0UseFX, _transform.position, (base.direction == 1) ? OrangeCharacter.NormalQuaternion : OrangeCharacter.ReversedQuaternion, Array.Empty<object>());
				HideFrame = GameLogicUpdateManager.GameFrame + (int)(HideTime * 20f);
				break;
			case SubStatus.Phase2:
				_velocity = VInt3.zero;
				EndPos = Target.transform.position + Vector3.right * ((float)(-base.direction) * 1.8f);
				if (EndPos.x + Controller.Collider2D.size.x / 2f > RightXPos || EndPos.x - Controller.Collider2D.size.x / 2f < LeftXPos)
				{
					EndPos = Target.transform.position + Vector3.right * ((float)base.direction * 1.8f);
					UpdateDirection(-base.direction);
				}
				_transform.position = EndPos;
				Controller.LogicPosition = new VInt3(EndPos);
				ShowFrame = GameLogicUpdateManager.GameFrame + (int)(ShowTime * 20f);
				break;
			case SubStatus.Phase3:
				PlaySE("BossSE03", "bs028_karnel05");
				Stealth(false);
				SwordCollider.Active(targetMask);
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_colonel_s_002", _transform.position, (base.direction == 1) ? OrangeCharacter.NormalQuaternion : OrangeCharacter.ReversedQuaternion, Array.Empty<object>());
				break;
			case SubStatus.Phase5:
				SwordCollider.BackToPool();
				if (_transform.position.x > CenterXPos)
				{
					UpdateDirection(1);
				}
				else
				{
					UpdateDirection(-1);
				}
				break;
			case SubStatus.Phase6:
				_velocity = VInt3.zero;
				IgnoreGravity = true;
				EndPos = Target._transform.position + Vector3.up * 2f;
				_transform.position = EndPos;
				Controller.LogicPosition = new VInt3(EndPos);
				ShowFrame = GameLogicUpdateManager.GameFrame + (int)(ShowTime * 20f);
				break;
			case SubStatus.Phase7:
				Stealth(false);
				PlaySE("BossSE03", "bs028_karnel05");
				break;
			case SubStatus.Phase8:
				if (mskillEft != null && !mskillEft.isPlaying)
				{
					mskillEft.Play();
				}
				_velocity.y = -8000;
				SwordCollider.Active(targetMask);
				break;
			case SubStatus.Phase9:
				IgnoreGravity = false;
				if (mskillEft != null && mskillEft.isPlaying)
				{
					mskillEft.Stop();
				}
				SwordCollider.BackToPool();
				if (_transform.position.x > CenterXPos)
				{
					UpdateDirection(1);
				}
				else
				{
					UpdateDirection(-1);
				}
				break;
			}
			break;
		case MainStatus.Skill1:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_velocity = VInt3.zero;
				break;
			case SubStatus.Phase2:
				ShootFrame = 0.55f;
				HasShot = false;
				break;
			case SubStatus.Phase4:
				ShootFrame = 0.5f;
				HasShot = false;
				break;
			case SubStatus.Phase6:
				ShootFrame = 0.55f;
				HasShot = false;
				break;
			}
			break;
		case MainStatus.Skill2:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_velocity = VInt3.zero;
				break;
			case SubStatus.Phase1:
				Stealth(true);
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(sSk0UseFX, _transform.position, (base.direction == 1) ? OrangeCharacter.NormalQuaternion : OrangeCharacter.ReversedQuaternion, Array.Empty<object>());
				HideFrame = GameLogicUpdateManager.GameFrame + (int)(HideTime * 20f);
				break;
			case SubStatus.Phase2:
				_velocity = VInt3.zero;
				EndPos = new Vector3(CenterXPos, _transform.position.y, 0f);
				_transform.position = EndPos;
				Controller.LogicPosition = new VInt3(EndPos);
				ShowFrame = GameLogicUpdateManager.GameFrame + (int)(ShowTime * 20f);
				break;
			case SubStatus.Phase3:
				Stealth(false);
				PlaySE("BossSE03", "bs028_karnel11");
				break;
			case SubStatus.Phase4:
				PlaySE("BossSE03", "bs028_karnel08");
				ThunderChargeFX.Play();
				ThunderFX.Play();
				break;
			case SubStatus.Phase5:
				ShootFrame = 0.7f;
				HasShot = false;
				break;
			case SubStatus.Phase6:
				ShootFrame = 40f;
				HasShot = false;
				AtkFrame = GameLogicUpdateManager.GameFrame + (int)(AtkTime * 20f);
				break;
			case SubStatus.Phase7:
				if (_transform.position.x > CenterXPos)
				{
					UpdateDirection(1);
				}
				else
				{
					UpdateDirection(-1);
				}
				break;
			}
			break;
		case MainStatus.Skill3:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				PlaySE("BossSE03", "bs028_karnel01");
				_velocity = VInt3.zero;
				_velocity.y = JumpForce;
				break;
			case SubStatus.Phase2:
				IgnoreGravity = true;
				break;
			case SubStatus.Phase4:
				IgnoreGravity = false;
				break;
			case SubStatus.Phase6:
				ShootFrame = 0.8f;
				HasShot = false;
				Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.CAMERA_SHAKE, 2f, false);
				break;
			}
			break;
		case MainStatus.Die:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				nDeadCount = 0;
				base.AllowAutoAim = false;
				if (Controller.Collisions.below)
				{
					SetStatus(MainStatus.Die, SubStatus.Phase2);
				}
				else
				{
					SetStatus(MainStatus.Die, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase2:
				_velocity = VInt3.zero;
				base.DeadPlayCompleted = true;
				break;
			case SubStatus.Phase3:
				UpdateDirection(1);
				break;
			case SubStatus.Phase4:
				Stealth(true);
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(sSk0UseFX, _transform.position, (base.direction == 1) ? OrangeCharacter.NormalQuaternion : OrangeCharacter.ReversedQuaternion, Array.Empty<object>());
				BackToPool();
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
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_IDLE;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_DASH_START;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_DASH_LOOP;
				break;
			case SubStatus.Phase3:
				_currentAnimationId = AnimationID.ANI_DASH_END;
				break;
			case SubStatus.Phase4:
				_currentAnimationId = AnimationID.ANI_STAND;
				break;
			case SubStatus.Phase5:
				_currentAnimationId = AnimationID.ANI_DEBUT;
				break;
			case SubStatus.Phase6:
				_currentAnimationId = AnimationID.ANI_IDLE;
				break;
			}
			break;
		case MainStatus.Idle:
			_currentAnimationId = AnimationID.ANI_IDLE;
			break;
		case MainStatus.Dash:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_DASH_START;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_DASH_LOOP;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_DASH_END;
				break;
			}
			break;
		case MainStatus.Skill0:
			switch (_subStatus)
			{
			default:
				return;
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_SKILL0_START1;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_SKILL0_LOOP1;
				break;
			case SubStatus.Phase3:
				_currentAnimationId = AnimationID.ANI_SKILL0_START2;
				break;
			case SubStatus.Phase4:
				_currentAnimationId = AnimationID.ANI_SKILL0_LOOP2;
				break;
			case SubStatus.Phase5:
				_currentAnimationId = AnimationID.ANI_SKILL0_END2;
				break;
			case SubStatus.Phase7:
				_currentAnimationId = AnimationID.ANI_SKILL0_START3;
				break;
			case SubStatus.Phase8:
				_currentAnimationId = AnimationID.ANI_SKILL0_LOOP3;
				break;
			case SubStatus.Phase9:
				_currentAnimationId = AnimationID.ANI_SKILL0_END3;
				break;
			case SubStatus.Phase2:
			case SubStatus.Phase6:
				return;
			}
			break;
		case MainStatus.Skill1:
			switch (_subStatus)
			{
			default:
				return;
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_SKILL1_START1;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_SKILL1_LOOP1;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_SKILL1_START2;
				break;
			case SubStatus.Phase3:
				_currentAnimationId = AnimationID.ANI_SKILL1_LOOP2;
				break;
			case SubStatus.Phase4:
				_currentAnimationId = AnimationID.ANI_SKILL1_START3;
				break;
			case SubStatus.Phase5:
				_currentAnimationId = AnimationID.ANI_SKILL1_LOOP3;
				break;
			case SubStatus.Phase6:
				_currentAnimationId = AnimationID.ANI_SKILL1_START4;
				break;
			case SubStatus.Phase7:
				_currentAnimationId = AnimationID.ANI_SKILL1_LOOP4;
				break;
			case SubStatus.Phase8:
				_currentAnimationId = AnimationID.ANI_SKILL1_END4;
				break;
			}
			break;
		case MainStatus.Skill2:
			switch (_subStatus)
			{
			default:
				return;
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_SKILL0_START1;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_SKILL0_LOOP1;
				break;
			case SubStatus.Phase3:
				_currentAnimationId = AnimationID.ANI_SKILL2_START1;
				break;
			case SubStatus.Phase4:
				_currentAnimationId = AnimationID.ANI_SKILL2_LOOP1;
				break;
			case SubStatus.Phase5:
				_currentAnimationId = AnimationID.ANI_SKILL2_START2;
				break;
			case SubStatus.Phase6:
				_currentAnimationId = AnimationID.ANI_SKILL2_LOOP2;
				break;
			case SubStatus.Phase7:
				_currentAnimationId = AnimationID.ANI_SKILL2_END2;
				break;
			case SubStatus.Phase2:
				return;
			}
			break;
		case MainStatus.Skill3:
			switch (_subStatus)
			{
			default:
				return;
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_SKILL3_START1;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_SKILL3_LOOP1;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_SKILL3_START2;
				break;
			case SubStatus.Phase3:
				_currentAnimationId = AnimationID.ANI_SKILL3_LOOP2;
				break;
			case SubStatus.Phase4:
				_currentAnimationId = AnimationID.ANI_SKILL3_START3;
				break;
			case SubStatus.Phase5:
				_currentAnimationId = AnimationID.ANI_SKILL3_LOOP3;
				break;
			case SubStatus.Phase6:
				_currentAnimationId = AnimationID.ANI_SKILL3_START4;
				break;
			case SubStatus.Phase7:
				_currentAnimationId = AnimationID.ANI_SKILL3_LOOP4;
				break;
			case SubStatus.Phase8:
				_currentAnimationId = AnimationID.ANI_SKILL3_END4;
				break;
			}
			break;
		case MainStatus.Die:
			switch (_subStatus)
			{
			default:
				return;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_HURT;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_DEAD;
				break;
			case SubStatus.Phase3:
				_currentAnimationId = AnimationID.ANI_DASH_START;
				break;
			case SubStatus.Phase4:
				_currentAnimationId = AnimationID.ANI_DASH_LOOP;
				break;
			}
			break;
		}
		_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
	}

	private void UpdateRandomState(MainStatus status = MainStatus.Idle)
	{
		MainStatus mainStatus = status;
		if (status == MainStatus.Idle)
		{
			switch (_mainStatus)
			{
			case MainStatus.Debut:
				SetStatus(MainStatus.Idle);
				break;
			case MainStatus.Idle:
				mainStatus = (MainStatus)RandomCard(3);
				break;
			}
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
				if (CanMove)
				{
					SetStatus(MainStatus.Debut, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Debut, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				if ((_transform.position.x - EndPos.x) * (float)base.direction > 0f)
				{
					_velocity = VInt3.zero;
					_transform.position = new Vector3(EndPos.x, _transform.position.y, 0f);
					Controller.LogicPosition = new VInt3(_transform.position);
					SetStatus(MainStatus.Debut, SubStatus.Phase3);
				}
				break;
			case SubStatus.Phase3:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Debut, SubStatus.Phase4);
				}
				break;
			case SubStatus.Phase4:
				if (_introReady)
				{
					SetStatus(MainStatus.Debut, SubStatus.Phase5);
				}
				break;
			case SubStatus.Phase5:
				if (_currentFrame > 1f)
				{
					introSE = base.SoundSource.PlaySE("BossSE03", "bs028_karnel12");
					SetStatus(MainStatus.Debut, SubStatus.Phase6);
				}
				break;
			case SubStatus.Phase6:
				if (introSE.GetStatus() == CriAtomExPlayback.Status.Removed && IntroCallBack != null)
				{
					IntroCallBack();
					SetStatus(MainStatus.Debut, SubStatus.Phase7);
				}
				break;
			case SubStatus.Phase7:
				if (Unlocked)
				{
					base.AllowAutoAim = true;
					SetColliderEnable(true);
					_collideBullet.Active(targetMask);
					CenterXPos = CheckRoomSize();
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
			if (!bWaitNetStatus && IdleWaitFrame < GameLogicUpdateManager.GameFrame)
			{
				Target = _enemyAutoAimSystem.GetClosetPlayer();
				if ((bool)Target)
				{
					TargetPos = new VInt3(Target._transform.position);
					UpdateDirection();
					UpdateRandomState();
				}
				else
				{
					UpdateRandomState(MainStatus.Skill1);
				}
			}
			break;
		case MainStatus.Dash:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Dash, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if ((bool)Physics2D.Raycast(_transform.position + Vector3.up, Vector3.right * base.direction, 3f, Controller.collisionMask))
				{
					SetStatus(MainStatus.Dash, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				if ((_currentFrame > 1f && Controller.Collisions.left) || Controller.Collisions.right)
				{
					SetStatus(MainStatus.Idle);
				}
				break;
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
				if (HideFrame < GameLogicUpdateManager.GameFrame)
				{
					if (isDownSlash)
					{
						SetStatus(MainStatus.Skill0, SubStatus.Phase6);
					}
					else
					{
						SetStatus(MainStatus.Skill0, SubStatus.Phase2);
					}
					isDownSlash = !isDownSlash;
				}
				break;
			case SubStatus.Phase2:
				if (ShowFrame < GameLogicUpdateManager.GameFrame)
				{
					SetStatus(MainStatus.Skill0, SubStatus.Phase3);
				}
				break;
			case SubStatus.Phase3:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill0, SubStatus.Phase4);
				}
				break;
			case SubStatus.Phase4:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill0, SubStatus.Phase5);
				}
				break;
			case SubStatus.Phase5:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Dash, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase6:
				if (ShowFrame < GameLogicUpdateManager.GameFrame)
				{
					SetStatus(MainStatus.Skill0, SubStatus.Phase7);
				}
				break;
			case SubStatus.Phase7:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill0, SubStatus.Phase8);
				}
				break;
			case SubStatus.Phase8:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill0, SubStatus.Phase9);
				}
				break;
			case SubStatus.Phase9:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Dash, SubStatus.Phase1);
				}
				break;
			}
			break;
		case MainStatus.Skill1:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill1, SubStatus.Phase1);
					if (skilllatFX != null)
					{
						PlayBossSE03("bs028_karnel00");
						skilllatFX.Play();
					}
				}
				break;
			case SubStatus.Phase1:
				if (_currentFrame > 0.5f)
				{
					SetStatus(MainStatus.Skill1, SubStatus.Phase2);
					if (skilllatFX1 != null && !skilllatFX1.isPlaying)
					{
						skilllatFX1.Play();
					}
				}
				break;
			case SubStatus.Phase2:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill1, SubStatus.Phase3);
				}
				if (!HasShot && _currentFrame > ShootFrame)
				{
					HasShot = true;
					BulletBase.TryShotBullet(EnemyWeapons[2].BulletData, _transform.position + Vector3.up + Vector3.right * base.direction, Vector3.right * base.direction, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
				}
				break;
			case SubStatus.Phase3:
				if (_currentFrame > 0.5f)
				{
					SetStatus(MainStatus.Skill1, SubStatus.Phase4);
				}
				break;
			case SubStatus.Phase4:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill1, SubStatus.Phase5);
				}
				if (!HasShot && _currentFrame > ShootFrame)
				{
					HasShot = true;
					BulletBase.TryShotBullet(EnemyWeapons[2].BulletData, _transform.position + Vector3.up + Vector3.right * base.direction, Vector3.right * base.direction, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
				}
				break;
			case SubStatus.Phase5:
				if (_currentFrame > 0.5f)
				{
					SetStatus(MainStatus.Skill1, SubStatus.Phase6);
				}
				break;
			case SubStatus.Phase6:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill1, SubStatus.Phase7);
				}
				if (!HasShot && _currentFrame > ShootFrame)
				{
					HasShot = true;
					BulletBase.TryShotBullet(EnemyWeapons[2].BulletData, _transform.position + Vector3.up + Vector3.right * base.direction, Vector3.right * base.direction, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
				}
				break;
			case SubStatus.Phase7:
				if (_currentFrame > 0.5f)
				{
					SetStatus(MainStatus.Skill1, SubStatus.Phase8);
				}
				break;
			case SubStatus.Phase8:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Idle);
					if (skilllatFX1 != null && skilllatFX1.isPlaying)
					{
						skilllatFX1.Stop();
					}
				}
				break;
			}
			break;
		case MainStatus.Skill2:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill2, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if (HideFrame < GameLogicUpdateManager.GameFrame)
				{
					SetStatus(MainStatus.Skill2, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				if (ShowFrame < GameLogicUpdateManager.GameFrame)
				{
					SetStatus(MainStatus.Skill2, SubStatus.Phase3);
				}
				break;
			case SubStatus.Phase3:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill2, SubStatus.Phase4);
				}
				break;
			case SubStatus.Phase4:
				if (_currentFrame > 3f)
				{
					SetStatus(MainStatus.Skill2, SubStatus.Phase5);
				}
				break;
			case SubStatus.Phase5:
				if (_currentFrame > 1f)
				{
					PlaySE("BossSE03", "bs028_karnel09");
					PlaySE("BossSE03", "bs028_karnel10");
					SetStatus(MainStatus.Skill2, SubStatus.Phase6);
					if (skilllatFX2 != null && !skilllatFX2.isPlaying)
					{
						skilllatFX2.Play();
					}
				}
				if (!HasShot && _currentFrame > ShootFrame)
				{
					HasShot = true;
					ThunderChargeFX.Stop();
					BulletBase.TryShotBullet(EnemyWeapons[3].BulletData, _transform.position + Vector3.up * 0.3f, Vector3.right, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
					BulletBase.TryShotBullet(EnemyWeapons[3].BulletData, _transform.position + Vector3.up * 0.3f, Vector3.left, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
				}
				break;
			case SubStatus.Phase6:
				if (AtkFrame < GameLogicUpdateManager.GameFrame)
				{
					SetStatus(MainStatus.Skill2, SubStatus.Phase7);
					if (skilllatFX2 != null && skilllatFX2.isPlaying)
					{
						skilllatFX2.Stop();
					}
				}
				if (!HasShot && (float)GameLogicUpdateManager.GameFrame > ShootFrame)
				{
					HasShot = true;
					float num = _transform.position.x + ShotDistance / 2f;
					float num2 = _transform.position.x + ShotDistance / 2f;
					int num3 = Mathf.Abs((int)((RightXPos - num) / ShotDistance));
					int num4 = Mathf.Abs((int)((LeftXPos - num2) / ShotDistance));
					for (int i = 0; i <= num3; i++)
					{
						BulletBase.TryShotBullet(worldPos: new Vector3(num + (float)i * ShotDistance, _transform.position.y + 0.3f, 0f), tSkillTable: EnemyWeapons[4].BulletData, pDirection: Vector3.up, weaponStatus: null, tBuffStatus: selfBuffManager.sBuffStatus, refMOB_TABLE: EnemyData, pTargetMask: targetMask);
					}
					for (int j = 0; j <= num4; j++)
					{
						BulletBase.TryShotBullet(worldPos: new Vector3(num2 - (float)j * ShotDistance, _transform.position.y + 0.3f, 0f), tSkillTable: EnemyWeapons[4].BulletData, pDirection: Vector3.up, weaponStatus: null, tBuffStatus: selfBuffManager.sBuffStatus, refMOB_TABLE: EnemyData, pTargetMask: targetMask);
					}
				}
				break;
			case SubStatus.Phase7:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Dash, SubStatus.Phase1);
				}
				break;
			}
			break;
		case MainStatus.Skill3:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill3, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if (_velocity.y < 1000)
				{
					SetStatus(MainStatus.Skill3, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill3, SubStatus.Phase3);
					PlaySE("BossSE03", "bs028_karnel06");
				}
				break;
			case SubStatus.Phase3:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill3, SubStatus.Phase4);
				}
				break;
			case SubStatus.Phase4:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill3, SubStatus.Phase5);
				}
				break;
			case SubStatus.Phase5:
				if (Controller.Collisions.below)
				{
					SetStatus(MainStatus.Skill3, SubStatus.Phase6);
				}
				break;
			case SubStatus.Phase6:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill3, SubStatus.Phase7);
				}
				if (!HasShot && _currentFrame > ShootFrame)
				{
					HasShot = true;
					BulletBase.TryShotBullet(EnemyWeapons[5].BulletData, _transform.position + Vector3.right * base.direction, Vector3.right * base.direction, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
				}
				break;
			case SubStatus.Phase7:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill3, SubStatus.Phase8);
				}
				break;
			case SubStatus.Phase8:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Idle);
				}
				break;
			}
			break;
		case MainStatus.Die:
			switch (_subStatus)
			{
			case SubStatus.Phase1:
				if (Controller.Collisions.below)
				{
					SetStatus(MainStatus.Die, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				if (_currentFrame > 0.9f)
				{
					if (nDeadCount > 10)
					{
						SetStatus(MainStatus.Die, SubStatus.Phase3);
					}
					else
					{
						nDeadCount++;
					}
				}
				break;
			case SubStatus.Phase3:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Die, SubStatus.Phase4);
				}
				break;
			}
			break;
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
		_characterMaterial.ChangeDissolveColor(new Color(1f, 1f, 1f));
		base.SetActive(isActive);
		if (isActive)
		{
			base.AllowAutoAim = false;
			SetColliderEnable(false);
			CanMove = false;
			Unlocked = false;
			_collideBullet.UpdateBulletData(EnemyWeapons[0].BulletData);
			_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			SwordCollider.UpdateBulletData(EnemyWeapons[1].BulletData);
			SwordCollider.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			Singleton<GenericEventManager>.Instance.AttachEvent<EventManager.StageEventCall>(EventManager.ID.STAGE_EVENT_CALL, StartMove);
			AI_STATE aiState = AiState;
			if (aiState == AI_STATE.mob_002)
			{
				Unlocked = false;
				SetStatus(MainStatus.Debut, SubStatus.Phase4);
			}
			else
			{
				SetStatus(MainStatus.Debut);
			}
		}
		else
		{
			_collideBullet.BackToPool();
			SwordCollider.BackToPool();
			Singleton<GenericEventManager>.Instance.DetachEvent<EventManager.StageEventCall>(EventManager.ID.STAGE_EVENT_CALL, StartMove);
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
		base.transform.position = pos;
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
		Unlocked = true;
		if ((int)Hp > 0)
		{
			SetColliderEnable(true);
		}
		if (InGame)
		{
			Activate = true;
		}
	}

	private int RandomCard(int StartPos)
	{
		int num = 0;
		int num2 = 0;
		int num3 = SkillCard.Length;
		for (int i = 0; i < num3; i++)
		{
			num2 += SkillCard[i];
		}
		int num4 = OrangeBattleUtility.Random(0, num2);
		for (int j = 0; j < num3; j++)
		{
			num += SkillCard[j];
			if (num4 < num)
			{
				return j + StartPos;
			}
		}
		return 0;
	}

	protected override void DeadBehavior(ref HurtPassParam tHurtPassParam)
	{
		if (_mainStatus != MainStatus.Die)
		{
			IgnoreGravity = false;
			LeanTween.cancel(base.gameObject);
			if ((bool)_collideBullet)
			{
				_collideBullet.BackToPool();
			}
			if ((bool)SwordCollider)
			{
				SwordCollider.BackToPool();
			}
			if ((bool)ThunderChargeFX)
			{
				ThunderChargeFX.Stop();
			}
			StageUpdate.SlowStage();
			SetColliderEnable(false);
			mfxuse_skill1.Stop();
			SetStatus(MainStatus.Die);
		}
	}

	public override void UpdateEnemyID(int id)
	{
		base.UpdateEnemyID(id);
		UpdateAIState();
		AI_STATE aiState = AiState;
	}

	private float CheckRoomSize()
	{
		int layerMask = (1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockLayer) | (1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockEnemyLayer) | (1 << LayerMask.NameToLayer("NoWallKick"));
		RaycastHit2D raycastHit2D = OrangeBattleUtility.RaycastIgnoreSelf(_transform.position + Vector3.up, Vector3.left, 20f, layerMask, _transform);
		RaycastHit2D raycastHit2D2 = OrangeBattleUtility.RaycastIgnoreSelf(_transform.position + Vector3.up, Vector3.right, 20f, layerMask, _transform);
		if (!raycastHit2D || !raycastHit2D2)
		{
			Debug.LogError("Boss 80 卡尼爾 需要有左右兩邊牆壁判斷場景中心位置");
			RightXPos = _transform.position.x + 8f;
			LeftXPos = _transform.position.x - 8f;
			return _transform.position.x;
		}
		if ((bool)raycastHit2D2)
		{
			RightXPos = raycastHit2D2.point.x - 0.5f;
		}
		if ((bool)raycastHit2D)
		{
			LeftXPos = raycastHit2D.point.x + 0.5f;
		}
		return (raycastHit2D2.point.x + raycastHit2D.point.x) / 2f;
	}

	private void Stealth(bool SwitchOn, bool opencollide = true)
	{
		if (SwitchOn)
		{
			PlaySE("BossSE03", "bs028_karnel04");
			for (int i = 0; i < MeshObjs.Length; i++)
			{
				MeshObjs[i].enabled = false;
			}
			_enemyCollider[0].SetColliderEnable(false);
			_collideBullet.BackToPool();
			return;
		}
		for (int j = 0; j < MeshObjs.Length; j++)
		{
			MeshObjs[j].enabled = true;
		}
		_enemyCollider[0].SetColliderEnable();
		if (opencollide)
		{
			_collideBullet.Active(targetMask);
		}
	}

	public override void SetPatrolPath(bool isLoop, int nMoveSpeed, Vector3[] paths)
	{
		EndPos = paths[1];
	}

	public void StartMove(EventManager.StageEventCall tStageEventCall)
	{
		if (EventID != -1 && tStageEventCall.nID == EventID)
		{
			CanMove = true;
		}
	}

	public override void SetEventCtrlID(int eventid)
	{
		EventID = eventid;
	}
}
