using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using CallbackDefs;
using CodeStage.AntiCheat.ObscuredTypes;
using StageLib;
using UnityEngine;

public class RideArmorController : RideBaseObj, IManagedUpdateBehavior, IManagedLateUpdateBehavior, IAimTarget, ILogicUpdate
{
	public enum MainStatus
	{
		Empty = 0,
		BootUp = 1,
		Shutdown = 2,
		Idle = 3,
		Walk = 4,
		AirDash = 5,
		Dash = 6,
		Jump = 7,
		Fall = 8,
		Attack = 9,
		Skill0 = 10,
		Skill1 = 11,
		MAX_STATUS = 12,
		HURT = 13
	}

	private struct RideArmorSE
	{
		public string Acb;

		public string Cue;

		public int MillisecondsDelay;
	}

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CDelayPlaySe_003Ed__54 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncVoidMethodBuilder _003C_003Et__builder;

		public RideArmorSE se;

		public RideArmorController _003C_003E4__this;

		private TaskAwaiter _003C_003Eu__1;

		private void MoveNext()
		{
			int num = _003C_003E1__state;
			RideArmorController rideArmorController = _003C_003E4__this;
			try
			{
				TaskAwaiter awaiter;
				if (num != 0)
				{
					awaiter = Task.Delay(se.MillisecondsDelay).GetAwaiter();
					if (!awaiter.IsCompleted)
					{
						num = (_003C_003E1__state = 0);
						_003C_003Eu__1 = awaiter;
						_003C_003Et__builder.AwaitUnsafeOnCompleted(ref awaiter, ref this);
						return;
					}
				}
				else
				{
					awaiter = _003C_003Eu__1;
					_003C_003Eu__1 = default(TaskAwaiter);
					num = (_003C_003E1__state = -1);
				}
				awaiter.GetResult();
				if ((int)rideArmorController.Hp > 0)
				{
					MonoBehaviourSingleton<AudioManager>.Instance.Play(se.Acb, se.Cue);
				}
			}
			catch (Exception exception)
			{
				_003C_003E1__state = -2;
				_003C_003Et__builder.SetException(exception);
				return;
			}
			_003C_003E1__state = -2;
			_003C_003Et__builder.SetResult();
		}

