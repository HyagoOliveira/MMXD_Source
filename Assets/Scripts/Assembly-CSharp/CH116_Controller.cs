#define RELEASE
using System;
using System.Collections.Generic;
using System.Linq;
using Better;
using UnityEngine;

public class CH116_Controller : CharacterControllerProxyBaseGen4
{
	private class PetSkillObject
	{
		public PetSummoner Summoner = new PetSummoner();

		public SKILL_TABLE SkillData;

		public float OffsetY;
	}

	private enum SkillAnimationId : uint
	{
		ANI_SKILL1_START = 65u,
		ANI_SKILL1_LOOP = 66u,
		ANI_SKILL1_STAND_END = 67u
	}

	private enum FxName
	{
		fxuse_EasterEgg_000 = 0,
		fxuse_EasterEgg_001 = 1,
		fxuse_ch116_skill1_000 = 2
	}

	public float TIME_SKILL_0_LOOP = 0.25f;

	public float TIME_SKILL_0_CANCEL = 0.05f;

	public float JUMP_SPEED_X_SKILL_0 = 0.15f;

	public float JUMP_SPEED_Y_SKILL_0 = 0.15f;

	public float MOVE_SPEED_X_SKILL_0 = 4f;

	public float OFFSET_Y_EGGGIFTTRAP = 0.25f;

	public float OFFSET_Y_EGGTRAP_S = 0.25f;

	public float OFFSET_Y_EGGTRAP_L = 0.45f;

	public float FX_SKILL_0_SHIFT_Y = 0.72f;

	public float FX_EGGTRAP_L_SCALE = 2f;

	private PetSkillObject _petSummonerEggGift = new PetSkillObject();

	private PetSkillObject _petSummonerEggGiftTrap = new PetSkillObject();

	private PetSkillObject _petSummonerEggTrapS = new PetSkillObject();

	private PetSkillObject _petSummonerEggTrapL = new PetSkillObject();

	private List<int> _petEggGiftMeshList = new List<int> { 0, 1, 2, 3, 4, 5, 6, 7 };

	private PET_TABLE _petEggGiftAttrData;

	private int _petEggGiftShootInterval = 100;

	private int _petEggGiftCount = 1;

	private OrangeTimer _petEggGiftShootTimer = OrangeTimerManager.GetTimer();

	private List<SCH027_EggGift_Controller> _petEggGiftList = new List<SCH027_EggGift_Controller>();

	private List<SCH027_EggTrap_Controller> _petEggTrapList = new List<SCH027_EggTrap_Controller>();

	private System.Collections.Generic.Dictionary<int, PetSkillObject> _petEggTrapRandMapping = new System.Collections.Generic.Dictionary<int, PetSkillObject>();

	private int _petEggTrapRandUpbound;

