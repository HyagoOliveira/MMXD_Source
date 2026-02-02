using System.Collections;
using StageLib;
using UnityEngine;

public class CH125_Controller : CharacterControlBase
{
	private int nowFrame;

	private int skillEventFrame;

	private int endFrame;

	private int endBreakFrame;

	private bool isSkillEventEnd;

	private EventManager.StageCameraFocus stageCameraFocus;

	private CharacterMaterial weaponGunL;

	private ParticleSystem psWeaponL;

	private ParticleSystem psWeaponR;

	private SkinnedMeshRenderer spMeshRenderer;

	protected Vector3 _vBeamShootDir = Vector3.right;

	private ParticleSystem _wingEffect;

	private bool isPlayTeleportIn;

	private bool isPlayTeleportOut;

	private readonly string SpWeaponMesh = "DualGun_L";

	private readonly string SpMesh = "ch125_Wing";

	private readonly string WeaponParticleR = "fxuse_barrage_001_R";

	private readonly string WeaponParticleL = "fxuse_barrage_001_L";

	private readonly string FX_0_00 = "fxuse_sweepbeam_000";

	protected readonly int SKL0_TRIGGER = (int)(0.18f / GameLogicUpdateManager.m_fFrameLen);

	protected readonly int SKL0_END = (int)(0.44f / GameLogicUpdateManager.m_fFrameLen);

	protected readonly int SKL0_END_BREAK = (int)(0.556f / GameLogicUpdateManager.m_fFrameLen);

	protected readonly int SKL1_TRIGGER_1ST = (int)(0.08f / GameLogicUpdateManager.m_fFrameLen);

	protected readonly int SKL1_END_1ST = (int)(0.4f / GameLogicUpdateManager.m_fFrameLen);

	protected readonly int SKL1_END_BREAK_1ST = (int)(0.28f / GameLogicUpdateManager.m_fFrameLen);

	protected readonly int SKL1_TRIGGER_2ND = 1;

