using System;
using System.Collections.Generic;
using StageLib;
using UnityEngine;

public class CH047_Controller : CharacterControlBase
{
	private readonly int SKILL0_END = (int)(0.5f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKILL1_END = (int)(0.5f / GameLogicUpdateManager.m_fFrameLen);

	private int nowFrame;

	private int skillProcessFrame;

	private Vector3 petSummonPoint;

	private Transform shoulderShootPoint;

	private Transform kneeShootPointLeft;

	private Transform kneeShootPointRight;

	private int petID = -1;

	private long petTime;

	private int mineCount;

	private Vector3 minePosition;

	protected List<SCH004Controller> _liPets = new List<SCH004Controller>();

	public override void Start()
	{
		base.Start();
		InitializeSkill();
		InitializePet();
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
		Transform[] target = _refEntity._transform.GetComponentsInChildren<Transform>(true);
		shoulderShootPoint = OrangeBattleUtility.FindChildRecursive(ref target, "Sub_ShootPoint", true);
		kneeShootPointLeft = OrangeBattleUtility.FindChildRecursive(ref target, "KneeCannon_Bone_L", true);
		kneeShootPointRight = OrangeBattleUtility.FindChildRecursive(ref target, "KneeCannon_Bone_R", true);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_battomahawk_000", 3);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_battomahawk_001", 3);
	}

	private void InitializePet()
	{
		bool flag = false;
		int num = 0;
		int follow_skill_id = 0;
		for (int i = 0; i < _refEntity.PlayerSkills.Length; i++)
		{
			if (_refEntity.PlayerSkills[i] != null && _refEntity.PlayerSkills[i].BulletData.n_EFFECT == 16)
			{
				flag = true;
				num = (int)_refEntity.PlayerSkills[i].BulletData.f_EFFECT_X;
				petTime = (long)(_refEntity.PlayerSkills[i].BulletData.f_EFFECT_Z * 1000f);
				follow_skill_id = i;
				break;
			}
		}
		if (flag)
		{
			petID = num;
			PetBuilder petBuilder = new GameObject().AddComponent<PetBuilder>();
			petBuilder.PetID = num;
			petBuilder.follow_skill_id = follow_skill_id;
			petBuilder.CreatePet(delegate(SCH004Controller obj)
			{
				obj.Set_follow_Player(_refEntity, false);
				obj.SetFollowEnabled(false);
				PET_TABLE pET_TABLE = ManagedSingleton<OrangeDataManager>.Instance.PET_TABLE_DICT[petID];
				MonoBehaviourSingleton<PoolManager>.Instance.CreatePoolBaseLocal<SCH004Controller>(obj, pET_TABLE.s_MODEL, 6);
				obj.activeSE = new string[2] { "SkillSE_VAVA", "va_pbomb" };
				obj.SetParams(pET_TABLE.s_MODEL, 0L, 0, null, 0L);
			});
		}
	}

	public override void CallPet(int petID, bool isHurt, int nSetNumID, Vector3? vSetPos = null)
	{
		if (petID != this.petID || (nSetNumID != -1 && mineCount >= nSetNumID) || (!_refEntity.IsLocalPlayer && nSetNumID == -1))
		{
			return;
		}
		if (nSetNumID == -1)
		{
			mineCount++;
			if (mineCount < 0)
			{
				mineCount = 0;
			}
		}
		else
		{
			mineCount = nSetNumID;
		}
		if (vSetPos.HasValue)
		{
			petSummonPoint = vSetPos ?? petSummonPoint;
		}
		PET_TABLE pET_TABLE = ManagedSingleton<OrangeDataManager>.Instance.PET_TABLE_DICT[this.petID];
		SCH004Controller poolObj = MonoBehaviourSingleton<PoolManager>.Instance.GetPoolObj<SCH004Controller>(pET_TABLE.s_MODEL);
		poolObj.PetID = petID;
		poolObj.SetOwner(_refEntity, mineCount);
		poolObj.SetParams(pET_TABLE.s_MODEL, petTime, 0, null, 0L);
		poolObj.activeSE = new string[2] { "SkillSE_VAVA", "va_pbomb" };
		poolObj.SetPositionAndRotation(petSummonPoint, false);
		MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_battomahawk_001", petSummonPoint, OrangeCharacter.NormalQuaternion, Array.Empty<object>());
		if (nSetNumID == -1)
		{
			StageUpdate.SyncStageObj(4, 4, _refEntity.sNetSerialID + "," + petID + "," + mineCount + "," + petSummonPoint.x + "," + petSummonPoint.y + "," + petSummonPoint.z, true);
		}
		poolObj.SetActive(true);
		for (int num = _liPets.Count - 1; num >= 0; num--)
		{
			if (_liPets[num] == null || !_liPets[num].Activate)
			{
				_liPets.RemoveAt(num);
			}
		}
		_liPets.Add(poolObj);
	}