	private void InitPetData()
	{
		_petSummonerEggGift.SkillData = _refEntity.PlayerSkills[0].FastBulletDatas[1];
		SKILL_TABLE skillData = _petSummonerEggGift.SkillData;
		PetSummoner summoner = _petSummonerEggGift.Summoner;
		ManagedSingleton<CharacterControlHelper>.Instance.PetInit<SCH027_EggGift_Controller>(summoner, _refEntity, 0, skillData);
		PET_TABLE value;
		if (ManagedSingleton<OrangeDataManager>.Instance.PET_TABLE_DICT.TryGetValue(summoner.PetID, out value))
		{
			_petEggGiftAttrData = value;
		}
		string[] array = skillData.s_CONTI.Split(',');
		float result;
		if (float.TryParse(array[0], out result))
		{
			_petEggGiftShootInterval = Mathf.RoundToInt(result * 1000f);
		}
		int result2;
		if (int.TryParse(array[1], out result2))
		{
			_petEggGiftCount = result2;
		}
		_petSummonerEggGiftTrap.SkillData = _refEntity.PlayerSkills[0].FastBulletDatas[2];
		_petSummonerEggGiftTrap.OffsetY = OFFSET_Y_EGGGIFTTRAP;
		skillData = _petSummonerEggGiftTrap.SkillData;
		summoner = _petSummonerEggGiftTrap.Summoner;
		ManagedSingleton<CharacterControlHelper>.Instance.PetInit<SCH027_EggTrap_Controller>(summoner, _refEntity, 0, skillData);
		_petSummonerEggTrapS.SkillData = _refEntity.PlayerSkills[1].FastBulletDatas[0];
		_petSummonerEggTrapS.OffsetY = OFFSET_Y_EGGTRAP_S;
		skillData = _petSummonerEggTrapS.SkillData;
		summoner = _petSummonerEggTrapS.Summoner;
		ManagedSingleton<CharacterControlHelper>.Instance.PetInit<SCH027_EggTrap_Controller>(summoner, _refEntity, 1, skillData);
		_petEggTrapRandMapping.Add(skillData.n_TRIGGER_RATE, _petSummonerEggTrapS);
		_petSummonerEggTrapL.SkillData = _refEntity.PlayerSkills[1].FastBulletDatas[1];
		_petSummonerEggTrapL.OffsetY = OFFSET_Y_EGGTRAP_L;
		skillData = _petSummonerEggTrapL.SkillData;
		summoner = _petSummonerEggTrapL.Summoner;
		ManagedSingleton<CharacterControlHelper>.Instance.PetInit<SCH027_EggTrap_Controller>(summoner, _refEntity, 1, skillData);
		_petEggTrapRandMapping.Add(skillData.n_TRIGGER_RATE, _petSummonerEggTrapL);
		_petEggTrapRandUpbound = _petEggTrapRandMapping.Keys.Max();
	}

	private bool CanShootEggGift()
	{
		if (!_refEntity.IsLocalPlayer)
		{
			return true;
		}
		if (_petEggGiftShootTimer.GetMillisecond() > _petEggGiftShootInterval)
		{
			_petEggGiftShootTimer.TimerStart();
			return true;
		}
		return false;
	}

	private void CreateEggGift(PetSkillObject petSkillObject, PET_TABLE petAttrData, WeaponStruct skillData)
	{
		if (_refEntity.IsLocalPlayer)
		{
			List<int> list = _petEggGiftMeshList.ToList();
			for (int i = 0; i < _petEggGiftCount; i++)
			{
				int index = OrangeBattleUtility.Random(0, list.Count);
				int meshIndex = list[index];
				CallPetOfEggGift(petSkillObject, petAttrData, skillData, i, meshIndex);
				list.RemoveAt(index);
			}
			_petEggGiftShootTimer.TimerStart();
		}
	}

	private void CallPetOfEggGift(PetSkillObject petSkillObject, PET_TABLE petAttrData, WeaponStruct skillData, int index, int meshIndex)
	{
		float y = 360f / (float)_petEggGiftCount * (float)index;
		Vector3 pos = _refEntity.AimPosition + Quaternion.Euler(0f, y, 0f) * Vector3.right;
		CallPetOfEggGift(petSkillObject, -1, petAttrData, skillData, pos, meshIndex);
	}

	private void CallPetOfEggGift(PetSkillObject petSkillObject, int netID, PET_TABLE petAttrData, WeaponStruct skillData, Vector3 pos, int meshIndex = 0)
	{
		PetSummoner summoner = petSkillObject.Summoner;
		SKILL_TABLE skillDatum = petSkillObject.SkillData;
		SCH027_EggGift_Controller sCH027_EggGift_Controller = ManagedSingleton<CharacterControlHelper>.Instance.CallPet<SCH027_EggGift_Controller>(summoner, _refEntity, summoner.PetID, netID, true, true, false, null, null, pos);
		if (sCH027_EggGift_Controller == null)
		{
			Debug.LogError(string.Format("petController in null, PetID = {0}", summoner.PetID));
			return;
		}
		sCH027_EggGift_Controller.transform.SetParentNull();
		sCH027_EggGift_Controller.activeSE = new string[2] { "SkillSE_ROLL", "rl_eggdash02_lp" };
		sCH027_EggGift_Controller.unactiveSE = new string[2] { "SkillSE_ROLL", "rl_eggdash02_stop" };
		sCH027_EggGift_Controller.SetSkillLv(skillData.SkillLV);
		sCH027_EggGift_Controller.SetParams(petAttrData.s_MODEL, summoner.PetTime, petAttrData.n_SKILL_0, skillData.weaponStatus, 0L);
		sCH027_EggGift_Controller.SetActive(true);
		sCH027_EggGift_Controller.SetPositionAndRotation(pos, false);
		sCH027_EggGift_Controller.SetFollowAngle(summoner.PetCount % _petEggGiftCount * 360 / _petEggGiftCount);
		if (_refEntity.IsLocalPlayer)
		{
			sCH027_EggGift_Controller.SetMeshIndex(meshIndex);
		}
		sCH027_EggGift_Controller._cbCanShoot = CanShootEggGift;
		_petEggGiftList.Add(sCH027_EggGift_Controller);
	}

