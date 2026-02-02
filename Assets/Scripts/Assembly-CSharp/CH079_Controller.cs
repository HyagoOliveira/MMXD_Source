using System;
using CriWare;
using StageLib;
using UnityEngine;

public class CH079_Controller : CharacterControlBase, IPetSummoner
{
	protected bool bInSkill;

	protected bool bNextAnime;

	protected SkinnedMeshRenderer _tfWeaponMesh;

	protected SkinnedMeshRenderer _tfLHandMesh;

	protected SkinnedMeshRenderer _tfRHandMesh;

	protected SkinnedMeshRenderer _tfSaberMeshM;

	protected SkinnedMeshRenderer _tfSaberMeshG;

	protected ParticleSystem _fxSaber;

	protected int _enhanceSlot;

	protected ChargeShootObj _refChargeShootObj;

	protected int _nSkill1AniIndex;

	protected ParticleSystem _fxAura;

	protected ParticleSystem _fxAura2;

	protected SCH022Controller _sch022;

	private string[] Cues = new string[2] { "BattleSE", "bt_boss05" };

	public int PetID { get; set; } = -1;


	public long PetTime { get; set; }

	public int PetCount { get; set; }

	public override string[] GetCharacterDependAnimations()
	{
		return new string[15]
		{
			"ch079_skill_01_stand_mid", "ch079_skill_01_jump_mid", "ch079_skill_01_crouch_mid", "ch079_skill_02_stand_mid", "ch079_skill_02_jump_mid", "ch079_skill_02_crouch_mid", "ch079_skill_02_slash_stand", "ch079_skill_02_slash_jump", "ch079_skill_02_slash_crouch", "ch079_dive_trigger_stand_start",
			"ch079_dive_trigger_stand_end", "ch079_dive_trigger_jump_start", "ch079_dive_trigger_jump_end", "ch079_dive_trigger_crouch_start", "ch079_dive_trigger_crouch_end"
		};
	}

	public override string[][] GetCharacterDependAnimationsBlendTree()
	{
		string[] array = new string[3] { "ch079_skill_01_stand_up", "ch079_skill_01_stand_mid", "ch079_skill_01_stand_down" };
		string[] array2 = new string[3] { "ch079_skill_01_jump_up", "ch079_skill_01_jump_mid", "ch079_skill_01_jump_end" };
		string[] array3 = new string[3] { "ch079_skill_01_crouch_up", "ch079_skill_01_crouch_mid", "ch079_skill_01_crouch_down" };
		string[] array4 = new string[3] { "ch079_skill_02_stand_up", "ch079_skill_02_stand_mid", "ch079_skill_02_stand_down" };
		string[] array5 = new string[3] { "ch079_skill_02_jump_up", "ch079_skill_02_jump_mid", "ch079_skill_02_jump_down" };
		string[] array6 = new string[3] { "ch079_skill_02_crouch_up", "ch079_skill_02_crouch_mid", "ch079_skill_02_crouch_down" };
		return new string[6][] { array, array2, array3, array4, array5, array6 };
	}

	public override void Start()
	{
		base.Start();
		InitLinkSkill();
		InitEnhanceSkill();
		InitPet();
		InitExtraMeshData();
	}