	public override void PlayerPressSkillCharacterCall(int id)
	{
		if (_refEntity.CurrentActiveSkill == -1 && id == 1 && _refEntity.CheckUseSkillKeyTrigger(id))
		{
			_refEntity.CurrentActiveSkill = id;
			_refEntity.SetHorizontalSpeed(0);
			_refEntity.PlayerStopDashing();
			_refEntity.SkillEnd = false;
			_refEntity.DisableCurrentWeapon();
			_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1);
		}
	}

	public override void PlayerReleaseSkillCharacterCall(int id)
	{
		if (_refEntity.CurrentActiveSkill == -1 && id == 0 && _refEntity.CheckUseSkillKeyTrigger(id))
		{
			_refEntity.CurrentActiveSkill = id;
			_refEntity.SetHorizontalSpeed(0);
			_refEntity.PlayerStopDashing();
			_refEntity.SkillEnd = false;
			_refEntity.DisableCurrentWeapon();
			_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0);
		}
	}

	public void SetStatusCharacterDepend(OrangeCharacter.MainStatus mainStatus, OrangeCharacter.SubStatus subStatus)
	{
		nowFrame = GameLogicUpdateManager.GameFrame;
		if (mainStatus != OrangeCharacter.MainStatus.SKILL)
		{
			return;
		}
		switch (subStatus)
		{
		case OrangeCharacter.SubStatus.SKILL0:
			petSummonPoint = shoulderShootPoint.position;
			CreateSkillBullet(_refEntity.PlayerSkills[_refEntity.CurrentActiveSkill]);
			_refEntity.CheckUsePassiveSkill(0, _refEntity.PlayerSkills[0].weaponStatus, _refEntity.PlayerSkills[0].ShootTransform[0]);
			OrangeBattleUtility.UpdateSkillCD(_refEntity.GetCurrentSkillObj());
			MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_battomahawk_000", shoulderShootPoint.position, (_refEntity._characterDirection != CharacterDirection.LEFT) ? OrangeCharacter.NormalQuaternion : OrangeCharacter.ReversedQuaternion, Array.Empty<object>());
			PlayVoiceSE("v_va_skill03");
			if (_refEntity.AnimateID == HumanBase.AnimateId.ANI_CROUCH || _refEntity.AnimateID == HumanBase.AnimateId.ANI_CROUCH_END)
			{
				_refEntity.SetAnimateId(HumanBase.AnimateId.ANI_BTSKILL_START);
			}
			else if (_refEntity.Controller.Collisions.below || _refEntity.Controller.Collisions.JSB_below)
			{
				_refEntity.SetAnimateId((HumanBase.AnimateId)128u);
			}
			else
			{
				_refEntity.IgnoreGravity = true;
				_refEntity.SetAnimateId((HumanBase.AnimateId)129u);
			}
			skillProcessFrame = nowFrame + SKILL0_END;
			break;
		case OrangeCharacter.SubStatus.SKILL1:
		{
			int num = Math.Sign(_refEntity.ShootDirection.normalized.x);
			_refEntity.direction = ((num != 0) ? num : _refEntity.direction);
			_refEntity.CheckUsePassiveSkill(1, _refEntity.PlayerSkills[1].weaponStatus, _refEntity.PlayerSkills[1].ShootTransform[0]);
			OrangeBattleUtility.UpdateSkillCD(_refEntity.GetCurrentSkillObj());
			if (_refEntity.AnimateID == HumanBase.AnimateId.ANI_CROUCH || _refEntity.AnimateID == HumanBase.AnimateId.ANI_CROUCH_END)
			{
				_refEntity.SetAnimateId(HumanBase.AnimateId.ANI_SKILL_START);
				petSummonPoint = kneeShootPointLeft.position;
			}
			else
			{
				_refEntity.IgnoreGravity = true;
				_refEntity.SetAnimateId((HumanBase.AnimateId)66u);
				petSummonPoint = kneeShootPointRight.position;
			}
			PlayVoiceSE("v_va_skill04");
			CallPet(petID, false, -1, null);
			skillProcessFrame = nowFrame + SKILL1_END;
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
			if (nowFrame < skillProcessFrame)
			{
				break;
			}
			ResetSkill();
			if (_refEntity.Controller.Collisions.below || _refEntity.Controller.Collisions.JSB_below)
			{
				if (_refEntity.AnimateID == HumanBase.AnimateId.ANI_BTSKILL_START)
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
				else
				{
					_refEntity.SetStatus(OrangeCharacter.MainStatus.IDLE, OrangeCharacter.SubStatus.IDLE);
				}
			}
			else
			{
				_refEntity.SetStatus(OrangeCharacter.MainStatus.FALL, OrangeCharacter.SubStatus.TELEPORT_POSE);
			}
			break;
		case OrangeCharacter.SubStatus.SKILL1:
			if (nowFrame < skillProcessFrame)
			{
				break;
			}
			ResetSkill();
			if (_refEntity.Controller.Collisions.below || _refEntity.Controller.Collisions.JSB_below)
			{
				if (_refEntity.AnimateID == HumanBase.AnimateId.ANI_SKILL_START)
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
				else
				{
					_refEntity.SetStatus(OrangeCharacter.MainStatus.IDLE, OrangeCharacter.SubStatus.IDLE);
				}
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
		if (_refEntity.CurrentActiveSkill != -1)
		{
			_refEntity.EnableCurrentWeapon();
			if (_refEntity.PlayerSkills[_refEntity.CurrentActiveSkill].MagazineRemain > 0f)
			{
				OrangeBattleUtility.UpdateSkillCD(_refEntity.GetCurrentSkillObj());
			}
			if (_refEntity.CurrentActiveSkill != 0)
			{
				int num = 1;
			}
		}
		ResetSkill();
	}

	private void ResetSkill()
	{
		_refEntity.Dashing = false;
		_refEntity.SetSpeed(0, 0);
		_refEntity.IgnoreGravity = false;
		_refEntity.SkillEnd = true;
		_refEntity.CurrentActiveSkill = -1;
		_refEntity.EnableCurrentWeapon();
	}

	public void TeleportInExtraEffect()
	{
		PlaySkillSE("va_start01");
		MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(GetTeleportInExtraEffect(), _refEntity.ModelTransform.position, Quaternion.identity, Array.Empty<object>());
	}

	public void TeleportIn_addEffect()
	{
	}

	public override void CreateSkillBullet(WeaponStruct weaponStruct)
	{
		_refEntity.FreshBullet = true;
		_refEntity.IsShoot = 1;
		_refEntity.StartShootTimer();
		_refEntity.PushBulletDetail(weaponStruct.FastBulletDatas[0], weaponStruct.weaponStatus, shoulderShootPoint, weaponStruct.SkillLV);
	}

	public bool CheckPetActive(int petId)
	{
		for (int num = _liPets.Count - 1; num >= 0; num--)
		{
			if (_liPets[num] == null || !_liPets[num].Activate)
			{
				_liPets.RemoveAt(num);
			}
			else if (_liPets[num].PetID == petId)
			{
				return true;
			}
		}
		return false;
	}

	public override string[][] GetCharacterDependAnimationsBlendTree()
	{
		string[] array = new string[3] { "ch047_skill_01_crouch_up", "ch047_skill_01_crouch_mid", "ch047_skill_01_crouch_down" };
		string[] array2 = new string[3] { "ch047_skill_01_stand_up", "ch047_skill_01_stand_mid", "ch047_skill_01_stand_down" };
		string[] array3 = new string[3] { "ch047_skill_01_jump_up", "ch047_skill_01_jump_mid", "ch047_skill_01_jump_down" };
		return new string[3][] { array, array2, array3 };
	}

	public override string[] GetCharacterDependAnimations()
	{
		return new string[2] { "ch047_skill_02_crouch", "ch047_skill_02_jump" };
	}

	public override string GetTeleportInExtraEffect()
	{
		return "fxuse_vavahalloween_in";
	}
}
