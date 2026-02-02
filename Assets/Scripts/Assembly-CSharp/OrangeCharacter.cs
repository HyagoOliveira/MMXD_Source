#define RELEASE
using System;
using System.Collections;
using System.Collections.Generic;
using Better;
using CallbackDefs;
using CodeStage.AntiCheat.ObscuredTypes;
using CriWare;
using NaughtyAttributes;
using Newtonsoft.Json;
using OrangeAudio;
using OrangeCriRelay;
using StageLib;
using UnityEngine;
using enums;

[RequireComponent(typeof(Controller2D))]
[RequireComponent(typeof(OrangeCriSource))]
public abstract class OrangeCharacter : StageObjBase, IManagedUpdateBehavior, IManagedLateUpdateBehavior, IAimTarget, ILogicUpdate
{
	public enum MainStatus
	{
		NONE = -1,
		IDLE = 0,
		CROUCH = 1,
		HURT = 2,
		WALK = 3,
		DASH = 4,
		AIRDASH = 5,
		JUMP = 6,
		FALL = 7,
		WALLKICK = 8,
		WALLGRAB = 9,
		GIGA_ATTACK = 10,
		TELEPORT_IN = 11,
		TELEPORT_OUT = 12,
		SLASH = 13,
		SKILL = 14,
		RIDE_ARMOR = 15,
		MAX_STATUS = 16
	}

	public enum SubStatus
	{
		NONE = -1,
		TELEPORT_POSE = 0,
		WIN_POSE = 1,
		RIDE_ARMOR = 2,
		IDLE = 3,
		LAND = 4,
		DASH_END = 5,
		CROUCH_UP = 6,
		SKILL_IDLE = 7,
		SLASH1_END = 37,
		SLASH2_END = 38,
		SLASH3_END = 39,
		SLASH4_END = 40,
		SLASH5_END = 41,
		CROUCH = 0,
		CROUCH_LOOP = 1,
		CROUCH_SLASH1 = 2,
		CROUCH_SLASH1_END = 3,
		HURT_BEGIN = 4,
		HURT_LOOP = 5,
		STEP = 0,
		WALK = 1,
		WALKBACK = 2,
		WALK_SLASH1 = 3,
		WALK_SLASH2 = 4,
		WALKSLASH1_END = 5,
		WALKSLASH2_END = 6,
		WALLGRAB_BEGIN = 0,
		WALLGRAB_TURN = 1,
		WALLGRAB_END = 2,
		WALLGRAB_SLASH = 3,
		WALLGRAB_SLASH_END = 4,
		WALLKICK = 0,
		WALLKICK_END = 1,
		DASH = 0,
		DASH_SLASH1 = 1,
		DASH_SLASH1_END = 2,
		DASH_SLASH2 = 3,
		DASH_SLASH2_END = 4,
		JUMP = 0,
		JUMP_END = 1,
		JUMP_SLASH = 2,
		JUMP_HOVERING = 3,
		FALL = 0,
		AIRDASH_END = 1,
		FALL_SLASH = 2,
		SKILL_FALL = 3,
		GIGA_ATTACK_START = 33,
		GIGA_ATTACK_END = 34,
		SLASH1 = 0,
		SLASH2 = 1,
		SLASH3 = 2,
		SLASH4 = 3,
		SLASH5 = 4,
		PUNCH_GROUND = 0,
		THROW = 1,
		NOVASTRIKE_BEGIN = 2,
		NOVASTRIKE_LOOP = 3,
		MARINO_DARTS_GROUND = 4,
		MARINO_DARTS_AIR = 5,
		MARINO_DASH_BEGIN = 6,
		MARINO_DASH_LOOP = 7,
		PALETTE_ATTACK_GROUND = 8,
		PALETTE_ATTACK_AIR = 9,
		PALETTE_ATTACK_END = 10,
		PALETTE_ARROW_GROUND = 11,
		PALETTE_ARROW_AIR = 12,
		VAVA_CANNON_GROUND = 13,
		VAVA_CANNON_AIR = 14,
		VAVA_KNEE_GROUND = 15,
		VAVA_KNEE_AIR = 16,
		SLASH_SKILL0_START = 17,
		SLASH_SKILL0_LOOP = 18,
		SKILL0 = 19,
		SKILL0_1 = 20,
		SKILL0_2 = 21,
		SKILL0_3 = 22,
		SKILL0_4 = 23,
		SKILL0_5 = 24,
		SKILL0_6 = 25,
		SKILL0_7 = 26,
		SKILL0_8 = 27,
		SKILL0_9 = 28,
		SKILL0_10 = 29,
		SKILL0_11 = 30,
		SKILL0_12 = 31,
		SKILL0_13 = 32,
		SKILL0_14 = 33,
		SKILL0_15 = 34,
		SKILL0_16 = 35,
		SKILL0_17 = 36,
		SKILL0_18 = 37,
		SKILL0_19 = 38,
		SKILL0_20 = 39,
		SKILL0_21 = 40,
		SKILL0_22 = 41,
		SKILL0_23 = 42,
		SKILL0_24 = 43,
		SKILL0_25 = 44,
		SKILL0_26 = 45,
		SKILL0_27 = 46,
		SKILL0_28 = 47,
		SKILL0_29 = 48,
		SKILL1 = 49,
		SKILL1_1 = 50,
		SKILL1_2 = 51,
		SKILL1_3 = 52,
		SKILL1_4 = 53,
		SKILL1_5 = 54,
		SKILL1_6 = 55,
		SKILL1_7 = 56,
		SKILL1_8 = 57,
		SKILL1_9 = 58,
		SKILL1_10 = 59,
		SKILL1_11 = 60,
		SKILL1_12 = 61,
		SKILL1_13 = 62,
		SKILL1_14 = 63,
		SKILL1_15 = 64,
		SKILL1_16 = 65,
		SKILL1_17 = 66,
		SKILL1_18 = 67,
		SKILL1_19 = 68,
		SKILL1_20 = 69,
		SKILL1_21 = 70,
		SKILL1_22 = 71,
		SKILL1_23 = 72,
		SKILL1_24 = 73,
		SKILL1_25 = 74,
		SKILL1_26 = 75,
		SKILL1_27 = 76,
		SKILL1_28 = 77,
		SKILL1_29 = 78
	}

	public enum SKIN_SUB_TYPE
	{
		NONE = -1,
		ALL = 0,
		VARIANT = 1
	}

	public enum DASH_TYPE
	{
		X_SERIES = 0,
		CLASSIC = 1
	}

	[Flags]
	public enum ABILITY
	{
		DASH = 1,
		AIR_DASH = 2,
		DOUBLE_JUMP = 4,
		HOVERING = 8,
		DEFAULT = 7
	}

	public enum FLY_DIR
	{
		HORIZONTAL = 0,
		UP = 1,
		DOWN = 2
	}

	public enum SoundVolumeDefine
	{
		LocalPlayer = 100,
		Co_opPlayer = 90,
		PvP1on1Enemy = 100,
		PvP3on3Teammate1 = 50,
		PvP3on3Teammate2 = 50,
		PvP3on3Emeny1 = 50,
		PvP3on3Emeny2 = 50,
		PvP3on3Emeny3 = 50
	}

	public const int MAX_SKILL_IDLE = 30;

	public const int MAX_SKILL_FALL = 30;

	private const string DEFAULT_CONTROLLER_NAME = "NewEmptyController";

	protected bool _localPlayer;

	public Better.Dictionary<MainStatus, float> AnimatorModelShiftYOverride;

	[BoxGroup("OrangeCharacter Param")]
	[SerializeField]
	[NaughtyAttributes.ReadOnly]
	private MainStatus _mainStatus = MainStatus.NONE;

	[BoxGroup("OrangeCharacter Param")]
	[SerializeField]
	[NaughtyAttributes.ReadOnly]
	private SubStatus _subStatus = SubStatus.NONE;

	public static readonly Quaternion NormalQuaternion = Quaternion.Euler(0f, 0f, 0f);

	public static readonly Quaternion ReversedQuaternion = Quaternion.Euler(0f, 180f, 0f);

	private static readonly int StepSpeed = Mathf.RoundToInt(1000f * OrangeBattleUtility.PPU * OrangeBattleUtility.FPS);

	public static readonly VInt MaxWallSlideGravity = new VInt(-1.5f * OrangeBattleUtility.PPU * OrangeBattleUtility.FPS);

	private static readonly VInt3 FP_WALLFALL_FORCE = new VInt3(250, 2000, 0);

	public static readonly int MaxDashChance = 1;

	public static readonly float MaxDashDistance = 30f;

	public static readonly float MaxAirDashDistance = 18f;

	public static readonly int BusterDelay = 32;

	public static readonly int AutoReloadDelay = OrangeConst.AUTO_RELOAD;

	public static readonly float AutoReloadPercent = (float)OrangeConst.AUTO_RELOAD_PERCENT * 0.01f;

	public Vector2 _normalHitboxSize = new Vector2(0.76f, 1.4f);

	public Vector2 _halfHitboxSize = new Vector2(0.76f, 0.7f);

	public Vector2 _normalHitboxOffset = new Vector2(0f, 0.7f);

	public Vector2 _halfHitboxOffset = new Vector2(0f, 0.35f);

	private bool _isHalfBox;

	private List<BulletDetails> _listBulletWaitforShoot = new List<BulletDetails>();

	private List<SlashDetails> _listSlashCache = new List<SlashDetails>();

	private float[] _slashTiming;

	private bool _ignoreGravity;

	private bool _ignoreGlobalVelocity;

	private bool _ignoreVelocityExtra;

	private VInt _gravityModifier = new VInt(1f);

	public static int WalkSpeed;

	public static int JumpSpeed;

	public static int DashSpeed;

	[SerializeField]
	protected float _moveSpeedMultiplier;

	private int _nStatusBit;

	private bool bIceSlide;

	private float fIceSlideParam = 1f;

	private bool bIceSlideSameDir;

	private float fIceSlide;

	public List<string> listQuickSand = new List<string>();

	public bool bQuickSand;

	public float fQuickSand = 0.5f;

	[BoxGroup("OrangeCharacter Param")]
	[SerializeField]
	[NaughtyAttributes.ReadOnly]
	private float _currentFrame;

	private float _prevFrame;

	private Callback LinkUpdatePrepare;

	private Callback LinkUpdateCall;

	private bool _usingVehicle;

	public RideBaseObj refRideBaseObj;

	private bool _isDash;

	private bool _dashEnable = true;

	private bool _airDashEnable = true;

	private bool _doubleJumpEnable = true;

	private int _dashChance;

	private bool _hoveringEnable;

	private bool _isHovering;

	private float MaxHoveringTicks = 180f;

	private int HoveringSpeed = WalkSpeed;

	private float HoveringHighLimit = 2f;

	private float HoveringStartHeight;

	[BoxGroup("OrangeCharacter Param")]
	[SerializeField]
	[NaughtyAttributes.ReadOnly]
	private int _lastSetAnimateFrame;

	[BoxGroup("OrangeCharacter Param")]
	[SerializeField]
	[NaughtyAttributes.ReadOnly]
	private HumanBase.AnimateId _animateID;

	[BoxGroup("OrangeCharacter Param")]
	[SerializeField]
	[NaughtyAttributes.ReadOnly]
	private HumanBase.AnimateId _animateIDPrev;

	private AnimatorBase _animatorBase;

	private Controller2D _controller2D;

	[HideInInspector]
	public AnimationParameters AnimationParams;

	private OrangeTimer _analogReleaseTimer;

	private OrangeTimer _analogPressTimer;

	private OrangeTimer _dashTimer;

	private OrangeTimer _wallKickTimer;

	private OrangeTimer _doubleTapTimer;

	private OrangeTimer _shootTimer;

	private OrangeTimer _rangeWpVoiceTimer;

	private OrangeTimer _plHurtVoiceTimer;

	private OrangeTimer _invincibleTimer;

	private OrangeTimer _weaponSwitchTimer;

	private OrangeTimer _pvpCCsafety;

	private OrangeTimer _hoveringTimer;

	protected Setting _setting = new Setting(true);

	protected VInt3 _velocity;

	protected VInt3 _velocityExtra;

	protected VInt3 _velocityShift;

	protected VInt3 _velocityForceField = VInt3.zero;

	[HideInInspector]
	public string WallKickSparkFx;

	[HideInInspector]
	public string LandFx;

	[HideInInspector]
	public string JumpUpFx;

	[HideInInspector]
	public string JumpLeftFx;

	[HideInInspector]
	public string JumpRightFx;

	[HideInInspector]
	public string DashSmokeFx;

	[HideInInspector]
	public string TeleportInFx;

	[HideInInspector]
	public string TeleportOutFx;

	private Callback _refWinPoseEndCB;

	private bool _teleportParticleFlag;

	private int _teleportOutParticlePhase;

	private int _freshWinPose;

	public bool IsTeleporting;

	public bool IsTeleportOut;

	public bool UseTeleportFollowCamera;

	private bool _freshCreateBullet;

	private sbyte _isShoot;

	private sbyte _isShootPrev;

	private bool _isHoldShoot;

	private bool _isHoldLeft;

	private bool _isHoldRight;

	private int _currentWeapon;

	private int _currentActiveSkill;

	private CharacterMaterial _characterMaterial;

	private CollideBullet _collideBullet;

	private Vector3 _shootDirection = Vector3.zero;

	protected PlayerAutoAimSystem _playerAutoAimSystem;

	protected IAimTarget _refAimTargetLogicUpdate;

	public Vector3 AimExtendPosition = Vector3.zero;

	public DASH_TYPE CharacterDashType;

	[HideInInspector]
	public Transform[] ExtraTransforms;

	protected internal Renderer[] ExtraMeshClose;

	protected internal Renderer[] ExtraMeshOpen;

	public SkinnedMeshRenderer[] _handMesh;

	protected ButtonId _doubleTapBtn;

	protected ButtonId _dashTriggerBtn;

	public WeaponStruct[] PlayerWeapons;

	public WeaponStruct[] PlayerSkills;

	public WeaponStruct[] PlayerFSkills;

	public int[] PlayerFSkillsID;

	public int SkillComboBuffIndex = 1;

	private LineRenderer _lineRenderer;

	public ObjInfoBar objInfoBar;

	public bool IsHitStop;

	private bool _preBelow;

	public int CharacterID;

	[HideInInspector]
	public CHARACTER_TABLE CharacterData;

	private int _wallKickMask = -1;

	private bool bIsDeadEnd;

	private List<Renderer> listRenders = new List<Renderer>();

	public string sPlayerName = "";

	public PlayerBuilder.PlayerBuildParam SetPBP = new PlayerBuilder.PlayerBuildParam();

	public int nBulletRecordID;

	public Vector3 vLastStandPt;

	public Vector3 vLastMovePt;

	private const float WALLGRAB_CHECK_DISTAND = 2f;

	private bool bLockBulletForNextStatus;
    [Obsolete]
    public CallbackObj PlayerPressDashCB;

	public Callback PlayerReleaseDashCB;

	public Callback PlayerPressLeftCB;

	public Callback PlayerHeldLeftCB;

	public Callback PlayerReleaseLeftCB;

	public Callback PlayerPressRightCB;

	public Callback PlayerHeldRightCB;

	public Callback PlayerReleaseRightCB;

	public Callback PlayerHeldUpCB;

	public Callback PlayerReleaseUpCB;

	public Callback PlayerPressDownCB;

	public Callback PlayerHeldDownCB;

	public Callback PlayerReleaseDownCB;
    [Obsolete]
    public CallbackObj PlayerHeldLeftRightSkillCB;
    [Obsolete]
    public CallbackObj PlayerReleaseLeftRightSkillCB;

	public Callback PlayerHeldShootCB;

	public Callback PlayerReleaseShootCB;

	public Callback PlayerPressJumpCB;

	public Callback PlayerReleaseJumpCB;

	public Callback PlayerResetPressJumpCB;
    [Obsolete]
    public CallbackIdx PlayerPressSkillCB;
    [Obsolete]
    public CallbackIdx PlayerHeldSkillCB;
    [Obsolete]
    public CallbackIdx PlayerReleaseSkillCB;
    [Obsolete]
    public CallbackIdx PlayerPressSkillCharacterCallCB;
    [Obsolete]
    public CallbackIdx PlayerReleaseSkillCharacterCallCB;

	public Callback PlayerSkillLandCB;

	public Callback<VInt3> AddForceFieldCB;

	public Func<int, bool> CanPlayerPressSkillFunc;

	public Callback PlayerPressSelectCB;

	public Callback PlayerPressGigaAttackCB;

	public Callback PlayerPressChipCB;

	public Callback<Voice> PlayVoiceCB;

	public Callback<CharaSE> PlayCharaSeCB;

	public Callback<MapCollisionEvent.MapCollisionEnum> mapCollisionCB;

	public Callback CheckSkillEvt;

	public Callback ClearSkillEvt;

	public Callback CheckSkillLockDirectionEvt;

	public Func<CriAtomExPlayback> TeleportOutCharacterSE;

	public Callback TeleportInExtraEffectEvt;

	public Callback TeleportInCharacterDependEvt;

	public Callback TeleportOutCharacterDependEvt;

	public Callback StageTeleportInCharacterDependEvt;

	public Callback StageTeleportOutCharacterDependEvt;

	public Callback TeleportInCharacterDependeEndEvt;

	public Callback PlayTeleportOutEffectEvt;

	public Func<int, int, bool> CheckActStatusEvt;

	public Action<MainStatus, SubStatus> AnimationEndCharacterDependEvt;

	public Action<MainStatus, SubStatus> SetStatusCharacterDependEvt;

	public Func<HumanBase.AnimateId, bool> GetUpperAnimateKeepFlagEvt;

	public Callback OverrideAnimatorParamtersEvt;

	public Callback StopHoveringEvt;

	public Func<bool> PlWallJumpCheckEvt;

	public Func<int, bool> PlWallStopCheckEvt;

	public Func<RideBaseObj, bool> EnterRideArmorEvt;

	public Action<RideBaseObj> LeaveRideArmorEvt;

	public Func<bool> CheckAvalibaleForRideArmorEvt;

	public Action<WeaponStruct> UpdateAimRangeByWeaponEvt;
    [Obsolete]
    public CallbackObjs ChangeComboSkillEventEvt;

	public Func<float> GetCurrentAimRangeEvt;

	public Func<bool> CheckDashLockEvt;

	public Func<HurtPassParam, bool> GuardCalculateEvt;

	public Callback<HurtPassParam> GuardHurtEvt;

	public Action UpdateFuncEndEvt;

	public Func<int, bool> CheckPetActiveEvt;

	public Func<WeaponStruct> GetCurrentWeaponObjEvt;

	public Callback<bool> LockAnimatorEvt;

	public Callback<bool> DeadAreaLockEvt;

	public bool LockInput = true;

	protected internal bool LockWeapon;

	protected internal bool LockSkill;

	protected internal bool ReverseRightAndLeft;

	protected internal bool BanAutoAim;

	private int _nomoveStack;

	protected int nLockInputCtrl;

	public bool bNeedUpdateAlways;

	private readonly List<StageCtrlInsTruction> _stageCtrlInstructions = new List<StageCtrlInsTruction>();

	private bool isPushNewBullet;

	private bool _autoAimCalibrated;

	private Vector3? _calibrate;

	public bool UseAutoAim = true;

	private float _aimDeadZone = 2.25f;

	private int _analogIDprev = -1;

	private bool _forceManualShoot;

	private bool _bPlayedWinPose;

	public WeaponStatus lastCreateBulletWeaponStatus;

	public Transform lastCreateBulletTransform;

	public Vector3? lastCreateBulletShotDir;

	private int _nTempDashChance;

	private int _nJumpCount;

	private float jumpSlashCount;

	protected WeaponStruct lastWeaponStruct;

	private int _stunStack;

	protected bool _isJack;

	protected bool _releaseJack;

	private bool _leaveRideArmorJump;

	protected ChargeShootObj _chargeShootObj;

	protected bool _bFloatingFlag;

	protected float _easeSpeed = 0.25f;

	protected float[] _globalWaypoints = new float[2];

	protected int _fromWaypointIndex;

	protected float _percentBetweenWaypoints;

	protected Coroutine _jumpThroughCoroutine;

	protected bool useWeaponSE = true;

	protected bool isUseKabesuriSE;

	public bool teleportInVoicePlayed;

	public bool PlayTeleportInVoice = true;

	protected int hurtVoiceLimit = -1;

	public bool UseHitSE = true;

	protected int _weakVoice;

	protected internal bool MuteBullet;

	private string _VoiceID = "";

	private string _CharaSEID = "";

	private string _SkillSEID = "";

	private OrangeCriSource _ss;

	private List<CharacterParam> listStatusParams = new List<CharacterParam>();

	public bool IsLocalPlayer
	{
		get
		{
			return _localPlayer;
		}
		set
		{
			SetLocalPlayer(value);
		}
	}

	public MainStatus CurMainStatus
	{
		get
		{
			return _mainStatus;
		}
		set
		{
			_mainStatus = value;
		}
	}

	public SubStatus CurSubStatus
	{
		get
		{
			return _subStatus;
		}
		set
		{
			_subStatus = value;
		}
	}

	public bool IgnoreGravity
	{
		get
		{
			return _ignoreGravity;
		}
		set
		{
			_ignoreGravity = value;
			SetVerticalSpeed(0);
		}
	}

	public VInt GravityMultiplier
	{
		get
		{
			return _gravityModifier;
		}
		set
		{
			_gravityModifier = value;
		}
	}

	protected int JumpSpeedEx
	{
		get
		{
			CharacterControlBase component = GetComponent<CharacterControlBase>();
			if ((bool)component)
			{
				return component.JumpSpeed();
			}
			return JumpSpeed;
		}
		private set
		{
		}
	}

	private int DashSpeedEx
	{
		get
		{
			CharacterControlBase component = GetComponent<CharacterControlBase>();
			if ((bool)component)
			{
				return component.DashSpeed();
			}
			return DashSpeed;
		}
	}

	public int WallSlideGravity
	{
		get
		{
			CharacterControlBase component = GetComponent<CharacterControlBase>();
			if ((bool)component)
			{
				return component.WallSlideGravity();
			}
			return MaxWallSlideGravity.i;
		}
	}

	public float CurrentFrame
	{
		get
		{
			return _currentFrame;
		}
		set
		{
			_currentFrame = value;
		}
	}

	public bool UsingVehicle
	{
		get
		{
			return _usingVehicle;
		}
	}

	public bool Dashing
	{
		get
		{
			return _isDash;
		}
		set
		{
			_isDash = value;
		}
	}

	public int LastSetAnimateFrame
	{
		get
		{
			return _lastSetAnimateFrame;
		}
		set
		{
			_lastSetAnimateFrame = value;
		}
	}

	public HumanBase.AnimateId AnimateID
	{
		get
		{
			return _animateID;
		}
	}

	public HumanBase.AnimateId AnimateIDPrev
	{
		get
		{
			return _animateIDPrev;
		}
	}

	[HideInInspector]
	public AnimatorBase Animator
	{
		get
		{
			return _animatorBase;
		}
		set
		{
			_animatorBase = value;
		}
	}

	[HideInInspector]
	public Controller2D Controller
	{
		get
		{
			return _controller2D;
		}
		set
		{
			_controller2D = value;
		}
	}

	public bool SkillEnd { get; set; }

	public Setting PlayerSetting
	{
		get
		{
			return _setting;
		}
		set
		{
			_setting = value;
		}
	}

	public float DistanceDelta { get; set; }

	public VInt3 Velocity
	{
		get
		{
			return _velocity;
		}
		private set
		{
			_velocity = value;
		}
	}

	public sbyte IsShoot
	{
		get
		{
			return _isShoot;
		}
		set
		{
			_isShoot = value;
		}
	}

	public sbyte IsShootPrev
	{
		get
		{
			return _isShootPrev;
		}
		set
		{
			_isShootPrev = value;
		}
	}

	public bool FreshBullet
	{
		get
		{
			return _freshCreateBullet;
		}
		set
		{
			_freshCreateBullet = value;
		}
	}

	public int WeaponCurrent
	{
		get
		{
			return _currentWeapon;
		}
		set
		{
			_currentWeapon = value;
		}
	}

	public int CurrentActiveSkill
	{
		get
		{
			return _currentActiveSkill;
		}
		set
		{
			_currentActiveSkill = value;
		}
	}

	public CharacterMaterial CharacterMaterials
	{
		get
		{
			return _characterMaterial;
		}
		set
		{
			_characterMaterial = value;
		}
	}

	public CollideBullet BulletCollider
	{
		get
		{
			return _collideBullet;
		}
		set
		{
			_collideBullet = value;
		}
	}

	public Vector3 ShootDirection
	{
		get
		{
			return _shootDirection;
		}
		set
		{
			_shootDirection = value;
		}
	}

	public Vector3 ChargeShootDirectionXType
	{
		get
		{
			if (!IsDashing)
			{
				return ShootDirection;
			}
			return Vector3.right * base.direction;
		}
	}

	public PlayerAutoAimSystem PlayerAutoAimSystem
	{
		get
		{
			return _playerAutoAimSystem;
		}
	}

	public IAimTarget IAimTargetLogicUpdate
	{
		get
		{
			return _refAimTargetLogicUpdate;
		}
		set
		{
			_refAimTargetLogicUpdate = value;
		}
	}

	public override Vector3 AimPosition
	{
		get
		{
			return base.AimTransform.position + base.transform.localRotation * base.AimPoint + AimExtendPosition;
		}
		set
		{
			Debug.LogWarning("Not Used !");
		}
	}

	public ParticleSystem SpeedLineParticleSystem { get; set; }

	public ParticleSystem DustParticleSystem { get; set; }

	public ParticleSystem LThrusterParticleSystem { get; set; }

	public ParticleSystem RThrusterParticleSystem { get; set; }

	public LayerMask TargetMask { get; set; }

	public bool IsJacking
	{
		get
		{
			return _isJack;
		}
		set
		{
			_isJack = value;
		}
	}

	public bool ReleaseJack
	{
		get
		{
			return _releaseJack;
		}
		set
		{
			_releaseJack = value;
		}
	}

	public bool IsStun
	{
		get
		{
			return _stunStack > 0;
		}
	}

	public bool PreBelow
	{
		get
		{
			return _preBelow;
		}
		set
		{
			_preBelow = value;
		}
	}

	public string UserID { get; set; }

	public string sPlayerID
	{
		get
		{
			return sNetSerialID;
		}
		set
		{
			sNetSerialID = value;
		}
	}

	public bool bLockInputCtrl
	{
		get
		{
			if (nLockInputCtrl == 0)
			{
				return MonoBehaviourSingleton<StageSyncManager>.Instance.bPauseAllPlayerInput;
			}
			return true;
		}
	}

	public bool EventLockInputingNet
	{
		get
		{
			return (nLockInputCtrl & 1) != 0;
		}
		set
		{
			if (value)
			{
				nLockInputCtrl |= 1;
			}
			else
			{
				nLockInputCtrl &= -2;
			}
		}
	}

	public bool EventLockInputing
	{
		get
		{
			return (nLockInputCtrl & 2) != 0;
		}
		set
		{
			if (value)
			{
				nLockInputCtrl |= 2;
			}
			else
			{
				nLockInputCtrl &= -3;
			}
		}
	}

	public bool DeadAreaLockInputing
	{
		get
		{
			return (nLockInputCtrl & 4) != 0;
		}
		set
		{
			if (value)
			{
				nLockInputCtrl |= 4;
			}
			else
			{
				nLockInputCtrl &= -5;
			}
		}
	}

	public override bool IsInvincible
	{
		get
		{
			if (!StageUpdate.gbRegisterPvpPlayer && _invincibleTimer.IsStarted())
			{
				return _invincibleTimer.GetMillisecond() < OrangeConst.PVE_IFRAME_DURATION;
			}
			return false;
		}
	}

	public bool IsInGround
	{
		get
		{
			if (!Controller.Collisions.below && !Controller.Collisions.JSB_below)
			{
				return Controller.BelowInBypassRange;
			}
			return true;
		}
	}

	public bool IsCrouching
	{
		get
		{
			if (IsInGround)
			{
				return ManagedSingleton<InputStorage>.Instance.IsHeld(UserID, ButtonId.DOWN);
			}
			return false;
		}
	}

	public bool IsDashing
	{
		get
		{
			if (CurMainStatus == MainStatus.DASH || CurMainStatus == MainStatus.AIRDASH)
			{
				return CurSubStatus == SubStatus.TELEPORT_POSE;
			}
			return false;
		}
	}

	public ChargeShootObj ChargeObject
	{
		get
		{
			return _chargeShootObj;
		}
	}

	public string VoiceID
	{
		get
		{
			return _VoiceID;
		}
		protected internal set
		{
			_VoiceID = value;
		}
	}

	public string CharaSEID
	{
		get
		{
			return _CharaSEID;
		}
		protected internal set
		{
			_CharaSEID = value;
		}
	}

	public string SkillSEID
	{
		get
		{
			return _SkillSEID;
		}
		protected internal set
		{
			_SkillSEID = value;
		}
	}

	public OrangeCriSource SoundSource
	{
		get
		{
			if (_ss == null)
			{
				_ss = base.gameObject.GetComponent<OrangeCriSource>();
				if (_ss == null)
				{
					_ss = base.gameObject.AddComponent<OrangeCriSource>();
					_ss.Initial(OrangeSSType.PVP1);
				}
			}
			return _ss;
		}
	}

	public string CharacterTypeName { get; private set; }

	public CharacterParam TeleportParam { get; private set; }

	public CharacterParam ChargeShotParam { get; private set; }

	public CharacterParam CallPetParam { get; private set; }

	public abstract void SetLocalPlayer(bool isLocal);

	public void ClearSlashAction()
	{
		_listSlashCache.Clear();
	}

	public int CalculateMoveSpeed()
	{
		return Mathf.RoundToInt(_moveSpeedMultiplier * (float)(Dashing ? DashSpeedEx : WalkSpeed));
	}

	public int CalculateFlySpeed()
	{
		return Mathf.RoundToInt(_moveSpeedMultiplier * (float)HoveringSpeed);
	}

	public bool CheckAvalibaleForRideArmor()
	{
		if (!CheckActStatusEvt(3, -1))
		{
			return CheckActStatusEvt(0, -1);
		}
		return true;
	}

	public bool CanUseDash()
	{
		if (_dashEnable)
		{
			return _dashChance > 0;
		}
		return false;
	}

	public void UseDashChance()
	{
		if (CanUseDash())
		{
			_dashChance--;
		}
	}

	public bool CheckDashLock()
	{
		return CurrentActiveSkill != -1;
	}

	public bool IsAnimateIDChanged()
	{
		return _animateID != _animateIDPrev;
	}

	public void ForceSetAnimateId(HumanBase.AnimateId id)
	{
		_animateID = (_animateIDPrev = id);
	}

	public void SetAnimateId(HumanBase.AnimateId id)
	{
		if (_animateID != id)
		{
			_lastSetAnimateFrame = GameLogicUpdateManager.GameFrame;
			_animateID = id;
		}
	}

	private void AnimationEndCharacterDepend(MainStatus mainStatus, SubStatus subStatus)
	{
	}

	private void SetStatusCharacterDepend(MainStatus mainStatus, SubStatus subStatus)
	{
	}

	public bool CheckSkillEndByShootTimer()
	{
		if ((_shootTimer.IsStarted() && _shootTimer.GetTicks() > BusterDelay) || IsShoot == 0)
		{
			SkillEnd = true;
			return true;
		}
		return false;
	}

	public void StartShootTimer()
	{
		_shootTimer.TimerStart();
	}

	public void StopShootTimer()
	{
		_shootTimer.TimerStop();
	}

	public void ResetVelocity()
	{
		_velocity = VInt3.zero;
	}

	public WeaponStruct GetCurrentWeaponObj()
	{
		return PlayerWeapons[WeaponCurrent];
	}

	public bool CheckCurrentWeaponIndex()
	{
		if (PlayerWeapons != null && WeaponCurrent >= 0 && WeaponCurrent < PlayerWeapons.Length)
		{
			return true;
		}
		return false;
	}

	public WeaponStruct GetCurrentSkillObj()
	{
		return PlayerSkills[CurrentActiveSkill];
	}

	public void CreateCharacterControl<T>() where T : CharacterControlBase
	{
		base.gameObject.AddComponent<T>().LinkEntityReference(this);
	}

	public void CreateCharacterControl(Type type)
	{
		CharacterControlBase obj = base.gameObject.AddComponent(type) as CharacterControlBase;
		UpdateAudioRelayParamByType(type);
		obj.LinkEntityReference(this);
	}

	protected void OnEnable()
	{
		MonoBehaviourSingleton<UpdateManager>.Instance.AddUpdate(this);
		MonoBehaviourSingleton<UpdateManager>.Instance.AddLateUpdate(this);
		MonoBehaviourSingleton<GameLogicUpdateManager>.Instance.AddUpdate(this);
	}

	protected void OnDisable()
	{
		StopAllLoopSE();
		MonoBehaviourSingleton<UpdateManager>.Instance.RemoveUpdate(this);
		MonoBehaviourSingleton<UpdateManager>.Instance.RemoveLateUpdate(this);
		MonoBehaviourSingleton<GameLogicUpdateManager>.Instance.RemoveUpdate(this);
	}

	protected virtual void Awake()
	{
		_pvpCCsafety = OrangeTimerManager.GetTimer();
		_weaponSwitchTimer = OrangeTimerManager.GetTimer();
		_analogReleaseTimer = OrangeTimerManager.GetTimer();
		_analogPressTimer = OrangeTimerManager.GetTimer();
		_dashTimer = OrangeTimerManager.GetTimer();
		_wallKickTimer = OrangeTimerManager.GetTimer();
		_doubleTapTimer = OrangeTimerManager.GetTimer();
		_shootTimer = OrangeTimerManager.GetTimer();
		_rangeWpVoiceTimer = OrangeTimerManager.GetTimer();
		_plHurtVoiceTimer = OrangeTimerManager.GetTimer();
		_invincibleTimer = OrangeTimerManager.GetTimer();
		_hoveringTimer = OrangeTimerManager.GetTimer();
		GuardTransform = new List<int>();
		base.gameObject.AddOrGetComponent<StageObjParam>().tLinkSOB = this;
		Controller = GetComponent<Controller2D>();
		if ((bool)Controller)
		{
			Controller.Collider2D.enabled = false;
		}
	}