	private void RandEggTrap(WeaponStruct skillData)
	{
		if (!_refEntity.IsLocalPlayer)
		{
			return;
		}
		int num = OrangeBattleUtility.Random(0, _petEggTrapRandUpbound);
		System.Collections.Generic.Dictionary<int, PetSkillObject>.Enumerator enumerator = _petEggTrapRandMapping.GetEnumerator();
		while (enumerator.MoveNext())
		{
			if (num < enumerator.Current.Key)
			{
				CallPetOfEggTrap(enumerator.Current.Value, skillData);
				break;
			}
		}
		enumerator.Dispose();
	}

	private void CallPetOfEggTrap(PetSkillObject petSkillObject, WeaponStruct skillData)
	{
		if (_refEntity.IsLocalPlayer)
		{
			Vector3 pos = _refEntity.ModelTransform.position + new Vector3(0f, petSkillObject.OffsetY, 0f);
			CallPetOfEggTrap(petSkillObject, -1, skillData, pos);
		}
	}

	private void CallPetOfEggTrap(PetSkillObject petSkillObject, int netID, WeaponStruct skillData, Vector3 pos)
	{
		PetSummoner summoner = petSkillObject.Summoner;
		SKILL_TABLE skillData2 = petSkillObject.SkillData;
		SCH027_EggTrap_Controller sCH027_EggTrap_Controller = ManagedSingleton<CharacterControlHelper>.Instance.CallPet<SCH027_EggTrap_Controller>(summoner, _refEntity, summoner.PetID, netID, true, false, false, null, null, pos);
		if (sCH027_EggTrap_Controller == null)
		{
			Debug.LogError(string.Format("petController in null, PetID = {0}", summoner.PetID));
			return;
		}
		sCH027_EggTrap_Controller.transform.SetParentNull();
		sCH027_EggTrap_Controller.sActiveSE2 = new string[2] { "", "" };
		if (skillData2.s_USE_SE != "null")
		{
			sCH027_EggTrap_Controller.activeSE = skillData2.s_USE_SE.Split(',');
		}
		sCH027_EggTrap_Controller.unactiveSE = null;
		sCH027_EggTrap_Controller.sExplodeSE = new string[2] { "SkillSE_ROLL", "rl_setegg03" };
		sCH027_EggTrap_Controller.SetSkillLv(skillData.SkillLV);
		sCH027_EggTrap_Controller.UpdateAimRange(_refEntity);
		sCH027_EggTrap_Controller.SetActive(true);
		sCH027_EggTrap_Controller.SetPositionAndRotation(pos, false);
		_petEggTrapList.Add(sCH027_EggTrap_Controller);
		if (_refEntity.IsLocalPlayer)
		{
			FxBase fxBase = MonoBehaviourSingleton<FxManager>.Instance.PlayReturn<FxBase>(FxName.fxuse_EasterEgg_001.ToString(), pos - new Vector3(0f, petSkillObject.OffsetY, 0f), Quaternion.identity, Array.Empty<object>());
			if (summoner.PetID == _petSummonerEggTrapL.Summoner.PetID)
			{
				fxBase.transform.localScale = Vector3.one * FX_EGGTRAP_L_SCALE;
			}
		}
	}

	private void RemoveDeactivePets()
	{
		_petEggGiftList.RemoveAll((SCH027_EggGift_Controller controller) => controller == null || !controller.Activate);
		_petEggTrapList.RemoveAll((SCH027_EggTrap_Controller controller) => controller == null || !controller.Activate);
	}

