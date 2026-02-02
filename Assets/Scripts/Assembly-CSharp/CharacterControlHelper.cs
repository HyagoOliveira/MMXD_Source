#define RELEASE
using System;
using System.Collections.Generic;
using OrangeCriRelay;
using StageLib;
using UnityEngine;

public class CharacterControlHelper : ManagedSingleton<CharacterControlHelper>
{
	public readonly int NOVASTRIKE_PREPARE_FRAME = 3;

	public Vector3 ClimbingOffset
	{
		get
		{
			return new Vector3(0.5f, 0f, 0f);
		}
	}

	public override void Initialize()
	{
	}

	public override void Dispose()
	{
	}

	public void ChangeToSklStatus(OrangeCharacter _refEntity, int p_skillIdx, int p_sklTriggerFrame, int p_endFrame, OrangeCharacter.SubStatus p_nextStatus, out int skillEventFrame, out int skillEndFrame)
	{
		skillEventFrame = GameLogicUpdateManager.GameFrame + p_sklTriggerFrame;
		skillEndFrame = GameLogicUpdateManager.GameFrame + p_endFrame;
		_refEntity.SkillEnd = false;
		_refEntity.DisableCurrentWeapon();
		_refEntity.CurrentActiveSkill = p_skillIdx;
		_refEntity.SetHorizontalSpeed(0);
		_refEntity.PlayerStopDashing();
		_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, p_nextStatus);
	}

	public void ChangeToNextStatus(OrangeCharacter _refEntity, int p_sklTriggerFrame, int p_endFrame, OrangeCharacter.SubStatus p_nextStatus, out int skillEventFrame, out int skillEndFrame)
	{
		skillEventFrame = GameLogicUpdateManager.GameFrame + p_sklTriggerFrame;
		skillEndFrame = GameLogicUpdateManager.GameFrame + p_endFrame;
		_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, p_nextStatus);
	}

	public void PushBulletSkl(OrangeCharacter _refEntity, Transform shootPointTransform, MagazineType magazineType, int reloadIdx = -1, sbyte isShoot = 0, bool triggerPassiveSkill = true)
	{
		PushBulletSkl(_refEntity, shootPointTransform, _refEntity.ShootDirection, magazineType, reloadIdx, isShoot, triggerPassiveSkill);
	}

	public void PushBulletSkl(OrangeCharacter _refEntity, Transform shootPointTransform, Vector3 shootDirection, MagazineType magazineType, int reloadIdx = -1, sbyte isShoot = 0, bool triggerPassiveSkill = true)
	{
		int currentActiveSkill = _refEntity.CurrentActiveSkill;
		WeaponStruct currentSkillObj = _refEntity.GetCurrentSkillObj();
		SKILL_TABLE bulletData;
		int? nSkillIndex;
		if (reloadIdx == -1)
		{
			bulletData = currentSkillObj.BulletData;
			nSkillIndex = null;
		}
		else
		{
			bulletData = currentSkillObj.FastBulletDatas[reloadIdx];
			nSkillIndex = reloadIdx;
		}
		_refEntity.FreshBullet = true;
		_refEntity.IsShoot = isShoot;
		_refEntity.PushBulletDetail(bulletData, currentSkillObj.weaponStatus, shootPointTransform, currentSkillObj.SkillLV, shootDirection);
		switch (magazineType)
		{
		case MagazineType.ENERGY:
			OrangeBattleUtility.UpdateSkillCD(currentSkillObj);
			break;
		case MagazineType.MEASURE:
			_refEntity.selfBuffManager.AddMeasure(-_refEntity.PlayerSkills[currentActiveSkill].BulletData.n_USE_COST);
			break;
		}
		if (triggerPassiveSkill)
		{
			if (isShoot > 0)
			{
				_refEntity.CheckUsePassiveSkill(currentActiveSkill, currentSkillObj.weaponStatus, currentSkillObj.ShootTransform[0], null, nSkillIndex);
			}
			else
			{
				_refEntity.CheckUsePassiveSkill(currentActiveSkill, currentSkillObj.weaponStatus, currentSkillObj.ShootTransform[0], _refEntity.direction * Vector2.right, nSkillIndex);
			}
		}
	}

	public void Play360ShootEft(OrangeCharacter _refEntity, string useFxName, Vector3 position)
	{
		Vector3 shootDirection = _refEntity.ShootDirection;
		float num = Mathf.Atan2(shootDirection.y, shootDirection.x);
		Vector3 one = Vector3.one;
		if (Mathf.Abs(num) - 90f > 0f)
		{
			num = (180f - Mathf.Abs(num)) * Mathf.Sign(num);
			one.x = -1f;
		}
		MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(useFxName, position, Quaternion.Euler(new Vector3(0f, 0f, num * 57.29578f)), new object[1] { one });
	}

	public void NOVASTRIKE_Prepare(OrangeCharacter _refEntity, int sklId)
	{
		if (_refEntity.CheckUseSkillKeyTrigger(sklId))
		{
			if (_refEntity.Dashing)
			{
				_refEntity.PlayerStopDashing();
			}
			_refEntity.CurrentActiveSkill = sklId;
			_refEntity.SkillEnd = false;
			_refEntity.SetSpeed((int)_refEntity._characterDirection * OrangeCharacter.WalkSpeed, (int)((float)OrangeCharacter.JumpSpeed * 0.5f));
			_refEntity.StopShootTimer();
			_refEntity.DisableCurrentWeapon();
			_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.RIDE_ARMOR);
		}
	}

	public void NOVASTRIKE_Begin(OrangeCharacter _refEntity, OrangeTimer NOVASTRIKETimer, int sklId, bool UsePassiveSkill = true, bool skillCD = true)
	{
		if (_refEntity.CurrentActiveSkill != sklId)
		{
			_refEntity.CurrentActiveSkill = sklId;
		}
		if (_refEntity.Velocity.y <= 0)
		{
			NOVASTRIKE_Prepare_To_Loop(_refEntity, NOVASTRIKETimer, sklId, UsePassiveSkill, skillCD);
		}
	}

	public void NOVASTRIKE_Prepare_To_Loop(OrangeCharacter _refEntity, OrangeTimer NOVASTRIKETimer, int sklId, bool UsePassiveSkill = true, bool skillCD = true)
	{
		if (skillCD)
		{
			OrangeBattleUtility.UpdateSkillCD(_refEntity.PlayerSkills[sklId]);
		}
		NOVASTRIKETimer.TimerStart();
		_refEntity.SetSpeed((int)_refEntity._characterDirection * (int)((float)OrangeCharacter.DashSpeed * 2.5f), 0);
		_refEntity.BulletCollider.UpdateBulletData(_refEntity.GetCurrentSkillObj().BulletData, _refEntity.sPlayerName, _refEntity.GetNowRecordNO(), _refEntity.nBulletRecordID++);
		_refEntity.BulletCollider.SetBulletAtk(_refEntity.GetCurrentSkillObj().weaponStatus, _refEntity.selfBuffManager.sBuffStatus);
		_refEntity.BulletCollider.BulletLevel = _refEntity.GetCurrentSkillObj().SkillLV;
		_refEntity.BulletCollider.Active(_refEntity.TargetMask);
		_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.IDLE);
		if (UsePassiveSkill)
		{
			_refEntity.CheckUsePassiveSkill(_refEntity.CurrentActiveSkill, _refEntity.GetCurrentSkillObj().weaponStatus, _refEntity.GetCurrentSkillObj().ShootTransform[0]);
		}
	}

	public bool NOVASTRIKE_Loop(OrangeCharacter _refEntity, OrangeTimer NOVASTRIKETimer, int sklId)
	{
		if (_refEntity.CurrentActiveSkill != sklId)
		{
			_refEntity.CurrentActiveSkill = sklId;
		}
		if (NOVASTRIKETimer.GetMillisecond() > 417)
		{
			_refEntity.SetStatus(OrangeCharacter.MainStatus.FALL, OrangeCharacter.SubStatus.TELEPORT_POSE);
			_refEntity.SetSpeed(0, 0);
			_refEntity.SkillEnd = true;
			_refEntity.BulletCollider.BackToPool();
			return true;
		}
		return false;
	}

	public void PushPet(CharacterControlBase characterControlBase, OrangeCharacter _refEntity, int petId, bool triggerPassiveSkillAndCd = true)
	{
		WeaponStruct currentSkillObj = _refEntity.GetCurrentSkillObj();
		characterControlBase.CallPet(petId, false, -1, null);
		if (triggerPassiveSkillAndCd)
		{
			_refEntity.CheckUsePassiveSkill(_refEntity.CurrentActiveSkill, currentSkillObj.weaponStatus, currentSkillObj.ShootTransform[_refEntity.CurrentActiveSkill]);
			OrangeBattleUtility.UpdateSkillCD(currentSkillObj);
		}
	}

	public void TurnToAimTarget(OrangeCharacter _refEntity)
	{
		_refEntity.IsShoot = 1;
		Vector3? vector = _refEntity.CalibrateAimDirection(_refEntity.AimPosition);
		if (vector.HasValue)
		{
			int num = Math.Sign(vector.Value.x);
			if (_refEntity._characterDirection != (CharacterDirection)num && Mathf.Abs(vector.Value.x) > 0.05f)
			{
				_refEntity._characterDirection = (CharacterDirection)((int)_refEntity._characterDirection * -1);
				_refEntity.ShootDirection = vector.Value;
			}
		}
	}

	public void UpdateShootDirByAimDir(OrangeCharacter _refEntity)
	{
		if (_refEntity.UseAutoAim && _refEntity.IAimTargetLogicUpdate != null)
		{
			Vector3? vector = _refEntity.CalibrateAimDirection(_refEntity.ExtraTransforms[0].position, _refEntity.IAimTargetLogicUpdate);
			if (vector.HasValue)
			{
				_refEntity._characterDirection = ((vector.Value.x > 0f) ? CharacterDirection.RIGHT : CharacterDirection.LEFT);
				_refEntity.UpdateDirection();
			}
		}
	}

	public void InitEnhanceSkill(OrangeCharacter _refEntity, int skillIdx, int[] enhanceSkillIds, ref int _enhanceSlot, bool forceChangeIcon = false)
	{
		_enhanceSlot = _refEntity.PlayerSkills[skillIdx].EnhanceEXIndex;
		int skillId = enhanceSkillIds[_enhanceSlot];
		_refEntity.ReInitSkillStruct(skillIdx, skillId, forceChangeIcon);
		for (int i = 0; i < _refEntity.PlayerSkills[skillIdx].FastBulletDatas.Length; i++)
		{
			string s_MODEL = _refEntity.PlayerSkills[skillIdx].FastBulletDatas[i].s_MODEL;
			if (!ManagedSingleton<OrangeTableHelper>.Instance.IsNullOrEmpty(s_MODEL) && !MonoBehaviourSingleton<PoolManager>.Instance.IsPreload(s_MODEL))
			{
				BulletBase.PreloadBullet<BulletBase>(_refEntity.PlayerSkills[skillIdx].FastBulletDatas[i]);
			}
		}
	}

	public void PreloadLinkSkl<T>(OrangeCharacter _refEntity, int sklIdx, out SKILL_TABLE linkSkl) where T : BulletBase
	{
		linkSkl = null;
		WeaponStruct weaponStruct = _refEntity.PlayerSkills[sklIdx];
		if (weaponStruct != null && weaponStruct.BulletData.n_LINK_SKILL != 0 && ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT.TryGetNewValue(weaponStruct.BulletData.n_LINK_SKILL, out linkSkl))
		{
			_refEntity.tRefPassiveskill.ReCalcuSkill(ref linkSkl);
			MonoBehaviourSingleton<PoolManager>.Instance.CreatePoolBase<T>("prefab/bullet/" + linkSkl.s_MODEL, linkSkl.s_MODEL, 3, null);
		}
	}

	public void OffesetEntity(OrangeCharacter _refEntity, Vector3 offset)
	{
		Vector3 position = _refEntity.Controller.LogicPosition.vec3 + offset;
		_refEntity.Controller.LogicPosition = new VInt3(position);
		_refEntity.Controller.transform.position = position;
	}

	public void SetAnimate(OrangeCharacter _refEntity, HumanBase.AnimateId crouch, HumanBase.AnimateId stand, HumanBase.AnimateId jump, bool setGravity = true)
	{
		HumanBase.AnimateId animateID = _refEntity.AnimateID;
		if (animateID - 18 <= HumanBase.AnimateId.ANI_RIDEARMOR)
		{
			_refEntity.SetAnimateId(crouch);
			return;
		}
		if (_refEntity.IsInGround && _refEntity.Velocity.y <= 0)
		{
			_refEntity.SetAnimateId(stand);
			return;
		}
		if (setGravity)
		{
			_refEntity.IgnoreGravity = true;
		}
		_refEntity.SetAnimateId(jump);
	}

	public void SetAnimate(OrangeCharacter _refEntity, HumanBase.AnimateId crouch, HumanBase.AnimateId stand, HumanBase.AnimateId jump, List<HumanBase.AnimateId> listCrouchAnim)
	{
		if (listCrouchAnim.Contains(_refEntity.AnimateID) && ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.DOWN))
		{
			_refEntity.SetAnimateId(crouch);
		}
		else
		{
			SetAnimate(_refEntity, crouch, stand, jump);
		}
	}

	public List<SKILL_TABLE> PetInit<T>(IPetSummoner summoner, OrangeCharacter _refEntity) where T : PetControllerBase
	{
		summoner.PetID = -1;
		summoner.PetTime = 0L;
		List<SKILL_TABLE> list = new List<SKILL_TABLE>();
		for (int i = 0; i < _refEntity.PlayerSkills.Length; i++)
		{
			int num = i;
			WeaponStruct weaponStruct = _refEntity.PlayerSkills[num];
			if (weaponStruct == null)
			{
				continue;
			}
			SKILL_TABLE bulletData = weaponStruct.BulletData;
			if (weaponStruct.BulletData.n_EFFECT == 16)
			{
				for (int j = 1; j < weaponStruct.BulletData.n_CHARGE_MAX_LEVEL + 1; j++)
				{
					list.Add(weaponStruct.FastBulletDatas[j]);
				}
				summoner.PetID = (int)bulletData.f_EFFECT_X;
				summoner.PetTime = (long)(bulletData.f_EFFECT_Z * 1000f);
				CreatePet<T>(_refEntity, summoner.PetID, num, summoner.PetTime, list);
				break;
			}
		}
		return list;
	}

	public void PetInit<T>(IPetSummoner summoner, OrangeCharacter _refEntity, int nSkillIndex, SKILL_TABLE tSkillTable) where T : PetControllerBase
	{
		summoner.PetID = -1;
		summoner.PetTime = 0L;
		if (tSkillTable != null && tSkillTable.n_EFFECT == 16)
		{
			summoner.PetID = (int)tSkillTable.f_EFFECT_X;
			summoner.PetTime = (long)(tSkillTable.f_EFFECT_Z * 1000f);
			CreatePet<T>(_refEntity, summoner.PetID, nSkillIndex, summoner.PetTime);
		}
	}

	private void CreatePet<T>(OrangeCharacter _refEntity, int petID, int skillIndex, long petTime = 0L, List<SKILL_TABLE> listPetSkillTable = null) where T : PetControllerBase
	{
		PET_TABLE petTable;
		if (ManagedSingleton<OrangeDataManager>.Instance.PET_TABLE_DICT.TryGetValue(petID, out petTable))
		{
			PetBuilder petBuilder = new GameObject().AddComponent<PetBuilder>();
			petBuilder.PetID = petID;
			petBuilder.follow_skill_id = skillIndex;
			petBuilder.CreatePet(delegate(T objs)
			{
				PET_TABLE pET_TABLE = ManagedSingleton<OrangeDataManager>.Instance.PET_TABLE_DICT[petID];
				MonoBehaviourSingleton<PoolManager>.Instance.CreatePoolBaseLocal<T>(objs, pET_TABLE.s_MODEL, 6);
				objs.SetParams(petTable.s_MODEL, petTime, petTable.n_SKILL_0, _refEntity.PlayerSkills[skillIndex].weaponStatus, 0L);
				if (listPetSkillTable != null && listPetSkillTable.Count > 0)
				{
					objs.ReplaceListBulletSkillTable(listPetSkillTable);
				}
			});
		}
		else
		{
			Debug.LogError("[CharacterControlHelper] Create Pet Fail ! Not Exist PetID :" + petID);
		}
	}

	public T CallPet<T>(IPetSummoner summoner, OrangeCharacter _refEntity, int petID, int nSetNumID, bool linkPlayerBuff, bool followPlayer, bool activeNow = true, string acb = null, string cue = null, Vector3? pos = null) where T : PetControllerBase
	{
		int petID2 = summoner.PetID;
		int petCount = summoner.PetCount;
		long petTime = summoner.PetTime;
		if (petID != petID2 || petID2 == -1)
		{
			return null;
		}
		if (_refEntity.IsLocalPlayer || nSetNumID != -1)
		{
			if (nSetNumID == -1)
			{
				petCount++;
				if (petCount < 0)
				{
					petCount = 0;
				}
			}
			else
			{
				petCount = nSetNumID;
			}
			PET_TABLE pET_TABLE = ManagedSingleton<OrangeDataManager>.Instance.PET_TABLE_DICT[petID2];
			T poolObj = MonoBehaviourSingleton<PoolManager>.Instance.GetPoolObj<T>(pET_TABLE.s_MODEL);
			poolObj.PetID = petID2;
			poolObj.Set_follow_Player(_refEntity, linkPlayerBuff);
			poolObj.SetFollowEnabled(followPlayer);
			poolObj.SetParams(pET_TABLE.s_MODEL, petTime, pET_TABLE.n_SKILL_0, _refEntity.PlayerSkills[0].weaponStatus, 0L);
			poolObj.sNetSerialID = _refEntity.sNetSerialID + pET_TABLE.n_ID + petCount;
			if (nSetNumID == -1)
			{
				if (!pos.HasValue)
				{
					StageUpdate.SyncStageObj(4, 4, _refEntity.sNetSerialID + "," + petID + "," + petCount, true);
				}
				else
				{
					Vector3 value = pos.Value;
					StageUpdate.SyncStageObj(4, 4, _refEntity.sNetSerialID + "," + petID + "," + petCount + "," + value.x + "," + value.y + "," + value.z, true);
				}
			}
			if (acb == null || cue == null)
			{
				CharacterParam callPetParam = _refEntity.CallPetParam;
				if (callPetParam != null)
				{
					_refEntity.PlaySE(callPetParam.PetSE[0], callPetParam.PetSE[1], callPetParam.Delay);
				}
			}
			else
			{
				_refEntity.PlaySE(acb, cue);
			}
			if (activeNow)
			{
				poolObj.SetActive(true);
			}
			summoner.PetCount = petCount;
			return poolObj;
		}
		return null;
	}

	public void CheckBreakFrame(string p_userId, ref int endFrame)
	{
		if (ManagedSingleton<InputStorage>.Instance.IsHeld(p_userId, ButtonId.LEFT) || ManagedSingleton<InputStorage>.Instance.IsHeld(p_userId, ButtonId.RIGHT))
		{
			endFrame = GameLogicUpdateManager.GameFrame + 1;
		}
	}
}
