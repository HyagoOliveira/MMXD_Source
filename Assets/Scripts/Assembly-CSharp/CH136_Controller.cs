using System;
using System.Collections;
using UnityEngine;
using enums;

public class CH136_Controller : CharacterControlBase
{
	private int nowFrame;

	private int skillEventFrame;

	private int endFrame;

	private int endBreakFrame;

	private bool isSkillEventEnd;

	protected int _enhanceSlot0;

	protected int _enhanceSlot1;

	private SKILL_TABLE linkSkl1;

	private FxBase tSkill1Fx;

	private bool isShowSkillWeapon;

	private SkinnedMeshRenderer tfLHandMesh;

	private SkinnedMeshRenderer tfLBusterMesh;

	private SkinnedMeshRenderer tfRHandMesh;

	private SkinnedMeshRenderer tfRBusterMesh;

	private CharacterMaterial cmSaber;

	private SkinnedMeshRenderer spMeshRenderer;

	private ParticleSystem _wingEffect;

	private FxBase fx_001_ex1_000;

	private Vector3 shootDirection = Vector3.right;

	private float dashExSpd = 6f;

	private readonly int SKL1_EX1_TRIGGER_1ST = (int)(0.19f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL1_EX1_TRIGGER_2ND = (int)(0.46f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL1_EX1_END = (int)(1f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL1_EX1_END_BREAK = (int)(0.65f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL1_EX3_TRIGGER = (int)(0.24f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL1_EX3_END = (int)(0.767f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL1_EX3_END_BREAK = (int)(0.61f / GameLogicUpdateManager.m_fFrameLen);

	private readonly string FX_000_EX2_000 = "fxuse_ReShowShotB_001";

	private readonly string FX_000_EX2_001 = "fxuse_ReShowShotB_002";

	private readonly string FX_001_EX1_000 = "fxuse_ReSlashA_000";

	private readonly string FX_001_EX2_000 = "fxuse_ReSlashB_000";

	private readonly string FX_001_EX3_000 = "fxuse_ReSlashC_000";

	public override void Start()
	{
		base.Start();
		InitializeSkill();
		_refEntity.PlayTeleportInVoice = false;
	}

	private void InitializeSkill()
	{
		Transform[] target = _refEntity._transform.GetComponentsInChildren<Transform>(true);
		ManagedSingleton<CharacterControlHelper>.Instance.InitEnhanceSkill(_refEntity, 0, new int[4] { 23401, 23401, 23402, 23406 }, ref _enhanceSlot0);
		ManagedSingleton<CharacterControlHelper>.Instance.InitEnhanceSkill(_refEntity, 1, new int[4] { 23431, 23431, 23433, 23434 }, ref _enhanceSlot1);
		if (_enhanceSlot1 < 2)
		{
			ManagedSingleton<CharacterControlHelper>.Instance.PreloadLinkSkl<ShingetsurinBullet>(_refEntity, 1, out linkSkl1);
		}
		Transform transform = new GameObject("CustomShootPoint3").transform;
		transform.SetParent(base.transform);
		transform.localPosition = new Vector3(0f, 0.8f, 0f);
		Transform transform2 = new GameObject("fxuse_ccs_point").transform;
		transform2.SetParent(base.transform);
		transform2.localPosition = new Vector3(0f, 0.95f, 0.8f);
		Transform transform3 = OrangeBattleUtility.FindChildRecursive(ref target, "HandMesh_L_m");
		tfLHandMesh = transform3.GetComponent<SkinnedMeshRenderer>();
		Transform transform4 = OrangeBattleUtility.FindChildRecursive(ref target, "BusterMesh_L_m");
		tfLBusterMesh = transform4.GetComponent<SkinnedMeshRenderer>();
		Transform transform5 = OrangeBattleUtility.FindChildRecursive(ref target, "HandMesh_R_m");
		tfRHandMesh = transform5.GetComponent<SkinnedMeshRenderer>();
		Transform transform6 = OrangeBattleUtility.FindChildRecursive(ref target, "BusterMesh_R_m");
		tfRBusterMesh = transform6.GetComponent<SkinnedMeshRenderer>();
		Transform transform7 = OrangeBattleUtility.FindChildRecursive(ref target, "WingMesh_g", true);
		spMeshRenderer = transform7.GetComponent<SkinnedMeshRenderer>();
		GameObject gameObject = OrangeBattleUtility.FindChildRecursive(ref target, "Saber_008_002_G", true).gameObject;
		if ((bool)gameObject)
		{
			cmSaber = gameObject.GetComponent<CharacterMaterial>();
		}
		_refEntity.ExtraTransforms = new Transform[4];
		_refEntity.ExtraTransforms[0] = OrangeBattleUtility.FindChildRecursive(ref target, "L WeaponPoint", true);
		_refEntity.ExtraTransforms[1] = OrangeBattleUtility.FindChildRecursive(ref target, "R WeaponPoint", true);
		_refEntity.ExtraTransforms[2] = transform2;
		_refEntity.ExtraTransforms[3] = transform;
		switch (_enhanceSlot0)
		{
		default:
			_refEntity.PlayerSkills[0].ShootTransform[0] = _refEntity.ExtraTransforms[0];
			break;
		case 2:
			_refEntity.PlayerSkills[0].ShootTransform[0] = _refEntity.ExtraTransforms[2];
			break;
		}
		switch (_enhanceSlot1)
		{
		case 0:
		case 1:
			_refEntity.PlayerSkills[1].ShootTransform[0] = _refEntity.ExtraTransforms[3];
			break;
		case 2:
			_refEntity.PlayerSkills[1].ShootTransform[0] = _refEntity.ModelTransform;
			break;
		case 3:
			_refEntity.PlayerSkills[1].ShootTransform[0] = _refEntity.ExtraTransforms[0];
			break;
		}
		tfRBusterMesh.enabled = false;
		tfLBusterMesh.enabled = false;
		_wingEffect = OrangeBattleUtility.FindChildRecursive(ref target, "fxdemo_DiveRico_004").GetComponent<ParticleSystem>();
		_wingEffect.Play(true);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(FX_000_EX2_000, 2);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(FX_000_EX2_001, 2);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(FX_001_EX1_000, 2);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(FX_001_EX2_000, 2);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(FX_001_EX3_000, 2);
	}

	public override void OverrideDelegateEvent()
	{
		base.OverrideDelegateEvent();
		_refEntity.AnimationEndCharacterDependEvt = AnimationEndCharacterDepend;
		_refEntity.SetStatusCharacterDependEvt = SetStatusCharacterDepend;
		_refEntity.PlayerSkillLandCB = PlayerSkillLand;
		_refEntity.TeleportOutCharacterDependEvt = TeleportOutCharacterDepend;
		_refEntity.StageTeleportOutCharacterDependEvt = StageTeleportOutCharacterDepend;
		_refEntity.StageTeleportInCharacterDependEvt = StageTeleportInCharacterDepend;
		_refEntity.PlayTeleportOutEffectEvt = PlayTeleportOutEffect;
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
		{
			int enhanceSlot = _enhanceSlot0;
			if (enhanceSlot == 2 && _refEntity.CheckUseSkillKeyTrigger(id))
			{
				_refEntity.IsShoot = 1;
				ShootSkill0();
				ToggleLeftBuster(true, false);
				PlayVoiceSE("v_ri2_skill05");
			}
			break;
		}
		case 1:
			switch (_enhanceSlot1)
			{
			case 0:
			case 1:
				if (_refEntity.CheckUseSkillKeyTrigger(id))
				{
					_refEntity.CurrentActiveSkill = id;
					endBreakFrame = GameLogicUpdateManager.GameFrame + SKL1_EX1_END_BREAK;
					ManagedSingleton<CharacterControlHelper>.Instance.TurnToAimTarget(_refEntity);
					ManagedSingleton<CharacterControlHelper>.Instance.ChangeToSklStatus(_refEntity, id, SKL1_EX1_TRIGGER_1ST, SKL1_EX1_TRIGGER_1ST, OrangeCharacter.SubStatus.SKILL1, out skillEventFrame, out endFrame);
					ManagedSingleton<CharacterControlHelper>.Instance.SetAnimate(_refEntity, (HumanBase.AnimateId)77u, (HumanBase.AnimateId)78u, (HumanBase.AnimateId)79u);
					UsePassiveSkill(1);
					_refEntity.DisableWeaponMesh(_refEntity.GetCurrentWeaponObj(), 0f);
					ToggleSaber(true);
					fx_001_ex1_000 = null;
					fx_001_ex1_000 = MonoBehaviourSingleton<FxManager>.Instance.PlayReturn<FxBase>(FX_001_EX1_000, _refEntity.ModelTransform.position, (_refEntity._characterDirection == CharacterDirection.LEFT) ? OrangeCharacter.ReversedQuaternion : OrangeCharacter.NormalQuaternion, Array.Empty<object>());
					PlayVoiceSE("v_ri2_skill01");
				}
				break;
			case 2:
				if (_refEntity.CheckUseSkillKeyTrigger(id))
				{
					_refEntity.CurrentActiveSkill = id;
					_refEntity.SkillEnd = false;
					_refEntity.SetSpeed(0, 0);
					UsePassiveSkill(1);
					_refEntity.DisableWeaponMesh(_refEntity.GetCurrentWeaponObj(), 0f);
					ToggleSaber(true);
					_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1_2);
					PlayVoiceSE("v_ri2_skill03");
					PlaySkillSE("ri2_raikou");
				}
				break;
			case 3:
				if (_refEntity.CheckUseSkillKeyTrigger(id))
				{
					_refEntity.CurrentActiveSkill = id;
					endBreakFrame = GameLogicUpdateManager.GameFrame + SKL1_EX3_END_BREAK;
					ManagedSingleton<CharacterControlHelper>.Instance.TurnToAimTarget(_refEntity);
					ManagedSingleton<CharacterControlHelper>.Instance.ChangeToSklStatus(_refEntity, id, SKL1_EX3_TRIGGER, SKL1_EX3_END, OrangeCharacter.SubStatus.SKILL1_5, out skillEventFrame, out endFrame);
					ManagedSingleton<CharacterControlHelper>.Instance.SetAnimate(_refEntity, (HumanBase.AnimateId)84u, (HumanBase.AnimateId)85u, (HumanBase.AnimateId)86u);
					isSkillEventEnd = false;
					UsePassiveSkill(1);
					_refEntity.DisableWeaponMesh(_refEntity.GetCurrentWeaponObj(), 0f);
					MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FX_001_EX3_000, _refEntity.ModelTransform.position, Quaternion.identity, Array.Empty<object>());
					PlayVoiceSE("v_ri2_skill04");
					PlaySkillSE("ri2_rekkou");
				}
				break;
			}
			break;
		}
	}

	private void UsePassiveSkill(int _idx)
	{
		WeaponStruct weaponStruct = _refEntity.PlayerSkills[_idx];
		OrangeBattleUtility.UpdateSkillCD(weaponStruct);
		_refEntity.CheckUsePassiveSkill(_idx, weaponStruct.BulletData, weaponStruct.weaponStatus, weaponStruct.ShootTransform[0]);
	}

	public override void PlayerReleaseSkillCharacterCall(int id)
	{
		if (_refEntity.CurrentActiveSkill != -1)
		{
			return;
		}
		if (id != 0)
		{
			int num = 1;
			return;
		}
		switch (_enhanceSlot0)
		{
		case 0:
		case 1:
			if (_refEntity.CheckUseSkillKeyTrigger(id))
			{
				_refEntity.CurrentActiveSkill = id;
				_refEntity.PlayerShootBuster(_refEntity.PlayerSkills[id], true, id, _refEntity.PlayerSkills[id].ChargeLevel);
				_refEntity.CheckUsePassiveSkill(id, _refEntity.PlayerSkills[id].weaponStatus, _refEntity.PlayerSkills[id].ShootTransform[0]);
				_refEntity.Animator.SetAnimatorEquip(1);
				ToggleLeftBuster(true, false);
				PlayVoiceSE("v_ri2_skill02");
			}
			break;
		case 3:
			if (_refEntity.CheckUseSkillKeyTrigger(id))
			{
				_refEntity.CurrentActiveSkill = id;
				_refEntity.PlayerShootBuster(_refEntity.PlayerSkills[id], true, id, _refEntity.PlayerSkills[id].ChargeLevel, _refEntity.ShootDirection);
				_refEntity.CheckUsePassiveSkill(id, _refEntity.PlayerSkills[id].weaponStatus, _refEntity.PlayerSkills[id].ShootTransform[0], _refEntity.ShootDirection);
				_refEntity.Animator.SetAnimatorEquip(1);
				ToggleLeftBuster(true, false);
				PlayVoiceSE("v_ri2_skill02");
			}
			break;
		}
	}

	public override void CheckSkill()
	{
		if (_refEntity.CurrentActiveSkill == 0)
		{
			int enhanceSlot = _enhanceSlot0;
			if (((uint)enhanceSlot <= 1u || enhanceSlot == 3) && _refEntity.CheckSkillEndByShootTimer())
			{
				ToggleLeftBuster(false, true);
			}
		}
		if (_refEntity.CurMainStatus != OrangeCharacter.MainStatus.SKILL || _refEntity.IsAnimateIDChanged())
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
		case OrangeCharacter.SubStatus.SKILL0_1:
			if (_refEntity.CurrentFrame > 1f)
			{
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0_2);
			}
			else if (_refEntity.CurrentFrame > 0.3f && !isSkillEventEnd)
			{
				isSkillEventEnd = true;
				ManagedSingleton<CharacterControlHelper>.Instance.UpdateShootDirByAimDir(_refEntity);
				CreateSkillBullet(_refEntity.PlayerSkills[0]);
				_refEntity.CheckUsePassiveSkill(0, _refEntity.PlayerSkills[0].weaponStatus, _refEntity.ExtraTransforms[1]);
				if (_refEntity.IsLocalPlayer)
				{
					_refEntity.TriggerComboSkillBuff(_refEntity.PlayerSkills[0].FastBulletDatas[0].n_ID);
				}
			}
			break;
		case OrangeCharacter.SubStatus.SKILL0_2:
			if (CheckCancelAnimate(0))
			{
				OnSkillEnd();
			}
			break;
		case OrangeCharacter.SubStatus.SKILL0_3:
			if (_refEntity.CurrentFrame > 1f)
			{
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0_4);
			}
			else if (_refEntity.CurrentFrame > 0.3f && !isSkillEventEnd)
			{
				SKILL_TABLE sKILL_TABLE = _refEntity.PlayerSkills[0].FastBulletDatas[_refEntity.PlayerSkills[0].Reload_index];
				isSkillEventEnd = true;
				OrangeBattleUtility.UpdateSkillCD(_refEntity.GetCurrentSkillObj(), sKILL_TABLE.n_USE_COST, -1f);
				CreateSkillBullet(_refEntity.PlayerSkills[0]);
				_refEntity.CheckUsePassiveSkill(0, _refEntity.PlayerSkills[0].weaponStatus, _refEntity.ExtraTransforms[0]);
				_refEntity.RemoveComboSkillBuff(sKILL_TABLE.n_ID);
			}
			break;
		case OrangeCharacter.SubStatus.SKILL0_4:
			if (CheckCancelAnimate(0))
			{
				OnSkillEnd();
			}
			break;
		case OrangeCharacter.SubStatus.SKILL1:
			if (nowFrame >= endFrame)
			{
				shootDirection = (float)_refEntity._characterDirection * Vector3.right;
				isSkillEventEnd = false;
				int p_sklTriggerFrame = SKL1_EX1_TRIGGER_2ND - SKL1_EX1_TRIGGER_1ST;
				int p_endFrame = SKL1_EX1_END - SKL1_EX1_TRIGGER_1ST;
				WeaponStruct weaponStruct = _refEntity.PlayerSkills[1];
				ManagedSingleton<CharacterControlHelper>.Instance.PushBulletSkl(_refEntity, weaponStruct.ShootTransform[0], shootDirection, MagazineType.NORMAL, -1, 0, false);
				ManagedSingleton<CharacterControlHelper>.Instance.ChangeToNextStatus(_refEntity, p_sklTriggerFrame, p_endFrame, OrangeCharacter.SubStatus.SKILL1_1, out skillEventFrame, out endFrame);
				PlaySkillSE("ri2_genmu01");
			}
			else if ((bool)fx_001_ex1_000)
			{
				fx_001_ex1_000.transform.localRotation = ((_refEntity._characterDirection == CharacterDirection.LEFT) ? OrangeCharacter.ReversedQuaternion : OrangeCharacter.NormalQuaternion);
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
				WeaponStruct weaponStruct2 = _refEntity.PlayerSkills[1];
				if (linkSkl1 != null)
				{
					_refEntity.PushBulletDetail(linkSkl1, weaponStruct2.weaponStatus, weaponStruct2.ShootTransform[0], weaponStruct2.SkillLV, shootDirection);
				}
				PlaySkillSE("ri2_genmu02");
			}
			else if (isSkillEventEnd && nowFrame >= endBreakFrame)
			{
				ManagedSingleton<CharacterControlHelper>.Instance.CheckBreakFrame(_refEntity.UserID, ref endFrame);
			}
			break;
		case OrangeCharacter.SubStatus.SKILL1_4:
			if (_refEntity.CurrentFrame > 0.6f && isShowSkillWeapon)
			{
				isShowSkillWeapon = false;
				ToggleSaber(false);
			}
			break;
		case OrangeCharacter.SubStatus.SKILL1_5:
			if (nowFrame >= endFrame)
			{
				OnSkillEnd();
			}
			else if (!isSkillEventEnd && nowFrame >= skillEventFrame)
			{
				isSkillEventEnd = true;
				ManagedSingleton<CharacterControlHelper>.Instance.PushBulletSkl(_refEntity, _refEntity.ModelTransform, MagazineType.NORMAL, -1, 0, false);
			}
			else if (isSkillEventEnd && nowFrame >= endBreakFrame)
			{
				ManagedSingleton<CharacterControlHelper>.Instance.CheckBreakFrame(_refEntity.UserID, ref endFrame);
			}
			break;
		}
	}

