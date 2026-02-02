#define RELEASE
using System;
using System.Collections.Generic;
using Better;
using UnityEngine;

public class CH122_Controller : CharacterControllerProxyBaseGen4
{
	private class PetSkillObject
	{
		public PetSummoner Summoner = new PetSummoner();

		public SKILL_TABLE SkillData;
	}

	private enum SkillAnimationId : uint
	{
		ANI_SKILL0_STAND = 65u,
		ANI_SKILL0_CROUCH = 66u,
		ANI_SKILL0_JUMP = 67u,
		ANI_SKILL1_STAND = 68u,
		ANI_SKILL1_CROUCH = 69u,
		ANI_SKILL1_JUMP = 70u
	}

	private enum FxName
	{
		fxuse_frisbee_000 = 0,
		fxuse_waterelf_000 = 1
	}

	public float TIME_SKILL_0_DELAY = 0.2f;

	public float TIME_SKILL_0_CANCEL = 0.1f;

	public Vector2 SKILL_0_FOLLOW_OFFSET = new Vector2(0.6f, 1f);

	public float TIME_SKILL_1_DELAY = 0.2f;

	public float TIME_SKILL_1_CANCEL = 0.1f;

	private Vector3 _skill1ShootDirection;

	private SkinnedMeshRenderer _elfMesh;

	private PET_TABLE _petElfAttrData;

	private PetSkillObject _petElf = new PetSkillObject();

	private List<SCH029Controller> _petElfList = new List<SCH029Controller>();

	private void InitPetData()
	{
		_petElf.SkillData = _refEntity.PlayerSkills[0].FastBulletDatas[0];
		SKILL_TABLE skillData = _petElf.SkillData;
		PetSummoner summoner = _petElf.Summoner;
		ManagedSingleton<CharacterControlHelper>.Instance.PetInit<SCH029Controller>(summoner, _refEntity, 0, skillData);
		PET_TABLE value;
		if (ManagedSingleton<OrangeDataManager>.Instance.PET_TABLE_DICT.TryGetValue(summoner.PetID, out value))
		{
			_petElfAttrData = value;
		}
	}

	private void CallPet(PetSkillObject petSkillObject, int netID, PET_TABLE petAttrData, WeaponStruct skillData, Vector3 pos)
	{
		RemoveAllPets();
		PetSummoner summoner = petSkillObject.Summoner;
		SKILL_TABLE skillDatum = petSkillObject.SkillData;
		SCH029Controller sCH029Controller = ManagedSingleton<CharacterControlHelper>.Instance.CallPet<SCH029Controller>(summoner, _refEntity, summoner.PetID, netID, true, true, false, null, null, pos);
		if (sCH029Controller == null)
		{
			Debug.LogError(string.Format("petController in null, PetID = {0}", summoner.PetID));
			return;
		}
		sCH029Controller.transform.SetParentNull();
		sCH029Controller.activeSE = new string[2] { "SkillSE_CIEL3", "cl3_elf02_lp" };
		sCH029Controller.unactiveSE = new string[2] { "SkillSE_CIEL3", "cl3_elf02_stop" };
		sCH029Controller.SetSkillLevel(skillData.SkillLV);
		sCH029Controller.SetParams(petAttrData.s_MODEL, summoner.PetTime, petAttrData.n_SKILL_0, skillData.weaponStatus, 0L);
		sCH029Controller.SetActive(true);
		sCH029Controller.SetPositionAndRotation(pos, false);
		sCH029Controller.SetFollowOffset(SKILL_0_FOLLOW_OFFSET);
		_petElfList.Add(sCH029Controller);
	}

	private void RemoveAllPets()
	{
		_petElfList.ForEach(delegate(SCH029Controller pet)
		{
			pet.SetActive(false);
		});
		RemoveDeactivePets();
	}

	private void RemoveDeactivePets()
	{
		_petElfList.RemoveAll((SCH029Controller controller) => controller == null || !controller.Activate);
	}