	public override void RemovePet()
	{
		for (int num = _petEggTrapList.Count - 1; num >= 0; num--)
		{
			if (_petEggTrapList[num] == null || !_petEggTrapList[num].Activate)
			{
				_petEggTrapList.RemoveAt(num);
			}
			else
			{
				_petEggTrapList[num].BackToPool();
			}
		}
		_petEggTrapList.Clear();
	}

	private void ActionStatusChanged_0_0()
	{
		ResetSpeed();
		SetIgnoreGravity();
		WeaponStruct weaponStruct = _refEntity.PlayerSkills[base.CurActiveSkill];
		PlayVoiceSE("v_rl_skill04");
		OrangeBattleUtility.UpdateSkillCD(weaponStruct);
		_refEntity.CheckUsePassiveSkill(base.CurActiveSkill, weaponStruct.weaponStatus, _refEntity.ExtraTransforms[0]);
		_refEntity.SetAnimateId(HumanBase.AnimateId.ANI_SKILL_START);
	}

	private void ActionStatusChanged_0_1()
	{
		SetSpeed((float)OrangeCharacter.DashSpeed * MOVE_SPEED_X_SKILL_0 * (float)_refEntity.direction, 0f);
		WeaponStruct weaponStruct = _refEntity.PlayerSkills[base.CurActiveSkill];
		_refEntity.BulletCollider.UpdateBulletData(weaponStruct.BulletData, _refEntity.sPlayerName, _refEntity.GetNowRecordNO(), _refEntity.nBulletRecordID++, _refEntity.direction);
		_refEntity.BulletCollider.SetBulletAtk(weaponStruct.weaponStatus, _refEntity.selfBuffManager.sBuffStatus);
		_refEntity.BulletCollider.BulletLevel = weaponStruct.SkillLV;
		_refEntity.BulletCollider.Active(_refEntity.TargetMask);
		CreateEggGift(_petSummonerEggGift, _petEggGiftAttrData, weaponStruct);
		MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FxName.fxuse_EasterEgg_000.ToString(), _refEntity.ModelTransform.position + new Vector3(0f, FX_SKILL_0_SHIFT_Y), Quaternion.identity, Array.Empty<object>());
		_refEntity.SetAnimateId(_refEntity.AnimateID + 1);
		SetSkillFrame(TIME_SKILL_0_LOOP);
	}

	private void ActionStatusChanged_0_2()
	{
		ResetSpeed();
		_refEntity.BulletCollider.BackToPool();
		WeaponStruct skillData = _refEntity.PlayerSkills[base.CurActiveSkill];
		CallPetOfEggTrap(_petSummonerEggGiftTrap, skillData);
		_refEntity.SetAnimateId(_refEntity.AnimateID + 1);
		SetSkillCancelFrame(TIME_SKILL_0_CANCEL);
	}

	public override void Awake()
	{
		base.Awake();
		_refEntity.ExtraTransforms = new Transform[2]
		{
			OrangeBattleUtility.FindChildRecursive(base.ChildTransforms, "L WeaponPoint", true),
			OrangeBattleUtility.FindChildRecursive(base.ChildTransforms, "R WeaponPoint", true)
		};
	}

	public override void Start()
	{
		base.Start();
		_refEntity.AnimatorModelShiftYOverride = new Better.Dictionary<OrangeCharacter.MainStatus, float>
		{
			{
				OrangeCharacter.MainStatus.TELEPORT_IN,
				-0.14f
			},
			{
				OrangeCharacter.MainStatus.TELEPORT_OUT,
				-0.14f
			},
			{
				OrangeCharacter.MainStatus.SKILL,
				-0.14f
			},
			{
				OrangeCharacter.MainStatus.GIGA_ATTACK,
				-0.14f
			}
		};
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
					OnAnimationEnd = base.ActionSetNextSkillStatus
				}
			},
			{
				OrangeCharacter.SubStatus.SKILL0_1,
				new SkillStateDelegateData
				{
					OnStatusChanged = ActionStatusChanged_0_1,
					OnLogicUpdate = base.ActionCheckNextSkillStatus
				}
			},
			{
				OrangeCharacter.SubStatus.SKILL0_2,
				new SkillStateDelegateData
				{
					OnStatusChanged = ActionStatusChanged_0_2,
					OnAnimationEnd = base.ActionSetSkillEnd,
					OnLogicUpdate = base.ActionCheckSkillCancel
				}
			}
		});
	}

	public override string[] GetCharacterDependAnimations()
	{
		return new string[3] { "ch116_skill_01_start", "ch116_skill_01_loop", "ch116_skill_01_stand_end" };
	}

	public override void GetUniqueMotion(out string[] source, out string[] target)
	{
		source = new string[3] { "login", "logout", "win" };
		target = new string[3] { "ch116_login", "ch116_logout", "ch116_win" };
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

	protected override void ToggleExtraTransforms(bool isActive)
	{
	}

	protected override bool OnEnterRideArmor(RideBaseObj targetRideArmor)
	{
		return base.OnEnterRideArmor(targetRideArmor);
	}

	protected override void ToggleWeapon(WeaponState weaponState)
	{
		if ((uint)weaponState <= 1u || (uint)(weaponState - 3) <= 1u)
		{
			ToggleNormalWeapon(false);
			_refEntity.EnableHandMesh(true);
		}
		else
		{
			ToggleNormalWeapon(true);
		}
	}

	protected override void OnPlayerPressSkill0(SkillID skillID)
	{
		MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FxName.fxuse_ch116_skill1_000.ToString(), new Vector3(base.transform.position.x, base.transform.position.y, (_refEntity._characterDirection == CharacterDirection.RIGHT) ? 0f : 1f), (_refEntity._characterDirection == CharacterDirection.RIGHT) ? OrangeCharacter.NormalQuaternion : OrangeCharacter.ReversedQuaternion, Array.Empty<object>());
		base.OnPlayerPressSkill0(skillID);
	}

	protected override void OnPlayerPressSkill1(SkillID skillID)
	{
		if (_refEntity.IsLocalPlayer)
		{
			PlayVoiceSE("v_rl_skill05");
		}
		WeaponStruct weaponStruct = _refEntity.PlayerSkills[(int)skillID];
		OrangeBattleUtility.UpdateSkillCD(weaponStruct);
		_refEntity.CheckUsePassiveSkill((int)skillID, weaponStruct.weaponStatus, weaponStruct.ShootTransform[0]);
		RandEggTrap(weaponStruct);
	}

	public override void CallPet(int nPetID, bool isHurt, int nSetNumID, Vector3? vSetPos)
	{
		if (nPetID == _petSummonerEggGift.Summoner.PetID)
		{
			CallPetOfEggGift(_petSummonerEggGift, nSetNumID, _petEggGiftAttrData, _refEntity.PlayerSkills[0], vSetPos.Value);
		}
		if (nPetID == _petSummonerEggGiftTrap.Summoner.PetID)
		{
			CallPetOfEggTrap(_petSummonerEggGiftTrap, nSetNumID, _refEntity.PlayerSkills[0], vSetPos.Value);
		}
		if (nPetID == _petSummonerEggTrapS.Summoner.PetID)
		{
			CallPetOfEggTrap(_petSummonerEggTrapS, nSetNumID, _refEntity.PlayerSkills[1], vSetPos.Value);
		}
		if (nPetID == _petSummonerEggTrapL.Summoner.PetID)
		{
			CallPetOfEggTrap(_petSummonerEggTrapL, nSetNumID, _refEntity.PlayerSkills[1], vSetPos.Value);
		}
	}

	protected override bool OnCheckPetActive(int petId)
	{
		RemoveDeactivePets();
		for (int num = _petEggGiftList.Count - 1; num >= 0; num--)
		{
			if (_petEggGiftList[num].PetID == petId)
			{
				return true;
			}
		}
		for (int num2 = _petEggTrapList.Count - 1; num2 >= 0; num2--)
		{
			if (_petEggTrapList[num2].PetID == petId)
			{
				return true;
			}
		}
		return false;
	}
}
