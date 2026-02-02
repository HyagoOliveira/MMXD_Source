#define RELEASE
using System;
using CallbackDefs;
using CodeStage.AntiCheat.ObscuredTypes;
using NaughtyAttributes;
using Newtonsoft.Json;
using StageLib;
using UnityEngine;

public class BS099_Controller : EnemyControllerBase, IManagedUpdateBehavior
{
	[Serializable]
	public class PlayInfo
	{
		public AnimationID anim = AnimationID.MAX_ANIMATION_ID;

		public float f_delayTime;

		public SE_Name e_Name = SE_Name.Max_Name;

		public PlayInfo(AnimationID ani = AnimationID.MAX_ANIMATION_ID, SE_Name e = SE_Name.Max_Name, float t = 0f)
		{
			anim = ani;
			e_Name = e;
			f_delayTime = t;
		}
	}

	public enum SE_Name
	{
		Hand_Lightning_0 = 0,
		Hand_Lightning_1 = 1,
		Hand_Push_0 = 2,
		Hand_Push_1 = 3,
		Bullet_0 = 4,
		Fire_0 = 5,
		Fire_1 = 6,
		Lightning_0 = 7,
		Lightning_1 = 8,
		Max_Name = 9
	}

	private enum MainStatus
	{
		Idle = 0,
		Debut = 1,
		Skill0 = 2,
		Skill2 = 3,
		Skill4 = 4,
		Skill1 = 5,
		Skill3 = 6,
		Hurt = 7,
		Die = 8
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
		ANI_DEBUT_LOOP = 1,
		ANI_DEBUT_END = 2,
		ANI_Skill0_START = 3,
		ANI_Skill0_SHOOT = 4,
		ANI_Skill0_RIGHT_TO_LEFT = 5,
		ANI_Skill0_LEFT_TO_RIGHT = 6,
		ANI_Skill1_START = 7,
		ANI_Skill1_LOOP = 8,
		ANI_Skill1_ATK_START = 9,
		ANI_Skill1_ATK_LOOP = 10,
		ANI_Skill1_END = 11,
		ANI_Skill2_START = 12,
		ANI_Skill2_LOOP = 13,
		ANI_Skill2_END = 14,
		ANI_Skill3_LEFT_START = 15,
		ANI_Skill3_LEFT_LOOP = 16,
		ANI_Skill3_LEFT_END = 17,
		ANI_Skill3_RIGHT_START = 18,
		ANI_Skill3_RIGHT_LOOP = 19,
		ANI_Skill3_RIGHT_END = 20,
		ANI_HURT = 21,
		ANI_DEAD = 22,
		MAX_ANIMATION_ID = 23
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

	private int nDeadCount;

	private Vector3 LastTargetPos = new Vector3(0f, 0f, 0f);

	private OrangeTimer ReloadTimer;

	private int[] SkillWeightArray = new int[3] { 4, 5, 3 };

	private int[] SkillWeightArrayMid = new int[3] { 2, 8, 2 };

	private BS041_FireSpray FireSpray;

	private Transform CannonTransform;

	private Transform CannonShootTransform;

	private bool Fire_Left_To_Right = true;

	private CollideBullet[] Thunderbolt = new CollideBullet[2];

	private int ShootTimes;

	private float NextShot;

	private BulletBase CircleBullet;

	private float angle;

	private float preangle;

	private readonly int _HashAngle = Animator.StringToHash("angle");

	private CollideBullet LeftHandPress;

	private CollideBullet RightHandPress;

	private bool _IsLeft = true;

	private Transform LeftShoulder;

	private CollideBullet[] LaserBullet = new CollideBullet[10];

	private int PreLaser;

	private int Laser;

	private int StartLaser;

	private int CloseLaser;

	private float NextShootFrame;

	private float NextUseFrame;

	private float NextStartFrame;

	private float NextCloseFrame;

	[SerializeField]
	private int LaserNum = 4;

	[SerializeField]
	private int WeakLaserNum = 3;

	private bool haveSpawn;

	private bool changedhand = true;

	private Transform LHand1;

	private Transform LHand2;

	private Transform RHand1;

	private Transform RHand2;

	private Transform LHandPos;

	private Transform RHandPos;

	private EM173_Controller FakeLHand;

	private EM173_Controller FakeRHand;

	private float SwingSpeed;

	[SerializeField]
	private float WeakHandSpeed;

	[SerializeField]
	public GameObject[] RenderModes;

	[SerializeField]
	private float ShakeTime = 2f;

	private int ShakeFrame;

	private bool[] HasBreak = new bool[5];

	[SerializeField]
	private SkinnedMeshRenderer[] PartMesh = new SkinnedMeshRenderer[7];

