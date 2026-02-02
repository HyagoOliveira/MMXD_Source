using System;
using UnityEngine;

public class CH140_Controller : CharacterControlBase, ILogicUpdate
{
	private enum GunFacing
	{
		Right = 0,
		Left = 1
	}

	private int nowFrame;

	private int skillEventFrame;

	private int endFrame;

	private int endBreakFrame;

	private bool isSkillEventEnd;

	private int comboId = 1512;

	private OrangeTimer NOVASTRIKETimer;

	private bool isFrenzyStatus;

	private int conditionId = 1513;

	private ParticleSystem[] _FrenzyEffect;

	private ParticleSystem _teleportEffect;

	private bool isInit;

	private Vector3 shootDirection = Vector3.right;

	private CharacterMaterial cmGun;

	private CharacterMaterial cmSaber;

	private CharacterMaterial cmSaberBack;

	[SerializeField]
	private int risingSpdY = 12000;

	private OrangeConsoleCharacter _refPlayer;

	private int teleportFxFrame = -1;

	private bool isWinPose;

	private readonly string FX_0_00 = "fxuse_DMCXStinger_000";

	private readonly string FX_0_01 = "fxuse_DMCXOverDrive_000";

	private readonly string FX_1_00 = "fxuse_DMCXRainStorm_000";

	private readonly string FX_1_01 = "fxuse_DMCXRainStorm_001";

	private readonly string FX_2_00 = "fxuse_DMCXDevilTrigger_000";

	protected readonly int SKL0_1_TRIGGER = (int)(0.09f / GameLogicUpdateManager.m_fFrameLen);

	protected readonly int SKL0_2_TRIGGER = (int)(0.29f / GameLogicUpdateManager.m_fFrameLen);

	protected readonly int SKL0_3_TRIGGER = (int)(0.49f / GameLogicUpdateManager.m_fFrameLen);

	protected readonly int SKL0_END = (int)(1f / GameLogicUpdateManager.m_fFrameLen);

	protected readonly int SKL0_END_BREAK = (int)(0.825f / GameLogicUpdateManager.m_fFrameLen);

	protected readonly int SKL1_TRIGGER = (int)(0.25f / GameLogicUpdateManager.m_fFrameLen);

	protected readonly int SKL1_LOOP = (int)(1.75f / GameLogicUpdateManager.m_fFrameLen);

	protected readonly int SKL1_END = (int)(0.667f / GameLogicUpdateManager.m_fFrameLen);

	protected readonly int SKL1_END_BREAK = (int)(0.6f / GameLogicUpdateManager.m_fFrameLen);

	protected readonly int LOGOUT_TRIGGER = (int)(0.73f / GameLogicUpdateManager.m_fFrameLen);

	private void OnEnable()
	{
		MonoBehaviourSingleton<GameLogicUpdateManager>.Instance.AddUpdate(this);
	}

	private void OnDisable()
	{
		MonoBehaviourSingleton<GameLogicUpdateManager>.Instance.RemoveUpdate(this);
	}

	public override void Start()
	{
		base.Start();
		InitializeSkill();
		_refPlayer = _refEntity as OrangeConsoleCharacter;
	}

	private void InitializeSkill()
	{
		Transform transform = new GameObject("CustomShootPoint0").transform;
		transform.SetParent(base.transform);
		transform.localPosition = new Vector3(0f, 0.8f, 0f);
		Transform[] target = _refEntity._transform.GetComponentsInChildren<Transform>(true);
		_refEntity.ExtraTransforms = new Transform[2];
		_refEntity.ExtraTransforms[0] = transform;
		_refEntity.ExtraTransforms[1] = OrangeBattleUtility.FindChildRecursive(ref target, "R WeaponPoint", true);
		_refEntity.PlayerSkills[0].ShootTransform[0] = _refEntity.ExtraTransforms[0];
		_refEntity.PlayerSkills[1].ShootTransform[0] = _refEntity.ExtraTransforms[0];
		_FrenzyEffect = new ParticleSystem[1];
		_FrenzyEffect[0] = OrangeBattleUtility.FindChildRecursive(ref target, "fxduring_DMCXDevilTrigger_000_(work)").GetComponent<ParticleSystem>();
		_FrenzyEffect[0].gameObject.SetActive(false);
		_teleportEffect = OrangeBattleUtility.FindChildRecursive(ref target, "fxdemo_DMCX_003").GetComponent<ParticleSystem>();
		GameObject gameObject = OrangeBattleUtility.FindChildRecursive(ref target, "SwordMesh_m", true).gameObject;
		if ((bool)gameObject)
		{
			cmSaber = gameObject.GetComponent<CharacterMaterial>();
		}
		GameObject gameObject2 = OrangeBattleUtility.FindChildRecursive(ref target, "BackSwordMesh_m", true).gameObject;
		if ((bool)gameObject2)
		{
			cmSaberBack = gameObject2.GetComponent<CharacterMaterial>();
		}
		GameObject gameObject3 = OrangeBattleUtility.FindChildRecursive(ref target, "GunMesh_L_m", true).gameObject;
		if ((bool)gameObject3)
		{
			cmGun = gameObject3.GetComponent<CharacterMaterial>();
		}
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(FX_0_00, 2);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(FX_0_01, 2);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(FX_1_00, 2);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(FX_1_01, 2);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(FX_2_00, 2);
		NOVASTRIKETimer = OrangeTimerManager.GetTimer();
		isInit = true;
	}

