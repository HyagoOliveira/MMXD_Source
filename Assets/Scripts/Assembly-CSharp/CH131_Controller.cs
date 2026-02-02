using System;
using StageLib;
using UnityEngine;

public class CH131_Controller : CharacterControlBase
{
	private int nowFrame;

	private int skillEventFrame;

	private int endFrame;

	private int endBreakFrame;

	private bool isSkillEventEnd;

	private SKILL_TABLE linkSkl0;

	protected ShieldBullet _pShieldBullet;

	private readonly int ShieldBuffId = -1;

	private SkinnedMeshRenderer HandMesh_L_c;

	private SkinnedMeshRenderer HandMesh_L_m;

	private SkinnedMeshRenderer busterMesh;

	private CharacterMaterial ch001CM;

	private CharacterMaterial weaponSwordR;

	private bool isConsoleCharacter = true;

	private bool isTeleportInStart;

	private bool isTeleportInChange;

	private readonly int _hashDirection = Animator.StringToHash("fDirection");

	private readonly int _hashEquip = Animator.StringToHash("fEquip");

	private readonly int _crouchEndShootHash = Animator.StringToHash("crouch_atk");

	private readonly int _dashStart = Animator.StringToHash("dash_start");

	private readonly int _dashAtkLoop = Animator.StringToHash("dash_atk_loop");

	private Transform flipModel;

	private Animator flipModelAnimator;

	private Transform[] shootPoint0;

	private Transform[] shootPoint1;

	private Transform[] shootPointOriginal0;

	private Transform[] shootPointOriginal1;

	private CharacterMaterial flipModelCM;

	private CharacterMaterial originalCM;

	private CharacterMaterial[] weaponMesh0;

	private CharacterMaterial[] weaponMesh1;

	private CharacterMaterial[] weaponMeshOriginal0;

	private CharacterMaterial[] weaponMeshOriginal1;

	private ChipSystem[] chipEfxOriginal;

	private ChipSystem[] chipEfx;

	private SkinnedMeshRenderer flipHandMesh_L_c;

	private SkinnedMeshRenderer flipHandMesh_L_m;

	private SkinnedMeshRenderer flipBusterMesh;

	private ParticleSystem psLThrusterParticleSystem;

	private ParticleSystem psRThrusterParticleSystem;

	private ParticleSystem psLThrusterParticleSystemOriginal;

	private ParticleSystem psRThrusterParticleSystemOriginal;

	private Transform ShootPointskill1;

	private Transform ShootPointskill1Original;

	private int[] _animateStatus;

	private bool isInCeiling;

	private int cacheAniId = -1;

	private Vector2 shootDirFrom = new Vector2(0f, -1f);

	private int hangAniEndFrame;

	private Vector3 hangAniOfsset = new Vector3(0f, -0.17f, 0f);

	private Vector3 platformOffset = new Vector3(0f, -0.5f, 0f);

	private Vector2 rayDirTop = Vector2.up;

	private Vector2 rayDirBottom = Vector2.down;

	private Vector2 rayOrginTop = Vector2.zero;

	private Vector2 rayOrginBottom = Vector2.zero;

	private float rayCeilingDist = 0.19f;

	private float rayRabbitDist = 1f;

	private bool moveWithRabbit;

	private bool rabbitMoved;

	private Transform invisibleRabbit;

	private Collider2D colliderRabbitParent;

	private float rabbitCachePosY;

	private readonly int upsideDownBuffId = -2;

	private readonly string FX_0_00 = "fxuse_GigaAttack_000";