	[SerializeField]
	private Transform[] PartPos = new Transform[7];

	[SerializeField]
	private PlayInfo[] AnimPlayList;

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
		_animationHash[0] = Animator.StringToHash("BS041@idle_loop");
		_animationHash[2] = Animator.StringToHash("BS041@debut");
		_animationHash[3] = Animator.StringToHash("BS041@skill_01_start");
		_animationHash[4] = Animator.StringToHash("BS041@skill_01_loop");
		_animationHash[5] = Animator.StringToHash("BS041@skill_01_atk_right_to_left");
		_animationHash[6] = Animator.StringToHash("BS041@skill_01_atk_left_to_right");
		_animationHash[7] = Animator.StringToHash("BS041@skill_02_start");
		_animationHash[8] = Animator.StringToHash("BS041@skill_02_loop");
		_animationHash[9] = Animator.StringToHash("BS041@skill_02_atk_start");
		_animationHash[10] = Animator.StringToHash("BS041@skill_02_atk_loop");
		_animationHash[11] = Animator.StringToHash("BS041@skill_02_end");
		_animationHash[12] = Animator.StringToHash("BS041@skill_03_start");
		_animationHash[13] = Animator.StringToHash("BS041@skill_03_loop");
		_animationHash[14] = Animator.StringToHash("BS041@skill_03_end");
		_animationHash[15] = Animator.StringToHash("BS041@skill_04_start");
		_animationHash[16] = Animator.StringToHash("BS041@skill_04_loop");
		_animationHash[17] = Animator.StringToHash("BS041@skill_04_end");
		_animationHash[18] = Animator.StringToHash("BS041@skill_05_start");
		_animationHash[19] = Animator.StringToHash("BS041@skill_05_loop");
		_animationHash[20] = Animator.StringToHash("BS041@skill_05_end");
		_animationHash[21] = Animator.StringToHash("BS041@hurt_loop");
		_animationHash[22] = Animator.StringToHash("BS041@dead");
	}

	private void LoadParts(ref Transform[] childs)
	{
		ModelTransform = OrangeBattleUtility.FindChildRecursive(ref childs, "model", true);
		_animator = ModelTransform.GetComponent<Animator>();
		CannonTransform = OrangeBattleUtility.FindChildRecursive(ref childs, "Cannon_ctrl", true);
		CannonShootTransform = OrangeBattleUtility.FindChildRecursive(ref childs, "Cannon_stretch_ctrl", true);
		Thunderbolt[1] = OrangeBattleUtility.FindChildRecursive(ref childs, "L_gun_ctrl").gameObject.AddOrGetComponent<CollideBullet>();
		Thunderbolt[0] = OrangeBattleUtility.FindChildRecursive(ref childs, "R_gun_ctrl").gameObject.AddOrGetComponent<CollideBullet>();
		LHand1 = OrangeBattleUtility.FindChildRecursive(ref childs, "BS099_HandMesh_L_0");
		LHand2 = OrangeBattleUtility.FindChildRecursive(ref childs, "BS099_HandMesh_L_1");
		RHand1 = OrangeBattleUtility.FindChildRecursive(ref childs, "BS099_HandMesh_R_0");
		RHand2 = OrangeBattleUtility.FindChildRecursive(ref childs, "BS099_HandMesh_R_1");
		LHandPos = OrangeBattleUtility.FindChildRecursive(ref childs, "L_Hand_ctrl");
		RHandPos = OrangeBattleUtility.FindChildRecursive(ref childs, "R_Hand_ctrl");
		LeftHandPress = OrangeBattleUtility.FindChildRecursive(ref childs, "L_Hand_ctrl").gameObject.AddOrGetComponent<CollideBullet>();
		RightHandPress = OrangeBattleUtility.FindChildRecursive(ref childs, "R_Hand_ctrl").gameObject.AddOrGetComponent<CollideBullet>();
		LeftShoulder = OrangeBattleUtility.FindChildRecursive(ref childs, "ShoulderPoint_R", true);
		Transform[] array = OrangeBattleUtility.FindMultiChildRecursive(childs, "LaserCollideBullet");
		for (int i = 0; i < LaserBullet.Length; i++)
		{
			LaserBullet[i] = array[i].gameObject.gameObject.AddOrGetComponent<CollideBullet>();
		}
		if (PartMesh[0] == null)
		{
			PartPos[0] = OrangeBattleUtility.FindChildRecursive(ref childs, "ShoulderPoint_L", true);
			PartMesh[0] = OrangeBattleUtility.FindChildRecursive(ref childs, "BS099_BodyMesh_3", true).GetComponent<SkinnedMeshRenderer>();
		}
		if (PartMesh[1] == null)
		{
			PartPos[1] = OrangeBattleUtility.FindChildRecursive(ref childs, "LShoulder2", true);
			PartMesh[1] = OrangeBattleUtility.FindChildRecursive(ref childs, "BS099_BodyMesh_4", true).GetComponent<SkinnedMeshRenderer>();
		}
		if (PartMesh[2] == null)
		{
			PartPos[2] = OrangeBattleUtility.FindChildRecursive(ref childs, "ShoulderPoint_R", true);
			PartMesh[2] = OrangeBattleUtility.FindChildRecursive(ref childs, "BS099_BodyMesh_5", true).GetComponent<SkinnedMeshRenderer>();
		}
		if (PartMesh[3] == null)
		{
			PartPos[3] = OrangeBattleUtility.FindChildRecursive(ref childs, "Spine05_jnt", true);
			PartMesh[3] = OrangeBattleUtility.FindChildRecursive(ref childs, "BS099_BodyMesh_2", true).GetComponent<SkinnedMeshRenderer>();
		}
		if (PartMesh[4] == null)
		{
			PartPos[4] = OrangeBattleUtility.FindChildRecursive(ref childs, "BS099_HandMesh_L_1", true);
			PartMesh[4] = OrangeBattleUtility.FindChildRecursive(ref childs, "BS099_HandMesh_L_1", true).GetComponent<SkinnedMeshRenderer>();
		}
		if (PartMesh[5] == null)
		{
			PartPos[5] = OrangeBattleUtility.FindChildRecursive(ref childs, "BS099_HandMesh_R_1", true);
			PartMesh[5] = OrangeBattleUtility.FindChildRecursive(ref childs, "BS099_HandMesh_R_1", true).GetComponent<SkinnedMeshRenderer>();
		}
		if (PartMesh[6] == null)
		{
			PartPos[6] = OrangeBattleUtility.FindChildRecursive(ref childs, "Spine05_jnt", true);
			PartMesh[6] = OrangeBattleUtility.FindChildRecursive(ref childs, "BS099_BodyMesh_1", true).GetComponent<SkinnedMeshRenderer>();
		}
	}