	public override void OverrideDelegateEvent()
	{
		base.OverrideDelegateEvent();
		_refEntity.SetStatusCharacterDependEvt = SetStatusCharacterDepend;
		_refEntity.PlayTeleportOutEffectEvt = PlayTeleportOutEffect;
		_refEntity.TeleportOutCharacterDependEvt = TeleportOutCharacterDepend;
		if (_refEntity is OrangeConsoleCharacter)
		{
			_refEntity.ChangeComboSkillEventEvt = ChangeComboSkillEvent;
		}
	}

	private void PlayTeleportOutEffect()
	{
		Vector3 p_worldPos = base.transform.position;
		if (_refEntity != null)
		{
			p_worldPos = _refEntity.AimPosition;
		}
		MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("FX_TELEPORT_OUT", p_worldPos, Quaternion.identity, Array.Empty<object>());
	}

	public void LogicUpdate()
	{
		if (isInit)
		{
			CheckFrenzyBuff();
		}
	}

	private void CheckFrenzyBuff()
	{
		if (_refEntity.PlayerSkills.Length == 0)
		{
			return;
		}
		if (_refEntity.PlayerSkills[0].Reload_index == 2)
		{
			if (!isFrenzyStatus)
			{
				PlayFrenzyFx();
			}
		}
		else if (isFrenzyStatus)
		{
			StopFrenzyFx();
		}
	}

