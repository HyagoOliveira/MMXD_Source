using System;
using StageLib;
using UnityEngine;

public class CH057_Controller : CharacterControlBase
{
	private int nowFrame;

	private int skillEventFrame;

	private int endFrame;

	private bool isSkillEventEnd;

	public CH031_SKL00 pet;

	private bool petActive;

	private int petActiveFrame = (int)(1f / GameLogicUpdateManager.m_fFrameLen);

	private int petActiveFrameEnd;

	private SKILL_TABLE petSklTable;

	private PET_TABLE petTable;

	private SKILL_TABLE linkSkl;

	private GameObject bowlMesh_c;

	private Transform shootPointTransform;

	private readonly string sCustomShootPoint = "CustomShootPoint";

	private readonly string sBowlMesh = "BowlMesh_c_L";

	private readonly string sFxuse000 = "fxuse_cielchoco_000";

	private readonly string sFxuse001 = "fxuse_cielchoco_001";

	private readonly string sFxuseCutIn = "fxuse_cielchoco_in";

	private readonly int SKL0_TRIGGER = (int)(0.1f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL0_END = (int)(0.333f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL1_TRIGGER = (int)(0.433f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL1_END = (int)(1f / GameLogicUpdateManager.m_fFrameLen);

	public override void Start()
	{
		base.Start();
		Transform[] target = _refEntity._transform.GetComponentsInChildren<Transform>(true);
		bowlMesh_c = OrangeBattleUtility.FindChildRecursive(ref target, sBowlMesh, true).gameObject;
		bowlMesh_c.SetActive(false);
		InitializeSkill();
	}

	public override void OverrideDelegateEvent()
	{
		base.OverrideDelegateEvent();
		_refEntity.SetStatusCharacterDependEvt = SetStatusCharacterDepend;
		_refEntity.TeleportInExtraEffectEvt = TeleportInExtraEffect;
		_refEntity.CheckPetActiveEvt = CheckPetActive;
	}

	private void InitializeSkill()
	{
		shootPointTransform = new GameObject(sCustomShootPoint).transform;
		shootPointTransform.SetParent(base.transform);
		shootPointTransform.localPosition = new Vector3(0f, 1.1f, 0.5f);
		linkSkl = null;
		petSklTable = null;
		petTable = null;
		for (int i = 0; i < _refEntity.PlayerSkills.Length; i++)
		{
			WeaponStruct weaponStruct = _refEntity.PlayerSkills[i];
			if (weaponStruct != null && weaponStruct.BulletData.n_LINK_SKILL != 0 && linkSkl == null && ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT.TryGetNewValue(weaponStruct.BulletData.n_LINK_SKILL, out linkSkl))
			{
				_refEntity.tRefPassiveskill.ReCalcuSkill(ref linkSkl);
				GameObject obj2 = new GameObject();
				CollideBullet go = obj2.AddComponent<CollideBullet>();
				obj2.name = linkSkl.s_MODEL;
				obj2.layer = ManagedSingleton<OrangeLayerManager>.Instance.BulletLayer;
				MonoBehaviourSingleton<PoolManager>.Instance.CreatePoolBaseLocal<CollideBullet>(go, linkSkl.s_MODEL);
				break;
			}
		}
		if (_refEntity.tRefPassiveskill.listHurtPassiveskill.Count > 0)
		{
			for (int j = 0; j < _refEntity.tRefPassiveskill.listHurtPassiveskill.Count; j++)
			{
				SKILL_TABLE tSKILL_TABLE = _refEntity.tRefPassiveskill.listHurtPassiveskill[j].tSKILL_TABLE;
				if (tSKILL_TABLE.n_EFFECT != 16)
				{
					continue;
				}
				int key = (int)tSKILL_TABLE.f_EFFECT_X;
				PetBuilder petBuilder = new GameObject().AddComponent<PetBuilder>();
				petTable = null;
				petSklTable = tSKILL_TABLE;
				if (ManagedSingleton<OrangeDataManager>.Instance.PET_TABLE_DICT.TryGetValue(key, out petTable))
				{
					petBuilder.PetID = (int)tSKILL_TABLE.f_EFFECT_X;
					petBuilder.follow_skill_id = 1;
					petBuilder.CreatePet(delegate(CH031_SKL00 obj)
					{
						MonoBehaviourSingleton<PoolManager>.Instance.CreatePoolBaseLocal<CH031_SKL00>(obj, petTable.s_MODEL, 3);
					});
					petActiveFrame = (int)(tSKILL_TABLE.f_EFFECT_Z / GameLogicUpdateManager.m_fFrameLen);
					break;
				}
			}
		}
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(sFxuse000, 2);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(sFxuse001, 2);
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
			bowlMesh_c.SetActive(true);
			if (_refEntity.AnimateID == HumanBase.AnimateId.ANI_CROUCH || _refEntity.AnimateID == HumanBase.AnimateId.ANI_CROUCH_END)
			{
				_refEntity.SetAnimateId((HumanBase.AnimateId)129u);
				break;
			}
			if (_refEntity.Controller.Collisions.below || _refEntity.Controller.Collisions.JSB_below)
			{
				_refEntity.SetAnimateId(HumanBase.AnimateId.ANI_BTSKILL_START);
				break;
			}
			_refEntity.IgnoreGravity = true;
			_refEntity.SetAnimateId((HumanBase.AnimateId)128u);
			break;
		case OrangeCharacter.SubStatus.SKILL1:
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
		}
	}

	public override void PlayerPressSkillCharacterCall(int id)
	{
		if (_refEntity.CurrentActiveSkill == -1 && id == 1 && _refEntity.CheckUseSkillKeyTrigger(id))
		{
			skillEventFrame = GameLogicUpdateManager.GameFrame + SKL1_TRIGGER;
			endFrame = GameLogicUpdateManager.GameFrame + SKL1_END;
			MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(sFxuse001, _refEntity.ModelTransform.position, Quaternion.identity, Array.Empty<object>());
			PlayVoiceSE("v_cl2_skill01");
			_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1);
			_refEntity.SkillEnd = false;
			_refEntity.DisableCurrentWeapon();
			_refEntity.CurrentActiveSkill = id;
		}
	}

	public override void PlayerReleaseSkillCharacterCall(int id)
	{
		if (_refEntity.CurrentActiveSkill == -1 && id == 0 && _refEntity.CheckUseSkillKeyTrigger(id))
		{
			skillEventFrame = GameLogicUpdateManager.GameFrame + SKL0_TRIGGER;
			endFrame = GameLogicUpdateManager.GameFrame + SKL0_END;
			_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0);
			MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(sFxuse000, _refEntity.ModelTransform.position, (_refEntity._characterDirection == CharacterDirection.LEFT) ? OrangeCharacter.ReversedQuaternion : OrangeCharacter.NormalQuaternion, Array.Empty<object>());
			PlayVoiceSE("v_cl2_skill02");
			_refEntity.SkillEnd = false;
			_refEntity.DisableCurrentWeapon();
			_refEntity.CurrentActiveSkill = id;
		}
	}