	private readonly int SKL0_TRIGGER = (int)(0.263f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL0_TRIGGER2 = (int)(0.4f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL0_END = (int)(0.733f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL0_END_BREAK = (int)(0.58 / (double)GameLogicUpdateManager.m_fFrameLen);

	private readonly int HANG_ANI_END = (int)(0.167f / GameLogicUpdateManager.m_fFrameLen);

	private void OnEnable()
	{
		Singleton<GenericEventManager>.Instance.AttachEvent(EventManager.ID.LOCK_WALL_JUMP, CancelWallClimbing);
	}

	private void OnDisable()
	{
		Singleton<GenericEventManager>.Instance.DetachEvent(EventManager.ID.LOCK_WALL_JUMP, CancelWallClimbing);
	}

	public override int JumpSpeed()
	{
		return Mathf.RoundToInt(OrangeBattleUtility.PlayerJumpSpeed * 1.1f * OrangeBattleUtility.PPU * OrangeBattleUtility.FPS * 1000f);
	}

	public override void Start()
	{
		base.Start();
		InitializeSkill();
	}

	private void InitializeSkill()
	{
		Transform transform = new GameObject("CustomShootPoint0").transform;
		transform.SetParent(base.transform);
		transform.localPosition = new Vector3(0f, 0.8f, 0f);
		Transform[] target = _refEntity._transform.GetComponentsInChildren<Transform>(true);
		_refEntity.ExtraTransforms = new Transform[3];
		_refEntity.ExtraTransforms[0] = OrangeBattleUtility.FindChildRecursive(ref target, "R WeaponPoint", true);
		_refEntity.ExtraTransforms[1] = OrangeBattleUtility.FindChildRecursive(ref target, "L WeaponPoint", true);
		_refEntity.ExtraTransforms[2] = transform;
		_refEntity.PlayerSkills[0].ShootTransform[0] = _refEntity.ExtraTransforms[2];
		_refEntity.PlayerSkills[1].ShootTransform[0] = _refEntity.ExtraTransforms[1];
		Transform transform2 = OrangeBattleUtility.FindChildRecursive(ref target, "CH001", true);
		if ((bool)transform2)
		{
			ch001CM = transform2.GetComponent<CharacterMaterial>();
		}
		Transform transform3 = OrangeBattleUtility.FindChildRecursive(ref target, "HandMesh_L_c", true);
		HandMesh_L_c = transform3.GetComponent<SkinnedMeshRenderer>();
		OrangeBattleUtility.FindChildRecursive(ref target, "HandMesh_L_m", true);
		HandMesh_L_m = transform3.GetComponent<SkinnedMeshRenderer>();
		Transform transform4 = OrangeBattleUtility.FindChildRecursive(ref target, "BusterMesh_m", true);
		busterMesh = transform4.GetComponent<SkinnedMeshRenderer>();
		Transform transform5 = OrangeBattleUtility.FindChildRecursive(ref target, "Saber_008_G_R", true);
		weaponSwordR = transform5.GetComponent<CharacterMaterial>();
		ToggleBusterWeapon(false);
		InstantiateFlipModel();
		ManagedSingleton<CharacterControlHelper>.Instance.PreloadLinkSkl<ShieldBullet>(_refEntity, 0, out linkSkl0);
		isConsoleCharacter = _refEntity as OrangeConsoleCharacter != null;
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(FX_0_00);
		if (!isConsoleCharacter || !MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.IsPvp)
		{
			return;
		}
		_refEntity.selfBuffManager.AddBuff(upsideDownBuffId, 0, 0, 0);
		LeanTween.delayedCall(1f, (Action)delegate
		{
			if (!isInCeiling && _refEntity.selfBuffManager.CheckHasEffectByCONDITIONID(upsideDownBuffId))
			{
				_refEntity.selfBuffManager.RemoveBuffByCONDITIONID(upsideDownBuffId);
			}
		});
	}

	public override void OverrideDelegateEvent()
	{
		base.OverrideDelegateEvent();
		_refEntity.TeleportInCharacterDependEvt = TeleportInCharacterDepend;
		_refEntity.OverrideAnimatorParamtersEvt = OverrideAnimatorParamters;
		_refEntity.SetStatusCharacterDependEvt = SetStatusCharacterDepend;
		_refEntity.TeleportOutCharacterDependEvt = TeleportOutCharacterDepend;
		_refEntity.StageTeleportOutCharacterDependEvt = TeleportOutCharacterDepend;
		_refEntity.EnterRideArmorEvt = EnterRideArmor;
		_refEntity.AddForceFieldCB = AddForceField;
		_refEntity.mapCollisionCB = MapCollisonCB;
		_refEntity.PlayerPressJumpCB = PlayerPressJump;
	}

	public void TeleportInCharacterDepend()
	{
		if (!isTeleportInStart)
		{
			isTeleportInStart = true;
			_refEntity.CharacterMaterials.Disappear(null, 0f);
			ch001CM.Appear(null, 0f);
		}
		if (_refEntity.CurrentFrame >= 0.4f && !isTeleportInChange)
		{
			isTeleportInChange = true;
			ch001CM.Disappear(null, 0f);
			_refEntity.CharacterMaterials.Appear(null, 0f);
		}
	}

	public void TeleportOutCharacterDepend()
	{
		if (isInCeiling)
		{
			UpdateCeilingStatus(false);
		}
	}

	public override void PlayerPressSkillCharacterCall(int id)
	{
		if (_refEntity.CurrentActiveSkill == -1 && id == 0 && !(_pShieldBullet != null) && _refEntity.selfBuffManager.nMeasureNow >= _refEntity.PlayerSkills[id].BulletData.n_USE_COST && _refEntity.CheckUseSkillKeyTrigger(id))
		{
			if (isInCeiling)
			{
				UpdateCeilingStatus(false);
			}
			ToggleSaberWeapon(true);
			endBreakFrame = GameLogicUpdateManager.GameFrame + SKL0_END_BREAK;
			ManagedSingleton<CharacterControlHelper>.Instance.TurnToAimTarget(_refEntity);
			ManagedSingleton<CharacterControlHelper>.Instance.ChangeToSklStatus(_refEntity, id, SKL0_TRIGGER, SKL0_TRIGGER, OrangeCharacter.SubStatus.SKILL0, out skillEventFrame, out endFrame);
			ManagedSingleton<CharacterControlHelper>.Instance.SetAnimate(_refEntity, HumanBase.AnimateId.ANI_SKILL_START, (HumanBase.AnimateId)66u, (HumanBase.AnimateId)67u);
			_refEntity.DisableWeaponMesh(_refEntity.GetCurrentWeaponObj(), 0f);
			_refEntity.CheckUsePassiveSkill(0, _refEntity.PlayerSkills[0].weaponStatus, _refEntity.PlayerSkills[0].ShootTransform[0]);
			_refEntity.selfBuffManager.AddMeasure(-_refEntity.PlayerSkills[0].BulletData.n_USE_COST);
		}
	}

	public override void PlayerReleaseSkillCharacterCall(int id)
	{
		if (_refEntity.CurrentActiveSkill == -1 && id == 1 && _refEntity.CheckUseSkillKeyTrigger(id))
		{
			_refEntity.CurrentActiveSkill = id;
			_refEntity.PlayerShootBuster(_refEntity.PlayerSkills[id], true, id, _refEntity.PlayerSkills[id].ChargeLevel);
			_refEntity.CheckUsePassiveSkill(id, _refEntity.PlayerSkills[id].weaponStatus, _refEntity.PlayerSkills[id].ShootTransform[0]);
			_refEntity.Animator.SetAnimatorEquip(1);
			ToggleBusterWeapon(true);
			PlaySkillSE("xs_shot");
			PlayVoiceSE("v_xs_skill02");
		}
	}

	public override void CheckSkill()
	{
		if (_pShieldBullet != null)
		{
			if (_pShieldBullet.bIsEnd)
			{
				ShieldBackToPool();
				_refEntity.selfBuffManager.RemoveBuffByCONDITIONID(ShieldBuffId);
			}
		}
		else if (_refEntity.selfBuffManager.CheckHasEffectByCONDITIONID(ShieldBuffId) && _pShieldBullet == null)
		{
			CreateShieldBullet();
		}
		int currentActiveSkill = _refEntity.CurrentActiveSkill;
		if (currentActiveSkill == 1 && _refEntity.CheckSkillEndByShootTimer())
		{
			ToggleBusterWeapon(false);
		}
		if (_refEntity.CurMainStatus != OrangeCharacter.MainStatus.SKILL)
		{
			UpdateUpsideDownStatus();
		}
		else if (_refEntity.CurrentActiveSkill == -1)
		{
			OrangeCharacter.MainStatus curMainStatus = _refEntity.CurMainStatus;
			if (curMainStatus == OrangeCharacter.MainStatus.SKILL)
			{
				OrangeCharacter.SubStatus curSubStatus = _refEntity.CurSubStatus;
				if (curSubStatus == OrangeCharacter.SubStatus.SKILL0_10)
				{
					CheckHangAnimEnd();
				}
			}
		}
		else
		{
			if (_refEntity.IsAnimateIDChanged())
			{
				return;
			}
			nowFrame = GameLogicUpdateManager.GameFrame;
			OrangeCharacter.MainStatus curMainStatus = _refEntity.CurMainStatus;
			if (curMainStatus != OrangeCharacter.MainStatus.SKILL)
			{
				return;
			}
			switch (_refEntity.CurSubStatus)
			{
			default:
			{
				int num = 49;
				break;
			}
			case OrangeCharacter.SubStatus.SKILL0:
				if (nowFrame >= endFrame)
				{
					MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FX_0_00, _refEntity.AimTransform.position, OrangeCharacter.NormalQuaternion, Array.Empty<object>());
					if (linkSkl0 != null && _refEntity.IsLocalPlayer)
					{
						_refEntity.selfBuffManager.AddBuff(ShieldBuffId, 0, 0, 0);
					}
					ManagedSingleton<CharacterControlHelper>.Instance.ChangeToNextStatus(_refEntity, SKL0_TRIGGER2, SKL0_END - SKL0_TRIGGER, OrangeCharacter.SubStatus.SKILL0_1, out skillEventFrame, out endFrame);
				}
				break;
			case OrangeCharacter.SubStatus.SKILL0_1:
				if (nowFrame >= endFrame)
				{
					OnSkillEnd();
				}
				else if (!isSkillEventEnd && nowFrame >= skillEventFrame)
				{
					isSkillEventEnd = true;
					Transform shootPointTransform = _refEntity.PlayerSkills[0].ShootTransform[0];
					ManagedSingleton<CharacterControlHelper>.Instance.PushBulletSkl(_refEntity, shootPointTransform, MagazineType.NORMAL, -1, 0, false);
				}
				else if (isSkillEventEnd && nowFrame >= endBreakFrame)
				{
					ManagedSingleton<CharacterControlHelper>.Instance.CheckBreakFrame(_refEntity.UserID, ref endBreakFrame);
				}
				break;
			}
		}
	}

	public override void ControlCharacterDead()
	{
		if (isInCeiling)
		{
			UpdateCeilingStatus(false);
		}
		ToggleSaberWeapon(false);
	}

	private void OnSkillEnd()
	{
		if (_refEntity.IgnoreGravity)
		{
			_refEntity.IgnoreGravity = false;
		}
		isSkillEventEnd = false;
		_refEntity.SkillEnd = true;
		_refEntity.CurrentActiveSkill = -1;
		_refEntity.EnableCurrentWeapon();
		ToggleSaberWeapon(false);
		HumanBase.AnimateId animateID = _refEntity.AnimateID;
		if (animateID != HumanBase.AnimateId.ANI_SKILL_START)
		{
			if (_refEntity.Controller.Collisions.below || _refEntity.Controller.Collisions.JSB_below)
			{
				_refEntity.Dashing = false;
				_refEntity.SetSpeed(0, 0);
				_refEntity.SetStatus(OrangeCharacter.MainStatus.IDLE, OrangeCharacter.SubStatus.IDLE);
			}
			else
			{
				_refEntity.SetStatus(OrangeCharacter.MainStatus.FALL, OrangeCharacter.SubStatus.TELEPORT_POSE);
			}
		}
		else
		{
			_refEntity.Dashing = false;
			if (ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.DOWN))
			{
				_refEntity.SetStatus(OrangeCharacter.MainStatus.CROUCH, OrangeCharacter.SubStatus.WIN_POSE);
			}
			else
			{
				_refEntity.SetStatus(OrangeCharacter.MainStatus.IDLE, OrangeCharacter.SubStatus.CROUCH_UP);
			}
		}
	}

	private void ToggleBusterWeapon(bool p_enable)
	{
		if (isInCeiling)
		{
			flipBusterMesh.enabled = p_enable;
			if (p_enable)
			{
				flipHandMesh_L_c.enabled = !p_enable;
				flipHandMesh_L_m.enabled = !p_enable;
			}
			else if (_refEntity.GetHandIsNeededEnable())
			{
				flipHandMesh_L_c.enabled = true;
				flipHandMesh_L_m.enabled = true;
			}
			else
			{
				flipHandMesh_L_c.enabled = false;
				flipHandMesh_L_m.enabled = false;
			}
		}
		else
		{
			busterMesh.enabled = p_enable;
			if (p_enable)
			{
				HandMesh_L_c.enabled = !p_enable;
				HandMesh_L_m.enabled = !p_enable;
			}
			else if (_refEntity.GetHandIsNeededEnable())
			{
				HandMesh_L_c.enabled = true;
				HandMesh_L_m.enabled = true;
			}
			else
			{
				HandMesh_L_c.enabled = false;
				HandMesh_L_m.enabled = false;
			}
		}
	}

	private void ToggleSaberWeapon(bool p_enable)
	{
		if (p_enable)
		{
			if ((bool)weaponSwordR)
			{
				weaponSwordR.Appear(null, 0f);
			}
		}
		else if ((bool)weaponSwordR)
		{
			weaponSwordR.Disappear(null, 0f);
		}
	}

	private void ToggleHand()
	{
		bool handMeshEnableStatus = _refEntity.GetHandMeshEnableStatus();
		flipHandMesh_L_c.enabled = handMeshEnableStatus;
		flipHandMesh_L_m.enabled = handMeshEnableStatus;
	}

	private void CreateShieldBullet()
	{
		WeaponStruct weaponStruct = _refEntity.PlayerSkills[0];
		_pShieldBullet = MonoBehaviourSingleton<PoolManager>.Instance.GetPoolObj<ShieldBullet>(linkSkl0.s_MODEL);
		_pShieldBullet.UpdateBulletData(linkSkl0, _refEntity.sPlayerName, _refEntity.GetNowRecordNO(), _refEntity.nBulletRecordID++);
		_pShieldBullet.SetBulletAtk(weaponStruct.weaponStatus, _refEntity.selfBuffManager.sBuffStatus);
		_pShieldBullet.BulletLevel = weaponStruct.SkillLV;
		_pShieldBullet.BindBuffId(ShieldBuffId, _refEntity.IsLocalPlayer);
		_pShieldBullet.Active(_refEntity.ExtraTransforms[2], Quaternion.identity, _refEntity.TargetMask, true);
		PlaySkillSE("xs_giga02_lp");
	}

	public override void ClearSkill()
	{
		isSkillEventEnd = false;
		_refEntity.IgnoreGravity = false;
		_refEntity.SkillEnd = true;
		_refEntity.CurrentActiveSkill = -1;
		_refEntity.EnableCurrentWeapon();
		UpdateCeilingStatus(false);
		ToggleBusterWeapon(false);
		ToggleSaberWeapon(false);
	}

	public override void SetStun(bool enable)
	{
		base.SetStun(enable);
		_refEntity.EnableCurrentWeapon();
		UpdateCeilingStatus(false);
		ToggleSaberWeapon(false);
		flipBusterMesh.enabled = false;
		busterMesh.enabled = false;
	}

	public bool EnterRideArmor(RideBaseObj targetRideArmor)
	{
		if (_refEntity.selfBuffManager.CheckHasEffectByCONDITIONID(ShieldBuffId))
		{
			_refEntity.selfBuffManager.RemoveBuffByCONDITIONID(ShieldBuffId);
		}
		ShieldBackToPool();
		UpdateCeilingStatus(false);
		return _refEntity.EnterRideArmor(targetRideArmor);
	}

	public override int WallSlideGravity()
	{
		return 0;
	}

	private void CancelWallClimbing()
	{
		if (_refEntity.CurMainStatus == OrangeCharacter.MainStatus.WALLGRAB)
		{
			ManagedSingleton<CharacterControlHelper>.Instance.OffesetEntity(_refEntity, ManagedSingleton<CharacterControlHelper>.Instance.ClimbingOffset * _refEntity.direction);
		}
	}

	private void ShieldBackToPool()
	{
		if (_pShieldBullet != null)
		{
			PlaySkillSE("xs_giga02_stop");
			if (!_pShieldBullet.bIsEnd)
			{
				_pShieldBullet.BackToPool();
			}
			MonoBehaviourSingleton<PoolManager>.Instance.BackToPool(_pShieldBullet, _pShieldBullet.itemName);
			_pShieldBullet = null;
		}
	}

	private void InstantiateFlipModel()
	{
		float y = 1.36f;
		_animateStatus = _refEntity.Animator.InitAnimator();
		GameObject gameObject = UnityEngine.Object.Instantiate(_refEntity.ModelTransform.gameObject);
		flipModel = gameObject.transform;
		flipModel.SetParent(_refEntity.ModelTransform);
		flipModel.localPosition = new Vector3(0f, y, 0f);
		flipModel.localRotation = Quaternion.Euler(0f, 0f, 180f);
		flipModel.localScale = new Vector3(-1f, 1f, 1f);
		flipModelAnimator = gameObject.GetComponent<Animator>();
		flipModelCM = gameObject.GetComponent<CharacterMaterial>();
		flipModelCM.Disappear(null, 0f);
		originalCM = _refEntity.CharacterMaterials;
		chipEfxOriginal = new ChipSystem[2];
		chipEfx = new ChipSystem[2];
		Transform[] target = flipModel.GetComponentsInChildren<Transform>(true);
		Transform transform = OrangeBattleUtility.FindChildRecursive(ref target, "NormalWeapon0", true);
		Transform transform2 = OrangeBattleUtility.FindChildRecursive(ref target, "NormalWeapon1", true);
		bool flag = true;
		if ((bool)transform)
		{
			CharacterMaterial component = transform.GetComponent<CharacterMaterial>();
			weaponMesh0 = new CharacterMaterial[1] { component };
			Transform transform3 = transform.Find("ShootPoint");
			shootPoint0 = new Transform[1] { transform3 };
			shootPointOriginal0 = _refEntity.PlayerWeapons[0].ShootTransform;
			weaponMeshOriginal0 = _refEntity.PlayerWeapons[0].WeaponMesh;
			transform.localScale = new Vector3(-1f, 1f, 1f);
			chipEfxOriginal[0] = _refEntity.PlayerWeapons[0].ChipEfx;
			flag = chipEfxOriginal[0] != null;
		}
		if ((bool)transform2)
		{
			CharacterMaterial component2 = transform2.GetComponent<CharacterMaterial>();
			weaponMesh1 = new CharacterMaterial[1] { component2 };
			Transform transform4 = transform2.Find("ShootPoint");
			shootPoint1 = new Transform[1] { transform4 };
			shootPointOriginal1 = _refEntity.PlayerWeapons[1].ShootTransform;
			weaponMeshOriginal1 = _refEntity.PlayerWeapons[1].WeaponMesh;
			transform2.localScale = new Vector3(-1f, 1f, 1f);
			chipEfxOriginal[1] = _refEntity.PlayerWeapons[1].ChipEfx;
		}
		ChipSystem[] componentsInChildren = flipModel.GetComponentsInChildren<ChipSystem>(true);
		if (!flag && componentsInChildren.Length != 0)
		{
			chipEfx[0] = null;
			chipEfx[1] = componentsInChildren[0];
			chipEfx[1].ResetBodyInfo();
			chipEfx[1].SetWeaponMesh(weaponMesh1);
			chipEfx[1].isActive = false;
		}
		else
		{
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				chipEfx[i] = componentsInChildren[i];
				chipEfx[i].ResetBodyInfo();
				if (i == 0)
				{
					chipEfx[i].SetWeaponMesh(weaponMesh0);
				}
				else
				{
					chipEfx[i].SetWeaponMesh(weaponMesh1);
				}
				chipEfx[i].isActive = false;
			}
		}
		Transform transform5 = OrangeBattleUtility.FindChildRecursive(ref target, "HandMesh_L_c", true);
		flipHandMesh_L_c = transform5.GetComponent<SkinnedMeshRenderer>();
		Transform transform6 = OrangeBattleUtility.FindChildRecursive(ref target, "HandMesh_L_m", true);
		flipHandMesh_L_m = transform6.GetComponent<SkinnedMeshRenderer>();
		Transform transform7 = OrangeBattleUtility.FindChildRecursive(ref target, "BusterMesh_m", true);
		flipBusterMesh = transform7.GetComponent<SkinnedMeshRenderer>();
		ShootPointskill1Original = _refEntity.PlayerSkills[1].ShootTransform[0];
		ShootPointskill1 = OrangeBattleUtility.FindChildRecursive(ref target, "L WeaponPoint", true);
		Transform transform8 = OrangeBattleUtility.FindChildRecursive(ref target, "L_Thruster", true);
		Transform transform9 = OrangeBattleUtility.FindChildRecursive(ref target, "R_Thruster", true);
		psLThrusterParticleSystem = transform8.GetComponentInChildren<ParticleSystem>();
		psRThrusterParticleSystem = transform9.GetComponentInChildren<ParticleSystem>();
		psLThrusterParticleSystemOriginal = _refEntity.LThrusterParticleSystem;
		psRThrusterParticleSystemOriginal = _refEntity.RThrusterParticleSystem;
		ParticleSystem[] componentsInChildren2 = psLThrusterParticleSystem.GetComponentsInChildren<ParticleSystem>();
		ParticleSystem[] componentsInChildren3 = psRThrusterParticleSystem.GetComponentsInChildren<ParticleSystem>();
		ParticleSystem[] array = componentsInChildren2;
		for (int j = 0; j < array.Length; j++)
		{
			ParticleSystem.MainModule main = array[j].main;
			main.maxParticles = 0;
		}
		array = componentsInChildren3;
		for (int j = 0; j < array.Length; j++)
		{
			ParticleSystem.MainModule main2 = array[j].main;
			main2.maxParticles = 0;
		}
		flipModelAnimator.Play(_animateStatus[19], 0);
		CreateRabbit();
	}