	private void InitExtraMeshData()
	{
		Transform[] target = _refEntity._transform.GetComponentsInChildren<Transform>(true);
		_refEntity.ExtraTransforms = new Transform[3];
		_refEntity.ExtraTransforms[0] = OrangeBattleUtility.FindChildRecursive(ref target, "L WeaponPoint", true);
		_refEntity.ExtraTransforms[1] = OrangeBattleUtility.FindChildRecursive(ref target, "R WeaponPoint", true);
		_refEntity.ExtraTransforms[2] = OrangeBattleUtility.FindChildRecursive(ref target, "Bip L Hand", true);
		Transform transform = OrangeBattleUtility.FindChildRecursive(ref target, "BusterMesh_m", true);
		_tfWeaponMesh = transform.GetComponent<SkinnedMeshRenderer>();
		_tfWeaponMesh.enabled = false;
		Transform transform2 = OrangeBattleUtility.FindChildRecursive(ref target, "HandMesh_L_c", true);
		_tfLHandMesh = transform2.GetComponent<SkinnedMeshRenderer>();
		_tfLHandMesh.enabled = true;
		Transform transform3 = OrangeBattleUtility.FindChildRecursive(ref target, "HandMesh_R_c", true);
		_tfRHandMesh = transform3.GetComponent<SkinnedMeshRenderer>();
		_tfRHandMesh.enabled = true;
		Transform transform4 = OrangeBattleUtility.FindChildRecursive(ref target, "SaberMesh_m", true);
		_tfSaberMeshM = transform4.GetComponent<SkinnedMeshRenderer>();
		_tfSaberMeshM.enabled = false;
		Transform transform5 = OrangeBattleUtility.FindChildRecursive(ref target, "SaberMesh_g", true);
		_tfSaberMeshG = transform5.GetComponent<SkinnedMeshRenderer>();
		_tfSaberMeshG.enabled = false;
		Transform transform6 = OrangeBattleUtility.FindChildRecursive(ref target, "SaberFX", true);
		_fxSaber = transform6.GetComponent<ParticleSystem>();
		_fxSaber.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
		_fxAura = OrangeBattleUtility.FindChildRecursive(ref target, "tttt_002 (1)", true).GetComponent<ParticleSystem>();
		_fxAura2 = OrangeBattleUtility.FindChildRecursive(ref target, "fxdemo_ch079_body_digital_code", true).GetComponent<ParticleSystem>();
		_refChargeShootObj = _refEntity.ChargeObject;
		_refChargeShootObj.StopCharge();
	}

	private void InitLinkSkill()
	{
	}

