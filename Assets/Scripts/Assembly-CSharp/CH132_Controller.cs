using System;
using System.Collections.Generic;
using UnityEngine;

public class CH132_Controller : CharacterControlBase, IPetSummoner
{
	private int nowFrame;

	private int skillEventFrame;

	private int endFrame;

	private int endBreakFrame;

	private bool isSkillEventEnd;

	protected PET_TABLE _tPetTable;

	protected List<SCH016Controller> _liPets = new List<SCH016Controller>();

	private CharacterMaterial saberCM;

	private readonly int SKL0_TRIGGER = (int)(0.3f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL0_END = (int)(0.833f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL0_END_BREAK = (int)(0.65f / GameLogicUpdateManager.m_fFrameLen);

	private readonly string FX_000 = "fxuse_GhostAttack_000";

	private readonly string FX_001 = "fxhit_PumpkinTrap_000";

	public int PetID { get; set; } = -1;


	public long PetTime { get; set; }

	public int PetCount { get; set; }

	public override void Start()
	{
		base.Start();
		InitPet();
		InitializeSkill();
	}

	private void InitPet()
	{
		ManagedSingleton<CharacterControlHelper>.Instance.PetInit<SCH016Controller>(this, _refEntity);
		PET_TABLE value;
		if (ManagedSingleton<OrangeDataManager>.Instance.PET_TABLE_DICT.TryGetValue(PetID, out value))
		{
			_tPetTable = value;
		}
	}

	private void InitializeSkill()
	{
		Transform[] target = _refEntity._transform.GetComponentsInChildren<Transform>(true);
		Transform transform = new GameObject("CustomShootPoint0").transform;
		transform.SetParent(base.transform);
		transform.localPosition = new Vector3(0f, 0.8f, 0f);
		_refEntity.ExtraTransforms = new Transform[1];
		_refEntity.ExtraTransforms[0] = transform;
		_refEntity.PlayerSkills[0].ShootTransform[0] = _refEntity.ExtraTransforms[0];
		_refEntity.PlayerSkills[1].ShootTransform[0] = _refEntity.ModelTransform;
		GameObject gameObject = OrangeBattleUtility.FindChildRecursive(ref target, "Saber_008_G", true).gameObject;
		if ((bool)gameObject)
		{
			saberCM = gameObject.GetComponent<CharacterMaterial>();
			saberCM.Appear();
		}
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(FX_000, 2);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(FX_001, 2);
	}

	public override void OverrideDelegateEvent()
	{
		base.OverrideDelegateEvent();
		_refEntity.CheckPetActiveEvt = CheckPetActive;
		_refEntity.TeleportInCharacterDependEvt = TeleportInCharacterDepend;
	}

	public void TeleportInCharacterDepend()
	{
		if (_refEntity.CurrentFrame >= 0.9f)
		{
			saberCM.Disappear();
		}
	}

	public override void PlayerPressSkillCharacterCall(int id)
	{
		if (_refEntity.CurrentActiveSkill == -1 && id == 1 && _refEntity.CheckUseSkillKeyTrigger(id))
		{
			if (_refEntity.IsLocalPlayer)
			{
				PlayVoiceSE("v_vi_skill04");
			}
			CreateTrap(_refEntity.PlayerSkills[1]);
		}
	}

	public override void PlayerReleaseSkillCharacterCall(int id)
	{
		if (_refEntity.CurrentActiveSkill == -1 && id == 0 && _refEntity.CheckUseSkillKeyTrigger(id))
		{
			_refEntity.CurrentActiveSkill = id;
			endBreakFrame = GameLogicUpdateManager.GameFrame + SKL0_END_BREAK;
			_refEntity.IsShoot = 1;
			ManagedSingleton<CharacterControlHelper>.Instance.ChangeToSklStatus(_refEntity, id, SKL0_TRIGGER, SKL0_END, OrangeCharacter.SubStatus.SKILL0, out skillEventFrame, out endFrame);
			ManagedSingleton<CharacterControlHelper>.Instance.SetAnimate(_refEntity, HumanBase.AnimateId.ANI_SKILL_START, (HumanBase.AnimateId)66u, (HumanBase.AnimateId)67u);
			WeaponStruct weaponStruct = _refEntity.PlayerSkills[0];
			OrangeBattleUtility.UpdateSkillCD(weaponStruct);
			_refEntity.CheckUsePassiveSkill(0, weaponStruct.weaponStatus, _refEntity.ModelTransform);
			MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FX_000, weaponStruct.ShootTransform[0].position, Quaternion.identity, Array.Empty<object>());
			PlayVoiceSE("v_vi_skill03");
			PlaySkillSE("vi_goast01");
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
		OrangeCharacter.SubStatus curSubStatus = _refEntity.CurSubStatus;
		if (curSubStatus == OrangeCharacter.SubStatus.SKILL0)
		{
			if (nowFrame >= endFrame)
			{
				OnSkillEnd();
			}
			else if (!isSkillEventEnd && nowFrame >= skillEventFrame)
			{
				isSkillEventEnd = true;
				ManagedSingleton<CharacterControlHelper>.Instance.UpdateShootDirByAimDir(_refEntity);
				ManagedSingleton<CharacterControlHelper>.Instance.PushBulletSkl(_refEntity, _refEntity.PlayerSkills[0].ShootTransform[0], MagazineType.NORMAL, -1, 1, false);
				PlaySkillSE("vi_goast02");
			}
			else if (isSkillEventEnd && nowFrame >= endBreakFrame)
			{
				ManagedSingleton<CharacterControlHelper>.Instance.CheckBreakFrame(_refEntity.UserID, ref endFrame);
			}
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

	public override void ClearSkill()
	{
		isSkillEventEnd = false;
		_refEntity.IgnoreGravity = false;
		_refEntity.SkillEnd = true;
		_refEntity.CurrentActiveSkill = -1;
		_refEntity.EnableCurrentWeapon();
	}

	public override void RemovePet()
	{
		for (int num = _liPets.Count - 1; num >= 0; num--)
		{
			if (_liPets[num] == null || !_liPets[num].Activate)
			{
				_liPets.RemoveAt(num);
			}
			else
			{
				_liPets[num].BackToPool();
			}
		}
		_liPets.Clear();
	}

	public override void SetStun(bool enable)
	{
		base.SetStun(enable);
		_refEntity.EnableCurrentWeapon();
	}

	protected void CreateTrap(WeaponStruct wsSkill)
	{
		if (_refEntity.IsLocalPlayer)
		{
			CallPet(PetID, false, -1, _refEntity.Controller.LogicPosition.vec3);
		}
		_refEntity.CheckUsePassiveSkill(1, wsSkill.weaponStatus, wsSkill.ShootTransform[0]);
		OrangeBattleUtility.UpdateSkillCD(wsSkill);
	}

	public override void CallPet(int petID, bool isHurt, int nSetNumID = -1, Vector3? vSetPos = null)
	{
		bool followPlayer = _tPetTable.n_MODE == 1;
		SCH016Controller sCH016Controller = ManagedSingleton<CharacterControlHelper>.Instance.CallPet<SCH016Controller>(this, _refEntity, petID, nSetNumID, false, followPlayer, false, null, null, vSetPos);
		if (!sCH016Controller)
		{
			return;
		}
		sCH016Controller.sActiveSE2 = new string[2] { "SkillSE_VIA", "vi_pumpkin01" };
		sCH016Controller.activeSE = new string[2] { "SkillSE_VIA", "vi_pumpkin02_lp" };
		sCH016Controller.unactiveSE = new string[2] { "SkillSE_VIA", "vi_pumpkin02_stop" };
		sCH016Controller.DestructSE = new string[2] { "SkillSE_VIA", "vi_pumpkin03" };
		sCH016Controller.DestructFx = FX_001;
		sCH016Controller.SetSkillLv(_refEntity.PlayerSkills[1].SkillLV);
		sCH016Controller.SetActive(true);
		sCH016Controller.Controller.LogicPosition = new VInt3(vSetPos ?? Vector3.zero);
		sCH016Controller.transform.position = vSetPos ?? Vector3.zero;
		for (int num = _liPets.Count - 1; num >= 0; num--)
		{
			if (_liPets[num] == null || !_liPets[num].Activate)
			{
				_liPets.RemoveAt(num);
			}
		}
		_liPets.Add(sCH016Controller);
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

	public override string[] GetCharacterDependAnimations()
	{
		return new string[3] { "ch132_skill_01_crouch", "ch132_skill_01_stand", "ch132_skill_01_jump" };
	}
}