	protected virtual void Start()
	{
		Controller.Collider2D.enabled = true;
		_lineRenderer = GetComponent<LineRenderer>();
		_animatorBase = GetComponent<AnimatorBase>();
		base.gameObject.AddOrGetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
		Transform transform = OrangeBattleUtility.FindChildRecursive(base.transform, "AutoAimSystem");
		_playerAutoAimSystem = transform.gameObject.AddComponent<PlayerAutoAimSystem>();
		Transform[] array = OrangeBattleUtility.FindAllChildRecursive(base.transform, "L_HandMesh");
		if (array.Length == 0)
		{
			array = OrangeBattleUtility.FindAllChildRecursive(base.transform, "HandMesh_L");
		}
		_handMesh = new SkinnedMeshRenderer[array.Length];
		for (int i = 0; i < array.Length; i++)
		{
			_handMesh[i] = array[i].GetComponent<SkinnedMeshRenderer>();
		}
		CharacterMaterials = GetComponentInChildren<CharacterMaterial>();
		BulletCollider = GetComponentInChildren<CollideBullet>();
		if (IsLocalPlayer)
		{
			CharacterMaterials.UpdateMask(0, 4f);
			TargetMask = ManagedSingleton<OrangeLayerManager>.Instance.PlayerUseMask;
		}
		_playerAutoAimSystem.targetMask = TargetMask;
		_playerAutoAimSystem.Init(IsLocalPlayer, PlayerSetting.AutoAim != 0);
		OrangeBattleUtility.UpdatePlayerParameters();
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("obj_player_die", 5);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("DistortionFx", 3);
		Initialize();
		InitializeAbility();
		InitializeVoice();
		SetNormalHitBox();
		Singleton<GenericEventManager>.Instance.AttachEvent<GameObject, StageCtrlInsTruction>(EventManager.ID.STAGE_OBJ_CTRL, ObjCtrl);
		Singleton<GenericEventManager>.Instance.AttachEvent<string, bool, float, float, bool?>(EventManager.ID.STAGE_CONTINUE_PLATER, ContinueCall);
		if (tRefPassiveskill == null)
		{
			tRefPassiveskill = new RefPassiveskill();
		}
		selfBuffManager.Init(this);
		SKILL_TABLE skillTable;
		if (tRefPassiveskill.HaveFlyMode() && tRefPassiveskill.GetFlyParam(out skillTable))
		{
			_hoveringEnable = true;
			MaxHoveringTicks = skillTable.f_EFFECT_X * 60f;
			HoveringSpeed = Mathf.RoundToInt((float)WalkSpeed * skillTable.f_EFFECT_Y);
			HoveringHighLimit = skillTable.f_EFFECT_Z;
		}
		Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_PLAYER_SPWAN_ED, this);
		MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.RemoveGeneratingPlayer(sPlayerID);
		vLastStandPt = base.transform.position;
		if ((int)Hp <= 0)
		{
			LockInput = false;
			LockWeapon = false;
			LockSkill = false;
			ReverseRightAndLeft = false;
			SetBanAutoAim(false);
			PlayerDead();
			bIsDeadEnd = true;
		}
		else
		{
			Controller.Collider2D.enabled = false;
		}
	}

	public void ObjCtrl(GameObject tObj, StageCtrlInsTruction tSCE)
	{
		if (tObj == null || tObj.GetInstanceID() != base.gameObject.GetInstanceID())
		{
			return;
		}
		if (tSCE.tStageCtrl == 10 || tSCE.tStageCtrl == 11 || tSCE.tStageCtrl == 12)
		{
			tSCE.nParam1 = 0f;
		}
		for (int i = 0; i < _stageCtrlInstructions.Count; i++)
		{
			if (_stageCtrlInstructions[i].tStageCtrl == tSCE.tStageCtrl)
			{
				_stageCtrlInstructions[i].fTime = tSCE.fTime;
				_stageCtrlInstructions[i].fWait = tSCE.fWait;
				_stageCtrlInstructions[i].nParam1 = tSCE.nParam1;
				_stageCtrlInstructions[i].nParam2 = tSCE.nParam2;
				_stageCtrlInstructions[i].sMsg = tSCE.sMsg;
				StageCtrlInsTruction stageCtrlInsTruction = _stageCtrlInstructions[i];
				stageCtrlInsTruction.RemoveCB = (Callback)Delegate.Combine(stageCtrlInsTruction.RemoveCB, tSCE.RemoveCB);
				return;
			}
		}
		_stageCtrlInstructions.Add(tSCE);
	}

	public virtual void DerivedContinueCall()
	{
		ManagedSingleton<InputStorage>.Instance.ResetPlayerInput(sPlayerID);
	}

	private void ContinueCall(string playerId, bool setPos, float posX, float posY, bool? lookBack)
	{
		if ((int)Hp > 0 || playerId != sPlayerID)
		{
			return;
		}
		if (setPos)
		{
			base.transform.position = new Vector3(posX, posY, 0f);
			Controller.LogicPosition = new VInt3(base.transform.position);
		}
		DerivedContinueCall();
		Hp = MaxHp;
		DmgHp = 0;
		HealHp = 0;
		NullHurtAction();
		selfBuffManager.Init(this);
		selfBuffManager.AddBuff(OrangeConst.REBOOT_BUFFID, 0, 0, 0, sNetSerialID != MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify, "", 3);
		ClearDmgStack();
		bool bNeedChangeNUAFalse = true;
		if (bNeedUpdateAlways)
		{
			bNeedChangeNUAFalse = false;
		}
		_stunStack = 0;
		_nomoveStack = 0;
		if (bIsDeadEnd)
		{
			foreach (Renderer listRender in listRenders)
			{
				listRender.enabled = true;
			}
		}
		else
		{
			bIsDeadEnd = true;
		}
		listRenders.Clear();
		bNeedUpdateAlways = true;
		LockInput = true;
		LockWeapon = false;
		LockSkill = false;
		ReverseRightAndLeft = false;
		SetBanAutoAim(false);
		SetSpeed(0, 0);
		UpdateFlyDirection(FLY_DIR.HORIZONTAL);
		IgnoreGravity = true;
		if (lookBack.HasValue)
		{
			if (lookBack ?? false)
			{
				base._characterDirection = CharacterDirection.LEFT;
			}
			else
			{
				base._characterDirection = CharacterDirection.RIGHT;
			}
		}
		if (MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.IsPvp)
		{
			StartCoroutine(ContinueEffectCoroutine(bNeedChangeNUAFalse, PlayerContinueAppear));
			return;
		}
		if (UsingVehicle)
		{
			SetStatus(MainStatus.RIDE_ARMOR, SubStatus.RIDE_ARMOR);
		}
		else
		{
			SetStatus(MainStatus.FALL, SubStatus.TELEPORT_POSE);
		}
		PlayerContinueAppear(bNeedChangeNUAFalse);
	}

	public bool IsDead()
	{
		if ((int)Hp <= 0)
		{
			return true;
		}
		if (!Controller.Collider2D.enabled && !IsTeleportOut && CurMainStatus != MainStatus.RIDE_ARMOR)
		{
			return true;
		}
		return false;
	}

	public void InitImportantVar()
	{
		IgnoreGravity = false;
		_ignoreGlobalVelocity = false;
		_ignoreVelocityExtra = false;
		bIceSlide = false;
		bIceSlideSameDir = false;
		fIceSlide = 0f;
		listQuickSand.Clear();
		bQuickSand = false;
		SkillEnd = true;
		if (CurrentActiveSkill != -1)
		{
			if (CurrentActiveSkill != 100)
			{
				ClearSkillEvt.CheckTargetToInvoke();
			}
			else
			{
				EnableCurrentWeapon();
			}
			CurrentActiveSkill = -1;
		}
		LockInput = false;
		nLockInputCtrl = 0;
		bLockBulletForNextStatus = false;
		Dashing = false;
		SetNormalHitBox();
		SetSpeed(0, 0);
		UpdateFlyDirection(FLY_DIR.HORIZONTAL);
		DistanceDelta = 0f;
		Controller.LogicPosition = new VInt3(base.transform.localPosition);
		if (BulletCollider != null)
		{
			BulletCollider.BackToPool();
		}
		CharacterControlBase component = GetComponent<CharacterControlBase>();
		if ((bool)component)
		{
			component.ExtraVariableInit();
		}
	}

	private void PlayerContinueAppear(bool bNeedChangeNUAFalse)
	{
		CharacterControlBase component = GetComponent<CharacterControlBase>();
		if ((bool)component)
		{
			component.ControlCharacterContinue();
		}
		_isJack = false;
		CharacterMaterials.Appear(delegate
		{
			Controller.Collider2D.enabled = true;
			base.AllowAutoAim = true;
			LockInput = false;
			LockWeapon = false;
			LockSkill = false;
			ReverseRightAndLeft = false;
			SetBanAutoAim(false);
			if (sPlayerID == MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify)
			{
				IsLocalPlayer = true;
			}
			if (bNeedChangeNUAFalse)
			{
				bNeedUpdateAlways = false;
			}
			PlayBattleSE(BattleSE.CRI_BATTLESE_BT_REVIVE01);
			Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_PLAYER_SPWAN_ED, this);
			EnableCurrentWeapon();
			if (GetCurrentWeaponObj().ChipEfx != null && GetCurrentWeaponObj().chip_switch)
			{
				tRefPassiveskill.bUsePassiveskill = true;
				GetCurrentWeaponObj().ChipEfx.ActiveChipSkill(true);
			}
			InitImportantVar();
		});
	}

	private IEnumerator ContinueEffectCoroutine(bool bNeedChangeNUAFalse, Action<bool> EndCB, bool bCallSCB = false)
	{
		yield return CoroutineDefine._waitForEndOfFrame;
		FxBase teleportParticleSystem = MonoBehaviourSingleton<FxManager>.Instance.PlayReturn<FxBase>(TeleportInFx, base.transform.position, Quaternion.identity, Array.Empty<object>());
		SoundSource.UpdateDistanceCall();
		PlayCharaSeCB(CharaSE.TELEPORTIN);
		while (teleportParticleSystem.pPS.time < 0.3f && !teleportParticleSystem.IsEnd)
		{
			yield return CoroutineDefine._waitForEndOfFrame;
		}
		if (UsingVehicle)
		{
			SetStatus(MainStatus.RIDE_ARMOR, SubStatus.RIDE_ARMOR);
		}
		else
		{
			SetStatus(MainStatus.IDLE, SubStatus.IDLE);
		}
		if (bCallSCB)
		{
			StageTeleportInCharacterDependEvt.CheckTargetToInvoke();
		}
		if (EndCB != null)
		{
			EndCB(bNeedChangeNUAFalse);
		}
	}

	protected void OverrideDelegateEvent()
	{
		CheckDashLockEvt = CheckDashLock;
		CheckActStatusEvt = CheckActStatus;
		EnterRideArmorEvt = EnterRideArmor;
		LeaveRideArmorEvt = LeaveRideArmor;
		CheckAvalibaleForRideArmorEvt = CheckAvalibaleForRideArmor;
		AnimationEndCharacterDependEvt = AnimationEndCharacterDepend;
		SetStatusCharacterDependEvt = SetStatusCharacterDepend;
		GetUpperAnimateKeepFlagEvt = GetUpperAnimateKeepFlag;
		PlWallJumpCheckEvt = PlWallJumpCheck;
		PlWallStopCheckEvt = PlWallStopCheck;
		UpdateAimRangeByWeaponEvt = UpdateAimRangeByWeapon;
		GetCurrentAimRangeEvt = GetCurrentAimRange;
		GuardCalculateEvt = GuardCalculate;
		CheckSkillLockDirectionEvt = CheckSkillLockDirection;
		CheckPetActiveEvt = CheckPetActive;
		GetCurrentWeaponObjEvt = GetCurrentWeaponObj;
		PlayVoiceCB = PlayVoice;
		PlayCharaSeCB = PlayCharaSE;
		PlayTeleportOutEffectEvt = PlayTeleportEffect;
		LockAnimatorEvt = LockCurrentAnimator;
		DeadAreaLockEvt = DeadAreaEventLock;
		CharacterControlBase component = GetComponent<CharacterControlBase>();
		if ((bool)component)
		{
			component.OverrideDelegateEvent();
		}
	}

	public void ConnectStandardCtrlCB()
	{
		PlayerPressDashCB = PlayerPressDash;
		PlayerReleaseDashCB = PlayerReleaseDash;
		PlayerPressLeftCB = PlayerPressLeft;
		PlayerHeldLeftCB = PlayerHeldLeft;
		PlayerReleaseLeftCB = PlayerReleaseLeftRight;
		PlayerPressRightCB = PlayerPressRight;
		PlayerHeldRightCB = PlayerHeldRight;
		PlayerReleaseRightCB = PlayerReleaseLeftRight;
		PlayerHeldUpCB = PlayerHeldUp;
		PlayerReleaseUpCB = PlayerReleaseUp;
		PlayerPressDownCB = PlayerPressDown;
		PlayerHeldDownCB = PlayerHeldDown;
		PlayerReleaseDownCB = PlayerReleaseDown;
		PlayerHeldShootCB = PlayerHeldShoot;
		PlayerReleaseShootCB = PlayerReleaseShoot;
		PlayerPressJumpCB = PlayerPressJump;
		PlayerReleaseJumpCB = PlayerReleaseJump;
		PlayerResetPressJumpCB = PlayerResetPressJump;
		PlayerPressSkillCB = PlayerPressSkill;
		PlayerHeldSkillCB = PlayerHeldSkill;
		PlayerReleaseSkillCB = PlayerReleaseSkill;
		PlayerPressSelectCB = PlayerPressSelect;
		PlayerPressChipCB = PlayerPressChip;
		PlayerPressGigaAttackCB = PlayerPressGigaAttack;
		AddForceFieldCB = AddForceField;
		OverrideDelegateEvent();
	}

	public void RemoveSelfLRCB()
	{
		PlayerPressLeftCB = DoNothingCB;
		PlayerHeldLeftCB = DoNothingCB;
		PlayerReleaseLeftCB = DoNothingCB;
		PlayerPressRightCB = DoNothingCB;
		PlayerHeldRightCB = DoNothingCB;
		PlayerReleaseRightCB = DoNothingCB;
	}

	public void RemoveSelfDashJumpCB()
	{
		PlayerPressJumpCB = DoNothingCB;
		PlayerPressDashCB = DoNothingCB;
	}

	public void RemoveSelfJumpCB()
	{
		PlayerPressJumpCB = DoNothingCB;
	}

	public void RemoveSelfGigaAtackCB()
	{
		PlayerPressGigaAttackCB = DoNothingCB;
	}

	private void DoNothingCB()
	{
	}

	private void DoNothingCB(object param)
	{
	}

	public bool IsStandJumpCB()
	{
		return PlayerPressJumpCB == new Callback(PlayerPressJump);
	}

	public void InitWeaponStruct()
	{
		for (int i = 0; i < 2; i++)
		{
			PlayerWeapons[i].Initialize(WeaponStruct.AbilityType.WEAPON, i, this);
			PlayerSkills[i].Initialize(WeaponStruct.AbilityType.SKILL, i, this);
			PlayerFSkills[i].Initialize(WeaponStruct.AbilityType.FS, i, this);
		}
		WeaponInModel componentInChildren = GetComponentInChildren<WeaponInModel>();
		if ((bool)componentInChildren)
		{
			componentInChildren.Initialize(this);
		}
	}

	public void ReInitSkillStruct(int idx, int skillId, bool forceChangeIcon = false)
	{
		if (idx < 0 || idx >= PlayerSkills.Length)
		{
			return;
		}
		PlayerSkills[idx].Initialize(WeaponStruct.AbilityType.SKILL, skillId, this);
		for (int i = 0; i < PlayerSkills[idx].FastBulletDatas.Length; i++)
		{
			StageResManager.LoadBulletBySkillTable(PlayerSkills[idx].FastBulletDatas[i]);
		}
		if (this is OrangeNetCharacter)
		{
			OrangeNetCharacter orangeNetCharacter = this as OrangeNetCharacter;
			if (idx == 0)
			{
				orangeNetCharacter.ResetSyncDataTimer(ESyncData.SKILL1_CHARGE);
				orangeNetCharacter.ResetSyncDataTimer(ESyncData.SKILL1_LASTUSE);
			}
			else if (idx == 1)
			{
				orangeNetCharacter.ResetSyncDataTimer(ESyncData.SKILL2_CHARGE);
				orangeNetCharacter.ResetSyncDataTimer(ESyncData.SKILL2_LASTUSE);
			}
		}
		if (!(this is OrangeConsoleCharacter))
		{
			return;
		}
		MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(AssetBundleScriptableObject.Instance.GetIconSkill(PlayerSkills[idx].FastBulletDatas[0].s_ICON), PlayerSkills[idx].FastBulletDatas[0].s_ICON, delegate(Sprite obj)
		{
			if (obj != null)
			{
				PlayerSkills[idx].Icon = obj;
				if (forceChangeIcon)
				{
					OrangeConsoleCharacter orangeConsoleCharacter = this as OrangeConsoleCharacter;
					if (orangeConsoleCharacter != null)
					{
						orangeConsoleCharacter.ForceChangeSkillIcon(idx + 1, obj);
					}
				}
			}
		});
	}

	public void CheckBuffGainPassiveSkill(int id)
	{
		if (ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT.ContainsKey(id) && ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT[id].n_USE_TYPE == 102)
		{
			for (int i = 0; i < 2; i++)
			{
				PlayerWeapons[i].ReCaluBulletData(ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT[id].n_TRIGGER_X, this);
				PlayerSkills[i].ReCaluBulletData(ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT[id].n_TRIGGER_X, this);
				PlayerFSkills[i].ReCaluBulletData(ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT[id].n_TRIGGER_X, this);
			}
		}
	}

	public void CheckBuffGainNewPassiveSkill(ref RefPassiveskill.PassiveskillStatus newPassiveSkill)
	{
		if (newPassiveSkill != null)
		{
			if (newPassiveSkill.tSKILL_TABLE.n_USE_TYPE == 101)
			{
				tRefPassiveskill.ReCalcuSkill(ref newPassiveSkill.tSKILL_TABLE);
			}
			StageResManager.LoadBulletBySkillTable(newPassiveSkill.tSKILL_TABLE);
		}
	}

	private void InitSlashTimeByModel()
	{
		_slashTiming = new float[20];
		for (int i = 0; i < 20; i++)
		{
			_slashTiming[i] = 1f;
		}
		string s_ANIMATOR = CharacterData.s_ANIMATOR;
		if (!(s_ANIMATOR == "femalesmallcontroller") && !(s_ANIMATOR == "femalemediumcontroller"))
		{
			_slashTiming[0] = 0.74f;
			_slashTiming[1] = 0.89f;
			_slashTiming[2] = 0.85f;
			_slashTiming[3] = 0.96f;
			_slashTiming[4] = 0.92f;
		}
		else
		{
			_slashTiming[0] = 0.84f;
			_slashTiming[1] = 0.9f;
			_slashTiming[2] = 0.88f;
			_slashTiming[3] = 0.9f;
			_slashTiming[4] = 0.86f;
		}
	}

	protected virtual void Initialize()
	{
		CharacterData = ManagedSingleton<OrangeDataManager>.Instance.CHARACTER_TABLE_DICT[CharacterID];
		_transform = base.transform;
		_wallKickMask = LayerMask.GetMask("BlockPlayer", "NoWallKick");
		ConnectStandardCtrlCB();
		InitWeaponStruct();
		InitializeFSRaySplasher();
		Transform[] target = _transform.GetComponentsInChildren<Transform>(true);
		ModelTransform = OrangeBattleUtility.FindChildRecursive(ref target, "model");
		base.AimTransform = OrangeBattleUtility.FindChildRecursive(ref target, "Bip");
		Activate = true;
		base.AimPoint = Vector3.zero;
		base.AllowAutoAim = true;
		base.AutoAimType = AimTargetType.Player;
		InitSlashTimeByModel();
		UpdateAimRangeByWeaponEvt(GetCurrentWeaponObj());
		SetStatus(MainStatus.TELEPORT_IN, SubStatus.TELEPORT_POSE);
		CurrentActiveSkill = -1;
		IsShoot = 0;
		_isHoldShoot = false;
		FreshBullet = false;
		if (base._characterDirection != CharacterDirection.RIGHT && base._characterDirection != CharacterDirection.LEFT)
		{
			base._characterDirection = CharacterDirection.RIGHT;
		}
		UpdateDirection();
		_lineRenderer.enabled = false;
		_lineRenderer.startWidth = 0.02f;
		_lineRenderer.endWidth = 0.02f;
		_lineRenderer.positionCount = 2;
		_doubleTapTimer.SetMode(TimerMode.MILLISECOND);
		_wallKickTimer.SetMode(TimerMode.FRAME);
		_dashTimer.SetMode(TimerMode.FRAME);
		_shootTimer.SetMode(TimerMode.FRAME);
		_hoveringTimer.SetMode(TimerMode.FRAME);
		_weakVoice = 0;
	}

	protected void InitializeAbility()
	{
		ResetDashChance();
		_nJumpCount = 0;
		ABILITY n_ABILITY = (ABILITY)CharacterData.n_ABILITY;
		_dashEnable = (n_ABILITY & ABILITY.DASH) == ABILITY.DASH;
		_airDashEnable = (n_ABILITY & ABILITY.AIR_DASH) == ABILITY.AIR_DASH;
		_doubleJumpEnable = (n_ABILITY & ABILITY.DOUBLE_JUMP) == ABILITY.DOUBLE_JUMP;
		_hoveringEnable = (n_ABILITY & ABILITY.HOVERING) == ABILITY.HOVERING;
	}

	public void UpdateFunc()
	{
		if (!Activate)
		{
			return;
		}
		CurrentFrame = Animator._animator.GetCurrentAnimatorStateInfo(0).normalizedTime % 1f;
		MainStatus curMainStatus = CurMainStatus;
		if (curMainStatus == MainStatus.WALK)
		{
			switch (CurSubStatus)
			{
			case SubStatus.WIN_POSE:
				if (_prevFrame < 0.3f && CurrentFrame >= 0.3f)
				{
					PlayCharaSeCB(CharaSE.STEP);
				}
				if (_prevFrame < 0.8f && CurrentFrame >= 0.8f)
				{
					PlayCharaSeCB(CharaSE.STEP);
				}
				break;
			case SubStatus.RIDE_ARMOR:
				if (_prevFrame < 0.6f && CurrentFrame >= 0.6f)
				{
					PlayCharaSeCB(CharaSE.STEP);
				}
				if (_prevFrame < 0.9f && CurrentFrame >= 0.9f)
				{
					PlayCharaSeCB(CharaSE.STEP);
				}
				break;
			}
		}
		UpdateSlashCollider();
		vLastMovePt = base.transform.position;
		if (!_usingVehicle && !_isJack)
		{
			base.transform.localPosition = Vector3.MoveTowards(base.transform.localPosition, Controller.LogicPosition.vec3, DistanceDelta);
			if (sNetSerialID == MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify && Mathf.Abs(_velocity.x) > 1500)
			{
				Vector3 position = MonoBehaviourSingleton<OrangeSceneManager>.Instance.MainCamera.transform.position;
				if (position.x - ManagedSingleton<StageHelper>.Instance.fCameraWHalf - (base.transform.localPosition.x - Controller.Collider2D.bounds.size.x) > 0f - CameraControl.fMaxEdgeDis)
				{
					CameraControl.bNeedStickTarget = true;
					EventManager.StageCameraFocus stageCameraFocus = new EventManager.StageCameraFocus();
					stageCameraFocus.nMode = 5;
					Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_CAMERA_FOCUS, stageCameraFocus);
				}
				else if (position.x + ManagedSingleton<StageHelper>.Instance.fCameraWHalf - (base.transform.localPosition.x + Controller.Collider2D.bounds.size.x) < CameraControl.fMaxEdgeDis)
				{
					CameraControl.bNeedStickTarget = true;
					EventManager.StageCameraFocus stageCameraFocus2 = new EventManager.StageCameraFocus();
					stageCameraFocus2.nMode = 5;
					Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_CAMERA_FOCUS, stageCameraFocus2);
				}
			}
		}
		if (UpdateFuncEndEvt != null)
		{
			UpdateFuncEndEvt();
		}
		_prevFrame = CurrentFrame;
	}

	public void ClearSlashCollider()
	{
		if (_listSlashCache.Count != 0)
		{
			SlashDetails slashDetails = _listSlashCache[0];
			if (_prevFrame < slashDetails.Info.timing && CurrentFrame >= slashDetails.Info.timing && slashDetails.MeleeBullet.IsActivate)
			{
				slashDetails.MeleeBullet.ClearList();
			}
			_listSlashCache.Clear();
		}
	}

	private void UpdateSlashCollider()
	{
		if (_listSlashCache.Count == 0)
		{
			return;
		}
		SlashDetails slashDetails = _listSlashCache[0];
		if (!(_prevFrame < slashDetails.Info.timing) || !(CurrentFrame >= slashDetails.Info.timing))
		{
			return;
		}
		if (!slashDetails.MeleeBullet.IsActivate)
		{
			if (slashDetails.SlashType == SlashType.Skill)
			{
				if (CurrentActiveSkill != -1)
				{
					slashDetails.MeleeBullet.Active(slashDetails.SlashType, TargetMask, slashDetails.TargetWeaponStruct);
				}
			}
			else
			{
				slashDetails.MeleeBullet.Active(slashDetails.SlashType, TargetMask, slashDetails.TargetWeaponStruct);
			}
		}
		else
		{
			slashDetails.MeleeBullet.ClearList();
		}
		_listSlashCache.Clear();
	}

	public void LateUpdateFunc()
	{
		if (!Activate)
		{
			return;
		}
		UpdateAimDirection();
		if (IsShoot != 0 || isPushNewBullet)
		{
			foreach (BulletDetails item in _listBulletWaitforShoot)
			{
				CreateBulletDetail(item);
			}
			_listBulletWaitforShoot.Clear();
			isPushNewBullet = false;
		}
		CheckDmgStackByRecordNO();
	}

	public virtual void LogicUpdate()
	{
		if (!Activate)
		{
			return;
		}
		nBulletRecordID = 1;
		if (!IsLocalPlayer)
		{
			if (bLockInputCtrl || bNeedUpdateAlways)
			{
				UpdateValue();
			}
			return;
		}
		UpdateCharacterInput();
		if (CurMainStatus != MainStatus.TELEPORT_IN)
		{
			IAimTargetLogicUpdate = PlayerAutoAimSystem.AutoAimTarget;
			LogicNetworkUpdate();
		}
		UpdateValue();
	}

	public virtual void UpdateCharacterInput()
	{
	}

	public virtual void LogicNetworkUpdate()
	{
	}

	private void OCAppear(bool b)
	{
		CharacterMaterials.Appear();
		EnableCurrentWeapon();
		if (!CheckHurtAction())
		{
			base.HurtActions += objInfoBar.HurtCB;
			BattleInfoUI.Instance.AddOrangeCharacter(this);
		}
		IsTeleporting = false;
	}

	public void TeleportIn()
	{
		IsTeleporting = true;
		IsTeleportOut = false;
		Controller.Collider2D.enabled = true;
		lastWeaponStruct = null;
		StartCoroutine(ContinueEffectCoroutine(false, OCAppear, true));
	}

	public virtual void TeleportOut()
	{
		StartCoroutine(CameraTeleportOut());
	}

	private void PlayTeleportEffect()
	{
		MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(TeleportOutFx, base.transform.position, Quaternion.identity, Array.Empty<object>());
	}

	private IEnumerator CameraTeleportOut()
	{
		IsTeleporting = true;
		CriAtomExPlayback _se = ((TeleportOutCharacterSE != null) ? TeleportOutCharacterSE() : SoundSource.PlaySE(_CharaSEID, 9));
		CharacterMaterials.Disappear();
		new Vector3(base.AimTransform.position.x, _transform.position.y, _transform.position.z);
		PlayTeleportOutEffectEvt.CheckTargetToInvoke();
		_teleportOutParticlePhase = 2;
		DisableCurrentWeapon();
		StageTeleportOutCharacterDependEvt.CheckTargetToInvoke();
		NullHurtAction();
		if (UseTeleportFollowCamera && !StageUpdate.IsRewardUI())
		{
			yield return new WaitForSeconds(0.3f);
			MonoBehaviourSingleton<UIManager>.Instance.OpenLoadingUI(null, OrangeSceneManager.LoadingType.WHITE);
			Vector3 position = OrangeBattleUtility.CurrentCharacter.transform.position;
			position.y += OrangeBattleUtility.CurrentCharacter.Controller.Collider2D.bounds.size.y * 6f;
			EventManager.StageCameraFocus stageCameraFocus = new EventManager.StageCameraFocus();
			stageCameraFocus.nMode = 1;
			stageCameraFocus.roominpos = position;
			stageCameraFocus.fRoomInTime = 0.5f;
			stageCameraFocus.fRoomOutTime = -1f;
			stageCameraFocus.fRoomInFov = 9f;
			stageCameraFocus.bDontPlayMotion = true;
			Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_CAMERA_FOCUS, stageCameraFocus);
		}
		yield return CoroutineDefine._waitForEndOfFrame;
		float fSeTimeOut = 3f;
		while (_se.status != CriAtomExPlayback.Status.Removed && fSeTimeOut >= 0f)
		{
			yield return CoroutineDefine._waitForEndOfFrame;
			fSeTimeOut -= Time.deltaTime;
		}
		yield return CoroutineDefine._waitForEndOfFrame;
		IsTeleporting = false;
		IsTeleportOut = true;
		Controller.Collider2D.enabled = false;
		yield return true;
	}

	public virtual void ClosePadAndPlayerUI()
	{
		RemovePlayerObjInfoBar();
	}

	public void RemovePlayerObjInfoBar()
	{
		if (objInfoBar != null)
		{
			objInfoBar.RemoveBar(this);
			objInfoBar.gameObject.SetActive(false);
			StageResManager.RemoveInfoBar(objInfoBar);
			objInfoBar = null;
		}
	}

	public virtual void UpdateValue()
	{
		selfBuffManager.UpdateBuffTime();
		CheckPerSecTriggerPassiveSkill();
		_moveSpeedMultiplier = 1f + selfBuffManager.sBuffStatus.fMoveSpeed * 0.01f;
		for (int i = 0; i < 2; i++)
		{
			if (PlayerWeapons[i].ChargeTimer.GetMillisecond() < 600000)
			{
				PlayerWeapons[i].ChargeTimer += GameLogicUpdateManager.m_fFrameLenMS;
			}
			if (PlayerWeapons[i].LastUseTimer.GetMillisecond() < 600000)
			{
				PlayerWeapons[i].LastUseTimer += GameLogicUpdateManager.m_fFrameLenMS;
			}
			if (PlayerSkills[i].ChargeTimer.GetMillisecond() < 600000)
			{
				PlayerSkills[i].ChargeTimer += GameLogicUpdateManager.m_fFrameLenMS;
			}
			if (PlayerSkills[i].LastUseTimer.GetMillisecond() < 600000)
			{
				PlayerSkills[i].LastUseTimer += GameLogicUpdateManager.m_fFrameLenMS;
			}
			if (PlayerFSkills[i].LastUseTimer.GetMillisecond() < 600000)
			{
				PlayerFSkills[i].LastUseTimer += GameLogicUpdateManager.m_fFrameLenMS;
			}
		}
		Animator.SetVelocity(_velocity.vec3);
		Animator.SetSpeedMultiplier(_moveSpeedMultiplier);
		CurrentFrame = Animator._animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
		AnimationEnd();
		if (_usingVehicle)
		{
			LinkUpdatePrepare();
		}
		long ticks = _wallKickTimer.GetTicks();
		Animator._modelShift.y = Animator._defaultModelshiftY;
		float value;
		if (AnimatorModelShiftYOverride != null && AnimatorModelShiftYOverride.TryGetValue(CurMainStatus, out value))
		{
			Animator._modelShift.y = value;
		}
		switch (CurMainStatus)
		{
		case MainStatus.TELEPORT_IN:
			TeleportInCharacterDependEvt.CheckTargetToInvoke();
			if (CurrentFrame > 0.3f && !_teleportParticleFlag)
			{
				CharacterMaterials.RecoverOriginalMaskSetting();
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("FX_TELEPORT_IN2", base.transform.position, Quaternion.identity, Array.Empty<object>());
				_teleportParticleFlag = true;
			}
			if (!teleportInVoicePlayed && IsLocalPlayer && !MonoBehaviourSingleton<UIManager>.Instance.IsLoading && Controller.Collisions.below)
			{
				if (PlayTeleportInVoice)
				{
					PlayVoiceCB(Voice.START1);
				}
				PlayCharaSeCB(CharaSE.CHAKUCHI);
				teleportInVoicePlayed = true;
				TeleportInExtraEffectEvt.CheckTargetToInvoke();
				if (TeleportParam != null)
				{
					PlaySE(SkillSEID, TeleportParam.CueName);
				}
			}
			break;
		case MainStatus.TELEPORT_OUT:
			TeleportOutCharacterDependEvt.CheckTargetToInvoke();
			switch (CurSubStatus)
			{
			case SubStatus.TELEPORT_POSE:
				if (_freshWinPose != 0)
				{
					break;
				}
				switch (_teleportOutParticlePhase)
				{
				case 0:
					_teleportOutParticlePhase = 1;
					break;
				case 1:
					if (CurrentFrame >= 1f)
					{
						TeleportOut();
					}
					break;
				case 2:
					if (!IsTeleporting)
					{
						_teleportOutParticlePhase = 3;
					}
					break;
				case 3:
					if (CurrentFrame >= 1f)
					{
						_teleportOutParticlePhase = 4;
					}
					break;
				}
				break;
			}
			break;
		case MainStatus.WALK:
			if (IsShoot == 0)
			{
				SubStatus curSubStatus = CurSubStatus;
				if (curSubStatus == SubStatus.RIDE_ARMOR)
				{
					SetStatus(MainStatus.WALK, SubStatus.WIN_POSE);
				}
			}
			break;
		case MainStatus.WALLKICK:
			switch (CurSubStatus)
			{
			case SubStatus.TELEPORT_POSE:
				if (ticks >= 2)
				{
					if (ManagedSingleton<InputStorage>.Instance.IsHeld(UserID, ButtonId.DASH) && _dashChance > 0)
					{
						Dashing = true;
						_dashChance--;
						SetHorizontalSpeed((int)base._characterDirection * -DashSpeedEx);
						PlayCharaSeCB(CharaSE.DASH);
					}
					else
					{
						Dashing = false;
						SetHorizontalSpeed((int)base._characterDirection * -WalkSpeed);
					}
					SetVerticalSpeed(JumpSpeedEx);
					SetStatus(MainStatus.WALLKICK, SubStatus.WIN_POSE);
				}
				else
				{
					SetVerticalSpeed(0);
				}
				break;
			case SubStatus.WIN_POSE:
				if (ticks >= 7)
				{
					_wallKickTimer.TimerStop();
					SetHorizontalSpeed(0);
					SetStatus(MainStatus.JUMP, SubStatus.WIN_POSE);
				}
				break;
			}
			break;
		case MainStatus.DASH:
			if ((float)_dashTimer.GetTicks() > MaxDashDistance)
			{
				PlayerStopDashing();
			}
			break;
		case MainStatus.AIRDASH:
			if ((float)_dashTimer.GetTicks() > MaxAirDashDistance)
			{
				PlayerStopDashing();
			}
			break;
		case MainStatus.JUMP:
		{
			SubStatus curSubStatus = CurSubStatus;
			if (curSubStatus == SubStatus.IDLE && (float)_hoveringTimer.GetTicks() > MaxHoveringTicks)
			{
				PlayerReleaseJumpCB();
			}
			break;
		}
		}
		MainStatus curMainStatus = CurMainStatus;
		if (curMainStatus == MainStatus.IDLE || (uint)(curMainStatus - 3) <= 1u)
		{
			if (bIceSlide)
			{
				fIceSlide *= fIceSlideParam;
				_velocityExtra = new VInt3(new Vector3(fIceSlide, 0f, 0f));
				if (_velocityExtra.x == 0)
				{
					bIceSlide = false;
				}
			}
		}
		else
		{
			fIceSlide = 0f;
			bIceSlide = false;
		}
		HandleInput();
		if (!_usingVehicle && !IsJacking)
		{
			UpdateGravity();
			if ((int)Hp > 0 && Controller.Collider2D.enabled)
			{
				VInt3 vInt = (_ignoreGlobalVelocity ? VInt3.zero : OrangeBattleUtility.GlobalVelocityExtra);
				VInt3 vInt2 = (_ignoreVelocityExtra ? VInt3.zero : _velocityExtra);
				Controller.Move((_velocity + vInt2 + vInt) * GameLogicUpdateManager.m_fFrameLen + _velocityShift);
			}
			else
			{
				Controller.Move((_velocity + _velocityExtra) * GameLogicUpdateManager.m_fFrameLen + _velocityShift);
			}
			DistanceDelta = Vector3.Distance(base.transform.localPosition, Controller.LogicPosition.vec3) * (Time.deltaTime / GameLogicUpdateManager.m_fFrameLen);
			_velocityExtra = VInt3.zero;
			_velocityShift = VInt3.zero;
			_velocityForceField = VInt3.zero;
			CheckLockDirection();
			UpdateDirection();
			for (int j = 0; j < 2; j++)
			{
				PlayerSkills[j].UpdateMagazine();
				PlayerWeapons[j].UpdateMagazine();
			}
			CheckSkillEvt.CheckTargetToInvoke();
		}
		else if (LinkUpdateCall != null)
		{
			LinkUpdateCall();
		}
		LogicLateUpdate();
	}

	protected internal bool CheckCanCombo(string[] command)
	{
		string text = command[0];
		int num = int.Parse(command[1]);
		int num2 = int.Parse(command[2]);
		if (!(text == "1"))
		{
			if (text == "2" && (int)Hp >= num && (int)Hp <= num2)
			{
				return true;
			}
		}
		else
		{
			int buffStack = selfBuffManager.GetBuffStack(num);
			if (buffStack == -1)
			{
				return false;
			}
			if (buffStack >= num2)
			{
				selfBuffManager.SetBuffTime(num, 0f);
				return true;
			}
		}
		return false;
	}

	protected void CreateBullet(WeaponStruct weaponStruct, sbyte lv, int shootPoint = 0, Vector3? ShotDir = null, bool isWithLink = true)
	{
		FreshBullet = true;
		IsShoot = (sbyte)(lv + 1);
		_shootTimer.TimerStart();
		if (weaponStruct.GatlingSpinner != null)
		{
			weaponStruct.GatlingSpinner.Activate = true;
		}
		if (lv != 0 && lv != 1)
		{
			MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("DistortionFx", weaponStruct.ShootTransform[shootPoint].position, Quaternion.identity, Array.Empty<object>());
		}
		PushBulletDetail(weaponStruct.FastBulletDatas[lv], weaponStruct.weaponStatus, (weaponStruct.ShootTransform.Length < shootPoint + 1) ? weaponStruct.ShootTransform[0] : weaponStruct.ShootTransform[shootPoint], weaponStruct.SkillLV, ShotDir);
		if (!isWithLink || !ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT.ContainsKey(weaponStruct.FastBulletDatas[lv].n_LINK_SKILL))
		{
			return;
		}
		for (int i = 0; i < weaponStruct.FastBulletDatas.Length; i++)
		{
			if (weaponStruct.FastBulletDatas[lv].n_LINK_SKILL == weaponStruct.FastBulletDatas[i].n_ID)
			{
				CreateBullet(weaponStruct, (sbyte)i, shootPoint, ShotDir);
			}
		}
	}

    [Obsolete]
    protected internal void PushBulletDetail(SKILL_TABLE bulletData, WeaponStatus refWS, Transform ShootTransform, int bulletlv = 0, Vector3? ShotDir = null, bool useExtraCollider = true, int? nSetDir = null, CallbackObj hitCB = null)
	{
		if (!bLockBulletForNextStatus)
		{
			BulletDetails bulletDetails = new BulletDetails();
			bulletDetails.bulletData = bulletData;
			bulletDetails.refWS = refWS;
			bulletDetails.ShootTransform = ShootTransform;
			bulletDetails.ShotDir = ShotDir;
			bulletDetails.nBulletRecordID = nBulletRecordID++;
			bulletDetails.nBulletLV = bulletlv;
			bulletDetails.nRecordID = GetNowRecordNO();
			bulletDetails.useExtraCollider = useExtraCollider;
			bulletDetails.tAutoAimTarget = IAimTargetLogicUpdate;
			if (bulletData.n_TRACKING > 0 && bulletDetails.tAutoAimTarget == null)
			{
				bulletDetails.tAutoAimTarget = PlayerAutoAimSystem.GetClosestTarget();
			}
			bulletDetails.nDirect = nSetDir ?? ((int)base._characterDirection);
			bulletDetails.ManualShoot = !UseAutoAim;
			bulletDetails.hitCallBack = hitCB;
			if (bulletData.n_SHOTLINE == 13)
			{
				SetShotLine13Bullet(ref bulletDetails);
			}
			_listBulletWaitforShoot.Add(bulletDetails);
			isPushNewBullet = true;
		}
	}

    [Obsolete]
    protected internal void PushBulletDetail(SKILL_TABLE bulletData, WeaponStatus refWS, Vector3 ShootPosition, int bulletlv = 0, Vector3? ShotDir = null, bool useExtraCollider = true, int? nSetDir = null, Callback hurt = null, CallbackObj hitCB = null)
	{
		if (!bLockBulletForNextStatus)
		{
			BulletDetails bulletDetails = new BulletDetails();
			bulletDetails.bulletData = bulletData;
			bulletDetails.refWS = refWS;
			bulletDetails.ShootTransform = null;
			bulletDetails.ShootPosition = ShootPosition;
			bulletDetails.ShotDir = ShotDir;
			bulletDetails.nBulletRecordID = nBulletRecordID++;
			bulletDetails.nBulletLV = bulletlv;
			bulletDetails.nRecordID = GetNowRecordNO();
			bulletDetails.useExtraCollider = useExtraCollider;
			bulletDetails.tAutoAimTarget = IAimTargetLogicUpdate;
			if (bulletData.n_TRACKING > 0 && bulletDetails.tAutoAimTarget == null)
			{
				bulletDetails.tAutoAimTarget = PlayerAutoAimSystem.GetClosestTarget();
			}
			bulletDetails.nDirect = nSetDir ?? ((int)base._characterDirection);
			bulletDetails.hitCallBack = hitCB;
			if (bulletData.n_SHOTLINE == 13)
			{
				SetShotLine13Bullet(ref bulletDetails);
			}
			_listBulletWaitforShoot.Add(bulletDetails);
			isPushNewBullet = true;
		}
	}

    [Obsolete]
    protected internal void PushBulletDetail(SKILL_TABLE bulletData, WeaponStatus refWS, Transform ShootTransform, IAimTarget target, int bulletlv = 0, Vector3? ShotDir = null, bool useExtraCollider = true, int? nSetDir = null, CallbackObj hitCB = null)
	{
		if (!bLockBulletForNextStatus)
		{
			BulletDetails bulletDetails = new BulletDetails();
			bulletDetails.bulletData = bulletData;
			bulletDetails.refWS = refWS;
			bulletDetails.ShootTransform = ShootTransform;
			bulletDetails.ShotDir = ShotDir;
			bulletDetails.nBulletRecordID = nBulletRecordID++;
			bulletDetails.nBulletLV = bulletlv;
			bulletDetails.nRecordID = GetNowRecordNO();
			bulletDetails.useExtraCollider = useExtraCollider;
			bulletDetails.tAutoAimTarget = target;
			if (bulletData.n_TRACKING > 0 && bulletDetails.tAutoAimTarget == null)
			{
				bulletDetails.tAutoAimTarget = PlayerAutoAimSystem.GetClosestTarget();
			}
			bulletDetails.nDirect = nSetDir ?? ((int)base._characterDirection);
			bulletDetails.ManualShoot = !UseAutoAim;
			bulletDetails.hitCallBack = hitCB;
			if (bulletData.n_SHOTLINE == 13)
			{
				SetShotLine13Bullet(ref bulletDetails);
			}
			_listBulletWaitforShoot.Add(bulletDetails);
			isPushNewBullet = true;
		}
	}

	protected void SetShotLine13Bullet(ref BulletDetails bulletDetails)
	{
		if (bulletDetails.tAutoAimTarget == null)
		{
			bulletDetails.tAutoAimTarget = PlayerAutoAimSystem.GetClosestTarget();
		}
		Vector3 position = _transform.position;
		CameraControl component = MonoBehaviourSingleton<OrangeSceneManager>.Instance.MainCamera.GetComponent<CameraControl>();
		if (base.direction == 1)
		{
			position.x = component.FocusPosition.x - 10f;
		}
		else
		{
			position.x = component.FocusPosition.x + 10f;
		}
		position.y = component.FocusPosition.y + 10f;
		bulletDetails.ShootPosition = position;
		bulletDetails.ShootTransform = null;
		if (bulletDetails.tAutoAimTarget != null)
		{
			bulletDetails.ShotDir = (bulletDetails.tAutoAimTarget.AimPosition - position).normalized;
			return;
		}
		Vector3 vector = component.FocusPosition;
		bulletDetails.ShotDir = (vector - position).normalized;
	}

	protected void CreateBulletDetail(BulletDetails tBulletDetails)
	{
		if (!IsLocalPlayer || bLockBulletForNextStatus || (((uint)(int)tBulletDetails.refWS.nWeaponCheck & 0xCu) != 0 && tBulletDetails.ShootTransform != null && MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.IsPvp && (tBulletDetails.bulletData.n_FLAG & 1) == 0 && Controller.CheckCenterToShotPos(tBulletDetails.ShootTransform.position)))
		{
			return;
		}
		BulletBase bulletBase = null;
		switch ((BulletType)(short)tBulletDetails.bulletData.n_TYPE)
		{
		case BulletType.Continuous:
			bulletBase = MonoBehaviourSingleton<PoolManager>.Instance.GetPoolObj<ContinuousBullet>(tBulletDetails.bulletData.s_MODEL);
			bulletBase.SoundSource = SoundSource;
			break;
		case BulletType.Spray:
			bulletBase = MonoBehaviourSingleton<PoolManager>.Instance.GetPoolObj<SprayBullet>(tBulletDetails.bulletData.s_MODEL);
			break;
		case BulletType.Collide:
			bulletBase = MonoBehaviourSingleton<PoolManager>.Instance.GetPoolObj<CollideBullet>(tBulletDetails.bulletData.s_MODEL);
			if (bulletBase != null)
			{
				((CollideBullet)bulletBase).bNeedBackPoolModelName = true;
			}
			break;
		case BulletType.LrColliderBulle:
			bulletBase = MonoBehaviourSingleton<PoolManager>.Instance.GetPoolObj<LrColliderBullet>(tBulletDetails.bulletData.s_MODEL);
			break;
		default:
			bulletBase = MonoBehaviourSingleton<PoolManager>.Instance.GetPoolObj<BulletBase>(tBulletDetails.bulletData.s_MODEL);
			break;
		}
		if (!bulletBase)
		{
			return;
		}
		bulletBase.UpdateBulletData(tBulletDetails.bulletData, sPlayerName, tBulletDetails.nRecordID, tBulletDetails.nBulletRecordID, tBulletDetails.nDirect);
		bulletBase.UseExtraCollider = tBulletDetails.useExtraCollider;
		bulletBase.SetBulletAtk(tBulletDetails.refWS, selfBuffManager.sBuffStatus);
		bulletBase.BulletLevel = tBulletDetails.nBulletLV;
		bulletBase.isMuteSE = MuteBullet;
		MuteBullet = false;
		if (tBulletDetails.hitCallBack != null)
		{
			bulletBase.HitCallback = tBulletDetails.hitCallBack;
		}
		if (!tBulletDetails.ManualShoot && !_autoAimCalibrated)
		{
			if ((bool)tBulletDetails.ShootTransform)
			{
				_calibrate = CalibrateAimDirection(tBulletDetails.ShootTransform.position, tBulletDetails.tAutoAimTarget);
			}
			else
			{
				_calibrate = CalibrateAimDirection(tBulletDetails.ShootPosition, tBulletDetails.tAutoAimTarget);
			}
			if (_calibrate.HasValue)
			{
				ShootDirection = _calibrate.Value;
			}
		}
		LayerMask layerMask = TargetMask;
		if (tBulletDetails.bulletData.n_TARGET == 3)
		{
			layerMask = BulletScriptableObject.Instance.BulletLayerMaskPlayer;
		}
		if (tBulletDetails.ShootTransform != null)
		{
			bulletBase.Active(tBulletDetails.ShootTransform, tBulletDetails.ShotDir ?? ShootDirection, layerMask, (tBulletDetails.bulletData.n_TRACKING > 0) ? tBulletDetails.tAutoAimTarget : null);
		}
		else
		{
			bulletBase.Active(tBulletDetails.ShootPosition, tBulletDetails.ShotDir ?? ShootDirection, layerMask, (tBulletDetails.bulletData.n_TRACKING > 0) ? tBulletDetails.tAutoAimTarget : null);
		}
		if (!StageUpdate.gbIsNetGame)
		{
			return;
		}
		WeaponStatus refWS = tBulletDetails.refWS;
		PerBuffManager.BuffStatus sBuffStatus = selfBuffManager.sBuffStatus;
		BulletBase.NetBulletData objFromPool = StageResManager.GetObjFromPool<BulletBase.NetBulletData>();
		objFromPool.sNetSerialID = sNetSerialID;
		objFromPool.nHP = refWS.nHP;
		objFromPool.nATK = refWS.nATK;
		objFromPool.nCRI = refWS.nCRI;
		objFromPool.nHIT = refWS.nHIT;
		objFromPool.nWeaponCheck = refWS.nWeaponCheck;
		objFromPool.nWeaponType = refWS.nWeaponType;
		objFromPool.nCriDmgPercent = refWS.nCriDmgPercent;
		objFromPool.nReduceBlockPercent = refWS.nReduceBlockPercent;
		objFromPool.fAtkDmgPercent = sBuffStatus.fAtkDmgPercent;
		objFromPool.fMissPercent = sBuffStatus.fMissPercent;
		objFromPool.fCriPercent = sBuffStatus.fCriPercent;
		objFromPool.fCriDmgPercent = sBuffStatus.fCriDmgPercent;
		objFromPool.fMissPercent = sBuffStatus.fMissPercent;
		if (tBulletDetails.ShootTransform != null)
		{
			objFromPool.vPos = tBulletDetails.ShootTransform.position;
			if (tBulletDetails.ShootTransform.gameObject == base.gameObject)
			{
				objFromPool.sShotTransPath = "root";
			}
			else
			{
				objFromPool.sShotTransPath = tBulletDetails.ShootTransform.gameObject.name;
				while (tBulletDetails.ShootTransform.parent != base.gameObject.transform)
				{
					tBulletDetails.ShootTransform = tBulletDetails.ShootTransform.parent;
					objFromPool.sShotTransPath = tBulletDetails.ShootTransform.gameObject.name + "/" + objFromPool.sShotTransPath;
				}
			}
		}
		else
		{
			objFromPool.vPos = tBulletDetails.ShootPosition;
			objFromPool.sShotTransPath = "";
		}
		objFromPool.vShotDir = tBulletDetails.ShotDir ?? ShootDirection;
		objFromPool.nTargetMask = layerMask;
		objFromPool.nRecordNO = tBulletDetails.nRecordID;
		objFromPool.nBulletID = tBulletDetails.nBulletRecordID;
		objFromPool.nSkillID = 0;
		objFromPool.tNetSkillTable = "";
		if (ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT.ContainsKey(tBulletDetails.bulletData.n_ID))
		{
			objFromPool.nSkillID = tBulletDetails.bulletData.n_ID;
			SKILL_TABLE sKILL_TABLE = ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT[tBulletDetails.bulletData.n_ID];
			if (!sKILL_TABLE.EqualValue(tBulletDetails.bulletData))
			{
				System.Collections.Generic.Dictionary<int, object> dictionary = tBulletDetails.bulletData.MakeDiffDictionary(sKILL_TABLE);
				NetDiffDataPacker netDiffDataPacker = new NetDiffDataPacker();
				foreach (KeyValuePair<int, object> item in dictionary)
				{
					netDiffDataPacker.Add(new NetDiffData
					{
						key = item.Key,
						value = item.Value
					});
				}
				objFromPool.tNetSkillTable = JsonHelper.Serialize(netDiffDataPacker);
			}
		}
		if (tBulletDetails.bulletData.n_TRACKING > 0 && IAimTargetLogicUpdate as StageObjBase != null)
		{
			objFromPool.sTargerNetID = (IAimTargetLogicUpdate as StageObjBase).sNetSerialID;
			objFromPool.vTargetAimPos = IAimTargetLogicUpdate.AimPosition;
		}
		else if (tBulletDetails.bulletData.n_TRACKING > 0 && tBulletDetails.tAutoAimTarget as StageObjBase != null)
		{
			objFromPool.sTargerNetID = (tBulletDetails.tAutoAimTarget as StageObjBase).sNetSerialID;
			objFromPool.vTargetAimPos = tBulletDetails.tAutoAimTarget.AimPosition;
		}
		else
		{
			objFromPool.sTargerNetID = "";
			objFromPool.vTargetAimPos = Vector3.zero;
		}
		objFromPool.nDirect = tBulletDetails.nDirect;
		StageUpdate.SyncStageObj(4, 2, JsonConvert.SerializeObject(objFromPool, Formatting.None, new ObscuredValueConverter()), true);
		StageResManager.BackObjToPool(objFromPool);
	}

	public virtual void LogicLateUpdate()
	{
		AnimationParams.AnimateUpperID = (short)_animateID;
		AnimationParams.AnimateUpperKeepFlag = GetUpperAnimateKeepFlag(_animateID);
		_animateIDPrev = _animateID;
		FreshBullet = false;
		IsShootPrev = IsShoot;
		_playerAutoAimSystem.SetUpdate(!ManagedSingleton<InputStorage>.Instance.IsHeld(UserID, ButtonId.SHOOT));
		if (SkillEnd || (_shootTimer.IsStarted() && _shootTimer.GetTicks() > BusterDelay))
		{
			if (GetCurrentWeaponObj().GatlingSpinner != null)
			{
				GetCurrentWeaponObj().GatlingSpinner.Activate = false;
			}
			IsShoot = 0;
			UpdateAimRender();
			_shootTimer.TimerStop();
			_isHoldShoot = false;
			if (SkillEnd)
			{
				SkillEnd = false;
				if (CurrentActiveSkill == -1 || CurrentActiveSkill == 100)
				{
					return;
				}
				if ((CurMainStatus != 0 || CurSubStatus != SubStatus.SKILL_IDLE) && (CurMainStatus != MainStatus.FALL || CurSubStatus != SubStatus.IDLE))
				{
					UpdateWeaponMesh(GetCurrentWeaponObj(), GetCurrentSkillObj());
				}
				CurrentActiveSkill = -1;
			}
		}
		AnimationParams.IsShoot = IsShoot;
		OverrideAnimatorParamtersEvt.CheckTargetToInvoke();
		Animator.SetAnimatorParameters(AnimationParams);
		if ((bool)Controller.BelowInBypassRange)
		{
			vLastStandPt = base.transform.position;
		}
	}

	public bool GetUpperAnimateKeepFlag(HumanBase.AnimateId id)
	{
		if (_animateID != _animateIDPrev)
		{
			switch (_animateID)
			{
			case HumanBase.AnimateId.ANI_SLASH1:
				if (_animateIDPrev == HumanBase.AnimateId.ANI_WALKSLASH1)
				{
					return true;
				}
				return false;
			case HumanBase.AnimateId.ANI_WALKSLASH1:
				if (_animateIDPrev == HumanBase.AnimateId.ANI_SLASH1)
				{
					return true;
				}
				return false;
			case HumanBase.AnimateId.ANI_SLASH2:
				if (_animateIDPrev == HumanBase.AnimateId.ANI_WALKSLASH2)
				{
					return true;
				}
				return false;
			case HumanBase.AnimateId.ANI_WALKSLASH2:
				if (_animateIDPrev == HumanBase.AnimateId.ANI_SLASH2)
				{
					return true;
				}
				return false;
			default:
				return false;
			}
		}
		if (IsShoot != IsShootPrev && id == HumanBase.AnimateId.ANI_STAND)
		{
			return false;
		}
		if (IsShoot == 0)
		{
			return true;
		}
		if ((id == HumanBase.AnimateId.ANI_STAND || id == HumanBase.AnimateId.ANI_CROUCH_END) && IsShoot < 3 && FreshBullet)
		{
			return false;
		}
		return true;
	}

	protected bool GetLowerAnimateKeepFlag(HumanBase.AnimateId id)
	{
		if (_animateID != _animateIDPrev)
		{
			HumanBase.AnimateId animateID = _animateID;
			if (animateID == HumanBase.AnimateId.ANI_WALK || animateID - 34 <= HumanBase.AnimateId.ANI_RIDEARMOR)
			{
				animateID = _animateIDPrev;
				if (animateID == HumanBase.AnimateId.ANI_WALK || animateID == HumanBase.AnimateId.ANI_SLASH1 || animateID - 34 <= HumanBase.AnimateId.ANI_RIDEARMOR)
				{
					return true;
				}
				return false;
			}
			return false;
		}
		if (IsShoot != IsShootPrev && id == HumanBase.AnimateId.ANI_STAND)
		{
			return false;
		}
		if (IsShoot == 0)
		{
			return true;
		}
		if ((id == HumanBase.AnimateId.ANI_STAND || id == HumanBase.AnimateId.ANI_CROUCH_END) && IsShoot < 3 && FreshBullet)
		{
			return false;
		}
		return true;
	}

	private float GetSelfPosition()
	{
		if (_usingVehicle)
		{
			return refRideBaseObj.transform.position.x;
		}
		return base.transform.position.x;
	}

	protected void HandleInput()
	{
		if (IsDead())
		{
			return;
		}
		for (int i = 0; i < _stageCtrlInstructions.Count; i++)
		{
			if (_stageCtrlInstructions[i].fTime >= 0f)
			{
				switch ((STAGE_CTRL_EVENT)_stageCtrlInstructions[i].tStageCtrl)
				{
				case STAGE_CTRL_EVENT.CTRL_MOVE_RIGHT:
					PlayerHeldRightCB();
					break;
				case STAGE_CTRL_EVENT.CTRL_MOVE_RIGHTSCARE:
					PlayerHeldRightCB();
					if (!selfBuffManager.IsHasBuffType(PerBuffManager.BUFF_TYPE.DEBUFF_SCARE))
					{
						_stageCtrlInstructions[i].fTime = 0f;
					}
					break;
				case STAGE_CTRL_EVENT.CTRL_MOVE_LEFT:
					PlayerHeldLeftCB();
					break;
				case STAGE_CTRL_EVENT.CTRL_MOVE_LEFTSCARE:
					PlayerHeldLeftCB();
					if (!selfBuffManager.IsHasBuffType(PerBuffManager.BUFF_TYPE.DEBUFF_SCARE))
					{
						_stageCtrlInstructions[i].fTime = 0f;
					}
					break;
				case STAGE_CTRL_EVENT.CTRL_MOVE_DOWN:
					PlayerHeldDownCB();
					break;
				case STAGE_CTRL_EVENT.CTRL_MOVE_ATKBTN:
					PlayerHeldShootCB.CheckTargetToInvoke();
					break;
				case STAGE_CTRL_EVENT.CTRL_MOVE_DASH:
					PlayerPressDashCB(ButtonId.DASH);
					break;
				case STAGE_CTRL_EVENT.CTRL_MOVE_STOPDASH:
					PlayerReleaseDashCB();
					break;
				case STAGE_CTRL_EVENT.CTRL_MOVE_JUMP:
					PlayerPressJumpCB();
					break;
				case STAGE_CTRL_EVENT.CTRL_MOVE_SKILL0:
				case STAGE_CTRL_EVENT.CTRL_MOVE_SKILL1:
				case STAGE_CTRL_EVENT.CTRL_MOVE_SKILL2:
					if (_stageCtrlInstructions[i].nParam1 == 0f)
					{
						PlayerPressSkillCB(_stageCtrlInstructions[i].tStageCtrl - 10);
						_stageCtrlInstructions[i].nParam1 = 1f;
					}
					else
					{
						PlayerHeldSkillCB(_stageCtrlInstructions[i].tStageCtrl - 10);
					}
					break;
				case STAGE_CTRL_EVENT.CTRL_MOVE_RIGHTPOS:
					if (GetSelfPosition() > _stageCtrlInstructions[i].nParam1)
					{
						PlayerReleaseRightCB();
						_stageCtrlInstructions[i].RemoveCB.CheckTargetToInvoke();
						_stageCtrlInstructions.RemoveAt(i);
						i--;
						continue;
					}
					PlayerHeldRightCB();
					break;
				case STAGE_CTRL_EVENT.CTRL_MOVE_LEFTPOS:
					if (GetSelfPosition() < _stageCtrlInstructions[i].nParam1)
					{
						PlayerReleaseLeftCB();
						_stageCtrlInstructions[i].RemoveCB.CheckTargetToInvoke();
						_stageCtrlInstructions.RemoveAt(i);
						i--;
						continue;
					}
					PlayerHeldLeftCB();
					break;
				}
			}
			_stageCtrlInstructions[i].fTime -= GameLogicUpdateManager.m_fFrameLen;
			if (_stageCtrlInstructions[i].fTime <= 0f)
			{
				switch ((STAGE_CTRL_EVENT)_stageCtrlInstructions[i].tStageCtrl)
				{
				case STAGE_CTRL_EVENT.CTRL_MOVE_RIGHT:
				case STAGE_CTRL_EVENT.CTRL_MOVE_LEFT:
				case STAGE_CTRL_EVENT.CTRL_MOVE_RIGHTPOS:
				case STAGE_CTRL_EVENT.CTRL_MOVE_LEFTPOS:
				case STAGE_CTRL_EVENT.CTRL_MOVE_RIGHTSCARE:
				case STAGE_CTRL_EVENT.CTRL_MOVE_LEFTSCARE:
					PlayerReleaseLeftCB();
					PlayerReleaseRightCB();
					break;
				case STAGE_CTRL_EVENT.CTRL_MOVE_DOWN:
					PlayerReleaseDownCB();
					break;
				case STAGE_CTRL_EVENT.CTRL_MOVE_SKILL0:
				case STAGE_CTRL_EVENT.CTRL_MOVE_SKILL1:
				case STAGE_CTRL_EVENT.CTRL_MOVE_SKILL2:
					PlayerReleaseSkillCB(_stageCtrlInstructions[i].tStageCtrl - 10);
					break;
				}
				_stageCtrlInstructions[i].RemoveCB.CheckTargetToInvoke();
				_stageCtrlInstructions.RemoveAt(i);
				i--;
			}
		}
		if (LockInput || bLockInputCtrl)
		{
			if (ManagedSingleton<InputStorage>.Instance.IsReleased(UserID, ButtonId.SKILL0) && PlayerSetting.AutoCharge != 1)
			{
				_chargeShootObj.StopCharge();
			}
			if (ManagedSingleton<InputStorage>.Instance.IsReleased(UserID, ButtonId.SKILL1) && PlayerSetting.AutoCharge != 1)
			{
				_chargeShootObj.StopCharge(1);
			}
			return;
		}
		ButtonStatus buttonStatus = ManagedSingleton<InputStorage>.Instance.GetButtonStatus(UserID, ButtonId.LEFT);
		ButtonStatus buttonStatus2 = ManagedSingleton<InputStorage>.Instance.GetButtonStatus(UserID, ButtonId.RIGHT);
		ButtonStatus buttonStatus3 = ManagedSingleton<InputStorage>.Instance.GetButtonStatus(UserID, ButtonId.UP);
		ButtonStatus buttonStatus4 = ManagedSingleton<InputStorage>.Instance.GetButtonStatus(UserID, ButtonId.DOWN);
		if (((buttonStatus == ButtonStatus.PRESSED || buttonStatus == ButtonStatus.HELD) && (buttonStatus2 == ButtonStatus.PRESSED || buttonStatus2 == ButtonStatus.HELD)) || _nomoveStack > 0)
		{
			PlayerReleaseLeftCB();
			PlayerReleaseRightCB();
		}
		else if (buttonStatus == ButtonStatus.PRESSED)
		{
			PlayerPressLeftCB();
			if (Dashing)
			{
				CheckUseKeyTrigger(ButtonId.DASH);
			}
		}
		else if (buttonStatus2 == ButtonStatus.PRESSED)
		{
			PlayerPressRightCB();
			if (Dashing)
			{
				CheckUseKeyTrigger(ButtonId.DASH);
			}
		}
		else if (buttonStatus == ButtonStatus.HELD)
		{
			CheckUseKeyTrigger(ButtonId.LEFT);
			_isHoldLeft = true;
			PlayerHeldLeftCB();
		}
		else if (buttonStatus2 == ButtonStatus.HELD)
		{
			CheckUseKeyTrigger(ButtonId.RIGHT);
			_isHoldRight = true;
			PlayerHeldRightCB();
		}
		else if (buttonStatus == ButtonStatus.RELEASED)
		{
			PlayerReleaseLeftCB();
		}
		else if (buttonStatus2 == ButtonStatus.RELEASED)
		{
			PlayerReleaseRightCB();
		}
		else if (buttonStatus == ButtonStatus.NONE && _isHoldLeft)
		{
			_isHoldLeft = false;
			PlayerReleaseLeftCB();
		}
		else if (buttonStatus2 == ButtonStatus.NONE && _isHoldRight)
		{
			_isHoldRight = false;
			PlayerReleaseRightCB();
		}
		if (((buttonStatus3 == ButtonStatus.PRESSED || buttonStatus3 == ButtonStatus.HELD) && (buttonStatus4 == ButtonStatus.PRESSED || buttonStatus4 == ButtonStatus.HELD)) || _nomoveStack > 0)
		{
			PlayerReleaseUpCB();
			PlayerReleaseDownCB();
		}
		else
		{
			switch (buttonStatus3)
			{
			case ButtonStatus.HELD:
				PlayerHeldUpCB();
				break;
			case ButtonStatus.RELEASED:
				PlayerReleaseUpCB();
				break;
			}
			switch (buttonStatus4)
			{
			case ButtonStatus.PRESSED:
				PlayerPressDownCB();
				break;
			case ButtonStatus.HELD:
				PlayerHeldDownCB();
				break;
			case ButtonStatus.RELEASED:
				PlayerReleaseDownCB();
				break;
			}
		}
		ButtonStatus buttonStatus5 = ManagedSingleton<InputStorage>.Instance.GetButtonStatus(UserID, ButtonId.DASH);
		if (_nomoveStack > 0)
		{
			PlayerReleaseDashCB();
		}
		else
		{
			switch (buttonStatus5)
			{
			case ButtonStatus.PRESSED:
				PlayerPressDashCB(ButtonId.DASH);
				if (Dashing)
				{
					CheckUseKeyTrigger(ButtonId.DASH);
				}
				break;
			case ButtonStatus.RELEASED:
				PlayerReleaseDashCB();
				break;
			}
		}
		if (!LockWeapon && !_isHovering)
		{
			switch (ManagedSingleton<InputStorage>.Instance.GetButtonStatus(UserID, ButtonId.SHOOT))
			{
			case ButtonStatus.PRESSED:
			case ButtonStatus.HELD:
				CheckUseKeyTrigger(ButtonId.SHOOT);
				PlayerHeldShootCB();
				break;
			case ButtonStatus.RELEASED:
				PlayerReleaseShootCB();
				break;
			}
		}
		ButtonStatus buttonStatus6 = ManagedSingleton<InputStorage>.Instance.GetButtonStatus(UserID, ButtonId.JUMP);
		if (_nomoveStack <= 0)
		{
			switch (buttonStatus6)
			{
			case ButtonStatus.PRESSED:
				_nTempDashChance = _dashChance;
				PlayerPressJumpCB();
				if (Dashing)
				{
					CheckUseKeyTrigger(ButtonId.DASH);
				}
				CheckUseJumpKeyTrigger();
				break;
			case ButtonStatus.RELEASED:
				PlayerReleaseJumpCB();
				break;
			}
		}
		if (!LockSkill && !_isHovering)
		{
			ButtonStatus buttonStatus7 = ManagedSingleton<InputStorage>.Instance.GetButtonStatus(UserID, ButtonId.SKILL0);
			ButtonStatus buttonStatus8 = ManagedSingleton<InputStorage>.Instance.GetButtonStatus(UserID, ButtonId.SKILL1);
			switch (buttonStatus7)
			{
			case ButtonStatus.PRESSED:
				PlayerPressSkillCB(0);
				break;
			case ButtonStatus.HELD:
				PlayerHeldSkillCB(0);
				break;
			case ButtonStatus.RELEASED:
				PlayerReleaseSkillCB(0);
				break;
			}
			switch (buttonStatus8)
			{
			case ButtonStatus.PRESSED:
				PlayerPressSkillCB(1);
				break;
			case ButtonStatus.HELD:
				PlayerHeldSkillCB(1);
				break;
			case ButtonStatus.RELEASED:
				PlayerReleaseSkillCB(1);
				break;
			}
		}
		if (ManagedSingleton<InputStorage>.Instance.IsPressed(UserID, ButtonId.SELECT) && !_isHovering)
		{
			PlayerPressSelectCB();
		}
		if (ManagedSingleton<InputStorage>.Instance.IsPressed(UserID, ButtonId.CHIP_SWITCH))
		{
			PlayerPressChipCB();
		}
		if (ManagedSingleton<InputStorage>.Instance.IsPressed(UserID, ButtonId.FS_SKILL) && !_isHovering)
		{
			PlayerPressGigaAttackCB();
		}
	}

	protected virtual bool IsVirtualButtonReloading(VirtualButtonId id, WeaponStruct currentWeaponStruct)
	{
		return false;
	}

	public Vector3? CalibrateAimDirection(Vector3 tShotPos, IAimTarget target = null)
	{
		if (target == null)
		{
			if (_playerAutoAimSystem == null || _playerAutoAimSystem.AutoAimTarget == null)
			{
				return null;
			}
			target = _playerAutoAimSystem.AutoAimTarget;
		}
		return (target.AimPosition - tShotPos).normalized;
	}

	public virtual void UpdateManualShoot(bool isManual)
	{
	}

	public void UpdateAimDirection()
	{
		int num = -1;
		WeaponStruct weaponStruct = null;
		Vector2 vector = Vector2.zero;
		for (int i = 1; i < 6; i++)
		{
			vector = ManagedSingleton<InputStorage>.Instance.GetAnalogStatus(UserID, (AnalogSticks)i);
			if (!(vector == Vector2.zero))
			{
				num = i;
				switch ((AnalogSticks)i)
				{
				case AnalogSticks.SUB0:
					weaponStruct = GetCurrentWeaponObjEvt();
					break;
				case AnalogSticks.SUB1:
					weaponStruct = PlayerSkills[0];
					break;
				case AnalogSticks.SUB2:
					weaponStruct = PlayerSkills[1];
					break;
				}
				break;
			}
		}
		if (num == -1)
		{
			if (_analogIDprev != -1)
			{
				_analogReleaseTimer.TimerStart();
			}
		}
		else if (num != _analogIDprev)
		{
			_analogPressTimer.TimerStart();
		}
		_analogIDprev = num;
		UseAutoAim = true;
		if (ManagedSingleton<InputStorage>.Instance.GetTouchChainLength(UserID) == 0 || _forceManualShoot)
		{
			if (PlayerSetting.AimManual != 0 && num != -1 && _analogPressTimer.IsStarted() && _analogPressTimer.GetMillisecond() > 300)
			{
				ShootDirection = vector.normalized;
				UseAutoAim = false;
				_forceManualShoot = true;
			}
			else if (num == -1 && _analogReleaseTimer.IsStarted() && _analogReleaseTimer.GetMillisecond() < 500)
			{
				UseAutoAim = false;
			}
			if (!ManagedSingleton<InputStorage>.Instance.IsAnyHeld(UserID))
			{
				_forceManualShoot = false;
			}
		}
		UpdateManualShoot(_forceManualShoot);
		if (UseAutoAim && CurrentActiveSkill < 2)
		{
			_autoAimCalibrated = false;
			if (IAimTargetLogicUpdate != null && IAimTargetLogicUpdate.AimTransform != null)
			{
				Vector3 aimPosition = IAimTargetLogicUpdate.AimPosition;
				Transform transform = ((CurrentActiveSkill == -1) ? GetCurrentWeaponObj().ShootTransform[0] : GetCurrentSkillObj().ShootTransform[0]);
				Transform transform2 = ((CurrentActiveSkill == -1) ? GetCurrentWeaponObj().ShootTransform2[0] : GetCurrentSkillObj().ShootTransform2[0]);
				if ((short)GetCurrentWeaponObj().WeaponData.n_TYPE == 8)
				{
					float num2 = 0f;
					if (PlayerSkills[0].BulletData.n_TYPE == 1 && PlayerSkills[0].BulletData.f_DISTANCE > num2)
					{
						transform = PlayerSkills[0].ShootTransform[0];
						num2 = PlayerSkills[0].BulletData.f_DISTANCE;
					}
					if (PlayerSkills[1].BulletData.n_TYPE == 1 && PlayerSkills[1].BulletData.f_DISTANCE > num2)
					{
						num2 = PlayerSkills[1].BulletData.f_DISTANCE;
						transform = PlayerSkills[1].ShootTransform[0];
					}
				}
				if (transform != null)
				{
					if (IsShoot != 0 && Vector2.Distance(aimPosition, transform.position) > _aimDeadZone)
					{
						ShootDirection = (aimPosition.xy() - transform.position.xy()).normalized;
					}
					else if (transform2 != null)
					{
						ShootDirection = (aimPosition.xy() - transform2.position.xy()).normalized;
					}
				}
				else if (transform2 != null)
				{
					ShootDirection = (aimPosition.xy() - transform2.position.xy()).normalized;
				}
				if ((short)GetCurrentWeaponObj().WeaponData.n_TYPE != 8 && transform != null && Vector2.Distance(aimPosition, transform.position) > GetCurrentAimRangeEvt())
				{
					_autoAimCalibrated = true;
					ShootDirection = Vector2.right * (float)base._characterDirection;
				}
			}
			else
			{
				ShootDirection = Vector2.right * (float)base._characterDirection;
			}
		}
		MainStatus curMainStatus = CurMainStatus;
		if ((uint)(curMainStatus - 4) <= 1u && CurSubStatus == SubStatus.TELEPORT_POSE && Mathf.Abs(Vector2.SignedAngle(Vector2.right * (float)base._characterDirection, ShootDirection.xy())) > 45f)
		{
			_autoAimCalibrated = true;
			ShootDirection = (float)base._characterDirection * Vector3.right;
		}
		UpdateAimRender(weaponStruct);
		Animator.SetAttackLayerActive(ShootDirection);
	}

	protected void UpdateAimDirection2(Vector3 dir)
	{
		WeaponStruct weaponStruct = null;
		Vector2 zero = Vector2.zero;
		for (int i = 1; i < 6; i++)
		{
			if (!(ManagedSingleton<InputStorage>.Instance.GetAnalogStatus(UserID, (AnalogSticks)i) == Vector2.zero))
			{
				switch ((AnalogSticks)i)
				{
				case AnalogSticks.SUB0:
					weaponStruct = GetCurrentWeaponObjEvt();
					break;
				case AnalogSticks.SUB1:
					weaponStruct = PlayerSkills[0];
					break;
				case AnalogSticks.SUB2:
					weaponStruct = PlayerSkills[1];
					break;
				}
				break;
			}
		}
		ShootDirection = dir;
		UpdateAimRender(weaponStruct);
		Animator.SetAttackLayerActive(ShootDirection);
	}

	protected internal void UpdateDirection()
	{
		DustParticleSystem.transform.eulerAngles = new Vector3(-90f, 0f, 0f);
		Animator.UpdateDirection((int)base._characterDirection);
	}

	protected virtual void CheckSkillLockDirection()
	{
		SubStatus curSubStatus = CurSubStatus;
		base._characterDirection = (CharacterDirection)((int)base._characterDirection * -1);
	}

	protected internal void CheckLockDirection()
	{
		int num = Math.Sign(ShootDirection.x);
		if (IsShoot != 0 && base._characterDirection != (CharacterDirection)num && Mathf.Abs(ShootDirection.x) > 0.05f)
		{
			if (PlayerSetting.AutoAim == 0 && ManagedSingleton<InputStorage>.Instance.IsPressed(UserID, ButtonId.SHOOT))
			{
				return;
			}
			switch (CurMainStatus)
			{
			case MainStatus.IDLE:
			case MainStatus.CROUCH:
			case MainStatus.FALL:
				base.direction *= -1;
				break;
			case MainStatus.JUMP:
			{
				SubStatus curSubStatus = CurSubStatus;
				if (curSubStatus != SubStatus.IDLE)
				{
					base.direction *= -1;
				}
				break;
			}
			case MainStatus.WALK:
				switch (CurSubStatus)
				{
				case SubStatus.WIN_POSE:
					base.direction *= -1;
					SetStatus(MainStatus.WALK, SubStatus.RIDE_ARMOR);
					break;
				case SubStatus.TELEPORT_POSE:
					base.direction *= -1;
					SetStatus(MainStatus.WALK, SubStatus.RIDE_ARMOR);
					break;
				case SubStatus.RIDE_ARMOR:
					base.direction *= -1;
					break;
				}
				break;
			case MainStatus.SKILL:
				CheckSkillLockDirectionEvt.CheckTargetToInvoke();
				break;
			}
			return;
		}
		MainStatus curMainStatus = CurMainStatus;
		if (curMainStatus == MainStatus.WALK)
		{
			SubStatus curSubStatus = CurSubStatus;
			if (curSubStatus == SubStatus.RIDE_ARMOR)
			{
				SetStatus(MainStatus.WALK, SubStatus.WIN_POSE);
			}
		}
	}

	protected internal void SetMeleeStatus(WeaponStruct weaponStruct)
	{
		WEAPON_TABLE weaponData = weaponStruct.WeaponData;
		SlashType slashType = GetSlashType(CurMainStatus, CurSubStatus);
		switch (CurMainStatus)
		{
		case MainStatus.SLASH:
			SlashVoice(slashType);
			if ((short)weaponData.n_TYPE == 8)
			{
				ActivateMeleeAttack(weaponStruct, slashType);
			}
			break;
		case MainStatus.CROUCH:
		{
			SubStatus curSubStatus = CurSubStatus;
			if (curSubStatus == SubStatus.RIDE_ARMOR)
			{
				SlashVoice(slashType);
				if ((short)weaponData.n_TYPE == 8)
				{
					ActivateMeleeAttack(weaponStruct, slashType);
				}
			}
			break;
		}
		case MainStatus.DASH:
		case MainStatus.AIRDASH:
		{
			SubStatus curSubStatus = CurSubStatus;
			if (curSubStatus == SubStatus.WIN_POSE || curSubStatus == SubStatus.IDLE)
			{
				SlashVoice(slashType);
				if ((short)weaponData.n_TYPE == 8)
				{
					ActivateMeleeAttack(weaponStruct, slashType);
				}
			}
			else if ((short)weaponData.n_TYPE == 8)
			{
				ActivateMeleeAttack(weaponStruct, slashType);
			}
			break;
		}
		case MainStatus.JUMP:
			if ((short)weaponData.n_TYPE == 8)
			{
				ActivateMeleeAttack(weaponStruct, slashType);
			}
			break;
		case MainStatus.FALL:
			if ((short)weaponData.n_TYPE == 8)
			{
				ActivateMeleeAttack(weaponStruct, slashType);
			}
			break;
		case MainStatus.WALK:
			switch (CurSubStatus)
			{
			case SubStatus.IDLE:
				SlashVoice(slashType);
				if ((short)weaponData.n_TYPE == 8)
				{
					ActivateMeleeAttack(weaponStruct, slashType);
				}
				break;
			case SubStatus.LAND:
				SlashVoice(slashType);
				if ((short)weaponData.n_TYPE == 8)
				{
					ActivateMeleeAttack(weaponStruct, slashType);
				}
				break;
			}
			break;
		default:
			if ((short)weaponData.n_TYPE == 8)
			{
				ActivateMeleeAttack(weaponStruct, slashType);
			}
			break;
		}
	}

	protected internal void SetStatus(MainStatus mainStatus, SubStatus subStatus)
	{
		if (CurMainStatus == MainStatus.TELEPORT_IN && CurSubStatus == SubStatus.TELEPORT_POSE && !Controller.Collider2D.enabled)
		{
			return;
		}
		if (CurMainStatus == MainStatus.GIGA_ATTACK && mainStatus != MainStatus.GIGA_ATTACK && CurrentActiveSkill == 100)
		{
			IgnoreGravity = false;
			CurrentActiveSkill = -1;
			EnableCurrentWeapon();
			if ((bool)Controller.BelowInBypassRange)
			{
				Dashing = false;
			}
		}
		CurMainStatus = mainStatus;
		CurSubStatus = subStatus;
		if (GetSlashType(CurMainStatus, CurSubStatus) == SlashType.None && (bool)GetCurrentWeaponObj().MeleeBullet)
		{
			DeActivateMeleeAttack(GetCurrentWeaponObj());
			_listSlashCache.Clear();
		}
		MainStatus curMainStatus = CurMainStatus;
		if (curMainStatus == MainStatus.HURT)
		{
			SubStatus curSubStatus = CurSubStatus;
			SubStatus subStatus2 = curSubStatus - 4;
			int num2 = 1;
		}
		switch (CurMainStatus)
		{
		case MainStatus.SKILL:
		{
			SubStatus curSubStatus = CurSubStatus;
			if (curSubStatus == SubStatus.SKILL_IDLE)
			{
				SpeedLineParticleSystem.Play();
				DustParticleSystem.Stop();
			}
			else
			{
				SpeedLineParticleSystem.Stop();
				SpeedLineParticleSystem.Clear();
				DustParticleSystem.Stop();
			}
			break;
		}
		case MainStatus.DASH:
		case MainStatus.AIRDASH:
			if (CharacterDashType == DASH_TYPE.X_SERIES || CurMainStatus == MainStatus.AIRDASH)
			{
				SpeedLineParticleSystem.Play();
			}
			DustParticleSystem.Stop();
			break;
		case MainStatus.WALLGRAB:
			SpeedLineParticleSystem.Stop();
			SpeedLineParticleSystem.Clear();
			DustParticleSystem.Play();
			break;
		default:
			SpeedLineParticleSystem.Stop();
			SpeedLineParticleSystem.Clear();
			DustParticleSystem.Stop();
			break;
		}
		if (LThrusterParticleSystem.isPlaying)
		{
			curMainStatus = CurMainStatus;
			if ((uint)(curMainStatus - 4) <= 1u)
			{
				SubStatus curSubStatus = CurSubStatus;
				if (curSubStatus == SubStatus.WIN_POSE || curSubStatus == SubStatus.IDLE)
				{
					LThrusterParticleSystem.Stop();
					RThrusterParticleSystem.Stop();
					LThrusterParticleSystem.Clear();
					RThrusterParticleSystem.Clear();
				}
			}
			else
			{
				LThrusterParticleSystem.Stop();
				RThrusterParticleSystem.Stop();
				LThrusterParticleSystem.Clear();
				RThrusterParticleSystem.Clear();
			}
		}
		switch (CurMainStatus)
		{
		case MainStatus.TELEPORT_IN:
			if (CurSubStatus == SubStatus.TELEPORT_POSE && CharacterData.n_WEAPON_IN != -1)
			{
				EnableSkillWeapon(CharacterData.n_WEAPON_IN);
			}
			break;
		case MainStatus.TELEPORT_OUT:
			switch (CurSubStatus)
			{
			case SubStatus.WIN_POSE:
				_freshWinPose = 1;
				break;
			case SubStatus.TELEPORT_POSE:
				_freshWinPose = 1;
				break;
			}
			break;
		case MainStatus.HURT:
		{
			SubStatus curSubStatus = CurSubStatus;
			if (curSubStatus != SubStatus.DASH_END || (int)Hp > 0)
			{
				break;
			}
			CharacterMaterials.Disappear(delegate
			{
				SetStatus(MainStatus.IDLE, SubStatus.IDLE);
				if (IsLocalPlayer)
				{
					IsLocalPlayer = false;
				}
				bNeedUpdateAlways = false;
				if (!bIsDeadEnd)
				{
					bIsDeadEnd = true;
					listRenders.AddRange(base.transform.GetComponentsInChildren<Renderer>());
					for (int num = listRenders.Count - 1; num >= 0; num--)
					{
						if (!listRenders[num].enabled)
						{
							listRenders.RemoveAt(num);
						}
						else
						{
							listRenders[num].enabled = false;
						}
					}
					Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_PLAYER_DESTROY_ED, this, false);
				}
			});
			break;
		}
		}
		switch (CurMainStatus)
		{
		case MainStatus.TELEPORT_IN:
			if (CurSubStatus == SubStatus.TELEPORT_POSE)
			{
				SetAnimateId(HumanBase.AnimateId.ANI_TELEPORT_IN_POSE);
			}
			break;
		case MainStatus.TELEPORT_OUT:
			switch (CurSubStatus)
			{
			case SubStatus.WIN_POSE:
				_bPlayedWinPose = true;
				SetAnimateId(HumanBase.AnimateId.ANI_WIN_POSE);
				break;
			case SubStatus.TELEPORT_POSE:
				if (_bPlayedWinPose)
				{
					SetAnimateId(HumanBase.AnimateId.ANI_TELEPORT_OUT_POSE);
				}
				else
				{
					SetAnimateId(HumanBase.AnimateId.ANI_LOGOUT2);
				}
				break;
			}
			break;
		case MainStatus.RIDE_ARMOR:
			SetAnimateId(HumanBase.AnimateId.ANI_RIDEARMOR);
			break;
		case MainStatus.IDLE:
			switch (CurSubStatus)
			{
			case SubStatus.IDLE:
				SetAnimateId(HumanBase.AnimateId.ANI_STAND);
				break;
			case SubStatus.LAND:
				SetAnimateId(HumanBase.AnimateId.ANI_LAND);
				break;
			case SubStatus.DASH_END:
				SetAnimateId(HumanBase.AnimateId.ANI_DASH_END);
				break;
			case SubStatus.CROUCH_UP:
				SetAnimateId(HumanBase.AnimateId.ANI_CROUCH_UP);
				break;
			case SubStatus.SLASH1_END:
				SetAnimateId((PlayerSetting.SlashClassic != 0) ? HumanBase.AnimateId.ANI_CLASSIC_SLASH1_END : HumanBase.AnimateId.ANI_SLASH1_END);
				break;
			case SubStatus.SLASH2_END:
				SetAnimateId((PlayerSetting.SlashClassic != 0) ? HumanBase.AnimateId.ANI_CLASSIC_SLASH2_END : HumanBase.AnimateId.ANI_SLASH2_END);
				break;
			case SubStatus.SLASH3_END:
				SetAnimateId((PlayerSetting.SlashClassic != 0) ? HumanBase.AnimateId.ANI_CLASSIC_SLASH3_END : HumanBase.AnimateId.ANI_SLASH3_END);
				break;
			case SubStatus.SLASH4_END:
				SetAnimateId(HumanBase.AnimateId.ANI_SLASH4_END);
				break;
			case SubStatus.SLASH5_END:
				SetAnimateId(HumanBase.AnimateId.ANI_SLASH5_END);
				break;
			}
			break;
		case MainStatus.WALK:
			switch (CurSubStatus)
			{
			case SubStatus.TELEPORT_POSE:
				SetAnimateId(HumanBase.AnimateId.ANI_STEP);
				break;
			case SubStatus.WIN_POSE:
				SetAnimateId(HumanBase.AnimateId.ANI_WALK);
				break;
			case SubStatus.RIDE_ARMOR:
				SetAnimateId(HumanBase.AnimateId.ANI_WALKBACK);
				break;
			case SubStatus.IDLE:
				SetAnimateId(HumanBase.AnimateId.ANI_WALKSLASH1);
				break;
			case SubStatus.LAND:
				SetAnimateId(HumanBase.AnimateId.ANI_WALKSLASH2);
				break;
			case SubStatus.DASH_END:
				SetAnimateId(HumanBase.AnimateId.ANI_WALKSLASH1_END);
				break;
			case SubStatus.CROUCH_UP:
				SetAnimateId(HumanBase.AnimateId.ANI_WALKSLASH2_END);
				break;
			}
			break;
		case MainStatus.DASH:
		case MainStatus.AIRDASH:
			switch (CurSubStatus)
			{
			case SubStatus.TELEPORT_POSE:
				if (CharacterDashType == DASH_TYPE.CLASSIC)
				{
					SetAnimateId((CurMainStatus == MainStatus.DASH) ? HumanBase.AnimateId.ANI_SLIDE : HumanBase.AnimateId.ANI_DASH);
				}
				else
				{
					SetAnimateId(HumanBase.AnimateId.ANI_DASH);
				}
				break;
			case SubStatus.WIN_POSE:
				SetAnimateId(HumanBase.AnimateId.ANI_DASHSLASH1);
				break;
			case SubStatus.RIDE_ARMOR:
				SetAnimateId(HumanBase.AnimateId.ANI_DASHSLASH1_END);
				break;
			case SubStatus.IDLE:
				SetAnimateId(HumanBase.AnimateId.ANI_DASHSLASH2);
				break;
			case SubStatus.LAND:
				SetAnimateId(HumanBase.AnimateId.ANI_DASHSLASH2_END);
				break;
			}
			break;
		case MainStatus.JUMP:
			switch (CurSubStatus)
			{
			case SubStatus.TELEPORT_POSE:
				SetAnimateId(HumanBase.AnimateId.ANI_JUMP);
				break;
			case SubStatus.WIN_POSE:
				SetAnimateId(HumanBase.AnimateId.ANI_WALLKICK_END);
				break;
			case SubStatus.RIDE_ARMOR:
				SetAnimateId(HumanBase.AnimateId.ANI_JUMPSLASH);
				break;
			case SubStatus.IDLE:
				SetAnimateId(HumanBase.AnimateId.ANI_DASH);
				break;
			}
			break;
		case MainStatus.FALL:
			Controller.Collisions.below = false;
			Controller.Collisions.JSB_below = false;
			switch (CurSubStatus)
			{
			case SubStatus.TELEPORT_POSE:
				SetAnimateId(HumanBase.AnimateId.ANI_FALL);
				break;
			case SubStatus.WIN_POSE:
				SetAnimateId(HumanBase.AnimateId.ANI_AIRDASH_END);
				break;
			case SubStatus.RIDE_ARMOR:
				SetAnimateId(HumanBase.AnimateId.ANI_JUMPSLASH);
				break;
			}
			break;
		case MainStatus.WALLGRAB:
			switch (CurSubStatus)
			{
			case SubStatus.TELEPORT_POSE:
				SetAnimateId(HumanBase.AnimateId.ANI_WALLGRAB_BEGIN);
				break;
			case SubStatus.WIN_POSE:
				SetAnimateId(HumanBase.AnimateId.ANI_WALLGRAB);
				break;
			case SubStatus.RIDE_ARMOR:
				SetVerticalSpeed(0);
				SetAnimateId(HumanBase.AnimateId.ANI_WALLGRAB_END);
				if (!isUseKabesuriSE && WallSlideGravity != 0)
				{
					PlaySE("BattleSE", "bt_wall_slide_lp");
					isUseKabesuriSE = true;
				}
				break;
			case SubStatus.IDLE:
				SetAnimateId(HumanBase.AnimateId.ANI_WALLGRAB_SLASH);
				break;
			case SubStatus.LAND:
				SetAnimateId(HumanBase.AnimateId.ANI_WALLGRAB_SLASH_END);
				break;
			}
			break;
		case MainStatus.WALLKICK:
			SetAnimateId(HumanBase.AnimateId.ANI_WALLKICK);
			break;
		case MainStatus.CROUCH:
			switch (CurSubStatus)
			{
			case SubStatus.TELEPORT_POSE:
				SetAnimateId(HumanBase.AnimateId.ANI_CROUCH);
				break;
			case SubStatus.WIN_POSE:
				SetAnimateId(HumanBase.AnimateId.ANI_CROUCH_END);
				break;
			case SubStatus.RIDE_ARMOR:
				SetAnimateId(HumanBase.AnimateId.ANI_CROUCHSLASH1);
				break;
			case SubStatus.IDLE:
				SetAnimateId(HumanBase.AnimateId.ANI_CROUCHSLASH1_END);
				break;
			}
			break;
		case MainStatus.HURT:
			switch (CurSubStatus)
			{
			case SubStatus.LAND:
				SetAnimateId(HumanBase.AnimateId.ANI_HURT_BEGIN);
				break;
			case SubStatus.DASH_END:
				SetAnimateId(HumanBase.AnimateId.ANI_HURT_LOOP);
				break;
			}
			break;
		case MainStatus.SLASH:
			switch (CurSubStatus)
			{
			case SubStatus.TELEPORT_POSE:
				SetSpeed(0, 0);
				SetAnimateId((PlayerSetting.SlashClassic != 0) ? HumanBase.AnimateId.ANI_CLASSIC_SLASH1 : HumanBase.AnimateId.ANI_SLASH1);
				break;
			case SubStatus.WIN_POSE:
				SetAnimateId((PlayerSetting.SlashClassic != 0) ? HumanBase.AnimateId.ANI_CLASSIC_SLASH2 : HumanBase.AnimateId.ANI_SLASH2);
				break;
			case SubStatus.RIDE_ARMOR:
				SetAnimateId((PlayerSetting.SlashClassic != 0) ? HumanBase.AnimateId.ANI_CLASSIC_SLASH3 : HumanBase.AnimateId.ANI_SLASH3);
				break;
			case SubStatus.IDLE:
				SetAnimateId(HumanBase.AnimateId.ANI_SLASH4);
				break;
			case SubStatus.LAND:
				SetAnimateId(HumanBase.AnimateId.ANI_SLASH5);
				break;
			}
			break;
		case MainStatus.SKILL:
			switch (CurSubStatus)
			{
			case SubStatus.TELEPORT_POSE:
				SetAnimateId(HumanBase.AnimateId.ANI_SKILL_START);
				break;
			case SubStatus.WIN_POSE:
				SetAnimateId(HumanBase.AnimateId.ANI_SKILL_START);
				break;
			case SubStatus.RIDE_ARMOR:
				SetAnimateId(HumanBase.AnimateId.ANI_SKILL_START);
				break;
			case SubStatus.IDLE:
				SetAnimateId(HumanBase.AnimateId.ANI_BLEND_SKILL_START);
				break;
			case SubStatus.LAND:
				SetAnimateId(HumanBase.AnimateId.ANI_BTSKILL_START);
				break;
			case SubStatus.DASH_END:
				SetAnimateId((HumanBase.AnimateId)128u);
				break;
			case SubStatus.CROUCH_UP:
				SetAnimateId((HumanBase.AnimateId)(65 + ((!PreBelow) ? 3 : 0)));
				break;
			case SubStatus.SKILL_IDLE:
				SetAnimateId((HumanBase.AnimateId)(66 + ((!PreBelow) ? 3 : 0)));
				break;
			case SubStatus.PALETTE_ATTACK_GROUND:
				SetAnimateId((HumanBase.AnimateId)129u);
				break;
			case SubStatus.PALETTE_ATTACK_AIR:
				SetAnimateId((HumanBase.AnimateId)130u);
				break;
			case SubStatus.PALETTE_ATTACK_END:
				SetAnimateId(HumanBase.AnimateId.ANI_SKILL_START);
				break;
			case SubStatus.PALETTE_ARROW_GROUND:
				SetAnimateId(HumanBase.AnimateId.ANI_BTSKILL_START);
				break;
			case SubStatus.PALETTE_ARROW_AIR:
				SetAnimateId((HumanBase.AnimateId)128u);
				break;
			case SubStatus.VAVA_CANNON_GROUND:
				SetAnimateId(HumanBase.AnimateId.ANI_BTSKILL_START);
				break;
			case SubStatus.VAVA_CANNON_AIR:
				SetAnimateId((HumanBase.AnimateId)128u);
				break;
			case SubStatus.VAVA_KNEE_GROUND:
				SetAnimateId(HumanBase.AnimateId.ANI_SKILL_START);
				break;
			case SubStatus.VAVA_KNEE_AIR:
				SetAnimateId((HumanBase.AnimateId)66u);
				break;
			case SubStatus.SLASH_SKILL0_START:
				SetAnimateId(HumanBase.AnimateId.ANI_BLEND_SKILL_START);
				break;
			}
			break;
		case MainStatus.GIGA_ATTACK:
			switch (CurSubStatus)
			{
			case SubStatus.GIGA_ATTACK_START:
				if (IsInGround)
				{
					if (IsCrouching)
					{
						SetAnimateId(HumanBase.AnimateId.ANI_GIGA_CROUCH_START);
					}
					else
					{
						SetAnimateId(HumanBase.AnimateId.ANI_GIGA_STAND_START);
					}
				}
				else
				{
					SetAnimateId(HumanBase.AnimateId.ANI_GIGA_JUMP_START);
				}
				break;
			case SubStatus.GIGA_ATTACK_END:
				switch (_animateID)
				{
				case HumanBase.AnimateId.ANI_GIGA_CROUCH_START:
					SetAnimateId(HumanBase.AnimateId.ANI_GIGA_CROUCH_END);
					break;
				case HumanBase.AnimateId.ANI_GIGA_JUMP_START:
					SetAnimateId(HumanBase.AnimateId.ANI_GIGA_JUMP_END);
					break;
				default:
					SetAnimateId(HumanBase.AnimateId.ANI_GIGA_STAND_END);
					break;
				}
				break;
			}
			break;
		}
		SoundRelayUpdate();
		if (Animator._animator.runtimeAnimatorController.name == "NewEmptyController")
		{
			SetStatusCharacterDependEvt(CurMainStatus, CurSubStatus);
		}
		UpdateHitBox();
		if (isUseKabesuriSE && CurSubStatus != SubStatus.RIDE_ARMOR)
		{
			PlaySE("BattleSE", "bt_wall_slide_stop");
			isUseKabesuriSE = false;
		}
	}

	public void SetHorizontalSpeed(int speed)
	{
		_velocity.x = speed;
	}

	public void SetVerticalSpeed(int speed)
	{
		_velocity.y = speed;
	}

	public void SetSpeed(int x, int y)
	{
		_velocity = new VInt3(x, y, 0);
	}

	public void SetRideShutdown()
	{
		PlayerHeldLeft();
	}

	protected void SetNormalHitBox()
	{
		_isHalfBox = false;
		Controller.SetColliderBox(ref _normalHitboxOffset, ref _normalHitboxSize);
	}

	protected void SetHalfHitBox()
	{
		_isHalfBox = true;
		Controller.SetColliderBox(ref _halfHitboxOffset, ref _halfHitboxSize);
	}

	protected void UpdateHitBox()
	{
		MainStatus curMainStatus = CurMainStatus;
		if (curMainStatus == MainStatus.CROUCH || (uint)(curMainStatus - 4) <= 1u)
		{
			if (!_isHalfBox)
			{
				SetHalfHitBox();
			}
		}
		else if (_isHalfBox)
		{
			SetNormalHitBox();
		}
	}

	public int HurtPercent(float dmgPercent, WeaponType wpnType = WeaponType.Buster, string owner = "")
	{
		int num = Mathf.RoundToInt((float)(int)MaxHp * dmgPercent * 0.01f);
		HurtPassParam hurtPassParam = new HurtPassParam();
		hurtPassParam.dmg = num;
		hurtPassParam.owner = owner;
		BulletBase.tNetDmgStack.sPlayerID = sPlayerID;
		BulletBase.tNetDmgStack.sShotPlayerID = "";
		BulletBase.tNetDmgStack.nDmg = num;
		BulletBase.tNetDmgStack.nRecordID = GetNowRecordNO();
		BulletBase.tNetDmgStack.nNetID = 0;
		BulletBase.tNetDmgStack.sOwner = "STO";
		BulletBase.tNetDmgStack.nSubPartID = 0;
		BulletBase.tNetDmgStack.nDamageType = 0;
		BulletBase.tNetDmgStack.nWeaponType = 0;
		BulletBase.tNetDmgStack.nHP = Hp;
		BulletBase.tNetDmgStack.nEnergyShield = selfBuffManager.sBuffStatus.nEnergyShield;
		BulletBase.tNetDmgStack.nBreakEnergyShieldBuffID = 0;
		Hurt(hurtPassParam);
		BulletBase.tNetDmgStack.nEndHP = Hp;
		DmgHp = (int)DmgHp + ((int)BulletBase.tNetDmgStack.nHP - (int)Hp);
		BulletBase.tNetDmgStack.nDmgHP = DmgHp;
		BulletBase.tNetDmgStack.nHealHP = HealHp;
		BulletBase.tNetDmgStack.nSkillID = 0;
		StageUpdate.SyncStageObj(0, 0, JsonConvert.SerializeObject(BulletBase.tNetDmgStack, Formatting.None, new ObscuredValueConverter()), true);
		return Hp;
	}

	public void CreateBulletByLastWSTranform(SKILL_TABLE tSKILL_TABLE)
	{
		int? nSetDir = null;
		Vector3 shootPosition = Vector3.zero;
		Transform transform = lastCreateBulletTransform;
		if (tSKILL_TABLE.n_USE_FX_FOLLOW == 3)
		{
			transform = null;
			shootPosition = _transform.position;
			nSetDir = 1;
		}
		if (tSKILL_TABLE.n_TRIGGER == 18)
		{
			WeaponStatus weaponStatus = new WeaponStatus();
			weaponStatus.CopyWeaponStatus(lastCreateBulletWeaponStatus, 3);
			if (transform != null)
			{
				PushBulletDetail(tSKILL_TABLE, weaponStatus, transform, 0, lastCreateBulletShotDir, true, nSetDir);
			}
			else
			{
				PushBulletDetail(tSKILL_TABLE, weaponStatus, shootPosition, 0, lastCreateBulletShotDir, true, nSetDir);
			}
		}
		else if (tSKILL_TABLE.n_TRIGGER == 4)
		{
			WeaponStatus weaponStatus2 = new WeaponStatus();
			weaponStatus2.CopyWeaponStatus(lastCreateBulletWeaponStatus, 3);
			Vector3? shotDir = lastCreateBulletShotDir;
			if (transform == null)
			{
				transform = ModelTransform;
			}
			if (tSKILL_TABLE.n_TRACKING != 0)
			{
				IAimTarget autoAimTarget = PlayerAutoAimSystem.AutoAimTarget;
				if (autoAimTarget != null)
				{
					shotDir = (autoAimTarget.AimPosition - transform.position).normalized;
				}
			}
			PushBulletDetail(tSKILL_TABLE, weaponStatus2, transform, 0, shotDir, true, nSetDir);
		}
		else if (transform != null)
		{
			PushBulletDetail(tSKILL_TABLE, lastCreateBulletWeaponStatus, transform, 0, lastCreateBulletShotDir, true, nSetDir);
		}
		else
		{
			PushBulletDetail(tSKILL_TABLE, lastCreateBulletWeaponStatus, shootPosition, 0, lastCreateBulletShotDir, true, nSetDir);
		}
	}

	public override void BuffChangeCheck()
	{
		for (int i = 0; i < PlayerSkills.Length; i++)
		{
			bool flag = false;
			for (int j = 0; j < PlayerSkills[i].ComboCheckDatas.Length; j++)
			{
				if (!PlayerSkills[i].ComboCheckDatas[j].CheckHasAllBuff(selfBuffManager))
				{
					continue;
				}
				flag = true;
				int reload_index = PlayerSkills[i].Reload_index;
				for (int k = 1; k < PlayerSkills[i].FastBulletDatas.Length; k++)
				{
					if (PlayerSkills[i].FastBulletDatas[k].n_ID == PlayerSkills[i].ComboCheckDatas[j].nComboSkillID)
					{
						PlayerSkills[i].Reload_index = k;
						break;
					}
				}
				if (reload_index != PlayerSkills[i].Reload_index)
				{
					UpdateComboSkill(flag, i, PlayerSkills[i].Reload_index);
				}
			}
			if (PlayerSkills[i].ComboCheckDatas.Length != 0 && !flag && PlayerSkills[i].Reload_index != 0)
			{
				PlayerSkills[i].Reload_index = 0;
				UpdateComboSkill(flag, i, 0);
			}
		}
	}

	protected virtual void UpdateComboSkill(bool hasCombo, int nSkillID, int reloadIdx)
	{
		ChangeComboSkillEventEvt.CheckTargetToInvoke(nSkillID, hasCombo ? reloadIdx : 0);
	}

	protected internal virtual void TriggerComboSkillBuff(int nSkillID)
	{
		for (int i = 0; i < PlayerSkills.Length; i++)
		{
			for (int j = 0; j < PlayerSkills[i].ComboCheckDatas.Length; j++)
			{
				if (PlayerSkills[i].ComboCheckDatas[j].nTriggerSkillID == nSkillID)
				{
					PlayerSkills[i].ComboCheckDatas[j].AddComboBuff(selfBuffManager);
				}
			}
		}
	}

	protected internal virtual void RemoveComboSkillBuff(int nSkillID)
	{
		for (int i = 0; i < PlayerSkills.Length; i++)
		{
			for (int j = 0; j < PlayerSkills[i].ComboCheckDatas.Length; j++)
			{
				if (PlayerSkills[i].ComboCheckDatas[j].nComboSkillID == nSkillID)
				{
					PlayerSkills[i].ComboCheckDatas[j].RemoveComboBuff(selfBuffManager);
				}
			}
		}
	}

	public override bool CheckIsLocalPlayer()
	{
		return IsLocalPlayer;
	}

	public override bool IsCanTriggerEvent()
	{
		if (bIsNpcCpy || (int)Hp <= 0)
		{
			return false;
		}
		if (!base.gameObject.activeSelf || !Controller.Collider2D.enabled)
		{
			return false;
		}
		return true;
	}

	public override string GetSOBName()
	{
		return sPlayerName;
	}

	public override int GetSOBType()
	{
		return 1;
	}

	public override int GetNowRecordNO()
	{
		return ManagedSingleton<InputStorage>.Instance.GetInputRecordNO(sPlayerID);
	}

	public override ObscuredInt GetCurrentWeapon()
	{
		return WeaponCurrent;
	}

	public override ObscuredInt GetCurrentWeaponCheck()
	{
		return GetCurrentWeaponObj().weaponStatus.nWeaponCheck;
	}

	public override ObscuredInt GetDOD(int nCurrentWeapn)
	{
		return ManagedSingleton<StageHelper>.Instance.StatusCorrection(SetPBP.tPlayerStatus.nDOD, StageHelper.STAGE_RULE_STATUS.DODGE) + tRefPassiveskill.GetAddStatus(9, PlayerWeapons[nCurrentWeapn].weaponStatus.nWeaponCheck);
	}

	public override ObscuredInt GetDEF(int nCurrentWeapn)
	{
		int pow = ManagedSingleton<StageHelper>.Instance.StatusCorrection((int)SetPBP.tPlayerStatus.nDEF + (int)SetPBP.chipStatus.nDEF, StageHelper.STAGE_RULE_STATUS.DEF);
		pow = MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.PowerCorrection(pow);
		pow = Mathf.FloorToInt((float)pow * tRefPassiveskill.GetRatioStatus(3, PlayerWeapons[nCurrentWeapn].weaponStatus.nWeaponCheck));
		pow += tRefPassiveskill.GetAddStatus(3, PlayerWeapons[nCurrentWeapn].weaponStatus.nWeaponCheck);
		return pow;
	}

	public override ObscuredInt GetReduceCriPercent(int nCurrentWeapn)
	{
		int pow = (int)SetPBP.tPlayerStatus.nReduceCriPercent + (int)SetPBP.mainWStatus.nReduceCriPercent + (int)SetPBP.subWStatus.nReduceCriPercent + (int)SetPBP.chipStatus.nReduceCriPercent;
		pow = ManagedSingleton<StageHelper>.Instance.StatusCorrection(pow, StageHelper.STAGE_RULE_STATUS.CRI_RESIST);
		return tRefPassiveskill.GetAddStatus(5, PlayerWeapons[nCurrentWeapn].weaponStatus.nWeaponCheck) + pow;
	}

	public override ObscuredInt GetReduceCriDmgPercent(int nCurrentWeapn)
	{
		return ManagedSingleton<StageHelper>.Instance.StatusCorrection(0, StageHelper.STAGE_RULE_STATUS.CRIDMG_RESIST) + tRefPassiveskill.GetAddStatus(7, PlayerWeapons[nCurrentWeapn].weaponStatus.nWeaponCheck);
	}

	public override ObscuredInt GetBlock()
	{
		int pow = (int)SetPBP.tPlayerStatus.nBlockPercent + (int)SetPBP.mainWStatus.nBlockPercent + (int)SetPBP.subWStatus.nBlockPercent + (int)SetPBP.chipStatus.nBlockPercent;
		pow = ManagedSingleton<StageHelper>.Instance.StatusCorrection(pow, StageHelper.STAGE_RULE_STATUS.PARRY);
		return pow;
	}

	public override ObscuredInt GetBlockDmgPercent()
	{
		int pow = (int)SetPBP.tPlayerStatus.nBlockDmgPercent + (int)SetPBP.mainWStatus.nBlockDmgPercent + (int)SetPBP.subWStatus.nBlockDmgPercent + (int)SetPBP.chipStatus.nBlockDmgPercent + OrangeConst.PLAYER_PARRYDEF_BASE;
		pow = ManagedSingleton<StageHelper>.Instance.StatusCorrection(pow, StageHelper.STAGE_RULE_STATUS.PARRY_DEF);
		return pow;
	}

	public override Vector2 GetDamageTextPos()
	{
		return base.transform.position.xy() + new Vector2(0f, 1.5f);
	}

	public void CloseChipEfx()
	{
		if (GetCurrentWeaponObj().ChipEfx != null && GetCurrentWeaponObj().chip_switch)
		{
			tRefPassiveskill.bUsePassiveskill = false;
			GetCurrentWeaponObj().ChipEfx.ActiveChipSkill(false);
		}
	}

	public void ReStartInvicibleTimer()
	{
		_invincibleTimer.TimerReset();
		_invincibleTimer.TimerStart();
	}

	public void SetLockBulletForNextStatus()
	{
		bLockBulletForNextStatus = true;
		StartCoroutine(LockBulletForNextStatusCoroutine());
	}

	private IEnumerator LockBulletForNextStatusCoroutine()
	{
		while (bLockBulletForNextStatus)
		{
			yield return CoroutineDefine._waitForEndOfFrame;
			if (CurMainStatus != MainStatus.SKILL)
			{
				bLockBulletForNextStatus = false;
			}
		}
	}

	public bool GuardCalculate(HurtPassParam tHurtPassParam)
	{
		return false;
	}

	public override ObscuredInt Hurt(HurtPassParam tHurtPassParam)
	{
		if (OrangeBattleUtility.GlobalInvincible || GuardTransform.Contains(tHurtPassParam.nSubPartID) || GuardCalculateEvt(tHurtPassParam))
		{
			GuardHurtEvt.CheckTargetToInvoke(tHurtPassParam);
			UpdateHurtAction();
			return Hp;
		}
		int num = 100;
		CharacterControlBase component = GetComponent<CharacterControlBase>();
		if ((bool)component)
		{
			num = component.ShieldDmgReduce(tHurtPassParam);
		}
		if (num < 100)
		{
			tHurtPassParam.dmg = Mathf.RoundToInt((float)((int)tHurtPassParam.dmg * num) * 0.01f);
		}
		_invincibleTimer.TimerStart();
		if ((int)tHurtPassParam.dmg >= 0)
		{
			lastCreateBulletWeaponStatus = GetCurrentWeaponObj().weaponStatus;
			lastCreateBulletTransform = GetCurrentWeaponObj().ShootTransform[0];
			if (IsLocalPlayer)
			{
				tRefPassiveskill.HurtTrigger(ref tHurtPassParam.dmg, GetCurrentWeaponObj().weaponStatus.nWeaponCheck, ref selfBuffManager, CreateBulletByLastWSTranform);
			}
		}
		tHurtPassParam.dmg = selfBuffManager.ReduceDmgByEnergyShild(tHurtPassParam.dmg);
		Hp = (int)Hp - (int)tHurtPassParam.dmg;
		UpdateHurtAction();
		if ((int)Hp > 0)
		{
			if (StageUpdate.gbRegisterPvpPlayer)
			{
				CharacterMaterials.Hurt();
			}
			else
			{
				CharacterMaterials.Invincible();
			}
			if (IsLocalPlayer && (_plHurtVoiceTimer.GetMillisecond() > hurtVoiceLimit || !_plHurtVoiceTimer.IsStarted()))
			{
				if ((int)tHurtPassParam.dmg > 0 && !CheckPlayWeakVoice())
				{
					PlayVoiceCB(Voice.HURT1);
				}
				hurtVoiceLimit = OrangeBattleUtility.Random(1000, 2000);
				_plHurtVoiceTimer.TimerReset();
				_plHurtVoiceTimer.TimerStart();
			}
			else
			{
				CheckPlayWeakVoice();
			}
		}
		else
		{
			TriggerDeadAfterHurt(tHurtPassParam.owner);
			Hp = 0;
			_weakVoice = 0;
		}
		return Hp;
	}

	public IEnumerator SetKnockOutWithAttacker(OrangeCharacter attacker)
	{
		if ((int)Hp <= 0)
		{
			yield break;
		}
		LockControl();
		_pvpCCsafety.TimerReset();
		_pvpCCsafety.TimerStart();
		Vector3 position = _transform.position;
		Vector3 zero = Vector3.zero;
		Vector2 dir = ((attacker._characterDirection == CharacterDirection.LEFT) ? Vector2.left : Vector2.right);
		SetStatus(MainStatus.HURT, SubStatus.LAND);
		if (!_isJack)
		{
			base.transform.SetParent(attacker.transform, true);
			_isJack = true;
			IgnoreGravity = true;
		}
		while (!attacker._releaseJack && (int)attacker.Hp > 0 && _isJack && (bool)_transform.parent)
		{
			if (_pvpCCsafety.GetMillisecond() > 3000)
			{
				attacker._releaseJack = true;
				continue;
			}
			Vector2 origin = new Vector2(_transform.position.x, _transform.position.y + 0.5f);
			float x = _transform.position.x;
			float num = _transform.position.y;
			RaycastHit2D raycastHit2D = OrangeBattleUtility.RaycastIgnoreSelf(origin, dir, 0.5f, 512, _transform);
			if ((bool)raycastHit2D.collider)
			{
				x = raycastHit2D.point.x - 0.5f * (float)attacker._characterDirection;
			}
			if ((bool)OrangeBattleUtility.RaycastIgnoreSelf(origin, Vector3.up, 1.5f, 512, _transform).collider)
			{
				num -= 0.1f;
			}
			_transform.position = new Vector2(x, num);
			yield return CoroutineDefine._waitForEndOfFrame;
		}
		_pvpCCsafety.TimerReset();
		if ((int)Hp > 0)
		{
			_transform.SetParentNull();
			IgnoreGravity = false;
			SetStatus(MainStatus.FALL, SubStatus.TELEPORT_POSE);
			SetHorizontalSpeed((int)attacker._characterDirection * WalkSpeed);
		}
		else
		{
			base.transform.SetParentNull();
		}
		if (!IsLocalPlayer)
		{
			SetStatus(MainStatus.HURT, SubStatus.DASH_END);
			yield return new WaitForSeconds(0.7f);
			SetStatus(MainStatus.FALL, SubStatus.TELEPORT_POSE);
		}
		OrangeCharacter orangeCharacter = this;
		OrangeCharacter orangeCharacter2 = this;
		bool isJack = false;
		orangeCharacter2.LockInput = false;
		orangeCharacter._isJack = isJack;
		Vector2 origin2 = new Vector2(_transform.position.x, _transform.position.y + 0.5f);
		float x2 = _transform.position.x;
		float num2 = _transform.position.y;
		if ((bool)OrangeBattleUtility.RaycastIgnoreSelf(origin2, dir, 0.5f, 512, _transform).collider)
		{
			x2 = _transform.position.x - 0.5f * (float)attacker._characterDirection;
		}
		if ((bool)OrangeBattleUtility.RaycastIgnoreSelf(origin2, Vector3.up, 1.5f, 512, _transform).collider)
		{
			num2 -= 0.1f;
		}
		Controller.LogicPosition = new VInt3(new Vector3(x2, num2));
	}

	private IEnumerator WaitDestoryCoroutine()
	{
		for (float fTime = 0f; fTime < 1.5f; fTime += Time.deltaTime)
		{
			if (bIsDeadEnd)
			{
				break;
			}
			yield return CoroutineDefine._waitForEndOfFrame;
		}
		if (!bIsDeadEnd)
		{
			bIsDeadEnd = true;
			listRenders.AddRange(base.transform.GetComponentsInChildren<Renderer>());
			for (int num = listRenders.Count - 1; num >= 0; num--)
			{
				if (!listRenders[num].enabled)
				{
					listRenders.RemoveAt(num);
				}
				else
				{
					listRenders[num].enabled = false;
				}
			}
			Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_PLAYER_DESTROY_ED, this, false);
		}
		bNeedUpdateAlways = false;
	}

	public abstract void DieFromServer();

	protected abstract void TriggerDeadAfterHurt(string killer);

	public virtual void PlayerDead(bool bJustRemove = false)
	{
		selfBuffManager.StopLoopSE();
		if (!bJustRemove)
		{
			Controller.Collider2D.enabled = false;
			base.AllowAutoAim = false;
			bNeedUpdateAlways = true;
			selfBuffManager.Init(this);
			if (_isJack)
			{
				if (base.transform.parent != null)
				{
					base.transform.SetParentNull();
				}
				IgnoreGravity = false;
			}
			Hp = 0;
			UpdateHurtAction();
			bIsDeadEnd = false;
			_chargeShootObj.StopCharge(-1);
			UpdateSkillIcon(PlayerSkills);
			PlayerSkills[0].FastBulletDatas[0] = PlayerSkills[0].BulletData;
			PlayerSkills[1].FastBulletDatas[0] = PlayerSkills[1].BulletData;
			_weakVoice = 0;
			PlayVoiceCB(Voice.LOSE1);
			PlayBattleSE(BattleSE.CRI_BATTLESE_BT_DIE01);
			MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("obj_player_die", _transform.position, NormalQuaternion, Array.Empty<object>());
			if (CurrentActiveSkill == 100)
			{
				CurrentActiveSkill = -1;
			}
			else
			{
				ClearSkillEvt.CheckTargetToInvoke();
			}
			DisableCurrentWeapon();
			CloseChipEfx();
			_velocity = VInt3.zero;
			_velocityExtra = VInt3.zero;
			_velocityShift = VInt3.zero;
			_velocityForceField = VInt3.zero;
			SetStatus(MainStatus.HURT, SubStatus.LAND);
			StartCoroutine(WaitDestoryCoroutine());
			if (sPlayerID == MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify)
			{
				EventManager.StageCameraFocus stageCameraFocus = new EventManager.StageCameraFocus();
				stageCameraFocus.nMode = 3;
				stageCameraFocus.bLock = false;
				Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_CAMERA_FOCUS, stageCameraFocus);
			}
		}
		else
		{
			SetStatus(MainStatus.IDLE, SubStatus.IDLE);
			if (IsLocalPlayer)
			{
				IsLocalPlayer = false;
			}
			bNeedUpdateAlways = true;
			NullHurtAction();
			RemovePlayerObjInfoBar();
			Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_PLAYER_DESTROY_ED, this, true);
			CharacterMaterials.Disappear(delegate
			{
				bNeedUpdateAlways = false;
				SetActiveFalse();
			});
		}
		CharacterControlBase component = GetComponent<CharacterControlBase>();
		if ((bool)component)
		{
			component.ControlCharacterDead();
		}
	}

	protected void PlayerPressLeft()
	{
		if (ReverseRightAndLeft)
		{
			PlayerPress(ButtonId.RIGHT);
		}
		else
		{
			PlayerPress(ButtonId.LEFT);
		}
	}

	protected void PlayerPressRight()
	{
		if (ReverseRightAndLeft)
		{
			PlayerPress(ButtonId.LEFT);
		}
		else
		{
			PlayerPress(ButtonId.RIGHT);
		}
	}

	protected void PlayerPress(ButtonId pressedBtn)
	{
		if (PlayerSetting.DoubleTapDash != 0 && _doubleTapTimer.GetMillisecond() < 200 && pressedBtn == _doubleTapBtn && MonoBehaviourSingleton<InputManager>.Instance.VirtualPadSystem.GetButton(ButtonId.DASH).gameObject.activeSelf)
		{
			PlayerPressDashCB(pressedBtn);
		}
		_doubleTapTimer.TimerStart();
		_doubleTapBtn = pressedBtn;
	}

	private bool SlashBlockTurnDirection()
	{
		switch (CurMainStatus)
		{
		case MainStatus.HURT:
		case MainStatus.WALLKICK:
			return true;
		case MainStatus.SLASH:
		{
			SubStatus curSubStatus = CurSubStatus;
			if (curSubStatus == SubStatus.LAND)
			{
				return true;
			}
			break;
		}
		}
		return false;
	}

	protected void PlayerHeldLeft()
	{
		if (!SlashBlockTurnDirection())
		{
			if (ReverseRightAndLeft)
			{
				PlayerHeldLeftRight(CharacterDirection.RIGHT);
			}
			else
			{
				PlayerHeldLeftRight(CharacterDirection.LEFT);
			}
		}
	}

	protected void PlayerHeldRight()
	{
		if (!SlashBlockTurnDirection())
		{
			if (ReverseRightAndLeft)
			{
				PlayerHeldLeftRight(CharacterDirection.LEFT);
			}
			else
			{
				PlayerHeldLeftRight(CharacterDirection.RIGHT);
			}
		}
	}

	protected void PlayerHeldLeftRight(CharacterDirection destFacing)
	{
		int num = CalculateMoveSpeed();
		switch (CurMainStatus)
		{
		case MainStatus.IDLE:
			if ((destFacing == CharacterDirection.RIGHT) ? Controller.Collisions.right : Controller.Collisions.left)
			{
				break;
			}
			switch (CurSubStatus)
			{
			case SubStatus.IDLE:
				if (_velocity.y <= 0)
				{
					SetStatus(MainStatus.WALK, SubStatus.TELEPORT_POSE);
				}
				break;
			case SubStatus.SLASH1_END:
				SetStatus(MainStatus.WALK, SubStatus.WIN_POSE);
				break;
			default:
				SetStatus(MainStatus.WALK, SubStatus.WIN_POSE);
				break;
			}
			break;
		case MainStatus.WALK:
			if ((destFacing == CharacterDirection.RIGHT) ? Controller.Collisions.right : Controller.Collisions.left)
			{
				SetStatus(MainStatus.IDLE, SubStatus.IDLE);
				break;
			}
			switch (CurSubStatus)
			{
			case SubStatus.TELEPORT_POSE:
				SetHorizontalSpeed((int)destFacing * StepSpeed);
				break;
			case SubStatus.WIN_POSE:
			case SubStatus.RIDE_ARMOR:
			case SubStatus.IDLE:
			case SubStatus.LAND:
			case SubStatus.DASH_END:
			case SubStatus.CROUCH_UP:
				SetHorizontalSpeed((int)destFacing * num);
				break;
			}
			break;
		case MainStatus.DASH:
			if (base._characterDirection != destFacing)
			{
				SubStatus curSubStatus2 = CurSubStatus;
				PlayerStopDashing();
				ResetDashChance();
				_nJumpCount = 0;
				switch (curSubStatus2)
				{
				case SubStatus.TELEPORT_POSE:
					SetStatus(MainStatus.WALK, SubStatus.WIN_POSE);
					break;
				case SubStatus.WIN_POSE:
					SetStatus(MainStatus.WALK, SubStatus.IDLE);
					break;
				case SubStatus.RIDE_ARMOR:
					SetStatus(MainStatus.WALK, SubStatus.WIN_POSE);
					break;
				}
			}
			break;
		case MainStatus.AIRDASH:
			if (base._characterDirection != destFacing)
			{
				SetStatus(MainStatus.FALL, SubStatus.TELEPORT_POSE);
			}
			break;
		case MainStatus.JUMP:
		{
			SubStatus curSubStatus = CurSubStatus;
			if (curSubStatus == SubStatus.IDLE)
			{
				SetHorizontalSpeed((int)destFacing * CalculateFlySpeed());
				break;
			}
			SetHorizontalSpeed((int)destFacing * num);
			if (CurrentFrame > 0.8f)
			{
				PlWallStopCheckEvt((int)destFacing);
			}
			break;
		}
		case MainStatus.FALL:
		{
			SetHorizontalSpeed((int)destFacing * num);
			SubStatus curSubStatus = CurSubStatus;
			if (curSubStatus != SubStatus.RIDE_ARMOR)
			{
				PlWallStopCheckEvt((int)destFacing);
			}
			break;
		}
		case MainStatus.WALLGRAB:
		{
			SubStatus curSubStatus = CurSubStatus;
			if (curSubStatus != 0 && (uint)(curSubStatus - 1) <= 3u)
			{
				destFacing = (CharacterDirection)(0 - destFacing);
			}
			if (!Controller.BelowInBypassRange && !Solid_meeting((float)destFacing * ((CurSubStatus == SubStatus.TELEPORT_POSE) ? 2f : (-2f)), 0f))
			{
				Dashing = false;
				PlayerStopDashing();
				SetStatus(MainStatus.FALL, SubStatus.TELEPORT_POSE);
			}
			break;
		}
		case MainStatus.SKILL:
			PlayerHeldLeftRightSkillCB.CheckTargetToInvoke(destFacing);
			return;
		}
		base._characterDirection = destFacing;
	}

	protected void PlayerReleaseLeftRight()
	{
		switch (CurMainStatus)
		{
		default:
			SetHorizontalSpeed(0);
			break;
		case MainStatus.FALL:
			SetHorizontalSpeed(0);
			break;
		case MainStatus.JUMP:
			SetHorizontalSpeed(0);
			break;
		case MainStatus.SKILL:
			PlayerReleaseLeftRightSkillCB.CheckTargetToInvoke(null);
			break;
		case MainStatus.WALK:
		{
			SubStatus curSubStatus2 = CurSubStatus;
			SetStatus(MainStatus.IDLE, SubStatus.IDLE);
			if ((bool)Controller.BelowInBypassRange)
			{
				StageSceneObjParam componentInParent = Controller.BelowInBypassRange.transform.GetComponentInParent<StageSceneObjParam>();
				if (componentInParent != null && componentInParent.IsIceBlock() && Vector2.Angle(Controller.BelowInBypassRange.normal, Vector2.up) < 10f)
				{
					fIceSlideParam = componentInParent.fIceSliderParam;
					bIceSlide = true;
				}
			}
			if (bIceSlide)
			{
				fIceSlide = _velocity.vec3.x;
			}
			SetHorizontalSpeed(0);
			break;
		}
		case MainStatus.DASH:
		case MainStatus.AIRDASH:
			if (_dashTriggerBtn == ButtonId.LEFT || _dashTriggerBtn == ButtonId.RIGHT)
			{
				PlayerStopDashing();
			}
			break;
		case MainStatus.WALLGRAB:
		{
			SetHorizontalSpeed(0);
			SetVerticalSpeed(0);
			SubStatus curSubStatus = CurSubStatus;
			if ((uint)curSubStatus <= 1u)
			{
				switch (base._characterDirection)
				{
				case CharacterDirection.LEFT:
					if (ManagedSingleton<InputStorage>.Instance.IsReleased(UserID, ButtonId.LEFT))
					{
						base._characterDirection = CharacterDirection.RIGHT;
					}
					break;
				case CharacterDirection.RIGHT:
					if (ManagedSingleton<InputStorage>.Instance.IsReleased(UserID, ButtonId.RIGHT))
					{
						base._characterDirection = CharacterDirection.LEFT;
					}
					break;
				}
			}
			AddForceFieldProxy(new VInt3((int)base._characterDirection * FP_WALLFALL_FORCE.x, FP_WALLFALL_FORCE.y, 0));
			SetStatus(MainStatus.JUMP, SubStatus.WIN_POSE);
			break;
		}
		case MainStatus.HURT:
		case MainStatus.WALLKICK:
			break;
		}
	}

	protected void PlayerHeldUp()
	{
		MainStatus curMainStatus = CurMainStatus;
		if (curMainStatus != MainStatus.JUMP)
		{
			return;
		}
		SubStatus curSubStatus = CurSubStatus;
		if (curSubStatus == SubStatus.IDLE)
		{
			if (base.transform.localPosition.y >= HoveringStartHeight + HoveringHighLimit)
			{
				SetVerticalSpeed(0);
			}
			else
			{
				SetVerticalSpeed(CalculateFlySpeed());
			}
			UpdateFlyDirection(FLY_DIR.UP);
		}
	}

	protected void PlayerReleaseUp()
	{
		MainStatus curMainStatus = CurMainStatus;
		if (curMainStatus == MainStatus.JUMP)
		{
			SubStatus curSubStatus = CurSubStatus;
			if (curSubStatus == SubStatus.IDLE)
			{
				SetVerticalSpeed(0);
				UpdateFlyDirection(FLY_DIR.HORIZONTAL);
			}
		}
	}

	protected void PlayerPressDown()
	{
		MainStatus curMainStatus = CurMainStatus;
		if (curMainStatus != MainStatus.SKILL)
		{
			if (PlayerSetting.DoubleTapThrough != 0 && _doubleTapTimer.GetMillisecond() < 200 && _doubleTapBtn == ButtonId.DOWN && Controller.Collisions.below && Controller.Collisions.JSB_below)
			{
				SetStatus(MainStatus.FALL, SubStatus.TELEPORT_POSE);
				Controller.JumpThrough = true;
				CheckSemiBlockPlayerRender();
				_nJumpCount = 1;
				CheckUseJumpKeyTrigger();
			}
			_doubleTapTimer.TimerStart();
			_doubleTapBtn = ButtonId.DOWN;
		}
	}

	protected void PlayerHeldDown()
	{
		switch (CurMainStatus)
		{
		default:
		{
			int num = 14;
			break;
		}
		case MainStatus.IDLE:
		case MainStatus.WALK:
			if (IsInGround)
			{
				SetStatus(MainStatus.CROUCH, SubStatus.TELEPORT_POSE);
				SetHorizontalSpeed(0);
			}
			break;
		case MainStatus.JUMP:
		{
			SubStatus curSubStatus = CurSubStatus;
			if (curSubStatus == SubStatus.IDLE)
			{
				SetVerticalSpeed(-CalculateFlySpeed());
				UpdateFlyDirection(FLY_DIR.DOWN);
			}
			break;
		}
		case MainStatus.CROUCH:
		case MainStatus.HURT:
		case MainStatus.DASH:
		case MainStatus.AIRDASH:
			break;
		}
	}

	protected void PlayerReleaseDown()
	{
		switch (CurMainStatus)
		{
		case MainStatus.CROUCH:
			SetStatus(MainStatus.IDLE, SubStatus.CROUCH_UP);
			break;
		case MainStatus.JUMP:
		{
			SubStatus curSubStatus = CurSubStatus;
			if (curSubStatus == SubStatus.IDLE)
			{
				SetVerticalSpeed(0);
				UpdateFlyDirection(FLY_DIR.HORIZONTAL);
			}
			break;
		}
		}
	}

	public void PlayerPressJump()
	{
		UpdateHitBox();
		if ((bool)Solid_meeting(0f, 1f) && !Solid_meeting(0f, 0f))
		{
			return;
		}
		switch (CurMainStatus)
		{
		case MainStatus.AIRDASH:
			if ((bool)Solid_meeting(0f, 0f))
			{
				SetStatus(MainStatus.FALL, SubStatus.TELEPORT_POSE);
			}
			return;
		case MainStatus.CROUCH:
			if ((bool)Solid_meeting(0f, 0f))
			{
				SetStatus(MainStatus.CROUCH, SubStatus.TELEPORT_POSE);
			}
			break;
		case MainStatus.DASH:
			if ((bool)SolidMettingIgnoreSideStageBlockWall())
			{
				SetStatus(MainStatus.CROUCH, SubStatus.TELEPORT_POSE);
			}
			break;
		case MainStatus.HURT:
		case MainStatus.WALLKICK:
		case MainStatus.SKILL:
			return;
		}
		if (!Controller.BelowInBypassRange && !Controller.Collisions.below && !_leaveRideArmorJump)
		{
			if (!PlWallJumpCheckEvt() && (_doubleJumpEnable || _hoveringEnable) && _dashChance > 0 && (CurMainStatus == MainStatus.JUMP || CurMainStatus == MainStatus.FALL))
			{
				PlayCharaSeCB(CharaSE.JUMPHIGH);
				string p_fxName = ((_velocity.x > 0) ? JumpLeftFx : ((_velocity.x >= 0) ? JumpUpFx : JumpRightFx));
				if (!base.IsHidden)
				{
					MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(p_fxName, _transform.position, NormalQuaternion, Array.Empty<object>());
				}
				if (IsLocalPlayer)
				{
					PlayVoiceCB(Voice.JUMP1);
				}
				_dashChance--;
				_nJumpCount = 2;
				if (!_hoveringEnable)
				{
					SetStatus(MainStatus.JUMP, SubStatus.TELEPORT_POSE);
					SetSpeed(0, JumpSpeedEx);
				}
				else
				{
					_hoveringTimer.TimerStart();
					_isHovering = true;
					HoveringStartHeight = base.transform.localPosition.y;
					SetStatus(MainStatus.JUMP, SubStatus.IDLE);
					IgnoreGravity = true;
					_ignoreGlobalVelocity = true;
					_ignoreVelocityExtra = true;
					SetSpeed(0, 0);
				}
			}
		}
		else if (CurMainStatus == MainStatus.CROUCH && Controller.Collisions.below && Controller.Collisions.JSB_below)
		{
			SetStatus(MainStatus.FALL, SubStatus.TELEPORT_POSE);
			Controller.JumpThrough = true;
			_nJumpCount = 1;
			CheckSemiBlockPlayerRender();
		}
		else
		{
			PlayCharaSeCB(CharaSE.JUMP);
			string p_fxName2 = ((_velocity.x > 0) ? JumpLeftFx : ((_velocity.x >= 0) ? JumpUpFx : JumpRightFx));
			if (!base.IsHidden)
			{
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(p_fxName2, _transform.position, NormalQuaternion, Array.Empty<object>());
			}
			if (IsLocalPlayer)
			{
				PlayVoiceCB(Voice.JUMP1);
			}
			ResetDashChance();
			_nJumpCount = 1;
			if (ManagedSingleton<InputStorage>.Instance.IsHeld(UserID, ButtonId.DASH))
			{
				if (!Dashing)
				{
					PlayCharaSeCB(CharaSE.DASH);
				}
				Dashing = true;
			}
			else if (CurMainStatus == MainStatus.FALL)
			{
				Dashing = false;
			}
			if (Dashing)
			{
				_dashChance--;
			}
			SetStatus(MainStatus.JUMP, SubStatus.TELEPORT_POSE);
			if (_leaveRideArmorJump)
			{
				int x = Mathf.RoundToInt(_moveSpeedMultiplier * (float)WalkSpeed * (float)((base._characterDirection != CharacterDirection.LEFT) ? 1 : (-1)));
				SetSpeed(x, JumpSpeedEx);
				_leaveRideArmorJump = false;
			}
			else
			{
				SetSpeed(0, JumpSpeedEx);
			}
		}
		if (Controller.JumpUPThrough)
		{
			if (_jumpThroughCoroutine == null)
			{
				OrangeBattleUtility.ChangeLayersRecursively(ModelTransform, ManagedSingleton<OrangeLayerManager>.Instance.RenderSPEnemy, false);
				_jumpThroughCoroutine = StartCoroutine(WaitJumpUPThrough());
			}
			else if (_dashChance <= 0)
			{
				StopCoroutine(_jumpThroughCoroutine);
				_jumpThroughCoroutine = StartCoroutine(WaitJumpUPThrough());
			}
		}
	}

	public void PlayerReleaseJump()
	{
		MainStatus curMainStatus;
		if (PlayerSetting.JumpClassic != 0)
		{
			curMainStatus = CurMainStatus;
			if (curMainStatus == MainStatus.JUMP)
			{
				SubStatus curSubStatus = CurSubStatus;
				if (curSubStatus == SubStatus.IDLE)
				{
					PlayerStopHovering();
				}
				if (_velocity.y > 0)
				{
					SetVerticalSpeed(0);
				}
			}
			return;
		}
		curMainStatus = CurMainStatus;
		if (curMainStatus == MainStatus.JUMP)
		{
			SubStatus curSubStatus = CurSubStatus;
			if (curSubStatus == SubStatus.IDLE)
			{
				PlayerStopHovering();
			}
		}
	}

	public void PlayerResetPressJump()
	{
		PlayerPressJumpCB = PlayerPressJump;
	}

	private void PlayerStopHovering(bool setFallStatus = true)
	{
		if (_hoveringEnable)
		{
			_hoveringTimer.TimerStop();
			_ignoreGravity = false;
			_ignoreGlobalVelocity = false;
			_ignoreVelocityExtra = false;
			_isHovering = false;
			_bFloatingFlag = false;
			UpdateFlyDirection(FLY_DIR.HORIZONTAL);
			StopHoveringEvt.CheckTargetToInvoke();
			if (setFallStatus)
			{
				SetStatus(MainStatus.FALL, SubStatus.TELEPORT_POSE);
			}
		}
	}

	private bool CheckCanDash()
	{
		if (!CanUseDash())
		{
			return false;
		}
		MainStatus curMainStatus = CurMainStatus;
		if (curMainStatus == MainStatus.HURT || curMainStatus == MainStatus.SKILL)
		{
			return false;
		}
		if (CheckDashLockEvt())
		{
			return false;
		}
		return true;
	}

	public void PlayerPressDash(object tParam)
	{
		if (CheckCanDash())
		{
			ButtonId buttonId = (ButtonId)tParam;
			CharacterDirection destFacing = base._characterDirection;
			_dashTriggerBtn = buttonId;
			if (ManagedSingleton<InputStorage>.Instance.IsHeld(UserID, ButtonId.RIGHT))
			{
				destFacing = ((!ReverseRightAndLeft) ? CharacterDirection.RIGHT : CharacterDirection.LEFT);
			}
			else if (ManagedSingleton<InputStorage>.Instance.IsHeld(UserID, ButtonId.LEFT))
			{
				destFacing = (ReverseRightAndLeft ? CharacterDirection.RIGHT : CharacterDirection.LEFT);
			}
			PlayerPressDash(buttonId, destFacing);
		}
	}

	protected void PlayerPressDash(ButtonId triggerBtn, CharacterDirection destFacing)
	{
		_dashTriggerBtn = triggerBtn;
		if ((destFacing == CharacterDirection.RIGHT) ? Controller.Collisions.right : Controller.Collisions.left)
		{
			return;
		}
		int num = Mathf.RoundToInt((float)DashSpeedEx * _moveSpeedMultiplier);
		if ((bool)Controller.BelowInBypassRange && CurMainStatus != MainStatus.JUMP)
		{
			if (!Dashing)
			{
				SetStatus(MainStatus.DASH, SubStatus.TELEPORT_POSE);
				PlayCharaSeCB(CharaSE.DASH);
				if (!base.IsHidden)
				{
					MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(DashSmokeFx, _transform.position, (base._characterDirection == CharacterDirection.LEFT) ? ReversedQuaternion : NormalQuaternion, Array.Empty<object>());
				}
				SetSpeed((int)destFacing * num, 0);
				Dashing = true;
				_dashTimer.TimerStart();
				base._characterDirection = destFacing;
			}
		}
		else if (_airDashEnable && _dashChance > 0 && (CurMainStatus == MainStatus.JUMP || CurMainStatus == MainStatus.FALL))
		{
			PlayCharaSeCB(CharaSE.DASH);
			if (ManagedSingleton<InputStorage>.Instance.IsHeld(UserID, ButtonId.JUMP) && ManagedSingleton<InputStorage>.Instance.GetHeldButtonFrame(UserID, ButtonId.JUMP) < 2f)
			{
				Dashing = true;
				_dashChance--;
				_dashTimer.TimerStart();
				base._characterDirection = destFacing;
			}
			else
			{
				SetStatus(MainStatus.AIRDASH, SubStatus.TELEPORT_POSE);
				_dashChance--;
				SetSpeed((int)destFacing * num, 0);
				Dashing = true;
				_dashTimer.TimerStart();
				base._characterDirection = destFacing;
			}
		}
	}

	protected internal RaycastHit2D Solid_meeting(float x, float y, int flag = -1)
	{
		if (flag != -1)
		{
			return Controller.ObjectMeeting(x * OrangeBattleUtility.PPU, y * OrangeBattleUtility.PPU, flag);
		}
		return Controller.SolidMeeting(x * OrangeBattleUtility.PPU, y * OrangeBattleUtility.PPU);
	}

	protected internal RaycastHit2D SolidMettingIgnoreSideStageBlockWall()
	{
		return Controller.ObjectMeetingIgnoreSideStageBlockWall(Controller.collisionMask);
	}

	public bool PlWallStopCheck(int dir)
	{
		if (!Controller.BelowInBypassRange && (bool)Solid_meeting(dir, 0f, Controller.wallkickMask))
		{
			PlayCharaSeCB(CharaSE.KABEPETA);
			ResetDashChance();
			_nJumpCount = 0;
			SetStatus(MainStatus.WALLGRAB, SubStatus.TELEPORT_POSE);
			PlayerStopDashing();
			PlayerStopHovering(false);
			SetHorizontalSpeed(0);
			return true;
		}
		return false;
	}

	public bool PlWallJumpCheck()
	{
		float value = 1f;
		RaycastHit2D raycastHit2D = Solid_meeting(4f, 0f, Controller.wallkickMask);
		if (!raycastHit2D)
		{
			raycastHit2D = Solid_meeting(-4f, 0f, Controller.wallkickMask);
			value = -1f;
		}
		RaycastHit2D raycastHit2D2 = Solid_meeting(3f, 0f, _wallKickMask);
		if (!raycastHit2D2)
		{
			raycastHit2D2 = Solid_meeting(-3f, 0f, _wallKickMask);
		}
		bool flag = false;
		if ((bool)raycastHit2D2)
		{
			flag = true;
			StageBlockWall component = raycastHit2D2.collider.GetComponent<StageBlockWall>();
			if ((bool)component && component.SemiBlockMode != 0)
			{
				flag = false;
			}
		}
		if ((bool)raycastHit2D && ManagedSingleton<InputStorage>.Instance.IsHeld(UserID, ButtonId.JUMP) && !flag)
		{
			PlayCharaSeCB(CharaSE.KABEKERI);
			if (IsLocalPlayer)
			{
				PlayVoiceCB(Voice.JUMP1);
			}
			ResetDashChance();
			base._characterDirection = (CharacterDirection)Math.Sign(value);
			SetStatus(MainStatus.WALLKICK, SubStatus.TELEPORT_POSE);
			_wallKickTimer.TimerStart();
			SetSpeed(0, 0);
			Vector2 vector = new Vector2(raycastHit2D.point.x, _transform.position.y);
			if (!base.IsHidden)
			{
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(WallKickSparkFx, vector, (base._characterDirection == CharacterDirection.LEFT) ? ReversedQuaternion : NormalQuaternion, Array.Empty<object>());
			}
			return true;
		}
		if (CurMainStatus == MainStatus.JUMP)
		{
			if (base._characterDirection == CharacterDirection.LEFT && ManagedSingleton<InputStorage>.Instance.IsHeld(UserID, ButtonId.LEFT))
			{
				raycastHit2D = Solid_meeting(-15f, 0f, Controller.wallkickMask);
				if ((bool)raycastHit2D)
				{
					return true;
				}
			}
			if (base._characterDirection == CharacterDirection.RIGHT && ManagedSingleton<InputStorage>.Instance.IsHeld(UserID, ButtonId.RIGHT))
			{
				raycastHit2D = Solid_meeting(15f, 0f, Controller.wallkickMask);
				if ((bool)raycastHit2D)
				{
					return true;
				}
			}
		}
		return false;
	}

	public void PlayerReleaseDash()
	{
		if (!CheckDashLockEvt())
		{
			PlayerStopDashing();
		}
	}

	protected internal void PlayerStopDashing()
	{
		_dashTriggerBtn = ButtonId.NONE;
		_dashTimer.TimerStop();
		if (Controller.Collisions.below)
		{
			ResetDashChance();
			_nJumpCount = 0;
		}
		switch (CurMainStatus)
		{
		case MainStatus.HURT:
		case MainStatus.DASH:
		case MainStatus.AIRDASH:
		case MainStatus.WALLKICK:
		case MainStatus.WALLGRAB:
		case MainStatus.SKILL:
			Dashing = false;
			if ((bool)Controller.BelowInBypassRange)
			{
				StageSceneObjParam componentInParent = Controller.BelowInBypassRange.transform.GetComponentInParent<StageSceneObjParam>();
				if (componentInParent != null && componentInParent.IsIceBlock() && Vector2.Angle(Controller.BelowInBypassRange.normal, Vector2.up) < 10f)
				{
					fIceSlideParam = componentInParent.fIceSliderParam;
					bIceSlide = true;
				}
			}
			if (bIceSlide)
			{
				fIceSlide = _velocity.vec3.x;
			}
			SetHorizontalSpeed(0);
			break;
		}
		switch (CurMainStatus)
		{
		case MainStatus.DASH:
		{
			SubStatus curSubStatus = CurSubStatus;
			if ((uint)curSubStatus <= 4u)
			{
				SetStatus(MainStatus.IDLE, SubStatus.DASH_END);
			}
			PlayCharaSeCB(CharaSE.DASHEND);
			break;
		}
		case MainStatus.AIRDASH:
			if ((bool)Controller.BelowInBypassRange)
			{
				SetStatus(MainStatus.IDLE, SubStatus.DASH_END);
			}
			else
			{
				SetStatus(MainStatus.FALL, SubStatus.WIN_POSE);
			}
			break;
		}
	}

	public void PlayerHeldShoot()
	{
		MainStatus curMainStatus = CurMainStatus;
		if (curMainStatus == MainStatus.HURT || curMainStatus == MainStatus.SKILL || CurrentActiveSkill != -1)
		{
			return;
		}
		WEAPON_TABLE weaponData = GetCurrentWeaponObj().WeaponData;
		switch ((WeaponType)(short)weaponData.n_TYPE)
		{
		case WeaponType.Buster:
		case WeaponType.Spray:
		case WeaponType.SprayHeavy:
		case WeaponType.DualGun:
		case WeaponType.MGun:
		case WeaponType.Gatling:
		case WeaponType.Launcher:
			if (GetCurrentWeaponObj().LastUseTimer.GetMillisecond() < GetCurrentWeaponObj().BulletData.n_FIRE_SPEED || GetCurrentWeaponObj().MagazineRemain <= 0f)
			{
				break;
			}
			switch (CurMainStatus)
			{
			case MainStatus.IDLE:
				if (CurSubStatus >= SubStatus.SKILL_IDLE && CurSubStatus < SubStatus.SLASH1_END)
				{
					SetStatus(CurMainStatus, SubStatus.IDLE);
				}
				break;
			case MainStatus.FALL:
				if (CurSubStatus >= SubStatus.IDLE && CurSubStatus < SubStatus.GIGA_ATTACK_START)
				{
					SetStatus(CurMainStatus, SubStatus.TELEPORT_POSE);
				}
				break;
			}
			PlayerShootBuster(GetCurrentWeaponObj(), false, WeaponCurrent, 0);
			CheckUsePassiveSkill(WeaponCurrent + 2, GetCurrentWeaponObj().weaponStatus, GetCurrentWeaponObj().ShootTransform[0]);
			break;
		case WeaponType.Melee:
			if (CurrentActiveSkill == -1)
			{
				PlayerDoSlash(GetCurrentWeaponObj(), 0);
				CheckUsePassiveSkill(WeaponCurrent + 2, GetCurrentWeaponObj().weaponStatus, GetCurrentWeaponObj().ShootTransform[0]);
			}
			break;
		default:
			Debug.Log("Unknown weapon type" + weaponData.n_TYPE + "detected !");
			break;
		}
	}

	public void PlayerPressChip()
	{
		tRefPassiveskill.bUsePassiveskill = !tRefPassiveskill.bUsePassiveskill;
		if (GetCurrentWeaponObj().ChipEfx != null)
		{
			if (!base.IsHidden)
			{
				GetCurrentWeaponObj().ChipEfx.ActiveChipSkill(tRefPassiveskill.bUsePassiveskill, false);
			}
			GetCurrentWeaponObj().chip_switch = tRefPassiveskill.bUsePassiveskill;
			if (tRefPassiveskill.bUsePassiveskill)
			{
				PlayBattleSE(BattleSE.CRI_BATTLESE_BT_BOSSON);
			}
			else
			{
				PlayBattleSE(BattleSE.CRI_BATTLESE_BT_BOSSOFF);
			}
		}
	}

	protected internal BulletBase CreateFSBulletEx(WeaponStruct weaponStruct, sbyte lv, Vector3? ShotDir = null)
	{
		SKILL_TABLE sKILL_TABLE = weaponStruct.FastBulletDatas[lv];
		BulletBase poolObj = MonoBehaviourSingleton<PoolManager>.Instance.GetPoolObj<FSkillBullet>(sKILL_TABLE.s_MODEL);
		if (!poolObj)
		{
			return null;
		}
		int nowRecordNO = GetNowRecordNO();
		poolObj.UpdateBulletData(sKILL_TABLE, sPlayerName, nowRecordNO, nBulletRecordID++);
		((FSkillBullet)poolObj).UPdata_MapObject_Mask();
		poolObj.SetBulletAtk(weaponStruct.weaponStatus, selfBuffManager.sBuffStatus);
		poolObj.BulletLevel = weaponStruct.SkillLV;
		if (IsLocalPlayer)
		{
			poolObj.isForceSE = true;
		}
		poolObj.Active(base.AimTransform, Vector2.right * (float)base._characterDirection, TargetMask);
		if (ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT.ContainsKey(weaponStruct.FastBulletDatas[lv].n_LINK_SKILL))
		{
			for (int i = 0; i < weaponStruct.FastBulletDatas.Length; i++)
			{
				if (weaponStruct.FastBulletDatas[lv].n_LINK_SKILL == weaponStruct.FastBulletDatas[i].n_ID)
				{
					CreateFSBullet(weaponStruct, (sbyte)i, ShotDir);
				}
			}
		}
		return poolObj;
	}

	protected void CreateFSBullet(WeaponStruct weaponStruct, sbyte lv, Vector3? ShotPosition = null)
	{
		SKILL_TABLE sKILL_TABLE = weaponStruct.FastBulletDatas[lv];
		BulletBase poolObj = MonoBehaviourSingleton<PoolManager>.Instance.GetPoolObj<FSkillBullet>(sKILL_TABLE.s_MODEL);
		if (!poolObj)
		{
			return;
		}
		int nowRecordNO = GetNowRecordNO();
		poolObj.UpdateBulletData(sKILL_TABLE, sPlayerName, nowRecordNO, nBulletRecordID++);
		((FSkillBullet)poolObj).UPdata_MapObject_Mask();
		poolObj.SetBulletAtk(weaponStruct.weaponStatus, selfBuffManager.sBuffStatus);
		poolObj.BulletLevel = weaponStruct.SkillLV;
		if (IsLocalPlayer)
		{
			poolObj.isForceSE = true;
		}
		if (sKILL_TABLE.n_USE_TYPE == 1)
		{
			if (!ShotPosition.HasValue)
			{
				poolObj.Active(base.AimTransform, ShootDirection, TargetMask);
			}
			else
			{
				poolObj.Active(ShotPosition.Value, ShootDirection, TargetMask);
			}
		}
		else if (!ShotPosition.HasValue)
		{
			poolObj.Active(base.AimTransform, Vector2.right * (float)base._characterDirection, TargetMask);
		}
		else
		{
			poolObj.Active(ShotPosition.Value, Vector2.right * (float)base._characterDirection, TargetMask);
		}
		if (!ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT.ContainsKey(weaponStruct.FastBulletDatas[lv].n_LINK_SKILL))
		{
			return;
		}
		for (int i = 0; i < weaponStruct.FastBulletDatas.Length; i++)
		{
			if (weaponStruct.FastBulletDatas[lv].n_LINK_SKILL == weaponStruct.FastBulletDatas[i].n_ID)
			{
				CreateFSBullet(weaponStruct, (sbyte)i, ShotPosition);
			}
		}
	}

	private void InitializeFSRaySplasher()
	{
		for (int i = 0; i < PlayerFSkills.Length; i++)
		{
			if (PlayerFSkills[i].FastBulletDatas[0].n_EFFECT != 16)
			{
				continue;
			}
			int num = (int)PlayerFSkills[i].FastBulletDatas[0].f_EFFECT_X;
			float f_EFFECT_Z = PlayerFSkills[i].BulletData.f_EFFECT_Z;
			PET_TABLE petTable = ManagedSingleton<OrangeDataManager>.Instance.PET_TABLE_DICT[num];
			if (!MonoBehaviourSingleton<PoolManager>.Instance.ExistsInPool<SCH006Controller>(petTable.s_MODEL))
			{
				PetBuilder petBuilder = new GameObject().AddComponent<PetBuilder>();
				WeaponStatus weaponStatus = PlayerFSkills[i].weaponStatus;
				petBuilder.PetID = num;
				petBuilder.follow_skill_id = i;
				petBuilder.CreatePet(delegate(SCH006Controller obj)
				{
					MonoBehaviourSingleton<PoolManager>.Instance.CreatePoolBaseLocal<SCH006Controller>(obj, petTable.s_MODEL, 5);
				});
				MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_ballskill_000", 2);
			}
		}
	}

	private void ActivateFSRaySplasher()
	{
		Debug.Log("ActivateFSRaySplasher.");
		int key = (int)PlayerFSkills[WeaponCurrent].FastBulletDatas[0].f_EFFECT_X;
		long lifeTime = (long)(PlayerFSkills[WeaponCurrent].BulletData.f_EFFECT_Z * 1000f);
		PET_TABLE pET_TABLE = ManagedSingleton<OrangeDataManager>.Instance.PET_TABLE_DICT[key];
		WeaponStatus weaponStatus = PlayerFSkills[WeaponCurrent].weaponStatus;
		SCH006Controller poolObj = MonoBehaviourSingleton<PoolManager>.Instance.GetPoolObj<SCH006Controller>(pET_TABLE.s_MODEL);
		if ((bool)poolObj)
		{
			poolObj.transform.localEulerAngles = new Vector3(0f, 90f, 0f);
			poolObj.Set_follow_Player(this, false);
			poolObj.SetFollowEnabled(false);
			poolObj.ForceSetLocalPlayer(true);
			poolObj.SetParams(pET_TABLE.s_MODEL, lifeTime, pET_TABLE.n_SKILL_0, weaponStatus, 0L);
			poolObj.SetActive(true);
		}
	}

	protected void PlayerPressGigaAttack()
	{
		if (CurrentActiveSkill == -1 && !(PlayerFSkills[WeaponCurrent].MagazineRemain <= 0f))
		{
			PlayerFSkills[WeaponCurrent].MagazineRemain -= 1f;
			PlayerFSkills[WeaponCurrent].LastUseTimer.TimerStart();
			SetSpeed(0, 0);
			UpdateFlyDirection(FLY_DIR.HORIZONTAL);
			IgnoreGravity = true;
			CurrentActiveSkill = 100;
			DisableCurrentWeapon();
			SetStatus(MainStatus.GIGA_ATTACK, SubStatus.GIGA_ATTACK_START);
			if (PlayerFSkills[WeaponCurrent].FastBulletDatas[0].n_EFFECT == 16)
			{
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_ballskill_000", ModelTransform.position, Quaternion.identity, Array.Empty<object>());
				ActivateFSRaySplasher();
			}
			else if (PlayerFSkillsID[WeaponCurrent] == 9 && base.AimTransform.localPosition.y < 0.7f)
			{
				CreateFSBullet(PlayerFSkills[WeaponCurrent], 0, ModelTransform.position + new Vector3(0f, 0.7f, 0f));
			}
			else
			{
				CreateFSBullet(PlayerFSkills[WeaponCurrent], 0);
			}
			switch (PlayerFSkillsID[WeaponCurrent])
			{
			case 1:
				PlayBattleSE(BattleSE.CRI_BATTLESE_BT_SPSKILL01);
				break;
			case 2:
				PlayBattleSE(BattleSE.CRI_BATTLESE_BT_SPSKILL02);
				break;
			case 3:
				PlayBattleSE(BattleSE.CRI_BATTLESE_BT_SPSKILL03);
				break;
			case 4:
				PlayBattleSE(BattleSE.CRI_BATTLESE_BT_SPSKILL05_1);
				break;
			case 5:
				PlayBattleSE(BattleSE.CRI_BATTLESE_BT_SPSKILL04_1);
				break;
			case 6:
				PlayBattleSE(BattleSE.CRI_BATTLESE_BT_SPSKILL06_1);
				break;
			case 9:
				PlayBattleSE(BattleSE.CRI_BATTLESE_BT_SPSKILL09);
				break;
			case 10:
				PlayBattleSE(BattleSE.CRI_BATTLESE_BT_SPSKILL10);
				break;
			}
			PlayVoiceCB(Voice.ATTACK1);
		}
	}

	public void PlayerReleaseShoot()
	{
		useWeaponSE = true;
		_isHoldShoot = false;
	}

	public void CheckUsePassiveSkill(int id, WeaponStatus weaponStatus, Transform ShootTransform, Vector3? forceDirection = null, int? nSkillIndex = null)
	{
		if (ShootTransform == null)
		{
			ShootTransform = GetCurrentWeaponObj().ShootTransform[0];
			if (ShootTransform == null)
			{
				ShootTransform = base.transform.Find("aim");
				if (ShootTransform == null)
				{
					ShootTransform = base.transform;
					if (ShootTransform == null)
					{
						return;
					}
				}
			}
		}
		int ntriggerskillid = 0;
		switch (id)
		{
		case 0:
		case 1:
			ntriggerskillid = PlayerSkills[id].FastBulletDatas[nSkillIndex ?? PlayerSkills[id].ChargeLevel].n_ID;
			if (PlayerSkills[id].FastBulletDatas[PlayerSkills[id].ChargeLevel].n_TYPE == 2)
			{
				lastCreateBulletShotDir = Vector3.right * (float)base._characterDirection;
			}
			else
			{
				lastCreateBulletShotDir = null;
			}
			break;
		case 2:
		case 3:
			if (PlayerWeapons[id - 2].FastBulletDatas[PlayerWeapons[id - 2].ChargeLevel].n_TYPE == 2)
			{
				lastCreateBulletShotDir = Vector3.right * (float)base._characterDirection;
			}
			else
			{
				lastCreateBulletShotDir = null;
			}
			break;
		}
		if (forceDirection.HasValue)
		{
			lastCreateBulletShotDir = forceDirection;
		}
		lastCreateBulletWeaponStatus = weaponStatus;
		lastCreateBulletTransform = ShootTransform;
		if (IsLocalPlayer)
		{
			tRefPassiveskill.UseSkillTrigger(1 << id, ntriggerskillid, weaponStatus, ref selfBuffManager, CreateBulletByLastWSTranform);
		}
	}

	protected internal void CheckUsePassiveSkill(int id, SKILL_TABLE skillTable, WeaponStatus weaponStatus, Transform ShootTransform, Vector3? forceDirection = null)
	{
		if (ShootTransform == null)
		{
			ShootTransform = GetCurrentWeaponObj().ShootTransform[0];
			if (ShootTransform == null)
			{
				ShootTransform = base.transform.Find("aim");
				if (ShootTransform == null)
				{
					ShootTransform = base.transform;
					if (ShootTransform == null)
					{
						return;
					}
				}
			}
		}
		int ntriggerskillid = 0;
		switch (id)
		{
		case 0:
		case 1:
			ntriggerskillid = skillTable.n_ID;
			if (skillTable.n_TYPE == 2)
			{
				lastCreateBulletShotDir = Vector3.right * (float)base._characterDirection;
			}
			else
			{
				lastCreateBulletShotDir = null;
			}
			break;
		case 2:
		case 3:
			if (skillTable.n_TYPE == 2)
			{
				lastCreateBulletShotDir = Vector3.right * (float)base._characterDirection;
			}
			else
			{
				lastCreateBulletShotDir = null;
			}
			break;
		}
		if (forceDirection.HasValue)
		{
			lastCreateBulletShotDir = forceDirection;
		}
		lastCreateBulletWeaponStatus = weaponStatus;
		lastCreateBulletTransform = ShootTransform;
		if (IsLocalPlayer)
		{
			tRefPassiveskill.UseSkillTrigger(1 << id, ntriggerskillid, weaponStatus, ref selfBuffManager, CreateBulletByLastWSTranform);
		}
	}

	protected void CheckPerSecTriggerPassiveSkill()
	{
		if (!IsLocalPlayer || StageUpdate.bWaitReconnect)
		{
			return;
		}
		bool isMoving = true;
		if (CurMainStatus == MainStatus.IDLE && IsShoot == 0)
		{
			isMoving = false;
		}
		Transform transform = GetCurrentWeaponObj().ShootTransform[0];
		if (transform == null)
		{
			transform = base.transform.Find("aim");
			if (transform == null)
			{
				transform = base.transform;
				if (transform == null)
				{
					return;
				}
			}
		}
		lastCreateBulletTransform = transform;
		tRefPassiveskill.PerSecTrigger(isMoving, GetCurrentWeaponObj().weaponStatus, ref selfBuffManager, CreateBulletByLastWSTranform);
	}

	public void CheckUseKeyTrigger(ButtonId buttonId, bool checkSkillStatus = true)
	{
		if (!IsLocalPlayer || (CurMainStatus == MainStatus.SKILL && checkSkillStatus))
		{
			return;
		}
		switch (buttonId)
		{
		case ButtonId.SHOOT:
			if (IsVirtualButtonReloading(VirtualButtonId.SHOOT, GetCurrentWeaponObj()))
			{
				return;
			}
			break;
		case ButtonId.SKILL0:
			if (IsVirtualButtonReloading(VirtualButtonId.SKILL0, PlayerSkills[0]))
			{
				return;
			}
			break;
		case ButtonId.SKILL1:
			if (IsVirtualButtonReloading(VirtualButtonId.SKILL1, PlayerSkills[1]))
			{
				return;
			}
			break;
		}
		Transform transform = GetCurrentWeaponObj().ShootTransform[0];
		if (transform == null)
		{
			transform = base.transform.Find("aim");
			if (transform == null)
			{
				transform = base.transform;
				if (transform == null)
				{
					return;
				}
			}
		}
		lastCreateBulletTransform = transform;
		tRefPassiveskill.UseKeyTrigger(buttonId, GetCurrentWeaponObj().weaponStatus, ref selfBuffManager, CreateBulletByLastWSTranform);
	}

	public bool CheckUseSkillKeyTrigger(int id, bool checkActiveSkill = true)
	{
		if (id == 0 || id == 1)
		{
			CheckUseKeyTrigger((ButtonId)(8 + id));
			if (CanPlayerPressSkill(id, checkActiveSkill))
			{
				return true;
			}
		}
		return false;
	}

	public bool CheckUseSkillKeyTriggerEX(int id)
	{
		if (id == 0 || id == 1)
		{
			CheckUseKeyTrigger((ButtonId)(8 + id));
			MainStatus curMainStatus = CurMainStatus;
			if (curMainStatus == MainStatus.HURT)
			{
				return false;
			}
			return true;
		}
		return false;
	}

	public bool CheckUseSkillKeyTriggerEX2(int id)
	{
		if (id == 0 || id == 1)
		{
			CheckUseKeyTrigger((ButtonId)(8 + id), false);
			if (CanPlayerPressSkill(id))
			{
				return true;
			}
		}
		return false;
	}

	public bool CheckUseSkillKeyTrigger(int id, WeaponStruct weaponStruct, bool checkActiveSkill = true)
	{
		if (id == 0 || id == 1)
		{
			CheckUseKeyTrigger((ButtonId)(8 + id));
			if (CanPlayerPressSkill(weaponStruct, checkActiveSkill))
			{
				return true;
			}
		}
		return false;
	}

	public int GetJumpCount()
	{
		return _nJumpCount;
	}

	public void CheckUseJumpKeyTrigger()
	{
		if ((_mainStatus == MainStatus.JUMP || _mainStatus == MainStatus.FALL || _mainStatus == MainStatus.WALLKICK) && !_isHovering && _nTempDashChance > 0)
		{
			CheckUseKeyTrigger(ButtonId.JUMP);
		}
	}

	public void Check3rdJumpTrigger(bool checkSkillStatus)
	{
		_nJumpCount = 3;
		CheckUseKeyTrigger(ButtonId.JUMP, checkSkillStatus);
	}

	public void CheckPetDeactiveTrigger(int petId, int triggerType)
	{
		if (!IsLocalPlayer)
		{
			return;
		}
		Transform transform = GetCurrentWeaponObj().ShootTransform[0];
		if (transform == null)
		{
			transform = base.transform.Find("aim");
			if (transform == null)
			{
				transform = base.transform;
				if (transform == null)
				{
					return;
				}
			}
		}
		lastCreateBulletTransform = transform;
		tRefPassiveskill.PetDeactiveTrigger(petId, triggerType, GetCurrentWeaponObj().weaponStatus, ref selfBuffManager, CreateBulletByLastWSTranform);
	}

	public void CDSkill(int nskillid)
	{
		if (nskillid >= 0 && nskillid < 2)
		{
			PlayerSkills[nskillid].LastUseTimer += (float)PlayerSkills[nskillid].BulletData.n_FIRE_SPEED;
			PlayerSkills[nskillid].MagazineRemain = PlayerSkills[nskillid].BulletData.n_MAGAZINE;
		}
	}

	public void CDSkillEx(int nskillid, int ntime)
	{
		if (nskillid >= 0 && nskillid < 2)
		{
			PlayerSkills[nskillid].LastUseTimer += (float)ntime;
		}
	}

	public void AddMagazine(int nType, int nADD)
	{
		switch (nType)
		{
		case 1:
			PlayerSkills[0].MagazineRemain += nADD;
			if (PlayerSkills[0].MagazineRemain > (float)PlayerSkills[0].BulletData.n_MAGAZINE)
			{
				PlayerSkills[0].MagazineRemain = PlayerSkills[0].BulletData.n_MAGAZINE;
			}
			break;
		case 2:
			PlayerSkills[1].MagazineRemain += nADD;
			if (PlayerSkills[1].MagazineRemain > (float)PlayerSkills[1].BulletData.n_MAGAZINE)
			{
				PlayerSkills[1].MagazineRemain = PlayerSkills[1].BulletData.n_MAGAZINE;
			}
			break;
		case 4:
			PlayerWeapons[0].MagazineRemain += nADD;
			if (PlayerWeapons[0].MagazineRemain > (float)PlayerWeapons[0].BulletData.n_MAGAZINE)
			{
				PlayerWeapons[0].MagazineRemain = PlayerWeapons[0].BulletData.n_MAGAZINE;
			}
			break;
		case 8:
			PlayerWeapons[1].MagazineRemain += nADD;
			if (PlayerWeapons[1].MagazineRemain > (float)PlayerWeapons[1].BulletData.n_MAGAZINE)
			{
				PlayerWeapons[1].MagazineRemain = PlayerWeapons[1].BulletData.n_MAGAZINE;
			}
			break;
		}
	}

	public bool CanPlayerPressSkill(int id, bool checkActiveSkill = true)
	{
		MainStatus curMainStatus = CurMainStatus;
		if (curMainStatus == MainStatus.HURT)
		{
			return false;
		}
		if (PlayerSkills[id].LastUseTimer.GetMillisecond() < PlayerSkills[id].BulletData.n_FIRE_SPEED || PlayerSkills[id].MagazineRemain <= 0f || PlayerSkills[id].ForceLock)
		{
			return false;
		}
		if (checkActiveSkill && CurrentActiveSkill != -1)
		{
			return false;
		}
		return true;
	}

	public bool CanPlayerPressSkill(WeaponStruct weaponStruct, bool checkActiveSkill = true)
	{
		MainStatus curMainStatus = CurMainStatus;
		if (curMainStatus == MainStatus.HURT)
		{
			return false;
		}
		if (weaponStruct.LastUseTimer.GetMillisecond() < weaponStruct.BulletData.n_FIRE_SPEED || weaponStruct.MagazineRemain <= 0f || weaponStruct.ForceLock)
		{
			return false;
		}
		if (checkActiveSkill && CurrentActiveSkill != -1)
		{
			return false;
		}
		return true;
	}

	public void PlayerPressSkill(int id)
	{
		if (CanPlayerPressSkillFunc(id))
		{
			PreBelow = Controller.Collisions.below;
			PlayerPressSkillCharacterCallCB.CheckTargetToInvoke(id);
		}
	}

	protected void PlayerHeldSkill(int id)
	{
		MainStatus curMainStatus = CurMainStatus;
		if (curMainStatus != MainStatus.HURT)
		{
			WeaponStruct weaponStruct = PlayerSkills[id];
			if (PlayerSetting.AutoCharge == 0 && !weaponStruct.ForceLock && !weaponStruct.ChargeTimer.IsStarted() && weaponStruct.Reload_index == 0 && weaponStruct.FastBulletDatas[0].n_CHARGE_MAX_LEVEL != 0 && CheckUseSkillKeyTrigger(id))
			{
				PlayerSkills[id].ChargeTimer.TimerStart();
			}
		}
	}

	public void PlayerReleaseSkill(int id)
	{
		MainStatus curMainStatus = CurMainStatus;
		if (curMainStatus != MainStatus.HURT)
		{
			if (PlayerSetting.AutoCharge == 0)
			{
				_chargeShootObj.StopCharge(id);
			}
			if (PlayerSkills[id].LastUseTimer.GetMillisecond() >= PlayerSkills[id].BulletData.n_FIRE_SPEED && !(PlayerSkills[id].MagazineRemain <= 0f) && !PlayerSkills[id].ForceLock && CurrentActiveSkill == -1)
			{
				PlayerReleaseSkillCharacterCallCB.CheckTargetToInvoke(id);
				PreBelow = Controller.Collisions.below;
			}
		}
	}

	protected internal void PlayerShootBuster(WeaponStruct weaponStruct, bool isSkill, int id, sbyte lv = 0, Vector3? ShotDir = null, bool changeWeaponMesh = true, bool isWithLink = true)
	{
		MainStatus curMainStatus = CurMainStatus;
		if (curMainStatus == MainStatus.HURT)
		{
			return;
		}
		if (isSkill)
		{
			CurrentActiveSkill = id;
			if (changeWeaponMesh)
			{
				UpdateWeaponMesh(weaponStruct, GetCurrentWeaponObj());
			}
		}
		else
		{
			EnableWeaponMesh(weaponStruct);
			if (CurrentActiveSkill != -1 && GetCurrentSkillObj().WeaponData != null && (short)GetCurrentSkillObj().WeaponData.n_TYPE == 1)
			{
				DisableWeaponMesh(GetCurrentSkillObj());
				CurrentActiveSkill = -1;
			}
		}
		SKILL_TABLE sKILL_TABLE = weaponStruct.FastBulletDatas[lv];
		weaponStruct.MagazineRemain -= sKILL_TABLE.n_USE_COST;
		if (lv == 0 && !isSkill)
		{
			int n_TYPE = weaponStruct.WeaponData.n_TYPE;
			if (useWeaponSE)
			{
				useWeaponSE = false;
				if (IsLocalPlayer && (_rangeWpVoiceTimer.GetMillisecond() > 5000 || !_rangeWpVoiceTimer.IsStarted()))
				{
					PlayVoiceCB(Voice.ATTACK1);
					_rangeWpVoiceTimer.TimerReset();
					_rangeWpVoiceTimer.TimerStart();
				}
			}
		}
		if (weaponStruct.LastUseTimer.GetMillisecond() > weaponStruct.BulletData.n_FIRE_SPEED * 2 || weaponStruct.MagazineRemain <= 0f || !_isHoldShoot)
		{
			weaponStruct.LastUseTimer.TimerStart();
		}
		else
		{
			weaponStruct.LastUseTimer -= (float)weaponStruct.BulletData.n_FIRE_SPEED;
		}
		CreateBullet(weaponStruct, lv, 0, ShotDir, isWithLink);
		_isHoldShoot = true;
		if (weaponStruct.WeaponData != null && (short)weaponStruct.WeaponData.n_TYPE == 16)
		{
			CreateBullet(weaponStruct, lv, 1, ShotDir, isWithLink);
		}
	}

	protected void PlayerDoSlash(WeaponStruct weaponStruct, sbyte lv = 0)
	{
		MainStatus mainStatus = MainStatus.NONE;
		SubStatus subStatus = SubStatus.NONE;
		switch (CurMainStatus)
		{
		case MainStatus.WALK:
			switch (CurSubStatus)
			{
			case SubStatus.TELEPORT_POSE:
			case SubStatus.WIN_POSE:
				mainStatus = CurMainStatus;
				subStatus = SubStatus.IDLE;
				weaponStruct.BulletData = weaponStruct.FastBulletDatas[6];
				break;
			case SubStatus.DASH_END:
				mainStatus = CurMainStatus;
				subStatus = SubStatus.LAND;
				weaponStruct.BulletData = weaponStruct.FastBulletDatas[7];
				break;
			case SubStatus.CROUCH_UP:
				mainStatus = CurMainStatus;
				subStatus = SubStatus.IDLE;
				weaponStruct.BulletData = weaponStruct.FastBulletDatas[6];
				break;
			}
			break;
		case MainStatus.DASH:
		case MainStatus.AIRDASH:
			if (CurSubStatus == SubStatus.TELEPORT_POSE)
			{
				mainStatus = CurMainStatus;
				subStatus = SubStatus.WIN_POSE;
				weaponStruct.BulletData = weaponStruct.FastBulletDatas[8];
			}
			break;
		case MainStatus.IDLE:
			switch (CurSubStatus)
			{
			case SubStatus.IDLE:
			case SubStatus.LAND:
			case SubStatus.DASH_END:
			case SubStatus.CROUCH_UP:
			case SubStatus.SKILL_IDLE:
				mainStatus = MainStatus.SLASH;
				subStatus = SubStatus.TELEPORT_POSE;
				weaponStruct.BulletData = weaponStruct.FastBulletDatas[0];
				break;
			case SubStatus.SLASH1_END:
				mainStatus = MainStatus.SLASH;
				subStatus = SubStatus.WIN_POSE;
				weaponStruct.BulletData = weaponStruct.FastBulletDatas[1];
				break;
			case SubStatus.SLASH2_END:
				mainStatus = MainStatus.SLASH;
				subStatus = SubStatus.RIDE_ARMOR;
				weaponStruct.BulletData = weaponStruct.FastBulletDatas[2];
				break;
			case SubStatus.SLASH3_END:
				if (PlayerSetting.SlashClassic == 0)
				{
					mainStatus = MainStatus.SLASH;
					subStatus = SubStatus.IDLE;
					weaponStruct.BulletData = weaponStruct.FastBulletDatas[3];
				}
				break;
			case SubStatus.SLASH4_END:
				if (PlayerSetting.SlashClassic == 0)
				{
					mainStatus = MainStatus.SLASH;
					subStatus = SubStatus.LAND;
					weaponStruct.BulletData = weaponStruct.FastBulletDatas[4];
				}
				break;
			}
			break;
		case MainStatus.JUMP:
			if (CurSubStatus != SubStatus.RIDE_ARMOR)
			{
				jumpSlashCount = 0f;
				mainStatus = CurMainStatus;
				subStatus = SubStatus.RIDE_ARMOR;
				weaponStruct.BulletData = weaponStruct.FastBulletDatas[10];
			}
			break;
		case MainStatus.FALL:
			if (CurSubStatus != SubStatus.RIDE_ARMOR)
			{
				jumpSlashCount = 0f;
				mainStatus = CurMainStatus;
				subStatus = SubStatus.RIDE_ARMOR;
				weaponStruct.BulletData = weaponStruct.FastBulletDatas[10];
			}
			break;
		case MainStatus.WALLGRAB:
			jumpSlashCount = 0f;
			mainStatus = MainStatus.FALL;
			subStatus = SubStatus.RIDE_ARMOR;
			break;
		case MainStatus.SLASH:
			switch (CurSubStatus)
			{
			case SubStatus.TELEPORT_POSE:
				if (CurrentFrame > _slashTiming[(int)GetSlashType(CurMainStatus, CurSubStatus)])
				{
					mainStatus = CurMainStatus;
					subStatus = SubStatus.WIN_POSE;
					weaponStruct.BulletData = weaponStruct.FastBulletDatas[1];
				}
				break;
			case SubStatus.WIN_POSE:
				if (CurrentFrame > _slashTiming[(int)GetSlashType(CurMainStatus, CurSubStatus)])
				{
					mainStatus = CurMainStatus;
					subStatus = SubStatus.RIDE_ARMOR;
					weaponStruct.BulletData = weaponStruct.FastBulletDatas[2];
				}
				break;
			case SubStatus.RIDE_ARMOR:
				if (PlayerSetting.SlashClassic == 0 && CurrentFrame > _slashTiming[(int)GetSlashType(CurMainStatus, CurSubStatus)])
				{
					mainStatus = CurMainStatus;
					subStatus = SubStatus.IDLE;
					weaponStruct.BulletData = weaponStruct.FastBulletDatas[3];
				}
				break;
			case SubStatus.IDLE:
				if (PlayerSetting.SlashClassic == 0 && CurrentFrame > _slashTiming[(int)GetSlashType(CurMainStatus, CurSubStatus)])
				{
					mainStatus = CurMainStatus;
					subStatus = SubStatus.LAND;
					weaponStruct.BulletData = weaponStruct.FastBulletDatas[4];
				}
				break;
			}
			break;
		case MainStatus.CROUCH:
		{
			SubStatus curSubStatus = CurSubStatus;
			if (curSubStatus == SubStatus.WIN_POSE)
			{
				mainStatus = CurMainStatus;
				subStatus = SubStatus.RIDE_ARMOR;
			}
			break;
		}
		default:
			mainStatus = MainStatus.SLASH;
			subStatus = SubStatus.TELEPORT_POSE;
			break;
		case MainStatus.WALLKICK:
			break;
		}
		if (mainStatus != MainStatus.NONE && subStatus != SubStatus.NONE && !(weaponStruct.MagazineRemain <= 0f) && !weaponStruct.ForceLock && (OrangeConst.SABER_OVERHEAT_MODE != 1 || weaponStruct.FastBulletDatas[0].n_RELOAD != 0 || !(weaponStruct.MagazineRemain < (float)weaponStruct.BulletData.n_USE_COST)))
		{
			OrangeBattleUtility.UpdateSkillCD(weaponStruct);
			SetStatus(mainStatus, subStatus);
			SetMeleeStatus(weaponStruct);
		}
	}

	public virtual void PlayerPressSelect()
	{
		if ((_weaponSwitchTimer.IsStarted() && _weaponSwitchTimer.GetMillisecond() < OrangeConst.WEAPON_SWITCH_CD) || CurrentActiveSkill != -1)
		{
			return;
		}
		if ((bool)GetCurrentWeaponObj().MeleeBullet)
		{
			DeActivateMeleeAttack(GetCurrentWeaponObj());
			_listSlashCache.Clear();
		}
		int weaponCurrent = WeaponCurrent;
		int num = WeaponCurrent + 1;
		if (num < PlayerWeapons.Length)
		{
			try
			{
				if (PlayerWeapons[num].WeaponData.n_ID != 0)
				{
					WeaponCurrent++;
				}
			}
			catch (Exception message)
			{
				Debug.Log(message);
			}
		}
		else
		{
			WeaponCurrent = 0;
		}
		if (WeaponCurrent == 0)
		{
			_weaponSwitchTimer.TimerStart();
		}
		if (tRefPassiveskill.bUsePassiveskill)
		{
			tRefPassiveskill.bUsePassiveskill = false;
			if (PlayerWeapons[weaponCurrent].ChipEfx != null)
			{
				PlayerWeapons[weaponCurrent].ChipEfx.ActiveChipSkill(tRefPassiveskill.bUsePassiveskill, false);
			}
		}
		if (IsLocalPlayer)
		{
			PlaySE("BattleSE", 9);
		}
		MainStatus curMainStatus = CurMainStatus;
		if (curMainStatus == MainStatus.SKILL)
		{
			return;
		}
		IsShoot = 0;
		_isHoldShoot = false;
		UpdateAimRender();
		_shootTimer.TimerStop();
		if (Controller.Collisions.below && _velocity.y <= 0)
		{
			Dashing = false;
			if (CurMainStatus == MainStatus.DASH || CurMainStatus == MainStatus.AIRDASH)
			{
				PlayerStopDashing();
			}
			else
			{
				SetHorizontalSpeed(0);
			}
			SetStatus(MainStatus.IDLE, SubStatus.IDLE);
			ResetDashChance();
			_nJumpCount = 0;
		}
		else
		{
			Controller.UpdateRaycastOrigins();
			if ((bool)Solid_meeting(0f, -2f) && !Solid_meeting(0f, 0f) && (CurMainStatus == MainStatus.DASH || CurMainStatus == MainStatus.AIRDASH))
			{
				Dashing = false;
				PlayerStopDashing();
			}
			SetStatus(MainStatus.FALL, SubStatus.TELEPORT_POSE);
		}
		if (GetCurrentWeaponObj().ChipEfx != null && GetCurrentWeaponObj().chip_switch)
		{
			tRefPassiveskill.bUsePassiveskill = true;
			if (!base.IsHidden)
			{
				GetCurrentWeaponObj().ChipEfx.ActiveChipSkill(tRefPassiveskill.bUsePassiveskill, false);
			}
		}
		UpdateWeaponMesh(GetCurrentWeaponObj(), PlayerWeapons[weaponCurrent]);
		UpdateAimRangeByWeaponEvt(GetCurrentWeaponObj());
		for (int i = 0; i < PlayerSkills.Length; i++)
		{
			PlayerSkills[i].weaponStatus.CopyWeaponStatus(GetCurrentWeaponObj().weaponStatus, 1 << i);
		}
		useWeaponSE = true;
	}

	protected void UpdateAimRender(WeaponStruct weaponStruct = null)
	{
		if (PlayerSetting.AimLine == 0 || PlayerSetting.AimManual == 0)
		{
			_lineRenderer.enabled = false;
			return;
		}
		_lineRenderer.enabled = weaponStruct != null;
		if (IsTeleporting || bLockInputCtrl || IsDead())
		{
			_lineRenderer.enabled = false;
		}
		if (weaponStruct != null)
		{
			Transform transform = ((weaponStruct.ShootTransform[0] != null) ? weaponStruct.ShootTransform[0] : base.AimTransform);
			_lineRenderer.SetPosition(0, (IsShoot != 0) ? transform.position : base.AimTransform.position);
			_lineRenderer.SetPosition(1, transform.position + ShootDirection * 10f);
			_lineRenderer.endColor = Color.green;
		}
	}

	public float GetCurrentAimRange()
	{
		return GetCurrentWeaponObj().BulletData.f_DISTANCE;
	}

	protected void UpdateGravity()
	{
		bool flag = true;
		if (_velocity.y < 0)
		{
			if (!Controller.Collisions.below)
			{
				fIceSlide = 0f;
				switch (CurMainStatus)
				{
				case MainStatus.JUMP:
					if (Controller.CollisionsOld.below)
					{
						if (Dashing && _dashChance > 0)
						{
							_dashChance--;
						}
						SetHorizontalSpeed(0);
					}
					switch (CurSubStatus)
					{
					default:
						SetStatus(MainStatus.FALL, SubStatus.TELEPORT_POSE);
						break;
					case SubStatus.RIDE_ARMOR:
						SetStatus(MainStatus.FALL, SubStatus.RIDE_ARMOR);
						break;
					case SubStatus.IDLE:
						break;
					}
					break;
				case MainStatus.IDLE:
				case MainStatus.WALK:
				case MainStatus.DASH:
					if (!Controller.BelowInBypassRange)
					{
						SetStatus(MainStatus.FALL, SubStatus.TELEPORT_POSE);
						if (Dashing && _dashChance > 0)
						{
							Dashing = false;
							_dashChance--;
						}
						SetHorizontalSpeed(0);
					}
					break;
				}
			}
			else
			{
				switch (CurMainStatus)
				{
				case MainStatus.IDLE:
				{
					if (!Controller.BelowInBypassRange)
					{
						break;
					}
					StageSceneObjParam componentInParent = Controller.BelowInBypassRange.transform.GetComponentInParent<StageSceneObjParam>();
					if (componentInParent != null && componentInParent.IsIceBlock())
					{
						if (Vector2.Angle(Controller.BelowInBypassRange.normal, Vector2.up) > 10f)
						{
							bIceSlide = false;
							Vector2 zero = Vector2.zero;
							zero.y = _velocity.vec3.y;
							Vector2 vector = Controller.BelowInBypassRange.normal * Vector2.Dot(zero, Controller.BelowInBypassRange.normal);
							fIceSlide += (zero - vector).x;
							_velocityExtra = new VInt3(new Vector3(fIceSlide, 0f, 0f));
						}
						else if (fIceSlide != 0f)
						{
							fIceSlideParam = componentInParent.fIceSliderParam;
							bIceSlide = true;
						}
					}
					break;
				}
				case MainStatus.WALK:
				case MainStatus.DASH:
				{
					if (!Controller.BelowInBypassRange)
					{
						break;
					}
					StageSceneObjParam componentInParent2 = Controller.BelowInBypassRange.transform.GetComponentInParent<StageSceneObjParam>();
					if (!(componentInParent2 != null) || !componentInParent2.IsIceBlock())
					{
						break;
					}
					if (Vector2.Angle(Controller.BelowInBypassRange.normal, Vector2.up) > 10f)
					{
						bIceSlide = false;
						Vector2 zero2 = Vector2.zero;
						zero2.y = _velocity.vec3.y;
						Vector2 vector2 = Controller.BelowInBypassRange.normal * Vector2.Dot(zero2, Controller.BelowInBypassRange.normal);
						zero2 -= vector2;
						Vector3 vec = _velocity.vec3;
						if (Mathf.Sign(zero2.x) != Mathf.Sign(vec.x))
						{
							bIceSlideSameDir = false;
							if (zero2.x > 0f)
							{
								fIceSlide = 0f - vec.x + 1f;
							}
							else
							{
								fIceSlide = 0f - vec.x - 1f;
							}
						}
						else
						{
							if (!bIceSlideSameDir)
							{
								bIceSlideSameDir = true;
								fIceSlide = 0f;
							}
							fIceSlide += zero2.x;
						}
						_velocityExtra = new VInt3(new Vector3(fIceSlide, 0f, 0f));
					}
					else if (fIceSlide != 0f)
					{
						fIceSlideParam = componentInParent2.fIceSliderParam;
						bIceSlide = true;
					}
					break;
				}
				case MainStatus.TELEPORT_IN:
					fIceSlide = 0f;
					break;
				case MainStatus.HURT:
				case MainStatus.WALLKICK:
					fIceSlide = 0f;
					break;
				case MainStatus.SKILL:
					fIceSlide = 0f;
					PlayerSkillLandCB.CheckTargetToInvoke();
					break;
				case MainStatus.JUMP:
				case MainStatus.FALL:
				case MainStatus.WALLGRAB:
					fIceSlide = 0f;
					if (_velocity.y < 0 && !Controller.JumpThrough)
					{
						ResetDashChance();
						_nJumpCount = 0;
						Dashing = false;
						PlayerStopDashing();
						PlayerStopHovering();
						if (!ManagedSingleton<InputStorage>.Instance.IsHeld(UserID, ButtonId.LEFT) && !ManagedSingleton<InputStorage>.Instance.IsHeld(UserID, ButtonId.RIGHT))
						{
							SetHorizontalSpeed(0);
						}
						PlayCharaSeCB(CharaSE.CHAKUCHI);
						if (!base.IsHidden)
						{
							MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(LandFx, _transform.position, NormalQuaternion, Array.Empty<object>());
						}
						SetStatus(MainStatus.IDLE, SubStatus.LAND);
					}
					break;
				default:
					fIceSlide = 0f;
					if (Controller.CollisionsOld.below)
					{
						break;
					}
					Debug.Log("特例著地 - " + CurMainStatus);
					ResetDashChance();
					_nJumpCount = 0;
					Dashing = false;
					PlayerStopDashing();
					PlayerStopHovering(false);
					if (!ManagedSingleton<InputStorage>.Instance.IsHeld(UserID, ButtonId.LEFT) && !ManagedSingleton<InputStorage>.Instance.IsHeld(UserID, ButtonId.RIGHT))
					{
						SetHorizontalSpeed(0);
					}
					if (CurMainStatus != MainStatus.CROUCH)
					{
						PlayCharaSeCB(CharaSE.CHAKUCHI);
						if (!base.IsHidden)
						{
							MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(LandFx, _transform.position, NormalQuaternion, Array.Empty<object>());
						}
					}
					if (ManagedSingleton<InputStorage>.Instance.IsHeld(UserID, ButtonId.DOWN))
					{
						SetStatus(MainStatus.CROUCH, SubStatus.WIN_POSE);
					}
					else
					{
						SetStatus(MainStatus.IDLE, SubStatus.LAND);
					}
					break;
				}
				SetVerticalSpeed(0);
			}
		}
		else if (_velocity.y > 0)
		{
			fIceSlide = 0f;
			if (_mainStatus == MainStatus.IDLE)
			{
				SetStatus(MainStatus.FALL, SubStatus.TELEPORT_POSE);
			}
			if (Controller.Collisions.above)
			{
				SetVerticalSpeed(0);
			}
		}
		int num = OrangeBattleUtility.FP_MaxGravity.i;
		MainStatus curMainStatus = CurMainStatus;
		if (curMainStatus == MainStatus.WALLGRAB)
		{
			num = WallSlideGravity;
		}
		switch (CurMainStatus)
		{
		case MainStatus.SKILL:
		{
			SubStatus curSubStatus = CurSubStatus;
			if (curSubStatus == SubStatus.IDLE || curSubStatus == SubStatus.SKILL_IDLE || curSubStatus == SubStatus.VAVA_KNEE_AIR)
			{
				flag = false;
			}
			break;
		}
		case MainStatus.AIRDASH:
			flag = false;
			break;
		case MainStatus.HURT:
			flag = Controller.BelowInBypassRange;
			break;
		case MainStatus.JUMP:
		{
			SubStatus curSubStatus = CurSubStatus;
			if (curSubStatus == SubStatus.IDLE)
			{
				if (ManagedSingleton<InputStorage>.Instance.IsHeld(UserID, ButtonId.UP) || ManagedSingleton<InputStorage>.Instance.IsHeld(UserID, ButtonId.DOWN) || ManagedSingleton<InputStorage>.Instance.IsHeld(UserID, ButtonId.LEFT) || ManagedSingleton<InputStorage>.Instance.IsHeld(UserID, ButtonId.RIGHT))
				{
					_bFloatingFlag = false;
				}
				else if (!_bFloatingFlag && _velocity.y == 0)
				{
					_bFloatingFlag = true;
					_globalWaypoints[0] = base.transform.localPosition.y + 0.2f;
					_globalWaypoints[1] = base.transform.localPosition.y - 0.2f;
				}
			}
			break;
		}
		}
		if (!flag || (int)Hp <= 0 || IgnoreGravity)
		{
			if (!flag || (int)Hp <= 0)
			{
				SetVerticalSpeed(0);
			}
			else if (_bFloatingFlag)
			{
				_velocity.y = Mathf.RoundToInt(CalculateVerticalMovement() * OrangeBattleUtility.PPU * OrangeBattleUtility.FPS * 1000f);
			}
		}
		else
		{
			VInt vInt = OrangeBattleUtility.FP_Gravity * GameLogicUpdateManager.g_fixFrameLenFP / 1000;
			_velocity.x += _velocityForceField.x;
			_velocity.y += vInt * GravityMultiplier / 1000 + _velocityForceField.y + OrangeBattleUtility.GlobalGravityExtra.y;
			_velocity.y = IntMath.Sign(_velocity.y) * IntMath.Min(IntMath.Abs(_velocity.y), IntMath.Abs(num * GravityMultiplier / 1000));
		}
		if (bQuickSand)
		{
			if (_velocity.y < 0)
			{
				_velocity.y = (int)((float)_velocity.y * fQuickSand);
			}
			else
			{
				_velocity.y = (int)((float)_velocity.y * fQuickSand);
			}
			_velocity.x = (int)((float)_velocity.x * 0.1f);
			ResetDashChance();
		}
	}

	public void AnimationEnd()
	{
		if (CurrentFrame < 1f || LastSetAnimateFrame > Animator.LastUpdateFrame)
		{
			return;
		}
		MainStatus curMainStatus = CurMainStatus;
		SubStatus curSubStatus = CurSubStatus;
		switch (CurMainStatus)
		{
		case MainStatus.TELEPORT_OUT:
			switch (CurSubStatus)
			{
			case SubStatus.WIN_POSE:
				switch (_freshWinPose)
				{
				case 1:
					_freshWinPose = 0;
					break;
				case 0:
					if (_refWinPoseEndCB != null)
					{
						_refWinPoseEndCB();
						_refWinPoseEndCB = null;
					}
					_freshWinPose = -1;
					break;
				}
				break;
			case SubStatus.TELEPORT_POSE:
				if (_freshWinPose != 0)
				{
					_freshWinPose = 0;
				}
				break;
			}
			break;
		case MainStatus.TELEPORT_IN:
			if (CurSubStatus == SubStatus.TELEPORT_POSE)
			{
				if (CharacterData.n_WEAPON_IN != -1)
				{
					DisableSkillWeapon(CharacterData.n_WEAPON_IN);
				}
				EnableCurrentWeapon();
				Controller.Collider2D.enabled = true;
				TeleportInCharacterDependeEndEvt.CheckTargetToInvoke();
				SetStatus(MainStatus.IDLE, SubStatus.IDLE);
				LockInput = false;
				if (BattleInfoUI.Instance != null)
				{
					BattleInfoUI.Instance.SwitchOptionBtn(true, 2);
				}
			}
			break;
		case MainStatus.IDLE:
		{
			SubStatus curSubStatus2 = CurSubStatus;
			if ((uint)(curSubStatus2 - 4) <= 3u || (uint)(curSubStatus2 - 37) <= 4u)
			{
				SetStatus(MainStatus.IDLE, SubStatus.IDLE);
			}
			break;
		}
		case MainStatus.CROUCH:
			switch (CurSubStatus)
			{
			case SubStatus.TELEPORT_POSE:
				SetStatus(MainStatus.CROUCH, SubStatus.WIN_POSE);
				break;
			case SubStatus.RIDE_ARMOR:
				SetStatus(MainStatus.CROUCH, SubStatus.IDLE);
				break;
			case SubStatus.IDLE:
				SetStatus(MainStatus.CROUCH, SubStatus.WIN_POSE);
				break;
			}
			break;
		case MainStatus.JUMP:
		{
			SubStatus curSubStatus2 = CurSubStatus;
			if (curSubStatus2 == SubStatus.RIDE_ARMOR && CurrentFrame - jumpSlashCount >= 1f)
			{
				if (ManagedSingleton<InputStorage>.Instance.IsHeld(UserID, ButtonId.SHOOT) && GetCurrentWeaponObj().MagazineRemain >= (float)GetCurrentWeaponObj().FastBulletDatas[10].n_USE_COST)
				{
					jumpSlashCount = Mathf.Floor(CurrentFrame);
					GetCurrentWeaponObj().MagazineRemain -= GetCurrentWeaponObj().FastBulletDatas[10].n_USE_COST;
				}
				else
				{
					SetStatus(MainStatus.FALL, SubStatus.TELEPORT_POSE);
				}
			}
			break;
		}
		case MainStatus.FALL:
			switch (CurSubStatus)
			{
			case SubStatus.RIDE_ARMOR:
				if (CurrentFrame - jumpSlashCount >= 1f)
				{
					if (ManagedSingleton<InputStorage>.Instance.IsHeld(UserID, ButtonId.SHOOT) && GetCurrentWeaponObj().MagazineRemain >= (float)GetCurrentWeaponObj().FastBulletDatas[10].n_USE_COST)
					{
						jumpSlashCount = Mathf.Floor(CurrentFrame);
						GetCurrentWeaponObj().MagazineRemain -= GetCurrentWeaponObj().FastBulletDatas[10].n_USE_COST;
					}
					else
					{
						SetStatus(MainStatus.FALL, SubStatus.TELEPORT_POSE);
					}
				}
				break;
			case SubStatus.WIN_POSE:
				SetStatus(MainStatus.FALL, SubStatus.TELEPORT_POSE);
				break;
			}
			break;
		case MainStatus.WALLGRAB:
			switch (CurSubStatus)
			{
			case SubStatus.TELEPORT_POSE:
				SetStatus(MainStatus.WALLGRAB, SubStatus.WIN_POSE);
				break;
			case SubStatus.WIN_POSE:
				SetStatus(MainStatus.WALLGRAB, SubStatus.RIDE_ARMOR);
				break;
			case SubStatus.IDLE:
				SetStatus(MainStatus.WALLGRAB, SubStatus.LAND);
				break;
			case SubStatus.LAND:
				SetStatus(MainStatus.WALLGRAB, SubStatus.RIDE_ARMOR);
				break;
			}
			break;
		case MainStatus.WALK:
			switch (CurSubStatus)
			{
			case SubStatus.TELEPORT_POSE:
				SetStatus(MainStatus.WALK, SubStatus.WIN_POSE);
				break;
			case SubStatus.IDLE:
				SetStatus(MainStatus.WALK, SubStatus.DASH_END);
				break;
			case SubStatus.LAND:
				SetStatus(MainStatus.WALK, SubStatus.CROUCH_UP);
				break;
			case SubStatus.DASH_END:
			case SubStatus.CROUCH_UP:
				SetStatus(MainStatus.WALK, SubStatus.WIN_POSE);
				break;
			}
			break;
		case MainStatus.DASH:
		case MainStatus.AIRDASH:
			switch (CurSubStatus)
			{
			case SubStatus.TELEPORT_POSE:
				if (CharacterDashType == DASH_TYPE.X_SERIES && !LThrusterParticleSystem.isPlaying)
				{
					LThrusterParticleSystem.Play();
					RThrusterParticleSystem.Play();
				}
				break;
			case SubStatus.RIDE_ARMOR:
			case SubStatus.LAND:
				SetStatus(CurMainStatus, SubStatus.TELEPORT_POSE);
				break;
			case SubStatus.WIN_POSE:
				SetStatus(CurMainStatus, SubStatus.RIDE_ARMOR);
				break;
			case SubStatus.IDLE:
				SetStatus(CurMainStatus, SubStatus.LAND);
				break;
			}
			break;
		case MainStatus.SLASH:
			switch (CurSubStatus)
			{
			case SubStatus.TELEPORT_POSE:
				SetStatus(MainStatus.IDLE, SubStatus.SLASH1_END);
				break;
			case SubStatus.WIN_POSE:
				SetStatus(MainStatus.IDLE, SubStatus.SLASH2_END);
				break;
			case SubStatus.RIDE_ARMOR:
				SetStatus(MainStatus.IDLE, SubStatus.SLASH3_END);
				break;
			case SubStatus.IDLE:
				SetStatus(MainStatus.IDLE, SubStatus.SLASH4_END);
				break;
			case SubStatus.LAND:
				SetStatus(MainStatus.IDLE, SubStatus.SLASH5_END);
				break;
			}
			break;
		case MainStatus.SKILL:
			switch (CurSubStatus)
			{
			case SubStatus.TELEPORT_POSE:
				SkillEnd = true;
				if (Controller.Collisions.below)
				{
					Dashing = false;
					SetStatus(MainStatus.IDLE, SubStatus.IDLE);
				}
				else
				{
					SetStatus(MainStatus.FALL, SubStatus.TELEPORT_POSE);
				}
				break;
			case SubStatus.VAVA_CANNON_GROUND:
			case SubStatus.VAVA_CANNON_AIR:
			case SubStatus.VAVA_KNEE_GROUND:
			case SubStatus.VAVA_KNEE_AIR:
				IgnoreGravity = false;
				SkillEnd = true;
				if (Controller.Collisions.below)
				{
					Dashing = false;
					SetStatus(MainStatus.IDLE, SubStatus.IDLE);
				}
				else
				{
					SetStatus(MainStatus.FALL, SubStatus.TELEPORT_POSE);
				}
				break;
			}
			break;
		case MainStatus.HURT:
		{
			SubStatus curSubStatus2 = CurSubStatus;
			if (curSubStatus2 == SubStatus.LAND)
			{
				SetStatus(CurMainStatus, SubStatus.DASH_END);
			}
			break;
		}
		case MainStatus.GIGA_ATTACK:
			switch (CurSubStatus)
			{
			case SubStatus.GIGA_ATTACK_START:
				if (CurrentFrame > 2f)
				{
					SetStatus(CurMainStatus, SubStatus.GIGA_ATTACK_END);
				}
				break;
			case SubStatus.GIGA_ATTACK_END:
				IgnoreGravity = false;
				CurrentActiveSkill = -1;
				if (_usingVehicle)
				{
					Debug.Log("[SetStatus]:GIGA_ATTACK -> RIDE_ARMOR");
					SetStatus(MainStatus.RIDE_ARMOR, SubStatus.RIDE_ARMOR);
				}
				else if ((bool)Controller.BelowInBypassRange)
				{
					Dashing = false;
					ResetDashChance();
					if (_animateID == HumanBase.AnimateId.ANI_GIGA_CROUCH_END && ManagedSingleton<InputStorage>.Instance.IsHeld(UserID, ButtonId.DOWN))
					{
						SetStatus(MainStatus.CROUCH, SubStatus.WIN_POSE);
					}
					else
					{
						SetStatus(MainStatus.IDLE, SubStatus.IDLE);
					}
				}
				else
				{
					SetStatus(MainStatus.FALL, SubStatus.TELEPORT_POSE);
				}
				EnableCurrentWeapon();
				break;
			}
			break;
		}
		AnimationEndCharacterDependEvt(curMainStatus, curSubStatus);
	}

	protected internal void ActivateMeleeAttack(WeaponStruct targetWeapon, SlashType slashType = SlashType.None)
	{
		bool flag = slashType != SlashType.None;
		if (flag)
		{
			if ((bool)targetWeapon.SlashObject && (bool)targetWeapon.SlashEfxCmp)
			{
				if (base._characterDirection == CharacterDirection.LEFT)
				{
					targetWeapon.SlashEfxCmp.ActivateMeleeEffect(flag, slashType, ReversedQuaternion, true);
				}
				else
				{
					targetWeapon.SlashEfxCmp.ActivateMeleeEffect(flag, slashType, NormalQuaternion, false);
				}
			}
			SlashDetails item = new SlashDetails
			{
				MeleeBullet = targetWeapon.MeleeBullet,
				Info = targetWeapon.MeleeBullet.GetSlashCollider((int)slashType),
				SlashType = slashType,
				TargetWeaponStruct = targetWeapon
			};
			_listSlashCache.Add(item);
			targetWeapon.MeleeBullet.SetDestroy(_mainStatus, _subStatus);
			targetWeapon.MeleeBullet.UpdateCollider((int)slashType, base._characterDirection == CharacterDirection.LEFT);
			targetWeapon.MeleeBullet.SetBulletAtk(targetWeapon.weaponStatus, selfBuffManager.sBuffStatus);
			targetWeapon.MeleeBullet.BulletLevel = targetWeapon.SkillLV;
			if (slashType == SlashType.Skill)
			{
				targetWeapon.MeleeBullet.UpdateBulletData(targetWeapon.FastBulletDatas[0], sPlayerName);
			}
			else
			{
				targetWeapon.MeleeBullet.UpdateBulletData(targetWeapon.FastBulletDatas[(int)slashType], sPlayerName);
			}
			if (slashType == SlashType.StandSlash5)
			{
				targetWeapon.MeleeBullet.isUseHitStop2Self = true;
			}
			_listSlashCache[0].MeleeBullet.PlayUseSE(true);
		}
		else
		{
			DeActivateMeleeAttack(targetWeapon);
		}
	}

	protected internal void DeActivateMeleeAttack(WeaponStruct targetWeapon)
	{
		if ((bool)targetWeapon.SlashObject && (bool)targetWeapon.SlashEfxCmp)
		{
			targetWeapon.SlashEfxCmp.DeActivateMeleeEffect();
		}
		if (targetWeapon.MeleeBullet.IsActivate)
		{
			targetWeapon.MeleeBullet.SetDestroy(_mainStatus, _subStatus);
			targetWeapon.MeleeBullet.isUseHitStop2Self = false;
		}
	}

	protected internal void DisableWeaponMesh(WeaponStruct weapon, float overrideDissolveTime = -1f)
	{
		lastWeaponStruct = null;
		if (weapon.GatlingSpinner != null)
		{
			weapon.GatlingSpinner.Activate = false;
		}
		if (weapon.WeaponData == null)
		{
			return;
		}
		switch ((WeaponType)(short)weapon.WeaponData.n_TYPE)
		{
		case WeaponType.Buster:
		case WeaponType.Spray:
		case WeaponType.SprayHeavy:
			EnableHandMesh(true);
			weapon.WeaponMesh[0].Disappear(null, overrideDissolveTime);
			break;
		case WeaponType.MGun:
		case WeaponType.Gatling:
		case WeaponType.Launcher:
			weapon.WeaponMesh[0].Disappear(null, overrideDissolveTime);
			break;
		case WeaponType.Melee:
			weapon.WeaponMesh[0].Disappear(null, overrideDissolveTime);
			DeActivateMeleeAttack(weapon);
			break;
		case WeaponType.DualGun:
			weapon.WeaponMesh[0].Disappear(null, overrideDissolveTime);
			if ((bool)weapon.WeaponMesh[1])
			{
				weapon.WeaponMesh[1].Disappear(null, overrideDissolveTime);
			}
			break;
		}
		if (weapon.ChipEfx != null)
		{
			weapon.ChipEfx.CloseWeaponEffectOnly();
		}
	}

	public bool GetHandIsNeededEnable()
	{
		WeaponStruct currentWeaponObj = GetCurrentWeaponObj();
		if (currentWeaponObj.WeaponData != null)
		{
			WeaponType weaponType = (WeaponType)currentWeaponObj.WeaponData.n_TYPE;
			if ((uint)(weaponType - 1) > 1u && weaponType != WeaponType.SprayHeavy)
			{
				return true;
			}
			return false;
		}
		return true;
	}

	protected internal void EnableWeaponMesh(WeaponStruct weapon, Callback pCB = null, float overrideDissolveTime = -1f)
	{
		if (weapon.WeaponData == null)
		{
			if (pCB != null)
			{
				pCB();
			}
			return;
		}
		if (lastWeaponStruct == weapon)
		{
			if (pCB != null)
			{
				pCB();
			}
			return;
		}
		Animator.SetAnimatorEquip(weapon.WeaponData.n_TYPE);
		if (CheckActStatusEvt(15, -1))
		{
			if (pCB != null)
			{
				pCB();
			}
			return;
		}
		lastWeaponStruct = weapon;
		switch ((WeaponType)(short)weapon.WeaponData.n_TYPE)
		{
		case WeaponType.Buster:
		case WeaponType.Spray:
		case WeaponType.SprayHeavy:
			if (!(weapon.WeaponMesh[0] != null))
			{
				break;
			}
			EnableHandMesh(false);
			weapon.WeaponMesh[0].Appear(delegate
			{
				if (pCB != null)
				{
					pCB();
				}
			}, overrideDissolveTime);
			break;
		case WeaponType.Melee:
		case WeaponType.MGun:
		case WeaponType.Gatling:
		case WeaponType.Launcher:
			EnableHandMesh(true);
			weapon.WeaponMesh[0].Appear(delegate
			{
				if (pCB != null)
				{
					pCB();
				}
			}, overrideDissolveTime);
			break;
		case WeaponType.DualGun:
			EnableHandMesh(true);
			weapon.WeaponMesh[0].Appear(delegate
			{
				if (pCB != null)
				{
					pCB();
				}
			}, overrideDissolveTime);
			if ((bool)weapon.WeaponMesh[1])
			{
				weapon.WeaponMesh[1].Appear(null, overrideDissolveTime);
			}
			break;
		default:
			if (pCB != null)
			{
				pCB();
			}
			break;
		}
		if (weapon.ChipEfx != null)
		{
			weapon.ChipEfx.ActiveChipSkill(tRefPassiveskill.bUsePassiveskill);
		}
	}

	public void EnableHandMesh(bool enable)
	{
		SkinnedMeshRenderer[] handMesh = _handMesh;
		for (int i = 0; i < handMesh.Length; i++)
		{
			handMesh[i].enabled = enable;
		}
	}

	public bool GetHandMeshEnableStatus()
	{
		SkinnedMeshRenderer[] handMesh = _handMesh;
		int num = 0;
		if (num < handMesh.Length)
		{
			return handMesh[num].enabled;
		}
		return false;
	}

	protected internal void UpdateWeaponMesh(WeaponStruct enableWeapon, WeaponStruct disableWeapon, float overrideDissolveTime = -1f)
	{
		DisableWeaponMesh(disableWeapon, overrideDissolveTime);
		EnableWeaponMesh(enableWeapon, null, overrideDissolveTime);
	}

	public static void UpdatePlayerParameters()
	{
		WalkSpeed = Mathf.RoundToInt(OrangeBattleUtility.PlayerWalkSpeed * OrangeBattleUtility.PPU * OrangeBattleUtility.FPS * 1000f);
		JumpSpeed = Mathf.RoundToInt(OrangeBattleUtility.PlayerJumpSpeed * OrangeBattleUtility.PPU * OrangeBattleUtility.FPS * 1000f);
		DashSpeed = Mathf.RoundToInt(OrangeBattleUtility.PlayerDashSpeed * OrangeBattleUtility.PPU * OrangeBattleUtility.FPS * 1000f);
	}

	protected void OnDestroy()
	{
		ManagedSingleton<InputStorage>.Instance.RemoveInputData(UserID);
		SetActiveFalse();
		if (PlayerWeapons == null)
		{
			return;
		}
		WeaponStruct[] playerWeapons = PlayerWeapons;
		foreach (WeaponStruct weaponStruct in playerWeapons)
		{
			if ((bool)weaponStruct.MeleeBullet)
			{
				UnityEngine.Object.Destroy(weaponStruct.MeleeBullet.gameObject);
			}
		}
	}

	public virtual void SetActiveFalse()
	{
		MonoBehaviourSingleton<UpdateManager>.Instance.RemoveUpdate(this);
		MonoBehaviourSingleton<UpdateManager>.Instance.RemoveLateUpdate(this);
		MonoBehaviourSingleton<GameLogicUpdateManager>.Instance.RemoveUpdate(this);
		Singleton<GenericEventManager>.Instance.DetachEvent<GameObject, StageCtrlInsTruction>(EventManager.ID.STAGE_OBJ_CTRL, ObjCtrl);
		Singleton<GenericEventManager>.Instance.DetachEvent<string, bool, float, float, bool?>(EventManager.ID.STAGE_CONTINUE_PLATER, ContinueCall);
		base.gameObject.SetActive(false);
	}

	protected SlashType GetSlashType(MainStatus mainStatus, SubStatus subStatus)
	{
		switch (mainStatus)
		{
		case MainStatus.CROUCH:
			if (subStatus == SubStatus.RIDE_ARMOR)
			{
				return SlashType.CrouchSlash1;
			}
			break;
		case MainStatus.WALK:
			switch (subStatus)
			{
			case SubStatus.IDLE:
				return SlashType.WalkSlash1;
			case SubStatus.LAND:
				return SlashType.WalkSlash2;
			}
			break;
		case MainStatus.DASH:
		case MainStatus.AIRDASH:
			if (subStatus == SubStatus.WIN_POSE)
			{
				return SlashType.DashSlash1;
			}
			break;
		case MainStatus.WALLGRAB:
			if (subStatus == SubStatus.IDLE)
			{
				return SlashType.WallGrabSlash;
			}
			break;
		case MainStatus.SLASH:
			switch (subStatus)
			{
			case SubStatus.TELEPORT_POSE:
				return SlashType.StandSlash1;
			case SubStatus.WIN_POSE:
				return SlashType.StandSlash2;
			case SubStatus.RIDE_ARMOR:
				return SlashType.StandSlash3;
			case SubStatus.IDLE:
				return SlashType.StandSlash4;
			case SubStatus.LAND:
				return SlashType.StandSlash5;
			}
			break;
		case MainStatus.JUMP:
			if (subStatus == SubStatus.RIDE_ARMOR)
			{
				return SlashType.JumpSlash1;
			}
			break;
		case MainStatus.FALL:
			if (subStatus == SubStatus.RIDE_ARMOR)
			{
				return SlashType.JumpSlash1;
			}
			break;
		case MainStatus.SKILL:
			if ((uint)(subStatus - 17) <= 1u)
			{
				return SlashType.Skill;
			}
			return SlashType.None;
		}
		return SlashType.None;
	}

	public void AddForce(VInt3 pForce)
	{
		_velocityExtra += pForce;
	}

	public void AddShift(VInt3 pForce)
	{
		_velocityShift += pForce;
	}

	public void AddForceFieldProxy(VInt3 pForce)
	{
		AddForceFieldCB.CheckTargetToInvoke(pForce);
	}

	public void AddForceField(VInt3 pForce)
	{
		_velocityForceField += pForce;
	}

	public void LockControl()
	{
		StopPlayer();
		LockInput = true;
	}

	public void StopPlayer()
	{
		if (CurMainStatus != MainStatus.TELEPORT_IN && CurMainStatus != MainStatus.RIDE_ARMOR)
		{
			InitImportantVar();
			PlayerReleaseLeftCB();
			PlayerReleaseRightCB();
			PlayerReleaseDashCB();
			PlayerReleaseDown();
			PlayerStopHovering(false);
			_velocityForceField = VInt3.zero;
			if (CurMainStatus != MainStatus.HURT)
			{
				SetStatus(MainStatus.IDLE, SubStatus.IDLE);
			}
		}
		if (refRideBaseObj != null)
		{
			refRideBaseObj.StopRideObj();
		}
		Animator.LastUpdateFrame = GameLogicUpdateManager.GameFrame + 10;
	}

	public override Vector3 GetTargetPoint()
	{
		return base.AimTransform.position + base.AimPoint;
	}

	public void SetWinPose(Callback WinPoseEndCB)
	{
		_chargeShootObj.StopCharge();
		if (GetCurrentWeaponObj().ChipEfx != null)
		{
			GetCurrentWeaponObj().ChipEfx.ActiveChipSkill(false);
		}
		DisableCurrentWeapon();
		if (CharacterData.n_WEAPON_OUT != -1)
		{
			EnableSkillWeapon(CharacterData.n_WEAPON_OUT);
		}
		SetStatus(MainStatus.TELEPORT_OUT, SubStatus.WIN_POSE);
		_refWinPoseEndCB = WinPoseEndCB;
	}

	public void SetTeleportOutPose()
	{
		_chargeShootObj.StopCharge();
		if (GetCurrentWeaponObj().ChipEfx != null)
		{
			GetCurrentWeaponObj().ChipEfx.ActiveChipSkill(false);
		}
		DisableCurrentWeapon();
		if (CharacterData.n_WEAPON_OUT != -1)
		{
			DisableSkillWeapon(CharacterData.n_WEAPON_OUT);
		}
		IsTeleporting = true;
		_teleportOutParticlePhase = 0;
		SetStatus(MainStatus.TELEPORT_OUT, SubStatus.TELEPORT_POSE);
	}

	public void DisableCurrentWeapon()
	{
		DisableWeaponMesh(GetCurrentWeaponObj());
	}

	public void EnableCurrentWeapon()
	{
		EnableWeaponMesh(GetCurrentWeaponObj(), null, 0f);
	}

	public void EnableSkillWeapon(int id, Callback pCB = null)
	{
		EnableWeaponMesh(PlayerSkills[id], pCB);
	}

	public void DisableSkillWeapon(int id)
	{
		DisableWeaponMesh(PlayerSkills[id]);
	}

	public override void SetStun(bool enable, bool bCheckOtherObj = true)
	{
		if (bCheckOtherObj)
		{
			if (refRideBaseObj != null)
			{
				refRideBaseObj.SetStun(enable);
			}
			else
			{
				SetStun(enable, false);
			}
			return;
		}
		if (enable)
		{
			_stunStack++;
			InitImportantVar();
			SetStatus(MainStatus.HURT, SubStatus.LAND);
			LockControl();
		}
		else
		{
			_stunStack--;
			if (_stunStack <= 0)
			{
				_stunStack = 0;
				InitImportantVar();
				if (!IsDead())
				{
					SetStatus(MainStatus.FALL, SubStatus.TELEPORT_POSE);
				}
			}
		}
		CharacterControlBase component = GetComponent<CharacterControlBase>();
		if ((bool)component)
		{
			component.SetStun(enable);
		}
	}

	public override void SetNoMove(bool enable, bool bCheckOtherObj = true)
	{
		if (bCheckOtherObj)
		{
			if (refRideBaseObj != null)
			{
				refRideBaseObj.SetNoMove(enable);
			}
			else
			{
				SetNoMove(enable, false);
			}
			return;
		}
		if (enable)
		{
			_nomoveStack++;
			InitImportantVar();
			StopPlayer();
			return;
		}
		_nomoveStack--;
		if (_nomoveStack <= 0)
		{
			_nomoveStack = 0;
		}
	}

	public override void SetBanWeapon(bool enable)
	{
		if (enable)
		{
			LockWeapon = true;
		}
		else if (!selfBuffManager.CheckHasEffect(110))
		{
			LockWeapon = false;
		}
	}

	public override void SetBanSkill(bool enable)
	{
		if (enable)
		{
			LockSkill = true;
		}
		else if (!selfBuffManager.CheckHasEffect(111))
		{
			LockSkill = false;
		}
	}

	public override void SetReverseRightAndLeft(bool enable)
	{
		if (enable)
		{
			ReverseRightAndLeft = true;
		}
		else if (!selfBuffManager.CheckHasEffect(118))
		{
			ReverseRightAndLeft = false;
		}
	}

	public override void SetBanAutoAim(bool enable)
	{
		if (enable)
		{
			BanAutoAim = true;
			PlayerAutoAimSystem.SetEnable(false);
		}
		else if (!selfBuffManager.CheckHasEffect(119))
		{
			BanAutoAim = false;
			_playerAutoAimSystem.SetEnable(PlayerSetting.AutoAim != 0);
		}
	}

	public void SetCatched(bool enable, int enemyDir)
	{
		_isJack = enable;
		Controller.Collider2D.enabled = !enable;
		if (enable)
		{
			LockControl();
			SetStatus(MainStatus.HURT, SubStatus.LAND);
			DisableCurrentWeapon();
			return;
		}
		Controller.SetLogicPosition(new VInt3(_transform.position));
		if ((bool)objInfoBar)
		{
			objInfoBar.transform.rotation = Quaternion.identity;
		}
		if (CurMainStatus == MainStatus.HURT)
		{
			SetStatus(MainStatus.FALL, SubStatus.TELEPORT_POSE);
			if ((bool)Physics2D.Raycast(_transform.position, Vector2.right * enemyDir, 2f, 1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockLayer))
			{
				SetSpeed(-enemyDir * WalkSpeed / 2, Mathf.RoundToInt(JumpSpeedEx / 2));
			}
			else
			{
				SetSpeed(enemyDir * WalkSpeed, Mathf.RoundToInt(JumpSpeedEx / 2));
			}
		}
		LockInput = false;
		EnableCurrentWeapon();
		SetNormalHitBox();
	}

	public IEnumerator CatchCheckWall(int enemyDir)
	{
		RaycastHit2D hitray = default(RaycastHit2D);
		while (hitray.collider == null && (int)Hp > 0 && base.transform.parent != null)
		{
			Vector3 vector = new Vector3(base.transform.position.x, base.transform.position.y + 1f, 0f);
			switch (enemyDir)
			{
			case 1:
				hitray = Physics2D.Raycast(vector, Vector2.right, 1.5f, 1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockLayer);
				break;
			case -1:
				hitray = Physics2D.Raycast(vector, Vector2.left, 1.5f, 1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockLayer);
				break;
			}
			yield return CoroutineDefine._waitForEndOfFrame;
		}
		base.transform.SetParentNull();
	}

	public virtual bool EnterRideArmor(RideBaseObj targetRideArmor)
	{
		_chargeShootObj.StopCharge();
		IgnoreGravity = false;
		_usingVehicle = true;
		if (CurrentActiveSkill != 100)
		{
			ClearSkillEvt.CheckTargetToInvoke();
		}
		CurrentActiveSkill = -1;
		_leaveRideArmorJump = false;
		DisableCurrentWeapon();
		base._characterDirection = targetRideArmor._characterDirection;
		PlayerStopHovering(false);
		Animator.UpdateDirection((int)base._characterDirection);
		SetSpeed(0, 0);
		UpdateFlyDirection(FLY_DIR.HORIZONTAL);
		DistanceDelta = 0f;
		_transform.SetParent(targetRideArmor.SeatTransform);
		_transform.localPosition = Vector3.zero;
		SetStatus(MainStatus.RIDE_ARMOR, SubStatus.RIDE_ARMOR);
		targetRideArmor.MasterPilot = this;
		targetRideArmor.gameObject.layer = base.gameObject.layer;
		Controller.Collider2D.enabled = false;
		LinkUpdateCall = targetRideArmor.LogicUpdateCall;
		LinkUpdatePrepare = targetRideArmor.LogicUpdatePrepare;
		RemovePlayerObjInfoBar();
		targetRideArmor.Update_AutoAim(_playerAutoAimSystem);
		refRideBaseObj = targetRideArmor;
		if (_jumpThroughCoroutine != null)
		{
			OrangeBattleUtility.ChangeLayersRecursively(ModelTransform, ManagedSingleton<OrangeLayerManager>.Instance.RenderPlayer, false);
			_chargeShootObj.ChangeLayer(ManagedSingleton<OrangeLayerManager>.Instance.RenderPlayer, true);
		}
		Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.ENTER_OR_LEAVE_RIDE_ARMOR, true);
		return true;
	}

	public virtual void LeaveRideArmor(RideBaseObj targetRideArmor)
	{
		_leaveRideArmorJump = true;
		base._characterDirection = targetRideArmor._characterDirection;
		Controller.SetLogicPosition(new VInt3(targetRideArmor.transform.position));
		_transform.SetParent(null);
		_transform.localRotation = Quaternion.identity;
		targetRideArmor.MasterPilot = null;
		targetRideArmor.gameObject.layer = ManagedSingleton<OrangeLayerManager>.Instance.VehicleLayer;
		ConnectStandardCtrlCB();
		PlayerPressJumpCB();
		_usingVehicle = false;
		Controller.Collider2D.enabled = true;
		LinkUpdateCall = null;
		LinkUpdatePrepare = null;
		refRideBaseObj = null;
		StageResManager.CreateHpBarToPlayer(this);
		if (base.transform.localScale.z < 0f)
		{
			base.transform.localScale = new Vector3(base.transform.localScale.x, base.transform.localScale.y, 0f - base.transform.localScale.z);
		}
		SetNormalHitBox();
		EnableCurrentWeapon();
		UpdateSkillIcon(PlayerSkills);
		UpdateAimRangeByWeaponEvt(GetCurrentWeaponObj());
		Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.ENTER_OR_LEAVE_RIDE_ARMOR, false);
	}

	public virtual void LeaveRider()
	{
		_transform.SetParent(null);
		_transform.localRotation = Quaternion.identity;
		_usingVehicle = false;
		Controller.Collider2D.enabled = true;
		LinkUpdateCall = null;
		LinkUpdatePrepare = null;
		if (base.transform.localScale.z < 0f)
		{
			base.transform.localScale = new Vector3(base.transform.localScale.x, base.transform.localScale.y, 0f - base.transform.localScale.z);
		}
		EnableCurrentWeapon();
		UpdateSkillIcon(PlayerSkills);
		UpdateAimRangeByWeaponEvt(GetCurrentWeaponObj());
		if (_stunStack > 0)
		{
			SetStatus(MainStatus.HURT, SubStatus.LAND);
		}
		else
		{
			IgnoreGravity = false;
		}
		MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("DistortionFx", GetTargetPoint(), Quaternion.identity, Array.Empty<object>());
	}

	public void Change_Chargefx_Layer(int layer, bool chageSelf = true)
	{
		_chargeShootObj.ChangeLayer(layer, true);
	}

	public override bool CheckActStatus(int mainstatus, int substatus)
	{
		if (mainstatus == (int)CurMainStatus && substatus == -1)
		{
			return true;
		}
		if (mainstatus == (int)CurMainStatus && substatus == (int)CurSubStatus)
		{
			return true;
		}
		return false;
	}

	public void ToggleExtraMesh(bool open)
	{
		Renderer[] extraMeshOpen = ExtraMeshOpen;
		for (int i = 0; i < extraMeshOpen.Length; i++)
		{
			extraMeshOpen[i].enabled = open;
		}
		extraMeshOpen = ExtraMeshClose;
		for (int i = 0; i < extraMeshOpen.Length; i++)
		{
			extraMeshOpen[i].enabled = !open;
		}
	}

	public IEnumerator HitStop()
	{
		Animator._animator.speed = 0.1f;
		yield return new WaitForSeconds(OrangeBattleUtility.HitStopTime / 2f);
		Animator._animator.speed = 1f;
		IsHitStop = false;
	}

	protected void CheckSemiBlockPlayerRender()
	{
		bool flag = true;
		RaycastHit2D raycastHit2D = OrangeBattleUtility.RaycastIgnoreSelf(_transform.position, Vector2.down, 0.1f, Controller.collisionMaskThrough, _transform);
		if ((bool)raycastHit2D && raycastHit2D.collider.transform.name == "a_colliderObj")
		{
			flag = false;
		}
		if (flag)
		{
			StartJumpThroughCorutine();
		}
	}

	public void UpdateAimRangeByWeapon(WeaponStruct weapon)
	{
		if (weapon.WeaponData != null && (short)weapon.WeaponData.n_TYPE == 8)
		{
			float num = 0f;
			if (PlayerSkills[0].BulletData.n_TYPE == 1 && PlayerSkills[0].BulletData.f_DISTANCE > num)
			{
				num = PlayerSkills[0].BulletData.f_DISTANCE;
			}
			if (PlayerSkills[1].BulletData.n_TYPE == 1 && PlayerSkills[1].BulletData.f_DISTANCE > num)
			{
				num = PlayerSkills[1].BulletData.f_DISTANCE;
			}
			if (num.Equals(0f))
			{
				num = 10f;
			}
			_playerAutoAimSystem.UpdateAimRange(num);
		}
		else
		{
			_playerAutoAimSystem.UpdateAimRange(weapon.BulletData.f_DISTANCE);
		}
	}

	public void SetupChargeComponent(ParticleSystem lv1, ParticleSystem lv2, ParticleSystem lv3, ParticleSystem start, ChargeData tChargeData, int nSkillID = 0)
	{
		_chargeShootObj = base.gameObject.AddOrGetComponent<ChargeShootObj>();
		_chargeShootObj.ChargeLv1ParticleSystem[nSkillID] = lv1;
		_chargeShootObj.ChargeLv2ParticleSystem[nSkillID] = lv2;
		_chargeShootObj.ChargeLv3ParticleSystem[nSkillID] = lv3;
		_chargeShootObj.ChargeStartParticleSystem[nSkillID] = start;
		_chargeShootObj.nUpdateInAdvance = new int[2][]
		{
			new int[4] { 0, tChargeData.nUpdateInAdvanceLV1FX, tChargeData.nUpdateInAdvanceLV2FX, tChargeData.nUpdateInAdvanceLV3FX },
			new int[4] { 0, tChargeData.nUpdateInAdvanceLV1FX2, tChargeData.nUpdateInAdvanceLV2FX2, tChargeData.nUpdateInAdvanceLV3FX2 }
		};
		if (ChargeShotParam != null)
		{
			if (IsLocalPlayer)
			{
				_chargeShootObj.ChargeSE = ChargeShotParam.ChargeSE_localPlayer;
			}
			else
			{
				_chargeShootObj.ChargeSE = ChargeShotParam.ChargeSE_otherPlayer;
			}
		}
	}

	public void CancelBusterChargeAtk()
	{
		IsShoot = 0;
		if (Controller.Collisions.below)
		{
			ForceSetAnimateId(HumanBase.AnimateId.ANI_STAND);
			Animator.PlayAnimation(HumanBase.AnimateId.ANI_STAND, CurrentFrame);
		}
		else
		{
			ForceSetAnimateId(HumanBase.AnimateId.ANI_FALL);
			Animator.PlayAnimation(HumanBase.AnimateId.ANI_FALL, CurrentFrame);
		}
	}

	public bool CheckPetActive(int petId)
	{
		return false;
	}

	public void ResetDashChance()
	{
		_dashChance = MaxDashChance;
	}

	public void ClearDashChance()
	{
		_dashChance = 0;
	}

	protected void UpdateFlyDirection(FLY_DIR upDown)
	{
		if (!_hoveringEnable)
		{
			ModelTransform.localEulerAngles = new Vector3(0f, ModelTransform.localEulerAngles.y, ModelTransform.localEulerAngles.z);
			return;
		}
		float x = 0f;
		switch (upDown)
		{
		case FLY_DIR.UP:
			x = -30f * (float)base._characterDirection;
			break;
		case FLY_DIR.DOWN:
			x = 30f * (float)base._characterDirection;
			break;
		}
		ModelTransform.localEulerAngles = new Vector3(x, ModelTransform.localEulerAngles.y, ModelTransform.localEulerAngles.z);
	}

	public override void LockAnimator(bool bLock)
	{
		LockAnimatorEvt.CheckTargetToInvoke(bLock);
	}

	public void LockCurrentAnimator(bool bLock)
	{
		base.LockAnimator(bLock);
	}

	private void DeadAreaEventLock(bool bLock)
	{
	}

	protected float CalculateVerticalMovement(bool raw = false)
	{
		_fromWaypointIndex %= _globalWaypoints.Length;
		int num = (_fromWaypointIndex + 1) % _globalWaypoints.Length;
		float num2 = Mathf.Abs(_globalWaypoints[_fromWaypointIndex] - _globalWaypoints[num]);
		_percentBetweenWaypoints += GameLogicUpdateManager.m_fFrameLen * _easeSpeed / num2;
		_percentBetweenWaypoints = Mathf.Clamp01(_percentBetweenWaypoints);
		float t = Ease(_percentBetweenWaypoints);
		float num3 = Mathf.Lerp(_globalWaypoints[_fromWaypointIndex], _globalWaypoints[num], t);
		if (_percentBetweenWaypoints >= 1f)
		{
			_percentBetweenWaypoints = 0f;
			_fromWaypointIndex++;
		}
		if (raw)
		{
			return num3;
		}
		return num3 - base.transform.position.y;
	}

	protected float Ease(float x)
	{
		return Mathf.Pow(x, 2f) / (Mathf.Pow(x, 2f) + Mathf.Pow(1f - x, 2f));
	}

	public void StartJumpThroughCorutine()
	{
		OrangeBattleUtility.ChangeLayersRecursively(ModelTransform, ManagedSingleton<OrangeLayerManager>.Instance.RenderSPEnemy, false);
		if (_jumpThroughCoroutine == null)
		{
			_jumpThroughCoroutine = StartCoroutine(WaitJumpUPThrough());
			return;
		}
		StopCoroutine(_jumpThroughCoroutine);
		_jumpThroughCoroutine = StartCoroutine(WaitJumpUPThrough());
	}

	protected IEnumerator WaitJumpThrough()
	{
		yield return new WaitForSeconds(0.3f);
		OrangeBattleUtility.ChangeLayersRecursively(ModelTransform, ManagedSingleton<OrangeLayerManager>.Instance.RenderPlayer, false);
		_chargeShootObj.ChangeLayer(ManagedSingleton<OrangeLayerManager>.Instance.RenderPlayer, true);
		Controller.JumpUPThrough = false;
		_jumpThroughCoroutine = null;
	}

	protected IEnumerator WaitJumpUPThrough()
	{
		yield return CoroutineDefine._waitForEndOfFrame;
		while (CurMainStatus != MainStatus.FALL && CurMainStatus != 0 && CurMainStatus != MainStatus.WALLGRAB)
		{
			yield return CoroutineDefine._waitForEndOfFrame;
		}
		while (!Controller.Collisions.below && CurMainStatus != MainStatus.WALLGRAB)
		{
			yield return CoroutineDefine._waitForEndOfFrame;
		}
		OrangeBattleUtility.ChangeLayersRecursively(ModelTransform, ManagedSingleton<OrangeLayerManager>.Instance.RenderPlayer, false);
		_chargeShootObj.ChangeLayer(ManagedSingleton<OrangeLayerManager>.Instance.RenderPlayer, true);
		Controller.JumpUPThrough = false;
		_jumpThroughCoroutine = null;
	}

	public virtual void UpdateSkillIcon(WeaponStruct[] PlayerSkills)
	{
	}

	public virtual void ForceChangeSkillIcon(int id, Sprite icon)
	{
	}

	public virtual void ForceChangeSkillIcon(int id, string sIcon)
	{
	}

	protected void InitializeVoice()
	{
		_VoiceID = AudioLib.GetVoice(ref CharacterData);
		_CharaSEID = AudioLib.GetCharaSE(ref CharacterData);
		_SkillSEID = AudioLib.GetSkillSE(ref CharacterData);
		AnimatorSoundHelper[] componentsInChildren = base.gameObject.GetComponentsInChildren<AnimatorSoundHelper>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].SoundSource = SoundSource;
		}
	}

	public void PlaySE(string s_acb, int n_cueId)
	{
		SoundSource.PlaySE(s_acb, n_cueId);
	}

	public override void PlaySE(string s_acb, string cueName, float delay = 0f, bool ForceTrigger = false, bool UseDisCheck = true)
	{
		SoundSource.PlaySE(s_acb, cueName, delay);
	}

	public void PlayBattleSE(BattleSE cueId)
	{
		SoundSource.PlaySE("BattleSE", (int)cueId);
	}

	public float GetOCSoundVolume()
	{
		bool isLocalPlayer = IsLocalPlayer;
		return 100f;
	}

	public float GetDisVolume(Vector3 tarPos)
	{
		float num = Vector2.Distance(base.transform.position, tarPos);
		if (num - 16f > 0f)
		{
			return 0f;
		}
		float num2 = 0f;
		num2 = ((!(num <= 3f)) ? (1f - num / 16f) : 1f);
		return 100f * num2;
	}

	public bool IsNearPlayer()
	{
		if (IsLocalPlayer)
		{
			return true;
		}
		OrangeCharacter mainPlayerOC = StageUpdate.GetMainPlayerOC();
		if (mainPlayerOC != null && Vector2.Distance(base.transform.position, mainPlayerOC._transform.position) >= 10f)
		{
			return false;
		}
		return true;
	}

	public void StopAllLoopSE()
	{
		_chargeShootObj.StopCharge(-1);
		if (isUseKabesuriSE)
		{
			_chargeShootObj.ClearSE();
			isUseKabesuriSE = false;
		}
	}

	protected bool CheckPlayWeakVoice()
	{
		if (MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.IsPvp && !IsLocalPlayer)
		{
			return false;
		}
		float num = (float)(int)Hp / (float)(int)MaxHp;
		if (num < 0.2f && _weakVoice == 2)
		{
			_weakVoice = 3;
			PlaySE(VoiceID, 22);
			return true;
		}
		if (num < 0.35f && _weakVoice == 1)
		{
			_weakVoice = 2;
			PlaySE(VoiceID, 21);
			return true;
		}
		if (num < 0.5f && _weakVoice == 0)
		{
			_weakVoice = 1;
			PlaySE(VoiceID, 20);
			return true;
		}
		if ((double)num > 0.5)
		{
			_weakVoice = 0;
		}
		return false;
	}

	protected void SlashVoice(SlashType pSlashType)
	{
		switch (pSlashType)
		{
		case SlashType.StandSlash1:
			PlaySE(VoiceID, 14);
			break;
		case SlashType.StandSlash2:
			PlaySE(VoiceID, 15);
			break;
		case SlashType.StandSlash3:
			PlaySE(VoiceID, 16);
			break;
		case SlashType.StandSlash4:
			PlaySE(VoiceID, 17);
			break;
		case SlashType.StandSlash5:
			PlaySE(VoiceID, 18);
			break;
		}
	}

	public void PlayVoice(Voice seId)
	{
		PlaySE(_VoiceID, (int)seId);
	}

	public void PlayCharaSE(CharaSE seId)
	{
		PlaySE(_CharaSEID, (int)seId);
	}

	public void UpdateAudioRelayParamByType(Type type)
	{
		CharacterTypeName = type.Name;
		listStatusParams = MonoBehaviourSingleton<OrangeCriRelayManager>.Instance.GetCharacterUpdateParam(CharacterTypeName);
		TeleportParam = MonoBehaviourSingleton<OrangeCriRelayManager>.Instance.GetCharacterTeleportParam(CharacterTypeName);
		ChargeShotParam = MonoBehaviourSingleton<OrangeCriRelayManager>.Instance.GetCharacterChargeShotParam(CharacterTypeName);
		CallPetParam = MonoBehaviourSingleton<OrangeCriRelayManager>.Instance.GetCharacterCallPetParam(CharacterTypeName);
	}

	public void SoundRelayUpdate()
	{
		for (int i = 0; i < listStatusParams.Count; i++)
		{
			CharacterParam characterParam = listStatusParams[i];
			if (characterParam.MainStatus == CurMainStatus && characterParam.SubStatus == CurSubStatus)
			{
				switch (characterParam.AudioType)
				{
				case OrangeCriRelay.AudioType.VOICE:
					PlaySE(VoiceID, characterParam.CueName, characterParam.Delay);
					break;
				case OrangeCriRelay.AudioType.SKILL:
					PlaySE(SkillSEID, characterParam.CueName, characterParam.Delay);
					break;
				case OrangeCriRelay.AudioType.CHARACTER:
					PlaySE(CharaSEID, characterParam.CueName, characterParam.Delay);
					break;
				}
			}
		}
	}
}