	public override void CheckSkill()
	{
		nowFrame = GameLogicUpdateManager.GameFrame;
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
				CreateSkillBullet(_refEntity.PlayerSkills[_refEntity.CurrentActiveSkill]);
				_refEntity.CheckUsePassiveSkill(_refEntity.CurrentActiveSkill, _refEntity.GetCurrentSkillObj().weaponStatus, _refEntity.GetCurrentSkillObj().ShootTransform[_refEntity.CurrentActiveSkill]);
				OrangeBattleUtility.UpdateSkillCD(_refEntity.GetCurrentSkillObj());
				isSkillEventEnd = true;
			}
			else if (isSkillEventEnd && (ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.LEFT) || ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.RIGHT)))
			{
				endFrame = nowFrame + 1;
			}
			break;
		case 1:
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
				Vector3 vec = _refEntity.Controller.LogicPosition.vec3;
				_refEntity.PushBulletDetail(_refEntity.GetCurrentSkillObj().BulletData, _refEntity.GetCurrentSkillObj().weaponStatus, vec, _refEntity.GetCurrentSkillObj().SkillLV, Vector3.zero, false, 1);
				_refEntity.CheckUsePassiveSkill(_refEntity.CurrentActiveSkill, _refEntity.GetCurrentSkillObj().weaponStatus, _refEntity.GetCurrentSkillObj().ShootTransform[_refEntity.CurrentActiveSkill]);
				if (linkSkl != null)
				{
					_refEntity.PushBulletDetail(linkSkl, _refEntity.GetCurrentSkillObj().weaponStatus, vec, _refEntity.GetCurrentSkillObj().SkillLV, Vector3.zero, false, 1);
					_refEntity.CheckUsePassiveSkill(0, linkSkl, _refEntity.PlayerSkills[1].weaponStatus, _refEntity.PlayerSkills[1].ShootTransform[1]);
				}
				OrangeBattleUtility.UpdateSkillCD(_refEntity.GetCurrentSkillObj());
				isSkillEventEnd = true;
			}
			break;
		}
	}

	private void ResetToIdle()
	{
		if (bowlMesh_c.activeSelf)
		{
			bowlMesh_c.SetActive(false);
		}
		if (_refEntity.AnimateID == (HumanBase.AnimateId)129u)
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
		else if (_refEntity.Controller.Collisions.below || _refEntity.Controller.Collisions.JSB_below)
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

	public override void ClearSkill()
	{
		if (_refEntity.CurrentActiveSkill != -1)
		{
			_refEntity.EnableCurrentWeapon();
			OrangeBattleUtility.UpdateSkillCD(_refEntity.GetCurrentSkillObj());
		}
		if (bowlMesh_c.activeSelf)
		{
			bowlMesh_c.gameObject.SetActive(false);
		}
		_refEntity.IgnoreGravity = false;
		isSkillEventEnd = false;
		_refEntity.SkillEnd = true;
		_refEntity.CurrentActiveSkill = -1;
		_refEntity.BulletCollider.HitCallback = null;
		_refEntity.BulletCollider.BackToPool();
	}

	public override void CreateSkillBullet(WeaponStruct weaponStruct)
	{
		_refEntity.FreshBullet = true;
		_refEntity.IsShoot = 1;
		_refEntity.StartShootTimer();
		_refEntity.PushBulletDetail(weaponStruct.BulletData, weaponStruct.weaponStatus, shootPointTransform, weaponStruct.SkillLV);
	}

	public override void CallPet(int petID, bool isHurt, int nSetNumID = -1, Vector3? vSetPos = null)
	{
		if (petTable == null || (nSetNumID == -1 && !_refEntity.IsLocalPlayer))
		{
			return;
		}
		RemovePet();
		pet = MonoBehaviourSingleton<PoolManager>.Instance.GetPoolObj<CH031_SKL00>(petTable.s_MODEL);
		if ((bool)pet)
		{
			pet.activeSE = new string[2] { "SkillSE_CIEL2", "cl2_elf01_lp" };
			pet.unactiveSE = new string[2] { "SkillSE_CIEL2", "cl2_elf01_stop" };
			pet.transform.SetParentNull();
			pet.Set_follow_Player(_refEntity);
			pet.SetParams(petTable.s_MODEL, (long)(petSklTable.f_EFFECT_Z * 1000f), petTable.n_SKILL_0, _refEntity.PlayerSkills[0].weaponStatus, 0L);
			pet.transform.localEulerAngles = new Vector3(0f, 90f, 0f);
			pet.SetFollowOffset(new Vector3(-0.6f, 1f, 0f));
			pet.SetFollowEnabled(true);
			pet.SetActive(true);
			petActive = true;
			if (_refEntity.IsLocalPlayer)
			{
				StageUpdate.SyncStageObj(4, 4, _refEntity.sNetSerialID + "," + pet.PetID + "," + 0, true);
			}
			petActiveFrameEnd = nowFrame + petActiveFrame;
		}
	}

	private new void RemovePet()
	{
		if (petActive)
		{
			petActive = false;
			if (pet != null)
			{
				pet.SetActive(false);
				pet = null;
			}
		}
	}

	public override void ControlCharacterDead()
	{
		RemovePet();
	}

	private void UpdatePetStatus()
	{
		if (petActive && (_refEntity.bLockInputCtrl || nowFrame >= petActiveFrameEnd))
		{
			RemovePet();
		}
	}

	public void TeleportInExtraEffect()
	{
		MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(GetTeleportInExtraEffect(), _refEntity.ModelTransform.position, Quaternion.identity, Array.Empty<object>());
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
		return sFxuseCutIn;
	}

	public override string[][] GetCharacterDependAnimationsBlendTree()
	{
		string[] array = new string[3] { "ch057_skill_01_stand_up", "ch057_skill_01_stand_mid", "ch057_skill_01_stand_down" };
		string[] array2 = new string[3] { "ch057_skill_01_jump_up", "ch057_skill_01_jump_mid", "ch057_skill_01_jump_down" };
		string[] array3 = new string[3] { "ch057_skill_01_crouch_up", "ch057_skill_01_crouch_mid", "ch057_skill_01_crouch_down" };
		return new string[3][] { array, array2, array3 };
	}

	public override string[] GetCharacterDependAnimations()
	{
		return new string[2] { "ch057_skill_02_stand", "ch057_skill_02_jump" };
	}
}