	private void ActionStatusChanged_0_0()
	{
		PlayVoiceSE("v_cl3_skill01");
		ResetSpeed();
		SetIgnoreGravity();
		WeaponStruct weaponStruct = _refEntity.PlayerSkills[base.CurActiveSkill];
		OrangeBattleUtility.UpdateSkillCD(weaponStruct);
		_refEntity.CheckUsePassiveSkill(base.CurActiveSkill, weaponStruct.weaponStatus, _refEntity.ExtraTransforms[0]);
		if (_refEntity.IsInGround)
		{
			if (_refEntity.IsCrouching)
			{
				_refEntity.SetAnimateId((HumanBase.AnimateId)66u);
			}
			else
			{
				_refEntity.SetAnimateId(HumanBase.AnimateId.ANI_SKILL_START);
			}
		}
		else
		{
			_refEntity.SetAnimateId((HumanBase.AnimateId)67u);
		}
		SetSkillFrame(TIME_SKILL_0_DELAY);
	}

	private void ActionStatusChanged_0_1()
	{
		WeaponStruct skillData = _refEntity.PlayerSkills[base.CurActiveSkill];
		PlaySkillSE("cl3_elf01");
		if (_refEntity.IsLocalPlayer)
		{
			CallPet(_petElf, -1, _petElfAttrData, skillData, _refEntity.ModelTransform.position.xy() + SKILL_0_FOLLOW_OFFSET);
		}
		MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FxName.fxuse_waterelf_000.ToString(), _refEntity.AimPosition, Quaternion.identity, Array.Empty<object>());
		SetSkillCancelFrame(TIME_SKILL_0_CANCEL);
	}

	private void ActionStatusChanged_1_0()
	{
		PlayVoiceSE("v_cl3_skill02");
		ResetSpeed();
		SetIgnoreGravity();
		_skill1ShootDirection = _refEntity.ShootDirection;
		WeaponStruct weaponStruct = _refEntity.PlayerSkills[base.CurActiveSkill];
		OrangeBattleUtility.UpdateSkillCD(weaponStruct);
		_refEntity.CheckUsePassiveSkill(base.CurActiveSkill, weaponStruct.weaponStatus, _refEntity.ExtraTransforms[0]);
		if (_refEntity.IsInGround)
		{
			if (_refEntity.IsCrouching)
			{
				_refEntity.SetAnimateId((HumanBase.AnimateId)69u);
			}
			else
			{
				_refEntity.SetAnimateId((HumanBase.AnimateId)68u);
			}
		}
		else
		{
			_refEntity.SetAnimateId((HumanBase.AnimateId)70u);
		}
		PlaySkillSE("cl3_bmr01");
		MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FxName.fxuse_frisbee_000.ToString(), _refEntity.ExtraTransforms[0], Quaternion.identity, Array.Empty<object>());
		SetSkillFrame(TIME_SKILL_1_DELAY);
	}

	private void ActionStatusChanged_1_1()
	{
		WeaponStruct weaponStruct = _refEntity.PlayerSkills[base.CurActiveSkill];
		_refEntity.IsShoot = 3;
		_refEntity.StartShootTimer();
		_refEntity.PushBulletDetail(weaponStruct.BulletData, weaponStruct.weaponStatus, _refEntity.ExtraTransforms[0], weaponStruct.SkillLV, _skill1ShootDirection);
		SetSkillCancelFrame(TIME_SKILL_1_CANCEL);
	}

	public override void Awake()
	{
		base.Awake();
		_refEntity.ExtraTransforms = new Transform[2]
		{
			OrangeBattleUtility.FindChildRecursive(base.ChildTransforms, "L WeaponPoint", true),
			OrangeBattleUtility.FindChildRecursive(base.ChildTransforms, "R WeaponPoint", true)
		};
		_elfMesh = OrangeBattleUtility.FindChildRecursive(base.ChildTransforms, "ElfMesh_c", true).GetComponent<SkinnedMeshRenderer>();
	}

	public override void Start()
	{
		base.Start();
		_refEntity.AnimatorModelShiftYOverride = new Better.Dictionary<OrangeCharacter.MainStatus, float>();
		Enum.GetNames(typeof(FxName)).ForEach(delegate(string fxName)
		{
			MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(fxName, 2);
		});
		InitPetData();
		InitializeSkillDependDelegators(new System.Collections.Generic.Dictionary<OrangeCharacter.SubStatus, SkillStateDelegateData>
		{
			{
				OrangeCharacter.SubStatus.SKILL0,
				new SkillStateDelegateData
				{
					OnStatusChanged = ActionStatusChanged_0_0,
					OnLogicUpdate = base.ActionCheckNextSkillStatus
				}
			},
			{
				OrangeCharacter.SubStatus.SKILL0_1,
				new SkillStateDelegateData
				{
					OnStatusChanged = ActionStatusChanged_0_1,
					OnAnimationEnd = base.ActionSetSkillEnd,
					OnLogicUpdate = base.ActionCheckSkillCancel
				}
			},
			{
				OrangeCharacter.SubStatus.SKILL1,
				new SkillStateDelegateData
				{
					OnStatusChanged = ActionStatusChanged_1_0,
					OnLogicUpdate = base.ActionCheckNextSkillStatus
				}
			},
			{
				OrangeCharacter.SubStatus.SKILL1_1,
				new SkillStateDelegateData
				{
					OnStatusChanged = ActionStatusChanged_1_1,
					OnAnimationEnd = base.ActionSetSkillEnd,
					OnLogicUpdate = base.ActionCheckSkillCancel
				}
			}
		});
	}

	public override string[] GetCharacterDependAnimations()
	{
		return new string[6] { "ch122_skill_02_stand", "ch122_skill_02_crouch", "ch122_skill_02_jump", "ch122_skill_01_stand", "ch122_skill_01_crouch", "ch122_skill_01_jump" };
	}

	public override void GetUniqueMotion(out string[] source, out string[] target)
	{
		source = new string[3] { "login", "logout", "win" };
		target = new string[3] { "ch122_login", "ch122_logout", "ch122_win" };
	}

	protected override void TeleportInCharacterDepend()
	{
		HumanBase.AnimateId animateID = _refEntity.AnimateID;
		if (animateID == HumanBase.AnimateId.ANI_TELEPORT_IN_POSE)
		{
			float currentFrame = _refEntity.CurrentFrame;
		}
	}

	protected override void TeleportOutCharacterDepend()
	{
		if (_refEntity.CurSubStatus == OrangeCharacter.SubStatus.TELEPORT_POSE)
		{
			float currentFrame = _refEntity.CurrentFrame;
		}
	}

	protected override void StageTeleportInCharacterDepend()
	{
	}

	protected override void StageTeleportOutCharacterDepend()
	{
	}

	public override void ControlCharacterContinue()
	{
	}

	public override void ControlCharacterDead()
	{
		RemoveAllPets();
	}

	protected override void ToggleExtraTransforms(bool isActive)
	{
	}

	protected override void OnPlayTeleportOutEffect()
	{
		Vector3 p_worldPos = ((_refEntity != null) ? _refEntity.AimPosition : base.transform.position);
		MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("FX_TELEPORT_OUT", p_worldPos, Quaternion.identity, Array.Empty<object>());
	}

	protected override void ToggleWeapon(WeaponState weaponState)
	{
		switch (weaponState)
		{
		case WeaponState.TELEPORT_IN:
			_elfMesh.enabled = true;
			ToggleNormalWeapon(false);
			_refEntity.EnableHandMesh(true);
			break;
		case WeaponState.TELEPORT_OUT:
		case WeaponState.SKILL_0:
		case WeaponState.SKILL_1:
			ToggleNormalWeapon(false);
			_refEntity.EnableHandMesh(true);
			break;
		default:
			ToggleNormalWeapon(true);
			_elfMesh.enabled = false;
			break;
		}
	}

	protected override void AttachSkillDelegateEvent()
	{
		OnPlayerPressSkill0Events[0] = OnPlayerPressSkill0;
		OnPlayerReleaseSkill1Events[0] = OnPlayerReleaseSkill1;
	}

	protected override void OnPlayerPressSkill0(SkillID skillID)
	{
		WeaponStruct weaponStruct = _refEntity.PlayerSkills[(int)skillID];
		base.OnPlayerPressSkill0(skillID);
	}

	protected override void OnPlayerReleaseSkill1(SkillID skillID)
	{
		WeaponStruct weaponStruct = _refEntity.PlayerSkills[(int)skillID];
		base.OnPlayerReleaseSkill1(skillID);
	}

	public override void CallPet(int nPetID, bool isHurt, int nSetNumID, Vector3? vSetPos)
	{
		if (nPetID == _petElf.Summoner.PetID)
		{
			CallPet(_petElf, nSetNumID, _petElfAttrData, _refEntity.PlayerSkills[0], vSetPos.Value);
		}
	}

	protected override bool OnCheckPetActive(int petId)
	{
		RemoveDeactivePets();
		for (int num = _petElfList.Count - 1; num >= 0; num--)
		{
			if (_petElfList[num].PetID == petId)
			{
				return true;
			}
		}
		return false;
	}
}
