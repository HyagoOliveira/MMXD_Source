using UnityEngine;

public class CH063_Controller : CH039_Controller
{
	private BulletBase mSkillBullet;

	private const int buffID = -1;

	private PetSummoner summoner = new PetSummoner();

	private SCH022Controller sch022;

	protected override string[] Pcb_activeSE
	{
		get
		{
			return new string[2] { "SkillSE_ICO", "ic_replo02_lp" };
		}
	}

	protected override string[] Pcb_unactiveSE
	{
		get
		{
			return new string[2] { "SkillSE_ICO", "ic_replo02_stop" };
		}
	}

	protected override string[] AnalyzeSE
	{
		get
		{
			return new string[2] { "", "" };
		}
	}

	protected override string Fxuse000
	{
		get
		{
			return "fxuse_datamining_002";
		}
	}

	protected override string Fxuse001
	{
		get
		{
			return "fxuse_datamining_003";
		}
	}

	protected override int SKL1_END
	{
		get
		{
			return (int)(0.833f / GameLogicUpdateManager.m_fFrameLen);
		}
	}

	protected override void InitializeSkill()
	{
		base.InitializeSkill();
		if (_refEntity.tRefPassiveskill.listUsePassiveskill.Count <= 0)
		{
			return;
		}
		for (int i = 0; i < _refEntity.tRefPassiveskill.listUsePassiveskill.Count; i++)
		{
			SKILL_TABLE tSKILL_TABLE = _refEntity.tRefPassiveskill.listUsePassiveskill[i].tSKILL_TABLE;
			if (tSKILL_TABLE.n_EFFECT == 16)
			{
				ManagedSingleton<CharacterControlHelper>.Instance.PetInit<SCH022Controller>(summoner, _refEntity, 1, tSKILL_TABLE);
				break;
			}
		}
	}

	public override void OverrideDelegateEvent()
	{
		base.OverrideDelegateEvent();
		_refEntity.EnterRideArmorEvt = EnterRideArmor;
	}

	public override void TeleportInCharacterDepend()
	{
		UpdateCustomWeaponRenderer(false);
	}

	protected override void UpdateTeleportOut(OrangeCharacter.SubStatus subStatus)
	{
	}

	public override void CheckSkill()
	{
		CheckSkillBullet();
		base.CheckSkill();
	}

	private void CheckSkillBullet()
	{
		if (mSkillBullet != null)
		{
			if (_refEntity.IsDead())
			{
				mSkillBullet.BackToPool();
				mSkillBullet = null;
				_refEntity.selfBuffManager.RemoveBuffByCONDITIONID(-1);
			}
			else if (!_refEntity.selfBuffManager.CheckHasEffectByCONDITIONID(-1))
			{
				mSkillBullet.BackToPool();
				mSkillBullet = null;
			}
			else if (mSkillBullet.bIsEnd && _refEntity.IsLocalPlayer)
			{
				_refEntity.selfBuffManager.RemoveBuffByCONDITIONID(-1);
				mSkillBullet = null;
			}
		}
		else if (_refEntity.selfBuffManager.CheckHasEffectByCONDITIONID(-1) && mSkillBullet == null)
		{
			mSkillBullet = _refEntity.CreateFSBulletEx(_refEntity.PlayerSkills[1], 0);
			mSkillBullet.transform.localRotation = Quaternion.identity;
		}
	}

	protected override void SKILL1()
	{
		WeaponStruct currentSkillObj = _refEntity.GetCurrentSkillObj();
		if (_refEntity.IsLocalPlayer)
		{
			_refEntity.selfBuffManager.AddBuff(-1, 0, 0, 0);
		}
		_refEntity.CheckUsePassiveSkill(_refEntity.CurrentActiveSkill, currentSkillObj.weaponStatus, currentSkillObj.ShootTransform[0]);
		OrangeBattleUtility.UpdateSkillCD(currentSkillObj);
		ayTimer.TimerStart();
	}

	public override void CallPet(int petID, bool isHurt, int nSetNumID, Vector3? vSetPos = null)
	{
		if (petID == base.PetID)
		{
			base.CallPet(petID, isHurt, nSetNumID, vSetPos);
			return;
		}
		RemovePet();
		SCH022Controller sCH022Controller = ManagedSingleton<CharacterControlHelper>.Instance.CallPet<SCH022Controller>(summoner, _refEntity, petID, nSetNumID, true, true, false);
		if ((bool)sCH022Controller)
		{
			sCH022Controller.transform.SetParentNull();
			sCH022Controller.SetFollowOffset(new Vector3(0f, 0.8f, 0f));
			sCH022Controller.activeSE = new string[2] { "", "" };
			sCH022Controller.unactiveSE = new string[2] { "", "" };
			sCH022Controller.SetActive(true);
			sch022 = sCH022Controller;
		}
	}

	public bool EnterRideArmor(RideBaseObj targetRideArmor)
	{
		if (mSkillBullet != null)
		{
			mSkillBullet.BackToPool();
		}
		CheckSkillBullet();
		RemovePet();
		return _refEntity.EnterRideArmor(targetRideArmor);
	}

	public override void ControlCharacterDead()
	{
		RemovePet();
	}

	private new void RemovePet()
	{
		if (sch022 != null && sch022.Activate)
		{
			sch022.SetActive(false);
			sch022 = null;
		}
	}

	public override bool CheckPetActive(int petId)
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
		if (sch022 != null && sch022.Activate && sch022.PetID == petId)
		{
			return true;
		}
		return false;
	}

	public override string[] GetCharacterDependAnimations()
	{
		return new string[6] { "ch063_skill_01_stand", "ch063_skill_01_stand", "ch063_skill_01_stand", "ch063_skill_02_stand", "ch063_skill_02_stand", "ch063_skill_02_stand" };
	}
}