	protected override void Awake()
	{
		base.Awake();
		Transform[] childs = _transform.GetComponentsInChildren<Transform>(true);
		LoadParts(ref childs);
		ReloadTimer = OrangeTimerManager.GetTimer();
		_animationHash = new int[23];
		HashAnimation();
		base.AimTransform = OrangeBattleUtility.FindChildRecursive(ref childs, "AimPoint", true);
		base.AimPoint = new Vector3(0f, 0f, 0f);
		OrangeBattleUtility.AddEnemyAutoAimSystem(_transform, out _enemyAutoAimSystem);
		_enemyAutoAimSystem.UpdateAimRange(20f);
		AiTimer.TimerStart();
		FallDownSE = new string[2] { "BattleSE", "bt_ridearmor02" };
	}

	protected override void Start()
	{
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuseTarget", 10);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_MHR_Magnamalo_000", 3);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxduring_MHR_Magnamalo_000", 3);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_MHR_Magnamalo_fire_shot_000", 3);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxduring_MHR_Magnamalo_002");
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_MHR_Magnamalo_003", 10);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxhit_MHR_Magnamalo_001", 5);
	}

	private void SetCurrentAnimationID(AnimationID id)
	{
		_currentAnimationId = id;
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
		case MainStatus.Debut:
		{
			SubStatus subStatus2 = _subStatus;
			if (subStatus2 == SubStatus.Phase1 && IntroCallBack != null)
			{
				IntroCallBack();
			}
			break;
		}
		case MainStatus.Idle:
			_velocity.x = 0;
			break;
		case MainStatus.Skill0:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				Fire_Left_To_Right = TargetPos.x - Controller.LogicPosition.x < 0;
				PlaySE("BossSE04", "bs035_onkosig10");
				break;
			case SubStatus.Phase1:
				base.SoundSource.PlaySE("WeaponSE", "wep_jet_fire02");
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_MHR_Magnamalo_000", CannonShootTransform.position, Quaternion.Euler(0f, 0f, 0f), new object[1]
				{
					new Vector3(0.8f, 0.8f, 0.8f)
				});
				break;
			case SubStatus.Phase2:
				base.SoundSource.PlaySE("BossSE04", "bs035_onkosig01");
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_MHR_Magnamalo_fire_shot_000", CannonShootTransform, Quaternion.Euler(0f, 0f, 0f), new Vector3(1f, 1f, 1f), Array.Empty<object>());
				ReloadTimer.TimerStart();
				break;
			}
			break;
		case MainStatus.Skill1:
			switch (_subStatus)
			{
			case SubStatus.Phase1:
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_MHR_Magnamalo_above_hand_000", Thunderbolt[0].transform.position, Quaternion.Euler(0f, 0f, 0f), new object[1]
				{
					new Vector3(1f, 1f, 1f)
				});
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_MHR_Magnamalo_above_hand_000", Thunderbolt[1].transform.position, Quaternion.Euler(0f, 0f, 0f), new object[1]
				{
					new Vector3(1f, 1f, 1f)
				});
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_MHR_Magnamalo_under_hand_000", Thunderbolt[0].transform.position, Quaternion.Euler(0f, 0f, 0f), new object[1]
				{
					new Vector3(1f, 1f, 1f)
				});
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_MHR_Magnamalo_under_hand_000", Thunderbolt[1].transform.position, Quaternion.Euler(0f, 0f, 0f), new object[1]
				{
					new Vector3(1f, 1f, 1f)
				});
				break;
			case SubStatus.Phase3:
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_MHR_Magnamalo_above_hand_000", Thunderbolt[0].transform.position, Quaternion.Euler(0f, 0f, 0f), new object[1]
				{
					new Vector3(1f, 1f, 1f)
				});
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_MHR_Magnamalo_above_hand_000", Thunderbolt[1].transform.position, Quaternion.Euler(0f, 0f, 0f), new object[1]
				{
					new Vector3(1f, 1f, 1f)
				});
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_MHR_Magnamalo_under_hand_000", Thunderbolt[0].transform.position, Quaternion.Euler(0f, 0f, 0f), new object[1]
				{
					new Vector3(1f, 1f, 1f)
				});
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_MHR_Magnamalo_under_hand_000", Thunderbolt[1].transform.position, Quaternion.Euler(0f, 0f, 0f), new object[1]
				{
					new Vector3(1f, 1f, 1f)
				});
				break;
			}
			break;
		case MainStatus.Skill2:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				PlaySE("BossSE04", "bs035_onkosig10");
				ShootTimes = 9;
				break;
			case SubStatus.Phase1:
				NextShot = 0f;
				break;
			}
			break;
		case MainStatus.Skill3:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_IsLeft = TargetPos.x - Controller.LogicPosition.x < 0;
				break;
			}
			break;
		case MainStatus.Skill4:
			if (_subStatus == SubStatus.Phase0)
			{
				PlaySE("BossSE04", "bs035_onkosig14");
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxduring_MHR_Magnamalo_002", LeftShoulder, Quaternion.Euler(0f, 0f, 0f), new Vector3(1f, 1f, 1f), Array.Empty<object>());
				PreLaser = 0;
				Laser = 0;
				StartLaser = 0;
				CloseLaser = 0;
				NextShootFrame = 0f;
				NextUseFrame = 0.3f;
				NextStartFrame = 0.4f;
				NextCloseFrame = 0.6f;
			}
			break;
		case MainStatus.Hurt:
		{
			if ((int)Hp <= 0)
			{
				SetStatus(MainStatus.Die);
				break;
			}
			for (int i = 0; i < LaserBullet.Length; i++)
			{
				if (LaserBullet[i] != null && LaserBullet[i].IsActivate)
				{
					LaserBullet[i].BackToPool();
				}
			}
			PlaySE("BossSE04", "bs035_onkosig05");
			Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.CLOSE_FX);
			ShakeFrame = GameLogicUpdateManager.GameFrame + (int)(ShakeTime * 20f);
			break;
		}
		case MainStatus.Die:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_velocity.y = 0;
				base.AllowAutoAim = false;
				_velocity.x = 0;
				OrangeBattleUtility.LockPlayer();
				if ((bool)FakeLHand)
				{
					FakeLHand.SetDead();
				}
				if ((bool)FakeRHand)
				{
					FakeRHand.SetDead();
				}
				break;
			case SubStatus.Phase1:
				base.SoundSource.PlaySE("BossSE04", "bs035_onkosig08");
				base.SoundSource.PlaySE("BossSE04", "bs035_onkosig09", 1.3f);
				nDeadCount = 0;
				FakeLHand.BackToPool();
				FakeRHand.BackToPool();
				LHand1.gameObject.SetActive(true);
				LHand2.gameObject.SetActive(true);
				RHand1.gameObject.SetActive(true);
				RHand2.gameObject.SetActive(true);
				break;
			case SubStatus.Phase2:
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
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				SetCurrentAnimationID(AnimationID.ANI_DEBUT_END);
				break;
			case SubStatus.Phase1:
				return;
			}
			break;
		case MainStatus.Idle:
			SetCurrentAnimationID(AnimationID.ANI_IDLE);
			break;
		case MainStatus.Skill0:
			switch (_subStatus)
			{
			default:
				return;
			case SubStatus.Phase0:
				SetCurrentAnimationID(AnimationID.ANI_Skill0_START);
				break;
			case SubStatus.Phase1:
				SetCurrentAnimationID(AnimationID.ANI_Skill0_SHOOT);
				break;
			case SubStatus.Phase2:
				if (Fire_Left_To_Right)
				{
					SetCurrentAnimationID(AnimationID.ANI_Skill0_LEFT_TO_RIGHT);
				}
				else
				{
					SetCurrentAnimationID(AnimationID.ANI_Skill0_RIGHT_TO_LEFT);
				}
				break;
			}
			break;
		case MainStatus.Skill1:
			switch (_subStatus)
			{
			default:
				return;
			case SubStatus.Phase0:
				SetCurrentAnimationID(AnimationID.ANI_Skill1_START);
				break;
			case SubStatus.Phase1:
				SetCurrentAnimationID(AnimationID.ANI_Skill1_LOOP);
				break;
			case SubStatus.Phase2:
				SetCurrentAnimationID(AnimationID.ANI_Skill1_ATK_START);
				break;
			case SubStatus.Phase3:
				SetCurrentAnimationID(AnimationID.ANI_Skill1_ATK_LOOP);
				break;
			case SubStatus.Phase4:
				SetCurrentAnimationID(AnimationID.ANI_Skill1_END);
				break;
			}
			break;
		case MainStatus.Skill2:
			switch (_subStatus)
			{
			default:
				return;
			case SubStatus.Phase0:
				SetCurrentAnimationID(AnimationID.ANI_Skill2_START);
				break;
			case SubStatus.Phase1:
				SetCurrentAnimationID(AnimationID.ANI_Skill2_LOOP);
				break;
			case SubStatus.Phase2:
				SetCurrentAnimationID(AnimationID.ANI_Skill2_END);
				break;
			}
			break;
		case MainStatus.Skill3:
			switch (_subStatus)
			{
			default:
				return;
			case SubStatus.Phase0:
				if (_IsLeft)
				{
					SetCurrentAnimationID(AnimationID.ANI_Skill3_LEFT_START);
				}
				else
				{
					SetCurrentAnimationID(AnimationID.ANI_Skill3_RIGHT_START);
				}
				break;
			case SubStatus.Phase1:
				if (_IsLeft)
				{
					SetCurrentAnimationID(AnimationID.ANI_Skill3_LEFT_LOOP);
				}
				else
				{
					SetCurrentAnimationID(AnimationID.ANI_Skill3_RIGHT_LOOP);
				}
				break;
			case SubStatus.Phase2:
				if (_IsLeft)
				{
					SetCurrentAnimationID(AnimationID.ANI_Skill3_LEFT_END);
				}
				else
				{
					SetCurrentAnimationID(AnimationID.ANI_Skill3_RIGHT_END);
				}
				break;
			}
			break;
		case MainStatus.Hurt:
			SetCurrentAnimationID(AnimationID.ANI_HURT);
			break;
		case MainStatus.Die:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				SetCurrentAnimationID(AnimationID.ANI_IDLE);
				break;
			case SubStatus.Phase1:
				SetCurrentAnimationID(AnimationID.ANI_DEAD);
				break;
			case SubStatus.Phase2:
				return;
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
			if (!haveSpawn)
			{
				haveSpawn = true;
				Debug.Log(string.Concat("LHandPos   ", LHandPos.position, "RHandPos   ", RHandPos.position));
				MOB_TABLE tMOB_TABLE = ManagedSingleton<OrangeDataManager>.Instance.MOB_TABLE_DICT[(int)EnemyWeapons[4].BulletData.f_EFFECT_X];
				EnemyControllerBase enemyControllerBase = StageUpdate.StageSpawnEnemyByMob(tMOB_TABLE, sNetSerialID + "0");
				if ((bool)enemyControllerBase)
				{
					FakeLHand = new EM173_Controller();
					FakeLHand = enemyControllerBase.gameObject.GetComponent<EM173_Controller>();
					if ((bool)FakeLHand)
					{
						LHand1.gameObject.SetActive(false);
						LHand2.gameObject.SetActive(false);
						FakeLHand.SetPositionAndRotation(new Vector3(LHandPos.position.x, LHandPos.position.y, 0f), true);
						FakeLHand.gameObject.name = "Left_Hand";
						FakeLHand.SetActive(true);
					}
				}
				EnemyControllerBase enemyControllerBase2 = StageUpdate.StageSpawnEnemyByMob(tMOB_TABLE, sNetSerialID + "1");
				if ((bool)enemyControllerBase2)
				{
					FakeRHand = new EM173_Controller();
					FakeRHand = enemyControllerBase2.gameObject.GetComponent<EM173_Controller>();
					if ((bool)FakeRHand)
					{
						RHand1.gameObject.SetActive(false);
						RHand2.gameObject.SetActive(false);
						FakeRHand.SetPositionAndRotation(new Vector3(RHandPos.position.x, RHandPos.position.y, 0f), false);
						FakeRHand.gameObject.name = "Right_Hand";
						FakeRHand.SetActive(true);
					}
				}
			}
			SetStatus(MainStatus.Idle);
			break;
		case MainStatus.Idle:
		{
			float num = 5f;
			if ((bool)Target)
			{
				num = Math.Abs(Target.transform.position.x - _transform.position.x);
			}
			mainStatus = (MainStatus)((!(num < 3.2f)) ? WeightRandom(SkillWeightArray, 2) : WeightRandom(SkillWeightArrayMid, 2));
			break;
		}
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
					SetStatus(MainStatus.Debut, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if (_currentFrame > 0.97f && _introReady && !bWaitNetStatus)
				{
					UpdateRandomState();
				}
				break;
			}
			break;
		case MainStatus.Idle:
			if (!bWaitNetStatus)
			{
				Target = _enemyAutoAimSystem.GetClosetPlayer();
				if ((bool)Target)
				{
					TargetPos = Target.Controller.LogicPosition;
					UpdateRandomState();
				}
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
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill0, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Idle);
					ReloadTimer.TimerStop();
				}
				if (_currentFrame > 0.25f && _currentFrame < 0.72f && ReloadTimer.GetMillisecond() > EnemyWeapons[0].BulletData.n_FIRE_SPEED)
				{
					Vector3 worldPos2 = (CannonTransform.position + CannonShootTransform.position) / 2f;
					worldPos2.z = 0f;
					Vector3 pDirection = CannonShootTransform.position - CannonTransform.position;
					pDirection.z = 0f;
					FireSpray = BulletBase.TryShotBullet(EnemyWeapons[0].BulletData, worldPos2, pDirection, null, selfBuffManager.sBuffStatus, EnemyData, targetMask) as BS041_FireSpray;
					ReloadTimer.TimerReset();
					ReloadTimer.TimerStart();
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
				}
				break;
			case SubStatus.Phase1:
				if (_currentFrame > 0.5f && !Thunderbolt[0].IsActivate)
				{
					Thunderbolt[0].Active(targetMask);
					Thunderbolt[1].Active(targetMask);
				}
				else if (_currentFrame > 1f)
				{
					Thunderbolt[0].BackToPool();
					Thunderbolt[1].BackToPool();
					SetStatus(MainStatus.Skill1, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill1, SubStatus.Phase3);
				}
				break;
			case SubStatus.Phase3:
				if (_currentFrame > 0.5f && !Thunderbolt[0].IsActivate)
				{
					Thunderbolt[0].Active(targetMask);
					Thunderbolt[1].Active(targetMask);
				}
				else if (_currentFrame > 1f)
				{
					Thunderbolt[0].BackToPool();
					Thunderbolt[1].BackToPool();
					SetStatus(MainStatus.Skill1, SubStatus.Phase4);
				}
				break;
			case SubStatus.Phase4:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Idle);
				}
				break;
			}
			break;
		case MainStatus.Skill2:
		{
			Target = _enemyAutoAimSystem.GetClosetPlayer();
			preangle = angle;
			Vector3 vector;
			if ((bool)Target)
			{
				vector = Target.transform.position - CannonTransform.position;
				vector.y += 0.5f;
				vector.z = 0f;
			}
			else
			{
				vector = Vector3.down;
			}
			angle = Vector3.Angle(vector.normalized, Vector3.down);
			if (vector.x < 0f)
			{
				angle *= -1f;
			}
			if (angle > 90f)
			{
				vector = Vector3.right;
				angle = 90f;
			}
			else if (angle < -90f)
			{
				vector = Vector3.left;
				angle = -90f;
			}
			angle = Mathf.Lerp(preangle, angle, 0.1f);
			_animator.SetFloat(_HashAngle, angle);
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill2, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if (_currentFrame > 1f && ShootTimes > 0)
				{
					SetStatus(MainStatus.Skill2, SubStatus.Phase1);
				}
				else if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill2, SubStatus.Phase2);
				}
				if (_currentFrame < 1f && _currentFrame > NextShot && ShootTimes > 0)
				{
					NextShot = _currentFrame + 0.34f;
					ShootTimes--;
					Vector3 position2 = CannonShootTransform.position;
					vector = CannonShootTransform.position - CannonTransform.position;
					vector.z = 0f;
					position2.z = 0f;
					CircleBullet = BulletBase.TryShotBullet(EnemyWeapons[1].BulletData, position2, vector, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
				}
				break;
			case SubStatus.Phase2:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Idle);
				}
				break;
			}
			break;
		}
		case MainStatus.Skill3:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (_currentFrame > 1f)
				{
					if (LeftHandPress.IsActivate)
					{
						LeftHandPress.BackToPool();
					}
					if (RightHandPress.IsActivate)
					{
						RightHandPress.BackToPool();
					}
					SetStatus(MainStatus.Skill3, SubStatus.Phase1);
				}
				else if (_currentFrame > 0.5f && !LeftHandPress.IsActivate && !RightHandPress.IsActivate)
				{
					if (_IsLeft)
					{
						RightHandPress.Active(targetMask);
					}
					else
					{
						LeftHandPress.Active(targetMask);
					}
				}
				break;
			case SubStatus.Phase1:
				if (_currentFrame > 0.5f)
				{
					SetStatus(MainStatus.Skill3, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Idle);
				}
				break;
			}
			break;
		case MainStatus.Skill4:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (_currentFrame > 0.3f)
				{
					SetStatus(MainStatus.Skill4, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if (_currentFrame > NextShootFrame && PreLaser < LaserNum * 2)
				{
					Vector3 worldPos = new Vector3(-2f, 2f, 0f) + LeftShoulder.position;
					BulletBase.TryShotBullet(EnemyWeapons[3].BulletData, worldPos, new Vector3(-2.5f, 2.5f, 0f), null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
					float num = Mathf.Abs(Thunderbolt[1].transform.position.x - Thunderbolt[0].transform.position.x) / (float)LaserNum;
					Vector3 position = new Vector3(Thunderbolt[PreLaser / LaserNum].transform.position.x + (float)(PreLaser % LaserNum - (LaserNum - 1) / 2 - PreLaser / LaserNum) * (num * 0.8f), LaserBullet[PreLaser].transform.position.y, 0f);
					LaserBullet[PreLaser].transform.position = position;
					MonoBehaviourSingleton<FxManager>.Instance.PlayReturn<psSwingTarget>("fxuseTarget", LaserBullet[PreLaser].transform.position, Quaternion.Euler(0f, 0f, Vector2.SignedAngle(Vector2.right, Vector2.down)), Array.Empty<object>()).SetEffect(20f, new Color(0.6f, 0f, 0.5f, 0.7f), new Color(0.6f, 0f, 0.5f), 1f);
					PreLaser++;
					NextShootFrame += 0.1f;
				}
				if (_currentFrame > NextUseFrame && Laser < LaserNum * 2)
				{
					MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_MHR_Magnamalo_003", LaserBullet[Laser].transform, Quaternion.Euler(0f, 0f, 0f), Vector3.one, Array.Empty<object>());
					Laser++;
					NextUseFrame += 0.1f;
				}
				if (_currentFrame > NextStartFrame && StartLaser < LaserNum * 2)
				{
					LaserBullet[StartLaser].Active(targetMask);
					StartLaser++;
					NextStartFrame += 0.1f;
				}
				if (_currentFrame > NextCloseFrame && CloseLaser < LaserNum * 2)
				{
					LaserBullet[CloseLaser].BackToPool();
					CloseLaser++;
					NextCloseFrame += 0.1f;
					if (LaserNum * 2 - 1 < CloseLaser)
					{
						SetStatus(MainStatus.Skill4, SubStatus.Phase2);
					}
				}
				break;
			case SubStatus.Phase2:
				if (_currentFrame > 0.2f)
				{
					SetStatus(MainStatus.Idle);
				}
				break;
			}
			break;
		case MainStatus.Hurt:
			if (GameLogicUpdateManager.GameFrame > ShakeFrame)
			{
				SetStatus(MainStatus.Idle);
			}
			break;
		case MainStatus.Die:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (_currentFrame > 0.4f)
				{
					if (nDeadCount > 10)
					{
						SetStatus(MainStatus.Die, SubStatus.Phase1);
					}
					else
					{
						nDeadCount++;
					}
				}
				break;
			case SubStatus.Phase1:
				if (_currentFrame > 0.6f)
				{
					SetStatus(MainStatus.Die, SubStatus.Phase2);
				}
				break;
			}
			break;
		}
	}

	public void UpdateFunc()
	{
		if (Activate || _mainStatus == MainStatus.Die)
		{
			base.transform.localPosition = Vector3.MoveTowards(base.transform.localPosition, Controller.LogicPosition.vec3, distanceDelta);
		}
	}

	public override void SetActive(bool isActive)
	{
		base.DeadPlayCompleted = true;
		IgnoreGravity = true;
		base.SetActive(isActive);
		if (isActive)
		{
			SetStatus(MainStatus.Debut);
			base.SoundSource.PlaySE("BossSE04", "bs035_onkosig11", 4f);
			base.SoundSource.PlaySE("BossSE04", "bs035_onkosig11", 5f);
			base.SoundSource.PlaySE("BossSE04", "bs035_onkosig07", 6.7f);
			for (int i = 0; i < LaserBullet.Length; i++)
			{
				LaserBullet[i].UpdateBulletData(EnemyWeapons[2].BulletData);
				LaserBullet[i].SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			}
		}
		else
		{
			for (int j = 0; j < LaserBullet.Length; j++)
			{
				LaserBullet[j].BackToPool();
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
		pos.z = 2f;
		base.transform.localScale = new Vector3(_transform.localScale.x, _transform.localScale.y, _transform.localScale.z);
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
		if ((int)Hp > 0)
		{
			SetColliderEnable(true);
		}
		if (InGame)
		{
			Activate = true;
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
			if ((bool)_collideBullet)
			{
				_collideBullet.BackToPool();
			}
			Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.CLOSE_FX);
			StageUpdate.SlowStage();
			SetColliderEnable(false);
			SetStatus(MainStatus.Die);
		}
	}

	public override ObscuredInt Hurt(HurtPassParam tHurtPassParam)
	{
		bool flag = false;
		ObscuredInt obscuredInt = base.Hurt(tHurtPassParam);
		if ((float)(int)obscuredInt < (float)(int)MaxHp * 0.8f)
		{
			LaserNum = WeakLaserNum;
			if (!HasBreak[0])
			{
				HasBreak[0] = true;
				flag = true;
				PartMesh[0].enabled = false;
				PartMesh[1].enabled = false;
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxhit_MHR_Magnamalo_001", PartPos[0], Quaternion.identity, Vector3.one, Array.Empty<object>());
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxhit_MHR_Magnamalo_001", PartPos[1], Quaternion.identity, Vector3.one, Array.Empty<object>());
			}
		}
		if ((float)(int)obscuredInt < (float)(int)MaxHp * 0.7f)
		{
			SkillWeightArray[2] = 0;
			SkillWeightArrayMid[2] = 0;
			if (!HasBreak[1])
			{
				HasBreak[1] = true;
				flag = true;
				PartMesh[2].enabled = false;
				PartMesh[3].enabled = false;
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxhit_MHR_Magnamalo_001", PartPos[2], Quaternion.identity, Vector3.one, Array.Empty<object>());
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxhit_MHR_Magnamalo_001", PartPos[3].position + Vector3.left * 2f, Quaternion.identity, new object[1] { Vector3.one });
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxhit_MHR_Magnamalo_001", PartPos[3].position + Vector3.right * 2f, Quaternion.identity, new object[1] { Vector3.one });
			}
		}
		if ((float)(int)obscuredInt < (float)(int)MaxHp * 0.5f)
		{
			FakeLHand.SetWeak(WeakHandSpeed);
			if (!HasBreak[2])
			{
				HasBreak[2] = true;
				flag = true;
				PartMesh[4].enabled = false;
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxhit_MHR_Magnamalo_001", FakeLHand.GetHandPart(), Quaternion.identity, Vector3.one, Array.Empty<object>());
			}
		}
		if ((float)(int)obscuredInt < (float)(int)MaxHp * 0.4f)
		{
			FakeRHand.SetWeak(WeakHandSpeed);
			if (!HasBreak[3])
			{
				HasBreak[3] = true;
				flag = true;
				PartMesh[5].enabled = false;
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxhit_MHR_Magnamalo_001", FakeRHand.GetHandPart(), Quaternion.identity, Vector3.one, Array.Empty<object>());
			}
		}
		if ((float)(int)obscuredInt < (float)(int)MaxHp * 0.3f)
		{
			SkillWeightArray[0] = 0;
			SkillWeightArrayMid[0] = 0;
			if (!HasBreak[4])
			{
				HasBreak[4] = true;
				flag = true;
				PartMesh[6].enabled = false;
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxhit_MHR_Magnamalo_001", PartPos[6], Quaternion.identity, Vector3.one, Array.Empty<object>());
				PlaySE("BossSE04", "bs035_onkosig06");
				PlaySE("BossSE04", "bs035_onkosig13");
				EventManager.StageEventCall stageEventCall = new EventManager.StageEventCall();
				stageEventCall.nID = 998;
				stageEventCall.tTransform = _transform;
				Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_EVENT_CALL, stageEventCall);
			}
		}
		if (flag && (int)Hp > 0 && _mainStatus != MainStatus.Die)
		{
			SetStatus(MainStatus.Hurt);
		}
		return obscuredInt;
	}
}