	protected readonly int SKL1_END_2ND = (int)(1f / GameLogicUpdateManager.m_fFrameLen);

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
		_refEntity.ExtraTransforms[0] = OrangeBattleUtility.FindChildRecursive(ref target, "ShootPoint_L", true);
		_refEntity.ExtraTransforms[1] = OrangeBattleUtility.FindChildRecursive(ref target, "ShootPoint_R", true);
		_refEntity.ExtraTransforms[2] = transform;
		_refEntity.PlayerSkills[0].ShootTransform[0] = _refEntity.ExtraTransforms[0];
		_refEntity.PlayerSkills[1].ShootTransform[0] = _refEntity.ExtraTransforms[0];
		Transform transform2 = OrangeBattleUtility.FindChildRecursive(ref target, SpWeaponMesh, true);
		Transform transform3 = OrangeBattleUtility.FindChildRecursive(ref target, WeaponParticleL, true);
		OrangeBattleUtility.FindChildRecursive(ref target, WeaponParticleR, true);
		weaponGunL = transform2.GetComponent<CharacterMaterial>();
		psWeaponL = transform3.GetComponent<ParticleSystem>();
		psWeaponR = transform3.GetComponent<ParticleSystem>();
		Transform transform4 = OrangeBattleUtility.FindChildRecursive(ref target, SpMesh, true);
		spMeshRenderer = transform4.GetComponent<SkinnedMeshRenderer>();
		stageCameraFocus = new EventManager.StageCameraFocus();
		stageCameraFocus.bLock = true;
		stageCameraFocus.bRightNow = true;
		_wingEffect = OrangeBattleUtility.FindChildRecursive(ref target, "CH062_WingEffect").GetComponent<ParticleSystem>();
		_wingEffect.Play(true);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(FX_0_00);
	}

	public override void OverrideDelegateEvent()
	{
		base.OverrideDelegateEvent();
		_refEntity.ChangeComboSkillEventEvt = ChangeComboSkillEvent;
		_refEntity.TeleportInCharacterDependEvt = TeleportInCharacterDepend;
		_refEntity.TeleportInCharacterDependeEndEvt = TeleportInCharacterDependeEnd;
		_refEntity.TeleportOutCharacterDependEvt = TeleportOutCharacterDepend;
		_refEntity.StageTeleportOutCharacterDependEvt = StageTeleportOutCharacterDepend;
		_refEntity.StageTeleportInCharacterDependEvt = StageTeleportInCharacterDepend;
	}

	public void TeleportInCharacterDepend()
	{
		if (!isPlayTeleportIn)
		{
			isPlayTeleportIn = true;
			ToggleSkillWeapon(false);
		}
	}

	public void TeleportInCharacterDependeEnd()
	{
		ToggleSkillWeapon(false);
	}

	public void TeleportOutCharacterDepend()
	{
		if (_refEntity.CurSubStatus == OrangeCharacter.SubStatus.TELEPORT_POSE)
		{
			float currentFrame = _refEntity.CurrentFrame;
			if (currentFrame > 1.5f && currentFrame <= 2f)
			{
				ToggleWing(false);
			}
		}
		if (!isPlayTeleportOut)
		{
			isPlayTeleportOut = true;
			weaponGunL.transform.localPosition = new Vector3(0f, 0f, 0f);
			weaponGunL.transform.localRotation = Quaternion.Euler(0f, 0f, -90f);
			CharacterMaterial getSubCharacterMaterials = weaponGunL.GetSubCharacterMaterials;
			if ((bool)getSubCharacterMaterials)
			{
				getSubCharacterMaterials.transform.localPosition = new Vector3(0f, 0f, 0f);
				getSubCharacterMaterials.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
			}
			ToggleSkillWeapon(true);
			_refEntity.CharacterMaterials.SetSubCharacterMaterial(weaponGunL);
		}
	}

	public override void PlayerPressSkillCharacterCall(int id)
	{
		if (_refEntity.CurrentActiveSkill != -1 || id != 1)
		{
			return;
		}
		int reload_index = _refEntity.PlayerSkills[1].Reload_index;
		if ((reload_index == 1 && !_refEntity.CheckUseSkillKeyTrigger(id)) || reload_index != 1)
		{
			return;
		}
		PerBuff perBuff = null;
		StageObjBase stageObjBase = null;
		for (int num = StageUpdate.runPlayers.Count - 1; num >= 0; num--)
		{
			if (StageUpdate.runPlayers[num].selfBuffManager.CheckHasMarkedEffect(115, _refEntity.sNetSerialID, out perBuff))
			{
				if (perBuff.sPlayerID == _refEntity.sPlayerID && !StageUpdate.runPlayers[num].IsDead())
				{
					stageObjBase = StageUpdate.runPlayers[num];
					break;
				}
				perBuff = null;
			}
		}
		if (perBuff != null && stageObjBase != null)
		{
			_refEntity.Controller.LogicPosition = new VInt3(stageObjBase._transform.position);
			_refEntity._transform.position = stageObjBase._transform.position;
			if (_refEntity.IsLocalPlayer)
			{
				Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_CAMERA_FOCUS, stageCameraFocus);
			}
		}
		PlayVoiceSE("v_a_skill01");
		WeaponStruct weaponStruct = _refEntity.PlayerSkills[1];
		_refEntity.CheckUsePassiveSkill(1, weaponStruct.FastBulletDatas[weaponStruct.Reload_index], weaponStruct.weaponStatus, weaponStruct.ShootTransform[0]);
		ManagedSingleton<CharacterControlHelper>.Instance.TurnToAimTarget(_refEntity);
		ManagedSingleton<CharacterControlHelper>.Instance.ChangeToSklStatus(_refEntity, id, 1, SKL1_END_2ND, OrangeCharacter.SubStatus.SKILL1_1, out skillEventFrame, out endFrame);
		ManagedSingleton<CharacterControlHelper>.Instance.SetAnimate(_refEntity, (HumanBase.AnimateId)68u, (HumanBase.AnimateId)69u, (HumanBase.AnimateId)70u);
		ToggleSkillWeapon(true);
	}

	public override void PlayerReleaseSkillCharacterCall(int id)
	{
		if (_refEntity.CurrentActiveSkill != -1)
		{
			return;
		}
		switch (id)
		{
		case 0:
			if (_refEntity.CheckUseSkillKeyTrigger(id))
			{
				PlayVoiceSE("v_a_skill05");
				endBreakFrame = GameLogicUpdateManager.GameFrame + SKL0_END_BREAK;
				_refEntity.IsShoot = 1;
				_vBeamShootDir = _refEntity.ShootDirection;
				ManagedSingleton<CharacterControlHelper>.Instance.ChangeToSklStatus(_refEntity, id, SKL0_TRIGGER, SKL0_END, OrangeCharacter.SubStatus.SKILL0, out skillEventFrame, out endFrame);
				ManagedSingleton<CharacterControlHelper>.Instance.SetAnimate(_refEntity, HumanBase.AnimateId.ANI_BTSKILL_START, (HumanBase.AnimateId)128u, (HumanBase.AnimateId)129u);
				ToggleSkillWeapon(true);
				_refEntity.DisableWeaponMesh(_refEntity.GetCurrentWeaponObj(), 0.01f);
			}
			break;
		case 1:
		{
			int reload_index = _refEntity.PlayerSkills[1].Reload_index;
			if ((reload_index != 0 || _refEntity.CheckUseSkillKeyTrigger(id)) && reload_index == 0)
			{
				PlayVoiceSE("v_a_skill04");
				endBreakFrame = GameLogicUpdateManager.GameFrame + SKL1_END_BREAK_1ST;
				ManagedSingleton<CharacterControlHelper>.Instance.TurnToAimTarget(_refEntity);
				ManagedSingleton<CharacterControlHelper>.Instance.ChangeToSklStatus(_refEntity, id, SKL1_TRIGGER_1ST, SKL1_END_1ST, OrangeCharacter.SubStatus.SKILL1, out skillEventFrame, out endFrame);
				ManagedSingleton<CharacterControlHelper>.Instance.SetAnimate(_refEntity, HumanBase.AnimateId.ANI_SKILL_START, (HumanBase.AnimateId)66u, (HumanBase.AnimateId)67u);
				ToggleSkillWeapon(true);
			}
			break;
		}
		}
	}

	public override void CheckSkill()
	{
		if (_refEntity.CurMainStatus != OrangeCharacter.MainStatus.SKILL || _refEntity.IsAnimateIDChanged() || _refEntity.CurrentActiveSkill == -1)
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
		case OrangeCharacter.SubStatus.SKILL0:
			if (nowFrame >= endFrame)
			{
				OnSkillEnd();
			}
			else if (!isSkillEventEnd && nowFrame >= skillEventFrame)
			{
				isSkillEventEnd = true;
				string cue = "a_kronos01";
				if (_refEntity.PlayerSkills[0].EnhanceEXIndex == 3)
				{
					cue = "a_kronos02";
				}
				PlaySkillSE(cue);
				WeaponStruct weaponStruct2 = _refEntity.PlayerSkills[0];
				if (_refEntity.UseAutoAim && _refEntity.IAimTargetLogicUpdate != null)
				{
					Vector3? vector = _refEntity.CalibrateAimDirection(_refEntity.ExtraTransforms[0].position, _refEntity.IAimTargetLogicUpdate);
					if (vector.HasValue)
					{
						_refEntity._characterDirection = ((vector.Value.x > 0f) ? CharacterDirection.RIGHT : CharacterDirection.LEFT);
						_refEntity.UpdateDirection();
					}
				}
				_refEntity.PlayerShootBuster(weaponStruct2, true, 0, weaponStruct2.ChargeLevel);
				_refEntity.CheckUsePassiveSkill(0, weaponStruct2.weaponStatus, weaponStruct2.ShootTransform[0]);
				ManagedSingleton<CharacterControlHelper>.Instance.Play360ShootEft(_refEntity, FX_0_00, _refEntity.PlayerSkills[0].ShootTransform[0].position);
			}
			else if (isSkillEventEnd && nowFrame >= endBreakFrame && (ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.LEFT) || ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.RIGHT)))
			{
				endFrame = nowFrame + 1;
			}
			break;
		case OrangeCharacter.SubStatus.SKILL1:
			if (nowFrame >= endFrame)
			{
				OnSkillEnd();
			}
			else if (!isSkillEventEnd && nowFrame >= skillEventFrame)
			{
				isSkillEventEnd = true;
				WeaponStruct currentSkillObj = _refEntity.GetCurrentSkillObj();
				ManagedSingleton<CharacterControlHelper>.Instance.PushBulletSkl(_refEntity, currentSkillObj.ShootTransform[0], MagazineType.NORMAL, 0, 0);
			}
			else if (isSkillEventEnd && nowFrame >= endBreakFrame && (ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.LEFT) || ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.RIGHT)))
			{
				endFrame = nowFrame + 1;
			}
			break;
		case OrangeCharacter.SubStatus.SKILL1_1:
			if (nowFrame >= endFrame)
			{
				OnSkillEnd();
			}
			else if (!isSkillEventEnd && nowFrame >= skillEventFrame)
			{
				isSkillEventEnd = true;
				PlaySkillSE("a_coda03");
				WeaponStruct weaponStruct = _refEntity.PlayerSkills[1];
				ManagedSingleton<CharacterControlHelper>.Instance.PushBulletSkl(_refEntity, _refEntity.ModelTransform, MagazineType.ENERGY, 1, 0, false);
				_refEntity.RemoveComboSkillBuff(weaponStruct.FastBulletDatas[weaponStruct.Reload_index].n_ID);
			}
			else if (isSkillEventEnd && nowFrame % 2 == 0 && (bool)psWeaponL && (bool)psWeaponR)
			{
				psWeaponL.Simulate(0f, true, true);
				psWeaponR.Simulate(0f, true, true);
				psWeaponL.Play(true);
				psWeaponR.Play(true);
			}
			break;
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
		ToggleSkillWeapon(false);
		HumanBase.AnimateId animateID = _refEntity.AnimateID;
		if (animateID != HumanBase.AnimateId.ANI_SKILL_START && animateID != (HumanBase.AnimateId)68u && animateID != HumanBase.AnimateId.ANI_BTSKILL_START)
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

	public override void ClearSkill()
	{
		isSkillEventEnd = false;
		_refEntity.IgnoreGravity = false;
		_refEntity.SkillEnd = true;
		_refEntity.CurrentActiveSkill = -1;
		_refEntity.EnableCurrentWeapon();
		if (_refEntity.BulletCollider.IsActivate)
		{
			_refEntity.BulletCollider.BackToPool();
		}
	}

	public override void SetStun(bool enable)
	{
		base.SetStun(enable);
		_refEntity.EnableCurrentWeapon();
	}

	private void ToggleSkillWeapon(bool enable)
	{
		if (enable)
		{
			weaponGunL.Appear(null, 0f);
			return;
		}
		weaponGunL.Disappear(null, 0f);
		if ((bool)psWeaponL && (bool)psWeaponR)
		{
			if (psWeaponL.isPlaying)
			{
				psWeaponL.Stop();
			}
			if (psWeaponR.isPlaying)
			{
				psWeaponR.Stop();
			}
			psWeaponL.Clear(true);
			psWeaponR.Clear(true);
		}
	}

	public override void ControlCharacterDead()
	{
		ToggleWing(false);
	}

	public override void ControlCharacterContinue()
	{
		StartCoroutine(OnToggleWing(true, 0.6f));
	}

	protected void StageTeleportInCharacterDepend()
	{
		if (spMeshRenderer != null && spMeshRenderer.enabled)
		{
			StopAllCoroutines();
			return;
		}
		ToggleWing(false);
		StartCoroutine(OnToggleWing(true, 0.6f));
	}

	protected void StageTeleportOutCharacterDepend()
	{
		if (_refEntity.CurMainStatus != OrangeCharacter.MainStatus.TELEPORT_OUT)
		{
			StartCoroutine(OnToggleWing(false, 0.2f));
		}
		else if (!_refEntity.Animator.IsDefaultAnimator)
		{
			StartCoroutine(OnToggleWing(false, 0.2f));
		}
	}

	private IEnumerator OnToggleWing(bool isActive, float delay)
	{
		yield return new WaitForSeconds(delay);
		ToggleWing(isActive);
	}

	private void ToggleWing(bool isActive)
	{
		spMeshRenderer.enabled = isActive;
		if (isActive)
		{
			_wingEffect.Play(true);
		}
		else
		{
			_wingEffect.Stop(true);
		}
	}

	public void ChangeComboSkillEvent(object[] parameters)
	{
		if (parameters.Length == 2)
		{
			int num = (int)parameters[0];
			int num2 = (int)parameters[1];
			if (_refEntity.CurMainStatus != OrangeCharacter.MainStatus.TELEPORT_IN && _refEntity.CurMainStatus != OrangeCharacter.MainStatus.TELEPORT_OUT && (int)_refEntity.Hp > 0 && num == 1 && _refEntity.PlayerSkills[1].Reload_index != num2)
			{
				_refEntity.PlayerSkills[1].Reload_index = num2;
			}
		}
	}

	public override string[] GetCharacterDependAnimations()
	{
		return new string[6] { "ch125_skill_01_step1_crouch", "ch125_skill_01_step1_stand", "ch125_skill_01_step1_jump", "ch125_skill_01_step2_crouch", "ch125_skill_01_step2_stand", "ch125_skill_01_step2_jump" };
	}

	public override string[][] GetCharacterDependAnimationsBlendTree()
	{
		string[] array = new string[3] { "ch125_skill_02_crouch_up", "ch125_skill_02_crouch_mid", "ch125_skill_02_crouch_down" };
		string[] array2 = new string[3] { "ch125_skill_02_stand_up", "ch125_skill_02_stand_mid", "ch125_skill_02_stand_down" };
		string[] array3 = new string[3] { "ch125_skill_02_jump_up", "ch125_skill_02_jump_mid", "ch125_skill_02_jump_down" };
		return new string[3][] { array, array2, array3 };
	}
}