	private void InitEnhanceSkill()
	{
		_enhanceSlot = _refEntity.PlayerSkills[1].EnhanceEXIndex;
		if (_enhanceSlot == 0)
		{
			return;
		}
		int skillId = (new int[4] { 19331, 19332, 19333, 19334 })[_enhanceSlot];
		_refEntity.ReInitSkillStruct(1, skillId);
		for (int i = 0; i < _refEntity.PlayerSkills[1].FastBulletDatas.Length; i++)
		{
			if (!MonoBehaviourSingleton<PoolManager>.Instance.IsPreload(_refEntity.PlayerSkills[1].FastBulletDatas[i].s_MODEL))
			{
				BulletBase.PreloadBullet<BasicBullet>(_refEntity.PlayerSkills[1].FastBulletDatas[i]);
			}
		}
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_ch079_skill_000", 5);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_ch079_skill2_000", 5);
		if (_enhanceSlot == 0)
		{
			MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_exechargeshot_0031", 5);
		}
		else if (_enhanceSlot == 1)
		{
			MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_bodyguard_000", 5);
		}
		else if (_enhanceSlot == 2)
		{
			MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_poisonphar_000", 5);
		}
		else if (_enhanceSlot == 3)
		{
			MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_dreamsword_000", 5);
		}
	}

	private void InitPet()
	{
		if (_enhanceSlot == 1)
		{
			ManagedSingleton<CharacterControlHelper>.Instance.PetInit<SCH022Controller>(this, _refEntity, 1, _refEntity.PlayerSkills[1].BulletData);
		}
	}

	public override void OverrideDelegateEvent()
	{
		base.OverrideDelegateEvent();
		_refEntity.AnimationEndCharacterDependEvt = AnimationEndCharacterDepend;
		_refEntity.SetStatusCharacterDependEvt = SetStatusCharacterDepend;
		_refEntity.CheckSkillLockDirectionEvt = CheckSkillLockDirection;
		if (base.gameObject.GetComponent<OrangeNPCCharacter>() != null && StageUpdate.gStageName == "stage04_3301_e1")
		{
			_refEntity.TeleportOutCharacterSE = TeleportoutSE;
		}
		_refEntity.TeleportOutCharacterDependEvt = TeleportOutCharacterDepend;
		_refEntity.StageTeleportOutCharacterDependEvt = StageTeleportOutCharacterDepend;
		_refEntity.StageTeleportInCharacterDependEvt = StageTeleportInCharacterDepend;
		_refEntity.PlayerReleaseSkillCB = PlayerReleaseSkill;
		_refEntity.CheckPetActiveEvt = CheckPetActive;
	}

	public override void ClearSkill()
	{
		if (_refEntity.CurrentActiveSkill == 0)
		{
			CancelSkill0();
		}
		else if (_refEntity.CurrentActiveSkill == 1)
		{
			CancelSkill1();
		}
		_refEntity.CurrentActiveSkill = -1;
	}

	public override void CheckSkill()
	{
		if (!_refEntity.IsAnimateIDChanged())
		{
			UpdateSkill();
		}
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
			if (!_refEntity.CheckUseSkillKeyTrigger(id))
			{
				break;
			}
			if (_refEntity.PlayerSetting.AutoCharge == 1)
			{
				if (!_refEntity.PlayerSkills[id].ChargeTimer.IsStarted() && _refEntity.PlayerSkills[0].Reload_index == 0)
				{
					_refEntity.PlayerSkills[id].ChargeTimer.TimerStart();
				}
				else if (_refEntity.CurrentActiveSkill == -1)
				{
					UseSkill0(id);
				}
			}
			else if (_refEntity.CurrentActiveSkill == -1)
			{
				UseSkill0(id);
				_refEntity.PlaySE(_refEntity.VoiceID, 8);
			}
			break;
		case 1:
			if (_refEntity.CheckUseSkillKeyTrigger(id))
			{
				PlayVoiceSE("v_re_skill02");
				if (_enhanceSlot == 1)
				{
					PlaySkillSE("re_panel02_2");
					PlayVoiceSE("v_re_skill02_2");
					PlaySkillSE("re_bodyguard01");
					CallBodyGuard(_refEntity.PlayerSkills[1]);
				}
				else
				{
					UseSkill1(id);
				}
			}
			break;
		}
	}

	public void PlayerReleaseSkill(int id)
	{
		OrangeCharacter.MainStatus curMainStatus = _refEntity.CurMainStatus;
		if (curMainStatus == OrangeCharacter.MainStatus.HURT)
		{
			return;
		}
		if (_refEntity.PlayerSkills[id].LastUseTimer.GetMillisecond() < _refEntity.PlayerSkills[id].BulletData.n_FIRE_SPEED || _refEntity.PlayerSkills[id].MagazineRemain <= 0f || _refEntity.PlayerSkills[id].ForceLock || _refEntity.CurrentActiveSkill != -1)
		{
			if (_refEntity.PlayerSetting.AutoCharge == 0)
			{
				_refChargeShootObj.StopCharge(id);
			}
		}
		else
		{
			_refEntity.PlayerReleaseSkillCharacterCallCB.CheckTargetToInvoke(id);
			_refEntity.PreBelow = _refEntity.Controller.Collisions.below;
		}
	}

	public override void PlayerReleaseSkillCharacterCall(int id)
	{
		if (_refEntity.CurrentActiveSkill == -1 && id == 0 && _refEntity.CurrentActiveSkill == -1 && _refEntity.PlayerSetting.AutoCharge != 1 && _refEntity.CheckUseSkillKeyTrigger(id))
		{
			UseSkill0(id);
		}
	}

	public void SetStatusCharacterDepend(OrangeCharacter.MainStatus mainStatus, OrangeCharacter.SubStatus subStatus)
	{
		switch (mainStatus)
		{
		case OrangeCharacter.MainStatus.TELEPORT_IN:
			break;
		case OrangeCharacter.MainStatus.TELEPORT_OUT:
			switch (subStatus)
			{
			case OrangeCharacter.SubStatus.WIN_POSE:
				ToggleWeapon(-2);
				break;
			case OrangeCharacter.SubStatus.TELEPORT_POSE:
				ToggleWeapon(-3);
				break;
			}
			break;
		case OrangeCharacter.MainStatus.SKILL:
			switch (subStatus)
			{
			case OrangeCharacter.SubStatus.SKILL0:
				_refEntity.SetAnimateId(HumanBase.AnimateId.ANI_BTSKILL_START);
				break;
			case OrangeCharacter.SubStatus.SKILL0_1:
				_refEntity.IgnoreGravity = true;
				_refEntity.SetAnimateId((HumanBase.AnimateId)128u);
				break;
			case OrangeCharacter.SubStatus.SKILL0_2:
				_refEntity.SetAnimateId((HumanBase.AnimateId)129u);
				break;
			case OrangeCharacter.SubStatus.SKILL1:
				_refEntity.SetAnimateId(HumanBase.AnimateId.ANI_SKILL_START);
				break;
			case OrangeCharacter.SubStatus.SKILL1_1:
				_refEntity.IgnoreGravity = true;
				_refEntity.SetAnimateId((HumanBase.AnimateId)66u);
				break;
			case OrangeCharacter.SubStatus.SKILL1_2:
				_refEntity.SetAnimateId((HumanBase.AnimateId)67u);
				break;
			case OrangeCharacter.SubStatus.SKILL1_6:
				_nSkill1AniIndex = 74;
				_refEntity.SetAnimateId((HumanBase.AnimateId)74u);
				break;
			case OrangeCharacter.SubStatus.SKILL1_7:
				_refEntity.IgnoreGravity = true;
				_nSkill1AniIndex = 76;
				_refEntity.SetAnimateId((HumanBase.AnimateId)76u);
				break;
			case OrangeCharacter.SubStatus.SKILL1_8:
				_nSkill1AniIndex = 78;
				_refEntity.SetAnimateId((HumanBase.AnimateId)78u);
				break;
			case OrangeCharacter.SubStatus.SKILL1_9:
				_refEntity.SetAnimateId((HumanBase.AnimateId)71u);
				break;
			case OrangeCharacter.SubStatus.SKILL1_10:
				_refEntity.IgnoreGravity = true;
				_refEntity.SetAnimateId((HumanBase.AnimateId)72u);
				break;
			case OrangeCharacter.SubStatus.SKILL1_11:
				_refEntity.SetAnimateId((HumanBase.AnimateId)73u);
				break;
			}
			break;
		case OrangeCharacter.MainStatus.SLASH:
			break;
		}
	}

	public void AnimationEndCharacterDepend(OrangeCharacter.MainStatus mainStatus, OrangeCharacter.SubStatus subStatus)
	{
		switch (mainStatus)
		{
		case OrangeCharacter.MainStatus.TELEPORT_IN:
			if (subStatus == OrangeCharacter.SubStatus.TELEPORT_POSE)
			{
				ToggleWeapon(0);
			}
			break;
		case OrangeCharacter.MainStatus.SKILL:
			switch (subStatus)
			{
			case OrangeCharacter.SubStatus.SKILL0:
			case OrangeCharacter.SubStatus.SKILL0_1:
				SkillEndChnageToIdle();
				break;
			case OrangeCharacter.SubStatus.SKILL0_2:
				SkillEndChnageToIdle(true);
				break;
			case OrangeCharacter.SubStatus.SKILL1:
			case OrangeCharacter.SubStatus.SKILL1_1:
			case OrangeCharacter.SubStatus.SKILL1_9:
			case OrangeCharacter.SubStatus.SKILL1_10:
				SkillEndChnageToIdle();
				break;
			case OrangeCharacter.SubStatus.SKILL1_2:
			case OrangeCharacter.SubStatus.SKILL1_11:
				SkillEndChnageToIdle(true);
				break;
			}
			break;
		}
	}

	public override void CreateSkillBullet(WeaponStruct wsSkill)
	{
		_refEntity.FreshBullet = true;
		OrangeCharacter.MainStatus curMainStatus = _refEntity.CurMainStatus;
		if (curMainStatus != OrangeCharacter.MainStatus.SKILL)
		{
			return;
		}
		switch (_refEntity.CurSubStatus)
		{
		case OrangeCharacter.SubStatus.SKILL0:
		case OrangeCharacter.SubStatus.SKILL0_1:
		case OrangeCharacter.SubStatus.SKILL0_2:
			_refEntity.PushBulletDetail(wsSkill.FastBulletDatas[wsSkill.ChargeLevel], wsSkill.weaponStatus, _refEntity.ExtraTransforms[2], wsSkill.SkillLV);
			_refEntity.CheckUsePassiveSkill(0, wsSkill.weaponStatus, wsSkill.ShootTransform[0]);
			OrangeBattleUtility.UpdateSkillCD(wsSkill, wsSkill.FastBulletDatas[wsSkill.ChargeLevel].n_USE_COST, -1f);
			_refChargeShootObj.StopCharge();
			break;
		case OrangeCharacter.SubStatus.SKILL1:
		case OrangeCharacter.SubStatus.SKILL1_1:
		case OrangeCharacter.SubStatus.SKILL1_2:
		case OrangeCharacter.SubStatus.SKILL1_3:
		case OrangeCharacter.SubStatus.SKILL1_4:
		case OrangeCharacter.SubStatus.SKILL1_5:
		case OrangeCharacter.SubStatus.SKILL1_6:
		case OrangeCharacter.SubStatus.SKILL1_7:
		case OrangeCharacter.SubStatus.SKILL1_8:
		case OrangeCharacter.SubStatus.SKILL1_9:
		case OrangeCharacter.SubStatus.SKILL1_10:
		case OrangeCharacter.SubStatus.SKILL1_11:
			if (_enhanceSlot == 0)
			{
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_exechargeshot_0031", _refEntity.ExtraTransforms[2].position, (_refEntity._characterDirection == CharacterDirection.RIGHT) ? OrangeCharacter.NormalQuaternion : OrangeCharacter.ReversedQuaternion, Array.Empty<object>());
				_refEntity.PushBulletDetail(wsSkill.BulletData, wsSkill.weaponStatus, _refEntity.ModelTransform, wsSkill.SkillLV);
			}
			else if (_enhanceSlot == 1)
			{
				CreatePets(wsSkill);
			}
			else if (_enhanceSlot == 2)
			{
				_refEntity.PushBulletDetail(wsSkill.BulletData, wsSkill.weaponStatus, _refEntity.ModelTransform, wsSkill.SkillLV);
			}
			else if (_enhanceSlot == 3)
			{
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_dreamsword_000", _refEntity.ModelTransform.position, (_refEntity._characterDirection == CharacterDirection.RIGHT) ? OrangeCharacter.NormalQuaternion : OrangeCharacter.ReversedQuaternion, Array.Empty<object>());
				_refEntity.PushBulletDetail(wsSkill.BulletData, wsSkill.weaponStatus, _refEntity.ModelTransform, wsSkill.SkillLV);
			}
			_refEntity.CheckUsePassiveSkill(1, wsSkill.weaponStatus, wsSkill.ShootTransform[0]);
			OrangeBattleUtility.UpdateSkillCD(wsSkill);
			break;
		}
	}

	protected void CallBodyGuard(WeaponStruct wsSkill)
	{
		if (_enhanceSlot == 1)
		{
			CreatePets(wsSkill);
			_refEntity.CheckUsePassiveSkill(1, wsSkill.weaponStatus, wsSkill.ShootTransform[0]);
			OrangeBattleUtility.UpdateSkillCD(wsSkill);
		}
	}

	protected void CreatePets(WeaponStruct wsSkill)
	{
		if (_refEntity.IsLocalPlayer)
		{
			CallPet(PetID, false, -1, null);
		}
	}

	public override void CallPet(int petID, bool isHurt, int nSetNumID = -1, Vector3? vSetPos = null)
	{
		MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_bodyguard_000", _refEntity.AimPosition, Quaternion.identity, Array.Empty<object>());
		MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_ch079_skill_000", _refEntity.ModelTransform.position, Quaternion.identity, Array.Empty<object>());
		SCH022Controller sCH022Controller = ManagedSingleton<CharacterControlHelper>.Instance.CallPet<SCH022Controller>(this, _refEntity, petID, nSetNumID, true, true, false);
		if ((bool)sCH022Controller)
		{
			sCH022Controller.transform.SetParentNull();
			sCH022Controller.SetFollowOffset(new Vector3(0f, 2f, 0f));
			sCH022Controller.activeSE = new string[2] { "", "" };
			sCH022Controller.unactiveSE = new string[2] { "", "" };
			sCH022Controller.SetActive(true);
			_sch022 = sCH022Controller;
		}
	}

	protected new void RemovePet()
	{
		if (_sch022 != null && _sch022.Activate)
		{
			_sch022.SetActive(false);
			_sch022 = null;
		}
	}

	public void CheckSkillLockDirection()
	{
		OrangeCharacter.SubStatus curSubStatus = _refEntity.CurSubStatus;
		_refEntity._characterDirection = (CharacterDirection)((int)_refEntity._characterDirection * -1);
	}

	public override void ControlCharacterDead()
	{
	}

	public CriAtomExPlayback TeleportoutSE()
	{
		return _refEntity.SoundSource.PlaySE(Cues[0], Cues[1]);
	}

	public void TeleportOutCharacterDepend()
	{
	}

	public void StageTeleportOutCharacterDepend()
	{
		if ((bool)_fxAura)
		{
			_fxAura.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
		}
		if ((bool)_fxAura2)
		{
			_fxAura2.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
		}
	}

	public void StageTeleportInCharacterDepend()
	{
		if ((bool)_fxAura)
		{
			_fxAura.Play(true);
		}
		if ((bool)_fxAura2)
		{
			_fxAura2.Play(true);
		}
	}

	private void UpdateSkill()
	{
		OrangeCharacter.MainStatus curMainStatus = _refEntity.CurMainStatus;
		if (curMainStatus != OrangeCharacter.MainStatus.SKILL)
		{
			return;
		}
		switch (_refEntity.CurSubStatus)
		{
		case OrangeCharacter.SubStatus.SKILL0:
		case OrangeCharacter.SubStatus.SKILL0_1:
		case OrangeCharacter.SubStatus.SKILL0_2:
			if (_refEntity.CurrentFrame > 1f)
			{
				SkillEndChnageToIdle();
			}
			else if (bInSkill && _refEntity.CurrentFrame > 0.1f)
			{
				bInSkill = false;
				CreateSkillBullet(_refEntity.PlayerSkills[0]);
			}
			else if (CheckCancelAnimate(0) && _refEntity.CurrentFrame > 0.4f)
			{
				SkipSkill0Animation();
			}
			break;
		case OrangeCharacter.SubStatus.SKILL1:
		case OrangeCharacter.SubStatus.SKILL1_1:
		case OrangeCharacter.SubStatus.SKILL1_2:
			if (_refEntity.CurrentFrame > 1f)
			{
				SkillEndChnageToIdle();
			}
			else if (bInSkill && _refEntity.CurrentFrame > 0.1f)
			{
				CreateSkillBullet(_refEntity.PlayerSkills[1]);
				bInSkill = false;
			}
			else if (CheckCancelAnimate(1) && _refEntity.CurrentFrame > 0.4f)
			{
				SkipSkill1Animation();
			}
			break;
		case OrangeCharacter.SubStatus.SKILL1_6:
		case OrangeCharacter.SubStatus.SKILL1_7:
		case OrangeCharacter.SubStatus.SKILL1_8:
			if (!bNextAnime)
			{
				if (_refEntity.CurrentFrame > 2f)
				{
					bNextAnime = true;
					_refEntity.SetAnimateId((HumanBase.AnimateId)(_nSkill1AniIndex + 1));
				}
				else if (bInSkill && _refEntity.CurrentFrame > 0.4f)
				{
					CreateSkillBullet(_refEntity.PlayerSkills[1]);
					bInSkill = false;
				}
				else if (_refEntity.CurrentFrame > 1f && CheckCancelAnimate(1))
				{
					SkipSkill1Animation();
				}
			}
			else if (_refEntity.CurrentFrame > 1f)
			{
				SkillEndChnageToIdle();
			}
			else if (CheckCancelAnimate(1))
			{
				SkipSkill1Animation();
			}
			break;
		case OrangeCharacter.SubStatus.SKILL1_9:
		case OrangeCharacter.SubStatus.SKILL1_10:
		case OrangeCharacter.SubStatus.SKILL1_11:
			if (_refEntity.CurrentFrame > 1f)
			{
				SkillEndChnageToIdle();
			}
			else if (bInSkill && _refEntity.CurrentFrame > 0.15f)
			{
				CreateSkillBullet(_refEntity.PlayerSkills[1]);
				bInSkill = false;
			}
			else if (CheckCancelAnimate(1) && _refEntity.CurrentFrame > 0.42f)
			{
				SkipSkill1Animation();
			}
			break;
		}
	}

	private void TurnToAimTarget()
	{
		Vector3? vector = _refEntity.CalibrateAimDirection(_refEntity.AimPosition);
		if (vector.HasValue)
		{
			int num = Math.Sign(vector.Value.x);
			if (_refEntity.direction != num && Mathf.Abs(vector.Value.x) > 0.05f)
			{
				_refEntity._characterDirection = (CharacterDirection)((int)_refEntity._characterDirection * -1);
				_refEntity.ShootDirection = vector.Value;
			}
		}
	}

	private void TurnToShootDirection(Vector3 dir)
	{
		int num = Math.Sign(dir.x);
		if (_refEntity.direction != num && Mathf.Abs(dir.x) > 0.05f)
		{
			_refEntity._characterDirection = (CharacterDirection)((int)_refEntity._characterDirection * -1);
			_refEntity.ShootDirection = dir;
		}
	}

	private void UseSkill0(int skillId)
	{
		bInSkill = true;
		_refEntity.CurrentActiveSkill = skillId;
		_refEntity.SkillEnd = false;
		_refEntity.PlayerStopDashing();
		_refEntity.SetSpeed(0, 0);
		_refEntity.IsShoot = 1;
		ToggleWeapon(1);
		PlayVoiceSE("v_re_skill01");
		PlaySkillSE("re_panel01");
		MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_ch079_skill_000", _refEntity.ModelTransform.position, Quaternion.identity, Array.Empty<object>());
		if (_refEntity.CurMainStatus == OrangeCharacter.MainStatus.CROUCH)
		{
			_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0_2);
		}
		else if (_refEntity.Controller.Collisions.below)
		{
			_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0);
		}
		else
		{
			_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0_1);
		}
	}

	private void CancelSkill0()
	{
		_refEntity.SkillEnd = true;
		SkipSkill0Animation();
	}

	private void SkipSkill0Animation()
	{
		SkillEndChnageToIdle();
	}

	private void UseSkill1(int skillId)
	{
		bInSkill = true;
		bNextAnime = false;
		_refEntity.CurrentActiveSkill = skillId;
		_refEntity.SkillEnd = false;
		_refEntity.PlayerStopDashing();
		_refEntity.SetSpeed(0, 0);
		_refEntity.IsShoot = 1;
		ToggleWeapon(2);
		TurnToAimTarget();
		_refEntity.ShootDirection = ((_refEntity._characterDirection == CharacterDirection.RIGHT) ? Vector3.right : Vector3.left);
		OrangeCharacter.SubStatus subStatus = OrangeCharacter.SubStatus.SKILL1;
		switch (_enhanceSlot)
		{
		case 0:
			PlaySkillSE("re_panel02_1");
			PlayVoiceSE("v_re_skill02_1");
			MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_ch079_skill2_000", _refEntity.ModelTransform.position, Quaternion.identity, Array.Empty<object>());
			break;
		case 2:
			PlaySkillSE("re_panel02_2");
			PlayVoiceSE("v_re_skill02_3");
			MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_ch079_skill_000", _refEntity.ModelTransform.position, Quaternion.identity, Array.Empty<object>());
			subStatus += 6;
			break;
		case 3:
			PlaySkillSE("re_panel02_2");
			PlayVoiceSE("v_re_skill02_4");
			MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_ch079_skill_000", _refEntity.ModelTransform.position, Quaternion.identity, Array.Empty<object>());
			subStatus += 9;
			break;
		}
		if (_refEntity.CurMainStatus == OrangeCharacter.MainStatus.CROUCH)
		{
			subStatus += 2;
		}
		else if (!_refEntity.Controller.Collisions.below)
		{
			subStatus++;
		}
		_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, subStatus);
	}

	private void CancelSkill1()
	{
		_refEntity.SkillEnd = true;
		SkipSkill1Animation();
	}

	private void SkipSkill1Animation()
	{
		if (_refEntity.CurSubStatus == OrangeCharacter.SubStatus.SKILL1_2 || _refEntity.CurSubStatus == OrangeCharacter.SubStatus.SKILL1_5 || _refEntity.CurSubStatus == OrangeCharacter.SubStatus.SKILL1_8 || _refEntity.CurSubStatus == OrangeCharacter.SubStatus.SKILL1_11)
		{
			SkillEndChnageToIdle(true);
		}
		else
		{
			SkillEndChnageToIdle();
		}
	}

	private bool CheckCancelAnimate(int skilliD)
	{
		switch (skilliD)
		{
		case 0:
			if (_refEntity.CurMainStatus == OrangeCharacter.MainStatus.SKILL && _refEntity.CurSubStatus == OrangeCharacter.SubStatus.SKILL0_2)
			{
				if (ManagedSingleton<InputStorage>.Instance.IsAnyHeld(_refEntity.UserID) && !ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.SKILL0) && !ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.DOWN))
				{
					return true;
				}
			}
			else if (ManagedSingleton<InputStorage>.Instance.IsAnyHeld(_refEntity.UserID) && !ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.SKILL0))
			{
				return true;
			}
			break;
		case 1:
			if (_refEntity.CurMainStatus == OrangeCharacter.MainStatus.SKILL && (_refEntity.CurSubStatus == OrangeCharacter.SubStatus.SKILL1_2 || _refEntity.CurSubStatus == OrangeCharacter.SubStatus.SKILL1_5 || _refEntity.CurSubStatus == OrangeCharacter.SubStatus.SKILL1_8 || _refEntity.CurSubStatus == OrangeCharacter.SubStatus.SKILL1_11))
			{
				if (ManagedSingleton<InputStorage>.Instance.IsAnyHeld(_refEntity.UserID) && !ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.SKILL1) && !ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.DOWN))
				{
					return true;
				}
			}
			else if (ManagedSingleton<InputStorage>.Instance.IsAnyHeld(_refEntity.UserID) && !ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.SKILL1))
			{
				return true;
			}
			break;
		}
		return false;
	}

	private void SkillEndChnageToIdle(bool isCrouch = false)
	{
		_refEntity.SkillEnd = true;
		_refEntity.Dashing = false;
		_refEntity.IgnoreGravity = false;
		_refEntity.GravityMultiplier = new VInt(1f);
		_refEntity.Animator._animator.speed = 1f;
		bInSkill = false;
		ToggleWeapon(0);
		if (isCrouch)
		{
			if (ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.DOWN))
			{
				_refEntity.SetStatus(OrangeCharacter.MainStatus.CROUCH, OrangeCharacter.SubStatus.WIN_POSE);
			}
			else
			{
				_refEntity.SetStatus(OrangeCharacter.MainStatus.IDLE, OrangeCharacter.SubStatus.CROUCH_UP);
			}
		}
		else if (_refEntity.Controller.Collisions.below)
		{
			_refEntity.SetStatus(OrangeCharacter.MainStatus.IDLE, OrangeCharacter.SubStatus.IDLE);
		}
		else
		{
			_refEntity.SetStatus(OrangeCharacter.MainStatus.FALL, OrangeCharacter.SubStatus.TELEPORT_POSE);
		}
	}

	private void ToggleWeapon(int style)
	{
		switch (style)
		{
		case -3:
			if (_refEntity.CheckCurrentWeaponIndex())
			{
				_refEntity.DisableWeaponMesh(_refEntity.GetCurrentWeaponObj(), 0.01f);
			}
			_tfWeaponMesh.enabled = false;
			_tfLHandMesh.enabled = true;
			SetRightHandSaber(false);
			break;
		case -2:
			if (_refEntity.CheckCurrentWeaponIndex())
			{
				_refEntity.DisableWeaponMesh(_refEntity.GetCurrentWeaponObj(), 0.01f);
			}
			_tfWeaponMesh.enabled = false;
			_tfLHandMesh.enabled = true;
			SetRightHandSaber(false);
			break;
		case -1:
			if (_refEntity.CheckCurrentWeaponIndex())
			{
				_refEntity.DisableWeaponMesh(_refEntity.GetCurrentWeaponObj(), 0.01f);
			}
			_tfWeaponMesh.enabled = false;
			_tfLHandMesh.enabled = true;
			SetRightHandSaber(false);
			break;
		case 1:
			if (_refEntity.CheckCurrentWeaponIndex())
			{
				_refEntity.DisableWeaponMesh(_refEntity.GetCurrentWeaponObj(), 0.01f);
			}
			_tfWeaponMesh.enabled = true;
			_tfLHandMesh.enabled = false;
			SetRightHandSaber(false);
			break;
		case 2:
			if (_refEntity.CheckCurrentWeaponIndex())
			{
				_refEntity.DisableWeaponMesh(_refEntity.GetCurrentWeaponObj(), 0.01f);
			}
			if (_enhanceSlot == 0)
			{
				_tfWeaponMesh.enabled = true;
				_tfLHandMesh.enabled = false;
				SetRightHandSaber(false);
			}
			else if (_enhanceSlot == 3)
			{
				_tfWeaponMesh.enabled = false;
				_tfLHandMesh.enabled = true;
				SetRightHandSaber(true);
			}
			else
			{
				_tfWeaponMesh.enabled = false;
				_tfLHandMesh.enabled = true;
				SetRightHandSaber(false);
			}
			break;
		default:
			if (_refEntity.CheckCurrentWeaponIndex())
			{
				_refEntity.EnableCurrentWeapon();
			}
			_tfWeaponMesh.enabled = false;
			_tfLHandMesh.enabled = true;
			SetRightHandSaber(false);
			break;
		}
	}

	private void SetRightHandSaber(bool enable)
	{
		if (enable)
		{
			_tfRHandMesh.enabled = false;
			_tfSaberMeshM.enabled = true;
			_tfSaberMeshG.enabled = true;
			_fxSaber.Play();
		}
		else
		{
			_tfRHandMesh.enabled = true;
			_tfSaberMeshM.enabled = false;
			_tfSaberMeshG.enabled = false;
			_fxSaber.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
		}
	}

	public bool CheckPetActive(int petId)
	{
		if (_sch022 != null && _sch022.Activate && _sch022.PetID == petId)
		{
			return true;
		}
		return false;
	}
}