	public void OverrideAnimatorParamters()
	{
		if (isInCeiling)
		{
			int num = 0;
			switch ((HumanBase.AnimateId)_refEntity.AnimationParams.AnimateUpperID)
			{
			case HumanBase.AnimateId.ANI_STAND:
			case HumanBase.AnimateId.ANI_JUMP:
			case HumanBase.AnimateId.ANI_FALL:
				num = ((_refEntity.IsShoot <= 0) ? _animateStatus[19] : _crouchEndShootHash);
				break;
			case HumanBase.AnimateId.ANI_DASH:
				num = ((_refEntity.IsShoot <= 0) ? _dashStart : _dashAtkLoop);
				break;
			case HumanBase.AnimateId.ANI_DASHSLASH1:
				UpdateCeilingStatus(false);
				return;
			}
			float value = Mathf.Abs(Vector2.SignedAngle(shootDirFrom, _refEntity.ShootDirection)) / 180f;
			flipModelAnimator.SetFloat(_hashDirection, value);
			flipModelAnimator.SetFloat(_hashEquip, _refEntity.Animator._animator.GetFloat(_hashEquip));
			if (cacheAniId != num)
			{
				flipModelAnimator.Play(num, 0);
				cacheAniId = num;
			}
		}
	}

	private void UpdateCeilingStatus(bool inCeiling)
	{
		if (isInCeiling == inCeiling)
		{
			return;
		}
		CharacterMaterial[] array;
		ChipSystem[] array2;
		if (inCeiling)
		{
			_refEntity.SetStatus(OrangeCharacter.MainStatus.FALL, OrangeCharacter.SubStatus.TELEPORT_POSE);
			_refEntity.IgnoreGravity = true;
			_refEntity.ResetDashChance();
			isInCeiling = true;
			originalCM.Disappear(null, 0f);
			flipModelCM.Appear(null, 0f);
			_refEntity.CharacterMaterials = flipModelCM;
			cacheAniId = -1;
			int num = _refEntity.GetCurrentWeapon();
			bool value = false;
			if (chipEfxOriginal[num] != null)
			{
				value = chipEfxOriginal[num].isActive;
			}
			if (num == 0)
			{
				array = weaponMesh0;
				for (int i = 0; i < array.Length; i++)
				{
					array[i].Appear(null, 0f);
				}
				if (chipEfx[0] != null)
				{
					chipEfx[0].ActiveChipSkill(value);
				}
			}
			else
			{
				array = weaponMesh1;
				for (int i = 0; i < array.Length; i++)
				{
					array[i].Appear(null, 0f);
				}
				if (chipEfx[1] != null)
				{
					chipEfx[1].ActiveChipSkill(value);
				}
			}
			array2 = chipEfxOriginal;
			foreach (ChipSystem chipSystem in array2)
			{
				if (chipSystem != null)
				{
					chipSystem.ActiveChipSkill(false);
				}
			}
			_refEntity.DisableWeaponMesh(_refEntity.GetCurrentWeaponObj(), 0f);
			_refEntity.PlayerWeapons[0].WeaponMesh = weaponMesh0;
			_refEntity.PlayerWeapons[1].WeaponMesh = weaponMesh1;
			_refEntity.PlayerWeapons[0].ShootTransform = shootPoint0;
			_refEntity.PlayerWeapons[1].ShootTransform = shootPoint1;
			_refEntity.PlayerSkills[1].ShootTransform[0] = ShootPointskill1;
			_refEntity.PlayerWeapons[0].ChipEfx = chipEfx[0];
			_refEntity.PlayerWeapons[1].ChipEfx = chipEfx[1];
			_refEntity.RThrusterParticleSystem = psRThrusterParticleSystem;
			_refEntity.LThrusterParticleSystem = psLThrusterParticleSystem;
			cacheAniId = _animateStatus[68];
			Vector3 position = _refEntity.Controller.LogicPosition.vec3 - hangAniOfsset;
			_refEntity.Controller.LogicPosition = new VInt3(position);
			_refEntity.Controller.transform.position = position;
			_refEntity.EnableCurrentWeapon();
			ToggleHand();
			moveWithRabbit = IsPlantRabbit();
			return;
		}
		RecycleRabbit();
		_refEntity.IgnoreGravity = false;
		busterMesh.enabled = false;
		flipBusterMesh.enabled = false;
		isInCeiling = false;
		_refEntity.ClearDashChance();
		originalCM.Appear(null, 0f);
		flipModelCM.Disappear(null, 0f);
		_refEntity.CharacterMaterials = originalCM;
		array = weaponMesh0;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Disappear(null, 0f);
		}
		array = weaponMesh1;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Disappear(null, 0f);
		}
		int num2 = _refEntity.GetCurrentWeapon();
		bool value2 = false;
		if (chipEfx[num2] != null)
		{
			value2 = chipEfx[num2].isActive;
		}
		array2 = chipEfx;
		foreach (ChipSystem chipSystem2 in array2)
		{
			if (chipSystem2 != null)
			{
				chipSystem2.ActiveChipSkill(false);
			}
		}
		if (num2 == 0)
		{
			if (chipEfxOriginal[0] != null)
			{
				chipEfxOriginal[0].ActiveChipSkill(value2);
			}
		}
		else if (chipEfxOriginal[1] != null)
		{
			chipEfxOriginal[1].ActiveChipSkill(value2);
		}
		cacheAniId = -1;
		_refEntity.PlayerWeapons[0].WeaponMesh = weaponMeshOriginal0;
		_refEntity.PlayerWeapons[1].WeaponMesh = weaponMeshOriginal1;
		_refEntity.PlayerWeapons[0].ShootTransform = shootPointOriginal0;
		_refEntity.PlayerWeapons[1].ShootTransform = shootPointOriginal1;
		_refEntity.PlayerSkills[1].ShootTransform[0] = ShootPointskill1Original;
		_refEntity.PlayerWeapons[0].ChipEfx = chipEfxOriginal[0];
		_refEntity.PlayerWeapons[1].ChipEfx = chipEfxOriginal[1];
		_refEntity.RThrusterParticleSystem = psRThrusterParticleSystemOriginal;
		_refEntity.LThrusterParticleSystem = psLThrusterParticleSystemOriginal;
		_refEntity.PlayerWeapons[(int)_refEntity.GetCurrentWeapon()].WeaponMesh[0].Appear(null, 0f);
		flipModelAnimator.Play(_animateStatus[19], 0);
		_refEntity.EnableCurrentWeapon();
		Vector3 vector = hangAniOfsset;
		if (invisibleRabbit.position.y < rabbitCachePosY)
		{
			vector = platformOffset;
		}
		Vector3 position2 = _refEntity.Controller.LogicPosition.vec3 + vector;
		_refEntity.Controller.LogicPosition = new VInt3(position2);
		_refEntity.Controller.transform.position = position2;
		if (isConsoleCharacter)
		{
			_refEntity.selfBuffManager.RemoveBuffByCONDITIONID(upsideDownBuffId);
		}
	}

	private void PrepareChangeToCeiling()
	{
		if (isConsoleCharacter)
		{
			_refEntity.selfBuffManager.AddBuff(upsideDownBuffId, 0, 0, 0);
		}
		_refEntity.SkillEnd = false;
		_refEntity.SetHorizontalSpeed(0);
		_refEntity.PlayerStopDashing();
		hangAniEndFrame = GameLogicUpdateManager.GameFrame + HANG_ANI_END;
		_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0_10);
		_refEntity.SetAnimateId((HumanBase.AnimateId)68u);
		_refEntity.IgnoreGravity = true;
		_refEntity.DisableWeaponMesh(_refEntity.GetCurrentWeaponObj(), 0f);
		Vector3 position = _refEntity.Controller.LogicPosition.vec3 + hangAniOfsset;
		_refEntity.Controller.LogicPosition = new VInt3(position);
		_refEntity.Controller.transform.position = position;
		busterMesh.enabled = false;
		flipBusterMesh.enabled = false;
		PlayCharaSE("xs_kabepeta");
	}

	private void CheckHangAnimEnd()
	{
		if (GameLogicUpdateManager.GameFrame >= hangAniEndFrame)
		{
			_refEntity.SkillEnd = true;
			UpdateCeilingStatus(true);
		}
	}

	private void UpdateUpsideDownStatus()
	{
		if (!isConsoleCharacter)
		{
			bool flag = _refEntity.selfBuffManager.CheckHasEffectByCONDITIONID(upsideDownBuffId);
			if (isInCeiling && !flag)
			{
				UpdateCeilingStatus(false);
			}
			else if (!isInCeiling && flag)
			{
				PrepareChangeToCeiling();
			}
		}
		else if (isInCeiling)
		{
			string userID = _refEntity.UserID;
			if (ManagedSingleton<InputStorage>.Instance.IsHeld(userID, ButtonId.LEFT) || ManagedSingleton<InputStorage>.Instance.IsHeld(userID, ButtonId.RIGHT) || ManagedSingleton<InputStorage>.Instance.IsHeld(userID, ButtonId.SELECT))
			{
				UpdateCeilingStatus(false);
			}
			else
			{
				if (_refEntity.CurMainStatus == OrangeCharacter.MainStatus.AIRDASH)
				{
					return;
				}
				if (rabbitMoved)
				{
					if (colliderRabbitParent == null || !colliderRabbitParent.isActiveAndEnabled)
					{
						RecycleRabbit();
						UpdateCeilingStatus(false);
						return;
					}
					Controller2D controller = _refEntity.Controller;
					Controller2D.RaycastOrigins raycastOrigins = controller.GetRaycastOrigins();
					rayOrginBottom = new Vector2((raycastOrigins.bottomLeft.x + raycastOrigins.bottomLeft.x) / 2f, raycastOrigins.bottomLeft.y);
					if ((bool)Physics2D.Raycast(rayOrginBottom, rayDirBottom, rayCeilingDist, (int)controller.collisionMask | (int)controller.collisionMaskThrough))
					{
						RecycleRabbit();
						Vector3 position = _refEntity.Controller.LogicPosition.vec3 + new Vector3(0f, 5f, 0f);
						_refEntity.Controller.LogicPosition = new VInt3(position);
						_refEntity.Controller.transform.position = position;
						UpdateCeilingStatus(false);
					}
					else
					{
						rabbitCachePosY = invisibleRabbit.transform.position.y;
					}
				}
				else
				{
					Controller2D controller2 = _refEntity.Controller;
					if (!Physics2D.Raycast(rayOrginTop, rayDirTop, rayRabbitDist, (int)controller2.collisionMask | (int)controller2.collisionMaskThrough))
					{
						UpdateCeilingStatus(false);
					}
				}
			}
		}
		else
		{
			if (!_refEntity.Controller.Collisions.above)
			{
				return;
			}
			string userID2 = _refEntity.UserID;
			if (!ManagedSingleton<InputStorage>.Instance.IsHeld(userID2, ButtonId.LEFT) && !ManagedSingleton<InputStorage>.Instance.IsHeld(userID2, ButtonId.RIGHT) && !ManagedSingleton<InputStorage>.Instance.IsHeld(userID2, ButtonId.DASH))
			{
				OrangeCharacter.MainStatus curMainStatus = _refEntity.CurMainStatus;
				if ((uint)(curMainStatus - 6) <= 2u)
				{
					PrepareChangeToCeiling();
				}
			}
		}
	}

	public void SetStatusCharacterDepend(OrangeCharacter.MainStatus mainStatus, OrangeCharacter.SubStatus subStatus)
	{
		switch (_refEntity.CurMainStatus)
		{
		case OrangeCharacter.MainStatus.FALL:
		{
			OrangeCharacter.SubStatus curSubStatus = _refEntity.CurSubStatus;
			if ((uint)(curSubStatus - 1) <= 1u && isInCeiling)
			{
				UpdateCeilingStatus(false);
			}
			break;
		}
		case OrangeCharacter.MainStatus.GIGA_ATTACK:
		{
			OrangeCharacter.SubStatus curSubStatus = _refEntity.CurSubStatus;
			if (curSubStatus == OrangeCharacter.SubStatus.GIGA_ATTACK_START && isInCeiling)
			{
				UpdateCeilingStatus(false);
				_refEntity.IgnoreGravity = true;
			}
			break;
		}
		case OrangeCharacter.MainStatus.IDLE:
		{
			OrangeCharacter.SubStatus curSubStatus = _refEntity.CurSubStatus;
			if (curSubStatus == OrangeCharacter.SubStatus.DASH_END && isInCeiling)
			{
				UpdateCeilingStatus(false);
			}
			break;
		}
		case OrangeCharacter.MainStatus.WALLKICK:
			if (isInCeiling)
			{
				UpdateCeilingStatus(false);
			}
			break;
		}
	}

	private void PlayerPressJump()
	{
		if (isInCeiling)
		{
			UpdateCeilingStatus(false);
		}
		else
		{
			_refEntity.PlayerPressJump();
		}
	}

	public override void ExtraVariableInit()
	{
		if (isInCeiling)
		{
			UpdateCeilingStatus(false);
		}
	}

	public void AddForceField(VInt3 pForce)
	{
		if (isInCeiling)
		{
			UpdateCeilingStatus(false);
		}
		_refEntity.AddForceField(pForce);
	}

	public void MapCollisonCB(MapCollisionEvent.MapCollisionEnum mapCollisionEnum)
	{
		if ((mapCollisionEnum == MapCollisionEvent.MapCollisionEnum.COLLISION_BUMPOBJ || mapCollisionEnum == MapCollisionEvent.MapCollisionEnum.COLLISION_BUMPOBJ2) && isInCeiling)
		{
			UpdateCeilingStatus(false);
		}
	}

	public void CreateRabbit()
	{
		invisibleRabbit = new GameObject("ch131_rabbit").transform;
	}

	public bool IsPlantRabbit()
	{
		Controller2D controller = _refEntity.Controller;
		Controller2D.RaycastOrigins raycastOrigins = controller.GetRaycastOrigins();
		Vector2 origin = new Vector2((raycastOrigins.topLeft.x + raycastOrigins.topRight.x) / 2f, raycastOrigins.topLeft.y);
		RaycastHit2D raycastHit2D = Physics2D.Raycast(origin, rayDirTop, rayRabbitDist, (int)controller.collisionMask | (int)controller.collisionMaskThrough);
		rayOrginTop = origin;
		if ((bool)raycastHit2D)
		{
			PlantRabbit(raycastHit2D.transform, raycastHit2D.point);
			return true;
		}
		RaycastHit2D raycastHit2D2 = Physics2D.Raycast(raycastOrigins.topLeft, rayDirTop, rayRabbitDist, (int)controller.collisionMask | (int)controller.collisionMaskThrough);
		if ((bool)raycastHit2D2)
		{
			PlantRabbit(raycastHit2D2.transform, raycastHit2D2.point);
			rayOrginTop = raycastOrigins.topLeft;
			return true;
		}
		RaycastHit2D raycastHit2D3 = Physics2D.Raycast(raycastOrigins.topRight, rayDirTop, rayRabbitDist, (int)controller.collisionMask | (int)controller.collisionMaskThrough);
		if ((bool)raycastHit2D3)
		{
			PlantRabbit(raycastHit2D3.transform, raycastHit2D3.point);
			rayOrginTop = raycastOrigins.topRight;
			return true;
		}
		return false;
	}

	public void PlantRabbit(Transform p_parent, Vector3 p_worldPos)
	{
		if (!invisibleRabbit)
		{
			CreateRabbit();
		}
		invisibleRabbit.transform.position = p_worldPos - new Vector3(0f, 1.4f);
		invisibleRabbit.SetParent(p_parent, true);
		invisibleRabbit.hasChanged = false;
		colliderRabbitParent = p_parent.GetComponent<Collider2D>();
	}

	public void RecycleRabbit()
	{
		if (!invisibleRabbit)
		{
			CreateRabbit();
		}
		moveWithRabbit = false;
		rabbitMoved = false;
		invisibleRabbit.SetParentNull();
		invisibleRabbit.hasChanged = false;
		colliderRabbitParent = null;
	}

	private void LateUpdate()
	{
		if (moveWithRabbit && invisibleRabbit.hasChanged)
		{
			Vector3 position = invisibleRabbit.position;
			_refEntity.Controller.LogicPosition = new VInt3(position);
			_refEntity.Controller.transform.position = position;
			rabbitMoved = true;
			invisibleRabbit.hasChanged = false;
		}
	}

	public override string[] GetCharacterDependAnimations()
	{
		return new string[4] { "ch131_skill_01_crouch", "ch131_skill_01_stand", "ch131_skill_01_jump", "ch131_hang" };
	}

	public override int GetUniqueWeaponType()
	{
		return 1;
	}

	public override void GetUniqueWeaponMotion(out string[] source, out string[] target)
	{
		source = new string[1] { "_wallgrab_loop" };
		target = new string[1] { "_wallgrab_static_loop" };
	}
}