		void IAsyncStateMachine.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			this.MoveNext();
		}

		[DebuggerHidden]
		private void SetStateMachine(IAsyncStateMachine stateMachine)
		{
			_003C_003Et__builder.SetStateMachine(stateMachine);
		}

		void IAsyncStateMachine.SetStateMachine(IAsyncStateMachine stateMachine)
		{
			//ILSpy generated this explicit interface implementation from .override directive in SetStateMachine
			this.SetStateMachine(stateMachine);
		}
	}

	private float _currentFrame;

	private float _LastFrame;

	private bool bAnimationEnd;

	private bool bAnimationNext;

	private float fWaitTime;

	protected VEHICLE_TABLE tVEHICLE_TABLE;

	protected WeaponStruct[] RASkills = new WeaponStruct[3];

	protected Transform[] AtkShotTrans = new Transform[3];

	protected Transform[] AtkTransSpecial = new Transform[2];

	private MainStatus _mainStatus;

	protected bool _isDash;

	protected bool _isHeldDash;

	protected float _dashTimer;

	protected float _minDashTime = 0.7f;

	protected int _dashChance = 1;

	protected bool _isJump;

	protected bool _airDashEnable = true;

	protected bool bWaitDead;

	protected Coroutine DeadWaitCoroutine;

	protected bool bFly;

	protected BulletBase.ShotBullerParam CreateBulletUseParam = new BulletBase.ShotBullerParam();

	private Animator _animator;

	private CharacterMaterial _characterMaterial;

	private Rigidbody2D rigidbody;

	protected float distanceDelta;

	protected VInt3 _velocity;

	protected VInt3 _velocityExtra;

	protected VInt3 _velocityShift;

	protected VInt _maxGravity;

	protected int MaxDashChance = 1;

	protected float MaxDashDistance = 0.7f;

	public bool IgnoreGravity;

	protected bool InitializedMaxHP;

	private ModelStatusCC tModelStatusCC;

	protected int nAnimationID = 1;

	protected bool _virtualButtonInitialized;

	protected readonly VirtualButton[] _virtualButton = new VirtualButton[6];

	private ParticleSystem mefx_bootup;

	private ParticleSystem efx_dash0;

	private ParticleSystem efx_dash1;

	private ParticleSystem efx_dash2;

	protected Setting setting = new Setting(true);

	protected OrangeTimer _doubleTapTimer;

	protected ButtonId _doubleTapBtn;

	private CollideBullet tCollideBullet;

	private WeaponStatus ArmorWeaponStatus = new WeaponStatus();

	private Dictionary<MainStatus, RideArmorSE> rideArmorSeDict = new Dictionary<MainStatus, RideArmorSE>();

	protected void OnEnable()
	{
		MonoBehaviourSingleton<UpdateManager>.Instance.AddUpdate(this);
		MonoBehaviourSingleton<UpdateManager>.Instance.AddLateUpdate(this);
		MonoBehaviourSingleton<GameLogicUpdateManager>.Instance.AddUpdate(this);
	}

	protected void OnDisable()
	{
		MonoBehaviourSingleton<UpdateManager>.Instance.RemoveUpdate(this);
		MonoBehaviourSingleton<UpdateManager>.Instance.RemoveLateUpdate(this);
		MonoBehaviourSingleton<GameLogicUpdateManager>.Instance.RemoveUpdate(this);
	}

	private void Start()
	{
		_transform = base.transform;
		Controller = GetComponent<Controller2D>();
		Transform[] target = _transform.GetComponentsInChildren<Transform>(true);
		ModelTransform = OrangeBattleUtility.FindChildRecursive(ref target, "model", true);
		base.AimTransform = OrangeBattleUtility.FindChildRecursive(ref target, "body_bone1");
		SeatTransform = OrangeBattleUtility.FindChildRecursive(ref target, "SeatPoint", true);
		for (int i = 0; i < AtkShotTrans.Length; i++)
		{
			AtkShotTrans[i] = OrangeBattleUtility.FindChildRecursive(ref target, "SkillShotPt" + (i + 1), true);
		}
		_animator = GetComponentInChildren<Animator>();
		_maxGravity = OrangeBattleUtility.FP_MaxGravity * 2;
		tModelStatusCC = ModelTransform.GetComponent<ModelStatusCC>();
		_characterMaterial = GetComponentInChildren<CharacterMaterial>();
		if ((bool)tModelStatusCC && (bool)tModelStatusCC.ExplosionPart)
		{
			MonoBehaviourSingleton<PoolManager>.Instance.CreatePoolBaseLocal<EnemyDieCollider>(UnityEngine.Object.Instantiate(tModelStatusCC.ExplosionPart, new Vector3(99999f, 99999f, 99999f), Quaternion.identity), tModelStatusCC.ExplosionPart.Name, 1);
		}
		UpdateDirection((int)base._characterDirection);
		for (int j = 0; j < RASkills.Length; j++)
		{
			RASkills[j] = new WeaponStruct();
		}
		if (tRefPassiveskill == null)
		{
			tRefPassiveskill = new RefPassiveskill();
		}
		if (ManagedSingleton<OrangeDataManager>.Instance.VEHICLE_TABLE_DICT.TryGetValue(nRideID, out tVEHICLE_TABLE))
		{
			ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT.TryGetValue(tVEHICLE_TABLE.n_SKILL_1, out RASkills[0].BulletData);
			ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT.TryGetValue(tVEHICLE_TABLE.n_SKILL_2, out RASkills[1].BulletData);
			ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT.TryGetValue(tVEHICLE_TABLE.n_SKILL_3, out RASkills[2].BulletData);
			if (tVEHICLE_TABLE.n_INITIAL_SKILL1 != 0)
			{
				tRefPassiveskill.AddPassivesSkill(tVEHICLE_TABLE.n_INITIAL_SKILL1);
			}
			if (tVEHICLE_TABLE.n_INITIAL_SKILL2 != 0)
			{
				tRefPassiveskill.AddPassivesSkill(tVEHICLE_TABLE.n_INITIAL_SKILL2);
			}
			if (tVEHICLE_TABLE.n_INITIAL_SKILL3 != 0)
			{
				tRefPassiveskill.AddPassivesSkill(tVEHICLE_TABLE.n_INITIAL_SKILL3);
			}
		}
		if (RASkills[0].BulletData == null)
		{
			RASkills[0].BulletData = new SKILL_TABLE();
		}
		if (RASkills[1].BulletData == null)
		{
			RASkills[1].BulletData = new SKILL_TABLE();
		}
		if (RASkills[2].BulletData == null)
		{
			RASkills[2].BulletData = new SKILL_TABLE();
		}
		for (int k = 0; k < RASkills.Length; k++)
		{
			RASkills[k].ForceLock = false;
			RASkills[k].ChargeTimer = new UpdateTimer();
			RASkills[k].LastUseTimer = new UpdateTimer();
			RASkills[k].LastUseTimer.TimerStart();
			RASkills[k].MagazineRemain = RASkills[k].BulletData.n_MAGAZINE;
		}
		SetStatus(MainStatus.Empty);
		Hp = tVEHICLE_TABLE.n_HP;
		MaxHp = tVEHICLE_TABLE.n_HP;
		base.gameObject.AddOrGetComponent<StageObjParam>().tLinkSOB = this;
		if (tVEHICLE_TABLE.n_ID == 2 || tVEHICLE_TABLE.n_ID == 3 || tVEHICLE_TABLE.n_ID == 5)
		{
			AtkTransSpecial[0] = OrangeBattleUtility.FindChildRecursive(ref target, "ShootPoint_L", true);
			AtkTransSpecial[1] = OrangeBattleUtility.FindChildRecursive(ref target, "ShootPoint_R", true);
		}
		ModelTransform.gameObject.AddComponent<AnimatorSoundHelper>().SoundSource = base.SoundSource;
		rigidbody = base.gameObject.AddComponent<Rigidbody2D>();
		rigidbody.bodyType = RigidbodyType2D.Kinematic;
		rigidbody.simulated = false;
		selfBuffManager.Init(this);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_headcrush_001", 2);
		Transform transform = OrangeBattleUtility.FindChildRecursive(ref target, "efx_bootup");
		if ((bool)transform)
		{
			mefx_bootup = transform.GetComponent<ParticleSystem>();
		}
		transform = OrangeBattleUtility.FindChildRecursive(ref target, "efx_dash1");
		if ((bool)transform)
		{
			efx_dash1 = transform.GetComponent<ParticleSystem>();
		}
		transform = OrangeBattleUtility.FindChildRecursive(ref target, "efx_dash2");
		if ((bool)transform)
		{
			efx_dash2 = transform.GetComponent<ParticleSystem>();
		}
		transform = OrangeBattleUtility.FindChildRecursive(ref target, "efx_dash0");
		if ((bool)transform)
		{
			efx_dash0 = transform.GetComponent<ParticleSystem>();
		}
		setting = MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.Setting;
		_doubleTapTimer = OrangeTimerManager.GetTimer();
		_doubleTapTimer.SetMode(TimerMode.MILLISECOND);
		InitRideArmorAudioDict();
	}

	private void InitRideArmorAudioDict()
	{
		rideArmorSeDict = new Dictionary<MainStatus, RideArmorSE>();
		int n_ID = tVEHICLE_TABLE.n_ID;
		if (n_ID == 6)
		{
			RideArmorSE rideArmorSE = default(RideArmorSE);
			rideArmorSE.Acb = "BattleSE02";
			rideArmorSE.Cue = "bt_rexride02";
			rideArmorSE.MillisecondsDelay = 0;
			RideArmorSE value = rideArmorSE;
			rideArmorSE = default(RideArmorSE);
			rideArmorSE.Acb = "BattleSE02";
			rideArmorSE.Cue = "bt_rexride04";
			rideArmorSE.MillisecondsDelay = 134;
			RideArmorSE value2 = rideArmorSE;
			rideArmorSE = default(RideArmorSE);
			rideArmorSE.Acb = "HitSE";
			rideArmorSE.Cue = "ht_ridearmor";
			rideArmorSE.MillisecondsDelay = 0;
			RideArmorSE value3 = rideArmorSE;
			rideArmorSeDict.Add(MainStatus.Dash, value);
			rideArmorSeDict.Add(MainStatus.Attack, value2);
			rideArmorSeDict.Add(MainStatus.HURT, value3);
		}
		else
		{
			RideArmorSE rideArmorSE = default(RideArmorSE);
			rideArmorSE.Acb = "BattleSE";
			rideArmorSE.Cue = "bt_ridearmor05";
			rideArmorSE.MillisecondsDelay = 0;
			RideArmorSE value4 = rideArmorSE;
			rideArmorSE = default(RideArmorSE);
			rideArmorSE.Acb = "HitSE";
			rideArmorSE.Cue = "ht_ridearmor";
			rideArmorSE.MillisecondsDelay = 0;
			RideArmorSE value5 = rideArmorSE;
			rideArmorSeDict.Add(MainStatus.Dash, value4);
			rideArmorSeDict.Add(MainStatus.HURT, value5);
		}
	}

	public void PlaySeByDict(MainStatus mainStatus)
	{
		RideArmorSE value;
		if ((int)Hp > 0 && rideArmorSeDict.TryGetValue(mainStatus, out value))
		{
			base.SoundSource.PlaySE(value.Acb, value.Cue, value.MillisecondsDelay / 1000);
		}
	}

	[AsyncStateMachine(typeof(_003CDelayPlaySe_003Ed__54))]
	private void DelayPlaySe(RideArmorSE se)
	{
		_003CDelayPlaySe_003Ed__54 stateMachine = default(_003CDelayPlaySe_003Ed__54);
		stateMachine._003C_003E4__this = this;
		stateMachine.se = se;
		stateMachine._003C_003Et__builder = AsyncVoidMethodBuilder.Create();
		stateMachine._003C_003E1__state = -1;
		AsyncVoidMethodBuilder _003C_003Et__builder = stateMachine._003C_003Et__builder;
		_003C_003Et__builder.Start(ref stateMachine);
	}

	protected bool InitializeVirtualButton()
	{
		if (_virtualButtonInitialized)
		{
			return true;
		}
		for (int i = 0; i < _virtualButton.Length; i++)
		{
			switch (i)
			{
			default:
				_virtualButton[i] = MonoBehaviourSingleton<InputManager>.Instance.GetVirtualButton(ButtonId.SHOOT);
				break;
			case 1:
				_virtualButton[i] = MonoBehaviourSingleton<InputManager>.Instance.GetVirtualButton(ButtonId.SKILL0);
				break;
			case 2:
				_virtualButton[i] = MonoBehaviourSingleton<InputManager>.Instance.GetVirtualButton(ButtonId.SKILL1);
				break;
			case 3:
				_virtualButton[i] = MonoBehaviourSingleton<InputManager>.Instance.GetVirtualButton(ButtonId.SELECT);
				break;
			case 4:
				_virtualButton[i] = MonoBehaviourSingleton<InputManager>.Instance.GetVirtualButton(ButtonId.FS_SKILL);
				break;
			case 5:
				_virtualButton[i] = MonoBehaviourSingleton<InputManager>.Instance.GetVirtualButton(ButtonId.CHIP_SWITCH);
				break;
			}
			if (_virtualButton[i] == null)
			{
				return false;
			}
		}
		_virtualButtonInitialized = true;
		UpdateWeaponIcon();
		UpdateSkillIcon();
		return true;
	}

	private void UpdateWeaponIcon()
	{
		if (!_virtualButtonInitialized)
		{
			return;
		}
		for (int i = 0; i < _virtualButton.Length; i++)
		{
			if ((bool)_virtualButton[i])
			{
				switch (i)
				{
				case 0:
					_virtualButton[i].UpdateIconBySklTable(RASkills[0].BulletData);
					break;
				case 3:
					_virtualButton[i].gameObject.SetActive(true);
					_virtualButton[i].UpdateIconByBundlePath(AssetBundleScriptableObject.Instance.m_uiPath + "ui_battleinfo", "UI_battle_button_drive");
					break;
				case 4:
					_virtualButton[i].gameObject.SetActive(false);
					break;
				case 5:
					_virtualButton[i].gameObject.SetActive(false);
					break;
				}
			}
		}
	}

	private void UpdateSkillIcon()
	{
		if (!_virtualButtonInitialized)
		{
			return;
		}
		for (int i = 0; i < _virtualButton.Length; i++)
		{
			if ((bool)_virtualButton[i])
			{
				SKILL_TABLE bulletData;
				switch (i)
				{
				case 1:
					_virtualButton[i].gameObject.SetActive(false);
					bulletData = RASkills[1].BulletData;
					break;
				case 2:
					_virtualButton[i].gameObject.SetActive(false);
					bulletData = RASkills[2].BulletData;
					break;
				default:
					continue;
				}
				_virtualButton[i].UpdateIconBySklTable(bulletData);
			}
		}
	}

	protected void UpdateVirtualButtonDisplay()
	{
		if (!MasterPilot.IsLocalPlayer || !InitializeVirtualButton())
		{
			return;
		}
		for (int i = 0; i < _virtualButton.Length; i++)
		{
			WeaponStruct weaponStruct;
			switch (i)
			{
			default:
				weaponStruct = RASkills[0];
				break;
			case 1:
				weaponStruct = RASkills[1];
				break;
			case 2:
				weaponStruct = RASkills[2];
				break;
			case 3:
			case 4:
			case 5:
				continue;
			}
			if (!_virtualButton[i])
			{
				continue;
			}
			float num = (float)(weaponStruct.BulletData.n_FIRE_SPEED - weaponStruct.LastUseTimer.GetMillisecond()) / 1000f;
			float num2 = (float)(weaponStruct.BulletData.n_RELOAD - weaponStruct.LastUseTimer.GetMillisecond()) / 1000f;
			float maskProgress = (float)weaponStruct.LastUseTimer.GetMillisecond() / (float)weaponStruct.BulletData.n_FIRE_SPEED;
			float num3 = (float)weaponStruct.LastUseTimer.GetMillisecond() / (float)weaponStruct.BulletData.n_RELOAD;
			switch (weaponStruct.BulletData.n_MAGAZINE_TYPE)
			{
			case 0:
				if (weaponStruct.MagazineRemain > 0f)
				{
					_virtualButton[i].SetBulletText(string.Format("{0}/{1}", weaponStruct.MagazineRemain, weaponStruct.BulletData.n_MAGAZINE));
					if ((float)weaponStruct.LastUseTimer.GetMillisecond() < (float)weaponStruct.BulletData.n_FIRE_SPEED && weaponStruct.BulletData.n_FIRE_SPEED >= 600)
					{
						_virtualButton[i].SetMaskProgress(maskProgress);
						_virtualButton[i].SetMaskText(string.Format("{0:N1}", num));
					}
					else
					{
						_virtualButton[i].SetMaskProgress(1f);
						_virtualButton[i].SetMaskText("");
					}
					if (weaponStruct.MagazineRemain != (float)weaponStruct.BulletData.n_MAGAZINE && weaponStruct.LastUseTimer.GetMillisecond() > OrangeConst.AUTO_RELOAD)
					{
						_virtualButton[i].SetProgress((float)(weaponStruct.LastUseTimer.GetMillisecond() - OrangeConst.AUTO_RELOAD) / ((float)(weaponStruct.BulletData.n_RELOAD * OrangeConst.AUTO_RELOAD_PERCENT) * 0.01f));
						if (weaponStruct.LastUseTimer.GetMillisecond() - OrangeConst.AUTO_RELOAD >= weaponStruct.BulletData.n_RELOAD)
						{
							weaponStruct.MagazineRemain = weaponStruct.BulletData.n_MAGAZINE;
						}
					}
					else
					{
						_virtualButton[i].SetProgress(0f);
					}
				}
				else
				{
					_virtualButton[i].SetBulletText(string.Format("{0}/{1}", weaponStruct.MagazineRemain, weaponStruct.BulletData.n_MAGAZINE), Color.red);
					_virtualButton[i].SetProgress(num3);
					_virtualButton[i].SetMaskProgress(num3);
					_virtualButton[i].SetMaskText(string.Format("{0:N1}", num2));
					if (weaponStruct.LastUseTimer.GetMillisecond() >= weaponStruct.BulletData.n_RELOAD)
					{
						weaponStruct.MagazineRemain = weaponStruct.BulletData.n_MAGAZINE;
					}
				}
				break;
			case 1:
				if (!weaponStruct.ForceLock)
				{
					if ((float)weaponStruct.LastUseTimer.GetMillisecond() < (float)weaponStruct.BulletData.n_FIRE_SPEED && weaponStruct.BulletData.n_FIRE_SPEED >= 600)
					{
						_virtualButton[i].SetMaskProgress(maskProgress);
						_virtualButton[i].SetMaskText(string.Format("{0:N1}", num));
					}
					else
					{
						_virtualButton[i].SetMaskProgress(1f);
						_virtualButton[i].SetMaskText("");
					}
					if (weaponStruct.MagazineRemain >= (float)weaponStruct.BulletData.n_USE_COST)
					{
						_virtualButton[i].SetBulletText(string.Format("{0:N1}", weaponStruct.MagazineRemain));
					}
					else
					{
						_virtualButton[i].SetBulletText(string.Format("{0:N1}", weaponStruct.MagazineRemain), Color.yellow);
					}
					_virtualButton[i].SetProgress(weaponStruct.MagazineRemain / (float)weaponStruct.BulletData.n_MAGAZINE);
				}
				else
				{
					_virtualButton[i].SetBulletText(string.Format("Overheat {0:N2}", num2), Color.red);
					_virtualButton[i].SetBulletText("0", Color.red);
					_virtualButton[i].SetProgress(num3);
					_virtualButton[i].SetMaskProgress(num3);
					_virtualButton[i].SetMaskText(string.Format("{0:N1}", num2));
				}
				break;
			}
		}
	}

	public void UpdateDirection(int pDirection)
	{
		ModelTransform.localScale = new Vector3(1f, 1f, pDirection);
	}

	public void UpdateFunc()
	{
		if (Activate && (int)Hp > 0)
		{
			base.transform.localPosition = Vector3.MoveTowards(base.transform.localPosition, Controller.LogicPosition.vec3, distanceDelta);
		}
	}

	public void AddShift(VInt3 pForce)
	{
		_velocityShift += pForce;
	}

	public override void Update_AutoAim(PlayerAutoAimSystem mPAAS)
	{
		float num = 0f;
		for (int i = 0; i < 3; i++)
		{
			if (RASkills[i].BulletData.s_MODEL != "null" && RASkills[i].BulletData.s_MODEL != "DUMMY" && RASkills[i].BulletData.s_MODEL != "" && RASkills[i].BulletData.f_DISTANCE > num)
			{
				num = RASkills[i].BulletData.f_DISTANCE;
			}
		}
		mPAAS.UpdateAimRange(num);
	}

	public void LateUpdateFunc()
	{
		if ((int)Hp <= 0 || !(MasterPilot == null) || _mainStatus != 0 || !bCanRide)
		{
			return;
		}
		Bounds tAB = Controller.Collider2D.bounds;
		foreach (OrangeCharacter runPlayer in StageUpdate.runPlayers)
		{
			if (runPlayer.IsDead() || runPlayer.UsingVehicle || runPlayer.bIsNpcCpy || runPlayer.selfBuffManager.CheckHasEffect(107))
			{
				continue;
			}
			Bounds tBB = runPlayer.Controller.Collider2D.bounds;
			if (!StageResManager.CheckBoundsIntersectNoZEffect(ref tAB, ref tBB))
			{
				continue;
			}
			if (!runPlayer.EnterRideArmorEvt(this))
			{
				break;
			}
			if (!InitializedMaxHP)
			{
				Hp = (MaxHp = (int)((float)(int)runPlayer.MaxHp * ((float)tVEHICLE_TABLE.n_HP / 100f)));
				InitializedMaxHP = true;
			}
			LinkControlToOC(runPlayer);
			MonoBehaviourSingleton<InputManager>.Instance.VirtualPadSystem.GetButton(ButtonId.SHOOT).ClearStick();
			if (MasterPilot.IsLocalPlayer)
			{
				MonoBehaviourSingleton<OrangeSceneManager>.Instance.MainCamera.GetComponent<CameraControl>().Target = Controller;
			}
			tObjInfoBar = base.transform.GetComponentInChildren<ObjInfoBar>();
			if (tObjInfoBar == null)
			{
				StageResManager.CreateHpBarToRideArmor(this);
			}
			MasterPilot.selfBuffManager.UpdateBuffBar += tObjInfoBar.UpdateBuffCB;
			ArmorWeaponStatus.CopyWeaponStatus(MasterPilot.PlayerWeapons[MasterPilot.WeaponCurrent].weaponStatus, 128, MasterPilot.PlayerWeapons[MasterPilot.WeaponCurrent].weaponStatus.nWeaponType);
			rigidbody.simulated = true;
			if (!base.gameObject.GetComponent<LockRangeObj>())
			{
				mLockRangeObj = base.gameObject.AddComponent<LockRangeObj>();
				mLockRangeObj.Init();
				LockRangeObj component = runPlayer.GetComponent<LockRangeObj>();
				if ((bool)component)
				{
					mLockRangeObj.vLockLR = component.vLockLR;
					mLockRangeObj.vLockTB = component.vLockTB;
					mLockRangeObj.nNoBack = component.nNoBack;
					mLockRangeObj.nReserveBack = component.nReserveBack;
					mLockRangeObj.vReserveBackLR = component.vReserveBackLR;
					mLockRangeObj.vReserveBackTB = component.vReserveBackTB;
				}
			}
			if ((bool)mefx_bootup)
			{
				mefx_bootup.Play();
			}
			InitializeVirtualButton();
			break;
		}
	}

	private void LinkControlToOC(OrangeCharacter tOC)
	{
		tOC.RemoveSelfLRCB();
		tOC.PlayerPressLeftCB = (Callback)Delegate.Combine(tOC.PlayerPressLeftCB, new Callback(PressLeft));
		tOC.PlayerHeldLeftCB = (Callback)Delegate.Combine(tOC.PlayerHeldLeftCB, new Callback(HeldLeft));
		tOC.PlayerReleaseLeftCB = (Callback)Delegate.Combine(tOC.PlayerReleaseLeftCB, new Callback(ReleaseLeftRight));
		tOC.PlayerPressRightCB = (Callback)Delegate.Combine(tOC.PlayerPressRightCB, new Callback(PressRight));
		tOC.PlayerHeldRightCB = (Callback)Delegate.Combine(tOC.PlayerHeldRightCB, new Callback(HeldRight));
		tOC.PlayerReleaseRightCB = (Callback)Delegate.Combine(tOC.PlayerReleaseRightCB, new Callback(ReleaseLeftRight));
		tOC.PlayerPressDashCB = PlayerPressDash;
		tOC.PlayerReleaseDashCB = PlayerReleaseDash;
		tOC.PlayerPressDownCB = PlayerPressDown;
		tOC.PlayerHeldDownCB = NoneCall;
		tOC.PlayerReleaseDownCB = PlayerReleaseDown;
		tOC.PlayerPressJumpCB = PlayerPressJump;
		tOC.PlayerReleaseJumpCB = PlayerReleaseJump;
		tOC.PlayerHeldShootCB = PressShoot;
		tOC.PlayerReleaseShootCB = NoneCall;
		tOC.PlayerPressSelectCB = PressSelect;
		tOC.PlayerPressSkillCB = PressSkill;
		tOC.PlayerHeldSkillCB = NoneCall;
		tOC.PlayerReleaseSkillCB = ReleaseSkill;
	}

	private void NoneCall()
	{
	}

	private void NoneCall(int idx)
	{
	}

	public void LogicUpdate()
	{
		if (MasterPilot == null && (int)Hp > 0)
		{
			LogicUpdatePrepare();
			LogicUpdateCall();
		}
	}

	private void CheckGoToNextAni()
	{
		if (_currentFrame > 1f && nAnimationID < tModelStatusCC.ModelStatusDatas[(int)_mainStatus].sAnimationNames.Length)
		{
			if (fWaitTime > 0f)
			{
				fWaitTime -= GameLogicUpdateManager.m_fFrameLen;
			}
			else
			{
				bAnimationNext = true;
			}
		}
	}

	private bool CheckBelowGoToIdle()
	{
		if ((bool)Controller.BelowInBypassRange)
		{
			if (_mainStatus == MainStatus.Fall)
			{
				PlayBattleSE(BattleSE.CRI_BATTLESE_BT_RIDEARMOR02);
			}
			SetStatus(MainStatus.Idle);
			StopDash();
			_isJump = false;
			return true;
		}
		return false;
	}

	public override void LogicUpdatePrepare()
	{
		selfBuffManager.UpdateBuffTime();
		for (int i = 0; i < RASkills.Length; i++)
		{
			RASkills[i].ChargeTimer += GameLogicUpdateManager.m_fFrameLenMS;
			RASkills[i].LastUseTimer += GameLogicUpdateManager.m_fFrameLenMS;
		}
		_dashTimer += GameLogicUpdateManager.m_fFrameLen;
		_LastFrame = _currentFrame;
		_currentFrame = _animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
		if (_currentFrame > 1f && nAnimationID < tModelStatusCC.ModelStatusDatas[(int)_mainStatus].sAnimationNames.Length)
		{
			if (fWaitTime > 0f)
			{
				fWaitTime -= GameLogicUpdateManager.m_fFrameLen;
			}
			else
			{
				bAnimationNext = true;
			}
		}
		else if (_currentFrame > 1f)
		{
			bAnimationEnd = true;
		}
		switch (_mainStatus)
		{
		case MainStatus.BootUp:
			if (bAnimationEnd && Controller.Collisions.below)
			{
				SetStatus(MainStatus.Idle);
			}
			break;
		case MainStatus.Shutdown:
			if (bAnimationEnd && Controller.Collisions.below)
			{
				SetStatus(MainStatus.Empty);
			}
			break;
		case MainStatus.Empty:
			if ((bool)MasterPilot)
			{
				SetStatus(MainStatus.BootUp);
				PlayBattleSE(BattleSE.CRI_BATTLESE_BT_RIDEARMOR00);
			}
			break;
		case MainStatus.Idle:
			if (!Controller.BelowInBypassRange)
			{
				SetStatus(MainStatus.Fall);
			}
			break;
		case MainStatus.Jump:
			CheckActDatas(-1);
			if (bAnimationEnd)
			{
				SetStatus(MainStatus.Fall);
			}
			else if (_velocity.y < 1500)
			{
				fWaitTime = 0f;
				CheckGoToNextAni();
			}
			break;
		case MainStatus.Dash:
			CheckActDatas(-1);
			if (bAnimationEnd)
			{
				if (!CheckBelowGoToIdle())
				{
					SetStatus(MainStatus.Fall);
				}
			}
			else if (_dashTimer > MaxDashDistance && _dashTimer - GameLogicUpdateManager.m_fFrameLen < MaxDashDistance)
			{
				StopDash();
				PlaySeByDict(MainStatus.Dash);
				fWaitTime = 0f;
				CheckGoToNextAni();
				StopDashFx();
			}
			break;
		case MainStatus.AirDash:
			CheckActDatas(-1);
			if (bAnimationEnd)
			{
				if (!CheckBelowGoToIdle())
				{
					SetStatus(MainStatus.Fall);
				}
			}
			else if (_dashTimer > MaxDashDistance && _dashTimer - GameLogicUpdateManager.m_fFrameLen < MaxDashDistance)
			{
				PlaySeByDict(MainStatus.Dash);
				StopDash();
				fWaitTime = 0f;
				CheckGoToNextAni();
				StopDashFx();
			}
			break;
		case MainStatus.Fall:
			CheckBelowGoToIdle();
			break;
		case MainStatus.Attack:
			CheckActDatas(0);
			if (bAnimationEnd)
			{
				SetStatus(MainStatus.Idle);
			}
			break;
		case MainStatus.Skill0:
			CheckActDatas(1);
			if (bAnimationEnd)
			{
				SetStatus(MainStatus.Idle);
			}
			break;
		case MainStatus.Skill1:
			CheckActDatas(2);
			if (bAnimationEnd)
			{
				SetStatus(MainStatus.Idle);
			}
			break;
		}
		if (bAnimationNext)
		{
			bAnimationNext = false;
			nAnimationID++;
			fWaitTime = 0f;
			_LastFrame = -1f;
			_currentFrame = 0f;
			bAnimationEnd = false;
			CheckActDatas((int)(_mainStatus - 9));
			if (tModelStatusCC.ModelStatusDatas[(int)_mainStatus].sAnimationNames.Length > nAnimationID - 1)
			{
				_animator.Play(tModelStatusCC.ModelStatusDatas[(int)_mainStatus].sAnimationNames[nAnimationID - 1], 0);
			}
		}
	}

	private void CheckActDatas(int nAtkID)
	{
		if (tModelStatusCC.ModelStatusDatas[(int)_mainStatus].fActTimeDatas.Length != 0)
		{
			for (int i = 0; i < tModelStatusCC.ModelStatusDatas[(int)_mainStatus].fActTimeDatas.Length; i++)
			{
				if (nAnimationID != tModelStatusCC.ModelStatusDatas[(int)_mainStatus].fActTimeDatas[i].nActIndex || !(_LastFrame < tModelStatusCC.ModelStatusDatas[(int)_mainStatus].fActTimeDatas[i].fActionTime) || !(_currentFrame >= tModelStatusCC.ModelStatusDatas[(int)_mainStatus].fActTimeDatas[i].fActionTime))
				{
					continue;
				}
				fWaitTime = tModelStatusCC.ModelStatusDatas[(int)_mainStatus].fActTimeDatas[i].fWaitTime;
				if (tModelStatusCC.ModelStatusDatas[(int)_mainStatus].fActTimeDatas[i].bTriggerSkill && nAtkID >= 0 && nAtkID <= 2)
				{
					if (RASkills[nAtkID].BulletData.s_MODEL == "null" || RASkills[nAtkID].BulletData.s_MODEL == "DUMMY" || RASkills[nAtkID].BulletData.s_MODEL == "")
					{
						if (RASkills[nAtkID].BulletData.s_FIELD != "null")
						{
							tCollideBullet = MonoBehaviourSingleton<PoolManager>.Instance.GetPoolObj<CollideBullet>("PoolColliderBullet");
							tCollideBullet.isForceSE = true;
							tCollideBullet.transform.SetParent(base.transform, false);
							tCollideBullet.transform.localPosition = Vector3.zero;
							tCollideBullet.UpdateBulletData(RASkills[nAtkID].BulletData, MasterPilot.sPlayerName, MasterPilot.GetNowRecordNO(), 0, (int)base._characterDirection);
							tCollideBullet.SetBulletAtk(ArmorWeaponStatus, MasterPilot.selfBuffManager.sBuffStatus);
							tCollideBullet.BulletLevel = MasterPilot.PlayerWeapons[MasterPilot.WeaponCurrent].SkillLV;
							tCollideBullet.Active(MasterPilot.TargetMask);
						}
					}
					else if (tVEHICLE_TABLE.n_ID == 2 || tVEHICLE_TABLE.n_ID == 3 || tVEHICLE_TABLE.n_ID == 5)
					{
						for (int j = 0; j < 2; j++)
						{
							CreateBulletUseParam.ZeroParam();
							CreateBulletUseParam.tSkillTable = RASkills[nAtkID].BulletData;
							CreateBulletUseParam.weaponStatus = ArmorWeaponStatus;
							CreateBulletUseParam.tBuffStatus = MasterPilot.selfBuffManager.sBuffStatus;
							CreateBulletUseParam.pTransform = AtkTransSpecial[j];
							Vector3 pDirection = Vector2.right * (float)base._characterDirection;
							if ((bool)MasterPilot.PlayerAutoAimSystem && MasterPilot.PlayerAutoAimSystem.AutoAimTarget != null)
							{
								pDirection = (MasterPilot.PlayerAutoAimSystem.GetTargetPoint().xy() - AtkTransSpecial[j].position.xy()).normalized;
							}
							CreateBulletUseParam.pDirection = pDirection;
							CreateBulletUseParam.nRecordID = MasterPilot.GetNowRecordNO();
							CreateBulletUseParam.pTargetMask = MasterPilot.TargetMask;
							CreateBulletUseParam.nNetID = MasterPilot.nBulletRecordID++;
							int num = Math.Sign(pDirection.x);
							if (base._characterDirection != (CharacterDirection)num && Mathf.Abs(pDirection.x) > 0.05f)
							{
								base._characterDirection = (CharacterDirection)((int)base._characterDirection * -1);
								UpdateDirection((int)base._characterDirection);
							}
							CreateBulletUseParam.nDirection = (int)base._characterDirection;
							BulletBase.TryShotBullet(CreateBulletUseParam);
						}
					}
					else
					{
						CreateBulletUseParam.ZeroParam();
						CreateBulletUseParam.tSkillTable = RASkills[nAtkID].BulletData;
						CreateBulletUseParam.weaponStatus = ArmorWeaponStatus;
						CreateBulletUseParam.tBuffStatus = MasterPilot.selfBuffManager.sBuffStatus;
						CreateBulletUseParam.pTransform = AtkShotTrans[nAtkID];
						Vector3 pDirection2 = MasterPilot.ShootDirection;
						if ((bool)MasterPilot.PlayerAutoAimSystem && MasterPilot.PlayerAutoAimSystem.AutoAimTarget != null)
						{
							pDirection2 = (MasterPilot.PlayerAutoAimSystem.GetTargetPoint().xy() - AtkShotTrans[nAtkID].position.xy()).normalized;
						}
						CreateBulletUseParam.pDirection = pDirection2;
						CreateBulletUseParam.nRecordID = MasterPilot.GetNowRecordNO();
						CreateBulletUseParam.pTargetMask = MasterPilot.TargetMask;
						CreateBulletUseParam.nNetID = MasterPilot.nBulletRecordID++;
						CreateBulletUseParam.nDirection = (int)base._characterDirection;
						BulletBase.TryShotBullet(CreateBulletUseParam);
					}
				}
				if (tModelStatusCC.ModelStatusDatas[(int)_mainStatus].fActTimeDatas[i].bShowFX && tModelStatusCC.ModelStatusDatas[(int)_mainStatus].fActTimeDatas[i].sFxName != "")
				{
					if (tModelStatusCC.ModelStatusDatas[(int)_mainStatus].fActTimeDatas[i].bFXFloow)
					{
						MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(tModelStatusCC.ModelStatusDatas[(int)_mainStatus].fActTimeDatas[i].sFxName, ModelTransform, Quaternion.identity, Array.Empty<object>());
					}
					else
					{
						MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(tModelStatusCC.ModelStatusDatas[(int)_mainStatus].fActTimeDatas[i].sFxName, ModelTransform.position, Quaternion.identity, Array.Empty<object>());
					}
				}
				if (tModelStatusCC.ModelStatusDatas[(int)_mainStatus].fActTimeDatas[i].bZeroX)
				{
					_velocity.x = 0;
				}
				if (tModelStatusCC.ModelStatusDatas[(int)_mainStatus].fActTimeDatas[i].bZeroY)
				{
					_velocity.y = 0;
				}
				if (tModelStatusCC.ModelStatusDatas[(int)_mainStatus].fActTimeDatas[i].bZeroCollideBullet && tCollideBullet.IsActivate)
				{
					tCollideBullet.BackToPool();
				}
				MainStatus mainStatus = _mainStatus;
				if ((uint)(mainStatus - 4) <= 2u)
				{
					SetSpeed(_velocity.x + (int)base._characterDirection * tModelStatusCC.ModelStatusDatas[(int)_mainStatus].fActTimeDatas[i].vx * tVEHICLE_TABLE.n_SPEED / 100, _velocity.y + tModelStatusCC.ModelStatusDatas[(int)_mainStatus].fActTimeDatas[i].vy);
				}
				else
				{
					SetSpeed(_velocity.x + (int)base._characterDirection * tModelStatusCC.ModelStatusDatas[(int)_mainStatus].fActTimeDatas[i].vx, _velocity.y + tModelStatusCC.ModelStatusDatas[(int)_mainStatus].fActTimeDatas[i].vy);
				}
				bFly = tModelStatusCC.ModelStatusDatas[(int)_mainStatus].fActTimeDatas[i].bFly;
			}
		}
		else
		{
			SetSpeed(0, _velocity.y);
			bFly = false;
		}
	}

	public override void LogicUpdateCall()
	{
		if (MasterPilot == null)
		{
			MainStatus mainStatus = _mainStatus;
			if ((uint)(mainStatus - 3) <= 3u || (uint)(mainStatus - 9) <= 2u)
			{
				SetStatus(Controller.Collisions.below ? MainStatus.Shutdown : MainStatus.Fall);
				SetSpeed(0, 0);
			}
		}
		UpdateDirection((int)base._characterDirection);
		UpdateGravity();
		_velocityExtra.z = 0;
		Controller.Move((_velocity + OrangeBattleUtility.GlobalVelocityExtra + _velocityExtra) * GameLogicUpdateManager.m_fFrameLen + _velocityShift);
		distanceDelta = Vector3.Distance(base.transform.localPosition, Controller.LogicPosition.vec3) * (Time.deltaTime / GameLogicUpdateManager.m_fFrameLen);
		_velocityExtra = VInt3.zero;
		_velocityShift = VInt3.zero;
		if ((bool)MasterPilot)
		{
			UpdateWeaponIcon();
			UpdateSkillIcon();
			UpdateVirtualButtonDisplay();
			MasterPilot._characterDirection = base._characterDirection;
		}
	}

	private void SetStatus(MainStatus mainStatus)
	{
		switch (_mainStatus)
		{
		case MainStatus.AirDash:
		case MainStatus.Dash:
			StopDashFx();
			break;
		}
		_mainStatus = mainStatus;
		switch (_mainStatus)
		{
		case MainStatus.AirDash:
		case MainStatus.Dash:
			_animator.speed = (float)tVEHICLE_TABLE.n_SPEED / 100f;
			break;
		case MainStatus.Walk:
			_animator.speed = (float)tVEHICLE_TABLE.n_SPEED / 100f;
			break;
		case MainStatus.Jump:
			_animator.speed = 1f;
			break;
		default:
			_animator.speed = 1f;
			break;
		}
		UpdateAnimation();
	}

	private void UpdateAnimation()
	{
		nAnimationID = 1;
		fWaitTime = 0f;
		_LastFrame = -1f;
		_currentFrame = 0f;
		bAnimationEnd = false;
		bAnimationNext = false;
		CheckActDatas((int)(_mainStatus - 9));
		_animator.Play(tModelStatusCC.ModelStatusDatas[(int)_mainStatus].sAnimationNames[nAnimationID - 1], 0);
	}

	public void AddForce(VInt3 pForce)
	{
		_velocityExtra += pForce;
	}

	protected void UpdateGravity()
	{
		if (IgnoreGravity)
		{
			_velocity.y = 0;
			return;
		}
		if ((_velocity.y < 0 && Controller.Collisions.below) || (_velocity.y > 0 && Controller.Collisions.above))
		{
			_velocity.y = 0;
		}
		if (bFly)
		{
			if (_velocity.y < 0)
			{
				_velocity.y = 0;
			}
		}
		else
		{
			_velocity.y += OrangeBattleUtility.FP_Gravity * GameLogicUpdateManager.g_fixFrameLenFP / 1000;
			_velocity.y = IntMath.Sign(_velocity.y) * IntMath.Min(IntMath.Abs(_velocity.y), IntMath.Abs(_maxGravity.i));
		}
	}

	public void SetSpeed(int sx, int sy)
	{
		_velocity.x = sx;
		_velocity.y = sy;
	}

	public override void StopRideObj()
	{
		SetStatus(MainStatus.Idle);
		SetSpeed(0, _velocity.y);
	}

	public void PressSelect()
	{
		UnRide(false);
	}

	public override void UnRide(bool bDisadle)
	{
		if (MasterPilot != null)
		{
			_virtualButton[0].ClearStick();
			_virtualButton[1].ClearStick();
			_virtualButton[2].ClearStick();
			_isJump = false;
			_isDash = false;
			_isHeldDash = false;
			if (tObjInfoBar != null)
			{
				MasterPilot.selfBuffManager.UpdateBuffBar -= tObjInfoBar.UpdateBuffCB;
			}
			MasterPilot.LeaveRideArmorEvt(this);
			if (tObjInfoBar != null)
			{
				tObjInfoBar.RemoveBar(this);
				tObjInfoBar.gameObject.SetActive(false);
				StageResManager.RemoveInfoBar(tObjInfoBar);
				tObjInfoBar = null;
			}
			rigidbody.simulated = false;
			if ((bool)mLockRangeObj)
			{
				UnityEngine.Object.Destroy(mLockRangeObj);
				mLockRangeObj = null;
			}
			if ((bool)mefx_bootup)
			{
				mefx_bootup.Stop();
			}
			RecoverCollideBullet();
		}
	}

	public void PressLeft()
	{
		switch (_mainStatus)
		{
		case MainStatus.Idle:
			if (!Controller.Collisions.left)
			{
				base._characterDirection = CharacterDirection.LEFT;
				SetStatus(MainStatus.Walk);
			}
			break;
		case MainStatus.Walk:
			if (base._characterDirection != CharacterDirection.LEFT)
			{
				base._characterDirection = CharacterDirection.LEFT;
				SetStatus(MainStatus.Walk);
			}
			break;
		case MainStatus.Jump:
			if (base._characterDirection != CharacterDirection.LEFT)
			{
				base._characterDirection = CharacterDirection.LEFT;
				_velocity.x = -_velocity.x;
			}
			break;
		case MainStatus.Fall:
			if (base._characterDirection != CharacterDirection.LEFT)
			{
				base._characterDirection = CharacterDirection.LEFT;
				_velocity.x = -_velocity.x;
			}
			break;
		case MainStatus.AirDash:
		case MainStatus.Dash:
			break;
		}
	}

	public void PressRight()
	{
		switch (_mainStatus)
		{
		case MainStatus.Idle:
			if (!Controller.Collisions.right)
			{
				base._characterDirection = CharacterDirection.RIGHT;
				SetStatus(MainStatus.Walk);
			}
			break;
		case MainStatus.Walk:
			if (base._characterDirection != CharacterDirection.RIGHT)
			{
				base._characterDirection = CharacterDirection.RIGHT;
				SetStatus(MainStatus.Walk);
			}
			break;
		case MainStatus.Jump:
			if (base._characterDirection != CharacterDirection.RIGHT)
			{
				base._characterDirection = CharacterDirection.RIGHT;
				_velocity.x = -_velocity.x;
			}
			break;
		case MainStatus.Fall:
			if (base._characterDirection != CharacterDirection.RIGHT)
			{
				base._characterDirection = CharacterDirection.RIGHT;
				_velocity.x = -_velocity.x;
			}
			break;
		case MainStatus.AirDash:
		case MainStatus.Dash:
			break;
		}
	}

	public void HeldLeft()
	{
		switch (_mainStatus)
		{
		case MainStatus.Idle:
			if (!Controller.Collisions.left)
			{
				base._characterDirection = CharacterDirection.LEFT;
				SetStatus(MainStatus.Walk);
			}
			break;
		case MainStatus.Walk:
			if (base._characterDirection != CharacterDirection.LEFT)
			{
				base._characterDirection = CharacterDirection.LEFT;
				SetStatus(MainStatus.Walk);
			}
			break;
		}
	}

	public void HeldRight()
	{
		switch (_mainStatus)
		{
		case MainStatus.Idle:
			if (!Controller.Collisions.right)
			{
				base._characterDirection = CharacterDirection.RIGHT;
				SetStatus(MainStatus.Walk);
			}
			break;
		case MainStatus.Walk:
			if (base._characterDirection != CharacterDirection.RIGHT)
			{
				base._characterDirection = CharacterDirection.RIGHT;
				SetStatus(MainStatus.Walk);
			}
			break;
		}
	}

	public void ReleaseLeftRight()
	{
		MainStatus mainStatus = _mainStatus;
		if (mainStatus == MainStatus.Walk)
		{
			SetStatus(MainStatus.Idle);
		}
	}

	protected void PlayerPressJump()
	{
		MainStatus mainStatus = _mainStatus;
		if ((uint)mainStatus <= 1u || (uint)(mainStatus - 8) <= 3u)
		{
			return;
		}
		if (_doubleTapBtn == ButtonId.DOWN && Controller.Collisions.below && Controller.Collisions.JSB_below)
		{
			Controller.JumpThrough = true;
		}
		else if (!_isJump)
		{
			if (_isHeldDash)
			{
				_isDash = true;
			}
			if (_isDash)
			{
				_dashChance--;
			}
			_isJump = true;
			MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_headcrush_001", base.transform.position, Quaternion.identity, Array.Empty<object>());
			SetStatus(MainStatus.Jump);
			PlayBattleSE(BattleSE.CRI_BATTLESE_BT_RIDEARMOR04);
		}
	}

	protected void PlayerReleaseDown()
	{
		_doubleTapBtn = ButtonId.NONE;
	}

	protected void PlayerPressDown()
	{
		if (setting.DoubleTapThrough != 0 && _doubleTapTimer.GetMillisecond() < 200 && _doubleTapBtn == ButtonId.DOWN && Controller.Collisions.below && Controller.Collisions.JSB_below)
		{
			Controller.JumpThrough = true;
		}
		_doubleTapTimer.TimerStart();
		_doubleTapBtn = ButtonId.DOWN;
	}

	protected void PlayerReleaseJump()
	{
		MainStatus mainStatus = _mainStatus;
		int num = 7;
	}

	protected void PlayerPressDash(object tParam)
	{
		CharacterDirection characterDirection = base._characterDirection;
		_isHeldDash = true;
		PlayerPressDash(characterDirection);
	}

	protected void PlayerPressDash(CharacterDirection destFacing)
	{
		MainStatus mainStatus = _mainStatus;
		if ((uint)mainStatus <= 1u || (uint)(mainStatus - 9) <= 2u || ((destFacing == CharacterDirection.RIGHT) ? Controller.Collisions.right : Controller.Collisions.left))
		{
			return;
		}
		if ((bool)Controller.BelowInBypassRange && !_isJump)
		{
			if (!_isDash)
			{
				base._characterDirection = destFacing;
				PlayBattleSE(BattleSE.CRI_BATTLESE_BT_RIDEARMOR03);
				SetStatus(MainStatus.Dash);
				_isDash = true;
				_dashTimer = 0f;
				PlayDashFx();
			}
		}
		else if (_airDashEnable && _dashChance > 0 && (_mainStatus == MainStatus.Jump || _mainStatus == MainStatus.Fall))
		{
			base._characterDirection = destFacing;
			PlayBattleSE(BattleSE.CRI_BATTLESE_BT_RIDEARMOR03);
			SetStatus(MainStatus.AirDash);
			_dashChance--;
			_isDash = true;
			_dashTimer = 0f;
			PlayDashFx();
		}
	}

	private void PlayDashFx()
	{
		if ((bool)efx_dash0)
		{
			efx_dash0.Play();
		}
		if ((bool)efx_dash1)
		{
			efx_dash1.Play();
		}
		if ((bool)efx_dash2)
		{
			efx_dash2.Play();
		}
	}

	private void StopDashFx()
	{
		if ((bool)efx_dash0)
		{
			efx_dash0.Stop();
		}
		if ((bool)efx_dash1)
		{
			efx_dash1.Stop();
		}
		if ((bool)efx_dash2)
		{
			efx_dash2.Stop();
		}
	}

	protected IEnumerator WaitMinDash()
	{
		while (_dashTimer < _minDashTime && _dashTimer < MaxDashDistance)
		{
			yield return CoroutineDefine._waitForEndOfFrame;
		}
		_isHeldDash = false;
		if (_isDash)
		{
			StopDash();
			MainStatus mainStatus = _mainStatus;
			if ((uint)(mainStatus - 5) <= 1u && !CheckBelowGoToIdle())
			{
				SetStatus(MainStatus.Fall);
			}
		}
		yield return CoroutineDefine._waitForEndOfFrame;
	}

	protected void PlayerReleaseDash()
	{
		_isHeldDash = false;
		if (_isDash)
		{
			StopDash();
			MainStatus mainStatus = _mainStatus;
			if ((uint)(mainStatus - 5) <= 1u && !CheckBelowGoToIdle())
			{
				SetStatus(MainStatus.Fall);
			}
		}
	}

	protected void StopDash()
	{
		if ((bool)Controller.BelowInBypassRange)
		{
			_dashChance = MaxDashChance;
		}
		_isDash = false;
	}

	protected void PressShoot()
	{
		MainStatus mainStatus = _mainStatus;
		if (((uint)(mainStatus - 3) <= 1u || mainStatus == MainStatus.Dash) && RASkills[0].LastUseTimer.GetMillisecond() >= RASkills[0].BulletData.n_FIRE_SPEED && !(RASkills[0].MagazineRemain <= 0f) && !RASkills[0].ForceLock)
		{
			RASkills[0].MagazineRemain -= RASkills[0].BulletData.n_USE_COST;
			SetStatus(MainStatus.Attack);
			RASkills[0].LastUseTimer.TimerStart();
			PlaySeByDict(MainStatus.Attack);
		}
	}

	protected void PressSkill(int id)
	{
	}

	protected void ReleaseSkill(int id)
	{
		MainStatus mainStatus = _mainStatus;
		if (((uint)(mainStatus - 3) <= 1u || mainStatus == MainStatus.Dash) && RASkills[id + 1].LastUseTimer.GetMillisecond() >= RASkills[id + 1].BulletData.n_FIRE_SPEED && !(RASkills[id + 1].MagazineRemain <= 0f) && !RASkills[id + 1].ForceLock)
		{
			RASkills[id + 1].MagazineRemain -= RASkills[id + 1].BulletData.n_USE_COST;
			SetStatus((MainStatus)(10 + id));
			RASkills[id + 1].LastUseTimer.TimerStart();
		}
	}

	public override int GetSOBType()
	{
		return 3;
	}

	public override ObscuredInt GetDOD(int nCurrentWeapon)
	{
		if (MasterPilot == null)
		{
			return 0;
		}
		return MasterPilot.GetDOD(nCurrentWeapon);
	}

	public override ObscuredInt GetDEF(int nCurrentWeapon)
	{
		if (MasterPilot == null)
		{
			return 0;
		}
		return MasterPilot.GetDEF(nCurrentWeapon);
	}

	public override ObscuredInt GetReduceCriPercent(int nCurrentWeapon)
	{
		if (MasterPilot == null)
		{
			return 0;
		}
		return MasterPilot.GetReduceCriPercent(nCurrentWeapon);
	}

	public override ObscuredInt GetReduceCriDmgPercent(int nCurrentWeapon)
	{
		if (MasterPilot == null)
		{
			return 0;
		}
		return MasterPilot.GetReduceCriDmgPercent(nCurrentWeapon);
	}

	public override ObscuredInt GetBlock()
	{
		if (MasterPilot == null)
		{
			return 0;
		}
		return MasterPilot.GetBlock();
	}

	public override ObscuredInt GetBlockDmgPercent()
	{
		if (MasterPilot == null)
		{
			return 0;
		}
		return MasterPilot.GetBlockDmgPercent();
	}

	public override ObscuredInt Hurt(HurtPassParam tHurtPassParam)
	{
		if (MasterPilot != null)
		{
			if ((int)tHurtPassParam.dmg >= 0)
			{
				MasterPilot.lastCreateBulletWeaponStatus = ArmorWeaponStatus;
				MasterPilot.lastCreateBulletTransform = MasterPilot.PlayerWeapons[MasterPilot.WeaponCurrent].ShootTransform[0];
				if (MasterPilot.IsLocalPlayer)
				{
					MasterPilot.tRefPassiveskill.HurtTrigger(ref tHurtPassParam.dmg, ArmorWeaponStatus.nWeaponCheck, ref MasterPilot.selfBuffManager, MasterPilot.CreateBulletByLastWSTranform);
				}
			}
			tHurtPassParam.dmg = MasterPilot.selfBuffManager.ReduceDmgByEnergyShild(tHurtPassParam.dmg);
		}
		Hp = (int)Hp - (int)tHurtPassParam.dmg;
		UpdateHurtAction();
		if ((int)Hp > 0)
		{
			_characterMaterial.Hurt();
		}
		else
		{
			if (!StageUpdate.gbIsNetGame || MasterPilot.IsLocalPlayer)
			{
				PlayerDead();
			}
			else
			{
				bWaitDead = true;
				if (DeadWaitCoroutine != null)
				{
					StopCoroutine(DeadWaitCoroutine);
				}
				DeadWaitCoroutine = StartCoroutine(WaitDead());
			}
			Hp = 0;
		}
		return Hp;
	}

	protected IEnumerator WaitDead(float fWaitTimeSet = 1f)
	{
		float fWaitTime = fWaitTimeSet;
		yield return CoroutineDefine._waitForEndOfFrame;
		while (bWaitDead)
		{
			fWaitTime -= Time.deltaTime;
			if (fWaitTime <= 0f)
			{
				break;
			}
			yield return CoroutineDefine._waitForEndOfFrame;
		}
		if (bWaitDead)
		{
			PlayerDead();
			bWaitDead = false;
		}
		DeadWaitCoroutine = null;
	}

	public void PlayerDead()
	{
		tModelStatusCC.explosionFxInfo.Play(ModelTransform.position);
		if ((bool)tModelStatusCC.ExplosionPart)
		{
			EnemyDieCollider poolObj = MonoBehaviourSingleton<PoolManager>.Instance.GetPoolObj<EnemyDieCollider>(tModelStatusCC.ExplosionPart.Name);
			poolObj.transform.position = ModelTransform.position;
			poolObj.ActiveExplosion();
		}
		base.SoundSource.PlaySE("HitSE", 103);
		IgnoreGravity = true;
		Controller.Collider2D.enabled = false;
		_velocity = VInt3.zero;
		PressSelect();
		_characterMaterial.Disappear();
		StopDashFx();
		if ((bool)_animator)
		{
			_animator.enabled = false;
		}
		StageResManager.GetStageUpdate().UnRegisterStageObjBase(this);
	}

	public override void SetStun(bool enable, bool bCheckOtherObj = true)
	{
		if ((bool)MasterPilot && !enable)
		{
			MasterPilot.InitImportantVar();
		}
	}

	public void RecoverCollideBullet()
	{
		if (tCollideBullet != null && tCollideBullet.IsActivate)
		{
			tCollideBullet.BackToPool();
		}
	}

	public void PlayBattleSE(BattleSE CueName)
	{
		if ((int)Hp > 0)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlayBattleSE(CueName);
		}
	}
}
