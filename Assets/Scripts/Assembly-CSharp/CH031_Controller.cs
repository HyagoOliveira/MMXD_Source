using System;
using UnityEngine;

public class CH031_Controller : CharacterControlBase, IPetSummoner
{
	private int nowFrame;

	private int skillEventFrame;

	private int endFrame;

	private bool isSkillEventEnd;

	private CH031_SKL00 pet;

	private SKILL_TABLE linkSkl;

	private int conditionId = -1;

	private bool bCallPet;

	private int petFrame;

	private int ipid;

	private int isid;

	private readonly int SKL0_TRIGGER = (int)(0.2f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL0_END = (int)(0.667f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL1_TRIGGER = (int)(0.433f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL1_END = (int)(1f / GameLogicUpdateManager.m_fFrameLen);

	public int PetID { get; set; } = -1;


	public long PetTime { get; set; }

	public int PetCount { get; set; }

	public override void Start()
	{
		base.Start();
		InitThisSkill();
	}

	public override void OverrideDelegateEvent()
	{
		base.OverrideDelegateEvent();
		_refEntity.SetStatusCharacterDependEvt = SetStatusCharacterDepend;
		_refEntity.TeleportInExtraEffectEvt = TeleportInExtraEffect;
		_refEntity.CheckPetActiveEvt = CheckPetActive;
	}

	private void InitThisSkill()
	{
		linkSkl = null;
		for (int i = 0; i < _refEntity.PlayerSkills.Length; i++)
		{
			WeaponStruct weaponStruct = _refEntity.PlayerSkills[i];
			if (weaponStruct == null)
			{
				continue;
			}
			if (conditionId == -1 && weaponStruct.BulletData.n_TARGET == 3 && weaponStruct.BulletData.n_CONDITION_ID > 0)
			{
				CONDITION_TABLE value = null;
				if (ManagedSingleton<OrangeDataManager>.Instance.CONDITION_TABLE_DICT.TryGetValue(weaponStruct.BulletData.n_CONDITION_ID, out value))
				{
					conditionId = value.n_EFFECT;
				}
			}
			if (weaponStruct.BulletData.n_LINK_SKILL != 0 && linkSkl == null && ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT.TryGetNewValue(weaponStruct.BulletData.n_LINK_SKILL, out linkSkl))
			{
				_refEntity.tRefPassiveskill.ReCalcuSkill(ref linkSkl);
				GameObject obj = new GameObject();
				CollideBullet go = obj.AddComponent<CollideBullet>();
				obj.name = linkSkl.s_MODEL;
				obj.layer = ManagedSingleton<OrangeLayerManager>.Instance.BulletLayer;
				MonoBehaviourSingleton<PoolManager>.Instance.CreatePoolBaseLocal<CollideBullet>(go, linkSkl.s_MODEL);
			}
		}
		ManagedSingleton<CharacterControlHelper>.Instance.PetInit<CH031_SKL00>(this, _refEntity);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_ciel_001", 2);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_elf_000", 2);
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
				return;
			}
			skillEventFrame = GameLogicUpdateManager.GameFrame + SKL0_TRIGGER;
			endFrame = GameLogicUpdateManager.GameFrame + SKL0_END;
			_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0);
			MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_elf_000", _refEntity.ModelTransform.position, (_refEntity._characterDirection == CharacterDirection.LEFT) ? OrangeCharacter.ReversedQuaternion : OrangeCharacter.NormalQuaternion, Array.Empty<object>());
			_refEntity.PlaySE(_refEntity.VoiceID, "v_cl_skill01");
			break;
		case 1:
			if ((!_refEntity.Controller.Collisions.below && !_refEntity.Controller.Collisions.JSB_below) || !_refEntity.CheckUseSkillKeyTrigger(id))
			{
				return;
			}
			skillEventFrame = GameLogicUpdateManager.GameFrame + SKL1_TRIGGER;
			endFrame = GameLogicUpdateManager.GameFrame + SKL1_END;
			MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_ciel_001", _refEntity.ModelTransform.position, Quaternion.identity, Array.Empty<object>());
			_refEntity.PlaySE(_refEntity.VoiceID, "v_cl_skill02");
			_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1);
			break;
		}
		_refEntity.SkillEnd = false;
		_refEntity.DisableCurrentWeapon();
		_refEntity.CurrentActiveSkill = id;
	}

	public override void PlayerReleaseSkillCharacterCall(int id)
	{
	}

	public override void CheckSkill()
	{
		nowFrame = GameLogicUpdateManager.GameFrame;
		if (bCallPet && petFrame < nowFrame)
		{
			CreatePet();
			bCallPet = false;
		}
		UpdatePetStatus();
		if (_refEntity.IsAnimateIDChanged() || _refEntity.CurrentActiveSkill == -1 || _refEntity.CurMainStatus != OrangeCharacter.MainStatus.SKILL)
		{
			return;
		}
		switch (_refEntity.CurrentActiveSkill)
		{
		case 0:
			if (nowFrame >= endFrame)
			{
				_refEntity.IgnoreGravity = false;
				isSkillEventEnd = false;
				_refEntity.SkillEnd = true;
				_refEntity.CurrentActiveSkill = -1;
				_refEntity.EnableCurrentWeapon();
				ResetToIdle();
			}
			else if (!isSkillEventEnd && nowFrame >= skillEventFrame)
			{
				if (_refEntity.IsLocalPlayer)
				{
					CallPet(PetID, false, -1, null);
				}
				_refEntity.CheckUsePassiveSkill(_refEntity.CurrentActiveSkill, _refEntity.GetCurrentSkillObj().weaponStatus, _refEntity.GetCurrentSkillObj().ShootTransform[_refEntity.CurrentActiveSkill]);
				OrangeBattleUtility.UpdateSkillCD(_refEntity.GetCurrentSkillObj());
				isSkillEventEnd = true;
			}
			break;
		case 1:
			if (nowFrame >= endFrame)
			{
				isSkillEventEnd = false;
				_refEntity.SkillEnd = true;
				_refEntity.CurrentActiveSkill = -1;
				_refEntity.EnableCurrentWeapon();
				ResetToIdle();
			}
			else if (!isSkillEventEnd && nowFrame >= skillEventFrame)
			{
				Vector3 vec = _refEntity.Controller.LogicPosition.vec3;
				_refEntity.PushBulletDetail(_refEntity.GetCurrentSkillObj().BulletData, _refEntity.GetCurrentSkillObj().weaponStatus, vec, _refEntity.GetCurrentSkillObj().SkillLV, Vector3.zero, false, 1);
				if (linkSkl != null)
				{
					_refEntity.PushBulletDetail(linkSkl, _refEntity.GetCurrentSkillObj().weaponStatus, vec, _refEntity.GetCurrentSkillObj().SkillLV, Vector3.zero, false, 1);
				}
				_refEntity.CheckUsePassiveSkill(_refEntity.CurrentActiveSkill, _refEntity.GetCurrentSkillObj().weaponStatus, _refEntity.GetCurrentSkillObj().ShootTransform[_refEntity.CurrentActiveSkill]);
				OrangeBattleUtility.UpdateSkillCD(_refEntity.GetCurrentSkillObj());
				isSkillEventEnd = true;
			}
			break;
		}
	}

	public void SetStatusCharacterDepend(OrangeCharacter.MainStatus mainStatus, OrangeCharacter.SubStatus subStatus)
	{
		if (mainStatus != OrangeCharacter.MainStatus.SKILL)
		{
			return;
		}
		switch (subStatus)
		{
		case OrangeCharacter.SubStatus.SKILL0:
			_refEntity.SetHorizontalSpeed(0);
			_refEntity.PlayerStopDashing();
			if (_refEntity.Controller.Collisions.below || _refEntity.Controller.Collisions.JSB_below)
			{
				_refEntity.SetAnimateId(HumanBase.AnimateId.ANI_SKILL_START);
				break;
			}
			_refEntity.IgnoreGravity = true;
			_refEntity.SetAnimateId((HumanBase.AnimateId)66u);
			break;
		case OrangeCharacter.SubStatus.SKILL1:
			_refEntity.SetHorizontalSpeed(0);
			_refEntity.PlayerStopDashing();
			_refEntity.SetAnimateId((HumanBase.AnimateId)67u);
			break;
		}
	}

	public override void ClearSkill()
	{
		if (_refEntity.CurrentActiveSkill != -1)
		{
			_refEntity.EnableCurrentWeapon();
			OrangeBattleUtility.UpdateSkillCD(_refEntity.GetCurrentSkillObj());
		}
		if (_refEntity.IsJacking && pet != null)
		{
			pet.SetCurrentBullet(0);
		}
		_refEntity.IgnoreGravity = false;
		isSkillEventEnd = false;
		_refEntity.SkillEnd = true;
		_refEntity.CurrentActiveSkill = -1;
	}

	public override void CallPet(int petID, bool isHurt, int nSetNumID = -1, Vector3? vSetPos = null)
	{
		RemovePet();
		petFrame = nowFrame + 3;
		bCallPet = true;
		ipid = petID;
		isid = nSetNumID;
	}

	private void CreatePet()
	{
		pet = ManagedSingleton<CharacterControlHelper>.Instance.CallPet<CH031_SKL00>(this, _refEntity, ipid, isid, true, true, false);
		if ((bool)pet)
		{
			pet.transform.SetParentNull();
			pet.transform.localEulerAngles = new Vector3(0f, 90f, 0f);
			pet.SetFollowOffset(new Vector3(-0.6f, 1f, 0f));
			pet.activeSE = new string[2] { "SkillSE_CIEL", "cl_elf01_lp" };
			pet.unactiveSE = new string[2] { "SkillSE_CIEL", "cl_elf01_stop" };
			pet.SetActive(true);
		}
	}

	private new void RemovePet()
	{
		if (pet != null && pet.Activate)
		{
			pet.SetActive(false);
			pet = null;
		}
	}

	private void ResetToIdle()
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

	public void TeleportInExtraEffect()
	{
		_refEntity.PlaySE(_refEntity.SkillSEID, "cl_system01");
		MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(GetTeleportInExtraEffect(), _refEntity.ModelTransform.position, Quaternion.identity, Array.Empty<object>());
	}

	public override void ControlCharacterDead()
	{
		RemovePet();
	}

	private void UpdatePetStatus()
	{
		if (pet != null && !_refEntity.bLockInputCtrl && pet.Activate)
		{
			if (conditionId > 0 && _refEntity.selfBuffManager.CheckHasEffect(conditionId))
			{
				pet.SetCurrentBullet(1);
			}
			else
			{
				pet.SetCurrentBullet(0);
			}
		}
	}

	public bool CheckPetActive(int petId)
	{
		if (pet != null && pet.Activate && pet.PetID == petId)
		{
			return true;
		}
		return false;
	}

	public override string GetTeleportInExtraEffect()
	{
		return "fxuse_ciel_in";
	}

	public override string[] GetCharacterDependAnimations()
	{
		return new string[4] { "ch031_skill_01_stand_spawn", "ch031_skill_01_jump_spawn", "ch031_skill_02_stand", "ch031_skill_02_jump" };
	}
}