	private void PlayFrenzyFx()
	{
		isFrenzyStatus = true;
		_FrenzyEffect[0].gameObject.SetActive(true);
		_FrenzyEffect[0].Play(true);
		MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FX_2_00, _refEntity.ModelTransform.position, OrangeCharacter.NormalQuaternion, Array.Empty<object>());
		PlaySkillSE("xc_majin01");
	}

	private void StopFrenzyFx()
	{
		isFrenzyStatus = false;
		_FrenzyEffect[0].gameObject.SetActive(false);
		_FrenzyEffect[0].Stop(true);
		PlaySkillSE("xc_majin02");
	}

	public override void PlayerPressSkillCharacterCall(int id)
	{
		if (_refEntity.CurrentActiveSkill != -1)
		{
			return;
		}
		switch (id)
		{
		case 0:
			if (IsUseStinger() && _refEntity.CheckUseSkillKeyTrigger(id))
			{
				ToggleSaber(true);
				skillEventFrame = GameLogicUpdateManager.GameFrame + ManagedSingleton<CharacterControlHelper>.Instance.NOVASTRIKE_PREPARE_FRAME;
				ManagedSingleton<CharacterControlHelper>.Instance.NOVASTRIKE_Prepare(_refEntity, 0);
				PlayVoiceSE("v_xc_skill01");
				PlaySkillSE("xc_stinger01");
			}
			break;
		case 1:
			if (_refEntity.CheckUseSkillKeyTrigger(id))
			{
				isSkillEventEnd = false;
				endBreakFrame = GameLogicUpdateManager.GameFrame + SKL1_END_BREAK;
				ManagedSingleton<CharacterControlHelper>.Instance.ChangeToSklStatus(_refEntity, id, SKL1_TRIGGER, SKL1_LOOP, OrangeCharacter.SubStatus.SKILL1, out skillEventFrame, out endFrame);
				_refEntity.SetAnimateId((HumanBase.AnimateId)70u);
				_refEntity.DisableWeaponMesh(_refEntity.GetCurrentWeaponObj(), 0.01f);
				ToggleGun(true);
				_refEntity.IgnoreGravity = true;
				_refEntity.SetSpeed(0, risingSpdY);
				WeaponStruct weaponStruct = _refEntity.PlayerSkills[1];
				_refEntity.CheckUsePassiveSkill(1, weaponStruct.weaponStatus, weaponStruct.ShootTransform[0]);
				OrangeBattleUtility.UpdateSkillCD(weaponStruct);
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FX_1_00, _refEntity.ModelTransform.position, OrangeCharacter.NormalQuaternion, Array.Empty<object>());
				PlayVoiceSE("v_xc_skill02");
				PlaySkillSE("xc_rain01");
			}
			break;
		}
	}

	public override void PlayerReleaseSkillCharacterCall(int id)
	{
		if (_refEntity.CurrentActiveSkill == -1 && id == 0 && !IsUseStinger() && _refEntity.CheckUseSkillKeyTrigger(id))
		{
			isSkillEventEnd = false;
			endBreakFrame = GameLogicUpdateManager.GameFrame + SKL0_END_BREAK;
			_refEntity.IsShoot = 1;
			ManagedSingleton<CharacterControlHelper>.Instance.ChangeToSklStatus(_refEntity, id, SKL0_1_TRIGGER, SKL0_1_TRIGGER, OrangeCharacter.SubStatus.SKILL0, out skillEventFrame, out endFrame);
			ManagedSingleton<CharacterControlHelper>.Instance.SetAnimate(_refEntity, (HumanBase.AnimateId)68u, (HumanBase.AnimateId)68u, (HumanBase.AnimateId)69u);
			_refEntity.DisableWeaponMesh(_refEntity.GetCurrentWeaponObj(), 0.01f);
			ToggleSaber(true);
			WeaponStruct weaponStruct = _refEntity.PlayerSkills[0];
			_refEntity.CheckUsePassiveSkill(0, weaponStruct.weaponStatus, weaponStruct.ShootTransform[0]);
			OrangeBattleUtility.UpdateSkillCD(weaponStruct);
			_refEntity.RemoveComboSkillBuff(weaponStruct.FastBulletDatas[1].n_ID);
			MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FX_0_01, _refEntity.ModelTransform.position, OrangeCharacter.NormalQuaternion, Array.Empty<object>());
			PlayVoiceSE("v_xc_skill01");
			PlaySkillSE("xc_stinger04");
		}
	}

	public override void CheckSkill()
	{
		if (_refEntity.IsAnimateIDChanged() || _refEntity.CurMainStatus != OrangeCharacter.MainStatus.SKILL || _refEntity.CurrentActiveSkill == -1)
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
		case OrangeCharacter.SubStatus.RIDE_ARMOR:
			if (_refEntity.CurrentActiveSkill != 0)
			{
				_refEntity.CurrentActiveSkill = 0;
			}
			if (nowFrame >= skillEventFrame)
			{
				ManagedSingleton<CharacterControlHelper>.Instance.NOVASTRIKE_Prepare_To_Loop(_refEntity, NOVASTRIKETimer, 0, true, false);
				PlaySkillSE("xc_stinger02");
				_refEntity.PlaySE(_refEntity.SkillSEID, "xc_stinger03", 0.07f);
			}
			break;
		case OrangeCharacter.SubStatus.IDLE:
			if (ManagedSingleton<CharacterControlHelper>.Instance.NOVASTRIKE_Loop(_refEntity, NOVASTRIKETimer, 0))
			{
				ToggleSaber(false);
			}
			break;
		case OrangeCharacter.SubStatus.SKILL0:
			if (nowFrame >= endFrame)
			{
				WeaponStruct weaponStruct = _refEntity.PlayerSkills[0];
				ManagedSingleton<CharacterControlHelper>.Instance.UpdateShootDirByAimDir(_refEntity);
				ManagedSingleton<CharacterControlHelper>.Instance.PushBulletSkl(_refEntity, weaponStruct.ShootTransform[0], MagazineType.NORMAL, GetSkill0ReloadIdx(), 1, false);
				shootDirection = _refEntity.ShootDirection;
				int num = SKL0_2_TRIGGER - SKL0_1_TRIGGER;
				ManagedSingleton<CharacterControlHelper>.Instance.ChangeToNextStatus(_refEntity, num, num, OrangeCharacter.SubStatus.SKILL0_1, out skillEventFrame, out endFrame);
			}
			break;
		case OrangeCharacter.SubStatus.SKILL0_1:
			if (nowFrame >= endFrame)
			{
				WeaponStruct weaponStruct2 = _refEntity.PlayerSkills[0];
				ManagedSingleton<CharacterControlHelper>.Instance.UpdateShootDirByAimDir(_refEntity);
				ManagedSingleton<CharacterControlHelper>.Instance.PushBulletSkl(_refEntity, weaponStruct2.ShootTransform[0], shootDirection, MagazineType.NORMAL, GetSkill0ReloadIdx(), 0, false);
				shootDirection = _refEntity.ShootDirection;
				int p_sklTriggerFrame = SKL0_3_TRIGGER - SKL0_2_TRIGGER;
				int p_endFrame = SKL0_END - SKL0_2_TRIGGER;
				isSkillEventEnd = false;
				ManagedSingleton<CharacterControlHelper>.Instance.ChangeToNextStatus(_refEntity, p_sklTriggerFrame, p_endFrame, OrangeCharacter.SubStatus.SKILL0_2, out skillEventFrame, out endFrame);
			}
			break;
		case OrangeCharacter.SubStatus.SKILL0_2:
			if (nowFrame >= endFrame)
			{
				OnSkillEnd();
			}
			else if (!isSkillEventEnd && nowFrame >= skillEventFrame)
			{
				isSkillEventEnd = true;
				WeaponStruct weaponStruct3 = _refEntity.PlayerSkills[0];
				ManagedSingleton<CharacterControlHelper>.Instance.UpdateShootDirByAimDir(_refEntity);
				ManagedSingleton<CharacterControlHelper>.Instance.PushBulletSkl(_refEntity, weaponStruct3.ShootTransform[0], shootDirection, MagazineType.NORMAL, GetSkill0ReloadIdx(), 0, false);
				shootDirection = _refEntity.ShootDirection;
			}
			else if (isSkillEventEnd && nowFrame >= endBreakFrame)
			{
				ManagedSingleton<CharacterControlHelper>.Instance.CheckBreakFrame(_refEntity.UserID, ref endFrame);
			}
			break;
		case OrangeCharacter.SubStatus.SKILL1:
			if (nowFrame >= endFrame)
			{
				_refEntity.SetAnimateId((HumanBase.AnimateId)72u);
				ManagedSingleton<CharacterControlHelper>.Instance.ChangeToNextStatus(_refEntity, SKL1_END, SKL1_END, OrangeCharacter.SubStatus.SKILL1_1, out skillEventFrame, out endFrame);
			}
			else if (!isSkillEventEnd && nowFrame >= skillEventFrame)
			{
				isSkillEventEnd = true;
				WeaponStruct weaponStruct4 = _refEntity.PlayerSkills[1];
				ManagedSingleton<CharacterControlHelper>.Instance.PushBulletSkl(_refEntity, weaponStruct4.ShootTransform[0], MagazineType.NORMAL, _refEntity.GetCurrentSkillObj().Reload_index, 0, false);
				_refEntity.SetAnimateId((HumanBase.AnimateId)71u);
				_refEntity.Dashing = false;
				_refEntity.SetSpeed(0, 0);
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FX_1_01, _refEntity.ModelTransform.position, OrangeCharacter.NormalQuaternion, Array.Empty<object>());
				PlaySkillSE("xc_rain02");
			}
			break;
		case OrangeCharacter.SubStatus.SKILL1_1:
			if (nowFrame >= endFrame)
			{
				OnSkillEnd();
			}
			else if (isSkillEventEnd && nowFrame >= endBreakFrame)
			{
				ManagedSingleton<CharacterControlHelper>.Instance.CheckBreakFrame(_refEntity.UserID, ref endFrame);
			}
			break;
		}
	}

	public void SetStatusCharacterDepend(OrangeCharacter.MainStatus mainStatus, OrangeCharacter.SubStatus subStatus)
	{
		switch (mainStatus)
		{
		case OrangeCharacter.MainStatus.SKILL:
			if (subStatus == OrangeCharacter.SubStatus.IDLE)
			{
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FX_0_00, _refEntity.ModelTransform, Quaternion.identity, Array.Empty<object>());
			}
			break;
		case OrangeCharacter.MainStatus.TELEPORT_OUT:
		{
			OrangeCharacter.SubStatus curSubStatus = _refEntity.CurSubStatus;
			if (curSubStatus == OrangeCharacter.SubStatus.WIN_POSE)
			{
				isWinPose = true;
			}
			else
			{
				teleportFxFrame = GameLogicUpdateManager.GameFrame + LOGOUT_TRIGGER;
			}
			break;
		}
		}
	}

	private void TeleportOutCharacterDepend()
	{
		if (isWinPose && _refEntity.CurSubStatus == OrangeCharacter.SubStatus.TELEPORT_POSE && GameLogicUpdateManager.GameFrame == teleportFxFrame && (bool)_teleportEffect)
		{
			_teleportEffect.Play(true);
		}
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
		ToggleSaber(false);
		ToggleGun(false);
		HumanBase.AnimateId animateID = _refEntity.AnimateID;
		if (_refEntity.IsInGround)
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

	private bool IsUseStinger()
	{
		return !_refEntity.BuffManager.CheckHasEffectByCONDITIONID(comboId);
	}

	private int GetSkill0ReloadIdx()
	{
		if (_refEntity.BuffManager.CheckHasEffectByCONDITIONID(conditionId))
		{
			return 2;
		}
		return 1;
	}

	private void ToggleSaber(bool enable, bool setBack = true)
	{
		if (!cmSaber)
		{
			return;
		}
		if (enable)
		{
			cmSaber.Appear();
			if (setBack)
			{
				cmSaberBack.Disappear(null, 0.01f);
			}
		}
		else
		{
			cmSaber.Disappear();
			if (setBack)
			{
				cmSaberBack.Appear(null, 0.01f);
			}
		}
	}

	private void ToggleGun(bool enable)
	{
		if (!cmGun)
		{
			return;
		}
		if (enable)
		{
			if (_refEntity._characterDirection == CharacterDirection.RIGHT)
			{
				cmGun.UpdateTex(0);
			}
			else
			{
				cmGun.UpdateTex(1);
			}
			cmGun.Appear();
		}
		else
		{
			cmGun.Disappear();
		}
	}

	public override void ClearSkill()
	{
		isSkillEventEnd = false;
		_refEntity.IgnoreGravity = false;
		_refEntity.SkillEnd = true;
		_refEntity.CurrentActiveSkill = -1;
		ToggleSaber(false);
		ToggleGun(false);
		_refEntity.EnableCurrentWeapon();
		if (_refEntity.BulletCollider.IsActivate)
		{
			_refEntity.BulletCollider.BackToPool();
		}
	}

	public override void SetStun(bool enable)
	{
		base.SetStun(enable);
		ToggleSaber(false);
		ToggleGun(false);
		_refEntity.EnableCurrentWeapon();
	}

	public override void ControlCharacterDead()
	{
		ToggleGun(false);
		ToggleSaber(false, false);
	}

	public void ChangeComboSkillEvent(object[] parameters)
	{
		if (parameters.Length != 2 || _refPlayer == null)
		{
			return;
		}
		int num = (int)parameters[0];
		int num2 = (int)parameters[1];
		bool flag = IsUseStinger();
		if (flag)
		{
			_refPlayer.ForceChangeSkillIcon(1, _refEntity.PlayerSkills[0].Icon);
		}
		if (num == 0)
		{
			if (flag)
			{
				_refPlayer.SetVirtualButtonAnalog(VirtualButtonId.SKILL0, false);
				_refPlayer.ClearVirtualButtonStick(VirtualButtonId.SKILL0);
			}
			else
			{
				_refPlayer.SetVirtualButtonAnalog(VirtualButtonId.SKILL0, true);
			}
		}
	}

	public override string[] GetCharacterDependAnimations()
	{
		return new string[8] { "ch140_skill_01_phase1_start", "ch140_skill_01_phase1_loop", "ch140_skill_01_phase1_jump_end", "ch140_skill_01_phase2_stand", "ch140_skill_01_phase2_jump", "ch140_skill_02_jump_start", "ch140_skill_02_jump_loop", "ch140_skill_02_jump_end" };
	}

	public override string[] GetCharacterDependBlendAnimations()
	{
		return new string[2] { "ch140_skill_01_phase1_start", "ch140_skill_01_phase1_loop" };
	}
}