	private void ShootSkill0()
	{
		WeaponStruct weaponStruct = _refEntity.PlayerSkills[0];
		_refEntity.CurrentActiveSkill = 0;
		if (!HasComboSkill(0))
		{
			_refEntity.SkillEnd = false;
			isSkillEventEnd = false;
			_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0_1);
			MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FX_000_EX2_000, _refEntity.ExtraTransforms[2].position, Quaternion.identity, Array.Empty<object>());
		}
		else if (weaponStruct.Reload_index != 0)
		{
			_refEntity.SkillEnd = false;
			isSkillEventEnd = false;
			_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0_3);
			MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FX_000_EX2_001, _refEntity.ExtraTransforms[2].position, Quaternion.identity, Array.Empty<object>());
		}
	}

	private bool HasComboSkill(int SkillIdx)
	{
		if (_refEntity.PlayerSkills[SkillIdx].ComboCheckDatas.Length != 0 && _refEntity.PlayerSkills[SkillIdx].ComboCheckDatas[0].CheckHasAllBuff(_refEntity.selfBuffManager))
		{
			return true;
		}
		return false;
	}

	public void PlayerSkillLand()
	{
		_refEntity.SetHorizontalSpeed(0);
		switch (_refEntity.CurSubStatus)
		{
		case OrangeCharacter.SubStatus.SKILL0_1:
			_refEntity.SetAnimateId((HumanBase.AnimateId)66u);
			break;
		case OrangeCharacter.SubStatus.SKILL0_2:
			_refEntity.SetAnimateId((HumanBase.AnimateId)69u);
			break;
		case OrangeCharacter.SubStatus.SKILL0_3:
			_refEntity.SetAnimateId((HumanBase.AnimateId)72u);
			break;
		case OrangeCharacter.SubStatus.SKILL0_4:
			_refEntity.SetAnimateId((HumanBase.AnimateId)75u);
			break;
		}
	}

	public override void CreateSkillBullet(WeaponStruct wsSkill)
	{
		_refEntity.FreshBullet = true;
		_refEntity.StartShootTimer();
		OrangeCharacter.MainStatus curMainStatus = _refEntity.CurMainStatus;
		if (curMainStatus == OrangeCharacter.MainStatus.SKILL)
		{
			switch (_refEntity.CurSubStatus)
			{
			case OrangeCharacter.SubStatus.SKILL0_1:
				_refEntity.PushBulletDetail(wsSkill.FastBulletDatas[0], wsSkill.weaponStatus, _refEntity.ExtraTransforms[1], wsSkill.SkillLV, (float)_refEntity._characterDirection * Vector3.right);
				break;
			case OrangeCharacter.SubStatus.SKILL0_3:
				_refEntity.PushBulletDetail(wsSkill.FastBulletDatas[wsSkill.Reload_index], wsSkill.weaponStatus, _refEntity.ExtraTransforms[0], wsSkill.SkillLV, (float)_refEntity._characterDirection * Vector3.right);
				break;
			}
		}
	}

	private bool CheckCancelAnimate(int skillId)
	{
		if (skillId == 0 && ManagedSingleton<InputStorage>.Instance.IsAnyPress(_refEntity.UserID))
		{
			return true;
		}
		return false;
	}

	public void SetStatusCharacterDepend(OrangeCharacter.MainStatus mainStatus, OrangeCharacter.SubStatus subStatus)
	{
		if (mainStatus != OrangeCharacter.MainStatus.SKILL)
		{
			return;
		}
		switch (subStatus)
		{
		case OrangeCharacter.SubStatus.SKILL0_1:
			_refEntity.DisableWeaponMesh(_refEntity.GetCurrentWeaponObj(), 0.01f);
			ToggleLeftBuster(true, false);
			ToggleRightBuster(true);
			if (_refEntity.Controller.Collisions.below)
			{
				_refEntity.SetHorizontalSpeed(0);
			}
			ManagedSingleton<CharacterControlHelper>.Instance.SetAnimate(_refEntity, HumanBase.AnimateId.ANI_SKILL_START, (HumanBase.AnimateId)66u, (HumanBase.AnimateId)67u, false);
			break;
		case OrangeCharacter.SubStatus.SKILL0_2:
			_refEntity.SkillEnd = true;
			ToggleLeftBuster(false, true);
			ToggleRightBuster(false);
			if (_refEntity.Controller.Collisions.below)
			{
				_refEntity.SetHorizontalSpeed(0);
			}
			ManagedSingleton<CharacterControlHelper>.Instance.SetAnimate(_refEntity, (HumanBase.AnimateId)68u, (HumanBase.AnimateId)69u, (HumanBase.AnimateId)70u, false);
			break;
		case OrangeCharacter.SubStatus.SKILL0_3:
			_refEntity.DisableWeaponMesh(_refEntity.GetCurrentWeaponObj(), 0.01f);
			ToggleLeftBuster(true, false);
			if (_refEntity.Controller.Collisions.below)
			{
				_refEntity.SetHorizontalSpeed(0);
			}
			ManagedSingleton<CharacterControlHelper>.Instance.SetAnimate(_refEntity, (HumanBase.AnimateId)71u, (HumanBase.AnimateId)72u, (HumanBase.AnimateId)73u, false);
			break;
		case OrangeCharacter.SubStatus.SKILL0_4:
			if (_refEntity.Controller.Collisions.below)
			{
				_refEntity.SetHorizontalSpeed(0);
			}
			ManagedSingleton<CharacterControlHelper>.Instance.SetAnimate(_refEntity, (HumanBase.AnimateId)74u, (HumanBase.AnimateId)75u, (HumanBase.AnimateId)76u, false);
			break;
		case OrangeCharacter.SubStatus.SKILL1_2:
			_refEntity.IgnoreGravity = true;
			_refEntity.SetAnimateId((HumanBase.AnimateId)80u);
			break;
		case OrangeCharacter.SubStatus.SKILL1_3:
		{
			WeaponStruct weaponStruct = _refEntity.PlayerSkills[1];
			_refEntity.SetSpeed((int)_refEntity._characterDirection * (int)((float)OrangeCharacter.DashSpeed * dashExSpd), 0);
			_refEntity.BulletCollider.UpdateBulletData(weaponStruct.BulletData, _refEntity.sPlayerName, _refEntity.GetNowRecordNO(), _refEntity.nBulletRecordID++);
			_refEntity.BulletCollider.SetBulletAtk(weaponStruct.weaponStatus, _refEntity.selfBuffManager.sBuffStatus);
			_refEntity.BulletCollider.BulletLevel = weaponStruct.SkillLV;
			_refEntity.BulletCollider.Active(_refEntity.TargetMask);
			_refEntity.SetAnimateId((HumanBase.AnimateId)81u);
			tSkill1Fx = MonoBehaviourSingleton<FxManager>.Instance.PlayReturn<FxBase>(FX_001_EX2_000, _refEntity.ModelTransform, Quaternion.identity, Array.Empty<object>());
			ParticleSystem[] componentsInChildren = tSkill1Fx.transform.GetComponentsInChildren<ParticleSystem>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				ParticleSystem.MainModule main = componentsInChildren[i].main;
				main.simulationSpeed = 4f;
			}
			break;
		}
		case OrangeCharacter.SubStatus.SKILL1_4:
			_refEntity.SetAnimateId((HumanBase.AnimateId)(82u + ((!_refEntity.PreBelow) ? 1u : 0u)));
			break;
		}
	}

	public void AnimationEndCharacterDepend(OrangeCharacter.MainStatus mainStatus, OrangeCharacter.SubStatus subStatus)
	{
		if (mainStatus != OrangeCharacter.MainStatus.SKILL)
		{
			return;
		}
		switch (subStatus)
		{
		case OrangeCharacter.SubStatus.SKILL0_2:
			ToggleLeftBuster(false, true);
			ToggleRightBuster(false);
			if (_refEntity.Controller.Collisions.below)
			{
				_refEntity.Dashing = false;
				_refEntity.SetStatus(OrangeCharacter.MainStatus.IDLE, OrangeCharacter.SubStatus.IDLE);
			}
			else
			{
				_refEntity.SetStatus(OrangeCharacter.MainStatus.FALL, OrangeCharacter.SubStatus.TELEPORT_POSE);
			}
			break;
		case OrangeCharacter.SubStatus.SKILL0_4:
			_refEntity.SkillEnd = true;
			ToggleLeftBuster(false, true);
			ToggleRightBuster(false);
			if (_refEntity.Controller.Collisions.below)
			{
				_refEntity.Dashing = false;
				_refEntity.SetStatus(OrangeCharacter.MainStatus.IDLE, OrangeCharacter.SubStatus.IDLE);
			}
			else
			{
				_refEntity.SetStatus(OrangeCharacter.MainStatus.FALL, OrangeCharacter.SubStatus.TELEPORT_POSE);
			}
			break;
		case OrangeCharacter.SubStatus.SKILL1_2:
			_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1_3);
			_refEntity.CharacterMaterials.Disappear();
			break;
		case OrangeCharacter.SubStatus.SKILL1_3:
			if ((bool)_refEntity.Controller.BelowInBypassRange)
			{
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1_4);
			}
			else
			{
				_refEntity.SetStatus(OrangeCharacter.MainStatus.FALL, OrangeCharacter.SubStatus.TELEPORT_POSE);
				OnSkillEnd();
			}
			_refEntity.IgnoreGravity = false;
			_refEntity.SetSpeed(0, 0);
			_refEntity.CharacterMaterials.Appear();
			_refEntity.BulletCollider.BackToPool();
			break;
		case OrangeCharacter.SubStatus.SKILL1_4:
			_refEntity.SkillEnd = true;
			_refEntity.Dashing = false;
			_refEntity.SetStatus(OrangeCharacter.MainStatus.IDLE, OrangeCharacter.SubStatus.IDLE);
			break;
		}
	}

	private void OnSkillEnd()
	{
		if (_refEntity.IgnoreGravity)
		{
			_refEntity.IgnoreGravity = false;
		}
		fx_001_ex1_000 = null;
		ToggleSaber(false);
		isSkillEventEnd = false;
		_refEntity.SkillEnd = true;
		_refEntity.CurrentActiveSkill = -1;
		_refEntity.EnableCurrentWeapon();
		switch (_refEntity.AnimateID)
		{
		default:
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
			break;
		case (HumanBase.AnimateId)77u:
		case (HumanBase.AnimateId)84u:
			_refEntity.Dashing = false;
			if (ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.DOWN))
			{
				_refEntity.SetStatus(OrangeCharacter.MainStatus.CROUCH, OrangeCharacter.SubStatus.WIN_POSE);
			}
			else
			{
				_refEntity.SetStatus(OrangeCharacter.MainStatus.IDLE, OrangeCharacter.SubStatus.CROUCH_UP);
			}
			break;
		case HumanBase.AnimateId.ANI_SKILL_START:
		case (HumanBase.AnimateId)68u:
		case (HumanBase.AnimateId)71u:
		case (HumanBase.AnimateId)74u:
			_refEntity.Dashing = false;
			_refEntity.SetHorizontalSpeed(0);
			if (ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.DOWN))
			{
				_refEntity.SetStatus(OrangeCharacter.MainStatus.CROUCH, OrangeCharacter.SubStatus.WIN_POSE);
			}
			else
			{
				_refEntity.SetStatus(OrangeCharacter.MainStatus.IDLE, OrangeCharacter.SubStatus.CROUCH_UP);
			}
			break;
		case (HumanBase.AnimateId)66u:
		case (HumanBase.AnimateId)67u:
		case (HumanBase.AnimateId)69u:
		case (HumanBase.AnimateId)70u:
		case (HumanBase.AnimateId)72u:
		case (HumanBase.AnimateId)73u:
		case (HumanBase.AnimateId)75u:
		case (HumanBase.AnimateId)76u:
			if (_refEntity.Controller.Collisions.below)
			{
				_refEntity.Dashing = false;
				_refEntity.SetHorizontalSpeed(0);
				_refEntity.SetStatus(OrangeCharacter.MainStatus.IDLE, OrangeCharacter.SubStatus.IDLE);
			}
			else
			{
				_refEntity.SetStatus(OrangeCharacter.MainStatus.FALL, OrangeCharacter.SubStatus.TELEPORT_POSE);
			}
			break;
		}
	}

	public override void ClearSkill()
	{
		if (_refEntity.CurrentActiveSkill == 0)
		{
			ToggleLeftBuster(false, true);
			ToggleRightBuster(false);
		}
		else
		{
			switch (_enhanceSlot1)
			{
			default:
				ToggleSaber(false);
				break;
			case 2:
				_refEntity.CharacterMaterials.Appear();
				_refEntity.BulletCollider.BackToPool();
				_refEntity.Dashing = false;
				ToggleSaber(false);
				break;
			case 3:
				break;
			}
		}
		isSkillEventEnd = false;
		_refEntity.IgnoreGravity = false;
		_refEntity.SkillEnd = true;
		_refEntity.CurrentActiveSkill = -1;
		_refEntity.EnableCurrentWeapon();
	}

	public override void SetStun(bool enable)
	{
		base.SetStun(enable);
		ToggleSaber(false);
		ToggleLeftBuster(false, true);
		ToggleRightBuster(false);
		_refEntity.EnableCurrentWeapon();
	}

	private void ToggleLeftBuster(bool enable, bool haveNormalWeapon)
	{
		if (enable)
		{
			if ((bool)tfLBusterMesh)
			{
				tfLBusterMesh.enabled = true;
			}
			if ((bool)tfLHandMesh)
			{
				tfLHandMesh.enabled = false;
			}
			return;
		}
		switch ((WeaponType)(short)_refEntity.GetCurrentWeaponObj().WeaponData.n_TYPE)
		{
		case WeaponType.Melee:
		case WeaponType.DualGun:
		case WeaponType.MGun:
		case WeaponType.Gatling:
		case WeaponType.Launcher:
			if ((bool)tfLHandMesh)
			{
				tfLHandMesh.enabled = true;
			}
			break;
		default:
			if ((bool)tfLHandMesh)
			{
				if (haveNormalWeapon)
				{
					tfLHandMesh.enabled = false;
				}
				else
				{
					tfLHandMesh.enabled = true;
				}
			}
			break;
		}
		if ((bool)tfLBusterMesh)
		{
			tfLBusterMesh.enabled = false;
		}
	}

	private void ToggleRightBuster(bool enable)
	{
		if ((bool)tfRBusterMesh)
		{
			tfRBusterMesh.enabled = enable;
		}
		if ((bool)tfRHandMesh)
		{
			tfRHandMesh.enabled = !enable;
		}
	}

	private void ToggleSaber(bool enable)
	{
		if ((bool)cmSaber)
		{
			if (enable)
			{
				isShowSkillWeapon = true;
				cmSaber.Appear();
			}
			else
			{
				cmSaber.Disappear();
			}
		}
	}

	public override void ControlCharacterDead()
	{
		ToggleWing(false);
		ToggleSaber(false);
	}

	public override void ControlCharacterContinue()
	{
		StartCoroutine(OnToggleWing(true, 0.6f));
	}

	private void TeleportOutCharacterDepend()
	{
		if (_refEntity.CurSubStatus == OrangeCharacter.SubStatus.TELEPORT_POSE)
		{
			float currentFrame = _refEntity.CurrentFrame;
			if (currentFrame > 1.5f && currentFrame <= 2f)
			{
				ToggleWing(false);
			}
		}
	}

	protected void StageTeleportInCharacterDepend()
	{
		if (spMeshRenderer != null && spMeshRenderer.enabled)
		{
			StopAllCoroutines();
			return;
		}
		ToggleWing(false);
		StopAllCoroutines();
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

	private void PlayTeleportOutEffect()
	{
		Vector3 p_worldPos = base.transform.position;
		if (_refEntity != null)
		{
			p_worldPos = _refEntity.AimPosition;
		}
		MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("FX_TELEPORT_OUT", p_worldPos, Quaternion.identity, Array.Empty<object>());
	}

	public override string[] GetCharacterDependAnimations()
	{
		return new string[22]
		{
			"ch136_skill_01_ex2_step1_crouch_start", "ch136_skill_01_ex2_step1_stand_start", "ch136_skill_01_ex2_step1_jump_start", "ch136_skill_01_ex2_step1_crouch_end", "ch136_skill_01_ex2_step1_stand_end", "ch136_skill_01_ex2_step1_jump_end", "ch136_skill_01_ex2_step2_crouch_start", "ch136_skill_01_ex2_step2_stand_start", "ch136_skill_01_ex2_step2_jump_start", "ch136_skill_01_ex2_step2_crouch_end",
			"ch136_skill_01_ex2_step2_stand_end", "ch136_skill_01_ex2_step2_jump_end", "ch136_skill_02_ex1_crouch", "ch136_skill_02_ex1_stand", "ch136_skill_02_ex1_jump", "ch136_skill_02_ex2_jump_start", "ch136_skill_02_ex2_jump_loop", "ch136_skill_02_ex2_stand_end", "ch136_skill_02_ex2_jump_end", "ch136_skill_02_ex3_crouch",
			"ch136_skill_02_ex3_stand", "ch136_skill_02_ex3_jump"
		};
	}

	public override int GetUniqueWeaponType()
	{
		return 1;
	}

	public override void GetUniqueMotion(out string[] source, out string[] target)
	{
		source = new string[3] { "buster_stand_charge_atk", "buster_fall_charge_atk", "buster_crouch_charge_atk" };
		target = new string[3] { "ch136_skill_01_ex1_stand_mid", "ch136_skill_01_ex1_jump_mid", "ch136_skill_01_ex1_crouch_mid" };
	}
}
